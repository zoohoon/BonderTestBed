using System;
using System.Threading.Tasks;
using ProberInterfaces;
using LogModule;
using RelayCommandBase;
using ProberErrorCode;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Windows.Input;
using UcDisplayPort;
using LoaderBase;
using LoaderBase.FactoryModules.ViewModelModule;
using Autofac;
using ProberInterfaces.Enum;
using System.Collections.ObjectModel;
using ProberInterfaces.ControlClass.ViewModel;
using ProberInterfaces.PnpSetup;
using LoaderBase.Communication;
using VirtualKeyboardControl;
using ProberInterfaces.Loader.RemoteDataDescription;
using System.Windows;
using System.Windows.Media;
using Pranas;
using System.IO;

namespace LoaderChuckPlanarityViewModel
{
    public class LoaderChuckPlanarityVM : IChuckPlanarityVM
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("1754d2b9-4dca-41c3-b8fa-0feaf7c01f3b");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;

        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();
        public IStageSupervisor StageSupervisor => this.StageSupervisor();

        private IDisplayPort _DisplayPort;
        public IDisplayPort DisplayPort
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

        private ICamera _CurCam;
        public ICamera CurCam
        {
            get { return _CurCam; }
            set
            {
                if (value != _CurCam)
                {
                    _CurCam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<BaseThing> _Items;
        public ObservableCollection<BaseThing> Items
        {
            get { return _Items; }
            set
            {
                if (value != _Items)
                {
                    _Items = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<BaseThing> _ItemBuf;
        public ObservableCollection<BaseThing> ItemBuf
        {
            get { return _ItemBuf; }
            set
            {
                if (value != _ItemBuf)
                {
                    _ItemBuf = value;
                    RaisePropertyChanged();
                }
            }
        }
        public IPnpManager PnpManager
        {
            get { return this.PnPManager(); }
            set { }
        }

        private ObservableCollection<ChuckPos> _ChuckPosList;
        public ObservableCollection<ChuckPos> ChuckPosList
        {
            get { return _ChuckPosList; }
            set
            {
                if (value != _ChuckPosList)
                {
                    _ChuckPosList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TextBoxVM _TmpTextboxVM = null;
        public TextBoxVM TmpTextboxVM
        {
            get { return _TmpTextboxVM; }
            set
            {
                if (value != _TmpTextboxVM)
                {
                    _TmpTextboxVM = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private EnumChuckPosition _CurrentChuckPosEnum;
        //public EnumChuckPosition CurrentChuckPosEnum
        //{
        //    get { return _CurrentChuckPosEnum; }
        //    set
        //    {
        //        if (value != _CurrentChuckPosEnum)
        //        {
        //            _CurrentChuckPosEnum = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private ILoaderSupervisor _LoaderMaster;
        public ILoaderSupervisor LoaderMaster
        {
            get { return _LoaderMaster; }
            set
            {
                if (value != _LoaderMaster)
                {

                    _LoaderMaster = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MinHeight;
        public double MinHeight
        {
            get { return _MinHeight; }
            set
            {
                if (value != _MinHeight)
                {
                    _MinHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MaxHeight;
        public double MaxHeight
        {
            get { return _MaxHeight; }
            set
            {
                if (value != _MaxHeight)
                {
                    _MaxHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DiffHeight;
        public double DiffHeight
        {
            get { return _DiffHeight; }
            set
            {
                if (value != _DiffHeight)
                {
                    _DiffHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _FocusingRange;
        public double FocusingRange
        {
            get { return _FocusingRange; }
            set
            {
                if (value != _FocusingRange)
                {
                    _FocusingRange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AvgHeight;
        public double AvgHeight
        {
            get { return _AvgHeight; }
            set
            {
                if (value != _AvgHeight)
                {
                    _AvgHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ChuckEndPointMargin;
        public double ChuckEndPointMargin
        {
            get { return _ChuckEndPointMargin; }
            set
            {
                if (value != _ChuckEndPointMargin)
                {
                    _ChuckEndPointMargin = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private double _SpecHeight;
        //public double SpecHeight
        //{
        //    get { return _SpecHeight; }
        //    set
        //    {
        //        if (value != _SpecHeight)
        //        {
        //            _SpecHeight = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

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

        private ILoaderViewModelManager LoaderViewModelManager => _Container.Resolve<ILoaderViewModelManager>();

        private Autofac.IContainer _Container => this.GetLoaderContainer();

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

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public EventCodeEnum InitModule()
        {
            try
            {
                DisplayPort = new DisplayPort();
                //DisplayPort.GridVisibility = false;
                LoaderMaster = _Container.Resolve<ILoaderSupervisor>();
                //LoaderViewModelManager.RegisteDisplayPort(DisplayPort);

                MainViewTarget = DisplayPort;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (_RemoteMediumProxy != null)
                {
                    //AsyncHelpers.RunSync(() => _RemoteMediumProxy.ChuckPlanarity_PageSwitched());😚
                    
                    await _RemoteMediumProxy.ChuckPlanarity_PageSwitched();
                }

                Task task = new Task(() =>
                {
                    UpdateDeviceChangeInfo();
                    MakePositionData();
                    this.VisionManager().SetDisplayChannel(null, DisplayPort);
                    CurCam = (this.ViewModelManager() as ILoaderViewModelManager).Camera;
                    if (CurCam != null)
                        PnpManager.PnpLightJog.InitCameraJog(this, CurCam.GetChannelType());
                });
                task.Start();
                await task;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //Task.Run(() =>
            //{
            //    UpdateDeviceChangeInfo();
            //    MakePositionData();
            //});

            return retval;
        }
        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.ChuckPlanarity_Cleanup();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        //private AsyncGenericCommand<EnumChuckPosition> _ChuckMoveCommand;
        //public IAsyncCommand<EnumChuckPosition> ChuckMoveCommand
        //{
        //    get
        //    {
        //        if (null == _ChuckMoveCommand) _ChuckMoveCommand = new AsyncGenericCommand<EnumChuckPosition>(ChuckMoveCommandFunc);
        //        return _ChuckMoveCommand;
        //    }
        //}

        private AsyncCommand<object> _ChuckMoveCommand;
        public IAsyncCommand ChuckMoveCommand
        {
            get
            {
                if (null == _ChuckMoveCommand) _ChuckMoveCommand = new AsyncCommand<object>(ChuckMoveCommandFunc);
                return _ChuckMoveCommand;
            }
        }

        private async Task ChuckMoveCommandFunc(object param)
        {
            try
            {
                await _RemoteMediumProxy.ChuckMoveCommand(param as ChuckPos);

                Task task = new Task(() =>
                {
                    UpdateDeviceChangeInfo();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //
            }
        }

        //private AsyncCommand<EnumChuckPosition> _ChuckMoveCommand;
        //public IAsyncCommand ChuckMoveCommand
        //{
        //    get
        //    {
        //        if (null == _ChuckMoveCommand) _ChuckMoveCommand = new AsyncCommand<EnumChuckPosition>(ChuckMoveCommandFunc);
        //        return _ChuckMoveCommand;
        //    }
        //}

        //private RelayCommand<object> _ChuckMoveCommand;
        //public ICommand ChuckMoveCommand
        //{
        //    get
        //    {
        //        if (null == _ChuckMoveCommand) _ChuckMoveCommand = new RelayCommand<object>(ChuckMoveCommandFunc);
        //        return _ChuckMoveCommand;
        //    }
        //}

        //private AsyncGenericCommand<EnumChuckPosition> _ChuckMoveCommand;
        //public IAsyncCommand<EnumChuckPosition> ChuckMoveCommand
        //{
        //    get
        //    {
        //        if (null == _ChuckMoveCommand) _ChuckMoveCommand = new AsyncGenericCommand<EnumChuckPosition>(ChuckMoveCommandFunc);
        //        return _ChuckMoveCommand;
        //    }
        //}

        private async void ChuckMoveCommandFunc(EnumChuckPosition param)
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.ChuckPlanarity_ChuckMoveCommand(param);
                }

                //Task.Run(() =>
                //{
                //    UpdateDeviceChangeInfo();
                //});
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }

        private AsyncCommand _MeasureOnePositionCommand;
        public IAsyncCommand MeasureOnePositionCommand
        {
            get
            {
                if (null == _MeasureOnePositionCommand) _MeasureOnePositionCommand = new AsyncCommand(MeasureOnePositionCommandFunc);
                return _MeasureOnePositionCommand;
            }
        }

        //private async Task<EventCodeEnum> MeasureOnePositionCommandFunc()
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        if (_RemoteMediumProxy != null)
        //        {
        //            await _RemoteMediumProxy.ChuckPlanarity_MeasureOnePositionCommand();
        //        }

        //        Task.Run(() =>
        //        {
        //            UpdateDeviceChangeInfo();
        //        });

        //        retval = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //    }

        //    return retval;
        //}

        private async Task<EventCodeEnum> MeasureOnePositionCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.ChuckPlanarity_MeasureOnePositionCommand();
                }

                Task task = new Task(() =>
                {
                    UpdateDeviceChangeInfo();
                });
                task.Start();
                await task;

                //Task.Run(() =>
                //{
                //    UpdateDeviceChangeInfo();
                //});

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }

            return retval;
        }

        private AsyncCommand _MeasureAllPositionCommand;
        public IAsyncCommand MeasureAllPositionCommand
        {
            get
            {
                if (null == _MeasureAllPositionCommand) _MeasureAllPositionCommand = new AsyncCommand(MeasureAllPositionCommandFunc);
                return _MeasureAllPositionCommand;
            }
        }
        private async Task<EventCodeEnum> MeasureAllPositionCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.ChuckPlanarity_MeasureAllPositionCommand();
                }

                Task task = new Task(() =>
                {
                    UpdateDeviceChangeInfo();
                });
                task.Start();
                await task;

                //Task.Run(() =>
                //{
                //    UpdateDeviceChangeInfo();
                //});

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }

            return retval;
        }

        private AsyncCommand _SetAdjustPlanartyCommand;
        public IAsyncCommand SetAdjustPlanartyCommand
        {
            get
            {
                if (null == _SetAdjustPlanartyCommand) _SetAdjustPlanartyCommand = new AsyncCommand(SetAdjustPlanartyFunc);
                return _SetAdjustPlanartyCommand;
            }
        }

        private async Task<EventCodeEnum> SetAdjustPlanartyFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.ChuckPlanarity_SetAdjustPlanartyFunc();
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }

            return retval;
        }
        public void ChangedMarginValue()
        {
            try
            {
                MakePositionData();

                MinHeight = 0;
                MaxHeight = 0;
                DiffHeight = 0;
                AvgHeight = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _CaptureCommand;
        public ICommand CaptureCommand
        {
            get
            {
                if (null == _CaptureCommand) _CaptureCommand = new RelayCommand<Object>(CaptureCommandFunc);
                return _CaptureCommand;
            }
        }

        private void CaptureCommandFunc(object obj)
        {
            try
            {
                string filepath;
                string filename;
                string fullpath;
                string fileextension;

                double curtemp = this.TempController().TempInfo.CurTemp.Value;

                System.Drawing.Image screen = ScreenshotCapture.TakeScreenshot(true);

                filepath = @"C:\ProberSystem\Snapshot\ChuckPlanarity";
                filename = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                filename = filename + $"_CurTemp_{curtemp}_Diff_{DiffHeight}";

                fileextension = ".jpg";

                filename = filename + fileextension;

                fullpath = System.IO.Path.Combine(filepath, filename);

                if (Directory.Exists(System.IO.Path.GetDirectoryName(fullpath)) == false)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullpath));
                }

                if (File.Exists(fullpath) == false)
                {
                    screen.Save(fullpath);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _DecimalBoxClickCommand;
        public ICommand DecimalBoxClickCommand
        {
            get
            {
                if (null == _DecimalBoxClickCommand) _DecimalBoxClickCommand = new RelayCommand<Object>(DecimalBoxClickCommandFunc);
                return _DecimalBoxClickCommand;
            }
        }

        private void DecimalBoxClickCommandFunc(Object param)
        {
            try
            {
                //System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                //tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 1, 5);

                //ChangeMarginValue(Convert.ToDouble(tb.Text));

                //tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                //ChangedMarginValue();

                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;

                double oldvalue = Convert.ToDouble(tb.Text);

                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 1, 7);

                double newvalue = Convert.ToDouble(tb.Text);

                if (newvalue < 0)
                {
                    tb.Text = oldvalue.ToString();
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                }
                else
                {
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                    if (oldvalue != newvalue)
                    {
                        ChangeMarginValue(Convert.ToDouble(tb.Text));

                        ChangedMarginValue();
                    }
                }

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

        private RelayCommand<Object> _FocusingRangeClickCommand;
        public ICommand FocusingRangeClickCommand
        {
            get
            {
                if (null == _FocusingRangeClickCommand) _FocusingRangeClickCommand = new RelayCommand<Object>(FocusingRangeClickCommandFunc);
                return _FocusingRangeClickCommand;
            }
        }
        private void FocusingRangeClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;

                double oldvalue = Convert.ToDouble(tb.Text);

                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 1, 7);

                double newvalue = Convert.ToDouble(tb.Text);

                if (newvalue < 50)
                {
                    tb.Text = oldvalue.ToString();
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                    LoggerManager.Debug($"FocusingRangeClickCommandFunc() value({newvalue})is invalid");
                }
                else if (newvalue > 1000)
                {
                    tb.Text = oldvalue.ToString();
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                    LoggerManager.Debug($"FocusingRangeClickCommandFunc() value({newvalue})is invalid");
                }
                else
                {
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                    if (oldvalue != newvalue)
                    {
                        ChangeFocusingRange(newvalue);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void FloatTextBoxClickCommandFunc(object param)
        {
            try
            {
                if (param is System.Windows.Controls.TextBox)
                {
                    System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;

                    //tb.Text.Replace("+/")

                    string replaceStr = tb.Text.Replace(" +/- ", "");

                    tb.Text = replaceStr;

                    tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.FLOAT | KB_TYPE.DECIMAL);

                    //ChangeSpecHeightValue(Convert.ToDouble(tb.Text));

                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangeMarginValue(double value)
        {
            try
            {
                // Stage로 전달 및 변경하도록 하기.

                if (_RemoteMediumProxy != null)
                {
                    _RemoteMediumProxy.ChuckPlanarity_ChangeMarginValue(value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ChangeFocusingRange(double value)
        {
            try
            {
                FocusingRange = value;
                if (_RemoteMediumProxy != null)
                {
                    _RemoteMediumProxy.ChuckPlanarity_FocusingRangeValue(FocusingRange);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public void ChangeSpecHeightValue(double value)
        //{
        //    try
        //    {
        //        // Stage로 전달 및 변경하도록 하기.

        //        if (_RemoteMediumProxy != null)
        //        {
        //            _RemoteMediumProxy.ChuckPlanarity_ChangeSpecHeightValue(value);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private void UpdateTextData()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        static double sin(double degrees)
        {
            return Math.Sin(degrees * 2d * Math.PI / MaxAngle);
        } //sin
        static double cos(double degrees)
        {
            return Math.Cos(degrees * 2d * Math.PI / MaxAngle);
        } //cos

        internal const double MaxAngle = 360;
        public const double ChuckSize = 300000;
        //public double ChuckEndPointMargin = 5000;
        public double CanvasDiameter = 368;

        public void MakePositionData()
        {
            try
            {
                // 0 : Rectangle
                // 1 : Circle
                int PositionShapeMode = 0;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Point Origin = new Point(0, 0);
                    Point RealOrigin = new Point(-(ChuckSize / 2.0), +(ChuckSize / 2.0));

                    double RealXRatio = ChuckSize / CanvasDiameter;
                    double RealYRatio = ChuckSize / CanvasDiameter;

                    //ChuckEndPointMargin = 50000;

                    double InnerMargin = ChuckEndPointMargin * CanvasDiameter / ChuckSize;

                    double InnerCircleDiameter = CanvasDiameter - InnerMargin;

                    Point InnerOrigin = new Point(InnerMargin / 2.0, InnerMargin / 2.0);

                    double RadiusX = CanvasDiameter / 2.0;
                    double RadiusY = CanvasDiameter / 2.0;

                    double InnerCircleRadiusX = InnerCircleDiameter / 2.0;
                    double InnerCircleRadiusY = InnerCircleDiameter / 2.0;

                    Point Center = new Point(RadiusX, RadiusY);

                    double LineNum = 8;
                    int PointNum = 2;

                    double Angle = 360 / LineNum;

                    //double AngularSize = 45;

                    //double angleTo = AngularSize;

                    if (Items == null)
                    {
                        Items = new ObservableCollection<BaseThing>();
                    }
                    else
                    {
                        Items.Clear();
                    }

                    CircleVM ChuckBaseCircle = new CircleVM();

                    ChuckBaseCircle.Top = Origin.X;
                    ChuckBaseCircle.Left = Origin.Y;

                    ChuckBaseCircle.EllipseWidth = CanvasDiameter;
                    ChuckBaseCircle.EllipseHeight = CanvasDiameter;

                    ChuckBaseCircle.color = Brushes.White;
                    ChuckBaseCircle.Fillcolor = Brushes.Transparent;

                    Items.Add(ChuckBaseCircle);

                    CircleVM ChuckBaseInnerCircle = new CircleVM();

                    ChuckBaseInnerCircle.Top = InnerOrigin.X;
                    ChuckBaseInnerCircle.Left = InnerOrigin.Y;

                    ChuckBaseInnerCircle.EllipseWidth = InnerCircleDiameter;
                    ChuckBaseInnerCircle.EllipseHeight = InnerCircleDiameter;
                    ChuckBaseInnerCircle.color = Brushes.Green;
                    ChuckBaseInnerCircle.Fillcolor = Brushes.Transparent;

                    Items.Add(ChuckBaseInnerCircle);

                    double ratio = (1.0 * 1 / (PointNum));
                    Double ShapeWidth = 30;
                    Double ShapeHeight = 30;

                    ChuckPosList.Clear();

                    //double textboxOffsetX = 0;
                    //double textboxOffsetY = 0;

                    for (int i = 0; i < LineNum; i++)
                    {
                        LineVM tmpline = new LineVM();
                        tmpline.color = Brushes.White;

                        double tmpAngle = Angle * i;

                        tmpline.X1 = Center.X;
                        tmpline.Y1 = Center.Y;

                        //tmpline.X2 = Center.X + RadiusX * cos(tmpAngle);
                        //tmpline.Y2 = Center.Y + RadiusY * sin(tmpAngle);

                        tmpline.X2 = Center.X + InnerCircleRadiusX * cos(tmpAngle);
                        tmpline.Y2 = Center.Y + InnerCircleRadiusY * sin(tmpAngle);

                        Items.Add(tmpline);

                        // Make Point

                        for (int j = 1; j <= PointNum; j++)
                        {
                            if (PositionShapeMode == 0)
                            {
                                RectangleVM tmpShape = new RectangleVM();

                                tmpShape.Width = ShapeWidth;
                                tmpShape.Height = ShapeHeight;

                                tmpShape.command = ChuckMoveCommand;

                                tmpShape.color = Brushes.Green;
                                tmpShape.Fillcolor = Brushes.White;

                                // 반드시 포함되는 점 : tmpline.X2, tmpline.Y2에 해당하는 위치
                                // 그 다음 위치는 위의 점으로부터 센터로 들어오면서...

                                if (j == 1)
                                {
                                    tmpShape.Top = tmpline.X2 - (tmpShape.Height / 2.0);
                                    tmpShape.Left = tmpline.Y2 - (tmpShape.Width / 2.0);
                                }
                                else
                                {
                                    tmpShape.Top = tmpline.X2 - (InnerCircleRadiusX * ratio * cos(tmpAngle)) * (j - 1);
                                    tmpShape.Left = tmpline.Y2 - (InnerCircleRadiusY * ratio * sin(tmpAngle)) * (j - 1);

                                    tmpShape.Top = tmpShape.Top - (tmpShape.Height / 2.0);
                                    tmpShape.Left = tmpShape.Left - (tmpShape.Width / 2.0);
                                }

                                TextBoxVM tmpTextBox = new TextBoxVM();

                                tmpTextBox.Top = tmpShape.Top;
                                tmpTextBox.Left = tmpShape.Left + (tmpShape.Width / 2.0);
                                tmpTextBox.TbText = $"X";
                                //tmpTextBox.ForegroundColor = Brushes.Red;
                                //tmpTextBox.BackgroundColor = Brushes.Transparent;

                                Items.Add(tmpShape);
                                Items.Add(tmpTextBox);

                                // Calculate Real Position

                                ChuckPos tmpChuckPos = new ChuckPos();
                                //tmpChuckPos.CanvasCircle = tmpShape;
                                tmpChuckPos.CanvasTextBox = tmpTextBox;

                                double RealXOffset = RealXRatio * (tmpShape.Left + (tmpShape.Width / 2.0));
                                double RealYOffset = RealYRatio * (tmpShape.Top + (tmpShape.Height / 2.0));

                                tmpChuckPos.XPos = RealOrigin.X + RealXOffset;
                                tmpChuckPos.YPos = RealOrigin.Y - RealYOffset;

                                tmpChuckPos.XPos = Math.Round(tmpChuckPos.XPos, 0);
                                tmpChuckPos.YPos = Math.Round(tmpChuckPos.YPos, 0);

                                tmpChuckPos.ZPos = 0;

                                ChuckPosList.Add(tmpChuckPos);

                                tmpShape.commandparam = tmpChuckPos;
                            }
                            else
                            {
                                CircleVM tmpShape = new CircleVM();

                                tmpShape.EllipseWidth = ShapeWidth;
                                tmpShape.EllipseHeight = ShapeHeight;

                                tmpShape.color = Brushes.Green;
                                tmpShape.Fillcolor = Brushes.White;

                                // 반드시 포함되는 점 : tmpline.X2, tmpline.Y2에 해당하는 위치
                                // 그 다음 위치는 위의 점으로부터 센터로 들어오면서...

                                if (j == 1)
                                {
                                    tmpShape.Top = tmpline.X2 - (tmpShape.EllipseHeight / 2.0);
                                    tmpShape.Left = tmpline.Y2 - (tmpShape.EllipseWidth / 2.0);
                                }
                                else
                                {
                                    tmpShape.Top = tmpline.X2 - (InnerCircleRadiusX * ratio * cos(tmpAngle)) * (j - 1);
                                    tmpShape.Left = tmpline.Y2 - (InnerCircleRadiusY * ratio * sin(tmpAngle)) * (j - 1);

                                    tmpShape.Top = tmpShape.Top - (tmpShape.EllipseHeight / 2.0);
                                    tmpShape.Left = tmpShape.Left - (tmpShape.EllipseWidth / 2.0);
                                }

                                TextBoxVM tmpTextBox = new TextBoxVM();

                                tmpTextBox.Top = tmpShape.Top;
                                tmpTextBox.Left = tmpShape.Left + (tmpShape.EllipseWidth / 2.0);
                                tmpTextBox.TbText = $"X";
                                //tmpTextBox.ForegroundColor = Brushes.Red;
                                //tmpTextBox.BackgroundColor = Brushes.Transparent;

                                Items.Add(tmpShape);
                                Items.Add(tmpTextBox);

                                // Calculate Real Position

                                ChuckPos tmpChuckPos = new ChuckPos();
                                //tmpChuckPos.CanvasCircle = tmpShape;
                                tmpChuckPos.CanvasTextBox = tmpTextBox;

                                double RealXOffset = RealXRatio * (tmpShape.Left + (tmpShape.EllipseWidth / 2.0));
                                double RealYOffset = RealYRatio * (tmpShape.Top + (tmpShape.EllipseHeight / 2.0));

                                tmpChuckPos.XPos = RealOrigin.X + RealXOffset;
                                tmpChuckPos.YPos = RealOrigin.Y - RealYOffset;

                                tmpChuckPos.ZPos = 0;

                                ChuckPosList.Add(tmpChuckPos);


                            }
                        }
                    }

                    if (PositionShapeMode == 0)
                    {
                        RectangleVM tmpShape = new RectangleVM();

                        tmpShape.Width = ShapeWidth;
                        tmpShape.Height = ShapeHeight;
                        tmpShape.color = Brushes.Green;
                        tmpShape.Fillcolor = Brushes.White;

                        tmpShape.Top = Center.X - (tmpShape.Width / 2.0);
                        tmpShape.Left = Center.Y - (tmpShape.Height / 2.0);

                        TextBoxVM tmpTextBox = new TextBoxVM();

                        tmpTextBox.Top = tmpShape.Top;
                        tmpTextBox.Left = tmpShape.Left + (tmpShape.Width / 2.0);
                        tmpTextBox.TbText = $"X";
                        //tmpTextBox.ForegroundColor = Brushes.Red;
                        //tmpTextBox.BackgroundColor = Brushes.Transparent;

                        Items.Add(tmpShape);
                        Items.Add(tmpTextBox);

                        ChuckPos tmpChuckPos = new ChuckPos();
                        tmpChuckPos.CanvasTextBox = tmpTextBox;

                        tmpChuckPos.XPos = 0;
                        tmpChuckPos.YPos = 0;
                        tmpChuckPos.ZPos = 0;

                        ChuckPosList.Add(tmpChuckPos);

                        tmpShape.command = ChuckMoveCommand;
                        tmpShape.commandparam = tmpChuckPos;
                    }
                    else
                    {
                        CircleVM tmpShape = new CircleVM();

                        tmpShape.EllipseWidth = ShapeWidth;
                        tmpShape.EllipseHeight = ShapeHeight;
                        tmpShape.color = Brushes.Green;
                        tmpShape.Fillcolor = Brushes.White;

                        tmpShape.Top = Center.X - (tmpShape.EllipseWidth / 2.0);
                        tmpShape.Left = Center.Y - (tmpShape.EllipseHeight / 2.0);

                        TextBoxVM tmpTextBox = new TextBoxVM();

                        tmpTextBox.Top = tmpShape.Top;
                        tmpTextBox.Left = tmpShape.Left + (tmpShape.EllipseWidth / 2.0);
                        tmpTextBox.TbText = $"X";
                        //tmpTextBox.ForegroundColor = Brushes.Red;
                        //tmpTextBox.BackgroundColor = Brushes.Transparent;

                        Items.Add(tmpShape);
                        Items.Add(tmpTextBox);

                        ChuckPos tmpChuckPos = new ChuckPos();
                        tmpChuckPos.CanvasTextBox = tmpTextBox;

                        tmpChuckPos.XPos = 0;
                        tmpChuckPos.YPos = 0;
                        tmpChuckPos.ZPos = 0;

                        ChuckPosList.Add(tmpChuckPos);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private void UpdateDeviceChangeInfo()
        //{
        //    try
        //    {
        //        var info = _RemoteMediumProxy.GetChuckPlanarityInfo();

        //        if (info != null)
        //        {
        //            this.MinHeight = info.MinHeight;
        //            this.MaxHeight = info.MaxHeight;
        //            this.DiffHeight = info.DiffHeight;
        //            //this.SpecHeight = info.SpecHeight;
        //            this.AvgHeight = info.AvgHeight;
        //            this.ChuckEndPointMargin = info.ChuckEndPointMargin;

        //            //this.CurrentChuckPosEnum = info.CurrentChuckPosEnum;

        //            this.ChuckPosList = new ObservableCollection<ChuckPos>(info.ChuckPosList);

        //            //if(info.Items != null)
        //            //{
        //            //    this.Items = new ObservableCollection<BaseThing>(info.Items);
        //            //}
        //            //object target;

        //            //bool retval = false;
        //            //retval = SerializeManager.DeserializeFromByte(info.ChuckPosList, out target, typeof(ObservableCollection<ChuckPos>));
        //            //target = SerializeManager.ByteToObject(info.ChuckPosList);

        //            //if (retval == true)
        //            //{
        //            //    this.ChuckPosList = target as ObservableCollection<ChuckPos>;
        //            //}
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public void UpdateDeviceChangeInfo(ChuckPlanarityDataDescription infoFromCell = null)
        {
            ChuckPlanarityDataDescription info = null;
            try
            {
                if (infoFromCell != null)
                {
                    info = infoFromCell;
                }
                else
                {
                    info = _RemoteMediumProxy.GetChuckPlanarityInfo();
                }
                

                if (info != null)
                {
                    this.MinHeight = info.MinHeight;
                    this.MaxHeight = info.MaxHeight;
                    this.DiffHeight = info.DiffHeight;
                    //this.SpecHeight = info.SpecHeight;
                    this.AvgHeight = info.AvgHeight;
                    this.ChuckEndPointMargin = info.ChuckEndPointMargin;
                    this.FocusingRange = info.ChuckFocusingRange;

                    //this.CurrentChuckPosEnum = info.CurrentChuckPosEnum;

                    if(this.ChuckPosList == null)
                    {
                        this.ChuckPosList = new ObservableCollection<ChuckPos>(info.ChuckPosList);
                    }
                    else
                    {
                        for (int i = 0; i < this.ChuckPosList.Count; i++)
                        {
                            this.ChuckPosList[i].CanvasTextBox = info.ChuckPosList[i].CanvasTextBox;
                            this.ChuckPosList[i].XPos = info.ChuckPosList[i].XPos;
                            this.ChuckPosList[i].YPos = info.ChuckPosList[i].YPos;
                            this.ChuckPosList[i].ZPos = info.ChuckPosList[i].ZPos;
                        }
                    }


                    if (ItemBuf == null)
                    {
                        ItemBuf = new ObservableCollection<BaseThing>();
                    }
                    else
                    {
                        ItemBuf.Clear();
                    }
                    if (Items != null)
                    {
                        for (int i = 0; i < Items.Count; i++)
                        {
                            if (Items[i] is TextBoxVM)
                            {
                                ItemBuf.Add(Items[i]);
                            }
                        }
                        for (int i = 0; i < ItemBuf.Count; i++)
                        {
                            TmpTextboxVM = ItemBuf[i] as TextBoxVM;
                            TmpTextboxVM.TbText = this.ChuckPosList[i].CanvasTextBox.TbText;
                            TmpTextboxVM.IsSelected = this.ChuckPosList[i].CanvasTextBox.IsSelected;
                        }
                    }
                    //if(info.Items != null)
                    //{
                    //    this.Items = new ObservableCollection<BaseThing>(info.Items);
                    //}
                    //object target;

                    //bool retval = false;
                    //retval = SerializeManager.DeserializeFromByte(info.ChuckPosList, out target, typeof(ObservableCollection<ChuckPos>));
                    //target = SerializeManager.ByteToObject(info.ChuckPosList);

                    //if (retval == true)
                    //{
                    //    this.ChuckPosList = target as ObservableCollection<ChuckPos>;
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public ChuckPlanarityDataDescription GetChuckPlanarityInfo()
        {
            throw new NotImplementedException();
        }

        public Task WrapperChuckMoveCommand(ChuckPos param)
        {
            throw new NotImplementedException();
        }

        Task IChuckPlanarityVM.ChuckMoveCommandFunc(object param)
        {
            throw new NotImplementedException();
        }

        Task<EventCodeEnum> IChuckPlanarityVM.MeasureOnePositionCommandFunc()
        {
            throw new NotImplementedException();
        }

        Task<EventCodeEnum> IChuckPlanarityVM.MeasureAllPositionCommandFunc()
        {
            throw new NotImplementedException();
        }

        Task<EventCodeEnum> IChuckPlanarityVM.SetAdjustPlanartyFunc()
        {
            throw new NotImplementedException();
        }
    }
}
