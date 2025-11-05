using System.Collections.Generic;

namespace MonitoringModule.HardwarePartChecker
{
    using ProberInterfaces;
    class AirChecker : HWPartChecker
    {
        public AirChecker(IMonitoringManager monitoringManager, List<EnumAxisConstants> axisList)
            : base(monitoringManager, axisList)
        {
        }

        protected override EnumHWPartErrorLevel Check()
        {
            IOPortDescripter<bool> mainAir = _MonitoringManager.IOManager().IO.Inputs.DIMAINAIR;
            if (mainAir.Value == false)
                return EnumHWPartErrorLevel.LOCK;

            //_MonitoringManager.IOManager().IO.Outputs.DOCAMERA_MUX_0

            return EnumHWPartErrorLevel.NONE;
        }
    }
}
