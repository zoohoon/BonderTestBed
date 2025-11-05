using System.Collections.Generic;
using System.Threading.Tasks;
using LoaderBase.Communication;
using ProberErrorCode;
using ProberInterfaces.CardChange;
using ProberInterfaces.Foup;
using ProberInterfaces.Loader;
using ProberInterfaces.Param;
using ProberInterfaces.Monitoring;
using LogModule;
using ProberInterfaces.Enum;

namespace ProberInterfaces.LoaderController
{
    public interface ILoaderController : IStateModule
    {
        bool LotEndCassetteFlag { get; set; }
        bool IsCancel { get; set; }
        bool IsAbort { get; }
        bool IsLotOut { get; set; }

        EventCodeEnum LoaderSystemInit();
        void RecoveryWithMotionInit();
        EventCodeEnum WaitForCommandDone();
        void SetLotEndFlag(bool flag);
        void OnFoupModuleStateChanged(FoupModuleInfo info);

        void RaiseWaferOutDetected(int foupNumber);
        Autofac.IContainer GetLoaderContainer();
        bool IsFoupUsingByLoader(int foupNumber);
        EventCodeEnum OFR_SetOcrID(string inputOCR);
        EventCodeEnum OFR_OCRRemoteEnd();
        EventCodeEnum OFR_OCRFail();

        EventCodeEnum OFR_OCRAbort();


        EventCodeEnum SaveDeviceParam();
        EventCodeEnum SaveSystemParam();
        EnumSubsStatus UpdateCardStatus(out EnumWaferState cardState);
        ProberInterfaces.CardChange.EnumCardChangeType GetCardChangeType();
        EventCodeEnum SetWaferInfo(IWaferInfo waferInfo);
        Task<EventCodeEnum> ConnectLoaderService();
        ILoaderInfo LoaderInfoObj { get; set; }

        EventCodeEnum ResponseSystemInit(EventCodeEnum errorCode);
        void BroadcastLotState(bool isBuzzerOn);
        string GetLoaderIP();
        int GetChuckIndex();
        string GetChuckIndexString();
        bool GetconnectFlag();
        EventCodeEnum NotifyStageAlarm(EventCodeParam noticeCodeInfo);        
        void NotifyReasonOfError(string errmsg);
        EventCodeEnum UpdateSoakingInfo(SoakingInfo soakinfo);

        void UpdateLotVerifyInfo(int portnum, bool flag);
        void UpdateDownloadMapResult(bool flag);
        void UpdateTesterConnectedStatus(bool flag);

        void UpdateLotDataInfo(StageLotDataEnum type, string val);

        void UpdateStageMove(StageMoveInfo info);
        void SetTitleMessage(int cellno, string message, string foreground = "", string background = "");

        void SetDeviceName(int cellno, string deviceName);
        void SetDeviceLoadResult( bool result);
        EventCodeEnum UploadCardPatternImages(byte[] data, string filename, string devicename, string cardid);
        EnumWaferType GetActiveLotWaferType(string lotid);
        EventCodeEnum ODTPUpload(int stageindex, string filename);
        EventCodeEnum ResultMapUpload(int stageindex, string filename);
        EventCodeEnum ResultMapDownload(int stageindex, string filename);
        List<CardImageBuffer> DownloadCardPatternImages(string devicename,int downimgcnt, string cardid);
        EventCodeEnum UploadProbeCardInfo(ProberCardListParameter probeCard);
        ProberCardListParameter DownloadProbeCardInfo(string cardID);

        void SetProbingStart(bool isStart);

        byte[] GetBytesFoupObjects();

        string GetFoupNumberStr();
        void SetTransferError(bool isError);
        void SetLotLogMessage(string message,int idx, ModuleLogType ModuleType, StateLogType State);
        void SetParamLogMessage(string message, int idx);
        bool SetNoReadScanState(int cassetteNumber);
        void SetCardStatus(bool isExist, string id, bool isDocked = false);
        EventCodeEnum SetRecoveryMode(bool isRecovery);
        void SetStopBeforeProbingFlag(bool flag, int stageidx = 0);
        void SetStopAfterProbingFlag(bool flag, int stageidx = 0);
        void SetOnceStopBeforeProbingFlag(bool flag, int stageidx = 0);
        void SetOnceStopAfterProbingFlag(bool flag, int stageidx = 0);
        EventCodeEnum WriteWaitHandle(short value);
        EventCodeEnum WaitForHandle(short handle, long timeout = 60000);

        void SetStageLock(StageLockMode mode);

        bool GetStopBeforeProbingFlag();
        bool GetStopAfterProbingFlag();
        EnumModuleForcedState GetModuleForcedState(ModuleEnum m);
        void SetForcedDoneState();
        EventCodeEnum SetAbort(bool isAbort, bool isForced = false);

        void SetTCW_Mode(TCW_Mode tcw_Mode);

        EventCodeEnum IsShutterClose();

        void SetMonitoringBehavior(List<IMonitoringBehavior> monitoringBehaviors, int stageIdx);

        void ChangeTabIndex(TabControlEnum tabEnum);
        EventCodeEnum GetLoaderEmergency();
        EventCodeEnum ReserveErrorEnd(string ErrorMessage = "Pause by host(CELL ABORT TEST).");
        bool LotOPPause(bool isabort = false);
        EnumWaferSize GetTransferWaferSize();
        EventCodeEnum SetTransferWaferSize(EnumWaferSize waferSize);
        void LotCancelSoakingAbort(int stageindex);
        void UpdateLogUploadList(EnumUploadLogType type);
    }

    public interface ILoaderControllerParam
    {

    }

    public interface ILoaderMainVM
    {
        string SoakingType { get; set; }
        string SoakRemainTIme { get; set; }
        void UpdateStageState();
    }

    public enum CellIndexDirection
    {
        BOTTOM_AND_Right = 0,
        BOTTOM_AND_LEFT,
        TOP_AND_Right,
        TOP_AND_LEFT,
    }

    /// <summary>
    /// UI ���濡 ����, Enum�� ����� �������� ���� �� �ִ�.
    /// �����ڰ� �����ϴ� TabItem�� Index�� �����ϰ� ������ ����� ��.
    /// Dictionary�� �̿��Ͽ�, Init ��, ����ؾߵǴ� ���� �־���� �̿�����!
    /// </summary>
    public enum TabControlEnum
    {
        FOUP = 0,
        CELL,
        LOTOPTION,
        VISION,
        LOTSETTING,
        MONITORING
    }

    public enum CCSettingOPTabControlEnum
    {
        DOCKSEUQUENCE = 0,
        UNDOCKSEQUENCE = 1,
        PARAMETER = 2,
        MANUAL = 3
    }

    public interface ILoaderStageSummaryViewModel
    {
        string StageMoveState { get; set; }
        Task StopBeforeProbingCommandFunc(int index);
        Task StopAfterProbingCommandFunc(int index);
        void ChangeTabIndex(TabControlEnum TabEnum);
        IStageObject SelectedStage { get; }
    }

    public interface ILoaderStageSummaryViewModel_OPERA : ILoaderStageSummaryViewModel
    {
        string SoakingType { get; set; }
        string SoakRemainTIme { get; set; }
        string SoakZClearance { get; set; }
        new string StageMoveState { get; set; }
    }

    public interface ILoaderStageSummaryViewModel_GOP : ILoaderStageSummaryViewModel
    {
        bool TriggerForStartConfirm { get; set; }
    }
}
