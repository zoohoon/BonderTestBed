using System.ComponentModel;

namespace ProberInterfaces
{
    public interface IFilterPanelViewModel : INotifyPropertyChanged, IFactoryModule
    {
        void RequestEnableMode();
        void RequestDisableMode();
        bool IsEnable { get; set; }
    }
}
