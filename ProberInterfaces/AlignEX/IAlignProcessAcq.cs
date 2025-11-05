using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces.AlignEX
{
    public interface IAlignProcessAcq : INotifyPropertyChanged
    {

    }

    public abstract class AlignProcessAcqBase : IAlignProcessAcq
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
