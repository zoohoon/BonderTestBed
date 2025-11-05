using System;

namespace ProberDevelopPackWindow.Tab
{
    using Autofac;
    using LoaderBase;
    using LoaderMaster;
    using LogModule;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class GPEnvControlViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion



        public GPEnvControlViewModel()
        {
            try
            {
                InitViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region <remark> GP Valve Tab </remark>

        #region <!-- Property -->
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

        private void InitViewModel()
        {
            try
            {
                if (this.GetLoaderContainer() == null)
                {
                    return;
                }

                Master = this.GetLoaderContainer().Resolve<ILoaderSupervisor>() as LoaderSupervisor;
                if (Master == null)
                {
                    return;
                }
                LoaderBase.IGPLoaderCommands gpLoader = Master.Loader.GetLoaderCommands();
                IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;

                Cells = new ObservableCollection<StageValveState>();
                for (int index = 1; index <= 12; index++)
                {
                    int stageIdx = 0;
                    switch (index)
                    {
                        case 1:
                            stageIdx = 9; break;
                        case 2:
                            stageIdx = 10; break;
                        case 3:
                            stageIdx = 11; break;
                        case 4:
                            stageIdx = 12; break;
                        case 5:
                            stageIdx = 5; break;
                        case 6:
                            stageIdx = 6; break;
                        case 7:
                            stageIdx = 7; break;
                        case 8:
                            stageIdx = 8; break;
                        case 9:
                            stageIdx = 1; break;
                        case 10:
                            stageIdx = 2; break;
                        case 11:
                            stageIdx = 2; break;
                        case 12:
                            stageIdx = 4; break;
                        default:
                            break;
                    }

                    var invalvestate = loadercommand.CoolantInletValveStates[stageIdx - 1];
                    var outvalvestate = loadercommand.CoolantOutletValveStates[stageIdx - 1];
                    var dryairvalvestate = loadercommand.DryAirValveStates[stageIdx - 1];
                    var purgevalvestate = loadercommand.PurgeValveStates[stageIdx - 1];
                    var drainairvalvestate = loadercommand.DrainValveStates[stageIdx - 1];
                    var leakvalvestate = loadercommand.CoolantLeaks[stageIdx - 1];

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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region <!-- InValveOn & Off Command -->

        private RelayCommand<object> _InValveOnCommand;
        public ICommand InValveOnCommand
        {
            get
            {
                if (null == _InValveOnCommand) _InValveOnCommand = new RelayCommand<object>(InValveOnCommandFunc);
                return _InValveOnCommand;
            }
        }
        private void InValveOnCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = true;
                EnumValveType valveType = EnumValveType.IN;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _InValveOffCommand;
        public ICommand InValveOffCommand
        {
            get
            {
                if (null == _InValveOffCommand) _InValveOffCommand = new RelayCommand<object>(InValveOffCommandFunc);
                return _InValveOffCommand;
            }
        }
        private void InValveOffCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = false;
                EnumValveType valveType = EnumValveType.IN;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <!-- OutVlaveOn & Off Command -->

        private RelayCommand<object> _OutValveOnCommand;
        public ICommand OutValveOnCommand
        {
            get
            {
                if (null == _OutValveOnCommand) _OutValveOnCommand = new RelayCommand<object>(OutValveOnCommandFunc);
                return _OutValveOnCommand;
            }
        }
        private void OutValveOnCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = true;
                EnumValveType valveType = EnumValveType.OUT;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _OutValveOffCommand;
        public ICommand OutValveOffCommand
        {
            get
            {
                if (null == _OutValveOffCommand) _OutValveOffCommand = new RelayCommand<object>(OutValveOffCommandFunc);
                return _OutValveOffCommand;
            }
        }
        private void OutValveOffCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = false;
                EnumValveType valveType = EnumValveType.OUT;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region <!-- DryAir VlaveOn & Off Command -->

        private RelayCommand<object> _DryAirValveOnCommand;
        public ICommand DryAirValveOnCommand
        {
            get
            {
                if (null == _DryAirValveOnCommand) _DryAirValveOnCommand = new RelayCommand<object>(DryAirValveOnCommandFunc);
                return _DryAirValveOnCommand;
            }
        }
        private void DryAirValveOnCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = true;
                EnumValveType valveType = EnumValveType.DRYAIR;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _DryAirValveOffCommand;
        public ICommand DryAirValveOffCommand
        {
            get
            {
                if (null == _DryAirValveOffCommand) _DryAirValveOffCommand = new RelayCommand<object>(DryAirValveOffCommandFunc);
                return _DryAirValveOffCommand;
            }
        }
        private void DryAirValveOffCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = false;
                EnumValveType valveType = EnumValveType.DRYAIR;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region <!-- Purge VlaveOn & Off Command -->

        private RelayCommand<object> _PurgeValveOnCommand;
        public ICommand PurgeValveOnCommand
        {
            get
            {
                if (null == _PurgeValveOnCommand) _PurgeValveOnCommand = new RelayCommand<object>(PurgeValveOnCommandFunc);
                return _PurgeValveOnCommand;
            }
        }
        private void PurgeValveOnCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = true;
                EnumValveType valveType = EnumValveType.PURGE;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _PurgeValveOffCommand;
        public ICommand PurgeValveOffCommand
        {
            get
            {
                if (null == _PurgeValveOffCommand) _PurgeValveOffCommand = new RelayCommand<object>(PurgeValveOffCommandFunc);
                return _PurgeValveOffCommand;
            }
        }
        private void PurgeValveOffCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = false;
                EnumValveType valveType = EnumValveType.PURGE;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region <!-- DrainAir VlaveOn & Off Command -->

        private RelayCommand<object> _DrainAirValveOnCommand;
        public ICommand DrainAirValveOnCommand
        {
            get
            {
                if (null == _DrainAirValveOnCommand) _DrainAirValveOnCommand = new RelayCommand<object>(DrainAirValveOnCommandFunc);
                return _DrainAirValveOnCommand;
            }
        }
        private void DrainAirValveOnCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = true;
                EnumValveType valveType = EnumValveType.DRAIN;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _DrainAirValveOffCommand;
        public ICommand DrainAirValveOffCommand
        {
            get
            {
                if (null == _DrainAirValveOffCommand) _DrainAirValveOffCommand = new RelayCommand<object>(DrainAirValveOffCommandFunc);
                return _DrainAirValveOffCommand;
            }
        }
        private void DrainAirValveOffCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = false;
                EnumValveType valveType = EnumValveType.DRAIN;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region <!-- Leak VlaveOn & Off Command -->

        private RelayCommand<object> _LeakValveOnCommand;
        public ICommand LeakValveOnCommand
        {
            get
            {
                if (null == _LeakValveOnCommand) _LeakValveOnCommand = new RelayCommand<object>(LeakValveOnCommandFunc);
                return _LeakValveOnCommand;
            }
        }
        private void LeakValveOnCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = true;
                EnumValveType valveType = EnumValveType.Leak;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
                int index = (int)obj; ;
                bool state = false;
                EnumValveType valveType = EnumValveType.Leak;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #endregion
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
