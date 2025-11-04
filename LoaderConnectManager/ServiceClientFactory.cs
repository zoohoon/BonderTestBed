/* Service Client Factory
 * Author: Michael Gerety, Senior Consultant, Tallan, Inc.
 * Description: A generic service client factory that automatically
 *              resets faulted channels for WCF services that have
 *              endpoints and behaviors defined in web/app.config files.
 */

using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace LoaderCommunicationModule
{
    using ProberInterfaces.Proxies;
    /// <summary>
    /// Singleton factory class for WCF Services.
    /// Creates service clients based on interface type and automatically
    /// resets faulted channels.
    /// </summary>
    public class ClientFactory
    {
        private readonly Dictionary<IProberProxy, object> factories;
        private static ClientFactory instance;

        private ClientFactory()
        {
            factories = new Dictionary<IProberProxy, object>();
        }



        /// <summary>
        /// Retrieves a service client for the interface specified in generic parameter.
        /// </summary>
        /// <typeparam name="T">Interface type to use for Service Client creation.</typeparam>
        /// <returns>Service client instance for specified interface.</returns>
        public T GetClient<T>()
        {
            
            return default(T);
        }



       

        /// <summary>
        /// Event handler for ClientBase.Faulted event.
        /// </summary>
        /// <typeparam name="T">Interface type of service</typeparam>
        /// <param name="sender">ClientBase instance</param>
        /// <param name="e">Event Args</param>
        private void Channel_Faulted<T>(object sender, EventArgs e)
        {
            ((ICommunicationObject)sender).Abort();
        }

        

        /// <summary>
        /// Returns the singleton instance of ServiceClientFactory.
        /// </summary>
        /// <returns>Singleton instance of ServiceClientFactory</returns>
        public static ClientFactory GetFactory()
        {
            if (instance == null)
            {
                instance = new ClientFactory();
            }
            return instance;
        }
    }
}
