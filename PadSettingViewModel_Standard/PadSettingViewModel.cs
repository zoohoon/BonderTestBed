using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PadSettingViewModel_Standard
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using LogModule;
    using ProberInterfaces.State;
    using MetroDialogInterfaces;

    public class PadSettingViewModel : IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("8DC261D5-B1C5-C803-CBB6-C47B0F0650D6");
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
            this.WaferAligner().SetIsNewSetup(false);
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        //public EventCodeEnum RollBackParameter()
        //{
        //    return EventCodeEnum.NONE;
        //}

        //public bool HasParameterToSave()
        //{
        //    return true;
        //}


        private AsyncCommand<CUI.Button> _PadSetupCommand;
        public ICommand PadSetupCommand
        {
            get
            {
                if (null == _PadSetupCommand)
                {
                    _PadSetupCommand = new AsyncCommand<CUI.Button>(PadSetup);
                }
                return _PadSetupCommand;
            }
        }

        private async Task PadSetup(CUI.Button cuiparam)
        {
            try
            {
                this.WaferAligner().SetIsNewSetup(true);
                if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                {
                    if (Extensions_IParam.ProberRunMode != RunMode.EMUL & this.GetParam_Wafer().WaferStatus != EnumSubsStatus.EXIST)
                    {
                        await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.ErrorMessageTitle, Properties.Resources.NotExistWaferMessage, EnumMessageStyle.Affirmative);

                        return;
                    }

                    //if (this.WaferAligner().ParamValidation() == EventCodeEnum.NONE & this.GetParam_Wafer().WaferAlignSetupChangedToggle.DoneState != ElementStateEnum.NEEDSETUP)
                    if (this.GetParam_Wafer().WaferAlignSetupChangedToggle.DoneState != ElementStateEnum.NEEDSETUP)
                    {
                        if (this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutList.Count != 0)
                        {
                            bool isCurTempWithinSetTempRange = this.TempController().IsCurTempWithinSetTempRange();
                            if (isCurTempWithinSetTempRange)
                            {
                                Guid viewguid = new Guid();
                                List<Guid> pnpsteps = new List<Guid>();
                                this.PnPManager().GetCuiBtnParam(this.WaferAligner(), cuiparam.GUID, out viewguid, out pnpsteps);
                                if (pnpsteps.Count != 0)
                                {
                                    this.WaferAligner().SetSetupState();
                                    this.PnPManager().SetNavListToGUIDs(this.WaferAligner(), pnpsteps);

                                    await this.ViewModelManager().ViewTransitionAsync(viewguid);

                                }
                            }
                            else
                            {
                                await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.TempErrorTitle, Properties.Resources.TempErrorMessage, EnumMessageStyle.Affirmative);
                            }
                        }
                        else
                        {
                            await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.ErrorMessageTitle, Properties.Resources.NotExistDutErrorMessage, EnumMessageStyle.Affirmative);
                        }
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.ErrorMessageTitle, Properties.Resources.WaferAlignSetupErrorMessage, EnumMessageStyle.Affirmative);
                    }
                }
                else if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    this.WaferAligner().SetSetupState();
                    await this.ViewModelManager().ViewTransitionAsync(new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0"));
                    await this.PnPManager().SettingRemotePNP("WaferAlign", "IWaferAligner", new Guid("21092926-c512-a80f-bfa6-ef25137b51a8"));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

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
    }
}
