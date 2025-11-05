using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProberInterfaces.Soaking
{
    #region ClassforTempData
    /// <summary>
    /// Stauts soaking parameter의 일부 정보를 사용하기 위한 Class
    /// </summary>
    public class SoakingStepListInfo
    {
        public SoakingStepListInfo(int StepIndex, int SoakingTime, double OD_Val)
        {
            this.stepIndex = StepIndex;
            this.SoakingTimeSec = SoakingTime;
            this.OD_Value = OD_Val;
        }
        public int stepIndex { get; set; }
        public int SoakingTimeSec { get; set; }
        public double OD_Value { get; set; }
    }

    /// <summary>
    /// Stauts soaking parameter의 일부 정보를 사용하기 위한 Class
    /// </summary>
    public class ChillingTimeTableInfo
    {
        public ChillingTimeTableInfo(int StepIndex, int ChillingTimeSec, int SoakingTimeSec)
        {
            this.StepIdx = StepIndex;
            this.ChillingTimeSec = ChillingTimeSec;
            this.SoakingTimeSec = SoakingTimeSec;
        }

        public int StepIdx { get; set; }
        public int ChillingTimeSec { get; set; }
        public int SoakingTimeSec { get; set; }
    }

    public class StatusSoakingCommonParameterInfo
    {
        public StatusSoakingCommonParameterInfo()
        {

        }

        public bool use_polishwafer { get; set; } = false;
        public bool enalbe_edge_detection { get; set; } = false;
        private ObservableCollection<string> _polishwaferList = new ObservableCollection<string>();
        public ObservableCollection<string> polishwaferList
        {
            get
            {
                return _polishwaferList;
            }
            set
            {
                if (value != _polishwaferList)
                {
                    _polishwaferList = value;
                }
            }
        }
   
        private List<SoakingStepListInfo> _soakingStepList = new List<SoakingStepListInfo>();
        public List<SoakingStepListInfo> soakingStepList 
        {
            get 
            {
                return _soakingStepList;
            }
            set
            {
                if(value != _soakingStepList)
                {
                    _soakingStepList = value;
                }
            }
        }

        public double notExistWaferObj_ODVal { get; set; } = -1000;
        public bool enableWatingPinAlign { get; set; } = false;
        public int waitingPinAlignPeriodSec { get; set; } = 0;
        public int SoakingPriority { get; set; } = 0;        
        public PinAlignType PinAlignMode_AfterSoaking { get; set; } = PinAlignType.Full_PinAlign;
        public EventSoakType SoakingEvtType { get; set; } = EventSoakType.None; //none이면 event 아님
        public float Recovery_NotExistWafer_Ratio { get; set; } = 0f;

    }

    public class EventSoakingParameterInfo
    {
        public EventSoakingParameterInfo()
        {
               
        }

        public EventSoakingParameterInfo(bool IsEnalbeFlag, EventSoakType evtType, int priority, int soakingTime, double OD, PinAlignType LastPinAlignType)
        {
            this.IsEnable = IsEnalbeFlag;
            this.Soaking_Type_Enum = evtType;
            this.Soaking_Priority = priority;
            this.Soaking_TimeSec = soakingTime;
            this.OD_Value = OD;
            this.Soaking_PinAlignMode = LastPinAlignType;
        }        
        public EventSoakType Soaking_Type_Enum { get; set; }
        public bool IsEnable { get; set; }
        public int Soaking_Priority { get; set; }
        public int Soaking_TimeSec { get; set; }

        public double OD_Value { get; set; }

        public PinAlignType Soaking_PinAlignMode { get; set; }

    }

    public enum EventSoakType
    {
        EveryWaferSoak,
        BeforeZUpSoak,
        None,
    }

    public enum PinAlignType
    {
        Full_PinAlign,
        Sample_PinAlign,
        DoNot_PinAlign
    }

    public enum SoakingRatioPolicy_Index
    {
        Maintain_NotExistWafer,
        Maintain_UsePolicyWafer,
        Maintain_UseStandardWafer,
        Recovery_NotExistWafer,
        Maintain_Probing,
        SoakingRatioPolicy_End
    }

    #endregion

    /// <summary>
    /// StatusSoakingParameter의 정보를 사용하기 위한 I/F
    /// </summary>
    public interface IStatusSoakingParam
    {
        bool IsEnableStausSoaking(ref bool enableFlag);
        bool IsUseLastStatusSoakingState(ref bool useLastSoakingState);
        bool Get_PinAlignValidTimeSec(ref int pinAlignValidTimeSec);
        bool Get_PrepareStatusSoakingTimeSec(ref int PrepareSoakingTime, bool useTempDiff_SoakingTime = false);
        bool Get_UsePolishWaferFlag(SoakingStateEnum StatusSoaking, ref bool usePolishWaferFlag);
        bool Get_EnableEdgeDetectionFlag(SoakingStateEnum StatusSoaking, ref bool enalbeEdgeDetection);
        bool Get_SelectedPolishWaferName(SoakingStateEnum StatusSoaking, ref ObservableCollection<string> polishwaferList);
        bool Get_SoakingStepTableList(SoakingStateEnum StatusSoaking, ref List<SoakingStepListInfo> soakingStepList);
        bool Get_NotExistWaferObj_OD(SoakingStateEnum StatusSoaking, ref double Od_value);
        bool Get_EnableWaitingPinAlign(SoakingStateEnum StatusSoaking, ref bool usePinAlign);
        bool Get_WaitingPinAlignTime(SoakingStateEnum StatusSoaking, ref int pinAlignPeriod);
        bool Get_ChillingTimeTableInfo(ref List<ChillingTimeTableInfo> ChillingTimeTableList);
        bool Get_OperationForElapsedTimeSec(ref int OperationForElapsedTimeSec);
        bool Get_SoakingElapsedTimeSecForPWLoading(ref int PWOperationForElapsedTimeSec);
        //bool Get_StatusCommonOption(SoakingStateEnum StatusSoaking, ref bool usePolishWaferFlag, ref bool enalbeEdgeDetection, ref ObservableCollection<string> polishwaferList, ref List<SoakingStepListInfo> soakingStepList,
        //    ref double NotExistWafer_Od_value, ref bool EnableWatinPinAlign, ref int WaitingPinAlignPeriodSec);

        bool Get_StatusCommonOption(SoakingStateEnum StatusSoaking, ref StatusSoakingCommonParameterInfo SoakcommonParam);
        bool Get_AllowableTimeForLastPinAign(ref int AllowalbeTime);
        bool Get_ManualSoakingTime(ref int SoakingTime);
        bool Get_EventSoakingParam(EventSoakType soakingType, ref EventSoakingParameterInfo EventSoakingParam);
        bool Get_ShowStatusSoakingSettingPageToggleValue();
        void Set_ShowStatusSoakingSettingPageToggleValue(bool ToggleValue);
        float Get_ChillingTimeRatio(SoakingRatioPolicy_Index ChillingTimeRatio_index);
    }
}
