using Newtonsoft.Json;
using System;
using Command.TCPIP;
using LoaderParameters.Data;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ProberEmulator
{
    public enum DummyDataAssignType
    {
        PROPERTY = 0,
        FUNC
    }

    public abstract class DummyDataAssignBaseComonent
    {
        public DummyDataAssignType AssignType { get; set; }

        public DummyDataAssignBaseComonent(DummyDataAssignType type)
        {
            this.AssignType = type;
        }

        public DummyDataAssignBaseComonent()
        {

        }
    }

    public class DummyDataAssignPropertyType : DummyDataAssignBaseComonent
    {
        public DummyDataAssignPropertyType(string propetyname) : base(DummyDataAssignType.PROPERTY)
        {
            this.Propetyname = propetyname;
        }

        public string Propetyname { get; set; }
    }

    public class DummyDataAssignFuncType : DummyDataAssignBaseComonent
    {
        public DummyDataAssignFuncType() : base(DummyDataAssignType.FUNC)
        {
        }

        public Func<string> Func;
    }

    public class DummyDataModule : IFactoryModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public Dictionary<string, List<DummyDataAssignBaseComonent>> DicDummyDatas { get; set; }

        //private ObservableCollection<IDeviceObject> _UnderDutDevs;
        //public ObservableCollection<IDeviceObject> UnderDutDevs
        //{
        //    get
        //    {
        //        return _UnderDutDevs;
        //    }
        //    set
        //    {
        //        if (_UnderDutDevs != value)
        //        {
        //            _UnderDutDevs = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        public ICamera CurCam { get; set; }

        private DummyTCPIPQueries _dummyTCPIPQueries;
        public DummyTCPIPQueries dummyTCPIPQueries
        {
            get { return _dummyTCPIPQueries; }
            set
            {
                if (value != _dummyTCPIPQueries)
                {
                    _dummyTCPIPQueries = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void InitModule()
        {
            try
            {
                LoadDummyQueryData();

                MakeDicDummydata();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum LoadDummyQueryData()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(DummyTCPIPQueries));

                if (RetVal == EventCodeEnum.NONE)
                {
                    dummyTCPIPQueries = tmpParam as DummyTCPIPQueries;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        private void MakeDicDummydata()
        {
            try
            {

                if (DicDummyDatas == null)
                {
                    DicDummyDatas = new Dictionary<string, List<DummyDataAssignBaseComonent>>();
                }

                // Query

                List<DummyDataAssignBaseComonent> assigncomonent = null;

                DummyDataAssignPropertyType dummyDataAssignPropertyType = null;
                DummyDataAssignFuncType dummyDataAssignFuncType = null;

                //assigncomonent = new List<DummyDataAssignBaseComonent>();
                //dummyDataAssignPropertyType = new DummyDataAssignPropertyType("CurTemp");
                //assigncomonent.Add(dummyDataAssignPropertyType);
                //DicDummyDatas.Add("CurrentChuckTemp?", assigncomonent);

                //assigncomonent = new List<DummyDataAssignBaseComonent>();
                //dummyDataAssignPropertyType = new DummyDataAssignPropertyType("ProberID");
                //assigncomonent.Add(dummyDataAssignPropertyType);
                //DicDummyDatas.Add("ProberID?", assigncomonent);

                //assigncomonent = new List<DummyDataAssignBaseComonent>();
                //dummyDataAssignPropertyType = new DummyDataAssignPropertyType("LotName");
                //assigncomonent.Add(dummyDataAssignPropertyType);
                //DicDummyDatas.Add("LotName?", assigncomonent);

                //assigncomonent = new List<DummyDataAssignBaseComonent>();
                //dummyDataAssignPropertyType = new DummyDataAssignPropertyType("DeviceName");
                //assigncomonent.Add(dummyDataAssignPropertyType);
                //DicDummyDatas.Add("RecipeName?", assigncomonent);

                assigncomonent = new List<DummyDataAssignBaseComonent>();
                dummyDataAssignPropertyType = new DummyDataAssignPropertyType("WaferSizeInch");
                assigncomonent.Add(dummyDataAssignPropertyType);
                DicDummyDatas.Add("WaferSize?", assigncomonent);

                assigncomonent = new List<DummyDataAssignBaseComonent>();
                dummyDataAssignPropertyType = new DummyDataAssignPropertyType("NotchAngle");
                assigncomonent.Add(dummyDataAssignPropertyType);
                DicDummyDatas.Add("OrientationFlatAngle?", assigncomonent);

                //assigncomonent = new List<DummyDataAssignBaseComonent>();
                //dummyDataAssignPropertyType = new DummyDataAssignPropertyType("WaferID");
                //assigncomonent.Add(dummyDataAssignPropertyType);
                //DicDummyDatas.Add("WaferID?", assigncomonent);

                //assigncomonent = new List<DummyDataAssignBaseComonent>();
                //dummyDataAssignFuncType = new DummyDataAssignFuncType();
                //dummyDataAssignFuncType.Func = MakeSlotNumber;

                //assigncomonent.Add(dummyDataAssignFuncType);
                //DicDummyDatas.Add("CurrentSlotNumber?", assigncomonent);

                //assigncomonent = new List<DummyDataAssignBaseComonent>();
                //dummyDataAssignPropertyType = new DummyDataAssignPropertyType("OverDrive");
                //assigncomonent.Add(dummyDataAssignPropertyType);
                //DicDummyDatas.Add("OverDrive?", assigncomonent);

                //assigncomonent = new List<DummyDataAssignBaseComonent>();
                //dummyDataAssignPropertyType = new DummyDataAssignPropertyType("SelectedCoordiante.XIndex");
                //assigncomonent.Add(dummyDataAssignPropertyType);
                //DicDummyDatas.Add("CurrentX?", assigncomonent);

                //assigncomonent = new List<DummyDataAssignBaseComonent>();
                //dummyDataAssignPropertyType = new DummyDataAssignPropertyType("SelectedCoordiante.YIndex");
                //assigncomonent.Add(dummyDataAssignPropertyType);
                //DicDummyDatas.Add("CurrentY?", assigncomonent);

                assigncomonent = new List<DummyDataAssignBaseComonent>();
                dummyDataAssignFuncType = new DummyDataAssignFuncType();
                dummyDataAssignFuncType.Func = MakeCarrierStatusInfo;
                assigncomonent.Add(dummyDataAssignFuncType);
                DicDummyDatas.Add("CarrierStatus?", assigncomonent);

                // Interrupt

                assigncomonent = new List<DummyDataAssignBaseComonent>();
                dummyDataAssignFuncType = new DummyDataAssignFuncType();
                dummyDataAssignFuncType.Func = MakeFoupNumber;
                assigncomonent.Add(dummyDataAssignFuncType);
                DicDummyDatas.Add($"{typeof(LotStart).FullName}", assigncomonent);

                assigncomonent = new List<DummyDataAssignBaseComonent>();
                dummyDataAssignFuncType = new DummyDataAssignFuncType();
                dummyDataAssignFuncType.Func = MakeFoupNumber;
                assigncomonent.Add(dummyDataAssignFuncType);
                dummyDataAssignFuncType = new DummyDataAssignFuncType();
                dummyDataAssignFuncType.Func = null;
                assigncomonent.Add(dummyDataAssignFuncType);
                DicDummyDatas.Add($"{typeof(ChipStart).FullName}", assigncomonent);

                //assigncomonent = new List<DummyDataAssignBaseComonent>();
                //dummyDataAssignFuncType = new DummyDataAssignFuncType();
                //dummyDataAssignFuncType.Func = MakeFoupNumber;
                //assigncomonent.Add(dummyDataAssignFuncType);
                //DicDummyDatas.Add($"{typeof(ChipStart).FullName}", assigncomonent);

                //assigncomonent = new List<DummyDataAssignBaseComonent>();
                //dummyDataAssignPropertyType = new DummyDataAssignPropertyType("ChuckZState");
                //assigncomonent.Add(dummyDataAssignPropertyType);
                //dummyDataAssignPropertyType = new DummyDataAssignPropertyType("SelectedCoordiante.XIndex");
                //assigncomonent.Add(dummyDataAssignPropertyType);
                //dummyDataAssignPropertyType = new DummyDataAssignPropertyType("SelectedCoordiante.YIndex");
                //assigncomonent.Add(dummyDataAssignPropertyType);
                //DicDummyDatas.Add($"{typeof(ZupDone).FullName}", assigncomonent);

                assigncomonent = new List<DummyDataAssignBaseComonent>();
                dummyDataAssignFuncType = new DummyDataAssignFuncType();
                dummyDataAssignFuncType.Func = MakeFoupNumber;
                assigncomonent.Add(dummyDataAssignFuncType);
                DicDummyDatas.Add($"{typeof(LotEndDone).FullName}", assigncomonent);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public string MakeSlotNumber()
        //{
        //    string retval = string.Empty;

        //    try
        //    {
        //        if (dummyTCPIPQueries.SelectedSlot != null)
        //        {
        //            retval = (dummyTCPIPQueries.SelectedSlot.Index + 1).ToString();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        public string MakeFoupNumber()
        {
            string retval = string.Empty;

            try
            {
                if (dummyTCPIPQueries.SelectedFoup != null)
                {
                    retval = (dummyTCPIPQueries.SelectedFoup.Index + 1).ToString();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public string MakeCarrierStatusInfo()
        {
            string retval = string.Empty;

            try
            {
                //Store each carrier status in order of carrier number, and separate with a comma(",")

                //N：without carrier
                //U：with carrier(not tested yet)
                //T：with carrier(under testing) => LotState is 
                //D：with carrier(already tested) => LotState is End

                // foupinfos[0].State => FoupStateEnum (ERROR, LOAD, UNLOAD, EMPTY_CASSETTE)
                // foupinfos[0].LotState => LotStateEnum (Idle, Running, Pause, Error, Done, End, Cancel)

                string FoupStatus = string.Empty;

                string lotstatealias = string.Empty;

                foreach (var foup in dummyTCPIPQueries.Foups)
                {
                    lotstatealias = string.Empty;

                    if (foup.State == FoupStateEnum.LOAD ||
                        foup.State == FoupStateEnum.UNLOAD)
                    {
                        switch (foup.LotState)
                        {
                            case LotStateEnum.Idle:
                            case LotStateEnum.Pause:
                            case LotStateEnum.Error:
                            case LotStateEnum.Done:
                            case LotStateEnum.Cancel:
                                lotstatealias = "U";

                                break;
                            case LotStateEnum.Running:
                                lotstatealias = "T";
                                break;
                            case LotStateEnum.End:
                                lotstatealias = "D";
                                break;
                            default:
                                break;
                        }

                        if (FoupStatus == string.Empty)
                        {
                            FoupStatus = lotstatealias;
                        }
                        else
                        {
                            FoupStatus = FoupStatus + "," + lotstatealias;
                        }
                    }
                    else if (foup.State == FoupStateEnum.EMPTY_CASSETTE)
                    {
                        if (FoupStatus == string.Empty)
                        {
                            FoupStatus = "N";
                        }
                        else
                        {
                            FoupStatus = FoupStatus + "," + "N";
                        }
                    }
                    else
                    {
                        if (FoupStatus == string.Empty)
                        {
                            FoupStatus = "N";
                        }
                        else
                        {
                            FoupStatus = FoupStatus + "," + "N";
                        }
                    }

                    LoggerManager.Debug($"[GetCarrierStatus], Run() : Foup index = {foup.Index}, Foup state = {foup.State}, Lot state = {foup.LotState}");
                }

                retval = FoupStatus;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SaveDummyQueryData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(dummyTCPIPQueries);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class DummyTCPIPQueries : ISystemParameterizable, INotifyPropertyChanged, IParam, IParamNode
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

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //if (SelectedCoordiante == null)
                //{
                //    if (ProbingSequence != null && ProbingSequence.Count > 0)
                //    {
                //        SelectedCoordiante = ProbingSequence.First();
                //    }
                //}

                if (Foups != null)
                {
                    if (Foups.Count > 0)
                    {
                        SelectedFoup = Foups.First();

                        if(SelectedFoup.Slots != null)
                        {
                            SelectedSlot = SelectedFoup.Slots.First();
                            SelectedSlot.IsSelected = true;

                            //SelectedSlot.WaferState = EnumWaferState.UNPROCESSED;

                            foreach (var slot in SelectedFoup.Slots)
                            {
                                slot.WaferState = EnumWaferState.UNPROCESSED;
                            }
                        }
                    }
                }

                //this.LotState = LotOPStateEnum.IDLE;

                //for (int i = 0; i < FoupCount; i++)
                //{
                //    if(Foups == null)
                //    {
                //        Foups = new ObservableCollection<FoupObject>();
                //    }

                //    FoupObject tmpfoup = new FoupObject();

                //    Foups.Add(tmpfoup);
                //}

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }

        [ParamIgnore]
        public string FilePath { get; } = "TCPIP";

        [ParamIgnore]
        public string FileName { get; } = "TCPIPDummyQuery.Json";
        public string Genealogy { get; set; } = "";
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

        private int _CassetteSlotCount;
        [JsonIgnore]
        public int CassetteSlotCount
        {
            get { return _CassetteSlotCount; }
            set
            {
                if (value != _CassetteSlotCount)
                {
                    _CassetteSlotCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _FoupCount = 1;
        [JsonIgnore]
        public int FoupCount
        {
            get { return _FoupCount; }
            set
            {
                if (value != _FoupCount)
                {
                    _FoupCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Element<double> _SetTemp = new Element<double>();
        //public Element<double> SetTemp
        //{
        //    get { return _SetTemp; }
        //    set
        //    {
        //        if (value != _SetTemp)
        //        {
        //            _SetTemp = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        // CurrentChuckTemp?
        //private Element<double> _CurTemp = new Element<double>(30);
        //[DataMember]
        //public Element<double> CurTemp
        //{
        //    get { return _CurTemp; }
        //    set
        //    {
        //        if (value != _CurTemp)
        //        {
        //            _CurTemp = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //// ProberID?
        //private Element<string> _ProberID = new Element<string>();
        //public Element<string> ProberID
        //{
        //    get { return _ProberID; }
        //    set
        //    {
        //        if (value != _ProberID)
        //        {
        //            _ProberID = value;
        //        }
        //    }
        //}

        // LotName?
        //private Element<string> _LotName = new Element<string>();
        //public Element<string> LotName
        //{
        //    get { return _LotName; }
        //    set
        //    {
        //        if (value != _LotName)
        //        {
        //            _LotName = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //// RecipeName?
        //private Element<string> _DeviceName = new Element<string>();
        //public Element<string> DeviceName
        //{
        //    get { return _DeviceName; }
        //    set
        //    {
        //        if (value != _DeviceName)
        //        {
        //            _DeviceName = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        // WaferSize?
        private Element<double> _WaferSizeInch = new Element<double>();
        public Element<double> WaferSizeInch
        {
            get { return _WaferSizeInch; }
            set
            {
                if (value != _WaferSizeInch)
                {
                    _WaferSizeInch = value;
                    RaisePropertyChanged();
                }
            }
        }

        // OrientationFlatAngle?
        private Element<double> _NotchAngle = new Element<double>();
        public Element<double> NotchAngle
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

        private Element<WaferNotchTypeEnum> _NotchType = new Element<WaferNotchTypeEnum>();
        public Element<WaferNotchTypeEnum> NotchType
        {
            get { return _NotchType; }
            set
            {
                if (value != _NotchType)
                {
                    _NotchType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _NotchAngleOffset = new Element<double>();
        public Element<double> NotchAngleOffset
        {
            get { return _NotchAngleOffset; }
            set
            {
                if (value != _NotchAngleOffset)
                {
                    _NotchAngleOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        //// WaferID?
        //private Element<string> _WaferID = new Element<string>();
        //public Element<string> WaferID
        //{
        //    get { return _WaferID; }
        //    set
        //    {
        //        if (value != _WaferID)
        //        {
        //            _WaferID = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        // CarrierStatus?
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

        //private Element<double> _OverDrive = new Element<double>();
        //public Element<double> OverDrive
        //{
        //    get { return _OverDrive; }
        //    set
        //    {
        //        if (value != _OverDrive)
        //        {
        //            _OverDrive = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Element<double> _DieSizeX = new Element<double>();
        public Element<double> DieSizeX
        {
            get { return _DieSizeX; }
            set
            {
                if (value != _DieSizeX)
                {
                    _DieSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _DieSizeY = new Element<double>();
        public Element<double> DieSizeY
        {
            get { return _DieSizeY; }
            set
            {
                if (value != _DieSizeY)
                {
                    _DieSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _WaferEdgeMargin = new Element<double>();
        public Element<double> WaferEdgeMargin
        {
            get { return _WaferEdgeMargin; }
            set
            {
                if (value != _WaferEdgeMargin)
                {
                    _WaferEdgeMargin = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Element<BinType> _BinType = new Element<BinType>();
        //public Element<BinType> BinType
        //{
        //    get { return _BinType; }
        //    set
        //    {
        //        if (value != _BinType)
        //        {
        //            _BinType = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private ObservableCollection<MachineIndex> _ProbingSequence = new ObservableCollection<MachineIndex>();
        public ObservableCollection<MachineIndex> ProbingSequence
        {
            get { return _ProbingSequence; }
            set
            {
                if (value != _ProbingSequence)
                {
                    _ProbingSequence = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private MachineIndex _SelectedCoordiante;
        //[JsonIgnore]
        //public MachineIndex SelectedCoordiante
        //{
        //    get { return _SelectedCoordiante; }
        //    set
        //    {
        //        if (value != _SelectedCoordiante)
        //        {
        //            _SelectedCoordiante = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private FoupObject _SelectedFoup;
        public FoupObject SelectedFoup
        {
            get { return _SelectedFoup; }
            set
            {
                if (value != _SelectedFoup)
                {
                    _SelectedFoup = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SlotObject _SelectedSlot;
        public SlotObject SelectedSlot
        {
            get { return _SelectedSlot; }
            set
            {
                if (value != _SelectedSlot)
                {
                    _SelectedSlot = value;

                    RaisePropertyChanged();
                }
            }
        }


        //private string _ChuckZState;
        //public string ChuckZState
        //{
        //    get { return _ChuckZState; }
        //    set
        //    {
        //        if (value != _ChuckZState)
        //        {
        //            _ChuckZState = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private LotOPStateEnum _LotState;
        //public LotOPStateEnum LotState
        //{
        //    get { return _LotState; }
        //    set
        //    {
        //        if (value != _LotState)
        //        {
        //            _LotState = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private long _XIndexForAddseq = 0;
        [JsonIgnore]
        public long XIndexForAddseq
        {
            get { return _XIndexForAddseq; }
            set
            {
                if (value != _XIndexForAddseq)
                {
                    _XIndexForAddseq = value;
                    RaisePropertyChanged();
                }
            }
        }


        private long _YIndexForAddseq = 0;
        [JsonIgnore]
        public long YIndexForAddseq
        {
            get { return _YIndexForAddseq; }
            set
            {
                if (value != _YIndexForAddseq)
                {
                    _YIndexForAddseq = value;
                    RaisePropertyChanged();
                }
            }
        }

        public DummyTCPIPQueries()
        {

        }

        private void DefaultDataSet()
        {
            try
            {
                CassetteSlotCount = 25;

                //this.CurTemp.Value = 30;

                //this.ProberID.Value = "ProberID";
                //this.LotName.Value = "LotName";
                //this.DeviceName.Value = "RecipeName";
                this.WaferSizeInch.Value = 12;
                this.NotchAngle.Value = 0;
                this.NotchAngleOffset.Value = 0;
                //this.WaferID.Value = "WaferID";
                
                //this.OverDrive.Value = 100;

                //this.BinType.Value = BinAnalyzer.Data.BinType.BIN_PASSFAIL;

                if (ProbingSequence == null)
                {
                    ProbingSequence = new ObservableCollection<MachineIndex>();
                }

                if (ProbingSequence.Count == 0)
                {
                    MachineIndex tmp = new MachineIndex();
                    tmp.XIndex = 10;
                    tmp.YIndex = -10;

                    ProbingSequence.Add(tmp);
                }

                //this.ChuckZState = "Zup";

                for (int i = 0; i < FoupCount; i++)
                {
                    if (Foups == null)
                    {
                        Foups = new ObservableCollection<FoupObject>();
                    }

                    FoupObject tmpfoup = new FoupObject();

                    tmpfoup.Name = $"Foup #{i + 1}";
                    tmpfoup.State = ProberInterfaces.Foup.FoupStateEnum.EMPTY_CASSETTE;
                    tmpfoup.LotState = LotStateEnum.Idle;

                    for (int s = 0; s < CassetteSlotCount; s++)
                    {
                        SlotObject tmpslot = new SlotObject();

                        tmpslot.Index = s;
                        tmpslot.Name = $"Slot #{s + 1}";

                        tmpfoup.Slots.Add(tmpslot);
                    }

                    Foups.Add(tmpfoup);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                DefaultDataSet();

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                DefaultDataSet();

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
    }
}
