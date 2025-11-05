using System;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RelayCommandBase;
using System.Windows.Input;

namespace NoticeDialog
{
    using LogModule;

    /// <summary>
    /// Result Map Upload 실패 Dialog에 대한 View model
    /// </summary>
    public class NoticeDialogViewModel : INotifyPropertyChanged
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private String _Name;
        public String Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }
        public delegate void ShowDetailCallBack();
        public ShowDetailCallBack CustomCallBack { get; set; }
       
        private AsyncCommand<object> _BtnCloseCmd;
        public ICommand BtnClose
        {
            get
            {
                if (null == _BtnCloseCmd)
                    _BtnCloseCmd = new AsyncCommand<object>(CloseCmdProc);

                return _BtnCloseCmd;
            }
        }

        private Task CloseCmdProc(object obj)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Window window = obj as Window;
                    if (window != null)
                    {
                        window.Hide();
                    }
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private AsyncCommand<object> _BtnDetail;
        public ICommand BtnDetail
        {
            get
            {
                if (null == _BtnDetail) 
                    _BtnDetail = new AsyncCommand<object>(ShowManualUpladDlg);

                return _BtnDetail;
            }
        }

        private async Task ShowManualUpladDlg(object obj)
        {
            try
            {
                await Task.Run(() =>
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        Window window = obj as Window;
                        if (window != null)
                        {
                            window.Hide();
                        }
                        CustomCallBack();                        
                    }));
                });              
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
    }

}