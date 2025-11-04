using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace WaferHandlingRecoveryControl
{
    using ProberInterfaces;
    using System.Globalization;

    /// <summary>
    /// UcWaferHandlingRecovery.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcWaferHandlingRecovery : UserControl, IMainScreenView
    {
        //readonly string _ViewModelType = "IWaferHandlingRecoveryViewModel";
        //public string ViewModelType { get { return _ViewModelType; } }

        readonly Guid _ViewGUID = new Guid("A0B5736F-7503-438F-8B9F-EBDA8A09DBD6");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public UcWaferHandlingRecovery()
        {
            InitializeComponent();
        }
    }

    public class MulitCommandParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
