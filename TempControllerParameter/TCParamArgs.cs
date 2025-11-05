using Newtonsoft.Json;
using ProberInterfaces;
using ProberInterfaces.Temperature.TempManager;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TempControllerParameter
{
    [Serializable]
    public class TCParamArgs : IParamNode, ITCParamArgs
    {
        private Element<double> _Pb;
        public Element<double> Pb
        {
            get { return _Pb; }
            set { _Pb = value; }
        }

        private Element<double> _iT;
        public Element<double> iT
        {
            get { return _iT; }
            set { _iT = value; }
        }

        private Element<double> _dE;
        public Element<double> dE
        {
            get { return _dE; }
            set { _dE = value; }
        }
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


    }
}
