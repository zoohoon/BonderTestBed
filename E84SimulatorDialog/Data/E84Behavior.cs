using E84;
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
using System.Windows.Threading;

namespace E84SimulatorDialog
{
    public class E84Behavior : INotifyPropertyChanged
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

        private int _StartDelay;
        public int StartDelay
        {
            get { return _StartDelay; }
            set
            {
                _StartDelay = value;
                RaisePropertyChanged();
            }
        }

        private int _nextBehaviorDelay;
        public int nextBehaviorDelay
        {
            get { return _nextBehaviorDelay; }
            set
            {
                _nextBehaviorDelay = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<SignalState> _Actions;
        public ObservableCollection<SignalState> Actions
        {
            get { return _Actions; }
            set
            {
                _Actions = value;
                RaisePropertyChanged();
            }
        }

        private bool _IsComplete;
        [JsonIgnore]
        public bool IsComplete
        {
            get { return _IsComplete; }
            set
            {
                _IsComplete = value;
                RaisePropertyChanged();
            }
        }

        private DateTime? _StartTime;
        [JsonIgnore]
        public DateTime? StartTime
        {
            get { return _StartTime; }
            set
            {
                _StartTime = value;
                RaisePropertyChanged();
            }
        }

        private DateTime? _EndTime;
        [JsonIgnore]
        public DateTime? EndTime
        {
            get { return _EndTime; }
            set
            {
                _EndTime = value;
                RaisePropertyChanged();
            }
        }

        private TimeSpan _Duration;
        [JsonIgnore]
        public TimeSpan Duration
        {
            get { return _Duration; }
            set
            {
                _Duration = value;
                RaisePropertyChanged();
            }
        }

        private E84BehaviorResult _Result;
        [JsonIgnore]
        public E84BehaviorResult Result
        {
            get { return _Result; }
            set
            {
                _Result = value;
                RaisePropertyChanged();
            }
        }

        public E84Behavior(string name, ObservableCollection<SignalState> actions, int startdelay, int nextbehaviordelay)
        {
            this.Name = name;
            this.Actions = actions;
            this.StartDelay = startdelay;
            this.nextBehaviorDelay = nextbehaviordelay;

            this.StartTime = null;
            this.EndTime = null;
        }
        public async Task<bool> Run(E84.E84Simulator simulator)
        {
            bool retVal = false;

            try
            {
                // Sleep before starting the loop
                await Task.Delay(StartDelay);

                LoggerManager.Debug($"START BEHAVIOR({Name})");

                // Set the start time
                DateTime startTime = DateTime.Now;
                StartTime = startTime;

                // Create a DispatcherTimer with an interval of 1 second
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(20);

                // Handler for the timer tick event
                EventHandler timerTickHandler = null;
                timerTickHandler = (sender, e) =>
                {
                    // Calculate the current duration
                    Duration = DateTime.Now - startTime;
                };

                // Attach the timer tick handler
                timer.Tick += timerTickHandler;

                // Start the timer
                timer.Start();

                foreach (var action in Actions)
                {
                    var status = await action.Run(simulator);
                    this.Result = status;

                    // 에러인 경우 대기하지 않음.
                    if (this.Result.ErrorCode.CodeNumber == 0)
                    {
                        await Task.Delay(action.nextActionDelay);
                    }
                    else
                    {
                        break;
                    }
                }

                // Set the end time
                DateTime endTime = DateTime.Now;
                EndTime = endTime;

                // Stop the timer
                timer.Stop();

                // Calculate and set the duration
                Duration = endTime - startTime;
            }
            catch (Exception)
            {

            }
            finally
            {
                LoggerManager.Debug($"END BEHAVIOR({Name})");

                IsComplete = true;
            }

            return retVal;
        }
    }

    public class BindableBool : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private bool _value;
        public bool Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged();
            }
        }

        public BindableBool(bool val)
        {
            this.Value = val;
        }
    }

    public class SignalData : INotifyPropertyChanged
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
                if (_Name != value)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<BindableBool> _Values;

        public ObservableCollection<BindableBool> Values
        {
            get { return _Values; }
            set
            {
                _Values = value;
                RaisePropertyChanged();
            }
        }

        private bool _IsEnabled;

        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SignalData(string name, int count, bool enabled)
        {
            Name = name;
            Values = new ObservableCollection<BindableBool>();

            for (int i = 0; i < count; i++)
            {
                Values.Add(new BindableBool(false));
            }

            IsEnabled = enabled;
        }
    }

    public class TraceSignal : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<SignalData> _signalDataList;

        public ObservableCollection<SignalData> SignalDataList
        {
            get { return _signalDataList; }
            private set
            {
                if (_signalDataList != value)
                {
                    _signalDataList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Dictionary<E84SignalInputIndex, bool> inputEnabledStates = new Dictionary<E84SignalInputIndex, bool>
        {
            { E84SignalInputIndex.CS_1, false },
            { E84SignalInputIndex.AM_AVBL, false },
            { E84SignalInputIndex.CONT, false }
        };

        private Dictionary<E84SignalOutputIndex, bool> outputEnabledStates = new Dictionary<E84SignalOutputIndex, bool>
        {
            { E84SignalOutputIndex.VA, false },
            { E84SignalOutputIndex.VS_0, false },
            { E84SignalOutputIndex.VS_1, false }
        };

        private bool GetInputEnabledState(E84SignalInputIndex inputIndex)
        {
            if (inputEnabledStates.ContainsKey(inputIndex))
            {
                return inputEnabledStates[inputIndex];
            }

            return true;
        }

        private bool GetOutputEnabledState(E84SignalOutputIndex outputIndex)
        {
            if (outputEnabledStates.ContainsKey(outputIndex))
            {
                return outputEnabledStates[outputIndex];
            }

            return true;
        }

        public TraceSignal(int count)
        {
            try
            {
                SignalDataList = new ObservableCollection<SignalData>();

                foreach (E84SignalInputIndex input in Enum.GetValues(typeof(E84SignalInputIndex)))
                {
                    bool isEnabled = GetInputEnabledState(input);

                    SignalDataList.Add(new SignalData(input.ToString(), count, isEnabled));
                }

                foreach (E84SignalOutputIndex output in Enum.GetValues(typeof(E84SignalOutputIndex)))
                {
                    bool isEnabled = GetOutputEnabledState(output);

                    SignalDataList.Add(new SignalData(output.ToString(), count, isEnabled));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateSignal(int index, E84SignalTypeWithValue signal)
        {
            try
            {
                var s = SignalDataList.FirstOrDefault(x => x.Name == signal.Type.ToString());

                if (s != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        s.Values[index].Value = signal.Value;
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateSignalSet(int index, ObservableCollection<E84SignalTypeWithValue> signals)
        {
            try
            {
                foreach (var s in signals)
                {
                    var signal = SignalDataList.FirstOrDefault(x => x.Name == s.Type.ToString());

                    if (signal != null)
                    {
                        signal.Values[index].Value = s.Value;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
