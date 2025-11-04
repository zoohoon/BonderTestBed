using ProberInterfaces;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace SoakingSettingView
{
    /// <summary>
    /// UcSoakingStep.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcSoakingStep : UserControl,IMainScreenView
    {
        public UcSoakingStep()
        {
            InitializeComponent();
            this.IsVisibleChanged += UcSoakingStep_IsVisibleChanged;
        }

        private void UcSoakingStep_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.DataContext is ProberViewModel.UcSoakingStepViewModel v)
            {
                v.VisibleChanged(this.IsVisible);
            }
        }

        readonly Guid _ViewGUID = new Guid("604B6B39-A3C8-4B58-AB74-9D9FD1013DAE");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListView))
            {
                return;
            }

            ListView listView = sender as ListView;
            if (listView?.SelectedItem != null)
            {
                listView.Dispatcher.BeginInvoke((Action)(() =>
                {
                    listView.UpdateLayout();
                    if (listView.SelectedItem != null)
                        listView.ScrollIntoView(listView.SelectedItem);
                }));
            }
        }
    }

    #region Value Converter
    public class ValueOffsetConverter : MarkupExtension, IValueConverter
    {
        private static ValueOffsetConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ret = System.Convert.ToDouble(value) + System.Convert.ToDouble(parameter);
            return (ret < 0) ? 0 : ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new ValueOffsetConverter());
        }
    }
    #endregion
}
