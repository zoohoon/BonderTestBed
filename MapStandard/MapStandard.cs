using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WAMapStandardModule
{
    using ProberInterfaces;
    using ProberInterfaces.WaferAlignEX;
    using System.IO;
    using RelayCommandBase;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using ProberInterfaces.Param;
    using System.Windows;
    using ProberInterfaces.PnpSetup;
    using System.Collections.ObjectModel;
    using System.Threading;
    using SubstrateObjects;
    using PnPControl;
    using ProberErrorCode;
    using ProberInterfaces.Vision;
    using WA_MapParameter_Standard;
    using ProberInterfaces.State;
    using LogModule;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using SerializerUtil;
    using ProberInterfaces.Enum;

    enum Direction
    {
        UNDIFIND,
        UPPER,
        BOTTOM,
        LEFT,
        RIGHT
    }

    enum MapModifyTypeEnum
    {
        UNDIFIND,
        AUTO,
        MANUAL,
    }
    enum AutoMapGernerateEnum
    {
        UNDIFIND,
        EDGE,
        FULL
    }

    public class MapStandard : PNPSetupBase, ISetup, IParamNode, IHasDevParameterizable, IPackagable
    {
        public override bool Initialized { get; set; } = false;
        public override Guid ScreenGUID { get; } = new Guid("4203F878-B532-8CCC-2613-D5745D4ED5AE");
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
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
        public new List<object> Nodes { get; set; }


        #region ..//Property

        private IWaferAligner WaferAligner;
        private WADrawMapSetupFunction ModifyCondition;

        //private FocusParameter FocusingParam { get; set; }


        private WaferObject Wafer
        {
            get
            {
                return (WaferObject)this.StageSupervisor().WaferObject;
            }
        }


        private SubModuleStateBase _AlignModuleState;
        public SubModuleStateBase SubModuleState
        {
            get { return _AlignModuleState; }
            set
            {
                if (value != _AlignModuleState)
                {
                    _AlignModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private WA_MapParam_Standard MapStandardParam_Clone;



        private WriteableBitmap _imgDieLeftUp;
        public WriteableBitmap imgDieLeftUp
        {
            get { return _imgDieLeftUp; }
            set
            {
                if (value != _imgDieLeftUp)
                {
                    _imgDieLeftUp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WriteableBitmap _imgDieRightUp;
        public WriteableBitmap imgDieRightUp
        {
            get { return _imgDieRightUp; }
            set
            {
                if (value != _imgDieRightUp)
                {
                    _imgDieRightUp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WriteableBitmap _imgDieLeftDown;
        public WriteableBitmap imgDieLeftDown
        {
            get { return _imgDieLeftDown; }
            set
            {
                if (value != _imgDieLeftDown)
                {
                    _imgDieLeftDown = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WriteableBitmap _imgDieRightDown;
        public WriteableBitmap imgDieRightDown
        {
            get { return _imgDieRightDown; }
            set
            {
                if (value != _imgDieRightDown)
                {
                    _imgDieRightDown = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MapModifyTypeEnum _MapModifyType;
        private AutoMapGernerateEnum _AutoMapGernerate;
        private MapStandardAutoMapGenerateState _AutoMapState;


        #endregion

        #region ..//Command & CommandMethod
        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                WaferObject.MapViewStageSyncEnable = true;
                InitDeviceState();
                SaveDevParameter();
                retVal = await base.Cleanup(parameter);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }

        private void InitDeviceState()
        {
            try
            {
                List<DeviceObject> Devices = Wafer.GetSubsInfo().Devices.ToList<DeviceObject>();
                foreach (var device in Devices.FindAll(device => device.DieType.Value == DieTypeEnum.CHANGEMARK_DIE))
                {
                    device.DieType.Value = DieTypeEnum.MARK_DIE;
                }
                foreach (var device in Devices.FindAll(device => device.DieType.Value == DieTypeEnum.CHANGETEST_DIE))
                {
                    device.DieType.Value = DieTypeEnum.TEST_DIE;
                }
                foreach (var device in Devices.FindAll(device => device.DieType.Value == DieTypeEnum.MODIFY_DIE))
                {
                    device.DieType.Value = DieTypeEnum.MARK_DIE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

        }

        private RelayCommand _UIModeChangeCommand;
        public ICommand UIModeChangeCommand
        {
            get
            {
                if (null == _UIModeChangeCommand) _UIModeChangeCommand = new RelayCommand(
                    UIModeChange, EvaluationPrivilege.Evaluate(
                            CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                             new Action(() => { ShowMessages("UIModeChange"); }));
                return _UIModeChangeCommand;
            }
        }

        private void UIModeChange()
        {

        }


        //private RelayCommand _Button1Command;
        //public ICommand Button1Command
        //{
        //    get
        //    {
        //        if (null == _Button1Command) _Button1Command
        //                = new RelayCommand(DrawMapTest);
        //        return _Button1Command;
        //    }
        //}

        private async Task DrawMapTest()
        {
            try
            {
                ModifyCondition = WADrawMapSetupFunction.CHANGETEST;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        //private RelayCommand _Button2Command;
        //public ICommand Button2Command
        //{
        //    get
        //    {
        //        if (null == _Button2Command) _Button2Command
        //                = new RelayCommand(DrawMapMark);
        //        return _Button2Command;
        //    }
        //}

        private async Task DrawMapMark()
        {
            try
            {
                ModifyCondition = WADrawMapSetupFunction.CHANGEMARK;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //private RelayCommand _Button3Command;
        //public ICommand Button3Command
        //{
        //    get
        //    {
        //        if (null == _Button3Command) _Button3Command
        //                = new RelayCommand(DrawMapSkip);
        //        return _Button3Command;
        //    }
        //}
        private async Task DrawMapSkip()
        {
            try
            {
                ModifyCondition = WADrawMapSetupFunction.CHANGESKIP;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        //}
        //private RelayCommand _Button4Command;
        //public ICommand Button4Command
        //{
        //    get
        //    {
        //        if (null == _Button4Command) _Button4Command
        //                = new RelayCommand(DrawMapEmpty);
        //        return _Button4Command;
        //    }
        //}

        private CancellationTokenSource CancelToken = null;

        private async Task DrawMapEmpty()
        {
            try
            {
                ModifyCondition = WADrawMapSetupFunction.CHANGEEMPTY;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private async Task<EventCodeEnum> AutoEdgeMapGenerate()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;

            try
            {
                //this.VisionManager().StartGrab(EnumProberCam.WAFER_HIGH_CAM);
                ModifyCondition = WADrawMapSetupFunction.DRAWAUTOMAP;
                _AutoMapGernerate = AutoMapGernerateEnum.EDGE;
                //Task<EventCodeEnum> stateTask;
                //stateTask = Task.Run(() => Modify());
                //await stateTask;
                //retval = stateTask.Result;
                retval = await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.AutoEdgeMapGenerate() : Error occured");
            }

            return retval;
        }

        private async Task<EventCodeEnum> AutoFullMapGenerate()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            try
            {
                ModifyCondition = WADrawMapSetupFunction.DRAWAUTOMAP;
                _AutoMapGernerate = AutoMapGernerateEnum.FULL;

                //Task<EventCodeEnum> stateTask;

                //stateTask = Task.Run(() => Modify());
                //await stateTask;
                //retval = stateTask.Result;
                retval = await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "AutoFullMapGenerate() : Error occured");
            }


            return retval;
        }

        private RelayCommand _ChangeMapModifyModeCommand;
        public ICommand ChangeMapModifyModeCommand
        {
            get
            {
                if (null == _ChangeMapModifyModeCommand) _ChangeMapModifyModeCommand
                        = new RelayCommand(ChangeMapModifyMode);
                return _ChangeMapModifyModeCommand;
            }
        }


        private void ChangeMapModifyMode()
        {
            try
            {
                if (_MapModifyType == MapModifyTypeEnum.MANUAL)
                {
                    AutoMapPNPUI();
                    PadJogRightDown.IsEnabled = false;
                }
                else if (_MapModifyType == MapModifyTypeEnum.AUTO)
                {
                    ManualMapPNPUI();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }



        private void AutoMapAbort()
        {
            try
            {
                if (CancelToken != null)
                    CancelToken.Cancel();
                this.VisionManager().StopWaitGrab(CurCam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion
        private IParam _MapStandardParam_IParam;
        [ParamIgnore]
        public IParam MapStandardParam_IParam
        {
            get { return _MapStandardParam_IParam; }
            set
            {
                if (value != _MapStandardParam_IParam)
                {
                    _MapStandardParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        public SubModuleMovingStateBase MovingState { get; set; }

        private IFocusing _WaferMapFocusModel;
        public IFocusing PinFocusModel
        {
            get
            {
                if (_WaferMapFocusModel == null)
                    _WaferMapFocusModel = this.FocusManager().GetFocusingModel((MapStandardParam_IParam as WA_MapParam_Standard).FocusingModuleDllInfo);

                return _WaferMapFocusModel;
            }
        }

        private IFocusParameter FocusParam => (MapStandardParam_IParam as WA_MapParam_Standard)?.FocusParam;

        /// <summary>
        /// 선택된 변경 다이의 상태를 판단하기위해.
        /// </summary>
        private WADrawMapSetupFunction _SelectedMapSetupFunction;
        private bool IsManualSetting { get; set; }

        private bool _IsManualEnterMode = false;
        /// <summary>
        /// 연속되게 다이 변경 가능하게 변경할 Mode ( Enter 모드가 켜져있을시에는 모션과 WaferMap 으로의 이동을 제한한다.)
        /// </summary>
        public bool IsManualEnterMode
        {
            get { return _IsManualEnterMode; }
            set
            {
                _IsManualEnterMode = value;
                if (_IsManualEnterMode)
                {
                    PadJogUp.IsEnabled = true;
                    PadJogDown.IsEnabled = true;
                    PadJogLeft.IsEnabled = true;
                    PadJogRight.IsEnabled = true;
                    MotionJogEnabled = false;
                    Wafer.MapViewStageSyncEnable = false;
                }
                else
                {
                    PadJogUp.IsEnabled = false;
                    PadJogDown.IsEnabled = false;
                    PadJogLeft.IsEnabled = false;
                    PadJogRight.IsEnabled = false;
                    MotionJogEnabled = true;
                    Wafer.MapViewStageSyncEnable = true;
                }
            }
        }


        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    WaferAligner = this.WaferAligner();

                    SetupState = new NotCompletedState(this);

                    _AutoMapState = new IdleState(this);

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
        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = "Map";

                retVal = InitPnpModuleStage_AdvenceSetting();
                SetNodeSetupState(EnumMoudleSetupState.NONE);
                //AdvanceSetupView = new UC.WaferMapStandardAdSetting();
                //AdvanceSetupViewModel = (IPnpAdvanceSetupViewModel)AdvanceSetupView;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                LoggerManager.Exception(err);
                throw err;
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                PackagableParams.Clear();
                PackagableParams.Add(SerializeManager.SerializeToByte(MapStandardParam_IParam));
                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                retVal = await InitSetup();

                InitLightJog(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }



        public override EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public override void ViewSwapFunc(object parameter)
        {
            object swap = MainViewTarget;
            MainViewTarget = MiniViewTarget;
            MiniViewTarget = swap;

            if (MiniViewTarget is IWaferObject)
            {
                DisplayClickToMoveEnalbe = true;
                DisplayIsHitTestVisible = true;
                MiniViewHorizontalAlignment = HorizontalAlignment.Right;
                MiniViewVerticalAlignment = VerticalAlignment.Top;
            }
            else
            {
                DisplayClickToMoveEnalbe = false;
                DisplayIsHitTestVisible = false;
                MiniViewHorizontalAlignment = HorizontalAlignment.Left;
                MiniViewVerticalAlignment = VerticalAlignment.Bottom;
            }
        }
        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                //this.StageSupervisor().LoadWaferObject();
                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                ushort defaultlightvalue = 85;

                for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                {
                    CurCam.SetLight(CurCam.LightsChannels[lightindex].Type.Value, defaultlightvalue);
                }

                this.StageSupervisor().StageModuleState.WaferHighViewMove(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY(), Wafer.GetSubsInfo().ActualThickness);

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                MiniViewHorizontalAlignment = HorizontalAlignment.Left;
                MiniViewVerticalAlignment = VerticalAlignment.Bottom;
                DisplayClickToMoveEnalbe = false;
                DisplayIsHitTestVisible = false;

                //DisplayPort.EnalbeClickToMove = false;
                //DisplayPort.DisplayIsHitTestVisible = false;

                MainViewTarget = Wafer;
                MiniViewTarget = DisplayPort;

                retVal = InitPNPSetupUI();
                retVal = InitLightJog(this);

                TargetRectangleWidth = 128;
                TargetRectangleHeight = 128;

                StepLabel = $"TEST DIE COUNT : {Wafer.TestDieCount.Value}";
                Wafer.MapViewControlMode = MapViewMode.MapMode;
                Wafer.MapViewStageSyncEnable = true;
                Wafer.MapViewCurIndexVisiablity = true;

                Wafer.IsMapViewShowPMIEnable = false;
                Wafer.IsMapViewShowPMITable = false;

                IsManualSetting = true;
                IsManualEnterMode = false;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.InitSetup(): Error occurred.");
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.WAFER_SETUP_PROCEDURE_EROOR;
                throw err;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //InitStateUI();

                //PadJogLeftDown.Caption = "MANUAL";
                //PadJogLeftDown.CaptionSize = 24;
                //PadJogLeftDown.Command = new RelayCommand(SetManualTrue);

                //PadJogRightDown.Caption = "AUTO";
                //PadJogRightDown.CaptionSize = 24;
                //PadJogRightDown.Command = new RelayCommand(SetAutoTrue);

                //if (IsManualSetting)
                //{
                //    ManualMapPNPUI();
                //}
                SetUI();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        private void SetManualTrue()
        {
            try
            {
                IsManualSetting = true;
                SetUI();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void SetAutoTrue()
        {
            try
            {
                IsManualSetting = false;
                SetUI();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private Task SetEnterMode()
        {
            if (IsManualEnterMode)
            {
                IsManualEnterMode = false;
            }
            else
            {
                IsManualEnterMode = true;
                MotionJogEnabled = false;
            }
            return Task.CompletedTask;
        }


        private Task MoveMapIndex(int index)
        {
            try
            {

                //0 UP 1 DOWN 2 LEFT 3 RIGHT
                switch (index)
                {
                    case (int)EnumArrowDirection.UP:
                        Wafer.CurrentMYIndex += 1;
                        break;
                    case (int)EnumArrowDirection.DOWN:
                        Wafer.CurrentMYIndex -= 1;
                        break;
                    case (int)EnumArrowDirection.LEFT:
                        Wafer.CurrentMXIndex -= 1;
                        break;
                    case (int)EnumArrowDirection.RIGHT:
                        Wafer.CurrentMXIndex += 1;
                        break;
                    default:
                        break;
                }

                ChangeDieState(_SelectedMapSetupFunction);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                MotionJogEnabled = false;
            }
            return Task.CompletedTask;
        }

        public Task WaferAlignExecute()
        {
            try
            {
                try
                {
                    UseUserControl = UserControlFucEnum.DEFAULT;
                    //this.ViewModelManager().Lock(this.GetHashCode(), "Plese Wait", "Operate WaferAlign");

                    ICamera curcam = CurCam;
                    List<LightValueParam> lights = new List<LightValueParam>();

                    foreach (var light in curcam.LightsChannels)
                    {
                        lights.Add(new LightValueParam(light.Type.Value, (ushort)curcam.GetLight(light.Type.Value)));
                    }

                    this.WaferAligner().ClearState();
                    this.WaferAligner().DoManualOperation();

                    this.StageSupervisor().StageModuleState.WaferHighViewMove(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY(), Wafer.GetSubsInfo().ActualThickness);

                    this.VisionManager().StartGrab(curcam.GetChannelType(), this);

                    foreach (var light in lights)
                    {
                        curcam.SetLight(light.Type.Value, light.Value.Value);

                    }

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                finally
                {
                    UseUserControl = UserControlFucEnum.PTRECT;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.CompletedTask;
        }


        private void SetUI()
        {
            try
            {
                InitStateUI();

                //PadJogLeftUp.Caption = "MANUAL";
                //PadJogLeftUp.CaptionSize = 16;
                //PadJogLeftUp.Command = new RelayCommand(SetManualTrue);
                //IsManualSetting = true;
                //PadJogRightUp.Caption = "AUTO";
                //PadJogRightUp.CaptionSize = 16;
                //PadJogRightUp.Command = new RelayCommand(SetAutoTrue);

                IsManualSetting = true;
                if (IsManualSetting)
                {
                    ManualMapPNPUI();
                    EnableUseBtn();
                    PadJogLeftUp.IsEnabled = false;

                }
                else
                {
                    AutoMapPNPUI();
                    EnableUseBtn();
                    PadJogRightUp.IsEnabled = false;
                }

                if (!this.WaferAligner().IsNewSetup)
                {
                    PadJogSelect.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Apply.png");
                    PadJogSelect.IconCaption = "ALIGN";
                    PadJogSelect.Command = new AsyncCommand(WaferAlignExecute);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private EventCodeEnum ManualMapPNPUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                OneButton.SetIconSoruceBitmap(Properties.Resources.TestICon);
                OneButton.IconCaption = "TEST";
                OneButton.Command = new AsyncCommand(DrawMapTest);

                TwoButton.SetIconSoruceBitmap(Properties.Resources.MarkIcon);
                TwoButton.IconCaption = "MARK";
                TwoButton.Command = new AsyncCommand(DrawMapMark);

                ThreeButton.SetIconSoruceBitmap(Properties.Resources.SkipIcon);
                ThreeButton.IconCaption = "SKIP";
                ThreeButton.Command = new AsyncCommand(DrawMapSkip);

                FourButton.SetIconSoruceBitmap(Properties.Resources.EmptyIcon);
                FourButton.IconCaption = "EMPTY";
                FourButton.Command = new AsyncCommand(DrawMapEmpty);

                PadJogSelect.SetIconSoruceBitmap(Properties.Resources.CheckIcon);
                PadJogSelect.IconCaption = "ENTER";
                PadJogSelect.Command = new AsyncCommand(SetEnterMode);


                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/ArrowUpW.png");
                PadJogUp.Command = new AsyncCommand<int>(MoveMapIndex);
                PadJogUp.CommandParameter = EnumArrowDirection.UP;
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/ArrowDownW.png");
                PadJogDown.Command = new AsyncCommand<int>(MoveMapIndex);
                PadJogDown.CommandParameter = EnumArrowDirection.DOWN;
                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/ArrowLeftW.png");
                PadJogLeft.Command = new AsyncCommand<int>(MoveMapIndex);
                PadJogLeft.CommandParameter = EnumArrowDirection.LEFT;
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/ArrowRightW.png");
                PadJogRight.Command = new AsyncCommand<int>(MoveMapIndex);
                PadJogRight.CommandParameter = EnumArrowDirection.RIGHT;

                UseUserControl = UserControlFucEnum.DEFAULT;
                _MapModifyType = MapModifyTypeEnum.MANUAL;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.ManualMapPNPUI(): Error occurred.");
                throw err;
            }
            return retVal;
        }

        private EventCodeEnum AutoMapPNPUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {



                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");

                PadJogLeft.Command = new RelayCommand(UCDisplayRectWidthMinus);
                PadJogRight.Command = new RelayCommand(UCDisplayRectWidthPlus);
                PadJogUp.Command = new RelayCommand(UCDisplayRectHeightPlus);
                PadJogDown.Command = new RelayCommand(UCDisplayRectHeightMinus);

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;

                OneButton.Caption = "EDGE";
                //TwoButton.MaskingLevel = 3;
                OneButton.Command = new AsyncCommand(AutoEdgeMapGenerate);
                //OneButton.IsEnabled = false;

                TwoButton.Caption = "FULL";
                //ThreeButton.MaskingLevel = 3;
                TwoButton.Command = new AsyncCommand(AutoFullMapGenerate);
                //TwoButton.IsEnabled = false;


                PadJogRightDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PnpAbort.png");
                PadJogRightDown.Command = new RelayCommand(AutoMapAbort);

                EnableUseBtn();

                PadJogRightDown.IsEnabled = false;

                UseUserControl = UserControlFucEnum.PTRECT;
                _MapModifyType = MapModifyTypeEnum.AUTO;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.AutoMapPNPUI(): Error occurred.");
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new WA_MapParam_Standard();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(WA_MapParam_Standard));

                if (RetVal == EventCodeEnum.NONE)
                {
                    MapStandardParam_IParam = tmpParam;
                    MapStandardParam_Clone = MapStandardParam_IParam as WA_MapParam_Standard;
                }

                if (FocusParam != null)
                {
                    FocusParam.FocusingAxis.Value = EnumAxisConstants.Z;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return RetVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                // TODO:
                if (MapStandardParam_Clone != null)
                {
                    MapStandardParam_IParam = MapStandardParam_Clone;
                    RetVal = this.SaveParameter(MapStandardParam_IParam);
                }
                this.StageSupervisor().SaveWaferObject();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return RetVal;
        }

        public void ApplyParams(List<byte[]> datas)
        {
            try
            {
                PackagableParams = datas;

                foreach (var param in datas)
                {
                    object target;
                    SerializeManager.DeserializeFromByte(param, out target, typeof(WA_MapParam_Standard));
                    if (target != null)
                    {
                        MapStandardParam_IParam = (WA_MapParam_Standard)target;
                        MapStandardParam_Clone = (WA_MapParam_Standard)target;
                        break;
                    }
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public MapStandard()
        {

        }

        public async Task<EventCodeEnum> Modify()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                // MovingState.Moving();
                switch (ModifyCondition)
                {
                    case WADrawMapSetupFunction.DRAWAUTOMAP:
                        //SetAllJogBtnDisable();
                        //RetVal = AutoMapGernerate().Result;
                        RetVal = await AutoMapGernerate();
                        //EnableUseBtn();
                        //SetAllJogBtnEnable();
                        break;
                    case WADrawMapSetupFunction.CHANGEMARK:
                        RetVal = ChangeDieState(ModifyCondition);
                        break;
                    case WADrawMapSetupFunction.CHANGETEST:
                        RetVal = ChangeDieState(ModifyCondition);
                        break;
                    case WADrawMapSetupFunction.CHANGEEMPTY:
                        RetVal = ChangeDieState(ModifyCondition);
                        break;
                    case WADrawMapSetupFunction.CHANGESKIP:
                        RetVal = ChangeDieState(ModifyCondition);
                        break;
                    case WADrawMapSetupFunction.SAVE:
                        break;
                    case WADrawMapSetupFunction.PREVSETP:
                        break;
                }
                //MovingState.Stop();
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
            return RetVal;
        }

        private EventCodeEnum ChangeDieState(WADrawMapSetupFunction state)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            List<DeviceObject> devices;
            DeviceObject dev;
            try
            {
                long xindex = Wafer.CurrentMXIndex;
                long yindex = Wafer.CurrentMYIndex;

                devices = Wafer.GetSubsInfo().Devices.ToList<DeviceObject>();

                //dev = devices.Find(item => item.DieIndexM.XIndex == CurCam.CamSystemMI.XIndex
                //        && item.DieIndexM.YIndex == CurCam.CamSystemMI.YIndex);
                dev = devices.Find(item => item.DieIndexM.XIndex == xindex &&
                                   item.DieIndexM.YIndex == yindex);

                if (dev == null)
                {
                    if ((((Wafer.GetPhysInfo().MapCountX.Value + 1) * Wafer.GetSubsInfo().ActualDieSize.Width.Value) >= Wafer.GetPhysInfo().WaferSize_um.Value)
                        || (((Wafer.GetPhysInfo().MapCountY.Value + 1) * Wafer.GetSubsInfo().ActualDieSize.Height.Value) >= Wafer.GetPhysInfo().WaferSize_um.Value))
                    {
                        return ret;
                    }

                    long maxX, minX, maxY, minY;

                    maxX = devices.Max(d => d.DieIndexM.XIndex);
                    minX = devices.Min(d => d.DieIndexM.XIndex);
                    maxY = devices.Max(d => d.DieIndexM.YIndex);
                    minY = devices.Min(d => d.DieIndexM.YIndex);

                    dev = new DeviceObject();
                    dev.DieIndexM.XIndex = xindex;
                    dev.DieIndexM.YIndex = yindex;

                    switch (state)
                    {
                        case WADrawMapSetupFunction.CHANGEMARK:
                            dev.DieType.Value = DieTypeEnum.MARK_DIE;
                            break;
                        case WADrawMapSetupFunction.CHANGETEST:
                            dev.DieType.Value = DieTypeEnum.TEST_DIE;
                            break;
                        case WADrawMapSetupFunction.CHANGEEMPTY:
                            dev.DieType.Value = DieTypeEnum.NOT_EXIST;
                            break;
                        case WADrawMapSetupFunction.CHANGESKIP:
                            dev.DieType.Value = DieTypeEnum.SKIP_DIE;
                            break;
                    }

                    Wafer.AddDevice(dev);

                    devices = Wafer.GetSubsInfo().Devices.ToList<DeviceObject>();
                    dev = devices.Find(item => item.DieIndexM.XIndex == xindex
                            && item.DieIndexM.YIndex == yindex);
                }
                else
                {
                    switch (state)
                    {
                        case WADrawMapSetupFunction.CHANGEMARK:

                            dev.DieType.Value = DieTypeEnum.MARK_DIE;
                            break;

                        case WADrawMapSetupFunction.CHANGETEST:

                            dev.DieType.Value = DieTypeEnum.TEST_DIE;
                            break;

                        case WADrawMapSetupFunction.CHANGEEMPTY:

                            dev.DieType.Value = DieTypeEnum.NOT_EXIST;

                            if (devices.FindAll(item => item.DieIndexM.XIndex == xindex).FindAll(st => st.DieType.Value != DieTypeEnum.NOT_EXIST).Count == 0)
                            {
                                Wafer.RemoveDevice(dev, xindex, -1);
                            }

                            devices = Wafer.GetSubsInfo().Devices.ToList<DeviceObject>();

                            if (devices.FindAll(item => item.DieIndexM.YIndex == yindex).FindAll(st => st.DieType.Value != DieTypeEnum.NOT_EXIST).Count == 0)
                            {
                                Wafer.RemoveDevice(dev, -1, yindex);
                            }

                            break;

                        case WADrawMapSetupFunction.CHANGESKIP:

                            dev.DieType.Value = DieTypeEnum.SKIP_DIE;
                            break;
                    }
                }

                //switch (state)
                //{
                //    case WADrawMapSetupFunction.CHANGEMARK:

                //            dev = devices.Find(item => item.DieIndexM.XIndex == xindex
                //        && item.DieIndexM.YIndex == yindex);

                //        if (dev != null)
                //            dev.DieType.Value = DieTypeEnum.MARK_DIE;
                //        break;
                //    case WADrawMapSetupFunction.CHANGETEST:

                //        dev = devices.Find(item => item.DieIndexM.XIndex == xindex
                //            && item.DieIndexM.YIndex == yindex);

                //        if (dev != null)
                //            dev.DieType.Value = DieTypeEnum.TEST_DIE;
                //        break;
                //    case WADrawMapSetupFunction.CHANGEEMPTY:

                //        dev = devices.Find(item => item.DieIndexM.XIndex == xindex
                //            && item.DieIndexM.YIndex == yindex);
                //        if (dev != null)
                //        {
                //            dev.DieType.Value = DieTypeEnum.NOT_EXIST;

                //            if (devices.FindAll(item => item.DieIndexM.XIndex == xindex)
                //                .FindAll(st => st.DieType.Value != DieTypeEnum.NOT_EXIST).Count == 0)
                //            {
                //                Wafer.RemoveDevice(dev, xindex,-1);
                //            }

                //            devices = Wafer.GetSubsInfo().Devices.ToList<DeviceObject>();

                //            if (devices.FindAll(item => item.DieIndexM.YIndex == yindex)
                //                .FindAll(st => st.DieType.Value != DieTypeEnum.NOT_EXIST).Count == 0)
                //            {
                //                Wafer.RemoveDevice(dev,-1, yindex);
                //            }
                //        }

                //        break;
                //    case WADrawMapSetupFunction.CHANGESKIP:
                //        dev = devices.Find(item => item.DieIndexM.XIndex == xindex
                //            && item.DieIndexM.YIndex == yindex);
                //        if (dev != null)
                //            dev.DieType.Value = DieTypeEnum.SKIP_DIE;
                //        break;
                //}

                Wafer.UpdateWaferObject();

                this.LoaderRemoteMediator().GetServiceCallBack()?.SetDieType(dev.DieIndexM.XIndex, dev.DieIndexM.YIndex, dev.DieType.Value);

                StepLabel = $"TEST DIE COUNT : {Wafer.TestDieCount.Value}";
                _SelectedMapSetupFunction = state;

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {

                LoggerManager.Debug("WAMapSetupStandard - DrawWaferMap Error : " + err.ToString());
            }

            return ret;
        }

        private Task<EventCodeEnum> AutoMapGernerate()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                PadJogRightDown.IsEnabled = true;


                CancelToken = new CancellationTokenSource();

                ReInitDeviceState();
                byte[,] map = Wafer.DevicesConvertByteArray();
                //retVal = await Task.Run(() => MapGernerate(ref map ));

                retVal = MapGernerate(ref map);

            }
            catch (Exception err)
            {
                LoggerManager.Debug("WAMapSetupStandard - DrawWaferMap Error : " + err.ToString());
            }
            finally
            {
                PadJogRightDown.IsEnabled = false;
                CancelToken = null;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private enum DieCornerDir
        {
            LEFTUPPER,
            RIGHTUPPER,
            RIGHTBOTTOM,
            LEFTBOTTOM
        }



        List<PatternInfomation> LeftBommtomPTs = new List<PatternInfomation>();
        List<PatternInfomation> RightBommtomPTs = new List<PatternInfomation>();
        List<PatternInfomation> RightUpperPTs = new List<PatternInfomation>();
        List<PatternInfomation> LeftUpperPTs = new List<PatternInfomation>();

        private EventCodeEnum MapGernerate(ref byte[,] map)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                List<string> direction = new List<string>();

                ICamera cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                this.VisionManager().StartGrab(cam.GetChannelType(), this);
                CurCam = cam;
                string extenstion = ".mmo";

                int pattmargin = 5;
                int grabsizex = (int)cam.GetGrabSizeWidth();
                int grabsizey = (int)cam.GetGrabSizeHeight();
                int halfgrabsizex = (int)cam.GetGrabSizeWidth() / 2;
                int halfgrabsizey = (int)cam.GetGrabSizeHeight() / 2;
                int pattsizex = (int)PatternSizeWidth;
                int pattsizey = (int)PatternSizeHeight;
                double wafer_margin = 1000;
                double waferradius;
                double testradius;

                double acindexsizex = Wafer.GetSubsInfo().ActualDieSize.Width.Value;
                double acindexsizey = Wafer.GetSubsInfo().ActualDieSize.Height.Value;
                double acdiesizex = Wafer.GetSubsInfo().ActualDeviceSize.Width.Value;
                double acdiesizey = Wafer.GetSubsInfo().ActualDeviceSize.Height.Value;
                //double centercorenrx = Wafer.SubsInfo.WaferCenter.GetX()
                //    + Wafer.SubsInfo.WaferCenterToCornerX;
                double centercorenrx = Wafer.GetSubsInfo().WaferCenter.GetX()
                    + Wafer.GetPhysInfo().LowLeftCorner.GetX();
                double centercornery = Wafer.GetSubsInfo().WaferCenter.GetY()
                    + Wafer.GetPhysInfo().LowLeftCorner.GetY();

                double wSize = Wafer.GetPhysInfo().WaferSize_um.Value;


                EnumProberCam camType = cam.GetChannelType();
                int grabsizeX = (int)cam.GetGrabSizeWidth();
                int grabsizeY = (int)cam.GetGrabSizeHeight();

                int verdiecount = Convert.ToInt32(Math.Ceiling((wSize / (acindexsizex))));
                int hordiecount = Convert.ToInt32(Math.Ceiling((wSize / (acindexsizey))));

                int indexY = hordiecount;
                string RootPath = this.FileManager().FileManagerParam.DeviceParamRootDirectory +
                     "\\" + this.FileManager().FileManagerParam.DeviceName;
                //string resultbasepath = RootPath + MapStandardParam.PatternPath + "ResultImage\\";
                //string resultbasepath = RootPath + "\\WaferAlignParam\\AlignPattern\\WaferMarginPT\\";

                string basepath = RootPath + "\\" + MapStandardParam_Clone.PatternPath;
                //string basepath = RootPath + "\\WaferAlignParam\\AlignPattern\\WaferMarginPT\\";

                if (Directory.Exists(System.IO.Path.GetDirectoryName(basepath)) == false)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(basepath));
                }

                //if (Directory.Exists(System.IO.Path.GetDirectoryName(resultbasepath)) == false)
                //{
                //    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(resultbasepath));
                //}

                Point pt = new Point();

                #region ..// 패턴저장

                //=======


                //if(MapStandardParam.Patterns!= null && MapStandardParam.Patterns.Count !=4)
                //{

                //Test Remove

                DirectoryInfo dir = new DirectoryInfo(basepath);

                foreach (FileInfo fi in dir.GetFiles())
                {
                    fi.Delete();
                }


                LeftBommtomPTs.Clear();
                RightBommtomPTs.Clear();
                RightUpperPTs.Clear();
                LeftUpperPTs.Clear();

                waferradius = wSize / 2d;
                testradius = waferradius - wafer_margin;
                int offsetindex = Convert.ToInt32((waferradius - Math.Abs(centercorenrx)) / acindexsizex);

                #region  ..// CenterDie패턴저장
                ObservableCollection<LightValueParam> lightparams;

                pt = WaferAligner.GetLeftCornerPosition(
                    Wafer.GetSubsInfo().WaferCenter.GetX(),
                    Wafer.GetSubsInfo().WaferCenter.GetY());

                double movex = pt.X - Wafer.GetSubsInfo().ActualDieSize.Width.Value;
                double movey = pt.Y;
                double movez = Wafer.GetSubsInfo().WaferCenter.GetZ();

                movex = centercorenrx;
                movey = centercornery;
                RegisteImageBufferParam patternParam = null;
                //..LEFTBOTTOM

                string lbpath = basepath + "LEFTBOTTOM" + LeftBommtomPTs.Count.ToString();
                if (!System.IO.File.Exists(lbpath + extenstion))
                {
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(movex, movey, movez);
                    //FocusingModule.Focusing_Retry(FocusingType.WAFER
                    //    , FocusingParam, false, true, false);
                    patternParam = GetDisplayPortRectInfo();
                    patternParam.CamType = cam.GetChannelType();
                    patternParam.PatternPath = lbpath + extenstion;
                    patternParam.LocationX = (int)(cam.GetGrabSizeWidth() / 2) - pattmargin;
                    patternParam.LocationY = (int)(cam.GetGrabSizeHeight() / 2) - patternParam.Height + pattmargin;


                    this.VisionManager().SavePattern(patternParam);

                    // this.VisionManager().RegistPatt(new ProberInterfaces.Param.RegistePatternParam(cam.GetChannelType(),
                    //(int)(cam.GetGrabSizeWidth() / 2) - pattmargin, (int)(cam.GetGrabSizeHeight() / 2) - pattsizey, pattsizex, pattsizey + pattmargin, path+ extenstion));

                    if (CancelToken.Token.IsCancellationRequested)
                        return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;
                }
                lightparams = new ObservableCollection<LightValueParam>();
                for (int index = 0; index < cam.LightsChannels.Count; index++)
                {
                    lightparams.Add(new LightValueParam(cam.LightsChannels[index].Type.Value,
                        (ushort)cam.GetLight(cam.LightsChannels[index].Type.Value)));
                }
                PMParameter param = new PMParameter();
                MapStandardParam_Clone.PMParam.CopyTo(param);
                LeftBommtomPTs.Add(new PatternInfomation(movex, movey, param, lbpath, extenstion, cam.GetChannelType(), lightparams));
                //MapStandardParam.Patterns.Add(LeftBommtomPTs[LeftBommtomPTs.Count - 1]);

                //======================================================
                //..RIGHTBOTTOM
                string rbpath = basepath + "RIGHTBOTTOM" + RightBommtomPTs.Count.ToString();
                if (!System.IO.File.Exists(rbpath + extenstion))
                {
                    //this.StageSupervisor().StageModuleState.StageRelMove(acdiesizex, 0);
                    this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.X), -acdiesizex);

                    patternParam = GetDisplayPortRectInfo();
                    patternParam.CamType = cam.GetChannelType();
                    patternParam.PatternPath = rbpath + extenstion;
                    //patternParam.LocationX = (int)(cam.GetGrabSizeWidth() / 2) - patternParam.Width + pattmargin;
                    //patternParam.LocationY = (int)(cam.GetGrabSizeHeight() / 2) - patternParam.Height + pattmargin;
                    patternParam.LocationX += pattmargin;
                    patternParam.LocationY += pattmargin;
                    patternParam.Width += pattmargin;
                    patternParam.Height += pattmargin;

                    this.VisionManager().SavePattern(patternParam);

                    //this.VisionManager().RegistPatt(new ProberInterfaces.Param.RegistePatternParam(cam.GetChannelType(),
                    //    (int)(cam.GetGrabSizeWidth() / 2) - pattsizex + pattmargin, (int)(cam.GetGrabSizeHeight() / 2) - pattsizex + pattmargin, pattsizex, pattsizey, path+ extenstion));

                    if (CancelToken.Token.IsCancellationRequested)
                        return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;
                }

                lightparams = new ObservableCollection<LightValueParam>();
                for (int index = 0; index < cam.LightsChannels.Count; index++)
                {
                    lightparams.Add(new LightValueParam(cam.LightsChannels[index].Type.Value,
                        (ushort)cam.GetLight(cam.LightsChannels[index].Type.Value)));
                }

                param = new PMParameter();
                MapStandardParam_Clone.PMParam.CopyTo(param);

                RightBommtomPTs.Add(new PatternInfomation(movex + acindexsizex, movey, param, rbpath, extenstion, cam.GetChannelType(), lightparams));
                //MapStandardParam.Patterns.Add(RightBommtomPTs[RightBommtomPTs.Count - 1]);

                //======================================================
                //..RIGHTUPPER
                string rupath = basepath + "RIGHTUPPER" + RightUpperPTs.Count.ToString();
                if (!System.IO.File.Exists(rupath + extenstion))
                {
                    //this.StageSupervisor().StageModuleState.StageRelMove(0, acdiesizey);
                    this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Y), -acdiesizey);

                    patternParam = GetDisplayPortRectInfo();
                    patternParam.CamType = cam.GetChannelType();
                    patternParam.PatternPath = rupath + extenstion;
                    patternParam.LocationX = (int)(cam.GetGrabSizeWidth() / 2) - patternParam.Width + pattmargin;
                    patternParam.LocationY = (int)(cam.GetGrabSizeHeight() / 2) - pattmargin;

                    this.VisionManager().SavePattern(patternParam);

                    //this.VisionManager().RegistPatt(new ProberInterfaces.Param.RegistePatternParam(cam.GetChannelType(),
                    //    (int)(cam.GetGrabSizeWidth() / 2) - pattsizex + pattmargin, (int)(cam.GetGrabSizeHeight() / 2) - pattmargin, pattsizex, pattsizey, path+ extenstion));

                    if (CancelToken.Token.IsCancellationRequested)
                        return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;
                }
                lightparams = new ObservableCollection<LightValueParam>();
                for (int index = 0; index < cam.LightsChannels.Count; index++)
                {
                    lightparams.Add(new LightValueParam(cam.LightsChannels[index].Type.Value,
                        (ushort)cam.GetLight(cam.LightsChannels[index].Type.Value)));
                }
                param = new PMParameter();
                MapStandardParam_Clone.PMParam.CopyTo(param);

                RightUpperPTs.Add(new PatternInfomation(movex + acindexsizex, movey + acindexsizey, param, rupath, extenstion, cam.GetChannelType(), lightparams));
                //MapStandardParam.Patterns.Add(RightUpperPTs[RightUpperPTs.Count - 1]);

                //======================================================
                //..LEFTUPPER
                string lupath = basepath + "LEFTUPPER" + LeftUpperPTs.Count.ToString();
                if (!System.IO.File.Exists(lupath + extenstion))
                {
                    //this.StageSupervisor().StageModuleState.StageRelMove(-acdiesizex, 0);
                    this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.X), acdiesizex);


                    patternParam = GetDisplayPortRectInfo();
                    patternParam.CamType = cam.GetChannelType();
                    patternParam.PatternPath = lupath + extenstion;
                    patternParam.LocationX = (int)(cam.GetGrabSizeWidth() / 2) - pattmargin;
                    patternParam.LocationY = (int)(cam.GetGrabSizeHeight() / 2) - pattmargin;

                    this.VisionManager().SavePattern(patternParam);

                    //this.VisionManager().RegistPatt(new ProberInterfaces.Param.RegistePatternParam(cam.GetChannelType(),
                    //    (int)(cam.GetGrabSizeWidth() / 2) - pattmargin, (int)(cam.GetGrabSizeHeight() / 2) - pattmargin, pattsizex + pattmargin, pattsizey, path+ extenstion));

                    if (CancelToken.Token.IsCancellationRequested)
                        return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;
                }
                lightparams = new ObservableCollection<LightValueParam>();
                for (int index = 0; index < cam.LightsChannels.Count; index++)
                {
                    lightparams.Add(new LightValueParam(cam.LightsChannels[index].Type.Value,
                        (ushort)cam.GetLight(cam.LightsChannels[index].Type.Value)));
                }
                param = new PMParameter();
                MapStandardParam_Clone.PMParam.CopyTo(param);
                LeftUpperPTs.Add(new PatternInfomation(movex, movey + acindexsizey, param, lupath, extenstion, cam.GetChannelType(), lightparams));
                //MapStandardParam.Patterns.Add(LeftUpperPTs[LeftUpperPTs.Count - 1]);
                //======================================================

                SaveDevParameter();
                #endregion

                //}
                //else
                //{
                //    LeftBommtomPTs.Add(MapStandardParam.Patterns[0]);
                //    RightBommtomPTs.Add(MapStandardParam.Patterns[1]);
                //    RightUpperPTs.Add(MapStandardParam.Patterns[2]);
                //    LeftUpperPTs.Add(MapStandardParam.Patterns[3]);
                //}

                #endregion

                #region 

                if (_AutoMapGernerate == AutoMapGernerateEnum.EDGE
                    || _AutoMapGernerate == AutoMapGernerateEnum.FULL)
                {
                    _AutoMapState = new EdgeState(this);
                    retVal = _AutoMapState.DoGenerate(cam);
                }

                if (retVal == EventCodeEnum.NONE &&
                    _AutoMapGernerate == AutoMapGernerateEnum.FULL)
                {
                    _AutoMapState = new FullState(this);
                    retVal = _AutoMapState.DoGenerate(cam);
                }

                this.StageSupervisor().StageModuleState.WaferHighViewMove(
                    Wafer.GetSubsInfo().WaferCenter.GetX(),
                    Wafer.GetSubsInfo().WaferCenter.GetY(),
                    Wafer.GetSubsInfo().WaferCenter.GetZ());

                #endregion


            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. MapStandard - MapGernerate() Error occured.");
            }

            return retVal;
        }

        private EventCodeEnum EdgeGernerate(ICamera cam)
        {
            MapStandardAutoMapGenerateState automapstate =
                    new UpperState(this);

            return automapstate.DoGenerate(cam);
        }

        private EventCodeEnum FullGernerate(ICamera cam)
        {
            MapStandardAutoMapGenerateState automapstate =
                    new FullState(this);

            return automapstate.DoGenerate(cam);
        }

        public EventCodeEnum DoGernerate(DeviceEdgePMPos type, ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                byte[,] map = this.StageSupervisor().WaferObject.DevicesConvertByteArray();
                switch (type)
                {
                    case DeviceEdgePMPos.EDGE:
                        retVal = InspectEdge(ref map, cam);
                        break;
                    case DeviceEdgePMPos.UPPER:
                        retVal = InspectUpper(ref map, cam);
                        break;
                    case DeviceEdgePMPos.LEFT:
                        retVal = InspectLeft(ref map, cam);
                        break;
                    case DeviceEdgePMPos.BOTTOM:
                        retVal = InspectBottom(ref map, cam);
                        break;
                    case DeviceEdgePMPos.RIGHT:
                        retVal = InspectRight(ref map, cam);
                        break;
                    case DeviceEdgePMPos.FULL:
                        retVal = InspectFull(ref map, cam);
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.DoGernerate() : Error occured");
            }
            return retVal;
        }


        private EventCodeEnum InspectEdge(ref byte[,] map, ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = InspectUpper(ref map, cam);
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = InspectLeft(ref map, cam);
                }
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = InspectBottom(ref map, cam);
                }
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = InspectRight(ref map, cam);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.InspcetEdge() : Error occured");
            }
            return retVal;
        }

        private EventCodeEnum InspectUpper(ref byte[,] map, ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Direction dire = Direction.UNDIFIND;
                long verdiecount = Wafer.GetPhysInfo().MapCountY.Value;
                long hordiecount = Wafer.GetPhysInfo().MapCountX.Value;

                long xIndex = 0;
                long yIndex = 0;
                long EdgeDiexIndex = 0;
                long EdgeDieyIndex = 0;

                bool endflag = false;
                bool isEndDie = false;
                WaferCoordinate wcoord;
                PMResult pmresult;
                Size PatternSize;

                string path = null;
                double acdiesizex = Wafer.GetSubsInfo().ActualDeviceSize.Width.Value;
                double acdiesizey = Wafer.GetSubsInfo().ActualDeviceSize.Height.Value;

                double grabSizeX = cam.GetGrabSizeWidth();
                double grabSizeY = cam.GetGrabSizeHeight();

                bool breakflag = true;

                yIndex = verdiecount - 1;

                //DieTypeEnum type = Wafer.GetDevices().Find(device => device.DieIndexM.XIndex.Value == 22 && device.DieIndexM.YIndex.Value == 34).DieType.Value;

                while (breakflag)
                {
                    for (xIndex = hordiecount - 1; xIndex > 0; xIndex--)
                    {
                        if (map[xIndex, yIndex] == (int)DieTypeEnum.TEST_DIE)
                        {
                            EdgeDiexIndex = xIndex;
                            EdgeDieyIndex = yIndex;
                            breakflag = false;
                            break;
                        }

                    }
                    yIndex--;
                }

                yIndex = verdiecount;

                yIndex = EdgeDieyIndex;
                while (!endflag)
                {
                    dire = Direction.UPPER;
                    for (xIndex = EdgeDiexIndex; xIndex > 0; xIndex--)
                    {
                        if (xIndex == EdgeDiexIndex && yIndex == EdgeDieyIndex)
                        {
                            isEndDie = true;
                        }
                        else if (map[xIndex - 1, EdgeDieyIndex] == (int)DieTypeEnum.MARK_DIE)
                        {
                            isEndDie = true;
                        }
                        if (map[xIndex, yIndex] == (int)DieTypeEnum.TEST_DIE)
                        {
                            dire = Direction.UPPER;
                        }
                        bool isrightsucess = false;
                        wcoord = WaferAligner.MachineIndexConvertToDieLeftCorner(Convert.ToInt32(xIndex), Convert.ToInt32(yIndex));
                        LoggerManager.Debug($"AutoMap - Move XIndex : [{xIndex}],  YIndex : [{yIndex}]");
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(wcoord.X.Value + (acdiesizex),
                           wcoord.Y.Value + (acdiesizey), Wafer.GetSubsInfo().WaferCenter.GetZ());
                        //wcoord = WaferAligner.MIndexToWPos(Convert.ToInt32(xIndex), Convert.ToInt32(yIndex));
                        //this.StageSupervisor().StageModuleState.WaferHighViewMove(wcoord.X.Value + (acdiesizex),
                        //   wcoord.Y.Value + (acdiesizey));


                        for (int index = 0; index < RightUpperPTs.Count; index++)
                        {
                            path = RightUpperPTs[index].PMParameter.ModelFilePath.Value + RightUpperPTs[index].PMParameter.PatternFileExtension.Value;
                            retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                return retVal;
                            }

                            RightUpperPTs[index].PMParameter.PMAcceptance.Value = 30;
                            pmresult = this.VisionManager().SearchPatternMatching(RightUpperPTs[index]);

                            if (CancelToken.Token.IsCancellationRequested)
                                return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;

                            //패턴매칭 실패
                            if (pmresult.RetValue != EventCodeEnum.NONE)
                            {
                                if (index == RightUpperPTs.Count - 1)
                                {

                                    if (map[xIndex, yIndex] != (int)DieTypeEnum.MARK_DIE)
                                    {
                                        ModifyDevideTestToMark(map, xIndex, yIndex);
                                    }

                                    if (isEndDie)
                                    {
                                        if (dire == Direction.RIGHT && xIndex > EdgeDiexIndex)
                                        {
                                            xIndex = EdgeDiexIndex;
                                            yIndex = EdgeDieyIndex;
                                            dire = Direction.UPPER;
                                            isEndDie = false;
                                        }
                                        else if (dire == Direction.UPPER)
                                        {
                                            if (map[xIndex - 1, yIndex - 1] == (int)DieTypeEnum.MARK_DIE)
                                            {
                                                yIndex = EdgeDieyIndex;
                                                dire = Direction.LEFT;
                                            }
                                            else if (yIndex > EdgeDieyIndex)
                                            {
                                                yIndex = EdgeDieyIndex;
                                                if (xIndex >= EdgeDiexIndex)
                                                    xIndex = EdgeDiexIndex;

                                                else
                                                    xIndex += 2;
                                                // xIndex--;
                                                isEndDie = false;
                                                dire = Direction.RIGHT;
                                            }
                                            else
                                            {
                                                //yIndex = EdgeDieyIndex;
                                                //isEndDie = false;
                                                xIndex++;
                                                yIndex--;

                                                isEndDie = false;
                                                dire = Direction.BOTTOM;
                                            }
                                        }
                                        else if (dire == Direction.LEFT)
                                        {
                                            endflag = true;
                                            xIndex = -1;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (dire == Direction.UPPER && yIndex > EdgeDieyIndex)
                                        {
                                            yIndex = EdgeDieyIndex;
                                            dire = Direction.LEFT;
                                        }
                                        else if (dire == Direction.UPPER)
                                        {
                                            xIndex++;
                                            yIndex--;
                                            dire = Direction.BOTTOM;
                                        }
                                        else if (dire == Direction.BOTTOM)
                                        {
                                            xIndex++;
                                            yIndex--;
                                        }
                                    }

                                    break;
                                }
                            }
                            else // 패턴매칭 성공
                            {
                                if (isEndDie)
                                {
                                    if (dire == Direction.UPPER && xIndex >= EdgeDiexIndex
                                        && yIndex > EdgeDieyIndex)
                                    {
                                        yIndex = EdgeDieyIndex;
                                        xIndex += 2;
                                        dire = Direction.RIGHT;
                                    }
                                    else if (dire == Direction.RIGHT)
                                    {
                                        if (map[xIndex, yIndex] == (int)DieTypeEnum.MARK_DIE)
                                        {
                                            ModifyDevideMarkToTest(map, xIndex, yIndex);

                                        }
                                        dire = Direction.UPPER;
                                        xIndex++;
                                        yIndex++;
                                        //isrightsucess = true;
                                    }
                                    else if (dire == Direction.UPPER || dire == Direction.LEFT)
                                    {
                                        isrightsucess = true;
                                    }

                                }
                                else
                                {
                                    isrightsucess = true;
                                }
                                break;
                            }
                        }
                        if (isrightsucess)
                        {
                            //this.StageSupervisor().StageModuleState.StageRelMove(-(acdiesizex), 0);

                            this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.X), acdiesizex);
                            for (int index = 0; index < LeftUpperPTs.Count; index++)
                            {
                                path = LeftUpperPTs[index].PMParameter.ModelFilePath.Value
                                     + LeftUpperPTs[index].PMParameter.PatternFileExtension.Value;

                                retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                                if (retVal != EventCodeEnum.NONE)
                                {
                                    return retVal;
                                }

                                pmresult = this.VisionManager().SearchPatternMatching(LeftUpperPTs[index]);

                                if (CancelToken.Token.IsCancellationRequested)
                                    return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;

                                //패턴매칭 실패
                                if (pmresult.RetValue != EventCodeEnum.NONE)
                                {
                                    if (index == LeftUpperPTs.Count - 1)
                                    {
                                        if (map[xIndex, yIndex] != (int)DieTypeEnum.MARK_DIE)
                                        {
                                            ModifyDevideTestToMark(map, xIndex, yIndex);
                                        }

                                        if (isEndDie)
                                        {
                                            if (dire == Direction.RIGHT || dire == Direction.UPPER)
                                            {
                                                xIndex = EdgeDiexIndex;
                                                yIndex--;
                                            }
                                            else if (dire == Direction.LEFT)
                                            {
                                                endflag = true;
                                                xIndex = -1;
                                            }
                                        }
                                        else
                                        {
                                            //xIndex++;
                                            yIndex--;
                                        }
                                        break;
                                    }

                                }
                                else //패턴매칭 성공
                                {
                                    if (isEndDie)
                                    {
                                        if (dire == Direction.RIGHT)
                                        {
                                            if (map[xIndex, yIndex] == (int)DieTypeEnum.MARK_DIE)
                                            {
                                                ModifyDevideMarkToTest(map, xIndex, yIndex);
                                            }
                                            xIndex += 2;
                                        }
                                        else if (dire == Direction.LEFT)
                                        {
                                            if (map[xIndex, yIndex] == (int)DieTypeEnum.MARK_DIE)
                                            {
                                                ModifyDevideMarkToTest(map, xIndex, yIndex);
                                            }
                                            dire = Direction.UPPER;
                                            xIndex++;
                                            yIndex++;
                                        }
                                        else if (dire == Direction.UPPER)
                                        {
                                            if (yIndex < EdgeDieyIndex)
                                            {
                                                endflag = true;
                                                xIndex = -1;
                                                break;
                                            }
                                            else
                                            {
                                                if (map[xIndex, yIndex] == (int)DieTypeEnum.MARK_DIE)
                                                {
                                                    ModifyDevideMarkToTest(map, xIndex, yIndex);
                                                }
                                                xIndex++;
                                                yIndex++;
                                            }


                                        }

                                    }
                                    else
                                    {
                                        if (map[xIndex - 1, EdgeDieyIndex] == (int)DieTypeEnum.MARK_DIE)
                                        {
                                            isEndDie = true;
                                        }

                                        if (dire == Direction.UPPER && yIndex >= EdgeDieyIndex || dire == Direction.LEFT)
                                        {
                                            xIndex++;
                                            yIndex++;
                                        }
                                        else if (dire == Direction.BOTTOM)
                                        {
                                            yIndex = EdgeDieyIndex;
                                            dire = Direction.UPPER;
                                        }
                                        else
                                        {
                                            yIndex = EdgeDieyIndex;
                                        }
                                    }

                                    break;
                                }
                            }
                        }

                        if (CancelToken != null)
                        {
                            if (CancelToken.IsCancellationRequested)
                            {
                                CancelToken.Dispose();
                                retVal = EventCodeEnum.UNDEFINED;
                                return retVal;
                            }
                        }
                    }

                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {

                LoggerManager.Debug($"{err.ToString()}.InspectRULU() : Error occured");
            }
            return retVal;
        }

        private EventCodeEnum InspectLeft(ref byte[,] map, ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Direction dire = Direction.UNDIFIND;
                List<DeviceObject> devices = Wafer.GetSubsInfo().Devices.ToList<DeviceObject>();
                long verdiecount = Wafer.GetPhysInfo().MapCountY.Value;
                long hordiecount = Wafer.GetPhysInfo().MapCountX.Value;

                long xIndex = 0;
                long yIndex = 0;
                long EdgeDiexIndex = 0;
                long EdgeDieyIndex = 0;

                WaferCoordinate wcoord;
                PMResult pmresult = null;

                string path = null;
                double acdiesizex = Wafer.GetSubsInfo().ActualDeviceSize.Width.Value;
                double acdiesizey = Wafer.GetSubsInfo().ActualDeviceSize.Height.Value;
                double grabSizeX = cam.GetGrabSizeWidth();
                double grabSizeY = cam.GetGrabSizeHeight();
                Size PatternSize;

                bool breakflag = true;

                yIndex = verdiecount - 1;
                bool isupperEdge = true;
                bool isbottomEdge = false;
                while (breakflag)
                {
                    if (isbottomEdge)
                    {
                        xIndex = -2;
                        breakflag = false;
                    }
                    for (xIndex = 0; xIndex < hordiecount; xIndex++)
                    {
                        if (map[xIndex, yIndex] == (int)DieTypeEnum.TEST_DIE)
                        {
                            if (!isupperEdge)
                            {
                                EdgeDiexIndex = xIndex;
                                EdgeDieyIndex = yIndex;
                                breakflag = false;
                                break;
                            }
                            else
                            {
                                isupperEdge = false;
                                break;
                            }

                        }


                    }
                    yIndex--;
                }

                long preyIndex = 0;

                for (yIndex = EdgeDieyIndex; yIndex > 0; yIndex--)
                {
                    if (yIndex != preyIndex)
                    {
                        for (int index = 0; index < hordiecount - 1; index++)
                        {
                            if (map[index, yIndex] == (int)DieTypeEnum.TEST_DIE)
                            {
                                xIndex = index;
                                EdgeDiexIndex = index;
                                dire = Direction.LEFT;

                                if ((yIndex - 2) >= 0)
                                {
                                    int count = devices.FindAll(x => x.DieIndexM.YIndex == yIndex - 2).FindAll(x => x.DieType.Value == DieTypeEnum.TEST_DIE).Count;
                                    if (count == 0)
                                    {
                                        isbottomEdge = true;
                                    }
                                }
                                break;
                            }
                        }
                    }

                    preyIndex = yIndex;

                    bool isuppersucess = false;
                    wcoord = WaferAligner.MachineIndexConvertToDieLeftCorner(Convert.ToInt32(xIndex), Convert.ToInt32(yIndex));
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(
                        wcoord.X.Value, wcoord.Y.Value + (acdiesizey), Wafer.GetSubsInfo().WaferCenter.GetZ());

                    for (int index = 0; index < LeftUpperPTs.Count; index++)
                    {
                        path = LeftUpperPTs[index].PMParameter.ModelFilePath.Value + LeftUpperPTs[index].PMParameter.PatternFileExtension.Value;
                        retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                        if (retVal != EventCodeEnum.NONE)
                        {
                            return retVal;
                        }

                        pmresult = this.VisionManager().SearchPatternMatching(LeftUpperPTs[index]);

                        if (CancelToken.Token.IsCancellationRequested)
                            return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;

                        if (pmresult.RetValue != EventCodeEnum.NONE)
                        {
                            if (map[xIndex, yIndex] != (int)DieTypeEnum.MARK_DIE)
                            {
                                ModifyDevideTestToMark(map, xIndex, yIndex);
                            }

                            if (dire == Direction.LEFT && xIndex >= EdgeDiexIndex
                                 || dire == Direction.RIGHT)
                            {
                                yIndex++;
                                xIndex++;
                                dire = Direction.RIGHT;
                            }
                            break;
                        }
                        else if (pmresult.RetValue == EventCodeEnum.NONE)
                        {
                            isuppersucess = true;
                            break;
                        }
                    }
                    if (isuppersucess)
                    {
                        //this.StageSupervisor().StageModuleState.StageRelMove(0, -acdiesizey);
                        this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Y), acdiesizey);
                        for (int index = 0; index < LeftBommtomPTs.Count; index++)
                        {
                            path = LeftBommtomPTs[index].PMParameter.ModelFilePath.Value
                                  + LeftBommtomPTs[index].PMParameter.PatternFileExtension.Value;

                            retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                return retVal;
                            }

                            pmresult = this.VisionManager().SearchPatternMatching(LeftBommtomPTs[index]);

                            if (CancelToken.Token.IsCancellationRequested)
                                return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;


                            if (pmresult.RetValue != EventCodeEnum.NONE)
                            {
                                if (map[xIndex, yIndex] != (int)DieTypeEnum.MARK_DIE)
                                {
                                    ModifyDevideTestToMark(map, xIndex, yIndex);
                                }

                                if (dire == Direction.LEFT && xIndex >= EdgeDiexIndex)
                                {
                                    yIndex++;
                                    xIndex++;
                                    dire = Direction.RIGHT;
                                }
                                else
                                {
                                    yIndex++;
                                }
                                break;
                            }
                            else if (pmresult.RetValue == EventCodeEnum.NONE)
                            {
                                if (dire == Direction.LEFT)
                                {
                                    if (map[xIndex, yIndex] == (int)DieTypeEnum.MARK_DIE)
                                    {
                                        ModifyDevideMarkToTest(map, xIndex, yIndex);
                                    }

                                    if (isbottomEdge)
                                    {
                                        yIndex = -2;
                                    }
                                    else
                                    {
                                        yIndex++;
                                        xIndex--;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    if (CancelToken != null)
                    {
                        if (CancelToken.IsCancellationRequested)
                        {
                            CancelToken.Dispose();
                            retVal = EventCodeEnum.UNDEFINED;
                            return retVal;
                        }
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.InspectLeft() : Error occured");
            }
            return retVal;
        }

        private EventCodeEnum InspectBottom(ref byte[,] map, ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Direction dire = Direction.UNDIFIND;
                long verdiecount = Wafer.GetPhysInfo().MapCountY.Value;
                long hordiecount = Wafer.GetPhysInfo().MapCountX.Value;

                long xIndex = 0;
                long yIndex = 0;
                long EdgeDiexIndex = 0;
                long EdgeDieyIndex = 0;

                bool endflag = false;
                bool isEndDie = false;
                WaferCoordinate wcoord;
                PMResult pmresult;

                string path = null;
                double acdiesizex = Wafer.GetSubsInfo().ActualDeviceSize.Width.Value;
                double acdiesizey = Wafer.GetSubsInfo().ActualDeviceSize.Height.Value;

                double grabSizeX = cam.GetGrabSizeWidth();
                double grabSizeY = cam.GetGrabSizeHeight();
                Size PatternSize;

                bool breakflag = true;
                yIndex = 0;

                while (breakflag)
                {
                    for (xIndex = 0; xIndex < verdiecount - 1; xIndex++)
                    {
                        if (map[xIndex, yIndex] == (int)DieTypeEnum.TEST_DIE ||
                            map[xIndex, yIndex] == (int)DieTypeEnum.CHANGEMARK_DIE ||
                            map[xIndex, yIndex] == (int)DieTypeEnum.CHANGETEST_DIE)
                        {
                            EdgeDiexIndex = xIndex;
                            EdgeDieyIndex = yIndex;
                            breakflag = false;
                            break;
                        }

                    }
                    yIndex++;
                }

                yIndex = EdgeDieyIndex;
                while (!endflag)
                {
                    dire = Direction.BOTTOM;
                    for (xIndex = EdgeDiexIndex; xIndex < verdiecount; xIndex++)
                    {
                        if (xIndex == EdgeDiexIndex && yIndex == EdgeDieyIndex)
                        {
                            isEndDie = true;
                        }
                        else if (map[xIndex + 1, EdgeDieyIndex] == (int)DieTypeEnum.MARK_DIE)
                        {
                            isEndDie = true;
                        }

                        if (map[xIndex, yIndex] == (int)DieTypeEnum.TEST_DIE)
                        {
                            dire = Direction.BOTTOM;
                        }

                        bool isleftsucess = false;
                        wcoord = WaferAligner.MachineIndexConvertToDieLeftCorner(Convert.ToInt32(xIndex), Convert.ToInt32(yIndex));
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(wcoord.X.Value,
                           wcoord.Y.Value, Wafer.GetSubsInfo().WaferCenter.GetZ());

                        for (int index = 0; index < LeftBommtomPTs.Count; index++)
                        {
                            path = LeftBommtomPTs[index].PMParameter.ModelFilePath.Value
                                 + LeftBommtomPTs[index].PMParameter.PatternFileExtension.Value;

                            retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                return retVal;
                            }

                            pmresult = this.VisionManager().SearchPatternMatching(LeftBommtomPTs[index]);

                            if (CancelToken.Token.IsCancellationRequested)
                                return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;

                            //패턴매칭 실패
                            if (pmresult.RetValue != EventCodeEnum.NONE)
                            {
                                if (map[xIndex, yIndex] != (int)DieTypeEnum.MARK_DIE)
                                {
                                    ModifyDevideTestToMark(map, xIndex, yIndex);
                                }

                                if (isEndDie)
                                {
                                    if (dire == Direction.LEFT && xIndex < EdgeDiexIndex)
                                    {
                                        xIndex = EdgeDiexIndex;
                                        yIndex = EdgeDieyIndex;
                                        dire = Direction.BOTTOM;
                                        isEndDie = false;
                                    }
                                    else if (dire == Direction.BOTTOM)
                                    {
                                        xIndex--;
                                        yIndex++;
                                        dire = Direction.UPPER;
                                    }
                                    else if (dire == Direction.UPPER)
                                    {
                                        xIndex--;
                                        yIndex++;
                                        dire = Direction.UPPER;
                                    }
                                    else if (dire == Direction.RIGHT)
                                    {
                                        endflag = true;
                                        xIndex = -1;
                                        break;
                                    }

                                }
                                else
                                {
                                    if (dire == Direction.BOTTOM && yIndex < EdgeDieyIndex)
                                    {
                                        yIndex = EdgeDieyIndex;
                                        dire = Direction.RIGHT;
                                    }
                                    else if (dire == Direction.BOTTOM)
                                    {
                                        xIndex--;
                                        yIndex++;
                                        dire = Direction.UPPER;
                                    }
                                    else if (dire == Direction.UPPER)
                                    {
                                        xIndex--;
                                        yIndex++;
                                    }
                                }
                                break;
                            }
                            else if (pmresult.RetValue == EventCodeEnum.NONE)
                            {
                                if (isEndDie)
                                {
                                    if (dire == Direction.LEFT)
                                    {
                                        if (map[xIndex, yIndex] == (int)DieTypeEnum.MARK_DIE)
                                        {
                                            ModifyDevideMarkToTest(map, xIndex, yIndex);
                                        }
                                        dire = Direction.BOTTOM;
                                        xIndex--;
                                        yIndex--;
                                    }
                                    else if (dire == Direction.BOTTOM ||
                                        dire == Direction.RIGHT || dire == Direction.UPPER)
                                    {
                                        isleftsucess = true;
                                    }
                                }
                                else
                                {
                                    isleftsucess = true;
                                }
                                break;
                            }
                        }
                        if (isleftsucess)
                        {
                            //this.StageSupervisor().StageModuleState.StageRelMove(acdiesizex, 0);
                            this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.X), -acdiesizex);
                            for (int index = 0; index < RightBommtomPTs.Count; index++)
                            {
                                path = RightBommtomPTs[index].PMParameter.ModelFilePath.Value
                                     + RightBommtomPTs[index].PMParameter.PatternFileExtension.Value;

                                retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                                if (retVal != EventCodeEnum.NONE)
                                {
                                    return retVal;
                                }

                                pmresult = this.VisionManager().SearchPatternMatching(RightBommtomPTs[index]);

                                if (CancelToken.Token.IsCancellationRequested)
                                    return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;

                                //패턴매칭 실패
                                if (pmresult.RetValue != EventCodeEnum.NONE)
                                {
                                    if (map[xIndex, yIndex] != (int)DieTypeEnum.MARK_DIE)
                                    {
                                        ModifyDevideTestToMark(map, xIndex, yIndex);
                                    }


                                    if (isEndDie)
                                    {
                                        if (dire == Direction.LEFT)
                                        {
                                            xIndex = EdgeDiexIndex;
                                        }
                                        else if (dire == Direction.BOTTOM || dire == Direction.UPPER)
                                        {
                                            xIndex--;
                                            yIndex++;
                                            dire = Direction.UPPER;
                                        }
                                        //else if (dire == Direction.UPPER)
                                        //{
                                        //    xIndex--;
                                        //    yIndex++;
                                        //}
                                        else if (dire == Direction.RIGHT)
                                        {
                                            endflag = true;
                                            xIndex = hordiecount + 1;
                                        }

                                    }
                                    else
                                    {
                                        xIndex--;
                                        yIndex++;
                                    }
                                    break;
                                }
                                else if (pmresult.RetValue == EventCodeEnum.NONE)
                                {
                                    if (isEndDie)
                                    {
                                        if (map[xIndex, yIndex] == (int)DieTypeEnum.MARK_DIE)
                                        {
                                            ModifyDevideMarkToTest(map, xIndex, yIndex);
                                        }


                                        if (dire == Direction.LEFT)
                                        {
                                            xIndex = hordiecount + 1;
                                        }
                                        else if (dire == Direction.RIGHT)
                                        {
                                            dire = Direction.BOTTOM;
                                            xIndex--;
                                            yIndex--;
                                        }
                                        else if (dire == Direction.BOTTOM && xIndex > EdgeDiexIndex && yIndex > EdgeDieyIndex)
                                        {
                                            endflag = true;
                                            xIndex = hordiecount + 1;
                                            break;
                                        }
                                        else if (dire == Direction.BOTTOM || dire == Direction.UPPER)
                                        {
                                            yIndex = EdgeDieyIndex;
                                            isEndDie = false;
                                        }
                                    }
                                    else
                                    {
                                        if (map[xIndex + 1, EdgeDieyIndex] == (int)DieTypeEnum.MARK_DIE)
                                        {
                                            isEndDie = true;
                                        }

                                        if (dire == Direction.BOTTOM && yIndex <= EdgeDieyIndex || dire == Direction.RIGHT)
                                        {
                                            xIndex--;
                                            yIndex--;
                                        }
                                        else if (dire == Direction.UPPER)
                                        {
                                            yIndex = EdgeDieyIndex;
                                            dire = Direction.BOTTOM;
                                        }
                                        else
                                        {
                                            yIndex = EdgeDieyIndex;
                                        }
                                    }
                                    break;
                                }
                            }
                        }

                        if (CancelToken != null)
                        {
                            if (CancelToken.IsCancellationRequested)
                            {
                                CancelToken.Dispose();
                                retVal = EventCodeEnum.UNDEFINED;
                                return retVal;
                            }
                        }
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.InspectBottom() : Error occured");
            }
            return retVal;
        }

        private EventCodeEnum InspectRight(ref byte[,] map, ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Direction dire = Direction.UNDIFIND;
                List<DeviceObject> devices = Wafer.GetSubsInfo().Devices.ToList<DeviceObject>();
                long verdiecount = Wafer.GetPhysInfo().MapCountY.Value;
                long hordiecount = Wafer.GetPhysInfo().MapCountX.Value;

                long xIndex = 0;
                long yIndex = 0;
                long EdgeDiexIndex = 0;
                long EdgeDieyIndex = 0;

                WaferCoordinate wcoord;
                PMResult pmresult = null;

                string path = null;
                double acdiesizex = Wafer.GetSubsInfo().ActualDeviceSize.Width.Value;
                double acdiesizey = Wafer.GetSubsInfo().ActualDeviceSize.Height.Value;

                double grabSizeX = cam.GetGrabSizeWidth();
                double grabSizeY = cam.GetGrabSizeHeight();
                Size PatternSize;

                bool breakflag = true;

                yIndex = 0;

                bool isbottomEdge = true;
                bool isupperEdge = false;
                while (breakflag)
                {
                    for (xIndex = hordiecount - 1; xIndex > 0; xIndex--)
                    {
                        if (/*map[xIndex, yIndex] == (int)DieTypeEnum.TEST_DIE)*/
                            map[xIndex, yIndex] == (int)DieTypeEnum.TEST_DIE ||
                            map[xIndex, yIndex] == (int)DieTypeEnum.CHANGEMARK_DIE ||
                            map[xIndex, yIndex] == (int)DieTypeEnum.CHANGETEST_DIE)
                        {
                            if (!isbottomEdge)
                            {
                                EdgeDiexIndex = xIndex;
                                EdgeDieyIndex = yIndex;
                                breakflag = false;
                                break;
                            }
                            else
                            {
                                isbottomEdge = false;
                                break;
                            }

                        }
                    }
                    yIndex++;
                }

                long preyIndex = 0;
                for (yIndex = EdgeDieyIndex; yIndex < verdiecount; yIndex++)
                {
                    if (yIndex != preyIndex)
                    {
                        for (long index = hordiecount - 1; index > 0; index--)
                        {
                            if (map[index, yIndex] == (int)DieTypeEnum.TEST_DIE)
                            {
                                xIndex = index;
                                EdgeDiexIndex = index;
                                dire = Direction.RIGHT;

                                if ((yIndex + 2) <= verdiecount)
                                {
                                    int count =
                                        devices.FindAll(x => x.DieIndexM.YIndex == yIndex + 2).
                                        FindAll(x => x.DieType.Value == DieTypeEnum.TEST_DIE).Count;
                                    if (count == 0)
                                    {
                                        isupperEdge = true;
                                    }
                                }


                                break;
                            }
                        }
                    }

                    preyIndex = yIndex;
                    bool isbottomsucess = false;
                    wcoord = WaferAligner.MachineIndexConvertToDieLeftCorner(Convert.ToInt32(xIndex), Convert.ToInt32(yIndex));
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(
                        wcoord.X.Value + (acdiesizex), wcoord.Y.Value, Wafer.GetSubsInfo().WaferCenter.GetZ());

                    for (int index = 0; index < RightBommtomPTs.Count; index++)
                    {

                        path = RightBommtomPTs[index].PMParameter.ModelFilePath.Value
                            + RightBommtomPTs[index].PMParameter.PatternFileExtension.Value;

                        retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                        if (retVal != EventCodeEnum.NONE)
                        {
                            return retVal;
                        }

                        pmresult = this.VisionManager().SearchPatternMatching(RightBommtomPTs[index]);

                        if (CancelToken.Token.IsCancellationRequested)
                            return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;

                        if (pmresult.RetValue != EventCodeEnum.NONE)
                        {

                            if (map[xIndex, yIndex] != (int)DieTypeEnum.MARK_DIE)
                            {
                                ModifyDevideTestToMark(map, xIndex, yIndex);
                            }

                            if (dire == Direction.RIGHT && xIndex <= EdgeDiexIndex)
                            {
                                xIndex = EdgeDiexIndex - 1;
                                yIndex--;
                                dire = Direction.LEFT;
                            }
                            else if (dire == Direction.RIGHT && xIndex >= EdgeDiexIndex)
                            {
                                if (isupperEdge)
                                {
                                    yIndex = verdiecount + 1;
                                }
                            }
                            else if (dire == Direction.LEFT)
                            {
                                xIndex--;
                                yIndex--;
                                if (map[xIndex - 1, yIndex] == (int)DieTypeEnum.MARK_DIE)
                                {
                                    yIndex--;
                                }
                                dire = Direction.LEFT;
                            }
                            break;
                        }
                        else if (pmresult.RetValue == EventCodeEnum.NONE)
                        {
                            isbottomsucess = true;
                            break;
                        }
                    }
                    if (isbottomsucess)
                    {
                        //this.StageSupervisor().StageModuleState.StageRelMove(0, +acdiesizey);
                        this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Y), acdiesizey);

                        for (int index = 0; index < RightUpperPTs.Count; index++)
                        {
                            path = RightUpperPTs[index].PMParameter.ModelFilePath.Value
                                 + RightUpperPTs[index].PMParameter.PatternFileExtension.Value;

                            retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                return retVal;
                            }

                            pmresult = this.VisionManager().SearchPatternMatching(RightUpperPTs[index]);

                            if (CancelToken.Token.IsCancellationRequested)
                                return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;

                            if (pmresult.RetValue != EventCodeEnum.NONE)
                            {
                                if (map[xIndex, yIndex] != (int)DieTypeEnum.MARK_DIE)
                                {

                                    ModifyDevideTestToMark(map, xIndex, yIndex);
                                }

                                if (dire == Direction.RIGHT && xIndex <= EdgeDiexIndex
                                    || dire == Direction.LEFT)
                                {
                                    xIndex--;
                                    yIndex--;
                                    dire = Direction.LEFT;
                                }
                                else if (isupperEdge)
                                {
                                    yIndex = verdiecount + 1;
                                }
                                break;
                            }

                            else if (pmresult.RetValue == EventCodeEnum.NONE)
                            {
                                if (dire == Direction.RIGHT)
                                {
                                    if (xIndex > EdgeDiexIndex)
                                    {
                                        if (map[xIndex, yIndex] == (int)DieTypeEnum.MARK_DIE)
                                        {
                                            ModifyDevideMarkToTest(map, xIndex, yIndex);
                                        }
                                    }
                                    yIndex--;
                                    xIndex++;
                                }
                                else if (dire == Direction.LEFT)
                                {
                                    //if(isupperEdge)
                                    //{
                                    //    yIndex = verdiecount + 1;
                                    //}
                                }
                                break;
                            }
                        }
                    }
                    if (CancelToken != null)
                    {
                        if (CancelToken.IsCancellationRequested)
                        {
                            CancelToken.Dispose();
                            retVal = EventCodeEnum.UNDEFINED;
                            return retVal;
                        }
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.InspectRight() : Error occured");
            }
            return retVal;
        }

        private EventCodeEnum InspectFull(ref byte[,] map, ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Direction dire = Direction.UNDIFIND;
                List<DeviceObject> devices = Wafer.GetSubsInfo().Devices.ToList<DeviceObject>();
                long verdiecount = Wafer.GetPhysInfo().MapCountY.Value;
                long hordiecount = Wafer.GetPhysInfo().MapCountX.Value;

                bool continueHor = true;
                long xIndex = 0;
                string path = null;

                WaferCoordinate wcoord;
                PMResult pmresult;

                double acdiesizex = Wafer.GetSubsInfo().ActualDeviceSize.Width.Value;
                double acdiesizey = Wafer.GetSubsInfo().ActualDeviceSize.Height.Value;

                double grabSizeX = cam.GetGrabSizeWidth();
                double grabSizeY = cam.GetGrabSizeHeight();
                Size PatternSize;

                bool istesting = false;
                bool isNextSetp = false;

                for (long yIndex = verdiecount - 1; yIndex >= 0; yIndex--)
                {
                    if (dire == Direction.UNDIFIND || dire == Direction.RIGHT)
                    {
                        dire = Direction.LEFT;
                    }
                    else if (dire == Direction.LEFT)
                    {
                        dire = Direction.RIGHT;
                    }

                    if (dire == Direction.LEFT)
                    {
                        xIndex = hordiecount - 1;
                    }
                    else if (dire == Direction.RIGHT)
                    {
                        xIndex = 0;
                    }
                    int validcount = devices.FindAll(item => item.DieIndexM.YIndex == yIndex)
                             .FindAll(item => item.DieType.Value == DieTypeEnum.TEST_DIE ||
                             item.DieType.Value == DieTypeEnum.CHANGETEST_DIE).Count;
                    if (validcount != 0)
                    {
                        continueHor = true;
                        while (continueHor)
                        {
                            if (map[xIndex, yIndex] == (int)DieTypeEnum.TEST_DIE)
                            {
                                if (istesting == false)
                                    istesting = true;

                                isNextSetp = false;

                                wcoord = WaferAligner.MachineIndexConvertToDieLeftCorner(Convert.ToInt32(xIndex), Convert.ToInt32(yIndex));

                                #region //.. Direction Left

                                if (dire == Direction.LEFT)
                                {
                                    retVal = this.StageSupervisor().StageModuleState.WaferHighViewMove(wcoord.X.Value + (acdiesizex),
                                        wcoord.Y.Value + (acdiesizey), Wafer.GetSubsInfo().WaferCenter.GetZ());

                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        //RightUpper
                                        for (int index = 0; index < RightUpperPTs.Count; index++)
                                        {
                                            path = RightUpperPTs[index].PMParameter.ModelFilePath.Value
                                                + RightUpperPTs[index].PMParameter.PatternFileExtension.Value;
                                            retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                                            if (retVal != EventCodeEnum.NONE)
                                            {
                                                return retVal;
                                            }

                                            pmresult = this.VisionManager().SearchPatternMatching(RightUpperPTs[index]);

                                            if (CancelToken.Token.IsCancellationRequested)
                                                return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;

                                            if (pmresult.RetValue != EventCodeEnum.NONE)
                                            {
                                                ModifyDevideTestToMark(map, xIndex, yIndex);

                                                isNextSetp = false;


                                                if (xIndex > 0)
                                                {
                                                    xIndex--;
                                                }

                                                break;
                                            }
                                            else if (pmresult.RetValue == EventCodeEnum.NONE)
                                            {
                                                isNextSetp = true;
                                                break;

                                            }
                                        }
                                    }

                                    //RightBottom
                                    if (isNextSetp)
                                    {
                                        //retVal = this.StageSupervisor().StageModuleState.StageRelMove(0, -(acdiesizey));
                                        this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Y), acdiesizey);
                                        if (retVal == EventCodeEnum.NONE)
                                        {
                                            for (int index = 0; index < RightBommtomPTs.Count; index++)
                                            {
                                                path = RightBommtomPTs[index].PMParameter.ModelFilePath.Value
                                                     + RightBommtomPTs[index].PMParameter.PatternFileExtension.Value;

                                                retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                                                if (retVal != EventCodeEnum.NONE)
                                                {
                                                    return retVal;
                                                }

                                                pmresult = this.VisionManager().SearchPatternMatching(RightBommtomPTs[index]);

                                                if (CancelToken.Token.IsCancellationRequested)
                                                    return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;

                                                if (pmresult.RetValue != EventCodeEnum.NONE)
                                                {
                                                    ModifyDevideTestToMark(map, xIndex, yIndex);

                                                    isNextSetp = false;

                                                    if (xIndex > 0)
                                                    {
                                                        xIndex--;
                                                    }
                                                    break;
                                                }
                                                else if (pmresult.RetValue == EventCodeEnum.NONE)
                                                {
                                                    isNextSetp = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    //LeftBottom
                                    if (isNextSetp)
                                    {
                                        //retVal = this.StageSupervisor().StageModuleState.StageRelMove(-(acdiesizex), 0);
                                        this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.X), acdiesizex);

                                        if (retVal == EventCodeEnum.NONE)
                                        {
                                            for (int index = 0; index < LeftBommtomPTs.Count; index++)
                                            {
                                                path = LeftBommtomPTs[index].PMParameter.ModelFilePath.Value
                                                     + LeftBommtomPTs[index].PMParameter.PatternFileExtension.Value;

                                                retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                                                if (retVal != EventCodeEnum.NONE)
                                                {
                                                    return retVal;
                                                }


                                                pmresult = this.VisionManager().SearchPatternMatching(LeftBommtomPTs[index]);

                                                if (CancelToken.Token.IsCancellationRequested)
                                                    return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;

                                                if (pmresult.RetValue != EventCodeEnum.NONE)
                                                {
                                                    ModifyDevideTestToMark(map, xIndex, yIndex);
                                                    isNextSetp = false;

                                                    if (xIndex > 0)
                                                    {
                                                        xIndex--;
                                                    }
                                                    break;
                                                }
                                                else if (pmresult.RetValue == EventCodeEnum.NONE)
                                                {
                                                    isNextSetp = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    //LeftUpper
                                    if (isNextSetp)
                                    {
                                        //retVal = this.StageSupervisor().StageModuleState.StageRelMove(0, acdiesizey);
                                        this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Y), -acdiesizey);

                                        if (retVal == EventCodeEnum.NONE)
                                        {
                                            for (int index = 0; index < LeftUpperPTs.Count; index++)
                                            {
                                                path = LeftUpperPTs[index].PMParameter.ModelFilePath.Value
                                                     + LeftUpperPTs[index].PMParameter.PatternFileExtension.Value;

                                                retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                                                if (retVal != EventCodeEnum.NONE)
                                                {
                                                    return retVal;
                                                }

                                                pmresult = this.VisionManager().SearchPatternMatching(LeftUpperPTs[index]);

                                                if (CancelToken.Token.IsCancellationRequested)
                                                    return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;

                                                if (pmresult.RetValue != EventCodeEnum.NONE)
                                                {
                                                    ModifyDevideTestToMark(map, xIndex, yIndex);

                                                    isNextSetp = false;

                                                    if (xIndex > 0)
                                                    {
                                                        xIndex--;
                                                    }
                                                    break;
                                                }
                                                else if (pmresult.RetValue == EventCodeEnum.NONE)
                                                {
                                                    xIndex--;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region //.. Direction Right

                                else if (dire == Direction.RIGHT)
                                {

                                    retVal = this.StageSupervisor().StageModuleState.WaferHighViewMove(wcoord.X.Value,
                                             wcoord.Y.Value + (acdiesizey), Wafer.GetSubsInfo().WaferCenter.GetZ());

                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        //LeftUpper
                                        if (retVal == EventCodeEnum.NONE)
                                        {
                                            for (int index = 0; index < LeftUpperPTs.Count; index++)
                                            {
                                                path = LeftUpperPTs[index].PMParameter.ModelFilePath.Value
                                                    + LeftUpperPTs[index].PMParameter.PatternFileExtension.Value;

                                                retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                                                if (retVal != EventCodeEnum.NONE)
                                                {
                                                    return retVal;
                                                }

                                                pmresult = this.VisionManager().SearchPatternMatching(LeftUpperPTs[index]);

                                                if (CancelToken.Token.IsCancellationRequested)
                                                    return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;

                                                if (pmresult.RetValue != EventCodeEnum.NONE)
                                                {
                                                    ModifyDevideTestToMark(map, xIndex, yIndex);

                                                    isNextSetp = false;

                                                    if (xIndex < hordiecount)
                                                    {
                                                        xIndex++;
                                                    }
                                                    break;
                                                }
                                                else if (pmresult.RetValue == EventCodeEnum.NONE)
                                                {
                                                    isNextSetp = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    //LeftBottom
                                    if (isNextSetp)
                                    {
                                        //retVal = this.StageSupervisor().StageModuleState.StageRelMove(0, -(acdiesizey));
                                        this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Y), acdiesizey);
                                        if (retVal == EventCodeEnum.NONE)
                                        {
                                            for (int index = 0; index < LeftBommtomPTs.Count; index++)
                                            {
                                                path = LeftBommtomPTs[index].PMParameter.ModelFilePath.Value
                                                     + LeftBommtomPTs[index].PMParameter.PatternFileExtension.Value;

                                                retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                                                if (retVal != EventCodeEnum.NONE)
                                                {
                                                    return retVal;
                                                }

                                                pmresult = this.VisionManager().SearchPatternMatching(LeftBommtomPTs[index]);

                                                if (CancelToken.Token.IsCancellationRequested)
                                                    return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;

                                                if (pmresult.RetValue != EventCodeEnum.NONE)
                                                {
                                                    ModifyDevideTestToMark(map, xIndex, yIndex);

                                                    isNextSetp = false;
                                                    if (dire == Direction.LEFT)
                                                    {
                                                        if (xIndex > 0)
                                                        {
                                                            xIndex--;
                                                        }
                                                    }
                                                    else if (dire == Direction.RIGHT)
                                                    {
                                                        if (xIndex < Wafer.GetPhysInfo().MapCountX.Value)
                                                        {
                                                            xIndex++;
                                                        }
                                                    }
                                                    break;
                                                }
                                                else if (pmresult.RetValue == EventCodeEnum.NONE)
                                                {

                                                    isNextSetp = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    //RightBottom
                                    if (isNextSetp)
                                    {
                                        //retVal = this.StageSupervisor().StageModuleState.StageRelMove((acdiesizex), 0);
                                        this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.X), -acdiesizex);
                                        if (retVal == EventCodeEnum.NONE)
                                        {
                                            for (int index = 0; index < RightBommtomPTs.Count; index++)
                                            {
                                                path = RightBommtomPTs[index].PMParameter.ModelFilePath.Value
                                                    + RightBommtomPTs[index].PMParameter.PatternFileExtension.Value;

                                                retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                                                if (retVal != EventCodeEnum.NONE)
                                                {
                                                    return retVal;
                                                }

                                                pmresult = this.VisionManager().SearchPatternMatching(RightBommtomPTs[index]);

                                                if (CancelToken.Token.IsCancellationRequested)
                                                    return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;


                                                if (pmresult.RetValue != EventCodeEnum.NONE)
                                                {
                                                    ModifyDevideTestToMark(map, xIndex, yIndex);

                                                    isNextSetp = false;

                                                    if (xIndex < Wafer.GetPhysInfo().MapCountX.Value)
                                                    {
                                                        xIndex++;
                                                    }
                                                    break;
                                                }
                                                else if (pmresult.RetValue == EventCodeEnum.NONE)
                                                {

                                                    isNextSetp = true;
                                                    break;

                                                }
                                            }
                                        }
                                        else
                                        {
                                            return retVal;
                                        }
                                    }

                                    //RightUpper
                                    if (isNextSetp)
                                    {
                                        //retVal = this.StageSupervisor().StageModuleState.StageRelMove(0, acdiesizey);
                                        this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Y), -acdiesizey);

                                        if (retVal == EventCodeEnum.NONE)
                                        {
                                            for (int index = 0; index < RightUpperPTs.Count; index++)
                                            {
                                                path = RightUpperPTs[index].PMParameter.ModelFilePath.Value
                                                     + RightUpperPTs[index].PMParameter.PatternFileExtension.Value;
                                                retVal = this.VisionManager().GetPatternSize(path, out PatternSize);

                                                if (retVal != EventCodeEnum.NONE)
                                                {
                                                    return retVal;
                                                }

                                                pmresult = this.VisionManager().SearchPatternMatching(RightUpperPTs[index]);

                                                if (CancelToken.Token.IsCancellationRequested)
                                                    return EventCodeEnum.AUTO_MAP_GERNERATE_USER_CANCEL;

                                                if (pmresult.RetValue != EventCodeEnum.NONE)
                                                {
                                                    ModifyDevideTestToMark(map, xIndex, yIndex);
                                                    isNextSetp = false;

                                                    if (xIndex < hordiecount)
                                                    {
                                                        xIndex++;
                                                    }
                                                    break;
                                                }
                                                else if (pmresult.RetValue == EventCodeEnum.NONE)
                                                {
                                                    xIndex++;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                if (dire == Direction.LEFT)
                                {
                                    if (xIndex > 0)
                                    {
                                        xIndex--;
                                    }
                                    else
                                    {
                                        if (yIndex > 0)
                                        {
                                            break;
                                        }
                                    }
                                }
                                else if (dire == Direction.RIGHT)
                                {
                                    if (xIndex < hordiecount - 1)
                                    {
                                        xIndex++;
                                    }
                                    else
                                    {
                                        if (yIndex > 0)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            if (CancelToken != null)
                            {
                                if (CancelToken.IsCancellationRequested)
                                {
                                    CancelToken.Dispose();
                                    retVal = EventCodeEnum.UNDEFINED;
                                    return retVal;
                                }
                            }
                        }
                    }
                    else
                    {
                        dire = Direction.UNDIFIND;
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.InspectFull() : Error occured");
            }
            return retVal;
        }

        private EventCodeEnum ModifyDevideTestToMark(byte[,] map, long xindex, long yindex)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                List<DeviceObject> devices = Wafer.GetSubsInfo().Devices.ToList<DeviceObject>();
                map[xindex, yindex] = (int)DieTypeEnum.CHANGEMARK_DIE;
                devices.Find(item => item.DieIndexM.XIndex == xindex && item.DieIndexM.YIndex == yindex)
                            .DieType.Value = DieTypeEnum.CHANGEMARK_DIE;
                //devices.Find(item => item.DieIndexM.XIndex.Value == xindex && item.DieIndexM.YIndex.Value == yindex)
                //        .State.Value = DieStateEnum.CHANGEDMARK;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.ModifyDevideTestToMark() : Error occured");
            }
            return retVal;
        }

        private EventCodeEnum ModifyDevideMarkToTest(byte[,] map, long xindex, long yindex)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                List<DeviceObject> devices = Wafer.GetSubsInfo().Devices.ToList<DeviceObject>();
                map[xindex, yindex] = (int)DieTypeEnum.CHANGETEST_DIE;
                devices.Find(item => item.DieIndexM.XIndex == xindex && item.DieIndexM.YIndex == yindex)
                        .DieType.Value = DieTypeEnum.CHANGETEST_DIE;

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.ModifyDevideTestToMark() : Error occured");
            }
            return retVal;
        }

        private void ReInitDeviceState()
        {
            List<DeviceObject> Devices = Wafer.GetSubsInfo().Devices.ToList<DeviceObject>();
            long maxX, minX, maxY, minY;
            long xNum, yNum;
            byte[,] devicesmap = null;

            try
            {
                maxX = Devices.Max(d => d.DieIndexM.XIndex);
                minX = Devices.Min(d => d.DieIndexM.XIndex);
                maxY = Devices.Max(d => d.DieIndexM.YIndex);
                minY = Devices.Min(d => d.DieIndexM.YIndex);
                xNum = maxX - minX + 1;
                yNum = maxY - minY + 1;

                devicesmap = new byte[xNum, yNum];
                //DIEs = new IDeviceObject[xNum, yNum];

                Parallel.For(0, yNum, y =>
                {
                    Parallel.For(0, xNum, x =>
                    {
                        DeviceObject dev = null;
                        dev = Devices.ToList<DeviceObject>().Find(
                            d => d.DieIndexM.XIndex == x & d.DieIndexM.YIndex == y);
                        if (dev != null)
                        {
                            switch (dev.DieType.Value)
                            {
                                case DieTypeEnum.CHANGEMARK_DIE:
                                    devicesmap[x, y] = (byte)DieTypeEnum.MARK_DIE;
                                    break;
                                case DieTypeEnum.CHANGETEST_DIE:
                                    devicesmap[x, y] = (byte)DieTypeEnum.TEST_DIE;
                                    break;

                                    //case DieStateEnum.MARK:
                                    //    devicesmap[x, y] = (byte)DieTypeEnum.MARK_DIE;
                                    //    break;
                                    //case DieStateEnum.CHANGEDMARK:
                                    //    {
                                    //        dev.State.Value = DieStateEnum.NORMAL;
                                    //        devicesmap[x, y] = (byte)DieTypeEnum.TEST_DIE;
                                    //    }

                                    //    break;
                                    //case DieStateEnum.NORMAL:
                                    //    devicesmap[x, y] = (byte)DieTypeEnum.TEST_DIE;
                                    //    break;
                                    //case DieStateEnum.NOT_EXIST:
                                    //    devicesmap[x, y] = (byte)DieTypeEnum.NOT_EXIST;
                                    //    break;
                                    //case DieStateEnum.CHANGEDTEST:
                                    //    dev.State.Value = DieStateEnum.MARK;
                                    //    devicesmap[x, y] = (byte)DieTypeEnum.MARK_DIE;
                                    //    break;
                            }
                        }


                    });
                });
            }
            catch (Exception)
            {

            }

        }

        public EventCodeEnum StateTransition(SubModuleStateBase state)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                SubModuleState = state;
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {

                throw err;
            }
            return RetVal;
        }


        public override void DeInitModule()
        {
            return;
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
                retVal = IsParamChanged | retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override void SetStepSetupState(string header = null)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }
    }

    #region //..AutoMapGenerateState

    public enum DeviceEdgePMPos
    {
        UNDIFIND,
        EDGE,
        FULL,
        UPPER,
        LEFT,
        BOTTOM,
        RIGHT
    }

    /// <summary>
    /// 각 State Class 들의 이름의 정의는 
    /// </summary>
    public abstract class MapStandardAutoMapGenerateState
    {
        protected MapStandard _Module;
        public MapStandardAutoMapGenerateState(MapStandard module)
        {
            _Module = module;
        }

        public abstract EventCodeEnum DoGenerate(ICamera cam);
    }

    public class IdleState : MapStandardAutoMapGenerateState
    {
        public IdleState(MapStandard module) : base(module)
        {
        }

        public override EventCodeEnum DoGenerate(ICamera cam)
        {
            return EventCodeEnum.NONE;
        }
    }
    public class EdgeState : MapStandardAutoMapGenerateState
    {
        public EdgeState(MapStandard module) : base(module)
        {
        }

        public override EventCodeEnum DoGenerate(ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ((MapStandard)_Module).DoGernerate(DeviceEdgePMPos.EDGE, cam);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.EdgeState - DoGenerate() : Error occured");
            }
            return retVal;
        }
    }
    public class FullState : MapStandardAutoMapGenerateState
    {
        public FullState(MapStandard module) : base(module)
        {

        }
        public override EventCodeEnum DoGenerate(ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ((MapStandard)_Module).DoGernerate(DeviceEdgePMPos.FULL, cam);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.FullState - DoGenerate() : Error occured");
            }
            return retVal;
        }
    }
    public class UpperState : MapStandardAutoMapGenerateState
    {
        public UpperState(MapStandard module) : base(module)
        {
        }

        public override EventCodeEnum DoGenerate(ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ((MapStandard)_Module).DoGernerate(DeviceEdgePMPos.UPPER, cam);


            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.LURUState - UpperState() : Error occured");
            }
            return retVal;
        }
    }

    public class LeftState : MapStandardAutoMapGenerateState
    {
        public LeftState(MapStandard module) : base(module)
        {
        }

        public override EventCodeEnum DoGenerate(ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ((MapStandard)_Module).DoGernerate(DeviceEdgePMPos.LEFT, cam);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.LeftState - DoGenerate() : Error occured");
            }
            return retVal;
        }
    }

    public class BottomState : MapStandardAutoMapGenerateState
    {
        public BottomState(MapStandard module) : base(module)
        {
        }

        public override EventCodeEnum DoGenerate(ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ((MapStandard)_Module).DoGernerate(DeviceEdgePMPos.BOTTOM, cam);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.BottomState - DoGenerate() : Error occured");
            }
            return retVal;
        }
    }

    public class RightState : MapStandardAutoMapGenerateState
    {
        public RightState(MapStandard module) : base(module)
        {
        }

        public override EventCodeEnum DoGenerate(ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ((MapStandard)_Module).DoGernerate(DeviceEdgePMPos.RIGHT, cam);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.RightState - DoGenerate() : Error occured");
            }
            return retVal;
        }
    }


    #endregion

}
