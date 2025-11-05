namespace StageCommunicationModule.CallBack
{
    using ProberInterfaces;
    using ProberInterfaces.ServiceHost;
    using System.ServiceModel;

    [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ImageDispHostCallback : IImageDispHostCallback, IFactoryModule
    {
        #region //..Property

        private InstanceContext _InstanceContext;

        public InstanceContext InstanceContext
        {
            get { return _InstanceContext; }
            set { _InstanceContext = value; }
        }

        #endregion

        #region //..Creator & Init
        public ImageDispHostCallback()
        {
            InstanceContext = new InstanceContext(this);
        }

        public InstanceContext GetInstanceContext()
        {
            return InstanceContext;
        }
        #endregion

        #region //..Method
        public void DisConnect()
        {
            this.StageCommunicationManager().DisConnectdispService();
        }
        #endregion
    }
}
