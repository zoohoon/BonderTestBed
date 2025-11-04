using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

using ProberInterfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using LogModule;
using Newtonsoft.Json;
using ProberInterfaces.Foup;

namespace LoaderParameters
{
    /// <summary>
    /// InspectionTray의 특성을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class InspectionTrayDefinition : INotifyPropertyChanged, ICloneable, IParamNode
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
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
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

        private Element<string> _DI6INCHWAFERONMODULE = new Element<string>();
        /// <summary>
        /// 6inch Wafer가 있는 지 검사하는 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DI6INCHWAFERONMODULE
        {
            get { return _DI6INCHWAFERONMODULE; }
            set { _DI6INCHWAFERONMODULE = value; RaisePropertyChanged(); }
        }

        private Element<string> _DI8INCHWAFERONMODULE = new Element<string>();
        /// <summary>
        /// 8inch Wafer가 있는 지 검사하는 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DI8INCHWAFERONMODULE
        {
            get { return _DI8INCHWAFERONMODULE; }
            set { _DI8INCHWAFERONMODULE = value; RaisePropertyChanged(); }
        }

        private Element<string> _DIOPENDED = new Element<string>();
        /// <summary>
        /// Inspection Tray가 열려 있는 지 검사하는 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DIOPENDED
        {
            get { return _DIOPENDED; }
            set { _DIOPENDED = value; RaisePropertyChanged(); }
        }

        private Element<string> _DIMOVED = new Element<string>();
        /// <summary>
        /// Inspection Tray가 Moved인지 검사하는 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DIMOVED
        {
            get { return _DIMOVED; }
            set { _DIMOVED = value; RaisePropertyChanged(); }
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

        private Element<bool> _IsInterferenceWithCassettePort = new Element<bool>();
        /// <summary>
        /// InspectionTray에 접근할 때 카세트 포트의 커버와 간섭이 발생하는 지 여부를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> IsInterferenceWithCassettePort
        {
            get { return _IsInterferenceWithCassettePort; }
            set { _IsInterferenceWithCassettePort = value; RaisePropertyChanged(); }
        }

        private Element<int> _InterferenceCassettePortNum = new Element<int>();
        /// <summary>
        /// InspectionTray에 접근할 때 간섭이 발생되는 카세트 포트의 번호를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> InterferenceCassettePortNum
        {
            get { return _InterferenceCassettePortNum; }
            set { _InterferenceCassettePortNum = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<InspectionTrayAccessParam> _AccessParams;
        /// <summary>
        /// InspectionTray에 Access하기 위한 파라미터를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<InspectionTrayAccessParam> AccessParams
        {
            get { return _AccessParams; }
            set { _AccessParams = value; RaisePropertyChanged(); }
        }

        public InspectionTrayDefinition()
        {
            try
            {
                _DIWAFERONMODULE.Value = string.Empty;
                _DIOPENDED.Value = string.Empty;
                _DIMOVED.Value = string.Empty;
                _IOCheckDelayTimeout.Value = 500;
                _AccessParams = new ObservableCollection<InspectionTrayAccessParam>();
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
            return ModuleTypeEnum.INSPECTIONTRAY;
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            try
            {
                var shallowClone = MemberwiseClone() as InspectionTrayDefinition;

                shallowClone.AccessParams = AccessParams.CloneFrom();

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
    /// InspectionTray에 Access하기위한 파라미터를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class InspectionTrayAccessParam : INotifyPropertyChanged, ICloneable, IParamNode
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
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
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

        public InspectionTrayAccessParam()
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
                var shallowClone = MemberwiseClone() as InspectionTrayAccessParam;
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
            var deepClone = new InspectionTrayAccessParam();
            try
            {

                deepClone = MemberwiseClone() as InspectionTrayAccessParam;
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
