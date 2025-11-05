
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// IWaferTransferRemoteService 을 정의합니다.
    /// </summary>
    public interface IWaferTransferRemoteService : ILoaderFactoryModule
    {
        /// <summary>
        /// 지정된 프로세스모듈을 활성화합니다.
        /// </summary>
        /// <param name="procModule"></param>
        void Activate(IWaferTransferRemotableProcessModule procModule);

        /// <summary>
        /// 지정된 프로세스모듈을 비활성화합니다.
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Chuck Up 위치로 이동합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ChuckUpMove(int option=0);

        /// <summary>
        /// Chuck Down 위치로 이동합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ChuckDownMove(int option=0);
        EventCodeEnum Wafer_MoveLoadingPosition();
        /// <summary>
        /// 
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum NotifyTransferObject(TransferObject transferobj);

        /// <summary>
        /// 현재 사용중인 ARM의 Vacuum을 입력된 value로 출력합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WriteVacuum(bool value);

        /// <summary>
        /// 현재 사용중인 ARM의 Vacuum의 상태가 value와 일치하는지 확인합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum MonitorForVacuum(bool value);

        /// <summary>
        /// 현재 사용중인 ARM의 Vacuum상태가 value가 되기를 기다립니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WaitForVacuum(bool value);

        /// <summary>
        /// 현재 사용중인 ARM을 Retract합니다.
        /// </summary>
        /// <returns></returns>
        EventCodeEnum RetractARM();
        /// <summary>
        /// 현재 사용중인 W을 SafePos로 이동합니다.
        /// </summary>
        /// <returns></returns>

        EventCodeEnum SafePosW();
        /// <summary>
        /// 현재 사용되는 모듈들의 Status를 Unknown상태로 변경합니다.
        /// </summary>
        /// <param name="isARMUnknown">ARM의 상태가 Unknwon인지 여부</param>
        /// <param name="isChuckUnknown">Chuck의 상태가 Unknown인지 여부</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown);
        /// <summary>
        /// 현재 사용되는 모듈들의 Status를 Unknown상태로 변경합니다.
        /// </summary>
        /// <param name="isARMUnknown">ARM의 상태가 Unknwon인지 여부</param>
        /// <param name="isChuckUnknown">Chuck의 상태가 Unknown인지 여부</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum Notifyhandlerholdwafer(bool ishandlerhold);
        
        /// <summary>
        /// TransferObject 가 ThreeLeg에 Load되었음을 알립니다.
        /// </summary>
        /// <param name="loadedObject">Loader에서 관리되는 TransferObject의 정보</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum NotifyLoadedToThreeLeg(out TransferObject loadedObject);

        /// <summary>
        /// TransferObject가 ARM으로 Unload되었음을 알립니다.
        /// </summary>
        /// <param name="waferState">Stage에서 처리된 Object의 Processing State</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum NotifyUnloadedFromThreeLeg(EnumWaferState waferState, int cellIdx,bool isWaferStateChange);

        /// <summary>
        /// Pickup Move를 수행합니다. (option)
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum PickUpMove();

        /// <summary>
        /// Placedown Move를 수행합니다.(option)
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum PlaceDownMove();

        /// <summary>
        /// WaferTransfer작업이 종료되었음을 알립니다.
        /// </summary>
        /// <param name="isSucceed">성공했는지 여부</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum NotifyWaferTransferResult(bool isSucceed);

        /// <summary>
        /// ARM을 Retract하는 Self Recovery를 수행합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SelfRecoveryRetractARM();

        /// <summary>
        /// ARM에 Wafer이 존재한다고 가정하고 PreAlign으로 이송하는 Self Recovery를 수행합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SelfRecoveryTransferToPreAlign();

        double GetCurrArmUpOffset();

        bool IsLoadWafer();
        EventCodeEnum GetWaferLoadObject(out TransferObject loadobj);

        /// <summary>
        /// ARM에 Wafer이 존재한다고 가정하고 Transfer obj WaferSize를 받아옵니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        SubstrateSizeEnum GetTransferWaferSize();
        //EventCodeEnum SetResonOfError(string errorMsg);

    }
}
