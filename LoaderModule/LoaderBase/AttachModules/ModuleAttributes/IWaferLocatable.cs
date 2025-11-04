
using ProberInterfaces;
using ProberErrorCode;
using LoaderParameters.Data;

namespace LoaderBase
{
    /// <summary>
    /// TransferObject가 위치 할 수 있는 모듈을 정의합니다.
    /// </summary>
    public interface IWaferLocatable : IAttachedModule
    {
        
    }

    /// <summary>
    /// TrnasferObject를 소유 할 수 있는 모듈을 정의합니다.
    /// </summary>

    public delegate EventCodeEnum UpdateHolderDelegate(ModuleID index,WaferHolder holder);

    public interface IWaferOwnable : IWaferLocatable
    {
        UpdateHolderDelegate HolderStatusChanged { get; set; }
        /// <summary>
        /// TrnasferObject를 관리하는 개체를 가져옵니다.
        /// </summary>
        WaferHolder Holder { get; }

        /// <summary>
        /// 현재 설정된 상태를 기준으로 WaferStatus를 검사합니다.<br/>
        /// (UNDEFINED 가 아닌 지정된 상태에서 변경이 발생 하면 UNKNOWN으로 설정됩니다.)
        /// </summary>
        //EventCodeEnum RecoveryWaferStatus();
        EventCodeEnum RecoveryWaferStatus(bool forcedAllocate = false);

        /// <summary>
        /// 현재 설정된 상태를 기준으로 WaferStatus를 검사합니다.<br/>
        /// (UNDEFINED 가 아닌 지정된 상태에서 변경이 발생 하면 UNKNOWN으로 설정됩니다.)
        /// </summary>
        //EventCodeEnum RecoveryWaferStatus();
        EventCodeEnum IsWaferonmodule(out bool Result);

        /// <summary>
        /// 현재 설정된 상태가 유효한 지 WaferStatus를 검사합니다.<br/>
        /// (유효하지 않으면 UNKNOWN으로 설정됩니다.)
        /// </summary>
        void ValidateWaferStatus();

        ReservationInfo ReservationInfo { get;}

        bool Enable { get; set; }
    }

    public interface IWaferTypeOwnable: IWaferOwnable
    {
        EnumWaferType WaferType { get; set; }
    }
    /// <summary>
    /// Wafer를 신규로 할당이 가능한 모듈임을 정의합니다.
    /// </summary>
    public interface IWaferSupplyModule : IWaferOwnable
    {
        /// <summary>
        /// Wafer가 할당 될 때 필요한 디바이스 정보를 가져옵니다.
        /// </summary>
        /// <returns>device info</returns>
        TransferObject GetSourceDeviceInfo();
    }

    public interface IBufferAble : IWaferOwnable
    {
        bool CanUseBuffer { get; set; }
    }
}
