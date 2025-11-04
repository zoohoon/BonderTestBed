using System;

namespace TemplateManager
{
    using ProberInterfaces;
    using ProberInterfaces.Template;
    using ProberErrorCode;
    using LogModule;
    using PnPControl;
    using System.Collections.ObjectModel;

    public class TemplateManager : ITemplateManager
    {
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
        public ITemplateCollection LoadTemplate(object hasTemplateModule)
        {
            return null;
        }

        public void SaveTemplate(object hasTemplateModule)
        {
            return;
        }

        public EventCodeEnum CheckTemplate(ITemplate module, bool applyload = true, int index = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (index != -1)
                {
                    retVal = InitTemplate(module, this.FileManager().GetDeviceParamFullPath(module.TemplateParameter.Param.TemplateInfos[index].Path.Value), applyload);
                    if (applyload)
                        module.TemplateParameter.Param.SeletedTemplate.Path = module.TemplateParameter.Param.TemplateInfos[index].Path;
                }
                else
                {
                    retVal = InitTemplate(module, module.TemplateParameter.Param.SeletedTemplate.Path.Value, applyload);
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    ObservableCollection<ITemplateModule> modules = new ObservableCollection<ITemplateModule>();
                    if (module is IConTemplateModule)
                    {
                        modules = (module as IConTemplateModule).Template.TemplateModules;
                    }
                    else if (module is ITemplateStateModule)
                    {
                        modules = (module as ITemplateStateModule).Template.TemplateModules;

                    }

                    foreach (var tmodule in modules)
                    {
                        if (tmodule is ICategoryNodeItem)
                        {
                            (tmodule as ICategoryNodeItem).ParamValidation();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err} CheckTemplate() : Error occurred.");
            }

            return retVal;
        }

        public EventCodeEnum CheckTemplate(ITemplate module, bool applyload = true, int index = -1, int stageindex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (index != -1)
                {
                    retVal = InitTemplate(module, this.FileManager().GetDeviceParamFullPath(module.TemplateParameter.Param.TemplateInfos[index].Path.Value), applyload);
                    if (applyload)
                        module.TemplateParameter.Param.SeletedTemplate.Path = module.TemplateParameter.Param.TemplateInfos[index].Path;
                }
                else
                {
                    retVal = InitTemplate(module, module.TemplateParameter.Param.SeletedTemplate.Path.Value, applyload);
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    ObservableCollection<ITemplateModule> modules = new ObservableCollection<ITemplateModule>();
                    if (module is IConTemplateModule)
                    {
                        modules = (module as IConTemplateModule).Template.TemplateModules;
                    }
                    else if (module is ITemplateStateModule)
                    {
                        modules = (module as ITemplateStateModule).Template.TemplateModules;

                    }

                    foreach (var tmodule in modules)
                    {
                        if (tmodule is ICategoryNodeItem)
                        {
                            (tmodule as ICategoryNodeItem).ParamValidation();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err} CheckTemplate() : Error occurred.");
            }

            return retVal;
        }


        public EventCodeEnum InitTemplate(ITemplate module, string fullpath = null, bool applyload = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;

                //LoggerManager.Debug($"Inittemplate Func() Start in {module.GetType().Name}");
                LoggerManager.Debug($"[S] {module.GetType().Name}, InitTemplate()");

                if (fullpath != null)
                {
                    retVal = this.LoadParameter(ref tmpParam, typeof(TemplateFileParam), null, fullpath);
                    LoggerManager.Debug($"{module.GetType().Name}, InitTemplate() : SeletedTemplate.Path {fullpath}");
                }
                else
                {
                    if (module.TemplateParameter is IDeviceParameterizable)
                    {
                        retVal = this.LoadParameter(ref tmpParam, typeof(TemplateFileParam), null, this.FileManager().GetDeviceParamFullPath(module.TemplateParameter.Param.SeletedTemplate.Path.Value));
                    }
                    else if (module.TemplateParameter is ISystemParameterizable)
                    {
                        retVal = this.LoadParameter(ref tmpParam, typeof(TemplateFileParam), null, this.FileManager().GetSystemParamFullPath(module.TemplateParameter.Param.SeletedTemplate.Path.Value, ""));
                    }
                    LoggerManager.Debug($"{module.GetType().Name}, InitTemplate() : SeletedTemplate.Path {module.TemplateParameter.Param.SeletedTemplate.Path.Value}");
                }
                
                if (retVal == EventCodeEnum.NONE)
                {
                    module.LoadTemplateParam = tmpParam as TemplateFileParam;

                    if (module is ITemplateExtension ext)// 로딩된 템플릿 정보를 변경
                    {
                        ext.InjectTemplate(module.LoadTemplateParam);
                    }

                   ITemplateCollection templatemodule = TemplatModuleService.LoadTemplateModule(this.GetContainer(), module, module.LoadTemplateParam, applyload);

                    if (module is IConTemplateModule)
                    {
                        (module as IConTemplateModule).Template = (TemplateCollection)templatemodule;
                    }
                    else if (module is ITemplateStateModule)
                    {
                        (module as ITemplateStateModule).Template = (TemplateStateCollection)templatemodule;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.PARAM_ERROR;

                    LoggerManager.Error(String.Format("[InitTemplate] InitTemplate(): DeSerialize Error"));
                }

                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err} InitTemplate() : Error occurred.");
                LoggerManager.Exception(err);
            }

            //LoggerManager.Debug($"Inittemplate Func() End in {module.GetType().Name}");
            LoggerManager.Debug($"[E] {module.GetType().Name}, InitTemplate()");

            return retVal;
        }

        public ObservableCollection<ITemplateModule> GetModules(ITemplate module)
        {
            ObservableCollection<ITemplateModule> modules = new ObservableCollection<ITemplateModule>();
            try
            {
                if (module is IConTemplateModule)
                {
                    modules = (module as IConTemplateModule).Template.TemplateModules;
                }
                else if (module is ITemplateStateModule)
                {
                    modules = (module as ITemplateStateModule).Template.TemplateModules;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return modules;
        }
    }
}
