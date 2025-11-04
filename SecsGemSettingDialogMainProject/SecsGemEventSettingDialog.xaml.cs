using SecsGemSettingDialogVM;
using System;
using System.Windows;

namespace SecsGemSettingDlg
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SecsGemEventSettingDialog : Window
    {
        private SecsGemEventSettingDialogViewModel _ViewModel { get; set; }

        public SecsGemEventSettingDialog()
        {
            InitializeComponent();
            _ViewModel = new SecsGemEventSettingDialogViewModel();
            this.DataContext = _ViewModel;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _ViewModel = null;
        }
    }
}
