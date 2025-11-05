using System.Collections.Generic;

namespace ProberInterfaces.Communication.WCF
{
    using ProberInterfaces.Proxies;
    public interface IClientFactory
    {
        Dictionary<IProberProxy, object> Factories { get; }
        IProberProxy GetClient<IProxyProxy>();
    }
}
