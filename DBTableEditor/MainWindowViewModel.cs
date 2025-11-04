namespace DBTableEditor
{
    using DBManagerModule;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public TableEditTabViewModel TableEditTabVM { get; set; }
        public UpdateTabViewModel UpdateTabVM { get; set; }

        public MainWindowViewModel()
        {
            DBManager.Open();

            TableEditTabVM = new TableEditTabViewModel();
            UpdateTabVM = new UpdateTabViewModel();
        }
    }
}
