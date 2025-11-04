
using ProberInterfaces;
using ProberErrorCode;

namespace LoaderBase
{
    public interface ICardLocatable : IAttachedModule
    {
    }
    public interface ICardOwnable : ICardLocatable
    {
        /// <summary>
        /// TrnasferObject를 관리하는 개체를 가져옵니다.
        /// </summary>
        CardHolder Holder { get; }

        /// <summary>
        /// 현재 설정된 상태를 기준으로 CardStatus를 검사합니다.<br/>
        /// (UNDEFINED 가 아닌 지정된 상태에서 변경이 발생 하면 UNKNOWN으로 설정됩니다.)
        /// </summary>
        EventCodeEnum RecoveryCardStatus();

        /// <summary>
        /// 현재 설정된 상태가 유효한 지 CardStatus를 검사합니다.<br/>
        /// (유효하지 않으면 UNKNOWN으로 설정됩니다.)
        /// </summary>
        void ValidateCardStatus();
        bool Enable { get; set; }
    }

    /// <summary>
    /// Wafer를 신규로 할당이 가능한 모듈임을 정의합니다.
    /// </summary>
    public interface ICardSupplyModule : ICardOwnable
    {
        /// <summary>
        /// Wafer가 할당 될 때 필요한 디바이스 정보를 가져옵니다.
        /// </summary>
        /// <returns>device info</returns>
        TransferObject GetSourceDeviceInfo();
    }
}
