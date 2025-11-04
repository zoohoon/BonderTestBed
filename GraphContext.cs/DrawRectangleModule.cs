
using System;

namespace Vision.GraphicsContext
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Vision;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;

    [DataContract]

    public class DrawRectangleModule : ControlDrawableBase, INotifyPropertyChanged
    {

        #region ==> PropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property

        private ICamera _Camera;
        public ICamera Camera
        {
            get { return _Camera; }
            set
            {
                if (value != _Camera)
                {
                    _Camera = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _XCenter;
        [DataMember]
        public double XCenter
        {
            get { return _XCenter; }
            set
            {
                if (value != _XCenter)
                {
                    _XCenter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _YCenter;
        [DataMember]
        public double YCenter
        {
            get { return _YCenter; }
            set
            {
                if (value != _YCenter)
                {
                    _YCenter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Width;
        [DataMember]
        public double Width
        {
            get { return _Width; }
            set
            {
                if (value != _Width)
                {
                    _Width = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Height;
        [DataMember]
        public double Height
        {
            get { return _Height; }
            set
            {
                if (value != _Height)
                {
                    _Height = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OffsetX;
        [DataMember]
        public double OffsetX
        {
            get { return _OffsetX; }
            set
            {
                if (value != _OffsetX)
                {
                    _OffsetX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OffsetY;
        [DataMember]
        public double OffsetY
        {
            get { return _OffsetY; }
            set
            {
                if (value != _OffsetY)
                {
                    _OffsetY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Thickness=1;
        [DataMember]
        public double Thickness
        {
            get { return _Thickness; }
            set
            {
                if (value != _Thickness)
                {
                    _Thickness = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Angle = 0.0;
        [DataMember]
        public double Angle
        {
            get { return _Angle; }
            set
            {
                if (value != _Angle)
                {
                    _Angle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Rectangle _Rectangle;
        double xStart = 0.0;
        double yStart = 0.0;
        #endregion

        public DrawRectangleModule()
        {

        }

        public DrawRectangleModule(ICamera cam, double xcenter, double ycenter, double width, double height)
        {
            try
            {
                this.Camera = cam;
                this.XCenter = xcenter;
                this.YCenter = ycenter;
                this.Width = width;
                this.Height = height;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public DrawRectangleModule(
            double xcenter, double ycenter, double width, double height, 
            double offsetx = 0, double offsety = 0,
            DispFlipEnum verflip = DispFlipEnum.NONE, 
            DispFlipEnum horflip = DispFlipEnum.NONE, 
            int left = 0, int right = 0, int top = 0, int bottom = 0)
        {
            try
            {
                //screenleft = 0;
                //screenright = 960;
                //screentop = 0;
                //screenbottom = 960;
                //xcenter = 300;
                //ycenter = 300;

                if (horflip == DispFlipEnum.FLIP)
                {
                    //double cenx = ((double)left + (double)right) / 2;
                    //xcenter = cenx + (cenx - xcenter);
                    xcenter = right - xcenter;
                }

                if (verflip == DispFlipEnum.FLIP)
                {
                    //double ceny = ((double)top + (double)bottom) / 2;
                    //ycenter = ceny + (ceny - ycenter);
                    ycenter = bottom - ycenter;
                }


                this.XCenter = xcenter;
                this.YCenter = ycenter;
                this.Width = width;
                this.Height = height;
                this.OffsetX = offsetx;
                this.OffsetY = offsety;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public DrawRectangleModule(double xcenter, double ycenter, double width, double height, Color color, double thickness)
        {
            try
            {
                this.XCenter = xcenter;
                this.YCenter = ycenter;
                this.Width = width;
                this.Height = height;
                this.Color = color;
                this.Thickness = thickness;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetParameter(double xcenter, double ycenter, double width, double height)
        {
            try
            {
                if (Camera != null)
                {
                    this.XCenter = xcenter;
                    this.YCenter = ycenter;
                    this.Width = width;
                    this.Height = height;

                    Camera.UpdateOverlayFlag = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Draw(IDisplay display, ImageBuffer img)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                //                display.OverlayCanvas.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                //(System.Threading.ThreadStart)delegate ()
                //{
                display.OverlayCanvas.Children.Remove(_Rectangle);
                _Rectangle = new Rectangle();
                _Rectangle.Stroke = new SolidColorBrush(Colors.AntiqueWhite);


                if (((SolidColorBrush)(_Rectangle.Stroke)).Color != Color && Color != default(Color))
                {
                    _Rectangle.Stroke = new SolidColorBrush(Color);
                }

                if (_Rectangle.StrokeThickness != Thickness)
                {
                    _Rectangle.StrokeThickness = Thickness;
                }

                _Rectangle.Width = (Width + Math.Abs(OffsetX)) * (display.OverlayCanvas.ActualWidth / img.SizeX);
                _Rectangle.Height = (Height + Math.Abs(OffsetY)) * (display.OverlayCanvas.ActualHeight / img.SizeY);

                xStart = (XCenter - (Width / 2) + OffsetX) * (display.OverlayCanvas.ActualWidth / img.SizeX);
                yStart = (YCenter - (Height / 2) + OffsetY) * (display.OverlayCanvas.ActualHeight / img.SizeY);

                if (Angle != 0.0)
                {
                    RotateTransform rotation = new RotateTransform();
                    rotation.Angle = Angle;
                    rotation.CenterX = _Rectangle.Width / 2;
                    rotation.CenterY = _Rectangle.Height / 2;
                    _Rectangle.RenderTransform = rotation;
                }

                Canvas.SetLeft(_Rectangle, xStart);
                Canvas.SetTop(_Rectangle, yStart);

                display.OverlayCanvas.Children.Add(_Rectangle);
                //});
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "Draw() : Error occured");
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}
