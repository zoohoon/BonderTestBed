
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// IOCRRemoteService 를 정의합니다.
    /// </summary>
    public interface IOCRRemoteService : ILoaderFactoryModule
    {
        /// <summary>
        /// 지정된 프로세스모듈을 활성화합니다.
        /// </summary>
        /// <param name="procModule"></param>
        void Activate(IOCRRemotableProcessModule procModule);

        /// <summary>
        /// 등록된 프로세스 모듈을 비활성화합니다.
        /// </summary>
        void Deactivate();

        /// <summary>
        /// OCR Image를 가져옵니다.
        /// </summary>
        /// <param name="imgBuf">image buffer</param>
        /// <param name="w">image width</param>
        /// <param name="h">image height</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum GetOCRImage(out byte[] imgBuf, out int w, out int h);

        /// <summary>
        /// OCR의 Light를 조정합니다.
        /// </summary>
        /// <param name="channel">light channel</param>
        /// <param name="intensity">light intensity</param>
        /// <returns></returns>
        EventCodeEnum ChangeLight(int channel, ushort intensity);

        /// <summary>
        /// 사용자가 입력한 OCR을 적용합니다.
        /// </summary>
        /// <param name="inputOCR">사용자가 입력한 문자열</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetOcrID(string inputOCR);

        /// <summary>
        /// OCR Remote를 종료합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum OCRRemoteEnd();
        EventCodeEnum GetOCRState();
        EventCodeEnum OCRRetry();
        EventCodeEnum OCRFail();

        EventCodeEnum OCRAbort();
    }
}
