using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.Device;
    using ProberInterfaces.Proxies;
    using System;
    using System.ServiceModel;

    [ServiceKnownType(typeof(PROBECARD_TYPE))]
    public interface IStageSupervisorProxy : IProberProxy
    {
        bool IsOpened();
        CommunicationState GetCommunicationState();
        void StageLotPause();
        IWaferObject GetWaferObject(IDeviceObject[,] preDies = null);
        void InitService(int stageAbsIndex = 0);
        Task InitLoaderClient();
        void SetDynamicMode(DynamicModeEnum dynamicModeEnum);
        EnumSubsStatus GetWaferStatus();
        EnumWaferType GetWaferType();
        IStageSupervisor GetChannel();
        void SetDevice(byte[] device, string devicename, string lotid, string lotCstHashCode, bool loaddev = true, int foupnumber = -1, bool showprogress = true, bool manualDownload = false);
        void SetNeedChangeParaemterInDeviceInfo(NeedChangeParameterInDevice needChangeParameter);
        void BindDispService(string uri);
        void BindEventEelegateService(string uri);
        void BindDataGatewayService(string uri);
        Task DoWaferAlign();
        void DoLot();
        Task DoSystemInit(bool showMessageDialogFlag = true);
        //Task DoPinAign();

        EventCodeEnum CheckManualZUpState();
        EventCodeEnum DoPinPadMatch_FirstSequence();

        EventCodeEnum DO_ManualZUP();

        EventCodeEnum DO_ManualZDown();

        EventCodeEnum DO_ManualSoaking();
        EventCodeEnum DO_ManualWaferAlign();
        EventCodeEnum DO_ManualPinAlign();
        int GetWaferObjHashCode();
        //void DoPMI();
        Task ChangeDeviceFuncUsingName(string devName);
        void SetAcceptUpdateDisp(bool flag);
        //Task<List<Guid>> GetRecoveryPnPNodeStepsFromModuleState();

        void SetWaitCancelDialogHashCode(string hashCode);
        bool InitServiceHost();
        string GetDeviceName();
        void SetEMG(EventCodeEnum errorCode);
        void SetStageClickMoveTarget(double xpos, double ypos);
        void MoveStageToTargetPos(object enableClickToMove);

        Task StageClickMove(object enableClickToMove);

        void SetWaferMapCam(EnumProberCam cam);
        void SetVacuum(bool ison);
        byte[] GetDevice();
        byte[] GetLog(string date);


        byte[] GetLogFromFilename(List<string> debug,List<string> temp,List<string> pin,List<string> pmi, List<string> lot);
        byte[] GetPinImageFromStage(List<string> pinImage);
       byte[] GetLogFromFileName(EnumUploadLogType logtype, List<string> data);
        byte[] GetRMdataFromFileName(string filename);
        byte[] GetODTPdataFromFileName(string filename);
        List<string> GetStageDebugDates();
        List<string> GetStageTempDates();
        List<string> GetStagePinDates();
        List<string> GetStagePMIDates();
        List<string> GetStageLotDates();
        EventCodeEnum WaferHighViewIndexCoordMove(long mix, long miy);
        EventCodeEnum WaferLowViewIndexCoordMove(long mix, long miy);

        IProbeCard GetProbeCardConcreteObject();
        byte[] GetProbeCardObject();

        byte[] GetMarkObject();
        //byte[] GetDIEs();
        SubstrateInfoNonSerialized GetSubstrateInfoNonSerialized();
        Element<AlignStateEnum> GetAlignState(AlignTypeEnum AlignType);
        Task<IDeviceObject[,]> GetConcreteDIEs();
        WaferObjectInfoNonSerialized GetWaferObjectInfoNonSerialize();
        void WaferIndexUpdated(long xindex, long yindex);
        EventCodeEnum CheckPinPadParameterValidity();
        EventCodeEnum GetPinDataFromPads();
        PROBECARD_TYPE GetProbeCardType();
        int DutPadInfosCount();
        EventCodeEnum InitGemConnectService();
        EventCodeEnum DeInitGemConnectService();
        EventCodeEnum NotifySystemErrorToConnectedCells(EnumLoaderEmergency emgtype);

        string[] LoadStageEventLog(string fileFath);
        string GetLotErrorMessage();
        (GPCellModeEnum, StreamingModeEnum) GetStageMode();

        EventCodeEnum HandlerVacOnOff(bool val, int stageindex = -1);
        bool CheckUsingHandler(int stageindex = -1);
        void StopBeforeProbingCmd(bool stopBeforeProbing);
        void StopAfterProbingCmd(bool stopAfterProbing);
        void OnceStopBeforeProbingCmd(bool onceStopBeforeProbing);
        void OnceStopAfterProbingCmd(bool onceStopAfterProbing);

        void ChangeLotMode(LotModeEnum mode);
        void SetLotModeByForcedLotMode();
        bool IsForcedDoneMode();
        (DispFlipEnum disphorflip, DispFlipEnum dispverflip) GetDisplayFlipInfo();
        (bool reverseX, bool reverseY) GetReverseMoveInfo();
        bool IsMovingState();
        void LoaderConnected();

        void SetErrorCodeAlarm(EventCodeEnum errorcode);

        CellInitModeEnum GetStageInitState();
    }
}
