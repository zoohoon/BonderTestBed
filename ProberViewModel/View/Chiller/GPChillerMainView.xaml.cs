using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProberViewModel.View.Chiller
{
    using ProberInterfaces;
    using System.Globalization;

    /// <summary>
    /// Interaction logic for GPChillerMainView.xaml
    /// </summary>
    public partial class GPChillerMainView : UserControl , IMainScreenView
    {
        public GPChillerMainView()
        {
            InitializeComponent();
        }
        private Autofac.IContainer Container { get; set; }

        readonly Guid _ViewGUID = new Guid("7797A3B8-5CF5-FCCD-22BB-F612618C4B34");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }

    public class CommConnectedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if(value is bool)
            {
                bool isconnect = (bool)value;
                if (isconnect)
                    return "ON";
                else
                    return "OFF";
            }
            return "OFF";
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
