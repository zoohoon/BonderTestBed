

namespace ProberDevelopPackWindow.Tab
{
    using Autofac;
    using LogModule;
    using PIVManagerModule;
    using ProberInterfaces;
    using RelayCommandBase;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    public class PIVSettingViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <!-- Property -->

        private PIVManager _PIVManager { get;set; }

        private PIVParameter _PIVParam;
        public PIVParameter PIVParam
        {
            get { return _PIVParam; }
            set
            {
                if (value != _PIVParam)
                {
                    _PIVParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IEnumerable<GEMFoupStateEnum> _FoupStateEnums;
        public IEnumerable<GEMFoupStateEnum> FoupStateEnums
        {
            get { return _FoupStateEnums; }
            set {_FoupStateEnums = value; }
        }

        private IEnumerable<GEMStageStateEnum> _StageStateEnums;

        public IEnumerable<GEMStageStateEnum> StageStateEnums
        {
            get { return _StageStateEnums; }
            set { _StageStateEnums = value; }
        }

        private IEnumerable<GEMPreHeatStateEnum> _PreHeatStateEnums;

        public IEnumerable<GEMPreHeatStateEnum> PreHeatStateEnums
        {
            get { return _PreHeatStateEnums; }
            set { _PreHeatStateEnums = value; }
        }

        private GEMFoupStateEnum _SelectedFoupState;
        public GEMFoupStateEnum SelectedFoupState
        {
            get { return _SelectedFoupState; }
            set
            {
                if (value != _SelectedFoupState)
                {
                    _SelectedFoupState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private GEMFoupStateEnum _SelectedFoupStateEnum;
        public GEMFoupStateEnum SelectedFoupStateEnum
        {
            get { return _SelectedFoupStateEnum; }
            set
            {
                if (value != _SelectedFoupStateEnum)
                {
                    _SelectedFoupStateEnum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private GEMStageStateEnum _SelectedStageState;
        public GEMStageStateEnum SelectedStageState
        {
            get { return _SelectedStageState; }
            set
            {
                if (value != _SelectedStageState)
                {
                    _SelectedStageState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private GEMStageStateEnum _SelectedStageStateEnum;
        public GEMStageStateEnum SelectedStageStateEnum
        {
            get { return _SelectedStageStateEnum; }
            set
            {
                if (value != _SelectedStageStateEnum)
                {
                    _SelectedStageStateEnum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private GEMPreHeatStateEnum _SelectedPreHeatState;
        public GEMPreHeatStateEnum SelectedPreHeatState
        {
            get { return _SelectedPreHeatState; }
            set
            {
                if (value != _SelectedPreHeatState)
                {
                    _SelectedPreHeatState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private GEMPreHeatStateEnum _SelectedPreHeatStateEnum;
        public GEMPreHeatStateEnum SelectedPreHeatStateEnum
        {
            get { return _SelectedPreHeatStateEnum; }
            set
            {
                if (value != _SelectedPreHeatStateEnum)
                {
                    _SelectedPreHeatStateEnum = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        public PIVSettingViewModel()
        {
            InitViewModel();
        }

        private void InitViewModel()
        {
            try
            {
                IPIVManager pivManager = null;
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                    pivManager = this.GetContainer()?.Resolve<IPIVManager>();
                else if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                    pivManager = this.GetLoaderContainer()?.Resolve<IPIVManager>();
                
                if(pivManager != null)
                {
                    _PIVManager = pivManager as PIVManager;
                    PIVParam = _PIVManager.PIVSysParam;
                }

                FoupStateEnums = Enum.GetValues(typeof(GEMFoupStateEnum)).Cast<GEMFoupStateEnum>();
                StageStateEnums = Enum.GetValues(typeof(GEMStageStateEnum)).Cast<GEMStageStateEnum>();
                PreHeatStateEnums = Enum.GetValues(typeof(GEMPreHeatStateEnum)).Cast<GEMPreHeatStateEnum>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<object> _AddCommand;
        public ICommand AddCommand
        {
            get
            {
                if (null == _AddCommand) _AddCommand = new RelayCommand<object>(AddCommandFunc);
                return _AddCommand;
            }
        }
        private void AddCommandFunc(object obj)
        {
            try
            {
                if(obj.ToString().Equals("FOUP"))
                {
                    if(PIVParam.FoupStates.SingleOrDefault(Sparam => Sparam.State == SelectedFoupStateEnum) == null)
                    {
                        PIVParam.FoupStates.Add(new FoupStateConvertParam(SelectedFoupStateEnum, -1, true));
                    }
                }
                else if(obj.ToString().Equals("STAGE"))
                {
                    if (PIVParam.StageStates.SingleOrDefault(Sparam => Sparam.State == SelectedStageStateEnum) == null)
                    {
                        PIVParam.StageStates.Add(new StageStateConvertParam(SelectedStageStateEnum, -1, true));
                    }
                }
                else if(obj.ToString().Equals("PREHEAT"))
                {
                    if (PIVParam.PreHeatStates.SingleOrDefault(Sparam => Sparam.State == SelectedPreHeatStateEnum) == null)
                    {
                        PIVParam.PreHeatStates.Add(new PreHeatStateConvertParam(SelectedPreHeatStateEnum, -1, true));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _DeleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (null == _DeleteCommand) _DeleteCommand = new RelayCommand<object>(DeleteCommandFunc);
                return _DeleteCommand;
            }
        }
        private void DeleteCommandFunc(object obj)
        {
            try
            {
                if (obj.ToString().Equals("FOUP"))
                {
                    var retstate = PIVParam.FoupStates.SingleOrDefault(Sparam => Sparam.State == SelectedFoupState);
                    if (retstate != null)
                    {
                        PIVParam.FoupStates.Remove(retstate);
                    }
                }
                else if (obj.ToString().Equals("STAGE"))
                {
                    var retstate = PIVParam.StageStates.SingleOrDefault(Sparam => Sparam.State == SelectedStageState);
                    if (retstate != null)
                    {
                        PIVParam.StageStates.Remove(retstate);
                    }
                }
                else if (obj.ToString().Equals("PREHEAT"))
                {
                    var retstate = PIVParam.PreHeatStates.SingleOrDefault(Sparam => Sparam.State == SelectedPreHeatState);
                    if (retstate != null)
                    {
                        PIVParam.PreHeatStates.Remove(retstate);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<object> _ClearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (null == _ClearCommand) _ClearCommand = new RelayCommand<object>(ClearCommandFunc);
                return _ClearCommand;
            }
        }
        private void ClearCommandFunc(object obj)
        {
            try
            {
                if (obj.ToString().Equals("FOUP"))
                {
                    PIVParam.FoupStates.Clear();
                }
                else if (obj.ToString().Equals("STAGE"))
                {
                    PIVParam.StageStates.Clear();
                }
                else if (obj.ToString().Equals("PREHEAT"))
                {
                    PIVParam.PreHeatStates.Clear();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _LoadParameterCommand;
        public ICommand LoadParameterCommand
        {
            get
            {
                if (null == _LoadParameterCommand) _LoadParameterCommand = new RelayCommand(LoadParameterCommandFunc);
                return _LoadParameterCommand;
            }
        }
        private void LoadParameterCommandFunc()
        {
            try
            {
                _PIVManager.LoadSysParameter();
                InitViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _SaveParameterCommand;
        public ICommand SaveParameterCommand
        {
            get
            {
                if (null == _SaveParameterCommand) _SaveParameterCommand = new RelayCommand(SaveParameterCommandFunc);
                return _SaveParameterCommand;
            }
        }
        private void SaveParameterCommandFunc()
        {
            try
            {
                _PIVManager.SaveSysParameter();
                _PIVManager.LoadSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }
}
