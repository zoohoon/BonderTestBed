
using System.Collections.ObjectModel;


namespace ProberInterfaces
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using System.Windows.Media;
    using ProberInterfaces.Vision;
    using ProberErrorCode;

    public interface IDisplay : INotifyPropertyChanged, IFactoryModule
    {
        WriteableBitmap WrbDispImage { get; set; }
        List<IDisplayPort> DispPorts { get; set; }
        Canvas OverlayCanvas { get; set; }
        ObservableCollection<IDrawable> DrawOverlayContexts { get; set; }
        bool needUpdateOverlayCanvasSize { get; set; }

        void SetImage(ICamera cam, ImageBuffer img = null);
        void ConvArrTOWRB_BAndW(ImageBuffer image, int sizex, int sizey, ICamera cam);
        void ConvArrTOWRB_BAndW_Overlay(byte[] image, int sizex, int sizey, ICamera cam);
        EventCodeEnum Draw(ImageBuffer img);
        EventCodeEnum ClearOverlayCanvas();
        EventCodeEnum OverlayRect(ImageBuffer img,double xCenter, double yCenter,
            double width, double heitgth, Color color = default(Color), double thickness = 1, double angle = 0.0);
        EventCodeEnum OverlayEllipse(ImageBuffer img, double xCenter, double yCenter, double radius,
            Color color = default(Color), double startAngle = 0, double endAngle = 360,  double thickness = 1);
        EventCodeEnum OverlayLine(ImageBuffer img, double xStart, double yStart,
            double xEnd, double yEnd, Color color = default(Color));
        EventCodeEnum OverlayString(ImageBuffer img, string text, double xStart, double yStart,
            double fontsize = 12, Color fontcolor = default(Color), Color backcolor = default(Color));

        event ImageUpdatedEventHandler ImageUpdated;
        bool IsImageUpdatedNull();
    }


}
