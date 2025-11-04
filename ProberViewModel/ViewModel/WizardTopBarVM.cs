using System;
using System.Threading.Tasks;

namespace WizardTopBarControlViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    //public class WizardTopBarVM : IMainTopBarViewModel
    public class WizardTopBarVM : IMainScreenViewModel
    {
        readonly Guid _ViewModelGUID = new Guid("506D8C65-DDF8-0F8D-E056-3292F0911CD2");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        private Autofac.IContainer Container { get; set; }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;


        public void DeInitModule()
        {
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
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
    }
}
