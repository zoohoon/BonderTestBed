using System;
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// IMotionManagerProxy 를 정의합니다.
    /// </summary>
    public interface IMotionManagerProxy : ILoaderFactoryModule, IModule
    {
        /// <summary>
        /// LoaderAxes 를 가져옵니다.
        /// </summary>
        ProbeAxes LoaderAxes { get; }

        /// <summary>
        /// ProbeAxisObject 를 가져옵니다.
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        ProbeAxisObject GetAxis(EnumAxisConstants axis);

        /// <summary>
        /// ProbeAxisObject가 Home에 위치해 있는 지 여부를 가져옵니다.
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        bool GetIOHome(ProbeAxisObject axis);

        /// <summary>
        /// LoaderAxes 를 저장합니다.
        /// </summary>
        /// <returns></returns>
        EventCodeEnum SaveLoaderAxesObject();

        /// <summary>
        /// NotchFindMove 를 실행합니다.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        EventCodeEnum NotchFindMove(EnumAxisConstants axis, EnumMotorDedicatedIn input);

        /// <summary>
        /// Motion 을 초기화합니다.
        /// </summary>
        /// <returns></returns>
        new EventCodeEnum InitModule();

        /// <summary>
        /// Home 위치로 이동합니다.
        /// </summary>
        /// <returns></returns>
        EventCodeEnum HomeMove();

        /// <summary>
        /// 지정된 절대 위치로 이동하고 모션이 완료되기를 기다립니다.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        EventCodeEnum AbsMove(EnumAxisConstants axis, double pos);

        /// <summary>
        /// 지정된 절대 위치로 이동하고 모션이 완료되기를 기다립니다.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="pos"></param>
        /// <param name="vel"></param>
        /// <param name="acc"></param>
        /// <returns></returns>
        EventCodeEnum AbsMove(EnumAxisConstants axis, double pos, double vel, double acc);

        /// <summary>
        /// 지정된 절대 위치로 이동합니다. 모션이 완료되기를 기다리지 않습니다.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="pos"></param>
        /// <param name="vel"></param>
        /// <param name="acc"></param>
        /// <returns></returns>
        EventCodeEnum AbsMoveAsync(EnumAxisConstants axis, double pos, double vel, double acc);

        /// <summary>
        /// 지정된 상대 위치로 이동하고 모션이 완료되기를 기다립니다.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        EventCodeEnum RelMove(EnumAxisConstants axis, double pos);

        /// <summary>
        /// 지정된 상대 위치로 이동하고 모션이 완료되기를 기다립니다.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="pos"></param>
        /// <param name="vel"></param>
        /// <param name="acc"></param>
        /// <returns></returns>
        EventCodeEnum RelMove(EnumAxisConstants axis, double pos, double vel, double acc);

        /// <summary>
        /// 지정된 상대 위치로 이동하고 모션이 완료되기를 기다리지 않습니다.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        EventCodeEnum RelMoveAsync(EnumAxisConstants axis, double pos);

        /// <summary>
        /// 지정된 상대 위치로 이동하고 모션이 완료되기를 기다리지 않습니다.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="pos"></param>
        /// <param name="vel"></param>
        /// <param name="acc"></param>
        /// <returns></returns>
        EventCodeEnum RelMoveAsync(EnumAxisConstants axis, double pos, double vel, double acc);

        /// <summary>
        /// 모션이 완료되기를 기다립니다.
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        EventCodeEnum WaitForAxisMotionDone(EnumAxisConstants axis);

        /// <summary>
        /// 모션이 완료되기를 기다립니다.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="GetSourceLevel"></param>
        /// <param name="resumeLevel"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        EventCodeEnum WaitForAxisMotionDone(EnumAxisConstants axis, Func<bool> GetSourceLevel, bool resumeLevel, long timeout = 0);

        /// <summary>
        /// 지정된 축의 캡쳐를 시작합니다.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="MotorDedicatedIn"></param>
        /// <returns></returns>
        EventCodeEnum StartScanPosCapt(EnumAxisConstants axis, EnumMotorDedicatedIn MotorDedicatedIn);

        /// <summary>
        /// 지정된 축의 캡쳐를 종료합니다.
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        EventCodeEnum StopScanPosCapt(EnumAxisConstants axis);
    }
}
