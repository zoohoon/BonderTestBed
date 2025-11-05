using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;

namespace BinAnalyzerManager
{
    [Serializable]
    public class BinDevParam : IDeviceParameterizable, INotifyPropertyChanged
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
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

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public string FilePath { get; } = "";

        public string FileName { get; } = "BinAnalyzerDeviceParam.json";

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                U32_32BitDictionary = new Dictionary<string, byte>();
                U32_64BitDictionary = new Dictionary<string, byte>();
                UserBinDictionary = new Dictionary<string, byte>();
                UserBinSize = 3;

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public void SetElementMetaData()
        {

        }   
        private Dictionary<string, byte> _U32_32BitDictionary;
        public Dictionary<string, byte> U32_32BitDictionary
        {
            get { return _U32_32BitDictionary; }
            set
            {
                if (value != _U32_32BitDictionary)
                {
                    _U32_32BitDictionary = value;
                    NotifyPropertyChanged(nameof(U32_32BitDictionary));
                }
            }
        }

        private Dictionary<string, byte> _U32_64BitDictionary;
        public Dictionary<string, byte> U32_64BitDictionary
        {
            get { return _U32_64BitDictionary; }
            set
            {
                if (value != _U32_64BitDictionary)
                {
                    _U32_64BitDictionary = value;
                    NotifyPropertyChanged(nameof(U32_64BitDictionary));
                }
            }
        }

        private Dictionary<string, byte> _UserBinDictionary;
        public Dictionary<string, byte> UserBinDictionary
        {
            get { return _UserBinDictionary; }
            set
            {
                if (value != _UserBinDictionary)
                {
                    _UserBinDictionary = value;
                    NotifyPropertyChanged(nameof(UserBinDictionary));
                }
            }
        }

        private int _UserBinSize;
        public int UserBinSize
        {
            get { return _UserBinSize; }
            set
            {
                if (value != _UserBinSize)
                {
                    _UserBinSize = value;
                    NotifyPropertyChanged(nameof(UserBinSize));
                }
            }
        }

    }
}
