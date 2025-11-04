using System.Threading.Tasks;

namespace ProberInterfaces.PnpSetup
{
    using ProberErrorCode;

    public interface ISetup 
    {
        Task<EventCodeEnum> InitSetup();
        
    }
}
