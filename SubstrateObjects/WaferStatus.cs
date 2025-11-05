using LogModule;
using System;
using System.ComponentModel;

namespace SubstrateObjects
{
    using Newtonsoft.Json;
    using ProberInterfaces;
    [Serializable]
    public abstract class WaferStatusBase : INotifyPropertyChanged, IWaferStatus
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private WaferObject _Wafer;

        public WaferObject Wafer
        {
            get { return _Wafer; }
            protected set
            {
                if (value != _Wafer)
                {
                    _Wafer = value;
                    NotifyPropertyChanged("Wafer");
                }
            }
        }

        public abstract EnumSubsStatus GetState();

        public virtual void SetStatusMissing()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetState()));
        }

        public virtual void SetStatusUnloaded()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetState()));
        }

        public virtual void SetStatusLoaded()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetState()));
        }

        public WaferStatusBase(WaferObject wafer)
        {
            Wafer = wafer;
        }
    }
    [Serializable]
    public class WaferMissingSatus : WaferStatusBase
    {
        public WaferMissingSatus(WaferObject wafer) : base(wafer)
        {
        }

        public override EnumSubsStatus GetState()
        {
            return EnumSubsStatus.UNKNOWN;
        }
        public override void SetStatusUnloaded()
        {
            Wafer.ChangeStatus(new WaferNotExistStatus(Wafer));
        }
        public override void SetStatusLoaded()
        {
            Wafer.ChangeStatus(new WaferLoadedStatus(Wafer));
        }

        public override void SetStatusMissing()
        {
        }
    }
    [Serializable]
    public class WaferUndefinedStatus : WaferStatusBase
    {
        public WaferUndefinedStatus(WaferObject wafer) : base(wafer)
        {
        }

        public override EnumSubsStatus GetState()
        {
            return EnumSubsStatus.UNDEFINED;
        }
        public override void SetStatusUnloaded()
        {
            Wafer.ChangeStatus(new WaferNotExistStatus(Wafer));
        }
        public override void SetStatusLoaded()
        {
            Wafer.ChangeStatus(new WaferLoadedStatus(Wafer));
        }
        public override void SetStatusMissing()
        {
            try
            {
                Wafer.ResetWaferData();
                Wafer.ChangeStatus(new WaferMissingSatus(Wafer));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
    [Serializable]
    public class WaferNotExistStatus : WaferStatusBase
    {
        public WaferNotExistStatus(WaferObject wafer) : base(wafer)
        {
        }

        public override EnumSubsStatus GetState()
        {
            return EnumSubsStatus.NOT_EXIST;
        }
        public override void SetStatusLoaded()
        {
            Wafer.ChangeStatus(new WaferLoadedStatus(Wafer));
        }
        public override void SetStatusUnloaded()
        {
            Wafer.ResetWaferData();
        }
        public override void SetStatusMissing()
        {
            try
            {
            Wafer.ResetWaferData();
            Wafer.ChangeStatus(new WaferMissingSatus(Wafer));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }
    [Serializable]
    public class WaferLoadedStatus : WaferStatusBase
    {
        public WaferLoadedStatus(WaferObject wafer) : base(wafer)
        {
        }

        public override EnumSubsStatus GetState()
        {
            return EnumSubsStatus.EXIST;
        }
        public override void SetStatusUnloaded()
        {
            try
            {
            Wafer.ResetWaferData();
            Wafer.ChangeStatus(new WaferNotExistStatus(Wafer));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public override void SetStatusMissing()
        {
            try
            {
            Wafer.ResetWaferData();
            Wafer.ChangeStatus(new WaferMissingSatus(Wafer));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }
}
