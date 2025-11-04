using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProberViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using LogModule;
    using RecipeEditorControl.RecipeEditorParamEdit;
    using ProberInterfaces.State;
    using MetroDialogInterfaces;
    using LoaderBase.Communication;
    using Autofac;

    public class WASettingViewModel : IMainScreenViewModel, IParamScrollingViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("F9D0CFDA-0611-822C-6D77-1B5FF69B815A");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #region //..Property

        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();

        public IWaferObject Wafer { get { return this.GetParam_Wafer(); } }



        public bool Initialized { get; set; } = false;

        private bool _IsWaferAlignRecoveryState;
        public bool IsWaferAlignRecoveryState
        {
            get { return _IsWaferAlignRecoveryState; }
            set
            {
                //if (value != _IsWaferAlignRecoveryState)
                //{
                    _IsWaferAlignRecoveryState = value;
                    RaisePropertyChanged();
                //}
            }
        }

        #region ==> RecipeEditorParamEdit
        private RecipeEditorParamEditViewModel _RecipeEditorParamEdit;
        public RecipeEditorParamEditViewModel RecipeEditorParamEdit
        {
            get { return _RecipeEditorParamEdit; }
            set
            {
                if (value != _RecipeEditorParamEdit)
                {
                    _RecipeEditorParamEdit = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #endregion


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
        private int _CategoryID = 10011002;
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
        public EventCodeEnum UpProc()
        {
            RecipeEditorParamEdit.PrevPageCommandFunc();
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DownProc()
        {
            RecipeEditorParamEdit.NextPageCommandFunc();
            return EventCodeEnum.NONE;
        }
        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Wafer.AlignState.Value = AlignStateEnum.DONE;
                if (_LoaderCommunicationManager != null)
                {
                    var selectedstage = _LoaderCommunicationManager?.SelectedStage;
                    if (selectedstage.StageInfo.LotData.WaferAlignState == "FAIL")
                    {
                        IsWaferAlignRecoveryState = true;
                    }
                    else
                    {
                        IsWaferAlignRecoveryState = false;
                    }
                    LoggerManager.Debug($"[{this.GetType().Name}] PageSwitched() : Stage No. {selectedstage.Index}, Wafer Align State {selectedstage.StageInfo.LotData.WaferAlignState}");
                }

                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                RecipeEditorParamEdit.HardCategoryFiltering(_CategoryID);
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }


        #region //..Command 
        private AsyncCommand<CUI.Button> _WaferAlignSetupCommand;
        public ICommand WaferAlignSetupCommand
        {
            get
            {
                if (null == _WaferAlignSetupCommand) _WaferAlignSetupCommand = new AsyncCommand<CUI.Button>(WaferAlignSetup);
                return _WaferAlignSetupCommand;
            }
        }
        private async Task WaferAlignSetup(CUI.Button cuiparam)
        {
            try
            {
                //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                //EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog(
                //           "Warning Message", "Setup to initialize all existing Wafer Alignment setup and pad data. Do you want to continue?", EnumMessageStyle.AffirmativeAndNegative);
                //if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                //{
                //    if (Extensions_IParam.ProberRunMode != RunMode.EMUL & Wafer.WaferStatus != EnumSubsStatus.EXIST)
                //    {
                //        await this.MetroDialogManager().ShowMessageDialog(
                //              Properties.Resources.ErrorMessageTitle, Properties.Resources.NotExistWaferMessage,
                //              EnumMessageStyle.Affirmative);
                //        return;
                //    }

                //    bool isCurTempWithinSetTempRange = this.TempController().IsCurTempWithinSetTempRange();
                //    if (isCurTempWithinSetTempRange)
                //    {
                //        Guid viewguid = new Guid();
                //        List<Guid> pnpsteps = new List<Guid>();
                //        this.WaferAligner().IsNewSetup = true;
                //        this.PnPManager().GetCuiBtnParam(this.WaferAligner(), cuiparam.GUID, out viewguid, out pnpsteps);
                //        if (pnpsteps.Count != 0)
                //        {
                //            this.WaferAligner().ClearState();
                //            this.WaferAligner().SetSetupState();
                //            this.PnPManager().SetNavListToGUIDs(this.WaferAligner(), pnpsteps);
                //            await this.ViewModelManager().ViewTransitionAsync(viewguid);

                //            //this.GetParam_Wafer().WaferAlignSetupChangedToggle.Value = 
                //            //    !this.GetParam_Wafer().WaferAlignSetupChangedToggle.Value;
                //            //this.StageSupervisor().SaveWaferObject();

                //            this.GetParam_Wafer().PadSetupChangedToggle.DoneState = ElementStateEnum.NEEDSETUP;
                //            this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos.Clear();
                //            this.GetParam_Wafer().SetAlignState(AlignStateEnum.IDLE);
                //            this.GetParam_ProbeCard().PinSetupChangedToggle.DoneState = ElementStateEnum.NEEDSETUP;
                //        }
                //    }
                //    else
                //    {
                //        ret = await this.MetroDialogManager().ShowMessageDialog(
                //            "Temp Error", "The current temperature and the set temperature are not the same. Please check the temperature.", EnumMessageStyle.Affirmative);
                //    }
                //}

                if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                {
                    var ret = await this.WaferAligner().CheckPossibleSetup().ConfigureAwait(false);
                    if (ret)
                    {
                        Guid viewguid = new Guid();
                        List<Guid> pnpsteps = new List<Guid>();
                        this.WaferAligner().IsNewSetup = true;
                        this.PnPManager().GetCuiBtnParam(this.WaferAligner(), cuiparam.GUID, out viewguid, out pnpsteps);
                        if (pnpsteps.Count != 0)
                        {
                            this.WaferAligner().ClearState();
                            this.WaferAligner().SetSetupState();
                            this.PnPManager().SetNavListToGUIDs(this.WaferAligner(), pnpsteps);
                            await this.ViewModelManager().ViewTransitionAsync(viewguid);
                        }
                    }
                }
                else if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    var ret = await this.WaferAligner().CheckPossibleSetup().ConfigureAwait(false);
                    if (ret)
                    //if (await this.WaferAligner().CheckPossibleSetup())
                    {
                        //await Task.Run(async() =>
                        //{
                        //    this.WaferAligner().IsNewSetup = true;
                        //    this.WaferAligner().ClearState();
                        //    this.WaferAligner().SetSetupState();
                        //    await this.ViewModelManager().ViewTransitionAsync(new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0"));
                        //    await this.PnPManager().SettingRemotePNP("WaferAlign", "IWaferAligner", new Guid("a05a34bf-e63f-41ee-9819-285274faef1a"));
                        //});
                        this.WaferAligner().IsNewSetup = true;
                        Task task = new Task(() =>
                        {
                            this.WaferAligner().ClearState();
                        });
                        task.Start();
                        await task;

                        this.WaferAligner().SetSetupState();
                        await this.ViewModelManager().ViewTransitionAsync(new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0"));
                        await this.PnPManager().SettingRemotePNP("WaferAlign", "IWaferAligner", new Guid("a05a34bf-e63f-41ee-9819-285274faef1a"));
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //finally
            //{
            //    await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            //}
            //Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
            //this.ViewModelManager().ViewTransitionUsingVM(ViewGUID, this.WaferAligner());

        }

        private AsyncCommand<CUI.Button> _WaferAlignRecoveryCommand;
        public ICommand WaferAlignRecoveryCommand
        {
            get
            {
                if (null == _WaferAlignRecoveryCommand) _WaferAlignRecoveryCommand = new AsyncCommand<CUI.Button>(WaferAlignRecovery);
                return _WaferAlignRecoveryCommand;
            }
        }
        private async Task WaferAlignRecovery(CUI.Button cuiparam)
        {
            try
            {
                if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                {
                    if (await this.WaferAligner().CheckPossibleSetup(true))
                    {
                        Guid viewguid = new Guid();
                        List<Guid> pnpsteps = new List<Guid>();
                        this.PnPManager().GetCuiBtnParam(this.WaferAligner(), cuiparam.GUID, out viewguid, out pnpsteps);
                        if (pnpsteps.Count != 0)
                        {
                            this.WaferAligner().IsNewSetup = false;
                            this.WaferAligner().SetSetupState();
                            await this.ViewModelManager().ViewTransitionAsync(viewguid);
                            await this.PnPManager().SettingRemoteRecoveryPNP("WaferAlign", "IWaferAligner", new Guid("D0A33FFE-DD22-4572-5B69-73F66C38CEB4"), false);
                        }
                    }
                }
                else if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    if (await this.WaferAligner().CheckPossibleSetup(true))
                    {
                        await Task.Run(async () =>
                        {
                            this.WaferAligner().IsNewSetup = false;
                            this.WaferAligner().SetSetupState();
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0"));
                            await this.PnPManager().SettingRemoteRecoveryPNP("WaferAlign", "IWaferAligner", new Guid("D0A33FFE-DD22-4572-5B69-73F66C38CEB4"), false);
                        });

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<CUI.Button> _WaferAlignSubStepSetupCommand;
        public ICommand WaferAlignSubStepSetupCommand
        {
            get
            {
                if (null == _WaferAlignSubStepSetupCommand) _WaferAlignSubStepSetupCommand = new AsyncCommand<CUI.Button>(WaferAlignSubStepSetupCommandFunc);
                return _WaferAlignSubStepSetupCommand;
            }
        }
        private async Task WaferAlignSubStepSetupCommandFunc(CUI.Button cuiparam)
        {
            if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
            {
                Guid viewguid = new Guid();
                List<Guid> pnpsteps = new List<Guid>();
                this.WaferAligner().IsNewSetup = true;
                this.PnPManager().GetCuiBtnParam(this.WaferAligner(), cuiparam.GUID, out viewguid, out pnpsteps);
                if (pnpsteps.Count != 0)
                {
                    this.WaferAligner().ClearState();
                    this.WaferAligner().SetSetupState();
                    this.PnPManager().SetNavListToGUIDs(this.WaferAligner(), pnpsteps);
                    await this.ViewModelManager().ViewTransitionAsync(viewguid);
                }
            }
            else if(System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
            {
                this.WaferAligner().IsNewSetup = true;
                await this.ViewModelManager().ViewTransitionAsync(new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0"));
                await this.PnPManager().SettingRemotePNP("WaferAlign", "IWaferAligner", cuiparam.GUID);
            }
        }

        private AsyncCommand<CUI.Button> _WaferAlignSetupModifyCommand;
        public ICommand WaferAlignSetupModifyCommand
        {
            get
            {
                if (null == _WaferAlignSetupModifyCommand) _WaferAlignSetupModifyCommand = new AsyncCommand<CUI.Button>(WaferAlignSetupModify);
                return _WaferAlignSetupModifyCommand;
            }
        }

        private async Task WaferAlignSetupModify(CUI.Button cuiparam)
        {
            try
            {

                //if (Extensions_IParam.ProberRunMode != RunMode.EMUL & Wafer.WaferStatus != EnumSubsStatus.EXIST)
                //{
                //    await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.ErrorMessageTitle, Properties.Resources.NotExistWaferMessage, EnumMessageStyle.Affirmative);

                //    return;
                //}

                if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe") 
                {
                    Guid viewguid = new Guid();
                    List<Guid> pnpsteps = new List<Guid>();
                    this.WaferAligner().IsNewSetup = false;
                    this.WaferAligner().SetSetupState();
                    this.PnPManager().GetCuiBtnParam(this.WaferAligner(), cuiparam.GUID, out viewguid, out pnpsteps);
                    if (pnpsteps.Count != 0)
                    {
                        //this.WaferAligner().InitIdelState();
                        this.WaferAligner().ClearState();
                        this.WaferAligner().SetSetupState();
                        this.PnPManager().SetNavListToGUIDs(this.WaferAligner(), pnpsteps);
                        if (this.PnPManager().ParamValidationSteps() != EventCodeEnum.NONE | this.GetParam_Wafer().WaferAlignSetupChangedToggle.DoneState == ElementStateEnum.NEEDSETUP)
                        {
                            await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.ErrorMessageTitle, Properties.Resources.WaferAlignSetupErrorMessage, EnumMessageStyle.Affirmative);

                            return;
                        }
                        else
                        {
                            bool isCurTempWithinSetTempRange = this.TempController().IsCurTempWithinSetTempRange();
                            if (isCurTempWithinSetTempRange)
                            {
                                this.WaferAligner().SetSetupState();
                                await this.ViewModelManager().ViewTransitionAsync(viewguid);
                            }
                            else
                            {
                                EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.TempErrorTitle, Properties.Resources.TempErrorMessage, EnumMessageStyle.Affirmative);
                            }
                        }
                    }

                }
                else if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    this.WaferAligner().IsNewSetup = false;
                    this.WaferAligner().SetSetupState();
                    this.WaferAligner().SetIsModifySetup(true);
                    await this.ViewModelManager().ViewTransitionAsync(new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0"));
                    await this.PnPManager().SettingRemotePNP("WaferAlign", "IWaferAligner", cuiparam.GUID);
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        #endregion

    }

}
