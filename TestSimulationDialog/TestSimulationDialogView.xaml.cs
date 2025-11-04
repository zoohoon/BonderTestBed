using LogModule;
using ProberInterfaces;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace TestSimulationDialog
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TestSimulationDialogView : Window
    {
        TestSimulationDialogViewModel vm;

        public TestSimulationDialogView()
        {
            InitializeComponent();

            AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
            Version assemblyVersion = assemblyName.Version;
            Title = $"Test Simulation (ver {assemblyVersion.ToString()})";

            vm = new TestSimulationDialogViewModel();
            this.DataContext = vm;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;
                Visibility = Visibility.Hidden;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    /// <summary>
	/// Converts a Boolean value into an opposite Boolean value (and back)
	/// </summary>
	[ValueConversion(typeof(bool), typeof(bool))]
    [MarkupExtensionReturnType(typeof(BoolToOppositeBoolConverter))]
    public class BoolToOppositeBoolConverter : MarkupExtension, IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool)
                {
                    return !(bool)value;
                }

                return value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool)
                {
                    return !(bool)value;
                }

                return value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion

        #region MarkupExtension "overrides"

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new BoolToOppositeBoolConverter();
        }

        #endregion
    }


    public class IOOverrideToForegroundConverter : MarkupExtension, IValueConverter
    {
        static SolidColorBrush NONE_Color = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush NLO_Color = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush NHI_Color = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush EMUL_Color = new SolidColorBrush(Colors.Gray);

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retval = null;

            try
            {
                if (value is EnumIOOverride)
                {
                    EnumIOOverride val = (EnumIOOverride)value;

                    switch (val)
                    {
                        case EnumIOOverride.NONE:
                            retval = NONE_Color;
                            break;
                        case EnumIOOverride.NLO:
                            retval = NLO_Color;
                            break;
                        case EnumIOOverride.NHI:
                            retval = NHI_Color;
                            break;
                        case EnumIOOverride.EMUL:
                            retval = EMUL_Color;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion

        #region MarkupExtension "overrides"

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new BoolToOppositeBoolConverter();
        }

        #endregion
    }

    public class GetIndexMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                var collection = values[1] as ObservableCollection<ObservableCollection<IOPortDescripter<bool>>>;

                if (collection != null)
                {
                    ObservableCollection<IOPortDescripter<bool>> iOPort = values[0] as ObservableCollection<IOPortDescripter<bool>>;

                    if (iOPort != null)
                    {
                        var itemIndex = collection.IndexOf(iOPort);
                        retval = (itemIndex + 1).ToString();
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
            throw new NotImplementedException("GetIndexMultiConverter_ConvertBack");
        }
    }

    public class YourConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("YourConverter");
        }
    }
    
}
