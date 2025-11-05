
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// IWaferTransferRemoteService 을 정의합니다.
    /// </summary>
    public interface ICardTransferRemoteService : ILoaderFactoryModule
    {
        /// <summary>
        /// 지정된 프로세스모듈을 활성화합니다.
        /// </summary>
        /// <param name="procModule"></param>
        void Activate(ICardTransferRemotableProcessModule procModule);

        /// <summary>
        /// 지정된 프로세스모듈을 비활성화합니다.
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Chuck Up 위치로 이동합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum CardChangePut(out TransferObject transObj);
        EventCodeEnum SetTransferAfterCardChangePutError(out TransferObject transObj, EnumWaferState waferState);
        /// <summary>
        /// Chuck Down 위치로 이동합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum CardChangePick();

        EventCodeEnum CardChangeCarrierPick();

        EventCodeEnum OriginCarrierPut();
        EventCodeEnum OriginCardPut();
        
        EventCodeEnum CardChangeCarrierPut();

        EventCodeEnum OriginCarrierPick();
        EventCodeEnum CardTransferDone(bool isSucceed);

        EventCodeEnum Card_MoveLoadingPosition();
        string GetProbeCardID();

        EventCodeEnum GetUserCardIDInput(out string UserCardIDInput);

        EventCodeEnum NotifyCardTransferResult(bool isSucceed);

        EventCodeEnum NotifyCardDocking();

        /// <summary>
        /// ARM을 Retract하는 Self Recovery를 수행합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>

    }
}
