using LoaderBase.Communication;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using ProberInterfaces.Loader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace LoaderParameters.Data
{
    [Serializable, DataContract]
    public enum FoupProcessingStateEnum
    {
        [EnumMember]
        IDLE,
        [EnumMember]
        SubReady,
        [EnumMember]
        Processing,
        [EnumMember]
        Processed
    }

    public enum DynamicFoupStateEnum
    {
        LOAD_AND_UNLOAD = 0,
        UNLOAD = 1,

    }

    //public enum SelectObjectTypeEnum
    //{
    //    FOUP,
    //    FIXEDTRAY,
    //    INSPECTIONTRAY,
    //    ALL
    //}

    [Serializable, DataContract]
    public class WrappedCasseteInfos : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<FoupObject> _Foups = new ObservableCollection<FoupObject>();
        [DataMember]
        public ObservableCollection<FoupObject> Foups
        {
            get { return _Foups; }
            set
            {
                if (value != _Foups)
                {
                    _Foups = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable, DataContract]
    public class CurrentLotJobObject : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Header;
        [DataMember]
        public string Header
        {
            get { return _Header; }
            set
            {
                if (value != _Header)
                {
                    _Header = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _FoupNum;
        [DataMember]
        public string FoupNum
        {
            get { return _FoupNum; }
            set
            {
                if (value!= _FoupNum)
                {
                    _FoupNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Index;
        [DataMember]
        public int Index
        {
            get { return _Index; }
            set { _Index = value; }
        }

        public CurrentLotJobObject()
        {

        }
        public CurrentLotJobObject(int CurrentLotindex)
        {
            this.Index = CurrentLotindex;
          
            this.Header = $"Lot #{this.Index + 1}";
            this.FoupNum = $"FOUP #{this.Index + 1}";
        }

    }

    [Serializable, DataContract]
    public class FoupObject : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Name;
        [DataMember]
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FoupProcessingStateEnum _ProcessingState;
        [DataMember]
        public FoupProcessingStateEnum ProcessingState
        {
            get { return _ProcessingState; }
            set
            {
                if (value != _ProcessingState)
                {
                    _ProcessingState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FoupStateEnum _State;    
          
        [DataMember]
        public FoupStateEnum State
        {
            get { return _State; }
            set
            {
                if (value != _State)
                {
                    _State = value;
                    RaisePropertyChanged();
                }
            }
        }


        private CassetteScanStateEnum _ScanState;
        [DataMember]
        public CassetteScanStateEnum ScanState
        {
            get { return _ScanState; }
            set
            {
                if (value != _ScanState)
                {
                    _ScanState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<SlotObject> _Slots = new ObservableCollection<SlotObject>();
        [DataMember]
        public ObservableCollection<SlotObject> Slots
        {
            get { return _Slots; }
            set
            {
                if (value != _Slots)
                {
                    _Slots = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnableTransfer = true;
        [DataMember]
        public bool IsEnableTransfer
        {
            get { return _IsEnableTransfer; }
            set
            {
                if (value != _IsEnableTransfer)
                {
                    _IsEnableTransfer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Index;
        [DataMember]
        public int Index
        {
            get { return _Index; }
            set { _Index = value; }
        }

        [DataMember]
        public bool _PreIsSelected { get; set; } = false;
        
        private bool _IsSelected = false;
        [DataMember]
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _PreIsSelected = _IsSelected;
                    _IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsLotEnd = false;
        [DataMember]
        public bool IsLotEnd
        {
            get { return _IsLotEnd; }
            set
            {
                if (value != _IsLotEnd)
                {
                    _IsLotEnd = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsLotEndEnable = false;
        [DataMember]
        public bool IsLotEndEnable
        {
            get { return _IsLotEndEnable; }
            set
            {
                if (value != _IsLotEndEnable)
                {
                    _IsLotEndEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private LotStateEnum _LotState;
        [DataMember]
        public LotStateEnum LotState
        {
            get { return _LotState; }
            set
            {
                if (value != _LotState)
                {
                    _LotState = value;
                    if(_LotState==LotStateEnum.Running)
                    {
                        IsLotEndEnable = true;
                        IsLotEnd = false;
                    }
                    else
                    {
                        IsLotEndEnable = false;
                        IsLotEnd = false;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private string _AllocatedCellInfo;
        [DataMember]
        public string AllocatedCellInfo
        {
            get { return _AllocatedCellInfo; }
            set
            {
                if (value != _AllocatedCellInfo)
                {
                    _AllocatedCellInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IStagelotSetting> _LotSettings;
        [DataMember]
        public ObservableCollection<IStagelotSetting> LotSettings
        {
            get { return _LotSettings; }
            set
            {
                if (value != _LotSettings)
                {
                    _LotSettings = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _LotPriority;
        [DataMember]
        public int LotPriority
        {
            get { return _LotPriority; }
            set
            {
                if (value != _LotPriority)
                {
                    _LotPriority = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _PreLotID = string.Empty;
        [DataMember]
        public string PreLotID
        {
            get { return _PreLotID; }
            set
            {
                if (value != _PreLotID)
                {
                    _PreLotID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _DoPMICount;
        [DataMember]
        public int DoPMICount
        {
            get { return _DoPMICount; }
            set
            {
                if (value != _DoPMICount)
                {
                    _DoPMICount = value;
                    RaisePropertyChanged();
                }
            }
        }
        private DateTime _LotStartTime;
        public DateTime LotStartTime
        {
            get { return _LotStartTime; }
            set
            {
                if (value != _LotStartTime)
                {
                    _LotStartTime = value;
                    LotStartTimeStr = _LotStartTime.ToString(@"MM/dd/yyyy hh:mm:ss tt", new CultureInfo("en-US"));
                    RaisePropertyChanged();
                }
            }
        }

        private string _LotStartTimeStr;
        public string LotStartTimeStr
        {
            get { return _LotStartTimeStr; }
            set
            {
                if (value != _LotStartTimeStr)
                {
                    _LotStartTimeStr = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime _LotEndTime;
        public DateTime LotEndTime
        {
            get { return _LotEndTime; }
            set
            {
                if (value != _LotEndTime)
                {
                    _LotEndTime = value;
                    if (_LotEndTime.Equals(default(DateTime)))
                    {
                        LotEndTimeStr = "";
                    }
                    else
                    {
                        LotEndTimeStr = _LotEndTime.ToString(@"MM/dd/yyyy hh:mm:ss tt", new CultureInfo("en-US"));
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private string _LotEndTimeStr;
        public string LotEndTimeStr
        {
            get { return _LotEndTimeStr; }
            set
            {
                if (value != _LotEndTimeStr)
                {
                    _LotEndTimeStr = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _NotchAngle;
        public double NotchAngle
        {
            get { return _NotchAngle; }
            set
            {
                if (value != _NotchAngle)
                {
                    _NotchAngle = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _EnableLotSetting;
        [DataMember]
        public bool EnableLotSetting
        {
            get { return _EnableLotSetting; }
            set
            {
                if (value != _EnableLotSetting)
                {
                    _EnableLotSetting = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DynamicFoupStateEnum _DynamicFoupState;
        [DataMember]
        public DynamicFoupStateEnum DynamicFoupState
        {
            get { return _DynamicFoupState; }
            set
            {
                if (value != _DynamicFoupState)
                {
                    _DynamicFoupState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CassetteID;
        [DataMember]
        public string CassetteID
        {
            get { return _CassetteID; }
            set
            {
                if (value != _CassetteID)
                {
                    _CassetteID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Enable;
        [DataMember]
        public bool Enable
        {
            get { return _Enable; }
            set
            {
                if (value != _Enable)
                {
                    _Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CassetteTypeEnum _CassetteType;
        [DataMember]
        public CassetteTypeEnum CassetteType
        {
            get { return _CassetteType; }
            set
            {
                if (value != _CassetteType)
                {
                    _CassetteType = value;
                    RaisePropertyChanged();
                }
            }
        }


        public FoupObject()
        {

        }

        private FoupModuleInfo _ModuleInfo;
        public FoupModuleInfo ModuleInfo
        {
            get { return _ModuleInfo; }
            set
            {
                if (value != _ModuleInfo)
                {
                    _ModuleInfo = value;
                    RaisePropertyChanged();
                }
            }
        }


        public FoupObject(int foupindex)
        {
            this.Index = foupindex;
            this.ProcessingState = FoupProcessingStateEnum.IDLE;

            this.Name = $"FOUP #{this.Index + 1}";            
            //this.LotTab = $"Foup #{this.Index + 1}";
            this.EnableLotSetting = true;
        }
    }

    [Serializable, DataContract]
    public class SlotObject : WaferSupplyMappingInfo, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized]
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _FoupNumber;
        [DataMember]
        public int FoupNumber
        {
            get { return _FoupNumber; }
            set
            {
                if (value != _FoupNumber)
                {
                    _FoupNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Name;
        [DataMember]
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumSubsStatus _WaferStatus;
        [DataMember]
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

        private EnumWaferState _WaferState;
        [DataMember]
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


        private bool _IsEnableTransfer = true;
        [DataMember]
        public bool IsEnableTransfer
        {
            get { return _IsEnableTransfer; }
            set
            {
                if (value != _IsEnableTransfer)
                {
                    _IsEnableTransfer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSelected = false;
        [DataMember]
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsPreSelected = false;
        [DataMember]
        public bool IsPreSelected
        {
            get { return _IsPreSelected; }
            set
            {
                if (value != _IsPreSelected)
                {
                    _IsPreSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Index;
        [DataMember]
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TransferObject _WaferObj;
        [DataMember]
        public TransferObject WaferObj
        {
            get { return _WaferObj; }
            set
            {
                if (value != _WaferObj)
                {
                    _WaferObj = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SlotObject()
        {
            this.IsPreSelected = false;
        }

        public SlotObject(WaferSupplyMappingInfo mappingInfo)
        {
            this.WaferSupplyInfo = mappingInfo.WaferSupplyInfo;
            this.DeviceInfo = mappingInfo.DeviceInfo;

            var substring = "SLOT";
            int CassetteNum = 25;

            string slotid = WaferSupplyInfo.ID.ToString();

            int slotindex = Convert.ToInt32(slotid.Substring(substring.Count()));

            this.FoupNumber = ((slotindex - 1) / CassetteNum) + 1;

            slotindex = ((slotindex - 1) % CassetteNum) + 1;

            Name = $"SLOT #{slotindex}";
            WaferStatus = EnumSubsStatus.NOT_EXIST;
            Index = slotindex;
            
            this.IsPreSelected = false;
        }

        public SlotObject(int index)
        {
            Name = $"SLOT #{index}";
            WaferStatus = EnumSubsStatus.NOT_EXIST;
            Index = index;
        }

        public void CopyTo(SlotObject slotObject)
        {
            slotObject.FoupNumber = this.FoupNumber;
            slotObject.Name = this.Name;
            slotObject.WaferStatus = this.WaferStatus;
            slotObject.WaferState = this.WaferState;
            slotObject.IsEnableTransfer = this.IsEnableTransfer;
            slotObject.IsSelected = this.IsSelected;
            slotObject.IsPreSelected = this.IsPreSelected;
            slotObject.Index = this.Index;
            this.WaferObj.Copy(slotObject.WaferObj);
        }
    }

    public class FixedTrayObject : WaferSupplyMappingInfo, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private bool _IsEnableTransfer = true;
        public bool IsEnableTransfer
        {
            get { return _IsEnableTransfer; }
            set
            {
                if (value != _IsEnableTransfer)
                {
                    _IsEnableTransfer = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _IsSelected = false;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }
        private TransferObject _WaferObj;
        public TransferObject WaferObj
        {
            get { return _WaferObj; }
            set
            {
                if (value != _WaferObj)
                {
                    _WaferObj = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CanUseBuffer;
        public bool CanUseBuffer
        {
            get { return _CanUseBuffer; }
            set
            {
                if (value != _CanUseBuffer)
                {
                    _CanUseBuffer = value;
                    RaisePropertyChanged();
                }
            }
        }

        public FixedTrayObject(WaferSupplyMappingInfo mappingInfo)
        {
            this.WaferSupplyInfo = mappingInfo.WaferSupplyInfo;
            this.DeviceInfo = mappingInfo.DeviceInfo;
            this.Name = WaferSupplyInfo.ID.ToString();
        }
    }

    public class InspectionTrayObject : WaferSupplyMappingInfo, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private bool _IsEnableTransfer = true;
        public bool IsEnableTransfer
        {
            get { return _IsEnableTransfer; }
            set
            {
                if (value != _IsEnableTransfer)
                {
                    _IsEnableTransfer = value;
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

        private bool _IsSelected = false;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }
        private TransferObject _WaferObj;
        public TransferObject WaferObj
        {
            get { return _WaferObj; }
            set
            {
                if (value != _WaferObj)
                {
                    _WaferObj = value;
                    RaisePropertyChanged();
                }
            }
        }

        public InspectionTrayObject(WaferSupplyMappingInfo mappingInfo)
        {
            this.WaferSupplyInfo = mappingInfo.WaferSupplyInfo;
            this.DeviceInfo = mappingInfo.DeviceInfo;

            this.Name = WaferSupplyInfo.ID.ToString();
        }
    }

    [Serializable, DataContract]
    public class TrasnferObjectSet : INotifyPropertyChanged, ITrasnferObjectSet
    {
        #region ==> PropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<FoupObject> _Foups = new ObservableCollection<FoupObject>();
        [DataMember]
        public ObservableCollection<FoupObject> Foups
        {
            get { return _Foups; }
            set
            {
                if (value != _Foups)
                {
                    _Foups = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<FixedTrayObject> _FixedTrays = new ObservableCollection<FixedTrayObject>();
        [DataMember]
        public ObservableCollection<FixedTrayObject> FixedTrays
        {
            get { return _FixedTrays; }
            set
            {
                if (value != _FixedTrays)
                {
                    _FixedTrays = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<InspectionTrayObject> _InspectionTrays = new ObservableCollection<InspectionTrayObject>();
        [DataMember]
        public ObservableCollection<InspectionTrayObject> InspectionTrays
        {
            get { return _InspectionTrays; }
            set
            {
                if (value != _InspectionTrays)
                {
                    _InspectionTrays = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum UpdateInfo(IPolishWaferSourceInformation pwinfo)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // Update Fixed Tray 

                foreach (var fixedtray in FixedTrays)
                {
                    // 이미 Polish Wafer로 설정되어 있고, 이름이 같은 경우
                    if ((fixedtray.DeviceInfo.WaferType.Value == EnumWaferType.POLISH) &&
                        (fixedtray.DeviceInfo.PolishWaferInfo?.DefineName.Value == pwinfo.DefineName.Value)
                        )
                    {
                        pwinfo.CurrentAngle.Value = pwinfo.NotchAngle.Value;
                        pwinfo.TouchCount.Value = 0;

                        fixedtray.DeviceInfo.PolishWaferInfo.Copy(pwinfo);
                    }
                }

                // Update Inspection Tray

                foreach (var inspectiontray in InspectionTrays)
                {
                    // 이미 Polish Wafer로 설정되어 있고, 이름이 같은 경우
                    if ((inspectiontray.DeviceInfo.WaferType.Value == EnumWaferType.POLISH) &&
                        (inspectiontray.DeviceInfo.PolishWaferInfo?.DefineName.Value == pwinfo.DefineName.Value)
                        )
                    {
                        pwinfo.CurrentAngle.Value = pwinfo.NotchAngle.Value;
                        pwinfo.TouchCount.Value = 0;

                        inspectiontray.DeviceInfo.PolishWaferInfo.Copy(pwinfo);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void UpdateAssignedWaferTypeColor(AsyncObservableCollection<IPolishWaferSourceInformation> polishWaferSources)
        {
            try
            {
                foreach (var fixedTray in FixedTrays)
                {
                    if (fixedTray.DeviceInfo.PolishWaferInfo != null)
                    {
                        for (int i = 0; i < polishWaferSources.Count; i++)
                        {
                            if (fixedTray.DeviceInfo.PolishWaferInfo.DefineName.Value == polishWaferSources[i].DefineName.Value)
                            {                                
                                fixedTray.DeviceInfo.PolishWaferInfo.ClearData();
                                fixedTray.DeviceInfo.PolishWaferInfo.Copy(polishWaferSources[i]);                                
                            }
                        }
                    }
                }
                foreach (var inspectionTray in InspectionTrays)
                {
                    if (inspectionTray.DeviceInfo.PolishWaferInfo != null)
                    {
                        for (int i = 0; i < polishWaferSources.Count; i++)
                        {
                            if (inspectionTray.DeviceInfo.PolishWaferInfo.DefineName.Value == polishWaferSources[i].DefineName.Value)
                            {                                
                                inspectionTray.DeviceInfo.PolishWaferInfo.ClearData();                                
                                inspectionTray.DeviceInfo.PolishWaferInfo.Copy(polishWaferSources[i]);                                
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void RemoveAssignedWaferType(IPolishWaferSourceInformation pwinfo)
        {
            try
            {
                foreach (var fixedTray in FixedTrays)
                {
                    if (fixedTray.DeviceInfo.PolishWaferInfo != null)
                    {
                        if (fixedTray.DeviceInfo.PolishWaferInfo.DefineName.Value == pwinfo.DefineName.Value)
                        {
                            fixedTray.DeviceInfo.ClearData();
                        }
                    }
                }
                foreach (var inspectionTray in InspectionTrays)
                {
                    if(inspectionTray.DeviceInfo.PolishWaferInfo != null)
                    {
                        if (inspectionTray.DeviceInfo.PolishWaferInfo.DefineName.Value == pwinfo.DefineName.Value)
                        {
                            inspectionTray.DeviceInfo.ClearData();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        // 1. Select된 오브젝트의 WaferType 변경
        // 2. Select된 오브젝트의 DeviceName 변경
        //public EventCodeEnum AssignWaferType(EnumWaferType type, string sourcename, SubstrateSizeEnum size)
        public EventCodeEnum AssignWaferType(EnumWaferType type, IPolishWaferSourceInformation pwinfo)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // Check Slot 
                foreach (var foup in Foups)
                {
                    foreach (var slot in foup.Slots)
                    {
                        if (slot.IsSelected == true)
                        {
                            slot.DeviceInfo.WaferType.Value = type;

                            if (type != EnumWaferType.UNDEFINED)
                            {
                                if (slot.DeviceInfo.PolishWaferInfo == null)
                                {
                                    slot.DeviceInfo.PolishWaferInfo = new PolishWaferInformation();
                                }

                                pwinfo.CurrentAngle.Value = pwinfo.NotchAngle.Value;
                                pwinfo.TouchCount.Value = 0;

                                slot.DeviceInfo.Size.Value = pwinfo.Size.Value;
                                slot.DeviceInfo.NotchType = pwinfo.NotchType.Value;
                                slot.DeviceInfo.PolishWaferInfo.Copy(pwinfo);
                            }
                            else
                            {
                                slot.DeviceInfo.ClearData();
                            }
                        }
                    }
                }

                // Check Fixed Tray 
                foreach (var fixedtray in FixedTrays)
                {
                    if (fixedtray.IsSelected == true)
                    {                        
                        if (type != EnumWaferType.UNDEFINED)
                        {
                            if(fixedtray.CanUseBuffer != true)
                            {
                                fixedtray.DeviceInfo.WaferType.Value = type;

                                if (fixedtray.DeviceInfo.PolishWaferInfo == null)
                                {
                                    fixedtray.DeviceInfo.PolishWaferInfo = new PolishWaferInformation();
                                }

                                pwinfo.CurrentAngle.Value = pwinfo.NotchAngle.Value;
                                pwinfo.TouchCount.Value = 0;

                                fixedtray.DeviceInfo.Size.Value = pwinfo.Size.Value;
                                fixedtray.DeviceInfo.NotchType = pwinfo.NotchType.Value;
                                fixedtray.DeviceInfo.PolishWaferInfo.Copy(pwinfo);
                            }
                            else
                            {
                                retval = EventCodeEnum.POLISHWAFER_ASSIGN_FAIL;
                            }
                        }
                        else
                        {                            
                            fixedtray.DeviceInfo.ClearData();
                        }
                    }
                }

                // Check Inspection Tray
                foreach (var inspectiontray in InspectionTrays)
                {
                    if (inspectiontray.IsSelected == true)
                    {
                        inspectiontray.DeviceInfo.WaferType.Value = type;

                        if (type != EnumWaferType.UNDEFINED)
                        {
                            if (inspectiontray.DeviceInfo.PolishWaferInfo == null)
                            {
                                inspectiontray.DeviceInfo.PolishWaferInfo = new PolishWaferInformation();
                            }

                            pwinfo.CurrentAngle.Value = pwinfo.NotchAngle.Value;
                            pwinfo.TouchCount.Value = 0;

                            inspectiontray.DeviceInfo.Size.Value = pwinfo.Size.Value;
                            inspectiontray.DeviceInfo.NotchType = pwinfo.NotchType.Value;
                            inspectiontray.DeviceInfo.PolishWaferInfo.Copy(pwinfo);
                        }
                        else
                        {
                            inspectiontray.DeviceInfo.ClearData();
                        }
                    }
                }
                if(retval == EventCodeEnum.POLISHWAFER_ASSIGN_FAIL)
                {
                    return retval;
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public List<IWaferSupplyMappingInfo> GetSelectedModulesList()
        {
            List<IWaferSupplyMappingInfo> retval = new List<IWaferSupplyMappingInfo>();
            try
            {
                foreach (var foup in Foups)
                {
                    foreach (var slot in foup.Slots)
                    {
                        if (slot.IsSelected == true)
                        {
                            retval.Add(slot);
                        }
                    }
                }

                foreach (var fixedtray in FixedTrays)
                {
                    if (fixedtray.IsSelected == true)
                    {
                        retval.Add(fixedtray);
                    }
                }

                foreach (var inspectiontray in InspectionTrays)
                {
                    if (inspectiontray.IsSelected == true)
                    {
                        retval.Add(inspectiontray);
                    }
                }

                if (retval.Count == 0) 
                {
                    retval = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public List<IWaferSupplyMappingInfo> GetDefineNameModulesList(string PWDefineName)
        {
            List<IWaferSupplyMappingInfo> retval = new List<IWaferSupplyMappingInfo>();
            try
            {
                foreach (var foup in Foups)
                {
                    foreach (var slot in foup.Slots)
                    {
                        if (slot.DeviceInfo.PolishWaferInfo != null && slot.DeviceInfo.PolishWaferInfo.DefineName.Value == PWDefineName)
                        {
                            retval.Add(slot);
                        }
                    }
                }

                foreach (var fixedtray in FixedTrays)
                {
                    if (fixedtray.DeviceInfo.PolishWaferInfo != null && fixedtray.DeviceInfo.PolishWaferInfo.DefineName.Value == PWDefineName)
                    {
                        retval.Add(fixedtray);
                    }
                }

                foreach (var inspectiontray in InspectionTrays)
                {
                    if (inspectiontray.DeviceInfo.PolishWaferInfo != null && inspectiontray.DeviceInfo.PolishWaferInfo.DefineName.Value == PWDefineName)
                    {
                        retval.Add(inspectiontray);
                    }
                }

                if (retval.Count == 0)
                {
                    retval = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public List<IWaferSupplyMappingInfo> GetAssignedMappingInfo(EnumWaferType type, string sourcename)
        {
            List<IWaferSupplyMappingInfo> retval = new List<IWaferSupplyMappingInfo>();

            try
            {
                // Check Slot 

                foreach (var foup in Foups)
                {
                    foreach (var slot in foup.Slots)
                    {
                        if ((slot.DeviceInfo.WaferType.Value == type) && (slot.DeviceInfo.DeviceName.Value == sourcename))
                        {
                            retval.Add(slot);
                        }
                    }
                }

                // Check Fixed Tray 

                foreach (var fixedtray in FixedTrays)
                {
                    if ((fixedtray.DeviceInfo.WaferType.Value == type) && (fixedtray.DeviceInfo.DeviceName.Value == sourcename))
                    {
                        retval.Add(fixedtray);
                    }
                }

                // Check Inspection Tray

                foreach (var inspectiontray in InspectionTrays)
                {
                    if ((inspectiontray.DeviceInfo.WaferType.Value == type) && (inspectiontray.DeviceInfo.DeviceName.Value == sourcename))
                    {
                        retval.Add(inspectiontray);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void UnSelectAll()
        {
            foreach (var foup in this.Foups)
            {
                foreach (var slot in foup.Slots)
                {
                    slot.IsSelected = false;
                }
            }

            foreach (var fixedtray in this.FixedTrays)
            {
                fixedtray.IsSelected = false;
            }

            foreach (var inspectiontray in this.InspectionTrays)
            {
                inspectiontray.IsSelected = false;
            }
        }

    }
}
