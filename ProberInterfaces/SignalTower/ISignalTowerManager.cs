using ProberErrorCode;
using ProberInterfaces.SignalTower;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.SignalTower
{
    public interface ISignalTowerManager : IFactoryModule, ILoaderFactoryModule, IModule
    {
        void UpdateEventObjList(object eventParam, bool enable);
    }
}
