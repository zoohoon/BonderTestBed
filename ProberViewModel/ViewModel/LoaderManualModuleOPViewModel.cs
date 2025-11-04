using System;
using System.Threading.Tasks;

namespace LoaderManualModuleOPViewModelModule
{
    using Autofac;
    using LogModule;
    using UcDisplayPort;
    using ProberInterfaces;
    using ProberErrorCode;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using LoaderBase.FactoryModules.ViewModelModule;
    using RelayCommandBase;
    using System.Windows.Input;
    using LoaderBase.Communication;
    using LoaderMapView;
    using LoaderBase;
    using MetroDialogInterfaces;

    public class LoaderManualModuleOPViewModel : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //.. Propety
        public ILoaderViewModelManager LoaderViewModelManager => (ILoaderViewModelManager)this.ViewModelManager();
        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        public ILoaderSupervisor _LoaderSupervisor => this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

        private UcDisplayPort.DisplayPort _DisplayPort;
        public UcDisplayPort.DisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value;
                    RaisePropertyChanged();
                }
            }
        }
        public StageObject SelectedStageObj
        {
            get { return (StageObject)_LoaderCommunicationManager.SelectedStage; }
        }
        #endregion

        #region //..Creator & Init
        public LoaderManualModuleOPViewModel()
        {

        }
        #endregion

        #region //..IMainScreenViewModel 

        readonly Guid _ViewModelGUID = new Guid("812BF3E9-A463-8A06-D27B-9B60F1C8A204");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;
        public void DeInitModule()
        {
            try
            {
                if (Initialized == false)
                {
                    Initialized = true;
                }
            }
            catch (Exception err)
            {

                throw err;
            }
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }


        public EventCodeEnum InitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            DisplayPort = new DisplayPort();
            //DisplayPort.GridVisibility = false;

            //LoaderViewModelManager.RegisteDisplayPort(DisplayPort);

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            this.VisionManager().SetDisplayChannel(null, DisplayPort);
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        #endregion

        #region //..Command & Method
        private bool CheckLoaderEmergency()
        {
            bool ret = false;
            bool emo = false;
            bool mainair = false;
            bool mainvac = false;
            try
            {
                emo = _LoaderSupervisor.IsLoaderEMOActive();
                mainair = _LoaderSupervisor.IsLoaderMainAirDown();
                mainvac = _LoaderSupervisor.IsLoaderMainVacDown();

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

        private AsyncCommand _SystemInitExcuteCommand;
        public ICommand SystemInitExcuteCommand
        {
            get
            {
                if (null == _SystemInitExcuteCommand) _SystemInitExcuteCommand = new AsyncCommand(SystemInitExcuteCommandFunc);
                return _SystemInitExcuteCommand;
            }
        }
        private async Task SystemInitExcuteCommandFunc()
        {
            try
            {
                if (_LoaderCommunicationManager.SelectedStage.StageInfo.LotData.SoakingState == ModuleStateEnum.RUNNING.ToString()
                      || _LoaderCommunicationManager.SelectedStage.StageInfo.LotData.SoakingState == ModuleStateEnum.SUSPENDED.ToString())
                {
                    await this.MetroDialogManager().ShowMessageDialog(
                                   "Error Message",
                                   "Cannot be switched during Manual Soaking status.", EnumMessageStyle.Affirmative);
                    LoggerManager.SoakingLog($"SystemInitExcuteCommandFunc(): Cannot be switched during Manual Soaking status.");
                }
                else
                {
                    await this.MetroDialogManager().ShowWaitCancelDialog("SystemInitExcuteCommandFunc", "System Initializing...");
                    await Task.Run(() => _LoaderCommunicationManager.WaitStageJob());
                    if (!CheckLoaderEmergency())
                    {
                        var stages = this._LoaderCommunicationManager.GetStages();

                        bool groupinitflag = false;
                        string stagenums = "";
                        foreach (var stage in stages)
                        {
                            if (stage.StageInfo.IsChecked)
                            {
                                stagenums = stagenums + $"#{stage.Index} ";
                                groupinitflag = true;
                            }
                        }

                        if (groupinitflag)
                        {
                            var msgRet = await this.MetroDialogManager().ShowMessageDialog("Message", "Stage [" +
                                    stagenums + "]Do you Want System Initialize?", EnumMessageStyle.AffirmativeAndNegative);

                            if (msgRet == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                foreach (var stage in stages)
                                {
                                    if (stage.StageInfo.IsChecked)
                                    {
                                        _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stage.Index)?.DoSystemInit(false);
                                        groupinitflag = true;
                                    }
                                }
                            }
                        }

                        if (!groupinitflag & _LoaderCommunicationManager.SelectedStage != null)
                        {
                            _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>()?.DoSystemInit();
                        }
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog("SystemError",
                            "Have to check that EMO button,Main Air,Main Vacuum", EnumMessageStyle.Affirmative);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                _LoaderCommunicationManager.SetLoaderWorkingFlag(false);
                _LoaderSupervisor.InitStageWaferObject(_LoaderCommunicationManager.SelectedStage.Index);
                await this.MetroDialogManager().CloseWaitCancelDialaog("SystemInitExcuteCommandFunc");
            }
        }

        private AsyncCommand _WaferAlignmentExcuteCommand;
        public ICommand WaferAlignmentExcuteCommand
        {
            get
            {
                if (null == _WaferAlignmentExcuteCommand) _WaferAlignmentExcuteCommand = new AsyncCommand(WaferAlignmentExcuteCommandFunc);
                return _WaferAlignmentExcuteCommand;
            }
        }
        private async Task WaferAlignmentExcuteCommandFunc()
        {
            try
            {
                if (_LoaderCommunicationManager.SelectedStage.StageInfo.LotData.SoakingState == ModuleStateEnum.RUNNING.ToString()
                      || _LoaderCommunicationManager.SelectedStage.StageInfo.LotData.SoakingState == ModuleStateEnum.SUSPENDED.ToString())
                {
                    await this.MetroDialogManager().ShowMessageDialog(
                                   "Error Message",
                                   "Cannot be switched during Manual Soaking status.", EnumMessageStyle.Affirmative);
                    LoggerManager.SoakingLog($"WaferAlignmentExcuteCommandFunc(): Cannot be switched during Manual Soaking status.");
                }
                else
                {
                    await this.MetroDialogManager().ShowWaitCancelDialog("WaferAlignmentExcuteCommandFunc", "System Initializing...");

                    if (!CheckLoaderEmergency())
                    {
                        await _LoaderCommunicationManager.WaitStageJob();
                        this.WaferAligner().ClearState();

                        Task task = new Task(() =>
                        {

                            _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().DoWaferAlign();
                        });
                        task.Start();
                        await task;
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog("SystemError",
                            "Have to check that EMO button,Main Air,Main Vacuum", EnumMessageStyle.Affirmative);
                    }
                }
                   
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                _LoaderCommunicationManager.SetLoaderWorkingFlag(false);
                await this.MetroDialogManager().CloseWaitCancelDialaog("WaferAlignmentExcuteCommandFunc");
            }
        }

        private AsyncCommand _PinAlignmentExcuteCommand;
        public ICommand PinAlignmentExcuteCommand
        {
            get
            {
                if (null == _PinAlignmentExcuteCommand) _PinAlignmentExcuteCommand = new AsyncCommand(PinAlignmentExcuteCommandFunc);
                return _PinAlignmentExcuteCommand;
            }
        }
        private async Task PinAlignmentExcuteCommandFunc()
        {
            try
            {

                if (_LoaderCommunicationManager.SelectedStage.StageInfo.LotData.SoakingState == ModuleStateEnum.RUNNING.ToString() || 
                    _LoaderCommunicationManager.SelectedStage.StageInfo.LotData.SoakingState == ModuleStateEnum.SUSPENDED.ToString())
                {
                    await this.MetroDialogManager().ShowMessageDialog(
                                   "Error Message",
                                   "Cannot be switched during Manual Soaking status.", EnumMessageStyle.Affirmative);
                    LoggerManager.SoakingLog($"PinAlignmentExcuteCommandFunc: Cannot be switched during Manual Soaking status.");
                }
                else
                {
                    await this.MetroDialogManager().ShowWaitCancelDialog("PinAlignmentExcuteCommandFunc", "System Initializing...");

                    await _LoaderCommunicationManager.WaitStageJob();

                    if (!CheckLoaderEmergency())
                    {
                        var ret = await this.MetroDialogManager().ShowMessageDialog("Pin Align", "Are you sure you want to pin alignment?", EnumMessageStyle.AffirmativeAndNegative);

                        if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().DO_ManualPinAlign();
                        }

                        //Task task = new Task(() =>
                        //{
                        //    _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().DoPinAign();
                        //});
                        //task.Start();
                        //await task;

                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog("SystemError",
                            "Have to check that EMO button,Main Air,Main Vacuum", EnumMessageStyle.Affirmative);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                _LoaderCommunicationManager.SetLoaderWorkingFlag(false);
                await this.MetroDialogManager().CloseWaitCancelDialaog("PinAlignmentExcuteCommandFunc");

            }
        }

        //private AsyncCommand _PMIExcuteCommand;
        //public ICommand PMIExcuteCommand
        //{
        //    get
        //    {
        //        if (null == _PMIExcuteCommand) _PMIExcuteCommand = new AsyncCommand(PMIExcuteCommandFunc);
        //        return _PMIExcuteCommand;
        //    }
        //}
        //private async Task PMIExcuteCommandFunc()
        //{
        //    try
        //    {
        //        if (!CheckLoaderEmergency())
        //        {
        //            _LoaderCommunicationManager.GetStageSupervisorClient().DoPMI();
        //        }
        //        else
        //        {
        //            this.MetroDialogManager().ShowMessageDialog("SystemError",
        //                "Have to check that EMO button,Main Air,Main Vacuum", EnumMessageStyle.Affirmative);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //    }
        //}
        #endregion

    }
}
