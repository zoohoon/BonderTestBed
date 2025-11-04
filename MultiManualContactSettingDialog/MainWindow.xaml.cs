using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LogModule;
using System.Windows;
using System.Windows.Controls;
using ProberInterfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LoaderBase;
using LoaderBase.Communication;
using Autofac;

namespace MultiManualContactSettingDialog
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : UserControl, IFactoryModule
    {
        MultiManualContactSettingVM viewModel;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MultiManualContactSettingVM();
            this.DataContext = viewModel;
        }

        public static readonly DependencyProperty CellIndexProperty =
           DependencyProperty.Register(nameof(CellIndex), typeof(string), typeof(MainWindow), new FrameworkPropertyMetadata(""));
        public string CellIndex
        {
            get { return (string)this.GetValue(CellIndexProperty); }
            set { this.SetValue(CellIndexProperty, value); }
        }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (viewModel != null)
                {                    
                    viewModel.StageIndex = Convert.ToInt32(CellIndex);
                    viewModel.Init();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            Brush retBrush = Brushes.Gray;
            try
            {

                if (value is bool)
                {
                    bool bValue = (bool)value;

                    if (bValue == true)
                    {
                        retBrush = Brushes.LightGreen;
                    }
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
            return retBrush;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                // I don't think you'll need this
                throw new Exception("Can't convert back");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class FirstContactAllContactSubCal : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double retVal = 0.0;

            try
            {
                if (2 <= values.Length)
                {
                    if (values[0] is double && values[1] is double)
                    {
                        double firContactHeight = (double)values[0];
                        double allContactHeight = (double)values[1];

                        retVal = allContactHeight - firContactHeight;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }

            return retVal;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Can't convert back");
        }
    }

    public class IsProbingStartPosAllContactConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            bool retVal = false;
            try
            {

                if (value is OverDriveStartPositionType)
                {
                    OverDriveStartPositionType bValue = (OverDriveStartPositionType)value;

                    if (bValue == OverDriveStartPositionType.ALL_CONTACT)
                    {
                        retVal = true;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            OverDriveStartPositionType retVal = OverDriveStartPositionType.ALL_CONTACT;
            try
            {

                if (value is bool)
                {
                    bool bValue = (bool)value;

                    if (bValue == false)
                    {
                        retVal = OverDriveStartPositionType.FIRST_CONTACT;
                    }
                    else
                    {
                        retVal = OverDriveStartPositionType.ALL_CONTACT;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class IsProbingStartPosFirstContactConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            bool retVal = false;
            try
            {

                if (value is OverDriveStartPositionType)
                {
                    OverDriveStartPositionType bValue = (OverDriveStartPositionType)value;

                    if (bValue == OverDriveStartPositionType.FIRST_CONTACT)
                    {
                        retVal = true;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            OverDriveStartPositionType retVal = OverDriveStartPositionType.ALL_CONTACT;
            try
            {

                if (value is bool)
                {
                    bool bValue = (bool)value;

                    if (bValue == false)
                    {
                        retVal = OverDriveStartPositionType.ALL_CONTACT;
                    }
                    else
                    {
                        retVal = OverDriveStartPositionType.FIRST_CONTACT;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
}
