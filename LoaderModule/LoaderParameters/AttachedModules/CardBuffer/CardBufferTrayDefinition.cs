using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

using ProberInterfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace LoaderParameters
{
    /// <summary>
    /// CardBufferTray의 특성을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class CardBufferTrayDefinition : INotifyPropertyChanged, ICloneable, IParamNode
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


        private Element<string> _DICARDONMODULE = new Element<string>();
        /// <summary>
        /// Wafer가 있는 지 검사하는 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DICARDONMODULE
        {
            get { return _DICARDONMODULE; }
            set { _DICARDONMODULE = value; RaisePropertyChanged(); }
        }

        private Element<string> _DIDRAWERSENSOR = new Element<string>();
        /// <summary>
        /// CARD DRAWER Senor  있는 지 검사하는 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DIDRAWERSENSOR
        {
            get { return _DIDRAWERSENSOR; }
            set { _DIDRAWERSENSOR = value; RaisePropertyChanged(); }
        }

        private Element<string> _DICARDONMODULE_DOWN = new Element<string>();
        /// <summary>
        /// Wafer가 있는 지 검사하는 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DICARDONMODULE_DOWN
        {
            get { return _DICARDONMODULE_DOWN; }
            set { _DICARDONMODULE_DOWN = value; RaisePropertyChanged(); }
        }
        private Element<string> _DICARDATTACHHOLDER = new Element<string>();
        /// <summary>
        /// Card Tray 에 Card Holder 위 Card가 있는 지 검사하는 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DICARDATTACHHOLDER
        {
            get { return _DICARDATTACHHOLDER; }
            set { _DICARDATTACHHOLDER = value; RaisePropertyChanged(); }
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

        private ObservableCollection<CardBufferTrayAccessParam> _AccessParams;
        /// <summary>
        /// CardBufferTray에 Access하기 위한 파라미터를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CardBufferTrayAccessParam> AccessParams
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

        public CardBufferTrayDefinition()
        {
            _DICARDONMODULE.Value = string.Empty;
            _IOCheckDelayTimeout.Value = 500;
            _AccessParams = new ObservableCollection<CardBufferTrayAccessParam>();
        }

        /// <summary>
        /// 정의하는 모듈의 타입을 가져옵니다.
        /// </summary>
        /// <returns>모듈 타입</returns>
        public ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.CARDBUFFER;
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            var shallowClone = MemberwiseClone() as CardBufferTrayDefinition;

            shallowClone.AccessParams = AccessParams.CloneFrom();

            return shallowClone;
        }
    }

    /// <summary>
    /// CardBufferTray에 Access하기위한 파라미터를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class CardBufferTrayAccessParam : INotifyPropertyChanged, ICloneable, IParamNode
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

        public CardBufferTrayAccessParam()
        {
            _Position = new LoaderCoordinate();
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            var shallowClone = MemberwiseClone() as CardBufferTrayAccessParam;

            shallowClone.Position = Position.Clone<LoaderCoordinate>();

            return shallowClone;
        }
    }




    [DataContract]
    [Serializable]
    public class CardBufferDefinition : INotifyPropertyChanged, ICloneable, IParamNode
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


        private Element<string> _DICARDONMODULE = new Element<string>();
        /// <summary>
        /// Wafer가 있는 지 검사하는 Input Description을 가져오거나 설정합니다.
        /// MPT에서 홀더만 체크
        /// </summary>
        [DataMember]
        public Element<string> DICARDONMODULE
        {
            get { return _DICARDONMODULE; }
            set { _DICARDONMODULE = value; RaisePropertyChanged(); }
        }
        private Element<string> _DICARRIERVAC = new Element<string>();
        /// <summary>
        /// CARD DRAWER Senor  있는 지 검사하는 Input Description을 가져오거나 설정합니다.
        /// MPT 캐리어 체크 : 흡착 구멍이 뚫려 있어서 홀더가 올라가면 true가 되는 구조. 그러므로 carrier만 있는 지는 확인할수없다. 
        /// </summary>v
        [DataMember]
        public Element<string> DICARRIERVAC
        {
            get { return _DICARRIERVAC; }
            set { _DICARRIERVAC = value; RaisePropertyChanged(); }
        }

        private Element<string> _DICARDATTACHMODULE = new Element<string>();
        /// <summary>
        /// CARD DRAWER Senor  있는 지 검사하는 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DICARDATTACHMODULE
        {
            get { return _DICARDATTACHMODULE; }
            set { _DICARDATTACHMODULE = value; RaisePropertyChanged(); }
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

        private ObservableCollection<CardBufferAccessParam> _AccessParams;
        /// <summary>
        /// CardBufferTray에 Access하기 위한 파라미터를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CardBufferAccessParam> AccessParams
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

        public CardBufferDefinition()
        {
            _DICARDONMODULE.Value = string.Empty;
            _IOCheckDelayTimeout.Value = 500;
            _AccessParams = new ObservableCollection<CardBufferAccessParam>();
        }

        /// <summary>
        /// 정의하는 모듈의 타입을 가져옵니다.
        /// </summary>
        /// <returns>모듈 타입</returns>
        public ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.CARDBUFFER;
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            var shallowClone = MemberwiseClone() as CardBufferDefinition;

            shallowClone.AccessParams = AccessParams.CloneFrom();

            return shallowClone;
        }
    }

    /// <summary>
    /// CardBufferTray에 Access하기위한 파라미터를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class CardBufferAccessParam : INotifyPropertyChanged, ICloneable, IParamNode
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

        public CardBufferAccessParam()
        {
            _Position = new LoaderCoordinate();
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            var shallowClone = MemberwiseClone() as CardBufferTrayAccessParam;

            shallowClone.Position = Position.Clone<LoaderCoordinate>();

            return shallowClone;
        }
    }
}
