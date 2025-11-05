using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PMISettingViewModel
{
    using CUIServices;
    using LogModule;

    public class PMISettingViewModel : IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("0437cc00-96b7-42a3-806c-2049b3f3710c");
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
        private AsyncCommand<CUI.Button> _PMISetupCommand;
        public ICommand PMISetupCommand
        {
            get
            {
                if (null == _PMISetupCommand) _PMISetupCommand = new AsyncCommand<CUI.Button>(FuncPMISetupCommand);
                return _PMISetupCommand;
            }
        }

        private async Task FuncPMISetupCommand(CUI.Button cuiparam)
        {
            try
            {
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    Guid viewguid = new Guid();
                    List<Guid> pnpsteps = new List<Guid>();
                    this.PnPManager().GetCuiBtnParam(this.PMIModule(), cuiparam.GUID, out viewguid, out pnpsteps);
                    if (pnpsteps.Count != 0)
                    {
                        this.PnPManager().SetNavListToGUIDs(this.PMIModule(), pnpsteps);
                        this.ViewModelManager().ViewTransitionAsync(viewguid);
                    }
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                    {
                        Guid viewguid = new Guid();
                        List<Guid> pnpsteps = new List<Guid>();
                        this.PnPManager().GetCuiBtnParam(this.PMIModule(), cuiparam.GUID, out viewguid, out pnpsteps);
                        if (pnpsteps.Count != 0)
                        {
                            this.PnPManager().SetNavListToGUIDs(this.PMIModule(), pnpsteps);
                            this.ViewModelManager().ViewTransitionAsync(viewguid);
                        }
                    }
                    else
                    {
                        await this.ViewModelManager().ViewTransitionAsync(new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0"));
                        await this.PnPManager().SettingRemotePNP("PMIModule", "IPMIModule", new Guid("103482a3-ad4c-4e9f-88fe-a4df5877924b"));
                    }
                }

                //Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                //this.ViewModelManager().ViewTransitionUsingVM(ViewGUID, this.PMIModule());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<CUI.Button> _PageSwitchCommand;
        public ICommand PageSwitchCommand
        {
            get
            {
                if (null == _PageSwitchCommand) _PageSwitchCommand = new RelayCommand<CUI.Button>(PageSwitchFunc);
                return _PageSwitchCommand;
            }
        }

        private void PageSwitchFunc(CUI.Button cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //this.ViewModelManager().ViewTransitionUsingVM(ViewGUIDnew Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0"), "WaferAlign");

        #endregion
    }
}
