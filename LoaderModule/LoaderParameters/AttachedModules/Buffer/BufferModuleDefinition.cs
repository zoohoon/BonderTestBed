using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

using ProberInterfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using ProberInterfaces.Foup;
using LogModule;

namespace LoaderParameters
{
    /// <summary>
    /// Buffer의 특성을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class BufferDefinition : INotifyPropertyChanged, ICloneable, IParamNode
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

        private ObservableCollection<BufferAccessParam> _AccessParams;
        /// <summary>
        /// Buffer에 Access하기 위한 파라미터를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<BufferAccessParam> AccessParams
        {
            get { return _AccessParams; }
            set { _AccessParams = value; RaisePropertyChanged(); }
        }


        private Element<bool> _Enable = new Element<bool>();
        /// <summary>
        /// Wafer가 있는 지 검사하는 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> Enable
        {
            get { return _Enable; }
            set { _Enable = value; RaisePropertyChanged(); }
        }


      

        public BufferDefinition()
        {
            _DIWAFERONMODULE.Value = string.Empty;
            _DOAIRON.Value = string.Empty;
            _IOCheckDelayTimeout.Value = 500;
            _Enable.Value = true;
            _AccessParams = new ObservableCollection<BufferAccessParam>();
        }

        /// <summary>
        /// 정의하는 모듈의 타입을 가져옵니다.
        /// </summary>
        /// <returns>모듈 타입</returns>
        public ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.BUFFER;
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            var shallowClone = MemberwiseClone() as BufferDefinition;

            shallowClone.AccessParams = AccessParams.CloneFrom();

            return shallowClone;
        }
    }

    /// <summary>
    /// Buffer에 Access하기위한 파라미터를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class BufferAccessParam : INotifyPropertyChanged, ICloneable, IParamNode
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

        private Element<SubstrateTypeEnum> _SubstrateType = new Element<SubstrateTypeEnum>();
        /// <summary>
        /// 이송 오브젝트의 타입을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<SubstrateTypeEnum> SubstrateType
        {
            get { return _SubstrateType; }
            set { _SubstrateType = value; RaisePropertyChanged(); }
        }

        private Element<SubstrateSizeEnum> _SubstrateSize = new Element<SubstrateSizeEnum>();
        /// <summary>
        /// 이송 오브젝트의 사이즈를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<SubstrateSizeEnum> SubstrateSize
        {
            get { return _SubstrateSize; }
            set { _SubstrateSize = value; RaisePropertyChanged(); }
        }


        private Element<CassetteTypeEnum> _CassetteType = new Element<CassetteTypeEnum>();
        /// <summary>
        /// 이송 오브젝트가 13Slot 인지 사이즈를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<CassetteTypeEnum> CassetteType
        {
            get { return _CassetteType; }
            set { _CassetteType = value; RaisePropertyChanged(); }
        }

        private LoaderCoordinate _Position;
        /// <summary>
        /// Chuck의 Pickup 위치를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public LoaderCoordinate Position
        {
            get { return _Position; }
            set { _Position = value; RaisePropertyChanged(); }
        }

        private Element<double> _PickupIncrement = new Element<double>();
        /// <summary>
        /// PickupIncrement를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> PickupIncrement
        {
            get { return _PickupIncrement; }
            set { _PickupIncrement = value; RaisePropertyChanged(); }
        }

        public BufferAccessParam()
        {
            _Position = new LoaderCoordinate();
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            var shallowClone = MemberwiseClone() as BufferAccessParam;

            shallowClone.Position = Position.Clone<LoaderCoordinate>();

            return shallowClone;
        }

        public object DeepClone()
        {
            var deepClone = new BufferAccessParam();
            try
            {

                deepClone = MemberwiseClone() as BufferAccessParam;
                deepClone.Position = (LoaderCoordinate)Position.DeepClone();

                var obj_enum = new Element<CassetteTypeEnum>();
                this.CassetteType.CopyTo(obj_enum);
                deepClone.CassetteType = obj_enum;

                var obj = new Element<double>();
                this.PickupIncrement.CopyTo(obj);
                deepClone.PickupIncrement = obj;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return deepClone;
        }
    }
}
