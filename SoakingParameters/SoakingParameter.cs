
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;
using AccountModule;

namespace SoakingParameters
{
    [Serializable]
    public class SoakingDeviceFile : IParam, INotifyPropertyChanged, IDeviceParameterizable, ICloneable, IParamNode
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (EventSoakingParams.Count(x => x.EventSoakingType.Value == EnumSoakingType.CHUCKAWAY_SOAK) == 0)
                {
                    ChuckAwayEventSoaking EventSoaking1 = new ChuckAwayEventSoaking(false, 3, EnumSoakingType.CHUCKAWAY_SOAK, 0, -2000, 10, 10, 10);
                    EventSoaking1.ChuckAwayToleranceX.Value = 100;
                    EventSoaking1.ChuckAwayToleranceY.Value = 100;
                    EventSoaking1.ChuckAwayToleranceZ.Value = 100;
                    EventSoaking1.ChuckAwayElapsedTime.Value = 60;
                    EventSoakingParams.Add(EventSoaking1);
                }

                if (EventSoakingParams.Count(x => x.EventSoakingType.Value == EnumSoakingType.TEMPDIFF_SOAK) == 0)
                {
                    TempDiffEventSoaking EventSoaking2 = new TempDiffEventSoaking(false, 1, EnumSoakingType.TEMPDIFF_SOAK, 0, -2000, 200, 200, 200);
                    EventSoaking2.OverTemperatureDifference.Value = 5.0;
                    EventSoakingParams.Add(EventSoaking2);
                }

                if (EventSoakingParams.Count(x => x.EventSoakingType.Value == EnumSoakingType.PROBECARDCHANGE_SOAK) == 0)
                {
                    ProbeCardChangeEventSoaking EventSoaking3 = new ProbeCardChangeEventSoaking(false, 1, EnumSoakingType.PROBECARDCHANGE_SOAK, 0, -2000, 200, 200, 200);
                    EventSoakingParams.Add(EventSoaking3);
                }

                if (EventSoakingParams.Count(x => x.EventSoakingType.Value == EnumSoakingType.LOTSTART_SOAK) == 0)
                {
                    LotStartEventSoaking EventSoaking4 = new LotStartEventSoaking(false, 1, EnumSoakingType.LOTSTART_SOAK, 0, -2000, 200, 200, 200);
                    if (EventSoaking4.LotStartSkipTime == null)
                    {
                        EventSoaking4.LotStartSkipTime = new Element<int>();
                    }
                    EventSoaking4.LotStartSkipTime.Value = 600;
                    EventSoakingParams.Add(EventSoaking4);
                }

                if (EventSoakingParams.Count(x => x.EventSoakingType.Value == EnumSoakingType.DEVICECHANGE_SOAK) == 0)
                {
                    DeviceChangeEventSoaking EventSoaking5 = new DeviceChangeEventSoaking(false, 1, EnumSoakingType.DEVICECHANGE_SOAK, 0, -2000, 200, 200, 200);
                    EventSoakingParams.Add(EventSoaking5);
                }

                if (EventSoakingParams.Count(x => x.EventSoakingType.Value == EnumSoakingType.LOTRESUME_SOAK) == 0)
                {
                    LotResumeEventSoaking EventSoaking6 = new LotResumeEventSoaking(false, 2, EnumSoakingType.LOTRESUME_SOAK, 0, -2000, 200, 200, 200);
                    EventSoakingParams.Add(EventSoaking6);
                }

                if (EventSoakingParams.Count(x => x.EventSoakingType.Value == EnumSoakingType.EVERYWAFER_SOAK) == 0)
                {
                    EveryWaferEventSoaking EventSoaking7 = new EveryWaferEventSoaking(false, 2, EnumSoakingType.EVERYWAFER_SOAK, 0, -2000, 200, 200, 200);
                    EventSoakingParams.Add(EventSoaking7);                    
                }

                if (AutoSoakingParam == null)
                {
                    AutoSoakParam Autosoaking = new AutoSoakParam(false, 0, EnumSoakingType.AUTO_SOAK, 0, -3000, 200, 200, 200, 30, 150, EnumLightType.COAXIAL, 70);
                    Autosoaking.ChuckAwayToleranceX.Value = 100;
                    Autosoaking.ChuckAwayToleranceY.Value = 100;
                    Autosoaking.ChuckAwayToleranceZ.Value = 100;
                    Autosoaking.ChuckAwayElapsedTime.Value = 60;
                    Autosoaking.Thickness_Tolerance.Value = 150;
                    Autosoaking.Pin_Align_Valid_Time.Value = 30;

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
        public void SetElementMetaData()
        {

            foreach (var param in EventSoakingParams)
            {
                // COMMON
                param.Enable.CategoryID = "10072001";
                param.Enable.ElementName = "Enable";

                param.SoakingPriority.CategoryID = "10072001";
                param.SoakingPriority.ElementName = "Priority";

                param.EventSoakingType.CategoryID = "10072001";
                param.EventSoakingType.ElementName = "Soaking Type";
                param.EventSoakingType.WriteMaskingLevel = AccountManager.SuperUserLevel;

                param.SoakingTimeInSeconds.CategoryID = "10072001";
                param.SoakingTimeInSeconds.ElementName = "Soaking Time";

                param.ZClearance.CategoryID = "10072001";
                param.ZClearance.ElementName = "Z Clearance";
                
                //param.PinToleranceX.CategoryID = "10072001";
                //param.PinToleranceX.ElementName = "Pin Tolerance X";

                //param.PinToleranceY.CategoryID = "10072001";
                //param.PinToleranceY.ElementName = "Pin Tolerance Y";

                //param.PinToleranceZ.CategoryID = "10072001";
                //param.PinToleranceZ.ElementName = "Pin Tolerance Z";

                switch (param.EventSoakingType.Value)
                {
                    case EnumSoakingType.UNDEFINED:
                        break;
                        //case EnumSoakingType.PREHAT_SOAK1:
                        //    break;
                        //case EnumSoakingType.PREHAT_SOAK2:
                        //    break;
                        //case EnumSoakingType.FIRSTWAFER_SOAK:
                        //    break;
                        
                        //case EnumSoakingType.LOTRESUME_SOAK:
                        //break;
                    //case EnumSoakingType.PINPOSCHANGED_SOAK:
                    //    break;
                    //case EnumSoakingType.POLISHWAFER_SOAK:
                    //    break;
                    case EnumSoakingType.CHUCKAWAY_SOAK:

                        ChuckAwayEventSoaking chuckaway_soak = param as ChuckAwayEventSoaking;

                        if (chuckaway_soak == null)
                        {
                            // ERROR
                            LoggerManager.Error("Event Soaking type is unknown.");
                        }

                        chuckaway_soak.ChuckAwayElapsedTime.CategoryID = "10072001";
                        chuckaway_soak.ChuckAwayElapsedTime.ElementName = "Chuck Away Elapsed Time";

                        chuckaway_soak.ChuckAwayToleranceX.CategoryID = "10072001";
                        chuckaway_soak.ChuckAwayToleranceX.ElementName = "Chuck Away Tolerance X";

                        chuckaway_soak.ChuckAwayToleranceY.CategoryID = "10072001";
                        chuckaway_soak.ChuckAwayToleranceY.ElementName = "Chuck Away Tolerance Y";

                        chuckaway_soak.ChuckAwayToleranceZ.CategoryID = "10072001";
                        chuckaway_soak.ChuckAwayToleranceZ.ElementName = "Chuck Away Tolerance Z";

                        break;
                    case EnumSoakingType.TEMPDIFF_SOAK:

                        TempDiffEventSoaking tempdiff_soak = param as TempDiffEventSoaking;

                        if (tempdiff_soak == null)
                        {
                            // ERROR
                            LoggerManager.Error("Event Soaking type is unknown.");
                        }

                        tempdiff_soak.OverTemperatureDifference.CategoryID = "10072001";
                        tempdiff_soak.OverTemperatureDifference.ElementName = "Temperature Diffrence Trigger Degree";

                        break;
                    case EnumSoakingType.PROBECARDCHANGE_SOAK:
                        break;
                    case EnumSoakingType.LOTSTART_SOAK:

                        LotStartEventSoaking lotstart = param as LotStartEventSoaking;

                        if (lotstart == null)
                        {
                            // ERROR
                            LoggerManager.Error("Event Soaking type is unknown.");
                        }
                        lotstart.LotStartSkipTime.CategoryID = "10072001";
                        lotstart.LotStartSkipTime.ElementName = "LotStart Soaking Skip Time(seconds)";
                        lotstart.LotStartSkipTime.Description = "Prevent excessive soaking for continous LOTs. Elapsed time is longer than specified, LOT start event soaking is re-enabled.";
                        lotstart.LotStartSkipTime.LowerLimit = 0;
                        lotstart.LotStartSkipTime.UpperLimit = 43200;
                        break;
                    case EnumSoakingType.LOTRESUME_SOAK:

                        param.ResumeSoakingSkipTime.CategoryID = "10072001";
                        param.ResumeSoakingSkipTime.ElementName = "Resume Soaking Skip Time(seconds)";
                        param.ResumeSoakingSkipTime.Description = "Prevent excessive soaking for continous LOTs. Elapsed time is longer than specified, resume soaking is re-enabled.";
                        param.ResumeSoakingSkipTime.LowerLimit = 0;
                        param.ResumeSoakingSkipTime.UpperLimit = 43200;
                        break;
                    case EnumSoakingType.EVERYWAFER_SOAK:
                        param.ResumeSoakingSkipTime.CategoryID = "10072001";
                        param.ResumeSoakingSkipTime.ElementName = "Resume Soaking Skip Time(seconds)";
                        param.ResumeSoakingSkipTime.Description = "";
                        param.ResumeSoakingSkipTime.LowerLimit = 0;
                        param.ResumeSoakingSkipTime.UpperLimit = 43200;
                        break;
                    default:
                        break;
                }
            }




            AutoSoakingParam.Enable.CategoryID = "10072002";
            AutoSoakingParam.Enable.ElementName = "Enable";

            AutoSoakingParam.EventSoakingType.CategoryID = "10072002";
            AutoSoakingParam.EventSoakingType.ElementName = "Soaking Type";
            AutoSoakingParam.EventSoakingType.WriteMaskingLevel = AccountManager.SuperUserLevel;

            AutoSoakingParam.ZClearance.CategoryID = "10072002";
            AutoSoakingParam.ZClearance.ElementName = "Z Clearance";

            AutoSoakingParam.SoakingTimeInSeconds.CategoryID = "10072002";
            AutoSoakingParam.SoakingTimeInSeconds.ElementName = "Soaking Time";

            AutoSoakingParam.ChuckAwayElapsedTime.CategoryID = "10072002";
            AutoSoakingParam.ChuckAwayElapsedTime.ElementName = "Chuck Away Elapsed Time";

            AutoSoakingParam.ChuckAwayToleranceX.CategoryID = "10072002";
            AutoSoakingParam.ChuckAwayToleranceX.ElementName = "Chuck Away Tolerance X";

            AutoSoakingParam.ChuckAwayToleranceY.CategoryID = "10072002";
            AutoSoakingParam.ChuckAwayToleranceY.ElementName = "Chuck Away Tolerance Y";

            AutoSoakingParam.ChuckAwayToleranceZ.CategoryID = "10072002";
            AutoSoakingParam.ChuckAwayToleranceZ.ElementName = "Chuck Away Tolerance Z";

            AutoSoakingParam.Pin_Align_Valid_Time.CategoryID = "10072002";
            AutoSoakingParam.Pin_Align_Valid_Time.Unit = "min";
            AutoSoakingParam.Pin_Align_Valid_Time.ElementName = "Pin Align Valid Time";
           
            AutoSoakingParam.Thickness_Tolerance.CategoryID = "10072002";
            AutoSoakingParam.Thickness_Tolerance.Unit = "um";
            AutoSoakingParam.Thickness_Tolerance.ElementName = "Thickness Tolerance";
            AutoSoakingParam.Thickness_Tolerance.LowerLimit = 0;


            AutoSoakingParam.LightType.CategoryID = "10072002";
            AutoSoakingParam.LightType.ElementName = "Light Type";

            AutoSoakingParam.LightValue.CategoryID = "10072002";
            AutoSoakingParam.LightValue.ElementName = "Light Value (Click Me!)";
            AutoSoakingParam.LightValue.Description = "Value Range : 0 ~ 255\n You can check the lights here.\n [MENU]-[Recipe]-[Configuration]-[Wafer]-[Basic Information]-[Creat wafer map]";
            AutoSoakingParam.LightValue.UpperLimit = 255;
            AutoSoakingParam.LightValue.LowerLimit = 0;


            //AutoSoakingType.ElementName = "Auto Soaking Type";
            //AutoSoakingType.ElementAdmin = "Alvin";
            //AutoSoakingType.CategoryID = "10072002";
            //AutoSoakingType.Description = "Unknown";

            //AutoSokaingZClearanceNotExistWaferOnChuck.ElementName = "Z Clearance Value (Wafer On Chuck)";
            //AutoSokaingZClearanceNotExistWaferOnChuck.ElementAdmin = "Alvin";
            //AutoSokaingZClearanceNotExistWaferOnChuck.CategoryID = "10072002";
            //AutoSokaingZClearanceNotExistWaferOnChuck.Description = "Unknown";

            //AutoSokaingZClearanceBeforeWaferAlign.ElementName = "Z Clearance Value (Before Wafer Align)";
            //AutoSokaingZClearanceBeforeWaferAlign.ElementAdmin = "Alvin";
            //AutoSokaingZClearanceBeforeWaferAlign.CategoryID = "10072002";
            //AutoSokaingZClearanceBeforeWaferAlign.Description = "Unknown";

            //AutoSokaingZClearanceAfterWaferAlign.ElementName = "Z Clearance Value (After Wafer Align)";
            //AutoSokaingZClearanceAfterWaferAlign.ElementAdmin = "Alvin";
            //AutoSokaingZClearanceAfterWaferAlign.CategoryID = "10072002";
            //AutoSokaingZClearanceAfterWaferAlign.Description = "Unknown";

            //AutoSokaingZClearanceLimit.ElementName = "Z Clearance Limit Value";
            //AutoSokaingZClearanceLimit.ElementAdmin = "Alvin";
            //AutoSokaingZClearanceLimit.CategoryID = "10072002";
            //AutoSokaingZClearanceLimit.Description = "Unknown";

            //AutoSoakingTempUpperValue.ElementName = "Temperature Value (Upper)";
            //AutoSoakingTempUpperValue.ElementAdmin = "Alvin";
            //AutoSoakingTempUpperValue.CategoryID = "10072002";
            //AutoSoakingTempUpperValue.Description = "Unknown";

            //AutoSoakingTempLowerValue.ElementName = "Temperature Value (Lower)";
            //AutoSoakingTempLowerValue.ElementAdmin = "Alvin";
            //AutoSoakingTempLowerValue.CategoryID = "10072002";
            //AutoSoakingTempLowerValue.Description = "Unknown";

            //ChuckAwayElapsedTime.ElementName = "Chuck Away Elapsed Time";
            //ChuckAwayElapsedTime.ElementAdmin = "Alvin";
            //ChuckAwayElapsedTime.CategoryID = "10072002";
            //ChuckAwayElapsedTime.Description = "Unknown";

            //ChuckAwayToleranceX.ElementName = "Chuck Away Tolerance X";
            //ChuckAwayToleranceX.ElementAdmin = "Alvin";
            //ChuckAwayToleranceX.CategoryID = "10072002";
            //ChuckAwayToleranceX.Description = "Unknown";

            //ChuckAwayToleranceY.ElementName = "Chuck Away Tolerance Y";
            //ChuckAwayToleranceY.ElementAdmin = "Alvin";
            //ChuckAwayToleranceY.CategoryID = "10072002";
            //ChuckAwayToleranceY.Description = "Unknown";

            //ChuckAwayToleranceZ.ElementName = "Chuck Away Tolerance Z";
            //ChuckAwayToleranceZ.ElementAdmin = "Alvin";
            //ChuckAwayToleranceZ.CategoryID = "10072002";
            //ChuckAwayToleranceZ.Description = "Unknown";

            //TemperatureDiffEnable.ElementName = "Temperature Diffrence Trigger Enable";
            //TemperatureDiffEnable.ElementAdmin = "Alvin";
            //TemperatureDiffEnable.CategoryID = "10072002";
            //TemperatureDiffEnable.Description = "Unknown";

            //TemperatureDiffZClearance.ElementName = "Temperature Diffrence Trigger Z Clearance";
            //TemperatureDiffZClearance.ElementAdmin = "Alvin";
            //TemperatureDiffZClearance.CategoryID = "10072002";
            //TemperatureDiffZClearance.Description = "Unknown";

            //TemperatureDiffDegree.ElementName = "Temperature Diffrence Trigger Degree";
            //TemperatureDiffDegree.ElementAdmin = "Alvin";
            //TemperatureDiffDegree.CategoryID = "10072002";
            //TemperatureDiffDegree.Description = "Unknown";


        }
        public string FilePath { get; } = "";

        public string FileName { get; } = "SoakingDeviceFile.Json";

        //private Element<ObservableCollection<Guid>> _WhiteList = new Element<ObservableCollection<Guid>>();
        //public Element<ObservableCollection<Guid>> WhiteList
        //{
        //    get { return _WhiteList; }
        //    set
        //    {
        //        if (value != _WhiteList)
        //        {
        //            _WhiteList = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //ObservableCollection<AxisObject>
        //    class SoakingTypes : , IParamNode
        //{
        //    ObservableCollection<Element<EnumSoakingType>> // list of element

        //        Element<ObservableCollection<EnumSoakingType>> //element of list type
        //}

        //private Element<ObservableCollection<EnumSoakingType>> _EventSoakingList = new Element<ObservableCollection<EnumSoakingType>>();
        //public Element<ObservableCollection<EnumSoakingType>> EventSoakingList
        //{
        //    get { return _EventSoakingList; }
        //    set
        //    {
        //        if (value != _EventSoakingList)
        //        {
        //            _EventSoakingList = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        private ObservableCollection<SoakingParamBase> _EventSoakingParams = new ObservableCollection<SoakingParamBase>();
        public ObservableCollection<SoakingParamBase> EventSoakingParams
        {
            get { return _EventSoakingParams; }
            set
            {
                if (value != _EventSoakingParams)
                {
                    _EventSoakingParams = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AutoSoakParam _AutoSoakingParam = new AutoSoakParam(false, 0, EnumSoakingType.AUTO_SOAK, 0, -3000, 200, 200, 200);
        public AutoSoakParam AutoSoakingParam
        {
            get { return _AutoSoakingParam; }
            set
            {
                if (value != _AutoSoakingParam)
                {
                    _AutoSoakingParam = value;
                    RaisePropertyChanged();
                }
            }
        }
      
        private ObservableCollection<SoakingTimeTable> _LotResumeTimeTable = new ObservableCollection<SoakingTimeTable>();
        public ObservableCollection<SoakingTimeTable> LotResumeTimeTable
        {
            get { return _LotResumeTimeTable; }
            set
            {
                if (value != _LotResumeTimeTable)
                {
                    _LotResumeTimeTable = value;
                    RaisePropertyChanged();
                }
            }
        }


        //private SoakingParamBase _AutoSoaking = new SoakingParamBase();
        //[SharePropPath]
        //public SoakingParamBase AutoSoaking
        //{
        //    get { return _AutoSoaking; }
        //    set
        //    {
        //        if (value != _AutoSoaking)
        //        {
        //            _AutoSoaking = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private SoakingParamBase _PreHeatSoaking1 = new SoakingParamBase();
        //[SharePropPath]
        //public SoakingParamBase PreHeatSoaking1
        //{
        //    get { return _PreHeatSoaking1; }
        //    set
        //    {
        //        if (value != _PreHeatSoaking1)
        //        {
        //            _PreHeatSoaking1 = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private SoakingParamBase _PreHeatSoaking2 = new SoakingParamBase();
        //[SharePropPath]
        //public SoakingParamBase PreHeatSoaking2
        //{
        //    get { return _PreHeatSoaking2; }
        //    set
        //    {
        //        if (value != _PreHeatSoaking2)
        //        {
        //            _PreHeatSoaking2 = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private SoakingParamBase _FirstWaferSoaking = new SoakingParamBase();
        //[SharePropPath]
        //public SoakingParamBase FirstWaferSoaking
        //{
        //    get { return _FirstWaferSoaking; }
        //    set
        //    {
        //        if (value != _FirstWaferSoaking)
        //        {
        //            _FirstWaferSoaking = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private SoakingParamBase _EveryWaferSoaking = new SoakingParamBase();
        //[SharePropPath]
        //public SoakingParamBase EveryWaferSoaking
        //{
        //    get { return _EveryWaferSoaking; }
        //    set
        //    {
        //        if (value != _EveryWaferSoaking)
        //        {
        //            _EveryWaferSoaking = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private SoakingParamBase _LotResumeSoaking = new SoakingParamBase();
        //[SharePropPath]
        //public SoakingParamBase LotResumeSoaking
        //{
        //    get { return _LotResumeSoaking; }
        //    set
        //    {
        //        if (value != _LotResumeSoaking)
        //        {
        //            _LotResumeSoaking = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //private SoakingParamBase _PolishWaferSoaking = new SoakingParamBase();
        //[SharePropPath]
        //public SoakingParamBase PolishWaferSoaking
        //{
        //    get { return _PolishWaferSoaking; }
        //    set
        //    {
        //        if (value != _PolishWaferSoaking)
        //        {
        //            _PolishWaferSoaking = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        // 50도라면 500으로 변환.
        private Element<int> _HotTempBasicPoint = new Element<int>();
        public Element<int> HotTempBasicPoint
        {
            get { return _HotTempBasicPoint; }
            set
            {
                if (value != _HotTempBasicPoint)
                {
                    _HotTempBasicPoint = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 50도라면 500으로 변환.
        private Element<int> _ColdTempBasicPoint = new Element<int>();
        public Element<int> ColdTempBasicPoint
        {
            get { return _ColdTempBasicPoint; }
            set
            {
                if (value != _ColdTempBasicPoint)
                {
                    _ColdTempBasicPoint = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<EnumAutoSoakingType> _AutoSoakingType = new Element<EnumAutoSoakingType>();
        public Element<EnumAutoSoakingType> AutoSoakingType
        {
            get { return _AutoSoakingType; }
            set
            {
                if (value != _AutoSoakingType)
                {
                    _AutoSoakingType = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _AutoSokaingZClearanceNotExistWaferOnChuck = new Element<double>();
        public Element<double> AutoSokaingZClearanceNotExistWaferOnChuck
        {
            get { return _AutoSokaingZClearanceNotExistWaferOnChuck; }
            set
            {
                if (value != _AutoSokaingZClearanceNotExistWaferOnChuck)
                {
                    _AutoSokaingZClearanceNotExistWaferOnChuck = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _AutoSokaingZClearanceBeforeWaferAlign = new Element<double>();
        public Element<double> AutoSokaingZClearanceBeforeWaferAlign
        {
            get { return _AutoSokaingZClearanceBeforeWaferAlign; }
            set
            {
                if (value != _AutoSokaingZClearanceBeforeWaferAlign)
                {
                    _AutoSokaingZClearanceBeforeWaferAlign = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _AutoSokaingZClearanceAfterWaferAlign = new Element<double>();
        public Element<double> AutoSokaingZClearanceAfterWaferAlign
        {
            get { return _AutoSokaingZClearanceAfterWaferAlign; }
            set
            {
                if (value != _AutoSokaingZClearanceAfterWaferAlign)
                {
                    _AutoSokaingZClearanceAfterWaferAlign = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _AutoSokaingZClearanceLimit = new Element<double>();
        public Element<double> AutoSokaingZClearanceLimit
        {
            get { return _AutoSokaingZClearanceLimit; }
            set
            {
                if (value != _AutoSokaingZClearanceLimit)
                {
                    _AutoSokaingZClearanceLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _AutoSoakingTempUpperValue = new Element<double>();
        public Element<double> AutoSoakingTempUpperValue
        {
            get { return _AutoSoakingTempUpperValue; }
            set
            {
                if (value != _AutoSoakingTempUpperValue)
                {
                    _AutoSoakingTempUpperValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _AutoSoakingTempLowerValue = new Element<double>();
        public Element<double> AutoSoakingTempLowerValue
        {
            get { return _AutoSoakingTempLowerValue; }
            set
            {
                if (value != _AutoSoakingTempLowerValue)
                {
                    _AutoSoakingTempLowerValue = value;
                    RaisePropertyChanged();
                }
            }
        }


        //// Unit : Seconds
        //private Element<int> _ChuckAwayElapsedTime = new Element<int>();
        //public Element<int> ChuckAwayElapsedTime
        //{
        //    get { return _ChuckAwayElapsedTime; }
        //    set
        //    {
        //        if (value != _ChuckAwayElapsedTime)
        //        {
        //            _ChuckAwayElapsedTime = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Element<double> _ChuckAwayToleranceX = new Element<double>();
        //public Element<double> ChuckAwayToleranceX
        //{
        //    get { return _ChuckAwayToleranceX; }
        //    set
        //    {
        //        if (value != _ChuckAwayToleranceX)
        //        {
        //            _ChuckAwayToleranceX = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Element<double> _ChuckAwayToleranceY = new Element<double>();
        //public Element<double> ChuckAwayToleranceY
        //{
        //    get { return _ChuckAwayToleranceY; }
        //    set
        //    {
        //        if (value != _ChuckAwayToleranceY)
        //        {
        //            _ChuckAwayToleranceY = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Element<double> _ChuckAwayToleranceZ = new Element<double>();
        //public Element<double> ChuckAwayToleranceZ
        //{
        //    get { return _ChuckAwayToleranceZ; }
        //    set
        //    {
        //        if (value != _ChuckAwayToleranceX)
        //        {
        //            _ChuckAwayToleranceX = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Element<bool> _TemperatureDiffEnable = new Element<bool>();
        //public Element<bool> TemperatureDiffEnable
        //{
        //    get { return _TemperatureDiffEnable; }
        //    set
        //    {
        //        if (value != _TemperatureDiffEnable)
        //        {
        //            _TemperatureDiffEnable = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Element<double> _TemperatureDiffZClearance = new Element<double>();
        //public Element<double> TemperatureDiffZClearance
        //{
        //    get { return _TemperatureDiffZClearance; }
        //    set
        //    {
        //        if (value != _TemperatureDiffZClearance)
        //        {
        //            _TemperatureDiffZClearance = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Element<double> _TemperatureDiffDegree = new Element<double>();
        //public Element<double> TemperatureDiffDegree
        //{
        //    get { return _TemperatureDiffDegree; }
        //    set
        //    {
        //        if (value != _TemperatureDiffDegree)
        //        {
        //            _TemperatureDiffDegree = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}



        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                //WhiteList.Value = new ObservableCollection<Guid>();

                //WhiteList.Value.Add(new Guid());

                //WhiteList.Value.Add(new Guid("a47da402-ed65-d3be-d8a4-bc62b2404498"));
                //WhiteList.Value.Add(new Guid("5A23188A-0D7C-EEA3-DFF0-54EBD97DAFDE"));
                //WhiteList.Value.Add(new Guid("91D47E22-66F7-C358-736C-5BCF7C3E287E"));
                //WhiteList.Value.Add(new Guid("B1B4E681-ADEA-18EF-0886-EDF8D3510861"));
                //WhiteList.Value.Add(new Guid("c66b8e15-7274-4a14-ad6f-381908019e14"));
                //WhiteList.Value.Add(new Guid("B9FDC1FA-F80D-8709-DE59-786DB49194E0"));
                //WhiteList.Value.Add(new Guid("55FF5FBD-95DB-1006-F8A0-3512C831B2F5"));
                //WhiteList.Value.Add(new Guid("D39D0AEA-1F74-75B5-97FE-8640869AC7CF"));
                //WhiteList.Value.Add(new Guid("4F42078C-05FE-B4B7-70ED-0602C9DF269B"));

                SoakingTimeTable timetable1 = new SoakingTimeTable();
                timetable1.PauseTime.Value = 10;
                timetable1.SoakingTime.Value = 20;

                SoakingTimeTable timetable2 = new SoakingTimeTable();
                timetable2.PauseTime.Value = 60;
                timetable2.SoakingTime.Value = 30;

                SoakingTimeTable timetable3 = new SoakingTimeTable();
                timetable3.PauseTime.Value = 120;
                timetable3.SoakingTime.Value = 50;

                SoakingTimeTable timetable4 = new SoakingTimeTable();
                timetable4.PauseTime.Value = 180;
                timetable4.SoakingTime.Value = 60;

                LotResumeTimeTable.Add(timetable1);
                LotResumeTimeTable.Add(timetable2);
                LotResumeTimeTable.Add(timetable3);
                LotResumeTimeTable.Add(timetable4);

                #region  AutoSoaking
                AutoSoakingParam = new AutoSoakParam(false, 0, EnumSoakingType.AUTO_SOAK, 0, -3000, 200, 200, 200, 30, 150);
                AutoSoakingParam.Enable.Value = false;
                AutoSoakingParam.EventSoakingType.Value = EnumSoakingType.AUTO_SOAK;
                AutoSoakingParam.SoakingTimeInSeconds.Value = 0;
                AutoSoakingParam.ZClearance.Value = -3000;
                AutoSoakingParam.ChuckAwayToleranceX.Value = 100;
                AutoSoakingParam.ChuckAwayToleranceY.Value = 100;
                AutoSoakingParam.ChuckAwayToleranceZ.Value = 100;
                AutoSoakingParam.ChuckAwayElapsedTime.Value = 60;
                AutoSoakingParam.Pin_Align_Valid_Time.Value = 30;
                AutoSoakingParam.Thickness_Tolerance.Value = 150;
                AutoSoakingParam.LightType.Value = EnumLightType.COAXIAL;
                AutoSoakingParam.LightValue.Value = 70;
                AutoSoakingParam.IdleSoak_AlignTrigger.Value = true;


                //old
                AutoSoakingType.Value = EnumAutoSoakingType.DISABLE;
                AutoSokaingZClearanceNotExistWaferOnChuck.Value = -20000;
                AutoSokaingZClearanceBeforeWaferAlign.Value = -20000;
                AutoSokaingZClearanceAfterWaferAlign.Value = -20000;
                AutoSokaingZClearanceLimit.Value = -20000;
                AutoSoakingTempUpperValue.Value = 30;
                AutoSoakingTempLowerValue.Value = 30;

                // AutoSoaking.Enable.Value = false;
                //AutoSoaking.SoakingPriority.Value = 0;
                //AutoSoaking.EventSoakingType.Value = EnumSoakingType.AUTO_SOAK;

                //AutoSoaking.PinTolerance.X.Value = 10;
                //AutoSoaking.PinTolerance.Y.Value = 10;
                //AutoSoaking.PinTolerance.Z.Value = 10;
                //AutoSoaking.SoakingTimeInSeconds.Value = 0;
                //AutoSoaking.ZClearance.Value = -1000;

                //AutoSoaking.Script.Add(autosoakingscript);
                #endregion

                // (1) 
                ChuckAwayEventSoaking EventSoaking1 = new ChuckAwayEventSoaking(false, 4, EnumSoakingType.CHUCKAWAY_SOAK, 0, -2000, 200, 200, 200);

                EventSoaking1.ChuckAwayToleranceX.Value = 100;
                EventSoaking1.ChuckAwayToleranceY.Value = 100;
                EventSoaking1.ChuckAwayToleranceZ.Value = 100;
                EventSoaking1.ChuckAwayElapsedTime.Value = 60;
                EventSoakingParams.Add(EventSoaking1);

                // (2)

                TempDiffEventSoaking EventSoaking2 = new TempDiffEventSoaking(false, 3, EnumSoakingType.TEMPDIFF_SOAK, 0, -2000, 200, 200, 200);

                EventSoaking2.OverTemperatureDifference.Value = 50;
                EventSoakingParams.Add(EventSoaking2);

                // (3) 

                ProbeCardChangeEventSoaking EventSoaking3 = new ProbeCardChangeEventSoaking(false, 1, EnumSoakingType.PROBECARDCHANGE_SOAK, 0, -2000, 200, 200, 200);

                EventSoakingParams.Add(EventSoaking3);

                // (4)
                LotStartEventSoaking EventSoaking4 = new LotStartEventSoaking(false, 1, EnumSoakingType.LOTSTART_SOAK, 0, -2000, 200, 200, 200);
                if(EventSoaking4.LotStartSkipTime == null)
                {
                    EventSoaking4.LotStartSkipTime = new Element<int>();
                }
                EventSoaking4.LotStartSkipTime.Value = 600;
                EventSoakingParams.Add(EventSoaking4);

                DeviceChangeEventSoaking EventSoaking5 = new DeviceChangeEventSoaking(false, 1, EnumSoakingType.DEVICECHANGE_SOAK, 0, -2000, 200, 200, 200);
                EventSoakingParams.Add(EventSoaking5);

                LotResumeEventSoaking EventSoaking6 = new LotResumeEventSoaking(false, 2, EnumSoakingType.LOTRESUME_SOAK, 0, -2000, 200, 200, 200);
                EventSoakingParams.Add(EventSoaking6);

                EveryWaferEventSoaking EventSoaking7 = new EveryWaferEventSoaking(false, 2, EnumSoakingType.EVERYWAFER_SOAK, 0, -2000, 200, 200, 200);
                EventSoakingParams.Add(EventSoaking7);

                HotTempBasicPoint.Value = 500; // 50도라면 500으로 변환.
                ColdTempBasicPoint.Value = 0;

                //EventSoakingList.Value = new ObservableCollection<EnumSoakingType>();

                //EventSoakingList.Value.Add(EnumSoakingType.PREHAT_SOAKING);
                //EventSoakingList.Value.Add(EnumSoakingType.FIRSTWAFER_SOAKING);
                //EventSoakingList.Value.Add(EnumSoakingType.EVERYWAFER_SOAKING);
                //EventSoakingList.Value.Add(EnumSoakingType.LOTRESUME_SOAKING);
                //EventSoakingList.Value.Add(EnumSoakingType.POLISHWAFER_SOAKING);

                //ChuckAwayToleranceX.Value = 100;
                //ChuckAwayToleranceY.Value = 100;
                //ChuckAwayToleranceZ.Value = 100;

                //// 60 Sec.
                //ChuckAwayElapsedTime.Value = 60;

                //TemperatureDiffEnable.Value = false;
                //TemperatureDiffZClearance.Value = -20000;
                //TemperatureDiffDegree.Value = 50; // 5도

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public static Type[] GetImplTypes()
        {
            try
            {
                return new Type[]
                {
                    //add abstract class impl
                };
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object Clone()
        {
            SoakingDeviceFile devfile = new SoakingDeviceFile();

            try
            {
                //devfile.AutoSoaking = this.AutoSoaking;
                devfile.ColdTempBasicPoint = this.ColdTempBasicPoint;
                devfile.LotResumeTimeTable = this.LotResumeTimeTable;
                devfile.HotTempBasicPoint = this.HotTempBasicPoint;

                //devfile.EventSoakingList = this.EventSoakingList;

                //devfile.EveryWaferSoaking = this.EveryWaferSoaking;
                //devfile.FirstWaferSoaking = this.FirstWaferSoaking;
                //devfile.LotResumeSoaking = this.LotResumeSoaking;
                //devfile.PolishWaferSoaking = this.PolishWaferSoaking;
                //devfile.PreHeatSoaking1 = this.PreHeatSoaking1;
                //devfile.PreHeatSoaking2 = this.PreHeatSoaking2;

                devfile.EventSoakingParams = this.EventSoakingParams;

                devfile.AutoSoakingParam = this.AutoSoakingParam;
                //devfile.WhiteList = this.WhiteList;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return devfile;
        }

        public void Copy(SoakingDeviceFile devfile)
        {
            try
            {
                //this.AutoSoaking = devfile.AutoSoaking;
                this.ColdTempBasicPoint = devfile.ColdTempBasicPoint;
                this.LotResumeTimeTable = devfile.LotResumeTimeTable;
                this.HotTempBasicPoint = devfile.HotTempBasicPoint;

                //this.EventSoakingList = devfile.EventSoakingList;
                //this.EveryWaferSoaking = devfile.EveryWaferSoaking;
                //this.FirstWaferSoaking = devfile.FirstWaferSoaking;
                //this.LotResumeSoaking = devfile.LotResumeSoaking;
                //this.PolishWaferSoaking = devfile.PolishWaferSoaking;
                //this.PreHeatSoaking1 = devfile.PreHeatSoaking1;
                //this.PreHeatSoaking2 = devfile.PreHeatSoaking2;

                devfile.EventSoakingParams = this.EventSoakingParams;
                devfile.AutoSoakingParam = this.AutoSoakingParam;
                //this.WhiteList = devfile.WhiteList;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
    //[Serializable]
    //public class PinPosChangedSoakingRecipe : SoakingParamBase
    //{
    //    private ObservableCollection<SoakBaseInfo> _PinPosChangedScript = new ObservableCollection<SoakBaseInfo>();
    //    public ObservableCollection<SoakBaseInfo> PinPosChangedScript
    //    {
    //        get { return _PinPosChangedScript; }
    //        set
    //        {
    //            if (value != _PinPosChangedScript)
    //            {
    //                _PinPosChangedScript = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }
    //}

    //[Serializable]
    //public class LotResumeSoakingRecipe : SoakingParamBase
    //{
    //    public LotResumeSoakingRecipe() { }
    //}
    //[Serializable]
    //public class EveryWaferSoakingRecipe : SoakingParamBase
    //{
    //    public EveryWaferSoakingRecipe() { }

    //}
    //[Serializable]
    //public class FirstWaferSoakingRecipe : SoakingParamBase
    //{
    //    public FirstWaferSoakingRecipe() { }

    //}
    //[Serializable]
    //public class AutoSoakingRecipe : SoakingParamBase
    //{
    //    public AutoSoakingRecipe() { }
    //}

    //[Serializable]
    //public class PreHeatingRecipe : SoakingParamBase
    //{
    //    public PreHeatingRecipe() { }
    //}
    //[Serializable]
    //public class PolishWaferSoakingRecipe : SoakingParamBase
    //{
    //    public PolishWaferSoakingRecipe() { }
    //}


    [Serializable]
    public class SoakingParamBase : INotifyPropertyChanged, IParamNode
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

        public SoakingParamBase()
        {

        }
        public SoakingParamBase(bool enable, int soakingpriority, EnumSoakingType soakingtype, int soakingtimeinseconds = 0, double zclearance = -2000, double pintolx = 0, double pintoly = 0, double pintolz = 0, bool post_pinalign = true)
        {
            this.Enable.Value = enable;
            this.SoakingPriority.Value = soakingpriority;
            this.EventSoakingType.Value = soakingtype;
            this.SoakingTimeInSeconds.Value = soakingtimeinseconds;
            this.ZClearance.Value = zclearance;
            this.Post_Pinalign.Value = post_pinalign;

        }

        //[NonSerialized]
        //private bool _Triggered = false;
        //[XmlIgnore, JsonIgnore, ParamIgnore]
        //public bool Triggered
        //{
        //    get { return _Triggered; }
        //    set
        //    {
        //        if (value != _Triggered)
        //        {
        //            _Triggered = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Element<bool> _Enable = new Element<bool>();
        public Element<bool> Enable
        {
            get { return _Enable; }
            set
            {
                if (value != _Enable)
                {
                    _Enable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<int> _SoakingPriority = new Element<int>();
        public Element<int> SoakingPriority
        {
            get { return _SoakingPriority; }
            set
            {
                if (value != _SoakingPriority)
                {
                    _SoakingPriority = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<EnumSoakingType> _EventSoakingType = new Element<EnumSoakingType>();
        [NoShowingParamEdit, EditIgnore]
        public Element<EnumSoakingType> EventSoakingType
        {
            get { return _EventSoakingType; }
            set
            {
                if (value != _EventSoakingType)
                {
                    _EventSoakingType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _SoakingTimeInSeconds = new Element<int>();
        public Element<int> SoakingTimeInSeconds
        {
            get { return _SoakingTimeInSeconds; }
            set
            {
                if (value != _SoakingTimeInSeconds)
                {
                    _SoakingTimeInSeconds = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _ZClearance = new Element<double>();
        public Element<double> ZClearance
        {
            get { return _ZClearance; }
            set
            {
                if (value != _ZClearance)
                {
                    _ZClearance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ResumeSoakingSkipTimet = new Element<int>();
        public Element<int> ResumeSoakingSkipTime
        {
            get { return _ResumeSoakingSkipTimet; }
            set
            {
                if (value != _ResumeSoakingSkipTimet)
                {
                    _ResumeSoakingSkipTimet = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<bool> _Post_Pinalign = new Element<bool>() { Value = true };
        public Element<bool> Post_Pinalign
        {
            get { return _Post_Pinalign; }
            set
            {
                if (value != _Post_Pinalign)
                {
                    _Post_Pinalign = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Element<double> _PinToleranceX = new Element<double>();
        //public Element<double> PinToleranceX
        //{
        //    get { return _PinToleranceX; }
        //    set
        //    {
        //        if (value != _PinToleranceX)
        //        {
        //            _PinToleranceX = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Element<double> _PinToleranceY = new Element<double>();
        //public Element<double> PinToleranceY
        //{
        //    get { return _PinToleranceY; }
        //    set
        //    {
        //        if (value != _PinToleranceY)
        //        {
        //            _PinToleranceY = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Element<double> _PinToleranceZ = new Element<double>();
        //public Element<double> PinToleranceZ
        //{
        //    get { return _PinToleranceZ; }
        //    set
        //    {
        //        if (value != _PinToleranceZ)
        //        {
        //            _PinToleranceZ = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        //private PinCoordinate _PinTolerance = new PinCoordinate();
        //public PinCoordinate PinTolerance
        //{
        //    get { return _PinTolerance; }
        //    set
        //    {
        //        if (value != _PinTolerance)
        //        {
        //            _PinTolerance = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
    }

    [Serializable]
    public class SoakingTimeTable : INotifyPropertyChanged, IParamNode
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private Element<int> _PauseTime = new Element<int>();
        public Element<int> PauseTime
        {
            get { return _PauseTime; }
            set
            {
                if (value != _PauseTime)
                {
                    _PauseTime = value;
                    NotifyPropertyChanged("PauseTime");
                }
            }
        }
        private Element<int> _SoakingTime = new Element<int>();
        public Element<int> SoakingTime
        {
            get { return _SoakingTime; }
            set
            {
                if (value != _SoakingTime)
                {
                    _SoakingTime = value;
                    NotifyPropertyChanged("SoakingTime");
                }
            }
        }

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
    }
}
