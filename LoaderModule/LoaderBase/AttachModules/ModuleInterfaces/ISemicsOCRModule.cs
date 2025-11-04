using LoaderParameters;
using ProberErrorCode;

namespace LoaderBase
{
    /// <summary>
    /// SemicsOCRModule을 정의합니다.
    /// </summary>
    public interface ISemicsOCRModule : IOCRReadable, IWaferLocatable
    {
        /// <summary>
        /// 모듈의 정의 파라미터를 가져옵니다.
        /// </summary>
        SemicsOCRDefinition Definition { get; }

        /// <summary>
        /// 모듈의 디바이스 파라미터를 가져옵니다.
        /// </summary>
        SemicsOCRDevice Device { get; }

        /// <summary>
        /// 모듈을 정의합니다.
        /// </summary>
        /// <param name="definition">파라미터</param>
        /// <param name="index">인덱스</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDefinition(SemicsOCRDefinition definition, int index);

        /// <summary>
        /// 모듈의 디바이스를 설정합니다.
        /// </summary>
        /// <param name="device">파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDevice(SemicsOCRDevice device);
    }

}
