using ProberInterfaces;
using System;

namespace SoakingParameters
{
    [Serializable]
    public class ChuckAwayEventSoaking : SoakingParamBase
    {
        public ChuckAwayEventSoaking()
        {
        }
        public ChuckAwayEventSoaking(bool enable, int soakingpriority, EnumSoakingType soakingtype, int soakingtimeinseconds = 0, double zclearance = -2000, double pintolx = 0, double pintoly = 0, double pintolz = 0, bool postpinalign = true)   
        {
            this.Enable.Value = enable;
            this.SoakingPriority.Value = soakingpriority;
            this.EventSoakingType.Value = soakingtype;
            this.SoakingTimeInSeconds.Value = soakingtimeinseconds;
            this.ZClearance.Value = zclearance;
            this.Post_Pinalign.Value = postpinalign;
        }

        private Element<double> _ChuckAwayToleranceX = new Element<double>();
        public Element<double> ChuckAwayToleranceX
        {
            get { return _ChuckAwayToleranceX; }
            set
            {
                if (value != _ChuckAwayToleranceX)
                {
                    _ChuckAwayToleranceX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _ChuckAwayToleranceY = new Element<double>();
        public Element<double> ChuckAwayToleranceY
        {
            get { return _ChuckAwayToleranceY; }
            set
            {
                if (value != _ChuckAwayToleranceY)
                {
                    _ChuckAwayToleranceY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _ChuckAwayToleranceZ = new Element<double>();
        public Element<double> ChuckAwayToleranceZ
        {
            get { return _ChuckAwayToleranceZ; }
            set
            {
                if (value != _ChuckAwayToleranceX)
                {
                    _ChuckAwayToleranceX = value;
                    RaisePropertyChanged();
                }
            }
        }

        // Unit : Seconds
        private Element<int> _ChuckAwayElapsedTime = new Element<int>();
        public Element<int> ChuckAwayElapsedTime
        {
            get { return _ChuckAwayElapsedTime; }
            set
            {
                if (value != _ChuckAwayElapsedTime)
                {
                    _ChuckAwayElapsedTime = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable]
    public class TempDiffEventSoaking : SoakingParamBase
    {
        public TempDiffEventSoaking()
        {
        }

        public TempDiffEventSoaking(bool enable, int soakingpriority, EnumSoakingType soakingtype, int soakingtimeinseconds = 0, double zclearance = -2000, double pintolx = 0, double pintoly = 0, double pintolz = 0, bool postpinalign = true)
        {
            this.Enable.Value = enable;
            this.SoakingPriority.Value = soakingpriority;
            this.EventSoakingType.Value = soakingtype;
            this.SoakingTimeInSeconds.Value = soakingtimeinseconds;
            this.ZClearance.Value = zclearance;
            this.Post_Pinalign.Value = postpinalign;
        }

        // Unit : Seconds
        private Element<double> _OverTemperatureDifference = new Element<double>();
        public Element<double> OverTemperatureDifference
        {
            get { return _OverTemperatureDifference; }
            set
            {
                if (value != _OverTemperatureDifference)
                {
                    _OverTemperatureDifference = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable]
    public class ProbeCardChangeEventSoaking : SoakingParamBase
    {
        public ProbeCardChangeEventSoaking()
        {
        }

        public ProbeCardChangeEventSoaking(bool enable, int soakingpriority, EnumSoakingType soakingtype, int soakingtimeinseconds = 0, double zclearance = -2000, double pintolx = 0, double pintoly = 0, double pintolz = 0, bool postpinalign = true)
        {
            this.Enable.Value = enable;
            this.SoakingPriority.Value = soakingpriority;
            this.EventSoakingType.Value = soakingtype;
            this.SoakingTimeInSeconds.Value = soakingtimeinseconds;
            this.ZClearance.Value = zclearance;
            this.Post_Pinalign.Value = postpinalign;
        }
    }

    [Serializable]
    public class LotStartEventSoaking : SoakingParamBase
    {
        public LotStartEventSoaking()
        {
        }

        public LotStartEventSoaking(bool enable, int soakingpriority, EnumSoakingType soakingtype, int soakingtimeinseconds = 0, double zclearance = -2000, double pintolx = 0, double pintoly = 0, double pintolz = 0, bool postpinalign = true)
        {
            this.Enable.Value = enable;
            this.SoakingPriority.Value = soakingpriority;
            this.EventSoakingType.Value = soakingtype;
            this.SoakingTimeInSeconds.Value = soakingtimeinseconds;
            this.ZClearance.Value = zclearance;
            this.Post_Pinalign.Value = postpinalign;
        }
        private Element<int> _LotStartSkipTime = new Element<int>();
        public Element<int> LotStartSkipTime
        {
            get { return _LotStartSkipTime; }
            set
            {
                if (value != _LotStartSkipTime)
                {
                    _LotStartSkipTime = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
    [Serializable]
    public class DeviceChangeEventSoaking : SoakingParamBase
    {
        public DeviceChangeEventSoaking()
        {
        }
        public DeviceChangeEventSoaking(bool enable, int soakingpriority, EnumSoakingType soakingtype, int soakingtimeinseconds = 0, double zclearance = -2000, double pintolx = 0, double pintoly = 0, double pintolz = 0, bool postpinalign = true)
        {
            this.Enable.Value = enable;
            this.SoakingPriority.Value = soakingpriority;
            this.EventSoakingType.Value = soakingtype;
            this.SoakingTimeInSeconds.Value = soakingtimeinseconds;
            this.ZClearance.Value = zclearance;
            this.Post_Pinalign.Value = postpinalign;
        }
    }
    [Serializable]
    public class LotResumeEventSoaking:SoakingParamBase
    {
        public LotResumeEventSoaking() { }
        public LotResumeEventSoaking(bool enable, int soakingpriority, EnumSoakingType soakingtype, int soakingtimeinseconds = 0, double zclearance = -2000, double pintolx = 0, double pintoly = 0, double pintolz = 0, bool postpinalign = true)
        {
            this.Enable.Value = enable;
            this.SoakingPriority.Value = soakingpriority;
            this.EventSoakingType.Value = soakingtype;
            this.SoakingTimeInSeconds.Value = soakingtimeinseconds;
            this.ZClearance.Value = zclearance;
            this.Post_Pinalign.Value = postpinalign;
        }
    }
    [Serializable]
    public class EveryWaferEventSoaking : SoakingParamBase
    {
        public EveryWaferEventSoaking() {}

        public EveryWaferEventSoaking(bool enable, int soakingpriority, EnumSoakingType soakingtype, int soakingtimeinseconds = 0, double zclearance = -2000, double pintolx = 0, double pintoly = 0, double pintolz = 0, bool postpinalign = true)
        {
            this.Enable.Value = enable;
            this.SoakingPriority.Value = soakingpriority;
            this.EventSoakingType.Value = soakingtype;
            this.SoakingTimeInSeconds.Value = soakingtimeinseconds;
            this.ZClearance.Value = zclearance;
            this.Post_Pinalign.Value = postpinalign;
        }
    }
}
