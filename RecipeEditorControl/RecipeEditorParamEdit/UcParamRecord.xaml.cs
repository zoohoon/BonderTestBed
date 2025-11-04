using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace RecipeEditorControl.RecipeEditorParamEdit
{
    using LogModule;
    using ProberInterfaces.State;
    using System.Globalization;
    /// <summary>
    /// UcParamRecord.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcParamRecord : UserControl
    {
        public UcParamRecord()
        {
            InitializeComponent();
        }
    }

    public class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            Brush color = Brushes.Black;
            try
            {

                switch ((ElementStateEnum)value)
                {
                    case ElementStateEnum.DEFAULT:
                        color = Brushes.LightGreen;
                        break;
                    case ElementStateEnum.NEEDSETUP:
                        color = Brushes.IndianRed;
                        break;
                    case ElementStateEnum.UNDEFINED:
                    case ElementStateEnum.DONE:
                        color = Brushes.Black;
                        break;
                    default:
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return color;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
