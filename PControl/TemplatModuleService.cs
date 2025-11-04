using System;
using System.Collections.Generic;
using System.Linq;

namespace PnPControl
{
    using Autofac;
    using System.Reflection;
    using DllImporter;
    using System.Collections.ObjectModel;
    using ProberInterfaces.Wizard;
    using System.Windows;
    using ProberInterfaces;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces.Template;

    public static class TemplatModuleService
    {
        private static Autofac.IContainer Container;
        public static SubStepModules LoadTemplatModule(Autofac.IContainer container, TemplateFileParam templats, bool applyload = true)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            Container = container;

            SubStepModules subStepModules = new SubStepModules();

            try
            {
                foreach (var templat in templats.TemlateModules)
                {
                    lock (templat)
                    {
                        retval = LoadDllInfo(templat, subStepModules, applyload);

                        if (retval != EventCodeEnum.NONE)
                        {
                            // ERROR
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return subStepModules;
        }

        private static List<ITemplateModule> LoadModuleDllInfo(ModuleInfo stepInfo, SubStepModules subStepModules, bool applyload)
        {
            List<ITemplateModule> cmodule = new List<ITemplateModule>();
            try
            {
                DllImporter DLLImporter = new DllImporter();
                ModuleInfo module = stepInfo;

                Tuple<bool, Assembly> ret = DLLImporter.LoadDLL(((ModuleInfo)stepInfo).DllInfo);

                if (ret != null && ret.Item1 == true)
                {
                    var TemplateModules = DLLImporter.Assignable<ITemplateModule>(ret.Item2);

                    for (int index = 0; index < TemplateModules.Count; index++)
                    {
                        object tmodule = TemplateModules[index];

                        if (module.DllInfo.ClassName.ToList<string>().Find(classname => classname.Equals(tmodule.GetType().Name)) != null)
                        {
                            if (tmodule is IHasDevParameterizable)
                                (tmodule as IHasDevParameterizable).LoadDevParameter();

                            if (tmodule is ISchedulingModule)
                            {
                                subStepModules.SchedulingModule = tmodule as ISchedulingModule;
                            }

                            else if (tmodule is IProcessingModule)
                            {
                                module.AlignModule.Add((IProcessingModule)tmodule);
                                //(module.AlignModule[module.AlignModule.Count - 1]).LoadDevParameter();
                                if (applyload)
                                {
                                    cmodule.Add(module.AlignModule[module.AlignModule.Count - 1] as ITemplateModule);

                                    if (tmodule is ICategoryNodeItem)
                                        (tmodule as ICategoryNodeItem).SetEnableState(module.SetupEnableState);

                                    subStepModules.SubModules.Add((IProcessingModule)tmodule);
                                    subStepModules.EntryTemplateModules.Add((ITemplateModule)tmodule);
                                }
                            }

                            else
                            {
                                if (applyload)
                                {
                                    if (tmodule is ITemplateModule)
                                    {
                                        cmodule.Add(tmodule as ITemplateModule);

                                        if (tmodule is ICategoryNodeItem)
                                            (tmodule as ICategoryNodeItem).SetEnableState(module.SetupEnableState);

                                    }
                                    subStepModules.EntryTemplateModules.Add((ITemplateModule)tmodule);
                                }
                            }

                            if (tmodule is IModule)
                            {
                                (tmodule as IModule).InitModule();
                            }
                            if (tmodule is IMainScreenViewModel)
                            {
                                (tmodule as IMainScreenViewModel).InitViewModel();
                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return cmodule;
        }

        //public static IFocusing LoadFocusingModule(ModuleDllInfo info)
        //{
        //    IFocusing retval;

        //    return retval;
        //}
        //public static IFocusing LoadFocusingModule(ModuleDllInfo info, Type moduleType)
        //{
        //    DllImporter DLLImporter = new DllImporter();
        //    IFocusing focusingModule = null;
        //    try
        //    {

        //        Tuple<bool, Assembly> ret = DLLImporter.LoadDLL(info);

        //        if (ret != null && ret.Item1 == true)
        //        {
        //            for (int index = 0; index < DLLImporter.Assignable<IFocusing>(ret.Item2).Count; index++)
        //            {
        //                focusingModule = DLLImporter.Assignable<IFocusing>(ret.Item2)[index];
        //                focusingModule.InitModule(moduleType);
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        //LoggerManager.Debug(err);
        //    }


        //    return focusingModule;
        //}

        private static EventCodeEnum LoadDllInfo(CategoryModuleBase stepInfo, SubStepModules templatemodules, bool applyload)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                DllImporter DLLImporter = new DllImporter();


                if (stepInfo is ModuleInfo)
                {
                    List<ITemplateModule> module = LoadModuleDllInfo((ModuleInfo)stepInfo, templatemodules, applyload);

                    if (module != null)
                    {
                        foreach (var obj in module)
                        {
                            if (obj is ITemplateModule)
                                templatemodules.TemplateModules.Add(obj);
                            if (obj is IWizardPostTemplateModule)
                                templatemodules.PostProcModules.Add((IWizardPostTemplateModule)obj);
                        }

                    }

                    //if (module is IWizardPostTemplateModule)
                    //{
                    //    foreach (var obj in module)
                    //    {
                    //        if (obj is ITemplateModule)
                    //        {
                    //            templatemodules.TemplateModules.Add(obj);
                    //            templatemodules.PostProcModules.Add((IWizardPostTemplateModule)module);
                    //        }
                    //    }

                    //}

                }
                else if (stepInfo is CategoryInfo)
                {
                    templatemodules.TemplateModules.Add(LoadNodeDllInfo((CategoryInfo)stepInfo, templatemodules, applyload));
                }

                retval = EventCodeEnum.NONE;

                return retval;

            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;
                System.Diagnostics.Debug.Assert(false);

                //LoggerManager.Error($err + "LoadDllInfo() : Error occured.");
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private static CategoryForm LoadNodeDllInfo(CategoryInfo stepInfo, SubStepModules templatemodules, bool applyload)
        {
            CategoryForm form = new CategoryForm(((CategoryInfo)stepInfo).Header);

            try
            {
                foreach (var step in stepInfo.Categories)
                {
                    if (step is ModuleInfo)
                    {
                        List<ITemplateModule> module = LoadModuleDllInfo((ModuleInfo)step, templatemodules, applyload);
                        if (module != null)
                        {
                            foreach (var obj in module)
                            {
                                form.Categories.Add(obj);
                                if (obj is IWizardPostTemplateModule)
                                {
                                    templatemodules.SubModules.Add(obj as ISubModule);
                                    templatemodules.PostProcModules.Add((IWizardPostTemplateModule)obj);

                                }
                            }
                        }
                    }
                }
                form.InitViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return form;
        }

        //=====================================================================================================================


        public static ITemplateCollection LoadTemplateModule(Autofac.IContainer container, object module, ITemplateParam param, bool applyload = true)
        {
            IViewModelManager viewModelManager = container.Resolve<IViewModelManager>();
            ITemplateCollection template = null;
            try
            {
                lock (module)
                {
                    if (module is ITemplate)
                    {
                        if (module is IConTemplateModule)
                        {
                            TemplateCollection collection = new TemplateCollection();

                            collection.TemplateModules = LoadDefaultDll((module as ITemplate), param, viewModelManager, applyload);

                            template = collection;
                        }
                        else if (module is ITemplateStateModule)
                        {
                            TemplateStateCollection collection = new TemplateStateCollection();

                            collection.TemplateModules = LoadDefaultDll((module as ITemplate), param, viewModelManager, applyload);

                            foreach (var tmodule in collection.TemplateModules)
                            {
                                if (tmodule is ISchedulingModule)
                                {
                                    collection.SchedulingModule = tmodule as ISchedulingModule;
                                    break;
                                }

                            }

                            template = collection;
                        }

                    }
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return template;
        }

        private static TemplateModuleObservableCollection LoadDefaultDll(ITemplate Module, ITemplateParam param, IViewModelManager viewModelManager, bool applyload = true)
        {
            TemplateModuleObservableCollection templatemodule = new TemplateModuleObservableCollection();
            try
            {
                TemplateModuleObservableCollection cs = new TemplateModuleObservableCollection();


                if (param.SubRoutineModule != null)
                {
                    LoadTemplateDll(Module, param.SubRoutineModule, templatemodule, cs, applyload);
                }
                for (int index = 0; index < param.TemlateModules.Count; index++)
                {
                    LoadTemplateDll(Module, param.TemlateModules[index], templatemodule, cs, applyload);
                }

                for (int index = 0; index < param.ControlTemplates.Count; index++)
                {
                    LoadControlDll(param.ControlTemplates[index], viewModelManager, applyload);

                }

                if (applyload)
                {
                    if (Module is ICategoryNodeItem)
                    {
                        (Module as ICategoryNodeItem).Categories = cs;
                    }
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            return templatemodule;
        }

        private static EventCodeEnum LoadControlDll(ControlTemplateParameter controltemplate, IViewModelManager viewModelManager, bool applyload)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = viewModelManager.RegisteViewInstance(controltemplate.ViewControlModuleInfo);
                retVal = viewModelManager.RegisteViewModelInstance(controltemplate.ViewModelControlModuleInfo);

                if (controltemplate.ViewControlModuleInfo != null && controltemplate.ViewModelControlModuleInfo != null)
                    retVal = viewModelManager.ConnectControlInstances(controltemplate.ViewControlModuleInfo.DllGuid, controltemplate.ViewModelControlModuleInfo.DllGuid);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
            return retVal;
        }

        private static EventCodeEnum LoadDll(ITemplate Module, ModuleDllInfo info, bool applyload)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        private static EventCodeEnum LoadTemplateDll(ITemplate Module, CategoryModuleBase stepInfo, ObservableCollection<ITemplateModule> outtemplate, ObservableCollection<ITemplateModule> categories, bool applyload)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                DllImporter DLLImporter = new DllImporter();


                if (stepInfo is ModuleInfo)
                {
                    ITemplateModule module = LoadModuleDllInfo((ModuleInfo)stepInfo, outtemplate, applyload);

                    if (applyload)
                    {
                        if (module is ISubRoutine)
                        {
                            Module.SubRoutine = module as ISubRoutine;
                        }
                        else
                        {

                            outtemplate.Add(module);
                        }


                        //if (Module is ICategoryNodeItem)
                        //    (Module as ICategoryNodeItem).Categories.Add(module);

                        if (Module is ICategoryNodeItem)
                            categories.Add(module);
                    }



                }
                else if (stepInfo is CategoryInfo)
                {
                    CategoryForm form = LoadNodeDllInfo((CategoryInfo)stepInfo, outtemplate, applyload);
                    if (applyload)
                    {
                        outtemplate.Add(form);
                    }

                }



                retval = EventCodeEnum.NONE;

                return retval;

            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;

                LoggerManager.Exception(err);


                return retval;
            }
        }

        private static ITemplateModule LoadModuleDllInfo(ModuleInfo stepInfo, ObservableCollection<ITemplateModule> outtemplate, bool applyload)
        {
            ITemplateModule cmodule = null;
            DllImporter DLLImporter = new DllImporter();
            ModuleInfo module = stepInfo;

            Tuple<bool, Assembly> ret = DLLImporter.LoadDLL(((ModuleInfo)stepInfo).DllInfo);

            try
            {
                if (ret != null && ret.Item1 == true)
                {
                    var TemplateModules = DLLImporter.Assignable<ITemplateModule>(ret.Item2);

                    for (int index = 0; index < TemplateModules.Count; index++)
                    {
                        object tmodule = TemplateModules[index];

                        if (module.DllInfo.ClassName.ToList<string>().Find(classname => classname.Equals(tmodule.GetType().Name)) != null)
                        {
                            if (tmodule is IHasDevParameterizable)
                            {
                                (tmodule as IHasDevParameterizable).LoadDevParameter();
                                (tmodule as IHasDevParameterizable).InitDevParameter();
                            }

                            if (tmodule is IHasSysParameterizable)
                            {
                                (tmodule as IHasSysParameterizable).LoadSysParameter();
                            }


                            if (tmodule is ITemplateModule)
                            {
                                cmodule = tmodule as ITemplateModule;

                                if (tmodule is ICategoryNodeItem)
                                    (tmodule as ICategoryNodeItem).SetEnableState(module.SetupEnableState);
                            }

                            if (tmodule is IModule)
                            {
                                (tmodule as IModule).InitModule();
                            }
                            if (tmodule is IMainScreenViewModel)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    (tmodule as IMainScreenViewModel).InitViewModel();
                                });
                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return cmodule;
        }


        private static EventCodeEnum LoadDllInfo(CategoryModuleBase stepInfo, ObservableCollection<ITemplateModule> outtemplate, bool applyload)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                DllImporter DLLImporter = new DllImporter();


                if (stepInfo is ModuleInfo)
                {
                    ITemplateModule module = LoadModuleDllInfo((ModuleInfo)stepInfo, outtemplate, applyload);

                    if (module != null)
                    {
                        outtemplate.Add(module);

                        //foreach (var obj in module)
                        //{
                        //    if (obj is ITemplateModule)
                        //        outtemplate.Add(obj);
                        //    //if (obj is IWizardPostTemplateModule)
                        //    //    templatemodules.PostProcModules.Add((IWizardPostTemplateModule)obj);
                        //}

                    }


                }
                else if (stepInfo is CategoryInfo)
                {
                    outtemplate.Add(LoadNodeDllInfo((CategoryInfo)stepInfo, outtemplate, applyload));
                }

                retval = EventCodeEnum.NONE;

                return retval;

            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;

                LoggerManager.Exception(err);

                return retval;
            }
        }

        private static CategoryForm LoadNodeDllInfo(CategoryInfo stepInfo, ObservableCollection<ITemplateModule> outtemplate, bool applyload)
        {
            CategoryForm form = new CategoryForm(((CategoryInfo)stepInfo).Header);
            try
            {
                foreach (var step in stepInfo.Categories)
                {
                    if (step is ModuleInfo)
                    {
                        ITemplateModule module = LoadModuleDllInfo((ModuleInfo)step, outtemplate, applyload);
                        if (module != null)
                            form.Categories.Add(module);
                        //if (module != null)
                        //{
                        //    foreach (var obj in module)
                        //    {
                        //        form.Categories.Add(obj);
                        //        //if (obj is IWizardPostTemplateModule)
                        //        //{
                        //        //    templatemodules.SubModules.Add(obj as ISubModule);
                        //        //    templatemodules.PostProcModules.Add((IWizardPostTemplateModule)obj);

                        //        //}
                        //    }
                        //}
                    }
                }
                form.InitViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

            return form;
        }

    }
}
