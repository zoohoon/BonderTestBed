using System.Collections.Generic;

namespace MonitoringModule.HardwarePartChecker
{
    using ProberInterfaces;
    class PowerChecker : HWPartChecker
    {
        public PowerChecker(IMonitoringManager monitoringManager, List<EnumAxisConstants> axisList)
            : base(monitoringManager, axisList)
        {
        }

        protected override EnumHWPartErrorLevel Check()
        {
            return EnumHWPartErrorLevel.NONE;
        }
    }
}
