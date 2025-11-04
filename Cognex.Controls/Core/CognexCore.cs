using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cognex.Controls.Core
{
    using Cognex.Command;
    using LoaderBase.AttachModules.ModuleInterfaces;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Media.Imaging;

    public class CognexCore : ICognexProcessManager
    {
        /*
         * cognex.win32display.exe
         * 
         * Cognex Module과 직접 통신하는 프로세스, ProberSystem.exe는 cognex.win32display.exe와 통신을 통해서
         * Cognex Module을 제어 한다.
         * 
         * ProberSystem.exed 와 같은 폴더에 있어야 한다.
         */
        private const String _CognexEXEPath = @".\cognex.win32display.exe";//==> Cogenx Process 이름, 
        private const int _WM_COPYDATA = 0x4A;
        private IntPtr _ClientWindowHandle;
        private bool _IsReadyCognexProcess;
        private AutoResetEvent _AutoReset = new AutoResetEvent(false);
        private bool _IsProcessInit;
        private object ocrLockObj = new object();
        public CognexCommandManager CognexCommandManager { get; set; }
        public CognexProcessSysParameter CognexProcSysParam { get; set; }
        public CognexProcessDevParameter CognexProcDevParam { get; set; }
        public Process CognexProcess { get; set; }
        public String ResponseData { get; set; }

        public int checkDotMat { get; set; }
        public List<string> Ocr { get; set; }//==> OCR String, Cognex Module Index로 접근 가능
        public List<string> LastOcrResult { get; set; }//==> Last OCR String, Cognex Module Index로 접근 가능 -> Fail일 경우 읽은 OCR 정보 저장을 위함
        public List<double> OcrScore { get; set; }//==> OCR Score, Cognex Module Index로 접근 가능
        public List<EnumCognexModuleState> HostRunning { get; set; }//==> OCR Status, Cognex Module Index로 접근 가능
        
        public bool InitWindow(IntPtr clientWindowHandle)
        {
            CreateCognexProcess();

            //==> cognex.win32display.exe 프로세스와 통신하기 위해 cognex.win32display.exe 프로세스에 대한 Window Handle을 받는다.
            _ClientWindowHandle = clientWindowHandle;

            _IsProcessInit = SetupCognexProcess().Result;

            return _IsProcessInit;
        }
        private void CreateCognexProcess()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = _CognexEXEPath;
            psi.Arguments = _ClientWindowHandle.ToString();

            CognexProcess = new Process();
            CognexProcess.StartInfo = psi;


            try
            {
                //==> cognex.win32display.exe 생성
                bool ret = CognexProcess.Start();
                CognexProcess.WaitForInputIdle();//==> cognex.win32display.exe가 생성 될 때까지 응답대기
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
            }
        }
        private bool[] ManualOCRState = new bool[3];
        public void SetManualOCRState(int hostIdx, bool isSuccess)
        {
            ManualOCRState[hostIdx] = isSuccess;
        }
        public bool GetManualOCRState(int hostIdx)
        {
            return ManualOCRState[hostIdx];
        }
        private Task<bool> SetupCognexProcess()
        {
            //SetParent(new IntPtr(), new IntPtr());
            _IsReadyCognexProcess = false;
            return Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int delayTime = 4000;
                while (true)
                {
                    SendMessage(CognexProcess.MainWindowHandle, COPYDATASTRUCT_KEYS.Setup, "");
                    if (_IsReadyCognexProcess)
                    {
                        SetParent(CognexProcess.MainWindowHandle, _ClientWindowHandle);
                        break;
                    }
                    else if (sw.ElapsedMilliseconds > delayTime)
                        break;

                    System.Threading.Thread.Sleep(100);
                }

                return _IsReadyCognexProcess;
            });
        }

        //==> cognex.win32display.exe로 부터의 응답을 받고 처리
        public void WndProc(int msg, IntPtr lParam)
        {

            if (msg != _WM_COPYDATA)
                return;

            try
            {
                COPYDATASTRUCT cds = (COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(COPYDATASTRUCT));
                String[] split = cds.lpData.Split(';');

                COPYDATASTRUCT_KEYS key;
                if (Enum.TryParse<COPYDATASTRUCT_KEYS>(split[0], out key) == false)
                    return;

                string val = split[1];
                switch (key)
                {
                    case COPYDATASTRUCT_KEYS.Setup:
                        _IsReadyCognexProcess = true;
                        break;
                    case COPYDATASTRUCT_KEYS.ConnectDisplay:
                        ResponseData = val;
                        _AutoReset.Set();
                        break;
                    case COPYDATASTRUCT_KEYS.ConnectNative:
                        ResponseData = val;
                        _AutoReset.Set();
                        break;
                    case COPYDATASTRUCT_KEYS.NativeCommand:
                        ResponseData = val;
                        _AutoReset.Set();
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void OnSizeChanged(EventArgs e, int clientWindowWidth, int clientWindowHeight)
        {
            //==> NOT USE
            if (CognexProcess == null)
                return;

            MoveWindow(CognexProcess.MainWindowHandle, 0, 0, clientWindowWidth, clientWindowHeight, true);
        }
        public void SendMessage(IntPtr handle, COPYDATASTRUCT_KEYS key, string data)
        {
            //==> NOT USE
            string msgData = string.Format("{0};{1}", key, data);

            COPYDATASTRUCT cds = new COPYDATASTRUCT();
            cds.dwData = _ClientWindowHandle;
            cds.cbData = Encoding.Default.GetByteCount(msgData) + 1;
            cds.lpData = msgData;

            SendMessage(handle, _WM_COPYDATA, 0, ref cds);
        }
        public Task<bool> ConnectDisplay(String host)
        {
            //==> NOT USE
            _AutoReset.Reset();
            ResponseData = String.Empty;
            SendMessage(CognexProcess.MainWindowHandle, COPYDATASTRUCT_KEYS.ConnectDisplay, host);
            _AutoReset.WaitOne(10000);

            return Task.FromResult<bool>(ResponseData == "True");
        }
        public Task<bool> ConnectNative(String host)
        {
            //==> NOT USE
            _AutoReset.Reset();
            ResponseData = String.Empty;
            SendMessage(CognexProcess.MainWindowHandle, COPYDATASTRUCT_KEYS.ConnectNative, host);
            _AutoReset.WaitOne(10000);

            return Task.FromResult<bool>(ResponseData == "True");
        }
        public void DisconnectDisplay()
        {
            //==> NOT USE
            SendMessage(CognexProcess.MainWindowHandle, COPYDATASTRUCT_KEYS.DisconnectDisplay, "");
        }
        public void DisconnectNative()
        {
            //==> NOT USE
            SendMessage(CognexProcess.MainWindowHandle, COPYDATASTRUCT_KEYS.DisconnectNative, "");
        }
        public void SwitchOnlineMode()
        {
            //==> NOT USE
            SendMessage(CognexProcess.MainWindowHandle, COPYDATASTRUCT_KEYS.Online, "");
        }
        public void SwitchLiveMode()
        {
            //==> 영상을 실시간으로 읽어 들이는 모드로 변경
            SendMessage(CognexProcess.MainWindowHandle, COPYDATASTRUCT_KEYS.Live, "");
        }
        public void SwitchGraphics(bool flag)
        {
            //==> NOT USE
            SendMessage(CognexProcess.MainWindowHandle, COPYDATASTRUCT_KEYS.Graphics, flag.ToString());
        }
        public void SwitchSpreadSwitch()
        {
            //==> NOT USE
            SendMessage(CognexProcess.MainWindowHandle, COPYDATASTRUCT_KEYS.SpreadSheet, "");
        }
        public String GetOCRString(String host, bool adjust, out String score)
        {
            /*
             * host : IP 주소
             * adjust : OCR 읽기 실패시 재조정을 하여 다시 시도 여부
             */
            lock (ocrLockObj)
            {
                if (adjust)
                {
                    //==> 실패시 조명을 바꿔가며 재 시도
                    CognexCommandManager.ReadConfig.GenerateCommandFormatFrame("A4", "-1");
                }
                else
                {
                    CognexCommandManager.ReadConfig.GenerateCommandFormatFrame("A4", "0");
                }

                DO_ReadConfig(host, "1");
                String ocr = CognexCommandManager.ReadConfig.String;
                score = CognexCommandManager.ReadConfig.Score;                
                LoggerManager.Debug($"OCR Operation: GetOCRString() . host:{host}, adjust:{adjust}, OCR:{ocr}, Score:{score}");
                CognexCommandManager.ReadConfig.Score = "0";
                return ocr;
            }
        }

        public bool SaveOCRImage(int hostIndex)
        {
            bool retVal = false;
            try
            {
                String firstCognexHostIP = CognexProcSysParam.GetIPOrNull_Index(hostIndex);
                retVal = DO_RI(firstCognexHostIP);
                if (retVal)
                {
                    BitmapImage image = CognexCommandManager.CognexRICommand.GetBitmapImage();
                    retVal = LoggerManager.CognexLoggerCtl.SaveImage(image);
                }
                else
                {
                    LoggerManager.Debug($"SaveOCRImage() : hostIndex= {hostIndex}, DO_RI() result is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return retVal;
        }
        public EventCodeEnum SetConfig(String hostIP, CognexConfig config)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int _MaxLightIntensity = 255;
                int _CognextMaxLightIntensity = 51;
                float _ConvertToCognexLightRatio = (float)_CognextMaxLightIntensity / (float)_MaxLightIntensity;
                double _RegionTheta = Convert.ToDouble(CognexCommandManager.GetConfigEx.RegionTheta);
                double _RegionPhi = Convert.ToDouble(CognexCommandManager.GetConfigEx.RegionPhi);
                DO_SetConfigOrientation(hostIP, config.Direction);
                DO_SetConfigMark(hostIP, config.Mark);
                DO_SetConfigChecksum(hostIP, config.CheckSum);
                DO_SetConfigRetry(hostIP, config.RetryOption);
                DO_SetConfigFieldString(hostIP, config.FieldString);
                DO_SetConfigRegion(hostIP, config.RegionY.ToString(), config.RegionX.ToString(), config.RegionHeight.ToString(), config.RegionWidth.ToString(), _RegionTheta.ToString(), _RegionPhi.ToString());
                DO_SetConfigCharSize(hostIP, config.CharHeight.ToString(), config.CharWidth.ToString());
                DO_SetConfigLightMode(hostIP, config.Light);
                DO_SetConfigLightPower(hostIP, (config.LightIntensity * _ConvertToCognexLightRatio).ToString());
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            return retVal;
        }

        public EventCodeEnum SetConfig(String hostIP, OCRConfig config)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                double _RegionTheta = 0;
                double _RegionPhi = 0;
                int _MaxLightIntensity = 255;
                int _CognextMaxLightIntensity = 51;
                float _ConvertToCognexLightRatio = (float)_CognextMaxLightIntensity / (float)_MaxLightIntensity;
                Double.TryParse(CognexCommandManager.GetConfigEx.RegionTheta, out _RegionTheta);
                Double.TryParse(CognexCommandManager.GetConfigEx.RegionPhi, out _RegionPhi);

                DO_SetConfigOrientation(hostIP, config.Direction);
                DO_SetConfigMark(hostIP, config.Mark);
                DO_SetConfigChecksum(hostIP, config.CheckSum);
                DO_SetConfigRetry(hostIP, config.RetryOption);
                DO_SetConfigFieldString(hostIP, config.FieldString);
                DO_SetConfigRegion(hostIP, config.RegionY.ToString(), config.RegionX.ToString(), config.RegionHeight.ToString(), config.RegionWidth.ToString(), _RegionTheta.ToString(), _RegionPhi.ToString());
                DO_SetConfigCharSize(hostIP, config.CharHeight.ToString(), config.CharWidth.ToString());
                DO_SetConfigLightMode(hostIP, config.Light);
                DO_SetConfigLightPower(hostIP, (config.LightIntensity * _ConvertToCognexLightRatio).ToString());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum GetOCRString(int hostIndex, bool adjust, out string ocr, out double ocrScore)
        {
            /*
             * host : IP 주소
             * adjust : OCR 읽기 실패시 재조정을 하여 다시 시도 여부
             */

            ocr = String.Empty;
            ocrScore = 0;

            if (hostIndex < 0 || hostIndex >= CognexProcDevParam.CognexHostList.Count)
            {
                return EventCodeEnum.COGNEX_OUT_OF_MODULE_INDEX;
            }

            String firstCognexHostIP = CognexProcSysParam.GetIPOrNull_Index(hostIndex);
            List<CognexConfig> configs = null;
            if (CognexProcDevParam.ConfigList.Count(i => i.Name == CognexProcDevParam.CognexHostList[hostIndex].ConfigName) > 0)
            {
                configs = CognexProcDevParam.ConfigList.Where(i => i.Name == CognexProcDevParam.CognexHostList[hostIndex].ConfigName).ToList();
            }
            else
            {
                configs = CognexProcDevParam.ConfigList.Where(i => i.Name == "DEFAULTDEVNAME").ToList();
            }


            for (int i = 0; i < configs.Count(); i++)
            {
                SetConfig(firstCognexHostIP, configs[i]);
                String ocrScoreStr = string.Empty;
                String ocrStr = GetOCRString(firstCognexHostIP, adjust, out ocrScoreStr);

                double dblOcrScore;
                double.TryParse(ocrScoreStr, out dblOcrScore);
                LoggerManager.Debug($"[OCR] String : {ocrStr}, Score : {ocrScoreStr}");
                //CognexConfig config = CognexProcDevParam.ConfigList.FirstOrDefault(item => item.Name == firstCognexHostConfig);
                //if (config == null)
                //{
                //    return EventCodeEnum.COGNEX_CONFIG_INVALID;
                //}
                if (configs[i].OCRCutLineScore <= 100)
                {
                    configs[i].OCRCutLineScore = 300;
                }

                if (dblOcrScore < configs[i].OCRCutLineScore)//==> Fail
                {
                    LoggerManager.Debug($"[OCR] String : {ocrStr}, Score : {ocrScoreStr}, Cut line : {configs[i].OCRCutLineScore}");
                }
                else
                {
                    ocr = ocrStr;
                    ocrScore = dblOcrScore;
                    return EventCodeEnum.NONE;
                }
            }



            return EventCodeEnum.COGNEX_SCORE_IS_UNDER_CUTLINE;
        }
        public EventCodeEnum GetOCRString(int hostIndex, bool adjust, out string ocr, out double ocrScore, OCRDevParameter ocrDev)
        {
            /*
             * host : IP 주소
             * adjust : OCR 읽기 실패시 재조정을 하여 다시 시도 여부
             */

            ocr = String.Empty;
            ocrScore = 0;

            if (hostIndex < 0)
            {
                return EventCodeEnum.COGNEX_OUT_OF_MODULE_INDEX;
            }

            String firstCognexHostIP = CognexProcSysParam.GetIPOrNull_Index(hostIndex);
            List<OCRConfig> configs = ocrDev.ConfigList;

            for (int i = 0; i < configs.Count(); i++)
            {
                SetConfig(firstCognexHostIP, configs[i]);
                String ocrScoreStr = string.Empty;
                String ocrStr = GetOCRString(firstCognexHostIP, adjust, out ocrScoreStr);

                double dblOcrScore;
                double.TryParse(ocrScoreStr, out dblOcrScore);
                LoggerManager.Debug($"[OCR] String : {ocrStr}, Score : {ocrScoreStr}");
                //CognexConfig config = CognexProcDevParam.ConfigList.FirstOrDefault(item => item.Name == firstCognexHostConfig);
                //if (config == null)
                //{
                //    return EventCodeEnum.COGNEX_CONFIG_INVALID;
                //}
                if (configs[i].OCRCutLineScore <= 100)
                {
                    configs[i].OCRCutLineScore = 300;
                }

                if (dblOcrScore < configs[i].OCRCutLineScore)//==> Fail
                {
                    LoggerManager.Debug($"[OCR] String : {ocrStr}, Score : {ocrScoreStr}, Cut line : {configs[i].OCRCutLineScore}");
                }
                else
                {
                    //LastOcrResult[hostIndex] = ocrStr;

                    if (configs[i].Mark == "11" && (GetCheckDotMat() == CognexCommandManager.ReadConfig.OrcList.Count))
                    {
                        ocrStr = CalOcrChecksum(ocrStr);
                    }
                    if (configs[i].CheckSum == "2")//SEMIChecksum
                    {
                        bool SEMI_CheckSum = SEMIChecksum(ocrStr);
                        if (SEMI_CheckSum)
                        {
                            ocr = ocrStr;
                            ocrScore = dblOcrScore;
                            LoggerManager.Debug($"[OCR] SEMI Checksum String : {ocrStr}, Score : {ocrScoreStr}, Cut line : {configs[i].OCRCutLineScore}");
                            return EventCodeEnum.NONE;
                        }
                        else//==> Fail
                        {
                            LoggerManager.Debug($"[OCR] SEMI Checksum Fail String : {ocrStr}, Score : {ocrScoreStr}, Cut line : {configs[i].OCRCutLineScore}");
                        }
                    }
                    else
                    {
                        ocr = ocrStr;
                        ocrScore = dblOcrScore;
                        return EventCodeEnum.NONE;
                    }
                }
            }



            return EventCodeEnum.COGNEX_SCORE_IS_UNDER_CUTLINE;
        }
        public EventCodeEnum GetOCRString(int hostIndex, bool adjust, out string ocr, out double ocrScore, OCRDevParameter ocrDev, LoaderBase.ActiveLotInfo activeLotInfo)
        {
            /*
             * host : IP 주소
             * adjust : OCR 읽기 실패시 재조정을 하여 다시 시도 여부
             */

            ocr = String.Empty;
            ocrScore = 0;

            if (hostIndex < 0)
            {
                return EventCodeEnum.COGNEX_OUT_OF_MODULE_INDEX;
            }

            String firstCognexHostIP = CognexProcSysParam.GetIPOrNull_Index(hostIndex);
            List<OCRConfig> configs = ocrDev.ConfigList;



            for (int i = 0; i < configs.Count(); i++)
            {
                SetConfig(firstCognexHostIP, configs[i]);
                String ocrScoreStr = string.Empty;
                String ocrStr = GetOCRString(firstCognexHostIP, adjust, out ocrScoreStr);

                double dblOcrScore;
                double.TryParse(ocrScoreStr, out dblOcrScore);
                LoggerManager.Debug($"[OCR] String : {ocrStr}, Score : {ocrScoreStr}");
                //CognexConfig config = CognexProcDevParam.ConfigList.FirstOrDefault(item => item.Name == firstCognexHostConfig);
                //if (config == null)
                //{
                //    return EventCodeEnum.COGNEX_CONFIG_INVALID;
                //}
                if (configs[i].OCRCutLineScore <= 100)
                {
                    configs[i].OCRCutLineScore = 300;
                }

                if (dblOcrScore < configs[i].OCRCutLineScore)//==> Fail
                {
                    LoggerManager.Debug($"[OCR] String : {ocrStr}, Score : {ocrScoreStr}, Cut line : {configs[i].OCRCutLineScore}");
                }
                else
                {
                    if (configs[i].Mark == "11" && (GetCheckDotMat() == CognexCommandManager.ReadConfig.OrcList.Count))
                    {
                        ocrStr = CalOcrChecksum(ocrStr);
                    }
                    if (configs[i].CheckSum == "2")//SEMIChecksum
                    {
                        bool SEMI_CheckSum = SEMIChecksum(ocrStr);
                        bool IntegrityEnable = CheckIntegrity(ocrDev, activeLotInfo, ocrStr);

                        if (IntegrityEnable && SEMI_CheckSum)
                        {
                            ocr = ocrStr;
                            ocrScore = dblOcrScore;
                            LoggerManager.Debug($"[OCR] SEMI Checksum String : {ocrStr}, Score : {ocrScoreStr}, Cut line : {configs[i].OCRCutLineScore}");
                            return EventCodeEnum.NONE;
                        }
                        else//==> Fail
                        {
                            LoggerManager.Debug($"[OCR] SEMI Checksum Fail String : {ocrStr}, Score : {ocrScoreStr}, Cut line : {configs[i].OCRCutLineScore}");
                        }
                    }
                    else
                    {
                        ocr = ocrStr;
                        ocrScore = dblOcrScore;
                        return EventCodeEnum.NONE;
                    }
                }
            }



            return EventCodeEnum.COGNEX_SCORE_IS_UNDER_CUTLINE;
        }

        public EventCodeEnum DoOCRStringCatch(int hostIndex, bool adjust)
        {

            if (hostIndex < 0 || hostIndex >= CognexProcDevParam.CognexHostList.Count)
            {
                return EventCodeEnum.COGNEX_OUT_OF_MODULE_INDEX;
            }
            if (HostRunning[hostIndex] == EnumCognexModuleState.RUNNING)
            {
                return EventCodeEnum.NONE;
            }

            //==> !!! OCR Read 읽음 완료 상태를 외부에서 바꿔 주어야 한다.
            HostRunning[hostIndex] = EnumCognexModuleState.RUNNING;
            EventCodeEnum result = EventCodeEnum.UNDEFINED;
            try
            {
                String ocr;
                double ocrScore;
                result = GetOCRString(hostIndex, adjust, out ocr, out ocrScore);

                //==> OCR 결과 저장
                Ocr[hostIndex] = ocr;
                OcrScore[hostIndex] = ocrScore;
                LoggerManager.Debug($"[DoOCR String Catch] hostIndex: {hostIndex}, adjust: {adjust} , OCR String: {ocr} , OCR Score:{ocrScore}");
            }
            catch (Exception ex)
            {
                LoggerManager.Error($"[COGNEX OCR] : {ex}");
            }
            finally
            {
                //HostRunning[hostIndex] = EnumCognexModuleState.READ_OCR;
            }

            return result;
        }

        public EventCodeEnum DoOCRStringCatch(int hostIndex, bool adjust, OCRDevParameter ocrDev, LoaderBase.ActiveLotInfo activeLotInfo)
        {
            LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.START, $"");

            if (hostIndex < 0)
            {
                return EventCodeEnum.COGNEX_OUT_OF_MODULE_INDEX;
            }
            if (HostRunning[hostIndex] == EnumCognexModuleState.RUNNING)
            {
                return EventCodeEnum.NONE;
            }

            //==> !!! OCR Read 읽음 완료 상태를 외부에서 바꿔 주어야 한다.
            HostRunning[hostIndex] = EnumCognexModuleState.RUNNING;
            EventCodeEnum result = EventCodeEnum.UNDEFINED;
            try
            {
                String ocr;
                double ocrScore;
                result = GetOCRString(hostIndex, adjust, out ocr, out ocrScore, ocrDev, activeLotInfo);

                //==> OCR 결과 저장
                Ocr[hostIndex] = ocr;
                OcrScore[hostIndex] = ocrScore;
                LoggerManager.Debug($"[DoOCR String Catch] hostIndex: {hostIndex}, adjust: {adjust} , OCR String: {ocr} , OCR Score:{ocrScore}");
            }
            catch (Exception ex)
            {
                LoggerManager.Error($"[COGNEX OCR] : {ex}");
            }
            finally
            {
                //HostRunning[hostIndex] = EnumCognexModuleState.READ_OCR;
            }

            return result;
        }

        public EventCodeEnum DoOCRStringCatch(int hostIndex, bool adjust, OCRDevParameter ocrDev)
        {

            if (hostIndex < 0)
            {
                return EventCodeEnum.COGNEX_OUT_OF_MODULE_INDEX;
            }
            if (HostRunning[hostIndex] == EnumCognexModuleState.RUNNING)
            {
                return EventCodeEnum.NONE;
            }

            //==> !!! OCR Read 읽음 완료 상태를 외부에서 바꿔 주어야 한다.
            HostRunning[hostIndex] = EnumCognexModuleState.RUNNING;
            EventCodeEnum result = EventCodeEnum.UNDEFINED;
            try
            {
                String ocr;
                double ocrScore;
                result = GetOCRString(hostIndex, adjust, out ocr, out ocrScore, ocrDev);

                //==> OCR 결과 저장
                Ocr[hostIndex] = ocr;
                OcrScore[hostIndex] = ocrScore;
                LoggerManager.Debug($"[DoOCR String Catch] hostIndex: {hostIndex}, adjust: {adjust} , OCR String: {ocr} , OCR Score:{ocrScore}");
            }
            catch (Exception ex)
            {
                LoggerManager.Error($"[COGNEX OCR] : {ex}");
            }
            finally
            {
                //HostRunning[hostIndex] = EnumCognexModuleState.READ_OCR;
            }

            return result;
        }

        public Task<EventCodeEnum> DoOCRStringCatchAsync(int hostIndex, bool adjust)
        {
            if (hostIndex < 0 || hostIndex >= CognexProcDevParam.CognexHostList.Count)
            {
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.COGNEX_OUT_OF_MODULE_INDEX);
            }
            if (HostRunning[hostIndex] == EnumCognexModuleState.RUNNING)
            {
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
            }

            Task<EventCodeEnum> taskOCR = new Task<EventCodeEnum>(() =>
            {
                HostRunning[hostIndex] = EnumCognexModuleState.RUNNING;

                String ocr;
                double ocrScore;
                EventCodeEnum result = GetOCRString(hostIndex, adjust, out ocr, out ocrScore);

                //==> OCR 결과 저장
                Ocr[hostIndex] = ocr;
                OcrScore[hostIndex] = ocrScore;

                //==> OCR을 읽은 상태로 바꾼다.
                HostRunning[hostIndex] = EnumCognexModuleState.READ_OCR;

                return result;
            });

            return taskOCR;
        }

        public Task<EventCodeEnum> DoOCRStringCatchAsync(int hostIndex, bool adjust, OCRDevParameter OCRDev)
        {
            if (hostIndex < 0 || hostIndex >= CognexProcDevParam.CognexHostList.Count)
            {
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.COGNEX_OUT_OF_MODULE_INDEX);
            }
            if (HostRunning[hostIndex] == EnumCognexModuleState.RUNNING)
            {
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
            }

            Task<EventCodeEnum> taskOCR = new Task<EventCodeEnum>(() =>
            {
                HostRunning[hostIndex] = EnumCognexModuleState.RUNNING;

                String ocr;
                double ocrScore;
                EventCodeEnum result = GetOCRString(hostIndex, adjust, out ocr, out ocrScore);

                //==> OCR 결과 저장
                Ocr[hostIndex] = ocr;
                OcrScore[hostIndex] = ocrScore;

                //==> OCR을 읽은 상태로 바꾼다.
                HostRunning[hostIndex] = EnumCognexModuleState.READ_OCR;

                return result;
            });

            return taskOCR;
        }
        public EventCodeEnum WaitForOCR(int hostIndex, int timeOut)
        {
            //==> OCR을 읽을 때 까지 대기 한다.
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            Stopwatch elapsedStopWatch = new Stopwatch();
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            try
            {
                bool runFlag = true;
                LoggerManager.Debug($"[CognexCore] WaitForOCR(): Start, HostIndex:{hostIndex}, Timeout:{timeOut}");
                System.Threading.Thread.Sleep(100);
                do
                {
                    if (HostRunning[hostIndex] == EnumCognexModuleState.READ_OCR)
                    {
                        //==> OCR을 읽었다.
                        retVal = EventCodeEnum.NONE;
                        runFlag = false;
                    }
                    else if (HostRunning[hostIndex] == EnumCognexModuleState.FAIL)
                    {
                        //==> OCR을 못 읽었다.
                        retVal = EventCodeEnum.UNDEFINED;
                        runFlag = false;
                    }
                    else if (HostRunning[hostIndex] == EnumCognexModuleState.ABORT)
                    {

                        retVal = EventCodeEnum.LOADER_FIND_NOTCH_FAIL;
                        runFlag = false;
                    }
                    else
                    {
                        if (elapsedStopWatch.ElapsedMilliseconds > timeOut)
                        {
                            retVal = EventCodeEnum.UNDEFINED;
                            runFlag = false;
                        }
                        else
                        {
                            runFlag = true;
                        }
                    }
                    System.Threading.Thread.Sleep(10);
                } while (runFlag == true);
                LoggerManager.Debug($"[CognexCore] WaitForOCR(): End, HostIndex:{hostIndex}, Timeout:{timeOut}, Result: {retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //==> OCR을 다 읽었으면 finally 가 실행 된다, OCR 상태를 IDLE로 만들어 다시 OCR을 읽을 수 있도록 상태를 초기화 한다.
                HostRunning[hostIndex] = EnumCognexModuleState.IDLE;
                elapsedStopWatch?.Stop();
            }

            //String cognexHostConfig = CognexProcDevParam.CognexHostList[hostIndex].ConfigName;
            //CognexConfig config = CognexProcDevParam.ConfigList.FirstOrDefault(item => item.Name == cognexHostConfig);
            //if (config == null)
            //{
            //    retVal = EventCodeEnum.COGNEX_CONFIG_INVALID;
            //}
            //else if (OcrScore[hostIndex] < config.OCRCutLineScore)
            //{
            //    retVal = EventCodeEnum.COGNEX_SCORE_IS_UNDER_CUTLINE;
            //}

            return retVal;
        }

        public bool DO_GetConfigEx(String host)
        {
            /*
             * Cognex Module의 조명, 문자열 타입등 설정 사항을 읽어 얻는다.
             * 읽은 데이터는 CognexCommandManager.GetConfigEx에 존재
             */
            String cmd = CognexCommandManager.GetConfigEx.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.GetConfigEx.ParseResponse(resp);

            return result;
        }
        public bool DO_AcquireConfig(String host)
        {
            /*
             * Cognex의 RI Command를 날리기 전에 먼저 호출 된다.
             */
            String cmd = CognexCommandManager.AcquireConfig.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.AcquireConfig.ParseResponse(resp);

            return result;
        }
        public bool DO_SetConfigRegion(String host, String y, String x, String height, String width, String theta, String phi)
        {
            /*
             * OCR 검색 영역 설정
             */
            SwitchLiveMode();
            CognexCommandManager.SetConfigRegion.SetCommandFormatArg(y, x, height, width, theta, phi);

            String cmd = CognexCommandManager.SetConfigRegion.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.SetConfigRegion.ParseResponse(resp);
            System.Threading.Thread.Sleep(30);
            SwitchLiveMode();

            return result;
        }
        public bool DO_SetConfigCharSize(String host, String height, String width)
        {
            /*
             * OCR 문자 크기 지정
             */
            CognexCommandManager.SetConfigCharSize.SetCommandFormatArg(height, width);

            String cmd = CognexCommandManager.SetConfigCharSize.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.SetConfigCharSize.ParseResponse(resp);

            return result;
        }
        public bool DO_SetConfigLightPower(String host, String lightPower)
        {
            /*
             * 조명 세기 조절
             */
            CognexCommandManager.SetConfigLightPower.SetCommandFormatArg(lightPower);

            String cmd = CognexCommandManager.SetConfigLightPower.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.SetConfigLightPower.ParseResponse(resp);

            return result;
        }

        ConcurrentQueue<Tuple<String, String, Func<bool>>> lightJobQueue = new ConcurrentQueue<Tuple<string, string, Func<bool>>>();
        bool lightTaskRun = false;
        public void Async_SetConfigLightPower(String host, String lightPower, Func<bool> callbackFunc)
        {
            /*
             * Async 방식으로 조명 세기 조절
             * 
             * 여러번 조명 세기를 바꾸더라도 맨 마지막에 바꾼 조명 값만 Cognex Module에 보낸다.
             */
            if (lightTaskRun)
            {
                lightJobQueue.Enqueue(new Tuple<string, string, Func<bool>>(host, lightPower, callbackFunc));
                return;
            }

            Task.Run(() =>
            {
                lightTaskRun = true;

                lightJobQueue.Enqueue(new Tuple<string, string, Func<bool>>(host, lightPower, callbackFunc));
                while (lightJobQueue.IsEmpty == false)
                {
                    Tuple<String, String, Func<bool>> item = null;
                    if (lightJobQueue.TryDequeue(out item) == false)
                        continue;

                    //==> 성능 최적화, 맨 마지막 작업만 실행 하기 위해
                    if (lightJobQueue.Count > 1)
                        continue;

                    String itemHost = item.Item1;
                    String itemLightPower = item.Item2;
                    Func<bool> itemFunc = item.Item3;

                    DO_SetConfigLightPower(itemHost, itemLightPower);

                    if (itemFunc != null)
                        itemFunc();
                }
                lightTaskRun = false;
            });
        }
        public bool DO_SetConfigLightMode(String host, String mode)
        {
            /*
             * 조명 타입 변경
             */
            CognexCommandManager.SetConfigLightMode.SetCommandFormatArg(mode);

            String cmd = CognexCommandManager.SetConfigLightMode.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.SetConfigLightMode.ParseResponse(resp);

            return result;
        }
        public bool DO_TuneConfigEx(String host, String flag, bool filter, bool light, bool size)
        {
            /*
             * Tuning 설정을 함
             * 
             * Tuning이란 OCR을 읽기위한 설정을 Fiter, 조명, 문자 Size등을 자동으로 조절하는 절차를 말함.
             * @ 조명만 바꾸기를 권장....
             */

            String filterFlag = filter ? "1" : "0";
            String lightFlag = light ? "1" : "0";
            String sizeFlag = size ? "1" : "0";

            CognexCommandManager.TuneConfigEx.SetCommandFormatArg(flag, filterFlag, lightFlag, sizeFlag);

            String cmd = CognexCommandManager.TuneConfigEx.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.TuneConfigEx.ParseResponse(resp);

            return result;
        }
        public bool DO_GetConfigTune(String host)
        {
            /*
             * Tunning Option의 설정 내용을 얻어옴.
             */
            String cmd = CognexCommandManager.GetConfigTune.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.GetConfigTune.ParseResponse(resp);

            return result;
        }
        public bool DO_GetFilterOperationList(String host)
        {
            /*
             * Filter 구성을 얻어옴,
             * Filter란 영상 처리에서 말하는 화면 필터를 말한다.
             */

            //==> NOT USE
            String cmd = CognexCommandManager.GetFilterOperationList.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.GetFilterOperationList.ParseResponse(resp);

            return result;
        }
        public bool DO_SetCustomFilterName(String host, String customFilterName)
        {
            /*
             * 현재 설정된 Filter 구성을 customFilterName 이름으로 저장
             */

            //==> NOT USE

            CognexCommandManager.SetCustomFilterName.SetCommandFormatArg($"\"{customFilterName}\"");

            String cmd = CognexCommandManager.SetCustomFilterName.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.SetCustomFilterName.ParseResponse(resp);

            return result;
        }
        public bool DO_RemoveFilterOperationAll(String host)
        {
            /*
             * 현재 설정된 Filter 구성을 제거
             */

            //==> NOT USE

            String cmd = CognexCommandManager.RemoveFilterOperationAll.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.RemoveFilterOperationAll.ParseResponse(resp);

            return result;
        }
        public bool DO_InsertFilterOperation(String host, params String[] args)
        {
            /*
             * Filter를 현재 구성에 추가.
             */

            //==> NOT USE

            CognexCommandManager.InsertFilterOperation.SetCommandFormatArg(args);

            String cmd = CognexCommandManager.InsertFilterOperation.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.InsertFilterOperation.ParseResponse(resp);

            return result;
        }
        public bool DO_SetConfigChecksum(String host, String checksum)
        {
            /*
             * Checksum 방식 설정(읽어들인 문자열이 맞는지 검사하는 방식을 Checksum이라 한다)
             * 
             * checksum
             * "Virtual", "0"
             * "SEMI", "1"
             * "BC 412 with Virtual", "3"
             * "IBM 412 with Virtual", "4"
             */

            CognexCommandManager.SetConfigChecksum.SetCommandFormatArg(checksum);

            String cmd = CognexCommandManager.SetConfigChecksum.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.SetConfigChecksum.ParseResponse(resp);

            return result;
        }
        public bool DO_SetConfigRetry(String host, String mode)
        {
            /*
             * Adjust Mode를 설정
             * 
             * mode
             * "Not Adjust", "0"
             * "Adjust", "1"
             * "Adjust & Save", "2"
             */

            CognexCommandManager.SetConfigRetry.SetCommandFormatArg(mode);

            String cmd = CognexCommandManager.SetConfigRetry.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.SetConfigRetry.ParseResponse(resp);

            return result;
        }

        public bool DO_SetConfigMark(String host, String mark)
        {
            /*
             * OCR 문자열 폰트 지정
             * 
             * mark
             * "BC, BC 412", "1"
             * "BC, IBM 412", "2"
             * "Internal Use Only", "3"
             * "Chars, SEMI", "4"
             * "Chars, IBM", "5"
             * "Chars, Triple", "6
             * "Chars, OCR-A ", "7"
             * "SEMI M1.15", "11"
             */

            CognexCommandManager.SetConfigMark.SetCommandFormatArg(mark);

            String cmd = CognexCommandManager.SetConfigMark.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.SetConfigMark.ParseResponse(resp);

            return result;
        }
        public bool DO_SetConfigOrientation(String host, String orientation)
        {
            /*
             * 이미지 영상 방향 변경
             * 
             * orientation
             * "Normal", "0"
             * "Mirrored horizontally", "1"
             * "Flipped vertically", "2"
             * "Rotated 180 degrees", "3"
             */
            CognexCommandManager.SetConfigOrientation.SetCommandFormatArg(orientation);

            String cmd = CognexCommandManager.SetConfigOrientation.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.SetConfigOrientation.ParseResponse(resp);

            return result;
        }
        public bool DO_ReadConfig(String host, String flag)
        {
            /*
             * OCR 값을 읽음
             * OCR String, Score 등의 값을 읽어 들임.
             */

            CognexCommandManager.ReadConfig.SetCommandFormatArg(flag);

            String cmd = CognexCommandManager.ReadConfig.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.ReadConfig.ParseResponse(resp);
            checkDotMat = 0;
            for (int i = 0; i < CognexCommandManager.ReadConfig.OrcList.Count; i++)
            {
                if (CognexCommandManager.ReadConfig.OrcList[i].Char == "0" &&
                    CognexCommandManager.ReadConfig.OrcList[i].Value == "0")
                {
                    checkDotMat++;
                }
            }
            return result;
        }
        public bool DO_SetConfigFieldString(String host, String fieldString)
        {
            /*
             * Field String을 설정
             * Field String이란 문자열 형식을 뜻한다(EX: ANNA-**-**)
             */

            CognexCommandManager.SetConfigFieldString.SetCommandFormatArg($"\"{fieldString}\"");

            String cmd = CognexCommandManager.SetConfigFieldString.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.SetConfigFieldString.ParseResponse(resp);

            return result;
        }
        public bool DO_RI(String host)
        {
            /*
             * Cognex OCR Module을 통해 Image를 읽어 들인다.
             */
            String cmd = CognexCommandManager.CognexRICommand.Command;
            cmd = CommandAndHostBinding(cmd, host);
            String resp = SendNativeCommand(cmd).Result;
            bool result = CognexCommandManager.CognexRICommand.ParseResponse(resp);

            return result;
        }
        public bool IsInit()
        {
            return _IsProcessInit;
        }
        public void Dispose()
        {
            if (CognexProcess == null)
                return;

            CognexProcess.Close();
            CognexProcess.Dispose();
        }
        private Task<String> SendNativeCommand(String command)
        {
            /*
             * !!! 모든 Command들은 이 함수를 통해 Cognex Module에 전달을 한다. !!!
             * 
             * Native Command를 cognex.win32display.exed에 보낸다
             * 
             */


            _AutoReset.Reset();//==> Command에 대한 응답을 받기위해 AutoReset을 Reset 상태로 둔다.
            ResponseData = String.Empty;

            //==> Command를 cognex.win32display.exe에 날리고 응답을 받는다면 AutoReset의 상태가 Set 싱테기 된다
            SendMessage(CognexProcess.MainWindowHandle, COPYDATASTRUCT_KEYS.NativeCommand, command);//==> 비동기

            _AutoReset.WaitOne(5000);//==> AutoReset의 상태가 Set 상태가 될때까지 대기

            return Task.FromResult<String>(ResponseData);
        }
        private String CommandAndHostBinding(String cmd, String host)
        {
            //==> host : Command를 보내고자 하는 Cognex Module의 IP 주소가 들어감.
            return $"{cmd}/{host}";//==> ip와 command를 조합하여 특정 IP로 전달되는 command를 만듬.
        }
        public bool SaveOCRImage(string host)
        {
            DO_RI(host);
            BitmapImage image = CognexCommandManager.CognexRICommand.GetBitmapImage();
            return LoggerManager.CognexLoggerCtl.SaveImage(image);
        }
        public void SaveConfig()
        {
            return;
        }
        public void LoadConfig()
        {
            return;
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, ref COPYDATASTRUCT lParam);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam, SendMessageTimeoutFlags fuFlags, uint uTimeout, out UIntPtr lpdwResult);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        public string CalOcrChecksum(string ocrString)
        {
            string retOCRString = null;
            string[] strOrdCS = new string[2];
            string[] strNewCS = new string[2];
            int[] chrtmp = new int[18];
            int i;
            int j;
            int seedval;
            try
            {
                strOrdCS[0] = "A";
                strOrdCS[1] = "0";
                retOCRString = ocrString + strOrdCS[0] + strOrdCS[1];

                for (i = 0; i < retOCRString.Length; i++)
                {
                    byte[] convertBytes = Encoding.ASCII.GetBytes(retOCRString.Substring(i, 1));
                    chrtmp[i] = convertBytes[0];
                }

                seedval = 0;

                for (i = 0; i < retOCRString.Length; i++)
                {
                    j = (seedval * 8) % 59;
                    j = j + (chrtmp[i] - 32);
                    seedval = j % 59;
                }

                if (seedval == 0)
                {
                    strNewCS[0] = strOrdCS[0];
                    strNewCS[1] = strOrdCS[1];
                }
                else
                {
                    seedval = 59 - seedval;
                    j = (seedval / 8) & 7;
                    i = seedval & 7;

                    char jchr = Convert.ToChar(j + 33 + 32);
                    char ichr = Convert.ToChar(i + 16 + 32);

                    strNewCS[0] = jchr.ToString();
                    strNewCS[1] = ichr.ToString();
                }
                retOCRString = ocrString + strNewCS[0] + strNewCS[1];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retOCRString;
        }
        public bool SEMIChecksum(String ocr)
        {

            char[] inputCheckSum = new char[2];
            bool result = false;
            try
            {
                if (ocr.Length > 2)
                {

                    inputCheckSum[0] = ocr[ocr.Length - 2];
                    inputCheckSum[1] = ocr[ocr.Length - 1];

                    StringBuilder stb = new StringBuilder(ocr);
                    stb[ocr.Length - 2] = 'A';
                    stb[ocr.Length - 1] = '0';
                    String strImsiOcrBuf = stb.ToString();

                    int[] chrtmp = new int[18];

                    for (int i = 0; i < strImsiOcrBuf.Length; i++)
                    {
                        chrtmp[i] = strImsiOcrBuf[i];
                    }

                    int seed = 0;

                    for (int i = 0; i < strImsiOcrBuf.Length; i++)
                    {
                        int j = (seed * 8) % 59;

                        j = j + chrtmp[i] - 32;

                        seed = j % 59;
                    }

                    char[] calcCheckSum = new char[2];

                    if (seed == 0)
                    {
                        calcCheckSum[0] = 'A';
                        calcCheckSum[1] = '0';
                    }
                    else
                    {
                        seed = 59 - seed;

                        int j = (seed / 8) & 0x7;
                        int i = seed & 0x7;

                        calcCheckSum[0] = (char)(j + 33 + 32);
                        calcCheckSum[1] = (char)(i + 16 + 32);
                    }

                    String checksum = $"{calcCheckSum[0]}{calcCheckSum[1]}";
                    result = inputCheckSum[0] == calcCheckSum[0] && inputCheckSum[1] == calcCheckSum[1];
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }
        public bool CheckIntegrity(OCRDevParameter ocrDev, LoaderBase.ActiveLotInfo activeLotInfo, String ocr)
        {
            // 다른 기타 OCR Integrity
            bool result = false;

            try
            {
                bool LotIntegrity = ocrDev.lotIntegrity.LotIntegrityEnable;

                LoggerManager.Debug($"[OCR] CheckIntegrity LotIntegrity: {LotIntegrity}");

                if (LotIntegrity == true)
                {
                    if(activeLotInfo == null || activeLotInfo.State == LotStateEnum.Idle)
                    {
                        //lot 중이 아님. lotIntegrity 확인할 필요 없음. 
                        result = true;
                        LoggerManager.Debug($"[OCR] CheckIntegrity TRUE activeLotInfo is nuill or Idle(cur: {activeLotInfo?.State}) State. Reading OCR : {ocr}");
                    }
                    else
                    {
                        //lot 중. 
                        if (string.IsNullOrEmpty(activeLotInfo.LotID) == false)
                        {
                            string Lotname = activeLotInfo.LotID.Substring(ocrDev.lotIntegrity.LotnameDigit, ocrDev.lotIntegrity.Lotnamelength);

                            if (ocr.Substring(ocrDev.lotIntegrity.LotnameDigit, ocrDev.lotIntegrity.Lotnamelength) == Lotname)
                            {
                                result = true;
                                LoggerManager.Debug($"[OCR] CheckIntegrity TRUE Lot ID: {activeLotInfo.LotID} ({Lotname}),  Reading OCR : {ocr}");
                            }
                            else
                            {
                                result = false;
                                LoggerManager.Debug($"[OCR] CheckIntegrity FALSE Lot ID: {activeLotInfo.LotID} ({Lotname}), Reading OCR : {ocr}");
                            }
                        }
                        else
                        {
                            result = false;
                            LoggerManager.Debug($"[OCR] CheckIntegrity FALSE LotID is null or empty, Reading OCR : {ocr}");
                        }
                    }                  
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }
        public int CheckSumStringToInteger(string charOCR)
        {
            int retVal = -1;
            try
            {
                #region OCR String -> Integer
                switch (charOCR)
                {
                    case "0":
                        retVal = 0;
                        break;
                    case "1":
                        retVal = 1;
                        break;
                    case "2":
                        retVal = 2;
                        break;
                    case "3":
                        retVal = 3;
                        break;
                    case "4":
                        retVal = 4;
                        break;
                    case "5":
                        retVal = 5;
                        break;
                    case "6":
                        retVal = 6;
                        break;
                    case "7":
                        retVal = 7;
                        break;
                    case "8":
                        retVal = 8;
                        break;
                    case "9":
                        retVal = 9;
                        break;
                    case "A":
                        retVal = 10;
                        break;
                    case "B":
                        retVal = 11;
                        break;
                    case "C":
                        retVal = 12;
                        break;
                    case "D":
                        retVal = 13;
                        break;
                    case "E":
                        retVal = 14;
                        break;
                    case "F":
                        retVal = 15;
                        break;
                    case "G":
                        retVal = 16;
                        break;
                    case "H":
                        retVal = 17;
                        break;
                    case "I":
                        retVal = 18;
                        break;
                    case "J":
                        retVal = 19;
                        break;
                    case "K":
                        retVal = 20;
                        break;
                    case "L":
                        retVal = 21;
                        break;
                    case "M":
                        retVal = 22;
                        break;
                    case "N":
                        retVal = 23;
                        break;
                    case "O":
                        retVal = 24;
                        break;
                    case "P":
                        retVal = 25;
                        break;
                    case "Q":
                        retVal = 26;
                        break;
                    case "R":
                        retVal = 27;
                        break;
                    case "S":
                        retVal = 28;
                        break;
                    case "T":
                        retVal = 29;
                        break;
                    case "U":
                        retVal = 30;
                        break;
                    case "W":
                        retVal = 31;
                        break;
                    case "X":
                        retVal = 32;
                        break;
                    case "Y":
                        retVal = 33;
                        break;
                    case "Z":
                        retVal = 34;
                        break;
                    default:
                        retVal = -1;
                        break;
                }
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public string CheckSumIntegerToString(int intOCR)
        {
            string retVal = null;
            try
            {
                #region OCR Integer -> String
                switch (intOCR)
                {
                    case 0:
                        retVal = "0";
                        break;
                    case 1:
                        retVal = "1";
                        break;
                    case 2:
                        retVal = "2";
                        break;
                    case 3:
                        retVal = "3";
                        break;
                    case 4:
                        retVal = "4";
                        break;
                    case 5:
                        retVal = "5";
                        break;
                    case 6:
                        retVal = "6";
                        break;
                    case 7:
                        retVal = "7";
                        break;
                    case 8:
                        retVal = "8";
                        break;
                    case 9:
                        retVal = "9";
                        break;
                    case 10:
                        retVal = "A";
                        break;
                    case 11:
                        retVal = "B";
                        break;
                    case 12:
                        retVal = "C";
                        break;
                    case 13:
                        retVal = "D";
                        break;
                    case 14:
                        retVal = "E";
                        break;
                    case 15:
                        retVal = "F";
                        break;
                    case 16:
                        retVal = "G";
                        break;
                    case 17:
                        retVal = "H";
                        break;
                    case 18:
                        retVal = "I";
                        break;
                    case 19:
                        retVal = "J";
                        break;
                    case 20:
                        retVal = "K";
                        break;
                    case 21:
                        retVal = "L";
                        break;
                    case 22:
                        retVal = "M";
                        break;
                    case 23:
                        retVal = "N";
                        break;
                    case 24:
                        retVal = "O";
                        break;
                    case 25:
                        retVal = "P";
                        break;
                    case 26:
                        retVal = "Q";
                        break;
                    case 27:
                        retVal = "R";
                        break;
                    case 28:
                        retVal = "S";
                        break;
                    case 29:
                        retVal = "T";
                        break;
                    case 30:
                        retVal = "U";
                        break;
                    case 31:
                        retVal = "W";
                        break;
                    case 32:
                        retVal = "X";
                        break;
                    case 33:
                        retVal = "Y";
                        break;
                    case 34:
                        retVal = "Z";
                        break;
                    default:
                        retVal = null;
                        break;
                }
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public int GetCheckDotMat()
        {
            int retVal = -1;
            try
            {
                retVal = checkDotMat;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}
