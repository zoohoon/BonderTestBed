using LogModule;
using ProberInterfaces;
using ResultMapParamObject.STIF;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProberViewModel.View.SubSetting.Device.ResultMap
{
    /// <summary>
    /// ResultMapConverterView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ResultMapConverterView : UserControl, IMainScreenView
    {
        public ResultMapConverterView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("19b2b401-17e5-4a4e-9c11-9dfc7c74b03c");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }

    public class RefCalcModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility reval = Visibility.Collapsed;

            try
            {
                if (value is RefCalcEnum)
                {
                    var inputVal = (RefCalcEnum)value;

                    if (inputVal == RefCalcEnum.MANUAL)
                    {
                        return Visibility.Visible;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return reval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    
}
