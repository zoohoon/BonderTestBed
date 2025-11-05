using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace ProberInterfaces
{
    /// <summary>
    /// OCR의  기본 디바이스를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public abstract class OCRDeviceBase : INotifyPropertyChanged, ICloneable, IParamNode
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

        #region Axes Offset
        private Element<double> _OffsetU = new Element<double>();
        /// <summary>
        /// Offset U 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OffsetU
        {
            get { return _OffsetU; }
            set { _OffsetU = value; RaisePropertyChanged(); }
        }
        
        private Element<double> _OffsetW = new Element<double>();
        /// <summary>
        /// Offset W 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OffsetW
        {
            get { return _OffsetW; }
            set { _OffsetW = value; RaisePropertyChanged(); }
        }

        private Element<double> _OffsetV = new Element<double>();
        /// <summary>
        /// Offset V 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OffsetV
        {
            get { return _OffsetV; }
            set { _OffsetV = value; RaisePropertyChanged(); }
        }
        #endregion

        public OCRDeviceBase()
        {
            _Label.Value = string.Empty;
        }

        /// <summary>
        /// 정의하는 모듈의 타입을 가져옵니다.
        /// </summary>
        /// <returns>모듈 타입</returns>
        public abstract ModuleTypeEnum GetModuleType();

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public abstract object Clone();
    }
}
