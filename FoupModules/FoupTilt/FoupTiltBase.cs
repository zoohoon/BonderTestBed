using ProberInterfaces.Foup;
using ProberErrorCode;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FoupModules.FoupTilt
{
    public abstract class FoupTiltBase : IFoupTilt
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public FoupTiltBase()
        {

        }
        public FoupTiltBase(IFoupModule module)
        {
            this.Module = module;
        }
        public IFoupModule Module { get; set; }
        public IFoupIOStates FoupIOManager => Module.IOManager;
        //public TiltStateEnum EnumStateState
        //{
        //    get
        //    {
        //        return GetState();
        //    }
        //}

        private TiltStateEnum _EnumState;
        public TiltStateEnum EnumState
        {
            get { return _EnumState; }
            set
            {
                _EnumState = value;
                RaisePropertyChanged();
            }
        }

        public abstract TiltStateEnum State { get; }
        public abstract TiltStateEnum GetState();
        public abstract EventCodeEnum StateInit();
        public abstract EventCodeEnum Up();
        public abstract EventCodeEnum Down();
    }
}
