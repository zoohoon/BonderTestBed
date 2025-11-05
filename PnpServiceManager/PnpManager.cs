using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnpServiceManager
{
    using Autofac;
    using PnPControl;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.PnpSetup;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using LogModule;
    using ProberInterfaces.LightJog;
    using HexagonJogControl;
    using System.Windows;
    using System.Runtime.CompilerServices;
    using ProberInterfaces.State;
    using SharpDXRender.RenderObjectPack;
    using UcDisplayPort;
    using ucDutViewer;

    public class PnpManager : IPnpManager
    {

        public bool Initialized { get; set; } = false;

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        //private ObservableCollection<ICategoryNodeItem> _PnpSteps;
        //public ObservableCollection<ICategoryNodeItem> PnpSteps
        //{
        //    get { return _PnpSteps; }
        //    set
        //    {
        //        if (value != _PnpSteps)
        //        {
        //            _PnpSteps = value;
        //            NotifyPropertyChanged("PnpSteps");
        //        }
        //    }
        //}
        #region ==> PnP UI에서 사용 할 Light Jog
        //==> Light Jog
        public ILightJobViewModel PnpLightJog { get; set; }
        //==> Motion Jog
        public IHexagonJogViewModel PnpMotionJog { get; set; }
        #endregion

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
        /// <summary>
        /// Loader Remote Step
        /// </summary>

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

        private List<LightSet> _LastlightSet = new List<LightSet>();
        public List<LightSet> LastlightSet
        {
            get { return _LastlightSet; }
            set
            {
                if (value != _LastlightSet)
                {
                    _LastlightSet = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsActivePageSwithcing { get; set; }

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

        private void ParamValidationInitPnpSteps()
        {
            try
            {
                if (PnpSteps != null)
                {
                    foreach (var module in PnpSteps)
                    {
                        for (int index = 0; index < module.Count; index++)
                        {
                            if (module[index] is CategoryForm)
                            {
                                for (int jndex = 0; jndex < (module[index] as ICategoryNodeItem)?.Categories.Count; jndex++)
                                {
                                    if ((module[index] as ICategoryNodeItem).Categories[jndex] is ICategoryNodeItem)
                                    {
                                        ((module[index] as ICategoryNodeItem).Categories[jndex] as ICategoryNodeItem).ClearSettingData();
                                        //(module[index] as ICategoryNodeItem).Categories[jndex].ParamValidation();                         
                                        //(module[index] as ICategoryNodeItem).Categories[jndex].IsParameterChanged();
                                        ((module[index] as ICategoryNodeItem).Categories[jndex] as ICategoryNodeItem).SetStepSetupState(((module[index] as ICategoryNodeItem).Categories[jndex] as ICategoryNodeItem).Header);
                                    }

                                }
                                module[index].ClearSettingData();
                                //module[index].IsParameterChanged();
                                module[index].SetStepSetupState();

                            }
                            if (module[index] is ICategoryNodeItem)
                            {
                                (module[index] as ICategoryNodeItem).ClearSettingData();
                                //(module[index] as ICategoryNodeItem).IsParameterChanged();
                                (module[index] as ICategoryNodeItem).SetStepSetupState((module[index] as ICategoryNodeItem).Header);
                            }
                        }
                    }
                }
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
                //    await SetViewModel(value, null);
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
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    PnpLightJog = new LightJogViewModel(
                        maxLightValue: 255,
                        minLightValue: 0);

                    PnpMotionJog = new HexagonJogViewModel();

                    // LJH
                    // 250908 LJH
                    // DisplayPort = new DisplayPort() { GUID = new Guid("34EA361B-3487-4DBC-BF5C-72040A09F73D") };
                    foreach (var cam in this.VisionManager().GetCameras())
                    {
                        this.VisionManager().SetDisplayChannel(cam, DisplayPort);
                    }
                    DutViewControl = new DutViewControl();
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

                throw;
            }

            return retval;
        }

        public EventCodeEnum GetPnpSteps(object module)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SeletedStep = null;
                if (module is IPnpSetupScreen)
                {
                    PnpSteps = ((IPnpSetupScreen)module).GetPnpSteps();
                    PnpScreen = module as IPnpSetupScreen;
                }



            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "PnpManager - GetPnpSteps() : Error occurred.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        private ICategoryNodeItem GetFirstStep(object step = null)
        {
            ICategoryNodeItem item = null;
            try
            {
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
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "PnpManager - GetFirstStep() : Error occurred.");
                LoggerManager.Exception(err);
            }
            return item;
        }

        private ICategoryNodeItem FindStep(ICategoryNodeItem item, bool setcategorystep = false)
        {
            ICategoryNodeItem step = null;
            try
            {
                if (item != null)
                {
                    if (item.Categories != null)
                    {
                        step = item;
                        return step;

                        //if (item.Categories.Count == 0)
                        //{
                        //    step = item;
                        //    return step;
                        //}
                        //else
                        //{
                        //    if (!setcategorystep)
                        //        return null;
                        //    else
                        //    {
                        //        foreach (var module in item.Categories)
                        //        {
                        //            if (module.Categories != null)
                        //            {
                        //                if (module.Categories.Count == 0)
                        //                {
                        //                    step = module;
                        //                    return step;
                        //                }
                        //                else
                        //                {
                        //                    step = FindStep(module);
                        //                    if (step != null)
                        //                        return step;
                        //                }
                        //            }
                        //        }
                        //    }

                        //}
                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "PnpManager - FindStep() : Error occurred.");
                LoggerManager.Exception(err);
            }
            return step;
        }

        public async Task<EventCodeEnum> SetViewModel(object value, object test = null)
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            try
            {
                if (value == null)
                    return EventCodeEnum.NONE;

                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Please wait");
                ICategoryNodeItem pnp = value as ICategoryNodeItem;
                if (pnp != null)
                {
                    //because comm display port 
                    //Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //    (DisplayPort as UcDisplayPort.DisplayPort).UseUserControlFunc = UserControlFucEnum.DEFAULT;
                    //});

                    //await Task.Factory.StartNew(async () =>
                    //{
                    //    retval = await pnp.PageSwitched();
                    //});
                    retval = await pnp.PageSwitched();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = pnp;
                    });

                    //Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //    (pnp as IPnpSetup).BindingPNPSetup();
                    //});
                    retval = this.ViewModelManager().SetDataContext(pnp);
                    //if (SeletedStep != pnp)
                    //    SeletedStep = pnp as ICategoryNodeItem;
                    return retval;
                }
                else
                {
                    return EventCodeEnum.NOT_PNPMODULE;
                }

                // REMOVE
                //if (retval == EventCodeEnum.NONE)
                //{
                //    //SeletedStep = pnp as ICategoryNodeItem;
                //    await SetToSelectedStep(pnp as ICategoryNodeItem);
                //}
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "PnpManager - SetViewModel() : Error occurred.");
                LoggerManager.Exception(err);

                return EventCodeEnum.PNP_EXCEPTION;
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        public Task<EventCodeEnum> SetDefaultInitViewModel(object step = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
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
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "PnpManager - SetDefaultInitViewModel() : Error occurred.");
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public async Task<EventCodeEnum> SetSeletedStep(ICategoryNodeItem module)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SeletedStep = module;
                //SetViewModel(SeletedStep).Wait();
                await SetViewModel(SeletedStep);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. PnpManager SetSetletedStep() : Error occurred.");
            }
            return retVal;
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

        public Guid GetViewGuid(object module, Guid cuiguid)
        {
            Guid viewguid = new Guid();
            try
            {

                if (module is ITemplate)
                {
                    ITemplate templatemodule = module as ITemplate;
                    foreach (var controltemplate in templatemodule.LoadTemplateParam.ControlTemplates)
                    {
                        foreach (var btn in controltemplate.ControlPNPButtons)
                        {
                            if (btn.CuiBtnGUID.ToString() == cuiguid.ToString())
                                viewguid = btn.ViewGuid;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return viewguid;
        }

        public EventCodeEnum GetCuiBtnParam(object module, Guid cuiguid, out Guid viewguid, out List<Guid> stepguids, bool extrastep = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            viewguid = new Guid();
            stepguids = new List<Guid>();

            try
            {
                if (module is ITemplate)
                {
                    ITemplate templatemodule = module as ITemplate;
                    foreach (var controltemplate in templatemodule.LoadTemplateParam.ControlTemplates)
                    {
                        foreach (var btn in controltemplate.ControlPNPButtons) 
                        {
                            if (btn.CuiBtnGUID.ToString() == cuiguid.ToString())
                            {
                                viewguid = btn.ViewGuid;
                                if(cuiguid.ToString() == "d0a33ffe-dd22-4572-5b69-73f66c38ceb4" && extrastep == true) //recovery setup
                                {
                                    stepguids = this.WaferAligner().GetRecoverySteps();

                                    if (stepguids.Count > 0)
                                    {
                                        return retVal;
                                    }
                                }

                                stepguids = btn.StepGUID;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public ITemplateStateModule GetTemplateModule(string modulename, string interfacename)
        {
            ITemplateStateModule module = null;
            Autofac.IContainer container = this.GetContainer();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.IsDynamic)
                {
                    string str = assembly.GetName().Name;
                    if (modulename.Equals(str))
                    {
                        object inst = null;

                        foreach (var atype in assembly.GetTypes())
                        {
                            Type[] ifaces = atype.GetInterfaces();
                            foreach (Type itype in ifaces)
                            {
                                if (itype.Name.Equals(interfacename))
                                {
                                    this.GetContainer().TryResolve(itype, out inst);
                                    if (inst != null)
                                    {
                                        if (inst is ITemplateStateModule)
                                            return (ITemplateStateModule)inst;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return module;
        }

        public ObservableCollection<ObservableCollection<CategoryNameItems>> GetCategoryNameList(string modulename, string interfacename, Guid cuiguid, bool extrastep = false)
        {

            ObservableCollection<ObservableCollection<CategoryNameItems>> items = null;
            try
            {
                ITemplateStateModule module = GetTemplateModule(modulename, interfacename);
                if (module != null)
                {
                    List<Guid> stepguids = new List<Guid>();
                    Guid viewGuid = new Guid();
                    GetCuiBtnParam(module, cuiguid, out viewGuid, out stepguids, extrastep);
                    if (stepguids.Count != 0)
                    {
                        ITemplateStateModule templateModule = module as ITemplateStateModule;
                        var ret = TemplateToPnpConverter.ConverterUserGuidList
                             (templateModule.Template.TemplateModules, stepguids, true);
                        items = ret;

                        //Stage 쪽에서 Step Setting 을 한다.
                        //if (module is IConTemplateModule)
                        //{
                        //    var contemplateModule = module as IConTemplateModule;
                        //    PnpSteps = TemplateToPnpConverter.ConverterUserGUIDList(
                        //        templateModule.Template.TemplateModules, stepguids, true);
                        //}
                        //else if (module is ITemplateStateModule)
                        //{
                        //    var statetemplateModule = module as ITemplateStateModule;
                        //    PnpSteps = TemplateToPnpConverter.ConverterUserGUIDList(
                        //        templateModule.Template.TemplateModules, stepguids, true);

                        //}
                        SetNavListToGUIDs(module, stepguids);

                    }


                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return items;
        }
        public ObservableCollection<ObservableCollection<CategoryNameItems>> GetRecoveryCategoryNameList(string modulename, string interfacename, Guid cuiguid, bool extrastep)
        {
            ObservableCollection<ObservableCollection<CategoryNameItems>> items = null;
            try
            {
                ITemplateStateModule module = GetTemplateModule(modulename, interfacename);
                if (module != null)
                {
                    List<Guid> stepguids = new List<Guid>();
                    Guid viewGuid = new Guid();
                    GetCuiBtnParam(module, cuiguid, out viewGuid, out stepguids, extrastep);
                    if (stepguids.Count != 0)
                    {
                        ITemplateStateModule templateModule = module as ITemplateStateModule;
                        var ret = TemplateToPnpConverter.ConverterUserGuidList
                             (templateModule.Template.TemplateModules, stepguids, true);
                        items = ret;

                        SetRecoveryNavListToGUIDs(module, stepguids);

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
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return items;
        }
        public EventCodeEnum SetRecoveryNavListToGUIDs(object module, List<Guid> guids)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (module is ITemplate)
                {
                    if (module is IConTemplateModule)
                    {
                        IConTemplateModule templateModule = module as IConTemplateModule;
                        PnpSteps = TemplateToPnpConverter.ConverterNotFormUserGUIDList(
                            templateModule.Template.TemplateModules, guids, true);
                    }
                    else if (module is ITemplateStateModule)
                    {
                        ITemplateStateModule templateModule = module as ITemplateStateModule;
                        PnpSteps = TemplateToPnpConverter.ConverterNotFormUserGUIDList(
                            templateModule.Template.TemplateModules, guids, true);
                        PnPNodeItem = new ObservableCollection<ICategoryNodeItem>();
                        foreach (var step in templateModule.Template.TemplateModules)
                        {
                            if (step is ICategoryNodeItem)
                            {
                                if (step is CategoryForm)
                                {
                                    foreach (var category in (step as ICategoryNodeItem).Categories)
                                    {
                                        PnPNodeItem.Add(category as ICategoryNodeItem);
                                    }
                                }
                                else
                                {
                                    PnPNodeItem.Add(step as ICategoryNodeItem);
                                }
                            }
                        }
                    }

                    if (PnpSteps.Count != 0)
                    {
                        PnpScreen = module as IPnpSetupScreen;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum SetNavListToGUIDs(object module, List<Guid> guids)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (module is ITemplate)
                {
                    if (module is IConTemplateModule)
                    {
                        IConTemplateModule templateModule = module as IConTemplateModule;
                        PnpSteps = TemplateToPnpConverter.ConverterUserGUIDList(
                            templateModule.Template.TemplateModules, guids, true);
                    }
                    else if (module is ITemplateStateModule)
                    {
                        ITemplateStateModule templateModule = module as ITemplateStateModule;
                        PnpSteps = TemplateToPnpConverter.ConverterUserGUIDList(
                            templateModule.Template.TemplateModules, guids, true);
                        PnPNodeItem = new ObservableCollection<ICategoryNodeItem>();
                        foreach (var step in templateModule.Template.TemplateModules)
                        {
                            if (step is ICategoryNodeItem)
                            {
                                if(step is CategoryForm)
                                {
                                    foreach (var category in (step as ICategoryNodeItem).Categories)
                                    {
                                        PnPNodeItem.Add(category as ICategoryNodeItem);
                                    }
                                }
                                else
                                {
                                    PnPNodeItem.Add(step as ICategoryNodeItem);
                                }
                            }
                        }
                    }

                    if (PnpSteps.Count != 0)
                    {
                        PnpScreen = module as IPnpSetupScreen;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public ObservableCollection<ICategoryNodeItem> GetNotFormPnpStps()
        {
            ObservableCollection<ICategoryNodeItem> stpes = new ObservableCollection<ICategoryNodeItem>();
            try
            {
                if (PnpSteps != null)
                {
                    foreach (var step in PnpSteps)
                    {
                        for (int index = 0; index < step.Count; index++)
                        {
                            if (step[index] is IPnpCategoryForm)
                            {
                                if (step[index].Categories.Count != 0)
                                {
                                    foreach (var module in step[index].Categories)
                                    {
                                        if (module is ICategoryNodeItem)
                                        {
                                            stpes.Add(module as ICategoryNodeItem);
                                        }
                                    }
                                }
                            }
                            else if (step[index] is ICategoryNodeItem)
                            {
                                stpes.Add(step[index]);
                            }


                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return stpes;
        }

        public EventCodeEnum ParamValidationSteps()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (var step in PnpSteps)
                {
                    for (int index = 0; index < step.Count; index++)
                    {
                        if (step[index] is CategoryForm)
                        {
                            for (int jndex = 0; jndex < (step[index] as ICategoryNodeItem)?.Categories.Count; jndex++)
                            {

                                retVal = (step[index] as ICategoryNodeItem).Categories[jndex].ParamValidation();
                                if (retVal != EventCodeEnum.NONE)
                                    return retVal;
                            }
                            retVal = step[index].ParamValidation();
                            if (retVal != EventCodeEnum.NONE)
                                return retVal;
                        }
                        else
                        {
                            retVal = step[index].ParamValidation();
                            if (retVal != EventCodeEnum.NONE)
                                return retVal;
                        }



                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public async Task ClosePnpAdavanceSetupWindow()
        {
            try
            {
                if (SelectedPnpStep != null)
                {
                    PNPSetupBase step = (PNPSetupBase)SelectedPnpStep;

                    if (step.AdvanceSetupView != null & step.AdvanceSetupViewModel != null)
                    {
                        await step.CloseAdvanceSetupView();
                        await this.MetroDialogManager().CloseWindow(step.AdvanceSetupView, this.GetHashCode().ToString());
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private PNPSetupBase pnp = new PnpDefaultViewModel();
        public async Task PnpCleanup()
        {
            try
            {
                //SeletedStep = pnp;
                await SetToSelectedStep(pnp);
                await SetViewModel(SeletedStep);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #region //..Step

        #region //..PageSwitching
        public async Task<EventCodeEnum> StepPageSwitching(string moduleheader, object parameter)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (PnpSteps != null)
                {
                    if (PnpSteps.Count != 0)
                    {
                        foreach (var step in PnpSteps[0])
                        {
                            var ret = await Sub_StepPageSwitching(moduleheader, parameter, step);
                            if (ret.Item2)
                            {
                                retVal = ret.Item1;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private async Task<Tuple<EventCodeEnum, bool>> Sub_StepPageSwitching(string moduleheader, object parameter, ICategoryNodeItem step)
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                _IsActivePageSwithcing = true;
                if (step != null)
                {
                    if (step.Categories != null)
                    {
                        if (step.Categories.Count != 0)
                        {
                            if (step.Header.Equals(moduleheader) || (step.RecoveryHeader != null && step.RecoveryHeader.Equals(moduleheader)))
                            {
                                //await Task.Run(async () =>
                                //{
                                //    retVal = await step.PageSwitched(parameter);
                                //});
                                retVal = await step.PageSwitched(parameter);
                                return new Tuple<EventCodeEnum, bool>(retVal, true);
                            }
                            foreach (var cstep in step.Categories)
                            {
                                var ret = await Sub_StepPageSwitching(moduleheader, parameter, (ICategoryNodeItem)cstep);
                                if (ret.Item2)
                                    return ret;
                            }
                        }
                        else
                        {
                            if (step.Header.Equals(moduleheader)|| (step.RecoveryHeader!=null&&step.RecoveryHeader.Equals(moduleheader)))
                            {
                                // await Task.Run(async () =>
                                //{
                                //    retVal = await step.PageSwitched(parameter);
                                //});
                                retVal = await step.PageSwitched(parameter);
                                return new Tuple<EventCodeEnum, bool>(retVal, true);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                _IsActivePageSwithcing = false;
            }
            return new Tuple<EventCodeEnum, bool>(EventCodeEnum.UNDEFINED, false);
        }
        #endregion

        #region //..Cleanup
        public async Task<EventCodeEnum> StepCleanup(string moduleheader, object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (PnpSteps != null)
                {
                    if (PnpSteps.Count != 0)
                    {
                        foreach (var step in PnpSteps[0])
                        {
                            var ret = await Sub_StepCleanup(moduleheader, parameter, step);

                            if (ret.Item2)
                            {
                                retVal = ret.Item1;
                                break;
                            }
                        }
                    }

                    if (parameter is EventCodeEnum)
                    {
                        if ((EventCodeEnum)parameter == EventCodeEnum.NONE)
                        {
                            ICategoryNodeItem currentstep = null;

                            foreach (var step in PnpSteps[0])
                            {
                                if(step.Header.Equals(moduleheader))
                                {
                                    currentstep = step;
                                    break;
                                }
                            }

                            if(currentstep != null)
                            {
                                var guids = new List<Guid>
                                {
                                    new Guid("5B98472E-6F6D-CDA3-20E1-53EF541856F4"),
                                    new Guid("830A6DE1-956A-54AB-407F-3D3222DFBA41"),
                                    new Guid("91AF5353-D69F-CAE4-DA09-7AE6BCF9B789"),
                                    new Guid("FD761F1E-F14C-FEF6-4EE3-FE3E6ADC5458"),
                                    new Guid("4203F878-B532-8CCC-2613-D5745D4ED5AE"),
                                    new Guid("8DC02412-4BA1-E1F4-E62E-0091081C67A8"),
                                    new Guid("CF1E43F7-AFEA-C748-B274-0307A4AE40E6")
                                };

                                if(currentstep is IMainScreenViewModel)
                                {
                                    var mv = currentstep as IMainScreenViewModel;

                                    if (guids.Contains(mv.ScreenGUID))
                                    {
                                        //TO-DO 적절한 위치로 변경 되어야 함.
                                        this.WaferAligner().ClearRecoverySetupPattern();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private async Task<Tuple<EventCodeEnum, bool>> Sub_StepCleanup(string moduleheader, object parameter, ICategoryNodeItem step)
        {
            try
            {
                if (step != null)
                {
                    if (step.Categories != null)
                    {
                        if (step.Categories.Count != 0)
                        {
                            if (step.Header.Equals(moduleheader) || (step.RecoveryHeader != null && step.RecoveryHeader.Equals(moduleheader)))
                            {
                                if (parameter is string)
                                    parameter = GetCategoryNodeItemModule((string)parameter);
                                var retVal = await step.Cleanup(parameter);
                                return new Tuple<EventCodeEnum, bool>(retVal, true);
                            }
                            foreach (var cstep in step.Categories)
                            {
                                var ret = await Sub_StepCleanup(moduleheader, parameter, (ICategoryNodeItem)cstep);
                                if (ret.Item2)
                                    return ret;
                            }
                        }
                        else
                        {
                            if (step.Header.Equals(moduleheader) || (step.RecoveryHeader != null && step.RecoveryHeader.Equals(moduleheader)))
                            {
                                if (parameter is string)
                                    parameter = GetCategoryNodeItemModule((string)parameter);
                                var retVal = await step.Cleanup(parameter);
                                return new Tuple<EventCodeEnum, bool>(retVal, true);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return new Tuple<EventCodeEnum, bool>(EventCodeEnum.UNDEFINED, false);
        }
        #endregion

        #region //..IsParameterChanged
        public bool StepIsParameterChanged(string moduleheader, bool issave)
        {
            bool retVal = false;
            bool isStep = false;
            try
            {
                if (PnpSteps != null)
                {
                    if (PnpSteps.Count != 0)
                    {
                        foreach (var step in PnpSteps[0])
                        {
                            if ((isStep = Sub_StepIsParameterChanged(moduleheader, issave, step, out retVal)))
                                break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private bool Sub_StepIsParameterChanged(string moduleheader, bool issave, ICategoryNodeItem step, out bool retVal)
        {
            retVal = false;
            try
            {
                if (step != null)
                {
                    if (step.Categories != null)
                    {
                        if (step.Categories.Count != 0)
                        {
                            if (step.Header.Equals(moduleheader) || (step.RecoveryHeader != null && step.RecoveryHeader.Equals(moduleheader)))
                            {
                                retVal = step.IsParameterChanged(issave);
                                return true;
                            }
                            foreach (var cstep in step.Categories)
                            {
                                var ret = Sub_StepIsParameterChanged(moduleheader, issave, (ICategoryNodeItem)cstep, out retVal);
                                if (ret)
                                    return ret;
                            }
                        }
                        else
                        {
                            if (step.Header.Equals(moduleheader) || (step.RecoveryHeader != null && step.RecoveryHeader.Equals(moduleheader)))
                            {
                                retVal = step.IsParameterChanged(issave);
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region //..ParamValidation
        public EventCodeEnum StepParamValidation(string moduleheader)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (PnpSteps != null)
                {
                    if (PnpSteps.Count != 0)
                    {
                        foreach (var step in PnpSteps[0])
                        {
                            var ret = Sub_StepParamValidation(moduleheader, step);
                            if (ret.Item2)
                            {
                                retVal = ret.Item1;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private Tuple<EventCodeEnum, bool> Sub_StepParamValidation(string moduleheader, ICategoryNodeItem step)
        {
            try
            {
                if (step != null)
                {
                    if (step.Categories != null)
                    {
                        if (step.Categories.Count != 0)
                        {
                            if (step.Header.Equals(moduleheader) || (step.RecoveryHeader != null && step.RecoveryHeader.Equals(moduleheader)))
                            {
                                var retVal = step.ParamValidation();
                                return new Tuple<EventCodeEnum, bool>(retVal, true);
                            }
                            foreach (var cstep in step.Categories)
                            {
                                var ret = Sub_StepParamValidation(moduleheader, (ICategoryNodeItem)cstep);
                                if (ret.Item2)
                                    return ret;
                            }
                        }
                        else
                        {
                            if (step.Header.Equals(moduleheader) || (step.RecoveryHeader != null && step.RecoveryHeader.Equals(moduleheader)))
                            {
                                var retVal = step.ParamValidation();
                                return new Tuple<EventCodeEnum, bool>(retVal, true);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return new Tuple<EventCodeEnum, bool>(EventCodeEnum.UNDEFINED, false);
        }
        #endregion

        #region //..Module
        public ICategoryNodeItem GetCategoryNodeItemModule(string modulename)
        {
            ICategoryNodeItem module = null;
            try
            {
                if (PnpSteps != null)
                {
                    if (PnpSteps.Count != 0)
                    {
                        foreach (var step in PnpSteps[0])
                        {
                            var ret = Sub_GetCategoryNodeItemModulestring(step, modulename);
                            if (ret != null)
                                return ret;
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return module;
        }

        public ICategoryNodeItem Sub_GetCategoryNodeItemModulestring(ICategoryNodeItem step, string modulename)
        {
            ICategoryNodeItem module = null;
            try
            {
                if (step != null)
                {
                    if (step.Categories != null)
                    {
                        if (step.Categories.Count != 0)
                        {
                            if (step.Header.Equals(modulename) || (step.RecoveryHeader != null && step.RecoveryHeader.Equals(modulename)))
                            {
                                return step;
                            }
                            foreach (var cstep in step.Categories)
                            {
                                if (Sub_GetCategoryNodeItemModulestring((ICategoryNodeItem)cstep, modulename) != null)
                                    return (ICategoryNodeItem)cstep;
                            }
                        }
                        else
                        {
                            if (step.Header.Equals(modulename)|| (step.RecoveryHeader!=null&& step.RecoveryHeader.Equals(modulename)))
                            {
                                return step;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
            return module;
        }

        public void SetSetupState(string moduleheader = null)
        {
            try
            {
                if (moduleheader != null)
                {
                    var step = GetCategoryNodeItemModule(moduleheader);
                    if (step != null)
                        step.SetStepSetupState(moduleheader);
                }
                else
                {
                    if (SelectedPnpStep != null)
                        (SelectedPnpStep as ICategoryNodeItem).SetStepSetupState();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetMiniViewTarget(object miniView)
        {
            try
            {
                if (SelectedPnpStep != null)
                    SelectedPnpStep.MiniViewTarget = miniView;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EnumMoudleSetupState GetSetupState(string moduleheader = null)
        {
            EnumMoudleSetupState retVal = EnumMoudleSetupState.UNDEFINED;
            try
            {
                if (moduleheader != null)
                {
                    var step = GetCategoryNodeItemModule(moduleheader);
                    if (step != null)
                        retVal = step.StateSetup;
                }
                else
                {
                    if (SelectedPnpStep != null)
                        retVal = (SelectedPnpStep as ICategoryNodeItem).StateSetup;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public async Task SetCurrentStep(string moduleheader)
        {
            //CurStep = GetCategoryNodeItemModule(moduleheader);

            Task task = new Task(() =>
            {
                CurStep = GetCategoryNodeItemModule(moduleheader);
            });
            //task.ConfigureAwait(false);
            task.Start();
            await task;
        }


        #endregion

        public void StepPageReload()
        {
            if (SelectedPnpStep is PNPSetupBase step)
            {
                step.PageSwitched();
            }
        }

        public PNPDataDescription GetPNPDataDescriptor()
        {
            return null;
        }

        public void ApplyParams(List<byte[]> parameters)
        {
            try
            {
                if (parameters != null && parameters.Count > 0)
                {
                    if (SelectedPnpStep is IPackagable)
                        (SelectedPnpStep as IPackagable).PackagableParams = parameters;
                    if (SelectedPnpStep is IPackagable)
                        (SelectedPnpStep as IPackagable).ApplyParams(parameters);
                    
                    this.ParamManager().SetChangedDeviceParam(true);
                    this.ParamManager().SetChangedSystemParam(true);

                    //if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    //{
                    //    if (CurStep is IPackagable)
                    //        (CurStep as IPackagable).PackagableParams = parameters;
                    //    if (CurStep is IPackagable)
                    //        (CurStep as IPackagable).ApplyParams(parameters);
                    //}
                    //else if(SystemManager.SysteMode == SystemModeEnum.Single)
                    //{
                    //    if (SeletedStep is IPackagable)
                    //        (SeletedStep as IPackagable).PackagableParams = parameters;
                    //    if (SeletedStep is IPackagable)
                    //        (SeletedStep as IPackagable).ApplyParams(parameters);
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<byte[]> ParamObjectListConvertToByteList(List<object> parameters)
        {
            return null;
        }

        public List<RenderContainer> GetRenderContainers()
        {
            List<RenderContainer> containers = null;
            if (SelectedPnpStep != null)
            {
                if (SelectedPnpStep.SharpDXLayer != null)
                    return SelectedPnpStep.SharpDXLayer.GetRenderContainers();
            }
            else
                return containers;

            return containers;
        }

        public void SetDislayPortTargetRectInfo(double left, double top)
        {
            try
            {
                if (SelectedPnpStep != null)
                {
                    SelectedPnpStep.TargetRectangleLeft = left;
                    SelectedPnpStep.TargetRectangleTop = top;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsActivePageSwithcing()
        {
            return _IsActivePageSwithcing;
        }

        public Task SettingRemotePNP(string modulename, string interfacename, Guid cuiguid)
        {
            return Task.FromResult(0);
        }
        public Task SettingRemoteRecoveryPNP(string modulename, string interfacename, Guid cuiguid, bool extrastep)
        {
            try
            {
                PnpSteps = null;
                GetRecoveryCategoryNameList(modulename, interfacename, cuiguid, extrastep);
                SetDefaultInitViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult(0);
        }
        public EventCodeEnum SetAdvancedViewModel(IPnpAdvanceSetupViewModel vm)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (vm != null)
                {
                    this.SelectedPnpStep.AdvanceSetupViewModel = vm;
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void CloseAdvanceSetupView()
        {
            try
            {
                (SelectedPnpStep as IHasAdvancedSetup)?.CloseAdvanceSetupView();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum RememberLastLightSetting(ICamera cam)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LastlightSet.Clear();

                if (cam != null)
                {


                    EnumProberCam currentcamtype = cam.GetChannelType();

                    ICamera AnotherCam = null;

                    if (currentcamtype == EnumProberCam.PIN_HIGH_CAM)
                    {
                        AnotherCam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                    }
                    else if (currentcamtype == EnumProberCam.PIN_LOW_CAM)
                    {
                        AnotherCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                    }

                    if (AnotherCam != null)
                    {
                        LastlightSet.Add(new LightSet(AnotherCam.GetChannelType()));

                        foreach (var light in AnotherCam.LightsChannels)
                        {
                            int val = AnotherCam.GetLight(light.Type.Value);

                            LastlightSet[LastlightSet.Count() - 1].LightParams.Add(new LightValueParam(light.Type.Value, (ushort)val));
                        }
                    }

                    LastlightSet.Add(new LightSet(cam.GetChannelType()));

                    foreach (var light in cam.LightsChannels)
                    {
                        int val = cam.GetLight(light.Type.Value);

                        LastlightSet[LastlightSet.Count() - 1].LightParams.Add(new LightValueParam(light.Type.Value, (ushort)val));
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.EXCEPTION;

                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum RestoreLastLightSetting(ICamera cam)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                bool Invalid = false;

                if (LastlightSet != null & LastlightSet.Count > 0)
                {
                    // Validation

                    EnumProberCam InputCamType = cam.GetChannelType();

                    bool IncludeType = false;

                    IncludeType = LastlightSet.Any(c => c.CamType == InputCamType);

                    // 기억해놓은 Light Setting 중, 일치하는 타입이 있는 경우
                    if (IncludeType == true)
                    {
                        // 기억해놓은 카메라별 조명 값을 세팅
                        foreach (var lightset in LastlightSet)
                        {
                            ICamera tmpCam = this.VisionManager().GetCam(lightset.CamType);

                            foreach (var light in lightset.LightParams)
                            {
                                tmpCam.SetLight(light.Type.Value, light.Value.Value);
                            }
                        }
                    }
                    else
                    {
                        Invalid = true;
                    }
                }
                else
                {
                    Invalid = true;
                }

                if (Invalid == true)
                {
                    // TODO : 첫 번째 채널의 값을 128로 설정. (디폴트)
                    cam.SetLight(cam.LightsChannels[0].Type.Value, 128);
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.EXCEPTION;

                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion
    }


    public class LightSet
    {
        public LightSet(EnumProberCam type)
        {
            CamType = type;
        }
        private EnumProberCam _CamType;
        public EnumProberCam CamType
        {
            get { return _CamType; }
            set
            {
                if (value != _CamType)
                {
                    _CamType = value;
                }
            }
        }

        private List<LightValueParam> _LightParams = new List<LightValueParam>();
        public List<LightValueParam> LightParams
        {
            get { return _LightParams; }
            set
            {
                if (value != _LightParams)
                {
                    _LightParams = value;
                }
            }
        }
    }


}
