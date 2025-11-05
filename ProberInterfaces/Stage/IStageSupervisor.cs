using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using ProberInterfaces.Param;
using ProberInterfaces.LightJog;
using System.Windows.Input;
using ProberInterfaces.NeedleClean;
using ProberErrorCode;
using SystemExceptions;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using ProberInterfaces.Data;
using ProberInterfaces.Device;

namespace ProberInterfaces
{
    [Serializable, DataContract]
    public class IsChangeLogDate
    {
        private bool _IsChangeYear;
        public bool IsChangeYear
        {
            get { return _IsChangeYear; }
            set { _IsChangeYear = value; }
        }

        private bool _IsChangeMonth;

        public bool IsChangeMonth
        {
            get { return _IsChangeMonth; }
            set { _IsChangeMonth = value; }
        }

        private bool _IsChangeDay;

        public bool IsChangeDay
        {
            get { return _IsChangeDay; }
            set { _IsChangeDay = value; }
        }
    }
    public enum ChuckStateEnum
    {
        Empty,
        OnWaferNormal,
        OnWaferPolish,
    }

    public interface IChuckStateModule
    {
        ChuckStateEnum ChuckState { get; }

        void Syncronize();
    }



    public class ProbeCardObjectEventArgs : EventArgs
    {
        public ProbeCardObjectEventArgs(IProbeCard probeCardObj)
        {
            this.ProbeCard = probeCardObj;
        }
        public IProbeCard ProbeCard { get; set; }
    }


    public class WaferObjectEventArgs : EventArgs
    {
        public WaferObjectEventArgs(IWaferObject waferObj)
        {
            this.WaferObject = waferObj;
        }
        public IWaferObject WaferObject { get; set; }
    }

    [ServiceKnownType(typeof(EnumProberCam))]
    //[ServiceContract(CallbackContract = typeof(IStageSupervisorCallback))]
    [ServiceContract]
    public interface IStageSupervisor : IFactoryModule, IHasDevParameterizable, IHasSysParameterizable, INotifyPropertyChanged
    {
        //Property
        int AbsoluteIndex { get; }
        bool StageMoveFlag_Display { get; }
        [OperationContract]
        CellInitModeEnum GetStageInitState();
        void SetStageInitState(CellInitModeEnum e);
        double PinZClearance { get; set; }
        double PinMaxRegRange { get; set; }
        double PinMinRegRange { get; set; }
        double WaferRegRange { get; set; }
        double WaferMaxThickness { get; set; }

        //int SlotIndex { get; set; }
        //IRecipeService RecipeService { get; }
        IWaferObject WaferObject { get; set; }
        IMarkObject MarkObject { get; set; }
        IStageMove StageModuleState { get; }
        EventCodeEnum MOVETONEXTDIE();
        //EventCodeEnum SaveProbeCardData();
        IProbeCard ProbeCardInfo { get; set; }
        ProbingInfo ProbingProcessStatus { get; set; }
        ICommand ClickToMoveLButtonDownCommand { get; }
        [OperationContract]
        Task ClickToMoveLButtonDown(object enableClickToMove);
        [OperationContract]
        void MoveStageToTargetPos(object enableClickToMove);
        bool IsExistParamFile(String paramPath);
        double MoveTargetPosX { get; set; }
        double MoveTargetPosY { get; set; }
        double UserCoordXPos { get; set; }
        double UserCoordYPos { get; set; }
        double UserCoordZPos { get; set; }
        double UserWaferIndexX { get; set; }
        double UserWaferIndexY { get; set; }
        //IProbingSequence SequenceModule { get; set; }
        LightJogViewModel PnpLightJog { get; set; }
        IHexagonJogViewModel PnpMotionJog { get; set; }
        double WaferINCH6Size { get; }
        double WaferINCH8Size { get; }
        double WaferINCH12Size { get; }
        StageStateEnum StageMoveState { get; }
        bool IsModeChanging { get; set; }
        ICylinderManager IStageCylinderManager { get; set; }
        IStageMoveLockStatus IStageMoveLockStatus { get; set; }
        INeedleCleanObject NCObject { get; set; }
        ITouchSensorObject TouchSensorObject { get; set; }
        EventCodeEnum SetStageMode(GPCellModeEnum cellmodeenum);
        GPCellModeEnum StageMode { get; }
        IStageMoveLockParameter IStageMoveLockParam { get; set; }
        bool IsRecoveryMode { get; set; }
        StreamingModeEnum StreamingMode { get; set; }
        bool IsRecipeDownloadEnable { get; set; }
        //Event
        event EventHandler ChangedWaferObjectEvent;
        event EventHandler ChangedProbeCardObjectEvent;

        void StageSupervisorStateTransition(StageState state);
        //void ChangeScreenToSetup(IPnpSetup setupstep);
        //void ChangeScreenLotOP();
        //EventCodeEnum LoadDllModules(IEnumerable<PropertyInfo> pList);
        //void ChangePin(PinData changePin, List<List<PinData>> changePinListList);
        //Task<EventCodeEnum> SystemInit();
        Task<ErrorCodeResult> SystemInit();
        Task<EventCodeEnum> LoaderInit();
        EventCodeEnum SaveWaferObject();
        EventCodeEnum LoadWaferObject();
        EventCodeEnum SaveProberCard();
        EventCodeEnum LoadProberCard();
        EventCodeEnum SaveNCSysObject();
        EventCodeEnum LoadNCSysObject();
        EventCodeEnum SaveTouchSensorObject();
        EventCodeEnum LoadTouchSensorObject();
        EventCodeEnum CheckAvailableStageAbsMove(double xPos, double yPos, double zPos, double tPos, double PZPos, ref bool stagebusy);
        EventCodeEnum CheckAvailableStageRelMove(double xPos, double yPos, double zPos, double tPos, double PZPos, ref bool stagebusy);
        EventCodeEnum SetWaferObjectStatus();
        EventCodeEnum CheckWaferStatus(bool isExist);
        ExceptionReturnData ConvertToExceptionErrorCode(Exception err);
        bool CheckAxisBusy();
        bool CheckAxisIdle();
        string GetWaitCancelDialogHashCode();
        void CallWaferobjectChangedEvent();
        EventHandler MachineInitEvent { get; set; }

        EventHandler MachineInitEndEvent { get; set; }
        //bool IsAvailableLoaderRemoteMediator();

        //IStagePIV GetStagePIV();
        [OperationContract]
        List<string> GetStageDebugDates();
        [OperationContract]
        List<string> GetStageTempDates();
        [OperationContract]
        List<string> GetStagePinDates();
        [OperationContract]
        List<string> GetStagePMIDates();
        [OperationContract]
        List<string> GetStageLotDates();
        [OperationContract]
        byte[] GetLog(string date);
        [OperationContract]
        byte[] GetLogFromFilename(List<string> debug, List<string> temp, List<string> pin, List<string> pmi, List<string> lot);
        [OperationContract]
        byte[] GetPinImageFromStage(List<string> pinImage);
        [OperationContract]
        byte[] GetLogFromFileName(EnumUploadLogType logtype, List<string> data);
        [OperationContract]
        byte[] GetRMdataFromFileName(string filename);
        [OperationContract]
        byte[] GetODTPdataFromFileName(string filename);
        [OperationContract]
        byte[] GetDevice();
        [OperationContract]
        byte[] GetWaferObject();
        [OperationContract]
        byte[] GetProbeCardObject();
        [OperationContract]
        byte[] GetDIEs();

        [OperationContract]
        Element<AlignStateEnum> GetAlignState(AlignTypeEnum AlignType);

        [OperationContract]
        byte[] GetMarkObject();

        [OperationContract]
        bool IsServiceAvailable();
        [OperationContract]
        void InitStageService(int stageAbsIndex = 0);
        [OperationContract]
        Task InitLoaderClient();
        [OperationContract]
        void SetDynamicMode(DynamicModeEnum dynamicModeEnum);
        void DeInitService();
        [OperationContract]
        List<DeviceObject> GetDevices();
        [OperationContract]
        EnumSubsStatus GetWaferStatus();
        [OperationContract]
        EnumWaferType GetWaferType();

        [OperationContract(IsOneWay = true)]
        void BindDispService(string uri);
        [OperationContract(IsOneWay = true)]
        void BindDelegateEventService(string uri);
        [OperationContract(IsOneWay = true)]
        void BindDataGatewayService(string uri);
        [OperationContract]
        Task<EventCodeEnum> DoWaferAlign();
        [OperationContract]
        void DoLot();

        [OperationContract]
        void LotPause();
        [OperationContract]
        Task DoSystemInit(bool showMessageDialogFlag = true);
        [OperationContract]
        EventCodeEnum DoManualPinAlign(bool CheckStageMode = true);
        [OperationContract]
        EventCodeEnum DoManualWaferAlign(bool CheckStageMode = true);

        [OperationContract]
        Task<EventCodeEnum> DoPinAlign();
        //[OperationContract]
        //void DoPMI();
        [OperationContract]
        Task ChangeDeviceFuncUsingName(string devName);

        [OperationContract(IsOneWay = true)]
        void SetAcceptUpdateDisp(bool flag);
        [OperationContract(IsOneWay = true)]
        void SetDevice(byte[] device, string devicename, string lotid, string lotCstHashCode, bool loaddev, int foupnumber = -1, bool showprogress = true, bool manualDownload = false);
        [OperationContract(IsOneWay = true)]
        void SetNeedChangeParaemterInDeviceInfo(NeedChangeParameterInDevice needChangeParameter);
        [OperationContract]
        string GetDeviceName();
        [OperationContract]
        byte[] GetNCObject();
        [OperationContract(IsOneWay = true)]
        void SetWaitCancelDialogHashCode(string hashCode);
        [OperationContract(IsOneWay = true)]
        void SetErrorCodeAlarm(EventCodeEnum errorCode);

        //[OperationContract(IsOneWay = true)]
        [OperationContract]
        void SetMoveTargetPos(double xpos, double ypos);
        [OperationContract(IsOneWay = true)]
        void SetWaferMapCam(EnumProberCam cam);
        [OperationContract]
        EventCodeEnum NotifySystemErrorToConnectedCells(EnumLoaderEmergency emgtype);

        [OperationContract(IsOneWay = true)]
        void SetEMG(EventCodeEnum errorCode);

        [OperationContract(IsOneWay = true)]
        void SetVacuum(bool ison);

        [OperationContract]
        EventCodeEnum WaferHighViewIndexCoordMove(long mix, long miy);
        [OperationContract]
        EventCodeEnum WaferLowViewIndexCoordMove(long mix, long miy);
        [OperationContract(IsOneWay = true)]
        void SetProbeCardObject(IProbeCard param);
        [OperationContract]
        SubstrateInfoNonSerialized GetSubstrateInfoNonSerialized();

        [OperationContract]
        WaferObjectInfoNonSerialized GetWaferObjectInfoNonSerialize();

        [OperationContract(IsOneWay = true)]
        void WaferIndexUpdated(long xindex, long yindex);

        [OperationContract]
        EventCodeEnum CheckPinPadParameterValidity();

        [OperationContract]
        EventCodeEnum GetPinDataFromPads();

        [OperationContract]
        PROBECARD_TYPE GetProbeCardType();

        [OperationContract]
        int DutPadInfosCount();

        [OperationContract]
        EventCodeEnum InitGemConnectService();
        [OperationContract]
        EventCodeEnum DeInitGemConnectService();

        [OperationContract]
        string[] LoadEventLog(string lFileName);
        List<List<string>> UpdateLogFile();
        byte[] OpenLogFile(string selectedFilePath);

        [OperationContract]
        string GetLotErrorMessage();
        [OperationContract]
        EventCodeEnum HandlerVacOnOff(bool val, int stageindex =-1);
        [OperationContract]
        bool CheckUsingHandler(int stageindex = -1);
        [OperationContract]
        void LoadLUT();
        [OperationContract]
        (GPCellModeEnum, StreamingModeEnum) GetStageMode();
        [OperationContract]
        (DispFlipEnum disphorflip, DispFlipEnum dispverflip) GetDisplayFlipInfo();
        [OperationContract]
        (bool reverseX, bool reverseY) GetReverseMoveInfo();


        [OperationContract(IsOneWay = true)]
        void StopBeforeProbingCmd(bool stopBeforeProbing);
        [OperationContract(IsOneWay = true)]
        void StopAfterProbingCmd(bool stopAfterProbing);
        [OperationContract(IsOneWay = true)]
        void OnceStopBeforeProbingCmd(bool onceStopBeforeProbing);
        [OperationContract(IsOneWay = true)]
        void OnceStopAfterProbingCmd(bool onceStopAfterProbing);

        [OperationContract]
        EventCodeEnum CheckManualZUpState();
        [OperationContract]
        EventCodeEnum DoPinPadMatch_FirstSequence();
        [OperationContract]

        EventCodeEnum DO_ManualZUP();
        [OperationContract]
        EventCodeEnum DO_ManualZDown();
        [OperationContract]
        EventCodeEnum DoManualSoaking();
        IStageSlotInformation GetSlotInfo();
        EventCodeEnum SaveSlotInfo();

        [OperationContract]
        int GetWaferObjHashCode();

        EventCodeEnum SetStageLock(ReasonOfStageMoveLock reason);
        EventCodeEnum SetStageUnlock(ReasonOfStageMoveLock reason);

        StageLockMode GetStageLockMode();

        [OperationContract]
        void SetLotModeByForcedLotMode();
        [OperationContract]
        void ChangeLotMode(LotModeEnum mode);
        [OperationContract]
        bool IsForcedDoneMode();

        [OperationContract]
        void Set_TCW_Mode(bool isOn);
        [OperationContract]
        TCW_Mode Get_TCW_Mode();


        [OperationContract]
        bool IsMovingState();

        [OperationContract]
        void LoaderConnected();
        [OperationContract]
        EventCodeEnum ClearWaferStatus();

    }

    public interface IStageSlotInformation
    {
        EnumSubsStatus WaferStatus { get; set; }
        EnumWaferState WaferState { get; set; }
        EnumWaferType WaferType { get; set; }
        SubstrateSizeEnum WaferSize { get; set; }
        WaferNotchTypeEnum NotchType { get; set; }
        string WaferID { get; set; }
        double LoadingAngle { get; set; }
        double UnloadingAngle { get; set; }
        double OCRAngle { get; set; }
        int FoupIndex { get; set; }
        int SlotIndex { get; set; }
        int OriginSlotIndex { get; set; }
        ModuleID OriginHolder { get; set; }
        string CSTHashCode { get; set; }

    }

    [ServiceContract]
    public interface IStageMove : IModule, IFactoryModule
    {
        EnumAxisConstants PinViewAxis { get; }
        [OperationContract]
        bool IsServiceAvailable();
        [OperationContract]
        void InitService();
        [OperationContract]
        ModuleStateEnum GetModuleState();
        [OperationContract]
        EventCodeEnum MOVETONEXTDIE();
        [OperationContract(Name = "probingzup_overdirve")]
        EventCodeEnum ProbingZUP(double overdrive);
        [OperationContract(Name = "probingzup_wafercoord_pincoord")]
        EventCodeEnum ProbingZUP(WaferCoordinate wafercoord, PinCoordinate pincoord, double overdrive);
        [OperationContract(Name = "probingzup_nccoord_pincoord")]
        EventCodeEnum ProbingZUP(NCCoordinate nccoord, PinCoordinate pincoord, double overdrive);
        [OperationContract(Name = "probingzdown_wafercoord_pincoord")]
        EventCodeEnum ProbingZDOWN(WaferCoordinate wafercoord, PinCoordinate pincoord, double overdrive, double zclearance);
        [OperationContract(Name = "probingzdown_nccoord_pincoord")]
        EventCodeEnum ProbingZDOWN(NCCoordinate nccoord, PinCoordinate pincoord, double overdrive, double zclearance);
        [OperationContract(Name = "probingzdown_overdrive")]
        EventCodeEnum ProbingZDOWN(double overdrive, double zclearance);
        [OperationContract]
        EventCodeEnum MoveToSoaking(WaferCoordinate waferoffset, PinCoordinate pinoffset, double zclearance);
        [OperationContract]
        EventCodeEnum ZCLEARED();
        [OperationContract]
        EventCodeEnum CCZCLEARED();
        [OperationContract]
        EventCodeEnum MoveToBackPosition();
        [OperationContract]
        EventCodeEnum MoveToFrontPosition();
        [OperationContract]
        EventCodeEnum MoveToCenterPosition();
        [OperationContract]
        EventCodeEnum MoveToNcPadChangePosition();
        [OperationContract(Name = "waferhighviewmove_4pos")]
        EventCodeEnum WaferHighViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "waferlowviewmove_4pos")]
        EventCodeEnum WaferLowViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "pinhighviewmove_4pos")]
        EventCodeEnum PinHighViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "pinlowviewmove_4pos")]
        EventCodeEnum PinLowViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "waferhighviewmove_3pos")]
        EventCodeEnum WaferHighViewMove(double xpos, double ypos, double zpos, bool NotUseHeightProfile = false, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "waferlowviewmove_3pos")]
        EventCodeEnum WaferLowViewMove(double xpos, double ypos, double zpos, bool NotUseHeightProfile = false, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        //EventCodeEnum WaferLowViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "pinhighviewmove_3pos")]
        EventCodeEnum PinHighViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "pinlowviewmove_3pos")]
        EventCodeEnum PinLowViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "waferhighviewmove_2pos")]
        EventCodeEnum WaferHighViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "waferlowviewmove_2pos")]
        EventCodeEnum WaferLowViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "pinhighviewmove_2pos")]
        EventCodeEnum PinHighViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "pinlowviewmove_2pos")]
        EventCodeEnum PinLowViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "waferhighviewmove_axis")]
        EventCodeEnum WaferHighViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "waferlowviewmove_axis")]
        EventCodeEnum WaferLowViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "pinhighviewmove_axis")]
        EventCodeEnum PinHighViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "pinlowviewmove_axis")]
        EventCodeEnum PinLowViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "cardviewmove_2pos")]
        EventCodeEnum CardViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "cardviewmove_axis")]
        EventCodeEnum CardViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract]
        EventCodeEnum PogoViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "vmviewmove_4pos")]
        EventCodeEnum VMViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "vmviewmove_3pos")]
        EventCodeEnum VMViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "vmwaferhighviewmove_4pos")]
        EventCodeEnum VMWaferHighViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "vmwaferhighviewmove_3pos")]
        EventCodeEnum VMWaferHighViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "vmwaferhighviewmove_2pos")]
        EventCodeEnum VMWaferHighViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract]
        EventCodeEnum VMRelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract]
        EventCodeEnum VMAbsMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract]
        EventCodeEnum MoveLoadingPosition(double offsetvalue);
        [OperationContract]
        EventCodeEnum MoveTCW_Position();
        [OperationContract]
        EventCodeEnum MoveLoadingOffsetPosition(double x, double y, double z, double t);
        [OperationContract]
        EventCodeEnum MovePadToPin(WaferCoordinate waferoffset, PinCoordinate pinoffset, double zclearance);
        [OperationContract]
        EventCodeEnum Handlerhold(long timeout = 0);
        [OperationContract]
        EventCodeEnum Handlerrelease(long timeout = 0);
        [OperationContract]
        EventCodeEnum CCRotLock(long timeout);
        [OperationContract]
        EventCodeEnum CCRotUnLock(long timeout);
        [OperationContract]
        EventCodeEnum NCPadUp();
        [OperationContract]
        EventCodeEnum NCPadDown();
        [OperationContract]
        EventCodeEnum WaferHighCamCoordMoveNCpad(NCCoordinate nccoord, double offsetZ);
        [OperationContract]
        EventCodeEnum WaferLowCamCoordMoveNCpad(NCCoordinate nccoord, double offsetZ);
        [OperationContract]
        EventCodeEnum SetWaferCamBasePos(bool value);
        [OperationContract]
        EventCodeEnum TouchSensorSensingMoveStage(WaferCoordinate wcoord, PinCoordinate pincoord, double offsetZ);
        [OperationContract]
        EventCodeEnum TouchSensorSensingMoveNCPad(NCCoordinate nccoord, PinCoordinate pincoord, double offsetZ);
        [OperationContract(Name = "movencpad")]
        EventCodeEnum ProbingCoordMoveNCPad(NCCoordinate nccoord, PinCoordinate pincoord, double offsetZ);
        [OperationContract(Name = "movencpad_speed")]
        EventCodeEnum ProbingCoordMoveNCPad(NCCoordinate nccoord, PinCoordinate pincoord, double offsetZ, double zspeed, double zacc);
        [OperationContract]
        EventCodeEnum TouchSensorLowViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract]
        EventCodeEnum TouchSensorHighViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract]
        EventCodeEnum WaferHighViewIndexMove(long mach_x, long mach_y, double zpos = 0.0, bool NotUseHeightProfile = false);
        [OperationContract]
        EventCodeEnum WaferLowViewIndexMove(long mach_x, long mach_y, double zpos = 0.0, bool NotUseHeightProfile = false);
        [OperationContract]
        EventCodeEnum MoveToCardHolderPositionAndCheck();
        [OperationContract]
        EventCodeEnum CC_AxisMoveToPos(ProbeAxisObject axis, double pos, double velScale, double accScale);
        [OperationContract]
        EventCodeEnum LockCCState();
        [OperationContract]
        EventCodeEnum UnLockCCState();
        [OperationContract]
        StageStateEnum GetState();
        [OperationContract]
        EventCodeEnum ReadVacuum(out bool val);
        [OperationContract]
        EventCodeEnum VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0);
        [OperationContract]
        EventCodeEnum WaitForVacuum(bool val, long timeout = 0);
        [OperationContract]
        EventCodeEnum MonitorForVacuum(bool val, long sustain = 0, long timeout = 0);        
        [OperationContract]
        EventCodeEnum ChuckMainVacOff();
        [OperationContract]
        EventCodeEnum CheckWaferStatus(bool isExist);

        [OperationContract]
        EventCodeEnum MoveStageRepeatRelMove(double xpos, double ypos, double xvel, double xacc, double yvel, double yacc);
        [OperationContract(Name = "stagerelmove_2pos")]
        EventCodeEnum StageRelMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract(Name = "stagerelmove_axis")]
        EventCodeEnum StageRelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract]
        EventCodeEnum StageVMove(ProbeAxisObject axis, double vel, EnumTrjType trjtype);
        [OperationContract]
        EventCodeEnum StageMoveStop(ProbeAxisObject axis);
        [OperationContract]
        EventCodeEnum NCPadMove(NCCoordinate nccoord, PinCoordinate pincoord);
        [OperationContract]
        EventCodeEnum TiltMove(ProbeAxisObject axis, double pos);
        [OperationContract]
        EventCodeEnum TiltingMove(double tz1offset, double tz2offset, double tz3offset);
        [OperationContract(Name = "airblowmove_2pos")]
        EventCodeEnum AirBlowMove(double xpos, double ypos, double zpos);
        [OperationContract]
        EventCodeEnum AirBlowAirOnOff(bool val);
        [OperationContract(Name = "airblowmove_axis")]
        EventCodeEnum AirBlowMove(EnumAxisConstants axis, double pos, double speed, double acc);
        [OperationContract]
        EventCodeEnum ChuckTiltMove(double RPos, double TTPos);
        [OperationContract]
        EventCodeEnum MoveToMark();
        [OperationContract]
        EventCodeEnum StageSystemInit();
        [OperationContract]
        EventCodeEnum CardChageMoveToIN();
        [OperationContract]
        EventCodeEnum CardChageMoveToIDLE();
        [OperationContract]
        EventCodeEnum CardChageMoveToOUT();
        [OperationContract]
        EventCodeEnum LoaderDoorOpen();
        [OperationContract]
        EventCodeEnum LoaderDoorClose();
        [OperationContract]
        EventCodeEnum CardDoorOpen();
        [OperationContract]
        EventCodeEnum CardDoorClose();

        [OperationContract]
        EventCodeEnum LoaderDoorCloseRecovery();
        [OperationContract]
        EventCodeEnum IsLoaderDoorOpen(ref bool isloaderdooropen, bool writelog = true);
        [OperationContract]
        EventCodeEnum IsLoaderDoorClose(ref bool isloaderdoorclose, bool writelog = true);
        [OperationContract]
        EventCodeEnum IsCardDoorOpen(ref bool iscarddooropen, bool writelog = true);
        [OperationContract]
        bool IsCardExist();
        [OperationContract]
        EventCodeEnum FrontDoorLock();
        [OperationContract]
        EventCodeEnum FrontDoorUnLock();
        [OperationContract]
        EventCodeEnum IsFrontDoorLock(ref bool isfrontdoorlock);
        [OperationContract]
        EventCodeEnum IsFrontDoorUnLock(ref bool isfrontdoorunlock);
        [OperationContract]
        EventCodeEnum IsFrontDoorOpen(ref bool isfrontdooropen);
        [OperationContract]
        EventCodeEnum IsFrontDoorClose(ref bool isfrontdoorclose);
        [OperationContract]
        bool IsHandlerholdWafer();
        [OperationContract]
        EventCodeEnum ManualZDownMove();
        [OperationContract]
        EventCodeEnum ManualRelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract]
        EventCodeEnum ManualAbsMove(double posX, double posY, double posZ, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract]
        Task<EventCodeEnum> StageHomeOffsetMove();
        [OperationContract]
        Task<EventCodeEnum> LoaderHomeOffsetMove();


        [OperationContract]
        EventCodeEnum StageLock();

        [OperationContract]
        EventCodeEnum StageUnlock();

        // Component Verification 기능을 통한 Wafer Align 수행시 WaferCamBrige를 접지 않는 옵션에 대한 Flag를 설정하는 함수
        [OperationContract]
        EventCodeEnum SetNoRetractWaferCamBridgeWhenMarkAlignFlag(bool isFlagOn);
        [OperationContract]
        EventCodeEnum ThreeLegUp(long timeout = 0);
        [OperationContract]
        EventCodeEnum ThreeLegDown(long timeout = 0);

    }

    //public interface IStageSupervisorCallback
    //{
    //    [OperationContract]
    //    bool IsServiceAvailable();
    //    [OperationContract(IsOneWay = true)]
    //    void UpdateWaferAlignState(int chuckindex, AlignTypeEnum type, Element<AlignStateEnum> alignstate);

    //    [OperationContract(IsOneWay = true)]
    //    void SetTitleMessage(int cellno, string message, string foreground = "", string background = "");

    //}


    public abstract class StageState
    {

        public abstract ModuleStateEnum GetModuleState();
        public abstract EventCodeEnum MOVETONEXTDIE();
        public abstract EventCodeEnum ProbingZUP(double overdrive);
        public abstract EventCodeEnum ProbingZUP(WaferCoordinate wafercoord, PinCoordinate pincoord, double overdrive,ProbingSpeedRateList FeedRateList = null);
        public abstract EventCodeEnum ProbingZUP(NCCoordinate nccoord, PinCoordinate pincoord, double overdrive);
        public abstract EventCodeEnum ProbingZDOWN(WaferCoordinate wafercoord, PinCoordinate pincoord, double overdrive, double zclearance);
        public abstract EventCodeEnum ProbingZDOWN(NCCoordinate nccoord, PinCoordinate pincoord, double overdrive, double zclearance);
        public abstract EventCodeEnum ProbingZDOWN(double overdrive, double zclearance);
        public abstract EventCodeEnum MoveToSoaking(WaferCoordinate waferoffset, PinCoordinate pinoffset, double zclearance);
        public abstract EventCodeEnum ZCLEARED();
        public abstract EventCodeEnum CCZCLEARED();
        public abstract EventCodeEnum CheckWaferStatus(bool isExist);
        public abstract EventCodeEnum MoveToBackPosition();
        public abstract EventCodeEnum MoveToFrontPosition();
        public abstract EventCodeEnum MoveToCenterPosition();
        public abstract EventCodeEnum MoveToNcPadChangePosition();
        public abstract EventCodeEnum WaferHighViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum WaferLowViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum PinHighViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum PinLowViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum WaferHighViewMove(double xpos, double ypos, double zpos, bool NotUseHeightProfile = false, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum WaferLowViewMove(double xpos, double ypos, double zpos, bool NotUseHeightProfile = false, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum PinHighViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum PinLowViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum WaferHighViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum WaferLowViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum PinHighViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum PinLowViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum WaferHighViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum WaferLowViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum PinHighViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum PinLowViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum CardViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum CardViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum PogoViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum MoveLoadingPosition(double offsetvalue);
        public abstract EventCodeEnum MoveTCW_Position();
        public abstract EventCodeEnum MoveLoadingOffsetPosition(double x, double y, double z, double t);
        public abstract EventCodeEnum MovePadToPin(WaferCoordinate waferoffset, PinCoordinate pinoffset, double zclearance);
        public abstract EventCodeEnum ThreeLegUp(long timeout = 0);
        public abstract EventCodeEnum ThreeLegDown(long timeout = 0);
        public abstract EventCodeEnum BernoulliHandlerhold();
        public abstract EventCodeEnum BernoulliHandlerrelease();
        public abstract EventCodeEnum CCRotLock(long timeout);
        public abstract EventCodeEnum CCRotUnLock(long timeout);
        public abstract EventCodeEnum NCPadUp();
        public abstract EventCodeEnum NCPadDown();
        public abstract EventCodeEnum WaferHighCamCoordMoveNCpad(NCCoordinate nccoord, double offsetZ);
        public abstract EventCodeEnum WaferLowCamCoordMoveNCpad(NCCoordinate nccoord, double offsetZ);
        public abstract EventCodeEnum SetWaferCamBasePos(bool value);
        public abstract EventCodeEnum TouchSensorSensingMoveNCPad(NCCoordinate nccoord, PinCoordinate pincoord, double offsetZ);
        public abstract EventCodeEnum TouchSensorSensingMoveStage(WaferCoordinate wcoord, PinCoordinate pincoord, double zclearance);
        public abstract EventCodeEnum ProbingCoordMoveNCPad(NCCoordinate nccoord, PinCoordinate pincoord, double offsetZ);
        public abstract EventCodeEnum ProbingCoordMoveNCPad(NCCoordinate nccoord, PinCoordinate pincoord, double offsetZ, double zspeed, double zacc);

        public abstract EventCodeEnum WaferHighViewIndexMove(long mach_x, long mach_y, double zpos = 0, bool NotUseHeightProfile = false);
        public abstract EventCodeEnum WaferLowViewIndexMove(long mach_x, long mach_y, double zpos = 0, bool NotUseHeightProfile = false);

        public abstract EventCodeEnum TouchSensorLowViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum TouchSensorHighViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum MoveToCardHolderPositionAndCheck();
        public abstract EventCodeEnum CC_AxisMoveToPos(ProbeAxisObject axis, double pos, double velScale, double accScale);
        public abstract EventCodeEnum LockCCState();
        public abstract EventCodeEnum UnLockCCState();
        public abstract StageStateEnum GetState();
        public abstract EventCodeEnum ReadVacuum(out bool val);
        public abstract EventCodeEnum VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0);
        public abstract EventCodeEnum WaitForVacuum(bool val, long timeout = 0);
        public abstract EventCodeEnum MonitorForVacuum(bool val, long sustain = 0, long timeout = 0);
        public abstract EventCodeEnum ChuckMainVacOff();
        public abstract EventCodeEnum MoveStageRepeatRelMove(double xpos, double ypos, double xvel, double xacc, double yvel, double yacc);
        public abstract EventCodeEnum StageRelMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum StageRelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum StageVMove(ProbeAxisObject axis, double vel, EnumTrjType trjtype);
        public abstract EventCodeEnum StageMoveStop(ProbeAxisObject axis);
        public abstract EventCodeEnum NCPadMove(NCCoordinate nccoord, PinCoordinate pincoord);
        public abstract EventCodeEnum TiltMove(ProbeAxisObject axis, double pos);
        public abstract EventCodeEnum TiltingMove(double tz1offset, double tz2offset, double tz3offset);
        public abstract EventCodeEnum AirBlowMove(double xpos, double ypos, double zpos);
        public abstract EventCodeEnum AirBlowAirOnOff(bool val);
        public abstract EventCodeEnum AirBlowMove(EnumAxisConstants axis, double pos, double speed, double acc);
        public abstract EventCodeEnum ChuckTiltMove(double RPos, double TTPos);
        public abstract EventCodeEnum MoveToMark();
        public abstract EventCodeEnum StageSystemInit();
        public abstract EventCodeEnum CardChageMoveToIN();
        public abstract EventCodeEnum CardChageMoveToIDLE();
        public abstract EventCodeEnum CardChageMoveToOUT();
        public abstract EventCodeEnum VMViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum VMViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum VMWaferHighViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum VMWaferHighViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum VMWaferHighViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum VMRelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum VMAbsMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum LoaderDoorOpen();
        public abstract EventCodeEnum LoaderDoorClose();
        public abstract EventCodeEnum CardDoorOpen();
        public abstract EventCodeEnum CardDoorClose();
        public abstract EventCodeEnum LoaderDoorCloseRecovery();
        public abstract EventCodeEnum IsLoaderDoorOpen(ref bool isloaderdooropen);
        public abstract EventCodeEnum IsLoaderDoorClose(ref bool isloaderdoorclose);
        public abstract EventCodeEnum IsCardDoorOpen(ref bool iscarddooropen);
        public abstract EventCodeEnum FrontDoorLock();
        public abstract EventCodeEnum FrontDoorUnLock();
        public abstract EventCodeEnum IsFrontDoorLock(ref bool isfrontdoorlock);
        public abstract EventCodeEnum IsFrontDoorUnLock(ref bool isfrontdoorunlock);
        public abstract EventCodeEnum IsFrontDoorOpen(ref bool isfrontdooropen);
        public abstract EventCodeEnum IsFrontDoorClose(ref bool isfrontdoorclose);
        public abstract bool IsHandlerholdWafer();
        public abstract EventCodeEnum ManualZDownMove();
        public abstract EventCodeEnum ManualRelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract EventCodeEnum ManualAbsMove(double posX, double posY, double posZ, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        public abstract Task<EventCodeEnum> StageHomeOffsetMove();
        public abstract Task<EventCodeEnum> LoaderHomeOffsetMove();
        public abstract EventCodeEnum StageLock();
        public abstract EventCodeEnum StageUnlock();
        // Component Verification 기능을 통한 Wafer Align 수행시 WaferCamBrige를 접지 않는 옵션에 대한 Flag를 설정하는 함수
        public abstract EventCodeEnum SetNoRetractWaferCamBridgeWhenMarkAlignFlag(bool isFlagOn);
    }

    public enum StageStateEnum
    {
        UNKNOWN = 0,
        ERROR,
        IDLE,
        MOVING,
        MOVETONEXTDIE,
        Z_CLEARED,
        Z_UP,
        Z_IDLE,
        Z_DOWN,
        MOVETOLOADPOS,
        MOVETODOWNPOS,
        WAFERVIEW,
        WAFERHIGHVIEW,
        WAFERLOWVIEW,
        PINVIEW,
        PINHIGHVIEW,
        PINLOWVIEW,
        PROBING,
        SOAKING,
        CARDCHANGE,
        TILT,
        AIRBLOW,
        CHUCKTILT,
        MARK,
        VISIONMAPPING,
        NC_CLEANING,
        NC_PADVIEW,
        NC_SENSING,
        NC_SENSORVIEW,
        MANUAL,
        STAGELOCK,
        LOCK,
        TCW
    }

    [DataContract]
    public class StageMoveInfo:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private int _ChuckIndex;
        [DataMember]
        public int ChuckIndex
        {
            get { return _ChuckIndex; }
            set
            {
                if (value != _ChuckIndex)
                {
                    _ChuckIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _StageMove;
        [DataMember]
        public string StageMove
        {
            get { return _StageMove; }
            set
            {
                if (value != _StageMove)
                {
                    _StageMove = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
