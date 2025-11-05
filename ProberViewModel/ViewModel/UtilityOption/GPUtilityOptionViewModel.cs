namespace ProberViewModel.ViewModel.UtilityOption
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Threading;
    using System.Windows.Controls;
    using Autofac;
    using LoaderBase;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Foup;
    using RelayCommandBase;
    using VirtualKeyboardControl;
    using System.Collections.Generic;

    public class GPUtilityOptionViewModel : INotifyPropertyChanged, IFactoryModule, IMainScreenViewModel
    {
        #region <remarks> PropertyChanged </remarks>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region <remarks> Property </remarks>

        readonly Guid _ViewModelGUID = new Guid("9e1c9a41-c7ff-4e68-97db-6c67ccbac94a");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;

        private ILoaderSupervisor _LoaderMaster;
        public ILoaderSupervisor LoaderMaster
        {
            get { return _LoaderMaster; }
            set
            {
                if (value != _LoaderMaster)
                {
                    _LoaderMaster = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region <remarks> Cassette Lock Property </remarks>

        private bool _IsCassetteAutoLockEnable;
        public bool IsCassetteAutoLockEnable
        {
            get { return _IsCassetteAutoLockEnable; }
            set
            {
                if (value != _IsCassetteAutoLockEnable)
                {
                    _IsCassetteAutoLockEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCassetteAutoLockLeftOHTEnable;
        public bool IsCassetteAutoLockLeftOHTEnable
        {
            get { return _IsCassetteAutoLockLeftOHTEnable; }
            set
            {
                if (value != _IsCassetteAutoLockLeftOHTEnable)
                {
                    _IsCassetteAutoLockLeftOHTEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCassetteLockToggleEnable;
        public bool IsCassetteLockToggleEnable
        {
            get { return _IsCassetteLockToggleEnable; }
            set
            {
                if (value != _IsCassetteLockToggleEnable)
                {
                    _IsCassetteLockToggleEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _PortFullLockMode;
        public bool PortFullLockMode
        {
            get { return _PortFullLockMode; }
            set
            {
                if (value != _PortFullLockMode)
                {
                    _PortFullLockMode = value;
                    RaisePropertyChanged();
                    if(_PortFullLockMode == true)
                    {
                        ChangePortMode(true,false);
                    }
                }
            }
        }

        private bool _PortEachLockMode;
        public bool PortEachLockMode
        {
            get { return _PortEachLockMode; }
            set
            {
                if (value != _PortEachLockMode)
                {
                    _PortEachLockMode = value;
                    RaisePropertyChanged();
                    if (_PortEachLockMode == true)
                    {
                        ChangePortMode(false, true);
                    }
                }
            }
        }



        private ObservableCollection<int> _FoupIndexs
             = new ObservableCollection<int>();
        public ObservableCollection<int> FoupIndexs
        {
            get { return _FoupIndexs; }
            set
            {
                if (value != _FoupIndexs)
                {
                    _FoupIndexs = value;
                    RaisePropertyChanged();
                    ChangeFoupIndex();
                }
            }
        }

        private int _SelectedFoupNum;
        public int SelectedFoupNum
        {
            get { return _SelectedFoupNum; }
            set
            {
                if (value != _SelectedFoupNum)
                {
                    _SelectedFoupNum = value;
                    RaisePropertyChanged();
                    ChangeFoupIndex();
                }
            }
        }


        private ObservableCollection<int> _CassetteTypeFoupIndexs
             = new ObservableCollection<int>();
        public ObservableCollection<int> CassetteTypeFoupIndexs
        {
            get { return _CassetteTypeFoupIndexs; }
            set
            {
                if (value != _CassetteTypeFoupIndexs)
                {
                    _CassetteTypeFoupIndexs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedCassetteTypeFoupNum;
        public int SelectedCassetteTypeFoupNum
        {
            get { return _SelectedCassetteTypeFoupNum; }
            set
            {
                if (value != _SelectedCassetteTypeFoupNum)
                {
                    _SelectedCassetteTypeFoupNum = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<CassetteTypeEnum> _CassetteTypeEnums = new ObservableCollection<CassetteTypeEnum>();
        public ObservableCollection<CassetteTypeEnum> CassetteTypeEnums
        {
            get { return _CassetteTypeEnums; }
            set
            {
                if (value != _CassetteTypeEnums)
                {
                    _CassetteTypeEnums = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CassetteTypeEnum _SelectedCassetteTypeEnum = CassetteTypeEnum.FOUP_25;
        public CassetteTypeEnum SelectedCassetteTypeEnum
        {
            get { return _SelectedCassetteTypeEnum; }
            set
            {
                _SelectedCassetteTypeEnum = value;
                RaisePropertyChanged();
            }
        }

        //private CassetteLockModeEnum _SelectedFoupCassetteLockE84ManualMode
        //     = CassetteLockModeEnum.UNDEFINED;
        //public CassetteLockModeEnum SelectedFoupCassetteLockE84ManualMode
        //{
        //    get { return _SelectedFoupCassetteLockE84ManualMode; }
        //    set
        //    {
        //        if (value != _SelectedFoupCassetteLockE84ManualMode)
        //        {
        //            _SelectedFoupCassetteLockE84ManualMode = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private CassetteLockModeEnum _SelectedFoupCassetteLockE84AutoMode
        //     = CassetteLockModeEnum.UNDEFINED;
        //public CassetteLockModeEnum SelectedFoupCassetteLockE84AutoMode
        //{
        //    get { return _SelectedFoupCassetteLockE84AutoMode; }
        //    set
        //    {
        //        if (value != _SelectedFoupCassetteLockE84AutoMode)
        //        {
        //            _SelectedFoupCassetteLockE84AutoMode = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private CassetteLockModeEnum _CassetteFullLockE84AutoMode;
        public CassetteLockModeEnum CassetteFullLockE84AutoMode
        {
            get { return _CassetteFullLockE84AutoMode; }
            set
            {
                if (value != _CassetteFullLockE84AutoMode)
                {
                    _CassetteFullLockE84AutoMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CassetteLockModeEnum _CassetteFullLockE84ManualMode;
        public CassetteLockModeEnum CassetteFullLockE84ManualMode
        {
            get { return _CassetteFullLockE84ManualMode; }
            set
            {
                if (value != _CassetteFullLockE84ManualMode)
                {
                    _CassetteFullLockE84ManualMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84CassetteLockParam _E84CassetteLockParam
             = new E84CassetteLockParam();
        public E84CassetteLockParam E84CassetteLockParam
        {
            get { return _E84CassetteLockParam; }
            set
            {
                if (value != _E84CassetteLockParam)
                {
                    _E84CassetteLockParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84PortLockOptionParam _SelectedPortLockParam;
        public E84PortLockOptionParam SelectedPortLockParam
        {
            get { return _SelectedPortLockParam; }
            set
            {
                if (value != _SelectedPortLockParam)
                {
                    _SelectedPortLockParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84PresenceTypeEnum _SelectedE84PresenceType;
        public E84PresenceTypeEnum SelectedE84PresenceType
        {
            get { return _SelectedE84PresenceType; }
            set
            {
                if (value != _SelectedE84PresenceType)
                {
                    _SelectedE84PresenceType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _TimeoutOnPresenceAfterOnExistSensorMs;
        public long TimeoutOnPresenceAfterOnExistSensorMs
        {
            get { return _TimeoutOnPresenceAfterOnExistSensorMs; }
            set
            {
                if (value != _TimeoutOnPresenceAfterOnExistSensorMs)
                {
                    _TimeoutOnPresenceAfterOnExistSensorMs = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        #region <remarks> LOT_GEM Property </remarks>

        private bool _IsCassetteAutoUnloadAfterLotEnable;
        public bool IsCassetteAutoUnloadAfterLotEnable
        {
            get { return _IsCassetteAutoUnloadAfterLotEnable; }
            set
            {
                if (value != _IsCassetteAutoUnloadAfterLotEnable)
                {
                    _IsCassetteAutoUnloadAfterLotEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCancelCarrierEventNotRuning;
        public bool IsCancelCarrierEventNotRuning
        {
            get { return _IsCancelCarrierEventNotRuning; }
            set
            {
                if (value != _IsCancelCarrierEventNotRuning)
                {
                    _IsCancelCarrierEventNotRuning = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCassetteDetectEventAfterRFID;
        public bool IsCassetteDetectEventAfterRFID
        {
            get { return _IsCassetteDetectEventAfterRFID; }
            set
            {
                if (value != _IsCassetteDetectEventAfterRFID)
                {
                    _IsCassetteDetectEventAfterRFID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsLoaderLotEndBuzzerON;
        public bool IsLoaderLotEndBuzzerON
        {
            get { return _IsLoaderLotEndBuzzerON; }
            set
            {
                if (value != _IsLoaderLotEndBuzzerON)
                {
                    _IsLoaderLotEndBuzzerON = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsAlwaysCloseFoupCover;
        public bool IsAlwaysCloseFoupCover
        {
            get { return _IsAlwaysCloseFoupCover; }
            set
            {
                if (value != _IsAlwaysCloseFoupCover)
                {
                    _IsAlwaysCloseFoupCover = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region <remarks> LOT </remarks>

        private RelayCommand<Object> _LOTParamTextBoxClickCommand;
        public ICommand LOTParamTextBoxClickCommand
        {
            get
            {
                if (null == _LOTParamTextBoxClickCommand) _LOTParamTextBoxClickCommand = new RelayCommand<Object>(LOTParamTextBoxClickCommandFunc);
                return _LOTParamTextBoxClickCommand;
            }
        }

        private void LOTParamTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                string inputText = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT);
                string errorReason = null;
                bool ret = VerifyParameterRange(tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).ParentBinding.Path.Path, inputText, ref errorReason);
                if (ret == false)
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", $"{errorReason}", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
                else 
                {
                    tb.Text = inputText.ToString();
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                }
                LoaderMaster.SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            bool VerifyParameterRange(string path, string inputText, ref string errorReasonStr)
            {
                bool retval = true;
                if (path == "LoaderMaster.LotSysParam.ExecutionTimeoutError.Value")
                {
                    if (!int.TryParse(inputText, out int value))
                    {
                        retval = false;
                        errorReasonStr = "Please enter a number equal to or greater than 5. Please provide a valid input.";
                        return retval;
                    }

                    if (value < 5)
                    {
                        retval = false;
                        errorReasonStr = "Please enter a value of 5 or greater. Please provide a valid input.";
                    }
                }
                return retval;
            }
        }

        #endregion

        #endregion

        #region <remarks> Command </remarks>

        private RelayCommand<object> _SaveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (null == _SaveCommand) _SaveCommand = new RelayCommand<object>(SaveCommandFunc);
                return _SaveCommand;
            }
        }

        private void SaveCommandFunc(object param)
        {
            try
            {
                LoaderMaster.SetIsCassetteAutoLock(this.IsCassetteAutoLockEnable);
                LoaderMaster.SetIsCassetteAutoLockLeftOHT(this.IsCassetteAutoLockLeftOHTEnable);
                LoaderMaster.SetIsCassetteAutoUnloadAfterLot(this.IsCassetteAutoUnloadAfterLotEnable);
                LoaderMaster.SetIsCancelCarrierEventNotRuning(this.IsCancelCarrierEventNotRuning);
                LoaderMaster.SetIsCassetteDetectEventAfterRFID(this.IsCassetteDetectEventAfterRFID);
                LoaderMaster.SetIsLoaderLotEndBuzzerON(this.IsLoaderLotEndBuzzerON);
                LoaderMaster.SetIsAlwaysCloseFoupCover(this.IsAlwaysCloseFoupCover);
                this.E84Module().SetTimeoutOnPresenceAfterOnExistSensor(this.TimeoutOnPresenceAfterOnExistSensorMs);
                this.E84Module().SetE84PreseceType(this.SelectedE84PresenceType);
                E84CassetteLockParam.CassetteLockE84AutoMode = CassetteFullLockE84AutoMode;
                E84CassetteLockParam.CassetteLockE84ManualMode = CassetteFullLockE84ManualMode;
                E84CassetteLockParam.PortFullLockMode = PortFullLockMode;
                this.E84Module().SetE84CassetteLockParam(E84CassetteLockParam);
                this.E84Module().SaveSysParameter();
                LoaderMaster.SaveSysParameter();

                InitOptionValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<ComboBox> _ValidateSelectedCassetteCommand;
        public ICommand ValidateSelectedCassetteCommand
        {
            get
            {
                if (null == _ValidateSelectedCassetteCommand) _ValidateSelectedCassetteCommand = new RelayCommand<ComboBox>(ValidateSelectedCassetteCommandFunc);
                return _ValidateSelectedCassetteCommand;
            }
        }


        private void ValidateSelectedCassetteCommandFunc(ComboBox comboBox)
        {
            bool retval = false;
            string msg = "";
            try
            {
                if (SelectedCassetteTypeFoupNum <= 0)
                    return;
                if (comboBox == null)
                    return;
                if (comboBox.SelectedValue == null && comboBox.SelectionBoxItem == null)
                    return;
                int foupindex = SelectedCassetteTypeFoupNum;

                var foupController = this.FoupOpModule().GetFoupController(foupindex);

                var selectedValue = comboBox.SelectedValue ?? comboBox.SelectionBoxItem;

                var SelectedCassette = (CassetteTypeEnum)selectedValue;
                if (SelectedCassette != foupController.GetCassetteType())
                {
                    var wafersNotInCst = new List<TransferObject>();
                    var wafersNotInCst_Unknown = new List<TransferObject>();
                    var loaderInfo = LoaderMaster.Loader.GetLoaderInfo();
                    var transferObjects = loaderInfo.StateMap.GetTransferObjectAll();
                    foreach (var w in transferObjects)
                    {
                        var condition1 = ((w.OriginHolder.Index - 1) / 25) + 1 == foupindex;
                        var condition2 = w.CurrHolder.ModuleType != ModuleTypeEnum.SLOT;
                        var condition3 = w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT;
                        if (condition1 && condition2 && condition3)
                        {
                            wafersNotInCst.Add(w);
                            msg += $"[{w.CurrHolder.Label}]\n";
                        }
                    }

                    var Holders_Unknown = LoaderMaster.Loader.ModuleManager.FindModules<IWaferOwnable>().Where(item => item.Holder.Status == EnumSubsStatus.UNKNOWN).ToList();
                    if (Holders_Unknown != null && Holders_Unknown.Count > 0)
                    {
                        msg += "There is an unknown item in the loader module: ";
                    }
                    foreach (var item in Holders_Unknown)
                    {
                        var backupto = item.Holder.BackupTransferObject;
                        if (backupto == null)
                        {
                            msg += $"[unknown source of the wafer state is unknown:{item.ID.Label}]\n";
                        }
                        else
                        {
                            var condition1 = ((backupto.OriginHolder.Index - 1) / 25) + 1 == foupindex;
                            var condition2 = backupto.CurrHolder.ModuleType != ModuleTypeEnum.SLOT;
                            var condition3 = backupto.OriginHolder.ModuleType == ModuleTypeEnum.SLOT;
                            if (condition1 && condition2 && condition3)
                            {
                                wafersNotInCst_Unknown.Add(backupto);
                                msg += $"{item}, ";
                                LoggerManager.Debug($"{this.GetType().Name}, Wafer state from Foup#{foupindex} is unknown: {item}");
                            }
                            else
                            {
                                msg += $"{item}, ";
                                LoggerManager.Debug($"{this.GetType().Name}, Wafer orign is {backupto.OriginHolder.ModuleType} ({item})");
                            }
                        }
                    }

                    if (wafersNotInCst.Count == 0 && wafersNotInCst_Unknown.Count == 0 && Holders_Unknown.Count() == 0)
                    {
                        if (foupController.GetFoupModuleInfo()?.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_ATTACH)
                        {
                            EventCodeEnum ret = foupController.ValidationCassetteAvailable(out msg, SelectedCassette);
                            if (ret != EventCodeEnum.NONE)
                            {
                                retval = false;
                                msg = msg.TrimEnd('\n');
                                msg += "\nPlease reload a compatible cassette to continue.";
                            }
                            else
                            {
                                retval = true;
                            }
                        }
                        else
                        {
                            retval = true;
                        }
                    }
                    else
                    {
                        if (wafersNotInCst.Count != 0) 
                        {
                            retval = false;
                            msg += "A wafer with cassette origin exists inside the Loader.";
                        }

                        if (wafersNotInCst_Unknown.Count != 0)
                        {
                            retval = false;
                            msg = msg.TrimEnd(',');
                            msg = msg.TrimEnd('\n');
                            msg += "\nPlease define the status of the wafer.";
                        }
                    }
                }
                else
                {
                    retval = true;
                }


                if (retval == false)
                {
                    SelectedCassette = foupController.GetCassetteType();
                    this.MetroDialogManager().ShowMessageDialog("Failed to set Cassette Type", $"Foup#{foupindex}\n{msg}", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }

                LoggerManager.Debug($"{this.GetType().Name}, Foup PresenceState: {foupController.GetFoupModuleInfo()?.FoupPRESENCEState}, set Cassette Type : {SelectedCassette}");
                comboBox.SelectedValue = SelectedCassette;
                
                // scan cassette 된 후에 cassette type 을 바꿨을 경우, scan 정보 초기화 시키기.
                if (SelectedCassette != foupController.GetCassetteType())
                {
                    var cassette = LoaderMaster.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, SelectedCassetteTypeFoupNum);
                    if(cassette.ScanState == LoaderParameters.CassetteScanStateEnum.READ)
                    {
                        cassette.SetNoReadScanState();
                        LoggerManager.Debug("Already scanstate: read, change the cassette type to initialize the scan information.");
                    }                    
                }

                var temp = this.FoupOpModule().FoupManagerSysParam_IParam as FoupManagerSystemParameter;
                temp.FoupModules[foupindex - 1].CassetteType.Value = SelectedCassette;
                LoaderMaster.Loader.Foups[SelectedCassetteTypeFoupNum - 1].CassetteType = SelectedCassette;
                this.SaveParameter(temp);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<ComboBox> _SelectedCassetteTypeFoupNumCommand;
        public ICommand SelectedCassetteTypeFoupNumCommand
        {
            get
            {
                if (null == _SelectedCassetteTypeFoupNumCommand) _SelectedCassetteTypeFoupNumCommand = new RelayCommand<ComboBox>(SelectedCassetteTypeFoupNumCommandFunc);
                return _SelectedCassetteTypeFoupNumCommand;
            }
        }

        private void SelectedCassetteTypeFoupNumCommandFunc(ComboBox comboBox)
        {            
            try
            {
                int foupindex = -1;
                if (comboBox == null)
                    return;
                var selectedValue = comboBox.SelectedValue ?? comboBox.SelectedItem;

                if (selectedValue == null)
                    return;

                bool isNumeric = int.TryParse(selectedValue.ToString(), out foupindex);

                if (isNumeric == true && foupindex <= 0)
                    return;

                SelectedCassetteTypeEnum = this.FoupOpModule().GetFoupController(foupindex).GetCassetteType();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private void ChangeFoupIndex()
        {
            try
            {
                if (SelectedFoupNum == 0)
                    return;
                if (E84CassetteLockParam != null)
                {
                    if (PortEachLockMode)
                    {
                        var lockparam = E84CassetteLockParam.E84PortEachLockOptions.
                                               ToList<E84PortLockOptionParam>().Find(options => options.FoupNumber == SelectedFoupNum);
                        if (lockparam != null)
                        {
                            SelectedPortLockParam = lockparam;

                            //SelectedPortLockParam.CassetteLockE84AutoMode = lockparam.CassetteLockE84AutoMode;
                            //SelectedPortLockParam.CassetteLockE84ManualMode = lockparam.CassetteLockE84ManualMode;
                        }
                        else
                        {
                            SelectedPortLockParam.CassetteLockE84AutoMode = CassetteLockModeEnum.UNDEFINED;
                            SelectedPortLockParam.CassetteLockE84ManualMode = CassetteLockModeEnum.UNDEFINED;
                            this.MetroDialogManager().ShowMessageDialog(
                                "Warning Message", "Selected index does not exist in E84 Parameter", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        }
                    }
                    if(PortFullLockMode)
                    {
                        //SelectedPortLockParam.CassetteLockE84AutoMode = CassetteFullLockE84AutoMode;
                        //SelectedPortLockParam.CassetteLockE84ManualMode = CassetteFullLockE84ManualMode;
                    }
                 }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ChangePortMode(bool fullmode = false , bool eachmode = false)
        {
            try
            {
                if (SelectedFoupNum == 0)
                    return;
                if (E84CassetteLockParam != null)
                {
                    if (eachmode)
                    {
                        var lockparam = E84CassetteLockParam.E84PortEachLockOptions.
                                               ToList<E84PortLockOptionParam>().Find(options => options.FoupNumber == SelectedFoupNum);
                        if (lockparam != null)
                        {
                            SelectedPortLockParam = lockparam;
                            //SelectedPortLockParam.CassetteLockE84AutoMode = lockparam.CassetteLockE84AutoMode;
                            //SelectedPortLockParam.CassetteLockE84ManualMode = lockparam.CassetteLockE84ManualMode;
                        }
                        else
                        {
                            SelectedPortLockParam.CassetteLockE84AutoMode = CassetteLockModeEnum.UNDEFINED;
                            SelectedPortLockParam.CassetteLockE84ManualMode = CassetteLockModeEnum.UNDEFINED;
                            this.MetroDialogManager().ShowMessageDialog(
                                "Warning Message", "Selected index does not exist in E84 Parameter", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        }
                    }
                    if (fullmode)
                    {
                        //SelectedPortLockParam.CassetteLockE84AutoMode = CassetteFullLockE84AutoMode;
                        //SelectedPortLockParam.CassetteLockE84ManualMode = CassetteFullLockE84ManualMode;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> IMainScreenViewModel Method </remarks>

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //InitOptionValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private void InitOptionValue()
        {
            try
            {
                LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                SelectedFoupNum = 0;

                if (LoaderMaster != null)
                {
                    var tempFoupIndexs = new ObservableCollection<int>();

                    for (int index = 1; index <= SystemModuleCount.ModuleCnt.FoupCount; index++)
                    {
                        tempFoupIndexs.Add(index);
                    }

                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        FoupIndexs = tempFoupIndexs;
                        CassetteTypeFoupIndexs = tempFoupIndexs;
                       
                    }));

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        SelectedCassetteTypeFoupNum = 1;
                        SelectedCassetteTypeEnum = this.FoupOpModule().GetFoupController(SelectedCassetteTypeFoupNum).GetCassetteType();
                        if (CassetteTypeEnums.Count != 0)
                        {
                            CassetteTypeEnums.Clear();
                        }
                        if (CassetteTypeEnums.Count == 0)
                        {
                            foreach (var item in CassetteTypeEnum.GetValues(typeof(CassetteTypeEnum)))
                            {
                                if ((CassetteTypeEnum)item != CassetteTypeEnum.UNDEFINED && LoaderMaster.Loader.GetModulesSupportingCassetteType((CassetteTypeEnum)item) == EventCodeEnum.NONE)
                                {
                                    CassetteTypeEnums.Add((CassetteTypeEnum)item);
                                }
                            }
                        }
                    });




                    IsCassetteAutoLockEnable = LoaderMaster.GetIsCassetteAutoLock();
                    IsCassetteAutoLockLeftOHTEnable = LoaderMaster.GetIsCassetteAutoLockLeftOHT();
                    IsCassetteAutoUnloadAfterLotEnable = LoaderMaster.GetIsCassetteAutoUnloadAfterLot();
                    IsCancelCarrierEventNotRuning = LoaderMaster.GetIsCancelCarrierEventNotRuning();
                    IsCassetteDetectEventAfterRFID = LoaderMaster.GetIsCassetteDetectEventAfterRFID();
                    IsLoaderLotEndBuzzerON = LoaderMaster.GetIsLoaderLotEndBuzzerON();
                    IsAlwaysCloseFoupCover = LoaderMaster.GetIsAlwaysCloseFoupCover();

                    TimeoutOnPresenceAfterOnExistSensorMs = this.E84Module().GetTimeoutOnPresenceAfterOnExistSensor();
                    SelectedE84PresenceType = this.E84Module().GetE84PreseceType();

                    var e84Param = this.E84Module().GetE84CassetteLockParam();

                    if (e84Param != null)
                    {
                        E84CassetteLockParam.AutoSetCassetteLockEnable = e84Param.AutoSetCassetteLockEnable;
                        SelectedPortLockParam = new E84PortLockOptionParam();
                        this.E84CassetteLockParam.E84PortEachLockOptions.Clear();

                        CassetteFullLockE84AutoMode = e84Param.CassetteLockE84AutoMode;
                        CassetteFullLockE84ManualMode = e84Param.CassetteLockE84ManualMode;
                        for (int index = 0; index < e84Param.E84PortEachLockOptions.Count; index++)
                        {
                            this.E84CassetteLockParam.E84PortEachLockOptions.Add(
                                new E84PortLockOptionParam(
                                    e84Param.E84PortEachLockOptions[index].FoupNumber,
                                    e84Param.E84PortEachLockOptions[index].CassetteLockE84ManualMode,
                                    e84Param.E84PortEachLockOptions[index].CassetteLockE84AutoMode));
                        }
                        SelectedFoupNum = 1;

                        if (e84Param.PortFullLockMode == true)
                        {
                            PortFullLockMode = true;
                            PortEachLockMode = false;
                        }
                        else
                        {
                            PortFullLockMode = false;
                            PortEachLockMode = true;
                        }
                    }
                }
                else
                {
                    ;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                InitOptionValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public void DeInitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        #endregion
    }
}
