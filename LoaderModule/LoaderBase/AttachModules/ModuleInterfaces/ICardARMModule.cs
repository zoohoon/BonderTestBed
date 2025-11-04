using LoaderParameters;
using ProberErrorCode;

namespace LoaderBase
{
    /// <summary>
    /// ARM을 정의합니다.
    /// </summary>
    public interface ICardARMModule : ICardOwnable, IAttachedModule
    {
        /// <summary>
        /// 모듈 정의 파라미터를 가져옵니다.
        /// </summary>
        CardARMDefinition Definition { get; }

        /// <summary>
        /// 디바이스 파라미터를 가져옵니다.
        /// </summary>
        CardARMDevice Device { get; }

        /// <summary>
        /// 모듈을 정의합니다.
        /// </summary>
        /// <param name="definition">파라미터</param>
        /// <param name="index">모듈 인덱스</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDefinition(CardARMDefinition definition, int index);

        /// <summary>
        /// 모듈 디바이스를 설정합니다.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        EventCodeEnum SetDevice(CardARMDevice device);

        /// <summary>
        /// ARM의 Vacuum을 조작합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WriteVacuum(bool value);

        /// <summary>
        /// ARM의 Vacuum 상태가 value와 일치하는 지 확인합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum MonitorForVacuum(bool value);

        /// <summary>
        /// ARM의 Vacuum 상태가 value가 되기를 기다립니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WaitForVacuum(bool value);

        EventCodeEnum AllocateCarrier();
        /// <summary>
        /// ARM의 Vacuum 상태가 value 인지 확인합니다..
        /// </summary>
        /// <param name="v">v</param>
        /// <returns></returns>
        EventCodeEnum MonitorForCARDExist(bool v);
    }
}
