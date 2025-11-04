using System.Collections.Generic;

using ProberInterfaces;
using ProberErrorCode;
using ProberInterfaces.Foup;
using System.ServiceModel;
using LoaderParameters;
using LoaderParameters.Data;
using ProberInterfaces.CardChange;
using System.Threading.Tasks;
using ProberInterfaces.Param;
using ProberInterfaces.Monitoring;
using ProberInterfaces.LoaderController;
using LogModule;
using ProberInterfaces.Enum;

namespace LoaderServiceBase
{
    /// <summary>
    /// LoaderService 를 정의합니다.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(ILoaderServiceCallback), SessionMode = SessionMode.Required)]
    //[ServiceContract]
    public interface ILoaderService
    {
        #region => Init Methods
        [OperationContract]
        /// <summary>
        /// Loader에 연결합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum Connect();

        [OperationContract]
        /// <summary>
        /// Loader에 연결을 해제합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum Disconnect();
        [OperationContract]
        bool IsServiceAvailable();
        /// <summary>
        /// Loader를 초기화합니다.
        /// </summary>
        /// <param name="rootParamPath">Root parameter path</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum Initialize(string rootParamPath);
        #endregion

        #region => Loader Work Methods
        [OperationContract]
        /// <summary>
        /// Loader 정보를 가져옵니다.
        /// </summary>
        /// <returns>LoaderInfo</returns>
        LoaderInfo GetLoaderInfo();

        [OperationContract]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cassetteNumber">카세트 번호</param>
        /// <returns></returns>
        bool SetNoReadScanState(int cassetteNumber);

        [OperationContract]
        /// <summary>
        /// Loader가 현재 Foup에 접근한 상태인지 여부를 가져옵니다.
        /// </summary>
        /// <param name="cassetteNumber">카세트 번호</param>
        /// <returns>접근상태면 true, 그렇지 않으면 false</returns>
        bool IsFoupAccessed(int cassetteNumber);
        [OperationContract]
        /// <summary>
        /// Loader System을 초기화합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum LoaderSystemInit();
        [OperationContract]
        /// <summary>
        /// Loader 명령을 요청합니다.
        /// </summary>
        /// <param name="dstMap">명령 맵</param>
        /// <returns>응답결과</returns>
        ResponseResult SetRequest(LoaderMap dstMap);
        [OperationContract]
        /// <summary>
        /// Loader 모듈을 Awake합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum AwakeProcessModule();
        [OperationContract]
        /// <summary>
        /// Loader에 요청된 명령맵을 취소합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum AbortRequest();
        [OperationContract]
        /// <summary>
        /// Loader에 요청된 명령을 초기화합니다. 
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ClearRequestData();
        [OperationContract]
        /// <summary>
        /// Loader가 Error상태일때 SelfRecovery명령을 요청합니다.
        /// </summary>
        void SelfRecovery();
        [OperationContract]
        void SetPause();
        [OperationContract]
        void SetResume();
        #endregion

        #region => Motion Methods
        [OperationContract]
        /// <summary>
        /// JogRelMove
        /// </summary>
        /// <param name="axis">axis</param>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum MOTION_JogRelMove(EnumAxisConstants axis, double value);
        [OperationContract]
        /// <summary>
        /// JogAbsMove
        /// </summary>
        /// <param name="axis">axis</param>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum MOTION_JogAbsMove(EnumAxisConstants axis, double value);
        #endregion

        #region => Setting Param Methods
        [OperationContract]
        /// <summary>
        /// Loader에 설정된 시스템 파라미터를 가져옵니다.
        /// </summary>
        /// <returns>시스템 파라미터</returns>
        LoaderSystemParameter GetSystemParam();
        //[OperationContract]
        ///// <summary>
        ///// Loader에 설정된 디바이스 파라미터를 가져옵니다.
        ///// </summary>
        ///// <returns>디바이스 파라미터</returns>
        //LoaderDeviceParameter GetDeviceParam();
        [OperationContract]
        /// <summary>
        /// Loader의 시스템 파라미터를 갱신합니다.
        /// </summary>
        /// <param name="systemParam">변경된 파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum UpdateSystemParam(LoaderSystemParameter systemParam);
        [OperationContract]
        EventCodeEnum SaveSystemParam(LoaderSystemParameter systemParam);
        [OperationContract]
        /// <summary>
        /// Loader의 디바이스 파라미터를 갱신합니다.
        /// </summary>
        /// <param name="deviceParam">변경된 파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum UpdateDeviceParam(LoaderDeviceParameter deviceParam);
        //[OperationContract]
        //EventCodeEnum SaveDeviceParam(LoaderDeviceParameter deviceParam);
        [OperationContract]
        EventCodeEnum MoveToModuleForSetup(ModuleTypeEnum module, bool skipuaxis, int slot, int index);
        [OperationContract]

        EventCodeEnum GetWaferLoadObject(out TransferObject loadobj);

         [OperationContract]
        EventCodeEnum RetractAll();
        #endregion

        #region => Notify Foup State
        [OperationContract]
        /// <summary>
        /// Loader에 Foup의 상태가 변경되었음을 알립니다.
        /// </summary>
        /// <param name="foupInfo">FOUP Info</param>
        void FOUP_RaiseFoupStateChanged(FoupModuleInfo foupInfo);
        [OperationContract]
        /// <summary>
        /// Loader에 Wafer Out Sensor가 감지되었음을 알립니다.
        /// </summary>
        /// <param name="cassetteNumber">감지된 카세트 번호</param>
        void FOUP_RaiseWaferOutDetected(int cassetteNumber);
        #endregion

        #region => WaferTransfer Remote Methods
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] ChuckUp 위치로 이동합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_ChuckUpMove(int option=0);

        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] ChuckUp 위치로 이동합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_Wafer_MoveLoadingPosition();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] ChuckDown 위치로 이동합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_ChuckDownMove();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] 현재 사용중인 ARM의 Vacuum을 입력된 value로 출력합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_WriteARMVacuum(bool value);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] 현재 사용중인 ARM의 Vacuum의 상태가 value와 일치하는지 확인합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_MonitorForARMVacuum(bool value);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] 현재 사용중인 ARM의 Vacuum상태가 value가 되기를 기다립니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_WaitForARMVacuum(bool value);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] 현재 사용중인 ARM을 Retract합니다.
        /// </summary>
        /// <returns></returns>
        EventCodeEnum WTR_RetractARM();
        [OperationContract]
        EventCodeEnum WTR_SafePosW();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] 현재 사용되는 모듈들의 Status를 Unknown상태로 변경합니다.
        /// </summary>
        /// <param name="isARMUnknown">ARM의 상태가 Unknwon인지 여부</param>
        /// <param name="isChuckUnknown">Chuck의 상태가 Unknown인지 여부</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] TransferObject 가 ThreeLeg에 Load되었음을 알립니다.
        /// </summary>
        /// <param name="loadedObject">Loader에서 관리되는 TransferObject의 정보</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_NotifyLoadedToThreeLeg(out TransferObject loadedObject);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] TransferObject가 ARM으로 Unload되었음을 알립니다.
        /// </summary>
        /// <param name="waferState">Stage에서 처리된 Object의 Processing State</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_NotifyUnloadedFromThreeLeg(EnumWaferState waferState, int cellIdx, bool isWaferStateChange);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] Pickup Move를 수행합니다. (option)
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_PickUpMove();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] Placedown Move를 수행합니다.(option)
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_PlaceDownMove();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] WaferTransfer작업이 종료되었음을 알립니다.
        /// </summary>
        /// <param name="isSucceed">성공했는지 여부</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_NotifyWaferTransferResult(bool isSucceed);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] ARM을 Retract하는 Self Recovery를 수행합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_SelfRecoveryRetractARM();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] ARM에 Wafer이 존재한다고 가정하고 PreAlign으로 이송하는 Self Recovery를 수행합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_SelfRecoveryTransferToPreAlign();

        //[OperationContract]
        ///// <summary>
        ///// [WaferTransferRemote] 
        ///// </summary>
        ///// <returns>ErrorCode</returns>
        //EventCodeEnum WTR_NotifyPolishWaferAngle(double CurAngle);

        [OperationContract]
        bool WTR_IsLoadWafer();
        #endregion

        #region => OCR Failed Remote Methods
        [OperationContract]
        /// <summary>
        /// [OCRFailedRemote] OCR Image를 가져옵니다.
        /// </summary>
        /// <param name="imgBuf">image buffer</param>
        /// <param name="w">image width</param>
        /// <param name="h">image height</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum OFR_GetOCRImage(out byte[] imgBuf, out int w, out int h);

        [OperationContract]
        /// <summary>
        /// [OCRFailedRemote] OCR의 Light를 조정합니다.
        /// </summary>
        /// <param name="channel">light channel</param>
        /// <param name="intensity">light intensity</param>
        /// <returns></returns>
        EventCodeEnum OFR_ChangeLight(int channel, ushort intensity);

        [OperationContract]
        /// <summary>
        /// [OCRFailedRemote] 사용자가 입력한 OCR을 적용합니다.
        /// </summary>
        /// <param name="inputOCR">사용자가 입력한 문자열</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum OFR_SetOcrID(string inputOCR);
        [OperationContract]
        /// <summary>
        /// [OCRFailedRemote] OCR Remote를 종료합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum OFR_OCRRemoteEnd();
        [OperationContract]
        EventCodeEnum OFR_GetOCRState();
        [OperationContract]
        EventCodeEnum OFR_OCRRetry();
        [OperationContract]
        EventCodeEnum OFR_OCRFail();

        [OperationContract]

        EventCodeEnum OFR_OCRAbort();

        #endregion

        #region => Recovery Methods
        [OperationContract]
        /// <summary>
        /// [Recovery] Loader Motion을 초기화합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum RECOVERY_MotionInit();
        [OperationContract]
        /// <summary>
        /// [Recovery] Loader의 Holder들의 WaferStatus를 다시 설정합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum RECOVERY_ResetWaferLocation();
        #endregion

        #region => SetTestCenteingflag
        [OperationContract]
        void SetTestCenteringFlag(bool testflag);
        #endregion
        [OperationContract]
        double GetArmUpOffset();
        [OperationContract]
        string GetResonOfError();
        [OperationContract]
        void SetLoaderTestOption(LoaderTestOption option);
        LoaderServiceTypeEnum GetServiceType();
        void SetContainer(Autofac.IContainer container);
        void SetCallBack(ILoaderServiceCallback loadercontroller);
        string GetProbeCardID();
        EventCodeEnum UpdateLoaderSystem(int foupIndex);
        EventCodeEnum UpdateCassetteSystem(SubstrateSizeEnum WaferSize, int foupIndex);
        EventCodeEnum ResultMapUpload(int v);
        EventCodeEnum ResultMapDownload(int stageindex, string filename);
        
    }
    [ServiceContract( CallbackContract = typeof(ILoaderServiceCallback), SessionMode = SessionMode.Required)]
    public interface IGPLoaderService
    {
        [OperationContract]
        /// <summary>
        /// Loader에 연결합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum Connect(string chuckID);

        [OperationContract]
        /// <summary>
        /// Loader에 연결을 해제합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum Disconnect(int chuckID = -1);

        [OperationContract]
        /// <summary>
        /// Loader 정보를 가져옵니다.
        /// </summary>
        /// <returns>LoaderInfo</returns>
        LoaderInfo GetLoaderInfo();
      
        [OperationContract]
        /// <summary>
        /// Loader 명령을 요청합니다.
        /// </summary>
        /// <param name="dstMap">명령 맵</param>
        /// <returns>응답결과</returns>
        ResponseResult SetRequest(LoaderMap dstMap);
        [OperationContract]
        /// <summary>
        /// Loader 모듈을 Awake합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum AwakeProcessModule();
        [OperationContract]
        /// <summary>
        /// Loader에 요청된 명령맵을 취소합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum AbortRequest();
        [OperationContract]
        /// <summary>
        /// Loader에 요청된 명령을 초기화합니다. 
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ClearRequestData();
      

        #region => Setting Param Methods
        [OperationContract]
        /// <summary>
        /// Loader에 설정된 시스템 파라미터를 가져옵니다.
        /// </summary>
        /// <returns>시스템 파라미터</returns>
        LoaderSystemParameter GetSystemParam();
        [OperationContract]
        /// <summary>
        /// Loader에 설정된 디바이스 파라미터를 가져옵니다.
        /// </summary>
        /// <returns>디바이스 파라미터</returns>
        LoaderDeviceParameter GetDeviceParam();
        
        [OperationContract]
        EventCodeEnum GetWaferLoadObject(out TransferObject loadobj);

        [OperationContract]
        EventCodeEnum RetractAll();
        #endregion


        #region => WaferTransfer Remote Methods
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] ChuckUp 위치로 이동합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_ChuckUpMove(int option=0);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] ChuckUp 위치로 이동합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_Wafer_MoveLoadingPosition();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] ChuckDown 위치로 이동합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_ChuckDownMove(int option=0);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] 현재 사용중인 ARM의 Vacuum을 입력된 value로 출력합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_WriteARMVacuum(bool value);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] 현재 사용중인 ARM의 Vacuum의 상태가 value와 일치하는지 확인합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_MonitorForARMVacuum(bool value);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] 현재 사용중인 ARM의 Vacuum상태가 value가 되기를 기다립니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_WaitForARMVacuum(bool value);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] 현재 사용중인 ARM을 Retract합니다.
        /// </summary>
        /// <returns></returns>
        EventCodeEnum WTR_RetractARM();
        [OperationContract]
        EventCodeEnum WTR_SafePosW();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] 현재 사용되는 모듈들의 Status를 Unknown상태로 변경합니다.
        /// </summary>
        /// <param name="isARMUnknown">ARM의 상태가 Unknwon인지 여부</param>
        /// <param name="isChuckUnknown">Chuck의 상태가 Unknown인지 여부</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] 현재 사용되는 모듈들의 Status를 Unknown상태로 변경합니다.
        /// </summary>
        EventCodeEnum WTR_Notifyhandlerholdwafer(bool ishandlerhold);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] TransferObject 가 ThreeLeg에 Load되었음을 알립니다.
        /// </summary>
        /// <param name="loadedObject">Loader에서 관리되는 TransferObject의 정보</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_NotifyLoadedToThreeLeg(out TransferObject loadedObject);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] TransferObject가 ARM으로 Unload되었음을 알립니다.
        /// </summary>
        /// <param name="waferState">Stage에서 처리된 Object의 Processing State</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_NotifyUnloadedFromThreeLeg(EnumWaferState waferState, int cellIdx,bool isWaferStateChange= true);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] Pickup Move를 수행합니다. (option)
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_PickUpMove();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] Placedown Move를 수행합니다.(option)
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_PlaceDownMove();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] WaferTransfer작업이 종료되었음을 알립니다.
        /// </summary>
        /// <param name="isSucceed">성공했는지 여부</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_NotifyWaferTransferResult(bool isSucceed);
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] ARM을 Retract하는 Self Recovery를 수행합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_SelfRecoveryRetractARM();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] ARM에 Wafer이 존재한다고 가정하고 PreAlign으로 이송하는 Self Recovery를 수행합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_SelfRecoveryTransferToPreAlign();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] ARM에 Wafer이 존재한다고 가정하고 Transfer obj WaferSize를 받아옵니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        SubstrateSizeEnum WTR_GetTransferWaferSize();
        [OperationContract]
        EventCodeEnum CTR_NotifyCardTransferResult(bool isSucceed);

        [OperationContract]
        EventCodeEnum CTR_NotifyCardDocking();

        [OperationContract(IsOneWay = true)]
        void IsServiceAvailable();


        [OperationContract]
        EventCodeEnum SetReasonOfError(string title, string errorMsg, string recoveryBeh = "", int cellIdx = 0);

        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] TransferObject 가 ThreeLeg에 Load되었음을 알립니다.
        /// </summary>
        /// <param name="loadedObject">Loader에서 관리되는 TransferObject의 정보</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum CTR_CardChangePick();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] TransferObject 가 ThreeLeg에 Load되었음을 알립니다.
        /// </summary>
        /// <param name="loadedObject">Loader에서 관리되는 TransferObject의 정보</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum CTR_CardChangeCarrierPick();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] TransferObject 가 ThreeLeg에 Load되었음을 알립니다.
        /// </summary>
        /// <param name="loadedObject">Loader에서 관리되는 TransferObject의 정보</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum CTR_CardChangeCarrierPut();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] TransferObject 가 ThreeLeg에 Load되었음을 알립니다.
        /// </summary>
        /// <param name="loadedObject">Loader에서 관리되는 TransferObject의 정보</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum CTR_OriginCarrierPut();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] TransferObject 가 Origin에 Load되었음을 알립니다.
        /// </summary>
        /// <param name="loadedObject">Loader에서 관리되는 TransferObject의 정보</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum CTR_OriginCardPut();
        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] TransferObject 가 ThreeLeg에 Load되었음을 알립니다.
        /// </summary>
        /// <param name="loadedObject">Loader에서 관리되는 TransferObject의 정보</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum CTR_OriginCarrierPick();
        [OperationContract]
        EventCodeEnum CTR_CardTransferDone(bool isSucceed);


        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] TransferObject 가 ThreeLeg에 Load되었음을 알립니다.
        /// </summary>
        /// <param name="loadedObject">Loader에서 관리되는 TransferObject의 정보</param>
        /// <returns>ErrorCode</returns
        EventCodeEnum CTR_CardChangePut(out TransferObject transObj);

        [OperationContract]
        EventCodeEnum CTR_SetTransferAfterCardChangePutError(out TransferObject transObj, EnumWaferState waferState);

        [OperationContract]
        EventCodeEnum CTR_Card_MoveLoadingPosition();
        #endregion

        #region => Recovery Methods
        [OperationContract]
        /// <summary>
        /// [Recovery] Loader Motion을 초기화합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum RECOVERY_MotionInit();
        [OperationContract]
        /// <summary>
        /// [Recovery] Loader의 Holder들의 WaferStatus를 다시 설정합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum RECOVERY_ResetWaferLocation();
        #endregion


        [OperationContract]
        /// <summary>
        /// [WaferTransferRemote] 
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WTR_NotifyTransferObject(TransferObject transferobj);

        LoaderServiceTypeEnum GetServiceType();
        void SetContainer(Autofac.IContainer container);

        [OperationContract]
        /// <summary>
        /// </summary>
        /// <returns>ErrorCode</returns>
        void SetStageState(int cellIdx, ModuleStateEnum StageState, bool isBuzzerOn);


        [OperationContract]
        /// <summary>
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ResponseSystemInit(EventCodeEnum errorCode);
        [OperationContract]
        EventCodeEnum ResponseCardRecovery(EventCodeEnum errorCode);
        [OperationContract]
        string CTR_GetProbeCardID();

        [OperationContract]
        EventCodeEnum CTR_UserCardIDInput(out string UserCardIDInput);
        [OperationContract]
        string CTR_GetProbeCardIDLastTwoWord();

        #region //MonitoringManager
        [OperationContract]
        EventCodeEnum NotifyStageSystemError(int cellindex);
        [OperationContract]
        EventCodeEnum NotifyClearStageSystemError(int cellindex);
        #endregion

        [OperationContract(IsOneWay = true)]
        void NotifyReasonOfError(string errmsg);

        [OperationContract(IsOneWay = true)]
        void SetTitleMessage(int cellno, string message, string foreground = "", string background = "");

        [OperationContract(IsOneWay = true)]
        void SetDeviceName(int cellno, string deviceName);
        [OperationContract(IsOneWay = true)]
        void SetDeviceLoadResult(int cellno, bool result);
        [OperationContract]
        EventCodeEnum SetCardState(int index, EnumWaferState CardState);

        [OperationContract(IsOneWay = true)]
        void UpdateSoakingInfo(SoakingInfo soakinfo);

        [OperationContract]
        void UpdateLotVerifyInfo(int foupindex, int cellindex, bool flag);
        [OperationContract]
        void UpdateDownloadMapResult(int cellindex, bool flag);

        [OperationContract(IsOneWay =true)]
        void UpdateLotModeEnum(int cellindex, LotModeEnum mode);

        [OperationContract]
        void UpdateTesterConnectedStatus(int cellindex, bool flag);

        [OperationContract(IsOneWay = true)]
        void UpdateLotDataInfo(int cellindex, StageLotDataEnum type, string val);
        
        [OperationContract(IsOneWay = true)]
        void UpdateStageMove(StageMoveInfo info);
        [OperationContract]
        Task<EventCodeEnum> PMIImageUploadLoaderToServer(int cellindex);
        [OperationContract]
        EventCodeEnum PMIImageUploadStageToLoader(int cellindex, byte[] data, string filename);
        [OperationContract]
        EventCodeEnum PINImageUploadLoaderToServer(int cellindex, byte[] images);
        [OperationContract]
        EventCodeEnum LogUpload(int cellindex, EnumUploadLogType logtype);
        [OperationContract]
        EventCodeEnum ODTPUpload(int stageindex, string filename);
        [OperationContract]
        EventCodeEnum ResultMapUpload(int stageindex, string filename);
        [OperationContract]
        EventCodeEnum ResultMapDownload(int stageindex, string filename);
        [OperationContract]
        EventCodeEnum UploadCardPatternImages(byte[] data, string filename, string devicename, string cardid);
        [OperationContract]
        List<CardImageBuffer> DownloadCardPatternImages(string devicename,int downimgcnt, string cardid);
        [OperationContract]
        EventCodeEnum UploadProbeCardInfo(ProberCardListParameter probeCard);
        [OperationContract]
        ProberCardListParameter DownloadProbeCardInfo(string cardID);
        [OperationContract]
        void SetProbingStart(int cellIdx,bool isStart);
        [OperationContract]
        void SetTransferError(int cellIdx, bool isError);

        [OperationContract(IsOneWay = true)]
        void SetActionLogMessage(string message,int idx, ModuleLogType ModuleType, StateLogType State);
        [OperationContract(IsOneWay = true)]
        void SetParamLogMessage(string message, int idx);

        [OperationContract]
        /// <summary>
        /// Loader 모듈을 Awake합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum AbortProcessModule();
        [OperationContract]
        byte[] GetBytesFoupObjects();

        [OperationContract]
        bool GetStopBeforeProbingFlag(int stageIdx);
        [OperationContract]
        bool GetStopAfterProbingFlag(int stageIdx);

        [OperationContract]
        EventCodeEnum SetRecoveryMode(int cellIdx, bool isRecovery);

        [OperationContract]
        void SetStopBeforeProbingFlag(int stageidx, bool flag);

        [OperationContract]
        void SetStopAfterProbingFlag(int stageidx, bool flag);

        [OperationContract]
        void SetOnceStopBeforeProbingFlag(int stageidx, bool flag);

        [OperationContract]
        void SetOnceStopAfterProbingFlag(int stageidx, bool flag);

        [OperationContract]
        EventCodeEnum WriteWaitHandle(short value);
        [OperationContract]
        EventCodeEnum WaitForHandle(short handle, long timeout = 60000);
        [OperationContract]
        int ReadWaitHandle();


        [OperationContract(IsOneWay = true)]
        void SetStageLock(int stageIndex, StageLockMode mode);    

        [OperationContract(IsOneWay = true)]
        void SetForcedDoneMode(int stageIndex, EnumModuleForcedState forcedDoneMode);
        
        [OperationContract]
        ModuleStateEnum GetPGVCardChangeState();

        [OperationContract]
        bool IsActiveCCAllocatedState(int stageNumber);


        [OperationContract(IsOneWay = true)]
        void SetTCW_Mode(int stageIndex, TCW_Mode mode);

        [OperationContract]
        EnumWaferType GetActiveLotWaferType(string lotid);

        [OperationContract]
        EventCodeEnum IsShutterClose(int cellIdx);

        [OperationContract(IsOneWay = true)]
        void SetMonitoringBehavior(byte[] monitoringBehaviors, int stageIdx);

        [OperationContract(IsOneWay = true)]
        void ChangeTabIndex(TabControlEnum tabEnum);

        [OperationContract]
        EventCodeEnum GetLoaderEmergency();

        [OperationContract]
        void SetTransferAbort();

        [OperationContract]
        void UpdateLogUploadList(int cellindex, EnumUploadLogType type);

        [OperationContract(IsOneWay = true)]
        void UploadRecentLogs(int cellindex);
    }

    public interface ILoaderServiceProxy: System.ServiceModel.ICommunicationObject
    {
        ILoaderService GetService();

        EventCodeEnum Connect();
    }

    public interface IGPLoaderServiceProxy : ICommunicationObject
    {
        IGPLoaderService GetService();

        EventCodeEnum Connect(string chuckID);
    }

    //[ServiceContract]
    //public interface IDataAccessLayer
    //{
    //    [OperationContract]
    //    int GetValue();
    //    [OperationContract]
    //    void SetValue(int value);
    //    string Data { get; set; }
    //    #region // LoaderService implementation
    //    #region => Init Methods
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader에 연결합니다.
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum Connect();
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader에 연결을 해제합니다.
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum Disconnect();
    //    [OperationContract]
    //    bool IsServiceAvailable();
    //    /// <summary>
    //    /// Loader를 초기화합니다.
    //    /// </summary>
    //    /// <param name="rootParamPath">Root parameter path</param>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum Initialize(string rootParamPath);
    //    #endregion

    //    #region => Loader Work Methods
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader 정보를 가져옵니다.
    //    /// </summary>
    //    /// <returns>LoaderInfo</returns>
    //    LoaderInfo GetLoaderInfo();
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader가 현재 Foup에 접근한 상태인지 여부를 가져옵니다.
    //    /// </summary>
    //    /// <param name="cassetteNumber">카세트 번호</param>
    //    /// <returns>접근상태면 true, 그렇지 않으면 false</returns>
    //    bool IsFoupAccessed(int cassetteNumber);

    //    /// <summary>
    //    /// Loader System을 초기화합니다.
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum LoaderSystemInit();
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader 명령을 요청합니다.
    //    /// </summary>
    //    /// <param name="dstMap">명령 맵</param>
    //    /// <returns>응답결과</returns>
    //    ResponseResult SetRequest(LoaderMap dstMap);
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader 모듈을 Awake합니다.
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum AwakeProcessModule();
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader에 요청된 명령맵을 취소합니다.
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum AbortRequest();
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader에 요청된 명령을 초기화합니다. 
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum ClearRequestData();
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader가 Error상태일때 SelfRecovery명령을 요청합니다.
    //    /// </summary>
    //    void SelfRecovery();
    //    [OperationContract]
    //    void SetPause();
    //    [OperationContract]
    //    void SetResume();
    //    #endregion

    //    #region => Motion Methods
    //    [OperationContract]
    //    /// <summary>
    //    /// JogRelMove
    //    /// </summary>
    //    /// <param name="axis">axis</param>
    //    /// <param name="value">value</param>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum MOTION_JogRelMove(EnumAxisConstants axis, double value);
    //    [OperationContract]
    //    /// <summary>
    //    /// JogAbsMove
    //    /// </summary>
    //    /// <param name="axis">axis</param>
    //    /// <param name="value">value</param>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum MOTION_JogAbsMove(EnumAxisConstants axis, double value);
    //    #endregion

    //    #region => Setting Param Methods
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader에 설정된 시스템 파라미터를 가져옵니다.
    //    /// </summary>
    //    /// <returns>시스템 파라미터</returns>
    //    LoaderSystemParameter GetSystemParam();
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader에 설정된 디바이스 파라미터를 가져옵니다.
    //    /// </summary>
    //    /// <returns>디바이스 파라미터</returns>
    //    LoaderDeviceParameter GetDeviceParam();
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader의 시스템 파라미터를 갱신합니다.
    //    /// </summary>
    //    /// <param name="systemParam">변경된 파라미터</param>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum UpdateSystemParam(LoaderSystemParameter systemParam);
    //    [OperationContract]
    //    EventCodeEnum SaveSystemParam(LoaderSystemParameter systemParam);
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader의 디바이스 파라미터를 갱신합니다.
    //    /// </summary>
    //    /// <param name="deviceParam">변경된 파라미터</param>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum UpdateDeviceParam(LoaderDeviceParameter deviceParam);
    //    [OperationContract]
    //    EventCodeEnum SaveDeviceParam(LoaderDeviceParameter deviceParam);
    //    [OperationContract]
    //    EventCodeEnum MoveToModuleForSetup(ModuleTypeEnum module, bool skipuaxis, int slot, int index);

    //    [OperationContract]
    //    EventCodeEnum RetractAll();
    //    #endregion

    //    #region => Notify Foup State
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader에 Foup의 상태가 변경되었음을 알립니다.
    //    /// </summary>
    //    /// <param name="foupInfo">FOUP Info</param>
    //    void FOUP_RaiseFoupStateChanged(FoupModuleInfo foupInfo);
    //    [OperationContract]
    //    /// <summary>
    //    /// Loader에 Wafer Out Sensor가 감지되었음을 알립니다.
    //    /// </summary>
    //    /// <param name="cassetteNumber">감지된 카세트 번호</param>
    //    void FOUP_RaiseWaferOutDetected(int cassetteNumber);
    //    #endregion

    //    #region => WaferTransfer Remote Methods
    //    [OperationContract]
    //    /// <summary>
    //    /// [WaferTransferRemote] ChuckUp 위치로 이동합니다.
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum WTR_ChuckUpMove();

    //    [OperationContract]
    //    /// <summary>
    //    /// [WaferTransferRemote] ChuckDown 위치로 이동합니다.
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum WTR_ChuckDownMove();
    //    [OperationContract]
    //    /// <summary>
    //    /// [WaferTransferRemote] 현재 사용중인 ARM의 Vacuum을 입력된 value로 출력합니다.
    //    /// </summary>
    //    /// <param name="value">value</param>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum WTR_WriteARMVacuum(bool value);
    //    [OperationContract]
    //    /// <summary>
    //    /// [WaferTransferRemote] 현재 사용중인 ARM의 Vacuum의 상태가 value와 일치하는지 확인합니다.
    //    /// </summary>
    //    /// <param name="value">value</param>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum WTR_MonitorForARMVacuum(bool value);
    //    [OperationContract]
    //    /// <summary>
    //    /// [WaferTransferRemote] 현재 사용중인 ARM의 Vacuum상태가 value가 되기를 기다립니다.
    //    /// </summary>
    //    /// <param name="value">value</param>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum WTR_WaitForARMVacuum(bool value);
    //    [OperationContract]
    //    /// <summary>
    //    /// [WaferTransferRemote] 현재 사용중인 ARM을 Retract합니다.
    //    /// </summary>
    //    /// <returns></returns>
    //    EventCodeEnum WTR_RetractARM();
    //    [OperationContract]
    //    EventCodeEnum WTR_SafePosW();
    //    [OperationContract]
    //    /// <summary>
    //    /// [WaferTransferRemote] 현재 사용되는 모듈들의 Status를 Unknown상태로 변경합니다.
    //    /// </summary>
    //    /// <param name="isARMUnknown">ARM의 상태가 Unknwon인지 여부</param>
    //    /// <param name="isChuckUnknown">Chuck의 상태가 Unknown인지 여부</param>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum WTR_SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown);
    //    [OperationContract]
    //    /// <summary>
    //    /// [WaferTransferRemote] TransferObject 가 ThreeLeg에 Load되었음을 알립니다.
    //    /// </summary>
    //    /// <param name="loadedObject">Loader에서 관리되는 TransferObject의 정보</param>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum WTR_NotifyLoadedToThreeLeg(out TransferObject loadedObject);
    //    [OperationContract]
    //    /// <summary>
    //    /// [WaferTransferRemote] TransferObject가 ARM으로 Unload되었음을 알립니다.
    //    /// </summary>
    //    /// <param name="waferState">Stage에서 처리된 Object의 Processing State</param>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum WTR_NotifyUnloadedFromThreeLeg(EnumWaferState waferState);
    //    [OperationContract]
    //    /// <summary>
    //    /// [WaferTransferRemote] Pickup Move를 수행합니다. (option)
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum WTR_PickUpMove();
    //    [OperationContract]
    //    /// <summary>
    //    /// [WaferTransferRemote] Placedown Move를 수행합니다.(option)
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum WTR_PlaceDownMove();
    //    [OperationContract]
    //    /// <summary>
    //    /// [WaferTransferRemote] WaferTransfer작업이 종료되었음을 알립니다.
    //    /// </summary>
    //    /// <param name="isSucceed">성공했는지 여부</param>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum WTR_NotifyWaferTransferResult(bool isSucceed);
    //    [OperationContract]
    //    /// <summary>
    //    /// [WaferTransferRemote] ARM을 Retract하는 Self Recovery를 수행합니다.
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum WTR_SelfRecoveryRetractARM();
    //    [OperationContract]
    //    /// <summary>
    //    /// [WaferTransferRemote] ARM에 Wafer이 존재한다고 가정하고 PreAlign으로 이송하는 Self Recovery를 수행합니다.
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum WTR_SelfRecoveryTransferToPreAlign();
    //    [OperationContract]
    //    bool WTR_IsLoadWafer();
    //    #endregion

    //    #region => OCR Failed Remote Methods
    //    [OperationContract]
    //    /// <summary>
    //    /// [OCRFailedRemote] OCR Image를 가져옵니다.
    //    /// </summary>
    //    /// <param name="imgBuf">image buffer</param>
    //    /// <param name="w">image width</param>
    //    /// <param name="h">image height</param>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum OFR_GetOCRImage(out byte[] imgBuf, out int w, out int h);

    //    [OperationContract]
    //    /// <summary>
    //    /// [OCRFailedRemote] OCR의 Light를 조정합니다.
    //    /// </summary>
    //    /// <param name="channel">light channel</param>
    //    /// <param name="intensity">light intensity</param>
    //    /// <returns></returns>
    //    EventCodeEnum OFR_ChangeLight(int channel, ushort intensity);

    //    [OperationContract]
    //    /// <summary>
    //    /// [OCRFailedRemote] 사용자가 입력한 OCR을 적용합니다.
    //    /// </summary>
    //    /// <param name="inputOCR">사용자가 입력한 문자열</param>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum OFR_SetOcrID(string inputOCR);
    //    [OperationContract]
    //    /// <summary>
    //    /// [OCRFailedRemote] OCR Remote를 종료합니다.
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum OFR_OCRRemoteEnd();
    //    [OperationContract]
    //    EventCodeEnum OFR_GetOCRState();
    //    [OperationContract]
    //    EventCodeEnum OFR_OCRRetry();
    //    [OperationContract]
    //    EventCodeEnum OFR_OCRFail();

    //    #endregion

    //    #region => Recovery Methods
    //    [OperationContract]
    //    /// <summary>
    //    /// [Recovery] Loader Motion을 초기화합니다.
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum RECOVERY_MotionInit();
    //    [OperationContract]
    //    /// <summary>
    //    /// [Recovery] Loader의 Holder들의 WaferStatus를 다시 설정합니다.
    //    /// </summary>
    //    /// <returns>ErrorCode</returns>
    //    EventCodeEnum RECOVERY_ResetWaferLocation();
    //    #endregion

    //    #region => SetTestCenteingflag
    //    [OperationContract]
    //    void SetTestCenteringFlag(bool testflag);
    //    #endregion
    //    [OperationContract]
    //    double GetArmUpOffset();
    //    [OperationContract]
    //    void SetLoaderTestOption(LoaderTestOption option);
    //    LoaderServiceTypeEnum GetServiceType();
    //    void SetContainer(Autofac.IContainer container);
    //    #endregion
    //}
}
