using System;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using LoaderParameters;
using ProberInterfaces;
using LogModule;

namespace LoaderCore
{
    public class LightProxy : ILightProxy
    {
        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL3;

        public IContainer Container { get; set; }

        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();

        public bool Initialized { get; set; } = false;

        public ILightAdmin Light { get; set; }

        public void DeInitModule()
        {
            try
            {
                if (Loader.ServiceType == LoaderServiceTypeEnum.DynamicLinking)
                {
                    //No WORKS.
                }
                else
                {
                    Light.DeInitModule();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule(IContainer container)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    this.Container = container;

                    if (Loader.ServiceType == LoaderServiceTypeEnum.DynamicLinking)
                    {
                        Light = Loader.StageContainer.Resolve<ILightAdmin>();

                        retval = EventCodeEnum.NONE;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    Initialized = true;
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

        public EventCodeEnum InitModule()
        {
            throw new NotImplementedException();
        }

        public void SetLight(int channelMapIdx, ushort intensity)
        {
            try
            {
                Light.SetLight(channelMapIdx, intensity);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
