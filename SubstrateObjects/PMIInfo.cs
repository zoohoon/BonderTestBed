using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.PMI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SubstrateObjects
{
    [Serializable]
    public class PMIInfo : IPMIInfo, IFactoryModule, INotifyPropertyChanged
    {
        private readonly int WaferTemplateCount = 25;

        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [JsonIgnore, ParamIgnore]
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
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        public PMIInfo()
        {
        }

        private int _MapWidth;
        [JsonIgnore]
        public int MapWidth
        {
            get { return _MapWidth; }
            set
            {
                if (value != _MapWidth)
                {
                    _MapWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _MapHeight;
        [JsonIgnore]
        public int MapHeight
        {
            get { return _MapHeight; }
            set
            {
                if (value != _MapHeight)
                {
                    _MapHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private int _SelectedPadTemplateIndex;
        //[JsonIgnore]
        //public int SelectedPadTemplateIndex
        //{
        //    get { return _SelectedPadTemplateIndex; }
        //    set
        //    {
        //        if (value != _SelectedPadTemplateIndex)
        //        {
        //            _SelectedPadTemplateIndex = value;
        //            RaisePropertyChanged();
        //        }

        //        SelectedPadTemplate = (PadTemplateInfo.Count > 0) ? PadTemplateInfo[_SelectedPadTemplateIndex] : null;

        //        if (SelectedPadTemplate != null)
        //        {
        //            Geometry g = Geometry.Parse(SelectedPadTemplate.PathData.Value);

        //            System.Windows.Application.Current.Dispatcher.Invoke(() =>
        //            {
        //                if (SelectedPadTemplatePath != null)
        //                    SelectedPadTemplatePath.Data = g;
        //            });
        //        }
        //    }
        //}

        [NonSerialized]
        private int _SelectedNormalPMIMapTemplateIndex;
        [JsonIgnore]
        public int SelectedNormalPMIMapTemplateIndex
        {
            get { return _SelectedNormalPMIMapTemplateIndex; }
            set
            {
                if (value != _SelectedNormalPMIMapTemplateIndex)
                {
                    _SelectedNormalPMIMapTemplateIndex = value;
                    RaisePropertyChanged();
                }

                //SelectedNormalPMIMapTemplate = (NormalPMIMapTemplateInfo.Count > 0) ? NormalPMIMapTemplateInfo[_SelectedNormalPMIMapTemplateIndex] : null;
                //PMIInfoUpdatedToLoader();
            }
        }

        //[NonSerialized]
        //private int _SelectedSamplePMIMapIndex;
        //[  JsonIgnore]
        //public int SelectedSamplePMIMapTemplateIndex
        //{
        //    get { return _SelectedSamplePMIMapIndex; }
        //    set
        //    {
        //        if (value != _SelectedSamplePMIMapIndex)
        //        {
        //            _SelectedSamplePMIMapIndex = value;
        //            RaisePropertyChanged();
        //        }

        //        SelectedSamplePMIMapTemplate = (SamplePMIMapTemplateInfo.Count > 0) ? SamplePMIMapTemplateInfo[_SelectedSamplePMIMapIndex] : null;
        //    }
        //}

        //private int _SelectedPadTableTemplateIndex;
        //[JsonIgnore]
        //public int SelectedPadTableTemplateIndex
        //{
        //    get { return _SelectedPadTableTemplateIndex; }
        //    set
        //    {
        //        if (value != _SelectedPadTableTemplateIndex)
        //        {
        //            _SelectedPadTableTemplateIndex = value;
        //            RaisePropertyChanged();
        //        }

        //        SelectedPadTableTemplate = (PadTableTemplateInfo.Count > 0) ? PadTableTemplateInfo[_SelectedPadTableTemplateIndex] : null;
        //        //PMIInfoUpdatedToLoader();
        //    }
        //}

        //private int _SelectedWaferTemplateIndex;
        //[JsonIgnore]
        //public int SelectedWaferTemplateIndex
        //{
        //    get { return _SelectedWaferTemplateIndex; }
        //    set
        //    {
        //        if (value != _SelectedWaferTemplateIndex)
        //        {
        //            _SelectedWaferTemplateIndex = value;
        //            RaisePropertyChanged();
        //        }

        //        SelectedWaferTemplate = (WaferTemplateInfo.Count > 0) ? WaferTemplateInfo[_SelectedWaferTemplateIndex] : null;
        //        //PMIInfoUpdatedToLoader();
        //    }
        //}

        //private PadTemplate _SelectedPadTemplate;
        //[JsonIgnore]
        //public PadTemplate SelectedPadTemplate
        //{
        //    get { return _SelectedPadTemplate; }
        //    set
        //    {
        //        if (value != _SelectedPadTemplate)
        //        {
        //            _SelectedPadTemplate = value;
        //            RaisePropertyChanged();
        //            //PMIInfoUpdatedToLoader();
        //        }
        //    }
        //}

        //private DieMapTemplate _SelectedNormalPMIMapTemplate;
        //[JsonIgnore]
        //public DieMapTemplate SelectedNormalPMIMapTemplate
        //{
        //    get { return _SelectedNormalPMIMapTemplate; }
        //    set
        //    {
        //        if (value != _SelectedNormalPMIMapTemplate)
        //        {
        //            _SelectedNormalPMIMapTemplate = value;
        //            RaisePropertyChanged();
        //            //PMIInfoUpdatedToLoader();
        //        }
        //    }
        //}

        //[NonSerialized]
        //private DieMapTemplate _SelectedSamplePMIMapTemplate;
        //[  JsonIgnore]
        //public DieMapTemplate SelectedSamplePMIMapTemplate
        //{
        //    get { return _SelectedSamplePMIMapTemplate; }
        //    set
        //    {
        //        if (value != _SelectedSamplePMIMapTemplate)
        //        {
        //            _SelectedSamplePMIMapTemplate = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private PadTableTemplate _SelectedPadTableTemplate;
        //[JsonIgnore]
        //public PadTableTemplate SelectedPadTableTemplate
        //{
        //    get { return _SelectedPadTableTemplate; }
        //    set
        //    {
        //        if (value != _SelectedPadTableTemplate)
        //        {
        //            _SelectedPadTableTemplate = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private WaferTemplate _SelectedWaferTemplate;
        //[JsonIgnore]
        //public WaferTemplate SelectedWaferTemplate
        //{
        //    get { return _SelectedWaferTemplate; }
        //    set
        //    {
        //        if (value != _SelectedWaferTemplate)
        //        {
        //            _SelectedWaferTemplate = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //[NonSerialized]
        //private Path _SelectedPadTemplatePath;
        //[JsonIgnore]
        //public Path SelectedPadTemplatePath
        //{
        //    get { return _SelectedPadTemplatePath; }
        //    set
        //    {
        //        if (value != _SelectedPadTemplatePath)
        //        {
        //            _SelectedPadTemplatePath = value;
        //            RaisePropertyChanged();

        //        }
        //    }
        //}

        private bool _IsFocusingTurn;
        [JsonIgnore]
        public bool IsFocusingTurn
        {
            get { return _IsFocusingTurn; }
            set
            {
                if (value != _IsFocusingTurn)
                {
                    _IsFocusingTurn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<PadTemplate> _PadTemplateInfo = new ObservableCollection<PadTemplate>();
        public ObservableCollection<PadTemplate> PadTemplateInfo
        {
            get { return _PadTemplateInfo; }
            set
            {
                if (value != _PadTemplateInfo)
                {
                    _PadTemplateInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<DieMapTemplate> _NormalPMIMapTemplateInfo = new ObservableCollection<DieMapTemplate>();
        public ObservableCollection<DieMapTemplate> NormalPMIMapTemplateInfo
        {
            get { return _NormalPMIMapTemplateInfo; }
            set
            {
                if (value != _NormalPMIMapTemplateInfo)
                {
                    _NormalPMIMapTemplateInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private ObservableCollection<DieMapTemplate> _SamplePMIMapTemplateInfo = new ObservableCollection<DieMapTemplate>();
        //public ObservableCollection<DieMapTemplate> SamplePMIMapTemplateInfo
        //{
        //    get { return _SamplePMIMapTemplateInfo; }
        //    set
        //    {
        //        if (value != _SamplePMIMapTemplateInfo)
        //        {
        //            _SamplePMIMapTemplateInfo = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private ObservableCollection<PadTableTemplate> _PadTableTemplateInfo = new ObservableCollection<PadTableTemplate>();
        public ObservableCollection<PadTableTemplate> PadTableTemplateInfo
        {
            get { return _PadTableTemplateInfo; }
            set
            {
                if (value != _PadTableTemplateInfo)
                {
                    _PadTableTemplateInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<WaferTemplate> _WaferTemplateInfo = new ObservableCollection<WaferTemplate>();
        public ObservableCollection<WaferTemplate> WaferTemplateInfo
        {
            get { return _WaferTemplateInfo; }
            set
            {
                if (value != _WaferTemplateInfo)
                {
                    _WaferTemplateInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private ObservableCollection<PadGroupTemplate> _PadGroupTemplateInfo = new ObservableCollection<PadGroupTemplate>();
        //public ObservableCollection<PadGroupTemplate> PadGroupTemplateInfo
        //{
        //    get { return _PadGroupTemplateInfo; }
        //    set
        //    {
        //        if (value != _PadGroupTemplateInfo)
        //        {
        //            _PadGroupTemplateInfo = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //[NonSerialized]
        //private bool _PadGroupingDone;
        //[  JsonIgnore]
        //public bool PadGroupingDone
        //{
        //    get { return _PadGroupingDone; }
        //    set
        //    {
        //        if (value != _PadGroupingDone)
        //        {
        //            _PadGroupingDone = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        //private PMI_SETUP_MODE _SetupMode;
        //[JsonIgnore]
        //public PMI_SETUP_MODE SetupMode
        //{
        //    get { return _SetupMode; }
        //    set
        //    {
        //        if (value != _SetupMode)
        //        {
        //            _SetupMode = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private PMI_RENDER_TABLE_MODE _RenderTableMode;
        //[JsonIgnore]
        //public PMI_RENDER_TABLE_MODE RenderTableMode
        //{
        //    get { return _RenderTableMode; }
        //    set
        //    {
        //        if (value != _RenderTableMode)
        //        {
        //            _RenderTableMode = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        #region Init Method

        //public EventCodeEnum Init()
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        // 데이터 유효성 확인

        //        List<int> isValidIndex = new List<int>();

        //        for (int i = 0; i < PadTemplateInfo.Count; i++)
        //        {
        //            if (PadTemplateInfo[i].Area.Value <= 0 ||
        //                (PadTemplateInfo[i].SizeX.Value <= 0) ||
        //                (PadTemplateInfo[i].SizeY.Value <= 0) ||
        //                (PadTemplateInfo[i].PathData.Value == null) ||
        //                (PadTemplateInfo[i].PathData.Value == string.Empty)
        //                )
        //            {
        //                isValidIndex.Add(i);
        //                continue;
        //            }
        //        }

        //        foreach (var index in isValidIndex)
        //        {
        //            PadTemplateInfo.RemoveAt(index);
        //        }

        //        // 기본 데이터 RECTANGLE 생성
        //        if (PadTemplateInfo.Count == 0)
        //        {
        //            var pad = new PadTemplate("M0,0 L0,1 1,1 1,0 z");

        //            PadTemplateInfo.Add(pad);
        //        }

        //        if (WaferTemplateInfo.Count != WaferTemplateCount)
        //        {
        //            WaferTemplateInfo.Clear();

        //            for (int i = 0; i < WaferTemplateCount; i++)
        //            {
        //                WaferTemplateInfo.Add(new WaferTemplate());
        //            }
        //        }

        //        retval = UpdatePadTableTemplateInfo();

        //        SelectedNormalPMIMapTemplateIndex = 0;

        //        //PadGroupingDone = false;

        //        //System.Windows.Application.Current.Dispatcher.Invoke(() =>
        //        //{
        //        //    SelectedPadTemplatePath = new Path();
        //        //    SelectedPadTemplatePath.Fill = Brushes.Transparent;

        //        //    this.SelectedPadTemplateIndex = 0;
        //        //    this.SelectedNormalPMIMapTemplateIndex = 0;
        //        //    this.SelectedPadTableTemplateIndex = 0;
        //        //    this.SelectedWaferTemplateIndex = 0;
        //        //    this.SelectedNormalPMIMapTemplateIndex = 0;
        //        //});

        //        // ValidationWaferTemplate Information

        //        foreach (var wafertemplate in WaferTemplateInfo)
        //        {
        //            // SelectedMapIndex의 값은 NormalPMIMapTemplate의 개수 - 1 이하의 값만 가질 수 있다.
        //            // 조건이 맞지 않는 경우, 값을 조절
        //            if (wafertemplate.SelectedMapIndex.Value < NormalPMIMapTemplateInfo.Count)
        //            {
        //            }
        //            else
        //            {
        //                wafertemplate.SelectedMapIndex.Value = 0;
        //            }
        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // 데이터 유효성 확인

                bool InVaildFlag = false;

                ObservableCollection<PadTemplate> TempPadTemplateInfo = new ObservableCollection<PadTemplate>();

                foreach (var templateinfo in PadTemplateInfo)
                {
                    if (templateinfo.Area.Value <= 0 ||
                        (templateinfo.SizeX.Value <= 0) ||
                        (templateinfo.SizeY.Value <= 0) ||
                        (templateinfo.PathData.Value == null) ||
                        (templateinfo.PathData.Value == string.Empty)
                        )
                    {
                        InVaildFlag = true;
                    }
                    else
                    {
                        InVaildFlag = false;
                    }

                    if (InVaildFlag == false)
                    {
                        TempPadTemplateInfo.Add(templateinfo);
                    }
                }

                PadTemplateInfo = TempPadTemplateInfo;

                //foreach (var index in InValidIndex)
                //{
                //    PadTemplateInfo.RemoveAt(index);
                //}

                // 기본 데이터 RECTANGLE 생성
                if (PadTemplateInfo.Count == 0)
                {
                    var pad = new PadTemplate("M0,0 L0,1 1,1 1,0 z");

                    PadTemplateInfo.Add(pad);
                }

                if (WaferTemplateInfo.Count != WaferTemplateCount)
                {
                    WaferTemplateInfo.Clear();

                    for (int i = 0; i < WaferTemplateCount; i++)
                    {
                        WaferTemplateInfo.Add(new WaferTemplate());
                    }
                }

                retval = UpdatePadTableTemplateInfo();

                SelectedNormalPMIMapTemplateIndex = 0;

                //PadGroupingDone = false;

                //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                //{
                //    SelectedPadTemplatePath = new Path();
                //    SelectedPadTemplatePath.Fill = Brushes.Transparent;

                //    this.SelectedPadTemplateIndex = 0;
                //    this.SelectedNormalPMIMapTemplateIndex = 0;
                //    this.SelectedPadTableTemplateIndex = 0;
                //    this.SelectedWaferTemplateIndex = 0;
                //    this.SelectedNormalPMIMapTemplateIndex = 0;
                //});

                // ValidationWaferTemplate Information

                foreach (var wafertemplate in WaferTemplateInfo)
                {
                    // SelectedMapIndex의 값은 NormalPMIMapTemplate의 개수 - 1 이하의 값만 가질 수 있다.
                    // 조건이 맞지 않는 경우, 값을 조절
                    if (wafertemplate.SelectedMapIndex.Value < NormalPMIMapTemplateInfo.Count)
                    {
                    }
                    else
                    {
                        wafertemplate.SelectedMapIndex.Value = 0;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum UpdatePadTableTemplateInfo()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                int PMIPadCount = 0;

                if ((this.GetParam_Wafer() != null) &&
                    (this.GetParam_Wafer().GetSubsInfo() != null) &&
                    (this.GetParam_Wafer().GetSubsInfo().Pads != null) &&
                    (this.GetParam_Wafer().GetSubsInfo().Pads.PMIPadInfos != null)
                    )
                {
                    PMIPadCount = this.GetParam_Wafer().GetSubsInfo().Pads.PMIPadInfos.Count;
                }

                // 등록된 패드가 존재하는 경우
                if (PMIPadCount > 0)
                {
                    // 테이블 정보가 하나도 존재하지 않는 경우
                    if (PadTableTemplateInfo.Count <= 0)
                    {
                        PadTableTemplateInfo.Add(new PadTableTemplate());

                        // 디폴트 값을 True로 => Inspection을 진행하겠다는 뜻
                        for (int i = 0; i < PMIPadCount; i++)
                        {
                            PadTableTemplateInfo[PadTableTemplateInfo.Count - 1].PadEnable.Add(new Element<bool> { Value = true });
                        }
                    }
                    else
                    {
                        // 테이블 정보가 이미 존재하는 경우 
                        foreach (var tabletemplate in PadTableTemplateInfo)
                        {
                            // TODO: 실제로 개수가 같은 것 뿐만이 아니라, 모든 패드 정보가 동일한 데이터인지 체크하는 로직이 추가 되어야 함.
                            // 개수는 같을 수 있지만, 데이터가 변경되었을 수 있기 때문!
                            if (tabletemplate.PadEnable.Count != PMIPadCount)
                            {
                                tabletemplate.Clear();

                                // 디폴트 값을 True로 => Inspection을 진행하겠다는 뜻
                                for (int i = 0; i < PMIPadCount; i++)
                                {
                                    tabletemplate.PadEnable.Add(new Element<bool> { Value = true });
                                }
                            }
                        }
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        #endregion

        //#region ==> Set Functions
        //public EventCodeEnum SetSelectedPadTableTemplateIndex(int index)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        SeletedPadTableIndex = index;

        //        retval = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //public EventCodeEnum SetSelectedNormalMapTemplateIndex(int index)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        SelectedNormalPMIMapIndex = index;

        //        retval = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //public EventCodeEnum SetSelectedSampleMapTemplateIndex(int index)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        SelectedSamplePMIMapIndex = index;

        //        retval = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //public EventCodeEnum SetSelectedPadTemplateIndex(int index)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        SeletedPadTemplateIndex = index;

        //        retval = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //#endregion

        #region ==> Get Functions
        //public int GetSelectedPadTableIndex()
        //{
        //    return SeletedPadTableIndex;
        //}

        //public int GetSelectedPadTemplateIndex()
        //{
        //    return SeletedPadTemplateIndex;
        //}

        //public int GetSelectedNormalPMIMapIndex()
        //{
        //    return SelectedNormalPMIMapIndex;
        //}

        //public int GetSelectedSamplePMIMapIndex()
        //{
        //    return SelectedSamplePMIMapIndex;
        //}


        public bool CheckUsingPadExistInTable(int TableIndex)
        {
            bool retVal = false;

            try
            {
                var CurPadTable = PadTableTemplateInfo[TableIndex];

                for (int i = 0; i < CurPadTable.PadEnable.Count; i++)
                {
                    if (CurPadTable.PadEnable[i].Value == true)
                    {
                        retVal = true;
                        break;
                    }
                }

                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIInfo] [CheckUsingPadExistInTable()] : {err}");
                return retVal;
            }
        }

        public PadTemplate GetPadTemplate(int index)
        {
            PadTemplate retval = null;

            try
            {
                if (PadTemplateInfo.ElementAtOrDefault(index) != null)
                {
                    retval = PadTemplateInfo[index];
                    //SetSeletedPadTemplateIndex(index);
                }
                else
                {
                    retval = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIInfo] [GetPadTemplate()]" + $"{index}" + $")] : {err}");
            }

            return retval;
        }

        public PadTableTemplate GetPadTableTemplate(int index)
        {
            PadTableTemplate retval = null;

            try
            {
                if (PadTableTemplateInfo.ElementAtOrDefault(index) != null)
                {
                    retval = PadTableTemplateInfo[index];
                }
                else
                {
                    retval = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIInfo] [GetPadTableTemplate()]" + $"{index}" + $")] : {err}");
            }

            return retval;
        }

        public DieMapTemplate GetNormalPMIMapTemplate(int index)
        {
            DieMapTemplate retval = null;

            try
            {
                if (NormalPMIMapTemplateInfo.ElementAtOrDefault(index) != null)
                {
                    retval = NormalPMIMapTemplateInfo[index];
                }
                else
                {
                    retval = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIInfo] [GetNormalPMIMapTemplate(" + $"{index}" + $")] : {err}");
            }

            return retval;
        }

        //public DieMapTemplate GetSamplePMIMapTemplate(int index)
        //{
        //    DieMapTemplate retval = null;

        //    try
        //    {
        //        if (SamplePMIMapTemplateInfo.ElementAtOrDefault(index) != null)
        //        {
        //            retval = SamplePMIMapTemplateInfo[index];
        //        }
        //        else
        //        {
        //            retval = null;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PMIInfo] [GetSamplePMIMapTemplate(" + $"{index}" + $")] : {err}");
        //    }

        //    return retval;
        //}

        public WaferTemplate GetWaferTemplate(int index)
        {
            WaferTemplate retval = null;

            try
            {
                if (WaferTemplateInfo.ElementAtOrDefault(index) != null)
                {
                    retval = WaferTemplateInfo[index];
                }
                else
                {
                    retval = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIInfo] [GetWaferTemplate(" + $"{index}" + $")] : {err}");
            }

            return retval;
        }

        //public PadGroupTemplate GetPadGroupTemplate(int index)
        //{
        //    PadGroupTemplate retval = null;

        //    try
        //    {
        //        if (PadGroupTemplateInfo.ElementAtOrDefault(index) != null)
        //        {
        //            retval = PadGroupTemplateInfo[index];
        //        }
        //        else
        //        {
        //            retval = null;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PMIInfo] [GetPadGroupTemplate(" + $"{index}" + $")] : {err}");
        //    }

        //    return retval;
        //}

        #endregion

        public void SetWaferInfo(int width, int height)
        {
            try
            {
                MapWidth = width;
                MapHeight = height;

                if (NormalPMIMapTemplateInfo != null)
                {
                    bool IsDefalutSet = false;

                    if (NormalPMIMapTemplateInfo.Count <= 0)
                    {
                        IsDefalutSet = true;
                    }
                    else
                    {
                        bool IsValid;

                        IsValid = ValidationWaferMapTemplateInfo(MapWidth, MapHeight);

                        if (IsValid == false)
                        {
                            IsDefalutSet = true;
                        }
                        else
                        {
                            foreach (var maptemplate in NormalPMIMapTemplateInfo)
                            {
                                maptemplate.MapWidth = MapWidth;
                                maptemplate.MapHeight = MapHeight;
                            }
                        }
                    }

                    if (IsDefalutSet == true)
                    {
                        var DefalutMap = new DieMapTemplate(MapWidth, MapHeight);

                        if (NormalPMIMapTemplateInfo.Count > 0)
                        {
                            NormalPMIMapTemplateInfo.Clear();
                        }

                        NormalPMIMapTemplateInfo.Add(DefalutMap);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool ValidationWaferMapTemplateInfo(int width, int height)
        {
            bool retVal = true;

            var DefalutMap = new DieMapTemplate(MapWidth, MapHeight);

            var NormalPMIMapCount = DefalutMap.PMIMap.Value.Count();
            var NormalPMITableMapCount = DefalutMap.PMITableMap.Value.Count();

            foreach (var map in NormalPMIMapTemplateInfo)
            {
                if ((map.PMIMap.Value.Count() != NormalPMIMapCount) ||
                    (map.PMITableMap.Value.Count() != NormalPMITableMapCount)
                    )
                {
                    retVal = false;
                    break;
                }

            }

            return retVal;
        }

        //public void PMIInfoUpdatedToLoader()
        //{
        //    try
        //    {
        //        if ((this.StageSupervisor().IsAvailableLoaderRemoteMediator()) && (this.LoaderController().GetconnectFlag() == true))
        //        {
        //            var bytes = SerializeManager.ObjectToByte(this);
        //            this.LoaderRemoteMediator().ServiceCallBack.PMIInfoUpdated(bytes);
        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        //public ObservableCollection<PadTemplate> GetPadTemplateInfo()
        //{
        //    ObservableCollection<PadTemplate> retval = null;

        //    try
        //    {
        //        retval = PadTemplateInfo;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}
        //public ObservableCollection<PadTableTemplate> GetPadTableTemplateInfo()
        //{
        //    ObservableCollection<PadTableTemplate> retval = null;

        //    try
        //    {
        //        retval = PadTableTemplateInfo;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //public ObservableCollection<DieMapTemplate> GetNormalPMIMapTemplateInfo()
        //{
        //    ObservableCollection<DieMapTemplate> retval = null;

        //    try
        //    {
        //        retval = NormalPMIMapTemplateInfo;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //public ObservableCollection<DieMapTemplate> GetSamplePMIMapTemplateInfo()
        //{
        //    ObservableCollection<DieMapTemplate> retval = null;

        //    try
        //    {
        //        retval = SamplePMIMapTemplateInfo;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //public ObservableCollection<WaferTemplate> GetWaferTemplateInfo()
        //{
        //    ObservableCollection<WaferTemplate> retval = null;

        //    try
        //    {
        //        retval = WaferTemplateInfo;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PMIInfo] [GetWaferTemplateInfo()] : {err}");
        //    }

        //    return retval;
        //}

        //public ObservableCollection<PadGroupTemplate> GetPadGroupTemplateInfo()
        //{
        //    ObservableCollection<PadGroupTemplate> retval = null;

        //    try
        //    {
        //        retval = PadGroupTemplateInfo;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PMIInfo] [GetPadGroupTemplateInfo()] : {err}");
        //    }

        //    return retval;
        //}
    }
}
