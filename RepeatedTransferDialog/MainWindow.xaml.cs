namespace RepeatedTransferDialog
{
    using LogModule;
    using ProberInterfaces.Enum;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Threading;

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        //private RepeatedTransferVM RepeatedTransferVM;

        public MainWindow(RepeatedTransferVM vm)
        {
            this.DataContext = vm;
            InitializeComponent();
        }
        bool? private_dialog_result;

        delegate void FHideWindow();

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            private_dialog_result = DialogResult;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new
            FHideWindow(_HideThisWindow));
        }
        void _HideThisWindow()
        {
            this.Hide();
            (typeof(Window)).GetField("_isClosing", BindingFlags.Instance |
            BindingFlags.NonPublic).SetValue(this, false);
            (typeof(Window)).GetField("_dialogResult", BindingFlags.Instance |
            BindingFlags.NonPublic).SetValue(this, private_dialog_result);
            private_dialog_result = null;
        }
    }

    public class RpeatedModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                EnumRepeatedTransferMode param = (EnumRepeatedTransferMode)parameter;
                if (value is EnumRepeatedTransferMode.OneCellMode)
                {
                    if ((EnumRepeatedTransferMode)value == param)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (value is EnumRepeatedTransferMode.MultipleCellMode)
                {
                    if ((EnumRepeatedTransferMode)value == param)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return false;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }

    public class AbortBtnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return !(bool)value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return false;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}
