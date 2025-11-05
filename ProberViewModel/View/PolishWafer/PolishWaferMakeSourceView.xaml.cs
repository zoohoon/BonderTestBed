using LogModule;
using ProberInterfaces;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using UcAnimationScrollViewer;

namespace UcPolishWaferMakeSourceView
{
    public partial class PolishWaferMakeSourceView : UserControl, IMainScreenView
    {
        private readonly Guid _ViewGUID = new Guid("4a828af3-fd28-4d05-974b-3e526f781454");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }

        public PolishWaferMakeSourceView()
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
            catch (Exception err)
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
            catch (Exception err)
            {
                throw;
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
    }

    public class DefineNameDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string retval = values[0].ToString();

            try
            {
                if (values.Length == 2)
                {
                    if (values[1] is bool canuse)
                    {
                        if (canuse)
                        {
                            values[0] = values[0] is string text ? text.Replace("_", "__") : "";
                            retval = values[0].ToString();
                        }
                        else
                        {
                            values[0] = values[0] is string text ? text.Replace("_", "__") : "";
                            retval = values[0].ToString() + " (Used)";
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
