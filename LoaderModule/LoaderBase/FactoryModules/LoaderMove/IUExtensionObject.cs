
using ProberErrorCode;
using LoaderParameters;

namespace LoaderBase
{
    /// <summary>
    /// UExtensionObject 를 정의합니다.
    /// </summary>
    public interface IUExtensionObject
    {
        /// <summary>
        /// Extension Object의 상태를 가져옵니다.
        /// </summary>
        /// <returns>EventCodeEnum</returns>
        UExtensionStateEnum GetState();

        /// <summary>
        /// Extension Object를 초기화합니다.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="extension"></param>
        /// <returns>EventCodeEnum</returns>
        EventCodeEnum Init(Autofac.IContainer container, UExtensionBase extension);

        /// <summary>
        /// Extension Object를 Home 위치로 이동합니다.
        /// </summary>
        /// <returns>EventCodeEnum</returns>
        EventCodeEnum Homming();

        /// <summary>
        /// Extension Object를 Retract 합니다.
        /// </summary>
        /// <param name="movingType"></param>
        /// <returns>EventCodeEnum</returns>
        EventCodeEnum Retract(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL);

        /// <summary>
        /// Extension Object를 Extend 합니다.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="movingType"></param>
        /// <returns>EventCodeEnum</returns>
        EventCodeEnum MoveTo(UExtensionMoveParam param, LoaderMovingTypeEnum movingType);
    }

    /// <summary>
    /// UExtensionState 를 정의합니다.
    /// </summary>
    public enum UExtensionStateEnum
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
