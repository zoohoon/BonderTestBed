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
    //enum SETTINGITEM
    //{
    //    SEARCHAREA = 0,
    //    BLOBMIN = 1,
    //    BLOBMAX = 2,
    //    THRESHOLD = 3
    //}
    public class BlobMinSizeSetupModule : PNPSetupBase, ISetup, ITemplateModule, IParamNode, IRecovery, IHasDevParameterizable
    {
        public override bool Initialized { get; set; } = false;
        private PinSetupMode CurrentMode;

        public override Guid ScreenGUID { get; } = new Guid("4DA84CB6-311F-469D-8AEC-B2899A854DF9");

        //double PRSX = 0;
        //double PRSY = 0;

        //double RectSizeRaitioX = 0;
        //double RectSizeRaitioY = 0;

        private ICamera Cam;
        //private SETTINGITEM SettingItem = SETTINGITEM.BLOBMIN;
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
        //private IFocusing FocusingModule { get; set; }

        //private IFocusParameter FocusingParam { get; set; }

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
            LoggerManager.Debug($"[BlobMinSizeSetupModule] InitModule() : InitModule Stert");
            try
            {
                if (Initialized == false)
                {
                    //_CoordinateManager = container.Resolve<ICoordinateManager>();
                    //_PinAligner = container.Resolve<IPinAligner>();

                    CurrMaskingLevel = this.ProberStation().MaskingLevel;
                    //InitBackupData();

                    //InitSetup();

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
            LoggerManager.Debug($"[BlobMinSizeSetupModule] InitModule() : InitModule Done");
            return retval;
        }
        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = "Pin Minimum Size Setup";

                retVal = InitPnpModuleStage();

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
            LoggerManager.Debug($"[BlobMinSizeSetupModule] InitSetup() : InitSetup Stert");

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
                }

                this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);

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

                //SettingItem = SETTINGITEM.BLOBMIN;

                //if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeX.Value != 0 &&
                //    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeY.Value != 0)
                //{
                //    PRSX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width;
                //    PRSY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height;

                //    TargetRectangleWidth = ConvertDisplayWidthPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeX.Value, CurCam.Param.GrabSizeX.Value);
                //    TargetRectangleHeight = ConvertDisplayHeightPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeY.Value, CurCam.Param.GrabSizeY.Value);
                //}
                //else
                //{
                //    PRSX = 120;
                //    PRSY = 120;

                //    TargetRectangleWidth = ConvertDisplayWidthPNP(120, CurCam.Param.GrabSizeX.Value);
                //    TargetRectangleHeight = ConvertDisplayHeightPNP(120, CurCam.Param.GrabSizeY.Value);
                //}

                //UseUserControl = UserControlFucEnum.PTRECT;

                LoggerManager.Debug($"[BlobMinSizeSetupModule] InitSetup() : Current Search Area Size X = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width.ToString() + " Y = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height.ToString());
                LoggerManager.Debug($"[BlobMinSizeSetupModule] InitSetup() : Min Blob Size X = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeX.Value.ToString() + " Y = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeY.Value.ToString());

                MainViewTarget = DisplayPort;
                MiniViewTarget = null;

                //double width = 0, height = 0;
                //GetDisplayPortActualSize(ref width, ref height);

                //RectSizeRaitioX = (double)CurCam.Param.GrabSizeX.Value / 890;
                //RectSizeRaitioY = (double)CurCam.Param.GrabSizeY.Value / 890;

                LoadDevParameter();

                this.PinAligner().StopDrawDutOverlay(CurCam);
                this.PinAligner().DrawDutOverlay(CurCam);

                ChangeModeCommandFunc(PinSetupMode.TIPBLOBMIN);

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Error($err + "InitSetup() : Error occured.");
            }

            LoggerManager.Debug($"[BlobMinSizeSetupModule] InitSetup() : InitSetup Done");

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                //SettingItem = SETTINGITEM.SEARCHAREA;

                MainViewTarget = DisplayPort;

                Header = "Pin Minimum Size Setup";

                ProcessingType = EnumSetupProgressState.IDLE;

                PadJogLeftUp.Caption = "Prev";
                PadJogRightUp.Caption = "Next";
                PadJogRightDown.Caption = "";
                PadJogRightDown.Caption = "";
                PadJogSelect.Caption = "Ok";

                PadJogLeftUp.Command = new AsyncCommand(DoPrev);
                PadJogRightUp.Command = new AsyncCommand(DoNext);
                PadJogSelect.Command = new AsyncCommand(DoSet);

                //PadJogUp.Caption = "+";
                //PadJogUp.Command = new RelayCommand(RectHeightSizeUp);
                //PadJogUp.RepeatEnable = true;

                //PadJogDown.Caption = "-";
                //PadJogDown.Command = new RelayCommand(RectHeightSizeDown);
                //PadJogDown.RepeatEnable = true;

                //PadJogLeft.Caption = "-";
                //PadJogLeft.Command = new RelayCommand(RectWidthSizeDown);
                //PadJogLeft.RepeatEnable = true;

                //PadJogRight.Caption = "+";
                //PadJogRight.Command = new RelayCommand(RectWidthSizeUp);
                //PadJogRight.RepeatEnable = true;

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
                PadJogRightDown.IsEnabled = false;
                PadJogLeftUp.IsEnabled = true;
                PadJogRightUp.IsEnabled = true;
                PadJogLeftDown.IsEnabled = false;
                PadJogSelect.IsEnabled = true;
                PadJogUp.IsEnabled = true;
                PadJogDown.IsEnabled = true;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;

                MainViewZoomVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                OneButton.Visibility = System.Windows.Visibility.Visible;
                TwoButton.Visibility = System.Windows.Visibility.Visible;//==> Focus
                ThreeButton.Visibility = System.Windows.Visibility.Visible;
                FourButton.Visibility = System.Windows.Visibility.Hidden;
                FiveButton.Visibility = System.Windows.Visibility.Collapsed;

                OneButton.Caption = "Set All";
                TwoButton.Caption = "Focusing";
                TwoButton.CaptionSize = 17;
                ThreeButton.Caption = "One Pin\nAlign";
                ThreeButton.CaptionSize = 19;

                OneButton.Command = Button1Command;
                TwoButton.Command = Button2Command;
                ThreeButton.Command = Button3Command;

                PnpManager.PnpLightJog.HighBtnEventHandler = new RelayCommand(CameraHighButton);
                PnpManager.PnpLightJog.LowBtnEventHandler = new RelayCommand(CameraLowButton);

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
            LoggerManager.Debug($"[BlobMinSizeSetupModule] LoadDevParameter() : Load Parameter Start");
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            FocusParam.FocusingAxis.Value = this.StageSupervisor().StageModuleState.PinViewAxis;

            LoggerManager.Debug($"[BlobMinSizeSetupModule] LoadDevParameter() : Load Parameter Done");
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
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                    }
                }
                else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                    }
                }


                Threshold = (ushort)pininfo.PinSearchParam.BlobThreshold.Value;
                //Cam.SetLight(EnumLightType.COAXIAL, (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value);
                //PRSY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height;
                //PRSX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width;

                //TargetRectangleWidth = ConvertDisplayWidthPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeX.Value, CurCam.Param.GrabSizeX.Value);
                //TargetRectangleHeight = ConvertDisplayHeightPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeY.Value, CurCam.Param.GrabSizeY.Value);

                LoggerManager.Debug($"[BlobMinSizeSetupModule] Next() : Move to Next Pin #" + pininfo.PinNum.Value + " Min Blob Size X = " + pininfo.PinSearchParam.MinBlobSizeX.Value.ToString() + " Y = " + pininfo.PinSearchParam.MinBlobSizeY.Value.ToString());

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
                    }
                }
                else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                    }
                }

                Threshold = (ushort)pininfo.PinSearchParam.BlobThreshold.Value;
                //Cam.SetLight(EnumLightType.COAXIAL, (ushort)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip.Value);
                //PRSY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height;
                //PRSX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width;

                //TargetRectangleWidth = ConvertDisplayWidthPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeX.Value, CurCam.Param.GrabSizeX.Value);
                //TargetRectangleHeight = ConvertDisplayHeightPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeY.Value, CurCam.Param.GrabSizeY.Value);

                LoggerManager.Debug($"[BlobMinSizeSetupModule] Prev() : Move to Previous Pin #" + pininfo.PinNum.Value + " Min Blob Size X = " + pininfo.PinSearchParam.MinBlobSizeX.Value.ToString() + " Y = " + pininfo.PinSearchParam.MinBlobSizeY.Value.ToString());
                //ImageProcessingEx();
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "Prev() : Error occured.");
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
                    IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                    if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinHighViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        }
                    }
                    else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
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

                    //TargetRectangleWidth = ConvertDisplayWidthPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeX.Value, CurCam.Param.GrabSizeX.Value);
                    //TargetRectangleHeight = ConvertDisplayHeightPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeY.Value, CurCam.Param.GrabSizeY.Value);

                    LoggerManager.Debug($"[BlobMinSizeSetupModule] NextFailPin() : Move to Next Fail Pin #" + pininfo.PinNum.Value + " Min Blob Size X = " + pininfo.PinSearchParam.MinBlobSizeX.Value.ToString() + " Y = " + pininfo.PinSearchParam.MinBlobSizeY.Value.ToString());
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
                        }
                    }
                    else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
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

                    //TargetRectangleWidth = ConvertDisplayWidthPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeX.Value, CurCam.Param.GrabSizeX.Value);
                    //TargetRectangleHeight = ConvertDisplayHeightPNP(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeY.Value, CurCam.Param.GrabSizeY.Value);
                    LoggerManager.Debug($"[BlobMinSizeSetupModule] PrevFailPin() : Move to Previous Fail Pin #" + pininfo.PinNum.Value + " Min Blob Size X = " + pininfo.PinSearchParam.MinBlobSizeX.Value.ToString() + " Y = " + pininfo.PinSearchParam.MinBlobSizeY.Value.ToString());
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
            //LoggerManager.Debug($"[BlobMinSizeSetupModule] Set() : Set Blob Min Size Start");
            //LoggerManager.Debug($"[BlobMinSizeSetupModule] Set() : Currernt Pin #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value);
            //LoggerManager.Debug($"[BlobMinSizeSetupModule] Set() : Original Blob Min Size X = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeX.ToString() + " Y = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeY.ToString());
            //LoggerManager.Debug($"[BlobMinSizeSetupModule] Set() : New Blob Min Size X  = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeX.ToString() + " Y = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeY.ToString());
            try
            {
                // 이미 변경되어 있음.
                //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeX.Value = (int)TargetRectangleWidth;
                //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeY.Value = (int)TargetRectangleHeight;

                //UpdateData();
                SaveProbeCardData();
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "Set() : Error occured.");
            }
            LoggerManager.Debug($"[BlobMinSizeSetupModule] Set() : Set Blob Min Size Done");
            //this.StageSupervisor().StageModuleState.PinHighViewMove(ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
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

                if (CurrentMode == PinSetupMode.TIPBLOBMIN)
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
                else if (CurrentMode == PinSetupMode.TIPBLOBMAX)
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

                if (CurrentMode == PinSetupMode.TIPBLOBMIN)
                {
                    this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.TIPBLOBMIN, true);
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


        #region pnp button 1
        private AsyncCommand _Button1Command;
        public ICommand Button1Command
        {
            get
            {
                if (null == _Button1Command)
                    _Button1Command = new AsyncCommand(
                    DoConfirmToSetAll//, EvaluationPrivilege.Evaluate(
                                     // CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                                     // new Action(() => { ShowMessages("UIModeChange"); })
                            );
                return _Button1Command;
            }
        }
        private async Task DoConfirmToSetAll()
        {
            try
            {
                

                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Confirm To Change All",
                    "Minimum tip size for all of pins will be updated at once, \nDo you want to proceed?", EnumMessageStyle.AffirmativeAndNegative);
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
        public void SetAll()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            //LoggerManager.Debug($"[BlobMinSizeSetupModule] SetAll() : Set All Min Size Start");
            //LoggerManager.Debug($"[BlobMinSizeSetupModule] SetAll() : Set All Blob Min Size X = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeX.Value.ToString() + " Y = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeY.Value.ToString());
            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (PinData pin in dut.PinList)
                    {
                        pin.PinSearchParam.MinBlobSizeX = pininfo.PinSearchParam.MinBlobSizeX;
                        pin.PinSearchParam.MinBlobSizeY = pininfo.PinSearchParam.MinBlobSizeY;

                        //if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value != pin.PinNum.Value)
                        //{
                        //pin.PinSearchParam.MinBlobSizeX.Value = (int)TargetRectangleWidth;// (int)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeX.GetValue();
                        //pin.PinSearchParam.MinBlobSizeY.Value = (int)TargetRectangleHeight; // this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.MinBlobSizeY.GetValue();
                        //}
                    }
                }

                LoggerManager.Debug($"[{this.GetType().Name}] SetAll() : Set All Min X Size [{pininfo.PinSearchParam.MinBlobSizeX}], Y Size[{pininfo.PinSearchParam.MinBlobSizeY}]");

                //UpdateData();
                retval = SaveProbeCardData();
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "SetAll() : Error occured.");
            }
            LoggerManager.Debug($"[BlobMinSizeSetupModule] SetAll() : Set All Min Size Done");
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
                    LoggerManager.Debug($"[BlobMinSizeSetupModule] DoFocusing() : One pin focusing Start");

                    DoingFocusing = true;

                    await Focusing();
                    DoingFocusing = false;
                    LoggerManager.Debug($"[BlobMinSizeSetupModule] DoFocusing() : One pin focusing done");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. BlobMinSizeSetupModule - DoFocusing() : Error occured.");
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

                //LoggerManager.Debug($"[BlobMinSizeSetupModule] Focusing() : Focusing Parameter ROI X = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinindex].PinSearchParam.SearchArea.Value.Width.ToString() + " Y = " + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinindex].PinSearchParam.SearchArea.Value.Height.ToString());
                //FocusingParam.FocusingROI.Value = new System.Windows.Rect(OffsetX, OffsetY, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinindex].PinSearchParam.SearchArea.Value.Width, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinindex].PinSearchParam.SearchArea.Value.Height);
                FocusParam.FocusRange.Value = Convert.ToInt32(PinAlignParam.PinFocusingRange.Value);
                LoggerManager.Debug($"[BlobMinSizeSetupModule] Focusing() : Focusing Parameter Focusing Range = " + FocusParam.FocusRange.Value.ToString());
                FocusParam.PeakRangeThreshold.Value = 100;

                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                if (PinFocusModel.Focusing_Retry(FocusParam, false, false, false, this) != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Fail", "Focusing Fail", EnumMessageStyle.Affirmative);
                    LoggerManager.Debug($"[BlobMinSizeSetupModule] Focusing() : Focusing Fail");
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Focusing Success", EnumMessageStyle.Affirmative);
                    LoggerManager.Debug($"[BlobMinSizeSetupModule] Focusing() : Focusing Success");
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
                    LoggerManager.Debug($"[BlobMinSizeSetupModule] DoOnepinAlign() : One pin Align Start");

                    DoingOnepinAlign = true;

                    await OnePinAlign();
                    DoingOnepinAlign = false;

                    LoggerManager.Debug($"[BlobMinSizeSetupModule] DoOnepinAlign() : One pin Align Done");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. BlobMinSizeSetupModule - DoOnepinAlign() : Error occured.");
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
                    LoggerManager.Debug($"[BlobMinSizeSetupModule] Focusing() : One Pin Align Success");
                }
                else if (EachPinResult == PINALIGNRESULT.PIN_FOCUS_FAILED)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Fail", "One Pin Align Focusing Fail", EnumMessageStyle.Affirmative);
                    LoggerManager.Debug($"[BlobMinSizeSetupModule] Focusing() : One Pin Align Focusing Fail");
                }
                else if (EachPinResult == PINALIGNRESULT.PIN_TIP_FOCUS_FAILED)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Fail", "One Pin Align Tip Focusing Fail", EnumMessageStyle.Affirmative);
                    LoggerManager.Debug($"[BlobMinSizeSetupModule] Focusing() : One Pin Align Tip Focusing Fail");
                }
                else if (EachPinResult == PINALIGNRESULT.PIN_BLOB_FAILED)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Fail", "One Pin Align Blob Search Fail", EnumMessageStyle.Affirmative);
                    LoggerManager.Debug($"[BlobMinSizeSetupModule] Focusing() : One Pin Align Blob Search Fail");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

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
            LoggerManager.Debug($"[BlobMinSizeSetupModule] SaveProbeCardData() : Save Start");

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


            }
            catch (Exception err)
            {
                throw err;
            }
            LoggerManager.Debug($"[BlobMinSizeSetupModule] SaveProbeCardData() : Save Done");
            return serialRes;
        }
        //private void InitBackupData()
        //{
        //    LoggerManager.Debug($"[BlobMinSizeSetupModule] InitBackupData() : Make Backup Data Stert");
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
        //    }
        //    catch
        //    {
        //        //  SubModuleState.SetRecoverySate();
        //        //this.AlignModuleState = new PinHighAlignRecoveryedState(this);
        //        //LoggerManager.Debug("PinAlign : InitPinAlignResult() Error Occured");
        //        LoggerManager.Debug($": InitBackupData() ERROR");
        //    }
        //    LoggerManager.Debug($"[BlobMinSizeSetupModule] InitBackupData() : Make Backup Data Done");
        //}
        public EventCodeEnum UpdateData()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[BlobMinSizeSetupModule] UpdateData() : Data Update Start");
            //EventCodeEnum ProbeCard = EventCodeEnum.UNDEFINED;
            //EventCodeEnum serialRes = EventCodeEnum.UNDEFINED;

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
                                        oripin.PinSearchParam.MinBlobSizeX.Value = (int)pin.PinSearchParam.MinBlobSizeX.GetValue();
                                        oripin.PinSearchParam.MinBlobSizeY.Value = (int)pin.PinSearchParam.MinBlobSizeY.GetValue();
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
            LoggerManager.Debug($"[BlobMinSizeSetupModule] UpdateData() : Data Update Done");
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
            LoggerManager.Debug($"[BlobMinSizeSetupModule] Cleanup() : Cleanup Start");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Set();

                SetNodeSetupState(EnumMoudleSetupState.NONE);
                LoggerManager.Debug($"[BlobMinSizeSetupModule] Cleanup() : Blob Min Size Setup Done");
                //this.PinAligner().StopDrawPinOverlay(CurCam);
                this.PinAligner().StopDrawDutOverlay(CurCam);

                this.PinAligner().ChangeAlignKeySetupControlFlag(PinSetupMode.TIPBLOBMIN, false);

                retVal = await base.Cleanup(parameter);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            LoggerManager.Debug($"[BlobMinSizeSetupModule] Cleanup() : Cleanup Done");
            return retVal;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
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
