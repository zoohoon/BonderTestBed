using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace Soaking
{
    using ProberInterfaces;
    using LogModule;
    using ProberInterfaces.Param;
    using ProberErrorCode;
    using SoakingParameters;
    using ProberInterfaces.Soaking;

    /// <summary>
    /// Soaking의 Chilling Time을 관리하는 Class.
    /// 참고: SoakingModule을 내부 멤버로 참조하고 있음.
    /// </summary>
    public class SoakingChillingTimeMng : IDisposable, ISoakingChillingTimeMng
    {

        #region ==> variable

        private System.Threading.Timer ChillingMonitoringThreadRestartTimer;
        bool InitFlag { get; set; } = false;
        private Thread ChillingTimeMonitoringThread;
        private bool monitoringThreadStopFlag { get; set; } = false;
        private long ChillingTimeMillSec { get; set; } = 0;
        private SoakingModule SoakingModule;
        private StatusSoakingConfig StatusSoakingConfigParameter;
        private object chillingTime_lockObject = new object();
        private readonly int checkIntervalTimeMil = 1000;
        private bool CardDocking = false;

        private DateTime CardDockinCheckLastTime;
        private readonly int CardDockingCheckInterval = 15;

        private DateTime DebugViewLogRefreshFlagTm;
        private readonly int DebugViewLogRefreshFlagInterval = 30;
        private readonly string FilePathForDebug = "C:\\ProberSystem\\StatusSoaking.fdbg";
        public bool showDebugViewData { get; set; } = false;
        #endregion

        public void Dispose()
        {
            Exit_ChillingTimeMng();
            if (null != ChillingMonitoringThreadRestartTimer)
                ChillingMonitoringThreadRestartTimer.Dispose();
        }

        private StatusSoakingConfig GetStatusSoakingConfig()
        {
            return SoakingModule.StatusSoakingDeviceFileObj?.StatusSoakingConfigParameter;
        }

        /// <summary>
        /// Chilling time 관리 Thread가 어떠한 이유로 종료되어 재 구동 처리를 하기 위한 Timer call back함수
        /// </summary>
        /// <param name="obj"></param>
        private void ChillingTimeMonitoringThreadRestart(object obj)
        {
            ChillingMonitoringThreadRestartTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            LoggerManager.SoakingLog($"--- ChillingTime Monitoring Thread Restart.---");
            ChillingTimeMonitoringThreadStart();
        }

        /// <summary>
        /// CardDocking여부를 체크한다.(마지막 상태 체크 후 5초 이상된 경우)
        /// </summary>
        /// <param name="FirstCall"></param>
        private void CardDockingStateCheck(bool FirstCall)
        {
            if (FirstCall)
            {
                CardDockinCheckLastTime = DateTime.Now;
                CardDocking = SoakingModule.CardChangeModule().IsExistCard();
                LoggerManager.SoakingLog($"CardDocking : {CardDocking.ToString()}");
            }
            else
            {
                TimeSpan ElapsedTime = DateTime.Now - CardDockinCheckLastTime;
                if (ElapsedTime.TotalSeconds > CardDockingCheckInterval)
                {
                    bool CardDockingTemp = SoakingModule.CardChangeModule().IsExistCard();
                    if (CardDocking != CardDockingTemp)
                    {
                        CardDocking = CardDockingTemp;
                        LoggerManager.SoakingLog($"CardDocking : {CardDocking.ToString()}");
                    }

                    CardDockinCheckLastTime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// 특정 파일을 존재여부를 확인하여 debugging 기능을 사용할 수 있도록 확인
        /// </summary>
        /// <param name="FirstCall"></param>
        private void CheckDebugFunc(bool FirstCall)
        {
            if (FirstCall)
            {
                DebugViewLogRefreshFlagTm = DateTime.Now;
                showDebugViewData = File.Exists(FilePathForDebug);
            }
            else
            {
                TimeSpan ElapsedTime = DateTime.Now - DebugViewLogRefreshFlagTm;
                if (ElapsedTime.TotalSeconds > DebugViewLogRefreshFlagInterval)
                {
                    bool ExistDebugFilePath = File.Exists(FilePathForDebug);
                    if (showDebugViewData != ExistDebugFilePath)
                    {
                        showDebugViewData = ExistDebugFilePath;
                        LoggerManager.SoakingLog($"Use DebugView Func : {showDebugViewData.ToString()}");
                    }

                    DebugViewLogRefreshFlagTm = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="module">Soaking Module class</param>
        public SoakingChillingTimeMng(SoakingModule module)
        {
            SoakingModule = module;
            StatusSoakingConfigParameter = SoakingModule.StatusSoakingDeviceFileObj?.StatusSoakingConfigParameter;
            ChillingMonitoringThreadRestartTimer = new Timer(new TimerCallback(ChillingTimeMonitoringThreadRestart), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Init Chilling TimeMng
        /// </summary>
        public void InitChillingTimeMng()
        {
            if (InitFlag)
                return;

            ChillingTimeMonitoringThreadStart();
            InitFlag = true;
        }

        /// <summary>
        /// chilling time의 증가/감소 처리
        /// </summary>
        /// <param name="time_MilliSec"> 증가/감소될 시간(millisecond)</param>
        public void CalculateChillingTime(long time_MilliSec)
        {
            try
            {
                lock (chillingTime_lockObject)
                {
                    ChillingTimeMillSec += time_MilliSec;
                    if (ChillingTimeMillSec < 0)
                        ChillingTimeMillSec = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// Return Chilling Time
        /// </summary>
        /// <returns>Chilling Time</returns>
        public long GetChillingTime()
        {
            lock (chillingTime_lockObject)
            {
                return ChillingTimeMillSec;
            }
        }

        /// <summary>
        /// Chilling Time을 관리하기 위한 Thread
        /// </summary>
        private void ChillingTimeMonitoringThreadStart()
        {
            ChillingTimeMonitoringThread = new Thread(() =>
            {
                LoggerManager.SoakingLog($"Monitoring Thread start.");
                try
                {
                    bool ChllingIncreaseFlag_ForLogWrite = false;
                    bool LogWriteForStauseSoaking_UsingFlag = false;
                    bool LogWriteForParameterNull = false;
                    bool firstCheck_CardDocking = true;
                    string PositionInfo = "";
                    
                    Stopwatch IntervalCheck_Stopwatch = new Stopwatch();
                    while (false == monitoringThreadStopFlag)
                    {
                        void CheckChillingCondition(ref bool chuckPos, ref bool temperatureDiff, bool Log)
                        {
                            chuckPos = IsChuckPositionIncreaseChillingTime(Log, ref PositionInfo);
                            temperatureDiff = IsTempIncreaseChillingTime();                                                                                   
                        }
                        SoakingModule.WaferAligner().ModuleState.GetState();
                        if (false == SoakingModule.MonitoringManager().IsMachineInitDone)
                        {
                            Thread.Sleep(3000);
                            continue;
                        }

                        CardDockingStateCheck(firstCheck_CardDocking);
                        CheckDebugFunc(firstCheck_CardDocking);
                        if (firstCheck_CardDocking)
                            firstCheck_CardDocking = false;

                        if (null == StatusSoakingConfigParameter)
                        {
                            if (false == LogWriteForParameterNull)
                            {
                                LoggerManager.SoakingErrLog($"Parameter is null.---------------------------");
                                LogWriteForParameterNull = true;
                            }

                            Thread.Sleep(5000);
                            continue;
                        }

                        bool ManualSoakingWorking = false;
                        if (SoakingModule.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE && SoakingModule.ManualSoakingStart)
                            ManualSoakingWorking = true;

                        if (false == GetStatusSoakingConfig().UseStatusSoaking.Value && false == ManualSoakingWorking)
                        {
                            if (false == LogWriteForStauseSoaking_UsingFlag)
                            {
                                LoggerManager.SoakingLog($"it doesn't set 'status using flag'");
                                LogWriteForStauseSoaking_UsingFlag = true;
                            }

                            Thread.Sleep(3000);
                            continue;
                        }
                        else
                        {
                            if (LogWriteForStauseSoaking_UsingFlag)
                            {
                                LogWriteForStauseSoaking_UsingFlag = false;
                                LoggerManager.SoakingLog($"Use status soaking");
                            }
                        }

                        bool before_IsPositionChillingTimeIncrease = false, before_IsTemperatureChiilingTimeIncrease = false;
                        CheckChillingCondition(ref before_IsPositionChillingTimeIncrease, ref before_IsTemperatureChiilingTimeIncrease, true);

                        IntervalCheck_Stopwatch.Reset();
                        IntervalCheck_Stopwatch.Start();
                        Thread.Sleep(checkIntervalTimeMil);

                        bool after_IsPositionChillingTimeIncrease = false, after_IsTemperatureChiilingTimeIncrease = false;
                        CheckChillingCondition(ref after_IsPositionChillingTimeIncrease, ref after_IsTemperatureChiilingTimeIncrease, false);
                        IntervalCheck_Stopwatch.Stop();

                        //good position 유지
                        if(false == before_IsPositionChillingTimeIncrease
                            && false == before_IsTemperatureChiilingTimeIncrease
                            && false  == after_IsPositionChillingTimeIncrease
                            && false == after_IsTemperatureChiilingTimeIncrease)
                        {
                                                       
                            ChillingTimeProc_InGoodPosition(IntervalCheck_Stopwatch.ElapsedMilliseconds);
                            if (showDebugViewData)
                                Trace.WriteLine($"[ChillingTimeMng][StatusSoakingDbg] ChillingTime Info - Good position, Current chilling time:{ChillingTimeMillSec}");

                            if (ChllingIncreaseFlag_ForLogWrite)
                            {
                                LoggerManager.SoakingLog($"Chilling time increase End(Current chillingTime:{ChillingTimeMillSec})");
                                ChllingIncreaseFlag_ForLogWrite = false;
                            }

                            continue;
                        }

                        //not good positon 유지
                        if( ( before_IsPositionChillingTimeIncrease ||  before_IsTemperatureChiilingTimeIncrease) 
                            && ( after_IsPositionChillingTimeIncrease || after_IsTemperatureChiilingTimeIncrease)
                        )
                        {
                            CalculateChillingTime(IntervalCheck_Stopwatch.ElapsedMilliseconds);
                            if (false == ChllingIncreaseFlag_ForLogWrite)
                            {
                                LoggerManager.SoakingLog($"Chilling time increase Start(Current ChillingTime:{ChillingTimeMillSec})");
                                ChllingIncreaseFlag_ForLogWrite = true;
                            }

                            if (showDebugViewData)
                                Trace.WriteLine($"[ChillingTimeMng][StatusSoakingDbg] ChillingTime Info - Not good position, Current chilling time:{ChillingTimeMillSec}");                            
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                finally
                {
                    if (false == monitoringThreadStopFlag) /// 정상적인 종료가 아닌것으로 판단하여 시간을 두고 재실행 해준다.
                    {
                        LoggerManager.SoakingErrLog($"thread abnormal termination.");
                        ChillingMonitoringThreadRestartTimer.Change(5000, System.Threading.Timeout.Infinite);
                    }
                    else
                    {
                        LoggerManager.SoakingLog($"thread termination.");
                    }
                }
            });
            ChillingTimeMonitoringThread.Start();
        }

        /// <summary>
        /// Good Position에서 Chilling Time Ratio Policy에 따른 Chiling Time 처리        
        /// </summary>
        /// <param name="accumulatedTime"> Good position에서 위치한 시간(millisecond)</param>
        private void ChillingTimeProc_InGoodPosition(long accumulatedTime)
        {
            try
            {                                
                /// Maintain state에서 chilling time 증가 position이 아니라면 정책에 따라 Chilling time decrease 처리
                SoakingStateEnum state = (SoakingModule.InnerState as SoakingState).GetState();
                float RatioForMaintain = GetCurrentRatioPolicy(state);
                float TempChillingTime = (float)accumulatedTime * RatioForMaintain;
                long ChillingTimeValue = Convert.ToInt32(TempChillingTime);
                if (0 != ChillingTimeValue)
                {
                    CalculateChillingTime(ChillingTimeValue);
                }                                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 현재 Soaking의 status와 chuck에 올란 object에(empty, polish, standard)에 따른 Chilling Time Ratio를 반환한다.
        /// </summary>
        /// <param name="CurSoakingStatus"> 현재 Soaking Status </param>
        /// <returns>Ratio Policy에 따른 Ratio 값 반환</returns>
        private float GetCurrentRatioPolicy(ProberInterfaces.SoakingStateEnum CurSoakingStatus)
        {
            try
            {
                if (CurSoakingStatus != ProberInterfaces.SoakingStateEnum.PREPARE &&
                    CurSoakingStatus != ProberInterfaces.SoakingStateEnum.RECOVERY &&
                    CurSoakingStatus != ProberInterfaces.SoakingStateEnum.MAINTAIN)
                    return 0;

                SoakingRatioPolicy_Index RatioPolicyIdx = SoakingRatioPolicy_Index.SoakingRatioPolicy_End;
                var waferExist = SoakingModule.GetParam_Wafer().GetStatus();
                var waferType = SoakingModule.StageSupervisor().WaferObject.GetWaferType();
                if (waferExist == EnumSubsStatus.EXIST) // wafer obje존재시 
                {             
                    if(ProberInterfaces.SoakingStateEnum.MAINTAIN == CurSoakingStatus)
                    {
                        if (EnumWaferType.STANDARD == waferType)
                        {
                            if ((SoakingModule.ProbingModule().ProbingStateEnum == EnumProbingState.ZUP ||
                                SoakingModule.ProbingModule().ProbingStateEnum == EnumProbingState.ZUPDWELL) &&
                                SoakingModule.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                            {
                                //probing 중일때
                                RatioPolicyIdx = SoakingRatioPolicy_Index.Maintain_Probing;
                            }
                            else
                            {
                                //maintatin soaking에서 standard/pw 사용 여부에 따른 처리(maintain에서 pw사용인데 standard라면 soaking 대상 wafer가 아니므로 Maintain_NotExistWafer로 설정)
                                if (false == GetStatusSoakingConfig().MaintainStateConfig.UsePolishWafer.Value)
                                    RatioPolicyIdx = SoakingRatioPolicy_Index.Maintain_UseStandardWafer;
                                else
                                    RatioPolicyIdx = SoakingRatioPolicy_Index.Maintain_NotExistWafer;
                            }
                        }
                        else if(EnumWaferType.POLISH == waferType)
                        {
                            if (false == GetStatusSoakingConfig().MaintainStateConfig.UsePolishWafer.Value)
                                RatioPolicyIdx = SoakingRatioPolicy_Index.Maintain_NotExistWafer;
                            else
                                RatioPolicyIdx = SoakingRatioPolicy_Index.Maintain_UseStandardWafer;
                        }
                    }
                    else if(ProberInterfaces.SoakingStateEnum.RECOVERY == CurSoakingStatus) //Recovery state에서 wafer가 있지만 Recovery soaking에 대상 wafer가 인지를 확인하여 결정한다.
                    {
                        if( (true == GetStatusSoakingConfig().RecoveryStateConfig.UsePolishWafer.Value && EnumWaferType.POLISH != waferType) || 
                            (false == GetStatusSoakingConfig().RecoveryStateConfig.UsePolishWafer.Value && EnumWaferType.STANDARD != waferType)
                            )
                        {
                            RatioPolicyIdx = SoakingRatioPolicy_Index.Recovery_NotExistWafer;
                        }
                    }
                }
                else
                {
                    if (ProberInterfaces.SoakingStateEnum.RECOVERY == CurSoakingStatus) //Recovery State에서는 wafer가 없는 경우에만 Good position에 대해 Chilling Time 처리를 해준다.
                        RatioPolicyIdx = SoakingRatioPolicy_Index.Recovery_NotExistWafer;
                    else if (ProberInterfaces.SoakingStateEnum.MAINTAIN == CurSoakingStatus)
                        RatioPolicyIdx = SoakingRatioPolicy_Index.Maintain_NotExistWafer;
                }

                if (SoakingRatioPolicy_Index.SoakingRatioPolicy_End != RatioPolicyIdx)
                {
                    var FindPolicyItem = GetStatusSoakingConfig().AdvancedSetting.ChillingRatioPolicy.Where(x => x.PolicyIndex.Value == (int)RatioPolicyIdx).FirstOrDefault();
                    if (null != FindPolicyItem)
                    {
                        return FindPolicyItem.Ratio.Value;
                    }
                    else
                    {
                        //probing 일때는 초당 1초씩 누적된 Chilling time을 감소하기 위함
                        if(RatioPolicyIdx == SoakingRatioPolicy_Index.Maintain_Probing)
                            return -1;
                    }
                }               
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return 0f;

        }

        /// <summary>
        /// PV와 SV의 온도 차이가 Temp deviation으로 발생하였는지 반환한다.
        /// </summary>
        /// <returns>ture: 증가 , false: 증가 안함 </returns>
        private bool IsTempIncreaseChillingTime()
        {
            try
            {
                if (SoakingModule.IsCurTempWithinSetSoakingTempRange())
                {
                    return false;
                }

                else
                {
                    if (CardDocking == false && (SoakingModule.SoakingSysParam_IParam as SoakingSysParameter).SoakWithoutCard.Value == false)
                    {
                        return false;
                    }
                    return true;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        #region ==> public

        /// <summary>
        /// 현재까지 누적된 Chilling Time을 초기화하는 함수
        /// 예를 들어 Prepare Soaking을 진행한다고 하면 Prepare Soaking 시작전 호출해서 초기화 해놓고
        /// Prepare Soaking을 진행한다.(진행하면서 다시 누적된 Chilling Time에 따른 Soaking을 마지막에 다시 하기 위함.
        /// </summary>
        public bool ChillingTimeInit()
        {
            try
            {
                lock (chillingTime_lockObject)
                {
                    LoggerManager.SoakingLog($"Set Chilling Time 0.({ChillingTimeMillSec} -> 0)");
                    ChillingTimeMillSec = 0;
                    return true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return false;
        }

        /// <summary>
        /// Monitoring thread 종료 처리 함수
        /// </summary>
        public void Exit_ChillingTimeMng()
        {
            monitoringThreadStopFlag = true;
        }

        /// <summary>
        /// 현재 Chuck의 위치가 Chilling Time이 증가하는 위치에 있는지 여부 체크        
        /// </summary>
        /// <returns>true : 증가하는 위치, false: 증가하지 않는 위치 </returns>
        public bool IsChuckPositionIncreaseChillingTime(bool debugLog, ref string PositionInfo)
        {
            double CurChuck_posX = 0;
            double CurChuck_posY = 0;
            double CurChuck_posZ = 0;

            double RefChuck_posX = 0;
            double RefChuck_posY = 0;
            double RefChuck_posZ = SoakingModule.StageSupervisor().PinMinRegRange;  /// card가 없는 경우. 
            AlignStateEnum pinalignstate = AlignStateEnum.IDLE;
            bool Allowance_Ratio_Exceeded = false;
            try
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;
                WaferCoordinate wafercoord = new WaferCoordinate();
                PinCoordinate pincoord = new PinCoordinate();
                MachineCoordinate targetpos = new MachineCoordinate();

                //pin / wafer align중이라면 chillingtime증가로 반환
                if (SoakingModule.WaferAligner().ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                   SoakingModule.PinAligner().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    if (showDebugViewData && debugLog)
                        Trace.WriteLine($"[ChillingTimeMng][StatusSoakingDbg] Increase Chilling Time(pin or wafer alignment is doing)");

                    return true;
                }


                if (SoakingModule.ProbingModule().ProbingProcessStatus?.UnderDutDevs?.Count > 0 &&
                    SoakingModule.StageSupervisor()?.ProbeCardInfo?.ProbeCardDevObjectRef?.DutList?.Count() > 0)
                {
                    var totalHeatingRatio = GetStatusSoakingConfig().AdvancedSetting.UnderDutDiesPercnetage.Value;
                    double prbingDutCnt = SoakingModule.ProbingModule().ProbingProcessStatus.UnderDutDevs.Count;
                    double TotalDutCnt = SoakingModule.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count();
                    if (prbingDutCnt / TotalDutCnt * 100 < totalHeatingRatio)
                    {
                        Allowance_Ratio_Exceeded = true;
                    }
                }

                bool need_to_target_pos = true;
                double maintain_od = 0;
                if (CardDocking)
                {
                    pinalignstate = SoakingModule.StageSupervisor().ProbeCardInfo.AlignState.Value;
                    if (AlignStateEnum.DONE != pinalignstate) // pin align이 되어 있지 않다면 Chilling Time 증가
                    {
                        if (showDebugViewData && debugLog)
                            Trace.WriteLine($"[ChillingTimeMng][StatusSoakingDbg] Increase Chilling Time(need to pin align)");

                        return true;
                    }
                }
                else
                {
                    if ((SoakingModule.SoakingSysParam_IParam as SoakingSysParameter).SoakWithoutCard.Value == false)
                    {
                        ProbeAxisObject zaxis = SoakingModule.MotionManager().GetAxis(EnumAxisConstants.Z);

                        RefChuck_posX = (SoakingModule.SoakingSysParam_IParam as SoakingSysParameter).WithoutCardAndNotSoaking_ChuckPosX.Value;
                        RefChuck_posY = (SoakingModule.SoakingSysParam_IParam as SoakingSysParameter).WithoutCardAndNotSoaking_ChuckPosY.Value;
                        RefChuck_posZ = zaxis.Param.ClearedPosition.Value;
                        need_to_target_pos = false;
                    }
                }
                if (need_to_target_pos)
                {
                    Get_Soaking_OD(out maintain_od);
                    retval = SoakingModule.Get_StatusSoakingPosition(ref wafercoord, ref pincoord, false, false); //기준이 되는 좌표를 확인(wafer가 없어도 chuck focusing안함. 매번 계속할 수도 없고, 해당 Thread에서 chuck focusiong은 위험, device정보로 사용)
                    if (showDebugViewData && debugLog)
                        Trace.WriteLine($"[ChillingTimeMng][StatusSoakingDbg] wafercoord.x:{wafercoord.X.Value:0.00}, wafercoord.y:{wafercoord.Y.Value:0.00}, wafercoord.z:{wafercoord.Z.Value:0.00}, pincoord.x({pincoord.X.Value:0.00}), pincoord.y({pincoord.Y.Value:0.00}), pincoord.z({pincoord.Z.Value:0.00})");

                    targetpos = SoakingModule.GetTargetMachinePosition(wafercoord, pincoord, maintain_od);

                    RefChuck_posX = targetpos.X.Value;
                    RefChuck_posY = targetpos.Y.Value;
                    RefChuck_posZ = targetpos.Z.Value;
                }
                SoakingModule.MotionManager().GetRefPos(EnumAxisConstants.X, ref CurChuck_posX);
                SoakingModule.MotionManager().GetRefPos(EnumAxisConstants.Y, ref CurChuck_posY);
                SoakingModule.MotionManager().GetRefPos(EnumAxisConstants.Z, ref CurChuck_posZ);

                double ChuckAwayTolForChilling_X = GetStatusSoakingConfig().AdvancedSetting.ChuckAwayTolForChillingTime_X.Value;
                double ChuckAwayTolForChilling_Y = GetStatusSoakingConfig().AdvancedSetting.ChuckAwayTolForChillingTime_Y.Value;
                double ChuckAwayTolForChilling_Z = GetStatusSoakingConfig().AdvancedSetting.ChuckAwayTolForChillingTime_Z.Value;

                var DiffX = CurChuck_posX - RefChuck_posX;
                var DiffY = CurChuck_posY - RefChuck_posY;
                var DiffZ = CurChuck_posZ - RefChuck_posZ;

                bool IncreaseChillingTime = false;
                if ((Math.Abs(DiffX) > ChuckAwayTolForChilling_X) ||
                   (Math.Abs(DiffY) > ChuckAwayTolForChilling_Y))
                {
                    if (Allowance_Ratio_Exceeded)
                    {
                        IncreaseChillingTime = true;
                    }
                }

                // z축이 Tolerance는 벗어났어도 기준점 보다 probe card쪽에 가까이 위치하지 않은 경우
                if ((Math.Abs(DiffZ) > ChuckAwayTolForChilling_Z) && (CurChuck_posZ < RefChuck_posZ))
                    IncreaseChillingTime = true;

                PositionInfo = $"CurChuckPos(x:{CurChuck_posX:0.00}, y:{CurChuck_posY:0.00}, z:{CurChuck_posZ:0.00}), RefPos(x:{RefChuck_posX:0.00}, y:{RefChuck_posY:0.00}, z:{RefChuck_posZ:0.00}), od:{maintain_od:0.00}, diff:(x:{DiffX:0.00}, y:{DiffY:0.00}, z:{DiffZ:0.00}), Increase:{IncreaseChillingTime.ToString()}, CurChilling:{ChillingTimeMillSec}";
                if (showDebugViewData && debugLog)
                {                    
                    Trace.WriteLine($"[StatuSoaking][StatusSoakingDbg] {PositionInfo}");
                }

                return IncreaseChillingTime;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return true;
            }
        }

        private EventCodeEnum Get_Soaking_OD(out double retval) 
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            retval = GetStatusSoakingConfig().MaintainStateConfig.NotExistWaferObj_OD.Value;
            try
            {
                var waferExist = SoakingModule.GetParam_Wafer().GetStatus();
                if (waferExist == EnumSubsStatus.EXIST) 
                {
                    var StepInfo = SoakingModule.StatusSoakingTempInfo.StatusSoakingStepProcList.FirstOrDefault();
                    if (StepInfo != null)
                    {
                        retval = StepInfo.OD_Value;
                        ret = EventCodeEnum.NONE;
                    }
                    else 
                    {
                        ret = EventCodeEnum.NONE;
                    }
                } 
                else 
                {
                    SoakingStateEnum state = (SoakingModule.InnerState as SoakingState).GetState();
                    switch (state)
                    {
                        case SoakingStateEnum.PREPARE:
                            retval = GetStatusSoakingConfig().PrepareStateConfig.NotExistWaferObj_OD.Value;
                            break;
                        case SoakingStateEnum.RECOVERY:
                            retval = GetStatusSoakingConfig().RecoveryStateConfig.NotExistWaferObj_OD.Value;
                            break;
                        case SoakingStateEnum.MAINTAIN:
                            retval = GetStatusSoakingConfig().MaintainStateConfig.NotExistWaferObj_OD.Value;
                            break;
                        case SoakingStateEnum.MANUAL:
                            retval = GetStatusSoakingConfig().ManualSoakingConfig.NotExistWaferObj_OD.Value;
                            break;
                    }
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }


        /// <summary>
        /// 현재 누적된 Chilling Time과 그에 따른 Soaking 해야할 시간을 반환한다.
        /// Soaking을 해야 하는 시간은 사용자가 입력한 Chilling Time Table에 의거하여 반환한다.
        /// </summary>
        /// <param name="accumulated_chillingTime"> UseAutomaticCalculatedChilingTime 인자에 따라 들어온 chilling time을 사용하거나, 누적된 ChillingTime을 반환</param>
        /// <param name="SoakingTime"> 누적된 Chilling Time에 따른 진행해야 할 Soaking 시간(milisecond)</param>
        /// <param name="InsideChillingTimeTable"> Chilling Time table 영역내에 있는지 여부(accumulated_chillingTime이 Table에 최소 시간보다 작을때만 false, 그 이외에는 true) </param>
        /// <param name="UseAutomaticCalculatedChilingTime"> chuck의 위치를 감시하여 누적된 ChillingTime을 사용할 것인지 아니면 첫번째 인자로 들어온 accumulated_chillingTime을 사용할 것인지 여부, true(자동), false(인자들어온것) </param>
        /// <returns>EventCodeEnum</returns>

        public EventCodeEnum GetCurrentChilling_N_TimeToSoaking(ref long accumulated_chillingTime, ref int SoakingTimeMil, ref bool InChillingTimeTable, bool UseAutomaticCalculatedChilingTime = true)
        {
            try
            {
                if (null == StatusSoakingConfigParameter)
                {
                    if (showDebugViewData)
                        Trace.WriteLine($"Parameter info is null");
                    return EventCodeEnum.PARAM_ERROR;
                }

                if (GetStatusSoakingConfig().RecoveryStateConfig.SoakingChillingTimeTable.Count() <= 0)
                {
                    if (showDebugViewData)
                        Trace.WriteLine($"[ChillingTimeMng][StatusSoakingDbg] ChillingTimeTable is empty");
                    return EventCodeEnum.PARAM_ERROR;
                }

                // 1. Get Current ChillingTime Value
                if(UseAutomaticCalculatedChilingTime)
                    accumulated_chillingTime = ChillingTimeMillSec;

                long CurChillingTimeMil = accumulated_chillingTime;

                var between_start = GetStatusSoakingConfig().RecoveryStateConfig.SoakingChillingTimeTable.LastOrDefault(x => (long)(x.ChillingTimeSec.Value * 1000) <= CurChillingTimeMil);
                var between_end = GetStatusSoakingConfig().RecoveryStateConfig.SoakingChillingTimeTable.FirstOrDefault(x => (long)(x.ChillingTimeSec.Value * 1000) >= CurChillingTimeMil);

                //< AccumulatedChillingTime이 사용자가 설정한 범위 이상.(PrepareSoaking Time과 비교하여 PrepareSoaking Time이 ChillingTime Table에 정의된 시간보다 클 때만 사용)
                if (null == between_end)
                {
                    InChillingTimeTable = true;                    
                    int PrepareSoakingTimeSec = GetStatusSoakingConfig().PrepareStateConfig.SoakingTimeSec.Value * 1000;
                    int RecoverySoakingTimeSec = 0;
                    if(null != between_start)
                        RecoverySoakingTimeSec = between_start.SoakingTimeSec.Value * 1000;

                    if (PrepareSoakingTimeSec > RecoverySoakingTimeSec) //prepare soaking time이 chilling time table에 지정된 max time보다 클 경우에만 사용                    
                        SoakingTimeMil = PrepareSoakingTimeSec;                    
                    else
                        SoakingTimeMil = RecoverySoakingTimeSec;

                    if (showDebugViewData)
                        Trace.WriteLine($"[ChillingTimeMng][StatusSoakingDbg] AccumulatedChillingTime is out of the chillingTime range(accumulated_chillingTime:{accumulated_chillingTime}, SoakingTime(PrePare):{SoakingTimeMil})");

                    return EventCodeEnum.NONE;
                }

                /// ChillingTime table의 사용자가 입력한 정보와 완전 매칭인 경우
                if (null != between_start && between_start.ChillingTimeSec == between_end.ChillingTimeSec)
                {
                    InChillingTimeTable = true;
                    SoakingTimeMil = between_start.SoakingTimeSec.Value * 1000; //start value가 chillingTimeTable의 가장 마지막 table row정보 반환(Parameter는 초단위 데이터로 계산은 millisecond로 처리한다.)                    
                    if (showDebugViewData)
                        Trace.WriteLine($"[ChillingTimeMng][StatusSoakingDbg] AccumulatedChillingTime matched exactly(accumulated_chillingTime:{accumulated_chillingTime}, SoakingTime:{SoakingTimeMil})");

                    return EventCodeEnum.NONE;
                }

                float ChillingTime_X_Pos = 0f;
                float SoakingTime_Y_Pos = 0f;

                //AccumlatedChillingTime이 ChillingTime Table의 최소시간 보다 작은 경우(Chilling 0, SoakingTime 0을 기준으로)
                if (null == between_start && null != between_end)
                {
                    InChillingTimeTable = false;
                    accumulated_chillingTime = CurChillingTimeMil;
                    ChillingTime_X_Pos = (float)(between_end.ChillingTimeSec.Value);
                    SoakingTime_Y_Pos = (float)(between_end.SoakingTimeSec.Value);
                    if (showDebugViewData)
                        Trace.WriteLine($"[ChillingTimeMng][StatusSoakingDbg] Pos1_Chilling_N_Soaking[ chilling(0),soaking(0) ], Pos_2_Chilling_N_Soaking[ chilling({between_end.ChillingTimeSec.Value}),soaking({between_end.SoakingTimeSec.Value}) ]");
                }
                else
                {
                    InChillingTimeTable = true;
                    ChillingTime_X_Pos = (float)(between_end.ChillingTimeSec.Value - between_start.ChillingTimeSec.Value);
                    SoakingTime_Y_Pos = (float)(between_end.SoakingTimeSec.Value - between_start.SoakingTimeSec.Value);
                    if (showDebugViewData)
                        Trace.WriteLine($"[ChillingTimeMng][StatusSoakingDbg] Pos_1_Chilling_N_Soaking[ chilling({between_start.ChillingTimeSec.Value}),soaking({between_start.SoakingTimeSec.Value}) ], Pos_2_Chilling_N_Soaking[ chilling({between_end.ChillingTimeSec.Value}), soaking({between_end.SoakingTimeSec.Value}) ]");
                }

                if (0 == ChillingTime_X_Pos)
                {
                    LoggerManager.SoakingErrLog($"Try to divide zero");
                    return EventCodeEnum.PARAM_ERROR;
                }

                //start와 end 일차 함수 그래프로 중간 사이의 값을 도출(y = ax + b )
                float slope = SoakingTime_Y_Pos / ChillingTime_X_Pos;
                float b_value = (float)(between_end.SoakingTimeSec.Value) - (slope * (float)(between_end.ChillingTimeSec.Value));
                float Soaking_Time = (float)(slope * CurChillingTimeMil) + (b_value * 1000f);
                SoakingTimeMil = (int)Soaking_Time;
                if (showDebugViewData)
                    Trace.WriteLine($"[ChillingTimeMng][StatusSoakingDbg] accumulated_chillingTime:{accumulated_chillingTime}, SoakingTime:{SoakingTimeMil}");
                return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.UNKNOWN_EXCEPTION;
            }
        }

        /// <summary>
        /// 실제 Soaking을 수행한 시간으로 사용자가 설정한 Chilling time table에 따라 감소해야할 Chilling Time을 산출하여
        /// 누적되어 있는 Chilling Time을 감소시킨다.
        /// </summary>
        /// <param name="ProcessedSoakingTimeMil"> 실제 Soaking을 진행한 시간(millisecond)</param>
        /// <param name="received_chillingTimeMil"> Soaking 진행을 위해 받았던 Chilling Time정보</param>
        /// <param name="completeSoaking"> Soaking을 모두 완료하였는지 여부, 어느정도 진행하다 멈춘 경우는 fasle로 </param>
        /// received_chillingTimeInfo가 필요한 이유는 Chilling Time table을 넘어가는 시간에 대해 Soaking 진행 시 SoakingTime은 고정되지만 Chilling Time은 계속 증가하면서 변경된다.
        /// 따라서 Soaking을 위해 호출한 시점에 받은 Chilling Time 정보를 받아 감소시켜야 한다.
        /// <returns>EventCodeEnum</returns>
        public EventCodeEnum ActualProcessedSoakingTime(long ProcessedSoakingTimeMil, long received_chillingTimeMil, bool completeSoaking)
        {
            if (ProcessedSoakingTimeMil <= 0)
                return EventCodeEnum.NONE;

            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"Parameter info is null");
                return EventCodeEnum.PARAM_ERROR;
            }

            if (GetStatusSoakingConfig().RecoveryStateConfig.SoakingChillingTimeTable.Count() <= 0)
            {
                LoggerManager.SoakingErrLog($"ChillingTimeTable is empty");
                return EventCodeEnum.PARAM_ERROR;
            }

            try
            {
                ///정해진 시간만큼 Soaking이 완료된 case로 전달해줬던 ChillingTime을 감소한다.
                if (completeSoaking)
                {
                    LoggerManager.SoakingLog($"Decrease ChillingTime(CurAccumulatedChillingTime:{ChillingTimeMillSec} ,Decrease_chillingTime:{received_chillingTimeMil}, ProcessedSoakingTime:{ProcessedSoakingTimeMil}, completeSoaking:{completeSoaking})");
                    CalculateChillingTime(received_chillingTimeMil * -1);
                    return EventCodeEnum.NONE;
                }
                else
                {                  
                    long chillingTime = 0;                    
                    if (EventCodeEnum.NONE == GetChillingTimeAccordingToSoakingTime(ProcessedSoakingTimeMil, out chillingTime))
                    {
                        CalculateChillingTime(chillingTime * -1);
                        LoggerManager.SoakingLog($"Decrease ChillingTime(CurAccumulatedChillingTime:{ChillingTimeMillSec} ,Decrease_chillingTime:{chillingTime}, ProcessedSoakingTime:{ProcessedSoakingTimeMil}, completeSoaking:{completeSoaking})");
                    }
                    else
                    {
                        LoggerManager.SoakingErrLog($"Failed to get chilling time according to soakingTime.");
                        return EventCodeEnum.SOAKING_FAILED_GET_SOAKING_DATA;

                    }
                        
                    return EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.UNKNOWN_EXCEPTION;
            }
        }

        /// <summary>
        /// soaking시간에 따른 chilling time을 반환한다.(chilling time table에 기반하여 인자로 들어온 SoakingTime에 따른 chilling time반환)
        /// </summary>
        /// <param name="ProcessedSoakingTimeMil"> Soaking time이 진행된 시간</param>
        /// <param name="chillingTime">Soaking time에 따른 chilling time 반환</param>
        /// <returns>EventCodeEnum</returns>
        public EventCodeEnum GetChillingTimeAccordingToSoakingTime(long ProcessedSoakingTimeMil, out long chillingTime)
        {
            chillingTime = 0;
 
            var between_start = GetStatusSoakingConfig().RecoveryStateConfig.SoakingChillingTimeTable.LastOrDefault(x => (long)(x.SoakingTimeSec.Value * 1000) <= ProcessedSoakingTimeMil);
            var between_end = GetStatusSoakingConfig().RecoveryStateConfig.SoakingChillingTimeTable.FirstOrDefault(x => (long)(x.SoakingTimeSec.Value * 1000) >= ProcessedSoakingTimeMil);

            float ChillingTime_X_Pos = 0f;
            float SoakingTime_Y_Pos = 0f;
                        
            if (null == between_end) // chilling time table에서 설정된 Soaking 시간보다 더 큰 시간의 Soaking을 진행한 경우, 현재까지 누적된 Chilling Time을 반환
            {
                chillingTime = GetChillingTime();
                LoggerManager.SoakingLog($"ProcessedSoakingTimeMil is too long. return current Elapsed ChillingTime({chillingTime})");
                return EventCodeEnum.NONE;
            }
            
            if (null == between_start && null != between_end)  // chilling time table에서 가장 작은 시간보다 더 작은 경우(Chilling 0, Soaking 0 시작 기준으로 계산)
            {
                ChillingTime_X_Pos = (float)between_end.ChillingTimeSec.Value;
                SoakingTime_Y_Pos = (float)between_end.SoakingTimeSec.Value;
                LoggerManager.SoakingLog($"ProcessedSoaking Time is less than the smallest time in ChillingTimeTable.(ProcessedSoakingTimeMil:{ProcessedSoakingTimeMil})");
                LoggerManager.SoakingLog($"Pos_1_Chilling_N_Soaking(0,0), Pos_2_Chilling_N_Soaking({between_end.ChillingTimeSec.Value }, {between_end.SoakingTimeSec.Value })");
            }
            else if (null != between_start && null != between_end)
            {
                ChillingTime_X_Pos = (float)(between_end.ChillingTimeSec.Value - between_start.ChillingTimeSec.Value);
                SoakingTime_Y_Pos = (float)(between_end.SoakingTimeSec.Value - between_start.SoakingTimeSec.Value);
                LoggerManager.SoakingLog($"Pos_1_Chilling_N_Soaking({between_start.ChillingTimeSec.Value},{between_start.SoakingTimeSec.Value}), Pos_2_Chilling_N_Soaking({between_end.ChillingTimeSec.Value }, {between_end.SoakingTimeSec.Value })");
            }

            if(ChillingTime_X_Pos == 0 && SoakingTime_Y_Pos == 0)
            {
                chillingTime = between_start.ChillingTimeSec.Value * 1000;
                if (showDebugViewData)
                    Trace.WriteLine($"[ChillingTimeMng][StatusSoakingDbg] SoakingTime matched exactly(accumulated_chillingTime:{between_end.ChillingTimeSec.Value }, SoakingTime:{between_end.SoakingTimeSec.Value})");

                return EventCodeEnum.NONE;
            }
            
            if (0 == ChillingTime_X_Pos)
            {
                LoggerManager.SoakingErrLog($"Try to divide zero ");
                return EventCodeEnum.PARAM_ERROR;
            }

            float slope = SoakingTime_Y_Pos / ChillingTime_X_Pos;
            float b_value = (float)(between_end.SoakingTimeSec.Value) - (slope * (float)(between_end.ChillingTimeSec.Value));
            float temp_val = (float)(ProcessedSoakingTimeMil - (b_value * 1000)) / slope;
            chillingTime = Convert.ToInt64(temp_val);
            LoggerManager.SoakingLog($"Slope({slope}), b_value({b_value}), chillingTime(float:{temp_val}, long:{chillingTime})");
            
            return EventCodeEnum.NONE;
        }

        /// <summary>
        /// card docking 여부를 반환
        /// </summary>
        /// <returns> true: 존재, false : 미존재</returns>
        public bool GetCardDockingFlag()
        {
            return CardDocking;
        }

        /// <summary>
        /// debug 문구 출력 여부 flag
        /// </summary>
        /// <returns></returns>
        public bool IsShowDebugString()
        {
            return showDebugViewData;
        }

        #endregion

    }
}
