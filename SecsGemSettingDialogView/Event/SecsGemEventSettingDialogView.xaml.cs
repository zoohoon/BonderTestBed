using SecsGemSettingDialogVM;
using System.Windows.Controls;

namespace UcSecsGemSettingDialog
{
    /// <summary>
    /// SecsGemEventSettingDialogView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SecsGemEventSettingDialogView : UserControl
    {
        public SecsGemEventSettingDialogView()
        {
            if(this.DataContext == null)
            {
                SecsGemEventSettingDialogViewModel viewmodel = new SecsGemEventSettingDialogViewModel();
                this.DataContext = viewmodel;
            }
            InitializeComponent();
        }
    }
}
