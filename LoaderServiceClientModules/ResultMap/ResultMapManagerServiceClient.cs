using Autofac;
using LoaderBase.Communication;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.ResultMap;
using ResultMapParamObject;
using SerializerUtil;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoaderServiceClientModules.ResultMap
{
    public class ResultMapManagerServiceClient : IResultMapManager, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        //private Autofac.IContainer _Container;
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
            }
        }

        public MapHeaderObject ResultMapHeader { get; set; }
        public ResultMapData ResultMapData { get; set; }

        public object ConvertedResultMap { get; private set; }

        public IParam ResultMapManagerSysIParam { get; private set; }

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

        public string LocalUploadPath => throw new NotImplementedException();

        public string LocalDownloadPath => throw new NotImplementedException();

        public EventCodeEnum ApplyBaseMap(MapHeaderObject mapHeader, ResultMapData resultMap)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ConvertResultMapToProberData(string lotID, string waferID)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ManualConvertResultMapToProberData(string filepath, FileManagerType managerType, FileReaderType readertype, Type deserializeObjtype = null)
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
                LoggerManager.Error($"[ResultMapManagerServiceClient], DeInitModule() : " + err.Message);
            }

            Initialized = false;
        }

        public EventCodeEnum InitModule()
        {
            Initialized = true;
            return EventCodeEnum.NONE;
        }

       
        public EventCodeEnum Download()
        {
            throw new NotImplementedException();
        }

        public char[,] GetASCIIMap()
        {
            throw new NotImplementedException();
        }

        public IParam GetResultMapConvIParam()
        {
            return ResultMapConvParam();
        }

        public ResultMapConverterParameter ResultMapConvParam()
        {
            byte[] obj = GetResultMapConvParam();
            object target = null;

            ResultMapConverterParameter retval = null;

            if (obj != null)
            {
                var result = SerializeManager.DeserializeFromByte(obj, out target, typeof(ResultMapConverterParameter));
                retval = target as ResultMapConverterParameter;
            }

            return retval;
        }

        public byte[] GetResultMapConvParam()
        {
            byte[] retval = null;

            IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

            if (proxy != null)
            {
                retval = proxy.GetResultMapConvParam();
            }

            return retval;
        }

        public ResultMapConvertType GetDownloadConverterType()
        {
            throw new NotImplementedException();
        }

        public ResultMapConvertType GetUploadConverterType()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitDevParameter()
        {
            throw new NotImplementedException();
        }

        public bool IsMapDownloadDone()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum LoadDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadRealTimeProbingData()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

                if (proxy != null)
                {
                    retval = proxy.SaveResultMapConvParam();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval; 
        }

        public string[] GetNamerAliaslist()
        {
            string[] retval = null;

            try
            {
                IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

                if (proxy != null)
                {
                    retval = proxy.GetNamerAliaslist();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SaveRealTimeProbingData()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum Upload()
        {
            throw new NotImplementedException();
        }
        public byte[] GetResultMapbyte(ref string filename)
        {
            throw new NotImplementedException();
        }

        public void SetResultMapConvIParam(byte[] param)
        {
            throw new NotImplementedException();
        }
        public bool SetResultMapByFileName(byte[] device, string resultmapname)
        {
            throw new NotImplementedException();
        }
        public bool NeedUpload()
        {
            throw new NotImplementedException();
        }

        public bool CanDownload()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum MakeResultMap(ref object resultmap, bool IsDummy = false)
        {
            throw new NotImplementedException();
        }

        public object GetOrgResultMap()
        {
            throw new NotImplementedException();
        }

        public bool NeedDownload()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum CheckAndDownload()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ManualUpload(object param, FileManagerType managertype, string filepath, FileReaderType readertype, Type serializeObjtype = null)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum Download(bool IsLotNameChangedTriggered)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum CheckAndDownload(bool IsLotNameChangedTriggered)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ConvertResultMapToProberData(string lotID, string waferID, string slotNum)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ConvertResultMapToProberDataAfterReadyToLoadWafer()
        {
            throw new NotImplementedException();
        }
    }
}
