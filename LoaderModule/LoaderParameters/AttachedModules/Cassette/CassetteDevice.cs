using LogModule;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

using ProberInterfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using ProberInterfaces.Enum;

namespace LoaderParameters
{
    /// <summary>
    /// 카세트의 디바이스를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    [KnownType(typeof(IElement))]
    public class CassetteDevice : INotifyPropertyChanged, ICloneable, IParamNode
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

        private Element<double> _SlotSize = new Element<double>();
        /// <summary>
        /// 카세트 슬롯의 사이즈를 가져오거나 설정합니다. (단위 um)
        /// </summary>
        [DataMember]
        public Element<double> SlotSize
        {
            get { return _SlotSize; }
            set { _SlotSize = value; RaisePropertyChanged(); }
        }

        private Element<double> _WaferThickness = new Element<double>();
        /// <summary>
        /// 카세트에서 할당되는 오브젝트의 두께를 가져오거나 설정합니다. (단위 um)
        /// </summary>
        [DataMember]
        public Element<double> WaferThickness
        {
            get { return _WaferThickness; }
            set { _WaferThickness = value; RaisePropertyChanged(); }
        }

        private Element<double> _LoadingNotchAngle = new Element<double>();
        /// <summary>
        /// Cassette에 적재 시 Notch의 각도를 가져오거나 설정합니다. (단위 degree)
        /// </summary>
        [DataMember]
        public Element<double> LoadingNotchAngle
        {
            get { return _LoadingNotchAngle; }
            set { _LoadingNotchAngle = value; RaisePropertyChanged(); }
        }

        private TransferObject _AllocateDeviceInfo;
        /// <summary>
        /// 카세트에서 할당되는 오브젝트의 디바이스 정보를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public TransferObject AllocateDeviceInfo
        {
            get { return _AllocateDeviceInfo; }
            set { _AllocateDeviceInfo = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<SlotDevice> _SlotModules;
        /// <summary>
        /// 카세트의 슬롯의 디바이스들을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<SlotDevice> SlotModules
        {
            get { return _SlotModules; }
            set { _SlotModules = value; RaisePropertyChanged(); }
        }

      

        public CassetteDevice()
        {
            try
            {
                _Label.Value = string.Empty;
                _AllocateDeviceInfo = new TransferObject();
                _SlotModules = new ObservableCollection<SlotDevice>();
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
            var shallowClone = MemberwiseClone() as CassetteDevice;
            try
            {

                shallowClone.AllocateDeviceInfo = AllocateDeviceInfo.Clone<TransferObject>();
                shallowClone.SlotModules = SlotModules.CloneFrom();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return shallowClone;
        }
    }

    /// <summary>
    /// 카세트 슬롯의 디바이를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class SlotDevice : INotifyPropertyChanged, ICloneable, IParamNode
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

        private Element<bool> _IsOverrideEnable = new Element<bool>();
        /// <summary>
        /// 카세트에 설정된 디바이스를 재정의하는 지 여부를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> IsOverrideEnable
        {
            get { return _IsOverrideEnable; }
            set { _IsOverrideEnable = value; RaisePropertyChanged(); }
        }

        private Element<EnumWaferType> _OverrideWaferType = new Element<EnumWaferType>();
        /// <summary>
        /// 재정의되는 웨이퍼의 타입을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<EnumWaferType> OverrideWaferType
        {
            get { return _OverrideWaferType; }
            set { _OverrideWaferType = value; RaisePropertyChanged(); }
        }

        private Element<OCRModeEnum> _OverrideOCRMode = new Element<OCRModeEnum>();
        /// <summary>
        /// 재정의되는 OCR Mode를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<OCRModeEnum> OverrideOCRMode
        {
            get { return _OverrideOCRMode; }
            set { _OverrideOCRMode = value; RaisePropertyChanged(); }
        }

        private Element<OCRTypeEnum> _OverrideOCRType = new Element<OCRTypeEnum>();
        /// <summary>
        /// 재정의되는 OCR Type을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<OCRTypeEnum> OverrideOCRType
        {
            get { return _OverrideOCRType; }
            set { _OverrideOCRType = value; RaisePropertyChanged(); }
        }

        private Element<OCRDirectionEnum> _OverrideOCRDirection = new Element<OCRDirectionEnum>();
        /// <summary>
        /// 재정의되는 OCR 방향을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<OCRDirectionEnum> OverrideOCRDirection
        {
            get { return _OverrideOCRDirection; }
            set { _OverrideOCRDirection = value; RaisePropertyChanged(); }
        }
        public SlotDevice()
        {
            _Label.Value = string.Empty;
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
            var shallowClone = MemberwiseClone();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return shallowClone;
        }
    }

}
