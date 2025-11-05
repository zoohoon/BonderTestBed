using System.ServiceModel.Description;
using System.ServiceModel;

namespace IOStateProxy
{
    using LogModule;
    using ProberInterfaces;
    public class IOStateProxy: ClientBase<IIOManager>
    {

        //public IOStateProxy():
        //    base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IIOStates)),
        //new NetTcpBinding(),
        //new EndpointAddress("net.tcp://localhost:9000/POS/IIOStatesService")))
        //{

        //}
        //public IOStateProxy(int port):
        //    base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IIOStates)),
        //        new NetTcpBinding(),
        //        new EndpointAddress($"net.tcp://localhost:{port}/POS/IIOStatesService")))
        //{
        //    Log.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
        //}
        public IOStateProxy() :
           base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IIOManager)),
       new NetNamedPipeBinding(),
       new EndpointAddress("net.pipe://localhost/POS/IIOStatesService")))
        {

        }
        public IOStateProxy(int port) :
            base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IIOManager)),
                new NetNamedPipeBinding() { MaxBufferSize = 1000000, MaxBufferPoolSize = 524288, MaxReceivedMessageSize = 1000000},
                new EndpointAddress($"net.pipe://localhost/POS/IIOStatesService")))
        {

            LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
        }
        //IIOService IOServ { [OperationContract]get; set; }
        //IIOMappingsParameter IO { [OperationContract]get; }
        public IIOService GetIOService()
        {
            return Channel.IOServ;
        }
        public IIOMappingsParameter GetIOMappingsParameter()
        {
            return Channel.IO;
        }
    }
    
}
