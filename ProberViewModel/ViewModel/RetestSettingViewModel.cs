using BinFunctionDlg;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Retest;
using RelayCommandBase;
using RetestObject;
using SerializerUtil;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using VirtualKeyboardControl;

namespace RetestSettingViewModel
{
    public class RetestSettingViewModel : IRetestSettingViewModel
    {
        public bool Initialized { get; set; } = false;

        private readonly Guid _ViewModelGUID = new Guid("753bbe30-60ae-4731-989e-a8afe5e12561");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private RetestDeviceParam _RetestDevParam;
        public RetestDeviceParam RetestDevParam
        {
            get { return _RetestDevParam; }
            set
            {
                if (value != _RetestDevParam)
                {
                    _RetestDevParam = value;
                    RaisePropertyChanged();
                }
            }
        }
       
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    RetestDevParam = this.RetestModule().RetestModuleDevParam_IParam as RetestDeviceParam;

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            RetestDevParam = this.RetestModule().RetestModuleDevParam_IParam as RetestDeviceParam;
           
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            this.RetestModule().SaveDevParameter();
            
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        private RelayCommand<object> _BinFunctionEditorCommand;
        public ICommand BinFunctionEditorCommand
        {
            get
            {
                if (null == _BinFunctionEditorCommand) _BinFunctionEditorCommand = new RelayCommand<object>(BinFunctionEditorCommandFunc);

                return _BinFunctionEditorCommand;
            }
        }

        public void BinFunctionEditorCommandFunc(object obj)
        {
            try
            {
                // TODO : BIN INFO SETTING WINDOW

                var window = new BinFunctionDialog();
                window.Show();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<ForcedLotModeEnum> _ForcedLotmodeCommand;
        public ICommand ForcedLotmodeCommand
        {
            get
            {
                if (null == _ForcedLotmodeCommand) _ForcedLotmodeCommand = new RelayCommand<ForcedLotModeEnum>(ForcedLotmodeCommandFunc);

                return _ForcedLotmodeCommand;
            }
        }

        public void ForcedLotmodeCommandFunc(ForcedLotModeEnum obj)
        {
            try
            {
                LoggerManager.Debug($"[RetestSettingViewModel], ForcedLotmodeCommandFunc() called. Mode = {obj}");
                if (obj != null) 
                {
                    this.StageSupervisor().SetLotModeByForcedLotMode();
                    this.RetestModule().SaveDevParameter();
                }   
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //private RelayCommand<object> _MPPEnableCommand;
        //public ICommand MPPEnableCommand
        //{
        //    get
        //    {
        //        if (null == _MPPEnableCommand) _MPPEnableCommand = new RelayCommand<object>(MPPEnableCommandFunc);

        //        return _MPPEnableCommand;
        //    }
        //}

        //public void MPPEnableCommandFunc(object obj)
        //{
        //    try
        //    {
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private RelayCommand<object> _OnlineRetestEnableCommand;
        public ICommand OnlineRetestEnableCommand
        {
            get
            {
                if (null == _OnlineRetestEnableCommand) _OnlineRetestEnableCommand = new RelayCommand<object>(OnlineRetestEnableCommandFunc);

                return _OnlineRetestEnableCommand;
            }
        }

        public void OnlineRetestEnableCommandFunc(object obj)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _CP1InstantRetestEnableCommand;
        public ICommand CP1InstantRetestEnableCommand
        {
            get
            {
                if (null == _CP1InstantRetestEnableCommand) _CP1InstantRetestEnableCommand = new RelayCommand<object>(CP1InstantRetestEnableCommandFunc);

                return _CP1InstantRetestEnableCommand;
            }
        }

        public void CP1InstantRetestEnableCommandFunc(object obj)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ForDamonCommand;
        public ICommand ForDamonCommand
        {
            get
            {
                if (null == _ForDamonCommand) _ForDamonCommand = new RelayCommand<object>(ForDamonFunc);

                return _ForDamonCommand;
            }
        }

        public void ForDamonFunc(object obj)
        {
            try
            {
                LoggerManager.Debug($"Pressed the For Damon Button.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region ==> TextBoxClickCommand
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
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private RelayCommand _ModeChangedCommand;
        public ICommand ModeChangedCommand
        {
            get
            {
                if (null == _ModeChangedCommand) _ModeChangedCommand = new RelayCommand(ModeChangedCommandFunc);
                return _ModeChangedCommand;
            }
        }
        private void ModeChangedCommandFunc()
        {
            try
            {
                this.RetestModule().SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetRetestIParam(byte[] param)
        {
            try
            {
                object target = null;

                var result = SerializeManager.DeserializeFromByte(param, out target, typeof(RetestDeviceParam));

                if (target != null)
                {
                    RetestDevParam = target as RetestDeviceParam;

                    this.RetestModule().RetestModuleDevParam_IParam = RetestDevParam;
                    this.RetestModule().SaveDevParameter();
                }
                else
                {
                    LoggerManager.Error($"SetPolishWaferIParam function is faild.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }
}
