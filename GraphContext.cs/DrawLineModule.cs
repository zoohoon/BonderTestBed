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
    using System.Windows.Media;
    using System.Windows.Shapes;

    [DataContract]
    public class DrawLineModule : ControlDrawableBase, INotifyPropertyChanged
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

        private double _XStart;
        [DataMember]
        public double XStart
        {
            get { return _XStart; }
            set
            {
                if (value != _XStart)
                {
                    _XStart = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _YStart;
        [DataMember]
        public double YStart
        {
            get { return _YStart; }
            set
            {
                if (value != _YStart)
                {
                    _YStart = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _XEnd;
        [DataMember]
        public double XEnd
        {
            get { return _XEnd; }
            set
            {
                if (value != _XEnd)
                {
                    _XEnd = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _YEnd;
        [DataMember]
        public double YEnd
        {
            get { return _YEnd; }
            set
            {
                if (value != _YEnd)
                {
                    _YEnd = value;
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

        private Line _Line;

        #endregion

        public void SetParameter(double xstart, double ystart, double xend, double yend)
        {
            try
            {
                this.XStart = xstart;
                this.YStart = ystart;
                this.XEnd = xend;
                this.YEnd = yend;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetParameter(double xstart, double ystart, double xend, double yend, Color color)
        {
            try
            {
                this.XStart = xstart;
                this.YStart = ystart;
                this.XEnd = xend;
                this.YEnd = yend;
                this.Color = color;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void SetParameter(double xstart, double ystart, double xend, double yend, double thickness)
        {
            try
            {
                this.XStart = xstart;
                this.YStart = ystart;
                this.XEnd = xend;
                this.YEnd = yend;
                this.Thickness = thickness;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetParameter(double xstart, double ystart, double xend, double yend, Color color, double thickness,
            DispFlipEnum verflip = DispFlipEnum.NONE, DispFlipEnum horflip = DispFlipEnum.NONE, int left = 0, int right = 0, int top = 0, int bottom = 0
            , double StrokeDashOffset = -1)
        {
            try
            {
                
                if (horflip == DispFlipEnum.FLIP)
                {                    
                    double cenx = (left + right) / 2;
                    xstart = cenx + (cenx - xstart);
                    xend = cenx + (cenx - xend);
                }

                if (verflip == DispFlipEnum.FLIP)
                {
                    double ceny = (bottom + top) / 2;
                    ystart = ceny + (ceny - ystart);
                    yend = ceny + (ceny - yend);
                }


                this.XStart = xstart;
                this.YStart = ystart;
                this.XEnd = xend;
                this.YEnd = yend;
                this.Color = color;
                this.Thickness = thickness;
                this.StrokeDashOffset = StrokeDashOffset;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Draw(IDisplay display, ImageBuffer img)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                _Line = new Line();
                _Line.Stroke = new SolidColorBrush(Colors.AntiqueWhite);

                if (Color != default(Color) && ((SolidColorBrush)(_Line.Stroke)).Color != Color)
                {
                    _Line.Stroke = new SolidColorBrush(Color);
                }

                if (_Line.StrokeThickness != Thickness)
                {
                    _Line.StrokeThickness = Thickness;
                }

                if (this.StrokeDashOffset != -1) 
                {
                    _Line.StrokeDashArray = new DoubleCollection(new double[] { this.StrokeDashOffset, this.StrokeDashOffset });
                }

                _Line.X1 = Math.Round(XStart, 5) * (display.OverlayCanvas.ActualWidth / img.SizeX);
                _Line.Y1 = Math.Round(YStart, 5) * (display.OverlayCanvas.ActualHeight / img.SizeY);
                _Line.X2 = Math.Round(XEnd, 5) * (display.OverlayCanvas.ActualWidth / img.SizeX);
                _Line.Y2 = Math.Round(YEnd, 5) * (display.OverlayCanvas.ActualHeight / img.SizeY);

                display.OverlayCanvas.Children.Add(_Line);
                //}));
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "Draw() : Error occured");
                LoggerManager.Exception(err);
            }
            return retval;
        }
    }
}
