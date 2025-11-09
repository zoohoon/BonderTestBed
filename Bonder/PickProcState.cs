using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Bonder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.PickProcStates
{
    public abstract class PickProcState
    {
        public PickProcModule Module { get; set; }
        public PickProcState(PickProcModule module)
        {
            this.Module = module;
        }
        public void StateTransition(PickProcState stateObj)
        {
            this.Module.StateObj = stateObj;
        }
        internal void SetDoneState()
        {
            StateTransition(new DoneState(Module));

            //SemaphoreSlim semaphore = new SemaphoreSlim(0);
            //Module.EventManager().RaisingEvent(typeof(WaferLoadedEvent).FullName, new ProbeEventArgs(this, semaphore));
            //semaphore.Wait();
        }
        public abstract BonderTransferProcStateEnum State { get; }

        public abstract void Execute();

        //protected long CHUCK_VAC_MAINTAIN_TIME = 50;
        //protected long CHUCK_VAC_CHECK_TIME = 5000;
        //protected long CHUCK_VAC_WAIT_TIME = 10000;
        //protected long CHUCK_THREELEG_WAIT_TIME = 10000;
    }
    public class IdleState : PickProcState
    {
        BonderModule bonderModule = ShareInstance.bonderModule;

        public IdleState(PickProcModule module) : base(module) { }

        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.IDLE;
        public override void Execute()
        {         
            if (bonderModule.IsPickerDoing || bonderModule.IsPickerOnlyDoing)
            {
                StateTransition(new RunningState(Module));
            }
        }
    }
    public class RunningState : PickProcState
    {
        BonderModule bonderModule = ShareInstance.bonderModule;

        public RunningState(PickProcModule module) : base(module) { }
        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.RUNNING;
        public override void Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug("[Bonder] (Pick) RunningState. Execute()");

                if (bonderModule.RotationCount % 2 == 1) // 홀수 = ARM1
                {
                    retVal = Module.BonderSupervisor().BonderModuleState.Arm1_AirOnOff(true);   // true = Arm1 Air On

                    LoggerManager.Debug("[Bonder] (Pick) check = Arm1_Pressure?");
                    LoggerManager.Debug("[Bonder] (Pick) check = Arm1_Flow?");

                    retVal = Module.BonderSupervisor().BonderModuleState.Arm1_VacuumOnOff(true, true);   // Pick1 Vacuum On , PickingState => StageMoveToPinZDown

                    LoggerManager.Debug("[Bonder] (Pick) check = Arm1_Vacuum?");
                }
                else  // 짝수 = ARM2
                {
                    retVal = Module.BonderSupervisor().BonderModuleState.Arm2_AirOnOff(true);   // true = Arm1 Air On

                    LoggerManager.Debug("[Bonder] (Pick) check = Arm2_Pressure?");
                    LoggerManager.Debug("[Bonder] (Pick) check = Arm2_Flow?");

                    retVal = Module.BonderSupervisor().BonderModuleState.Arm2_VacuumOnOff(true, true);   // Pick2 Vacuum On , PickingState => StageMoveToPinZDown

                    LoggerManager.Debug("[Bonder] (Pick) check = Arm2_Vacuum?");
                }

                SetDoneState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class SuspendedState : PickProcState
    {
        public Task<EventCodeEnum> LoadResultMapTask;
        public SuspendedState(PickProcModule module) : base(module) { }

        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.SUSPENDED;
        public override void Execute()
        {
            LoggerManager.Debug("[Bonder] (Pick) SuspendedState. Execute()");
            SetDoneState();
        }
    }
    public class DoneState : PickProcState
    {
        BonderModule bonderModule = ShareInstance.bonderModule;

        public DoneState(PickProcModule module) : base(module) { }

        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.DONE;

        public override void Execute()
        {
            LoggerManager.Debug("[Bonder] (Pick) DoneState. Execute()");

            if (bonderModule.IsPickerDoing)
            {
                bonderModule.IsPickerDoing = false;

                bonderModule.IsFDResume = true;     // FD Stage 재시작
            }

            StateTransition(new IdleState(Module));
        } 
    }
    public class SystemErrorState : PickProcState
    {
        public SystemErrorState(PickProcModule module) : base(module) { }

        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.ERROR;

        public override void Execute()
        {
            LoggerManager.Debug("[Bonder] (Pick) SystemErrorState. Execute()");
            SetDoneState();
        }
    }
}
