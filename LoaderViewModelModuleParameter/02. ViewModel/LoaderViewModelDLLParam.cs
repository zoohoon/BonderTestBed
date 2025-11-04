using ChillerScreenViewModel;
using ChuckPlanaritySubSettingViewModel;
using CommunicationConnectViewModelModule;
using ContactSettingVM;
using FileSystemVIewModel;
using FoupReoveryViewModel;
using GP_MarkAlignSettingViewModel;
using GPCognexOCRMainPageViewModel;
using LoaderChuckPlanarityViewModel;
using LoaderDeviceChangeViewModelModule;
using LoaderDutEditorViewModelModule;
using LoaderFileTransferViewModelModule;
using LoaderHandlingViewModelModule;
using LoaderInspectionViewModelModule;
using LoaderMainMenuViewModel;
using LoaderManualContactViewModelModule;
using LoaderManualModuleOPViewModelModule;
using LoaderOperateViewModelModule;
using LoaderParameterSettingView;
using LoaderPolishWaferMakeSourceViewModelModule;
using LoaderPolishWaferRecipeSettingViewModelModule;
using LoaderStageSummaryViewModelModule;
using LoaderStatusSoakingSettingVM;
using LoaderTopBarViewModelModule;
using LoaderWaferMapMakerViewModelModule;
using LoaderWaferSequenceMakerViewModelModule;
using LogModule;
using LooaderPolishWaferViewModelModule;
using MotorsVM;
using MultiManualContactVM;
using Newtonsoft.Json;
using PinBasicInfoVM;
using PMIViewerViewModel;
using PnpControlViewModel;
using PolishWaferSourceSettingVM;
using ProberErrorCode;
using ProberInterfaces;
using ProberViewModel;
using ProberViewModel.ViewModel;
using ProberViewModel.ViewModel.E84;
using ProberViewModel.ViewModel.ResultMap;
using ProberViewModel.ViewModel.UtilityOption;
using ProbingSystemSettingViewModel;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TempDeviationVM;
using TempDeviceSettingVM;
using TemperatureCalViewModelProject;
using TouchSensorVM;
using WaferRecipeSettingVM;

namespace LoaderViewModelModuleParameter
{
    [Serializable]
    //public class LoaderViewModelDLLParam : ViewModelDLLParam, IParam, ISystemParameterizable
    public class LoaderViewModelDLLParam : IParam, ISystemParameterizable
    {
        public List<ModuleDllInfo> info = new List<ModuleDllInfo>();

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }

        public string FilePath { get; } = "";

        public string FileName { get; } = "LoaderProberViewModel.json";

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
        public List<object> Nodes { get; set; } = new List<object>();

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

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public void SetElementMetaData()
        {

        }

        public EventCodeEnum SetDefaultParam()
        {
            var dllname = "ProberViewModel.dll";

            //PNP
            info.Add(MakeViewModelInfo(dllname, nameof(PnpControlVM), 1000, true));
            //Handling
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderHandlingViewModel), 1000, true));
            //Operate
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderOperateViewModel), 1000, true));
            //Module Operate( Technologies)
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderManualModuleOPViewModel), 1000, true));
            //Loader ParameterSetting
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderParameterViewModel), 1000, true));
            //Cognex OCR
            info.Add(MakeViewModelInfo(dllname, nameof(VmGPCognexOCRMainPage), 1000, true));
            //Connect
            info.Add(MakeViewModelInfo(dllname, nameof(CommunicationConnectViewModel), 1000, true));
            //Polish Wafer
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderPolishWaferViewModel), 1000, true));
            //Manual Contact
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderManualContactViewModel), 1000, true));
            //Inspection
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderInspectionViewModel), 1000, true));
            //Wafer Map
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderWaferMapMakerViewModel), 1000, true));
            ////Dut Editor
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderDutEditorViewModel), 1000, true));
            //Device Change
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderDeviceChangeViewModel), 1000, true));
            //Temp. calibration
            info.Add(MakeViewModelInfo(dllname, nameof(TemperatureCalViewModel), 1000, true));
            //Motion
            info.Add(MakeViewModelInfo(dllname, nameof(MotorsViewModel), 1000, true));
            //System Parametr
            info.Add(MakeViewModelInfo(dllname, nameof(SettingTemplateVM), 1000, true));
            //RecipeEditorMainPageViewModel
            info.Add(MakeViewModelInfo(dllname, nameof(VmRecipeEditorMainPage), 1000, true));
            // Soaking Recipe
            info.Add(MakeViewModelInfo(dllname, nameof(SoakingRecipeSettingViewModel.SoakingRecipeSettingViewModel), 1000, true));

            //TopBar
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderTopBarViewModel), 1000, true));

            // MainMenu
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderMainMenuVM), 1000, true));

            //Login
            info.Add(MakeViewModelInfo(dllname, nameof(LoginControlViewModel.LoaderLoginControlVM), 1000, true));


            // TODO : REMOVE, For Preview Pin Basic Info
            info.Add(MakeViewModelInfo(dllname, nameof(PinBasicInfoViewModel), 1000, true));
            // Polish Wafer SubSetting
            info.Add(MakeViewModelInfo(dllname, nameof(PolishWaferSourceSettingViewModel), 1000, true));
            // PolishWaferMakeSource 
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderPolishWaferMakeSourceVM), 1000, true));
            // Polish Wafer Interval Setting
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderPolishWaferRecipeSettingVM), 1000, true));
            // PMI Setting
            info.Add(MakeViewModelInfo(dllname, nameof(PMISettingViewModel.PMISettingViewModel), 1000, true));
            // File Transfer
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderFileTransferViewModel), 1000, true));
            // Dut Editor Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(DutEditorSubViewModel.DutEditorSubViewModel), 1000, true));
            // Pin Align Setting
            info.Add(MakeViewModelInfo(dllname, nameof(PinAlignSettingViewModelPnP.PinAlignSettingViewModelPnP), 1000, true));
            // Contact Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ContactSettingViewModel), 1000, true));
            //GPCCOP
            info.Add(MakeViewModelInfo(dllname, nameof(GPCardChangeOPViewModel.LoaderGPCardChangeOPViewModel), 1000, true));
            //GPCCObservationOP
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderCardChangeObservationViewModelModule.LoaderCardChangeObservationViewModelModule), 1000, true));
            //GPLoaderSetupWithCell
            info.Add(MakeViewModelInfo(dllname, nameof(GPLoaderSetupWithCellViewModel), 1000, true));
            
            // Stage Summary - Opera
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderStageSummaryViewModel), 1000, true));

            // Stage Summary - Operetta
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderStageSummaryViewModel_GOP), 1000, true));

            // Probing Sequence
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderWaferSequenceMakerViewModel), 1000, true));
            // Probing Sequence Sub
            info.Add(MakeViewModelInfo(dllname, nameof(ProbingSequenceSubViewModel.ProbingSequenceSubViewModel), 1000, true));
            // WaferRecipe Setting 
            info.Add(MakeViewModelInfo(dllname, nameof(WaferRecipeSettingViewModel), 1000, true));
            // Cognex OCR Sub View
            info.Add(MakeViewModelInfo(dllname, nameof(CognexOCRSubSettingViewModel.CognexOCRSubSettingViewModel), 1000, true));
            // Chuck Planarity
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderChuckPlanarityVM), 1000, true));
            // Chuck Planarity Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ChuckPlanaritySubSettingVM), 1000, true));

            #region Utility

            // PMI Viewer
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderPMIViewerVM), 1000, true));

            // LogCollectControl
            info.Add(MakeViewModelInfo(dllname, nameof(LogCollectViewModel), 1000, true));

            // Foup Recovery
            info.Add(MakeViewModelInfo(dllname, nameof(GP_LoaderFoupRecoveryControlViewModel), 1000, true));
            #endregion

            // Mark Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(GP_MarkAlignSettingVM), 1000, true));
            // Probing System Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ProbingSystemSettingVM), 1000, true));

            // File System Setting
            info.Add(MakeViewModelInfo(dllname, nameof(FileSystemVM), 1000, true));
            // Soaking System Setting
            info.Add(MakeViewModelInfo(dllname, nameof(SoakingSystemViewModel.SoakingSystemVM), 1000, true));

            // TempDeviation
            info.Add(MakeViewModelInfo(dllname, nameof(TempDeviationViewModel), 1000, true));
            // TempSet
            info.Add(MakeViewModelInfo(dllname, nameof(TempDeviceSettingViewModel), 1000, true));
            // WASetting View
            info.Add(MakeViewModelInfo(dllname, nameof(WASettingViewModel), 1000, true));
            // PadSetting View
            info.Add(MakeViewModelInfo(dllname, nameof(PadSettingViewModel), 1000, true));
            //Chiller
            info.Add(MakeViewModelInfo(dllname, nameof(GPChillerMainViewModel), 1000, true));
            // Chiller OP
            info.Add(MakeViewModelInfo(dllname, nameof(HBChillerBase), 1000, true));

            // Tester communication Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(TesterCommunicationSettingViewModel.TesterCommunicationSettingViewModel), 1000, true));

            // TCP/IP Setup Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(TCPIPSettingViewModel.TCPIPSettingViewModel), 1000, true));

            // Gpib
            info.Add(MakeViewModelInfo(dllname, nameof(GpibSettingVM.GpibSettingViewModel), 1000, true));

            // ResultMap - Converter Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ResultMapConverterVM), 1000, true));

            // Retest
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderRetestSettingViewModel), 1000, true));

            // E84ControlViewModel
            info.Add(MakeViewModelInfo(dllname, nameof(E84ControlViewModel), 1000, true));

            // EnvMonitoringViewModel
            info.Add(MakeViewModelInfo(dllname, nameof(EnvMonitoringViewModel), 1000, true));

            // VerifyParameterViewModel 
            info.Add(MakeViewModelInfo(dllname, nameof(VerifyParameterViewModel), 1000, true));
            //AccountViewModel
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderAccountVM.LoaderAccountViewModel), 1000, true));

            //MultiContact
            info.Add(MakeViewModelInfo(dllname, nameof(MultiManualContactViewModel), 1000, false));

            // GPUtilityOptionViewModel
            info.Add(MakeViewModelInfo(dllname, nameof(GPUtilityOptionViewModel), 1000, false));

            //TesterInterfaceVM
            info.Add(MakeViewModelInfo(dllname, nameof(TesterInterfaceVM), 1000, false));

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

            //Touch Sensor Setup ViewModel
            info.Add(MakeViewModelInfo(dllname, nameof(TouchSensorViewModel), 1000, true));
            // Wafer Align System
            info.Add(MakeViewModelInfo(dllname, nameof(WaferAlignSysSubViewModel), 1000, true));

            return EventCodeEnum.NONE;
        }

    }
}
