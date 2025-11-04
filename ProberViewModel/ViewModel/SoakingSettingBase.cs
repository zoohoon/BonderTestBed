using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using SoakingParameters;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SoakingSettingViewModel
{
    public class SoakingSettingBase : IMainScreenViewModel, INotifyPropertyChanged
    {
        readonly Guid _ViewModelGUID = new Guid("2750664f-42df-4d8a-880c-5400993a4049");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public string ViewModelType => throw new NotImplementedException();

        public bool Initialized { get; set; } = false;


        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Autofac.IContainer Container { get; set; }

        private ISoakingModule _SoakingModule;
        public ISoakingModule SoakingModule
        {
            get { return _SoakingModule; }
            set
            {
                if (value != _SoakingModule)
                {
                    _SoakingModule = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IFileManager _FileManager;
        public IFileManager FileManager
        {
            get { return _FileManager; }
            set
            {
                if (value != _FileManager)
                {
                    _FileManager = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SoakingDeviceFile _SoakingDevFile;
        public SoakingDeviceFile SoakingDevFile
        {
            get { return _SoakingDevFile; }
            set
            {
                if (value != _SoakingDevFile)
                {
                    _SoakingDevFile = value;
                    RaisePropertyChanged();
                }
            }
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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    _SoakingModule = this.SoakingModule();
                    _FileManager = this.FileManager();

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        #region Command

        private RelayCommand<object> _NewCommand;
        public ICommand NewCommand
        {
            get
            {
                if (_NewCommand == null) _NewCommand = new RelayCommand<object>(CreateNewCommand);
                return _NewCommand;
            }
        }

        private void CreateNewCommand(object noparam)
        {
            try
            {
                SoakingDevFile = new SoakingDeviceFile();
                SoakingDeviceFile temp = new SoakingDeviceFile();
                temp.SetDefaultParam();
                SoakingDevFile = temp;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _LoadParam;
        public ICommand LoadParam
        {
            get
            {
                if (null == _LoadParam) _LoadParam = new RelayCommand<object>(SoakingLoadparam);
                return _LoadParam;
            }
        }
        private void SoakingLoadparam(object noparam)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            SoakingDevFile = new SoakingDeviceFile();

            object deserializedObj;

            try
            {
                IParam tmpParam = null;
                ret = this.LoadParameter(ref tmpParam, typeof(SoakingDeviceFile));

                if (ret == EventCodeEnum.NONE)
                {
                    SoakingDevFile = tmpParam as SoakingDeviceFile;
                }
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[SoakingSetting] LoadDevParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SaveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (_SaveCommand == null) _SaveCommand = new RelayCommand<object>(SaveFIleCommand);
                return _SaveCommand;
            }
        }

        private void SaveFIleCommand(object noparam)
        {
            try
            {

                SoakingDeviceFile soakingdevfile = SoakingModule.SoakingDeviceFile_IParam as SoakingDeviceFile;
                if (soakingdevfile == null)
                {
                    throw new Exception();
                }
                soakingdevfile.Copy(SoakingDevFile);
                SoakingModule.SaveSoakingDeviceFile();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

    }
}
