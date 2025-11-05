using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UcViewer3DViewModel
{
    public class Viewer3DViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

    }
}
