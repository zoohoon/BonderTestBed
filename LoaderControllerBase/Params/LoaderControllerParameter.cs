using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using LoaderParameters;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.LoaderController;

namespace LoaderControllerBase
{
    [Serializable]
    public class LoaderControllerParam : ILoaderControllerParam, INotifyPropertyChanged, IParam
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }

        private string _ControllerID;
        public string ControllerID
        {
            get { return _ControllerID; }
            set { _ControllerID = value; RaisePropertyChanged(); }
        }

        private int _ChuckIndex;
        public int ChuckIndex
        {
            get { return _ChuckIndex; }
            set { _ChuckIndex = value; RaisePropertyChanged(); }
        }

        private LoaderServiceTypeEnum _LoaderServiceType;
        public LoaderServiceTypeEnum LoaderServiceType
        {
            get { return _LoaderServiceType; }
            set { _LoaderServiceType = value; RaisePropertyChanged(); }
        }

        private string _EndpointConfigurationName;

        public string EndpointConfigurationName
        {
            get { return _EndpointConfigurationName; }
            set { _EndpointConfigurationName = value; RaisePropertyChanged(); }
        }

        private string _LoaderIP;

        public string LoaderIP
        {
            get { return _LoaderIP; }
            set { _LoaderIP = value; RaisePropertyChanged(); }
        }

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [JsonIgnore]
        public string FilePath { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }

        [JsonIgnore]
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

        [JsonIgnore]
        public List<object> Nodes { get; set; }
        = new List<object>();
    }
}
