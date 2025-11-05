using LogModule;
using MetroDialogInterfaces;
using Newtonsoft.Json;
using PnPControl;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign.ProbeCardData;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

namespace VisionSetupModule
{
    public class BlobSearchAreaSetupModule : PNPSetupBase, ISetup, ITemplateModule, IParamNode, IRecovery, IHasDevParameterizable
    {
        public override bool Initialized { get; set; } = false;
        private PinSetupMode CurrentMode;
        public override Guid ScreenGUID { get; } = new Guid("36CA2A9B-BB3E-49AE-DEA5-CCD2DD7FEEC3");

        //bool isChanged = false;
        private ICamera Cam;
        private ushort Threshold = 0;

        private SubModuleStateBase _ModuleState;
        public SubModuleStateBase SubModuleState
        {
            get { return _ModuleState; }
            set
            {
                if (value != _ModuleState)
                {
                    _ModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int CurPinIndex = 0;
        private int CurDutIndex = 0;
        private int CurPinArrayIndex = 0;


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

        [XmlIgnore, JsonIgnore]
        public new List<object> Nodes { get; set; }

        //private ObservableCollection<IDut> BackupData;

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

        public EventCodeEnum ClearData()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum Execute()
        {
            return EventCodeEnum.NONE;
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleStateEnum.IDLE;
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[BlobSearchAreaSetupModule] InitModule() : InitModule Stert");
            try
            {
                if (Initialized == false)
                {
                    //_CoordinateManager = container.Resolve<ICoordinateManager>();
                    //_PinAligner = container.Resolve<IPinAligner>();

                    CurrMaskingLevel = this.ProberStation().MaskingLevel;

                    //retval = InitBackupData();

                    //if (retval != EventCodeEnum.NONE)
                    //{
                    //    LoggerManager.Error($"InitBackupData() Failed");
                    //}

                    //InitSetup();
                    LoggerManager.Debug($"[BlobSearchAreaSetupModule] InitModule() : InitModule Done");

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
                //LoggerManager.Error($err + "InitModule() : Error occured.");
            }

            return retval;
        }
        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = "Pin Search Area Setup";

                retVal = InitPnpModuleStage();
                retVal = InitLightJog(this);
                //retVal = InitPnpModuleStage_AdvenceSetting();

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
                retVal = await InitSetup();

                InitLightJog(this);

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


        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //InitBackupData();

            LoggerManager.Debug($"[BlobSearchAreaSetupModule] InitSetup() : InitSetup Stert");
            try
            {
                if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count < 1)
                {
                    retVal = EventCodeEnum.NODATA;
                    throw new Exception();
                }

                this.StageSupervisor().ProbeCardInfo.CheckValidPinParameters();

                CurPinIndex = 0;
                CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);
                IPinData tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(CurPinIndex);
                if (tmpPinData != null)
                    CurDutIndex = tmpPinData.DutNumber.Value - 1;
                else
                    CurDutIndex = 0;

                if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                {
                    string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                    this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                    this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                }
                LoggerManager.Debug($"[BlobSearchAreaSetupModule] InitSetup() : Move to Ref Pin");
                this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                LoggerManager.Debug($"[BlobSearchAreaSetupModule] InitSetup() : Move to Ref Pin Done");


                // 로우 조명 설정
                Cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                CurCam = Cam;
                CurCam.SetLight(EnumLightType.COAXIAL, 70);


                Cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);

                CurCam = Cam;

                foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
                {
                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                }


                if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value == 0)
                {
                    Threshold = 127;
                }
                else
                {
                    Threshold = (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value;
                }

                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                retVal = InitPNPSetupUI();
                retVal = InitLightJog(this);
                //retVal = SetDefaultFocusParam();

                //SettingItem = SETTINGITEM.SEARCHAREA;

                //if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width != 0 &&
                //    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height != 0)
                //{
                //    PRSX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width;
                //    PRSY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height;

                //    TargetRectangleWidth = ConvertDisplayWidthPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width, CurCam.Param.GrabSizeX.Value);
                //    TargetRectangleHeight = ConvertDisplayHeightPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height, CurCam.Param.GrabSizeY.Value);
                //    LoggerManager.Debug($"[BlobSearchAreaSetupModule] InitSetup() : Current Search Area Size X = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width.ToString() + " Y = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height.ToString());
                //}
                //else
                //{
                //    PRSX = 120;
                //    PRSY = 120;

                //    TargetRectangleWidth = ConvertDisplayWidthPNP(120, CurCam.Param.GrabSizeX.Value);
                //    TargetRectangleHeight = ConvertDisplayHeightPNP(120, CurCam.Param.GrabSizeY.Value);
                //}

                //UseUserControl = UserControlFucEnum.PTRECT;

                MainViewTarget = DisplayPort;
                MiniViewTarget = null;

                //RectSizeRaitioX = (double)CurCam.Param.GrabSizeX.Value / 890;
                //RectSizeRaitioY = (double)CurCam.Param.GrabSizeY.Value / 890;

                LoadDevParameter();

                this.PinAligner().StopDrawDutOverlay(CurCam);
                this.PinAligner().DrawDutOverlay(CurCam);

                ChangeModeCommandFunc(PinSetupMode.TIPSEARCHAREA);

                //await Task.Run(() =>
                //{
                //    ImageProcessingEx();
                //});

                //ImageProcessingEx();

                LoggerManager.Debug($"[BlobSearchAreaSetupModule] InitSetup() : InitSetup Done");
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Error($err + "InitSetup() : Error occured.");
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        //public EventCodeEnum SetDefaultFocusParam()
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        if (this.FocusManager() != null)
        //        {
        //            retval = this.FocusManager().ValidationFocusParam(DefaultFocusParam);

        //            if (retval != EventCodeEnum.NONE)
        //            {
        //                this.FocusManager().MakeDefalutFocusParam(EnumProberCam.PIN_HIGH_CAM, EnumAxisConstants.PZ, DefaultFocusParam);
        //            }
        //        }
        //        if (PinAlignParam.PinHighAlignTipRoughParam.FocusingParam == null)
        //        {
        //            PinAlignParam.PinHighAlignTipRoughParam.FocusingParam = new NormalFocusParameter();
        //            PinAlignParam.PinHighAlignTipRoughParam.FocusingParam = DefaultFocusParam;
        //        }
        //        if ((PinAlignParam.PinHighAlignTipRoughParam.FocusingParam as FocusParameter).LightParams.Count == 0)
        //        {
        //            LightValueParam tmp1 = new LightValueParam();
        //            tmp1.Type.Value = EnumLightType.COAXIAL;
        //            tmp1.Value.Value = 100;
        //            (PinAlignParam.PinHighAlignTipRoughParam.FocusingParam as FocusParameter).LightParams.Add(tmp1);
        //        }

        //        if (PinAlignParam.PinHighAlignTipFocusingParam.FocusingParam == null)
        //        {
        //            PinAlignParam.PinHighAlignTipFocusingParam.FocusingParam = new NormalFocusParameter();
        //            PinAlignParam.PinHighAlignTipFocusingParam.FocusingParam = DefaultFocusParam;
        //        }
        //        if ((PinAlignParam.PinHighAlignTipFocusingParam.FocusingParam as FocusParameter).LightParams.Count == 0)
        //        {
        //            LightValueParam tmp1 = new LightValueParam();
        //            tmp1.Type.Value = EnumLightType.COAXIAL;
        //            tmp1.Value.Value = 100;
        //            (PinAlignParam.PinHighAlignTipFocusingParam.FocusingParam as FocusParameter).LightParams.Add(tmp1);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                MainViewTarget = DisplayPort;

                Header = "Pin Search Area Setup";

                ProcessingType = EnumSetupProgressState.IDLE;

                PadJogLeftUp.Caption = "Prev";
                PadJogRightUp.Caption = "Next";
                PadJogRightDown.Caption = "";
                PadJogRightDown.Caption = "";
                PadJogSelect.Caption = "";

                PadJogLeftUp.Command = new AsyncCommand(DoPrev);
                PadJogRightUp.Command = new AsyncCommand(DoNext);

                //PadJogSelect.Command = new RelayCommand(Set);

                //PadJogUp.CaptionSize = 36;
                //PadJogUp.Caption = "+";
                //PadJogUp.Command = new RelayCommand(RectHeightSizeUp);
                //PadJogUp.RepeatEnable = true;

                //PadJogDown.CaptionSize = 36;
                //PadJogDown.Caption = "-";
                //PadJogDown.Command = new RelayCommand(RectHeightSizeDown);
                //PadJogDown.RepeatEnable = true;

                //PadJogLeft.CaptionSize = 36;
                //PadJogLeft.Caption = "-";
                //PadJogLeft.Command = new RelayCommand(RectWidthSizeDown);
                //PadJogLeft.RepeatEnable = true;

                //PadJogRight.CaptionSize = 36;
                //PadJogRight.Caption = "+";
                //PadJogRight.Command = new RelayCommand(RectWidthSizeUp);
                //PadJogRight.RepeatEnable = true;

                PadJogUp.Caption = "▲";
                PadJogUp.Command = ChangeSearchAreaCommand;
                PadJogUp.CommandParameter = EnumArrowDirection.UP;
                PadJogUp.RepeatEnable = true;

                PadJogDown.Caption = "▼";
                PadJogDown.Command = ChangeSearchAreaCommand;
                PadJogDown.CommandParameter = EnumArrowDirection.DOWN;
                PadJogDown.RepeatEnable = true;

                PadJogLeft.Caption = "◀";
                PadJogLeft.Command = ChangeSearchAreaCommand;
                PadJogLeft.CommandParameter = EnumArrowDirection.LEFT;
                PadJogLeft.RepeatEnable = true;

                PadJogRight.Caption = "▶";
                PadJogRight.Command = ChangeSearchAreaCommand;
                PadJogRight.CommandParameter = EnumArrowDirection.RIGHT;
                PadJogRight.RepeatEnable = true;

                PadJogUp.IsEnabled = true;
                PadJogDown.IsEnabled = true;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;
                PadJogRightDown.IsEnabled = false;
                PadJogLeftUp.IsEnabled = true;
                PadJogRightUp.IsEnabled = true;
                PadJogLeftDown.IsEnabled = false;
                PadJogSelect.IsEnabled = false;
                PadJogUp.IsEnabled = true;
                PadJogDown.IsEnabled = true;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;

                MainViewZoomVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                OneButton.Visibility = System.Windows.Visibility.Visible;
                TwoButton.Visibility = System.Windows.Visibility.Visible;//==> Focus
                ThreeButton.Visibility = System.Windows.Visibility.Hidden;
                FourButton.Visibility = System.Windows.Visibility.Hidden;
                FiveButton.Visibility = System.Windows.Visibility.Collapsed;

                OneButton.Caption = "Set All";
                TwoButton.Caption = "Focusing";
                TwoButton.CaptionSize = 17;

                OneButton.Command = Button1Command;
                TwoButton.Command = Button2Command;

                PnpManager.PnpLightJog.HighBtnEventHandler = new RelayCommand(CameraHighButton);
                PnpManager.PnpLightJog.LowBtnEventHandler = new RelayCommand(CameraLowButton);

                UseUserControl = UserControlFucEnum.PTRECT;

                if (this.PinAligner().IsRecoveryStarted == true) //this.PinAligner().GetPAInnerStateEnum() == PinAlignInnerStateEnum.RECOVERY)
                {
                    PadJogRightDown.Caption = "Next\nFail";
                    PadJogLeftDown.Caption = "Prev\nFail";
                    PadJogRightDown.IsEnabled = true;
                    PadJogLeftDown.IsEnabled = true;
                    PadJogLeftDown.Command = new AsyncCommand(DoPrevFailPin);
                    PadJogRightDown.Command = new AsyncCommand(DoNextFailPin);
                    FourButton.Caption = "Show\nResult";
                    //FourButton.Command = Button4Command;
                    FourButton.Visibility = System.Windows.Visibility.Visible;
                }

                UseUserControl = UserControlFucEnum.DEFAULT;
                TargetRectangleHeight = 0;
                TargetRectangleWidth = 0;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {

                throw err;
            }
            return retVal;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            if (FocusParam == null)
            {

            }

            FocusParam.FocusingAxis.Value = this.StageSupervisor().StageModuleState.PinViewAxis;

            return retval;
        }

        public override EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
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


        public EventCodeEnum SaveDevParameter()
        {
            return SaveProbeCardData();
        }

        public EventCodeEnum StartJob()
        {
            return EventCodeEnum.NONE;
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
                this.PinAligner().DrawDutOverlay(CurCam);                
            }
            return Task.CompletedTask;
        }
        public void Next()
        {
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
                CurDutIndex = tmpPinData.DutNumber.Value - 1;

                // CurPinArrayIndex는 현재 선택된 실제 핀 번호이지만, 데이터 상으로는 더트별로 해당 핀이 들어가 있는 배열의 인덱스는 핀 번호하고는 다르다.
                // 따라서 배열상의 어레이로 변환해 주어야 한다.
                CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                foreach (var light in pininfo.PinSearchParam.LightForTip)
                {
                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                }

                //int light = 0;
                //if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value == 0)
                //{
                //    light = 85;
                //}
                //else
                //{
                //    light = (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value;
                //}
                //Cam.SetLight(EnumLightType.COAXIAL, Convert.ToUInt16(light));

                if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinHighViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);

                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                    }
                }
                else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);

                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                    }
                }

                Threshold = (ushort)pininfo.PinSearchParam.BlobThreshold.Value;
                //Cam.SetLight(EnumLightType.COAXIAL, (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value);
                //PRSY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height;
                //PRSX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width;

                //TargetRectangleWidth = ConvertDisplayWidthPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width, CurCam.Param.GrabSizeX.Value);
                //TargetRectangleHeight = ConvertDisplayHeightPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height, CurCam.Param.GrabSizeY.Value);
                //LoggerManager.Debug($"[BlobSearchAreaSetupModule] Next() : Move to Next Pin #" + pininfo.PinNum.Value + " Search Area X = " + PRSX.ToString() + " Y = " + PRSY.ToString());

                LoggerManager.Debug($"[BlobSearchAreaSetupModule] Next() : Move to Next Pin #" + pininfo.PinNum.Value + " Search Area X = " + pininfo.PinSearchParam.SearchArea.Value.Width + " Y = " + pininfo.PinSearchParam.SearchArea.Value.Height);

                //ImageProcessingEx();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Error($err + "Next() : Error occured.");
            }
            //this.StageSupervisor().StageModuleState.PinHighViewMove(ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
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
                this.PinAligner().DrawDutOverlay(CurCam);
                
            }
            return Task.CompletedTask;
        }
        public void Prev()
        {
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
                CurDutIndex = tmpPinData.DutNumber.Value - 1;

                // CurPinArrayIndex는 현재 선택된 실제 핀 번호이지만, 데이터 상으로는 더트별로 해당 핀이 들어가 있는 배열의 인덱스는 핀 번호하고는 다르다.
                // 따라서 배열상의 어레이로 변환해 주어야 한다.
                CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                foreach (var light in pininfo.PinSearchParam.LightForTip)
                {
                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                }

                //int light = 0;
                //if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value == 0)
                //{
                //    light = 85;
                //}
                //else
                //{
                //    light = (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value;
                //}
                //Cam.SetLight(EnumLightType.COAXIAL, Convert.ToUInt16(light));

                if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinHighViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                    }
                }
                else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                    }
                }
                Threshold = (ushort)pininfo.PinSearchParam.BlobThreshold.Value;
                //Cam.SetLight(EnumLightType.COAXIAL, (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value);
                //PRSY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height;
                //PRSX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width;

                //TargetRectangleWidth = ConvertDisplayWidthPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width, CurCam.Param.GrabSizeX.Value);
                //TargetRectangleHeight = ConvertDisplayHeightPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height, CurCam.Param.GrabSizeY.Value);
                //LoggerManager.Debug($"[BlobSearchAreaSetupModule] Prev() : Move to Previous Pin #" + pininfo.PinNum.Value + " Search Area X = " + PRSX.ToString() + " Y = " + PRSY.ToString());
                LoggerManager.Debug($"[BlobSearchAreaSetupModule] Prev() : Move to Previous Pin #" + pininfo.PinNum.Value + " Search Area X = " + pininfo.PinSearchParam.SearchArea.Value.Width + " Y = " + pininfo.PinSearchParam.SearchArea.Value.Height);

                //ImageProcessingEx();
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "Prev() : Error occured.");
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
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                this.PinAligner().DrawDutOverlay(CurCam);
                
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
                    IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                    if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinHighViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                        }
                    }
                    else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                        }
                    }

                    foreach (var light in pininfo.PinSearchParam.LightForTip)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }

                    Threshold = (ushort)pininfo.PinSearchParam.BlobThreshold.Value;
                    //Cam.SetLight(EnumLightType.COAXIAL, (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value);
                    //PRSY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height;
                    //PRSX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width;

                    //TargetRectangleWidth = ConvertDisplayWidthPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width, CurCam.Param.GrabSizeX.Value);
                    //TargetRectangleHeight = ConvertDisplayHeightPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height, CurCam.Param.GrabSizeY.Value);
                    LoggerManager.Debug($"[BlobSearchAreaSetupModule] NextFailPin() : Move to Next Fail Pin #" + pininfo.PinNum.Value + " Search Area X = " + pininfo.PinSearchParam.SearchArea.Value.Width + " Y = " + pininfo.PinSearchParam.SearchArea.Value.Height);
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
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                this.PinAligner().DrawDutOverlay(CurCam);
                
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
                    IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                    if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinHighViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                        }
                    }
                    else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                        }
                    }

                    foreach (var light in pininfo.PinSearchParam.LightForTip)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }

                    Threshold = (ushort)pininfo.PinSearchParam.BlobThreshold.Value;
                    //Cam.SetLight(EnumLightType.COAXIAL, (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value);
                    //PRSY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height;
                    //PRSX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width;

                    //TargetRectangleWidth = ConvertDisplayWidthPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width, CurCam.Param.GrabSizeX.Value);
                    //TargetRectangleHeight = ConvertDisplayHeightPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height, CurCam.Param.GrabSizeY.Value);
                    //LoggerManager.Debug($"[BlobSearchAreaSetupModule] PrevFailPin() : Move to Previous Fail Pin #" + pininfo.PinNum.Value + " Search Area X = " + PRSX.ToString() + " Y = " + PRSY.ToString());
                    LoggerManager.Debug($"[BlobSearchAreaSetupModule] PrevFailPin() : Move to Previous Fail Pin #" + pininfo.PinNum.Value + " Search Area X = " + pininfo.PinSearchParam.SearchArea.Value.Width + " Y = " + pininfo.PinSearchParam.SearchArea.Value.Height);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err + "PrevFailPin() : Error occured.");
            }
        }

        private Task DoSet()
        {
            try
            {
                
                Set();
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
        public void Set()
        {
            try
            {
                LoggerManager.Debug($"[BlobSearchAreaSetupModule] Set() : Set Search Area Start");
                LoggerManager.Debug($"[BlobSearchAreaSetupModule] Set() : Currernt Pin #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value);
                //LoggerManager.Debug($"[BlobSearchAreaSetupModule] Set() : Original Search Area  X = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width.ToString() + " Y = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height.ToString());
                LoggerManager.Debug($"[BlobSearchAreaSetupModule] Set() : Search Area  X = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width.ToString() + " Y = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height.ToString());

                // 이미 업데이트 되어 있음.
                //UpdateData();

                SaveProbeCardData();
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "Set() : Error occured.");
            }
            LoggerManager.Debug($"[BlobSearchAreaSetupModule] Set() : Set Search Area Done");
            //this.StageSupervisor().StageModuleState.PinHighViewMove(ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
        }

        private RelayCommand<EnumArrowDirection> _ChangeSizeCommand;
        public ICommand ChangeSearchAreaCommand
        {
            get
            {
                if (null == _ChangeSizeCommand)
                    _ChangeSizeCommand = new RelayCommand<EnumArrowDirection>(ChangeSizeCommandFunc);
                return _ChangeSizeCommand;
            }
        }

        private void ChangeSizeCommandFunc(EnumArrowDirection param)
        {
            try
            {
                double PossibleMaxSizeX = CurCam.GetGrabSizeWidth();
                double PossibleMaxSizeY = CurCam.GetGrabSizeHeight();

                double offsetX = 0;
                double offsetY = 0;

                switch (param)
                {
                    case EnumArrowDirection.LEFTUP:
                    case EnumArrowDirection.RIGHTUP:
                    case EnumArrowDirection.LEFTDOWN:
                    case EnumArrowDirection.RIGHTDOWN:

                        LoggerManager.Debug("Not Support.");
                        break;

                    case EnumArrowDirection.UP:
                        offsetY++;
                        break;
                    case EnumArrowDirection.LEFT:
                        offsetX--;
                        break;
                    case EnumArrowDirection.RIGHT:
                        offsetX++;
                        break;
                    case EnumArrowDirection.DOWN:
                        offsetY--;
                        break;
                    default:
                        break;
                }

                bool changed = false;

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (param == EnumArrowDirection.LEFT || param == EnumArrowDirection.RIGHT)
                {
                    if ((pininfo.PinSearchParam.SearchArea.Value.Width + offsetX) != 0 && (offsetX != 0) && (pininfo.PinSearchParam.SearchArea.Value.Width + offsetX < PossibleMaxSizeX))
                    {
                        pininfo.PinSearchParam.SearchArea.Value = new Rect(0, 0, pininfo.PinSearchParam.SearchArea.Value.Width + offsetX, pininfo.PinSearchParam.SearchArea.Value.Height + offsetY);
                        changed = true;
                    }
                }

                if (param == EnumArrowDirection.UP || param == EnumArrowDirection.DOWN)
                {
                    if ((pininfo.PinSearchParam.SearchArea.Value.Height + offsetY) != 0 && (offsetY != 0) && (pininfo.PinSearchParam.SearchArea.Value.Height + offsetY < PossibleMaxSizeY))
                    {
                        pininfo.PinSearchParam.SearchArea.Value = new Rect(0, 0, pininfo.PinSearchParam.SearchArea.Value.Width + offsetX, pininfo.PinSearchParam.SearchArea.Value.Height + offsetY);
                        changed = true;
                    }
                }


                if (changed == true)
                {
                    CurCam.UpdateOverlayFlag = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ChangeModeCommandFunc(PinSetupMode mode)
        {
            try
            {
                CurrentMode = mode;

                if (CurrentMode == PinSetupMode.TIPSEARCHAREA)
                {
                    this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.TIPSEARCHAREA, true);
                }
                else
                {
                    LoggerManager.Debug($"Mode is wrong {mode}");
                }

                CurCam.UpdateOverlayFlag = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public void RectHeightSizeUp()
        //{
        //    try
        //    {
        //        isChanged = true;

        //        if (TargetRectangleHeight < 887)
        //        {
        //            TargetRectangleHeight = TargetRectangleHeight + 4;
        //        }

        //        RegisteImageBufferParam param = GetDisplayPortRectInfo();

        //        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value = new Rect(0, 0, TargetRectangleWidth * RectSizeRaitioX, TargetRectangleHeight * RectSizeRaitioX);

        //        PRSY = TargetRectangleHeight * RectSizeRaitioX;

        //        //PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value
        //        //    = new Rect(
        //        //        (Cam.Param.GrabSizeX.Value / 2) - ((TargetRectangleWidth * RectSizeRaitioX) / 2),
        //        //        (Cam.Param.GrabSizeY.Value / 2) - ((TargetRectangleHeight * RectSizeRaitioY) / 2),
        //        //        TargetRectangleWidth * RectSizeRaitioX,
        //        //        TargetRectangleHeight * RectSizeRaitioY
        //        //        );
        //        //ImageProcessingEx();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug(err + "RectHeightSizeUp() : Error occured.");
        //    }
        //}
        //public void RectHeightSizeDown()
        //{
        //    try
        //    {
        //        isChanged = true;

        //        if (TargetRectangleHeight > 4)
        //        {
        //            TargetRectangleHeight = TargetRectangleHeight - 4;
        //        }

        //        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value = new Rect(0, 0, TargetRectangleWidth * RectSizeRaitioX, TargetRectangleHeight * RectSizeRaitioX);
        //        PRSY = TargetRectangleHeight * RectSizeRaitioX;

        //        //PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value
        //        //    = new Rect(
        //        //        (Cam.Param.GrabSizeX.Value / 2) - ((TargetRectangleWidth * RectSizeRaitioX) / 2),
        //        //        (Cam.Param.GrabSizeY.Value / 2) - ((TargetRectangleHeight * RectSizeRaitioY) / 2),
        //        //        TargetRectangleWidth * RectSizeRaitioX,
        //        //        TargetRectangleHeight * RectSizeRaitioY
        //        //        );

        //        //ImageProcessingEx();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug(err + "RectHeightSizeDown() : Error occured.");
        //    }
        //}
        //public void RectWidthSizeUp()
        //{
        //    try
        //    {
        //        isChanged = true;
        //        if (TargetRectangleWidth < 887)
        //        {
        //            TargetRectangleWidth = TargetRectangleWidth + 4;
        //            TargetRectangleHeight = TargetRectangleHeight;
        //        }

        //        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value = new Rect(0, 0, TargetRectangleWidth * RectSizeRaitioX, TargetRectangleHeight * RectSizeRaitioX);
        //        PRSY = TargetRectangleWidth * RectSizeRaitioX;

        //        //PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value
        //        //    = new Rect(
        //        //        (Cam.Param.GrabSizeX.Value / 2) - ((TargetRectangleWidth * RectSizeRaitioX) / 2),
        //        //        (Cam.Param.GrabSizeY.Value / 2) - ((TargetRectangleHeight * RectSizeRaitioY) / 2),
        //        //        TargetRectangleWidth * RectSizeRaitioX,
        //        //        TargetRectangleHeight * RectSizeRaitioY
        //        //        );
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug(err + "RectWidthSizeUp() : Error occured.");
        //    }
        //}
        //public void RectWidthSizeDown()
        //{
        //    try
        //    {
        //        isChanged = true;
        //        if (TargetRectangleWidth > 4)
        //        {

        //            TargetRectangleWidth = TargetRectangleWidth - 4;
        //            TargetRectangleHeight = TargetRectangleHeight;
        //        }

        //        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value = new Rect(0, 0, TargetRectangleWidth * RectSizeRaitioX, TargetRectangleHeight * RectSizeRaitioX);
        //        PRSY = TargetRectangleWidth * RectSizeRaitioX;

        //        //PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value
        //        //    = new Rect(
        //        //        (Cam.Param.GrabSizeX.Value / 2) - ((TargetRectangleWidth * RectSizeRaitioX) / 2),
        //        //        (Cam.Param.GrabSizeY.Value / 2) - ((TargetRectangleHeight * RectSizeRaitioY) / 2),
        //        //        TargetRectangleWidth * RectSizeRaitioX,
        //        //        TargetRectangleHeight * RectSizeRaitioY
        //        //        );
        //        //ImageProcessingEx();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug(err + "RectWidthSizeDown() : Error occured.");
        //    }
        //}

        #region pnp button 1
        private AsyncCommand _Button1Command;
        public ICommand Button1Command
        {
            get
            {
                if (null == _Button1Command)
                    _Button1Command = new AsyncCommand(DoConfirmToSetAll);

                return _Button1Command;

                //if (null == _Button1Command)
                //    _Button1Command = new RelayCommand(                    
                //    SetAll//, EvaluationPrivilege.Evaluate(
                //          // CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                //          // new Action(() => { ShowMessages("UIModeChange"); })
                //            );
                //return _Button1Command;
            }
        }

        private async Task DoConfirmToSetAll()
        {
            try
            {
                

                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Confirm To Change All",
                        "Search area for all of pins will be updated at once, \nDo you want to proceed?", EnumMessageStyle.AffirmativeAndNegative);

                if (result == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    SetAll();
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

        //private void ConfirmToSetAll()
        //{
        //    try
        //    {
        //        

        //        Task t = Task.Run(async () =>
        //        {
        //            EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Confirm To Change All",
        //                "Search area for all of pins will be updated at once, \nDo you want to proceed?", EnumMessageStyle.AffirmativeAndNegative);
        //            if (result == EnumMessageDialogResult.AFFIRMATIVE)
        //            {
        //                SetAll();
        //            }
        //        });
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug(err + "ConfirmToSetAll() : Error occured.");
        //    }
        //    finally
        //    {
        //        
        //    }
        //}


        public void SetAll()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                LoggerManager.Debug($"[BlobSearchAreaSetupModule] SetAll() : Set All Search Area Start");
                LoggerManager.Debug($"[BlobSearchAreaSetupModule] SetAll() : Set All Search Area  X = " + pininfo.PinSearchParam.SearchArea.Value.Width + " Y = " + pininfo.PinSearchParam.SearchArea.Value.Height);

                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (IPinData pin in dut.PinList)
                    {
                        pin.PinSearchParam.SearchArea.Value = new Rect(0, 0, pininfo.PinSearchParam.SearchArea.Value.Width, pininfo.PinSearchParam.SearchArea.Value.Height);

                        //if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value != pin.PinNum.Value)
                        //{
                        //    pin.PinSearchParam.SearchArea.Value = new Rect(0, 0, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height);
                        //}
                    }
                }

                //for (int i = 0; i < CurCam.LightsChannels.Count; i++)
                //{
                //    int val = CurCam.GetLight(CurCam.LightsChannels[i].Type.Value);
                //    if ((PinAlignParam.PinHighAlignTipRoughParam.FocusingParam as FocusParameter).LightParams.Count > i)
                //    {
                //        (PinAlignParam.PinHighAlignTipRoughParam.FocusingParam as FocusParameter).LightParams[i].Value.Value = (ushort)val;
                //    }
                //    if ((PinAlignParam.PinHighAlignTipFocusingParam.FocusingParam as FocusParameter).LightParams.Count > i)
                //    {
                //        (PinAlignParam.PinHighAlignTipFocusingParam.FocusingParam as FocusParameter).LightParams[i].Value.Value = (ushort)val;
                //    }
                //}

                // 이미 업데이트 되어 있음.
                //UpdateData();

                retval = SaveProbeCardData();
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "SetAll() : Error occured.");
            }
            LoggerManager.Debug($"[BlobSearchAreaSetupModule] SetAll() : Set All Search Area Done");
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
                    //await this.ViewModelManager().LockNotification(this.GetHashCode(), "Wait", "Pin Focusing");

                    LoggerManager.Debug($"[BlobSearchAreaSetupModule] DoFocusing() : One pin focusing Start");
                    DoingFocusing = true;

                    await Focusing();
                    DoingFocusing = false;
                    LoggerManager.Debug($"[BlobSearchAreaSetupModule] DoFocusing() : One pin focusing done");
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
        private Task Focusing()
        {
            try
            {
                FocusParam.FocusingCam.Value = CurCam.GetChannelType();
                if (FocusParam.FocusingAxis.Value != EnumAxisConstants.PZ)
                {
                    FocusParam.FocusingAxis.Value = EnumAxisConstants.PZ;
                }
                ICamera cam = this.VisionManager().GetCam(FocusParam.FocusingCam.Value);

                int OffsetX = cam.Param.GrabSizeX.Value / 2 - Convert.ToInt32(Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width)) / 2;
                int OffsetY = cam.Param.GrabSizeY.Value / 2 - Convert.ToInt32(Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height)) / 2;

                FocusParam.FocusingROI.Value = new System.Windows.Rect(OffsetX, OffsetY, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height);

                //LoggerManager.Debug($"[BlobSearchAreaSetupModule] Focusing() : Focusing Parameter ROI X = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width.ToString() + " Y = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height.ToString());
                //FocusingParam.FocusingROI.Value = new System.Windows.Rect(OffsetX, OffsetY, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height);
                FocusParam.FocusRange.Value = Convert.ToInt32(PinAlignParam.PinFocusingRange.Value);
                LoggerManager.Debug($"[BlobSearchAreaSetupModule] Focusing() : Focusing Parameter Focusing Range = " + FocusParam.FocusRange.Value.ToString());

                // ann todo -> OutFocusLimit 주석
                //FocusParam.OutFocusLimit.Value = 40;
                FocusParam.DepthOfField.Value = 1;
                FocusParam.PeakRangeThreshold.Value = 100;


                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                if (PinFocusModel.Focusing_Retry(FocusParam, false, false, false, this) != EventCodeEnum.NONE)
                {
                    //this.ViewModelManager().ShowNotifyToastMessage(this.GetHashCode(), "", "Focusing Fail", 2);
                    LoggerManager.Debug($"[BlobSearchAreaSetupModule] Focusing() : Focusing Fail");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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

        public override async Task Save()
        {
            try
            {
                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Vision Setup Save",
                    "Do you want to save the contents of the Vision Setup ?", EnumMessageStyle.AffirmativeAndNegative);

                if (result == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    foreach (var module in this.PinAligner().Template)
                    {
                        //if (module.GetType().GetInterfaces().Contains(typeof(ISubModule)))
                        //{
                        //    ((ISubModule)module).SaveDevParameter();
                        //}

                        if (module.GetType().GetInterfaces().Contains(typeof(IHasDevParameterizable)))
                        {
                            ((IHasDevParameterizable)module).SaveDevParameter();
                        }
                    }

                    this.SaveParameter(((IParam)this.StageSupervisor().WaferObject));

                    await this.ViewModelManager().BackPreScreenTransition();
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Debug($err);
            }
        }

        public override async Task Exit()
        {
            try
            {
                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Vision Setup Exit",
                   "If you exit now, Setup will exit without saving.", EnumMessageStyle.AffirmativeAndNegative);

                if (result == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    await this.ViewModelManager().BackPreScreenTransition();
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Debug($err);
            }
        }

        public EventCodeEnum SaveProbeCardData()
        {
            EventCodeEnum serialRes = EventCodeEnum.UNDEFINED;

            LoggerManager.Debug($"[BlobSearchAreaSetupModule] SaveProbeCardData() : Save Start");
            //UpdateData();

            try
            {
                serialRes = this.StageSupervisor().SaveProberCard();

                if (serialRes == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug("SaveDevParameter() : Save Ok.");
                }
                else
                {
                    LoggerManager.Debug("SaveDevParameter() : Save Fail.");
                }
                LoggerManager.Debug($"[BlobSearchAreaSetupModule] SaveProbeCardData() : Save Done");
            }
            catch (Exception err)
            {
                throw err;
            }
            return serialRes;
        }
        //private EventCodeEnum InitBackupData()
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    LoggerManager.Debug($"[BlobSearchAreaSetupModule] InitBackupData() : Make Backup Data Stert");
        //    try
        //    {
        //        if (BackupData != null)
        //        {
        //            BackupData.Clear();
        //        }
        //        else
        //        {
        //            BackupData = new ObservableCollection<IDut>();
        //        }

        //        foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
        //        {
        //            BackupData.Add(new Dut(dut));
        //        }
        //        LoggerManager.Debug($"[BlobSearchAreaSetupModule] InitBackupData() : Make Backup Data Done");

        //        retval = EventCodeEnum.NONE;
        //    }
        //    catch
        //    {
        //        //  SubModuleState.SetRecoverySate();
        //        //this.AlignModuleState = new PinHighAlignRecoveryedState(this);
        //        //LoggerManager.Debug("PinAlign : InitPinAlignResult() Error Occured");
        //        LoggerManager.Debug($": InitBackupData() ERROR");
        //    }

        //    return retval;
        //}
        public EventCodeEnum UpdateData()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            //EventCodeEnum ProbeCard = EventCodeEnum.UNDEFINED;
            //EventCodeEnum serialRes = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[BlobSearchAreaSetupModule] UpdateData() : Data Update Start");
            try
            {
                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (IDut oridut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                    {
                        if (dut.MacIndex.XIndex == oridut.MacIndex.XIndex && dut.MacIndex.YIndex == oridut.MacIndex.YIndex)
                        {
                            foreach (IPinData pin in dut.PinList)
                            {
                                foreach (IPinData oripin in oridut.PinList)
                                {
                                    if (pin.PinNum.Value == oripin.PinNum.Value)
                                    {
                                        oripin.PinSearchParam.SearchArea.Value = new Rect(0, 0, pin.PinSearchParam.SearchArea.Value.Width, pin.PinSearchParam.SearchArea.Value.Height);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "UpdataData() : Error occured.");
            }
            LoggerManager.Debug($"[BlobSearchAreaSetupModule] UpdateData() : Data Update Done");
            return retval;
        }

        public bool IsExecute()
        {
            return true;
        }

        public EventCodeEnum Recovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = DoRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public MovingStateEnum GetMovingState()
        {
            return MovingStateEnum.STOP;
        }

        public EventCodeEnum ExitRecovery()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DoExecute()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DoClearData()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DoRecovery()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DoExitRecovery()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Set();

                SetNodeSetupState(EnumMoudleSetupState.NONE);
                //this.PinAligner().StopDrawPinOverlay(CurCam);
                this.PinAligner().StopDrawDutOverlay(CurCam);

                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.TIPSEARCHAREA, false);

                retVal = await base.Cleanup(parameter);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
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
