using UcAnimationScrollViewer;
using ProberInterfaces;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace UcDeviceChangeView
{
    public partial class DeviceChangeView : UserControl, IMainScreenView
    {
        private Guid _ViewGUID = new Guid("956bb44f-4b89-42b3-b21a-69f896a840fe");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public DeviceChangeView()
        {
            InitializeComponent();
        }

        private void CategoryUpBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DoubleAnimation verticalAnimation = new DoubleAnimation();

                verticalAnimation.From = svViewer.VerticalOffset;
                verticalAnimation.To = svViewer.VerticalOffset - ((svViewer.ActualHeight / 3) * 2);
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);

                Storyboard.SetTarget(verticalAnimation, svViewer);
                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimationScrollViewer.CurrentVerticalOffsetProperty));

                storyboard.Begin();
            }
            catch(Exception err)
            {
                throw;
            }
        }

        private void CategoryDwBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DoubleAnimation verticalAnimation = new DoubleAnimation();

                verticalAnimation.From = svViewer.VerticalOffset;
                verticalAnimation.To = svViewer.VerticalOffset + ((svViewer.ActualHeight / 3) * 2);
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);

                Storyboard.SetTarget(verticalAnimation, svViewer);
                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimationScrollViewer.CurrentVerticalOffsetProperty));
                storyboard.Begin();
            }
            catch(Exception err)
            {
                throw;
            }
        }
    }

    public class TempToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retString = "";
            try
            {
                if (value is double)
                {
                    double convValue = double.Parse(value.ToString());
                    convValue = convValue / 1.0;
                    retString = convValue + retString;
                }
            }
            catch(Exception err)
            {
                throw;
            }

            return retString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

}
