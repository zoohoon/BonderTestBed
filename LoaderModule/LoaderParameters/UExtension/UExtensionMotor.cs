using LogModule;
using System;
using System.Collections.Generic;

using ProberInterfaces;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace LoaderParameters
{
    /// <summary>
    /// UExtensionMotor 을 정의합니다.
    /// </summary>
    ///
    [Serializable]
    [DataContract]
    public class UExtensionMotor : UExtensionBase
    {
        [XmlIgnore, JsonIgnore]
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
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
        public new List<object> Nodes { get; set; }

        /// <summary>
        /// 인스턴스를 생성합니다.
        /// </summary>
        public UExtensionMotor()
        {
            UExtensionType.Value = UExtensionTypeEnum.MOTOR;
        }

        private Element<EnumAxisConstants> _AxisType = new Element<EnumAxisConstants>();
        /// <summary>
        /// AxisType 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<EnumAxisConstants> AxisType
        {
            get { return _AxisType; }
            set { _AxisType = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public override object Clone()
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
