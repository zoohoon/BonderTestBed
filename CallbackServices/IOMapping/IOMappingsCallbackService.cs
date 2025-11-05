using System.Linq;

namespace CallbackServices
{
    using LogModule;
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    public class IOMappingsCallbackService : IIOMappingsCallback, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
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

        private ObservableCollection<IOPortDescripter<bool>> _Inputs = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> Inputs
        {
            get { return _Inputs; }
            set
            {
                if (value != _Inputs)
                {
                    _Inputs = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _Outputs = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> Outputs
        {
            get { return _Outputs; }
            set
            {
                if (value != _Outputs)
                {
                    _Outputs = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region //..Creator & Init
        public IOMappingsCallbackService()
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
        public void OnPortStateUpdated(IOPortDescripter<bool> port)
        {
            LoggerManager.Debug($"OnPortStateUpdated({port.Key.Value}): Port state has been updated.");
            var io = Inputs.FirstOrDefault(i => (i.Key.Value == port.Key.Value));
            if (io != null) io.Value = port.Value;

            var outport = Outputs.FirstOrDefault(o => (o.Key.Value == port.Key.Value));
            if (outport != null) outport.Value = port.Value;
        }

        #endregion
    }
}
