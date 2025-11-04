using LogModule;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using ProberInterfaces;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using ProberErrorCode;
using System.Collections.ObjectModel;
using ProberInterfaces.Loader;

namespace LoaderParameters
{
    [DataContract]
    [Serializable]
    public class DeviceManagerParameter : INotifyPropertyChanged, ISystemParameterizable
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
        public string FileName { get; } = "GPDeviceInfos.json";

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
                if(SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    for (int i = 1; i <= 75; i++) //SLOT
                    {
                        WaferSupplyMappingInfo mappingInfo = new WaferSupplyMappingInfo();
                        mappingInfo.DeviceInfo = new TransferObject();
                        mappingInfo.WaferSupplyInfo = new WaferSupplyModuleInfo(ModuleTypeEnum.SLOT, i);
                        _DeviceMappingInfos.Add(mappingInfo);
                    }

                    for (int i = 1; i <= 9; i++)
                    {
                        WaferSupplyMappingInfo mappingInfo = new WaferSupplyMappingInfo();
                        mappingInfo.DeviceInfo = new TransferObject();
                        mappingInfo.WaferSupplyInfo = new WaferSupplyModuleInfo(ModuleTypeEnum.FIXEDTRAY, i);
                        _DeviceMappingInfos.Add(mappingInfo);
                    }

                    for (int i = 1; i <= 4; i++)
                    {
                        WaferSupplyMappingInfo mappingInfo = new WaferSupplyMappingInfo();
                        mappingInfo.DeviceInfo = new TransferObject();
                        mappingInfo.WaferSupplyInfo = new WaferSupplyModuleInfo(ModuleTypeEnum.INSPECTIONTRAY, i);
                        _DeviceMappingInfos.Add(mappingInfo);
                    }
                }
                else
                {

                    for (int i = 1; i <= 25; i++) //SLOT
                    {
                        WaferSupplyMappingInfo mappingInfo = new WaferSupplyMappingInfo();
                        mappingInfo.DeviceInfo = new TransferObject();
                        mappingInfo.WaferSupplyInfo = new WaferSupplyModuleInfo(ModuleTypeEnum.SLOT, i);
                        _DeviceMappingInfos.Add(mappingInfo);
                    }

                    for (int i = 1; i <= 9; i++)
                    {
                        WaferSupplyMappingInfo mappingInfo = new WaferSupplyMappingInfo();
                        mappingInfo.DeviceInfo = new TransferObject();
                        mappingInfo.WaferSupplyInfo = new WaferSupplyModuleInfo(ModuleTypeEnum.FIXEDTRAY, i);
                        _DeviceMappingInfos.Add(mappingInfo);
                    }

                    for (int i = 1; i <= 4; i++)
                    {
                        WaferSupplyMappingInfo mappingInfo = new WaferSupplyMappingInfo();
                        mappingInfo.DeviceInfo = new TransferObject();
                        mappingInfo.WaferSupplyInfo = new WaferSupplyModuleInfo(ModuleTypeEnum.INSPECTIONTRAY, i);
                        _DeviceMappingInfos.Add(mappingInfo);
                    }

                    if(PolishWaferSourceParameters == null)
                    {
                        PolishWaferSourceParameters = new AsyncObservableCollection<IPolishWaferSourceInformation>();
                    }
                }

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

        private ObservableCollection<WaferSupplyMappingInfo> _DeviceMappingInfos = new ObservableCollection<WaferSupplyMappingInfo>();
        /// <summary>
        /// ScanCameraModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<WaferSupplyMappingInfo> DeviceMappingInfos
        {
            get { return _DeviceMappingInfos; }
            set { _DeviceMappingInfos = value; RaisePropertyChanged(); }
        }

        private bool _DetachDevice = false;
        /// <summary>
        /// GP 에서 Stage 의 Device 를 각폴더로 구분할 것 인지에 대한 프로퍼티
        /// true : 분리함 , flase : 분리안함
        /// </summary>
        [DataMember]
        public bool DetachDevice
        {
            get { return _DetachDevice; }
            set
            {
                if (value != _DetachDevice)
                {
                    _DetachDevice = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncObservableCollection<IPolishWaferSourceInformation> _PolishWaferSourceParameters = new AsyncObservableCollection<IPolishWaferSourceInformation>();
        [DataMember]
        public AsyncObservableCollection<IPolishWaferSourceInformation> PolishWaferSourceParameters
        {
            get { return _PolishWaferSourceParameters; }
            set
            {
                if (value != _PolishWaferSourceParameters)
                {
                    _PolishWaferSourceParameters = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
    /// <summary>
    /// Wafer Supply Module의 Device를 정의 합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class WaferSupplyMappingInfo : INotifyPropertyChanged, ICloneable, IParamNode, IWaferSupplyMappingInfo
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
        private WaferSupplyModuleInfo _WaferSupplyInfo = new WaferSupplyModuleInfo();
        /// <summary>
        /// </summary>
        [DataMember]
        public WaferSupplyModuleInfo WaferSupplyInfo
        {
            get { return _WaferSupplyInfo; }
            set { _WaferSupplyInfo = value; RaisePropertyChanged(); }
        }

        //private TransferObjectDeviceInfo _DeviceInfo = new TransferObjectDeviceInfo();
        ///// <summary>
        ///// </summary>
        //[DataMember]
        //public TransferObjectDeviceInfo DeviceInfo
        //{
        //    get { return _DeviceInfo; }
        //    set { _DeviceInfo = value; RaisePropertyChanged(); }
        //}

        private TransferObject _DeviceInfo = new TransferObject();
        /// <summary>
        /// </summary>
        [DataMember]
        public TransferObject DeviceInfo
        {
            get { return _DeviceInfo; }
            set { _DeviceInfo = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            var shallowClone = MemberwiseClone() as TransferObject;
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

        //public ModuleTypeEnum GetModuleType()
        //{
        //    return WaferSupplyInfo.ModuleType;
        //}
    }
    ///// <summary>
    ///// Transfer Object의 디바이스를 정의합니다.
    ///// </summary>
    //[DataContract]
    //[Serializable]
    //public class TransferObjectDeviceInfo : ITransferObjectDeviceInfo, INotifyPropertyChanged, ICloneable, IParamNode
    //{
    //    [XmlIgnore, JsonIgnore]
    //    public string Genealogy { get; set; }
    //    [NonSerialized]
    //    private Object _Owner;
    //    [XmlIgnore, JsonIgnore, ParamIgnore]
    //    public Object Owner
    //    {
    //        get { return _Owner; }
    //        set
    //        {
    //            if (_Owner != value)
    //            {
    //                _Owner = value;
    //            }
    //        }
    //    }

    //    [XmlIgnore, JsonIgnore]
    //    public List<object> Nodes { get; set; }

    //    /// <summary>
    //    /// 속성값이 변경되면 발생합니다.
    //    /// </summary>
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    /// <summary>
    //    /// 지정된 속성이 변경되었음을 발생시킵니다.
    //    /// </summary>
    //    /// <param name="propertyName">속성 이름</param>
    //    protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //    }

    //    private Element<SubstrateTypeEnum> _Type = new Element<SubstrateTypeEnum>();
    //    /// <summary>
    //    /// 오브젝트의 타입을 가져오거나 설정합니다.
    //    /// </summary>
    //    [DataMember]
    //    public Element<SubstrateTypeEnum> Type
    //    {
    //        get { return _Type; }
    //        set { _Type = value; RaisePropertyChanged(); }
    //    }

    //    private Element<SubstrateSizeEnum> _Size = new Element<SubstrateSizeEnum>();
    //    /// <summary>
    //    /// 오브젝트의 사이즈를 가져오거나 설정합니다.
    //    /// Stage쪽에서는 EnumWaferSize이 있습니다.
    //    /// </summary>
    //    [DataMember]
    //    public Element<SubstrateSizeEnum> Size
    //    {
    //        get { return _Size; }
    //        set { _Size = value; RaisePropertyChanged(); }
    //    }

    //    private Element<EnumWaferType> _WaferType = new Element<EnumWaferType>();
    //    /// <summary>
    //    /// 오브젝트의 웨이퍼 타입을 가져오거나 설정합니다.
    //    /// </summary>
    //    [DataMember]
    //    public Element<EnumWaferType> WaferType
    //    {
    //        get { return _WaferType; }
    //        set { _WaferType = value; RaisePropertyChanged(); }
    //    }

    //    private Element<OCRModeEnum> _OCRMode = new Element<OCRModeEnum>();
    //    /// <summary>
    //    /// 오브젝트의 OCR Mode를 가져오거나 설정합니다.
    //    /// </summary>
    //    [DataMember]
    //    public Element<OCRModeEnum> OCRMode
    //    {
    //        get { return _OCRMode; }
    //        set { _OCRMode = value; RaisePropertyChanged(); }
    //    }

    //    private Element<OCRTypeEnum> _OCRType = new Element<OCRTypeEnum>();
    //    /// <summary>
    //    /// 오브젝트의 OCR Type을 가져오거나 설정합니다.
    //    /// </summary>
    //    [DataMember]
    //    public Element<OCRTypeEnum> OCRType
    //    {
    //        get { return _OCRType; }
    //        set { _OCRType = value; RaisePropertyChanged(); }
    //    }

    //    private Element<OCRDirectionEnum> _OCRDirection = new Element<OCRDirectionEnum>();
    //    /// <summary>
    //    /// 오브젝트의 OCR 방향을 가져오거나 설정합니다.
    //    /// </summary>
    //    [DataMember]
    //    public Element<OCRDirectionEnum> OCRDirection
    //    {
    //        get { return _OCRDirection; }
    //        set { _OCRDirection = value; RaisePropertyChanged(); }
    //    }

    //    private Element<string> _DeviceName = new Element<string>();
    //    /// <summary>
    //    /// 오브젝트의 DeviceKey를 가져오거나 설정합니다.
    //    /// </summary>
    //    [DataMember]
    //    public Element<string> DeviceName
    //    {
    //        get { return _DeviceName; }
    //        set { _DeviceName = value; RaisePropertyChanged(); }
    //    }



    //    private Element<double> _NotchAngle = new Element<double>();
    //    /// <summary>
    //    /// 오브젝트의 DeviceKey를 가져오거나 설정합니다.
    //    /// </summary>
    //    [DataMember]
    //    public Element<double> NotchAngle
    //    {
    //        get { return _NotchAngle; }
    //        set { _NotchAngle = value; RaisePropertyChanged(); }
    //    }




    //    private Element<double> _SlotNotchAngle = new Element<double>();
    //    /// <summary>
    //    /// 오브젝트의 DeviceKey를 가져오거나 설정합니다.
    //    /// </summary>
    //    [DataMember]
    //    public Element<double> SlotNotchAngle
    //    {
    //        get { return _SlotNotchAngle; }
    //        set { _SlotNotchAngle = value; RaisePropertyChanged(); }
    //    }
    //    /// <summary>
    //    /// 오브젝트의 복사본을 가져옵니다.
    //    /// </summary>
    //    /// <returns>복사본</returns>
    //    public object Clone()
    //    {
    //        var shallowClone = MemberwiseClone() as TransferObject;
    //        try
    //        {

    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //             throw;
    //        }
    //        return shallowClone;
    //    }

    //    //public void Copy(TransferObjectDeviceInfo data)
    //    //{
    //    //    this.Type = data.Type;
    //    //    this.Size = data.Size;
    //    //    this.WaferType = data.WaferType;
    //    //    this.OCRMode = data.OCRMode;
    //    //    this.OCRType = data.OCRType;
    //    //    this.OCRDirection = data.OCRDirection;
    //    //    this.DeviceName = data.DeviceName;

    //    //}
    //}
}
