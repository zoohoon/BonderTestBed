namespace EnvControlWindow.GP
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    public class GpEnvControlMainWindowViewModel :  INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ..Property

        private ObservableCollection<int> _StageList
             = new ObservableCollection<int>();
        public ObservableCollection<int> StageList
        {
            get { return _StageList; }
            set
            {
                if (value != _StageList)
                {
                    _StageList = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public GpEnvControlMainWindowViewModel()
        {
            StageList = new ObservableCollection<int>();
            for (int index = 0; index <= 12; index++)
            {
                StageList.Add(index);
            }
        }

    }

}
