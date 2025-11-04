using Autofac;
using LoaderBase;
using LoaderBase.Communication;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.LoaderController;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LoaderStageSummaryViewModelModule
{


    public class ManualOperationCommand : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Autofac.IContainer _Container => this.GetLoaderContainer();

        public ILoaderCommunicationManager LoaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();

        public ILoaderSupervisor LoaderMaster => _Container.Resolve<ILoaderSupervisor>();

        private ILoaderStageSummaryViewModel _ParentVM;
        public ILoaderStageSummaryViewModel ParentVM
        {
            get { return _ParentVM; }
            set
            {
                if (value != _ParentVM)
                {
                    _ParentVM = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ManualOperationCommand(ILoaderStageSummaryViewModel parent)
        {
            try
            {
                this.ParentVM = parent;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task<EventCodeEnum> CanManualOp()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.ParentVM.SelectedStage == null)
                {
                    await this.MetroDialogManager().ShowMessageDialog(
                                "Error Message",
                                "Please select a stage first.", EnumMessageStyle.Affirmative);
                }
                else
                {
                    GPCellModeEnum Mode = this.ParentVM.SelectedStage.StageMode;

                    if (Mode == GPCellModeEnum.OFFLINE || Mode == GPCellModeEnum.MAINTENANCE)
                    {
                        IRemoteMediumProxy tmpRemoteMedumProxy = this.LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

                        if (tmpRemoteMedumProxy != null)
                        {
                            retval = EventCodeEnum.NONE;
                        }
                        else
                        {
                            // TODO : 메시지 
                            retval = EventCodeEnum.PARAM_ERROR;
                        }
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog(
                                "Error Message",
                                "You can move pages only when stage is in maintanance mode.", EnumMessageStyle.Affirmative);

                        retval = EventCodeEnum.PARAM_ERROR;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private bool CheckLoaderEmergency()
        {
            bool ret = false;
            bool emo = false;
            bool mainair = false;
            bool mainvac = false;

            try
            {
                emo = LoaderMaster.IsLoaderEMOActive();
                mainair = LoaderMaster.IsLoaderMainAirDown();
                mainvac = LoaderMaster.IsLoaderMainVacDown();

                if (emo || mainair || mainvac)
                {
                    ret = true;
                }
                else
                {
                    ret = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        /// <summary>
        /// 현재 soaking 상태임을 확인하는 함수
        /// </summary>
        /// <returns>true : soaking 상태임</returns>
        private bool IsSoakingStatus()
        {
            bool bSoakingStatus = false;
            if (LoaderCommunicationManager.SelectedStage.StageInfo.LotData != null &&
                    (LoaderCommunicationManager.SelectedStage.StageInfo.LotData.SoakingState == ModuleStateEnum.RUNNING.ToString()
                     || LoaderCommunicationManager.SelectedStage.StageInfo.LotData.SoakingState == ModuleStateEnum.SUSPENDED.ToString()))
            {
                bSoakingStatus = true;
            }
            return bSoakingStatus;
        }

        private AsyncCommand _SystemInitExcuteCommand;
        public ICommand SystemInitExcuteCommand
        {
            get
            {
                if (null == _SystemInitExcuteCommand)
                {
                    _SystemInitExcuteCommand = new AsyncCommand(SystemInitExcuteCommandFunc);
                    //_SystemInitExcuteCommand.SetJobTask(LoaderCommunicationManager.WaitStageJob);
                }
                return _SystemInitExcuteCommand;
            }
        }
        private async Task SystemInitExcuteCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsSoakingStatus())
                {
                    await this.MetroDialogManager().ShowMessageDialog(
                                   "Error Message",
                                   "Cannot be switched during Manual Soaking status.", EnumMessageStyle.Affirmative);
                    LoggerManager.SoakingLog($"SystemInitExcuteCommandFunc(): Cannot be switched during Manual Soaking status.");
                }
                else
                {
                    if (CanManualOp().Result == EventCodeEnum.NONE)
                    {
                        if (!CheckLoaderEmergency())
                        {
                            //await Task.Run(() => LoaderCommunicationManager.WaitStageJob());

                            await LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().DoSystemInit();

                            this.LoaderCommunicationManager.GetStage().StageInfo.LotData = LoaderMaster.GetStageLotData(this.LoaderCommunicationManager.GetStage().Index);

                            retval = EventCodeEnum.NONE;
                        }
                        else
                        {
                            retval = EventCodeEnum.PARAM_ERROR;

                            this.MetroDialogManager().ShowMessageDialog("SystemError", "Have to check that EMO button,Main Air,Main Vacuum", EnumMessageStyle.Affirmative);
                        }
                    }
                    else
                    {
                        retval = EventCodeEnum.PARAM_ERROR;
                    }
                }
                    

                if (retval != EventCodeEnum.NONE)
                {
                    LoaderCommunicationManager.SetStageWorkingFlag(false);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                LoaderMaster.InitStageWaferObject(this.LoaderCommunicationManager.GetStage().Index);
            }
        }

        private AsyncCommand _WaferAlignmentExcuteCommand;
        public ICommand WaferAlignmentExcuteCommand
        {
            get
            {
                if (null == _WaferAlignmentExcuteCommand)
                {
                    _WaferAlignmentExcuteCommand = new AsyncCommand(WaferAlignmentExcuteCommandFunc);
                    //_WaferAlignmentExcuteCommand.SetJobTask(LoaderCommunicationManager.WaitStageJob);
                }
                return _WaferAlignmentExcuteCommand;
            }
        }
        private async Task WaferAlignmentExcuteCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsSoakingStatus())
                {
                    await this.MetroDialogManager().ShowMessageDialog(
                                   "Error Message",
                                   "Cannot be switched during Manual Soaking status.", EnumMessageStyle.Affirmative);
                    LoggerManager.SoakingLog($"WaferAlignmentExcuteCommandFunc(): Cannot be switched during Manual Soaking status.");
                }
                else
                {
                    if (CanManualOp().Result == EventCodeEnum.NONE)
                    {
                        if (!CheckLoaderEmergency())
                        {
                            //await Task.Run(() => LoaderCommunicationManager.WaitStageJob());

                            retval = this.WaferAligner().ClearState();

                            if (retval == EventCodeEnum.NONE)
                            {
                                ParentVM.ChangeTabIndex(TabControlEnum.VISION);

                                await LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().DoWaferAlign();

                                this.LoaderCommunicationManager.GetStage().StageInfo.LotData = LoaderMaster.GetStageLotData(this.LoaderCommunicationManager.GetStage().Index);
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            retval = EventCodeEnum.PARAM_ERROR;

                            await this.MetroDialogManager().ShowMessageDialog("SystemError", "Have to check that EMO button,Main Air,Main Vacuum", EnumMessageStyle.Affirmative);
                        }
                    }
                    else
                    {
                        retval = EventCodeEnum.PARAM_ERROR;
                    }
                }
               

                //if (retval != EventCodeEnum.NONE)
                //{
                //    LoaderCommunicationManager.SetStageWorkingFlag(false);
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _PinAlignmentExcuteCommand;
        public ICommand PinAlignmentExcuteCommand
        {
            get
            {
                if (null == _PinAlignmentExcuteCommand)
                {
                    _PinAlignmentExcuteCommand = new AsyncCommand(PinAlignmentExcuteCommandFunc);
                    //_PinAlignmentExcuteCommand.SetJobTask(LoaderCommunicationManager.WaitStageJob);
                }
                return _PinAlignmentExcuteCommand;
            }
        }
        private async Task PinAlignmentExcuteCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsSoakingStatus())
                {
                    await this.MetroDialogManager().ShowMessageDialog(
                                   "Error Message",
                                   "Cannot be switched during Manual Soaking status.", EnumMessageStyle.Affirmative);
                    LoggerManager.SoakingLog($"PinAlignmentExcuteCommandFunc(): Cannot be switched during Manual Soaking status.");
                }
                else
                {
                    if (CanManualOp().Result == EventCodeEnum.NONE)
                    {
                        if (!CheckLoaderEmergency())
                        {
                            var ret = await this.MetroDialogManager().ShowMessageDialog("Pin Align", "Are you sure you want to pin alignment?", EnumMessageStyle.AffirmativeAndNegative);

                            if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                ParentVM.ChangeTabIndex(TabControlEnum.VISION);

                                retval = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().DO_ManualPinAlign();
                                this.LoaderCommunicationManager.GetStage().StageInfo.LotData = LoaderMaster.GetStageLotData(this.LoaderCommunicationManager.GetStage().Index);
                            }

                            retval = EventCodeEnum.NONE;
                        }
                        else
                        {
                            retval = EventCodeEnum.PARAM_ERROR;

                            await this.MetroDialogManager().ShowMessageDialog("SystemError", "Have to check the EMO button, Main Air, Main Vacuum", EnumMessageStyle.Affirmative);
                        }
                    }
                    else
                    {
                        retval = EventCodeEnum.PARAM_ERROR;
                    }

                }

                //if (retval != EventCodeEnum.NONE)
                //{
                //    LoaderCommunicationManager.SetStageWorkingFlag(false);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

}
