using LogModule;
using Newtonsoft.Json;
using ProberInterfaces;
using ProberInterfaces.Communication.E84;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace E84SimulatorDialog
{
    public class E84Script : INotifyPropertyChanged
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

        private ObservableCollection<E84Behavior> _Behaviors = new ObservableCollection<E84Behavior>();
        public ObservableCollection<E84Behavior> Behaviors
        {
            get { return _Behaviors; }
            set
            {
                _Behaviors = value;
                RaisePropertyChanged();
            }
        }

        
        private ObservableCollection<SignalState> _Actions = new ObservableCollection<SignalState>();
        [JsonIgnore]
        public ObservableCollection<SignalState> Actions
        {
            get { return _Actions; }
            set
            {
                _Actions = value;
                RaisePropertyChanged();
            }
        }

        private SignalState _SelectedAction;
        [JsonIgnore]
        public SignalState SelectedAction
        {
            get { return _SelectedAction; }
            set
            {
                _SelectedAction = value;
                RaisePropertyChanged();
            }
        }

        private E84Behavior _SelectedBehavior;
        [JsonIgnore]
        public E84Behavior SelectedBehavior
        {
            get { return _SelectedBehavior; }
            set
            {
                _SelectedBehavior = value;
                RaisePropertyChanged();
            }
        }

        private TraceSignal _Trace;
        [JsonIgnore]
        public TraceSignal Trace
        {
            get { return _Trace; }
            set
            {
                if (_Trace != value)
                {
                    _Trace = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsRun;
        [JsonIgnore]
        public bool IsRun
        {
            get { return _IsRun; }
            set
            {
                _IsRun = value;
                RaisePropertyChanged();
            }
        }

        private bool _IsStop;
        [JsonIgnore]
        public bool IsStop
        {
            get { return _IsStop; }
            set
            {
                _IsStop = value;
                RaisePropertyChanged();
            }
        }

        public E84Script(string name)
        {
            this.Name = name;
        }
        public void InitTrace(E84.E84Simulator simulator)
        {
            try
            {
                Trace = new TraceSignal(this.Behaviors.Count);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task<bool> Run(E84.E84Simulator simulator, ObservableCollection<E84Input> Inputs, ObservableCollection<E84Output> Outputs)
        {
            bool retVal = false;

            try
            {
                simulator.IsStop = false;
                InitTrace(simulator);

                IsRun = true;

                LoggerManager.Debug($"START SCRIPT({Name})");

                int i = 0;

                foreach (var Behavior in Behaviors)
                {
                    retVal = await Behavior.Run(simulator);

                    UpdateTrace(i, simulator);

                    i++;

                    if (Behavior.Result.ErrorCode.CodeNumber != 0)
                    {
                        break;
                    }

                    if (IsStop)
                    {
                        break;
                    }

                    await Task.Delay(Behavior.nextBehaviorDelay);
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                IsRun = false;

                LoggerManager.Debug($"END SCRIPT({Name})");
            }

            return retVal;
        }
        private void UpdateTrace(int index, E84.E84Simulator simulator)
        {
            try
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Trace.SignalDataList[0].Values[index].Value = simulator.E84Inputs[0];
                    Trace.SignalDataList[1].Values[index].Value = simulator.E84Inputs[1];
                    Trace.SignalDataList[2].Values[index].Value = simulator.E84Inputs[2];
                    Trace.SignalDataList[3].Values[index].Value = simulator.E84Inputs[3];
                    Trace.SignalDataList[4].Values[index].Value = simulator.E84Inputs[4];
                    Trace.SignalDataList[5].Values[index].Value = simulator.E84Inputs[5];
                    Trace.SignalDataList[6].Values[index].Value = simulator.E84Inputs[6];
                    Trace.SignalDataList[7].Values[index].Value = simulator.E84Inputs[7];
                    Trace.SignalDataList[8].Values[index].Value = simulator.E84Inputs[8];

                    Trace.SignalDataList[9].Values[index].Value = simulator.E84Outputs[0];
                    Trace.SignalDataList[10].Values[index].Value = simulator.E84Outputs[1];
                    Trace.SignalDataList[11].Values[index].Value = simulator.E84Outputs[2];
                    Trace.SignalDataList[12].Values[index].Value = simulator.E84Outputs[3];
                    Trace.SignalDataList[13].Values[index].Value = simulator.E84Outputs[4];
                    Trace.SignalDataList[14].Values[index].Value = simulator.E84Outputs[5];
                    Trace.SignalDataList[15].Values[index].Value = simulator.E84Outputs[6];
                    Trace.SignalDataList[16].Values[index].Value = simulator.E84Outputs[7];
                    Trace.SignalDataList[17].Values[index].Value = simulator.E84Outputs[8];
                    Trace.SignalDataList[18].Values[index].Value = simulator.E84Outputs[9];
                });

                //Application.Current.Dispatcher.InvokeAsync(() =>
                //{
                //    Trace.SignalDataList[0].Values[index].Value = Inputs[0].CurrentValue;
                //    Trace.SignalDataList[1].Values[index].Value = Inputs[1].CurrentValue;
                //    Trace.SignalDataList[2].Values[index].Value = Inputs[2].CurrentValue;
                //    Trace.SignalDataList[3].Values[index].Value = Inputs[3].CurrentValue;
                //    Trace.SignalDataList[4].Values[index].Value = Inputs[4].CurrentValue;
                //    Trace.SignalDataList[5].Values[index].Value = Inputs[5].CurrentValue;
                //    Trace.SignalDataList[6].Values[index].Value = Inputs[6].CurrentValue;
                //    Trace.SignalDataList[7].Values[index].Value = Inputs[7].CurrentValue;
                //    Trace.SignalDataList[8].Values[index].Value = Inputs[8].CurrentValue;

                //    Trace.SignalDataList[9].Values[index].Value = Outputs[0].CurrentValue;
                //    Trace.SignalDataList[10].Values[index].Value = Outputs[1].CurrentValue;
                //    Trace.SignalDataList[11].Values[index].Value = Outputs[2].CurrentValue;
                //    Trace.SignalDataList[12].Values[index].Value = Outputs[3].CurrentValue;
                //    Trace.SignalDataList[13].Values[index].Value = Outputs[4].CurrentValue;
                //    Trace.SignalDataList[14].Values[index].Value = Outputs[5].CurrentValue;
                //    Trace.SignalDataList[15].Values[index].Value = Outputs[6].CurrentValue;
                //    Trace.SignalDataList[16].Values[index].Value = Outputs[7].CurrentValue;
                //    Trace.SignalDataList[17].Values[index].Value = Outputs[8].CurrentValue;
                //    Trace.SignalDataList[18].Values[index].Value = Outputs[9].CurrentValue;
                //});
            }
            catch (Exception)
            {
            }
        }
        public void Stop(E84.E84Simulator simulator)
        {
            try
            {
                IsStop = true;
                simulator.IsStop = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public SignalState MakeSignalState(string name, bool state, int nextActionDelay)
        {
            SignalState retval = new SignalState(name, state, nextActionDelay);

            return retval;
        }
        public E84Behavior CreateBehavior(ObservableCollection<SignalState> actions, int startdelay, int nextbehaviordelay)
        {
            int behaviorCounter = Behaviors.Count + 1;
            string behaviorName = "Behavior_" + behaviorCounter;

            return new E84Behavior(behaviorName, actions, startdelay, nextbehaviordelay);
        }
        public void MakeLoadSequence()
        {
            int startdelay = 0;
            int nextbehaviordelay = 500;

            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.CS_0.ToString(), true, 0) }, startdelay, nextbehaviordelay));
            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.VALID.ToString(), true, 0) }, startdelay, nextbehaviordelay));
            //Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalOutputIndex.L_REQ.ToString(), true, 0) }, startdelay, nextbehaviordelay));

            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.TR_REQ.ToString(), true, 0) }, startdelay, nextbehaviordelay));
            //Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalOutputIndex.READY.ToString(), true, 0) }, startdelay, nextbehaviordelay));

            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.BUSY.ToString(), true, 0) }, startdelay, nextbehaviordelay));
            //Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalOutputIndex.L_REQ.ToString(), false, 0) }, startdelay, nextbehaviordelay));

            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.BUSY.ToString(), false, 0) }, startdelay, nextbehaviordelay));
            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.TR_REQ.ToString(), false, 0) }, startdelay, nextbehaviordelay));
            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.COMPT.ToString(), true, 0) }, startdelay, nextbehaviordelay));
            //Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalOutputIndex.READY.ToString(), false, 0) }, startdelay, nextbehaviordelay));

            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.VALID.ToString(), false, 0) }, startdelay, nextbehaviordelay));
            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.COMPT.ToString(), false, 0) }, startdelay, nextbehaviordelay));
            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.CS_0.ToString(), false, 0) }, startdelay, nextbehaviordelay));
        }
        public void MakeUnLoadSequence()
        {
            int startdelay = 0;
            int nextbehaviordelay = 500;

            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.CS_0.ToString(), true, 0) }, startdelay, nextbehaviordelay));
            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.VALID.ToString(), true, 0) }, startdelay, nextbehaviordelay));
            //Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalOutputIndex.UL_REQ.ToString(), true, 0) }, startdelay, nextbehaviordelay));

            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.TR_REQ.ToString(), true, 0) }, startdelay, nextbehaviordelay));
            //Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalOutputIndex.READY.ToString(), true, 0) }, startdelay, nextbehaviordelay));

            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.BUSY.ToString(), true, 0) }, startdelay, nextbehaviordelay));
            //Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalOutputIndex.UL_REQ.ToString(), false, 0) }, startdelay, nextbehaviordelay));

            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.BUSY.ToString(), false, 0) }, startdelay, nextbehaviordelay));
            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.TR_REQ.ToString(), false, 0) }, startdelay, nextbehaviordelay));
            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.COMPT.ToString(), true, 0) }, startdelay, nextbehaviordelay));
            //Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalOutputIndex.READY.ToString(), false, 0) }, startdelay, nextbehaviordelay));

            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.VALID.ToString(), false, 0) }, startdelay, nextbehaviordelay));
            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.COMPT.ToString(), false, 0) }, startdelay, nextbehaviordelay));
            Behaviors.Add(CreateBehavior(new ObservableCollection<SignalState>() { MakeSignalState(E84SignalInputIndex.CS_0.ToString(), false, 0) }, startdelay, nextbehaviordelay));
        }
        public E84Script Copy()
        {
            E84Script copiedScript = null;

            try
            {
                copiedScript = new E84Script(this.Name);

                // Create a deep copy of the Behaviors collection
                foreach (var behavior in this.Behaviors)
                {
                    ObservableCollection<SignalState> copiedActions = new ObservableCollection<SignalState>();

                    foreach (var action in behavior.Actions)
                    {
                        SignalState copiedAction = new SignalState(action.Name, action.State, action.nextActionDelay);
                        copiedActions.Add(copiedAction);
                    }

                    E84Behavior copiedBehavior = new E84Behavior(behavior.Name, copiedActions, behavior.StartDelay, behavior.nextBehaviorDelay);

                    copiedScript.Behaviors.Add(copiedBehavior);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }



            return copiedScript;
        }
    }
}
