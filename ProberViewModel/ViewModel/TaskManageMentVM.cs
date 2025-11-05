using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TaskManagementViewModel
{
    public class TaskManageMentVM : IMainScreenViewModel, INotifyPropertyChanged
    {
        readonly Guid _ViewModelGUID = new Guid("B2C604C1-57E6-557E-6D4A-D2A3C7D120E8");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;
        public ISequenceEngineManager SequenceEngineManager
        {
            get;
            set;
        }

        //private IDummyModule _DummyModule;
        //public IDummyModule DummyModule
        //{
        //    get { return _DummyModule; }
        //    set
        //    {
        //        if (value != _DummyModule)
        //        {
        //            _DummyModule = value;
        //            NotifyPropertyChanged("DummyModule");
        //        }
        //    }
        //}

        //private List<TaskAttribute> _TaskList;
        //public List<TaskAttribute> TaskList
        //{
        //    get { return _TaskList; }
        //    set
        //    {
        //        if (value != _TaskList)
        //        {
        //            _TaskList = value;
        //            NotifyPropertyChanged("TaskList");
        //        }
        //    }
        //}
        private float _ZoomLevel;
        public float ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                if (value != _ZoomLevel)
                {
                    _ZoomLevel = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _IsLotModule = true;
        public bool IsLotModule
        {
            get { return _IsLotModule; }
            set
            {
                if (value != _IsLotModule)
                {
                    _IsLotModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsLoaderModule = true;
        public bool IsLoaderModule
        {
            get { return _IsLoaderModule; }
            set
            {
                if (value != _IsLoaderModule)
                {
                    _IsLoaderModule = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsIModule = true;
        public bool IsIModule
        {
            get { return _IsIModule; }
            set
            {
                if (value != _IsIModule)
                {
                    _IsIModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand<object> _cmdZoomLevel1;
        public ICommand cmdZoomLevel1
        {
            get
            {
                if (null == _cmdZoomLevel1) _cmdZoomLevel1 = new RelayCommand<object>(ZoomLevel1Func);
                return _cmdZoomLevel1;
            }
        }
        public void ZoomLevel1Func(object noparam)
        {
            ZoomLevel = 1;
        }
        private RelayCommand<object> _cmdZoomLevel2;
        public ICommand cmdZoomLevel2
        {
            get
            {
                if (null == _cmdZoomLevel2) _cmdZoomLevel2 = new RelayCommand<object>(ZoomLevel2Func);
                return _cmdZoomLevel2;
            }
        }
        public void ZoomLevel2Func(object noparam)
        {
            ZoomLevel = 2;
        }
        private RelayCommand<object> _cmdZoomLevel3;
        public ICommand cmdZoomLevel3
        {
            get
            {
                if (null == _cmdZoomLevel3) _cmdZoomLevel3 = new RelayCommand<object>(ZoomLevel3Func);
                return _cmdZoomLevel3;
            }
        }
        public void ZoomLevel3Func(object noparam)
        {
            ZoomLevel = 3;
        }
        private RelayCommand<object> _cmdZoomLevel4;
        public ICommand cmdZoomLevel4
        {
            get
            {
                if (null == _cmdZoomLevel4) _cmdZoomLevel4 = new RelayCommand<object>(ZoomLevel4Func);
                return _cmdZoomLevel4;
            }
        }
        public void ZoomLevel4Func(object noparam)
        {
            ZoomLevel = 4;
        }
        private RelayCommand<object> _cmdZoomLevel5;
        public ICommand cmdZoomLevel5
        {
            get
            {
                if (null == _cmdZoomLevel5) _cmdZoomLevel5 = new RelayCommand<object>(ZoomLevel5Func);
                return _cmdZoomLevel5;
            }
        }
        public void ZoomLevel5Func(object noparam)
        {
            ZoomLevel = 5;
        }
        private RelayCommand<object> _cmdZoomLevel6;
        public ICommand cmdZoomLevel6
        {
            get
            {
                if (null == _cmdZoomLevel6) _cmdZoomLevel6 = new RelayCommand<object>(ZoomLevel6Func);
                return _cmdZoomLevel6;
            }
        }
        public void ZoomLevel6Func(object noparam)
        {
            ZoomLevel = 6;
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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    SequenceEngineManager = this.SequenceEngineManager();

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
    }
}
