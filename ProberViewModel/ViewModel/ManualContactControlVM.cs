using System;
using System.ComponentModel;
using System.Threading.Tasks;
using ProberErrorCode;
using ProberInterfaces;
using System.Runtime.CompilerServices;
using RelayCommandBase;
using System.Windows.Input;
using ProberInterfaces.PnpSetup;
using LogModule;
using VirtualKeyboardControl;
using System.Windows;
using ProbingModule;
using MetroDialogInterfaces;
using ProberInterfaces.State;
using System.Threading;
using ProberInterfaces.Event;
using NotifyEventModule;

namespace ProberViewModel
{
    public class ManualContactControlVM : IMainScreenViewModel, IManualContactControlVM, ISetUpState
    {
        readonly Guid _ViewModelGUID = new Guid("48468e20-b3dc-4e45-b075-690056f566bf");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public IStageSupervisor StageSupervisor { get; set; }

        public bool Initialized { get; set; } = false;
        private PinPadMatchModule.PinPadMatchModule PTPAModule;
        public IProbingModule ProbingModule { get; set; }
        public IManualContact MCM { get; set; }

        private bool _CanUsingManualContactControl;
        public bool CanUsingManualContactControl
        {
            get { return _CanUsingManualContactControl; }
            set
            {
                if (value != _CanUsingManualContactControl)
                {
                    _CanUsingManualContactControl = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _WantToMoveZInterval = 10;
        public double WantToMoveZInterval
        {
            get { return _WantToMoveZInterval; }
            set
            {
                if (value != _WantToMoveZInterval)
                {
                    _WantToMoveZInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IForceMeasure _ZFM;
        public IForceMeasure ZFM
        {
            get { return _ZFM; }
            set
            {
                if (value != _ZFM)
                {
                    _ZFM = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ForceVal = 10;
        public double ForceVal
        {
            get { return _ForceVal; }
            set
            {
                if (value != _ForceVal)
                {
                    _ForceVal = value;
                    RaisePropertyChanged();
                }
            }
        }


        #region ==> PadJogLeft
        private PNPCommandButtonDescriptor _PadJogLeft = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogLeft
        {
            get { return _PadJogLeft; }
            set { _PadJogLeft = value; }
        }
        #endregion

        #region ==> PadJogRight
        private PNPCommandButtonDescriptor _PadJogRight = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogRight
        {
            get { return _PadJogRight; }
            set { _PadJogRight = value; }
        }
        #endregion

        #region ==> PadJogUp
        private PNPCommandButtonDescriptor _PadJogUp = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogUp
        {
            get { return _PadJogUp; }
            set { _PadJogUp = value; }
        }
        #endregion

        #region ==> PadJogDown
        private PNPCommandButtonDescriptor _PadJogDown = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogDown
        {
            get { return _PadJogDown; }
            set { _PadJogDown = value; }
        }
        #endregion

        #region ==> PadJogLeftUp
        private PNPCommandButtonDescriptor _PadJogLeftUp = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogLeftUp
        {
            get { return _PadJogLeftUp; }
            set { _PadJogLeftUp = value; }
        }
        #endregion

        #region ==> PadJogRightUp
        private PNPCommandButtonDescriptor _PadJogRightUp = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogRightUp
        {
            get { return _PadJogRightUp; }
            set { _PadJogRightUp = value; }
        }
        #endregion

        #region ==> PadJogLeftDown
        private PNPCommandButtonDescriptor _PadJogLeftDown = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogLeftDown
        {
            get { return _PadJogLeftDown; }
            set { _PadJogLeftDown = value; }
        }
        #endregion

        #region ==> PadJogSelect

        private PNPCommandButtonDescriptor _PadJogSelect = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogSelect
        {
            get { return _PadJogSelect; }
            set { _PadJogSelect = value; }
        }
        #endregion

        #region ==> PadJogRightDown
        private PNPCommandButtonDescriptor _PadJogRightDown = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogRightDown
        {
            get { return _PadJogRightDown; }
            set { _PadJogRightDown = value; }
        }
        #endregion

        private bool _IsVisiblePanel;
        public bool IsVisiblePanel
        {
            get { return _IsVisiblePanel; }
            set
            {
                if (value != _IsVisiblePanel)
                {
                    _IsVisiblePanel = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _Tab_Idx;
        public int Tab_Idx
        {
            get { return _Tab_Idx; }
            set
            {
                if (value != _Tab_Idx)
                {
                    _Tab_Idx = value;
                    RaisePropertyChanged();
                }
            }
        }
        

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    ProbingModule = this.ProbingModule();
                    MCM = this.ManualContactModule();
                    MCM.CPC_Visibility = Visibility.Hidden;
                    IsVisiblePanel = true;
                    Tab_Idx = 0;
                    PadJogSelect.IconSource = null;
                    PadJogSelect.IconCaption = null;
                    PadJogSelect.Caption = null;
                    PadJogSelect.Command = null;
                    PadJogSelect.IsEnabled = true;

                    Initialized = true;

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

        public async Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // TODO :
                StageSupervisor = this.StageSupervisor();

                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogLeftG.png");
                PadJogLeft.Command = new RelayCommand(DecreaseX);

                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogRightG.png");
                PadJogRight.Command = new RelayCommand(IncreaseX);

                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogUpG.png");
                PadJogUp.Command = new RelayCommand(IncreaseY);

                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogDownG.png");
                PadJogDown.Command = new RelayCommand(DecreaseY);

                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogLeftUpG.png");
                PadJogLeftUp.Command = new RelayCommand(DecreaseXIncreaseY);

                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogRightUpG.png");
                PadJogRightUp.Command = new RelayCommand(IncreaseXIncreaseY);

                PadJogLeftDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogLeftDownG.png");
                PadJogLeftDown.Command = new RelayCommand(DecreaseXDecreaseY);

                PadJogRightDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogRightDownG.png");
                PadJogRightDown.Command = new RelayCommand(IncreaseXDecreaseY);

                ZFM = this.GetForceMeasure();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {

            try
            {

                //this.SysState().SetSetUpState();
                PTPAModule = new PinPadMatchModule.PinPadMatchModule();
                ILotOPModule LotOpModule = this.LotOPModule();

                /******** 중요 ********/
                // 화면 진입 후, Contact 위치로 이동하기 전, 호출되어야 한다.
                // LoaderSystem에서도 화면 진입 시, 호출됨
                // 관련 변수 및 함수
                // this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffAngle
                // MachineIndexConvertToProbingCoord()
                var ret = PTPAModule.DoPinPadMatch();

                if(ret != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[{this.GetType().Name}], PageSwitched() : DoPinPadMatch is {ret}");
                }
                /**********************/

                // 여기서 메뉴얼 컨텍 초기 좌표 설정 필요함.

                //  1) 프로빙 하다 말고 왔을 때 (마지막으로 프로빙 했던 인덱스로 이동)
                //  2) 인스펙션 화면에서 들어왔을 때 (마지막으로 카메라에서 보던 인덱스로 이동)
                //  3) 아무데서나 놀다 왔을 때    (웨이퍼 가운데로 이동)


                // 이전 화면 확인
                //IViewModelManager ViewModelManager = this.ViewModelManager();
                //IMainScreenView preMainScreenView = ViewModelManager.PreMainScreenViewlist.Last();

                //long MXIndex = this.StageSupervisor().WaferObject.GetPhysInfo().CenM.XIndex.Value;
                //long MYIndex = this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value;
                MCM.GetOverDriveFromProbingModule();

                if (LotOpModule.ModuleState.State != ModuleStateEnum.RUNNING)
                {
                    CanUsingManualContactControl = true;
                }
                else
                {
                    CanUsingManualContactControl = false;
                }

                if (CanUsingManualContactControl == true)
                {
                    // TODO : 
                    await MCM.ChangeToZDownState();
                }
                if (MCM != null)
                {
                    MCM.CPC_Visibility = Visibility.Hidden;
                    Tab_Idx = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
            return EventCodeEnum.NONE;
        }

        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                //this.SysState().SetSetUpDoneState();
                this.ProbingModule().ClearUnderDutDevs();
                if (MCM.IsZUpState == true)
                {
                    MCM.ZDownMode();
                }
                this.StageSupervisor().StageModuleState.ZCLEARED();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
        private AsyncCommand _ControlLoadedCommand;
        public IAsyncCommand ControlLoadedCommand
        {
            get
            {
                if (null == _ControlLoadedCommand)
                    _ControlLoadedCommand = new AsyncCommand(ControlLoaded);
                return _ControlLoadedCommand;
            }
        }


        private RelayCommand _ZoomInCommand;
        public ICommand ZoomInCommand
        {
            get
            {
                if (null == _ZoomInCommand) _ZoomInCommand = new RelayCommand(ZoomIn);
                return _ZoomInCommand;
            }
        }

        private RelayCommand _ZoomOutCommand;
        public ICommand ZoomOutCommand
        {
            get
            {
                if (null == _ZoomOutCommand) _ZoomOutCommand = new RelayCommand(ZoomOut);
                return _ZoomOutCommand;
            }
        }

        private AsyncCommand _FirstContactSetCommand;
        public IAsyncCommand FirstContactSetCommand
        {
            get
            {
                if (null == _FirstContactSetCommand) _FirstContactSetCommand = new AsyncCommand(FirstContactSet);
                return _FirstContactSetCommand;
            }
        }

        private AsyncCommand _AllContactSetCommand;
        public IAsyncCommand AllContactSetCommand
        {
            get
            {
                if (null == _AllContactSetCommand) _AllContactSetCommand = new AsyncCommand(AllContactSet);
                return _AllContactSetCommand;
            }
        }

        //private RelayCommand _EnterZUpModeCommand;
        //public ICommand EnterZUpModeCommand
        //{
        //    get
        //    {
        //        if (null == _EnterZUpModeCommand) _EnterZUpModeCommand = new RelayCommand(EnterZUpMode);
        //        return _EnterZUpModeCommand;
        //    }
        //}

        private AsyncCommand _ChangeZUpStateCommand;
        public IAsyncCommand ChangeZUpStateCommand
        {
            get
            {
                if (null == _ChangeZUpStateCommand)
                    _ChangeZUpStateCommand = new AsyncCommand(ChangeZUpStateFunc);
                return _ChangeZUpStateCommand;
            }
        }

        private AsyncCommand _OverDriveTBClickCommand;
        public IAsyncCommand OverDriveTBClickCommand
        {
            get
            {
                if (null == _OverDriveTBClickCommand) _OverDriveTBClickCommand = new AsyncCommand(OverDriveTBClick);
                return _OverDriveTBClickCommand;
            }
        }

        private AsyncCommand _CPC_Z1_ClickCommand;
        public IAsyncCommand CPC_Z1_ClickCommand
        {
            get
            {
                if (null == _CPC_Z1_ClickCommand) _CPC_Z1_ClickCommand = new AsyncCommand(CPC_Z1_Click);
                return _CPC_Z1_ClickCommand;
            }
        }
        private AsyncCommand _CPC_Z2_ClickCommand;
        public IAsyncCommand CPC_Z2_ClickCommand
        {
            get
            {
                if (null == _CPC_Z2_ClickCommand) _CPC_Z2_ClickCommand = new AsyncCommand(CPC_Z2_Click);
                return _CPC_Z2_ClickCommand;
            }
        }

        private AsyncCommand _WantToMoveZIntervalTBClickCommand;
        public IAsyncCommand WantToMoveZIntervalTBClickCommand
        {
            get
            {
                if (null == _WantToMoveZIntervalTBClickCommand) _WantToMoveZIntervalTBClickCommand = new AsyncCommand(WantToMoveZIntervalTBClick);
                return _WantToMoveZIntervalTBClickCommand;
            }
        }

        private AsyncCommand _SetOverDriveCommand;
        public IAsyncCommand SetOverDriveCommand
        {
            get
            {
                if (null == _SetOverDriveCommand) _SetOverDriveCommand = new AsyncCommand(SetOverDrive);
                return _SetOverDriveCommand;
            }
        }

        private AsyncCommand _OverDriveValueUpCommand;
        public IAsyncCommand OverDriveValueUpCommand
        {
            get
            {
                if (null == _OverDriveValueUpCommand) _OverDriveValueUpCommand = new AsyncCommand(OverDriveValueUp);
                return _OverDriveValueUpCommand;
            }
        }

        private AsyncCommand _OverDriveValueDownCommand;
        public IAsyncCommand OverDriveValueDownCommand
        {
            get
            {
                if (null == _OverDriveValueDownCommand) _OverDriveValueDownCommand = new AsyncCommand(OverDriveValueDown);
                return _OverDriveValueDownCommand;
            }
        }

        private AsyncCommand _MoveToWannaZIntervalPlusCommand;
        public IAsyncCommand MoveToWannaZIntervalPlusCommand
        {
            get
            {
                if (null == _MoveToWannaZIntervalPlusCommand)
                    _MoveToWannaZIntervalPlusCommand = new AsyncCommand(MoveToWannaZIntervalPlus);
                return _MoveToWannaZIntervalPlusCommand;
            }
        }


        private AsyncCommand _MoveToWannaZIntervalMinusCommand;
        public IAsyncCommand MoveToWannaZIntervalMinusCommand
        {
            get
            {
                if (null == _MoveToWannaZIntervalMinusCommand)
                    _MoveToWannaZIntervalMinusCommand = new AsyncCommand(MoveToWannaZIntervalMinus);
                return _MoveToWannaZIntervalMinusCommand;
            }
        }

        private AsyncCommand<CUI.Button> _GoToInspectionViewCommand;
        public IAsyncCommand GoToInspectionViewCommand
        {
            get
            {
                if (null == _GoToInspectionViewCommand) _GoToInspectionViewCommand = new AsyncCommand<CUI.Button>(GoToInspectionView);
                return _GoToInspectionViewCommand;
            }
        }

        private AsyncCommand _ResetContactStartPositionCommand;
        public IAsyncCommand ResetContactStartPositionCommand
        {
            get
            {
                if (null == _ResetContactStartPositionCommand)
                    _ResetContactStartPositionCommand = new AsyncCommand(ResetContactStartPosition);
                return _ResetContactStartPositionCommand;
            }
        }

        private async Task ControlLoaded()
        {
        }

        private void ZoomIn()
        {
            try
            {
                MCM.ZoomIn();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ZoomOut()
        {
            try
            {
                MCM.ZoomOut();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task FirstContactSet()
        {
            try
            {

                if (MCM.IsZUpState == true)
                {
                    await MCM.FirstContactSet(); 
                    SetContactStartPosition();
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("FirstContactSet", "Only Setting on Z Up State", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task AllContactSet()
        {
            try
            {
                if (MCM.IsZUpState == true)
                {
                    await MCM.AllContactSet();
                    SetContactStartPosition();
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("AllContactSet", "Only Setting on Z Up State", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void EnterZUpMode()
        {
            try
            {
                bool IsPossibleZUp = false;

                double xPos = 0.0;
                double yPos = 0.0;
                this.StageSupervisor().MotionManager().GetRefPos(EnumAxisConstants.X, ref xPos);
                this.StageSupervisor().MotionManager().GetRefPos(EnumAxisConstants.Y, ref yPos);
                
                ProbingModuleDevParam probingDevParam = ProbingModule.ProbingModuleDevParam_IParam as ProbingModuleDevParam;


                double radiusDist = Math.Sqrt(Math.Pow(xPos, 2) + Math.Pow(yPos, 2));
                if (this.GetParam_ProbeCard().GetAlignState() != AlignStateEnum.DONE)
                {
                    IsPossibleZUp = false;
                    this.MetroDialogManager().ShowMessageDialog("Z Up", "Could not proceed Z up operation! \n" +
                        "Required to finish pin alignment previously.",
                        EnumMessageStyle.Affirmative);
                }
                else if (this.GetParam_Wafer().GetAlignState() != AlignStateEnum.DONE)
                {
                    IsPossibleZUp = false;
                    this.MetroDialogManager().ShowMessageDialog("Z Up", "Could not proceed Z up operation! \n" +
                        "Required to finish wafer alignment previously.",
                        EnumMessageStyle.Affirmative);
                }
                else if (radiusDist > this.CoordinateManager().StageCoord.ProbingSWRadiusLimit.Value)
                {
                    IsPossibleZUp = false;
                    this.MetroDialogManager().ShowMessageDialog("Z Up", $"Could not proceed Z up operation! \n" +
                        $"Out of probing radius({this.CoordinateManager().StageCoord.ProbingSWRadiusLimit.Value}) limit.",
                        EnumMessageStyle.Affirmative);
                }
                else if (MCM.OverDrive > probingDevParam.OverdriveUpperLimit.Value)
                {
                    IsPossibleZUp = false;
                    this.MetroDialogManager().ShowMessageDialog("Z Up", $"Could not proceed Z up operation! \n" +
                        $"Positive SW Limit occurred.\n Overdrive Upper Limit Value => {probingDevParam.OverdriveUpperLimit.Value} Overdrive Value => {MCM.OverDrive}\n",
                        EnumMessageStyle.Affirmative);
                }
                //-> Manual Contact화면에서는 OD값을 변경해가며 Contact Test를 진행하고 이 때 OD값을 저장하지는 않기 때문에 OD를 낮추고 LowLimit과의 비교를 하는 validation은 굳이 필요 하지 않다.
                //else if (MCM.OverDrive < probingDevParam.OverdriveLowLimit.Value)
                //{
                //    IsPossibleZUp = false;
                //    this.MetroDialogManager().ShowMessageDialog("Z Up", $"Could not proceed Z up operation! \n" +
                //        $"Positive SW Limit occurred.\n Overdrive Low Limit Value => {probingDevParam.OverdriveLowLimit.Value} Overdrive Value => {MCM.OverDrive}\n",
                //        EnumMessageStyle.Affirmative);
                //}
                else
                {
                    var ret = PTPAModule.DoPinPadMatch();

                    if (ret == EventCodeEnum.NONE)
                    {
                        this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.DONE);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Pin_To_Pad_Alignment_OK);

                        bool needtoask = true;
                        if(this.CardChangeModule().IsZifRequestedState(true, writelog:true) == EventCodeEnum.NONE)
                        {
                            needtoask = false;
                        }

                        var isZupok = true;
                        if (needtoask)
                        {
                            var msg = this.MetroDialogManager().ShowMessageDialog("Z Up", $"Zif Lock State is Unlock. Are you sure Zup?",
                                        EnumMessageStyle.AffirmativeAndNegative);
                            if (msg.Result == EnumMessageDialogResult.NEGATIVE)
                            {
                                isZupok = false;
                            }
                        }


                        if (isZupok)
                        {
                            IsPossibleZUp = true;
                            MCM.ZUpMode();

                        }
                        else
                        {
                            IsPossibleZUp = false;
                            LoggerManager.Debug($"Do not action zup. User canceled zup.");
                        }


                    }
                    else
                    {
                        IsPossibleZUp = false;
                        this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);
                        LoggerManager.Error($"PTPA Error. return code:{ret.ToString()}");

                        if (PTPAModule.LastResultStringBuilder == null)
                        {
                            this.MetroDialogManager().ShowMessageDialog("Z Up", "Could not proceed Z up operation! \n" +
                                "Required to finish Pin Pad Alignment previously.", EnumMessageStyle.Affirmative);
                        }
                        else
                        {
                            this.MetroDialogManager().ShowMessageDialog("Z Up", $"Could not proceed Z up operation! \n" + PTPAModule.LastResultStringBuilder, EnumMessageStyle.Affirmative);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task ChangeZUpStateFunc()
        {
            try
            {
                
                if (MCM.IsZUpState)
                {
                    MCM.ZDownMode();
                }
                else
                {
                    EnterZUpMode();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }

        private async Task OverDriveTBClick()
        {
            try
            {
                string retVal = null;

                retVal = GetInputValueFromVirtualKeyboard(MCM.OverDrive.ToString());
                MCM.ChangeOverDrive(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task CPC_Z1_Click()
        {
            try
            {
                string retVal = null;

                retVal = GetInputValueFromVirtualKeyboard(MCM.CPC_Z1.ToString());
                MCM.ChangeCPC_Z1(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task CPC_Z2_Click()
        {
            try
            {
                string retVal = null;

                retVal = GetInputValueFromVirtualKeyboard(MCM.CPC_Z2.ToString());
                MCM.ChangeCPC_Z2(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public void MCMChangeOverDrive(string overdrive)
        {
            try
            {
                MCM.ChangeOverDrive(overdrive);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void MCMChangeCPC_Z1(string z1)
        {
            try
            {
                MCM.ChangeCPC_Z1(z1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void MCMChangeCPC_Z2(string z2)
        {
            try
            {
                MCM.ChangeCPC_Z2(z2);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private async Task WantToMoveZIntervalTBClick()
        {
            try
            {
                string retVal = null;

                retVal = GetInputValueFromVirtualKeyboard(WantToMoveZInterval.ToString());
                ChangeWantToMoveZInterval(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangeWantToMoveZInterval(string WantToMoveZIntervalValue)
        {
            try
            {
                bool parseSuccessResult = false;
                double parseValue = 0;

                parseSuccessResult = ParsingToDouble(WantToMoveZIntervalValue, out parseValue);

                if (parseSuccessResult == true)
                {
                    this.WantToMoveZInterval = parseValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool ParsingToDouble(string parseData, out double parseValue)
        {
            bool retVal = false;

            try
            {
                parseValue = 0;

                if (!string.IsNullOrEmpty(parseData))
                {
                    retVal = double.TryParse(parseData, out parseValue);
                }
                else
                {
                    retVal = false;
                    parseValue = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = false;
                parseValue = 0;
            }

            return retVal;
        }


        private string GetInputValueFromVirtualKeyboard(string curValue)
        {
            string retString = null;
            try
            {
                //Window Owner = Application.Current.MainWindow;

                //int left = (int)((Owner.ActualWidth / 2) - (647 / 2));
                //int top = (int)((Owner.ActualHeight) - (400));
                Application.Current.Dispatcher.Invoke(() =>
                {
                    retString = VirtualKeyboard.Show(WindowLocationType.BOTTOM, curValue);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retString;
        }
        private async Task SetOverDrive()
        {
            try
            {
                ProbingModuleSysParam probingSysParam = ProbingModule.ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                ProbingModuleDevParam probingDevParam = ProbingModule.ProbingModuleDevParam_IParam as ProbingModuleDevParam;


                string limitStr = "";
                bool isChanged = true;

                if (probingSysParam.OverDriveStartPosition.Value == OverDriveStartPositionType.ALL_CONTACT)
                {
                    if (probingDevParam.IsEnableAllContactToOverdriveLimit.Value == true &&
                        probingDevParam.AllContactToOverdriveLimitRange.Value < MCM.OverDrive)
                    {
                        isChanged = false;
                        limitStr += $"StartPosition Type => All Contact.\n" +
                            $"AllContact to Overdrive Limit Value => {probingDevParam.AllContactToOverdriveLimitRange.Value}\n";
                    }
                }
                else if (probingSysParam.OverDriveStartPosition.Value == OverDriveStartPositionType.FIRST_CONTACT)
                {
                    if (probingDevParam.IsEnableFirstContactToOverdriveLimit.Value == true &&
                        probingDevParam.FirstContactToOverdriveLimitRange.Value < MCM.OverDrive)
                    {
                        isChanged = false;
                        limitStr += $"StartPosition Type => First Contact.\n" +
             $"First Contact to Overdrive Limit Value => {probingDevParam.FirstContactToOverdriveLimitRange.Value}\n";

                    }
                    if (probingDevParam.IsEnableAllContactToOverdriveLimit.Value == true &&
                        probingDevParam.AllContactToOverdriveLimitRange.Value < MCM.OverDrive)
                    {
                        isChanged = false;
                        limitStr += $"StartPosition Type => First Contact.\n" +
             $"AllContact to Overdrive Limit Value => {probingDevParam.AllContactToOverdriveLimitRange.Value}\n";

                    }
                }


                if (MCM.OverDrive > probingDevParam.OverdriveUpperLimit.Value)
                {
                    isChanged = false;
                    limitStr += $"Positive SW Limit occurred.\n" +
             $"Overdrive Limit Value => {probingDevParam.OverdriveUpperLimit.Value}\n" +
             $"Overdrive Value => {probingDevParam.OverDrive.Value}\n";

                }

                if (isChanged)
                {
                    this.ProbingModule.OverDrive = MCM.OverDrive;
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Not Set OverDrive.\n", $"{limitStr}", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private async Task OverDriveValueUp()
        {
            try
            {
                MCM.OverDriveValueUp();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task OverDriveValueDown()
        {
            try
            {
                MCM.OverDriveValueDown();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task MoveToWannaZIntervalPlus()
        {
            try
            {
                MCM.MoveToWannaZIntervalPlus(WantToMoveZInterval);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task MoveToWannaZIntervalMinus()
        {
            try
            {
                MCM.MoveToWannaZIntervalMinus(WantToMoveZInterval);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private double AdjustmentZPosition(double ZValue) //todo: 구현
        //{
        //    double adjustmentZPosition = 0;

        //    try
        //    {
        //        adjustmentZPosition = ZValue;
        //        //Z Value 리미트체크. Max, Min 조건에 맞지 않다면 조정.
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return adjustmentZPosition;
        //}

        //private EventCodeEnum MoveZPosition(double zValue) //todo: 구현
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //    return retVal;
        //}


        private async Task GoToInspectionView(CUI.Button cuiparam)
        {
            try
            {
                

                (this).StageSupervisor().StageModuleState.ZCLEARED();
                Guid ViewGUID = CUIServices.CUIService.GetTargetViewGUID(cuiparam.GUID);
                this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }


        private void IncreaseY()
        {
            try
            {
                MCM.IncreaseY();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DecreaseY()
        {
            try
            {
                MCM.DecreaseY();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void IncreaseX()
        {
            try
            {
                MCM.IncreaseX();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DecreaseX()
        {
            try
            {
                MCM.DecreaseX();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void IncreaseXIncreaseY()
        {
            try
            {
                MCM.SetIndex(EnumMovingDirection.RIGHT, EnumMovingDirection.UP);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DecreaseXIncreaseY()
        {
            try
            {
                MCM.SetIndex(EnumMovingDirection.LEFT, EnumMovingDirection.UP);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void IncreaseXDecreaseY()
        {
            try
            {
                MCM.SetIndex(EnumMovingDirection.RIGHT, EnumMovingDirection.DOWN);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DecreaseXDecreaseY()
        {
            try
            {
                MCM.SetIndex(EnumMovingDirection.LEFT, EnumMovingDirection.DOWN);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetContactStartPosition()
        {
            try
            {
                MCM.SetContactStartPosition();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task ResetContactStartPosition()
        {
            try
            {
                MCM.ResetContactStartPosition();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ManaulContactDataDescription GetManaulContactSetupDataDescription()
        {
            ManaulContactDataDescription des = new ManaulContactDataDescription();
            try
            {                
                des.ProbingModuleOverDriveStartPosition = this.ProbingModule().GetOverDriveStartPosition();
                des.ProbingModuleFirstContactHeight = this.ProbingModule().FirstContactHeight;
                des.ProbingModuleAllContactHeight = this.ProbingModule().AllContactHeight;
                des.ManualContactModuleOverDrive = this.ManualContactModule().OverDrive;
                //des.ManualCotactModuleIsZUpState = this.ManualContactModule().IsZUpState;
                des.WantToMoveZInterval = WantToMoveZInterval;
                des.CanUsingManualContactControl = CanUsingManualContactControl;
                des.IsVisiblePanel = IsVisiblePanel;
                des.ManualContactModuleMachinePosition = this.ManualContactModule().MachinePosition;
                des.ManualContactModuleXYIndex = this.ManualContactModule().MXYIndex;
                des.IsZUpState = this.ManualContactModule().IsZUpState;

                ProbingModuleSysParam sysparam = ProbingModule.ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                if (sysparam != null && sysparam.CPC != null & sysparam.CPC.Value.Count > 0)
                {
                    des.CPC_Z0 = sysparam.CPC.Value[0].Z0;
                    des.CPC_Z1 = sysparam.CPC.Value[0].Z1;
                    des.CPC_Z2 = sysparam.CPC.Value[0].Z2;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return des;
        }

        public bool GetManaulContactMovingStage()
        {
            bool retval = true;

            try
            {
                retval = this.ManualContactModule().IsMovingStage;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #region Remote Command

        public async Task FirstContactSetRemoteCommand()
        {
            try
            {
                if (MCM.IsZUpState == true)
                {
                    await MCM.FirstContactSet();
                    SetContactStartPosition();
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("FirstContactSet", "Only Setting on Z Up State", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task AllContactSetRemoteCommand()
        {

            try
            {
                if (MCM.IsZUpState == true)
                {
                    await MCM.AllContactSet();
                    SetContactStartPosition();
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("AllContactSet", "Only Setting on Z Up State", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task ResetContactStartPositionRemoteCommand()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                MCM.ResetContactStartPosition();
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

        public async Task OverDriveTBClickRemoteCommand()
        {
            try
            {
                string retVal = null;

                retVal = GetInputValueFromVirtualKeyboard(MCM.OverDrive.ToString());
                MCM.ChangeOverDrive(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task CPC_Z1_ClickRemoteCommand()
        {
            try
            {
                string retVal = null;

                retVal = GetInputValueFromVirtualKeyboard(MCM.CPC_Z1.ToString());
                MCM.ChangeCPC_Z1(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task CPC_Z2_ClickRemoteCommand()
        {
            try
            {
                string retVal = null;

                retVal = GetInputValueFromVirtualKeyboard(MCM.CPC_Z2.ToString());
                MCM.ChangeCPC_Z2(retVal);
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task ChangeZUpStateRemoteCommand()
        {
            try
            {
                
                if (MCM.IsZUpState)
                {
                    MCM.ZDownMode();
                }
                else
                {
                    EnterZUpMode();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task MoveToWannaZIntervalPlusRemoteCommand()
        {
            try
            {
                MCM.MoveToWannaZIntervalPlus(WantToMoveZInterval);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task WantToMoveZIntervalTBClickRemoteCommand()
        {
            try
            {
                string retVal = null;

                retVal = GetInputValueFromVirtualKeyboard(WantToMoveZInterval.ToString());

                ChangeWantToMoveZInterval(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task SetOverDriveRemoteCommand()
        {
            try
            {
                ProbingModuleSysParam probingSysParam = ProbingModule.ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                ProbingModuleDevParam probingDevParam = ProbingModule.ProbingModuleDevParam_IParam as ProbingModuleDevParam;

                string limitStr = "";
                bool isChanged = true;

                if (probingSysParam.OverDriveStartPosition.Value == OverDriveStartPositionType.ALL_CONTACT)
                {
                    if (probingDevParam.IsEnableAllContactToOverdriveLimit.Value == true &&
                        probingDevParam.AllContactToOverdriveLimitRange.Value < MCM.OverDrive)
                    {
                        isChanged = false;
                        limitStr += $"StartPosition Type => All Contact.\n" +
                            $"AllContact to Overdrive Limit Value => {probingDevParam.AllContactToOverdriveLimitRange.Value}\n";
                    }
                }
                else if (probingSysParam.OverDriveStartPosition.Value == OverDriveStartPositionType.FIRST_CONTACT)
                {
                    if (probingDevParam.IsEnableFirstContactToOverdriveLimit.Value == true &&
                        probingDevParam.FirstContactToOverdriveLimitRange.Value < MCM.OverDrive)
                    {
                        isChanged = false;
                        limitStr += $"StartPosition Type => First Contact.\n" +
             $"First Contact to Overdrive Limit Value => {probingDevParam.FirstContactToOverdriveLimitRange.Value}\n";

                    }
                    if (probingDevParam.IsEnableAllContactToOverdriveLimit.Value == true &&
                        probingDevParam.AllContactToOverdriveLimitRange.Value < MCM.OverDrive)
                    {
                        isChanged = false;
                        limitStr += $"StartPosition Type => First Contact.\n" +
             $"AllContact to Overdrive Limit Value => {probingDevParam.AllContactToOverdriveLimitRange.Value}\n";

                    }
                }

                if (MCM.OverDrive > probingDevParam.OverdriveUpperLimit.Value) 
                {
                    isChanged = false;
                    limitStr += $"Positive SW Limit occurred.\n" +
             $"Overdrive Limit Value => {probingDevParam.OverdriveUpperLimit.Value}\n" +
             $"Overdrive Value => {probingDevParam.OverDrive.Value}\n";

                }

                if (isChanged)
                {
                    this.ProbingModule.OverDrive = MCM.OverDrive;
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Not Set OverDrive.\n", $"{limitStr}", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task MoveToWannaZIntervalMinusRemoteCommand()
        {
            try
            {
                MCM.MoveToWannaZIntervalMinus(WantToMoveZInterval);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void GetOverDriveFromProbingModule()
        {
            try
            {
                MCM.GetOverDriveFromProbingModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }            
        }

        #endregion

    }
}
