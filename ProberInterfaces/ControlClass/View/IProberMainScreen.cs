using System;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    using ProberErrorCode;
    using System.ComponentModel;

    public interface IScreenGUID
    {
        Guid ScreenGUID { get; }
    }

    public interface IParamScrollingViewModel
    {
        EventCodeEnum UpProc();
        EventCodeEnum DownProc();
    }

    public interface ISecurityViewModel
    {
        bool CheckSecurityPassword(string password);
    }

    public interface IUpDownBtnNoneVisible { }

    public interface IHasCameraControl
    {
        string CameraPosition { get; set; }
        string CameraLookDirection { get; set; }
        string CameraUpDirection { get; set; }
    }

    public interface IResourceProvider
    {

    }

    public interface IViewModel : IScreenGUID
    {
    }

    public interface IView : IFactoryModule, IResourceProvider, IScreenGUID
    {
    }

    public interface IMainScreenView : IView
    {
    }

    public interface IMainScreenViewModel : IFactoryModule, INotifyPropertyChanged, IModule, IViewModel
    {
        Task<EventCodeEnum> InitViewModel();
        Task<EventCodeEnum> PageSwitched(object parameter = null);
        Task<EventCodeEnum> Cleanup(object parameter = null);
        //Task<EventCodeEnum> InitViewModel(object parameter = null);
        Task<EventCodeEnum> DeInitViewModel(object parameter = null);
    }

    public interface ISettingTemplateViewModel
    {
        bool SettingNameIsDifferent();
    }
    public interface IHasClock
    {
        DateTime DateTimeStr { get; }
    }
}

