namespace ProberInterfaces.EnvControl.Parameter
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    public class DewPointSysParameter : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
