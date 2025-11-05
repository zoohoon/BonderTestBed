using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// TransferObject를 적재 시 Notch Angle이 필요함을 정의합니다.
    /// </summary>
    public interface IHasLoadNotchAngle : IWaferOwnable
    {
        /// <summary>
        /// 설정된 Loading Angle을 가져옵니다.
        /// </summary>
        /// <returns>Degree</returns>
        Degree GetLoadingAngle();

        /// <summary>
        /// 설정된 W axis의 Angle을 가져옵니다.
        /// </summary>
        /// <param name="type">타입</param>
        /// <param name="size">사이즈</param>
        /// <returns>Degree</returns>
        Degree GetWaxisAngle(SubstrateTypeEnum type, SubstrateSizeEnum size);
    }
}
