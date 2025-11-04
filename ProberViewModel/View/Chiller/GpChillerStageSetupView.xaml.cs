using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ProberViewModel.View.Chiller
{
    using LogModule;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.Globalization;
    using System.Windows.Media.Animation;
    using UcAnimationScrollViewer;

    /// <summary>
    /// Interaction logic for GpChillerStageSetupView.xaml
    /// </summary>
    public partial class GpChillerStageSetupView : UserControl, IMainScreenView
    {
        public GpChillerStageSetupView()
        {
            InitializeComponent();
        }
        private Autofac.IContainer Container { get; set; }

        readonly Guid _ViewGUID = new Guid("0C1CA138-115F-24A9-E172-378C0E6D9B28");
        public Guid ScreenGUID { get { return _ViewGUID; } }



        private RelayCommand<object> _StageListUpBtnClickCommand;
        public ICommand StageListUpBtnClickCommand
        {
            get
            {
                if (null == _StageListUpBtnClickCommand) _StageListUpBtnClickCommand = new RelayCommand<object>(StageListUpBtnClickCommandFunc);
                return _StageListUpBtnClickCommand;
            }
        }
        private async void StageListUpBtnClickCommandFunc(object parameter)
        {
            try
            {
                AnimationScrollViewer svCategoryView = null;
                if (parameter is AnimationScrollViewer)
                {
                    svCategoryView = (AnimationScrollViewer)parameter;
                }
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
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _StageListDownBtnClickCommand;
        public ICommand StageListDownBtnClickCommand
        {
            get
            {
                if (null == _StageListDownBtnClickCommand) _StageListDownBtnClickCommand = new RelayCommand<object>(StageListDownBtnClickCommandFunc);
                return _StageListDownBtnClickCommand;
            }
        }
        private async void StageListDownBtnClickCommandFunc(object parameter)
        {
            try
            {
                AnimationScrollViewer svCategoryView = null;
                if (parameter is AnimationScrollViewer)
                {
                    svCategoryView = (AnimationScrollViewer)parameter;
                }
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
                LoggerManager.Exception(err);
            }
        }

        private void StageListDownBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
               
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }

    public class ValveColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool isconnect = (bool)value;
                if (isconnect)
                    return Colors.Blue;
                else
                    return Colors.Red;
            }
            return Colors.LightGray;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class VirtualKeyBoardTextBoxConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            TextBox textBox = null;
            IElement element = null;
            if (values[0] != DependencyProperty.UnsetValue)
            {
                textBox = (System.Windows.Controls.TextBox)values[0];
            }
            if (values[1] != DependencyProperty.UnsetValue)
            {
                element = (IElement)values[1];
            }

            return (textBox, element);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StageVirtualKeyBoardTextBoxConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            TextBox textBox = null;
            int index = 0;
            if (values[0] != DependencyProperty.UnsetValue)
            {
                textBox = (System.Windows.Controls.TextBox)values[0];
            }
            if (values[1] != DependencyProperty.UnsetValue)
            {
                index = (int)values[1];
            }

            return (textBox, index);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ChillerActiveIConPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                                object parameter, CultureInfo culture)
        {
            if (value is bool isactive)
            {
                if (isactive)
                {
                    return "PauseCircleOutline";
                }
                else
                {
                    return "PlayCircleOutline";
                }
            }
            return "HighlightOff";
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class ChillerActiveContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                             object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool isactive = (bool)value;
                if (isactive)
                    return "STOP";
                else
                    return "START";
            }
            return "---";
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ChillerMatenanceEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                     object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !((bool)value);
            }
            return false;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StageEndEmergencyEnableForeGroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                             object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                    return Colors.MediumPurple;
                else
                    return Colors.DimGray;
            }
            return Colors.DimGray;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class EndEmergencyVisiableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                             object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }
            return Visibility.Hidden;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ActivateVisiableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                             object parameter, CultureInfo culture)
        {
            try
            {
                if (value is EnumTemperatureState)
                {
                    EnumTemperatureState state = (EnumTemperatureState)value;
                    if (state == EnumTemperatureState.WaitForCondition)
                        return Visibility.Visible;
                    else
                        return Visibility.Hidden;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Visibility.Hidden;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StageCommandBtnForeGroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                             object parameter, CultureInfo culture)
        {
            Color color = Colors.DimGray;
            try
            {
                if (value is EnumTemperatureState)
                {
                    EnumTemperatureState state = (EnumTemperatureState)value;
                    if (state == EnumTemperatureState.WaitForCondition)
                        return Colors.MediumPurple;
                    else
                        return Colors.DimGray;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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

