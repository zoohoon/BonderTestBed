using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// ILightProxy 를 정의합니다.
    /// </summary>
    public interface ILightProxy : ILoaderFactoryModule, IModule
    {
        /// <summary>
        /// 라이트를 설정합니다.
        /// </summary>
        /// <param name="lightChannel"></param>
        /// <param name="intensity"></param>
        void SetLight(int lightChannel, ushort intensity);
    }
    
}
