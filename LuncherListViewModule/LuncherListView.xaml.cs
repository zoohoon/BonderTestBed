using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using LogModule;

namespace LuncherListViewModule
{
    using System.Globalization;
    using System.Windows.Media.Animation;
    using UcAnimationScrollViewer;

    /// <summary>
    /// LuncherListView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LuncherListView : UserControl
    {
        public LuncherListView()
        {
            InitializeComponent();
        }
        private void StageListUpBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DoubleAnimation verticalAnimation = new DoubleAnimation();

                verticalAnimation.From = svCategoryLuncherView.VerticalOffset;
                verticalAnimation.To = svCategoryLuncherView.VerticalOffset - ((svCategoryLuncherView.ActualHeight / 3) * 2);
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);

                Storyboard.SetTarget(verticalAnimation, svCategoryLuncherView);
                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimationScrollViewer.CurrentVerticalOffsetProperty));

                storyboard.Begin();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        private void StageListDownBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DoubleAnimation verticalAnimation = new DoubleAnimation();

                verticalAnimation.From = svCategoryLuncherView.VerticalOffset;
                verticalAnimation.To = svCategoryLuncherView.VerticalOffset + ((svCategoryLuncherView.ActualHeight / 3) * 2);
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);

                Storyboard.SetTarget(verticalAnimation, svCategoryLuncherView);
                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimationScrollViewer.CurrentVerticalOffsetProperty));

                storyboard.Begin();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class ListViewConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class CellObjectForeGroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            try
            {
                if (value != null)
                {
                    if (value is bool)
                    {
                        if (((bool)value))
                            //return Colors.Green;
                            return new SolidColorBrush(Colors.Green);
                        else
                            return new SolidColorBrush(Colors.White);
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                return null;
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class HiddenDriveObjectForeGroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            try
            {
                if (value != null)
                {
                    if (value is bool)
                    {
                        if (((bool)value))
                            return Visibility.Collapsed;
                        else
                            return Visibility.Visible;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                return null;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class VisibleDriveObjectForeGroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            try
            {
                if (value != null)
                {
                    if (value is bool)
                    {
                        if (((bool)value))
                            return Visibility.Visible;
                        else
                            return Visibility.Collapsed;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                return null;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    


}
