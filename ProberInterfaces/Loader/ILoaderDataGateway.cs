using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.Loader
{
    [ServiceContract(CallbackContract = typeof(ILoaderDataGatewayHostCallback))]
    public interface ILoaderDataGateway : IFactoryModule, IModule
    {
        [OperationContract]
        bool IsServiceAvailable();
        [OperationContract]
        void InitService(int chuckId);
        [OperationContract]
        EventCodeEnum NotifyStageAlarm(EventCodeParam noticeCodeInfo);
        EventCodeEnum Disconnect(int index = -1);
        ILoaderDataGatewayHostCallback GetDataGatewayHostClient(int index = -1);
    }

    public interface ILoaderDataGatewayHostCallback
    {
        [OperationContract(IsOneWay = true)]
        void DisConnect();
    }
}
