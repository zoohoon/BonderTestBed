using System;
using System.Collections.Generic;
using System.Windows;

namespace LoaderBase.Communication
{
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using ProberInterfaces.Proxies;
    using LogModule;
    using LoaderParameters;
    using ProberInterfaces.Monitoring;
    using ProberInterfaces.Enum;

    public enum StageStateEnum
    {
        Not_Request,
        Requested,
    }
    public interface IStagelotSetting
    {
        int FoupNumber { get; set; }
        string LotName { get; set; }
        string OperatorName { get; set; }
        string RecipeName { get; set; }
        bool IsAssigned { get; set; }
        bool IsVerified { get; set; }
        int Index { get; set; }
        bool IsSelected { get; set; }
        LotModeEnum LotMode { get; set; }
        //bool bexist { get; set; }
        void Clear(bool UseLotProcessingVerify = false);
    }
    public interface IStageObject
    {
        StageStateEnum State { get; set; }
        int Index { get; set; }
        int LauncherIndex { get; set; }
        string Name { get; set; }
        string TargetName { get; set; }
        EnumSubsStatus WaferStatus { get; set; }
        EnumSubsStatus CardStatus { get; set; }
        ModuleStateEnum StageState { get; set; }
        int Progress { get; set; }
        bool IsEnableTransfer { get; set; }
        ICellInfo StageInfo { get; set; }
        GPCellModeEnum StageMode { get; set; }
        
        StageLockMode LockMode { get; set; }

        bool Reconnecting { get; set; }
        bool IsStageModeChanged { get; set; }
        StreamingModeEnum StreamingMode { get; set; }
        void Clear();
        TransferObject WaferObj { get; set; }
        TransferObject CardObj { get; set; }
        bool IsProbing { get; set; }
        DateTime ProbingTime { get; set; }
        String ProbingTimeStr { get; set; }
        bool StopBeforeProbing { get; set; }
        bool StopAfterProbing { get; set; }
        bool OnceStopBeforeProbing { get; set; }
        bool OnceStopAfterProbing { get; set; }
        bool isTransferError { get; set; }
        //IStagelotSetting LotSetting { get; set; }
        string PauseReason { get; set; }

        bool IsRecoveryMode { get; set; }

        EnumModuleForcedState ForcedDone { get; set; }
        DispFlipEnum DispHorFlip { get; set; }
        DispFlipEnum DispVerFlip { get; set; }
        bool ReverseManualMoveX { get; set; }
        bool ReverseManualMoveY { get; set; }

        long ChillingTimeMax { get; set; }

        long ChillingTime { get; set; }
        Visibility ChillingTimeProgressBar_Visibility { get; set; }
        TCW_Mode TCWMode { get; set; }
        List<IMonitoringBehavior> MonitoringBehaviorList { get; set; }
        bool IsWaferOnHandler { get; set; }
    }
    public interface ICellInfo
    {
        bool IsExcuteProgram { get; set; }
        //Dictionary<Type, ICommunicationObject> Proxies { get; }

        ProxyManager ProxyManager { get; set; }

        //Dictionary<Type, IProberProxy> Proxies { get; set; }
        //void CollectProxies();
        void DisConnectProxies();
        int Index { get; set; }
        long MapIndexX { get; set; }
        long MapIndexY { get; set; }
        bool IsConnectChecking { get; set; } //loader client connect or disconnect 중일때 true임
        bool IsConnected { get; set; }
        bool IsChecked { get; set; }
        string DeviceName { get; set; }
        bool DeviceLoadResult { get; set; }
        bool IsExcuteLot { get; set; }
        bool PreIsExcuteProgram { get; set; }
        void Clear();
        StageLotData LotData { get; set; }
        bool AlarmEnabled { get; set; }
        ObservableCollection<AlarmLogData> ErrorCodeAlarams { get; set; }
        //ObservableCollection<String> AlaramMessages { get; set; }
        int AlarmMessageNotNotifiedCount { get; set; }
        ObservableCollection<String> SetTitles { get; set; }
        string LastTitle { get; set; }
        bool GemConnState { get; set; }
        bool ChillerConnState { get; set; }
        bool EnableAutoConnect { get; set; }
        bool IsReservePause { get; set; }
        //bool IsOccurTempError { get; set; }
        double PV { get; set; }
        double DewPoint { get; set; }
        //bool IsVerified { get; set; }
        bool IsAvailableTesterConnect { get; set; }
        bool IsTesterConnected { get; set; }
        bool IsReceiveErrorEnd { get; set; }
        int WaferObjHashCode { get; set; }
        IWaferObject WaferObject { get; set; }
        bool IsMapDownloaded { get; set; }
        List<EnumUploadLogType> UploadLogList { get; set; }
    }
    public interface ILauncherObject
    {
        string Name { get; set; }
        int Index { get; set; }
        bool IsConnected { get; set; }
        bool IsChecked { get; set; }
        List<int> CellIndexs { get; set; }
        IMultiExecuterProxy MultiExecuterProxy { get; set; }
        ObservableCollection<ILauncherDiskObject> LauncherDiskObjectCollection { get; set; }
    }
    public interface ILauncherDiskObject
    {
        string DriveName { get; set; }
        string AvailableSpace { get; set; }
        string TotalSpace { get; set; }
        string UsageSpace { get; set; }
        string Percent { get; set; }
    }
}
