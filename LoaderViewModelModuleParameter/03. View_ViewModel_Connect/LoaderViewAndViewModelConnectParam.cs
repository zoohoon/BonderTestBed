using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LoaderViewModelModuleParameter
{
    using ChillerScreenViewModel;
    using ChuckPlanaritySubSettingViewModel;
    using CommunicationConnectViewModelModule;
    using CommunicationConnectViewModule;
    using ContactSettingVM;
    using FileSystemVIewModel;
    using GP_MarkAlignSettingViewModel;
    using GPCardChangeMainPageView;
    using GPCognexOCRMainPageView;
    using GPCognexOCRMainPageViewModel;
    using GpibSettingVM;
    using LoaderChuckPlanarityViewModel;
    using LoaderDeviceChangeViewModelModule;
    using LoaderDutEditorViewModelModule;
    using LoaderFileTransferViewModelModule;
    using LoaderFileTransferViewModule;
    using LoaderHandlingViewModelModule;
    using LoaderInspectionViewModelModule;
    using LoaderMainMenuViewModel;
    using LoaderManualContactViewModelModule;
    using LoaderManualModuleOPViewModelModule;
    using LoaderManualModuleOPViewModule;
    using LoaderOperateViewModelModule;
    using LoaderParameterSettingView;
    using LoaderPolishWaferMakeSourceViewModelModule;
    using LoaderPolishWaferRecipeSettingViewModelModule;
    using LoaderStageSummaryViewModelModule;
    using LoaderStageSummaryViewModule;
    using LoaderStatusSoakingSettingVM;
    using LoaderTopBarViewModelModule;
    using LoaderWaferMapMakerViewModelModule;
    using LoaderWaferSequenceMakerViewModelModule;
    using LooaderPolishWaferViewModelModule;
    using MotorsVM;
    using MultiManualContact;
    using MultiManualContactVM;
    using PadSettingView_Standard;
    using PinBasicInfoVM;
    using PMISettingSubView;
    using PMIViewerViewModel;
    using PnpControlViewModel;
    using PolishWaferSourceSettingVM;
    using FoupReoveryViewModel;
    using ProberViewModel;
    using ProberViewModel.View;
    using ProberViewModel.View.Chiller;
    using ProberViewModel.View.E84;
    using ProberViewModel.View.GPLoaderSetupWithCell;
    using ProberViewModel.View.EnvMonitoring;
    using ProberViewModel.View.LogCollect;
    using ProberViewModel.View.SubSetting.Device.ResultMap;
    using ProberViewModel.View.TesterInterface;
    using ProberViewModel.View.UtilityOption;
    using ProberViewModel.View.VerifyParam;
    using ProberViewModel.View.Wafer;
    using ProberViewModel.ViewModel;
    using ProberViewModel.ViewModel.E84;
    using ProberViewModel.ViewModel.ResultMap;
    using ProberViewModel.ViewModel.UtilityOption;
    using ProbingSystemSettingViewModel;
    using SequenceMakerScreen;
    using SettingTemplateView;
    using SoakingSettingView;
    using StatusSoakingRecipeSettingView;
    using TCPIP;
    using TempDeviationVM;
    using TempDeviceSettingVM;
    using TemperatureCalViewModelProject;
    using TemperatureCalViewProject;
    using TesterCommnuication;
    using TouchSensorVM;
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
    using ViewModelModuleParameter;
    using WaferRecipeSettingVM;
    using WASettingView_Standard;

    [Serializable]
    public class LoaderViewAndViewModelConnectParam : IParam, ISystemParameterizable
    {
        public string FilePath { get; } = "";

        public string FileName { get; } = "LoaderProberViewConnect.json";

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

        //private string _ParamLabel;

        //public string Genealogy
        //{
        //    get { return _ParamLabel; }
        //    set { _ParamLabel = value; }
        //}

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }

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
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            // View and ViewModel
            // Main
            // REMOVED
            //_ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("DD8BED78-B2A7-974E-6941-C970993050FD"), GetGuid(typeof("3491BBA7-8AF7-EE23-58DA-E028066E22A1")));

            //PNP
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcPnpControl)), GetGuid(typeof(PnpControlVM))));

            //Handling
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderHandlingView)), GetGuid(typeof(LoaderHandlingViewModel))));

            //Handling - GOP
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderHandlingView_GOP)), GetGuid(typeof(LoaderHandlingViewModel))));

            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderHandlingView_6X2)), GetGuid(typeof(LoaderHandlingViewModel))));

            //Operate
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderOperateView)), GetGuid(typeof(LoaderOperateViewModel))));
            //Module Operate( Technologies)
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderManualModuleOPView)), GetGuid(typeof(LoaderManualModuleOPViewModel))));
            //ParameterSetting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderParameterSettingView)), GetGuid(typeof(LoaderParameterViewModel))));
            //Cognex OCR
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcGPCognexOCRMainPage)), GetGuid(typeof(VmGPCognexOCRMainPage))));
            //Connect
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(CommunicationConnectView)), GetGuid(typeof(CommunicationConnectViewModel))));
            //Polish Wafer
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderPolishWaferView)), GetGuid(typeof(LoaderPolishWaferViewModel))));

            //Manual Contact
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ManualContactControl)), GetGuid(typeof(LoaderManualContactViewModel))));
            //Inspection
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(InspectionControl)), GetGuid(typeof(LoaderInspectionViewModel))));
            // Create Wafer Map
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(WaferMapMaker)), GetGuid(typeof(LoaderWaferMapMakerViewModel))));
            //// Dut Editor
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UCDutEditor.UCDutEditor)), GetGuid(typeof(LoaderDutEditorViewModel))));
            //Device Change
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(DeviceChangeView)), GetGuid(typeof(LoaderDeviceChangeViewModel))));
            //_ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("956bb44f-4b89-42b3-b21a-69f896a840fe"), GetGuid(typeof("39d5c48c-0bd7-4b6a-ab2d-113df85dce0e")));
            //Temp. Calibration
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(TemperatureCalView)), GetGuid(typeof(TemperatureCalViewModel))));
            //System settings
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(SettingTemplatePage)), GetGuid(typeof(SettingTemplateVM))));
            //Motor Settings
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(MotorsView)), GetGuid(typeof(MotorsViewModel))));
            //RecipeEditorMainPage
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcRecipeEditorMainPage)), GetGuid(typeof(VmRecipeEditorMainPage))));

            // Soaking Recipe
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(SoakingRecipeSettingView.SoakingRecipeSettingView)), GetGuid(typeof(SoakingRecipeSettingViewModel.SoakingRecipeSettingViewModel))));
            //_ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("00425A3B-902C-42BB-9501-9AA58952E57B"), GetGuid(typeof("88456861-8597-C693-3937-16DDDA228BCD")));

            // TopBar
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderTopBarView)), GetGuid(typeof(LoaderTopBarViewModel))));

            // MainMenu
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderMainMenuControl.LoaderMainMenuControl)), GetGuid(typeof(LoaderMainMenuVM))));

            //
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoginControl.LoginControl)), GetGuid(typeof(LoginControlViewModel.LoaderLoginControlVM))));

            // TODO : REMOVE, For Preview Pin Basic Info
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PinBasicInfoView)), GetGuid(typeof(PinBasicInfoViewModel))));

            // Polish Wafer SubSetting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PolishWaferSourceSettingView)), GetGuid(typeof(PolishWaferSourceSettingViewModel))));

            // PolishWaferMake Source
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PolishWaferMakeSourceView)), GetGuid(typeof(LoaderPolishWaferMakeSourceVM))));

            // Polish Wafer Interval Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PolishWaferRecipeSettingView)), GetGuid(typeof(LoaderPolishWaferRecipeSettingVM))));

            // PMI Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PMISettingView.PMISettingView)), GetGuid(typeof(PMISettingViewModel.PMISettingViewModel))));

            // File Transfer
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderFileTransferView)), GetGuid(typeof(LoaderFileTransferViewModel))));

            // Dut Editor Sub Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(DutEditorSubView.DutEditorSubView)), GetGuid(typeof(DutEditorSubViewModel.DutEditorSubViewModel))));

            // Dut Editor
            //_ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof("78744426-ef1d-4624-a961-4a756669a9b7"), GetGuid(typeof("5899DCFE-3032-5360-03D7-1F356B7A0800")));

            // Pin Align Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PinAlignSettingViewPnP.PinAlignSettingViewPnP)), GetGuid(typeof(PinAlignSettingViewModelPnP.PinAlignSettingViewModelPnP))));

            // Contact Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ContactSettingView)), GetGuid(typeof(ContactSettingViewModel))));

            // Stage Summary - OPERA
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderStageSummaryView)), GetGuid(typeof(LoaderStageSummaryViewModel))));

            // Stage Summary - GOP
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderStageSummary_GOP)), GetGuid(typeof(LoaderStageSummaryViewModel_GOP))));

            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderStageSummary_6X2)), GetGuid(typeof(LoaderStageSummaryViewModel))));

            //GPCardChangeOP
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderGPCardChangeOPView)), GetGuid(typeof(GPCardChangeOPViewModel.LoaderGPCardChangeOPViewModel))));

            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LoaderGPCardChangeOPView_6X2)), GetGuid(typeof(GPCardChangeOPViewModel.LoaderGPCardChangeOPViewModel))));
            //GPCardChangeObservation
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcGPCardChangeMainPage)), GetGuid(typeof(LoaderCardChangeObservationViewModelModule.LoaderCardChangeObservationViewModelModule))));

            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcGPCardChangeMainPage_DRAX)), GetGuid(typeof(LoaderCardChangeObservationViewModelModule.LoaderCardChangeObservationViewModelModule))));
            //GPLoaderSetupWithCell
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(GPLoaderSetupWithCellView)), GetGuid(typeof(GPLoaderSetupWithCellViewModel))));

            // Probing Sequence
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(SequenceMaker)), GetGuid(typeof(LoaderWaferSequenceMakerViewModel))));

            // Probing Sequence Sub
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ProbingSequenceSubView.ProbingSequenceSubView)), GetGuid(typeof(ProbingSequenceSubViewModel.ProbingSequenceSubViewModel))));

            // WaferRecipe Setting 
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(WaferRecipeSettingView)), GetGuid(typeof(WaferRecipeSettingViewModel))));
            
            // WASetting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(WASettingView)), GetGuid(typeof(WASettingViewModel))));

            // PadSetting View
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PadSettingView)), GetGuid(typeof(PadSettingViewModel))));

            // Cognex OCR Sub View
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(CognexOCRSubSettingView.CognexOCRSubSettingView)), GetGuid(typeof(CognexOCRSubSettingViewModel.CognexOCRSubSettingViewModel))));

            // Chuck Plnarity
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ChuckPlanarityView.ChuckPlanarityView)), GetGuid(typeof(LoaderChuckPlanarityVM))));

            #region Utility

            // PMI Viewer
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(PMIViewerPage)), GetGuid(typeof(LoaderPMIViewerVM))));
            
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcAccountView.AccountView)), GetGuid(typeof(LoaderAccountVM.LoaderAccountViewModel))));

            // LogCollectControl
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(LogCollectControl)), GetGuid(typeof(LogCollectViewModel))));
            // Foup Recovery Viewer
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(GPFocupRecoveryControlView)), GetGuid(typeof(GP_LoaderFoupRecoveryControlViewModel))));
            #endregion

            #region //.. System Cell


            // Chuck Plnarity Sub Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ChuckPlanaritySubSettingView.ChuckPlanaritySubSettingView)), GetGuid(typeof(ChuckPlanaritySubSettingVM))));

            // MarkAlign Sub Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(MarkSettingSubView)), GetGuid(typeof(GP_MarkAlignSettingVM))));

            // Probing System Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ProbingSystemSettingView.ProbingSystemSettingView)), GetGuid(typeof(ProbingSystemSettingVM))));

            // FIle System
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(FileSystemView.FileSystemView)), GetGuid(typeof(FileSystemVM))));
            
            // Soaking System
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(SoakingSystemView.SoakingSystemView)), GetGuid(typeof(SoakingSystemViewModel.SoakingSystemVM))));
            #endregion

            #region Communication

            // Tester communication
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(TesterCommnuicationSettingView)), GetGuid(typeof(TesterCommunicationSettingViewModel.TesterCommunicationSettingViewModel))));
            // TCP/IP
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(TCPIPSettingView)), GetGuid(typeof(TCPIPSettingViewModel.TCPIPSettingViewModel))));
            // GPIB
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(GpibSettingView)), GetGuid(typeof(GpibSettingViewModel))));

            #endregion

            // TempDeviation
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(TempDeviationView)), GetGuid(typeof(TempDeviationViewModel))));

            // TempSet
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(TempDeviceSettingView)), GetGuid(typeof(TempDeviceSettingViewModel))));

            //Chiller
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(GPChillerMainView)), GetGuid(typeof(GPChillerMainViewModel))));

            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(GPChillerMainView_6X2)), GetGuid(typeof(GPChillerMainViewModel))));
            //Chiller Setup
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(GpChillerStageSetupView)), GetGuid(typeof(GPChillerMainViewModel))));
            // Chiller OP
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(UcHBChiller)), GetGuid(typeof(HBChillerBase))));

            // ResultMap - Converter Sub Setting
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ResultMapConverterView)), GetGuid(typeof(ResultMapConverterVM))));

            // E84ControlView 
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(E84ControlView)), GetGuid(typeof(E84ControlViewModel))));

            //EnvMonitoringView
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(EnvMonitoringView)), GetGuid(typeof(EnvMonitoringViewModel))));

            // VerifyParameterView
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(VerifyParameterView)), GetGuid(typeof(VerifyParameterViewModel))));

            // MultiManualContact
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(MultiManualContactView)), GetGuid(typeof(MultiManualContactViewModel))));

            // GPUtilityOption
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(GPUtilityOptionView)), GetGuid(typeof(GPUtilityOptionViewModel))));

            // TesterInterface
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(TesterInterfaceView)), GetGuid(typeof(TesterInterfaceVM))));

            // Retest
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(RetestSettingSubView.RetestSettingSubView)), GetGuid(typeof(LoaderRetestSettingViewModel))));

            // Manual Soaking
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(ManualSoakingView)), GetGuid(typeof(ManualSoakingViewModel))));

            // Uc Card Change Setting
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

            //Touch Sensor View
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(TouchSensorView)), GetGuid(typeof(TouchSensorViewModel))));
            // WaferAlign Sys
            _ConnectionInfos.Add(MakeViewConnectInfo(GetGuid(typeof(WaferAlignSysSubView)), GetGuid(typeof(WaferAlignSysSubViewModel))));

            return EventCodeEnum.NONE;
        }

        private ViewConnectInfo MakeViewConnectInfo(Guid guid)
        {
            throw new NotImplementedException();
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

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LoaderViewAndViewModelConnectParam] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
    }
}
