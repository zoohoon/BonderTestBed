using System.Collections.Generic;

namespace SubstrateObjects
{
    //using MapObject;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using ProberErrorCode;
    using System.Runtime.CompilerServices;
    using LogModule;
    using Newtonsoft.Json;
    using ProberInterfaces.Enum;
    using System.Collections.ObjectModel;

    [Serializable]
    public class PhysicalInfo : INotifyPropertyChanged, IPhysicalInfo , IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [SharePropPath]
        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }

        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        [SharePropPath]
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

        //private Element<string> _CarrierID = new Element<string>();
        //public Element<string> CassetteID
        //{
        //    get { return _CarrierID; }
        //    set { _CarrierID = value;}
        //}

        //private Element<DateTime> _LoadTime = new Element<DateTime>();
        //public Element<DateTime> LoadTime
        //{
        //    get { return _LoadTime; }
        //    set { _LoadTime = value; }
        //}

        private Element<MapHorDirectionEnum> _MapHorDir = new Element<MapHorDirectionEnum>();
        public Element<MapHorDirectionEnum> MapDirX
        {
            get { return _MapHorDir; }
            set { _MapHorDir = value; }
        }

        private Element<MapVertDirectionEnum> _MapVertDir = new Element<MapVertDirectionEnum>();
        public Element<MapVertDirectionEnum> MapDirY
        {
            get { return _MapVertDir; }
            set { _MapVertDir = value; }
        }

        private Element<double> _NotchAngle = new Element<double>();
        public Element<double> NotchAngle
        {
            get { return _NotchAngle; }
            set { _NotchAngle = value; }
        }

        private Element<string> _NotchType = new Element<string>();
        public Element<string> NotchType
        {
            get { return _NotchType; }
            set { _NotchType = value; }
        }

        private Element<NotchDriectionEnum> _NotchDirection = new Element<NotchDriectionEnum>();
        public Element<NotchDriectionEnum> NotchDirection
        {
            get { return _NotchDirection; }
            set { _NotchDirection = value; }
        }

        private Element<double> _NotchAngleOffset = new Element<double>();
        public Element<double> NotchAngleOffset
        {
            get { return _NotchAngleOffset; }
            set { _NotchAngleOffset = value; }
        }

        private CatCoordinates _CenDieOffset = new CatCoordinates();
        public CatCoordinates CenDieOffset
        {
            get { return _CenDieOffset; }
            set { _CenDieOffset = value; }
        }

        private ElemUserIndex _CenU = new ElemUserIndex();
        public ElemUserIndex CenU
        {
            get { return _CenU; }
            set { _CenU = value; }
        }

        private ElemUserIndex _OrgU = new ElemUserIndex();
        public ElemUserIndex OrgU
        {
            get { return _OrgU; }
            set { _OrgU = value; }
        }

        private ElemUserIndex _RefU = new ElemUserIndex();
        public ElemUserIndex RefU
        {
            get { return _RefU; }
            set { _RefU = value; }
        }

        private ElemMachineIndex _CenM = new ElemMachineIndex();
        public ElemMachineIndex CenM
        {
            get { return _CenM; }
            set { _CenM = value; }
        }

        private ObservableCollection<ElemUserIndex> _TargetUs = new ObservableCollection<ElemUserIndex>();
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public ObservableCollection<ElemUserIndex> TargetUs
        {
            get { return _TargetUs; }
            set { _TargetUs = value; }
        }

        private ElemMachineIndex _OrgM = new ElemMachineIndex();
        public ElemMachineIndex OrgM
        {
            get { return _OrgM; }
            set { _OrgM = value; }
        }

        //private Element<int> _SlotIndex = new Element<int>();
        //public Element<int> SlotIndex
        //{
        //    get { return _SlotIndex; }
        //    set { _SlotIndex = value; }
        //}

        private Element<double> _Thickness = new Element<double>();
        public Element<double> Thickness
        {
            get { return _Thickness; }
            set { _Thickness = value; }
        }

        private Element<double> _ThicknessTolerance = new Element<double>() { Value = 200 };
        public Element<double> ThicknessTolerance
        {
            get { return _ThicknessTolerance; }
            set { _ThicknessTolerance = value; }
        }

        //private Element<DateTime> _UnloadTime = new Element<DateTime>();
        //public Element<DateTime> UnloadTime
        //{
        //    get { return _UnloadTime; }
        //    set { _UnloadTime = value; }
        //}

        //private Element<string> _WaferID = new Element<string>();
        //public Element<string> WaferID
        //{
        //    get { return _WaferID; }
        //    set { _WaferID = value; }
        //}

        private EnumWaferSize _WaferSizeEnum;
        [XmlIgnore, JsonIgnore]
        public virtual EnumWaferSize WaferSizeEnum
        {
            get { return _WaferSizeEnum; }
            set
            {
                if (value != _WaferSizeEnum)
                {
                    _WaferSizeEnum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _WaferSize_um = new Element<double>();
        public Element<double> WaferSize_um
        {
            get { return _WaferSize_um; }
            set { _WaferSize_um = value; }
        }

        private Element<double> _WaferSize_Offset_um = new Element<double>();
        public Element<double> WaferSize_Offset_um
        {
            get { return _WaferSize_Offset_um; }
            set { _WaferSize_Offset_um = value; }
        }

        private Element<double> _WaferMargin_um = new Element<double>();
        public virtual Element<double> WaferMargin_um
        {
            get { return _WaferMargin_um; }
            set { _WaferMargin_um = value; }
        }

        private Element<double> _FramedNotchAngle = new Element<double>();
        public virtual Element<double> FramedNotchAngle
        {
            get { return _FramedNotchAngle; }
            set
            {
                if (value != _FramedNotchAngle)
                {
                    _FramedNotchAngle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WaferCoordinate _LowLeftCorner = new WaferCoordinate();
        public WaferCoordinate LowLeftCorner
        {
            get { return _LowLeftCorner; }
            set
            {
                if (value != _LowLeftCorner)
                {
                    _LowLeftCorner = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<int> _MapCountX = new Element<int>();
        public Element<int> MapCountX
        {
            get { return _MapCountX; }
            set
            {
                if (value != _MapCountX)
                {
                    _MapCountX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _MapCountY = new Element<int>();
        public Element<int> MapCountY
        {
            get { return _MapCountY; }
            set
            {
                if (value != _MapCountY)
                {
                    _MapCountY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _DieSizeX = new Element<double>();
        public Element<double> DieSizeX
        {
            get { return _DieSizeX; }
            set
            {
                if (value != _DieSizeX)
                {
                    _DieSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _DieSizeY = new Element<double>();
        public Element<double> DieSizeY
        {
            get { return _DieSizeY; }
            set
            {
                if (value != _DieSizeY)
                {
                    _DieSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Element<CategoryStatsBase> _CatStatistics = new Element<CategoryStatsBase>(); 
        //public Element<CategoryStatsBase> CatStatistics
        //{
        //    get
        //    {
        //        return _CatStatistics;
        //    }
        //    set
        //    {
        //        if (_CatStatistics != value)
        //        {
        //            _CatStatistics = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Element<double> _DieXClearance = new Element<double>();
        public Element<double> DieXClearance
        {
            get { return _DieXClearance; }
            set { _DieXClearance = value; }
        }

        private Element<double> _DieYClearance = new Element<double>();
        public Element<double> DieYClearance
        {
            get { return _DieYClearance; }
            set { _DieYClearance = value; }
        }

        private Element<SubstrateType> _SubstrateType = new Element<SubstrateType>();
        public Element<SubstrateType> SubstrateType
        {
            get { return _SubstrateType; }
            set
            {
                if (value != _SubstrateType)
                {
                    _SubstrateType = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<WaferSubstrateTypeEnum> _WaferSubstrateType = new Element<WaferSubstrateTypeEnum>();
        public Element<WaferSubstrateTypeEnum> WaferSubstrateType
        {
            get { return _WaferSubstrateType; }
            set
            {
                if (value != _WaferSubstrateType)
                {
                    _WaferSubstrateType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<EnumPadType> _PadType = new Element<EnumPadType>();
        public Element<EnumPadType> PadType
        {
            get { return _PadType; }
            set
            {
                if (value != _PadType)
                {
                    _PadType = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _BumpPadHeight = new Element<double>();
        public Element<double> BumpPadHeight
        {
            get { return _BumpPadHeight; }
            set
            {
                if (value != _BumpPadHeight)
                {
                    _BumpPadHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<OCRModeEnum> _OCRMode = new Element<OCRModeEnum>();
        public Element<OCRModeEnum> OCRMode
        {
            get { return _OCRMode; }
            set
            {
                if (value != _OCRMode)
                {
                    _OCRMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<OCRTypeEnum> _OCRType = new Element<OCRTypeEnum>();
        public Element<OCRTypeEnum> OCRType
        {
            get { return _OCRType; }
            set
            {
                if (value != _OCRType)
                {
                    _OCRType = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<OCRDirectionEnum> _OCRDirection = new Element<OCRDirectionEnum>();
        public Element<OCRDirectionEnum> OCRDirection
        {
            get { return _OCRDirection; }
            set
            {
                if (value != _OCRDirection)
                { 
                    _OCRDirection = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _UnLoadingAngle = new Element<double>();
        public Element<double> UnLoadingAngle
        {
            get { return _UnLoadingAngle; }
            set { _UnLoadingAngle = value; }
        }

        private Element<MachineIndex> _TeachDieMIndex = new Element<MachineIndex>() {Value = new MachineIndex(-1,-1)};
        public Element<MachineIndex> TeachDieMIndex
        {
            get { return _TeachDieMIndex; }
            set
            {
                if (value != _TeachDieMIndex)
                {
                    _TeachDieMIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _OCRAngle = new Element<double>();
        public Element<double> OCRAngle
        {
            get { return _OCRAngle; }
            set { _OCRAngle = value; }
        }

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

        public PhysicalInfo()
        {

        }

        //public EventCodeEnum SetEmulParam()
        //{
        //    return SetDefaultParam();
        //}

        //public EventCodeEnum SetDefaultParam()
        //{
        //    //WaferSize.Value = 200000;
        //    //DeviceSizeWidth.Value = 5420.166666666667;
        //    //DeviceSizeHeight.Value = 5369.5625;
        //    //Thickness.Value = 560;
        //    //Thickness = 560;

        //    WaferSize_um.Value = 300000; // 12INCH
        //    DieSizeX.Value = 7177;
        //    DieSizeY.Value = 8460;
        //    Thickness.Value = 768;
        //    NotchType.Value = WaferNotchTypeEnum.NOTCH.ToString();

        //    WaferMargin_um.Value = 5000;
        //    FramedNotchAngle.Value = 0;
        //    IndexAlignType.Value = 0;
        //    LowLeftCorner = new WaferCoordinate();

        //    CassetteID.Value = "";
        //    WaferID.Value = "";

        //    MapCountY.Value = Convert.ToInt32(Math.Ceiling
        //       ((double)WaferSize_um.Value / DieSizeY.Value));

        //    MapCountX.Value = Convert.ToInt32(Math.Ceiling
        //        ((double)WaferSize_um.Value / DieSizeX.Value));

        //    CenM.XIndex.Value = MapCountX.Value / 2;
        //    CenM.YIndex.Value = MapCountY.Value / 2;

        //    CenU.XIndex.Value = MapCountX.Value / 2;
        //    CenU.YIndex.Value = MapCountY.Value / 2;

        //    OrgM.XIndex.Value = CenM.XIndex.Value;
        //    OrgM.YIndex.Value = CenM.YIndex.Value;

        //    OrgU.XIndex.Value = CenU.XIndex.Value;
        //    OrgU.YIndex.Value = CenU.YIndex.Value;

        //    MapDirX.Value = MapHorDirectionEnum.RIGHT;
        //    MapDirY.Value = MapVertDirectionEnum.UP;

        //    return EventCodeEnum.NONE;

        //    #region ==> Backup

        //    ////==> Param Element Init
        //    //NotchAngle = new DblParamElement();
        //    //ParameterCrawler.CrawlingParamSetting(this);
        //    ////==>

        //    //DeviceSize = new RectSize();
        //    //DeviceSize.Width = 6768.761873;
        //    //DeviceSize.Height = 6690.280437;
        //    ////PhysInfo.DeviceSize.Width = 19898.6;
        //    ////PhysInfo.DeviceSize.Height = 19906.7;
        //    ////PhysInfo.DeviceSize.Width = 2000;
        //    ////PhysInfo.DeviceSize.Height = 2001;

        //    //NotchType = WaferNotchTypeEnum.NOTCH;
        //    //WaferSize = 200000; // 8INCH
        //    ////PhysInfo.WaferSize = 300000; // 12INCH
        //    //WaferMargin = 5000;
        //    //Thickness = 720;
        //    //FramedNotchAngle = 0;
        //    ////NotchAngle = 90;
        //    //IndexAlignType = 0;
        //    //Corner = new WaferCoordinate();
        //    //VertDieCount = Convert.ToInt64(Math.Ceiling
        //    //   ((double)WaferSize / DeviceSize.Height));

        //    //HorDieCount = Convert.ToInt64(Math.Ceiling
        //    //    ((double)WaferSize / DeviceSize.Width));

        //    //Thickness = Thickness;

        //    //CarrierID = "";
        //    //WaferID = "";
        //    //RefMIndex.XIndex = VertDieCount / 2;
        //    //RefMIndex.YIndex = HorDieCount / 2;

        //    //RefUIndex.XIndex = VertDieCount / 2;
        //    //RefUIndex.YIndex = HorDieCount / 2;

        //    //OrgMIndex.XIndex = RefMIndex.XIndex;
        //    //OrgMIndex.YIndex = RefMIndex.YIndex;

        //    //OrgUIndex.XIndex = RefUIndex.XIndex;
        //    //OrgUIndex.YIndex = RefUIndex.YIndex;

        //    //MapHorDir = MapHorDirectionEnum.RIGHT;
        //    //MapVertDir = MapVertDirectionEnum.UP;

        //    //return EventCodeEnum.NONE;
        //    #endregion
        //}
    }
}
