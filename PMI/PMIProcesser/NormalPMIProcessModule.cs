using LogModule;
using Newtonsoft.Json;
using PMIModuleParameter;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PMI;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace PMIProcesser
{
    public class NormalPMIProcessModule : IProcessingModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> Common PNP Declaration

        public NormalPMIProcessModule()
        {

        }
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

        public SubModuleStateBase SubModuleState { get; set; }

        public SubModuleMovingStateBase MovingState { get; set; }

        #endregion

        public bool Initialized { get; set; } = false;


        //private List<MachineIndex> GetRemainingPMIDies(DieMapTemplate PMIMap)
        //{
        //    List<MachineIndex> retval = new List<MachineIndex>();

        //    try
        //    {
        //        // this.ProbingModule().ProbingProcessStatus.UnderDutDevs
        //        // this.PMIModule().DoPMIInfo.ProcessedPMIMIndex

        //        foreach (var dut in this.ProbingModule().ProbingProcessStatus.UnderDutDevs.Select((value, i) => new { i, value }))
        //        {
        //            var value = dut.value;
        //            var index = dut.i;

        //            MachineIndex MI = value.DieIndexM;

        //            MachineIndex AlreayTestedMI = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.FirstOrDefault(x => x.XIndex == MI.XIndex && x.YIndex == MI.YIndex);

        //            // 아직 테스트 되지 않았을 때
        //            if(AlreayTestedMI == null)
        //            {
        //                // 테스트 다이만 진행한다.
        //                if (value.State.Value == DieStateEnum.TESTED)
        //                {
        //                    if (PMIMap.GetEnable((int)MI.XIndex, (int)MI.YIndex) == 0x01)
        //                    {
        //                        retval.Add(MI);
        //                    }
        //                }
        //            }
        //        }

        //        LoggerManager.Debug($"[NormalPMIProcessModule], GetRemainingPMIDies() : Tested Count : {this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Count}, Remaining Count : {retval.Count}");
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        #region ==> Common PNP Function
        /// <summary>
        /// 실제 프로세싱 하는 코드
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum DoExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                MovingState.Moving();

                if (IsExecute())
                {
                    retVal = this.StageSupervisor().StageModuleState.ZCLEARED();

                    if (retVal == EventCodeEnum.NONE)
                    {
                        this.LotOPModule().VisionScreenToLotScreen();

                        var pmidevparam = this.PMIModule().PMIModuleDevParam_IParam as PMIModuleDevParam;

                        if (pmidevparam != null)
                        {
                            //IPMIInfo PMIInfo = this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo();

                            //int curWaferNum = this.PMIModule().LotOPModule().LotInfo.LoadedWaferCountUntilBeforeLotStart;
                            //int curMapIndex = PMIInfo.WaferTemplateInfo[curWaferNum - 1].SelectedMapIndex.Value;

                            //var PMIMap = PMIInfo.NormalPMIMapTemplateInfo[curMapIndex];

                            if (pmidevparam.SkipPMIDiesAfterPause.Value == true)
                            {
                                // ProcessedPMIMIndex의 개수가 존재한다는 것은, 이미 PMI를 진행한적이 있다는 것, 이 때, 이곳을 다시 들어왔다는 것은
                                // Pause 이후, 재개 된 후, 다시 들어온 것을 의미한다.
                                // 이 때, 해당 파라미터가 켜져있으면, PMI를 진행하지 않은 DIE가 남아있더라도, 진행하지 않고 동작을 종료한다.

                                if (this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Count > 0)
                                {
                                    // Set module state as done state
                                    SubModuleState = new SubModuleDoneState(this);

                                    retVal = EventCodeEnum.NONE;

                                    return retVal;
                                }
                            }

                            List<MachineIndex> remainPMIDiesCount = this.PMIModule().GetRemainingPMIDies();

                            // PMI를 진행할 DIE가 남아 있는 경우
                            if (remainPMIDiesCount != null && remainPMIDiesCount.Count > 0)
                            {
                                // Light
                                // Light의 경우, 한 번만 설정해준다.
                                if (pmidevparam.AutoLightEnable.Value == true)
                                {
                                    this.PMIModule().DoPMIInfo.IsTurnAutoLight = true;
                                }
                                else
                                {
                                    this.PMIModule().DoPMIInfo.IsTurnAutoLight = false;
                                }
                                this.VisionManager().VisionLib.RecipeInit();

                                EventCodeEnum pmiDieResult = EventCodeEnum.NONE;
                                foreach (var MI in remainPMIDiesCount.ToList())
                                {
                                    this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Add(MI);

                                    // Focusing

                                    if (pmidevparam.FocusingEachGroupEnable.Value == true)
                                    {
                                        this.PMIModule().DoPMIInfo.IsTurnFocusing = true;
                                        this.PMIModule().DoPMIInfo.RememberFocusingZValue = 0;
                                    }
                                    else
                                    {
                                        if (pmidevparam.FocusingEnable.Value == true)
                                        {
                                            int processedcount = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Count();

                                            this.PMIModule().DoPMIInfo.IsTurnFocusing = this.PMIModule().CheckFocusingInterval(processedcount);
                                            this.PMIModule().DoPMIInfo.RememberFocusingZValue = 0;
                                        }
                                        else
                                        {
                                            this.PMIModule().DoPMIInfo.IsTurnFocusing = false;
                                        }
                                    }

                                    this.PMIModule().DoPMIInfo.WaferMapIndex = this.LotOPModule().LotInfo.LoadedWaferCountUntilBeforeLotStart;
                                    this.PMIModule().DoPMIInfo.WorkMode = PMIWORKMODE.AUTO;

                                    retVal = this.PMIModule().UpdateGroupingInformation();

                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        // PMI를 수행하기 전, 마지막 DIE 결과 초기화하고 사용.

                                        this.PMIModule().DoPMIInfo.LastPMIDieResult.PassPadCount = 0;
                                        this.PMIModule().DoPMIInfo.LastPMIDieResult.FailPadCount = 0;
                                        this.PMIModule().DoPMIInfo.LastPMIDieResult.MI = MI;
                                        this.PMIModule().DoPMIInfo.LastPMIDieResult.UI = this.CoordinateManager().MachineIndexConvertToUserIndex(MI);
                                        LoggerManager.Debug($"[NormalPMIProcessModule], PMI Start DIE XIndex = {this.PMIModule().DoPMIInfo.LastPMIDieResult.UI .XIndex} YIndex = {this.PMIModule().DoPMIInfo.LastPMIDieResult.UI.YIndex}");
                                        
                                        retVal = this.PMIModule().DoPMI();

                                        // PMI가 정상적으로 끝난 경우, 해당 결과를 설정되어 있는 파라미터에 의해 비교한 후, 리턴 값 생성.
                                        if (retVal == EventCodeEnum.NONE)
                                        {
                                            // 더 이상 진행할 DIE가 남아 있지 않을 때
                                            // 전체 성공 및 실패 개수 데이터 생성.

                                            // PMI를 진행한 모든 다이의 결과를 누적한 데이터 생성,
                                            // (1) 인스펙션을 진행한 전체 다이
                                            // (2) 인스펙션을 진행한 모든 패드 개수
                                            // (3) 전체 성공 개수
                                            // (4) 전체 실패 개수
                                            // 로그 찍고, 결과를 파라미터와 비교, Pause할지 말지 결정.

                                            List<MachineIndex> CheckedLastDie = this.PMIModule().GetRemainingPMIDies();

                                            //if (CheckedLastDie.Count == 0)
                                            //{
                                            retVal = MakeAllPadInfo(this.PMIModule().DoPMIInfo.LastPMIDieResult);
                                            //}

                                            if (retVal == EventCodeEnum.NONE)
                                            {
                                                if (pmidevparam.PauseMethod.Value == PMI_PAUSE_METHOD.IMMEDIATELY)
                                                {
                                                    var lastPMIDieResult = this.PMIModule().DoPMIInfo.LastPMIDieResult;
                                                    if (pmidevparam.PausePadCntPerWafer.Value > 0)
                                                    {
                                                        if (this.PMIModule().DoPMIInfo.AllFailPadCount >= pmidevparam.PausePadCntPerWafer.Value) //조건 변경
                                                        {
                                                            LoggerManager.Debug($"[NormalPMIProcessModule], DoExecute(), Fail Count used in Pause = {pmidevparam.PausePadCntPerWafer.Value}");
                                                            retVal = EventCodeEnum.PMI_PAUSE_IMMEDIATELY;
                                                        }
                                                    }

                                                    if (pmidevparam.PausePercentPerDie.Value > 0)
                                                    {
                                                        //perdie percent 를 count로 환산, 전체값 X 퍼센트 ÷ 100
                                                        double pauserperdie = (lastPMIDieResult.FailPadCount + lastPMIDieResult.PassPadCount) * pmidevparam.PausePercentPerDie.Value / 100;
                                                        if (lastPMIDieResult.FailPadCount >= pauserperdie)
                                                        {
                                                            LoggerManager.Debug($"[NormalPMIProcessModule], DoExecute(), Immediately Pause." +
                                                                    $" DIE Result Failed Count = {lastPMIDieResult.FailPadCount} >= Per Die Setting Count = {pauserperdie:0.00}({pmidevparam.PausePercentPerDie.Value})%");
                                                            retVal = EventCodeEnum.PMI_PAUSE_IMMEDIATELY;
                                                        }
                                                    }
                                                }
                                                else if (pmidevparam.PauseMethod.Value == PMI_PAUSE_METHOD.AFTER_ALL_DIE_INSPECTION)
                                                {
                                                    // 더 이상 진행할 DIE가 남아 있지 않을 때
                                                    if (CheckedLastDie.Count == 0)
                                                    {
                                                        if (pmidevparam.PausePadCntPerWafer.Value > 0)
                                                        {
                                                            if (this.PMIModule().DoPMIInfo.AllFailPadCount >= pmidevparam.PausePadCntPerWafer.Value)
                                                            {
                                                                LoggerManager.Debug($"[NormalPMIProcessModule], DoExecute(), Fail Count used in Pause = {pmidevparam.PausePadCntPerWafer.Value}");

                                                                retVal = EventCodeEnum.PMI_PAUSE_AFTER_ALL_DIE_INSPECTION;
                                                            }
                                                        }

                                                        if (pmidevparam.PausePercentPerDie.Value > 0)
                                                        {
                                                            var lastPMIDieResult = this.PMIModule().DoPMIInfo.LastPMIDieResult;
                                                            // perdie percent 를 count로 환산, 전체값 X 퍼센트 ÷ 100
                                                            double pauserperdie = (lastPMIDieResult.FailPadCount + lastPMIDieResult.PassPadCount) * pmidevparam.PausePercentPerDie.Value / 100;
                                                            if (lastPMIDieResult.FailPadCount >= pauserperdie)
                                                            {
                                                                LoggerManager.Debug($"[NormalPMIProcessModule], DoExecute(), All DIE Done." +
                                                                        $" DIE Result Failed Count = {lastPMIDieResult.FailPadCount} >= Per Die Setting Count = {pauserperdie:0.00}({pmidevparam.PausePercentPerDie.Value})%");
                                                                retVal = EventCodeEnum.PMI_PAUSE_AFTER_ALL_DIE_INSPECTION;
                                                            }

                                                            if (pmiDieResult == EventCodeEnum.PMI_RESERVE_PAUSE)
                                                            {
                                                                retVal = EventCodeEnum.PMI_PAUSE_AFTER_ALL_DIE_INSPECTION;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // 더 진행할 DIE가 남아 있을 경우
                                                        if (pmidevparam.PausePercentPerDie.Value > 0)
                                                        {
                                                            var lastPMIDieResult = this.PMIModule().DoPMIInfo.LastPMIDieResult;
                                                            // perdie percent 를 count로 환산, 전체값 X 퍼센트 ÷ 100
                                                            double pauserperdie = (lastPMIDieResult.FailPadCount + lastPMIDieResult.PassPadCount) * pmidevparam.PausePercentPerDie.Value / 100;
                                                            if (lastPMIDieResult.FailPadCount >= pauserperdie)
                                                            {
                                                                LoggerManager.Debug($"[NormalPMIProcessModule], DoExecute(),Reserve Pause because DIE remains" +
                                                                        $" DIE Result Failed Count = {lastPMIDieResult.FailPadCount} >= Per Die Setting Count = {pauserperdie:0.00}({pmidevparam.PausePercentPerDie.Value})%");
                                                                pmiDieResult = EventCodeEnum.PMI_RESERVE_PAUSE;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // Nothing
                                                    LoggerManager.Debug($"Nothing. Pause Option = {pmidevparam.PauseMethod.Value}");
                                                }

                                                //mode에 따라 값 validation.
                                                LoggerManager.Debug($"[NormalPMIProcessModule], DoExecute(), Validation Result = {retVal}");
                                                if (retVal != EventCodeEnum.NONE)
                                                {
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                LoggerManager.Error($"[NormalPMIProcessModule], DoExecute(), Error Occurred. MakeAllPadInfo() Return value = {retVal}");
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Error($"[NormalPMIProcessModule], DoExecute(), Error Occurred. DoPMI() Return value = {retVal}");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            retVal = EventCodeEnum.PARAM_INSUFFICIENT;
                            // Set module state as Error state
                            SubModuleState = new SubModuleErrorState(this);
                        }
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }

                if (retVal == EventCodeEnum.NONE || retVal == EventCodeEnum.PMI_PAUSE_IMMEDIATELY || retVal == EventCodeEnum.PMI_PAUSE_AFTER_ALL_DIE_INSPECTION)
                {
                    // Set module state as done state
                    SubModuleState = new SubModuleDoneState(this);
                }
                else
                {
                    // Set module state as Error state
                    SubModuleState = new SubModuleErrorState(this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIProcessModule] [DoExecute()] : {err}");
                LoggerManager.Exception(err);
            }
            finally
            {
                MovingState.Stop();

                this.LotOPModule().MapScreenToLotScreen();

                this.StageSupervisor().StageModuleState.ZCLEARED();
                this.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
            }

            return retVal;
        }

        private EventCodeEnum MakeAllPadInfo(PMIDieResult lastpmidieresult)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                DeviceObject curdie = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[lastpmidieresult.MI.XIndex, lastpmidieresult.MI.YIndex] as DeviceObject;

                if (curdie != null)
                {
                    if (curdie.Pads != null && curdie.Pads.PMIPadInfos.Count > 0)
                    {
                        foreach (var pads in curdie.Pads.PMIPadInfos)
                        {
                            // 패드의 마지막 결과 확인, Pass or Fail

                            bool IsFailed = false;

                            PMIPadResult lastresult = pads.PMIResults.LastOrDefault();

                            if (lastresult != null)
                            {
                                if (lastresult.PadStatus.Count == 1)
                                {
                                    if (lastresult.PadStatus[0] == PadStatusCodeEnum.PASS)
                                    {
                                        // PASS
                                    }
                                    else
                                    {
                                        IsFailed = true;
                                    }
                                }
                                else if (lastresult.PadStatus.Count == 2)
                                {
                                    if ((lastresult.PadStatus[0] == PadStatusCodeEnum.NEED_REFERENCE_IMAGE) &&
                                        (lastresult.PadStatus[1] == PadStatusCodeEnum.PASS))
                                    {
                                        // PASS
                                    }
                                    else
                                    {
                                        IsFailed = true;
                                    }
                                }
                                else
                                {
                                    IsFailed = true;
                                }
                            }
                            else
                            {
                                LoggerManager.Error($"[NormalPMIProcessModule], MakeAllPadInfo(), Error Occurred. All die information is not confirmed. X index = {lastpmidieresult.MI.XIndex}, Y Index = {lastpmidieresult.MI.YIndex} (MI), Return value = {retval}");

                                break;
                            }

                            if (IsFailed == true)
                            {
                                this.PMIModule().DoPMIInfo.AllFailPadCount++;
                            }
                            else
                            {
                                this.PMIModule().DoPMIInfo.AllPassPadCount++;
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[NormalPMIProcessModule], MakeAllPadInfo(), Error Occurred. Return value = {retval}");
                    }
                }
                else
                {
                    LoggerManager.Error($"[NormalPMIProcessModule], MakeAllPadInfo(), Error Occurred. Return value = {retval}");
                }

                LoggerManager.Debug($"[NormalPMIProcessModule], MakeAllPadInfo() : Inspected die count : {this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Count}, " +
                                                            $"Total pad count : {this.PMIModule().DoPMIInfo.AllPassPadCount + this.PMIModule().DoPMIInfo.AllFailPadCount}, " +
                                                            $"Pass : {this.PMIModule().DoPMIInfo.AllPassPadCount}, " +
                                                            $"Fail : {this.PMIModule().DoPMIInfo.AllFailPadCount}, ");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNKNOWN_EXCEPTION;

                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 현재 Parameter Check 및 Init하는 코드
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIProcessModule] [DoClearData()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        /// <summary>
        /// Recovery때 하는 코드
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIProcessModule] [DoRecovery()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        /// <summary>
        /// Recovery 종료할 때 하는 코드
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum DoExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIProcessModule] [DoExitRecovery()] : {err}");
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// SubModule이 Processing 가능한지 판단하는 조건 
        /// </summary>
        /// <returns></returns>
        public bool IsExecute()
        {
            bool retVal = false;

            try
            {
                // ProbingModule의 State가 Suspend일 때, 가능하다.
                if (this.ProbingModule().ProbingStateEnum == EnumProbingState.SUSPENDED || this.ProbingModule().ProbingStateEnum == EnumProbingState.ZDN)
                {
                    if (this.PMIModule().IsTurnOnPMIInLotRun() == true)
                    {
                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                    }
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIProcessModule] [IsExecute()] : {err}");
                LoggerManager.Exception(err);

                retVal = false;
            }

            return retVal;
        }

        /// <summary>
        /// 현재 단계의 Parameter Setting 이 다 되었는지 확인하는 함수.
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum ParamValidation()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum ClearData()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SubModuleState.ClearData();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SubModuleState.Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum Recovery()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SubModuleState.Recovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum ExitRecovery()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SubModuleState.ExitRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SubModuleStateEnum GetState()
        {
            SubModuleStateEnum retval = SubModuleStateEnum.UNDEFINED;

            try
            {
                retval = SubModuleState.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MovingStateEnum GetMovingState()
        {
            MovingStateEnum retval = MovingStateEnum.STOP;

            try
            {
                retval = MovingState.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void ClearState()
        {
            try
            {
                SubModuleState = new SubModuleIdleState(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// PNP 모듈 init 해주는 함수 한번만 호출된다.
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    SubModuleState = new SubModuleIdleState(this);
                    MovingState = new SubModuleStopState(this);

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIProcessModule] [InitModule()] : {err}");
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// PNP 모듈 deinit 해주는 함수
        /// </summary>
        /// <returns></returns>
        public void DeInitModule()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIProcessModule] [DeInitModule()] : {err}");
                LoggerManager.Exception(err);
            }
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }
        #endregion
    }
}
