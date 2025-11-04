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
    /// SCANSENSOR의 특성을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ScanSensorDefinition : INotifyPropertyChanged, ICloneable, IParamNode
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

        private Element<string> _DOSCAN_SENSOR_OUT = new Element<string>();
        /// <summary>
        ///  DOSCAN_SENSOR_OUT 의 Output Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DOSCAN_SENSOR_OUT
        {
            get { return _DOSCAN_SENSOR_OUT; }
            set { _DOSCAN_SENSOR_OUT = value; RaisePropertyChanged(); }
        }

        private Element<string> _DISCAN_SENSOR_OUT = new Element<string>();
        /// <summary>
        /// DISCAN_SENSOR_OUT 의 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DISCAN_SENSOR_OUT
        {
            get { return _DISCAN_SENSOR_OUT; }
            set { _DISCAN_SENSOR_OUT = value; RaisePropertyChanged(); }
        }

        private Element<string> _DISCAN_SENSOR_IN = new Element<string>();
        /// <summary>
        /// DISCAN_SENSOR_IN 의 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DISCAN_SENSOR_IN
        {
            get { return _DISCAN_SENSOR_IN; }
            set { _DISCAN_SENSOR_IN = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ScanSensorParam> _ScanParams;
        /// <summary>
        /// ScanParams 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<ScanSensorParam> ScanParams
        {
            get { return _ScanParams; }
            set { _ScanParams = value; RaisePropertyChanged(); }
        }

        public ScanSensorDefinition()
        {
            try
            {
            _IOCheckDelayTimeout.Value = 500;
            _IOWaitTimeout.Value = 60000;
            _DOSCAN_SENSOR_OUT.Value = "";
            _DISCAN_SENSOR_OUT.Value = "";
            _DISCAN_SENSOR_IN.Value = "";
            _ScanParams = new ObservableCollection<ScanSensorParam>();
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
            return ModuleTypeEnum.SCANSENSOR;
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {

            try
            {
            var shallowClone = MemberwiseClone() as ScanSensorDefinition;
                shallowClone.ScanParams = ScanParams.CloneFrom();
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
    /// ScanSensorParam 을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ScanSensorParam : INotifyPropertyChanged, ICloneable, IParamNode
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
        /// SubstrateType 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<SubstrateTypeEnum> SubstrateType
        {
            get { return _SubstrateType; }
            set { _SubstrateType = value; RaisePropertyChanged(); }
        }

        private Element<SubstrateSizeEnum> _SubstrateSize = new Element<SubstrateSizeEnum>();
        /// <summary>
        /// SubstrateSize 을 가져오거나 설정합니다.
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


        private Element<double> _SensorOffset = new Element<double>();
        /// <summary>
        /// SensorOffset 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> SensorOffset
        {
            get { return _SensorOffset; }
            set { _SensorOffset = value; RaisePropertyChanged(); }
        }

        private Element<double> _UpOffset = new Element<double>();
        /// <summary>
        /// UpOffset 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> UpOffset 
        {
            get { return _UpOffset; }
            set { _UpOffset = value; RaisePropertyChanged(); }
        }

        private Element<double> _DownOffset = new Element<double>();
        /// <summary>
        /// DownOffset 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> DownOffset 
        {
            get { return _DownOffset; }
            set { _DownOffset = value; RaisePropertyChanged(); }
        }

        private Element<double> _InSlotLowerRatio = new Element<double>();
        /// <summary>
        /// InSlotLowerRatio 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> InSlotLowerRatio 
        {
            get { return _InSlotLowerRatio; }
            set { _InSlotLowerRatio = value; RaisePropertyChanged(); }
        }

        private Element<double> _InSlotUpperRatio = new Element<double>();
        /// <summary>
        /// InSlotUpperRatio 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> InSlotUpperRatio 
        {
            get { return _InSlotUpperRatio; }
            set { _InSlotUpperRatio = value; RaisePropertyChanged(); }
        }

        private Element<double> _ThicknessTol = new Element<double>();
        /// <summary>
        /// ThicknessTol 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> ThicknessTol 
        {
            get { return _ThicknessTol; }
            set { _ThicknessTol = value; RaisePropertyChanged(); }
        }

        private Element<EnumAxisConstants> _ScanAxis = new Element<EnumAxisConstants>();
        /// <summary>
        /// ScanAxis 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<EnumAxisConstants> ScanAxis 
        {
            get { return _ScanAxis; }
            set { _ScanAxis = value; RaisePropertyChanged(); }
        }

        private Element<EnumMotorDedicatedIn> _SensorInputPort = new Element<EnumMotorDedicatedIn>();
        /// <summary>
        /// ScanAxis 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<EnumMotorDedicatedIn> SensorInputPort
        {
            get { return _SensorInputPort; }
            set { _SensorInputPort = value; RaisePropertyChanged(); }
        }

        private Element<EnumMotorDedicatedIn> _SensorDownInputPort = new Element<EnumMotorDedicatedIn>();
        /// <summary>
        /// ScanAxis 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<EnumMotorDedicatedIn> SensorDownInputPort
        {
            get { return _SensorDownInputPort; }
            set { _SensorDownInputPort = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            try
            {
            var shallowClone = MemberwiseClone() as ScanSensorParam;
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
            var deepClone = new ScanSensorParam();
            try
            {

                deepClone = MemberwiseClone() as ScanSensorParam;

                var obj = new Element<double>();
                this.SensorOffset.CopyTo(obj);
                deepClone.SensorOffset = obj;

                obj = new Element<double>();
                this.UpOffset.CopyTo(obj);
                deepClone.UpOffset = obj;

                obj = new Element<double>();
                this.DownOffset.CopyTo(obj);
                deepClone.DownOffset = obj;

                obj = new Element<double>();
                this.InSlotLowerRatio.CopyTo(obj);
                deepClone.InSlotLowerRatio = obj;

                obj = new Element<double>();
                this.InSlotUpperRatio.CopyTo(obj);
                deepClone.InSlotUpperRatio = obj;

                obj = new Element<double>();
                this.ThicknessTol.CopyTo(obj);
                deepClone.ThicknessTol = obj;

                var obj_enum = new Element<EnumAxisConstants>();
                this.ScanAxis.CopyTo(obj_enum);
                deepClone.ScanAxis = obj_enum;

                var obj_enum_1 = new Element<EnumMotorDedicatedIn>();
                this.SensorDownInputPort.CopyTo(obj_enum_1);
                deepClone.SensorDownInputPort = obj_enum_1;

                var obj_enum_2 = new Element<EnumMotorDedicatedIn>();
                this.SensorInputPort.CopyTo(obj_enum_2);
                deepClone.SensorInputPort = obj_enum_2;

                var obj_enum_3 = new Element<CassetteTypeEnum>();
                this.CassetteType.CopyTo(obj_enum_3);
                deepClone.CassetteType = obj_enum_3;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return deepClone;
        }
    }
    
}
