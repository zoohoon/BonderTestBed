using AccountVM;
using CardChangeScreenVM;
using ChillerScreenViewModel;
using ChuckPlanaritySubSettingViewModel;
using ChuckPlanarityViewModel;
using ChuckTiltingViewControl;
using ChuckTiltingViewModel;
using CleanUnitVM;
using CognexOCRMainPageView;
using CognexOCRMainPageViewModel;
using ContactSettingVM;
using DeviceChangeVM;
using DeviceSettingMainView;
using DeviceSettingMainViewModel;
using DeviceUpDownMainPageView;
using DeviceUpDownMainPageViewModel;
using DeviceUpDownSubSettingVM;
using DutEditorPageViewModel;
using FileSystemVIewModel;
using ForcedDoneView;
using ForcedDoneViewModel;
using FoupControlViewModel;
using FoupMainControl;
using FoupReoveryViewModel;
using GemSysSettingVM;
using GPCardChangeMainPageView;
using GPCardChangeMainPageViewModel;
using GPCardChangeOPView;
using GPCCAlignSettingView_Standard;
using GPCCAlignSettingViewModel_Standard;
using GpibSettingVM;
using InspectionControlViewModel;
using IOPanelViewModel;
using LoaderSetupPageView;
using LoaderSetupViewModel;
using LoaderStatusSoakingSettingVM;
using LoaderWaferMapMakerViewModelModule;
using LoginControlViewModel;
using LogModule;
using LogSettingVM;
using LogViewerVM;
using LotInfoRecipeControl;
using LotInfoYieldControl;
using LotScreenViewModel;
using MainTopBarControlViewModel;
using MainTopBarView;
using ManualJogView;
using ManualJogViewModel;
using MappingVM;
using MaskingSettingControlViewModel;
using MonitoringMainPageView;
using MonitoringMainPageViewModel;
using MotorsLoaderVM;
using MotorsVM;
using NCPadChangeScreenVM;
using NeedleCleanManualPageView;
using NeedleCleanManualPageViewModel;
using NeedleCleanRecipeSettingVM;
using Newtonsoft.Json;
using OCRView;
using OCRViewModel;
using OPUSV3DView;
using OPUSV3DViewModel;
using PathMakerControlViewModel;
using PathMakerView;
using PinAlignIntervalSettingVM;
using PinBasicInfoVM;
using PMIManualResultVM;
using PMISettingSubView;
using PMISettingSubViewModel;
using PMIViewerViewModel;
using PnpControlViewModel;
using PolishWaferMakeSourceVM;
using PolishWaferRecipeSettingVM;
using PolishWaferSourceSettingVM;
using ProberErrorCode;
using ProberInterfaces;
using ProberViewModel;
using ProberViewModel.View;
using ProberViewModel.View.LogView;
using ProberViewModel.View.ResultMap;
using ProberViewModel.View.SubSetting.Device.ResultMap;
using ProberViewModel.View.Wafer;
using ProberViewModel.ViewModel;
using ProberViewModel.ViewModel.ResultMap;
using ProbingSystemSettingViewModel;
using SequenceMakerScreen;
using SettingTemplateView;
using SoakingSettingView;
using SoakingSettingViewModel;
using StatusSoakingRecipeSettingView;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SystemSettingMainView;
using SystemSettingMainViewModel;
using TaskManagementViewModel;
using TCPIP;
using TempDeviationVM;
using TempDeviceSettingVM;
using TemperatureCalViewModelProject;
using TemperatureCalViewProject;
using TesterCommnuication;
using TestHeadDockScreenVM;
using TestSetupDialog;
using TouchSensorVM;
using UcAccountView;
using UcChillerScreen;
using UcCleanUnitView;
using UcContactSettingView;
using UcDeviceChangeView;
using UcDeviceUpDownSubSettingView;
using UcGemSysSettingView;
using UcGpibSettingView;
using UcMappingView;
using UcMotorsLoaderView;
using UcMotorsView;
using UcNCPadChangeScreenView;
using UcNeedleCleanRecipeSettingView;
using UcPinAlignIntervalSettingView;
using UcPinAlignSettingView;
using UcPinBasicInfoView;
using UcPolishWaferMakeSourceView;
using UcPolishWaferRecipeSettingView;
using UcPolishWaferSourceSettingView;
using UcTempDeviationView;
using UcTempDeviceSettingView;
using UcTouchSensorView;
using UcWaferRecipeSettingView;
using VisionTestView;
using VisionTestViewModel;
using WaferHandlingControl;
using WaferHandlingRecoveryControl;
using WaferHandlingRecoveryViewModel;
using WaferHandlingViewModel;
using WaferRecipeSettingVM;
using WaferSelectionViewModel;

namespace ViewModelModuleParameter
{

    [Serializable]
    public class ViewAndViewModelConnectParam : IParam, ISystemParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }

        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
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


        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ViewAndViewModelConnectParam] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        public string FilePath { get; } = "";

        public string FileName { get; } = "ProberViewConnect.json";

        private List<ViewConnectInfo> _ConnectionInfos = new List<ViewConnectInfo>();
        public List<ViewConnectInfo> ConnectionInfos
        {
            get { return _ConnectionInfos; }
            set
            {
                if (value != _ConnectionInfos)
                {
                    _ConnectionInfos = value;
                }
            }
        }

        private string _ParamLabel;

        public string Genealogy
        {
            get { return _ParamLabel; }
            set { _ParamLabel = value; }
        }

        private ViewConnectInfo MakeViewConnectInfo(Guid pageguid, Guid viewmodelguid)
        {
            ViewConnectInfo tmp = new ViewConnectInfo();
            try
            {

                tmp.ViewGUID = pageguid;
                tmp.ViewModelGUID = viewmodelguid;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return tmp;
        }
        public virtual EventCodeEnum SetEmulParam()
        {
            // TODO : Temporary code 
            //return MakeDefaultParamSetFor_BSCI();

            return SetDefaultParam();
        }

        private Guid GetGuid(Type type)
        {
            Guid retval = default(Guid);

            try
            {
                var tmpobj = Activator.CreateInstance(type);

                retval = (tmpobj as IScreenGUID).ScreenGUID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public virtual EventCodeEnum SetDefaultParam()
        {
            // TODO : 주석 처리 된 내용 있음.
            // 1. AdjustLight
            // 2. Needle Clean sheet setup View

            // View and ViewModel

            // Card Change
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(CardChangeScreenView)), GetGuid(typeof(CardChangeScreenViewModel))));

            // TestHead Dock Undock Change
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(TestHeadDockScreenView)), GetGuid(typeof(TestHeadDockScreenViewModel))));

            // NC Pad Change
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(NCPadChangeScreenView)), GetGuid(typeof(NCPadChangeScreenViewModel))));

            // Chiller
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcHBChiller)), GetGuid(typeof(HBChillerBase))));

            // DeviceUpDown
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcDeviceUpDownMainPage)), GetGuid(typeof(VmDeviceUpDownMainPage))));

            // DutEditor
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UCDutEditor.UCDutEditor)), GetGuid(typeof(VmDutEditorPage))));

            // Foup Main
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(FoupMainControlView)), GetGuid(typeof(FoupControlVM))));

            // IO
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UCIOPanel.UCIOPanel)), GetGuid(typeof(IOPanelVM))));

            // Login
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoginControl.LoginControl)), GetGuid(typeof(LoginControlVM))));

            // Lot
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LotScreen)), GetGuid(typeof(LotScreenVM))));

            // PnP
            //ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("154C019E-D289-D522-BA84-D1EDF3CC0F43"), "IPnpSetup"));
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcPnpControl)), GetGuid(typeof(PnpControlVM))));

            // RecipeEditor
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcRecipeEditorMainPage)), GetGuid(typeof(VmRecipeEditorMainPage))));

            // Sequence
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(SequenceMaker)), GetGuid(typeof(SequenceMakerVM))));

            // Soaking
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UCSoaking)), GetGuid(typeof(SoakingSettingBase))));

            // TaskManagement
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UCTaskManagement.UCTaskManagement)), GetGuid(typeof(TaskManageMentVM))));

            //// ThreadLockExplorer
            //_ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("7C4D2394-B6A6-455C-8EE0-92CD86CC4789")), GetGuid(typeof("5BCDF36D-5FCD-4467-A1F7-447069EFD685"))));

            // TestSetup
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcTestSetupPage)), GetGuid(typeof(VmTestSetupPage))));

            // Wafer Handling
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcWaferHandling)), GetGuid(typeof(WaferHandlingVM))));

            // Wafer Handling Recovery
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcWaferHandlingRecovery)), GetGuid(typeof(WaferHandlingRecoveryVM))));

            // TopBar
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(MainTopBarControl)), GetGuid(typeof(MainTopBarControlVM))));

            // VisionMapping

            //// ManualProbing
            //_ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("770931DB-F411-EEE3-B6E4-43359776AFCA")), GetGuid(typeof("AF9D79D1-2F04-8321-7C83-07D5DDCAC6DD"))));


            // PMI Viewer
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PMIViewerPage)), GetGuid(typeof(PMIViewerVM))));

            //Foup Recovery Viewer
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(GPFocupRecoveryControlView)), GetGuid(typeof(GP_FoupRecoveryControlViewModel))));

            // ManualJog
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ManualJogPage)), GetGuid(typeof(ManualJogViewModelBase))));

            // OCR
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcOCRSetting)), GetGuid(typeof(OCRSettingBase))));

            ////Wizard
            //_ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("2DFA38AC-D0C0-6046-7C6C-B6AFEB295828")), GetGuid(typeof("469ABEA3-6986-805B-0051-F8E2CC88F29C"))));

            // OperatorView
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(OperatorControl.OperatorView)), GetGuid(typeof(LotScreenVM))));
            
            // Monitoring
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcMonitoringMainPage)), GetGuid(typeof(VmMonitoringMainPage))));

            // VisionTest
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcVisionTest)), GetGuid(typeof(VisionTestViewModelBase))));

            // ManualContact
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ManualContactControl)), GetGuid(typeof(ManualContactControlVM))));

            // Inspection
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(InspectionControl)), GetGuid(typeof(InspectionControlVM))));

            // MaskingSetting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(MaskingSettingControl.MaskingSettingControl)), GetGuid(typeof(MaskingSettingControlVM))));

            // Path Maker Page
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PathMakerControl)), GetGuid(typeof(PathMakerControlVM))));

            // Needle clean  Selly 180725
            //_ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("119e4fda-380f-4bd3-8863-f0998c7fb50d")), GetGuid(typeof("ef6cdfcc-7ee8-437a-a38a-1abebec3d664"))));

            // Cognex OCR
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcCognexOCRMainPage)), GetGuid(typeof(VmCognexOCRMainPage))));

            // Manual NC
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcNeedleCleanManualPage)), GetGuid(typeof(VmNeedleCleanManualPage))));

            // System Setting View
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(SystemSettingMainPage)), GetGuid(typeof(SystemSettingMainVM))));

            // Device Setting View
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(DeviceSettingMainPage)), GetGuid(typeof(DeviceSettingMainVM))));

            // Setting Template View
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(SettingTemplatePage)), GetGuid(typeof(SettingTemplateVM))));

            // Device Change
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(DeviceChangeView)), GetGuid(typeof(DeviceChangeViewModel))));

            // Alarms
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(AlarmsViewControl.AlarmsViewControl)), GetGuid(typeof(AlarmsViewModel.AlarmsViewModel))));

            // OPUSV3D
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(OPUSV3D)), GetGuid(typeof(OPUSV3DVM))));

            // Lot Info - Yield
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LotInfoYieldView)), GetGuid(typeof(LotScreenVM))));

            // Lot Info - Recipe
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LotInfoRecipeView)), GetGuid(typeof(LotScreenVM))));

            //// Lot Info - Device Setting
            //_ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("436db5ea-e49e-4fca-a1a8-e3be24639e31")), GetGuid(typeof(LotScreenVM))));

            // BinXMlViewer
            //_ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("d7799f5b-d7e5-4036-8e73-337391c17efe")), GetGuid(typeof("11b91980-f2e0-43f2-8f8f-ae50dc905916"))));

            // Chuck Tilting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ChuckTiltingView)), GetGuid(typeof(ChuckTiltingVM))));

            #region Sub Screen(Setting)

            // Tester communication
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(TesterCommnuicationSettingView)), GetGuid(typeof(TesterCommunicationSettingViewModel.TesterCommunicationSettingViewModel))));
            // TCP/IP
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(TCPIPSettingView)), GetGuid(typeof(TCPIPSettingViewModel.TCPIPSettingViewModel))));
            // GPIB
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(GpibSettingView)), GetGuid(typeof(GpibSettingViewModel))));
            // GEM
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(GemSysSettingView)), GetGuid(typeof(GemSysSettingViewModel))));
            // Pin Align Setting PnP
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PinAlignSettingViewPnP.PinAlignSettingViewPnP)), GetGuid(typeof(PinAlignSettingViewModelPnP.PinAlignSettingViewModelPnP))));
            // Pin Align Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PinAlignSettingView)), GetGuid(typeof(PinAlignSettingVM.PinAlignSettingVM))));
            // Pin Align Interval Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PinAlignIntervalSettingView)), GetGuid(typeof(PinAlignIntervalSettingViewModel))));

            // Needle Clean sheet setup View
            //_ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("e2f9ffe8-ee73-40a9-a35d-56a1cf3f3ba7")), GetGuid(typeof("aa745b2e-d291-4bc4-aeed-4a1bfec6eb89"))));

            // Needle Clean Recipe setup View
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(NeedleCleanRecipeSettingView)), GetGuid(typeof(NeedleCleanRecipeSettingViewModel))));

            //Wafer Recipe Setup 
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(WaferRecipeSettingView)), GetGuid(typeof(WaferRecipeSettingViewModel))));
            //WaferAlign Setting
            //_ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("DE32A33D-AC5D-9A21-CFDA-02A032611C11")), GetGuid(typeof("F9D0CFDA-0611-822C-6D77-1B5FF69B815A"))));
            //Pad Setting
            //_ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("56AF1E26-1C9F-1FDD-B38B-EA0C7324311F")), GetGuid(typeof("8DC261D5-B1C5-C803-CBB6-C47B0F0650D6"))));

            // Mark
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(MarkSettingSubView)), GetGuid(typeof(MarkSettingSubViewModel))));

            // PMI
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PMISettingView.PMISettingView)), GetGuid(typeof(PMISettingViewModel.PMISettingViewModel))));

            // OCR - COGNEX
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(CognexOCRSubSettingView.CognexOCRSubSettingView)), GetGuid(typeof(CognexOCRSubSettingViewModel.CognexOCRSubSettingViewModel))));

            // GPCC Sub Setting View
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(GPCCAlignSettingView)), GetGuid(typeof(GPCCAlignSettingViewModel))));

            // GPCC OP View
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(GPCardChageOPView)), GetGuid(typeof(GPCardChangeOPViewModelModule.GPCardChangeOPViewModelModule))));

            // Machine Setting - Stage
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(MachineSettingStageView.MachineSettingStageView)), GetGuid(typeof(MachineSettingStageViewModel.MachineSettingStageViewModel))));

            // Machine Setting - Loader
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(MachineSettingLoaderView.MachineSettingLoaderView)), GetGuid(typeof(MachineSettingLoaderViewModel.MachineSettingLoaderViewModel))));

            // Vision Mapping
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(VisionMappingSubView.VisionMappingSubView)), GetGuid(typeof(VisionMappingSubViewModel.VisionMappingSubViewModel))));

            // IO
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(IOSubView.IOSubView)), GetGuid(typeof(IOSubViewModel.IOSubViewModel))));

            // FOUP
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(FoupSubView.FoupSubView)), GetGuid(typeof(FoupSubViewModel.FoupSubViewModel))));

            // Maksing
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(MaskingSubView.MaskingSubView)), GetGuid(typeof(MaskingSubViewModel.MaskingSubViewModel))));

            // Dut Editor
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(DutEditorSubView.DutEditorSubView)), GetGuid(typeof(DutEditorSubViewModel.DutEditorSubViewModel))));

            // Probing Sequence
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ProbingSequenceSubView.ProbingSequenceSubView)), GetGuid(typeof(ProbingSequenceSubViewModel.ProbingSequenceSubViewModel))));

            // Log Setting View
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LogSettingView)), GetGuid(typeof(LogSettingViewModel))));

            // Log Viewer View
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LogViewerView)), GetGuid(typeof(LogViewerViewModel))));

            // Account View
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(AccountView)), GetGuid(typeof(AccountViewModel))));

            // Alarms
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(AlarmsSubView.AlarmsSubView)), GetGuid(typeof(AlarmsSubViewModel.AlarmsSubViewModel))));

            // Motors View 
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(MotorsView)), GetGuid(typeof(MotorsViewModel))));

            // Clean Unit View 
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(CleanUnitView)), GetGuid(typeof(CleanUnitViewModel))));

            //Touch Sensor View
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(TouchSensorView)), GetGuid(typeof(TouchSensorViewModel))));

            // Contact
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ContactSettingView)), GetGuid(typeof(ContactSettingViewModel))));

            // Motors ( Loader )
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(MotorsLoaderView)), GetGuid(typeof(MotorsLoaderViewModel))));

            // Pin Basic information
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PinBasicInfoView)), GetGuid(typeof(PinBasicInfoViewModel))));

            // Chuck Tilting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ChuckTiltingSubView.ChuckTiltingSubView)), GetGuid(typeof(ChuckTiltingSubViewModel.ChuckTiltingSubViewModel))));

            // Temp Device Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(TempDeviceSettingView)), GetGuid(typeof(TempDeviceSettingViewModel))));

            // DeviceUpDown Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(DeviceUpDownSubSettingView)), GetGuid(typeof(DeviceUpDownSubSettingViewModel))));

            // Temperature Deviation
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(TempDeviationView)), GetGuid(typeof(TempDeviationViewModel))));

            // Retest
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(RetestSettingSubView.RetestSettingSubView)), GetGuid(typeof(RetestSettingViewModel.RetestSettingViewModel))));

            //PolishWafeRecipe Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PolishWaferRecipeSettingView)), GetGuid(typeof(PolishWaferRecipeSettingViewModel))));

            //PolishWaferDevMainPage - 누락?
            //_ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("398086FA-3531-5841-45C4-A371C317A39C")), GetGuid(typeof(VmPolishWaferDevMainPage))));


            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PolishWaferSourceSettingView)), GetGuid(typeof(PolishWaferSourceSettingViewModel))));

            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PolishWaferMakeSourceView)), GetGuid(typeof(PolishWaferMakeSourceViewModel))));

            #endregion

            // LoaderSetup
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderSetupPage)), GetGuid(typeof(LoaderSetupViewModelBase))));

            // wafer selection
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcWaferSelectionView)), GetGuid(typeof(WaferSelectionVM))));

            // LotRunList Forced Done page
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LotRunForcedDoneView)), GetGuid(typeof(LotRunForcedDoneViewModel))));

            // Create Wafer Map
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(WaferMapMaker)), GetGuid(typeof(LoaderWaferMapMakerViewModel))));

            // Mapping
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(MappingView)), GetGuid(typeof(MappingViewModel))));

            // Manual PMI Page
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PMIManualResultView)), GetGuid(typeof(PMIManualResultViewModel))));

            // SoakingView
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(SoakingRecipeSettingView.SoakingRecipeSettingView)), GetGuid(typeof(SoakingRecipeSettingViewModel.SoakingRecipeSettingViewModel))));

            // GPCCAlign
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcGPCardChangeMainPage)), GetGuid(typeof(VmGPCardChangeMainPage))));

            // OCR - SEMICS
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(SemicsOCRSubSettingView.SemicsOCRSubSettingView)), GetGuid(typeof(SemicsOCRSubSettingViewModel.SemicsOCRSubSettingViewModel))));

            // OCR - COMMON
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(OCRSubSettingView.OCRSubSettingView)), GetGuid(typeof(OCRSubSettingViewModel.OCRSubSettingViewModel))));

            // Temperature Calibration
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(TemperatureCalView)), GetGuid(typeof(TemperatureCalViewModel))));

            // Probing System Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ProbingSystemSettingView.ProbingSystemSettingView)), GetGuid(typeof(ProbingSystemSettingVM))));

            // FIle System
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(FileSystemView.FileSystemView)), GetGuid(typeof(FileSystemVM))));

            // Soaking System
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(SoakingSystemView.SoakingSystemView)), GetGuid(typeof(SoakingSystemViewModel.SoakingSystemVM))));

            // Chuck Planarity
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ChuckPlanarityView.ChuckPlanarityView)), GetGuid(typeof(ChuckPlanarityVM))));

            // Chuck Planarity Sub Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ChuckPlanaritySubSettingView.ChuckPlanaritySubSettingView)), GetGuid(typeof(ChuckPlanaritySubSettingVM))));

            // Bin Analyze
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(BINAnalyzeView)), GetGuid(typeof(BINAnalyzeViewModel))));

            // Result Map Analyze
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ResultMapAnalyzeViewer)), GetGuid(typeof(ResultMapAnalyzeViewerVM))));

            // STIP Map Analyze
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(STIFMapAnalyzeView)), GetGuid(typeof(STIFMapAnalyzeVM))));

            // E142 Analyze
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(E142MapAnalyzeView)), GetGuid(typeof(E142MapAnalyzeVM))));

            // ResultMap - Converter Sub Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ResultMapConverterView)), GetGuid(typeof(ResultMapConverterVM))));

            // MultiManual Contact
            _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("6c307199-ddcd-496d-89f7-a462bd13f949"), new Guid("d940ad3e-b182-48aa-b23e-9384b86aa316")));

            // Manual Soaking
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ManualSoakingView)), GetGuid(typeof(ManualSoakingViewModel))));

            // Uc Card Change Setting View
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcCardChangeSettingView)), GetGuid(typeof(UcCardChangeSettingViewModel))));

            #region Status Soaking
            // Status Soaking Recipe Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(StatusSoakingSettingView)), GetGuid(typeof(StatusSoakingSettingViewModel))));

            // Soaking Step
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcSoakingStep)), GetGuid(typeof(UcSoakingStepViewModel))));

            // Soaking Polish Wafer
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcSoakingPolishWafer)), GetGuid(typeof(UcSoakingPolishWaferViewModel))));

            // Soaking OD
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcSoakingOD)), GetGuid(typeof(UcSoakingODViewModel))));
            #endregion

            #region Digital Twin

            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(DigitalTwinSettingView)), GetGuid(typeof(DigitalTwinSettingViewModel))));
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(DigitalTwinView)), GetGuid(typeof(DigitalTwinViewModel))));

            #endregion

            // Accuracy Check
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(AccuracyCheckSetup)), GetGuid(typeof(AccuracyCheckSetupViewModel))));
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(AccuracyCheckSubSettingView)), GetGuid(typeof(AccuracyCheckSubSettingViewModel))));

            // Manual Soaking
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ManualSoakingView)), GetGuid(typeof(ManualSoakingViewModel))));

            #region Status Soaking
            // Status Soaking Recipe Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(StatusSoakingSettingView)), GetGuid(typeof(StatusSoakingSettingViewModel))));

            // Soaking Step
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcSoakingStep)), GetGuid(typeof(UcSoakingStepViewModel))));

            // Soaking Polish Wafer
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcSoakingPolishWafer)), GetGuid(typeof(UcSoakingPolishWaferViewModel))));

            // Soaking OD
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcSoakingOD)), GetGuid(typeof(UcSoakingODViewModel))));
            #endregion

            // Wafer Align System
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(WaferAlignSysSubView)), GetGuid(typeof(WaferAlignSysSubViewModel))));

            return EventCodeEnum.NONE;
        }

        private EventCodeEnum MakeDefaultParamSetFor_BSCI()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("367FF140-F606-B65B-7C17-1A743AD06ED6"), new Guid("01613590-0FAD-CE9F-9A79-F13B71BA2A05")));

                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("0922a4d5-d811-425d-a68f-6ce5a210102b"), new Guid("91d47e22-66f7-c358-736c-5bcf7c3e287e")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("5c897e13-2d0b-4a12-8f6e-7389e5a75482"), new Guid("f1a99db1-571b-4871-92ed-4849e77c1755")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("82816653-ae8a-462e-9f9a-cae770926a37"), new Guid("7e637da9-b58b-4157-bb16-3e6c6cff186f")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("5b755687-ae34-4bbf-8112-71738b9f91d8"), new Guid("c8855847-e34e-4651-878f-0e4a3875a02e")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("78744426-ef1d-4624-a961-4a756669a9b7"), new Guid("5899dcfe-3032-5360-03d7-1f356b7a0800")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("e89d213f-abed-4962-b410-71ae4f0cdf53"), new Guid("972b231e-ca73-4aa1-9f43-c5115cf980bb")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("10ca2446-3785-44c1-ab07-e292036b82ea"), new Guid("30be2fda-e484-d816-3f5e-405677fe3f2e")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("28a11f12-8918-47fe-8161-3652f2efef29"), new Guid("1f60343b-5f92-4aa1-99da-788a4e12c25d")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("6223dfd5-efaa-4b49-ab70-d8a5f03fa65d"), new Guid("cbed19a9-1a90-43db-b31f-daf29bc852b4")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("1b96aa21-1613-108a-71d6-9bce684a4dd0"), new Guid("1c96aa21-1613-108a-71d6-9bce684a4dd0")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("e9a65dcf-90d7-421c-8174-6808d4466bae"), new Guid("f05a242e-3834-33b7-9b4a-b3d913f2f255")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("ec8fb998-222f-1e88-2c18-6df6a742b3e9"), new Guid("c3bc83a1-c6ca-4bb4-0de2-34df6ea06df2")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("e063e8f5-ee65-8fb5-0ca3-f3fbd7b51b78"), new Guid("b2c604c1-57e6-557e-6d4a-d2a3c7d120e8")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("0958e509-2985-42ef-857c-660e2f05789a"), new Guid("aecf25cc-42ba-4d30-b94c-50ea23b3934e")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("4f42078c-05fe-b4b7-70ed-0602c9df269b"), new Guid("030b1cfa-a617-404a-9c44-031754f15f7e")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("51562d6c-d283-85e5-d743-350e2f0c8abd"), new Guid("a9796e36-d6d8-6ea1-349b-6e5e30a90e68")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("5142bd1c-e64b-51f5-29ce-50620bed445a"), new Guid("4576db4b-63f1-478a-ab94-44772306bdf1")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("d1bc0f1e-36b7-4508-b9c0-c09f08a5587c"), new Guid("cbed19a9-1a90-43db-b31f-daf29bc852b4")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("8d6da80d-f181-4d4c-8a96-4a1367682f99"), new Guid("5d9d5473-d933-4151-9d1b-93e001e0d589")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("ac247488-2cb3-4250-9cfd-bd6852802a83"), new Guid("48468e20-b3dc-4e45-b075-690056f566bf")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("f8396e3a-b8ce-4dcd-9a0d-643532a7d9d1"), new Guid("652c9ce4-f811-47ab-88f2-da972eeb66d9")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("cff8b896-1ad0-4c13-b51a-859f352385f4"), new Guid("9570a178-bd3f-49e1-b37a-8d4fa85d2d85")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("8aa6dac9-5c54-43e3-8802-0a5dd69e25c8"), new Guid("242c19d4-1866-4501-9556-1bc1e7f9699e")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("4e9f3ab5-d0b8-47c0-8ce6-cbcb56803e98"), new Guid("370c0268-621f-42b5-b915-0174470b1e94")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("a43a956d-b46e-61b6-5a61-31766111750d"), new Guid("d34c35d7-5c00-cbe0-20b6-d424137d2e18")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("c059599b-bedf-2137-859b-47c15e433e4d"), new Guid("af818282-0a06-791f-910f-5b8bcfc21466")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("b0737a90-696c-9155-d4c9-a628d1eed129"), new Guid("aa74ce25-f777-9dcc-c6fb-a0cfc1259db5")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("956bb44f-4b89-42b3-b21a-69f896a840fe"), new Guid("39d5c48c-0bd7-4b6a-ab2d-113df85dce0e")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("b0722a2f-bc56-4e74-88e7-63fbf4ec7d63"), new Guid("cbed19a9-1a90-43db-b31f-daf29bc852b4")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("7c94444f-d655-407b-9a7e-0b938505eb99"), new Guid("cbed19a9-1a90-43db-b31f-daf29bc852b4")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("5d647d3b-affd-4a4d-9c1f-d0ad8c3d1a75"), new Guid("c3ff1695-274b-4075-8c0f-19172fc56506")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("24389ce4-fa92-44b7-9d31-fdbfe2cab2df"), new Guid("154addae-bc21-4747-8dc5-06bf3a53cfba")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("a4bd4bb5-030d-4644-9c3d-c21921c09eee"), new Guid("377bfd55-081b-4f46-9c48-5e20ec3e20e9")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("a0005054-423a-f131-4b41-29b0091c34c0"), new Guid("870a076c-a427-28aa-729a-d50d3a19509e")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("297ecf90-74ae-47e3-aba5-d21d01eb095a"), new Guid("e35de570-8418-4a75-abfd-6ad13fbbf1ab")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("bf4d5ae0-9778-45ab-b42d-218bf162d4a6"), new Guid("0437cc00-96b7-42a3-806c-2049b3f3710c")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("c1f9e022-9fac-4214-88c9-f3019234c1a4"), new Guid("8127b8e5-2bef-41bd-a426-fce7a9527b7d")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("e88db401-f69d-49ee-b4f4-dd7a1816d874"), new Guid("928e13af-714d-44c1-9ee7-6894a7f88848")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("647dc97d-99ab-4355-b7ac-1976d322e900"), new Guid("14135260-e9b4-4a56-a392-fe696665122b")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("3cf6c16e-a992-47a7-a923-b77297c5e7b7"), new Guid("b15b9119-6ed9-4a89-987f-8be2454ff473")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("e9a9d64a-4b28-40f1-9f64-1f348dc183ae"), new Guid("1f7d4581-2baf-4703-9c38-7989972ba2f4")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("eafd6cea-ce4e-438b-8e9e-b4ebf5155641"), new Guid("2704fa09-3351-4aff-af7e-f9ea8c9b2d78")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("9f908e5e-db34-4b12-931b-73cc175457f2"), new Guid("e9b53f9b-0d21-4f1f-bf62-19cc01058d8a")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("016b67d9-9d63-471a-97af-075abc3a841e"), new Guid("1fee6287-7877-4593-be13-645a535cc50b")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("f83a2fc4-3e01-4b13-ae84-b59acd8e1a26"), new Guid("e4cfa765-19c2-4f08-bcc1-2ec80a2c90dd")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("5538cad1-5238-43a7-9ed9-c789f191e488"), new Guid("0df9a407-e70c-4f20-8396-e3968ee79be1")));
                //_ConnectionInfos.Add(MakeViewConnectInfo(new Guid("ba31e6d7-f7d1-4234-92de-0c373e87d7ee"), new Guid("30be3fa8-07b7-4724-b029-1c5a3ea50dd5")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("53201f06-8810-4e46-b2b0-e748dc9d97f1"), new Guid("29342882-1143-477c-90b6-062460bea1b3")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("67a5e6b4-c986-4323-b05c-9139e703a0f9"), new Guid("e4a7761e-d257-45ca-9752-76ea3e35e314")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("7bd6a1b7-8d90-4dee-8de7-633bb966de6a"), new Guid("85fae7d6-723f-490e-a163-0bf24f8acb03")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("89829dc2-e884-4afb-a7d0-ca7576834a18"), new Guid("c93e4f41-0b94-4e37-9e50-a71fb3d565b1")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("9FF1993E-2306-4894-A1A1-271317338E41"), new Guid("8D28C9F7-35C2-48DD-96C1-5BB8E48FE083")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("d13fc548-bce1-4c4e-b1e3-413a445db9f0"), new Guid("7a33ace3-1aa4-4ad2-9719-bebe14ef2092")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("bccda8db-7848-4e6b-af7a-79d796c987ab"), new Guid("615077c7-e0c9-49e0-ba5c-cc74b1fe6920")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("edf7abaa-b134-40bf-b970-0dbf0f8afa45"), new Guid("cb73097c-1b95-4838-b2f8-f1dbedef3904")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("38059571-0235-407e-be2f-b3cf6073034a"), new Guid("2fbad965-a727-4392-9cf0-e80252ee08c5")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("8cd732eb-7cd4-4a0b-bb18-b33b99dafb90"), new Guid("8c421b73-df22-4b94-8d29-464c53de471b")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("17f02446-d73d-4e02-9685-0ea1f4569e5f"), new Guid("7ee3929b-d54a-4e39-bf50-cfb2193515e4")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("31211d6f-b8a1-16c6-04ea-1656f17c4c54"), new Guid("6f439e21-c584-9fbc-5d01-1d7aee29f665")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("02cf70d9-f2d3-4180-9efc-6fc8e5a22041"), new Guid("dc47a2a3-404d-4669-b445-6c3293ee1d47")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("589ecfad-b887-4e38-a9e3-68feca7e7513"), new Guid("0e719d6c-c283-4643-9afb-b2c1465892c8")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("5ccad8ef-1255-f3ce-5118-c245943f1993"), new Guid("156c4231-360d-138c-ed86-6e586c45f359")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("5e911839-a446-4c6c-bed4-a7efda752bfd"), new Guid("02f764ee-ab24-405c-8127-c92873a4efe9")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("f1862d58-2c59-4fd3-8439-2c673605e2ce"), new Guid("c104db0e-af00-4760-b557-601c5b7b6a26")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("e905653f-52ff-460a-b7bf-d82f8996c0d8"), new Guid("9a7d8e76-e26c-47bc-bc95-4976c87d314b")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("6d270b53-0fe3-40b2-831f-1bd8766ede86"), new Guid("d1255b25-a2fd-4abe-85c7-3ef13f8c7878")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("8cf01c41-628c-4ea9-bba2-38a6f934e5fc"), new Guid("7c1326fc-580e-4ede-a8a4-287387402b74")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("d3b97c85-2bee-4fd8-834f-bb4c3401752a"), new Guid("060f91cf-30ff-4d81-9025-d434822d384a")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("31c6df0a-ff3c-4d31-b8bd-b7168ac4a7fb"), new Guid("2c50095f-50b0-4984-8208-d913f5a2b3fd")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("EC8FB988-222F-1E88-2C18-6DF6A742B3E9"), new Guid("CBED19F9-1A90-43DB-B31F-DAF29BC852B4")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("88573974-bdc7-4a80-b399-6fc44852122b"), new Guid("2792ccc6-95d0-48bd-8ad7-c1c0e855f5ec")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("c55fcfdc-0dca-47a9-a94e-12f4199383ea"), new Guid("86664d7c-c444-4228-a3ea-e4c52b8557e6")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("F398D89E-7A38-4CD2-BA9F-291FAED435EB"), new Guid("17EC126F-7F6E-4797-8407-37F58862626D")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("444B1280-FC75-4932-9B63-D055DF8C91C9"), new Guid("AA8DD25E-E5AC-47CC-A1E5-517C867D26D7")));
                _ConnectionInfos.Add(MakeViewConnectInfo(new Guid("DA742EF8-7AA4-41BE-BA65-81F53166213C"), new Guid("2C284C1F-CCBD-4E1F-800E-7BA2B365A208")));


                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
