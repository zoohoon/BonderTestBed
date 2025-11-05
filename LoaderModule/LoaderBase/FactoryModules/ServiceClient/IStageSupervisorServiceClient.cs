namespace LoaderBase
{
    using ProberInterfaces;
    public interface IStageSupervisorServiceClient : ILoaderFactoryModule, IModule, IStageSupervisor
    {
        void UpdateProbeCardObject();
    }

    public interface IStageMoveServiceClient : ILoaderFactoryModule, IStageMove
    {

    }
}
