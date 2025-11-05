using ProberInterfaces.Command;
using ProberInterfaces.State;
using System.Threading.Tasks;

namespace ProberInterfaces.CardChange
{
    public interface ICardChangeState : IFactoryModule, IInnerState
    {
        Task<int> ExecuteCC();
        int InitExecute();
        CardChangeModuleStateEnum GetState();

        bool CanExecute(IProbeCommandToken token);
        
    }
    public interface IGPCardChangeState : IFactoryModule, IInnerState
    {
        CardChangeModuleStateEnum GetState();
    }
}
