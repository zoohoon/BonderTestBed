using LogModule;
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

namespace ProberViewModel.View.Wafer
{
    /// <summary>
    /// Interaction logic for WaferMapMaker.xaml
    /// </summary>
    public partial class WaferMapMaker : UserControl, IMainScreenView
    {
        public WaferMapMaker()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("5CCAD8EF-1255-F3CE-5118-C245943F1993");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                
                
            });
            //MotionJog.IsEnabled = true;

        }

    }


    public class NotchFlipConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            
            double currangle = 0;

            if (values[0] is double)
            {
                double.TryParse(values[0].ToString(), out currangle);

                if(values[1] is DispFlipEnum && values[2] is DispFlipEnum)
                {
                    if ((DispFlipEnum)values[1] == DispFlipEnum.FLIP &&
                        (DispFlipEnum)values[2] == DispFlipEnum.FLIP)
                    {
                        currangle = (double)values[0] + 180;
                        if (currangle >= 360)
                        {
                            currangle = currangle - 360;
                        }

                    }
                }
            }
            return currangle.ToString();
        }
      
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



}
