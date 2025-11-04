using LogModule;
using SecsGemSettingDialogVM;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SecsGemSettingDlg
{
    public partial class SecsGemSettingDialog : Window, INotifyPropertyChanged
    {
        private SecsGemSettingDialogViewModel _ViewModel = new SecsGemSettingDialogViewModel();
        public SecsGemSettingDialogViewModel ViewModel
        {
            get { return _ViewModel; }
            set
            {
                if(_ViewModel != value)
                {
                    _ViewModel = value;
                    RaisePropertyChanged();
                }
            }
        }
        public SecsGemSettingDialog(bool issortcut = true)
        {
            InitializeComponent();
            ViewModel = new SecsGemSettingDialogViewModel(issortcut);
            this.DataContext = ViewModel;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                ViewModel = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}