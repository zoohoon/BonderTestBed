using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LoginParamObject
{
    [Serializable]
    public class LoginParameter : ISystemParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FilePath { get; } = "";
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; } = "LoginParameter.json";
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set { _Owner = value; }
        }


        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            LastLoginedAccount = "semics";
            LoginSkipEnable = true;
            UserList = new ObservableCollection<Account>();
            retVal = EventCodeEnum.NONE;

            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = SetEmulParam();

            return retVal;
        }
        public void SetElementMetaData()
        {

        }

        public String LastLoginedAccount { get; set; }
        public bool LoginSkipEnable { get; set; }
        private ObservableCollection<Account> _UserList;
        public ObservableCollection<Account> UserList
        {
            get { return _UserList; }
            set
            {
                if (value != _UserList)
                {
                    _UserList = value;
                }
            }
        }


    }
}
