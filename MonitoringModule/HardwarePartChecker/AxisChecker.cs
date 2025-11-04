using System.Collections.Generic;

namespace MonitoringModule.HardwarePartChecker
{
    using ProberInterfaces;
    class AxisChecker : HWPartChecker
    {
        public AxisChecker(IMonitoringManager monitoringManager, List<EnumAxisConstants> axisList)
            : base(monitoringManager, axisList)
        {
        }

        protected override EnumHWPartErrorLevel Check()
        {
            return EnumHWPartErrorLevel.NONE;
        }
    }
}
