//using CleaningScheduler.ScheduleState;
using LogModule;
using ProberErrorCode;
using ProberInterfaces.PolishWafer;
using System;
using System.ComponentModel;

namespace CleaningScheduler
{
    using PolishWaferParameters;
    using ProberInterfaces;
    using System.Runtime.CompilerServices;
    public class CleaningScheduleModule : ISchedulingModule, INotifyPropertyChanged, ICleaningScheduleModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property
        public bool Initialized { get; set; }
        #endregion

        #region //..ISchedulingModule Method

        public CleaningScheduleModule()
        {

        }

        #region //..Init & DeInit

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void DeInitModule()
        {
            return;
        }
        #endregion

        #endregion

        public bool _RequestLoadPolishWafer { get; set; }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }

        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        private PolishWaferParameter _PWParameter
        {
            get { return this.PolishWaferModule().PolishWaferParameter as PolishWaferParameter; }
        }
        public bool IsExecute()
        {
            bool retVal = false;

            try
            {
                if (this.PolishWaferModule().ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    retVal = false;
                    return retVal;
                }

                if (this.LotOPModule().ModuleStopFlag)
                {
                    retVal = false;
                    return retVal;
                }

                // LOT RUN
                if (this.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING &&
                    this.ProbingModule().ModuleState.GetState() != ModuleStateEnum.SUSPENDED &&
                    this.ProbingModule().ProbingStateEnum != EnumProbingState.ZDN &&
                    this.SequenceEngineManager().GetRunState())
                {
                    retVal = RequiredRunningStateTransition(true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }

        public bool RequiredRunningStateTransition(bool CanTriggered = false)
        {
            bool retval = false;

            try
            {
                if (this.PolishWaferModule().ModuleState.GetState() != ModuleStateEnum.DONE)
                {
                    //// Lot 시작 시, 초기화 되는 변수들
                    //int processedwafercount = this.LotOPModule().LotInfo.ProcessedWaferCnt;
                    //int processeddiecount = this.LotOPModule().LotInfo.ProcessedDieCnt;

                    // Card Changed 시, 초기화 되는 변수들
                    int ProcessedWaferCount = Convert.ToInt32(this.LotOPModule().SystemInfo.ProcessedWaferCountUntilBeforeCardChange);
                    int TouchDownCount = Convert.ToInt32(this.LotOPModule().SystemInfo.TouchDownCountUntilBeforeCardChange);

                    int MarkedProcessedWaferCount = Convert.ToInt32(this.LotOPModule().SystemInfo.MarkedWaferCountLastPolishWaferCleaning);
                    int MarkedTouchDownCount = Convert.ToInt32(this.LotOPModule().SystemInfo.MarkedTouchDownCountLastPolishWaferCleaning);

                    bool RequiredTrigger = false;

                    bool IsProbingComplete = false;
                    bool IsCanPWRetry = false; // pw 재 시도시에 

                    foreach (var intervalparam in _PWParameter.PolishWaferIntervalParameters)
                    {
                        int IntervalIndex = _PWParameter.PolishWaferIntervalParameters.IndexOf(intervalparam);

                        if (intervalparam.CleaningParameters.Count > 0)
                        {
                            // 만약 일반 웨이퍼가 존재하고, Processed가 아닌 경우
                            if (this.StageSupervisor().WaferObject.WaferStatus == EnumSubsStatus.EXIST &&
                                this.StageSupervisor().WaferObject.GetWaferType() == EnumWaferType.STANDARD)
                            {
                                if (this.StageSupervisor().WaferObject.GetState() == EnumWaferState.PROCESSED)
                                {
                                    IsProbingComplete = true;
                                }
                                else
                                {
                                    IsProbingComplete = false;
                                }
                            }
                            else if (this.StageSupervisor().WaferObject.WaferStatus == EnumSubsStatus.EXIST &&
                               this.StageSupervisor().WaferObject.GetWaferType() == EnumWaferType.POLISH)
                            {
                                IPolishWaferIntervalParameter curintervalparam = this.PolishWaferModule().GetCurrentIntervalParam();

                                if(curintervalparam != null)
                                {
                                    if ((this.StageSupervisor().WaferObject.GetState() == EnumWaferState.CLEANING || this.StageSupervisor().WaferObject.GetState() == EnumWaferState.UNPROCESSED)
                                    && this.StageSupervisor().WaferObject.GetState() != EnumWaferState.READY
                                    && intervalparam?.CleaningTriggerMode.Value == curintervalparam?.CleaningTriggerMode.Value)
                                    {
                                        IsCanPWRetry = true;

                                        this.PolishWaferModule().ProcessingInfo.SetCurrentPolishWaferCleaningRetry(true);

                                        LoggerManager.Debug($"[CleaningScheduleModule] RequiredRunningStateTransition() : Interval Triggered, Wafer Object State()= {this.StageSupervisor().WaferObject.GetState()},");
                                    }
                                }    
                            }

                            if (intervalparam.CleaningTriggerMode.Value == EnumCleaningTriggerMode.LOT_START)
                            {
                                if (this.PolishWaferModule().LotStartFlag == true || IsCanPWRetry)
                                {
                                    if (CanTriggered == true)
                                    {
                                        this.PolishWaferModule().ProcessingInfo.SetIntervalTrigger(intervalparam, true);
                                        //intervalparam.Triggered = true;
                                    }

                                    if (IsCanPWRetry == true)
                                    {
                                        // LOT_START 의 경우 동작 전  LotStartFlag 를 false 로 만들어 버리기 때문에 해당 과정에서 IsCanPWRetry 를 봐야함
                                        // 이미 pw 가 존재 하는 상태 
                                        // todo: 현재 존재 하는 pw 가 진짜 cleaning 을 해야 해서 load 한 웨이퍼 인가 확인 하는 로직 필요 함.
                                    }
                                    else
                                    {
                                        RequiredTrigger = true;
                                    }
                                }
                            }
                            else if (intervalparam.CleaningTriggerMode.Value == EnumCleaningTriggerMode.LOT_END)
                            {
                                if (this.PolishWaferModule().LotEndFlag == true || IsCanPWRetry)
                                {
                                    if (CanTriggered == true)
                                    {
                                        this.PolishWaferModule().ProcessingInfo.SetIntervalTrigger(intervalparam, true);
                                        //intervalparam.Triggered = true;
                                    }

                                    if (IsCanPWRetry == true)
                                    {
                                        // LOT_START 의 경우 동작 전  LotStartFlag 를 false 로 만들어 버리기 때문에 해당 과정에서 IsCanPWRetry 를 봐야함
                                        // 이미 pw 가 존재 하는 상태 
                                        // todo: 현재 존재 하는 pw 가 진짜 cleaning 을 해야 해서 load 한 웨이퍼 인가 확인 하는 로직 필요 함.
                                    }
                                    else
                                    {
                                        RequiredTrigger = true;
                                    }
                                }
                            }
                            else if (intervalparam.CleaningTriggerMode.Value == EnumCleaningTriggerMode.WAFER_INTERVAL)
                            {
                                if (IsProbingComplete == true || IsCanPWRetry == true)
                                {
                                    if (ProcessedWaferCount != 0 && intervalparam.IntervalCount.Value != 0)
                                    {
                                        //int NumberOfWafersProcessedSinceLastCleaning = ProcessedWaferCount - this.PolishWaferModule().RestoredMarkedWaferCountLastPolishWaferCleaning - intervalparam.TriggeredInterval;

                                        if (ProcessedWaferCount != 0)
                                        {
                                            if (ProcessedWaferCount % intervalparam.IntervalCount.Value == 0)
                                            {
                                                if (CanTriggered == true)
                                                {
                                                    //intervalparam.Triggered = true;
                                                    this.PolishWaferModule().ProcessingInfo.SetIntervalTrigger(intervalparam, true);
                                                }

                                                if (IsCanPWRetry == true)
                                                {
                                                    // 이미 pw 가 존재 하는 상태 
                                                    // todo: 현재 존재 하는 pw 가 진짜 cleaning 을 해야 해서 load 한 웨이퍼 인가 확인 하는 로직 필요 함.
                                                }
                                                else
                                                {
                                                    RequiredTrigger = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (intervalparam.CleaningTriggerMode.Value == EnumCleaningTriggerMode.TOUCHDOWN_COUNT)
                            {
                                if (IsProbingComplete == true)
                                {
                                    if (TouchDownCount != 0 && intervalparam.TouchdownCount.Value != 0)
                                    {
                                        // 마지막 클리닝 이후, 설정 된 TouchdownCount 이상의 Touch가 이루어졌을 때, 트리거 된다.

                                        //int NumberOfTouchDownSinceLastCleaning = TouchDownCount - this.PolishWaferModule().RestoredMarkedTouchDownCountLastPolishWaferCleaning - intervalparam.TriggeredTouchDown;

                                        int TotalProbingSeq = this.ProbingSequenceModule().ProbingSequenceCount;

                                        if (TouchDownCount != 0)
                                        {
                                            if ((TouchDownCount % intervalparam.TouchdownCount.Value - TotalProbingSeq) < 0)
                                            {
                                                if (CanTriggered == true)
                                                {
                                                    //intervalparam.Triggered = true;
                                                    this.PolishWaferModule().ProcessingInfo.SetIntervalTrigger(intervalparam, true);
                                                }

                                                if (IsCanPWRetry == true)
                                                {
                                                    // 이미 pw 가 존재 하는 상태 
                                                    // todo: 현재 존재 하는 pw 가 진짜 cleaning 을 해야 해서 load 한 웨이퍼 인가 확인 하는 로직 필요 함.
                                                }
                                                else
                                                {
                                                    RequiredTrigger = true;
                                                }
                                            }
                                        }

                                        //if (NumberOfTouchDownSinceLastCleaning != 0)
                                        //{
                                        //    if ((NumberOfTouchDownSinceLastCleaning) >= intervalparam.TouchdownCount.Value)
                                        //    {
                                        //        if (CanTriggered == true)
                                        //        {
                                        //            intervalparam.Triggered = true;
                                        //        }

                                        //        RequiredTrigger = true;
                                        //    }
                                        //}
                                    }
                                }
                            }

                            if (CanTriggered == true)
                            {
                                bool IsTriggered = this.PolishWaferModule().ProcessingInfo.GetIntervalTrigger(intervalparam.HashCode);

                                //if (intervalparam.Triggered == true)
                                if (IsTriggered == true)
                                {
                                    LoggerManager.Debug($"[CleaningScheduleModule] RequiredRunningStateTransition() : Interval Triggered, Index = {IntervalIndex}, TriggerMode = {intervalparam.CleaningTriggerMode.Value}, Cleaning Count = {intervalparam.CleaningParameters.Count}");

                                    retval = true;
                                }
                            }
                            else
                            {
                                if (RequiredTrigger == true)
                                {
                                    retval = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

}
