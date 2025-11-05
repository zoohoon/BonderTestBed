namespace ProberInterfaces.CassetteIDReader
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    public enum EnumCSTIDReaderType
    {
        RFID,
        BARCODE
    }

    public class CassetteIDReaderParameters : ISystemParameterizable, IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
        public string FilePath { get; } = "";
        public string FileName { get; } = "CassetteIDReaderSysParam.json";
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }

        private Element<EnumCSTIDReaderType> _CassetteIDReaderType = new Element<EnumCSTIDReaderType>();
        public Element<EnumCSTIDReaderType> CassetteIDReaderType
        {
            get { return _CassetteIDReaderType; }
            set
            {
                if (value != _CassetteIDReaderType)
                {
                    _CassetteIDReaderType = value;
                }
            }
        }

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
        public void DefaultSetting()
        {
            try
            {
                CassetteIDReaderType.Value = EnumCSTIDReaderType.RFID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                DefaultSetting();
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
        public void SetElementMetaData()
        {

        }

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
    }
}
