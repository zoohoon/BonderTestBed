using ProberErrorCode;
using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// LOADER를 구성하고 있는 기계적인 모듈로서 각 특성을 정의한다.
    /// </summary>
    public interface IAttachedModule : ILoaderSubModule
    {
        /// <summary>
        /// 모듈의 타입
        /// </summary>
        ModuleTypeEnum ModuleType { get; }

        /// <summary>
        /// 모듈의 고유 ID
        /// </summary>
        ModuleID ID { get; }

        /// <summary>
        /// 컨테이너를 설정합니다.
        /// </summary>
        /// <param name="container">컨테이너</param>
        void SetContainer(Autofac.IContainer container);

        /// <summary>
        /// 모듈을 초기화합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum InitModule();

        /// <summary>
        /// 모듈을 파괴합니다.
        /// </summary>
        void DeInitModule();
    }
    
}
