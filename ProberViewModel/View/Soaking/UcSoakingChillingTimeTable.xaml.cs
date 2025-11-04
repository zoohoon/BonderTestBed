using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace SoakingSettingView
{
    /// <summary>
    /// UcSoakingStep.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcSoakingChillingTimeTable : UserControl,IMainScreenView
    {
        public UcSoakingChillingTimeTable()
        {
            InitializeComponent();
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
}
