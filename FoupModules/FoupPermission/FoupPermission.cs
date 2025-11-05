using LogModule;
using System;
using ProberInterfaces.Foup;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberErrorCode;

namespace FoupModules
{
    public abstract class FoupPermissionStateBase : IFoupPermission
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public IFoupModule Module { get; set; }
        public FoupPermissionStateBase(IFoupModule module)
        {
            try
            {
            Module = module;
           
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
      
        public abstract FoupPermissionStateEnum GetState();

        public abstract EventCodeEnum SetBusy();

        public abstract EventCodeEnum SetAuto();

        public abstract EventCodeEnum SetEveryOne();
    }
    
    public class FoupPermissionAutoState : FoupPermissionStateBase
    {
        public FoupPermissionAutoState(IFoupModule module) : base(module)
        {
        }

        public override FoupPermissionStateEnum GetState()
        {
            return FoupPermissionStateEnum.AUTO;
        }

        public override EventCodeEnum SetAuto()
        {
            //LoggerManager.Debug($"Already FoupPermission State is Auto");

            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum SetBusy()
        {
            Module.FoupPermissionStateTransition(new FoupPermissionBusyState(Module));

            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum SetEveryOne()
        {
            Module.FoupPermissionStateTransition(new FoupPermissionEveryOneState(Module));

            return EventCodeEnum.NONE;
        }
    }

    public class FoupPermissionBusyState : FoupPermissionStateBase
    {
        public FoupPermissionBusyState(IFoupModule module) : base(module)
        {
        }

        public override FoupPermissionStateEnum GetState()
        {
            return FoupPermissionStateEnum.BUSY;
        }

        public override EventCodeEnum SetAuto()
        {
            Module.FoupPermissionStateTransition(new FoupPermissionAutoState(Module));

            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum SetBusy()
        {
            //LoggerManager.Debug($"Already FoupPermission State is Busy");

            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum SetEveryOne()
        {
            Module.FoupPermissionStateTransition(new FoupPermissionEveryOneState(Module));

            return EventCodeEnum.NONE;
        }
    }

    public class FoupPermissionEveryOneState : FoupPermissionStateBase
    {
        public FoupPermissionEveryOneState(IFoupModule module) : base(module)
        {
        }

        public override FoupPermissionStateEnum GetState()
        {
            return FoupPermissionStateEnum.EVERY_ONE;
        }

        public override EventCodeEnum SetAuto()
        {
            Module.FoupPermissionStateTransition(new FoupPermissionAutoState(Module));

            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum SetBusy()
        {
            Module.FoupPermissionStateTransition(new FoupPermissionBusyState(Module));

            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum SetEveryOne()
        {
            //LoggerManager.Debug($"Already FoupPermission State is EveryOne");

            return EventCodeEnum.NONE;
        }
    }
}
