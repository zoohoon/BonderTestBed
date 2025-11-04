using BaseFocusingAdvanceSetup.View;
using PnPControl;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using RelayCommandBase;
using SerializerUtil;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LogModule;
using BaseFocusingAdvanceSetup.ViewModel;
using ProberInterfaces.PinAlign.ProbeCardData;
using ProbeCardObject;
using MetroDialogInterfaces;
using ProberInterfaces.Param;
using static ProberInterfaces.Param.PinSearchParameter;
using Focusing;

namespace BaseFocusingModule
{
    public class BaseFocusingModule : PNPSetupBase, ISetup, IRecovery, IPackagable
    {
        public override bool Initialized { get; set; } = false;
        private PinSetupMode CurrentMode;

        public override Guid ScreenGUID { get; } = new Guid("8E781BDD-ACBD-48D1-80D4-31322749150B");

        //double RectSizeRaitioX = 0;
        //double RectSizeRaitioY = 0;

        private ICamera Cam;

        public Task<EventCodeEnum> InitRecovery()
        {
            throw new NotImplementedException();
        }

        private int CurPinIndex = 0;
        private int CurDutIndex = 0;
        private int CurPinArrayIndex = 0;

        private PinAlignDevParameters PinAlignParam => (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);

        private bool bToogleStateCardBase = false;

        private IFocusParameter FocusParam => (this.PinAligner().PinAlignDevParam as PinAlignDevParameters)?.FocusParam;

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

        private FocusParameter _DefaultFocusParam = new NormalFocusParameter();
        public FocusParameter DefaultFocusParam
        {
            get { return _DefaultFocusParam; }
            set
            {
                if (value != _DefaultFocusParam)
                {
                    _DefaultFocusParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[BaseFocusingModule] InitSetup() : InitSetup Stert");
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

                this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);

                Cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                CurCam = Cam;

                this.PnPManager().RestoreLastLightSetting(CurCam);

                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                retVal = InitPNPSetupUI();
                retVal = InitLightJog(this);
                retVal = SetDefaultFocusParam();

                if (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value.Width != 0 &&
                    PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value.Height != 0)
                {
                    TargetRectangleWidth = ConvertDisplayWidthPNP(PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value.Width, CurCam.Param.GrabSizeX.Value);
                    TargetRectangleHeight = ConvertDisplayHeightPNP(PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value.Height, CurCam.Param.GrabSizeY.Value);
                }
                else
                {
                    TargetRectangleWidth = ConvertDisplayWidthPNP(120, CurCam.Param.GrabSizeX.Value);
                    TargetRectangleHeight = ConvertDisplayHeightPNP(120, CurCam.Param.GrabSizeY.Value);
                }

                if (TargetRectangleWidth >= 890)
                {
                    TargetRectangleWidth = 886;
                }
                if (TargetRectangleHeight >= 890)
                {
                    TargetRectangleHeight = 886;
                }

                //UseUserControl = UserControlFucEnum.PTRECT;

                MainViewTarget = DisplayPort;
                MiniViewTarget = null;

                //RectSizeRaitioX = (double)CurCam.Param.GrabSizeX.Value / 890;
                //RectSizeRaitioY = (double)CurCam.Param.GrabSizeY.Value / 890;

                CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                this.PinAligner().StopDrawDutOverlay(CurCam);
                this.PinAligner().DrawDutOverlay(CurCam);

                ChangeModeCommandFunc(PinSetupMode.BASEFOCUSINGAREA);

                UpdateLabel();

                LoggerManager.Debug($"[BaseFocusingModule] InitSetup() : InitSetup Done");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private void ChangeModeCommandFunc(PinSetupMode mode)
        {
            try
            {
                CurrentMode = mode;

                if (CurrentMode == PinSetupMode.BASEFOCUSINGAREA)
                {
                    this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.BASEFOCUSINGAREA, true);
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

        public EventCodeEnum SetDefaultFocusParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.FocusManager() != null)
                {
                    retval = this.FocusManager().ValidationFocusParam(DefaultFocusParam);

                    if (retval != EventCodeEnum.NONE)
                    {
                        this.FocusManager().MakeDefalutFocusParam(EnumProberCam.PIN_HIGH_CAM, EnumAxisConstants.PZ, DefaultFocusParam);
                    }
                }
                if (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam == null)
                {
                    PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam = new NormalFocusParameter();
                    PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam = DefaultFocusParam;
                }
                if ((PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams.Count != 2)
                {
                    (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams.Clear();
                    LightValueParam tmp1 = new LightValueParam();
                    tmp1.Type.Value = EnumLightType.COAXIAL;
                    tmp1.Value.Value = 100;
                    (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams.Add(tmp1);
                    tmp1 = new LightValueParam();
                    tmp1.Type.Value = EnumLightType.AUX;
                    tmp1.Value.Value = 0;
                    (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams.Add(tmp1);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public override void UpdateLabel()
        {
            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                BaseInfo baseinfo = pininfo.PinSearchParam.BaseParam;

                double relativeDisZ = baseinfo.DistanceBaseAndTip;

                StepLabel = $"Base ZPos - Pin ZPos: {relativeDisZ}um, OffsetX : {baseinfo.BaseOffsetX}, OffsetY : {baseinfo.BaseOffsetY}";

                LoggerManager.Debug($"[BaseFocusingModule] UpdateLabel() : [Base ZPos - Pin ZPos: {relativeDisZ}um], OffsetX : {baseinfo.BaseOffsetX}, OffsetY : {baseinfo.BaseOffsetY}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                MainViewTarget = DisplayPort;

                Header = "Base Focusing";

                ProcessingType = EnumSetupProgressState.IDLE;

                PadJogLeftUp.Caption = "Prev";
                PadJogRightUp.Caption = "Next";

                PadJogLeftUp.Command = new AsyncCommand(DoPrev);
                PadJogRightUp.Command = new AsyncCommand(DoNext);

                PadJogUp.Caption = "";
                PadJogUp.Command = new RelayCommand((Action)null);

                PadJogDown.Caption = "";
                PadJogDown.Command = new RelayCommand((Action)null);

                PadJogLeft.Caption = "";
                PadJogLeft.Command = new RelayCommand((Action)null);

                PadJogRight.Caption = "";
                PadJogRight.Command = new RelayCommand((Action)null);

                PadJogSelect.Caption = "Base";
                PadJogSelect.IsEnabled = true;
                PadJogSelect.Command = new AsyncCommand(DoToggle);

                PadJogLeftDown.IsEnabled = false;
                PadJogRightDown.IsEnabled = false;
                PadJogLeftUp.IsEnabled = true;
                PadJogRightUp.IsEnabled = true;

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
                PadJogUp.Command = ChangeBaseFocusingAreaCommand;
                PadJogUp.CommandParameter = EnumArrowDirection.UP;
                PadJogUp.RepeatEnable = true;

                PadJogDown.Caption = "▼";
                PadJogDown.Command = ChangeBaseFocusingAreaCommand;
                PadJogDown.CommandParameter = EnumArrowDirection.DOWN;
                PadJogDown.RepeatEnable = true;

                PadJogLeft.Caption = "◀";
                PadJogLeft.Command = ChangeBaseFocusingAreaCommand;
                PadJogLeft.CommandParameter = EnumArrowDirection.LEFT;
                PadJogLeft.RepeatEnable = true;

                PadJogRight.Caption = "▶";
                PadJogRight.Command = ChangeBaseFocusingAreaCommand;
                PadJogRight.CommandParameter = EnumArrowDirection.RIGHT;
                PadJogRight.RepeatEnable = true;

                PadJogUp.IsEnabled = true;
                PadJogDown.IsEnabled = true;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;

                MainViewZoomVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                OneButton.Visibility = System.Windows.Visibility.Visible;
                OneButton.Caption = "Set All";
                OneButton.Command = Button1Command;

                TwoButton.Visibility = System.Windows.Visibility.Visible;//==> Focus
                TwoButton.Caption = "Focusing";
                TwoButton.Command = Button2Command;
                TwoButton.CaptionSize = 17;

                PnpManager.PnpLightJog.HighBtnEventHandler = new RelayCommand(CameraHighButton);
                PnpManager.PnpLightJog.LowBtnEventHandler = new RelayCommand(CameraLowButton);

                if (Extensions_IParam.ProberExecuteMode == ExecuteMode.ENGINEER)
                {
                    ThreeButton.Visibility = System.Windows.Visibility.Visible;
                    FourButton.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    ThreeButton.Visibility = System.Windows.Visibility.Hidden;
                    FourButton.Visibility = System.Windows.Visibility.Hidden;
                }

                if (this.PinAligner().IsRecoveryStarted == true) //this.PinAligner().GetPAInnerStateEnum() == PinAlignInnerStateEnum.RECOVERY)
                {
                    FourButton.Caption = "Show\nResult";
                    FourButton.Visibility = System.Windows.Visibility.Visible;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void ApplyParams(List<byte[]> datas)
        {
            try
            {
                if (datas != null)
                {
                    foreach (var param in datas)
                    {
                        object target;

                        SerializeManager.DeserializeFromByte(param, out target, typeof(PinAlignDevParameters));

                        if (target != null)
                        {
                            this.PinAligner().PinAlignDevParam = (PinAlignDevParameters)target;
                        }
                    }
                }

                this.PinAligner().SaveDevParameter();
                this.PinAligner().CollectElement();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
        public new EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Initialized == false)
                {
                    LoggerManager.Debug($"[TeachPinModule] : Start InitModule");

                    LoggerManager.Debug($"[TeachPinModule] : End InitModule");

                    Initialized = true;

                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = "Base Focusing";
                retVal = InitPnpModuleStage();
                retVal = InitPnpModuleStage_AdvenceSetting();

                AdvanceSetupView = new BaseFocusingAdvanceSetupView();
                AdvanceSetupViewModel = new BaseFocusingAdvanceSetupViewModel();

                SetNodeSetupState(EnumMoudleSetupState.NONE);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                LoggerManager.Exception(err);
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

                if (PnpManager.PnpLightJog.CurSelectedMag == CameraBtnType.High)
                    TwoButton.IsEnabled = true;
                else
                    TwoButton.IsEnabled = false;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[BaseFocusingModule] Cleanup() : Cleanup Start");
            try
            {
                this.PinAligner().SaveDevParameter();
                SaveProbeCardData();
                SetNodeSetupState(EnumMoudleSetupState.NONE);
                LoggerManager.Debug($"[BaseFocusingModule] Cleanup() : BaseFocusing Setup Done");
                this.PinAligner().StopDrawDutOverlay(CurCam);
                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.BASEFOCUSINGAREA, false);

                retVal = await base.Cleanup(parameter);

                this.PnpManager.RememberLastLightSetting(CurCam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            LoggerManager.Debug($"[BaseFocusingModule] Cleanup() : Cleanup Done");
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

        public override void SetPackagableParams()
        {
            try
            {
                PackagableParams.Clear();

                PackagableParams.Add(SerializeManager.SerializeToByte(this.PinAligner().PinAlignDevParam));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        private RelayCommand<EnumArrowDirection> _ChangeSizeCommand;
        public ICommand ChangeBaseFocusingAreaCommand
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

                if (param == EnumArrowDirection.LEFT || param == EnumArrowDirection.RIGHT)
                {
                    if ((PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value.Width + offsetX) != 0 &&
                        (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value.Width + offsetX < PossibleMaxSizeX))
                    {
                        double focusingAreaX = PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value.Width + offsetX;
                        double focusingAreaY = PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value.Height;

                        double OffsetX = ((int)CurCam.GetGrabSizeWidth() / 2) - (int)(focusingAreaX / 2);
                        double OffsetY = ((int)CurCam.GetGrabSizeHeight() / 2) - (int)(focusingAreaY / 2);

                        PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value = new Rect(OffsetX, OffsetY, focusingAreaX, focusingAreaY);
                        changed = true;
                    }
                }

                if (param == EnumArrowDirection.UP || param == EnumArrowDirection.DOWN)
                {
                    if ((PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value.Height + offsetY) != 0 &&
                        (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value.Height + offsetY < PossibleMaxSizeY))
                    {
                        double focusingAreaX = PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value.Width;
                        double focusingAreaY = PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value.Height + offsetY;

                        double OffsetX = ((int)CurCam.GetGrabSizeWidth() / 2) - (int)(focusingAreaX / 2);
                        double OffsetY = ((int)CurCam.GetGrabSizeHeight() / 2) - (int)(focusingAreaY / 2);

                        PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value = new Rect(OffsetX, OffsetY, focusingAreaX, focusingAreaY);
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

        //public void RectHeightSizeUp()
        //{
        //    try
        //    {
        //        if (TargetRectangleHeight < 887)
        //        {
        //            TargetRectangleHeight = TargetRectangleHeight + 4;
        //        }

        //        PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value
        //            = new Rect(
        //                (Cam.Param.GrabSizeX.Value / 2) - ((TargetRectangleWidth * RectSizeRaitioX) / 2),
        //                (Cam.Param.GrabSizeY.Value / 2) - ((TargetRectangleHeight * RectSizeRaitioY) / 2), 
        //                TargetRectangleWidth * RectSizeRaitioX, 
        //                TargetRectangleHeight * RectSizeRaitioY
        //                );
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        //public void RectHeightSizeDown()
        //{
        //    try
        //    {
        //        if (TargetRectangleHeight > 4)
        //        {
        //            TargetRectangleHeight = TargetRectangleHeight - 4;
        //        }

        //        PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value
        //            = new Rect(
        //                (Cam.Param.GrabSizeX.Value / 2) - ((TargetRectangleWidth * RectSizeRaitioX) / 2),
        //                (Cam.Param.GrabSizeY.Value / 2) - ((TargetRectangleHeight * RectSizeRaitioY) / 2),
        //                TargetRectangleWidth * RectSizeRaitioX,
        //                TargetRectangleHeight * RectSizeRaitioY
        //                );
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        //public void RectWidthSizeUp()
        //{
        //    try
        //    {
        //        if (TargetRectangleWidth < 887)
        //        {
        //            TargetRectangleWidth = TargetRectangleWidth + 4;
        //        }

        //        PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value
        //            = new Rect(
        //                (Cam.Param.GrabSizeX.Value / 2) - ((TargetRectangleWidth * RectSizeRaitioX) / 2),
        //                (Cam.Param.GrabSizeY.Value / 2) - ((TargetRectangleHeight * RectSizeRaitioY) / 2),
        //                TargetRectangleWidth * RectSizeRaitioX,
        //                TargetRectangleHeight * RectSizeRaitioY
        //                );
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        //public void RectWidthSizeDown()
        //{
        //    try
        //    {
        //        if (TargetRectangleWidth > 4)
        //        {
        //            TargetRectangleWidth = TargetRectangleWidth - 4;
        //        }

        //        PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value
        //            = new Rect(
        //                (Cam.Param.GrabSizeX.Value / 2) - ((TargetRectangleWidth * RectSizeRaitioX) / 2),
        //                (Cam.Param.GrabSizeY.Value / 2) - ((TargetRectangleHeight * RectSizeRaitioY) / 2),
        //                TargetRectangleWidth * RectSizeRaitioX,
        //                TargetRectangleHeight * RectSizeRaitioY
        //                );
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private Task DoNext()
        {
            try
            {
                Next();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
                double posX = 0.0;
                double posY = 0.0;
                double posZ = 0.0;

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
                CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (bToogleStateCardBase == false)
                {
                    // 핀 위치로 이동
                    posX = pininfo.AbsPos.X.Value;
                    posY = pininfo.AbsPos.Y.Value;
                    posZ = pininfo.AbsPos.Z.Value;

                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }

                    //foreach (var light in pininfo.PinSearchParam.LightForTip)
                    //{
                    //    CurCam.SetLight(light.Type.Value, light.Value.Value);
                    //}
                }
                else
                {
                    // Base 위치로 이동
                    posX = pininfo.AbsPos.X.Value + pininfo.PinSearchParam.BaseParam.BaseOffsetX;
                    posY = pininfo.AbsPos.Y.Value + pininfo.PinSearchParam.BaseParam.BaseOffsetY;
                    posZ = pininfo.AbsPos.Z.Value + pininfo.PinSearchParam.BaseParam.DistanceBaseAndTip;

                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }

                    //if ((PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams != null)
                    //{
                    //    foreach (var light in (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams)
                    //    {
                    //        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    //    }
                    //}
                }

                if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                    }
                }
                else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                {
                    if (posZ == this.StageSupervisor().PinMaxRegRange)
                    {
                        this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, posZ);
                    }
                    else
                    {
                        this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                    }
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                    }
                }
            }
            catch (Exception err)
            {
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
                LoggerManager.Exception(err);
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
                double posX = 0.0;
                double posY = 0.0;
                double posZ = 0.0;

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
                CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (bToogleStateCardBase == false)
                {
                    // 핀 위치로 이동
                    posX = pininfo.AbsPos.X.Value;
                    posY = pininfo.AbsPos.Y.Value;
                    posZ = pininfo.AbsPos.Z.Value;

                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }

                    //foreach (var light in pininfo.PinSearchParam.LightForTip)
                    //{
                    //    CurCam.SetLight(light.Type.Value, light.Value.Value);
                    //}
                }
                else
                {
                    var curKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;

                    // Base 위치로 이동
                    posX = pininfo.AbsPos.X.Value + pininfo.PinSearchParam.BaseParam.BaseOffsetX;
                    posY = pininfo.AbsPos.Y.Value + pininfo.PinSearchParam.BaseParam.BaseOffsetY;
                    posZ = pininfo.AbsPos.Z.Value + pininfo.PinSearchParam.BaseParam.DistanceBaseAndTip;

                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }

                    //if ((PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams != null)
                    //{
                    //    foreach (var light in (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams)
                    //    {
                    //        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    //    }
                    //}
                }


                if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                    }
                }
                else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                {
                    if (posZ == this.StageSupervisor().PinMaxRegRange)
                    {
                        this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, posZ);
                    }
                    else
                    {
                        this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                    }
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private Task DoToggle()
        {
            try
            {
                TogglePosition();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.CompletedTask;
        }

        private void TogglePosition()
        {
            // 팁과 얼라인 마크의 위치를 서로 토글하여 위치를 전환한다.
            double posX = 0;
            double posY = 0;
            double posZ = 0;

            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (bToogleStateCardBase == true)
                {
                    PadJogSelect.Caption = "Base";
                    // 지금 얼라인 키를 보고 있다면 핀 위치로 이동한다. 
                    posX = pininfo.AbsPos.X.Value;
                    posY = pininfo.AbsPos.Y.Value;
                    posZ = pininfo.AbsPos.Z.Value;

                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }

                    this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);
                    // 핀을 보고 조명을 바꿔버릴것 같은데 이부분에서 SetLight를 하는게 맞는가..?
                    //foreach (var light in pininfo.PinSearchParam.LightForTip)
                    //{
                    //    CurCam.SetLight(light.Type.Value, light.Value.Value);
                    //}

                    bToogleStateCardBase = false;
                }
                else
                {
                    PadJogSelect.Caption = "PinTip";

                    posX = pininfo.AbsPos.X.Value + pininfo.PinSearchParam.BaseParam.BaseOffsetX;
                    posY = pininfo.AbsPos.Y.Value + pininfo.PinSearchParam.BaseParam.BaseOffsetY;
                    posZ = pininfo.AbsPos.Z.Value + pininfo.PinSearchParam.BaseParam.DistanceBaseAndTip;

                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }

                    this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);

                    if ((PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams != null)
                    {
                        foreach (var light in (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams)
                        {
                            CurCam.SetLight(light.Type.Value, light.Value.Value);
                        }
                    }

                    bToogleStateCardBase = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.PinAligner().DrawDutOverlay(CurCam);
            }
        }

        #region pnp button 1
        private AsyncCommand _Button1Command;
        public ICommand Button1Command
        {
            get
            {
                if (null == _Button1Command) _Button1Command = new AsyncCommand(
                    SetAll
                    //, EvaluationPrivilege.Evaluate(
                    //        CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                    //         new Action(() => { ShowMessages("UIModeChange"); })
                             );
                return _Button1Command;
            }
        }
        private async Task SetAll()
        {
            LoggerManager.Debug($"[BaseFocusingModule] SetAll() : Set All Start");

            try
            {
                // 카메라 사용도 high 이고, CurCam.LightsChannels 저장하니 수정
                if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                {
                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Set All", "Base to PinTip Distance and FocusParam values are set based on the current pin.", EnumMessageStyle.AffirmativeAndNegative);
                    if (result == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                        double distance = pininfo.PinSearchParam.BaseParam.DistanceBaseAndTip;
                        
                        double offsetX = pininfo.PinSearchParam.BaseParam.BaseOffsetX;
                        double offsetY = pininfo.PinSearchParam.BaseParam.BaseOffsetY;
                        
                        double baseposz = pininfo.PinSearchParam.BaseParam.BasePos.Z.Value;

                        foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                        {
                            foreach (IPinData pin in dut.PinList)
                            {
                                if (pininfo.PinNum.Value != pin.PinNum.Value)
                                {
                                    pin.PinSearchParam.BaseParam.DistanceBaseAndTip = distance;
                                    
                                    pin.PinSearchParam.BaseParam.BaseOffsetX = offsetX;
                                    pin.PinSearchParam.BaseParam.BaseOffsetY = offsetY;

                                    pin.PinSearchParam.BaseParam.BasePos.Z.Value = baseposz;
                                }
                            }
                        }
                        // CurCam.GetLight(CurCam.LightsChannels[i].Type.Value);

                        (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams.Clear();

                        for (int i = 0; i < CurCam.LightsChannels.Count; i++)
                        {
                            int val = CurCam.GetLight(CurCam.LightsChannels[i].Type.Value);

                            (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams.Add(new LightValueParam(CurCam.LightsChannels[i].Type.Value, (ushort)CurCam.GetLight(CurCam.LightsChannels[i].Type.Value)));

                            // Low 카메라 조명 저장때문...? 
                            //if (!(PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams.Any( x => x.Type.Value == CurCam.LightsChannels[i].Type.Value))
                            //{
                            //    (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams.
                            //        Add(new LightValueParam(CurCam.LightsChannels[i].Type.Value, (ushort)CurCam.GetLight(CurCam.LightsChannels[i].Type.Value)));
                            //}
                            //if ((PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams.Count > i)
                            //{
                            //    if (CurCam.LightsChannels[i].Type == (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams[i].Type) 
                            //    { 
                            //       (PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams[i].Value.Value = (ushort)val; 
                            //    }
                            //}
                        }

                        SaveProbeCardData();
                        this.PinAligner().SaveDevParameter();

                        CurCam.UpdateOverlayFlag = true;
                    }
                }
                else
                {
                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Set All", "Please use High-Magnitude to register on this page.", EnumMessageStyle.AffirmativeAndNegative);

                    //if (result == EnumMessageDialogResult.AFFIRMATIVE)
                    //{ }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "SetAll() : Error occured.");
            }
            LoggerManager.Debug($"[BaseFocusingModule] SetAll() : Set All Done");
        }
        #endregion

        public EventCodeEnum SaveProbeCardData()
        {
            EventCodeEnum serialRes = EventCodeEnum.UNDEFINED;

            LoggerManager.Debug($"[BaseFocusingModule] SaveProbeCardData() : Save Start");

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
                LoggerManager.Debug($"[BaseFocusingModule] SaveProbeCardData() : Save Done");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return serialRes;
        }

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
                    await Focusing();
                    DoingFocusing = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. BaseFocusingModule - DoFocusing() : Error occured.");
            }
        }
        private Task Focusing()
        {
            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (pininfo != null)
                {
                    FocusParam.FocusingCam.Value = CurCam.GetChannelType();

                    if (FocusParam.FocusingAxis.Value != EnumAxisConstants.PZ)
                    {
                        FocusParam.FocusingAxis.Value = EnumAxisConstants.PZ;
                    }

                    ICamera cam = this.VisionManager().GetCam(FocusParam.FocusingCam.Value);

                    // 일단, 현재 핀을 보고 있는지, Base를 보고 있는지에 따라 포커싱 할 때, 사용되는 파라미터를 달리 해보자.

                    if (bToogleStateCardBase == false)
                    {
                        double FocusingWidth = pininfo.PinSearchParam.PinSize.Value.Width + 10;
                        double FocusingHeight = pininfo.PinSearchParam.PinSize.Value.Height + 10;

                        int OffsetX = cam.Param.GrabSizeX.Value / 2 - Convert.ToInt32(Convert.ToInt32(FocusingWidth)) / 2;
                        int OffsetY = cam.Param.GrabSizeY.Value / 2 - Convert.ToInt32(Convert.ToInt32(FocusingHeight)) / 2;

                        FocusParam.FocusingROI.Value = new System.Windows.Rect(OffsetX, OffsetY, pininfo.PinSearchParam.SearchArea.Value.Width, pininfo.PinSearchParam.SearchArea.Value.Height);

                        FocusParam.FocusRange.Value = Convert.ToInt32(PinAlignParam.PinHighAlignParam.PinTipFocusRange.Value);
                        // ann todo -> OutFocusLimit 주석
                        //FocusParam.OutFocusLimit.Value = 40;
                        FocusParam.DepthOfField.Value = 1;
                        FocusParam.PeakRangeThreshold.Value = 100;

                        this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                        if (PinFocusModel.Focusing_Retry(FocusParam, false, false, false, this) != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[BaseFocusingModule] Focusing() : Focusing Fail");
                        }
                        else
                        {
                            PinCoordinate curpos = new PinCoordinate(cam.GetCurCoordPos());

                            pininfo.AbsPosOrg.Z.Value = curpos.Z.Value;
                        }
                    }
                    else
                    {
                        // ann todo -> OutFocusLimit 주석
                        //PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.OutFocusLimit.Value = 40;
                        PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.DepthOfField.Value = 1;
                        PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam.PeakRangeThreshold.Value = 100;

                        EventCodeEnum ret = PinFocusModel.Focusing_Retry(PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam, false, false, false, this);

                        if (ret != EventCodeEnum.NONE)
                        {
                            var result = this.MetroDialogManager().ShowMessageDialog("Focusing", $"Fail, Reason = {ret}", EnumMessageStyle.Affirmative);

                            LoggerManager.Debug($"[BaseFocusingModule] Focusing() : Focusing Fail");
                        }
                        else
                        {
                            int beforePinFocusingRange = Convert.ToInt32(PinAlignParam.PinFocusingRange.Value);

                            PinCoordinate curpos = new PinCoordinate(cam.GetCurCoordPos());

                            double newval_X = Math.Round(curpos.X.Value - pininfo.AbsPos.X.Value, 2);
                            double newval_Y = Math.Round(curpos.Y.Value - pininfo.AbsPos.Y.Value, 2);
                            double newval_Z = curpos.Z.Value - pininfo.AbsPos.Z.Value;

                            pininfo.PinSearchParam.BaseParam.DistanceBaseAndTip = newval_Z;

                            pininfo.PinSearchParam.BaseParam.BaseOffsetX = newval_X;
                            pininfo.PinSearchParam.BaseParam.BaseOffsetY = newval_Y;

                            pininfo.PinSearchParam.BaseParam.BasePos.Z.Value = curpos.Z.Value;
                        }

                        UpdateLabel();
                    }
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
    }
}
