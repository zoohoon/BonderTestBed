namespace ProberInterfaces.Param
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [Serializable]
    public class LoaderProbeCardListParameter : ISystemParameterizable
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }

        public void SetElementMetaData()
        {
            
        }
        #endregion

        private List<ProberCardListParameter> _ProbeCardList = new List<ProberCardListParameter>();
        public List<ProberCardListParameter> ProbeCardList
        {
            get { return _ProbeCardList; }
            set
            {
                if (value != _ProbeCardList)
                {
                    _ProbeCardList = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string FilePath { get; set; } = "";
        public string FileName { get; } = "LoaderProbeCardListParameter.json";
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

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
    }
}
