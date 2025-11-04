using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace WaferHandlingRecoveryControl.WaferHandleExplorerUC
{
    using ProberInterfaces;
    using System.Globalization;

    /// <summary>
    /// UcSlotHolderIcon.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcSlotHolderIcon : UserControl
    {
        public UcSlotHolderIcon()
        {
            InitializeComponent();
        }

        private void icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            icon.Opacity = 0.5;
        }

        private void icon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            icon.Opacity = 1;
        }

        private void icon_MouseLeave(object sender, MouseEventArgs e)
        {
            icon.Opacity = 1;
        }
    }

    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = Brushes.Transparent;
            try
            {
                if (value is EnumSubsStatus)
                {
                    EnumSubsStatus status = (EnumSubsStatus)value;

                    switch (status)
                    {
                        case EnumSubsStatus.EXIST:
                            brush = Brushes.Brown;
                            break;
                        case EnumSubsStatus.UNKNOWN:
                        case EnumSubsStatus.UNDEFINED:
                        case EnumSubsStatus.NOT_EXIST:
                            brush = Brushes.Transparent;
                            break;
                        default:
                            brush = Brushes.Transparent;
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                throw;
            }

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
