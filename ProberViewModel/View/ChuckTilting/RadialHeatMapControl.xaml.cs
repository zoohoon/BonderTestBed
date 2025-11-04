using LogModule;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

namespace ChuckTiltingViewControl
{
    /// <summary>
    /// Interaction logic for RadialHeatMap.xaml
    /// </summary>
    public partial class RadialHeatMapControl : UserControl
    {
        public RadialHeatMapControl()
        {
            InitializeComponent();
        }
    }
    public class AngleToNormPosConverter : IValueConverter
    {
        //static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
        //static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
        //static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        //static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        //static SolidColorBrush Crimsonbrush = new SolidColorBrush(Colors.Crimson);
        //static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Yellow);
        //static SolidColorBrush Balckbrush = new SolidColorBrush(Colors.Black);
        //static SolidColorBrush Whitebrush = new SolidColorBrush(Colors.White);
        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            //Brush brush = Redbrush;
            Point originPoint = new Point(0, 0);
            try
            {
                double angle;
                int intAngle;
                if (value is int)
                {
                    intAngle = (int)value;
                    angle = (double)intAngle;
                    originPoint.X = 0.5 + Math.Cos(Math.PI * (angle) / 180.0);
                    originPoint.Y = 0.5 + Math.Sin(Math.PI * (angle) / 180.0);
                }
                if (value is double)
                {
                    angle = (double)value;
                    originPoint.X = 0.5 + (Math.Cos(Math.PI * (angle) / 180.0) / 2d);
                    originPoint.Y = 0.5 + ((Math.Sin(Math.PI * (angle) / 180.0)) * -1d / 2d);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return originPoint;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class HeightToColorConverter : IValueConverter
    {
        //static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
        //static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
        //static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Greenbrush = new SolidColorBrush(Colors.Green);
        //static SolidColorBrush Crimsonbrush = new SolidColorBrush(Colors.Crimson);
        //static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Yellow);
        //static SolidColorBrush Balckbrush = new SolidColorBrush(Colors.Black);
        //static SolidColorBrush Whitebrush = new SolidColorBrush(Colors.White);
        static Color levelColor = Colors.Red;
        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            Brush brush = Redbrush;
            Color color = Colors.Red;
            try
            {
                double height = 0;
                int iHeight;
                if (value is int)
                {
                    iHeight = (int)value;
                    height = (double)iHeight;
                }
                else if (value is double)
                {
                    height = (double)value;

                }
                if (height == 0)
                {
                    brush = Greenbrush;
                    color = Colors.Green;
                }
                else
                {
                    double heightMax = 250;

                    byte r;
                    byte g;
                    double heightForColor = height;
                    if (heightForColor > 250)
                    {
                        heightForColor = 250;
                    }
                    r = (byte)((heightForColor / heightMax) * 255d);
                    g = (byte)(32 - (heightForColor / heightMax) * 32d);
                    levelColor = Color.FromRgb(r, g, 0);
                    color = levelColor;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
