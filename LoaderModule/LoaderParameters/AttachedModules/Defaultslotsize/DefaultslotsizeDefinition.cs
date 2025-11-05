using LogModule;
using Newtonsoft.Json;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace LoaderParameters.AttachedModules.Defaultslotsize
{

    [DataContract]
    [Serializable]
    public class DefaultslotsizeDefinition : INotifyPropertyChanged, ICloneable, IParamNode
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

        /// <summary>
        /// 속성값이 변경되면 발생합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Element<double> _Slotsize6inch = new Element<double>();
        /// <summary>
        /// Standard 6inch cassette slot size.
        /// </summary>
        [DataMember]
        public Element<double> SlotSize6inch
        {
            get { return _Slotsize6inch; }
            set { _Slotsize6inch = value; RaisePropertyChanged(); }
        }

        private Element<double> _Slotsize6inchTol = new Element<double>();
        /// <summary>
        /// User can change the 6inch cassette slot size within tolerance.
        /// </summary>
        [DataMember]
        public Element<double> SlotSize6inchTolerance
        {
            get { return _Slotsize6inchTol; }
            set { _Slotsize6inchTol = value; RaisePropertyChanged(); }
        }

        private Element<double> _Slotsize8inch = new Element<double>();
        /// <summary>
        /// Standard 8inch cassette slot size.
        /// </summary>
        [DataMember]
        public Element<double> SlotSize8inch
        {
            get { return _Slotsize8inch; }
            set { _Slotsize8inch = value; RaisePropertyChanged(); }
        }

        private Element<double> _Slotsize8inchTol = new Element<double>();
        /// <summary>
        /// User can change the 8inch cassette slot size within tolerance.
        /// </summary>
        [DataMember]
        public Element<double> SlotSize8inchTolerance
        {
            get { return _Slotsize8inchTol; }
            set { _Slotsize8inchTol = value; RaisePropertyChanged(); }
        }

        private Element<double> _Slotsize12inch = new Element<double>();
        /// <summary>
        /// Standard 12inch cassette slot size
        /// </summary>
        [DataMember]
        public Element<double> SlotSize12inch
        {
            get { return _Slotsize12inch; }
            set { _Slotsize12inch = value; RaisePropertyChanged(); }
        }

        private Element<double> _Slotsize12inchTol = new Element<double>();
        /// <summary>
        /// User can change the 12inch cassette slot size within tolerance.
        /// </summary>
        [DataMember]
        public Element<double> SlotSize12inchTolerance
        {
            get { return _Slotsize12inchTol; }
            set { _Slotsize12inchTol = value; RaisePropertyChanged(); }
        }
        public object Clone()
        {
            try
            {
                var shallowClone = MemberwiseClone() as DefaultslotsizeDefinition;

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
