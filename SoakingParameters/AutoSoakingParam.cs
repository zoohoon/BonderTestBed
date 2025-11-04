using ProberInterfaces;
using System;

namespace SoakingParameters
{
    [Serializable]
    public class AutoSoakParam : SoakingParamBase
    {
        public AutoSoakParam() 
        {
            Post_Pinalign.Value = false;
        }
        public AutoSoakParam(bool enable, int soakingpriority, EnumSoakingType soakingtype, int soakingtimeinseconds = 0, double zclearance = -2000
                                        , double pintolx = 0, double pintoly = 0, double pintolz = 0, int pin_align_valid_time = 30,double Thickness_Tolerance = 150
                                        , EnumLightType light_type = EnumLightType.COAXIAL, ushort light_value = 70, bool idlesoak_aligntrigger = true)
        {
            this.Enable.Value = enable;
            this.SoakingPriority.Value = soakingpriority;
            this.EventSoakingType.Value = soakingtype;
            this.SoakingTimeInSeconds.Value = soakingtimeinseconds;
            this.ZClearance.Value = zclearance;
            this.Pin_Align_Valid_Time.Value = pin_align_valid_time;
            this.Thickness_Tolerance.Value = Thickness_Tolerance;
            this.LightType.Value = light_type;
            this.LightValue.Value = light_value;
            this.IdleSoak_AlignTrigger.Value = idlesoak_aligntrigger;
            this.ChuckFocusingFlatnessThd.Value = 95.0;
            Post_Pinalign.Value = false;
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
        private Element<int> _Pin_Align_Valid_Time = new Element<int> { Value = 30 };
        public Element<int> Pin_Align_Valid_Time
        {
            get { return _Pin_Align_Valid_Time; }
            set
            {
                if (value != _Pin_Align_Valid_Time)
                {
                    _Pin_Align_Valid_Time = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<EnumLightType> _LightType = new Element<EnumLightType> { Value = EnumLightType.COAXIAL };
        public Element<EnumLightType> LightType
        {
            get { return _LightType; }
            set
            {
                if (value != _LightType)
                {
                    _LightType = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<ushort> _LightValue = new Element<ushort> { Value = 70 };
        public Element<ushort> LightValue
        {
            get { return _LightValue; }
            set
            {
                if (value != _LightValue)
                {
                    _LightValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _Thickness_Tolerance = new Element<double> { Value = 150 };
        public Element<double> Thickness_Tolerance
        {
            get { return _Thickness_Tolerance; }
            set
            {
                if (value != _Thickness_Tolerance)
                {
                    _Thickness_Tolerance = value;
                    RaisePropertyChanged();
                }
            }
        }

        //auto soaking할 때 pin align이랑 focusing 할지 말지
        private Element<bool> _IdleSoak_AlignTrigger = new Element<bool> { Value = true };
        public Element<bool> IdleSoak_AlignTrigger
        {
            get { return _IdleSoak_AlignTrigger; }
            set
            {
                if (value != _IdleSoak_AlignTrigger)
                {
                    _IdleSoak_AlignTrigger = value;
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
        private Element<double> _ChuckFocusingFlatnessThd = new Element<double>();
        public Element<double> ChuckFocusingFlatnessThd
        {
            get { return _ChuckFocusingFlatnessThd; }
            set
            {
                if (value != _ChuckFocusingFlatnessThd)
                {
                    _ChuckFocusingFlatnessThd = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

}
