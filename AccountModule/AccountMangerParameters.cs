using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace AccountModule
{
    [Serializable]
    public class MaskingLevelListParameter : INotifyPropertyChanged, IParam, ISystemParameterizable
    {
        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private ObservableCollection<int> _MaskingLevelCollection
            = new ObservableCollection<int>();
        public ObservableCollection<int> MaskingLevelCollection
        {
            get { return _MaskingLevelCollection; }
            set
            {
                if (value != _MaskingLevelCollection)
                {
                    _MaskingLevelCollection = value;
                    RaisePropertyChanged();
                }
            }
        }
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; set; } = "Account";
        public string FileName { get; set; } = "UserMakingLevelList.json";
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
        public List<object> Nodes { get; set; } = new List<object>();

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        public EventCodeEnum SetDefaultParam()
        {
            //MaskingLevelCollection.Add(9999999);
            MaskingLevelCollection.Add(5);
            MaskingLevelCollection.Add(4);
            MaskingLevelCollection.Add(3);
            MaskingLevelCollection.Add(2);
            MaskingLevelCollection.Add(1);
            MaskingLevelCollection.Add(0);

            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }
    }
}
