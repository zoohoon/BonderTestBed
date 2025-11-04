
using LoaderParameters;
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// 카세트의 SlotModue을 정의합니다.
    /// </summary>
    public interface ISlotModule : IWaferSupplyModule, IHasLoadNotchAngle,IFactoryModule
    {
        /// <summary>
        /// 모듈의 정의 파라미터를 가져옵니다.
        /// </summary>
        SlotDefinition Definition { get; }

        /// <summary>
        /// 모듈의 디바이스 파라미터를 가져옵니다.
        /// </summary>
        SlotDevice Device { get; }

        /// <summary>
        /// 모듈을 정의합니다.
        /// </summary>
        /// <param name="definition">파라미터</param>
        /// <param name="index">인덱스</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDefinition(SlotDefinition definition, int index);

        /// <summary>
        /// 모듈의 디바이스를 설정합니다.
        /// </summary>
        /// <param name="device">파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDevice(SlotDevice device);

        /// <summary>
        /// 모듈을 소유하고 있는 카세트 모듈을 가져옵니다.
        /// </summary>
        ICassetteModule Cassette { get; }

        /// <summary>
        /// 모듈을 소유하고 있는 카세트에서 몇번째 인지 가져옵니다.
        /// </summary>
        int LocalSlotNumber { get; }

        /// <summary>
        /// 모듈의 소유자를 설정합니다.
        /// </summary>
        /// <param name="cassette">카세트 모듈 인스턴스</param>
        /// <param name="localSlotNumber">카세트 모듈 내부 슬롯 번호</param>
        void SetCassette(ICassetteModule cassette, int localSlotNumber);

    }
}
