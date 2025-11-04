using LogModule;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace LoaderParameters
{
    [DataContract]
    [Serializable]
    [KnownType(typeof(IElement))]
    public class PreAlignDevice : INotifyPropertyChanged, ICloneable, IParamNode
    {
        [XmlIgnore, JsonIgnore]
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

        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Element<string> _Label = new Element<string>();
        public Element<string> Label
        {
            get { return _Label; }
            set { _Label = value; RaisePropertyChanged(); }
        }

        public PreAlignDevice()
        {
            _Label.Value = string.Empty;
        }

        public ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.PA;
        }

        public object Clone()
        {
            try
            {
                var shallowClone = MemberwiseClone();
                return shallowClone;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

    }
}
