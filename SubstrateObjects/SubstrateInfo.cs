using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubstrateObjects
{
    using ProberInterfaces.Param;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using ProberInterfaces;
    using System.Runtime.CompilerServices;
    using ProberErrorCode;
    using ProberInterfaces.PMI;
    using Newtonsoft.Json;
    using LogModule;
    using ProbingDataInterface;

    [Serializable]
    public class SubstrateInfo : ISubstrateInfo, INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        public List<object> Nodes { get; set; }


        [NonSerialized]
        private Element<bool> _WaferObjectChangedToggle
            = new Element<bool>();
        [XmlIgnore, JsonIgnore]
        public Element<bool> WaferObjectChangedToggle
        {
            get { return _WaferObjectChangedToggle; }
            set
            {
                if (value != _WaferObjectChangedToggle)
                {
                    _WaferObjectChangedToggle = value;
                    RaisePropertyChanged();
                }
            }
        }



        [NonSerialized]
        private WaferCoordinate _WaferCenter = new WaferCoordinate();
        [XmlIgnore, JsonIgnore]
        //[ParamIgnore]
        public WaferCoordinate WaferCenter
        {
            get { return _WaferCenter; }
            set
            {
                if (value != _WaferCenter)
                {
                    _WaferCenter = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Edge 를 통해 구한 Center 값 ( Edge align 을 할때 업데이트 됨 )
        /// </summary>
        [NonSerialized]
        private WaferCoordinate _WaferCenterOriginatEdge = new WaferCoordinate();
        [XmlIgnore, JsonIgnore]
        public WaferCoordinate WaferCenterOriginatEdge
        {
            get { return _WaferCenterOriginatEdge; }
            set
            {
                if (value != _WaferCenterOriginatEdge)
                {
                    _WaferCenterOriginatEdge = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _LoadingAngle;
        [XmlIgnore, JsonIgnore]
        public double LoadingAngle
        {
            get { return _LoadingAngle; }
            set
            {
                if (value != _LoadingAngle)
                {
                    _LoadingAngle = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _ContactCount;
        [XmlIgnore, JsonIgnore]
        public double ContactCount
        {
            get { return _ContactCount; }
            set
            {
                if (value != _ContactCount)
                {
                    _ContactCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        //=> WaferMaterialEnum
        private EnumWaferType _WaferType;
        [XmlIgnore, JsonIgnore]
        public EnumWaferType WaferType
        {
            get { return _WaferType; }
            set
            {
                _WaferType = value;
                RaisePropertyChanged();
            }
        }

        [NonSerialized]
        private IDeviceObject[,] _DIEs;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public IDeviceObject[,] DIEs
        {
            get { return _DIEs; }
            set { _DIEs = value; }
        }

        [NonSerialized]
        private SynchronizedObservableCollection<DeviceObject> _PMIDIEs;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public SynchronizedObservableCollection<DeviceObject> PMIDIEs
        {
            get { return _PMIDIEs; }
            set { _PMIDIEs = value; }
        }

        [NonSerialized]
        private DeviceObject _CurrentDie;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public DeviceObject CurrentDie
        {
            get { return _CurrentDie; }
            set { _CurrentDie = value; }
        }

        [NonSerialized]
        private Element<int> _SequenceProcessedCount = new Element<int>();
        [XmlIgnore, JsonIgnore]
        public Element<int> SequenceProcessedCount
        {
            get { return _SequenceProcessedCount; }
            set { _SequenceProcessedCount = value; }
        }

        [NonSerialized]
        private RectSize _ActualDieSize = new RectSize();
        [XmlIgnore, JsonIgnore]
        public RectSize ActualDieSize
        {
            get { return _ActualDieSize; }
            set { _ActualDieSize = value; }
        }

        [NonSerialized]
        private RectSize _ActualDeviceSize = new RectSize();
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public RectSize ActualDeviceSize
        {
            get { return _ActualDeviceSize; }
            set { _ActualDeviceSize = value; }
        }


        [NonSerialized]
        private WaferCoordinate _WaferCenterOffset = new WaferCoordinate();
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public WaferCoordinate WaferCenterOffset
        {
            get { return _WaferCenterOffset; }
            set { _WaferCenterOffset = value; }
        }

        [NonSerialized]
        private double _MachineCoordZeroPosX;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public double MachineCoordZeroPosX
        {
            get { return _MachineCoordZeroPosX; }
            set
            {
                if (value != _MachineCoordZeroPosX)
                {
                    _MachineCoordZeroPosX = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _MachineCoordZeroPosY;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public double MachineCoordZeroPosY
        {
            get { return _MachineCoordZeroPosY; }
            set
            {
                if (value != _MachineCoordZeroPosY)
                {
                    _MachineCoordZeroPosY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _DieXClearance = new Element<double>();
        public Element<double> DieXClearance
        {
            get { return _DieXClearance; }
            set
            {
                if (value != _DieXClearance)
                {
                    _DieXClearance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _DieYClearance = new Element<double>();
        public Element<double> DieYClearance
        {
            get { return _DieYClearance; }
            set
            {
                if (value != _DieYClearance)
                {
                    _DieYClearance = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<double> _WaferSequareness = new Element<double>();
        [XmlIgnore, JsonIgnore]
        public Element<double> WaferSequareness
        {
            get { return _WaferSequareness; }
            set
            {
                if (value != _WaferSequareness)
                {
                    _WaferSequareness = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<double> _MachineSequareness = new Element<double>();
        [XmlIgnore, JsonIgnore]
        public Element<double> MachineSequareness
        {
            get { return _MachineSequareness; }
            set
            {
                if (value != _MachineSequareness)
                {
                    _MachineSequareness = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _AveWaferThick;
        [XmlIgnore, JsonIgnore]
        public double AveWaferThick
        {
            get { return _AveWaferThick; }
            set
            {
                if (value != _AveWaferThick)
                {
                    _AveWaferThick = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _ActualThickness;
        [XmlIgnore, JsonIgnore]
        public double ActualThickness
        {
            get { return _ActualThickness; }
            set
            {
                if (value != _ActualThickness)
                {
                    _ActualThickness = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private WaferCoordinate _RefDieLeftCorner = new WaferCoordinate();
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public WaferCoordinate RefDieLeftCorner
        {
            get { return _RefDieLeftCorner; }
            set
            {
                if (value != _RefDieLeftCorner)
                {
                    _RefDieLeftCorner = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private bool _isProbingDone;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public bool isProbingDone
        {
            get { return _isProbingDone; }
            set
            {
                if (value != _isProbingDone)
                {
                    _isProbingDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private string _OperatorID;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public string OperatorID
        {
            get { return _OperatorID; }
            set
            {
                if (value != _OperatorID)
                {
                    _OperatorID = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private long _OperatorLevel;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public long OperatorLevel
        {
            get { return _OperatorLevel; }
            set
            {
                if (value != _OperatorLevel)
                {
                    _OperatorLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private int _CPCount;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public int CPCount
        {
            get { return _CPCount; }
            set
            {
                if (value != _CPCount)
                {
                    _CPCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private int _RetestCount;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public int RetestCount
        {
            get { return _RetestCount; }
            set
            {
                if (value != _RetestCount)
                {
                    _RetestCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<long> _TestedDieCount = new Element<long>();
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public Element<long> TestedDieCount
        {
            get { return _TestedDieCount; }
            set
            {
                if (value != _TestedDieCount)
                {
                    _TestedDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<long> _PassedDieCount = new Element<long>();
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public Element<long> PassedDieCount
        {
            get { return _PassedDieCount; }
            set
            {
                if (value != _PassedDieCount)
                {
                    _PassedDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<long> _FailedDieCount = new Element<long>();
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public Element<long> FailedDieCount
        {
            get { return _FailedDieCount; }
            set
            {
                if (value != _FailedDieCount)
                {
                    _FailedDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }


        [NonSerialized]
        private Element<long> _CurTestedDieCount = new Element<long>();
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public Element<long> CurTestedDieCount
        {
            get { return _CurTestedDieCount; }
            set
            {
                if (value != _CurTestedDieCount)
                {
                    _CurTestedDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<long> _CurPassedDieCount = new Element<long>();
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public Element<long> CurPassedDieCount
        {
            get { return _CurPassedDieCount; }
            set
            {
                if (value != _CurPassedDieCount)
                {
                    _CurPassedDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<long> _CurFailedDieCount = new Element<long>();
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public Element<long> CurFailedDieCount
        {
            get { return _CurFailedDieCount; }
            set
            {
                if (value != _CurFailedDieCount)
                {
                    _CurFailedDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _Yield;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
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

        [NonSerialized]
        private double _RetestYield;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
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

        [NonSerialized]
        private float _ChuckTemperature;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public float ChuckTemperature
        {
            get { return _ChuckTemperature; }
            set
            {
                if (value != _ChuckTemperature)
                {
                    _ChuckTemperature = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private DateTime _ProbingStartTime;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public DateTime ProbingStartTime
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

        [NonSerialized]
        private DateTime _ProbingEndTime;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public DateTime ProbingEndTime
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

        [NonSerialized]
        private DateTime _LoadingTime;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public DateTime LoadingTime
        {
            get { return _LoadingTime; }
            set
            {
                if (value != _LoadingTime)
                {
                    _LoadingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private DateTime _UnloadingTime;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public DateTime UnloadingTime
        {
            get { return _UnloadingTime; }
            set
            {
                if (value != _UnloadingTime)
                {
                    _UnloadingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IList<DeviceObject> _Devices = new List<DeviceObject>();
        [SharePropPath]
        public IList<DeviceObject> Devices
        {
            get { return _Devices; }
            set { _Devices = value; }
        }

        private Element<string> _WaferID = new Element<string>();
        public Element<string> WaferID
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

        private Element<string> _OperatorName = new Element<string>();
        public Element<string> OperatorName
        {
            get { return _OperatorName; }
            set
            {
                if (value != _OperatorName)
                {
                    _OperatorName = value;
                    RaisePropertyChanged();
                }
            }
        }


        private PadGroup _Pads = new PadGroup();
        public PadGroup Pads
        {
            get { return _Pads; }
            set { _Pads = value; }
        }

        private PMIInfo _PMIInfo = new PMIInfo();
        [SharePropPath]
        public PMIInfo PMIInfo
        {
            get { return _PMIInfo; }
            set { _PMIInfo = value; }
        }


        private List<DutWaferIndex> _DutDieMatchIndexs;

        public List<DutWaferIndex> DutDieMatchIndexs
        {
            get { return _DutDieMatchIndexs; }
            set { _DutDieMatchIndexs = value; }
        }

        private List<int> _TestDutNum = new List<int>();
        [ParamIgnore]
        public List<int> TestDutNum
        {
            get { return _TestDutNum; }
            set
            {
                if (value != _TestDutNum)
                {
                    _TestDutNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<long> _TestDieCount = new Element<long>();
        public Element<long> TestDieCount
        {
            get { return _TestDieCount; }
            set
            {
                if (value != _TestDieCount)
                {
                    _TestDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<long> _MarkDieCount = new Element<long>();
        public Element<long> MarkDieCount
        {
            get { return _MarkDieCount; }
            set
            {
                if (value != _MarkDieCount)
                {
                    _MarkDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _DeviceName = new Element<string>();
        public Element<string> DeviceName
        {
            get { return _DeviceName; }
            set
            {
                if (value != _DeviceName)
                {
                    _DeviceName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private byte _CassetteNo;
        public byte CassetteNo
        {
            get { return _CassetteNo; }
            set
            {
                if (value != _CassetteNo)
                {
                    _CassetteNo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CassetteID;
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

        private Element<DateTime> _LoadTime = new Element<DateTime>();
        public Element<DateTime> LoadTime
        {
            get { return _LoadTime; }
            set { _LoadTime = value; }
        }


        private Element<DateTime> _UnloadTime = new Element<DateTime>();
        public Element<DateTime> UnloadTime
        {
            get { return _UnloadTime; }
            set { _UnloadTime = value; }
        }

        private Element<CategoryStatsBase> _CatStatistics = new Element<CategoryStatsBase>();
        public Element<CategoryStatsBase> CatStatistics
        {
            get
            {
                return _CatStatistics;
            }
            set
            {
                if (_CatStatistics != value)
                {
                    _CatStatistics = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _SlotIndex = new Element<int>();
        public Element<int> SlotIndex
        {
            get { return _SlotIndex; }
            set { _SlotIndex = value; }
        }

        [NonSerialized]
        private double _DutCenX;
        [ParamIgnore, JsonIgnore]
        public double DutCenX
        {
            get { return _DutCenX; }
            set
            {
                if (value != _DutCenX)
                {
                    _DutCenX = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _DutCenY;
        [ParamIgnore, JsonIgnore]
        public double DutCenY
        {
            get { return _DutCenY; }
            set
            {
                if (value != _DutCenY)
                {
                    _DutCenY = value;
                    RaisePropertyChanged();
                }
            }
        }


        //pad 포커싱 위치로 이동 시 offset을 적용하기 위한 값
        //현재 위치로 - ZBasePosForZOffset 의 차이 값
        //wafer inspection 화면에서만 우선 사용

        [NonSerialized]
        private double _MoveZOffset;
        [ParamIgnore, JsonIgnore]
        public double MoveZOffset
        {
            get { return _MoveZOffset; }
            set
            {
                if (value != _MoveZOffset)
                {
                    _MoveZOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SubstrateInfo()
        {
            //this.WaferHeightMapping = new WaferHeightMapping();
        }
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CassetteID = string.Empty;

                if (WaferCenter == null)
                {
                    WaferCenter = new WaferCoordinate();
                }

                if (SequenceProcessedCount == null)
                {
                    SequenceProcessedCount = new Element<int>();
                }

                if (ActualDieSize == null)
                {
                    ActualDieSize = new RectSize();
                }

                if (ActualDeviceSize == null)
                {
                    ActualDeviceSize = new RectSize();
                }

                if (WaferCenterOffset == null)
                {
                    WaferCenterOffset = new WaferCoordinate();
                }

                if (RefDieLeftCorner == null)
                {
                    RefDieLeftCorner = new WaferCoordinate();
                }

                if (TestedDieCount == null)
                {
                    TestedDieCount = new Element<long>();
                }

                if (PassedDieCount == null)
                {
                    PassedDieCount = new Element<long>();
                }

                if (FailedDieCount == null)
                {
                    FailedDieCount = new Element<long>();
                }

                if (WaferID == null)
                {
                    WaferID = new Element<string>();
                }

                if (OperatorName == null)
                {
                    OperatorName = new Element<string>();
                }

                if (Pads.RefPad == null)
                {
                    Pads.RefPad = new PadObject();
                }

                if (PMIDIEs == null)
                {
                    PMIDIEs = new SynchronizedObservableCollection<DeviceObject>();
                }

                retval = UpdatePadsToDevices();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public EventCodeEnum UpdatePadsToDevice(DeviceObject target)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (target.Pads == null) target.Pads = new PadGroup();

                Pads.CopyTo(target.Pads);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum UpdatePadsToDevices()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //// TODO: REMOVE Test code
                //if(Devices.Count > 0)
                //{
                //    //System.Diagnostics.Stopwatch sw = new Stopwatch();
                //    //sw.Reset();         // 초기화
                //    //sw.Start();

                //    if (Devices[0].Pads == null)
                //    {
                //        Devices[0].Pads = new PadGroup();
                //    }

                //    Pads.CopyTo(Devices[0].Pads);

                //    //sw.Stop();          // 종료
                //    //Console.WriteLine("수행 시간 : {0}", sw.ElapsedMilliseconds / 1000.0F);
                //}

                Parallel.For(0, Devices.Count, i =>
                {
                    if (Devices[i].Pads == null) Devices[i].Pads = new PadGroup();

                    Pads.CopyTo(Devices[i].Pads);
                });

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum UpdateCurrentDieCount()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CurPassedDieCount.Value = 0;
                CurFailedDieCount.Value = 0;
                CurTestedDieCount.Value = 0;

                foreach (var item in Devices)
                {
                    if (item != null)
                    {
                        if (item.State.Value == DieStateEnum.TESTED)
                        {
                            // TODO : BIN CODE = 1이 항상 PASS를 의미하는 것이 아니다. 
                            // BIN 설정에 의해 동작하도록 로직이 추가되어야 한다.

                            if (item.CurTestHistory.TestResult.Value == TestState.MAP_STS_PASS)
                            {
                                CurPassedDieCount.Value++;
                            }
                            else
                            {
                                CurFailedDieCount.Value++;
                            }

                            //if (item.CurTestHistory.BinCode.Value == 1)
                            //{
                            //    waferSubstrateInfo.CurPassedDieCount.Value++;
                            //}
                            //else
                            //{
                            //    waferSubstrateInfo.CurFailedDieCount.Value++;
                            //}

                            CurTestedDieCount.Value++;
                        }
                        else
                        {
                        }
                    }
                }

                //Parallel.For(0, waferSubstrateInfo.Devices.Count, i =>
                //{
                //    if (waferSubstrateInfo.Devices[i] != null)
                //    {
                //        if (waferSubstrateInfo.Devices[i].State.Value == DieStateEnum.TESTED)
                //        {
                //            // TODO : BIN CODE = 1이 항상 PASS를 의미하는 것이 아니다. 
                //            // BIN 설정에 의해 동작하도록 로직이 추가되어야 한다.

                //            if (waferSubstrateInfo.Devices[i].CurTestHistory.BinCode.Value == 1)
                //            {
                //                waferSubstrateInfo.CurPassedDieCount.Value++;
                //            }
                //            else
                //            {
                //                waferSubstrateInfo.CurFailedDieCount.Value++;
                //            }

                //            waferSubstrateInfo.CurTestedDieCount.Value++;
                //        }
                //        else
                //        {
                //            testc2++;
                //        }

                //        testc1++;
                //    }
                //    else
                //    {

                //    }
                //});

                //Parallel.For(0, this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value.Count, i =>
                //{
                //    var mi = this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value[i];
                //    var die = waferSubstrateInfo.DIEs[mi.XIndex, mi.YIndex];

                //    if (die != null)
                //    {
                //        if (die.State.Value == DieStateEnum.TESTED)
                //        {
                //            // TODO : BIN CODE = 1이 항상 PASS를 의미하는 것이 아니다. 
                //            // BIN 설정에 의해 동작하도록 로직이 추가되어야 한다.

                //            if (die.CurTestHistory.BinCode.Value == 1)
                //            {
                //                waferSubstrateInfo.CurPassedDieCount.Value++;
                //            }
                //            else
                //            {
                //                waferSubstrateInfo.CurFailedDieCount.Value++;
                //            }
                //            waferSubstrateInfo.CurTestedDieCount.Value++;

                //        }
                //    }
                //});

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum UpdateYield()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                double passedcount = this.CurPassedDieCount.Value;
                double failedcount = this.CurFailedDieCount.Value;
                this.Yield = Math.Round((passedcount / (passedcount + failedcount)), 6) * 100;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public IPMIInfo GetPMIInfo()
        {
            return PMIInfo;
        }

        public void SetPMIInfo(IPMIInfo pmiinfo)
        {
            PMIInfo = pmiinfo as PMIInfo;

            if (PMIInfo == null)
            {
                LoggerManager.Error($"[SubstrateInfo] SetPMIInfo() : Input value is not PMIInfo.");
            }
        }

        public SubstrateInfo(double xpos, double ypos)
        {

        }

        public void DefaultSetting()
        {
        }

    }
}
