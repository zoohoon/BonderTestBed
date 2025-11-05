
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SecsGemSettingDialogVM
{
    public class SecsGemSettingDialogViewModel : INotifyPropertyChanged, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private IGEMModule _Gem;
        public IGEMModule Gem
        {
            get { return _Gem; }
            set
            {
                if (_Gem != value)
                {
                    _Gem = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region <!-- Main  & Gem Setting-->
        private IHasClock _ClockSource;
        public IHasClock ClockSource
        {
            get { return _ClockSource; }
            set
            {
                if (_ClockSource != value)
                {
                    _ClockSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _TerminalMsg;
        public string TerminalMsg
        {
            get { return _TerminalMsg; }
            set
            {
                if(_TerminalMsg != value)
                {
                    _TerminalMsg = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Isshortcut;
        public bool Isshortcut
        {
            get { return _Isshortcut; }
            set
            {
                _Isshortcut = value;
                RaisePropertyChanged();
            }
        }

        public SecsGemSettingDialogViewModel(bool Isshortcut = true)
        {
            try
            {
                Gem = this.GEMModule();
                ClockSource = (this.ViewModelManager().MainTopBarView as FrameworkElement)?.DataContext as IHasClock;
                GemApITypeEnums.Add(GemAPIType.XGEM);
                GemApITypeEnums.Add(GemAPIType.XGEM300);

                GemProcessorTypeEnums.Add(GemProcessorType.CELL);
                GemProcessorTypeEnums.Add(GemProcessorType.COMMANDER);
                GemProcessorTypeEnums.Add(GemProcessorType.SINGLE);
                this.Isshortcut = Isshortcut;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ChangeCommunicationStateCommand;
        public ICommand ChangeCommunicationStateCommand
        {
            get
            {
                if (null == _ChangeCommunicationStateCommand) _ChangeCommunicationStateCommand = new RelayCommand(ChangeCommunicationState);
                return _ChangeCommunicationStateCommand;
            }
        }

        public void ChangeCommunicationState()
        {
            try
            {
                if((this.Gem?.GemCommManager?.SecsCommInformData.Enable ?? SecsEnum_Enable.UNKNOWN) == SecsEnum_Enable.ENABLE)
                {
                    LoggerManager.Debug("[SecsGemSettingDialogVM] ChangeCommunicationState() - Will attempt to Change the 'GEM Enable' to Enable");
                    this.Gem?.CommEnable();
                }
                else if ((this.Gem?.GemCommManager?.SecsCommInformData.Enable ?? SecsEnum_Enable.UNKNOWN) == SecsEnum_Enable.DISABLE)
                {
                    LoggerManager.Debug("[SecsGemSettingDialogVM] ChangeCommunicationState() - Will attempt to Change the 'GEM Enable' to Disable");
                    this.Gem?.CommDisable();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private RelayCommand<SecsEnum_ControlState> _ChangeControlStateCommand;
        public ICommand ChangeControlStateCommand
        {
            get
            {
                if (null == _ChangeControlStateCommand) _ChangeControlStateCommand = new RelayCommand<SecsEnum_ControlState>(ChangeControlState);
                return _ChangeControlStateCommand;
            }
        }

        public void ChangeControlState(SecsEnum_ControlState parameter)
        {
            try
            {
                //SecsEnum_ControlState controlState = this.Gem?.GemCommManager?.SecsCommInformData.ControlState ?? SecsEnum_ControlState.UNKNOWN;
                SecsEnum_ControlState controlState = parameter;
                if (controlState == SecsEnum_ControlState.EQ_OFFLINE ||
                    controlState == SecsEnum_ControlState.HOST_OFFLINE)
                {
                    this.Gem?.ReqOffLine();

                }
                else if(controlState == SecsEnum_ControlState.ONLINE_LOCAL)
                {
                    this.Gem?.ReqLocal();

                }
                else if(controlState == SecsEnum_ControlState.ONLINE_REMOTE)
                {
                    this.Gem?.ReqRemote();

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private RelayCommand _ChangeInitEstablishSourceCommand;
        public ICommand ChangeInitEstablishSourceCommand
        {
            get
            {
                if (null == _ChangeInitEstablishSourceCommand) _ChangeInitEstablishSourceCommand = new RelayCommand(ChangeInitEstablishSource);
                return _ChangeInitEstablishSourceCommand;
            }
        }

        public void ChangeInitEstablishSource()
        {
            try
            {
                if ((this.Gem?.GemCommManager?.SecsCommInformData.Equipment_Initiated_Connected ?? SecsEnum_EstablishSource.UNKNOWN) == SecsEnum_EstablishSource.HOST)
                {
                    Gem.SetInitHostEstablish();
                }
                else
                {
                    Gem.SetInitProberEstablish();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private RelayCommand _ChangeInitEnableCommand;
        public ICommand ChangeInitEnableCommand
        {
            get
            {
                if (null == _ChangeInitEnableCommand) _ChangeInitEnableCommand = new RelayCommand(ChangeInitEnable);
                return _ChangeInitEnableCommand;
            }
        }

        public void ChangeInitEnable()
        {
            try
            {
                if ((this.Gem?.GemCommManager?.SecsCommInformData.DefaultCommState ?? SecsEnum_Enable.UNKNOWN) == SecsEnum_Enable.ENABLE)
                {
                    Gem.SetInitCommunicationState_Enable();
                }
                else if ((this.Gem?.GemCommManager?.SecsCommInformData.DefaultCommState ?? SecsEnum_Enable.UNKNOWN) == SecsEnum_Enable.DISABLE)
                {
                    Gem.SetInitCommunicationState_Disable();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private RelayCommand _ChangeInitControlStateCommand;
        public ICommand ChangeInitControlStateCommand
        {
            get
            {
                if (null == _ChangeInitControlStateCommand) _ChangeInitControlStateCommand = new RelayCommand(ChangeInitControlState);
                return _ChangeInitControlStateCommand;
            }
        }

        public void ChangeInitControlState()
        {
            try
            {
                var initControlState = this.Gem?.GemCommManager?.SecsCommInformData.InitControlState ?? SecsEnum_ON_OFFLINEState.OFFLINE;
                if (initControlState == SecsEnum_ON_OFFLINEState.OFFLINE)
                {
                    Gem.SetInitControlState_Offline();
                }
                else if (initControlState == SecsEnum_ON_OFFLINEState.ONLINE)
                {
                    var onlineSubState = Gem.GemCommManager?.SecsCommInformData.OnLineSubState;
                    if (onlineSubState == SecsEnum_OnlineSubState.LOCAL)
                    {
                        Gem.SetInitControlState_Local();
                    }
                    else if (onlineSubState == SecsEnum_OnlineSubState.REMOTE)
                    {
                        Gem.SetInitControlState_Remote();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private RelayCommand _ApplyGemInfoCommand;
        public ICommand ApplyGemInfoCommand
        {
            get
            {
                if (null == _ApplyGemInfoCommand) _ApplyGemInfoCommand = new RelayCommand(ApplyGemInfo);
                return _ApplyGemInfoCommand;
            }
        }

        public void ApplyGemInfo()
        {
            try
            {
                Gem.GemCommManager?.CommunicationParamApply();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private RelayCommand _RequestTimeCommand;
        public ICommand RequestTimeCommand
        {
            get
            {
                if (null == _RequestTimeCommand) _RequestTimeCommand = new RelayCommand(RequestTime);
                return _RequestTimeCommand;
            }
        }

        public void RequestTime()
        {
            try
            {


                //this.Gem?.TimeRequest();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private RelayCommand _SendTerminalMsgCommand;
        public ICommand SendTerminalMsgCommand
        {
            get
            {
                if (null == _SendTerminalMsgCommand) _SendTerminalMsgCommand = new RelayCommand(SendTerminalMsg);
                return _SendTerminalMsgCommand;
            }
        }

        public void SendTerminalMsg()
        {
            try
            {
                if(!string.IsNullOrEmpty(TerminalMsg))
                {
                    this.Gem?.SendTerminal(TerminalMsg);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        #endregion

        #region <!-- Ect -->

        #region <!-- Property -->

        private long _Vid;
        public long Vid
        {
            get { return _Vid; }
            set
            {
                if (value != _Vid)
                {
                    _Vid = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _VidValue;
        public string VidValue
        {
            get { return _VidValue; }
            set
            {
                if (value != _VidValue)
                {
                    _VidValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _ElementID;
        public long ElementID
        {
            get { return _ElementID; }
            set
            {
                if (value != _ElementID)
                {
                    _ElementID = value;
                    RaisePropertyChanged();
                }
            }
        }

        //=============Param Setting
        private string _SerialNumber;
        public string SerialNumber
        {
            get { return _SerialNumber; }
            set
            {
                if (value != _SerialNumber)
                {
                    _SerialNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ConfigPath;
        public string ConfigPath
        {
            get { return _ConfigPath; }
            set
            {
                if (value != _ConfigPath)
                {
                    _ConfigPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ServiceHostAppPath;
        public string ServiceHostAppPath
        {
            get { return _ServiceHostAppPath; }
            set
            {
                if (value != _ServiceHostAppPath)
                {
                    _ServiceHostAppPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<GemAPIType> _GemApITypeEnums = new List<GemAPIType>();
        public List<GemAPIType> GemApITypeEnums
        {
            get { return _GemApITypeEnums; }
            set
            {
                if (value != _GemApITypeEnums)
                {
                    _GemApITypeEnums = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<GemProcessorType> _GemProcessorTypeEnums = new List<GemProcessorType>();
        public List<GemProcessorType> GemProcessorTypeEnums
        {
            get { return _GemProcessorTypeEnums; }
            set
            {
                if (value != _GemProcessorTypeEnums)
                {
                    _GemProcessorTypeEnums = value;
                    RaisePropertyChanged();
                }
            }
        }



        private GemAPIType _SelectedGemApIType;
        public GemAPIType SelectedGemApIType
        {
            get { return _SelectedGemApIType; }
            set
            {
                if (value != _SelectedGemApIType)
                {
                    _SelectedGemApIType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private GemProcessorType _SelectedGemProcessorType;
        public GemProcessorType SelectedGemProcessorType
        {
            get { return _SelectedGemProcessorType; }
            set
            {
                if (value != _SelectedGemProcessorType)
                {
                    _SelectedGemProcessorType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ReciverProberType;
        public string ReciverProberType
        {
            get { return _ReciverProberType; }
            set
            {
                if (value != _ReciverProberType)
                {
                    _ReciverProberType = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private AsyncCommand _SaveParameterCommand;
        public ICommand SaveParameterCommand
        {
            get
            {
                if (null == _SaveParameterCommand) _SaveParameterCommand = new AsyncCommand(SaveParameterCommandFunc);
                return _SaveParameterCommand;
            }
        }

        public Task SaveParameterCommandFunc()
        {
            try
            {
                Gem.GemSysParam.GEMSerialNum.Value = SerialNumber;
                Gem.GemSysParam.ConfigPath.Value = ConfigPath;
                Gem.GemSysParam.GEMServiceHostWCFPath.Value = ServiceHostAppPath;
                Gem.GemSysParam.GEMServiceHostAPIType = SelectedGemApIType;
                Gem.GemSysParam.GemProcessrorType.Value = SelectedGemProcessorType;
                Gem.GemSysParam.ReceiveMessageType = ReciverProberType;

                var retVal = Gem.SaveSysParameter();
                if(retVal == ProberErrorCode.EventCodeEnum.NONE)
                {
                    MessageBox.Show("Success");
                }
                else
                {
                    MessageBox.Show("Fail");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return Task.CompletedTask;
        }


        private RelayCommand<object> _SendEventCommand;
        public ICommand SendEventCommand
        {
            get
            {
                if (null == _SendEventCommand) _SendEventCommand = new RelayCommand<object>(SendEventCommandFunc);
                return _SendEventCommand;
            }
        }

        public void SendEventCommandFunc(object parameter)
        {
            try
            {
                if (parameter != null)
                {
                    long eventId = Convert.ToInt64(parameter);
                    this.Gem.SetEvent(eventId);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private RelayCommand _SetVidValueCommand;
        public ICommand SetVidValueCommand
        {
            get
            {
                if (null == _SetVidValueCommand) _SetVidValueCommand = new RelayCommand(SetVidValueCommandFunc);
                return _SetVidValueCommand;
            }
        }

        public void SetVidValueCommandFunc()
        {
            try
            {
                var element = this.ParamManager().GetElement((int)ElementID);
                this.GEMModule().GemCommManager.SetVariable(new long[] { Convert.ToInt64(Vid) }, new string[] { VidValue }, ProberInterfaces.GEM.EnumVidType.SVID);
                this.GEMModule().NotifyValueChanged(ElementID,  VidValue, element);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private RelayCommand _StartGemCommand;
        public ICommand StartGemCommand
        {
            get
            {
                if (null == _StartGemCommand) _StartGemCommand = new RelayCommand(StartGemCommandFunc);
                return _StartGemCommand;
            }
        }

        public void StartGemCommandFunc()
        {
            try
            {
                Gem?.GemCommManager.InitConnectService();           
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private RelayCommand _EndGemCommand;
        public ICommand EndGemCommand
        {
            get
            {
                if (null == _EndGemCommand) _EndGemCommand = new RelayCommand(EndGemCommandFunc);
                return _EndGemCommand;
            }
        }

        public void EndGemCommandFunc()
        {
            try
            {
                Gem?.DeInitModule();
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        private RelayCommand<object> _FindPathCommand;
        public ICommand FindPathCommand
        {
            get
            {
                if (null == _FindPathCommand) _FindPathCommand = new RelayCommand<object>(FindPathCommandFunc);
                return _FindPathCommand;
            }
        }

        private void FindPathCommandFunc(object param)
        {
            try
            {
                string pathparam = param.ToString();
                if(pathparam.Equals("Config"))
                {
                    using (var dialog = new System.Windows.Forms.OpenFileDialog() { InitialDirectory = "C:\\ProberSystem\\Utility\\GEM", Filter = "CFG files (*.cgf)|*.cfg" })
                    {
                        System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                        if (dialog.FileName != "")
                        {
                            ConfigPath = dialog.FileName;
                        }
                    }
                }
                else if(pathparam.Equals("HostApp"))
                {
                    using (var dialog = new System.Windows.Forms.OpenFileDialog() { InitialDirectory = "C:\\ProberSystem\\Utility\\GEM", Filter = "EXE files (*.exe)|*.exe" })
                    {
                        System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                        if (dialog.FileName != "")
                        {
                            ServiceHostAppPath = dialog.FileName;
                        }
                    }
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        private RelayCommand<object> _TabItem_MouseLeftButtonUpCommand;
        public ICommand TabItem_MouseLeftButtonUpCommand
        {
            get
            {
                if (null == _TabItem_MouseLeftButtonUpCommand) _TabItem_MouseLeftButtonUpCommand = new RelayCommand<object>(TabItem_MouseLeftButtonUpCommandFunc);
                return _TabItem_MouseLeftButtonUpCommand;
            }
        }

        private void TabItem_MouseLeftButtonUpCommandFunc(object param)
        {
            string pathparam = param.ToString();
            
            if(pathparam.Equals("ECT"))
            {
                SerialNumber = Gem.GemSysParam.GEMSerialNum.Value;
                ConfigPath = Gem.GemSysParam.ConfigPath.Value;
                ServiceHostAppPath = Gem.GemSysParam.GEMServiceHostWCFPath.Value;
                SelectedGemApIType = Gem.GemSysParam.GEMServiceHostAPIType;
                SelectedGemProcessorType = Gem.GemSysParam.GemProcessrorType.Value;
                ReciverProberType = Gem.GemSysParam.ReceiveMessageType;
            }
        }

        private RelayCommand _SetReceiverTypeCommand;
        public ICommand SetReceiverTypeCommand
        {
            get
            {
                if (null == _SetReceiverTypeCommand) _SetReceiverTypeCommand = new RelayCommand(SetReceiverTypeCommandFunc);
                return _SetReceiverTypeCommand;
            }
        }

        public void SetReceiverTypeCommandFunc()
        {
            try
            {
                if (ReciverProberType.Equals(Gem.GemSysParam.ReceiveMessageType) == false)
                {
                    Gem.GemSysParam.ReceiveMessageType = ReciverProberType;
                    EventCodeEnum retVal = Gem.GemCommManager.SetSecsMessageReceiver();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        MessageBox.Show("Load Receiver DLL Fail");
                    }
                    else
                    {
                        MessageBox.Show("Load Receiver DLL Success");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion
    }
}