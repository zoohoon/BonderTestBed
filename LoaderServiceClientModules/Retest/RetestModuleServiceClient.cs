using Autofac;
using LoaderBase.Communication;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Proxies;
using ProberInterfaces.Retest;
using RetestObject;
using SerializerUtil;
using System;

namespace LoaderServiceClientModules.Retest
{
    public class RetestModuleServiceClient : IRetestModule
    {
        private IParam _RetestModuleDevParam_IParam;
        public IParam RetestModuleDevParam_IParam
        {
            get
            {
                _RetestModuleDevParam_IParam = RetestModuleParam();
                return _RetestModuleDevParam_IParam;
            }
            set { _RetestModuleDevParam_IParam = value; }
        }

        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
            }
        }

        public bool Initialized
        {
            get
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    if (LoaderCommunicationManager.SelectedStage.StageInfo.IsConnected == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            set { }
        }

        public IParam GetRetestIParam()
        {
            return RetestModuleParam();
        }

        public void SetRetestIParam(byte[] param)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<IRetestModuleProxy>().SetRetestParam(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public byte[] GetRetestParam()
        {
            byte[] retval = null;

            IRetestModuleProxy proxy = LoaderCommunicationManager.GetProxy<IRetestModuleProxy>();

            if (proxy != null)
            {
                retval = proxy.GetRetestParam();
            }

            return retval;
        }

        public RetestDeviceParam RetestModuleParam()
        {
            byte[] obj = GetRetestParam();
            object target = null;

            RetestDeviceParam retval = null;

            if (obj != null)
            {
                var result = SerializeManager.DeserializeFromByte(obj, out target, typeof(RetestDeviceParam));
                retval = target as RetestDeviceParam;
            }

            return retval;
        }

        public EventCodeEnum InitModule()
        {
            try
            {
                Initialized = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"[RetestModuleServiceClient], Function error: " + err.Message);
            }

            Initialized = false;
        }

        public bool IsServiceAvailable()
        {
            return true;
        }

        public EventCodeEnum LoadDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum InitDevParameter()
        {
            throw new NotImplementedException();
        }

    }
}
