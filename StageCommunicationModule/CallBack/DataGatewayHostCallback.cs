using LogModule;
using ProberInterfaces;
using ProberInterfaces.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace StageCommunicationModule.CallBack
{
    [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DataGatewayHostCallback : IFactoryModule, ILoaderDataGatewayHostCallback
    {
        #region <remarks> Property </remarks>

        private InstanceContext _InstanceContext;

        public InstanceContext InstanceContext
        {
            get { return _InstanceContext; }
            set { _InstanceContext = value; }
        }

        #endregion

        #region  <remarks> Creator & Init </remarks>
        public DataGatewayHostCallback()
        {
            InstanceContext = new InstanceContext(this);
        }

        public InstanceContext GetInstanceContext()
        {
            return InstanceContext;
        }
        #endregion

        #region <remarks> Callback Method </remarks>
        public void DisConnect()
        {
            try
            {
                this.StageCommunicationManager().DisconnectDataGatewayService();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
    }
}
