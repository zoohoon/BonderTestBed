namespace StageCommunicationModule.CallBack
{
    using ProberInterfaces;
    using System.ServiceModel;

    [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DelegateEventHostCallback : IDelegateEventHostCallback , IFactoryModule
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
        public DelegateEventHostCallback()
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
            this.StageCommunicationManager().DisConnectDelegateEventService();
        }


        public void WaitCancelDialogCancelEvent()
        {
            // TODO : CANCEL
            //this.MetroDialogManager().CancelDialog();
            //this.WaitCancelDialogService().CancelDialog();
        }

        #endregion

    }
}
