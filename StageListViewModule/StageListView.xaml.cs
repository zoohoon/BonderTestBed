using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UcAnimationScrollViewer;
using LogModule;

namespace StageListViewModule
{
    public partial class StageListView : UserControl
    {
        public StageListView()
        {
            InitializeComponent();
        }

        private void StageListUpBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DoubleAnimation verticalAnimation = new DoubleAnimation();

                verticalAnimation.From = svCategoryView.VerticalOffset;
                verticalAnimation.To = svCategoryView.VerticalOffset - ((svCategoryView.ActualHeight / 3) * 2);
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);

                Storyboard.SetTarget(verticalAnimation, svCategoryView);
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

                verticalAnimation.From = svCategoryView.VerticalOffset;
                verticalAnimation.To = svCategoryView.VerticalOffset + ((svCategoryView.ActualHeight / 3) * 2);
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);

                Storyboard.SetTarget(verticalAnimation, svCategoryView);
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

    #region //..Converter
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
    #endregion
}
