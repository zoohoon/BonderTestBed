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
    /// Chuck의 특성을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ChuckDefinition : INotifyPropertyChanged, ICloneable, IParamNode
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

        private ObservableCollection<ChuckAccessParam> _AccessParams;
        /// <summary>
        /// Chuck에 Access하기 위한 파라미터를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<ChuckAccessParam> AccessParams
        {
            get { return _AccessParams; }
            set { _AccessParams = value; RaisePropertyChanged(); }
        }

        public ChuckDefinition()
        {
            _AccessParams = new ObservableCollection<ChuckAccessParam>();
        }
        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            try
            {

            var shallowClone = MemberwiseClone() as ChuckDefinition;
                shallowClone.AccessParams = AccessParams.CloneFrom();
                return shallowClone;

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
            return ModuleTypeEnum.CHUCK;
        }
    }

    /// <summary>
    /// Chuck에 Access하기위한 파라미터를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ChuckAccessParam : INotifyPropertyChanged, ICloneable, IParamNode
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

        public ChuckAccessParam()
        {
            _Position = new LoaderCoordinate();
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            try
            {

                var shallowClone = MemberwiseClone() as ChuckAccessParam;
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
            var deepClone = new ChuckAccessParam();
            try
            {

                deepClone = MemberwiseClone() as ChuckAccessParam;
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
