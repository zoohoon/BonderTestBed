using System;
using System.Collections.Generic;

namespace CameraChannelManager
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Xml.Serialization;

    [Serializable]
    public class CameraChannelParameter : ISystemParameterizable, IParamNode, IParam
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }



        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);


                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        [XmlIgnore, JsonIgnore]
        public string FilePath { get; } = "CameraChannelParameter";
        [XmlIgnore, JsonIgnore]
        public string FileName { get; } = "CameraChannelMapping.Json";
        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        private List<CameraChannelDescripter> _CameraChannelServiceParams = new List<CameraChannelDescripter>();
        public List<CameraChannelDescripter> CameraChannelServiceParams
        {
            get { return _CameraChannelServiceParams; }
            set { _CameraChannelServiceParams = value; }
        }

        public CameraChannelParameter()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }


        private void SetDefaultParam_Default()
        {
            try
            {
                CameraChannelDescripter stageParam = new CameraChannelDescripter();
                stageParam.ValuePortDescStr.Value = "3C0P:3C1P:3C2P";
                stageParam.CLPortDescStr.Value = "3C4P";
                stageParam.DataLoadPortDescStr.Value = "3C0P";
                stageParam.CameraChannelList.Add(new CameraChannel(0, 0));
                stageParam.CameraChannelList.Add(new CameraChannel(0, 4));
                stageParam.CameraChannelList.Add(new CameraChannel(0, 2));
                stageParam.CameraChannelList.Add(new CameraChannel(0, 6));

                CameraChannelDescripter loaderParam = new CameraChannelDescripter();
                loaderParam.ValuePortDescStr.Value = "3C5P:3C6P:3C7P";
                loaderParam.CLPortDescStr.Value = "3C4P";
                loaderParam.DataLoadPortDescStr.Value = "3C0P";
                loaderParam.CameraChannelList.Add(new CameraChannel(1, 0));
                loaderParam.CameraChannelList.Add(new CameraChannel(1, 1));
                loaderParam.CameraChannelList.Add(new CameraChannel(1, 2));
                loaderParam.CameraChannelList.Add(new CameraChannel(1, 3));
                loaderParam.CameraChannelList.Add(new CameraChannel(1, 4));
                loaderParam.CameraChannelList.Add(new CameraChannel(1, 5));
                loaderParam.CameraChannelList.Add(new CameraChannel(1, 6));
                loaderParam.CameraChannelList.Add(new CameraChannel(1, 7));

                _CameraChannelServiceParams.Add(stageParam);
                _CameraChannelServiceParams.Add(loaderParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SetDefaultParam_Default();


                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }
            return retVal;
        }
        public void SetElementMetaData()
        {

        }
    }
}
