using System.Threading.Tasks;

namespace PnpControlViewModel
{
    using System;
    using System.ComponentModel;
    using ProberInterfaces;
    using ProberErrorCode;
    using PnPControl;
    using ProberInterfaces.PnpSetupPage;
    using LogModule;

    public class PnpControlVM : PNPSetupBase, INotifyPropertyChanged, IMainScreenViewModel, ISetupPageViewModel
    {

        readonly Guid _ViewModelGUID = new Guid("1C96AA21-1613-108A-71D6-9BCE684A4DD0");
        public override Guid ScreenGUID { get { return _ViewModelGUID; } }


        public bool Initialized { get; set; } = false;
        public PnpControlVM()
        {

        }


        //================


        public async Task<EventCodeEnum> DeInitViewModel(object parameter = null)
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

            return retval;
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

        public override async Task<EventCodeEnum> InitViewModel()
        {
            return EventCodeEnum.NONE;
        }

        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (parameter != null)
                {
                    retVal = this.PnPManager().GetPnpSteps(parameter);
                }
                else
                {
                    //await Task.Run(async () =>
                    //{
                    this.PnPManager().SetPnpStps(null);
                    retVal = await this.PnPManager().SetDefaultInitViewModel();
                    //});

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override void SetStepSetupState(string header = null)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }


        //================

    }
}
