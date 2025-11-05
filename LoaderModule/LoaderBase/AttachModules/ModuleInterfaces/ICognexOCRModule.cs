using LoaderParameters;
using ProberErrorCode;

namespace LoaderBase
{
    /// <summary>
    /// COGNEX OCR 모듈을 정의합니다.
    /// </summary>
    public interface ICognexOCRModule : IOCRReadable, IWaferLocatable
    {
        /// <summary>
        /// 모듈의 정의 파라미터를 가져옵니다.
        /// </summary>
        CognexOCRDefinition Definition { get; }

        /// <summary>
        /// 모듈의 디바이스 파라미터를 가져옵니다.
        /// </summary>
        CognexOCRDevice Device { get; }

        /// <summary>
        /// 모듈을 정의합니다.
        /// </summary>
        /// <param name="definition">파라미터</param>
        /// <param name="index">인덱스</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDefinition(CognexOCRDefinition definition, int index);

        /// <summary>
        /// 모듈의 디바이스를 설정합니다.
        /// </summary>
        /// <param name="device">파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDevice(CognexOCRDevice device);
    }
}
