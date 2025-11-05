using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChillerConnSettingDialog
{
    using Autofac;
    using ControlModules;
    using EnvControlModule.Parameter;
    using LoaderBase.Communication;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Temperature.Chiller;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using Temperature.Temp.Chiller;
    using VirtualKeyboardControl;
    using AccountModule;

    public class ChillerConnSettingDialgVM : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region // Property

        private IChillerModule _ChillerModule;
        public IChillerModule ChillerModule
        {
            get { return _ChillerModule; }
            set
            {
                if (value != _ChillerModule)
                {
                    _ChillerModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsOperationLock;
        public bool IsOperationLock
        {
            get { return _IsOperationLock; }
            set
            {
                if (value != _IsOperationLock)
                {
                    _IsOperationLock = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _OffsetDataGridSelectedIndex;
        public int OffsetDataGridSelectedIndex
        {
            get { return _OffsetDataGridSelectedIndex; }
            set
            {
                if (value != _OffsetDataGridSelectedIndex)
                {
                    _OffsetDataGridSelectedIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ChillerOffsetDicInfo> _OffsetDicList
             = new ObservableCollection<ChillerOffsetDicInfo>();
        public ObservableCollection<ChillerOffsetDicInfo> OffsetDicList
        {
            get { return _OffsetDicList; }
            set
            {
                if (value != _OffsetDicList)
                {
                    _OffsetDicList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<EnumChillerModuleMode> _ChillerModeList
             = new List<EnumChillerModuleMode>();
        public List<EnumChillerModuleMode> ChillerModeList
        {
            get { return _ChillerModeList; }
            set
            {
                if (value != _ChillerModeList)
                {
                    _ChillerModeList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumChillerModuleMode _SelectedChillerMode;
        public EnumChillerModuleMode SelectedChillerMode
        {
            get { return _SelectedChillerMode; }
            set
            {
                if (value != _SelectedChillerMode)
                {
                    _SelectedChillerMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _PingTimeOut;
        public int PingTimeOut
        {
            get { return _PingTimeOut; }
            set
            {
                if (value != _PingTimeOut)
                {
                    _PingTimeOut = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _CommReadDelayTime;
        public long CommReadDelayTime
        {
            get { return _CommReadDelayTime; }
            set
            {
                if (value != _CommReadDelayTime)
                {
                    _CommReadDelayTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _CommWriteDelayTime;
        public long CommWriteDelayTime
        {
            get { return _CommWriteDelayTime; }
            set
            {
                if (value != _CommWriteDelayTime)
                {
                    _CommWriteDelayTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public ChillerConnSettingDialgVM(IChillerModule module)
        {
            ChillerModeList.Add(EnumChillerModuleMode.EMUL);
            ChillerModeList.Add(EnumChillerModuleMode.HUBER);
            ChillerModeList.Add(EnumChillerModuleMode.SOLARDIN);
            ChillerModeList.Add(EnumChillerModuleMode.REMOTE);
            ChillerModeList.Add(EnumChillerModuleMode.NONE);
            Init(module);
        }

        public void Init(IChillerModule module)
        {
            try
            {
                ChillerModule = module;
                IsOperationLock = ChillerModule.IsOperatingLock().Item1;
                OffsetDicList.Clear();
                foreach (var info in ChillerModule.ChillerParam.CoolantOffsetDictionary.Value)
                {
                    OffsetDicList.Add(new ChillerOffsetDicInfo(info.Key, info.Value));
                }

                SelectedChillerMode = ChillerModule.ChillerParam.ChillerModuleMode.Value;

                PingTimeOut = ChillerModule.EnvControlManager().ChillerManager.GetPingTimeOut();
                CommReadDelayTime = ChillerModule.EnvControlManager().ChillerManager.GetCommReadDelayTime();
                CommWriteDelayTime = ChillerModule.EnvControlManager().ChillerManager.GetCommWriteDelayTime();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region Command & Method

        private AsyncCommand<object> _SetOperaionLockCommand;
        public ICommand SetOperaionLockCommand
        {
            get
            {
                if (null == _SetOperaionLockCommand) _SetOperaionLockCommand = new AsyncCommand<object>(SetOperaionLockCommandFunc);
                return _SetOperaionLockCommand;
            }
        }
        private Task SetOperaionLockCommandFunc(object parameter)
        {
            try
            {
                if (parameter.Equals("true"))
                {
                    ChillerModule.EnvControlManager().ChillerManager.Set_OperaionLockValue(ChillerModule.ChillerInfo.Index, true);
                }
                else
                {
                    ChillerModule.EnvControlManager().ChillerManager.Set_OperaionLockValue(ChillerModule.ChillerInfo.Index, false);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private RelayCommand _OffsetTempAddCommand;
        public ICommand OffsetTempAddCommand
        {
            get
            {
                if (null == _OffsetTempAddCommand) _OffsetTempAddCommand = new RelayCommand(OffsetTempAddCommandFunc);
                return _OffsetTempAddCommand;
            }
        }
        private void OffsetTempAddCommandFunc()
        {
            try
            {
                if (OffsetDicList.ToList<ChillerOffsetDicInfo>().Find(info => info.TargetTemp == 99999) != null)
                {
                    //await ChillerModule.MetroDialogManager().ShowMessageDialog("", "",MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
                else
                {

                    OffsetDicList.Add(new ChillerOffsetDicInfo(99999, 99999));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _OffsetTempDeleteCommand;
        public ICommand OffsetTempDeleteCommand
        {
            get
            {
                if (null == _OffsetTempDeleteCommand) _OffsetTempDeleteCommand = new RelayCommand(OffsetTempDeleteCommandFunc);
                return _OffsetTempDeleteCommand;
            }
        }
        private void OffsetTempDeleteCommandFunc()
        {
            try
            {
                OffsetDicList.RemoveAt(OffsetDataGridSelectedIndex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ConnectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                if (null == _ConnectCommand) _ConnectCommand = new AsyncCommand(ConnectCommandFunc);
                return _ConnectCommand;
            }
        }
        private Task ConnectCommandFunc()
        {
            try
            {
                if (CheckSuperUser() == false)
                {
                    MessageBox.Show("Incorrect password.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    return Task.CompletedTask;
                }

                if (ChillerModule.GetCommState() == ProberInterfaces.Enum.EnumCommunicationState.DISCONNECT)
                {
                    ChillerModule.Start();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private AsyncCommand _DisConnectCommand;
        public ICommand DisConnectCommand
        {
            get
            {
                if (null == _DisConnectCommand) _DisConnectCommand = new AsyncCommand(DisConnectCommandFunc);
                return _DisConnectCommand;
            }
        }
        private Task DisConnectCommandFunc()
        {
            try
            {
                if (CheckSuperUser() == false)
                {
                    MessageBox.Show("Incorrect password.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    return Task.CompletedTask;
                }

                if (ChillerModule.GetCommState() == ProberInterfaces.Enum.EnumCommunicationState.CONNECTED)
                {
                    ChillerModule.DisConnect();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private AsyncCommand _SetModeCommand;
        public ICommand SetModeCommand
        {
            get
            {
                if (null == _SetModeCommand) _SetModeCommand = new AsyncCommand(SetModeCommandFunc);
                return _SetModeCommand;
            }
        }
        private Task SetModeCommandFunc()
        {
            try
            {
                if (CheckSuperUser() == false)
                {
                    MessageBox.Show("Incorrect password.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    return Task.CompletedTask;
                }

                if (ChillerModule.ChillerParam.ChillerModuleMode.Value != SelectedChillerMode)
                {
                    int listIndex = this.EnvControlManager().ChillerManager.GetChillerModules().FindIndex(module => module.ChillerInfo.Index == ChillerModule.ChillerInfo.Index);
                    if (listIndex != -1)
                    {
                        EventCodeEnum eventCode = EventCodeEnum.UNDEFINED;
                        if ((eventCode = ChillerModule.DisConnect()) == EventCodeEnum.NONE)
                        {
                            ChillerModule.ChillerParam.ChillerModuleMode.Value = SelectedChillerMode;

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ChillerModule newChillerModule = new ChillerModule(ChillerModule.ChillerParam, ChillerModule.ChillerInfo.Index);
                                newChillerModule.InitModule();
                                Init(newChillerModule);
                                ChillerModule.DeInitModule();
                                ChillerManager chillerManager = this.EnvControlManager().ChillerManager as ChillerManager;
                                chillerManager.Chillers.Insert(listIndex, newChillerModule);
                                chillerManager.Chillers.Remove(ChillerModule);
                            });
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
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
                //coolant offset data 달라진것 있는지 확인.
                //1.리스트 갯수 확인
                bool isDifferentCount = ChillerModule.ChillerParam.CoolantOffsetDictionary.Value.Count == OffsetDicList.Count ? false : true;
                //2. target , offset 값 일치하는지 확인
                bool isDifferentValue = false;

                foreach (var info in OffsetDicList)
                {
                    double offsetVal = 999;
                    if (ChillerModule.ChillerParam.CoolantOffsetDictionary.Value.ContainsKey(info.TargetTemp))
                    {
                        ChillerModule.ChillerParam.CoolantOffsetDictionary.Value.TryGetValue(info.TargetTemp, out offsetVal);
                        if (offsetVal != info.TempOffset)
                        {

                            isDifferentValue = true;
                            break;
                        }

                    }
                    else
                    {
                        isDifferentValue = true;
                        break;
                    }
                }

                if (isDifferentCount | isDifferentValue)
                {
                    ChillerModule.ChillerParam.CoolantOffsetDictionary.Value.Clear();
                    foreach (var info in OffsetDicList)
                    {
                        ChillerModule.ChillerParam.CoolantOffsetDictionary.Value.Add(info.TargetTemp, info.TempOffset);
                    }
                    ChillerModule.ChillerParam.CoolantOffsetDictionary.Value = 
                        ChillerModule.ChillerParam.CoolantOffsetDictionary.Value.OrderByDescending(dic => dic.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
                }

                EnvControlParameter envParam = this.EnvControlManager().EnvSysParam as EnvControlParameter;
                envParam.ChillerSysParam.ChillerParams[ChillerModule.ChillerInfo.Index - 1] = ChillerModule.ChillerParam as ChillerParameter;
                this.EnvControlManager().SaveSysParameter();

                //Stage 에도 적용.
                ILoaderCommunicationManager loaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                foreach (var stageIdx in ChillerModule.ChillerParam.StageIndexs)
                {
                    bool isconnect = loaderCommunicationManager.GetStage(stageIdx)?.StageInfo.IsConnected ?? false;
                    if(isconnect)
                    {
                        var stageClient = this.EnvControlManager().GetEnvControlClient(stageIdx);
                        if (stageClient != null)
                        {
                            stageClient.SetChillerData(ChillerModule.GetChillerParam(), isDifferentCount | isDifferentValue);
                        }
                    }
                }

                //Chiller 동작 중 이라면 적용.
                //if (ChillerModule.IsTempControlActive())
                //{
                //    var chillerTargetTemp = ChillerModule.GetSetTempValue() + ChillerModule.GetChillerTempoffset(ChillerModule.GetSetTempValue());
                //    //this.EnvControlManager().ChillerManager.SetTargetTemp()
                //    ChillerModule.SetTargetTemp(chillerTargetTemp, ProberInterfaces.Temperature.TempValueType.HUBER);
                //    ChillerModule.ChillerInfo.TargetTemp = chillerTargetTemp;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        private RelayCommand _AddErrorCodeCommand;
        public ICommand AddErrorCodeCommand
        {
            get
            {
                if (null == _AddErrorCodeCommand) _AddErrorCodeCommand = new RelayCommand(AddErrorCodeCommandFunc);
                return _AddErrorCodeCommand;
            }
        }
        private void AddErrorCodeCommandFunc()
        {
            try
            {
                if (ChillerModule != null)
                {
                    ChillerModule _chillerModule = (ChillerModule)ChillerModule;
                    _chillerModule.ChillerInfo.ErrorReport = 999;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ClearErrorCodeCommand;
        public ICommand ClearErrorCodeCommand
        {
            get
            {
                if (null == _ClearErrorCodeCommand) _ClearErrorCodeCommand = new RelayCommand(ClearErrorCodeCommandFunc);
                return _ClearErrorCodeCommand;
            }
        }
        private void ClearErrorCodeCommandFunc()
        {
            try
            {
                if (ChillerModule != null)
                {
                    ChillerModule _chillerModule = (ChillerModule)ChillerModule;
                    _chillerModule.ChillerInfo.ErrorReport = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _AddWarningCodeCommand;
        public ICommand AddWarningCodeCommand
        {
            get
            {
                if (null == _AddWarningCodeCommand) _AddWarningCodeCommand = new RelayCommand(AddWarningCodeCommandFunc);
                return _AddWarningCodeCommand;
            }
        }
        private void AddWarningCodeCommandFunc()
        {
            try
            {
                if (ChillerModule != null)
                {
                    ChillerModule _chillerModule = (ChillerModule)ChillerModule;
                    _chillerModule.ChillerInfo.WarningReport.Add(999);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ClearWarningCodeCommand;
        public ICommand ClearWarningCodeCommand
        {
            get
            {
                if (null == _ClearWarningCodeCommand) _ClearWarningCodeCommand = new RelayCommand(ClearWarningCodeCommandFunc);
                return _ClearWarningCodeCommand;
            }
        }
        private void ClearWarningCodeCommandFunc()
        {
            try
            {
                if(ChillerModule != null)
                {
                    ChillerModule _chillerModule = (ChillerModule)ChillerModule;
                    _chillerModule.ChillerInfo.WarningReport.Clear();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private bool CheckSuperUser()
        {
            bool retVal = false;
            try
            {
                string text = null;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                });

                String superPassword = AccountManager.MakeSuperAccountPassword();
                
                if (text.ToLower().CompareTo(superPassword) == 0)
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void TextBox_TextChanged(object sender, MouseEventArgs e)
        {

        }
        public void DataGridTextColumn_TargetUpdated(object sender, MouseEventArgs e)
        {

        }

    }

    public class ChillerOffsetDicInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion

        private double _TargetTemp;
        public double TargetTemp
        {
            get { return _TargetTemp; }
            set
            {
                if (value != _TargetTemp)
                {
                    _TargetTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _TempOffset;
        public double TempOffset
        {
            get { return _TempOffset; }
            set
            {
                if (value != _TempOffset)
                {
                    _TempOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ChillerOffsetDicInfo(double target, double offset)
        {
            TargetTemp = target;
            TempOffset = offset;
        }
    }
}
