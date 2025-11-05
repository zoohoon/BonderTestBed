using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MultiExecuter.Model
{
    [Serializable]
    public class MultiExecuterParam : INotifyPropertyChanged, IParam
    {
        private ObservableCollection<ExecuteItem> _ExecuteItemCollection;
        public ObservableCollection<ExecuteItem> ExecuteItemCollection
        {
            get { return _ExecuteItemCollection; }
            set
            {
                _ExecuteItemCollection = value;
                NotifyPropertyChanged(nameof(ExecuteItemCollection));
            }
        }

        private string _ExePath;
        public string ExePath
        {
            get { return _ExePath; }
            set
            {
                if (value != _ExePath)
                {
                    _ExePath = value;
                    NotifyPropertyChanged(nameof(ExePath));
                }
            }
        }

        private string _IP = "localhost";

        public string IP
        {
            get { return _IP; }
            set { _IP = value; }
        }

        private int _Port = 20000;

        public int Port
        {
            get { return _Port; }
            set { _Port = value; }
        }

        private string _LIP = "192.168.8.101";
        public string LIP
        {
            get { return _LIP; }
            set { _LIP = value; }
        }

        private int _Time = 30;
        public int Time
        {
            get { return _Time; }
            set { _Time = value; }
        }

        private int _Limit = 95;
        public int Limit
        {
            get { return _Limit; }
            set
            {
                if (value >= 100)
                {
                    _Limit = 100;
                }
                else if (value < 0)
                {
                    _Limit = 0;
                }
                else
                {
                    _Limit = value;
                }
            }
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
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
            return SetDefaultParam();
        }


        public EventCodeEnum SetDefaultParam()
        {

            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                ExecuteItemCollection = new ObservableCollection<ExecuteItem>();
                ExecuteItem Cell_1 = new ExecuteItem(1);
                Cell_1.Path = @"C:\ProberSystem\Emul";
                ExecuteItemCollection.Add(Cell_1);

                retval = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
            
        }

       
    }
}
