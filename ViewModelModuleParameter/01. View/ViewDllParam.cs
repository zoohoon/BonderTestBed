using ChuckTiltingViewControl;
using CognexOCRMainPageView;
using DeviceSettingMainView;
using DeviceUpDownMainPageView;
using ForcedDoneView;
using FoupMainControl;
using GPCardChangeMainPageView;
using GPCardChangeOPView;
using GPCCAlignSettingView_Standard;
using LoaderSetupPageView;
using LogModule;
using LotInfoRecipeControl;
using LotInfoYieldControl;
using MainTopBarView;
using ManualJogView;
using MonitoringMainPageView;
using NeedleCleanManualPageView;
using Newtonsoft.Json;
using OCRView;
using OperatorControl;
using OPUSV3DView;
using PathMakerView;
using PMISettingSubView;
using ProberErrorCode;
using ProberInterfaces;
using ProberViewModel;
using ProberViewModel.View;
using ProberViewModel.View.LogView;
using ProberViewModel.View.ResultMap;
using ProberViewModel.View.SubSetting.Device.ResultMap;
using ProberViewModel.View.Wafer;
using SequenceMakerScreen;
using SettingTemplateView;
using SoakingSettingView;
using StatusSoakingRecipeSettingView;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SystemSettingMainView;
using TCPIP;
using TemperatureCalViewProject;
using TesterCommnuication;
using TestSetupDialog;
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
using UcSamplePinAlignSettingView;
using UcTempDeviationView;
using UcTempDeviceSettingView;
using UcTouchSensorView;
using UcWaferRecipeSettingView;
using VisionTestView;
using WaferHandlingControl;

namespace ViewModelModuleParameter
{
    [Serializable]
    public class ViewDLLParam : IParam, ISystemParameterizable
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
                LoggerManager.Debug($"[ViewDLLParam] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }

        public List<ModuleDllInfo> info = new List<ModuleDllInfo>();

        public string FilePath { get; } = "";

        public string FileName { get; } = "ProberView.json";

        //private string _ParamLabel;

        //public string Genealogy
        //{
        //    get { return _ParamLabel; }
        //    set { _ParamLabel = value; }
        //}

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }

        private ModuleDllInfo MakeModuleDllInfo(string assemblyname, int version, bool enablebackwardcompatibility)
        {
            ModuleDllInfo tmp = new ModuleDllInfo();
            try
            {

                tmp.AssemblyName = assemblyname;
                tmp.Version = version;
                tmp.EnableBackwardCompatibility = enablebackwardcompatibility;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return tmp;
        }
        public EventCodeEnum SetEmulParam()
        {
            // TODO : Temporary code 
            //return MakeDefaultParamSetFor_BSCI();

            return SetDefaultParam();
        }

        private ModuleDllInfo MakeViewModelInfo(string assemblyname, string classname, int version, bool enablebackwardcompatibility)
        {
            ModuleDllInfo tmp = new ModuleDllInfo();

            try
            {
                tmp.AssemblyName = assemblyname;
                tmp.ClassName.Add(classname);
                tmp.Version = version;
                tmp.EnableBackwardCompatibility = enablebackwardcompatibility;
            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return tmp;
        }

        public virtual EventCodeEnum SetDefaultParam()
        {
            var dllname = "ProberViewModel.dll";

            // TODO :
            // LotInfoDeviceSettingView - 확인 필요

            // Card Change
            info.Add(MakeViewModelInfo(dllname, nameof(CardChangeScreenView), 1000, true));
            // NC Pad Change
            info.Add(MakeViewModelInfo(dllname, nameof(NCPadChangeScreenView), 1000, true));
            // Test Head Dock
            info.Add(MakeViewModelInfo(dllname, nameof(TestHeadDockScreenView), 1000, true));
            // Chiller
            info.Add(MakeViewModelInfo(dllname, nameof(UcHBChiller), 1000, true));
            // DeviceUpDown
            info.Add(MakeViewModelInfo(dllname, nameof(UcDeviceUpDownMainPage), 1000, true));
            // DutEditor
            info.Add(MakeViewModelInfo(dllname, nameof(UCDutEditor.UCDutEditor), 1000, true));
            // Foup Main
            info.Add(MakeViewModelInfo(dllname, nameof(FoupMainControlView), 1000, true));
            // IO
            info.Add(MakeViewModelInfo(dllname, nameof(UCIOPanel.UCIOPanel), 1000, true));
            // Login
            info.Add(MakeViewModelInfo(dllname, nameof(LoginControl.LoginControl), 1000, true));
            // Lot
            info.Add(MakeViewModelInfo(dllname, nameof(LotScreen), 1000, true));
            // PnP
            info.Add(MakeViewModelInfo(dllname, nameof(UcPnpControl), 1000, true));
            // RecipeEditor
            info.Add(MakeViewModelInfo(dllname, nameof(UcRecipeEditorMainPage), 1000, true));
            // Sequence
            info.Add(MakeViewModelInfo(dllname, nameof(SequenceMaker), 1000, true));
            
            // TaskManagement
            info.Add(MakeViewModelInfo(dllname, nameof(UCTaskManagement.UCTaskManagement), 1000, true));

            // Soaking
            info.Add(MakeViewModelInfo(dllname, nameof(UCSoaking), 1000, true));
            // TopBar
            info.Add(MakeViewModelInfo(dllname, nameof(MainTopBarControl), 1000, true));

            // OperatorView
            info.Add(MakeViewModelInfo(dllname, nameof(OperatorView), 1000, true));

            // PMI Viewer
            info.Add(MakeViewModelInfo(dllname, nameof(PMIViewerPage), 1000, true));

            // Foup Recovery Viewer
            info.Add(MakeViewModelInfo(dllname, nameof(GPFocupRecoveryControlView), 1000, true));

            //ManualJog
            info.Add(MakeViewModelInfo(dllname, nameof(ManualJogPage), 1000, true));

            //OCR
            info.Add(MakeViewModelInfo(dllname, nameof(UcOCRSetting), 1000, true));
            //WaferHandlingControl
            info.Add(MakeViewModelInfo(dllname, nameof(UcWaferHandling), 1000, true));
            //Monitoring
            info.Add(MakeViewModelInfo(dllname, nameof(UcMonitoringMainPage), 1000, true));
            //VisionTest
            info.Add(MakeViewModelInfo(dllname, nameof(UcVisionTest), 1000, true));
            // ManualContact
            info.Add(MakeViewModelInfo(dllname, nameof(ManualContactControl), 1000, true));
            // Inspection
            info.Add(MakeViewModelInfo(dllname, nameof(InspectionControl), 1000, true));
            // MaskingSetting
            info.Add(MakeViewModelInfo(dllname, nameof(MaskingSettingControl.MaskingSettingControl), 1000, true));
            // Path Maker Page
            info.Add(MakeViewModelInfo(dllname, nameof(PathMakerControl), 1000, true));
            //System Setting View Page
            info.Add(MakeViewModelInfo(dllname, nameof(SystemSettingMainPage), 1000, true));
            //Device Setting View Page
            info.Add(MakeViewModelInfo(dllname, nameof(DeviceSettingMainPage), 1000, true));
            //Setting Template View Page
            info.Add(MakeViewModelInfo(dllname, nameof(SettingTemplatePage), 1000, true));
            // Cognex OCR
            info.Add(MakeViewModelInfo(dllname, nameof(UcCognexOCRMainPage), 1000, true));
            // Manual NC
            info.Add(MakeViewModelInfo(dllname, nameof(UcNeedleCleanManualPage), 1000, true));
            // Device Change
            info.Add(MakeViewModelInfo(dllname, nameof(DeviceChangeView), 1000, true));
            // Alarms
            info.Add(MakeViewModelInfo(dllname, nameof(AlarmsViewControl.AlarmsViewControl), 1000, true));

            // OPUSV3D
            info.Add(MakeViewModelInfo(dllname, nameof(OPUSV3D), 1000, true));

            // Lot Info - Yield
            info.Add(MakeViewModelInfo(dllname, nameof(LotInfoYieldView), 1000, true));
            // Lot Info - Recipe
            info.Add(MakeViewModelInfo(dllname, nameof(LotInfoRecipeView), 1000, true));
            
            //// Lot Info - Device Setting
            //info.Add(MakeViewModelInfo(dllname, nameof(LotInfoDeviceSettingView), 1000, true));

            // Chuck Tilting
            info.Add(MakeViewModelInfo(dllname, nameof(ChuckTiltingView), 1000, true));
            #region Sub Screen(Setting)
            // Retest
            info.Add(MakeViewModelInfo(dllname, nameof(RetestSettingSubView.RetestSettingSubView), 1000, true));
            // Gpib
            info.Add(MakeViewModelInfo(dllname, nameof(GpibSettingView), 1000, true));
            // GPCCAlignSettingView_Standard
            info.Add(MakeViewModelInfo(dllname, nameof(GPCCAlignSettingView), 1000, true));
            // GPCCCardChangeMainPageView
            info.Add(MakeViewModelInfo(dllname, nameof(UcGPCardChangeMainPage), 1000, true));
            // GPCardChangeOP
            info.Add(MakeViewModelInfo(dllname, nameof(GPCardChageOPView), 1000, true));
            // GEM
            info.Add(MakeViewModelInfo(dllname, nameof(GemSysSettingView), 1000, true));
            // Needle Clean Recipe View
            info.Add(MakeViewModelInfo(dllname, nameof(NeedleCleanRecipeSettingView), 1000, true));
            // Wafer Recipe Setting View
            info.Add(MakeViewModelInfo(dllname, nameof(WaferRecipeSettingView), 1000, true));
            // Pin Align Setting View PnP
            info.Add(MakeViewModelInfo(dllname, nameof(PinAlignSettingViewPnP.PinAlignSettingViewPnP), 1000, true));
            // Pin Align Setting View
            //info.Add(MakeViewModelInfo(dllname, nameof(UcPinAlignSettingView.dll), 1000, true));
            info.Add(MakeViewModelInfo(dllname, nameof(PinAlignSettingView), 1000, true));
            // Sample Pin Align Setting View
            info.Add(MakeViewModelInfo(dllname, nameof(SamplePinAlignSettingView), 1000, true));
            // Pin Align Interval Setting View
            info.Add(MakeViewModelInfo(dllname, nameof(PinAlignIntervalSettingView), 1000, true));
            // Mark
            info.Add(MakeViewModelInfo(dllname, nameof(MarkSettingSubView), 1000, true));
            // OCR - COGNEX
            info.Add(MakeViewModelInfo(dllname, nameof(CognexOCRSubSettingView.CognexOCRSubSettingView), 1000, true));
            // PMI
            info.Add(MakeViewModelInfo(dllname, nameof(PMISettingView.PMISettingView), 1000, true));
            // MachineSetting - Stage
            info.Add(MakeViewModelInfo(dllname, nameof(MachineSettingStageView.MachineSettingStageView), 1000, true));
            // MachineSetting - Loader
            info.Add(MakeViewModelInfo(dllname, nameof(MachineSettingLoaderView.MachineSettingLoaderView), 1000, true));
            // Vision Mapping
            info.Add(MakeViewModelInfo(dllname, nameof(VisionMappingSubView.VisionMappingSubView), 1000, true));
            // IO
            info.Add(MakeViewModelInfo(dllname, nameof(IOSubView.IOSubView), 1000, true));
            // FOUP
            info.Add(MakeViewModelInfo(dllname, nameof(FoupSubView.FoupSubView), 1000, true));
            // Masking
            info.Add(MakeViewModelInfo(dllname, nameof(MaskingSubView.MaskingSubView), 1000, true));
            // Dut Editor
            info.Add(MakeViewModelInfo(dllname, nameof(DutEditorSubView.DutEditorSubView), 1000, true));
            // Probing Seq
            info.Add(MakeViewModelInfo(dllname, nameof(ProbingSequenceSubView.ProbingSequenceSubView), 1000, true));
            // Log setting View
            info.Add(MakeViewModelInfo(dllname, nameof(LogSettingView), 1000, true));
            // Log Viewer View
            info.Add(MakeViewModelInfo(dllname, nameof(LogViewerView), 1000, true));
            //Account
            info.Add(MakeViewModelInfo(dllname, nameof(AccountView), 1000, true));
            //Alarms
            info.Add(MakeViewModelInfo(dllname, nameof(AlarmsSubView.AlarmsSubView), 1000, true));
            //Motors View
            info.Add(MakeViewModelInfo(dllname, nameof(MotorsView), 1000, true));
            //Clean Unit View
            info.Add(MakeViewModelInfo(dllname, nameof(CleanUnitView), 1000, true));
            //Touch Sensor Setup View
            info.Add(MakeViewModelInfo(dllname, nameof(TouchSensorView), 1000, true));
            // ContactSetting
            info.Add(MakeViewModelInfo(dllname, nameof(ContactSettingView), 1000, true));
            // Motors ( Loader )
            info.Add(MakeViewModelInfo(dllname, nameof(MotorsLoaderView), 1000, true));
            // Pin Basic information
            info.Add(MakeViewModelInfo(dllname, nameof(PinBasicInfoView), 1000, true));
            // Temp Device Setting
            info.Add(MakeViewModelInfo(dllname, nameof(TempDeviceSettingView), 1000, true));
            // Mapping
            info.Add(MakeViewModelInfo(dllname, nameof(MappingView), 1000, true));
            // DeviceUpDown
            info.Add(MakeViewModelInfo(dllname, nameof(DeviceUpDownSubSettingView), 1000, true));
            // Temperature Deviation
            info.Add(MakeViewModelInfo(dllname, nameof(TempDeviationView), 1000, true));
            // Soaking
            info.Add(MakeViewModelInfo(dllname, nameof(SoakingRecipeSettingView.SoakingRecipeSettingView), 1000, true));

            // VM이 주석되어 있음.
            //info.Add(MakeViewModelInfo(dllname, nameof(PolishWaferManualPage), 1000, true));

            // PolishWafer Recipe Setting

            info.Add(MakeViewModelInfo(dllname, nameof(PolishWaferRecipeSettingView), 1000, true));

            // PolishWafer Source
            info.Add(MakeViewModelInfo(dllname, nameof(PolishWaferSourceSettingView), 1000, true));
            info.Add(MakeViewModelInfo(dllname, nameof(PolishWaferMakeSourceView), 1000, true));


            #endregion

            //LoaderSetup
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderSetupPage), 1000, true));
            //wafer selection
            info.Add(MakeViewModelInfo(dllname, nameof(UcWaferSelectionView), 1000, true));
            //ForcedDone
            info.Add(MakeViewModelInfo(dllname, nameof(LotRunForcedDoneView), 1000, true));
            // Chuck Tilting
            info.Add(MakeViewModelInfo(dllname, nameof(ChuckTiltingSubView.ChuckTiltingSubView), 1000, true));
            // Create Wafer Map
            info.Add(MakeViewModelInfo(dllname, nameof(WaferMapMaker), 1000, true));
            // Manual PMI Page
            info.Add(MakeViewModelInfo(dllname, nameof(PMIManualResultView), 1000, true));
            // Test Setup Page View
            info.Add(MakeViewModelInfo(dllname, nameof(UcTestSetupPage), 1000, true));
            // OCR - SEMICS
            info.Add(MakeViewModelInfo(dllname, nameof(SemicsOCRSubSettingView.SemicsOCRSubSettingView), 1000, true));
            // OCR - COMMON
            info.Add(MakeViewModelInfo(dllname, nameof(OCRSubSettingView.OCRSubSettingView), 1000, true));
            // Temperature Calibration
            info.Add(MakeViewModelInfo(dllname, nameof(TemperatureCalView), 1000, true));
            // Probing System Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ProbingSystemSettingView.ProbingSystemSettingView), 1000, true));

            // File System Setting
            info.Add(MakeViewModelInfo(dllname, nameof(FileSystemView.FileSystemView), 1000, true));
            
            // Soaking System Setting
            info.Add(MakeViewModelInfo(dllname, nameof(SoakingSystemView.SoakingSystemView), 1000, true));

            // Chuck Planarity
            info.Add(MakeViewModelInfo(dllname, nameof(ChuckPlanarityView.ChuckPlanarityView), 1000, true));
            // Chuck Planarity Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ChuckPlanaritySubSettingView.ChuckPlanaritySubSettingView), 1000, true));

            // Tester communication Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(TesterCommnuicationSettingView), 1000, true));

            // TCP/IP Setup Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(TCPIPSettingView), 1000, true));

            // BIN Analyze
            info.Add(MakeViewModelInfo(dllname, nameof(BINAnalyzeView), 1000, true));

            // ResultMap - Converter Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ResultMapConverterView), 1000, true));

            // Result Map Analyze
            info.Add(MakeViewModelInfo(dllname, nameof(ResultMapAnalyzeViewer), 1000, true));

            // STIF Map Analyze
            info.Add(MakeViewModelInfo(dllname, nameof(STIFMapAnalyzeView), 1000, true));

            // E142 Analyze
            info.Add(MakeViewModelInfo(dllname, nameof(E142MapAnalyzeView), 1000, true));

            // ResultMap - Converter Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ResultMapConverterView), 1000, true));

            //Manual Soaking
            info.Add(MakeViewModelInfo(dllname, nameof(ManualSoakingView), 1000, true));

            // Card Change settingView
            info.Add(MakeViewModelInfo(dllname, nameof(UcCardChangeSettingView), 1000, true));

            #region Status Soaking
            // Status Soaking Recipe Setting
            info.Add(MakeViewModelInfo(dllname, nameof(StatusSoakingSettingView), 1000, true));

            // Soaking Step
            info.Add(MakeViewModelInfo(dllname, nameof(UcSoakingStep), 1000, true));

            // Soaking Polish Wafer
            info.Add(MakeViewModelInfo(dllname, nameof(UcSoakingPolishWafer), 1000, true));

            // Soaking OD
            info.Add(MakeViewModelInfo(dllname, nameof(UcSoakingOD), 1000, true));

            #endregion

            #region Digital Twin

            info.Add(MakeViewModelInfo(dllname, nameof(DigitalTwinView), 1000, true));
            info.Add(MakeViewModelInfo(dllname, nameof(DigitalTwinSettingView), 1000, true));

            #endregion

            // Accuracy Check
            info.Add(MakeViewModelInfo(dllname, nameof(AccuracyCheckSetup), 1000, true));
            info.Add(MakeViewModelInfo(dllname, nameof(AccuracyCheckSubSettingView), 1000, true));
            // WaferAlign Sys
            info.Add(MakeViewModelInfo(dllname, nameof(WaferAlignSysSubView), 1000, true));

            return EventCodeEnum.NONE;
        }

        /// <summary>
        /// BSCI에서 사용할 디폴트 파라미터 세트를 만들어주는 함수
        /// </summary>
        private EventCodeEnum MakeDefaultParamSetFor_BSCI()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "UcVisionTest", 1000, true));

                info.Add(MakeViewModelInfo("ProberViewModel.dll", "CardChangeScreenView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "NCPadChangeScreenView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "TestHeadDockScreenView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "UcDeviceUpDownMainPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "UCDutEditor", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "FoupMainControlView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "UCIOPanel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "LoginControl", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "LotScreen", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "UcPnpControl", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "UcRecipeEditorMainPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "SequenceMaker", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "UCTaskManagement", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MainTopBarControl", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "ManualJogPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "UcOCRSetting", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "UcWaferHandling", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "OperatorView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "UcMonitoringMainPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "ManualContactControl", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "InspectionControl", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MaskingSettingControl", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "SystemSettingMainPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "DeviceSettingMainPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "SettingTemplatePage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "UcCognexOCRMainPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "UcNeedleCleanManualPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "DeviceChangeView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "LotInfoYieldView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "LotInfoRecipeView", 1000, true));
                
                //info.Add(MakeViewModelInfo("ProberViewModel.dll", "LotInfoDeviceSettingView", 1000, true)); // 삭제?

                info.Add(MakeViewModelInfo("ProberViewModel.dll", "GpibSettingView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "NeedleCleanRecipeSettingView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "WaferRecipeSettingView", 1000, true));
                
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "PinAlignSettingViewPnP", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "PinAlignIntervalSettingView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "PinAlignSettingView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MarkSettingSubView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "CognexOCRSubSettingView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "PMISettingView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MachineSettingStageView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MachineSettingLoaderView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "VisionMappingSubView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "IOSubView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "FoupSubView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MaskingSubView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "DutEditorSubView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "ProbingSequenceSubView", 1000, true));
                //info.Add(MakeViewModelInfo("ProberViewModel.dll", "LogSettingView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "LogViewerView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "StatisticsSubView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "AccountView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MotorsView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "CleanUnitView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "TouchSensorView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "ContactSettingView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MotorsLoaderView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "PinBasicInfoView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "TempDeviceSettingView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MappingView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "DeviceUpDownSubSettingView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "TempDeviationView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "LoaderSetupPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "UcWaferSelectionView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "LotRunForcedDoneView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "WaferMapMaker", 1000, true));
                
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "PMIManualResultView", 1000, true)); // ?

                info.Add(MakeViewModelInfo("ProberViewModel.dll", "UcTestSetupPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "OCRSubSettingView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "TemperatureCalView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "ProbingSystemSettingView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "FileSystemView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "ChuckPlanarityView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "ChuckPlanaritySubSettingView", 1000, true));
                
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "OperatorView", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "WaferAlignSysSubView", 1000, true));

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
