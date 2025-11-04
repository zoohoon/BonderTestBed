using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;
using System.ServiceModel;
using System.Reflection;
using System.Timers;

namespace GPLoaderRouter
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [Serializable]
    public class IOSymbolMappings : INotifyPropertyChanged, ISystemParameterizable, IParamNode
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
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public string FilePath { get; } = "IO";

        public string FileName { get; } = "IOSymbolMappings.Json";



        public IOSymbolMappings()
        {

        }

        private List<IOPort_To_Symbol> _IOPortToSymbolList = new List<IOPort_To_Symbol>();
        public List<IOPort_To_Symbol> IOPortToSymbolList
        {
            get { return _IOPortToSymbolList; }
            set
            {
                if (_IOPortToSymbolList != value)
                {
                    _IOPortToSymbolList = value;
                }
            }
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                SetDefaultParam();
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From IOSymbolMappings. {err.Message}");
            }
            return RetVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                #region //FoupIO Symbol Mappings
                IOPortToSymbolList = new List<IOPort_To_Symbol>();

                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DOCCArmVac", "GV_Post_HW.DO_LCC_Vac"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DOCCArmVac_Break", "GV_Post_HW.DO_LCC_Vac_Break"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DOBuffVacs", "GV_Post_HW.DO_Buffer_Vac"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_COVER_OPENs", "GV_Post_HW.DO_CoverOpen"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_COVER_Closes", "GV_Post_HW.DO_CoverClose"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_COVER_LOCKs", "GV_Post_HW.DO_CoverLock"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_COVER_UNLOCKs", "GV_Post_HW.DO_CoverUnlock"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_LOADs", "GV_Post_HW.DO_CST_Load"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_UNLOADs", "GV_Post_HW.DO_CST_Unload"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_12INCH_LOCKs", "GV_Post_HW.DO_CST_Lock_12inch"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_12INCH_UNLOCKs", "GV_Post_HW.DO_CST_Unlock_12inch"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_8INCH_LOCKs", "GV_Post_HW.DO_CST_Lock_8inch"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_8INCH_UNLOCKs", "GV_Post_HW.DO_CST_Unlock_8inch"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_VACUUMs", "GV_Post_HW.DO_CST_Vacuum"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_MAPPINGs", "GV_Post_HW.DO_CST_Mapping"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_IND_ALARMs", "GV_Post_HW.DO_CST_IND_ALARM"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_IND_BUSYs", "GV_Post_HW.DO_CST_IND_BUSY"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_IND_RESERVEDs", "GV_Post_HW.DO_CST_IND_RESERVED"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_IND_AUTOs", "GV_Post_HW.DO_CST_IND_AUTO"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_IND_LOADs", "GV_Post_HW.DO_CST_IND_LOAD"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_IND_UNLOADs", "GV_Post_HW.DO_CST_IND_UNLOAD"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_IND_PLACEMENTs", "GV_Post_HW.DO_CST_IND_PLACEMENT"));
                IOPortToSymbolList.Add(new IOPort_To_Symbol($"DO_CST_IND_PRESENCEs", "GV_Post_HW.DO_CST_IND_PRESENCE"));




                #endregion

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }


        public bool IsServiceAvailable()
        {
            bool retVal = false;

            return retVal;
        }





    }

    [Serializable]
    public class IOPort_To_Symbol : INotifyPropertyChanged, IParamNode
    {
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

        public IOPort_To_Symbol()
        {
        }
        public IOPort_To_Symbol(string key, string symbolName)
        {
            IOPortKey = key;
            SymbolName = symbolName;

        }
        private string _IOPortKey;
        public string IOPortKey
        {
            get { return _IOPortKey; }
            set
            {
                if (_IOPortKey != value)
                {
                    _IOPortKey = value;
                }
            }
        }
        private string _SymbolName;
        public string SymbolName
        {
            get { return _SymbolName; }
            set
            {
                if (_SymbolName != value)
                {
                    _SymbolName = value;
                }
            }
        }
    }
}
