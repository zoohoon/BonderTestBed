using System;

namespace RemoteServiceProxy
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Temperature.DryAir;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DryAirServiceDirectProxy : ClientBase<IDryAirService>, IFactoryModule
    {
        int stageIdx = -1;
        public DryAirServiceDirectProxy(string ip, int port)
            : base(
            new ServiceEndpoint(ContractDescription.GetContract(typeof(IDryAirService)),
            new NetTcpBinding()
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                MaxBufferPoolSize = 524288,
                MaxReceivedMessageSize = 50000000,
                Security = new NetTcpSecurity() { Mode = SecurityMode.None }
            },
            new EndpointAddress($"net.tcp://{ip}:{port}/POS/dryairpipe")))
        { 
            stageIdx = this.LoaderController().GetChuckIndex();
        }
        public void InitService()
        {
            try
            {
                Channel.InitService();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool IsOpened()
        {
            return (State == CommunicationState.Opened || State == CommunicationState.Created);
        }
        public byte[] GetDryAirParam()
        {
            byte[] retVal = null;
            try
            {
                if (IsOpened())
                    retVal = Channel.GetDryAirParam(stageIdx);
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1)
        {
            try
            {
                if (IsOpened())
                    return Channel.DryAirForProber(value, dryairType, this.stageIdx);
                else
                    return EventCodeEnum.DRYAIR_REMOTE_CHANNEL_FAULT;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.DRYAIR_SETVALUE_ERROR;
            }
        }

        public int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageindex = -1)
        {
            value = false;
            try
            {
                if (IsOpened())
                    return Channel.GetLeakSensor(out value, leakSensorIndex, this.stageIdx);
                else
                    return -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return -1;
            }
        }
    }
}
