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

    [DataContract]
    public class DrawTextModule : ITextDrawable, INotifyPropertyChanged
    {


        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region //..Property


        private string _Text;
        [DataMember]
        public string Text
        {
            get { return _Text; }
            set
            {
                if (value != _Text)
                {
                    _Text = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        private double _FontSize = 12;
        [DataMember]
        public double FontSize
        {
            get { return _FontSize; }
            set
            {
                if (value != _FontSize)
                {
                    _FontSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _StringTypeFontColor;
        [DataMember]
        public string StringTypeFontColor
        {
            get { return _StringTypeFontColor; }
            set
            {
                if (value != _StringTypeFontColor)
                {
                    _StringTypeFontColor = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Color _Fontcolor = default(Color);
        public Color Fontcolor
        {
            get { return _Fontcolor; }
            set
            {
                if (value != _Fontcolor)
                {
                    _Fontcolor = value;
                    StringTypeFontColor = Fontcolor.ToString();
                    RaisePropertyChanged();
                }
            }
        }

        private string _StringTypeBackColor;
        [DataMember]
        public string StringTypeBackColor
        {
            get { return _StringTypeBackColor; }
            set
            {
                if (value != _StringTypeBackColor)
                {
                    _StringTypeBackColor = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Color _BackColor = default(Color);
        public Color BackColor
        {
            get { return _BackColor; }
            set
            {
                if (value != _BackColor)
                {
                    _BackColor = value;
                    StringTypeBackColor = BackColor.ToString();
                    RaisePropertyChanged();
                }
            }
        }



        private TextBlock _TextBlock;
        #endregion


        public DrawTextModule()
        {

        }
        public DrawTextModule(double xstart, double ystart, string text)
        {
            try
            {
                XStart = xstart;
                YStart = ystart;
                Text = text;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void SetParameter(double xstart, double ystart, string text)
        {
            try
            {
                XStart = xstart;
                YStart = ystart;
                Text = text;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void SetParameter(string text, double xstart, double ystart, double fontsize, Color fontcolor)
        {
            try
            {
                Text = text;
                XStart = xstart;
                YStart = ystart;
                FontSize = fontsize;
                Fontcolor = fontcolor;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void SetParameter(string text, double xstart, double ystart, double fontsize, Color fontcolor, Color backcolor)
        {
            try
            {
                Text = text;
                XStart = xstart;
                YStart = ystart;
                FontSize = fontsize;
                Fontcolor = fontcolor;
                BackColor = backcolor;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum Draw(IDisplay display, ImageBuffer img)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                _TextBlock = new TextBlock();
                _TextBlock.Foreground = new SolidColorBrush();

                if (Fontcolor != default(Color) || ((SolidColorBrush)(_TextBlock.Foreground)).Color != Fontcolor)
                {
                    _TextBlock.Foreground = new SolidColorBrush(Fontcolor);
                }

                if (BackColor != default(Color) || ((SolidColorBrush)(_TextBlock.Foreground)).Color != BackColor)
                {
                    _TextBlock.Background = new SolidColorBrush(BackColor);
                }

                if (_TextBlock.FontSize != FontSize)
                {
                    _TextBlock.FontSize = FontSize;
                }

                _TextBlock.Text = Text;

                if (XStart < 0 || XStart > img.SizeX) XStart = 0;
                if (YStart < 0 || YStart > img.SizeY) YStart = 0;


                //XStart = (XStart) * (display.OverlayCanvas.ActualWidth / img.SizeX);
                //YStart = (YStart) * (display.OverlayCanvas.ActualHeight / img.SizeY);

                double xStart = (XStart) * (display.OverlayCanvas.ActualWidth / img.SizeX);
                double yStart = (YStart) * (display.OverlayCanvas.ActualHeight / img.SizeY);

                Canvas.SetLeft(_TextBlock, xStart);
                Canvas.SetTop(_TextBlock, yStart);

                display.OverlayCanvas.Children.Add(_TextBlock);

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
