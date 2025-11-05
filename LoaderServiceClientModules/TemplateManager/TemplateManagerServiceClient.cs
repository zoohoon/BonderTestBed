using Autofac;
using LoaderBase.Communication;
using LogModule;
using PnPControl;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Template;
using System;
using System.Collections.ObjectModel;

namespace LoaderServiceClientModules.TemplateManager
{
    public class TemplateManagerServiceClient : ITemplateManager
    {
        public bool Initialized => throw new NotImplementedException();

        //private Autofac.IContainer _Container;
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
            }
        }

        public EventCodeEnum CheckTemplate(ITemplate module, bool applyload = true, int index = -1)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                // Get RemoteProxy
                IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

                if (proxy != null)
                {
                    retval = proxy.CheckTemplate(module, applyload, index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum CheckTemplate(ITemplate module, bool applyload = true, int index = -1, int stageindex = -1)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                // Get RemoteProxy
                IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(stageindex);

                if (proxy != null)
                {
                    retval = proxy.CheckTemplate(module, applyload, index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum CheckTemplateUsedType(Type moduletype, bool applyload = true, int index = -1)
        {
            return EventCodeEnum.NONE;
        }

        public void DeInitModule()
        {
            return;
        }

        public ObservableCollection<ITemplateModule> GetModules(ITemplate module)
        {
            return null;
        }

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
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
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    module.LoadTemplateParam = tmpParam as TemplateFileParam;

                    if (module is ITemplateExtension ext)
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

        public ITemplateCollection LoadTemplate(object hasTemplateModule)
        {
            return null;
        }

        public void SaveTemplate(object hasTemplateModule)
        {
            return;
        }
    }
}
