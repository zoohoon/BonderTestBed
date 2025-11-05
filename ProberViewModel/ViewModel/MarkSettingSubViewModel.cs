using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMISettingSubViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using LogModule;

    public class MarkSettingSubViewModel : IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("e35de570-8418-4a75-abfd-6ad13fbbf1ab");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public bool Initialized { get; set; } = false;

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
        //public EventCodeEnum RollBackParameter()
        //{
        //    return EventCodeEnum.NONE;
        //}

        //public bool HasParameterToSave()
        //{
        //    return true;
        //}

        #region //..Command 
        private AsyncCommand<CUI.Button> _MarkAlignSetupCommand;
        public ICommand MarkAlignSetupCommand
        {
            get
            {
                if (null == _MarkAlignSetupCommand) _MarkAlignSetupCommand = new AsyncCommand<CUI.Button>(FuncMarkAlignSetupCommand);
                return _MarkAlignSetupCommand;
            }
        }

        private async Task FuncMarkAlignSetupCommand(CUI.Button cuiparam)
        {
            try
            {

                Guid viewguid = new Guid();
                List<Guid> pnpsteps = new List<Guid>();
                this.PnPManager().GetCuiBtnParam(this.MarkAligner(), cuiparam.GUID, out viewguid, out pnpsteps);
                if (pnpsteps.Count != 0)
                {
                    //this.MarkAligner().SetSetupState();
                    this.PnPManager().SetNavListToGUIDs(this.MarkAligner(), pnpsteps);

                    await this.ViewModelManager().ViewTransitionAsync(viewguid);

                }

                //Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                //this.PnPManager().GetPnpSteps(this.MarkAligner());
                //this.ViewModelManager().ViewTransitionAsync(ViewGUID);

                //this.ViewModelManager().ViewTransition(ViewGUID);
                //this.ViewModelManager().SetDataContext(this.MarkAligner().Template.TemplateModules[0]);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion
    }
}
