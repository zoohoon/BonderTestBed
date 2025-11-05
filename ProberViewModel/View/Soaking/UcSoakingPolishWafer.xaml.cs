using ProberInterfaces;
using ProberViewModel;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SoakingSettingView
{
    /// <summary>
    /// UcSoakingPolishWafer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcSoakingPolishWafer : UserControl,IMainScreenView
    {
        public UcSoakingPolishWafer()
        {
            InitializeComponent();
        }
        readonly Guid _ViewGUID = new Guid("AD94A2F1-3059-4CB7-AFA0-DE15D43A2757");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        private void OnCboSelectPolishWaferChecked(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (SelectableObject<string> cbObject in CboSelectPolishWafer.Items)
            {
                if (cbObject.IsSelected)
                    sb.AppendFormat("{0}, ", cbObject.ObjectData.ToString());
            }

            CboTextSelectPolishWafer.Text = sb.ToString().Trim().TrimEnd(',');
        }
        
        /// <summary>
        /// ComboBox 선택을 취소 시키는 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxSelectionChanged_AlwaysNull(object sender, SelectionChangedEventArgs e)
        {
            if(sender is ComboBox c)
            {
                if (c.SelectedItem != null)
                {
                    c.SelectedItem = null;
                }
            }
        }
    }
}
