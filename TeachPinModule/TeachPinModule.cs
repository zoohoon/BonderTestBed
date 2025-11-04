using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TeachPinModule
{
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.AlignEX;
    using ProberInterfaces.Enum;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using RelayCommandBase;
    using System.Windows.Input;
    using System.Windows;
    using ProberInterfaces.PnpSetup;
    using PnPControl;
    using ProberInterfaces.PinAlign;
    using ProberErrorCode;
    using ProberInterfaces.State;
    using LogModule;
    using PinAlign;
    using ProbeCardObject;
    using Newtonsoft.Json;
    using System.Xml.Serialization;
    using MetroDialogInterfaces;

    public class TeachPinModule : PNPSetupBase, ISetup, IProcessingModule, IRecovery
    {
        public override bool Initialized { get; set; } = false;

        public override Guid ScreenGUID { get; } = new Guid("EA95E665-70D6-BDE9-C512-6F81A4621CC8");
        public new List<object> Nodes { get; set; }

        //private ICamera Cam;

        private bool TeachRefPinFlag = false;
        //private bool TeachTargetPinFlag = false;

        private PinCoordinate RefPinPos;
        private PinCoordinate NewTargetPinPos;
        private PinCoordinate OldTargetPinPos;
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
        //private ObservableCollection<IDut> BackUpDutList;
        //private PinCoordinate CardCenPos;

        PinAlignAdditionalFunctionClass PinAlignFunc = new PinAlignAdditionalFunctionClass();

        private int targetDutIndex = -1;
        private int targetPinIndex = -1;

        int CurPinIndex = 0;
        int CurPinArrayIndex = 0;
        int CurDutIndex = 0;

        #region DutViewerProperties
        private double? _ZoomLevel;
        public new double? ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                if (value != _ZoomLevel)
                {
                    _ZoomLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowGrid;
        public new bool? ShowGrid
        {
            get { return _ShowGrid; }
            set
            {
                if (value != _ShowGrid)
                {
                    _ShowGrid = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowPin;
        public new bool? ShowPin
        {
            get { return _ShowPin; }
            set
            {
                if (value != _ShowPin)
                {
                    _ShowPin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowPad;
        public new bool? ShowPad
        {
            get { return _ShowPad; }
            set
            {
                if (value != _ShowPad)
                {
                    _ShowPad = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowCurrentPos;
        public new bool? ShowCurrentPos
        {
            get { return _ShowCurrentPos; }
            set
            {
                if (value != _ShowCurrentPos)
                {
                    _ShowCurrentPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private EnumProberCam _CamType;
        //public EnumProberCam CamType
        //{
        //    get { return _CamType; }
        //    set
        //    {
        //        if (value != _CamType)
        //        {
        //            _CamType = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private bool? _EnableDragMap;
        public new bool? EnableDragMap
        {
            get { return _EnableDragMap; }
            set
            {
                if (value != _EnableDragMap)
                {
                    _EnableDragMap = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowSelectedDut;
        public new bool? ShowSelectedDut
        {
            get { return _ShowSelectedDut; }
            set
            {
                if (value != _ShowSelectedDut)
                {
                    _ShowSelectedDut = value;
                    RaisePropertyChanged();
                }
            }
        }

        public new IStageSupervisor StageSupervisor
        {
            get { return this.StageSupervisor(); }
        }

        public new IVisionManager VisionManager
        {
            get { return this.VisionManager(); }
        }
        #endregion


        private ICoordinateManager _CoordinateManager;
        public ICoordinateManager CoordinateManager
        {
            get { return _CoordinateManager; }
        }

        private new IProbeCard ProbeCard { get { return this.GetParam_ProbeCard(); } }

        private IPinAligner _PinAligner;

        private IFocusing _PinFocusModel;
        public IFocusing PinFocusModel
        {
            get
            {
                if (_PinFocusModel == null)
                    _PinFocusModel = this.FocusManager().GetFocusingModel(FocusingDLLInfo.GetNomalFocusingDllInfo());

                return _PinFocusModel;
            }
        }

        private IFocusParameter FocusParam => (this.PinAligner().PinAlignDevParam as PinAlignDevParameters)?.FocusParam;
        private PinAlignDevParameters PinAlignParam => (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);

        //private IFocusing FocusingModule { get; set; }
        //private IFocusParameter FocusingParam { get; set; }


        //private ModuleDllInfo _FocusingModuleDllInfo;

        //public ModuleDllInfo FocusingModuleDllInfo
        //{
        //    get { return _FocusingModuleDllInfo; }
        //    set { _FocusingModuleDllInfo = value; }
        //}

        public new UserControlFucEnum UseUserControl { get; set; }
        public AlginParamBase Param { get; set; }
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }
        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }

        public SubModuleMovingStateBase MovingState { get; set; }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (Initialized == false)
                {
                    LoggerManager.Debug($"[TeachPinModule] : Start InitModule");
                    //_ProbeCard = (ProbeCard)container.Resolve<IProbeCard>();
                    _CoordinateManager = this.CoordinateManager();
                    _PinAligner = this.PinAligner();

                    //(ProbeCard)Container.Resolve<IProbeCard>();   
                    // ModuleState = null;//new EdgeIdleState(this);                

                    CurrMaskingLevel = this.ProberStation().MaskingLevel;
                    //ProcessingType = EnumSetupProgressState.IDLE;

                    MovingState = new SubModuleStopState(this);
                    SubModuleState = new SubModuleIdleState(this);

                    //InitBackupData();

                    //this.VisionManager.StartGrab(EnumProberCam.PIN_HIGH_CAM);
                    //DisplayPort = new DisplayPort();
                    //DisplayPort.StageSuperVisor = StageSupervisor;                

                    LoggerManager.Debug($"[TeachPinModule] : End InitModule");

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
                //LoggerManager.Error($err + "InitModule() : Error occured.");
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //Don`t Touch
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }
        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = "Teach Pin";
                retVal = InitPnpModuleStage();

                SetNodeSetupState(EnumMoudleSetupState.NONE);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                //LoggerManager.Debug(err);
                throw err;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }


        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                retVal = InitLightJog(this);
                retVal = await InitSetup();


                if (PnpManager.PnpLightJog.CurSelectedMag == CameraBtnType.High)
                    TwoButton.IsEnabled = true;
                else
                    TwoButton.IsEnabled = false;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                //LoggerManager.Debug(err);
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum DoExecute()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum deserialRes = EventCodeEnum.UNDEFINED;

            return deserialRes;
        }

        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }

        //private void InitBackupData()
        //{
        //    try
        //    {
        //        LoggerManager.Debug($"[TeachPinModule] InitBackupData() : Make Backup Data");
        //        if (BackUpDutList != null)
        //        {
        //            BackUpDutList.Clear();                    
        //        }
        //        else
        //        {
        //            BackUpDutList = new ObservableCollection<IDut>();
        //        }

        //        foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
        //        {
        //            BackUpDutList.Add(new Dut(dut));
        //        }
        //        LoggerManager.Debug($"[TeachPinModule] InitBackupData() : Make Backup Done");
        //    }
        //    catch(Exception err)
        //    {
        //        //LoggerManager.Error($err + "SaveProbeCardData() : Error occured.");                
        //        LoggerManager.Exception(err);
        //    }

        //}
        public EventCodeEnum InitSetupProcType()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "InitSetupProcType() : Error occured.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public async Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[TeachPinModule] InitSetup() : Start InitSetup");
            int totalPinCount = 0;
            int dutIndex = 0;
            int pinArrayIndex = 0;
            double posZ = 0.0;
            IPinData pinData = null;
            string pinListStr = "";

            try
            {
                //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                if (this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.Count() < 1)
                {
                    retVal = EventCodeEnum.NODATA;
                    throw new Exception();
                }
                else if (!isExistPinCheck())
                {
                    LoggerManager.Debug($"[TeachPinModule] InitSetup() : Pin data is not exist. Get Pin data from Pad");
                    if (this.StageSupervisor().ProbeCardInfo.GetPinDataFromPads() == EventCodeEnum.PIN_INVALID_PAD_DATA)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Pin Data ", "The integrity of between pin and pad data is broken. Please, Check the Pad or Dut data.", EnumMessageStyle.Affirmative);
                        await this.ViewModelManager().BackPreScreenTransition();
                    }
                    else
                    {

                    }
                }

                this.StageSupervisor().ProbeCardInfo.CheckValidPinParameters();

                LoadDevParameter();

                CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);

                this.PnpManager.RestoreLastLightSetting(CurCam);

                if (!isExistPinCheck())
                {
                    this.StageSupervisor().StageModuleState.PinHighViewMove(0, 0, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinDefaultHeight.Value);
                }
                else
                {
                    //ref pin의 dut로 이동하기 위한 작업
                    CurPinIndex = 0; //ref pin index = 0, number = 1
                    CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);
                    IPinData tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(CurPinIndex);
                    if (tmpPinData != null)
                        CurDutIndex = tmpPinData.DutNumber.Value - 1;
                    else
                        CurDutIndex = 0;

                    if (CurPinArrayIndex >= 0 && tmpPinData != null && CurDutIndex >= 0)
                    {
                        LoggerManager.Debug($"[TeachPinModule] InitBackupData() : Move To Ref Pin Position Start");
                        var dut = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.SingleOrDefault(dutinfo => dutinfo.DutNumber == CurDutIndex + 1);
                        if (dut.PinList.Count != 0)
                        {
                            if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                            {
                                string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(tmpPinData.PinNum.Value) % 5).ToString() + ".bmp";
                                this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                            }

                            if (tmpPinData.AbsPos.Z.Value >= this.StageSupervisor().PinMaxRegRange ||
                                tmpPinData.AbsPos.Z.Value < this.StageSupervisor().PinMinRegRange)
                            {
                                LoggerManager.Debug($"Pin position is initialized forcedly : org height = {tmpPinData.AbsPos.Z.Value}");
                                tmpPinData.AbsPosOrg.Z.Value = -10000;
                            }

                            IStageSupervisor StageSupervisor = this.StageSupervisor();

                            this.StageSupervisor.StageModuleState.ZCLEARED();

                            var ret = this.StageSupervisor.StageModuleState.PinHighViewMove(
                                tmpPinData.AbsPos.X.Value,
                                tmpPinData.AbsPos.Y.Value,
                                tmpPinData.AbsPos.Z.Value);

                            foreach (var light in tmpPinData.PinSearchParam.LightForTip)
                            {
                                CurCam.SetLight(light.Type.Value, light.Value.Value);
                            }

                            if (ret != EventCodeEnum.NONE)
                            {
                                ret = this.StageSupervisor.StageModuleState.PinHighViewMove(
                                    0, 0, this.CoordinateManager().StageCoord.PinReg.PinRegMin.Value);

                            }
                        }

                        LoggerManager.Debug($"[TeachPinModule] InitBackupData() : Move To Ref Pin Position Done");
                    }
                    else
                    {
                        LoggerManager.Debug($"[TeachPinModule] InitSetup() : invalid ref pin position, Move To Center Position");
                        this.StageSupervisor().StageModuleState.PinHighViewMove(0, 0, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinDefaultHeight.Value);
                    }
                }

                //foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
                //{
                //    CurCam.SetLight(light.Type.Value, light.Value.Value);
                //}
                var pinlist = this.StageSupervisor().ProbeCardInfo.GetPinList();
                totalPinCount = pinlist.Count; 
                if (totalPinCount > 0)
                {
                    for (int i = 0; i < totalPinCount; i++)
                    {
                        pinData = this.StageSupervisor().ProbeCardInfo.GetPin(i);
                        dutIndex = pinData.DutNumber.Value - 1;
                        pinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(i);
                        var dut = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.SingleOrDefault(dutinfo => dutinfo.DutNumber == pinData.DutNumber.Value);
                        if (dut != null)
                        {
                            if (dut.PinList.Count > pinArrayIndex)
                            {
                                posZ = dut.PinList[pinArrayIndex].AbsPos.Z.Value;
                            }
                            else
                            {
                                posZ = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinDefaultHeight.Value;
                            }
                        }
                        else
                        {
                            posZ = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinDefaultHeight.Value;
                        }

                        if (posZ > this.StageSupervisor().PinMaxRegRange)
                        {
                            LoggerManager.Debug($"[HighAlignKeyBrightnessSetupModule] InitSetup() : pin#{i + 1} Z value({posZ}) cannot be larger than PinMaxRegRange({this.StageSupervisor().PinMaxRegRange})");
                            pinListStr += $"Pin#{pinData.PinNum} ZPos = {posZ}\n";
                        }
                    }
                    if (pinListStr != "")
                    {
                        await this.MetroDialogManager().ShowMessageDialog($"Pin Z position", $"Pin Z height cannot exceed the maximum registration range({this.StageSupervisor().PinMaxRegRange})\n" +
                            $"{pinListStr}", EnumMessageStyle.Affirmative);
                    }
                }

                retVal = InitPNPSetupUI();
                retVal = InitLightJog(this);
                //BindingPNPSetup();

                //DisplayPort.StageSuperVisor = StageSupervisor;

                TargetRectangleWidth = 10;
                TargetRectangleHeight = 10;
                UseUserControl = UserControlFucEnum.DEFAULT;

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                MainViewTarget = DisplayPort;
                //StageSupervisor.PnpLightJog.InitCameraJog();
                MiniViewTarget = ProbeCard;

                ShowPad = false;
                ShowPin = true;
                EnableDragMap = true;
                ShowSelectedDut = false;
                ShowGrid = false;
                ZoomLevel = 5;
                ShowCurrentPos = true;

                //this.PinAligner().StopDrawPinOverlay(CurCam);
                //this.PinAligner().StopDrawDutOverlay(CurCam);

                //this.PinAligner().DrawPinOverlay(CurCam);
                this.PinAligner().DrawDutOverlay(CurCam);

                //WaferObject Wafer = (WaferObject)container.Resolve<IStageSupervisor>().WaferObject;

                //CurCam.UpdateOverlayFlag = true;

                //retVal = BindingPNPSetup();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "InitSetup() : Error occured.");
                LoggerManager.Exception(err);
            }
            finally
            {
                //await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
            return retVal;
        }

        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Buttons = new ObservableCollection<PNPCommandButtonDescriptor>();

                MainViewTarget = DisplayPort;

                Header = "Teach Pin";

                ProcessingType = EnumSetupProgressState.IDLE;

                PadJogLeftUp.Caption = "Prev";
                PadJogRightUp.Caption = "Next";

                PadJogLeftDown.Caption = "RefPin";
                PadJogRightDown.Caption = "TargetPin";

                //PadJogLeftDown.Caption = "";
                //PadJogRightDown.Caption = "";

                PadJogLeftUp.Command = new AsyncCommand(DoPrev);
                PadJogRightUp.Command = new AsyncCommand(DoNext);

                PadJogLeftDown.Command = TeachRefCommand; // new RelayCommand(TeachRefPin);
                PadJogRightDown.Command = TeachTargetCommand; // new RelayCommand(TeahchTargetPin);

                //PadJogLeftDown.Command = null;
                //PadJogRightDown.Command = null;

                PadJogUp.Caption = "";
                PadJogUp.Command = new RelayCommand((Action)null);

                PadJogDown.Caption = "";
                PadJogDown.Command = new RelayCommand((Action)null);

                PadJogLeft.Caption = "";
                PadJogLeft.Command = new RelayCommand((Action)null);

                PadJogRight.Caption = "";
                PadJogRight.Command = new RelayCommand((Action)null);

                PadJogSelect.Caption = "";

                PadJogLeftDown.IsEnabled = true;
                PadJogRightDown.IsEnabled = false;
                PadJogLeftUp.IsEnabled = true;
                PadJogRightUp.IsEnabled = true;

                PadJogUp.IsEnabled = false;
                PadJogDown.IsEnabled = false;
                PadJogLeft.IsEnabled = false;
                PadJogRight.IsEnabled = false;
                PadJogSelect.IsEnabled = false;

                MainViewZoomVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                OneButton.Visibility = System.Windows.Visibility.Visible;
                TwoButton.Visibility = System.Windows.Visibility.Visible;//==> Focus
                OneButton.Caption = "Reset";
                //OneButton.MaskingLevel = 3;
                OneButton.Command = Button1Command;

                TwoButton.Command = Button2Command;
                TwoButton.Caption = "Focusing";
                TwoButton.CaptionSize = 17;

                PnpManager.PnpLightJog.HighBtnEventHandler = new RelayCommand(CameraHighButton);
                PnpManager.PnpLightJog.LowBtnEventHandler = new RelayCommand(CameraLowButton);

                // For Engineering (Required trigger)
                ThreeButton.Command = Button3Command;
                ThreeButton.Caption = "Add";            // Add Pin and Pad Together
                FourButton.Command = Button4Command;
                FourButton.Caption = "Del";             // Delete Pin and Pad Together
                FiveButton.Command = Button5Command;
                FiveButton.Caption = "Focus \nAll Pin";
                FiveButton.CaptionSize = 17;

                if (Extensions_IParam.ProberExecuteMode == ExecuteMode.ENGINEER)
                {
                    ThreeButton.Visibility = System.Windows.Visibility.Visible;
                    FourButton.Visibility = System.Windows.Visibility.Visible;
                    FiveButton.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    ThreeButton.Visibility = System.Windows.Visibility.Hidden;
                    FourButton.Visibility = System.Windows.Visibility.Hidden;
                    FiveButton.Visibility = System.Windows.Visibility.Hidden;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        #region pnp button 1
        private AsyncCommand _Button1Command;
        public ICommand Button1Command
        {
            get
            {
                if (null == _Button1Command) _Button1Command = new AsyncCommand(
                    Reset
                    //, EvaluationPrivilege.Evaluate(
                    //        CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                    //         new Action(() => { ShowMessages("UIModeChange"); })
                             );
                return _Button1Command;
            }
        }
        private async Task Reset()
        {
            try
            {
                
                // await this.ViewModelManager().LockNotification(this.GetHashCode(), "Wait", "Reset Pin Data");

                const string message = "Pin data will be initialized from pad data...\nPrevious setup data will be lost, do you want to continue?";
                const string caption = "Warning";

                EnumMessageDialogResult answer = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.AffirmativeAndNegative);

                if (answer == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    ResetData();
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                
            }
        }

        private void ResetData()
        {
            try
            {
                LoggerManager.Debug($"[TeachPinModule] ResetData() : Reset Data Start");
                //foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                //{
                //    foreach (PinData pin in dut.PinList)
                //    {
                //        foreach (Dut oridut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                //        {
                //            if (dut.MacIndex.Equals(oridut.MacIndex))
                //            {
                //                foreach (PinData oripin in oridut.PinList)
                //                {
                //                    if (pin.PinNum.Value == oripin.PinNum.Value)
                //                    {
                //                        pin.AbsPos = null;
                //                        pin.AbsPos = new PinCoordinate(oripin.AbsPos);
                //                    }
                //                }
                //            }

                //        }
                //    }
                //}

                TeachRefPinFlag = false;
                //TeachTargetPinFlag = false;

                PadJogLeftDown.IsEnabled = true;
                PadJogRightDown.IsEnabled = false;

                RefPinPos = null;
                NewTargetPinPos = null;
                OldTargetPinPos = null;

                this.StageSupervisor().ProbeCardInfo.GetPinDataFromPads();
                //PinAlignParam.PinAlignInterval.FlagAlignProcessedAfterCardChange = false;   // Reg 다시 해야 함
                var refDut = ProbeCard.GetDutFromPinNum(1);
                var refpin = refDut.PinList.Find(p => p.PinNum.Value == 1);
                RefPinPos = new PinCoordinate(refpin.AbsPos.X.Value, refpin.AbsPos.Y.Value, refpin.AbsPos.Z.Value);
                NewTargetPinPos = new PinCoordinate(0, 0, refpin.AbsPos.Z.Value);
                OldTargetPinPos = new PinCoordinate(0, 0, refpin.AbsPos.Z.Value);

                targetDutIndex = -1;
                targetPinIndex = -1;

                //CardCenPos = new PinCoordinate();
                //CardCenPos.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                //CardCenPos.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                //CardCenPos.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);

                //InitBackupData();

                SaveDevParameter();

                CurCam.UpdateOverlayFlag = true;

                this.StageSupervisor().StageModuleState.PinHighViewMove(RefPinPos.X.Value, RefPinPos.Y.Value, RefPinPos.Z.Value);
                LoggerManager.Debug($"[TeachPinModule] ResetData() : Reset Data Done");
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                targetDutIndex = -1;
            }
        }
        #endregion

        #region pnp button 2
        private AsyncCommand _Button2Command;
        public ICommand Button2Command
        {
            get
            {
                if (null == _Button2Command)
                    _Button2Command = new AsyncCommand(
                    DoFocusing//, EvaluationPrivilege.Evaluate(
                              // CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                              // new Action(() => { ShowMessages("UIModeChange"); })
                            );
                return _Button2Command;
            }
        }
        private bool DoingFocusing = false;
        private async Task DoFocusing()
        {
            try
            {
                

                if (!DoingFocusing)
                {
                    DoingFocusing = true;
                    //Task t1 = await Task.Run(async () => Focusing());
                    await Focusing();
                    DoingFocusing = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. TeachPinModule - DoFocusing() : Error occured.");
            }
            finally
            {
                
            }
        }
        private Task Focusing()
        {
            try
            {
                //await this.ViewModelManager().LockNotification(this.GetHashCode(), "Wait", "Pin Focusing");

                int TotalPinNum = 0;
                TotalPinNum = this.StageSupervisor().ProbeCardInfo.GetPinCount();
                if (TotalPinNum <= 0) 
                    return Task.CompletedTask;

                FocusParam.FocusingCam.Value = CurCam.GetChannelType();
                if (FocusParam.FocusingAxis.Value != EnumAxisConstants.PZ)
                {
                    FocusParam.FocusingAxis.Value = EnumAxisConstants.PZ;
                }
                ICamera cam = this.VisionManager().GetCam(FocusParam.FocusingCam.Value);

                int OffsetX = cam.Param.GrabSizeX.Value / 2 - Convert.ToInt32(Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width)) / 2;
                int OffsetY = cam.Param.GrabSizeY.Value / 2 - Convert.ToInt32(Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height)) / 2;
                double width = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width;
                double height = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height;

                if (width == 0 || height == 0)
                {
                    FocusParam.FocusingROI.Value = new Rect(new Size(240, 240));
                }
                else
                {
                    FocusParam.FocusingROI.Value = new System.Windows.Rect(OffsetX, OffsetY, width, height);
                }

                if (PinAlignParam.PinHighAlignParam.PinTipFocusEnable.Value == true)
                {
                    FocusParam.FocusRange.Value = Convert.ToInt32(PinAlignParam.PinHighAlignParam.PinTipFocusRange.Value);
                }
                else
                {
                    FocusParam.FocusRange.Value = Convert.ToInt32(PinAlignParam.PinFocusingRange.Value);
                }
                LoggerManager.Debug($"[TeachPinModule] Focusing() : Focusing Parameter Pin Tip Focus Enable = " + PinAlignParam.PinHighAlignParam.PinTipFocusEnable.Value.ToString());
                LoggerManager.Debug($"[TeachPinModule] Focusing() : Focusing Parameter Focusing Range = " + FocusParam.FocusRange.Value.ToString());
                // ann todo -> OutFocusLimit 주석
                // FocusParam.OutFocusLimit.Value = 40;
                FocusParam.DepthOfField.Value = 1;
                FocusParam.PeakRangeThreshold.Value = 100;

                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                if (PinFocusModel.Focusing_Retry(FocusParam, false, false, false, this) != EventCodeEnum.NONE)
                {
                    //this.ViewModelManager().ShowNotifyToastMessage(this.GetHashCode(), "", "Focusing Fail", 2);
                    LoggerManager.Debug($"[TachchPinModule] Focusing() : Focusing Fail");
                }
                //else
                //{
                //    this.ViewModelManager().ShowNotifyToastMessage(this.GetHashCode(), "", "Focusing Success", 2);
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //await this.ViewModelManager().UnLockNotification(this.GetHashCode());
            }
            return Task.CompletedTask;
        }
        private void CameraHighButton()
        {
            TwoButton.IsEnabled = true;
        }
        private void CameraLowButton()
        {
            TwoButton.IsEnabled = false;
        }

        #endregion


        #region EngineeringButtons

        private AsyncCommand _Button3Command;
        public ICommand Button3Command
        {
            get
            {
                if (null == _Button3Command) _Button3Command = new AsyncCommand(AddCommand);
                return _Button3Command;
            }
        }
        private async Task AddCommand()
        {
            try
            {
                Task task = new Task(() =>
                {
                    AddPinPadData();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {

            }
        }
        private void AddPinPadData()
        {
            try
            {
                // 이 함수는 엔지니어링 용도로 만들어진 것으로, 패드->핀 등록 순서를 거꾸로 합니다.
                // 핀의 위치를 바탕으로 패드를 함께 등록합니다.

                double firstDut_LLX = 0;
                double firstDut_LLY = 0;
                PinCoordinate CenPos = new PinCoordinate();
                double DutAngle = 0;
                double PosLL_X = 0;
                double PosLL_Y = 0;
                bool bFound = false;
                CatCoordinates curPos;
                double[,] DutCornerPos = new double[4, 2];
                PinCoordinate NewPos = new PinCoordinate();
                PinCoordinate ZeroAnglePos = new PinCoordinate();
                int CurDutNum = 0;
                double CurDut_LLX = 0;
                double CurDut_LLY = 0;
                PinData add_pin = new PinData();
                double tmpcenx = 0;
                double tmpceny = 0;
                double tmpdutcenx = 0;
                double tmpdutceny = 0;

                // 핀을 추가하기 위한 조건으로 
                // 1. 반드시 두 개 이상의 패드가 등록이 되어 있어야 한다. (같은 곳에 등록해 두어도 상관 없다)
                // 2. Ref 핀과 Target 핀 위치 지정이 끝나 있어야 한다. (즉 Dut center 계산이 완료 되어 있어야 한다)

                // 1. 현재 DUT를 계산하고, DUT 바깥인 경우 에러 처리한다.

                CenPos.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                CenPos.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                DutAngle = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutAngle;

                firstDut_LLX = CenPos.X.Value - ((double)((this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX / 2.0)) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value);
                firstDut_LLX += this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                firstDut_LLY = CenPos.Y.Value - ((double)((this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY / 2.0)) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value);
                firstDut_LLY += this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

                //firstDut_LLX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX - this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenX;
                //firstDut_LLY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY - this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenY;

                curPos = CurCam.GetCurCoordPos();

                // 핀 좌표계에서 1번 더트의 LL 위치
                firstDut_LLX = CenPos.X.Value - ((double)((this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX / 2.0)) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value);
                firstDut_LLX += this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                firstDut_LLY = CenPos.Y.Value - ((double)((this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY / 2.0)) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value);
                firstDut_LLY += this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

                bFound = false;
                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    // 이 더트의 좌하단 위치
                    PosLL_X = firstDut_LLX + (dut.MacIndex.XIndex - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                    PosLL_Y = firstDut_LLY + (dut.MacIndex.YIndex - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

                    // 이 더트의 각 꼭지점 위치
                    // 좌하
                    DutCornerPos[0, 0] = PosLL_X;
                    DutCornerPos[0, 1] = PosLL_Y;
                    // 우하
                    DutCornerPos[1, 0] = PosLL_X + this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                    DutCornerPos[1, 1] = PosLL_Y;
                    // 좌상
                    DutCornerPos[2, 0] = PosLL_X;
                    DutCornerPos[2, 1] = PosLL_Y + this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;
                    // 우상
                    DutCornerPos[3, 0] = PosLL_X + this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                    DutCornerPos[3, 1] = PosLL_Y + this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

                    Rect displayScreenRect = new Rect(new Point(DutCornerPos[0, 0], DutCornerPos[0, 1]), new Point(DutCornerPos[3, 0], DutCornerPos[3, 1]));
                    //Point displayCenterPoint = new Point((img.RatioX.Value * (img.SizeX / 2)), (img.RatioY.Value * (img.SizeY / 2)));
                    // 더트 각도를 고려하기 위해서 일단 현재 보고 있는 지점에 더트 각도를 반대로 적용시켜 위치를 구하고, 이 위치값을 각도가 적용되기 전
                    // 더트에 대입하여 더트 내부에 현재 지점이 존재하는지 확인한다. (꼼수 얍~)
                    GetRotCoordEx(ref ZeroAnglePos, new PinCoordinate(curPos.X.Value, curPos.Y.Value), CenPos, -DutAngle);
                    Point displayCenterPoint = new Point(ZeroAnglePos.X.Value, ZeroAnglePos.Y.Value);
                    //if (displayScreenRect.Contains(displayCenterPoint))
                    {
                        // 빙고. 이 더트가 니 더트임.
                        bFound = true;
                        CurDutNum = dut.DutNumber;

                        // 더트 각도 적용
                        GetRotCoordEx(ref NewPos, new PinCoordinate(PosLL_X, PosLL_Y), CenPos, DutAngle);
                        CurDut_LLX = NewPos.X.Value;
                        CurDut_LLY = NewPos.Y.Value;
                        break;
                    }

                }

                if (bFound == false)
                {
                    LoggerManager.Debug($"Could not proceed add command because current position is not in DUT.");
                    return;
                }

                // 2. 현재 DUT의 LL 위치를 계산하고, 그 위치로부터 현재 위치까지의 상대거리를 구한다.
                add_pin.AbsPosOrg = new PinCoordinate(CurCam.GetCurCoordPos());
                add_pin.AlignedOffset = new PinCoordinate(0, 0, 0);
                add_pin.RelPos = new PinCoordinate(0, 0, 0);
                add_pin.RelPos.X.Value = add_pin.AbsPos.X.Value - CurDut_LLX;
                add_pin.RelPos.Y.Value = add_pin.AbsPos.Y.Value - CurDut_LLY;
                add_pin.PinSearchParam.AddLight(new LightValueParam(EnumLightType.COAXIAL, 70));
                add_pin.PinSearchParam.AddLight(new LightValueParam(EnumLightType.AUX, 0));
                add_pin.PinSearchParam.BlobThreshold.Value = 140;
                add_pin.PinSearchParam.MaxBlobSizeX.Value = 50;
                add_pin.PinSearchParam.MaxBlobSizeY.Value = 50;
                add_pin.PinSearchParam.MinBlobSizeX.Value = 15;
                add_pin.PinSearchParam.MinBlobSizeY.Value = 15;
                add_pin.PinSearchParam.SearchArea = new Element<Rect>();
                add_pin.PinSearchParam.SearchArea.Value = new Rect(new Size(240, 240));
                add_pin.DutNumber.Value = CurDutNum;


                //==========Add Leina
                //IPinData tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(CurPinIndex);
                //foreach (var alignkeyhigh in tmpPinData.PinSearchParam.AlignKeyHigh)
                //{
                //    PinSearchParameter.AlignKeyInfo keyinfo = new PinSearchParameter.AlignKeyInfo();

                //    add_pin.PinSearchParam.AlignKeyHigh.Add(alignkeyhigh);
                //}
                //=======================

                int maxpin = int.MinValue;
                foreach (var tmpdutlist in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (var tmplist in tmpdutlist.PinList)
                    {
                        if (tmplist.PinNum.Value > maxpin)
                        {
                            maxpin = tmplist.PinNum.Value;
                        }
                    }
                }

                add_pin.PinNum.Value = maxpin + 1;

                // 3. 2번항목에서 구한 상대거리만큼 핀과 패드 위치를 등록해 준다.
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutNum - 1].PinList.Add(add_pin);
                //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutNum].PinList.Add(new PinData(add_pin));

                if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList == null)
                {
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList = new List<IGroupData>();
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList.Add(new GroupData());
                }

                if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList.Count == 0)
                {
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList.Add(new GroupData());
                }
                //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList[0].DutPinPairList.Clear();
                if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList[0].PinNumList == null)
                {
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList[0].PinNumList = new List<int>();
                }
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList[0].PinNumList.Add(add_pin.PinNum.Value);           //아직은 그룹을 한개만 써서 0번에 집어 넣는다.




                DUTPadObject padparam = new DUTPadObject();
                padparam.PadSizeX.Value = 80;
                padparam.PadSizeY.Value = 80;
                padparam.PadShape.Value = EnumPadShapeType.SQUARE;
                padparam.PadCenter.X.Value = add_pin.RelPos.X.Value;
                padparam.PadCenter.Y.Value = add_pin.RelPos.Y.Value;
                padparam.DutNumber = CurDutNum;

                // 이미 등록된 패드들 중 현재 더트를 공유하는 패드가 있는지 확인하고 있으면 이전에 등록된 패드의 더트 정보를 가져다 사용한다.
                // 만약 이 더트에 패드를 처음 등록하는 경우, 어쩔수 없이 계산한다. (LL 위치 등은 약간 오차가 발생할 수 있다)
                bFound = false;
                foreach (var dutinfo in this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos)
                {
                    if (dutinfo.DutNumber == CurDutNum)
                    {
                        padparam.MachineIndex = dutinfo.MachineIndex;
                        padparam.MIndexLCWaferCoord = dutinfo.MIndexLCWaferCoord;
                        padparam.DutMIndex = dutinfo.DutMIndex;
                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                {
                    // 첫번째 패드 정보를 기반으로 현재 값을 계산한다.
                    long chai_x = 0;
                    long chai_y = 0;
                    int refNo = 0;
                    refNo = (int)this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[0].DutNumber;

                    // 첫번째 패드를 가진 더트와 현재 등록중인 더트 간 인덱스 간격
                    chai_x = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutNum - 1].MacIndex.XIndex - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[refNo - 1].MacIndex.XIndex;
                    padparam.MachineIndex.XIndex = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[0].MachineIndex.XIndex + chai_x;
                    chai_y = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutNum - 1].MacIndex.YIndex - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[refNo - 1].MacIndex.YIndex;
                    padparam.MachineIndex.YIndex = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[0].MachineIndex.YIndex + chai_y;

                    // 앞서 구해진 간격에 인덱스 사이즈를 곱해서 LL 위치 계산
                    padparam.MIndexLCWaferCoord = new WaferCoordinate();
                    padparam.MIndexLCWaferCoord.X.Value = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[0].MIndexLCWaferCoord.X.Value
                                                          + (chai_x * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value);
                    padparam.MIndexLCWaferCoord.Y.Value = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[0].MIndexLCWaferCoord.Y.Value
                                                          + (chai_y * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value);

                    padparam.DutMIndex = new MachineIndex();
                    padparam.DutMIndex.XIndex = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[0].DutMIndex.XIndex + chai_x;
                    padparam.DutMIndex.YIndex = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[0].DutMIndex.YIndex + chai_y;
                }


                int maxAge = int.MinValue;
                foreach (var type in this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos)
                {
                    if (type.PadNumber.Value > maxAge)
                    {
                        maxAge = type.PadNumber.Value;
                    }
                }
                padparam.PadNumber.Value = maxAge + 1;
                this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.Add(padparam);

                // 4. 핀/패드 센터를 다시 계산해 준다.
                this.WaferAligner().UpdatePadCen();

                this.StageSupervisor().ProbeCardInfo.CalcPinCen(out tmpcenx, out tmpceny, out tmpdutcenx, out tmpdutceny);
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX = tmpcenx;
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY = tmpceny;
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX = tmpdutcenx;
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY = tmpdutceny;

                this.PinAligner().StopDrawDutOverlay(CurCam);
                this.PinAligner().DrawDutOverlay(CurCam);

                // 5. 파일에 저장한다
                SaveDevParameter();
                this.StageSupervisor().WaferObject.SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _Button4Command;
        public ICommand Button4Command
        {
            get
            {
                if (null == _Button4Command) _Button4Command = new AsyncCommand(DeleteCommand);
                return _Button4Command;
            }
        }
        private async Task DeleteCommand()
        {
            try
            {
                

                //Task t = Task.Run(async () =>
                //{
                //    DelPinPad();
                //});

                Task task = new Task(() =>
                {
                    DelPinPad();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                
            }
        }

        private AsyncCommand _Button5Command;
        public ICommand Button5Command
        {
            get
            {
                if (null == _Button5Command) _Button5Command = new AsyncCommand(FocusingAllPinCommand);
                return _Button5Command;
            }
        }

        private async Task FocusingAllPinCommand()
        {
            try
            {
                int pinCnt = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList.Count;
                StringBuilder stb = new StringBuilder();
                stb.Append($"Focusing on all pins. Count:{pinCnt}");
                stb.Append(System.Environment.NewLine);

                foreach (var pin in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList)
                {
                    await Focusing();

                    PinCoordinate CurPos = this.CoordinateManager().PinHighPinConvert.CurrentPosConvert();
                    stb.Append($" - Pin #{pin.PinNum} Z Height: {Math.Truncate(CurPos.Z.Value)}");
                    stb.Append(System.Environment.NewLine);

                    Next();
                }

                await this.MetroDialogManager().ShowMessageDialog("Information", stb.ToString(), EnumMessageStyle.Affirmative);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DelPinPad()
        {
            CatCoordinates curPos;
            double dist = 0.0;
            double mindist = 300000.0;
            PinCoordinate refPos;
            int pinNum = 0;
            int dutNum = 0;
            double tmpcenx = 0;
            double tmpceny = 0;
            double tmpdutcenx = 0;
            double tmpdutceny = 0;

            try
            {
                // 1. 현재 위치에서 가장 가까운 핀을 찾는다.
                curPos = CurCam.GetCurCoordPos();
                refPos = new PinCoordinate(0, 0, 0);
                refPos.X.Value = curPos.X.Value;
                refPos.Y.Value = curPos.Y.Value;
                refPos.Z.Value = curPos.Z.Value;

                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (IPinData cur_pin in dut.PinList)
                    {
                        dist = GetDistance2D(refPos, cur_pin.AbsPos);
                        if (mindist > dist)
                        {
                            mindist = dist;
                            pinNum = cur_pin.PinNum.Value;
                            dutNum = cur_pin.DutNumber.Value;
                        }
                    }
                }

                if (mindist > 100)
                    return;

                if (pinNum == 1)        // 0번 패드는 지울 수 없다.
                    return;

                // 2. 해당 핀/패드 데이터를 지우고 남은 핀들의 번호를 재정렬한다.
                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    var index = dut.PinList.FindIndex(i => i.PinNum.Value == pinNum);
                    if (index >= 0)
                    {
                        dut.PinList.RemoveAt(index);
                    }

                    foreach (IPinData cur_pin in dut.PinList)
                    {
                        if (cur_pin.PinNum.Value > pinNum)
                            cur_pin.PinNum.Value = cur_pin.PinNum.Value - 1;
                    }
                }

                foreach (DUTPadObject type in this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.ToList())
                {
                    if (type.DutNumber == dutNum && type.PadNumber.Value == pinNum)
                    {
                        this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.Remove(type);
                    }
                    else if (type.PadNumber.Value > pinNum)
                    {
                        type.PadNumber.Value -= 1;
                    }
                }

                // 3. 핀과 패드 센터를 업데이트 한다.
                this.WaferAligner().UpdatePadCen();

                this.StageSupervisor().ProbeCardInfo.CalcPinCen(out tmpcenx, out tmpceny, out tmpdutcenx, out tmpdutceny);
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX = tmpcenx;
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY = tmpceny;
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX = tmpdutcenx;
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY = tmpdutceny;

                this.PinAligner().StopDrawDutOverlay(CurCam);
                this.PinAligner().DrawDutOverlay(CurCam);

                // 4. 파일에 저장한다
                SaveDevParameter();
                this.StageSupervisor().WaferObject.SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region pnp button 5
        //private AsyncCommand _Button5Command;
        //public ICommand Button5Command
        //{
        //    get
        //    {
        //        if (null == _Button5Command)
        //            _Button5Command = new AsyncCommand(DoPinAlign);
        //        return _Button5Command;
        //    }
        //}
        private async Task DoPinAlign()
        {
            bool injected = this.CommandManager().SetCommand<ProberInterfaces.Command.Internal.IDOPINALIGN>(this);
            if (injected)
            {
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                LoggerManager.Debug($"DoPinAlign. sender is {this.ToString()}");
            }
            else
            {
                // 에러 ,로그 금지 
            }
            try
            {
                PinAlignResultes AlignResult = (this.PinAligner().PinAlignInfo as PinAlignInfo)?.AlignResult;
                if (this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                {
                    StringBuilder stb = new StringBuilder();
                    stb.Append("Pin Alignment is done successfully.");
                    stb.Append(System.Environment.NewLine);

                    stb.Append($"Source : {AlignResult.AlignSource}");
                    stb.Append(System.Environment.NewLine);

                    foreach (var result in AlignResult.EachPinResultes)
                    {
                        stb.Append($" - Pin #{result.PinNum}. Shift = ");
                        stb.Append($"X: {result.DiffX,4:0.0}um, ");
                        stb.Append($"Y: {result.DiffY,4:0.0}um, ");
                        stb.Append($"Z: {result.DiffZ,4:0.0}um, ");
                        stb.Append($" Height: {result.Height,7:0.0}um");
                        if (PinAlignParam.PinHighAlignParam.PinTipFocusEnable.Value == true)
                        {
                            stb.Append($", Tip Result = {result.PinTipOptResult}");
                        }
                        stb.Append(System.Environment.NewLine);
                    }

                    //foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                    //{
                    //    foreach (IPinData cur_pin in dut.PinList)
                    //    {
                    //        stb.Append($"Pin #{cur_pin.PinNum.Value} @");
                    //        stb.Append($"X: {cur_pin.AlignedOffset.X.Value}um, ");
                    //        stb.Append($"Y: {cur_pin.AlignedOffset.Y.Value}um, ");
                    //        stb.Append($"Z: {cur_pin.AlignedOffset.Z.Value}um");
                    //        stb.Append(System.Environment.NewLine);
                    //    }
                    //}
                    await this.MetroDialogManager().ShowMessageDialog("Information", stb.ToString(), EnumMessageStyle.Affirmative);

                    this.GetParam_ProbeCard().PinSetupChangedToggle.DoneState = ElementStateEnum.DONE;
                }
                else if (this.StageSupervisor().ProbeCardInfo.GetAlignState() != AlignStateEnum.DONE)
                {
                    StringBuilder stb = new StringBuilder();
                    stb.Append($"Pin Alignment is failed.");
                    stb.Append(System.Environment.NewLine);

                    stb.Append($"Source : {AlignResult.AlignSource}");
                    stb.Append(System.Environment.NewLine);

                    //stb.Append($"Reason : {this.PinAligner().ReasonOfError.Reason}");

                    stb.Append($"Reason : { this.PinAligner().ReasonOfError.GetLastEventMessage()}");

                    stb.Append(System.Environment.NewLine);

                    string FailDescription = this.PinAligner().MakeFailDescription();

                    if (FailDescription != string.Empty)
                    {
                        stb.Append(FailDescription);
                        stb.Append(System.Environment.NewLine);
                    }

                    if (AlignResult.Result == EventCodeEnum.PIN_CARD_CENTER_TOLERANCE)
                    {
                        stb.Append($"Center Diff X: {AlignResult.CardCenterDiffX,4:0.0}, Y: {AlignResult.CardCenterDiffY,4:0.0}, Z:{AlignResult.CardCenterDiffZ,4:0.0}");
                        stb.Append(System.Environment.NewLine);
                    }

                    foreach (var result in AlignResult.EachPinResultes)
                    {
                        stb.Append($" - Pin #{result.PinNum}. Shift = ");
                        stb.Append($"X: {result.DiffX,4:0.0}um, ");
                        stb.Append($"Y: {result.DiffY,4:0.0}um, ");
                        stb.Append($"Z: {result.DiffZ,4:0.0}um, ");
                        stb.Append(System.Environment.NewLine);
                        stb.Append($"   Height: {result.Height,7:0.0}um");
                        if (PinAlignParam.PinHighAlignParam.PinTipFocusEnable.Value == true)
                        {
                            stb.Append($", Tip Result = {result.PinTipOptResult}");
                        }
                        stb.Append(System.Environment.NewLine);

                        //stb.Append($"Pin #{result.PinNum} ({result.PinResult}) | Shift = X : {result.DiffX,4:0.0} um, Y : {result.DiffY,4:0.0} um, Height: {result.Height,7:0.0} um");
                        //stb.Append(System.Environment.NewLine);
                        //stb.Append($"Result =  {result.PinResult}");

                        //stb.Append($" - Pin #{result.PinNum}. Shift = ");
                        //stb.Append($"X: {result.DiffX,4:0.0}um, ");
                        //stb.Append($"Y: {result.DiffY,4:0.0}um, ");
                        //stb.Append($" Height: {result.Height,7:0.0}um");

                        //stb.Append($"    Result =  {result.PinResult}");
                        //stb.Append(System.Environment.NewLine);
                    }
                    await this.MetroDialogManager().ShowMessageDialog("Warning", stb.ToString(), EnumMessageStyle.Affirmative, "OK");
                    //this.MetroDialogManager().ShowMessageDialog("Warning", $"Pin Alignment is failed\n{this.PinAligner().ReasonOfError.Reason}", EnumMessageStyle.Affirmative, "OK");
                }
                else
                {
                    EventCodeInfo lastevent = this.PinAligner().ReasonOfError.GetLastEventCode();
                    string failreason = lastevent?.Message;

                    await this.MetroDialogManager().ShowMessageDialog("Warning", $"Pin to Pad Alignment is failed\n{failreason}", EnumMessageStyle.Affirmative, "OK");
                }


            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. PinHighAlignModule - PinAlign() : Error occured.");
            }
            finally
            {
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                this.PinAligner().DrawDutOverlay(CurCam);
            }
        }


        #endregion

        #region pnp button 5
        //private RelayCommand _Button5Command;
        //public ICommand Button5Command
        //{
        //    get
        //    {
        //        if (null == _Button5Command)
        //            _Button5Command = new RelayCommand(
        //            DoShowResult//, EvaluationPrivilege.Evaluate(
        //                        // CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
        //                        // new Action(() => { ShowMessages("UIModeChange"); })
        //                    );
        //        return _Button5Command;
        //    }
        //}

        //private void DoShowResult()
        //{
        //    try
        //    {
        //        //
        //        ShowResult();
        //    }
        //    catch (Exception err)
        //    {

        //    }
        //    finally
        //    {
        //        //
        //    }
        //}

        //private void ShowResult()
        //{
        //    UcPinAlignResult ucPinResult = new UcPinAlignResult();
        //    int num = 0;
        //    try
        //    {

        //        //foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
        //        //{
        //        //    pinnum += dut.PinList.Count;
        //        //}

        //        ucPinResult.DataContext = new PinAlignResultViewModel();

        //        Window window = new Window()
        //        {
        //            WindowStartupLocation = WindowStartupLocation.CenterScreen,
        //            ResizeMode = ResizeMode.NoResize,
        //            Background = Brushes.Black,
        //            Height = 960,
        //            Width = 1280,
        //            Content = ucPinResult
        //        };
        //        window.ShowDialog();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //}

        #endregion



        private AsyncCommand _TeachRefCommand;
        public ICommand TeachRefCommand
        {
            get
            {
                if (null == _TeachRefCommand) _TeachRefCommand = new AsyncCommand(
                    TeachRefPin);
                return _TeachRefCommand;
            }
        }
        private async Task TeachRefPin()
        {
            string message = String.Empty;
            string caption = String.Empty;
            double tmpcenx = 0;
            double tmpceny = 0;
            double tmpdutcenx = 0;
            double tmpdutceny = 0;
            bool IsValid = true;
            try
            {
                //await this.ViewModelManager().LockNotification(this.GetHashCode(), "Wait", "Teach Ref Pin");

                if (this.StageSupervisor().ProbeCardInfo.GetPinCount() <= 1)
                {
                    message = message = "Not enough pin count, you need 2 pin at least.";
                    caption = "Information";

                    EnumMessageDialogResult answer = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.AffirmativeAndNegative);
                }
                else
                {
                    if (CurCam.GetChannelType() == EnumProberCam.PIN_LOW_CAM)
                    {
                        RefPinPos = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                    }
                    else
                    {
                        RefPinPos = this.CoordinateManager().PinHighPinConvert.CurrentPosConvert();
                    }

                    foreach (Dut dutdata in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                    {
                        foreach (PinData pin in dutdata.PinList)
                        {
                            //등록하려는 ref 핀 위치와 디바이스의 핀 위치를 비교한다
                            if (pin.PinNum.Value == this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value)
                            {
                                if (this.PinAlignParam.RefPinRegoffsetX.Value != 0 || this.PinAlignParam.RefPinRegoffsetY.Value != 0)
                                {
                                    IsValid = true;
                                    if (Math.Abs(pin.AbsPosOrg.X.Value - RefPinPos.X.Value) > this.PinAlignParam.RefPinRegoffsetX.Value || Math.Abs(pin.AbsPosOrg.Y.Value - RefPinPos.Y.Value) > this.PinAlignParam.RefPinRegoffsetY.Value)
                                    {
                                        message = message = $"The difference value from the previously registered reference pin position is greater than the set offset (X:{this.PinAlignParam.RefPinRegoffsetX.Value},Y:{this.PinAlignParam.RefPinRegoffsetY.Value}). Do you want to set it up?";
                                        caption = "Reference Pin Position";

                                        EnumMessageDialogResult answer = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.AffirmativeAndNegative);
                                        if (answer != EnumMessageDialogResult.AFFIRMATIVE)
                                        {
                                            IsValid = false;
                                        }
                                    }
                                    LoggerManager.Debug($"[TeachPinModule] TeachRefPin() : RefPinRegoffset ({this.PinAlignParam.RefPinRegoffsetX.ToString()}, {this.PinAlignParam.RefPinRegoffsetY.ToString()} | diff ({pin.AbsPosOrg.X.Value - RefPinPos.X.Value},{pin.AbsPosOrg.Y.Value - RefPinPos.Y.Value})");
                                }
                            }
                        }
                    }

                    if (IsValid == true)
                    {
                        message = message = "Do you want to register the selected position as the Reference Pin position?";
                        caption = "Reference Pin Position";

                        EnumMessageDialogResult answer = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.AffirmativeAndNegative);

                        if (answer == EnumMessageDialogResult.AFFIRMATIVE)
                        {


                            //PinCoordinate AdjustOffset = new PinCoordinate((RefPinPos.X.Value - 
                            //                             this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value), 
                            //                             (RefPinPos.Y.Value - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value), 
                            //                             (RefPinPos.Z.Value - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value));

                            // Ref 핀을 새로 Teach 하는 경우, Ref 핀 기준으로 다른 핀들의 위치은 패드 위치로부터 초기화 한다.

                            // 우선 Ref핀과 Ref패드 간의 관계로부터 상대거리를 구한다.
                            double distX = 0;
                            double distY = 0;

                            foreach (DUTPadObject DutDataFromPads in this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos)
                            {
                                if ((DutDataFromPads.DutNumber == this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].DutNumber.Value)
                                    && (DutDataFromPads.PadNumber.Value == this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value))
                                {                               
                                    distX = DutDataFromPads.MIndexLCWaferCoord.X.Value + DutDataFromPads.PadCenter.X.Value - RefPinPos.X.Value;
                                    distY = DutDataFromPads.MIndexLCWaferCoord.Y.Value + DutDataFromPads.PadCenter.Y.Value - RefPinPos.Y.Value;
                                    break;
                                }
                            }

                            // Verify 

                            // 현재 로드되어 있는 (디바이스 사이즈 + ProbingSWRadiusLimit / 2.0)를 이용한다.
                            // 변경되는 위치의 절댓값이 위 기준값보다 큰 경우, Teach Pin 작업이 캔슬 됨.
                            // 사용자에게는 첫 번째로 조건을 만족하지 못한, Dut Number와 Pin Number를 알려준다.

                            PinCoordinate tempPinPos = new PinCoordinate();
                            int tmpDutNum = -1;
                            int tmpPinNum = -1;

                            double TeachAbsLimit = Math.Abs((this.GetParam_Wafer().GetPhysInfo().WaferSize_um.Value + this.CoordinateManager().StageCoord.ProbingSWRadiusLimit.Value) / 2.0);


                            bool XOn = false;
                            bool YOn = false;
                            ushort defaultlightvalue = 128;
                            foreach (Dut dutdata in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                foreach (PinData pin in dutdata.PinList)
                                {
                                    if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value == PROBECARD_TYPE.MEMS_Dual_AlignKey)
                                    {
                                        if (pin.PinSearchParam.LightForTip == null)
                                        {
                                            pin.PinSearchParam.LightForTip = new List<LightValueParam>();
                                            pin.PinSearchParam.LightForTip.Clear();
                                            foreach (var lightChannel in CurCam.LightsChannels)
                                            {
                                                var lightVal = CurCam.GetLight(lightChannel.Type.Value);
                                                pin.PinSearchParam.AddLight(
                                                    new LightValueParam(
                                                        lightChannel.Type.Value
                                                        , lightVal != -1 ? (ushort)lightVal : defaultlightvalue));
                                            }
                                        }
                                        else
                                        {
                                            if (pin.PinSearchParam.LightForTip.Count == 0)
                                            {
                                                pin.PinSearchParam.LightForTip.Clear();
                                                foreach (var lightChannel in CurCam.LightsChannels)
                                                {
                                                    var lightVal = CurCam.GetLight(lightChannel.Type.Value);
                                                    pin.PinSearchParam.AddLight(
                                                        new LightValueParam(
                                                            lightChannel.Type.Value
                                                            , lightVal != -1 ? (ushort)lightVal : defaultlightvalue));
                                                }
                                            }
                                        }
                                    }

                                    if (pin.PinNum.Value == this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value)
                                    {
                                        // 현재 핀 = Ref 핀

                                        tempPinPos.X.Value = RefPinPos.X.Value;
                                        tempPinPos.Y.Value = RefPinPos.Y.Value;

                                        if (Math.Abs(tempPinPos.X.Value) >= TeachAbsLimit)
                                        {
                                            IsValid = false;
                                            XOn = true;

                                            tmpDutNum = dutdata.DutNumber;
                                            tmpPinNum = pin.PinNum.Value;

                                            break;
                                        }

                                        if (Math.Abs(tempPinPos.Y.Value) >= TeachAbsLimit)
                                        {
                                            IsValid = false;
                                            YOn = true;

                                            tmpDutNum = dutdata.DutNumber;
                                            tmpPinNum = pin.PinNum.Value;

                                            break;
                                        }
                                    }
                                    else
                                    {
                                        // 그 외 핀들. 패드 데이터로부터 초기화

                                    foreach (DUTPadObject DutDataFromPads in this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos)
                                    {
                                        if ((DutDataFromPads.DutNumber == pin.DutNumber.Value) && (DutDataFromPads.PadNumber.Value == pin.PinNum.Value))
                                        {
                                            tempPinPos.X.Value = (DutDataFromPads.MIndexLCWaferCoord.X.Value + DutDataFromPads.PadCenter.X.Value - distX);
                                            tempPinPos.Y.Value = (DutDataFromPads.MIndexLCWaferCoord.Y.Value + DutDataFromPads.PadCenter.Y.Value - distY);

                                                if (Math.Abs(tempPinPos.X.Value) >= TeachAbsLimit)
                                                {
                                                    IsValid = false;
                                                    XOn = true;

                                                    tmpDutNum = DutDataFromPads.PadNumber.Value;
                                                    tmpPinNum = pin.PinNum.Value;


                                                    break;
                                                }

                                                if (Math.Abs(tempPinPos.Y.Value) >= TeachAbsLimit)
                                                {

                                                    IsValid = false;
                                                    YOn = true;

                                                    tmpDutNum = DutDataFromPads.PadNumber.Value;
                                                    tmpPinNum = pin.PinNum.Value;

                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (IsValid == false)
                                {
                                    if (XOn)
                                    {
                                        LoggerManager.Debug($"[TeachPinModule] TeachRefPin() : Canceled, Dut #{tmpDutNum}, Pin #{tmpPinNum}, Exceeded value : X = {tempPinPos.X.Value}, Limit = {TeachAbsLimit}");
                                    }

                                    if (YOn)
                                    {
                                        LoggerManager.Debug($"[TeachPinModule] TeachRefPin() : Canceled, Dut #{tmpDutNum}, Pin #{tmpPinNum}, Exceeded value : Y = {tempPinPos.Y.Value}, Limit = {TeachAbsLimit}");
                                    }

                                    break;
                                }
                            }

                            if (IsValid == true)
                            {
                                // 각 핀의 위치를 초기화 한다. 높이는 Ref핀의 높이로 맞춘다.
                                foreach (Dut dutdata in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                                {
                                    foreach (PinData pin in dutdata.PinList)
                                    {
                                        if (pin.PinNum.Value == this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value)
                                        {
                                            // 현재 핀 = Ref 핀
                                            pin.AbsPosOrg.X.Value = RefPinPos.X.Value;
                                            pin.AbsPosOrg.Y.Value = RefPinPos.Y.Value;
                                            pin.AbsPosOrg.Z.Value = RefPinPos.Z.Value;

                                            pin.AlignedOffset.X.Value = 0;
                                            pin.AlignedOffset.Y.Value = 0;
                                            pin.AlignedOffset.Z.Value = 0;

                                            pin.MarkCumulativeCorrectionValue.X.Value = 0;
                                            pin.MarkCumulativeCorrectionValue.Y.Value = 0;
                                            pin.MarkCumulativeCorrectionValue.Z.Value = 0;

                                            pin.LowCompensatedOffset.X.Value = 0;
                                            pin.LowCompensatedOffset.Y.Value = 0;
                                            pin.LowCompensatedOffset.Z.Value = 0;
                                        }
                                        else
                                        {
                                            // 그 외 핀들. 패드 데이터로부터 초기화

                                        foreach (DUTPadObject DutDataFromPads in this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos)
                                        {
                                            if ((DutDataFromPads.DutNumber == pin.DutNumber.Value) && (DutDataFromPads.PadNumber.Value == pin.PinNum.Value))
                                            {
                                                pin.AbsPosOrg.X.Value = DutDataFromPads.MIndexLCWaferCoord.X.Value + DutDataFromPads.PadCenter.X.Value - distX;
                                                pin.AbsPosOrg.Y.Value = DutDataFromPads.MIndexLCWaferCoord.Y.Value + DutDataFromPads.PadCenter.Y.Value - distY;
                                                pin.AbsPosOrg.Z.Value = RefPinPos.Z.Value;

                                                    pin.AlignedOffset.X.Value = 0;
                                                    pin.AlignedOffset.Y.Value = 0;
                                                    pin.AlignedOffset.Z.Value = 0;

                                                    pin.MarkCumulativeCorrectionValue.X.Value = 0;
                                                    pin.MarkCumulativeCorrectionValue.Y.Value = 0;
                                                    pin.MarkCumulativeCorrectionValue.Z.Value = 0;

                                                    pin.LowCompensatedOffset.X.Value = 0;
                                                    pin.LowCompensatedOffset.Y.Value = 0;
                                                    pin.LowCompensatedOffset.Z.Value = 0;

                                                    break;
                                                }
                                            }
                                        }

                                        LoggerManager.Debug($"[TeachPinModule] TeachRefPin() : First Adjusted Position #" + pin.PinNum.Value + " X = " + pin.AbsPos.X.Value + " Y = " + pin.AbsPos.Y.Value + " Z = " + pin.AbsPos.Z.Value);
                                    }
                                }

                                LoggerManager.Debug($"[TeachPinModule] TeachRefPin() : Teached Ref Pin Position, X = " + RefPinPos.GetX().ToString() + " Y = " + RefPinPos.GetY().ToString() + " Z = " + RefPinPos.GetZ().ToString());

                                //LoggerManager.Debug($"[TeachPinModule] TeachRefPin() : Adjust Offset, X = " + AdjustOffset.GetX().ToString() + " Y = " + AdjustOffset.GetY().ToString() + " Z = " + AdjustOffset.GetZ().ToString());
                                //LoggerManager.Debug($"");
                                //LoggerManager.Debug($"[TeachPinModule] TeachRefPin() : #########################################################");
                                //foreach (Dut dutdata in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                                //{
                                //    foreach (PinData pin in dutdata.PinList)
                                //    {
                                //        LoggerManager.Debug($"[TeachPinModule] TeachRefPin() : Original Position #" + pin.PinNum.Value + " X = " + pin.AbsPos.X.Value + " Y = " + pin.AbsPos.Y.Value + " Z = " + pin.AbsPos.Z.Value);
                                //    }
                                //}
                                //LoggerManager.Debug($"");
                                //LoggerManager.Debug($"[TeachPinModule] TeachRefPin() : #########################################################");

                                //foreach (Dut dutdata in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                                //{
                                //    foreach (PinData pin in dutdata.PinList)
                                //    {
                                //        pin.AbsPosOrg.X.Value = pin.AbsPosOrg.X.Value + AdjustOffset.X.Value;
                                //        pin.AbsPosOrg.Y.Value = pin.AbsPosOrg.Y.Value + AdjustOffset.Y.Value;
                                //        pin.AbsPosOrg.Z.Value = pin.AbsPosOrg.Z.Value + AdjustOffset.Z.Value;
                                //        LoggerManager.Debug($"[TeachPinModule] TeachRefPin() : First Adjusted Position #" + pin.PinNum.Value + " X = " + pin.AbsPos.X.Value + " Y = " + pin.AbsPos.Y.Value + " Z = " + pin.AbsPos.Z.Value);
                                //    }
                                //}

                                this.StageSupervisor().ProbeCardInfo.CalcPinCen(out tmpcenx, out tmpceny, out tmpdutcenx, out tmpdutceny);
                                LoggerManager.Debug($"TeachRefPin():Pin CenX = {tmpcenx:0.00}, Pin CenY = {tmpceny:0.00}, Dut CenX = {tmpdutcenx:0.00}, Dut CenY = {tmpdutceny:0.00}");
                                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX = tmpcenx;
                                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY = tmpceny;
                                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX = tmpdutcenx;
                                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY = tmpdutceny;

                                LoggerManager.Debug($"Updated pin center = ({tmpcenx}, {tmpceny})");

                                this.PinAligner().StopDrawDutOverlay(CurCam);

                                this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                                this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);

                                //PinAlignParam.PinAlignInterval.FlagAlignProcessedAfterCardChange = false;

                                //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutDiffX = (double)AdjustOffset.X.GetValue();
                                //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutDiffY = (double)AdjustOffset.Y.GetValue();

                                //CardCenPos.X.Value = CardCenPos.X.Value + AdjustOffset.X.Value;
                                //CardCenPos.Y.Value = CardCenPos.Y.Value + AdjustOffset.Y.Value;

                                GotoTargetPinPos();

                                //this.StageSupervisor().StageModuleState.PinHighViewMove
                                TeachRefPinFlag = true;
                                PadJogLeftDown.IsEnabled = false;
                                PadJogRightDown.IsEnabled = true;
                            }
                            else
                            {
                                if (XOn)
                                {
                                    await this.MetroDialogManager().ShowMessageDialog($"Teach pin process is failed", $"The selected position is out of the range. \nDut #{tmpDutNum}, Pin #{tmpPinNum}, Exceeded value(X axis) = {tempPinPos.X.Value}, Limit = {TeachAbsLimit}", EnumMessageStyle.Affirmative);
                                }

                                if (YOn)
                                {
                                    await this.MetroDialogManager().ShowMessageDialog($"Teach pin process is failed", $"The selected position is out of the range. \nDut #{tmpDutNum}, Pin #{tmpPinNum}, Exceeded value(Y axis) = {tempPinPos.Y.Value}, Limit = {TeachAbsLimit}", EnumMessageStyle.Affirmative);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                this.PinAligner().DrawDutOverlay(CurCam);
            }
        }


        private EventCodeEnum GotoTargetPinPos()// #Hynix_Merge: 검토 필요, 뭔가 소스를 잘못 고친것 같은 느낌.. 이전에는 핀만봤는데 더트넘버랑 핀넘버를 같이 찾도록 수정됨.
        {
            try
            {
                int refpinnumber = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.RefPinNum.Value;
                PinCoordinate refpin = null;
                foreach (var dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (var pin in dut.PinList)
                    {
                        if (pin.PinNum.Value == refpinnumber)
                        {
                            refpin = pin.AbsPos;
                            break;
                        }
                    }
                }


                double distReftoTarget = -1;
                double NewDist = -1;
                IDut tmpDut = null;
                IPinData tmpPin = null;

                foreach (Dut dutdata in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (PinData pin in dutdata.PinList)
                    {
                        NewDist = GetDistance2D(refpin, pin.AbsPos);
                        if (distReftoTarget < NewDist)
                        {
                            tmpDut = this.StageSupervisor().ProbeCardInfo.GetDutFromPinNum(pin.PinNum.Value);                            
                            if(tmpDut != null)
                            {
                                targetDutIndex = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.IndexOf(dutdata);
                                targetPinIndex = dutdata.PinList.IndexOf(pin);
                                tmpPin = pin;
                            }
                            distReftoTarget = NewDist;
                        }
                    }
                }
                OldTargetPinPos = tmpPin.AbsPos;

                if (CurCam.GetChannelType() == EnumProberCam.PIN_LOW_CAM)
                {
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[targetDutIndex].PinList[targetPinIndex].PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                    }
                    this.StageSupervisor().StageModuleState.PinLowViewMove(OldTargetPinPos.X.Value, OldTargetPinPos.Y.Value);
                }
                else
                {
                    //PinCoordinate pcoord = new PinCoordinate(OldTargetPinPos.X.Value, OldTargetPinPos.Y.Value, OldTargetPinPos.Z.Value);
                    //MachineCoordinate mcoord = this.CoordinateManager().PinHighPinConvert.ConvertBack(pcoord);
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[targetDutIndex].PinList[targetPinIndex].PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                    }
                    
                    //if (mcoord.X.Value < this.CoordinateManager().StageCoord.ProbingSWLimitNegative.X.Value || mcoord.X.Value < this.CoordinateManager().StageCoord.ProbingSWLimitPositive.X.Value
                    //    || mcoord.Y.Value < this.CoordinateManager().StageCoord.ProbingSWLimitNegative.Y.Value || mcoord.Y.Value < this.CoordinateManager().StageCoord.ProbingSWLimitPositive.Y.Value)
                    //{
                    //    LoggerManager.Debug($"SW Limit mcoord (x,y) = ({mcoord.X.Value}, {mcoord.Y.Value})");
                    //}
                    //else
                    //{
                        this.StageSupervisor().StageModuleState.PinHighViewMove(OldTargetPinPos.X.Value, OldTargetPinPos.Y.Value);
                    //}
                    
                }
                //foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[targetDutIndex].PinList[targetPinIndex].PinSearchParam.LightForTip)
                //{
                //    CurCam.SetLight(light.Type.Value, light.Value.Value);
                //}

                //Move 함수에서 SW limit check를 하니까 핦 필요가 없나...........??
                //MotionManager.CheckSWLimit(EnumAxisConstants.X, CoordinateManager.PinHighPinConvert.ConvertBack(RefPinPos).GetX());
                //MotionManager.CheckSWLimit(EnumAxisConstants.Y, CoordinateManager.PinHighPinConvert.ConvertBack(RefPinPos).GetY());
                //MotionManager.CheckSWLimit(EnumAxisConstants.Z, CoordinateManager.PinHighPinConvert.ConvertBack(RefPinPos).GetZ());

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            
            return EventCodeEnum.NONE;
        }

        private double GetDistance2D(PinCoordinate FirstPin, PinCoordinate SecondPin)
        {
            double Distance = -1;

            Distance = Math.Sqrt(Math.Pow(FirstPin.GetX() - SecondPin.GetX(), 2) + Math.Pow(FirstPin.GetY() - SecondPin.GetY(), 2));

            return Distance;
        }
        public double GetDegree(PinCoordinate pivot, PinCoordinate pointOld, PinCoordinate pointNew)
        {
            //==> degree = atan((y2 - cy) / (x2-cx)) - atan((y1 - cy)/(x1-cx)) : 세점사이의 각도 구함
            double originDegree = Math.Atan2(
                 pointOld.Y.Value - pivot.Y.Value,
                 pointOld.X.Value - pivot.X.Value)
                 * 180 / Math.PI;

            double updateDegree = Math.Atan2(
                 pointNew.Y.Value - pivot.Y.Value,
                 pointNew.X.Value - pivot.X.Value)
                 * 180 / Math.PI;

            //==> 프로버 카드가 틀어진 θ 각
            return updateDegree - originDegree;
        }

        private AsyncCommand _TeachTargetCommand;
        public ICommand TeachTargetCommand
        {
            get
            {
                if (null == _TeachTargetCommand) _TeachTargetCommand = new AsyncCommand(TeahchTargetPin);
                return _TeachTargetCommand;
            }
        }

        private async Task TeahchTargetPin()
        {
            string message = String.Empty;
            string caption = String.Empty;
            double PinHeight = 0;
            double tmpcenx = 0;
            double tmpceny = 0;
            double tmpdutcenx = 0;
            double tmpdutceny = 0;
            EnumMessageDialogResult answer = EnumMessageDialogResult.UNDEFIND;
            try
            {
                this.PinAligner().StopDrawDutOverlay(CurCam);

                if (this.StageSupervisor().ProbeCardInfo.GetPinCount() <= 1)
                {
                    message = message = "Not enough pin count, you need 2 pin at least.";
                    caption = "Information";

                    answer = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.AffirmativeAndNegative);
                }
                else
                {
                    if (TeachRefPinFlag == true)
                    {
                        if (CurCam.GetChannelType() == EnumProberCam.PIN_LOW_CAM)
                        {
                            NewTargetPinPos = CoordinateManager.PinLowPinConvert.CurrentPosConvert();
                        }
                        else
                        {
                            NewTargetPinPos = CoordinateManager.PinHighPinConvert.CurrentPosConvert();
                        }

                        if (this.PinAlignParam.TargetPinRegoffsetX.Value != 0 || this.PinAlignParam.TargetPinRegoffsetY.Value != 0)
                        {
                            PinCoordinate newtargetpinabspos = new PinCoordinate(NewTargetPinPos.GetX(), NewTargetPinPos.GetY(), PinHeight);

                            var devtargetpin = OldTargetPinPos;

                            if (Math.Abs(devtargetpin.X.Value - newtargetpinabspos.X.Value) > this.PinAlignParam.TargetPinRegoffsetX.Value || Math.Abs(devtargetpin.Y.Value - newtargetpinabspos.Y.Value) > this.PinAlignParam.TargetPinRegoffsetY.Value)
                            {
                                message = $"The difference value from the previously registered Target pin position is greater than the set offset (X:{this.PinAlignParam.TargetPinRegoffsetX.Value},Y:{this.PinAlignParam.TargetPinRegoffsetY.Value}).\n" +
                                    $" diff ({devtargetpin.X.Value - newtargetpinabspos.X.Value}, {devtargetpin.Y.Value - newtargetpinabspos.Y.Value}).\nDo you want to set it up?";
                                caption = "Target Pin Position";

                                LoggerManager.Debug($"[TeachPinModule] TeahchTargetPin() : Retval = TargetPinRegoffset ({this.PinAlignParam.TargetPinRegoffsetX.Value}, {this.PinAlignParam.TargetPinRegoffsetY.Value} " +
                                    $"| diff ({devtargetpin.X.Value - newtargetpinabspos.X.Value}, {devtargetpin.Y.Value - newtargetpinabspos.Y.Value})");

                                answer = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.AffirmativeAndNegative);
                                if (answer != EnumMessageDialogResult.AFFIRMATIVE)
                                {
                                    return;
                                }
                            }
                        }

                        message = "Do you want to register the selected position as the Target Pin position?";
                        caption = "Target Pin Position";

                        answer = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.AffirmativeAndNegative);

                        if (answer == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            //NewTargetPinPos = new PinCoordinate();
                            LoggerManager.Debug($"[TeachPinModule] TeachTargetPin() : Teached Target Pin Position  X = " + NewTargetPinPos.GetX() + " Y = " + NewTargetPinPos.GetY() + " Z = " + NewTargetPinPos.GetZ());
                            //TeachTargetPinFlag = true;

                            double CardDgree = 0;
                            //double MissplacedAmount = 0;
                            //double[] Diff = new double[2];

                            if (GetDistance2D(RefPinPos, NewTargetPinPos) < 1000)
                            {
                                // 핀 간 거리가 너무 짧은 경우, 각도는 무의미하다. (== 너무 크게 계산된다)
                                CardDgree = 0;
                            }
                            else
                            {
                                CardDgree = GetDegree(RefPinPos, OldTargetPinPos, NewTargetPinPos);
                                //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutAngle = CardDgree*1;
                                //int refPinNum = 0;
                                //var refDut = this.StageSupervisor().ProbeCardInfo.GetDutFromPinNum(refPinNum);
                                //var targetDut = this.StageSupervisor().ProbeCardInfo.GetDutFromPinNum(GetTargetPinNum());

                                //PinCoordinate refDutPos = new PinCoordinate();
                                //PinCoordinate targetDutPos = new PinCoordinate();
                                //targetDutPos.X.Value = targetDut.MacIndex.XIndex * this.WaferObject.GetPhysInfo().DieSizeX.Value;
                                //targetDutPos.Y.Value = targetDut.MacIndex.YIndex * this.WaferObject.GetPhysInfo().DieSizeY.Value;
                                //LoggerManager.Debug($"TeachTargetPin(): Estimated target pin pos from Ref. pin = ({targetDutPos.X.Value:0.00}, {targetDutPos.Y.Value:0.00})");
                                //var padAngle = GetDegree(refDutPos, refDutPos, targetDutPos);
                                //var teachPinAngle = GetDegree(RefPinPos, RefPinPos, NewTargetPinPos);
                                //LoggerManager.Debug($"TeachTargetPin(): Calculated Pad Angle = {padAngle:0.00000}, Teached pin angle = {teachPinAngle:0.00000}");
                                //CardDgree = padAngle - teachPinAngle;

                                if (CardDgree < 0)
                                {
                                    CardDgree = 360 + CardDgree;
                                    //MissplacedAmount = -CardDgree;
                                }
                                else
                                {
                                    //MissplacedAmount = CardDgree;
                                }
                            }
                            LoggerManager.Debug($"[TeachPinModule] TeachTargetPin() : Degree = " + CardDgree.ToString());

                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutAngle = CardDgree;

                            PinHeight = (RefPinPos.GetZ() + NewTargetPinPos.GetZ()) / 2;
                            LoggerManager.Debug($"[TeachPinModule] TeahchTargetPin() : Pin Height = " + PinHeight.ToString());
                            LoggerManager.Debug($"[TeachPinModule] TeahchTargetPin() : #########################################################");
                            PinCoordinate NewPos;// = new PinCoordinate();



                            for (int i = 0; i <= this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count() - 1; i++)
                            {
                                NewPos = new PinCoordinate();
                                GetRotCoordEx(ref NewPos, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].RefCorner, RefPinPos, CardDgree);
                                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].RefCorner = new PinCoordinate(NewPos);

                                //LoggerManager.Debug($"[TeachPinModule] TeachRefPin() : Dut #{this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].DutNumber}, RefCorner : X = {NewPos.X.Value}, Y = {NewPos.Y.Value}, {NewPos.Z.Value}");

                                NewPos = null;
                                for (int j = 0; j <= this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList.Count() - 1; j++)
                                {
                                    int pinindex = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList.IndexOf(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j]);
                                    if (i == targetDutIndex && pinindex == targetPinIndex)
                                    {
                                        // 좀 전에 사람이 찍은 위치를 그냥 넣는다. 
                                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].AbsPosOrg = new PinCoordinate(NewTargetPinPos.GetX(), NewTargetPinPos.GetY(), PinHeight);
                                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].AlignedOffset = new PinCoordinate(0, 0, 0);
                                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].RelPos.X.Value = NewTargetPinPos.GetX() - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].RefCorner.GetX();
                                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].RelPos.Y.Value = NewTargetPinPos.GetY() - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].RefCorner.GetY();
                                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].RelPos.Z.Value = NewTargetPinPos.GetZ() - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].RefCorner.GetZ();
                                    }
                                    else
                                    {
                                        //두 점의 위치로 미루어 위치를 보정한다.
                                        NewPos = new PinCoordinate();
                                        GetRotCoordEx(ref NewPos, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].AbsPos, RefPinPos, CardDgree);

                                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].AbsPosOrg = new PinCoordinate(NewPos.GetX(), NewPos.GetY(), PinHeight);

                                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].AlignedOffset = new PinCoordinate(0, 0, 0);

                                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].RelPos.X.Value = NewPos.GetX() - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].RefCorner.GetX();
                                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].RelPos.Y.Value = NewPos.GetY() - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].RefCorner.GetY();
                                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].RelPos.Z.Value = NewPos.GetZ() - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].RefCorner.GetZ();

                                        NewPos = null;
                                    }

                                    LoggerManager.Debug($"[TeachPinModule] TeahchTargetPin() : Final Adjusted Position #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].PinNum.Value + " X = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].AbsPos.X.Value + " Y = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].AbsPos.Y.Value + " Z = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].AbsPos.Z.Value);
                                }

                            }

                            this.StageSupervisor().ProbeCardInfo.CalcPinCen(out tmpcenx, out tmpceny, out tmpdutcenx, out tmpdutceny);

                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX = tmpcenx;
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY = tmpceny;
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX = tmpdutcenx;
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY = tmpdutceny;

                            LoggerManager.Debug($"TeahchTargetPin() Updated pin center = Pin CenX = {tmpcenx:0.00}, Pin CenY = {tmpceny:0.00}, Dut CenX = {tmpdutcenx:0.00}, Dut CenY = {tmpdutceny:0.00}");

                            //this.PinAligner().DrawDutOverlay(CurCam);

                            //PinAlignParam.PinAlignInterval.FlagAlignProcessedAfterCardChange = false;

                            this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                            this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);

                            NewPos = new PinCoordinate();
                            SaveDevParameter();

                            PadJogLeftDown.IsEnabled = true;
                            PadJogRightDown.IsEnabled = false;

                            NewPos = null;
                            var cardinfo = this.StageSupervisor().ProbeCardInfo.ProbeCardSysObjectRef;
                            if (cardinfo != null)
                            {
                                cardinfo.ProbercardinfoClear();
                            }
                            //SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                            SetNodeSetupState(EnumMoudleSetupState.NONE);
                        }
                    }
                    else
                    {
                        message = "You can't teach target pin now.\nPlease teach reference pin firstly.";
                        caption = "Warning";

                        answer = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative);

                        this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].PinList[0].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].PinList[0].AbsPos.Y.Value);
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                this.PinAligner().DrawDutOverlay(CurCam);
                //await this.ViewModelManager().UnLockNotification(this.GetHashCode());
            }
        }
        private int GetTargetPinNum()
        {
            double distReftoTarget = -1;
            double NewDist = -1;
            int tgtPinNum = -1;
            try
            {
                int refpinnumber = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.RefPinNum.Value;
                PinCoordinate refpin = null;
                foreach (var dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (var pin in dut.PinList)
                    {
                        if (pin.PinNum.Value == refpinnumber)
                        {
                            refpin = pin.AbsPos;
                            break;
                        }
                    }
                }

                foreach (IDut dutdata in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (IPinData pin in dutdata.PinList)
                    {
                        NewDist = GetDistance2D(refpin, pin.AbsPos);
                        if (distReftoTarget < NewDist)
                        {
                            targetDutIndex = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.IndexOf(dutdata);
                            targetPinIndex = dutdata.PinList.IndexOf(pin);
                            distReftoTarget = NewDist;
                        }
                    }
                }
                LoggerManager.Debug($"GetTargetPinNum(): Target pin num. = {tgtPinNum}");
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"GetTargetPinNum(): Error occurred. Err = {err.Message}");
            }
            return tgtPinNum;
        }

        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        private void GetRotCoordEx(ref PinCoordinate NewPos, PinCoordinate OriPos, PinCoordinate RefPos, double angle)
        {
            double newx = 0.0;
            double newy = 0.0;
            double th = DegreeToRadian(angle);

            newx = OriPos.X.Value - RefPos.X.Value;
            newy = OriPos.Y.Value - RefPos.Y.Value;

            NewPos.X.Value = newx * Math.Cos(th) - newy * Math.Sin(th) + RefPos.X.Value;
            NewPos.Y.Value = newx * Math.Sin(th) + newy * Math.Cos(th) + RefPos.Y.Value;
        }

        private void MoveToUp()
        {
            PinCoordinate CurPos = CoordinateManager.PinHighPinConvert.CurrentPosConvert();

            CurPos.Y.Value += 10;

            if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
            {
                this.StageSupervisor().StageModuleState.PinHighViewMove(CurPos.X.Value, CurPos.Y.Value);
            }
            else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
            {
                this.StageSupervisor().StageModuleState.PinLowViewMove(CurPos.X.Value, CurPos.Y.Value);
            }


        }
        private void MoveToDown()
        {
            PinCoordinate CurPos = CoordinateManager.PinHighPinConvert.CurrentPosConvert();

            CurPos.Y.Value -= 10;

            if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
            {
                this.StageSupervisor().StageModuleState.PinHighViewMove(CurPos.X.Value, CurPos.Y.Value);
            }
            else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
            {
                this.StageSupervisor().StageModuleState.PinLowViewMove(CurPos.X.Value, CurPos.Y.Value);
            }
        }
        private void MoveToLeft()
        {
            PinCoordinate CurPos = CoordinateManager.PinHighPinConvert.CurrentPosConvert();

            CurPos.X.Value -= 10;

            if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
            {
                this.StageSupervisor().StageModuleState.PinHighViewMove(CurPos.X.Value, CurPos.Y.Value);
            }
            else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
            {
                this.StageSupervisor().StageModuleState.PinLowViewMove(CurPos.X.Value, CurPos.Y.Value);
            }
        }
        private void MoveToRight()
        {
            PinCoordinate CurPos = CoordinateManager.PinHighPinConvert.CurrentPosConvert();

            CurPos.X.Value += 10;

            if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
            {
                this.StageSupervisor().StageModuleState.PinHighViewMove(CurPos.X.Value, CurPos.Y.Value);
            }
            else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
            {
                this.StageSupervisor().StageModuleState.PinLowViewMove(CurPos.X.Value, CurPos.Y.Value);
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


        private void SeletedItemChanged()
        {
            //ProberViewModuel.ScreenTestkeyValuePair.TryGetValue(SetupsTreeView, out _ProberMain);
            // _ProberMain.InitModule(Container);
            //ProberMainScreens = _ProberMain;
        }

        public EventCodeEnum SaveProbeCardData()
        {
            LoggerManager.Debug($"[TeachPinModule] SaveProbeCardData() : Start Save");
            EventCodeEnum serialRes = EventCodeEnum.UNDEFINED;

            try
            {
                serialRes = this.StageSupervisor().SaveProberCard();
                double centerX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                double centerY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                this.LoaderRemoteMediator()?.GetServiceCallBack()?.ChangedProbeCardObjectDutCenter(centerX, centerY);
                if (serialRes == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug("SaveDevParameter() : Save Ok.");
                }
                else
                {
                    LoggerManager.Debug("SaveDevParameter() : Save Fail.");
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "SaveProbeCardData() : Error occured.");
                LoggerManager.Exception(err);
            }
            LoggerManager.Debug($"[TeachPinModule] SaveProbeCardData() : End Save");
            return serialRes;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            FocusParam.FocusingAxis.Value = EnumAxisConstants.PZ;

            return retval;
        }

        public EventCodeEnum Modify()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SaveProbeCardData();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "PreRun() : Error occured.");
                LoggerManager.Exception(err);
            }


            return retVal;
        }

        public EventCodeEnum ClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            int NumOfPin = 0;
            try
            {
                retVal = EventCodeEnum.NONE;
                foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    NumOfPin += dut.PinList.Count;
                }
                if (NumOfPin < 2)
                {
                    retVal = EventCodeEnum.PIN_NOT_ENOUGH;
                    //SubModuleState.SetErrorState(); 
                    //this.StateTransition(new TeachPinNoDataState(this));
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                    //  SubModuleState.SetIdleState();
                    //this.StateTransition(new TeachPinIdleState(this));
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "PreRun() : Error occured.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override void DeInitModule()
        {

        }

        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }
        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
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
        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }


        public EventCodeEnum Recovery()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }
        public EventCodeEnum ExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
        public bool IsExecute()
        {
            return false;
        }
        public EventCodeEnum DoExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //if(!TeachTargetPinFlag || !TeachRefPinFlag)
                //{
                //    //ResetData();
                //}
                //else
                //{
                SaveProbeCardData();

                //SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                SetNodeSetupState(EnumMoudleSetupState.NONE);
                //this.PinAligner().StopDrawPinOverlay(CurCam);
                this.PinAligner().StopDrawDutOverlay(CurCam);

                retVal = await base.Cleanup(parameter);
                if (retVal == EventCodeEnum.NONE)
                {
                    this.StageSupervisor().StageModuleState.ZCLEARED();
                }
                //}
                this.PnpManager.RememberLastLightSetting(CurCam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        private bool isExistPinCheck()
        {
            int NumOfPin = 0;
            try
            {
                foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    NumOfPin += dut.PinList.Count;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return NumOfPin > 0 ? true : false;
        }


        private Task DoNext()
        {
            try
            {
                
                Next();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                
            }
            return Task.CompletedTask;
        }
        public void Next()
        {
            double posX = 0.0;
            double posY = 0.0;
            double posZ = 0.0;
            try
            {
                int TotalPinNum = 0;
                IPinData tmpPinData;


                TotalPinNum = this.StageSupervisor().ProbeCardInfo.GetPinCount();
                if (TotalPinNum <= 0) return;

                if (CurPinIndex >= TotalPinNum - 1)
                {
                    CurPinIndex = 0;
                }
                else
                {
                    CurPinIndex++;
                }
                tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(CurPinIndex);

                if (tmpPinData == null) return;

                CurDutIndex = tmpPinData.DutNumber.Value - 1;

                // CurPinArrayIndex는 현재 선택된 실제 핀 번호이지만, 데이터 상으로는 더트별로 해당 핀이 들어가 있는 배열의 인덱스는 핀 번호하고는 다르다.
                // 따라서 배열상의 어레이로 변환해 주어야 한다.
                CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);

                if (CurPinArrayIndex < 0 || CurDutIndex < 0) return;

                //foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
                //{
                //    CurCam.SetLight(light.Type.Value, light.Value.Value);
                //}

                //int light = 0;
                //light = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value;
                //if (light < 0) light = 0;
                //Cam.SetLight(EnumLightType.COAXIAL, Convert.ToUInt16(light));

                var dut = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.SingleOrDefault(dutinfo => dutinfo.DutNumber == CurDutIndex + 1);

                posX = dut.PinList[CurPinArrayIndex].AbsPos.X.Value;
                posY = dut.PinList[CurPinArrayIndex].AbsPos.Y.Value;
                posZ = dut.PinList[CurPinArrayIndex].AbsPos.Z.Value;

                if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                {
                    string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(dut.PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                    this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                }
                if (posZ > this.StageSupervisor().PinMaxRegRange)
                {
                    posZ = this.StageSupervisor().PinMaxRegRange;
                }
                if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);
                }
                else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinLowViewMove(posX, posY, posZ);
                }
                LoggerManager.Debug($"[TeachPinModule] Next() : Move to Next Pin #" + dut.PinList[CurPinArrayIndex].PinNum.Value);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "Next() : Error occured.");
                LoggerManager.Exception(err);

            }

        }
        private Task DoPrev()
        {
            try
            {
                
                Prev();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                
            }
            return Task.CompletedTask;
        }
        public void Prev()
        {
            double posX = 0.0;
            double posY = 0.0;
            double posZ = 0.0;
            try
            {
                int TotalPinNum = 0;
                IPinData tmpPinData;

                TotalPinNum = this.StageSupervisor().ProbeCardInfo.GetPinCount();
                if (TotalPinNum <= 0) return;

                if (CurPinIndex <= 0)
                {
                    CurPinIndex = TotalPinNum - 1;
                }
                else
                {
                    CurPinIndex--;
                }
                tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(CurPinIndex);

                if (tmpPinData == null) return;

                CurDutIndex = tmpPinData.DutNumber.Value - 1;

                // CurPinArrayIndex는 현재 선택된 실제 핀 번호이지만, 데이터 상으로는 더트별로 해당 핀이 들어가 있는 배열의 인덱스는 핀 번호하고는 다르다.
                // 따라서 배열상의 어레이로 변환해 주어야 한다.
                CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);

                if (CurDutIndex < 0 || CurPinArrayIndex < 0) return;


                //foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
                //{
                //    CurCam.SetLight(light.Type.Value, light.Value.Value);
                //}

                //int light = 0;
                //light = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value;
                //Cam.SetLight(EnumLightType.COAXIAL, Convert.ToUInt16(light));
                posX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value;
                posY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value;
                posZ = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value;
                if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                {
                    string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                    this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                }
                if (posZ > this.StageSupervisor().PinMaxRegRange)
                {
                    posZ = this.StageSupervisor().PinMaxRegRange;
                }
                if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);
                }
                else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinLowViewMove(posX, posY, posZ);
                }
                LoggerManager.Debug($"[TeachPinModule] Prev() : Move to Previous Pin #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "Prev() : Error occured.");
                LoggerManager.Exception(err);

            }
        }
        private Task DoNextFailPin()
        {
            try
            {
                
                NextFailPin();
            }
            catch (Exception err)
            {
                return Task.FromException<Exception>(err);
            }
            finally
            {
                
            }
            return Task.CompletedTask;
        }
        public void NextFailPin()
        {
            bool isFoundFailPin = false;
            int OriDutIndex = CurDutIndex, OriPinIndex = CurPinArrayIndex;
            try
            {
                int TotalPinNum = 0;
                IPinData tmpPinData;
                int i = 0;
                int j = 0;
                int k = 0;

                TotalPinNum = this.StageSupervisor().ProbeCardInfo.GetPinCount();
                if (TotalPinNum <= 0) return;

                for (i = CurPinIndex + 1; i <= TotalPinNum - 1; i++)
                {
                    tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(i);
                    j = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(i);
                    k = tmpPinData.DutNumber.Value - 1;

                    if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_FORCED_PASS &&
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_SKIP &&
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_NOT_PERFORMED &&
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_PASSED)

                    {
                        isFoundFailPin = true;
                        CurPinIndex = i;
                        CurPinArrayIndex = j;
                        CurDutIndex = k;
                        break;
                    }
                }

                if (isFoundFailPin == false)
                {
                    for (i = 0; i <= CurPinIndex - 1; i++)
                    {
                        tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(i);
                        j = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(i);
                        k = tmpPinData.DutNumber.Value - 1;

                        if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_FORCED_PASS &&
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_SKIP &&
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_NOT_PERFORMED &&
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_PASSED)

                        {
                            isFoundFailPin = true;
                            CurPinIndex = i;
                            CurPinArrayIndex = j;
                            CurDutIndex = k;
                            break;
                        }
                    }
                }

                if (isFoundFailPin)
                {
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                    }
                    if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                    }
                    else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinLowViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                    }
                    LoggerManager.Debug($"[TeachPinModule] NextFailPin() : Move to Next Fail Pin #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err + "NextFailPin() : Error occured.");
            }
        }
        private Task DoPrevFailPin()
        {
            try
            {
                
                PrevFailPin();
            }
            catch (Exception err)
            {
                return Task.FromException<Exception>(err);
            }
            finally
            {
                
            }
            return Task.CompletedTask;
        }
        public void PrevFailPin()
        {
            bool isFoundFailPin = false;
            int OriDutIndex = CurDutIndex, OriPinIndex = CurPinArrayIndex;
            try
            {
                int TotalPinNum = 0;
                IPinData tmpPinData;
                int i = 0;
                int j = 0;
                int k = 0;

                TotalPinNum = this.StageSupervisor().ProbeCardInfo.GetPinCount();

                for (i = CurPinIndex - 1; i >= 0; i--)
                {
                    tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(i);
                    j = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(i);
                    k = tmpPinData.DutNumber.Value - 1;

                    if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_FORCED_PASS &&
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_SKIP &&
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_NOT_PERFORMED &&
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_PASSED)

                    {
                        isFoundFailPin = true;
                        CurPinIndex = i;
                        CurPinArrayIndex = j;
                        CurDutIndex = k;
                        break;
                    }
                }

                if (isFoundFailPin == false)
                {
                    for (i = TotalPinNum - 1; i >= CurPinIndex + 1; i--)
                    {
                        tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(i);
                        j = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(i);
                        k = tmpPinData.DutNumber.Value - 1;

                        if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_FORCED_PASS &&
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_SKIP &&
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_NOT_PERFORMED &&
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_PASSED)

                        {
                            isFoundFailPin = true;
                            CurPinIndex = i;
                            CurPinArrayIndex = j;
                            CurDutIndex = k;
                            break;
                        }
                    }
                }

                if (isFoundFailPin)
                {
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                    }
                    if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                    }
                    else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinLowViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                    }
                    LoggerManager.Debug($"[TeachPinModule] PrevFailPin() : Move to Previous Fail Pin #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err + "PrevFailPin() : Error occured.");
            }
        }

        public Task<EventCodeEnum> InitRecovery()
        {
            throw new NotImplementedException();
        }

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }
    }

}
