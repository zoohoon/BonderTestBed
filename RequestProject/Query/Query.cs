using ProberErrorCode;
using ProberInterfaces;
using RequestInterface;
using System;

namespace RequestCore.QueryPack
{
    [Serializable]
    public abstract class Query : RequestBase, IFactoryModule
    {
        public abstract override EventCodeEnum Run();
    }
}
