using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.State;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SystemState
{

    public class SystemStatusModule : ISystemstatus, IFactoryModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private SystemStatus _State;

        public SystemStatus State
        {
            get { return _State; }
            private set { _State = value; }
        }
        private EnumSysState _CurrState;
        public EnumSysState CurrState
        {
            get { return _CurrState; }
            set
            {
                if (value != _CurrState)
                {
                    _CurrState = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SystemStatusModule()
        {
            this.State = new SystemIdleStatus(this);
            SetIdleState();
        }

        public EnumSysState GetSysState()
        {
            EnumSysState retval = EnumSysState.IDLE;
            try
            {
                retval = State.GetSysState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EnumSysState.ERROR;
            }
            return retval;
        }

        public EventCodeEnum SetSetUpState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = State.SetSetUpState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.EXCEPTION;
            }
            return retval;
        }

        public EventCodeEnum SetIdleState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = State.SetIdleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.EXCEPTION;
            }
            return retval;  
        }
        //public EventCodeEnum SetSetUpDoneState()
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        retval = State.SetSetUpDoneState();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        retval = EventCodeEnum.EXCEPTION;
        //    }
        //    return retval;
        //}
        public EventCodeEnum SetErrorState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = State.SetErrorState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.EXCEPTION;
            }
            return retval;
        }

        public EventCodeEnum SetLotState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = State.SetLotState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.EXCEPTION;
            }
            return retval;
        }

        public EventCodeEnum StateTransition(EnumSysState state)
        {
            EventCodeEnum eventCode = EventCodeEnum.UNDEFINED;
            try
            {
                switch (state)
                {
                    case EnumSysState.IDLE:
                        this.State = new SystemIdleStatus(this);
                        break;
                    case EnumSysState.SETUP:
                        this.State = new SystemSetUpStatus(this);
                        break;
                    case EnumSysState.LOT:
                        this.State = new SystemLotStatus(this);
                        break;
                    case EnumSysState.SETUPDONE:
                        this.State = new SystemSetUpDoneStatus(this);
                        break;
                    case EnumSysState.ERROR:
                    default:
                        this.State = new SystemErrorStatus(this);
                        break;
                }
                CurrState = GetSysState();
                LoggerManager.Debug($"SystemStatusModule.StateTransition(): State transition to {state}");
                eventCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCode;
        }
    }
    public class SystemIdleStatus: SystemStatusBase
    {
        public SystemIdleStatus(SystemStatusModule module): base(module)
        {
            Module = module;
        }

        public override EnumSysState GetSysState()
        {
            return EnumSysState.IDLE;
        }
    }
    public class SystemSetUpStatus : SystemStatusBase
    {
        public SystemSetUpStatus(SystemStatusModule module) : base(module)
        {
            Module = module;
        }

        public override EnumSysState GetSysState()
        {
            return EnumSysState.SETUP;
        }
    }
    public class SystemSetUpDoneStatus : SystemStatusBase
    {
        public SystemSetUpDoneStatus(SystemStatusModule module) : base(module)
        {
            Module = module;
        }

        public override EnumSysState GetSysState()
        {
            return EnumSysState.SETUPDONE;
        }
    }
    public class SystemLotStatus : SystemStatusBase
    {
        public SystemLotStatus(SystemStatusModule module) : base(module)
        {
            Module = module;
        }

        public override EnumSysState GetSysState()
        {
            return EnumSysState.LOT;
        }
    }
    public class SystemErrorStatus : SystemStatusBase
    {
        public SystemErrorStatus(SystemStatusModule module) : base(module)
        {
            Module = module;
        }

        public override EnumSysState GetSysState()
        {
            return EnumSysState.ERROR;
        }
    }

    //base
    public abstract class SystemStatusBase : SystemStatus
    {
        private ISystemstatus _Module;
        public ISystemstatus Module
        {
            get { return _Module; }
            set
            {
                if (value != _Module)
                {
                    _Module = value;
                }
            }
        }

        public SystemStatusBase(ISystemstatus module)
        {
            try
            {
                Module = module;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
       
        public override EventCodeEnum SetErrorState()
        {
            EventCodeEnum eventCode = EventCodeEnum.UNDEFINED;
            try
            {
                Module.StateTransition(EnumSysState.ERROR);
                eventCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCode;
        }
        public override EventCodeEnum SetIdleState()
        {
            EventCodeEnum eventCode = EventCodeEnum.UNDEFINED;
            try
            {
                Module.StateTransition(EnumSysState.IDLE);
                eventCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCode;
        }
        public override EventCodeEnum SetSetUpState()
        {
            EventCodeEnum eventCode = EventCodeEnum.UNDEFINED;
            try
            {
                Module.StateTransition(EnumSysState.SETUP);
                eventCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCode;
        }
        public override EventCodeEnum SetSetUpDoneState()
        {
            EventCodeEnum eventCode = EventCodeEnum.UNDEFINED;
            try
            {
                Module.StateTransition(EnumSysState.SETUPDONE);
                eventCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCode;
        }
        public override EventCodeEnum SetLotState()
        {
            EventCodeEnum eventCode = EventCodeEnum.UNDEFINED;
            try
            {
                Module.StateTransition(EnumSysState.LOT);
                eventCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCode;
        }
    }
}
