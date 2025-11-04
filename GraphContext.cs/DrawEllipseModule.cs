using System;

namespace Vision.GraphicsContext
{
    using System.Windows;
    using ProberInterfaces;
    using ProberInterfaces.Vision;
    using System.ComponentModel;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using ProberErrorCode;
    using LogModule;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public class DrawEllipseModule : ControlDrawableBase, INotifyPropertyChanged
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

        private double _Radius_X;
        [DataMember]
        public double Radius_X
        {
            get { return _Radius_X; }
            set
            {
                if (value != _Radius_X)
                {
                    _Radius_X = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Radius_Y;
        [DataMember]
        public double Radius_Y
        {
            get { return _Radius_Y; }
            set
            {
                if (value != _Radius_Y)
                {
                    _Radius_Y = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _StartAngle = 0.0;
        [DataMember]
        public double StartAngle
        {
            get { return _StartAngle; }
            set
            {
                if (value != _StartAngle)
                {
                    _StartAngle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _EndAngle = 360.0;
        [DataMember]
        public double EndAngle
        {
            get { return _EndAngle; }
            set
            {
                if (value != _EndAngle)
                {
                    _EndAngle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Thickness = 1.0;
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

        private double _StrokeDashOffset = -1;
        [DataMember]
        public double StrokeDashOffset
        {
            get { return _StrokeDashOffset; }
            set
            {
                if (value != _StrokeDashOffset)
                {
                    _StrokeDashOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        public DrawEllipseModule()
        {

        }
        public DrawEllipseModule(double xcenter, double ycenter, double radius,
            DispFlipEnum verflip = DispFlipEnum.NONE, DispFlipEnum horflip = DispFlipEnum.NONE, int left = 0, int right = 0, int top = 0, int bottom = 0)
        {
            try
            {
                if (horflip == DispFlipEnum.FLIP)
                {
                    double cenx = ((double)left + (double)right) / 2;
                    xcenter = cenx + (cenx - xcenter);
                }

                if (verflip == DispFlipEnum.FLIP)
                {
                    double ceny = ((double)top + (double)bottom) / 2;
                    ycenter = ceny + (ceny - ycenter);
                }

                this.XCenter = xcenter;
                this.YCenter = ycenter;
                this.Radius_X = radius;
                this.Radius_Y = radius;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public DrawEllipseModule(double xcenter, double ycenter, double radius,
            double startangle, double endangle)
        {
            try
            {
                this.XCenter = xcenter;
                this.YCenter = ycenter;
                this.Radius_X = radius;
                this.Radius_Y = radius;
                this.StartAngle = startangle;
                this.EndAngle = endangle;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public DrawEllipseModule(double xcenter, double ycenter, double radius_X, double radius_Y)
        {
            try
            {
                this.XCenter = xcenter;
                this.YCenter = ycenter;
                this.Radius_X = radius_X;
                this.Radius_Y = radius_Y;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        public override EventCodeEnum Draw(IDisplay display, ImageBuffer img)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                Path _Ellipse = new Path();
                _Ellipse.Stroke = new SolidColorBrush(Colors.AntiqueWhite);

                PathGeometry pathGeometry = new PathGeometry();
                ArcSegment arcSegment = new ArcSegment();
                PathFigure pathFigure = new PathFigure();

                if (Color != default(Color) && ((SolidColorBrush)(_Ellipse.Stroke)).Color != Color)
                {
                    _Ellipse.Stroke = new SolidColorBrush(Color);
                }

                if (_Ellipse.StrokeThickness != Thickness)
                {
                    _Ellipse.StrokeThickness = Thickness;
                }

                //StartAngle = ((StartAngle % (Math.PI * 2)) + Math.PI * 2) % (Math.PI * 2);
                //EndAngle = ((EndAngle % (Math.PI * 2)) + Math.PI * 2) % (Math.PI * 2);
                if (EndAngle < StartAngle)
                {
                    double temp_angle = EndAngle;
                    EndAngle = StartAngle;
                    StartAngle = temp_angle;

                }

                if (this.StrokeDashOffset != -1)
                {
                    _Ellipse.StrokeDashArray = new DoubleCollection(new double[] { this.StrokeDashOffset, this.StrokeDashOffset });
                }

                //XCenter = 240;
                //YCenter = 240;
                double xCenter = XCenter * (display.OverlayCanvas.ActualWidth / img.SizeX);
                double yCenter = YCenter * (display.OverlayCanvas.ActualHeight / img.SizeY);

                double angle_diff = EndAngle - StartAngle;
                arcSegment.IsLargeArc = angle_diff >= Math.PI;

                //Set start of arc
                pathFigure.StartPoint = new Point(xCenter + Radius_X * Convert.ToInt32(Math.Cos(StartAngle)),
                    yCenter + Radius_Y * Convert.ToInt32(Math.Sin(StartAngle)));
                //set end point of arc.
                if (EndAngle == 360)
                {
                    arcSegment.Point = new Point(pathFigure.StartPoint.X, pathFigure.StartPoint.Y - 1);
                    arcSegment.SweepDirection = SweepDirection.Clockwise;
                }
                else
                {
                    arcSegment.Point = new Point(xCenter + Radius_X * Convert.ToInt32(Math.Cos(EndAngle)),
                        yCenter + Radius_Y * Convert.ToInt32(Math.Sin(EndAngle)));
                }

                arcSegment.Size = new Size(Radius_X, Radius_Y);

                pathFigure.Segments.Add(arcSegment);
                pathGeometry.Figures.Add(pathFigure);
                _Ellipse.Data = pathGeometry;

                display.OverlayCanvas.Children.Add(_Ellipse);


                //_Ellipse.Width = width * (display.OverlayCanvas.ActualWidth / img.SizeX);
                //_Ellipse.Height = heigth * (display.OverlayCanvas.ActualHeight / img.SizeY);

                //xStart = (XCenter - (width / 2)) * (display.OverlayCanvas.ActualWidth / img.SizeX);
                //yStart = (YCenter - (heigth / 2)) * (display.OverlayCanvas.ActualHeight / img.SizeY);

                //Canvas.SetLeft(_Ellipse, xStart);
                //Canvas.SetTop(_Ellipse, yStart);
                //}));
            }
            catch (Exception err)
            {
                //LoggerManager.Debug($err + "Draw() : Error occured");
                //LoggerManager.Error($err + "Draw() : Error occured");
                LoggerManager.Exception(err);

            }
            return retval;
        }
    }
}
