using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows;

using Autofac;
using LoaderBase.Communication;
using LoaderBase.LoaderResultMapUpDown;
using LoaderBase.LoaderLog;
using ProberErrorCode;
using ProberInterfaces;
using LogModule;
using SpoolingUtil;
using SpoolingUtil.TransferMethod;
using NoticeDialog;
using LoaderResultMapUpDown.LoaderResulMapManualUpload;
//using ProberViewModel.ViewModel;
//using ProberViewModel.View.LoaderResulMapManualUpload;

namespace LoaderResultMapUpDown
{
    /// <summary>
    /// Loader에서 Cell로 전달받은 Result MAP 파일을 외부 서버에 Upload 및 Download 처리를 하는 Class
    /// </summary>

    public class LoaderResultMapUpDownMng : ILoaderResultMapUpDownMng, IFactoryModule, IDisposable, ILoaderFactoryModule
    {

        #region ==> define lists
        public enum TransferMethodType
        {
            UsingFTP,
            UsingNetwork_LocalDisk,
            UsingNetwork_NetDrv,
            UsingUnknownType
        }

        #endregion

        #region ==> Variable
        public bool Initialized { get; set; }
        public InitPriorityEnum InitPriority { get; set; }

        private ILoaderCommunicationManager LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        /// Path
        private ILoaderLogManagerModule LoaderLogmanager => this.GetLoaderContainer().Resolve<ILoaderLogManagerModule>(); //ResultMap Upload/Down 옵션이 해당 클래스에서 관리되고 있다.

        private readonly string ftpPattern = @"^ftp://|FTP:// ";
        private readonly string networkPattern = @"^\\(\\(\w+\s*)+)";
        private readonly string localPattern = @"^[A-Z]:(\\(\w+\s*)+)";
        private readonly string engineerModeKeyword = "semics:";
        private readonly string localDownPath = @"C:\ProberSystem\LoaderSystem\EMUL\Parameters\Resultmaps\Download\";

        public int FTPReqTimeout { get; set; } = 5000;
        public int FTPStreamReadWriteTimeout { get; set; } = 5000;
        public ITransferMethods ITransferMethod { get; set; } ///외부 서버에 전달처리를 하는 Interface
        public Dictionary<TransferMethodType, ITransferMethods> TransferMethodStorage { get; set;}

        SpoolingManager spoolingMng;
        NoticeDialog.NoticeWindow noticeDialog;        
        NoticeDialogViewModel NoticeDlgVM;
        LoaderResultMapManualUploadDlg maualUploadDlg;
        LoadResultMapManualUploadViewVM maualUploadDlgVM;
        #endregion

        #region ==>Init & Destroy
        public LoaderResultMapUpDownMng()
        {
            TransferMethodStorage = new Dictionary<TransferMethodType, ITransferMethods>();
            int retryDelayTime = this.LoaderLogmanager.LoaderLogParam.ResultMapUploadDelayTime.Value * 1000;
            spoolingMng = new SpoolingManager(this.LoaderLogmanager.LoaderLogParam.SpoolingBasePath.Value, this.LoaderLogmanager.LoaderLogParam.ResultMapUploadRetryCount.Value, 500, retryDelayTime);
            spoolingMng.InitSpooling(this.LoaderLogmanager.LoaderLogParam.ResultMapUpLoadPath.Value, this.LoaderLogmanager.LoaderLogParam.UserName.Value, this.LoaderLogmanager.LoaderLogParam.Password.Value, this.LoaderLogmanager.LoaderLogParam.FTPUsePassive.Value);
            spoolingMng.CallBackFunc = FailedToUploadCallBack;

            noticeDialog = new NoticeDialog.NoticeWindow();                            
            noticeDialog.Topmost = true;
            NoticeDlgVM = noticeDialog.DataContext as NoticeDialogViewModel;
            if (null != NoticeDlgVM) 
            {
                NoticeDlgVM.CustomCallBack = ShowManaualUploadDlg;
                NoticeDlgVM.Name = "Result Map";
            }
            
            maualUploadDlg = new LoaderResultMapManualUploadDlg(spoolingMng);
            maualUploadDlg.Topmost = true;
            maualUploadDlgVM = maualUploadDlg.DataContext as LoadResultMapManualUploadViewVM;
        }

        /// <summary>
        /// ResultMap Manual upload 창 출력
        /// </summary>
        public void ShowManaualUploadDlg()
        {       
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (null != maualUploadDlg)
                    {
                        if (maualUploadDlg.Visibility == Visibility.Visible)
                        {
                            return;
                        }
                        if (null != maualUploadDlgVM)
                        {
                            maualUploadDlg.Left = SystemParameters.WorkArea.Left + (SystemParameters.WorkArea.Width - maualUploadDlg.ActualWidth) / 2;
                            maualUploadDlg.Top = SystemParameters.WorkArea.Top + (SystemParameters.WorkArea.Height - maualUploadDlg.ActualHeight) / 2;
                            maualUploadDlgVM.ShowMaualUploadDlg();
                        }

                    }
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }            
        }
                  
        /// <summary>
        /// result map upload 실패에 따른 callback 함수
        /// </summary>
        /// <param name="showNoticeInfo"></param>
        public void FailedToUploadCallBack(bool showNoticeInfo)
        {
            ShowFailedNoticeDlg(showNoticeInfo);
        }

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }

        public void DeInitModule()
        {
            return;
        }

        public void Dispose()
        {
            if (null != spoolingMng)
                spoolingMng.Dispose();

            return;
        }
        #endregion

        #region ==> private function
        /// <summary>
        /// Upload 실패에 따른 실패 Dialog 창 출력
        /// </summary>
        /// <param name="showNoticeInfo"> show or hide 처리</param>
        private void ShowFailedNoticeDlg(bool showNoticeInfo)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (null != noticeDialog)
                    {
                        if ((showNoticeInfo && noticeDialog.Visibility == Visibility.Visible) ||
                               (false == showNoticeInfo && noticeDialog.Visibility == Visibility.Hidden) ||
                               (showNoticeInfo && maualUploadDlg.Visibility == Visibility.Visible)
                            )
                        {
                            return;
                        }

                        if (showNoticeInfo)
                        {
                            noticeDialog.Left = SystemParameters.WorkArea.Left + (SystemParameters.WorkArea.Width - noticeDialog.ActualWidth) / 2;
                            noticeDialog.Top = SystemParameters.WorkArea.Top + (SystemParameters.WorkArea.Height - noticeDialog.ActualHeight) / 2;
                            noticeDialog.Show();
                        }
                        else
                        {
                            noticeDialog.Hide();
                        }
                    }
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }           
        }

        /// <summary>
        /// Cell로 부터 받은 ResultMAP파일이 배치하는 경로 생성 및 file full path 반환
        /// </summary>
        /// <returns>
        /// bool : 함수 성공 여부
        /// string: file이 생성될 full path
        /// </returns>
        private (bool, string) MakeResultMapPathForRecvCell(int stageindex, string filename)
        {
            string foldername = Path.GetFileNameWithoutExtension(filename);
            string Loaderrootpath = Path.Combine("C:\\ProberSystem\\LoaderSystem\\EMUL\\Parameters\\Resultmaps",
                                                        "Upload", "Cell" + stageindex.ToString().PadLeft(2, '0'));
            string localDirectory = Path.Combine(Loaderrootpath, foldername);
            string localpath = Path.Combine(Loaderrootpath, foldername, filename);

            bool success = true;
            if (false == Directory.Exists(localDirectory))
            {
                try
                {
                    Directory.CreateDirectory(localDirectory);
                }
                catch(Exception err)
                {
                    success = false;
                    LoggerManager.Error($"[ResultMapUpDown] {MethodBase.GetCurrentMethod()} : Failed to create directory about Result Map. Err = {err.Message})");
                }                                    
            }
            else
            {
                if (File.Exists(localpath))
                {
                    File.Delete(localpath);
                }
            }

            return (success, localpath);
        }

        /// <summary>
        /// cell로 부터 Result Map 파일을 가져와 파일로 생성한다.
        /// </summary>
        /// <param name="stageindex"> cell index </param>
        /// <param name="filename"> result map file name </param>
        /// <param name="resultMapFilePath"> result map file full path </param>
        /// <returns></returns>
        private bool GetResultMapFileData(int stageindex, string filename, string resultMapFilePath)
        {
            bool success = false;
            var cell = this.LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stageindex);
            if(null != cell)
            {
                try
                {
                    File.WriteAllBytes(resultMapFilePath, cell.GetRMdataFromFileName(filename));
                    success = true;
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Failed to write Result Map file data. Err = {err.Message}");
                }
            }
            else
            {
                LoggerManager.Error($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Failed to get stage proxy(stage idx:{stageindex})");
            }
            return success;
        }

        /// <summary>
        /// ResultMap file을 Cell로 전송
        /// </summary>
        /// <param name="stageindex"></param>
        /// <param name="targetFilePath"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        private bool SendResultMapFileData(int stageindex, string targetFilePath, string filename)
        {
            var RMServiceClient = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(stageindex);
            if(null != RMServiceClient)
            {
                try
                {
                    if (File.Exists(targetFilePath))
                    {
                        byte[] resultmap = File.ReadAllBytes(targetFilePath);
                        if(resultmap?.Length > 0)
                        {
                            bool Ret = RMServiceClient.SetResultMapByFileName(resultmap, filename);
                            if(false == Ret)
                            {
                                LoggerManager.Error($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} - Failed to send 'ResultMap' (file ={filename})");
                                return false;
                            }
                        }
                        else
                        {
                            int Len = (null != resultmap ? resultmap.Length : 0);
                            LoggerManager.Error($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} - read data is wrong(file ={filename}, read len ={Len})");
                            return false;
                        }


                        return true;
                    }
                    else
                    {
                        LoggerManager.Error($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} - can not found file(path={targetFilePath})");
                        return false;
                    }
                }
                catch(Exception err)
                {
                    LoggerManager.Error($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} - Error occurred. Err = {err.Message}");
                    LoggerManager.Exception(err);
                }

                return false;
            }
            else
            {
                LoggerManager.Error($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} - can't get cell proxy(object is null), cell idx ={stageindex}");
                return false;
            }
        }

        /// <summary>
        /// Engineer Mode로 작동하기 위한 path인지 확인
        /// </summary>
        /// <param name="accessPath"> Path  </param>
        /// <returns> Engineer Mode 여부 반환 </returns>
        private bool CheckEngineerMode(string accessPath)
        {
            if (accessPath.Length >= 7 )
            {
                string checkStr = accessPath.Substring(0, 7);
                if (checkStr.ToUpper() == engineerModeKeyword.ToUpper())
                {
                    LoggerManager.Debug($"[ResultMapUpDown] Path is Engineer mode");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// path에 따라 전송 방식을 결정
        /// </summary>
        /// <param name="serverpath"> 전송할 server path</param>
        /// <param name="transferMethod"> 전송할 방식</param>
        /// <returns>true - 성공, false - 실패</returns>
        private bool SetTransferMethod(string serverpath, out TransferMethodType transferMethod)
        {
            if (Regex.IsMatch(serverpath, ftpPattern)) // ftp 
            {
                if (TransferMethodStorage.ContainsKey(TransferMethodType.UsingFTP))
                    ITransferMethod = TransferMethodStorage[TransferMethodType.UsingFTP];
                else
                {
                    ITransferMethod = new TransferUsingFTP(this.LoaderLogmanager.LoaderLogParam.UserName.Value, this.LoaderLogmanager.LoaderLogParam.Password.Value, this.LoaderLogmanager.LoaderLogParam.FTPUsePassive.Value);
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
            else if(Regex.IsMatch(serverpath, networkPattern))
            {
                // to-do ?
                LoggerManager.Debug($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Upload path networkType(not supported). path ={serverpath}");
            }
            else
            {
                LoggerManager.Debug($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Upload path is wrong. path ={serverpath}");

            }

            transferMethod = TransferMethodType.UsingUnknownType;
            return false;
        }


        #endregion

        /// <summary>
        /// Result Map파일을 Server로 전달(Cell로 부터 Map파일 가져옴)
        /// </summary>
        /// <param name="stageindex"> cell idx</param>
        /// <param name="filename"> file name</param>
        /// <returns></returns>
        public EventCodeEnum CellResultMapUploadToServer(int stageindex, string filename)
        {
            try
            {
                var task = Task<EventCodeEnum>.Run(() =>
                {
                    LoggerManager.Debug($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Get ResultMap file from cell(stage idx:{stageindex}, fileName:{filename})");
                    /// 1. create dir and getting path
                    (bool success, string resultMapFilePath) = MakeResultMapPathForRecvCell(stageindex, filename);
                    if (false == success)
                    {
                        LoggerManager.Debug($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Failed to proc 'MakeResultMapPathForRecvCell'(stage idx:{stageindex}, fileName:{filename})");
                        return EventCodeEnum.RESULTMAP_MAKE_PATH_FAIL;
                    }


                    /// 2. get result map file from cell 
                    success = GetResultMapFileData(stageindex, filename, resultMapFilePath);
                    if (false == success)
                    {
                        LoggerManager.Debug($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Failed to proc 'GetResultMapFileData'(stage idx:{stageindex}, fileName:{filename})");
                        return EventCodeEnum.RESULTMAP_GET_MAPFILE_FAIL;
                    }


                    /// 3. Check the path(also check 'Engineer mode')
                    string serverpath = this.LoaderLogmanager.LoaderLogParam.ResultMapUpLoadPath.Value;
                    string path = string.Empty;
                    if (serverpath == null || serverpath == string.Empty || CheckEngineerMode(serverpath))
                    {
                        LoggerManager.Debug($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : server path is empty or EngineerMode(path={serverpath})");
                        return EventCodeEnum.NONE;
                    }

                    /// 4. Select transfer method                     
                    TransferMethodType transferMethod;
                    if (false == SetTransferMethod(serverpath, out transferMethod))
                    {
                        LoggerManager.Debug($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Faile to proc 'SetTransferMethod'(path={serverpath})");
                        return EventCodeEnum.RESULTMAP_UNKNOW_TRANSFER_METHOD_FAIL;
                    }

                    if (TransferMethodType.UsingFTP == transferMethod) //ftp 옵션 설정(Resultmap Upload 시 기존 설정)
                    {
                        TransferUsingFTP transferFtp = ITransferMethod as TransferUsingFTP;
                        if (null != transferFtp)
                        {
                            transferFtp.FtpFunctionObj.UseBinary = false;
                            transferFtp.FtpFunctionObj.Timeout = 30000;
                            transferFtp.FtpFunctionObj.ReadWriteStreamTimeout = 30000;
                        }
                    }

                    /// 5. upload file to server
                    if (null != ITransferMethod)
                    {
                        string user_name = this.LoaderLogmanager.LoaderLogParam.UserName.Value;
                        string password = this.LoaderLogmanager.LoaderLogParam.Password.Value;
                        if (ITransferMethod.UploadFileData(serverpath, resultMapFilePath, filename, user_name, password))
                        {
                            LoggerManager.Debug($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Upload sucess(server path={serverpath}, targetFile={resultMapFilePath}, user={user_name}, pwd={password})");
                            return EventCodeEnum.NONE;
                        }
                        else
                        {
                            LoggerManager.Debug($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Faile to Upload (server path={serverpath}, targetFile={resultMapFilePath})");

                            string separator = "/";
                            if (TransferMethodType.UsingFTP != transferMethod)
                                separator = "\\";

                            string foldername = Path.GetFileNameWithoutExtension(filename);
                            string upload_subPath = filename.Substring(0, 4) + separator + foldername + separator + filename;
                            spoolingMng.AddToItemInSpooling(resultMapFilePath, upload_subPath, stageindex, false);
                            ShowFailedNoticeDlg(true);

                            return EventCodeEnum.RESULTMAP_UPLOAD_FAIL;
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Failed to set transfer mode(ITransferMethod mode is null)");
                        return EventCodeEnum.UNKNOWN_EXCEPTION;
                    }
                });
                task.Wait();
                return task.Result;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{MethodBase.GetCurrentMethod().Name} - Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.UNKNOWN_EXCEPTION;
        }

        /// <summary>
        /// Server로 부터 Map파일을 download하여 cell에 배치
        /// </summary>
        /// <param name="stageindex"></param>
        /// <param name="filename"></param>
        /// <returns>EventCodeEnum </returns>
        public EventCodeEnum ServerResultMapDownloadToCell(int stageindex, string filename)
        {
            try
            {
                var task = Task<EventCodeEnum>.Run(() =>
                {
                    LoggerManager.Debug($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Before Cell{stageindex.ToString().PadLeft(2, '0')} anf Loader Download resultmap({filename}) from server");

                    /// 1. Check the path(also check 'Engineer mode')
                    string serverpath = this.LoaderLogmanager.LoaderLogParam.ResultMapDownLoadPath.Value;
                    if (serverpath == null || serverpath == string.Empty || CheckEngineerMode(serverpath))
                    {
                        LoggerManager.Debug($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : server path is empty or EngineerMode(path={serverpath})");
                        return EventCodeEnum.NONE;
                    }

                    /// 2. select transfer method                     
                    TransferMethodType transferMethod;
                    if (false == SetTransferMethod(serverpath, out transferMethod))
                    {
                        LoggerManager.Error($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} - can't select method( path ={serverpath})");
                        return EventCodeEnum.RESULTMAP_UNKNOW_TRANSFER_METHOD_FAIL;
                    }

                    /// 3. download file and send to cell
                    if (null != ITransferMethod)
                    {
                        string user_name = this.LoaderLogmanager.LoaderLogParam.UserName.Value;
                        string password = this.LoaderLogmanager.LoaderLogParam.Password.Value;
                        if (TransferMethodType.UsingFTP == transferMethod) //ftp 옵션 설정(Resultmap download을 위한 기존 설정)
                        {
                            TransferUsingFTP transferFtp = ITransferMethod as TransferUsingFTP;
                            if(null != transferFtp)
                            {
                                transferFtp.FtpFunctionObj.UseBinary = false;
                                transferFtp.FtpFunctionObj.Timeout = 30000;
                                transferFtp.FtpFunctionObj.ReadWriteStreamTimeout = 30000;
                            }
                        }

                        if (ITransferMethod.DownloadFileData(serverpath, filename, localDownPath, user_name, password)) /// download result map
                        {
                            LoggerManager.Debug($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} - Successfully moving file({filename}) from server to loader");
                            string destSubPath = Path.GetFileNameWithoutExtension(filename);

                            string downloadFullpath = Path.Combine(localDownPath, destSubPath, filename); 
                            bool Ret = SendResultMapFileData(stageindex, downloadFullpath, filename); /// send to cell
                            if (Ret)
                            {
                                LoggerManager.Debug($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Successfully moving file({filename}) from loader to cell({stageindex})");
                                return EventCodeEnum.NONE;
                            }
                            else
                            {
                                LoggerManager.Error($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Failed to send file({filename}) to cell({stageindex})");
                                return EventCodeEnum.RESULTMAP_SEND_TO_CELL_FAIL;
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Failed to download (server path = {serverpath}, downPath={localDownPath + filename})");
                            return EventCodeEnum.RESULTMAP_DOWNLOAD_FAIL;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} : Failed to set transfer mode(ITransferMethod mode is null)");
                        return EventCodeEnum.UNKNOWN_EXCEPTION;
                    }
                });
                task.Wait();
                return task.Result;
            }
            catch(Exception err)
            {
                LoggerManager.Error($"{MethodBase.GetCurrentMethod().Name} - Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.UNKNOWN_EXCEPTION;
        }

        /// <summary>
        /// Server 경로에 해당 path로 접속하여 존재여부 반환(해당 함수에서 접근 경로는 Download Path이다)
        /// </summary>
        /// <param name="foldername"></param>
        /// <returns> EventCodeEnum </returns>
        public EventCodeEnum ServerResultMapPathCheck(string foldername)
        {
            try
            {
                string serverpath = this.LoaderLogmanager.LoaderLogParam.ResultMapDownLoadPath.Value;
                if (serverpath == null || serverpath == string.Empty || CheckEngineerMode(serverpath))
                    return EventCodeEnum.NONE;

                TransferMethodType transferMethod;
                if (false == SetTransferMethod(serverpath, out transferMethod))
                {
                    LoggerManager.Error($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} - can't select method( path ={serverpath})");
                    return EventCodeEnum.RESULTMAP_UNKNOW_TRANSFER_METHOD_FAIL;
                }

                /// 3. download file and send to cell
                if (null != ITransferMethod)
                {
                    string user_name = this.LoaderLogmanager.LoaderLogParam.UserName.Value;
                    string password = this.LoaderLogmanager.LoaderLogParam.Password.Value;

                    if (ITransferMethod.PathCheck(serverpath, foldername, user_name, password))
                    {
                        return EventCodeEnum.NONE;
                    }
                    else
                        return EventCodeEnum.RESULTMAP_FOLDER_NOT_EXIST;
                }
                else
                {
                    LoggerManager.Error($"[ResultMapUpDown] {MethodBase.GetCurrentMethod().Name} - Transfer method is null");
                    return EventCodeEnum.UNKNOWN_EXCEPTION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.UNKNOWN_EXCEPTION;
            }
        }

        /// <summary>
        /// spooling 관련 옵션 변경 처리 함수
        /// </summary>
        /// <param name="upload_basePath"> upload 할 path</param>
        /// <param name="username"> user name</param>
        /// <param name="password"> password</param>
        /// <param name="ftp_userpassive"> ftp 사용시 passive mode</param>
        /// <param name="retry_count"> spooling 동작 시 재시도 할 count</param>
        /// <param name="reupload_delayTm"> spooling 동작 주시 시간(sec)</param>
        public void SpoolingOptionUpdate(string upload_basePath, string username, string password, bool ftp_userpassive, int retry_count, int reupload_delayTm)
        {
            if (null != spoolingMng)
                spoolingMng.UpdateSpoolingOption(upload_basePath, username, password, ftp_userpassive, retry_count, reupload_delayTm);
        }
    }
}
