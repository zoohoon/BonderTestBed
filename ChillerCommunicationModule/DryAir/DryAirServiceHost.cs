namespace ChillerControlModule.DryAir
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Temperature.DryAir;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DryAirServiceHost : IDryAirService
    {
        public IDryAirManager Manager {get;set;}
        // Cell과 통신하는 Host입니다.
        private ServiceHost CommanderServiceHost = null;

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ServiceCreateHelper.CreateServiceHost<IDryAirService>
                    (this, "localhost", 0, ServicePort.DryAirServicePort, "dryairpipe", CommanderServiceHost);
                //retVal = InitServiceHost("localhost", ServicePort.DryAirServicePort);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InitConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// port : 8423
        /// </summary>
        public EventCodeEnum InitServiceHost(string ip, int port)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ServiceMetadataBehavior serviceMetadataBehavior = null;
                ServiceDebugBehavior debugBehavior = null;
                string localURI = $"net.tcp://{ip}:{port}/dryairpipe";

                var netTcpBinding = new NetTcpBinding()
                {
                    MaxBufferPoolSize = 2147483647,
                    MaxBufferSize = 2147483647,
                    MaxReceivedMessageSize = 2147483647,
                    SendTimeout = TimeSpan.MaxValue,
                    ReceiveTimeout = TimeSpan.MaxValue
                };

                netTcpBinding.Security.Mode = SecurityMode.None;
                CommanderServiceHost = new ServiceHost(this);
                CommanderServiceHost.AddServiceEndpoint(typeof(IDryAirService), netTcpBinding, localURI);

                debugBehavior = CommanderServiceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
                if (debugBehavior != null)
                {
                    debugBehavior.IncludeExceptionDetailInFaults = true;
                }

                serviceMetadataBehavior = CommanderServiceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                if (serviceMetadataBehavior == null)
                    serviceMetadataBehavior = new ServiceMetadataBehavior();

                serviceMetadataBehavior.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                CommanderServiceHost.Description.Behaviors.Add(serviceMetadataBehavior);

                CommanderServiceHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                    MetadataExchangeBindings.CreateMexTcpBinding(),
                    $"{localURI}/mex"
                    );

                CommanderServiceHost.Open();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public void InitService()
        {
            return;
        }

        public EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1)
        {
            return Manager.DryAirForProber(value, dryairType, stageIndex);
        }
        public bool GetDryAirState(EnumDryAirType dryairType, int stageIndex = -1)
        {
            return Manager.GetDryAirState(dryairType, stageIndex);
        }
        public int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageindex = -1)
        {
            return Manager.GetLeakSensor(out value, leakSensorIndex, stageindex);
        }
        public byte[] GetDryAirParam(int stageindex = -1)
        {
            return Manager.GetDryAirParam(stageindex);
        }
    }
}
