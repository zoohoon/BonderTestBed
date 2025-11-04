using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberInterfaces.Foup;
using ProberErrorCode;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProberInterfaces;

namespace FoupModules.DockingPlate
{
    public abstract class FoupDockingPlateBase : IFoupDockingPlate, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public FoupDockingPlateBase(IFoupModule module)
        {
            this.Module = module;
        }
        public FoupDockingPlateBase()
        {
            
        }
        public IFoupModule Module { get; set; }
        public virtual IFoupIOStates FoupIOManager => Module.IOManager;

        private DockingPlateStateEnum _EnumState;
        public DockingPlateStateEnum EnumState
        {
            get { return _EnumState; }
            set
            {
                _EnumState = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _Inputs=new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> Inputs
        {
            get { return _Inputs; }
            set
            {
                if (value != _Inputs)
                {
                    _Inputs = value;
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _Outputs=new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> Outputs
        {
            get { return _Outputs; }
            set
            {
                if (value != _Outputs)
                {
                    _Outputs = value;
                }
            }
        }
        public abstract DockingPlateStateEnum GetState();
        public abstract EventCodeEnum StateInit();
        public abstract EventCodeEnum CheckState();
        public abstract EventCodeEnum Lock();
        public abstract EventCodeEnum Unlock();
        public abstract EventCodeEnum RecoveryUnlock();
    }

}
