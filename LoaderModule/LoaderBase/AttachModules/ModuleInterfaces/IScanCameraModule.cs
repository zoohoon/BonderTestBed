
using LoaderParameters;
using ProberErrorCode;

namespace LoaderBase
{
    /// <summary>
    /// ScanCameraModule을 정의합니다.
    /// </summary>
    public interface IScanCameraModule : ICassetteScanable
    {
        /// <summary>
        /// 모듈의 정의 파라미터를 가져옵니다.
        /// </summary>
        ScanCameraDefinition Definition { get; }

        /// <summary>
        /// 모듈의 디바이스 파라미터를 가져옵니다.
        /// </summary>
        ScanCameraDevice Device { get; }

        /// <summary>
        /// 모듈을 정의합니다.
        /// </summary>
        /// <param name="definition">파라미터</param>
        /// <param name="index">인덱스</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDefinition(ScanCameraDefinition definition, int index);

        /// <summary>
        /// 모듈의 디바이스를 설정합니다.
        /// </summary>
        /// <param name="device">파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDevice(ScanCameraDevice device);

        /// <summary>
        /// 스캔파라미터를 가져옵니다.
        /// </summary>
        /// <param name="Cassette">카세트 모듈 인스턴스</param>
        /// <returns>파라미터</returns>
        ScanCameraParam GetScanCameraParam(ICassetteModule Cassette);
    }
}
