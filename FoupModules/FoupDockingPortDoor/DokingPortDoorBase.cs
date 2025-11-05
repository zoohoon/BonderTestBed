using ProberInterfaces.Foup;
using ProberErrorCode;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FoupModules.DockingPortDoor
{
    public abstract class DockingPortDoorBase : IFoupDoor
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public DockingPortDoorBase(IFoupModule module)
        {
            this.Module = module;
        }
        public DockingPortDoorBase()
        {
        }
        public IFoupModule Module { get; set; }
        public IFoupIOStates FoupIOManager => Module.IOManager;

        //public DockingPortDoorStateEnum EnumStateState
        //{
        //    get
        //    {
        //        return GetState();
        //    }
        //}

        private DockingPortDoorStateEnum _EnumState;
        public DockingPortDoorStateEnum EnumState
        {
            get { return _EnumState; }
            set
            {
                _EnumState = value;
                RaisePropertyChanged();
            }
        }

        public abstract DockingPortDoorStateEnum GetState();

        public abstract EventCodeEnum StateInit();
        public abstract EventCodeEnum Open();

        public abstract EventCodeEnum Close();


    }
}
