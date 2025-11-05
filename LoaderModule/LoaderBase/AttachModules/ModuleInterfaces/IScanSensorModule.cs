
using ProberErrorCode;
using LoaderParameters;

namespace LoaderBase
{
    /// <summary>
    /// ScanSensorModule을 정의합니다.
    /// </summary>
    public interface IScanSensorModule : ICassetteScanable
    {
        /// <summary>
        /// 모듈의 정의 파라미터를 가져옵니다.
        /// </summary>
        ScanSensorDefinition Definition { get; }

        /// <summary>
        /// 모듈의 디바이스 파라미터를 가져옵니다.
        /// </summary>
        ScanSensorDevice Device { get; }
        
        /// <summary>
        /// 모듈을 정의합니다.
        /// </summary>
        /// <param name="definition">파라미터</param>
        /// <param name="index">인덱스</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDefinition(ScanSensorDefinition definition, int index);

        /// <summary>
        /// 모듈의 디바이스를 설정합니다.
        /// </summary>
        /// <param name="device">파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDevice(ScanSensorDevice device);

        /// <summary>
        /// ScanSensorModule의 상태를 가져옵니다.
        /// </summary>
        /// <returns>ScanSensorModule state</returns>
        ScanSensorStateEnum GetState();

        /// <summary>
        /// ScanSensorModule을 Retract합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum Retract();

        /// <summary>
        /// ScanSensorModule을 Extend합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum Extend();

        /// <summary>
        /// 입력된 카세트 모듈의 스캔 파라미터를 가져옵니다.
        /// </summary>
        /// <param name="Cassette">카세트 모듈 인스턴스</param>
        /// <returns>ErrorCode</returns>
        ScanSensorParam GetScanParam(ICassetteModule Cassette);
    }

    /// <summary>
    /// 스캔 센서 상태를 정의합니다.
    /// </summary>
    public enum ScanSensorStateEnum
    {
        /// <summary>
        /// UNKNOWN
        /// </summary>
        UNKNOWN,
        /// <summary>
        /// RETRACTED
        /// </summary>
        RETRACTED,
        /// <summary>
        /// EXTENDED
        /// </summary>
        EXTENDED,
    }

}
