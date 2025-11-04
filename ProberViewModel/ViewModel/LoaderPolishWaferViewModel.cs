using System;
using System.Linq;
using System.Threading.Tasks;

namespace LooaderPolishWaferViewModelModule
{
    using Autofac;
    using LoaderBase.Communication;
    using LoaderBase.FactoryModules.ViewModelModule;
    using LoaderMapView;
    using LoaderParameters;
    using LogModule;
    using PolishWaferParameters;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.PolishWafer;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using VirtualKeyboardControl;
    using Focusing;
    using LoaderBase;
    using System.Collections.ObjectModel;
    using MetroDialogInterfaces;
    using System.Windows.Controls;
    using ProberViewModel;
    using LoaderCore;
    using LoaderRecoveryControl;
    using System.Threading;
    using UcDisplayPort;
    using System.Windows;
    using TouchSensorSystemParameter;
    using PolishWaferFocusingModule;
    using PolishWaferFocusingBySensorModule;
    using System.Collections;
    using ProberInterfaces.TouchSensor;
    using ProberInterfaces.Foup;

    public class LoaderPolishWaferViewModel : IMainScreenViewModel, ILoaderMapConvert
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public ILoaderViewModelManager LoaderViewModelManager => (ILoaderViewModelManager)this.ViewModelManager();
        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        public ILoaderModule LoaderModule => this.GetLoaderContainer().Resolve<ILoaderModule>();
        private SubstrateSizeEnum SelectedStageWaferSize = SubstrateSizeEnum.UNDEFINED;

        private bool _SourceEnable = true;
        public bool SourceEnable
        {
            get { return _SourceEnable; }
            set
            {
                if (value != _SourceEnable)
                {
                    _SourceEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<FixedTrayInfoObject> _FTs = new ObservableCollection<FixedTrayInfoObject>();
        public ObservableCollection<FixedTrayInfoObject> FTs
        {
            get { return _FTs; }
            set
            {
                if (value != _FTs)
                {
                    _FTs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<InspectionTrayInfoObject> _ITs = new ObservableCollection<InspectionTrayInfoObject>();
        public ObservableCollection<InspectionTrayInfoObject> ITs
        {
            get { return _ITs; }
            set
            {
                if (value != _ITs)
                {
                    _ITs = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<StageObject> _Cells = new ObservableCollection<StageObject>();
        public ObservableCollection<StageObject> Cells
        {
            get { return _Cells; }
            set
            {
                if (value != _Cells)
                {
                    _Cells = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<PolishWaferIntervalParameter> _ManualCleaningIntervalParameters = new ObservableCollection<PolishWaferIntervalParameter>();
        public ObservableCollection<PolishWaferIntervalParameter> ManualCleaningIntervalParameters
        {
            get { return _ManualCleaningIntervalParameters; }
            set
            {
                if (value != _ManualCleaningIntervalParameters)
                {
                    _ManualCleaningIntervalParameters = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IStageObject _SelectedCell;
        public IStageObject SelectedCell
        {
            get { return _SelectedCell; }
            set
            {
                if (value != _SelectedCell)
                {
                    _SelectedCell = value;
                    RaisePropertyChanged();
                }
            }
        }


        private PolishWaferParameter _PolishWaferParam;
        public PolishWaferParameter PolishWaferParam
        {
            get { return _PolishWaferParam; }
            set
            {
                if (value != _PolishWaferParam)
                {
                    _PolishWaferParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PolishWaferCleaningParameter _ManualCleaningParam;
        public PolishWaferCleaningParameter ManualCleaningParam
        {
            get { return _ManualCleaningParam; }
            set
            {
                if (value != _ManualCleaningParam)
                {
                    _ManualCleaningParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DisplayPort _DisplayPort;
        public DisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ILotOPModule LotOPModule => this.LotOPModule();

        private PolishWaferIntervalParameter _SelectedIntervalParam;
        public PolishWaferIntervalParameter SelectedIntervalParam
        {
            get { return _SelectedIntervalParam; }
            set
            {
                if (value != _SelectedIntervalParam)
                {
                    _SelectedIntervalParam = value;

                    ChangeSelectedCleaningParam(_SelectedIntervalParam);
                    RaisePropertyChanged();
                }
            }
        }

        private IPolishWaferCleaningParameter _SelectedCleaningParam;
        public IPolishWaferCleaningParameter SelectedCleaningParam
        {
            get { return _SelectedCleaningParam; }
            set
            {
                if (value != _SelectedCleaningParam)
                {
                    _SelectedCleaningParam = value;

                    ChangeManualCleaningParam();
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCleaning = true;
        public bool IsCleaning
        {
            get { return _IsCleaning; }
            set
            {
                if (value != _IsCleaning)
                {
                    _IsCleaning = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IPolishWaferFocusing _FocusingModule;

        public IPolishWaferFocusing FocusingModule
        {
            get { return _FocusingModule; }
            set { _FocusingModule = value; }
        }

        private IPolishWaferFocusingBySensor _FocusingBySensorModule;

        public IPolishWaferFocusingBySensor FocusingBySensorModule
        {
            get { return _FocusingBySensorModule; }
            set { _FocusingBySensorModule = value; }
        }

        private ITouchSensorSysParam _TouchSensorParam;
        public ITouchSensorSysParam TouchSensorParam
        {
            get { return _TouchSensorParam; }
            set
            {
                if (value != _TouchSensorParam)
                {
                    _TouchSensorParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        readonly Guid _ViewModelGUID = new Guid("07D0C75C-BC6A-B120-51E6-CBCC9044DDF4");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;

        public LoaderPolishWaferViewModel()
        {

        }
        public void UpdateChanged()
        {
        }
        public void DeInitModule()
        {
            try
            {
                if (Initialized == false)
                {
                    Initialized = true;
                }
            }
            catch (Exception err)
            {

                throw err;
            }
        }

        public EventCodeEnum InitModule()
        {
            try
            { 
                FocusingModule = new PolishWaferFocusing_Standard();
                FocusingBySensorModule = new PolishWaferFocusingBySensor_Standard();

                //InitData();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            try
            {
                DisplayPort = new DisplayPort();
                ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;
                //LoaderViewModelManager.RegisteDisplayPort(DisplayPort);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private bool isClickedTransferSource { get; set; } = false;
        private bool isClickedTransferTarget { get; set; } = false;

        private object _TransferSource;
        public object TransferSource
        {
            get { return _TransferSource; }
            set
            {
                if (value != _TransferSource)
                {
                    if (value is true || value is null)
                    {
                        TransferEnable = false;
                    }
                    else
                    {
                        TransferEnable = true;
                    }
                    _TransferSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private bool _IsCleanningEnable = false;
        //public bool IsCleanningEnable
        //{
        //    get { return _IsCleanningEnable; }
        //    set
        //    {
        //        if (value != _IsCleanningEnable)
        //        {
        //            _IsCleanningEnable = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        private bool _TargetEnable = false;
        public bool TargetEnable
        {
            get { return _TargetEnable; }
            set
            {
                if (value != _TargetEnable)
                {
                    _TargetEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _TransferEnable = false;
        public bool TransferEnable
        {
            get { return _TransferEnable; }
            set
            {
                if (value != _TransferEnable)
                {
                    _TransferEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private object _TransferTarget;
        public object TransferTarget
        {
            get { return _TransferTarget; }
            set
            {
                if (value != _TransferTarget)
                {
                    _TransferTarget = value;
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
                //if (value != _IsSelected)
                //{
                _IsSelected = value;
                if (_IsSelected == false)
                {
                    RaisePropertyChanged();
                }
                //}
            }
        }

        bool targetToggle = true;
        bool sourceToggle = true;

        private bool _ModuleInfoEnable = true;
        public bool ModuleInfoEnable
        {
            get { return _ModuleInfoEnable; }
            set
            {
                if (value != _ModuleInfoEnable)
                {
                    _ModuleInfoEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand<object> _CellClickCommand;
        public ICommand CellClickCommand
        {
            get
            {
                if (null == _CellClickCommand) _CellClickCommand = new RelayCommand<object>(CellClickCommandFunc);
                return _CellClickCommand;
            }
        }

        private void CellClickCommandFunc(object obj)
        {
            try
            {
                if (isClickedTransferSource)
                {
                    TransferSource = SelectedCell;
                    isClickedTransferTarget = false;
                    isClickedTransferSource = false;
                    IsSelected = false;
                    sourceToggle = true;

                    if (TransferSource != null)
                    {
                        LoggerManager.Debug($"[LoaderPolishWaferViewModel], CellClickCommandFunc() : TransferSource = {SelectedCell.Name}.");

                        if(SelectedCell.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            var moduleType = SelectedCell.WaferObj.OriginHolder.ModuleType;
                            int idx = SelectedCell.WaferObj.OriginHolder.Index;
                            if (moduleType == ModuleTypeEnum.FIXEDTRAY ||
                                moduleType == ModuleTypeEnum.INSPECTIONTRAY)
                            {
                                TransferTarget = GetAttachObject(moduleType, idx);
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderPolishWaferViewModel], CellClickCommandFunc() : TransferSource is null.");
                    }
                }
                else if (isClickedTransferTarget)
                {
                    TransferTarget = SelectedCell;
                    isClickedTransferTarget = false;
                    isClickedTransferSource = false;
                    IsSelected = false;
                    targetToggle = true;

                    if (TransferTarget != null)
                    {
                        LoggerManager.Debug($"[LoaderPolishWaferViewModel], CellClickCommandFunc() : TransferTarget = {SelectedCell.Name}."); 
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderPolishWaferViewModel], CellClickCommandFunc() : TransferTarget is null.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private object GetAttachObject(ModuleTypeEnum module, int index)
        {
            object retVal = null;
            try
            {
                switch (module)
                {
                    case ModuleTypeEnum.FIXEDTRAY:
                        retVal = FTs.Where(i => i.Index == index).FirstOrDefault();
                        break;
                    case ModuleTypeEnum.INSPECTIONTRAY:
                        retVal = ITs.Where(i => i.Index == index).FirstOrDefault();
                        break;
                    default:
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private RelayCommand<object> _ObjectClickCommand;
        public ICommand ObjectClickCommand
        {
            get
            {
                if (null == _ObjectClickCommand) _ObjectClickCommand = new RelayCommand<object>(ObjectClickCommandFunc);
                return _ObjectClickCommand;
            }
        }

        private void ObjectClickCommandFunc(object param)
        {
            try
            {
                object[] _param = param as object[];
                // 0 : ListView
                // 1 : SelectedIndex
                // 2 : SelectedItem
                ListView listView = null;
                int index = -1;
                ListViewItem listViewItem = null;
                ListView foupListView = null;


                listView = (ListView)_param[0];
                index = (int)_param[1];

                if (index == -1)
                    return;

                listViewItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(index);

                object item = _param[2];

                listViewItem.IsSelected = false;
                if (ModuleInfoEnable)
                {
                    if (item is FixedTrayInfoObject)
                    {
                        var FixedTrayObj = item as FixedTrayInfoObject;
                        if (FixedTrayObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                        {
                            ModuleInfoVM.Show(ModuleTypeEnum.FIXEDTRAY, item, this.LoaderModule.LoaderMaster);
                        }
                    }
                    else if (item is InspectionTrayInfoObject)
                    {
                        var INSPTrayObj = item as InspectionTrayInfoObject;
                        if (INSPTrayObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST || INSPTrayObj.WaferStatus == EnumSubsStatus.UNKNOWN)
                        {
                            ModuleInfoVM.Show(ModuleTypeEnum.INSPECTIONTRAY, item, this.LoaderModule.LoaderMaster);
                        }
                    }
                }
                else
                {
                    if (isClickedTransferSource)
                    {
                        TransferSource = item;

                        if (TransferSource != null)
                        {
                            if (TransferSource is FixedTrayInfoObject)
                            {
                                var FixedTrayObj = TransferSource as FixedTrayInfoObject;

                                LoggerManager.Debug($"[LoaderPolishWaferViewModel], ObjectClickCommandFunc() : TransferSource = {FixedTrayObj.Name}");
                            }
                            else if (TransferSource is InspectionTrayInfoObject)
                            {
                                var INSPTrayObj = TransferSource as InspectionTrayInfoObject;

                                LoggerManager.Debug($"[LoaderPolishWaferViewModel], ObjectClickCommandFunc() : TransferSource = {INSPTrayObj.Name}");
                            }
                            else
                            {
                                LoggerManager.Debug($"[LoaderPolishWaferViewModel], ObjectClickCommandFunc() : TransferSource type is wrong.");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"[LoaderPolishWaferViewModel], ObjectClickCommandFunc() : TransferSource is null.");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task LoaderMapConvert(LoaderMap map)
        {
            try
            {
                int FixedTrayCnt = map.FixedTrayModules.Count();
                int iNSPCnt = map.InspectionTrayModules.Count();

                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    FTs.Clear();
                    ITs.Clear();

                    //if (SelectedCell == null)
                    //{
                    //    SelectedCell = new StageObject();
                    //}

                    for (int i = 0; i < FixedTrayCnt; i++)
                    {
                        FTs.Add(new FixedTrayInfoObject(i));
                    }

                    for (int i = 0; i < iNSPCnt; i++)
                    {
                        ITs.Add(new InspectionTrayInfoObject(i));
                    }


                    for (int i = 0; i < FixedTrayCnt; i++)
                    {
                        if (map.FixedTrayModules[i].Substrate != null)
                        {
                            FTs[i].WaferObj = map.FixedTrayModules[i].Substrate;
                        }

                        FTs[i].WaferStatus = map.FixedTrayModules[i].WaferStatus;
                    }

                    for (int i = 0; i < iNSPCnt; i++)
                    {
                        if (map.InspectionTrayModules[i].Substrate != null)
                        {
                            ITs[i].WaferObj = map.InspectionTrayModules[i].Substrate;
                        }
                        ITs[i].WaferStatus = map.InspectionTrayModules[i].WaferStatus;
                    }

                    if (_LoaderCommunicationManager != null)
                    {
                        if (_LoaderCommunicationManager.SelectedStage != null)
                        {
                            //if (SelectedCell == null)
                            //{
                            //    SelectedCell = new StageObject(_LoaderCommunicationManager.SelectedStage.Index);
                            //}

                            SelectedCell = _LoaderCommunicationManager.SelectedStage;
                            //SelectedCell.Name = $"Cell #{_LoaderCommunicationManager.SelectedStage.Index}";
                            //SelectedCell.StageInfo.Index = _LoaderCommunicationManager.SelectedStage.Index;
                            //SelectedCell.Index = _LoaderCommunicationManager.SelectedStage.Index;

                            if (map.ChuckModules[_LoaderCommunicationManager.SelectedStage.Index - 1].Substrate != null)
                            {
                                //SelectedCell.State = StageStateEnum.Requested;
                                //SelectedCell.TargetName = map.ChuckModules[idx].Substrate.PrevPos.ToString();
                                SelectedCell.WaferStatus = map.ChuckModules[_LoaderCommunicationManager.SelectedStage.Index - 1].WaferStatus;
                                SelectedCell.Progress = map.ChuckModules[_LoaderCommunicationManager.SelectedStage.Index - 1].Substrate.OriginHolder.Index;
                                SelectedCell.WaferObj = map.ChuckModules[_LoaderCommunicationManager.SelectedStage.Index - 1].Substrate;
                            }
                            else
                            {
                                //SelectedCell.State = StageStateEnum.Not_Request;
                                SelectedCell.WaferStatus = map.ChuckModules[_LoaderCommunicationManager.SelectedStage.Index - 1].WaferStatus;
                            }

                            if (map.CCModules[_LoaderCommunicationManager.SelectedStage.Index - 1].Substrate != null)
                            {
                                SelectedCell.CardStatus = map.CCModules[_LoaderCommunicationManager.SelectedStage.Index - 1].WaferStatus;
                                SelectedCell.Progress = map.CCModules[_LoaderCommunicationManager.SelectedStage.Index - 1].Substrate.OriginHolder.Index;
                                SelectedCell.CardObj = map.CCModules[_LoaderCommunicationManager.SelectedStage.Index - 1].Substrate;
                            }
                            else
                            {
                                SelectedCell.CardStatus = ProberInterfaces.EnumSubsStatus.NOT_EXIST;
                            }
                        }
                    }
                }));
            }
            catch (Exception err)
            {

            }
        }

        private void ChangeManualCleaningParam()
        {
            try
            {
                if (SelectedCleaningParam != null)
                {
                    UpdateTrayUI();// Cleaning List 선택 시 Tray 상태 업데이트
                    if (SelectedCell != null && SelectedCell.WaferStatus == EnumSubsStatus.NOT_EXIST)// Cleaning List 선택 시 Source null로 초기화
                    {
                       TransferSource = null;
                        isClickedTransferSource = true;
                    }

                     PolishWaferCleaningParameter SelectedCleaningParamConcrete = SelectedCleaningParam as PolishWaferCleaningParameter;

                    if (ManualCleaningParam == null)
                    {
                        ManualCleaningParam = new PolishWaferCleaningParameter();
                    }

                    ManualCleaningParam.WaferDefineType.Value = SelectedCleaningParamConcrete.WaferDefineType.Value;
                    ManualCleaningParam.CleaningScrubMode.Value = SelectedCleaningParamConcrete.CleaningScrubMode.Value;
                    ManualCleaningParam.CleaningDirection.Value = SelectedCleaningParamConcrete.CleaningDirection.Value;
                    ManualCleaningParam.ContactLength.Value = SelectedCleaningParamConcrete.ContactLength.Value;
                    ManualCleaningParam.ContactCount.Value = SelectedCleaningParamConcrete.ContactCount.Value;
                    ManualCleaningParam.ScrubingLength.Value = SelectedCleaningParamConcrete.ScrubingLength.Value;
                    ManualCleaningParam.OverdriveValue.Value = SelectedCleaningParamConcrete.OverdriveValue.Value;
                    ManualCleaningParam.Clearance.Value = SelectedCleaningParamConcrete.Clearance.Value;

                    if (ManualCleaningParam.FocusParam == null) 
                    {
                        ManualCleaningParam.FocusParam = new NormalFocusParameter();
                    }
                    SelectedCleaningParamConcrete.FocusParam.CopyTo(ManualCleaningParam.FocusParam);
                    
                    ManualCleaningParam.FocusingModuleDllInfo = SelectedCleaningParamConcrete.FocusingModuleDllInfo;
                    ManualCleaningParam.FocusingPointMode.Value = SelectedCleaningParamConcrete.FocusingPointMode.Value;

                    ManualCleaningParam.FocusingHeightTolerance.Value = SelectedCleaningParamConcrete.FocusingHeightTolerance.Value;

                    ManualCleaningParam.EdgeDetectionBeforeCleaning.Value = SelectedCleaningParamConcrete.EdgeDetectionBeforeCleaning.Value;
                    ManualCleaningParam.PinAlignBeforeCleaning.Value = SelectedCleaningParamConcrete.PinAlignBeforeCleaning.Value;
                    ManualCleaningParam.PinAlignAfterCleaning.Value = SelectedCleaningParamConcrete.PinAlignAfterCleaning.Value;

                    if (ManualCleaningParam.CenteringLightParams == null)
                    {
                        ManualCleaningParam.CenteringLightParams = new ObservableCollection<LightValueParam>();
                    }

                    ManualCleaningParam.CenteringLightParams.Clear();

                    foreach (var light in SelectedCleaningParamConcrete.CenteringLightParams.ToList())
                    {
                        LightValueParam l = new LightValueParam();
                        l.Type.Value = light.Type.Value;
                        l.Value.Value = light.Value.Value;

                        ManualCleaningParam.CenteringLightParams.Add(l);
                    }

                    //this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, ManualCleaningParam.FocusParam, 300);
                    if (ManualCleaningParam.FocusParam.FocusingCam.Value != EnumProberCam.WAFER_HIGH_CAM ||
                        ManualCleaningParam.FocusParam.FocusingAxis.Value != EnumAxisConstants.Z)
                    {
                        LoggerManager.Debug($"ChangeManualCleaningParam() : ManualCleaningParam FocusingCam: {ManualCleaningParam.FocusParam.FocusingCam.Value}," +
                            $"ManualCleaningParam FocusingAxis: {ManualCleaningParam.FocusParam.FocusingAxis.Value}");
                        ManualCleaningParam.FocusParam.FocusingCam.Value = EnumProberCam.WAFER_HIGH_CAM;
                        ManualCleaningParam.FocusParam.FocusingAxis.Value = EnumAxisConstants.Z;
                        LoggerManager.Debug($"ChangeManualCleaningParam() : ManualCleaningParam FocusingCam: {ManualCleaningParam.FocusParam.FocusingCam.Value}," +
                            $"ManualCleaningParam FocusingAxis: {ManualCleaningParam.FocusParam.FocusingAxis.Value}");

                    }

                    if (SelectedCleaningParamConcrete.FocusParam.LightParams.Count == 0)
                    {
                        //ICamera CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                        LightValueParam tmp1 = new LightValueParam();
                        tmp1.Type.Value = EnumLightType.COAXIAL;
                        tmp1.Value.Value = 100;

                        ManualCleaningParam.FocusParam.LightParams.Add(tmp1);

                        LightValueParam tmp2 = new LightValueParam();
                        tmp2.Type.Value = EnumLightType.OBLIQUE;
                        tmp2.Value.Value = 100;

                        ManualCleaningParam.FocusParam.LightParams.Add(tmp2);
                    }
                    else
                    {
                    }
                    
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void ChangeSelectedCleaningParam(PolishWaferIntervalParameter SelectedIntervalParam)
        {
            try 
            {
                if (SelectedIntervalParam != null)
                {
                    if (SelectedIntervalParam.CleaningParameters != null && SelectedIntervalParam.CleaningParameters.Count > 0)
                    {
                        ObservableCollection<IPolishWaferCleaningParameter> PolishWaferCleaningParameters = new ObservableCollection<IPolishWaferCleaningParameter>();
                        foreach (var polishWaferInfos in this.DeviceManager().GetPolishWaferSources())
                        {
                            if (polishWaferInfos.Size.Value == SelectedStageWaferSize)
                            {
                                var filteredParameters = SelectedIntervalParam.CleaningParameters.Where(cleaningParam => cleaningParam.WaferDefineType.Value == polishWaferInfos.DefineName.Value);
                                foreach (var cleaningParam in filteredParameters)
                                {
                                    PolishWaferCleaningParameters.Add(cleaningParam);

                                    SelectedCleaningParam = cleaningParam;
                                }

                            }
                        }
                        SelectedIntervalParam.CleaningParameters = PolishWaferCleaningParameters;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetDefaultCleaningParam()
        {
            try
            {
                // 0 : Start
                // 11 : End

                int stageindex = _LoaderCommunicationManager.SelectedStageIndex;

                HolderModuleInfo chuckinfo = LoaderModule.GetLoaderInfo().StateMap.ChuckModules[stageindex - 1];

                string polishwafername = string.Empty;

                if (this.PolishWaferParam != null)
                {
                    IPolishWaferCleaningParameter FirstCleaningParam = null;

                    if (this.PolishWaferParam.PolishWaferIntervalParameters.Count > 0)
                    {
                        if (this.PolishWaferParam.PolishWaferIntervalParameters[0].CleaningParameters.Count > 0)
                        {
                            FirstCleaningParam = this.PolishWaferParam.PolishWaferIntervalParameters[0].CleaningParameters[0];
                        }
                    }

                    if (FirstCleaningParam != null)
                    {
                        ManualCleaningParam.WaferDefineType.Value = FirstCleaningParam.WaferDefineType.Value;
                        ManualCleaningParam.CleaningScrubMode.Value = FirstCleaningParam.CleaningScrubMode.Value;
                        ManualCleaningParam.CleaningDirection.Value = FirstCleaningParam.CleaningDirection.Value;
                        ManualCleaningParam.ContactLength.Value = FirstCleaningParam.ContactLength.Value;
                        ManualCleaningParam.ContactCount.Value = FirstCleaningParam.ContactCount.Value;
                        ManualCleaningParam.ScrubingLength.Value = FirstCleaningParam.ScrubingLength.Value;
                        ManualCleaningParam.OverdriveValue.Value = FirstCleaningParam.OverdriveValue.Value;
                        ManualCleaningParam.Clearance.Value = FirstCleaningParam.Clearance.Value;
                        ManualCleaningParam.FocusParam = FirstCleaningParam.FocusParam;
                        ManualCleaningParam.FocusingModuleDllInfo = FirstCleaningParam.FocusingModuleDllInfo;
                        ManualCleaningParam.FocusingPointMode.Value = FirstCleaningParam.FocusingPointMode.Value;

                        ManualCleaningParam.FocusingHeightTolerance.Value = FirstCleaningParam.FocusingHeightTolerance.Value;

                        ManualCleaningParam.EdgeDetectionBeforeCleaning.Value = FirstCleaningParam.EdgeDetectionBeforeCleaning.Value;
                        ManualCleaningParam.PinAlignBeforeCleaning.Value = FirstCleaningParam.PinAlignBeforeCleaning.Value;
                        ManualCleaningParam.PinAlignAfterCleaning.Value = FirstCleaningParam.PinAlignAfterCleaning.Value;

                        if (ManualCleaningParam.CenteringLightParams == null)
                        {
                            ManualCleaningParam.CenteringLightParams = new ObservableCollection<LightValueParam>();
                        }

                        ManualCleaningParam.CenteringLightParams.Clear();

                        foreach (var light in FirstCleaningParam.CenteringLightParams)
                        {
                            LightValueParam l = new LightValueParam();
                            l.Type.Value = light.Type.Value;
                            l.Value.Value = light.Value.Value;

                            ManualCleaningParam.CenteringLightParams.Add(l);
                        }

                        if (ManualCleaningParam.FocusParam.LightParams == null)
                        {
                            ManualCleaningParam.FocusParam.LightParams = new ObservableCollection<LightValueParam>();
                        }

                        if (ManualCleaningParam.FocusParam.FocusingCam.Value == EnumProberCam.UNDEFINED)
                        {
                            ManualCleaningParam.FocusParam.FocusingCam.Value = EnumProberCam.WAFER_LOW_CAM;
                        }

                        ManualCleaningParam.FocusParam.LightParams.Clear();

                        foreach (var light in FirstCleaningParam.FocusParam.LightParams)
                        {
                            LightValueParam l = new LightValueParam();
                            l.Type.Value = light.Type.Value;
                            l.Value.Value = light.Value.Value;

                            ManualCleaningParam.FocusParam.LightParams.Add(l);
                        }
                    }
                    else
                    {
                        ManualCleaningParam.WaferDefineType.Value = string.Empty;
                        ManualCleaningParam.CleaningScrubMode.Value = PWScrubMode.UP_DOWN;
                        ManualCleaningParam.CleaningDirection.Value = CleaningDirection.Up;
                        ManualCleaningParam.ContactLength.Value = 1000;
                        ManualCleaningParam.ContactCount.Value = 10;
                        ManualCleaningParam.ScrubingLength.Value = 10;
                        ManualCleaningParam.OverdriveValue.Value = -1000;
                        ManualCleaningParam.Clearance.Value = -10000;
                        ManualCleaningParam.FocusParam = new NormalFocusParameter();
                        ManualCleaningParam.FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();

                        this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, ManualCleaningParam.FocusParam, 300);

                        ManualCleaningParam.FocusingPointMode.Value = PWFocusingPointMode.POINT1;

                        ManualCleaningParam.FocusingHeightTolerance.Value = 0;

                        ManualCleaningParam.EdgeDetectionBeforeCleaning.Value = true;
                        ManualCleaningParam.PinAlignBeforeCleaning.Value = true;
                        ManualCleaningParam.PinAlignAfterCleaning.Value = true;

                        if (ManualCleaningParam.CenteringLightParams == null)
                        {
                            ManualCleaningParam.CenteringLightParams = new ObservableCollection<LightValueParam>();

                            LightValueParam tmp = new LightValueParam();

                            tmp.Type.Value = EnumLightType.COAXIAL;
                            tmp.Value.Value = 255;

                            ManualCleaningParam.CenteringLightParams.Add(tmp);
                        }

                        if (ManualCleaningParam.FocusParam.LightParams == null)
                        {
                            ManualCleaningParam.FocusParam.LightParams = new ObservableCollection<LightValueParam>();

                            LightValueParam tmp = new LightValueParam();

                            tmp.Type.Value = EnumLightType.COAXIAL;
                            tmp.Value.Value = 70;

                            ManualCleaningParam.FocusParam.LightParams.Add(tmp);
                        }
                    }
                }

                //if ((chuckinfo.WaferStatus == EnumSubsStatus.EXIST) && (chuckinfo.Substrate.WaferType.Value == EnumWaferType.POLISH))
                //{
                //    if (chuckinfo.Substrate.PolishWaferInfo != null)
                //    {
                //        polishwafername = chuckinfo.Substrate.PolishWaferInfo.DefineName.Value;
                //    }
                //}
            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private IStageObject _SelectedStage;
        public IStageObject SelectedStage
        {
            get { return _SelectedStage; }
            set
            {
                if (value != _SelectedStage)
                {
                    _SelectedStage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                PolishWaferParam = this.PolishWaferModule().GetPolishWaferIParam() as PolishWaferParameter;

                ManualCleaningParam = null;
                var cellinfo = LoaderModule.LoaderMaster.GetDeviceInfoClient(LoaderModule.LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index));
                SelectedStageWaferSize = cellinfo.Size.Value;
                if (PolishWaferParam != null)
                {
                    if (PolishWaferParam.PolishWaferIntervalParameters != null && (PolishWaferParam.PolishWaferIntervalParameters.Count > 0))
                    {
                        if (ManualCleaningIntervalParameters != null) 
                        {
                            ManualCleaningIntervalParameters = new ObservableCollection<PolishWaferIntervalParameter>();
                        }

                        foreach (var polishWaferInfos in this.DeviceManager().GetPolishWaferSources())
                        {
                            if (polishWaferInfos.Size.Value == SelectedStageWaferSize)
                            {
                                var filteredParameters = PolishWaferParam.PolishWaferIntervalParameters.Where(item => item.CleaningParameters != null && item.CleaningParameters.Any(cleaningParam => cleaningParam.WaferDefineType.Value == polishWaferInfos.DefineName.Value)).Select(item => item);
                                foreach (var intervalparameter in filteredParameters)
                                {
                                    if (!ManualCleaningIntervalParameters.Contains(intervalparameter))
                                    {
                                        ManualCleaningIntervalParameters.Add(intervalparameter);
                                        if (SelectedIntervalParam == null && SelectedCleaningParam == null)
                                        {
                                            SelectedIntervalParam = intervalparameter;
                                        }
                                    }
                                }
                            }
                        }

                        if (SelectedIntervalParam == null)
                        {
                            isClickedTransferSource = false;
                            isClickedTransferTarget = false;
                            TransferSource = false;
                            TransferTarget = false;
                            TransferEnable = false;
                            LoaderModule.SetLoaderMapConvert(this);
                            LoaderModule.BroadcastLoaderInfo();
                            LoggerManager.Debug($"[LoaderPolishWaferViewModel], PageSwitched() : {(SelectedIntervalParam == null ? "SelectedIntervalParam" : "SelectedCleaningParam")} is null.");
                            this.MetroDialogManager().ShowMessageDialog("[Information]", "Please check polish wafer type data first.\nThere is currently no polish wafer type that matches the device size of the cell.", EnumMessageStyle.Affirmative);
                            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
                        }
                        else if(SelectedIntervalParam.CleaningParameters == null)
                        {
                            isClickedTransferSource = false;
                            isClickedTransferTarget = false;
                            TransferSource = false;
                            TransferTarget = false;
                            TransferEnable = false;
                            LoaderModule.SetLoaderMapConvert(this);
                            LoaderModule.BroadcastLoaderInfo();
                            LoggerManager.Debug($"[LoaderPolishWaferViewModel], PageSwitched() : {(SelectedIntervalParam == null ? "SelectedIntervalParam" : "SelectedCleaningParam")} is null.");
                            this.MetroDialogManager().ShowMessageDialog("[Information]", "Please check polish wafer type data first.\nThere is currently no polish wafer type that matches the device size of the cell.", EnumMessageStyle.Affirmative);
                            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
                        }
                    }
                    else
                    {
                        isClickedTransferSource = false;
                        isClickedTransferTarget = false;
                        TransferSource = false;
                        TransferTarget = false;
                        TransferEnable = false;
                        LoaderModule.SetLoaderMapConvert(this);
                        LoaderModule.BroadcastLoaderInfo();
                        LoggerManager.Debug($"[LoaderPolishWaferViewModel], PageSwitched() : PolishWaferParam.PolishWaferIntervalParameters is null.");
                        this.MetroDialogManager().ShowMessageDialog("[Information]", "Please check the cleaning recipe data first. \nInvalid cleaning wafer parameters.", EnumMessageStyle.Affirmative);
                        return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
                    }
                }
                else
                {
                    isClickedTransferSource = false;
                    isClickedTransferTarget = false;
                    TransferSource = false;
                    TransferTarget = false;
                    TransferEnable = false;
                    LoaderModule.SetLoaderMapConvert(this);
                    LoaderModule.BroadcastLoaderInfo();
                    LoggerManager.Debug($"[LoaderPolishWaferViewModel], PageSwitched() : PolishWaferParam is null.");
                    this.MetroDialogManager().ShowMessageDialog("[Information]", "Please check the cleaning recipe data first. \nInvalid cleaning wafer parameters.", EnumMessageStyle.Affirmative);
                    return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
                }
                //_ManualCleaningParam = new PolishWaferCleaningParameter();

                //SetDefaultCleaningParam();

                LoaderModule.SetLoaderMapConvert(this);
                LoaderModule.BroadcastLoaderInfo();
                //Cells.Clear();

                //if(Cells.Count != 1)
                //{
                //    Cells.Add(SelectedCell);
                //}
                this.VisionManager().SetDisplayChannel(null, DisplayPort);
                RaisePropertyChanged(nameof(ManualCleaningParam));
                
                if(SelectedCell != null)
                {
                    if (SelectedCell.WaferStatus == EnumSubsStatus.EXIST)// Source가 Cell일 때
                    {
                        if (SelectedCell.WaferObj.WaferType.Value == EnumWaferType.POLISH)
                        {
                            TransferSource = SelectedCell;
                            var moduleType = SelectedCell.WaferObj.OriginHolder.ModuleType;
                        
                            int idx = SelectedCell.WaferObj.OriginHolder.Index;
                          
                            if (moduleType == ModuleTypeEnum.FIXEDTRAY ||
                                moduleType == ModuleTypeEnum.INSPECTIONTRAY)
                            {
                                IsCleaning = true;
                                TransferTarget = GetAttachObject(moduleType, idx);
                                isClickedTransferTarget = false;
                                isClickedTransferSource = false;
                            }
                        } 
                    }
                    else if (SelectedCell.WaferStatus == EnumSubsStatus.NOT_EXIST) //Target이 Cell 일때
                    {
                        IsCleaning = false;
                        TransferTarget = SelectedCell;
                        isClickedTransferTarget = false;
                        isClickedTransferSource = true;
                    }
                    this.UpdateTrayUI();
                    ModuleInfoEnable = false;
                }
                else
                {
                    LoggerManager.Debug($"PageSwitched(), Selected Cell is null. ");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                this.PolishWaferParam = null;
                this.SelectedCell = null;
                this.SelectedIntervalParam = null;
                this.SelectedCleaningParam = null;

                this.TransferSource = null;
                this.TransferTarget = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private AsyncCommand _ManualPWCleaningCommand;
        public IAsyncCommand ManualPWCleaningCommand
        {
            get
            {
                if (_ManualPWCleaningCommand == null) _ManualPWCleaningCommand = new AsyncCommand(ManualPWCleaningCommandFunc);
                return _ManualPWCleaningCommand;
            }
        }

        private async Task ManualPWCleaningCommandFunc()
        {
            try
            {
                IsCleaning = false;
                bool IsManualOp = false;

                IStageSupervisorProxy stageproxy = null;

                stageproxy = _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();

                if ((stageproxy.GetWaferStatus() == EnumSubsStatus.EXIST) &&
                     (stageproxy.GetWaferType() == EnumWaferType.POLISH))
                {
                    if ((_ManualCleaningParam != null) && (_ManualCleaningParam.WaferDefineType.Value != string.Empty))
                    {
                        byte[] param = this.ObjectToByteArray(ManualCleaningParam);

                        EventCodeEnum cleaningRetVal = EventCodeEnum.UNDEFINED;
                        cleaningRetVal = await _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>()?.DoManualPolishWaferCleaningCommand(param);
                         if(cleaningRetVal == EventCodeEnum.NONE)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Information", "Successed Card Cleaning.\n", EnumMessageStyle.Affirmative);
                        }
                        else
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Information", $"Failed Card Cleaning.\nError Code: {cleaningRetVal}\n", EnumMessageStyle.Affirmative);
                        }
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Information", "Cleaning data is wrong.\n", EnumMessageStyle.Affirmative);
                    }
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Information", "Need a polish wafer on the chuck.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                IsCleaning = true;
                //_LoaderCommunicationManager.SetLoaderWorkingFlag(false);
            }
        }
       
        private RelayCommand<Object> _CleaningParamCopyCommand;
        public ICommand CleaningParamCopyCommand
        {
            get
            {
                if (null == _CleaningParamCopyCommand) _CleaningParamCopyCommand = new RelayCommand<Object>(CleaningParamCopyCommandFunc);
                return _CleaningParamCopyCommand;
            }
        }

        private void CleaningParamCopyCommandFunc(object param)
        {
            try
            {
                if (SelectedCleaningParam != null)
                {
                    PolishWaferCleaningParameter SelectedCleaningParamConcrete = SelectedCleaningParam as PolishWaferCleaningParameter;

                    _ManualCleaningParam.WaferDefineType.Value = SelectedCleaningParamConcrete.WaferDefineType.Value;

                    _ManualCleaningParam.CleaningScrubMode.Value = SelectedCleaningParamConcrete.CleaningScrubMode.Value;
                    _ManualCleaningParam.CleaningDirection.Value = SelectedCleaningParamConcrete.CleaningDirection.Value;
                    _ManualCleaningParam.ContactLength.Value = SelectedCleaningParamConcrete.ContactLength.Value;
                    _ManualCleaningParam.ContactCount.Value = SelectedCleaningParamConcrete.ContactCount.Value;
                    _ManualCleaningParam.ScrubingLength.Value = SelectedCleaningParamConcrete.ScrubingLength.Value;
                    _ManualCleaningParam.OverdriveValue.Value = SelectedCleaningParamConcrete.OverdriveValue.Value;
                    _ManualCleaningParam.Clearance.Value = SelectedCleaningParamConcrete.Clearance.Value;
                    _ManualCleaningParam.FocusingPointMode.Value = SelectedCleaningParamConcrete.FocusingPointMode.Value;

                    _ManualCleaningParam.FocusingHeightTolerance.Value = SelectedCleaningParamConcrete.FocusingHeightTolerance.Value;

                    _ManualCleaningParam.FocusParam.FocusRange.Value = SelectedCleaningParamConcrete.FocusParam.FocusRange.Value;
                    _ManualCleaningParam.PinAlignBeforeCleaning.Value = SelectedCleaningParamConcrete.PinAlignBeforeCleaning.Value;
                    _ManualCleaningParam.PinAlignAfterCleaning.Value = SelectedCleaningParamConcrete.PinAlignAfterCleaning.Value;
                    _ManualCleaningParam.EdgeDetectionBeforeCleaning.Value = SelectedCleaningParamConcrete.EdgeDetectionBeforeCleaning.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _DefaultSetCommand;
        public ICommand DefaultSetCommand
        {
            get
            {
                if (null == _DefaultSetCommand) _DefaultSetCommand = new RelayCommand<Object>(DefaultSetCommandFunc);
                return _DefaultSetCommand;
            }
        }

        private void DefaultSetCommandFunc(object param)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _DecimalTextBoxClickCommand;
        public ICommand DecimalTextBoxClickCommand
        {
            get
            {
                if (null == _DecimalTextBoxClickCommand) _DecimalTextBoxClickCommand = new RelayCommand<Object>(FuncDecimalTextBoxClickCommand);
                return _DecimalTextBoxClickCommand;
            }
        }

        private void FuncDecimalTextBoxClickCommand(object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _FloatTextBoxClickCommand;
        public ICommand FloatTextBoxClickCommand
        {
            get
            {
                if (null == _FloatTextBoxClickCommand) _FloatTextBoxClickCommand = new RelayCommand<Object>(FloatTextBoxClickCommandFunc);
                return _FloatTextBoxClickCommand;
            }
        }

        private void FloatTextBoxClickCommandFunc(object param)
        {
            try
            {
                string backuptext = "";

                (System.Windows.Controls.TextBox, IElement) paramVal = ((System.Windows.Controls.TextBox, IElement))param;
                System.Windows.Controls.TextBox textbox = paramVal.Item1;
                IElement element = paramVal.Item2;
                backuptext = textbox.Text;

                if (textbox != null && element != null)
                {
                    textbox.Text = VirtualKeyboard.Show(textbox.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 7);

                    if (Convert.ToInt32(textbox.Text) >= (int)element.LowerLimit && Convert.ToInt32(textbox.Text) <= (int)element.UpperLimit)
                    {

                    }
                    else
                    {
                        textbox.Text = backuptext;
                        var ret = this.MetroDialogManager().ShowMessageDialog($"Value Limit Error", $"Out of set value, Lower Limit : {element.LowerLimit} , Upper Limit : {element.UpperLimit}, \n Set to previous value", EnumMessageStyle.Affirmative);
                    }
                    textbox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void SetAllObjectEnable()
        {
            int cellidx = 0;

            if (SelectedCell != null)
            {
                SelectedCell.IsEnableTransfer = true;
            }

            //foreach (var cell in Cells)
            //{
            //    cell.IsEnableTransfer = true;
            //}

            foreach (var ft in FTs)
            {
                ft.IsEnableTransfer = true;
            }

        }


        private void SetNotExistObjectDisable()
        {
            int cellidx = 0;

            if (SelectedCell != null)
            {
                if (SelectedCell.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST | SelectedCell.CardStatus == EnumSubsStatus.EXIST)
                {
                    SelectedCell.IsEnableTransfer = true;
                }
                else
                {
                    SelectedCell.IsEnableTransfer = false;
                }
            }

            //foreach (var cell in Cells)
            //{
            //    if (cell.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST
            //         | cell.CardStatus == EnumSubsStatus.EXIST)
            //        cell.IsEnableTransfer = true;
            //    else
            //        cell.IsEnableTransfer = false;
            //}

            foreach (var ft in FTs)
            {
                if (ft.WaferStatus != EnumSubsStatus.EXIST)
                    ft.IsEnableTransfer = false;
            }
        }

        private void SetExistObjectDisable()
        {
            int cellidx = 0;

            if (SelectedCell != null)
            {
                if (SelectedCell.WaferStatus == ProberInterfaces.EnumSubsStatus.NOT_EXIST || SelectedCell.CardStatus == ProberInterfaces.EnumSubsStatus.NOT_EXIST)
                {
                    SelectedCell.IsEnableTransfer = true;
                }
                else
                {
                    SelectedCell.IsEnableTransfer = false;
                }
            }

            //foreach (var cell in Cells)
            //{

            //    if (cell.WaferStatus == ProberInterfaces.EnumSubsStatus.NOT_EXIST || cell.CardStatus == ProberInterfaces.EnumSubsStatus.NOT_EXIST)
            //        cell.IsEnableTransfer = true;
            //    else
            //    {
            //        cell.IsEnableTransfer = false;
            //    }
            //}

            foreach (var ft in FTs)
            {
                if (ft.WaferStatus != EnumSubsStatus.NOT_EXIST)
                    ft.IsEnableTransfer = false;
            }

        }


        private AsyncCommand _TransferSourceClickCommand;
        public ICommand TransferSourceClickCommand
        {
            get
            {
                if (null == _TransferSourceClickCommand) _TransferSourceClickCommand = new AsyncCommand(TransferSourceClickCommandFunc);
                return _TransferSourceClickCommand;
            }
        }
        private async Task TransferSourceClickCommandFunc()
        {
            try
            {
                if (!targetToggle)
                {
                    TransferTarget = null;
                    isClickedTransferTarget = false;
                    SetAllObjectEnable();
                    IsSelected = false;
                    targetToggle = true;
                }
                if (sourceToggle)
                {
                    if (!isClickedTransferTarget)
                    {
                        IsSelected = false;
                        TransferSource = true;
                        isClickedTransferSource = true;
                        SetAllObjectEnable();
                        SetNotExistObjectDisable();


                        if (TransferTarget != null)
                        {
                            TransferTarget = null;
                            isClickedTransferTarget = false;
                            //if (IsCardObject(TransferTarget))
                            //{
                            //    isSourceCard = true;
                            //    SetDisalbeDontMoveCard();
                            //}
                            //else
                            //    SetDisableDontMoveWafer();
                        }

                    }
                    sourceToggle = false;
                }
                else
                {

                    TransferSource = null;
                    isClickedTransferSource = false;
                    SetAllObjectEnable();
                    IsSelected = false;
                    sourceToggle = true;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private AsyncCommand _TransferTargetClickCommand;
        public ICommand TransferTargetClickCommand
        {
            get
            {
                if (null == _TransferTargetClickCommand) _TransferTargetClickCommand = new AsyncCommand(TransferTargetClickCommandFunc);
                return _TransferTargetClickCommand;
            }
        }
        private async Task TransferTargetClickCommandFunc()
        {
            try
            {
                if (!sourceToggle)
                {
                    if (TransferSource is bool)
                    {
                        return;
                    }
                }
                if (targetToggle)
                {
                    if (!isClickedTransferSource)
                    {
                        IsSelected = false;
                        isClickedTransferTarget = true;
                        TransferTarget = true;
                        SetAllObjectEnable();
                        SetExistObjectDisable();

                        if (TransferSource != null)
                        {
                            if (TransferSource is StageObject)
                            {
                                if (SelectedCell != null)
                                {
                                    SelectedCell.IsEnableTransfer = false;
                                }

                                //foreach (var cell in Cells)
                                //{
                                //    cell.IsEnableTransfer = false;
                                //}
                            }
                            if (TransferSource is StageObject && (TransferSource as StageObject).CardStatus == EnumSubsStatus.EXIST && (TransferSource as StageObject).WaferStatus == EnumSubsStatus.EXIST)
                            {

                            }
                            else { }
                            //SetDisableDontMoveWafer();
                        }
                    }
                    targetToggle = false;
                }
                else
                {
                    TransferTarget = null;
                    isClickedTransferTarget = false;
                    SetAllObjectEnable();
                    IsSelected = false;
                    targetToggle = true;
                    //if (IsTargetCardObject(TransferTarget))
                    //{
                    //    isSourceCard = true;
                    //}
                    //else
                    //{
                    //    isSourceCard = false;
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private bool SetTransferInfoToModule(object param, ref object Id, bool issource, ref EventCodeEnum reason)
        {
            bool retval = false;

            try
            {
                var loader = LoaderModule;
                Map = loader.GetLoaderInfo().StateMap;

                HolderModuleInfo holdermodule = null;
                HolderModuleInfo[] holdermoduleinfos = null;
                int index = -1;

                if (param is StageObject)
                {
                    index = Convert.ToInt32(((param as StageObject).Name.Split('#'))[1]);
                    holdermoduleinfos = Map.ChuckModules;

                    //if (issource)
                    //{
                    //    holdermodule = Map.ChuckModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == chuckindex);
                    //}
                    //else
                    //{
                    //    holdermodule = Map.ChuckModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == chuckindex);
                    //}

                    //if (holdermodule == null)
                    //{
                    //    return false;
                    //}

                    //if (issource)
                    //{
                    //    Id = holdermodule.Substrate.ID.Value;
                    //}
                    //else
                    //{
                    //    Id = holdermodule.ID;
                    //}
                }
                else if (param is BufferObject)
                {
                    index = Convert.ToInt32(((param as BufferObject).Name.Split('#'))[1]);
                    holdermoduleinfos = Map.BufferModules;

                    //if (issource)
                    //{
                    //    holdermodule = Map.BufferModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == bufferindex);
                    //}
                    //else
                    //{
                    //    holdermodule = Map.BufferModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == bufferindex);
                    //}

                    //if (holdermodule == null)
                    //{
                    //    return false;
                    //}

                    //if (issource)
                    //{
                    //    Id = holdermodule.Substrate.ID.Value;
                    //}
                    //else
                    //{
                    //    Id = holdermodule.ID;
                    //}
                }
                else if (param is CardTrayObject)
                {
                    index = Convert.ToInt32(((param as CardTrayObject).Name.Split('#'))[1]);
                    holdermoduleinfos = Map.CardTrayModules;

                    //if (issource)
                    //{
                    //    holdermodule = Map.CardTrayModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == cardtaryindex);
                    //}
                    //else
                    //{
                    //    holdermodule = Map.CardTrayModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == cardtaryindex);
                    //}

                    //if (holdermodule == null)
                    //{
                    //    return false;
                    //}

                    //if (issource)
                    //{
                    //    Id = holdermodule.Substrate.ID.Value;
                    //}
                    //else
                    //{
                    //    Id = holdermodule.ID;
                    //}
                }
                else if (param is FixedTrayInfoObject)
                {
                    index = Convert.ToInt32(((param as FixedTrayInfoObject).Name.Split('#'))[1]);
                    holdermoduleinfos = Map.FixedTrayModules;

                    //if (issource)
                    //{
                    //    holdermodule = Map.FixedTrayModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == fixedtrayindex);
                    //}
                    //else
                    //{
                    //    holdermodule = Map.FixedTrayModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == fixedtrayindex);
                    //}

                    //if (holdermodule == null)
                    //{
                    //    return false;
                    //}

                    //if (issource)
                    //{
                    //    Id = holdermodule.Substrate.ID.Value;
                    //}
                    //else
                    //{
                    //    Id = holdermodule.ID;
                    //}
                }
                else if (param is InspectionTrayInfoObject)
                {
                    index = Convert.ToInt32(((param as InspectionTrayInfoObject).Name.Split('#'))[1]);
                    holdermoduleinfos = Map.InspectionTrayModules;

                    //if (issource)
                    //{
                    //    holdermodule = Map.InspectionTrayModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == iNSPtrayindex);
                    //}
                    //else
                    //{
                    //    holdermodule = Map.InspectionTrayModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == iNSPtrayindex);
                    //}

                    //if (holdermodule == null)
                    //{
                    //    return false;
                    //}

                    //if (issource)
                    //{
                    //    Id = holdermodule.Substrate.ID.Value;
                    //}
                    //else
                    //{
                    //    Id = holdermodule.ID;
                    //}
                }

                if (issource)
                {
                    holdermodule = holdermoduleinfos.FirstOrDefault(i => i.Substrate != null && i.ID.Index == index);
                }
                else
                {
                    holdermodule = holdermoduleinfos.FirstOrDefault(i => i.Substrate == null && i.ID.Index == index);
                }

                if (holdermodule == null)
                {
                    if (param is StageObject)
                    {
                        reason = EventCodeEnum.TRANSFER_ALREADY_EXIST_ON_CHUCK;
                    }
                    else if (param is FixedTrayInfoObject)
                    {
                        reason = EventCodeEnum.TRANSFER_ALREADY_EXIST_ON_FIXEDTRAY;
                    }
                    else if (param is InspectionTrayInfoObject)
                    {
                        reason = EventCodeEnum.TRANSFER_ALREADY_EXIST_ON_INSPECTIONTRAY;
                    }

                    retval = false;
                }
                else
                {
                    if (issource)
                    {
                        Id = holdermodule.Substrate.ID.Value;
                    }
                    else
                    {
                        Id = holdermodule.ID;
                    }

                    retval = true;
                }
            }
            catch (Exception err)
            {
                retval = false;
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetTransfer(string id, ModuleID destinationID)
        {
            TransferObject subObj = Map.GetTransferObjectAll().Where(item => item.ID.Value == id).FirstOrDefault();
            ModuleInfoBase dstLoc = Map.GetLocationModules().Where(item => item.ID == destinationID).FirstOrDefault();
            bool stageBusy = true;
            if (subObj == null)
            {
                Map = null;
            }

            if (dstLoc == null)
            {
                Map = null;
            }

            if (subObj.CurrPos == destinationID)
            {
                Map = null;
            }

            if (subObj.CurrPos.ModuleType == ModuleTypeEnum.CHUCK)
            {
                stageBusy = LoaderModule.LoaderMaster.GetClient(subObj.CurrPos.Index).GetRunState();
            }
            if (stageBusy == false)
            {
                var retVal = (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{subObj.CurrPos.Index} is Busy Right Now", EnumMessageStyle.Affirmative).Result;

                if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    return;
                }
            }

            var card = this.LoaderModule.ModuleManager.GetTransferObjectAll().Where(item => item.ID.Value == id).FirstOrDefault();
            if (card != null)
            {
                card.CardSkip = ProberInterfaces.Enum.CardSkipEnum.NONE;
            }
            if (dstLoc is HolderModuleInfo)
            {
                var currHolder = Map.GetHolderModuleAll().Where(item => item.ID == subObj.CurrHolder).FirstOrDefault();
                var dstHolder = dstLoc as HolderModuleInfo;

                subObj.PrevHolder = subObj.CurrHolder;
                subObj.PrevPos = subObj.CurrPos;

                subObj.CurrHolder = destinationID;
                subObj.CurrPos = destinationID;

                if (dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK)
                {
                    stageBusy = LoaderModule.LoaderMaster.GetClient(dstLoc.ID.Index).GetRunState();

                    if (stageBusy == false)
                    {
                        var retVal = (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{dstLoc.ID.Index} is Busy Right Now", EnumMessageStyle.Affirmative).Result;

                        if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            return;
                        }
                    }
                    if (LoaderModule.LoaderMaster.ClientList.ContainsKey(dstLoc.ID.ToString()))
                    {
                        var deviceInfo = LoaderModule.LoaderMaster.GetDeviceInfoClient(LoaderModule.LoaderMaster.ClientList[dstLoc.ID.ToString()]);
                        if (deviceInfo != null)
                        {
                            subObj.NotchAngle.Value = deviceInfo.NotchAngle.Value;
                        }
                    }
                    else
                    {

                    }
                }
                else if (dstLoc.ID.ModuleType == ModuleTypeEnum.SLOT)
                {
                    subObj.NotchAngle.Value = (this.LoaderModule.ModuleManager.FindModule(dstLoc.ID) as ISlotModule).Cassette.Device.LoadingNotchAngle.Value;
                }

                currHolder.WaferStatus = EnumSubsStatus.NOT_EXIST;
                currHolder.Substrate = null;
                dstHolder.WaferStatus = EnumSubsStatus.EXIST;
                dstHolder.Substrate = subObj;
            }
            else
            {
                subObj.PrevPos = subObj.CurrPos;
                subObj.CurrPos = destinationID;
            }
        }

        LoaderMap Map;

        private AsyncCommand _TransferSourceToCellCommand;
        public ICommand TransferSourceToCellCommand
        {
            get
            {
                if (null == _TransferSourceToCellCommand) _TransferSourceToCellCommand = new AsyncCommand(TransferSourceToCellCommandFunc);
                return _TransferSourceToCellCommand;
            }
        }

        private async Task TransferSourceToCellCommandFunc()
        {
            try
            {
                bool isDestChuck = false;
                //TransferSource = null;
                //TransferTarget = null;
                await Task.Run(() =>
                {
                    Map = LoaderModule.GetLoaderInfo().StateMap;

                    string sourceId = null;
                    ModuleID targetId = new ModuleID();
                    object transfermodule = null;

                    EventCodeEnum reason = EventCodeEnum.UNDEFINED;

                    //Source Info
                    SetTransferInfoToModule(TransferSource, ref transfermodule, true, ref reason);
                    sourceId = (string)transfermodule;

                    if (SelectedCell != null)
                    {
                        TransferTarget = SelectedCell;
                    }

                    //Target Info
                    SetTransferInfoToModule(TransferTarget, ref transfermodule, false, ref reason);
                    targetId = (ModuleID)transfermodule;
                    if (TransferTarget is StageObject)
                    {
                        isDestChuck = true;
                    }
                    else
                    {
                        isDestChuck = false;
                    }

                    if (sourceId == null | targetId == null)
                        return;

                    SetTransfer(sourceId, targetId);
                    var mapSlicer = new LoaderMapSlicer();
                    var slicedMap = mapSlicer.ManualSlicing(Map);

                    for (int i = 0; i < slicedMap.Count; i++)
                    {
                        LoaderModule.SetRequest(slicedMap[i]);
                        while (true)
                        {

                            if (LoaderModule.ModuleState == ModuleStateEnum.DONE)
                            {
                                LoaderModule.ClearRequestData();

                                LoaderMapConvert(LoaderModule.GetLoaderInfo().StateMap);

                                Thread.Sleep(100);

                                break;
                            }
                            else if (LoaderModule.ModuleState == ModuleStateEnum.ERROR)
                            {
                                isDestChuck = false;
                                LoaderRecoveryControlVM.Show(this.GetLoaderContainer(), LoaderModule.ResonOfError, LoaderModule.ErrorDetails);
                                LoaderModule.ResonOfError = "";
                                LoaderModule.ErrorDetails = "";
                                break;
                            }
                            Thread.Sleep(100);
                        }


                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(33);
                });

                //if (IsCleanningEnable && isDestChuck)
                if (isDestChuck)
                {
                    bool IsManualOp = false;

                    IStageSupervisorProxy stageproxy = null;

                    stageproxy = _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();

                
                        for (int i = 0; i < 10; i++)
                        {
                            Thread.Sleep(2000);

                            if (stageproxy.GetWaferStatus() == EnumSubsStatus.EXIST)
                            {
                               
                                break;
                            }
                        }
                    

                   

                        if ((stageproxy.GetWaferStatus() == EnumSubsStatus.EXIST) &&
                         (stageproxy.GetWaferType() == EnumWaferType.POLISH))
                        {
                            if ((_ManualCleaningParam != null) && (_ManualCleaningParam.WaferDefineType.Value != string.Empty))
                            {
                                byte[] param = this.ObjectToByteArray(ManualCleaningParam);
                                //await Task.Run(async () => _LoaderCommunicationManager.WaitStageJob());
                                await _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>()?.DoManualPolishWaferCleaningCommand(param);
                            }
                            else
                            {
                                await this.MetroDialogManager().ShowMessageDialog("Information", "Cleaning data is wrong.\n", EnumMessageStyle.Affirmative);
                            }
                        }
                        else
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Information", "Need a polish wafer on the chuck.", EnumMessageStyle.Affirmative);
                        }
                    
                }

                TransferTarget = null;
                isClickedTransferTarget = false;
                SetAllObjectEnable();
                IsSelected = false;
                targetToggle = true;
                TransferSource = null;
                isClickedTransferSource = false;
                sourceToggle = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _TransferCellToSourceCommand;
        public ICommand TransferCellToSourceCommand
        {
            get
            {
                if (null == _TransferCellToSourceCommand) _TransferCellToSourceCommand = new AsyncCommand(TransferCellToSourceCommandFunc);
                return _TransferCellToSourceCommand;
            }
        }

        private async Task TransferCellToSourceCommandFunc()
        {
            try
            {
                bool isDestChuck = false;
                //TransferSource = null;
                //TransferTarget = null;
                await Task.Run(() =>
                {
                    Map = LoaderModule.GetLoaderInfo().StateMap;

                    string sourceId = null;
                    ModuleID targetId = new ModuleID();
                    object transfermodule = null;

                    if (SelectedCell != null)
                    {
                        TransferTarget = TransferSource;
                    }

                    TransferSource = SelectedCell;

                    EventCodeEnum reason = EventCodeEnum.UNDEFINED;

                    //Source Info
                    SetTransferInfoToModule(TransferSource, ref transfermodule, true, ref reason);
                    sourceId = (string)transfermodule;

                    //Target Info
                    SetTransferInfoToModule(TransferTarget, ref transfermodule, false, ref reason);
                    targetId = (ModuleID)transfermodule;
                    if (TransferTarget is StageObject)
                    {
                        isDestChuck = true;
                    }
                    else
                    {
                        isDestChuck = false;
                    }

                    if (sourceId == null | targetId == null)
                        return;

                    SetTransfer(sourceId, targetId);
                    var mapSlicer = new LoaderMapSlicer();
                    var slicedMap = mapSlicer.ManualSlicing(Map);

                    for (int i = 0; i < slicedMap.Count; i++)
                    {
                        LoaderModule.SetRequest(slicedMap[i]);
                        while (true)
                        {

                            if (LoaderModule.ModuleState == ModuleStateEnum.DONE)
                            {
                                LoaderModule.ClearRequestData();

                                LoaderMapConvert(LoaderModule.GetLoaderInfo().StateMap);

                                Thread.Sleep(100);

                                break;
                            }
                            else if (LoaderModule.ModuleState == ModuleStateEnum.ERROR)
                            {
                                isDestChuck = false;
                                LoaderRecoveryControlVM.Show(this.GetLoaderContainer(), LoaderModule.ResonOfError, LoaderModule.ErrorDetails);
                                LoaderModule.ResonOfError = "";
                                LoaderModule.ErrorDetails = "";
                                break;
                            }
                            Thread.Sleep(100);
                        }


                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(33);
                });

                //if (IsCleanningEnable && isDestChuck)
                if (isDestChuck)

                {
                    IStageSupervisorProxy stageproxy = null;

                    stageproxy = _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();

                    bool isMovingState = false;
                    if (isMovingState == false)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Thread.Sleep(2000);

                            if (stageproxy.GetWaferStatus() == EnumSubsStatus.EXIST)
                            {
                                isMovingState = true;
                                break;
                            }
                        }
                    }

                    if (isMovingState == true)
                    {
                        bool IsManualOp = false;

                        stageproxy = _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();

                        if ((stageproxy.GetWaferStatus() == EnumSubsStatus.EXIST) &&
                             (stageproxy.GetWaferType() == EnumWaferType.POLISH))
                        {
                            if ((_ManualCleaningParam != null) && (_ManualCleaningParam.WaferDefineType.Value != string.Empty))
                            {
                                byte[] param = this.ObjectToByteArray(ManualCleaningParam);
                                //await Task.Run(async () => _LoaderCommunicationManager.WaitStageJob());
                                _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>()?.DoManualPolishWaferCleaningCommand(param);
                            }
                            else
                            {
                                await this.MetroDialogManager().ShowMessageDialog("Information", "Cleaning data is wrong.\n", EnumMessageStyle.Affirmative);
                            }
                        }
                        else
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Information", "Need a polish wafer on the chuck.", EnumMessageStyle.Affirmative);
                        }
                    }
                }

                TransferTarget = null;
                isClickedTransferTarget = false;
                SetAllObjectEnable();
                IsSelected = false;
                targetToggle = true;
                TransferSource = null;
                isClickedTransferSource = false;
                sourceToggle = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum ValidatiaonManualCleaningParam(string sourceguid, ref string failReason)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (_ManualCleaningParam == null)
                {
                    failReason = $"ManualCleaningParam is null.";

                    return retval;
                }

                if (_ManualCleaningParam.FocusingPointMode.Value == PWFocusingPointMode.UNDEFINED)
                {
                    failReason = $"ManualCleaningParam, FocusingPointMode is {_ManualCleaningParam.FocusingPointMode.Value}";

                    return retval;
                }

                if (_ManualCleaningParam.WaferDefineType.Value == string.Empty)
                {
                    failReason = $"ManualCleaningParam, WaferDefineType is empty.";

                    return retval;
                }

                LoaderMap loadermap = LoaderModule.GetLoaderInfo().StateMap;

                //var loadablePolishWafers = loadermap.GetTransferObjectAll().Where(item => item.WaferType.Value == EnumWaferType.POLISH &&
                //                                                                          (item.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY || item.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY) &&
                //                                                                          item.ReservationState == ReservationStateEnum.NONE &&
                //                                                                          item.PolishWaferInfo != null &&
                //                                                                          item.PolishWaferInfo.DefineName.Value == _ManualCleaningParam.WaferDefineType.Value).ToList();

                //TransferObject loadWafer = loadablePolishWafers.OrderBy(item => item.OriginHolder.Index).FirstOrDefault();

                var tmpsubs = loadermap.GetSubstrateByGUID(sourceguid);

                if (tmpsubs == null)
                {
                    failReason = $"Can't find source.";

                    return retval;
                }

                if (tmpsubs.PolishWaferInfo != null)
                {
                    if (tmpsubs.PolishWaferInfo.DefineName.Value != _ManualCleaningParam.WaferDefineType.Value)
                    {
                        failReason = $"Define name is not matched.";

                        return retval;
                    }
                }
                else
                {
                    failReason = $"Source's polishwafer information is not enough.";

                    return retval;
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.EXCEPTION;

                LoggerManager.Exception(err);
            }

            return retval;
        }

        private AsyncCommand _TransferCommand;
        public ICommand TransferCommand
        {
            get
            {
                if (null == _TransferCommand) _TransferCommand = new AsyncCommand(TransferObjectFunc);
                return _TransferCommand;
            }
        }

        private async Task TransferObjectFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                bool isDestChuck = false;
                //TransferSource = null;
                //TransferTarget = null;


                Map = LoaderModule.GetLoaderInfo().StateMap;

                string sourceId = null;
                ModuleID targetId = new ModuleID();
                object transfermodule = null;

                bool CanUseSource = false;
                bool CanUseTarget = false;
                EventCodeEnum SourceReason = EventCodeEnum.UNDEFINED;
                EventCodeEnum TargetReason = EventCodeEnum.UNDEFINED;

                //Source Info
                CanUseSource = SetTransferInfoToModule(TransferSource, ref transfermodule, true, ref SourceReason);

                if (CanUseSource == true)
                {
                    sourceId = (string)transfermodule;
                }

                //Target Info
                CanUseTarget = SetTransferInfoToModule(TransferTarget, ref transfermodule, false, ref TargetReason);

                if (CanUseTarget == true)
                {
                    targetId = (ModuleID)transfermodule;
                }

                if (CanUseSource == true && CanUseTarget == true)
                {
                    if (TransferTarget is StageObject)
                    {
                        isDestChuck = true;
                    }
                    else
                    {
                        isDestChuck = false;
                    }

                    if (isDestChuck == true)
                    {
                        string validationStr = string.Empty;

                        retval = ValidatiaonManualCleaningParam(sourceId, ref validationStr);

                        // Before Pin Align Turn Off & Pin Align State is Idle => Can not operate cleaning manually.

                        if (_ManualCleaningParam.PinAlignBeforeCleaning.Value == false)
                        {
                            Element<AlignStateEnum> PinAlignState = _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>()?.GetAlignState(AlignTypeEnum.Pin);

                            if (PinAlignState == null || PinAlignState.Value != AlignStateEnum.DONE)
                            {
                                await this.MetroDialogManager().ShowMessageDialog("Information",
                                $"Pin alignment data is required for cleaning.\n" +
                                $"Turn on the parameter of pin align before cleaning or perform pin alignment before operating.", EnumMessageStyle.Affirmative);

                                return;
                            }
                        }

                        if (retval != EventCodeEnum.NONE)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Information", $"Validation failed.\n" + $"{validationStr}", EnumMessageStyle.Affirmative);

                            return;
                        }
                    }

                    if (sourceId == null | targetId == null)
                        return;

                    bool canusearm = false;
                    for (int i = 0; i < Map.ARMModules.Count(); i++)
                    {
                        if (Map.ARMModules[i].WaferStatus == EnumSubsStatus.NOT_EXIST)
                        {
                            canusearm = true;
                            break;
                        }
                    }

                    if (canusearm == false)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Transfer Warning", $"There is already a wafer on the ARM.\n Please Check the ARM.", EnumMessageStyle.Affirmative);
                        return;
                    }


                    bool canusePA = false;
                    for (int i = 0; i < Map.PreAlignModules.Count(); i++)
                    {
                        if (Map.PreAlignModules[i].WaferStatus == EnumSubsStatus.NOT_EXIST)
                        {
                            canusePA = true;
                            break;
                        }
                    }

                    if (canusePA == false)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Transfer Warning", $"There is already a wafer on the PreAlign.\n Please Check the PreAlign.", EnumMessageStyle.Affirmative);
                        return;
                    }

                    string transferToTrayFailReason = "";
                    retval = this.TranferManager().ValidationTransferToTray(TransferSource, TransferTarget, ref transferToTrayFailReason);
                    if(retval != EventCodeEnum.NONE)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Transfer Error", $"{transferToTrayFailReason}", EnumMessageStyle.Affirmative);
                        return;
                    }

                    SetTransfer(sourceId, targetId);
                    var mapSlicer = new LoaderMapSlicer();
                    var slicedMap = mapSlicer.ManualSlicing(Map);

                    bool isError = false;

                    for (int i = 0; i < slicedMap.Count; i++)
                    {
                        LoaderModule.SetRequest(slicedMap[i]);

                        while (true)
                        {

                            if (LoaderModule.ModuleState == ModuleStateEnum.DONE)
                            {
                                LoaderModule.ClearRequestData();

                                LoaderMapConvert(LoaderModule.GetLoaderInfo().StateMap);

                                Thread.Sleep(100);

                                break;
                            }
                            else if (LoaderModule.ModuleState == ModuleStateEnum.ERROR)
                            {
                                isDestChuck = false;
                                LoaderRecoveryControlVM.Show(this.GetLoaderContainer(), LoaderModule.ResonOfError, LoaderModule.ErrorDetails);
                                LoaderModule.ResonOfError = "";
                                LoaderModule.ErrorDetails = "";
                                isError = true;
                                break;
                            }
                            Thread.Sleep(100);
                        }

                        if (isError)
                        {
                            break;
                        }

                        Thread.Sleep(1000);
                    }

                    Thread.Sleep(33);
                    UpdateTrayUI();
                    if(SelectedCell.WaferStatus == EnumSubsStatus.NOT_EXIST)
                    {
                        IsCleaning = false;
                        TransferSource = null;
                        TransferTarget = SelectedCell;
                        isClickedTransferSource = true;
                        isClickedTransferTarget = false;
                    }
                    else
                    {
                        IsCleaning = true;
                        TransferSource = true;
                        TransferSource = SelectedCell;

                        var moduleType = SelectedCell.WaferObj.OriginHolder.ModuleType;
                        int idx = SelectedCell.WaferObj.OriginHolder.Index;

                        if (moduleType == ModuleTypeEnum.FIXEDTRAY ||
                            moduleType == ModuleTypeEnum.INSPECTIONTRAY)
                        {
                            TransferTarget = GetAttachObject(moduleType, idx);
                            isClickedTransferTarget = false;
                            isClickedTransferSource = false;
                        }
                    }
                }
                else
                {
                    if (CanUseTarget == false)
                    {
                        int idx = -1;
                        if(SelectedCell != null && SelectedCell.WaferObj != null && SelectedCell.WaferObj.OriginHolder != null)
                        {
                            idx = SelectedCell.WaferObj.OriginHolder.Index;
                        }
                        // 이미 척에 웨이퍼가 있는 경우
                        if (TargetReason == EventCodeEnum.TRANSFER_ALREADY_EXIST_ON_CHUCK)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Information", $"Already exist wafer on chuck.", EnumMessageStyle.Affirmative);
                        }
                        else if (TargetReason == EventCodeEnum.TRANSFER_ALREADY_EXIST_ON_CHUCK)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Information", $"Fixed Tray#{idx} is not available.", EnumMessageStyle.Affirmative);
                        }
                        else if(TargetReason == EventCodeEnum.TRANSFER_ALREADY_EXIST_ON_INSPECTIONTRAY)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Information", $"Inspection Tray#{idx} is not available.", EnumMessageStyle.Affirmative);
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        private AsyncCommand _ManualTransferAndCleaningCommand;
        public ICommand ManualTransferAndCleaningCommand
        {
            get
            {
                if (null == _ManualTransferAndCleaningCommand) _ManualTransferAndCleaningCommand = new AsyncCommand(ManualTransferAndCleaningCommandFunc);
                return _ManualTransferAndCleaningCommand;
            }
        }

        private async Task ManualTransferAndCleaningCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                IsCleaning = false;
                bool isDestChuck = false;
                Map = LoaderModule.GetLoaderInfo().StateMap;

                string sourceId = null;
                ModuleID targetId = new ModuleID();
                object transfermodule = null;

                bool CanUseSource = false;
                bool CanUseTarget = false;
                EventCodeEnum SourceReason = EventCodeEnum.UNDEFINED;
                EventCodeEnum TargetReason = EventCodeEnum.UNDEFINED;

                //Source Info
                CanUseSource = SetTransferInfoToModule(TransferSource, ref transfermodule, true, ref SourceReason);

                if (CanUseSource == true)
                {
                    sourceId = (string)transfermodule;
                }

                //Target Info
                CanUseTarget = SetTransferInfoToModule(TransferTarget, ref transfermodule, false, ref TargetReason);

                if (CanUseTarget == true)
                {
                    targetId = (ModuleID)transfermodule;
                }

                if (CanUseSource == true && CanUseTarget == true)
                {
                    if (TransferTarget is StageObject)
                    {
                        isDestChuck = true;
                    }
                    else
                    {
                        isDestChuck = false;
                    }

                    if (isDestChuck == true)
                    {
                        string validationStr = string.Empty;

                        retval = ValidatiaonManualCleaningParam(sourceId, ref validationStr);

                        // Before Pin Align Turn Off & Pin Align State is Idle => Can not operate cleaning manually.

                        if (_ManualCleaningParam.PinAlignBeforeCleaning.Value == false)
                        {
                            Element<AlignStateEnum> PinAlignState = _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>()?.GetAlignState(AlignTypeEnum.Pin);

                            if (PinAlignState == null || PinAlignState.Value != AlignStateEnum.DONE)
                            {
                                await this.MetroDialogManager().ShowMessageDialog("Information",
                                $"Pin alignment data is required for cleaning.\n" +
                                $"Turn on the parameter of pin align before cleaning or perform pin alignment before operating.", EnumMessageStyle.Affirmative);

                                return;
                            }
                        }

                        if (retval != EventCodeEnum.NONE)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Information", $"Validation failed.\n" + $"{validationStr}", EnumMessageStyle.Affirmative);

                            return;
                        }
                    }

                    if (sourceId == null | targetId == null)
                        return;

                    bool canusearm = false;
                    for (int i = 0; i < Map.ARMModules.Count(); i++)
                    {
                        if (Map.ARMModules[i].WaferStatus == EnumSubsStatus.NOT_EXIST)
                        {
                            canusearm = true;
                            break;
                        }
                    }

                    if (canusearm == false)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Transfer Warning", $"There is already a wafer on the ARM.\n Please Check the ARM.", EnumMessageStyle.Affirmative);
                        return;
                    }


                    bool canusePA = false;
                    for (int i = 0; i < Map.PreAlignModules.Count(); i++)
                    {
                        if (Map.PreAlignModules[i].WaferStatus == EnumSubsStatus.NOT_EXIST)
                        {
                            canusePA = true;
                            break;
                        }
                    }

                    if (canusePA == false)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Transfer Warning", $"There is already a wafer on the PreAlign.\n Please Check the PreAlign.", EnumMessageStyle.Affirmative);
                        return;
                    }

                    string transferToTrayFailReason = "";
                    retval = this.TranferManager().ValidationTransferToTray(TransferSource, TransferTarget, ref transferToTrayFailReason);
                    if (retval != EventCodeEnum.NONE)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Transfer Error", $"{transferToTrayFailReason}", EnumMessageStyle.Affirmative);
                        return;
                    }

                    SetTransfer(sourceId, targetId);
                    var mapSlicer = new LoaderMapSlicer();
                    var slicedMap = mapSlicer.ManualSlicing(Map);

                    bool isError = false;

                    for (int i = 0; i < slicedMap.Count; i++)
                    {
                        LoaderModule.SetRequest(slicedMap[i]);

                        while (true)
                        {

                            if (LoaderModule.ModuleState == ModuleStateEnum.DONE)
                            {
                                LoaderModule.ClearRequestData();

                                LoaderMapConvert(LoaderModule.GetLoaderInfo().StateMap);

                                Thread.Sleep(100);

                                break;
                            }
                            else if (LoaderModule.ModuleState == ModuleStateEnum.ERROR)
                            {
                                isDestChuck = false;
                                LoaderRecoveryControlVM.Show(this.GetLoaderContainer(), LoaderModule.ResonOfError, LoaderModule.ErrorDetails);
                                LoaderModule.ResonOfError = "";
                                LoaderModule.ErrorDetails = "";
                                isError = true;
                                break;
                            }
                            Thread.Sleep(100);
                        }

                        if (isError)
                        {
                            break;
                        }

                        Thread.Sleep(1000);
                    }

                    Thread.Sleep(33);

                    UpdateTrayUI();
                    if (isDestChuck)
                    {
                        bool IsManualOp = false;

                        IStageSupervisorProxy stageproxy = null;

                        stageproxy = _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();

                        bool isMovingState = false;
                        isMovingState = stageproxy.IsMovingState();
                        if (isMovingState == false)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                Thread.Sleep(2000);
                                isMovingState = stageproxy.IsMovingState();
                                if (isMovingState)
                                {
                                    break;
                                }
                            }
                        }
                        if (isMovingState == true)
                        {
                            EnumSubsStatus waferstatus = stageproxy.GetWaferStatus();
                            EnumWaferType wafertype = stageproxy.GetWaferType();



                            LoggerManager.Debug($"TrasnferObjectFunc() : Wafer Status = {waferstatus}, Wafer Type = {wafertype}");

                            //retval = ValidatiaonManualCleaningParam();

                            if (retval == EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"TrasnferObjectFunc() : WaferDefineType = {ManualCleaningParam.WaferDefineType}");

                                if ((waferstatus == EnumSubsStatus.EXIST) &&
                                    (wafertype == EnumWaferType.POLISH))
                                {
                                    byte[] param = this.ObjectToByteArray(ManualCleaningParam);
                                    EventCodeEnum cleaningRetVal = EventCodeEnum.UNDEFINED;
                                    cleaningRetVal = await _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>()?.DoManualPolishWaferCleaningCommand(param);
                                    if (cleaningRetVal == EventCodeEnum.NONE)
                                    {
                                        EnumMessageDialogResult confirmWaferReturn = await this.MetroDialogManager().ShowMessageDialog("Confirm to wafer unloading", $"Do you want to unload the wafer?", EnumMessageStyle.AffirmativeAndNegative);
                                        if (confirmWaferReturn == EnumMessageDialogResult.AFFIRMATIVE)
                                        {
                                            //unload
                                            TransferSource = SelectedCell;
                                            var moduleType = SelectedCell.WaferObj.OriginHolder.ModuleType;
                                            int idx = SelectedCell.WaferObj.OriginHolder.Index;

                                            if (moduleType == ModuleTypeEnum.FIXEDTRAY ||
                                                moduleType == ModuleTypeEnum.INSPECTIONTRAY)
                                            {
                                                TransferTarget = GetAttachObject(moduleType, idx);
                                                isClickedTransferTarget = false;
                                                isClickedTransferSource = false;
                                            }
                                            isDestChuck = false;
                                            Map = LoaderModule.GetLoaderInfo().StateMap;

                                            sourceId = null;
                                            targetId = new ModuleID();
                                            transfermodule = null;

                                            CanUseSource = false;
                                            CanUseTarget = false;
                                            SourceReason = EventCodeEnum.UNDEFINED;
                                            TargetReason = EventCodeEnum.UNDEFINED;

                                            //Source Info
                                            CanUseSource = SetTransferInfoToModule(TransferSource, ref transfermodule, true, ref SourceReason);

                                            if (CanUseSource == true)
                                            {
                                                sourceId = (string)transfermodule;
                                            }

                                            //Target Info
                                            CanUseTarget = SetTransferInfoToModule(TransferTarget, ref transfermodule, false, ref TargetReason);

                                            if (CanUseTarget == true)
                                            {
                                                targetId = (ModuleID)transfermodule;
                                            }

                                            if (CanUseSource == true && CanUseTarget == true)
                                            {
                                                if (TransferTarget is StageObject)
                                                {
                                                    isDestChuck = true;
                                                }
                                                else
                                                {
                                                    isDestChuck = false;
                                                }

                                                if (isDestChuck == true)
                                                {
                                                    string validationStr = string.Empty;

                                                    retval = ValidatiaonManualCleaningParam(sourceId, ref validationStr);

                                                    // Before Pin Align Turn Off & Pin Align State is Idle => Can not operate cleaning manually.

                                                    if (_ManualCleaningParam.PinAlignBeforeCleaning.Value == false)
                                                    {
                                                        Element<AlignStateEnum> PinAlignState = _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>()?.GetAlignState(AlignTypeEnum.Pin);

                                                        if (PinAlignState == null || PinAlignState.Value != AlignStateEnum.DONE)
                                                        {
                                                            await this.MetroDialogManager().ShowMessageDialog("Information",
                                                            $"Pin alignment data is required for cleaning.\n" +
                                                            $"Turn on the parameter of pin align before cleaning or perform pin alignment before operating.", EnumMessageStyle.Affirmative);

                                                            return;
                                                        }
                                                    }

                                                    if (retval != EventCodeEnum.NONE)
                                                    {
                                                        await this.MetroDialogManager().ShowMessageDialog("Information", $"Validation failed.\n" + $"{validationStr}", EnumMessageStyle.Affirmative);

                                                        return;
                                                    }
                                                }

                                                if (sourceId == null | targetId == null)
                                                    return;

                                                canusearm = false;
                                                for (int i = 0; i < Map.ARMModules.Count(); i++)
                                                {
                                                    if (Map.ARMModules[i].WaferStatus == EnumSubsStatus.NOT_EXIST)
                                                    {
                                                        canusearm = true;
                                                        break;
                                                    }
                                                }

                                                if (canusearm == false)
                                                {
                                                    await this.MetroDialogManager().ShowMessageDialog("Transfer Warning", $"There is already a wafer on the ARM.\n Please Check the ARM.", EnumMessageStyle.Affirmative);
                                                    return;
                                                }


                                                canusePA = false;
                                                for (int i = 0; i < Map.PreAlignModules.Count(); i++)
                                                {
                                                    if (Map.PreAlignModules[i].WaferStatus == EnumSubsStatus.NOT_EXIST)
                                                    {
                                                        canusePA = true;
                                                        break;
                                                    }
                                                }

                                                if (canusePA == false)
                                                {
                                                    await this.MetroDialogManager().ShowMessageDialog("Transfer Warning", $"There is already a wafer on the PreAlign.\n Please Check the PreAlign.", EnumMessageStyle.Affirmative);
                                                    return;
                                                }

                                                transferToTrayFailReason = "";
                                                retval = this.TranferManager().ValidationTransferToTray(TransferSource, TransferTarget, ref transferToTrayFailReason);
                                                if (retval != EventCodeEnum.NONE)
                                                {
                                                    await this.MetroDialogManager().ShowMessageDialog("Transfer Error", $"{transferToTrayFailReason}", EnumMessageStyle.Affirmative);
                                                    return;
                                                }

                                                SetTransfer(sourceId, targetId);
                                                mapSlicer = new LoaderMapSlicer();
                                                slicedMap = mapSlicer.ManualSlicing(Map);

                                                isError = false;

                                                for (int i = 0; i < slicedMap.Count; i++)
                                                {
                                                    LoaderModule.SetRequest(slicedMap[i]);

                                                    while (true)
                                                    {

                                                        if (LoaderModule.ModuleState == ModuleStateEnum.DONE)
                                                        {
                                                            LoaderModule.ClearRequestData();

                                                            LoaderMapConvert(LoaderModule.GetLoaderInfo().StateMap);

                                                            Thread.Sleep(100);

                                                            break;
                                                        }
                                                        else if (LoaderModule.ModuleState == ModuleStateEnum.ERROR)
                                                        {
                                                            isDestChuck = false;
                                                            LoaderRecoveryControlVM.Show(this.GetLoaderContainer(), LoaderModule.ResonOfError, LoaderModule.ErrorDetails);
                                                            LoaderModule.ResonOfError = "";
                                                            LoaderModule.ErrorDetails = "";
                                                            isError = true;
                                                            break;
                                                        }
                                                        Thread.Sleep(100);
                                                    }

                                                    if (isError)
                                                    {
                                                        break;
                                                    }

                                                    Thread.Sleep(1000);
                                                }
                                                TransferSource = null;
                                                TransferTarget = SelectedCell;
                                                isClickedTransferSource = true;
                                                IsCleaning = false;
                                                UpdateTrayUI();
                                                Thread.Sleep(33);
                                            }
                                            else
                                            {
                                                //cell에 대기
                                                IsCleaning = true;
                                            }
                                        }
                                        else
                                        {
                                            //Cancel
                                            IsCleaning = true;
                                            TransferSource = true;
                                            TransferSource = SelectedCell;
                                            var moduleType = SelectedCell.WaferObj.OriginHolder.ModuleType;
                                            int idx = SelectedCell.WaferObj.OriginHolder.Index;

                                            if (moduleType == ModuleTypeEnum.FIXEDTRAY ||
                                                moduleType == ModuleTypeEnum.INSPECTIONTRAY)
                                            {
                                                TransferTarget = GetAttachObject(moduleType, idx);
                                                isClickedTransferTarget = false;
                                                isClickedTransferSource = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        IsCleaning = true;
                                        TransferSource = true;
                                        TransferSource = SelectedCell;

                                        var moduleType = SelectedCell.WaferObj.OriginHolder.ModuleType;
                                        int idx = SelectedCell.WaferObj.OriginHolder.Index;

                                        if (moduleType == ModuleTypeEnum.FIXEDTRAY ||
                                            moduleType == ModuleTypeEnum.INSPECTIONTRAY)
                                        {
                                            TransferTarget = GetAttachObject(moduleType, idx);
                                            isClickedTransferTarget = false;
                                            isClickedTransferSource = false;
                                        }
                                        await this.MetroDialogManager().ShowMessageDialog("Error Massage", $"Card Cleaning Fail. ErrorCode: {cleaningRetVal}", EnumMessageStyle.Affirmative);
                                    }
                                }
                                else
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("Information", $"Need a polish wafer on the chuck.\n Wafer status = {waferstatus} | Wafer type = {wafertype}", EnumMessageStyle.Affirmative);
                                }
                            }
                            else
                            {
                                await this.MetroDialogManager().ShowMessageDialog("Information", "Please check the cleaning parameters and card status.", EnumMessageStyle.Affirmative);
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"TrasnferObjectFunc() : IsMovingState  = {isMovingState}");
                        }
                    } 
                }
                else
                {
                    if (CanUseTarget == false)
                    {
                        // 이미 척에 웨이퍼가 있는 경우
                        if (TargetReason == EventCodeEnum.TRANSFER_ALREADY_EXIST_ON_CHUCK)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Information", "Already exist wafer on chuck.", EnumMessageStyle.Affirmative);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        private AsyncCommand _PWGetHeightCommand;
        public ICommand PWGetHeightCommand
        {
            get
            {

                if (null == _PWGetHeightCommand) _PWGetHeightCommand = new AsyncCommand(PWGetHeightCommandFunc);
                return _PWGetHeightCommand;
            }
        }
        private async Task PWGetHeightCommandFunc()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IsCleaning = true;

                bool IsManualOp = false;

                IStageSupervisorProxy stageproxy = null;

                stageproxy = _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();

                if ((stageproxy.GetWaferStatus() == EnumSubsStatus.EXIST) &&
                     (stageproxy.GetWaferType() == EnumWaferType.POLISH))
                {
                    if ((_ManualCleaningParam != null) && (_ManualCleaningParam.WaferDefineType.Value != string.Empty))
                    {
                        byte[] param = this.ObjectToByteArray(ManualCleaningParam);
                        await _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>()?.ManualPolishWaferFocusingCommand(param);
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Information", "Cleaning data is wrong.\n", EnumMessageStyle.Affirmative);
                    }
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Information", "Need a polish wafer on the chuck.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                IsCleaning = false;
                //_LoaderCommunicationManager.SetLoaderWorkingFlag(false);
            }

            
        }
        /// <summary>
        /// Holder 상태에 따라 UI를 업데이트 해주는 함수
        /// </summary>
        public void UpdateTrayUI()
        {
            foreach (var ft in FTs)
            {
                if (ft.WaferObj != null)
                {
                    if (ft.WaferObj.PolishWaferInfo != null && ft.WaferObj.PolishWaferInfo.DefineName != null)
                    {
                        if (ft.WaferObj.PolishWaferInfo.DefineName.Value == this.SelectedCleaningParam.WaferDefineType.Value)
                        {
                            ft.IsEnableTransfer = true;
                        }
                        else
                        {
                            ft.IsEnableTransfer = false;
                        }
                    }
                    else
                    {
                        ft.IsEnableTransfer = false;
                    }
                }
                else
                {
                    ft.IsEnableTransfer = false;
                }
            }
            foreach (var it in ITs)
            {
                if (it.WaferObj != null)
                {
                    if (it.WaferObj.PolishWaferInfo != null && it.WaferObj.PolishWaferInfo.DefineName != null)
                    {
                        if (it.WaferObj.PolishWaferInfo.DefineName.Value == this.SelectedCleaningParam.WaferDefineType.Value)
                        {
                            it.IsEnableTransfer = true;
                        }
                        else
                        {
                            it.IsEnableTransfer = false;
                        }
                    }
                    else
                    {
                        it.IsEnableTransfer = false;
                    }
                }
                else
                {
                    it.IsEnableTransfer = false;
                }
            }
        }
    }
}
