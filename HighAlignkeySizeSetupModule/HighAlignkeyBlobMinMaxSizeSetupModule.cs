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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using static ProberInterfaces.Param.PinSearchParameter;

namespace HighAlignKeyBrightnessSetupModule
{
    public class HighAlignkeyBlobMinMaxSizeSetupModule : PNPSetupBase, ISetup, ITemplateModule, IParamNode, IRecovery, IHasDevParameterizable
    {
        public override bool Initialized { get; set; } = false;

        public override Guid ScreenGUID { get; } = new Guid("F7B25A63-15B0-2D41-7A2F-51300EA9532F");

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
            LoggerManager.Debug($"[HighAlignKeyBrightnessSetupModule] InitModule() : InitModule Stert");
            try
            {
                if (Initialized == false)
                {
                    CurrMaskingLevel = this.ProberStation().MaskingLevel;

                    LoggerManager.Debug($"[HighAlignKeyBrightnessSetupModule] InitModule() : InitModule Done");

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

        public override void SetPackagableParams()
        {
            // 현재 선택되어 있는 Pin 정보를 넘기면 되는가???

            //PackagableParams.Add(SerializeManager.SerializeToByte(this.PinAligner().PinAlignDevParam));

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = "Key Min / Max Setup";

                retVal = InitPnpModuleStage_AdvenceSetting();
                retVal = InitLightJog(this);

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

                // Page가 바뀌면 Pin High를 가리키고 있어서 조명값도 그에 맞게 업데이트 해준다.
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                foreach (var light in pininfo.PinSearchParam.LightForTip)
                {
                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                }
                UpdateCameraLightValue();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                //LoggerManager.Debug(err);
                throw err;
            }
            return retVal;
        }

        private PinSetupMode CurrentMode;

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

        private void ChangeModeCommandFunc(PinSetupMode mode)
        {
            CurrentMode = mode;

            if (CurrentMode == PinSetupMode.KEYBLOBMIN)
            {
                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.KEYBLOBMIN, true);
                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.KEYBLOBMAX, false);

                PadJogLeftDown.IsEnabled = false;
                PadJogRightDown.IsEnabled = true;
            }
            else if (CurrentMode == PinSetupMode.KEYBLOBMAX)
            {
                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.KEYBLOBMIN, false);
                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.KEYBLOBMAX, true);

                PadJogLeftDown.IsEnabled = true;
                PadJogRightDown.IsEnabled = false;
            }
            else
            {
                LoggerManager.Debug($"Mode is wrong");
            }

            CurCam.UpdateOverlayFlag = true;

            UpdateLabel();
        }

        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //InitBackupData();

            LoggerManager.Debug($"[HighAlignkeyBlobMinMaxSizeSetupModule] InitSetup() : InitSetup Stert");

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

                bToogleStateAlignKey = false;  // 초기 이동은 핀 위치로 간다.


                Cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                CurCam = Cam;

                //foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
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

                retVal = LoadDevParameter();

                this.PinAligner().StopDrawDutOverlay(CurCam);
                this.PinAligner().DrawDutOverlay(CurCam);

                CurCam.UpdateOverlayFlag = true;

                UpdateLabel();

                ChangeModeCommandFunc(PinSetupMode.KEYBLOBMIN);

                LoggerManager.Debug($"[HighAlignkeyBlobMinMaxSizeSetupModule] InitSetup() : InitSetup Done");
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Error($err + "InitSetup() : Error occured.");
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private RelayCommand<EnumArrowDirection> _ChangeSizeCommand;
        public ICommand ChangeBlobSizeCommand
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
                double PossibleMaxSizeX = CurCam.GetGrabSizeWidth();
                double PossibleMaxSizeY = CurCam.GetGrabSizeHeight();

                if (bToogleStateAlignKey == false)
                {
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

                    if (CurrentMode == PinSetupMode.KEYBLOBMIN)
                    {
                        if ((pininfo.PinSearchParam.MinBlobSizeX.Value + offsetX) != 0 && (offsetX != 0) && (pininfo.PinSearchParam.MinBlobSizeX.Value + offsetX < PossibleMaxSizeX))
                        {
                            pininfo.PinSearchParam.MinBlobSizeX.Value += (int)offsetX;

                            changed = true;
                        }

                        if ((pininfo.PinSearchParam.MinBlobSizeY.Value + offsetY) != 0 && (offsetY != 0) && (pininfo.PinSearchParam.MinBlobSizeY.Value + offsetY < PossibleMaxSizeY))
                        {
                            pininfo.PinSearchParam.MinBlobSizeY.Value += (int)offsetY;
                            changed = true;
                        }

                        if (changed == true)
                        {
                            //pininfo.BlobSizeMin.Value = keyinfo.BlobSizeMinX.Value * keyinfo.BlobSizeMinY.Value;
                        }
                    }
                    else if (CurrentMode == PinSetupMode.KEYBLOBMAX)
                    {
                        if ((pininfo.PinSearchParam.MaxBlobSizeX.Value + offsetX) != 0 && (offsetX != 0) && (pininfo.PinSearchParam.MaxBlobSizeX.Value + offsetX < PossibleMaxSizeX))
                        {
                            pininfo.PinSearchParam.MaxBlobSizeX.Value += (int)offsetX;

                            changed = true;
                        }

                        if ((pininfo.PinSearchParam.MaxBlobSizeY.Value + offsetY) != 0 && (offsetY != 0) && (pininfo.PinSearchParam.MaxBlobSizeY.Value + offsetY < PossibleMaxSizeY))
                        {
                            pininfo.PinSearchParam.MaxBlobSizeY.Value += (int)offsetY;
                            changed = true;
                        }

                        if (changed == true)
                        {
                            //keyinfo.BlobSizeMax.Value = keyinfo.BlobSizeMaxX.Value * keyinfo.BlobSizeMaxY.Value;
                        }


                    }
                    else
                    {
                        LoggerManager.Error($"Curret Mode is wrong");
                    }

                    if (changed == true)
                    {
                        //SaveProbeCardData();
                        CurCam.UpdateOverlayFlag = true;

                        UpdateLabel();
                    }



                    bToogleStateAlignKey = false;
                }
                else
                {
                    
                    PinSearchParameter CurrentPinSerachParam = pininfo.PinSearchParam;
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


                            if (CurrentMode == PinSetupMode.KEYBLOBMIN)
                            {
                                if ((keyinfo.BlobSizeMinX.Value + offsetX) != 0 && (offsetX != 0) && (keyinfo.BlobSizeMinX.Value + offsetX < PossibleMaxSizeX))
                                {
                                    keyinfo.BlobSizeMinX.Value += offsetX;

                                    changed = true;
                                }

                                if ((keyinfo.BlobSizeMinY.Value + offsetY) != 0 && (offsetY != 0) && (keyinfo.BlobSizeMinY.Value + offsetY < PossibleMaxSizeY))
                                {
                                    keyinfo.BlobSizeMinY.Value += offsetY;
                                    changed = true;
                                }

                                if (changed == true)
                                {
                                    keyinfo.BlobSizeMin.Value = keyinfo.BlobSizeMinX.Value * keyinfo.BlobSizeMinY.Value;
                                }
                            }
                            else if (CurrentMode == PinSetupMode.KEYBLOBMAX)
                            {
                                if ((keyinfo.BlobSizeMaxX.Value + offsetX) != 0 && (offsetX != 0) && (keyinfo.BlobSizeMaxX.Value + offsetX < PossibleMaxSizeX))
                                {
                                    keyinfo.BlobSizeMaxX.Value += offsetX;

                                    changed = true;
                                }

                                if ((keyinfo.BlobSizeMaxY.Value + offsetY) != 0 && (offsetY != 0) && (keyinfo.BlobSizeMaxY.Value + offsetY < PossibleMaxSizeY))
                                {
                                    keyinfo.BlobSizeMaxY.Value += offsetY;
                                    changed = true;
                                }

                                if (changed == true)
                                {
                                    keyinfo.BlobSizeMax.Value = keyinfo.BlobSizeMaxX.Value * keyinfo.BlobSizeMaxY.Value;
                                }


                            }
                            else
                            {
                                LoggerManager.Error($"Curret Mode is wrong");
                            }

                            if (changed == true)
                            {
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
                    bToogleStateAlignKey = true;
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

                Header = "Key Min / Max Setup";

                ProcessingType = EnumSetupProgressState.IDLE;

                PadJogLeftUp.Caption = "Prev";
                PadJogRightUp.Caption = "Next";
                PadJogLeftUp.Command = new AsyncCommand(DoPrev);
                PadJogRightUp.Command = new AsyncCommand(DoNext);

                PadJogSelect.Caption = "Toggle";

                PadJogLeftDown.Caption = "MIN";
                PadJogLeftDown.Command = ChangeModeCommand;
                PadJogLeftDown.CommandParameter = PinSetupMode.KEYBLOBMIN;

                PadJogRightDown.Caption = "MAX";
                PadJogRightDown.Command = ChangeModeCommand;
                PadJogRightDown.CommandParameter = PinSetupMode.KEYBLOBMAX;

                PadJogSelect.Command = new AsyncCommand(DoToggle);

                PadJogUp.Caption = "▲";
                PadJogUp.Command = ChangeBlobSizeCommand;
                PadJogUp.CommandParameter = EnumArrowDirection.UP;
                PadJogUp.RepeatEnable = true;

                PadJogDown.Caption = "▼";
                PadJogDown.Command = ChangeBlobSizeCommand;
                PadJogDown.CommandParameter = EnumArrowDirection.DOWN;
                PadJogDown.RepeatEnable = true;

                PadJogLeft.Caption = "◀";
                PadJogLeft.Command = ChangeBlobSizeCommand;
                PadJogLeft.CommandParameter = EnumArrowDirection.LEFT;
                PadJogLeft.RepeatEnable = true;

                PadJogRight.Caption = "▶";
                PadJogRight.Command = ChangeBlobSizeCommand;
                PadJogRight.CommandParameter = EnumArrowDirection.RIGHT;
                PadJogRight.RepeatEnable = true;

                PadJogUp.IsEnabled = true;
                PadJogDown.IsEnabled = true;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;

                PadJogLeftUp.IsEnabled = true;
                PadJogRightUp.IsEnabled = true;
                PadJogSelect.IsEnabled = true;

                MainViewZoomVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                OneButton.Visibility = System.Windows.Visibility.Visible;
                TwoButton.Visibility = System.Windows.Visibility.Visible;
                ThreeButton.Visibility = System.Windows.Visibility.Hidden;
                FourButton.Visibility = System.Windows.Visibility.Hidden;
                FiveButton.Visibility = System.Windows.Visibility.Hidden;

                OneButton.Caption = "Set All";
                OneButton.Command = Button1Command;

                TwoButton.Caption = "Set";
                TwoButton.Command = Button2Command;

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


        private async Task DoToggle()
        {
            try
            {
                

                Task task = new Task(() =>
                {
                    TogglePosition();
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
                if (bToogleStateAlignKey == true)
                {
                    // 지금 얼라인 키를 보고 있다면 핀 위치로 이동한다. 
                    posX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value;
                    posY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value;
                    posZ = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value;
                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }
                    this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);

                    foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }

                    bToogleStateAlignKey = false;
                }
                else
                {
                    // 지금 핀을 보고 있다면 이 핀의 첫번째 얼라인 마크 위치로 이동한다.
                    posX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value
                           + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[0].AlignKeyPos.X.Value;
                    posY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value
                           + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[0].AlignKeyPos.Y.Value;
                    posZ = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value
                           + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[0].AlignKeyPos.Z.Value;
                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }
                    this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);

                    if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[0].PatternIfo.LightParams != null)
                    {
                        foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[0].PatternIfo.LightParams)
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
            return EventCodeEnum.NONE;
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

        private async Task DoNext()
        {
            try
            {
                
                await Next();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                
            }
        }
        public async Task Next()
        {
            try
            {
                int TotalPinNum = 0;
                IPinData tmpPinData;
                double posX = 0.0;
                double posY = 0.0;
                double posZ = 0.0;

                Task task = new Task(() =>
                {

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

                    if (bToogleStateAlignKey == false)
                    {
                        // 핀 위치로 이동
                        posX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value;
                        posY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value;
                        posZ = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value;

                        if (posZ > this.StageSupervisor().PinMaxRegRange)
                        {
                            posZ = this.StageSupervisor().PinMaxRegRange;
                        }

                        foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
                        {
                            CurCam.SetLight(light.Type.Value, light.Value.Value);
                        }
                    }
                    else
                    {
                        // 얼라인 키 위치로 이동
                        posX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value
                               + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[0].AlignKeyPos.X.Value;
                        posY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value
                               + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[0].AlignKeyPos.Y.Value;
                        posZ = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value
                               + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[0].AlignKeyPos.Z.Value;
                    
                        if (posZ > this.StageSupervisor().PinMaxRegRange)
                        {
                            posZ = this.StageSupervisor().PinMaxRegRange;
                        }

                        if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[0].PatternIfo.LightParams != null)
                        {
                            foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[0].PatternIfo.LightParams)
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
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                        }
                    }
                    else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                    {
                        if (posZ == this.StageSupervisor().PinMaxRegRange)
                        {
                            this.StageSupervisor().StageModuleState.PinLowViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, posZ);
                        }
                        else
                        {
                            this.StageSupervisor().StageModuleState.PinLowViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                        }
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                        }
                    }
                    UpdateLabel();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //this.StageSupervisor().StageModuleState.PinHighViewMove(ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
        }

        private async Task DoPrev()
        {
            try
            {
                
                await Prev();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                
            }
        }
        public async Task Prev()
        {
            try
            {
                int TotalPinNum = 0;
                IPinData tmpPinData;
                double posX = 0.0;
                double posY = 0.0;
                double posZ = 0.0;

                Task task = new Task(() =>
                {

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

                    if (bToogleStateAlignKey == false)
                    {
                        // 핀 위치로 이동
                        posX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value;
                        posY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value;
                        posZ = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value;

                        if (posZ > this.StageSupervisor().PinMaxRegRange)
                        {
                            posZ = this.StageSupervisor().PinMaxRegRange;
                        }

                        foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
                        {
                            CurCam.SetLight(light.Type.Value, light.Value.Value);
                        }
                    }
                    else
                    {
                        // 얼라인 키 위치로 이동
                        posX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value
                               + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[0].AlignKeyPos.X.Value;
                        posY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value
                               + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[0].AlignKeyPos.Y.Value;
                        posZ = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value
                               + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[0].AlignKeyPos.Z.Value;

                        if (posZ > this.StageSupervisor().PinMaxRegRange)
                        {
                            posZ = this.StageSupervisor().PinMaxRegRange;
                        }

                        ObservableCollection<LightValueParam> lighrparams = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[0].PatternIfo.LightParams;

                            if (lighrparams != null)
                            {
                                foreach (var light in lighrparams)
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
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                        }
                    }
                    else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                    {
                        if (posZ == this.StageSupervisor().PinMaxRegRange)
                        {
                            this.StageSupervisor().StageModuleState.PinLowViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, posZ);
                        }
                        else
                        {
                            this.StageSupervisor().StageModuleState.PinLowViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                        }
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                        }
                    }
                    UpdateLabel();
                });
                task.Start();
                await task;

            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "Prev() : Error occured.");
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
                Task taskSetAll = null;
                if (bToogleStateAlignKey == false)
                {
                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Confirm To Change All",
                    "Brightness for all of pins will be updated at once, \nDo you want to proceed?", EnumMessageStyle.AffirmativeAndNegative);

                    if (result == EnumMessageDialogResult.AFFIRMATIVE)
                    {

                        taskSetAll = new Task(() =>
                        {
                            SetAll();
                        });
                        taskSetAll.Start();
                        await taskSetAll;
                    }
                }
                else
                {
                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Confirm To Change All",
                    "Brightenss for all of align marks will be updated at once, \nDo you want to proceed?", EnumMessageStyle.AffirmativeAndNegative);
                    if (result == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        taskSetAll = new Task(() =>
                        {
                            SetAll();
                        });
                        taskSetAll.Start();
                        await taskSetAll;
                    }
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
            LoggerManager.Debug($"[HighAlignkeyBlobMinMaxSizeSetupModule] SetAll() : Set All Alignmark Brightness Start");

            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                var curKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;

                AlignKeyInfo keyinfo = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex];

                double xpos = keyinfo.AlignKeyPos.X.Value;
                double ypos = keyinfo.AlignKeyPos.Y.Value;
                double zpos = keyinfo.AlignKeyPos.Z.Value;

                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (IPinData pin in dut.PinList)
                    {
                        if(bToogleStateAlignKey == false)
                        {
                            pin.PinSearchParam.MinBlobSizeX = pininfo.PinSearchParam.MinBlobSizeX;
                            pin.PinSearchParam.MinBlobSizeY = pininfo.PinSearchParam.MinBlobSizeY;
                            pin.PinSearchParam.MaxBlobSizeX = pininfo.PinSearchParam.MaxBlobSizeX;
                            pin.PinSearchParam.MaxBlobSizeY = pininfo.PinSearchParam.MaxBlobSizeY;
                        }
                        else
                        {
                            foreach (AlignKeyInfo curMark in pin.PinSearchParam.AlignKeyHigh)
                            {
                                //curMark

                                curMark.BlobSizeMinX.Value = keyinfo.BlobSizeMinX.Value;
                                curMark.BlobSizeMinY.Value = keyinfo.BlobSizeMinY.Value;
                                curMark.BlobSizeMin.Value = keyinfo.BlobSizeMin.Value;

                                curMark.BlobSizeMaxX.Value = keyinfo.BlobSizeMaxX.Value;
                                curMark.BlobSizeMaxY.Value = keyinfo.BlobSizeMaxY.Value;
                                curMark.BlobSizeMax.Value = keyinfo.BlobSizeMax.Value;

                            }
                        }
                        
                    }
                }

                SaveProbeCardData();

                CurCam.UpdateOverlayFlag = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            LoggerManager.Debug($"[HighAlignkeyBlobMinMaxSizeSetupModule] SetAll() : Set All Search Area Done");
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
                    DoSet);
                return _Button2Command;
            }
        }

        private async Task DoSet()
        {
            try
            {
                await Set();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
            }
        }
        private async Task Set()
        {
            try
            {
                Task task = new Task(() =>
                {
                    if (bToogleStateAlignKey == false)
                    {
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Clear();
                        foreach (var light in CurCam.LightsChannels)
                        {
                            int val = CurCam.GetLight(light.Type.Value);
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AddLight(new LightValueParam(light.Type.Value, (ushort)val));
                        }
                    }
                    else
                    {
                        foreach (AlignKeyInfo curMark in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh)
                        {
                            if (curMark.PatternIfo.LightParams == null)
                            {
                                curMark.PatternIfo.LightParams = new ObservableCollection<LightValueParam>();
                            }

                            curMark.PatternIfo.LightParams.Clear();

                            foreach (var light in CurCam.LightsChannels)
                            {
                                int val = CurCam.GetLight(light.Type.Value);
                                curMark.PatternIfo.LightParams.Add(new LightValueParam(light.Type.Value, (ushort)val));
                            }
                        }
                    }
                
                SaveProbeCardData();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        #endregion

        //#region pnp button 5
        //private RelayCommand _Button5Command;
        //public ICommand Button5Command
        //{
        //    get
        //    {
        //        if (null == _Button5Command)
        //            _Button5Command = new RelayCommand(
        //            DoShowResult);
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

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //}

        //#endregion

        public override async Task Save()
        {
            try
            {
                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Vision Setup Save",
                    "Do you want to save the contents of the Vision Setup ?", EnumMessageStyle.AffirmativeAndNegative);

                if (result == EnumMessageDialogResult.AFFIRMATIVE)
                {

                    Task task = new Task(() =>
                    {

                        foreach (var module in this.PinAligner().Template)
                        {
                            if (module.GetType().GetInterfaces().Contains(typeof(IHasDevParameterizable)))
                            {
                                ((IHasDevParameterizable)module).SaveDevParameter();
                            }
                        }

                        this.SaveParameter(((IParam)this.StageSupervisor().WaferObject));

                        
                    });
                    task.Start();
                    await task;

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
                    await this.ViewModelManager().BackPreScreenTransition(); ;
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

            LoggerManager.Debug($"[HighAlignKeyBrightnessSetupModule] SaveProbeCardData() : Save Start");
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
                LoggerManager.Debug($"[HighAlignKeyBrightnessSetupModule] SaveProbeCardData() : Save Done");
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

                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.KEYBLOBMIN, false);
                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.KEYBLOBMAX, false);

                retVal = await base.Cleanup(parameter);
                if (retVal == EventCodeEnum.NONE)
                {
                    this.StageSupervisor().StageModuleState.ZCLEARED();
                }

                this.PnPManager().RememberLastLightSetting(CurCam);

                //Key나 Tip의 Min/ Max Size를 설정하고 나올 때 Advanced Setup Page에서 사용되는 Validation Size의 값도 확인후 바꿔줘야 한다. 
                if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef != null && this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count > 0)
                {
                    AsyncObservableCollection<IDut> DutList = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList;

                    IEnumerable<List<IPinData>> pinListes = DutList.Select(dut => dut.PinList);

                    List<IPinData> pinList = new List<IPinData>();
                    foreach (List<IPinData> pines in pinListes)
                    {
                        pinList.AddRange(pines);
                    }
                    if (pinListes.Count() > 0)
                    {
                        int MinBlobSizeX = pinList[0].PinSearchParam.MinBlobSizeX.Value;
                        int MinBlobSizeY = pinList[0].PinSearchParam.MinBlobSizeY.Value;

                        int MaxBlobSizeX = pinList[0].PinSearchParam.MaxBlobSizeX.Value;
                        int MaxBlobSizeY = pinList[0].PinSearchParam.MaxBlobSizeY.Value;

                        if (PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Min_X.Value < MinBlobSizeX ||
                            PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Min_X.Value > MaxBlobSizeX ||
                            PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Min_X.Value > PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Max_X.Value)
                        {
                            LoggerManager.Debug($"[HighAlignKeyAdvancedSetupControlService] SetParameters() : MinBlobSizeX = {MinBlobSizeX}, PinTipSizeValidation_Min_X = {PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Min_X.Value}");

                            PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Min_X.Value = MinBlobSizeX;
                        }

                        if (PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Min_Y.Value < MinBlobSizeY ||
                            PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Min_Y.Value > MaxBlobSizeY ||
                            PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Min_Y.Value > PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Max_Y.Value)
                        {
                            LoggerManager.Debug($"[HighAlignKeyAdvancedSetupControlService] SetParameters() : MinBlobSizeY = {MinBlobSizeY}, PinTipSizeValidation_Min_Y = {PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Min_Y.Value}");

                            PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Min_Y.Value = MinBlobSizeY;
                        }

                        if (PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Max_X.Value > MaxBlobSizeX ||
                            PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Max_X.Value < MinBlobSizeX ||
                            PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Max_X.Value < PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Min_X.Value)
                        {
                            LoggerManager.Debug($"[HighAlignKeyAdvancedSetupControlService] SetParameters() : MaxBlobSizeX = {MaxBlobSizeX}, PinTipSizeValidation_Max_X = {PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Max_X.Value}");

                            PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Max_X.Value = MaxBlobSizeX;
                        }

                        if (PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Max_Y.Value > MaxBlobSizeY ||
                            PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Max_Y.Value < MinBlobSizeY ||
                            PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Max_Y.Value < PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Min_Y.Value)
                        {
                            LoggerManager.Debug($"[HighAlignKeyAdvancedSetupControlService] SetParameters() : MaxBlobSizeY = {MaxBlobSizeY}, PinTipSizeValidation_Max_Y = {PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Max_Y.Value}");

                            PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Max_Y.Value = MaxBlobSizeY;
                        }
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NODATA;
                    LoggerManager.Debug($"HighAlignkeyBlobMinMaxSizeSetupModule, Cleanup() DutList count is {this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count}, return value: {retVal}");
                    throw new Exception();
                }
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
            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                PinSearchParameter CurrentPinSerachParam = pininfo.PinSearchParam;

                if (CurrentPinSerachParam.AlignKeyHigh.Count > 0)
                {
                    AlignKeyInfo keyInfo = CurrentPinSerachParam.AlignKeyHigh[CurrentPinSerachParam.AlignKeyIndex.Value];

                    double blobsizemin = keyInfo.BlobSizeMin.Value;
                    double blobsizemax = keyInfo.BlobSizeMax.Value;

                    int pinMin = CurrentPinSerachParam.MinBlobSizeX.Value * CurrentPinSerachParam.MinBlobSizeY.Value;
                    int pinMax = CurrentPinSerachParam.MaxBlobSizeX.Value * CurrentPinSerachParam.MaxBlobSizeY.Value;
                    StepLabel = $"Pin: Min = {pinMin}um^2, Max = {pinMax}um^2 \n " +
                        $"Key: Min = {blobsizemin}um^2, Max = {blobsizemax}um^2 ";

                }
                else
                {
                    StepLabel = $"Not registered key";
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}

