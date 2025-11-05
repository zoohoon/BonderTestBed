using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using ProberErrorCode;
using LogModule;
using Newtonsoft.Json;

namespace ProberInterfaces.AirCooling
{
    public interface IAirCoolingModule : IStateModule
    {
    }
    [Serializable]
    public class AirCoolingSystemFile : INotifyPropertyChanged, ISystemParameterizable,IParamNode
    {

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
                

                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        public string FilePath { get; } = "";

        public string FileName { get; } = "AirCoolingSystemFile.json";
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore]
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

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                AirCoolingOnProber.Value = false;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }


            return retVal;
        }
        public AirCoolingSystemFile() { }

        private Element<bool> _AirCoolingOnProber =  new Element<bool>();
        public Element<bool> AirCoolingOnProber
        {
            get { return _AirCoolingOnProber; }
            set
            {
                if (value != _AirCoolingOnProber)
                {
                    _AirCoolingOnProber = value;
                    NotifyPropertyChanged("AirCoolingOnProber");
                }
            }
        }

       
    }
    [Serializable]
    public class AirCoolingDeviceFile : INotifyPropertyChanged, IDeviceParameterizable, IParamNode
    {

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
                LoggerManager.Debug($"[AirCoolingDeviceFile] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        public string FilePath { get; set; } = "";

        public string FileName { get; } = "AirCoolingDeviceFile.json";


        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
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

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        /// <summary>
        /// 에어쿨링할때 척의 목표 온도
        /// </summary>
        private Element<double> _TargetTemp = new Element<double>();
        public Element<double> TargetTemp
        {
            get { return _TargetTemp; }
            set
            {
                if (value != _TargetTemp)
                {
                    _TargetTemp = value;
                    NotifyPropertyChanged("TargetTemp");
                }
            }
        }

        private Element<bool> _AirCoolingEnable = new Element<bool>();
        public Element<bool> AirCoolingEnable
        {
            get { return _AirCoolingEnable; }
            set
            {
                if (value != _AirCoolingEnable)
                {
                    _AirCoolingEnable = value;
                    NotifyPropertyChanged("AirCoolingEnable");
                }
            }
        }

        private Element<int> _AirActivatingTime = new Element<int>();
        public Element<int> AirActivatingTime
        {
            get { return _AirActivatingTime; }
            set
            {
                if (value != _AirActivatingTime)
                {
                    _AirActivatingTime = value;
                    NotifyPropertyChanged("AirActivatingTime");
                }
            }
        }

        private Element<int> _AirDeActivatingTime = new Element<int>();
        public Element<int> AirDeActivatingTime
        {
            get { return _AirDeActivatingTime; }
            set
            {
                if (value != _AirDeActivatingTime)
                {
                    _AirDeActivatingTime = value;
                    NotifyPropertyChanged("AirDeActivatingTime");
                }
            }
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[AirCoolingSystemFile] [Method = SetDefaultParam] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
    }
}
