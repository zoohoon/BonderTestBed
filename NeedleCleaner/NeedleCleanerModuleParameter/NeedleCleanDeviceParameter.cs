using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NeedleCleanerModuleParameter
{

    public enum NC_SCRUB_Direction
    {
        NO_SCRUB = 0,
        TOP = 1,
        TOP_RIGHT = 2,
        RIGHT = 3,
        BOTTOM_RIGHT = 4,
        BOTTOM = 5,
        BOTTOM_LEFT = 6,
        LEFT = 7,
        TOP_LEFT = 8,
        SQUARE = 9,
        OCTAGONAL = 10
    }
    public enum NC_FocusingInterval
    {
        EVERY_LOT_START = 0,
        EVERY_WAFER_START = 1,
        EVERY_CLEANING = 2
    }

    //public enum NC_CleaningInterval
    //{
    //    EVERY_LOT_START = 0,
    //    EVERY_WAFER_START = 1,
    //    DIE_INTERVAL = 2
    //}
    public enum NC_CleaningType
    {
        SINGLEDIR = 0,
        USER_DEFINE = 1
    }
    public enum NC_CleaningDirection
    {
        HOLD = 0,
        TOP = 1,
        TOP_RIGHT = 2,
        RIGHT = 3,
        BOTTOM_RIGHT = 4,
        BOTTOM = 5,
        BOTTOM_LEFT = 6,
        LEFT = 7,
        TOP_LEFT = 8
    }

    public enum NC_ContactMethod
    {
        UP_DOWN = 0,
        SCRUB_LINE = 1,
        SCRUB_OCTA = 2
    }

    public enum NC_OverdriveType
    {
        PROBING_OD = 1,
        NC_OD = 2
    }

    [Serializable]
    public class NeedleCleanDeviceParameter : IDeviceParameterizable, INotifyPropertyChanged, IParamNode
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public NeedleCleanDeviceParameter()
        {

        }

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; } = "NeedleCleanModule";
        public string FileName { get; } = "NeedleCleanDevParameter.json";
        private string _Genealogy;
        public string Genealogy
        {
            get { return _Genealogy; }
            set { _Genealogy = value; }
        }

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


        //private ModuleDllInfo _SubrutineDllInfo;

        //public ModuleDllInfo SubrutineDllInfo
        //{
        //    get { return _SubrutineDllInfo; }
        //    set { _SubrutineDllInfo = value; }
        //}


        private List<NCSheetDevParam> _SheetDevs;
        public List<NCSheetDevParam> SheetDevs
        {
            get { return _SheetDevs; }
            set
            {
                if (value != _SheetDevs)
                {
                    _SheetDevs = value;
                    NotifyPropertyChanged("SheetDefs");
                }
            }
        }

        private Element<bool> _PinAlignBeforeCleaning = new Element<bool>();
        public Element<bool> PinAlignBeforeCleaning
        {
            get { return _PinAlignBeforeCleaning; }
            set
            {
                if (value != _PinAlignBeforeCleaning)
                {
                    _PinAlignBeforeCleaning = value;
                    NotifyPropertyChanged("PinAlignBeforeCleaning");
                }
            }
        }

        private Element<bool> _PinAlignAfterCleaning = new Element<bool>();
        public Element<bool> PinAlignAfterCleaning
        {
            get { return _PinAlignAfterCleaning; }
            set
            {
                if (value != _PinAlignBeforeCleaning)
                {
                    _PinAlignAfterCleaning = value;
                    NotifyPropertyChanged("PinAlignAfterCleaning");
                }
            }
        }

        private int _SelectedIndex;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {
                if (value != _SelectedIndex)
                {
                    _SelectedIndex = value;
                    NotifyPropertyChanged("SelectedIndex");
                }
            }
        }


        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SetEmulParam();
                //SubrutineDllInfo = new ModuleDllInfo(
                //   @"NeeldleCleanerSubRutineStandardModule.dll",
                //   @"NeeldleCleanerSubRutineStandard", 1000, true);


                //Enabled.Value = true;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return retVal;
        }
        public void SetElementMetaData()
        {

        }
        public void CopyTo(NeedleCleanDeviceParameter target)
        {
            try
            {
                target = new NeedleCleanDeviceParameter();
                target.SheetDevs = new List<NCSheetDevParam>();
                foreach (var item in this.SheetDevs)
                {
                    NCSheetDevParam dev = new NCSheetDevParam();
                    item.CopyTo(dev);
                    target.SheetDevs.Add(dev);
                }
                target.PinAlignAfterCleaning.Value = this.PinAlignAfterCleaning.Value;
                target.PinAlignBeforeCleaning.Value = this.PinAlignBeforeCleaning.Value;
                target.SelectedIndex = this.SelectedIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Test Code
                //Enabled.Value = true;
                //FocusInterval.Value = NC_FocusingInterval.EVERY_LOT_START;
                //CleaningInterval.Value = NC_CleaningInterval.EVERY_LOT_START;
                //DieInterval.Value = 100;
                SheetDevs = new List<NCSheetDevParam>();
                SheetDevs.Add(new NCSheetDevParam());
                SheetDevs.Add(new NCSheetDevParam());
                SheetDevs.Add(new NCSheetDevParam());
                PinAlignAfterCleaning.Value = false;
                PinAlignBeforeCleaning.Value = false;
                
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            //SetDefaultParam();
            return retVal;
        }
    }

    [Serializable]
    public class NCSheetDevParam : INotifyPropertyChanged, IParamNode
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName]string info = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public void CopyTo(NCSheetDevParam target)
        {
            try
            {
                target.CleaningAssistanceInterval = new List<Element<int>>();
                foreach (var item in this.CleaningAssistanceInterval)
                {
                    Element<int> interval = new Element<int>();
                    item.CopyTo(interval);
                    target.CleaningAssistanceInterval.Add(interval);
                }
                target.CleaningCount.Value = this.CleaningCount.Value;
                target.CleaningDieInterval.Value = this.CleaningDieInterval.Value;
                target.CleaningDirection.Value = this.CleaningDirection.Value;
                target.CleaningDistance.Value = this.CleaningDistance.Value;
                target.CleaningType.Value = this.CleaningType.Value;
                target.CleaningWaferInterval.Value = this.CleaningWaferInterval.Value;
                target.Clearance.Value = this.Clearance.Value;
                target.EnableAssistanceInterval.Value = this.EnableAssistanceInterval.Value;
                target.EnableCleaningDieInterval.Value = this.EnableCleaningDieInterval.Value;
                target.EnableCleaningLotInterval.Value = this.EnableCleaningLotInterval.Value;
                target.EnableCleaningWaferInterval.Value = this.EnableCleaningWaferInterval.Value;
                target.Enabled.Value = this.Enabled.Value;
                target.FocusInterval.Value = this.FocusInterval.Value;
                target.ScrubDirection.Value = this.ScrubDirection.Value;
                target.ScrubLength.Value = this.ScrubLength.Value;
                target.UserDefinedSeq = new List<Element<NCCoordinate>>();
                target.Overdrive.Value = this.Overdrive.Value;
                target.OverdriveLimit.Value = this.OverdriveLimit.Value;

                target.EnableContactCount.Value = this.EnableContactCount.Value;
                target.ContactLimit.Value = this.ContactLimit.Value;
                target.ContactCount.Value = this.ContactCount.Value;

                target.EnableCycleCount.Value = this.EnableCycleCount.Value;
                target.CycleLimit.Value = this.CycleLimit.Value;
                target.CycleCount.Value = this.CycleCount.Value;

                target.EnableNcSoftTouch.Value = this.EnableNcSoftTouch.Value;
                target.InclineOrigin.Value = this.InclineOrigin.Value;
                //target.InclinedAccel.Value = this.InclinedAccel.Value;
                //target.InclinedSpeed.Value = this.InclinedSpeed.Value;            
                target.NCOverdrive.Value = this.NCOverdrive.Value;
                target.RelativeOdRatio.Value = this.RelativeOdRatio.Value;

                foreach (var item in this.UserDefinedSeq)
                {
                    Element<NCCoordinate> seq = new Element<NCCoordinate>();
                    item.CopyTo(seq);
                    target.UserDefinedSeq.Add(seq);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private Element<bool> _Enabled = new Element<bool>();
        public Element<bool> Enabled
        {
            get { return _Enabled; }
            set
            {
                if (value != _Enabled)
                {
                    _Enabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<NC_FocusingInterval> _FocusInterval = new Element<NC_FocusingInterval>();
        public Element<NC_FocusingInterval> FocusInterval
        {
            get { return _FocusInterval; }
            set
            {
                if (value != _FocusInterval)
                {
                    _FocusInterval = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<bool> _EnableCleaningLotInterval = new Element<bool>();
        public Element<bool> EnableCleaningLotInterval
        {
            get { return _EnableCleaningLotInterval; }
            set
            {
                if (value != _EnableCleaningLotInterval)
                {
                    _EnableCleaningLotInterval = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<bool> _EnableCleaningWaferInterval = new Element<bool>();
        public Element<bool> EnableCleaningWaferInterval
        {
            get { return _EnableCleaningWaferInterval; }
            set
            {
                if (value != _EnableCleaningWaferInterval)
                {
                    _EnableCleaningWaferInterval = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<bool> _EnableCleaningDieInterval = new Element<bool>();
        public Element<bool> EnableCleaningDieInterval
        {
            get { return _EnableCleaningDieInterval; }
            set
            {
                if (value != _EnableCleaningDieInterval)
                {
                    _EnableCleaningDieInterval = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<bool> _EnableAssistanceInterval = new Element<bool>();
        public Element<bool> EnableAssistanceInterval
        {
            get { return _EnableAssistanceInterval; }
            set
            {
                if (value != _EnableAssistanceInterval)
                {
                    _EnableAssistanceInterval = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<long> _CleaningDieInterval = new Element<long>();
        public Element<long> CleaningDieInterval
        {
            get { return _CleaningDieInterval; }
            set
            {
                if (value != _CleaningDieInterval)
                {
                    _CleaningDieInterval = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<long> _CleaningWaferInterval = new Element<long>();
        public Element<long> CleaningWaferInterval
        {
            get { return _CleaningWaferInterval; }
            set
            {
                if (value != _CleaningWaferInterval)
                {
                    _CleaningWaferInterval = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private List<Element<int>> _CleaningAssistanceInterval = new List<Element<int>>();
        public List<Element<int>> CleaningAssistanceInterval
        {
            get { return _CleaningAssistanceInterval; }
            set
            {
                if (value != _CleaningAssistanceInterval)
                {
                    _CleaningAssistanceInterval = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<NC_CleaningDirection> _CleaningDirection = new Element<NC_CleaningDirection>();
        public Element<NC_CleaningDirection> CleaningDirection
        {
            get { return _CleaningDirection; }
            set
            {
                if (value != _CleaningDirection)
                {
                    _CleaningDirection = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private Element<NC_CleaningType> _CleaningType = new Element<NC_CleaningType>();
        public Element<NC_CleaningType> CleaningType
        {
            get { return _CleaningType; }
            set
            {
                if (value != _CleaningType)
                {
                    _CleaningType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<NC_ContactMethod> _ContactMethod = new Element<NC_ContactMethod>();
        public Element<NC_ContactMethod> ContactMethod
        {
            get { return _ContactMethod; }
            set
            {
                if (value != _ContactMethod)
                {
                    _ContactMethod = value;
                    NotifyPropertyChanged("ContactMethod");
                }
            }
        }

        private Element<NC_OverdriveType> _OverdriveType = new Element<NC_OverdriveType>();
        public Element<NC_OverdriveType> OverdriveType
        {
            get { return _OverdriveType; }
            set
            {
                if (value != _OverdriveType)
                {
                    _OverdriveType = value;
                    NotifyPropertyChanged("OverdriveType");
                }
            }
        }


        private Element<long> _CleaningDistance = new Element<long>();
        public Element<long> CleaningDistance
        {
            get { return _CleaningDistance; }
            set
            {
                if (value != _CleaningDistance)
                {
                    _CleaningDistance = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private Element<int> _CleaningCount = new Element<int>();
        public Element<int> CleaningCount
        {
            get { return _CleaningCount; }
            set
            {
                if (value != _CleaningCount)
                {
                    _CleaningCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<double> _Overdrive = new Element<double>();
        public Element<double> Overdrive
        {
            get { return _Overdrive; }
            set
            {
                if (value != _Overdrive)
                {
                    _Overdrive = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<double> _Clearance = new Element<double>();
        public Element<double> Clearance
        {
            get { return _Clearance; }
            set
            {
                if (value != _Clearance)
                {
                    _Clearance = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private Element<double> _ScrubLength = new Element<double>();
        public Element<double> ScrubLength
        {
            get { return _ScrubLength; }
            set
            {
                if (value != _ScrubLength)
                {
                    _ScrubLength = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private Element<NC_SCRUB_Direction> _ScrubDirection = new Element<NC_SCRUB_Direction>();
        public Element<NC_SCRUB_Direction> ScrubDirection
        {
            get { return _ScrubDirection; }
            set
            {
                if (value != _ScrubDirection)
                {
                    _ScrubDirection = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private List<Element<NCCoordinate>> _UserDefinedSeq = new List<Element<NCCoordinate>>();
        public List<Element<NCCoordinate>> UserDefinedSeq
        {
            get { return _UserDefinedSeq; }
            set
            {
                if (value != _UserDefinedSeq)
                {
                    _UserDefinedSeq = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // 소프트 터치 시 속도가 꺾이는 높이 설정. All contact 높이로부터 상대 옵셋값. 기본값은 -50 정도
        private Element<double> _InclineOrigin = new Element<double>();
        public Element<double> InclineOrigin
        {
            get { return _InclineOrigin; }
            set
            {
                if (value != _InclineOrigin)
                {
                    _InclineOrigin = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // 소프트 터치시 사용될 감속
        private Element<double> _InclineAccel = new Element<double>();
        public Element<double> InclineAccel
        {
            get { return _InclineAccel; }
            set
            {
                if (value != _InclineAccel)
                {
                    _InclineAccel = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // 소프트 터치시 사용될 속도
        private Element<double> _InclineSpeed = new Element<double>();
        public Element<double> InclineSpeed
        {
            get { return _InclineSpeed; }
            set
            {
                if (value != _InclineSpeed)
                {
                    _InclineSpeed = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // 소프트 터치시 사용될 속도
        private Element<bool> _EnableNcSoftTouch = new Element<bool>();
        public Element<bool> EnableNcSoftTouch
        {
            get { return _EnableNcSoftTouch; }
            set
            {
                if (value != _EnableNcSoftTouch)
                {
                    _EnableNcSoftTouch = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // 프로빙 OD로부터의 상대 높이 사용 모드
        private Element<double> _RelativeOdRatio = new Element<double>();
        public Element<double> RelativeOdRatio
        {
            get { return _RelativeOdRatio; }
            set
            {
                if (value != _RelativeOdRatio)
                {
                    _RelativeOdRatio = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // 클리닝 OD리미트, 절대 이 값 이상 OD가 적용될 수 없다.
        private Element<double> _OverdriveLimit = new Element<double>();
        public Element<double> OverdriveLimit
        {
            get { return _OverdriveLimit; }
            set
            {
                if (value != _OverdriveLimit)
                {
                    _OverdriveLimit = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<bool> _EnableContactCount = new Element<bool>();
        public Element<bool> EnableContactCount
        {
            get { return _EnableContactCount; }
            set
            {
                if (value != _EnableContactCount)
                {
                    _EnableContactCount = value;
                    NotifyPropertyChanged("EnableContactCount");
                }
            }
        }




        private Element<long> _ContactLimit = new Element<long>();
        public Element<long> ContactLimit
        {
            get { return _ContactLimit; }
            set
            {
                if (value != _ContactLimit)
                {
                    _ContactLimit = value;
                    NotifyPropertyChanged("ContactLimit");
                }
            }
        }

        private Element<long> _ContactCount = new Element<long>();
        public Element<long> ContactCount
        {
            get { return _ContactCount; }
            set
            {
                if (value != _ContactCount)
                {
                    _ContactCount = value;
                    NotifyPropertyChanged("ContactCount");
                }
            }
        }

        private Element<bool> _EnableCycleCount = new Element<bool>();
        public Element<bool> EnableCycleCount
        {
            get { return _EnableCycleCount; }
            set
            {
                if (value != _EnableCycleCount)
                {
                    _EnableCycleCount = value;
                    NotifyPropertyChanged("EnableCycleCount");
                }
            }
        }



        private Element<long> _CycleLimit = new Element<long>();
        public Element<long> CycleLimit
        {
            get { return _CycleLimit; }
            set
            {
                if (value != _CycleLimit)
                {
                    _CycleLimit = value;
                    NotifyPropertyChanged("CycleLimit");
                }
            }
        }

        private Element<long> _CycleCount = new Element<long>();
        public Element<long> CycleCount
        {
            get { return _CycleCount; }
            set
            {
                if (value != _CycleCount)
                {
                    _CycleCount = value;
                    NotifyPropertyChanged("CycleCount");
                }
            }
        }

        // 느려질 가속도 (니들 클리닝 용 기본 가속도를 초과할 수 없다)
        //private Element<long> _InclinedAccel = new Element<long>();
        //public Element<long> InclinedAccel
        //{
        //    get { return _InclinedAccel; }
        //    set
        //    {
        //        if (value != _InclinedAccel)
        //        {
        //            _InclinedAccel = value;
        //            NotifyPropertyChanged("InclinedAccel");
        //        }
        //    }
        //}

        //// 느려질 속도 (니들 클리닝 용 기본 속도를 초과할 수 없다)
        //private Element<long> _InclinedSpeed = new Element<long>();
        //public Element<long> InclinedSpeed
        //{
        //    get { return _InclinedSpeed; }
        //    set
        //    {
        //        if (value != _InclinedSpeed)
        //        {
        //            _InclinedSpeed = value;
        //            NotifyPropertyChanged("InclinedSpeed");
        //        }
        //    }
        //}

        private Element<double> _NCOverdrive = new Element<double>();
        public Element<double> NCOverdrive
        {
            get { return _NCOverdrive; }
            set
            {
                if (value != _NCOverdrive)
                {
                    _NCOverdrive = value;
                    NotifyPropertyChanged("NCOverdrive");
                }
            }
        }



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

        public NCSheetDevParam()
        {
            try
            {
                Enabled.Value = true;
                FocusInterval.Value = NC_FocusingInterval.EVERY_LOT_START;
                EnableCleaningLotInterval.Value = true;
                EnableCleaningWaferInterval.Value = true;
                EnableCleaningDieInterval.Value = false;
                EnableAssistanceInterval.Value = false;
                CleaningDieInterval.Value = 20;
                CleaningWaferInterval.Value = 3;
                CleaningCount.Value = 10;
                CleaningDistance.Value = 10000;
                CleaningType.Value = NC_CleaningType.SINGLEDIR;
                CleaningDirection.Value = NC_CleaningDirection.BOTTOM_RIGHT;
                ContactMethod.Value = NC_ContactMethod.UP_DOWN;
                EnableContactCount.Value = false;
                ContactLimit.Value = 999999;
                EnableCycleCount.Value = false;
                CycleLimit.Value = 999999;
                EnableNcSoftTouch.Value = false;
                InclineOrigin.Value = -50;
                InclineAccel.Value = 2500000;
                InclineSpeed.Value = 25000;
                OverdriveType.Value = NC_OverdriveType.NC_OD;
                RelativeOdRatio.Value = 65;
                NCOverdrive.Value = 20;
                OverdriveLimit.Value = 150;
                Clearance.Value = 300;
                ScrubDirection.Value = NC_SCRUB_Direction.BOTTOM_RIGHT;
                ScrubLength.Value = 20;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
