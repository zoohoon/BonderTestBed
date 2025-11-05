using ProberErrorCode;

namespace LoaderServiceBase
{
    /// <summary>
    /// Dynamic Linking LoaderSerivce의 확장을 정의합니다.
    /// </summary>
    public interface IDirectLoaderService : ILoaderService
    {
        /// <summary>
        /// Loader를 종료합니다.
        /// </summary>
        /// <returns></returns>
        EventCodeEnum Deinitialize();

        /// <summary>
        /// 컨테이너를 설정합니다.
        /// </summary>
        /// <param name="loaderContainer"></param>
        void SetLoaderContainer(Autofac.IContainer loaderContainer);

        Autofac.IContainer GetLoaderContainer();
        /// <summary>
        /// Loader에 Stage의 정보를 설정합니다.
        /// </summary>
        /// <param name="stageContainer">Stage 컨테이너</param>
        /// <param name="callback">서비스 콜백 인스턴스</param>
        void Set(Autofac.IContainer stageContainer, ILoaderServiceCallback callback);
        
    }
    
}
