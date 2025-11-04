using LogModule;
using System;
using System.ServiceModel;

namespace ProberInterfaces.Proxies
{
    public static class ProxyHelper
    {
        //private static CommunicationState GetState(IProberProxy proxy)
        //{
        //    return (proxy as ICommunicationObject).State;
        //}

        public static bool IsOpened(CommunicationState state)
        {
            bool retVal = false;

            try
            {
                if (state == CommunicationState.Opened | state == CommunicationState.Created)
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        //private ProxyManager ProxyManager { get; set; }

        //public Proxies(ProxyManager proxyManager)
        //{
        //    this.ProxyManager = proxyManager;
        //}
    }
}
