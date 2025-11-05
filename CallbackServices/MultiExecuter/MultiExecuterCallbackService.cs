namespace CallbackServices
{
    using LogModule;
    using ProberInterfaces;
    using LoaderBase.Communication;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using Autofac;


    public class MultiExecuterCallbackService : IMultiExecuterCallback, INotifyPropertyChanged, IFactoryModule
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property

        private InstanceContext _InstanceContext;


        public InstanceContext InstanceContext
        {
            get { return _InstanceContext; }
            set { _InstanceContext = value; }
        }


        #endregion

        #region //..Creator & Init
        public MultiExecuterCallbackService()
        {
            InstanceContext = new InstanceContext(this);

        }

        public InstanceContext GetInstanceContext()
        {
            return InstanceContext;
        }
        #endregion

        #region //..Method
        public bool IsServiceAvailable()
        {
            return true;
        }

        public void CellLoaded()
        {

            LoggerManager.Debug($"CellLoaded(): Wafer state has been updated.");
        }

        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();

        //public void InfoSend(string ip, string pc_name, string drivename, int usagespace, int availablespace, int totalspace, int percent)
        //{
        //    _LoaderCommunicationManager.GetDiskInfo(ip, pc_name, drivename, usagespace, availablespace, totalspace, percent); //UI Alarm
        //}

        public void MessageSend(int cellnum, string pc_name, string drivename)
        {
            int lunchernum = _LoaderCommunicationManager.FindLuncherIndex(cellnum);
            _LoaderCommunicationManager.DiskAlarm(lunchernum, pc_name, drivename);
        }

        public void DisConnect(int cellnum)
        {
            int lunchernum = _LoaderCommunicationManager.FindLuncherIndex(cellnum);
            _LoaderCommunicationManager.DisConnectLauncher(lunchernum);
        }

        public void InfoSend(int cellnum, string pc_name, string drivename, string usagespace, string availablespace, string totalspace, string percent)
        {

            int lunchernum = _LoaderCommunicationManager.FindLuncherIndex(cellnum);
            _LoaderCommunicationManager.SetDiskInfo(lunchernum, pc_name, drivename, usagespace, availablespace, totalspace, percent); //UI Alarm
        }

        #endregion
    }
}
