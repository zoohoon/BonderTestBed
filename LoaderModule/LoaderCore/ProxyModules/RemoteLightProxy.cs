using Autofac;
using LoaderBase;
using ProberErrorCode;
using ProberInterfaces;
using System;

namespace LoaderCore.ProxyModules
{
    public class RemoteLightProxy : ILightProxy, IFactoryModule
    {
        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL3;
        public IContainer Container { get; set; }
        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();
        public bool Initialized { get; set; } = false;

        public void DeInitModule()
        {
            Initialized = false;
        }

        public EventCodeEnum InitModule(IContainer container)
        {
            Container = container;
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum InitModule()
        {
            Initialized = true;
            return EventCodeEnum.NONE;
        }

        public void SetLight(int lightChannel, ushort intensity)
        {
            throw new NotImplementedException();
        }
    }
}
