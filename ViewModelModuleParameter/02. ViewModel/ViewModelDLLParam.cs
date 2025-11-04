using AccountVM;
using CardChangeScreenVM;
using ChillerScreenViewModel;
using ChuckPlanaritySubSettingViewModel;
using ChuckPlanarityViewModel;
using ChuckTiltingViewModel;
using CleanUnitVM;
using CognexOCRMainPageViewModel;
using ContactSettingVM;
using DeviceChangeVM;
using DeviceSettingMainViewModel;
using DeviceUpDownMainPageViewModel;
using DeviceUpDownSubSettingVM;
using DutEditorPageViewModel;
using FileSystemVIewModel;
using ForcedDoneViewModel;
using FoupControlViewModel;
using FoupReoveryViewModel;
using GemSysSettingVM;
using GPCardChangeMainPageViewModel;
using GPCCAlignSettingViewModel_Standard;
using GpibSettingVM;
using InspectionControlViewModel;
using IOPanelViewModel;
using LoaderSetupViewModel;
using LoaderStatusSoakingSettingVM;
using LoginControlViewModel;
using LogModule;
using LogSettingVM;
using LogViewerVM;
using LotScreenViewModel;
using MainTopBarControlViewModel;
using ManualJogViewModel;
using MappingVM;
using MaskingSettingControlViewModel;
using MonitoringMainPageViewModel;
using MotorsLoaderVM;
using MotorsVM;
using NCPadChangeScreenVM;
using NeedleCleanManualPageViewModel;
using NeedleCleanRecipeSettingVM;
using Newtonsoft.Json;
using OCRViewModel;
using OPUSV3DViewModel;
using PathMakerControlViewModel;
using PinAlignIntervalSettingVM;
using PinBasicInfoVM;
using PMIManualResultVM;
using PMISettingSubViewModel;
using PMIViewerViewModel;
using PnpControlViewModel;
using PolishWaferMakeSourceVM;
using PolishWaferRecipeSettingVM;
using PolishWaferSettingViewModel;
using PolishWaferSourceSettingVM;
using ProberErrorCode;
using ProberInterfaces;
using ProberViewModel;
using ProberViewModel.ViewModel;
using ProberViewModel.ViewModel.ResultMap;
using ProbingSystemSettingViewModel;
using SamplePinAlignSetteingVM;
using SoakingSettingViewModel;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SystemSettingMainViewModel;
using TaskManagementViewModel;
using TempDeviationVM;
using TempDeviceSettingVM;
using TemperatureCalViewModelProject;
using TestHeadDockScreenVM;
using TestSetupDialog;
using VisionTestViewModel;
using WaferHandlingViewModel;
using WaferMapMakerViewModel;
using WaferRecipeSettingVM;
using WaferSelectionViewModel;
using TouchSensorVM;

namespace ViewModelModuleParameter
{
    [Serializable]
    public class ViewModelDLLParam : IParam, ISystemParameterizable
    {
        public List<ModuleDllInfo> info = new List<ModuleDllInfo>();
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; } = "";

        public string FileName { get; } = "ProberViewModel.json";

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
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
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
         = new List<object>();

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

        public EventCodeEnum SetDefaultParam()
        {
            var dllname = "ProberViewModel.dll";

            // Card Change
            info.Add(MakeViewModelInfo(dllname, nameof(CardChangeScreenViewModel), 1000, true));
            // NC Pad Change
            info.Add(MakeViewModelInfo(dllname, nameof(NCPadChangeScreenViewModel), 1000, true));
            // Test head Change
            info.Add(MakeViewModelInfo(dllname, nameof(TestHeadDockScreenViewModel), 1000, true));
            // Chiller
            info.Add(MakeViewModelInfo(dllname, nameof(HBChillerBase), 1000, true));
            // DeviceUpDown
            info.Add(MakeViewModelInfo(dllname, nameof(VmDeviceUpDownMainPage), 1000, true));
            // DutEditor
            info.Add(MakeViewModelInfo(dllname, nameof(VmDutEditorPage), 1000, true));
            // Foup
            info.Add(MakeViewModelInfo(dllname, nameof(FoupControlVM), 1000, true));
            // IO
            info.Add(MakeViewModelInfo(dllname, nameof(IOPanelVM), 1000, true));
            // Login
            info.Add(MakeViewModelInfo(dllname, nameof(LoginControlVM), 1000, true));
            // Lot
            info.Add(MakeViewModelInfo(dllname, nameof(LotScreenVM), 1000, true));
            // PnP
            info.Add(MakeViewModelInfo(dllname, nameof(PnpControlVM), 1000, true));
            // PolishWafer
            info.Add(MakeViewModelInfo(dllname, nameof(PolishWaferSettingVM) , 1000, true));

            // RecipeEditor
            info.Add(MakeViewModelInfo(dllname, nameof(VmRecipeEditorMainPage), 1000, true));
            // Sequence
            info.Add(MakeViewModelInfo(dllname, nameof(SequenceMakerVM), 1000, true));
            // TaskManagement
            info.Add(MakeViewModelInfo(dllname, nameof(TaskManageMentVM), 1000, true)); 
            // Soaking
            info.Add(MakeViewModelInfo(dllname, nameof(SoakingSettingBase), 1000, true));
            // TopBar
            info.Add(MakeViewModelInfo(dllname, nameof(MainTopBarControlVM), 1000, true));

            // OperatorView
            info.Add(MakeViewModelInfo(dllname, nameof(OperatorControl), 1000, true));

            // PMI Viewer
            info.Add(MakeViewModelInfo(dllname, nameof(PMIViewerVM), 1000, true));
            //Foup Recovery Vieer
            info.Add(MakeViewModelInfo(dllname, nameof(GP_FoupRecoveryControlViewModel), 1000, true));

            //ManualJog
            info.Add(MakeViewModelInfo(dllname, nameof(ManualJogViewModelBase), 1000, true));
            // OCR
            info.Add(MakeViewModelInfo(dllname, nameof(OCRSettingBase), 1000, true));
            // WaferHandling
            info.Add(MakeViewModelInfo(dllname, nameof(WaferHandlingVM), 1000, true));
            //Monitoring
            info.Add(MakeViewModelInfo(dllname, nameof(VmMonitoringMainPage), 1000, true));
            // VisionTest
            info.Add(MakeViewModelInfo(dllname, nameof(VisionTestViewModelBase), 1000, true));
            // ManualContact
            info.Add(MakeViewModelInfo(dllname, nameof(ManualContactControlVM), 1000, true));
            // Inspection
            info.Add(MakeViewModelInfo(dllname, nameof(InspectionControlVM), 1000, true));
            // MaskingSetting
            info.Add(MakeViewModelInfo(dllname, nameof(MaskingSettingControlVM), 1000, true));
            // Path Maker Page
            info.Add(MakeViewModelInfo(dllname, nameof(PathMakerControlVM), 1000, true));
            // System Setting MainView Page
            info.Add(MakeViewModelInfo(dllname, nameof(SystemSettingMainVM), 1000, true));
            // Device Setting MainView Page
            info.Add(MakeViewModelInfo(dllname, nameof(DeviceSettingMainVM), 1000, true));
            // Setting Tempalte View Page
            info.Add(MakeViewModelInfo(dllname, nameof(SettingTemplateVM), 1000, true));
            // Manual NC
            info.Add(MakeViewModelInfo(dllname, nameof(VmNeedleCleanManualPage), 1000, true));
            // Device Change
            info.Add(MakeViewModelInfo(dllname, nameof(DeviceChangeViewModel), 1000, true));
            // Alarms
            info.Add(MakeViewModelInfo(dllname, nameof(AlarmsViewModel.AlarmsViewModel), 1000, true));
            // Chuck Tilting
            info.Add(MakeViewModelInfo(dllname, nameof(ChuckTiltingVM), 1000, true));
            #region Sub Screen(Setting)
            // Retest
            info.Add(MakeViewModelInfo(dllname, nameof(RetestSettingViewModel.RetestSettingViewModel), 1000, true));
            // Gpib
            info.Add(MakeViewModelInfo(dllname, nameof(GpibSettingViewModel), 1000, true));
            // GPCCAlignSettingViewModel_Standard
            info.Add(MakeViewModelInfo(dllname, nameof(GPCCAlignSettingViewModel), 1000, true));
            // GPCCAlign OP
            info.Add(MakeViewModelInfo(dllname, nameof(GPCardChangeOPViewModelModule.GPCardChangeOPViewModelModule), 1000, true));
            // GEM
            info.Add(MakeViewModelInfo(dllname, nameof(GemSysSettingViewModel), 1000, true));
            // Needle Clean Recipe View Model
            info.Add(MakeViewModelInfo(dllname, nameof(NeedleCleanRecipeSettingViewModel), 1000, true));
            // Wafer Recipe Setting View Model
            info.Add(MakeViewModelInfo(dllname, nameof(WaferRecipeSettingViewModel), 1000, true));
            // PinAlign Setting View Model PnP
            info.Add(MakeViewModelInfo(dllname, nameof(PinAlignSettingViewModelPnP.PinAlignSettingViewModelPnP), 1000, true));
            // PinAlign Setting View Model
            info.Add(MakeViewModelInfo(dllname, nameof(PinAlignSettingVM.PinAlignSettingVM), 1000, true));
            // Sample PinAlign Setting View Model
            info.Add(MakeViewModelInfo(dllname, nameof(SamplePinAlignSettingViewModel), 1000, true));
            // PinAlign Interval Setting View Model
            info.Add(MakeViewModelInfo(dllname, nameof(PinAlignIntervalSettingViewModel), 1000, true));
            // Mark
            info.Add(MakeViewModelInfo(dllname, nameof(MarkSettingSubViewModel), 1000, true));
            // OCR - COGNEX
            info.Add(MakeViewModelInfo(dllname, nameof(CognexOCRSubSettingViewModel.CognexOCRSubSettingViewModel), 1000, true));
            // PMI
            info.Add(MakeViewModelInfo(dllname, nameof(PMISettingViewModel.PMISettingViewModel), 1000, true));
            // MachineSetting - Stage
            info.Add(MakeViewModelInfo(dllname, nameof(MachineSettingStageViewModel.MachineSettingStageViewModel), 1000, true));
            // MachineSetting - Loader
            info.Add(MakeViewModelInfo(dllname, nameof(MachineSettingLoaderViewModel.MachineSettingLoaderViewModel), 1000, true));
            // Vision Mapping
            info.Add(MakeViewModelInfo(dllname, nameof(VisionMappingSubViewModel.VisionMappingSubViewModel), 1000, true));
            // IO
            info.Add(MakeViewModelInfo(dllname, nameof(IOSubViewModel.IOSubViewModel), 1000, true));
            // FOUP
            info.Add(MakeViewModelInfo(dllname, nameof(FoupSubViewModel.FoupSubViewModel), 1000, true));
            // Masking
            info.Add(MakeViewModelInfo(dllname, nameof(MaskingSubViewModel.MaskingSubViewModel), 1000, true));
            // Dut Editor
            info.Add(MakeViewModelInfo(dllname, nameof(DutEditorSubViewModel.DutEditorSubViewModel), 1000, true));
            // Probing Seq
            info.Add(MakeViewModelInfo(dllname, nameof(ProbingSequenceSubViewModel.ProbingSequenceSubViewModel), 1000, true));
            //Log Setting ViewModel
            info.Add(MakeViewModelInfo(dllname, nameof(LogSettingViewModel), 1000, true));
            //Log Viewer ViewModel
            info.Add(MakeViewModelInfo(dllname, nameof(LogViewerViewModel), 1000, true));
            // Account
            info.Add(MakeViewModelInfo(dllname, nameof(AccountViewModel), 1000, true));
            // Alarms
            info.Add(MakeViewModelInfo(dllname, nameof(AlarmsSubViewModel.AlarmsSubViewModel), 1000, true));
            // OPUSV3D
            info.Add(MakeViewModelInfo(dllname, nameof(OPUSV3DVM), 1000, true));
            //Motors ViewModel
            info.Add(MakeViewModelInfo(dllname, nameof(MotorsViewModel), 1000, true));
            //Clean Unit ViewModel
            info.Add(MakeViewModelInfo(dllname, nameof(CleanUnitViewModel), 1000, true));
            //Touch Sensor Setup ViewModel
            info.Add(MakeViewModelInfo(dllname, nameof(TouchSensorViewModel), 1000, true));
            // ContactSetting
            info.Add(MakeViewModelInfo(dllname, nameof(ContactSettingViewModel), 1000, true));
            // Motors ( Loader )
            info.Add(MakeViewModelInfo(dllname, nameof(MotorsLoaderViewModel), 1000, true));
            // Pin Basic information
            info.Add(MakeViewModelInfo(dllname, nameof(PinBasicInfoViewModel), 1000, true));
            // Temp Device Setting
            info.Add(MakeViewModelInfo(dllname, nameof(TempDeviceSettingViewModel), 1000, true));
            // Mapping
            info.Add(MakeViewModelInfo(dllname, nameof(MappingViewModel), 1000, true));
            // DeviceUpDown
            info.Add(MakeViewModelInfo(dllname, nameof(DeviceUpDownSubSettingViewModel), 1000, true));
            // Temperature Deviation
            info.Add(MakeViewModelInfo(dllname, nameof(TempDeviationViewModel), 1000, true));
            // Soaking
            info.Add(MakeViewModelInfo(dllname, nameof(SoakingRecipeSettingViewModel.SoakingRecipeSettingViewModel), 1000, true));

            // PolishWafer Recipe Setting
            info.Add(MakeViewModelInfo(dllname, nameof(PolishWaferRecipeSettingViewModel), 1000, true));
            
            // PolishWafer Source
            info.Add(MakeViewModelInfo(dllname, nameof(PolishWaferSourceSettingViewModel), 1000, true));

            info.Add(MakeViewModelInfo(dllname, nameof(PolishWaferMakeSourceViewModel), 1000, true));

            
            #endregion
            // LoaderSetup
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderSetupViewModelBase), 1000, true));
            // wafer selection
            info.Add(MakeViewModelInfo(dllname, nameof(WaferSelectionVM), 1000, true));
            // ForcedDone
            info.Add(MakeViewModelInfo(dllname, nameof(LotRunForcedDoneViewModel), 1000, true));
            // Chuck Tilting
            info.Add(MakeViewModelInfo(dllname, nameof(ChuckTiltingSubViewModel.ChuckTiltingSubViewModel), 1000, true));
            // Create Wafer Map
            info.Add(MakeViewModelInfo(dllname, nameof(WaferMapMakerVM), 1000, true));
            // Manual PMI Page
            info.Add(MakeViewModelInfo(dllname, nameof(PMIManualResultViewModel), 1000, true));
            // Test Setup Page
            info.Add(MakeViewModelInfo(dllname, nameof(VmTestSetupPage), 1000, true));
            // Cognex OCR
            info.Add(MakeViewModelInfo(dllname, nameof(VmCognexOCRMainPage), 1000, true));
            // Semics OCR
            info.Add(MakeViewModelInfo(dllname, nameof(SemicsOCRSubSettingViewModel.SemicsOCRSubSettingViewModel), 1000, true));
            // Common OCR
            info.Add(MakeViewModelInfo(dllname, nameof(OCRSubSettingViewModel.OCRSubSettingViewModel), 1000, true));
            info.Add(MakeViewModelInfo(dllname, nameof(VmGPCardChangeMainPage), 1000, true));
            // Temperature Calibration
            info.Add(MakeViewModelInfo(dllname, nameof(TemperatureCalViewModel), 1000, true));
            // Probing System Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ProbingSystemSettingVM), 1000, true));

            // File System Setting
            info.Add(MakeViewModelInfo(dllname, nameof(FileSystemVM), 1000, true));
            // Soaking System Setting
            info.Add(MakeViewModelInfo(dllname, nameof(SoakingSystemViewModel.SoakingSystemVM), 1000, true));

            // Chuck Planarity
            info.Add(MakeViewModelInfo(dllname, nameof(ChuckPlanarityVM), 1000, true));
            // Chuck Planarity Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ChuckPlanaritySubSettingVM), 1000, true));

            // Tester communication Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(TesterCommunicationSettingViewModel.TesterCommunicationSettingViewModel), 1000, true));

            // TCP/IP Setup Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(TCPIPSettingViewModel.TCPIPSettingViewModel), 1000, true));

            // BIN Analyze
            info.Add(MakeViewModelInfo(dllname, nameof(BINAnalyzeViewModel), 1000, true));

            // Result Map Analyze
            info.Add(MakeViewModelInfo(dllname, nameof(ResultMapAnalyzeViewerVM), 1000, true));

            // STIP Map Analyze
            info.Add(MakeViewModelInfo(dllname, nameof(STIFMapAnalyzeVM), 1000, true));

            // E142 Analyze
            info.Add(MakeViewModelInfo(dllname, nameof(E142MapAnalyzeVM), 1000, true));

            // ResultMap - Converter Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ResultMapConverterVM), 1000, true));

            //ManualSoakingViewModel
            info.Add(MakeViewModelInfo(dllname, nameof(ManualSoakingViewModel), 1000, false));

            //UcCardChangeSettingViewModel
            info.Add(MakeViewModelInfo(dllname, nameof(UcCardChangeSettingViewModel), 1000, false));

            #region Status Soaking
            // Status Soaking Recipe Setting
            info.Add(MakeViewModelInfo(dllname, nameof(StatusSoakingSettingViewModel), 1000, true));

            // Soaking Step
            info.Add(MakeViewModelInfo(dllname, nameof(UcSoakingStepViewModel), 1000, true));

            // Soaking Polish Wafer
            info.Add(MakeViewModelInfo(dllname, nameof(UcSoakingPolishWaferViewModel), 1000, true));

            // Soaking OD
            info.Add(MakeViewModelInfo(dllname, nameof(UcSoakingODViewModel), 1000, true));
            #endregion

            #region Digital Twin

            info.Add(MakeViewModelInfo(dllname, nameof(DigitalTwinViewModel), 1000, true));
            info.Add(MakeViewModelInfo(dllname, nameof(DigitalTwinSettingViewModel), 1000, true));

            #endregion

            // Accuracy Check
            info.Add(MakeViewModelInfo(dllname, nameof(AccuracyCheckSetupViewModel), 1000, true));
            info.Add(MakeViewModelInfo(dllname, nameof(AccuracyCheckSubSettingViewModel), 1000, true));

            // Wafer Align System
            info.Add(MakeViewModelInfo(dllname, nameof(WaferAlignSysSubViewModel), 1000, true));

            return EventCodeEnum.NONE;
        }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            // TODO : Temporary code
            //return MakeDefaultParamSetFor_BSCI();

            return SetDefaultParam();
        }
        public void SetElementMetaData()
        {

        }

        /// <summary>
        /// BSCI에서 사용할 디폴트 파라미터 세트를 만들어주는 함수
        /// </summary>
        private EventCodeEnum MakeDefaultParamSetFor_BSCI()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "VisionTestViewModelBase", 1000, true));

                info.Add(MakeViewModelInfo("ProberViewModel.dll", "CardChangeScreenViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "TestHeadDockScreenViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "NCPadChangeScreenViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "VmDeviceUpDownMainPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "VmDutEditorPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "IOPanelViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "LoginControlVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "LotScreenVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "PnpControlVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "VmRecipeEditorMainPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "SequenceMakerVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "TaskManageMentVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "VmTestSetupPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "WaferHandlingVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "ManualJogViewModelBase", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "OCRSettingBase", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "VmMonitoringMainPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "ManualContactControlVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "InspectionControlVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MaskingSettingControlVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "VmCognexOCRMainPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "VmNeedleCleanManualPage", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "SystemSettingMainVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "DeviceSettingMainVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "SettingTemplateVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "DeviceChangeViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "GpibSettingViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "PinAlignSettingVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "NeedleCleanRecipeSettingViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "WaferRecipeSettingViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MarkSettingSubViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "PMISettingViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "CognexOCRSubSettingViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MachineSettingStageViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MachineSettingLoaderViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "VisionMappingSubViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "IOSubViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "FoupSubViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "FoupControlVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MaskingSubViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "DutEditorSubViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "ProbingSequenceSubViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "LogSettingViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "LogViewerViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "AccountViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MotorsViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "CleanUnitViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "TouchSensorViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "ContactSettingViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MotorsLoaderViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "PinBasicInfoViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "TempDeviceSettingViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "DeviceUpDownSubSettingViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "TempDeviationViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "LoaderSetupViewModelBase", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "WaferSelectionVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "LotRunForcedDoneViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "WaferMapMakerVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MappingViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "PMIManualResultViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "TemperatureCalViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "ProbingSystemSettingVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "FileSystemVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "SoakingSystemVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "ChuckPlanarityVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "ChuckPlanaritySubSettingVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "MainTopBarControlVM", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "OCRSubSettingViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "PinAlignSettingViewModelPnP", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "PinAlignIntervalSettingViewModel", 1000, true));
                info.Add(MakeViewModelInfo("ProberViewModel.dll", "WaferAlignSysSubViewModel", 1000, true));

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
