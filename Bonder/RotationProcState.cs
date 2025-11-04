using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Bonder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonder.RotationProcStates
{
    public abstract class RotationProcState
    {
        public RotationProcModule Module { get; set; }
        public RotationProcState(RotationProcModule module)
        {
            this.Module = module;
        }
        public void StateTransition(RotationProcState stateObj)
        {
            this.Module.StateObj = stateObj;
        }
        internal void SetDoneState()
        {
            StateTransition(new DoneState(Module));
        }
        public abstract BonderTransferProcStateEnum State { get; }

        public abstract void Execute();
    }
    public class IdleState : RotationProcState
    {
        BonderModule bonderModule = ShareInstance.bonderModule;

        public IdleState(RotationProcModule module) : base(module) { }

        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.IDLE;
        public override void Execute()
        {
            if(bonderModule.IsRotationMove || bonderModule.IsRotationOnlyMove)
            {
                StateTransition(new RunningState(Module));
            }
        }
    }
    public class RunningState : RotationProcState
    {
        BonderModule bonderModule = ShareInstance.bonderModule;

        public RunningState(RotationProcModule module) : base(module) { }
        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.RUNNING;
        public override void Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug("[Bonder] (Rotation) RunningState. Execute()");

                //double armOffset = 0;
                //retVal = Module.StageSupervisor().StageModuleState.MoveLoadingPosition(armOffset);
                //retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(true, extraVacReady: true);

                // Pick에서 안켜진 쪽 Arm의 Air On
                if (bonderModule.RotationCount % 2 == 1) // 홀수 = ARM2
                {
                    retVal = Module.BonderSupervisor().BonderModuleState.Arm2_AirOnOff(true);   // Arm2 Vacuum On
                }
                else  // 짝수 = ARM1
                {
                    retVal = Module.BonderSupervisor().BonderModuleState.Arm1_AirOnOff(true);   // Arm1 Vacuum On
                }

                retVal = Module.BonderSupervisor().BonderModuleState.MoveRotation();

                // Pick에서 Air가 안 켜지고 바로 위에서 켜진 Arm 쪽의 Air Off
                if (bonderModule.RotationCount % 2 == 1) // 홀수 = ARM2
                {
                    retVal = Module.BonderSupervisor().BonderModuleState.Arm2_AirOnOff(false);  // Arm2 Vacuum Off
                                                                                                // RotatingState => StageMoveToPlacePosition
                }
                else  // 짝수 = ARM1
                {
                    retVal = Module.BonderSupervisor().BonderModuleState.Arm1_AirOnOff(false);  // Arm1 Vacuum Off
                                                                                                // RotatingState => StageMoveToPlacePosition
                }

                bonderModule.RotationCount++;   // 증가 위치 주의
                SetDoneState();
            }
            catch
            {

            }
        }
    }
    public class SuspendedState : RotationProcState
    {
        public Task<EventCodeEnum> LoadResultMapTask;
        public SuspendedState(RotationProcModule module) : base(module) { }
        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.SUSPENDED;
        public override void Execute()
        {
            LoggerManager.Debug("[Bonder] (Rotation) SuspendedState. Execute()");
            SetDoneState();
        }
    }
    public class DoneState : RotationProcState
    {
        BonderModule bonderModule = ShareInstance.bonderModule;

        public DoneState(RotationProcModule module) : base(module) { }
        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.DONE;

        public override void Execute()
        {
            LoggerManager.Debug("[Bonder] RotationProcState. DoneState. Execute()");

            if(bonderModule.IsRotationOnlyMove)
            {
                bonderModule.IsRotationOnlyMove = false;
                bonderModule.IsRotationMove = false;
            }
            else
            {
                bonderModule.IsRotationMove = false;
                bonderModule.IsWaferChuckMove = true;   // 웨이퍼 스테이지 움직임 시작
            }

            StateTransition(new IdleState(Module));
        }
    }
    public class SystemErrorState : RotationProcState
    {
        public SystemErrorState(RotationProcModule module) : base(module) { }

        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.ERROR;

        public override void Execute()
        {
            LoggerManager.Debug("[Bonder] (Rotation) SystemErrorState. Execute()");
        }
    }
}
