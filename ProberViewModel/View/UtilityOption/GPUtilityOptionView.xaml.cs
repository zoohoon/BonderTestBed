namespace ProberViewModel.View.UtilityOption
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;
    using ProberInterfaces;

    /// <summary>
    /// GPUtilityOptionView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GPUtilityOptionView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("386e667d-046c-415a-b383-12c40902e5cb");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public GPUtilityOptionView()
        {
            InitializeComponent();
        }
    }

    #region <remarks> Converter </remarks>
    public class CassetteLockEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool flag = (bool)value;
                return !flag;
            }
            else
            {
                return true;
            }
            
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    #endregion
}
