using LogModule;
using Newtonsoft.Json;

using ProberInterfaces.Param;
using ProbingDataInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ProberInterfaces
{
    [Serializable, DataContract]
    public class ExistSeqs
    {
        private long _XIndex;
        [DataMember]
        public long XIndex
        {
            get { return _XIndex; }
            set
            {
                if (value != _XIndex)
                {
                    _XIndex = value;
                }
            }
        }

        private long _YIndex;
        [DataMember]
        public long YIndex
        {
            get { return _YIndex; }
            set
            {
                if (value != _YIndex)
                {
                    _YIndex = value;
                }
            }
        }

        private List<bool> _ExistSeq;
        [DataMember]
        public List<bool> ExistSeq
        {
            get { return _ExistSeq; }
            set
            {
                if (value != _ExistSeq)
                {
                    _ExistSeq = value;
                }
            }
        }
    }

    public interface IDeviceObject
    {
        PadGroup Pads { get; set; }
        RectSize Size { get; set; }
        CatCoordinates Position { get; set; }
        UserIndex DieIndex { get; set; }
        MachineIndex DieIndexM { get; set; }
        Element<DieStateEnum> State { get; set; }
        Element<DieTypeEnum> DieType { get; set; }
        Element<SubstrateTypeEnum> SubstrateType { get; set; }
        Element<WaferSubstrateTypeEnum> WaferSubstrateType { get; set; }
        //Element<double> TestTemp { get; set; }
        TestHistory CurTestHistory { get; set; }
        List<TestHistory> TestHistory { get; set; }
        List<IDeviceObject> GetChildren();
        int DutNumber { get; set; }
        List<bool> ExistSeq { get; set; }

        bool NeedTest { get; set; }
    }

    [Serializable, DataContract]
    public class RectSize : IParamNode
    {

        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
        [XmlIgnore, JsonIgnore]
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


        private Element<double> _Width = new Element<double>();
        [XmlAttribute("Width")]
        [DataMember]
        public Element<double> Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        private Element<double> _Height = new Element<double>();
        [XmlAttribute("Height")]
        [DataMember]
        public Element<double> Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        public RectSize()
        {
        }

        public RectSize(double width, double height)
        {
            try
            {
                _Width.Value = width;
                _Height.Value = height;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
    public enum DieStateEnum
    {
        UNKNOWN = -1,
        NOT_EXIST = 0,
        NORMAL = 1,
        TESTING = 2,
        TESTED = 3,
        MARK = 4,
        SKIPPED = 5,
    }
    public enum PMIDieStateEnum
    {
        NONE = 0,
        PROCESSED = 1,
    }
    [Serializable]
    public enum DieTypeEnum
    {
        UNKNOWN = -1,
        NOT_EXIST = 0,
        TEST_DIE,
        SKIP_DIE,
        MARK_DIE,
        //PASS_DIE,
        //FAIL_DIE,
        CUR_DIE,
        TEACH_DIE,
        SAMPLE_DIE,
        CHANGETEST_DIE,
        CHANGEMARK_DIE,
        CONFIRM_MARK_DIE,
        CONFIRM_TEST_DIE,
        MODIFY_DIE,
        TARGET_DIE,
    }
    public enum SubstrateTypeEnum
    {
        UNDEFINED = -1,
        Wafer = 0,
        //Frame,
        //Strip,
        //Tray,
        Card
    }
    public enum WaferSubstrateTypeEnum
    {
        UNDEFINED = -1,
        Normal = 0, // threeleg
        Thin, // Bernoulli
        Framed,
        Strip,
        Tray
    }

    [Serializable]
    [DataContract]
    public enum SubstrateSizeEnum
    {
        [EnumMember]
        INVALID = -1,
        [EnumMember]
        UNDEFINED = 0,
        [EnumMember]
        INCH6 = UNDEFINED + 1,
        [EnumMember]
        INCH8,
        [EnumMember]
        INCH12,
        [EnumMember]
        CUSTOM,
    }

    [Serializable, DataContract]
    public class DeviceObject : DeviceObjectBase, IParamNode
    {


        protected DeviceObject(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
        public DeviceObject() : base()
        {
            //Groups = new ObservableCollection<DeviceGroup>();
        }

        private DeviceGroup _SubDevice = new DeviceGroup();
        [DataMember]
        public DeviceGroup SubDevice
        {
            get { return _SubDevice; }
            set { _SubDevice = value; }
        }
        //private ObservableCollection<DeviceGroup> _Groups;
        //public ObservableCollection<DeviceGroup> Groups
        //{
        //    get { return _Groups; }
        //    set
        //    {
        //        if (value != _Groups)
        //        {
        //            _Groups = value;
        //            NotifyPropertyChanged("Groups");
        //        }
        //    }
        //}
        public override List<IDeviceObject> GetChildren()
        {
            List<IDeviceObject> devs = new List<IDeviceObject>();

            try
            {
                devs = SubDevice.Children.ToList<IDeviceObject>();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return devs;
        }
        public IDeviceObject SubDev(long xindex, long yindex)
        {
            SubDieObject dev = null;
            try
            {
                dev = SubDevice.Children.ToList().Find(
                    d => d.DieIndex.XIndex == xindex & d.DieIndex.YIndex == yindex);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "Error occurred while query device.");
                LoggerManager.Exception(err);

            }
            return dev;
        }
        public override void CopyTo(IDeviceObject target)
        {
            try
            {
                DeviceObject targetDev = (DeviceObject)target;
                base.CopyTo(target);

                targetDev.SubDevice = new DeviceGroup();
                if (this.SubDevice != null)
                {
                    this.SubDevice.CopyTo(targetDev.SubDevice);
                }

                //if (this.Groups != null)
                //{
                //    targetDev.Groups = new ObservableCollection<DeviceGroup>();
                //    if (this.Groups.Count > 0)
                //    {
                //        for (int i = 0; i < this.Groups.Count; i++)
                //        {
                //            DeviceGroup devgrp = new DeviceGroup();
                //            this.Groups[i].CopyTo(devgrp);
                //            targetDev.Groups.Add(devgrp);
                //        }
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

    }

    [Serializable, DataContract]
    public class SubDieObject : DeviceObjectBase
    {


        protected SubDieObject(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
        public SubDieObject() : base()
        {

        }

        public override List<IDeviceObject> GetChildren()
        {
            return null;
        }
    }

    [Serializable]
    public class TestHistory : IParamNode, INotifyPropertyChanged
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

        [XmlIgnore, JsonIgnore]
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
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }


        private Element<string> _BinData = new Element<string>();
        public Element<string> BinData
        {
            get { return _BinData; }
            set
            {
                if (value != _BinData)
                {
                    _BinData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _BinCode = new Element<int>();
        public Element<int> BinCode
        {
            get { return _BinCode; }
            set
            {
                if (value != _BinCode)
                {
                    _BinCode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _DutIndex = new Element<int>();
        public Element<int> DutIndex
        {
            get { return _DutIndex; }
            set
            {
                if (value != _DutIndex)
                {
                    _DutIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<TestState> _TestResult = new Element<TestState>();
        public Element<TestState> TestResult
        {
            get { return _TestResult; }
            set
            {
                if (value != _TestResult)
                {
                    _TestResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _TestTemp = new Element<double>();
        public Element<double> TestTemp
        {
            get { return _TestTemp; }
            set
            {
                if (value != _TestTemp)
                {
                    _TestTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _Inkded = new Element<bool>();
        public Element<bool> Inkded
        {
            get { return _Inkded; }
            set
            {
                if (value != _Inkded)
                {
                    _Inkded = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _XWaferCenterDistance = new Element<double>();
        public Element<double> XWaferCenterDistance
        {
            get { return _XWaferCenterDistance; }
            set
            {
                if (value != _XWaferCenterDistance)
                {
                    _XWaferCenterDistance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _YWaferCenterDistance = new Element<double>();
        public Element<double> YWaferCenterDistance
        {
            get { return _YWaferCenterDistance; }
            set
            {
                if (value != _YWaferCenterDistance)
                {
                    _YWaferCenterDistance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _Overdrive = new Element<double>();
        public Element<double> Overdrive
        {
            get { return _Overdrive; }
            set
            {
                if (value != _Overdrive)
                {
                    _Overdrive = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<DateTime> _StartTime = new Element<DateTime>();
        public Element<DateTime> StartTime
        {
            get { return _StartTime; }
            set
            {
                if (value != _StartTime)
                {
                    _StartTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<DateTime> _TestedTime = new Element<DateTime>();
        public Element<DateTime> EndTime
        {
            get { return _TestedTime; }
            set
            {
                if (value != _TestedTime)
                {
                    _TestedTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _FailMarkInspection = new Element<bool>();
        public Element<bool> FailMarkInspection
        {
            get { return _FailMarkInspection; }
            set
            {
                if (value != _FailMarkInspection)
                {
                    _FailMarkInspection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _NeedleMarkInspection = new Element<bool>();
        public Element<bool> NeedleMarkInspection
        {
            get { return _NeedleMarkInspection; }
            set
            {
                if (value != _NeedleMarkInspection)
                {
                    _NeedleMarkInspection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _NeedleCleaning = new Element<bool>();
        public Element<bool> NeedleCleaning
        {
            get { return _NeedleCleaning; }
            set
            {
                if (value != _NeedleCleaning)
                {
                    _NeedleCleaning = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _NeedleAlign = new Element<bool>();
        public Element<bool> NeedleAlign
        {
            get { return _NeedleAlign; }
            set
            {
                if (value != _NeedleAlign)
                {
                    _NeedleAlign = value;
                    RaisePropertyChanged();
                }
            }
        }

        public TestHistory()
        {
        }

        public void CopyTo(TestHistory target)
        {
            try
            {
                target.BinCode.Value = this.BinCode.Value;
                target.DutIndex.Value = this.DutIndex.Value;
                target.TestTemp.Value = this.TestTemp.Value;
                target.Inkded.Value = this.Inkded.Value;
                target.XWaferCenterDistance.Value = this.XWaferCenterDistance.Value;
                target.YWaferCenterDistance.Value = this.YWaferCenterDistance.Value;
                target.Overdrive.Value = this.Overdrive.Value;
                target.StartTime.Value = this.StartTime.Value;
                target.EndTime.Value = this.EndTime.Value;
                target.FailMarkInspection.Value = this.FailMarkInspection.Value;
                target.NeedleMarkInspection.Value = this.NeedleMarkInspection.Value;
                target.NeedleMarkInspection.Value = this.NeedleMarkInspection.Value;
                target.NeedleAlign.Value = this.NeedleAlign.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
    [Serializable, DataContract]
    public abstract class DeviceObjectBase : IDeviceObject, INotifyPropertyChanged, IParamNode
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

        [XmlIgnore, JsonIgnore]
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

        /// <summary>
        /// User Index에 대응되는 값. 
        /// </summary>
        private UserIndex _DieIndex = new UserIndex();
        [DataMember]
        public UserIndex DieIndex
        {
            get { return _DieIndex; }
            set { _DieIndex = value; }
        }


        /// <summary>
        /// Machine Index에 대응되는 값. 
        /// </summary>
        private MachineIndex _DieIndexM = new MachineIndex();
        [DataMember]
        public MachineIndex DieIndexM
        {
            get { return _DieIndexM; }
            set { _DieIndexM = value; }
        }

        private CatCoordinates _Position = new CatCoordinates();
        [DataMember]
        public CatCoordinates Position
        {
            get { return _Position; }
            set { _Position = value; }
        }

        private RectSize _Size = new RectSize();
        [DataMember]
        public RectSize Size
        {
            get { return _Size; }
            set { _Size = value; }
        }

        private Element<DieStateEnum> _State = new Element<DieStateEnum>(); /* DieStateEnum.UNKNOWN;*/
        [XmlAttribute("State")]
        [DataMember]
        public Element<DieStateEnum> State
        {
            get { return _State; }
            set { _State = value; }
        }

        private Element<DieTypeEnum> _DieType = new Element<DieTypeEnum>(); /*DieTypeEnum.UNKNOWN;*/
        [XmlAttribute("DieType")]
        [DataMember]
        public Element<DieTypeEnum> DieType
        {
            get { return _DieType; }
            set { _DieType = value; }
        }

        private Element<SubstrateTypeEnum> _SubstrateType = new Element<SubstrateTypeEnum>(); /* SubstrateTypeEnum.UNDEFINED;*/
        [XmlAttribute("SubstrateTypeEnum")]
        [DataMember]
        public Element<SubstrateTypeEnum> SubstrateType
        {
            get { return _SubstrateType; }
            set { _SubstrateType = value; }
        }

        private Element<WaferSubstrateTypeEnum> _WaferSubstrateType = new Element<WaferSubstrateTypeEnum>(); /* WaferSubstrateTypeEnum.UNDEFINED;*/
        [XmlAttribute("WaferSubstrateTypeEnum")]
        [DataMember]
        public Element<WaferSubstrateTypeEnum> WaferSubstrateType
        {
            get { return _WaferSubstrateType; }
            set { _WaferSubstrateType = value; }
        }

        //private Element<double> _TestTemp;
        //[XmlAttribute("TestTemp")]
        //public Element<double> TestTemp
        //{
        //    get { return _TestTemp; }
        //    set { _TestTemp = value; }
        //}

        private TestHistory _CurTestHistory = new ProberInterfaces.TestHistory();
        public TestHistory CurTestHistory
        {
            get { return _CurTestHistory; }
            set
            {
                if (value != _CurTestHistory)
                {
                    _CurTestHistory = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<TestHistory> _TestHistory = new List<TestHistory>();
        [SharePropPath]
        public List<TestHistory> TestHistory
        {
            get { return _TestHistory; }
            set
            {
                if (value != _TestHistory)
                {
                    _TestHistory = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _PMIEnable = new Element<bool>();
        public Element<bool> PMIEnable
        {
            get { return _PMIEnable; }
            set
            {
                if (value != _PMIEnable)
                {
                    _PMIEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _PMITableIndex = new Element<int>();
        public Element<int> PMITableIndex
        {
            get { return _PMITableIndex; }
            set
            {
                if (value != _PMITableIndex)
                {
                    _PMITableIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private PadGroup _Pads;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore, DataMember]
        public PadGroup Pads
        {
            get { return _Pads; }
            set { _Pads = value; }
        }

        private int _DutNumber;
        [DataMember]
        public int DutNumber
        {
            get { return _DutNumber; }
            set
            {
                if (value != _DutNumber)
                {
                    _DutNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private List<bool> _ExistSeq = new List<bool>();
        [XmlIgnore, JsonIgnore, ParamIgnore, DataMember]
        public List<bool> ExistSeq
        {
            get { return _ExistSeq; }
            set
            {
                if (value != _ExistSeq)
                {
                    _ExistSeq = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private bool _NeedTest = new bool();
        [XmlIgnore, JsonIgnore, ParamIgnore, DataMember]
        public bool NeedTest
        {
            get { return _NeedTest; }
            set
            {
                if (value != _NeedTest)
                {
                    _NeedTest = value;
                    RaisePropertyChanged();
                }
            }
        }

        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        public abstract List<IDeviceObject> GetChildren();

        protected DeviceObjectBase(SerializationInfo info, StreamingContext context)
        {

        }
        public DeviceObjectBase()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        public virtual void CopyTo(IDeviceObject target)
        {
            try
            {
                target = new DeviceObject();

                target.DieIndex = new UserIndex(this.DieIndex.XIndex, this.DieIndex.YIndex);
                target.DieIndexM = new MachineIndex(this.DieIndexM.XIndex, this.DieIndexM.YIndex);
                target.Position = new CatCoordinates(this.Position.GetX(), this.Position.GetY(), this.Position.GetZ(), this.Position.GetT());
                target.Size = new RectSize(this.Size.Width.Value, this.Size.Height.Value);
                target.State.Value = this.State.Value;
                target.DieType.Value = this.DieType.Value;
                target.SubstrateType.Value = this.SubstrateType.Value;
                target.WaferSubstrateType.Value = this.WaferSubstrateType.Value;

                this.CurTestHistory.CopyTo(target.CurTestHistory);

                TestHistory.CopyTo(target.TestHistory.ToArray<TestHistory>());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }


    }
    [Serializable, DataContract]
    public class DeviceGroup : INotifyPropertyChanged, IParamNode
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

        [XmlIgnore, JsonIgnore]
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
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        private ObservableCollection<SubDieObject> _Children = new ObservableCollection<SubDieObject>();
        [DataMember]
        public ObservableCollection<SubDieObject> Children
        {
            get { return _Children; }
            set
            {
                if (value != _Children)
                {
                    _Children = value;
                    RaisePropertyChanged();
                }
            }
        }
        protected DeviceGroup(SerializationInfo info, StreamingContext context)
        {

        }
        public DeviceGroup()
        {
        }

        public void CopyTo(DeviceGroup devgroup)
        {
            try
            {
                if (Children != null)
                {
                    if (Children.Count > 0)
                    {
                        SubDieObject child;
                        for (int i = 0; i < Children.Count; i++)
                        {
                            devgroup.Children = new ObservableCollection<SubDieObject>();
                            child = new SubDieObject();
                            Children[i].CopyTo(child);
                            devgroup.Children.Add(child);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }
    }

    public class HighlightDieComponent : INotifyPropertyChanged
    {
        //public static readonly ReadOnlyCollection<string> _MapViewColorBrushResource = new ReadOnlyCollection<string>
        //(new ReadOnlyCollection<string>(new[]
        //{
        //    "backgroundBrush",
        //    "rectBrush",
        //    "passBrush",
        //    "passBrush",
        //    "testingBrush",
        //    "failBrush",
        //    "edgeBrush",
        //    "markBrush",
        //    "markBrush",
        //    "DarkRedBrush",
        //    "changedtestkBrush",
        //    "modifymarkBrush",
        //    "skipBrush",
        //    "textBrush",
        //    "miniMapBackGroundBrus",
        //    "miniMapForeGroundBrus",
        //    "selectedBrush",
        //    "infoBackgroundBrush",
        //    "CurBrush",
        //    "FirstDutBrush",
        //    "lockBrush",
        //    "brownBrush",
        //    "DarkGrayBrush",
        //    "OrangeBrush",
        //    "DarkGreenBrush",
        //    "VioletBrush",
        //    "LightGreenBrush"
        //}));

        //public static ReadOnlyCollection<string> MapViewColorBrushResource
        //{
        //    get { return _MapViewColorBrushResource; }
        //}

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private UserIndex _UI;
        public UserIndex UI
        {
            get { return _UI; }
            set
            {
                if (value != _UI)
                {
                    _UI = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MachineIndex _MI;
        public MachineIndex MI
        {
            get { return _MI; }
            set
            {
                if (value != _MI)
                {
                    _MI = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _BrushAlias;
        public string BrushAlias
        {
            get { return _BrushAlias; }
            set
            {
                if (value != _BrushAlias)
                {
                    _BrushAlias = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
