namespace ProberInterfaces.Communication.BarcodeReader
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    [Serializable]
    public class BarcodeReaderSysParameters : ISystemParameterizable, IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
        public string FilePath { get; } = "";
        public string FileName { get; } = "BarcodeReaderSysParam.json";
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
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
            }

            return retval;
        }
        public void DefaultSetting()
        {
            try
            {
                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                {
                    BarcodeReaderParams.Add(new CommunicationParameterBase());
                    BarcodeReaderParams[i].ModuleIndex.Value = i;
                    BarcodeReaderParams[i].ModuleAttached.Value = false;
                    BarcodeReaderParams[i].ModuleCommType.Value = EnumCommmunicationType.EMUL;
                    BarcodeReaderParams[i].IP.Value = "127.0.0.1";
                    BarcodeReaderParams[i].Port.Value = 9004;
                }
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

        private ObservableCollection<CommunicationParameterBase> _BarcodeReaderParams
     = new ObservableCollection<CommunicationParameterBase>();
        public ObservableCollection<CommunicationParameterBase> BarcodeReaderParams
        {
            get { return _BarcodeReaderParams; }
            set
            {
                if (value != _BarcodeReaderParams)
                {
                    _BarcodeReaderParams = value;
                }
            }
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
