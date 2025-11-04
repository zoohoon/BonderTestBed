using E84;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LogModule;
using MetroDialogInterfaces;
using ProberInterfaces;
using ProberInterfaces.Communication.E84;
using ProberInterfaces.E84;
using ProberInterfaces.E84.ProberInterfaces;
using ProberViewModel.View.E84.Setting;
using ProberViewModel.ViewModel.E84;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WinAPIWrapper;

namespace E84SimulatorDialog
{
    public class E84SimulControllerViewModel : IFactoryModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties

        private E84Info _E84Info;
        public E84Info E84Info
        {
            get { return _E84Info; }
            set
            {
                _E84Info = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<E84Chart> _Charts;
        public ObservableCollection<E84Chart> Charts
        {
            get { return _Charts; }
            set
            {
                if (value != _Charts)
                {
                    _Charts = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<E84Input> _Inputs;
        public ObservableCollection<E84Input> Inputs
        {
            get { return _Inputs; }
            set
            {
                if (_Inputs != value)
                {
                    _Inputs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<E84Output> _Outputs;
        public ObservableCollection<E84Output> Outputs
        {
            get { return _Outputs; }
            set
            {
                if (_Outputs != value)
                {
                    _Outputs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedTapIndex;
        public int SelectedTapIndex
        {
            get { return _SelectedTapIndex; }
            set
            {
                if (value != _SelectedTapIndex)
                {
                    _SelectedTapIndex = value;

                    OnSimulModeChanged();

                    RaisePropertyChanged();
                }
            }
        }

        private E84SimulMode _SimulMode;
        public E84SimulMode SimulMode
        {
            get { return _SimulMode; }
            set
            {
                if (value != _SimulMode)
                {
                    _SimulMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _LastUpdateBrush;
        public Brush LastUpdateBrush
        {
            get { return _LastUpdateBrush; }
            private set
            {
                if (value != _LastUpdateBrush)
                {
                    _LastUpdateBrush = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _OnBrush;
        public Brush OnBrush
        {
            get { return _OnBrush; }
            private set
            {
                if (value != _OnBrush)
                {
                    _OnBrush = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _OffBrush;
        public Brush OffBrush
        {
            get { return _OffBrush; }
            private set
            {
                if (value != _OffBrush)
                {
                    _OffBrush = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsReset;
        public bool IsReset
        {
            get { return _IsReset; }
            set
            {
                _IsReset = value;
                RaisePropertyChanged();
            }
        }

        private E84Script _SelectedScript;
        public E84Script SelectedScript
        {
            get { return _SelectedScript; }
            set
            {
                if (value != _SelectedScript)
                {
                    _SelectedScript = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84Script _CopiedScript;
        public E84Script CopiedScript
        {
            get { return _CopiedScript; }
            set
            {
                if (value != _CopiedScript)
                {
                    _CopiedScript = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TraceSignal _InitialTrace;
        public TraceSignal InitialTrace
        {
            get { return _InitialTrace; }
            set
            {
                if (_InitialTrace != value)
                {
                    _InitialTrace = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84Simulator _Simulator;
        public E84Simulator Simulator
        {
            get { return _Simulator; }
            set
            {
                _Simulator = value;
                RaisePropertyChanged();
            }
        }

        #region Commands

        public ICommand InputOnCommand { get; private set; }
        public ICommand InputOffCommand { get; private set; }
        public ICommand OutputOnCommand { get; private set; }
        public ICommand OutputOffCommand { get; private set; }

        public ICommand InputAux_0_OnCommand { get; private set; }
        public ICommand InputAux_0_OffCommand { get; private set; }
        public ICommand InputAux_1_OnCommand { get; private set; }
        public ICommand InputAux_1_OffCommand { get; private set; }
        public ICommand InputAux_2_OnCommand { get; private set; }
        public ICommand InputAux_2_OffCommand { get; private set; }
        public ICommand InputAux_3_OnCommand { get; private set; }
        public ICommand InputAux_3_OffCommand { get; private set; }
        public ICommand InputAux_4_OnCommand { get; private set; }
        public ICommand InputAux_4_OffCommand { get; private set; }
        public ICommand InputAux_5_OnCommand { get; private set; }
        public ICommand InputAux_5_OffCommand { get; private set; }
        public ICommand OutputAux_0_OnCommand { get; private set; }
        public ICommand OutputAux_0_OffCommand { get; private set; }


        public ICommand SimulModeChangedCommand { get; private set; }
        public ICommand ScriptStartCommand { get; private set; }
        public ICommand ScriptStopCommand { get; private set; }
        public ICommand SelectedScriptChangedCommand { get; private set; }

        public ICommand SetChartScrollViewerCommand { get; private set; }

        #endregion
        //public E84SimualtorViewObj ViewObj { get; set; }

        public void Init()
        {
            try
            {
                InitializeCommands();
                InitBrush();

                SimulMode = E84SimulMode.MANUAL;
                OnSimulModeChanged();

                InitInPutAndOutput();

                InitCharts();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void Deinit()
        {
            try
            {
                if (E84Info.E84Controller.CommModule != null)
                {
                    if (E84Info.E84Controller.CommModule is SimulE84CommModule commModule)
                    {
                        commModule.E84Inputs.E84SignalChangeEvent -= ChangedSignal;
                        commModule.E84Outputs.E84SignalChangeEvent -= ChangedSignal;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        //public enum E84SignalInputIndex
        //{
        //    (O) VALID = 0,
        //    (O) CS_0 = 1,
        //    (X) CS_1 = 2, : Carrier Stage 1
        //    (X) AM_AVBL = 3, : interbay AMHS (수동 OHS 운송 장비)
        //    (O) TR_REQ = 4,
        //    (O) BUSY = 5,
        //    (O) COMPT = 6,
        //    (X) CONT = 7, : Continuous HandOff(연속 핸드오프)
        //    (O) GO = 8,
        //}

        //public enum E84SignalOutputIndex
        //{
        //    (O)  L_REQ = 0,
        //    (O)  UL_REQ = 1,
        //    (X)  VA = 2, : interbay AMHS (수동 OHS 운송 장비)
        //    (O)  READY = 3,
        //    (X)  VS_0 = 4, : interbay AMHS (수동 OHS 운송 장비)
        //    (X)  VS_1 = 5, : interbay AMHS (수동 OHS 운송 장비)
        //    (O)  HO_AVBL = 6,
        //    (O)  ES = 7,
        //    (△) SELECT = 8,
        //    (△) MODE = 9,
        //}

        public Dictionary<E84SignalInputIndex, bool> inputEnabledStates = new Dictionary<E84SignalInputIndex, bool>
        {
            { E84SignalInputIndex.CS_1, false },
            { E84SignalInputIndex.AM_AVBL, false },
            { E84SignalInputIndex.CONT, false }
        };

        public Dictionary<E84SignalOutputIndex, bool> outputEnabledStates = new Dictionary<E84SignalOutputIndex, bool>
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

        public E84SignalTypeEnum GetSignalTypeEnum(E84SignalInputIndex inputIndex)
        {
            switch (inputIndex)
            {
                case E84SignalInputIndex.VALID:
                    return E84SignalTypeEnum.VALID;
                case E84SignalInputIndex.CS_0:
                    return E84SignalTypeEnum.CS_0;
                case E84SignalInputIndex.CS_1:
                    return E84SignalTypeEnum.CS_1;
                case E84SignalInputIndex.AM_AVBL:
                    return E84SignalTypeEnum.AM_AVBL;
                case E84SignalInputIndex.TR_REQ:
                    return E84SignalTypeEnum.TR_REQ;
                case E84SignalInputIndex.BUSY:
                    return E84SignalTypeEnum.BUSY;
                case E84SignalInputIndex.COMPT:
                    return E84SignalTypeEnum.COMPT;
                case E84SignalInputIndex.CONT:
                    return E84SignalTypeEnum.CONT;
                case E84SignalInputIndex.GO:
                    return E84SignalTypeEnum.GO;
                default:
                    return E84SignalTypeEnum.INVALID;
            }
        }

        public E84SignalTypeEnum GetSignalTypeEnum(E84SignalOutputIndex outputIndex)
        {
            switch (outputIndex)
            {
                case E84SignalOutputIndex.L_REQ:
                    return E84SignalTypeEnum.L_REQ;
                case E84SignalOutputIndex.U_REQ:
                    return E84SignalTypeEnum.U_REQ;
                case E84SignalOutputIndex.VA:
                    return E84SignalTypeEnum.VA;
                case E84SignalOutputIndex.READY:
                    return E84SignalTypeEnum.READY;
                case E84SignalOutputIndex.VS_0:
                    return E84SignalTypeEnum.VS_0;
                case E84SignalOutputIndex.VS_1:
                    return E84SignalTypeEnum.VS_1;
                case E84SignalOutputIndex.HO_AVBL:
                    return E84SignalTypeEnum.HO_AVBL;
                case E84SignalOutputIndex.ES:
                    return E84SignalTypeEnum.ES;
                case E84SignalOutputIndex.SELECT:
                    return E84SignalTypeEnum.SELECT;
                case E84SignalOutputIndex.MODE:
                    return E84SignalTypeEnum.MODE;
                default:
                    return E84SignalTypeEnum.INVALID;
            }
        }

        private void UpdateChartColor(E84Chart chart)
        {
            try
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ((StepLineSeries)chart.Series[0]).Stroke = chart.LastValue == 0 ? OffBrush : OnBrush;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CreateOrResetChart(E84SignalTypeEnum signalTypeEnum, SignalIOType iotype, bool isEnabled)
        {
            try
            {
                E84Chart matchingChart = Charts.FirstOrDefault(c => c.SignalType == signalTypeEnum);

                if (matchingChart == null)
                {
                    E84Chart chart = MakeE84Chart(iotype, signalTypeEnum, GetSimValue(signalTypeEnum) ? 1 : 0, isEnabled);
                    Charts.Add(chart);

                    UpdateChartColor(chart);
                }
                else
                {
                    ResetChartValues(matchingChart, GetSimValue(signalTypeEnum) ? 1 : 0);

                    UpdateChartColor(matchingChart);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void InitCharts()
        {
            try
            {
                Charts = new BulkUpdateObservableCollection<E84Chart>();

                foreach (E84SignalInputIndex index in Enum.GetValues(typeof(E84SignalInputIndex)))
                {
                    CreateOrResetChart(GetSignalTypeEnum(index), SignalIOType.INPUT, GetInputEnabledState(index));
                }

                foreach (E84SignalOutputIndex index in Enum.GetValues(typeof(E84SignalOutputIndex)))
                {
                    CreateOrResetChart(GetSignalTypeEnum(index), SignalIOType.OUTPUT, GetOutputEnabledState(index));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ResetChartValues(E84Chart chart, double initVal)
        {
            try
            {
                double minTime = DateTime.UtcNow.AddSeconds(-5).Ticks;
                double maxTime = DateTime.UtcNow.AddSeconds(5).Ticks;

                chart.MinDateTimeValue = minTime;
                chart.MaxDateTimeValue = maxTime;

                chart.Series[0].Values = new ChartValues<ObservablePoint>
                {
                    new ObservablePoint(minTime, initVal),
                    new ObservablePoint(maxTime, initVal)
                };
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public E84Chart MakeE84Chart(SignalIOType iotype, E84SignalTypeEnum signalType, double initVal, bool isEnabled)
        {
            E84Chart retval = new E84Chart();
            
            try
            {
                double minTime = DateTime.UtcNow.AddSeconds(-5).Ticks;
                double maxTime = DateTime.UtcNow.AddSeconds(5).Ticks;

                retval.MinDateTimeValue = minTime;
                retval.MaxDateTimeValue = maxTime;

                retval.Formatter = value => new DateTime((long)value).ToString("mm:ss:fff");
                retval.Type = iotype;
                retval.SignalType = signalType;
                retval.LastValue = initVal;

                retval.Series.Add(new StepLineSeries
                {
                    Title = signalType.ToString(),
                    Values = new ChartValues<ObservablePoint>
                    {
                        new ObservablePoint(minTime, retval.LastValue),
                        new ObservablePoint(maxTime, retval.LastValue),
                    },
                    PointGeometry = null,
                    Fill = Brushes.Transparent,
                    StrokeThickness = 2
                });

                retval.IsVisible = isEnabled ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void ResetCharts()
        {
            try
            {
                foreach (var chart in Charts)
                {
                    CreateOrResetChart(chart.SignalType, chart.Type, chart.IsVisible == Visibility.Visible);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void InitInPutAndOutput()
        {
            try
            {
                Inputs = new ObservableCollection<E84Input>();
                Outputs = new ObservableCollection<E84Output>();

                foreach (E84SignalInputIndex input in Enum.GetValues(typeof(E84SignalInputIndex)))
                {
                    bool isEnabled = GetInputEnabledState(input);

                    E84Input i = new E84Input(input);
                    i.IsEnabled = isEnabled;

                    Inputs.Add(i);
                }

                foreach (E84SignalOutputIndex output in Enum.GetValues(typeof(E84SignalOutputIndex)))
                {
                    bool isEnabled = GetOutputEnabledState(output);

                    E84Output o = new E84Output(output);
                    o.IsEnabled = isEnabled;

                    Outputs.Add(o);
                }

                if (E84Info.E84Controller.CommModule != null)
                {
                    if (E84Info.E84Controller.CommModule is SimulE84CommModule commModule)
                    {
                        commModule.E84Inputs.E84SignalChangeEvent += ChangedSignal;
                        commModule.E84Outputs.E84SignalChangeEvent += ChangedSignal;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InitBrush()
        {
            try
            {
                LastUpdateBrush = Brushes.LimeGreen;
                OnBrush = Brushes.Red;
                OffBrush = Brushes.Black;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InitializeCommands()
        {
            try
            {
                InputOnCommand = new RelayCommand<object>(ExecuteCommand);
                InputOffCommand = new RelayCommand<object>(ExecuteCommand);

                OutputOnCommand = new RelayCommand<object>(ExecuteCommand);
                OutputOffCommand = new RelayCommand<object>(ExecuteCommand);

                InputAux_0_OnCommand = new RelayCommand(InputAux_0_OnCommandFunc);
                InputAux_0_OffCommand = new RelayCommand(InputAux_0_OffCommandFunc);
                InputAux_1_OnCommand = new RelayCommand(InputAux_1_OnCommandFunc);
                InputAux_1_OffCommand = new RelayCommand(InputAux_1_OffCommandFunc);
                InputAux_2_OnCommand = new RelayCommand(InputAux_2_OnCommandFunc);
                InputAux_2_OffCommand = new RelayCommand(InputAux_2_OffCommandFunc);
                InputAux_3_OnCommand = new RelayCommand(InputAux_3_OnCommandFunc);
                InputAux_3_OffCommand = new RelayCommand(InputAux_3_OffCommandFunc);
                InputAux_4_OnCommand = new RelayCommand(InputAux_4_OnCommandFunc);
                InputAux_4_OffCommand = new RelayCommand(InputAux_4_OffCommandFunc);
                InputAux_5_OnCommand = new RelayCommand(InputAux_5_OnCommandFunc);
                InputAux_5_OffCommand = new RelayCommand(InputAux_5_OffCommandFunc);

                OutputAux_0_OnCommand = new RelayCommand(OutputAux_0_OnCommandFunc);
                OutputAux_0_OffCommand = new RelayCommand(OutputAux_0_OffCommandFunc);

                SimulModeChangedCommand = new RelayCommand(OnSimulModeChanged);

                ScriptStartCommand = new RelayCommand(ScriptStartCommandFunc);
                ScriptStopCommand = new RelayCommand(ScriptStopCommandFunc);

                SelectedScriptChangedCommand = new RelayCommand(SelectedScriptChangedCommandFunc);

                SetChartScrollViewerCommand = new RelayCommand<object>(SetChartScrollViewerCommandFunc);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void OutputAux_0_OffCommandFunc()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void OutputAux_0_OnCommandFunc()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InputAux_5_OffCommandFunc()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InputAux_5_OnCommandFunc()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InputAux_4_OffCommandFunc()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InputAux_4_OnCommandFunc()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InputAux_3_OffCommandFunc()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InputAux_3_OnCommandFunc()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InputAux_2_OffCommandFunc()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InputAux_2_OnCommandFunc()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InputAux_1_OffCommandFunc()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InputAux_1_OnCommandFunc()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InputAux_0_OffCommandFunc()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InputAux_0_OnCommandFunc()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        private void ExecuteCommand(object parameter)
        {
            if (parameter is E84Execute param)
            {
                bool flag = param.Flag;
                object Type = param.Type;

                if (this.SimulMode == E84SimulMode.MANUAL)
                {
                    var sim = this.Simulator;

                    if (sim != null)
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], ExecuteCommand() : Type = {Type.ToString()}, Flag = {flag}");

                        if (Type is E84SignalInputIndex)
                        {
                            switch (Type)
                            {
                                case E84SignalInputIndex.VALID:
                                    sim.E84Inputs.Valid = flag;
                                    break;
                                case E84SignalInputIndex.CS_0:
                                    sim.E84Inputs.CS0 = flag;
                                    break;
                                case E84SignalInputIndex.CS_1:
                                    sim.E84Inputs.CS1 = flag;
                                    break;
                                case E84SignalInputIndex.AM_AVBL:
                                    sim.E84Inputs.AmAvbl = flag;
                                    break;
                                case E84SignalInputIndex.TR_REQ:
                                    sim.E84Inputs.TrReq = flag;
                                    break;
                                case E84SignalInputIndex.BUSY:
                                    sim.E84Inputs.Busy = flag;
                                    break;
                                case E84SignalInputIndex.COMPT:
                                    sim.E84Inputs.Compt = flag;
                                    break;
                                case E84SignalInputIndex.CONT:
                                    sim.E84Inputs.Cont = flag;
                                    break;
                                case E84SignalInputIndex.GO:
                                    sim.E84Inputs.Go = flag;
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (Type is E84SignalOutputIndex)
                        {
                            switch (Type)
                            {
                                case E84SignalOutputIndex.L_REQ:
                                    sim.E84Outputs.LReq = flag;
                                    break;
                                case E84SignalOutputIndex.U_REQ:
                                    sim.E84Outputs.UlReq = flag;
                                    break;
                                case E84SignalOutputIndex.VA:
                                    sim.E84Outputs.Va = flag;
                                    break;
                                case E84SignalOutputIndex.READY:
                                    sim.E84Outputs.Ready = flag;
                                    break;
                                case E84SignalOutputIndex.VS_0:
                                    sim.E84Outputs.VS0 = flag;
                                    break;
                                case E84SignalOutputIndex.VS_1:
                                    sim.E84Outputs.VS1 = flag;
                                    break;
                                case E84SignalOutputIndex.HO_AVBL:
                                    sim.E84Outputs.HoAvbl = flag;
                                    break;
                                case E84SignalOutputIndex.ES:
                                    sim.E84Outputs.ES = flag;
                                    break;
                                case E84SignalOutputIndex.SELECT:
                                    sim.E84Outputs.Select = flag;
                                    break;
                                case E84SignalOutputIndex.MODE:
                                    sim.E84Outputs.Mode = flag;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], ExecuteCommand() : Simulator is null.");
                    }
                }
            }
        }

        private bool GetSimValue(E84SignalTypeEnum type)
        {
            bool retval = false;

            try
            {
                if (Simulator != null)
                {
                    switch (type)
                    {
                        case E84SignalTypeEnum.INVALID:
                        case E84SignalTypeEnum.UNDEFINED:
                            // NOTHING
                            break;
                        case E84SignalTypeEnum.L_REQ:
                            retval = Simulator.E84Outputs.LReq;
                            break;
                        case E84SignalTypeEnum.U_REQ:
                            retval = Simulator.E84Outputs.UlReq;
                            break;
                        case E84SignalTypeEnum.VA:
                            retval = Simulator.E84Outputs.Va;
                            break;
                        case E84SignalTypeEnum.READY:
                            retval = Simulator.E84Outputs.Ready;
                            break;
                        case E84SignalTypeEnum.VS_0:
                            retval = Simulator.E84Outputs.VS0;
                            break;
                        case E84SignalTypeEnum.VS_1:
                            retval = Simulator.E84Outputs.VS1;
                            break;
                        case E84SignalTypeEnum.HO_AVBL:
                            retval = Simulator.E84Outputs.HoAvbl;
                            break;
                        case E84SignalTypeEnum.ES:
                            retval = Simulator.E84Outputs.ES;
                            break;
                        case E84SignalTypeEnum.NC:
                            // NOTHING
                            break;
                        case E84SignalTypeEnum.SELECT:
                            retval = Simulator.E84Outputs.Select;
                            break;
                        case E84SignalTypeEnum.MODE:
                            retval = Simulator.E84Outputs.Mode;
                            break;
                        case E84SignalTypeEnum.GO:
                            retval = Simulator.E84Inputs.Go;
                            break;
                        case E84SignalTypeEnum.VALID:
                            retval = Simulator.E84Inputs.Valid;
                            break;
                        case E84SignalTypeEnum.CS_0:
                            retval = Simulator.E84Inputs.CS0;
                            break;
                        case E84SignalTypeEnum.CS_1:
                            retval = Simulator.E84Inputs.CS1;
                            break;
                        case E84SignalTypeEnum.AM_AVBL:
                            retval = Simulator.E84Inputs.AmAvbl;
                            break;
                        case E84SignalTypeEnum.TR_REQ:
                            retval = Simulator.E84Inputs.TrReq;
                            break;
                        case E84SignalTypeEnum.BUSY:
                            retval = Simulator.E84Inputs.Busy;
                            break;
                        case E84SignalTypeEnum.COMPT:
                            retval = Simulator.E84Inputs.Compt;
                            break;
                        case E84SignalTypeEnum.CONT:
                            retval = Simulator.E84Inputs.Cont;
                            break;
                        case E84SignalTypeEnum.V24:
                        case E84SignalTypeEnum.V24GNC:
                        case E84SignalTypeEnum.SIGCOM:
                        case E84SignalTypeEnum.FG:
                            // NOTHING
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private void CopyScriptinAutoMode()
        {
            try
            {
                if (SimulMode == E84SimulMode.AUTO)
                {
                    if (SelectedScript != null)
                    {
                        CopiedScript = SelectedScript.Copy();

                        CopiedScript.InitTrace(Simulator);

                        InitialTrace = new TraceSignal(1);

                        ObservableCollection<E84SignalTypeWithValue> e84SignalTypeWithValues = new ObservableCollection<E84SignalTypeWithValue>();

                        foreach (E84SignalTypeEnum signal in Enum.GetValues(typeof(E84SignalTypeEnum)))
                        {
                            bool flag = false;

                            switch (signal)
                            {
                                case E84SignalTypeEnum.INVALID:
                                case E84SignalTypeEnum.UNDEFINED:
                                    // NOTHING
                                    break;
                                case E84SignalTypeEnum.L_REQ:
                                    flag = Simulator.E84Outputs.LReq;
                                    break;
                                case E84SignalTypeEnum.U_REQ:
                                    flag = Simulator.E84Outputs.UlReq;
                                    break;
                                case E84SignalTypeEnum.VA:
                                    flag = Simulator.E84Outputs.Va;
                                    break;
                                case E84SignalTypeEnum.READY:
                                    flag = Simulator.E84Outputs.Ready;
                                    break;
                                case E84SignalTypeEnum.VS_0:
                                    flag = Simulator.E84Outputs.VS0;
                                    break;
                                case E84SignalTypeEnum.VS_1:
                                    flag = Simulator.E84Outputs.VS1;
                                    break;
                                case E84SignalTypeEnum.HO_AVBL:
                                    flag = Simulator.E84Outputs.HoAvbl;
                                    break;
                                case E84SignalTypeEnum.ES:
                                    flag = Simulator.E84Outputs.ES;
                                    break;
                                case E84SignalTypeEnum.NC:
                                    // NOTHING
                                    break;
                                case E84SignalTypeEnum.SELECT:
                                    flag = Simulator.E84Outputs.Select;
                                    break;
                                case E84SignalTypeEnum.MODE:
                                    flag = Simulator.E84Outputs.Mode;
                                    break;
                                case E84SignalTypeEnum.GO:
                                    flag = Simulator.E84Inputs.Go;
                                    break;
                                case E84SignalTypeEnum.VALID:
                                    flag = Simulator.E84Inputs.Valid;
                                    break;
                                case E84SignalTypeEnum.CS_0:
                                    flag = Simulator.E84Inputs.CS0;
                                    break;
                                case E84SignalTypeEnum.CS_1:
                                    flag = Simulator.E84Inputs.CS1;
                                    break;
                                case E84SignalTypeEnum.AM_AVBL:
                                    flag = Simulator.E84Inputs.AmAvbl;
                                    break;
                                case E84SignalTypeEnum.TR_REQ:
                                    flag = Simulator.E84Inputs.TrReq;
                                    break;
                                case E84SignalTypeEnum.BUSY:
                                    flag = Simulator.E84Inputs.Busy;
                                    break;
                                case E84SignalTypeEnum.COMPT:
                                    flag = Simulator.E84Inputs.Compt;
                                    break;
                                case E84SignalTypeEnum.CONT:
                                    flag = Simulator.E84Inputs.Cont;
                                    break;
                                case E84SignalTypeEnum.V24:
                                case E84SignalTypeEnum.V24GNC:
                                case E84SignalTypeEnum.SIGCOM:
                                case E84SignalTypeEnum.FG:
                                    // NOTHING
                                    break;
                                default:
                                    break;
                            }
                            e84SignalTypeWithValues.Add(new E84SignalTypeWithValue(signal, flag));
                        }

                        InitialTrace.UpdateSignalSet(0, e84SignalTypeWithValues);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void OnSimulModeChanged()
        {
            try
            {
                SimulMode = (E84SimulMode)SelectedTapIndex;

                CopyScriptinAutoMode();

                //AdjustWidth();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //private void AdjustWidth()
        //{
        //    try
        //    {
        //        if (SimulMode != E84SimulMode.MANUAL)
        //        {
        //            var actualwidth = this.ViewObj.window.ActualWidth;

        //            if (this.ViewObj.grs != null && actualwidth != 0)
        //            {
        //                // 원하는 너비 값 계산
        //                double newWidth = actualwidth / 2.5;
        //                this.ViewObj.GC1.Width = new GridLength(newWidth);
        //                this.ViewObj.GC3.Width = new GridLength(actualwidth - newWidth - this.ViewObj.grs.ActualWidth);
        //            }
        //        }
        //        else
        //        {
        //            this.ViewObj.GC1.Width = new GridLength(9, GridUnitType.Star);
        //            this.ViewObj.GC3.Width = GridLength.Auto;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        private async void ScriptStartCommandFunc()
        {
            try
            {
                if (E84Info.E84Controller.CommModule.RunMode == E84Mode.AUTO)
                {
                    if (SelectedScript != null)
                    {
                        if (Simulator != null)
                        {
                            if (IsReset)
                            {
                                Simulator.SignalReset();

                                ResetCharts();

                                Thread.Sleep(100);
                            }

                            // Create a deep copy of the selected script
                            CopiedScript = SelectedScript.Copy();

                            // Assuming you have the required input and output objects
                            bool result = await CopiedScript.Run(Simulator, Inputs, Outputs);
                        }
                    }
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog($"ERROR", $"Can only be operated in Auto mode. Mode is {E84Info.E84Controller.CommModule.RunMode}", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void ScriptStopCommandFunc()
        {
            try
            {
                if (CopiedScript != null)
                {
                    if (CopiedScript.IsRun)
                    {
                        if (Simulator != null)
                        {
                            CopiedScript.Stop(Simulator);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void SelectedScriptChangedCommandFunc()
        {
            try
            {
                CopyScriptinAutoMode();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ScrollViewer ScrollViewer { get; set; }
        private void SetChartScrollViewerCommandFunc(object parameter)
        {
            try
            {
                if (ScrollViewer == null)
                {
                    if (parameter is ListView listView)
                    {
                        var scrollViewer = GetScrollViewerFromListView(listView);

                        if (scrollViewer != null)
                        {
                            ScrollViewer = scrollViewer;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ScrollViewer GetScrollViewerFromListView(DependencyObject depObj)
        {
            ScrollViewer retval = null;

            try
            {
                if (depObj is ScrollViewer)
                {
                    retval = depObj as ScrollViewer;
                }
                else
                {
                    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                    {
                        var child = VisualTreeHelper.GetChild(depObj, i);

                        var result = GetScrollViewerFromListView(child);

                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void UpdateDateTimeRange()
        {
            try
            {
                var allPoints = Charts.SelectMany(x => ((StepLineSeries)x.Series[0]).Values.Cast<ObservablePoint>());

                double min = allPoints.Min(p => p.X);
                double max = allPoints.Max(p => p.X);

                foreach (var item in Charts)
                {
                    item.MinDateTimeValue = min;
                    item.MaxDateTimeValue = max;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ChangedSignal(string signalName, bool value)
        {
            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}], ChangedSignal() : Signal = {signalName}, Value = {value}");

                double d = value ? 1 : 0;
                long currentTime = DateTime.UtcNow.Ticks;

                // Initialize all series colors to default (gray in this case)
                foreach (var item in Charts)
                {
                    if (item.LastValue == 0)
                    {
                        Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            ((StepLineSeries)item.Series[0]).Stroke = OffBrush;
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            ((StepLineSeries)item.Series[0]).Stroke = OnBrush;
                        });
                    }
                }

                List<Tuple<E84Chart, long, double>> accumulatedData = new List<Tuple<E84Chart, long, double>>();
                E84Chart matchedChart = null;

                foreach (var Chart in Charts)
                {
                    StepLineSeries series = (StepLineSeries)Chart.Series[0];
                    ObservablePoint lastPoint = series.Values.Count > 0 ? series.Values[series.Values.Count - 1] as ObservablePoint : null;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        double newValue = series.Title == signalName ? d : (lastPoint != null ? lastPoint.Y : 0);
                        accumulatedData.Add(Tuple.Create(Chart, currentTime, newValue));

                        // Assign the chart to the matched chart if the title matches.
                        if (series.Title == signalName)
                        {
                            matchedChart = Chart;
                        }
                    });
                }

                foreach (var data in accumulatedData)
                {

                    var series = (StepLineSeries)data.Item1.Series[0];
                    series.Values.Insert(series.Values.Count, new ObservablePoint(data.Item2, data.Item3));
                }

                // Update the LastValue of the matched chart, if any.
                if (matchedChart != null)
                {
                    matchedChart.LastValue = d;
                }

                // Update
                var ip = Inputs.FirstOrDefault(x => x.Type.ToString() == signalName);
                var op = Outputs.FirstOrDefault(x => x.Type.ToString() == signalName);

                if (ip != null)
                {
                    ip.CurrentValue = value;
                }

                if (op != null)
                {
                    op.CurrentValue = value;
                }

                UpdateDateTimeRange();
                UpdateScrollPosition();

                if (CopiedScript != null)
                {
                    if (CopiedScript.IsRun != true)
                    {
                        if (this.InitialTrace != null)
                        {
                            E84SignalTypeEnum type;

                            if (Enum.TryParse<E84SignalTypeEnum>(signalName, out var enumValue))
                            {
                                type = enumValue;
                            }
                            else
                            {
                                type = E84SignalTypeEnum.INVALID;
                            }

                            if (type != E84SignalTypeEnum.INVALID)
                            {
                                E84SignalTypeWithValue e84SignalTypeWithValue = new E84SignalTypeWithValue(type, value);
                                this.InitialTrace.UpdateSignal(0, e84SignalTypeWithValue);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void UpdateScrollPosition()
        {
            try
            {
                if (ScrollViewer != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ScrollViewer.ScrollToHorizontalOffset(double.MaxValue);
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public static ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            ScrollViewer retval = null;

            try
            {
                if (depObj is ScrollViewer viewer)
                {
                    retval = viewer;
                    return retval;
                }

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    var result = GetScrollViewer(child);
                    if (result != null)
                    {
                        retval = result;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void ChangeZoom(MouseWheelEventArgs e)
        {
            try
            {
                double minX = Charts[0].MinDateTimeValue;
                double maxX = Charts[0].MaxDateTimeValue;

                double zoomFactor = 0.1; // 10% zoom for each scroll
                double range = maxX - minX;

                if (e.Delta > 0) // Zoom in
                {
                    minX += range * zoomFactor;
                    maxX -= range * zoomFactor;
                }
                else // Zoom out
                {
                    minX -= range * zoomFactor;
                    maxX += range * zoomFactor;
                }

                foreach (var item in Charts)
                {
                    item.MinDateTimeValue = minX;
                    item.MaxDateTimeValue = maxX;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
