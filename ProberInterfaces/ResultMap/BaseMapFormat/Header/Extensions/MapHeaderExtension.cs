using ProberErrorCode;
using System;
//using ProberInterfaces;

namespace ProberInterfaces
{
    public abstract class MapHeaderExtensionBase : IFactoryModule
    {
        public abstract Type ExtensionType { get; set; }
        public abstract EventCodeEnum AssignProperties();

    }
    
}
