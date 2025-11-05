
namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.Vision;
    public interface IMil
    {
        EventCodeEnum InitMilObjects(EnumGrabberRaft type);
        int GetMilSystem(EnumGrabberRaft grabberRaft);
        int GetMilSystem(EnumVisionProcRaft visionProcRaft);
        void Dispose();
    }
}
