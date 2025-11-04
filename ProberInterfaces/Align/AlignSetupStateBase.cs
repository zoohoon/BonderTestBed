using System;
using ProberErrorCode;

namespace ProberInterfaces.Align
{
    using Enum;
    using System.ComponentModel;

    public abstract class AlignSetupStateBase : INotifyPropertyChanged
    {

        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public AlignSetupStateBase(AlignSetupBase module)
        {
            SetupModule = module;
        }

        protected AlignSetupBase _SetupModule;
        public AlignSetupBase SetupModule
        {
            get { return _SetupModule; }
            set
            {
                if (value != _SetupModule)
                {
                    _SetupModule = value;
                    NotifyPropertyChanged("SetupModule");
                }
            }
        }

        public abstract SetupProcStateEnum GetState();
        public abstract EventCodeEnum Run();
        public abstract EventCodeEnum Modify();
    }

    public abstract class AlignSetupNoDataState : AlignSetupStateBase
    {
        public AlignSetupNoDataState(AlignSetupBase module) : base(module)
        {
        }

        public override SetupProcStateEnum GetState()
        {
            return SetupProcStateEnum.IDLE;
        }
    }

    public abstract class AlignSetupIdleState : AlignSetupStateBase
    {
        public AlignSetupIdleState(AlignSetupBase module) : base(module)
        {
        }
        public override SetupProcStateEnum GetState()
        {
            return SetupProcStateEnum.IDLE;
        }
    }
    public abstract class AlignSetupModifyState : AlignSetupStateBase
    {
        public AlignSetupModifyState(AlignSetupBase module) : base(module)
        {
        }
        public override SetupProcStateEnum GetState()
        {
            return SetupProcStateEnum.MODIFIY;
        }

    }

    public abstract class AlignSetupDoneState : AlignSetupStateBase
    {
        public AlignSetupDoneState(AlignSetupBase module) : base(module)
        {
        }
        public override SetupProcStateEnum GetState()
        {
            return SetupProcStateEnum.DONE;
        }
    }
    public abstract class AlignSetupErrorState : AlignSetupStateBase
    {
        public AlignSetupErrorState(AlignSetupBase module) : base(module)
        {
        }
        public override SetupProcStateEnum GetState()
        {
            return SetupProcStateEnum.ERROR;
        }
    }
}
