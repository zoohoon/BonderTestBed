using System;
using ProberErrorCode;
using ProberInterfaces;
using RequestInterface;

namespace RequestCore.ActionPack
{
    [Serializable]
    public abstract class Action : RequestBase, IFactoryModule
    {
        public abstract override EventCodeEnum Run();

        public Action() { }
    }
}
