using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PolishWaferParameters;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.PolishWafer;
using RelayCommandBase;
using System.Runtime.CompilerServices;
using LogModule;

namespace PolishWaferSettingViewModel
{
    public class PolishWaferSettingVM : IMainScreenViewModel
    {
        readonly Guid _ViewModelGUID = new Guid("C66B8E15-7274-4A14-AD6F-381908019E14");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        
        #region enumProperty
        //public Object Enable =>
        //   Enum.GetValues(typeof(EnumEnable))
        //    .Cast<EnumEnable>()
        //    .Select(p => new { Key = p, Value = p.ToString() })
        //    .ToList();
        //public Object InterValMode =>
        //    Enum.GetValues(typeof(PWIntervalMode))
        //     .Cast<PWIntervalMode>()
        //     .Select(p => new { Key = p, Value = p.ToString() })
        //     .ToList();
        //public Object WaferSize =>
        //    Enum.GetValues(typeof(PWSize))
        //     .Cast<PWSize>()
        //     .Select(p => new { Key = p, Value = p.ToString() })
        //     .ToList();
        //public Object FocusingMode =>
        //   Enum.GetValues(typeof(PWFocusingMode))
        //    .Cast<PWFocusingMode>()
        //    .Select(p => new { Key = p, Value = p.ToString() })
        //    .ToList();
        //public Object OverdriveMode =>
        //   Enum.GetValues(typeof(PWOverdriveMode))
        //    .Cast<PWOverdriveMode>()
        //    .Select(p => new { Key = p, Value = p.ToString() })
        //    .ToList();
        //public Object CleaningMode =>
        //     Enum.GetValues(typeof(PWScrubMode))
        //      .Cast<PWScrubMode>()
        //      .Select(p => new { Key = p, Value = p.ToString() })
        //      .ToList();

        //public Object CleaningDirection =>
        //     Enum.GetValues(typeof(PWCleaningDirection))
        //      .Cast<PWCleaningDirection>()
        //      .Select(p => new { Key = p, Value = p.ToString() })
        //      .ToList();
        public Object IsPinAlignBeforePW =>
            Enum.GetValues(typeof(EnumEnable))
            .Cast<EnumEnable>()
            .Select(p => new { Key = p, Value = p.ToString() })
            .ToList();

        public Object IsPinAlignAfterPW =>
        Enum.GetValues(typeof(EnumEnable))
         .Cast<EnumEnable>()
         .Select(p => new { Key = p, Value = p.ToString() })
         .ToList();
        public Object ContactSeqMode =>
             Enum.GetValues(typeof(PWContactSeqMode))
              .Cast<PWContactSeqMode>()
              .Select(p => new { Key = p, Value = p.ToString() })
              .ToList();
        public Object ShiftMode =>
            Enum.GetValues(typeof(PWPadShiftMode))
             .Cast<PWPadShiftMode>()
             .Select(p => new { Key = p, Value = p.ToString() })
             .ToList();
        public Object IsUserSeq =>
               Enum.GetValues(typeof(EnumEnable))
                .Cast<EnumEnable>()
                .Select(p => new { Key = p, Value = p.ToString() })
                .ToList();
        #endregion

        public bool Initialized { get; set; } = false;

        private IPolishWaferModule _PWModule;
        public IPolishWaferModule PWModule
        {
            get { return _PWModule; }
            set
            {
                if (value != _PWModule)
                {
                    _PWModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Autofac.IContainer Container { get; set; }

        //public EventCodeEnum InitPage(object parameter = null)
        //{
        //    return EventCodeEnum.NONE;
        //}

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    _PWModule = this.PolishWaferModule();

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

        private RelayCommand<object> _cmdSave;
        public ICommand cmdSave
        {
            get
            {
                if (null == _cmdSave) _cmdSave = new RelayCommand<object>(Save);
                return _cmdSave;
            }
        }
        private void Save(object noparam)
        {
            try
            {
                //PolishWaferParams PWParams = PWModule.PolishWaferParams_IParam as PolishWaferParams;
                //if (PWParams == null)
                //{
                //    throw new Exception();
                //}
                //PWParams.SelectedPolishWaferParameter.Copy(PWParams.TempParam);
                //PWModule.SaveParamFunc();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _cmdCleaning;
        public ICommand cmdCleaning
        {
            get
            {
                if (null == _cmdCleaning) _cmdCleaning = new RelayCommand<object>(Cleaning);
                return _cmdCleaning;
            }
        }
        private void Cleaning(object noparam)
        {
            try
            {
                //PWModule.PolishWaferCleaning();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _cmdFocusing;
        public ICommand cmdFocusing
        {
            get
            {
                if (null == _cmdFocusing) _cmdFocusing = new RelayCommand<object>(Focusing);
                return _cmdFocusing;
            }
        }

        private void Focusing(object noparam)
        {
            //PWModule.PolishWaferFocusing();
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
