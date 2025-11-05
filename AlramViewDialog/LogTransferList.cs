using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AlarmViewDialog
{
    class LogTransferList
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private string _LogFilePath;
        public string LogFilePath
        {
            get { return _LogFilePath; }
            set
            {
                if (value != _LogFilePath)
                {
                    _LogFilePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LogFileName;
        public string LogFileName
        {
            get { return _LogFileName; }
            set
            {
                if (value != _LogFileName)
                {
                    _LogFileName = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
