using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.Temperature.EnvMonitoring
{
    public interface IEnvMonitoringManager : IFactoryModule, IModule, IHasSysParameterizable
    {
        List<IEnvMonitoringHub> EnvMonitoringHubs { get; set; }
        SensorParameters SensorParams { get; set; }        
        int GetSensorMaxCount();        
    }
}
