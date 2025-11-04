using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EnvControlWindow.GP
{
    using LogModule;
    using LoaderMaster;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using ProberInterfaces;
    using RelayCommandBase;
    using LoaderBase;
    using System.Threading;

    /// <summary>
    /// Interaction logic for GPEnvControlMainWindow.xaml
    /// </summary>
    public partial class GPEnvControlMainWindow : Window, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public GPEnvControlMainWindow()
        {
            InitializeComponent();
        }

        public GPEnvControlMainWindow(LoaderSupervisor master) : this()
        {
            try
            {
                new Thread(() =>
                {
                    Master = master;
                    //IGPLoader gpLoader = Master.GetGPLoader();
                    LoaderBase.IGPLoaderCommands gpLoader = Master.Loader.GetLoaderCommands();
                    IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;

                    Cells = new ObservableCollection<StageValveState>();
                    for (int index = 1; index <= 12; index++)
                    {

                        var invalvestate = master.EnvControlManager().GetValveState(EnumValveType.IN, index); //loadercommand.CoolantInletValveStates[stageIdx - 1];
                        var outvalvestate = master.EnvControlManager().GetValveState(EnumValveType.OUT, index); //loadercommand.CoolantOutletValveStates[stageIdx - 1];
                        var dryairvalvestate = master.EnvControlManager().GetValveState(EnumValveType.DRYAIR, index); //loadercommand.DryAirValveStates[stageIdx - 1];
                        var purgevalvestate = master.EnvControlManager().GetValveState(EnumValveType.MANUAL_PURGE, index); //loadercommand.PurgeValveStates[stageIdx - 1];
                        var drainairvalvestate = master.EnvControlManager().GetValveState(EnumValveType.DRAIN, index); //loadercommand.DrainValveStates[stageIdx - 1];
                        var leakvalvestate = master.EnvControlManager().GetValveState(EnumValveType.Leak, index);  //loadercommand.CoolantLeaks[stageIdx - 1];

                        Cells.Add(new StageValveState()
                        {
                            Index = index,
                            CoolantInValveState = invalvestate,
                            CoolantOutValveState = outvalvestate,
                            DryAirValveState = dryairvalvestate,
                            PurgeValveState = purgevalvestate,
                            DraninValveState = drainairvalvestate,
                            LeakValveState = leakvalvestate
                        });
                    }
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        DataContext = this;
                    });

                }).Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region ..Property
        private LoaderSupervisor Master;

        private ObservableCollection<StageValveState> _Cells
             = new ObservableCollection<StageValveState>();
        public ObservableCollection<StageValveState> Cells
        {
            get { return _Cells; }
            set
            {
                if (value != _Cells)
                {
                    _Cells = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region ..InValveOn & Off Command

        private AsyncCommand<object> _InValveOnCommand;
        public ICommand InValveOnCommand
        {
            get
            {
                if (null == _InValveOnCommand) _InValveOnCommand = new AsyncCommand<object>(InValveOnCommandFunc);
                return _InValveOnCommand;
            }
        }
        private Task InValveOnCommandFunc(object obj)
        {
            try
            {
                
                Master.EnvControlManager().SetValveState(true,EnumValveType.IN, (int)obj);
                //IGPLoader gpLoader = Master.GetGPLoader();
                //LoaderBase.IGPLoaderCommands gpLoader = Master.Loader.GetLoaderCommands();
                //IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;
                //loadercommand.CoolantInletValveControl((int)obj, true);
                LoggerManager.Debug($"Cell#{(int)obj} : InValve On");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private AsyncCommand<object> _InValveOffCommand;
        public ICommand InValveOffCommand
        {
            get
            {
                if (null == _InValveOffCommand) _InValveOffCommand = new AsyncCommand<object>(InValveOffCommandFunc);
                return _InValveOffCommand;
            }
        }
        private Task InValveOffCommandFunc(object obj)
        {
            try
            {
                Master.EnvControlManager().SetValveState(false, EnumValveType.IN, (int)obj);
                //IGPLoader gpLoader = Master.GetGPLoader();
                //LoaderBase.IGPLoaderCommands gpLoader = Master.Loader.GetLoaderCommands();
                //IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;
                //loadercommand.CoolantInletValveControl((int)obj, false);
                LoggerManager.Debug($"Cell#{(int)obj} : InValve Off");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion

        #region .. OutVlaveOn & Off Command

        private AsyncCommand<object> _OutValveOnCommand;
        public ICommand OutValveOnCommand
        {
            get
            {
                if (null == _OutValveOnCommand) _OutValveOnCommand = new AsyncCommand<object>(OutValveOnCommandFunc);
                return _OutValveOnCommand;
            }
        }
        private Task OutValveOnCommandFunc(object obj)
        {
            try
            {
                Master.EnvControlManager().SetValveState(true, EnumValveType.OUT, (int)obj);
                //IGPLoader gpLoader = Master.GetGPLoader();
                //LoaderBase.IGPLoaderCommands gpLoader = Master.Loader.GetLoaderCommands();
                //IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;
                //loadercommand.CoolantOutletValveControl((int)obj, true);
                LoggerManager.Debug($"Cell#{(int)obj} : OutValve On");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        private AsyncCommand<object> _OutValveOffCommand;
        public ICommand OutValveOffCommand
        {
            get
            {
                if (null == _OutValveOffCommand) _OutValveOffCommand = new AsyncCommand<object>(OutValveOffCommandFunc);
                return _OutValveOffCommand;
            }
        }
        private Task OutValveOffCommandFunc(object obj)
        {
            try
            {
                Master.EnvControlManager().SetValveState(false, EnumValveType.OUT, (int)obj);
                //IGPLoader gpLoader = Master.GetGPLoader();
                //LoaderBase.IGPLoaderCommands gpLoader = Master.Loader.GetLoaderCommands();
                //IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;
                //loadercommand.CoolantOutletValveControl((int)obj, false);
                LoggerManager.Debug($"Cell#{(int)obj} : OutValve Off");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        #endregion

        #region ..DryAir VlaveOn & Off Command

        private AsyncCommand<object> _DryAirValveOnCommand;
        public ICommand DryAirValveOnCommand
        {
            get
            {
                if (null == _DryAirValveOnCommand) _DryAirValveOnCommand = new AsyncCommand<object>(DryAirValveOnCommandFunc);
                return _DryAirValveOnCommand;
            }
        }
        private Task DryAirValveOnCommandFunc(object obj)
        {
            try
            {
                Master.EnvControlManager().SetValveState(true, EnumValveType.DRYAIR, (int)obj);
                //IGPLoader gpLoader = Master.GetGPLoader();
                //LoaderBase.IGPLoaderCommands gpLoader = Master.Loader.GetLoaderCommands();
                //IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;
                //loadercommand.DryAirValveControl((int)obj, true);
                LoggerManager.Debug($"Cell#{(int)obj} : DryAirValve On");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        private AsyncCommand<object> _DryAirValveOffCommand;
        public ICommand DryAirValveOffCommand
        {
            get
            {
                if (null == _DryAirValveOffCommand) _DryAirValveOffCommand = new AsyncCommand<object>(DryAirValveOffCommandFunc);
                return _DryAirValveOffCommand;
            }
        }
        private Task DryAirValveOffCommandFunc(object obj)
        {
            try
            {
                Master.EnvControlManager().SetValveState(false, EnumValveType.DRYAIR, (int)obj);
                //IGPLoader gpLoader = Master.GetGPLoader();
                //LoaderBase.IGPLoaderCommands gpLoader = Master.Loader.GetLoaderCommands();
                //IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;
                //loadercommand.DryAirValveControl((int)obj, false);
                LoggerManager.Debug($"Cell#{(int)obj} : DryAirValve Off");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        #endregion

        #region ..Purge VlaveOn & Off Command

        private AsyncCommand<object> _PurgeValveOnCommand;
        public ICommand PurgeValveOnCommand
        {
            get
            {
                if (null == _PurgeValveOnCommand) _PurgeValveOnCommand = new AsyncCommand<object>(PurgeValveOnCommandFunc);
                return _PurgeValveOnCommand;
            }
        }
        private Task PurgeValveOnCommandFunc(object obj)
        {
            try
            {
                //Master.EnvControlManager().GetValveState(EnumValveType.PRUGE, (int)obj);
                Master.EnvControlManager().SetValveState(true, EnumValveType.PURGE, (int)obj);
                //IGPLoader gpLoader = Master.GetGPLoader();
                //LoaderBase.IGPLoaderCommands gpLoader = Master.Loader.GetLoaderCommands();
                //IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;
                //loadercommand.PurgeValveControl((int)obj, true);
                LoggerManager.Debug($"Cell#{(int)obj} : PurgeValve On");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }


        private AsyncCommand<object> _PurgeValveOffCommand;
        public ICommand PurgeValveOffCommand
        {
            get
            {
                if (null == _PurgeValveOffCommand) _PurgeValveOffCommand = new AsyncCommand<object>(PurgeValveOffCommandFunc);
                return _PurgeValveOffCommand;
            }
        }
        private Task PurgeValveOffCommandFunc(object obj)
        {
            try
            {
                //Master.EnvControlManager().GetValveState(EnumValveType.PRUGE, (int)obj);
                Master.EnvControlManager().SetValveState(false, EnumValveType.PURGE, (int)obj);
                //IGPLoader gpLoader = Master.GetGPLoader();
                //LoaderBase.IGPLoaderCommands gpLoader = Master.Loader.GetLoaderCommands();
                //IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;
                //loadercommand.PurgeValveControl((int)obj, false);
                LoggerManager.Debug($"Cell#{(int)obj} : PurgeValve Off");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        #endregion

        #region ..Manual Pruge VlaveOn & Off Command

        private AsyncCommand<object> _ManualPurgeValveOnCommand;
        public ICommand ManualPurgeValveOnCommand
        {
            get
            {
                if (null == _ManualPurgeValveOnCommand) _ManualPurgeValveOnCommand = new AsyncCommand<object>(ManualPurgeValveOnCommandFunc);
                return _ManualPurgeValveOnCommand;
            }
        }
        private Task ManualPurgeValveOnCommandFunc(object obj)
        {
            try
            {
                Master.EnvControlManager().SetValveState(true, EnumValveType.MANUAL_PURGE, (int)obj);
                LoggerManager.Debug($"Cell#{(int)obj} : PurgeValve On");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }


        private AsyncCommand<object> _ManualPurgeValveOffCommand;
        public ICommand ManualPurgeValveOffCommand
        {
            get
            {
                if (null == _ManualPurgeValveOffCommand) _ManualPurgeValveOffCommand = new AsyncCommand<object>(ManualPurgeValveOffCommandFunc);
                return _ManualPurgeValveOffCommand;
            }
        }
        private Task ManualPurgeValveOffCommandFunc(object obj)
        {
            try
            {
                Master.EnvControlManager().SetValveState(false, EnumValveType.MANUAL_PURGE, (int)obj);
                LoggerManager.Debug($"Cell#{(int)obj} : PurgeValve Off");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        #endregion

        #region ..DrainAir VlaveOn & Off Command

        private AsyncCommand<object> _DrainAirValveOnCommand;
        public ICommand DrainAirValveOnCommand
        {
            get
            {
                if (null == _DrainAirValveOnCommand) _DrainAirValveOnCommand = new AsyncCommand<object>(DrainAirValveOnCommandFunc);
                return _DrainAirValveOnCommand;
            }
        }
        private Task DrainAirValveOnCommandFunc(object obj)
        {
            try
            {
                Master.EnvControlManager().GetValveState(EnumValveType.DRAIN, (int)obj);
                Master.EnvControlManager().SetValveState(true, EnumValveType.DRAIN, (int)obj);
                //IGPLoader gpLoader = Master.GetGPLoader();
                //LoaderBase.IGPLoaderCommands gpLoader = Master.Loader.GetLoaderCommands();
                //IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;
                //loadercommand.DrainValveControl((int)obj, true);
                LoggerManager.Debug($"Cell#{(int)obj} : DraningValve On");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        private AsyncCommand<object> _DrainAirValveOffCommand;
        public ICommand DrainAirValveOffCommand
        {
            get
            {
                if (null == _DrainAirValveOffCommand) _DrainAirValveOffCommand = new AsyncCommand<object>(DrainAirValveOffCommandFunc);
                return _DrainAirValveOffCommand;
            }
        }
        private Task DrainAirValveOffCommandFunc(object obj)
        {
            try
            {
                Master.EnvControlManager().GetValveState(EnumValveType.DRAIN, (int)obj);
                Master.EnvControlManager().SetValveState(false, EnumValveType.DRAIN, (int)obj);
                //IGPLoader gpLoader = Master.GetGPLoader();
                //LoaderBase.IGPLoaderCommands gpLoader = Master.Loader.GetLoaderCommands();
                //IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;
                //loadercommand.DrainValveControl((int)obj, false);
                LoggerManager.Debug($"Cell#{(int)obj} : DraningValve Off");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        #endregion

        #region ..DrainAir VlaveOn & Off Command

        private AsyncCommand<object> _LeakValveOnCommand;
        public ICommand LeakValveOnCommand
        {
            get
            {
                if (null == _LeakValveOnCommand) _LeakValveOnCommand = new AsyncCommand<object>(LeakValveOnCommandFunc);
                return _LeakValveOnCommand;
            }
        }
        private Task LeakValveOnCommandFunc(object obj)
        {
            try
            {
                Master.EnvControlManager().GetValveState(EnumValveType.Leak, (int)obj);
                Master.EnvControlManager().SetValveState(true, EnumValveType.Leak, (int)obj);
                LoggerManager.Debug($"Cell#{(int)obj} : DraningValve On");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        private RelayCommand<object> _LeakValveOffCommand;
        public ICommand LeakValveOffCommand
        {
            get
            {
                if (null == _LeakValveOffCommand) _LeakValveOffCommand = new RelayCommand<object>(LeakValveOffCommandFunc);
                return _LeakValveOffCommand;
            }
        }
        private void LeakValveOffCommandFunc(object obj)
        {
            try
            {
                Master.EnvControlManager().GetValveState(EnumValveType.Leak, (int)obj);
                Master.EnvControlManager().SetValveState(false, EnumValveType.Leak, (int)obj);
                LoggerManager.Debug($"Cell#{(int)obj} : DraningValve Off");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        #endregion

        public void UpdateState()
        {
            try
            {
                //IGPLoader gpLoader = Master.GetGPLoader();
                LoaderBase.IGPLoaderCommands gpLoader = Master.Loader.GetLoaderCommands();
                IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;
                foreach (var cell in Cells)
                {
                    cell.CoolantInValveState = loadercommand.CoolantInletValveStates[cell.Index - 1];
                    cell.CoolantOutValveState = loadercommand.CoolantOutletValveStates[cell.Index - 1];
                    cell.DryAirValveState = loadercommand.DryAirValveStates[cell.Index - 1];
                    cell.PurgeValveState = loadercommand.PurgeValveStates[cell.Index - 1];
                    cell.DraninValveState = loadercommand.DrainValveStates[cell.Index - 1];
                    cell.LeakValveState = loadercommand.CoolantLeaks[cell.Index - 1];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
}




    public class StageValveState : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CoolantInValveState;
        public bool CoolantInValveState
        {
            get { return _CoolantInValveState; }
            set
            {
                if (value != _CoolantInValveState)
                {
                    _CoolantInValveState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CoolantOutValveState;
        public bool CoolantOutValveState
        {
            get { return _CoolantOutValveState; }
            set
            {
                if (value != _CoolantOutValveState)
                {
                    _CoolantOutValveState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _DryAirValveState;
        public bool DryAirValveState
        {
            get { return _DryAirValveState; }
            set
            {
                if (value != _DryAirValveState)
                {
                    _DryAirValveState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _PurgeValveState;
        public bool PurgeValveState
        {
            get { return _PurgeValveState; }
            set
            {
                if (value != _PurgeValveState)
                {
                    _PurgeValveState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ManualPurgeValveState;
        public bool ManualPurgeValveState
        {
            get { return _ManualPurgeValveState; }
            set
            {
                if (value != _ManualPurgeValveState)
                {
                    _ManualPurgeValveState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _DraninValveState;
        public bool DraninValveState
        {
            get { return _DraninValveState; }
            set
            {
                if (value != _DraninValveState)
                {
                    _DraninValveState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _LeakValveState;
        public bool LeakValveState
        {
            get { return _LeakValveState; }
            set
            {
                if (value != _LeakValveState)
                {
                    _LeakValveState = value;
                    RaisePropertyChanged();
                }
            }
        }

    }
}
