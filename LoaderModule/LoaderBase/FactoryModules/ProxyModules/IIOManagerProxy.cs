
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// IIOManagerProxy 을 정의합니다.
    /// </summary>
    public interface IIOManagerProxy : ILoaderFactoryModule, IModule
    {
        /// <summary>
        /// IIOMappingsParameter 를 가져옵니다.
        /// </summary>
        IIOMappingsParameter IOMappings { get; }
        /// <summary>
        /// 원격지의 IOManager를 가져 옵니다.
        /// </summary>
        IIOManager IOStates { get; }

        IGPLoader Remote { get; }
        /// <summary>
        /// IOPortDescripter 를 가져옵니다.
        /// </summary>
        /// <param name="ioName"></param>
        /// <returns>ErrorCode</returns>
        IOPortDescripter<bool> GetIOPortDescripter(string ioName);

        /// <summary>
        /// IOPortDescripter의 값을 가져옵니다.
        /// </summary>
        /// <param name="iodesc"></param>
        /// <param name="value"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ReadIO(IOPortDescripter<bool> iodesc, out bool value);

        /// <summary>
        /// IOPortDescripter의 값이 입력된 value와 일치하는지 검사합니다.
        /// </summary>
        /// <param name="iodesc"></param>
        /// <param name="value"></param>
        /// <param name="maintainTime"></param>
        /// <param name="timeout"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum MonitorForIO(IOPortDescripter<bool> iodesc, bool value, long maintainTime = 500, long timeout = 1000, bool writelog = true);

        /// <summary>
        /// IOPortDescripter의 값이 value가 되기를 기다립니다.
        /// </summary>
        /// <param name="iodesc"></param>
        /// <param name="value"></param>
        /// <param name="timeout"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WaitForIO(IOPortDescripter<bool> iodesc, bool value, long timeout = 1000);

        /// <summary>
        /// IOPortDescripter를 조작합니다.
        /// </summary>
        /// <param name="iodesc"></param>
        /// <param name="value"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WriteIO(IOPortDescripter<bool> iodesc, bool value);
    }
    
}
