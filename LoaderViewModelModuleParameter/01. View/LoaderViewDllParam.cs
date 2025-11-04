using CommunicationConnectViewModule;
using GPCardChangeMainPageView;
using GPCognexOCRMainPageView;
using LoaderFileTransferViewModule;
using LoaderManualModuleOPViewModule;
using LoaderStageSummaryViewModule;
using LogModule;
using MultiManualContact;
using Newtonsoft.Json;
using PadSettingView_Standard;
using PMISettingSubView;
using ProberErrorCode;
using ProberInterfaces;
using ProberViewModel;
using ProberViewModel.View;
using ProberViewModel.View.Chiller;
using ProberViewModel.View.E84;
using ProberViewModel.View.EnvMonitoring;
using ProberViewModel.View.GPLoaderSetupWithCell;
using ProberViewModel.View.LogCollect;
using ProberViewModel.View.SubSetting.Device.ResultMap;
using ProberViewModel.View.TesterInterface;
using ProberViewModel.View.UtilityOption;
using ProberViewModel.View.VerifyParam;
using ProberViewModel.View.Wafer;
using SequenceMakerScreen;
using SettingTemplateView;
using SoakingSettingView;
using StatusSoakingRecipeSettingView;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TCPIP;
using TemperatureCalViewProject;
using TesterCommnuication;
using UcChillerScreen;
using UcContactSettingView;
using UcDeviceChangeView;
using UcGpibSettingView;
using UcMotorsView;
using UcPinBasicInfoView;
using UcPolishWaferMakeSourceView;
using UcPolishWaferRecipeSettingView;
using UcPolishWaferSourceSettingView;
using UcTempDeviationView;
using UcTempDeviceSettingView;
using UcTouchSensorView;
using UcWaferRecipeSettingView;
using WASettingView_Standard;

namespace LoaderViewModelModuleParameter
{
    [Serializable]
    public class LoaderViewDLLParam : IParam, ISystemParameterizable
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

        //private string _ParamLabel;

        //public string Genealogy
        //{
        //    get { return _ParamLabel; }
        //    set { _ParamLabel = value; }
        //}


        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }

        public List<ModuleDllInfo> info = new List<ModuleDllInfo>();

        public string FilePath { get; } = "";

        public string FileName { get; } = "LoaderProberView.json";


        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LoaderViewDLLParam] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }

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
        private ModuleDllInfo MakeModuleDllInfo(string assemblyname, int version, bool enablebackwardcompatibility, bool remoteflag)
        {
            ModuleDllInfo tmp = new ModuleDllInfo();
            try
            {

                tmp.AssemblyName = assemblyname;
                tmp.Version = version;
                tmp.EnableBackwardCompatibility = enablebackwardcompatibility;
                tmp.RemoteFlag = remoteflag;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return tmp;
        }

        private ModuleDllInfo MakeViewModelInfo(string assemblyname, string classname, int version, bool enablebackwardcompatibility, bool remoteflag = false)
        {
            ModuleDllInfo tmp = new ModuleDllInfo();

            try
            {
                tmp.AssemblyName = assemblyname;
                tmp.ClassName.Add(classname);
                tmp.Version = version;
                tmp.EnableBackwardCompatibility = enablebackwardcompatibility;
                tmp.RemoteFlag = remoteflag;
            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return tmp;
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        public EventCodeEnum SetDefaultParam()
        {
            var dllname = "ProberViewModel.dll";

            // Main
            //info.Add(MakeViewModelInfo("ProberViewModel", nameof(LoaderMainView), 1000, true));

            //PNP
            info.Add(MakeViewModelInfo(dllname, nameof(UcPnpControl), 1000, true, true));

            //Handling
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderHandlingView), 1000, true));

            //Handling - GOP
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderHandlingView_GOP), 1000, true));

            info.Add(MakeViewModelInfo(dllname, nameof(LoaderHandlingView_6X2), 1000, true));
            //Operate
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderOperateView), 1000, true));
            //Module Operate( Technologies)
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderManualModuleOPView), 1000, true));
            //Loader ParameterSetting
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderParameterSettingView.LoaderParameterSettingView), 1000, true));
            //Cognex OCR
            info.Add(MakeViewModelInfo(dllname, nameof(UcGPCognexOCRMainPage), 1000, true));
            //Connect
            info.Add(MakeViewModelInfo(dllname, nameof(CommunicationConnectView), 1000, true));
            ////Polish Wafer
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderPolishWaferView), 1000, true));
            //Manual Contact
            info.Add(MakeViewModelInfo(dllname, nameof(ManualContactControl), 1000, true, true));
            //Inspection
            info.Add(MakeViewModelInfo(dllname, nameof(InspectionControl), 1000, true, true));
            //Device Change
            info.Add(MakeViewModelInfo(dllname, nameof(DeviceChangeView), 1000, true, true));
            //Temp. Calibration
            info.Add(MakeViewModelInfo(dllname, nameof(TemperatureCalView), 1000, true, true));
            //Motion
            info.Add(MakeViewModelInfo(dllname, nameof(MotorsView), 1000, true, true));
            //System Parametr
            info.Add(MakeViewModelInfo(dllname, nameof(SettingTemplatePage), 1000, true, true));
            //Recipe Editor
            info.Add(MakeViewModelInfo(dllname, nameof(UcRecipeEditorMainPage), 1000, true, true));
            // Soaking Recipe
            info.Add(MakeViewModelInfo(dllname, nameof(SoakingRecipeSettingView.SoakingRecipeSettingView), 1000, true, true));

            // TopBar
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderTopBarView), 1000, true, true));

            // MainMenu
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderMainMenuControl.LoaderMainMenuControl), 1000, true, true));

            #region //..Wafer

            //Wafer Recipe Setting View
            info.Add(MakeViewModelInfo(dllname, nameof(WaferRecipeSettingView), 1000, true));
            // Create Wafer Map
            info.Add(MakeViewModelInfo(dllname, nameof(WaferMapMaker), 1000, true, true));
            #endregion
            // TODO : REMOVE, For Preview Pin Basic Info
            info.Add(MakeViewModelInfo(dllname, nameof(PinBasicInfoView), 1000, true, true));

            // Polish Wafer SubSetting
            info.Add(MakeViewModelInfo(dllname, nameof(PolishWaferSourceSettingView), 1000, true, true));

            // Polish Wafer Type Setting
            info.Add(MakeViewModelInfo(dllname, nameof(PolishWaferMakeSourceView), 1000, true, true));

            // Polish Wafer Interval Setting
            info.Add(MakeViewModelInfo(dllname, nameof(PolishWaferRecipeSettingView), 1000, true, true));

            // PMI Setting
            info.Add(MakeViewModelInfo(dllname, nameof(PMISettingView.PMISettingView), 1000, true, true));

            // File Transfer
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderFileTransferView), 1000, true));

            // Dut Editor Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(DutEditorSubView.DutEditorSubView), 1000, true, true));

            // Dut Editor
            info.Add(MakeViewModelInfo(dllname, nameof(UCDutEditor.UCDutEditor), 1000, true, true));

            // Pin Align Setting
            info.Add(MakeViewModelInfo(dllname, nameof(PinAlignSettingViewPnP.PinAlignSettingViewPnP), 1000, true, true));

            // Contact Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ContactSettingView), 1000, true, true));

            //GPCCOP
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderGPCardChangeOPView), 1000, true));

            info.Add(MakeViewModelInfo(dllname, nameof(LoaderGPCardChangeOPView_6X2), 1000, true, true));

            // Stage Summary
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderStageSummaryView), 1000, true));

            // Stage Summary - GOP
            info.Add(MakeViewModelInfo(dllname, nameof(LoaderStageSummary_GOP), 1000, true));

            info.Add(MakeViewModelInfo(dllname, nameof(LoaderStageSummary_6X2), 1000, true));

            //GPCCOvservation
            info.Add(MakeViewModelInfo(dllname, nameof(UcGPCardChangeMainPage), 1000, true, true));

            info.Add(MakeViewModelInfo(dllname, nameof(UcGPCardChangeMainPage_DRAX), 1000, true, true));

            //GPLoaderSetupWithCell
            info.Add(MakeViewModelInfo(dllname, nameof(GPLoaderSetupWithCellView), 1000, true, true));

            // Probing Sequence
            info.Add(MakeViewModelInfo(dllname, nameof(SequenceMaker), 1000, true, true));

            // Probing Sequence Sub
            info.Add(MakeViewModelInfo(dllname, nameof(ProbingSequenceSubView.ProbingSequenceSubView), 1000, true));

            // WASetting View
            info.Add(MakeViewModelInfo(dllname, nameof(WASettingView), 1000, true, true));

            // PadSetting View
            info.Add(MakeViewModelInfo(dllname, nameof(PadSettingView), 1000, true, true));

            // Cognex OCR Sub View
            info.Add(MakeViewModelInfo(dllname, nameof(CognexOCRSubSettingView.CognexOCRSubSettingView), 1000, true, true));

            // Chuck Planarity
            info.Add(MakeViewModelInfo(dllname, nameof(ChuckPlanarityView.ChuckPlanarityView), 1000, true, true));

            #region Utility

            // PMI Viewer
            info.Add(MakeViewModelInfo(dllname, nameof(PMIViewerPage), 1000, true));

            // LogCollectControl
            info.Add(MakeViewModelInfo(dllname, nameof(LogCollectControl), 1000, true));

            //Foup Recovery Viewer
            info.Add(MakeViewModelInfo(dllname, nameof(GPFocupRecoveryControlView), 1000, true));

            #endregion

            #region //.. System Cell
            // Chuck Planarity Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ChuckPlanaritySubSettingView.ChuckPlanaritySubSettingView), 1000, true));

            // Mark Planarity Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(MarkSettingSubView), 1000, true));

            // Probing System Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ProbingSystemSettingView.ProbingSystemSettingView), 1000, true));

            // File System Setting
            info.Add(MakeViewModelInfo(dllname, nameof(FileSystemView.FileSystemView), 1000, true));

            // Soaking System Setting
            info.Add(MakeViewModelInfo(dllname, nameof(SoakingSystemView.SoakingSystemView), 1000, true));

            #endregion

            // TempDeviation
            info.Add(MakeViewModelInfo(dllname, nameof(TempDeviationView), 1000, true, true));

            // TempSet
            info.Add(MakeViewModelInfo(dllname, nameof(TempDeviceSettingView), 1000, true, true));

            //Chiller
            info.Add(MakeViewModelInfo(dllname, nameof(GPChillerMainView), 1000, true, false));

            info.Add(MakeViewModelInfo(dllname, nameof(GPChillerMainView_6X2), 1000, true, false));
            //ChillerSetup
            info.Add(MakeViewModelInfo(dllname, nameof(GpChillerStageSetupView), 1000, true, false));
            // Chiller OP
            info.Add(MakeViewModelInfo(dllname, nameof(UcHBChiller), 1000, true));

            // Tester communication Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(TesterCommnuicationSettingView), 1000, true));

            // TCP/IP Setup Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(TCPIPSettingView), 1000, true));

            // Gpib
            info.Add(MakeViewModelInfo(dllname, nameof(GpibSettingView), 1000, true));

            // ResultMap - Converter Sub Setting
            info.Add(MakeViewModelInfo(dllname, nameof(ResultMapConverterView), 1000, true));

            // E84ControlView
            info.Add(MakeViewModelInfo(dllname, nameof(E84ControlView), 1000, true));

            //EnvMonitoringView
            info.Add(MakeViewModelInfo(dllname, nameof(EnvMonitoringView), 1000, true));

            // VerifyParameterView
            info.Add(MakeViewModelInfo(dllname, nameof(VerifyParameterView), 1000, true));

            // MultiContact
            info.Add(MakeViewModelInfo(dllname, nameof(MultiManualContactView), 1000, true, false));

            // GPUtilityOption
            info.Add(MakeViewModelInfo(dllname, nameof(GPUtilityOptionView), 1000, true));

            //TesterInterface
            info.Add(MakeViewModelInfo(dllname, nameof(TesterInterfaceView), 1000, true));

            //Touch Sensor Setup View
            info.Add(MakeViewModelInfo(dllname, nameof(TouchSensorView), 1000, true));

            // Retest
            info.Add(MakeViewModelInfo(dllname, nameof(RetestSettingSubView.RetestSettingSubView), 1000, true));

            //Login
            info.Add(MakeViewModelInfo(dllname, nameof(LoginControl.LoginControl), 1000, true));

            //Account
            info.Add(MakeViewModelInfo(dllname, nameof(UcAccountView.AccountView), 1000, true));

            //Manual Soaking
            info.Add(MakeViewModelInfo(dllname, nameof(ManualSoakingView), 1000, true, true));

            //Uc Card Change Setting
            info.Add(MakeViewModelInfo(dllname, nameof(UcCardChangeSettingView), 1000, true, true));

            #region Status Soaking
            // Status Soaking Recipe Setting
            info.Add(MakeViewModelInfo(dllname, nameof(StatusSoakingSettingView), 1000, true, true));

            // Soaking Step
            info.Add(MakeViewModelInfo(dllname, nameof(UcSoakingStep), 1000, true));

            // Soaking Polish Wafer
            info.Add(MakeViewModelInfo(dllname, nameof(UcSoakingPolishWafer), 1000, true));

            // Soaking OD
            info.Add(MakeViewModelInfo(dllname, nameof(UcSoakingOD), 1000, true));
            #endregion

            // Wafer Align System
            info.Add(MakeViewModelInfo(dllname, nameof(WaferAlignSysSubView), 1000, true));

            return EventCodeEnum.NONE;

        }
    }
}
