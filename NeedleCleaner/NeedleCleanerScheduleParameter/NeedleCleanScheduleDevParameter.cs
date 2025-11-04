using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NeedleCleanerScheduleParameter
{
    [Serializable]
    public class NeedleCleanScheduleDevParameter : IDeviceParameterizable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public NeedleCleanScheduleDevParameter()
        {

        }
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; } = "NeedleCleanModule";
        public string FileName { get; } = "NeedleCleanScheduleDevParameter.json";
        private string _Genealogy;
        public string Genealogy
        {
            get { return _Genealogy; }
            set { _Genealogy = value; }
        }
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

        public List<object> Nodes { get; set; }

        private Element<int> _DieInterval
            = new Element<int>();
        public Element<int> DieInterval
        {
            get { return _DieInterval; }
            set
            {
                if (value != _DieInterval)
                {
                    _DieInterval = value;
                    NotifyPropertyChanged("DieInterval");
                }
            }
        }

        private Element<int> _WaferInterval
            = new Element<int>();
        public Element<int> WaferInterval
        {
            get { return _WaferInterval; }
            set
            {
                if (value != _WaferInterval)
                {
                    _WaferInterval = value;
                    NotifyPropertyChanged("WaferInterval");
                }
            }
        }

        private Element<int> _PadFocusingInterval
            = new Element<int>();
        public Element<int> PadFocusingInterval
        {
            get { return _PadFocusingInterval; }
            set
            {
                if (value != _PadFocusingInterval)
                {
                    _PadFocusingInterval = value;
                    NotifyPropertyChanged("PadFocusingInterval");
                }
            }
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
        public void SetElementMetaData()
        {

        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
    }
}
