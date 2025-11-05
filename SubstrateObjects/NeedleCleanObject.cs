using ProberInterfaces;
using ProberInterfaces.NeedleClean;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using ProberErrorCode;
using NeedleCleanerModuleParameter;
using ProberInterfaces.Param;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows;
using ProberInterfaces.PinAlign.ProbeCardData;
using LogModule;
using SerializerUtil;

namespace SubstrateObjects
{
    public class NeedleCleanObject : INeedleCleanObject, IParamNode, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion


        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
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

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //EventCodeEnum ret = EventCodeEnum.NONE;
                //IParam param = null;

                //ret = Extensions_IParam.LoadParameter(null, ref param, typeof(NeedleCleanSystemParameter), owner: this);
                //this.NCSysParam = param as NeedleCleanSystemParameter;

                //this.NCSysParam.Genealogy = Extensions_IParam.GetOwner(this, null).GetType().Name + "." + this.NCSysParam.GetType().Name + ".";
                //Extensions_IParam.MakeNodes(this.NCSysParam, ParamType.COMMON, this);

                IParam tmpParam = null;
                tmpParam = new NeedleCleanSystemParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retval = this.LoadParameter(ref tmpParam, typeof(NeedleCleanSystemParameter));

                if (retval == EventCodeEnum.NONE)
                {
                    this.NCSysParam = tmpParam as NeedleCleanSystemParameter;
                }
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }

            return retval;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            try
            {
                ret = Extensions_IParam.SaveParameter(null, NCSysParam);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
            return ret;
        }

        public bool IsInitialized { get; set; } = false;
        public EventCodeEnum Init()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            try
            {

                if (!IsInitialized)
                {
                    IsInitialized = true;
                    NCSheetVMDefs = new ObservableCollection<NCSheetVMDefinition>();

                    for (int i = 0; i < (NCSysParam?.SheetDefs.Count ?? 0); i++)
                    {
                        NCSheetVMDefs.Add(new NCSheetVMDefinition());
                    }
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
            return ret;
        }

        private ISubRoutine _ISubRutine;
        [ParamIgnore]
        public ISubRoutine SubRoutine
        {
            get { return _ISubRutine; }
            set { _ISubRutine = value; }
        }

        //private IParam _NCSysParam_IParam;
        [ParamIgnore]
        public IParam NCSysParam_IParam
        {
            get { return (IParam)NCSysParam; }
            set
            {
                NCSysParam = (NeedleCleanSystemParameter)value;
            }
        }

        private NeedleCleanSystemParameter _NCSysParam;
        [ParamIgnore]
        public NeedleCleanSystemParameter NCSysParam
        {
            get { return _NCSysParam; }
            set
            {
                if (value != _NCSysParam)
                {
                    _NCSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<NCSheetVMDefinition> _NCSheetVMDefs
            = new ObservableCollection<NCSheetVMDefinition>();
        [ParamIgnore]
        public ObservableCollection<NCSheetVMDefinition> NCSheetVMDefs
        {
            get { return _NCSheetVMDefs; }
            set
            {
                if (value != _NCSheetVMDefs)
                {
                    _NCSheetVMDefs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private NCSheetVMDefinition _NCSheetVMDef
            = new NCSheetVMDefinition();
        [ParamIgnore]
        public NCSheetVMDefinition NCSheetVMDef
        {
            get { return _NCSheetVMDef; }
            set
            {
                if (value != _NCSheetVMDef)
                {
                    _NCSheetVMDef = value;
                    RaisePropertyChanged();
                }
            }
        }

        private NCSheetDefinitionMapping _NCSheetDefinitionMapping;
        [ParamIgnore]
        public NCSheetDefinitionMapping NCSheetDefinitionMapping
        {
            get { return _NCSheetDefinitionMapping; }
            set
            {
                if (value != _NCSheetDefinitionMapping)
                {
                    _NCSheetDefinitionMapping = value;
                    RaisePropertyChanged();
                }
            }
        }

        // PinAlign Before Needle Cleaning 명령이 발동되었다는 내부 플래그
        // 니들 클리닝 모듈이 IDLE에서 RUN으로 처음 넘어갈 때 초기화 한다.
        private bool _PinAlignBeforeCleaningProcessed = new bool();
        public bool PinAlignBeforeCleaningProcessed
        {
            get { return _PinAlignBeforeCleaningProcessed; }
            set
            {
                if (value != _PinAlignBeforeCleaningProcessed)
                {
                    _PinAlignBeforeCleaningProcessed = value;
                    RaisePropertyChanged();
                }
            }
        }

        // PinAlign After Needle Cleaning 명령이 발동되었다는 내부 플래그
        // 니들 클리닝 모듈이 IDLE에서 RUN으로 처음 넘어갈 때 초기화 한다.
        private bool _PinAlignAfterCleaningProcessed = new bool();
        public bool PinAlignAfterCleaningProcessed
        {
            get { return _PinAlignAfterCleaningProcessed; }
            set
            {
                if (value != _PinAlignAfterCleaningProcessed)
                {
                    _PinAlignAfterCleaningProcessed = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 니들 클리닝이 수행되었다는 내부 플래그
        // Pin Align After Cleaning의 경우 앞에서 클리닝이 진행되고 자기 턴이 온 것인지, SKIP 되고 온 것인지 구분이 불가능하다.
        // 그렇기에 실제로 클리닝이 수행되었는지 아닌지를 앞에서 알려 주어야 한다.
        // 이 플래그는 클리닝을 담당하는 프로세싱 모듈에서 켜주고, Pin Align After Cleaning 모듈에서 꺼준다.
        private bool _NeedleCleaningProcessed = new bool();
        public bool NeedleCleaningProcessed
        {
            get { return _NeedleCleaningProcessed; }
            set
            {
                if (value != _NeedleCleaningProcessed)
                {
                    _NeedleCleaningProcessed = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Element<PinCoordinate> SensorPos
        {
            get { return NCSysParam.SensorPos; }
        }

        public Element<PinCoordinate> SensorFocusedPos
        {
            get { return NCSysParam.SensorFocusedPos; }
        }

        public Element<PinCoordinate> SensorBasePos
        {
            get { return NCSysParam.SensorBasePos; }
        }

        public Element<NCCoordinate> SensingPadBasePos
        {
            get { return NCSysParam.SensingPadBasePos; }
        }

        #region //.. NC Render

        #region CleanPad
        //private float _PadSizeRatio;

        public float PadSizeRatio
        {
            get { return (NCSysParam.NeedleCleanPadWidth.Value / NCSysParam.NeedleCleanPadHeight.Value); }
        }


        private double _RatioX;

        public double RatioX
        {
            get { return _RatioX; }
            set { _RatioX = value; }
        }

        private double _RatioY;

        public double RatioY
        {
            get { return _RatioY; }
            set { _RatioY = value; }
        }

        private float _WinSizeX;

        public float WinSizeX
        {
            get { return _WinSizeX; }
            set { _WinSizeX = value; }
        }

        private float _WinSizeY;

        public float WinSizeY
        {
            get { return _WinSizeY; }
            set { _WinSizeY = value; }
        }



        private double _NeedleCleanPadWidth;
        public double NeedleCleanPadWidth
        {
            get { return _NeedleCleanPadWidth; }
            set
            {
                if (value != _NeedleCleanPadWidth)
                {
                    _NeedleCleanPadWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _NeedleCleanPadHeight;
        public double NeedleCleanPadHeight
        {
            get { return _NeedleCleanPadHeight; }
            set
            {
                if (value != _NeedleCleanPadHeight)
                {
                    _NeedleCleanPadHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Sequence 

        private float _DutWidth;
        public float DutWidth
        {
            get { return _DutWidth; }
            set
            {
                if (value != _DutWidth)
                {
                    _DutWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private float _DutHeight;
        public float DutHeight
        {
            get { return _DutHeight; }
            set
            {
                if (value != _DutHeight)
                {
                    _DutHeight = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _SheetWidth;
        public double SheetWidth
        {
            get { return _SheetWidth; }
            set
            {
                if (value != _SheetWidth)
                {
                    _SheetWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SheetHeight;
        public double SheetHeight
        {
            get { return _SheetHeight; }
            set
            {
                if (value != _SheetHeight)
                {
                    _SheetHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _TotalCount;
        public int TotalCount
        {
            get { return _TotalCount; }
            set
            {
                if (value != _TotalCount)
                {
                    _TotalCount = value;
                    RaisePropertyChanged();
                    this.NCSequencesUpdated();
                }
            }
        }

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                    this.NCSequencesUpdated();
                }
            }
        }

        private long _Distance;
        public long Distance
        {
            get { return _Distance; }
            set
            {
                if (value != _Distance)
                {
                    _Distance = value;
                    RaisePropertyChanged();
                    this.NCSequencesUpdated();
                }
            }
        }


        private Visibility _DistanceVisible;
        public Visibility DistanceVisible
        {
            get { return _DistanceVisible; }
            set
            {
                if (value != _DistanceVisible)
                {
                    _DistanceVisible = value;
                    RaisePropertyChanged();
                    this.NCSequencesUpdated();
                }
            }
        }

        private ObservableCollection<IDut> _TempDutList = new ObservableCollection<IDut>();
        public ObservableCollection<IDut> TempDutList
        {
            get { return _TempDutList; }
            set
            {
                if (value != _TempDutList)
                {
                    _TempDutList = value;
                    RaisePropertyChanged();
                    this.NCSequencesUpdated();
                }
            }
        }

        private string _ImageSource;
        public string ImageSource
        {
            get { return _ImageSource; }
            set
            {
                if (value != _ImageSource)
                {
                    _ImageSource = value;
                    RaisePropertyChanged();
                    this.NCSequencesUpdated();
                }
            }
        }

        private NCSheetDefsInfo _NcSheets
             = new NCSheetDefsInfo();

        public NCSheetDefsInfo NcSheets
        {
            get { return _NcSheets; }
            set { _NcSheets = value; }
        }

        private NCSequencesInfo _NCSequences
             = new NCSequencesInfo();

        public NCSequencesInfo NCSequences
        {
            get { return _NCSequences; }
            set { _NCSequences = value; }
        }


        #endregion



        public void InitCleanPadRender()
        {
            // 화면에 표시되는 영역은 가로 800, 세로 400이다. 따라서 가로 세로의 비율은 2:1며 표시하고 싶은 패드 사이즈의 비율에 따라 더 
            // 긴쪽을 기준으로 디스플레이 될 기준을 잡는다.
            NCSysParam = (NeedleCleanSystemParameter)this.NeedleCleaner().GetParam_NcObject().NCSysParam_IParam;
            if (PadSizeRatio > 2)
            {   // 가로의 길이가 세로의 길이보다 2배 이상 긴 경우. 가로를 먼저 맞춘다
                _WinSizeX = 800;
                _WinSizeY = 800 / PadSizeRatio;
            }
            else
            {
                // 가로의 길이가 세로의 길이보다 2배 미만인 경우 혹은 같거나 세로가 더 긴 경우. 세로를 먼저 맞춘다.
                _WinSizeY = 400;
                _WinSizeX = 400 * PadSizeRatio;
            }

            _RatioX = _WinSizeX / NCSysParam.NeedleCleanPadWidth.Value;
            _RatioY = _WinSizeY / NCSysParam.NeedleCleanPadHeight.Value;
            //this.NCSheetVMDef.SheetWidth = this.NCSheetVMDefs[NCSheetVMDef.Index].SheetWidth;
            //this.NCSheetVMDef.SheetHeight = this.NCSheetVMDefs[NCSheetVMDef.Index].SheetHeight;
            this.NCSheetVMDef.SheetWidth = NCSysParam.SheetDefs[NCSheetVMDef.Index].Range.Value.X.Value * 2 * _RatioX;
            this.NCSheetVMDef.SheetHeight = NCSysParam.SheetDefs[NCSheetVMDef.Index].Range.Value.Y.Value * 2 * _RatioY;
            NeedleCleanPadWidth = NCSysParam.NeedleCleanPadWidth.Value * _RatioX;
            NeedleCleanPadHeight = NCSysParam.NeedleCleanPadHeight.Value * _RatioY;
        }

        public void InitNCSequenceRender()
        {
            if (PadSizeRatio > 2)
            {   // 가로의 길이가 세로의 길이보다 2배 이상 긴 경우. 가로를 먼저 맞춘다
                _WinSizeX = 800;
                _WinSizeY = 800 / PadSizeRatio;
            }
            else
            {
                // 가로의 길이가 세로의 길이보다 2배 미만인 경우 혹은 같거나 세로가 더 긴 경우. 세로를 먼저 맞춘다.
                _WinSizeY = 400;
                _WinSizeX = 400 * PadSizeRatio;
            }

            _RatioX = _WinSizeX / (NCSysParam.SheetDefs[NCSheetVMDef.Index].Range.Value.X.Value * 2);
            _RatioY = _WinSizeY / (NCSysParam.SheetDefs[NCSheetVMDef.Index].Range.Value.Y.Value * 2);
            SheetWidth = NCSysParam.SheetDefs[NCSheetVMDef.Index].Range.Value.X.Value * 2 * _RatioX;
            SheetHeight = NCSysParam.SheetDefs[NCSheetVMDef.Index].Range.Value.Y.Value * 2 * _RatioY;

            _DutWidth = (float)(this.GetParam_Wafer().GetPhysInfo().DieSizeX.Value * _RatioX);
            _DutHeight = (float)(this.GetParam_Wafer().GetPhysInfo().DieSizeY.Value * _RatioY);

            Index = NCSheetVMDef.Index;
        }

        public byte[] GetNCObjectByteArray()
        {
            byte[] compressedData = null;
            try
            {
                var bytes = SerializeManager.SerializeToByte(this, typeof(NeedleCleanObject));
                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return compressedData;
        }

        public void NCSheetVMDefsUpdated()
        {
            try
            {
                if (this.LoaderRemoteMediator().GetServiceCallBack() != null)
                {
                    NcSheets.NCSheetVMDefs = this.NCSheetVMDefs;
                    NcSheets.NCSheetVMDef = this.NCSheetVMDef;
                    this.LoaderRemoteMediator().GetServiceCallBack()?.NCSheetVMDefsUpdated(SerializeManager.SerializeToByte(NcSheets));
                    //this.StageDataManager().ServiceCallBack.NCSheetVMDefsUpdated(SerializeManager.SerializeToByte(NCSheetVMDefs,typeof(ObservableCollection<NCSheetVMDefinition>)));
                    //this.StageDataManager().ServiceCallBack.NCSheetVMDefUpdated(SerializeManager.SerializeToByte(NCSheetVMDef));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void NCSequencesUpdated()
        {
            try
            {
                if (this.LoaderRemoteMediator().GetServiceCallBack() != null)
                {
                    NCSequences.Index = this.Index;
                    NCSequences.TotalCount = this.TotalCount;
                    NCSequences.DistanceVisible = this.DistanceVisible;
                    NCSequences.ImageSource = this.ImageSource;
                    NCSequences.TempDutList = this.TempDutList.ToList();
                    // NCSequences.TempDutList.Clear();
                    //foreach (var dut in TempDutList)
                    //{
                    //    NCSequences.TempDutList.Add((Dut)dut);
                    //}
                    this.LoaderRemoteMediator().GetServiceCallBack()?.NCSequencesInfoUpdated(SerializeManager.SerializeToByte(NCSequences));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        #endregion
    }

    public class NCSheetDefinitionMapping
    {
        private NeedleCleanObject NCObj = null;
        private List<NCSheetDefinition> SheetDefs { get { return NCObj.NCSysParam.SheetDefs; } }
        private ObservableCollection<NCSheetVMDefinition> NCSheetVMDefs { get { return NCObj.NCSheetVMDefs; } }

        public int Count => SheetDefs?.Count ?? 0;

        public NCSheetDefinitionMapping(NeedleCleanObject NCObj)
        {
            this.NCObj = NCObj;
        }

        public void Add()
        {
            try
            {
                SheetDefs.Add(new NCSheetDefinition());
                NCSheetVMDefs.Add(new NCSheetVMDefinition());
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public void Add(NCSheetDefinition ncSheetDefinition)
        {
            try
            {
                SheetDefs.Add(ncSheetDefinition);
                NCSheetVMDefs.Add(new NCSheetVMDefinition());
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public void Add(NCSheetVMDefinition ncSheetVMDefinition)
        {
            try
            {
                SheetDefs.Add(new NCSheetDefinition());
                NCSheetVMDefs.Add(ncSheetVMDefinition);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public void Add(NCSheetDefinition ncSheetDefinition,
                        NCSheetVMDefinition ncSheetVMDefinition)
        {
            try
            {
                SheetDefs.Add(ncSheetDefinition);
                NCSheetVMDefs.Add(ncSheetVMDefinition);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public void RemoveAt(int idx)
        {
            try
            {
                SheetDefs.RemoveAt(idx);
                NCSheetVMDefs.RemoveAt(idx);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public void Clear()
        {
            try
            {
                SheetDefs.Clear();
                NCSheetVMDefs.Clear();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

    }

    public class NCSheetDefsInfo
    {
        public NCSheetDefsInfo()
        {

        }
        public NCSheetDefsInfo(ObservableCollection<NCSheetVMDefinition> nCSheetVMDefs, NCSheetVMDefinition nCSheetVMDef)
        {
            NCSheetVMDefs = nCSheetVMDefs;
            NCSheetVMDef = nCSheetVMDef;
        }

        private ObservableCollection<NCSheetVMDefinition> _NCSheetVMDefs
           = new ObservableCollection<NCSheetVMDefinition>();
        [ParamIgnore]
        public ObservableCollection<NCSheetVMDefinition> NCSheetVMDefs
        {
            get { return _NCSheetVMDefs; }
            set
            {
                if (value != _NCSheetVMDefs)
                {
                    _NCSheetVMDefs = value;
                }
            }
        }

        private NCSheetVMDefinition _NCSheetVMDef
            = new NCSheetVMDefinition();
        [ParamIgnore]
        public NCSheetVMDefinition NCSheetVMDef
        {
            get { return _NCSheetVMDef; }
            set
            {
                if (value != _NCSheetVMDef)
                {
                    _NCSheetVMDef = value;
                }
            }
        }
    }

    [Serializable]
    public class NCSequencesInfo
    {
        private int _TotalCount;
        public int TotalCount
        {
            get { return _TotalCount; }
            set
            {
                if (value != _TotalCount)
                {
                    _TotalCount = value;
                }
            }
        }

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                }
            }
        }


        private Visibility _DistanceVisible;
        public Visibility DistanceVisible
        {
            get { return _DistanceVisible; }
            set
            {
                if (value != _DistanceVisible)
                {
                    _DistanceVisible = value;
                }
            }
        }

        private List<IDut> _TempDutList = new List<IDut>();
        public List<IDut> TempDutList
        {
            get { return _TempDutList; }
            set
            {
                if (value != _TempDutList)
                {
                    _TempDutList = value;
                }
            }
        }
        //private List<Dut> _TempDutList = new List<Dut>();
        //public List<Dut> TempDutList
        //{
        //    get { return _TempDutList; }
        //    set
        //    {
        //        if (value != _TempDutList)
        //        {
        //            _TempDutList = value;

        //        }
        //    }
        //}


        private string _ImageSource;
        public string ImageSource
        {
            get { return _ImageSource; }
            set
            {
                if (value != _ImageSource)
                {
                    _ImageSource = value;
                }
            }
        }


    }
}
