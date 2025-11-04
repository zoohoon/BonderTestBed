using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces.AlignEX
{
    public interface IAlignAcqStep : INotifyPropertyChanged
    {
        ObservableCollection<AlignProcessAcqBase> Acquisitions { get; set; }
    }

    public abstract class AlignAcqStepBase : IAlignAcqStep
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public abstract ObservableCollection<AlignProcessAcqBase> Acquisitions { get; set; }

    }
}
