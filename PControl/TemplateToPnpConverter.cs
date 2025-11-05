using System;
using System.Collections.Generic;
using System.Linq;

namespace PnPControl
{
    using System.Collections.ObjectModel;
    using ProberInterfaces;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.Wizard;
    using ProberInterfaces.State;
    using PnPontrol;
    using LogModule;

    public static class TemplateToPnpConverter
    {

        //public static ObservableCollection<ObservableCollection<ICategoryNodeItem>> Converter(
        //    ITemplateModule module, ObservableCollection<int> initIDs = null, bool issetup = true, bool ispostmodule = false)
        //{
        //    ObservableCollection<ObservableCollection<ICategoryNodeItem>> pnpSteps = new ObservableCollection<ObservableCollection<ICategoryNodeItem>>();
        //    ObservableCollection<ICategoryNodeItem> step = new ObservableCollection<ICategoryNodeItem>();

        //    try
        //    {
        //        if (module is CategoryForm)
        //        {
        //            ICategoryNodeItem pnp = LoadNodePnp((CategoryForm)module, issetup);
        //            if (pnp != null)
        //            {
        //                step.Add(pnp);
        //            }
        //        }
        //        else
        //        {
        //            ICategoryNodeItem pnp = LoadPnpModule(module, issetup,ispostmodule);
        //            if (pnp != null)
        //            {
        //                step.Add(pnp);
        //            }
        //        }

        //        pnpSteps.Add(step);
        //    }
        //    catch (Exception  err)
        //    {

        //        Log.Error(err, "TemplateToPnpConverter - Converter() : Error occurred.");

        //    }


        //    return pnpSteps;
        //}

        public static ObservableCollection<ObservableCollection<ICategoryNodeItem>> Converter(
    ITemplateModule module, bool issetup = true, bool ispostmodule = false)
        {
            ObservableCollection<ObservableCollection<ICategoryNodeItem>> pnpSteps = new ObservableCollection<ObservableCollection<ICategoryNodeItem>>();
            try
            {
                ObservableCollection<ICategoryNodeItem> step = new ObservableCollection<ICategoryNodeItem>();

                try
                {
                    if (module is CategoryForm)
                    {
                        ICategoryNodeItem pnp = LoadNodePnp((CategoryForm)module, issetup);
                        if (pnp != null)
                        {
                            step.Add(pnp);
                        }
                    }
                    else
                    {
                        ICategoryNodeItem pnp = LoadPnpModule(module, issetup, ispostmodule);
                        if (pnp != null)
                        {
                            step.Add(pnp);
                        }
                    }

                    pnpSteps.Add(step);
                }
                catch (Exception err)
                {
                    //LoggerManager.Error($err, "TemplateToPnpConverter - Converter() : Error occurred.");
                    LoggerManager.Exception(err);
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return pnpSteps;
        }

        public static ObservableCollection<ObservableCollection<ICategoryNodeItem>> Converter(
            ObservableCollection<ITemplateModule> templatemodule, bool issetup = true)
        {
            ObservableCollection<ObservableCollection<ICategoryNodeItem>> pnpSteps = new ObservableCollection<ObservableCollection<ICategoryNodeItem>>();
            try
            {
                ObservableCollection<ICategoryNodeItem> step = new ObservableCollection<ICategoryNodeItem>();
                try
                {
                    //if (initIDs != null)
                    //{
                    //    step.Add(new InitSettingPnpBase(initIDs));
                    //}

                    foreach (var module in templatemodule)
                    {
                        if (module is CategoryForm)
                        {
                            ICategoryNodeItem pnp = LoadNodePnp((CategoryForm)module, issetup);
                            if (pnp != null)
                            {
                                //ObservableCollection<ICategoryNodeItem> node = new ObservableCollection<ICategoryNodeItem>();
                                //node.Add(pnp);
                                step.Add(pnp);
                                //pnpSteps.Add(node);
                            }
                        }
                        else
                        {
                            ICategoryNodeItem pnp = LoadPnpModule(module, issetup);
                            if (pnp != null)
                            {
                                step.Add(pnp);

                                //ObservableCollection<ICategoryNodeItem> node = new ObservableCollection<ICategoryNodeItem>();
                                //node.Add(pnp);
                                //pnpSteps.Add(node);
                                //pnpSteps.Add(pnp);
                            }
                        }
                    }

                    pnpSteps.Add(step);

                }
                catch (Exception err)
                {
                    //LoggerManager.Exception(err);
                    //LoggerManager.Error($err, "TemplateToPnpConverter - Converter() : Error occurred.");
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return pnpSteps;
        }
        //public static T Clone<T>(this T obj)
        //{
        //    var inst = obj.GetType().GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        //    return (T)inst?.Invoke(obj, null);
        //}
        private static ICategoryNodeItem LoadPnpModule(ITemplateModule module, bool issetup = true, bool ispostmodule = false)
        {
            try
            {
                if (issetup)
                {
                    if (!ispostmodule)
                    {
                        if (module is ISetup && module is ICategoryNodeItem && !(module is IWizardPostTemplateModule))
                        {
                            if ((module as ICategoryNodeItem).StateEnable == EnumEnableState.ENABLE
                                 || (module as ICategoryNodeItem).StateEnable == EnumEnableState.MUST)
                            {

                                if ((module as ICategoryNodeItem).Categories.Count != 0)
                                {
                                    //ICategoryNodeItem node = (ICategoryNodeItem)ObjectExtensions.DeepClone(module);
                                    //ObservableCollection<ITemplateModule> categorymdoules = new ObservableCollection<ITemplateModule>();
                                    //foreach (var cmodule in (module as ICategoryNodeItem).Categories)
                                    //{
                                    //    ITemplateModule item = (ITemplateModule)cmodule;
                                    //    if (cmodule is ISetup && cmodule is ICategoryNodeItem && !(cmodule is IWizardPostTemplateModule))
                                    //    {
                                    //        if ((cmodule as ICategoryNodeItem).StateEnable == EnumEnableState.ENABLE
                                    //             || (cmodule as ICategoryNodeItem).StateEnable == EnumEnableState.MUST)
                                    //        {
                                    //            categorymdoules.Add(cmodule as ITemplateModule);
                                    //        }
                                    //    }
                                    //}
                                    //node.Categories = categorymdoules;
                                    //return (ICategoryNodeItem)node;
                                }
                                return (ICategoryNodeItem)module;

                            }

                        }

                    }
                    else
                    {
                        if (module is ISetup && module is ICategoryNodeItem && module is IWizardPostTemplateModule)
                        {
                            if ((module as ICategoryNodeItem).StateEnable == EnumEnableState.ENABLE
                                 || (module as ICategoryNodeItem).StateEnable == EnumEnableState.MUST)
                                return (ICategoryNodeItem)module;
                        }
                    }

                }
                else
                {
                    if (module is IRecovery && module is IPnpSetup && !(module is IWizardPostTemplateModule))
                    {
                        if ((module as ICategoryNodeItem).StateEnable == EnumEnableState.ENABLE
                                 || (module as ICategoryNodeItem).StateEnable == EnumEnableState.MUST)
                        {


                            if ((module as ICategoryNodeItem).Categories.Count != 0)
                            {
                                ICategoryNodeItem node = Extensions_IParam.Copy<ICategoryNodeItem>(module as ICategoryNodeItem);
                                ObservableCollection<ITemplateModule> categorymdoules = new ObservableCollection<ITemplateModule>();
                                foreach (var cmodule in (module as ICategoryNodeItem).Categories)
                                {
                                    ITemplateModule item = (ITemplateModule)cmodule;
                                    if (cmodule is IRecovery && cmodule is ICategoryNodeItem && !(cmodule is IWizardPostTemplateModule))
                                    {
                                        if ((cmodule as ICategoryNodeItem).StateEnable == EnumEnableState.ENABLE
                                             || (cmodule as ICategoryNodeItem).StateEnable == EnumEnableState.MUST)
                                        {
                                            categorymdoules.Add(cmodule as ITemplateModule);
                                        }
                                    }
                                }
                                node.Categories = categorymdoules;
                                return (ICategoryNodeItem)node;
                            }

                            return (ICategoryNodeItem)module;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return null;
        }

        private static ICategoryNodeItem LoadNodePnp(CategoryForm module, bool issetup = true)
        {
            try
            {
                foreach (var submodule in module.Categories)
                {
                    ICategoryNodeItem pnp = LoadPnpModule(submodule, issetup);

                    if (pnp != null)
                    {
                        (submodule as ICategoryNodeItem).Parent = module;
                        //module.Categories.Add(pnp);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return module;
        }


        public static ObservableCollection<ObservableCollection<ICategoryNodeItem>> ConverterUserGUIDList(ObservableCollection<ITemplateModule> templatemodule, List<Guid> guids, bool issetup = true)
        {
            ObservableCollection<ObservableCollection<ICategoryNodeItem>> pnpSteps = new ObservableCollection<ObservableCollection<ICategoryNodeItem>>();
            ObservableCollection<ICategoryNodeItem> step = new ObservableCollection<ICategoryNodeItem>();
            try
            {

                foreach (var guid in guids)
                {
                    List<Guid> tmpguid = new List<Guid>() { guid };
                    foreach (var module in templatemodule)
                    {
                        if (module is CategoryForm)
                        {
                            ICategoryNodeItem pnp = N_LoadNodePnp((CategoryForm)module, tmpguid, issetup);
                            if (pnp != null)
                            {
                                if(step.ToList<ICategoryNodeItem>().Find(sp => sp.Header == pnp.Header) == null)
                                {
                                    step.Add(pnp);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            ICategoryNodeItem pnp = N_LoadPnpModule(module, tmpguid, issetup);
                            if (pnp != null)
                            {
                                step.Add(pnp);
                                break;
                            }
                        }
                    }
                }

                pnpSteps.Add(step);
            }
            catch (Exception err)
            {
                throw err;
            }
            return pnpSteps;
        }
        public static ObservableCollection<ObservableCollection<ICategoryNodeItem>> ConverterNotFormUserGUIDList(ObservableCollection<ITemplateModule> templatemodule, List<Guid> guids, bool issetup = true)
        {
            ObservableCollection<ObservableCollection<ICategoryNodeItem>> pnpSteps = new ObservableCollection<ObservableCollection<ICategoryNodeItem>>();
            ObservableCollection<ICategoryNodeItem> step = new ObservableCollection<ICategoryNodeItem>();
            try
            {

                foreach (var guid in guids)
                {
                    List<Guid> tmpguid = new List<Guid>() { guid };
                    foreach (var module in templatemodule)
                    {
                        if (module is CategoryForm)
                        {
                            foreach (var item in (module as ICategoryNodeItem).Categories)
                            {
                                ICategoryNodeItem pnp = N_LoadPnpModule(item, tmpguid, issetup);
                                if (pnp != null)
                                {
                                    if (step.ToList<ICategoryNodeItem>().Find(sp => sp.Header == pnp.Header) == null)
                                    {
                                        step.Add(pnp);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            ICategoryNodeItem pnp = N_LoadPnpModule(module, tmpguid, issetup);
                            if (pnp != null)
                            {
                                step.Add(pnp);
                                break;
                            }
                        }
                    }
                }

                pnpSteps.Add(step);
            }
            catch (Exception err)
            {
                throw err;
            }
            return pnpSteps;
        }

        private static ICategoryNodeItem N_LoadPnpModule(ITemplateModule module, List<Guid> guids, bool issetup = true)
        {
            try
            {
                if (module is IMainScreenViewModel)
                {
                    Guid mguid = (module as IMainScreenViewModel).ScreenGUID;

                    foreach (var guid in guids)
                    {
                        if (guid.ToString() == mguid.ToString())
                        {
                            if (issetup)
                            {
                                if (module is ISetup && module is ICategoryNodeItem)
                                {
                                    if ((module as ICategoryNodeItem).StateEnable == EnumEnableState.ENABLE
                                        || (module as ICategoryNodeItem).StateEnable == EnumEnableState.MUST)
                                        return (ICategoryNodeItem)module;
                                    // if ((module as ICategoryNodeItem).StateEnable == EnumEnableState.ENABLE
                                    //|| (module as ICategoryNodeItem).StateEnable == EnumEnableState.MUST)
                                    // {


                                    //     if ((module as ICategoryNodeItem).Categories.Count != 0)
                                    //     {
                                    //         ICategoryNodeItem node = (ICategoryNodeItem)ObjectExtensions.DeepClone(module);
                                    //         ObservableCollection<ITemplateModule> categorymdoules = new ObservableCollection<ITemplateModule>();
                                    //         foreach (var cmodule in (module as ICategoryNodeItem).Categories)
                                    //         {
                                    //             ITemplateModule item = (ITemplateModule)cmodule;
                                    //             if (cmodule is ISetup && cmodule is ICategoryNodeItem && !(cmodule is IWizardPostTemplateModule))
                                    //             {
                                    //                 if ((cmodule as ICategoryNodeItem).StateEnable == EnumEnableState.ENABLE
                                    //                      || (cmodule as ICategoryNodeItem).StateEnable == EnumEnableState.MUST)
                                    //                 {
                                    //                     categorymdoules.Add(cmodule as ITemplateModule);
                                    //                 }
                                    //             }
                                    //         }

                                    //         node.Categories = categorymdoules;
                                    //         return (ICategoryNodeItem)node;
                                    //     }
                                    //     return (module as ICategoryNodeItem);

                                    // }

                                }

                            }
                            else
                            {
                                if (module is IRecovery && module is IPnpSetup)
                                {
                                    //if ((module as ICategoryNodeItem).StateEnable == EnumEnableState.ENABLE
                                    //                                 || (module as ICategoryNodeItem).StateEnable == EnumEnableState.MUST)
                                    //{

                                    //    if ((module as ICategoryNodeItem).Categories.Count != 0)
                                    //    {
                                    //        ICategoryNodeItem node = (ICategoryNodeItem)ObjectExtensions.DeepClone(module);

                                    //        ObservableCollection<ITemplateModule> categorymdoules = new ObservableCollection<ITemplateModule>();
                                    //        foreach (var cmodule in (module as ICategoryNodeItem).Categories)
                                    //        {
                                    //            ITemplateModule item = (ITemplateModule)cmodule;
                                    //            if (cmodule is IRecovery && cmodule is ICategoryNodeItem && !(cmodule is IWizardPostTemplateModule))
                                    //            {
                                    //                if ((cmodule as ICategoryNodeItem).StateEnable == EnumEnableState.ENABLE
                                    //                     || (cmodule as ICategoryNodeItem).StateEnable == EnumEnableState.MUST)
                                    //                {
                                    //                    categorymdoules.Add(cmodule as ITemplateModule);
                                    //                }
                                    //            }
                                    //        }
                                    //        node.Categories = categorymdoules;
                                    //        return (ICategoryNodeItem)node;
                                    //    }

                                    //    return (ICategoryNodeItem)module;
                                    //}
                                    if ((module as ICategoryNodeItem).StateEnable == EnumEnableState.ENABLE
                                        || (module as ICategoryNodeItem).StateEnable == EnumEnableState.MUST)
                                        return (ICategoryNodeItem)module;
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

            return null;
        }

        private static ICategoryNodeItem N_LoadNodePnp(CategoryNodeSetupBase module, List<Guid> guids, bool issetup = true)
        {
            try
            {
                bool existequalmodule = false;
                foreach (var submodule in module.Categories)
                {
                    ICategoryNodeItem pnp = N_LoadPnpModule(submodule, guids, issetup);

                    if (pnp != null)
                    {
                        (submodule as ICategoryNodeItem).Parent = module;
                        existequalmodule = true;
                        //module.Categories.Add(pnp);
                    }

                }

                if (!existequalmodule)
                    return null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return module;
        }



        public static ObservableCollection<ObservableCollection<CategoryNameItems>> ConverterUserGuidList(ObservableCollection<ITemplateModule> templatemodule, List<Guid> guids, bool issetup = true)
        {
            ObservableCollection<ObservableCollection<CategoryNameItems>> pnpSteps = new ObservableCollection<ObservableCollection<CategoryNameItems>>();
            ObservableCollection<CategoryNameItems> step = new ObservableCollection<CategoryNameItems>();

                try
                {


                foreach (var guid in guids)
                {
                    List<Guid> tmpguid = new List<Guid>() { guid };
                    foreach (var module in templatemodule)
                    {
                        if (module is CategoryForm)
                        {
                            CategoryNameItems pnp = CreateNameNodePNP((CategoryForm)module, tmpguid, issetup);
                            if (pnp != null)
                            {
                                if(step.ToList<CategoryNameItems>().Find(sp => sp.Header == pnp.Header) == null)
                                 {
                                    step.Add(pnp);
                                   
                                }
                                else
                                {
                                    var form = step.ToList<CategoryNameItems>().Find(sp => sp.Header == pnp.Header);
                                    if(form.Categories.ToList< CategoryNameItems>().Find(sp => sp.Header == pnp.Header) == null)
                                    {
                                        foreach (var cate in pnp.Categories)
                                        {
                                            form.Categories.Add(cate);
                                        }

                                    }
                                }

                            }
                        }
                        else
                        {
                            CategoryNameItems pnp = CreateNameNodeModulePNP(module, tmpguid, issetup);
                            if (pnp != null)
                            {
                                step.Add(pnp);
                                break;
                            }
                        }
                    }
                }
                

                    pnpSteps.Add(step);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return pnpSteps;
         }

        private static CategoryNameItems CreateNameNodeModulePNP(ITemplateModule module, List<Guid> guids, bool issetup = true)
        {
            try
            {
                if (module is IMainScreenViewModel)
                {
                    Guid mguid = (module as IMainScreenViewModel).ScreenGUID;

                    foreach (var guid in guids)
                    {
                        if (guid.ToString() == mguid.ToString())
                        {
                            if (issetup)
                            {
                                if (module is ISetup && module is ICategoryNodeItem)
                                {
                                    if ((module as ICategoryNodeItem).StateEnable == EnumEnableState.ENABLE
                                        || (module as ICategoryNodeItem).StateEnable == EnumEnableState.MUST)
                                    {
                                        //return (ICategoryNodeItem)module;
                                        return new CategoryNameItems((module as ICategoryNodeItem).Header, (module as ICategoryNodeItem).RecoveryHeader);
                                    }
                                }
                            }
                            else
                            {
                                if (module is IRecovery && module is IPnpSetup)
                                {              
                                    if ((module as ICategoryNodeItem).StateEnable == EnumEnableState.ENABLE
                                        || (module as ICategoryNodeItem).StateEnable == EnumEnableState.MUST)
                                    {
                                        //return (ICategoryNodeItem)module;
                                        return new CategoryNameItems((module as ICategoryNodeItem).Header, (module as ICategoryNodeItem).RecoveryHeader);
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

            return null;
        }


        private static CategoryNameItems CreateNameNodePNP(CategoryNodeSetupBase module, List<Guid> guids, bool issetup = true)
        {
            CategoryNameItems item = new CategoryNameItems();
            try
            {
                item.IsCategoryForm = true;
                item.Header = module.Header;
                bool existequalmodule = false;
                foreach (var submodule in module.Categories)
                {
                    CategoryNameItems pnp = CreateNameNodeModulePNP(submodule, guids, issetup);

                    if (pnp != null)
                    {
                        (submodule as ICategoryNodeItem).Parent = module;
                        item.Categories.Add(new CategoryNameItems(module.Header,pnp.Header, pnp.RecoveryHeader));
                        existequalmodule = true;
                    }
                }
                if (!existequalmodule)
                    return null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return item;
        }


    }
}
