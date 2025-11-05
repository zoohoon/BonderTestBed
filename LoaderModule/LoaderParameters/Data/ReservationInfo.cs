using LogModule;
using Newtonsoft.Json;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace LoaderParameters.Data
{
    [DataContract]
    public class ReservationInfo : INotifyPropertyChanged, ICloneable
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
        private ModuleID _TargetModule;
     
        [DataMember]
        public ModuleID TargetModule
        {
            get { return _TargetModule; }
            set { _TargetModule = value; RaisePropertyChanged(); }
        }

        private EnumReservationState _ReservationState;
        [DataMember]
        public EnumReservationState ReservationState
        {
            get { return _ReservationState; }
            set { _ReservationState = value; RaisePropertyChanged(); }
        }
        public object Clone()
        {
          
            var shallowClone = MemberwiseClone() as ReservationInfo;
            try
            {

                shallowClone.TargetModule = TargetModule;
                shallowClone.ReservationState = ReservationState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return shallowClone;
        }
    }
}
