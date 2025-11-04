
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace ProberInterfaces
{

    public interface ICameraDescriptor : INotifyPropertyChanged 
    {
        //Element<ObservableCollection<ICameraParameter>> CameraParams { get; set; }
        ObservableCollection<ICamera> Cams { get; set; }
    }
}
