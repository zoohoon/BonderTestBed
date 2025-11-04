using ProberErrorCode;
using LoaderParameters;
using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// FixedTrayModue을 정의합니다.
    /// </summary>
    public interface IFixedTrayModule : IWaferOwnable, IWaferSupplyModule
    {
        /// <summary>
        /// 모듈의 정의 파라미터를 가져옵니다.
        /// </summary>
        FixedTrayDefinition Definition { get; }

        /// <summary>
        /// 모듈의 디바이스 파라미터를 가져옵니다.
        /// </summary>
        FixedTrayDevice Device { get; }

        /// <summary>
        /// 모듈의 디바이스 파라미터를 가져옵니다.
        /// </summary>
        bool CanUseBuffer { get; set; }

        /// <summary>
        /// 모듈을 정의합니다.
        /// </summary>
        /// <param name="definition">파라미터</param>
        /// <param name="index">인덱스</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDefinition(FixedTrayDefinition definition, int index);

        /// <summary>
        /// 모듈의 디바이스를 설정합니다.
        /// </summary>
        /// <param name="device">파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDevice(FixedTrayDevice device);

        /// <summary>
        /// Wafer를 소유 하고 있는 상태가 value와 일치 하는 지 확인합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum MonitorForSubstrate(bool value);

        /// <summary>
        /// FixedTray에 접근하는 파라미터를 가져옵니다.
        /// </summary>
        /// <param name="type">타입</param>
        /// <param name="size">사이즈</param>
        /// <returns>파라미터</returns>
        FixedTrayAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size);
    }
}
