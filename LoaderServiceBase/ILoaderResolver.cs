namespace LoaderServiceBase
{
    /// <summary>
    /// LoaderResolver 를 정의합니다.
    /// </summary>
    public interface ILoaderResolver
    {
        /// <summary>
        /// Loader에 종속된 모듈을 구성합니다.
        /// </summary>
        /// <returns>컨테이너</returns>
        Autofac.IContainer ConfigureDependencies();
        Autofac.IContainer RemoteModeConfigDependencies();
    }
}
