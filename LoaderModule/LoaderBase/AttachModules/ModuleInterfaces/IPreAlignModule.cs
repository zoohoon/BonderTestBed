
using ProberErrorCode;
using LoaderParameters;
using ProberInterfaces.PreAligner;

namespace LoaderBase
{
    /// <summary>
    /// Wafer Type을 PreAlign할 수 있는 모듈을 정의합니다.
    /// </summary>
    public interface IPreAlignModule : IPreAlignable, IWaferOwnable
    {
        /// <summary>
        /// 모듈의 정의 파라미터를 가져옵니다.
        /// </summary>
        PreAlignDefinition Definition { get; }

        /// <summary>
        /// 모듈의 정의 파라미터를 가져옵니다.
        /// </summary>
        EnumPAStatus PAStatus { get; }

        /// <summary>
        /// 모듈의 디바이스 파라미터를 가져옵니다.
        /// </summary>
        PreAlignDevice Device { get; }

        /// <summary>
        /// 모듈을 정의합니다.
        /// </summary>
        /// <param name="definition">파라미터</param>
        /// <param name="index">인덱스</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDefinition(PreAlignDefinition definition, int index);

        /// <summary>
        /// 모듈의 디바이스를 설정합니다.
        /// </summary>
        /// <param name="device">파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDevice(PreAlignDevice device);

        /// <summary>
        /// PreAlignModule의 Vacuum을 조작합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WriteVacuum(bool value);

        /// <summary>
        /// PreAlignModule의 Vacuum상태가 value와 일치하는 지 검사합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum MonitorForVacuum(bool value);

        /// <summary>
        /// PreAlignModule의 Vacuum 상태가 value가 되기를 기다립니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WaitForVacuum(bool value);
    }
}
