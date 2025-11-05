using Autofac;
using BinFunctionDlg;
using LoaderBase.Communication;
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

namespace ProberViewModel.ViewModel
{
    public class LoaderRetestSettingViewModel : IRetestSettingViewModel
    {
        public bool Initialized { get; set; } = false;

        private readonly Guid _ViewModelGUID = new Guid("a756578b-ef18-458b-bb7c-66841b2df2d8");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }
        private Autofac.IContainer _Container => this.GetLoaderContainer();
        public ILoaderCommunicationManager LoaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();

        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

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
                ParamSynchronization();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
                var window = new BinFunctionDialog();
                window.Show();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ForcedLotmodeCommand;
        public ICommand ForcedLotmodeCommand
        {
            get
            {
                if (null == _ForcedLotmodeCommand) _ForcedLotmodeCommand = new RelayCommand<object>(ForcedLotmodeCommandFunc);

                return _ForcedLotmodeCommand;
            }
        }

        public void ForcedLotmodeCommandFunc(object obj)
        {
            try
            {
                LoggerManager.Debug($"[LoaderRetestSettingViewModel], ForcedLotmodeCommandFunc() called. Mode = {obj}");
                if (obj != null) 
                {
                    ParamSynchronization();
                    IStageSupervisorProxy stageSupervisorProxy = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(this.LoaderController().GetChuckIndex());
                    if (stageSupervisorProxy != null)
                    {
                        stageSupervisorProxy.SetLotModeByForcedLotMode();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                await _RemoteMediumProxy.RetestViewModel_PageSwitched();

                RetestDevParam = this.RetestModule().GetRetestIParam() as RetestDeviceParam;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //this.ProbingModule().RetestModule.SaveDevParameter();

                ParamSynchronization();

                //await _RemoteMediumProxy.PolishWaferRecipeSettingVM_Cleanup();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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

        public void SetRetestIParam(byte[] param)
        {
            try
            {
                _RemoteMediumProxy.RetestViewModel_SetRetestIParam(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ParamSynchronization()
        {
            try
            {
                byte[] param = null;

                param = SerializeManager.SerializeToByte(RetestDevParam, typeof(RetestDeviceParam));


                if (param != null)
                {
                    // 셀쪽의 SaveDevParameter() 호출 됨.
                    SetRetestIParam(param);
                }
                else
                {
                    LoggerManager.Error($"ParamSynchronization() Failed");
                }

                RetestDevParam = this.RetestModule().GetRetestIParam() as RetestDeviceParam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }
}
