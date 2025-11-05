
using Autofac;
using ProberErrorCode;
using LoaderBase;
using ProberInterfaces;

namespace LoaderCore
{
    internal abstract class AttachedModuleBase : IAttachedModule, IModule
    {
        public override string ToString()
        {
            return $"{ID}";
        }

        public abstract bool Initialized { get; set; }
        public abstract ModuleTypeEnum ModuleType { get; }

        public ModuleID ID { get; protected set; }

        public Autofac.IContainer Container { get; private set; }

        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();

        public void SetContainer(Autofac.IContainer container)
        {
            this.Container = container;
        }

        public abstract EventCodeEnum InitModule();

        public abstract void DeInitModule();

    }
}
