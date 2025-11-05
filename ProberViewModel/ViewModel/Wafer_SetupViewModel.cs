using System;
using System.Threading.Tasks;

namespace PolishWaferDevMainPageViewModel.SettingMainViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using Focusing;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.LightJog;
    using ProberInterfaces.Param;
    using ProberInterfaces.PolishWafer;
    using RelayCommandBase;
    using UcDisplayPort;
    using VirtualKeyboardControl;

    public class Wafer_SetupViewModel : INotifyPropertyChanged, IFactoryModule, IIPolishWaferSetupViewModel, IUseLightJog
    {
        #region == > PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region //..Property
        public IWaferObject WaferObject { get; set; }

        public double WaferSize
        {
            get { return this.GetParam_Wafer().GetPhysInfo().WaferSize_um.Value; }
        }

        public EnumWaferSize WaferSizeEnum
        {
            get { return this.GetParam_Wafer().GetPhysInfo().WaferSizeEnum; }
        }


        private double _Thickness = 0.0;
        public double Thickness
        {
            get { return _Thickness; }
            set
            {
                if (value != _Thickness)
                {
                    _Thickness = value;
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

        private double _EdgeMargin;
        public double EdgeMargin
        {
            get { return _EdgeMargin; }
            set
            {
                if (value != _EdgeMargin)
                {
                    _EdgeMargin = value;
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
                _CurCam = value;
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
                    if (_MainViewTarget is IWaferObject)
                    {
                        LightJogVisibility = Visibility.Hidden;
                        MiniViewHorizontalAlignment = HorizontalAlignment.Left;
                        MiniViewVerticalAlignment = VerticalAlignment.Bottom;
                    }

                    else if (_MainViewTarget is IDisplayPort)
                    {
                        LightJogVisibility = Visibility.Visible;
                        MiniViewHorizontalAlignment = HorizontalAlignment.Right;
                        MiniViewVerticalAlignment = VerticalAlignment.Top;
                    }
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
                    RaisePropertyChanged();
                }
            }
        }

        private HorizontalAlignment _MiniViewHorizontalAlignment
            = HorizontalAlignment.Left;
        public HorizontalAlignment MiniViewHorizontalAlignment
        {
            get { return _MiniViewHorizontalAlignment; }
            set
            {
                if (value != _MiniViewHorizontalAlignment)
                {
                    _MiniViewHorizontalAlignment = value;
                    RaisePropertyChanged();
                }
            }
        }


        private VerticalAlignment _MiniViewVerticalAlignment
             = VerticalAlignment.Bottom;
        public VerticalAlignment MiniViewVerticalAlignment
        {
            get { return _MiniViewVerticalAlignment; }
            set
            {
                if (value != _MiniViewVerticalAlignment)
                {
                    _MiniViewVerticalAlignment = value;
                    RaisePropertyChanged();
                }
            }
        }

        private HorizontalAlignment _LightJogVerticalAlignment
           = HorizontalAlignment.Center;
        public HorizontalAlignment LightJogVerticalAlignment
        {
            get { return _LightJogVerticalAlignment; }
            set
            {
                if (value != _LightJogVerticalAlignment)
                {
                    _LightJogVerticalAlignment = value;
                    RaisePropertyChanged();
                }
            }
        }
        private VerticalAlignment _LightJogHorizontalAlignmentt
             = VerticalAlignment.Bottom;
        public VerticalAlignment LightJogHorizontalAlignmentt
        {
            get { return _LightJogHorizontalAlignmentt; }
            set
            {
                if (value != _LightJogHorizontalAlignmentt)
                {
                    _LightJogHorizontalAlignmentt = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Visibility _LightJogVisibility;
        public Visibility LightJogVisibility
        {
            get { return _LightJogVisibility; }
            set
            {
                if (value != _LightJogVisibility)
                {
                    _LightJogVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IDisplayPort _DisplayPort;

        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set { _DisplayPort = value; }
        }

        public LightJogViewModel LightJog { get; set; }

        private IFocusing _FocusModel;
        public IFocusing FocusModel
        {
            get
            {
                return _FocusModel;
            }
        }

        private ModuleDllInfo _FocusingModuleDllInfo;

        public ModuleDllInfo FocusingModuleDllInfo
        {
            get { return _FocusingModuleDllInfo; }
            set { _FocusingModuleDllInfo = value; }
        }

        private FocusParameter _FocusParam;
        public FocusParameter FocusParam
        {
            get { return _FocusParam; }
            set { _FocusParam = value; }
        }

        #endregion

        public Wafer_SetupViewModel()
        {
            try
            {
                WaferObject = this.GetParam_Wafer();
                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                _FocusModel = this.FocusManager().GetFocusingModel(FocusingModuleDllInfo);

                FocusParam = new NormalFocusParameter();
                this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, FocusParam);


                LightJog = new LightJogViewModel(
                       maxLightValue: 255,
                       minLightValue: 0);

                _DisplayPort = new DisplayPort() { GUID = new Guid("C54B873F-F2B7-1CBB-BD95-E9F6F460654E") };
                ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;
                foreach (var cam in this.VisionManager().GetCameras())
                {
                    this.VisionManager().SetDisplayChannel(cam, DisplayPort);
                }

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

                MainViewTarget = DisplayPort;
                MiniViewTarget = this.GetParam_Wafer();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
            _FocusModel = this.FocusManager().GetFocusingModel(FocusingModuleDllInfo);

            FocusParam = new NormalFocusParameter();
            this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, FocusParam);
        }

        public Task<EventCodeEnum> PageSwitched()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                LightJog.InitCameraJog(this);

                #region //..DataInit

                #endregion

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }


        #region //..Command & Command Method

        #region //..ViewSwapCommand
        private RelayCommand<object> _ViewSwapCommand;
        public ICommand ViewSwapCommand
        {
            get
            {
                if (null == _ViewSwapCommand) _ViewSwapCommand = new RelayCommand<object>(ViewSwapFunc);
                return _ViewSwapCommand;
            }
        }
        public virtual void ViewSwapFunc(object parameter)
        {
            object swap = MainViewTarget;
            //MainViewTarget = WaferObject;
            MainViewTarget = MiniViewTarget;
            MiniViewTarget = swap;
        }
        #endregion

        #region //..Wafe Define Name Input Command
        private RelayCommand<Object> _DefineNameTextBoxClickCommand;
        public ICommand DefineNameTextBoxClickCommand
        {
            get
            {
                if (null == _DefineNameTextBoxClickCommand) _DefineNameTextBoxClickCommand = new RelayCommand<Object>(DefineNameTextBoxClickCommandFunc);
                return _DefineNameTextBoxClickCommand;
            }
        }
        private void DefineNameTextBoxClickCommandFunc(Object param)
        {
            System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
            tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.ALPHABET);
            tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
        }
        #endregion

        #region //..Text Box Input Command
        private RelayCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
                return _TextBoxClickCommand;
            }
        }
        private void TextBoxClickCommandFunc(Object param)
        {
            System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
            tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.FLOAT);
            tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
        }
        #endregion

        //private AsyncCommand _MoveToWaferThicknessCommand;
        //public ICommand MoveToWaferThicknessCommand
        //{
        //    get
        //    {
        //        if (null == _MoveToWaferThicknessCommand) _MoveToWaferThicknessCommand = new AsyncCommand(MoveToWaferThicknessCommandFunc);
        //        return _MoveToWaferThicknessCommand;
        //    }
        //}
        //private async Task MoveToWaferThicknessCommandFunc()
        //{
        //    try
        //    {
        //        
        //        CatCoordinates coordinate = CurCam.GetCurCoordPos();
        //        if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
        //            this.StageSupervisor().StageModuleState.WaferHighViewMove(coordinate.GetX(), coordinate.GetY(), Wafer.GetPhysInfo().Thickness.Value);
        //        else
        //            this.StageSupervisor().StageModuleState.WaferLowViewMove(coordinate.GetX(), coordinate.GetY(), Wafer.GetPhysInfo().Thickness.Value);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        
        //    }
        //}

        //private AsyncCommand _AdjustWaferHeightCommand;
        //public ICommand AdjustWaferHeightCommand
        //{
        //    get
        //    {
        //        if (null == _AdjustWaferHeightCommand) _AdjustWaferHeightCommand = new AsyncCommand(AdjustWaferHeightCommandFunc);
        //        return _AdjustWaferHeightCommand;
        //    }
        //}
        //private async Task AdjustWaferHeightCommandFunc()
        //{
        //    try
        //    {
        //        
        //        FocusParam.FocusingCam.Value = CurCam.GetChannelType();
        //        FocusModel.Focusing_Retry(FocusParam, false, true, false);
        //        Wafer.GetPhysInfo().Thickness.Value = Math.Round(CurCam.GetCurCoordPos().GetZ(), 3);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        
        //    }
        //}

        #endregion
    }
}
