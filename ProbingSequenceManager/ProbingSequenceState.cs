using System;
using ProberInterfaces;
using ProberErrorCode;
using LogModule;

namespace ProbingSequenceManager
{
    //public class ProbingSequenceIDleState : ProbingSequenceStateBase
    //{
    //    public ProbingSequenceIDleState(ProbingSequenceModule module) : base(module)
    //    {
    //    }

    //    public override ModuleStateEnum GetModuleState()
    //    {
    //        return ModuleStateEnum.IDLE;
    //    }

    //    public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
    //    {
    //        EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

    //        MI = new MachineIndex();

    //        if (Module.ProbingSequenceRemainCount > 0)
    //        {
    //            MI = Module.ProbingSeqParameter.ProbingSeq[(Module.ProbingSequenceCount - Module.ProbingSequenceRemainCount)];

    //            Module.ProbingSequenceRemainCount--;

    //            //List<IDeviceObject> testedDevices;

    //            //testedDevices = Module.Prober.Probing.GetTestResult(Module.Prober._StageSuperVisor.WaferObject);
    //            Module.Prober.Probing.GetUnderDutDices(Module.Prober._StageSuperVisor.ProbeCardInfo, MI);

    //            Module.ProbingSequenceStateTransition(new ProbingSequenceSRState(Module));

    //            RetVal = EventCodeEnum.NONE;
    //        }
    //        else
    //        {
    //            RetVal = EventCodeEnum.NODATA;

    //            Module.ProbingSequenceStateTransition(new ProbingSequenceNMSRState(Module));
    //        }

    //        return RetVal;
    //    }

    //    public override ProbingSequenceStateEnum GetState()
    //    {
    //        return ProbingSequenceStateEnum.IDLE;
    //    }
    //}

    public abstract class ProbingSequenceState
    {
        public abstract EventCodeEnum GetNextSequence(ref MachineIndex MI);
        public abstract ProbingSequenceStateEnum GetState();
        public abstract ModuleStateEnum GetModuleState();
        //public ProbingSequenceInnerStateBase ProbingInnerState { get; set; }
    }

    public abstract class ProbingSequenceInnerStateBase
    {
        private ProbingSequenceModule _ProbingSequenceModule;

        public ProbingSequenceModule ProbingSequenceModule
        {
            get { return _ProbingSequenceModule; }
            set { _ProbingSequenceModule = value; }
        }

        public ProbingSequenceInnerStateBase(ProbingSequenceModule module)
        {
            ProbingSequenceModule = module;
        }

        public EnumProbingInnerState PrevCP1InnerState;
        public abstract LotModeEnum GetState();
        public abstract EventCodeEnum GetNextSequence(ref MachineIndex MI);
    }

    public abstract class ProbingSequenceStateBase : ProbingSequenceState
    {
        private ProbingSequenceModule _Module;

        public ProbingSequenceModule Module
        {
            get { return _Module; }
            set { _Module = value; }
        }

        public ProbingSequenceStateBase(ProbingSequenceModule module)
        {
            Module = module;
        }
    }

    public class ProbingSequenceSRState : ProbingSequenceStateBase
    {
        public ProbingSequenceSRState(ProbingSequenceModule module) : base(module)
        {
            //ProbingInnerState = new CircuitProbe1State(this);
            //Module.SetProbingInnerState(LotModeEnum.CP1);
            if (Module.LotOPModule().LotInfo.LotMode.Value == LotModeEnum.CP1)
            {
                Module.SetProbingInnerState(LotModeEnum.CP1);
            }
            else if (Module.LotOPModule().LotInfo.LotMode.Value == LotModeEnum.MPP)
            {
                Module.SetProbingInnerState(LotModeEnum.MPP);
            }
            else if (Module.LotOPModule().LotInfo.LotMode.Value == LotModeEnum.CONTINUEPROBING)
            {
                Module.SetProbingInnerState(LotModeEnum.CONTINUEPROBING);
            }
            else
            {
                Module.SetProbingInnerState(LotModeEnum.CP1);
            }
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }



        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                MI = new MachineIndex();

                if (Module.ProbingSequenceRemainCount > 0 || 
                    Module.ProbingCP1OnlineRetestRemainCount > 0 || 
                    Module.ProbingMPPRetestRemainCount > 0)
                {
                    RetVal = Module.ProbingSequenceInnerState.GetNextSequence(ref MI);

                    //// TODO : ???
                    //if (MI == null)
                    //{
                    //    Module.ProbingSequenceInnerState.GetNextSequence(ref MI);
                    //}

                    RetVal = EventCodeEnum.NONE;
                }
                else
                {
                    Module.ProbingSequenceInnerState.GetNextSequence(ref MI);

                    //// TODO : ???
                    //if (MI == null)
                    //{
                    //    Module.ProbingSequenceInnerState.GetNextSequence(ref MI);
                    //}

                    //Module.ProbingSequenceState = new ProbingSequenceNMSRState(Module);
                    //Module.ProbingSequenceRemainCount = 0;

                    RetVal = EventCodeEnum.NODATA;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public override ProbingSequenceStateEnum GetState()
        {
            return ProbingSequenceStateEnum.SEQREMAIN;
        }
    }
    public class ProbingSequenceNMSRState : ProbingSequenceStateBase
    {
        public ProbingSequenceNMSRState(ProbingSequenceModule module) : base(module)
        {
            //ProbingInnerState = new CircuitProbe1State(this);
            if (Module.LotOPModule().LotInfo.LotMode.Value == LotModeEnum.CP1)
            {
                Module.SetProbingInnerState(LotModeEnum.CP1);
            }
            else if (Module.LotOPModule().LotInfo.LotMode.Value == LotModeEnum.MPP)
            {
                Module.SetProbingInnerState(LotModeEnum.MPP);
            }
            else if (Module.LotOPModule().LotInfo.LotMode.Value == LotModeEnum.CONTINUEPROBING)
            {
                Module.SetProbingInnerState(LotModeEnum.CONTINUEPROBING);
            }
            else
            {
                Module.SetProbingInnerState(LotModeEnum.CP1);
            }
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }

        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                // Init
                //Module.ProbingSequenceRemainCount = Module.ProbingSequenceCount;

                MI = null;

                if (Module.ProbingSequenceRemainCount > 0)
                {
                    //MI = Module.ProbingSeqParameter.ProbingSeq.Value[(Module.ProbingSequenceCount - Module.ProbingSequenceRemainCount)];

                    //Module.ProbingSequenceRemainCount--;
                    Module.ProbingSequenceStateTransition(new ProbingSequenceSRState(Module));

                    Module.ProbingSequenceInnerState.GetNextSequence(ref MI);


                    RetVal = EventCodeEnum.NONE;
                }
                else
                {
                    RetVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public override ProbingSequenceStateEnum GetState()
        {
            return ProbingSequenceStateEnum.NOSEQ;
        }
    }

    public class ProbingSequenceERRORState : ProbingSequenceStateBase
    {
        public ProbingSequenceERRORState(ProbingSequenceModule module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                RetVal = EventCodeEnum.UNDEFINED;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                
            }
            return RetVal;
        }

        public override ProbingSequenceStateEnum GetState()
        {
            return ProbingSequenceStateEnum.ERROR;
        }
    }
}
