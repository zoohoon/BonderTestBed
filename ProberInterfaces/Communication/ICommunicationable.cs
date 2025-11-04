using ProberErrorCode;
using ProberInterfaces.Enum;

namespace ProberInterfaces
{
    public interface ICommunicationable
    {
        EnumCommunicationState CommunicationState { get; set; }
    }

    //public interface IExternalControllable : ICommunicationable
    //{
    //    EventCodeEnum IsControlAvailableState(out string errorlog);
    //}

}
