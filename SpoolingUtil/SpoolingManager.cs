using System;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Collections.ObjectModel;

namespace SpoolingUtil
{
    using SpoolingUtil.TransferMethod;
    using LogModule;

    /// <summary>
    /// Spooling 기능에 대한 Manager 
    /// </summary>
    public class SpoolingManager : IDisposable
    {
     
        public enum TransferMethodType
        {
            UsingFTP,
            UsingNetwork_LocalDisk,
            UsingNetwork_NetDrv,
            UsingUnknownType,
        }

        #region ==> variable
        private readonly string ftpPattern = @"^ftp://|FTP:// ";
        private readonly string networkPattern = @"^\\(\\(\w+\s*)+)";
        private readonly string localPattern = @"^[A-Z]:(\\(\w+\s*)+)";

        public string uploadBasePath { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public bool FTPUserPassiveMode { get; set; }

        public string spoolingList_MngPath { get; set; }
        public int spoolingRetryCount { get; set; } = 0;
        public int spoolingItemProcDelayTime { get; set; } = 1000;
        public int spoolingLoopDelayTime { get; set; } = 5000;

        public int spoolingPauseDelay { get; set; } = 3000;
        public bool spoolingThreadPauseState { get; set; } = false; ///< Spooling thread 가 대기 상태인지 여부 flag

        private bool spoolingThreadStopFlag { get; set; } = false;
        private bool refreshOptionFlag = false;
        public bool StopSleepFlag = false;
        public bool UseForceNextItem { get; set; } = false;  ///< 연속으로 upload 실패 시 실패로 끝내것인지 다른 Item으로 넘어가 진행할것인지(default는 false이다.)
        private readonly string engineerModeKeyword = "semics:";
        public ITransferMethods ITransferMethod { get; set; }

        private Dictionary<TransferMethodType, ITransferMethods> TransferMethodStorage = new Dictionary<TransferMethodType, ITransferMethods>();

        public Dictionary<TransferMethodType, ITransferMethods> GetSet_TransferMethodStorage { get; set; }

        public bool InitFlag { get; set; } = false;
        public bool ManualUploadEndFlag { get; set; } = true;
        public bool PreviousTaskDoing { get; set; } = false;
        // manual uploading 진행여부 flag로 해당 flag가 true면 자동 upload thread는 pasue 상태가 되어 한다.
        private bool _ManualUploading = false;
        public bool ManualUploading
        {
            get
            {
                return _ManualUploading;
            }

            set
            {
                _ManualUploading = value;
                if (_ManualUploading)
                    StopSleepFlag = true;
            }
        }

        public bool sendedFailedInfo { get; set; } = false; /// 최종 전송 실패에 대한 정보를 전달했는지(CallBack함수로 실패로 전달했다가 다시 성공되는 경우 실패창을 닫기 위해)
        private Thread spoolingThread;
        private object lockObject = new object();
        private System.Threading.Timer SpoolingThreadRestartTimer;
        SpoolingListParamMng spoolingListItemMng;
        #endregion

        #region ==> CallBackFunc 
        
        public delegate void UploadNotice(bool show_noticeInfo);
        public UploadNotice CallBackFunc { get; set; }

        public delegate void ManualUploadResult(string fileName, string dataTime, bool sucess, string detail);
        public ManualUploadResult ManualUploadResultCallBackFunc { get; set; }
        
        #endregion


        /// <summary>
        /// 생성자
        /// </summary>        
        /// <param name="spoolingList_MngPath"> Spooling list가 보관될 path</param>
        /// <param name="spoolingRetryCount"> Spooling으로 재 시도할 count</param>
        /// <param name="spoolingItemProcDelayTime"> Spooling list에 있는 item별 전송 간격 delay 시간</param>
        /// <param name="spoolingLoopDelayTime">Spooling 전달할 것이 있는지 polling 하는 시간</param>
        public SpoolingManager(string spoolingList_MngPath, int spoolingRetryCount, int spoolingItemProcDelayTime = 500, int spoolingLoopDelayTime = 5000)
        {
            this.spoolingList_MngPath = spoolingList_MngPath;
            this.spoolingRetryCount = spoolingRetryCount;
            this.spoolingItemProcDelayTime = spoolingItemProcDelayTime;
            this.spoolingLoopDelayTime = spoolingLoopDelayTime;
            this.StopSleepFlag = false;

            if (false == Directory.Exists(spoolingList_MngPath))
            {
                try
                {
                    Directory.CreateDirectory(spoolingList_MngPath);
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"[Spooling] Failed to create directory. Path={spoolingList_MngPath}, Err = {err.Message})");
                }
            }

            spoolingListItemMng = new SpoolingListParamMng(spoolingList_MngPath);
            SpoolingThreadRestartTimer = new Timer(new TimerCallback(spoolingThreadRestart), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        public void Dispose()
        {
            Exit_SpoolingProcess();
            if (null != SpoolingThreadRestartTimer)
                SpoolingThreadRestartTimer.Dispose();
        }

        /// <summary>
        /// Spooling 처리 Thread에 어떠한 이유로 종료되어 재 구동 처리를 하기 위한 Timer call back함수
        /// </summary>
        /// <param name="obj"></param>
        private void spoolingThreadRestart(object obj)
        {
            SpoolingThreadRestartTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            LoggerManager.Debug($"[Spooling] --- SpoolingThread Restart.---");
            SpoolingThreadStart();
        }

        /// <summary>
        /// Spooing init
        /// </summary>
        /// <param name="uploadBasePath"> upload base path</param>
        /// <param name="username"> user name </param>
        /// <param name="password"> password </param>
        /// <param name="FTPUserPassive"> ftp passive mode</param>
        public void InitSpooling(string uploadBasePath, string username, string password, bool FTPUserPassive)
        {
            if (InitFlag)
                return;

            this.uploadBasePath = uploadBasePath;
            this.username = username;
            this.password = password;
            this.FTPUserPassiveMode = FTPUserPassive;
            SpoolingThreadStart(); // thread 발행
            InitFlag = true;            
        }

        /// <summary>
        /// spooling 옵션 변경 함수
        /// </summary>
        /// <param name="uploadBasePath"> upload base path</param>
        /// <param name="username"> user name </param>
        /// <param name="password"> password </param>
        /// <param name="FTPUserPassive">ftp passive mode </param>
        /// <param name="retry_count"> spooling retry count</param>
        /// <param name="reupload_delayTm"> spooling loop delay time</param>
        public void UpdateSpoolingOption(string uploadBasePath, string username, string password, bool FTPUserPassive, int retry_count, int reupload_delayTm)
        {
            this.uploadBasePath = uploadBasePath;
            this.username = username;
            this.password = password;
            this.FTPUserPassiveMode = FTPUserPassive;
            this.spoolingRetryCount = retry_count;
            this.spoolingLoopDelayTime = reupload_delayTm * 1000;
            SetRefreshOptionFlag();
        }


        /// <summary>
        /// spooling 종료 처리 함수(일반적으로 프로그램 종료시 호출)
        /// </summary>
        public void Exit_SpoolingProcess()
        {
            ManualUploading = true;
            StopSleepFlag = true;
            spoolingThreadStopFlag = true;
        }

        /// <summary>
        /// 옵션 데이터를 refresh 하도록 Flag설정
        /// </summary>
        private void SetRefreshOptionFlag()
        {
            lock (lockObject)
            {
                refreshOptionFlag = true;
            }
        }

        /// <summary>
        /// 전송 방식 렬정
        /// </summary>
        /// <param name="serverpath"> server path </param>
        /// <param name="transferMethod"> transfer mode 반환 </param>
        /// <returns></returns>
        private bool SetTransferMethod(string serverpath, out TransferMethodType transferMethod)
        {
            if (Regex.IsMatch(serverpath, ftpPattern)) // ftp 
            {
                if (TransferMethodStorage.ContainsKey(TransferMethodType.UsingFTP))
                    ITransferMethod = TransferMethodStorage[TransferMethodType.UsingFTP];
                else
                {
                    ITransferMethod = new TransferUsingFTP(username, password, FTPUserPassiveMode, true, 5000, 5000);
                    TransferMethodStorage.Add(TransferMethodType.UsingFTP, ITransferMethod);
                }

                transferMethod = TransferMethodType.UsingFTP;
                return true;
            }
            else if (Regex.IsMatch(serverpath, localPattern)) //local path
            {
                if (TransferMethodStorage.ContainsKey(TransferMethodType.UsingNetwork_LocalDisk))
                    ITransferMethod = TransferMethodStorage[TransferMethodType.UsingNetwork_LocalDisk];
                else
                {
                    ITransferMethod = new TransferUsingLocalDisk();
                    TransferMethodStorage.Add(TransferMethodType.UsingNetwork_LocalDisk, ITransferMethod);
                }

                transferMethod = TransferMethodType.UsingNetwork_LocalDisk;
                return true;
            }
            else if (Regex.IsMatch(serverpath, networkPattern))
            {
                // to-do
                LoggerManager.Debug($"Upload path networkType(not supported). path ={serverpath}");
            }
            else
            {
                LoggerManager.Debug($"[Spooling] Upload path is wrong. path ={serverpath}");
            }

            transferMethod = TransferMethodType.UsingUnknownType;
            return false;
        }

        /// <summary>
        /// manual로 처리 시 현 호출시점까지 수집된것을 기준으로 수동 upload item이 구성되며, Manulal 처리 모드로 동작된다.
        /// Manual mode가 끝나면 ManualUploadSpooling_Fin()을 호출해줘야 한다.
        /// </summary>
        /// <param name="itemList"> 처리되지 않은 Spooling items 반환</param>
        /// <returns>true - 성공, false - 실패</returns>
        /// Spooling auto thread의 stop을 기다렸다가 처리하는 방식에서 기다리지 않도록 변경.
        /// network 또는 상이한 ip 입력으로 인해 timeout이 발생하고 있다면 계속 지연됨.그리고 자동 upload 처리함수 바로 밑에서 manual upload 체크하고 괜찮음
        public bool ManualUploadSpooling_Init(ref ObservableCollection<SpoolingListItem> itemList)
        {
            // spooling thread pause
            ManualUploading = true;
            LoggerManager.Debug($"[Spooling] {MethodBase.GetCurrentMethod().Name} - Manual Upload Init");

            // collect spooling item
            spoolingListItemMng.MoveUnprocessdToProdessd();
            itemList = spoolingListItemMng.Processing_SpoolingListParam.SpoolingListParam;
            return true;
        }

        /// <summary>
        /// Manual upload fin - Manual upload 종료 처리
        /// </summary>
        public void ManualUploadSpooling_Fin()
        {
            ManualUploading = false;
            LoggerManager.Debug($"[Spooling] {MethodBase.GetCurrentMethod().Name} - Manual Upload Fin");
        }

        /// <summary>
        /// 인자로 들어온 Item별로 Upload 처리 시작(Manual upload처리 부분)
        /// </summary>
        /// <param name="itemList"> Manual로 전송할 Item list</param>
        public void StartManualUploadProc(ref ObservableCollection<SpoolingListItem> itemList)
        {
            if (false == ManualUploading)
            {
                LoggerManager.Debug($"[Spooling] Not yet init for 'ManualUploadSpooling_Init'");
                return;
            }

            ManualUploadEndFlag = false;
            try
            {
                SpoolingListItem item;
                int SpoolingIdx = 0;
                while (SpoolingIdx < itemList.Count)
                {
                    if (false == ManualUploading)
                    {
                        LoggerManager.Debug($"[Spooling] Manual upload stop.( Call 'ManualUploadSpooling_fin() )' ");
                        break;
                    }

                    item = itemList[SpoolingIdx];
                    string uploadTargetFilePath = item.TargetItemPath;

                    if (string.IsNullOrEmpty(uploadBasePath) || CheckEngineerMode(uploadBasePath))
                    {
                        string filename = Path.GetFileName(uploadTargetFilePath);
                        ManualUploadResultCallBackFunc(filename, item.Date, false, "Upload Path is empty or EngineerMode");
                        ++SpoolingIdx;
                        Thread.Sleep(200);
                        continue;
                    }

                    /// 무결성 검증
                    string key_value = spoolingListItemMng.GetSpoolingKeyInfo(uploadTargetFilePath);
                    if (key_value == item.Key)
                    {
                        TransferMethodType transferMethod;
                        if (SetTransferMethod(uploadBasePath, out transferMethod))
                        {

                            TransferUsingFTP transferFTP = null;
                            string separator = "/";
                            if (transferMethod != TransferMethodType.UsingFTP)
                                separator = "\\";
                            else
                            {
                                // update ftp info
                                transferFTP = ITransferMethod as TransferUsingFTP;
                                if (null != transferFTP)
                                    transferFTP.FtpFunctionObj.UseBinary = item.UseBinary;
                            }

                            bool success = false;
                            string destFullPath;
                            string TempPath = uploadBasePath;
                            string check_separator = "";
                            if (TempPath.Length > 0)
                                check_separator = TempPath.Substring(TempPath.Length - 1, 1);

                            if (check_separator != separator)
                                destFullPath = uploadBasePath + separator + item.UploadSubPath;
                            else
                                destFullPath = uploadBasePath + item.UploadSubPath;

                            success = ITransferMethod.UploadFileProc(destFullPath, uploadTargetFilePath, username, password);
                            if(PreviousTaskDoing) //이전 작업중이던 상태에서 창을 닫고 Hide 되었다가 다시 show된 Case로 해당 함수 qk return되어야 함
                            {
                                LoggerManager.Debug($"[Spooling] previouse task so return");
                                PreviousTaskDoing = false;
                                return;
                            }

                            if (null != ManualUploadResultCallBackFunc)
                            {
                                string result_detail = "Success";
                                if (transferMethod == TransferMethodType.UsingFTP)
                                {
                                    transferFTP = ITransferMethod as TransferUsingFTP;
                                    if (null != transferFTP && false == success)
                                    {
                                        result_detail = transferFTP.FtpFunctionObj.last_err_info;
                                    }
                                }

                                string filename = Path.GetFileName(destFullPath);
                                ManualUploadResultCallBackFunc(filename, item.Date, success, result_detail);
                            }

                            if (success) //전송에 성공했다면 성공한 Item은 메모리 제거 후 파일로 동기화 처리
                            {
                                LoggerManager.Debug($"[Spooling] Spooling Maunual upload sucess.(path={destFullPath}, target={uploadTargetFilePath})");
                                itemList.RemoveAt(SpoolingIdx);
                                spoolingListItemMng.SaveProcessingListParam();
                            }
                            else
                            {
                                LoggerManager.Debug($"[Spooling] Spooling Maunual upload fail.(path={destFullPath}, target={uploadTargetFilePath})");
                                ++SpoolingIdx;
                            }

                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[Spooling] file is wrong. path ={uploadTargetFilePath}, org_key={item.Key}, cur_key={key_value}");
                        itemList.RemoveAt(SpoolingIdx);
                        spoolingListItemMng.SaveProcessingListParam();
                        string filename = Path.GetFileName(uploadTargetFilePath);
                        string noticeTxt = $"This file is unverified.\r\nPath={uploadTargetFilePath}\r\nThis item will be deleted.";
                        ManualUploadResultCallBackFunc(filename, item.Date, false, noticeTxt);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                ManualUploadEndFlag = true;
            }            
        }

        /// <summary>
        /// Spooling 할 Item 추가 처리 함수
        /// </summary>
        /// <param name="targetFilePath"> 전송할 file full path</param>
        /// <param name="uploadSubPath"> 목적지 sub path(main path는 옵션에 따라 변경되므로)</param>
        /// <param name="cell_idx">해당 파일이 생성된 cell idx</param>
        /// <param name="useBinary">ftp mode인 경우 binary mode로 전송할 지 여부</param>
        public void AddToItemInSpooling(string targetFilePath, string uploadSubPath, int cell_idx, bool useBinary)
        {
            LoggerManager.Debug($"[Spooling] add spooling item. target={targetFilePath}, uploadSubPath={uploadSubPath}, cell_idx={cell_idx}");
            spoolingListItemMng.AddSpoolingItem(targetFilePath, uploadSubPath, cell_idx, useBinary);
        }

        /// <summary>
        /// 옵션 갱신 여부 확인
        /// </summary>
        /// <returns></returns>
        private bool GetRefreshOptionFlag()
        {
            bool refreshFlag = false;
            lock (lockObject)
            {
                refreshFlag = refreshOptionFlag;
                refreshOptionFlag = false;
            }

            return refreshFlag;
        }
       
        /// <summary>
        /// Sleep 함수를 쪼개 중간에 flag를 통해 탈출할 수 있도록 처리한 함수
        /// </summary>
        /// <param name="delayTime"> Sleep time</param>
        /// <param name="pieceSleepTime"> 몇 ms 단위로 sleep하면서 체크할 것인지 </param>
        /// <param name="stopFlag"> Sleep 함수 탈출 flag로 동일 인자로 여러곳에서 사용하지 말 것(사용하는곳 마다 한개씩 배정)</param>
        /// 인자로 들어온 pieceSleepTime 으로 나누어 사용하기 때문에 이를 고려하여 사용할 것
        private void StopEnalbeSleep(int delayTime, int pieceSleepTime, ref bool stopFlag)
        {            
            int nLoopCount = delayTime / pieceSleepTime;
            for (int i = 0; i < nLoopCount; i++)
            {
                Thread.Sleep(pieceSleepTime);
                if (stopFlag)
                {
                    stopFlag = false;
                    break;
                }
            }
        }

        /// <summary>
        /// 접근할 목적지 경로가 Engineer mode동작될 정보가 있는지 확인
        /// </summary>
        /// <param name="accessPath"> 목적지 path</param>
        /// <returns>true - Engineer mode, false - 일반</returns>
        private bool CheckEngineerMode(string accessPath)
        {
            if (accessPath.Length >= 7)
            {
                string checkStr = accessPath.Substring(0, 7);
                if (checkStr.ToUpper() == engineerModeKeyword.ToUpper())
                {
                    LoggerManager.Debug($"[Spooling] Path is Engineer mode");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Spooling 처리 thread 시작 함수
        /// </summary>
        private void SpoolingThreadStart()
        {
            spoolingThread = new Thread(() =>
            {
                LoggerManager.Debug($"[Spooling] Spooling thread start.");
                try
                {
                    bool firstLoop = true;
                    string UserName = this.username;
                    string Password = this.password;
                    string uploadPath = this.uploadBasePath;
                    int retryCount = this.spoolingRetryCount;
                    int loopDelayTm = this.spoolingLoopDelayTime;
                    bool LogWriteAbtouUploadPathIsEmpty = false;
                    while (false == spoolingThreadStopFlag)
                    {
                        StopSleepFlag = false;

                        //옵변 변경여부 체크
                        if (GetRefreshOptionFlag())
                        {
                            uploadPath = this.uploadBasePath;
                            retryCount = this.spoolingRetryCount;
                            UserName = this.username;
                            Password = this.password;
                            loopDelayTm = this.spoolingLoopDelayTime;
                        }

                        if (ManualUploading || 0 >= retryCount) //자동 upload 처리 하면 안되는 상태(manual upload 중이거나 retry count가 0 보다 작은 경우)
                        {
                            spoolingThreadPauseState = true;
                            StopEnalbeSleep(spoolingPauseDelay, 1000, ref StopSleepFlag);                            
                            firstLoop = false;
                            continue;
                        }
                        else  // Loop 순환 시 delay time
                        {
                            spoolingThreadPauseState = false;
                            if (firstLoop)
                                firstLoop = false;
                            else
                                StopEnalbeSleep(loopDelayTm, 1000, ref StopSleepFlag);
                        }

                        if(string.IsNullOrEmpty(uploadPath) || CheckEngineerMode(uploadPath)) // upload server Path가 없거나 Engineer mode 여부 확인
                        {
                            if(false == LogWriteAbtouUploadPathIsEmpty) // 매 중복으로 동일 로그를 기록하지 않기 위함
                            {
                                LoggerManager.Debug($"[Spooling] Upload server path is empty or EngineerMode.so can not run");
                                LogWriteAbtouUploadPathIsEmpty = true;                                
                            }
                            continue;
                        }
                        else
                        {
                            if (LogWriteAbtouUploadPathIsEmpty)
                            {
                                LoggerManager.Debug($"[Spooling] Upload server path is setted(path={uploadPath})");
                                LogWriteAbtouUploadPathIsEmpty = false;
                            }                                
                        }

                        SpoolingItemProc(retryCount, spoolingItemProcDelayTime, uploadPath, UserName, Password);
                    }

                    LoggerManager.Debug($"[Spooling] Spooling thread end.");
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                finally
                {
                    if (false == spoolingThreadStopFlag) /// 정상적인 종료가 아닌것으로 판단하여 시간을 두고 재실행 해준다.
                    {
                        LoggerManager.Debug($"[Spooling] Spooling thread abnormal termination.");
                        SpoolingThreadRestartTimer.Change(5000, System.Threading.Timeout.Infinite);
                    }
                }
            });
            spoolingThread.Start();
        }

        /// <summary>
        /// Item에 있는 항목되로 정보를 전달한다.
        /// </summary>
        /// <param name="retryCount"> 실패 시 재 시도 횟수 </param>
        /// <param name="delayPerItem"> item 전송당 delay 시간</param>
        /// <param name="uploadBasePath"> upload 할 server 경로</param>
        /// <param name="username"> username</param>
        /// <param name="password"> password</param>
        private void SpoolingItemProc(int retryCount, int delayPerItem, string uploadBasePath, string username, string password)
        {
            /// item gathering
            spoolingListItemMng.MoveUnprocessdToProdessd();
            
            SpoolingListItem item;
            int SpoolingIdx = 0;
            while (SpoolingIdx < spoolingListItemMng.Processing_SpoolingListParam.SpoolingListParam.Count)
            {
                if (ManualUploading)
                    break;

                item = spoolingListItemMng.Processing_SpoolingListParam.SpoolingListParam[SpoolingIdx];

                string uploadTargetFilePath = item.TargetItemPath;

                /// 무결성 검증
                string key_value = spoolingListItemMng.GetSpoolingKeyInfo(uploadTargetFilePath);
                if (key_value == item.Key)
                {
                    TransferMethodType transferMethod;
                    if (SetTransferMethod(uploadBasePath, out transferMethod))
                    {
                        string separator = "/";
                        if (transferMethod != TransferMethodType.UsingFTP)                        
                            separator = "\\";                                                    
                        else
                        {
                            // update ftp info
                            TransferUsingFTP transferFTP = ITransferMethod as TransferUsingFTP;
                            if (null != transferFTP)
                                transferFTP.FtpFunctionObj.UseBinary = item.UseBinary;
                        }

                        bool success = false;                        
                        string destFullPath;
                        string TempPath = uploadBasePath;
                        string check_separator = "";
                        if(TempPath.Length > 0)
                            check_separator = TempPath.Substring(TempPath.Length - 1, 1);

                        if (check_separator != separator)
                            destFullPath = uploadBasePath + separator + item.UploadSubPath;
                        else
                            destFullPath = uploadBasePath + item.UploadSubPath;

                        if (false == ITransferMethod.UploadFileProc(destFullPath, uploadTargetFilePath, username, password))
                        {
                            int nRetryCount = retryCount - 1;  // -1을 이유는 위에서 최초 한번 시도했기 때문 
                            if (nRetryCount < 0)               // 음수로 나올수는 없으나 예방차원 
                                nRetryCount = 0;

                            for (int i = 0; i < nRetryCount ; i++)
                            {
                                if (ManualUploading)
                                    break;

                                Thread.Sleep(2000);
                                if (ITransferMethod.UploadFileProc(destFullPath, uploadTargetFilePath, username, password))
                                {
                                    success = true;
                                    break;
                                }

                                if (ManualUploading)
                                    break;
                            }

                            /// call back 함수 호출(최종 실패에 대한 전달을 위함)
                            if (false == success && false == ManualUploading)
                            {
                                if (null != CallBackFunc)
                                    CallBackFunc(true);

                                sendedFailedInfo = true;
                                LoggerManager.Debug($"[Spooling] Spooling upload fail.(src={uploadTargetFilePath}, path={destFullPath})");

                                if(false == UseForceNextItem)
                                    return;
                                else
                                {
                                    LoggerManager.Debug($"[Spooling] force move the next item.");
                                }
                            }
                        }
                        else
                            success = true;

                        if (success) //전송에 성공했다면 성공한 Item은 제거 후 파일로 저장
                        {
                            LoggerManager.Debug($"[Spooling] Spooling upload sucess.(src={uploadTargetFilePath},path={destFullPath})");
                            spoolingListItemMng.Processing_SpoolingListParam.SpoolingListParam.RemoveAt(SpoolingIdx);
                            spoolingListItemMng.SaveProcessingListParam();
                            if(sendedFailedInfo)
                            {
                                CallBackFunc(false);
                                sendedFailedInfo = false;
                            }
                        }
                    }
                }
                else    ///< 무결성 검증에 상이한것은 item 삭제
                {
                    LoggerManager.Debug($"[Spooling] file is wrong. path ={uploadTargetFilePath}, org_key={item.Key}, cur_key={key_value}");
                    spoolingListItemMng.Processing_SpoolingListParam.SpoolingListParam.RemoveAt(SpoolingIdx);
                    spoolingListItemMng.SaveProcessingListParam();
                }

                if (ManualUploading)
                    break;

                Thread.Sleep(delayPerItem);
            }
        }      
    }
}
