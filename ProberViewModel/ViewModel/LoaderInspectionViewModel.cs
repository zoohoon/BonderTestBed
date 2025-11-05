using System;
using System.Linq;
using System.Threading.Tasks;

namespace LoaderInspectionViewModelModule
{
    using Autofac;
    using CameraModule;
    using LoaderBase.Communication;
    using LoaderBase.FactoryModules.ViewModelModule;
    using LoaderMapView;
    using LogModule;
    using MetroDialogInterfaces;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.LightJog;
    using ProberInterfaces.Param;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.Utility;
    using ProberViewModel;
    using ProbingModule;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using VirtualKeyboardControl;

    public class LoaderInspectionViewModel : IMainScreenViewModel, IInspectionControlVM, IUseLightJog, ILoaderFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //.. Property
        public InitPriorityEnum InitPriority { get; }
        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();
        public IStageSupervisor StageSupervisor => this.StageSupervisor();

        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        public StageObject SelectedStageObj
        {
            get { return (StageObject)_LoaderCommunicationManager.SelectedStage; }
            set
            {
                if (value != (_LoaderCommunicationManager.SelectedStage))
                {
                    _LoaderCommunicationManager.SelectedStage = (StageObject)value;
                    RaisePropertyChanged();
                }
            }
        }
        public IWaferObject Wafer => this.GetParam_Wafer();

        private IDisplayPort _DisplayPort;

        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set { _DisplayPort = value; }
        }

        private ObservableDictionary<double, CatCoordinates> _ProbeTemperaturePositionTable
            = new ObservableDictionary<double, CatCoordinates>();
        public ObservableDictionary<double, CatCoordinates> ProbeTemperaturePositionTable
        {
            get { return _ProbeTemperaturePositionTable; }
            set
            {
                if (value != _ProbeTemperaturePositionTable)
                {
                    _ProbeTemperaturePositionTable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _TempOffset;
        public double TempOffset
        {
            get { return _TempOffset; }
            set
            {
                if (value != _TempOffset)
                {
                    _TempOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _AddTempOffset;
        public ICommand AddTempOffset
        {
            get
            {
                if (null == _AddTempOffset) _AddTempOffset = new AsyncCommand(AddTempOffsetFunc);
                return _AddTempOffset;
            }
        }

        private async Task AddTempOffsetFunc()
        {
            try
            {
                if (!ProbeTemperaturePositionTable.ContainsKey(TempOffset))
                {
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ProbeTemperaturePositionTable.Add(TempOffset, new CatCoordinates(0, 0, 0, 0));
                        SaveTempOffsetFunc();
                    }));
                }
                else
                {
                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", "Duplicate temperature offset value exists.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _DeleteTempOffset;
        public ICommand DeleteTempOffset
        {
            get
            {
                if (null == _DeleteTempOffset) _DeleteTempOffset = new AsyncCommand(DeleteTempOffsetFunc);
                return _DeleteTempOffset;
            }
        }

        private async Task DeleteTempOffsetFunc()
        {
            try
            {
                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", "Are you sure you want to delete it?", EnumMessageStyle.AffirmativeAndNegative);

                if (result == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (ProbeTemperaturePositionTable.ContainsKey(TempOffset))
                    {
                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ProbeTemperaturePositionTable.Remove(TempOffset);
                            SaveTempOffsetFunc();
                        }));
                    }
                    else
                    {
                        EnumMessageDialogResult result2 = await this.MetroDialogManager().ShowMessageDialog("Message", "Target temp does not exist. No data to delete", EnumMessageStyle.Affirmative);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ICamera _CurCam;
        public ICamera CurCam
        {
            get { return (this.ViewModelManager() as ILoaderViewModelManager).Camera; }
            set
            {
                _CurCam = value;
                RaisePropertyChanged();
            }
        }

        private ICoordinateManager _CoordManager;
        public ICoordinateManager CoordManager
        {
            get { return _CoordManager; }
            set
            {
                if (value != _CoordManager)
                {
                    _CoordManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IInspection _Inspection;
        public IInspection Inspection
        {
            get { return _Inspection; }
            set
            {
                if (value != _Inspection)
                {
                    _Inspection = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IPnpManager PnpManager
        {
            get { return this.PnPManager(); }
        }

        private IZoomObject _ZoomObject;

        public IZoomObject ZoomObject
        {
            get { return _ZoomObject; }
            set
            {
                if (value != _ZoomObject)
                {
                    _ZoomObject = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _SetFromToggle;
        public bool SetFromToggle
        {
            get
            {
                return _SetFromToggle;
            }
            set
            {
                _SetFromToggle = value;
                RaisePropertyChanged();
            }
        }
        private object _MainViewTarget;
        public object MainViewTarget
        {
            get { return _MainViewTarget; }
            set
            {
                if (value != _MainViewTarget)
                {
                    _MainViewTarget = value;
                    if (_MainViewTarget == ZoomObject)
                        MainViewZoomVisible = Visibility.Visible;
                    else
                        MainViewZoomVisible = Visibility.Hidden;

                    RaisePropertyChanged();
                }
            }
        }
        private object _MiniViewTarget;
        public object MiniViewTarget
        {
            get { return _MiniViewTarget; }
            set
            {
                if (value != _MiniViewTarget)
                {
                    _MiniViewTarget = value;
                    if (_MiniViewTarget == ZoomObject)
                        MiniViewZoomVisible = Visibility.Visible;
                    else
                        MiniViewZoomVisible = Visibility.Hidden;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _MainViewZoomVisible;
        public Visibility MainViewZoomVisible
        {
            get { return _MainViewZoomVisible; }
            set
            {
                if (value != _MainViewZoomVisible)
                {
                    _MainViewZoomVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _MiniViewZoomVisible;
        public Visibility MiniViewZoomVisible
        {
            get { return _MiniViewZoomVisible; }
            set
            {
                if (value != _MiniViewZoomVisible)
                {
                    _MiniViewZoomVisible = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _UserXShiftValue;
        public double UserXShiftValue
        {
            get
            {
                return _UserXShiftValue;
            }
            set
            {
                if (value != _UserXShiftValue && value != 0)
                {
                    _UserXShiftValue = value;
                    RaisePropertyChanged();
                }
                else if (value == 0)
                {
                    _UserXShiftValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _UserYShiftValue;
        public double UserYShiftValue
        {
            get
            {
                return _UserYShiftValue;
            }
            set
            {
                if (value != _UserYShiftValue && value != 0)
                {
                    _UserYShiftValue = value;
                    RaisePropertyChanged();
                }
                else if (value == 0)
                {
                    _UserYShiftValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _SystemXShiftValue;
        public double SystemXShiftValue
        {
            get
            {
                return _SystemXShiftValue;
            }
            set
            {
                if (value != _SystemXShiftValue && value != 0)
                {
                    _SystemXShiftValue = value;
                    RaisePropertyChanged();
                }
                else if (value == 0)
                {
                    _SystemXShiftValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SystemYShiftValue;
        public double SystemYShiftValue
        {
            get
            {
                return _SystemYShiftValue;
            }
            set
            {
                if (value != _SystemYShiftValue && value != 0)
                {
                    _SystemYShiftValue = value;
                    RaisePropertyChanged();
                }
                else if (value == 0)
                {
                    _SystemYShiftValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ToggleSetIndex = true;
        public bool ToggleSetIndex
        {
            get { return _ToggleSetIndex; }
            set
            {
                if (value != _ToggleSetIndex)
                {
                    _ToggleSetIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _MapIndexX;
        public long MapIndexX
        {
            get { return _MapIndexX; }
            set
            {
                if (value != _MapIndexX)
                {
                    _MapIndexX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _MapIndexY;
        public long MapIndexY
        {
            get { return _MapIndexY; }
            set
            {
                if (value != _MapIndexY)
                {
                    _MapIndexY = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _AxisX;
        public ProbeAxisObject AxisX
        {
            get { return _AxisX; }
            set
            {
                if (value != _AxisX)
                {
                    _AxisX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbeAxisObject _AxisY;
        public ProbeAxisObject AxisY
        {
            get { return _AxisY; }
            set
            {
                if (value != _AxisY)
                {
                    _AxisY = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _XSetFromCoord;
        public double XSetFromCoord
        {
            get
            {
                return _XSetFromCoord;
            }
            set
            {
                if (value != _XSetFromCoord && value != 0)
                {
                    _XSetFromCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _YSetFromCoord;
        public double YSetFromCoord
        {
            get
            {
                return _YSetFromCoord;
            }
            set
            {
                if (value != _YSetFromCoord && value != 0)
                {
                    _YSetFromCoord = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region //..IMainScreenViewModel 

        readonly Guid _ViewModelGUID = new Guid("54372B75-F13D-615F-F614-4E317570F11D");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;
        public IProbingModule ProbingModule { get; set; }
        public LoaderInspectionViewModel()
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
                Inspection = this.InspectionModule();
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
                CurCam = new Camera();
                DisplayPort = (this.ViewModelManager() as ILoaderViewModelManager).DisplayPort;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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


        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbingModule = this.ProbingModule();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;

                    ((UcDisplayPort.DisplayPort)DisplayPort).AxisXPos = this.MotionManager().GetAxis(EnumAxisConstants.X);
                    ((UcDisplayPort.DisplayPort)DisplayPort).AxisYPos = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    ((UcDisplayPort.DisplayPort)DisplayPort).AxisZPos = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    ((UcDisplayPort.DisplayPort)DisplayPort).AxisTPos = this.MotionManager().GetAxis(EnumAxisConstants.C);
                    ((UcDisplayPort.DisplayPort)DisplayPort).AxisPZPos = this.MotionManager().GetAxis(EnumAxisConstants.PZ);

                    AxisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
                    AxisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);

                    Binding bindX = new Binding
                    {
                        Path = new System.Windows.PropertyPath("StageSupervisor.MoveTargetPosX"),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.MoveToX, bindX);

                    Binding bindY = new Binding
                    {
                        Path = new System.Windows.PropertyPath("StageSupervisor.MoveTargetPosY"),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.MoveToY, bindY);

                    Binding bindCamera = new Binding
                    {
                        Path = new System.Windows.PropertyPath("CurCam"),
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.AssignedCamearaProperty, bindCamera);
                });

                
                this.VisionManager().SetDisplayChannel(null, DisplayPort);
                //if (_RemoteMediumProxy == null)
                //{
                //    await this.MetroDialogManager().ShowMessageDialog("Error Message", "Please use page after connection to Stage.",
                //        ProberInterfaces.Enum.EnumMessageStyle.Affirmative);
                //    return EventCodeEnum.NONE;
                //}

                await _RemoteMediumProxy.InspectionVM_PageSwitched();

                //_RemoteMediumProxy.PageSwitched(new Guid("f8396e3a-b8ce-4dcd-9a0d-643532a7d9d1"), parameter);
                ZoomObject = this.GetParam_Wafer();


                //Task task = new Task(() =>
                //{
                await UpdateInspectionInfo();
                SetFromToggle = false;
                //});
                //task.Start();
                //await task;

                //await Task.Run(() =>
                //{
                //    UpdateInspectionInfo();
                //});

                Task task = new Task(() =>
                {
                    try
                    {
                        CurCam = (this.ViewModelManager() as ILoaderViewModelManager).Camera;
                        if (CurCam != null)
                            PnpManager.PnpLightJog.InitCameraJog(this, CurCam.GetChannelType());
                        PnpManager.PnpLightJog.InitCameraJog(this, _RemoteMediumProxy.GetCamType());
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }

                });
                task.Start();
                await task;

                Wafer.MapViewCurIndexVisiablity = true;
                MainViewTarget = DisplayPort;
                MiniViewTarget = Wafer;

                CoordManager = this.CoordinateManager();

                ProbeTemperaturePositionTable = new ObservableDictionary<double, CatCoordinates>(_RemoteMediumProxy.GetTemperaturePMShifhtTable());

                TempOffset = Convert.ToDouble(_LoaderCommunicationManager.SelectedStage.StageInfo.LotData.SetTemp);

                SelectedStage = _LoaderCommunicationManager.Cells[_LoaderCommunicationManager.SelectedStageIndex - 1];

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private async Task UpdateInspectionInfo()
        {
            try
            {
                var info = await _RemoteMediumProxy.GetInspectionInfo();

                if (info != null)
                {
                    SetFromToggle = info.SetFromToggle;

                    Inspection.ViewDutIndex = info.InspectionViewDutIndex;
                    Inspection.ViewPadIndex = info.InspectionViewPadIndex;

                    Inspection.DUTCount = info.InspcetionDutCount;
                    Inspection.PADCount = info.InspcetionPADCount;
                    Inspection.XDutStartIndexPoint = info.InspcetoinXDutStartIndexPoint;
                    Inspection.YDutStartIndexPoint = info.InspectionYDutStartIndexPoint;
                    Inspection.ManualSetIndexToggle = info.InspectionManualSetIndexToggle;

                    XSetFromCoord = info.XSetFromCoord;
                    YSetFromCoord = info.YSetFromCoord;

                    MapIndexX = info.MapIndexX;
                    MapIndexY = info.MapIndexY;
                    UserXShiftValue = info.UserXShiftValue;
                    UserYShiftValue = info.UserYShiftValue;
                    SystemXShiftValue = info.SystemXShiftValue;
                    SystemYShiftValue = info.SystemYShiftValue;

                    ToggleSetIndex = info.ToggleSetIndex;

                    (this.ViewModelManager() as ILoaderViewModelManager).WaitUIUpdate("UpdateInspectionInfo",10);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task SetInspectionInfo()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var info = await _RemoteMediumProxy.GetInspectionInfo();

                    if (info != null)
                    {
                        info.UserXShiftValue = UserXShiftValue;
                        info.UserYShiftValue = UserYShiftValue;
                        info.SystemXShiftValue = SystemXShiftValue;
                        info.SystemYShiftValue = SystemYShiftValue;

                        if (_RemoteMediumProxy != null)
                        {
                            await _RemoteMediumProxy.Inspection_SaveCommand(info);
                        }

                        (this.ViewModelManager() as ILoaderViewModelManager).WaitUIUpdate("SaveInspectionInfo", 10);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task<EventCodeEnum> PMShiftLimit(double checkvalue)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if(_RemoteMediumProxy != null)
                {
                    ret = await _RemoteMediumProxy.Inspection_CheckPMShiftLimit(checkvalue);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }
        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CatCoordinates pmTempshift = new CatCoordinates();

                if (_RemoteMediumProxy != null)
                {
                    retVal = _RemoteMediumProxy.UpdateSysparam();
                    pmTempshift =  _RemoteMediumProxy.GetSetTemperaturePMShifhtValue();
                    LoggerManager.Debug($"ProbeTemperaturePositionTable x : {pmTempshift.GetX()}");
                    SetFromToggle = false;
                    await _RemoteMediumProxy.InspectionVM_Cleanup();
                }

                Wafer.MapViewCurIndexVisiablity = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        #endregion

        #region //.. Command & Method
        private RelayCommand _PlusCommand;
        public ICommand PlusCommand
        {
            get
            {
                if (null == _PlusCommand) _PlusCommand = new RelayCommand(PlusCommandFunc);
                return _PlusCommand;
            }
        }
        private void PlusCommandFunc()
        {
            try
            {
                ZoomObject.ZoomLevel--;

                //if (_RemoteMediumProxy != null)
                //{
                //    _RemoteMediumProxy.Inspection_PlusCommand();
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _MinusCommand;
        public ICommand MinusCommand
        {
            get
            {
                if (null == _MinusCommand) _MinusCommand = new RelayCommand(Minus);
                return _MinusCommand;
            }
        }
        private void Minus()
        {
            string Minus = string.Empty;

            try
            {
                ZoomObject.ZoomLevel++;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ViewSwapCommand;
        public ICommand ViewSwapCommand
        {
            get
            {
                if (null == _ViewSwapCommand) _ViewSwapCommand = new RelayCommand<object>(ViewSwapFunc);
                return _ViewSwapCommand;
            }
        }
        private void ViewSwapFunc(object parameter)
        {
            try
            {

                object swap = MainViewTarget;
                MainViewTarget = MiniViewTarget;
                MiniViewTarget = swap;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SaveTempOffset;
        public IAsyncCommand SaveTempOffset
        {
            get
            {
                if (null == _SaveTempOffset) _SaveTempOffset = new AsyncCommand(SaveTempOffsetFunc);
                return _SaveTempOffset;
            }
        }
        private async Task SaveTempOffsetFunc()
        {
            try
            {
                var sortedlist = ProbeTemperaturePositionTable.OrderBy(x => x.Key).ToList();

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ProbeTemperaturePositionTable.Clear();

                    foreach (var item in sortedlist)
                    {
                        ProbeTemperaturePositionTable.Add(item.Key, item.Value);
                    }
                }));

                await _RemoteMediumProxy.Inspection_SaveTempOffset(ProbeTemperaturePositionTable);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _SetFromCommand;
        public IAsyncCommand SetFromCommand
        {
            get
            {
                if (null == _SetFromCommand) _SetFromCommand = new AsyncCommand(FuncSetFromCommand);
                return _SetFromCommand;
            }
        }
        private async Task FuncSetFromCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.Inspection_SetFromCommand();

                    await UpdateInspectionInfo();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _TempPMShiftClickCommand;
        public ICommand TempPMShiftClickCommand
        {
            get
            {
                if (null == _TempPMShiftClickCommand) _TempPMShiftClickCommand = new RelayCommand<Object>(TempPMShiftClickCommandFunc);
                return _TempPMShiftClickCommand;
            }
        }


        private void TempPMShiftClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 10);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //throw;
            }
        }
        private AsyncCommand<Object> _ShiftTextBoxClickCommand;
        public ICommand ShiftTextBoxClickCommand
        {
            get
            {
                if (null == _ShiftTextBoxClickCommand) _ShiftTextBoxClickCommand = new AsyncCommand<Object>(ShiftTextBoxClickCommandFunc);
                return _ShiftTextBoxClickCommand;
            }
        }

        private async Task ShiftTextBoxClickCommandFunc(object param)
        {
            try
            {
                EventCodeEnum ret = EventCodeEnum.UNDEFINED;
                System.Windows.Controls.TextBox tb = null;
                EnumAxisConstants axis = EnumAxisConstants.Undefined;
                (System.Windows.Controls.TextBox, double) paramVal = (null, 0.0);
                string backup = "";
                double checkvalue = -1;
                string userPrefix = "User";
                string systemPrefix = "System";
                string xSuffix = "X";
                string ySuffix = "Y";

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    paramVal = ((System.Windows.Controls.TextBox, double))param;
                    tb = paramVal.Item1;

                    string tbName = tb.Name;
                    string userPart = NameExtraction(tbName, userPrefix);
                    string systemPart = NameExtraction(tbName, systemPrefix);
                    string axisPart = AxisExtraction(tbName, xSuffix, ySuffix);

                    if (axisPart == xSuffix)
                    {
                        axis = EnumAxisConstants.X;
                    }
                    else if (axisPart == ySuffix)
                    {
                        axis = EnumAxisConstants.Y;
                    }
                    backup = tb.Text;
                    tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.FLOAT | KB_TYPE.DECIMAL);
                    if (!double.TryParse(tb.Text, out checkvalue))
                    {
                        checkvalue = -1;
                    }

                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                }));

                if (checkvalue != -1 && axis != EnumAxisConstants.Undefined)
                {
                    double userValue = GetUserValue(axis);
                    double systemValue = GetSystemValue(axis);
                    double totalValue = userValue + systemValue;

                    ret = await PMShiftLimit(totalValue);

                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ret != EventCodeEnum.NONE) // 유효하지 않는 경우 원래 값으로 돌린다.
                        {
                            tb.Text = backup;
                            tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                        }   
                    }));
                }
                if(ret == EventCodeEnum.NONE)
                {
                    paramVal.Item2 = checkvalue;
                    await SetInspectionInfo();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private string NameExtraction(string tbName, string prefix)
        {
            return tbName.StartsWith(prefix) ? prefix : string.Empty;
        }

        private string AxisExtraction(string tbName, string xSuffix, string ySuffix)
        {
            if (tbName.Contains(xSuffix))
            {
                return xSuffix;
            }

            if (tbName.Contains(ySuffix))
            {
                return ySuffix;
            }

            return string.Empty;
        }

        private double GetUserValue(EnumAxisConstants axis)
        {
            return axis == EnumAxisConstants.X ? UserXShiftValue : UserYShiftValue;
        }

        private double GetSystemValue(EnumAxisConstants axis)
        {
            return axis == EnumAxisConstants.X ? SystemXShiftValue : SystemYShiftValue;
        }

        private AsyncCommand _ApplyCommand;
        public IAsyncCommand ApplyCommand
        {
            get
            {
                if (null == _ApplyCommand) _ApplyCommand = new AsyncCommand(FuncApplyCommand);
                return _ApplyCommand;
            }
        }
        private async Task FuncApplyCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.Inspection_ApplyCommand();
                    
                    await UpdateInspectionInfo();

                    //await Task.Run(() =>
                    //{
                    //    UpdateInspectionInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SystemApplyCommand;
        public IAsyncCommand SystemApplyCommand
        {
            get
            {
                if (null == _SystemApplyCommand) _SystemApplyCommand = new AsyncCommand(SystemApplyFunc);
                return _SystemApplyCommand;
            }
        }
        private async Task SystemApplyFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.Inspection_SystemApplyCommand();

                    await UpdateInspectionInfo();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _ClearCommand;
        public IAsyncCommand ClearCommand
        {
            get
            {
                if (null == _ClearCommand) _ClearCommand = new AsyncCommand(FuncClearCommand);
                return _ClearCommand;
            }
        }
        private async Task FuncClearCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.Inspection_ClearCommand();

                    await UpdateInspectionInfo();

                    //await Task.Run(() =>
                    //{
                    //    UpdateInspectionInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _SystemClearCommand;
        public IAsyncCommand SystemClearCommand
        {
            get
            {
                if (null == _SystemClearCommand) _SystemClearCommand = new AsyncCommand(SystemClearFunc);
                return _SystemClearCommand;
            }
        }
        private async Task SystemClearFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.Inspection_SystemClearCommand();

                    await UpdateInspectionInfo();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _PrevDutCommand;
        public IAsyncCommand PrevDutCommand
        {
            get
            {
                if (null == _PrevDutCommand) _PrevDutCommand = new AsyncCommand(FuncPrevDutCommand);
                return _PrevDutCommand;
            }
        }
        private async Task FuncPrevDutCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.Inspection_PrevDutCommand();

                    await UpdateInspectionInfo();

                    //await Task.Run(() =>
                    //{
                    //    UpdateInspectionInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _NextDutCommand;
        public IAsyncCommand NextDutCommand
        {
            get
            {
                if (null == _NextDutCommand) _NextDutCommand = new AsyncCommand(FuncNextDutCommand);
                return _NextDutCommand;
            }
        }
        private async Task FuncNextDutCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.Inspection_NextDutCommand();

                    await UpdateInspectionInfo();
                    //await Task.Run(() =>
                    //{
                    //    UpdateInspectionInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _PadPrevCommand;
        public IAsyncCommand PadPrevCommand
        {
            get
            {
                if (null == _PadPrevCommand) _PadPrevCommand = new AsyncCommand(FuncPadPrevCommand);
                return _PadPrevCommand;
            }
        }

        private async Task FuncPadPrevCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.Inspection_PadPrevCommand();

                    await UpdateInspectionInfo();

                    //await Task.Run(() =>
                    //{
                    //    UpdateInspectionInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _PadNextCommand;
        public IAsyncCommand PadNextCommand
        {
            get
            {
                if (null == _PadNextCommand) _PadNextCommand = new AsyncCommand(FuncPadNextCommand);
                return _PadNextCommand;
            }
        }
        private async Task FuncPadNextCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.Inspection_PadNextCommand();

                    await UpdateInspectionInfo();

                    //await Task.Run(() =>
                    //{
                    //    UpdateInspectionInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ManualSetIndexCommand;
        public IAsyncCommand ManualSetIndexCommand
        {
            get
            {
                if (null == _ManualSetIndexCommand) _ManualSetIndexCommand = new AsyncCommand(FuncManualSetIndexCommand);
                return _ManualSetIndexCommand;
            }
        }



        private async Task FuncManualSetIndexCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.Inspection_PadNextCommand();

                    await UpdateInspectionInfo();

                    //await Task.Run(() =>
                    //{
                    //    UpdateInspectionInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _ChangeXManualCommand;
        public ICommand ChangeXManualCommand
        {
            get
            {
                if (null == _ChangeXManualCommand) _ChangeXManualCommand = new RelayCommand<Object>(ChangeXManualCommandFunc);
                return _ChangeXManualCommand;
            }
        }

        private void ChangeXManualCommandFunc(Object param)
        {
            try
            {
                int dieXlength = this.GetParam_Wafer().GetSubsInfo().DIEs.GetLength(0);
                double xIndex = 0;
                string xValue = string.Empty;
                bool isSuccessToDouble = false;

                xValue = MapIndexX.ToString();
                xValue = VirtualKeyboard.Show(xValue, KB_TYPE.DECIMAL, 0, 100);
                isSuccessToDouble = double.TryParse(xValue, out xIndex);

                if (isSuccessToDouble)
                {
                    if (xIndex < 0)
                    {
                        MapIndexX = 0;
                        xValue = "0";
                    }
                    else if (xIndex > dieXlength)
                    {
                        MapIndexX = 0;
                        xValue = "0";
                    }
                    else
                    {
                        MapIndexX = (long)xIndex;
                        //Inspection.ManualSetIndexX = (int)MapIndexX;
                    }
                }

                // TODO : 변경된 값을 스테이지로 전달할 것.

                _RemoteMediumProxy.Inspection_ChangeXManualIndex(MapIndexX);
                UpdateInspectionInfo();

                //if (_RemoteMediumProxy != null)
                //{
                //    _RemoteMediumProxy.Inspection_ChangeXManualCommand();
                //    UpdateInspectionInfo();
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _ChangeYManualCommand;
        public ICommand ChangeYManualCommand
        {
            get
            {
                if (null == _ChangeYManualCommand) _ChangeYManualCommand = new RelayCommand<Object>(ChangeYManualCommandFunc);
                return _ChangeYManualCommand;
            }
        }

        private void ChangeYManualCommandFunc(Object param)
        {
            try
            {
                int dieYlength = this.GetParam_Wafer().GetSubsInfo().DIEs.GetLength(1);
                double yIndex = 0;
                string yValue = string.Empty;
                bool isSuccessToDouble = false;

                yValue = MapIndexY.ToString();
                yValue = VirtualKeyboard.Show(yValue, KB_TYPE.DECIMAL, 0, 100);
                isSuccessToDouble = double.TryParse(yValue, out yIndex);

                if (isSuccessToDouble)
                {
                    if (yIndex < 0)
                    {
                        MapIndexY = 0;
                        yValue = "0";
                    }
                    else if (yIndex > dieYlength)
                    {
                        MapIndexY = 0;
                        yValue = "0";
                    }
                    else
                    {
                        MapIndexY = (long)yIndex;
                        //Inspection.ManualSetIndexY = (int)MapIndexY;
                    }
                }

                // TODO : 변경된 값을 스테이지로 전달할 것.

                _RemoteMediumProxy.Inspection_ChangeYManualIndex(MapIndexY);
                UpdateInspectionInfo();

                //if (_RemoteMediumProxy != null)
                //{
                //    _RemoteMediumProxy.Inspection_ChangeYManualCommand();
                //    UpdateInspectionInfo();
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _SavePadsCommand;
        public IAsyncCommand SavePadsCommand
        {
            get
            {
                if (null == _SavePadsCommand) _SavePadsCommand = new AsyncCommand(SavePadsCommandFunc);
                return _SavePadsCommand;
            }
        }
        private async Task SavePadsCommandFunc() // 
        {
            try
            {                
                await _RemoteMediumProxy.Inspection_SavePads();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }
        private AsyncCommand _PinAlignCommand;
        public IAsyncCommand PinAlignCommand
        {
            get
            {
                if (null == _PinAlignCommand) _PinAlignCommand = new AsyncCommand(PinAlignCommandFunc);
                return _PinAlignCommand;
            }
        }
        private async Task PinAlignCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.Inspection_PinAlignCommand();

                    await UpdateInspectionInfo();

                    //await Task.Run(() =>
                    //{
                    //    UpdateInspectionInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _WaferAlignCommand;
        public IAsyncCommand WaferAlignCommand
        {
            get
            {
                if (null == _WaferAlignCommand) _WaferAlignCommand = new AsyncCommand(WaferAlignCommandFunc);
                return _WaferAlignCommand;
            }
        }
        private async Task WaferAlignCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.Inspection_WaferAlignCommand();
                    
                    await UpdateInspectionInfo();

                    //await Task.Run(() =>
                    //{
                    //    UpdateInspectionInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public InspcetionDataDescription GetInscpetionInfo()
        {
            return null;
        }

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            return EventCodeEnum.NONE;
        }

        public void Inspection_ChangeXManualIndex(long index)
        {
            throw new NotImplementedException();
        }

        public void Inspection_ChangeYManualIndex(long index)
        {
            throw new NotImplementedException();
        }

        public Task SetFromRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task ApplyRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task ClearRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task PrevDutRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task NextDutRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task PadPrevRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task PadNextRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task ManualSetIndexRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task PinAlignRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task WaferAlignRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task SavePadsRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task SaveTempOffsetRemoteCommand(ObservableDictionary<double, CatCoordinates> table)
        {
            throw new NotImplementedException();
        }

        public Task SaveRemoteCommand(InspcetionDataDescription info)
        {
            throw new NotImplementedException();
        }

        public Task SystemApplyRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task SystemClearRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task<EventCodeEnum> CheckPMShiftLimit(double checkvalue)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
