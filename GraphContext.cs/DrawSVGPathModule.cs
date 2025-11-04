using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Vision;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Vision.GraphicsContext
{
    public class DrawSVGPathModule : ControlDrawableBase, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName] string propertyName = "")
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


        private double _XPos;
        [DataMember]
        public double XPos
        {
            get { return _XPos; }
            set
            {
                if (value != _XPos)
                {
                    _XPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _YPos;
        [DataMember]
        public double YPos
        {
            get { return _YPos; }
            set
            {
                if (value != _YPos)
                {
                    _YPos = value;
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

        private double _Thickness = 1;
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


        private Geometry _Data;
        [DataMember]
        public Geometry Data
        {
            get { return _Data; }
            set
            {
                if (value != _Data)
                {
                    _Data = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Path _SVG;
        double xStart = 0.0;
        double yStart = 0.0;
        #endregion

        public DrawSVGPathModule()
        {

        }

        public DrawSVGPathModule(double xpos, double ypos, double width, double height, string data)
        {
            try
            {
                this.XPos = xpos;
                this.YPos = ypos;
                this.Width = width;
                this.Height = height;
                Geometry geometry = Geometry.Parse(data);
                this.Data = geometry;
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
                display.OverlayCanvas.Children.Remove(_SVG);
                _SVG = new Path();
                _SVG.Stroke = new SolidColorBrush(Colors.AntiqueWhite);
                if (((SolidColorBrush)(_SVG.Stroke)).Color != Color && Color != default(Color))
                {
                    _SVG.Stroke = new SolidColorBrush(Color);
                }

                if (_SVG.StrokeThickness != Thickness)
                {
                    _SVG.StrokeThickness = Thickness;
                }

                _SVG.Data = Data;

                //_SVG.Width = Width * (display.OverlayCanvas.ActualWidth / img.SizeX);
                //_SVG.Height = Height * (display.OverlayCanvas.ActualHeight / img.SizeY);
                _SVG.Fill = new SolidColorBrush(Color);

                Size renderedSize = MeasureRenderedSize(_SVG);

                xStart = (XPos - (renderedSize.Width / 2))* (display.OverlayCanvas.ActualWidth / img.SizeX);
                yStart = (XPos - (renderedSize.Height / 2)) * (display.OverlayCanvas.ActualHeight / img.SizeY);

                Canvas.SetLeft(_SVG, xStart);
                Canvas.SetTop(_SVG, yStart);

                display.OverlayCanvas.Children.Add(_SVG);
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        static Size MeasureRenderedSize(UIElement element)
        {
            // 요소를 렌더링하고 측정
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));

            // 렌더링 된 크기 반환
            return element.RenderSize;
        }
    }
}
