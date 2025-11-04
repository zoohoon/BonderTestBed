using ProberErrorCode;
using System.ServiceModel;

namespace ProberInterfaces
{
    [ServiceContract]
    public interface IModule
    {
        [OperationContract]
        void DeInitModule();
        [OperationContract]
        EventCodeEnum InitModule();
        bool Initialized { get; }

    }
}
