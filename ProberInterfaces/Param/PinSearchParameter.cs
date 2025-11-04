using LogModule;

using Newtonsoft.Json;
using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Serialization;

namespace ProberInterfaces.Param
{
    [Serializable]
    [XmlInclude(typeof(PinSearchParameter))]
    public class PinSearchParameter : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


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
        public List<object> Nodes { get; set; }
        private Element<Rect> _PinSize
             = new Element<Rect>();
        public Element<Rect> PinSize
        {
            get { return _PinSize; }
            set
            {
                if (value != _PinSize)
                {
                    _PinSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<Rect> _BaseSize
             = new Element<Rect>();
        public Element<Rect> BaseSize
        {
            get { return _BaseSize; }
            set
            {
                if (value != _BaseSize)
                {
                    _BaseSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<Rect> _PlateSize
             = new Element<Rect>();
        public Element<Rect> PlateSize
        {
            get { return _PlateSize; }
            set
            {
                if (value != _PlateSize)
                {
                    _PlateSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<Rect> _HighAlignKeySize
             = new Element<Rect>();
        public Element<Rect> HighAlignKeySize
        {
            get { return _HighAlignKeySize; }
            set
            {
                if (value != _HighAlignKeySize)
                {
                    _HighAlignKeySize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PinCoordinate _PinPosition;
        public PinCoordinate PinPosition
        {
            get { return _PinPosition; }
            set
            {
                if (value != _PinPosition)
                {
                    _PinPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PinCoordinate _HighAlignkeyPosition;
        public PinCoordinate HighAlignkeyPosition
        {
            get { return _HighAlignkeyPosition; }
            set
            {
                if (value != _HighAlignkeyPosition)
                {
                    _HighAlignkeyPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PinCoordinate _Offset;
        public PinCoordinate Offset
        {
            get { return _Offset; }
            set
            {
                if (value != _Offset)
                {
                    _Offset = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<Rect> _SearchArea
             = new Element<Rect>();
        public Element<Rect> SearchArea
        {
            get { return _SearchArea; }
            set
            {
                if (value != _SearchArea)
                {
                    _SearchArea = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<Rect> _PinFocusingArea
             = new Element<Rect>();
        public Element<Rect> PinFocusingArea
        {
            get { return _PinFocusingArea; }
            set
            {
                if (value != _PinFocusingArea)
                {
                    _PinFocusingArea = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Element<int> _LightForTip
        //     = new Element<int>();
        //public Element<int> Light
        //{
        //    get { return _LightForTip; }
        //    set
        //    {
        //        if (value != _LightForTip)
        //        {
        //            _LightForTip = value;
        //            NotifyPropertyChanged("Light");
        //        }
        //    }
        //}

        private List<LightValueParam> _LightForTip = new List<LightValueParam>();
        public List<LightValueParam> LightForTip
        {
            get { return _LightForTip; }
            set
            {
                if (value != _LightForTip)
                {
                    _LightForTip = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum AddLight(LightValueParam light)
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            try
            {
                if (LightForTip.Count(i => i.Type.Value == light.Type.Value) > 0)
                {
                    if (LightForTip.Count(i => i.Type.Value == light.Type.Value) == 1)
                    {
                        LightForTip.FirstOrDefault(i => i.Type.Value == light.Type.Value).Value.Value = light.Value.Value;
                    }
                    else
                    {
                        LightForTip.RemoveAll(i => i.Type.Value == light.Type.Value);
                        LightForTip.Add(light);
                    }
                }
                else
                {
                    LightForTip.Add(light);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                throw err;
            }
            return retval;
        }

        private Element<int> _BlobThreshold
             = new Element<int>();
        public Element<int> BlobThreshold
        {
            get { return _BlobThreshold; }
            set
            {
                if (value != _BlobThreshold)
                {
                    _BlobThreshold = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _MinBlobSizeX
             = new Element<int>();
        public Element<int> MinBlobSizeX
        {
            get { return _MinBlobSizeX; }
            set
            {
                if (value != _MinBlobSizeX)
                {
                    _MinBlobSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _MinBlobSizeY
             = new Element<int>();
        public Element<int> MinBlobSizeY
        {
            get { return _MinBlobSizeY; }
            set
            {
                if (value != _MinBlobSizeY)
                {
                    _MinBlobSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _MaxBlobSizeX
             = new Element<int>();
        public Element<int> MaxBlobSizeX
        {
            get { return _MaxBlobSizeX; }
            set
            {
                if (value != _MaxBlobSizeX)
                {
                    _MaxBlobSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<int> _MaxBlobSizeY
             = new Element<int>();
        public Element<int> MaxBlobSizeY
        {
            get { return _MaxBlobSizeY; }
            set
            {
                if (value != _MaxBlobSizeY)
                {
                    _MaxBlobSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _TipSizeX
             = new Element<int>();
        public Element<int> TipSizeX
        {
            get { return _TipSizeX; }
            set
            {
                if (value != _TipSizeX)
                {
                    _TipSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<int> _TipSizeY
             = new Element<int>();
        public Element<int> TipSizeY
        {
            get { return _TipSizeY; }
            set
            {
                if (value != _TipSizeY)
                {
                    _TipSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        //PROBECARD_TYPE CurProbeCardType;
        private Element<PROBECARD_TYPE> _CurProbeCardType;
        public Element<PROBECARD_TYPE> CurProbeCardType
        {
            get { return _CurProbeCardType; }
            set
            {
                if (value != _CurProbeCardType)
                {
                    _CurProbeCardType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _AlignKeyIndex = new Element<int>();
        public Element<int> AlignKeyIndex
        {
            get { return _AlignKeyIndex; }
            set
            {
                if (value != _AlignKeyIndex)
                {
                    _AlignKeyIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<AlignKeyInfo> _AlignKeyHigh
            = new List<AlignKeyInfo>();
        public List<AlignKeyInfo> AlignKeyHigh
        {
            get { return _AlignKeyHigh; }
            set
            {
                if (value != _AlignKeyHigh)
                {
                    _AlignKeyHigh = value;
                    RaisePropertyChanged();
                }
            }
        }

        private BaseInfo _BaseParam
            = new BaseInfo();
        public BaseInfo BaseParam
        {
            get { return _BaseParam; }
            set
            {
                if (value != _BaseParam)
                {
                    _BaseParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        // Todo for pinalignment : ?????? ?????? ??
        // ???? ???????? ???κ? ??? ????? ?????? ?? ??????? ???????? ?о???? ???δ? ??? ???????.
        // ???? ??? Read?? ?????? Write??? ???? ????. (??? ????귯?? ?????? ???? ???? ???α???? ?????? ??)
        // ???? Element?? ??? ????. LightValue?? ??? ???? ?????? ??? Element?? ??? ???? ??? ???? ??? ????...
        // ???? ???????? ???κ? ??? ????? ???? ???????? ????? ?? ???? ?? ????

        [Serializable]
        public class AlignKeyLib
        {
            // ???? ?? ??? ????? ?????? ??? (???? ???????? ?? ?????)
            private PinCoordinate _AlignKeyPosition = new PinCoordinate();
            public PinCoordinate AlignKeyPosition
            {
                get { return _AlignKeyPosition; }
                set
                {
                    if (value != _AlignKeyPosition)
                    {
                        _AlignKeyPosition = value;
                    }
                }
            }

            public IMAGE_PROC_TYPE ImageProcType;
            public EnumProberCam CamType;
            public EnumThresholdType ImageBlobType;
            public PIN_OBJECT_COLOR ImageObjectColor;

            // High ????? ????? ????? (Coaxial)
            private ushort _CoaxialLight = 80;
            public ushort CoaxialLight
            {
                get { return _CoaxialLight; }
                set
                {
                    if (value != _CoaxialLight)
                    {
                        _CoaxialLight = value;
                    }
                }
            }

            // High ????? ????? ????? (?????)
            private ushort _Oblique = 0;
            public ushort Oblique
            {
                get { return _Oblique; }
                set
                {
                    if (value != _Oblique)
                    {
                        _Oblique = value;
                    }
                }
            }

            // High ????? ????? ??Ŀ?? ROI?? X (?? ???????? ??????? ??? ?????? ?? ?? ??????? ?¿?? ?????? ???????)
            private double _FocusingAreaX = 300;
            public double FocusingAreaX
            {
                get { return _FocusingAreaX; }
                set
                {
                    if (value != _FocusingAreaX)
                    {
                        _FocusingAreaX = value;
                    }
                }
            }

            // High ????? ????? ??Ŀ?? ROI?? Y (?? ???????? ??????? ??? ?????? ?? ?? ??????? ?¿?? ?????? ???????)
            private double _FocusingAreaY = 150;
            public double FocusingAreaY
            {
                get { return _FocusingAreaY; }
                set
                {
                    if (value != _FocusingAreaY)
                    {
                        _FocusingAreaY = value;
                    }
                }
            }

            // High ????? ????? ??Ŀ?? Range
            private double _FocusingRange = 300;
            public double FocusingRange
            {
                get { return _FocusingRange; }
                set
                {
                    if (value != _FocusingRange)
                    {
                        _FocusingRange = value;
                    }
                }
            }

            // High ????? ???? ?????? X
            private double _BlobSizeX = 15;
            public double BlobSizeX
            {
                get { return _BlobSizeX; }
                set
                {
                    if (value != _BlobSizeX)
                    {
                        _BlobSizeX = value;
                    }
                }
            }
            // High ????? ???? ?????? Y
            private double _BlobSizeY = 10;
            public double BlobSizeY
            {
                get { return _BlobSizeY; }
                set
                {
                    if (value != _BlobSizeY)
                    {
                        _BlobSizeY = value;
                    }
                }
            }
            // High ????? ???? ?????? ???? ???? X
            private double _BlobSizeTolX = 3;
            public double BlobSizeTolX
            {
                get { return _BlobSizeTolX; }
                set
                {
                    if (value != _BlobSizeTolX)
                    {
                        _BlobSizeTolX = value;
                    }
                }
            }
            // High ????? ???? ?????? ???? ???? Y
            private double _BlobSizeTolY = 3;
            public double BlobSizeTolY
            {
                get { return _BlobSizeTolY; }
                set
                {
                    if (value != _BlobSizeTolY)
                    {
                        _BlobSizeTolY = value;
                    }
                }
            }
            // ??? ROI ?????? X
            private double _BlobRoiSizeX
                 = new double();
            public double BlobRoiSizeX
            {
                get { return _BlobRoiSizeX; }
                set
                {
                    if (value != _BlobRoiSizeX)
                    {
                        _BlobRoiSizeX = value;
                    }
                }
            }
            // ??? ROI ?????? Y
            private double _BlobRoiSizeY
                 = new double();
            public double BlobRoiSizeY
            {
                get { return _BlobRoiSizeY; }
                set
                {
                    if (value != _BlobRoiSizeY)
                    {
                        _BlobRoiSizeY = value;
                    }
                }
            }
            // High ????? ???? ??? Acceptance
            private double _PMAcceptance = 80;
            public double PMAcceptance
            {
                get { return _PMAcceptance; }
                set
                {
                    if (value != _PMAcceptance)
                    {
                        _PMAcceptance = value;
                    }
                }
            }

            private double _BlobThreshold = 70;
            public double BlobThreshold
            {
                get { return _BlobThreshold; }
                set
                {
                    if (value != _BlobThreshold)
                    {
                        _BlobThreshold = value;
                    }
                }
            }
        }

        // ?? ??? ???????? ????????? ?? ????? ????? ??? ?? ?? ???, ?? ????? ?? ??????????? ??? ??????? ???? ??? ?? ???. 
        // ????? ??????? ???????? ?????. 
        [Serializable]
        public class AlignKeyLibPack : ISystemParameterizable
        {
            #region ==> PropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion

            private List<AlignKeyLib> _AlignKeyLib = new List<AlignKeyLib>();
            public List<AlignKeyLib> AlignKeyLib
            {
                get { return _AlignKeyLib; }
                set
                {
                    if (value != _AlignKeyLib)
                    {
                        _AlignKeyLib = value;
                    }
                }
            }

            [JsonIgnore]
            public string FilePath { get; set; } = "PinAlign";

            [JsonIgnore]
            public string FileName { get; set; } = "AlignKeyLibPack.json";

            [JsonIgnore]
            public bool IsParamChanged { get; set; }
            [JsonIgnore]
            public string Genealogy { get; set; }
            [JsonIgnore]
            public object Owner { get; set; }
            [JsonIgnore]
            public List<object> Nodes { get; set; }
             = new List<object>();

            public EventCodeEnum Init()
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                retVal = EventCodeEnum.NONE;
                return retVal;
            }

            public EventCodeEnum SetDefaultParam()
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                retVal = SetEmulParam();
                return retVal;
            }

            public void SetElementMetaData()
            {

            }

            public EventCodeEnum SetEmulParam()
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                try
                {
                    AlignKeyLib = new List<AlignKeyLib>();

                    AlignKeyLib tmpLib = new AlignKeyLib();
                    tmpLib.AlignKeyPosition.X.Value = 99;
                    tmpLib.AlignKeyPosition.Y.Value = 0;
                    tmpLib.AlignKeyPosition.Z.Value = 133;
                    tmpLib.ImageProcType = IMAGE_PROC_TYPE.PROC_BLOB;
                    tmpLib.ImageBlobType = EnumThresholdType.AUTO;
                    tmpLib.CamType = EnumProberCam.PIN_HIGH_CAM;
                    tmpLib.ImageObjectColor = PIN_OBJECT_COLOR.WHITE;
                    tmpLib.Oblique = 0;
                    tmpLib.CoaxialLight = 120;
                    tmpLib.FocusingAreaX = 100;
                    tmpLib.FocusingAreaY = 100;
                    tmpLib.FocusingRange = 300;
                    tmpLib.BlobSizeTolX = 5;
                    tmpLib.BlobSizeTolY = 5;
                    tmpLib.BlobSizeX = 16;
                    tmpLib.BlobSizeY = 10;
                    tmpLib.PMAcceptance = 80;
                    tmpLib.BlobThreshold = 70;
                    tmpLib.BlobRoiSizeX = 100;
                    tmpLib.BlobRoiSizeY = 120;
                    AlignKeyLib.Add(tmpLib);

                    // ??????? ???κ? ????? ????? ??? ?? ????.
                    tmpLib = new AlignKeyLib();
                    tmpLib.AlignKeyPosition.X.Value = 157;
                    tmpLib.AlignKeyPosition.Y.Value = 0;
                    tmpLib.AlignKeyPosition.Z.Value = 133;
                    tmpLib.ImageProcType = IMAGE_PROC_TYPE.PROC_BLOB;
                    tmpLib.ImageBlobType = EnumThresholdType.AUTO;
                    tmpLib.CamType = EnumProberCam.PIN_HIGH_CAM;
                    tmpLib.ImageObjectColor = PIN_OBJECT_COLOR.WHITE;
                    tmpLib.Oblique = 0;
                    tmpLib.CoaxialLight = 120;
                    tmpLib.FocusingAreaX = 100;
                    tmpLib.FocusingAreaY = 100;
                    tmpLib.FocusingRange = 300;
                    tmpLib.BlobSizeTolX = 5;
                    tmpLib.BlobSizeTolY = 5;
                    tmpLib.BlobSizeX = 37;
                    tmpLib.BlobSizeY = 11;
                    tmpLib.PMAcceptance = 80;
                    tmpLib.BlobThreshold = 70;
                    tmpLib.BlobRoiSizeX = 100;
                    tmpLib.BlobRoiSizeY = 120;

                    AlignKeyLib.Add(tmpLib);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }

                retVal = EventCodeEnum.NONE;
                return retVal;
            }
        }

        [Serializable]
        public class AlignKeyInfo : INotifyPropertyChanged, IParamNode
        {
            #region ==> PropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion

            public IMAGE_PROC_TYPE ImageProcType;
            public EnumThresholdType ImageBlobType;
            public PIN_OBJECT_COLOR ImageObjectColor;

            // High ????? ?????? ???? ???????? ??? ????ĸ? ????.
            // ???? ????? ??? ?????? ?????? ??? ??? ?¾? ?? ???? ????? ??? ???? ??????? ???? ?? ?? ???? ?? ?? ??? ????? ????
            // ????귯???? ???? ?????? ??? ???????? ????? ?????? 0?? ?????? ????. (?? ?????? ????? ??? ????. ?ð? ?????????? ?????? ???? 90, 180, 270)
            private Element<double> _AlignKeyAngle
                 = new Element<double>();
            public Element<double> AlignKeyAngle
            {
                get { return _AlignKeyAngle; }
                set
                {
                    if (value != _AlignKeyAngle)
                    {
                        _AlignKeyAngle = value;
                        RaisePropertyChanged();
                    }
                }
            }

            // ???? ?? ??? ????? ?????? ??? (???? ???????? ?? ?????)
            private PinCoordinate _AlignKeyPos
                 = new PinCoordinate();
            public PinCoordinate AlignKeyPos
            {
                get { return _AlignKeyPos; }
                set
                {
                    if (value != _AlignKeyPos)
                    {
                        _AlignKeyPos = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private Element<Rect> _KeySearchArea
            = new Element<Rect>();
            public Element<Rect> KeySearchArea
            {
                get { return _KeySearchArea; }
                set
                {
                    if (value != _KeySearchArea)
                    {
                        _KeySearchArea = value;
                        RaisePropertyChanged();
                    }
                }
            }

            // 얼라인 패턴용 포커싱 ROI값 X
            private Element<double> _FocusingAreaSizeX
                 = new Element<double>();
            public Element<double> FocusingAreaSizeX
            {
                get { return _FocusingAreaSizeX; }
                set
                {
                    if (value != _FocusingAreaSizeX)
                    {
                        _FocusingAreaSizeX = value;
                        RaisePropertyChanged();
                    }
                }
            }
            // ????? ????? ??Ŀ?? ROI?? Y
            private Element<double> _FocusingAreaSizeY
                 = new Element<double>();
            public Element<double> FocusingAreaSizeY
            {
                get { return _FocusingAreaSizeY; }
                set
                {
                    if (value != _FocusingAreaSizeY)
                    {
                        _FocusingAreaSizeY = value;
                        RaisePropertyChanged();
                    }
                }
            }

            // ????? ????? ??Ŀ?? Range??
            private Element<double> _FocusingRange
                 = new Element<double>();
            public Element<double> FocusingRange
            {
                get { return _FocusingRange; }
                set
                {
                    if (value != _FocusingRange)
                    {
                        _FocusingRange = value;
                        RaisePropertyChanged();
                    }
                }
            }
            // ????? ???? ?????? X
            private Element<double> _BlobSizeX
                 = new Element<double>();
            public Element<double> BlobSizeX
            {
                get { return _BlobSizeX; }
                set
                {
                    if (value != _BlobSizeX)
                    {
                        _BlobSizeX = value;
                        RaisePropertyChanged();
                    }
                }
            }
            // ????? ???? ?????? Y
            private Element<double> _BlobSizeY
                 = new Element<double>();
            public Element<double> BlobSizeY
            {
                get { return _BlobSizeY; }
                set
                {
                    if (value != _BlobSizeY)
                    {
                        _BlobSizeY = value;
                        RaisePropertyChanged();
                    }
                }
            }
            // ???? ?????? ???? ???? X
            private Element<double> _BlobSizeTolX
                 = new Element<double>();
            public Element<double> BlobSizeTolX
            {
                get { return _BlobSizeTolX; }
                set
                {
                    if (value != _BlobSizeTolX)
                    {
                        _BlobSizeTolX = value;
                        RaisePropertyChanged();
                    }
                }
            }
            // ???? ?????? ???? ???? Y
            private Element<double> _BlobSizeTolY
                 = new Element<double>();
            public Element<double> BlobSizeTolY
            {
                get { return _BlobSizeTolY; }
                set
                {
                    if (value != _BlobSizeTolY)
                    {
                        _BlobSizeTolY = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private Element<double> _BlobSizeMinX = new Element<double>();
            public Element<double> BlobSizeMinX
            {
                get { return _BlobSizeMinX; }
                set
                {
                    if (value != _BlobSizeMinX)
                    {
                        _BlobSizeMinX = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private Element<double> _BlobSizeMinY = new Element<double>();
            public Element<double> BlobSizeMinY
            {
                get { return _BlobSizeMinY; }
                set
                {
                    if (value != _BlobSizeMinY)
                    {
                        _BlobSizeMinY = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private Element<double> _BlobSizeMaxX = new Element<double>();
            public Element<double> BlobSizeMaxX
            {
                get { return _BlobSizeMaxX; }
                set
                {
                    if (value != _BlobSizeMaxX)
                    {
                        _BlobSizeMaxX = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private Element<double> _BlobSizeMaxY = new Element<double>();
            public Element<double> BlobSizeMaxY
            {
                get { return _BlobSizeMaxY; }
                set
                {
                    if (value != _BlobSizeMaxY)
                    {
                        _BlobSizeMaxY = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private Element<double> _BlobSizeMin = new Element<double>();
            public Element<double> BlobSizeMin
            {
                get { return _BlobSizeMin; }
                set
                {
                    if (value != _BlobSizeMin)
                    {
                        _BlobSizeMin = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private Element<double> _BlobSizeMax
                = new Element<double>();
            public Element<double> BlobSizeMax
            {
                get { return _BlobSizeMax; }
                set
                {
                    if (value != _BlobSizeMax)
                    {
                        _BlobSizeMax = value;
                        RaisePropertyChanged();
                    }
                }
            }

            // ??? ROI ?????? X
            private Element<double> _BlobRoiSizeX
                 = new Element<double>();
            public Element<double> BlobRoiSizeX
            {
                get { return _BlobRoiSizeX; }
                set
                {
                    if (value != _BlobRoiSizeX)
                    {
                        _BlobRoiSizeX = value;
                        RaisePropertyChanged();
                    }
                }
            }
            // ??? ROI ?????? Y
            private Element<double> _BlobRoiSizeY
                 = new Element<double>();
            public Element<double> BlobRoiSizeY
            {
                get { return _BlobRoiSizeY; }
                set
                {
                    if (value != _BlobRoiSizeY)
                    {
                        _BlobRoiSizeY = value;
                        RaisePropertyChanged();
                    }
                }
            }
            // ????? Threshold
            private Element<double> _BlobThreshold
                 = new Element<double>();
            public Element<double> BlobThreshold
            {
                get { return _BlobThreshold; }
                set
                {
                    if (value != _BlobThreshold)
                    {
                        _BlobThreshold = value;
                        RaisePropertyChanged();
                    }
                }
            }
            // ???? ????. (???ο? ??? ?? ????, Acceptance ?? ?????? ??? ?????? ???)
            private PatternInfomation _PatternIfo = new PatternInfomation();
            public PatternInfomation PatternIfo
            {
                get { return _PatternIfo; }
                set
                {
                    if (value != _PatternIfo)
                    {
                        _PatternIfo = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private Element<int> _FoundGrayLevelForFocusing = new Element<int>();
            public Element<int> FoundGrayLevelForFocusing
            {
                get { return _FoundGrayLevelForFocusing; }
                set
                {
                    if (value != _FoundGrayLevelForFocusing)
                    {
                        _FoundGrayLevelForFocusing = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private Element<int> _FoundGrayLevelForBlob = new Element<int>();
            public Element<int> FoundGrayLevelForBlob
            {
                get { return _FoundGrayLevelForBlob; }
                set
                {
                    if (value != _FoundGrayLevelForBlob)
                    {
                        _FoundGrayLevelForBlob = value;
                        RaisePropertyChanged();
                    }
                }
            }

            public string Genealogy { get; set; } = string.Empty;
            public object Owner { get; set; }
            public List<object> Nodes { get; set; }
                = new List<object>();
        }

        [Serializable]
        public class BaseInfo : INotifyPropertyChanged, IParamNode
        {
            #region ==> PropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
            public string Genealogy { get; set; } = string.Empty;
            public object Owner { get; set; }
            public List<object> Nodes { get; set; } = new List<object>();

            private double _DistanceBaseAndTip;
            public double DistanceBaseAndTip
            {
                get { return _DistanceBaseAndTip; }
                set
                {
                    if (value != _DistanceBaseAndTip)
                    {
                        _DistanceBaseAndTip = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private double _BaseOffsetX;
            public double BaseOffsetX
            {
                get { return _BaseOffsetX; }
                set
                {
                    if (value != _BaseOffsetX)
                    {
                        _BaseOffsetX = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private double _BaseOffsetY;
            public double BaseOffsetY
            {
                get { return _BaseOffsetY; }
                set
                {
                    if (value != _BaseOffsetY)
                    {
                        _BaseOffsetY = value;
                        RaisePropertyChanged();
                    }
                }
            }
            private PinCoordinate _BasePos = new PinCoordinate();
            public PinCoordinate BasePos
            {
                get { return _BasePos; }
                set
                {
                    if (value != _BasePos)
                    {
                        _BasePos = value;
                        RaisePropertyChanged();
                    }
                }
            }

            //private PatternInfomation _PatternIfo = new PatternInfomation();
            //public PatternInfomation PatternIfo
            //{
            //    get { return _PatternIfo; }
            //    set
            //    {
            //        if (value != _PatternIfo)
            //        {
            //            _PatternIfo = value;
            //            RaisePropertyChanged();
            //        }
            //    }
            //}

            private Element<int> _FoundGrayLevelForFocusing = new Element<int>();
            public Element<int> FoundGrayLevelForFocusing
            {
                get { return _FoundGrayLevelForFocusing; }
                set
                {
                    if (value != _FoundGrayLevelForFocusing)
                    {
                        _FoundGrayLevelForFocusing = value;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        public PinSearchParameter()
        {
            try
            {
                _PinSize.Value = new Rect(0, 0, 0, 0);
                _BaseSize.Value = new Rect(0, 0, 0, 0);
                _PlateSize.Value = new Rect(0, 0, 0, 0);
                _HighAlignKeySize.Value = new Rect(0, 0, 0, 0);
                _PinPosition = new PinCoordinate(0, 0, 0);
                _HighAlignkeyPosition = new PinCoordinate(0, 0, 0);
                _Offset = new PinCoordinate(0, 0, 0);
                _SearchArea.Value = new Rect(0, 0, 0, 0);
                _PinFocusingArea.Value = new Rect(0, 0, 0, 0);
                //_LightForTip.Value = 75;

                _LightForTip = new List<LightValueParam>();


                _MaxBlobSizeX = new Element<int>();
                _MaxBlobSizeX.UpperLimit = 890;
                _MaxBlobSizeX.LowerLimit = 0;

                _MinBlobSizeX = new Element<int>();
                _MinBlobSizeX.UpperLimit = 890;
                _MinBlobSizeX.LowerLimit = 0;

                _MaxBlobSizeY = new Element<int>();
                _MaxBlobSizeY.UpperLimit = 890;
                _MaxBlobSizeY.LowerLimit = 0;

                _MinBlobSizeY = new Element<int>();
                _MinBlobSizeY.UpperLimit = 890;
                _MinBlobSizeY.LowerLimit = 0;

                _BlobThreshold = new Element<int>();
                _BlobThreshold.UpperLimit = 890;
                _BlobThreshold.LowerLimit = 0;

                AlignKeyIndex = new Element<int>();
                AlignKeyIndex.Value = 0;
                AlignKeyIndex.Description = "High alignment key index";
                AlignKeyIndex.LowerLimit = 0;
                AlignKeyIndex.UpperLimit = 100;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public PinSearchParameter(PinSearchParameter param)
        {
            try
            {
                _PinSize = param.PinSize;
                _BaseSize = param.BaseSize;
                _PlateSize = param.PlateSize;
                _HighAlignKeySize = param.HighAlignKeySize;
                _PinPosition = new PinCoordinate(param.PinPosition);
                _HighAlignkeyPosition = new PinCoordinate(param.HighAlignkeyPosition);
                _Offset = new PinCoordinate(param.Offset);
                _SearchArea.Value = new Rect(param.SearchArea.Value.X, param.SearchArea.Value.Y,
                    param.SearchArea.Value.Width, param.SearchArea.Value.Height);
                _PinFocusingArea.Value = new Rect(param.PinFocusingArea.Value.X, param.PinFocusingArea.Value.Y,
                    param.PinFocusingArea.Value.Width, param.PinFocusingArea.Value.Height);
                _LightForTip = param.LightForTip;

                _MaxBlobSizeX = new Element<int>();
                _MaxBlobSizeX.UpperLimit = 890;
                _MaxBlobSizeX.LowerLimit = 0;
                _MaxBlobSizeX.Value = param.MaxBlobSizeX.Value;

                _MinBlobSizeX = new Element<int>();
                _MinBlobSizeX.UpperLimit = 890;
                _MinBlobSizeX.LowerLimit = 0;
                _MinBlobSizeX.Value = param._MinBlobSizeX.Value;

                _MaxBlobSizeY = new Element<int>();
                _MaxBlobSizeY.UpperLimit = 890;
                _MaxBlobSizeY.LowerLimit = 0;
                _MaxBlobSizeY.Value = param._MaxBlobSizeY.Value;

                _MinBlobSizeY = new Element<int>();
                _MinBlobSizeY.UpperLimit = 890;
                _MinBlobSizeY.LowerLimit = 0;
                _MinBlobSizeY.Value = param._MinBlobSizeY.Value;

                _BlobThreshold = new Element<int>();
                _BlobThreshold.UpperLimit = 890;
                _BlobThreshold.LowerLimit = 0;
                _BlobThreshold.Value = param._BlobThreshold.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
