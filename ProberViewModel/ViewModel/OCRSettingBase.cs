using Autofac;
using LoaderControllerBase;
using LoaderParameters;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.LoaderController;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using UcDisplayPort;
using VirtualKeyboardControl;
using Vision.GraphicsContext;

namespace OCRViewModel
{
    public class OCRSettingBase : IMainScreenViewModel
    {

        readonly Guid _ViewModelGUID = new Guid("4576db4b-63f1-478a-ab94-44772306bdf1");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public ICamera CurCam { get; set; } = null;

        DrawRectangleModule OCRPosOverlay = null;
        List<DrawRectangleModule> OCRCharOverlay = new List<DrawRectangleModule>();

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private IDisplayPort _OCRDisplayPort;
        public IDisplayPort OCRDisplayPort
        {
            get { return _OCRDisplayPort; }
            set { _OCRDisplayPort = value; RaisePropertyChanged(); }
        }

        private ILoaderControllerExtension _LoaderControllerExt;
        public ILoaderControllerExtension LoaderControllerExt
        {
            get { return _LoaderControllerExt; }
            set { _LoaderControllerExt = value; RaisePropertyChanged(); }
        }

        public IStageSupervisor StageSupervisor { get; set; }


        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
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
        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                InitData();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                InitData();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            //return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);

            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsWaferOnOCR)
                {
                    WaferMoveToPACommandFunc();
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            IsWaferOnOCR = false;

            try
            {
                if (Initialized == false)
                {
                    OCRDisplayPort = new DisplayPort() { GUID = new Guid("1A62DBDB-96AD-49E4-B23B-74EFE35A7561") };

                    Array loadercamvalues = Enum.GetValues(typeof(LoaderCam));

                    foreach (var cam in this.VisionManager().CameraDescriptor.Cams)
                    {
                        for (int index = 0; index < loadercamvalues.Length; index++)
                        {
                            if (((LoaderCam)loadercamvalues.GetValue(index)).ToString() == cam.GetChannelType().ToString())
                            {
                                this.VisionManager().SetDisplayChannel(cam, OCRDisplayPort);
                                break;
                            }
                        }
                    }

                    ((DisplayPort)OCRDisplayPort).DataContext = this;

                    Binding binding = new Binding("CurCam");
                    BindingOperations.SetBinding((DisplayPort)OCRDisplayPort, DisplayPort.AssignedCamearaProperty, binding);

                    _LoaderControllerExt = this.LoaderController() as ILoaderControllerExtension;
                    StageSupervisor = this.StageSupervisor();

                    //InitData();
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

        #region ==>Properties
        private bool _IsEditEnable;
        public bool IsEditEnable
        {
            get { return _IsEditEnable; }
            set { _IsEditEnable = value; RaisePropertyChanged(); }
        }
        private double _OcrTestScore;
        public double OcrTestScore
        {
            get { return _OcrTestScore; }
            set { _OcrTestScore = value; RaisePropertyChanged(); }
        }

        private bool _OcrTestStatus;
        public bool OcrTestStatus
        {
            get { return _OcrTestStatus; }
            set { _OcrTestStatus = value; RaisePropertyChanged(); }
        }

        private bool _OcrTestCheckSum;
        public bool OcrTestCheckSum
        {
            get { return _OcrTestCheckSum; }
            set { _OcrTestCheckSum = value; RaisePropertyChanged(); }
        }

        private int _OcrRotateAngle;
        public int OcrRotateAngle
        {
            get { return _OcrRotateAngle; }
            set { _OcrRotateAngle = value; RaisePropertyChanged(); }
        }

        private int _RegionJogOption;
        public int RegionJogOption
        {
            get { return _RegionJogOption; }
            set { _RegionJogOption = value; RaisePropertyChanged(); }
        }

        private bool _OCRLotIntegrity;
        public bool OCRLotIntegrity
        {
            get { return _OCRLotIntegrity; }
            set { _OCRLotIntegrity = value; RaisePropertyChanged(); }
        }
        private bool _OCRSlotIntegrity;
        public bool OCRSlotIntegrity
        {
            get { return _OCRSlotIntegrity; }
            set { _OCRSlotIntegrity = value; RaisePropertyChanged(); }
        }
        private bool _OCRWaferIntegrity;
        public bool OCRWaferIntegrity
        {
            get { return _OCRWaferIntegrity; }
            set { _OCRWaferIntegrity = value; RaisePropertyChanged(); }
        }
        private bool _OCRLotIDFix;
        public bool OCRLotIDFix
        {
            get { return _OCRLotIDFix; }
            set { _OCRLotIDFix = value; RaisePropertyChanged(); }
        }
        private bool _OCRConfirmWaferIDPrefix;
        public bool OCRConfirmWaferIDPrefix
        {
            get { return _OCRConfirmWaferIDPrefix; }
            set { _OCRConfirmWaferIDPrefix = value; RaisePropertyChanged(); }
        }
        private bool _OCRChecksumEnable;
        public bool OCRChecksumEnable
        {
            get { return _OCRChecksumEnable; }
            set { _OCRChecksumEnable = value; RaisePropertyChanged(); }
        }
        private bool _OCRCharPosCheck;
        public bool OCRCharPosCheck
        {
            get { return _OCRCharPosCheck; }
            set { _OCRCharPosCheck = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ==>Init Data
        private OCRModuleInfo[] _OCRModules;
        public OCRModuleInfo[] OCRModules
        {
            get { return _OCRModules; }
            set { _OCRModules = value; RaisePropertyChanged(); }
        }

        private OCRModuleInfo _SelectedOCRModule;
        public OCRModuleInfo SelectedOCRModule
        {
            get { return _SelectedOCRModule; }
            set { _SelectedOCRModule = value; RaisePropertyChanged(); }
        }

        private HolderModuleInfo[] _PreAlignModules;
        public HolderModuleInfo[] PreAlignModules
        {
            get { return _PreAlignModules; }
            set { _PreAlignModules = value; RaisePropertyChanged(); }
        }

        private HolderModuleInfo _SelectedPreAlignModule;
        public HolderModuleInfo SelectedPreAlignModule
        {
            get { return _SelectedPreAlignModule; }
            set { _SelectedPreAlignModule = value; RaisePropertyChanged(); }
        }

        private LoaderDeviceParameter _DeviceParamClone;
        public LoaderDeviceParameter DeviceParamClone
        {
            get { return _DeviceParamClone; }
            set { _DeviceParamClone = value; RaisePropertyChanged(); }
        }

        private SemicsOCRDevice _OCRDevCloneParam;
        public SemicsOCRDevice OCRDevCloneParam
        {
            get { return _OCRDevCloneParam; }
            set { _OCRDevCloneParam = value; RaisePropertyChanged(); }
        }

        private ReadOCRResult _OCRResultForCalibrate = new ReadOCRResult();
        public ReadOCRResult OCRResultForCalibrate
        {
            get { return _OCRResultForCalibrate; }
            set { _OCRResultForCalibrate = value; RaisePropertyChanged(); }
        }

        //private SemicsOCRDevice _OCRDevParam;
        //public SemicsOCRDevice OCRDevParam
        //{
        //    get { return _OCRDevParam; }
        //    set { _OCRDevParam = value; RaisePropertyChanged(); }
        //}

        private SemicsOCRDefinition _OCRSysParam;
        public SemicsOCRDefinition OCRSysParam
        {
            get { return _OCRSysParam; }
            set { _OCRSysParam = value; RaisePropertyChanged(); }
        }
        #endregion

        #region ==>Commands
        private RelayCommand _ReadOCRCommand;
        public RelayCommand ReadOCRCommand
        {
            get
            {
                if (null == _ReadOCRCommand) _ReadOCRCommand = new RelayCommand(ReadOCR);
                return _ReadOCRCommand;
            }
        }
        private async void ReadOCR()
        {
            try
            {
                IVisionManager vision = this.VisionManager();
                ImageBuffer ibuf;
                ibuf = vision.LoadImageFile(@"C:\OCR\Original.bmp");

                string font_output_path = this.FileManager().GetDeviceParamFullPath(OCRDevCloneParam.OCRFontFilePath.Value, OCRDevCloneParam.OCRFontFileName.Value);

                var ocrDev = LoaderControllerExt.LoaderDeviceParam.SemicsOCRModules[0];
                OCRDevCloneParam = ocrDev.Clone() as SemicsOCRDevice;

                ReadOCRProcessingParam procParam = new ReadOCRProcessingParam();
                procParam = ConvertToOCRProcessingParam(ocrDev, 0);

                vision.ReadOCRProcessing(ibuf, procParam, font_output_path, false);

                //// vision processing

                //await Task.Run(() =>
                //{
                //    bool isInjected = MoveToOCR(true, true, OCRDevCloneParam);

                    
                //    //Wait
                //    if (isInjected)
                //    {
                //        this.LoaderController().WaitForCommandDone();
                //    }
                //});


                //// update result screen
                //UpdateSusbstrateINFO();

                //OcrTestScore
                //OcrTestStatus
                //OcrTestCheckSum
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _CalibrateCommand;
        public AsyncCommand CalibrateCommand
        {
            get
            {
                if (null == _CalibrateCommand) _CalibrateCommand = new AsyncCommand(CalibrateOCR);
                return _CalibrateCommand;
            }
        }
        private async Task CalibrateOCR()
        {
            try
            {
                IVisionManager vision = this.VisionManager();
                ImageBuffer ibuf;
                ImageBuffer ib;
                ReadOCRProcessingParam procParam = new ReadOCRProcessingParam();

                //DeviceParamClone = LoaderControllerExt.LoaderDeviceParam.Clone<LoaderDeviceParameter>();
                DeviceParamClone = LoaderControllerExt.LoaderDeviceParam;
                OCRDevCloneParam = DeviceParamClone.SemicsOCRModules[0];

                EnumProberCam camtype = this.OCRSysParam.OCRCam.Value;

                //ibuf = vision.LoadImageFile(@"C:\OCR\Original.bmp");
                ib = vision.SingleGrab(camtype,this);
                procParam = ConvertToOCRProcessingParam(OCRDevCloneParam, 0);

                //OCRDevCloneParam.OCRFontFilePath = new Element<string>();
                //OCRDevCloneParam.OCRFontFileName = new Element<string>();

                OCRDevCloneParam.OCRFontFilePath.Value = "OCR";
                OCRDevCloneParam.OCRFontFileName.Value = "Calibrated.mfo";

                string font_output_path = this.FileManager().GetDeviceParamFullPath(OCRDevCloneParam.OCRFontFilePath.Value, OCRDevCloneParam.OCRFontFileName.Value);

                OCRResultForCalibrate = vision.OcrCalibrateFontProcessing(ib, procParam, font_output_path, false);

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _SaveOCRSettingCommand;
        public RelayCommand SaveOCRSettingCommand
        {
            get
            {
                if (null == _SaveOCRSettingCommand) _SaveOCRSettingCommand = new RelayCommand(SaveOCRSetting);
                return _SaveOCRSettingCommand;
            }
        }
        private void SaveOCRSetting()
        {
            try
            {
                if (IsEditEnable == false)
                    return;

                EventCodeEnum retVal;
                //retVal = LoaderControllerExt.UpdateDeviceParam(DeviceParamClone.Clone<LoaderDeviceParameter>());
                //retVal = LoaderControllerExt.UpdateDeviceParam(DeviceParamClone);

                var loadercontroller = this.LoaderController() as ILoaderController;

                if (loadercontroller != null)
                {
                    loadercontroller.SaveDeviceParam();
                }
                else
                {

                }

                //TODO : show result
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand _EditOCRSettingCommand;
        public RelayCommand EditOCRSettingCommand
        {
            get
            {
                if (null == _EditOCRSettingCommand) _EditOCRSettingCommand = new RelayCommand(EditOCRSetting);
                return _EditOCRSettingCommand;
            }
        }

        private string _CurrSubID;
        private ModuleID _CurrPreAlignID;
        private ModuleID _CurrOcrID;
        private TransferObject _CurrSubINFO;
        public TransferObject CurrSubINFO
        {
            get { return _CurrSubINFO; }
            set { _CurrSubINFO = value; RaisePropertyChanged(); }
        }

        private void UpdateSusbstrateINFO()
        {
            try
            {
                CurrSubINFO = LoaderControllerExt.LoaderInfo.StateMap.ARMModules
                                .Where(item =>
                                item.WaferStatus == EnumSubsStatus.EXIST &&
                                item.Substrate.CurrPos == _CurrOcrID)
                                .Select(item => item.Substrate).FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async void EditOCRSetting()
        {
            try
            {
                InitData();

                if (IsEditEnable == false && SelectedOCRModule != null && SelectedPreAlignModule != null)
                {
                    //Set Transfer INFO
                    _CurrSubID = SelectedPreAlignModule.Substrate.ID.Value;
                    _CurrPreAlignID = SelectedPreAlignModule.ID;
                    _CurrOcrID = SelectedOCRModule.ID;

                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                    await Task.Run(() =>
                    {
                        //Move
                        bool isInjected = MoveToOCR(false, false);

                        //Wait
                        if (isInjected)
                        {
                            retVal = this.LoaderController().WaitForCommandDone();

                            if (retVal == EventCodeEnum.NONE)
                            {
                                InitEditSetting();

                                FlipOCRImage();
                                ILightAdmin light = this.LightAdmin();

                                if (OCRDevCloneParam.UserLightEnable.Value)
                                {
                                    light.SetLight(OCRSysParam.LightChannel1.Value, OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value);
                                    light.SetLight(OCRSysParam.LightChannel2.Value, OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value);
                                    light.SetLight(OCRSysParam.LightChannel3.Value, OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value);
                                }
                                else
                                {
                                    //light.SetContainer(SelectedOCRModule.LightChannel1,  );
                                    //light.SetContainer(SelectedOCRModule.LightChannel2.Value,  );
                                    //light.SetContainer(SelectedOCRModule.LightChannel3,  );
                                }

                            }
                        }
                    });


                    if (retVal == EventCodeEnum.NONE)
                    {
                        IsEditEnable = true;
                    }

                    IsWaferOnOCR = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

            }
        }

        private RelayCommand _EndOCRSettingCommand;
        public RelayCommand EndOCRSettingCommand
        {
            get
            {
                if (null == _EndOCRSettingCommand) _EndOCRSettingCommand = new RelayCommand(EndOCRSetting);
                return _EndOCRSettingCommand;
            }
        }
        private async void EndOCRSetting()
        {
            try
            {
                if (IsEditEnable == true)
                {
                    // Erase Overlay
                    if (CurCam != null)
                    {
                        CurCam.InDrawOverlayDisplay();
                    }
                    OCRPosOverlay = null;
                    OCRCharOverlay.Clear();

                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                    await Task.Run(() =>
                    {
                        ILightAdmin light = this.LightAdmin();
                        light.SetLight(OCRSysParam.LightChannel1.Value, 0);
                        light.SetLight(OCRSysParam.LightChannel2.Value, 0);
                        light.SetLight(OCRSysParam.LightChannel3.Value, 0);

                        //Move
                        bool isInjected = MoveToPreAlign();
                        if (isInjected)
                        {
                            retVal = this.LoaderController().WaitForCommandDone();
                        }

                    });

                    await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                    if (retVal == EventCodeEnum.NONE)
                    {
                        //TODO : Clear UI data
                        IsEditEnable = false;

                        //Reset bidning data
                        InitData();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region ==> Functions
        private void InitData()
        {
            try
            {
                //Set PreAlign 
                if (LoaderControllerExt != null)
                {
                    PreAlignModules = LoaderControllerExt
                        .LoaderInfo.StateMap.PreAlignModules
                        .Where(item => item.WaferStatus == EnumSubsStatus.EXIST).ToArray();

                    SelectedPreAlignModule = PreAlignModules.FirstOrDefault();

                    //Set Semics OCR 
                    OCRModules = LoaderControllerExt
                        .LoaderInfo.StateMap.SemicsOCRModules
                        .Where(item =>
                        item.ID.ModuleType == ModuleTypeEnum.SEMICSOCR).ToArray();

                    SelectedOCRModule = OCRModules.FirstOrDefault();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum InitEditSetting()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = InitOCRSetting();

                retVal = InitOCROverlay();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
            //=> inner methods
            EventCodeEnum InitOCRSetting()
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                try
                {
                    this.OCRSysParam = LoaderControllerExt.LoaderSystemParam.SemicsOCRModules[SelectedOCRModule.ID.Index - 1];

                    //DeviceParamClone = LoaderControllerExt.LoaderDeviceParam.Clone<LoaderDeviceParameter>();
                    DeviceParamClone = LoaderControllerExt.LoaderDeviceParam;
                    OCRDevCloneParam = DeviceParamClone.SemicsOCRModules[SelectedOCRModule.ID.Index - 1];

                    retval = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

                return retval;
            }

        }
        EventCodeEnum InitOCROverlay()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CurCam = this.VisionManager().GetCam(this.OCRSysParam.OCRCam.Value);

                if (CurCam != null)
                {
                    CurCam.InDrawOverlayDisplay();
                }
                OCRPosOverlay = null;
                OCRCharOverlay.Clear();

                this.VisionManager().SetDisplayChannel(CurCam, OCRDisplayPort);
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                OCRPosOverlay = new DrawRectangleModule(CurCam, (OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrReadRegionWidth.Value / 2), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrReadRegionHeight.Value / 2), OCRDevCloneParam.OcrReadRegionWidth.Value, OCRDevCloneParam.OcrReadRegionHeight.Value);

                for (int i = 0; i < OCRDevCloneParam.OcrMaxStringLength.Value; i++)
                {
                    OCRCharOverlay.Add(new DrawRectangleModule(CurCam, (OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeY.Value / 2) + (OCRDevCloneParam.OcrCharSizeY.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeY.Value, OCRDevCloneParam.OcrCharSizeY.Value));
                }

                CurCam.DisplayService.DrawOverlayContexts.Add(OCRPosOverlay);

                foreach (var OverlayItem in OCRCharOverlay)
                {
                    CurCam.DisplayService.DrawOverlayContexts.Add(OverlayItem);
                }

                CurCam.DrawOverlayDisplay();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #region ==> MoveToOCR
        private bool MoveToOCR(bool isPerformOCR = false, bool isOverrideDevice = false, OCRDeviceBase overrideDevice = null)
        {
            bool isInjected = false;
            //=> Find Source Substrate
            string targetSubID = _CurrSubID;

            try
            {
                //=> Find Desitination Pos
                ModuleID destPos = _CurrOcrID;

                if ((LoaderControllerExt.LoaderInfo.StateMap.PreAlignModules[0].Substrate.Size.Value != SubstrateSizeEnum.UNDEFINED) &&
                    (LoaderControllerExt.LoaderInfo.StateMap.PreAlignModules[0].Substrate.Type.Value != SubstrateTypeEnum.UNDEFINED))
                {
                    //=> Req to loader
                    if (string.IsNullOrEmpty(targetSubID) == false &&
                    destPos.ModuleType == ModuleTypeEnum.SEMICSOCR &&
                    this.LoaderController().ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        var editor = LoaderControllerExt.GetLoaderMapEditor();

                        OCRPerformOption ocrOption = null;
                        if (isPerformOCR)
                        {
                            ocrOption = new OCRPerformOption();
                            ocrOption.IsEnable.Value = true;
                            ocrOption.IsPerform.Value = true;
                        }

                        OCRDeviceOption ocrdeviceOption = null;
                        if (isOverrideDevice)
                        {
                            ocrdeviceOption = new OCRDeviceOption();
                            ocrdeviceOption.IsEnable.Value = true;
                            ocrdeviceOption.OCRDeviceBase = overrideDevice;
                        }

                        editor.EditorState.SetOCR(targetSubID, destPos, ocrOption, ocrdeviceOption);

                        LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                        cmdParam.Editor = editor;
                        isInjected = this.CommandManager().SetCommand<ILoaderMapCommand>(this, cmdParam);
                    }
                }
                else
                {
                    isInjected = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isInjected;
        }

        private bool MoveToPreAlign()
        {
            bool isInjected = false;

            try
            {
                //=> Find Source Substrate
                string targetSubID = _CurrSubID;

                //=> Find Desitination Pos
                ModuleID destPos = _CurrPreAlignID;

                if (string.IsNullOrEmpty(targetSubID) == false && destPos.ModuleType == ModuleTypeEnum.PA)
                {
                    if (this.LoaderController().ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        //=> Req to loader
                        var editor = LoaderControllerExt.GetLoaderMapEditor();

                        OCRPerformOption ocrOption = new OCRPerformOption();
                        ocrOption.IsEnable.Value = true;
                        ocrOption.IsPerform.Value = false;

                        editor.EditorState.SetOcrToPreAlign(targetSubID, destPos, ocrOption);

                        LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                        cmdParam.Editor = editor;
                        isInjected = this.CommandManager().SetCommand<ILoaderMapCommand>(this, cmdParam);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isInjected;
        }


        #endregion

        #region ==> OCR Light Control 

        private RelayCommand<object> _OCRLightControlCommand;
        public RelayCommand<object> OCRLightControlCommand
        {
            get
            {
                if (null == _OCRLightControlCommand) _OCRLightControlCommand = new RelayCommand<object>(OCRLightControl);
                return _OCRLightControlCommand;
            }
        }
        public void OCRLightControl(object param)
        {
            try
            {
                string[] operation = (param.ToString()).Split(',');
                ILightAdmin light = this.LightAdmin();
                var ocrSys = LoaderControllerExt.LoaderSystemParam.SemicsOCRModules[SelectedOCRModule.ID.Index - 1];
                //operation[0] => light channel number
                //operation[1] => 0: Down, 1: Up
                if (operation[0] == "1")
                {
                    switch (operation[1])
                    {
                        case "0":
                            if (OCRDevCloneParam.UserLightEnable.Value)
                            {
                                if (OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value > 0) { OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value--; }
                                else { OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value = 0; }
                            }
                            else
                            {
                                if (ocrSys.OcrLight1_Offset.Value > 0) { ocrSys.OcrLight1_Offset.Value--; }
                                else { ocrSys.OcrLight1_Offset.Value = 0; }
                            }
                            break;
                        case "1":
                            if (OCRDevCloneParam.UserLightEnable.Value)
                            {
                                if (OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value < 255) { OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value++; }
                                else { OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value = 255; }
                            }
                            else
                            {
                                if (ocrSys.OcrLight1_Offset.Value < 255) { ocrSys.OcrLight1_Offset.Value++; }
                                else { ocrSys.OcrLight1_Offset.Value = 255; }
                            }
                            break;
                        default:
                            break;
                    }

                    if (OCRDevCloneParam.UserLightEnable.Value)
                    {
                        light.SetLight(OCRSysParam.LightChannel1.Value, OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value);
                    }
                    else
                    {
                        light.SetLight(OCRSysParam.LightChannel1.Value, ocrSys.OcrLight1_Offset.Value);
                    }
                }
                else if (operation[0] == "2")
                {
                    switch (operation[1])
                    {
                        case "0":
                            if (OCRDevCloneParam.UserLightEnable.Value)
                            {
                                if (OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value > 0) { OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value--; }
                                else { OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value = 0; }
                            }
                            else
                            {
                                if (ocrSys.OcrLight2_Offset.Value > 0) { ocrSys.OcrLight2_Offset.Value--; }
                                else { ocrSys.OcrLight2_Offset.Value = 0; }
                            }
                            break;
                        case "1":
                            if (OCRDevCloneParam.UserLightEnable.Value)
                            {
                                if (OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value < 255) { OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value++; }
                                else { OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value = 255; }
                            }
                            else
                            {
                                if (ocrSys.OcrLight2_Offset.Value < 255) { ocrSys.OcrLight2_Offset.Value++; }
                                else { ocrSys.OcrLight2_Offset.Value = 255; }
                            }
                            break;
                        default:
                            break;
                    }

                    if (OCRDevCloneParam.UserLightEnable.Value)
                    {
                        light.SetLight(OCRSysParam.LightChannel2.Value, OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value);
                    }
                    else
                    {
                        light.SetLight(OCRSysParam.LightChannel2.Value, ocrSys.OcrLight2_Offset.Value);
                    }
                }
                else if (operation[0] == "3")
                {
                    switch (operation[1])
                    {
                        case "0":
                            if (OCRDevCloneParam.UserLightEnable.Value)
                            {
                                if (OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value > 0) { OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value--; }
                                else { OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value = 0; }
                            }
                            else
                            {
                                if (ocrSys.OcrLight3_Offset.Value > 0) { ocrSys.OcrLight3_Offset.Value--; }
                                else { ocrSys.OcrLight3_Offset.Value = 0; }
                            }
                            break;
                        case "1":
                            if (OCRDevCloneParam.UserLightEnable.Value)
                            {
                                if (OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value < 255) { OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value++; }
                                else { OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value = 255; }
                            }
                            else
                            {
                                if (ocrSys.OcrLight3_Offset.Value < 255) { ocrSys.OcrLight3_Offset.Value++; }
                                else { ocrSys.OcrLight3_Offset.Value = 255; }
                            }
                            break;
                        default:
                            break;
                    }

                    if (OCRDevCloneParam.UserLightEnable.Value)
                    {
                        light.SetLight(OCRSysParam.LightChannel3.Value, OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value);
                    }
                    else
                    {
                        light.SetLight(OCRSysParam.LightChannel3.Value, ocrSys.OcrLight3_Offset.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRLight1ValCommand;
        public RelayCommand EditOCRLight1ValCommand
        {
            get
            {
                if (null == _EditOCRLight1ValCommand) _EditOCRLight1ValCommand = new RelayCommand(EditOCRLight1Val);
                return _EditOCRLight1ValCommand;
            }
        }
        public void EditOCRLight1Val()
        {
            try
            {
                string retVal;
                ILightAdmin light = this.LightAdmin();
                var ocrSys = LoaderControllerExt.LoaderSystemParam.SemicsOCRModules[SelectedOCRModule.ID.Index - 1];

                if (OCRDevCloneParam.UserLightEnable.Value)
                {
                    retVal = (OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value).ToString();
                    retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL);
                    OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value = Convert.ToUInt16(retVal);

                    if (OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value < 0)
                    {
                        OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value = 0;
                    }
                    else if (OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value > 255)
                    {
                        OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value = 255;
                    }
                    light.SetLight(OCRSysParam.LightChannel1.Value, OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value);
                }
                else
                {
                    retVal = (ocrSys.OcrLight1_Offset).ToString();
                    retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL);
                    ocrSys.OcrLight1_Offset.Value = Convert.ToUInt16(retVal);

                    if (ocrSys.OcrLight1_Offset.Value < 0)
                    {
                        ocrSys.OcrLight1_Offset.Value = 0;
                    }
                    else if (ocrSys.OcrLight1_Offset.Value > 255)
                    {
                        ocrSys.OcrLight1_Offset.Value = 255;
                    }
                    light.SetLight(OCRSysParam.LightChannel1.Value, ocrSys.OcrLight1_Offset.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRLight2ValCommand;
        public RelayCommand EditOCRLight2ValCommand
        {
            get
            {
                if (null == _EditOCRLight2ValCommand) _EditOCRLight2ValCommand = new RelayCommand(EditOCRLight2Val);
                return _EditOCRLight2ValCommand;
            }
        }
        public void EditOCRLight2Val()
        {
            try
            {
                string retVal;
                ILightAdmin light = this.LightAdmin();
                var ocrSys = LoaderControllerExt.LoaderSystemParam.SemicsOCRModules[SelectedOCRModule.ID.Index - 1];

                if (OCRDevCloneParam.UserLightEnable.Value)
                {
                    retVal = (OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value).ToString();
                    retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL);
                    OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value = Convert.ToUInt16(retVal);

                    if (OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value < 0)
                    {
                        OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value = 0;
                    }
                    else if (OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value > 255)
                    {
                        OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value = 255;
                    }
                    light.SetLight(OCRSysParam.LightChannel2.Value, OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value);
                }
                else
                {
                    retVal = (ocrSys.OcrLight2_Offset.Value).ToString();
                    retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL);
                    ocrSys.OcrLight2_Offset.Value = Convert.ToUInt16(retVal);

                    if (ocrSys.OcrLight2_Offset.Value < 0)
                    {
                        ocrSys.OcrLight2_Offset.Value = 0;
                    }
                    else if (ocrSys.OcrLight2_Offset.Value > 255)
                    {
                        ocrSys.OcrLight2_Offset.Value = 255;
                    }
                    light.SetLight(OCRSysParam.LightChannel2.Value, ocrSys.OcrLight2_Offset.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRLight3ValCommand;
        public RelayCommand EditOCRLight3ValCommand
        {
            get
            {
                if (null == _EditOCRLight3ValCommand) _EditOCRLight3ValCommand = new RelayCommand(EditOCRLight3Val);
                return _EditOCRLight3ValCommand;
            }
        }
        public void EditOCRLight3Val()
        {
            try
            {
                string retVal;
                ILightAdmin light = this.LightAdmin();
                var ocrSys = LoaderControllerExt.LoaderSystemParam.SemicsOCRModules[SelectedOCRModule.ID.Index - 1];

                if (OCRDevCloneParam.UserLightEnable.Value)
                {
                    retVal = (OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value).ToString();
                    retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL);
                    OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value = Convert.ToUInt16(retVal);

                    if (OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value < 0)
                    {
                        OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value = 0;
                    }
                    else if (OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value > 255)
                    {
                        OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value = 255;
                    }
                    light.SetLight(OCRSysParam.LightChannel3.Value, OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value);
                }
                else
                {
                    retVal = (ocrSys.OcrLight3_Offset.Value).ToString();
                    retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL);
                    ocrSys.OcrLight3_Offset.Value = Convert.ToUInt16(retVal);

                    if (ocrSys.OcrLight3_Offset.Value < 0)
                    {
                        ocrSys.OcrLight3_Offset.Value = 0;
                    }
                    else if (ocrSys.OcrLight3_Offset.Value > 255)
                    {
                        ocrSys.OcrLight3_Offset.Value = 255;
                    }
                    light.SetLight(OCRSysParam.LightChannel3.Value, ocrSys.OcrLight3_Offset.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ==> Angle Control

        private RelayCommand _EditOcrAngleValCommand;
        public RelayCommand EditOcrAngleValCommand
        {
            get
            {
                if (null == _EditOcrAngleValCommand) _EditOcrAngleValCommand = new RelayCommand(EditOcrAngleVal);
                return _EditOcrAngleValCommand;
            }
        }
        public void EditOcrAngleVal()
        {
            try
            {
                string retVal;
                int angVal = 0;

                retVal = OcrRotateAngle.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL);

                angVal = Convert.ToInt32(retVal);
                if (angVal <= 0)
                {
                    angVal = 1;
                }
                else if (angVal >= 36000)
                {
                    angVal = 35999;
                }

                OcrRotateAngle = Convert.ToInt32(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _RotateOCRAngleCommand;
        public RelayCommand<object> RotateOCRAngleCommand
        {
            get
            {
                if (null == _RotateOCRAngleCommand) _RotateOCRAngleCommand = new RelayCommand<object>(RotateOCRAngle);
                return _RotateOCRAngleCommand;
            }
        }
        public async void RotateOCRAngle(object param)
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                // vision processing
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                await Task.Run(() =>
                {
                    //Move to Prealign
                    ILightAdmin light = this.LightAdmin();
                    light.SetLight(OCRSysParam.LightChannel1.Value, 0);
                    light.SetLight(OCRSysParam.LightChannel2.Value, 0);
                    light.SetLight(OCRSysParam.LightChannel3.Value, 0);

                    bool isInjected = MoveToPreAlign();
                    if (isInjected)
                    {
                        retVal = this.LoaderController().WaitForCommandDone();
                    }

                    //Change the Parameter
                    if ((OCRDevCloneParam.OcrVerticalFlipImage.Value == true && OCRDevCloneParam.OcrHorizontalFlipImage.Value == false) ||
                    (OCRDevCloneParam.OcrVerticalFlipImage.Value == false && OCRDevCloneParam.OcrHorizontalFlipImage.Value == true))
                    {
                        switch (param.ToString())
                        {
                            case "CW":
                                OCRDevCloneParam.OffsetV.Value += OcrRotateAngle * -1.0;
                                break;
                            case "CCW":
                                OCRDevCloneParam.OffsetV.Value += OcrRotateAngle;
                                break;
                            default:
                                break;
                        }
                    }

                    else if ((OCRDevCloneParam.OcrVerticalFlipImage.Value == true && OCRDevCloneParam.OcrHorizontalFlipImage.Value == true) ||
                    (OCRDevCloneParam.OcrVerticalFlipImage.Value == false && OCRDevCloneParam.OcrHorizontalFlipImage.Value == false))
                    {
                        switch (param.ToString())
                        {
                            case "CW":
                                OCRDevCloneParam.OffsetV.Value += OcrRotateAngle;
                                break;
                            case "CCW":
                                OCRDevCloneParam.OffsetV.Value += OcrRotateAngle * -1.0;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {

                    }
                    
                    //Move to OCR
                    isInjected = MoveToOCR(false, false);

                    //Wait
                    if (isInjected)
                    {
                        retVal = this.LoaderController().WaitForCommandDone();

                        OCRPosOverlay.SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrReadRegionWidth.Value / 2), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrReadRegionHeight.Value / 2), OCRDevCloneParam.OcrReadRegionWidth.Value, OCRDevCloneParam.OcrReadRegionHeight.Value);

                        for (int i = 0; i < OCRCharOverlay.Count; i++)
                        {
                            OCRCharOverlay[i].SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeX.Value / 2) + (OCRDevCloneParam.OcrCharSizeX.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeX.Value, OCRDevCloneParam.OcrCharSizeY.Value);
                        }

                        CurCam.DisplayService.DrawOverlayContexts.Add(OCRPosOverlay);

                        foreach (var OverlayItem in OCRCharOverlay)
                        {
                            CurCam.DisplayService.DrawOverlayContexts.Add(OverlayItem);
                        }
                    }
                    //bool isInjected = MoveToOCR(false, true, OCRDevCloneParam);

                    ////Wait
                    //if (isInjected)
                    //{
                    //    this.LoaderController().WaitForCommandDone();
                    //}
                });

                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                IsWaferOnOCR = true;

                // update result screen
                UpdateSusbstrateINFO();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> Rigion Jog Control
        private RelayCommand<object> _RegionOptionSelectCommand;
        public RelayCommand<object> RegionOptionSelectCommand
        {
            get
            {
                if (null == _RegionOptionSelectCommand) _RegionOptionSelectCommand = new RelayCommand<object>(RegionOptionSelect);
                return _RegionOptionSelectCommand;
            }
        }
        public void RegionOptionSelect(object param)
        {
            try
            {
                int radioBtnIdx;
                int.TryParse(param.ToString(), out radioBtnIdx);
                RegionJogOption = radioBtnIdx;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _RegionJogFuncSelectCommand;
        public RelayCommand<object> RegionJogFuncSelectCommand
        {
            get
            {
                if (null == _RegionJogFuncSelectCommand) _RegionJogFuncSelectCommand = new RelayCommand<object>(RegionJogFuncSelect);
                return _RegionJogFuncSelectCommand;
            }
        }
        private void RegionJogFuncSelect(object param)
        {
            try
            {
                if (RegionJogOption == 0)
                {
                    EditOCRPosition(param);
                }
                else if (RegionJogOption == 1)
                {
                    EditOCRSize(param);
                }
                else if (RegionJogOption == 2)
                {
                    EditCharPosition(param);
                }
                else if (RegionJogOption == 3)
                {
                    EditCharSize(param);
                }
                else if (RegionJogOption == 4)
                {
                    EditCharSpacing(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void EditOCRPosition(object param)
        {
            try
            {
                switch (param.ToString())
                {
                    case "UP":
                        if (OCRDevCloneParam.OcrReadRegionPosY.Value > 0)
                        {
                            OCRDevCloneParam.OcrReadRegionPosY.Value--;
                        }
                        break;
                    case "DN":
                        if (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrReadRegionHeight.Value < 480)
                        {
                            OCRDevCloneParam.OcrReadRegionPosY.Value++;
                        }
                        break;
                    case "LF":
                        if (OCRDevCloneParam.OcrReadRegionPosX.Value > 0)
                        {
                            OCRDevCloneParam.OcrReadRegionPosX.Value--;
                        }
                        break;
                    case "RT":
                        if (OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrReadRegionWidth.Value < 480)
                        {
                            OCRDevCloneParam.OcrReadRegionPosX.Value++;
                        }
                        break;
                    default:
                        break;
                }

                OCRPosOverlay.SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrReadRegionWidth.Value / 2), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrReadRegionHeight.Value / 2), OCRDevCloneParam.OcrReadRegionWidth.Value, OCRDevCloneParam.OcrReadRegionHeight.Value);

                //for (int i = 0; i < OCRCharOverlay.Count; i++)
                //{
                //    OCRCharOverlay[i].SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeY.Value / 2) + (OCRDevCloneParam.OcrCharSizeY.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeY.Value, OCRDevCloneParam.OcrCharSizeY.Value);
                //}

                for (int i = 0; i < OCRCharOverlay.Count; i++)
                {
                    OCRCharOverlay[i].SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeX.Value / 2) + (OCRDevCloneParam.OcrCharSizeX.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeX.Value, OCRDevCloneParam.OcrCharSizeY.Value);
                }

                CurCam.DisplayService.DrawOverlayContexts.Add(OCRPosOverlay);

                foreach (var OverlayItem in OCRCharOverlay)
                {
                    CurCam.DisplayService.DrawOverlayContexts.Add(OverlayItem);
                }

                // TODO : CHECK
                //CurCam.DrawOverlayDisplay();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void EditOCRSize(object param)
        {
            try
            {
                switch (param.ToString())
                {
                    case "UP":
                        if (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrReadRegionHeight.Value < 480)
                        {
                            OCRDevCloneParam.OcrReadRegionHeight.Value++;
                        }
                        break;
                    case "DN":
                        if (OCRDevCloneParam.OcrReadRegionHeight.Value > OCRDevCloneParam.OcrCharSizeY.Value)
                        {
                            OCRDevCloneParam.OcrReadRegionHeight.Value--;
                        }
                        break;
                    case "LF":
                        if (OCRDevCloneParam.OcrReadRegionWidth.Value > ((OCRDevCloneParam.OcrCharSizeX.Value * OCRDevCloneParam.OcrMaxStringLength.Value) + (OCRDevCloneParam.OcrCharSpacing.Value * OCRDevCloneParam.OcrMaxStringLength.Value)))
                        {
                            OCRDevCloneParam.OcrReadRegionWidth.Value--;
                        }
                        break;
                    case "RT":
                        if (OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrReadRegionWidth.Value < 480)
                        {
                            OCRDevCloneParam.OcrReadRegionWidth.Value++;
                        }
                        break;
                    default:
                        break;
                }

                OCRPosOverlay.SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrReadRegionWidth.Value / 2), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrReadRegionHeight.Value / 2), OCRDevCloneParam.OcrReadRegionWidth.Value, OCRDevCloneParam.OcrReadRegionHeight.Value);

                //for (int i = 0; i < OCRCharOverlay.Count; i++)
                //{
                //    OCRCharOverlay[i].SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeY.Value / 2) + (OCRDevCloneParam.OcrCharSizeY.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeY.Value, OCRDevCloneParam.OcrCharSizeY.Value);
                //}

                for (int i = 0; i < OCRCharOverlay.Count; i++)
                {
                    OCRCharOverlay[i].SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeX.Value / 2) + (OCRDevCloneParam.OcrCharSizeX.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeX.Value, OCRDevCloneParam.OcrCharSizeY.Value);
                }

                CurCam.DisplayService.DrawOverlayContexts.Add(OCRPosOverlay);

                foreach (var OverlayItem in OCRCharOverlay)
                {
                    CurCam.DisplayService.DrawOverlayContexts.Add(OverlayItem);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void EditCharPosition(object param)
        {
            try
            {
                switch (param.ToString())
                {
                    case "UP":
                        if (OCRDevCloneParam.OcrCharPosY.Value > 0)
                        {
                            OCRDevCloneParam.OcrCharPosY.Value--;
                        }
                        //
                        break;
                    case "DN":
                        if ((OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrReadRegionHeight.Value) > (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value))
                        {
                            OCRDevCloneParam.OcrCharPosY.Value++;
                        }
                        //
                        break;
                    case "LF":
                        if (OCRDevCloneParam.OcrCharPosX.Value > 0)
                        {
                            OCRDevCloneParam.OcrCharPosX.Value--;
                        }
                        break;
                    case "RT":
                        if ((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrReadRegionWidth.Value) + 5 > (OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeX.Value * OCRDevCloneParam.OcrMaxStringLength.Value) + (OCRDevCloneParam.OcrCharSpacing.Value * OCRDevCloneParam.OcrMaxStringLength.Value)))
                        {
                            OCRDevCloneParam.OcrCharPosX.Value++;
                        }
                        break;
                    default:
                        break;
                }

                //for (int i = 0; i < OCRCharOverlay.Count; i++)
                //{
                //    OCRCharOverlay[i].SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeY.Value / 2) + (OCRDevCloneParam.OcrCharSizeY.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeY.Value, OCRDevCloneParam.OcrCharSizeY.Value);
                //}

                for (int i = 0; i < OCRCharOverlay.Count; i++)
                {
                    OCRCharOverlay[i].SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeX.Value / 2) + (OCRDevCloneParam.OcrCharSizeX.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeX.Value, OCRDevCloneParam.OcrCharSizeY.Value);
                }

                CurCam.DisplayService.DrawOverlayContexts.Add(OCRPosOverlay);

                foreach (var OverlayItem in OCRCharOverlay)
                {
                    CurCam.DisplayService.DrawOverlayContexts.Add(OverlayItem);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void EditCharSize(object param)
        {
            try
            {
                switch (param.ToString())
                {
                    case "UP":
                        if ((OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrReadRegionHeight.Value) > (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value))
                        {
                            OCRDevCloneParam.OcrCharSizeY.Value = OCRDevCloneParam.OcrCharSizeY.Value + 0.1;
                        }
                        break;
                    case "DN":
                        if (OCRDevCloneParam.OcrCharSizeY.Value > 1)
                        {
                            OCRDevCloneParam.OcrCharSizeY.Value = OCRDevCloneParam.OcrCharSizeY.Value - 0.1;
                        }
                        break;
                    case "LF":
                        if (OCRDevCloneParam.OcrCharSizeX.Value > 1)
                        {
                            OCRDevCloneParam.OcrCharSizeX.Value = OCRDevCloneParam.OcrCharSizeX.Value - 0.1;
                        }
                        break;
                    case "RT":
                        if ((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrReadRegionWidth.Value) > (OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeX.Value * OCRDevCloneParam.OcrMaxStringLength.Value) + (OCRDevCloneParam.OcrCharSpacing.Value * OCRDevCloneParam.OcrMaxStringLength.Value)))
                        {
                            OCRDevCloneParam.OcrCharSizeX.Value = OCRDevCloneParam.OcrCharSizeX.Value + 0.1;
                        }
                        break;
                    default:
                        break;
                }

                for (int i = 0; i < OCRCharOverlay.Count; i++)
                {
                    OCRCharOverlay[i].SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeX.Value / 2) + (OCRDevCloneParam.OcrCharSizeX.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeX.Value, OCRDevCloneParam.OcrCharSizeY.Value);
                }

                CurCam.DisplayService.DrawOverlayContexts.Add(OCRPosOverlay);

                foreach (var OverlayItem in OCRCharOverlay)
                {
                    CurCam.DisplayService.DrawOverlayContexts.Add(OverlayItem);
                }
            }
            //try
            //{
            //    switch (param.ToString())
            //    {
            //        case "UP":
            //            if ((OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrReadRegionHeight.Value) > (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value))
            //            {
            //                OCRDevCloneParam.OcrCharSizeY.Value = OCRDevCloneParam.OcrCharSizeY.Value + 0.1;
            //            }
            //            break;
            //        case "DN":
            //            if (OCRDevCloneParam.OcrCharSizeY.Value > 1)
            //            {
            //                OCRDevCloneParam.OcrCharSizeY.Value = OCRDevCloneParam.OcrCharSizeY.Value - 0.1;
            //            }
            //            break;
            //        case "LF":
            //            if (OCRDevCloneParam.OcrCharSizeY.Value > 1)
            //            {
            //                OCRDevCloneParam.OcrCharSizeY.Value = OCRDevCloneParam.OcrCharSizeY.Value - 0.1;
            //            }
            //            break;
            //        case "RT":
            //            if ((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrReadRegionWidth.Value) > (OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeY.Value * OCRDevCloneParam.OcrMaxStringLength.Value) + (OCRDevCloneParam.OcrCharSpacing.Value * OCRDevCloneParam.OcrMaxStringLength.Value)))
            //            {
            //                OCRDevCloneParam.OcrCharSizeY.Value = OCRDevCloneParam.OcrCharSizeY.Value + 0.1;
            //            }
            //            break;
            //        default:
            //            break;
            //    }

            //    for (int i = 0; i < OCRCharOverlay.Count; i++)
            //    {
            //        OCRCharOverlay[i].SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeY.Value / 2) + (OCRDevCloneParam.OcrCharSizeY.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeY.Value, OCRDevCloneParam.OcrCharSizeY.Value);
            //    }

            //    CurCam.DisplayService.DrawOverlayContexts.Add(OCRPosOverlay);

            //    foreach (var OverlayItem in OCRCharOverlay)
            //    {
            //        CurCam.DisplayService.DrawOverlayContexts.Add(OverlayItem);
            //    }
            //}
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void EditCharSpacing(object param)
        {
            try
            {
                switch (param.ToString())
                {
                    case "UP":
                    case "RT":
                        if ((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrReadRegionWidth.Value) + 10 > (OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeX.Value * OCRDevCloneParam.OcrMaxStringLength.Value) + (OCRDevCloneParam.OcrCharSpacing.Value * OCRDevCloneParam.OcrMaxStringLength.Value)))
                        {
                            OCRDevCloneParam.OcrCharSpacing.Value = OCRDevCloneParam.OcrCharSpacing.Value + 0.1;
                        }
                        break;
                    case "DN":
                    case "LF":
                        if (OCRDevCloneParam.OcrCharSpacing.Value > 0)
                        {
                            OCRDevCloneParam.OcrCharSpacing.Value = OCRDevCloneParam.OcrCharSpacing.Value - 0.1;
                        }
                        break;
                    default:
                        break;
                }

                //for (int i = 0; i < OCRCharOverlay.Count; i++)
                //{
                //    OCRCharOverlay[i].SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeY.Value / 2) + (OCRDevCloneParam.OcrCharSizeY.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeY.Value, OCRDevCloneParam.OcrCharSizeY.Value);
                //}

                for (int i = 0; i < OCRCharOverlay.Count; i++)
                {
                    OCRCharOverlay[i].SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeX.Value / 2) + (OCRDevCloneParam.OcrCharSizeX.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeX.Value, OCRDevCloneParam.OcrCharSizeY.Value);
                }

                CurCam.DisplayService.DrawOverlayContexts.Add(OCRPosOverlay);

                foreach (var OverlayItem in OCRCharOverlay)
                {
                    CurCam.DisplayService.DrawOverlayContexts.Add(OverlayItem);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ==> Arm Jog Control
        private RelayCommand<object> _MoveOCRWaferPosCommand;
        public RelayCommand<object> MoveOCRWaferPosCommand
        {
            get
            {
                if (null == _MoveOCRWaferPosCommand) _MoveOCRWaferPosCommand = new RelayCommand<object>(MoveOCRWaferPos);
                return _MoveOCRWaferPosCommand;
            }
        }
        public void MoveOCRWaferPos(object param)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //this.LoaderController().OCRUaxisRelMove()
                //this.LoaderController().OCRWaxisRelMove

                bool isUAxisRelMove = false;
                bool isWAxisRelMove = false;

                double U_STEP_DIST = 500;
                double W_STEP_DIST = 5;
                double value = 0;

                switch (param.ToString())
                {

                    case "LF":
                        isUAxisRelMove = true;

                        if (OCRDevCloneParam.OcrHorizontalFlipImage.Value == true)
                        {
                            value = U_STEP_DIST;
                        }
                        else
                        {
                            value = -U_STEP_DIST;
                        }

                        break;
                    case "RT":
                        isUAxisRelMove = true;

                        if (OCRDevCloneParam.OcrHorizontalFlipImage.Value == true)
                        {
                            value = -U_STEP_DIST;
                        }
                        else
                        {
                            value = +U_STEP_DIST;
                        }

                        break;
                    case "UP":
                        isWAxisRelMove = true;

                        if (OCRDevCloneParam.OcrVerticalFlipImage.Value == true)
                        {
                            value = -W_STEP_DIST;
                        }
                        else
                        {
                            value = +W_STEP_DIST;
                        }

                        //if (OCRDevCloneParam.OcrFlipImage.Value == true)
                        //{
                        //    value = W_STEP_DIST;
                        //}
                        //else
                        //{
                        //    value = -W_STEP_DIST;
                        //}
                        break;
                    case "DN":
                        isWAxisRelMove = true;

                        if (OCRDevCloneParam.OcrVerticalFlipImage.Value == true)
                        {
                            value = W_STEP_DIST;
                        }
                        else
                        {
                            value = -W_STEP_DIST;
                        }

                        //if (OCRDevCloneParam.OcrFlipImage.Value == true)
                        //{
                        //    value = -W_STEP_DIST;
                        //}
                        //else
                        //{
                        //    value = W_STEP_DIST;
                        //}
                        break;
                    default:
                        break;
                }


                if (isUAxisRelMove)
                {
                    retVal = LoaderControllerExt.OCRUaxisRelMove(value);

                    OCRDevCloneParam.OffsetU.Value += value;
                }
                else if (isWAxisRelMove)
                {
                    retVal = LoaderControllerExt.OCRWaxisRelMove(value);

                    OCRDevCloneParam.OffsetW.Value += value;
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private RelayCommand _EditOCRSampleStringCommand;
        public RelayCommand EditOCRSampleStringCommand
        {
            get
            {
                if (null == _EditOCRSampleStringCommand) _EditOCRSampleStringCommand = new RelayCommand(EditOCRSampleString);
                return _EditOCRSampleStringCommand;
            }
        }
        public void EditOCRSampleString()
        {
            try
            {
                OCRDevCloneParam.OCRParamTables[0].OcrSampleString.Value = VirtualKeyboard.Show(OCRDevCloneParam.OCRParamTables[0].OcrSampleString.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRConstraintsCommmand;
        public RelayCommand EditOCRConstraintsCommmand
        {
            get
            {
                if (null == _EditOCRConstraintsCommmand) _EditOCRConstraintsCommmand = new RelayCommand(EditOCRConstraints);
                return _EditOCRConstraintsCommmand;
            }
        }
        public void EditOCRConstraints()
        {
            try
            {
                OCRDevCloneParam.OCRParamTables[0].OcrConstraint.Value = VirtualKeyboard.Show(OCRDevCloneParam.OCRParamTables[0].OcrConstraint.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRStringAcceptanceCommand;
        public RelayCommand EditOCRStringAcceptanceCommand
        {
            get
            {
                if (null == _EditOCRStringAcceptanceCommand) _EditOCRStringAcceptanceCommand = new RelayCommand(EditOCRStringAcceptance);
                return _EditOCRStringAcceptanceCommand;
            }
        }
        public void EditOCRStringAcceptance()
        {
            try
            {
                string retVal;

                retVal = OCRDevCloneParam.OCRParamTables[0].OcrStrAcceptance.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1, 3);
                OCRDevCloneParam.OCRParamTables[0].OcrStrAcceptance.Value = Convert.ToInt32(retVal);

                if (OCRDevCloneParam.OCRParamTables[0].OcrStrAcceptance.Value > 100)
                {
                    OCRDevCloneParam.OCRParamTables[0].OcrStrAcceptance.Value = 100;
                }
                else if (OCRDevCloneParam.OCRParamTables[0].OcrStrAcceptance.Value < 0)
                {
                    OCRDevCloneParam.OCRParamTables[0].OcrStrAcceptance.Value = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRCharAcceptanceCommand;
        public RelayCommand EditOCRCharAcceptanceCommand
        {
            get
            {
                if (null == _EditOCRCharAcceptanceCommand) _EditOCRCharAcceptanceCommand = new RelayCommand(EditOCRCharAcceptance);
                return _EditOCRCharAcceptanceCommand;
            }
        }
        public void EditOCRCharAcceptance()
        {
            try
            {
                string retVal;

                retVal = OCRDevCloneParam.OCRParamTables[0].OcrCharAcceptance.Value.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1, 3);
                OCRDevCloneParam.OCRParamTables[0].OcrCharAcceptance.Value = Convert.ToInt32(retVal);

                if (OCRDevCloneParam.OCRParamTables[0].OcrCharAcceptance.Value > 100)
                {
                    OCRDevCloneParam.OCRParamTables[0].OcrCharAcceptance.Value = 100;
                }
                else if (OCRDevCloneParam.OCRParamTables[0].OcrCharAcceptance.Value < 0)
                {
                    OCRDevCloneParam.OCRParamTables[0].OcrCharAcceptance.Value = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRCalStringAcceptanceCommand;
        public RelayCommand EditOCRCalStringAcceptanceCommand
        {
            get
            {
                if (null == _EditOCRCalStringAcceptanceCommand) _EditOCRCalStringAcceptanceCommand = new RelayCommand(EditOCRCalStringAcceptance);
                return _EditOCRCalStringAcceptanceCommand;
            }
        }
        public void EditOCRCalStringAcceptance()
        {
            try
            {
                string retVal;

                retVal = OCRDevCloneParam.OCRParamTables[0].OcrCalStrAcceptance.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1, 3);
                OCRDevCloneParam.OCRParamTables[0].OcrCalStrAcceptance.Value = Convert.ToInt32(retVal);

                if (OCRDevCloneParam.OCRParamTables[0].OcrCalStrAcceptance.Value > 100)
                {
                    OCRDevCloneParam.OCRParamTables[0].OcrCalStrAcceptance.Value = 100;
                }
                else if (OCRDevCloneParam.OCRParamTables[0].OcrCalStrAcceptance.Value < 0)
                {
                    OCRDevCloneParam.OCRParamTables[0].OcrCalStrAcceptance.Value = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRCalCharAcceptanceCommand;
        public RelayCommand EditOCRCalCharAcceptanceCommand
        {
            get
            {
                if (null == _EditOCRCalCharAcceptanceCommand) _EditOCRCalCharAcceptanceCommand = new RelayCommand(EditOCRCalCharAcceptance);
                return _EditOCRCalCharAcceptanceCommand;
            }
        }
        public void EditOCRCalCharAcceptance()
        {
            try
            {
                string retVal;

                retVal = OCRDevCloneParam.OCRParamTables[0].OcrCalCharAcceptance.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1, 3);
                OCRDevCloneParam.OCRParamTables[0].OcrCalCharAcceptance.Value = Convert.ToInt32(retVal);

                if (OCRDevCloneParam.OCRParamTables[0].OcrCalCharAcceptance.Value > 100)
                {
                    OCRDevCloneParam.OCRParamTables[0].OcrCalCharAcceptance.Value = 100;
                }
                else if (OCRDevCloneParam.OCRParamTables[0].OcrCalCharAcceptance.Value < 0)
                {
                    OCRDevCloneParam.OCRParamTables[0].OcrCalCharAcceptance.Value = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRStringLengthCommand;
        public RelayCommand EditOCRStringLengthCommand
        {
            get
            {
                if (null == _EditOCRStringLengthCommand) _EditOCRStringLengthCommand = new RelayCommand(EditOCRStringLength);
                return _EditOCRStringLengthCommand;
            }
        }
        public void EditOCRStringLength()
        {
            try
            {
                string retVal;

                retVal = OCRDevCloneParam.OcrMaxStringLength.Value.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1, 2);
                OCRDevCloneParam.OcrMaxStringLength.Value = Convert.ToInt32(retVal);

                OCRCharOverlay.Clear();
                OCRPosOverlay = new DrawRectangleModule((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrReadRegionWidth.Value / 2), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrReadRegionHeight.Value / 2), OCRDevCloneParam.OcrReadRegionWidth.Value, OCRDevCloneParam.OcrReadRegionHeight.Value);

                for (int i = 0; i < OCRDevCloneParam.OcrMaxStringLength.Value; i++)
                {
                    OCRCharOverlay.Add(new DrawRectangleModule((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeX.Value / 2) + (OCRDevCloneParam.OcrCharSizeX.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeX.Value, OCRDevCloneParam.OcrCharSizeX.Value));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRCharSizeXCommand;
        public RelayCommand EditOCRCharSizeXCommand
        {
            get
            {
                if (null == _EditOCRCharSizeXCommand) _EditOCRCharSizeXCommand = new RelayCommand(EditOCRCharSizeX);
                return _EditOCRCharSizeXCommand;
            }
        }
        public void EditOCRCharSizeX()
        {
            try
            {
                string retVal;
                double MaxSize;

                MaxSize = (OCRDevCloneParam.OcrReadRegionWidth.Value - OCRDevCloneParam.OcrCharPosX.Value - (OCRDevCloneParam.OcrCharSpacing.Value * OCRDevCloneParam.OcrMaxStringLength.Value)) / OCRDevCloneParam.OcrMaxStringLength.Value;

                retVal = OCRDevCloneParam.OcrCharSizeY.Value.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1);
                OCRDevCloneParam.OcrCharSizeY.Value = Convert.ToDouble(retVal);

                if (MaxSize < OCRDevCloneParam.OcrCharSizeY.Value)
                {
                    OCRDevCloneParam.OcrCharSizeY.Value = MaxSize;
                }

                for (int i = 0; i < OCRCharOverlay.Count; i++)
                {
                    OCRCharOverlay[i].SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeY.Value / 2) + (OCRDevCloneParam.OcrCharSizeY.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeY.Value, OCRDevCloneParam.OcrCharSizeY.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRCharSizeYCommand;
        public RelayCommand EditOCRCharSizeYCommand
        {
            get
            {
                if (null == _EditOCRCharSizeYCommand) _EditOCRCharSizeYCommand = new RelayCommand(EditOCRCharSizeY);
                return _EditOCRCharSizeYCommand;
            }
        }
        public void EditOCRCharSizeY()
        {
            try
            {
                string retVal;
                double MaxSize;

                MaxSize = OCRDevCloneParam.OcrReadRegionHeight.Value - OCRDevCloneParam.OcrCharPosY.Value;

                retVal = OCRDevCloneParam.OcrCharSizeY.Value.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1);
                OCRDevCloneParam.OcrCharSizeY.Value = Convert.ToDouble(retVal);

                if (MaxSize < OCRDevCloneParam.OcrCharSizeY.Value)
                {
                    OCRDevCloneParam.OcrCharSizeY.Value = MaxSize;
                }

                for (int i = 0; i < OCRCharOverlay.Count; i++)
                {
                    OCRCharOverlay[i].SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeY.Value / 2) + (OCRDevCloneParam.OcrCharSizeY.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeY.Value, OCRDevCloneParam.OcrCharSizeY.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRCharSpacingCommand;
        public RelayCommand EditOCRCharSpacingCommand
        {
            get
            {
                if (null == _EditOCRCharSpacingCommand) _EditOCRCharSpacingCommand = new RelayCommand(EditOCRCharSpacing);
                return _EditOCRCharSpacingCommand;
            }
        }
        public void EditOCRCharSpacing()
        {
            try
            {
                string retVal;
                double MaxSize;

                MaxSize = (OCRDevCloneParam.OcrReadRegionWidth.Value - OCRDevCloneParam.OcrCharPosX.Value - (OCRDevCloneParam.OcrCharSizeY.Value * OCRDevCloneParam.OcrMaxStringLength.Value)) / OCRDevCloneParam.OcrMaxStringLength.Value;

                retVal = OCRDevCloneParam.OcrCharSpacing.Value.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1);
                OCRDevCloneParam.OcrCharSpacing.Value = Convert.ToDouble(retVal);

                if (MaxSize < OCRDevCloneParam.OcrCharSpacing.Value)
                {
                    OCRDevCloneParam.OcrCharSpacing.Value = MaxSize;
                }

                for (int i = 0; i < OCRCharOverlay.Count; i++)
                {
                    OCRCharOverlay[i].SetParameter((OCRDevCloneParam.OcrReadRegionPosX.Value + OCRDevCloneParam.OcrCharPosX.Value + (OCRDevCloneParam.OcrCharSizeY.Value / 2) + (OCRDevCloneParam.OcrCharSizeY.Value * i) + (OCRDevCloneParam.OcrCharSpacing.Value * i)), (OCRDevCloneParam.OcrReadRegionPosY.Value + OCRDevCloneParam.OcrCharPosY.Value + OCRDevCloneParam.OcrCharSizeY.Value / 2), OCRDevCloneParam.OcrCharSizeY.Value, OCRDevCloneParam.OcrCharSizeY.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRCalMinXCommand;
        public RelayCommand EditOCRCalMinXCommand
        {
            get
            {
                if (null == _EditOCRCalMinXCommand) _EditOCRCalMinXCommand = new RelayCommand(EditOCRCalMinX);
                return _EditOCRCalMinXCommand;
            }
        }
        public void EditOCRCalMinX()
        {
            try
            {
                string retVal;

                retVal = OCRDevCloneParam.OcrCalibrateMinX.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1);
                OCRDevCloneParam.OcrCalibrateMinX.Value = Convert.ToDouble(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRCalMaxXCommand;
        public RelayCommand EditOCRCalMaxXCommand
        {
            get
            {
                if (null == _EditOCRCalMaxXCommand) _EditOCRCalMaxXCommand = new RelayCommand(EditOCRCalMaxX);
                return _EditOCRCalMaxXCommand;
            }
        }
        public void EditOCRCalMaxX()
        {
            try
            {
                string retVal;

                retVal = OCRDevCloneParam.OcrCalibrateMaxX.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1);
                OCRDevCloneParam.OcrCalibrateMaxX.Value = Convert.ToDouble(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRCalMinYCommand;
        public RelayCommand EditOCRCalMinYCommand
        {
            get
            {
                if (null == _EditOCRCalMinYCommand) _EditOCRCalMinYCommand = new RelayCommand(EditOCRCalMinY);
                return _EditOCRCalMinYCommand;
            }
        }
        public void EditOCRCalMinY()
        {
            try
            {
                string retVal;

                retVal = OCRDevCloneParam.OcrCalibrateMinY.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1);
                OCRDevCloneParam.OcrCalibrateMinY.Value = Convert.ToDouble(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRCalMaxYCommand;
        public RelayCommand EditOCRCalMaxYCommand
        {
            get
            {
                if (null == _EditOCRCalMaxYCommand) _EditOCRCalMaxYCommand = new RelayCommand(EditOCRCalMaxY);
                return _EditOCRCalMaxYCommand;
            }
        }
        public void EditOCRCalMaxY()
        {
            try
            {
                string retVal;

                retVal = OCRDevCloneParam.OcrCalibrateMaxY.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1);
                OCRDevCloneParam.OcrCalibrateMaxY.Value = Convert.ToDouble(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOcrCalXOffsetCommand;
        public RelayCommand EditOcrCalXOffsetCommand
        {
            get
            {
                if (null == _EditOcrCalXOffsetCommand) _EditOcrCalXOffsetCommand = new RelayCommand(EditOcrCalXOffset);
                return _EditOcrCalXOffsetCommand;
            }
        }
        public void EditOcrCalXOffset()
        {
            try
            {
                string retVal;

                retVal = OCRDevCloneParam.OcrCalibrateXOffset.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1);
                while (true)
                {
                    if (Convert.ToInt32(retVal) > 5)
                    {
                        retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1);
                    }
                    else
                    {
                        break;
                    }
                }
                OCRDevCloneParam.OcrCalibrateMaxX.Value = OCRDevCloneParam.OcrCharSizeX.Value + Convert.ToDouble(retVal);
                OCRDevCloneParam.OcrCalibrateMinX.Value = OCRDevCloneParam.OcrCharSizeX.Value - Convert.ToDouble(retVal);
                OCRDevCloneParam.OcrCalibrateXOffset.Value = Convert.ToDouble(retVal);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOcrCalYOffsetCommand;
        public RelayCommand EditOcrCalYOffsetCommand
        {
            get
            {
                if (null == _EditOcrCalYOffsetCommand) _EditOcrCalYOffsetCommand = new RelayCommand(EditOcrCalYOffset);
                return _EditOcrCalYOffsetCommand;
            }
        }
        public void EditOcrCalYOffset()
        {
            try
            {
                string retVal;

                retVal = OCRDevCloneParam.OcrCalibrateYOffset.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1);
                while (true)
                {
                    if (Convert.ToInt32(retVal) > 5)
                    {
                        retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1);
                    }
                    else
                    {
                        break;
                    }
                }
                OCRDevCloneParam.OcrCalibrateMaxY.Value = OCRDevCloneParam.OcrCharSizeY.Value + Convert.ToDouble(retVal);
                OCRDevCloneParam.OcrCalibrateMinY.Value = OCRDevCloneParam.OcrCharSizeY.Value - Convert.ToDouble(retVal);
                OCRDevCloneParam.OcrCalibrateYOffset.Value = Convert.ToDouble(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRCalStepXCommand;
        public RelayCommand EditOCRCalStepXCommand
        {
            get
            {
                if (null == _EditOCRCalStepXCommand) _EditOCRCalStepXCommand = new RelayCommand(EditOCRCalStepX);
                return _EditOCRCalStepXCommand;
            }
        }
        public void EditOCRCalStepX()
        {
            try
            {
                string retVal;

                retVal = OCRDevCloneParam.OcrCalibrateStepX.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1);
                OCRDevCloneParam.OcrCalibrateStepX.Value = Convert.ToDouble(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOCRCalStepYCommand;
        public RelayCommand EditOCRCalStepYCommand
        {
            get
            {
                if (null == _EditOCRCalStepYCommand) _EditOCRCalStepYCommand = new RelayCommand(EditOCRCalStepY);
                return _EditOCRCalStepYCommand;
            }
        }
        public void EditOCRCalStepY()
        {
            try
            {
                string retVal;

                retVal = OCRDevCloneParam.OcrCalibrateStepY.ToString();
                retVal = VirtualKeyboard.Show(retVal, KB_TYPE.DECIMAL, 1);
                OCRDevCloneParam.OcrCalibrateStepY.Value = Convert.ToDouble(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand _OCRCalibrateTestCommand;
        public RelayCommand OCRCalibrateTestCommand
        {
            get
            {
                if (null == _OCRCalibrateTestCommand) _OCRCalibrateTestCommand = new RelayCommand(OCRCalibrateTest);
                return _OCRCalibrateTestCommand;
            }
        }
        public void OCRCalibrateTest()
        {
            // vision processing
        }


        private RelayCommand _EditOCRRetryTableCommand;
        public RelayCommand EditOCRRetryTableCommand
        {
            get
            {
                if (null == _EditOCRRetryTableCommand) _EditOCRRetryTableCommand = new RelayCommand(EditOCRRetryTable);
                return _EditOCRRetryTableCommand;
            }
        }
        public void EditOCRRetryTable()
        {
            //
        }


        private RelayCommand _FlipOCRImageCommand;
        public RelayCommand FlipOCRImageCommand
        {
            get
            {
                if (null == _FlipOCRImageCommand) _FlipOCRImageCommand = new RelayCommand(FlipOCRImage);
                return _FlipOCRImageCommand;
            }
        }
        public void FlipOCRImage()
        {
            try
            {
                this.VisionManager().StopGrab(CurCam.GetChannelType());

                //Vertical Flip
                if (OCRDevCloneParam.OcrVerticalFlipImage.Value == true)
                {
                    CurCam.SetVerticalFlip(FlipEnum.FLIP);
                    IVisionManager visionManager = this.VisionManager();
                    visionManager.SettingGrab(CurCam.GetChannelType());
                }
                else
                {
                    CurCam.SetVerticalFlip(FlipEnum.NONE);
                    IVisionManager visionManager = this.VisionManager();
                    visionManager.SettingGrab(CurCam.GetChannelType());
                }

                //Horizontal Flip
                if (OCRDevCloneParam.OcrHorizontalFlipImage.Value == true)
                {
                    CurCam.SetHorizontalFlip(FlipEnum.FLIP);
                    IVisionManager visionManager = this.VisionManager();
                    visionManager.SettingGrab(CurCam.GetChannelType());
                }
                else
                {
                    CurCam.SetHorizontalFlip(FlipEnum.NONE);
                    IVisionManager visionManager = this.VisionManager();
                    visionManager.SettingGrab(CurCam.GetChannelType());
                }

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion
        //private RelayCommand _TestCommand;
        //public RelayCommand TestCommand
        //{
        //    get
        //    {
        //        if (null == _TestCommand) _TestCommand = new RelayCommand(TestFunc);
        //        return _TestCommand;
        //    }
        //}

        public string ViewModelType { get;set; }

        //public void TestFunc()
        //{
        //    try
        //    {
        //        IVisionManager vision = this.VisionManager();
        //        ImageBuffer ibuf;
        //        ibuf = vision.LoadImageFile(@"C:\OCR\Original.bmp");

        //        var ocrDev = LoaderControllerExt.LoaderDeviceParam.SemicsOCRModules[0];
        //        OCRDevCloneParam = ocrDev.Clone() as SemicsOCRDevice;

        //        ReadOCRProcessingParam procParam = new ReadOCRProcessingParam();
        //        procParam = ConvertToOCRProcessingParam(ocrDev, 0);

        //        vision.ReadOCRProcessing(ibuf, procParam, false);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //    }
        //}
        public ReadOCRProcessingParam ConvertToOCRProcessingParam(SemicsOCRDevice device, int index)
        {
            ReadOCRProcessingParam param = new ReadOCRProcessingParam();

            try
            {
                //param.OcrCalibrateMaxX = device.OcrCalibrateMaxX.Value;
                //param.OcrCalibrateMaxX = device.OcrCalibrateMaxX.Value;
                param.OcrReadRegionPosX = device.OcrReadRegionPosX.Value;
                param.OcrReadRegionPosY = device.OcrReadRegionPosY.Value;
                param.OcrReadRegionWidth = device.OcrReadRegionWidth.Value;
                param.OcrReadRegionHeight = device.OcrReadRegionHeight.Value;
                param.OcrCharSizeX = device.OcrCharSizeX.Value;
                param.OcrCharSizeY = device.OcrCharSizeY.Value;
                param.OcrCharSpacing = device.OcrCharSpacing.Value;
                param.OcrMaxStringLength = device.OcrMaxStringLength.Value;
                param.OcrCalibrateMinX = device.OcrCalibrateMinX.Value;
                param.OcrCalibrateMaxX = device.OcrCalibrateMaxX.Value;
                param.OcrCalibrateStepX = device.OcrCalibrateStepX.Value;
                param.OcrCalibrateMinY = device.OcrCalibrateMinY.Value;
                param.OcrCalibrateMaxY = device.OcrCalibrateMaxY.Value;
                param.OcrCalibrateStepY = device.OcrCalibrateStepY.Value;

                param.OcrSampleString = device.OCRParamTables[index].OcrSampleString.Value;
                param.OcrConstraint = device.OCRParamTables[index].OcrConstraint.Value;
                param.OcrStrAcceptance = device.OCRParamTables[index].OcrStrAcceptance.Value;
                param.OcrCharAcceptance = device.OCRParamTables[index].OcrCharAcceptance.Value;
                param.UserOcrLightType = device.OCRParamTables[index].UserOcrLightType.Value;
                param.UserOcrLight1_Offset = device.OCRParamTables[index].UserOcrLight1_Offset.Value;
                param.UserOcrLight2_Offset = device.OCRParamTables[index].UserOcrLight2_Offset.Value;
                param.UserOcrLight3_Offset = device.OCRParamTables[index].UserOcrLight3_Offset.Value;
                param.OcrCalibrationType = device.OCRParamTables[index].OcrCalibrationType.Value;
                param.OcrMasterFilter = device.OCRParamTables[index].OcrMasterFilter.Value;
                param.OcrMasterFilterGain = device.OCRParamTables[index].OcrMasterFilterGain.Value;
                param.OcrSlaveFilter = device.OCRParamTables[index].OcrSlaveFilter.Value;
                param.OcrSlaveFilterGain = device.OCRParamTables[index].OcrSlaveFilterGain.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return param;
        }

        #region ==> WaferMoveToOCRCommand
        private RelayCommand _WaferMoveToOCRCommand;
        public RelayCommand WaferMoveToOCRCommand
        {
            get
            {
                if (null == _WaferMoveToOCRCommand) _WaferMoveToOCRCommand = new RelayCommand(WaferMoveToOCRCommandFunc);
                return _WaferMoveToOCRCommand;
            }
        }

        private async void WaferMoveToOCRCommandFunc()
        {
            try
            {
                if (IsEditEnable == true && SelectedOCRModule != null && SelectedPreAlignModule != null)
                {
                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                    await Task.Run(() =>
                    {
                        //Move
                        bool isInjected = MoveToOCR(false, false);

                        //Wait
                        if (isInjected)
                        {
                            retVal = this.LoaderController().WaitForCommandDone();

                            if (retVal == EventCodeEnum.NONE)
                            {
                                InitEditSetting();

                                FlipOCRImage();
                                ILightAdmin light = this.LightAdmin();

                                if (OCRDevCloneParam.UserLightEnable.Value)
                                {
                                    light.SetLight(OCRSysParam.LightChannel1.Value, OCRDevCloneParam.OCRParamTables[0].UserOcrLight1_Offset.Value);
                                    light.SetLight(OCRSysParam.LightChannel2.Value, OCRDevCloneParam.OCRParamTables[0].UserOcrLight2_Offset.Value);
                                    light.SetLight(OCRSysParam.LightChannel3.Value, OCRDevCloneParam.OCRParamTables[0].UserOcrLight3_Offset.Value);
                                }
                                else
                                {

                                }

                            }
                        }
                    });

                    await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                    if (retVal == EventCodeEnum.NONE)
                    {
                        //InitData();
                    }

                    IsWaferOnOCR = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> WaferMoveToOCRCommand
        private RelayCommand _WaferMoveToPACommand;
        public RelayCommand WaferMoveToPACommand
        {
            get
            {
                if (null == _WaferMoveToPACommand) _WaferMoveToPACommand = new RelayCommand(WaferMoveToPACommandFunc);
                return _WaferMoveToPACommand;
            }
        }
        private async void WaferMoveToPACommandFunc()
        {
            try
            {
                if (IsEditEnable == true)
                {
                    // Erase Overlay
                    if (CurCam != null)
                    {
                        CurCam.InDrawOverlayDisplay();
                    }
                    OCRPosOverlay = null;
                    OCRCharOverlay.Clear();

                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                    await Task.Run(() =>
                    {
                        ILightAdmin light = this.LightAdmin();
                        light.SetLight(OCRSysParam.LightChannel1.Value, 0);
                        light.SetLight(OCRSysParam.LightChannel2.Value, 0);
                        light.SetLight(OCRSysParam.LightChannel3.Value, 0);

                        //Move
                        bool isInjected = MoveToPreAlign();
                        if (isInjected)
                        {
                            retVal = this.LoaderController().WaitForCommandDone();
                        }

                    });

                    await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                    if (retVal == EventCodeEnum.NONE)
                    {
                        //Reset bidning data
                        //InitData();
                    }

                    IsWaferOnOCR = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> IsWaferOnOCR
        private bool _IsWaferOnOCR;
        public bool IsWaferOnOCR
        {
            get { return _IsWaferOnOCR; }
            set
            {
                if (value != _IsWaferOnOCR)
                {
                    _IsWaferOnOCR = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
    }
}
