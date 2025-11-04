using LogModule;
using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace ProberInterfaces
{
    public class LotModeWithIndex
    {
        public int Index { get; set; }
        public LotModeEnum Lotmode { get; set; }
    }

    public enum LotModeEnum
    {
        [Description("UNDEFINED")]
        UNDEFINED = 0,
        [Description("Normal Probing(CP1)")]
        CP1,
        [Description("Multi-Pass Probing(CP2)")]
        MPP,
        [Description("Continue Probing")]
        CONTINUEPROBING
    }
    public enum ForcedLotModeEnum
    {
        UNDEFINED = 0,
        ForcedCP1,
        ForcedMPP,
    }

    public enum PROBINGMODE
    {
        EMUL = 0,
        ACTUAL = 1
    }
    public enum LOTSTATE
    {
        IDLE = 0,
        RUNNING = 1,
        PAUSE = 2
    }
    public enum CASSETTESTATE
    {
        READY = 0,
        PROCESSING = 1,
        PROCESSED = 2,
        PROBING = 3,
        EMPTY = 4
    }
    public enum WAFERSTATE
    {
        LOADED = 0,
        ALIGNING = 1,
        PROBING = 2,
        PROBINGDONE = 3,
        UNLOADING = 4
    }
    public enum DynamicModeEnum
    {
        NORMAL = 0,
        DYNAMIC = 1
    }
    public enum ResultMapDownloadTriggerEnum
    {
        [Description("NONE")]
        UNDIFINED,
        [Description("LOT NAME INPUT")]
        LOT_NAME_INPUT_TRIGGER,
        [Description("READ WAFER ID")]
        READ_WAFERID_TRIGGER,
    }

    /// <summary> 
    /// Host 를 통한 LOT 할당 상태
    /// UNASSIGNED : LOT 가 할당되지 않거나, 할당 해제 된 상태
    /// JOB_FINISHED : LOT 가 정상 종료 된 상태 --> LOT 종료시 CANCEL 상태면 JOB_FINISHED 으로 변경 하면 안됨.
    /// ASSIGNED : LOT가 할당 된 상태 
    /// CANCEL : LOT 가 취소 된 상태 
    /// CANCELED : LOT 가 취소된 후 UNLOAD 까지 된 상태
    /// POSTPONED : LOT 가 연기된 상태 (Loader 가 수동조작 되고 있는 경우 대기 상태)
    /// PROCESSING : (Cell) LOT 의 Wafer 가 Load 된 이후 상태. ( 양산 시작으로 본다 )
    /// </summary>
    public enum LotAssignStateEnum
    {
        UNASSIGNED,
        JOB_FINISHED,
        ASSIGNED,
        CANCEL,
        CANCELED,
        POSTPONED,
        PROCESSING
    }


    [Serializable, DataContract]
    public enum LotStateEnum
    {
        [EnumMember]
        Idle,
        [EnumMember]
        Running,
        [EnumMember]
        Pause,
        [EnumMember]
        Error,
        [EnumMember]
        Done,
        [EnumMember]
        End,
        [EnumMember]
        Cancel,
        [EnumMember]
        Suspend,
        [EnumMember]
        Abort
    }
    public interface ILotInfo : IFactoryModule
    {
        LotAccumulateInfo AccumulateInfo { get; set; }
        int LoadedWaferCountUntilBeforeLotStart { get; set; }
        int LoadedWaferCountUntilBeforeDeviceChange { get; set; }
        Element<string> LotName { get; set; }
        Element<string> OperatorID { get; set; }
        Element<string> DeviceName { get; set; }
        Element<int> FoupNumber { get; set; }
        string CSTHashCode { get; set;}
        //int CurrentWaferSlotNumber { get; }

        long TouchDownCount { get; set; }
        int ProcessedWaferCnt { get; set; }
        int ProcessedDieCnt { get; set; }
        bool LotStartTimeEnable { get; set; }
        DateTime LotStartTime { get; set; }
        bool LotEndTimeEnable { get; set; }
        DateTime LotEndTime { get; set; }
        ObservableCollection<WaferSummary> WaferSummarys { get; }
        bool StopAfterScanCSTFlag { get; set; }
        bool StopBeforeProbeFlag { get; set; }
        bool? ContinueLot { get; set; }
        bool isNewLot { get; set; }

        Element<LotModeEnum> LotMode { get; set; }
        //event EventHandler LotModeChanged;
        bool NeedLotDeallocated { get; set; }
        //int ProcessedDieCount();
        int ProcessWaferCount();
        int ProcessedWaferCount();
        int UnProcessedWaferCount();
        void UpdateWafer(IWaferObject waferObject);
        void UpdateWafer(WaferSummary waferSummary);
        bool IsLoadingWafer(int slotNum);
        void SetLotName(string lotName);
        void SetFoupInfo(int foupnumber, string cstHashCode);
        void SetWaferID(int slotNum, string waferID);
        string GetWaferID(int slotNum);
        void SetHolder(int slotNum, string holder);
        void SetWaferState(int slotNum, EnumSubsStatus waferStatus, EnumWaferState waferState);
        void WaferSwapChanged(int originSlotNum, int changeSlotNum, bool isInit = false);
        void ClearWaferSummary();
        void IncreaseTouchDownCount();
        ResultMapDownloadTriggerEnum ResultMapDownloadTrigger { get; set; }
        Element<DynamicModeEnum> DynamicMode { get; set; }

        List<StageLotInfo> GetLotInfos();
        void CreateLotInfo(int foupnumber, string recipeid = "", string lotid = "", bool isAssignLot = false, string cstHashCode = "", string carrierid = null);
        void RemoveLotInfo(int foupnumber, string lotid = "", string cstHashCode = "");
        void ClearLotInfos();
        void SetCassetteHashCode(int foupnumber, string lotid, string cstHashCode);
        void SetCassetteHashCode(string csthashcode);
        void SetLotStarted(int foupnumber, string lotid, string cstHashCode);
        void SetDevDownResult(bool result, int foupnumber, string deviceid, string lotid);
        void SetDevLoadResult(bool result, int foupnumber = 0, string deviceid = "", string lotid = "");
        bool GetDevResult(int foupnumber = 0, string lotid = "", bool getdownresult = false, bool getloadresult = false);

        string GetLotIDAtStageLotInfos(int foupnumber);
        int GetFoupNumbetAtStageLotInfos(string lotid);
        void SetStageLotAssignState(LotAssignStateEnum assignStateEnum, string lotid = "", string cstHashCode = "");
        LotAssignStateEnum GetStageLotAssignState(string lotid = "", string cstHashCode = "");
    }

    [Serializable]
    public class LotProcessingVerify : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _PortNumber;
        [DataMember]
        public int PortNumber
        {
            get { return _PortNumber; }
            set
            {
                if (value != _PortNumber)
                {
                    _PortNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _LotProcessingVerifed = new Element<bool>();
        [DataMember]
        public Element<bool> LotProcessingVerifed
        {
            get { return _LotProcessingVerifed; }
            set
            {
                if (value != _LotProcessingVerifed)
                {
                    _LotProcessingVerifed = value;
                    RaisePropertyChanged();
                }
            }
        }
    }


    public interface ISystemInfo
    {
        long WaferCount { get; }
        long DieCount { get; }
        long ProcessedWaferCountUntilBeforeCardChange { get; }
        long MarkedWaferCountLastPolishWaferCleaning { get; }
        long TouchDownCountUntilBeforeCardChange { get; }
        long MarkedTouchDownCountLastPolishWaferCleaning { get; }

        void IncreaseWaferCount();
        void IncreaseDieCount();

        void IncreaseProcessedWaferCountUntilBeforeCardChange();
        void ResetProcessedWaferCountUntilBeforeCardChange();
        void SetMarkedWaferCountLastPolishWaferCleaning();

        void IncreaseTouchDownCountUntilBeforeCardChange();
        void ResetTouchDownCountUntilBeforeCardChange();
        void SetMarkedTouchDownCountLastPolishWaferCleaning();

        EventCodeEnum LoadInfo();
        void SaveLotInfo();
    }

    public interface IDeviceInfo
    {
        double WaferCount { get; set; }
        double DieCount { get; set; }
    }

    public class WaferSummary : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        public WaferSummary()
        {
            try
            {
                ProbingStartTime = DateTime.MinValue;
                ProbingEndTime = DateTime.MinValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private int _SlotNumber;
        public int SlotNumber
        {
            get { return _SlotNumber; }
            set
            {
                if (value != _SlotNumber)
                {
                    _SlotNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _WaferID;
        public string WaferID
        {
            get { return _WaferID; }
            set
            {
                if (value != _WaferID)
                {
                    _WaferID = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _WaferHolder;
        public string WaferHolder
        {
            get { return _WaferHolder; }
            set
            {
                if (value != _WaferHolder)
                {
                    _WaferHolder = value;
                    RaisePropertyChanged();
                }
            }
        }


        private DateTime? _ProbingStartTime;
        public DateTime? ProbingStartTime
        {
            get { return _ProbingStartTime; }
            set
            {
                if (value != _ProbingStartTime)
                {
                    _ProbingStartTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime? _ProbingEndTime;
        public DateTime? ProbingEndTime
        {
            get { return _ProbingEndTime; }
            set
            {
                if (value != _ProbingEndTime)
                {
                    _ProbingEndTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _DoProceedProbing;
        public bool? DoProceedProbing
        {
            get { return _DoProceedProbing; }
            set
            {
                if (value != _DoProceedProbing)
                {
                    _DoProceedProbing = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumWaferState _WaferState;
        public EnumWaferState WaferState
        {
            get { return _WaferState; }
            set
            {
                if (value != _WaferState)
                {
                    _WaferState = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumSubsStatus _WaferStatus;
        public EnumSubsStatus WaferStatus
        {
            get { return _WaferStatus; }
            set
            {
                if (value != _WaferStatus)
                {
                    _WaferStatus = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _Yield;
        public double Yield
        {
            get { return _Yield; }
            set
            {
                if (value != _Yield)
                {
                    _Yield = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _RetestYield;
        public double RetestYield
        {
            get { return _RetestYield; }
            set
            {
                if (value != _RetestYield)
                {
                    _RetestYield = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class LotAccumulateInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private long _TotalFailedDieCount;
        public long TotalFailedDieCount
        {
            get { return _TotalFailedDieCount; }
            set
            {
                if (value != _TotalFailedDieCount)
                {
                    _TotalFailedDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _TotalTestedDieCount;
        public long TotalTestedDieCount
        {
            get { return _TotalTestedDieCount; }
            set
            {
                if (value != _TotalTestedDieCount)
                {
                    _TotalTestedDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _TotalPassedDieCount;
        public long TotalPassedDieCount
        {
            get { return _TotalPassedDieCount; }
            set
            {
                if (value != _TotalPassedDieCount)
                {
                    _TotalPassedDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _TotalYield;
        public double TotalYield
        {
            get { return _TotalYield; }
            set
            {
                if (value != _TotalYield)
                {
                    _TotalYield = value;

                    RaisePropertyChanged();
                }
            }
        }

        private double _TotalRetestYield;
        public double TotalRetestYield
        {
            get { return _TotalRetestYield; }
            set
            {
                if (value != _TotalRetestYield)
                {
                    _TotalRetestYield = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class LotInfoPack : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public PropertyChangedEventHandler propertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }
        #endregion

        private string _LotID;
        public string LotID
        {
            get { return _LotID; }
            set
            {
                if (value != _LotID)
                {
                    _LotID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _FoupIndex;
        public int FoupIndex
        {
            get { return _FoupIndex; }
            set
            {
                if (value != _FoupIndex)
                {
                    _FoupIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CassetteHashcode;
        public string CassetteHashcode
        {
            get { return _CassetteHashcode; }
            set
            {
                if (value != _CassetteHashcode)
                {
                    _CassetteHashcode = value;
                    RaisePropertyChanged();
                }
            }
        }

        public LotInfoPack()
        {

        }
        public LotInfoPack(string lotId, int foupIdx, string cstHashCode)
        {
            LotID = lotId;
            FoupIndex = foupIdx;
            CassetteHashcode = cstHashCode;
        }
    }


}
