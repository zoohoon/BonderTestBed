using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using ProberErrorCode;
using LogModule;
using Newtonsoft.Json;
using ProberInterfaces.Enum;

namespace LoaderParameters
{
    /// <summary>
    /// LoaderDeviceParameter 을 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class LoaderDeviceParameter : INotifyPropertyChanged, ICloneable, IDeviceParameterizable
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
        public string FileName { get; } = "LoaderDevice.json";


        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                //RetVal = SetDefaultParamOPUSVTestMahcine();
                SetDefaultParamGPL();
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

        private ObservableCollection<ScanCameraDevice> _ScanCameraModules = new ObservableCollection<ScanCameraDevice>();
        /// <summary>
        /// ScanCameraModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<ScanCameraDevice> ScanCameraModules
        {
            get { return _ScanCameraModules; }
            set { _ScanCameraModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ScanSensorDevice> _ScanSensorModules = new ObservableCollection<ScanSensorDevice>();
        /// <summary>
        /// ScanSensorModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<ScanSensorDevice> ScanSensorModules
        {
            get { return _ScanSensorModules; }
            set { _ScanSensorModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<CassetteDevice> _CassetteModules = new ObservableCollection<CassetteDevice>();
        /// <summary>
        /// CassetteModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CassetteDevice> CassetteModules
        {
            get { return _CassetteModules; }
            set { _CassetteModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ARMDevice> _ARMModules = new ObservableCollection<ARMDevice>();
        /// <summary>
        /// ARMModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<ARMDevice> ARMModules
        {
            get { return _ARMModules; }
            set { _ARMModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<PreAlignDevice> _PreAlignModules = new ObservableCollection<PreAlignDevice>();
        /// <summary>
        /// PreAlignModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<PreAlignDevice> PreAlignModules
        {
            get { return _PreAlignModules; }
            set { _PreAlignModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<CognexOCRDevice> _CognexOCRModules = new ObservableCollection<CognexOCRDevice>();
        /// <summary>
        /// CognexOCRModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CognexOCRDevice> CognexOCRModules
        {
            get { return _CognexOCRModules; }
            set { _CognexOCRModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<SemicsOCRDevice> _SemicsOCRModules = new ObservableCollection<SemicsOCRDevice>();
        /// <summary>
        /// SemicsOCRModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<SemicsOCRDevice> SemicsOCRModules
        {
            get { return _SemicsOCRModules; }
            set { _SemicsOCRModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ChuckDevice> _ChuckModules = new ObservableCollection<ChuckDevice>();
        /// <summary>
        /// ChuckModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<ChuckDevice> ChuckModules
        {
            get { return _ChuckModules; }
            set { _ChuckModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<FixedTrayDevice> _FixedTrayModules = new ObservableCollection<FixedTrayDevice>();
        /// <summary>
        /// FixedTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<FixedTrayDevice> FixedTrayModules
        {
            get { return _FixedTrayModules; }
            set { _FixedTrayModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<InspectionTrayDevice> _InspectionTrayModules = new ObservableCollection<InspectionTrayDevice>();
        /// <summary>
        /// InspectionTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<InspectionTrayDevice> InspectionTrayModules
        {
            get { return _InspectionTrayModules; }
            set { _InspectionTrayModules = value; RaisePropertyChanged(); }
        }


        private ObservableCollection<CCDevice> _CCModules = new ObservableCollection<CCDevice>();
        /// <summary>
        /// InspectionTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CCDevice> CCModules
        {
            get { return _CCModules; }
            set { _CCModules = value; RaisePropertyChanged(); }
        }

        /// <summary>
        private ObservableCollection<CardBufferDevice> _CardBufferModules = new ObservableCollection<CardBufferDevice>();
        /// <summary>
        /// InspectionTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CardBufferDevice> CardBufferModules
        {
            get { return _CardBufferModules; }
            set { _CardBufferModules = value; RaisePropertyChanged(); }
        }


        private ObservableCollection<CardBufferTrayDevice> _CardBufferTrayModules = new ObservableCollection<CardBufferTrayDevice>();
        /// <summary>
        /// InspectionTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CardBufferTrayDevice> CardBufferTrayModules
        {
            get { return _CardBufferTrayModules; }
            set { _CardBufferTrayModules = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<CardARMDevice> _CardArmModules = new ObservableCollection<CardARMDevice>();
        /// <summary>
        /// InspectionTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<CardARMDevice> CardArmModules
        {
            get { return _CardArmModules; }
            set { _CardArmModules = value; RaisePropertyChanged(); }
        }
        /// GetImplTypes을 반환합니다.
        /// </summary>
        /// <returns>Type Array</returns>
        public static Type[] GetImplTypes()
        {
            return new Type[]
            {
                //add abstract class impl
            };
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            var shallowClone = MemberwiseClone() as LoaderDeviceParameter;

            try
            {
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
                shallowClone.CardArmModules = CardArmModules.CloneFrom();
                shallowClone.CardBufferModules = CardBufferModules.CloneFrom();
                shallowClone.CardBufferTrayModules = CardBufferTrayModules.CloneFrom();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return shallowClone;
        }
        
        private EventCodeEnum SetDefaultParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                //=> CameraScan
                //CameraScanDevice camScanDev = new CameraScanDevice();
                //devParam.CameraScanModules.Add(camScanDev);

                //=> SensorScan
                ScanSensorDevice sensorScanDev = new ScanSensorDevice();
                ScanSensorModules.Add(sensorScanDev);

                //=> Cassette
                var cstDev = new CassetteDevice();
                cstDev.SlotSize.Value = 6354;
                cstDev.WaferThickness.Value = 500;
                cstDev.LoadingNotchAngle.Value = 0;
                
                cstDev.AllocateDeviceInfo = new TransferObject();
                cstDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                cstDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH8;
                cstDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.STANDARD;
                cstDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.READ;
                cstDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.COGNEX;
                cstDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.FRONT;

                //cstDev.AllocateDeviceInfo = new TransferObjectDeviceInfo()
                //{
                //    Type = SubstrateTypeEnum.Wafer,
                //    Size = SubstrateSizeEnum.INCH8,
                //    WaferType = EnumWaferType.STANDARD,
                //    OCRMode = OCRModeEnum.READ,
                //    OCRType = OCRTypeEnum.COGNEX,
                //    OCRDirection = OCRDirectionEnum.FRONT,
                //};

                for (int i = 0; i < 25; i++)
                {
                    var SlotDeviceVar = new SlotDevice();

                    SlotDeviceVar.IsOverrideEnable.Value = false;

                    cstDev.SlotModules.Add(SlotDeviceVar);

                    //cstDev.SlotModules.Value.Add(new SlotDevice()
                    //{
                    //    IsOverrideEnable = false,
                    //});
                }
                CassetteModules.Add(cstDev);

                //=> ARM1
                ARMDevice armDev = new ARMDevice();

                ARMModules.Add(armDev);

                //=> ARM2
                armDev = new ARMDevice();
                ARMModules.Add(armDev);

                //=> PreAlign1
                PreAlignDevice preDev = new PreAlignDevice();
                //preDev.Label = "PRE";
                PreAlignModules.Add(preDev);


                //=>SemicsOCR
                var semicsOcrDev = new SemicsOCRDevice();
                semicsOcrDev.OffsetU.Value = 0;
                semicsOcrDev.OffsetW.Value = 0;
                semicsOcrDev.OffsetV.Value = -27850;
                semicsOcrDev.UserLightEnable.Value = true;
                semicsOcrDev.LotIntegrityEnable.Value = false;
                semicsOcrDev.SlotIntegrityEnable.Value = false;
                semicsOcrDev.WaferIntegrityEnable.Value = false;
                semicsOcrDev.OcrAdvancedReadEnable.Value = true;
                semicsOcrDev.OcrConfirmEnable.Value = false;
                semicsOcrDev.OcrCheckSumEnable.Value = true;
                semicsOcrDev.OcrLotIdFixEnable.Value = false;
                semicsOcrDev.OcrConfirmWaferIDPrefix.Value = false;
                semicsOcrDev.RejectOrConfirm.Value = false;
                semicsOcrDev.OcrReadRegionPosX.Value = 21;
                semicsOcrDev.OcrReadRegionPosY.Value = 215;
                semicsOcrDev.OcrCharPosX.Value = 6;
                semicsOcrDev.OcrCharPosY.Value = 6;
                semicsOcrDev.OcrReadRegionWidth.Value = 404;
                semicsOcrDev.OcrReadRegionHeight.Value = 50;
                semicsOcrDev.OcrCharSizeX.Value = 26.0;
                semicsOcrDev.OcrCharSizeY.Value = 38.5;
                semicsOcrDev.OcrCharSpacing.Value = 7.34;
                semicsOcrDev.OcrMaxStringLength.Value = 12;
                semicsOcrDev.OcrCalibrateMinX.Value = 25.0;
                semicsOcrDev.OcrCalibrateMaxX.Value = 27.0;
                semicsOcrDev.OcrCalibrateStepX.Value = 0.50;
                semicsOcrDev.OcrCalibrateMinY.Value = 39.0;
                semicsOcrDev.OcrCalibrateMaxY.Value = 39.0;
                semicsOcrDev.OcrCalibrateStepY.Value = 0.50;
                semicsOcrDev.OcrFlipImage.Value = false;
                semicsOcrDev.OcrVerticalFlipImage.Value = false;
                semicsOcrDev.OcrHorizontalFlipImage.Value = false;

                var OCRParamTableVar = new OCRParamTable();

                OCRParamTableVar.UserOcrLightType.Value = 0;
                OCRParamTableVar.UserOcrLight1_Offset.Value = 99;
                OCRParamTableVar.UserOcrLight2_Offset.Value = 0;
                OCRParamTableVar.UserOcrLight3_Offset.Value = 255;
                OCRParamTableVar.OcrStrAcceptance.Value = 70;
                OCRParamTableVar.OcrCharAcceptance.Value = 70;
                OCRParamTableVar.OcrCalStrAcceptance.Value = 0;
                OCRParamTableVar.OcrCalCharAcceptance.Value = 0;
                OCRParamTableVar.OcrSampleString.Value = "TESTLOT-24-E";
                OCRParamTableVar.OcrConstraint.Value = "LLLLLLL-DD-CC";
                OCRParamTableVar.OcrCalibrationType.Value = 0;
                OCRParamTableVar.OcrMasterFilter.Value = 0;
                OCRParamTableVar.OcrMasterFilterGain.Value = 0;
                OCRParamTableVar.OcrSlaveFilter.Value = 0;
                OCRParamTableVar.OcrSlaveFilterGain.Value = 0;

                semicsOcrDev.OCRParamTables.Add(OCRParamTableVar);

                //semicsOcrDev.OCRParamTables.Add(new OCRParamTable()
                //{
                //    UserOcrLightType = 0,
                //    UserOcrLight1_Offset = 99,
                //    UserOcrLight2_Offset = 0,
                //    UserOcrLight3_Offset = 255,
                //    OcrStrAcceptance = 70,
                //    OcrCharAcceptance = 70,
                //    OcrCalStrAcceptance = 0,
                //    OcrCalCharAcceptance = 0,
                //    OcrSampleString = "TESTLOT-24-E",
                //    OcrConstraint = "LLLLLLL-DD-CC",
                //    OcrCalibrationType = 0,
                //    OcrMasterFilter = 0,
                //    OcrMasterFilterGain = 0,
                //    OcrSlaveFilter = 0,
                //    OcrSlaveFilterGain = 0,
                //});

                var OCRParamTableVar2 = new OCRParamTable();

                OCRParamTableVar2.UserOcrLightType.Value = 0;
                OCRParamTableVar2.UserOcrLight1_Offset.Value = 115;
                OCRParamTableVar2.UserOcrLight2_Offset.Value = 100;
                OCRParamTableVar2.UserOcrLight3_Offset.Value = 0;
                OCRParamTableVar2.OcrStrAcceptance.Value = 70;
                OCRParamTableVar2.OcrCharAcceptance.Value = 70;
                OCRParamTableVar2.OcrCalStrAcceptance.Value = 0;
                OCRParamTableVar2.OcrCalCharAcceptance.Value = 0;
                OCRParamTableVar2.OcrSampleString.Value = "TESTLOT-24-E3";
                OCRParamTableVar2.OcrConstraint.Value = "LLLLLLL-DD-CC";
                OCRParamTableVar2.OcrCalibrationType.Value = 0;
                OCRParamTableVar2.OcrMasterFilter.Value = 0;
                OCRParamTableVar2.OcrMasterFilterGain.Value = 0;
                OCRParamTableVar2.OcrSlaveFilter.Value = 0;
                OCRParamTableVar2.OcrSlaveFilterGain.Value = 0;

                semicsOcrDev.OCRParamTables.Add(OCRParamTableVar2);

                //semicsOcrDev.OCRParamTables.Add(new OCRParamTable()
                //{
                //    UserOcrLightType = 0,
                //    UserOcrLight1_Offset = 115,
                //    UserOcrLight2_Offset = 100,
                //    UserOcrLight3_Offset = 0,
                //    OcrStrAcceptance = 70,
                //    OcrCharAcceptance = 70,
                //    OcrCalStrAcceptance = 0,
                //    OcrCalCharAcceptance = 0,
                //    OcrSampleString = "TESTLOT-24-E3",
                //    OcrConstraint = "LLLLLLL-DD-CC",
                //    OcrCalibrationType = 0,
                //    OcrMasterFilter = 0,
                //    OcrMasterFilterGain = 0,
                //    OcrSlaveFilter = 0,
                //    OcrSlaveFilterGain = 0,
                //});

                var OCRParamTableVar3 = new OCRParamTable();

                OCRParamTableVar3.UserOcrLightType.Value = 0;
                OCRParamTableVar3.UserOcrLight1_Offset.Value = 150;
                OCRParamTableVar3.UserOcrLight2_Offset.Value = 0;
                OCRParamTableVar3.UserOcrLight3_Offset.Value = 100;
                OCRParamTableVar3.OcrStrAcceptance.Value = 70;
                OCRParamTableVar3.OcrCharAcceptance.Value = 70;
                OCRParamTableVar3.OcrCalStrAcceptance.Value = 0;
                OCRParamTableVar3.OcrCalCharAcceptance.Value = 0;
                OCRParamTableVar3.OcrSampleString.Value = "TESTLOT-24-E3";
                OCRParamTableVar3.OcrConstraint.Value = "XXXXXXX-DD-CC";
                OCRParamTableVar3.OcrCalibrationType.Value = 0;
                OCRParamTableVar3.OcrMasterFilter.Value = 0;
                OCRParamTableVar3.OcrMasterFilterGain.Value = 0;
                OCRParamTableVar3.OcrSlaveFilter.Value = 0;
                OCRParamTableVar3.OcrSlaveFilterGain.Value = 0;

                semicsOcrDev.OCRParamTables.Add(OCRParamTableVar3);

                //semicsOcrDev.OCRParamTables.Add(new OCRParamTable()
                //{
                //    UserOcrLightType = 0,
                //    UserOcrLight1_Offset = 150,
                //    UserOcrLight2_Offset = 0,
                //    UserOcrLight3_Offset = 100,
                //    OcrStrAcceptance = 70,
                //    OcrCharAcceptance = 70,
                //    OcrCalStrAcceptance = 0,
                //    OcrCalCharAcceptance = 0,
                //    OcrSampleString = "TESTLOT-24-E3",
                //    OcrConstraint = "XXXXXXX-DD-CC",
                //    OcrCalibrationType = 0,
                //    OcrMasterFilter = 0,
                //    OcrMasterFilterGain = 0,
                //    OcrSlaveFilter = 0,
                //    OcrSlaveFilterGain = 0,
                //});

                var OCRParamTableVar4 = new OCRParamTable();

                OCRParamTableVar4.UserOcrLightType.Value = 0;
                OCRParamTableVar4.UserOcrLight1_Offset.Value = 130;
                OCRParamTableVar4.UserOcrLight2_Offset.Value = 0;
                OCRParamTableVar4.UserOcrLight3_Offset.Value = 120;
                OCRParamTableVar4.OcrStrAcceptance.Value = 70;
                OCRParamTableVar4.OcrCharAcceptance.Value = 70;
                OCRParamTableVar4.OcrCalStrAcceptance.Value = 0;
                OCRParamTableVar4.OcrCalCharAcceptance.Value = 0;
                OCRParamTableVar4.OcrSampleString.Value = "TESTLOT-24-E3";
                OCRParamTableVar4.OcrConstraint.Value = "LLLLLLL-DD-XX";
                OCRParamTableVar4.OcrCalibrationType.Value = 0;
                OCRParamTableVar4.OcrMasterFilter.Value = 0;
                OCRParamTableVar4.OcrMasterFilterGain.Value = 0;
                OCRParamTableVar4.OcrSlaveFilter.Value = 0;
                OCRParamTableVar4.OcrSlaveFilterGain.Value = 0;

                semicsOcrDev.OCRParamTables.Add(OCRParamTableVar4);

                //semicsOcrDev.OCRParamTables.Add(new OCRParamTable()
                //{
                //    UserOcrLightType = 0,
                //    UserOcrLight1_Offset = 130,
                //    UserOcrLight2_Offset = 0,
                //    UserOcrLight3_Offset = 120,
                //    OcrStrAcceptance = 70,
                //    OcrCharAcceptance = 70,
                //    OcrCalStrAcceptance = 0,
                //    OcrCalCharAcceptance = 0,
                //    OcrSampleString = "TESTLOT-24-E3",
                //    OcrConstraint = "LLLLLLL-DD-XX",
                //    OcrCalibrationType = 0,
                //    OcrMasterFilter = 0,
                //    OcrMasterFilterGain = 0,
                //    OcrSlaveFilter = 0,
                //    OcrSlaveFilterGain = 0,
                //});

                SemicsOCRModules.Add(semicsOcrDev);


                CognexOCRModules.Add(new CognexOCRDevice());
                //=>Chuck
                var chuckDev = new ChuckDevice();
                chuckDev.LoadingNotchAngle.Value = 180;
                ChuckModules.Add(chuckDev);

                //=>FixedTray1
                var fixedDev = new FixedTrayDevice();

                fixedDev.AllocateDeviceInfo = new TransferObject();

                fixedDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                fixedDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH12;
                fixedDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.POLISH;
                fixedDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.NONE;
                fixedDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.UNDEFINED;
                fixedDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.UNDEFINED;

                //fixedDev.AllocateDeviceInfo = new TransferObjectDeviceInfo()
                //{
                //    Type = SubstrateTypeEnum.Wafer,
                //    Size = SubstrateSizeEnum.INCH12,
                //    WaferType = EnumWaferType.POLISH,
                //    OCRMode = OCRModeEnum.NONE,
                //    OCRType = OCRTypeEnum.UNDEFINED,
                //    OCRDirection = OCRDirectionEnum.UNDEFINED,
                //};
                FixedTrayModules.Add(fixedDev);

                //=>FixedTray2
                fixedDev = new FixedTrayDevice();

                fixedDev.AllocateDeviceInfo = new TransferObject();

                fixedDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                fixedDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH12;
                fixedDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.POLISH;
                fixedDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.NONE;
                fixedDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.UNDEFINED;
                fixedDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.UNDEFINED;

                //fixedDev.AllocateDeviceInfo = new TransferObjectDeviceInfo()
                //{
                //    Type = SubstrateTypeEnum.Wafer,
                //    Size = SubstrateSizeEnum.INCH12,
                //    WaferType = EnumWaferType.POLISH,
                //    OCRMode = OCRModeEnum.NONE,
                //    OCRType = OCRTypeEnum.UNDEFINED,
                //    OCRDirection = OCRDirectionEnum.UNDEFINED,
                //};
                FixedTrayModules.Add(fixedDev);

                //=>FixedTray3
                fixedDev = new FixedTrayDevice();

                fixedDev.AllocateDeviceInfo = new TransferObject();

                fixedDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                fixedDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH12;
                fixedDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.POLISH;
                fixedDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.NONE;
                fixedDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.UNDEFINED;
                fixedDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.UNDEFINED;

                //fixedDev.AllocateDeviceInfo = new TransferObjectDeviceInfo()
                //{
                //    Type = SubstrateTypeEnum.Wafer,
                //    Size = SubstrateSizeEnum.INCH12,
                //    WaferType = EnumWaferType.POLISH,
                //    OCRMode = OCRModeEnum.NONE,
                //    OCRType = OCRTypeEnum.UNDEFINED,
                //    OCRDirection = OCRDirectionEnum.UNDEFINED,
                //};
                FixedTrayModules.Add(fixedDev);

                //=>FixedTray4
                fixedDev = new FixedTrayDevice();

                fixedDev.AllocateDeviceInfo = new TransferObject();

                fixedDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                fixedDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH12;
                fixedDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.POLISH;
                fixedDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.NONE;
                fixedDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.UNDEFINED;
                fixedDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.UNDEFINED;

                //fixedDev.AllocateDeviceInfo = new TransferObjectDeviceInfo()
                //{
                //    Type = SubstrateTypeEnum.Wafer,
                //    Size = SubstrateSizeEnum.INCH12,
                //    WaferType = EnumWaferType.POLISH,
                //    OCRMode = OCRModeEnum.NONE,
                //    OCRType = OCRTypeEnum.UNDEFINED,
                //    OCRDirection = OCRDirectionEnum.UNDEFINED,
                //};
                FixedTrayModules.Add(fixedDev);

                //=>FixedTray5
                fixedDev = new FixedTrayDevice();

                fixedDev.AllocateDeviceInfo = new TransferObject();

                fixedDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                fixedDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH12;
                fixedDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.POLISH;
                fixedDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.NONE;
                fixedDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.UNDEFINED;
                fixedDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.UNDEFINED;

                //fixedDev.AllocateDeviceInfo = new TransferObjectDeviceInfo()
                //{
                //    Type = SubstrateTypeEnum.Wafer,
                //    Size = SubstrateSizeEnum.INCH12,
                //    WaferType = EnumWaferType.POLISH,
                //    OCRMode = OCRModeEnum.NONE,
                //    OCRType = OCRTypeEnum.UNDEFINED,
                //    OCRDirection = OCRDirectionEnum.UNDEFINED,
                //};
                FixedTrayModules.Add(fixedDev);

                //=>InspectionTray1
                //var inspDev = new InspectionTrayDevice();
                //inspDev.AllocateDeviceInfo = new TransferObjectDeviceInfo()
                //{
                //    Type = SubstrateTypeEnum.Wafer,
                //    Size = SubstrateSizeEnum.INCH12,
                //    WaferType = EnumWaferType.STANDARD,
                //    OCRMode = OCRModeEnum.READ,
                //    OCRType = OCRTypeEnum.SEMICS,
                //    OCRDirection = OCRDirectionEnum.FRONT,
                //};
                //devParam.InspectionTrayModules.Add(inspDev);

                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private EventCodeEnum SetDefaultParamOPUSVTestMahcine()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {


                //=> CameraScan
                //CameraScanDevice camScanDev = new CameraScanDevice();
                //devParam.CameraScanModules.Add(camScanDev);

                //=> SensorScan
                ScanSensorDevice sensorScanDev = new ScanSensorDevice();
                ScanSensorModules.Add(sensorScanDev);

                //=> Cassette
                var cstDev = new CassetteDevice();

                cstDev.SlotSize.Value = 6354;
                cstDev.WaferThickness.Value = 510;
                cstDev.LoadingNotchAngle.Value = 0;

                cstDev.AllocateDeviceInfo = new TransferObject();

                cstDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                cstDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH8;
                cstDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.STANDARD;
                cstDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.READ;
                cstDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.COGNEX;
                cstDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.FRONT;

                //cstDev.AllocateDeviceInfo = new TransferObjectDeviceInfo()
                //{
                //    Type = SubstrateTypeEnum.Wafer,
                //    Size = SubstrateSizeEnum.INCH8,
                //    WaferType = EnumWaferType.STANDARD,
                //    OCRMode = OCRModeEnum.READ,
                //    OCRType = OCRTypeEnum.COGNEX,
                //    OCRDirection = OCRDirectionEnum.FRONT,
                //};

                for (int i = 0; i < 25; i++)
                {
                    var SlotDeviceVar = new SlotDevice();

                    SlotDeviceVar.IsOverrideEnable.Value = false;

                    cstDev.SlotModules.Add(SlotDeviceVar);

                    //cstDev.SlotModules.Add(new SlotDevice()
                    //{
                    //    IsOverrideEnable = false,
                    //});
                }
                CassetteModules.Add(cstDev);

                //=> ARM1
                ARMDevice armDev = new ARMDevice();

                ARMModules.Add(armDev);

                //=> ARM2
                armDev = new ARMDevice();
                ARMModules.Add(armDev);

                //=> PreAlign1
                PreAlignDevice preDev = new PreAlignDevice();
                //preDev.Label = "PRE";
                PreAlignModules.Add(preDev);


                //=>SemicsOCR
                var semicsOcrDev = new SemicsOCRDevice();
                semicsOcrDev.OffsetU.Value = 0;
                semicsOcrDev.OffsetW.Value = 0;
                semicsOcrDev.OffsetV.Value = -27850;
                semicsOcrDev.UserLightEnable.Value = true;
                semicsOcrDev.LotIntegrityEnable.Value = false;
                semicsOcrDev.SlotIntegrityEnable.Value = false;
                semicsOcrDev.WaferIntegrityEnable.Value = false;
                semicsOcrDev.OcrAdvancedReadEnable.Value = true;
                semicsOcrDev.OcrConfirmEnable.Value = false;
                semicsOcrDev.OcrCheckSumEnable.Value = true;
                semicsOcrDev.OcrLotIdFixEnable.Value = false;
                semicsOcrDev.OcrConfirmWaferIDPrefix.Value = false;
                semicsOcrDev.RejectOrConfirm.Value = false;
                semicsOcrDev.OcrReadRegionPosX.Value = 21;
                semicsOcrDev.OcrReadRegionPosY.Value = 215;
                semicsOcrDev.OcrCharPosX.Value = 6;
                semicsOcrDev.OcrCharPosY.Value = 6;
                semicsOcrDev.OcrReadRegionWidth.Value = 404;
                semicsOcrDev.OcrReadRegionHeight.Value = 50;
                semicsOcrDev.OcrCharSizeX.Value = 26.0;
                semicsOcrDev.OcrCharSizeY.Value = 38.5;
                semicsOcrDev.OcrCharSpacing.Value = 7.34;
                semicsOcrDev.OcrMaxStringLength.Value = 12;
                semicsOcrDev.OcrCalibrateMinX.Value = 25.0;
                semicsOcrDev.OcrCalibrateMaxX.Value = 27.0;
                semicsOcrDev.OcrCalibrateStepX.Value = 0.50;
                semicsOcrDev.OcrCalibrateMinY.Value = 39.0;
                semicsOcrDev.OcrCalibrateMaxY.Value = 39.0;
                semicsOcrDev.OcrCalibrateStepY.Value = 0.50;
                semicsOcrDev.OcrFlipImage.Value = false;
                semicsOcrDev.OCRFontFilePath.Value = "OCR";
                semicsOcrDev.OCRFontFileName.Value = "Calibrated.mfo";

                var OCRParamTableVar = new OCRParamTable();

                OCRParamTableVar.UserOcrLightType.Value = 0;
                OCRParamTableVar.UserOcrLight1_Offset.Value = 99;
                OCRParamTableVar.UserOcrLight2_Offset.Value = 0;
                OCRParamTableVar.UserOcrLight3_Offset.Value = 255;
                OCRParamTableVar.OcrStrAcceptance.Value = 70;
                OCRParamTableVar.OcrCharAcceptance.Value = 70;
                OCRParamTableVar.OcrCalStrAcceptance.Value = 0;
                OCRParamTableVar.OcrCalCharAcceptance.Value = 0;
                OCRParamTableVar.OcrSampleString.Value = "TESTLOT-24-E";
                OCRParamTableVar.OcrConstraint.Value = "LLLLLLL-DD-CC";
                OCRParamTableVar.OcrCalibrationType.Value = 0;
                OCRParamTableVar.OcrMasterFilter.Value = 0;
                OCRParamTableVar.OcrMasterFilterGain.Value = 0;
                OCRParamTableVar.OcrSlaveFilter.Value = 0;
                OCRParamTableVar.OcrSlaveFilterGain.Value = 0;

                semicsOcrDev.OCRParamTables.Add(OCRParamTableVar);

                //semicsOcrDev.OCRParamTables.Add(new OCRParamTable()
                //{
                //    UserOcrLightType = 0,
                //    UserOcrLight1_Offset = 99,
                //    UserOcrLight2_Offset = 0,
                //    UserOcrLight3_Offset = 255,
                //    OcrStrAcceptance = 70,
                //    OcrCharAcceptance = 70,
                //    OcrCalStrAcceptance = 0,
                //    OcrCalCharAcceptance = 0,
                //    OcrSampleString = "TESTLOT-24-E",
                //    OcrConstraint = "LLLLLLL-DD-CC",
                //    OcrCalibrationType = 0,
                //    OcrMasterFilter = 0,
                //    OcrMasterFilterGain = 0,
                //    OcrSlaveFilter = 0,
                //    OcrSlaveFilterGain = 0,
                //});

                var OCRParamTableVar2 = new OCRParamTable();

                OCRParamTableVar2.UserOcrLightType.Value = 0;
                OCRParamTableVar2.UserOcrLight1_Offset.Value = 115;
                OCRParamTableVar2.UserOcrLight2_Offset.Value = 100;
                OCRParamTableVar2.UserOcrLight3_Offset.Value = 0;
                OCRParamTableVar2.OcrStrAcceptance.Value = 70;
                OCRParamTableVar2.OcrCharAcceptance.Value = 70;
                OCRParamTableVar2.OcrCalStrAcceptance.Value = 0;
                OCRParamTableVar2.OcrCalCharAcceptance.Value = 0;
                OCRParamTableVar2.OcrSampleString.Value = "TESTLOT-24-E3";
                OCRParamTableVar2.OcrConstraint.Value = "LLLLLLL-DD-CC";
                OCRParamTableVar2.OcrCalibrationType.Value = 0;
                OCRParamTableVar2.OcrMasterFilter.Value = 0;
                OCRParamTableVar2.OcrMasterFilterGain.Value = 0;
                OCRParamTableVar2.OcrSlaveFilter.Value = 0;
                OCRParamTableVar2.OcrSlaveFilterGain.Value = 0;

                semicsOcrDev.OCRParamTables.Add(OCRParamTableVar2);

                //semicsOcrDev.OCRParamTables.Add(new OCRParamTable()
                //{
                //    UserOcrLightType = 0,
                //    UserOcrLight1_Offset = 115,
                //    UserOcrLight2_Offset = 100,
                //    UserOcrLight3_Offset = 0,
                //    OcrStrAcceptance = 70,
                //    OcrCharAcceptance = 70,
                //    OcrCalStrAcceptance = 0,
                //    OcrCalCharAcceptance = 0,
                //    OcrSampleString = "TESTLOT-24-E3",
                //    OcrConstraint = "LLLLLLL-DD-CC",
                //    OcrCalibrationType = 0,
                //    OcrMasterFilter = 0,
                //    OcrMasterFilterGain = 0,
                //    OcrSlaveFilter = 0,
                //    OcrSlaveFilterGain = 0,
                //});

                var OCRParamTableVar3 = new OCRParamTable();

                OCRParamTableVar3.UserOcrLightType.Value = 0;
                OCRParamTableVar3.UserOcrLight1_Offset.Value = 150;
                OCRParamTableVar3.UserOcrLight2_Offset.Value = 0;
                OCRParamTableVar3.UserOcrLight3_Offset.Value = 100;
                OCRParamTableVar3.OcrStrAcceptance.Value = 70;
                OCRParamTableVar3.OcrCharAcceptance.Value = 70;
                OCRParamTableVar3.OcrCalStrAcceptance.Value = 0;
                OCRParamTableVar3.OcrCalCharAcceptance.Value = 0;
                OCRParamTableVar3.OcrSampleString.Value = "TESTLOT-24-E3";
                OCRParamTableVar3.OcrConstraint.Value = "XXXXXXX-DD-CC";
                OCRParamTableVar3.OcrCalibrationType.Value = 0;
                OCRParamTableVar3.OcrMasterFilter.Value = 0;
                OCRParamTableVar3.OcrMasterFilterGain.Value = 0;
                OCRParamTableVar3.OcrSlaveFilter.Value = 0;
                OCRParamTableVar3.OcrSlaveFilterGain.Value = 0;

                semicsOcrDev.OCRParamTables.Add(OCRParamTableVar3);

                //semicsOcrDev.OCRParamTables.Add(new OCRParamTable()
                //{
                //    UserOcrLightType = 0,
                //    UserOcrLight1_Offset = 150,
                //    UserOcrLight2_Offset = 0,
                //    UserOcrLight3_Offset = 100,
                //    OcrStrAcceptance = 70,
                //    OcrCharAcceptance = 70,
                //    OcrCalStrAcceptance = 0,
                //    OcrCalCharAcceptance = 0,
                //    OcrSampleString = "TESTLOT-24-E3",
                //    OcrConstraint = "XXXXXXX-DD-CC",
                //    OcrCalibrationType = 0,
                //    OcrMasterFilter = 0,
                //    OcrMasterFilterGain = 0,
                //    OcrSlaveFilter = 0,
                //    OcrSlaveFilterGain = 0,
                //});

                var OCRParamTableVar4 = new OCRParamTable();

                OCRParamTableVar4.UserOcrLightType.Value = 0;
                OCRParamTableVar4.UserOcrLight1_Offset.Value = 130;
                OCRParamTableVar4.UserOcrLight2_Offset.Value = 0;
                OCRParamTableVar4.UserOcrLight3_Offset.Value = 120;
                OCRParamTableVar4.OcrStrAcceptance.Value = 70;
                OCRParamTableVar4.OcrCharAcceptance.Value = 70;
                OCRParamTableVar4.OcrCalStrAcceptance.Value = 0;
                OCRParamTableVar4.OcrCalCharAcceptance.Value = 0;
                OCRParamTableVar4.OcrSampleString.Value = "TESTLOT-24-E3";
                OCRParamTableVar4.OcrConstraint.Value = "LLLLLLL-DD-XX";
                OCRParamTableVar4.OcrCalibrationType.Value = 0;
                OCRParamTableVar4.OcrMasterFilter.Value = 0;
                OCRParamTableVar4.OcrMasterFilterGain.Value = 0;
                OCRParamTableVar4.OcrSlaveFilter.Value = 0;
                OCRParamTableVar4.OcrSlaveFilterGain.Value = 0;

                semicsOcrDev.OCRParamTables.Add(OCRParamTableVar4);

                //semicsOcrDev.OCRParamTables.Add(new OCRParamTable()
                //{
                //    UserOcrLightType = 0,
                //    UserOcrLight1_Offset = 130,
                //    UserOcrLight2_Offset = 0,
                //    UserOcrLight3_Offset = 120,
                //    OcrStrAcceptance = 70,
                //    OcrCharAcceptance = 70,
                //    OcrCalStrAcceptance = 0,
                //    OcrCalCharAcceptance = 0,
                //    OcrSampleString = "TESTLOT-24-E3",
                //    OcrConstraint = "LLLLLLL-DD-XX",
                //    OcrCalibrationType = 0,
                //    OcrMasterFilter = 0,
                //    OcrMasterFilterGain = 0,
                //    OcrSlaveFilter = 0,
                //    OcrSlaveFilterGain = 0,
                //});

                SemicsOCRModules.Add(semicsOcrDev);

                CognexOCRModules.Add(new CognexOCRDevice());
                //=>Chuck
                var chuckDev = new ChuckDevice();
                chuckDev.LoadingNotchAngle.Value = 180;
                ChuckModules.Add(chuckDev);

                //=>FixedTray1
                var fixedDev = new FixedTrayDevice();

                fixedDev.AllocateDeviceInfo = new TransferObject();

                fixedDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                fixedDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH8;
                fixedDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.POLISH;
                fixedDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.NONE;
                fixedDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.UNDEFINED;
                fixedDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.UNDEFINED;

                //fixedDev.AllocateDeviceInfo = new TransferObjectDeviceInfo()
                //{
                //    Type = SubstrateTypeEnum.Wafer,
                //    Size = SubstrateSizeEnum.INCH8,
                //    WaferType = EnumWaferType.POLISH,
                //    OCRMode = OCRModeEnum.NONE,
                //    OCRType = OCRTypeEnum.UNDEFINED,
                //    OCRDirection = OCRDirectionEnum.UNDEFINED,
                //};
                FixedTrayModules.Add(fixedDev);

                //=>FixedTray2
                fixedDev = new FixedTrayDevice();

                fixedDev.AllocateDeviceInfo = new TransferObject();

                fixedDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                fixedDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH8;
                fixedDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.POLISH;
                fixedDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.NONE;
                fixedDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.UNDEFINED;
                fixedDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.UNDEFINED;

                //fixedDev.AllocateDeviceInfo = new TransferObjectDeviceInfo()
                //{
                //    Type = SubstrateTypeEnum.Wafer,
                //    Size = SubstrateSizeEnum.INCH8,
                //    WaferType = EnumWaferType.POLISH,
                //    OCRMode = OCRModeEnum.NONE,
                //    OCRType = OCRTypeEnum.UNDEFINED,
                //    OCRDirection = OCRDirectionEnum.UNDEFINED,
                //};
                FixedTrayModules.Add(fixedDev);

                //=>FixedTray3
                fixedDev = new FixedTrayDevice();

                fixedDev.AllocateDeviceInfo = new TransferObject();

                fixedDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                fixedDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH8;
                fixedDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.POLISH;
                fixedDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.NONE;
                fixedDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.UNDEFINED;
                fixedDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.UNDEFINED;

                //fixedDev.AllocateDeviceInfo = new TransferObjectDeviceInfo()
                //{
                //    Type = SubstrateTypeEnum.Wafer,
                //    Size = SubstrateSizeEnum.INCH8,
                //    WaferType = EnumWaferType.POLISH,
                //    OCRMode = OCRModeEnum.NONE,
                //    OCRType = OCRTypeEnum.UNDEFINED,
                //    OCRDirection = OCRDirectionEnum.UNDEFINED,
                //};
                FixedTrayModules.Add(fixedDev);

                //=>FixedTray4
                fixedDev = new FixedTrayDevice();

                fixedDev.AllocateDeviceInfo = new TransferObject();

                fixedDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                fixedDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH8;
                fixedDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.POLISH;
                fixedDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.NONE;
                fixedDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.UNDEFINED;
                fixedDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.UNDEFINED;

                //fixedDev.AllocateDeviceInfo = new TransferObjectDeviceInfo()
                //{
                //    Type = SubstrateTypeEnum.Wafer,
                //    Size = SubstrateSizeEnum.INCH8,
                //    WaferType = EnumWaferType.POLISH,
                //    OCRMode = OCRModeEnum.NONE,
                //    OCRType = OCRTypeEnum.UNDEFINED,
                //    OCRDirection = OCRDirectionEnum.UNDEFINED,
                //};
                FixedTrayModules.Add(fixedDev);

                //=>FixedTray5
                fixedDev = new FixedTrayDevice();

                fixedDev.AllocateDeviceInfo = new TransferObject();

                fixedDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                fixedDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH8;
                fixedDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.POLISH;
                fixedDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.NONE;
                fixedDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.UNDEFINED;
                fixedDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.UNDEFINED;

                //fixedDev.AllocateDeviceInfo = new TransferObjectDeviceInfo()
                //{
                //    Type = SubstrateTypeEnum.Wafer,
                //    Size = SubstrateSizeEnum.INCH8,
                //    WaferType = EnumWaferType.POLISH,
                //    OCRMode = OCRModeEnum.NONE,
                //    OCRType = OCRTypeEnum.UNDEFINED,
                //    OCRDirection = OCRDirectionEnum.UNDEFINED,
                //};
                FixedTrayModules.Add(fixedDev);

                //=>InspectionTray1
                var inspDev = new InspectionTrayDevice();
                inspDev.AllocateDeviceInfo = new TransferObject();
                inspDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                inspDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH8;
                inspDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.STANDARD;
                inspDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.READ;
                inspDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.COGNEX;
                inspDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.FRONT;
                InspectionTrayModules.Add(inspDev);
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private EventCodeEnum SetDefaultParamGPL()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {


                //=> CameraScan
                //CameraScanDevice camScanDev = new CameraScanDevice();
                //devParam.CameraScanModules.Add(camScanDev);

                //=> SensorScan
                ScanSensorDevice sensorScanDev = new ScanSensorDevice();
                ScanSensorModules.Add(sensorScanDev);

                //=> Cassette
                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                {
                    var cstDev = new CassetteDevice();

                    cstDev.SlotSize.Value = 6354;
                    cstDev.WaferThickness.Value = 510;
                    cstDev.LoadingNotchAngle.Value = 0;

                    cstDev.AllocateDeviceInfo = new TransferObject();

                    cstDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                    cstDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH8;
                    cstDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.STANDARD;
                    cstDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.READ;
                    cstDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.COGNEX;
                    cstDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.FRONT;

                    //cstDev.AllocateDeviceInfo = new TransferObjectDeviceInfo()
                    //{
                    //    Type = SubstrateTypeEnum.Wafer,
                    //    Size = SubstrateSizeEnum.INCH8,
                    //    WaferType = EnumWaferType.STANDARD,
                    //    OCRMode = OCRModeEnum.READ,
                    //    OCRType = OCRTypeEnum.COGNEX,
                    //    OCRDirection = OCRDirectionEnum.FRONT,
                    //};

                    for (int cstindex = 0; cstindex < SystemModuleCount.ModuleCnt.SlotCount; cstindex++)
                    {
                        var SlotDeviceVar = new SlotDevice();

                        SlotDeviceVar.IsOverrideEnable.Value = false;

                        cstDev.SlotModules.Add(SlotDeviceVar);

                        //cstDev.SlotModules.Add(new SlotDevice()
                        //{
                        //    IsOverrideEnable = false,
                        //});
                    }
                    CassetteModules.Add(cstDev);
                }



                //=> ARM1
                ARMDevice armDev = new ARMDevice();

                ARMModules.Add(armDev);

                //=> ARM2
                armDev = new ARMDevice();
                ARMModules.Add(armDev);

                //=> CC_ARM
                CardARMDevice ccarmDev = new CardARMDevice();
                CardArmModules.Add(ccarmDev);



                for (int paIndex = 0; paIndex < SystemModuleCount.ModuleCnt.PACount; paIndex++)
                {
                    //=> PreAlign1
                    PreAlignDevice preDev = new PreAlignDevice();
                    preDev.Label.Value = "";
                    //preDev.Label = "PRE";
                    PreAlignModules.Add(preDev);
                }

                //=>SemicsOCR
                var semicsOcrDev = new SemicsOCRDevice();
                semicsOcrDev.OffsetU.Value = 0;
                semicsOcrDev.OffsetW.Value = 0;
                semicsOcrDev.OffsetV.Value = -27850;
                semicsOcrDev.UserLightEnable.Value = true;
                semicsOcrDev.LotIntegrityEnable.Value = false;
                semicsOcrDev.SlotIntegrityEnable.Value = false;
                semicsOcrDev.WaferIntegrityEnable.Value = false;
                semicsOcrDev.OcrAdvancedReadEnable.Value = true;
                semicsOcrDev.OcrConfirmEnable.Value = false;
                semicsOcrDev.OcrCheckSumEnable.Value = true;
                semicsOcrDev.OcrLotIdFixEnable.Value = false;
                semicsOcrDev.OcrConfirmWaferIDPrefix.Value = false;
                semicsOcrDev.RejectOrConfirm.Value = false;
                semicsOcrDev.OcrReadRegionPosX.Value = 21;
                semicsOcrDev.OcrReadRegionPosY.Value = 215;
                semicsOcrDev.OcrCharPosX.Value = 6;
                semicsOcrDev.OcrCharPosY.Value = 6;
                semicsOcrDev.OcrReadRegionWidth.Value = 404;
                semicsOcrDev.OcrReadRegionHeight.Value = 50;
                semicsOcrDev.OcrCharSizeX.Value = 26.0;
                semicsOcrDev.OcrCharSizeY.Value = 38.5;
                semicsOcrDev.OcrCharSpacing.Value = 7.34;
                semicsOcrDev.OcrMaxStringLength.Value = 12;
                semicsOcrDev.OcrCalibrateMinX.Value = 25.0;
                semicsOcrDev.OcrCalibrateMaxX.Value = 27.0;
                semicsOcrDev.OcrCalibrateStepX.Value = 0.50;
                semicsOcrDev.OcrCalibrateMinY.Value = 39.0;
                semicsOcrDev.OcrCalibrateMaxY.Value = 39.0;
                semicsOcrDev.OcrCalibrateStepY.Value = 0.50;
                semicsOcrDev.OcrFlipImage.Value = false;

                var OCRParamTableVar = new OCRParamTable();

                OCRParamTableVar.UserOcrLightType.Value = 0;
                OCRParamTableVar.UserOcrLight1_Offset.Value = 99;
                OCRParamTableVar.UserOcrLight2_Offset.Value = 0;
                OCRParamTableVar.UserOcrLight3_Offset.Value = 255;
                OCRParamTableVar.OcrStrAcceptance.Value = 70;
                OCRParamTableVar.OcrCharAcceptance.Value = 70;
                OCRParamTableVar.OcrCalStrAcceptance.Value = 0;
                OCRParamTableVar.OcrCalCharAcceptance.Value = 0;
                OCRParamTableVar.OcrSampleString.Value = "TESTLOT-24-E";
                OCRParamTableVar.OcrConstraint.Value = "LLLLLLL-DD-CC";
                OCRParamTableVar.OcrCalibrationType.Value = 0;
                OCRParamTableVar.OcrMasterFilter.Value = 0;
                OCRParamTableVar.OcrMasterFilterGain.Value = 0;
                OCRParamTableVar.OcrSlaveFilter.Value = 0;
                OCRParamTableVar.OcrSlaveFilterGain.Value = 0;

                semicsOcrDev.OCRParamTables.Add(OCRParamTableVar);

                //semicsOcrDev.OCRParamTables.Add(new OCRParamTable()
                //{
                //    UserOcrLightType = 0,
                //    UserOcrLight1_Offset = 99,
                //    UserOcrLight2_Offset = 0,
                //    UserOcrLight3_Offset = 255,
                //    OcrStrAcceptance = 70,
                //    OcrCharAcceptance = 70,
                //    OcrCalStrAcceptance = 0,
                //    OcrCalCharAcceptance = 0,
                //    OcrSampleString = "TESTLOT-24-E",
                //    OcrConstraint = "LLLLLLL-DD-CC",
                //    OcrCalibrationType = 0,
                //    OcrMasterFilter = 0,
                //    OcrMasterFilterGain = 0,
                //    OcrSlaveFilter = 0,
                //    OcrSlaveFilterGain = 0,
                //});

                var OCRParamTableVar2 = new OCRParamTable();

                OCRParamTableVar2.UserOcrLightType.Value = 0;
                OCRParamTableVar2.UserOcrLight1_Offset.Value = 115;
                OCRParamTableVar2.UserOcrLight2_Offset.Value = 100;
                OCRParamTableVar2.UserOcrLight3_Offset.Value = 0;
                OCRParamTableVar2.OcrStrAcceptance.Value = 70;
                OCRParamTableVar2.OcrCharAcceptance.Value = 70;
                OCRParamTableVar2.OcrCalStrAcceptance.Value = 0;
                OCRParamTableVar2.OcrCalCharAcceptance.Value = 0;
                OCRParamTableVar2.OcrSampleString.Value = "TESTLOT-24-E3";
                OCRParamTableVar2.OcrConstraint.Value = "LLLLLLL-DD-CC";
                OCRParamTableVar2.OcrCalibrationType.Value = 0;
                OCRParamTableVar2.OcrMasterFilter.Value = 0;
                OCRParamTableVar2.OcrMasterFilterGain.Value = 0;
                OCRParamTableVar2.OcrSlaveFilter.Value = 0;
                OCRParamTableVar2.OcrSlaveFilterGain.Value = 0;

                semicsOcrDev.OCRParamTables.Add(OCRParamTableVar2);

                //semicsOcrDev.OCRParamTables.Add(new OCRParamTable()
                //{
                //    UserOcrLightType = 0,
                //    UserOcrLight1_Offset = 115,
                //    UserOcrLight2_Offset = 100,
                //    UserOcrLight3_Offset = 0,
                //    OcrStrAcceptance = 70,
                //    OcrCharAcceptance = 70,
                //    OcrCalStrAcceptance = 0,
                //    OcrCalCharAcceptance = 0,
                //    OcrSampleString = "TESTLOT-24-E3",
                //    OcrConstraint = "LLLLLLL-DD-CC",
                //    OcrCalibrationType = 0,
                //    OcrMasterFilter = 0,
                //    OcrMasterFilterGain = 0,
                //    OcrSlaveFilter = 0,
                //    OcrSlaveFilterGain = 0,
                //});

                var OCRParamTableVar3 = new OCRParamTable();

                OCRParamTableVar3.UserOcrLightType.Value = 0;
                OCRParamTableVar3.UserOcrLight1_Offset.Value = 150;
                OCRParamTableVar3.UserOcrLight2_Offset.Value = 0;
                OCRParamTableVar3.UserOcrLight3_Offset.Value = 100;
                OCRParamTableVar3.OcrStrAcceptance.Value = 70;
                OCRParamTableVar3.OcrCharAcceptance.Value = 70;
                OCRParamTableVar3.OcrCalStrAcceptance.Value = 0;
                OCRParamTableVar3.OcrCalCharAcceptance.Value = 0;
                OCRParamTableVar3.OcrSampleString.Value = "TESTLOT-24-E3";
                OCRParamTableVar3.OcrConstraint.Value = "XXXXXXX-DD-CC";
                OCRParamTableVar3.OcrCalibrationType.Value = 0;
                OCRParamTableVar3.OcrMasterFilter.Value = 0;
                OCRParamTableVar3.OcrMasterFilterGain.Value = 0;
                OCRParamTableVar3.OcrSlaveFilter.Value = 0;
                OCRParamTableVar3.OcrSlaveFilterGain.Value = 0;

                semicsOcrDev.OCRParamTables.Add(OCRParamTableVar3);

                //semicsOcrDev.OCRParamTables.Add(new OCRParamTable()
                //{
                //    UserOcrLightType = 0,
                //    UserOcrLight1_Offset = 150,
                //    UserOcrLight2_Offset = 0,
                //    UserOcrLight3_Offset = 100,
                //    OcrStrAcceptance = 70,
                //    OcrCharAcceptance = 70,
                //    OcrCalStrAcceptance = 0,
                //    OcrCalCharAcceptance = 0,
                //    OcrSampleString = "TESTLOT-24-E3",
                //    OcrConstraint = "XXXXXXX-DD-CC",
                //    OcrCalibrationType = 0,
                //    OcrMasterFilter = 0,
                //    OcrMasterFilterGain = 0,
                //    OcrSlaveFilter = 0,
                //    OcrSlaveFilterGain = 0,
                //});

                var OCRParamTableVar4 = new OCRParamTable();

                OCRParamTableVar4.UserOcrLightType.Value = 0;
                OCRParamTableVar4.UserOcrLight1_Offset.Value = 130;
                OCRParamTableVar4.UserOcrLight2_Offset.Value = 0;
                OCRParamTableVar4.UserOcrLight3_Offset.Value = 120;
                OCRParamTableVar4.OcrStrAcceptance.Value = 70;
                OCRParamTableVar4.OcrCharAcceptance.Value = 70;
                OCRParamTableVar4.OcrCalStrAcceptance.Value = 0;
                OCRParamTableVar4.OcrCalCharAcceptance.Value = 0;
                OCRParamTableVar4.OcrSampleString.Value = "TESTLOT-24-E3";
                OCRParamTableVar4.OcrConstraint.Value = "LLLLLLL-DD-XX";
                OCRParamTableVar4.OcrCalibrationType.Value = 0;
                OCRParamTableVar4.OcrMasterFilter.Value = 0;
                OCRParamTableVar4.OcrMasterFilterGain.Value = 0;
                OCRParamTableVar4.OcrSlaveFilter.Value = 0;
                OCRParamTableVar4.OcrSlaveFilterGain.Value = 0;

                semicsOcrDev.OCRParamTables.Add(OCRParamTableVar4);
                SemicsOCRModules.Add(semicsOcrDev);


                for (int paIndex = 0; paIndex < SystemModuleCount.ModuleCnt.PACount; paIndex++)
                {
                    CognexOCRModules.Add(new CognexOCRDevice());
                }
                
                //=>Chuck
                for (int stageIndex = 0; stageIndex < GPLoaderDef.StageCount; stageIndex++)
                {
                    var chuckDev = new ChuckDevice();
                    chuckDev.LoadingNotchAngle.Value = 180;
                    ChuckModules.Add(chuckDev);
                }
                //=>CC
                for (int stageIndex = 0; stageIndex < GPLoaderDef.StageCount; stageIndex++)
                {
                    var ccDev = new CCDevice();
                    ccDev.LoadingNotchAngle.Value = 180;
                    CCModules.Add(ccDev);
                }

                //=>FixedTray1
                for (int i = 0; i < GPLoaderDef.FTCount; i++)
                {
                    var fixedDev = new FixedTrayDevice();

                    fixedDev.AllocateDeviceInfo = new TransferObject();
                    fixedDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                    fixedDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH8;
                    fixedDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.POLISH;
                    fixedDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.NONE;
                    fixedDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.UNDEFINED;
                    fixedDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.UNDEFINED;

                    FixedTrayModules.Add(fixedDev);
                }
                //=>Card buffer
                for (int i = 0; i < GPLoaderDef.CardBufferCount; i++)
                {
                    var cbDev = new CardBufferDevice();

                    cbDev.AllocateDeviceInfo = new TransferObject();
                    cbDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Card;
                    cbDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.CUSTOM;
                    cbDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.INVALID;
                    cbDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.READ;
                    cbDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.SEMICS;
                    cbDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.FRONT;

                    CardBufferModules.Add(cbDev);
                }
                for (int i = 0; i < GPLoaderDef.CardBufferTrayCount; i++)
                {
                    var cbDev = new CardBufferTrayDevice();

                    cbDev.AllocateDeviceInfo = new TransferObject();
                    cbDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Card;
                    cbDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.CUSTOM;
                    cbDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.INVALID;
                    cbDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.READ;
                    cbDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.SEMICS;
                    cbDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.FRONT;

                    CardBufferTrayModules.Add(cbDev);
                }

                //=>InspectionTray1
                var inspDev = new InspectionTrayDevice();
                inspDev.AllocateDeviceInfo = new TransferObject();
                inspDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                inspDev.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH8;
                inspDev.AllocateDeviceInfo.WaferType.Value = EnumWaferType.STANDARD;
                inspDev.AllocateDeviceInfo.OCRMode.Value = OCRModeEnum.READ;
                inspDev.AllocateDeviceInfo.OCRType.Value = OCRTypeEnum.COGNEX;
                inspDev.AllocateDeviceInfo.OCRDirection.Value = OCRDirectionEnum.FRONT;
                InspectionTrayModules.Add(inspDev);
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

}
