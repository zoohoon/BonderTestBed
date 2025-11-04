using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.EnvControl.Enum
{
    public enum SensorStatusEnum
    {
        NOTUSED = 0,
        DISCONNECT_INDICATOR = 1,
        DISCONNECT_HUB = 2,
        NORMAL = 3,
        TEMP_ALARM = 4,
        TEMP_WARN = 5,
        SMOKE_DETECTED = 6,
        INIT = 7,
        ERROR = 8
    }
}
