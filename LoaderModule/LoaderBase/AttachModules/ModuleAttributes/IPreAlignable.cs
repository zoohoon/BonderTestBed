
using LoaderParameters;
using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// PreAlign을 수행하는 모듈임을 정의합니다.
    /// </summary>
    public interface IPreAlignable : IWaferOwnable
    {
        /// <summary>
        /// 입력된 오브젝트를 PreAlign할 수 있는지 여부를 가져옵니다.
        /// </summary>
        /// <param name="transferObject">오브젝트 인스턴스</param>
        /// <returns>PreAlign 가능하면 true, 그렇지 않으면 false</returns>
        bool CanPreAlignable(TransferObject transferObject);

        /// <summary>
        /// 오브젝트를 목적지에 이송하기 위해 Notch를 설정해야 하는 지 여부를 가져옵니다.
        /// </summary>
        /// <param name="transferObject">이송 오브젝트 인스턴스</param>
        /// <param name="destination">목적 모듈</param>
        /// <returns>설정해야 하면 true, 그렇지 않으면 false</returns>
        bool IsNeedRatateOffsetNotchAngle(TransferObject transferObject, IHasLoadNotchAngle destination);

        /// <summary>
        /// 오브젝트를 목적지에 이송하기 위해 필요한 회전 각도를 계산합니다.
        /// </summary>
        /// <param name="transferObject">이송 오브젝트 인스턴스</param>
        /// <param name="destination">목적 모듈</param>
        /// <returns>필요한 회전 각도</returns>
        Degree CalcRatateOffsetNotchAngle(TransferObject transferObject, IHasLoadNotchAngle destination);

        /// <summary>
        /// 오브젝트를 목적지에 이송하기 위해 Notch를 설정해야 하는 지 여부를 가져옵니다.
        /// </summary>
        /// <param name="transferObject">이송 오브젝트 인스턴스</param>
        /// <param name="destination">목적 모듈</param>
        /// <returns>설정해야 하면 true, 그렇지 않으면 false</returns>
        bool IsNeedRatateOffsetNotchAngle(TransferObject transferObject, IOCRReadable destination);

        /// <summary>
        /// 오브젝트를 목적지에 이송하기 위해 필요한 회전 각도를 계산합니다.
        /// </summary>
        /// <param name="transferObject">이송 오브젝트 인스턴스</param>
        /// <param name="destination">목적 모듈</param>
        /// <returns>필요한 회전 각도</returns>
        Degree CalcRatateOffsetNotchAngle(TransferObject transferObject, IOCRReadable destination);

        /// <summary>
        /// 모듈에 접근하기 위한 파라미터를 가져옵니다.
        /// </summary>
        /// <param name="type">타입</param>
        /// <param name="size">사이즈</param>
        /// <returns>파라미터</returns>
        PreAlignAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size);

        /// <summary>
        /// PreAlign을 수행 하기 위한 파라미터를 가져옵니다.
        /// </summary>
        /// <param name="type">타입</param>
        /// <param name="size">사이즈</param>
        /// <returns>파라미터</returns>
        PreAlignProcessingParam GetProcessingParam(SubstrateTypeEnum type, SubstrateSizeEnum size);
    }

}
