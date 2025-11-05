using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberErrorCode;
using ProberInterfaces;
using LogModule;
using Newtonsoft.Json;
using LoaderParameters.AttachedModules.Defaultslotsize;
using ProberInterfaces.Enum;
using System.Linq;

namespace LoaderParameters
{
    /// <summary>
    /// LoaderSystemParameter 을 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class LoaderSystemParameter : INotifyPropertyChanged, ICloneable, ISystemParameterizable, IHasAbstactClassSerialized
    {
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
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }

        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

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
        public string FilePath { get; } = "Loader";

        [XmlIgnore, JsonIgnore]
        public string FileName { get; } = "LoaderSystem.json";


        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.DefaultSlotSizes == null)
                {
                    this.DefaultSlotSizes = new DefaultslotsizeDefinition();

                    this.DefaultSlotSizes.SlotSize6inch.Value = 4760;
                    this.DefaultSlotSizes.SlotSize6inchTolerance.Value = 0;

                    this.DefaultSlotSizes.SlotSize8inch.Value = 6350;
                    this.DefaultSlotSizes.SlotSize8inchTolerance.Value = 0;

                    this.DefaultSlotSizes.SlotSize12inch.Value = 10000;
                    this.DefaultSlotSizes.SlotSize12inchTolerance.Value = 0;
                }
                else
                {
                    if (this.DefaultSlotSizes.SlotSize6inch.Value == 0)
                        this.DefaultSlotSizes.SlotSize6inch.Value = 4760;

                    if (this.DefaultSlotSizes.SlotSize8inch.Value == 0)
                        this.DefaultSlotSizes.SlotSize8inch.Value = 6350;

                    if (this.DefaultSlotSizes.SlotSize12inch.Value == 0)
                        this.DefaultSlotSizes.SlotSize12inch.Value = 10000;
                }
                
                if (this.DefaultWaferSize.Value == SubstrateSizeEnum.UNDEFINED)
                {
                    DefaultWaferSize.Value = SubstrateSizeEnum.INCH12;
                }
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = SetDefaultParamOPUSVTestMachine();
                //RetVal = SetDefaultParamBSCIMachine();
                //RetVal = SetDefaultParamGPLoader();
                //SetDefaultParamGPLoader_OPRT();
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public void SetElementMetaData()
        {
            foreach (var item in CognexOCRModules)
            {
                foreach (var accParam in item.AccessParams)
                {
                    var size = accParam.SubstrateSize.Value;

                    accParam.VPos.ElementName = $"Cognex V Pos for {size}";
                    accParam.VPos.ReadMaskingLevel = 0;
                    accParam.VPos.WriteMaskingLevel = 0;
                    accParam.VPos.CategoryID = "10014";

                    accParam.Position.A.ElementName = $"Cognex A Pos for {size}";
                    accParam.Position.A.ReadMaskingLevel = 0;
                    accParam.Position.A.WriteMaskingLevel = 0;
                    accParam.Position.A.CategoryID = "10014";

                    accParam.Position.W.ElementName = $"Cognex W Pos for {size}";
                    accParam.Position.W.ReadMaskingLevel = 0;
                    accParam.Position.W.WriteMaskingLevel = 0;
                    accParam.Position.W.CategoryID = "10014";

                    accParam.Position.U.ElementName = $"Cognex U Pos for {size}";
                    accParam.Position.U.ReadMaskingLevel = 0;
                    accParam.Position.U.WriteMaskingLevel = 0;
                    accParam.Position.U.CategoryID = "10014";

                }
            }
            #region ScanModule
            foreach (var item in ScanSensorModules)
            {
                foreach (var scanParam in item.ScanParams)
                {
                    var size = scanParam.SubstrateSize.Value;
                    scanParam.SensorOffset.CategoryID = "00010010";
                    scanParam.SensorOffset.ElementName = $"Sensor offset for {size}";
                    scanParam.SensorOffset.Description = $"Sensor offset for {size}";

                    scanParam.UpOffset.CategoryID = "00010010";
                    scanParam.UpOffset.ElementName = $"Sensor Up Position offset for {size}";
                    scanParam.UpOffset.Description = $"Sensor Up Position offset for {size}";

                    scanParam.DownOffset.CategoryID = "00010010";
                    scanParam.DownOffset.ElementName = $"Sensor Down Position offset for {size}";
                    scanParam.DownOffset.Description = $"Sensor Down Position offset for {size}";

                    scanParam.InSlotLowerRatio.CategoryID = "00010010";
                    scanParam.InSlotLowerRatio.ElementName = $"In Slot Lower ratio for {size}";
                    scanParam.InSlotLowerRatio.Description = $"In Slot Lower ratio for {size}";

                    scanParam.InSlotUpperRatio.CategoryID = "00010010";
                    scanParam.InSlotUpperRatio.ElementName = $"In Slot Upper Ratio for {size}";
                    scanParam.InSlotUpperRatio.Description = $"In Slot Upper Ratio for {size}";

                    scanParam.ThicknessTol.CategoryID = "00010010";
                    scanParam.ThicknessTol.ElementName = $"Scan sensor Thickness Tolerence for {size}";
                    scanParam.ThicknessTol.Description = $"Scan sensor Thickness Tolerence for {size}";
                }
            }
            #endregion

            #region PreAlignModule
            foreach (var item in PreAlignModules)
            {
                foreach (var acc in item.AccessParams)
                {
                    var size = acc.SubstrateSize.Value;

                    acc.Position.A.CategoryID = "00010008";
                    acc.Position.A.ElementName = $"Prealigner for {size} Access Position A";
                    acc.Position.A.Description = $"Prealigner for {size} Access Position A";

                    acc.Position.E.CategoryID = "00010008";
                    acc.Position.E.ElementName = $"Prealigner for {size} Access Position E";
                    acc.Position.E.Description = $"Prealigner for {size} Access Position E";

                    acc.Position.U.CategoryID = "00010008";
                    acc.Position.U.ElementName = $"Prealigner for {size} Access Position U";
                    acc.Position.U.Description = $"Prealigner for {size} Access Position U";

                    acc.Position.W.CategoryID = "00010008";
                    acc.Position.W.ElementName = $"Prealigner for {size} Access Position W";
                    acc.Position.W.Description = $"Prealigner for {size} Access Position W";

                    acc.PickupIncrement.CategoryID = "00010008";
                    acc.PickupIncrement.ElementName = $"Pre-Aligner for {size} PickupIncrement Value";
                    acc.PickupIncrement.Description = $"Pre-Aligner for {size} PickupIncrement Value";
                }
            }
            #endregion

            #region CassetteModule
            foreach (var item in CassetteModules)
            {
                foreach (var acc in item.Slot1AccessParams)
                {
                    var size = acc.SubstrateSize.Value;

                    acc.Position.A.CategoryID = "00010010";
                    acc.Position.A.ElementName = $"CassetteModule for {size} Access Position A";
                    acc.Position.A.Description = $"CassetteModule for {size} Access Position A";

                    acc.Position.E.CategoryID = "00010010";
                    acc.Position.E.ElementName = $"CassetteModule for {size} Access Position E";
                    acc.Position.E.Description = $"CassetteModule for {size} Access Position E";

                    acc.Position.U.CategoryID = "00010010";
                    acc.Position.U.ElementName = $"CassetteModule for {size} Access Position U";
                    acc.Position.U.Description = $"CassetteModule for {size} Access Position U";

                    acc.Position.W.CategoryID = "00010010";
                    acc.Position.W.ElementName = $"CassetteModule for {size} Access Position W";
                    acc.Position.W.Description = $"CassetteModule for {size} Access Position W";

                    acc.PickupIncrement.CategoryID = "00010010";
                    acc.PickupIncrement.ElementName = $"CassetteModule for {size} PickupIncrement";
                    acc.PickupIncrement.Description = $"CassetteModule for {size} PickupIncrement";

                    acc.UStopPosOffset.CategoryID = "00010010";
                    acc.UStopPosOffset.ElementName = $"CassetteModule for {size} U stop pos offset";
                    acc.UStopPosOffset.Description = $"CassetteModule for {size} U stop pos offset";

                }
            }
            #endregion 

            #region ChuckModule
            foreach (var item in ChuckModules)
            {
                foreach (var acc in item.AccessParams)
                {
                    var size = acc.SubstrateSize.Value;
                    
                    acc.Position.A.CategoryID = "00010008";
                    acc.Position.A.ElementName = $"ChuckModules for {size} Access Position A";
                    acc.Position.A.Description = $"ChuckModules for {size} Access Position A";
                    
                    acc.Position.E.CategoryID = "00010008";
                    acc.Position.E.ElementName = $"ChuckModules for {size} Access Position E";
                    acc.Position.E.Description = $"ChuckModules for {size} Access Position E";
                    
                    acc.Position.U.CategoryID = "00010008";
                    acc.Position.U.ElementName = $"ChuckModules for {size} Access Position U";
                    acc.Position.U.Description = $"ChuckModules for {size} Access Position U";
                    
                    acc.Position.W.CategoryID = "00010008";
                    acc.Position.W.ElementName = $"ChuckModules for {size} Access Position W";
                    acc.Position.W.Description = $"ChuckModules for {size} Access Position W";

                    acc.PickupIncrement.CategoryID = "00010008";
                    acc.PickupIncrement.ElementName = $"ChuckModules for {size} PickupIncrement";
                    acc.PickupIncrement.Description = $"ChuckModules for {size} PickupIncrement";

                }
            }
            #endregion 

            #region ArmModule
            foreach(var item in ARMModules)
            {
                item.UpOffset.CategoryID = "00010009";
                item.UpOffset.ElementName = $"ArmModule up offset for {item.AxisType.Value}";
                item.UpOffset.Description = $"ArmModule up offset for {item.AxisType.Value}";

                item.EndOffset.CategoryID = "00010009";
                item.EndOffset.ElementName = $"ArmModule End offset for {item.AxisType.Value}";
                item.EndOffset.Description = $"ArmModule End offset for {item.AxisType.Value}";
            }
            #endregion 

            #region InpectionTray
            foreach (var item in InspectionTrayModules)
            {
                foreach (var acc in item.AccessParams)
                {
                    var size = acc.SubstrateSize.Value;
                    
                    acc.Position.A.CategoryID = "00010008";
                    acc.Position.A.ElementName = $"InpectionTrayModule for {size} Access Position A";
                    acc.Position.A.Description = $"InpectionTrayModule for {size} Access Position A";
                    
                    acc.Position.E.CategoryID = "00010008";
                    acc.Position.E.ElementName = $"InpectionTrayModule for {size} Access Position E";
                    acc.Position.E.Description = $"InpectionTrayModule for {size} Access Position E";
                    
                    acc.Position.U.CategoryID = "00010008";
                    acc.Position.U.ElementName = $"InpectionTrayModule for {size} Access Position U";
                    acc.Position.U.Description = $"InpectionTrayModule for {size} Access Position U";
                    
                    acc.Position.W.CategoryID = "00010008";
                    acc.Position.W.ElementName = $"InpectionTrayModule for {size} Access Position W";
                    acc.Position.W.Description = $"InpectionTrayModule for {size} Access Position W";

                    acc.PickupIncrement.CategoryID = "00010008";
                    acc.PickupIncrement.ElementName = $"InpectionTrayModule for {size} PickupIncrement";
                    acc.PickupIncrement.Description = $"InpectionTrayModule for {size} PickupIncrement";
                }
            }
            #endregion 

            #region FixedTray
            foreach (var item in FixedTrayModules)
            {
                foreach (var acc in item.AccessParams)
                {
                    var size = acc.SubstrateSize.Value;

                    acc.Position.A.CategoryID = "00010008";
                    acc.Position.A.ElementName = $"FixedTrayModule for {size} Access Position A";
                    acc.Position.A.Description = $"FixedTrayModule for {size} Access Position A";
                    
                    acc.Position.E.CategoryID = "00010008";
                    acc.Position.E.ElementName = $"FixedTrayModule for {size} Access Position E";
                    acc.Position.E.Description = $"FixedTrayModule for {size} Access Position E";
                    
                    acc.Position.U.CategoryID = "00010008";
                    acc.Position.U.ElementName = $"FixedTrayModule for {size} Access Position U";
                    acc.Position.U.Description = $"FixedTrayModule for {size} Access Position U";
                    
                    acc.Position.W.CategoryID = "00010008";
                    acc.Position.W.ElementName = $"FixedTrayModule for {size} Access Position W";
                    acc.Position.W.Description = $"FixedTrayModule for {size} Access Position W";

                    acc.PickupIncrement.CategoryID = "00010008";
                    acc.PickupIncrement.ElementName = $"FixedTrayModule for {size} PickupIncrement";
                    acc.PickupIncrement.Description = $"FixedTrayModule for {size} PickupIncrement";
                }
            }
            #endregion


        }
        private UExtensionBase _UExtension = new UExtensionNone();
        /// <summary>
        /// UExtension 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public UExtensionBase UExtension
        {
            get { return _UExtension; }
            set { _UExtension = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ScanCameraDefinition> _ScanCameraModules = new ObservableCollection<ScanCameraDefinition>();
        /// <summary>
        /// ScanCameraModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<ScanCameraDefinition> ScanCameraModules
        {
            get { return _ScanCameraModules; }
            set { _ScanCameraModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ScanSensorDefinition> _ScanSensorModules = new ObservableCollection<ScanSensorDefinition>();
        /// <summary>
        /// ScanSensorModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<ScanSensorDefinition> ScanSensorModules
        {
            get { return _ScanSensorModules; }
            set { _ScanSensorModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<CassetteDefinition> _CassetteModules = new ObservableCollection<CassetteDefinition>();
        /// <summary>
        /// CassetteModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CassetteDefinition> CassetteModules
        {
            get { return _CassetteModules; }
            set { _CassetteModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ARMDefinition> _ARMModules = new ObservableCollection<ARMDefinition>();
        /// <summary>
        /// ARMModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<ARMDefinition> ARMModules
        {
            get { return _ARMModules; }
            set { _ARMModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<CardARMDefinition> _CardARMModules = new ObservableCollection<CardARMDefinition>();
        /// <summary>
        /// CardARMModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CardARMDefinition> CardARMModules
        {
            get { return _CardARMModules; }
            set { _CardARMModules = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<PreAlignDefinition> _PreAlignModules = new ObservableCollection<PreAlignDefinition>();
        /// <summary>
        /// PreAlignModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<PreAlignDefinition> PreAlignModules
        {
            get { return _PreAlignModules; }
            set { _PreAlignModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<CognexOCRDefinition> _CognexOCRModules = new ObservableCollection<CognexOCRDefinition>();
        /// <summary>
        /// CognexOCRModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CognexOCRDefinition> CognexOCRModules
        {
            get { return _CognexOCRModules; }
            set { _CognexOCRModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<SemicsOCRDefinition> _SemicsOCRModules = new ObservableCollection<SemicsOCRDefinition>();
        /// <summary>
        /// SemicsOCRModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<SemicsOCRDefinition> SemicsOCRModules
        {
            get { return _SemicsOCRModules; }
            set { _SemicsOCRModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ChuckDefinition> _ChuckModules = new ObservableCollection<ChuckDefinition>();
        /// <summary>
        /// ChuckModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<ChuckDefinition> ChuckModules
        {
            get { return _ChuckModules; }
            set { _ChuckModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<FixedTrayDefinition> _FixedTrayModules = new ObservableCollection<FixedTrayDefinition>();
        /// <summary>
        /// FixedTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<FixedTrayDefinition> FixedTrayModules
        {
            get { return _FixedTrayModules; }
            set { _FixedTrayModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<InspectionTrayDefinition> _InspectionTrayModules = new ObservableCollection<InspectionTrayDefinition>();
        /// <summary>
        /// InspectionTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<InspectionTrayDefinition> InspectionTrayModules
        {
            get { return _InspectionTrayModules; }
            set { _InspectionTrayModules = value; RaisePropertyChanged(); }
        }

        private DefaultslotsizeDefinition _Defaultslotsizes = new DefaultslotsizeDefinition();
        /// <summary>
        /// InspectionTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public DefaultslotsizeDefinition DefaultSlotSizes
        {
            get { return _Defaultslotsizes; }
            set { _Defaultslotsizes = value; RaisePropertyChanged(); }
        }

        private Element<SubstrateSizeEnum> _DefaultWaferSize = new Element<SubstrateSizeEnum>(SubstrateSizeEnum.INCH12);
        /// <summary>
        /// DefaultWaferSize  기본값은 12inch .stm 만 6ich.
        /// </summary>
        [DataMember]
        public Element<SubstrateSizeEnum> DefaultWaferSize
        {
            get { return _DefaultWaferSize; }
            set { _DefaultWaferSize = value; RaisePropertyChanged(); }
        }
        private Element<bool> _useLotProcessingVerify = new Element<bool>(false);
        [DataMember]
        public Element<bool> UseLotProcessingVerify
        {
            get { return _useLotProcessingVerify; }
            set
            {
                if (value != _useLotProcessingVerify)
                {
                    _useLotProcessingVerify = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<long> _LoaderJobTimeoutforOpenedDoor = new Element<long>();
        [DataMember]
        public Element<long> LoaderJobTimeoutforOpenedDoor
        {
            get { return _LoaderJobTimeoutforOpenedDoor; }
            set
            {
                if (value != _LoaderJobTimeoutforOpenedDoor)
                {
                    _LoaderJobTimeoutforOpenedDoor = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region // GPLoader

        private ObservableCollection<BufferDefinition> _BufferModules = new ObservableCollection<BufferDefinition>();
        /// <summary>
        /// BufferModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<BufferDefinition> BufferModules
        {
            get { return _BufferModules; }
            set { _BufferModules = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<CardBufferTrayDefinition> _CardBufferTrayModules = new ObservableCollection<CardBufferTrayDefinition>();
        /// <summary>
        /// CardBufferTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CardBufferTrayDefinition> CardBufferTrayModules
        {
            get { return _CardBufferTrayModules; }
            set { _CardBufferTrayModules = value; RaisePropertyChanged(); }
        }

        private EnumLoaderMovingMethodType _LoaderMovingMethodType = EnumLoaderMovingMethodType.OPUSV_MINI;
        ///<summary>
        ///LoaderType을 설정 
        ///</summary>
        [DataMember]
        public EnumLoaderMovingMethodType LoaderMovingMethodType
        {
            get { return _LoaderMovingMethodType; }
            set { _LoaderMovingMethodType = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<CCDefinition> _CCModules = new ObservableCollection<CCDefinition>();
        /// <summary>
        /// CCModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CCDefinition> CCModules
        {
            get { return _CCModules; }
            set { _CCModules = value; RaisePropertyChanged(); }
        }



        private ObservableCollection<CardBufferDefinition> _CardBufferModules = new ObservableCollection<CardBufferDefinition>();
        /// <summary>
        /// CCModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CardBufferDefinition> CardBufferModules
        {
            get { return _CardBufferModules; }
            set { _CardBufferModules = value; RaisePropertyChanged(); }
        }

        private LoaderCoordinate _Card_ID_Position = new LoaderCoordinate() ;
        /// <summary>
        /// Chuck의 Pickup 위치를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public LoaderCoordinate Card_ID_Position
        {
            get { return _Card_ID_Position; }
            set { _Card_ID_Position = value; RaisePropertyChanged(); }
        }

        private Element<int> _CardTrayIndexOffset=new Element<int>() ;
        /// <summary>
        /// Chuck의 Pickup 위치를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> CardTrayIndexOffset
        {
            get { return _CardTrayIndexOffset; }
            set { _CardTrayIndexOffset = value; RaisePropertyChanged(); }
        }

        private Element<double> _TCW_LoadingAngle = new Element<double>();
        /// <summary>
        /// Chuck의 Pickup 위치를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> TCW_LoadingAngle
        {
            get { return _TCW_LoadingAngle; }
            set { _TCW_LoadingAngle = value; RaisePropertyChanged(); }
        }
        private Element<double> _TCW_UnloadingAngle = new Element<double>();
        /// <summary>
        /// Chuck의 Pickup 위치를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> TCW_UnloadingAngle
        {
            get { return _TCW_UnloadingAngle; }
            set { _TCW_UnloadingAngle = value; RaisePropertyChanged(); }
        }
        private Element<double> _DangerousPosOffset_LX = new Element<double>() { Value = 15000};
        /// <summary>
        ///  현재 있는 LX 위치에서 +- 값을 파라미터와 비교해서 셔터를 닫을지 결정한다.
        /// </summary>
        [DataMember]
        //[XmlIgnore, JsonIgnore]
        public Element<double> DangerousPosOffset_LX
        {
            get { return _DangerousPosOffset_LX; }
            set { _DangerousPosOffset_LX = value; RaisePropertyChanged(); }
        }


        private Element<double> _DangerousPosOffset_LZM = new Element<double>() { Value = 15000 };
        /// <summary>
        /// 현재 있는 LZM 위치에서 +- 값을 파라미터와 비교해서 셔터를 닫을지 결정한다.
        /// </summary>
        [DataMember]
        //[XmlIgnore, JsonIgnore]
        public Element<double> DangerousPosOffset_LZM
        {
            get { return _DangerousPosOffset_LZM; }
            set { _DangerousPosOffset_LZM = value; RaisePropertyChanged(); }
        }

        private Element<double> _DangerousPosOffset_LW = new Element<double>() { Value = 30000 };
        /// <summary>
        ///  현재 있는 LW 위치에서 +- 값을 파라미터와 비교해서 셔터를 닫을지 결정한다.
        /// </summary>
        /// 
        [DataMember]
        //[XmlIgnore, JsonIgnore]
        public Element<double> DangerousPosOffset_LW
        {
            get { return _DangerousPosOffset_LW; }
            set { _DangerousPosOffset_LW = value; RaisePropertyChanged(); }
        }
        private Element<double> _DangerousPos_ARM_Limit = new Element<double>() { Value = 5000 };
        /// <summary>
        /// Arm이 현재 있는 위치와 Limit값을 비교해서 Cell에 셔터를 닫을지 결정한다.
        /// </summary>
        [DataMember]
        //[XmlIgnore, JsonIgnore]
        public Element<double> DangerousPos_ARM_Limit
        {
            get { return _DangerousPos_ARM_Limit; }
            set { _DangerousPos_ARM_Limit = value; RaisePropertyChanged(); }
        }
        private Element<int> _Cassette_Arm_LX_Interference = new Element<int>() { Value = 50000 };
        /// <summary>
        /// Loader 와 카세트가 간섭이 있는 위치인지 비교하기 위한 Offset 
        /// CST COVER 를 동작 할지 결정한다.
        /// </summary>
        [DataMember]
        //[XmlIgnore, JsonIgnore]
        public Element<int> Cassette_Arm_LX_Interference
        {
            get { return _Cassette_Arm_LX_Interference; }
            set { _Cassette_Arm_LX_Interference = value; RaisePropertyChanged(); }
        }

        private Element<int> _Cassette_Arm_LZ_Interference = new Element<int>() { Value = 50000 };
        /// <summary>
        /// Loader 와 카세트가 간섭이 있는 위치인지 비교하기 위한 Offset 
        /// CST COVER 를 동작 할지 결정한다.
        /// </summary>
        [DataMember]
        //[XmlIgnore, JsonIgnore]
        public Element<int> Cassette_Arm_LZ_Interference
        {
            get { return _Cassette_Arm_LZ_Interference; }
            set { _Cassette_Arm_LZ_Interference = value; RaisePropertyChanged(); }
        }

        private Element<int> _Cassette_Arm_LW_Interference = new Element<int>() { Value = 50000 };
        /// <summary>
        /// Loader 와 카세트가 간섭이 있는 위치인지 비교하기 위한 Offset 
        /// CST COVER 를 동작 할지 결정한다.
        /// </summary>
        [DataMember]
        //[XmlIgnore, JsonIgnore]
        public Element<int> Cassette_Arm_LW_Interference
        {
            get { return _Cassette_Arm_LW_Interference; }
            set { _Cassette_Arm_LW_Interference = value; RaisePropertyChanged(); }
        }

        private Element<int> _Cassette_Arm_LU_Interference = new Element<int>() { Value = 10000 };
        /// <summary>
        /// Loader 와 카세트가 간섭이 있는 위치인지 비교하기 위한 Offset 
        /// CST COVER 를 동작 할지 결정한다.
        /// </summary>
        [DataMember]
        //[XmlIgnore, JsonIgnore]
        public Element<int> Cassette_Arm_LU_Interference
        {
            get { return _Cassette_Arm_LU_Interference; }
            set { _Cassette_Arm_LU_Interference = value; RaisePropertyChanged(); }
        }

        #endregion
        /// <summary>
        /// ImplTypes 을 반환합니다.
        /// </summary>
        /// <returns>Type Array</returns>
        public Type[] GetImplTypes()
        {
            return new Type[]
            {
                //=> UExtension Definitions
                typeof(UExtensionNone),
                typeof(UExtensionCylinder),
                typeof(UExtensionMotor),

                //=> UExtension Move Params
                typeof(UExtensionNoneMoveParam),
                typeof(UExtensionCylinderMoveParam),
                typeof(UExtensionMotorMoveParam),
                //add abstract class impl
            };
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            var shallowClone = MemberwiseClone() as LoaderSystemParameter;

            try
            {
                shallowClone.UExtension = UExtension.Clone<UExtensionBase>();
                shallowClone.ScanCameraModules = ScanCameraModules.CloneFrom();
                shallowClone.ScanSensorModules = ScanSensorModules.CloneFrom();
                shallowClone.CassetteModules = CassetteModules.CloneFrom();
                shallowClone.ARMModules = ARMModules.CloneFrom();
                shallowClone.PreAlignModules = PreAlignModules.CloneFrom();
                shallowClone.CognexOCRModules = CognexOCRModules.CloneFrom();
                shallowClone.SemicsOCRModules = SemicsOCRModules.CloneFrom();
                shallowClone.ChuckModules = ChuckModules.CloneFrom();
                shallowClone.FixedTrayModules = FixedTrayModules.CloneFrom();
                shallowClone.InspectionTrayModules = InspectionTrayModules.CloneFrom();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return shallowClone;
        }
        public EventCodeEnum SetDefalutParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // 250911 LJH 주석품
                retVal = SetDefaultParamOPUSVTestMachine();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private EventCodeEnum SetDefaultParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {


                //=> Loader Setting
                //UExtension = new UExtensionCylinder()
                //{
                //    IOWaitTimeout = 10000,
                //    DOARMINAIR = "DOARMINAIR",
                //    DOARMOUTAIR = "DOARMOUTAIR",
                //    DIARMIN = "DIARMIN",
                //    DIARMOUT = "DIARMOUT"
                //};

                UExtension = new UExtensionCylinder();
                var UExtensionVar = (UExtension as UExtensionCylinder);

                UExtensionVar.IOWaitTimeout.Value = 10000;
                UExtensionVar.DOARMINAIR.Value = "DOARMINAIR";
                UExtensionVar.DOARMOUTAIR.Value = "DOARMOUTAIR";
                UExtensionVar.DIARMIN.Value = "DIARMIN";
                UExtensionVar.DIARMOUT.Value = "DIARMOUT";

                //=> CameraScan
                //CameraScanDefinition def = new CameraScanDefinition();
                //sysParam.CameraScanModules.Add(def);

                //=> SensorScan
                ScanSensorDefinition scanSensorDef = new ScanSensorDefinition();

                scanSensorDef.DOSCAN_SENSOR_OUT.Value = "DOSCAN_SENSOR_OUT";
                scanSensorDef.DISCAN_SENSOR_OUT.Value = "DISCAN_SENSOR_OUT";
                scanSensorDef.DISCAN_SENSOR_IN.Value = "DISCAN_SENSOR_IN";
                scanSensorDef.IOCheckDelayTimeout.Value = 500;
                scanSensorDef.IOWaitTimeout.Value = 60000;


                var ScanSensorParamVar = new ScanSensorParam();

                ScanSensorParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                ScanSensorParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                ScanSensorParamVar.SensorOffset.Value = 25299.08;
                ScanSensorParamVar.UpOffset.Value = 5000;
                ScanSensorParamVar.DownOffset.Value = -5000;
                ScanSensorParamVar.InSlotLowerRatio.Value = 0.15;
                ScanSensorParamVar.InSlotUpperRatio.Value = 0.75;
                ScanSensorParamVar.ThicknessTol.Value = 1.5;
                ScanSensorParamVar.ScanAxis.Value = EnumAxisConstants.SC;

                scanSensorDef.ScanParams.Add(ScanSensorParamVar);

                //scanSensorDef.ScanParams.Value.Add(new ScanSensorParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    SensorOffset = 25299.08,
                //    UpOffset = 5000,
                //    DownOffset = -5000,
                //    InSlotLowerRatio = 0.15,
                //    InSlotUpperRatio = 0.75,
                //    ThicknessTol = 1.5,
                //    ScanAxis = EnumAxisConstants.SC
                //});
                ScanSensorModules.Add(scanSensorDef);

                //=> Cassette
                CassetteDefinition cassetteDef = new CassetteDefinition();
                cassetteDef.ScanModuleType.Value = ModuleTypeEnum.SCANSENSOR;

                for (int i = 1; i <= 25; i++)
                {
                    cassetteDef.SlotModules.Add(new SlotDefinition());
                }

                var Slot1AccessParamsVar = new CassetteSlot1AccessParam();

                Slot1AccessParamsVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                Slot1AccessParamsVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                Slot1AccessParamsVar.Position = CreateLoaderCoordinate(47903.7, 198600, 0, false);
                Slot1AccessParamsVar.UStopPosOffset.Value = 15000;
                Slot1AccessParamsVar.PickupIncrement.Value = 7000;

                cassetteDef.Slot1AccessParams.Add(Slot1AccessParamsVar);

                //cassetteDef.Slot1AccessParams.Value.Add(new CassetteSlot1AccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(47903.7, 198600, 0, false),
                //    UStopPosOffset = 15000,
                //    PickupIncrement = 7000,
                //});
                CassetteModules.Add(cassetteDef);

                //=> ARM1
                ARMDefinition armDef = new ARMDefinition();
                armDef.AxisType.Value = EnumAxisConstants.U1;
                armDef.EndOffset.Value = 0;
                armDef.UpOffset.Value = 0;
                armDef.DIWAFERONMODULE.Value = "DIWAFERONARM";
                armDef.DOAIRON.Value = "DOARMAIRON";
                armDef.UseType.Value = ARMUseTypeEnum.BOTH;
                armDef.IOCheckMaintainTime.Value = 100;
                armDef.IOCheckDelayTimeout.Value = 500;
                armDef.IOWaitTimeout.Value = 10000;
                ARMModules.Add(armDef);

                //=> ARM2
                armDef = new ARMDefinition();
                armDef.AxisType.Value = EnumAxisConstants.U2;
                armDef.EndOffset.Value = -1100;
                armDef.UpOffset.Value = 15400;
                armDef.DIWAFERONMODULE.Value = "DIWAFERONARM2";
                armDef.DOAIRON.Value = "DOARM2AIRON";
                armDef.UseType.Value = ARMUseTypeEnum.UNLOADING;
                armDef.IOCheckMaintainTime.Value = 100;
                armDef.IOCheckDelayTimeout.Value = 500;
                armDef.IOWaitTimeout.Value = 10000;
                ARMModules.Add(armDef);

                //=> PA1
                PreAlignDefinition paDef = new PreAlignDefinition();
                paDef.AxisType.Value = EnumAxisConstants.V;
                paDef.IOCheckDelayTimeout.Value = 500;
                paDef.IOWaitTimeout.Value = 10000;
                paDef.DIWAFERONMODULE.Value = "DIWAFERONSUBCHUCK";
                paDef.DOAIRON.Value = "DOSUBCHUCKAIRON";

                paDef.RetryCount.Value = 5; //TODO : => Processing Param으로 이동

                var ProcessingParamsVar = new PreAlignProcessingParam();

                ProcessingParamsVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                ProcessingParamsVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                ProcessingParamsVar.LightChannel.Value = 15;
                ProcessingParamsVar.LightIntensity.Value = 255;
                ProcessingParamsVar.NotchSensorAngle.Value = 90;
                ProcessingParamsVar.CameraAngle.Value = 135;
                ProcessingParamsVar.CameraRatio.Value = 49.16;
                ProcessingParamsVar.CenteringTolerance.Value = 80;

                paDef.ProcessingParams.Add(ProcessingParamsVar);

                //paDef.ProcessingParams.Value.Add(new PreAlignProcessingParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    LightChannel = 15,
                //    LightIntensity = 255,
                //    NotchSensorAngle = 90,
                //    CameraAngle = 135,
                //    CameraRatio = 49.16,
                //    CenteringTolerance = 150,
                //});

                var PreAlignAccessParam = new PreAlignAccessParam();

                PreAlignAccessParam.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                PreAlignAccessParam.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                PreAlignAccessParam.Position = CreateLoaderCoordinate(10650, 166511.87, -17991, false);
                PreAlignAccessParam.PickupIncrement.Value = 7000;

                paDef.AccessParams.Add(PreAlignAccessParam);

                //paDef.AccessParams.Add(new PreAlignAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(10650, 166511.87, -17991, false),
                //    PickupIncrement = 7000,
                //});
                PreAlignModules.Add(paDef);

                //=> SemicsOCR
                SemicsOCRDefinition semicsOcrDef = new SemicsOCRDefinition();

                semicsOcrDef.DependencyPreAlignNum.Value = 1;
                semicsOcrDef.OCRDirection.Value = OCRDirectionEnum.FRONT;

                var OCRAccessParamVar = new OCRAccessParam();

                OCRAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                OCRAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                OCRAccessParamVar.Position = CreateLoaderCoordinate(10650 + 7000, 166511.87 - 46500, -17991 + 810, false);
                OCRAccessParamVar.VPos.Value = 0;

                semicsOcrDef.AccessParams.Add(OCRAccessParamVar);

                //semicsOcrDef.AccessParams.Add(new OCRAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(10650 + 7000, 166511.87 - 46500, -17991 + 810, false),
                //    VPos = 0,
                //});

                semicsOcrDef.OCRCam.Value = EnumProberCam.OCR1_CAM;
                semicsOcrDef.LightChannel1.Value = 12;
                semicsOcrDef.LightChannel2.Value = 0;
                semicsOcrDef.LightChannel3.Value = 0;
                semicsOcrDef.OcrLight1_Offset.Value = 0;
                semicsOcrDef.OcrLight2_Offset.Value = 0;
                semicsOcrDef.OcrLight3_Offset.Value = 0;
                //semicsOcrDef.UserLightEnable = false;
                //semicsOcrDef.CheckLotNameBeforeOcr = false;
                //semicsOcrDef.CreateLotNameBeforeOcr = false;
                //semicsOcrDef.LotIntegrityEnable = false;
                //semicsOcrDef.SlotIntegrityEnable = false;
                //semicsOcrDef.WaferIntegrityEnable = false;
                //semicsOcrDef.LotPauseAfterReject = false;
                //semicsOcrDef.OcrReadAfterManualInput = false;
                //semicsOcrDef.OcrEditInLotRun = false;
                //semicsOcrDef.OcrThresholdVal = 0;
                //semicsOcrDef.OcrSmooth = false;
                //semicsOcrDef.OcrGradientEnable = false;

                SemicsOCRModules.Add(semicsOcrDef);

                var cognexParam = new CognexOCRDefinition();
                cognexParam.DependencyPreAlignNum.Value = 1;
                cognexParam.OCRDirection.Value = OCRDirectionEnum.FRONT;

                var OCRAccessParamVar2 = new OCRAccessParam();

                OCRAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                OCRAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                OCRAccessParamVar2.Position = CreateLoaderCoordinate(10650 + 7000, 166511.87 - 46500, -17991 + 810, false);
                OCRAccessParamVar2.VPos.Value = 0;

                cognexParam.AccessParams.Add(OCRAccessParamVar2);

                //cognexParam.AccessParams.Add(new OCRAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(10650 + 7000, 166511.87 - 46500, -17991 + 810, false),
                //    VPos = 0,
                //});

                cognexParam.OCRCam.Value = EnumProberCam.OCR1_CAM;
                CognexOCRModules.Add(cognexParam);

                //=> CHUCK
                ChuckDefinition chuckDef = new ChuckDefinition();

                var ChuckAccessParamVar = new ChuckAccessParam();

                ChuckAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                ChuckAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                ChuckAccessParamVar.Position = CreateLoaderCoordinate(4499.04, 218000, -9000, true);
                ChuckAccessParamVar.PickupIncrement.Value = 0;

                chuckDef.AccessParams.Add(ChuckAccessParamVar);

                //chuckDef.AccessParams.Add(new ChuckAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(4499.04, 218000, -9000, true),
                //    PickupIncrement = 0,
                //});
                ChuckModules.Add(chuckDef);

                //=> FixedTray1
                FixedTrayDefinition fixedDef = new FixedTrayDefinition();
                fixedDef.IOCheckDelayTimeout.Value = 500;
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY0";

                var FixedTrayAccessParamVar = new FixedTrayAccessParam();

                FixedTrayAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                FixedTrayAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, false);
                FixedTrayAccessParamVar.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(198000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //=> FixedTray2
                fixedDef = new FixedTrayDefinition();
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY1";
                fixedDef.IOCheckDelayTimeout.Value = 500;

                var FixedTrayAccessParamVar2 = new FixedTrayAccessParam();

                FixedTrayAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                FixedTrayAccessParamVar2.Position = CreateLoaderCoordinate(226000, 166000, -17991, false);
                FixedTrayAccessParamVar2.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar2);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(226000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //=> FixedTray3
                fixedDef = new FixedTrayDefinition();
                fixedDef.IOCheckDelayTimeout.Value = 500;
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY2";

                var FixedTrayAccessParamVar3 = new FixedTrayAccessParam();

                FixedTrayAccessParamVar3.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar3.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                FixedTrayAccessParamVar3.Position = CreateLoaderCoordinate(254000, 166000, -17991, false);
                FixedTrayAccessParamVar3.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar3);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(254000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //=> FixedTray4
                fixedDef = new FixedTrayDefinition();
                fixedDef.IOCheckDelayTimeout.Value = 500;
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY3";

                var FixedTrayAccessParamVar4 = new FixedTrayAccessParam();

                FixedTrayAccessParamVar4.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar4.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                FixedTrayAccessParamVar4.Position = CreateLoaderCoordinate(282000, 166000, -17991, false);
                FixedTrayAccessParamVar4.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar4);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(282000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //=> FixedTray5
                fixedDef = new FixedTrayDefinition();
                fixedDef.IOCheckDelayTimeout.Value = 500;
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY4";

                var FixedTrayAccessParamVar5 = new FixedTrayAccessParam();

                FixedTrayAccessParamVar5.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar5.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                FixedTrayAccessParamVar5.Position = CreateLoaderCoordinate(310000, 166000, -17991, false);
                FixedTrayAccessParamVar5.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar5);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(310000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //  => InspectionTray
                InspectionTrayDefinition insDef = new InspectionTrayDefinition();
                insDef.IsInterferenceWithCassettePort.Value = false;
                insDef.IOCheckDelayTimeout.Value = 500;
                insDef.DIWAFERONMODULE.Value = "DIWAFERONDRAWER";
                insDef.DIOPENDED.Value = "DIDRAWEROPEN";
                insDef.DIMOVED.Value = "DIDRAWEREMOVED";

                var InspectionTrayAccessParamVar = new InspectionTrayAccessParam();

                InspectionTrayAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                InspectionTrayAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                InspectionTrayAccessParamVar.Position = CreateLoaderCoordinate(-88000, 200000, 0, false);
                InspectionTrayAccessParamVar.PickupIncrement.Value = 10000;

                insDef.AccessParams.Add(InspectionTrayAccessParamVar);

                //insDef.AccessParams.Add(new InspectionTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(-88000, 200000, 0, false),
                //    PickupIncrement = 10000,
                //});
                InspectionTrayModules.Add(insDef);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private EventCodeEnum SetDefaultParamOPUSVTestMachine()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                //=> Loader Setting
                //UExtension = new UExtensionCylinder()
                //{
                //    IOWaitTimeout = 10000,
                //    DOARMINAIR = "DOARMINAIR",
                //    DOARMOUTAIR = "DOARMOUTAIR",
                //    DIARMIN = "DIARMIN",
                //    DIARMOUT = "DIARMOUT"
                //};

                UExtension = new UExtensionCylinder();
                UExtension.UExtensionType.Value = UExtensionTypeEnum.NONE;
                var UExtensionVar = (UExtension as UExtensionCylinder);

                UExtensionVar.IOWaitTimeout.Value = 10000;
                UExtensionVar.DOARMINAIR.Value = "DOARMINAIR";
                UExtensionVar.DOARMOUTAIR.Value = "DOARMOUTAIR";
                UExtensionVar.DIARMIN.Value = "DIARMIN";
                UExtensionVar.DIARMOUT.Value = "DIARMOUT";

                //=> CameraScan
                //CameraScanDefinition def = new CameraScanDefinition();
                //sysParam.CameraScanModules.Add(def);

                //=> SensorScan
                ScanSensorDefinition scanSensorDef = new ScanSensorDefinition();
                scanSensorDef.DOSCAN_SENSOR_OUT.Value = "DOSCAN_SENSOR_OUT";
                scanSensorDef.DISCAN_SENSOR_OUT.Value = "DISCAN_SENSOR_OUT";
                scanSensorDef.DISCAN_SENSOR_IN.Value = "DISCAN_SENSOR_IN";
                scanSensorDef.IOCheckDelayTimeout.Value = 500;
                scanSensorDef.IOWaitTimeout.Value = 60000;

                var ScanSensorParamVar = new ScanSensorParam();

                ScanSensorParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_2R;
                ScanSensorParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                ScanSensorParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                ScanSensorParamVar.SensorOffset.Value = 0;
                ScanSensorParamVar.UpOffset.Value = 3000;
                ScanSensorParamVar.DownOffset.Value = -2500;
                ScanSensorParamVar.InSlotLowerRatio.Value = 0.40;
                ScanSensorParamVar.InSlotUpperRatio.Value = 0.90;
                ScanSensorParamVar.ThicknessTol.Value = 2.5;
                ScanSensorParamVar.ScanAxis.Value = EnumAxisConstants.SC;

                scanSensorDef.ScanParams.Add(ScanSensorParamVar);

                //scanSensorDef.ScanParams.Add(new ScanSensorParam()
                //{
                //    SensorInputPort = EnumMotorDedicatedIn.MotorDedicatedIn_2R,
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    SensorOffset = 0,
                //    UpOffset = 3000,
                //    DownOffset = -2500,
                //    InSlotLowerRatio = 0.35,
                //    InSlotUpperRatio = 0.75,
                //    ThicknessTol = 1.5,
                //    ScanAxis = EnumAxisConstants.SC

                //});
                ScanSensorModules.Add(scanSensorDef);

                //=> Cassette
                CassetteDefinition cassetteDef = new CassetteDefinition();
                cassetteDef.ScanModuleType.Value = ModuleTypeEnum.SCANSENSOR;

                for (int i = 1; i <= 25; i++)
                {
                    cassetteDef.SlotModules.Add(new SlotDefinition());
                }

                var CassetteSlot1AccessParamVar = new CassetteSlot1AccessParam();

                CassetteSlot1AccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                CassetteSlot1AccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                CassetteSlot1AccessParamVar.Position = CreateLoaderCoordinate(22300, 145500, 0, false, 27940);
                CassetteSlot1AccessParamVar.UStopPosOffset.Value = 200;
                CassetteSlot1AccessParamVar.PickupIncrement.Value = 3800;

                cassetteDef.Slot1AccessParams.Add(CassetteSlot1AccessParamVar);

                //cassetteDef.Slot1AccessParams.Add(new CassetteSlot1AccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(22900, 145500, 0, false, 27450),
                //    UStopPosOffset = 200,
                //    PickupIncrement = 3800,
                //});
                CassetteModules.Add(cassetteDef);

                //=> ARM1
                ARMDefinition armDef = new ARMDefinition();
                armDef.AxisType.Value = EnumAxisConstants.U1;
                armDef.EndOffset.Value = 0;
                armDef.UpOffset.Value = 0;
                armDef.DIWAFERONMODULE.Value = "DIWAFERONARM";
                armDef.DOAIRON.Value = "DOARMAIRON";
                armDef.UseType.Value = ARMUseTypeEnum.BOTH;
                armDef.IOCheckMaintainTime.Value = 100;
                armDef.IOCheckDelayTimeout.Value = 500;
                armDef.IOWaitTimeout.Value = 10000;
                ARMModules.Add(armDef);

                //=> ARM2
                armDef = new ARMDefinition();
                armDef.AxisType.Value = EnumAxisConstants.U2;
                armDef.EndOffset.Value = 0;
                armDef.UpOffset.Value = 16900;
                armDef.DIWAFERONMODULE.Value = "DIWAFERONARM2";
                armDef.DOAIRON.Value = "DOARM2AIRON";
                armDef.UseType.Value = ARMUseTypeEnum.BOTH;
                armDef.IOCheckMaintainTime.Value = 100;
                armDef.IOCheckDelayTimeout.Value = 500;
                armDef.IOWaitTimeout.Value = 10000;
                ARMModules.Add(armDef);

                ////=> CARDARM
                //CardARMDefinition CardARMDef = new CardARMDefinition();
                //CardARMDef = new CardARMDefinition();
                //CardARMDef.AxisType.Value = EnumAxisConstants.LCC;
                //CardARMDef.EndOffset.Value = 0;
                //CardARMDef.UpOffset.Value = 16900;
                //CardARMDef.DIWAFERONMODULE.Value = "DICCARMVAC";
                //CardARMDef.DOAIRON.Value = "DOCCArmVac";
                //CardARMDef.UseType.Value = ARMUseTypeEnum.BOTH;
                //CardARMDef.IOCheckMaintainTime.Value = 100;
                //CardARMDef.IOCheckDelayTimeout.Value = 500;
                //CardARMDef.IOWaitTimeout.Value = 10000;
                //CardARMModules.Add(CardARMDef);

                //=> PA1
                PreAlignDefinition paDef = new PreAlignDefinition();
                paDef.AxisType.Value = EnumAxisConstants.V;
                paDef.IOCheckDelayTimeout.Value = 500;
                paDef.IOWaitTimeout.Value = 10000;
                paDef.DIWAFERONMODULE.Value = "DIWAFERONSUBCHUCK";
                paDef.DOAIRON.Value = "DOSUBCHUCKAIRON";

                paDef.RetryCount.Value = 5; //TODO : => Processing Param으로 이동

                var PreAlignProcessingParamVar = new PreAlignProcessingParam();

                PreAlignProcessingParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                PreAlignProcessingParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                PreAlignProcessingParamVar.LightChannel.Value = 0;
                PreAlignProcessingParamVar.LightIntensity.Value = 255;
                PreAlignProcessingParamVar.NotchSensorAngle.Value = -90;
                PreAlignProcessingParamVar.CameraAngle.Value = 320;
                PreAlignProcessingParamVar.CameraRatio.Value = 49.16;
                PreAlignProcessingParamVar.CenteringTolerance.Value = 80;
                PreAlignProcessingParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_2R;
                paDef.ProcessingParams.Add(PreAlignProcessingParamVar);
                //paDef.ProcessingParams.Add(new PreAlignProcessingParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    LightChannel = 0,
                //    LightIntensity = 255,
                //    NotchSensorAngle = -90,
                //    CameraAngle = 320,
                //    CameraRatio = 25.468,
                //    CenteringTolerance = 150,
                //});

                var PreAlignAccessParamVar = new PreAlignAccessParam();

                PreAlignAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                PreAlignAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                PreAlignAccessParamVar.Position = CreateLoaderCoordinate(-202300, 141500, 0, false, 0);
                PreAlignAccessParamVar.PickupIncrement.Value = 7000;

                paDef.AccessParams.Add(PreAlignAccessParamVar);

                //paDef.AccessParams.Add(new PreAlignAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(-202300, 141500, 0, false, 0),
                //    PickupIncrement = 7000,
                //});
                PreAlignModules.Add(paDef);

                //=> SemicsOCR
                SemicsOCRDefinition semicsOcrDef = new SemicsOCRDefinition();
                semicsOcrDef.DependencyPreAlignNum.Value = 1;
                semicsOcrDef.OCRDirection.Value = OCRDirectionEnum.FRONT;

                var OCRAccessParamVar = new OCRAccessParam();

                OCRAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                OCRAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                OCRAccessParamVar.Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, -17991 + 810, false);
                OCRAccessParamVar.VPos.Value = 0;

                semicsOcrDef.AccessParams.Add(OCRAccessParamVar);

                //semicsOcrDef.AccessParams.Add(new OCRAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, -17991 + 810, false),
                //    VPos = 0,
                //});

                semicsOcrDef.OCRCam.Value = EnumProberCam.OCR1_CAM;
                semicsOcrDef.LightChannel1.Value = 12;
                semicsOcrDef.LightChannel2.Value = 0;
                semicsOcrDef.LightChannel3.Value = 0;
                semicsOcrDef.OcrLight1_Offset.Value = 0;
                semicsOcrDef.OcrLight2_Offset.Value = 0;
                semicsOcrDef.OcrLight3_Offset.Value = 0;
                //semicsOcrDef.UserLightEnable = false;
                //semicsOcrDef.CheckLotNameBeforeOcr = false;
                //semicsOcrDef.CreateLotNameBeforeOcr = false;
                //semicsOcrDef.LotIntegrityEnable = false;
                //semicsOcrDef.SlotIntegrityEnable = false;
                //semicsOcrDef.WaferIntegrityEnable = false;
                //semicsOcrDef.LotPauseAfterReject = false;
                //semicsOcrDef.OcrReadAfterManualInput = false;
                //semicsOcrDef.OcrEditInLotRun = false;
                //semicsOcrDef.OcrThresholdVal = 0;
                //semicsOcrDef.OcrSmooth = false;
                //semicsOcrDef.OcrGradientEnable = false;

                SemicsOCRModules.Add(semicsOcrDef);

                var cognexParam = new CognexOCRDefinition();
                cognexParam.DependencyPreAlignNum.Value = 1;
                cognexParam.OCRDirection.Value = OCRDirectionEnum.FRONT;

                var OCRAccessParamVar2 = new OCRAccessParam();

                OCRAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                OCRAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                OCRAccessParamVar2.Position = CreateLoaderCoordinate(-184300, 120011.87, 23, false);
                OCRAccessParamVar2.VPos.Value = 18000;

                cognexParam.AccessParams.Add(OCRAccessParamVar2);

                //cognexParam.AccessParams.Add(new OCRAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, 0.0, false),
                //    VPos = 0,
                //});
                cognexParam.OCRCam.Value = EnumProberCam.OCR1_CAM;
                CognexOCRModules.Add(cognexParam);

                //=> CHUCK
                //ChuckDefinition chuckDef12 = new ChuckDefinition();

                //var ChuckAccessParamVar = new ChuckAccessParam();

                //ChuckAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                //ChuckAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                //ChuckAccessParamVar.Position = CreateLoaderCoordinate(147200, 140000, -9000, true);
                //ChuckAccessParamVar.PickupIncrement.Value = 0;

                //chuckDef12.AccessParams.Add(ChuckAccessParamVar);

                //chuckDef12.AccessParams.Add(new ChuckAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(147200, 140000, -9000, true),
                //    PickupIncrement = 0,
                //});

                ChuckDefinition chuckDef8 = new ChuckDefinition();

                var ChuckAccessParamVar2 = new ChuckAccessParam();

                ChuckAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                ChuckAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                ChuckAccessParamVar2.Position = CreateLoaderCoordinate(155700, 145000, -9000, true);
                ChuckAccessParamVar2.PickupIncrement.Value = 0;

                chuckDef8.AccessParams.Add(ChuckAccessParamVar2);

                //chuckDef8.AccessParams.Add(new ChuckAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(147200, 145000, -9000, true),
                //    PickupIncrement = 0,
                //});

                //ChuckDefinition chuckDef6 = new ChuckDefinition();

                //var ChuckAccessParamVar3 = new ChuckAccessParam();

                //ChuckAccessParamVar3.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                //ChuckAccessParamVar3.SubstrateSize.Value = SubstrateSizeEnum.INCH6;
                //ChuckAccessParamVar3.Position = CreateLoaderCoordinate(147200, 140000, -9000, true);
                //ChuckAccessParamVar3.PickupIncrement.Value = 0;

                //chuckDef6.AccessParams.Add(ChuckAccessParamVar3);

                //chuckDef6.AccessParams.Add(new ChuckAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH6,
                //    Position = CreateLoaderCoordinate(147200, 140000, -9000, true),
                //    PickupIncrement = 0,
                //});
                ChuckModules.Add(chuckDef8);
                //  ChuckModules.Add(chuckDef12);
                //  ChuckModules.Add(chuckDef6);

                //=> FixedTray1
                FixedTrayDefinition fixedDef = new FixedTrayDefinition();
                fixedDef.IOCheckDelayTimeout.Value = 500;
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY0";

                var FixedTrayAccessParamVar = new FixedTrayAccessParam();

                FixedTrayAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                FixedTrayAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, false);
                FixedTrayAccessParamVar.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(198000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //=> FixedTray2
                fixedDef = new FixedTrayDefinition();
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY1";
                fixedDef.IOCheckDelayTimeout.Value = 500;

                var FixedTrayAccessParamVar2 = new FixedTrayAccessParam();

                FixedTrayAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                FixedTrayAccessParamVar2.Position = CreateLoaderCoordinate(226000, 166000, -17991, false);
                FixedTrayAccessParamVar2.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar2);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(226000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //=> FixedTray3
                fixedDef = new FixedTrayDefinition();
                fixedDef.IOCheckDelayTimeout.Value = 500;
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY2";

                var FixedTrayAccessParamVar3 = new FixedTrayAccessParam();

                FixedTrayAccessParamVar3.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar3.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                FixedTrayAccessParamVar3.Position = CreateLoaderCoordinate(254000, 166000, -17991, false);
                FixedTrayAccessParamVar3.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar3);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(254000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //=> FixedTray4
                fixedDef = new FixedTrayDefinition();

                fixedDef.IOCheckDelayTimeout.Value = 500;
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY3";

                var FixedTrayAccessParamVar4 = new FixedTrayAccessParam();

                FixedTrayAccessParamVar4.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar4.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                FixedTrayAccessParamVar4.Position = CreateLoaderCoordinate(310000, 166000, -17991, false);
                FixedTrayAccessParamVar4.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar4);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(282000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //=> FixedTray5
                fixedDef = new FixedTrayDefinition();
                fixedDef.IOCheckDelayTimeout.Value = 500;
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY4";

                var FixedTrayAccessParamVar5 = new FixedTrayAccessParam();

                FixedTrayAccessParamVar5.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar5.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                FixedTrayAccessParamVar5.Position = CreateLoaderCoordinate(310000, 166000, -17991, false);
                FixedTrayAccessParamVar5.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar5);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(310000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //  => InspectionTray
                InspectionTrayDefinition insDef = new InspectionTrayDefinition();
                insDef.IsInterferenceWithCassettePort.Value = false;
                insDef.IOCheckDelayTimeout.Value = 500;
                insDef.DIWAFERONMODULE.Value = "DIWAFERONDRAWER";
                insDef.DIOPENDED.Value = "DIDRAWEROPEN";
                insDef.DIMOVED.Value = "DIDRAWEREMOVED";

                var InspectionTrayAccessParamVar = new InspectionTrayAccessParam();

                InspectionTrayAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                InspectionTrayAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                InspectionTrayAccessParamVar.Position = CreateLoaderCoordinate(-64441, 140000, 0, false, 0);
                InspectionTrayAccessParamVar.PickupIncrement.Value = 3800;

                insDef.AccessParams.Add(InspectionTrayAccessParamVar);

                //insDef.AccessParams.Add(new InspectionTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(-64441, 140000, 0, false, 0),
                //    PickupIncrement = 3800,
                //});
                InspectionTrayModules.Add(insDef);

                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private EventCodeEnum SetDefaultParamBSCIMachine()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                //=> Loader Setting
                //UExtension = new UExtensionCylinder()
                //{
                //    IOWaitTimeout = 10000,
                //    DOARMINAIR = "DOARMINAIR",
                //    DOARMOUTAIR = "DOARMOUTAIR",
                //    DIARMIN = "DIARMIN",
                //    DIARMOUT = "DIARMOUT"
                //};

                UExtension = new UExtensionCylinder();
                var UExtensionVar = (UExtension as UExtensionCylinder);

                UExtensionVar.IOWaitTimeout.Value = 10000;
                UExtensionVar.DOARMINAIR.Value = "DOARMINAIR";
                UExtensionVar.DOARMOUTAIR.Value = "DOARMOUTAIR";
                UExtensionVar.DIARMIN.Value = "DIARMIN";
                UExtensionVar.DIARMOUT.Value = "DIARMOUT";

                //=> CameraScan
                //CameraScanDefinition def = new CameraScanDefinition();
                //sysParam.CameraScanModules.Add(def);

                //=> SensorScan
                ScanSensorDefinition scanSensorDef = new ScanSensorDefinition();
                scanSensorDef.DOSCAN_SENSOR_OUT.Value = "DOSCAN_SENSOR_OUT";
                scanSensorDef.DISCAN_SENSOR_OUT.Value = "DISCAN_SENSOR_OUT";
                scanSensorDef.DISCAN_SENSOR_IN.Value = "DISCAN_SENSOR_IN";
                scanSensorDef.IOCheckDelayTimeout.Value = 500;
                scanSensorDef.IOWaitTimeout.Value = 60000;

                var ScanSensorParamVar = new ScanSensorParam();
                // 6inch scan
                ScanSensorParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_2R;
                ScanSensorParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                ScanSensorParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH6;
                ScanSensorParamVar.SensorOffset.Value = 0;
                ScanSensorParamVar.UpOffset.Value = 3000;
                ScanSensorParamVar.DownOffset.Value = -2500;
                ScanSensorParamVar.InSlotLowerRatio.Value = 0.40;
                ScanSensorParamVar.InSlotUpperRatio.Value = 0.50;
                ScanSensorParamVar.ThicknessTol.Value = 3;
                ScanSensorParamVar.ScanAxis.Value = EnumAxisConstants.SC;

                scanSensorDef.ScanParams.Add(ScanSensorParamVar);

                // 8inch scan
                ScanSensorParamVar = new ScanSensorParam();

                ScanSensorParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_2R;
                ScanSensorParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                ScanSensorParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                ScanSensorParamVar.SensorOffset.Value = 0;
                ScanSensorParamVar.UpOffset.Value = 3000;
                ScanSensorParamVar.DownOffset.Value = -2500;
                ScanSensorParamVar.InSlotLowerRatio.Value = 0.35;
                ScanSensorParamVar.InSlotUpperRatio.Value = 0.65;
                ScanSensorParamVar.ThicknessTol.Value = 2.5;
                ScanSensorParamVar.ScanAxis.Value = EnumAxisConstants.SC;

                scanSensorDef.ScanParams.Add(ScanSensorParamVar);

                //scanSensorDef.ScanParams.Add(new ScanSensorParam()
                //{
                //    SensorInputPort = EnumMotorDedicatedIn.MotorDedicatedIn_2R,
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    SensorOffset = 0,
                //    UpOffset = 3000,
                //    DownOffset = -2500,
                //    InSlotLowerRatio = 0.35,
                //    InSlotUpperRatio = 0.75,
                //    ThicknessTol = 1.5,
                //    ScanAxis = EnumAxisConstants.SC

                //});
                ScanSensorModules.Add(scanSensorDef);

                //=> Cassette
                CassetteDefinition cassetteDef = new CassetteDefinition();
                cassetteDef.ScanModuleType.Value = ModuleTypeEnum.SCANSENSOR;

                for (int i = 1; i <= 25; i++)
                {
                    cassetteDef.SlotModules.Add(new SlotDefinition());
                }

                // 6 Inch Slot #1
                var CassetteSlot1AccessParamVar = new CassetteSlot1AccessParam();

                CassetteSlot1AccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                CassetteSlot1AccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH6;
                CassetteSlot1AccessParamVar.Position = CreateLoaderCoordinate(12900, 145500, 0, false, 18800);
                CassetteSlot1AccessParamVar.UStopPosOffset.Value = 200;
                CassetteSlot1AccessParamVar.PickupIncrement.Value = 2400;

                cassetteDef.Slot1AccessParams.Add(CassetteSlot1AccessParamVar);

                // 8 Inch Slot #1
                CassetteSlot1AccessParamVar = new CassetteSlot1AccessParam();

                CassetteSlot1AccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                CassetteSlot1AccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                CassetteSlot1AccessParamVar.Position = CreateLoaderCoordinate(22000, 145500, 0, false, 28740);
                CassetteSlot1AccessParamVar.UStopPosOffset.Value = 200;
                CassetteSlot1AccessParamVar.PickupIncrement.Value = 3800;

                cassetteDef.Slot1AccessParams.Add(CassetteSlot1AccessParamVar);



                //cassetteDef.Slot1AccessParams.Add(new CassetteSlot1AccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(22900, 145500, 0, false, 27450),
                //    UStopPosOffset = 200,
                //    PickupIncrement = 3800,
                //});
                CassetteModules.Add(cassetteDef);

                //=> ARM1
                ARMDefinition armDef = new ARMDefinition();
                armDef.AxisType.Value = EnumAxisConstants.U1;
                armDef.EndOffset.Value = 0;
                armDef.UpOffset.Value = 0;
                armDef.DIWAFERONMODULE.Value = "DIWAFERONARM";
                armDef.DOAIRON.Value = "DOARMAIRON";
                armDef.UseType.Value = ARMUseTypeEnum.BOTH;
                armDef.IOCheckMaintainTime.Value = 100;
                armDef.IOCheckDelayTimeout.Value = 500;
                armDef.IOWaitTimeout.Value = 10000;
                ARMModules.Add(armDef);

                //=> ARM2
                armDef = new ARMDefinition();
                armDef.AxisType.Value = EnumAxisConstants.U2;
                armDef.EndOffset.Value = 0;
                armDef.UpOffset.Value = 16900;
                armDef.DIWAFERONMODULE.Value = "DIWAFERONARM2";
                armDef.DOAIRON.Value = "DOARM2AIRON";
                armDef.UseType.Value = ARMUseTypeEnum.UNLOADING;
                armDef.IOCheckMaintainTime.Value = 100;
                armDef.IOCheckDelayTimeout.Value = 500;
                armDef.IOWaitTimeout.Value = 10000;
                ARMModules.Add(armDef);

                //=> PA1
                PreAlignDefinition paDef = new PreAlignDefinition();
                paDef.AxisType.Value = EnumAxisConstants.V;
                paDef.IOCheckDelayTimeout.Value = 500;
                paDef.IOWaitTimeout.Value = 10000;
                paDef.DIWAFERONMODULE.Value = "DIWAFERONSUBCHUCK";
                paDef.DOAIRON.Value = "DOSUBCHUCKAIRON";

                paDef.RetryCount.Value = 5; //TODO : => Processing Param으로 이동

                // 6 Inch
                var PreAlignProcessingParamVar = new PreAlignProcessingParam();

                PreAlignProcessingParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                PreAlignProcessingParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH6;
                PreAlignProcessingParamVar.LightChannel.Value = 8;
                PreAlignProcessingParamVar.LightIntensity.Value = 255;
                PreAlignProcessingParamVar.NotchSensorAngle.Value = -90;
                PreAlignProcessingParamVar.CameraAngle.Value = 0;
                PreAlignProcessingParamVar.CameraRatio.Value = 41.66;
                PreAlignProcessingParamVar.CenteringTolerance.Value = 80;
                PreAlignProcessingParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_1R;
                paDef.ProcessingParams.Add(PreAlignProcessingParamVar);

                // 8 Inch
                PreAlignProcessingParamVar = new PreAlignProcessingParam();

                PreAlignProcessingParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                PreAlignProcessingParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                PreAlignProcessingParamVar.LightChannel.Value = 9;
                PreAlignProcessingParamVar.LightIntensity.Value = 255;
                PreAlignProcessingParamVar.NotchSensorAngle.Value = -90;
                PreAlignProcessingParamVar.CameraAngle.Value = 320;
                PreAlignProcessingParamVar.CameraRatio.Value = 41.66;
                PreAlignProcessingParamVar.CenteringTolerance.Value = 80;
                PreAlignProcessingParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_2R;
                paDef.ProcessingParams.Add(PreAlignProcessingParamVar);
                //paDef.ProcessingParams.Add(new PreAlignProcessingParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    LightChannel = 0,
                //    LightIntensity = 255,
                //    NotchSensorAngle = -90,
                //    CameraAngle = 320,
                //    CameraRatio = 25.468,
                //    CenteringTolerance = 150,
                //});
                // 6 Inch
                var PreAlignAccessParamVar = new PreAlignAccessParam();

                PreAlignAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                PreAlignAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH6;
                PreAlignAccessParamVar.Position = CreateLoaderCoordinate(-202300, 143000, 0, false, 0);
                PreAlignAccessParamVar.PickupIncrement.Value = 7000;

                paDef.AccessParams.Add(PreAlignAccessParamVar);

                // 8 Inch
                PreAlignAccessParamVar = new PreAlignAccessParam();

                PreAlignAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                PreAlignAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                PreAlignAccessParamVar.Position = CreateLoaderCoordinate(-202300, 142500, 0, false, 0);
                PreAlignAccessParamVar.PickupIncrement.Value = 7000;

                paDef.AccessParams.Add(PreAlignAccessParamVar);
                //paDef.AccessParams.Add(new PreAlignAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(-202300, 141500, 0, false, 0),
                //    PickupIncrement = 7000,
                //});
                PreAlignModules.Add(paDef);

                //=> SemicsOCR
                SemicsOCRDefinition semicsOcrDef = new SemicsOCRDefinition();
                semicsOcrDef.DependencyPreAlignNum.Value = 1;
                semicsOcrDef.OCRDirection.Value = OCRDirectionEnum.FRONT;

                var OCRAccessParamVar = new OCRAccessParam();

                OCRAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                OCRAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                OCRAccessParamVar.Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, -17991 + 810, false);
                OCRAccessParamVar.VPos.Value = 0;

                semicsOcrDef.AccessParams.Add(OCRAccessParamVar);

                //semicsOcrDef.AccessParams.Add(new OCRAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, -17991 + 810, false),
                //    VPos = 0,
                //});

                semicsOcrDef.OCRCam.Value = EnumProberCam.OCR1_CAM;
                semicsOcrDef.LightChannel1.Value = 12;
                semicsOcrDef.LightChannel2.Value = 0;
                semicsOcrDef.LightChannel3.Value = 0;
                semicsOcrDef.OcrLight1_Offset.Value = 0;
                semicsOcrDef.OcrLight2_Offset.Value = 0;
                semicsOcrDef.OcrLight3_Offset.Value = 0;
                //semicsOcrDef.UserLightEnable = false;
                //semicsOcrDef.CheckLotNameBeforeOcr = false;
                //semicsOcrDef.CreateLotNameBeforeOcr = false;
                //semicsOcrDef.LotIntegrityEnable = false;
                //semicsOcrDef.SlotIntegrityEnable = false;
                //semicsOcrDef.WaferIntegrityEnable = false;
                //semicsOcrDef.LotPauseAfterReject = false;
                //semicsOcrDef.OcrReadAfterManualInput = false;
                //semicsOcrDef.OcrEditInLotRun = false;
                //semicsOcrDef.OcrThresholdVal = 0;
                //semicsOcrDef.OcrSmooth = false;
                //semicsOcrDef.OcrGradientEnable = false;

                SemicsOCRModules.Add(semicsOcrDef);

                var cognexParam = new CognexOCRDefinition();
                cognexParam.DependencyPreAlignNum.Value = 1;
                cognexParam.OCRDirection.Value = OCRDirectionEnum.FRONT;

                var OCRAccessParamVar2 = new OCRAccessParam();

                OCRAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                OCRAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH6;
                OCRAccessParamVar2.Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, 0.0, false);
                OCRAccessParamVar2.VPos.Value = 0;

                cognexParam.AccessParams.Add(OCRAccessParamVar2);


                OCRAccessParamVar2 = new OCRAccessParam();

                OCRAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                OCRAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                OCRAccessParamVar2.Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, 0.0, false);
                OCRAccessParamVar2.VPos.Value = 0;

                cognexParam.AccessParams.Add(OCRAccessParamVar2);

                //cognexParam.AccessParams.Add(new OCRAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, 0.0, false),
                //    VPos = 0,
                //});
                cognexParam.OCRCam.Value = EnumProberCam.OCR1_CAM;
                CognexOCRModules.Add(cognexParam);

                //=> CHUCK
                //ChuckDefinition chuckDef12 = new ChuckDefinition();

                //var ChuckAccessParamVar = new ChuckAccessParam();

                //ChuckAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                //ChuckAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                //ChuckAccessParamVar.Position = CreateLoaderCoordinate(147200, 140000, -9000, true);
                //ChuckAccessParamVar.PickupIncrement.Value = 0;

                //chuckDef12.AccessParams.Add(ChuckAccessParamVar);

                //chuckDef12.AccessParams.Add(new ChuckAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(147200, 140000, -9000, true),
                //    PickupIncrement = 0,
                //});



                ChuckDefinition chuckDef = new ChuckDefinition();

                var ChuckAccessParamVar2 = new ChuckAccessParam();

                // 6 Inch
                ChuckAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                ChuckAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH6;
                ChuckAccessParamVar2.Position = CreateLoaderCoordinate(156000, 145000, -9000, true);
                ChuckAccessParamVar2.PickupIncrement.Value = 0;

                chuckDef.AccessParams.Add(ChuckAccessParamVar2);

                // 8 Inch
                //ChuckDefinition chuckDef8 = new ChuckDefinition();

                ChuckAccessParamVar2 = new ChuckAccessParam();

                ChuckAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                ChuckAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                ChuckAccessParamVar2.Position = CreateLoaderCoordinate(156000, 145000, -9000, true);
                ChuckAccessParamVar2.PickupIncrement.Value = 0;

                chuckDef.AccessParams.Add(ChuckAccessParamVar2);

                //chuckDef8.AccessParams.Add(new ChuckAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(147200, 145000, -9000, true),
                //    PickupIncrement = 0,
                //});

                //ChuckDefinition chuckDef6 = new ChuckDefinition();

                //var ChuckAccessParamVar3 = new ChuckAccessParam();

                //ChuckAccessParamVar3.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                //ChuckAccessParamVar3.SubstrateSize.Value = SubstrateSizeEnum.INCH6;
                //ChuckAccessParamVar3.Position = CreateLoaderCoordinate(147200, 140000, -9000, true);
                //ChuckAccessParamVar3.PickupIncrement.Value = 0;

                //chuckDef6.AccessParams.Add(ChuckAccessParamVar3);

                //chuckDef6.AccessParams.Add(new ChuckAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH6,
                //    Position = CreateLoaderCoordinate(147200, 140000, -9000, true),
                //    PickupIncrement = 0,
                //});
                ChuckModules.Add(chuckDef);
                //ChuckModules.Add(chuckDef8);
                //  ChuckModules.Add(chuckDef12);
                //  ChuckModules.Add(chuckDef6);

                //=> FixedTray1
                FixedTrayDefinition fixedDef = new FixedTrayDefinition();
                fixedDef.IOCheckDelayTimeout.Value = 500;
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY0";

                var FixedTrayAccessParamVar = new FixedTrayAccessParam();

                FixedTrayAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                FixedTrayAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, false);
                FixedTrayAccessParamVar.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(198000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //=> FixedTray2
                fixedDef = new FixedTrayDefinition();
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY1";
                fixedDef.IOCheckDelayTimeout.Value = 500;

                var FixedTrayAccessParamVar2 = new FixedTrayAccessParam();

                FixedTrayAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                FixedTrayAccessParamVar2.Position = CreateLoaderCoordinate(226000, 166000, -17991, false);
                FixedTrayAccessParamVar2.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar2);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(226000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //=> FixedTray3
                fixedDef = new FixedTrayDefinition();
                fixedDef.IOCheckDelayTimeout.Value = 500;
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY2";

                var FixedTrayAccessParamVar3 = new FixedTrayAccessParam();

                FixedTrayAccessParamVar3.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar3.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                FixedTrayAccessParamVar3.Position = CreateLoaderCoordinate(254000, 166000, -17991, false);
                FixedTrayAccessParamVar3.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar3);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(254000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //=> FixedTray4
                fixedDef = new FixedTrayDefinition();

                fixedDef.IOCheckDelayTimeout.Value = 500;
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY3";

                var FixedTrayAccessParamVar4 = new FixedTrayAccessParam();

                FixedTrayAccessParamVar4.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar4.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                FixedTrayAccessParamVar4.Position = CreateLoaderCoordinate(310000, 166000, -17991, false);
                FixedTrayAccessParamVar4.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar4);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(282000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //=> FixedTray5
                fixedDef = new FixedTrayDefinition();
                fixedDef.IOCheckDelayTimeout.Value = 500;
                fixedDef.DIWAFERONMODULE.Value = "DIWAFERONFIXEDTRAY4";

                var FixedTrayAccessParamVar5 = new FixedTrayAccessParam();

                FixedTrayAccessParamVar5.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                FixedTrayAccessParamVar5.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                FixedTrayAccessParamVar5.Position = CreateLoaderCoordinate(310000, 166000, -17991, false);
                FixedTrayAccessParamVar5.PickupIncrement.Value = 12000;

                fixedDef.AccessParams.Add(FixedTrayAccessParamVar5);

                //fixedDef.AccessParams.Add(new FixedTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(310000, 166000, -17991, false),
                //    PickupIncrement = 12000,
                //});
                FixedTrayModules.Add(fixedDef);

                //  => InspectionTray
                InspectionTrayDefinition insDef = new InspectionTrayDefinition();
                insDef.IsInterferenceWithCassettePort.Value = false;
                insDef.IOCheckDelayTimeout.Value = 500;
                insDef.DIWAFERONMODULE.Value = "DIWAFERONDRAWER";
                insDef.DIOPENDED.Value = "DIDRAWEROPEN";
                insDef.DIMOVED.Value = "DIDRAWEREMOVED";

                var InspectionTrayAccessParamVar = new InspectionTrayAccessParam();

                InspectionTrayAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                InspectionTrayAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH6;
                InspectionTrayAccessParamVar.Position = CreateLoaderCoordinate(-64441, 140000, 0, false, 0);
                InspectionTrayAccessParamVar.PickupIncrement.Value = 3800;

                insDef.AccessParams.Add(InspectionTrayAccessParamVar);

                InspectionTrayAccessParamVar = new InspectionTrayAccessParam();

                InspectionTrayAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                InspectionTrayAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                InspectionTrayAccessParamVar.Position = CreateLoaderCoordinate(-64441, 140000, 0, false, 0);
                InspectionTrayAccessParamVar.PickupIncrement.Value = 3800;

                insDef.AccessParams.Add(InspectionTrayAccessParamVar);

                //insDef.AccessParams.Add(new InspectionTrayAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    Position = CreateLoaderCoordinate(-64441, 140000, 0, false, 0),
                //    PickupIncrement = 3800,
                //});
                InspectionTrayModules.Add(insDef);

                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private EventCodeEnum SetDefaultParamGPLoader()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                //=> Loader Setting
                UExtension = new UExtensionCylinder();
                var UExtensionVar = (UExtension as UExtensionCylinder);

                UExtensionVar.IOWaitTimeout.Value = 10000;
                UExtensionVar.DOARMINAIR.Value = "DOARMINAIR";
                UExtensionVar.DOARMOUTAIR.Value = "DOARMOUTAIR";
                UExtensionVar.DIARMIN.Value = "DIARMIN";
                UExtensionVar.DIARMOUT.Value = "DIARMOUT";
                //=> CameraScan
                //CameraScanDefinition def = new CameraScanDefinition();
                //sysParam.CameraScanModules.Add(def);
                //=> SensorScan
                ScanSensorDefinition scanSensorDef = new ScanSensorDefinition();
                scanSensorDef.DOSCAN_SENSOR_OUT.Value = "DOSCAN_SENSOR_OUT";
                scanSensorDef.DISCAN_SENSOR_OUT.Value = "DISCAN_SENSOR_OUT";
                scanSensorDef.DISCAN_SENSOR_IN.Value = "DISCAN_SENSOR_IN";
                scanSensorDef.IOCheckDelayTimeout.Value = 500;
                scanSensorDef.IOWaitTimeout.Value = 60000;

                var ScanSensorParamVar = new ScanSensorParam();
                // 6inch scan
                ScanSensorParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_2R;
                ScanSensorParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                ScanSensorParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH6;
                ScanSensorParamVar.SensorOffset.Value = 0;
                ScanSensorParamVar.UpOffset.Value = 3000;
                ScanSensorParamVar.DownOffset.Value = -2500;
                ScanSensorParamVar.InSlotLowerRatio.Value = 0.40;
                ScanSensorParamVar.InSlotUpperRatio.Value = 0.50;
                ScanSensorParamVar.ThicknessTol.Value = 3;
                ScanSensorParamVar.ScanAxis.Value = EnumAxisConstants.SC;

                scanSensorDef.ScanParams.Add(ScanSensorParamVar);

                // 8inch scan
                ScanSensorParamVar = new ScanSensorParam();

                ScanSensorParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_2R;
                ScanSensorParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                ScanSensorParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                ScanSensorParamVar.SensorOffset.Value = 0;
                ScanSensorParamVar.UpOffset.Value = 3000;
                ScanSensorParamVar.DownOffset.Value = -2500;
                ScanSensorParamVar.InSlotLowerRatio.Value = 0.35;
                ScanSensorParamVar.InSlotUpperRatio.Value = 0.65;
                ScanSensorParamVar.ThicknessTol.Value = 2.5;
                ScanSensorParamVar.ScanAxis.Value = EnumAxisConstants.SC;

                scanSensorDef.ScanParams.Add(ScanSensorParamVar);

                ScanSensorParamVar = new ScanSensorParam();

                ScanSensorParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_2R;
                ScanSensorParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                ScanSensorParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                ScanSensorParamVar.SensorOffset.Value = 0;
                ScanSensorParamVar.UpOffset.Value = 3000;
                ScanSensorParamVar.DownOffset.Value = -2500;
                ScanSensorParamVar.InSlotLowerRatio.Value = 0.35;
                ScanSensorParamVar.InSlotUpperRatio.Value = 0.65;
                ScanSensorParamVar.ThicknessTol.Value = 2.5;
                ScanSensorParamVar.ScanAxis.Value = EnumAxisConstants.SC;

                scanSensorDef.ScanParams.Add(ScanSensorParamVar);


                //scanSensorDef.ScanParams.Add(new ScanSensorParam()
                //{
                //    SensorInputPort = EnumMotorDedicatedIn.MotorDedicatedIn_2R,
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    SensorOffset = 0,
                //    UpOffset = 3000,
                //    DownOffset = -2500,
                //    InSlotLowerRatio = 0.35,
                //    InSlotUpperRatio = 0.75,
                //    ThicknessTol = 1.5,
                //    ScanAxis = EnumAxisConstants.SC

                //});
                ScanSensorModules.Add(scanSensorDef);

                //=> Cassette
                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                {
                    CassetteDefinition cassetteDef = new CassetteDefinition();
                    cassetteDef.ScanModuleType.Value = ModuleTypeEnum.SCANSENSOR;

                    for (int slotIndex = 1; slotIndex <= 25; slotIndex++)
                    {
                        cassetteDef.SlotModules.Add(new SlotDefinition());
                    }

                    // 6 Inch Slot #1

                    // 8 Inch Slot #1
                    var CassetteSlot1AccessParamVar = new CassetteSlot1AccessParam();

                    CassetteSlot1AccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    CassetteSlot1AccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    CassetteSlot1AccessParamVar.Position = CreateLoaderCoordinate(22000, 145500, 0, false, 28740);
                    CassetteSlot1AccessParamVar.UStopPosOffset.Value = 200;
                    CassetteSlot1AccessParamVar.PickupIncrement.Value = 3800;

                    cassetteDef.Slot1AccessParams.Add(CassetteSlot1AccessParamVar);

                    // 12 Inch Slot #1
                    CassetteSlot1AccessParamVar = new CassetteSlot1AccessParam();

                    CassetteSlot1AccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    CassetteSlot1AccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    CassetteSlot1AccessParamVar.Position = CreateLoaderCoordinate(22000, 145500, 0, false, 28740);
                    CassetteSlot1AccessParamVar.UStopPosOffset.Value = 200;
                    CassetteSlot1AccessParamVar.PickupIncrement.Value = 10000;

                    cassetteDef.Slot1AccessParams.Add(CassetteSlot1AccessParamVar);

                    CassetteModules.Add(cassetteDef);
                }


                //=> ARM1
                ARMDefinition armDef = new ARMDefinition();
                armDef.AxisType.Value = EnumAxisConstants.LUD;
                armDef.EndOffset.Value = 0;
                armDef.UpOffset.Value = 0;
                armDef.DIWAFERONMODULE.Value = "DIARM1VAC";
                armDef.DOAIRON.Value = "DOARM1Vac";
                armDef.UseType.Value = ARMUseTypeEnum.BOTH;
                armDef.IOCheckMaintainTime.Value = 100;
                armDef.IOCheckDelayTimeout.Value = 500;
                armDef.IOWaitTimeout.Value = 10000;
                ARMModules.Add(armDef);

                //=> ARM2
                armDef = new ARMDefinition();
                armDef.AxisType.Value = EnumAxisConstants.LUU;
                armDef.EndOffset.Value = 0;
                armDef.UpOffset.Value = 16900;
                armDef.DIWAFERONMODULE.Value = "DIARM2VAC";
                armDef.DOAIRON.Value = "DOARMVac2";
                armDef.UseType.Value = ARMUseTypeEnum.BOTH;
                armDef.IOCheckMaintainTime.Value = 100;
                armDef.IOCheckDelayTimeout.Value = 500;
                armDef.IOWaitTimeout.Value = 10000;
                ARMModules.Add(armDef);

                //=> CARDARM
                CardARMDefinition CardARMDef = new CardARMDefinition();
                CardARMDef = new CardARMDefinition();
                CardARMDef.AxisType.Value = EnumAxisConstants.LCC;
                CardARMDef.EndOffset.Value = 0;
                CardARMDef.UpOffset.Value = 16900;
                CardARMDef.DIWAFERONMODULE.Value = "DICCARMVAC";
                CardARMDef.DOAIRON.Value = "DOCCArmVac";
                CardARMDef.DOAIROFF.Value = "DOCCArmVac_Break";
                CardARMDef.UseType.Value = ARMUseTypeEnum.BOTH;
                CardARMDef.IOCheckMaintainTime.Value = 100;
                CardARMDef.IOCheckDelayTimeout.Value = 500;
                CardARMDef.IOWaitTimeout.Value = 10000;
                CardARMModules.Add(CardARMDef);


                //=> PA1
                for (int i = 0; i < SystemModuleCount.ModuleCnt.PACount; i++)
                {
                    PreAlignDefinition paDef = new PreAlignDefinition();
                    paDef.AxisType.Value = EnumAxisConstants.V;
                    paDef.IOCheckDelayTimeout.Value = 500;
                    paDef.IOWaitTimeout.Value = 10000;
                    paDef.DIWAFERONMODULE.Value = "DIWAFERONSUBCHUCK";
                    paDef.DOAIRON.Value = "DOSUBCHUCKAIRON";

                    paDef.RetryCount.Value = 5; //TODO : => Processing Param으로 이동

                    // 6 Inch
                    var PreAlignProcessingParamVar = new PreAlignProcessingParam();

                    PreAlignProcessingParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    PreAlignProcessingParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    PreAlignProcessingParamVar.LightChannel.Value = 8;
                    PreAlignProcessingParamVar.LightIntensity.Value = 255;
                    PreAlignProcessingParamVar.NotchSensorAngle.Value = -90;
                    PreAlignProcessingParamVar.CameraAngle.Value = 0;
                    PreAlignProcessingParamVar.CameraRatio.Value = 41.66;
                    PreAlignProcessingParamVar.CenteringTolerance.Value = 80;
                    PreAlignProcessingParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_1R;
                    paDef.ProcessingParams.Add(PreAlignProcessingParamVar);

                    // 8 Inch
                    PreAlignProcessingParamVar = new PreAlignProcessingParam();

                    PreAlignProcessingParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    PreAlignProcessingParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    PreAlignProcessingParamVar.LightChannel.Value = 9;
                    PreAlignProcessingParamVar.LightIntensity.Value = 255;
                    PreAlignProcessingParamVar.NotchSensorAngle.Value = -90;
                    PreAlignProcessingParamVar.CameraAngle.Value = 320;
                    PreAlignProcessingParamVar.CameraRatio.Value = 41.66;
                    PreAlignProcessingParamVar.CenteringTolerance.Value = 80;
                    PreAlignProcessingParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_2R;
                    paDef.ProcessingParams.Add(PreAlignProcessingParamVar);
                    //paDef.ProcessingParams.Add(new PreAlignProcessingParam()
                    //{
                    //    SubstrateType = SubstrateTypeEnum.Wafer,
                    //    SubstrateSize = SubstrateSizeEnum.INCH8,
                    //    LightChannel = 0,
                    //    LightIntensity = 255,
                    //    NotchSensorAngle = -90,
                    //    CameraAngle = 320,
                    //    CameraRatio = 25.468,
                    //    CenteringTolerance = 150,
                    //});
                    // 6 Inch

                    var PreAlignAccessParamVar = new PreAlignAccessParam();

                    PreAlignAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    PreAlignAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    PreAlignAccessParamVar.Position = CreateLoaderCoordinate(-202300, 143000, 0, false, 0);
                    PreAlignAccessParamVar.PickupIncrement.Value = 7000;

                    paDef.AccessParams.Add(PreAlignAccessParamVar);

                    // 8 Inch
                    PreAlignAccessParamVar = new PreAlignAccessParam();

                    PreAlignAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    PreAlignAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    PreAlignAccessParamVar.Position = CreateLoaderCoordinate(-202300, 142500, 0, false, 0);
                    PreAlignAccessParamVar.PickupIncrement.Value = 7000;

                    paDef.AccessParams.Add(PreAlignAccessParamVar);
                    //paDef.AccessParams.Add(new PreAlignAccessParam()
                    //{
                    //    SubstrateType = SubstrateTypeEnum.Wafer,
                    //    SubstrateSize = SubstrateSizeEnum.INCH8,
                    //    Position = CreateLoaderCoordinate(-202300, 141500, 0, false, 0),
                    //    PickupIncrement = 7000,
                    //});
                    PreAlignModules.Add(paDef);
                }
                //=> SemicsOCR
                SemicsOCRDefinition semicsOcrDef = new SemicsOCRDefinition();
                semicsOcrDef.DependencyPreAlignNum.Value = 1;
                semicsOcrDef.OCRDirection.Value = OCRDirectionEnum.FRONT;

                var OCRAccessParamVar = new OCRAccessParam();

                OCRAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                OCRAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                OCRAccessParamVar.Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, -17991 + 810, false);
                OCRAccessParamVar.VPos.Value = 0;

                semicsOcrDef.AccessParams.Add(OCRAccessParamVar);

                //semicsOcrDef.AccessParams.Add(new OCRAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, -17991 + 810, false),
                //    VPos = 0,
                //});

                semicsOcrDef.OCRCam.Value = EnumProberCam.OCR1_CAM;
                semicsOcrDef.LightChannel1.Value = 12;
                semicsOcrDef.LightChannel2.Value = 0;
                semicsOcrDef.LightChannel3.Value = 0;
                semicsOcrDef.OcrLight1_Offset.Value = 0;
                semicsOcrDef.OcrLight2_Offset.Value = 0;
                semicsOcrDef.OcrLight3_Offset.Value = 0;
                //semicsOcrDef.UserLightEnable = false;
                //semicsOcrDef.CheckLotNameBeforeOcr = false;
                //semicsOcrDef.CreateLotNameBeforeOcr = false;
                //semicsOcrDef.LotIntegrityEnable = false;
                //semicsOcrDef.SlotIntegrityEnable = false;
                //semicsOcrDef.WaferIntegrityEnable = false;
                //semicsOcrDef.LotPauseAfterReject = false;
                //semicsOcrDef.OcrReadAfterManualInput = false;
                //semicsOcrDef.OcrEditInLotRun = false;
                //semicsOcrDef.OcrThresholdVal = 0;
                //semicsOcrDef.OcrSmooth = false;
                //semicsOcrDef.OcrGradientEnable = false;

                SemicsOCRModules.Add(semicsOcrDef);

                for (int coIndex = 0; coIndex < SystemModuleCount.ModuleCnt.PACount; coIndex++)
                {
                    var cognexParam = new CognexOCRDefinition();
                    cognexParam.DependencyPreAlignNum.Value = 1;
                    cognexParam.OCRDirection.Value = OCRDirectionEnum.FRONT;

                    var OCRAccessParamVar2 = new OCRAccessParam();

                    OCRAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    OCRAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    OCRAccessParamVar2.Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, 0.0, false);
                    OCRAccessParamVar2.VPos.Value = 0;

                    cognexParam.AccessParams.Add(OCRAccessParamVar2);


                    OCRAccessParamVar2 = new OCRAccessParam();

                    OCRAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    OCRAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    OCRAccessParamVar2.Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, 0.0, false);
                    OCRAccessParamVar2.VPos.Value = 0;

                    cognexParam.AccessParams.Add(OCRAccessParamVar2);

                    var SubchuckMotionParamVar = new SubchuckMotionParam();
                    SubchuckMotionParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    SubchuckMotionParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH6;
                    SubchuckMotionParamVar.SubchuckXCoord.Value = 0;
                    SubchuckMotionParamVar.SubchuckYCoord.Value = 30000;
                    SubchuckMotionParamVar.SubchuckAngle_Offset.Value = 0;
                    cognexParam.SubchuckMotionParams.Add(SubchuckMotionParamVar);

                    SubchuckMotionParamVar = new SubchuckMotionParam();
                    SubchuckMotionParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    SubchuckMotionParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    SubchuckMotionParamVar.SubchuckXCoord.Value = 0;
                    SubchuckMotionParamVar.SubchuckYCoord.Value = -20000;
                    SubchuckMotionParamVar.SubchuckAngle_Offset.Value = 0;
                    cognexParam.SubchuckMotionParams.Add(SubchuckMotionParamVar);

                    SubchuckMotionParamVar = new SubchuckMotionParam();
                    SubchuckMotionParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    SubchuckMotionParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    SubchuckMotionParamVar.SubchuckXCoord.Value = 0;
                    SubchuckMotionParamVar.SubchuckYCoord.Value = 0;
                    SubchuckMotionParamVar.SubchuckAngle_Offset.Value = 0;
                    cognexParam.SubchuckMotionParams.Add(SubchuckMotionParamVar);

                    cognexParam.OCRCam.Value = EnumProberCam.OCR1_CAM;
                    CognexOCRModules.Add(cognexParam);
                }


                //=> CHUCK
                //ChuckDefinition chuckDef12 = new ChuckDefinition();

                //var ChuckAccessParamVar = new ChuckAccessParam();

                //ChuckAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                //ChuckAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                //ChuckAccessParamVar.Position = CreateLoaderCoordinate(147200, 140000, -9000, true);
                //ChuckAccessParamVar.PickupIncrement.Value = 0;

                //chuckDef12.AccessParams.Add(ChuckAccessParamVar);

                //chuckDef12.AccessParams.Add(new ChuckAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(147200, 140000, -9000, true),
                //    PickupIncrement = 0,
                //});

                for (int stageIndex = 0; stageIndex < GPLoaderDef.StageCount; stageIndex++)
                {
                    ChuckDefinition chuckDef = new ChuckDefinition();

                    var ChuckAccessParamVar2 = new ChuckAccessParam();

                    // 8 Inch
                    ChuckAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    ChuckAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    ChuckAccessParamVar2.Position = CreateLoaderCoordinate(156000, 145000, -9000, true);
                    ChuckAccessParamVar2.PickupIncrement.Value = 0;
                    chuckDef.AccessParams.Add(ChuckAccessParamVar2);

                    // 12 Inch
                    //ChuckDefinition chuckDef8 = new ChuckDefinition();
                    ChuckAccessParamVar2 = new ChuckAccessParam();
                    ChuckAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    ChuckAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    ChuckAccessParamVar2.Position = CreateLoaderCoordinate(156000, 145000, -9000, true);
                    ChuckAccessParamVar2.PickupIncrement.Value = 0;
                    chuckDef.AccessParams.Add(ChuckAccessParamVar2);
                    ChuckModules.Add(chuckDef);
                }

                //=> FixedTray1
                for (int ftIndex = 0; ftIndex < GPLoaderDef.FTCount; ftIndex++)
                {
                    FixedTrayDefinition fixedDef = new FixedTrayDefinition();
                    fixedDef.IOCheckDelayTimeout.Value = 500;
                    fixedDef.DIWAFERONMODULE.Value = $"DIFixTrays.{ftIndex}";
                    fixedDef.DI6INCHWAFERONMODULE.Value = $"DI6inchWaferOnFixTs.{ftIndex}";
                    fixedDef.DI8INCHWAFERONMODULE.Value = $"DI8inchWaferOnFixTs.{ftIndex}";

                    var fixedTrayAccessParamVar = new FixedTrayAccessParam();
                    fixedTrayAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    fixedTrayAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    fixedTrayAccessParamVar.Position = CreateLoaderCoordinate(198000 + ftIndex * 250000, 166000, -17991, false);
                    fixedTrayAccessParamVar.PickupIncrement.Value = 12000;

                    fixedDef.AccessParams.Add(fixedTrayAccessParamVar);

                    fixedTrayAccessParamVar = new FixedTrayAccessParam();
                    fixedTrayAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    fixedTrayAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    fixedTrayAccessParamVar.Position = CreateLoaderCoordinate(198000 + ftIndex * 250000, 166000, -17991, false);
                    fixedTrayAccessParamVar.PickupIncrement.Value = 12000;

                    fixedDef.AccessParams.Add(fixedTrayAccessParamVar);
                    FixedTrayModules.Add(fixedDef);
                }
                //=> Buffer
                for (int buffIndex = 0; buffIndex < GPLoaderDef.BufferCount; buffIndex++)
                {
                    BufferDefinition buffDef = new BufferDefinition();
                    buffDef.IOCheckDelayTimeout.Value = 500;
                    buffDef.DIWAFERONMODULE.Value = $"DIBUFVACS.{buffIndex}";
                    buffDef.DOAIRON.Value = $"DOBuffVacs.{buffIndex}";

                    var buffAccessParamVar = new BufferAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;

                    buffDef.AccessParams.Add(buffAccessParamVar);
                    buffAccessParamVar = new BufferAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;
                    buffDef.Enable.Value = true;
                    buffDef.AccessParams.Add(buffAccessParamVar);
                    BufferModules.Add(buffDef);
                }

                //=> CardBufferTray
                for (int buffIndex = 0; buffIndex < GPLoaderDef.CardBufferTrayCount; buffIndex++)
                {
                    CardBufferTrayDefinition cbuffDef = new CardBufferTrayDefinition();
                    cbuffDef.IOCheckDelayTimeout.Value = 500;
                    cbuffDef.DICARDONMODULE.Value = $"DICardBuffs.{buffIndex + 4}";
                    cbuffDef.DIDRAWERSENSOR.Value = $"DICardBuffs.{buffIndex + 13}";
                    var buffAccessParamVar = new CardBufferTrayAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Card;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;

                    cbuffDef.AccessParams.Add(buffAccessParamVar);

                    buffAccessParamVar = new CardBufferTrayAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Card;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;

                    cbuffDef.AccessParams.Add(buffAccessParamVar);


                    CardBufferTrayModules.Add(cbuffDef);
                }

                //=> CC
                for (int buffIndex = 0; buffIndex < GPLoaderDef.StageCount; buffIndex++)
                {
                    CCDefinition cbuffDef = new CCDefinition();

                    var buffAccessParamVar = new CCAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Card;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;
                    cbuffDef.AccessParams.Add(buffAccessParamVar);

                    buffAccessParamVar = new CCAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Card;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;

                    cbuffDef.AccessParams.Add(buffAccessParamVar);
                    CCModules.Add(cbuffDef);
                }
                for (int buffIndex = 0; buffIndex < GPLoaderDef.CardBufferCount; buffIndex++)
                {
                    CardBufferDefinition cbuffDef = new CardBufferDefinition();
                    cbuffDef.IOCheckDelayTimeout.Value = 500;
                    cbuffDef.DICARDONMODULE.Value = $"DICardBuffs.{buffIndex}";

                    var buffAccessParamVar = new CardBufferAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Card;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;

                    cbuffDef.AccessParams.Add(buffAccessParamVar);


                    buffAccessParamVar = new CardBufferAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Card;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;

                    cbuffDef.AccessParams.Add(buffAccessParamVar);
                    CardBufferModules.Add(cbuffDef);
                }

                //  => InspectionTray
                for (int InspIndex = 0; InspIndex < GPLoaderDef.ITCount; InspIndex++)
                {
                    InspectionTrayDefinition insDef = new InspectionTrayDefinition();
                    insDef.IsInterferenceWithCassettePort.Value = false;
                    insDef.IOCheckDelayTimeout.Value = 500;
                    insDef.DIWAFERONMODULE.Value = $"DIWaferOnInSPs.{InspIndex}";
                    insDef.DIOPENDED.Value = $"DIOpenInSPs.{InspIndex}";
                    insDef.DIMOVED.Value = $"DIMovedInSPs.{InspIndex}";
                    insDef.DI6INCHWAFERONMODULE.Value = $"DI6inchWaferOnInSPs.{InspIndex}";
                    insDef.DI8INCHWAFERONMODULE.Value = $"DI8inchWaferOnInSPs.{InspIndex}";

                    var InspectionTrayAccessParamVar = new InspectionTrayAccessParam();

                    InspectionTrayAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    InspectionTrayAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    InspectionTrayAccessParamVar.Position = CreateLoaderCoordinate(-64441, 140000, 0, false, 0);
                    InspectionTrayAccessParamVar.PickupIncrement.Value = 3800;

                    insDef.AccessParams.Add(InspectionTrayAccessParamVar);

                    InspectionTrayModules.Add(insDef);
                }

                this.UseLotProcessingVerify.Value = false;
                this.DefaultWaferSize.Value = SubstrateSizeEnum.INCH12;
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private EventCodeEnum SetDefaultParamGPLoader_OPRT()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                //=> Loader Setting
                UExtension = new UExtensionCylinder();
                var UExtensionVar = (UExtension as UExtensionCylinder);

                UExtensionVar.IOWaitTimeout.Value = 10000;
                UExtensionVar.DOARMINAIR.Value = "DOARMINAIR";
                UExtensionVar.DOARMOUTAIR.Value = "DOARMOUTAIR";
                UExtensionVar.DIARMIN.Value = "DIARMIN";
                UExtensionVar.DIARMOUT.Value = "DIARMOUT";
                //=> CameraScan
                //CameraScanDefinition def = new CameraScanDefinition();
                //sysParam.CameraScanModules.Add(def);
                //=> SensorScan
                ScanSensorDefinition scanSensorDef = new ScanSensorDefinition();
                scanSensorDef.DOSCAN_SENSOR_OUT.Value = "DOSCAN_SENSOR_OUT";
                scanSensorDef.DISCAN_SENSOR_OUT.Value = "DISCAN_SENSOR_OUT";
                scanSensorDef.DISCAN_SENSOR_IN.Value = "DISCAN_SENSOR_IN";
                scanSensorDef.IOCheckDelayTimeout.Value = 500;
                scanSensorDef.IOWaitTimeout.Value = 60000;

                var ScanSensorParamVar = new ScanSensorParam();
                // 6inch scan
                ScanSensorParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_2R;
                ScanSensorParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                ScanSensorParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH6;
                ScanSensorParamVar.SensorOffset.Value = 0;
                ScanSensorParamVar.UpOffset.Value = 3000;
                ScanSensorParamVar.DownOffset.Value = -2500;
                ScanSensorParamVar.InSlotLowerRatio.Value = 0.40;
                ScanSensorParamVar.InSlotUpperRatio.Value = 0.50;
                ScanSensorParamVar.ThicknessTol.Value = 3;
                ScanSensorParamVar.ScanAxis.Value = EnumAxisConstants.SC;

                scanSensorDef.ScanParams.Add(ScanSensorParamVar);

                // 8inch scan
                ScanSensorParamVar = new ScanSensorParam();

                ScanSensorParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_2R;
                ScanSensorParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                ScanSensorParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                ScanSensorParamVar.SensorOffset.Value = 0;
                ScanSensorParamVar.UpOffset.Value = 3000;
                ScanSensorParamVar.DownOffset.Value = -2500;
                ScanSensorParamVar.InSlotLowerRatio.Value = 0.35;
                ScanSensorParamVar.InSlotUpperRatio.Value = 0.65;
                ScanSensorParamVar.ThicknessTol.Value = 2.5;
                ScanSensorParamVar.ScanAxis.Value = EnumAxisConstants.SC;

                scanSensorDef.ScanParams.Add(ScanSensorParamVar);

                //scanSensorDef.ScanParams.Add(new ScanSensorParam()
                //{
                //    SensorInputPort = EnumMotorDedicatedIn.MotorDedicatedIn_2R,
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH8,
                //    SensorOffset = 0,
                //    UpOffset = 3000,
                //    DownOffset = -2500,
                //    InSlotLowerRatio = 0.35,
                //    InSlotUpperRatio = 0.75,
                //    ThicknessTol = 1.5,
                //    ScanAxis = EnumAxisConstants.SC

                //});
                ScanSensorModules.Add(scanSensorDef);

                //=> Cassette
                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                {
                    CassetteDefinition cassetteDef = new CassetteDefinition();
                    cassetteDef.ScanModuleType.Value = ModuleTypeEnum.SCANSENSOR;

                    for (int slotIndex = 1; slotIndex <= 25; slotIndex++)
                    {
                        cassetteDef.SlotModules.Add(new SlotDefinition());
                    }

                    // 6 Inch Slot #1

                    // 8 Inch Slot #1
                    var CassetteSlot1AccessParamVar = new CassetteSlot1AccessParam();

                    CassetteSlot1AccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    CassetteSlot1AccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    CassetteSlot1AccessParamVar.Position = CreateLoaderCoordinate(22000, 145500, 0, false, 28740);
                    CassetteSlot1AccessParamVar.UStopPosOffset.Value = 200;
                    CassetteSlot1AccessParamVar.PickupIncrement.Value = 3800;

                    cassetteDef.Slot1AccessParams.Add(CassetteSlot1AccessParamVar);

                    // 12 Inch Slot #1
                    CassetteSlot1AccessParamVar = new CassetteSlot1AccessParam();

                    CassetteSlot1AccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    CassetteSlot1AccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    CassetteSlot1AccessParamVar.Position = CreateLoaderCoordinate(22000, 145500, 0, false, 28740);
                    CassetteSlot1AccessParamVar.UStopPosOffset.Value = 200;
                    CassetteSlot1AccessParamVar.PickupIncrement.Value = 10000;

                    cassetteDef.Slot1AccessParams.Add(CassetteSlot1AccessParamVar);

                    CassetteModules.Add(cassetteDef);
                }


                //=> ARM1
                ARMDefinition armDef = new ARMDefinition();
                armDef.AxisType.Value = EnumAxisConstants.LUD;
                armDef.EndOffset.Value = 0;
                armDef.UpOffset.Value = 0;
                armDef.DIWAFERONMODULE.Value = "DIARM1VAC";
                armDef.DOAIRON.Value = "DOARM1Vac";
                armDef.UseType.Value = ARMUseTypeEnum.BOTH;
                armDef.IOCheckMaintainTime.Value = 100;
                armDef.IOCheckDelayTimeout.Value = 500;
                armDef.IOWaitTimeout.Value = 10000;
                ARMModules.Add(armDef);

                //=> ARM2
                armDef = new ARMDefinition();
                armDef.AxisType.Value = EnumAxisConstants.LUU;
                armDef.EndOffset.Value = 0;
                armDef.UpOffset.Value = 16900;
                armDef.DIWAFERONMODULE.Value = "DIARM2VAC";
                armDef.DOAIRON.Value = "DOARMVac2";
                armDef.UseType.Value = ARMUseTypeEnum.BOTH;
                armDef.IOCheckMaintainTime.Value = 100;
                armDef.IOCheckDelayTimeout.Value = 500;
                armDef.IOWaitTimeout.Value = 10000;
                ARMModules.Add(armDef);

                //=> CARDARM
                CardARMDefinition CardARMDef = new CardARMDefinition();
                CardARMDef = new CardARMDefinition();
                CardARMDef.AxisType.Value = EnumAxisConstants.LCC;
                CardARMDef.EndOffset.Value = 0;
                CardARMDef.UpOffset.Value = 16900;
                CardARMDef.DIWAFERONMODULE.Value = "DICCARMVAC";
                CardARMDef.DOAIRON.Value = "DOCCArmVac";
                CardARMDef.DOAIROFF.Value = "DOCCArmVac_Break";
                CardARMDef.UseType.Value = ARMUseTypeEnum.BOTH;
                CardARMDef.IOCheckMaintainTime.Value = 100;
                CardARMDef.IOCheckDelayTimeout.Value = 500;
                CardARMDef.IOWaitTimeout.Value = 10000;
                CardARMModules.Add(CardARMDef);


                //=> PA1
                for (int i = 0; i < SystemModuleCount.ModuleCnt.PACount; i++)
                {
                    PreAlignDefinition paDef = new PreAlignDefinition();
                    paDef.AxisType.Value = EnumAxisConstants.V;
                    paDef.IOCheckDelayTimeout.Value = 500;
                    paDef.IOWaitTimeout.Value = 10000;
                    paDef.DIWAFERONMODULE.Value = "DIWAFERONSUBCHUCK";
                    paDef.DOAIRON.Value = "DOSUBCHUCKAIRON";

                    paDef.RetryCount.Value = 5; //TODO : => Processing Param으로 이동

                    // 6 Inch
                    var PreAlignProcessingParamVar = new PreAlignProcessingParam();

                    PreAlignProcessingParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    PreAlignProcessingParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    PreAlignProcessingParamVar.LightChannel.Value = 8;
                    PreAlignProcessingParamVar.LightIntensity.Value = 255;
                    PreAlignProcessingParamVar.NotchSensorAngle.Value = -90;
                    PreAlignProcessingParamVar.CameraAngle.Value = 0;
                    PreAlignProcessingParamVar.CameraRatio.Value = 41.66;
                    PreAlignProcessingParamVar.CenteringTolerance.Value = 80;
                    PreAlignProcessingParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_1R;
                    paDef.ProcessingParams.Add(PreAlignProcessingParamVar);

                    // 8 Inch
                    PreAlignProcessingParamVar = new PreAlignProcessingParam();

                    PreAlignProcessingParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    PreAlignProcessingParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    PreAlignProcessingParamVar.LightChannel.Value = 9;
                    PreAlignProcessingParamVar.LightIntensity.Value = 255;
                    PreAlignProcessingParamVar.NotchSensorAngle.Value = -90;
                    PreAlignProcessingParamVar.CameraAngle.Value = 320;
                    PreAlignProcessingParamVar.CameraRatio.Value = 41.66;
                    PreAlignProcessingParamVar.CenteringTolerance.Value = 80;
                    PreAlignProcessingParamVar.SensorInputPort.Value = EnumMotorDedicatedIn.MotorDedicatedIn_2R;
                    paDef.ProcessingParams.Add(PreAlignProcessingParamVar);
                    //paDef.ProcessingParams.Add(new PreAlignProcessingParam()
                    //{
                    //    SubstrateType = SubstrateTypeEnum.Wafer,
                    //    SubstrateSize = SubstrateSizeEnum.INCH8,
                    //    LightChannel = 0,
                    //    LightIntensity = 255,
                    //    NotchSensorAngle = -90,
                    //    CameraAngle = 320,
                    //    CameraRatio = 25.468,
                    //    CenteringTolerance = 150,
                    //});
                    // 6 Inch

                    var PreAlignAccessParamVar = new PreAlignAccessParam();

                    PreAlignAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    PreAlignAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    PreAlignAccessParamVar.Position = CreateLoaderCoordinate(-202300, 143000, 0, false, 0);
                    PreAlignAccessParamVar.PickupIncrement.Value = 7000;

                    paDef.AccessParams.Add(PreAlignAccessParamVar);

                    // 8 Inch
                    PreAlignAccessParamVar = new PreAlignAccessParam();

                    PreAlignAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    PreAlignAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    PreAlignAccessParamVar.Position = CreateLoaderCoordinate(-202300, 142500, 0, false, 0);
                    PreAlignAccessParamVar.PickupIncrement.Value = 7000;

                    paDef.AccessParams.Add(PreAlignAccessParamVar);
                    //paDef.AccessParams.Add(new PreAlignAccessParam()
                    //{
                    //    SubstrateType = SubstrateTypeEnum.Wafer,
                    //    SubstrateSize = SubstrateSizeEnum.INCH8,
                    //    Position = CreateLoaderCoordinate(-202300, 141500, 0, false, 0),
                    //    PickupIncrement = 7000,
                    //});
                    PreAlignModules.Add(paDef);
                }
                //=> SemicsOCR
                SemicsOCRDefinition semicsOcrDef = new SemicsOCRDefinition();
                semicsOcrDef.DependencyPreAlignNum.Value = 1;
                semicsOcrDef.OCRDirection.Value = OCRDirectionEnum.FRONT;

                var OCRAccessParamVar = new OCRAccessParam();

                OCRAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                OCRAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                OCRAccessParamVar.Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, -17991 + 810, false);
                OCRAccessParamVar.VPos.Value = 0;

                semicsOcrDef.AccessParams.Add(OCRAccessParamVar);

                //semicsOcrDef.AccessParams.Add(new OCRAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, -17991 + 810, false),
                //    VPos = 0,
                //});

                semicsOcrDef.OCRCam.Value = EnumProberCam.OCR1_CAM;
                semicsOcrDef.LightChannel1.Value = 12;
                semicsOcrDef.LightChannel2.Value = 0;
                semicsOcrDef.LightChannel3.Value = 0;
                semicsOcrDef.OcrLight1_Offset.Value = 0;
                semicsOcrDef.OcrLight2_Offset.Value = 0;
                semicsOcrDef.OcrLight3_Offset.Value = 0;
                //semicsOcrDef.UserLightEnable = false;
                //semicsOcrDef.CheckLotNameBeforeOcr = false;
                //semicsOcrDef.CreateLotNameBeforeOcr = false;
                //semicsOcrDef.LotIntegrityEnable = false;
                //semicsOcrDef.SlotIntegrityEnable = false;
                //semicsOcrDef.WaferIntegrityEnable = false;
                //semicsOcrDef.LotPauseAfterReject = false;
                //semicsOcrDef.OcrReadAfterManualInput = false;
                //semicsOcrDef.OcrEditInLotRun = false;
                //semicsOcrDef.OcrThresholdVal = 0;
                //semicsOcrDef.OcrSmooth = false;
                //semicsOcrDef.OcrGradientEnable = false;

                SemicsOCRModules.Add(semicsOcrDef);

                for (int coIndex = 0; coIndex < SystemModuleCount.ModuleCnt.PACount; coIndex++)
                {
                    var cognexParam = new CognexOCRDefinition();
                    cognexParam.DependencyPreAlignNum.Value = 1;
                    cognexParam.OCRDirection.Value = OCRDirectionEnum.FRONT;

                    var OCRAccessParamVar2 = new OCRAccessParam();

                    OCRAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    OCRAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    OCRAccessParamVar2.Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, 0.0, false);
                    OCRAccessParamVar2.VPos.Value = 0;

                    cognexParam.AccessParams.Add(OCRAccessParamVar2);


                    OCRAccessParamVar2 = new OCRAccessParam();

                    OCRAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    OCRAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    OCRAccessParamVar2.Position = CreateLoaderCoordinate(-202300 + 7000, 166511.87 - 46500, 0.0, false);
                    OCRAccessParamVar2.VPos.Value = 0;

                    cognexParam.AccessParams.Add(OCRAccessParamVar2);

                    cognexParam.OCRCam.Value = EnumProberCam.OCR1_CAM;
                    CognexOCRModules.Add(cognexParam);
                }


                //=> CHUCK
                //ChuckDefinition chuckDef12 = new ChuckDefinition();

                //var ChuckAccessParamVar = new ChuckAccessParam();

                //ChuckAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                //ChuckAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                //ChuckAccessParamVar.Position = CreateLoaderCoordinate(147200, 140000, -9000, true);
                //ChuckAccessParamVar.PickupIncrement.Value = 0;

                //chuckDef12.AccessParams.Add(ChuckAccessParamVar);

                //chuckDef12.AccessParams.Add(new ChuckAccessParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //    Position = CreateLoaderCoordinate(147200, 140000, -9000, true),
                //    PickupIncrement = 0,
                //});

                for (int stageIndex = 0; stageIndex < GPLoaderDef.StageCount; stageIndex++)
                {
                    ChuckDefinition chuckDef = new ChuckDefinition();

                    var ChuckAccessParamVar2 = new ChuckAccessParam();

                    // 8 Inch
                    ChuckAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    ChuckAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    ChuckAccessParamVar2.Position = CreateLoaderCoordinate(156000, 145000, -9000, true);
                    ChuckAccessParamVar2.PickupIncrement.Value = 0;
                    chuckDef.AccessParams.Add(ChuckAccessParamVar2);

                    // 12 Inch
                    //ChuckDefinition chuckDef8 = new ChuckDefinition();
                    ChuckAccessParamVar2 = new ChuckAccessParam();
                    ChuckAccessParamVar2.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    ChuckAccessParamVar2.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    ChuckAccessParamVar2.Position = CreateLoaderCoordinate(156000, 145000, -9000, true);
                    ChuckAccessParamVar2.PickupIncrement.Value = 0;
                    chuckDef.AccessParams.Add(ChuckAccessParamVar2);
                    ChuckModules.Add(chuckDef);
                }

                //=> FixedTray1
                for (int ftIndex = 0; ftIndex < GPLoaderDef.FTCount; ftIndex++)
                {
                    FixedTrayDefinition fixedDef = new FixedTrayDefinition();
                    fixedDef.IOCheckDelayTimeout.Value = 500;
                    fixedDef.DIWAFERONMODULE.Value = $"DIFixTrays.{ftIndex}";

                    var fixedTrayAccessParamVar = new FixedTrayAccessParam();
                    fixedTrayAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    fixedTrayAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    fixedTrayAccessParamVar.Position = CreateLoaderCoordinate(198000 + ftIndex * 250000, 166000, -17991, false);
                    fixedTrayAccessParamVar.PickupIncrement.Value = 12000;

                    fixedDef.AccessParams.Add(fixedTrayAccessParamVar);

                    fixedTrayAccessParamVar = new FixedTrayAccessParam();
                    fixedTrayAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    fixedTrayAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    fixedTrayAccessParamVar.Position = CreateLoaderCoordinate(198000 + ftIndex * 250000, 166000, -17991, false);
                    fixedTrayAccessParamVar.PickupIncrement.Value = 12000;

                    fixedDef.AccessParams.Add(fixedTrayAccessParamVar);
                    FixedTrayModules.Add(fixedDef);
                }
                //=> Buffer
                for (int buffIndex = 0; buffIndex < GPLoaderDef.BufferCount; buffIndex++)
                {
                    BufferDefinition buffDef = new BufferDefinition();
                    buffDef.IOCheckDelayTimeout.Value = 500;
                    buffDef.DIWAFERONMODULE.Value = $"DIBUFVACS.{buffIndex}";
                    buffDef.DOAIRON.Value = $"DOBuffVacs.{buffIndex}";

                    var buffAccessParamVar = new BufferAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;

                    buffDef.AccessParams.Add(buffAccessParamVar);
                    buffAccessParamVar = new BufferAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;
                    buffDef.Enable.Value = true;
                    buffDef.AccessParams.Add(buffAccessParamVar);
                    BufferModules.Add(buffDef);
                }

                //=> CardBufferTray
                for (int buffIndex = 0; buffIndex < GPLoaderDef.CardBufferTrayCount; buffIndex++)
                {
                    CardBufferTrayDefinition cbuffDef = new CardBufferTrayDefinition();
                    cbuffDef.IOCheckDelayTimeout.Value = 500;
                    cbuffDef.DICARDONMODULE_DOWN.Value = $"DICardBuffs.{buffIndex}";
                    cbuffDef.DICARDONMODULE.Value = $"DICardBuffs.{buffIndex + 5}";
                    cbuffDef.DIDRAWERSENSOR.Value = $"DICardBuffs.{buffIndex + 10}";
                    cbuffDef.DICARDATTACHHOLDER.Value = $"DICardBuffs.{buffIndex + 15}";
                    var buffAccessParamVar = new CardBufferTrayAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Card;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;

                    cbuffDef.AccessParams.Add(buffAccessParamVar);

                    buffAccessParamVar = new CardBufferTrayAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Card;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;

                    cbuffDef.AccessParams.Add(buffAccessParamVar);


                    CardBufferTrayModules.Add(cbuffDef);
                }

                //=> CC
                for (int buffIndex = 0; buffIndex < GPLoaderDef.StageCount; buffIndex++)
                {
                    CCDefinition cbuffDef = new CCDefinition();

                    var buffAccessParamVar = new CCAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Card;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;
                    cbuffDef.AccessParams.Add(buffAccessParamVar);

                    buffAccessParamVar = new CCAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Card;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;

                    cbuffDef.AccessParams.Add(buffAccessParamVar);
                    CCModules.Add(cbuffDef);
                }
                for (int buffIndex = 0; buffIndex < GPLoaderDef.CardBufferCount; buffIndex++)
                {
                    CardBufferDefinition cbuffDef = new CardBufferDefinition();
                    cbuffDef.IOCheckDelayTimeout.Value = 500;
                    cbuffDef.DICARDONMODULE.Value = $"DICardBuffs.{buffIndex}";

                    var buffAccessParamVar = new CardBufferAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Card;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;

                    cbuffDef.AccessParams.Add(buffAccessParamVar);


                    buffAccessParamVar = new CardBufferAccessParam();

                    buffAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Card;
                    buffAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    buffAccessParamVar.Position = CreateLoaderCoordinate(198000, 166000, -17991, 0 + buffIndex * 10000);
                    buffAccessParamVar.PickupIncrement.Value = 12000;

                    cbuffDef.AccessParams.Add(buffAccessParamVar);
                    CardBufferModules.Add(cbuffDef);
                }




                //  => InspectionTray
                for (int InspIndex = 0; InspIndex < GPLoaderDef.ITCount; InspIndex++)
                {
                    InspectionTrayDefinition insDef = new InspectionTrayDefinition();
                    insDef.IsInterferenceWithCassettePort.Value = false;
                    insDef.IOCheckDelayTimeout.Value = 500;
                    insDef.DIWAFERONMODULE.Value = $"DIWaferOnInSPs.{InspIndex}";
                    insDef.DIOPENDED.Value = $"DIOpenInSPs.{InspIndex}";
                    insDef.DIMOVED.Value = $"DIMovedInSPs.{InspIndex}";

                    var InspectionTrayAccessParamVar = new InspectionTrayAccessParam();

                    InspectionTrayAccessParamVar.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    InspectionTrayAccessParamVar.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    InspectionTrayAccessParamVar.Position = CreateLoaderCoordinate(-64441, 140000, 0, false, 0);
                    InspectionTrayAccessParamVar.PickupIncrement.Value = 3800;

                    insDef.AccessParams.Add(InspectionTrayAccessParamVar);


                    InspectionTrayModules.Add(insDef);
                }
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private LoaderCoordinate CreateLoaderCoordinate(double A, double U, double W, bool E)
        {
            LoaderCoordinate pos = new LoaderCoordinate();
            try
            {
                pos.A.Value = A;
                pos.U.Value = U;
                pos.W.Value = W;

                //pos.E.Value = new UExtensionCylinderMoveParam() { Value = E };

                pos.E.Value = new UExtensionCylinderMoveParam();

                var UExtensionCylinderMoveParamVar = (pos.E.Value as UExtensionCylinderMoveParam);

                UExtensionCylinderMoveParamVar.Port.Value = E;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return pos;
        }

        private LoaderCoordinate CreateLoaderCoordinate(double A, double U, double W, bool E, double SC)
        {
            LoaderCoordinate pos = new LoaderCoordinate();
            try
            {
                pos.A.Value = A;
                pos.U.Value = U;
                pos.W.Value = W;

                //pos.E = new UExtensionCylinderMoveParam() { Port = E };

                pos.E.Value = new UExtensionCylinderMoveParam();

                var UExtensionCylinderMoveParamVar = (pos.E.Value as UExtensionCylinderMoveParam);

                UExtensionCylinderMoveParamVar.Port.Value = E;

                pos.SC.Value = SC;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return pos;
        }
        private LoaderCoordinate CreateLoaderCoordinate(double A, double U, double W, double BT)
        {
            LoaderCoordinate pos = new LoaderCoordinate();
            try
            {
                pos.A.Value = A;
                pos.U.Value = U;
                pos.W.Value = W;
                pos.BT.Value = BT;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return pos;
        }

        public SubstrateSizeEnum GetMaxWaferSizeHandledByLoader()
        {
            SubstrateSizeEnum Size = SubstrateSizeEnum.UNDEFINED;
            try
            {
                if (this.ChuckModules != null && this.ChuckModules.Count > 0)
                {
                    var maxSubstrateSize = this.ChuckModules
                        .SelectMany(module => module.AccessParams)
                        .Where(accessParam => accessParam.SubstrateSize.Value != SubstrateSizeEnum.INVALID && accessParam.SubstrateSize.Value != SubstrateSizeEnum.UNDEFINED)
                        .Select(accessParam => accessParam.SubstrateSize.Value)
                        .Max();

                    Size = maxSubstrateSize;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Size;
        }
    }
}

