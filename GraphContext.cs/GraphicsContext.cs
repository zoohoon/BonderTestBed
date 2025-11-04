using System;
using System.Collections.Generic;
using System.ComponentModel;
using Matrox.MatroxImagingLibrary;


namespace Vision.GraphicsContext
{
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using ProberInterfaces.Param;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Shapes;
    using System.Windows.Media;
    using ProberErrorCode;
    using LogModule;

    public class GraphicsContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private IDisplay Display;

        private MIL_ID OriginalImgBuffer = MIL.M_NULL;
        private MIL_ID GraphicsImgBuffer = MIL.M_NULL;
        private MIL_ID GraphicsList = MIL.M_NULL;
        double xStart = 0.0;
        double yStart = 0.0;

        private Rectangle _Rectangle ;
        //private Ellipse _Ellipse;
        private Line _Line;
        private TextBlock _Text ;
       

        PinCoordinate _CrossPin = new PinCoordinate(0, 0);
        public int InitModule(IDisplay display)
        {
            int retVal = -1;
            try
            {
                GraList = new ObservableCollection<IGraphicsContext>();
                Display = display;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                throw err;
            }
           
            return retVal;
        }

        private EventCodeEnum InitShape()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                _Rectangle.Stroke = new SolidColorBrush();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public ObservableCollection<IGraphicsContext> GraList { get; set; }

        private ImageBuffer _GraphicsImageBuffer = new ImageBuffer();

        public ImageBuffer GraphicsImageBuffer
        {
            get { return _GraphicsImageBuffer; }
            set { _GraphicsImageBuffer= value; }
        }
        //===== Draw DieOverlay
        private List<UserIndex> ScreenIncludeIndex = new List<UserIndex>();
        private CatCoordinates LUcorner = new CatCoordinates();
        private CatCoordinates RDcorner = new CatCoordinates();
        //=====
        public void DieCount()
        {

        }
        public EventCodeEnum OverlayRect(ImageBuffer img,double xCenter, double yCenter,
            double width, double heigth, Color color = default(Color), double thickness = 1, double angle = 0.0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _Rectangle = new Rectangle();
                    _Rectangle.Stroke = new SolidColorBrush(Colors.AntiqueWhite);


                    if (((SolidColorBrush)(_Rectangle.Stroke)).Color != color && color != default(Color))
                    {
                        _Rectangle.Stroke = new SolidColorBrush(color);
                    }

                    if ( _Rectangle.StrokeThickness != thickness)
                    {
                        _Rectangle.StrokeThickness = thickness;
                    }
                    _Rectangle.Width = width * (Display.OverlayCanvas.ActualWidth / img.SizeX);
                    _Rectangle.Height = heigth * (Display.OverlayCanvas.ActualHeight / img.SizeY);

                    xStart = (xCenter - (width / 2)) * (Display.OverlayCanvas.ActualWidth / img.SizeX);
                    yStart = (yCenter - (heigth / 2)) * (Display.OverlayCanvas.ActualHeight / img.SizeY);

                    if(angle != 0.0)
                    {
                        RotateTransform rotation = new RotateTransform();
                        rotation.Angle = angle;
                        rotation.CenterX = _Rectangle.Width/2;
                        rotation.CenterY = _Rectangle.Height/2;
                        _Rectangle.RenderTransform = rotation;
                    }

                    Canvas.SetLeft(_Rectangle, xStart);
                    Canvas.SetTop(_Rectangle, yStart);

                    Display.OverlayCanvas.Children.Add(_Rectangle);
                }));
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public EventCodeEnum OverlayEllipse(ImageBuffer img, double xCenter, double yCenter, double radius,
            Color color = default(Color), double startAngle = 0, double endAngle = 360, double thickness = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Path _Ellipse = new Path();
                    _Ellipse.Stroke = new SolidColorBrush(Colors.AntiqueWhite);

                    PathGeometry pathGeometry = new PathGeometry();
                    ArcSegment arcSegment = new ArcSegment();
                    PathFigure pathFigure = new PathFigure();

                    if (color != default(Color) && ((SolidColorBrush)(_Ellipse.Stroke)).Color != color)
                    {
                        _Ellipse.Stroke = new SolidColorBrush(color);
                    }

                    if (_Ellipse.StrokeThickness != thickness)
                    {
                        _Ellipse.StrokeThickness = thickness;
                    }

                    //startAngle = ((startAngle % (Math.PI * 2)) + Math.PI * 2) % (Math.PI * 2);
                    //endAngle = ((endAngle % (Math.PI * 2)) + Math.PI * 2) % (Math.PI * 2);
                    if (endAngle < startAngle)
                    {
                        double temp_angle = endAngle;
                        endAngle = startAngle;
                        startAngle = temp_angle;

                    }


                    xCenter *= (Display.OverlayCanvas.ActualWidth / img.SizeX);
                    yCenter *= (Display.OverlayCanvas.ActualHeight / img.SizeY);

                    double angle_diff = endAngle - startAngle;
                    arcSegment.IsLargeArc = angle_diff >= Math.PI;

                    //Set start of arc
                    pathFigure.StartPoint = new Point(xCenter + radius *Convert.ToInt32(Math.Cos(startAngle)),
                        yCenter + radius * Convert.ToInt32(Math.Sin(startAngle)));
                    //set end point of arc.
                    if (endAngle == 360)
                    {
                        arcSegment.Point = new Point(pathFigure.StartPoint.X, pathFigure.StartPoint.Y - 1);
                        arcSegment.SweepDirection = SweepDirection.Clockwise;
                    }
                    else
                    {
                        arcSegment.Point = new Point(xCenter + radius * Convert.ToInt32(Math.Cos(endAngle)), 
                            yCenter + radius * Convert.ToInt32(Math.Sin(endAngle)));
                    }
                
                    arcSegment.Size = new Size(radius, radius);

                    pathFigure.Segments.Add(arcSegment);
                    pathGeometry.Figures.Add(pathFigure);
                    _Ellipse.Data = pathGeometry;

                    Display.OverlayCanvas.Children.Add(_Ellipse);
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public EventCodeEnum OverlayLine(ImageBuffer img, double xStart, double yStart,
            double xEnd, double yEnd, Color color = default(Color), double thickness = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _Line = new Line();
                    _Line.Stroke = new SolidColorBrush(Colors.AntiqueWhite);

                    if (color != default(Color) && ((SolidColorBrush)(_Line.Stroke)).Color != color)
                    {
                        _Line.Stroke = new SolidColorBrush(color);
                    }

                    if (_Line.StrokeThickness != thickness)
                    {
                        _Line.StrokeThickness = thickness;
                    }

                    _Line.X1 = xStart * (Display.OverlayCanvas.ActualWidth / img.SizeX);
                    _Line.Y1 = yStart * (Display.OverlayCanvas.ActualHeight / img.SizeY);
                    _Line.X2 = xEnd * (Display.OverlayCanvas.ActualWidth / img.SizeX);
                    _Line.Y2 = yEnd * (Display.OverlayCanvas.ActualHeight / img.SizeY);

                    Display.OverlayCanvas.Children.Add(_Line);
                }));
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "OverlayLine() Error occurred.");
                LoggerManager.Exception(err);

            }

            return retVal;
        }


        public EventCodeEnum OverlayText(ImageBuffer img, string text, double xStart, double yStart,
           double fontsize = 12, Color fontcolor = default(Color), Color backcolor = default(Color))
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _Text = new TextBlock();
                    _Text.Foreground = new SolidColorBrush();

                    if (fontcolor != default(Color) || ((SolidColorBrush)(_Text.Foreground)).Color != fontcolor)
                    {
                        _Text.Foreground = new SolidColorBrush(fontcolor);
                    }

                    if(backcolor != default(Color) || ((SolidColorBrush)(_Text.Foreground)).Color != backcolor)
                    {
                        _Text.Background = new SolidColorBrush(backcolor);
                    }

                    if (_Text.FontSize != fontsize)
                    {
                        _Text.FontSize = fontsize;
                    }

                    _Text.Text = text;

                    xStart = (xStart) * (Display.OverlayCanvas.ActualWidth / img.SizeX);
                    yStart = (yStart) * (Display.OverlayCanvas.ActualHeight / img.SizeY);

                    Canvas.SetLeft(_Text, xStart);
                    Canvas.SetTop(_Text, yStart);

                    Display.OverlayCanvas.Children.Add(_Text);

                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
