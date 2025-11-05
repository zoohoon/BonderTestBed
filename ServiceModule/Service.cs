using ClientLibrary;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Service;
using ServiceModule;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClientModule
{
    public class RemoteServiceModule : IRemoteServiceModule, INotifyPropertyChanged, IHasSysParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private IClientService _ClientService;
        public IClientService ClientService
        {
            get { return _ClientService; }
            set
            {
                if (value != _ClientService)
                {
                    _ClientService = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RemoteServiceModuleParam remoteServiceParam;

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

            try
            {
                if (Initialized == false)
                {
                    if (remoteServiceParam != null)
                    {
                        if (remoteServiceParam.ServiceType.Value == ProberInterfaces.Enum.EnumServiceType.WCF)
                        {
                            ClientService = new WCFService();
                        }
                        else
                        {

                        }

                        ClientService?.Connect();
                    }

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

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retval;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            try
            {

            IParam tmpParam = null;
            tmpParam = new RemoteServiceModuleParam();
            tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
            RetVal = this.LoadParameter(ref tmpParam, typeof(RemoteServiceModuleParam));

            if (RetVal == EventCodeEnum.NONE)
            {
                remoteServiceParam = tmpParam as RemoteServiceModuleParam;
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        private EventCodeEnum LoadRemoteServiceParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            RemoteServiceModuleParam moduleParam = new RemoteServiceModuleParam();
            string FullPath = this.FileManager().GetSystemParamFullPath(moduleParam.FilePath, moduleParam.FileName);

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(RemoteServiceModuleParam), null, FullPath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    remoteServiceParam = tmpParam as RemoteServiceModuleParam;
                }

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[RemoteServiceModule] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            RemoteServiceModuleParam moduleParam = new RemoteServiceModuleParam();
            string FullPath = this.FileManager().GetSystemParamFullPath(moduleParam.FilePath, moduleParam.FileName);

            retVal = Extensions_IParam.SaveParameter(null, remoteServiceParam, null, FullPath);
            if (retVal != EventCodeEnum.NONE)
            {
                throw new Exception($"[{this.GetType().Name} - SaveAutoTiltSysFile] Faile SaveParameter");
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
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
    }
}
