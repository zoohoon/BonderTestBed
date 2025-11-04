
using ProberInterfaces.Vision;

namespace ProberInterfaces
{
    public delegate void SwitchCamera(EnumProberCam cam);
    public interface IDigitizer
    {
        void Dispose();
        void ServiceDispose();

        IGrabber GrabberService { get; set; }
        ICamera CurCamera { get; set; }
        ImageBuffer.ImageReadyDelegate ImageReady { get; set; }
        string DigitizerName { get; set; }

        int GrabTimeOut { get; set; }
        int InitDigitizer();
        int InitDigitizer(int milSystem, int camchn = 0, string path = null);
        int InitDigitizer(int milSystem, string camUserName, string path);
        void DeInitDigitize();
        void DeInitDigitizer();
        void PassageGrab();
        void BlockGrabEvent();
        EnumGrabberRaft GetGrabberRaft();
        void Setparameter(string modulename, string path);
        void Setparameters(string modulename, string[] paths);
    }
}
