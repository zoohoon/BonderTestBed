using System.Threading.Tasks;

namespace ProberInterfaces.DialogControl
{
    public interface ITempDisplayDialogService : IFactoryModule, IModule
    {
        bool IsShowing { get; }
        void TurnOnPossibleFlag();
        Task<bool> ShowDialog();
        Task CloseDialog();
    }
}
