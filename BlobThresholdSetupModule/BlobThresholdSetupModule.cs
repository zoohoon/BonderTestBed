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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

namespace VisionSetupModule
{
    //enum SETTINGITEM
    //{
    //    SEARCHAREA = 0,
    //    BLOBMIN = 1,
    //    BLOBMAX = 2,
    //    THRESHOLD = 3
    //}
    public class BlobThresholdSetupModule : PNPSetupBase, ISetup, IHasDevParameterizable, ITemplateModule, IParamNode, IRecovery, IPackagable
    {
        public override bool Initialized { get; set; } = false;
        private PinSetupMode CurrentMode;
        public override Guid ScreenGUID { get; } = new Guid("EAFB86C4-D46C-D156-CF7F-7AB335BDF82E");

        //double PRSX = 0;
        //double PRSY = 0;

        private ICamera Cam;
        //private SETTINGITEM SettingItem = SETTINGITEM.SEARCHAREA;

        private ushort Threshold = 0;
        private bool isProcessing = false;
        private Task ImgProcThre;

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

        //private IParam _DevParam;
        //[ParamIgnore]
        //public IParam DevParam
        //{
        //    get { return _DevParam; }
        //    set
        //    {
        //        if (value != _DevParam)
        //        {
        //            _DevParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        private int CurPinIndex = 0;            // 현재 선택된 핀 번호
        private int CurDutIndex = 0;
        private int CurPinArrayIndex = 0;       // 현재 선택된 핀의 더트 데이터 상 인덱스 번호

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

        //private ModuleDllInfo _FocusingModuleDllInfo;
        //public ModuleDllInfo FocusingModuleDllInfo
        //{
        //    get { return _FocusingModuleDllInfo; }
        //    set { _FocusingModuleDllInfo = value; }
        //}

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
            LoggerManager.Debug($"[BlobThresholdSetupModule] InitModule() : InitModule Stert");
            try
            {
                if (Initialized == false)
                {
                    //_CoordinateManager = container.Resolve<ICoordinateManager>();
                    //_PinAligner = container.Resolve<IPinAligner>();

                    CurrMaskingLevel = this.ProberStation().MaskingLevel;

                    //retval = InitBackupData();
                    //InitSetup();

                    retval = EventCodeEnum.NONE;

                    //if (retval != EventCodeEnum.NONE)
                    //{
                    //    LoggerManager.Error($"InitBackupData() Failed");
                    //}

                    Initialized = true;
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
            LoggerManager.Debug($"[BlobThresholdSetupModule] InitModule() : InitModule Done");
            return retval;
        }
        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = "Searching Threshold/Light Setup";

                retVal = InitPnpModuleStage_AdvenceSetting();

                retVal = InitLightJog(this);

                //SettingItem = SETTINGITEM.THRESHOLD;

                //AdvanceSetupView = new PinBlobThresholdSetting();
                AdvanceSetupView = new PinBlobThresholdAdvanceSetup.View.BlobThresboldAdvanceSetupView();
                AdvanceSetupViewModel = new PinBlobThresholdAdvanceSetup.ViewModel.BlobThresholdAdvanceSetupViewModel();

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

        public override EventCodeEnum ClearSettingData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //PinParam_Clone = this.PinAligner().PinAlignDevParam_IParam as PinAlignInfo;
                //(AdvanceSetupView as PinBlobThresholdSetting).SettingData(this.PinAligner().PinAlignDevParam as PinAlignDevParameters);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                PackagableParams.Clear();
                PackagableParams.Add(SerializeManager.SerializeToByte(this.PinAligner().PinAlignDevParam));

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
                //LoggerManager.Debug(err);
                throw err;
            }
            return retVal;
        }


        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[BlobThresholdSetupModule] InitSetup() : InitSetup Stert");

            //int defaultlightvalue = 0;

            try
            {
                //retval = InitBackupData();

                //AdvenceSettingDialog = new PinBlobThresholdSetting();

                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"InitBackupData() Failed");
                }

                if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count < 1)
                {
                    retval = EventCodeEnum.NODATA;
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
                }
                LoggerManager.Debug($"[BlobThresholdSetupModule] InitSetup() : Move to Ref Pin");
                this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                LoggerManager.Debug($"[BlobThresholdSetupModule] InitSetup() : Move to Ref Pin Done");


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

                //if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value == 0)
                //{
                //    defaultlightvalue = 85;
                //}
                //else
                //{
                //    defaultlightvalue = (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value;
                //}
                //LoggerManager.Debug($"[BlobThresholdSetupModule] InitSetup() : SetLight = " + defaultlightvalue.ToString());

                //if (Cam == null)
                //{
                //    Cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                //}

                //Cam.SetLight(EnumLightType.COAXIAL, Convert.ToUInt16(defaultlightvalue));

                if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value == 0)
                {
                    Threshold = 127;
                }
                else
                {
                    Threshold = (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value;
                }

                LoggerManager.Debug($"[BlobThresholdSetupModule] InitSetup() : SetThreshold = " + Threshold.ToString());

                UpdateLabel();

                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                retval = InitPNPSetupUI();
                retval = InitLightJog(this);

                //if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width != 0 &&
                //    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height != 0)
                //{
                //    PRSX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width;
                //    PRSY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height;

                //    TargetRectangleWidth = ConvertDisplayWidthPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width, CurCam.Param.GrabSizeX.Value);
                //    TargetRectangleHeight = ConvertDisplayHeightPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height, CurCam.Param.GrabSizeY.Value);
                //}
                //else
                //{
                //    PRSX = 120;
                //    PRSY = 120;
                //}

                LoggerManager.Debug($"[BlobThresholdSetupModule] InitSetup() : Current Search Area Size X = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width.ToString() + " Y = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height.ToString());

                MainViewTarget = DisplayPort;
                MiniViewTarget = null;

                LoadDevParameter();

                this.PinAligner().StopDrawDutOverlay(CurCam);
                this.PinAligner().DrawDutOverlay(CurCam);

                ChangeModeCommandFunc(PinSetupMode.TIPSEARCHAREA);

                //this.VisionManager().StopGrab(CurCam.CameraChannel.Type);
                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                //this.VisionManager().StartBinarizeGrab(EnumProberCam.PIN_HIGH_CAM, Threshold, Convert.ToInt32(PRSX), Convert.ToInt32(PRSY));

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

                //if ((this.PinAligner().PinAlignDevParam as PinAlignDevParameters).EnableAutoThreshold.Value == EnumThresholdType.AUTO)
                //{
                //    isProcessing = false;
                //    this.VisionManager().StartGrab(CurCam.GetChannelType());
                //}
                //else
                //{
                //    // Manual Threshold Mode
                //    if (this.MotionManager().IsEmulMode(this.MotionManager().GetAxis(EnumAxisConstants.PZ)) == false)
                //    {
                //        isProcessing = true;
                //        ImgProcThre = Task.Run(() => ImageProcessing());
                //        //await ImageProcessing();
                //    }
                //    else
                //        isProcessing = false;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "InitSetup() : Error occured.");
            }

            LoggerManager.Debug($"[BlobThresholdSetupModule] InitSetup() : InitSetup Done");

            return Task.FromResult<EventCodeEnum>(retval);
        }

        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                MainViewTarget = DisplayPort;

                Header = "Searching Threshold/Light Setup";

                ProcessingType = EnumSetupProgressState.IDLE;

                PadJogLeftUp.Caption = "Prev";
                PadJogRightUp.Caption = "Next";
                PadJogRightDown.Caption = "";
                //PadJogLeftDown.Caption = "Find\nThreshold";
                PadJogLeftDown.Caption = "";
                PadJogLeftDown.CaptionSize = 18;
                PadJogSelect.Caption = "Ok";

                PadJogLeftUp.Command = new AsyncCommand(DoPrev);
                PadJogRightUp.Command = new AsyncCommand(DoNext);
                PadJogSelect.Command = new AsyncCommand(DoSet);

                //PadJogLeftDown.Command = new AsyncCommand(FindThreshold);


                if ((this.PinAligner().PinAlignDevParam as PinAlignDevParameters)?.EnableAutoThreshold.Value == EnumThresholdType.AUTO)
                {
                    PadJogUp.Caption = "";
                    PadJogDown.Caption = "";
                    PadJogUp.IsEnabled = false;
                    PadJogDown.IsEnabled = false;
                }
                else
                {
                    PadJogUp.Caption = "+";
                    PadJogUp.Command = new RelayCommand(RectHeightSizeUp);
                    PadJogUp.RepeatEnable = true;
                    PadJogUp.IsEnabled = true;

                    PadJogDown.Caption = "-";
                    PadJogDown.Command = new RelayCommand(RectHeightSizeDown);
                    PadJogDown.RepeatEnable = true;
                    PadJogDown.IsEnabled = true;
                }

                PadJogLeft.Caption = "";
                PadJogRight.Caption = "";

                PadJogLeft.IsEnabled = false;
                PadJogRight.IsEnabled = false;
                PadJogRightDown.IsEnabled = false;
                PadJogLeftUp.IsEnabled = true;
                PadJogRightUp.IsEnabled = true;
                PadJogLeftDown.IsEnabled = false;
                PadJogSelect.IsEnabled = true;

                MainViewZoomVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                //OneButton.Visibility = System.Windows.Visibility.Visible;
                //TwoButton.Visibility = System.Windows.Visibility.Visible;//==> Focus
                //ThreeButton.Visibility = System.Windows.Visibility.Visible;
                //FourButton.Visibility = System.Windows.Visibility.Hidden;
                //FiveButton.Visibility = System.Windows.Visibility.Visible;

                OneButton.Caption = "Set All";
                TwoButton.Caption = "Focusing";
                TwoButton.CaptionSize = 17;
                ThreeButton.Caption = "One Pin\nAlign";
                ThreeButton.CaptionSize = 19;
                FourButton.Caption = "Binary\nOn/Off";
                FourButton.CaptionSize = 17;

                OneButton.Command = Button1Command;
                TwoButton.Command = Button2Command;
                ThreeButton.Command = Button3Command;
                FourButton.Command = Button4Command;

                PnpManager.PnpLightJog.HighBtnEventHandler = new RelayCommand(CameraHighButton);
                PnpManager.PnpLightJog.LowBtnEventHandler = new RelayCommand(CameraLowButton);

                // Auto threshold option
                //FiveButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PNP_Setting.png");
                //FiveButton.Command = Button5Command;
                //FiveButton.IsEnabled = true;

                //TwoButton.MaskingLevel = 3;


                if (this.PinAligner().IsRecoveryStarted == true) //this.PinAligner().GetPAInnerStateEnum() == PinAlignInnerStateEnum.RECOVERY)
                {
                    PadJogRightDown.Caption = "Next Fail";
                    PadJogLeftDown.Caption = "Prev Fail";
                    PadJogRightDown.IsEnabled = true;
                    PadJogLeftDown.IsEnabled = true;
                    PadJogLeftDown.Command = new AsyncCommand(DoPrevFailPin);
                    PadJogRightDown.Command = new AsyncCommand(DoNextFailPin);
                    FourButton.Caption = "Show\nResult";
                    //FourButton.Command = Button4Command;
                    FourButton.Visibility = System.Windows.Visibility.Visible;
                }

                //UseUserControl = UserControlFucEnum.UNDEFINED;
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            FocusParam.FocusingAxis.Value = EnumAxisConstants.PZ;

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
            this.PinAligner().SavePinAlignDevParam();
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

                // CurPinIndex 현재 선택된 실제 핀 번호이지만, 데이터 상으로는 더트별로 해당 핀이 들어가 있는 배열의 인덱스는 핀 번호하고는 다르다.
                // 따라서 배열상의 어레이로 변환해 주어야 한다.
                CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);

                foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
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
                    this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                    }
                }
                else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinLowViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                    }
                }


                Threshold = (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value;
                //Cam.SetLight(EnumLightType.COAXIAL, (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value);
                //PRSY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height;
                //PRSX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width;

                UpdateLabel();

                //LoggerManager.Debug($"[BlobThresholdSetupModule] Next() : Move to Next Pin #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value + " Threshold = " + Threshold.ToString() + " Light = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value);

                //ImageProcessingEx();
            }
            catch (Exception err)
            {
                LoggerManager.Error(err + "Next() : Error occured.");
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

                foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
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
                    this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                    }
                }
                else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinLowViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                    }
                }
                Threshold = (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value;
                //Cam.SetLight(EnumLightType.COAXIAL, (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value);
                //PRSY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height;
                //PRSX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width;

                UpdateLabel();

                //LoggerManager.Debug($"[BlobThresholdSetupModule] Prev() : Move to Previous Pin #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value + " Threshold = " + Threshold.ToString() + " Light = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value);

                //ImageProcessingEx();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "Prev() : Error occured.");
                LoggerManager.Exception(err);
            }
            //this.StageSupervisor().StageModuleState.PinHighViewMove(ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
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
                    if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        }
                    }
                    else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinLowViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                        }
                    }

                    foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }

                    Threshold = (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value;
                    //Cam.SetLight(EnumLightType.COAXIAL, (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value);
                    //PRSY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height;
                    //PRSX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width;

                    UpdateLabel();

                    //LoggerManager.Debug($"[BlobThresholdSetupModule] NextFailPin() : Move to next fail pin #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value + " Threshold = " + Threshold.ToString() + " Light = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value);
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
                    if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        }
                    }
                    else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinLowViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                        }
                    }

                    foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }

                    Threshold = (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value;
                    //Cam.SetLight(EnumLightType.COAXIAL, (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value);
                    //PRSY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height;
                    //PRSX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width;

                    UpdateLabel();

                    //LoggerManager.Debug($"[BlobThresholdSetupModule] PrevFailPin() : Move to previous fail pin #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value + " Threshold = " + Threshold.ToString() + " Light = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value);
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
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        public void Set()
        {
            try
            {
                if ((this.PinAligner().PinAlignDevParam as PinAlignDevParameters)?.EnableAutoThreshold.Value == EnumThresholdType.MANUAL)
                {
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value = Threshold;
                }

                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Clear();

                foreach (var light in CurCam.LightsChannels)
                {
                    int val = CurCam.GetLight(light.Type.Value);
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AddLight(new LightValueParam(light.Type.Value, (ushort)val));
                }

                //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value = Cam.GetLight(EnumLightType.COAXIAL);
                //LoggerManager.Debug($"[BlobThresholdSetupModule] Set() : Set Threshold/Light Start");
                //LoggerManager.Debug($"[BlobThresholdSetupModule] Set() : Currernt Pin #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value);
                //LoggerManager.Debug($"[BlobThresholdSetupModule] Set() : Original Threshold/Light  Threshold = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value.ToString() + " Light = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value.ToString());
                //LoggerManager.Debug($"[BlobThresholdSetupModule] Set() : New Threshold/Light  Threshold = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value.ToString() + " Light = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value.ToString());

                //UpdateData();
                SaveProbeCardData();
                this.PinAligner().SavePinAlignDevParam();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "Set() : Error occured.");
                LoggerManager.Exception(err);
            }

            LoggerManager.Debug($"[BlobThresholdSetupModule] Set() : Set Threshold/Light Done");
            //this.StageSupervisor().StageModuleState.PinHighViewMove(ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
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

        public void RectHeightSizeUp()
        {
            try
            {
                //isProcessing = false;

                if (Threshold < 255)
                {
                    Threshold++;
                    if (Threshold > 255)
                        Threshold = 255;

                    //LoggerManager.Debug(Threshold.ToString());
                }
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value = Convert.ToInt32(Threshold);
                //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value = Cam.GetLight(EnumLightType.COAXIAL);
                UpdateLabel();

                // update original data directly, set button will be removed
                //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value = Convert.ToInt32(Threshold);
                //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value = Cam.GetLight(EnumLightType.COAXIAL);

                //isProcessing = true;
                //ImageProcessingEx();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "RectHeightSizeUp() : Error occured.");
                LoggerManager.Exception(err);
            }


        }
        public void RectHeightSizeDown()
        {
            try
            {

                if (Threshold > 255)
                    Threshold = 255;
                if (Threshold > 0)
                    Threshold--;
                //.Info(Threshold.ToString());
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value = Convert.ToInt32(Threshold);
                //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value = Cam.GetLight(EnumLightType.COAXIAL);

                UpdateLabel();

                // update original data directly, set button will be removed
                //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.Value = Convert.ToInt32(Threshold);
                //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value = Cam.GetLight(EnumLightType.COAXIAL);

                //ImageProcessingEx();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "RectHeightSizeDown() : Error occured.");
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
                        "Threshold for all of pins will be updated at once, \nDo you want to proceed?", EnumMessageStyle.AffirmativeAndNegative);

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
        //        Task t = Task.Run(async () =>
        //        {
        //            EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Confirm To Change All",
        //                "Threshold for all of pins will be updated at once, \nDo you want to proceed?", EnumMessageStyle.AffirmativeAndNegative);
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
        //}

        public void SetAll()
        {
            try
            {
                

                foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (PinData pin in dut.PinList)
                    {
                        if ((this.PinAligner().PinAlignDevParam as PinAlignDevParameters)?.EnableAutoThreshold.Value == EnumThresholdType.MANUAL)
                            pin.PinSearchParam.BlobThreshold.Value = Threshold; // (int)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.BlobThreshold.GetValue();

                        pin.PinSearchParam.LightForTip.Clear();
                        foreach (var light in CurCam.LightsChannels)
                        {
                            int val = CurCam.GetLight(light.Type.Value);
                            pin.PinSearchParam.AddLight(new LightValueParam(light.Type.Value, (ushort)val));
                        }
                    }
                }

                //UpdateData();
                SaveProbeCardData();
                this.PinAligner().SavePinAlignDevParam();

                LoggerManager.Debug($"[BlobThresholdSetupModule] SetAll() : Set All Threshold Done");
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "SetAll() : Error occured.");
            }
            finally
            {
                
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
                    LoggerManager.Debug($"[BlobThresholdSetupModule] DoFocusing() : One pin focusing Start");
                    isProcessing = false;
                    if (ImgProcThre != null) await ImgProcThre;

                    DoingFocusing = true;

                    await Focusing();

                    DoingFocusing = false;
                    LoggerManager.Debug($"[BlobThresholdSetupModule] DoFocusing() : One pin focusing done");

                    //if ((this.PinAligner().PinAlignDevParam as PinAlignDevParameters).EnableAutoThreshold.Value == EnumThresholdType.MANUAL)
                    //{
                    //    isProcessing = true;
                    //    ImgProcThre = Task.Run(() => ImageProcessing());

                    //    //ImgProcThre = ImageProcessing();
                    //    //await ImageProcessing();
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. BlobThresholdSetupModule - DoFocusing() : Error occured.");
            }
            finally
            {
                
            }
        }
        private async Task Focusing()
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
                FocusParam.PeakRangeThreshold.Value = 100;

                //LoggerManager.Debug($"[BlobThresholdSetupModule] Focusing() : Focusing Parameter ROI X = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinindex].PinSearchParam.SearchArea.Value.Width.ToString() + " Y = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinindex].PinSearchParam.SearchArea.Value.Height.ToString());
                //FocusingParam.FocusingROI.Value = new System.Windows.Rect(OffsetX, OffsetY, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinindex].PinSearchParam.SearchArea.Value.Width, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinindex].PinSearchParam.SearchArea.Value.Height);
                FocusParam.FocusRange.Value = Convert.ToInt32(PinAlignParam.PinFocusingRange.Value);
                LoggerManager.Debug($"[BlobThresholdSetupModule] Focusing() : Focusing Parameter Focusing Range = " + FocusParam.FocusRange.Value.ToString());
                FocusParam.PeakRangeThreshold.Value = 100;

                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                if (PinFocusModel.Focusing_Retry(FocusParam, false, false, false, this) != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Fail", "Focusing Fail", EnumMessageStyle.Affirmative);
                    LoggerManager.Debug($"[BlobThresholdSetupModule] Focusing() : Focusing Fail");
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Focusing Success", EnumMessageStyle.Affirmative);
                    LoggerManager.Debug($"[BlobThresholdSetupModule] Focusing() : Focusing Success");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

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

        #region pnp button 3
        private AsyncCommand _Button3Command;
        public ICommand Button3Command
        {
            get
            {
                if (null == _Button3Command)
                    _Button3Command = new AsyncCommand(
                    DoOnepinAlign//, EvaluationPrivilege.Evaluate(
                                 // CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                                 // new Action(() => { ShowMessages("UIModeChange"); })
                            );
                return _Button3Command;
            }
        }

        private bool DoingOnepinAlign = false;
        private async Task DoOnepinAlign()
        {
            try
            {
                

                if (!DoingOnepinAlign)
                {
                    LoggerManager.Debug($"[BlobThresholdSetupModule] DoOnepinAlign() : One pin Align Start");
                    isProcessing = false;
                    if (ImgProcThre != null) await ImgProcThre;

                    DoingOnepinAlign = true;

                    await OnePinAlign();
                    DoingOnepinAlign = false;

                    LoggerManager.Debug($"[BlobThresholdSetupModule] DoOnepinAlign() : One pin Align Done");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. BlobThresholdSetupModule - DoFocusing() : Error occured.");
            }
            finally
            {
                
            }
        }
        private async Task OnePinAlign()
        {
            PINALIGNRESULT EachPinResult = PINALIGNRESULT.PIN_SKIP;
            PinCoordinate NewPinPos;
            try
            {
                EachPinResult = this.PinAligner().SinglePinAligner.SinglePinalign(out NewPinPos, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex], this.PinFocusModel, FocusParam);

                if (EachPinResult == PINALIGNRESULT.PIN_PASSED)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "One Pin Align Test Success", EnumMessageStyle.Affirmative);
                    LoggerManager.Debug($"[BlobThresholdSetupModule] Focusing() : One Pin Align Success");
                }
                else if (EachPinResult == PINALIGNRESULT.PIN_FOCUS_FAILED)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Fail", "One Pin Align Focusing Fail", EnumMessageStyle.Affirmative);
                    LoggerManager.Debug($"[BlobThresholdSetupModule] Focusing() : One Pin Align Focusing Fail");
                }
                else if (EachPinResult == PINALIGNRESULT.PIN_TIP_FOCUS_FAILED)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Fail", "One Pin Align Tip Focusing Fail", EnumMessageStyle.Affirmative);
                    LoggerManager.Debug($"[BlobThresholdSetupModule] Focusing() : One Pin Align Tip Focusing Fail");
                }
                else if (EachPinResult == PINALIGNRESULT.PIN_BLOB_FAILED)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Fail", "One Pin Align Blob Search Fail", EnumMessageStyle.Affirmative);
                    LoggerManager.Debug($"[BlobThresholdSetupModule] Focusing() : One Pin Align Blob Search Fail");
                }
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
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }
        #endregion

        #region pnp button 5
        private AsyncCommand _Button5Command;
        public ICommand Button5Command
        {
            get
            {
                if (null == _Button5Command)
                    _Button5Command = new AsyncCommand(DoAutoThresholdSetup
                            //DoShowResult//, EvaluationPrivilege.Evaluate(
                            // CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                            // new Action(() => { ShowMessages("UIModeChange"); })
                            );
                return _Button5Command;
            }
        }

        private async Task DoAutoThresholdSetup()
        {
            try
            {
                

                await this.MetroDialogManager().ShowWindow(AdvanceSetupView);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                if ((this.PinAligner().PinAlignDevParam as PinAlignDevParameters).EnableAutoThreshold.Value == EnumThresholdType.AUTO)
                {
                    isProcessing = false;
                    this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                    PadJogUp.Caption = "";
                    PadJogDown.Caption = "";
                    PadJogUp.IsEnabled = false;
                    PadJogDown.IsEnabled = false;
                }
                else
                {
                    //isProcessing = true;
                    //ImgProcThre = Task.Run(() => ImageProcessing());

                    //ImgProcThre = ImageProcessing();
                    //await ImageProcessing();

                    PadJogUp.Caption = "+";
                    PadJogUp.Command = new RelayCommand(RectHeightSizeUp);
                    PadJogUp.RepeatEnable = true;
                    PadJogUp.IsEnabled = true;

                    PadJogDown.Caption = "-";
                    PadJogDown.Command = new RelayCommand(RectHeightSizeDown);
                    PadJogDown.RepeatEnable = true;
                    PadJogDown.IsEnabled = true;
                }

                
            }
        }

        #endregion

        public SubModuleMovingStateBase MovingState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        //private async Task CmdSetBlobMaxSize()
        //{
        //    UseUserControl = UserControlFucEnum.PTRECT;
        //    SettingItem = SETTINGITEM.BLOBMAX;

        //    OneButton.IsEnabled = true;
        //    TwoButton.IsEnabled = true;
        //    ThreeButton.IsEnabled = true;
        //    FourButton.IsEnabled = false;

        //    PadJogLeft.IsEnabled = true;
        //    PadJogRight.IsEnabled = true;

        //    if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MaxBlobSizeX.Value != 0 &&
        //            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MaxBlobSizeY.Value != 0)
        //    {
        //        TargetRectangleWidth = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MaxBlobSizeX.Value * CurCam.GetRatioX();
        //        TargetRectangleHeight = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MaxBlobSizeY.Value * CurCam.GetRatioY();
        //    }
        //    else
        //    {
        //        TargetRectangleWidth = 120;
        //        TargetRectangleHeight = 120;
        //    }
        //    //ImageProcessingEx();
        //}

        public override async Task Save()
        {
            try
            {
                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Vision Setup Save",
                    "Do you want to save the contents of the Vision Setup ?", EnumMessageStyle.AffirmativeAndNegative);
                if (result == EnumMessageDialogResult.AFFIRMATIVE)
                {

                    //foreach (var module in this.PinAligner().SubModules.EntryTemplateModules)
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

        private async Task<EventCodeEnum> ImageProcessing()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            int tmpthreshold = 0;

            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}] ImageProcessing() : ImageProcessing Start");
                string imgpath = string.Empty;

                this.VisionManager().StopGrab(CurCam.GetChannelType());
                ImageBuffer image = new ImageBuffer();
                this.VisionManager().VisionLib.RecipeInit();

                Task task = new Task(() =>
                {
                    while (isProcessing)
                    {
                        if (CurCam.GetChannelType() == EnumProberCam.PIN_HIGH_CAM)
                        {

                            IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                            PinSearchParameter CurrentPinSerachParam = pininfo.PinSearchParam;

                            double ratioX = CurCam.GetRatioX();
                            double ratioY = CurCam.GetRatioY();

                            int OffsetX = 0;
                            int OffsetY = 0;

                            int RealSearchAreaX = 0;
                            int RealSearchAreaY = 0;

                            var area = CurrentPinSerachParam.SearchArea.Value;

                            //RealSearchAreaX = Convert.ToInt32(area.Width / ratioX);
                            //RealSearchAreaY = Convert.ToInt32(area.Height / ratioY);

                            RealSearchAreaX = Convert.ToInt32(area.Width);
                            RealSearchAreaY = Convert.ToInt32(area.Height);

                            OffsetX = ((int)CurCam.GetGrabSizeWidth() / 2) - (int)(RealSearchAreaX / 2);
                            OffsetY = ((int)CurCam.GetGrabSizeHeight() / 2) - (int)(RealSearchAreaY / 2);

                            bool isDilation = this.PinAlignParam.EnableDilation.Value;

                            if (isDilation)
                            {
                                //이미지 그랩
                                ImageBuffer curImg = this.VisionManager().SingleGrab(EnumProberCam.PIN_HIGH_CAM, this);

                                // TODO : 20220928
                                //그랩이미지를 전달할 변수로 변환
                                (byte[], int, int, int)[] inputImages = { (curImg.Buffer, curImg.SizeX, curImg.SizeY, 1) };

                                bool autoMode = true;

                                if (PinAlignParam.EnableAutoThreshold.Value != EnumThresholdType.AUTO) // 이진화 기준값 설정이 자동이 아니면 사용자 설정값으로 적용.
                                {
                                    tmpthreshold = Convert.ToInt32(CurrentPinSerachParam.BlobThreshold.Value);
                                    autoMode = false;
                                }

                                //사용자 입력값을 필터에 설정
                                //tmpThreshold  메뉴얼 이진화 기준값  autoMode가 false일때 동작됨, 그외 무시됨.
                                //autoMode  이진화 자동,수동 선택 
                                //OffsetX OffsetY RealSearchAreaX RealSearchAreaY   x,y,width,height 값에 대응. 입력 이미지를 설정 영역으로 크롭.  출력 이미지는 width,height로 조정됨.
                                string recipeParam = $"{{" +
                                    $" \"ThresholdFilter\" : {{\"Params\":[  {{\"Key\":\"Threshold\", \"Value\":\"{tmpthreshold}\"}}]}}" +
                                    $", \"PinBlobModeSelector\" : {{\"Params\":[  {{\"Key\":\"AutoMode\", \"Value\":\"{autoMode}\"}}]}}" +
                                    $", \"CropFilter\" : {{\"Params\":[  {{\"Key\":\"x\", \"Value\":\"{OffsetX}\"}},{{\"Key\":\"y\", \"Value\":\"{OffsetY}\"}},{{\"Key\":\"width\", \"Value\":\"{RealSearchAreaX}\"}},{{\"Key\":\"height\", \"Value\":\"{RealSearchAreaY}\"}}]}}" +
                                    $"}}";

                                (byte[] image, int width, int height) procImg = this.VisionManager().
                                    VisionLib.RecipeExecute(images: inputImages
                                    , recipe: this.VisionManager().VisionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.SinglePinAlignTipBlob)
                                    , result: out string recipeResult
                                    , param: recipeParam
                                    );

                                //프로세싱 결과값 반환. 자동일 경우 자동으로 계산된 threshold값이 반환됨, 수동일 경우 적용된 threshold 그대로 반환.
                                int.TryParse(this.VisionManager().VisionLib.FindValue(recipeResult, "threshold"), out tmpthreshold);
                                Threshold = Convert.ToUInt16(tmpthreshold);


                                // Merge  그랩 이미지 + 이진화 이미지 
                                curImg.CopyTo(image);
                                for (int i = 0; i < RealSearchAreaX; i++)
                                {
                                    for (int j = 0; j < RealSearchAreaY; j++)
                                    {
                                        image.Buffer[(i + OffsetX) + ((j + OffsetY) * curImg.SizeX)] = procImg.image[i + j * RealSearchAreaX];
                                    }
                                }

                                //GC
                                procImg.image = null;
                            }
                            else
                            {
                                if (PinAlignParam.EnableAutoThreshold.Value == EnumThresholdType.AUTO)
                                {
                                    tmpthreshold = this.PinAligner().SinglePinAligner.getOtsuThreshold(CurCam.GetChannelType(), OffsetX, OffsetY, RealSearchAreaX, RealSearchAreaY);

                                    if (tmpthreshold <= 0)
                                    {
                                        tmpthreshold = 127;
                                    }

                                    Threshold = Convert.ToUInt16(tmpthreshold);
                                }
                                else
                                {
                                    tmpthreshold = Convert.ToInt32(CurrentPinSerachParam.BlobThreshold.Value);
                                }

                                this.VisionManager().Binarize(EnumProberCam.PIN_HIGH_CAM, ref image, tmpthreshold, OffsetX, OffsetY, RealSearchAreaX, RealSearchAreaY);
                            }

                            UpdateLabel();
                            this.VisionManager().ImageGrabbed(EnumProberCam.PIN_HIGH_CAM, image);
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(33);
                        }
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

        public EventCodeEnum SaveProbeCardData()
        {
            EventCodeEnum serialRes = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[BlobThresholdSetupModule] SaveProbeCardData() : Save Start");

            try
            {
                serialRes = this.StageSupervisor().SaveProberCard();

                if (serialRes == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"SaveDevParameter() : Save Ok.");
                }
                else
                {
                    LoggerManager.Debug($"SaveDevParameter() : Save Fail.");
                }
            }
            catch (Exception err)
            {
                throw err;
            }
            LoggerManager.Debug($"[BlobThresholdSetupModule] SaveProbeCardData() : Save Done");
            return serialRes;
        }

        public EventCodeEnum UpdateData()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[BlobThreaholdSetupModule] UpdateData() : Data Update Start");
            try
            {
                foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (Dut oridut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                    {
                        if (dut.MacIndex.XIndex == oridut.MacIndex.XIndex && dut.MacIndex.YIndex == oridut.MacIndex.YIndex)
                        {
                            foreach (PinData pin in dut.PinList)
                            {
                                foreach (PinData oripin in oridut.PinList)
                                {
                                    if (pin.PinNum.Value == oripin.PinNum.Value)
                                    {
                                        oripin.PinSearchParam.BlobThreshold.Value = (int)pin.PinSearchParam.BlobThreshold.GetValue();

                                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Clear();
                                        foreach (var light in CurCam.LightsChannels)
                                        {
                                            int val = CurCam.GetLight(light.Type.Value);
                                            oripin.PinSearchParam.AddLight(new LightValueParam(light.Type.Value, (ushort)val));
                                        }
                                        //oripin.PinSearchParam.LightForTip.Value = (int)pin.PinSearchParam.LightForTip.GetValue();
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
            LoggerManager.Debug($"[BlobThreaholdSetupModule] UpdateData() : Data Update Done");
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
                retVal = SubModuleState.Recovery();
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
        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[BlobThresholdSetupModule] Cleanup() : Cleanup Start");
            try
            {
                isProcessing = false;
                if (ImgProcThre != null)
                {
                    await ImgProcThre;
                    ImgProcThre.Dispose();
                    ImgProcThre = null;
                }

                SetNodeSetupState(EnumMoudleSetupState.NONE);
                LoggerManager.Debug($"[BlobThresholdSetupModule] Cleanup() : Blob Threshold Setup Done");
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
            LoggerManager.Debug($"[BlobThresholdSetupModule] Cleanup() : Cleanup Done");
            return retVal;
        }

        public Task<EventCodeEnum> InitRecovery()
        {
            throw new NotImplementedException();
        }

        public void ClearState()
        {
            throw new NotImplementedException();
        }
        public int getOtsuThreshold()
        {
            byte t = 0;
            float[] vet = new float[256];
            int[] hist = new int[256];
            try
            {
                vet.Initialize();
                hist.Initialize();

                float p1, p2, p12;
                int k;
                int OffsetX = 0, OffsetY = 0;

                ImageBuffer curImg = null;
                curImg = this.VisionManager().SingleGrab(EnumProberCam.PIN_HIGH_CAM, this);

                OffsetX = (curImg.SizeX / 2) - (int)(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width / 2);
                OffsetY = (curImg.SizeY / 2) - (int)(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height / 2);

                for (int i = 0; i < curImg.SizeX; i++)
                {
                    for (int j = 0; j < curImg.SizeY; j++)
                    {
                        if ((i >= OffsetX && i <= (OffsetX + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width)) && (j >= OffsetY && j <= (OffsetY + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height)))
                        {
                            hist[curImg.Buffer[(curImg.SizeX * j) + i]]++;
                        }
                    }
                }

                int minindex = 0, maxindex = 0;

                for (int i = 0; i < 256; i++)
                {
                    if (hist[i] != 0)
                    {
                        minindex = i;
                        break;
                    }
                }
                for (int i = 255; i >= 0; i--)
                {
                    if (hist[i] != 0)
                    {
                        maxindex = i;
                        break;
                    }
                }


                for (k = minindex + 1; k != maxindex; k++)
                {
                    p1 = Px(minindex, k, hist);             // 제일 어두운 색부터 제일 밝은 색까지 모두 더한 값
                    p2 = Px(k + 1, maxindex, hist);
                    p12 = p1 * p2;
                    if (p12 == 0)
                        p12 = 1;
                    float diff = (Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1);
                    vet[k] = (float)diff * diff / p12;

                }

                t = (byte)findMax(vet, 256);
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "getOtsuThreshold() : Error occured.");
            }
            return t;
        }

        private float Px(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;
            try
            {
                for (i = init; i <= end; i++)
                    sum += hist[i];
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "getOtsuThreshold() : Error occured.");
            }
            return (float)sum;
        }

        // function is used to compute the mean values in the equation (mu)
        private float Mx(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;
            try
            {
                for (i = init; i <= end; i++)
                    sum += i * hist[i];
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "getOtsuThreshold() : Error occured.");
            }
            return (float)sum;
        }

        // finds the maximum element in a vector
        private int findMax(float[] vec, int n)
        {
            float maxVec = 0;
            int idx = 0;
            int i;

            try
            {
                for (i = 1; i < n - 1; i++)
                {
                    if (vec[i] > maxVec)
                    {
                        maxVec = vec[i];
                        idx = i;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "getOtsuThreshold() : Error occured.");
            }
            return idx;
        }

        public override void UpdateLabel()
        {
            try
            {
                string thresholdmodsStr = string.Empty;

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                PinSearchParameter CurrentPinSerachParam = pininfo.PinSearchParam;

                if (PinAlignParam.EnableAutoThreshold.Value == EnumThresholdType.AUTO)
                {
                    thresholdmodsStr = "(AUTO)";
                }
                else
                {
                    thresholdmodsStr = "(MANUAL)";
                }

                StepLabel = $"Current PinTip Threshold {thresholdmodsStr}: {Threshold}";
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
    }
}
