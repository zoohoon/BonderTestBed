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
using static ProberInterfaces.Param.PinSearchParameter;

namespace HighAlignKeyPositionSetupModule
{
    public class HighAlignKeyPositionSetupModule : PNPSetupBase, ISetup, ITemplateModule, IParamNode, IRecovery, IHasDevParameterizable
    {
        public override bool Initialized { get; set; } = false;

        public override Guid ScreenGUID { get; } = new Guid("8BB3E2C6-F680-E713-8D89-BA1D768F66B8");

        private ICamera Cam;

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

        private PinAlignDevParameters PinAlignParam => (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);

        private bool bToogleStateAlignKey = false;  // 현재 PREV/NEXT로 이동할 때 핀 팁을 볼 것인지 얼라인 키를 볼 것인지 토글하는 역할을 한다.


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
            LoggerManager.Debug($"[HighAlignKeyPositionSetupModule] InitModule() : InitModule Stert");
            try
            {
                if (Initialized == false)
                {
                    CurrMaskingLevel = this.ProberStation().MaskingLevel;

                    LoggerManager.Debug($"[HighAlignKeyPositionSetupModule] InitModule() : InitModule Done");

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
                Header = "Position & Size Setup";

                retVal = InitPnpModuleStage();
                retVal = InitLightJog(this);

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

                InitLightJog(this);

                // Page가 바뀌면 Pin High를 가리키고 있어서 조명값도 그에 맞게 업데이트 해준다.
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                foreach (var light in pininfo.PinSearchParam.LightForTip)
                {
                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                }
                UpdateCameraLightValue();

                UpdateArrowJog();

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            LoggerManager.Debug($"[HighAlignKeyPositionSetupModule] InitSetup() : InitSetup Stert");

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

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                {
                    string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                    this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                    this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                }

                this.StageSupervisor().StageModuleState.PinHighViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);

                Cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                CurCam = Cam;

                //foreach (var light in pininfo.PinSearchParam.LightForTip)
                //{
                //    CurCam.SetLight(light.Type.Value, light.Value.Value);
                //}

                this.PnPManager().RestoreLastLightSetting(CurCam);

                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                retVal = InitPNPSetupUI();
                retVal = InitLightJog(this);

                UseUserControl = UserControlFucEnum.DEFAULT;

                MainViewTarget = DisplayPort;

                MiniViewTarget = null;

                //LoadDevParameter();

                //this.PinAligner().StopDrawDutOverlay(CurCam);
                this.PinAligner().DrawDutOverlay(CurCam);

                CurCam.UpdateOverlayFlag = true;

                UpdateLabel();


                ChangeModeCommandFunc(PinSetupMode.POSITION);
                CheckReferencePinForBlock();

                LoggerManager.Debug($"[HighAlignKeyPositionSetupModule] InitSetup() : InitSetup Done");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private void CheckReferencePinForBlock()
        {
            try
            {
                int refpinnumber = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.RefPinNum.Value;
                int refpindutnumber = 1;
                foreach (var dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (var pin in dut.PinList)
                    {
                        if (pin.PinNum.Value == refpinnumber)
                        {
                            refpindutnumber = dut.DutNumber;
                            break;
                        }
                    }
                }

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                PadJogLeftDown.IsEnabled = true;
                PadJogRightDown.IsEnabled = true;

                PadJogUp.IsEnabled = true;
                PadJogDown.IsEnabled = true;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;

                OneButton.IsEnabled = true;
                TwoButton.IsEnabled = true;


                //if ((pininfo.DutNumber.Value == refpindutnumber) && (pininfo.PinNum.Value == 1))
                //{
                //    PadJogLeftDown.IsEnabled = true;
                //    PadJogRightDown.IsEnabled = true;

                //    PadJogUp.IsEnabled = true;
                //    PadJogDown.IsEnabled = true;
                //    PadJogLeft.IsEnabled = true;
                //    PadJogRight.IsEnabled = true;

                //    OneButton.IsEnabled = true;
                //    TwoButton.IsEnabled = true;
                //}
                //else
                //{
                //    PadJogLeftDown.IsEnabled = false;
                //    PadJogRightDown.IsEnabled = false;

                //    PadJogUp.IsEnabled = false;
                //    PadJogDown.IsEnabled = false;
                //    PadJogLeft.IsEnabled = false;
                //    PadJogRight.IsEnabled = false;

                //    OneButton.IsEnabled = false;
                //    TwoButton.IsEnabled = false;
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<EnumArrowDirection> _ChangeSizeCommand;
        public ICommand ChangeSizeCommand
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
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                PinSearchParameter CurrentPinSerachParam = pininfo.PinSearchParam;
                if(bToogleStateAlignKey == true)
                {
                    double offsetX = 0;
                    double offsetY = 0;

                    if (CurrentPinSerachParam.AlignKeyHigh.Count > 0)
                    {
                        int currentalignkeyindex = CurrentPinSerachParam.AlignKeyIndex.Value;

                        if ((currentalignkeyindex >= 0) && (currentalignkeyindex <= CurrentPinSerachParam.AlignKeyHigh.Count - 1))
                        {
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

                            AlignKeyInfo keyinfo = CurrentPinSerachParam.AlignKeyHigh[currentalignkeyindex];

                            double PossibleMaxSizeX = CurCam.GetGrabSizeWidth();
                            double PossibleMaxSizeY = CurCam.GetGrabSizeHeight();

                            if ((keyinfo.BlobSizeX.Value + offsetX) != 0 && (offsetX != 0) && (keyinfo.BlobSizeX.Value + offsetX < PossibleMaxSizeX))
                            {
                                keyinfo.BlobSizeX.Value += offsetX;

                                changed = true;
                            }

                            if ((keyinfo.BlobSizeY.Value + offsetY) != 0 && (offsetY != 0) && (keyinfo.BlobSizeY.Value + offsetY < PossibleMaxSizeY))
                            {
                                keyinfo.BlobSizeY.Value += offsetY;
                                changed = true;
                            }


                            if (changed == true)
                            {
                                keyinfo.BlobSizeMinX.Value = keyinfo.BlobSizeX.Value / 2.0;
                                keyinfo.BlobSizeMinY.Value = keyinfo.BlobSizeY.Value / 2.0;

                                keyinfo.BlobSizeMaxX.Value = keyinfo.BlobSizeX.Value;
                                keyinfo.BlobSizeMaxY.Value = keyinfo.BlobSizeY.Value;

                                keyinfo.BlobSizeMin.Value = keyinfo.BlobSizeMinX.Value * keyinfo.BlobSizeMinY.Value;
                                keyinfo.BlobSizeMax.Value = keyinfo.BlobSizeMaxX.Value * keyinfo.BlobSizeMaxY.Value;

                                keyinfo.BlobRoiSizeX.Value = keyinfo.BlobSizeX.Value * 2;
                                keyinfo.BlobRoiSizeY.Value = keyinfo.BlobSizeY.Value * 2;

                                keyinfo.FocusingAreaSizeX.Value = keyinfo.BlobSizeX.Value;
                                keyinfo.FocusingAreaSizeY.Value = keyinfo.BlobSizeY.Value;

                                //SaveProbeCardData(); 
                                CurCam.UpdateOverlayFlag = true;

                                UpdateLabel();
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"Align Key Index is wrong. index = {currentalignkeyindex}");
                        }
                    }
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<EnumArrowDirection> _PositionShiftCommand;
        public ICommand PositionShiftCommand
        {
            get
            {
                if (null == _PositionShiftCommand)
                    _PositionShiftCommand = new RelayCommand<EnumArrowDirection>(PositionShiftCommandFunc);
                return _PositionShiftCommand;
            }
        }

        private void PositionShiftCommandFunc(EnumArrowDirection param)
        {
            try
            {
                double offsetX = 0;
                double offsetY = 0;

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                PinSearchParameter CurrentPinSerachParam = pininfo.PinSearchParam;

                if (CurrentPinSerachParam.AlignKeyHigh.Count > 0)
                {
                    int currentalignkeyindex = CurrentPinSerachParam.AlignKeyIndex.Value;

                    if ((currentalignkeyindex >= 0) && (currentalignkeyindex <= CurrentPinSerachParam.AlignKeyHigh.Count - 1))
                    {
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

                        CurrentPinSerachParam.AlignKeyHigh[currentalignkeyindex].AlignKeyPos.X.Value += offsetX;
                        CurrentPinSerachParam.AlignKeyHigh[currentalignkeyindex].AlignKeyPos.Y.Value += offsetY;

                        //SaveProbeCardData();
                        CurCam.UpdateOverlayFlag = true;

                        UpdateLabel();
                    }
                    else
                    {
                        LoggerManager.Error($"Align Key Index is wrong. index = {currentalignkeyindex}");
                    }
                }

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

                Header = "Position & Size Setup";

                ProcessingType = EnumSetupProgressState.SKIP;

                PadJogLeftUp.Caption = "Prev";
                PadJogRightUp.Caption = "Next";
                PadJogLeftUp.Command = new AsyncCommand(DoPrev);
                PadJogRightUp.Command = new AsyncCommand(DoNext);

                PadJogSelect.Caption = "Toggle";

                PadJogLeftDown.Caption = "Position";
                PadJogRightDown.Caption = "Size";

                PadJogLeftDown.IsEnabled = true;
                PadJogRightDown.IsEnabled = true;

                PadJogLeftDown.Command = ChangeModeCommand;
                PadJogLeftDown.CommandParameter = PinSetupMode.POSITION;

                PadJogRightDown.Command = ChangeModeCommand;
                PadJogRightDown.CommandParameter = PinSetupMode.SIZE;

                //PadJogLeftDown.Command = new AsyncCommand(DoPrevLibPack);
                //PadJogRightDown.Command = new AsyncCommand(DoNextLibPack);
                //PadJogLeftDown.Caption = "Prev. Key";
                //PadJogRightDown.Caption = "Next. Key";

                PadJogSelect.Command = new AsyncCommand(DoToggle);

                var horflip = this.VisionManager().GetDispHorFlip();
                var verflip = this.VisionManager().GetDispVerFlip();

                PadJogUp.Caption = "▲";
                PadJogUp.Command = PositionShiftCommand;
                PadJogUp.RepeatEnable = true;

                PadJogDown.Caption = "▼";
                PadJogDown.Command = PositionShiftCommand;
                PadJogDown.RepeatEnable = true;

                if (verflip == DispFlipEnum.FLIP)
                {
                    PadJogUp.CommandParameter = EnumArrowDirection.DOWN;
                    PadJogDown.CommandParameter = EnumArrowDirection.UP;
                }
                else
                {
                    PadJogUp.CommandParameter = EnumArrowDirection.UP;
                    PadJogDown.CommandParameter = EnumArrowDirection.DOWN;
                }                               
                

                

                PadJogLeft.Caption = "◀";
                PadJogLeft.Command = PositionShiftCommand;                
                PadJogLeft.RepeatEnable = true;

                PadJogRight.Caption = "▶";
                PadJogRight.Command = PositionShiftCommand;
                PadJogRight.RepeatEnable = true;

                if (horflip == DispFlipEnum.FLIP)
                {
                    PadJogLeft.CommandParameter = EnumArrowDirection.RIGHT;
                    PadJogRight.CommandParameter = EnumArrowDirection.LEFT;
                }
                else
                {
                    PadJogLeft.CommandParameter = EnumArrowDirection.LEFT;
                    PadJogRight.CommandParameter = EnumArrowDirection.RIGHT;
                }



                PadJogUp.IsEnabled = true;
                PadJogDown.IsEnabled = true;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;

                PadJogLeftUp.IsEnabled = true;
                PadJogRightUp.IsEnabled = true;

                PadJogSelect.IsEnabled = true;
                PadJogUp.IsEnabled = true;
                PadJogDown.IsEnabled = true;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;

                MainViewZoomVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                OneButton.Visibility = System.Windows.Visibility.Visible;
                TwoButton.Visibility = System.Windows.Visibility.Visible;
                ThreeButton.Visibility = System.Windows.Visibility.Hidden;
                FourButton.Visibility = System.Windows.Visibility.Hidden;
                FiveButton.Visibility = System.Windows.Visibility.Hidden;

                OneButton.Caption = "Set All";
                OneButton.Command = Button1Command;

                TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Focusing.png");
                TwoButton.IconCaption = "FOCUS";
                TwoButton.Command = Button2Command;

                UseUserControl = UserControlFucEnum.DEFAULT;

                //if (this.PinAligner().IsRecoveryStarted == true)
                //{
                //    FourButton.Caption = "Show\nResult";
                //    FourButton.Command = Button4Command;
                //    FourButton.Visibility = System.Windows.Visibility.Visible;
                //}

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

        private void LoadAlignKeyData()
        {
            for (int j = 0; j < this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count; j++)
            {
                for (int k = 0; k < this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[j].PinList.Count; k++)
                {


                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[j].PinList[k].PinSearchParam.AlignKeyHigh = new List<PinSearchParameter.AlignKeyInfo>();
                    for (int num = 0; num < this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib[0].AlignKeyLib.Count; num++)
                    {
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[j].PinList[k].PinSearchParam.AlignKeyHigh.Add(new PinSearchParameter.AlignKeyInfo());
                    }
                    for (int i = 0; i < this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib[0].AlignKeyLib.Count; i++)  ///TEST CODE TODO:REMOVE
                    {
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[j].PinList[k].PinSearchParam.AlignKeyHigh[i].AlignKeyPos.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib[0].AlignKeyLib[i].AlignKeyPosition.X.Value;
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[j].PinList[k].PinSearchParam.AlignKeyHigh[i].AlignKeyPos.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib[0].AlignKeyLib[i].AlignKeyPosition.Y.Value;
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[j].PinList[k].PinSearchParam.AlignKeyHigh[i].AlignKeyPos.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib[0].AlignKeyLib[i].AlignKeyPosition.Z.Value;
                    }
                }
            }
        }

        public void UpdateOverlay()
        {
            //this.PinAligner().StopDrawDutOverlay(CurCam);
            this.PinAligner().DrawDutOverlay(CurCam); ;
        }

        private double DegreeToRadian(double angle)
        {
            try
            {
                return Math.PI * angle / 180.0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return 0;
            }
        }

        private void GetRotCoordEx(ref PinCoordinate NewPos, PinCoordinate OriPos, PinCoordinate RefPos, double angle)
        {
            double newx = 0.0;
            double newy = 0.0;
            double th = DegreeToRadian(angle);

            try
            {
                newx = OriPos.X.Value - RefPos.X.Value;
                newy = OriPos.Y.Value - RefPos.Y.Value;

                NewPos.X.Value = newx * Math.Cos(th) - newy * Math.Sin(th) + RefPos.X.Value;
                NewPos.Y.Value = newx * Math.Sin(th) + newy * Math.Cos(th) + RefPos.Y.Value;
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
                UpdateArrowJog();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return Task.CompletedTask;
        }

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

        private AsyncCommand _Button2Command;
        public ICommand Button2Command
        {
            get
            {
                if (null == _Button2Command)
                    _Button2Command = new AsyncCommand(DoFocusing);
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
        private async Task Focusing()
        {
            try
            {
                //await this.ViewModelManager().LockNotification(this.GetHashCode(), "Wait", "Pin Focusing");

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (pininfo != null)
                {
                    FocusParam.FocusingCam.Value = CurCam.GetChannelType();

                    if (FocusParam.FocusingAxis.Value != EnumAxisConstants.PZ)
                    {
                        FocusParam.FocusingAxis.Value = EnumAxisConstants.PZ;
                    }

                    ICamera cam = this.VisionManager().GetCam(FocusParam.FocusingCam.Value);

                    // 일단, 현재 핀을 보고 있는지, 키를 보고 있는지에 따라 포커싱 할 때, 사용되는 파라미터를 달리 해보자.

                    if (bToogleStateAlignKey == false)
                    {
                        double FocusingWidth = pininfo.PinSearchParam.PinSize.Value.Width + 10;
                        double FocusingHeight = pininfo.PinSearchParam.PinSize.Value.Height + 10;

                        int OffsetX = cam.Param.GrabSizeX.Value / 2 - Convert.ToInt32(Convert.ToInt32(FocusingWidth)) / 2;
                        int OffsetY = cam.Param.GrabSizeY.Value / 2 - Convert.ToInt32(Convert.ToInt32(FocusingHeight)) / 2;

                        //int OffsetX = cam.Param.GrabSizeX.Value / 2 - Convert.ToInt32(Convert.ToInt32(pininfo.PinSearchParam.SearchArea.Value.Width)) / 2;
                        //int OffsetY = cam.Param.GrabSizeY.Value / 2 - Convert.ToInt32(Convert.ToInt32(pininfo.PinSearchParam.SearchArea.Value.Height)) / 2;

                        FocusParam.FocusingROI.Value = new System.Windows.Rect(OffsetX, OffsetY, pininfo.PinSearchParam.SearchArea.Value.Width, pininfo.PinSearchParam.SearchArea.Value.Height);
                        FocusParam.FocusRange.Value = Convert.ToInt32(PinAlignParam.PinHighAlignParam.PinTipFocusRange.Value);
                        // ann todo -> OutFocusLimit 주석
                        //FocusParam.OutFocusLimit.Value = 40;
                        FocusParam.DepthOfField.Value = 1;
                        FocusParam.PeakRangeThreshold.Value = 100;

                        this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                        if (PinFocusModel.Focusing_Retry(FocusParam, false, false, false, this) != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[TachchPinModule] Focusing() : Focusing Fail");
                        }
                        else
                        {
                            PinCoordinate curpos = new PinCoordinate(cam.GetCurCoordPos());

                            //pininfo.AbsPosOrg.X.Value = curpos.X.Value;
                            //pininfo.AbsPosOrg.Y.Value = curpos.Y.Value;
                            pininfo.AbsPosOrg.Z.Value = curpos.Z.Value;
                        }

                    }
                    else
                    {
                        double ratioX = cam.GetRatioX();
                        double ratioY = cam.GetRatioY();

                        int curKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;

                        double focusingROIX = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].FocusingAreaSizeX.Value / ratioX;
                        double focusingROIY = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].FocusingAreaSizeY.Value / ratioY;

                        double blobsizeX = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].BlobSizeX.Value / ratioX;
                        double blobsizeY = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].BlobSizeY.Value / ratioY;
                        if(blobsizeX <= 0)
                        {
                            blobsizeX = 50;
                        }
                        if (blobsizeY <= 0)
                        {
                            blobsizeY = 50;
                        }
                        double focuisngrange = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].FocusingRange.Value;

                        int OffsetX = cam.Param.GrabSizeX.Value / 2 - Convert.ToInt32(Convert.ToInt32(focusingROIX)) / 2;
                        int OffsetY = cam.Param.GrabSizeY.Value / 2 - Convert.ToInt32(Convert.ToInt32(focusingROIY)) / 2;

                        FocusParam.FocusingROI.Value = new System.Windows.Rect(OffsetX, OffsetY, focusingROIX, focusingROIY);

                        FocusParam.FocusRange.Value = Convert.ToInt32(focuisngrange);
                        // ann todo -> OutFocusLimit 주석
                        //FocusParam.OutFocusLimit.Value = 40;
                        FocusParam.DepthOfField.Value = 1;
                        FocusParam.PeakRangeThreshold.Value = 100;

                        if (PinFocusModel.Focusing_Retry(FocusParam, false, false, false, this) != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[TachchPinModule] Focusing() : Focusing Fail");
                        }
                        else
                        {
                            PinCoordinate curpos = new PinCoordinate(cam.GetCurCoordPos());

                            double oldval = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Z.Value;
                            double newval = curpos.Z.Value - pininfo.AbsPos.Z.Value;
                            EnumMessageDialogResult answer = EnumMessageDialogResult.UNDEFIND;
                            if (newval > PinAlignParam.PinHighAlignParam.PinKeyMinDistance.Value)
                            {
                                answer = await this.MetroDialogManager().ShowMessageDialog("FOCUS", $"Do you want to update the Z value of the Key?\nKey Pos - Pin Pos: (Old value = {oldval:0.0#}, New value = {newval:0.0#})", EnumMessageStyle.AffirmativeAndNegative);
                            }                           
                            else
                            {
                                answer = await this.MetroDialogManager().ShowMessageDialog("PIN SETUP INVALID", $"Need to focus on the Pin and the Key \nKey Pos - Pin Pos: (Old value = {oldval:0.0#}, New value = {newval:0.0#})", EnumMessageStyle.Affirmative);
                                answer = EnumMessageDialogResult.INVALID;
                            }
                            

                            if (answer == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Z.Value = newval;
                            }
                        }

                        UpdateLabel();
                    }
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
        private void TogglePosition()
        {
            // 팁과 얼라인 마크의 위치를 서로 토글하여 위치를 전환한다.
            double posX = 0;
            double posY = 0;
            double posZ = 0;

            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (bToogleStateAlignKey == true)
                {
                    // 지금 얼라인 키를 보고 있다면 핀 위치로 이동한다. 
                    posX = pininfo.AbsPos.X.Value;
                    posY = pininfo.AbsPos.Y.Value;
                    posZ = pininfo.AbsPos.Z.Value;

                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }

                    this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);

                    foreach (var light in pininfo.PinSearchParam.LightForTip)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }

                    bToogleStateAlignKey = false;
                }
                else
                {
                    // 지금 핀을 보고 있다면 이 핀의 첫번째 얼라인 마크 위치로 이동한다.
                    // 선택된 얼라인 마크로 이동 하도록 수정됨. 
                    var currKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;
                    posX = pininfo.AbsPos.X.Value
                           + pininfo.PinSearchParam.AlignKeyHigh[currKeyIndex].AlignKeyPos.X.Value;
                    posY = pininfo.AbsPos.Y.Value
                           + pininfo.PinSearchParam.AlignKeyHigh[currKeyIndex].AlignKeyPos.Y.Value;
                    posZ = pininfo.AbsPos.Z.Value
                           + pininfo.PinSearchParam.AlignKeyHigh[currKeyIndex].AlignKeyPos.Z.Value;

                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }

                    this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);

                    if (pininfo.PinSearchParam.AlignKeyHigh[currKeyIndex].PatternIfo.LightParams != null)
                    {
                        foreach (var light in pininfo.PinSearchParam.AlignKeyHigh[currKeyIndex].PatternIfo.LightParams)
                        {
                            CurCam.SetLight(light.Type.Value, light.Value.Value);
                        }
                    }

                    bToogleStateAlignKey = true;
                }

                UpdateCameraLightValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
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


        private RelayCommand<PinSetupMode> _ChangeModeCommand;
        public ICommand ChangeModeCommand
        {
            get
            {
                if (null == _ChangeModeCommand)
                    _ChangeModeCommand = new RelayCommand<PinSetupMode>(ChangeModeCommandFunc);
                return _ChangeModeCommand;
            }
        }

        private PinSetupMode CurrentMode;

        private void ChangeModeCommandFunc(PinSetupMode mode)
        {
            CurrentMode = mode;

            if (CurrentMode == PinSetupMode.POSITION)
            {
                PadJogLeftDown.IsEnabled = false;
                PadJogRightDown.IsEnabled = true;
            }
            else if (CurrentMode == PinSetupMode.SIZE)
            {
                PadJogLeftDown.IsEnabled = true;
                PadJogRightDown.IsEnabled = false;
            }
            else
            {
                LoggerManager.Error($"Parameter is wrong. {CurrentMode}");
            }

            UpdateArrowJog();
        }

        private void UpdateArrowJog()
        {
            // TODO : 현재 핀과 선택되어 있는 AlignKey를 획득, AlignKey의 Angle을 확인
            // Angle이 0, 180 => X축으로만 움직일 수 있음
            // Angle이 90, 270 => Y축으로만 움직일 수 있음
            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                PinSearchParameter CurrentPinSerachParam = pininfo.PinSearchParam;

                if (CurrentPinSerachParam.AlignKeyHigh.Count > 0)
                {
                    int currentalignkeyindex = CurrentPinSerachParam.AlignKeyIndex.Value;

                    if ((currentalignkeyindex >= 0) && (currentalignkeyindex <= CurrentPinSerachParam.AlignKeyHigh.Count - 1))
                    {
                        if (CurrentMode == PinSetupMode.POSITION)
                        {
                            double keyangle = CurrentPinSerachParam.AlignKeyHigh[currentalignkeyindex].AlignKeyAngle.Value;

                            if (this.VisionManager().GetDispVerFlip() == DispFlipEnum.FLIP)
                            {
                                PadJogUp.CommandParameter = EnumArrowDirection.DOWN;
                                PadJogDown.CommandParameter = EnumArrowDirection.UP;
                            }

                            if (this.VisionManager().GetDispHorFlip() == DispFlipEnum.FLIP)
                            {
                                PadJogLeft.CommandParameter = EnumArrowDirection.RIGHT;
                                PadJogRight.CommandParameter = EnumArrowDirection.LEFT;
                            }

                            if ((keyangle == 0) || (keyangle == 180))
                            {
                                PadJogUp.IsEnabled = true;
                                PadJogDown.IsEnabled = true;

                                PadJogLeft.IsEnabled = true;
                                PadJogRight.IsEnabled = true;

                                PadJogUp.Command = PositionShiftCommand;
                                PadJogDown.Command = PositionShiftCommand;
                                PadJogLeft.Command = PositionShiftCommand;
                                PadJogRight.Command = PositionShiftCommand;

                            }
                            else if ((keyangle == 90) || (keyangle == 270))
                            {
                                PadJogUp.IsEnabled = true;
                                PadJogDown.IsEnabled = true;

                                PadJogUp.Command = PositionShiftCommand;
                                PadJogDown.Command = PositionShiftCommand;
                                PadJogLeft.Command = PositionShiftCommand;
                                PadJogRight.Command = PositionShiftCommand;

                                PadJogLeft.IsEnabled = true;
                                PadJogRight.IsEnabled = true;
                            }
                            else
                            {
                                LoggerManager.Error($"Key's angle is wrong. Angle = {keyangle}");
                            }
                        }
                        else
                        {
                            PadJogUp.IsEnabled = true;
                            PadJogDown.IsEnabled = true;
                            PadJogLeft.IsEnabled = true;
                            PadJogRight.IsEnabled = true;

                            if(this.VisionManager().GetDispVerFlip() == DispFlipEnum.FLIP)
                            {
                                PadJogUp.CommandParameter = EnumArrowDirection.UP;
                                PadJogDown.CommandParameter = EnumArrowDirection.DOWN;                                
                            }

                            if (this.VisionManager().GetDispHorFlip() == DispFlipEnum.FLIP)
                            {
                                PadJogLeft.CommandParameter = EnumArrowDirection.LEFT;
                                PadJogRight.CommandParameter = EnumArrowDirection.RIGHT;
                            }


                            PadJogUp.Command = ChangeSizeCommand;
                            PadJogDown.Command = ChangeSizeCommand;
                            PadJogLeft.Command = ChangeSizeCommand;
                            PadJogRight.Command = ChangeSizeCommand;
                        }

                    }
                    else
                    {
                        PadJogUp.IsEnabled = false;
                        PadJogDown.IsEnabled = false;

                        PadJogLeft.IsEnabled = false;
                        PadJogRight.IsEnabled = false;
                    }
                }
                else
                {
                    PadJogUp.IsEnabled = false;
                    PadJogDown.IsEnabled = false;

                    PadJogLeft.IsEnabled = false;
                    PadJogRight.IsEnabled = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private Task DoNext()
        {
            try
            {
                
                Next();
                UpdateArrowJog();
                CheckReferencePinForBlock();
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

                // CurPinArrayIndex는 현재 선택된 실제 핀 번호이지만, 데이터 상으로는 더트별로 해당 핀이 들어가 있는 배열의 인덱스는 핀 번호하고는 다르다.
                // 따라서 배열상의 어레이로 변환해 주어야 한다.
                CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                var curKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;

                if (bToogleStateAlignKey == false)
                {
                    // 핀 위치로 이동
                    posX = pininfo.AbsPos.X.Value;
                    posY = pininfo.AbsPos.Y.Value;
                    posZ = pininfo.AbsPos.Z.Value;

                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }

                    foreach (var light in pininfo.PinSearchParam.LightForTip)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }
                else
                {
                    // 얼라인 키 위치로 이동
                    posX = pininfo.AbsPos.X.Value
                           + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.X.Value;
                    posY = pininfo.AbsPos.Y.Value
                           + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Y.Value;
                    posZ = pininfo.AbsPos.Z.Value
                           + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Z.Value;

                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }

                    if (pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].PatternIfo.LightParams != null)
                    {
                        foreach (var light in pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].PatternIfo.LightParams)
                        {
                            CurCam.SetLight(light.Type.Value, light.Value.Value);
                        }
                    }
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
                UpdateLabel();
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
                UpdateArrowJog();
                CheckReferencePinForBlock();
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

                // CurPinArrayIndex는 현재 선택된 실제 핀 번호이지만, 데이터 상으로는 더트별로 해당 핀이 들어가 있는 배열의 인덱스는 핀 번호하고는 다르다.
                // 따라서 배열상의 어레이로 변환해 주어야 한다.
                CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (bToogleStateAlignKey == false)
                {
                    // 핀 위치로 이동
                    posX = pininfo.AbsPos.X.Value;
                    posY = pininfo.AbsPos.Y.Value;
                    posZ = pininfo.AbsPos.Z.Value;

                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }

                    foreach (var light in pininfo.PinSearchParam.LightForTip)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }
                else
                {
                    var curKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;

                    // 얼라인 키 위치로 이동
                    posX = pininfo.AbsPos.X.Value
                           + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.X.Value;
                    posY = pininfo.AbsPos.Y.Value
                           + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Y.Value;
                    posZ = pininfo.AbsPos.Z.Value
                           + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Z.Value;

                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }

                    if (pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].PatternIfo.LightParams != null)
                    {
                        foreach (var light in pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].PatternIfo.LightParams)
                        {
                            CurCam.SetLight(light.Type.Value, light.Value.Value);
                        }
                    }
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
                UpdateLabel();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        #region pnp button 1
        private AsyncCommand _Button1Command;
        public ICommand Button1Command
        {
            get
            {
                if (null == _Button1Command)
                    _Button1Command = new AsyncCommand(DoConfirmToSetAll);

                return _Button1Command;
            }
        }

        private async Task DoConfirmToSetAll()
        {
            try
            {
                

                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Confirm To Change All",
                    "Align mark for all of pins will be updated at once, \nDo you want to proceed?", EnumMessageStyle.AffirmativeAndNegative);
                if (result == EnumMessageDialogResult.AFFIRMATIVE)
                {

                    Task task = new Task(() =>
                    {
                        SetAll();
                    });
                    task.Start();
                    await task;
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

        public void SetAll()
        {
            LoggerManager.Debug($"[HighAlignKeyPositionSetupModule] SetAll() : Set All Alignmark Start");
            double curAngle = 0;

            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                var curKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;
                curAngle = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyAngle.Value;
                double xpos = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.X.Value;
                double ypos = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Y.Value;
                double zpos = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Z.Value;

                // 현재 핀의 Key 인덱스와 동일한 인덱스들에 해당하는 키들의 데이터를 동일하게 변경한다.
                // 어떤 데이터를 동일하게 바꿀 것인가?
                // Position과 Size를 바꾼다.
                // Angle에 주의하여 값을 업데이트 할 것.

                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (IPinData pin in dut.PinList)
                    {
                        if (pininfo.PinNum.Value != pin.PinNum.Value)
                        {
                            // 동일한 인덱스의 키를 갖고 있는 경우
                            if (pin.PinSearchParam.AlignKeyHigh.Count > 0)
                            {
                                if (curKeyIndex >= 0 && curKeyIndex <= pin.PinSearchParam.AlignKeyHigh.Count - 1)
                                {
                                    // angle 이 같은 경우
                                    if (pin.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyAngle.Value == curAngle)
                                    {
                                        pin.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.X.Value = xpos;
                                        pin.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Y.Value = ypos;
                                        pin.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Z.Value = zpos;
                                    }
                                    else
                                    {
                                        // 일반적으로 angle은 다를 수 있지만, 상대거리의 경우 같은 경우가 많다는 가정하에 처리되는 로직

                                        // angle의 차이가 90도 나는 경우와 180도 나는 경우를 나눠서 처리한다.
                                        // angle이 180도 차이가 난다는 것은 x 또는 y의 값 중 하나의 값만 부호를 변경하면 되는 경우이며
                                        // angle이 90도 차이가 난다는 것은 x와 y의 값을 변경한 뒤, 부호를 처리해야 되는 경우이다.
                                        // Z값은 관계없이 업데이트

                                        double currentangle = pin.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyAngle.Value;
                                        double DiffAngle = Math.Abs(curAngle - currentangle);

                                        double meaningvalue = 0;

                                        pin.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Z.Value = zpos;

                                        if (Math.Abs(xpos) > 0)
                                        {
                                            meaningvalue = xpos;
                                        }

                                        if (Math.Abs(ypos) > 0)
                                        {
                                            meaningvalue = ypos;
                                        }

                                        if (DiffAngle == 90)
                                        {
                                            if (currentangle == 0)
                                            {
                                                // Y = 0
                                                // Math.Abs(X) > 0 

                                                meaningvalue = Math.Abs(meaningvalue);

                                                pin.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.X.Value = -meaningvalue;
                                            }
                                            else if (currentangle == 90)
                                            {
                                                // X = 0
                                                // Math.Abs(Y) > 0 

                                                meaningvalue = Math.Abs(meaningvalue);

                                                pin.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Y.Value = meaningvalue;
                                            }
                                            else if (currentangle == 180)
                                            {
                                                // Y = 0
                                                // Math.Abs(X) > 0 

                                                meaningvalue = Math.Abs(meaningvalue) * -1;

                                                pin.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.X.Value = meaningvalue;
                                            }
                                            else if (currentangle == 270)
                                            {
                                                // X = 0
                                                // Math.Abs(Y) > 0 

                                                meaningvalue = Math.Abs(meaningvalue) * -1;

                                                pin.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Y.Value = meaningvalue;
                                            }
                                        }
                                        else if (DiffAngle == 180)
                                        {
                                            // 현재 Angle이 0도 180도 인경우, X의 부호만 반대로
                                            // 현재 Angle이 90도 270인경우, Y의 부호만 반대로

                                            if ((currentangle == 0) || (currentangle == 180))
                                            {
                                                pin.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.X.Value = meaningvalue * -1;
                                            }
                                            else if ((currentangle == 90) || (currentangle == 270))
                                            {
                                                pin.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Y.Value = meaningvalue * -1;
                                            }
                                            else
                                            {
                                                LoggerManager.Error($"Current angle is wrong. Angle = {currentangle}");
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Error($"Diffrence angle is wrong. Angle = {DiffAngle}");
                                        }
                                    }

                                    pin.PinSearchParam.AlignKeyHigh[curKeyIndex].BlobSizeX.Value = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].BlobSizeX.Value;
                                    pin.PinSearchParam.AlignKeyHigh[curKeyIndex].BlobSizeY.Value = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].BlobSizeY.Value;

                                    pin.PinSearchParam.AlignKeyHigh[curKeyIndex].BlobSizeMin.Value = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].BlobSizeMin.Value;
                                    pin.PinSearchParam.AlignKeyHigh[curKeyIndex].BlobSizeMax.Value = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].BlobSizeMax.Value;

                                    pin.PinSearchParam.AlignKeyHigh[curKeyIndex].BlobRoiSizeX.Value = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].BlobRoiSizeX.Value;
                                    pin.PinSearchParam.AlignKeyHigh[curKeyIndex].BlobRoiSizeY.Value = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].BlobRoiSizeY.Value;

                                }

                            }
                        }
                    }
                }

                SaveProbeCardData();

                CurCam.UpdateOverlayFlag = true;
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "SetAll() : Error occured.");
            }
            LoggerManager.Debug($"[HighAlignKeyPositionSetupModule] SetAll() : Set All Search Area Done");
        }

        #endregion

        //#region pnp button 5
        //private RelayCommand _Button4Command;
        //public ICommand Button4Command
        //{
        //    get
        //    {
        //        if (null == _Button4Command)
        //            _Button4Command = new RelayCommand(DoShowResult);
        //        return _Button4Command;
        //    }
        //}

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
                    //t1.Wait();
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

            LoggerManager.Debug($"[HighAlignKeyPositionSetupModule] SaveProbeCardData() : Save Start");
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
                LoggerManager.Debug($"[HighAlignKeyPositionSetupModule] SaveProbeCardData() : Save Done");
            }
            catch (Exception err)
            {
                throw err;
            }
            return serialRes;
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
                SaveProbeCardData();
                SetNodeSetupState(EnumMoudleSetupState.NONE);
                this.PinAligner().StopDrawDutOverlay(CurCam);
                retVal = await base.Cleanup(parameter);

                if (retVal == EventCodeEnum.NONE)
                {
                    this.StageSupervisor().StageModuleState.ZCLEARED();
                }

                this.PnPManager().RememberLastLightSetting(CurCam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public Task<EventCodeEnum> InitRecovery()
        {
            throw new NotImplementedException();
        }

        public override void UpdateLabel()
        {
            IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

            //// 핀을 보고 있을 때 
            //if (bToogleStateAlignKey == true)
            //{

            //}
            //else // 키를 보고 있을 때 
            //{

            //}

            if (pininfo.PinSearchParam.AlignKeyHigh.Count > 0)
            {

                int index = pininfo.PinSearchParam.AlignKeyIndex.Value;
                AlignKeyInfo keyinfo = pininfo.PinSearchParam.AlignKeyHigh[index];

                double angle = keyinfo.AlignKeyAngle.Value;
                //string angleStr = string.Empty;

                double relativeDist = 0.0;
                double relativeDisZ = keyinfo.AlignKeyPos.Z.Value;

                double KeySizeWidth = keyinfo.BlobSizeX.Value;
                double KeySizeHeight = keyinfo.BlobSizeY.Value;

                // TODO: 상대거리를 알려주자.

                if (angle == 0 || angle == 180)
                {
                    relativeDist = keyinfo.AlignKeyPos.X.Value;
                }
                else if (angle == 90 || angle == 270)
                {
                    relativeDist = keyinfo.AlignKeyPos.Y.Value;
                }
                else
                {
                    LoggerManager.Debug($"Angle is wrong.");
                }

                // TODO : Check Format
                StepLabel = $"Registered key, Distance({relativeDist:0.0#}um, Key ZPos - Pin ZPos:{relativeDisZ}um) \nSize : (X = {KeySizeWidth}um, Y = {KeySizeHeight}um)";
            }
            else
            {
                StepLabel = $"Not registered key";
            }
        }
    }
}
