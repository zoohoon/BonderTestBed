using System;


namespace ProberInterfaces
{
    using ProberInterfaces.Param;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.Windows.Input;

    [ServiceContract(Name = "DisplayPort")]
    public interface IDisplayPort : INotifyPropertyChanged
    {
        ICamera AssignedCamera { get; set; }
        System.Windows.Point GetCornerInfo();
        //void SetImage(ICamera cam, WriteableBitmap wrbmp = null);
        void SetImage(ICamera cam, ImageBuffer img);
        double TargetRectangleTop { get; set; }
        double TargetRectangleLeft { get; set; }
        double TargetRectangleWidth { get; set; }
        double TargetRectangleHeight { get; set; }

        double MoveXValue { get; set; }
        double MoveYValue { get; set; }
        double SetZeroCoordXPos { get; set; }
        double SetZeroCoordYPos { get; set; }
        double SetZeroCoordZPos { get; set; }
        double StandardOverlayCanvaseWidth { get; }
        double StandardOverlayCanvaseHeight { get; }
        RegisteImageBufferParam GetPatternRectInfo();
        double ConvertDisplayWidth(double sizex, double imgwidth);
        double ConvertDisplayHeight(double sizey, double imgheight);
        void RegistMouseDownEvent(MouseButtonEventHandler MouseDownHandler);
        Object GetViewObject();
        Object GetRenderControl();

        bool EnalbeClickToMove { get; set; }

        bool GridVisibility { get; set; }
    }


    public enum StageCam
    {
        WAFER_HIGH_CAM,
        WAFER_LOW_CAM,
        PIN_HIGH_CAM,
        PIN_LOW_CAM,
    }
    public enum LoaderCam
    {
        PACL6_CAM,
        PACL8_CAM,
        PACL12_CAM,
        ARM_6_CAM,
        ARM_8_12_CAM,
        OCR1_CAM,
        OCR2_CAM,
    }
}
