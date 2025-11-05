using System.Linq;

namespace CallbackServices
{
    using LogModule;
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    public class MotionManagerCallbackService : IMotionManagerCallback, INotifyPropertyChanged

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

        private ObservableCollection<ProbeAxisObject> _Axes = new ObservableCollection<ProbeAxisObject>();
        public ObservableCollection<ProbeAxisObject> Axes
        {
            get { return _Axes; }
            set
            {
                if (value != _Axes)
                {
                    _Axes = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region //..Creator & Init
        public MotionManagerCallbackService()
        {
            InstanceContext = new InstanceContext(this);
        }

        public InstanceContext GetInstanceContext()
        {
            return InstanceContext;
        }

        #endregion

        #region //.. Method

        public bool IsServiceAvailable()
        {
            return true;
        }

        public void OnAxisStateUpdated(ProbeAxisObject axis)
        {

            ProbeAxisObject updateAxis = Axes.FirstOrDefault(ax => ax.AxisType.Value == axis.AxisType.Value);
            if (updateAxis != null)
            {
                updateAxis.Status = axis.Status;
                LoggerManager.Debug($"StateUpdated({axis.Label}): Axis state  has been updated.");
            }
            else
            {
                LoggerManager.Debug($"StateUpdated({axis.Label}): Axis does not exist on list.");
            }
        }
        #endregion

    }
}
