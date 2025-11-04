using ProberErrorCode;
using ProberInterfaces.Enum;
using ProberInterfaces.Param;
using ProberInterfaces.SequenceRunner;
using ProberInterfaces.Vision;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;


namespace ProberInterfaces.CardChange
{
    public interface ICardChangeModule : IStateModule, IHasSysParameterizable, IHasDevParameterizable
    {
        IParam CcSysParams_IParam { get; set; }
        IParam CIDSysParams_IParam { get; set; }
        IParam CcDevParams_IParam { get; set; }
        IParam GPCCObservationTempParams_IParam { get; set; }
        ISequenceBehaviors CCBehaviors { get; set; }
        ISequenceBehaviors CCDockBehaviors { get; set; }
        ISequenceBehaviors CCUnDockBehaviors { get; set; }
        int CurBehaviorIdx { get; set; }
        ICardChangeState CardChangeState { get; }
        bool StartCardDockingFlag { get; set; }
        bool IsCardStuck { get; set; }
        bool IsDocked { get; }
        bool Observation_DockContinue { get; set; }
        int Behavior_Count { get; set; }
        ObservableCollection<IOPortDescripter<bool>> InputPorts { get; set; }
        ObservableCollection<ISequenceBehaviorGroupItem> StepGroupCollection { get; set; }
        CardChangeModuleStateEnum CCRecoveryState { get; set; }
        int CCStartPoint { get; set; }

        ObservableCollection<ISequenceBehaviorGroupItem> GetCcGroupCollection();
        ObservableCollection<ISequenceBehaviorGroupItem> GetCcDockCollection();
        //ObservableCollection<ISequenceBehaviorGroupItem> GetCcUnDockCollection();

        ObservableCollection<ISequenceBehaviorGroupItem> GetTHDockGroupCollection(THDockType type);
        EventCodeEnum CardChangeInit();
        EventCodeEnum GPCardChangeInit();

        EventCodeEnum SaveGPCCObservationTempParam();

        Task<EventCodeEnum> BehaviorRun(ISequenceBehavior behavior);
        EventCodeEnum CardIDValidate(string CardID, out string Msg);
        EventCodeEnum SetProbeCardID(string ID);
        bool GetCardIDValidationEnable();
        string GetProbeCardID();
        bool GetWaitForCardPermitEnable();
        EventCodeEnum SetWaitForCardPermitEnable(bool enable);
        EnumCardChangeType GetCCType();
        EnumCardDockType GetCCDockType();
        bool GetCardDoorAttached();
        double GetCCActivatableTemp();
        bool IsExistCard(bool bHolderCheck = false);
        bool IsCheckCardPodState();
        void SetCardDockingStatus(EnumCardDockingStatus status);
        EnumCardDockingStatus GetCardDockingStatus();

        //EventCodeEnum IsZifLockStateValid();
        EventCodeEnum IsZifRequestedState(bool lock_request, bool writelog = true);

        EventCodeEnum IsTeadLockRequestedState(bool lock_request, bool writelog = true);

        EventCodeEnum IsTeadClampLockRequestedState(bool lock_request, bool writelog = true);
        //void UpdateDockState();
        //void UpdateUnDockState();
        void UpdateErrorMessage(string err_msg);
        ISequenceBehaviors GetSequence(ISequenceBehaviors sequence);
        ISequenceBehavior GetNextBehavior();
        ISequenceBehavior GetBehavior();
        ObservableCollection<ISequenceBehaviorGroupItem> GetCCCommandParamCollection();
        void Dock_PauseCommand();
        void Dock_StepUpCommand();
        void Dock_ContinueCommand();
        void Dock_AbortCommand();
        void UnDock_AbortCommand();
        void UnDock_PauseCommand();
        void UnDock_StepUpCommand();
        void UnDock_ContinueCommand();
        void WaitForCardPermission();
        void ReleaseWaitForCardPermission();
        ObservableCollection<ISequenceBehaviorRun> GetPreCheckBehavior();
        ObservableCollection<ISequenceBehaviorRun> GetPostCheckBehavior();
        IBehaviorResult GetExecutionResult();
        void SetExecutionResult(IBehaviorResult result);
        void returnFirstErrorInfo(IBehaviorResult result, out string reason, out EventCodeEnum errorCodeEnum);
        void SetIsDocked(bool IsDocked);
        void SetIsCardExist(bool IsCardExist);
        EventCodeEnum LoaderNotifyCardStatus();
        bool IsCheckCarrierIsOnPCardPod();
        EventCodeEnum IsCCAvailableSatisfied(bool needToSetTempToken);
        bool NeedToSetCCActivatableTemp();
        EventCodeEnum IsAvailableToSetOtherThanCCActiveTemp();
        EventCodeEnum RecoveryCCBeforeTemp();
        EventCodeEnum SetCCActiveTemp();
        void AbortCardChange();
    }
    public interface ICardChangeSysParam
    {
        Element<EnumCardChangeType> CardChangeType { get; set; }
        Element<EnumCardDockType> CardDockType { get; set; }

        bool GPTesterVacSeqSkip { get; set; }
        double GP_LoadingPosT { get; set; }
        double GP_CardCheckOffsetZ { get; set; }

        double GP_ContactCorrectionX { get; set; }
        double GP_ContactCorrectionY { get; set; }
        double GP_ContactCorrectionZ { get; set; }

        double GP_Undock_ContactCorrectionZ { get; set; }

        double GP_Undock_CardContactPosX { get; set; }
        double GP_Undock_CardContactPosY { get; set; }
        double GP_Undock_CardContactPosZ { get; set; }
        double GP_Undock_CardContactPosT { get; set; }

        PinCoordinate GP_PogoCenter { get; set; }
        List<WaferCoordinate> CardPodMarkPosList { get; set; }
        List<PinCoordinate> GP_SearchedPogoMarkPosList { get; set; }
        List<PinCoordinate> GP_PogoMarkPosList { get; set; }
        List<PinCoordinate> GP_SearchedPogoMarkPosList3P { get; set; }
        List<PinCoordinate> GP_PogoMarkPosList3P { get; set; }
        Element<EnumPogoAlignPoint> PogoAlignPoint { get; set; }

        List<WaferCoordinate> GP_SearchedCardMarkPosList { get; set; }
        List<WaferCoordinate> GP_CardMarkPosList { get; set; }
        List<PinBaseFiducialMarkParameter> PinBaseFiducialMarkInfos { get; set; }
        WaferCoordinate PodCenterOffset { get; set; }
        ushort GP_WL_COAXIAL { get; set; }
        ushort GP_WL_OBLIQUE { get; set; }
        ushort GP_PL_COAXIAL { get; set; }
        ushort GP_PL_OBLIQUE { get; set; }
        int PatternMatchingRetryCount { get; set; }
        double DistanceOffset { get; set; }

        Element<double> CardDockPosX { get; set; }
        Element<double> CardDockPosY { get; set; }
        Element<double> CardDockPosZ { get; set; }
        Element<double> CardDockPosT { get; set; }


        Element<double> CardHolderCheckPosX { get; set; }
        Element<double> CardHolderCheckPosY { get; set; }
        Element<double> CardHolderCheckPosT { get; set; }
        Element<double> CardHolderCheckPosZ { get; set; }

        bool GP_ManualPogoAlignMode { get; set; }
        bool IsCardExist { get; set; }
        bool IsDocked { get;}

        bool MoveToCardHolderPosEnable { get; set; }

        Element<CatCoordinates> CardTransferPos { get; set; }
        Element<double> CardTransferOffsetX { get; set; }
        Element<double> CardTransferOffsetY { get; set; }
        Element<double> CardTransferOffsetZ { get; set; }
        Element<double> CardTransferOffsetT { get; set; }

        Element<double> CardPodCenterX { get; set; }
        Element<double> CardPodCenterY { get; set; }
        Element<double> CardPodCenterZ { get; set; }
        Element<double> CardTopFromChuckPlane { get; set; }
        Element<double> CardPodDeg { get; set; }
        Element<double> CardPodRadiusMax { get; set; }
        Element<double> CardPodRadiusMin { get; set; }
        Element<double> CardPodMinHeight { get; set; }


        Element<double> DD_CardDockZInterval{ get; set; }
        Element<double> DD_CardDockZMaxValue { get; set; }
        Element<double> DD_CardUndockZOffeset { get; set; }

        List<ProberCardListParameter> ProberCardList { get; set; }
        int SnailPointsOdd { get; set; }

        Element<bool> WaitForCardPermitEnable { get; set; }
        Element<double> CardContactRadiusLimit { get; set; }
        Element<bool> WaitTesterResponse { get; set; }
        Element<string> WMBIdentifier { get; set; }
        AlignStateEnum CardAlignState { get; set; }
        Element<int> PatternDistMargin { get; set; }
    }

    public interface ICardChangeDevParam
    {
        CatCoordinates CCStageBackPos { get; set; }
        CatCoordinates CCStageFrontDoor { get; set; }
        CatCoordinates CCStageCarrierPos { get; set; }
        Element<EnumManipulatorType> ManipulatorType { get; set; }
        Element<double> CCSVelScale { get; set; }
        Element<double> CCSAccScale { get; set; }
        Element<double> SACCOffSetY { get; set; }
        Element<long> CLAMPLock_Timeout { get; set; }
        Element<long> TPLock_Timeout { get; set; }
        Element<long> ZIFLock_Timeout { get; set; }
        Element<int> ZIFSEQTYPE { get; set; }
        Element<int> CLPLKSEQTYPE { get; set; }
        Element<int> ControllerType { get; set; }
        Element<bool> BlTubSynqNetControl { get; set; }
        Element<bool> FrontDoorOpenSensorAttached { get; set; }
        Element<bool> BlZIFoutputMode { get; set; }
        Element<bool> AddedPCDetectEXSensorOnPlate { get; set; }
        double GP_CardContactPosX { get; set; }
        double GP_CardContactPosY { get; set; }
        double GP_CardContactPosT { get; set; }
        double GP_CardContactPosZ { get; set; }
        double GP_CardPatternWidth { get; set; }
        double GP_CardPatternHeight { get; set; }
        double GP_PogoPatternWidth { get; set; }
        double GP_PogoPatternHeight { get; set; }
        double CardPodPatternWidth { get; set; }
        double CardPodPatternHeight { get; set; }

        double GP_PogoCenterDiffX { get; set; }
        double GP_PogoCenterDiffY { get; set; }
        double GP_PogoDegreeDiff { get; set; }
        FocusParameter CardFocusParam { get; set; }
        FocusParameter PogoFocusParam { get; set; }
        List<MFParameter> ModelInfos { get; set; }
    }

    public interface IGPCCObservationTempParam
    {
        List<PinCoordinate> ObservationMarkPosList { get; set; }
        List<PinCoordinate> RegisteredMarkPosList { get; set; }
        double ObservationPatternWidth { get; set; }
        double ObservationPatternHeight { get; set; }
        ushort ObservationCOAXIAL { get; set; }
        ushort ObservationOBLIQUE { get; set; }
    }

    public enum EnumCCWaitHandle
    {
        // Odd: Loader, Even: Stage
        NO_JOB = 0,
        CC_STANDBY = 1,
        LOCK_REQUEST = 2,
        LUC_LOCKED = 3,
        ALIGNMENT = 4,
        RELEASE_REQ = 6,
        RELEASED = 7,
        TRANSFFERED = 8,
        LUC_ESCAPED = 9,

        PICK_APPROACH = 50,
        CARD_PICKED = 51,
        HANDOFF_CARD_TO_ARM = 52,
        ARM_RETRACT_DONE = 53,
        PICK_REQ = 54,

        TRANSFER_REFUSE_REQ = 90,
        RETRACTING = 91,
        POD_CLEARED = 92,
        TRANSFER_CANCELED = 93,
        ABORT_REQ = 98,
        ABORTED = 99,
    }

    public interface ICardChangeStatus
    {

    }

    public enum EnumCardDockingStatus
    {
        UNDEFINED,
        DOCKING,
        DOCKED,
        DOCKEDWITHSUBVAC,
        UNDOCKING,
        UNDOCKED,
        STUCKED
    }

    public enum EnumCardChangeMode
    {
        UNDEFINED,
        AUTO,
        MANUAL
    }
}

