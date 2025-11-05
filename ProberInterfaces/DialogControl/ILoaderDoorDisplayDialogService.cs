using System.Threading.Tasks;

namespace ProberInterfaces.DialogControl
{
    public interface ILoaderDoorDisplayDialogService:ILoaderFactoryModule,IModule
    {
        Task<bool> ShowDialog(string message);
        Task CloseDialog();
    }
    public interface ILoaderParkingDisplayDialogService : ILoaderFactoryModule, IModule
    {
        Task<bool> ShowDialog(string message);
        Task CloseDialog();
    }
}
