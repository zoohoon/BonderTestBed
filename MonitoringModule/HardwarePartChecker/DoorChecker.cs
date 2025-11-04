using System.Collections.Generic;
using ProberInterfaces;

namespace MonitoringModule.HardwarePartChecker
{
    class DoorChecker : HWPartChecker
    {
        public DoorChecker(IMonitoringManager monitoringManager, List<EnumAxisConstants> axisList)
            : base(monitoringManager, axisList)
        {
        }

        protected override EnumHWPartErrorLevel Check()
        {
            IOPortDescripter<bool> frontDoorOpen = _MonitoringManager.IOManager().IO.Inputs.DIFRONTDOOROPEN;

            if (frontDoorOpen.Value == true)
                return EnumHWPartErrorLevel.LOCK;

            return EnumHWPartErrorLevel.NONE;
        }
    }
}
