using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ProberInterfaces.Param;
using SubstrateObjects;
using ErrorMapping;
using ErrorMapping.Graph;
using ProberErrorCode;
using ProberInterfaces.Vision;
using Focusing;
using LogModule;
using System.Runtime.CompilerServices;
using ProbeMotion;
using MetroDialogInterfaces;
using ProberInterfaces.State;

namespace ProberSystem.UserControls.VisionMapping
{
    /// <summary>
    /// UcVisionMappingOPUSVSetup.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcVisionMappingOPUSVSetup : UserControl, IMainScreenView, INotifyPropertyChanged, IMainScreenViewModel, ISetUpState
    {
        readonly Guid _PageGUID = new Guid("8412aec0-e1c1-4a4e-aa73-2b0fef470909");
        public Guid ScreenGUID { get { return _PageGUID; } }


        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private double _ZeroPosX = -1;
        public double ZeroPosX
        {
            get { return _ZeroPosX; }
            set
            {
                if (value != _ZeroPosX)
                {
                    _ZeroPosX = value;
                }
            }
        }

        private double _ZeroPosY = -1;
        public double ZeroPosY
        {
            get { return _ZeroPosY; }
            set
            {
                if (value != _ZeroPosY)
                {
                    _ZeroPosY = value;
                }
            }
        }

        private double _ZeroPosZ = -1;
        public double ZeroPosZ
        {
            get { return _ZeroPosZ; }
            set
            {
                if (value != _ZeroPosZ)
                {
                    _ZeroPosZ = value;
                }
            }
        }


        //DispatcherTimer timer = new DispatcherTimer();

        private ErrorMappingManager _ErrMappingManager;
        public ErrorMappingManager ErrMappingManager
        {
            get { return _ErrMappingManager; }
            set
            {
                if (value != _ErrMappingManager)
                {
                    _ErrMappingManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFocusing _FocusingModule;
        public IFocusing FocusingModule
        {
            get
            {
                if (_FocusingModule == null)
                    _FocusingModule = this.FocusManager().GetFocusingModel(FocusingDLLInfo.GetNomalFocusingDllInfo());

                return _FocusingModule;
            }
            set
            {
                if (value != _FocusingModule)
                {
                    _FocusingModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFocusParameter _FocusingParam;
        public IFocusParameter FocusingParam
        {
            get { return _FocusingParam; }
            set
            {
                if (value != _FocusingParam)
                {
                    _FocusingParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        private WaferObject Wafer;

        private double _PZValue;
        public double PZValue
        {
            get { return _PZValue; }
            set
            {
                if (value != _PZValue)
                {
                    _PZValue = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _RectWidth;
        public double RectWidth
        {
            get { return _RectWidth; }
            set
            {
                if (value != _RectWidth)
                {
                    _RectWidth = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _RectHeight;
        public double RectHeight
        {
            get { return _RectHeight; }
            set
            {
                if (value != _RectHeight)
                {
                    _RectHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _RectLeft;
        public double RectLeft
        {
            get { return _RectLeft; }
            set
            {
                if (value != _RectLeft)
                {
                    _RectLeft = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _RectTop;
        public double RectTop
        {
            get { return _RectTop; }
            set
            {
                if (value != _RectTop)
                {
                    _RectTop = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IVisionManager _VisionManager;
        public IVisionManager VisionManager
        {
            get { return _VisionManager; }
            set
            {
                if (value != _VisionManager)
                {
                    _VisionManager = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IMotionManager _MotionManager;
        public IMotionManager MotionManager
        {
            get { return _MotionManager; }
            set
            {
                if (value != _MotionManager)
                {
                    _MotionManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IStageSupervisor _StageSupervisor;
        public IStageSupervisor StageSupervisor
        {
            get { return _StageSupervisor; }
            set
            {
                if (value != _StageSupervisor)
                {
                    _StageSupervisor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICamera _AssignedCamera;
        public ICamera AssignedCamera
        {
            get { return _AssignedCamera; }
            set
            {
                if (value != _AssignedCamera)
                {
                    _AssignedCamera = value;
                    RaisePropertyChanged();
                }
            }
        }



        public UcVisionMappingOPUSVSetup()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string ViewModelType => throw new NotImplementedException();

        private int preCamIndex = -1;

        public new bool Initialized { get; set; } = false;

        public bool isStopFlag = false;

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    ErrMappingManager = new ErrorMappingManager();

                    ErrMappingManager.InitModule();

                    FocusingParam = new NormalFocusParameter();
                    FocusingParam.SetDefaultParam();

                    Wafer = (WaferObject)this.StageSupervisor().WaferObject;

                    VisionManager = this.VisionManager();
                    MotionManager = this.MotionManager();
                    StageSupervisor = this.StageSupervisor();

                    //VMSetupDisPlay.StageSuperVisor = this.StageSupervisor();

                    this.StageSupervisor().StageModuleState.ZCLEARED();

                    DieSizeX_TB.Text = ErrMappingManager.ErrMappingParam.DieSizeX.ToString();
                    DieSizeY_TB.Text = ErrMappingManager.ErrMappingParam.DieSizeY.ToString();
                    DieSizeX_LB.Content = ErrMappingManager.ErrMappingParam.DieSizeX.ToString();
                    DieSizeY_LB.Content = ErrMappingManager.ErrMappingParam.DieSizeY.ToString();
                    var axisT = this.MotionManager().GetAxis(EnumAxisConstants.C);
                    //    MotionManager.AbsMove(axisT, -35953.2, axisT.Param.Speed.Value, axisT.Param.Acceleration.Value);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, true);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, true);

                    if (this.MotionManager().ErrorManager.CompensationModule.Enable1D == true)
                    {
                        FirstErrorComp_CB.IsChecked = true;
                    }
                    else
                    {
                        FirstErrorComp_CB.IsChecked = false;
                    }
                    //IOManager.IOServ.IOList[0].WriteBit(4, 0, true);
                    //IOManager.IOServ.IOList[0].WriteBit(4, 0, true);
                    //DataContext = this;

                    AssignedCamera = this.VisionManager().GetCam(EnumProberCam.MAP_REF_CAM);
                    
                    this.VisionManager().StartGrab(EnumProberCam.MAP_1_CAM, this);

                    foreach (var cam in this.VisionManager().GetCameras())
                    {
                        this.VisionManager().SetDisplayChannel(cam, VMSetupDisPlay);
                    }
                    RectWidth = 360;
                    RectHeight = 360;

                    bool waf = false;
                    this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK, out waf);
                    LockWafer.IsChecked = waf;
                    PZValue = this.MotionManager().GetAxis(EnumAxisConstants.PZ).Status.Position.Command;

                    Initialized = true;
                    MachineSquareness.Text = this.CoordinateManager().StageCoord.MachineSequareness.ToString();

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void DeInitModule()
        {

        }

        public EventCodeEnum InitPage(object parameter = null)
        {
            return EventCodeEnum.NONE;
        }
        private void btnJogUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double posY = Double.Parse(txt_y.Text);
                this.StageSupervisor().StageModuleState.VMRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Y), -posY);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btnJogDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double posY = Double.Parse(txt_y.Text);
                this.StageSupervisor().StageModuleState.VMRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Y), posY);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btnJogLeft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double posX = Double.Parse(txt_x.Text);
                this.StageSupervisor().StageModuleState.VMRelMove(this.MotionManager().GetAxis(EnumAxisConstants.X), posX);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btnJogRight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double posX = Double.Parse(txt_x.Text);
                this.StageSupervisor().StageModuleState.VMRelMove(this.MotionManager().GetAxis(EnumAxisConstants.X), -posX);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void btnJogZUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double posZ = Double.Parse(txt_Z.Text);
                this.StageSupervisor().StageModuleState.VMRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Z), posZ);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btnJogZDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double posZ = Double.Parse(txt_Z.Text);
                this.StageSupervisor().StageModuleState.VMRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Z), -posZ);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btnJogThetaLeft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axisT = this.MotionManager().GetAxis(EnumAxisConstants.C);
                double posT = Double.Parse(txt_T.Text);
                posT *= 10000;
                this.StageSupervisor().StageModuleState.VMRelMove(axisT, posT);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btnJogThetaRight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axisT = this.MotionManager().GetAxis(EnumAxisConstants.C);
                double posT = Double.Parse(txt_T.Text);
                posT *= 10000;
                this.StageSupervisor().StageModuleState.VMRelMove(axisT, -posT);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void thetaAlignBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, false);
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, false);
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, false);
                //this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCK_BLOW, true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MoveZero_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, true);
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //double posX = Double.Parse(txt_x.Text);
            //this.StageSupervisor().StageModuleState.WaferHighViewMove(0, 0,
            //    Wafer.PhysInfo.Thickness.Value);

            //this.VisionManager().StartGrab(EnumProberCam.WAFER_HIGH_CAM);
        }




        private void WaferFocusingBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                //==> New Focusing
                //FocusParameter focusParam = new FocusParameter(this.WaferAligner().WADeviceFile.WaferFocusParam);
                //focusParam.FocusingCam = EnumProberCam.MAP_1_CAM;

                //await Task.Run(() => this.WaferAligner().WaferFocusingModule.Focusing_Retry(
                //    FocusingType.WAFER,
                //    focusParam,
                //    false,//==> Light Change
                //    false,//==> brute force retry
                //    false));//==> find potential
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void VisionCam1_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonColorChange(preCamIndex, Brushes.Green);
                ButtonColorChange(0, Brushes.Blue);
                ErrMappingManager.TransferCurrentCam(0);
                int axisNum = 0;
                if (RB_X.IsChecked.Value)
                {
                    axisNum = 0;
                }
                else if (RB_Y.IsChecked.Value)
                {
                    axisNum = 1;
                }
                else
                {
                    return;
                }
                ErrMappingManager.CurrentMoveToVMPosOPUSV(axisNum);
                preCamIndex = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void VisionCam2_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonColorChange(preCamIndex, Brushes.Green);
                ButtonColorChange(1, Brushes.Blue);
                ErrMappingManager.TransferCurrentCam(1);
                int axisNum = 0;
                if (RB_X.IsChecked.Value)
                {
                    axisNum = 0;
                }
                else if (RB_Y.IsChecked.Value)
                {
                    axisNum = 1;
                }
                else
                {
                    return;
                }
                ErrMappingManager.CurrentMoveToVMPosOPUSV(axisNum);
                preCamIndex = 1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void VisionCam3_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonColorChange(preCamIndex, Brushes.Green);
                ButtonColorChange(2, Brushes.Blue);
                ErrMappingManager.TransferCurrentCam(2);
                int axisNum = 0;
                if (RB_X.IsChecked.Value)
                {
                    axisNum = 0;
                }
                else if (RB_Y.IsChecked.Value)
                {
                    axisNum = 1;
                }
                else
                {
                    return;
                }
                ErrMappingManager.CurrentMoveToVMPosOPUSV(axisNum);
                preCamIndex = 2;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void VisionCam4_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonColorChange(preCamIndex, Brushes.Green);
                ButtonColorChange(3, Brushes.Blue);
                ErrMappingManager.TransferCurrentCam(3);
                int axisNum = 0;
                if (RB_X.IsChecked.Value)
                {
                    axisNum = 0;
                }
                else if (RB_Y.IsChecked.Value)
                {
                    axisNum = 1;
                }
                else
                {
                    return;
                }
                ErrMappingManager.CurrentMoveToVMPosOPUSV(axisNum);
                preCamIndex = 3;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void VisionCam5_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonColorChange(preCamIndex, Brushes.Green);
                ButtonColorChange(4, Brushes.Blue);
                ErrMappingManager.TransferCurrentCam(4);
                int axisNum = 0;
                if (RB_X.IsChecked.Value)
                {
                    axisNum = 0;
                }
                else if (RB_Y.IsChecked.Value)
                {
                    axisNum = 1;
                }
                else
                {
                    return;
                }
                ErrMappingManager.CurrentMoveToVMPosOPUSV(axisNum);
                preCamIndex = 4;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void VisionCam6_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonColorChange(preCamIndex, Brushes.Green);
                ButtonColorChange(5, Brushes.Blue);
                ErrMappingManager.TransferCurrentCam(5);
                int axisNum = 0;
                if (RB_X.IsChecked.Value)
                {
                    axisNum = 0;
                }
                else if (RB_Y.IsChecked.Value)
                {
                    axisNum = 1;
                }
                else
                {
                    return;
                }
                ErrMappingManager.CurrentMoveToVMPosOPUSV(axisNum);
                preCamIndex = 5;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private void DieSizeSet_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double dieSizeX = Double.Parse(DieSizeX_TB.Text);
                double dieSizeY = Double.Parse(DieSizeY_TB.Text);
                ErrMappingManager.ErrMappingParam.DieSizeX = dieSizeX;
                ErrMappingManager.ErrMappingParam.DieSizeY = dieSizeY;
                DieSizeX_LB.Content = dieSizeX.ToString();
                DieSizeY_LB.Content = dieSizeY.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async void RegPattern_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //ImageBuffer image = null;
                string pattenPath = "";
                CatCoordinates vmPos = null;
                //FocusParameter focusParam = new FocusParameter(this.WaferAligner().WADeviceFile.WaferFocusParam);
                //focusParam.FocusingCam = ErrMappingManager.CurrentMappingCam.CamType;
                bool isRB_X = false;
                //await Task.Run(() => this.WaferAligner().WaferFocusingModule.Focusing_Retry(
                //FocusingType.WAFER,
                //    focusParam,
                //    false,//==> Light Change
                //    false,//==> brute force retry
                //    false));//==> find potential
                if (RB_X.IsChecked.Value && ErrMappingManager.CurrentMappingCam.CamNumber <= 4)
                {
                    pattenPath = ErrMappingManager.ErrMappingParam.PMParam_X.PMParameter.ModelFilePath.Value;
                    vmPos = ErrMappingManager.CurrentMappingCam.VMPos_X;
                    isRB_X = true;
                }
                else if (RB_Y.IsChecked.Value)
                {
                    pattenPath = ErrMappingManager.ErrMappingParam.PMParam_Y.PMParameter.ModelFilePath.Value;
                    vmPos = ErrMappingManager.CurrentMappingCam.VMPos_Y;
                    isRB_X = false;
                }
                else
                {
                    return;
                }

                this.VisionManager().StopGrab(ErrMappingManager.CurrentMappingCam.CamType);
                ImageBuffer img = null;
                this.VisionManager().GetCam(ErrMappingManager.CurrentMappingCam.CamType).GetCurImage(out img);
                RegisteImageBufferParam rparam = new RegisteImageBufferParam(ErrMappingManager.CurrentMappingCam.CamType,
                  (int)((VMSetupDisPlay.AssignedCamera.GetGrabSizeWidth() / 2) - (VMSetupDisPlay.TargetRectangleWidth / 2)),
                  (int)((VMSetupDisPlay.AssignedCamera.GetGrabSizeHeight() / 2) - (VMSetupDisPlay.TargetRectangleHeight / 2)),
                  (int)VMSetupDisPlay.TargetRectangleWidth, (int)VMSetupDisPlay.TargetRectangleHeight, pattenPath);
                rparam.ImageBuffer = img;
                this.VisionManager().SavePattern(rparam);

                double xPos = 0, yPos = 0;
                
                this.VisionManager().StartGrab(ErrMappingManager.CurrentMappingCam.CamType, this);

                await Task.Run(() =>
                {
                    PatterMatching(out xPos, out yPos, pattenPath);
                });

                vmPos.X.Value = xPos;
                vmPos.Y.Value = yPos;
                vmPos.Z.Value = this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Command;

                if (isRB_X)
                {
                    ErrMappingManager.CurrentMappingCam.RelPos_X = new CatCoordinates(ErrMappingManager.CurrentMappingCam.CamPos.X.Value - vmPos.X.Value, ErrMappingManager.CurrentMappingCam.CamPos.Y.Value - vmPos.Y.Value);
                }
                else
                {
                    ErrMappingManager.CurrentMappingCam.RelPos_Y = new CatCoordinates(ErrMappingManager.CurrentMappingCam.CamPos.X.Value - vmPos.X.Value, ErrMappingManager.CurrentMappingCam.CamPos.Y.Value - vmPos.Y.Value);
                }

                this.StageSupervisor().StageModuleState.VMViewMove(xPos, yPos, vmPos.Z.Value);
                ErrMappingManager.SaveErrorMappingParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async void FindPattern_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string pattenPath = "";
                CatCoordinates vmPos = null;
                //FocusParameter focusParam = new FocusParameter(this.WaferAligner().WADeviceFile.WaferFocusParam);
                //focusParam.FocusingCam = ErrMappingManager.CurrentMappingCam.CamType;

                bool isRB_X = false;
                FocusingParam.FocusingCam.Value = ErrMappingManager.CurrentMappingCam.CamType;
                EventCodeEnum ret = await Task.Run(() => FocusingModule.Focusing_Retry(FocusingParam, false, false, false, this));
                if (ret != EventCodeEnum.NONE)
                {
                    return;
                }

                if (RB_X.IsChecked.Value && ErrMappingManager.CurrentMappingCam.CamNumber <= 4)
                {
                    pattenPath = ErrMappingManager.ErrMappingParam.PMParam_X.PMParameter.ModelFilePath.Value;
                    vmPos = ErrMappingManager.CurrentMappingCam.VMPos_X;
                    isRB_X = true;
                }
                else if (RB_Y.IsChecked.Value)
                {
                    pattenPath = ErrMappingManager.ErrMappingParam.PMParam_Y.PMParameter.ModelFilePath.Value;
                    vmPos = ErrMappingManager.CurrentMappingCam.VMPos_Y;
                    isRB_X = false;
                }
                else
                {
                    return;
                }

                double xPos = 0, yPos = 0;
                await Task.Run(() =>
                PatterMatching(out xPos, out yPos, pattenPath)
                );
                vmPos.X.Value = xPos;
                vmPos.Y.Value = yPos;
                vmPos.Z.Value = this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Command;
                if (isRB_X)
                {
                    ErrMappingManager.CurrentMappingCam.RelPos_X = new CatCoordinates(ErrMappingManager.CurrentMappingCam.CamPos.X.Value - vmPos.X.Value, ErrMappingManager.CurrentMappingCam.CamPos.Y.Value - vmPos.Y.Value);
                }
                else
                {
                    ErrMappingManager.CurrentMappingCam.RelPos_Y = new CatCoordinates(ErrMappingManager.CurrentMappingCam.CamPos.X.Value - vmPos.X.Value, ErrMappingManager.CurrentMappingCam.CamPos.Y.Value - vmPos.Y.Value);
                }
                this.StageSupervisor().StageModuleState.VMViewMove(xPos, yPos, vmPos.Z.Value);
                ErrMappingManager.SaveErrorMappingParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private int PatterMatching(out double ptPosX, out double ptPosY, string filePath, EnumProberCam camtype = EnumProberCam.INVALID)
        {
            int retVal = 0;
            ptPosX = 0;
            ptPosY = 0;
            try
            {

                var camType = ErrMappingManager.CurrentMappingCam.CamType;
                if (camtype != EnumProberCam.INVALID)
                {
                    camType = camtype;
                }
                this.VisionManager().StopGrab(camType);
                ImageBuffer img = null;

                this.VisionManager().GetCam(camType).GetCurImage(out img);

                PMResult pmresult = this.VisionManager().PatternMatching(new PatternInfomation(camType, new PMParameter(75, 98), filePath), this);

                this.VisionManager().StartGrab(camType, this);

                if (pmresult.ResultParam.Count == 1)
                {
                    double machinex = this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Actual;
                    double machiney = this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Actual;

                    double ptxpos = pmresult.ResultParam[0].XPoss;
                    double ptypos = pmresult.ResultParam[0].YPoss;

                    double offsetx = (pmresult.ResultBuffer.SizeX / 2) - ptxpos;
                    //ptPosX = machinex + (offsetx * VMSetupDisPlay.AssignedCamera.GetRatioX());
                    ptPosX = machinex + (offsetx * this.VisionManager().GetCam(camType).GetRatioX());

                    double offsety = (pmresult.ResultBuffer.SizeY / 2) - ptypos;
                    //ptPosY = machiney - (offsety * VMSetupDisPlay.AssignedCamera.GetRatioY());
                    ptPosY = machiney - (offsety * this.VisionManager().GetCam(camType).GetRatioY());


                    return 1;
                }
                else
                {
                    retVal = -1;
                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
                return retVal;
            }
        }
        private int PatterMatchingOffset(out double ptOffsetPosX, out double ptOffsetPosY, string pattenPath)
        {
            int retVal = 0;
            ptOffsetPosX = 0;
            ptOffsetPosY = 0;
            try
            {
                var camType = ErrMappingManager.CurrentMappingCam.CamType;
                
                this.VisionManager().StopGrab(camType);
                PMResult pmresult = this.VisionManager().PatternMatching(new PatternInfomation(camType, new PMParameter(), pattenPath), this);

                this.VisionManager().StartGrab(camType, this);

                if (pmresult.ResultParam.Count == 1)
                {

                    double ptxpos = pmresult.ResultParam[0].XPoss;
                    double ptypos = pmresult.ResultParam[0].YPoss;

                    double offsetx = (pmresult.ResultBuffer.SizeX / 2) - ptxpos;
                    ptOffsetPosX = -1 * (offsetx * this.VisionManager().GetCam(camType).GetRatioX());

                    double offsety = (pmresult.ResultBuffer.SizeY / 2) - ptypos;
                    ptOffsetPosY = (offsety * this.VisionManager().GetCam(camType).GetRatioY());

                    return 1;
                }
                else
                {
                    retVal = -1;
                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
                return retVal;
            }
        }

        private void RectWidthPlus_Click(object sender, RoutedEventArgs e)
        {
            RectWidth += 10;
        }

        private void RectWidthMinus_Click(object sender, RoutedEventArgs e)
        {
            RectWidth -= 10;
        }

        private void RectHeightPlus_Click(object sender, RoutedEventArgs e)
        {
            RectHeight += 10;
        }

        private void RectHeightMinus_Click(object sender, RoutedEventArgs e)
        {
            RectHeight -= 10;
        }

        private async void MeasureData_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int retVal = 0;
                double dir = 1;
                double offsetX = 0, offsetY = 0, posX = 0, posY = 0, posZ = 0;
                int FocusingCnt = 0;
                isStopFlag = false;
                bool compState = this.MotionManager().ErrorManager.CompensationModule.Enable1D;
                if (RB_X.IsChecked.Value)
                {
                    if (RB_All.IsChecked.Value)
                    {
                        ButtonColorChange(preCamIndex, Brushes.Green);
                        for (int i = 0; i < ErrMappingManager.ErrorDataXTable.Count; i++)
                        {
                            if (ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].RelPos_X.GetX() == 0 && ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].RelPos_X.GetY() == 0)
                            {
                                await this.MetroDialogManager().ShowMessageDialog("VM Error", $"VM{preCamIndex + 1} RelPos X,Y= 0 . Please register again", EnumMessageStyle.Affirmative);
                                return;
                            }
                            ButtonColorChange(i, Brushes.Blue);
                            ErrMappingManager.TransferCurrentCam(i);
                            ErrMappingManager.CurrentMoveToVMPos(0);
                            FocusingCnt = 0;
                            ErrMappingManager.ErrorDataXTable[i].ErrorData_HOR.Clear();
                            ErrMappingManager.ErrorDataXTable[i].ErrorData_VER.Clear();

                            while (true)
                            {
                                //if (FocusingCnt % 10 == 0)
                                //{
                                if (isStopFlag)
                                {
                                    return;
                                }
                                
                                FocusingParam.FocusingCam.Value = ErrMappingManager.CurrentMappingCam.CamType;
                                
                                EventCodeEnum retvalue = await Task.Run(() => this.FocusingModule.Focusing(FocusingParam, this));

                                if (retvalue != EventCodeEnum.NONE)
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("VM Focusing", $"VM{preCamIndex + 1} Focusing Fail", EnumMessageStyle.Affirmative);
                                    // return;
                                }

                                //}
                                await Task.Run(() =>
                                retVal = PatterMatchingOffset(out offsetX, out offsetY, ErrMappingManager.ErrMappingParam.PMParam_X.PMParameter.ModelFilePath.Value)
                                );
                                if (retVal == -1)
                                {
                                    if(ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinXCnt> ErrMappingManager.ErrorDataXTable[i].ErrorData_HOR.Count)
                                    {
                                        await this.MetroDialogManager().ShowMessageDialog("PatterMatching Error", $"ErrorXTable Count:{ErrMappingManager.ErrorDataXTable[i].ErrorData_HOR.Count} , ErrorXTable Minimum Count:{ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinXCnt}", EnumMessageStyle.Affirmative);
                                        return;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref posX);
                                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref posY);
                                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref posZ);
                                ErrMappingManager.ErrorDataXTable[i].ErrorData_HOR.Add(new ErrorData(offsetX, posX, posY, posZ, ErrMappingManager.CurrentCamPosX - posX, ErrMappingManager.CurrentCamPosY - posY));
                                ErrMappingManager.ErrorDataXTable[i].ErrorData_VER.Add(new ErrorData(offsetY, posX, posY, posZ, ErrMappingManager.CurrentCamPosX - posX, ErrMappingManager.CurrentCamPosY - posY));
                                if (i % 2 == 0)
                                {
                                    dir = -1;
                                }
                                else
                                {
                                    dir = 1;
                                }

                                EventCodeEnum ret = await Task.Run(() =>
                                  this.MotionManager().RelMove(EnumAxisConstants.X, dir * ErrMappingManager.ErrMappingParam.DieSizeX)
                               );

                                if (ret != EventCodeEnum.NONE) break;
                                System.Threading.Thread.Sleep(1200);

                                FocusingCnt++;
                            }
                            ErrMappingManager.SaveMesureErrorData(0, i);
                            ButtonColorChange(i, Brushes.Green);
                        }
                    }
                    else if (RB_Single.IsChecked.Value)
                    {
                        if (preCamIndex < ErrMappingManager.ErrorDataXTable.Count)
                        {
                            if (ErrMappingManager.ErrMappingParam.ErrorMappingCameras[preCamIndex].RelPos_X.GetX() == 0 && ErrMappingManager.ErrMappingParam.ErrorMappingCameras[preCamIndex].RelPos_X.GetY() == 0)
                            {
                                // error 메세지 박스 출력
                                await this.MetroDialogManager().ShowMessageDialog("VM Error", $"VM{preCamIndex + 1} RelPos X,Y= 0 . Please register again", EnumMessageStyle.Affirmative);
                                return;
                            }
                            ErrMappingManager.TransferCurrentCam(preCamIndex);
                            ErrMappingManager.CurrentMoveToVMPos(0);
                            FocusingCnt = 0;
                            ErrMappingManager.ErrorDataXTable[preCamIndex].ErrorData_HOR.Clear();
                            ErrMappingManager.ErrorDataXTable[preCamIndex].ErrorData_VER.Clear();
                            while (true)
                            {
                                //if (FocusingCnt % 10 == 0)
                                //{
                                if (isStopFlag)
                                {
                                    return;
                                }
                                FocusingParam.FocusingCam.Value = ErrMappingManager.CurrentMappingCam.CamType;
                                EventCodeEnum retvalue = await Task.Run(() => this.FocusingModule.Focusing(FocusingParam, this));
                                if (retvalue != EventCodeEnum.NONE)
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("VM Focusing", $"VM{preCamIndex + 1} Focusing Fail", EnumMessageStyle.Affirmative);
                                    // return;
                                }

                                //}
                                await Task.Run(() =>
                                retVal = PatterMatchingOffset(out offsetX, out offsetY, ErrMappingManager.ErrMappingParam.PMParam_X.PMParameter.ModelFilePath.Value)
                                );
                                if (retVal == -1)
                                {
                                    if (ErrMappingManager.ErrMappingParam.ErrorMappingCameras[preCamIndex].MinXCnt > ErrMappingManager.ErrorDataXTable[preCamIndex].ErrorData_HOR.Count)
                                    {
                                        await this.MetroDialogManager().ShowMessageDialog("PatterMatching Error", $"ErrorXTable Count:{ErrMappingManager.ErrorDataXTable[preCamIndex].ErrorData_HOR.Count} , ErrorXTable Minimum Count:{ErrMappingManager.ErrMappingParam.ErrorMappingCameras[preCamIndex].MinXCnt}", EnumMessageStyle.Affirmative);
                                        return;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref posX);
                                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref posY);
                                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref posZ);
                                ErrMappingManager.ErrorDataXTable[preCamIndex].ErrorData_HOR.Add(new ErrorData(offsetX, posX, posY, posZ, ErrMappingManager.CurrentCamPosX - posX, ErrMappingManager.CurrentCamPosY - posY));
                                ErrMappingManager.ErrorDataXTable[preCamIndex].ErrorData_VER.Add(new ErrorData(offsetY, posX, posY, posZ, ErrMappingManager.CurrentCamPosX - posX, ErrMappingManager.CurrentCamPosY - posY));
                                if (preCamIndex % 2 == 0)
                                {
                                    dir = -1;
                                }
                                else
                                {
                                    dir = 1;
                                }

                                EventCodeEnum ret = await Task.Run(() =>
                                  this.MotionManager().RelMove(EnumAxisConstants.X, dir * ErrMappingManager.ErrMappingParam.DieSizeX)
                               );

                                if (ret != EventCodeEnum.NONE) break;
                                System.Threading.Thread.Sleep(1200);

                                FocusingCnt++;
                            }
                            ErrMappingManager.SaveMesureErrorData(0, preCamIndex);
                            ButtonColorChange(preCamIndex, Brushes.Green);

                        }
                        else
                        {
                            await this.MetroDialogManager().ShowMessageDialog("VM Camera Not Used Error", $"X Axis cannot be measured with VM{preCamIndex + 1}", EnumMessageStyle.Affirmative);
                            // error 메세지 박스 출력
                        }
                    }

                }
                else if (RB_Y.IsChecked.Value)
                {
                    if (RB_All.IsChecked.Value)
                    {
                        ButtonColorChange(preCamIndex, Brushes.Green);
                        for (int i = 0; i < ErrMappingManager.ErrorDataYTable.Count; i++)
                        {
                            System.Threading.Thread.Sleep(1200);
                            if (ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].RelPos_Y.GetX() == 0 && ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].RelPos_Y.GetY() == 0)
                            {
                                await this.MetroDialogManager().ShowMessageDialog("VM Error", $"VM{preCamIndex + 1} RelPos X,Y= 0 . Please register again", EnumMessageStyle.Affirmative);
                                return;
                            }
                            ButtonColorChange(i, Brushes.Blue);
                            ErrMappingManager.TransferCurrentCam(i);
                            ErrMappingManager.CurrentMoveToVMPos(1);
                            FocusingCnt = 0;
                            ErrMappingManager.ErrorDataYTable[i].ErrorData_HOR.Clear();
                            ErrMappingManager.ErrorDataYTable[i].ErrorData_VER.Clear();
                            while (true)
                            {
                                if (isStopFlag)
                                {
                                    return;
                                }
                                FocusingParam.FocusingCam.Value = ErrMappingManager.CurrentMappingCam.CamType;
                                //if (FocusingCnt!=0&&FocusingCnt % 10 == 0)
                                //{
                                EventCodeEnum retvalue = await Task.Run(() => this.FocusingModule.Focusing(FocusingParam, this));
                                if (retvalue != EventCodeEnum.NONE)
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("VM Focusing", $"VM{preCamIndex + 1} Focusing Fail", EnumMessageStyle.Affirmative);
                                    //return;
                                }
                                //}
                                await Task.Run(() =>
                                retVal = PatterMatchingOffset(out offsetX, out offsetY, ErrMappingManager.ErrMappingParam.PMParam_Y.PMParameter.ModelFilePath.Value)
                                );
                                if (retVal == -1)
                                {
                                    if (ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinYCnt > ErrMappingManager.ErrorDataYTable[i].ErrorData_HOR.Count)
                                    {
                                        await this.MetroDialogManager().ShowMessageDialog("PatterMatching Error", $"ErrorYTable Count:{ErrMappingManager.ErrorDataYTable[i].ErrorData_HOR.Count} , ErrorYTable Minimum Count:{ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinXCnt}", EnumMessageStyle.Affirmative);
                                        return;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref posX);
                                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref posY);
                                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref posZ);
                                ErrMappingManager.ErrorDataYTable[i].ErrorData_HOR.Add(new ErrorData(offsetX, posX, posY, posZ, ErrMappingManager.CurrentCamPosX - posX, ErrMappingManager.CurrentCamPosY - posY));
                                ErrMappingManager.ErrorDataYTable[i].ErrorData_VER.Add(new ErrorData(offsetY, posX, posY, posZ, ErrMappingManager.CurrentCamPosX - posX, ErrMappingManager.CurrentCamPosY - posY));

                                EventCodeEnum ret = await Task.Run(() =>
                                  this.MotionManager().RelMove(EnumAxisConstants.Y, ErrMappingManager.ErrMappingParam.DieSizeY)
                               );

                                if (ret != EventCodeEnum.NONE) break;
                                System.Threading.Thread.Sleep(2000);

                                FocusingCnt++;
                            }
                            ErrMappingManager.SaveMesureErrorData(1, i);
                            ButtonColorChange(i, Brushes.Green);
                        }
                    }
                    else if (RB_Single.IsChecked.Value)
                    {
                        if (preCamIndex < ErrMappingManager.ErrorDataYTable.Count)
                        {
                            if (ErrMappingManager.ErrMappingParam.ErrorMappingCameras[preCamIndex].RelPos_Y.GetX() == 0 && ErrMappingManager.ErrMappingParam.ErrorMappingCameras[preCamIndex].RelPos_Y.GetY() == 0)
                            {
                                // error 메세지 박스 출력
                                return;
                            }
                            ErrMappingManager.TransferCurrentCam(preCamIndex);
                            ErrMappingManager.CurrentMoveToVMPos(1);
                            FocusingCnt = 0;
                            ErrMappingManager.ErrorDataYTable[preCamIndex].ErrorData_HOR.Clear();
                            ErrMappingManager.ErrorDataYTable[preCamIndex].ErrorData_VER.Clear();
                            while (true)
                            {
                                if (isStopFlag)
                                {
                                    return;
                                }
                                FocusingParam.FocusingCam.Value = ErrMappingManager.CurrentMappingCam.CamType;
                                //if (FocusingCnt!=0&&FocusingCnt % 10 == 0)
                                //{
                                EventCodeEnum retvalue = await Task.Run(() => this.FocusingModule.Focusing(FocusingParam, this));
                                if (retvalue != EventCodeEnum.NONE)
                                {
                                    LoggerManager.Debug($"Focusing failed. Curr. camera = {FocusingParam.FocusingCam.Value}");
                                    //return;
                                }
                                //}
                                await Task.Run(() =>
                                retVal = PatterMatchingOffset(out offsetX, out offsetY, ErrMappingManager.ErrMappingParam.PMParam_Y.PMParameter.ModelFilePath.Value)
                                );
                                if (retVal == -1)
                                {
                                    if (ErrMappingManager.ErrMappingParam.ErrorMappingCameras[preCamIndex].MinYCnt > ErrMappingManager.ErrorDataYTable[preCamIndex].ErrorData_HOR.Count)
                                    {
                                        await this.MetroDialogManager().ShowMessageDialog("PatterMatching Error", $"ErrorYTable Count:{ErrMappingManager.ErrorDataYTable[preCamIndex].ErrorData_HOR.Count} , ErrorYTable Minimum Count:{ErrMappingManager.ErrMappingParam.ErrorMappingCameras[preCamIndex].MinXCnt}", EnumMessageStyle.Affirmative);
                                        return;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref posX);
                                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref posY);
                                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref posZ);
                                ErrMappingManager.ErrorDataYTable[preCamIndex].ErrorData_HOR.Add(new ErrorData(offsetX, posX, posY, posZ, ErrMappingManager.CurrentCamPosX - posX, ErrMappingManager.CurrentCamPosY - posY));
                                ErrMappingManager.ErrorDataYTable[preCamIndex].ErrorData_VER.Add(new ErrorData(offsetY, posX, posY, posZ, ErrMappingManager.CurrentCamPosX - posX, ErrMappingManager.CurrentCamPosY - posY));

                                EventCodeEnum ret = await Task.Run(() =>
                                  this.MotionManager().RelMove(EnumAxisConstants.Y, ErrMappingManager.ErrMappingParam.DieSizeY)
                               );

                                if (ret != EventCodeEnum.NONE) break;
                                System.Threading.Thread.Sleep(2000);

                                FocusingCnt++;
                            }
                            ErrMappingManager.SaveMesureErrorData(1, preCamIndex);
                            ButtonColorChange(preCamIndex, Brushes.Green);
                        }
                        else
                        {
                            await this.MetroDialogManager().ShowMessageDialog("VM Camera Not Used Error", $"Y Axis cannot be measured with VM{preCamIndex + 1}", EnumMessageStyle.Affirmative);
                        }
                    }
                }
                else if (RB_Single.IsChecked.Value)
                {
                    if (ErrMappingManager.CurrentMappingCam.RelPos_Y.GetX() == 0 && ErrMappingManager.CurrentMappingCam.RelPos_Y.GetY() == 0)
                    {
                        // error 메세지 박스 출력
                    }
                    else if (ErrMappingManager.CurrentMappingCam.RelPos_Y.GetX() == 0 && ErrMappingManager.CurrentMappingCam.RelPos_Y.GetY() == 0)
                    {
                        // error 메세지 박스 출력
                    }
                    else
                    {
                        ErrMappingManager.CurrentMoveToVMPos(0);
                        FocusingCnt = 0;
                        ErrMappingManager.CurrentMappingCam.ErrorDataX.ErrorData_HOR.Clear();
                        ErrMappingManager.CurrentMappingCam.ErrorDataX.ErrorData_VER.Clear();
                        while (true)
                        {
                            //if (FocusingCnt % 10 == 0)
                            //{
                            //    FocusingModule.FocusParameter.FocusingCam.Value = ErrMappingManager.CurrentMappingCam.CamType;
                            //    EventCodeEnum retvalue = await Task.Run(() => FocusingModule.Focusing_Retry(false, false, false));
                            //    if (retvalue == EventCodeEnum.NONE)
                            //    {
                            //        return;
                            //    }
                            //}
                            await Task.Run(() =>
                           retVal = PatterMatchingOffset(out offsetX, out offsetY, ErrMappingManager.ErrMappingParam.PMParam_X.PMParameter.ModelFilePath.Value)
                           );
                            if (retVal == -1)
                            {
                                break;
                            }
                            this.MotionManager().GetActualPos(EnumAxisConstants.X, ref posX);
                            this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref posY);
                            this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref posZ);
                            ErrMappingManager.CurrentMappingCam.ErrorDataX.ErrorData_HOR.Add(new ErrorData(Math.Round(offsetX, 3), Math.Round(posX, 3), Math.Round(posY, 3), Math.Round(posZ, 3), ErrMappingManager.CurrentCamPosX - posX, ErrMappingManager.CurrentCamPosY - posY));
                            ErrMappingManager.CurrentMappingCam.ErrorDataX.ErrorData_VER.Add(new ErrorData(Math.Round(offsetY, 3), Math.Round(posX, 3), Math.Round(posY, 3), Math.Round(posZ, 3), ErrMappingManager.CurrentCamPosX - posX, ErrMappingManager.CurrentCamPosY - posY));
                            if (ErrMappingManager.CurCamIndex % 2 == 0)
                            {
                                dir = -1;
                            }
                            else
                            {
                                dir = 1;
                            }
                            EventCodeEnum ret = await Task.Run(() =>
                              this.MotionManager().RelMove(EnumAxisConstants.X, dir * ErrMappingManager.ErrMappingParam.DieSizeX)
                           );

                            if (ret != EventCodeEnum.NONE) break;
                            System.Threading.Thread.Sleep(100);

                            FocusingCnt++;
                        }
                        ErrMappingManager.CurrentMoveToVMPos(1);
                        FocusingCnt = 0;
                        ErrMappingManager.CurrentMappingCam.ErrorDataY.ErrorData_HOR.Clear();
                        ErrMappingManager.CurrentMappingCam.ErrorDataY.ErrorData_VER.Clear();
                        while (true)
                        {
                            if (FocusingCnt % 10 == 0)
                            {
                                FocusingParam.FocusingCam.Value = ErrMappingManager.CurrentMappingCam.CamType;
                                EventCodeEnum retvalue = await Task.Run(() => FocusingModule.Focusing_Retry(FocusingParam, false, false, false, this));

                                if (retvalue == EventCodeEnum.NONE)
                                {
                                    return;
                                }
                            }
                            await Task.Run(() =>
                            retVal = PatterMatchingOffset(out offsetX, out offsetY, ErrMappingManager.ErrMappingParam.PMParam_Y.PMParameter.ModelFilePath.Value)
                            );
                            if (retVal == -1)
                            {
                                break;
                            }
                            this.MotionManager().GetActualPos(EnumAxisConstants.X, ref posX);
                            this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref posY);
                            this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref posZ);
                            ErrMappingManager.CurrentMappingCam.ErrorDataY.ErrorData_HOR.Add(new ErrorData(Math.Round(offsetX, 3), Math.Round(posX, 3), Math.Round(posY, 3), Math.Round(posZ, 3), ErrMappingManager.CurrentCamPosX - posX, ErrMappingManager.CurrentCamPosY - posY));
                            ErrMappingManager.CurrentMappingCam.ErrorDataY.ErrorData_VER.Add(new ErrorData(Math.Round(offsetY, 3), Math.Round(posX, 3), Math.Round(posY, 3), Math.Round(posZ, 3), ErrMappingManager.CurrentCamPosX - posX, ErrMappingManager.CurrentCamPosY - posY));

                            EventCodeEnum ret = await Task.Run(() =>
                              this.MotionManager().RelMove(EnumAxisConstants.Y, ErrMappingManager.ErrMappingParam.DieSizeY)
                           );

                            if (ret != EventCodeEnum.NONE) break;
                            System.Threading.Thread.Sleep(100);

                            FocusingCnt++;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void ButtonColorChange(int index, SolidColorBrush color)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        VisionCam1_Btn.Background = color;
                        break;
                    case 1:
                        VisionCam2_Btn.Background = color;
                        break;
                    case 2:
                        VisionCam3_Btn.Background = color;
                        break;
                    case 3:
                        VisionCam4_Btn.Background = color;
                        break;
                    case 4:
                        VisionCam5_Btn.Background = color;
                        break;
                    case 5:
                        VisionCam6_Btn.Background = color;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }

        private void MakeErrData_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (RB_X.IsChecked.Value)
                {
                    ErrMappingManager.MakeErrorDataX();
                }
                else if (RB_Y.IsChecked.Value)
                {
                    ErrMappingManager.MakeOPUSVErrorDataY();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void GraphData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MahApps.Metro.Controls.MetroWindow graphX = null;

                if (graphX != null)
                {
                    graphX.Activate();
                    return;
                }

                graphX = new ErrorDataGraph(ErrMappingManager.ErrorCompTable);
                // graphX.Owner = Model.ProberMain;
                graphX.Closed += (o, args) => graphX = null;
                graphX.Show();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void GraphSingleData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MahApps.Metro.Controls.MetroWindow graphSingle = null;

                if (graphSingle != null)
                {
                    graphSingle.Activate();
                    return;
                }

                graphSingle = new ErrorDataSingleGraph(ErrMappingManager.CurCamIndex, ErrMappingManager.CurrentMappingCam.ErrorDataX, ErrMappingManager.CurrentMappingCam.ErrorDataY);
                // graphSingle.Owner = Model.ProberMain;
                graphSingle.Closed += (o, args) => graphSingle = null;
                graphSingle.Show();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private void MarkAlgin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.MarkAligner().Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                VisionManager = this.VisionManager();
                MotionManager = this.MotionManager();
                StageSupervisor = this.StageSupervisor();

                FirstErrorComp_CB.IsChecked = this.MotionManager().ErrorManager.CompensationModule.Enable1D;
                SecondErrorComp_CB.IsChecked = this.MotionManager().ErrorManager.CompensationModule.Enable2D;
                

                ErrMappingManager = new ErrorMappingManager();
                ErrMappingManager.InitModule();
                VisionManager = this.VisionManager();
                MotionManager = this.MotionManager();
                StageSupervisor = this.StageSupervisor();

                //FirstErrorComp_CB.IsChecked = this.MotionManager().ErrorManager.CompensationModule.Enable1D;
                //SecondErrorComp_CB.IsChecked = this.MotionManager().ErrorManager.CompensationModule.Enable2D;

                FocusingParam = new NormalFocusParameter();
                FocusingParam.SetDefaultParam();
                FocusingParam.FocusRange.Value = 80;
                //ThetaAligner = new VMThetaAligner();
                //ThetaAligner.InitModule();
                Wafer = (WaferObject)this.StageSupervisor().WaferObject;

                //VMSetupDisPlay.StageSuperVisor = this.StageSupervisor();

                DieSizeX_TB.Text = ErrMappingManager.ErrMappingParam.DieSizeX.ToString();
                DieSizeY_TB.Text = ErrMappingManager.ErrMappingParam.DieSizeY.ToString();
                DieSizeX_LB.Content = ErrMappingManager.ErrMappingParam.DieSizeX.ToString();
                DieSizeY_LB.Content = ErrMappingManager.ErrMappingParam.DieSizeY.ToString();
                //var axisT = this.MotionManager().GetAxis(EnumAxisConstants.C);
                //    MotionManager.AbsMove(axisT, -35953.2, axisT.Param.Speed.Value, axisT.Param.Acceleration.Value);
                //this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
                //this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, true);
                //this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, true);

                if (this.MotionManager().ErrorManager.CompensationModule.Enable1D == true)
                {
                    FirstErrorComp_CB.IsChecked = true;
                }
                else
                {
                    FirstErrorComp_CB.IsChecked = false;
                }
                //IOManager.IOServ.IOList[0].WriteBit(4, 0, true);
                //IOManager.IOServ.IOList[0].WriteBit(4, 0, true);
                //DataContext = this;


                RectWidth = 360;
                RectHeight = 360;

                bool waf = false;
                this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK, out waf);
                LockWafer.IsChecked = waf;
                PZValue = this.MotionManager().GetAxis(EnumAxisConstants.PZ).Status.Position.Command;

                Initialized = true;
                MachineSquareness.Text = this.CoordinateManager().StageCoord.MachineSequareness.ToString();
                
                bool isSave = false;
                for (int i = 0; i < ErrMappingManager.ErrMappingParam.ErrorMappingCameras.Count; i++)
                {
                    if (ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinXCnt == 0)
                    {
                        isSave = true;
                        switch (i)
                        {
                            case 0:
                                ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinXCnt = 24;
                                break;
                            case 1:
                                ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinXCnt = 24;
                                break;
                            case 2:
                                ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinXCnt = 24;
                                break;
                            case 3:
                                ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinXCnt = 24;
                                break;
                            case 4:
                                ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinXCnt = -1;
                                break;
                            case 5:
                                ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinXCnt = -1;
                                break;
                            default:
                                break;
                        }
                    }

                    if (ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinYCnt == 0)
                    {
                        isSave = true;
                        switch (i)
                        {
                            case 0:
                                ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinYCnt = 30;
                                break;
                            case 1:
                                ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinYCnt = 30;
                                break;
                            case 2:
                                ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinYCnt = 30;
                                break;
                            case 3:
                                ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinYCnt = 30;
                                break;
                            case 4:
                                ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinYCnt = 20;
                                break;
                            case 5:
                                ErrMappingManager.ErrMappingParam.ErrorMappingCameras[i].MinYCnt = 20;
                                break;
                            default:
                                break;
                        }
                    }
                }
                if(isSave)
                {
                    ErrMappingManager.SaveErrorMappingParameter();
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                //this.SysState().SetSetUpState();
                this.VisionManager().SetDisplayChannelStageCameras(VMSetupDisPlay);
                this.StageSupervisor().StageModuleState.ZCLEARED();
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, true);
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, true);
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    LockWafer.IsChecked = true;

                }));
                
                AssignedCamera = this.VisionManager().GetCam(EnumProberCam.MAP_REF_CAM);

                //this.VisionManager().StopGrab(EnumProberCam.WAFER_LOW_CAM);
                EventCodeEnum ret = this.VisionManager().StartGrab(EnumProberCam.MAP_1_CAM, this);

                foreach (var cam in this.VisionManager().CameraDescriptor.Cams)
                {
                    this.VisionManager().SetDisplayChannel(cam, VMSetupDisPlay);
                }
                bool waf = false;
                this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK, out waf);
                //LockWafer.IsChecked = waf;
                PZValue = this.MotionManager().GetAxis(EnumAxisConstants.PZ).Status.Position.Command;
                //FirstErrorComp_CB.IsChecked = this.Motion.ErrorManager.CompensationModule.Enable1D;
                //SecondErrorComp_CB.IsChecked = this.Motion.ErrorManager.CompensationModule.Enable2D;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retval);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            
            try
            {

                Task<EventCodeEnum> task = new Task<EventCodeEnum>(() =>
                {
                    var ret = this.StageSupervisor().StageModuleState.ZCLEARED();
                    return ret;
                });
                task.Start();
                return task;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Debug($"Cleanup(): Error occurred while Z Clear.");
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.STAGEMOVE_ZCLEARD_MOVE_ERROR);
            }


            //return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            try
            {

                //throw new NotImplementedException();
                bool waf = false;

                Task<EventCodeEnum> t = Task.Run<EventCodeEnum>(() =>
                {
                    this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK, out waf);
                    LockWafer.IsChecked = waf;
                    return EventCodeEnum.NONE;
                });
                return t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.UNDEFINED);
            }

        }

        private void LockWafer_IsCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (LockWafer.IsChecked == true)
                {
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, true);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, true);
                    LoggerManager.Debug("Chuck Vac On");
                }
                else
                {
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, false);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, false);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, false);
                    LoggerManager.Debug("Chuck Vac Off");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void VisionCamRef_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VisionManager.StopGrab(EnumProberCam.MAP_1_CAM);
                VisionManager.StartGrab(EnumProberCam.MAP_REF_CAM, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void PZExecute_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Random r = new Random();
                double xPos, yPos;
                string pattenPath = pattern.PMParameter.ModelFilePath.Value;

                List<Point> data = new List<Point>();
                //int pz=
                double movez = this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Command;
                double movepz = -89000;
                for (int i = 0; i < 100; i++)
                {
                    int movex = r.Next(-130000, 130000);
                    int movey = r.Next(-190000, 300000);
                    this.MotionManager().AbsMove(this.MotionManager().GetAxis(EnumAxisConstants.PZ), movepz);
                    this.StageSupervisor().StageModuleState.VMViewMove(movex, movey, movez);
                    PZMovePos_Click(sender, e);
                    PatterMatching(out xPos, out yPos, pattenPath, EnumProberCam.MAP_1_CAM);
                    double machinex = this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Actual;
                    double machiney = this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Actual;
                    data.Add(new Point(xPos - machinex, machiney - yPos));

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private void PZUp_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        double posZ = Double.Parse(txt_PZ.Text);
        //        this.StageSupervisor().StageModuleState.VMRelMove(this.MotionManager().GetAxis(EnumAxisConstants.PZ), posZ);
        //        PZValue = this.MotionManager().GetAxis(EnumAxisConstants.PZ).Status.Position.Command;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //private void PZDown_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        double posZ = Double.Parse(txt_PZ.Text);
        //        this.StageSupervisor().StageModuleState.VMRelMove(this.MotionManager().GetAxis(EnumAxisConstants.PZ), -posZ);
        //        PZValue = this.MotionManager().GetAxis(EnumAxisConstants.PZ).Status.Position.Command;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private void PZMovePos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //this.VisionManager().StartGrab(EnumProberCam.MAP_1_CAM);
                double zpos = this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Command;
                this.StageSupervisor().StageModuleState.VMViewMove(tmp_x, tmp_y, zpos);
                this.MotionManager().AbsMove(this.MotionManager().GetAxis(EnumAxisConstants.PZ), tmp_pz);
                PZValue = this.MotionManager().GetAxis(EnumAxisConstants.PZ).Status.Position.Command;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private PatternInfomation pattern = new PatternInfomation(@"C:\ProberSystem\Default\Parameters\SystemParam\VisionMapping\Pattern\Pattern");
        private double tmp_x = -25944.6;
        private double tmp_y = 143683.8;
        private double tmp_pz = -19649;

        //private double tmp_SafePZ = -80649;
        private void PZPattern_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string pattenPath = pattern.PMParameter.ModelFilePath.Value;
                this.VisionManager().StopGrab(EnumProberCam.MAP_1_CAM);
                ImageBuffer img = null;

                this.VisionManager().GetCam(EnumProberCam.MAP_1_CAM).GetCurImage(out img);
                RegisteImageBufferParam rparam = new RegisteImageBufferParam(ErrMappingManager.CurrentMappingCam.CamType,
                  (int)((VMSetupDisPlay.AssignedCamera.GetGrabSizeWidth() / 2) - (VMSetupDisPlay.TargetRectangleWidth / 2)),
                  (int)((VMSetupDisPlay.AssignedCamera.GetGrabSizeHeight() / 2) - (VMSetupDisPlay.TargetRectangleHeight / 2)),
                  (int)VMSetupDisPlay.TargetRectangleWidth, (int)VMSetupDisPlay.TargetRectangleHeight, pattenPath);
                rparam.ImageBuffer = img;

                this.VisionManager().SavePattern(rparam);

                double xPos, yPos;
                
                this.VisionManager().StartGrab(EnumProberCam.MAP_1_CAM, this);
                
                PatterMatching(out xPos, out yPos, pattenPath, EnumProberCam.MAP_1_CAM);
                tmp_x = xPos;
                tmp_y = yPos;
                tmp_pz = this.MotionManager().GetAxis(EnumAxisConstants.PZ).Status.Position.Command;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void ParamReload_Click(object sender, RoutedEventArgs e)
        {
            (this.MotionManager as MotionManager).ErrorManager.CompensationModule.ForcedLoadFirstErrorTable();
        }

        private void ThetaAlign_Btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void T_DieSizeSet_Btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Theta_RegPattern_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Theta_FindPattern_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SquarenessSave_Click(object sender, RoutedEventArgs e)
        {
            double Squareness = Double.Parse(MachineSquareness.Text);
            this.CoordinateManager().StageCoord.MachineSequareness.Value = Squareness;
            this.CoordinateManager().SaveSysParameter();
        }

        private void x_1um_Click(object sender, RoutedEventArgs e)
        {
            txt_x.Text = "1";
        }

        private void x_10um_Click(object sender, RoutedEventArgs e)
        {
            txt_x.Text = "10";
        }

        private void x_100um_Click(object sender, RoutedEventArgs e)
        {
            txt_x.Text = "100";
        }

        private void x_Die_Click(object sender, RoutedEventArgs e)
        {
            txt_x.Text = ErrMappingManager.ErrMappingParam.DieSizeX.ToString();
        }

        private void y_1um_Click(object sender, RoutedEventArgs e)
        {
            txt_y.Text = "1";
        }

        private void y_10um_Click(object sender, RoutedEventArgs e)
        {
            txt_y.Text = "10";
        }

        private void y_100um_Click(object sender, RoutedEventArgs e)
        {
            txt_y.Text = "100";
        }

        private void y_Die_Click(object sender, RoutedEventArgs e)
        {
            txt_y.Text = ErrMappingManager.ErrMappingParam.DieSizeY.ToString();
        }

        private void t_0001_Click(object sender, RoutedEventArgs e)
        {
            txt_T.Text = "0.001";
        }

        private void t_001_Click(object sender, RoutedEventArgs e)
        {
            txt_T.Text = "0.01";
        }

        private void t_01_Click(object sender, RoutedEventArgs e)
        {
            txt_T.Text = "0.1";
        }

        private void t_1_Click(object sender, RoutedEventArgs e)
        {
            txt_T.Text = "1";
        }

        private void z_1um_Click(object sender, RoutedEventArgs e)
        {
            txt_Z.Text = "1";
        }

        private void z_10um_Click(object sender, RoutedEventArgs e)
        {
            txt_Z.Text = "10";
        }

        private void z_100um_Click(object sender, RoutedEventArgs e)
        {
            txt_Z.Text = "100";
        }

        private void z_1000um_Click(object sender, RoutedEventArgs e)
        {
            txt_Z.Text = "1000";
        }

        private async void Stop_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ret = await this.MetroDialogManager().ShowMessageDialog("Mapping Measure Stop", $"Are you sure you want to stop?", EnumMessageStyle.AffirmativeAndNegative);
                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    isStopFlag = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
            }
        }

        //private void FirstErrorComp_CB_Checked(object sender, RoutedEventArgs e)
        //{
        //    this.MotionManager().ErrorManager.CompensationModule.Enable1D = true;
        //}

        //private void FirstErrorComp_CB_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    this.MotionManager().ErrorManager.CompensationModule.Enable1D = false;
        //}


        //private void SecondErrorComp_CB_Checked(object sender, RoutedEventArgs e)
        //{
        //    this.MotionManager().ErrorManager.CompensationModule.Enable2D = true;

        //}

        //private void SecondErrorComp_CB_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    this.MotionManager().ErrorManager.CompensationModule.Enable2D = false;
        //}


    }
}

