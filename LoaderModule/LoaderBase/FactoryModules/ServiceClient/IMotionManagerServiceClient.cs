namespace LoaderBase.FactoryModules.ServiceClient
{
    using ProberInterfaces;
    public interface IMotionManagerServiceClient: ILoaderFactoryModule, IModule, IMotionManager
    {
        void ThreadPuase();

        void ThreadResume();

        IMotionAxisProxy MotionAxisProxy { get; set; }
    }
}
