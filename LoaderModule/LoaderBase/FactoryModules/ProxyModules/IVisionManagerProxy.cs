using System.Collections.Generic;
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// IVisionManagerProxy 를 정의합니다.
    /// </summary>
    public interface IVisionManagerProxy : ILoaderFactoryModule, IModule, IFactoryModule , IVisionManager
    {
        /// <summary>
        /// StartGrab
        /// </summary>
        /// <param name="camType"></param>
        /// <returns></returns>
        EventCodeEnum StartGrab(EnumProberCam camType);

        /// <summary>
        /// StopGrab
        /// </summary>
        /// <param name="camType"></param>
        /// <returns></returns>
        new EventCodeEnum StopGrab(EnumProberCam camType);

        /// <summary>
        /// Grab된 이미지 사이즈를 가져옵니다.
        /// </summary>
        /// <param name="camType"></param>
        /// <returns></returns>
        System.Windows.Size GetGrabSize(EnumProberCam camType);

        /// <summary>
        /// WaitGrab
        /// </summary>
        /// <param name="camType"></param>
        /// <returns></returns>
        ImageBuffer WaitGrab(EnumProberCam camType);

        /// <summary>
        /// SingleGrab
        /// </summary>
        /// <param name="camType"></param>
        /// <returns></returns>
        ImageBuffer SingleGrab(EnumProberCam camType);

        /// <summary>
        /// FindPreAlignCenteringEdge
        /// </summary>
        /// <param name="ib"></param>
        /// <param name="saveDump"></param>
        /// <returns></returns>
        new EdgeProcResult FindPreAlignCenteringEdge(ImageBuffer ib, bool saveDump = false);

        /// <summary>
        /// CassetteScanProcessing
        /// </summary>
        /// <param name="ib"></param>
        /// <param name="slotParams"></param>
        /// <param name="saveDump"></param>
        /// <returns></returns>
        new CassetteScanSlotResult CassetteScanProcessing(ImageBuffer ib, CassetteScanSlotParam slotParams, bool saveDump = false);

        /// <summary>
        /// ReadOCRProcessing
        /// </summary>
        /// <param name="ocrImage"></param>
        /// <param name="ocrParams"></param>
        /// <param name="saveDump"></param>
        /// <returns></returns>
        new ReadOCRResult ReadOCRProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_get_path, bool saveDump = false);

        /// <summary>
        /// OcrCalibrateFontProcessing
        /// </summary>
        /// <param name="ocrImage"></param>
        /// <param name="ocrParams"></param>
        /// <param name="saveDump"></param>
        /// <returns></returns>
        new ReadOCRResult OcrCalibrateFontProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_output_path, bool saveDump = false);

        new void LoadImageFromFileToGrabber(string filepath, EnumProberCam camtype);
        new void LoadImageFromFloder(string folderpath, EnumProberCam camtype);
        new bool ConfirmDigitizerEmulMode(EnumProberCam camtype);
        EventCodeEnum AddEdgePosBuffer(ImageBuffer img, double x, double y);
        new ICamera GetCam(EnumProberCam camtype);
        new void SetDisplayChannel(ICamera cam, IDisplayPort port);
        new List<ICamera> GetCameras();
    }
    
}
