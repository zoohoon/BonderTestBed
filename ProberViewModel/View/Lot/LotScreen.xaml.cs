using ProberInterfaces;
using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
using LogModule;

namespace ProberViewModel
{
    /// <summary>
    /// LotScreen.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LotScreen : UserControl, IMainScreenView
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public LotScreen()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("6223DFD5-EFAA-4B49-AB70-D8A5F03FA65D");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }

    public class ViewTargetConverter : IValueConverter
    {
        public ViewTargetConverter()
        {
            try
            {
            //_MapViewControl.WaferObject = this.StageSupervisor().WaferObject;
            //_MapViewControl.UnderDutDices = this.ProbingModule().ProbingProcessStatus.UnderDutDevs;
            //_MapViewControl.CurrXIndex = (int)this.ProbingModule().ProbingMXIndex;
            //_MapViewControl.CurrYIndex = (int)this.ProbingModule().ProbingMYIndex;
            //_MapViewControl.EnalbeClickToMove = false;
            //_MapViewControl.ZoomLevel = this.StageSupervisor().WaferObject.ZoomLevel;
            //_MapViewControl.CoordinateManager = this.CoordinateManager();
            //_MapViewControl.IsCrossLineVisible = false;
            //_MapViewControl.RenderMode = this.StageSupervisor().WaferObject.MapViewControlMode;

            }
            catch (Exception err)
            {
                 throw;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                //if (value is IWaferObject)
                //    return _MapViewControl;
                //else if (value is ICamera)
                //    return value;
                //else if (value is INeedleCleanObject)
                //    return _NeedleCleanView;

            }
            catch (Exception err)
            {
                return null;
            }
            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AlignStateToForegroundConverter : IValueConverter
    {
        static SolidColorBrush DONEBrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush IDLEBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush UnknownBrush = new SolidColorBrush(Colors.DimGray);

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = UnknownBrush;

            try
            {
                AlignStateEnum state = (AlignStateEnum)value;

                switch (state)
                {
                    case AlignStateEnum.IDLE:
                        retval = IDLEBrush;
                        break;
                    case AlignStateEnum.DONE:
                        retval = DONEBrush;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class PadCountToForegroundConverter : IValueConverter
    {
        static SolidColorBrush RegistBrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush NotRegistBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush UnknownBrush = new SolidColorBrush(Colors.DimGray);

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = UnknownBrush;

            try
            {
                int padcount = (int)value;

                if (padcount > 0)
                {
                    retval = RegistBrush;
                }
                else
                {
                    retval = NotRegistBrush;
                }

                //if (Int32.TryParse(inputvalue, out padcount))
                //{
                //    // you know that the parsing attempt
                //    // was successful.

                //    if (padcount > 0)
                //    {
                //        retval = RegistBrush;
                //    }
                //    else
                //    {
                //        retval = NotRegistBrush;
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class PadCountToTextConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                int padcount = (int)value;

                if (padcount > 0)
                {
                    retval = $"Registered ({padcount})";
                }
                else
                {
                    retval = $"Not registered";
                }

                //string inputvalue = value as string;

                //int padcount = 0;

                //if (Int32.TryParse(inputvalue, out padcount))
                //{
                //    // you know that the parsing attempt
                //    // was successful.

                //    if (padcount > 0)
                //    {
                //        retval = $"Registered ({padcount})";
                //    }
                //    else
                //    {
                //        retval = $"Not registered";
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class WaferStatusToForegroundConverter : IValueConverter
    {
        static SolidColorBrush UnknownBrush = new SolidColorBrush(Colors.DimGray);
        
        static SolidColorBrush NotExistBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush ExistBrush = new SolidColorBrush(Colors.LimeGreen);
        

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = UnknownBrush;

            try
            {
                EnumSubsStatus status = (EnumSubsStatus)value;

                switch (status)
                {
                    case EnumSubsStatus.UNKNOWN:
                    case EnumSubsStatus.UNDEFINED:
                        retval = UnknownBrush;
                        break;
                    case EnumSubsStatus.NOT_EXIST:
                        retval = NotExistBrush;
                        break;
                    case EnumSubsStatus.EXIST:
                        retval = ExistBrush;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
