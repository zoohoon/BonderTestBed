using ProberErrorCode;
using System.Threading.Tasks;

namespace ProberInterfaces.PolishWafer
{
    public interface IIPolishWaferSetupViewModel
    {
        Task<EventCodeEnum> PageSwitched();
    }
}
