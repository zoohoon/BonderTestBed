using ProberErrorCode;
using LoaderParameters;
using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// InspectionTrayModule을 정의합니다.
    /// </summary>
    public interface IInspectionTrayModule : IWaferSupplyModule
    {
        /// <summary>
        /// 모듈의 정의 파라미터를 가져옵니다.
        /// </summary>
        InspectionTrayDefinition Definition { get; }

        /// <summary>
        /// 모듈의 디바이스 파라미터를 가져옵니다.
        /// </summary>
        InspectionTrayDevice Device { get; }

        /// <summary>
        /// 모듈을 정의합니다.
        /// </summary>
        /// <param name="definition">파라미터</param>
        /// <param name="index">인덱스</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDefinition(InspectionTrayDefinition definition, int index);

        /// <summary>
        /// 모듈의 디바이스를 설정합니다.
        /// </summary>
        /// <param name="device">파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDevice(InspectionTrayDevice device);

        /// <summary>
        /// InspectionTray가 열려 있는 지 여부를 가져옵니다.
        /// </summary>
        /// <param name="value">열려있는 지 여부</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ReadOpened(out bool value);

        /// <summary>
        /// InspectionTray의 개폐 상태가 value와 일치하는 지 검사합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum MonitorForOpened(bool value);

        /// <summary>
        /// InspectionTray의 웨이퍼 검출 상태가 value와 일치하는 지 검사합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum MonitorForSubstrate(bool value);

        /// <summary>
        /// InspectionTray에 접근할 수 있는 파라미터를 가져옵니다.
        /// </summary>
        /// <param name="type">타입</param>
        /// <param name="size">사이즈</param>
        /// <returns>파라미터</returns>
        InspectionTrayAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size);
    }
}
