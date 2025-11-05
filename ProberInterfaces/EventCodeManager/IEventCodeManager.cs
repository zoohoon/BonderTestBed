using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.EventCodeManager
{
    public interface IEventCodeManager : IFactoryModule, IModule, IHasSysParameterizable
    {
        IParam EventCodeSystemParam { get; }

        bool IsCriticalError(EventCodeEnum eventcode);
    }
}