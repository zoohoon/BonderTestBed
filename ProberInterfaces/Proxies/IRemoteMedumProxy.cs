using System;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.Enum;
    using ProberInterfaces.State;
    using ProberInterfaces.PnpSetup;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;
    using SharpDXRender.RenderObjectPack;
    using ProberInterfaces.Temperature;
    using System.ServiceModel;
    using ProberInterfaces.Proxies;
    using ProberInterfaces.ViewModel;
    using ProberInterfaces.ControlClass.ViewModel.Wafer.Sequence;
    using ProberInterfaces.Loader.RemoteDataDescription;
    using ProberInterfaces.PMI;
    using ProberInterfaces.PolishWafer;
    using ProberInterfaces.ControlClass.ViewModel;
    using System.IO;
    using ProberInterfaces.ControlClass.ViewModel.PMI;
    using ProberInterfaces.BinData;
    using ProberInterfaces.Param;
    using ProberInterfaces.Utility;
    using ProberInterfaces.SequenceRunner;
    using LogModule.LoggerParam;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.WaferAlignEX;
    using LogModule;

    [ServiceKnownType(typeof(EnumArrowDirection))]
    [ServiceKnownType(typeof(DispFlipEnum))]
    [ServiceKnownType(typeof(PinSetupMode))]
    [ServiceKnownType(typeof(SelectionUIType))]
    [ServiceKnownType(typeof(HeightPointEnum))]
    [ServiceKnownType(typeof(PNPCommandButtonType))]
    public interface IRemoteMediumProxy : IFactoryModule, IProberProxy
    {
        bool IsOpened();
        CommunicationState GetCommunicationState();
        new void InitService();
        //bool IsAvailable();
        #region /.. WaferObject
        void SetWaferPhysicalInfo(IPhysicalInfo physinfo);

        byte[] GetWaferDevObjectbyFileToStream();
        //byte[] GetWaferObject();
        byte[] GetWaferDevObject();
        #endregion

        #region //..ViewModelManager
        Guid GetViewGuidFromViewModelGuid(Guid guid);
        Task<EventCodeEnum> PageSwitched(Guid viewGuid, object parameter = null);
        Task<EventCodeEnum> Cleanup(Guid viewGuid, object parameter = null);
        #endregion

        #region ..PMI

        byte[] GetPMIDevParam();

        #endregion

        #region ..ResultMap

        byte[] GetResultMapConvParam();
        EventCodeEnum SaveResultMapConvParam();

        string[] GetNamerAliaslist();
        void SetResultMapConvIParam(byte[] param);
        bool SetResultMapByFileName(byte[] device, string resultmapname);
        #endregion

        #region Probing

        EventCodeEnum ProbingModuleSaveDevParameter();
        byte[] GetProbingDevParam();
        void SetProbingDevParam(IParam param);

        byte[] GetBinDevParam();
        Task<MarkShiftValues> GetUserSystemMarkShiftValue();
        EventCodeEnum SetBinInfos(List<IBINInfo> binInfos);
        EventCodeEnum SaveBinDevParam();

        #endregion
        #region //..PNP
        PMITemplateMiniViewModel GetPMITemplateMiniViewModel();
        List<RenderContainer> GetRenderContainers();
        ObservableCollection<ObservableCollection<CategoryNameItems>> GetCategoryNameList(string modulename, string interfacename, Guid cuiguid, bool extrastep = false);

        PNPDataDescription GetPNPDataDescriptor();
        PnpUIData GetRemoteData();
        void PNPSetPackagableParams();
        List<byte[]>  PNPGetPackagableParams();
        
        IPnpSetup GetPnpSetup();

        void ButtonExecuteSync(object param, PNPCommandButtonType type);
        Task ButtonExecuteAsync(object param, PNPCommandButtonType type);
        PNPCommandButtonDescriptor GetPNPButtonDescriptor(PNPCommandButtonType param);

        EnumProberCam GetCamType();

        void SetSetupState(string header = null);

        void SetMiniViewTarget(object miniView);

        EnumMoudleSetupState GetSetupState(string header = null);
        Task<EventCodeEnum> StepPageSwitching(string moduleheader, object parameter);
        Task<EventCodeEnum> StepCleanup(string moduleheader, object parameter);

        bool StepIsParameterChanged(string moduleheader, bool issave);

        EventCodeEnum StepParamValidation(string moduleheader);
        Task SetCurrentStep(string moduleheader);
        void ApplyParams(List<byte[]> parameters);

        void SetDislayPortTargetRectInfo(double left, double top);
        //void SetPackagableParams();        
        #endregion

        #region //..Jog

        /// Light Jog
        void ChangeCamPosition(EnumProberCam cam);
        void UpdateCamera(EnumProberCam cam, string interfaceType);

        void SetLightValue(int intensity);
        void SetLightChannel(EnumLightType lightchnnel);
        int GetLightValue(EnumLightType lightchnnel);
        List<EnumLightType> GetLightTypes();
        bool CheckSWLimit(EnumProberCam cam);

        /// Motion Jog
        void StickIndexMove(JogParam parameter, bool setzoffsetenable);
        void StickStepMove(JogParam parameter);
        #endregion

        #region //..DisplayPort
        void StageMove();
     
        #endregion

        #region //..Soaking
        void SetSoakingParam(byte[] param);
        #endregion

        #region //..RetestViewModel

        Task RetestViewModel_PageSwitched();

        Task RetestViewModel_Cleanup();

        void RetestViewModel_SetRetestIParam(byte[] param);

        #endregion

        #region //..PolishWaferMakeSourceVM

        Task PolishWaferMakeSourceVM_PageSwitched();
        Task PolishWaferMakeSourceVM_Cleanup();
        Task PolishWaferMakeSourceVM_AddSourceCommand();
        Task PolishWaferMakeSourceVM_RemoveSourceCommand();
        void PolishWaferMakeSourceVM_AssignCommand();
        void PolishWaferMakeSourceVM_RemoveCommand();

        void UpdateCleaningParameters(string sourcename);

        //void PolishWaferMakeSourceVM_SelectedObjectCommand(object param);
        //void PolishWaferMakeSourceVM_SetPolishWaferIParam(byte[] param);
        //void PolishWaferMakeSourceVM_SetSelectedObjectCommand(byte[] info);

        //void TestSampleMethod();

        #endregion
            
        #region //..PolishWaferRecipeSettingVM

        Task PolishWaferRecipeSettingVM_PageSwitched();
        Task PolishWaferRecipeSettingVM_Cleanup();

        Task PolishWaferRecipeSettingVM_IntervalAddCommand();
        //Task PolishWaferRecipeSettingVM_CleaningDeleteCommand(PolishWaferIndexModel param);
        void PolishWaferRecipeSettingVM_CleaningDelete(PolishWaferIndexModel param);
        void PolishWaferRecipeSettingVM_SetPolishWaferIParam(byte[] param);
        Task PolishWaferRecipeSettingVM_SetSelectedInfos(SelectionUIType selectiontype, byte[] cleaningparam, byte[] pwinfo, byte[] intervalparam, int intervalindex, int cleaningindex);

        //Task PolishWaferRecipeSettingVM_CleaningAddCommand(object param);
        void PolishWaferRecipeSettingVM_CleaningAdd(int index);

        //Task PolishWaferRecipeSettingVM_IntervalDeleteCommand(object param);
        void PolishWaferRecipeSettingVM_IntervalDelete(int param);

        #endregion
        #region //..PolishWafer
        void SetPolishWaferParam(byte[] param);

        Task ManualPolishWaferFocusingCommand(byte[] param);
        Task<EventCodeEnum> DoManualPolishWaferCleaningCommand(byte[] param);
        #endregion

        #region //..InspectionVM
        Task<InspcetionDataDescription> GetInspectionInfo();
        Task Inspection_SetFromCommand();
        Task<EventCodeEnum> Inspection_CheckPMShiftLimit(double checkvalue);
        Task Inspection_SaveCommand(InspcetionDataDescription info);
        Task Inspection_ApplyCommand();
        Task Inspection_SystemApplyCommand();
        Task Inspection_ClearCommand();
        Task Inspection_SystemClearCommand();
        Task Inspection_PrevDutCommand();
        Task Inspection_NextDutCommand();
        Task Inspection_PadPrevCommand();
        Task Inspection_PadNextCommand();
        //Task Inspection_ManualSetIndexCommand();
        Task Inspection_WaferAlignCommand();
        Task Inspection_PinAlignCommand();
        Task Inspection_SavePads();

        Task Inspection_SaveTempOffset(ObservableDictionary<double, CatCoordinates> table);

        void Inspection_ChangeXManualCommand();
        void Inspection_ChangeYManualCommand();

        void Inspection_ChangeXManualIndex(long index);
        void Inspection_ChangeYManualIndex(long index);

        Task InspectionVM_PageSwitched();
        Task InspectionVM_Cleanup();
        CatCoordinates GetSetTemperaturePMShifhtValue();

        Dictionary<double, CatCoordinates> GetTemperaturePMShifhtTable();
        #endregion
        EventCodeEnum UpdateSysparam();
        #region // ..PMI Viewer VM

        Task PMIViewer_PageSwitched();
        int PMIViewer_GetTotalImageCount();

        void PMIViewer_UpdateFilterDatas(DateTime Startdate, DateTime Enddate, PadStatusResultEnum Status);

        void PMIViewer_LoadImage();

        PMIImageInformationPack PMIViewer_GetImageFileData(int index);

        ObservableCollection<PMIWaferInfo> PMIViewer_GetWaferlist();

        void PMIViewer_WaferListClear();
        void PMIViewer_ChangedWaferListItem(PMIWaferInfo pmiwaferinfo);

        #endregion

        #region Sequence Maker
        Task SequenceMakerVM_PageSwitched();
        Task SequenceMakerVM_Cleanup();
        Task SequenceMakerVM_SetMXYIndex(Point mxyindex);
        Task<EventCodeEnum> SequenceMakerVM_GetUnderDutDices(MachineIndex mxy);
        Task SequenceMakerVM_MoveToPrevSeqCommand();
        Task SequenceMakerVM_MoveToNextSeqCommand();
        Task SequenceMakerVM_InsertSeqCommand();
        Task SequenceMakerVM_DeleteSeqCommand();
        void SequenceMakerVM_ChangeAutoAddSeqEnable(bool flag);
        Task SequenceMakerVM_SeqNumberSeletedCommand(object param);
        Task SequenceMakerVM_AutoMakeSeqCommand();
        Task SequenceMakerVM_DeleteAllSeqCommand();
        Task SequenceMakerVM_MapMoveCommand(object param);
        SequenceMakerDataDescription GetSequenceMakerInfo();
        List<DeviceObject> GetUnderDutDIEs();
        #endregion

        #region //..Manual Contact
        ManaulContactDataDescription GetManualContactInfo();
        bool GetManaulContactMovingStage();

        SoakingRecipeDataDescription GetSoakingRecipeInfo();
        Task SetMCM_XYInex(Point index);
        void MCMIncreaseX();
        void MCMIncreaseY();
        void MCMDecreaseX();
        void MCMDecreaseY();
        Task ManualContact_FirstContactSetCommand();
        Task ManualContact_AllContactSetCommand();
        Task ManualContact_ResetContactStartPositionCommand();
        Task ManualContact_OverDriveTBClickCommand();
        void MCMChangeOverDrive(string overdrive);
        Task ManualContact_ChangeZUpStateCommand();
        Task ManualContact_MoveToWannaZIntervalPlusCommand();
        Task ManualContact_WantToMoveZIntervalTBClickCommand();
        Task ManualContact_MoveToWannaZIntervalMinusCommand();
        Task ManualContact_SetOverDriveCommand();
        Task ManualContactVM_PageSwitched();
        Task ManualContactVM_Cleanup();
        void MCMSetIndex(EnumMovingDirection dirx, EnumMovingDirection diry);

        Task ManualContact_CPC_Z1_ClickCommand();
        void MCMChangeCPC_Z1(string z1);
        Task ManualContact_CPC_Z2_ClickCommand();
        void MCMChangeCPC_Z2(string z2);
        void GetOverDriveFromProbingModule();
        //void MCMSetAlawaysMoveToTeachDie(bool flag);
        #endregion

        #region //.. Soaking Recipe

        void SoakingRecipe_DropDownClosedCommand();

        #endregion

        #region //..Wafer Map Maker VM
        void WaferMapMaker_UpdateWaferSize(EnumWaferSize waferSize);
        void WaferMapMaker_UpdateWaferSizeOffset(double WaferSizeOffset);
        void WaferMapMakerVM_WaferSubstrateType(WaferSubstrateTypeEnum wafersubstratetype);
        void WaferMapMaker_UpdateDieSizeX(double diesizex);
        void WaferMapMaker_UpdateDieSizeY(double diesizey);
        void WaferMapMaker_UpdateThickness(double thickness);
        void WaferMapMake_UpdateEdgeMargin(double margin);
        void WaferMapMaker_NotchAngle(double notchangle);
        void WaferMapMaker_NotchType(WaferNotchTypeEnum notchtype);

        void WaferMapMaker_NotchAngleOffset(double notchangleoffset);
        Task WaferMapMaker_Cleanup();

        Task WaferMapMaker_ApplyCreateWaferMapCommand();
        Task WaferMapMaker_MoveToWaferThicknessCommand();
        Task WaferMapMaker_AdjustWaferHeightCommand();
        Task<EventCodeEnum> WaferMapMaker_CmdImportWaferData(Stream fileStream);
        void WaferMapMaker_ImportFilePath(string filePath);
        void WaferMapMakerVM_SetHighStandardParam(HeightPointEnum heightpoint);
        HeightPointEnum WaferMapMakerVM_GetHeightProfiling();
        #endregion

        #region //..Dut Editor VM

        byte[] DutEditor_GetDutlist();
        DutEditorDataDescription GetDutEditorInfo();

        void DutEditor_ImportFilePath(string filePath);
        Task<EventCodeEnum> DutEditor_CmdImportCardDataCommand(Stream fileStream);
        Task DutEditor_InitializePalletCommand();
        Task DutEditor_DutAddCommand();
        Task DutEditor_DutDeleteCommand();
        void DutEditor_ZoomInCommand();
        void DutEditor_ZoomOutCommand();
        Task DutEditor_DutEditerMoveCommand(EnumArrowDirection param);
        Task DutEditor_DutAddMouseDownCommand();

        Task DutEditor_PageSwitched();
        Task DutEditor_Cleanup();

        void DutEditor_ChangedAddCheckBoxIsChecked(bool? param);
        void DutEditor_ChangedSelectedCoordM(MachineIndex param);

        void DutEditor_ChangedFirstDutM(MachineIndex param);

        #endregion

        #region //.. Device Change (FileManager)

        Task GetParamFromDevice(DeviceInfo device);
        DeviceChangeDataDescription GetDeviceChangeInfo();
        void DeviceChange_RefreshDevListCommand();
        void DeviceChange_ClearSearchDataCommand();
        void DeviceChange_SearchTBClickCommand();

        void DeviceChange_PageSwitchingCommand();

        Task DeviceChange_GetDevList(bool isPageSwiching = false);
        Task DeviceChange_ChangeDeviceCommand();
        Task DeviceChangeWithName_ChangeDeviceCommand(string deviceName);
        Task DeviceChange_CreateNewDeviceCommand();
        Task DeviceChange_SaveAsDeviceCommand();
        Task DeviceChange_DeleteDeviceCommand();

        //byte[] DeviceChange_CreateDeviceFolder(string devpath);
        //byte[] DeviceChange_SaveAsDeviceFolder(string devpath);
        #endregion

        #region ChuckPlanarity

        Task ChuckPlanarity_ChuckMoveCommand(EnumChuckPosition param);
        Task ChuckPlanarity_MeasureOnePositionCommand();
        Task ChuckPlanarity_SetAdjustPlanartyFunc();
        Task ChuckPlanarity_MeasureAllPositionCommand();
        //void ChuckPlanarity_ChangeSpecHeightValue(double value);

        void ChuckPlanarity_ChangeMarginValue(double value);

        void ChuckPlanarity_FocusingRangeValue(double value);
        ChuckPlanarityDataDescription GetChuckPlanarityInfo();

        Task ChuckPlanarity_PageSwitched();

        Task ChuckPlanarity_Cleanup();

        Task ChuckMoveCommand(ChuckPos param);

        #endregion

        #region // Temp. Module
        ITempController GetTempModule();
        #endregion

        #region GPCC_Observation
        Task GPCC_Observation_CardSettingCommand();
        Task GPCC_Observation_PogoSettingCommand();
        Task GPCC_Observation_PodSettingCommand();
        void GPCC_Observation_PatternWidthPlusCommand();
        void GPCC_Observation_PatternWidthMinusCommand();
        void GPCC_Observation_PatternHeightPlusCommand();
        void GPCC_Observation_PatternHeightMinusCommand();
        Task GPCC_Observation_WaferCamExtendCommand();
        Task GPCC_Observation_WaferCamFoldCommand();
        Task GPCC_Observation_MoveToCenterCommand();
        Task GPCC_Observation_ReadyToGetCardCommand();
        Task GPCC_Observation_DropZCommand();
        Task GPCC_Observation_RaiseZCommand();

        Task GPCC_Observation_ManualZMoveCommand(EnumProberCam camType,double value);
        void GPCC_Observation_IncreaseLightIntensityCommand();
        void GPCC_Observation_DecreaseLightIntensityCommand();
        Task GPCC_Observation_RegisterPatternCommand();
        Task GPCC_Observation_PogoAlignPointCommand(EnumPogoAlignPoint point);
        Task GPCC_Observation_RegisterPosCommand();
        GPCardChangeVMData GPCC_Observation_GetGPCCDataCommand();
        Task GPCC_Observation_MoveToMarkCommand();
        Task GPCC_Observation_SetSelectedMarkPosCommand(int selectedMarkposIdx);
        Task GPCC_Observation_SetSelectLightTypeCommand(EnumLightType selectlight);
        Task GPCC_Observation_SetLightValueCommand(ushort lightvalue);
        Task GPCC_Observation_SetZTickValueCommand(int ztickvalue);
        Task GPCC_Observation_SetZDistanceValueCommand(double zdistancevalue);
        Task GPCC_Observation_SetLightTickValueCommand(int lighttickvalue);
        Task GPCC_Observation_SetMFModelLightsCommand();
        Task GPCC_Observation_SetMFChildLightsCommand();

        Task GPCC_Observation_PageSwitchCommand();
        Task GPCC_Observation_CleanUpCommand();
        Task GPCC_Observation_FocusingCommand();
        Task GPCC_Observation_AlignCommand();


        #endregion

        #region GPCC_OP
        Task GPCC_OP_SetContactOffsetZValueCommand(double offsetz);
        Task GPCC_OP_SetContactOffsetXValueCommand(double offsetx);
        Task GPCC_OP_SetContactOffsetYValueCommand(double offsety);
        void GPCC_OP_CardContactSettingZCommand();
        Task GPCC_OP_SetUndockContactOffsetZValueCommand(double offsetz);
        Task GPCC_OP_SetFocusRangeValueCommand(double rangevalue);
        void GPCC_OP_CardUndockContactSettingZCommand();
        Task GPCC_OP_CardFocusRangeSettingZCommand(double rangevalue);
        Task GPCC_OP_SetAlignRetryCountCommand(int retrycount);

        Task GPCC_OP_SetDistanceOffsetCommand(double distanceOffset);

        Task GPCC_OP_SetCardTopFromChuckPlaneSettingCommand(double distance);

        Task GPCC_OP_SetCardCenterOffsetX1Command(double value);
        Task GPCC_OP_SetCardCenterOffsetX2Command(double value);
        Task GPCC_OP_SetCardCenterOffsetY1Command(double value);
        Task GPCC_OP_SetCardCenterOffsetY2Command(double value);

        Task GPCC_OP_SetCardPodCenterXCommand(double value);
        Task GPCC_OP_SetCardPodCenterYCommand(double value);

        Task GPCC_OP_SetCardLoadZLimitCommand(double value);
        Task GPCC_OP_SetCardLoadZIntervalCommand(double value);
        Task GPCC_OP_SetCardUnloadZOffsetCommand(double value);

        Task GPCC_OP_ZifToggleCommand();
        Task GPCC_OP_SmallRaiseChuckCommand();
        Task GPCC_OP_SmallDropChuckCommand();
        void GPCC_OP_MoveToZClearedCommand();
        Task GPCC_OP_MoveToLoaderCommand();

        Task GPCC_OP_MoveToCenterCommand();
        Task GPCC_OP_MoveToBackCommand();
        Task GPCC_OP_MoveToFrontCommand();
        Task GPCC_OP_RaisePodAfterMoveCardAlignPosCommand();
        Task GPCC_OP_RaisePodCommand();
        Task GPCC_OP_DropPodCommand();
        Task<EventCodeEnum> GPCC_OP_ForcedDropPodCommand();
        Task GPCC_OP_TopPlateSolLockCommand();
        Task GPCC_OP_TopPlateSolUnLockCommand();
        Task GPCC_OP_PCardPodVacuumOffCommand();
        Task GPCC_OP_PCardPodVacuumOnCommand();
        Task GPCC_OP_UpPlateTesterCOfftactVacuumOffCommand();
        Task GPCC_OP_UpPlateTesterContactVacuumOnCommand();
        Task GPCC_OP_UpPlateTesterPurgeAirOffCommand();
        Task GPCC_OP_UpPlateTesterPurgeAirOnCommand();
        Task GPCC_OP_PogoVacuumOffCommand();
        Task GPCC_OP_PogoVacuumOnCommand();
        Task GPCC_OP_DockCardCommand();
        Task GPCC_OP_UnDockCardCommand();
        Task GPCC_OP_CardAlignCommand();
        Task GPCC_OP_PageSwitchCommand();
        Task GPCC_OP_CleanUpCommand();

        void GPCC_OP_LoaderDoorOpenCommand();

        void GPCC_OP_LoaderDoorCloseCommand();
        bool GPCC_OP_IsLoaderDoorCloseCommand(bool writelog = true);
        bool GPCC_OP_IsLoaderDoorOpenCommand(bool writelog = true);
        void GPCC_OP_CardDoorOpenCommand();

        void GPCC_OP_CardDoorCloseCommand();
        bool GPCC_OP_IsLCardExistCommand();

        Task<CardChangeSysparamData> GPCC_OP_GetGPCardChangeSysParamData();
        Task<CardChangeDevparamData> GPCC_OP_GetGPCardChangeDevParamData();
        CardChangeVacuumAndIOStatus GPCC_OP_GetCCVacuumStatus();

        
        string GPCC_OP_GetPosition();

        ObservableCollection<ISequenceBehaviorGroupItem> GPCC_OP_GetDockSequence();
        ObservableCollection<ISequenceBehaviorGroupItem> GPCC_OP_GetUnDockSequence();

        int GPCC_OP_GetCurBehaviorIdx();

        void GPCC_Docking_PauseState();
        void GPCC_Docking_StepUpState();
        void GPCC_Docking_ContinueState();
        void GPCC_Docking_AbortState();

        void GPCC_UnDocking_PauseState();
        void GPCC_UnDocking_StepUpState();
        void GPCC_UnDocking_ContinueState();
        void GPCC_UnDocking_AbortState();

        Task GPCC_SetWaitForCardPermitEnableCommand(bool value);
        bool IsCheckCardPodState();
        #endregion

        #region //..TemplateMaanger

        EventCodeEnum CheckTemplate(ITemplate module, bool applyload = true, int index = -1);

        #endregion

        #region LogTransfer
        Task<List<List<string>>> LogTransfer_UpdateLogFile();
        Task<byte[]> LogTransfer_OpenLogFile(string selectedFilePath);
        ObservableDictionary<string, string> GetLogPathInfos();
        EventCodeEnum SetLogPathInfos(ObservableDictionary<string, string> infos);
        //ObservableDictionary<string, string> GetImageLogPathInfos();
        //ImageLoggerParam GetImageLoggerParam();
        void SetmageLoggerParam(ImageLoggerParam imageLoggerParam);

        ImageDataSet GetImageDataSet(EnumProberModule moduletype, string moduleStartTime, string hashcode);

        #endregion
        #region LoadLUT
        void LightAdmin_LoadLUT();
        #endregion

        #region TCPIP
        EventCodeEnum ReInitializeAndConnect();
        EventCodeEnum CheckAndConnect();

        EventCodeEnum FoupAllocated(FoupAllocatedInfo allocatedInfo);
        #endregion

        void SetOperatorName(string name);
    }
}
