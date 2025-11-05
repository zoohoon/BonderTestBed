using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LogModule;
using ProberInterfaces;
using Newtonsoft.Json;

namespace LoaderParameters
{
    /// <summary>
    /// SEMICSOCR의 디바이스를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    [KnownType(typeof(IElement))]
    public class SemicsOCRDevice : OCRDeviceBase, IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
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
        public new List<object> Nodes { get; set; }

        #region operation flag
        private Element<bool> _UserLightEnable = new Element<bool>();
        /// <summary>
        /// UserLightEnable 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> UserLightEnable
        {
            get { return _UserLightEnable; }
            set { _UserLightEnable = value; RaisePropertyChanged(); }
        }

        private Element<bool> _LotIntegrityEnable = new Element<bool>();
        /// <summary>
        /// LotIntegrityEnable 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> LotIntegrityEnable
        {
            get { return _LotIntegrityEnable; }
            set { _LotIntegrityEnable = value; RaisePropertyChanged(); }
        }

        private Element<bool> _SlotIntegrityEnable = new Element<bool>();
        /// <summary>
        /// SlotIntegrityEnable 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> SlotIntegrityEnable
        {
            get { return _SlotIntegrityEnable; }
            set { _SlotIntegrityEnable = value; RaisePropertyChanged(); }
        }

        private Element<bool> _WaferIntegrityEnable = new Element<bool>();
        /// <summary>
        /// WaferIntegrityEnable 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> WaferIntegrityEnable
        {
            get { return _WaferIntegrityEnable; }
            set { _WaferIntegrityEnable = value; RaisePropertyChanged(); }
        }

        private Element<bool> _OcrAdvancedReadEnable = new Element<bool>();
        /// <summary>
        /// OcrAdvancedReadEnable 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> OcrAdvancedReadEnable
        {
            get { return _OcrAdvancedReadEnable; }
            set { _OcrAdvancedReadEnable = value; RaisePropertyChanged(); }
        }

        private Element<bool> _OcrConfirmEnable = new Element<bool>();
        /// <summary>
        /// OcrConfirmEnable 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> OcrConfirmEnable
        {
            get { return _OcrConfirmEnable; }
            set { _OcrConfirmEnable = value; RaisePropertyChanged(); }
        }

        private Element<bool> _OcrCheckSumEnable = new Element<bool>();
        /// <summary>
        /// OcrCheckSumEnable 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> OcrCheckSumEnable
        {
            get { return _OcrCheckSumEnable; }
            set { _OcrCheckSumEnable = value; RaisePropertyChanged(); }
        }

        private Element<bool> _OcrLotIdFixEnable = new Element<bool>();
        /// <summary>
        /// OcrLotIdFixEnable 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> OcrLotIdFixEnable
        {
            get { return _OcrLotIdFixEnable; }
            set { _OcrLotIdFixEnable = value; RaisePropertyChanged(); }
        }

        private Element<bool> _OcrConfirmWaferIDPrefix = new Element<bool>();
        /// <summary>
        /// OcrConfirmWaferIDPrefix 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> OcrConfirmWaferIDPrefix
        {
            get { return _OcrConfirmWaferIDPrefix; }
            set { _OcrConfirmWaferIDPrefix = value; RaisePropertyChanged(); }
        }

        private Element<bool> _RejectOrConfirm = new Element<bool>();
        /// <summary>
        /// RejectOrConfirm 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> RejectOrConfirm
        {
            get { return _RejectOrConfirm; }
            set { _RejectOrConfirm = value; RaisePropertyChanged(); }
        }
        #endregion

        #region common processing data
        private Element<long> _OcrReadRegionPosX = new Element<long>();
        /// <summary>
        /// OcrReadRegionPosX 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<long> OcrReadRegionPosX
        {
            get { return _OcrReadRegionPosX; }
            set { _OcrReadRegionPosX = value; RaisePropertyChanged(); }
        }

        private Element<long> _OcrReadRegionPosY = new Element<long>();
        /// <summary>
        /// OcrReadRegionPosY 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<long> OcrReadRegionPosY
        {
            get { return _OcrReadRegionPosY; }
            set { _OcrReadRegionPosY = value; RaisePropertyChanged(); }
        }

        private Element<long> _OcrCharPosX = new Element<long>();
        /// <summary>
        /// OcrCharPosX 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<long> OcrCharPosX
        {
            get { return _OcrCharPosX; }
            set { _OcrCharPosX = value; RaisePropertyChanged(); }
        }

        private Element<long> _OcrCharPosY = new Element<long>();
        /// <summary>
        /// OcrCharPosY 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<long> OcrCharPosY
        {
            get { return _OcrCharPosY; }
            set { _OcrCharPosY = value; RaisePropertyChanged(); }
        }

        private Element<long> _OcrReadRegionWidth = new Element<long>();
        /// <summary>
        /// OcrReadRegionWidth 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<long> OcrReadRegionWidth
        {
            get { return _OcrReadRegionWidth; }
            set { _OcrReadRegionWidth = value; RaisePropertyChanged(); }
        }

        private Element<long> _OcrReadRegionHeight = new Element<long>();
        /// <summary>
        /// OcrReadRegionHeight 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<long> OcrReadRegionHeight
        {
            get { return _OcrReadRegionHeight; }
            set { _OcrReadRegionHeight = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrCharSizeX = new Element<double>();
        /// <summary>
        /// OcrCharSizeX 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OcrCharSizeX
        {
            get { return _OcrCharSizeX; }
            set { _OcrCharSizeX = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrCharSizeY = new Element<double>();
        /// <summary>
        /// OcrCharSizeY 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OcrCharSizeY
        {
            get { return _OcrCharSizeY; }
            set { _OcrCharSizeY = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrCharSpacing = new Element<double>();
        /// <summary>
        /// OcrCharSpacing 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OcrCharSpacing
        {
            get { return _OcrCharSpacing; }
            set { _OcrCharSpacing = value; RaisePropertyChanged(); }
        }

        private Element<int> _OcrMaxStringLength = new Element<int>();
        /// <summary>
        /// OcrMaxStringLength 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> OcrMaxStringLength
        {
            get { return _OcrMaxStringLength; }
            set { _OcrMaxStringLength = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrCalibrateXOffset = new Element<double>();
        [DataMember]
        public Element<double> OcrCalibrateXOffset
        {
            get { return _OcrCalibrateXOffset; }
            set { _OcrCalibrateXOffset = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrCalibrateMinX = new Element<double>();
        /// <summary>
        /// OcrCalibrateMinX 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OcrCalibrateMinX
        {
            get { return _OcrCalibrateMinX; }
            set { _OcrCalibrateMinX = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrCalibrateMaxX = new Element<double>();
        /// <summary>
        /// OcrCalibrateMaxX 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OcrCalibrateMaxX
        {
            get { return _OcrCalibrateMaxX; }
            set { _OcrCalibrateMaxX = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrCalibrateStepX = new Element<double>();
        /// <summary>
        /// OcrCalibrateStepX 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OcrCalibrateStepX
        {
            get { return _OcrCalibrateStepX; }
            set { _OcrCalibrateStepX = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrCalibrateYOffset = new Element<double>();
        [DataMember]
        public Element<double> OcrCalibrateYOffset
        {
            get { return _OcrCalibrateYOffset; }
            set { _OcrCalibrateYOffset = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrCalibrateMinY = new Element<double>();
        /// <summary>
        /// OcrCalibrateMinY 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OcrCalibrateMinY
        {
            get { return _OcrCalibrateMinY; }
            set { _OcrCalibrateMinY = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrCalibrateMaxY = new Element<double>();
        /// <summary>
        /// OcrCalibrateMaxY 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OcrCalibrateMaxY
        {
            get { return _OcrCalibrateMaxY; }
            set { _OcrCalibrateMaxY = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrCalibrateStepY = new Element<double>();
        /// <summary>
        /// OcrCalibrateStepY 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OcrCalibrateStepY
        {
            get { return _OcrCalibrateStepY; }
            set { _OcrCalibrateStepY = value; RaisePropertyChanged(); }
        }


        private Element<bool> _OcrFlipImage = new Element<bool>();
        /// <summary>
        /// OcrFlipImage 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> OcrFlipImage
        {
            get { return _OcrFlipImage; }
            set { _OcrFlipImage = value; RaisePropertyChanged(); }
        }
        private Element<bool> _OcrHorizontalFlipImage = new Element<bool>();
        /// <summary>
        /// OcrHorizontalFlipImage 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> OcrHorizontalFlipImage
        {
            get { return _OcrHorizontalFlipImage; }
            set { _OcrHorizontalFlipImage = value; RaisePropertyChanged(); }
        }
        private Element<bool> _OcrVerticalFlipImage = new Element<bool>();
        /// <summary>
        /// OcrVertlcalFlipImage 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> OcrVerticalFlipImage
        {
            get { return _OcrVerticalFlipImage; }
            set { _OcrVerticalFlipImage = value; RaisePropertyChanged(); }
        }
        #endregion
        private Element<string> _OCRFontFilePath = new Element<string>();

        [DataMember]
        public Element<string> OCRFontFilePath
        {
            get { return _OCRFontFilePath; }
            set { _OCRFontFilePath = value; RaisePropertyChanged(); }
        }

        private Element<string> _OCRFontFileName = new Element<string>();

        [DataMember]
        public Element<string> OCRFontFileName
        {
            get { return _OCRFontFileName; }
            set { _OCRFontFileName = value; RaisePropertyChanged(); }
        }

        private Element<string> _OcrFontType = new Element<string>();
        [DataMember]
        public Element<string> OcrFontType
        {
            get { return _OcrFontType; }
            set { _OcrFontType = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<OCRParamTable> _OCRParamTables;
        /// <summary>
        /// OCRParamTables 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<OCRParamTable> OCRParamTables
        {
            get { return _OCRParamTables; }
            set { _OCRParamTables = value; RaisePropertyChanged(); }
        }

        public SemicsOCRDevice()
        {
            _OCRParamTables = new ObservableCollection<OCRParamTable>();
        }

        /// <summary>
        /// 정의하는 모듈의 타입을 가져옵니다.
        /// </summary>
        /// <returns>모듈 타입</returns>
        public override ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.SEMICSOCR;
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public override object Clone()
        {
            //TODO : deepcopy if ref property is exist.
            try
            {
            var shallowClone = MemberwiseClone() as SemicsOCRDevice;
                shallowClone.OCRParamTables = OCRParamTables.CloneFrom();
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
    /// OCR의 ParamTable을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class OCRParamTable : INotifyPropertyChanged, ICloneable, IParamNode
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

        private Element<int> _UserOcrLightType = new Element<int>();
        /// <summary>
        /// UserOcrLightType 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> UserOcrLightType
        {
            get { return _UserOcrLightType; }
            set { _UserOcrLightType = value; RaisePropertyChanged(); }
        }

        private Element<ushort> _UserOcrLight1_Offset = new Element<ushort>();
        /// <summary>
        /// UserOcrLight1_Offset 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<ushort> UserOcrLight1_Offset
        {
            get { return _UserOcrLight1_Offset; }
            set { _UserOcrLight1_Offset = value; RaisePropertyChanged(); }
        }

        private Element<ushort> _UserOcrLight2_Offset = new Element<ushort>();
        /// <summary>
        /// UserOcrLight2_Offset 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<ushort> UserOcrLight2_Offset
        {
            get { return _UserOcrLight2_Offset; }
            set { _UserOcrLight2_Offset = value; RaisePropertyChanged(); }
        }

        private Element<ushort> _UserOcrLight3_Offset = new Element<ushort>();
        /// <summary>
        /// UserOcrLight3_Offset 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<ushort> UserOcrLight3_Offset
        {
            get { return _UserOcrLight3_Offset; }
            set { _UserOcrLight3_Offset = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrStrAcceptance = new Element<double>();
        /// <summary>
        /// OcrStrAcceptance 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OcrStrAcceptance
        {
            get { return _OcrStrAcceptance; }
            set { _OcrStrAcceptance = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrCharAcceptance = new Element<double>();
        /// <summary>
        /// OcrCharAcceptance 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OcrCharAcceptance
        {
            get { return _OcrCharAcceptance; }
            set { _OcrCharAcceptance = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrCalStrAcceptance = new Element<double>();
        /// <summary>
        /// OcrCalStrAcceptance 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OcrCalStrAcceptance
        {
            get { return _OcrCalStrAcceptance; }
            set { _OcrCalStrAcceptance = value; RaisePropertyChanged(); }
        }

        private Element<double> _OcrCalCharAcceptance = new Element<double>();
        /// <summary>
        /// OcrCalCharAcceptance 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OcrCalCharAcceptance
        {
            get { return _OcrCalCharAcceptance; }
            set { _OcrCalCharAcceptance = value; RaisePropertyChanged(); }
        }

        private Element<string> _OcrSampleString = new Element<string>();
        /// <summary>
        /// OcrSampleString 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> OcrSampleString
        {
            get { return _OcrSampleString; }
            set { _OcrSampleString = value; RaisePropertyChanged(); }
        }

        private Element<string> _OcrConstraint = new Element<string>();
        /// <summary>
        /// OcrConstraint 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> OcrConstraint
        {
            get { return _OcrConstraint; }
            set { _OcrConstraint = value; RaisePropertyChanged(); }
        }

        private Element<int> _OcrCalibrationType = new Element<int>();
        /// <summary>
        /// OcrCalibrationType 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> OcrCalibrationType
        {
            get { return _OcrCalibrationType; }
            set { _OcrCalibrationType = value; RaisePropertyChanged(); }
        }

        private Element<int> _OcrMasterFilter = new Element<int>();
        /// <summary>
        /// OcrMasterFilter 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> OcrMasterFilter
        {
            get { return _OcrMasterFilter; }
            set { _OcrMasterFilter = value; RaisePropertyChanged(); }
        }

        private Element<int> _OcrMasterFilterGain = new Element<int>();
        /// <summary>
        /// OcrMasterFilterGain 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> OcrMasterFilterGain
        {
            get { return _OcrMasterFilterGain; }
            set { _OcrMasterFilterGain = value; RaisePropertyChanged(); }
        }

        private Element<int> _OcrSlaveFilter = new Element<int>();
        /// <summary>
        /// OcrSlaveFilter 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> OcrSlaveFilter
        {
            get { return _OcrSlaveFilter; }
            set { _OcrSlaveFilter = value; RaisePropertyChanged(); }
        }

        private Element<int> _OcrSlaveFilterGain = new Element<int>();
        /// <summary>
        /// OcrSlaveFilterGain 의 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> OcrSlaveFilterGain
        {
            get { return _OcrSlaveFilterGain; }
            set { _OcrSlaveFilterGain = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            try
            {
            var shallowClone = MemberwiseClone() as OCRParamTable;
                //TODO : deepcopy if ref property is exist.
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
