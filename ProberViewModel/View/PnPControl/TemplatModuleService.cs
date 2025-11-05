namespace PnPControl
{
    public static class TemplatModuleService
    {
        //public static void LoadTemplatModule(Autofac.IContainer container,ObservableCollection<CategoryModuleBase> templats)
        //{
        //    foreach(var templat in templats)
        //    {
        //        LoadDllInfo(container, templat);
        //    }
        //}

        //private static void LoadDllInfo(Autofac.IContainer container, ICategoryModule stepInfo)
        //{
        //    try
        //    {
        //        DllImporter DLLImporter = new DllImporter();

        //        if (stepInfo is ModuleInfo)
        //        {
        //            ModuleInfo module = (ModuleInfo)stepInfo;

        //            Tuple<bool, Assembly> ret = DLLImporter.LoadDLL(((ModuleInfo)stepInfo).DllInfo);

        //            if (ret != null && ret.Item1 == true)
        //            {
        //                for (int index = 0; index < DLLImporter.Assignable<ICategoryModule>(ret.Item2).Count; index++)
        //                {

        //                    module.AlignModule.Add(DLLImporter.Assignable<IAlignModule>(ret.Item2)[index]);

        //                    ((IAlignModule)module.AlignModule[module.AlignModule.Count - 1]).SetContainer(container);
        //                    ((IAlignModule)module.AlignModule[module.AlignModule.Count - 1]).LoadDevParameter();
        //                    ((IAlignModule)module.AlignModule[module.AlignModule.Count - 1]).InitModule(container);

        //                }
        //            }
        //        }

        //        if (stepInfo.Categories != null)
        //        {
        //            foreach (var module in stepInfo.Categories)
        //            {
        //                LoadDllInfo(container,module);

        //            }

        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Error($err + "LoadDllInfo() : Error occured.");
        //    }
        //}
    }
}
