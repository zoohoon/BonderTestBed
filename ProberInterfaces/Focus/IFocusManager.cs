using ProberErrorCode;
using ProberInterfaces.Param;

namespace ProberInterfaces.Focus
{
    public interface IFocusManager : IFactoryModule, IModule
    {
        EventCodeEnum ValidationFocusParam(FocusParameter param);
        void MakeDefalutFocusParam(EnumProberCam cam, EnumAxisConstants axis, FocusParameter param);
        void MakeDefalutFocusParam(EnumProberCam cam, EnumAxisConstants axis, FocusParameter param, double range);

        IFocusing GetFocusingModel(ModuleDllInfo info);
    }

    public static class FocusingStaticParam
    {

        public static int FocusDelayTime { get; set; } = 25;

        public static string SaveDebugImagePath { get; set; } = "C:\\Logs\\C01\\FocusDebugImage";

        public static bool SaveImageFlag { get; set; } = false;
        public static bool OverlayFocusROIFlag { get; set; } = false;

        public static double FocusingOffsetX { get; set; } = 0;
        public static double FocusingOffsetY { get; set; } = 0;
        public static double FocusingWidth { get; set; } = 0;
        public static double FocusingHeight { get; set; } = 0;

        public static double PinFocusingOffsetX { get; set; } = 0;
        public static double PinFocusingOffsetY { get; set; } = 0;
        public static double PinFocusingWidth { get; set; } = 0;
        public static double PinFocusingHeight { get; set; } = 0;
        public static int SetIdleGrabCount { get; set; } = 2;
        public static EventCodeEnum ErrorEventCodeEnum = EventCodeEnum.UNDEFINED;
    }
}
