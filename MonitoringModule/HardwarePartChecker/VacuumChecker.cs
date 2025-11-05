using System.Collections.Generic;

namespace MonitoringModule.HardwarePartChecker
{
    using ProberInterfaces;
    class VacuumChecker : HWPartChecker
    {
        public VacuumChecker(IMonitoringManager monitoringManager, List<EnumAxisConstants> axisList) 
            : base(monitoringManager, axisList)
        {
        }

        protected override EnumHWPartErrorLevel Check()
        {
            IOPortDescripter<bool> waferOnChuck = _MonitoringManager.IOManager().IO.Inputs.DIWAFERONCHUCK;

            if (waferOnChuck.Value == false)
                return EnumHWPartErrorLevel.LOCK;

            return EnumHWPartErrorLevel.NONE;
        }
    }
}
