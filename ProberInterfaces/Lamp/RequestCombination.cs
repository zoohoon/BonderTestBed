using System;

namespace ProberInterfaces.Lamp
{
    public class RequestCombination : LampCombination
    {
        public RequestCombination()
        {

        }
        public RequestCombination(
            LampStatusEnum redLampStatus,
            LampStatusEnum yellowLampStatus,
            LampStatusEnum blueLampStatus,
            LampStatusEnum buzzerStatus,
            AlarmPriority priority = AlarmPriority.Normal,
            String id = "")
            : base(redLampStatus, yellowLampStatus, blueLampStatus, buzzerStatus, priority, id)
        {

        }
    }
}
