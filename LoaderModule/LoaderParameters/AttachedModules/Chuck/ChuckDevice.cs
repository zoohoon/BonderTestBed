using LogModule;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using ProberInterfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace LoaderParameters
{
    /// <summary>
    /// Chuck의 디바이스를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    [KnownType(typeof(IElement))]
    public class ChuckDevice : INotifyPropertyChanged, ICloneable, IParamNode
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

        private Element<string> _Label = new Element<string>();
        /// <summary>
        /// Label를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> Label
        {
            get { return _Label; }
            set { _Label = value; RaisePropertyChanged(); }
        }

        private Element<double> _LoadingNotchAngle = new Element<double>();
        /// <summary>
        /// Chuck에 적재 시 Notch의 각도를 가져오거나 설정합니다. (단위 degree)
        /// </summary>
        [DataMember]
        public Element<double> LoadingNotchAngle
        {
            get { return _LoadingNotchAngle; }
            set { _LoadingNotchAngle = value; RaisePropertyChanged(); }
        }

        public ChuckDevice()
        {
            _Label.Value = string.Empty;
        }

        /// <summary>
        /// 정의하는 모듈의 타입을 가져옵니다.
        /// </summary>
        /// <returns>모듈 타입</returns>
        public ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.CHUCK;
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
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
