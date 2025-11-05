

namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.Vision;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public delegate void ImageReadyEventHandler(ImageBuffer args);
    public interface IGrabber : IFactoryModule
    {
        bool WaitOne(int millisecondsTimeout);

        //AutoResetEvent AreUpdateEvent { get; set; }
        ImageBuffer.ImageReadyDelegate ImageGrabbed { get; set; }
        ImageBuffer CurImageBuffer { get; set; }
        byte[] ArrImageBuff { get; }
        bool bContinousGrab { get; set; }
        EnumGrabberMode GrabMode { get; set; }
        ImageBuffer SingleGrab();
        ImageBuffer GetCurGrabImage();

        void InitGrabber(int milSystem, int digitizer, int camchn);
        void SettingGrabber(EnumGrabberRaft grabberType, ICameraParameter camparam);
        void SetCaller(object caller);
        //void SetCallerFocusingAssembly(object assembly, Type declaringtype = null);
        void Dispose();

        void ContinuousGrab();
        void StopContinuousGrab();
        void DigitizerHalt();

        EventCodeEnum LoadUserImageFiles(List<ImageBuffer> imgs);
        EventCodeEnum clearUserimageFiles();
        Task<ImageBuffer> WaitSingleShot();
        void StopWaitGrab();

        void SetParameter(string modulename, string path);
        void SetParameters(string modulename, string[] paths);
    }
}
