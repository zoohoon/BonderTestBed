using LogModule;
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

namespace LoaderParameters
{
    /// <summary>
    /// Cassette의 특성을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class CassetteDefinition : INotifyPropertyChanged, ICloneable, IParamNode
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

        private Element<ModuleTypeEnum> _ScanModuleType = new Element<ModuleTypeEnum>();
        /// <summary>
        /// 스캔에 사용되는 모듈 타입을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<ModuleTypeEnum> ScanModuleType
        {
            get { return _ScanModuleType; }
            set { _ScanModuleType = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<SlotDefinition> _SlotModules;
        /// <summary>
        /// 카세트의 슬롯을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<SlotDefinition> SlotModules
        {
            get { return _SlotModules; }
            set { _SlotModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<CassetteSlot1AccessParam> _Slot1AccessParams;
        /// <summary>
        /// 카세트의 슬롯1의 위치 파라미터를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CassetteSlot1AccessParam> Slot1AccessParams
        {
            get { return _Slot1AccessParams; }
            set { _Slot1AccessParams = value; RaisePropertyChanged(); }
        }

        private Element<bool> _Enable = new Element<bool>() { Value = true};
        [DataMember]
        public Element<bool> Enable
        {
            get { return _Enable; }
            set { _Enable = value; RaisePropertyChanged(); }
        }


        public CassetteDefinition()
        {
            try
            {
                _SlotModules = new ObservableCollection<SlotDefinition>();
                _Slot1AccessParams = new ObservableCollection<CassetteSlot1AccessParam>();
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
            return ModuleTypeEnum.CST;
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            try
            {

            var shallowClone = MemberwiseClone() as CassetteDefinition;
                shallowClone.Slot1AccessParams = Slot1AccessParams.CloneFrom();
                return shallowClone;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    /// <summary>
    /// Slot의 특성을 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class SlotDefinition : INotifyPropertyChanged, ICloneable
    {
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
        /// 정의하는 모듈의 타입을 가져옵니다.
        /// </summary>
        /// <returns>모듈 타입</returns>
        public ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.SLOT;
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

    [Serializable]
    [DataContract]
    public class CassetteSlot1AccessParam : INotifyPropertyChanged, ICloneable, IParamNode
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Element<SubstrateTypeEnum> _SubstrateType = new Element<SubstrateTypeEnum>();
        [DataMember]
        public Element<SubstrateTypeEnum> SubstrateType
        {
            get { return _SubstrateType; }
            set { _SubstrateType = value; RaisePropertyChanged(); }
        }

        private Element<SubstrateSizeEnum> _SubstrateSize = new Element<SubstrateSizeEnum>();
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
        [DataMember]
        public LoaderCoordinate Position
        {
            get { return _Position; }
            set { _Position = value; RaisePropertyChanged(); }
        }

        private Element<double> _UStopPosOffset = new Element<double>();
        [DataMember]
        public Element<double> UStopPosOffset
        {
            get { return _UStopPosOffset; }
            set { _UStopPosOffset = value; RaisePropertyChanged(); }
        }

        private Element<double> _PickupIncrement = new Element<double>();
        [DataMember]
        public Element<double> PickupIncrement
        {
            get { return _PickupIncrement; }
            set { _PickupIncrement = value; RaisePropertyChanged(); }
        }

        public CassetteSlot1AccessParam()
        {
            try
            {
                _Position = new LoaderCoordinate();
                _UStopPosOffset.Value = 15000;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object Clone()
        {
            try
            {

                var shallowClone = MemberwiseClone() as CassetteSlot1AccessParam;
                shallowClone.Position = Position.Clone<LoaderCoordinate>();
                return shallowClone;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object DeepClone()
        {
            var deepClone = new CassetteSlot1AccessParam();
            try
            {

                deepClone = MemberwiseClone() as CassetteSlot1AccessParam;
                deepClone.Position = (LoaderCoordinate)Position.DeepClone();

                var obj_enum = new Element<CassetteTypeEnum>();
                this.CassetteType.CopyTo(obj_enum);
                deepClone.CassetteType = obj_enum;

                var obj = new Element<double>();
                this.PickupIncrement.CopyTo(obj);
                deepClone.PickupIncrement = obj;

                obj = new Element<double>();
                this.UStopPosOffset.CopyTo(obj);
                deepClone.UStopPosOffset = obj;
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return deepClone;
        }
    }
}
