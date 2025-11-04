using ProberInterfaces;
using System;
using System.Windows.Controls;
using System.Windows;
using System.Globalization;
using System.Windows.Data;
using SequenceRunnerModule;

namespace UcNCPadChangeScreenView
{
    public partial class NCPadChangeScreenView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("82816653-AE8A-462E-9F9A-CAE770926A37");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public NCPadChangeScreenView()
        {
            InitializeComponent();
        }
    }

    public static class ListBoxExtensions
    {
        #region ListBoxExtensions.KeepSelectedItemVisible Attached Property
        public static bool GetKeepSelectedItemVisible(ListBox lb)
        {
            return (bool)lb.GetValue(KeepSelectedItemVisibleProperty);
        }

        public static void SetKeepSelectedItemVisible(ListBox lb, bool value)
        {
            lb.SetValue(KeepSelectedItemVisibleProperty, value);
        }

        public static readonly DependencyProperty KeepSelectedItemVisibleProperty =
            DependencyProperty.RegisterAttached("KeepSelectedItemVisible", typeof(bool), typeof(ListBoxExtensions),
                new PropertyMetadata(false, KeepSelectedItemVisible_PropertyChanged));

        private static void KeepSelectedItemVisible_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var lb = (ListBox)d;

                if ((bool)e.NewValue)
                {
                    lb.SelectionChanged += ListBox_SelectionChanged;
                    ScrollSelectedItemIntoView(lb);
                }
                else
                {
                    lb.SelectionChanged -= ListBox_SelectionChanged;
                }
            }
            catch (Exception err)
            {
                throw;
            }
        }

        private static void ScrollSelectedItemIntoView(ListBox lb)
        {
            lb.ScrollIntoView(lb.SelectedItem);
        }

        private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScrollSelectedItemIntoView((ListBox)sender);
        }
        #endregion ListBoxExtensions.KeepSelectedItemVisible Attached Property
    }

    public class ModuleStateIsIdleStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;
            try
            {
                if (value is ModuleStateBase && targetType == typeof(bool))
                {
                    ModuleStateBase module = (ModuleStateBase)value;

                    if (module.State == ModuleStateEnum.IDLE)
                    {
                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                    }
                }
            }
            catch (Exception err)
            {
                throw;
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class StateIsErrorStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;
            try
            {
                if (value is SequenceRunnerErrorState && targetType == typeof(bool))
                {
                    retVal = true;

                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                throw;
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class StateIsManualStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;
            try
            {
                if (value is SequenceRunnerRecoveryState && targetType == typeof(bool))
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                throw;
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class StateIsErrorOrManualStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;
            try
            {
                if (((value is SequenceRunnerManualWaitingState) || (value is SequenceRunnerErrorState))
    && targetType == typeof(bool))
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                throw;
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
