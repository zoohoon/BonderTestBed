using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cognex.Controls
{
    using Cognex.Command;
    using Cognex.Controls.Core;
    using LoaderBase.AttachModules.ModuleInterfaces;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Runtime.InteropServices;

    /*
     * cognex.win32display.exe 를 제어함
     * 실제 구현은 CognexCore에 구현되어 있음
     */
    public class CognexProcessManager : ICognexProcessManager, IHasSysParameterizable, IHasDevParameterizable, IFactoryModule
    {
        public CognexProcessSysParameter CognexProcSysParam { get; set; }
        public CognexProcessDevParameter CognexProcDevParam { get; set; }
        public CognexCommandManager CognexCommandManager { get; set; }


        public List<String> Ocr { get; set; }
        public List<String> LastOcrResult { get; set; }
        public List<double> OcrScore { get; set; }
        public List<EnumCognexModuleState> HostRunning { get; set; }
        public int checkDotMat { get; set; }

        private ICognexProcessManager _Core;
        public CognexProcessManager()
        {
            try
            {
                CognexCommandManager = new CognexCommandManager();
                LoadSysParameter();
                LoadDevParameter();

                if (CognexProcDevParam.RunMode == EnumCognexRunMode.COGNEX)
                {
                    _Core = new CognexCore();
                }
                else
                {
                    _Core = new EmulCore();
                }




                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    Ocr = new List<String>();
                    LastOcrResult = new List<String>();
                    HostRunning = new List<EnumCognexModuleState>();
                    OcrScore = new List<double>();
                    for (int i = 0; i < CognexProcDevParam.CognexHostList.Count; ++i)
                    {
                        Ocr.Add(String.Empty);
                        LastOcrResult.Add(String.Empty);
                        OcrScore.Add(0);
                        HostRunning.Add(EnumCognexModuleState.IDLE);
                    }

                    _Core.HostRunning = HostRunning;
                    _Core.Ocr = Ocr;
                    _Core.OcrScore = OcrScore;
                }

                _Core.CognexCommandManager = CognexCommandManager;
                _Core.CognexProcSysParam = CognexProcSysParam;
                _Core.CognexProcDevParam = CognexProcDevParam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void LoadConfig()
        {
            EventCodeEnum loadDevResult = LoadDevParameter();
            if (loadDevResult != EventCodeEnum.NONE)
            {
                LoggerManager.Error("Load Cognex Device Param Error");
            }
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new CognexProcessSysParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";

                retVal = this.LoadParameter(ref tmpParam, typeof(CognexProcessSysParameter));

                if (retVal == EventCodeEnum.NONE)
                {
                    CognexProcSysParam = tmpParam as CognexProcessSysParameter;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }
        public void SetManualOCRState(int hostIdx, bool isSuccess)
        {
            _Core.SetManualOCRState(hostIdx, isSuccess);
        }
        public bool GetManualOCRState(int hostIdx)
        {
            return _Core.GetManualOCRState(hostIdx);
        }
        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = this.SaveParameter(CognexProcSysParam);

                if (retVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error(String.Format("[CognexProcess] Save system param: Serialize Error"));
                    return retVal;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }
        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new CognexProcessDevParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";

                retVal = this.LoadParameter(ref tmpParam, typeof(CognexProcessDevParameter));

                if (retVal == EventCodeEnum.NONE)
                {
                    CognexProcDevParam = tmpParam as CognexProcessDevParameter;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = this.SaveParameter(CognexProcDevParam);

                if (retVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error(String.Format("[CognexProcess] Save device param: Serialize Error"));
                    return retVal;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }

        public EventCodeEnum InitDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public void SaveConfig()
        {
            try
            {
                SaveSysParameter();
                SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return;
            }
        }
        public bool InitWindow(IntPtr clientWindowHandle)
        {
            try
            {
                return _Core.InitWindow(clientWindowHandle);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }
        public void WndProc(int msg, IntPtr lParam)
        {
            try
            {
                _Core.WndProc(msg, lParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return;
            }
        }
        public void OnSizeChanged(EventArgs e, int clientWindowWidth, int clientWindowHeight)
        {
            try
            {
                _Core.OnSizeChanged(e, clientWindowWidth, clientWindowHeight);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return;
            }
        }
        public Task<bool> ConnectDisplay(string host)
        {
            try
            {
                if (_Core.IsInit() == false)
                {
                    Task<bool> t = new Task<bool>(() =>
                    {
                        return false;
                    });

                    t.Start();

                    return t;
                }

                return _Core.ConnectDisplay(host);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return Task.FromResult<bool>(false);
            }
        }

        public Task<bool> ConnectNative(string host)
        {
            try
            {
                if (_Core.IsInit() == false)
                {
                    Task<bool> t = new Task<bool>(() =>
                    {
                        return false;
                    });

                    t.Start();

                    return t;
                }

                return _Core.ConnectNative(host);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return Task.FromResult<bool>(false);
            }
        }

        public void DisconnectDisplay()
        {
            try
            {
                if (_Core.IsInit() == false)
                    return;

                _Core.DisconnectDisplay();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void DisconnectNative()
        {
            try
            {
                if (_Core.IsInit() == false)
                    return;

                _Core.DisconnectNative();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SwitchOnlineMode()
        {
            try
            {
                if (_Core.IsInit() == false)
                    return;

                _Core.SwitchOnlineMode();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SwitchLiveMode()
        {
            try
            {
                if (_Core.IsInit() == false)
                    return;

                _Core.SwitchLiveMode();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SwitchGraphics(bool flag)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return;

                _Core.SwitchGraphics(flag);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SwitchSpreadSwitch()
        {
            try
            {
                if (_Core.IsInit() == false)
                    return;

                _Core.SwitchSpreadSwitch();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public String GetOCRString(string host, bool adjust, out String score)
        {
            score = string.Empty;
            try
            {                
                if (_Core.IsInit() == false)
                    return String.Empty;

                return _Core.GetOCRString(host, adjust, out score);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return String.Empty;
            }
        }

        public bool DO_GetConfigEx(string host)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_GetConfigEx(host);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_AcquireConfig(string host)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_AcquireConfig(host);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_SetConfigRegion(string host, string y, string x, string height, string width, string theta, string phi)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_SetConfigRegion(host, y, x, height, width, theta, phi);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_SetConfigCharSize(string host, string height, string width)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_SetConfigCharSize(host, height, width);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_SetConfigLightPower(string host, string lightPower)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_SetConfigLightPower(host, lightPower);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public void Async_SetConfigLightPower(string host, string lightPower, Func<bool> func)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return;
                _Core.Async_SetConfigLightPower(host, lightPower, func);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool DO_SetConfigLightMode(string host, string mode)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_SetConfigLightMode(host, mode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_TuneConfigEx(string host, string flag, bool filter, bool light, bool size)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_TuneConfigEx(host, flag, filter, light, size);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_GetConfigTune(string host)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_GetConfigTune(host);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_GetFilterOperationList(string host)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_GetFilterOperationList(host);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_SetCustomFilterName(string host, string customFilterName)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_SetCustomFilterName(host, customFilterName);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_RemoveFilterOperationAll(string host)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_RemoveFilterOperationAll(host);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_InsertFilterOperation(string host, params string[] args)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_InsertFilterOperation(host, args);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_SetConfigChecksum(string host, string checksum)
        {
            try
            {
                bool retVal = false;
                if (_Core.IsInit() == false)
                {
                    retVal = false;
                    return retVal;
                }
                retVal = _Core.DO_SetConfigChecksum(host, checksum);
                if (retVal == false)
                {
                    this.NotifyManager().Notify(EventCodeEnum.OCR_CHECKSUM_FAIL);
                }
                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_SetConfigRetry(string host, string mode)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_SetConfigRetry(host, mode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_SetConfigMark(string host, string mark)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_SetConfigMark(host, mark);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_SetConfigOrientation(string host, string orientation)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_SetConfigOrientation(host, orientation);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_ReadConfig(string host, string flag)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_ReadConfig(host, flag);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_SetConfigFieldString(string host, string fieldString)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_SetConfigFieldString(host, fieldString);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool DO_RI(string host)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.DO_RI(host);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }
        public bool IsInit()
        {
            try
            {
                return _Core.IsInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }
        public bool SaveOCRImage(String host)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.SaveOCRImage(host);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }
        public EventCodeEnum DoOCRStringCatch(int hostIndex, bool adjust)
        {
            try
            {
                return _Core.DoOCRStringCatch(hostIndex, adjust);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.UNDEFINED;
            }
        }
        public EventCodeEnum DoOCRStringCatch(int hostIndex, bool adjust, OCRDevParameter ocrDev)
        {
            try
            {
                return _Core.DoOCRStringCatch(hostIndex, adjust, ocrDev);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.UNDEFINED;
            }
        }
        public EventCodeEnum DoOCRStringCatch(int hostIndex, bool adjust, OCRDevParameter ocrDev, LoaderBase.ActiveLotInfo activeLotInfo)
        {
            try
            {
                return _Core.DoOCRStringCatch(hostIndex, adjust, ocrDev, activeLotInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.UNDEFINED;
            }
        }
        
        public Task<EventCodeEnum> DoOCRStringCatchAsync(int hostIndex, bool adjust)
        {
            try
            {
                return _Core.DoOCRStringCatchAsync(hostIndex, adjust);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.UNDEFINED);
            }
        }
        public Task<EventCodeEnum> DoOCRStringCatchAsync(int hostIndex, bool adjust, OCRDevParameter ocrDev)
        {
            try
            {
                return _Core.DoOCRStringCatchAsync(hostIndex, adjust, ocrDev);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.UNDEFINED);
            }
        }
        public EventCodeEnum WaitForOCR(int hostIndex, int timeOut)
        {
            try
            {
                return _Core.WaitForOCR(hostIndex, timeOut);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.UNDEFINED;
            }
        }
        public EventCodeEnum GetOCRString(int hostIndex, bool adjust, out String ocr, out double ocrScore)
        {
            ocr = String.Empty;
            ocrScore = 0;

            try
            {
                return _Core.GetOCRString(hostIndex, adjust, out ocr, out ocrScore);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.UNDEFINED;
            }
        }
        public EventCodeEnum GetOCRString(int hostIndex, bool adjust, out String ocr, out double ocrScore, OCRDevParameter ocrDev)
        {
            ocr = String.Empty;
            ocrScore = 0;

            try
            {
                return _Core.GetOCRString(hostIndex, adjust, out ocr, out ocrScore, ocrDev);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.UNDEFINED;
            }
        }
        public EventCodeEnum GetOCRString(int hostIndex, bool adjust, out string ocr, out double ocrScore, OCRDevParameter ocrDev, LoaderBase.ActiveLotInfo activeLotInfo)
        {
            ocr = String.Empty;
            ocrScore = 0;

            try
            {
                return _Core.GetOCRString(hostIndex, adjust, out ocr, out ocrScore, ocrDev, activeLotInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.UNDEFINED;
            }
        }
        
        public void Dispose()
        {
            try
            {
                if (_Core.IsInit() == false)
                    return;

                _Core.Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool SaveOCRImage(int hostIndex)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.SaveOCRImage(hostIndex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public string CalOcrChecksum(string ocrString)
        {
            string retVal = null;
            try
            {
                if (_Core.IsInit() == false)
                    return retVal;

                return _Core.CalOcrChecksum(ocrString);
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
                if (_Core.IsInit() == false)
                    return retVal;

                retVal = _Core.GetCheckDotMat();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public bool SEMIChecksum(String ocr)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.SEMIChecksum(ocr);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }
        public bool CheckIntegrity(OCRDevParameter ocrDev, LoaderBase.ActiveLotInfo activeLotInfo, String ocr)
        {
            try
            {
                if (_Core.IsInit() == false)
                    return false;

                return _Core.CheckIntegrity(ocrDev, activeLotInfo,ocr);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

    }
    //==> cognex.win32display.exe 와 통신하면서 사용되는 데이터그램
    public struct COPYDATASTRUCT
    {
        public IntPtr dwData;
        public int cbData;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpData;
    }
    [Flags]
    public enum SendMessageTimeoutFlags : uint
    {
        SMTO_NORMAL = 0x0,
        SMTO_BLOCK = 0x1,
        SMTO_ABORTIFHUNG = 0x2,
        SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
        SMTO_ERRORONEXIT = 0x20
    }

}