using LogModule;
using MetroDialogInterfaces;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Event;
using ProberInterfaces.ODTP;
using ProberInterfaces.Param;
using ProberInterfaces.PnpSetup;
using ProbingModule;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ManualContact
{
    public class ManualContactModule : IManualContact, INotifyPropertyChanged
    {
        enum StageCam
        {
            WAFER_HIGH_CAM,
            WAFER_LOW_CAM,
            PIN_HIGH_CAM,
            PIN_LOW_CAM,
        }

        public bool Initialized { get; set; } = false;


        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region => MovingDirection Enum Function

        private EnumDirectionSign GetDirectionSign(EnumMovingDirection direction)
        {
            EnumDirectionSign retSign = EnumDirectionSign.PLUS;
            try
            {
                retSign = (EnumDirectionSign)((int)direction % (int)EnumMovingDirection.DIRECTION_DISTINGUISH);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retSign;
        }

        private EnumDirectionAxis GetDirectionAxis(EnumMovingDirection direction)
        {
            EnumDirectionAxis retAxis = EnumDirectionAxis.X;
            try
            {
                retAxis = (EnumDirectionAxis)((int)direction / (int)EnumMovingDirection.DIRECTION_DISTINGUISH);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retAxis;
        }

        #endregion

        #region => Property

        private object _ViewTarget;
        public object ViewTarget
        {
            get { return _ViewTarget; }
            set
            {
                if (value != _ViewTarget)
                {
                    _ViewTarget = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ManualContactStateBase _ManualContactZUpState;
        public ManualContactStateBase ManualContactZAxisState
        {
            get { return _ManualContactZUpState; }
            set
            {
                if (value != _ManualContactZUpState)
                {
                    _ManualContactZUpState = value;
                    RaisePropertyChanged(nameof(ManualContactZAxisState));
                }
            }
        }


        private System.Windows.Point _MXYIndex = new System.Windows.Point(0, 0);
        public System.Windows.Point MXYIndex
        {
            get { return _MXYIndex; }
            set
            {
                if (IsMovingStage == false)
                {
#pragma warning disable 4014
                    // 시간이 오래걸리는 작업이라 Await를 걸지 않았다.
                    // 향후, 커맨드 처리로 변경 검토 필요 by brett.
                    MoveToPosition(value);
#pragma warning restore 4014
                }
            }
        }

        private double _OverDrive;
        public double OverDrive
        {
            get
            {
                return _OverDrive;
            }
            set
            {
                if (value != _OverDrive)
                {
                    _OverDrive = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private double _CPC_Z0;
        public double CPC_Z0
        {
            get
            {
                return _CPC_Z0;
            }
            set
            {
                if (value != _CPC_Z0)
                {
                    _CPC_Z0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CPC_Z1;
        public double CPC_Z1
        {
            get
            {
                return _CPC_Z1;
            }
            set
            {
                if (value != _CPC_Z1)
                {
                    _CPC_Z1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CPC_Z2;
        public double CPC_Z2
        {
            get
            {
                return _CPC_Z2;
            }
            set
            {
                if (value != _CPC_Z2)
                {
                    _CPC_Z2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsZUpState;
        public bool IsZUpState
        {
            get { return _IsZUpState; }
            set
            {
                if (value != _IsZUpState)
                {
                    _IsZUpState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumStartContactPosition _StartContactPositionType = EnumStartContactPosition.NONE;
        public EnumStartContactPosition StartContactPositionType
        {
            get { return _StartContactPositionType; }
            set
            {
                if (_StartContactPositionType != value)
                {
                    _StartContactPositionType = value;
                }
            }
        }

        private double _SelectedContactPosition;
        public double SelectedContactPosition
        {
            get { return _SelectedContactPosition; }
            set
            {
                if (value != _SelectedContactPosition)
                {
                    _SelectedContactPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MachinePosition _MachinePosition;
        public MachinePosition MachinePosition
        {
            get { return _MachinePosition; }
            set
            {
                if (value != _MachinePosition)
                {
                    _MachinePosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsMovingStage = false;
        public bool IsMovingStage
        {
            get { return _IsMovingStage; }
            set
            {
                if (value != _IsMovingStage)
                {
                    _IsMovingStage = value;
                }
            }
        }

        //private bool _AlawaysMoveToTeachDie;
        //public bool AlawaysMoveToTeachDie
        //{
        //    get { return _AlawaysMoveToTeachDie; }
        //    set
        //    {
        //        if (value != _AlawaysMoveToTeachDie)
        //        {
        //            _AlawaysMoveToTeachDie = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private object LockObject;

        #endregion

        #region => Modules
        private ILotOPModule _LotOPModule;
        public ILotOPModule LotOPModule
        {
            get { return _LotOPModule; }
            set
            {
                if (value != _LotOPModule)
                {
                    _LotOPModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IProbingModule _ProbingModule;
        public IProbingModule ProbingModule
        {
            get { return _ProbingModule; }
            set
            {
                if (value != _ProbingModule)
                {
                    _ProbingModule = value;
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

        private ICoordinateManager _CoordinateManager;
        public ICoordinateManager CoordinateManager
        {
            get { return _CoordinateManager; }
            set
            {
                if (value != _CoordinateManager)
                {
                    _CoordinateManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IWaferAligner WaferAligner { get; set; }
        private IMotionManager MotionManager { get; set; }
        public IStageSupervisor StageSupervisor { get; set; }
        private IProberStation ProberStation { get; set; }

        public IWaferObject Wafer
        {
            get
            {
                return (IWaferObject)this.StageSupervisor().WaferObject;
            }
        }

        private IZoomObject _ZoomObject;
        public IZoomObject ZoomObject
        {
            get { return _ZoomObject; }
            set { _ZoomObject = value; }
        }

        public bool IsCallbackEntry { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

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

        private void WrapperMoveToPosition()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public delegate object TransformCoordinate(System.Windows.Point index);

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    LotOPModule = this.LotOPModule();
                    ProbingModule = this.ProbingModule();
                    CoordinateManager = this.CoordinateManager();
                    WaferAligner = this.WaferAligner();
                    MotionManager = this.MotionManager();
                    StageSupervisor = this.StageSupervisor();
                    ProberStation = this.ProberStation();

                    CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                    ZoomObject = Wafer;
                    LockObject = new object();
                    SelectedContactPosition = 0;
                    OverDrive = -100;
                    this.ManualContactZAxisState = new ManualContactZDown(this);
                    this.MachinePosition = new MachinePosition();

                    ViewTarget = Wafer;
                    retval = this.EventManager().RegisterEvent(typeof(CardChangedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(CardLoadingEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(CardUnloadedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    Initialized = true;
                    CPC_Visibility = Visibility.Hidden;
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
        private Visibility _CPC_Visibility;
        public Visibility CPC_Visibility
        {
            get { return _CPC_Visibility; }
            set
            {
                if (value != _CPC_Visibility)
                {
                    _CPC_Visibility = value;
                    RaisePropertyChanged();
                }
            }
        }
        private void EventFired(object sender, ProbeEventArgs e)
        {
            try
            {
                if (sender is CardChangedEvent)
                {
                    this.SelectedContactPosition = 0;
                    this.ProbingModule().FirstContactHeight = 0;
                    this.ProbingModule().AllContactHeight = 0;
                    LoggerManager.Debug($"[ManualContactModule] EventFired() : sender = CardChangedEvent");
                }
                else if (sender is CardLoadingEvent)
                {
                    this.SelectedContactPosition = 0;
                    this.ProbingModule().FirstContactHeight = 0;
                    this.ProbingModule().AllContactHeight = 0;
                    LoggerManager.Debug($"[ManualContactModule] EventFired() : sender = CardLoadingEvent");
                }
                else if (sender is CardUnloadedEvent)
                {
                    this.SelectedContactPosition = 0;
                    this.ProbingModule().FirstContactHeight = 0;
                    this.ProbingModule().AllContactHeight = 0;
                    LoggerManager.Debug($"[ManualContactModule] EventFired() : sender = CardUnloadedEvent");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ManualContactZAxisStateTransition(ManualContactStateBase changeState)
        {
            try
            {
                //if (StageSupervisor.StageMoveState == StageStateEnum.Z_UP)
                //{
                //    changeState = new ManualContactZUp(this);
                //}

                // TODO : Check 

                if (StageSupervisor.StageMoveState == StageStateEnum.Z_UP && changeState.IsZUpState != true)
                {
                    LoggerManager.Error("Unknown Status.");
                }

                this.ManualContactZAxisState = changeState;
                this.IsZUpState = this.ManualContactZAxisState.IsZUpState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ManualContactZDownStateTransition()
        {
            try
            {
                ManualContactZAxisStateTransition(new ManualContactZDown(this));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ChangeToZUpState()
        {
            ManualContactZAxisStateTransition(new ManualContactZUp(this));
        }

        public Task ChangeToZDownState()
        {
            try
            {
                EventCodeEnum stageErrorCode = EventCodeEnum.UNDEFINED;
                IInspection Inspection = this.InspectionModule();

                double selectedStartContactPosition = this.SelectedContactPosition;

                long MXIndex = this.StageSupervisor().WaferObject.GetCurrentMIndex().XIndex;
                long MYIndex = this.StageSupervisor().WaferObject.GetCurrentMIndex().YIndex;

                stageErrorCode = this.StageSupervisor().StageModuleState.ZCLEARED();

                ///1) 프로빙 시퀀스의 갯수가 1 개 이상이라면 첫번째 시퀀스의 위치로 이동한다.
                ///2) 시퀀스의 갯수가 0 개 라면, Teach Die 위치로 이동한다.
                ///3) 이동 후에, Move 결과를 보고 판단한다. (이동전에 위치를 판단하기에는 내부 계산 로직을 가져 와야함)

                bool isAssignMIndex = false;
                if(this.ProbingSequenceModule().ProbingSequenceCount >0)
                {
                    var firstProbingSeq = this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value[0];
                    if(firstProbingSeq != null)
                    {
                        MXIndex = firstProbingSeq.XIndex;
                        MYIndex = firstProbingSeq.YIndex;
                        isAssignMIndex = true;
                        LoggerManager.Debug($"ChangeToZDownState() 1st probing sequence data MXIndex : {MXIndex}, MYIndex : {MYIndex}");
                    }
                    else
                    {
                        LoggerManager.Debug("ChangeToZDownState() ProbingSequenceCount is not 0, 1st Probing sequence data is null");
                    }
                }
                else
                {
                    if (this.GetParam_ProbeCard().GetAlignState() == AlignStateEnum.DONE &&
                            this.GetParam_Wafer().GetAlignState() == AlignStateEnum.DONE)
                    {
                        if(this.Wafer.GetPhysInfo().TeachDieMIndex.Value.XIndex != -1 &&
                           this.Wafer.GetPhysInfo().TeachDieMIndex.Value.YIndex != -1)
                        {
                            MXIndex = this.Wafer.GetPhysInfo().TeachDieMIndex.Value.XIndex;
                            MYIndex = this.Wafer.GetPhysInfo().TeachDieMIndex.Value.YIndex;
                            isAssignMIndex = true;
                            LoggerManager.Debug($"ChangeToZDownState() TeachDie MXIndex : {MXIndex}, MYIndex : {MYIndex}");
                        }
                    }
                }

                if(isAssignMIndex)
                {
                    this.MXYIndex = new System.Windows.Point(MXIndex, MYIndex);
                    ProbingModule.GetUnderDutDices(new MachineIndex(MXIndex, MYIndex));
                }
                else
                {
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    this.MotionManager().StageMove(0, 0, zaxis.Param.ClearedPosition.Value, 0);
                    this.StageSupervisor().StageModuleState.ZCLEARED();
                    LoggerManager.Debug("ChangeToZDownState() move to 0,0");

                    //brett// message close 대기하지 않고 state transition 을 위해 await 하지 않음(func의 async도 제거함)
                    this.MetroDialogManager().ShowMessageDialog("Information Message",
                        "Not enough data to specify the initial location. \nCheck the position and index of the stage.", EnumMessageStyle.Affirmative);
                }

                if (stageErrorCode == EventCodeEnum.NONE)
                {
                    ManualContactZAxisStateTransition(new ManualContactZDown(this));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.CompletedTask;
        }

        //private void SetMxMyFromInspection(ref long mXIndex, ref long mYIndex)
        //{
        //    IInspection Inspection = this.InspectionModule();
        //    //mXIndex = Inspection.ManualSetIndexX;
        //    //mYIndex = Inspection.ManualSetIndexY;
        //}

        public void ZoomIn()
        {
            try
            {
                string Plus = string.Empty;
                Wafer.ZoomLevel--;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ZoomOut()
        {
            try
            {
                string Minus = string.Empty;
                Wafer.ZoomLevel++;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void IncreaseY()
        {
            SetPosition(EnumMovingDirection.UP);
        }

        public void DecreaseY()
        {
            SetPosition(EnumMovingDirection.DOWN);
        }

        public void IncreaseX()
        {
            SetPosition(EnumMovingDirection.RIGHT);
        }

        public void DecreaseX()
        {
            SetPosition(EnumMovingDirection.LEFT);
        }
        public void SetIndex(EnumMovingDirection xdir, EnumMovingDirection ydir)
        {
            SetPosition(xdir, ydir);
        }

        public async Task<bool> FirstContactSet()
        {

            //AllContactPosition = OverDrive;
            //FirstContactPosition = AllContactPosition;
            ProbingModuleDevParam probingDevParam = null;

            probingDevParam = ProbingModule.ProbingModuleDevParam_IParam as ProbingModuleDevParam;

            if (probingDevParam.IsEnableFirstContactToAllContactLimit.Value &&
                probingDevParam.FirstContactToAllContactLimitRange.Value < (ProbingModule.AllContactHeight - ProbingModule.FirstContactHeight + OverDrive))
            {
                EnumMessageDialogResult messageResult = EnumMessageDialogResult.UNDEFIND;

                messageResult = await this.MetroDialogManager().ShowMessageDialog("Set AllContact", "The limits of First Contact and All Contact Range exceeded.", EnumMessageStyle.Affirmative);

                return false;
            }


            ProbingModule.FirstContactHeight = ProbingModule.FirstContactHeight + OverDrive;
            ProbingModule.AllContactHeight = ProbingModule.FirstContactHeight;

            //if (ProbingModule.AllContactHeight < OverDrive)
            //{
            //    EnumMessageDialogResult messageResult = EnumMessageDialogResult.UNDEFIND;
            //    messageResult = await this.MetroDialogManager().ShowMessageDialog("Set FirstContact", "AllContactPosition lower than FirstContactPosition." +
            //        "If you want to set same value to FirstContactPosition, please click the 'OK' Button." +
            //        "If you Click the 'No' Button, than Setting AllContactPosition Nothing.",
            //                      EnumMessageStyle.AffirmativeAndNegative);
            //    if (messageResult == ProberInterfaces.Enum.EnumMessageDialogResult.AFFIRMATIVE)
            //    {
            //        ProbingModule.AllContactHeight = OverDrive;
            //        ProbingModule.FirstContactHeight = ProbingModule.AllContactHeight;
            //    }
            //    else
            //    {
            //    }
            //}
            //else
            //{
            //    ProbingModule.FirstContactHeight = OverDrive;
            //}

            return true;
        }

        public async Task<bool> AllContactSet()
        {

            ProbingModuleDevParam probingDevParam = null;

            probingDevParam = ProbingModule.ProbingModuleDevParam_IParam as ProbingModuleDevParam;

            if (probingDevParam.IsEnableFirstContactToAllContactLimit.Value &&
                probingDevParam.FirstContactToAllContactLimitRange.Value < (OverDrive + ProbingModule.AllContactHeight - ProbingModule.FirstContactHeight))
            {
                EnumMessageDialogResult messageResult = EnumMessageDialogResult.UNDEFIND;

                messageResult = await this.MetroDialogManager().ShowMessageDialog("Set AllContact", "The limits of First Contact and All Contact Range exceeded.", EnumMessageStyle.Affirmative);

                return false;
            }


            if (OverDrive + ProbingModule.AllContactHeight < ProbingModule.FirstContactHeight)
            {
                EnumMessageDialogResult messageResult = EnumMessageDialogResult.UNDEFIND;

                messageResult = await this.MetroDialogManager().ShowMessageDialog("Set AllContact", "AllContactPosition lower than FirstContactPosition." +
                    "If you want to set same value to FirstContactPosition, please click the 'OK' Button." +
                    "If you Click the 'No' Button, than Setting AllContactPosition Nothing.",
                                    EnumMessageStyle.AffirmativeAndNegative);

                if (messageResult == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    ProbingModule.AllContactHeight = OverDrive + ProbingModule.AllContactHeight;
                    ProbingModule.FirstContactHeight = ProbingModule.AllContactHeight;
                }
                else
                {
                }
            }
            else
            {
                ProbingModule.AllContactHeight = OverDrive + ProbingModule.AllContactHeight;
            }

            return true;
        }

        public void ZUpMode()   // manual contact 화면에서 타는 함수
        {
            try
            {
                EventCodeEnum stageErrorCode = EventCodeEnum.UNDEFINED;
                double selectedStartContactPosition = this.SelectedContactPosition;
                stageErrorCode = this.ManualContactZAxisState.ZUp(MXYIndex, OverDrive + selectedStartContactPosition);
                if (stageErrorCode == EventCodeEnum.NONE)
                {
                    MachinePositionUpdate();
                    ManualContactZAxisStateTransition(new ManualContactZUp(this));

                    if (this.SelectedContactPosition != 0)
                    {
                        ProbingModuleSysParam probingSysParam = ProbingModule.ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                        LoggerManager.Debug($"[Probing Contact Option] " +
                            $"ODStartPosition : {probingSysParam.OverDriveStartPosition.Value}, " +
                            $"OverDrive : {this.SelectedContactPosition}");
                    }

                    // ManualProbingEvent OverDrive 값은 all . first contact 값 제외 한 값이여야 함.
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(ManualProbingEvent).FullName, new ProbeEventArgs(this, semaphore, new ManualProbingEventArg(MXYIndex, OverDrive)));
                    semaphore.Wait();

                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.SetContactPosInfo(_MXYIndex, IsZUpState);
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Error Message",
                        "Can not to Zup. Please check and change the target position.", EnumMessageStyle.Affirmative);
                    throw new Exception("Fail ZUp.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        //todo : 여기에 Contact Param으로 조건 넣어야 함.
        public EventCodeEnum ZUpMode(long xIndex, long yIndex, double OD)   // multi manual contact 화면에서 타는 함수
        {
            EventCodeEnum stageErrorCode = EventCodeEnum.UNDEFINED;

            try
            {
                double selectedStartContactPosition = this.SelectedContactPosition;
                Point pt = new Point(xIndex, yIndex);
                stageErrorCode = this.ManualContactZAxisState.ZUp(pt, OD + selectedStartContactPosition);
                if (stageErrorCode == EventCodeEnum.NONE)
                {
                    MachinePositionUpdate();
                    ManualContactZAxisStateTransition(new ManualContactZUp(this));

                    // ManualProbingEvent OverDrive 값은 all . first contact 값 제외 한 값이여야 함.
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(ManualProbingEvent).FullName, new ProbeEventArgs(this, semaphore, new ManualProbingEventArg(pt, OD)));
                    semaphore.Wait();

                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.SetContactPosInfo(_MXYIndex, IsZUpState);
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Error Message",
                        "Can not to Zup. Please check and change the target position.", EnumMessageStyle.Affirmative);
                    throw new Exception("Fail ZUp.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return stageErrorCode;
        }

        public void ZDownMode(bool needZCleared = false)
        {
            try
            {
                EventCodeEnum stageErrorCode = EventCodeEnum.UNDEFINED;
                double selectedStartContactPosition = this.SelectedContactPosition;
                stageErrorCode = this.ManualContactZAxisState.ZDown(MXYIndex, OverDrive + selectedStartContactPosition);
                if (stageErrorCode == EventCodeEnum.NONE)
                {
                    MachinePositionUpdate();
                    ManualContactZAxisStateTransition(new ManualContactZDown(this));

                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.SetContactPosInfo(_MXYIndex, IsZUpState);
                }

                //ManualContactZAxisStateTransition(new ManualContactZDown(this));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void MachinePositionUpdate()
        {
            try
            {
                double xPos = 0;
                double yPos = 0;
                double zPos = 0;
                double tPos = 0;

                MotionManager.GetActualPos(EnumAxisConstants.X, ref xPos);
                MotionManager.GetActualPos(EnumAxisConstants.Y, ref yPos);
                MotionManager.GetActualPos(EnumAxisConstants.Z, ref zPos);
                MotionManager.GetActualPos(EnumAxisConstants.C, ref tPos);
                MachinePosition.ChangePosition(xPos, yPos, zPos, tPos);

                this.LoaderRemoteMediator()?.GetServiceCallBack()?.SetMachinePosition(MachinePosition);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ChangeOverDrive(string OverDriveValue)
        {
            bool parseSuccessResult = false;
            double parseValue = 0;

            parseSuccessResult = ParsingToDouble(OverDriveValue, out parseValue);

            if (parseSuccessResult == true)
            {
                this.OverDrive = parseValue;
            }
        }

        public void ChangeCPC_Z1(string CPC_Z1)
        {
            try
            {
                bool parseSuccessResult = false;
                double parseValue = 0;

                parseSuccessResult = ParsingToDouble(CPC_Z1, out parseValue);

                if (parseSuccessResult == true)
                {
                    this.CPC_Z1 = parseValue;
                    ProbingModuleSysParam sysparam = ProbingModule.ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                    if (sysparam != null && sysparam.CPC != null & sysparam.CPC.Value.Count > 0)
                    {
                        LoggerManager.Debug($"Change CPC_Z1={this.CPC_Z1}");
                        sysparam.CPC.Value[0].Z1 = this.CPC_Z1;
                        ProbingModule.SaveSysParameter();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ChangeCPC_Z2(string CPC_Z2)
        {
            try
            {
                bool parseSuccessResult = false;
                double parseValue = 0;

                parseSuccessResult = ParsingToDouble(CPC_Z2, out parseValue);

                if (parseSuccessResult == true)
                {
                    this.CPC_Z2 = parseValue;
                    ProbingModuleSysParam sysparam = ProbingModule.ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                    if (sysparam != null && sysparam.CPC != null & sysparam.CPC.Value.Count > 0)
                    {
                        LoggerManager.Debug($"Change CPC_Z2={this.CPC_Z2}");
                        sysparam.CPC.Value[0].Z2 = this.CPC_Z2;
                        ProbingModule.SaveSysParameter();
                    }
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
            parseValue = 0;
            if (!string.IsNullOrEmpty(parseData))
            {
                retVal = double.TryParse(parseData, out parseValue);
            }

            return retVal;
        }

        public void OverDriveValueUp()
        {
            this.OverDrive++;
        }

        public void OverDriveValueDown()
        {
            this.OverDrive--;
        }

        public EventCodeEnum MoveToWannaZIntervalPlus(double wantToMoveZInterval)
        {
            EventCodeEnum moveResult = EventCodeEnum.UNDEFINED;
            try
            {
                double selectedStartContactPosition = this.SelectedContactPosition;
                bool isChanged = true;
                ProbingModuleSysParam probingSysParam = ProbingModule.ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                ProbingModuleDevParam probingDevParam = ProbingModule.ProbingModuleDevParam_IParam as ProbingModuleDevParam;

                string limitStr = "";

                OverDrive += wantToMoveZInterval;
                if (probingSysParam.OverDriveStartPosition.Value == OverDriveStartPositionType.ALL_CONTACT)
                {
                    if (probingDevParam.IsEnableAllContactToOverdriveLimit.Value == true &&
                        probingDevParam.AllContactToOverdriveLimitRange.Value < OverDrive)
                    {
                        OverDrive -= wantToMoveZInterval;
                        isChanged = false;
                        limitStr += $"StartPosition Type => All Contact.\n" +
                            $"AllContact to Overdrive Limit Value => {probingDevParam.AllContactToOverdriveLimitRange.Value}\n";
                    }
                }
                else if (probingSysParam.OverDriveStartPosition.Value == OverDriveStartPositionType.FIRST_CONTACT)
                {
                    if (probingDevParam.IsEnableFirstContactToOverdriveLimit.Value == true &&
                        probingDevParam.FirstContactToOverdriveLimitRange.Value < OverDrive)
                    {
                        OverDrive -= wantToMoveZInterval;
                        isChanged = false;
                        limitStr += $"StartPosition Type => First Contact.\n" +
             $"First Contact to Overdrive Limit Value => {probingDevParam.FirstContactToOverdriveLimitRange.Value}\n";

                    }
                    if (probingDevParam.IsEnableAllContactToOverdriveLimit.Value == true &&
                        probingDevParam.AllContactToOverdriveLimitRange.Value < OverDrive)
                    {
                        OverDrive -= wantToMoveZInterval;
                        isChanged = false;
                        limitStr += $"StartPosition Type => First Contact.\n" +
             $"AllContact to Overdrive Limit Value => {probingDevParam.AllContactToOverdriveLimitRange.Value}\n";

                    }
                }

                // 
                if (probingDevParam.OverDrive.Value > probingDevParam.OverdriveUpperLimit.Value)
                {
                    isChanged = false;
                    limitStr += $"Positive SW Limit occurred.\n" +
             $"Overdrive Limit Value => {probingDevParam.OverdriveUpperLimit.Value}\n" +
              $"Overdrive Value => {probingDevParam.OverDrive.Value}\n";

                }

                if (isChanged)
                {
                    moveResult = ManualContactZAxisState.ZUp(MXYIndex, OverDrive + selectedStartContactPosition);

                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.SetContactPosInfo(_MXYIndex, IsZUpState);

                    if (moveResult == EventCodeEnum.NONE)
                    {
                        MachinePositionUpdate();
                    }
                    else
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}] MoveToWannaZIntervalPlus() : Fail Z Up");
                    }
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Could not change OverDrive value.", $"{limitStr}", EnumMessageStyle.Affirmative);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return moveResult;
        }

        public EventCodeEnum MoveToWannaZIntervalMinus(double wantToMoveZInterval)
        {
            EventCodeEnum moveResult = EventCodeEnum.UNDEFINED;
            try
            {
                double selectedStartContactPosition = this.SelectedContactPosition;
                bool isChanged = true;
                ProbingModuleSysParam probingSysParam = ProbingModule.ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                ProbingModuleDevParam probingDevParam = ProbingModule.ProbingModuleDevParam_IParam as ProbingModuleDevParam;

                OverDrive -= wantToMoveZInterval;
                //OD Start 위치에 따른 상황.
                if (probingSysParam.OverDriveStartPosition.Value == OverDriveStartPositionType.ALL_CONTACT)
                {
                    if (probingDevParam.IsEnableAllContactToOverdriveLimit.Value == true &&
                        probingDevParam.AllContactToOverdriveLimitRange.Value < OverDrive)
                    {
                        OverDrive += wantToMoveZInterval;
                        isChanged = false;
                    }
                }
                else if (probingSysParam.OverDriveStartPosition.Value == OverDriveStartPositionType.FIRST_CONTACT)
                {
                    if (probingDevParam.IsEnableFirstContactToOverdriveLimit.Value == true &&
                        probingDevParam.FirstContactToOverdriveLimitRange.Value < OverDrive)
                    {
                        OverDrive += wantToMoveZInterval;
                        isChanged = false;
                    }
                    if (probingDevParam.IsEnableAllContactToOverdriveLimit.Value == true &&
                        probingDevParam.AllContactToOverdriveLimitRange.Value < OverDrive)
                    {
                        OverDrive += wantToMoveZInterval;
                        isChanged = false;
                    }
                }

                //OD Low Limit 체크/수정.
                if (OverDrive < probingDevParam.OverdriveLowLimit.Value)
                {
                    OverDrive = probingDevParam.OverdriveLowLimit.Value;
                }

                //OD Upper Limit 체크/수정.
                if (OverDrive > probingDevParam.OverdriveUpperLimit.Value)
                {
                    isChanged = false;
                }

                if (isChanged)
                {
                    moveResult = ManualContactZAxisState.ZUp(MXYIndex, OverDrive + selectedStartContactPosition);

                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.SetContactPosInfo(_MXYIndex, IsZUpState);

                    if (moveResult == EventCodeEnum.NONE)
                    {
                        MachinePositionUpdate();
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return moveResult;
        }

        #region => SetPosition Function
        private void SetPosition(EnumMovingDirection movingDirection)
        {
            try
            {
                long movingIndex = 0;
                EnumDirectionAxis moveingAxis = EnumDirectionAxis.X;
                int moveIndexInterval = 0;

                moveingAxis = GetDirectionAxis(movingDirection);
                moveIndexInterval = GetMoveIndexIntervalWithSign(movingDirection);
                movingIndex = GetmovingIndex(moveingAxis, moveIndexInterval);
                movingIndex = AdjustmentPosition(moveingAxis, movingIndex);
                SetMovingIndex(moveingAxis, movingIndex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void SetPosition(EnumMovingDirection movingDirectionX, EnumMovingDirection movingDirectionY)
        {
            try
            {
                long movingIndexX = 0;
                EnumDirectionAxis moveingAxisX = EnumDirectionAxis.X;
                int moveIndexIntervalX = 0;
                moveingAxisX = GetDirectionAxis(movingDirectionX);
                moveIndexIntervalX = GetMoveIndexIntervalWithSign(movingDirectionX);
                movingIndexX = GetmovingIndex(moveingAxisX, moveIndexIntervalX);
                movingIndexX = AdjustmentPosition(moveingAxisX, movingIndexX);

                long movingIndexY = 0;
                EnumDirectionAxis moveingAxisY = EnumDirectionAxis.Y;
                int moveIndexIntervalY = 0;

                moveingAxisY = GetDirectionAxis(movingDirectionY);
                moveIndexIntervalY = GetMoveIndexIntervalWithSign(movingDirectionY);
                movingIndexY = GetmovingIndex(moveingAxisY, moveIndexIntervalY);
                movingIndexY = AdjustmentPosition(moveingAxisY, movingIndexY);

                SetMovingIndex(movingIndexX, movingIndexY);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetMovingIndex(EnumDirectionAxis movingAxis, long movingIndex)
        {
            try
            {

                //if (ManualContactZAxisState.IsCanMoveStage())
                {
                    switch (movingAxis)
                    {
                        case EnumDirectionAxis.X:
                            MXYIndex = new System.Windows.Point(movingIndex, MXYIndex.Y);
                            break;
                        case EnumDirectionAxis.Y:
                            MXYIndex = new System.Windows.Point(MXYIndex.X, movingIndex);
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void SetMovingIndex(long movingindexX, long movingIndexY)
        {
            try
            {
                MXYIndex = new System.Windows.Point(movingindexX, movingIndexY);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private int GetMoveIndexIntervalWithSign(EnumMovingDirection movingDirection)
        {
            int moveIndexInterval = 0;
            try
            {
                int calMethod = 0;

                moveIndexInterval = GetMoveIndexInterval();
                calMethod = DistinguishSign(movingDirection);
                moveIndexInterval *= calMethod;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return moveIndexInterval;
        }

        private int DistinguishSign(EnumMovingDirection movingDirection)
        {
            int retSign = 1;
            try
            {
                EnumDirectionSign enumDirectionSign = EnumDirectionSign.PLUS;

                enumDirectionSign = GetDirectionSign(movingDirection);
                switch (enumDirectionSign)
                {
                    case EnumDirectionSign.PLUS:
                        retSign = 1;
                        break;
                    case EnumDirectionSign.MINUS:
                    default:
                        retSign = -1;
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retSign;
        }

        private long GetmovingIndex(EnumDirectionAxis enumMovingAxis, int moveIndex)
        {
            long movingIndex = 0;
            try
            {
                int reverseDir = 1;

                if (this.CoordinateManager().GetReverseManualMoveX() == true && this.CoordinateManager().GetReverseManualMoveY() == true)
                {
                    reverseDir = -1;
                }
                moveIndex = moveIndex * reverseDir;

                switch (enumMovingAxis)
                {
                    case EnumDirectionAxis.X:
                        movingIndex = (long)(MXYIndex.X + moveIndex);
                        break;
                    case EnumDirectionAxis.Y:
                        movingIndex = (long)(MXYIndex.Y + moveIndex);
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return movingIndex;
        }

        private long AdjustmentPosition(EnumDirectionAxis enumMovingAxis, long movingIndex)
        {
            long adjustmentIndex = 0;
            try
            {
                int xLength = 0;
                int yLength = 0;
                int axisLength = 0;
                xLength = Wafer.GetSubsInfo().DIEs.GetLength(0);
                yLength = Wafer.GetSubsInfo().DIEs.GetLength(1);

                switch (enumMovingAxis)
                {
                    case EnumDirectionAxis.X:
                        axisLength = xLength;
                        break;
                    case EnumDirectionAxis.Y:
                        axisLength = yLength;
                        break;
                }

                if (axisLength <= movingIndex)
                {
                    adjustmentIndex = axisLength - 1;
                }
                else if (movingIndex < 0)
                {
                    adjustmentIndex = 0;
                }
                else
                {
                    adjustmentIndex = movingIndex;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return adjustmentIndex;
        }

        private int GetMoveIndexInterval()
        {
            int moveIndex = 1;
            return moveIndex;
        }



        //private async Task<EventCodeEnum> MoveToPosition(System.Windows.Point mxyIndex)
        private async Task MoveToPosition(System.Windows.Point mxyIndex)
        {
            EventCodeEnum moveResult = EventCodeEnum.UNDEFINED;
            bool successmove = true;
            try
            {
                await Task.Run(async () =>
                {
                    IsMovingStage = true;

                    double zc = 0;
                    double od = ProbingModule.OverDrive;


                    WaferCoordinate waferCoordinate = null;
                    MachineCoordinate moveCoordinate = null;
                    PinCoordinate pinCoordinate = new PinCoordinate();

                    try
                    {
                        if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                        {
                            //await this.WaitCancelDialogService().ShowDialog("Wait");
                            await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                        }
                        lock (LockObject)
                        {
                            zc = ProbingModule.ZClearence;
                            zc = ProbingModule.CalculateZClearenceUsingOD(od, zc);

                            //waferCoordinate = WaferAligner.MachineIndexConvertToProbingCoord((int)mxyIndex.X, (int)mxyIndex.Y);
                            waferCoordinate = WaferAligner.MachineIndexConvertToProbingCoord((int)mxyIndex.X, (int)mxyIndex.Y);

                            pinCoordinate.X.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                            pinCoordinate.Y.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                            pinCoordinate.Z.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                            if (!(this.GetParam_ProbeCard().GetAlignState() == AlignStateEnum.DONE
                                    && this.GetParam_Wafer().GetAlignState() == AlignStateEnum.DONE
                                    && this.GetParam_ProbeCard().GetPinPadAlignState() == AlignStateEnum.DONE))
                            {
                                var axispz = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                                moveCoordinate = new MachineCoordinate(0, 0, axispz.Param.ClearedPosition.Value);
                                var coord = this.CoordinateManager().PinHighPinConvert.Convert(moveCoordinate);
                                pinCoordinate.Z.Value = coord.Z.Value;
                                pinCoordinate.Z.Value = this.StageSupervisor().PinMinRegRange;
                                var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                                moveCoordinate = new MachineCoordinate(0, 0, axisz.Param.ClearedPosition.Value);
                                var wcoord = this.CoordinateManager().WaferHighChuckConvert.Convert(moveCoordinate);
                                waferCoordinate.Z.Value = wcoord.Z.Value;
                                waferCoordinate.Z.Value = 2000.0;
                            }

                            moveResult = ManualContactZAxisState.MovePadToPin(waferCoordinate, pinCoordinate, zc);

                            if (moveResult == EventCodeEnum.NONE)
                            {
                                MachinePositionUpdate();
                                ProbingModule.GetUnderDutDices(new MachineIndex((long)mxyIndex.X, (long)mxyIndex.Y));
                            }
                            else
                            {
                                successmove = false;

                                double ox = _MXYIndex.X;
                                double oy = _MXYIndex.Y;
                                _MXYIndex.X = 0;
                                _MXYIndex.Y = 0;

                                RaisePropertyChanged(nameof(MXYIndex));
                                this.LoaderRemoteMediator()?.GetServiceCallBack()?.SetContactPosInfo(_MXYIndex, IsZUpState);
                                System.Threading.Thread.Sleep(20);
                                _MXYIndex.X = ox;
                                _MXYIndex.Y = oy;
                                RaisePropertyChanged(nameof(MXYIndex));
                                this.LoaderRemoteMediator()?.GetServiceCallBack()?.SetContactPosInfo(_MXYIndex, IsZUpState);
                                System.Threading.Thread.Sleep(20);

                            }
                        }
                    }
                    catch (Exception err)
                    {
                        moveResult = EventCodeEnum.UNDEFINED;
                        LoggerManager.Error($"[ManualContactControlVM] MoveToPosition Error, ErrorCode: {err}");
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                MachinePositionUpdate();

                if (successmove)
                {
                    _MXYIndex = mxyIndex;
                    RaisePropertyChanged(nameof(MXYIndex));

                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.SetContactPosInfo(_MXYIndex, IsZUpState);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Error Message",
                                   "This is a non-moveable location. Please move to another location.", EnumMessageStyle.Affirmative);
                }

                IsMovingStage = false;

                //await this.WaitCancelDialogService().CloseDialog();
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }

            //return moveResult;
        }

        public void SetContactStartPosition()
        {
            try
            {
                ProbingModuleSysParam probingSysParam = null;
                probingSysParam = ProbingModule.ProbingModuleSysParam_IParam as ProbingModuleSysParam;

                if (probingSysParam.OverDriveStartPosition.Value == OverDriveStartPositionType.FIRST_CONTACT)
                {
                    SelectedContactPosition = ProbingModule.FirstContactHeight;
                }
                else if (probingSysParam.OverDriveStartPosition.Value == OverDriveStartPositionType.ALL_CONTACT)
                {
                    SelectedContactPosition = ProbingModule.AllContactHeight;
                }

                OverDrive = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ResetContactStartPosition()
        {
            try
            {
                SelectedContactPosition = 0;
                ProbingModule.FirstContactHeight = 0;
                ProbingModule.AllContactHeight = 0;

                GetOverDriveFromProbingModule();
                GetCPCFromProbingModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void GetOverDriveFromProbingModule()
        {
            OverDrive = ProbingModule.OverDrive;
        }

        public void GetCPCFromProbingModule()
        {
            var cpc = ProbingModule.GetCPCValues();
            if (cpc.Count > 0)
            {
                CPC_Z0 = cpc[0].Z0;
                CPC_Z1 = cpc[0].Z1;
                CPC_Z2 = cpc[0].Z2;
            }
        }

        public void InitSelectedContactPosition()
        {
            ResetContactStartPosition();
        }
        #endregion

    }
}
