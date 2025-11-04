using System.ServiceModel;

using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using LoaderParameters;
using ProberInterfaces.LoaderController;
using ProberInterfaces.Param;
using System.Collections.Generic;
using System;
using ProberInterfaces.Monitoring;
using ProberInterfaces.CardChange;

namespace LoaderServiceBase
{
    /// <summary>
    /// LoaderSerivce의 Callback을 정의합니다.
    /// </summary>
    public interface ILoaderServiceCallback
    {



        /// <summary>
        /// Loader의 파라미터가 변경되었음을 알립니다.
        /// </summary>
        /// <param name="systemParam">시스템 파라미터</param>
        /// <param name="deviceParam">디바이스 파라미터</param>
        /// <returns>ErrorCode</returns>
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum OnLoaderParameterChanged(LoaderSystemParameter systemParam, LoaderDeviceParameter deviceParam);

        /// <summary>
        /// Loader의 정보가 변경되었음을 알립니다.
        /// </summary>
        /// <param name="info">로더정보</param>
        /// <returns>ErrorCode</returns>
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum OnLoaderInfoChanged(LoaderInfo info);


        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum CSTInfoChanged(LoaderInfo info);


        [OperationContract]
        EventCodeEnum WaferIDChanged(int slotNum, string ID);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum WaferHolderChanged(int slotNum, string holder);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum WaferStateChanged(int slotNum, EnumSubsStatus waferStatus, EnumWaferState waferState);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum WaferSwapChanged(int originSlotNum, int changeSlotNum, bool isInit);

        #region => Vision 
        /// <summary>
        /// [UI] Loader에 현재 활성화된 카메라를 보여줍니다.
        /// </summary>
        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void UI_ShowLoaderCam();

        /// <summary>
        /// [UI] Loader에 현재 활성화된 카메라를 숨깁니다.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void UI_HideLoaderCam();
        #endregion

        /// <summary>
        /// Stage에서 관리되는 Chuck의 Status를 가져옵니다(For Sync)
        /// </summary>
        /// <param name="id">Chuck ID</param>
        /// <returns>WaferStatus</returns>
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EnumSubsStatus GetChuckWaferStatus();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool IsHandlerholdWafer();
        [OperationContract]
        EventCodeEnum ClearHandlerStatus();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EnumSubsStatus UpdateCardStatus(out EnumWaferState cardState);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EnumWaferType GetWaferType();


        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        LoaderDeviceParameter GetLoaderDeviceParameter();

        #region => Foup 
        /// <summary>
        /// FOUP 의 정보를 가져옵니다.
        /// </summary>
        /// <param name="cassetteNumber">카세트 번호</param>
        /// <returns>FOUP 정보</returns>
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        FoupModuleInfo FOUP_GetFoupModuleInfo(int cassetteNumber);

        /// <summary>
        /// FOUP의 WaferSensor가 value와 일치하는지 확인합니다.
        /// </summary>
        /// <param name="cassetteNumber">카세트 번호</param>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum FOUP_MonitorForWaferOutSensor(int cassetteNumber, bool value);

        /// <summary>
        /// FOUP Cover Up을 수행합니다.
        /// </summary>
        /// <param name="cassetteNumber">카세트 번호</param>
        /// <returns>ErrorCode</returns>
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum FOUP_FoupCoverUp(int cassetteNumber);

        /// <summary>
        /// FOUP Cover Down을 수행합니다.
        /// </summary>
        /// <param name="cassetteNumber">카세트 번호</param>
        /// <returns>ErrorCode</returns>
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum FOUP_FoupCoverDown(int cassetteNumber);





        /// <summary>
        /// FOUP Tilt Down을 수행합니다.
        /// </summary>
        /// <param name="cassetteNumber">카세트 번호</param>
        /// <returns>ErrorCode</returns>
        //[OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum FOUP_FoupTiltDown(int cassetteNumber);


        /// <summary>
        /// FOUP Tilt Up을 수행합니다.
        /// </summary>
        /// <param name="cassetteNumber">카세트 번호</param>
        /// <returns>ErrorCode</returns>
        //[OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum FOUP_FoupTiltUp(int cassetteNumber);
        ILoaderControllerParam LoaderConnectParam { get; }
        #endregion
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        ModuleID GetChuckID();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        ModuleID GetOriginHolder();
        bool FoupTiltIgoreFlag { get; set; }

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool GetMachineInitDoneState();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        LoaderMap RequestJob(LoaderInfo loaderInfo, out bool isExchange, out bool isNeedWafer, out bool isTempReady, out string cstHashCodeOfRequestLot, bool canloadwafer = true);
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void UpdateIsNeedLotEnd(LoaderInfo loaderInfo);
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool IsCanPerformLotStart();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool LotOPStart(int foupnumber, bool iscellstart = false, string lotID = "", string cstHashCode = "");
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool IsLotEndReady();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool LotOPPause(bool isabort = false);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool LotOPResume();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        string GetProbeCardID();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        string GetWaferID();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        int GetSlotIndex();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool LotOPEnd(int foupnumber = -1);
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum SetAbort(bool isAbort,bool isForced=false);
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void SetLotOut(bool islotout);
     
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool CardAbort();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        ModuleStateEnum GetLotState();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        TransferObject GetDeviceInfo();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool IsServiceAvailable();

        [OperationContract(IsOneWay = true)]
        void DisConnect();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void SetActiveLotInfo(int foupNumber, string lotId, string cstHashCode, string carrierid);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void SetLotStarted(int foupNumber, string lotId, string cstHashCode);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void SetCassetteHashCode(int foupNumber, string lotId, string cstHashCode);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        StageLotData GetStageInfo();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool GetTesterAvailableData();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum SetStageMode(GPCellModeEnum mode);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void SetCellModeChanging();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void ResetCellModeChanging();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void SetStreamingMode(StreamingModeEnum mode);
        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void SetForcedDone(EnumModuleForcedState flag);
        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void SetForcedDoneSpecificModule(EnumModuleForcedState flag, ModuleEnum moduleEnum);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        ModuleEnum[] GetForcedDoneModules();

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void CellProbingResume();
        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void SetStilProbingZUp(bool flag = true);
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EnumWaferState GetWaferState();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void SetWaferStateOnChuck(EnumWaferState waferstate);
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        ModuleStateEnum GetSoakingModuleState();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum WaferCancel();
      
        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void SetLotLoadingPosCheckMode(bool flag);
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool GetRunState(bool isTransfer=false);
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool GetMovingState();
        [OperationContract]
        bool IsCardUpModuleUp();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        double GetSetTemp();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        CatCoordinates GetPMShifhtValue();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        ModuleStateEnum GetSetSoakingState();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void LotCancelSoakingAbort(int stageindex);
        [OperationContract]
        EventCodeEnum ReserveErrorEnd(string ErrorMessage = "Paused by host(CELL ABORT TEST).");
        [OperationContract]
        ErrorEndStateEnum GetErrorEndState();
        [OperationContract]
        void SetRecipeDownloadEnable(bool flag);
        [OperationContract]
        bool GetReservePause();
        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void CancelCellReservePause();
        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void CancelLot(int foupnumber, bool iscellend, string lotID = "", string cstHashCode = "");
        [OperationContract]
        EventCodeEnum CanWaferUnloadRecovery(ref bool canrecovery,ref ModuleStateEnum wafertransferstate);
        [OperationContract]
        EventCodeEnum NotifyLotEnd(int foupNumber, string lotID);
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        string GetPauseReason();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool GetErrorEndFlag();
        
        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void SetErrorEndFalg(bool flag);
        //EventCodeEnum CanWaferUnloadRecovery(ref bool canrecovery, ref ModuleStateEnum wafertransferstate);
        //[OperationContract]
        //EventCodeEnum NotifyLotEnd(int foupNumber, string lotID);

        //[OperationContract(IsOneWay = true)]
        //void NotifyLotEnd(int foupNumber, string lotID);


        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum GetAngleInfo(out double notchAngle, out double slotAngle, out double ocrAngle);
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum GetNotchTypeInfo(out WaferNotchTypeEnum notchType);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void SetCardStatus(bool isExist, string id, bool isDocked = false);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum StageMoveLockState(ReasonOfStageMoveLock reason);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum StageMoveUnLockState(ReasonOfStageMoveLock reason);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        List<ReasonOfStageMoveLock> GetReasonofLockFromClient();

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void SetStopBeforeProbingFlag (bool flag, int stageidx = 0);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void SetStopAfterProbingFlag(bool flag, int stageidx = 0);
        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void SetOnceStopBeforeProbingFlag(bool flag, int stageidx = 0);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void SetOnceStopAfterProbingFlag(bool flag, int stageidx = 0);
        [OperationContract]
        EventCodeEnum DoPinAlign();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        StageLockMode GetStageLock();

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void ResetAssignLotInfo(int foupnumber, string lotid, string cstHashCode);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        ProberInterfaces.CardChange.EnumCardChangeType GetCardChangeType();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void ReadStatusSoakingChillingTime(ref long _ChillingTimeMax, ref long _ChillingTime, ref SoakingStateEnum CurStatusSoaking_State);
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum GetCardIDValidateResult(string CardID, out string Msg);
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void IsAlignDoing(ref bool pinAlignDoing, ref bool waferAlignDoing);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool IsEnablePolishWaferSoaking();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool IsNeedLotEnd();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool IsLotAbort();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool GetLotOutState();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool GetShowStatusSoakingSettingPageToggleValue();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void Check_N_ClearStatusSoaking();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool CanRunningLot();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum ChangeWaferStatus(EnumSubsStatus status, out bool iswaferhold, out string errormsg);
        [OperationContract]
        TransferObject GetTransferObjectToSlotInfo();
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        byte[] GetMonitoringBehaviorFromClient();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum ManualRecoveryToStage(int behaviorIndex);
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        bool CheckCurrentAssignLotInfo(string lotID, string cstHashCode);
        [OperationContract]
        List<StageLotInfo> GetStageLotInfos();

        /// <summary>
        /// StageLotInfo 의 AssignState 중 Processing 인 상태가 있는 지 확인 하는 함수 
        /// AssignState 이 Processing 상태라는 건 할당 받은 LOT 중 의 Wafer 가 한번이라도 Load 된 적이 있고, LOT 가 할당 해제 되지 않은 상태.
        /// true : 있다, false : 없다.
        /// </summary>
        [OperationContract]
        bool IsHasProcessingLotAssignState();

        /// <summary>
        /// CC를 진행하기 위해서 현재 SV를 변경해야하는 지 판단하기 위한 함수
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool NeedToSetCCActivatableTemp();
        /// <summary>
        /// CardChange 시 동작 시 온도 조건을 판단하고, 
        /// </summary>        
        /// <returns>NONE : CC 동작 가능한 온도</returns>
        [OperationContract]
        EventCodeEnum CardChangeIsConditionSatisfied(bool needToSetTempToken);

        /// <summary>
        /// 이전 온도로 되돌릴 수 있는 상태인 지 확인하는 함수.
        /// </summary>
        [OperationContract]
        EventCodeEnum IsAvailableToSetOtherThanCCActiveTemp();

        /// <summary>
        /// 이전 온도로 되돌릴 수 있는 상태인 지 확인하고 이전 온도로 되돌린다. 
        /// </summary>
        [OperationContract]
        EventCodeEnum RecoveryCCBeforeTemp();

        /// <summary>
        /// CC ActiveTemp로 설정한다.
        /// </summary>
        [OperationContract]
        EventCodeEnum SetCCActiveTemp();

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void AbortCardChange();

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        EventCodeEnum GetCardPodState();
    }



}
