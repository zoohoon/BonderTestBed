using LogModule;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using ProberInterfaces;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace LoaderParameters
{
    /// <summary>
    /// ARM의 용도
    /// </summary>
    [DataContract]
    [Flags]
    public enum ARMUseTypeEnum
    {
        /// <summary>
        /// 사용 안함.
        /// </summary>
        [EnumMember]
        NONE = 0,
        /// <summary>
        /// LOADING 목적으로만 사용
        /// </summary>
        [EnumMember]
        LOADING = 1 << 0,
        /// <summary>
        /// UNLOADING 목적으로만 사용
        /// </summary>
        [EnumMember]
        UNLOADING = 1 << 1,
        /// <summary>
        /// Loading, Unloading 모두 사용
        /// </summary>
        [EnumMember]
        BOTH = int.MaxValue,
    }

    /// <summary>
    /// ARM의 특성을 정의합니다. 
    /// </summary>
    [Serializable]
    [DataContract]
    public class ARMDefinition : INotifyPropertyChanged, ICloneable, IParamNode
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

        private Element<EnumAxisConstants> _AxisType = new Element<EnumAxisConstants>();
        /// <summary>
        /// ARM의 AxisType을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<EnumAxisConstants> AxisType
        {
            get { return _AxisType; }
            set { _AxisType = value; RaisePropertyChanged(); }
        }

        private Element<double> _EndOffset = new Element<double>();
        /// <summary>
        /// ARM이 타겟 위치로 접근할 때 더해지는 Offset 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> EndOffset
        {
            get { return _EndOffset; }
            set { _EndOffset = value; RaisePropertyChanged(); }
        }

        private Element<double> _UpOffset = new Element<double>();
        /// <summary>
        /// 타겟의 A 위치를 설정할 때 기준이 되는 ARM에 더해지는 Offset 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> UpOffset
        {
            get { return _UpOffset; }
            set { _UpOffset = value; RaisePropertyChanged(); }
        }
                
        private Element<ARMUseTypeEnum> _UseType = new Element<ARMUseTypeEnum>();
        /// <summary>
        /// ARM의 용도를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<ARMUseTypeEnum> UseType
        {
            get { return _UseType; }
            set { _UseType = value; RaisePropertyChanged(); }
        }

        private Element<long> _IOCheckMaintainTime = new Element<long>();
        /// <summary>
        /// IO의 값을 확인 할 때 유지되는 시간을 가져오거나 설정합니다. (단위 ms)
        /// </summary>
        [DataMember]
        public Element<long> IOCheckMaintainTime
        {
            get { return _IOCheckMaintainTime; }
            set { _IOCheckMaintainTime = value; RaisePropertyChanged(); }
        }

        private Element<long> _IOCheckDelayTimeout = new Element<long>();
        /// <summary>
        /// IO의 값을 확인 할 때 Timeout이 발생하는 시간을 가져오거나 설정합니다. (단위 ms)
        /// </summary>
        [DataMember]
        public Element<long> IOCheckDelayTimeout
        {
            get { return _IOCheckDelayTimeout; }
            set { _IOCheckDelayTimeout = value; RaisePropertyChanged(); }
        }

        private Element<long> _IOWaitTimeout = new Element<long>();
        /// <summary>
        /// IO의 값을 기다릴 때 Timeout이 발생하는 시간을 가져오거나 설정합니다. (단위 ms)
        /// </summary>
        [DataMember]
        public Element<long> IOWaitTimeout
        {
            get { return _IOWaitTimeout; }
            set { _IOWaitTimeout = value; RaisePropertyChanged(); }
        }

        private Element<string> _DIWAFERONMODULE = new Element<string>();
        /// <summary>
        /// Wafer가 있는 지 검사하는 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DIWAFERONMODULE
        {
            get { return _DIWAFERONMODULE; }
            set { _DIWAFERONMODULE = value; RaisePropertyChanged(); }
        }

        private Element<string> _DOAIRON = new Element<string>();
        /// <summary>
        /// Air를 켜거나 끄는 Output Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DOAIRON
        {
            get { return _DOAIRON; }
            set { _DOAIRON = value; RaisePropertyChanged(); }
        }

        public ARMDefinition()
        {
            try
            {
            _IOCheckMaintainTime.Value= 100;
            _IOCheckDelayTimeout.Value = 500;
            _IOWaitTimeout.Value = 1000;
            _DIWAFERONMODULE.Value = "";
            _DOAIRON.Value = "";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        /// <summary>
        /// 정의하는 모듈의 타입을 가져옵니다.
        /// </summary>
        /// <returns>모듈 타입</returns>
        public ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.ARM;
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
