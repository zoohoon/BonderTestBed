using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.Enum;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.State;
    using ProberInterfaces.ViewModel;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.Windows;
    using SharpDXRender.RenderObjectPack;
    using SharpDXRender;
    using ProberInterfaces.PMI;
    using System.IO;
    using ProberInterfaces.Temperature;
    using ProberInterfaces.ControlClass.ViewModel.Wafer.Sequence;
    using ProberInterfaces.ControlClass.ViewModel;
    using ProberInterfaces.Loader.RemoteDataDescription;
    using ProberInterfaces.PolishWafer;
    using System.Threading;
    using ProberInterfaces.ControlClass.ViewModel.PMI;
    //using ProberInterfaces.SequenceRunner;
    using ProberInterfaces.Param;
    using ProberInterfaces.Utility;
    using LogModule.LoggerParam;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.WaferAlignEX;
    using System.Windows.Media;
    using LogModule;

    [ServiceKnownType(typeof(WindowsPoint))]
    [ServiceKnownType(typeof(RenderEllipse))]
    [ServiceKnownType(typeof(RenderGeometry))]
    [ServiceKnownType(typeof(RenderLine))]
    [ServiceKnownType(typeof(RenderRectangle))]
    [ServiceKnownType(typeof(RenderText))]
    [ServiceKnownType(typeof(EventCodeEnum))]
    [ServiceKnownType(typeof(EnumJogDirection))]
    [ServiceKnownType(typeof(ProberInterfaces.PMI.JOG_DIRECTION))]
    [ServiceKnownType(typeof(EnumProberCam))]
    [ServiceKnownType(typeof(SETUP_DIRECTION))]
    [ServiceKnownType(typeof(MARK_SIZE))]
    [ServiceKnownType(typeof(SELECTION_MODE))]
    [ServiceKnownType(typeof(EnumWaferSize))]
    [ServiceKnownType(typeof(EnumChuckPosition))]
    [ServiceKnownType(typeof(EnumArrowDirection))]
    [ServiceKnownType(typeof(PinSetupMode))]
    [ServiceKnownType(typeof(PinLowAlignPatternOrderEnum))]
    [ServiceKnownType(typeof(DeviceObject))]
    [ServiceKnownType(typeof(MapHorDirectionEnum))]
    [ServiceKnownType(typeof(MapVertDirectionEnum))]
    [ServiceKnownType(typeof(PMI_SETUP_MODE))]
    [ServiceKnownType(typeof(PAD_SHAPE))]
    [ServiceKnownType(typeof(LOGGING_MODE))]
    [ServiceKnownType(typeof(Visibility))]
    [ServiceKnownType(typeof(SideViewMode))]
    [ServiceKnownType(typeof(VerticalAlignment))]
    [ServiceKnownType(typeof(HorizontalAlignment))]
    [ServiceKnownType(typeof(MapViewMode))]
    [ServiceKnownType(typeof(PadStatusResultEnum))]
    [ServiceKnownType(typeof(HeightPointEnum))]
    [ServiceKnownType(typeof(PNPCommandButtonType))]
    [ServiceKnownType(typeof(JogMode))]
    public interface ILoaderRemoteMediatorCallback
    {
        [OperationContract(IsOneWay = true)]
        void SetWaferDevice(byte[] device);
        [OperationContract(IsOneWay = true)]
        Task RequestGetWaferObject();

        //[OperationContract(IsOneWay = true)]
        //void UpdateDockChangeStateObject(byte[] seq);
        //[OperationContract(IsOneWay = true)]
        //void UpdateUnDockChangeStateObject(byte[] seq);
        [OperationContract(IsOneWay = true)]
        void CallBack_Update_Error_MSG(string err_msg);

        [OperationContract(IsOneWay = true)]
        void SetDieType(long xindex, long yindex, DieTypeEnum dietype);

        [OperationContract(IsOneWay = true)]
        void UpdateDutPadInfos(byte[] dutpadinfos);

        [OperationContract(IsOneWay = true)]
        void StageDataLoaded();

        [OperationContract(IsOneWay = true)]
        void WaferIndexUpdated(long xindex, long yindex);

        [OperationContract(IsOneWay = true)]
        void UploadFile(byte[] bytestream, string filepath);

        [OperationContract(IsOneWay = true)]
        void UpdateStageMove(StageMoveInfo info);

        #region //..PNP

        [OperationContract(IsOneWay = true)]
        void UpdatgePMITemplateMiniViewModel(PMITemplateMiniViewModel vm);

        [OperationContract(IsOneWay = true)]
        void SetupStateUpdated(EnumMoudleSetupState state, bool isparent);
        [OperationContract(IsOneWay = true)]
        void SetupStateHeaderUpdated(EnumMoudleSetupState state, bool isparent,string header, string recoveryHeader);

        [OperationContract(IsOneWay = true)]
        void SetupRecoveryStateHeaderUpdated(EnumMoudleSetupState state, bool isparent, string header, string recoveryHeader);
        [OperationContract(IsOneWay = true)]
        void StepLabelUpdated(string label);
        [OperationContract(IsOneWay = true)]
        void StepSecondLabelUpdated(string label);

        [OperationContract(IsOneWay = true)]
        void StepLabelActiveUpdated(bool active);
        [OperationContract(IsOneWay = true)]
        void StepSecondLabelActiveUpdated(bool active);
        [OperationContract(IsOneWay = true)]
        void DisplayPortUserControlUpdated(UserControlFucEnum dUserControl);
        [OperationContract(IsOneWay = true)]
        void DisplayPortRetangleUpdated(double width, double height);

        [OperationContract(IsOneWay = true)]
        void ImageBufferUpdated(ImageBuffer imgBuffer);


        [OperationContract(IsOneWay = true)]
        void PNPMainViewImageSourceUpdated(byte[] imagesource);
        [OperationContract(IsOneWay = true)]
        void PNPButtonsUpdated(PNPDataDescription pNPDataDescriptor);
        [OperationContract(IsOneWay = true)]
        void PNPLightJogUpdated(EnumLightType lighttype, int intensity);
        [OperationContract(IsOneWay = true)]
        void JogModeUpdated(JogMode mode);
        [OperationContract(IsOneWay = true)]
        void WaferObjectInfoNonSerializeUpdated(WaferObjectInfoNonSerialized waferobjinfo);

        //[OperationContract]
        //void DisplayPortRetangleUpdated(double width, double height);
        #endregion

        #region //..NC
        [OperationContract(IsOneWay = true)]
        void SetNCObjectUpdated(byte[] ncobject);
        [OperationContract(IsOneWay = true)]
        void NCSheetVMDefsUpdated(byte[] ncsheetvmedfs);
        [OperationContract(IsOneWay = true)]
        void NCSequencesInfoUpdated(byte[] ncsequencesinfo);

        #endregion

        #region //..PMI
        [OperationContract(IsOneWay = true)]
        void PMIInfoUpdated(byte[] pmiinfo);

        [OperationContract]
        void UpdatgeNormalPMIMapTemplateInfo(byte[] infos);

        [OperationContract]
        void ChangedIsMapViewShowPMITable(bool flag);
        [OperationContract]
        void ChangedIsMapViewShowPMIEnable(bool flag);

        [OperationContract]
        void ChangedIsMapViewControlMode(MapViewMode mode);

        #endregion

        #region //..Probing Sequence
        [OperationContract(IsOneWay = true)]
        void SequenceMakerVM_SetMXYIndex(Point mxyindex);

        [OperationContract(IsOneWay = true)]
        Task SequenceMakerVM_UpdateDeviceObject(List<ExistSeqs> list);

        #endregion

        #region //..Manual Contact
        [OperationContract(IsOneWay = true)]
        void SetProbingDevices(int index, ObservableCollection<IDeviceObject> devs);

        [OperationContract(IsOneWay = true)]
        void ClearUnderDutDevs(int index);


        [OperationContract(IsOneWay = true)]
        void SetContactPosInfo(Point mindex, bool iszupstate);

        [OperationContract(IsOneWay = true)]
        //void UpdateShowingDevicelist(ObservableCollection<DeviceInfo> showingdeiveinfos);
        //void UpdateShowingDevicelist(byte[] showingdeiveinfos);
        Task UpdateShowingDevicelist(List<DeviceInfo> showingdeiveinfos);


        [OperationContract(IsOneWay = true)]
        Task UpdateShowingDevice(DeviceInfo showingdevice, byte[] wafer, byte[] dies, SubstrateInfoNonSerialized subNonSerialized, byte[] probecard);

        [OperationContract(IsOneWay = true)]
        void SetMachinePosition(MachinePosition machinepos);
        #endregion

        [OperationContract]
        bool IsServiceAvailable();

        //#region //..Soaking
        //[OperationContract(IsOneWay = true)]
        //void UpdateSoakingInfo(SoakingInfo soakinfo);
        //#endregion

        [OperationContract(IsOneWay = true)]
        void ChangedMapDirection(MapHorDirectionEnum MapDirX, MapVertDirectionEnum MaxDirY);

        [OperationContract(IsOneWay = true)]
        void ChangedWaferObjectDutCenter(double CenterX, double CenterY);

        [OperationContract(IsOneWay = true)]
        void ChangedProbeCardObjectDutCenter(double CenterX, double CenterY);

        #region //..Lot Screen
        [OperationContract(IsOneWay = true)]
        void MapScreenToLotScreen(int index);
        [OperationContract(IsOneWay = true)]
        void VisionScreenToLotScreen(int index);
        #endregion

        #region //.. Stage Info
        void SetStageMode(int index, GPCellModeEnum cellmode);

        #endregion

        [OperationContract(IsOneWay = true)]
        void StageJobFinished();
        [OperationContract]
        void UpdateStopOptionToStage(int index);

        [OperationContract]
        byte[] GetProberCardListFromCCSysparam();
        [OperationContract(IsOneWay = true)]
        void UpdateDeviceChangeInfo(ChuckPlanarityDataDescription info);
    }
    [ServiceKnownType(typeof(WindowsPoint))]
    [ServiceKnownType(typeof(RenderEllipse))]
    [ServiceKnownType(typeof(RenderGeometry))]
    [ServiceKnownType(typeof(RenderLine))]
    [ServiceKnownType(typeof(RenderRectangle))]
    [ServiceKnownType(typeof(RenderText))]
    [ServiceKnownType(typeof(LoggerParameter))]
    [ServiceKnownType(typeof(EventCodeEnum))]
    [ServiceKnownType(typeof(EnumJogDirection))]
    [ServiceKnownType(typeof(ProberInterfaces.PMI.JOG_DIRECTION))]
    [ServiceKnownType(typeof(EnumProberCam))]
    [ServiceKnownType(typeof(SETUP_DIRECTION))]
    [ServiceKnownType(typeof(MARK_SIZE))]
    [ServiceKnownType(typeof(SELECTION_MODE))]
    [ServiceKnownType(typeof(EnumWaferSize))]
    [ServiceKnownType(typeof(EnumChuckPosition))]
    [ServiceKnownType(typeof(EnumArrowDirection))]
    [ServiceKnownType(typeof(PinSetupMode))]
    [ServiceKnownType(typeof(PinLowAlignPatternOrderEnum))]
    [ServiceKnownType(typeof(LOGGING_MODE))]
    [ServiceKnownType(typeof(Visibility))]
    [ServiceKnownType(typeof(SideViewMode))]
    [ServiceKnownType(typeof(VerticalAlignment))]
    [ServiceKnownType(typeof(HorizontalAlignment))]
    [ServiceKnownType(typeof(PadStatusResultEnum))]
    [ServiceKnownType(typeof(HeightPointEnum))]
    [ServiceKnownType(typeof(PNPCommandButtonType))]
    [ServiceKnownType(typeof(JogMode))]
    [ServiceContract(CallbackContract = typeof(ILoaderRemoteMediatorCallback))]
    public interface ILoaderRemoteMediator : IFactoryModule, IModule
    {
        ILoaderRemoteMediatorCallback GetServiceCallBack();
        void DeInitService();

        [OperationContract]
        bool IsServiceAvailable();
        [OperationContract]
        void InitService();
        [OperationContract]
        bool IsAvailable();


        #region PMI
        [OperationContract]
        byte[] GetPMIDevParam();

        #endregion

        #region ResultMap

        [OperationContract]
        byte[] GetResultMapConvParam();

        [OperationContract]
        EventCodeEnum SaveResultMapConvParam();

        [OperationContract]
        void SetResultMapConvIParam(byte[] param);
        [OperationContract]
        bool SetResultMapByFileName(byte[] device, string resultmapname);
        [OperationContract]
        string[] GetNamerAliaslist();

        #endregion

        #region Probing

        [OperationContract]
        byte[] GetBinDevParam();
        [OperationContract]
        EventCodeEnum SetBinInfos(byte[] binInfos);
        [OperationContract]
        EventCodeEnum SaveBinDevParam();

        [OperationContract]
        byte[] GetProbingDevParam();

        [OperationContract]
        void SetProbingDevParam(byte[] param);

        [OperationContract]
        EventCodeEnum ProbingModuleSaveDevParameter();
        [OperationContract]
        CatCoordinates GetSetTemperaturePMShifhtValue();

        [OperationContract]
        Dictionary<double, CatCoordinates> GetTemperaturePMShifhtTable();
        [OperationContract]
        Task<MarkShiftValues> GetUserSystemMarkShiftValue();

        [OperationContract]
        EventCodeEnum UpdateSysparam();
        #endregion
        #region //..(WaferObject)
        [OperationContract]
        //void SetWaferPhysicalInfo(IPhysicalInfo physinfo);
        void SetWaferPhysicalInfo(byte[] physinfo);


        [OperationContract]
        byte[] GetWaferDevObjectbyFileToStream();

        [OperationContract]
        byte[] GetWaferDevObject();
        #endregion

        #region //..ViewModelManagerModule
        [OperationContract]
        Guid GetViewGuidFromViewModelGuid(Guid guid);
        [OperationContract(AsyncPattern = true)]
        Task<EventCodeEnum> PageSwitched(Guid viewGuid, object parameter = null);
        [OperationContract(AsyncPattern = true)]
        Task<EventCodeEnum> CleanUp(Guid viewGuid, object parameter = null);
        #endregion

        #region //..PNP

        [OperationContract]
        ObservableCollection<ObservableCollection<CategoryNameItems>> GetCategoryNameList(string modulename, string interfacename, Guid cuiguid, bool extrastep = false);

        [OperationContract]
        EventCodeEnum GetCuiBtnParam(object module, Guid cuiguid, out Guid viewguid, out List<Guid> stepguids, bool extrastep = false);

        [OperationContract(AsyncPattern = true)]
        Task<bool> CheckWaferAlignPossibleSetup();

        [OperationContract]
        EnumProberCam GetCamType();
        [OperationContract]
        EnumMoudleSetupState GetSetupState(string header = null);
        [OperationContract]
        List<RenderContainer> GetRenderContainers();
        [OperationContract]
        void SetSetupState(string moduleheader);

        [OperationContract]
        void SetMiniViewTarget(object miniView);

        [OperationContract(AsyncPattern = true)]
        Task<EventCodeEnum> StepPageSwitching(string moduleheader, object parameter);

        [OperationContract(AsyncPattern = true)]
        Task<EventCodeEnum> StepCleanup(string moduleheader, object parameter);
        [OperationContract]
        bool StepIsParameterChanged(string moduleheader, bool issave);
        [OperationContract]
        EventCodeEnum StepParamValidation(string moduleheader);
        [OperationContract]
        Task SetCurrentStep(string moduleheader);
        [OperationContract]
        void ApplyParams(List<byte[]> parameters);
        [OperationContract]
        void CloseAdvanceSetupView();

        [OperationContract(IsOneWay = true)]
        void SetDislayPortTargetRectInfo(double left, double top);
        //[OperationContract]
        //void SetPackagableParams();
        #region //..Button
        [OperationContract]
        PNPDataDescription GetPNPDataDescriptor();
        [OperationContract]
        PnpUIData GetRemoteData();
        [OperationContract]
        void PNPSetPackagableParams();
        [OperationContract]
        List<byte[]> PNPGetPackagableParams();
        [OperationContract]
        byte[] GetPMITemplateMiniViewModel();
        [OperationContract]
        IPnpSetup GetPnpSetup();
        [OperationContract(IsOneWay = true)]
        void PNPButtonExecuteSync(object param, PNPCommandButtonType type);
        [OperationContract]
        Task PNPButtonExecuteAsync(object param, PNPCommandButtonType type);
        [OperationContract]
        PNPCommandButtonDescriptor GetPNPCommandButtonDescriptorCurStep(PNPCommandButtonType type);
        #endregion

        #region //..Jog
        /// Light Jog
        [OperationContract]
        void ChangeCamPosition(EnumProberCam cam);
        [OperationContract]
        void UpdateCamera(EnumProberCam cam, string interfaceType);
        [OperationContract]
        void SetLightValue(int intensity);
        [OperationContract]
        void SetLightChannel(EnumLightType lightchnnel);
        [OperationContract]
        List<EnumLightType> GetLightTypes();
        [OperationContract]
        int GetLightValue(EnumLightType lightchannel);

        /// Motion Jog
        [OperationContract]
        void StickIndexMove(JogParam parameter, bool setzoffsetenable);
        [OperationContract]
        void StickStepMove(JogParam parameter);

        #endregion

        #region //..Soaking
        [OperationContract]
        void SetSoakingParam(byte[] param);

        #endregion

        #region //..PolishWaferMakeSourceVM

        [OperationContract]
        Task RetestViewModel_PageSwitched();
        [OperationContract]
        Task RetestViewModel_Cleanup();
        [OperationContract]
        void RetestViewModel_SetRetestIParam(byte[] param);

        #endregion

        #region //..PolishWaferMakeSourceVM
        [OperationContract]
        Task PolishWaferMakeSourceVM_PageSwitched();
        [OperationContract]
        Task PolishWaferMakeSourceVM_Cleanup();
        [OperationContract]
        Task PolishWaferMakeSourceVM_AddSourceCommandExcute();
        [OperationContract]
        Task PolishWaferMakeSourceVM_RemoveSourceCommandExcute();
        [OperationContract]
        Task PolishWaferMakeSourceVM_AssignCommandExcute();
        [OperationContract]
        Task PolishWaferMakeSourceVM_RemoveCommandExcute();

        [OperationContract]
        void PolishWaferMakeSourceVM_UpdateCleaningParameters(string sourcename);

        //[OperationContract]
        //void PolishWaferMakeSourceVM_SelectedObjectCommandExcute(object param);

        //[OperationContract]
        //void PolishWaferMakeSourceVM_SetPolishWaferIParam(byte[] param);

        //[OperationContract]
        //void PolishWaferMakeSourceVM_SetSelectedObjectCommand(byte[] info);

        //[OperationContractAttribute(AsyncPattern = true)]
        //IAsyncResult BeginSampleMethod(string msg, AsyncCallback callback, object asyncState);

        ////Note: There is no OperationContractAttribute for the end method.
        //string EndSampleMethod(IAsyncResult result);

        #endregion

        #region //..PolishWaferRecipeSetting

        [OperationContract]
        Task PolishWaferRecipeSettingVM_PageSwitched();

        [OperationContract]
        Task PolishWaferRecipeSettingVM_Cleanup();
        [OperationContract]
        Task PolishWaferRecipeSettingVM_IntervalAddCommandExcute();
        //[OperationContract]
        //Task PolishWaferRecipeSettingVM_CleaningDeleteCommandExcute(byte[] param);
        [OperationContract]
        void PolishWaferRecipeSettingVM_CleaningDelete(PolishWaferIndexModel param);
        [OperationContract]
        void PolishWaferRecipeSettingVM_SetPolishWaferIParam(byte[] param);

        [OperationContract]
        Task PolishWaferRecipeSettingVM_SetSelectedInfos(SelectionUIType selectiontype, byte[] cleaningparam, byte[] pwinfo, byte[] intervalparam, int intervalindex, int cleaningindex);

        //[OperationContract]
        //Task PolishWaferRecipeSettingVM_CleaningAddCommandExcute(object param);
        [OperationContract]
        void PolishWaferRecipeSettingVM_CleaningAdd(int param);
        //[OperationContract]
        //Task PolishWaferRecipeSettingVM_IntervalDeleteCommandExcute(object param);
        [OperationContract]
        void PolishWaferRecipeSettingVM_IntervalDelete(int param);
        #endregion

        #region //..PolishWafer
        [OperationContract]
        void SetPolishWaferParam(byte[] param);
        [OperationContract]
        Task<EventCodeEnum> DoManualPolishWaferCleaningCommand(byte[] param);
        [OperationContract]
        Task ManualPolishWaferFocusingCommand(byte[] param);

        #endregion

        #region //..Manul Contact

        #endregion


        #endregion

        #region ..Sequence Maker
        [OperationContract]
        SequenceMakerDataDescription GetSequenceMakerInfo();

        [OperationContract]
        List<DeviceObject> GetUnderDutDevices();

        [OperationContract]
        Task SequenceMakerVM_PageSwitched();
        [OperationContract]
        Task SequenceMakerVM_Cleanup();
        [OperationContract]
        Task SequenceMakerVM_SetMXYIndex(Point mxyindex);
        [OperationContract]
        Task<EventCodeEnum> SequenceMakerVM_GetUnderDutDices(MachineIndex mxy);
        [OperationContract]
        Task SequenceMakerVM_MoveToPrevSeqCommandExcute();
        [OperationContract]
        Task SequenceMakerVM_MoveToNextSeqCommandExcute();
        [OperationContract]
        Task SequenceMakerVM_InsertSeqCommandExcute();
        [OperationContract]
        Task SequenceMakerVM_DeleteSeqCommandExcute();
        [OperationContract]
        void SequenceMakerVM_ChangeAutoAddSeqEnable(bool flag);
        [OperationContract]
        Task SequenceMakerVM_MapMoveCommandExcute(object param);

        [OperationContract]
        Task SequenceMakerVM_SeqNumberSeletedCommandExcute(object param);

        [OperationContract]
        Task SequenceMakerVM_AutoMakeSeqCommandExcute();
        [OperationContract]
        Task SequenceMakerVM_DeleteAllSeqCommandExcute();
        #endregion

        #region //..InspectionVM
        [OperationContract]
        Task<InspcetionDataDescription> GetInspectionInfo();
        [OperationContract]
        Task InspectionVM_SetFromCommandExcute();

        [OperationContract]
        Task InspectionVM_SaveCommandExcute(InspcetionDataDescription info);
        [OperationContract]
        Task<EventCodeEnum> InspectionVM_CheckPMShiftLimit(double checkvalue);
        [OperationContract]
        Task InspectionVM_ApplyCommandExcute();
        [OperationContract]
        Task InspectionVM_SystemApplyCommandExcute();
        [OperationContract]
        Task InspectionVM_ClearCommandExcute();
        [OperationContract]
        Task InspectionVM_SystemClearCommandExcute();
        [OperationContract]
        Task InspectionVM_PrevDutCommandExcute();
        [OperationContract]
        Task InspectionVM_NextDutCommandExcute();
        [OperationContract]
        Task InspectionVM_PadPrevCommandExcute();
        [OperationContract]
        Task InspectionVM_PadNextCommandExcute();
        //[OperationContract]
        //Task InspectionVM_ManualSetIndexCommandExcute();
        [OperationContract]
        void InspectionVM_ChangeXManualCommandExcute();
        [OperationContract]
        void InspectionVM_ChangeYManualCommandExcute();
        [OperationContract]
        Task InspectionVM_PinAlignCommandExcute();
        [OperationContract]
        Task InspectionVM_WaferAlignCommandExcute();
        [OperationContract]
        void InspectionVM_ChangeXManualIndex(long index);

        [OperationContract]
        void InspectionVM_ChangeYManualIndex(long index);

        [OperationContract]
        Task InspectionVM_PageSwitched();
        [OperationContract]
        Task InspectionVM_Cleanup();

        [OperationContract]
        Task InspectionVM_SavePads();

        [OperationContract]
        Task InspectionVM_SaveTempOffset(ObservableDictionary<double, CatCoordinates> table);

        #endregion

        #region //..PMI Viewer

        [OperationContract]
        Task PMIViewer_PageSwitched();
        [OperationContract]
        int PMIViewer_GetTotalImageCount();

        [OperationContract]
        void PMIViewer_UpdateFilterDatas(DateTime Startdate, DateTime Enddate, PadStatusResultEnum Status);


        [OperationContract]
        void PMIViewer_LoadImage();

        [OperationContract]
        PMIImageInformationPack PMIViewer_GetImageFileData(int index);

        [OperationContract]
        ObservableCollection<PMIWaferInfo> PMIViewer_GetWaferlist();

        [OperationContract]

        void PMIViewer_WaferListClear();

        [OperationContract]
        void PMIViewer_ChangedWaferListItem(PMIWaferInfo pmiwaferinfo);

        #endregion

        #region //..Manual Contact VM
        [OperationContract]
        void MCMIncreaseX();
        [OperationContract]
        void MCMIncreaseY();
        [OperationContract]
        void MCMDecreaseX();
        [OperationContract]
        void MCMDecreaseY();
        //[OperationContract]
        //void MCMSetAlawaysMoveToFirstDut(bool flag);
        [OperationContract]
        void MCMSetIndex(EnumMovingDirection dirx, EnumMovingDirection diry);
        [OperationContract]
        ManaulContactDataDescription GetManualContactInfo();
        [OperationContract]
        bool GetManaulContactMovingStage();
        [OperationContract]
        SoakingRecipeDataDescription GetSaokingRecipeInfo();
        [OperationContract(AsyncPattern = true)]
        Task SetMCM_XYInex(Point index);
        [OperationContract]
        Task ManualContactVM_FirstContactSetCommandExcute();
        [OperationContract]
        Task ManualContactVM_AllContactSetCommandExcute();
        [OperationContract]
        Task ManualContactVM_ResetContactStartPositionCommandExcute();
        [OperationContract]
        Task ManualContactVM_OverDriveTBClickCommandExcute();
        [OperationContract]
        Task ManualContactVM_ChangeZUpStateCommandExcute();
        [OperationContract]
        Task ManualContactVM_MoveToWannaZIntervalPlusCommandExcute();
        [OperationContract]
        Task ManualContactVM_WantToMoveZIntervalTBClickCommandExcute();
        [OperationContract]
        Task ManualContactVM_SetOverDriveCommandExcute();
        [OperationContract]
        Task ManualContactVM_MoveToWannaZIntervalMinusCommandExcute();
        [OperationContract]
        Task ManualContactVM_PageSwitched();
        [OperationContract]
        Task ManualContactVM_Cleanup();
        [OperationContract]
        void MCMChangeOverDrive(string overdrive);


        [OperationContract]
        Task ManualContactVM_CPC_Z1_ClickCommandExcute();
        [OperationContract]
        void MCMChangeCPC_Z1(string z1);

        [OperationContract]
        Task ManualContactVM_CPC_Z2_ClickCommandExcute();
        [OperationContract]
        void MCMChangeCPC_Z2(string z2);
        [OperationContract]
        void GetOverDriveFromProbingModule();

        #endregion

        #region //..Soaking Recipe

        [OperationContract]
        void SoakingRecipeVM_DropDownClosedCommandExecute();

        #endregion

        #region //..Wafer Map Maker VM
        [OperationContract]
        void WaferMapMaker_UpdateWaferSize(EnumWaferSize waferSize);
        [OperationContract]
        void WaferMapMaker_UpdateWaferSizeOffset(double WaferSizeOffset);
        
        [OperationContract]
        void WaferMapMakerVM_UpdateDieSizeX(double diesizex);
        [OperationContract]
        void WaferMapMakerVM_WaferSubstrateType(WaferSubstrateTypeEnum wafersubstratetype);
        [OperationContract]
        void WaferMapMakerVM_UpdateDieSizeY(double diesizey);
        [OperationContract]
        void WaferMapMakerVM_UpdateThickness(double thickness);
        [OperationContract]
        void WaferMapMakeVM_UpdateEdgeMargin(double margin);
        [OperationContract]
        void WaferMapMakerVM_NotchAngle(double notchangle);
        [OperationContract]
        void WaferMapMakerVM_NotchType(WaferNotchTypeEnum notchtype);

        [OperationContract]
        void WaferMapMakerVM_NotchAngleOffset(double notchangleoffset);

        [OperationContract]
        Task WaferMapMaker_Cleanup();

        [OperationContract]
        Task WaferMapMakerVM_ApplyCreateWaferMapCommandExcute();
        [OperationContract]
        Task WaferMapMakerVM_MoveToWaferThicknessCommandExcute();
        [OperationContract]
        Task WaferMapMakerVM_AdjustWaferHeightCommandExcute();
        [OperationContract]
        Task<EventCodeEnum> WaferMapMakerVM_CmdImportWaferDataCommandExcute(Stream fileStream);
        [OperationContract]
        void WaferMapMakerVM_ImportFilePath(string filePath);
        [OperationContract]
        void WaferMapMakerVM_SetHighStandardParam(HeightPointEnum heightpoint);
        [OperationContract]
        HeightPointEnum WaferMapMakerVM_GetHeightProfiling();
        #endregion

        #region //..Dut Editor VM

        [OperationContract]
        byte[] DutEditor_GetDutlist();
        [OperationContract]
        void DutEditor_ChangedSelectedCoordM(MachineIndex param);
        [OperationContract]
        void DutEditor_ChangedChangedFirstDutM(MachineIndex param);

        [OperationContract]
        void DutEditor_ChangedAddCheckBoxIsChecked(bool? param);
        [OperationContract]
        DutEditorDataDescription GetDutEditorInfo();
        [OperationContract]
        Task<EventCodeEnum> DutEditor_CmdImportCardDataCommandExcute(Stream fileStream);
        [OperationContract]
        void DutEditor_ImportFilePath(string filePath);
        [OperationContract]
        Task DutEditor_InitializePalletCommandExcute();
        [OperationContract]
        Task DutEditor_DutAddCommandExcute();
        [OperationContract]
        Task DutEditor_DutDeleteCommandExcute();
        [OperationContract]
        void DutEditor_ZoomInCommandExcute();
        [OperationContract]
        void DutEditor_ZoomOutCommandExcute();
        [OperationContract]
        Task DutEditor_DutEditerMoveCommandExcute(EnumArrowDirection param);
        [OperationContract]
        Task DutEditor_DutAddMouseDownCommandExcute();

        [OperationContract]
        Task DutEditor_PageSwitched();
        [OperationContract]
        Task DutEditor_Cleanup();
        #endregion

        #region //.. Device Change (FilManager)

        [OperationContract]
        Task GetParamFromDevice(DeviceInfo device);

        [OperationContract]
        DeviceChangeDataDescription GetDeviceChangeInfo();
        [OperationContract]
        Task DeviceChange_GetDevList(bool isPageSwiching = false);

        [OperationContract]
        Task DeviceChange_GetParamFromDeviceCommandExcute(DeviceInfo device);

        [OperationContract]
        void DeviceChange_RefreshDevListCommandExcute();
        [OperationContract]
        void DeviceChange_ClearSearchDataCommandExcute();
        [OperationContract]
        void DeviceChange_SearchTBClickCommandExcute();
        [OperationContract]
        void DeviceChange_PageSwitchingCommandExcute();
        [OperationContract]
        Task DeviceChange_ChangeDeviceCommandExcute();
        [OperationContract]
        Task DeviceChangeWithName_ChangeDeviceCommandExcute(string deviceName);
        [OperationContract]
        Task DeviceChange_CreateNewDeviceCommandExcute();
        [OperationContract]
        Task DeviceChange_SaveAsDeviceCommandExcute();
        [OperationContract]
        Task DeviceChange_DeleteDeviceCommandExcute();

        //[OperationContract]
        //byte[] DeviceChange_CreateDeviceFolder(string devpath);
        //[OperationContract]
        //byte[] DeviceChange_SaveAsDeviceFolder(string devpath);
        #endregion

        #region ChuckPlanarity

        [OperationContract]
        Task ChuckPlanarity_ChuckMoveCommandExcute(EnumChuckPosition param);
        [OperationContract]
        Task ChuckPlanarity_MeasureOnePositionCommandExcute();
        [OperationContract]
        Task ChuckPlanarity_MeasureAllPositionCommandExcute();
        [OperationContract]
        Task ChuckPlanarity_SetAdjustPlanartyFuncExcute();
        //[OperationContract]
        //void ChuckPlanarity_ChangeSpecHeightValue(double value);

        [OperationContract]
        void ChuckPlanarity_ChangeMarginValue(double value);

        [OperationContract]
        void ChuckPlanarity_FocusingRangeValue(double value);

        [OperationContract]
        ChuckPlanarityDataDescription ChuckPlanarity_GetChuckPlanarityInfo();

        [OperationContract]
        Task ChuckPlanarity_PageSwitched();

        [OperationContract]
        Task ChuckPlanarity_Cleanup();

        [OperationContract]
        Task ChuckMoveCommand(ChuckPos param);

        #endregion

        #region // Temp. Module
        [OperationContract]
        ITempController GetTempModule();
        #endregion

        #region GPCC_Observation

        [OperationContract]
        Task GPCC_Observation_CardSettingCommandExcute();
        [OperationContract]
        Task GPCC_Observation_PogoSettingCommandExcute();
        [OperationContract]
        Task GPCC_Observation_PodSettingCommandExcute();

        //[OperationContract(IsOneWay = true)]
        [OperationContract]
        void GPCC_Observation_PatternWidthPlusCommandExcute();
        //[OperationContract(IsOneWay = true)]
        [OperationContract]
        void GPCC_Observation_PatternWidthMinusCommandExcute();
        //[OperationContract(IsOneWay = true)]
        [OperationContract]
        void GPCC_Observation_PatternHeightPlusCommandExcute();
        //[OperationContract(IsOneWay = true)]
        [OperationContract]
        void GPCC_Observation_PatternHeightMinusCommandExcute();
        [OperationContract]
        Task GPCC_Observation_WaferCamExtendCommandExcute();
        [OperationContract]
        Task GPCC_Observation_WaferCamFoldCommandExcute();
        [OperationContract]
        Task GPCC_Observation_MoveToCenterCommandExcute();
        [OperationContract]
        Task GPCC_Observation_ReadyToGetCardCommandExcute();
        [OperationContract]
        Task GPCC_Observation_RaiseZCommandExcute();
        [OperationContract]
        Task GPCC_Observation_ManualZMoveCommand(EnumProberCam camType, double Value);
        [OperationContract]
        Task GPCC_Observation_DropZCommandExcute();
        [OperationContract]
        void GPCC_Observation_IncreaseLightIntensityCommandExcute();
        [OperationContract]
        void GPCC_Observation_DecreaseLightIntensityCommandExcute();
        [OperationContract]
        Task GPCC_Observation_RegisterPatternCommandExcute();
        [OperationContract]
        Task GPCC_Observation_PogoAlignPointCommandExcute(EnumPogoAlignPoint point);
        [OperationContract]
        Task GPCC_Observation_SetMFModelLightsCommandExcute();
        [OperationContract]
        Task GPCC_Observation_SetMFChildLightsCommandExcute();

        [OperationContract]
        Task GPCC_Observation_RegisterPosCommandExcute();
        [OperationContract]
        GPCardChangeVMData GetGPCCData();
        [OperationContract]
        Task GPCC_Observation_MoveToMarkCommandExcute();
        [OperationContract]
        Task GPCC_Observation_SetSelectedMarkPosCommandExcute(int selectedmarkposIdx);
        [OperationContract]
        Task GPCC_Observation_SetSelectLightTypeCommandExcute(EnumLightType selectlight);
        [OperationContract]
        //void GPCC_Observation_SetLightValueCommandExcute(byte[] lightvalue);
        Task GPCC_Observation_SetLightValueCommandExcute(ushort lightvalue);
        [OperationContract]
        Task GPCC_Observation_SetZTickValueCommandExcute(int ztickvalue);
        [OperationContract]
        Task GPCC_Observation_SetZDistanceValueCommandExcute(double zdistancevalue);
        [OperationContract]
        Task GPCC_Observation_SetLightTickValueCommandExcute(int lighttickvalue);

        [OperationContract]
        string GPCC_GetPosition();
        [OperationContract]
        bool IsCheckCardPodState();

        [OperationContract]
        Task GPCC_Observation_PageSwitchCommandExcute(bool observation);
        [OperationContract]
        Task GPCC_Observation_CleanUpCommandExcute();
        [OperationContract]
        Task GPCC_Observation_FocusingCommandExcute();
        [OperationContract]
        Task GPCC_Observation_AlignCommandExcute();

        #endregion

        #region GPCC_OP
        //[OperationContract]
        //void GPCC_OP_SetAlignRetryValueCommandExcute(byte[] retrycnt);

        [OperationContract]
        Task GPCC_OP_SetCardFuncsRangeCommandExcute(double range);
        [OperationContract]
        Task GPCC_OP_SetUndockContactOffsetzCommandExcute(double offsetz);
        [OperationContract]
        Task GPCC_OP_SetContactOffsetzCommandExcute(double offsetz);
        [OperationContract]
        Task GPCC_OP_SetContactOffsetxCommandExcute(double offsetx);
        [OperationContract]
        Task GPCC_OP_SetContactOffsetyCommandExcute(double offsety);
        [OperationContract]
        void GPCC_OP_CardContactSettingZCommandExcute();

        [OperationContract]
        void GPCC_OP_CardUndockContactSettingZCommandExcute();
        [OperationContract]
        Task GPCC_OP_CardFocusRangeSettingZCommandExcute(double rangevalue);
        [OperationContract]
        Task GPCC_OP_SetAlignRetryCountCommandExcute(int retrycount);
        [OperationContract]
        Task GPCC_OP_SetDistanceOffsetCommandExcute(double distanceOffset);

        [OperationContract]
        Task GPCC_OP_SetCardTopFromChuckPlaneSettingCommandExcute(double distance);


        [OperationContract]
        Task GPCC_OP_SetCardCenterOffsetX1CommandExcute(double value);
        [OperationContract]
        Task GPCC_OP_SetCardCenterOffsetX2CommandExcute(double value);
        [OperationContract]
        Task GPCC_OP_SetCardCenterOffsetY1CommandExcute(double value);
        [OperationContract]
        Task GPCC_OP_SetCardCenterOffsetY2CommandExcute(double value);
        [OperationContract]
        Task GPCC_OP_SetCardPodCenterXCommandExcute(double value);
        [OperationContract]
        Task GPCC_OP_SetCardPodCenterYCommandExcute(double value);
        [OperationContract]
        Task GPCC_OP_SetCardLoadZLimitCommandExcute(double value);
        [OperationContract]
        Task GPCC_OP_SetCardLoadZIntervalCommandExcute(double value);
        [OperationContract]
        Task GPCC_OP_SetCardUnloadZOffsetCommandExcute(double value);




        [OperationContract]
        Task GPCC_OP_ZifToggleCommandExcute();
        [OperationContract]
        Task GPCC_OP_SmallRaiseChuckCommandExcute();
        [OperationContract]
        Task GPCC_OP_SmallDropChuckCommandExcute();
        [OperationContract]
        void GPCC_OP_MoveToZClearedCommandExcute();
        [OperationContract]
        Task GPCC_OP_MoveToLoaderCommandExcute();
        [OperationContract]
        Task GPCC_OP_MoveToBackCommandExcute();
        [OperationContract]
        Task GPCC_OP_MoveToFrontCommandExcute();
        [OperationContract]
        Task GPCC_OP_MoveToCenterCommandExcute();
        [OperationContract]
        Task GPCC_OP_RaisePodAfterMoveCardAlignPosCommandExcute();
        [OperationContract]
        Task GPCC_OP_RaisePodCommandExcute();
        [OperationContract]
        Task GPCC_OP_DropPodCommandExcute();
        [OperationContract]
        Task<EventCodeEnum> GPCC_OP_ForcedDropPodCommandExcute();
        [OperationContract]
        Task GPCC_OP_TopPlateSolLockCommandExcute();
        [OperationContract]
        Task GPCC_OP_TopPlateSolUnLockCommandExcute();
        [OperationContract]
        Task GPCC_OP_PCardPodVacuumOffCommandExcute();
        [OperationContract]
        Task GPCC_OP_PCardPodVacuumOnCommandExcute();
        [OperationContract]
        Task GPCC_OP_UpPlateTesterCOfftactVacuumOffCommandExcute();
        [OperationContract]
        Task GPCC_OP_UpPlateTesterContactVacuumOnCommandExcute();
        [OperationContract]
        Task GPCC_OP_UpPlateTesterPurgeAirOffCommandExcute();
        [OperationContract]
        Task GPCC_OP_UpPlateTesterPurgeAirOnCommandExcute();

        [OperationContract]
        Task GPCC_OP_PogoVacuumOffCommandExcute();
        [OperationContract]
        Task GPCC_OP_PogoVacuumOnCommandExcute();

        [OperationContract]
        Task GPCC_OP_DockCardCommandExcute();
        [OperationContract]
        Task GPCC_OP_UnDockCardCommandExcute();
        [OperationContract]
        Task GPCC_OP_CardAlignCommandExcute();
        [OperationContract]
        Task GPCC_OP_PageSwitchCommandExcute();
        [OperationContract]
        Task GPCC_OP_CleanUpCommandExcute();
        [OperationContract]
        byte[] GetGPCardChangeSysParamData();

        [OperationContract]
        byte[] GPCC_OP_GetDockSequence();
        [OperationContract]
        byte[] GPCC_OP_GetUnDockSequence();

        [OperationContract]
        int GPCC_OP_GetCurBehaviorIdx();


        [OperationContract]
        Task<CardChangeDevparamData> GetGPCardChangeDevParamData();

        [OperationContract]
        CardChangeVacuumAndIOStatus GetCCVacuumStatus();

        [OperationContract]
        void GPCC_OP_LoaderDoorOpenCommandExcute();

        [OperationContract]
        void GPCC_OP_LoaderDoorCloseCommandExcute();

        [OperationContract]
        bool GPCC_OP_IsLoaderDoorCloseCommandExcute(bool writelog = true);

        [OperationContract]
        bool GPCC_OP_IsLoaderDoorOpenCommandExcute(bool writelog = true);
        [OperationContract]
        void GPCC_OP_CardDoorOpenCommandExcute();
        [OperationContract]
        void GPCC_OP_CardDoorCloseCommandExcute();
        [OperationContract]
        void GPCC_Docking_PauseState();
        [OperationContract]
        void GPCC_Docking_StepUpState();
        [OperationContract]
        void GPCC_Docking_ContinueState();
        [OperationContract]
        void GPCC_Docking_AbortState();
        [OperationContract]
        void GPCC_UnDocking_PauseState();
        [OperationContract]
        void GPCC_UnDocking_StepUpState();
        [OperationContract]
        void GPCC_UnDocking_ContinueState();
        [OperationContract]
        void GPCC_UnDocking_AbortState();

        [OperationContract]
        bool GPCC_OP_IsCardExistExcute();

        [OperationContract]
        Task GPCC_SetWaitForCardPermitEnable(bool value);
        #endregion


        #region TemplateManager

        [OperationContract]
        EventCodeEnum CheckTemplateUsedType(string moduletype, bool applyload = true, int index = -1);

        #endregion
        #region LogTransfer
        [OperationContract]
        List<List<string>> LogTransfer_UpdateLogFile();
        [OperationContract]
        byte[] LogTransfer_OpenLogFile(string selectedFilePath);
        [OperationContract]
        ObservableDictionary<string, string> GetLogPathInfos();
        [OperationContract]
        EventCodeEnum SetLogPathInfos(ObservableDictionary<string, string> infos);
        //[OperationContract]
        //ObservableDictionary<string, string> GetImageLogPathInfos();
        //[OperationContract]
        //ImageLoggerParam GetImageLoggerParam();
        [OperationContract]
        void SetmageLoggerParam(ImageLoggerParam imageLoggerParam);
        [OperationContract]
        ImageDataSet GetImageDataSet(EnumProberModule moduletype, string moduleStartTime, string hashcode);
        #endregion
        [OperationContract]
        void LightAdmin_LoadLUT();

        [OperationContract]
        EventCodeEnum ReInitializeAndConnect();

        [OperationContract]
        EventCodeEnum CheckAndConnect();
        [OperationContract]
        EventCodeEnum FoupAllocated(FoupAllocatedInfo allocatedInfo);

        [OperationContract]
        void SetOperatorName(string name);
      
        [OperationContract]
        void Update_Error_MSG(string err_msg);

    }

    public enum StageLotDataEnum
    {
        WAFERLOADINGTIME,
        FOUPNUMBER,
        SLOTNUMBER,
        WAFERCOUNT,
        PROCESSEDWAFERCOUNTUNTILBEFORECARDCHANGE,
        TOUCHDOWNCOUNTUNTILBEFORECARDCHANGE,
        SETTEMP,
        DEVIATION,
        LOTSTATE,
        WAFERALIGNSTATE,
        PADCOUNT,
        PINALIGNSTATE,
        MARKALIGNSTATE,
        PROBINGSTATE,
        PROBINGOD,
        CLEARANCE,
        TEMPSTATE,
        STAGEMOVESTATE,
        LOTSTARTTIME,
        SOAKING,
        LOTENDTIME,
        LOTNAME,
        LOTMODE
    }

    public class StageLotDataComponent
    {
        public StageLotDataEnum stageLotDataEnum { get; set; }
        public string value{ get; set; }

        public StageLotDataComponent(StageLotDataEnum type, string val)
        {
            this.stageLotDataEnum = type;
            this.value = val;
        }
    }



    // Simple async result implementation.
    public class CompletedAsyncResult<T> : IAsyncResult
    {
        T data;

        public CompletedAsyncResult(T data)
        { this.data = data; }

        public T Data
        { get { return data; } }

        #region IAsyncResult Members
        public object AsyncState
        { get { return (object)data; } }

        public WaitHandle AsyncWaitHandle
        { get { throw new Exception("The method or operation is not implemented."); } }

        public bool CompletedSynchronously
        { get { return true; } }

        public bool IsCompleted
        { get { return true; } }
        #endregion
    }

}
