using System;
using System.Threading.Tasks;

using Autofac;
using LoaderBase;
using LoaderParameters;
using ProberInterfaces;
using ProberErrorCode;
using System.Runtime.CompilerServices;
using LogModule;

namespace LoaderCore.ARMToChuckStates
{
    public abstract class ARMToChuckState : LoaderProcStateBase
    {
        public ARMToChuck Module { get; set; }

        public ARMToChuckState(ARMToChuck module)
        {
            this.Module = module;
        }
        protected void StateTransition(ARMToChuckState stateObj)
        {
            try
            {

                Module.StateObj = stateObj;
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} state transition : {State}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        protected ILoaderModule Loader => Module.Container.Resolve<ILoaderModule>();

        protected IARMModule ARM => Module.Param.Curr as IARMModule;

        protected IChuckModule CHUCK => Module.Param.Next as IChuckModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

        protected Task<EventCodeEnum> _PreChuckMoveTask = null;
        public EventCodeEnum WaitForPrechuckMove()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            if (_PreChuckMoveTask != null)
            {
                retVal = _PreChuckMoveTask.Result;//wait
                _PreChuckMoveTask = null;
            }
            return retVal;
        }

        public virtual EventCodeEnum ChuckUpMove(int option=0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum ChuckDownMove(int option=0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum RetractARM()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum SafePosW()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum WTR_SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown)
        {
            return RaiseInvalidState();
        }

        public virtual EventCodeEnum MonitorForVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum WaitForVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum NotifyLoadedToThreeLeg(out TransferObject loadedObject)
        {
            loadedObject = null;
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum GetLoadedObject(out TransferObject loadedObject)
        {
            loadedObject = null;
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum NotifyUnloadedFromThreeLeg(EnumWaferState waferState, int cellIdx, bool NotifyUnloadedFromThreeLeg = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum PickUpMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum PlaceDownMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum WriteVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum SelfRecoveryTransferToPreAlign()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum NotifyWaferTransferResult(bool isSucceed)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum SelfRecoveryRetractARM()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual double GetCurrArmUpOffset()
        {
            double retVal = 0;
            try
            {
                RaiseInvalidState();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class IdleState : ARMToChuckState
    {
        public IdleState(ARMToChuck module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            StateTransition(new PreChuckMoveState(Module));
        }
    }

    public class PreChuckMoveState : ARMToChuckState
    {
        public PreChuckMoveState(ARMToChuck module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {

                ARM.ValidateWaferStatus();

                CHUCK.ValidateWaferStatus();

                if (ARM.Holder.Status != EnumSubsStatus.EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is no Wafer on the ARM. ");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is no Wafer on the ARM.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                if (CHUCK.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is Wafer on the Chuck. ");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is Wafer on the Chuck.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }


                //TODO : Async
                _PreChuckMoveTask = Task.Run(() =>
                {
                    LoggerManager.Debug($"[LOADER] {Module.GetType().Name} PreChuckUpMove ");
                    Loader.ResonOfError = $"{Module.GetType().Name} PreChuckUp Motion Error";
                    return Loader.Move.PreChuckUpMove(ARM, CHUCK);
                });

                //retVal = Loader.Move.PreChuckUpMove(ARM, CHUCK);
                //if (retVal != EventCodeEnum.NONE)
                //{
                //    StateTransition(new SystemErrorState(Module));
                //    return;
                //}
                LoggerManager.Debug("Loader Suspend Reason=WAFER LOAD");
                StateTransition(new WaitForRemotingState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class WaitForRemotingState : ARMToChuckState
    {
        public WaitForRemotingState(ARMToChuck module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SUSPENDED;

        public override ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.LOAD;

        public override void Execute() { }

        public override EventCodeEnum GetLoadedObject(out TransferObject loadedObject)
        {
            loadedObject = null;
            try
            {
                loadedObject = this.Module.Param.TransferObject.Clone() as TransferObject;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public override void Resume()
        {
            StateTransition(new RemotingState(Module));
        }
    }

    public class RemotingState : ARMToChuckState
    {
        public RemotingState(ARMToChuck module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute() { }

        public override EventCodeEnum NotifyWaferTransferResult(bool isSucceed)
        {
            EventCodeEnum retVal = WaitForPrechuckMove();
            
            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} NotifyWaferTransferResult() ReturnValue={isSucceed}");
                if (isSucceed)
                {
                    StateTransition(new DoneState(Module));
                }
                else
                {
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum WriteVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            
            try
            {
                retVal = ARM.WriteVacuum(value);
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} WriteVacuum() ReturnValue={retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum MonitorForVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = ARM.MonitorForVacuum(value);
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} MonitorForVacuum() ReturnValue={retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum WaitForVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
               
                retVal = ARM.WaitForVacuum(value);
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} WaitForVacuum() ReturnValue={retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ChuckUpMove(int option=0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

           
            retVal = WaitForPrechuckMove();
            if (retVal != EventCodeEnum.NONE)
            {
                LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForPrechuckMove() ReturnValue={retVal}");
                Loader.ResonOfError = $"{Module.GetType().Name} WaitForPrechuck Motion Error";
                StateTransition(new SystemErrorState(Module));
                return retVal;
            }
            try
            {
           
                retVal = Loader.Move.ChuckUpMove(ARM, CHUCK);
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} ChuckUpMove() ReturnValue={retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PickUpMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.Move.PickUp(ARM, CHUCK);
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} PickUp() ReturnValue={retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PlaceDownMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.Move.PlaceDown(ARM, CHUCK);
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} PlaceDown() ReturnValue={retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }



        public override EventCodeEnum RetractARM()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.Move.RetractAll();
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} RetractAll() ReturnValue={retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum SafePosW()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.Move.SafePosW();
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} SafePosW() ReturnValue={retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }



        public override EventCodeEnum WTR_SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (isARMUnknown)
                {
                    ARM.Holder.SetUnknown();
                }

                if (isChuckUnknown)
                {
                    CHUCK.Holder.SetUnknown();
                }

                Loader.BroadcastLoaderInfo();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum NotifyLoadedToThreeLeg(out TransferObject loadedObject)
        {
            try
            {
                ARM.Holder.SetTransfered(CHUCK);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            loadedObject = this.Module.Param.TransferObject.Clone() as TransferObject;

            try
            {
                Loader.BroadcastLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public override double GetCurrArmUpOffset()
        {
            double retVal = 0;
            try
            {
                retVal = ARM.Definition.UpOffset.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class DoneState : ARMToChuckState
    {
        public DoneState(ARMToChuck module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }

    public class SystemErrorState : ARMToChuckState
    {
        public SystemErrorState(ARMToChuck module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}

