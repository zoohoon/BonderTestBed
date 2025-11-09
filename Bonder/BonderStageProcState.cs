using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Bonder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.BonderStageProcStates
{
    public abstract class BonderStageProcState
    {
        BonderModule bonderModule = ShareInstance.bonderModule;

        public BonderStageProcModule Module { get; set; }
        public abstract BonderTransferProcStateEnum State { get; }

        public abstract void Execute();
        public BonderStageProcState(BonderStageProcModule module)
        {
            this.Module = module;
        }
        public void StateTransition(BonderStageProcState stateObj)
        {
            this.Module.StateObj = stateObj;
        }
        internal void SetDoneState()
        {
            // 모든 변수 초기화
            bonderModule.IsFDChuckMove = false;
            bonderModule.IsWaferChuckMove = false;
            bonderModule.IsPickerDoing = false;
            bonderModule.IsFDResume = false;
            bonderModule.IsRotationMove = false;
            bonderModule.IsPlaceDoing = false;

            bonderModule.IsPickerOnlyDoing = false;
            bonderModule.IsRotationOnlyMove = false;
            bonderModule.IsPlaceOnlyDoing = false;

            bonderModule.RotationCount = 1; // 초기화 = 1

            StateTransition(new DoneState(Module));

            //SemaphoreSlim semaphore = new SemaphoreSlim(0);
            //Module.EventManager().RaisingEvent(typeof(WaferLoadedEvent).FullName, new ProbeEventArgs(this, semaphore));
            //semaphore.Wait();
        }
    }
    public class IdleState : BonderStageProcState
    {
        public IdleState(BonderStageProcModule module) : base(module) { }
        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.IDLE;
        public override void Execute()
        {
            StateTransition(new RunningState(Module));
        }
    }
    public class RunningState : BonderStageProcState
    {
        BonderModule bonderModule = ShareInstance.bonderModule;

        public RunningState(BonderStageProcModule module) : base(module) { }
        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.RUNNING;
        public override void Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(bonderModule.IsFDChuckMove)
                {
                    LoggerManager.Debug("[Bonder] (BonderStage) RunningState : IsFDChuckMove Start");

                    //LoggerManager.Debug("[Bonder] (BonderStage) check = Die exist?");   // FD Map의 데이터를 확인    => 나중에

                    // FDStage_VacuumOnOff 체크 추가 필요

                    double pickOffset = 0;
                    retVal = Module.BonderSupervisor().BonderModuleState.MovePickPosition(pickOffset);  // IDLE => StageMoveToPickPosition

                    double EjPickOffset = 0;
                    retVal = Module.BonderSupervisor().BonderModuleState.MoveEjPickPosition(EjPickOffset);  // StageMoveToPickPosition => StageMoveToZUp 

                    retVal = Module.BonderSupervisor().BonderModuleState.MovePickZMove(true);    // true = FD척 Z Up

                    retVal = Module.BonderSupervisor().BonderModuleState.MoveEjPickZMove(true);     // true = Ejection Z Up , StageMoveToZUp => StageMoveToPinZUp

                    retVal = Module.BonderSupervisor().BonderModuleState.MovePinZMove(true);    // true = Ejection Pin Z Up

                    retVal = Module.BonderSupervisor().BonderModuleState.EjPin_VacuumOnOff(true, true);    // StageMoveToPinZUp => PickingState

                    bonderModule.IsPickerDoing = true;  // Pick 시작

                    StateTransition(new SuspendedState(Module));    // Pick 끝날 때까지 대기
                }
                else if(bonderModule.IsWaferChuckMove)
                {
                    LoggerManager.Debug("[Bonder] (BonderStage) RunningState : IsWaferChuckMove Start");

                    LoggerManager.Debug("[Bonder] (BonderStage) check = Wafer Stage Z Safe Position?");

                    double placeOffset = 0;
                    retVal = Module.BonderSupervisor().BonderModuleState.MovePlacePosition(placeOffset);

                    retVal = Module.BonderSupervisor().BonderModuleState.MovePlaceZMove(true);  // Z Up , StageMoveToPlacePosition => PlacingState

                    bonderModule.IsWaferChuckMove = false;
                    bonderModule.IsPlaceDoing = true;   // Place 시작
                }
                else
                {
                    // No work = Suspended
                    // FD Stage 움직이고 Wafer Stage 움직이기 전 그 사이에 고립됨 (Rotation 하는 동안)
                    // Rotation 후 Wafer Stage 움직이고 고립됨

                    if (bonderModule.IsBonderEnd)   // 전체 종료
                    {
                        SetDoneState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
    public class SuspendedState : BonderStageProcState
    {
        BonderModule bonderModule = ShareInstance.bonderModule;

        public Task<EventCodeEnum> LoadResultMapTask;
        public SuspendedState(BonderStageProcModule module) : base(module) { }
        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.SUSPENDED;
        public override void Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            if (bonderModule.IsFDResume) // Pick 끝나고 FD Stage 재이동(Z축 복귀)
            {
                LoggerManager.Debug("[Bonder] (BonderStage) SuspendedState. Execute()");

                bonderModule.IsFDResume = false;

                retVal = Module.BonderSupervisor().BonderModuleState.EjPin_VacuumOnOff(false, false);   // false = Vac Off

                LoggerManager.Debug("[Bonder] (BonderStage) check = Ejection Pin Vacuum Off?");

                retVal = Module.BonderSupervisor().BonderModuleState.MovePinZMove(false);    // false = Ejection Pin Z Down , StageMoveToPinZDown => StageMoveToZDown

                retVal = Module.BonderSupervisor().BonderModuleState.MoveEjPickZMove(false);    // false = Ejection Z Down

                retVal = Module.BonderSupervisor().BonderModuleState.MovePickZMove(false);    // false = FD척 Z Down , StageMoveToZDown => RotatingState

                LoggerManager.Debug("[Bonder] (BonderStage) check = Die exist on Picker?");

                if(bonderModule.IsPickerOnlyDoing == true)
                {
                    // Pick만 하는 경우
                    bonderModule.IsFDChuckMove = false;     // FD척 이동 종료
                    StateTransition(new RunningState(Module));   // Rotation 끝날 때까지 대기
                }
                else
                {
                    bonderModule.IsFDChuckMove = false;     // FD척 이동 종료
                    bonderModule.IsRotationMove = true;     // Rotation 시작
                    StateTransition(new RunningState(Module));   // Rotation 끝날 때까지 대기
                }
            }
            else
            {
                // No Work = Suspended
                if (bonderModule.IsBonderEnd)   // 전체 종료
                {
                    SetDoneState();
                }
            }
        }
    }
    public class DoneState : BonderStageProcState
    {
        public DoneState(BonderStageProcModule module) : base(module) { }
        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.DONE;
        public override void Execute()
        {
            LoggerManager.Debug("[Bonder] (BonderStage). DoneState. Execute()");

            StateTransition(new IdleState(Module));
        }
    }
    public class SystemErrorState : BonderStageProcState
    {
        public SystemErrorState(BonderStageProcModule module) : base(module) { }
        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.ERROR;
        public override void Execute() { /*NoWORKS*/ }
    }
}
