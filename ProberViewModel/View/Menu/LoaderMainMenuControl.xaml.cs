using MaterialDesignThemes.Wpf.Converters;
using ProberInterfaces;
using ProberInterfaces.Loader;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LoaderMainMenuControl
{
    /// <summary>
    /// LoaderMainMenuControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderMainMenuControl : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("fe9417f5-9c54-40bc-b73c-6d415ea1c398");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public LoaderMainMenuControl()
        {
            InitializeComponent();
        }

        private void Menu_LostFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = (ILoaderMainMenuVM)DataContext;

            if (viewModel.MenuCloseclickCommand.CanExecute(null))
            {
                viewModel.MenuCloseclickCommand.Execute(null);
            }
        }

        //private void GridMenu_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    //var viewModel = (ILoaderMainMenuVM)DataContext;
        //    //if (viewModel.MenuCloseclickCommand.CanExecute(null))
        //    //{
        //    //    //viewModel.MenuCloseclickCommand.Execute(null);
        //    //}
            
        //}
    }

    public sealed class MathMultipleConverter : IMultiValueConverter
    {
        public MathOperation Operation { get; set; }

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.Length < 2 || value[0] == null || value[1] == null) return Binding.DoNothing;

            if (!double.TryParse(value[0].ToString(), out double value1) || !double.TryParse(value[1].ToString(), out double value2))
                return 0;

            switch (Operation)
            {
                default:
                    // (case MathOperation.Add:)
                    return value1 + value2;
                case MathOperation.Divide:
                    return value1 / value2;
                case MathOperation.Multiply:
                    return value1 * value2;
                case MathOperation.Subtract:
                    return value1 - value2;
            }

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
