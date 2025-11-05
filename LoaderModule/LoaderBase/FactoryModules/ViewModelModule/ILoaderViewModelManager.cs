using System;
using System.Threading.Tasks;

namespace LoaderBase.FactoryModules.ViewModelModule
{
    using ProberErrorCode;
    using ProberInterfaces;

    public interface ILoaderViewModelManager : IFactoryModule , IViewModelManager
    {
        ICamera Camera { get; set; }
        IDisplayPort DisplayPort { get; set; }
        void SetContainer(Autofac.IContainer container);
        IMainScreenViewModel GetViewModel(int cellindex);
        void DispHostService_ImageUpdate(ImageBuffer image);
        void RegisteDisplayPort(IDisplayPort displayport);
        void RegisteDisplayPort(Type type, IDisplayPort displayport);
        new EventCodeEnum SetDataContext(object obj);
        void UpdateLoaderAlarmCount();
        //void CloseMainMenu();
        void WaitUIUpdate(string methodName, int timeoutSec = 3);
        Task HomeViewTransition();
        IMainScreenViewModel CurrentVM { get; set; }
    }
}
