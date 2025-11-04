using RelayCommandBase;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProberInterfaces.ControlClass.ViewModel
{
    public interface IDeviceChangeVM
    {
        string SearchStr { get; set; }
        bool IsSearchDataClearButtonVisible { get; set; }
        bool IsCanChangeDevice { get; set; }
        DeviceInfo ShowingDevice { get; set; }
        ObservableCollection<DeviceInfo> ShowingDeviceInfoCollection { get; set; }
        DeviceInfo SelectedDeviceInfo { get; set; }
        ICommand SearchTBClickCommand { get; }
        ICommand ClearSearchDataCommand { get; }
        ICommand PageSwitchingCommand { get; }
        ICommand RefreshDevListCommand { get; }

        IAsyncCommand ChangeDeviceCommand { get; }
        IAsyncCommand CreateNewDeviceCommand { get; }
        IAsyncCommand SaveAsDeviceCommand { get; }
        IAsyncCommand DeleteDeviceCommand { get; }

        Task GetDevList(bool isPageSwiching = false);

        Task SetShowingDeviceInfoCollectio(ObservableCollection<DeviceInfo> collection);
        //Task GetParamFromDevice(DeviceInfo device);
        //Task GetParamFromDevice(DeviceInfo device);
        //IAsyncCommand<DeviceInfo> GetParamFromDeviceCommand { get; }



        IAsyncCommand GetParamFromDeviceCommand { get; }

        DeviceChangeDataDescription GetDeviceChangeInfo();


        Task ChangeDeviceFunc();
        Task ChangeDeviceFunc(string deviceName);
        Task CreateNewDeviceFunc();
        Task SaveAsDeviceFunc();
        Task DeleteDeviceFunc();
        Task GetParamFromDevice(DeviceInfo device);
    }
}
