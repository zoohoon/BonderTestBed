using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using LogModule;
using Newtonsoft.Json;

namespace LoaderParameters
{
    /// <summary>
    /// SCANCAMERA의 특성을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ScanCameraDefinition : INotifyPropertyChanged, ICloneable, IParamNode
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

        private ObservableCollection<ScanCameraParam> _ScanParams;
        /// <summary>
        /// 스캔 파라미터를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<ScanCameraParam> ScanParams
        {
            get { return _ScanParams; }
            set { _ScanParams = value; RaisePropertyChanged(); }
        }

        public ScanCameraDefinition()
        {
            _ScanParams = new ObservableCollection<ScanCameraParam>();
        }

        /// <summary>
        /// 정의하는 모듈의 타입을 가져옵니다.
        /// </summary>
        /// <returns>모듈 타입</returns>
        public ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.SCANCAM;
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            try
            {

                var shallowClone = MemberwiseClone() as ScanCameraDefinition;
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
    /// Light의 방향을 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public enum CassetteScanLightDirectionEnum
    {
        /// <summary>
        /// 웨이퍼 정면
        /// </summary>
        [EnumMember]
        FRONT,
        /// <summary>
        /// 웨이퍼 후면
        /// </summary>
        [EnumMember]
        BACK,
    }

    /// <summary>
    /// 스캔 카메라 파라미터를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ScanCameraParam : INotifyPropertyChanged, ICloneable, IParamNode
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

        private Element<int> _CassetteNumber = new Element<int>();
        /// <summary>
        /// CassetteNumber 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> CassetteNumber
        {
            get { return _CassetteNumber; }
            set { _CassetteNumber = value; RaisePropertyChanged(); }
        }

        private Element<SubstrateTypeEnum> _SubstrateType = new Element<SubstrateTypeEnum>();
        /// <summary>
        /// SubstrateType 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<SubstrateTypeEnum> SubstrateType
        {
            get { return _SubstrateType; }
            set { _SubstrateType = value; RaisePropertyChanged(); }
        }

        private Element<SubstrateSizeEnum> _SubstrateSize = new Element<SubstrateSizeEnum>();
        /// <summary>
        /// SubstrateSize 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<SubstrateSizeEnum> SubstrateSize
        {
            get { return _SubstrateSize; }
            set { _SubstrateSize = value; RaisePropertyChanged(); }
        }

        private LoaderCoordinate _Slot1Position;
        /// <summary>
        /// Slot1Position 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public LoaderCoordinate Slot1Position
        {
            get { return _Slot1Position; }
            set { _Slot1Position = value; RaisePropertyChanged(); }
        }

        private Element<double> _UpScanWOffset = new Element<double>();
        /// <summary>
        /// UpScanWOffset 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> UpScanWOffset
        {
            get { return _UpScanWOffset; }
            set { _UpScanWOffset = value; RaisePropertyChanged(); }
        }

        private Element<double> _DownScanWOffset = new Element<double>();
        /// <summary>
        /// DownScanWOffset 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> DownScanWOffset
        {
            get { return _DownScanWOffset; }
            set { _DownScanWOffset = value; RaisePropertyChanged(); }
        }

        private Element<double> _CameraRatio = new Element<double>();
        /// <summary>
        /// CameraRatio 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> CameraRatio
        {
            get { return _CameraRatio; }
            set { _CameraRatio = value; RaisePropertyChanged(); }
        }

        private Element<int> _LightChannel = new Element<int>();
        /// <summary>
        /// LightChannel 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> LightChannel
        {
            get { return _LightChannel; }
            set { _LightChannel = value; RaisePropertyChanged(); }
        }

        private Element<CassetteScanLightDirectionEnum> _LightDirection = new Element<CassetteScanLightDirectionEnum>();
        /// <summary>
        /// LightDirection 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<CassetteScanLightDirectionEnum> LightDirection
        {
            get { return _LightDirection; }
            set { _LightDirection = value; RaisePropertyChanged(); }
        }

        private Element<ushort> _LightIntensity = new Element<ushort>();
        /// <summary>
        /// LightIntensity 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<ushort> LightIntensity
        {
            get { return _LightIntensity; }
            set { _LightIntensity = value; RaisePropertyChanged(); }
        }

        private Element<double> _ROIWidthRatio = new Element<double>();
        /// <summary>
        /// ROIWidthRatio 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> ROIWidthRatio
        {
            get { return _ROIWidthRatio; }
            set { _ROIWidthRatio = value; RaisePropertyChanged(); }
        }

        private Element<double> _ROIHeightRatio = new Element<double>();
        /// <summary>
        /// ROIHeightRatio 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> ROIHeightRatio
        {
            get { return _ROIHeightRatio; }
            set { _ROIHeightRatio = value; RaisePropertyChanged(); }
        }

        private Element<double> _ROEWidthScoreRatio = new Element<double>();
        /// <summary>
        /// ROEWidthScoreRatio 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> ROEWidthScoreRatio
        {
            get { return _ROEWidthScoreRatio; }
            set { _ROEWidthScoreRatio = value; RaisePropertyChanged(); }
        }

        private Element<double> _MinThicknessRatio = new Element<double>();
        /// <summary>
        /// MinThicknessRatio 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> MinThicknessRatio
        {
            get { return _MinThicknessRatio; }
            set { _MinThicknessRatio = value; RaisePropertyChanged(); }
        }

        private Element<double> _MaxThicknessRatio = new Element<double>();
        /// <summary>
        /// MaxThicknessRatio 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> MaxThicknessRatio
        {
            get { return _MaxThicknessRatio; }
            set { _MaxThicknessRatio = value; RaisePropertyChanged(); }
        }

        public ScanCameraParam()
        {
            _Slot1Position = new LoaderCoordinate();
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {

            try
            {
                var shallowClone = MemberwiseClone() as ScanCameraParam;
                shallowClone.Slot1Position = Slot1Position.Clone<LoaderCoordinate>();
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
