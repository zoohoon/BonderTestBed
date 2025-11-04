using Newtonsoft.Json;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace LoaderParameters
{
    /// <summary>
    /// UExtensionType 을 정의합니다.
    /// </summary>
    [DataContract]
    public enum UExtensionTypeEnum
    {
        /// <summary>
        /// NONE
        /// </summary>
        [EnumMember]
        NONE,
        /// <summary>
        /// MOTOR
        /// </summary>
        [EnumMember]
        MOTOR,
        /// <summary>
        /// CYLINDER
        /// </summary>
        [EnumMember]
        CYLINDER,
    }

    /// <summary>
    /// UExtensionBase 을 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    [KnownType(typeof(UExtensionNone))]
    [KnownType(typeof(UExtensionCylinder))]
    [KnownType(typeof(UExtensionMotor))]
    public abstract class UExtensionBase : INotifyPropertyChanged, ICloneable, IParamNode
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

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public abstract object Clone();

        private Element<UExtensionTypeEnum> _UExtensionType = new Element<UExtensionTypeEnum>();
        /// <summary>
        /// UExtensionType 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<UExtensionTypeEnum> UExtensionType
        {
            get { return _UExtensionType; }
            set { _UExtensionType = value; RaisePropertyChanged(); }
        }
    }
}
