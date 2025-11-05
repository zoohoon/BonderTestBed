
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;

namespace LoaderBase
{
    /// <summary>
    /// ChuckModule을 정의합니다.
    /// </summary>
    public interface IChuckModule : IWaferOwnable, IHasLoadNotchAngle
    {
        /// <summary>
        /// 모듈의 정의 파라미터를 가져옵니다.
        /// </summary>
        ChuckDefinition Definition { get; }

        /// <summary>
        /// 모듈의 디바이스 파라미터를 가져옵니다.
        /// </summary>
        ChuckDevice Device { get; }

        /// <summary>
        /// 모듈을 정의합니다.
        /// </summary>
        /// <param name="definition">파라미터</param>
        /// <param name="index">인덱스</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDefinition(ChuckDefinition definition, int index);

        /// <summary>
        /// 모듈의 디바이스를 설정합니다.
        /// </summary>
        /// <param name="device">파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDevice(ChuckDevice device);

        /// <summary>
        /// Chuck에 접근하기 위한 파라미터를 가져옵니다.
        /// </summary>
        /// <param name="type">타입</param>
        /// <param name="size">사이즈</param>
        /// <returns>파라미터</returns>
        ChuckAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size);
    }

}
