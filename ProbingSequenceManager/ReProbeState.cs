using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.BinData;
using ProberInterfaces.Retest;
using ProbingDataInterface;
using RetestObject;
using System;
using System.Reflection;

namespace ProbingSequenceManager
{
    #region CP1

    public abstract class CircuitProbe1OptionBase
    {
        private CircuitProbe1State _CP1Module;

        public CircuitProbe1State CP1Module
        {
            get { return _CP1Module; }
            private set { _CP1Module = value; }
        }
        public CircuitProbe1OptionBase(CircuitProbe1State module)
        {
            CP1Module = module;
        }
        abstract public EventCodeEnum GetNextSequence(ref MachineIndex MI);

    }
    public class CP1NormalState : CircuitProbe1OptionBase
    {
        public CP1NormalState(CircuitProbe1State module) : base(module)
        {

        }

        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            // CP1일때
            //Retest일때나 인스턴트 일때 조건이 필요함 
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                MI = new MachineIndex();
                var wfObj = CP1Module.ProbingSequenceModule.StageSupervisor().WaferObject;

                if (CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount > 0)
                {
                    MI = CP1Module.ProbingSequenceModule.ProbingSeqParameter.ProbingSeq.Value[CP1Module.ProbingSequenceModule.ProbingSequenceCount - CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount];

                    //Instant가 켜져있으면 InstantState로 

                    RetestDeviceParam retestDeviceParam = CP1Module.ProbingSequenceModule.RetestModule().RetestModuleDevParam_IParam as RetestDeviceParam;

                    if (retestDeviceParam.Retest_CP1.InstantRetest.Value == true)
                    {
                        CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1Instant);
                        CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount--;

                        if (CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount == 0)
                        {
                            CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount++;
                            CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1FinalInstantRetest);
                        }
                    }
                    else
                    {
                        CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount--;

                        if (CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount == 0)
                        {
                            CP1Module.ConditionForStateTransitionNoseqOrOnlineStart();
                        }
                    }
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    MI = null;
                    CP1Module.ConditionForStateTransitionNoseqOrOnlineStart();

                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            CP1Module.LogMethod(MethodBase.GetCurrentMethod(), MI);

            return retVal;
        }
    }

    public class CP1InstantState : CircuitProbe1OptionBase
    {
        public CP1InstantState(CircuitProbe1State module) : base(module)
        {

        }

        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //CP1Module.ProbingSequenceModule.Module.ProbingSequenceRemainCount = CP1Module.ProbingSequenceModule.Module.ProbingSequenceCount;
            MI = new MachineIndex();
            var wfobj = CP1Module.ProbingSequenceModule.StageSupervisor().WaferObject.GetSubsInfo();

            //BinSetting이 되어있을때
            //EnableEnum caninstant = EnableEnum.Disable;
            //var bininfos = CP1Module.ProbingSequenceModule.ProbingModule().GetBinInfos();

            try
            {
                if (CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount > 0)
                {
                    int remaincnt = CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount + 1;
                    MI = CP1Module.ProbingSequenceModule.ProbingSeqParameter.ProbingSeq.Value[CP1Module.ProbingSequenceModule.ProbingSequenceCount - remaincnt];

                    //caninstant = bininfos.ContainsKey(wfobj.DIEs[MI.XIndex, MI.YIndex].CurTestHistory.BinCode.Value);

                    //var needRetest = CP1Module.ProbingSequenceModule.ProbingModule().GetRetestEnable(RetestTypeEnum.INSTANTRETESTFORCP1, MI.XIndex, MI.YIndex);

                    var needRetest = CP1Module.ProbingSequenceModule.ProbingModule().NeedRetestbyBIN(RetestTypeEnum.INSTANTRETESTFORCP1, MI.XIndex, MI.YIndex);

                    TestState testState = wfobj.DIEs[MI.XIndex, MI.YIndex].CurTestHistory.TestResult.Value;

                    if (needRetest == true || testState == TestState.MAP_STS_FAIL)
                    {
                        CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1Normal);
                    }
                    else
                    {
                        MI = CP1Module.ProbingSequenceModule.ProbingSeqParameter.ProbingSeq.Value[CP1Module.ProbingSequenceModule.ProbingSequenceCount - CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount];

                        if (CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount == 1)
                        {
                            CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1FinalInstantRetest);
                        }
                        else
                        {
                            CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount--;
                        }
                    }
                }
                else
                {
                    MI = null;
                    CP1Module.ConditionForStateTransitionNoseqOrOnlineStart();
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            CP1Module.LogMethod(MethodBase.GetCurrentMethod(), MI);

            return retVal;
        }
    }

    public class CP1FinalInstantSeqState : CircuitProbe1OptionBase
    {
        public CP1FinalInstantSeqState(CircuitProbe1State module) : base(module)
        {

        }

        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            MI = new MachineIndex();
            var wfobj = CP1Module.ProbingSequenceModule.StageSupervisor().WaferObject.GetSubsInfo();

            //EnableEnum caninstant = EnableEnum.Disable;
            //var bininfos = CP1Module.ProbingSequenceModule.ProbingModule().GetBinInfos();

            try
            {
                if (CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount > 0)
                {
                    int remaincnt = CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount;
                    MI = CP1Module.ProbingSequenceModule.ProbingSeqParameter.ProbingSeq.Value[CP1Module.ProbingSequenceModule.ProbingSequenceCount - remaincnt];

                    //caninstant = bininfos.ContainsKey(wfobj.DIEs[MI.XIndex, MI.YIndex].CurTestHistory.BinCode.Value);

                    //var needRetest = CP1Module.ProbingSequenceModule.ProbingModule().GetRetestEnable(RetestTypeEnum.INSTANTRETESTFORCP1, MI.XIndex, MI.YIndex);
                    var needRetest = CP1Module.ProbingSequenceModule.ProbingModule().NeedRetestbyBIN(RetestTypeEnum.INSTANTRETESTFORCP1, MI.XIndex, MI.YIndex);

                    TestState testState = wfobj.DIEs[MI.XIndex, MI.YIndex].CurTestHistory.TestResult.Value;

                    if (needRetest == true || testState == TestState.MAP_STS_FAIL)
                    {
                        MI = CP1Module.ProbingSequenceModule.ProbingSeqParameter.ProbingSeq.Value[CP1Module.ProbingSequenceModule.ProbingSequenceCount - remaincnt];
                    }
                    else
                    {
                        MI = null;
                    }

                    CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount--;
                    CP1Module.ConditionForStateTransitionNoseqOrOnlineStart();
                }
                else
                {
                    MI = null;
                    CP1Module.ConditionForStateTransitionNoseqOrOnlineStart();
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            CP1Module.LogMethod(MethodBase.GetCurrentMethod(), MI);

            return retVal;
        }
    }

    public class CP1OnlineRetestStartState : CircuitProbe1OptionBase
    {
        public CP1OnlineRetestStartState(CircuitProbe1State module) : base(module)
        {

        }
        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                MI = new MachineIndex();
                MachineIndex onlineretestMI = new MachineIndex();
                var wfObj = CP1Module.ProbingSequenceModule.StageSupervisor().WaferObject;

                //bool caninstant = false;
                //var bininfos = CP1Module.ProbingSequenceModule.ProbingModule().GetBinInfos();

                CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.Clear();

                for (int i = 0; i < CP1Module.ProbingSequenceModule.ProbingSequenceCount; i++)
                {
                    onlineretestMI = CP1Module.ProbingSequenceModule.ProbingSeqParameter.ProbingSeq.Value[i];

                    ReTestMode reTestMode = CP1Module.ProbingSequenceModule.OnlineRetestList[0].Mode.Value;

                    if (reTestMode == ReTestMode.ALL)
                    {
                        if (CP1Module.ProbingSequenceModule.ProbingModule().NeedRetest(onlineretestMI.XIndex, onlineretestMI.YIndex) == true)
                        {
                            CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.Add(onlineretestMI);
                        }

                        //var deviceObject = wfObj.GetSubsInfo().DIEs[onlineretestMI.XIndex, onlineretestMI.YIndex];

                        //DieStateEnum dieStateEnum = deviceObject.State.Value;
                        //TestState testState = deviceObject.CurTestHistory.TestResult.Value;

                        //if (dieStateEnum == DieStateEnum.TESTED && testState == TestState.MAP_STS_FAIL)
                        //{
                        //    CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.Add(onlineretestMI);
                        //}
                    }
                    else
                    {
                        //var needRetest = CP1Module.ProbingSequenceModule.ProbingModule().GetRetestEnable(RetestTypeEnum.RETESTFORCP1, onlineretestMI.XIndex, onlineretestMI.YIndex);
                        var needRetest = CP1Module.ProbingSequenceModule.ProbingModule().NeedRetestbyBIN(RetestTypeEnum.RETESTFORCP1, onlineretestMI.XIndex, onlineretestMI.YIndex);

                        if (needRetest == true)
                        {
                            CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.Add(onlineretestMI);
                        }
                    }
                }

                CP1Module.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount = CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.Count;

                if (CP1Module.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount > 0)
                {
                    MI = CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq[0];

                    if (CP1Module.ProbingSequenceModule.OnlineRetestList[0].InstantRetest.Value == true)
                    {
                        if (CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.Count == 1)
                        {
                            CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1FinalOnlineInstant);
                        }
                        else
                        {
                            CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1OnlineInstant);
                        }
                    }
                    else
                    {
                        CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1OnlineRetest);
                        CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.RemoveAt(0);
                        CP1Module.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount = CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.Count;
                    }
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    MI = null;
                    CP1Module.ConditionForStateTrainsitionNoseqOrOnlineStart2(ref MI);
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            CP1Module.LogMethod(MethodBase.GetCurrentMethod(), MI);

            return retVal;

        }
    }

    public class CP1OnlineRetestState : CircuitProbe1OptionBase
    {
        public CP1OnlineRetestState(CircuitProbe1State module) : base(module)
        {

        }
        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                MI = new MachineIndex();
                var wfObj = CP1Module.ProbingSequenceModule.StageSupervisor().WaferObject;
                if (CP1Module.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount > 0)
                {
                    MI = CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq[0];

                    if (CP1Module.ProbingSequenceModule.OnlineRetestList[0].InstantRetest.Value == true)
                    {
                        if (CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.Count == 1)
                        {
                            CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1FinalOnlineInstant);
                        }
                        else
                        {
                            CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1OnlineInstant);
                        }
                    }
                    else
                    {
                        CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.RemoveAt(0);
                        CP1Module.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount--;
                    }

                    if (CP1Module.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount == 0)
                    {
                        CP1Module.ConditionForStateTrainsitionNoseqOrOnlineStart2(ref MI);
                    }
                    else
                    {
                    }
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    MI = null;
                    CP1Module.ConditionForStateTrainsitionNoseqOrOnlineStart2(ref MI);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }
            CP1Module.LogMethod(MethodBase.GetCurrentMethod(), MI);
            return retVal;

        }
    }

    public class CP1InstantOnlineRetestState : CircuitProbe1OptionBase
    {
        public CP1InstantOnlineRetestState(CircuitProbe1State module) : base(module)
        {

        }
        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            MI = new MachineIndex();
            var wfobj = CP1Module.ProbingSequenceModule.StageSupervisor().WaferObject.GetSubsInfo();

            //BinSetting이 되어있을때
            //bool caninstant = false;
            //var bininfos = CP1Module.ProbingSequenceModule.ProbingModule().GetBinInfos();

            try
            {
                if (CP1Module.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount > 0)
                {
                    MI = CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq[0];

                    //caninstant = bininfos.ContainsKey(wfobj.DIEs[MI.XIndex, MI.YIndex].CurTestHistory.BinCode.Value);

                    //var needRetest = CP1Module.ProbingSequenceModule.ProbingModule().GetRetestEnable(RetestTypeEnum.INSTANTRETESTFORCP1, MI.XIndex, MI.YIndex);
                    var needRetest = CP1Module.ProbingSequenceModule.ProbingModule().NeedRetestbyBIN(RetestTypeEnum.INSTANTRETESTFORCP1, MI.XIndex, MI.YIndex);

                    TestState testState = wfobj.DIEs[MI.XIndex, MI.YIndex].CurTestHistory.TestResult.Value;

                    if (needRetest == true || testState == TestState.MAP_STS_FAIL)
                    {
                        MI = CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq[0];

                        CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.RemoveAt(0);
                        CP1Module.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount--;

                        CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1OnlineRetest);
                    }
                    else
                    {
                        CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.RemoveAt(0);
                        CP1Module.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount--;

                        MI = CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq[0];

                        if (CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.Count == 1)
                        {
                            CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1FinalOnlineInstant);
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    MI = null;
                    CP1Module.ConditionForStateTrainsitionNoseqOrOnlineStart2(ref MI);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            CP1Module.LogMethod(MethodBase.GetCurrentMethod(), MI);

            return retVal;

        }
    }

    public class CP1FinalOnlineInstantSeqState : CircuitProbe1OptionBase
    {
        public CP1FinalOnlineInstantSeqState(CircuitProbe1State module) : base(module)
        {

        }
        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            MI = new MachineIndex();
            var wfobj = CP1Module.ProbingSequenceModule.StageSupervisor().WaferObject.GetSubsInfo();

            //bool caninstant = false;
            //var bininfos = CP1Module.ProbingSequenceModule.ProbingModule().GetBinInfos();

            try
            {
                if (CP1Module.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount > 0)
                {
                    MI = CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq[0];

                    //caninstant = bininfos.ContainsKey(wfobj.DIEs[MI.XIndex, MI.YIndex].CurTestHistory.BinCode.Value);

                    //var needRetest = CP1Module.ProbingSequenceModule.ProbingModule().GetRetestEnable(RetestTypeEnum.INSTANTRETESTFORCP1, MI.XIndex, MI.YIndex);
                    var needRetest = CP1Module.ProbingSequenceModule.ProbingModule().NeedRetestbyBIN(RetestTypeEnum.INSTANTRETESTFORCP1, MI.XIndex, MI.YIndex);

                    TestState testState = wfobj.DIEs[MI.XIndex, MI.YIndex].CurTestHistory.TestResult.Value;

                    if (needRetest == true || testState == TestState.MAP_STS_FAIL)
                    {

                        MI = CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq[0];
                        CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.RemoveAt(0);
                        CP1Module.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount--;
                    }
                    else
                    {
                        MI = null;
                        CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.RemoveAt(0);
                        CP1Module.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount--;
                    }

                    if (CP1Module.ProbingSequenceModule.CP1OnlineRetestSeq.Count <= 0)
                    {
                        CP1Module.ConditionForStateTrainsitionNoseqOrOnlineStart2(ref MI);
                    }
                }
                else
                {
                    MI = null;
                    CP1Module.ConditionForStateTrainsitionNoseqOrOnlineStart2(ref MI);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            CP1Module.LogMethod(MethodBase.GetCurrentMethod(), MI);

            return retVal;

        }
    }

    public class CP1DoneState : CircuitProbe1OptionBase
    {
        public CP1DoneState(CircuitProbe1State module) : base(module)
        {

        }
        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            MI = new MachineIndex();

            try
            {
                MI = null;
                CP1Module.ProbingSequenceModule.ProbingSequenceStateTransition(new ProbingSequenceNMSRState(CP1Module.ProbingSequenceModule));

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            CP1Module.LogMethod(MethodBase.GetCurrentMethod(), MI);

            return retVal;
        }
    }

    public class ContinueStartState : CircuitProbe1OptionBase
    {
        public ContinueStartState(CircuitProbe1State module) : base(module)
        {

        }
        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //CP1Module.ProbingSequenceModule.Module.ProbingSequenceRemainCount = CP1Module.ProbingSequenceModule.Module.ProbingSequenceCount;
                MI = new MachineIndex();

                MachineIndex continueretestMI = new MachineIndex();
                var wfObj = CP1Module.ProbingSequenceModule.StageSupervisor().WaferObject;

                //BinSetting이 되어있을때
                //bool caninstant = false;
                //var bininfos = CP1Module.ProbingSequenceModule.ProbingModule().GetBinInfos();

                CP1Module.ProbingSequenceModule.MPPRetestSeq.Clear();
                CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount = 0;

                int testedcount = 0;

                for (int i = 0; i < CP1Module.ProbingSequenceModule.ProbingSequenceCount; i++)
                {
                    continueretestMI = CP1Module.ProbingSequenceModule.ProbingSeqParameter.ProbingSeq.Value[i];

                    var deviceObject = wfObj.GetSubsInfo().DIEs[continueretestMI.XIndex, continueretestMI.YIndex];

                    DieStateEnum dieStateEnum = deviceObject.State.Value;

                    if (dieStateEnum == DieStateEnum.TESTED)
                    {
                        testedcount++;
                    }
                }

                CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount = CP1Module.ProbingSequenceModule.ProbingSequenceCount - testedcount;

                if (CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount > 0)
                {
                    MI = CP1Module.ProbingSequenceModule.ProbingSeqParameter.ProbingSeq.Value[CP1Module.ProbingSequenceModule.ProbingSequenceCount - CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount];

                    RetestDeviceParam retestDeviceParam = CP1Module.ProbingSequenceModule.RetestModule().RetestModuleDevParam_IParam as RetestDeviceParam;

                    if (retestDeviceParam.Retest_CP1.InstantRetest.Value == true)
                    {
                        CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1Instant);
                        CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount--;

                        if (CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount == 0)
                        {
                            CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount++;
                            CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1FinalInstantRetest);
                        }
                    }
                    else
                    {
                        CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount--;

                        if (CP1Module.ProbingSequenceModule.ProbingSequenceRemainCount == 0)
                        {
                            CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1Done);
                            CP1Module.ProbingSequenceModule.ProbingSequenceStateTransition(new ProbingSequenceNMSRState(CP1Module.ProbingSequenceModule));
                        }
                        else
                        {
                            CP1Module.SetProbingCP1InnerState(EnumProbingInnerState.CP1Normal);
                        }

                    }
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    MI = null;
                    CP1Module.ProbingSequenceModule.ProbingSequenceStateTransition(new ProbingSequenceNMSRState(CP1Module.ProbingSequenceModule));
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            CP1Module.LogMethod(MethodBase.GetCurrentMethod(), MI);

            return retVal;

        }
    }

    #endregion

    #region MPP
    public abstract class MultiPassProbeOptionBase
    {
        private MultiPassProbeState _MPPModule;

        public MultiPassProbeState MPPModule
        {
            get { return _MPPModule; }
            private set { _MPPModule = value; }
        }
        public MultiPassProbeOptionBase(MultiPassProbeState module)
        {
            MPPModule = module;
        }
        abstract public EventCodeEnum GetNextSequence(ref MachineIndex MI);

        public EventCodeEnum LogMethod(MethodBase caller, MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                ProbingSequenceModule sequenceModule = this.MPPModule.ProbingSequenceModule;

                if (MI != null)
                {
                    LoggerManager.Debug($"[ReProbe] State: {caller.DeclaringType} XIndex = {MI.XIndex}, YIndex = {MI.YIndex} " +
                        $" ProbingSequenceRemainCount : {sequenceModule.ProbingSequenceRemainCount}" +
                        $" OnlineRetestListCount : {sequenceModule.OnlineRetestList.Count}" +
                        $" ProbingCP1OnlineRetestRemainCount : {sequenceModule.ProbingCP1OnlineRetestRemainCount}");
                }
                else
                {
                    LoggerManager.Debug($"[ReProbe] State: {caller.DeclaringType} MI is NULL " +
                        $" ProbingSequenceRemainCount : {sequenceModule.ProbingSequenceRemainCount}" +
                        $" OnlineRetestListCount : {sequenceModule.OnlineRetestList.Count}" +
                        $" ProbingCP1OnlineRetestRemainCount : {sequenceModule.ProbingCP1OnlineRetestRemainCount}");
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

    }
    public class MPPNormalStartState : MultiPassProbeOptionBase
    {
        public MPPNormalStartState(MultiPassProbeState module) : base(module)
        {

        }
        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //CP1Module.ProbingSequenceModule.Module.ProbingSequenceRemainCount = CP1Module.ProbingSequenceModule.Module.ProbingSequenceCount;
                MI = new MachineIndex();
                MachineIndex mppretestMI = new MachineIndex();
                var wfObj = MPPModule.ProbingSequenceModule.StageSupervisor().WaferObject;

                //BinSetting이 되어있을때
                //bool caninstant = false;
                //var bininfos = MPPModule.ProbingSequenceModule.ProbingModule().GetBinInfos();

                MPPModule.ProbingSequenceModule.MPPRetestSeq.Clear();
                MPPModule.ProbingSequenceModule.ProbingSequenceRemainCount = 0;

                for (int i = 0; i < MPPModule.ProbingSequenceModule.ProbingSequenceCount; i++)
                {
                    mppretestMI = MPPModule.ProbingSequenceModule.ProbingSeqParameter.ProbingSeq.Value[i];

                    RetestDeviceParam retestDeviceParam = MPPModule.ProbingSequenceModule.RetestModule().RetestModuleDevParam_IParam as RetestDeviceParam;

                    ReTestMode reTestMode = retestDeviceParam.Retest_MPP.Mode.Value;

                    if (reTestMode == ReTestMode.ALL)
                    {
                        if(MPPModule.ProbingSequenceModule.ProbingModule().NeedRetest(mppretestMI.XIndex, mppretestMI.YIndex) == true)
                        {
                            MPPModule.ProbingSequenceModule.MPPRetestSeq.Add(mppretestMI);
                        }

                        //var deviceObject = wfObj.GetSubsInfo().DIEs[mppretestMI.XIndex, mppretestMI.YIndex];

                        //DieStateEnum dieStateEnum = deviceObject.State.Value;
                        //TestState testState = deviceObject.CurTestHistory.TestResult.Value;

                        //if (dieStateEnum == DieStateEnum.TESTED && testState == TestState.MAP_STS_FAIL)
                        //{
                        //    MPPModule.ProbingSequenceModule.MPPRetestSeq.Add(mppretestMI);
                        //}
                    }
                    else
                    {
                        //var needRetest = MPPModule.ProbingSequenceModule.ProbingModule().GetRetestEnable(RetestTypeEnum.RETESTFORCP2, mppretestMI.XIndex, mppretestMI.YIndex);
                        var needRetest = MPPModule.ProbingSequenceModule.ProbingModule().NeedRetestbyBIN(RetestTypeEnum.RETESTFORCP2, mppretestMI.XIndex, mppretestMI.YIndex);

                        if (needRetest == true)
                        {
                            MPPModule.ProbingSequenceModule.MPPRetestSeq.Add(mppretestMI);
                        }
                    }
                }

                MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount = MPPModule.ProbingSequenceModule.MPPRetestSeq.Count;

                if (MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount > 0)
                {
                    MI = MPPModule.ProbingSequenceModule.MPPRetestSeq[0];

                    RetestDeviceParam retestDeviceParam = MPPModule.ProbingSequenceModule.RetestModule().RetestModuleDevParam_IParam as RetestDeviceParam;

                    if (retestDeviceParam.Retest_MPP.InstantRetest.Value == true)
                    {
                        MPPModule.SetProbingMPPInnerState(EnumProbingInnerState.MPPInstant);

                        if (MPPModule.ProbingSequenceModule.MPPRetestSeq.Count == 1)
                        {
                            MPPModule.SetProbingMPPInnerState(EnumProbingInnerState.MPPFinalInstant);
                        }
                    }
                    else
                    {
                        MPPModule.SetProbingMPPInnerState(EnumProbingInnerState.MPPNormal);
                        MPPModule.ProbingSequenceModule.MPPRetestSeq.RemoveAt(0);
                        MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount = MPPModule.ProbingSequenceModule.MPPRetestSeq.Count;
                    }

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    MI = null;

                    MPPModule.SetProbingMPPInnerState(EnumProbingInnerState.MPPDone);
                    MPPModule.ProbingSequenceModule.ProbingSequenceStateTransition(new ProbingSequenceNMSRState(MPPModule.ProbingSequenceModule));

                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                MI = null;
                retVal = EventCodeEnum.UNDEFINED;
            }

            this.LogMethod(MethodBase.GetCurrentMethod(), MI);

            //if (MI != null)
            //{
            //    LoggerManager.Debug($"[Reprobe] State: {MethodBase.GetCurrentMethod().DeclaringType} XIndex{MI.XIndex} YIndex{MI.YIndex} " +
            //        $"ProbingMPPRetestRemainCount : {MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount}");
            //}
            //else
            //{
            //    LoggerManager.Debug($"[Reprobe] State: {MethodBase.GetCurrentMethod().DeclaringType} MI is NULL " +
            //        $"ProbingMPPRetestRemainCount : {MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount}");
            //}

            return retVal;

        }
    }

    public class MPPNormalState : MultiPassProbeOptionBase
    {
        public MPPNormalState(MultiPassProbeState module) : base(module)
        {

        }
        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                MI = new MachineIndex();

                MachineIndex onlineretestMI = new MachineIndex();

                //var wfObj = MPPModule.ProbingSequenceModule.StageSupervisor().WaferObject;

                //BinSetting이 되어있을때
                //bool caninstant = false;
                //var bininfos = MPPModule.ProbingSequenceModule.ProbingModule().GetBinInfos();

                if (MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount > 0)
                {
                    MI = MPPModule.ProbingSequenceModule.MPPRetestSeq[0];

                    RetestDeviceParam retestDeviceParam = MPPModule.ProbingSequenceModule.RetestModule().RetestModuleDevParam_IParam as RetestDeviceParam;

                    if (retestDeviceParam.Retest_MPP.InstantRetest.Value == true)
                    {
                        MPPModule.SetProbingMPPInnerState(EnumProbingInnerState.MPPInstant);

                        if (MPPModule.ProbingSequenceModule.MPPRetestSeq.Count == 1)
                        {
                            MPPModule.SetProbingMPPInnerState(EnumProbingInnerState.MPPFinalInstant);
                        }
                    }
                    else
                    {
                        MPPModule.ProbingSequenceModule.MPPRetestSeq.RemoveAt(0);
                        MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount = MPPModule.ProbingSequenceModule.MPPRetestSeq.Count;
                    }

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    MI = null;

                    MPPModule.SetProbingMPPInnerState(EnumProbingInnerState.MPPDone);
                    MPPModule.ProbingSequenceModule.ProbingSequenceStateTransition(new ProbingSequenceNMSRState(MPPModule.ProbingSequenceModule));

                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            if (MI != null)
            {
                LoggerManager.Debug($"[Reprobe] State: {MethodBase.GetCurrentMethod().DeclaringType} XIndex{MI.XIndex} YIndex{MI.YIndex} " +
                    $"ProbingMPPRetestRemainCount : {MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount}");
            }
            else
            {
                LoggerManager.Debug($"[Reprobe] State: {MethodBase.GetCurrentMethod().DeclaringType} MI is NULL " +
                    $"ProbingMPPRetestRemainCount : {MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount}");
            }

            return retVal;

        }
    }

    public class MPPInstantRetestState : MultiPassProbeOptionBase
    {
        public MPPInstantRetestState(MultiPassProbeState module) : base(module)
        {

        }
        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            MI = new MachineIndex();

            var wfobj = MPPModule.ProbingSequenceModule.StageSupervisor().WaferObject.GetSubsInfo();

            //BinSetting이 되어있을때
            //bool caninstant = false;
            //var bininfos = MPPModule.ProbingSequenceModule.ProbingModule().GetBinInfos();

            try
            {
                if (MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount > 0)
                {
                    MI = MPPModule.ProbingSequenceModule.MPPRetestSeq[0];

                    //caninstant = bininfos.ContainsKey(wfobj.DIEs[MI.XIndex, MI.YIndex].CurTestHistory.BinCode.Value);

                    //var needRetest = MPPModule.ProbingSequenceModule.ProbingModule().GetRetestEnable(RetestTypeEnum.RETESTFORCP2, MI.XIndex, MI.YIndex);
                    var needRetest = MPPModule.ProbingSequenceModule.ProbingModule().NeedRetestbyBIN(RetestTypeEnum.RETESTFORCP2, MI.XIndex, MI.YIndex);

                    TestState testState = wfobj.DIEs[MI.XIndex, MI.YIndex].CurTestHistory.TestResult.Value;

                    if (needRetest == true || testState == TestState.MAP_STS_FAIL)
                    {
                        MI = MPPModule.ProbingSequenceModule.MPPRetestSeq[0];

                        MPPModule.ProbingSequenceModule.MPPRetestSeq.RemoveAt(0);
                        MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount--;
                        MPPModule.SetProbingMPPInnerState(EnumProbingInnerState.MPPNormal);
                    }
                    else
                    {
                        MPPModule.ProbingSequenceModule.MPPRetestSeq.RemoveAt(0);
                        MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount--;

                        if (MPPModule.ProbingSequenceModule.MPPRetestSeq.Count <= 0)
                        {
                            MPPModule.SetProbingMPPInnerState(EnumProbingInnerState.MPPDone);
                        }
                        else
                        {
                            MI = MPPModule.ProbingSequenceModule.MPPRetestSeq[0];

                            if (MPPModule.ProbingSequenceModule.MPPRetestSeq.Count == 1)
                            {
                                MPPModule.SetProbingMPPInnerState(EnumProbingInnerState.MPPFinalInstant);
                            }
                            else
                            {
                                //CP1Module.SetProbingCP1InnerState(EnumCP1InnerState.OnlineRetest);
                            }
                        }

                    }
                }
                else
                {
                    MI = null;

                    MPPModule.SetProbingMPPInnerState(EnumProbingInnerState.MPPDone);
                    MPPModule.ProbingSequenceModule.ProbingSequenceStateTransition(new ProbingSequenceNMSRState(MPPModule.ProbingSequenceModule));

                    retVal = EventCodeEnum.NONE;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            if (MI != null)
            {
                LoggerManager.Debug($"[Reprobe] State: {MethodBase.GetCurrentMethod().DeclaringType} XIndex{MI.XIndex} YIndex{MI.YIndex} " +
                    $"ProbingMPPRetestRemainCount : {MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount}");
            }
            else
            {
                LoggerManager.Debug($"[Reprobe] State: {MethodBase.GetCurrentMethod().DeclaringType} MI is NULL " +
                    $"ProbingMPPRetestRemainCount : {MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount}");
            }

            return retVal;
        }
    }

    public class MPPFinalInstantState : MultiPassProbeOptionBase
    {
        public MPPFinalInstantState(MultiPassProbeState module) : base(module)
        {

        }
        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            MI = new MachineIndex();

            var wfobj = MPPModule.ProbingSequenceModule.StageSupervisor().WaferObject.GetSubsInfo();

            //BinSetting이 되어있을때
            //bool caninstant = false;
            //var bininfos = MPPModule.ProbingSequenceModule.ProbingModule().GetBinInfos();

            try
            {
                if (MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount > 0)
                {
                    MI = MPPModule.ProbingSequenceModule.MPPRetestSeq[0];

                    //caninstant = bininfos.ContainsKey(wfobj.DIEs[MI.XIndex, MI.YIndex].CurTestHistory.BinCode.Value);
                    //var needRetest = MPPModule.ProbingSequenceModule.ProbingModule().GetRetestEnable(RetestTypeEnum.RETESTFORCP2, MI.XIndex, MI.YIndex);
                    var needRetest = MPPModule.ProbingSequenceModule.ProbingModule().NeedRetestbyBIN(RetestTypeEnum.RETESTFORCP2, MI.XIndex, MI.YIndex);

                    TestState testState = wfobj.DIEs[MI.XIndex, MI.YIndex].CurTestHistory.TestResult.Value;

                    if (needRetest == true || testState == TestState.MAP_STS_FAIL)
                    {
                        MI = MPPModule.ProbingSequenceModule.MPPRetestSeq[0];

                        MPPModule.ProbingSequenceModule.MPPRetestSeq.RemoveAt(0);
                        MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount--;
                    }
                    else
                    {
                        MPPModule.ProbingSequenceModule.MPPRetestSeq.RemoveAt(0);
                        MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount--;
                    }

                    if (MPPModule.ProbingSequenceModule.CP1OnlineRetestSeq.Count <= 0)
                    {
                        MPPModule.SetProbingMPPInnerState(EnumProbingInnerState.MPPDone);
                    }
                }
                else
                {
                    MI = null;

                    MPPModule.SetProbingMPPInnerState(EnumProbingInnerState.MPPDone);
                    MPPModule.ProbingSequenceModule.ProbingSequenceStateTransition(new ProbingSequenceNMSRState(MPPModule.ProbingSequenceModule));

                    retVal = EventCodeEnum.NONE;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            if (MI != null)
            {
                LoggerManager.Debug($"[Reprobe] State: {MethodBase.GetCurrentMethod().DeclaringType} XIndex{MI.XIndex} YIndex{MI.YIndex} " +
                    $"ProbingMPPRetestRemainCount : {MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount}");
            }
            else
            {
                LoggerManager.Debug($"[Reprobe] State: {MethodBase.GetCurrentMethod().DeclaringType} MI is NULL " +
                    $"ProbingMPPRetestRemainCount : {MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount}");
            }

            return retVal;

        }
    }

    public class MPPDoneState : MultiPassProbeOptionBase
    {
        public MPPDoneState(MultiPassProbeState module) : base(module)
        {

        }

        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            MI = new MachineIndex();

            var wfobj = MPPModule.ProbingSequenceModule.StageSupervisor().WaferObject.GetSubsInfo();

            bool IstestresultFail = true;

            // TODO : canInstant => ???
            //BinSetting이 되어있을때
            bool canInstant = true;
            //var bininfos = MPPModule.ProbingSequenceModule.ProbingModule().GetBinInfos();

            try
            {
                if (MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount > 0)
                {
                    MI = MPPModule.ProbingSequenceModule.MPPRetestSeq[0];
                    var dies = MPPModule.ProbingSequenceModule.GetUnderDutDices(MI);

                    foreach (var mi in dies)
                    {
                        //var needRetest = MPPModule.ProbingSequenceModule.ProbingModule().GetRetestEnable(RetestTypeEnum.RETESTFORCP2, mi.DieIndex.XIndex, mi.DieIndex.YIndex);
                        var needRetest = MPPModule.ProbingSequenceModule.ProbingModule().NeedRetestbyBIN(RetestTypeEnum.RETESTFORCP2, mi.DieIndex.XIndex, mi.DieIndex.YIndex);

                        //bool tmpinstant = bininfos.ContainsKey(wfobj.DIEs[mi.DieIndex.XIndex, mi.DieIndex.YIndex].CurTestHistory.BinCode.Value);

                        TestState testState = wfobj.DIEs[mi.DieIndex.XIndex, mi.DieIndex.YIndex].CurTestHistory.TestResult.Value;

                        bool temptestresult = testState == TestState.MAP_STS_FAIL;

                        canInstant = (canInstant && needRetest) ? true : false;
                        IstestresultFail = (IstestresultFail && temptestresult) ? true : false;
                    }

                    if (canInstant || IstestresultFail)
                    {
                        MI = MPPModule.ProbingSequenceModule.MPPRetestSeq[0];

                        MPPModule.ProbingSequenceModule.MPPRetestSeq.RemoveAt(0);
                        MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount--;
                    }
                    else
                    {
                        MPPModule.ProbingSequenceModule.MPPRetestSeq.RemoveAt(0);
                        MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount--;
                    }

                    if (MPPModule.ProbingSequenceModule.CP1OnlineRetestSeq.Count <= 0)
                    {
                        MPPModule.SetProbingMPPInnerState(EnumProbingInnerState.MPPDone);
                    }
                }
                else
                {
                    MI = null;
                    MPPModule.ProbingSequenceModule.ProbingSequenceStateTransition(new ProbingSequenceNMSRState(MPPModule.ProbingSequenceModule));
                    retVal = EventCodeEnum.NONE;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            if (MI != null)
            {
                LoggerManager.Debug($"[Reprobe] State: {MethodBase.GetCurrentMethod().DeclaringType} XIndex{MI.XIndex} YIndex{MI.YIndex} " +
                    $"ProbingMPPRetestRemainCount : {MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount}");
            }
            else
            {
                LoggerManager.Debug($"[Reprobe] State: {MethodBase.GetCurrentMethod().DeclaringType} MI is NULL " +
                    $"ProbingMPPRetestRemainCount : {MPPModule.ProbingSequenceModule.ProbingMPPRetestRemainCount}");
            }

            return retVal;

        }
    }


    #endregion

    public class CircuitProbe1State : ProbingSequenceInnerStateBase
    {
        public CircuitProbe1State(ProbingSequenceModule module) : base(module)
        {
            //ProbingSequenceModule = module;
            //CP1InnerState = new CP1NormalState(this);
            //if(this.ProbingSequenceModule.LotOPModule().LotInfo.LotMode == LotModeEnum.CP1)
            //{
            //    CP1InnerState = new CP1NormalState(this);
            //    CP1InnerState = new ContinueStartState(this);
            //}
            //else if (this.ProbingSequenceModule.LotOPModule().LotInfo.LotMode == LotModeEnum.CONTINUEPROBING)
            //{
            //    CP1InnerState = new ContinueStartState(this);
            //}
            CP1InnerState = new ContinueStartState(this);

        }

        private CircuitProbe1OptionBase _CP1InnerState;
        public CircuitProbe1OptionBase CP1InnerState
        {
            get { return _CP1InnerState; }
            set
            {
                _CP1InnerState = value;
            }
        }

        public override LotModeEnum GetState()
        {
            return LotModeEnum.CP1;
        }

        public EventCodeEnum SetProbingCP1InnerState(EnumProbingInnerState state)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            var prevstate = CP1InnerState.GetType();

            try
            {

                switch (state)
                {
                    case EnumProbingInnerState.CP1Normal:
                        CP1InnerState = new CP1NormalState(this);
                        PrevCP1InnerState = EnumProbingInnerState.CP1Normal;
                        break;
                    case EnumProbingInnerState.CP1Instant:
                        CP1InnerState = new CP1InstantState(this);
                        PrevCP1InnerState = EnumProbingInnerState.CP1Instant;
                        break;
                    case EnumProbingInnerState.CP1FinalInstantRetest:
                        CP1InnerState = new CP1FinalInstantSeqState(this);
                        PrevCP1InnerState = EnumProbingInnerState.CP1Instant;
                        break;
                    case EnumProbingInnerState.CP1OnlineRetestStart:
                        CP1InnerState = new CP1OnlineRetestStartState(this);
                        PrevCP1InnerState = EnumProbingInnerState.CP1Instant;
                        break;
                    case EnumProbingInnerState.CP1OnlineRetest:
                        CP1InnerState = new CP1OnlineRetestState(this);
                        PrevCP1InnerState = EnumProbingInnerState.CP1OnlineRetest;
                        break;
                    case EnumProbingInnerState.CP1OnlineInstant:
                        CP1InnerState = new CP1InstantOnlineRetestState(this);
                        PrevCP1InnerState = EnumProbingInnerState.CP1OnlineInstant;
                        break;
                    case EnumProbingInnerState.CP1FinalOnlineInstant:
                        CP1InnerState = new CP1FinalOnlineInstantSeqState(this);
                        PrevCP1InnerState = EnumProbingInnerState.CP1FinalOnlineInstant;
                        break;
                    case EnumProbingInnerState.CP1Done:
                        CP1InnerState = new CP1DoneState(this);
                        PrevCP1InnerState = EnumProbingInnerState.CP1Done;
                        break;
                    case EnumProbingInnerState.ContinueStart:
                        CP1InnerState = new ContinueStartState(this);
                        PrevCP1InnerState = EnumProbingInnerState.ContinueStart;
                        break;
                    default:
                        break;
                }

                LoggerManager.Debug($"[ReProbe] State Transition {prevstate} To {CP1InnerState.GetType()} ");

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retval = EventCodeEnum.UNDEFINED;
            }

            return retval;
        }

        public EventCodeEnum SetTransitionToNoSeqState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                SetProbingCP1InnerState(EnumProbingInnerState.CP1Done);
                this.ProbingSequenceModule.ProbingSequenceStateTransition(new ProbingSequenceNMSRState(this.ProbingSequenceModule));

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        public EventCodeEnum ConditionForStateTransitionNoseqOrOnlineStart()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.ProbingSequenceModule.OnlineRetestList.Count > 0)
                {
                    this.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount = 1;
                    this.SetProbingCP1InnerState(EnumProbingInnerState.CP1OnlineRetestStart);
                }
                else
                {
                    this.SetTransitionToNoSeqState();
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }
        public EventCodeEnum ConditionForStateTrainsitionNoseqOrOnlineStart2(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.ProbingSequenceModule.OnlineRetestList.Count > 0)
                {
                    this.ProbingSequenceModule.OnlineRetestList.RemoveAt(0);

                    if (this.ProbingSequenceModule.OnlineRetestList.Count == 0)
                    {
                        this.SetTransitionToNoSeqState();
                    }
                    else
                    {
                        OnlineRetestGetNextSequnce(ref MI);
                        //this.SetProbingCP1InnerState(EnumProbingInnerState.CP1OnlineRetestStart);
                        //this.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount = 1;
                    }
                }
                else
                {
                    this.SetTransitionToNoSeqState();
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        public EventCodeEnum LogMethod(MethodBase caller, MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                ProbingSequenceModule sequenceModule = this.ProbingSequenceModule;

                if (MI != null)
                {
                    LoggerManager.Debug($"[ReProbe] State: {caller.DeclaringType} XIndex = {MI.XIndex}, YIndex = {MI.YIndex} " +
                        $" ProbingSequenceRemainCount : {sequenceModule.ProbingSequenceRemainCount}" +
                        $" OnlineRetestListCount : {sequenceModule.OnlineRetestList.Count}" +
                        $" ProbingCP1OnlineRetestRemainCount : {sequenceModule.ProbingCP1OnlineRetestRemainCount}");
                }
                else
                {
                    LoggerManager.Debug($"[ReProbe] State: {caller.DeclaringType} MI is NULL " +
                        $" ProbingSequenceRemainCount : {sequenceModule.ProbingSequenceRemainCount}" +
                        $" OnlineRetestListCount : {sequenceModule.OnlineRetestList.Count}" +
                        $" ProbingCP1OnlineRetestRemainCount : {sequenceModule.ProbingCP1OnlineRetestRemainCount}");
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        public EventCodeEnum OnlineRetestGetNextSequnce(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                MI = new MachineIndex();

                MachineIndex onlineretestMI = new MachineIndex();

                var wfObj = this.ProbingSequenceModule.StageSupervisor().WaferObject;

                //bool caninstant = false;
                //var bininfos = this.ProbingSequenceModule.ProbingModule().GetBinInfos();

                this.ProbingSequenceModule.CP1OnlineRetestSeq.Clear();

                for (int i = 0; i < this.ProbingSequenceModule.ProbingSequenceCount; i++)
                {
                    onlineretestMI = this.ProbingSequenceModule.ProbingSeqParameter.ProbingSeq.Value[i];

                    ReTestMode reTestMode = this.ProbingSequenceModule.OnlineRetestList[0].Mode.Value;

                    if (reTestMode == ReTestMode.ALL)
                    {
                        if (this.ProbingSequenceModule.ProbingModule().NeedRetest(onlineretestMI.XIndex, onlineretestMI.YIndex) == true)
                        {
                            this.ProbingSequenceModule.CP1OnlineRetestSeq.Add(onlineretestMI);
                        }

                        //var deviceObject = wfObj.GetSubsInfo().DIEs[onlineretestMI.XIndex, onlineretestMI.YIndex];

                        //DieStateEnum dieStateEnum = deviceObject.State.Value;
                        //TestState testState = deviceObject.CurTestHistory.TestResult.Value;

                        //if (dieStateEnum == DieStateEnum.TESTED && testState == TestState.MAP_STS_FAIL)
                        //{
                        //    this.ProbingSequenceModule.CP1OnlineRetestSeq.Add(onlineretestMI);
                        //}
                    }
                    else
                    {
                        //var needRetest = this.ProbingSequenceModule.ProbingModule().GetRetestEnable(RetestTypeEnum.RETESTFORCP1, MI.XIndex, MI.YIndex);
                        var needRetest = this.ProbingSequenceModule.ProbingModule().NeedRetestbyBIN(RetestTypeEnum.RETESTFORCP1, MI.XIndex, MI.YIndex);

                        if (needRetest == true)
                        {
                            this.ProbingSequenceModule.CP1OnlineRetestSeq.Add(onlineretestMI);
                        }

                    }
                }

                this.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount = this.ProbingSequenceModule.CP1OnlineRetestSeq.Count;

                if (this.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount > 0)
                {
                    MI = this.ProbingSequenceModule.CP1OnlineRetestSeq[0];

                    if (this.ProbingSequenceModule.OnlineRetestList[0].InstantRetest.Value == true)
                    {
                        this.SetProbingCP1InnerState(EnumProbingInnerState.CP1OnlineInstant);

                        if (this.ProbingSequenceModule.ProbingCP1OnlineRetestRemainCount == 1)
                        {
                            this.SetProbingCP1InnerState(EnumProbingInnerState.CP1FinalOnlineInstant);
                        }
                    }
                    else
                    {
                        this.SetProbingCP1InnerState(EnumProbingInnerState.CP1OnlineRetest);
                    }

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    MI = null;
                    this.ConditionForStateTrainsitionNoseqOrOnlineStart2(ref MI);

                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            this.LogMethod(MethodBase.GetCurrentMethod(), MI);

            return retVal;
        }
        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = CP1InnerState.GetNextSequence(ref MI);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;

        }
    }

    public class MultiPassProbeState : ProbingSequenceInnerStateBase
    {
        public MultiPassProbeState(ProbingSequenceModule module) : base(module)
        {
            MPPInnerState = new MPPNormalStartState(this);
        }

        private MultiPassProbeOptionBase _MPPInnerState;
        public MultiPassProbeOptionBase MPPInnerState
        {
            get { return _MPPInnerState; }
            set
            {
                _MPPInnerState = value;
            }
        }
        public override LotModeEnum GetState()
        {
            return LotModeEnum.MPP;
        }

        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = MPPInnerState.GetNextSequence(ref MI);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }

        public EventCodeEnum SetProbingMPPInnerState(EnumProbingInnerState state)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {

                switch (state)
                {
                    case EnumProbingInnerState.MPPNormal:
                        MPPInnerState = new MPPNormalState(this);
                        break;
                    case EnumProbingInnerState.MPPNormalStart:
                        MPPInnerState = new MPPNormalStartState(this);
                        break;
                    case EnumProbingInnerState.MPPInstant:
                        MPPInnerState = new MPPInstantRetestState(this);
                        break;
                    case EnumProbingInnerState.MPPFinalInstant:
                        MPPInnerState = new MPPFinalInstantState(this);
                        break;
                    case EnumProbingInnerState.MPPDone:
                        MPPInnerState = new MPPDoneState(this);
                        break;
                    default:
                        break;
                }
                LoggerManager.Debug($"State Transition to {state} ");

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retval = EventCodeEnum.UNDEFINED;
            }

            return retval;
        }
    }

    public class UndefinedState : ProbingSequenceInnerStateBase
    {
        public UndefinedState(ProbingSequenceModule module) : base(module)
        {

        }

        public override LotModeEnum GetState()
        {
            return LotModeEnum.UNDEFINED;
        }

        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            throw new NotImplementedException();
        }
    }
}