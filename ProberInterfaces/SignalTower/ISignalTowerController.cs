using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.SignalTower
{
    public interface ISignalTowerController : IFactoryModule
    {
        void SetPulse();
        void UpdateEventObjList(object parameter, bool enable);     
    }
}
