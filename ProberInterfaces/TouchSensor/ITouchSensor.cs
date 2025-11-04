using ProberErrorCode;
using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.TouchSensor
{
    public interface ITouchSensorSysParam : IParam
    {

    }

    public interface ITouchSensorBaseSetupModule : IFactoryModule
    {
        EventCodeEnum TouchSensorBaseSetupSystemInit(out double diffX, out double diffY, out double diffZ);
    }

    public interface ITouchSensorTipSetupModule : IFactoryModule
    {
        EventCodeEnum TouchSensorTipSetupSystemInit(double diffX, double diffY, double diffZ);
    }

    public interface ITouchSensorPadRefSetupModule : IFactoryModule
    {
        EventCodeEnum TouchSensorPadRefSetupSystemInit();
    }
    public interface ITouchSensorCalcOffsetModule : IFactoryModule
    {
        EventCodeEnum TouchSensorCalcOffsetSystemInit();
        EventCodeEnum SetCalcOffsetCommand();
    }
}
