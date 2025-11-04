using ProberErrorCode;

namespace ProberInterfaces
{
    public interface ITouchSensorObject
    {
        IParam TouchSensorParam_IParam { get; set; }
        bool TouchSensorRegistered { get; set; }
        bool TouchSensorBaseRegistered { get; set; }
        bool TouchSensorPadBaseRegistered { get; set; }
        bool TouchSensorOffsetRegistered { get; set; }
        bool IsReadyToTouchSensor();
        EventCodeEnum SaveSysParameter();
        EventCodeEnum LoadSysParameter();
    }
}
