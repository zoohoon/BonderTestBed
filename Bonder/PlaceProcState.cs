using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Bonder;
using System.Threading;
using System.Threading.Tasks;

namespace Bonder.PlaceProcStates
{
    public abstract class PlaceProcState
    {
        public PlaceProcModule Module { get; set; }
        public PlaceProcState(PlaceProcModule module)
        {
            this.Module = module;
        }
        public void StateTransition(PlaceProcState stateObj)
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

    }
    public class IdleState : PlaceProcState
    {
        BonderModule bonderModule = ShareInstance.bonderModule;

        public IdleState(PlaceProcModule module) : base(module) { }
        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.IDLE;
        public override void Execute()
        {
            if(bonderModule.IsPlaceDoing || bonderModule.IsPlaceOnlyDoing)
            {
                StateTransition(new RunningState(Module));
            }
        }
    }
    public class RunningState : PlaceProcState
    {
        BonderModule bonderModule = ShareInstance.bonderModule;

        public RunningState(PlaceProcModule module) : base(module) { }
        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.RUNNING;
        public override void Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug("[Bonder] (Place) RunningState. Execute()");

                LoggerManager.Debug("[Bonder] (Place) check = Die exist on Arm?");

                retVal = Module.BonderSupervisor().BonderModuleState.MagneticOnOff(true);   // true = On

                retVal = Module.BonderSupervisor().BonderModuleState.Arm1_AirOnOff(false);   // false = Air1 Air Off
                LoggerManager.Debug("[Bonder] (Place) Check = Arm1 Air Off");

                retVal = Module.BonderSupervisor().BonderModuleState.Arm2_AirOnOff(false);   // false = Air2 Air Off
                LoggerManager.Debug("[Bonder] (Place) Check = Arm2 Air Off");

                // retVal = Module.BonderSupervisor().BonderModuleState.MoveNanoZMove(false);    // false = Z Down , 나노스테이지는 나중에
                // LoggerManager.Debug("[Bonder] (Place) Nano Stage = Die Align Processing");
                // LoggerManager.Debug("[Bonder] (Place) check = Die Align finish?");

                // (추가) 플1 플2 구분 추가
                if (bonderModule.RotationCount % 2 == 1) // 홀수 = ARM2
                {
                    retVal = Module.BonderSupervisor().BonderModuleState.Arm2_VacuumOnOff(false, false);   // Arm2 Vacuum OnOFF

                    retVal = Module.BonderSupervisor().BonderModuleState.Arm2_BlowOnOff(true, true);  // Arm2 Blow On
                    Thread.Sleep(250);
                    retVal = Module.BonderSupervisor().BonderModuleState.Arm2_BlowOnOff(false, false);  // Arm2 Blow Off

                }
                else  // 짝수 = ARM1
                {
                    retVal = Module.BonderSupervisor().BonderModuleState.Arm1_VacuumOnOff(false, false);   // Arm1 Vacuum OnOFF

                    retVal = Module.BonderSupervisor().BonderModuleState.Arm1_BlowOnOff(true, true);  // Arm1 Blow On
                    Thread.Sleep(250);
                    retVal = Module.BonderSupervisor().BonderModuleState.Arm1_BlowOnOff(false, false);  // Arm1 Blow Off
                }

                // retVal = Module.BonderSupervisor().BonderModuleState.MoveNanoZMove(true);    // true = Z Up , 나노스테이지는 나중에

                retVal = Module.BonderSupervisor().BonderModuleState.MagneticOnOff(false);   // false = Off , PlacingState => StageIDLEState

                SetDoneState();
            }
            catch
            {

            }
        }
    }
    public class SuspendedState : PlaceProcState
    {
        public Task<EventCodeEnum> LoadResultMapTask;
        public SuspendedState(PlaceProcModule module) : base(module) { }
        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.SUSPENDED;
        public override void Execute()
        {
            LoggerManager.Debug("[Bonder] (Place) SuspendedState. Execute()");
            SetDoneState();
        }
    }
    public class DoneState : PlaceProcState
    {
        BonderModule bonderModule = ShareInstance.bonderModule;

        public DoneState(PlaceProcModule module) : base(module) { }
        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.DONE;
        public override void Execute()
        {
            LoggerManager.Debug("[Bonder] (Place) DoneState. Execute()");

            if (bonderModule.IsDieExist() == EventCodeEnum.NONE)
            {
                bonderModule.IsBonderEnd = true;    // 본더 종료
            }
            else
            {
                bonderModule.IsFDChuckMove = true;
            }

            bonderModule.IsPlaceDoing = false;

            StateTransition(new IdleState(Module));
        }
    }
    public class SystemErrorState : PlaceProcState
    {
        public SystemErrorState(PlaceProcModule module) : base(module) { }
        public override BonderTransferProcStateEnum State => BonderTransferProcStateEnum.ERROR;
        public override void Execute()
        {
            LoggerManager.Debug("[Bonder] (Place) SystemErrorState. Execute()");
        }
    }
}
