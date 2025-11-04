using ProberErrorCode;
using System.Collections.Generic;
using System.ServiceModel;

namespace ProberInterfaces
{
    [ServiceContract(CallbackContract = typeof(IIOMappingsCallback), Namespace = "http://ProberInterfaces")]
   
    public interface IIOMappingsParameter
    {
        InputPortDefinitions Inputs { get; }
        OutputPortDefinitions Outputs { get; }
        RemoteInputPortDefinitions RemoteInputs { get; }
        RemoteOutputPortDefinitions RemoteOutputs { get; }

        [OperationContract]
        List<IOPortDescripter<bool>> GetInputPorts();
        [OperationContract]
        List<IOPortDescripter<bool>> GetOutputPorts();
        [OperationContract]
        IOPortDescripter<bool> GetPortStatus(string key);
        [OperationContract]
        void SetPortStateAs(IOPortDescripter<bool> port, bool value);
        [OperationContract]
        void InitService();
        [OperationContract]
        bool IsServiceAvailable();

        [OperationContract]
        void SetForcedIO(IOPortDescripter<bool> ioport, bool IsForced, bool ForecedValue);
    }

    public interface IIOManager: IFactoryModule, IModule
    {
        IIOService IOServ { get; set; }
        IIOMappingsParameter IO { get; }
        EventCodeEnum InitIOStates();
        void DeInitService();
    }

    public interface IIOMappingsCallback
    {
        [OperationContract]
        void OnPortStateUpdated(IOPortDescripter<bool> port);
        [OperationContract]
        bool IsServiceAvailable();
    }
}
