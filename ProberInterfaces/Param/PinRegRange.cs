

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ProberInterfaces.Param
{
    using Newtonsoft.Json;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    [Serializable()]
    public class PinRegRange : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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

        #region ==> PinRegMin
        private Element<double> _PinRegMin = new Element<double>();
        public Element<double> PinRegMin
        {
            get { return _PinRegMin; }
            set
            {
                if (value != _PinRegMin)
                {
                    _PinRegMin = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PinRegMax
        private Element<double> _PinRegMax = new Element<double>();
        public Element<double> PinRegMax
        {
            get { return _PinRegMax; }
            set
            {
                if (value != _PinRegMax)
                {
                    _PinRegMax = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
    }
}
