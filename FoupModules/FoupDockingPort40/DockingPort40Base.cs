using System.Net.Sockets;
using ProberInterfaces.Foup;
using ProberErrorCode;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FoupModules.DockingPort40
{
    public abstract class DockingPort40Base : IFoupDockingPort40, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public DockingPort40Base(IFoupModule module)
        {
            this.Module = module;
        }
        public DockingPort40Base()
        {
        }
        public IFoupModule Module { get; set; }
        public IFoupIOStates FoupIOManager => Module.IOManager;

        private DockingPort40StateEnum _EnumState;
        public DockingPort40StateEnum EnumState
        {
            get { return _EnumState; }
            set
            {
                _EnumState = value;
                RaisePropertyChanged();
            }
        }

        public abstract DockingPort40StateEnum GetState();
        public abstract EventCodeEnum StateInit();
        public abstract EventCodeEnum In();
        public abstract EventCodeEnum Out();


    }
}
