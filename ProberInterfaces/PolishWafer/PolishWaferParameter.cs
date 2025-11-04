//using ProberInterfaces.Param;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Serialization;

//namespace ProberInterfaces.PolishWafer
//{
//    public class PolishWaferParameter : ICloneable, INotifyPropertyChanged
//    {
//        public event PropertyChangedEventHandler PropertyChanged;
//        private void NotifyPropertyChanged(String info)
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
//        }
//        public PolishWaferParameter()
//        {

//        }
//        public PolishWaferParameter(int idx)
//        {
//            _ID = "PolishWafer" + idx;
//        }
//        private string _ID;
//        public string ID
//        {
//            get { return _ID; }
//            set
//            {
//                if (value != _ID)
//                {
//                    _ID = value;
//                    NotifyPropertyChanged("ID");
//                }
//            }
//        }

//        private EnumEnable _Enable;
//        public EnumEnable Enable
//        {
//            get { return _Enable; }
//            set
//            {
//                if (value != _Enable)
//                {
//                    _Enable = value;
//                    NotifyPropertyChanged("Enable");
//                }
//            }
//        }


//        private double _WaferSize;
//        public double WaferSize
//        {
//            get { return _WaferSize; }
//            set
//            {
//                if (value != _WaferSize)
//                {
//                    _WaferSize = value;
//                    NotifyPropertyChanged("WaferSize");
//                }
//            }
//        }


//        private double _Thickness;
//        public double Thickness
//        {
//            get { return _Thickness; }
//            set
//            {
//                if (value != _Thickness)
//                {
//                    _Thickness = value;
//                    NotifyPropertyChanged("Thickness");
//                }
//            }
//        }

//        private int _PositionIndex; //Fixed Tray Index
//        public int PositionIndex
//        {
//            get { return _PositionIndex; }
//            set
//            {
//                if (value != _PositionIndex)
//                {
//                    _PositionIndex = value;
//                    NotifyPropertyChanged("PositionIndex");
//                }
//            }
//        }


//        private double _WaferInterval;
//        public double WaferInterval
//        {
//            get { return _WaferInterval; }
//            set
//            {
//                if (value != _WaferInterval)
//                {
//                    _WaferInterval = value;
//                    NotifyPropertyChanged("WaferInterval");
//                }
//            }
//        }

//        private PWIntervalMode _IntervalMode;
//        public PWIntervalMode IntervalMode
//        {
//            get { return _IntervalMode; }
//            set
//            {
//                if (value != _IntervalMode)
//                {
//                    _IntervalMode = value;
//                    NotifyPropertyChanged("IntervalMode");
//                }
//            }
//        }


//        private double _FocusingTolerance;
//        public double FocusingTolerance
//        {
//            get { return _FocusingTolerance; }
//            set
//            {
//                if (value != _FocusingTolerance)
//                {
//                    _FocusingTolerance = value;
//                    NotifyPropertyChanged("FocusingTolerance");
//                }
//            }
//        }

//        private double _FocusingRangeOffset;
//        public double FocusingRangeOffset
//        {
//            get { return _FocusingRangeOffset; }
//            set
//            {
//                if (value != _FocusingRangeOffset)
//                {
//                    _FocusingRangeOffset = value;
//                    NotifyPropertyChanged("FocusingRangeOffset");
//                }
//            }
//        }

//        private double _LightValue;
//        public double LightValue
//        {
//            get { return _LightValue; }
//            set
//            {
//                if (value != _LightValue)
//                {
//                    _LightValue = value;
//                    NotifyPropertyChanged("LightValue");
//                }
//            }
//        }

//        private double _Acceleration;
//        public double Acceleration
//        {
//            get { return _Acceleration; }
//            set
//            {
//                if (value != _Acceleration)
//                {
//                    _Acceleration = value;
//                    NotifyPropertyChanged("Acceleration");
//                }
//            }
//        }


//        private PWFocusingMode _FocusingMode;
//        public PWFocusingMode FocusingMode
//        {
//            get { return _FocusingMode; }
//            set
//            {
//                if (value != _FocusingMode)
//                {
//                    _FocusingMode = value;
//                    NotifyPropertyChanged("FocusingMode");
//                }
//            }
//        }
//        private double _ContactNumber;
//        public double ContactNumber
//        {
//            get { return _ContactNumber; }
//            set
//            {
//                if (value != _ContactNumber)
//                {
//                    _ContactNumber = value;
//                    NotifyPropertyChanged("ContactNumber");
//                }
//            }
//        }

//        private double _CleaningLength;
//        public double CleaningLength
//        {
//            get { return _CleaningLength; }
//            set
//            {
//                if (value != _CleaningLength)
//                {
//                    _CleaningLength = value;
//                    NotifyPropertyChanged("CleaningLength");
//                }
//            }
//        }

//        private PWOverdriveMode _OverdriveMode;
//        public PWOverdriveMode OverdriveMode
//        {
//            get { return _OverdriveMode; }
//            set
//            {
//                if (value != _OverdriveMode)
//                {
//                    _OverdriveMode = value;
//                    NotifyPropertyChanged("OverdriveMode");
//                }
//            }
//        }
//        private double _OverdriveValue;
//        public double OverdriveValue
//        {
//            get { return _OverdriveValue; }
//            set
//            {
//                if (value != _OverdriveValue)
//                {
//                    _OverdriveValue = value;
//                    NotifyPropertyChanged("OverdriveValue");
//                }
//            }
//        }

//        private double _WaferOverdriveOffset;
//        public double WaferOverdriveOffset
//        {
//            get { return _WaferOverdriveOffset; }
//            set
//            {
//                if (value != _WaferOverdriveOffset)
//                {
//                    _WaferOverdriveOffset = value;
//                    NotifyPropertyChanged("WaferOverdriveOffset");
//                }
//            }
//        }

//        private double _Clearance;
//        public double Clearance
//        {
//            get { return _Clearance; }
//            set
//            {
//                if (value != _Clearance)
//                {
//                    _Clearance = value;
//                    NotifyPropertyChanged("Clearance");
//                }
//            }
//        }

//        private double _ScrubingLength;
//        public double ScrubingLength
//        {
//            get { return _ScrubingLength; }
//            set
//            {
//                if (value != _ScrubingLength)
//                {
//                    _ScrubingLength = value;
//                    NotifyPropertyChanged("ScrubingLength");
//                }
//            }
//        }

//        private double _TotalRotationAngle;
//        public double TotalRotationAngle
//        {
//            get { return _TotalRotationAngle; }
//            set
//            {
//                if (value != _TotalRotationAngle)
//                {
//                    _TotalRotationAngle = value;
//                    NotifyPropertyChanged("TotalRotationAngle");
//                }
//            }
//        }
//        private double _RotationAngle;
//        public double RotationAngle
//        {
//            get { return _RotationAngle; }
//            set
//            {
//                if (value != _RotationAngle)
//                {
//                    _RotationAngle = value;
//                    NotifyPropertyChanged("RotationAngle");
//                }
//            }
//        }

//        private double _CurrentAngle;
//        [XmlIgnore, JsonIgnore]
//        public double CurrentAngle
//        {
//            get { return _CurrentAngle; }
//            set
//            {
//                if (value != _CurrentAngle)
//                {
//                    _CurrentAngle = value;
//                    NotifyPropertyChanged("CurrentAngle");
//                }
//            }
//        }



//        private PWCleaningMode _CleaningMode;
//        public PWCleaningMode CleaningMode
//        {
//            get { return _CleaningMode; }
//            set
//            {
//                if (value != _CleaningMode)
//                {
//                    _CleaningMode = value;
//                    NotifyPropertyChanged("CleaningMode");
//                }
//            }
//        }

//        private PWCleaningDirection _CleaningDirection;
//        public PWCleaningDirection CleaningDirection
//        {
//            get { return _CleaningDirection; }
//            set
//            {
//                if (value != _CleaningDirection)
//                {
//                    _CleaningDirection = value;
//                    NotifyPropertyChanged("CleaningDirection");
//                }
//            }
//        }


//        private EnumEnable _IsPinAlignBeforePW;
//        public EnumEnable IsPinAlignBeforePW
//        {
//            get { return _IsPinAlignBeforePW; }
//            set
//            {
//                if (value != _IsPinAlignBeforePW)
//                {
//                    _IsPinAlignBeforePW = value;
//                    NotifyPropertyChanged("IsPinAlignBeforePW");
//                }
//            }
//        }
//        private EnumEnable _IsPinAlignAfterPW;
//        public EnumEnable IsPinAlignAfterPW
//        {
//            get { return _IsPinAlignAfterPW; }
//            set
//            {
//                if (value != _IsPinAlignAfterPW)
//                {
//                    _IsPinAlignAfterPW = value;
//                    NotifyPropertyChanged("IsPinAlignAfterPW");
//                }
//            }
//        }


//        //Cleaning Sequence
//        private PWContactSeqMode _ContactSeqMode;
//        public PWContactSeqMode ContactSeqMode
//        {
//            get { return _ContactSeqMode; }
//            set
//            {
//                if (value != _ContactSeqMode)
//                {
//                    _ContactSeqMode = value;
//                    NotifyPropertyChanged("ContactSeqMode");
//                }
//            }
//        }

//        private PWPadShiftMode _ShiftMode;
//        public PWPadShiftMode ShiftMode
//        {
//            get { return _ShiftMode; }
//            set
//            {
//                if (value != _ShiftMode)
//                {
//                    _ShiftMode = value;
//                    NotifyPropertyChanged("ShiftMode");
//                }
//            }
//        }

//        private double _PadShiftOffset;
//        public double PadShiftOffset
//        {
//            get { return _PadShiftOffset; }
//            set
//            {
//                if (value != _PadShiftOffset)
//                {
//                    _PadShiftOffset = value;
//                    NotifyPropertyChanged("PadShiftOffset");
//                }
//            }
//        }


//        private EnumEnable _IsUserSeq;
//        public EnumEnable IsUserSeq
//        {
//            get { return _IsUserSeq; }
//            set
//            {
//                if (value != _IsUserSeq)
//                {
//                    _IsUserSeq = value;
//                    NotifyPropertyChanged("IsUserSeq");
//                }
//            }
//        }




//        private double _CenterOffsetX;
//        [XmlIgnore, JsonIgnore]
//        public double CenterOffsetX
//        {
//            get { return _CenterOffsetX; }
//            set
//            {
//                if (value != _CenterOffsetX)
//                {
//                    _CenterOffsetX = value;
//                    NotifyPropertyChanged("CenterOffsetX");
//                }
//            }
//        }

//        private double _CenterOffsetY;
//        [XmlIgnore, JsonIgnore]
//        public double CenterOffsetY
//        {
//            get { return _CenterOffsetY; }
//            set
//            {
//                if (value != _CenterOffsetY)
//                {
//                    _CenterOffsetY = value;
//                    NotifyPropertyChanged("CenterOffsetY");
//                }
//            }
//        }

//        private double _MaxCleaningArea;
//        [XmlIgnore, JsonIgnore]
//        public double MaxCleaningArea
//        {
//            get { return _MaxCleaningArea; }
//            set
//            {
//                if (value != _MaxCleaningArea)
//                {
//                    _MaxCleaningArea = value;
//                    NotifyPropertyChanged("MaxCleaningArea");
//                }
//            }
//        }

//        private List<WaferCoordinate> _UserSeqPositions = new List<WaferCoordinate>();
//        public List<WaferCoordinate> UserSeqPositions
//        {
//            get { return _UserSeqPositions; }
//            set
//            {
//                if (value != _UserSeqPositions)
//                {
//                    _UserSeqPositions = value;
//                    NotifyPropertyChanged("UserSeqPositions");
//                }
//            }
//        }



//        public void DefaultSetting()
//        {
//            FocusingMode = PWFocusingMode.pt5;
//            WaferSize = 300000;
//            Thickness = 713;
//            PositionIndex = 0;
//            WaferInterval = 10;
//            FocusingTolerance = 1000;
//            FocusingRangeOffset = 1000;
//            LightValue = 20000;
//            ContactNumber = 10;
//            CleaningLength = 50000;
//            OverdriveValue = 200;
//            WaferOverdriveOffset = 100;
//            Clearance = -100;
//        }

//        public object Clone()
//        {
//            PolishWaferParameter param = new PolishWaferParameter();
//            param.Enable = this.Enable;
//            param.WaferSize = this.WaferSize;
//            param.Thickness = this.Thickness;
//            param.PositionIndex = this.PositionIndex;
//            param.WaferInterval = this.WaferInterval;
//            param.IntervalMode = this.IntervalMode;
//            param.FocusingTolerance = this.FocusingTolerance;
//            param.FocusingRangeOffset = this.FocusingRangeOffset;
//            param.LightValue = this.LightValue;
//            param.FocusingMode = this.FocusingMode;
//            param.ContactNumber = this.ContactNumber;
//            param.CleaningLength = this.CleaningLength;
//            param.OverdriveMode = this.OverdriveMode;
//            param.OverdriveValue = this.OverdriveValue;
//            param.WaferOverdriveOffset = this.WaferOverdriveOffset;
//            param.Clearance = this.Clearance;
//            param.ScrubingLength = this.ScrubingLength;
//            param.TotalRotationAngle = this.TotalRotationAngle;
//            param.RotationAngle = this.RotationAngle;
//            param.CleaningMode = this.CleaningMode;
//            param.CleaningDirection = this.CleaningDirection;
//            param.IsPinAlignAfterPW = this.IsPinAlignAfterPW;
//            param.IsPinAlignBeforePW = this.IsPinAlignBeforePW;
//            param.ContactSeqMode = this.ContactSeqMode;
//            param.ShiftMode = this.ShiftMode;
//            param.PadShiftOffset = this.PadShiftOffset;
//            param.IsUserSeq = this.IsUserSeq;
//            param.UserSeqPositions.Clear();
//            foreach (WaferCoordinate coord in UserSeqPositions)
//            {
//                param.UserSeqPositions.Add(coord);
//            }
//            return param;
//        }
//        public void Copy(PolishWaferParameter param)
//        {
//            this.Enable = param.Enable;
//            this.WaferSize = param.WaferSize;
//            this.Thickness = param.Thickness;
//            this.PositionIndex = param.PositionIndex;
//            this.WaferInterval = param.WaferInterval;
//            this.IntervalMode = param.IntervalMode;
//            this.FocusingTolerance = param.FocusingTolerance;
//            this.FocusingRangeOffset = param.FocusingRangeOffset;
//            this.LightValue = param.LightValue;
//            this.FocusingMode = param.FocusingMode;
//            this.ContactNumber = param.ContactNumber;
//            this.CleaningLength = param.CleaningLength;
//            this.OverdriveMode = param.OverdriveMode;
//            this.OverdriveValue = param.OverdriveValue;
//            this.WaferOverdriveOffset = param.WaferOverdriveOffset;
//            this.Clearance = param.Clearance;
//            this.ScrubingLength = param.ScrubingLength;
//            this.TotalRotationAngle = param.TotalRotationAngle;
//            this.RotationAngle = param.RotationAngle;
//            this.CleaningMode = param.CleaningMode;
//            this.CleaningDirection = param.CleaningDirection;
//            this.IsPinAlignAfterPW = param.IsPinAlignAfterPW;
//            this.IsPinAlignBeforePW = param.IsPinAlignBeforePW;
//            this.ContactSeqMode = param.ContactSeqMode;
//            this.ShiftMode = param.ShiftMode;
//            this.PadShiftOffset = param.PadShiftOffset;
//            this.IsUserSeq = param.IsUserSeq;

//            UserSeqPositions.Clear();
//            foreach (WaferCoordinate coord in param.UserSeqPositions)
//            {
//                this.UserSeqPositions.Add(coord);
//            }
//        }

//    }

//    [Serializable]
//    public class PolishWaferSequenceParameter : INotifyPropertyChanged
//    {
//        public event PropertyChangedEventHandler PropertyChanged;
//        private void NotifyPropertyChanged(String info)
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
//        }
//        public PolishWaferSequenceParameter()
//        {

//        }

//        private int _SelectNumber;
//        [XmlIgnore, JsonIgnore]
//        public int SelectNumber
//        {
//            get { return _SelectNumber; }
//            set
//            {
//                if (value != _SelectNumber)
//                {
//                    _SelectNumber = value;
//                    NotifyPropertyChanged("SelectNumber");
//                }
//            }
//        }

//        private int _TotalCount;
//        [XmlIgnore, JsonIgnore]
//        public int TotalCount
//        {
//            get { return _TotalCount; }
//            set
//            {
//                if (value != _TotalCount)
//                {
//                    _TotalCount = value;
//                    NotifyPropertyChanged("TotalCount");
//                }
//            }
//        }


//        private WaferCoordinate _SeqPos = new WaferCoordinate();
//        [XmlIgnore, JsonIgnore]
//        public WaferCoordinate SeqPos
//        {
//            get { return _SeqPos; }
//            set
//            {
//                if (value != _SeqPos)
//                {
//                    _SeqPos = value;
//                    NotifyPropertyChanged("SeqPos");
//                }
//            }
//        }
//    }
//    public enum EnumEnable
//    {
//        Disable = 0,
//        Enable = 1
//    }
//    public enum PWSize
//    {
//        NONE = 0,
//        INCH6 = 6,
//        INCH8 = 8,
//        INCH12 = 12
//    }

//    public enum PWIntervalMode
//    {
//        None,
//        LotStart,
//        WaferInterval
//    }

//    public enum PWFocusingMode
//    {
//        NONE,
//        pt5 = 5,
//        pt1 = 1
//    }

//    public enum PWOverdriveMode
//    {
//        OD,
//        OD_Minus_Offset
//    }

//    public enum PWCleaningMode
//    {
//        UP_DOWN,
//        One_Direction,
//        Octagonal,
//        Square
//    }

//    public enum PWCleaningDirection
//    {
//        Right,
//        Left,
//        Up,
//        Down,
//        Right_Up,
//        Right_Down,
//        Left_Up,
//        Left_Down
//    }
//    public enum PWContactSeqMode
//    {
//        ContactLength,
//        PositionShift
//    }
//    public enum PWPadShiftMode
//    {
//        DutSize,
//        DutSize_Total,
//        UserOffset
//    }
//}
