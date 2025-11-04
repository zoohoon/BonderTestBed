namespace ProberViewModel.ViewModel.E84
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.E84;
    using ProberInterfaces.E84.ProberInterfaces;
    using ProberInterfaces.Foup;
    using RelayCommandBase;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Controls;
    using System.Windows.Input;
    using VirtualKeyboardControl;

    public class E84ControlSettingViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region <remarks> PropertyChanged </remarks>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region <remarks> Property </remarks>

        private IE84Controller _E84Controller;

        public IE84Controller E84Controller
        {
            get { return _E84Controller; }
            set { _E84Controller = value; }
        }

        private bool _IsAutoMode;
        public bool IsAutoMode
        {
            get { return _IsAutoMode; }
            set
            {
                if (value != _IsAutoMode)
                {
                    _IsAutoMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsManualMode;
        public bool IsManualMode
        {
            get { return _IsManualMode; }
            set
            {
                if (value != _IsManualMode)
                {
                    _IsManualMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsConnected;
        public bool IsConnected
        {
            get { return _IsConnected; }
            set
            {
                if (value != _IsConnected)
                {
                    _IsConnected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsDisConnected;
        public bool IsDisConnected
        {
            get { return _IsDisConnected; }
            set
            {
                if (value != _IsDisConnected)
                {
                    _IsDisConnected = value;
                    RaisePropertyChanged();
                }
            }
        }



        private string _PropertyName;
        public string PropertyName
        {
            get { return _PropertyName; }
            set
            {
                if (value != _PropertyName)
                {
                    _PropertyName = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string _Description;
        public string Description
        {
            get { return _Description; }
            set
            {
                if (value != _Description)
                {
                    _Description = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<E84Info> _E84Infos = new ObservableCollection<E84Info>();
        public ObservableCollection<E84Info> E84Infos
        {
            get { return _E84Infos; }
            set
            {
                if (value != _E84Infos)
                {
                    _E84Infos = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private int _RecoveryTimeout;
        //public int RecoveryTimeout
        //{
        //    get { return _RecoveryTimeout; }
        //    set
        //    {
        //        if (value != _RecoveryTimeout)
        //        {
        //            _RecoveryTimeout = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private int _RetryCount;
        //public int RetryCount
        //{
        //    get { return _RetryCount; }
        //    set
        //    {
        //        if (value != _RetryCount)
        //        {
        //            _RetryCount = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private bool _IsAttatched;
        public bool IsAttatched
        {
            get { return _IsAttatched; }
            set
            {
                if (value != _IsAttatched)
                {
                    _IsAttatched = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region <remarks> Creator & Init </remarks>

        public E84ControlSettingViewModel(IE84Controller e84Controller)
        {
            try
            {
                this.E84Controller = e84Controller;
                InitModeFlag(E84Mode.UNDEFIND);
                InitConnFlag();
                InitCommParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        ~E84ControlSettingViewModel()
        {
            try
            {
                if (this.E84Controller != null)
                {
                    this.E84Controller.CommModule.SetIsGetOptionFlag(false);
                    this.E84Controller = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void InitModeFlag(E84Mode mode)
        {
            try
            {
                if (E84Controller != null)
                {
                    if (mode == E84Mode.UNDEFIND)
                    {
                        if (E84Controller.CommModule.RunMode == E84Mode.AUTO)
                        {
                            IsAutoMode = true;
                            IsManualMode = false;
                        }
                        else if (E84Controller.CommModule.RunMode == E84Mode.MANUAL)
                        {
                            IsAutoMode = false;
                            IsManualMode = true;
                        }
                        else
                        {
                            IsAutoMode = false;
                            IsManualMode = true;
                        }
                    }
                    else
                    {
                        if (mode == E84Mode.AUTO)
                        {
                            IsAutoMode = true;
                            IsManualMode = false;
                        }
                        else if (mode == E84Mode.MANUAL)
                        {
                            IsAutoMode = false;
                            IsManualMode = true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void InitConnFlag()
        {
            try
            {
                if (E84Controller != null)
                {
                    foreach (var moduleparam in this.E84Module().E84SysParam.E84Moduls)
                    {
                        if (moduleparam.FoupIndex == E84Controller.E84ModuleParaemter.FoupIndex && moduleparam.E84OPModuleType == E84Controller.E84ModuleParaemter.E84OPModuleType)
                        {
                            IsAttatched = moduleparam.E84_Attatched;
                        }
                    }
                    if (E84Controller.CommModule.Connection == E84ComStatus.CONNECTED)
                    {
                        IsConnected = true;
                        IsDisConnected = false;
                    }
                    else if (E84Controller.CommModule.Connection == E84ComStatus.DISCONNECTED)
                    {
                        IsConnected = false;
                        IsDisConnected = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void InitCommParam()
        {
            //v22_merge// 코드 검토 필요
            try
            {
                this.E84Controller.CommModule.SetIsGetOptionFlag(true);
                foreach (var foupinfo in this.E84Module().E84SysParam.E84Moduls)
                {
                    var controller = this.E84Module().GetE84Controller(foupinfo.FoupIndex, foupinfo.E84OPModuleType);

                    if (controller != null)
                    {
                        if (controller.CommModule.Connection == E84ComStatus.CONNECTED)
                        {
                            E84Infos.Add(new E84Info(foupinfo.FoupIndex, controller, true));
                        }
                        else if (controller.CommModule.Connection == E84ComStatus.DISCONNECTED)
                        {
                            E84Infos.Add(new E84Info(foupinfo.FoupIndex, controller, false));
                        }
                    }
                }

                this.E84Module().SaveSysParameter();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remark> Command </remarks>

        //private RelayCommand _SetCommunication;
        //public ICommand SetCommunication
        //{
        //    get
        //    {
        //        if (null == _SetCommunication) _SetCommunication
        //                = new RelayCommand(SetCommunicationFunc);
        //        return _SetCommunication;
        //    }
        //}

        //private void SetCommunicationFunc()
        //{
        //    try
        //    {
        //        if (E84Controller != null)
        //        {
        //            int ret = E84Controller.CommModule.GetCommunication(out int timeout, out int retry);
        //            if (ret == 0)
        //            {
        //                LoggerManager.Debug($"\nBefore timeout: {timeout}, retry: {retry}" +
        //                                     $"\nAfter timeout: {RecoveryTimeout}, retry: {RetryCount}");
        //                E84Controller.CommModule.SetCommunication(RecoveryTimeout, RetryCount);

        //                this.E84Module().E84SysParam.E84Moduls[E84Controller.FoupIndex - 1].RecoveryTimeout.Value = RecoveryTimeout;
        //                this.E84Module().E84SysParam.E84Moduls[E84Controller.FoupIndex - 1].RetryCount.Value = RetryCount;
        //                this.E84Module().SaveSysParameter();
        //            }
        //            else
        //            {
        //                LoggerManager.Debug($"Please Check E84 Connection");
        //            }
        //        }
        //        else
        //        {
        //            LoggerManager.Debug($"E84Controller is null");
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private RelayCommand<object> _ChangeModeCommand;
        public ICommand ChangeModeCommand
        {
            get
            {
                if (null == _ChangeModeCommand) _ChangeModeCommand
                        = new RelayCommand<object>(ChangeModeCommandFunc);
                return _ChangeModeCommand;
            }
        }

        private void ChangeModeCommandFunc(object param)
        {
            try
            {
                E84Mode mode = (E84Mode)param;
                /// 0 : Manual <br/>
                /// 1 : Auto
                //if (E84Controller.GetModuleStateEnum() == ModuleStateEnum.RUNNING)
                //{
                //    E84Controller.E84ErrorOccured(code: E84EventCode.HAND_SHAKE_ERROR_LOAD_MODE);
                //}
                //else
                //{

                foreach (var moduleparam in this.E84Module().E84SysParam.E84Moduls)
                {
                    if (moduleparam.FoupIndex == E84Controller.E84ModuleParaemter.FoupIndex && moduleparam.E84OPModuleType == E84Controller.E84ModuleParaemter.E84OPModuleType)
                    {
                        moduleparam.AccessMode.Value = mode;
                    }
                }
                this.E84Module().SaveSysParameter();


                var ret = E84Controller.SetMode((int)mode);
                if (ret == 0)
                {
                    InitModeFlag(mode);
                }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<object> _GroupChangeModeCommand;
        public ICommand GroupChangeModeCommand
        {
            get
            {
                if (null == _GroupChangeModeCommand) _GroupChangeModeCommand
                        = new RelayCommand<object>(GroupChangeModeCommandFunc);
                return _GroupChangeModeCommand;
            }
        }

        private void GroupChangeModeCommandFunc(object param)
        {
            try
            {
                E84Mode mode = (E84Mode)param;

                foreach (var e84 in E84Infos)
                {
                    if (e84.IsConnected)
                    {
                        var ret = e84.E84Controller.SetMode((int)mode);
                        if (ret == 0)
                        {
                            InitModeFlag(mode);
                        }
                    }
                }



            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<object> _ChangeConnectCommand;
        public ICommand ChangeConnectCommand
        {
            get
            {
                if (null == _ChangeConnectCommand) _ChangeConnectCommand
                        = new RelayCommand<object>(ChangeConnectCommandFunc);
                return _ChangeConnectCommand;
            }
        }

        private void ChangeConnectCommandFunc(object param)
        {
            try
            {
                E84ComStatus mode = (E84ComStatus)param;
                if (mode == E84ComStatus.CONNECTED)
                {
                    foreach (var moduleparam in this.E84Module().E84SysParam.E84Moduls)
                    {
                        if (moduleparam.FoupIndex == E84Controller.E84ModuleParaemter.FoupIndex && moduleparam.E84OPModuleType == E84Controller.E84ModuleParaemter.E84OPModuleType)
                        {
                            E84Controller.ConnectCommodule(moduleparam, this.E84Module().E84SysParam.E84PinSignal);
                        }
                    }


                    //v22_merge//일단 살림
                    if (E84Controller.E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.FOUP)
                    {
                        E84Controller.FoupOpModule().FoupControllers[E84Controller.E84ModuleParaemter.FoupIndex - 1].Service.FoupModule.UpdateFoupState();
                    }
                    else if (E84Controller.E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.CARD)
                    {
                        E84Controller.UpdateCardBufferState(forced_event: false);
                    }
                    InitCommParam();
                }
                else if (mode == E84ComStatus.DISCONNECTED)
                {
                    // E84Module과 Loader가 DisConnect되어 더이상 신호를 받고 있지 않아 이재작업을 하면 안되는데 E84Module은 켜져 있었음. 
                    E84Controller.CommModule.SetHoAvblSignal(0);
                    E84Controller.DisConnectCommodule();
                }
                InitConnFlag();
                InitModeFlag(E84Mode.UNDEFIND);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _SetAttatchedCommand;
        public ICommand SetAttatchedCommand
        {
            get
            {
                if (null == _SetAttatchedCommand) _SetAttatchedCommand
                        = new RelayCommand<object>(SetAttatchedCommandFunc);
                return _SetAttatchedCommand;
            }
        }
        /// <summary>
        ///  Attatched Button Command : Click시 Commodule생성과 Connect수행, E84_Attatched true로 Set
        /// </summary>
        /// <param name="param"></param>
        private void SetAttatchedCommandFunc(object param)
        {
            try
            {
                this.E84Module().SetAttatched(E84Controller.E84ModuleParaemter.FoupIndex, E84Controller.E84ModuleParaemter.E84OPModuleType);
                InitConnFlag();
                InitModeFlag(E84Mode.UNDEFIND);
                InitCommParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _SetDetachedCommand;
        public ICommand SetDetachedCommand
        {
            get
            {
                if (null == _SetDetachedCommand) _SetDetachedCommand
                        = new RelayCommand<object>(SetDetachedCommandFunc);
                return _SetDetachedCommand;
            }
        }
        /// <summary>
        /// Detached Button Command : Click시 DisConnect수행, E84_Attatched False로 Set
        /// </summary>
        /// <param name="param"></param>
        private void SetDetachedCommandFunc(object param)
        {
            try
            {
                this.E84Module().SetDetached(E84Controller.E84ModuleParaemter.FoupIndex, E84Controller.E84ModuleParaemter.E84OPModuleType);
                InitConnFlag();
                InitModeFlag(E84Mode.UNDEFIND);
                InitCommParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _ComboBoxSelectionChanged;
        public ICommand ComboBoxSelectionChanged
        {
            get
            {
                if (null == _ComboBoxSelectionChanged) _ComboBoxSelectionChanged
                        = new RelayCommand<object>(ComboBoxSelectionChangedFunc);
                return _ComboBoxSelectionChanged;
            }
        }

        private void ComboBoxSelectionChangedFunc(object param)
        {
            try
            {
                object[] _param = param as object[];
                ComboBox control = _param[0] as ComboBox;
                string mode = _param[1].ToString();
                switch (mode)
                {
                    case "UseLP1":
                        {
                            bool selectedValue = (bool)control.SelectedValue;
                            int useLP1 = -1;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetUseLp1(out useLP1);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (UseLP1)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }
                            if ((selectedValue ? 1 : 0) != useLP1)
                            {
                                if (useLP1 == 0)
                                    useLP1 = 1;
                                else if (useLP1 == 1)
                                    useLP1 = 0;

                                E84Controller.CommModule.SetUseLp1(useLP1);

                                //옵션변경 후 파라미터에 저장
                                (E84Controller.E84ModuleParaemter as E84ModuleParameter).UseLP1.Value = useLP1;
                                LoggerManager.Debug($"[E84 Option Set (UseLP1)] Value : {useLP1}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                            }
                        }
                        break;
                    case "UseClamp":
                        {
                            bool selectedValue = (bool)control.SelectedValue;
                            int use, inputType, actionType, timer = -1;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetClampOptions(out use, out inputType, out actionType, out timer);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (UseClamp)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }
                            if ((selectedValue ? 1 : 0) != use)
                            {
                                if (use == 0)
                                    use = 1;
                                else if (use == 1)
                                    use = 0;

                                E84Controller.CommModule.SetClampOptions(use, inputType, actionType, timer);
                                //옵션변경 후 파라미터에 저장
                                (E84Controller.E84ModuleParaemter as E84ModuleParameter).UseClamp.Value = use;
                                LoggerManager.Debug($"[E84 Option Set (UseClamp)] Value : {use}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                            }
                        }
                        break;
                    case "ActionType":
                        {
                            bool selectedValue = (bool)control.SelectedValue;
                            int use, inputType, actionType, timer = -1;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetClampOptions(out use, out inputType, out actionType, out timer);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (ActionType)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }
                            if ((selectedValue ? 1 : 0) != actionType)
                            {
                                if (actionType == 0)
                                    actionType = 1;
                                else if (actionType == 1)
                                    actionType = 0;

                                E84Controller.CommModule.SetClampOptions(use, inputType, actionType, timer);
                                //옵션변경 후 파라미터에 저장
                                (E84Controller.E84ModuleParaemter as E84ModuleParameter).DisableClampEvent.Value = actionType;
                                LoggerManager.Debug($"[E84 Option Set (ActionType)] Value : {actionType}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                            }
                        }
                        break;
                    case "InputType":
                        {
                            bool selectedValue = (bool)control.SelectedValue;
                            int use, inputType, actionType, timer = -1;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetClampOptions(out use, out inputType, out actionType, out timer);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (ClampComType)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }
                            if ((selectedValue ? 1 : 0) != inputType)
                            {
                                if (inputType == 0)
                                    inputType = 1;
                                else if (inputType == 1)
                                    inputType = 0;

                                E84Controller.CommModule.SetClampOptions(use, inputType, actionType, timer);
                                //옵션변경 후 파라미터에 저장
                                (E84Controller.E84ModuleParaemter as E84ModuleParameter).ClampComType.Value = inputType;
                                LoggerManager.Debug($"[E84 Option Set (ClampComType)] Value : {inputType}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                            }
                        }
                        break;


                    case "UseES":
                        {
                            bool selectedValue = (bool)control.SelectedValue;
                            int horet, esret = -1;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetLightCuratinSignalOptions(out horet, out esret);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (UseES)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }
                            if ((selectedValue ? 1 : 0) != esret)
                            {
                                if (esret == 0)
                                    esret = 1;
                                else if (esret == 1)
                                    esret = 0;

                                E84Controller.CommModule.SetLightCurtainSignalOptions(horet, esret);
                                //옵션변경 후 파라미터에 저장
                                (E84Controller.E84ModuleParaemter as E84ModuleParameter).UseES.Value = esret;
                                LoggerManager.Debug($"[E84 Option Set (UseES)] Value : {esret}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                            }
                        }
                        break;
                    case "UseHOAVBL":
                        {
                            bool selectedValue = (bool)control.SelectedValue;
                            int horet, esret = -1;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetLightCuratinSignalOptions(out horet, out esret);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (UseHOAVBL)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }
                            if ((selectedValue ? 1 : 0) != horet)
                            {
                                if (horet == 0)
                                    horet = 1;
                                else if (horet == 1)
                                    horet = 0;

                                E84Controller.CommModule.SetLightCurtainSignalOptions(horet, esret);

                                //옵션변경 후 파라미터에 저장
                                (E84Controller.E84ModuleParaemter as E84ModuleParameter).UseHOAVBL.Value = horet;
                                LoggerManager.Debug($"[E84 Option Set (UseHOAVBL)] Value : {horet}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                            }
                        }
                        break;
                    case "ReadyOff":
                        {
                            bool selectedValue = (bool)control.SelectedValue;
                            int useCs1, readyOff, validOn, validOff = -1;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetE84SignalOptions(out useCs1, out readyOff, out validOn, out validOff);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (ReadyOff)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }
                            if ((selectedValue ? 1 : 0) != readyOff)
                            {
                                if (readyOff == 0)
                                    readyOff = 1;
                                else if (readyOff == 1)
                                    readyOff = 0;

                                E84Controller.CommModule.SetE84SignalOptions(useCs1, readyOff, validOn, validOff);

                                //옵션변경 후 파라미터에 저장
                                (E84Controller.E84ModuleParaemter as E84ModuleParameter).ReadyOff.Value = readyOff;
                                LoggerManager.Debug($"[E84 Option Set (ReadyOff)] Value : {readyOff}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                            }
                        }
                        break;
                    case "UseCS1":
                        {
                            bool selectedValue = (bool)control.SelectedValue;
                            int useCs1, readyOff, validOn, validOff = -1;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetE84SignalOptions(out useCs1, out readyOff, out validOn, out validOff);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (UseCS1)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }
                            if ((selectedValue ? 1 : 0) != useCs1)
                            {
                                if (useCs1 == 0)
                                    useCs1 = 1;
                                else if (useCs1 == 1)
                                    useCs1 = 0;

                                E84Controller.CommModule.SetE84SignalOptions(useCs1, readyOff, validOn, validOff);

                                //옵션변경 후 파라미터에 저장
                                (E84Controller.E84ModuleParaemter as E84ModuleParameter).UseCS1.Value = useCs1;
                                LoggerManager.Debug($"[E84 Option Set (UseCS1)] Value : {useCs1}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                            }
                        }
                        break;
                    case "ValidOff":
                        {
                            bool selectedValue = (bool)control.SelectedValue;
                            int useCs1, readyOff, validOn, validOff = -1;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetE84SignalOptions(out useCs1, out readyOff, out validOn, out validOff);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (ValidOff)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }
                            if ((selectedValue ? 1 : 0) != validOff)
                            {
                                if (validOff == 0)
                                    validOff = 1;
                                else if (validOff == 1)
                                    validOff = 0;

                                E84Controller.CommModule.SetE84SignalOptions(useCs1, readyOff, validOn, validOff);

                                //옵션변경 후 파라미터에 저장
                                (E84Controller.E84ModuleParaemter as E84ModuleParameter).ValidOff.Value = validOff;
                                LoggerManager.Debug($"[E84 Option Set (ValidOff)] Value : {validOff}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                            }
                        }
                        break;
                    case "ValidOn":
                        {
                            bool selectedValue = (bool)control.SelectedValue;
                            int useCs1, readyOff, validOn, validOff = -1;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetE84SignalOptions(out useCs1, out readyOff, out validOn, out validOff);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (ValidOn)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }
                            if ((selectedValue ? 1 : 0) != validOn)
                            {
                                if (validOn == 0)
                                    validOn = 1;
                                else if (validOn == 1)
                                    validOn = 0;

                                E84Controller.CommModule.SetE84SignalOptions(useCs1, readyOff, validOn, validOff);

                                //옵션변경 후 파라미터에 저장
                                (E84Controller.E84ModuleParaemter as E84ModuleParameter).ValidOn.Value = validOn;
                                LoggerManager.Debug($"[E84 Option Set (ValidOn)] Value : {validOn}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                            }
                        }
                        break;
                    case "Control_Ho_Avbl":
                        {
                            bool selectedValue = (bool)control.SelectedValue;
                            int nSigHoAvbl, nSigReq, nSigReady = -1;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetE84SignalOutOptions(out nSigHoAvbl, out nSigReq, out nSigReady);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (Control_Ho_Avbl)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }
                            if ((selectedValue ? 1 : 0) != nSigHoAvbl)
                            {
                                if (nSigHoAvbl == 0)
                                    nSigHoAvbl = 1;
                                else if (nSigHoAvbl == 1)
                                    nSigHoAvbl = 0;

                                E84Controller.CommModule.SetE84SignalOutOptions(nSigHoAvbl, nSigReq, nSigReady);

                                //옵션변경 후 파라미터에 저장
                                (E84Controller.E84ModuleParaemter as E84ModuleParameter).Control_HoAvblOff.Value = nSigHoAvbl;
                                LoggerManager.Debug($"[E84 Option Set (Control_HoAvblOff)] Value : {nSigHoAvbl}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                            }

                        }
                        break;
                    case "Control_Ready":
                        {
                            bool selectedValue = (bool)control.SelectedValue;
                            int nSigHoAvbl, nSigReq, nSigReady = -1;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetE84SignalOutOptions(out nSigHoAvbl, out nSigReq, out nSigReady);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (Control_Ready)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }
                            if ((selectedValue ? 1 : 0) != nSigReady)
                            {
                                if (nSigReady == 0)
                                    nSigReady = 1;
                                else if (nSigReady == 1)
                                    nSigReady = 0;

                                E84Controller.CommModule.SetE84SignalOutOptions(nSigHoAvbl, nSigReq, nSigReady);

                                //옵션변경 후 파라미터에 저장
                                (E84Controller.E84ModuleParaemter as E84ModuleParameter).Control_ReadyOff.Value = nSigReady;
                                LoggerManager.Debug($"[E84 Option Set (Control_ReadyOff)] Value : {nSigReady}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                            }
                        }

                        break;
                    case "Control_Req":
                        {
                            bool selectedValue = (bool)control.SelectedValue;
                            int nSigHoAvbl, nSigReq, nSigReady = -1;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetE84SignalOutOptions(out nSigHoAvbl, out nSigReq, out nSigReady);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (Control_Req)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }
                            if ((selectedValue ? 1 : 0) != nSigReq)
                            {
                                if (nSigReq == 0)
                                    nSigReq = 1;
                                else if (nSigReq == 1)
                                    nSigReq = 0;

                                E84Controller.CommModule.SetE84SignalOutOptions(nSigHoAvbl, nSigReq, nSigReady);

                                //옵션변경 후 파라미터에 저장
                                (E84Controller.E84ModuleParaemter as E84ModuleParameter).Control_ReqOff.Value = nSigReq;
                                LoggerManager.Debug($"[E84 Option Set (Control_ReqOff)] Value : {nSigReq}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                            }
                        }

                        break;
                    case "RVAUXIN0":
                        {
                            if (control.SelectedValue != null)
                            {
                                bool selectedValue = (bool)control.SelectedValue;
                                int rvAuxInput0, rvAuxInput1, rvAuxInput2, rvAuxInput3, rvAuxInput4, rvAuxInput5, rvAuxOutput0 = -1;
                                int retVal = 0;

                                retVal = E84Controller.CommModule.GetAuxReverseOption(out rvAuxInput0, out rvAuxInput1, out rvAuxInput2, out rvAuxInput3, out rvAuxInput4, out rvAuxInput5, out rvAuxOutput0);

                                if (retVal == -2)
                                {
                                    LoggerManager.Debug($"[E84 is not connected (RVAUXIN0)] " +
                                        $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                        $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                    break;
                                }
                                if ((selectedValue ? 1 : 0) != rvAuxInput0)
                                {
                                    if (rvAuxInput0 == 0)
                                        rvAuxInput0 = 1;
                                    else if (rvAuxInput0 == 1)
                                        rvAuxInput0 = 0;

                                    E84Controller.CommModule.SetAuxReverseOption(rvAuxInput0, rvAuxInput1, rvAuxInput2, rvAuxInput3, rvAuxInput4, rvAuxInput5, rvAuxOutput0);

                                    //옵션변경 후 파라미터에 저장
                                    (E84Controller.E84ModuleParaemter as E84ModuleParameter).RVAUXIN0.Value = rvAuxInput0;
                                    LoggerManager.Debug($"[E84 Option Set (RVAUXIN0)] Value : {rvAuxInput0}, " +
                                        $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                        $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                }
                            }
                        }
                        break;
                    case "RVAUXIN1":
                        {
                            if (control.SelectedValue != null)
                            {
                                bool selectedValue = (bool)control.SelectedValue;
                                int rvAuxInput0, rvAuxInput1, rvAuxInput2, rvAuxInput3, rvAuxInput4, rvAuxInput5, rvAuxOutput0 = -1;
                                int retVal = 0;

                                retVal = E84Controller.CommModule.GetAuxReverseOption(out rvAuxInput0, out rvAuxInput1, out rvAuxInput2, out rvAuxInput3, out rvAuxInput4, out rvAuxInput5, out rvAuxOutput0);

                                if (retVal == -2)
                                {
                                    LoggerManager.Debug($"[E84 is not connected (RVAUXIN1)] " +
                                        $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                        $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                    break;
                                }
                                if ((selectedValue ? 1 : 0) != rvAuxInput1)
                                {
                                    if (rvAuxInput1 == 0)
                                        rvAuxInput1 = 1;
                                    else if (rvAuxInput1 == 1)
                                        rvAuxInput1 = 0;

                                    E84Controller.CommModule.SetAuxReverseOption(rvAuxInput0, rvAuxInput1, rvAuxInput2, rvAuxInput3, rvAuxInput4, rvAuxInput5, rvAuxOutput0);

                                    //옵션변경 후 파라미터에 저장
                                    (E84Controller.E84ModuleParaemter as E84ModuleParameter).RVAUXIN1.Value = rvAuxInput1;
                                    LoggerManager.Debug($"[E84 Option Set (RVAUXIN1)] Value : {rvAuxInput1}, " +
                                        $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                        $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                }
                            }
                        }
                        break;
                    case "RVAUXIN2":
                        {
                            if (control.SelectedValue != null)
                            {
                                bool selectedValue = (bool)control.SelectedValue;
                                int rvAuxInput0, rvAuxInput1, rvAuxInput2, rvAuxInput3, rvAuxInput4, rvAuxInput5, rvAuxOutput0 = -1;
                                int retVal = 0;

                                retVal = E84Controller.CommModule.GetAuxReverseOption(out rvAuxInput0, out rvAuxInput1, out rvAuxInput2, out rvAuxInput3, out rvAuxInput4, out rvAuxInput5, out rvAuxOutput0);

                                if (retVal == -2)
                                {
                                    LoggerManager.Debug($"[E84 is not connected (RVAUXIN2)] " +
                                        $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                        $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                    break;
                                }
                                if ((selectedValue ? 1 : 0) != rvAuxInput2)
                                {
                                    if (rvAuxInput2 == 0)
                                        rvAuxInput2 = 1;
                                    else if (rvAuxInput2 == 1)
                                        rvAuxInput2 = 0;

                                    E84Controller.CommModule.SetAuxReverseOption(rvAuxInput0, rvAuxInput1, rvAuxInput2, rvAuxInput3, rvAuxInput4, rvAuxInput5, rvAuxOutput0);

                                    //옵션변경 후 파라미터에 저장
                                    (E84Controller.E84ModuleParaemter as E84ModuleParameter).RVAUXIN2.Value = rvAuxInput2;
                                    LoggerManager.Debug($"[E84 Option Set (RVAUXIN2)] Value : {rvAuxInput2}, " +
                                        $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                        $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                }
                            }
                        }
                        break;
                    case "RVAUXIN3":
                        {
                            if (control.SelectedValue != null)
                            {
                                bool selectedValue = (bool)control.SelectedValue;
                                int rvAuxInput0, rvAuxInput1, rvAuxInput2, rvAuxInput3, rvAuxInput4, rvAuxInput5, rvAuxOutput0 = -1;
                                int retVal = 0;

                                retVal = E84Controller.CommModule.GetAuxReverseOption(out rvAuxInput0, out rvAuxInput1, out rvAuxInput2, out rvAuxInput3, out rvAuxInput4, out rvAuxInput5, out rvAuxOutput0);

                                if (retVal == -2)
                                {
                                    LoggerManager.Debug($"[E84 is not connected (RVAUXIN3)] " +
                                        $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                        $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                    break;
                                }
                                if ((selectedValue ? 1 : 0) != rvAuxInput3)
                                {
                                    if (rvAuxInput3 == 0)
                                        rvAuxInput3 = 1;
                                    else if (rvAuxInput3 == 1)
                                        rvAuxInput3 = 0;

                                    E84Controller.CommModule.SetAuxReverseOption(rvAuxInput0, rvAuxInput1, rvAuxInput2, rvAuxInput3, rvAuxInput4, rvAuxInput5, rvAuxOutput0);

                                    //옵션변경 후 파라미터에 저장
                                    (E84Controller.E84ModuleParaemter as E84ModuleParameter).RVAUXIN3.Value = rvAuxInput3;
                                    LoggerManager.Debug($"[E84 Option Set (RVAUXIN3)] Value : {rvAuxInput3}, " +
                                        $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                        $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                }
                            }
                        }
                        break;
                    case "RVAUXIN4":
                        {
                            if (control.SelectedValue != null)
                            {
                                bool selectedValue = (bool)control.SelectedValue;
                                int rvAuxInput0, rvAuxInput1, rvAuxInput2, rvAuxInput3, rvAuxInput4, rvAuxInput5, rvAuxOutput0 = -1;
                                int retVal = 0;

                                retVal = E84Controller.CommModule.GetAuxReverseOption(out rvAuxInput0, out rvAuxInput1, out rvAuxInput2, out rvAuxInput3, out rvAuxInput4, out rvAuxInput5, out rvAuxOutput0);

                                if (retVal == -2)
                                {
                                    LoggerManager.Debug($"[E84 is not connected (RVAUXIN4)] " +
                                        $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                        $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                    break;
                                }
                                if ((selectedValue ? 1 : 0) != rvAuxInput4)
                                {
                                    if (rvAuxInput4 == 0)
                                        rvAuxInput4 = 1;
                                    else if (rvAuxInput4 == 1)
                                        rvAuxInput4 = 0;

                                    E84Controller.CommModule.SetAuxReverseOption(rvAuxInput0, rvAuxInput1, rvAuxInput2, rvAuxInput3, rvAuxInput4, rvAuxInput5, rvAuxOutput0);

                                    //옵션변경 후 파라미터에 저장
                                    (E84Controller.E84ModuleParaemter as E84ModuleParameter).RVAUXIN4.Value = rvAuxInput4;
                                    LoggerManager.Debug($"[E84 Option Set (RVAUXIN4)] Value : {rvAuxInput4}, " +
                                        $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                        $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                }
                            }
                        }
                        break;
                    case "RVAUXIN5":
                        {
                            if (control.SelectedValue != null)
                            {
                                bool selectedValue = (bool)control.SelectedValue;
                                int rvAuxInput0, rvAuxInput1, rvAuxInput2, rvAuxInput3, rvAuxInput4, rvAuxInput5, rvAuxOutput0 = -1;
                                int retVal = 0;

                                retVal = E84Controller.CommModule.GetAuxReverseOption(out rvAuxInput0, out rvAuxInput1, out rvAuxInput2, out rvAuxInput3, out rvAuxInput4, out rvAuxInput5, out rvAuxOutput0);

                                if (retVal == -2)
                                {
                                    LoggerManager.Debug($"[E84 is not connected (RVAUXIN5)] " +
                                        $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                        $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                    break;
                                }
                                if ((selectedValue ? 1 : 0) != rvAuxInput5)
                                {
                                    if (rvAuxInput5 == 0)
                                        rvAuxInput5 = 1;
                                    else if (rvAuxInput5 == 1)
                                        rvAuxInput5 = 0;

                                    E84Controller.CommModule.SetAuxReverseOption(rvAuxInput0, rvAuxInput1, rvAuxInput2, rvAuxInput3, rvAuxInput4, rvAuxInput5, rvAuxOutput0);

                                    //옵션변경 후 파라미터에 저장
                                    (E84Controller.E84ModuleParaemter as E84ModuleParameter).RVAUXIN5.Value = rvAuxInput5;
                                    LoggerManager.Debug($"[E84 Option Set (RVAUXIN5)] Value : {rvAuxInput5}, " +
                                        $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                        $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                }
                            }
                        }
                        break;
                    case "RVAUXOUT0":
                        {
                            if (control.SelectedValue != null)
                            {
                                bool selectedValue = (bool)control.SelectedValue;
                                int rvAuxInput0, rvAuxInput1, rvAuxInput2, rvAuxInput3, rvAuxInput4, rvAuxInput5, rvAuxOutput0 = -1;
                                int retVal = 0;

                                retVal = E84Controller.CommModule.GetAuxReverseOption(out rvAuxInput0, out rvAuxInput1, out rvAuxInput2, out rvAuxInput3, out rvAuxInput4, out rvAuxInput5, out rvAuxOutput0);

                                if (retVal == -2)
                                {
                                    LoggerManager.Debug($"[E84 is not connected (RVAUXOUT0)] " +
                                        $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                        $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                    break;
                                }
                                if ((selectedValue ? 1 : 0) != rvAuxOutput0)
                                {
                                    if (rvAuxOutput0 == 0)
                                        rvAuxOutput0 = 1;
                                    else if (rvAuxOutput0 == 1)
                                        rvAuxOutput0 = 0;

                                    E84Controller.CommModule.SetAuxReverseOption(rvAuxInput0, rvAuxInput1, rvAuxInput2, rvAuxInput3, rvAuxInput4, rvAuxInput5, rvAuxOutput0);

                                    //옵션변경 후 파라미터에 저장
                                    (E84Controller.E84ModuleParaemter as E84ModuleParameter).RVAUXOUT0.Value = rvAuxOutput0;
                                    LoggerManager.Debug($"[E84 Option Set (RVAUXOUT0)] Value : {rvAuxOutput0}, " +
                                        $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                        $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
                this.E84Module().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
                return _TextBoxClickCommand;
            }
        }

        private void TextBoxClickCommandFunc(Object param)
        {
            try
            {
                object[] _param = param as object[];
                TextBox tb = _param[0] as TextBox;
                string mode = _param[1].ToString();

                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT | KB_TYPE.SPECIAL, -50, 150);

                switch (mode)
                {
                    case "TP1":
                        {
                            int tp1, tp2, tp3, tp4, tp5, tp6 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetTpTimeout(out tp1, out tp2, out tp3, out tp4, out tp5, out tp6);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (TP1)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            tp1 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetTpTimeout(tp1, tp2, tp3, tp4, tp5, tp6);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).TP1.Value = tp1;
                            LoggerManager.Debug($"[E84 Option Set (TP1)] Value : {tp1}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "TP2":
                        {
                            int tp1, tp2, tp3, tp4, tp5, tp6 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetTpTimeout(out tp1, out tp2, out tp3, out tp4, out tp5, out tp6);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (TP2)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            tp2 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetTpTimeout(tp1, tp2, tp3, tp4, tp5, tp6);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).TP2.Value = tp2;
                            LoggerManager.Debug($"[E84 Option Set (TP2)] Value : {tp2}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "TP3":
                        {
                            int tp1, tp2, tp3, tp4, tp5, tp6 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetTpTimeout(out tp1, out tp2, out tp3, out tp4, out tp5, out tp6);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (TP3)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            tp3 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetTpTimeout(tp1, tp2, tp3, tp4, tp5, tp6);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).TP3.Value = tp3;
                            LoggerManager.Debug($"[E84 Option Set (TP3)] Value : {tp3}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "TP4":
                        {
                            int tp1, tp2, tp3, tp4, tp5, tp6 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetTpTimeout(out tp1, out tp2, out tp3, out tp4, out tp5, out tp6);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (TP4)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            tp4 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetTpTimeout(tp1, tp2, tp3, tp4, tp5, tp6);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).TP4.Value = tp4;
                            LoggerManager.Debug($"[E84 Option Set (TP4)] Value : {tp4}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "TP5":
                        {
                            int tp1, tp2, tp3, tp4, tp5, tp6 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetTpTimeout(out tp1, out tp2, out tp3, out tp4, out tp5, out tp6);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (TP5)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            tp5 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetTpTimeout(tp1, tp2, tp3, tp4, tp5, tp6);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).TP5.Value = tp5;
                            LoggerManager.Debug($"[E84 Option Set (TP5)] Value : {tp5}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "TP6":
                        {
                            int tp1, tp2, tp3, tp4, tp5, tp6 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetTpTimeout(out tp1, out tp2, out tp3, out tp4, out tp5, out tp6);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (TP6)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            tp6 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetTpTimeout(tp1, tp2, tp3, tp4, tp5, tp6);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).TP6.Value = tp6;
                            LoggerManager.Debug($"[E84 Option Set (TP6)] Value : {tp6}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "EhternetID":

                        break;
                    case "Timer":
                        {

                            int use, inputType, actionType, timer = -1;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetClampOptions(out use, out inputType, out actionType, out timer);
                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (ClampOffWaitTime)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            int gettimer = -1;
                            int.TryParse(tb.Text, out gettimer);
                            if (gettimer != timer)
                            {
                                E84Controller.CommModule.SetClampOptions(use, inputType, actionType, gettimer);
                                //옵션변경 후 파라미터에 저장
                                (E84Controller.E84ModuleParaemter as E84ModuleParameter).ClampOffWaitTime.Value = gettimer;
                                LoggerManager.Debug($"[E84 Option Set (ClampOffWaitTime)] Value : {gettimer}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                            }


                        }
                        break;
                    case "TD0":
                        {
                            int td0, td1 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetTdDelayTime(out td0, out td1);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (TD0)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            td0 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetTdDelayTime(td0, td1);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).TD0.Value = td0;
                            LoggerManager.Debug($"[E84 Option Set (TD0)] Value : {td0}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "TD1":
                        {
                            int td0, td1 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetTdDelayTime(out td0, out td1);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (TD1)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            td1 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetTdDelayTime(td0, td1);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).TD1.Value = td1;
                            LoggerManager.Debug($"[E84 Option Set (TD1)] Value : {td1}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "HeartBeat":
                        {
                            int heartBeat = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetHeartBeatTime(out heartBeat);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (HeartBeat)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            heartBeat = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetHeartBeatTime(heartBeat);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).HeartBeat.Value = heartBeat;
                            LoggerManager.Debug($"[E84 Option Set (HeartBeat)] Value : {heartBeat}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "InputFilter":
                        {
                            int inputFilter = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetInputFilterTime(out inputFilter);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (InputFilter)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            inputFilter = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetInputFilterTime(inputFilter);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).InputFilter.Value = inputFilter;
                            LoggerManager.Debug($"[E84 Option Set (InputFilter)] Value : {inputFilter}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "AUXIN0":
                        {
                            int auxin0, auxin1, auxin2, auxin3, auxin4, auxin5, auxout0 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetAuxOptions(out auxin0, out auxin1, out auxin2, out auxin3, out auxin4, out auxin5, out auxout0);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (AUXIN0)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            auxin0 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetAuxOptions(auxin0, auxin1, auxin2, auxin3, auxin4, auxin5, auxout0);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).AUXIN0.Value = auxin0;
                            LoggerManager.Debug($"[E84 Option Set (AUXIN0)] Value : {auxin0}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "AUXIN1":
                        {
                            int auxin0, auxin1, auxin2, auxin3, auxin4, auxin5, auxout0 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetAuxOptions(out auxin0, out auxin1, out auxin2, out auxin3, out auxin4, out auxin5, out auxout0);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (AUXIN1)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            auxin1 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetAuxOptions(auxin0, auxin1, auxin2, auxin3, auxin4, auxin5, auxout0);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).AUXIN1.Value = auxin1;
                            LoggerManager.Debug($"[E84 Option Set (AUXIN1)] Value : {auxin1}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "AUXIN2":
                        {
                            int auxin0, auxin1, auxin2, auxin3, auxin4, auxin5, auxout0 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetAuxOptions(out auxin0, out auxin1, out auxin2, out auxin3, out auxin4, out auxin5, out auxout0);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (AUXIN2)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            auxin2 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetAuxOptions(auxin0, auxin1, auxin2, auxin3, auxin4, auxin5, auxout0);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).AUXIN2.Value = auxin2;
                            LoggerManager.Debug($"[E84 Option Set (AUXIN2)] Value : {auxin2}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "AUXIN3":
                        {
                            int auxin0, auxin1, auxin2, auxin3, auxin4, auxin5, auxout0 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetAuxOptions(out auxin0, out auxin1, out auxin2, out auxin3, out auxin4, out auxin5, out auxout0);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (AUXIN3)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            auxin3 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetAuxOptions(auxin0, auxin1, auxin2, auxin3, auxin4, auxin5, auxout0);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).AUXIN3.Value = auxin3;
                            LoggerManager.Debug($"[E84 Option Set (AUXIN3)] Value : {auxin3}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "AUXIN4":
                        {
                            int auxin0, auxin1, auxin2, auxin3, auxin4, auxin5, auxout0 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetAuxOptions(out auxin0, out auxin1, out auxin2, out auxin3, out auxin4, out auxin5, out auxout0);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (AUXIN4)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            auxin4 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetAuxOptions(auxin0, auxin1, auxin2, auxin3, auxin4, auxin5, auxout0);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).AUXIN4.Value = auxin4;
                            LoggerManager.Debug($"[E84 Option Set (AUXIN4)] Value : {auxin4}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "AUXIN5":
                        {
                            int auxin0, auxin1, auxin2, auxin3, auxin4, auxin5, auxout0 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetAuxOptions(out auxin0, out auxin1, out auxin2, out auxin3, out auxin4, out auxin5, out auxout0);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (AUXIN5)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            auxin5 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetAuxOptions(auxin0, auxin1, auxin2, auxin3, auxin4, auxin5, auxout0);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).AUXIN5.Value = auxin5;
                            LoggerManager.Debug($"[E84 Option Set (AUXIN5)] Value : {auxin5}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "AUXOUT0":
                        {
                            int auxin0, auxin1, auxin2, auxin3, auxin4, auxin5, auxout0 = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetAuxOptions(out auxin0, out auxin1, out auxin2, out auxin3, out auxin4, out auxin5, out auxout0);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (AUXOUT0)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            auxout0 = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetAuxOptions(auxin0, auxin1, auxin2, auxin3, auxin4, auxin5, auxout0);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).AUXOUT0.Value = auxout0;
                            LoggerManager.Debug($"[E84 Option Set (AUXOUT0)] Value : {auxout0}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "RecoveryTimeout":
                        {
                            int timeout, retry = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetCommunication(out timeout, out retry);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (RecoveryTimeout)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            timeout = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetCommunication(timeout, retry);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).RecoveryTimeout.Value = timeout;
                            LoggerManager.Debug($"[E84 Option Set (RecoveryTimeout)] Value : {timeout}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    case "RetryCount":
                        {
                            int timeout, retry = 0;
                            int retVal = 0;

                            retVal = E84Controller.CommModule.GetCommunication(out timeout, out retry);

                            if (retVal == -2)
                            {
                                LoggerManager.Debug($"[E84 is not connected (RecoveryTimeout)] " +
                                    $"Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                                break;
                            }

                            retry = Convert.ToInt32(tb.Text);
                            E84Controller.CommModule.SetCommunication(timeout, retry);

                            //옵션변경 후 파라미터에 저장
                            (E84Controller.E84ModuleParaemter as E84ModuleParameter).RetryCount.Value = retry;
                            LoggerManager.Debug($"[E84 Option Set (RetryCount)] Value : {retry}, " +
                                    $" Controller Type : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).E84OPModuleType}," +
                                    $" Index : {(E84Controller.E84ModuleParaemter as E84ModuleParameter).FoupIndex}");
                        }
                        break;
                    default:
                        break;
                }
                this.E84Module().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<string> _ClickControlCommand;
        public ICommand ClickControlCommand
        {
            get
            {
                if (null == _ClickControlCommand) _ClickControlCommand = new RelayCommand<string>(UpdateDescriptionFunc);
                return _ClickControlCommand;
            }
        }

        private void UpdateDescriptionFunc(string propertyName)
        {
            try
            {
                PropertyName = propertyName;
                switch (propertyName)
                {
                    case "UseLP1":
                        {
                            Description = " False : Not use Load__Port 1 \r True : Use Load__Port1";
                        }
                        break;
                    case "InputType":
                        {
                            Description = " False : Wiring \r True : Command from Host";
                        }
                        break;
                    case "Timer":
                        {
                            Description = "Unclamp Checking Timer (Unit: sec, Max: 20)";
                        }
                        break;
                    case "ActionType":
                        {
                            Description = " False : immediateley error of unclamp status \r True : waiting until unclamp status";
                        }
                        break;
                    case "UseClamp":
                        {
                            Description = " False : Not use Clamp 1 \r True : Use Clamp";
                        }
                        break;
                    case "UseES":
                        {
                            Description = " [When Light Curtain Status happened] " +
                                "\r Faile : no action about ES \r True : off action about ES";
                        }
                        break;
                    case "UseHOAVBL":
                        {
                            Description = " [When Light Curtain Status happened]" +
                                " \r Faile : no action about HO_AVBL \r True : off action about HO_AVBL";
                        }
                        break;
                    case "ReadyOff":
                        {
                            Description = " [BUSY off -> TR__REQ off -> COMPT on] \r False : sequentially \r True : in any order ";
                        }
                        break;
                    case "UseCS1":
                        {
                            Description = " False : Not use CS1 signal \r True : Use CS1 signal";
                        }
                        break;
                    case "ValidOff":
                        {
                            Description = " [VALID off -> COMPT off -> CS off] \r False : sequentially \r True : in any order";
                        }
                        break;
                    case "ValidOn":
                        {
                            Description = " [CS on -> VALID on] \r False : sequentially \r True : in any order";
                        }
                        break;
                    case "Control_Ho_Avbl":
                        {
                            Description = " [When Event Status happened] \r False : E84 Controls Ho_Avbl Sig. \r True : Master Controls Ho_Avbl Sig.  \r *this option offer upper 3013v firmware version.";
                        }
                        break;
                    case "Control_Ready":
                        {
                            Description = " [When Event Status happened] \r False : E84 Controls Ready Sig. \r True : Master Controls Ready Sig. \r *this option offer upper 3013v firmware version.";
                        }
                        break;
                    case "Control_Req":
                        {
                            Description = " [When Event Status happened] \r False : E84 Controls Req Sig. \r True : Master Controls Req Sig. \r *this option offer upper 3013v firmware version.";
                        }
                        break;
                    case "RVAUXIN0":
                        {
                            Description = " 0 --> Signal On \r 1 --> Signal Off";
                        }
                        break;
                    case "RVAUXIN1":
                        {
                            Description = " 0 --> Signal On \r 1 --> Signal Off";
                        }
                        break;
                    case "RVAUXIN2":
                        {
                            Description = " 0 --> Signal On \r 1 --> Signal Off";
                        }
                        break;
                    case "RVAUXIN3":
                        {
                            Description = " 0 --> Signal On \r 1 --> Signal Off";
                        }
                        break;
                    case "RVAUXIN4":
                        {
                            Description = " 0 --> Signal On \r 1 --> Signal Off";
                        }
                        break;
                    case "RVAUXIN5":
                        {
                            Description = " 0 --> Signal On \r 1 --> Signal Off";
                        }
                        break;
                    case "RVAUXOUT0":
                        {
                            Description = " 0 --> Signal On \r 1 --> Signal Off";
                        }
                        break;
                    case "TP1":
                        {
                            Description = " L__REQ(or U__REQ) On ~ TR__REQ On \r 1-->1sec, limit : 999";
                        }
                        break;
                    case "TP2":
                        {
                            Description = " READY On ~ BUSY On \r 1-->100msec, limit : 999";
                        }
                        break;
                    case "TP3":
                        {
                            Description = " BUSY On ~ Carrier Dectedted(or Undetected) \r 1-->1sec, limit : 999";
                        }
                        break;
                    case "TP4":
                        {
                            Description = " L__REQ(or U__REQ) Off ~ TR__REQ Off \r 1-->1sec, limit : 999";
                        }
                        break;
                    case "TP5":
                        {
                            Description = " READY Off ~ VALID Off \r 1-->100msec, limit : 999";
                        }
                        break;
                    case "TP6":
                        {
                            Description = " VALID Off ~ VALID On \r 1-->100msec, limit : 999";
                        }
                        break;
                    case "EhternetID":
                        {
                            Description = " Select Ethernet__ID";
                        }
                        break;
                    case "TD0":
                        {
                            Description = " CS__0(or CS__1) ~ VALID On \r 1-->1sec, limit : 999";
                        }
                        break;
                    case "TD1":
                        {
                            Description = " VALID Off ~ VALID On \r (**) TD1 < TP6 \r 1-->1sec, limit : 999 ";
                        }
                        break;
                    case "HeartBeat":
                        {
                            Description = " U (0~999) \r 1-->1sec, limit : 999";
                        }
                        break;
                    case "InputFilter":
                        {
                            Description = " U (0~999) \r 1-->10msec, limit : 100";
                        }
                        break;
                    case "AUXIN0":
                        {
                            Description = " Carrier exist status of LP0 \r 0 : Not Used \r 1 : LP0 Placement_Sensor";
                        }
                        break;
                    case "AUXIN1":
                        {
                            Description = " Carrier exist status of LP1 \r 0 : Not Used \r 1 : LP1 Placement_Sensor";
                        }
                        break;
                    case "AUXIN2":
                        {
                            Description = " Auxiliary Light Curtain Signal \r 0 : Not Used \r 1 : Auxiliary Light_Curtain_Signal In";
                        }
                        break;
                    case "AUXIN3":
                        {
                            Description = " Light Curtain Signal \r 0 : Not Used \r 1 : Light_Curtain_Signal In";
                        }
                        break;
                    case "AUXIN4":
                        {
                            Description = " Clamp status of LP0 \r 0 : Not Used \r 1 :  User Input";
                        }
                        break;
                    case "AUXIN5":
                        {
                            Description = " Clamp status of LP1 \r 0 : Not Used \r 1 :  User Input";
                        }
                        break;
                    case "AUXOUT0":
                        {
                            Description = " 0 : Not Used \r 1 : Use as User Output";
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SetCarrierAttachStateCommand;
        public ICommand SetCarrierAttachStateCommand
        {
            get
            {
                if (null == _SetCarrierAttachStateCommand) _SetCarrierAttachStateCommand = new RelayCommand<object>(SetCarrierAttachStateCommandFunc);
                return _SetCarrierAttachStateCommand;
            }
        }
        private void SetCarrierAttachStateCommandFunc(object param)
        {
            try
            {
                int foupindex = (int)param;
                var foupController = this.FoupOpModule().GetFoupController(foupindex);
                this.GetFoupIO().IOMap.Inputs.DI_CST12_PRESs[foupindex - 1].Value = true;
                this.GetFoupIO().IOMap.Inputs.DI_CST12_PRES2s[foupindex - 1].Value = true;
                this.GetFoupIO().IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2.Value = true;
                foupController.FoupModuleInfo.FoupPRESENCEState = FoupPRESENCEStateEnum.CST_ATTACH;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SetCarrierDetachStateCommand;
        public ICommand SetCarrierDetachStateCommand
        {
            get
            {
                if (null == _SetCarrierDetachStateCommand) _SetCarrierDetachStateCommand = new RelayCommand<object>(SetCarrierDetachStateCommandFunc);
                return _SetCarrierDetachStateCommand;
            }
        }
        private void SetCarrierDetachStateCommandFunc(object param)
        {
            try
            {
                int foupindex = (int)param;
                var foupController = this.FoupOpModule().GetFoupController(foupindex);
                this.GetFoupIO().IOMap.Inputs.DI_CST12_PRESs[foupindex - 1].Value = false;
                this.GetFoupIO().IOMap.Inputs.DI_CST12_PRES2s[foupindex - 1].Value = false;
                this.GetFoupIO().IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2.Value = false;
                foupController.FoupModuleInfo.FoupPRESENCEState = FoupPRESENCEStateEnum.CST_DETACH;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
    }


}