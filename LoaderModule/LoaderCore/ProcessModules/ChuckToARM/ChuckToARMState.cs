using System;
using System.Runtime.CompilerServices;

using Autofac;
using LoaderBase;
using LoaderParameters;
using ProberInterfaces;
using ProberErrorCode;
using LogModule;

namespace LoaderCore.ChuckToARMStates
{
    public abstract class ChuckToARMState : LoaderProcStateBase
    {
        public ChuckToARM Module { get; set; }

        public ChuckToARMState(ChuckToARM module)
        {
            this.Module = module;
        }

        protected void StateTransition(ChuckToARMState stateObj)
        {
            try
            {
                Module.StateObj = stateObj;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        protected ILoaderModule Loader => Module.Container.Resolve<ILoaderModule>();

        protected IChuckModule CHUCK => Module.Param.Curr as IChuckModule;

        protected IARMModule ARM => Module.Param.Next as IARMModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
                LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

                retval = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum ChuckUpMove()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum ChuckDownMove(int option=0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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
        public virtual EventCodeEnum RetractARM()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum WriteVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum MonitorForVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum WaitForVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum WTR_SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum NotifyLoadedToThreeLeg(out TransferObject loadedObject)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            loadedObject = null;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public virtual EventCodeEnum GetWaferLoadObject(out TransferObject loadObject)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            loadObject = null;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum NotifyUnloadedFromThreeLeg(EnumWaferState waferState, int cellIdx, bool NotifyUnloadedFromThreeLeg = false)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum PickUpMove()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum PlaceDownMove()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum WaferTransferEnd(bool isSucceed)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum SelfRecoveryTransferToPreAlign()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum SelfRecoveryRetractARM()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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

    public class IdleState : ChuckToARMState
    {
        public IdleState(ChuckToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                StateTransition(new PreChuckDownMoveState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void Resume()
        {
            try
            {
                LoggerManager.Error("$Not implemented error");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SelfRecovery()
        {
            try
            {
                LoggerManager.Error("$Not implemented error");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class PreChuckDownMoveState : ChuckToARMState
    {
        public PreChuckDownMoveState(ChuckToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CHUCK.ValidateWaferStatus();

                if (CHUCK.Holder.Status != EnumSubsStatus.EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is no Wafer on the Chuck. ");
                    Loader.ResonOfError = $"{Module.GetType().Name}  There is no Wafer on the Chuck.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                ARM.ValidateWaferStatus();

                if (ARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is Wafer on the ARM. ");
                    Loader.ResonOfError = $"{Module.GetType().Name}  There is Wafer on the ARM.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Clean PreAlignState
                CHUCK.Holder.TransferObject.CleanPreAlignState(reason: "Chuck To Arm.");

                //=> PreChuckDownMove
                retval = Loader.Move.PreChuckDownMove(ARM, CHUCK);

                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PreChuckDownMove() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name}  PreChuckDown Motion Error.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                LoggerManager.Debug("Loader Suspend Reason = WAFER UNLOAD");
                StateTransition(new WaitForRemotingState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class WaitForRemotingState : ChuckToARMState
    {
        public WaitForRemotingState(ChuckToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SUSPENDED;

        public override ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.UNLOAD;

        public override void Execute()
        {
            //No WORKS.
            try
            {
                LoggerManager.Error("$Not implemented error");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void Resume()
        {
            try
            {
                StateTransition(new RemotingState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }

    public class RemotingState : ChuckToARMState
    {
        public RemotingState(ChuckToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute() { }

        public override EventCodeEnum WaferTransferEnd(bool isSucceed)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
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

            return retval;
        }

        public override EventCodeEnum WriteVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = ARM.WriteVacuum(value);
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} WriteVacuum({value}) ReturnValue={retval}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum MonitorForVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = ARM.MonitorForVacuum(value);
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} MonitorForVacuum({value}) ReturnValue={retval}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum WaitForVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = ARM.WaitForVacuum(value);
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} WaitForVacuum() ReturnValue={retval}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum ChuckDownMove(int option=0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Loader.Move.ChuckDownMove(ARM, CHUCK);
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} ChuckDownMove() ReturnValue={retval}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum PickUpMove()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Loader.Move.PickUp(ARM, CHUCK);
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} PickUpMove() ReturnValue={retval}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum PlaceDownMove()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Loader.Move.PlaceDown(ARM, CHUCK);
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} PlaceDownMove() ReturnValue={retval}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum RetractARM()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} RetractARM()");
                retval = Loader.Move.RetractAll();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum WTR_SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} WTR_SetWaferUnknownStatus({isARMUnknown},{isChuckUnknown})");
                if (isARMUnknown)
                {
                    ARM.Holder.SetUnknown();
                }

                if (isChuckUnknown)
                {
                    CHUCK.Holder.SetUnknown();
                }

                Loader.BroadcastLoaderInfo();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum NotifyUnloadedFromThreeLeg(EnumWaferState waferState,int cellIdx, bool NotifyUnloadedFromThreeLeg = false)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} NotifyUnloadedFromThreeLeg()");
                CHUCK.Holder.TransferObject.WaferState = waferState;
                CHUCK.Holder.SetTransfered(ARM);

                Loader.BroadcastLoaderInfo();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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
        public override EventCodeEnum SelfRecoveryTransferToPreAlign()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} SelfRecoveryTransferToPreAlign() ");
                IPreAlignable usablePA;

                usablePA = Loader.ModuleManager.FindUsablePreAlignable(Module.Param.TransferObject);

                if (usablePA == null)
                {
                    retval = EventCodeEnum.UNDEFINED;
                    return retval;
                }

                if (usablePA is IPreAlignModule)
                {
                    var PA = usablePA as IPreAlignModule;

                    //Self Recovery처리하는 동안 웨이퍼가 정상적으로 이송되었다고 가정한다.
                    CHUCK.Holder.SetUnload();
                    ARM.Holder.SetLoad(Module.Param.TransferObject);

                    retval = Loader.Move.RetractAll(LoaderMovingTypeEnum.RECOVERY);

                    if (retval != EventCodeEnum.NONE)
                    {
                        CHUCK.Holder.SetUnknown();
                        ARM.Holder.SetUnknown();
                        return retval;
                    }

                    retval = Loader.Move.PreAlignUpMove(ARM, PA, LoaderMovingTypeEnum.RECOVERY);
                    if (retval != EventCodeEnum.NONE)
                    {
                        CHUCK.Holder.SetUnknown();
                        ARM.Holder.SetUnknown();
                        return retval;
                    }

                    retval = PA.WriteVacuum(true);
                    if (retval != EventCodeEnum.NONE)
                    {
                        CHUCK.Holder.SetUnknown();
                        ARM.Holder.SetUnknown();
                        return retval;
                    }

                    retval = Loader.Move.PlaceDown(ARM, PA, LoaderMovingTypeEnum.RECOVERY);
                    if (retval != EventCodeEnum.NONE)
                    {
                        CHUCK.Holder.SetUnknown();
                        ARM.Holder.SetUnknown();
                        PA.Holder.SetUnknown();

                        return retval;
                    }

                    retval = PA.WaitForVacuum(true);
                    if (retval != EventCodeEnum.NONE)
                    {
                        CHUCK.Holder.SetUnknown();
                        ARM.Holder.SetUnknown();
                        PA.Holder.SetUnknown();

                        return retval;
                    }

                    //Recovered wafer on PreAligner
                    //CHUCK.Holder.SetUnload();
                    ARM.Holder.SetUnload();
                    PA.Holder.SetLoad(Module.Param.TransferObject);

                    Loader.BroadcastLoaderInfo();

                    retval = Loader.Move.RetractAll(LoaderMovingTypeEnum.RECOVERY);
                }
                else
                {
                    retval = EventCodeEnum.UNDEFINED;
                    return retval;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum SelfRecoveryRetractARM()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Loader.Move.RetractAll(LoaderMovingTypeEnum.RECOVERY);
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} SelfRecoveryRetractARM() ReturnValue={retval}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override double GetCurrArmUpOffset()
        {
           double retVal = 0;
            try
            {
                retVal=ARM.Definition.UpOffset.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }

    public class DoneState : ChuckToARMState
    {
        public DoneState(ChuckToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }
    }

    public class SystemErrorState : ChuckToARMState
    {
        public SystemErrorState(ChuckToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}

