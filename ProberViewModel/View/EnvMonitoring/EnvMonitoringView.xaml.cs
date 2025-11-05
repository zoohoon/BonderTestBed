using EnvMonitoring;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VirtualKeyboardControl;

namespace ProberViewModel.View.EnvMonitoring
{
    /// <summary>
    /// EnvMonitoringView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EnvMonitoringView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("BF53C480-8E52-4228-80E4-449E62EA8AA7");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public EnvMonitoringView()
        {
            InitializeComponent();
        }
    }
    public class MyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values != null)
            {
                int idx = -1;
                //if(values[0] is int)
                {
                    // temp value
                    System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)values[0];
                    tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 10);
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                }
                if(values[1] is int)
                {
                    // module index
                    idx = (int)values[1];
                }
                //EnvMonitoringHub envMonitoringHub = new EnvMonitoringHub();
                //if(envMonitoringHub.SensorModules.Find(a => a.ModuleIndex == idx))
                //{

                //}
                //EnvMonitoringManager envMonitoringManager = new EnvMonitoringManager();
                //envMonitoringManager.EnvMonitoringHubs.
            }
            //{
            //    WaferAlignInfomation info = new WaferAlignInfomation();

            //    if (values[1] != DependencyProperty.UnsetValue) info.DieSizeX = Double.Parse(values[1].ToString());
            //    if (values[2] != DependencyProperty.UnsetValue) info.DieSizeX = Double.Parse(values[2].ToString());

            //    return info;
            //}
            //else
            //{
            //    return null;
            //}



            return values.Clone();
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }        
    }
    public class MultiBindingParamConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
