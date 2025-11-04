using Cognex.Command;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoaderBase.AttachModules.ModuleInterfaces
{
    using ProberInterfaces;
    using ProberErrorCode;

    public interface ICognexProcessManager : IDisposable
    {
        List<String> Ocr { get; set; }
        List<String> LastOcrResult { get; set; } 
        List<double> OcrScore { get; set; }
        List<EnumCognexModuleState> HostRunning { get; set; }
        CognexProcessSysParameter CognexProcSysParam { get; set; }
        CognexProcessDevParameter CognexProcDevParam { get; set; }
        CognexCommandManager CognexCommandManager { get; set; }
        Task<bool> ConnectDisplay(String host);
        Task<bool> ConnectNative(String host);
        void DisconnectDisplay();
        void DisconnectNative();
        void SwitchOnlineMode();
        void SwitchLiveMode();
        void SwitchGraphics(bool flag);
        void SwitchSpreadSwitch();
        bool InitWindow(IntPtr clientWindowHandle);
        void WndProc(int msg, IntPtr lParam);
        void OnSizeChanged(EventArgs e, int clientWindowWidth, int clientWindowHeight);

        String GetOCRString(String host, bool adjust, out String Score);
        EventCodeEnum GetOCRString(int hostIndex, bool adjust, out String ocr, out double ocrScore);
        EventCodeEnum GetOCRString(int hostIndex, bool adjust, out string ocr, out double ocrScore, OCRDevParameter ocrDev, ActiveLotInfo activeLotInfo);
        EventCodeEnum GetOCRString(int hostIndex, bool adjust, out string ocr, out double ocrScore, OCRDevParameter ocrDev);
        EventCodeEnum DoOCRStringCatch(int hostIndex, bool adjust);
        EventCodeEnum DoOCRStringCatch(int hostIndex, bool adjust, OCRDevParameter ocrDev);

        EventCodeEnum DoOCRStringCatch(int hostIndex, bool adjust, OCRDevParameter ocrDev, ActiveLotInfo activeLotInfo);
        Task<EventCodeEnum> DoOCRStringCatchAsync(int hostIndex, bool adjust);
        Task<EventCodeEnum> DoOCRStringCatchAsync(int hostIndex, bool adjust, OCRDevParameter ocrDev);
        EventCodeEnum WaitForOCR(int hostIndex, int timeOut);
        bool DO_GetConfigEx(String host);
        bool DO_AcquireConfig(String host);
        bool DO_SetConfigRegion(String host, String y, String x, String height, String width, String theta, String phi);
        bool DO_SetConfigCharSize(String host, String height, String width);
        bool DO_SetConfigLightPower(String host, String lightPower);
        //void Async_SetConfigLightPower(String host, String lightPower);
        void Async_SetConfigLightPower(String host, String lightPower, Func<bool> func);
        bool DO_SetConfigLightMode(String host, String mode);
        bool DO_TuneConfigEx(String host, String flag, bool filter, bool light, bool size);
        bool DO_GetConfigTune(String host);
        bool DO_GetFilterOperationList(String host);
        bool DO_SetCustomFilterName(String host, String customFilterName);
        bool DO_RemoveFilterOperationAll(String host);
        bool DO_InsertFilterOperation(String host, params String[] args);
        bool DO_SetConfigChecksum(String host, String checksum);
        bool DO_SetConfigRetry(string host, string mode);
        bool DO_SetConfigMark(String host, String mark);
        bool DO_SetConfigOrientation(String host, String orientation);
        bool DO_ReadConfig(String host, String flag);
        bool DO_SetConfigFieldString(String host, String fieldString);
        bool DO_RI(String host);
        bool SaveOCRImage(String host);
        bool SaveOCRImage(int hostIndex);
        bool IsInit();
        void SaveConfig();
        void LoadConfig();
        void SetManualOCRState(int hostIdx, bool isSuccess);
        bool GetManualOCRState(int hostIdx);
        string CalOcrChecksum(string ocrString);
        int GetCheckDotMat();
        bool SEMIChecksum(String ocr);
        bool CheckIntegrity(OCRDevParameter ocrDev, LoaderBase.ActiveLotInfo activeLotInfo, String ocr);
    }
}