
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubstrateObjects
{
    using Autofac;
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.PMI;
    using ProberInterfaces.State;
    using ProberInterfaces.WaferAlign;
    using ProbingDataInterface;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Xml.Serialization;
    using VirtualStageConnector;

    [Serializable]
    public class WaferObject : IWaferObject, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; }

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public object Owner { get; set; }

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public List<Object> Nodes { get; set; }

        #region NonSerialized


        [field: NonSerialized, JsonIgnore]
        private WaferObjectDelegate.ChangeMapIndexDelegate _ChangeMapIndexDelegate;
        [XmlIgnore, JsonIgnore]
        public WaferObjectDelegate.ChangeMapIndexDelegate ChangeMapIndexDelegate
        {
            get { return _ChangeMapIndexDelegate; }
            set
            {
                _ChangeMapIndexDelegate = value;
            }
        }

        [field: NonSerialized, JsonIgnore]
        private Delegate _ChangeMapIndexInControlDelegate;
        [XmlIgnore, JsonIgnore]
        public Delegate ChangeMapIndexInControlDelegate
        {
            get { return _ChangeMapIndexInControlDelegate; }
            set
            {
                if (value != _ChangeMapIndexInControlDelegate)
                {
                    _ChangeMapIndexInControlDelegate = value;
                    RaisePropertyChanged();
                }
            }
        }

        [field: NonSerialized, JsonIgnore]
        private bool IsInfo = true;

        [field: NonSerialized, JsonIgnore]
        private event EventHandler _ChangedWaferObjectEvent;
        public event EventHandler ChangedWaferObjectEvent
        {
            add
            {
                if (_ChangedWaferObjectEvent == null || !_ChangedWaferObjectEvent.GetInvocationList().Contains(value))
                {
                    _ChangedWaferObjectEvent += value;
                }
            }
            remove
            {
                _ChangedWaferObjectEvent -= value;
            }
        }

        [NonSerialized]
        private Element<AlignStateEnum> _AlignState = new Element<AlignStateEnum>();
        [XmlIgnore, JsonIgnore]
        public Element<AlignStateEnum> AlignState
        {
            get { return _AlignState; }
            set
            {
                if (value != _AlignState)
                {
                    _AlignState = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private UserIndex _CurrentUIndex = new UserIndex();
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public UserIndex CurrentUIndex
        {
            get { return _CurrentUIndex; }
            set
            {
                if (value != _CurrentUIndex)
                {
                    _CurrentUIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<int> _CurSubIndexX = new Element<int>();
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public Element<int> CurSubIndexX
        {
            get { return _CurSubIndexX; }
            set
            {
                if (value != _CurSubIndexX)
                {
                    _CurSubIndexX = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<int> _CurSubIndexY = new Element<int>();
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public Element<int> CurSubIndexY
        {
            get { return _CurSubIndexY; }
            set
            {
                if (value != _CurSubIndexY)
                {
                    _CurSubIndexY = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private WaferGraphicsModule _WaferGraphicsContext = new WaferGraphicsModule();
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public WaferGraphicsModule WaferGraphicsContext
        {
            get { return _WaferGraphicsContext; }
            set
            {
                if (value != _WaferGraphicsContext)
                {
                    _WaferGraphicsContext = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private WaferStateBase _State;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public WaferStateBase State
        {
            get { return _State; }
            set { _State = value; }
        }


        [NonSerialized]
        private EnumWaferState _WaferState;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
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

        [NonSerialized]
        private WaferStatusBase _Status;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public WaferStatusBase Status
        {
            get { return _Status; }
            set { _Status = value; }
        }

        [NonSerialized]
        private EnumSubsStatus _WaferStatus;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
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

        [NonSerialized]
        private bool _MapViewStageSyncEnable;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        /// <summary>
        /// Map 의 현재 좌표와 스테이지 위치를 동기화 시킬것인지
        /// </summary>
        public bool MapViewStageSyncEnable
        {
            get { return _MapViewStageSyncEnable; }
            set
            {
                if (value != _MapViewStageSyncEnable)
                {
                    _MapViewStageSyncEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private bool _OnceStopBeforeProbing = false;
        [XmlIgnore, JsonIgnore]
        public bool OnceStopBeforeProbing
        {
            get { return _OnceStopBeforeProbing; }
            set
            {
                if (value != _OnceStopBeforeProbing)
                {
                    _OnceStopBeforeProbing = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private bool _OnceStopAfterProbing = false;
        [XmlIgnore, JsonIgnore]
        public bool OnceStopAfterProbing
        {
            get { return _OnceStopAfterProbing; }
            set
            {
                if (value != _OnceStopAfterProbing)
                {
                    _OnceStopAfterProbing = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private bool _IsSendfWaferStartEvent = false;
        [XmlIgnore, JsonIgnore]
        public bool IsSendfWaferStartEvent
        {
            get { return _IsSendfWaferStartEvent; }
            set
            {
                if (value != _IsSendfWaferStartEvent)
                {
                    _IsSendfWaferStartEvent = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private string _MapViewStepLabel;
        [XmlIgnore, JsonIgnore]
        [SharePropPath]
        public string MapViewStepLabel
        {
            get { return _MapViewStepLabel; }
            set
            {
                if (value != _MapViewStepLabel)
                {
                    _MapViewStepLabel = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<EnumProberCam> _MapViewAssignCamType;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public Element<EnumProberCam> MapViewAssignCamType
        {
            get { return _MapViewAssignCamType; }
            set
            {
                if (value != _MapViewAssignCamType)
                {
                    _MapViewAssignCamType = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Visibility _DPMarkerVisible = Visibility.Visible;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public Visibility DPMarkerVisible
        {
            get { return _DPMarkerVisible; }
            set
            {
                if (value != _DPMarkerVisible)
                {
                    _DPMarkerVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Visibility _TopLeftToBottomRightLineVisible = Visibility.Collapsed;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public Visibility TopLeftToBottomRightLineVisible
        {
            get { return _TopLeftToBottomRightLineVisible; }
            set
            {
                if (value != _TopLeftToBottomRightLineVisible)
                {
                    _TopLeftToBottomRightLineVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Visibility _BottomLeftToTopRightLineVisible = Visibility.Collapsed;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public Visibility BottomLeftToTopRightLineVisible
        {
            get { return _BottomLeftToTopRightLineVisible; }
            set
            {
                if (value != _BottomLeftToTopRightLineVisible)
                {
                    _BottomLeftToTopRightLineVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private MapViewMode _MapViewControlMode;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public MapViewMode MapViewControlMode
        {
            get { return _MapViewControlMode; }
            set
            {
                if (value != _MapViewControlMode)
                {
                    _MapViewControlMode = value;
                    RaisePropertyChanged();
                }

                this.LoaderRemoteMediator()?.GetServiceCallBack()?.ChangedIsMapViewControlMode(_MapViewControlMode);
            }
        }

        [NonSerialized]
        private bool _IsMapViewShowPMITable;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public bool IsMapViewShowPMITable
        {
            get { return _IsMapViewShowPMITable; }
            set
            {
                if (value != _IsMapViewShowPMITable)
                {
                    _IsMapViewShowPMITable = value;
                    RaisePropertyChanged();
                }

                this.LoaderRemoteMediator()?.GetServiceCallBack()?.ChangedIsMapViewShowPMITable(_IsMapViewShowPMITable);

            }
        }

        [NonSerialized]
        private bool _IsMapViewShowPMIEnable;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public bool IsMapViewShowPMIEnable
        {
            get { return _IsMapViewShowPMIEnable; }
            set
            {
                if (value != _IsMapViewShowPMIEnable)
                {
                    _IsMapViewShowPMIEnable = value;
                    RaisePropertyChanged();
                }

                this.LoaderRemoteMediator()?.GetServiceCallBack()?.ChangedIsMapViewShowPMIEnable(_IsMapViewShowPMIEnable);

            }
        }

        [NonSerialized]
        private DispFlipEnum _DispHorFlip;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public DispFlipEnum DispHorFlip
        {
            get { return _DispHorFlip; }
            set
            {
                if (value != _DispHorFlip)
                {
                    _DispHorFlip = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private DispFlipEnum _DispVerFlip;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public DispFlipEnum DispVerFlip
        {
            get { return _DispVerFlip; }
            set
            {
                if (value != _DispVerFlip)
                {
                    _DispVerFlip = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private WaferHeightMapping _WaferHeightMapping = new WaferHeightMapping();
        [XmlIgnore]
        public WaferHeightMapping WaferHeightMapping
        {
            get
            {
                if (_WaferHeightMapping == null)
                {
                    _WaferHeightMapping = new WaferHeightMapping();
                }
                return _WaferHeightMapping;
            }

            set { _WaferHeightMapping = value; }
        }
        #endregion

        private Element<bool> _WaferAlignSetupChangedToggle = new Element<bool>();
        public Element<bool> WaferAlignSetupChangedToggle
        {
            get { return _WaferAlignSetupChangedToggle; }
            set { _WaferAlignSetupChangedToggle = value; }
        }

        private Element<bool> _PadSetupChangedToggle = new Element<bool>();
        public Element<bool> PadSetupChangedToggle
        {
            get { return _PadSetupChangedToggle; }
            set { _PadSetupChangedToggle = value; }
        }

        private WaferDevObject _WaferDevObject;
        [ParamIgnore]
        public WaferDevObject WaferDevObject
        {
            get { return _WaferDevObject; }
            set
            {
                if (_WaferDevObject != value)
                {
                    _WaferDevObject = value;
                    RaisePropertyChanged();
                }
            }
        }

        [ParamIgnore]
        public IWaferDevObject WaferDevObjectRef
        {
            get { return WaferDevObject; }
        }

        private PolishWaferInformation _PolishWaferInfo = new PolishWaferInformation();
        public PolishWaferInformation PolishWaferInfo
        {
            get { return _PolishWaferInfo; }
            set { _PolishWaferInfo = value; RaisePropertyChanged(); }
        }

        private float _ZoomLevel;
        public float ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                if (value != _ZoomLevel)
                {
                    _ZoomLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ZoomLevelInit;

        public bool ZoomLevelInit
        {
            get { return _ZoomLevelInit; }
            set { _ZoomLevelInit = value; }
        }

        private long _CurrentMXIndex;
        public long CurrentMXIndex
        {
            get { return _CurrentMXIndex; }
            set
            {
                if (value != _CurrentMXIndex)
                {
                    _CurrentMXIndex = value;
                    RaisePropertyChanged();

                    if (ChangeMapIndexDelegate != null)
                        ChangeMapIndexDelegate(null);

                    if (this.LoaderRemoteMediator()?.GetServiceCallBack() != null)
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.WaferIndexUpdated(CurrentMXIndex, CurrentMYIndex);
                    }
                }
            }
        }

        private long _CurrentMYIndex;
        public long CurrentMYIndex
        {
            get { return _CurrentMYIndex; }
            set
            {
                if (value != _CurrentMYIndex)
                {
                    _CurrentMYIndex = value;
                    RaisePropertyChanged();

                    if (ChangeMapIndexDelegate != null)
                        ChangeMapIndexDelegate(null);

                    if (this.LoaderRemoteMediator()?.GetServiceCallBack() != null)
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.WaferIndexUpdated(CurrentMXIndex, CurrentMYIndex);
                    }

                }
            }
        }

        private bool _MapViewCurIndexVisiablity = true;
        /// <summary>
        /// Map 왼쪽 상단에 좌표계, Map에 현재 좌표 표시 할것인지
        /// </summary>
        public bool MapViewCurIndexVisiablity
        {
            get { return _MapViewCurIndexVisiablity; }
            set
            {
                if (value != _MapViewCurIndexVisiablity)
                {
                    _MapViewCurIndexVisiablity = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MapViewEncoderVisiability = false;
        /// <summary>
        /// MapView 오른쪽 하단에 Motion Encoder표시할지 안할지. (True: 표시함, False :표시안함)
        /// </summary>
        public bool MapViewEncoderVisiability
        {
            get { return _MapViewEncoderVisiability; }
            set
            {
                if (value != _MapViewEncoderVisiability)
                {
                    _MapViewEncoderVisiability = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsInitialized
        {
            get { return true; }
        }

        [ParamIgnore]
        public Element<int> SequenceProcessedCount
        {
            get { return WaferDevObject.Info.SequenceProcessedCount; }
        }
        [ParamIgnore]
        public Element<long> ValidDieCount
        {
            get { return WaferDevObject.ValidDieCount; }
        }
        [ParamIgnore]
        public Element<long> TestDieCount
        {
            get { return WaferDevObject.Info.TestDieCount; }
        }

        [XmlIgnore, ParamIgnore, SharePropPath]
        public IPMIInfo PMIInfo
        {
            get { return WaferDevObject.Info.PMIInfo; }
        }
        public List<DutWaferIndex> DutDieMatchIndexs
        {
            get { return WaferDevObject.Info.DutDieMatchIndexs; }
        }

        #region ==> Method
        public void SetCurrentMIndex(MachineIndex index)
        {
            try
            {
                CurrentMXIndex = index.XIndex;
                CurrentMYIndex = index.YIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EnumWaferType GetWaferType()
        {
            EnumWaferType retval = EnumWaferType.UNDEFINED;

            try
            {
                retval = WaferDevObject?.Info?.WaferType ?? EnumWaferType.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void SetCurrentMIndex(long xindex, long yindex)
        {
            try
            {
                _CurrentMXIndex = xindex;
                RaisePropertyChanged("CurrentMXIndex");

                _CurrentMYIndex = yindex;
                RaisePropertyChanged("CurrentMYIndex");

                if (ChangeMapIndexDelegate != null)
                {
                    ChangeMapIndexDelegate(new Point(xindex, yindex));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetCurrentUIndex(UserIndex index)
        {
            try
            {
                CurrentUIndex.XIndex = index.XIndex;
                CurrentUIndex.YIndex = index.YIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public MachineIndex GetCurrentMIndex()
        {
            return new MachineIndex(CurrentMXIndex, CurrentMYIndex)/*CurrentMIndex*/;
        }
        public UserIndex GetCurrentUIndex()
        {
            return CurrentUIndex;
        }
        public IPhysicalInfo GetPhysInfo()
        {
            if (WaferDevObject != null)
            {
                return WaferDevObject.PhysInfo;
            }
            else
            {
                return new PhysicalInfo();
            }
        }
        public ISubstrateInfo GetSubsInfo()
        {
            if (WaferDevObject != null)
            {
                return WaferDevObject.Info;
            }
            else
            {
                return null;
            }
        }
        public IPolishWaferSourceInformation GetPolishInfo()
        {
            if (PolishWaferInfo != null)
            {
                return PolishWaferInfo;
            }
            else
            {
                return null;
            }
        }
        public void SetPhysInfo(IPhysicalInfo physicalInfo)
        {
            try
            {
                if (physicalInfo != null)
                {
                    if (WaferDevObject != null)
                    {
                        WaferDevObject.PhysInfo = (PhysicalInfo)physicalInfo;
                    }
                    else
                    {
                        LoggerManager.Error($"[WaferObject] SetPhysInfo() : WaferDevObject is null.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public UserIndex MachineToUserIndex(MachineIndex machine)
        {
            UserIndex uidx = new UserIndex();
            try
            {

                switch (WaferDevObject.PhysInfo.MapDirX.Value)
                {
                    case MapHorDirectionEnum.RIGHT:
                        uidx.XIndex = machine.XIndex + WaferDevObject.PhysInfo.OrgU.XIndex.Value;
                        break;
                    case MapHorDirectionEnum.LEFT:
                        uidx.XIndex = WaferDevObject.PhysInfo.MapCountX.Value - 1
                            - machine.XIndex + WaferDevObject.PhysInfo.OrgU.XIndex.Value;
                        break;
                    default:

                        break;
                }
                switch (WaferDevObject.PhysInfo.MapDirY.Value)
                {
                    case MapVertDirectionEnum.DOWN:
                        uidx.YIndex = WaferDevObject.PhysInfo.MapCountY.Value - 1
                            - machine.YIndex + WaferDevObject.PhysInfo.OrgU.YIndex.Value;
                        break;
                    case MapVertDirectionEnum.UP:
                        uidx.YIndex = machine.YIndex + WaferDevObject.PhysInfo.OrgU.YIndex.Value;
                        break;
                    default:
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return uidx;
        }
        public MachineIndex UserToMachineIndex(UserIndex user)
        {
            MachineIndex midx = new MachineIndex();

            try
            {
                switch (WaferDevObject.PhysInfo.MapDirX.Value)
                {
                    case MapHorDirectionEnum.RIGHT:
                        midx.XIndex = user.XIndex - WaferDevObject.PhysInfo.OrgU.XIndex.Value;
                        break;
                    case MapHorDirectionEnum.LEFT:
                        midx.XIndex = WaferDevObject.PhysInfo.MapCountX.Value - 1
                            - (user.XIndex - WaferDevObject.PhysInfo.OrgU.XIndex.Value);
                        break;
                    default:
                        break;
                }
                switch (WaferDevObject.PhysInfo.MapDirY.Value)
                {
                    case MapVertDirectionEnum.DOWN:
                        midx.YIndex = WaferDevObject.PhysInfo.MapCountY.Value - 1
                            - (user.YIndex - WaferDevObject.PhysInfo.OrgU.YIndex.Value);
                        break;
                    case MapVertDirectionEnum.UP:
                        midx.YIndex = user.YIndex - WaferDevObject.PhysInfo.OrgU.YIndex.Value;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return midx;
        }
        public int GetExistDeviceCount(long index, bool horizantal = true)
        {
            int retVal = 0;
            try
            {
                IDeviceObject[,] devices = GetSubsInfo().DIEs;

                if (horizantal)
                {
                    for (int idx = 0; idx < GetPhysInfo().MapCountX.Value; idx++)
                    {
                        if (devices[idx, index].DieType.Value != DieTypeEnum.NOT_EXIST)
                        {
                            retVal++;
                        }
                    }
                }
                else
                {
                    for (int idx = 0; idx < GetPhysInfo().MapCountY.Value; idx++)
                    {
                        if (devices[index, idx].DieType.Value != DieTypeEnum.NOT_EXIST)
                        {
                            retVal++;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public List<IDeviceObject> GetDevices()
        {
            List<IDeviceObject> devs = new List<IDeviceObject>();

            try
            {
                devs.AddRange(WaferDevObject.Info.Devices);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return devs;
        }
        public IDeviceObject Map(long xindex, long yindex)
        {
            DeviceObject dev = null;

            try
            {
                if (WaferDevObject != null)
                {
                    dev = WaferDevObject.Info.Devices.ToList<DeviceObject>().Find(d => d.DieIndex.XIndex == xindex & d.DieIndex.YIndex == yindex);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return dev;
        }
        public EventCodeEnum AddDevice(DeviceObject device)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                try
                {
                    long xindexoffset = 0;
                    long yindexoffset = 0;

                    long maxX = WaferDevObject.Info.Devices.Max(d => d.DieIndexM.XIndex);
                    long minX = WaferDevObject.Info.Devices.Min(d => d.DieIndexM.XIndex);
                    long maxY = WaferDevObject.Info.Devices.Max(d => d.DieIndexM.YIndex);
                    long minY = WaferDevObject.Info.Devices.Min(d => d.DieIndexM.YIndex);
                    long xNum = maxX - minX + 1;
                    long yNum = maxY - minY + 1;

                    byte[,] map = null;
                    if (device.DieIndexM.XIndex < 0)
                    {
                        // X (-)
                        xindexoffset = Math.Abs(device.DieIndexM.XIndex);
                        map = new byte[xNum + xindexoffset, yNum];

                        for (int i = 0; i < xindexoffset; i++)
                        {
                            for (int j = 0; j < yNum; j++)
                            {
                                map[i, j] = (byte)DieTypeEnum.NOT_EXIST;
                                if (i == device.DieIndexM.XIndex + xindexoffset & j == device.DieIndexM.YIndex)
                                {
                                    map[i, j] = (byte)device.DieType.Value;
                                }
                            }
                        }

                        byte[,] curmap = DevicesConvertByteArray();
                        for (int i = 0; i < curmap.GetUpperBound(0) + 1; i++)
                        {
                            for (int j = 0; j < curmap.GetUpperBound(1) + 1; j++)
                            {
                                map[i + xindexoffset, j] = curmap[i, j];
                            }
                        }
                        WaferDevObject.UpdateWaferObject(map, false);
                    }
                    else if (device.DieIndexM.XIndex >= xNum)
                    {
                        // X (+)
                        xindexoffset = device.DieIndexM.XIndex - (WaferDevObject.PhysInfo.MapCountX.Value - 1);
                        map = new byte[xNum + xindexoffset, yNum];
                        byte[,] curmap = DevicesConvertByteArray();

                        for (int i = 0; i < curmap.GetUpperBound(0) + 1; i++)
                        {
                            for (int j = 0; j < curmap.GetUpperBound(1) + 1; j++)
                            {
                                map[i, j] = curmap[i, j];
                            }
                        }


                        for (int i = curmap.GetUpperBound(0) + 1; i < map.GetUpperBound(0) + 1; i++)
                        {
                            for (int j = 0; j < map.GetUpperBound(1) + 1; j++)
                            {
                                map[i, j] = (byte)DieTypeEnum.NOT_EXIST;
                                if (i == device.DieIndexM.XIndex & j == device.DieIndexM.YIndex)
                                {
                                    map[i, j] = (byte)device.DieType.Value;
                                }
                            }
                        }
                        WaferDevObject.UpdateWaferObject(map, false);
                    }



                    if (device.DieIndexM.YIndex < 0)
                    {
                        // Y (-)
                        yindexoffset = Math.Abs(device.DieIndexM.YIndex);
                        map = new byte[xNum, yNum + yindexoffset];

                        for (int j = 0; j < yindexoffset; j++)
                        {
                            for (int i = 0; i < xNum; i++)
                            {
                                map[i, j] = (byte)DieTypeEnum.NOT_EXIST;
                                if (i == device.DieIndexM.XIndex & j == device.DieIndexM.YIndex + yindexoffset)
                                {
                                    map[i, j] = (byte)device.DieType.Value;
                                }
                            }
                        }

                        byte[,] curmap = DevicesConvertByteArray();
                        for (int i = 0; i < curmap.GetUpperBound(0) + 1; i++)
                        {
                            for (int j = 0; j < curmap.GetUpperBound(1) + 1; j++)
                            {
                                map[i, j + yindexoffset] = curmap[i, j];
                            }
                        }
                        WaferDevObject.UpdateWaferObject(map, false);

                    }
                    else if (device.DieIndexM.YIndex >= yNum)
                    {
                        // Y (+)
                        yindexoffset = device.DieIndexM.YIndex - (WaferDevObject.PhysInfo.MapCountY.Value - 1);
                        map = new byte[xNum, yNum + yindexoffset];
                        byte[,] curmap = DevicesConvertByteArray();

                        for (int i = 0; i < curmap.GetUpperBound(0) + 1; i++)
                        {
                            for (int j = 0; j < curmap.GetUpperBound(1) + 1; j++)
                            {
                                map[i, j] = curmap[i, j];
                            }
                        }

                        for (int j = curmap.GetUpperBound(1) + 1; j < map.GetUpperBound(1) + 1; j++)
                        {
                            for (int i = curmap.GetUpperBound(0) + 1; i < map.GetUpperBound(0) + 1; i++)
                            {

                                map[i, j] = (byte)DieTypeEnum.NOT_EXIST;
                                if (i == device.DieIndexM.XIndex & j == device.DieIndexM.YIndex)
                                {
                                    map[i, j] = (byte)device.DieType.Value;
                                }
                            }
                        }
                        WaferDevObject.UpdateWaferObject(map, false);
                    }

                    InitMap();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum RemoveDevice(DeviceObject device, long xindex = -1, long yindex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                long xindexoffset = 0;
                long yindexoffset = 0;

                long maxX = WaferDevObject.Info.Devices.Max(d => d.DieIndexM.XIndex);
                long minX = WaferDevObject.Info.Devices.Min(d => d.DieIndexM.XIndex);
                long maxY = WaferDevObject.Info.Devices.Max(d => d.DieIndexM.YIndex);
                long minY = WaferDevObject.Info.Devices.Min(d => d.DieIndexM.YIndex);

                long xNum = maxX - minX + 1;
                long yNum = maxY - minY + 1;

                byte[,] map = null;


                if (xindex == 0)
                {
                    // X (-)
                    xindexoffset = xindex;
                    map = new byte[xNum - (xindexoffset + 1), yNum];
                    byte[,] curmap = DevicesConvertByteArray();

                    for (int i = 1; i < curmap.GetUpperBound(0) + 1; i++)
                    {
                        for (int j = 0; j < curmap.GetUpperBound(1) + 1; j++)
                        {
                            map[i - 1, j] = curmap[i, j];
                        }
                    }

                    WaferDevObject.UpdateWaferObject(map, false);
                }
                else if (xindex == WaferDevObject.PhysInfo.MapCountX.Value - 1)
                {
                    // X (+)
                    xindexoffset = (WaferDevObject.PhysInfo.MapCountX.Value - 1) - xindex;
                    map = new byte[xNum - 1, yNum];
                    byte[,] curmap = DevicesConvertByteArray();

                    for (int i = 0; i < curmap.GetUpperBound(0); i++)
                    {
                        for (int j = 0; j < curmap.GetUpperBound(1) + 1; j++)
                        {
                            map[i, j] = curmap[i, j];
                        }
                    }

                    WaferDevObject.UpdateWaferObject(map, false);
                }

                if (yindex == 0)
                {
                    // Y (-)
                    yindexoffset = yindex;
                    map = new byte[xNum, yNum - (yindexoffset + 1)];
                    byte[,] curmap = DevicesConvertByteArray();

                    for (int j = 1; j < curmap.GetUpperBound(1) + 1; j++)
                    {
                        for (int i = 0; i < curmap.GetUpperBound(0) + 1; i++)
                        {

                            map[i, j - 1] = curmap[i, j];
                        }
                    }

                    WaferDevObject.UpdateWaferObject(map, false);
                }
                else if (yindex == WaferDevObject.PhysInfo.MapCountY.Value - 1)
                {
                    // Y (+)
                    yindexoffset = (WaferDevObject.PhysInfo.MapCountY.Value - 1) - yindex;
                    map = new byte[xNum, yNum - (yindexoffset + 1)];
                    byte[,] curmap = DevicesConvertByteArray();
                    for (int j = 0; j < curmap.GetUpperBound(1); j++)
                    {
                        for (int i = 0; i < curmap.GetUpperBound(0) + 1; i++)
                        {

                            map[i, j] = curmap[i, j];
                        }
                    }
                    WaferDevObject.UpdateWaferObject(map, false);
                }

                InitMap();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum UpdateWaferObject()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                InitMap();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public List<MachineIndex> makeMultiProbeSeq(IProbeCard card, bool bLeft, bool bTop, int SampleInterval = 1, bool bStartRight = false, bool bUseOneDir = false)
        {
            int ix;
            int iy;
            //int x_num;
            //int y_num;
            long x_num;
            long y_num;
            bool bIsLeft;
            //Collection<int[]> colSeq;
            int[] mi;
            long curInterval;
            int tmpEndVal;
            int tmpStep;
            //barr.Duts[0].XIndex;
            //barr.Duts[0].YIndex;

            //colSeq = new Collection<int[]>();
            List<MachineIndex> seqs = new List<MachineIndex>();
            try
            {
                List<UserIndex> seqsUser = new List<UserIndex>();

                if (bLeft)
                {
                    bIsLeft = true;
                }
                else
                {
                    bIsLeft = false;
                }

                //x_num = barr.GetUpperBound(1) + 1;
                //y_num = barr.GetUpperBound(2) + 1;

                x_num = WaferDevObject.PhysInfo.MapCountX.Value;
                y_num = WaferDevObject.PhysInfo.MapCountY.Value;
                //x_num = 36;
                //y_num = 56;

                //x_num = Dut.NumX;
                //y_num = Dut.NumY;

                long y_start;
                long y_end;
                int y_step;

                int X_Start;
                int X_End;
                int x_Step;
                int ik;
                long il;
                long im;
                int[] ipc;
                int[] ipc1;
                Byte[,] tmp_map;
                int[] tmpsize = new int[4];
                IDeviceObject foundedDev;

                if (card.ProbeCardDevObjectRef.DutList.Count > 0)
                {
                    //ipc = new int[dut.NumY];
                    //ipc1 = new int[dut.NumY];
                    ipc = new int[card.ProbeCardDevObjectRef.DutIndexSizeY];
                    ipc1 = new int[card.ProbeCardDevObjectRef.DutIndexSizeY];

                    for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutIndexSizeY - 1; ik++)
                    {
                        ipc[ik] = 0;
                    }

                    for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutList.Count - 1; ik++)
                    {
                        //card.DutList[ik].MacIndex.YIndex = 0;
                        ipc[card.ProbeCardDevObjectRef.DutList[ik].MacIndex.YIndex] = ipc[card.ProbeCardDevObjectRef.DutList[ik].MacIndex.YIndex] + 1;        //ipc[Dut.DutY[ik]] = ipc[Dut.DutY[ik]] + 1;
                    }

                    //tmpsize[0] = -(dut.NumX - 1);
                    //tmpsize[1] = (int)x_num + dut.NumX - 2;
                    //tmpsize[2] = -(dut.NumY - 1 - 7);
                    //tmpsize[3] = (int)y_num + dut.NumY - 2 + 7;

                    //tmpsize[0] = -(card.XSize - card.XSize);
                    //tmpsize[1] = (int)x_num + card.XSize - 2 + card.XSize;
                    //tmpsize[2] = -(card.YSize - card.YSize);
                    //tmpsize[3] = (int)y_num + card.YSize - 2 + card.YSize;

                    tmpsize[0] = 0;
                    tmpsize[1] = (int)x_num + 1;
                    tmpsize[2] = 0;
                    tmpsize[3] = (int)y_num + 1;

                    //tmp_map = new byte[tmpsize[1] - tmpsize[0] + 1, tmpsize[3] - tmpsize[2] + 1];    //ReDim tmp_map(tmpsize(0) To tmpsize(1), tmpsize(2) To tmpsize(3))                     
                    tmp_map = new byte[tmpsize[1] + 1, tmpsize[3] + 1];    //ReDim tmp_map(tmpsize(0) To tmpsize(1), tmpsize(2) To tmpsize(3))                     

                    il = 0;
                    im = y_num - 1;

                    try
                    {
                        for (ix = tmpsize[0]; ix < tmpsize[1]; ix++)
                        {
                            for (iy = tmpsize[2]; iy < tmpsize[3]; iy++)
                            {
                                if ((ix <= WaferDevObject.PhysInfo.MapCountX.Value - 1 && ix >= 0) && (iy <= WaferDevObject.PhysInfo.MapCountY.Value - 1 && iy >= 0))
                                {
                                    tmp_map[ix, iy] = (byte)DieTypeEnum.NOT_EXIST;

                                    foundedDev = WaferDevObject.Info.Devices.ToList<DeviceObject>().Find(d => d.DieIndexM.XIndex == ix & d.DieIndexM.YIndex == iy);

                                    tmp_map[ix, iy] = (byte)foundedDev.State.Value;

                                    if (tmp_map[ix, iy] == 4)
                                    {
                                        tmp_map[ix, iy] = 3;
                                    }

                                    if (foundedDev.DieType.Value == DieTypeEnum.TEST_DIE)
                                    {
                                        if (iy > il)
                                        {
                                            il = iy;
                                        }

                                        if (iy < im)
                                        {
                                            im = iy;
                                        }
                                    }
                                }
                                else
                                {
                                    //tmp_map[ix, iy] = (byte)DieTypeEnum.NOT_EXIST;
                                    //barr.Devices.Find(d => d.Type == DieTypeEnum.NOT_EXIST).Type;
                                    tmp_map[ix, iy] = (byte)DieTypeEnum.NOT_EXIST;
                                    //tmp_map[ix, iy] = DieTypeEnum.NOT_EXIST;
                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        //LoggerManager.Error($String.Format("Err = {0}", err.Message));
                        LoggerManager.Exception(err);


                        throw;
                    }

                    if (bTop)
                    {
                        y_start = il - (card.ProbeCardDevObjectRef.DutIndexSizeY - 1) + card.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex;         //Dut.DutY[1];
                        y_end = im - (card.ProbeCardDevObjectRef.DutIndexSizeY - 1) + card.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex;
                        y_step = -1;
                    }
                    else
                    {
                        y_start = im + card.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex;
                        y_end = il + card.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex;
                        y_step = 1;
                    }

                    X_Start = -(card.ProbeCardDevObjectRef.DutIndexSizeX - 1);
                    X_End = (int)x_num + card.ProbeCardDevObjectRef.DutIndexSizeX - 2;
                    x_Step = 1;

                    curInterval = SampleInterval;

                    mi = new int[2];
                    int ic;
                    int id;
                    int ie = 0;
                    int ig;

                    try
                    {
                        for (iy = (int)y_start; iy != y_end + y_step; iy += y_step)
                        {
                            if (bIsLeft)
                            {
                                ie = 0;

                                for (ix = X_Start; ix != X_End + x_Step; ix += x_Step)
                                //for (ix = X_Start; ix <= X_End; ix+=x_Step)
                                {
                                    ic = 0;
                                    id = 0;     //Error state

                                    if (CheckSampleDieExist(ix, iy, card, tmp_map) == 1)
                                    {
                                        for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutList.Count - 1; ik++)
                                        {
                                            il = ix + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.XIndex;         // Dut.DutX1[ik];
                                            im = iy + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.YIndex;

                                            if ((il < tmpsize[0] || il > tmpsize[1]) || (im < tmpsize[2] || im > tmpsize[3]))
                                            {

                                            }
                                            else
                                            {
                                                if (tmp_map[il, im] == (byte)DieTypeEnum.TEACH_DIE)
                                                {
                                                    id = 3;
                                                    break;
                                                }
                                                if (tmp_map[il, im] == (byte)DieTypeEnum.TEST_DIE)
                                                {
                                                    ic = ic + 1;
                                                }
                                            }
                                        }

                                        if (ic > 0 && id == 0)
                                        {
                                            if ((curInterval < SampleInterval) && (SampleInterval > 1))
                                            {
                                                curInterval = curInterval + 1;
                                                goto SKIP_SEQ1;     //GoTo SKIP_SEQ1
                                            }
                                            else
                                            {
                                                curInterval = 1;
                                            }

                                            if (bIsLeft)
                                            {
                                                tmpEndVal = ix + card.ProbeCardDevObjectRef.DutIndexSizeX - 1;
                                                tmpStep = 1;
                                            }
                                            else
                                            {
                                                tmpEndVal = ix - card.ProbeCardDevObjectRef.DutIndexSizeX + 1;
                                                tmpStep = -1;
                                            }

                                            if (ie == 0)
                                            {
                                                ic = card.ProbeCardDevObjectRef.DutIndexSizeX;
                                                id = 0;

                                                for (ig = ix; ig != tmpEndVal + tmpStep; ig += tmpStep)
                                                {
                                                    for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutIndexSizeY - 1; ik++)
                                                    {
                                                        ipc1[ik] = 0;
                                                    }

                                                    for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutList.Count - 1; ik++)
                                                    {
                                                        il = ig + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.XIndex;     // DutX1[ik];
                                                        im = iy + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.YIndex;

                                                        if ((il < tmpsize[0] || il > tmpsize[1]) || (im < tmpsize[2] || im > tmpsize[3]))
                                                        {
                                                            //goto SysError;
                                                        }
                                                        else
                                                        {
                                                            if (tmp_map[il, im] == (byte)DieTypeEnum.TEST_DIE)
                                                            {
                                                                ipc1[card.ProbeCardDevObjectRef.DutList[ik].MacIndex.YIndex] = ipc1[card.ProbeCardDevObjectRef.DutList[ik].MacIndex.YIndex] + 1;
                                                            }
                                                            else if (tmp_map[il, im] == (byte)DieTypeEnum.TEACH_DIE)
                                                            {
                                                                goto LineSkip;      //GoTo LineSkip
                                                            }
                                                        }
                                                    }
                                                    for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutIndexSizeY - 1; ik++)
                                                    {
                                                        if ((ipc[ik] > 0) && (ipc1[ik] == ipc[ik]))
                                                        {
                                                            ie = 1;
                                                        }
                                                        else if ((ipc[ik] > 0) && (ipc1[ik] != 0))        //ElseIf (ipc(ik) > 0 && ipc1(ik)) Then
                                                        {
                                                            if (ic > (ipc[ik] - ipc1[ik]))
                                                            {
                                                                ic = ipc[ik] - ipc1[ik];
                                                                id = ig;
                                                            }

                                                        }
                                                    }
                                                    if (ie == 1)
                                                    {
                                                        ix = ig;
                                                        break;
                                                    }
                                                }
                                                if (ie != 1)
                                                {
                                                    ix = id;
                                                }
                                            }

                                            for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutList.Count - 1; ik++)
                                            {
                                                il = ix + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.XIndex;
                                                im = iy + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.YIndex;         //DutY1[ik];

                                                if ((il < tmpsize[0] || il > tmpsize[1]) || (im < tmpsize[2] || im > tmpsize[3]))
                                                {
                                                    //goto SysError;
                                                }
                                                else
                                                {
                                                    if (tmp_map[il, im] == (byte)DieTypeEnum.TEST_DIE)
                                                    {
                                                        tmp_map[il, im] = (byte)DieTypeEnum.TEACH_DIE;
                                                    }
                                                }
                                                ie = 1;
                                            }

                                            mi[0] = ix;
                                            mi[1] = iy;
                                            //colSeq.Add(mi);
                                            seqs.Add(new MachineIndex(ix, iy));
                                            seqsUser.Add(this.CoordinateManager().WMIndexConvertWUIndex(ix, iy));


                                            if (bUseOneDir == false)
                                            {
                                                if (ix < (x_num / 2))
                                                {
                                                    bIsLeft = true;
                                                }
                                                else
                                                {
                                                    bIsLeft = false;
                                                }
                                            }
                                        }

                                    SKIP_SEQ1:
                                        ;
                                    }
                                }

                                ic = 0;

                                for (id = iy - (int)card.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex; id <= iy + card.ProbeCardDevObjectRef.DutIndexSizeY - card.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex - 1; id++)
                                {
                                    if ((id < tmpsize[2]) || (id > tmpsize[3]))
                                    {

                                    }
                                    else
                                    {
                                        for (ix = 0; ix <= x_num - 1; ix++)
                                        {
                                            if ((ix < 0 || ix > x_num - 1) || (id < 0 || id > y_num - 1))
                                            {

                                            }
                                            else
                                            {
                                                foundedDev = WaferDevObject.Info.Devices.ToList<DeviceObject>().Find(d => d.DieIndexM.XIndex == ix & d.DieIndexM.YIndex == id);

                                                if (foundedDev.DieType.Value == DieTypeEnum.TEST_DIE)
                                                {
                                                    if (tmp_map[ix, id] != (byte)DieTypeEnum.TEACH_DIE)
                                                        ic = ic + 1;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (ic == 0)
                                {
                                    for (id = iy - (int)card.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex; id <= iy + card.ProbeCardDevObjectRef.DutIndexSizeY - card.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex - 1; id++)
                                    {
                                        for (ix = tmpsize[0]; ix < tmpsize[1]; ix++)
                                        {
                                            if ((ix < tmpsize[0] || ix > tmpsize[1]) || (id < tmpsize[2] || id > tmpsize[3]))
                                            {

                                            }
                                            else
                                            {
                                                tmp_map[ix, id] = (byte)DieTypeEnum.TEACH_DIE;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ie = 0;

                                for (ix = X_End; ix != X_Start - x_Step; ix -= x_Step)
                                {
                                    if (CheckSampleDieExist(ix, iy, card, tmp_map) == 1)
                                    {
                                        ic = 0;
                                        id = 0;

                                        for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutList.Count - 1; ik++)
                                        {
                                            il = ix + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.XIndex;         // DutX1[ik];
                                            im = iy + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.YIndex;         // DutY1[ik];

                                            if ((il < tmpsize[0] || il > tmpsize[1]) || (im < tmpsize[2] || im > tmpsize[3]))
                                            {

                                            }
                                            else
                                            {
                                                if (tmp_map[il, im] == (byte)DieTypeEnum.TEACH_DIE)
                                                {
                                                    id = 3;
                                                    break;
                                                }
                                                if (tmp_map[il, im] == (byte)DieTypeEnum.TEST_DIE)
                                                    ic = ic + 1;
                                            }
                                        }

                                        if (ic > 0 && id == 0)
                                        {
                                            if ((curInterval < SampleInterval) && (SampleInterval > 1))
                                            {
                                                curInterval = curInterval + 1;
                                                goto SKIP_SEQ2;
                                            }
                                            else
                                            {
                                                curInterval = 1;
                                            }

                                            if (bIsLeft)
                                            {
                                                tmpEndVal = ix + card.ProbeCardDevObjectRef.DutIndexSizeX - 1;
                                                tmpStep = 1;
                                            }
                                            else
                                            {
                                                tmpEndVal = ix - card.ProbeCardDevObjectRef.DutIndexSizeX + 1;
                                                tmpStep = -1;
                                            }

                                            if (ie == 0)
                                            {
                                                ic = card.ProbeCardDevObjectRef.DutIndexSizeX;
                                                id = 0;

                                                for (ig = ix; ig != tmpEndVal + tmpStep; ig += tmpStep)
                                                {
                                                    for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutIndexSizeY - 1; ik++)
                                                    {
                                                        ipc1[ik] = 0;
                                                    }
                                                    for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutList.Count - 1; ik++)
                                                    {
                                                        il = ig + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.XIndex;     // DutX1[ik];
                                                        im = iy + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.YIndex;
                                                        if ((il < tmpsize[0] || il > tmpsize[1]) || (im < tmpsize[2] || im > tmpsize[3]))
                                                        {
                                                            //goto SysError;
                                                        }
                                                        else
                                                        {
                                                            if (tmp_map[il, im] == (byte)DieTypeEnum.TEST_DIE)
                                                            {
                                                                ipc1[card.ProbeCardDevObjectRef.DutList[ik].MacIndex.YIndex] = ipc1[card.ProbeCardDevObjectRef.DutList[ik].MacIndex.YIndex] + 1;
                                                            }
                                                            else if (tmp_map[il, im] == (byte)DieTypeEnum.TEACH_DIE)
                                                            {
                                                                goto LineSkip;
                                                            }
                                                        }
                                                    }
                                                    for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutIndexSizeY - 1; ik++)
                                                    {
                                                        if ((ipc[ik] > 0) && (ipc1[ik] == ipc[ik]))
                                                            ie = 1;
                                                        else if ((ipc[ik] > 0) && (ipc1[ik] != 0))
                                                        {
                                                            if (ic > ipc[ik] - ipc1[ik])
                                                            {
                                                                ic = ipc[ik] - ipc1[ik];
                                                                id = ig;
                                                            }
                                                        }
                                                    }

                                                    if (ie == 1)
                                                    {
                                                        ix = ig;
                                                        break;
                                                        //Exit for
                                                    }
                                                }
                                                if (ie != 1)
                                                {
                                                    ix = id;
                                                }
                                            }

                                            for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutList.Count - 1; ik++)
                                            {
                                                il = ix + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.XIndex;     // DutX1[ik];
                                                im = iy + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.YIndex;
                                                if ((il < tmpsize[0] || il > tmpsize[1]) || (im < tmpsize[2] || im > tmpsize[3]))
                                                {

                                                }
                                                else
                                                {
                                                    if (tmp_map[il, im] == (byte)DieTypeEnum.TEST_DIE)
                                                    {
                                                        tmp_map[il, im] = (byte)DieTypeEnum.TEACH_DIE;
                                                    }
                                                }
                                                ie = 1;
                                            }
                                            mi[0] = ix;
                                            mi[1] = iy;
                                            //colSeq.Add(mi);
                                            seqs.Add(new MachineIndex(ix, iy));
                                            seqsUser.Add(this.CoordinateManager().WMIndexConvertWUIndex(ix, iy));

                                            if (bUseOneDir == false)
                                            {
                                                if (ix < (x_num / 2))
                                                {
                                                    bIsLeft = true;
                                                }
                                                else
                                                {
                                                    bIsLeft = false;
                                                }
                                            }
                                        }
                                    SKIP_SEQ2:
                                        ;
                                    }
                                }

                                ic = 0;

                                for (id = iy - (int)card.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex; id <= iy + card.ProbeCardDevObjectRef.DutIndexSizeY - card.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex - 1; id++)
                                {
                                    for (ix = 0; ix <= x_num - 1; ix++)
                                    {
                                        if ((ix < 0 || ix > x_num - 1) || (id < 0 || id > y_num - 1))
                                        {

                                        }
                                        else
                                        {
                                            foundedDev = WaferDevObject.Info.Devices.ToList<DeviceObject>().Find(d => d.DieIndexM.XIndex == ix & d.DieIndexM.YIndex == id);
                                            if (foundedDev.DieType.Value == DieTypeEnum.TEST_DIE)
                                            //if ((byte)(barr.Devices.Find(d => d.DieIndexM.XIndex == ix).DieIndex.XIndex & barr.Devices.Find(d => d.DieIndexM.YIndex == id).DieIndex.YIndex) == (byte)DieTypeEnum.TEST_DIE)
                                            {
                                                if (tmp_map[ix, id] != (byte)DieTypeEnum.TEACH_DIE)
                                                    ic = ic + 1;
                                            }
                                        }
                                    }
                                }

                                if (ic == 0)
                                {
                                    for (id = iy - (int)card.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex; id <= iy + card.ProbeCardDevObjectRef.DutIndexSizeY - (int)card.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex - 1; id++)
                                    {
                                        for (ix = tmpsize[0]; ix < tmpsize[1]; ix++)
                                        {
                                            if ((ix < tmpsize[0] || ix > tmpsize[1]) || (id < tmpsize[2] || id > tmpsize[3]))
                                            {

                                            }
                                            else
                                            {
                                                tmp_map[ix, id] = (byte)DieTypeEnum.TEACH_DIE;
                                            }
                                        }
                                    }
                                }
                            }
                        LineSkip:
                            ;
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);

                        throw;
                    }

                    tmp_map = null;
                    mi = null;
                    ipc = null;
                    ipc1 = null;

                    LoggerManager.Debug($"SEQ : {seqs.Count}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return seqs;
        }
        public List<MachineIndex> makeMultiProbeSeq_hor(IProbeCard card, bool bLeft, bool bTop, int SampleInterval = 1, bool bStartRight = false, bool bUseOneDir = false)
        {
            int ix;
            int iy;
            long x_num;
            long y_num;
            bool bIsBottom;
            //Collection<int[]> colSeq;
            int[] mi;
            long curInterval;

            //bool bSecond;
            int tmpEndVal;
            int tmpStep;

            //colSeq = new Collection<int[]>();
            List<MachineIndex> seqs = new List<MachineIndex>();
            List<UserIndex> seqsUser = new List<UserIndex>();

            try
            {

                if (bTop)
                {
                    bIsBottom = false;
                }
                else
                {
                    bIsBottom = true;
                }

                x_num = WaferDevObject.PhysInfo.MapCountX.Value;
                y_num = WaferDevObject.PhysInfo.MapCountY.Value;

                int y_start;
                int y_end;
                int y_step;

                long X_Start;
                long X_End;
                int x_Step;
                int ik;
                long il;
                long im;
                int[] ipc;
                int[] ipc1;
                Byte[,] tmp_map;
                int[] tmpsize = new int[4];
                //int tmpStartVal;
                IDeviceObject foundedDev;

                ipc = new int[card.ProbeCardDevObjectRef.DutIndexSizeY];
                ipc1 = new int[card.ProbeCardDevObjectRef.DutIndexSizeY];

                for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutIndexSizeX - 1; ik++)
                {
                    ipc[ik] = 0;
                }
                for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutList.Count - 1; ik++)
                {
                    //card.DutList[ik].MacIndex.YIndex = 0;
                    ipc[card.ProbeCardDevObjectRef.DutList[ik].MacIndex.YIndex] = ipc[card.ProbeCardDevObjectRef.DutList[ik].MacIndex.YIndex] + 1;        //ipc[Dut.DutY[ik]] = ipc[Dut.DutY[ik]] + 1;
                }

                tmpsize[0] = 0;
                tmpsize[1] = (int)x_num + 1;
                tmpsize[2] = 0;
                tmpsize[3] = (int)y_num + 1;

                //tmp_map = new byte[tmpsize[1] - tmpsize[0] + 1, tmpsize[3] - tmpsize[2] + 1];    //ReDim tmp_map(tmpsize(0) To tmpsize(1), tmpsize(2) To tmpsize(3))                     
                tmp_map = new byte[tmpsize[1] + 1, tmpsize[3] + 1];    //ReDim tmp_map(tmpsize(0) To tmpsize(1), tmpsize(2) To tmpsize(3))                     

                il = 0;
                im = x_num - 1;

                for (ix = tmpsize[0]; ix < tmpsize[1]; ix++)
                {
                    for (iy = tmpsize[2]; iy < tmpsize[3]; iy++)
                    {
                        if (ix <= WaferDevObject.PhysInfo.MapCountX.Value - 1 && ix >= 0 && iy <= WaferDevObject.PhysInfo.MapCountY.Value - 1 && iy >= 0)
                        {
                            tmp_map[ix, iy] = (byte)DieTypeEnum.NOT_EXIST;

                            foundedDev = WaferDevObject.Info.Devices.ToList<DeviceObject>().Find(d => d.DieIndexM.XIndex == ix & d.DieIndexM.YIndex == iy);

                            tmp_map[ix, iy] = (byte)foundedDev.State.Value;

                            if (tmp_map[ix, iy] == 4)
                            {
                                tmp_map[ix, iy] = 3;
                            }
                            if (foundedDev.DieType.Value == DieTypeEnum.TEST_DIE)
                            {
                                if (ix > il)
                                    il = ix;
                                if (ix < im)
                                    im = ix;
                            }
                        }
                        else
                        {
                            tmp_map[ix, iy] = (byte)DieTypeEnum.NOT_EXIST;
                        }
                    }
                }

                //y_start = -(dut.NumY - 1);
                y_start = -(card.ProbeCardDevObjectRef.DutIndexSizeY - 1);
                y_end = (int)y_num + card.ProbeCardDevObjectRef.DutIndexSizeY - 2;
                y_step = 1;

                if (bLeft)
                {
                    X_Start = im + card.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex;
                    X_End = il + card.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex;
                    x_Step = 1;
                }
                else
                {
                    X_Start = il - ((card.ProbeCardDevObjectRef.DutIndexSizeX - 1) - card.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex);
                    X_End = im - ((card.ProbeCardDevObjectRef.DutIndexSizeX - 1) - card.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex);
                    x_Step = -1;
                }

                curInterval = SampleInterval;
                mi = new int[2];
                int ic;
                int id;
                int ie = 0;
                int ig;

                for (ix = (int)X_Start; ix != X_End + x_Step; ix += x_Step)
                {
                    if (bIsBottom)
                    {
                        ie = 0;
                        //for (iy = y_start; iy <= y_end; iy++)
                        for (iy = y_start; iy != y_end + y_step; iy += y_step)
                        {
                            ic = 0;
                            id = 0;

                            if (CheckSampleDieExist(ix, iy, card, tmp_map) == 1)
                            {
                                for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutList.Count - 1; ik++)
                                {
                                    il = ix + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.XIndex;
                                    im = iy + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.YIndex;

                                    if ((il < tmpsize[0] || il > tmpsize[1]) || (im < tmpsize[2] || im > tmpsize[3]))
                                    {

                                    }
                                    else
                                    {
                                        if (tmp_map[il, im] == (byte)DieTypeEnum.TEACH_DIE)
                                        {
                                            id = 3;
                                            break;
                                        }
                                        if (tmp_map[il, im] == (byte)DieTypeEnum.TEST_DIE)
                                            ic = ic + 1;
                                    }
                                }

                                if (ic > 0 && id == 0)
                                {
                                    if ((curInterval < SampleInterval) && (SampleInterval > 1))
                                    {
                                        curInterval = curInterval + 1;
                                        goto SKIP_SEQ1;
                                    }
                                    else
                                    {
                                        curInterval = 1;
                                    }

                                    if (bIsBottom)
                                    {
                                        tmpEndVal = iy + card.ProbeCardDevObjectRef.DutIndexSizeY - 1;
                                        tmpStep = 1;
                                    }
                                    else
                                    {
                                        tmpEndVal = iy - card.ProbeCardDevObjectRef.DutIndexSizeY + 1;
                                        tmpStep = -1;
                                    }

                                    if (ie == 0)
                                    {
                                        ic = card.ProbeCardDevObjectRef.DutIndexSizeY;
                                        id = 0;

                                        for (ig = iy; ig != tmpEndVal + tmpStep; ig += tmpStep)
                                        {
                                            for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutIndexSizeX - 1; ik++)
                                            {
                                                ipc1[ik] = 0;
                                            }

                                            for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutList.Count - 1; ik++)
                                            {
                                                il = ix + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.XIndex;
                                                im = ig + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.YIndex;

                                                if ((il < tmpsize[0] || il > tmpsize[1]) || (im < tmpsize[2] || im > tmpsize[3]))
                                                {

                                                }
                                                else
                                                {
                                                    if (tmp_map[il, im] == (byte)DieTypeEnum.TEST_DIE)
                                                        ipc1[card.ProbeCardDevObjectRef.DutList[ik].MacIndex.XIndex] = ipc1[card.ProbeCardDevObjectRef.DutList[ik].MacIndex.XIndex] + 1;
                                                    else if (tmp_map[il, im] == (byte)DieTypeEnum.TEACH_DIE)
                                                    {
                                                        goto LineSkip;
                                                    }
                                                }
                                            }
                                            for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutIndexSizeX - 1; ik++)
                                            {
                                                if ((ipc[ik] > 0) && (ipc1[ik] == ipc[ik]))
                                                    ie = 1;
                                                else if ((ipc[ik] > 0) && (ipc1[ik] != 0))
                                                {
                                                    if (ic > (ipc[ik] - ipc1[ik]))
                                                    {
                                                        ic = ipc[ik] - ipc1[ik];
                                                        id = ig;
                                                    }
                                                }
                                            }
                                            if (ie == 1)
                                            {
                                                iy = ig;
                                                break;
                                            }
                                        }
                                        if (ie != 1)
                                        {
                                            iy = id;
                                        }
                                    }

                                    for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutList.Count - 1; ik++)
                                    {
                                        il = ix + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.XIndex;
                                        im = iy + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.YIndex;

                                        if ((il < tmpsize[0] || il > tmpsize[1]) || (im < tmpsize[2] || im > tmpsize[3]))
                                        {

                                        }
                                        else
                                        {
                                            if (tmp_map[il, im] == (byte)DieTypeEnum.TEST_DIE)
                                            {
                                                tmp_map[il, im] = (byte)DieTypeEnum.TEACH_DIE;
                                            }
                                        }
                                        ie = 1;
                                    }
                                    mi[0] = ix;
                                    mi[1] = iy;

                                    seqs.Add(new MachineIndex(ix, iy));
                                    seqsUser.Add(this.CoordinateManager().WMIndexConvertWUIndex(ix, iy));


                                    if (bUseOneDir == false)
                                    {
                                        if (iy < (y_num / 2))
                                        {
                                            bIsBottom = true;
                                        }
                                        else
                                        {
                                            bIsBottom = false;
                                        }
                                    }
                                }
                            SKIP_SEQ1:
                                ;
                            }
                        }


                        ic = 0;

                        for (id = ix - (int)card.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex; id <= ix + card.ProbeCardDevObjectRef.DutIndexSizeX - card.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex - 1; id++)
                        {
                            if (id < tmpsize[0] || id > tmpsize[1])
                            {

                            }
                            else
                            {
                                for (iy = 0; iy <= y_num - 1; iy++)
                                {
                                    if ((iy < 0 || iy > y_num - 1) || (id < 0 || id > x_num - 1))
                                    {

                                    }
                                    else
                                    {
                                        foundedDev = WaferDevObject.Info.Devices.ToList<DeviceObject>().Find(d => d.DieIndexM.XIndex == id & d.DieIndexM.YIndex == iy);
                                        if (foundedDev.DieType.Value == DieTypeEnum.TEST_DIE)
                                        {
                                            if (tmp_map[id, iy] != (byte)DieTypeEnum.TEACH_DIE)
                                                ic = ic + 1;
                                        }
                                    }
                                }
                            }
                        }

                        if (ic == 0)
                        {
                            for (id = ix - (int)card.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex; id <= ix + card.ProbeCardDevObjectRef.DutIndexSizeX - card.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex - 1; id++)
                            {
                                for (iy = tmpsize[2]; iy < tmpsize[3]; iy++)
                                {
                                    if ((iy < tmpsize[2] || iy > tmpsize[3]) || (id < tmpsize[0] || id > tmpsize[1]))
                                    {

                                    }
                                    else
                                    {
                                        tmp_map[id, iy] = (byte)DieTypeEnum.TEACH_DIE;
                                    }
                                }
                            }
                        }
                    }

                    else
                    {
                        ie = 0;

                        for (iy = y_end; iy != y_start - y_step; iy -= y_step)
                        {
                            if (CheckSampleDieExist(ix, iy, card, tmp_map) == 1)
                            {
                                ic = 0;
                                id = 0;

                                for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutList.Count - 1; ik++)
                                {
                                    il = ix + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.XIndex;
                                    im = iy + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.YIndex;

                                    if ((il < tmpsize[0] || il > tmpsize[1]) || (im < tmpsize[2] || im > tmpsize[3]))
                                    {

                                    }
                                    else
                                    {
                                        if (tmp_map[il, im] == (byte)DieTypeEnum.TEACH_DIE)
                                        {
                                            id = 3;
                                            break;
                                        }
                                        if (tmp_map[il, im] == (byte)DieTypeEnum.TEST_DIE)
                                            ic = ic + 1;
                                    }
                                }

                                if (ic > 0 && id == 0)
                                {
                                    if ((curInterval < SampleInterval) && (SampleInterval > 1))
                                    {
                                        curInterval = curInterval + 1;
                                        goto SKIP_SEQ2;
                                    }
                                    else
                                    {
                                        curInterval = 1;
                                    }

                                    if (bIsBottom)
                                    {
                                        tmpEndVal = iy + card.ProbeCardDevObjectRef.DutIndexSizeY - 1;
                                        tmpStep = 1;
                                    }
                                    else
                                    {
                                        tmpEndVal = iy - card.ProbeCardDevObjectRef.DutIndexSizeY + 1;
                                        tmpStep = -1;
                                    }

                                    if (ie == 0)
                                    {
                                        ic = card.ProbeCardDevObjectRef.DutIndexSizeY;
                                        id = 0;

                                        for (ig = iy; ig >= tmpEndVal + tmpStep; ig += tmpStep)
                                        {
                                            for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutIndexSizeX - 1; ik++)
                                            {
                                                ipc1[ik] = 0;
                                            }
                                            for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutList.Count - 1; ik++)
                                            {
                                                il = ix + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.XIndex;
                                                im = ig + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.YIndex;

                                                if ((il < tmpsize[0] || il > tmpsize[1]) || (im < tmpsize[2] || im > tmpsize[3]))
                                                {

                                                }
                                                else
                                                {
                                                    if (tmp_map[il, im] == (byte)DieTypeEnum.TEST_DIE)
                                                        ipc1[card.ProbeCardDevObjectRef.DutList[ik].MacIndex.XIndex] = ipc1[card.ProbeCardDevObjectRef.DutList[ik].MacIndex.XIndex] + 1;
                                                    else if (tmp_map[il, im] == (byte)DieTypeEnum.TEACH_DIE)
                                                    {
                                                        goto LineSkip;
                                                    }
                                                }
                                            }

                                            for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutIndexSizeX - 1; ik++)
                                            {
                                                if ((ipc[ik] > 0) && (ipc1[ik] == ipc[ik]))
                                                {
                                                    ie = 1;
                                                }
                                                else if ((ipc[ik] > 0) && (ipc1[ik] != 0))
                                                {
                                                    if (ic > (ipc[ik] - ipc1[ik]))
                                                    {
                                                        ic = ipc[ik] - ipc1[ik];
                                                        id = ig;
                                                    }
                                                }
                                            }
                                            if (ie == 1)
                                            {
                                                iy = ig;
                                                break;
                                            }
                                        }

                                        if (ie != 1)
                                        {
                                            iy = id;
                                        }
                                    }

                                    for (ik = 0; ik <= card.ProbeCardDevObjectRef.DutList.Count - 1; ik++)
                                    {
                                        il = ix + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.XIndex;
                                        im = iy + card.ProbeCardDevObjectRef.DutList[ik].UserIndex.YIndex;

                                        if ((il < tmpsize[0] || il > tmpsize[1]) || (im < tmpsize[2] || im > tmpsize[3]))
                                        {

                                        }
                                        else
                                        {
                                            if (tmp_map[il, im] == (byte)DieTypeEnum.TEST_DIE)
                                            {
                                                tmp_map[il, im] = (byte)DieTypeEnum.TEACH_DIE;
                                            }

                                            ie = 1;
                                        }
                                    }

                                    mi[0] = ix;
                                    mi[1] = iy;
                                    seqs.Add(new MachineIndex(ix, iy));
                                    seqsUser.Add(this.CoordinateManager().WMIndexConvertWUIndex(ix, iy));

                                    if (bUseOneDir == false)
                                    {
                                        if (iy < (y_num / 2))
                                        {
                                            bIsBottom = true;
                                        }
                                        else
                                        {
                                            bIsBottom = false;
                                        }
                                    }
                                }
                            SKIP_SEQ2:
                                ;
                            }
                        }

                        ic = 0;

                        for (id = ix - (int)card.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex; id <= ix + card.ProbeCardDevObjectRef.DutIndexSizeX - card.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex - 1; id++)
                        {
                            for (iy = 0; iy <= y_num - 1; iy++)
                            {
                                if ((iy < 0 || iy > y_num - 1) || (id < 0 || id > x_num - 1))
                                {

                                }
                                else
                                {
                                    foundedDev = WaferDevObject.Info.Devices.ToList<DeviceObject>().Find(d => d.DieIndexM.XIndex == id & d.DieIndexM.YIndex == iy);
                                    if (foundedDev.DieType.Value == DieTypeEnum.TEST_DIE)
                                    //if ((byte)(barr.Devices.Find(d => d.DieIndexM.XIndex == id).DieIndexM.XIndex & barr.Devices.Find(d => d.DieIndexM.YIndex == iy).DieIndexM.YIndex) == (byte)barr.Devices.Find(d => d.Type == DieTypeEnum.TEST_DIE).Type)
                                    {
                                        if (tmp_map[id, iy] != (byte)DieTypeEnum.TEACH_DIE)
                                            ic = ic + 1;
                                    }
                                }
                            }
                        }

                        if (ic == 0)
                        {
                            for (id = ix - (int)card.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex; id <= ix + card.ProbeCardDevObjectRef.DutIndexSizeX - card.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex - 1; id++)
                            {
                                for (iy = tmpsize[2]; iy < tmpsize[3]; iy++)
                                {
                                    if ((iy < tmpsize[2] || iy > tmpsize[3]) || (id < tmpsize[0] || id > tmpsize[1]))
                                    {

                                    }
                                    else
                                    {
                                        tmp_map[id, iy] = (byte)DieTypeEnum.TEACH_DIE;
                                    }
                                }
                            }
                        }
                    }
                LineSkip:
                    ;
                }

                tmp_map = null;
                mi = null;
                ipc = null;
                ipc1 = null;
            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return seqs;
        }
        public int CheckSampleDieExist(int mx, int my, IProbeCard card, Byte[,] tmp_map)
        {
            int j;
            int result = -1;

            try
            {
                for (j = 0; j < card.ProbeCardDevObjectRef.DutList.Count - 1; j++)
                {
                    if (mx + card.ProbeCardDevObjectRef.DutList[j].UserIndex.XIndex >= 0 &&
                        my + card.ProbeCardDevObjectRef.DutList[j].UserIndex.YIndex >= 0 &&
                        mx + card.ProbeCardDevObjectRef.DutList[j].UserIndex.XIndex <= WaferDevObject.PhysInfo.MapCountX.Value - 1 &&
                        my + card.ProbeCardDevObjectRef.DutList[j].UserIndex.YIndex <= WaferDevObject.PhysInfo.MapCountY.Value - 1)
                    {
                        if (tmp_map[mx + card.ProbeCardDevObjectRef.DutList[j].UserIndex.XIndex, my + card.ProbeCardDevObjectRef.DutList[j].UserIndex.YIndex] == (byte)DieTypeEnum.SAMPLE_DIE)
                        {
                            result = -1;
                            break;
                        }
                    }
                }

                result = 1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }
        public byte[,] DevicesConvertByteArray()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            // Begin timing.
            stopwatch.Start();

            long maxX, minX, maxY, minY;
            long xNum, yNum;

            byte[,] devicesmap = null;

            try
            {
                maxX = WaferDevObject.Info.Devices.Max(d => d.DieIndexM.XIndex);
                minX = WaferDevObject.Info.Devices.Min(d => d.DieIndexM.XIndex);
                maxY = WaferDevObject.Info.Devices.Max(d => d.DieIndexM.YIndex);
                minY = WaferDevObject.Info.Devices.Min(d => d.DieIndexM.YIndex);

                xNum = maxX - minX + 1;
                yNum = maxY - minY + 1;

                devicesmap = new byte[xNum, yNum];

                Parallel.For(0, yNum, y =>
                {
                    Parallel.For(0, xNum, x =>
                    {
                        DeviceObject dev = null;
                        DieStateEnum state = DieStateEnum.UNKNOWN;

                        dev = WaferDevObject.Info.Devices.ToList<DeviceObject>().Find(d => d.DieIndexM.XIndex == x && d.DieIndexM.YIndex == y);

                        if (dev != null)
                        {
                            state = dev.State.Value;
                        }

                        switch (state)
                        {
                            case DieStateEnum.MARK:
                                devicesmap[x, y] = (byte)DieTypeEnum.MARK_DIE;
                                break;
                            case DieStateEnum.NORMAL:
                                devicesmap[x, y] = (byte)DieTypeEnum.TEST_DIE;
                                break;
                            case DieStateEnum.NOT_EXIST:
                                devicesmap[x, y] = (byte)DieTypeEnum.NOT_EXIST;
                                break;
                            case DieStateEnum.TESTED:
                                devicesmap[x, y] = (byte)DieTypeEnum.TEST_DIE;
                                break;
                            case DieStateEnum.SKIPPED:
                                devicesmap[x, y] = (byte)DieTypeEnum.SKIP_DIE;
                                break;
                        }
                    });
                });
            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            // Stop timing.
            stopwatch.Stop();

            // Write result.
            LoggerManager.Debug($"[{this.GetType().Name}], DevicesConvertByteArray() : Time elapsed: {stopwatch.Elapsed}");

            return devicesmap;
        }
        public EventCodeEnum DrawDieOverlay(ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (cam != null)
                {
                    cam.DrawDisplayDelegate += (ImageBuffer img, ICamera camera) =>
                    {
                        WaferGraphicsContext.DrawDieOverlay(img, camera);
                    };
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum StopDrawDieOberlay(ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (cam != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        cam.DisplayService.OverlayCanvas.Children.Clear();
                    }));
                    cam.DrawDisplayDelegate -= (ImageBuffer img, ICamera camera) =>
                    {
                        WaferGraphicsContext.DrawDieOverlay(img, camera);
                    };
                    cam.DrawDisplayDelegate = null;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum ChangeAlignSetupControlFlag(DrawDieOverlayEnum mode, bool flag, int offset = 0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                switch (mode)
                {
                    case DrawDieOverlayEnum.ALLDie:
                        break;
                    case DrawDieOverlayEnum.CenterDie:
                        WaferGraphicsContext.WaferAlignSetupControl.CenterDie = flag;
                        break;
                    case DrawDieOverlayEnum.Edge_Center:
                        WaferGraphicsContext.WaferAlignSetupControl.Edge_Center = flag;
                        WaferGraphicsContext.WaferAlignSetupControl.Edge_Center_Area = offset;
                        break;
                    case DrawDieOverlayEnum.Align_Center:
                        WaferGraphicsContext.WaferAlignSetupControl.Align_Center = flag;
                        break;
                    case DrawDieOverlayEnum.Centerofthecenterdie:
                        WaferGraphicsContext.WaferAlignSetupControl.Centerofthecenterdie = flag;
                        break;
                    default:
                        break;
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum DrawPadOverlay(ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (cam != null)
                {
                    cam.DrawDisplayDelegate += (ImageBuffer img, ICamera camera) =>
                    {
                        WaferGraphicsContext.DrawPadOverlay(img, camera);
                    };
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum StopDrawPadOverlay(ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (cam != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        cam.DisplayService.OverlayCanvas.Children.Clear();
                    }));

                    if (cam.DrawDisplayDelegate != null)
                    {
                        cam.DrawDisplayDelegate -= (ImageBuffer img, ICamera camera) =>
                        {
                            WaferGraphicsContext.DrawPadOverlay(img, camera);
                        };
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum ResetWaferData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                List<DeviceObject> devices = WaferDevObject.Info.Devices.ToList<DeviceObject>();
                List<DeviceObject> resultdevices = devices.FindAll(device => device.State.Value == DieStateEnum.TESTED);

                Parallel.For(0, resultdevices.Count, i =>
                {
                    resultdevices[i].State.Value = DieStateEnum.NORMAL;
                    resultdevices[i].CurTestHistory.BinCode.Value = 0;
                    resultdevices[i].CurTestHistory.TestResult.Value = TestState.MAP_STS_TEST;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum ResetWaferData(MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                List<DeviceObject> devices = WaferDevObject.Info.Devices.ToList<DeviceObject>();
                List<DeviceObject> resultdevices = devices.FindAll(device => device.State.Value == DieStateEnum.TESTED);
                DeviceObject dev = resultdevices.Where(device => device.DieIndexM.XIndex == MI.XIndex && device.DieIndexM.YIndex == MI.YIndex).FirstOrDefault();

                var idx = resultdevices.IndexOf(dev);
                idx = this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value.IndexOf(MI);

                Parallel.For(idx, this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value.Count, i =>
                {
                    var mi = this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value[i];
                    var die = resultdevices.Where(device => device.DieIndexM.XIndex == mi.XIndex && device.DieIndexM.YIndex == mi.YIndex).FirstOrDefault();
                    if (die != null)
                    {
                        die.State.Value = DieStateEnum.NORMAL;
                        die.CurTestHistory.BinCode.Value = 0;
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        #region Status
        public EnumSubsStatus GetStatus()
        {
            return Status.GetState();
        }
        public void ChangeStatus(WaferStatusBase status)
        {
            try
            {
                Status = status;
                WaferStatus = Status.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetWaferStatus(EnumSubsStatus status, EnumWaferType waferType = EnumWaferType.UNDEFINED, string waferID = "", int slotNum = 0)
        {
            try
            {
                var preStatus = Status?.GetState() ?? EnumSubsStatus.UNDEFINED;

                switch (status)
                {
                    case EnumSubsStatus.UNKNOWN:

                        Status.SetStatusMissing();
                        this.StageSupervisor().GetSlotInfo().WaferState = EnumWaferState.MISSED;
                        this.LotOPModule().LotInfo.isNewLot = false;
                        break;
                    case EnumSubsStatus.UNDEFINED:

                        Status = new WaferUndefinedStatus(this);
                        break;
                    case EnumSubsStatus.NOT_EXIST:

                        Status.SetStatusUnloaded();
                        SetWaferState(EnumWaferState.UNDEFINED);

                        this.WaferDevObject.Info.WaferType = EnumWaferType.UNDEFINED;
                        this.LotOPModule().UpdateWaferID(string.Empty);
                        this.WaferDevObject.Info.SlotIndex.Value = 0;
                        this.LotOPModule().LotInfo.isNewLot = false;
                        this.OnceStopBeforeProbing = false;
                        this.OnceStopAfterProbing = false;
                        this.IsSendfWaferStartEvent = false;
                        this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset = 0; //wafer unload시 Move Z Offset 값을 초기화 하도록 한다.
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DONO_WAFER, true);

                        VirtualStageConnector.Instance.SendTCPCommand(TCPCommand.VIRTUAL_PROBING_OFF);

                        break;
                    case EnumSubsStatus.EXIST:

                        if (waferType != EnumWaferType.UNDEFINED)
                        {
                            var info = this.LotOPModule().LotInfo;

                            if (waferType == EnumWaferType.STANDARD)
                            {
                                if (this.LotOPModule().ModuleState != null && this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                                {
                                    info.LoadedWaferCountUntilBeforeDeviceChange += 1;
                                    info.LoadedWaferCountUntilBeforeLotStart += 1;

                                    LoggerManager.Debug($"[{this.GetType().Name}], SetWaferStatus() : LoadedWaferCountUntilBeforeDeviceChange = {info.LoadedWaferCountUntilBeforeDeviceChange}, LoadedWaferCountUntilBeforeLotStart = {info.LoadedWaferCountUntilBeforeLotStart}");
                                }

                                this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.FOUPNUMBER, this.LotOPModule().LotInfo.FoupNumber.Value.ToString());
                            }
                            else if (waferType == EnumWaferType.POLISH)
                            {
                                // TODO : 
                            }

                            this.WaferDevObject.Info.WaferType = waferType;
                            
                            this.LotOPModule().UpdateWaferID(waferID);

                            this.WaferDevObject.Info.SlotIndex.Value = slotNum;

                            int? slotnum = GetSlotIndex();

                            if (slotnum != null)
                            {
                                this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.SLOTNUMBER, slotnum.ToString());
                            }
                            else
                            {
                                this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.SLOTNUMBER, string.Empty);
                            }

                            VirtualStageConnector.Instance.SetWaferType(waferType);
                        }
                        else
                        {
                            LoggerManager.Debug($"[{this.GetType().Name}], SetWaferStatus(), waferType = {waferType}");
                        }

                        if (preStatus != EnumSubsStatus.EXIST)
                        {
                            Status.SetStatusLoaded();
                            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DONO_WAFER, false);
                        }
                        this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset = 0; //wafer load시 Move Z Offset 값을 초기화 하도록 한다.

                        break;
                    case EnumSubsStatus.HIDDEN:
                        // NOTHING
                        break;
                    case EnumSubsStatus.CARRIER:
                        // NOTHING
                        break;
                    default:
                        break;
                }

                var message = $"[{this.GetType().Name}], SetWaferStatus() : {preStatus} => {Status.GetState()}";
                LoggerManager.Debug(message, isInfo: IsInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region State
        public EnumWaferState GetState()
        {
            return State.GetState();
        }
        public void ChangeState(WaferStateBase state)
        {
            State = state;
        }
        public void SetWaferState(EnumWaferState state)
        {
            try
            {
                var preState = State?.GetState() ?? EnumWaferState.UNDEFINED;

                switch (state)
                {
                    case EnumWaferState.UNDEFINED:
                        State = new NullWaferState(this);
                        break;
                    case EnumWaferState.UNPROCESSED:
                        State.SetUnprocessed();
                        this.SoakingModule().SoackingDone = false;
                        break;
                    case EnumWaferState.PROBING:
                        State.SetProcessing();
                        break;
                    case EnumWaferState.TESTED:
                        State.SetTested();
                        break;
                    case EnumWaferState.PROCESSED:
                        State.SetProcessed();
                        break;
                    case EnumWaferState.SKIPPED:
                        State.SetSkipped();
                        break;
                    case EnumWaferState.MISSED:
                        // NOTHING
                        break;
                    case EnumWaferState.CLEANING:
                        State.SetCleaning();
                        break;
                    case EnumWaferState.READY:
                        State.SetReady();
                        break;
                    case EnumWaferState.SOAKINGSUSPEND:
                        State.SetSoakingSuspend();
                        break;
                    case EnumWaferState.SOAKINGDONE:
                        State.SetSoakingDone();
                        break;
                    default:
                        break;
                }

                // TODO : UNDEFINED일 때, 의도적으로 업데이트를 하지 않았나?
                if (state != EnumWaferState.UNDEFINED)
                {
                    // Update slot information
                    this.StageSupervisor().GetSlotInfo().WaferState = state;
                    this.StageSupervisor().SaveSlotInfo();
                }

                var message = $"[{this.GetType().Name}], SetWaferState() : {preState} => {State.GetState()}";
                LoggerManager.Debug(message, isInfo: IsInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        public int? GetSlotIndex()
        {
            int slotcount = 25;

            int? retval = null;

            try
            {
                retval = WaferDevObject.Info.SlotIndex.Value;

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    retval = WaferDevObject.Info.SlotIndex.Value % slotcount;

                    if (retval == 0)
                    {
                        retval = slotcount;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public int GetOriginFoupNumber()
        {
            int ret = 0;
            try
            {
                if (this.GetParam_Wafer()?.GetSubsInfo() != null)
                {
                    int slotNum = this.GetParam_Wafer().GetSubsInfo().SlotIndex.Value % 25;
                    int offset = 0;

                    if (slotNum == 0)
                    {
                        slotNum = 25;
                        offset = -1;
                    }

                    ret = (((this.GetParam_Wafer().GetSubsInfo().SlotIndex.Value + offset) / 25) + 1);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private void InitMap()
        {
            try
            {
                WaferDevObject.InitMap();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                WaferGraphicsContext = new WaferGraphicsModule();
                WaferGraphicsContext.InitModule();

                CurrentUIndex = new UserIndex();

                if (CurrentMXIndex == 0 || CurrentMYIndex == 0)
                {
                    CurrentMXIndex = WaferDevObject.PhysInfo.MapCountX.Value / 2;
                    CurrentMYIndex = WaferDevObject.PhysInfo.MapCountY.Value / 2;
                }

                MapViewControlMode = MapViewMode.MapMode;
                MapViewStageSyncEnable = true;

                IsMapViewShowPMITable = false;
                IsMapViewShowPMIEnable = false;

                MapViewAssignCamType = new Element<EnumProberCam>();
                MapViewAssignCamType.Value = EnumProberCam.UNDEFINED;

                SetWaferState(EnumWaferState.UNDEFINED);
                SetWaferStatus(EnumSubsStatus.UNDEFINED);

                AlignState = new Element<AlignStateEnum>();
                SetAlignState(AlignStateEnum.IDLE);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public EventCodeEnum UpdateData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                IsMapViewShowPMIEnable = false;
                IsMapViewShowPMITable = false;

                WaferDevObject.SetDefaultParam();
                WaferDevObject.UpdateWaferObject(WaferDevObject.AutoCalWaferMap(true));

                if (CurrentMXIndex == 0 || CurrentMYIndex == 0)
                {
                    CurrentMXIndex = ((int)WaferDevObject.PhysInfo.MapCountX.Value / 2);
                    CurrentMYIndex = ((int)WaferDevObject.PhysInfo.MapCountY.Value / 2);
                }

                InitMap();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;

        }
        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = new WaferDevObject();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(WaferDevObject));
                this.WaferDevObject = tmpParam as WaferDevObject;

                //this.GetPhysInfo().WaferID.Value = WaferDevObject.PhysInfo.WaferID.Value;
                //this.GetPhysInfo().SlotIndex.Value = WaferDevObject.PhysInfo.SlotIndex.Value;

                retVal = this.Init();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(WaferDevObject);

                _ChangedWaferObjectEvent?.Invoke(this, new WaferObjectEventArgs(this));
                GetSubsInfo().WaferObjectChangedToggle.Value = false;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        /// <summary>
        /// SubsInfo 는 건들지 않으려.
        /// </summary>
        /// <param name="source"></param>
        public void CopyForm(WaferObject source)
        {
            try
            {
                this.WaferDevObject.PhysInfo = source.WaferDevObject.PhysInfo;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetDutDieMatchIndexs(List<DutWaferIndex> dutDieMatchIndexs)
        {
            this.WaferDevObject.Info.DutDieMatchIndexs = dutDieMatchIndexs;
        }
        public AlignStateEnum GetAlignState()
        {
            AlignStateEnum state = AlignStateEnum.IDLE;

            try
            {
                if (AlignState.DoneState == ElementStateEnum.DONE)
                {
                    state = AlignStateEnum.DONE;
                }
                else
                {
                    state = AlignStateEnum.IDLE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return state;
        }
        public void SetAlignState(AlignStateEnum state)
        {
            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}], SetAlignState() : {AlignState.Value} => {state}", isInfo: IsInfo);

                if (state != AlignState.Value)
                {
                    this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.WAFERALIGNSTATE, state.ToString());
                }

                AlignState.Value = state;

                switch (state)
                {
                    case AlignStateEnum.IDLE:
                        AlignState.DoneState = ElementStateEnum.DEFAULT;
                        break;
                    case AlignStateEnum.DONE:
                        AlignState.DoneState = ElementStateEnum.DONE;
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetElementMetaData()
        {
            WaferAlignSetupChangedToggle.AssociateElementID = "C2";
            PadSetupChangedToggle.AssociateElementID = "C6,C4";
        }
        public void CallWaferobjectChangedEvent()
        {
            this._ChangedWaferObjectEvent?.Invoke(this, new WaferObjectEventArgs(this));
        }

        #endregion
    }
}
