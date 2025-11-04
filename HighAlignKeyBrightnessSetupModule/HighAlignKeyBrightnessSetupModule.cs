using HighAlignKeyProcAdvanceSetup;
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
using SerializerUtil;
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
    public class HighAlignKeyBrightnessSetupModule : PNPSetupBase, ISetup, ITemplateModule, IParamNode, IRecovery, IHasDevParameterizable, IPackagable, IHasAdvancedSetup
    {
        public override bool Initialized { get; set; } = false;

        public override Guid ScreenGUID { get; } = new Guid("AA2D6613-33F2-46DB-9EC4-2FD1D483B207");

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

        public void UpdateBlobType()
        {
            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                var curKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;

                AlignKeyInfo keyinfo = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex];

                keyinfo.ImageBlobType = PinAlignParam.EnableAutoThreshold.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public new void CloseAdvanceSetupView()
        {
            try
            {
                UpdateBlobType();

                ChangeModeCommandFunc(this.CurrentMode);

                UpdateLabel();
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
                Header = "Processing Setup";

                retVal = InitPnpModuleStage_AdvenceSetting();
                retVal = InitLightJog(this);

                SetNodeSetupState(EnumMoudleSetupState.NONE);

                AdvanceSetupView = new HighAlignKeyProcAdvanceSetupView();
                AdvanceSetupViewModel = new HighAlignKeyProcAdvanceSetupViewModel();

                //SetPackagableParams();

                //AdvanceSetupViewModel.SetParameters(PackagableParams);

                //(AdvanceSetupViewModel as HighAlignKeyProcAdvanceSetupViewModel).SettingData(this.PinAligner().PinAlignDevParam as PinAlignDevParameters);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                //LoggerManager.Debug(err);
                throw err;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private int keyThreshold = 0;
        private bool isProcessing = false;
        private Task ImgProcThre;

        private async Task<EventCodeEnum> ImageProcessing()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            int tmpthreshold = 0;
            try
            {
                LoggerManager.Debug($"[HighAlignKeyBrightnessSetupModule] ImageProcessing() : ImageProcessing Start");
                string imgpath = string.Empty;

                this.VisionManager().StopGrab(CurCam.GetChannelType());
                ImageBuffer image = null;

                Task task = new Task(() =>
                {
                    while (isProcessing)
                    {
                        IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                        PinSearchParameter CurrentPinSerachParam = pininfo.PinSearchParam;
                        AlignKeyInfo keyinfo = CurrentPinSerachParam.AlignKeyHigh[CurrentPinSerachParam.AlignKeyIndex.Value];

                        double ratioX = CurCam.GetRatioX();
                        double ratioY = CurCam.GetRatioY();

                        int ROISizeX = 0;
                        int ROISizeY = 0;

                        int OffsetX = 0;
                        int OffsetY = 0;

                    int RealSearchAreaX = 0;
                    int RealSearchAreaY = 0;

                    if (bToogleStateAlignKey == false)
                    {
                        var area = CurrentPinSerachParam.SearchArea.Value;
                        //OffsetX = (int)area.X / ratioX;
                        //OffsetY = (int)area.Y / ratioY;


                        RealSearchAreaX = Convert.ToInt32(area.Width / ratioX);
                        RealSearchAreaY = Convert.ToInt32(area.Height / ratioY);

                        OffsetX = ((int)CurCam.GetGrabSizeWidth() / 2) - (int)(RealSearchAreaX / 2);
                        OffsetY = ((int)CurCam.GetGrabSizeHeight() / 2) - (int)(RealSearchAreaY / 2);


                        tmpthreshold = Convert.ToInt32(CurrentPinSerachParam.BlobThreshold.Value);
                    }
                    else
                    {
                        keyinfo = CurrentPinSerachParam.AlignKeyHigh[CurrentPinSerachParam.AlignKeyIndex.Value];
                        
                        bool BlobExtensionFlag = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobExtension.Value;

                       

                        if (BlobExtensionFlag == true)
                        {
                            double ExtensionWidth = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobExtensionWidth.Value;
                            double ExtensionHeight = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobExtensionHeight.Value;

                            int ExtensionWidthPixel = 0;
                            int ExtensionHeightPixel = 0;

                            if (ExtensionWidth > 0)
                            {
                                ExtensionWidthPixel = Convert.ToInt32(ExtensionWidth / ratioX);
                            }

                            if (ExtensionHeight > 0)
                            {
                                ExtensionHeightPixel = Convert.ToInt32(ExtensionHeight / ratioY);
                            }

                            int SignX = keyinfo.AlignKeyPos.X.Value < 0 ? -1 : 1;
                            int SignY = keyinfo.AlignKeyPos.Y.Value > 0 ? -1 : 1;

                            ROISizeX = Convert.ToInt32(keyinfo.BlobRoiSizeX.Value / ratioX);
                            ROISizeY = Convert.ToInt32(keyinfo.BlobRoiSizeY.Value / ratioY);

                            if (SignX == -1)
                            {
                                OffsetX = ((int)CurCam.GetGrabSizeWidth() / 2) - (int)(ROISizeX / 2) + (SignX * ExtensionWidthPixel);
                            }
                            else
                            {
                                OffsetX = ((int)CurCam.GetGrabSizeWidth() / 2) - (int)(ROISizeX / 2);
                            }

                            if (SignY == -1)
                            {
                                OffsetY = ((int)CurCam.GetGrabSizeHeight() / 2) - (int)(ROISizeY / 2) + (SignY * ExtensionHeightPixel);
                            }
                            else
                            {
                                OffsetY = ((int)CurCam.GetGrabSizeHeight() / 2) - (int)(ROISizeY / 2);
                            }

                            RealSearchAreaX = ROISizeX + ExtensionWidthPixel;
                            RealSearchAreaY = ROISizeY + ExtensionHeightPixel;
                        }
                        else
                        {
                            ROISizeX = Convert.ToInt32(keyinfo.BlobRoiSizeX.Value / ratioX);
                            ROISizeY = Convert.ToInt32(keyinfo.BlobRoiSizeY.Value / ratioY);

                            OffsetX = ((int)CurCam.GetGrabSizeWidth() / 2) - (int)(ROISizeX / 2);
                            OffsetY = ((int)CurCam.GetGrabSizeHeight() / 2) - (int)(ROISizeY / 2);

                            RealSearchAreaX = ROISizeX;
                            RealSearchAreaY = ROISizeY;
                        }

                        string extrastr = string.Empty;

                        //if ((this.PinAligner().PinAlignDevParam as PinAlignDevParameters)?.EnableAutoThreshold.Value == EnumThresholdType.AUTO)
                        if (keyinfo.ImageBlobType == EnumThresholdType.AUTO)
                        {
                            tmpthreshold = this.PinAligner().SinglePinAligner.getOtsuThreshold(CurCam.GetChannelType(), OffsetX, OffsetY, RealSearchAreaX, RealSearchAreaY);

                            if (tmpthreshold <= 0)
                            {
                                tmpthreshold = 127;
                            }
                        }
                        else
                        {
                            tmpthreshold = Convert.ToInt32(keyinfo.BlobThreshold.Value);
                        }

                        keyThreshold = tmpthreshold;
                    }

                    

                    UpdateLabel();

                    this.VisionManager().Binarize(EnumProberCam.PIN_HIGH_CAM, ref image, tmpthreshold, OffsetX, OffsetY, RealSearchAreaX, RealSearchAreaY);

                    this.VisionManager().ImageGrabbed(EnumProberCam.PIN_HIGH_CAM, image);

                        //this.VisionManager().DisplayProcessing(EnumProberCam.PIN_HIGH_CAM, image);

                        //if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        //{
                        //    imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value)).ToString() + ".bmp";
                        //    this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        //}


                        //if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        //{
                        //    imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value)).ToString() + ".bmp";
                        //    this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        //}
                    }
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
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
            }

            LoggerManager.Debug($"[BlobThresholdSetupModule] ImageProcessing() : ImageProcessing Done");
            return retVal;
        }

        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                retVal = await InitSetup();

                SetPackagableParams();
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

            // TODO : SEARCH AREA Mode 일 때, JOG의 상하좌우를 모두 사용
            // Threshold Mode일 때, 상하만 사용

            if (CurrentMode == PinSetupMode.TIPANDKEYSEARCHAREA)
            {
                PadJogLeftDown.IsEnabled = false;
                PadJogRightDown.IsEnabled = true;

                PadJogUp.IsEnabled = true;
                PadJogDown.IsEnabled = true;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;

                PadJogUp.RepeatEnable = false;
                PadJogDown.RepeatEnable = false;
                PadJogLeft.RepeatEnable = false;
                PadJogRight.RepeatEnable = false;

                PadJogUp.Command = ChangeSearchAreaCommand;
                PadJogDown.Command = ChangeSearchAreaCommand;
                PadJogLeft.Command = ChangeSearchAreaCommand;
                PadJogRight.Command = ChangeSearchAreaCommand;

                //PadJogUp.CommandParameter = EnumArrowDirection.UP;
                //PadJogDown.CommandParameter = EnumArrowDirection.DOWN;
                //PadJogLeft.CommandParameter = EnumArrowDirection.LEFT;
                //PadJogRight.CommandParameter = EnumArrowDirection.RIGHT;

                PadJogLeftDown.IsEnabled = false;
                PadJogRightDown.IsEnabled = true;

                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.TIPANDKEYSEARCHAREA, true);
                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.THRESHOLD, false);
            }
            else if (CurrentMode == PinSetupMode.THRESHOLD)
            {
                if (PinAlignParam.EnableAutoThreshold.Value == EnumThresholdType.MANUAL)
                {
                    //PadJogUp.Caption = "▲";
                    PadJogUp.Command = ChangeManualThresholdCommand;
                    //PadJogUp.CommandParameter = EnumArrowDirection.UP;
                    //PadJogUp.RepeatEnable = true;
                    PadJogUp.Visibility = Visibility.Visible;
                    PadJogUp.IsEnabled = true;

                    //PadJogDown.Caption = "▼";
                    PadJogDown.Command = ChangeManualThresholdCommand;
                    //PadJogDown.CommandParameter = EnumArrowDirection.DOWN;
                    //PadJogDown.RepeatEnable = true;
                    PadJogDown.Visibility = Visibility.Visible;
                    PadJogDown.IsEnabled = true;

                    PadJogLeft.IsEnabled = false;
                    PadJogLeft.Visibility = Visibility.Hidden;

                    PadJogRight.IsEnabled = false;
                    PadJogRight.Visibility = Visibility.Hidden;
                }
                else // AUTO
                {
                    //PadJogUp.Caption = "";
                    //PadJogUp.Command = null;
                    //PadJogUp.RepeatEnable = false;
                    PadJogUp.IsEnabled = false;
                    PadJogUp.Visibility = Visibility.Hidden;

                    //PadJogDown.Caption = "";
                    //PadJogDown.Command = null;
                    //PadJogDown.RepeatEnable = true;
                    PadJogDown.IsEnabled = false;
                    PadJogDown.Visibility = Visibility.Hidden;

                    PadJogLeft.IsEnabled = false;
                    PadJogLeft.Visibility = Visibility.Hidden;

                    PadJogRight.IsEnabled = false;
                    PadJogRight.Visibility = Visibility.Hidden;
                }

                PadJogLeftDown.IsEnabled = true;
                PadJogRightDown.IsEnabled = false;

                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.TIPANDKEYSEARCHAREA, true);
                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.THRESHOLD, true);
            }
            else
            {
                LoggerManager.Error($"Parameter is wrong. {CurrentMode}");
            }

            CurCam.UpdateOverlayFlag = true;
            UpdateLabel();
        }

        public async Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //InitBackupData();

            LoggerManager.Debug($"[HighAlignKeyBrightnessSetupModule] InitSetup() : InitSetup Stert");
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

                LoadDevParameter();

                this.PinAligner().StopDrawDutOverlay(CurCam);
                this.PinAligner().DrawDutOverlay(CurCam);

                // Real-Time Binarization
                if (this.MotionManager().IsEmulMode(this.MotionManager().GetAxis(EnumAxisConstants.PZ)) == false)
                {
                    if (isProcessing != true && ImgProcThre == null)
                    {
                        isProcessing = true;
                        //ImgProcThre = Task.Run(() => ImageProcessing());
                        ImgProcThre = ImageProcessing();

                    }
                }
                else
                {
                    isProcessing = false;
                }

                CurCam.UpdateOverlayFlag = true;

                Task task = new Task(() =>
                {
                    ChangeModeCommandFunc(PinSetupMode.TIPANDKEYSEARCHAREA);
                });
                task.Start();
                await task;

                TargetRectangleWidth = 0;
                TargetRectangleHeight = 0;

                LoggerManager.Debug($"[HighAlignKeyBrightnessSetupModule] InitSetup() : InitSetup Done");

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Error($err + "InitSetup() : Error occured.");
            }

            return retVal;
        }

        private RelayCommand<EnumArrowDirection> _ChangeSearchAreaCommand;
        public ICommand ChangeSearchAreaCommand
        {
            get
            {
                if (null == _ChangeSearchAreaCommand)
                    _ChangeSearchAreaCommand = new RelayCommand<EnumArrowDirection>(ChangeSearchAreaCommandFunc);
                return _ChangeSearchAreaCommand;
            }
        }

        private void ChangeSearchAreaCommandFunc(EnumArrowDirection param)
        {
            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                PinSearchParameter CurrentPinSerachParam = pininfo.PinSearchParam;


                double ratioX = CurCam.GetRatioX();
                double ratioY = CurCam.GetRatioY();
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

                    var searchArea = CurrentPinSerachParam.SearchArea.Value;

                    if ((searchArea.Width + offsetX) != 0 && (offsetX != 0) && (((searchArea.Width + offsetX) / ratioX) < PossibleMaxSizeX))
                    {
                        searchArea.Width += offsetX;
                        CurrentPinSerachParam.SearchArea.Value = searchArea;
                        changed = true;
                    }

                    if ((searchArea.Height + offsetY) != 0 && (offsetY != 0) && (((searchArea.Height + offsetY) / ratioY) < PossibleMaxSizeY))
                    {
                        searchArea.Height += offsetY;
                        CurrentPinSerachParam.SearchArea.Value = searchArea;
                        changed = true;
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

                            

                            if ((keyinfo.BlobRoiSizeX.Value + offsetX) != 0 && (offsetX != 0) && (((keyinfo.BlobRoiSizeX.Value + offsetX) / ratioX) < PossibleMaxSizeX))
                            {
                                keyinfo.BlobRoiSizeX.Value += offsetX;
                                changed = true;
                            }

                            if ((keyinfo.BlobRoiSizeY.Value + offsetY) != 0 && (offsetY != 0) && (((keyinfo.BlobRoiSizeY.Value + offsetY) / ratioY) < PossibleMaxSizeY))
                            {
                                keyinfo.BlobRoiSizeY.Value += offsetY;
                                changed = true;
                            }


                            if (changed == true)
                            {
                                keyinfo.FocusingAreaSizeX.Value = keyinfo.BlobRoiSizeX.Value;
                                keyinfo.FocusingAreaSizeY.Value = keyinfo.BlobRoiSizeY.Value;

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

        private RelayCommand<EnumArrowDirection> _ChangeManualThresholdCommand;
        public ICommand ChangeManualThresholdCommand
        {
            get
            {
                if (null == _ChangeManualThresholdCommand)
                    _ChangeManualThresholdCommand = new RelayCommand<EnumArrowDirection>(ChangeManualThresholdCommandFunc);
                return _ChangeManualThresholdCommand;
            }
        }

        private void ChangeManualThresholdCommandFunc(EnumArrowDirection param)
        {
            try
            {
               

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                PinSearchParameter CurrentPinSerachParam = pininfo.PinSearchParam;

                if(bToogleStateAlignKey == false)
                {
                    //double offsetX = 0;
                    double offsetY = 0;

                    switch (param)
                    {
                        case EnumArrowDirection.LEFTUP:
                        case EnumArrowDirection.RIGHTUP:
                        case EnumArrowDirection.LEFTDOWN:
                        case EnumArrowDirection.RIGHTDOWN:
                        case EnumArrowDirection.LEFT:
                        case EnumArrowDirection.RIGHT:
                            LoggerManager.Debug("Not Support.");
                            break;

                        case EnumArrowDirection.UP:
                            offsetY++;
                            break;
                        case EnumArrowDirection.DOWN:
                            offsetY--;
                            break;
                        default:
                            break;
                    }

                    if (PinAlignParam.EnableAutoThreshold.Value == EnumThresholdType.MANUAL)
                    {
                        if (CurrentPinSerachParam.BlobThreshold.Value + offsetY <= 255)
                        {
                            CurrentPinSerachParam.BlobThreshold.Value = (int)(CurrentPinSerachParam.BlobThreshold.Value + offsetY);
                            //Threshold = Convert.ToInt32(CurrentPinSerachParam.BlobThreshold.Value);
                            CurCam.UpdateOverlayFlag = true;
                            UpdateLabel();
                        }
                    }
                }
                else
                {
                    //double offsetX = 0;
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
                                case EnumArrowDirection.LEFT:
                                case EnumArrowDirection.RIGHT:
                                    LoggerManager.Debug("Not Support.");
                                    break;

                                case EnumArrowDirection.UP:
                                    offsetY++;
                                    break;
                                case EnumArrowDirection.DOWN:
                                    offsetY--;
                                    break;
                                default:
                                    break;
                            }

                            if ((CurrentPinSerachParam.AlignKeyHigh[currentalignkeyindex].ImageProcType == IMAGE_PROC_TYPE.PROC_BLOB) &&
                                (CurrentPinSerachParam.AlignKeyHigh[currentalignkeyindex].ImageBlobType == EnumThresholdType.MANUAL))
                            {
                                if (CurrentPinSerachParam.AlignKeyHigh[currentalignkeyindex].BlobThreshold.Value + offsetY <= 255)
                                {
                                    CurrentPinSerachParam.AlignKeyHigh[currentalignkeyindex].BlobThreshold.Value += offsetY;
                                    CurCam.UpdateOverlayFlag = true;

                                    keyThreshold = Convert.ToInt32(CurrentPinSerachParam.AlignKeyHigh[currentalignkeyindex].BlobThreshold.Value);

                                    UpdateLabel();
                                }
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


        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                MainViewTarget = DisplayPort;

                Header = "Processing Setup";

                ProcessingType = EnumSetupProgressState.IDLE;

                PadJogLeftUp.Caption = "Prev";
                PadJogRightUp.Caption = "Next";
                PadJogLeftUp.Command = new AsyncCommand(DoPrev);
                PadJogRightUp.Command = new AsyncCommand(DoNext);

                PadJogSelect.Caption = "Toggle";

                PadJogLeftDown.Caption = "Search Area";
                PadJogLeftDown.Command = ChangeModeCommand;
                PadJogLeftDown.CommandParameter = PinSetupMode.TIPANDKEYSEARCHAREA;

                PadJogRightDown.Caption = "Threshold";
                PadJogRightDown.Command = ChangeModeCommand;
                PadJogRightDown.CommandParameter = PinSetupMode.THRESHOLD;

                PadJogSelect.Command = new AsyncCommand(DoToggle);

                PadJogUp.Caption = "▲";
                //PadJogUp.Command = ChangeManualThresholdCommand;
                PadJogUp.CommandParameter = EnumArrowDirection.UP;
                PadJogUp.RepeatEnable = false;

                PadJogDown.Caption = "▼";
                //PadJogDown.Command = ChangeManualThresholdCommand;
                PadJogDown.CommandParameter = EnumArrowDirection.DOWN;
                PadJogDown.RepeatEnable = false;

                PadJogLeft.Caption = "◀";
                //PadJogLeft.Command = ChangeSearchAreaCommand;
                PadJogLeft.CommandParameter = EnumArrowDirection.LEFT;
                PadJogLeft.RepeatEnable = false;

                PadJogRight.Caption = "▶";
                //PadJogRight.Command = ChangeSearchAreaCommand;
                PadJogRight.CommandParameter = EnumArrowDirection.RIGHT;
                PadJogRight.RepeatEnable = false;

                //PadJogLeft.Caption = "";
                //PadJogRight.Caption = "";

                //PadJogUp.IsEnabled = false;
                //PadJogDown.IsEnabled = false;
                //PadJogLeft.IsEnabled = false;
                //PadJogRight.IsEnabled = false;

                PadJogLeftUp.IsEnabled = true;
                PadJogRightUp.IsEnabled = true;
                PadJogSelect.IsEnabled = true;


                MainViewZoomVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                OneButton.Visibility = System.Windows.Visibility.Visible;
                TwoButton.Visibility = System.Windows.Visibility.Visible;
                ThreeButton.Visibility = System.Windows.Visibility.Visible;
                FourButton.Visibility = System.Windows.Visibility.Visible;
                FiveButton.Visibility = System.Windows.Visibility.Visible;

                // TODO : REMOVE
                //AdvanceSetupUISetting();

                OneButton.Caption = "Set All";
                OneButton.Command = Button1Command;

                TwoButton.Caption = "Set";
                TwoButton.Command = Button2Command;

                ThreeButton.Caption = "Focus Area";
                ThreeButton.Command = Button3Command;

                FourButton.Caption = "Binary On/Off";
                FourButton.Command = Button4Command;

                if (this.PinAligner().IsRecoveryStarted == true)
                {
                    FourButton.Caption = "Show\nResult";
                    FourButton.Command = Button4Command;
                    FourButton.Visibility = System.Windows.Visibility.Visible;
                }

                UseUserControl = UserControlFucEnum.DEFAULT;
                TargetRectangleHeight = 0;
                TargetRectangleWidth = 0;

                ChangeModeCommandFunc(this.CurrentMode);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {

                throw err;
            }
            return retVal;
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
            finally
            {
                
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

                CurCam.UpdateOverlayFlag = true;

                UpdateCameraLightValue();

                UpdateLabel();
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

                CurCam.UpdateOverlayFlag = true;
                UpdateLabel();
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

                CurCam.UpdateOverlayFlag = true;
                UpdateLabel();
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
                
                if (bToogleStateAlignKey == false)
                {
                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Confirm To Change All",
                    "Brightness for all of pins will be updated at once, \nDo you want to proceed?", EnumMessageStyle.AffirmativeAndNegative);

                    if (result == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        SetAll();
                    }
                }
                else
                {
                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Confirm To Change All",
                    "Brightenss for all of align marks will be updated at once, \nDo you want to proceed?", EnumMessageStyle.AffirmativeAndNegative);

                    if (result == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        SetAll();
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
            LoggerManager.Debug($"[HighAlignKeyBrightnessSetupModule] SetAll() : Set All Alignmark Brightness Start");

            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                
                    
                var curKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;

                AlignKeyInfo keyinfo = pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex];

                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (IPinData pin in dut.PinList)
                    {
                        //if (bToogleStateAlignKey == false)
                        //{
                        //    if (pin.PinSearchParam.LightForTip == null)
                        //    {
                        //        pin.PinSearchParam.LightForTip = new List<LightValueParam>();
                        //    }

                        //    pin.PinSearchParam.LightForTip.Clear();

                        //    foreach (var light in CurCam.LightsChannels)
                        //    {
                        //        int val = CurCam.GetLight(light.Type.Value);
                        //        pin.PinSearchParam.LightForTip.Add(new LightValueParam(light.Type.Value, (ushort)val));
                        //    }
                        //}
                        //else
                        //{
                        // 1. Light
                        // 2. Search Area
                        // 3. Threshold Value
                        // 4. Threshold Mode

                        if(bToogleStateAlignKey == false)
                        {
                            pin.PinSearchParam.LightForTip.Clear();

                            foreach (var light in CurCam.LightsChannels)
                            {
                                int val = CurCam.GetLight(light.Type.Value);
                                pin.PinSearchParam.LightForTip.Add(new LightValueParam(light.Type.Value, (ushort)val));
                            }

                            if (PinAlignParam.EnableAutoThreshold.Value == EnumThresholdType.MANUAL)
                            {
                                pin.PinSearchParam.BlobThreshold.Value = pininfo.PinSearchParam.BlobThreshold.Value;
                            }
                            pin.PinSearchParam.SearchArea.Value = new Rect(pin.PinSearchParam.SearchArea.Value.X, pin.PinSearchParam.SearchArea.Value.Y, pininfo.PinSearchParam.SearchArea.Value.Width, pininfo.PinSearchParam.SearchArea.Value.Height);
                            
                        }
                        else
                        {
                            foreach (AlignKeyInfo curMark in pin.PinSearchParam.AlignKeyHigh)
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

                                curMark.BlobRoiSizeX.Value = keyinfo.BlobRoiSizeX.Value;
                                curMark.BlobRoiSizeY.Value = keyinfo.BlobRoiSizeY.Value;

                                curMark.FocusingAreaSizeX.Value = keyinfo.FocusingAreaSizeX.Value;
                                curMark.FocusingAreaSizeY.Value = keyinfo.FocusingAreaSizeY.Value;

                                curMark.ImageBlobType = keyinfo.ImageBlobType;

                                if (PinAlignParam.EnableAutoThreshold.Value == EnumThresholdType.MANUAL)
                                {
                                    curMark.BlobThreshold.Value = keyThreshold;
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
            LoggerManager.Debug($"[HighAlignKeyBrightnessSetupModule] SetAll() : Set All Search Area Done");
        }
        #endregion

        #region pnp button 2
        private AsyncCommand _Button2Command;
        public ICommand Button2Command
        {
            get
            {
                if (null == _Button2Command)
                    _Button2Command = new AsyncCommand(DoSet);
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
        private Task Set()
        {
            try
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        #endregion

        #region pnp button 3
        private RelayCommand _Button3Command;
        public ICommand Button3Command
        {
            get
            {
                if (null == _Button3Command)
                    _Button3Command = new RelayCommand(Button3CommandFunc);
                return _Button3Command;
            }
        }


        private void Button3CommandFunc()
        {
            try
            {
                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.KEYFOCUSINGAREA, (!this.PinAligner().GetAlignKeySetupControlFlag(PinSetupMode.KEYFOCUSINGAREA)));
                CurCam.UpdateOverlayFlag = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region pnp button 4
        private AsyncCommand _Button4Command;
        public ICommand Button4Command
        {
            get
            {
                if (null == _Button4Command)
                    _Button4Command = new AsyncCommand(BinaryOnOff);
                return _Button4Command;
            }
        }

        private async Task BinaryOnOff()
        {
            try
            {
                isProcessing = !isProcessing;

                if (isProcessing)
                {
                    if (ImgProcThre != null)
                    {
                        await ImgProcThre;
                        ImgProcThre.Dispose();
                        ImgProcThre = null;
                    }
                    ImgProcThre = ImageProcessing();
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
                //this.PinAligner().StopDrawPinOverlay(CurCam);
                this.PinAligner().StopDrawDutOverlay(CurCam);

                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.TIPANDKEYSEARCHAREA, false);
                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.THRESHOLD, false);
                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.KEYFOCUSINGAREA, false);

                // Task 종료
                isProcessing = false;

                if (ImgProcThre != null)
                {
                    await ImgProcThre;
                    ImgProcThre.Dispose();
                    ImgProcThre = null;
                }

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
            try
            {
                if (CurrentMode == PinSetupMode.THRESHOLD)
                {
                    string thresholdmodsStr = string.Empty;

                    IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                    PinSearchParameter CurrentPinSerachParam = pininfo.PinSearchParam;
                    AlignKeyInfo keyinfo = CurrentPinSerachParam.AlignKeyHigh[CurrentPinSerachParam.AlignKeyIndex.Value];

                    //if ((this.PinAligner().PinAlignDevParam as PinAlignDevParameters)?.EnableAutoThreshold.Value == EnumThresholdType.AUTO)
                    if (keyinfo.ImageBlobType == EnumThresholdType.AUTO)
                    {
                        thresholdmodsStr = "(AUTO)";
                    }
                    else
                    {
                        thresholdmodsStr = "(MANUAL)";
                    }

                    if(PinAlignParam.EnableAutoThreshold.Value == EnumThresholdType.AUTO)
                    {
                        //thresholdtipmodsStr = "(AUTO)";
                        thresholdmodsStr = "(AUTO)";
                    }
                    else
                    {
                        //thresholdtipmodsStr = "(MANUAL)";
                        thresholdmodsStr = "(MANUAL)";
                    }

                    StepLabel = $"Current PinTip Threshold {thresholdmodsStr}:{CurrentPinSerachParam.BlobThreshold.Value}\n" +
                        $" Current Key Threshold {thresholdmodsStr}: {keyThreshold}";
                }
                else if (CurrentMode == PinSetupMode.TIPANDKEYSEARCHAREA)
                {
                    IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                    PinSearchParameter CurrentPinSerachParam = pininfo.PinSearchParam;
                    AlignKeyInfo keyinfo = pininfo.PinSearchParam.AlignKeyHigh[CurrentPinSerachParam.AlignKeyIndex.Value];

                    double SearchAreaWidth = keyinfo.BlobRoiSizeX.Value;
                    double SearchAreaHeight = keyinfo.BlobRoiSizeY.Value;

                    bool BlobExtensionFlag = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobExtension.Value;

                    if (BlobExtensionFlag == true)
                    {
                        double ExtensionWidth = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobExtensionWidth.Value;
                        double ExtensionHeight = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobExtensionHeight.Value;

                        StepLabel = $"Key Search Area : Width = {SearchAreaWidth:F0}µm, Height = {SearchAreaHeight:F0}µm \n" +
                            $"PinTip Search Area : Width = {CurrentPinSerachParam.SearchArea.Value.Width:F0}µm, Height = {CurrentPinSerachParam.SearchArea.Value.Height:F0}µm \n"+
                            $"Extension : Width = {ExtensionWidth:F0}µm, Height = {ExtensionHeight:F0}µm";
                    }
                    else
                    {
                        StepLabel = $"Key Search Area : Width = {SearchAreaWidth:F0}µm, Height = {SearchAreaHeight:F0}µm\n" +
                            $"PinTip Search Area : Width = {CurrentPinSerachParam.SearchArea.Value.Width:F0}µm, Height = {CurrentPinSerachParam.SearchArea.Value.Height:F0}µm" ;
                    }
                }
                else
                {
                    StepLabel = $"";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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

        public void ApplyParams(List<byte[]> datas)
        {
            try
            {
                PackagableParams = datas;

                foreach (var param in datas)
                {
                    object target;
                    SerializeManager.DeserializeFromByte(param, out target, typeof(PinAlignDevParameters));

                    if (target != null)
                    {
                        this.PinAligner().PinAlignDevParam = (PinAlignDevParameters)target;
                        break;
                    }
                }

                this.PinAligner().SaveDevParameter();
                this.PinAligner().CollectElement();

                UpdateBlobType();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
