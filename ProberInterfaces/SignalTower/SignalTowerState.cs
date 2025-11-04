using LogModule;
using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ProberInterfaces.SignalTower
{
    public enum EnumSignalTowerState
    {
        UNDEFINED,
        ON,
        OFF,
        BLINK,
    }

    public enum EnumSignalTowerColor
    {
        UNDEFINED,
        RED,
        YELLOW,
        GREEN,
        BUZZER,
    }

    public abstract class SignalTowerStateBase : IFactoryModule, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private EnumSignalTowerState _State;
        public EnumSignalTowerState State
        {
            get { return _State; }
            set
            {
                if (value != _State)
                {
                    _State = value;
                    NotifyPropertyChanged("State");
                }
            }
        }

        public ISignalTowerManager SignalTowerManager { get; private set; }

        public abstract EnumSignalTowerState GetState();
        public virtual void SetOn()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetState()));
        }
        public virtual void SetOff()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetState()));
        }
        public virtual void SetBlink()
        {

        }
    }

    public class SignalTowerONState : SignalTowerStateBase
    {
        public SignalTowerONState() : base()
        {
            this.State = EnumSignalTowerState.ON;
        }

        public override EnumSignalTowerState GetState() => EnumSignalTowerState.ON;

        public override void SetBlink()
        {
            //only possible green signal
            try
            {
                SignalTowerManager.InnerStateTransition(new SignalTowerBLINKState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public override void SetOff()
        {
            //on state -> off state
            try
            {
                SignalTowerManager.InnerStateTransition(new SignalTowerOFFState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
    }

    public class SignalTowerOFFState : SignalTowerStateBase
    {
        public SignalTowerOFFState() : base()
        {
            this.State = EnumSignalTowerState.OFF;
        }

        public override EnumSignalTowerState GetState() => EnumSignalTowerState.OFF;

        public override void SetBlink()
        {
            //off state -> blink state
            try
            {
                SignalTowerManager.InnerStateTransition(new SignalTowerBLINKState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public override void SetOn()
        {
            //off state -> on state
            try
            {
                SignalTowerManager.InnerStateTransition(new SignalTowerONState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
    }

    public class SignalTowerBLINKState : SignalTowerStateBase
    {
        public SignalTowerBLINKState() : base()
        {
            this.State = EnumSignalTowerState.BLINK;
        }

        public override EnumSignalTowerState GetState() => EnumSignalTowerState.BLINK;
        public override void SetOff()
        {
            //blink state -> off state
            try
            {
                SignalTowerManager.InnerStateTransition(new SignalTowerOFFState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public override void SetOn()
        {
            //blink state -> on state
            try
            {
                SignalTowerManager.InnerStateTransition(new SignalTowerONState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
    }    
}
