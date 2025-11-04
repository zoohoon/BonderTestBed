using LogModule;
using NotifyEventModule;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProbingDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Command.Internal
{
    [Serializable]
    public class GOTOSTARTDIE : ProbeCommand, IGoToStartDie
    {
        public override bool Execute()
        {
            bool IsExecute = false;

            try
            {
                IProbingModule Probing = this.ProbingModule();

                IsExecute = SetCommandTo(Probing);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return IsExecute;
        }
    }

    [Serializable]
    public class GoToCenterDie : ProbeCommand, IGoToCenterDie
    {
        public override bool Execute()
        {
            bool IsExecute = false;

            try
            {
                IProbingModule Probing = this.ProbingModule();

                IsExecute = SetCommandTo(Probing);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return IsExecute;
        }
    }

    [Serializable]
    public class ResponseGoToStartDie : ProbeCommand, IResponseGoToStartDie
    {
        public override bool Execute()
        {
            IProbingModule Probing = this.ProbingModule();
            ICommandManager cm = this.CommandManager();

            bool isInjected = true;
            try
            {
                var acknowledgeParam = this.Parameter as AcknowledgeParam;

                string commandName = null;

                if (acknowledgeParam != null)
                {
                    isInjected = false;
                    if (Probing.ProbingStateEnum == EnumProbingState.PINPADMATCHED)
                    {
                        commandName = acknowledgeParam.ACK;
                    }
                    else
                    {
                        commandName = acknowledgeParam.NACK;
                    }
                    isInjected = cm.SetCommand(this, commandName);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return isInjected;
        }
    }

    [Serializable]
    public class ZUPRequest : ProbeCommand, IZUPRequest
    {
        public override bool Execute()
        {
            IProbingModule Probing = this.ProbingModule();
            bool retVal = SetCommandTo(Probing);

            try
            {
                if (retVal == false)
                {
                    LoggerManager.Error($"[ZUPRequest], Execute() : Return false.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class ZUPResponse : ProbeCommand, IZUPResponse
    {
        public override bool Execute()
        {
            IProbingModule Probing = this.ProbingModule();
            ICommandManager cm = this.CommandManager();

            bool isInjected = true;
            try
            {
                var acknowledgeParam = this.Parameter as AcknowledgeParam;

                string commandName = null;

                if (acknowledgeParam != null)
                {
                    isInjected = false;
                    if (Probing.ProbingStateEnum == EnumProbingState.ZUP)
                    {
                        commandName = acknowledgeParam.ACK;
                    }
                    else
                    {
                        commandName = acknowledgeParam.NACK;
                    }
                    isInjected = cm.SetCommand(this, commandName);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return isInjected;
        }
    }

    [Serializable]
    public class ZDownRequest : ProbeCommand, IZDownRequest
    {
        public override bool Execute()
        {
            IProbingModule Probing = this.ProbingModule();

            return SetCommandTo(Probing);
        }
    }

    [Serializable]
    public class ZDownResponse : ProbeCommand, IZDownResponse
    {
        public override bool Execute()
        {
            IProbingModule Probing = this.ProbingModule();
            ICommandManager cm = this.CommandManager();

            bool isInjected = true;
            try
            {
                var acknowledgeParam = this.Parameter as AcknowledgeParam;

                string commandName = null;

                if (acknowledgeParam != null)
                {
                    isInjected = false;
                    if (Probing.ProbingStateEnum == EnumProbingState.ZDN)
                    {
                        commandName = acknowledgeParam.ACK;
                    }
                    else
                    {
                        commandName = acknowledgeParam.NACK;
                    }
                    isInjected = cm.SetCommand(this, commandName);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return isInjected;
        }
    }

    [Serializable]
    public class MoveToNextDie : ProbeCommand, IMoveToNextDie
    {
        public override bool Execute()
        {
            IProbingModule Probing = this.ProbingModule();
            bool retVal = false;

            try
            {
                retVal = SetCommandTo(Probing);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class MoveToDiePosition : ProbeCommand, IMoveToDiePosition
    {
        public override bool Execute()
        {
            bool retVal = false;
            try
            {
                IProbingModule Probing = this.ProbingModule();
                ICommandManager CommandManager = this.CommandManager();
                if (this.Parameter != null)
                {
                    if ((Probing.ProbingStateEnum & EnumProbingState.ZUP) == EnumProbingState.ZUP)
                    {
                        retVal = CommandManager.SetCommand<IMoveToDiePositionAndZUp>(this, this.Parameter);
                    }
                    else
                    {
                        if (Probing.CanExecute(this))
                        {
                            retVal = SetCommandTo(Probing);
                        }
                    }
                }
                else
                {
                    retVal = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }
    }

    [Serializable]
    public class MoveToDiePositionAndZUp : ProbeCommand, IMoveToDiePositionAndZUp
    {
        public override bool Execute()
        {
            bool retVal = false;
            try
            {
                IProbingModule Probing = this.ProbingModule();

                if (this.Parameter != null)
                {
                    retVal = SetCommandTo(Probing);
                }
                else
                {
                    retVal = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }
    }

    [Serializable]
    public class ProbingSequenceResponse : ProbeCommand, IProbingSequenceResponse
    {
        public ProbingSequenceResponse()
        {
        }
        public override bool Execute()
        {
            ICommandManager cm = this.CommandManager();
            IProbingSequenceModule ProbingSeq = this.ProbingSequenceModule();

            bool isInjected = true;
            try
            {
                var acknowledgeParam = this.Parameter as AcknowledgeParam;
                string commandName = null;

                if (acknowledgeParam != null)
                {
                    isInjected = false;
                    if (ProbingSeq.ProbingSequenceRemainCount > 0)
                    {
                        commandName = acknowledgeParam.ACK;
                    }
                    else
                    {
                        commandName = acknowledgeParam.NACK;
                    }
                    isInjected = cm.SetCommand(this, commandName);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return isInjected;
        }
    }

    [Serializable]
    public class UnLoadWafer : ProbeCommand, IUnloadWafer
    {
        public UnLoadWafer()
        {
        }

        public override bool Execute()
        {
            bool bVal = false;
            try
            {
                IProbingModule Probing = this.ProbingModule();
                bVal = SetCommandTo(Probing);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return bVal;
        }
    }

    [Serializable]
    public class ZDownAndPause : ProbeCommand, IZDownAndPause
    {
        public override bool Execute()
        {
            bool bVal = false;
            try
            {
                IProbingModule Probing = this.ProbingModule();
                bVal = SetCommandTo(Probing);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return bVal;
        }
    }

    [Serializable]
    public class ResumeProbing : ProbeCommand, IResumeProbing
    {
        public override bool Execute()
        {
            bool bVal = false;
            try
            {
                IProbingModule Probing = this.ProbingModule();
                bVal = SetCommandTo(Probing);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return bVal;
        }
    }

    [Serializable]
    public class SetBinAnalysisData : ProbeCommand, ISetBinAnalysisData
    {
        public override bool Execute()
        {
            bool retVal = true;
            try
            {
                BinCommandParam binCmdParam = this.Parameter as BinCommandParam;

                if (binCmdParam != null)
                {
                    var binAnalyses = binCmdParam.Param;                    
                    if(binAnalyses != null)
                    {
                        IWaferObject waferObj = this.GetParam_Wafer();
                        ISubstrateInfo waferSubstrateInfo = waferObj.GetSubsInfo();
                        IProbeCard probeCard = this.GetParam_ProbeCard();
                        IProbingModule probingModule = this.ProbingModule();
                        ISubstrateInfo substrateInfo = waferObj.GetSubsInfo();

                        long probingLastXIndex = probingModule.ProbingLastMIndex.XIndex;
                        long probingLastYIndex = probingModule.ProbingLastMIndex.YIndex;

                        int dutCount = probeCard.ProbeCardDevObjectRef.DutList.Count;

                        if (dutCount == binAnalyses.Count)
                        {
                            for (int i = 0; i < dutCount; i++)
                            {
                                //현재 위치해 있는 곳에서 프로빙하고 있는 Dut계산.
                                long dutXIndex = probingLastXIndex + probeCard.ProbeCardDevObjectRef.DutList[i].UserIndex.XIndex;
                                long dutYIndex = probingLastYIndex + probeCard.ProbeCardDevObjectRef.DutList[i].UserIndex.YIndex;

                                bool isDutIsInRange = probingModule.DutIsInRange(dutXIndex, dutYIndex);

                            if (isDutIsInRange)
                            {
                                var die = substrateInfo.DIEs[dutXIndex, dutYIndex];

                                // TODO : 
                                if (this.ProbingModule().IsTestedDIE(dutXIndex, dutYIndex) == true)
                                {
                                    die.State.Value = DieStateEnum.TESTED;
                                }

                                if (die.TestHistory == null)
                                {
                                    die.TestHistory = new List<TestHistory>();
                                }

                                if (die.DieType.Value == DieTypeEnum.TEST_DIE)
                                {
                                    if (this.ProbingModule().IsTestedDIE(dutXIndex, dutYIndex) == true)
                                    {
                                        if (die.TestHistory.Count == 0)
                                        {
                                            die.TestHistory.Add(new TestHistory());
                                            die.CurTestHistory = die.TestHistory.Last();
                                        }

                                        die.CurTestHistory.BinCode.Value = binAnalyses[i].analysis_Data;
                                        die.CurTestHistory.BinData.Value = binAnalyses[i].bin_data;

                                        if (binAnalyses[i].pf_Data == 4)
                                        {
                                            die.CurTestHistory.TestResult.Value = TestState.MAP_STS_PASS;
                                            waferSubstrateInfo.PassedDieCount.Value++;
                                        }
                                        else
                                        {
                                            die.CurTestHistory.TestResult.Value = TestState.MAP_STS_FAIL;
                                            waferSubstrateInfo.FailedDieCount.Value++;
                                        }

                                        probingModule.ProbingProcessStatus.Progress = GetProgressOfProbingProcessStatus();
                                        waferSubstrateInfo.TestedDieCount.Value++;

                                        die.TestHistory.Add(die.CurTestHistory);// #Hynix_Merge: 검토 필요, 이 라인만 V19Merge 쪽 코드 
                                        }
                                    else
                                    {
                                        if (die.TestHistory.Count > 0)
                                        {
                                            var lasthistory = die.TestHistory.Last();

                                            if(lasthistory.TestResult.Value == TestState.MAP_STS_PASS)
                                            {
                                                waferSubstrateInfo.PassedDieCount.Value++;
                                            }
                                            else if (lasthistory.TestResult.Value == TestState.MAP_STS_FAIL)
                                            {
                                                waferSubstrateInfo.FailedDieCount.Value++;
                                            }

                                            // TODO : NEED?
                                            waferSubstrateInfo.TestedDieCount.Value++;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                LoggerManager.Error($"[SetBinAnalysisData], Execute() : The calculated dut position is not within range. X = {dutXIndex}, Y = {dutYIndex}");
                            }
                        }

                            waferSubstrateInfo.UpdateCurrentDieCount();
                            waferSubstrateInfo.UpdateYield();
                        }
                        else
                        {
                            LoggerManager.Error($"[SetBinAnalysisData], Execute() : Data is not matched. Dut count = {probeCard.ProbeCardDevObjectRef.DutList.Count}, Bin analysis count : {binAnalyses.Count}");
                        }
                    }
                    else
                    {
                        this.EventManager().RaisingEvent(typeof(CalculatePfNYieldEvent).FullName);//TODO: MPT 에서는 데이터 못받아도 상관없음, CATANIA 에서는 데이터 받으면 안넘어가야함 나중에 수정되어야함
                        LoggerManager.Error($"[SetBinAnalysisData], Execute() : Data is null");
                    }
                    
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }


        private double GetProgressOfProbingProcessStatus()
        {
            double retProgress = 0.0;
            try
            {
                IProbingSequenceModule probingSequenceModule = this.ProbingSequenceModule();
                int seqCount = probingSequenceModule.ProbingSequenceCount;
                int sequenceRemainCount = probingSequenceModule.ProbingSequenceRemainCount;

                // TODO : 점검 필요? 
                //double progressValue = ((double)(seqCount - (sequenceRemainCount + 1)) / (double)seqCount);
                double progressValue = ((double)(seqCount - (sequenceRemainCount)) / (double)seqCount);

                retProgress = Math.Ceiling(progressValue * 100.0);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retProgress;
        }

        //private static void CalculateTestingYield(ISubstrateInfo waferSubstrateInfo)
        //{
        //    try
        //    {
        //        double passedcount = waferSubstrateInfo.CurPassedDieCount.Value;
        //        double failedcount = waferSubstrateInfo.CurFailedDieCount.Value;
        //        waferSubstrateInfo.Yield = Math.Round((passedcount / (passedcount + failedcount)), 6) * 100;
        //    }
        //    catch (Exception err)
        //    {

        //    }
        //}
    }

}
