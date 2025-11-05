using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.ResultMap;
using ProberInterfaces.ResultMap.Attributes;
using ResultMapParamObject.E142;
using ResultMapParamObject.STIF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ResultMapParamObject
{
    public class ResultMapConverterParameter : IDeviceParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region ==> IParam
        public string FilePath { get; set; } = "ResultMap";
        public string FileName { get; set; } = nameof(ResultMapConverterParameter) + ".json";
        public bool IsParamChanged { get; set; }
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; } = new List<object>();
        #endregion

        private Element<ResultMapConvertType> _uploadType = new Element<ResultMapConvertType>();
        public Element<ResultMapConvertType> UploadType
        {
            get { return _uploadType; }
            set
            {
                if (value != _uploadType)
                {
                    _uploadType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<ResultMapConvertType> _downloadType = new Element<ResultMapConvertType>();
        public Element<ResultMapConvertType> DownloadType
        {
            get { return _downloadType; }
            set
            {
                if (value != _downloadType)
                {
                    _downloadType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private STIFMapParameter _STIFParam;
        [MapConverterPropety(ResultMapConvertType.STIF)]
        public STIFMapParameter STIFParam
        {
            get { return _STIFParam; }
            set
            {
                if (value != _STIFParam)
                {
                    _STIFParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E142MapParameter _E142Param;
        [MapConverterPropety(ResultMapConvertType.E142)]
        public E142MapParameter E142Param
        {
            get { return _E142Param; }
            set
            {
                if (value != _E142Param)
                {
                    _E142Param = value;
                    RaisePropertyChanged();
                }
            }
        }


        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SetEmulParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                UploadType.Value = ResultMapConvertType.STIF;
                DownloadType.Value = ResultMapConvertType.STIF;

                STIFParam = new STIFMapParameter();
                STIFParam.Init();

                E142Param = new E142MapParameter();
                E142Param.Init();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void SetElementMetaData()
        {
            try
            {
                UploadType.CategoryID = "10051001";
                UploadType.ElementName = "Upload type";
                UploadType.Description = "Type used when uploading and converting";

                DownloadType.CategoryID = "10051001";
                DownloadType.ElementName = "Download type";
                DownloadType.Description = "Type used when downloading and converting";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public MapConverterParamBase GetConvParam(ResultMapConvertType type)
        {
            MapConverterParamBase retval = null;

            try
            {
                Type t = this.GetType();

                PropertyInfo[] props = t.GetProperties();

                foreach (var prop in props)
                {
                    if (Attribute.IsDefined(prop, typeof(MapConverterPropetyAttribute)))
                    {
                        var att = prop.GetCustomAttributes(typeof(MapConverterPropetyAttribute), false).FirstOrDefault();

                        if (att != null)
                        {
                            ResultMapConvertType convtype = (att as MapConverterPropetyAttribute).CoverterType;

                            if (convtype == type)
                            {
                                retval = prop.GetValue(this, null) as MapConverterParamBase;
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

            return retval;
        }
    }
}
