using System;

namespace ProberInterfaces
{
    using LogModule;
    using ProberErrorCode;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public static class ServiceCreateHelper
    {

        public static EventCodeEnum CreateServiceHost<T>(object singletoninstance, string ip, int port, int portoffset, string address, System.ServiceModel.ServiceHost servicehost,
            TimeSpan? sendtimeout = null,
            TimeSpan? receivetimeout = null,
            TimeSpan? opentimeout = null,
            TimeSpan? closetimeout = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Type type = typeof(T);

                var serviceHostOverTCP = new System.ServiceModel.ServiceHost(singletoninstance, new Uri[] { new Uri($"net.tcp://{ip}:{port + portoffset}/POS") });

                NetTcpBinding tcpBinding = new NetTcpBinding();

                if (sendtimeout == null)
                {
                    tcpBinding.SendTimeout = new TimeSpan(0, 5, 0);
                }
                else
                {
                    tcpBinding.SendTimeout = (TimeSpan)sendtimeout;
                }

                if (receivetimeout == null)
                {
                    tcpBinding.ReceiveTimeout = TimeSpan.MaxValue;
                }
                else
                {
                    tcpBinding.ReceiveTimeout = (TimeSpan)receivetimeout;
                }

                if (opentimeout == null)
                {
                    tcpBinding.OpenTimeout = new TimeSpan(0, 10, 0);
                }
                else
                {
                    tcpBinding.OpenTimeout = (TimeSpan)opentimeout;
                }

                if (closetimeout == null)
                {
                    tcpBinding.CloseTimeout = new TimeSpan(0, 10, 0);
                }
                else
                {
                    tcpBinding.OpenTimeout = (TimeSpan)closetimeout;
                }

                tcpBinding.MaxBufferPoolSize = 524288;
                tcpBinding.MaxReceivedMessageSize = 2147483646;



                tcpBinding.ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true };



                tcpBinding.Security.Mode = SecurityMode.None;



                serviceHostOverTCP.AddServiceEndpoint(type, tcpBinding, address);

                ServiceDebugBehavior debug = serviceHostOverTCP.Description.Behaviors.Find<ServiceDebugBehavior>();

                // if not found - add behavior with setting turned on 
                if (debug == null)
                {
                    serviceHostOverTCP.Description.Behaviors.Add(
                         new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
                }
                else
                {
                    // make sure setting is turned ON
                    if (!debug.IncludeExceptionDetailInFaults)
                    {
                        debug.IncludeExceptionDetailInFaults = true;
                    }
                }
                if ((serviceHostOverTCP.State == CommunicationState.Closed) || (serviceHostOverTCP.State == CommunicationState.Faulted)
                    || (serviceHostOverTCP.State == CommunicationState.Created))
                {
                    serviceHostOverTCP.Open();

                    servicehost = serviceHostOverTCP;

                    LoggerManager.Debug("Service started. Available in following endpoints");

                    foreach (var serviceEndpoint in serviceHostOverTCP.Description.Endpoints)
                    {
                        LoggerManager.Debug($"Service End point :{serviceEndpoint.ListenUri.AbsoluteUri}");
                    }

                }
                else {

                    foreach (System.Uri adr in serviceHostOverTCP.BaseAddresses)
                    {
                        LoggerManager.Debug($"Service State: {serviceHostOverTCP.State}. Not available open ServiceHost:{adr.ToString()}");
                    }
                    
                }


                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
