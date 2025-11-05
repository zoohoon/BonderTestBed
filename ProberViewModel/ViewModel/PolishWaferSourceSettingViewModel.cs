using CUIServices;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PolishWaferSourceSettingVM
{
    public class PolishWaferSourceSettingViewModel : IMainScreenViewModel
    {
        #region //..IMainScreenViewModel Property
        public bool Initialized { get; set; } = false;
        private readonly Guid _ViewModelGUID = new Guid("67e49c8a-0829-4c42-8564-8ce6e9c0d7ed");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }
        #endregion


        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        private PolisWaferSourceSelectionBase _PolishWaferSourceModel = new PolisWaferSourceSelectionBase();
        public PolisWaferSourceSelectionBase PolishWaferSourceModel
        {
            get { return _PolishWaferSourceModel; }
            set
            {
                if (value != _PolishWaferSourceModel)
                {
                    _PolishWaferSourceModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    //PolishWaferSourceModel.Init();

                    Initialized = true;

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION");

                    retVal = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SelectWaferIndexCommand(Object index)
        {
            try
            {
                //int count1 = GetFixedTraySlotSelectedIndexs().Count;
                //int count2 = GetWaferSlotSelectedIndexs().Count;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [SelectWaferIndexCommand()] : {err}");
            }
        }

        public void DeInitModule()
        {
            return;
        }

       

        public Task<EventCodeEnum> InitViewModel()
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

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            //TempFunc();

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        #region //..Command 
        private AsyncCommand<CUI.Button> _PolishWaferSourceCommand;
        public ICommand PolishWaferSourceCommand
        {
            get
            {
                if (null == _PolishWaferSourceCommand) _PolishWaferSourceCommand = new AsyncCommand<CUI.Button>(PolishWaferSourceCommandFunc);
                return _PolishWaferSourceCommand;
            }
        }

        private async Task PolishWaferSourceCommandFunc(CUI.Button cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                await this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

    }
}
