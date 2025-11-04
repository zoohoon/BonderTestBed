using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoaderPnpManagerModule
{
    using Autofac;
    using JogViewModelModule;
    using LoaderBase.Communication;
    using LoaderBase.FactoryModules.ViewModelModule;
    using LogModule;
    using PnPControl;
    using PnpViewModelModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.State;
    using RelayCommandBase;
    using RemoteServiceProxy;
    using SerializerUtil;
    using SharpDXRender.RenderObjectPack;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using UcDisplayPort;
    using ucDutViewer;

    public class LoaderPnpManager : IPnpManager, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region //..Property
        private Autofac.IContainer _Container => this.GetLoaderContainer();
        public ILoaderCommunicationManager LoaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();


        public ILoaderViewModelManager ViewModelManager => _Container.Resolve<ILoaderViewModelManager>();
        private IPnpSetupScreen _PnpScreen; 
        public IPnpSetupScreen PnpScreen
        {
            get { return _PnpScreen; }
            set
            {
                if (value != _PnpScreen)
                {
                    _PnpScreen = value;
                    RaisePropertyChanged();
                }
            }
        }



        private ObservableCollection<ObservableCollection<CategoryNameItems>> _PnpNodeSteps;
        public ObservableCollection<ObservableCollection<CategoryNameItems>> PnpNodeSteps
        {
            get { return _PnpNodeSteps; }
            set
            {
                if (value != _PnpNodeSteps)
                {
                    _PnpNodeSteps = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<ObservableCollection<ICategoryNodeItem>> _PnpSteps;
        public ObservableCollection<ObservableCollection<ICategoryNodeItem>> PnpSteps
        {
            get { return _PnpSteps; }
            set
            {
                if (value != _PnpSteps)
                {
                    _PnpSteps = value;
                    ParamValidationInitPnpSteps();
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<ICategoryNodeItem> _PnPNodeItem;
        public ObservableCollection<ICategoryNodeItem> PnPNodeItem
        {
            get { return _PnPNodeItem; }
            set
            {
                if (value != _PnPNodeItem)
                {
                    _PnPNodeItem = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICategoryNodeItem PreStep;

        private ICategoryNodeItem _SeletedStep;
        public ICategoryNodeItem SeletedStep
        {
            get { return _SeletedStep; }
            set
            {
                if (value != _SeletedStep)
                {
                    PreStep = SeletedStep;
                    _SeletedStep = value;
#pragma warning disable 4014
                    // 시간이 오래걸리는 작업이라 Await를 걸지 않았다.
                    // 향후, 커맨드 처리로 변경 검토 필요 by brett.
                    SetToSelectedStep(_SeletedStep);
#pragma warning restore 4014
                    RaisePropertyChanged();
                }
            }
        }

        private IPnpSetup _SelectedPnpStep;
        /// <summary>
        /// Stage | Loader Selected Step.
        /// </summary>
        public IPnpSetup SelectedPnpStep
        {
            get { return _SelectedPnpStep; }
            set
            {
                if (value != _SelectedPnpStep)
                {
                    _SelectedPnpStep = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICategoryNodeItem _CurStep;

        public ICategoryNodeItem CurStep
        {
            get { return _CurStep; }
            set
            {
                _CurStep = value;
                SelectedPnpStep = (IPnpSetup)_CurStep;
            }
        }

        private IDisplayPort _DisplayPort;
        /// <summary>
        /// PNP 공용 DisplyPort ( PNP의 Display(vision)화면은 한개이므로 공용으로사용)
        /// </summary>
        public IDisplayPort DisplayPort
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
        private IDutViewControlVM _DutViewControl;
        public IDutViewControlVM DutViewControl

        {
            get { return _DutViewControl; }
            set
            {
                if (value != _DutViewControl)
                {
                    _DutViewControl = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> PnP UI에서 사용 할 Light Jog
        //==> Light Jog
        public ILightJobViewModel PnpLightJog { get; set; }
        //==> Motion Jog
        public IHexagonJogViewModel PnpMotionJog { get; set; }
        #endregion

        public bool Initialized { get; set; }

        #endregion

        public LoaderPnpManager()
        {

        }

        public void SetContainer(Autofac.IContainer container)
        {
            try
            {
                InitJog();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public void InitJog()
        {
            this.StageSupervisor().SetAcceptUpdateDisp(false);

            PnpLightJog = new LoaderLightJogViewModel(
                maxLightValue: 255,
                minLightValue: 0);
            ((LoaderLightJogViewModel)PnpLightJog).SetContainer(_Container);
            PnpMotionJog = new LoaderHexagonJogViewModel();
            ((LoaderHexagonJogViewModel)PnpMotionJog).SetContainer(_Container);
            this.StageSupervisor().SetAcceptUpdateDisp(true);

        }

        #region //..Method

        private void ParamValidationInitPnpSteps()
        {
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                if (PnpSteps != null)
                { 
                    foreach (var module in PnpSteps)
                    {
                        for (int index = 0; index < module.Count; index++)
                        {
                            if (module[index] is PnPControl.CategoryForm)
                            {
                                for (int jndex = 0; jndex < (module[index] as ICategoryNodeItem)?.Categories.Count; jndex++)
                                {
                                    if ((module[index] as ICategoryNodeItem).Categories[jndex] is ICategoryNodeItem)
                                    {
                                        ((module[index] as ICategoryNodeItem).Categories[jndex] as ICategoryNodeItem).ClearSettingData();
                                        ((module[index] as ICategoryNodeItem).Categories[jndex] as ICategoryNodeItem).SetStepSetupState();
                                    }

                                }
                                module[index].ClearSettingData();
                                module[index].SetStepSetupState();

                            }
                            if (module[index] is ICategoryNodeItem)
                            {
                                (module[index] as ICategoryNodeItem).ClearSettingData();
                                (module[index] as ICategoryNodeItem).SetStepSetupState();
                            }
                        }
                    }
                }
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

        }

        private async Task SetToSelectedStep(ICategoryNodeItem value)
        {
            try
            {
                
                //if (SeletedStep == null & value != null & PreStep == null)
                //{
                //    await SetViewModel(value, null).ConfigureAwait(false);
                //}
                if (PreStep != value & value != null)
                {
                    await SetViewModel(value, null);
                }
                SelectedPnpStep = (IPnpSetup)_SeletedStep;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"SetToSelectedStep({value.ToString()}): Error occurred. Err = {err.Message} ");
            }

        }

        public Task<EventCodeEnum> SetDefaultInitViewModel(object step = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                this.StageSupervisor().SetAcceptUpdateDisp(false);

                SeletedStep = null;
                PreStep = null;
                ICategoryNodeItem module = GetFirstStep(step);
                if (module != null)
                {
                    if (SeletedStep == module)
                        return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
                    SeletedStep = module;
                    //await SetToSelectedStep(module);
                    retVal = EventCodeEnum.NONE;
                }
                this.StageSupervisor().SetAcceptUpdateDisp(true);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PNP_EXCEPTION;
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> SetRecoveryInitViewModel(string moduleName, object step = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                this.StageSupervisor().SetAcceptUpdateDisp(false);

                SeletedStep = null;
                PreStep = null;
                ICategoryNodeItem module = null;
                if (PnpSteps != null)
                {

                    foreach (var pnpsteps in PnpSteps)
                    {
                        foreach (var pstep in pnpsteps)
                        {
                            if (pstep.RecoveryHeader != null)
                            {
                                pstep.Header = pstep.RecoveryHeader;
                            }
                        }
                    }
                }
                if(module==null)
                {
                    module = GetFirstStep(step);
                }

                if (module != null)
                {
                    if (SeletedStep != module)
                    {
                        SeletedStep = module;
                        //await SetToSelectedStep(module);
                    }
                    retVal = EventCodeEnum.NONE;
                }
                this.StageSupervisor().SetAcceptUpdateDisp(true);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PNP_EXCEPTION;
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private ICategoryNodeItem GetFirstStep(object step = null)
        {
            ICategoryNodeItem item = null;
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                if (step != null)
                {
                    item = FindStep(step as ICategoryNodeItem, true);
                    return item;
                }
                else
                {
                    if (PnpSteps != null)
                    {

                        foreach (var pnpsteps in PnpSteps)
                        {
                            foreach (var pstep in pnpsteps)
                            {
                                item = FindStep(pstep);
                                return item;
                            }
                        }
                    }
                }
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return item;
        }

        private ICategoryNodeItem FindStep(ICategoryNodeItem item, bool setcategorystep = false)
        {
            ICategoryNodeItem step = null;
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                if (item != null)
                {
                    if (item.Categories != null)
                    {
                        step = item;
                        return step;
                    }
                }
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return step;
        }

        public EventCodeEnum SetPnpStps(ObservableCollection<ObservableCollection<ICategoryNodeItem>> pnpsteps)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                PnpSteps = pnpsteps;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. PnpManager SetSetletedStep() : Error occurred.");
            }
            return retVal;
        }

        public EventCodeEnum GetPnpSteps(object module)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                SeletedStep = null;
                if (module is IPnpSetupScreen)
                {
                    PnpSteps = ((IPnpSetupScreen)module).GetPnpSteps();
                    PnpScreen = module as IPnpSetupScreen;
                }
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public async Task<EventCodeEnum> SetSeletedStep(ICategoryNodeItem module)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                SeletedStep = module;
                await SetToSelectedStep(SeletedStep);
                //SetViewModel(SeletedStep).Wait();
                //await SetViewModel(SeletedStep);

                this.StageSupervisor().SetAcceptUpdateDisp(true);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PNP_EXCEPTION;
                LoggerManager.Debug($"{err.ToString()}. PnpManager SetSetletedStep() : Error occurred.");
            }
            return retVal;
        }

        public async Task<EventCodeEnum> SetViewModel(object value, object test = null)
        {
            EventCodeEnum retval = EventCodeEnum.NONE;

            if (value == null)
            {
                return EventCodeEnum.NONE;
            }
            else
            {
                try
                {
                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                    this.StageSupervisor().SetAcceptUpdateDisp(false);
                    ICategoryNodeItem pnp = value as ICategoryNodeItem;
                    if (pnp != null)
                    {


                        //because comm display port 
                        //Application.Current.Dispatcher.Invoke(() =>
                        //{
                        //    (DisplayPort as UcDisplayPort.DisplayPort).UseUserControlFunc = UserControlFucEnum.DEFAULT;
                        //});

                        //(pnp as IPnpSetup).UseUserControl = UserControlFucEnum.DEFAULT;

                        // var task = Task.Factory.StartNew( async () =>
                        // {
                        //     try
                        //     {

                        //         retval = await pnp.PageSwitched();
                        //     }
                        //     catch (Exception err)
                        //     {
                        //         LoggerManager.Exception(err);
                        //     }
                        //     finally
                        //     {

                        //     }

                        //});
                        // await task.ConfigureAwait(false);

                        //await Application.Current.Dispatcher.Invoke<Task>(() =>
                        //{
                        //    return pnp.PageSwitched();
                        //});
                        await SetCurrentStep(pnp.Header);
                        await pnp.PageSwitched();

                        //await Task.Factory.StartNew(async () =>
                        //{
                        //    retval = await pnp.PageSwitched();
                        //});

                        //System.Threading.Thread.Sleep(100);

                        retval = ViewModelManager.SetDataContext(pnp);
                        this.StageSupervisor().SetAcceptUpdateDisp(true);
                        return retval;
                    }
                    else
                    {
                        retval = ViewModelManager.SetDataContext(value);
                        this.StageSupervisor().SetAcceptUpdateDisp(true);
                        return EventCodeEnum.NOT_PNPMODULE;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    return EventCodeEnum.PNP_EXCEPTION;
                }
                finally
                {
                    await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                }
            }

        }
        public async Task ClosePnpAdavanceSetupWindow()
        {
            try
            {
                if (SelectedPnpStep != null)
                {
                    this.StageSupervisor().SetAcceptUpdateDisp(false);

                    PNPSetupBase step = (PNPSetupBase)SelectedPnpStep;

                    if (step.AdvanceSetupView != null & step.AdvanceSetupViewModel != null)
                    {
                        await this.MetroDialogManager().CloseWindow(step.AdvanceSetupView, step.AdvanceSetupView.GetType().Name);
                        
                        await step.CloseAdvanceSetupView();
                    }

                    this.StageSupervisor().SetAcceptUpdateDisp(true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Guid GetViewGuid(object module, Guid cuiguid)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum GetCuiBtnParam(object module, Guid cuiguid, out Guid viewguid, out List<Guid> stepguids, bool extrastep = false)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            viewguid = Guid.NewGuid();
            stepguids = new List<Guid>();

            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                retval = ((LoaderCommunicationManager).GetProxy<IRemoteMediumProxy>() as RemoteMediumProxy).GetCuiBtnParam
                    (module, cuiguid, out viewguid, out stepguids, extrastep);
                this.StageSupervisor().SetAcceptUpdateDisp(true);


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ObservableCollection<ICategoryNodeItem> GetNotFormPnpStps()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ParamValidationSteps()
        {
            throw new NotImplementedException();
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

        public EventCodeEnum InitModule()
        {
            InitJog();
            DisplayPort = new DisplayPort() { GUID = new Guid("34EA361B-3487-4DBC-BF5C-72040A09F73D") };
            //(this.ViewModelManager() as ILoaderViewModelManager).RegisteDisplayPort(DisplayPort);
            DutViewControl = new DutViewControl();
            return EventCodeEnum.NONE;
        }

        public ObservableCollection<ObservableCollection<CategoryNameItems>> GetCategoryNameList(string modulename, string interfacename, Guid cuiguid, bool extrastep = false)
        {

            this.StageSupervisor().SetAcceptUpdateDisp(false);

            PnpNodeSteps = ((LoaderCommunicationManager).GetProxy<IRemoteMediumProxy>() as RemoteMediumProxy).GetCategoryNameList(modulename, interfacename, cuiguid, extrastep);
            this.StageSupervisor().SetAcceptUpdateDisp(true);

            return PnpNodeSteps;
        }
        public ObservableCollection<ObservableCollection<CategoryNameItems>> GetRecoveryCategoryNameList(string modulename, string interfacename, Guid cuiguid, bool extrastep)
        {
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                List<string> recoveryPnPNodeSteps = new List<string>();

                PnpNodeSteps = ((LoaderCommunicationManager).GetProxy<IRemoteMediumProxy>() as RemoteMediumProxy).GetCategoryNameList(modulename, interfacename, cuiguid, extrastep);
                ObservableCollection<CategoryNameItems> pnpNodeList = new ObservableCollection<CategoryNameItems>();

                foreach (var pnp in PnpNodeSteps)
                {
                    foreach (var category in pnp)
                    {
                        if (category.Categories.Count > 0)
                        {
                            foreach (var node in category.Categories)
                            {
                                pnpNodeList.Add(node);
                            }
                        }
                        else
                        {
                            pnpNodeList.Add(category);
                        }
                    }
                }

                foreach (var node in pnpNodeList)
                {
                    if (node.RecoveryHeader != null && !node.RecoveryHeader.Equals(""))
                    {
                        node.Header = node.RecoveryHeader;
                    }
                }

                if (PnpNodeSteps.Count == 1)
                {
                    PnpNodeSteps[0] = pnpNodeList;
                }
                this.StageSupervisor().SetAcceptUpdateDisp(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return PnpNodeSteps;
        }

        public EventCodeEnum SetNavListToGUIDs(object module, List<Guid> guids)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                if (PnpNodeSteps != null)
                {
                    MakePnpSteps();

                    if (PnpSteps.Count != 0)
                    {
                        PnpScreen = module as IPnpSetupScreen;

                        foreach (var step in PnpSteps[0])
                        {
                            step.SetStepSetupState(step.Header);
                        }

                    }

                }
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void MakePnpSteps()
        {
            ObservableCollection<ObservableCollection<ICategoryNodeItem>> pnpSteps = new ObservableCollection<ObservableCollection<ICategoryNodeItem>>();
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                ObservableCollection<ICategoryNodeItem> steps = new ObservableCollection<ICategoryNodeItem>();
                foreach (var nodestep in PnpNodeSteps[0])
                {
                    var step = CreateNodeItem(nodestep);
                    if (step != null)
                        steps.Add(step);
                }
                PnpSteps = new ObservableCollection<ObservableCollection<ICategoryNodeItem>>();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PnpSteps.Add(steps);
                });
                
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public ICategoryNodeItem CreateNodeItem(CategoryNameItems nameItems)
        {
            ICategoryNodeItem item = null;
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                if (nameItems.IsCategoryForm)
                {
                    item = new LoaderCategoryForm(_Container, nameItems.Header);
                    item.Header = nameItems.Header;
                    item.InitViewModel();
                    foreach (var category in nameItems.Categories)
                    {
                        var step = CreateNodeItem(category);
                        if (step != null)
                        {
                            var vm = new PnpViewModel(_Container, item, step.Header);
                            vm.InitViewModel();
                            item.Categories.Add(vm);
                        }
                    }
                }
                else
                {
                    item = new PnpViewModel(_Container, nameItems.Header);
                    item.InitViewModel();
                }
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return item;
        }

        public void ApplyParams(List<byte[]> parameters)
        {
            ((LoaderCommunicationManager).GetProxy<IRemoteMediumProxy>() as RemoteMediumProxy).ApplyParams(parameters);
        }

        public void CloseAdvanceSetupView()
        {
            ((LoaderCommunicationManager).GetProxy<IRemoteMediumProxy>() as RemoteMediumProxy).CloseAdvanceSetupView();
        }

        public List<byte[]> ParamObjectListConvertToByteList(List<object> parameters)
        {
            List<byte[]> byteparames = null;
            try
            {
                if (parameters != null)
                {
                    byteparames = new List<byte[]>();
                    foreach (var param in parameters)
                    {
                        byteparames.Add(SerializeManager.SerializeToByte(param));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return byteparames;
        }

        public void SetDislayPortTargetRectInfo(double left, double top)
        {

        }

        public async Task SettingRemotePNP(string modulename, string interfacename, Guid cuiguid)
        {
            try
            {
                //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Please wait");

                PnpSteps = null;
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                GetCategoryNameList(modulename, interfacename, cuiguid);
                SetNavListToGUIDs(null, null);
                await SetDefaultInitViewModel();
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        public async Task SettingRemoteRecoveryPNP(string modulename, string interfacename, Guid cuiguid, bool extrastep)
        {
            try
            {
                //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Please wait");

                PnpSteps = null;
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                GetRecoveryCategoryNameList(modulename, interfacename, cuiguid, extrastep);
                //List<Guid> recoverystep = await this.StageSupervisor().GetRecoveryPnPNodeStepsFromModuleState();
                SetNavListToGUIDs(null, null);
                await SetRecoveryInitViewModel(modulename,null); 
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private PNPSetupBase pnp = new PnpDefaultViewModel();
        public async Task PnpCleanup()
        {
            try
            {

                //SeletedStep = pnp;
                await SetToSelectedStep(null); ;
                //await Task.Run(async() =>
                //{
                //    await SetViewModel(SeletedStep);
                //});
                //await SetViewModel(SeletedStep); ;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region //..StepMethod

        public async Task<EventCodeEnum> StepPageSwitching(string moduleheader, object parameter)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);
                //retVal = AsyncHelpers.RunSync(() => ((LoaderCommunicationManager).GetRemoteMediumClient() as RemoteMediumProxy).StepPageSwitching(moduleheader, parameter));
                retVal = await ((LoaderCommunicationManager).GetProxy<IRemoteMediumProxy>() as RemoteMediumProxy).StepPageSwitching(moduleheader, parameter);
                this.StageSupervisor().SetAcceptUpdateDisp(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public async Task<EventCodeEnum> StepCleanup(string moduleheader, object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);
                retVal = await((LoaderCommunicationManager).GetProxy<IRemoteMediumProxy>() as RemoteMediumProxy).StepCleanup(moduleheader, parameter); ;
                this.StageSupervisor().SetAcceptUpdateDisp(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool StepIsParameterChanged(string moduleheader, bool issave)
        {
            bool retVal = false;
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);
                retVal = ((LoaderCommunicationManager).GetProxy<IRemoteMediumProxy>() as RemoteMediumProxy).StepIsParameterChanged(moduleheader, issave);
                this.StageSupervisor().SetAcceptUpdateDisp(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum StepParamValidation(string moduleheader)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);
                retVal = ((LoaderCommunicationManager).GetProxy<IRemoteMediumProxy>() as RemoteMediumProxy).StepParamValidation(moduleheader);
                this.StageSupervisor().SetAcceptUpdateDisp(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public async Task SetCurrentStep(string moduleheader)
        {
            this.StageSupervisor().SetAcceptUpdateDisp(false);
            await ((LoaderCommunicationManager).GetProxy<IRemoteMediumProxy>() as RemoteMediumProxy).SetCurrentStep(moduleheader);
            this.StageSupervisor().SetAcceptUpdateDisp(true);

        }

        public PNPDataDescription GetPNPDataDescriptor()
        {
            return null;
        }

        public void StepPageReload()
        {
            if (SelectedPnpStep is PNPSetupBase step)
            {
                step.PageSwitched();
            }
        }

        #endregion

        #region //..Command 
        private AsyncCommand _WaferAlignSetupCommand;
        public ICommand WaferAlignSetupCommand
        {
            get
            {
                if (null == _WaferAlignSetupCommand) _WaferAlignSetupCommand = new AsyncCommand(WaferAlignSetupCommandFunc);
                return _WaferAlignSetupCommand;
            }
        }
        private async Task WaferAlignSetupCommandFunc()
        {
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                //ViewModelManager.GetViewModel(0);
                GetCategoryNameList("WaferAlign", "IWaferAligner", new Guid("a05a34bf-e63f-41ee-9819-285274faef1a"));
                //if (await LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>()?.CheckPossibleSetup())
                //{
                //await LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>()?.CheckPossibleSetup();
                SetNavListToGUIDs(null, null);

                await SetDefaultInitViewModel(); ;
                //}
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _PadSetupCommand;
        public ICommand PadSetupCommand
        {
            get
            {
                if (null == _PadSetupCommand) _PadSetupCommand = new AsyncCommand(PadSetupCommandFunc);
                return _PadSetupCommand;
            }
        }
        private async Task PadSetupCommandFunc()
        {
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                GetCategoryNameList("WaferAlign", "IWaferAligner", new Guid("21092926-c512-a80f-bfa6-ef25137b51a8"));
                SetNavListToGUIDs(null, null);
                await SetDefaultInitViewModel(); ;
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _PinSetupCommand;
        public ICommand PinSetupCommand
        {
            get
            {
                if (null == _PinSetupCommand) _PinSetupCommand = new AsyncCommand(PinSetupCommandFunc);
                return _PinSetupCommand;
            }
        }
        private async Task PinSetupCommandFunc()
        {
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                GetCategoryNameList("PinAlign", "IPinAligner", new Guid("29cc6805-9418-4fb5-813c-fa1101820b3c"));
                SetNavListToGUIDs(null, null);
                await SetDefaultInitViewModel(); ;
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private AsyncCommand _CleanPadSequenceSetupCommand;
        public ICommand CleanPadSequenceSetupCommand
        {
            get
            {
                if (null == _CleanPadSequenceSetupCommand) _CleanPadSequenceSetupCommand = new AsyncCommand(CleanPadSequenceSetupCommandFunc);
                return _CleanPadSequenceSetupCommand;
            }
        }
        private async Task CleanPadSequenceSetupCommandFunc()
        {
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                GetCategoryNameList("NeedleCleanerModule", "INeedleCleanModule", new Guid("2d37a087-fad5-4c3a-ba72-30759c929a57"));
                SetNavListToGUIDs(null, null);
                await SetDefaultInitViewModel(); ;
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _CleanPadSetupCommand;
        public ICommand CleanPadSetupCommand
        {
            get
            {
                if (null == _CleanPadSetupCommand) _CleanPadSetupCommand = new AsyncCommand(CleanPadSetupCommandFunc);
                return _CleanPadSetupCommand;
            }
        }
        private async Task CleanPadSetupCommandFunc()
        {
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                GetCategoryNameList("NeedleCleanerModule", "INeedleCleanModule", new Guid("8d2cd7a9-caf5-4bb0-89f5-3b6642d26fb6"));
                SetNavListToGUIDs(null, null);
                await SetDefaultInitViewModel(); ;
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _TouchSensorSetupCommand;
        public ICommand TouchSensorSetupCommand
        {
            get
            {
                if (null == _TouchSensorSetupCommand) _TouchSensorSetupCommand = new AsyncCommand(TouchSensorSetupCommandFunc);
                return _TouchSensorSetupCommand;
            }
        }
        private async Task TouchSensorSetupCommandFunc()
        {
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                GetCategoryNameList("NeedleCleanerModule", "INeedleCleanModule", new Guid("26168D6E-408A-8D81-28BF-4A77F4A3599E"));
                SetNavListToGUIDs(null, null);
                await SetDefaultInitViewModel(); ;
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _PMISetupCommand;
        public ICommand PMISetupCommand
        {
            get
            {
                if (null == _PMISetupCommand) _PMISetupCommand = new AsyncCommand(PMISetupCommandFunc);
                return _PMISetupCommand;
            }
        }
        private async Task PMISetupCommandFunc()
        {
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                GetCategoryNameList("PMIModule", "IPMIModule", new Guid("103482a3-ad4c-4e9f-88fe-a4df5877924b"));
                SetNavListToGUIDs(null, null);
                await SetDefaultInitViewModel(); ;
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _MarkSetupCommand;
        public ICommand MarkSetupCommand
        {
            get
            {
                if (null == _MarkSetupCommand) _MarkSetupCommand = new AsyncCommand(MarkSetupCommandFunc);
                return _MarkSetupCommand;
            }
        }
        private async Task MarkSetupCommandFunc()
        {
            try
            {
                this.StageSupervisor().SetAcceptUpdateDisp(false);

                GetCategoryNameList("MarkAlign", "IMarkAligner", new Guid("66ae4fca-caf5-42b9-a4ba-bd22d026e65a"));
                SetNavListToGUIDs(null, null);
                await SetDefaultInitViewModel(); ;
                this.StageSupervisor().SetAcceptUpdateDisp(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetSetupState(string moduleheader = null)
        {
        }
        public void SetMiniViewTarget(object miniView)
        {
        }
        public EnumMoudleSetupState GetSetupState(string moduleheader = null)
        {
            return EnumMoudleSetupState.UNDEFINED;
        }

        public List<RenderContainer> GetRenderContainers()
        {
            return null;
        }

        public bool IsActivePageSwithcing()
        {
            return false;
        }

        public EventCodeEnum SetAdvancedViewModel(IPnpAdvanceSetupViewModel vm)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if(vm != null)
                {
                    if(this.SelectedPnpStep != null)
                    {
                        this.SelectedPnpStep.AdvanceSetupViewModel = vm;
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retval;
        }

        public EventCodeEnum RememberLastLightSetting(ICamera cam)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum RestoreLastLightSetting(ICamera cam)
        {
            throw new NotImplementedException();
        }

        public void SetNextStepsNotCompleteState(string selectedsetp)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
