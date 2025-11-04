using E84;
using LogModule;
using ProberInterfaces;
using ProberInterfaces.Communication.E84;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace E84SimulatorDialog
{
    public class SignalState : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                RaisePropertyChanged();
            }
        }

        private bool _State;
        public bool State
        {
            get { return _State; }
            set
            {
                _State = value;
                RaisePropertyChanged();
            }
        }

        private int _nextActionDelay;
        public int nextActionDelay
        {
            get { return _nextActionDelay; }
            set
            {
                _nextActionDelay = value;
                RaisePropertyChanged();
            }
        }

        public SignalState(string name, bool state, int nextActionDelay)
        {
            this.Name = name;
            this.State = state;
            this.nextActionDelay = nextActionDelay;
        }
        public async Task<E84BehaviorResult> Run(E84.E84Simulator simulator)
        {
            E84BehaviorResult retVal = null;

            try
            {
                LoggerManager.Debug($"START ACTION({Name}), STATE = {State}");

                retVal = await simulator.SetInput(this.Name, State);
            }
            catch (Exception)
            {

            }
            finally
            {
                LoggerManager.Debug($"END ACTION({Name}), STATE = {State}");
            }

            return retVal;
        }
    }
}
