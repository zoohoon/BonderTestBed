using Autofac;
using LoaderBase;
using LoaderParameters;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LoaderCore.GP_ARMToChuckStates
{
    public abstract class GP_ARMToChuckState : LoaderProcStateBase
    {
        public GP_ARMToChuck Module { get; set; }

        public GP_ARMToChuckState(GP_ARMToChuck module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_ARMToChuckState stateObj)
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

        ILoaderModule loader;
        protected ILoaderModule Loader
        {
            get
            {
                if (loader == null)
                {
                    loader = Module.Container.Resolve<ILoaderModule>();
                }
                return loader;
            }
        }
        protected IARMModule ARM => Module.Param.Curr as IARMModule;

        protected IChuckModule CHUCK => Module.Param.Next as IChuckModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName] string memberName = "")
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
        public virtual EventCodeEnum ChuckUpMove(int option = 0)
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

        public virtual EventCodeEnum ChuckDownMove()
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
        public virtual EventCodeEnum WTR_Notifyhandlerholdwafer(bool ishandlerhold)
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

        public virtual EventCodeEnum GetWaferLoadedObject(out TransferObject loadedObject)
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

        public virtual EventCodeEnum NotifyUnloadedFromThreeLeg(EnumWaferState waferState, int cellIdx, bool isWaferStateChange = false)
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
        //public virtual EventCodeEnum SetResonOfError(string errorMsg)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        retVal = RaiseInvalidState();
        //    }
        //    catch (Exception err)
        //    {
        //        retVal = EventCodeEnum.UNDEFINED;
        //        LoggerManager.Exception(err);
        //    }
        //    return retVal;
        //}
       
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

        public virtual EventCodeEnum Wafer_MoveLoadingPosition()
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



    }

    public class IdleState : GP_ARMToChuckState
    {
        public IdleState(GP_ARMToChuck module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.ARM_TO_STAGE;
                Loader.ProcModuleInfo.Source = ARM.ID;
                Loader.ProcModuleInfo.Destnation = CHUCK.ID;
                Loader.ProcModuleInfo.Origin = ARM.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_ARMToChuckState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try 
            { 
                Loader.ChuckNumber = CHUCK.ID.Index;

                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} OriginHolder:{Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)} , DestinationHolder: {CHUCK}");

                if(Loader.LoaderMaster.ModuleState.State==ModuleStateEnum.RUNNING|| Loader.LoaderMaster.ModuleState.State == ModuleStateEnum.ABORT)
                {
                    if(Loader.LoaderMaster.GetStageMode(CHUCK.ID.Index) == GPCellModeEnum.MAINTENANCE)
                    {
                        LoggerManager.Debug($"GP_ARMToChuckState(): Error occurred. LOT State:{Loader.LoaderMaster.ModuleState.State}, Cell {CHUCK.ID.Index} Mode: MAINTENANCE");
                        StateTransition(new SystemErrorState(Module));
                        this.MetroDialogManager().ShowMessageDialog("Can not be Load", $"During LOT, Cell{CHUCK.ID.Index} is in maintenance mode.\r\nPlease LOT Resume again.", EnumMessageStyle.Affirmative);
                        return;
                    }
                }

                StateTransition(new WaitForRemotingState(Module));
             } 
            catch (Exception err)
            {
                LoggerManager.Error($"GP_ARMToChuckState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
}
    }
    public class PreChuckMoveState : GP_ARMToChuckState
    {
        public PreChuckMoveState(GP_ARMToChuck module) : base(module)
        {
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {

                //ARM.ValidateWaferStatus();

                //CHUCK.ValidateWaferStatus();

                //if (ARM.Holder.Status != EnumSubsStatus.EXIST)
                //{
                //    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is no Wafer on the ARM. ");
                //    Loader.ResonOfError = $"{Module.GetType().Name} There is no Wafer on the ARM.";
                //    StateTransition(new SystemErrorState(Module));
                //    return;
                //}

                //if (CHUCK.Holder.Status != EnumSubsStatus.NOT_EXIST)
                //{
                //    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is Wafer on the Chuck. ");
                //    Loader.ResonOfError = $"{Module.GetType().Name} There is Wafer on the Chuck.";
                //    StateTransition(new SystemErrorState(Module));
                //    return;
                //}

                var result = this.GetLoaderCommands().ChuckPut(Module.Param.UseARM, (IChuckModule)Module.Param.DestPos);

                if (result == EventCodeEnum.NONE)
                {
                    StateTransition(new DoneState(Module));
                }
                else
                {
                    // CHUCK.Holder.SetUnknown();
                    // ARM.Holder.SetUnknown();
                    LoggerManager.Error($"GP_ARMToChuckState(): Transfer failed. Job result = {result}");
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                //   CHUCK.Holder.SetUnknown();
                //  ARM.Holder.SetUnknown();
                LoggerManager.Error($"GP_ARMToChuckState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class RemotingState : GP_ARMToChuckState
    {
        public RemotingState(GP_ARMToChuck module) : base(module)
        {
            Loader.BroadcastLoaderInfo();
        }
        

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
        //public override EventCodeEnum SetResonOfError(string errorMsg)
        //{
        //    try
        //    {
        //        Loader.ResonOfError = "ArmToChuck Error.\n";
        //        Loader.ResonOfError += errorMsg;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return EventCodeEnum.NONE;
        //}

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
        public override EventCodeEnum Wafer_MoveLoadingPosition()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.GetLoaderCommands().ChuckMoveLoadingPosition((IChuckModule)Module.Param.DestPos, Module.Param.UseARM);

                if (retval == EventCodeEnum.NONE)
                {

                }
                else
                {
                    //CHUCK.Holder.SetUnknown();
                    //ARM.Holder.SetUnknown();
                    LoggerManager.Error($"Wafer_MoveLoadingPosition(): Transfer failed. Job result = {retval}");
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public override EventCodeEnum ChuckUpMove(int option = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            LoggerManager.ActionLog(ModuleLogType.ARM_TO_STAGE, StateLogType.START, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {ARM}, DestinationHolder: {CHUCK}");
            LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_STAGE, StateLogType.START, ARM.ID.Label, CHUCK.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label);

            retVal = WaitForPrechuckMove();

            if (retVal != EventCodeEnum.NONE)
            {
                Loader.ResonOfError = "ChuckUpMove Error. result:" + retVal.ToString();
                LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForPrechuckMove() ReturnValue={retVal}");
                Loader.ResonOfError = $"{Module.GetType().Name} WaitForPrechuck Motion Error";
                StateTransition(new SystemErrorState(Module));

                return retVal;
            }
            try
            {
                try
                {
                    retVal = this.Loader.SetTransferWaferSize(Module.Param.TransferObject, ARM.Holder.Status);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        retVal = this.GetLoaderCommands().ChuckPut(ARM, CHUCK);
                    }
                }
                catch(Exception err)
                {
                    LoggerManager.Exception(err);
                }



                if (retVal == EventCodeEnum.NONE)
                {
                    ARM.Holder.CurrentWaferInfo = ARM.Holder.TransferObject;

                    //ARM.Holder.SetTransfered(CHUCK);
                    //Loader.BroadcastLoaderInfo();
                }
                else if (retVal==EventCodeEnum.NODATA)
                {
                    Loader.ResonOfError = $"GP_ARMToChuckState() Nodata Error";
                    LoggerManager.Error($"GP_ARMToChuckState(): Transfer failed. Job result = {retVal}");
                    StateTransition(new SystemErrorState(Module));
                }
                else
                {
                    EventCodeEnum vacuumCheckRetVal= EventCodeEnum.UNDEFINED;
                   
                    Loader.ResonOfError = $"ARM{ARM.ID.Index} To CHUCK{CHUCK.ID.Index} Transfer failed. {Environment.NewLine} Job result = {retVal}";
                    //Loader.AddUnknownModule(CHUCK);
                    //Loader.AddUnknownModule(ARM);
                    vacuumCheckRetVal = ARM.MonitorForVacuum(true); //베큠을 체크해본다.
                    if (vacuumCheckRetVal != EventCodeEnum.NONE) // arm에 웨이퍼가 없을 경우
                    {
                        LoggerManager.Error($"GP_ARMToChuckState(): ArmVacuum Check: NoxExist. result:{vacuumCheckRetVal}");
                        ARM.Holder.SetTransfered(CHUCK);
                    }
                    Loader.BroadcastLoaderInfo();
                    LoggerManager.Error($"GP_ARMToChuckState(): Transfer failed. Job result = {retVal}");
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                EventCodeEnum vacuumCheckRetVal = EventCodeEnum.UNDEFINED;
                
                LoggerManager.ActionLog(ModuleLogType.ARM_TO_STAGE, StateLogType.ERROR, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {ARM}, DestinationHolder: {CHUCK}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_STAGE, StateLogType.ERROR, ARM.ID.Label, CHUCK.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label, errMsg: "Exception Occurred");

                Loader.ResonOfError = $"ARM{ARM.ID.Index} To CHUCK{CHUCK.ID.Index} Transfer failed. {Environment.NewLine} Job result = {err.Message}";
                vacuumCheckRetVal = ARM.MonitorForVacuum(true); //베큠을 체크해본다.
                if (vacuumCheckRetVal != EventCodeEnum.NONE) // arm에 웨이퍼가 없을 경우
                {
                    ARM.Holder.SetTransfered(CHUCK);
                }
                Loader.BroadcastLoaderInfo();
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

        public override EventCodeEnum WTR_Notifyhandlerholdwafer(bool ishandlerhold)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                CHUCK.Holder.IsWaferOnHandler = ishandlerhold;
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

        public override EventCodeEnum GetWaferLoadedObject(out TransferObject loadedObject)
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
    }


    public class WaitForRemotingState : GP_ARMToChuckState
    {
        public WaitForRemotingState(GP_ARMToChuck module) : base(module)
        {
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SUSPENDED;

        public override ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.LOAD;

        bool firstFlag = true;
        public override void Execute()
        {
            if (firstFlag)
            {
                Loader.BroadcastLoaderInfo();
                firstFlag = false;
            }
        }

        public override void Resume()
        {
            StateTransition(new RemotingState(Module));
        }

        public override EventCodeEnum GetWaferLoadedObject(out TransferObject loadedObject)
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
    }

    public class AbortState : GP_ARMToChuckState
    {
        public AbortState(GP_ARMToChuck module) : base(module)
        {
        }
        public override LoaderProcStateEnum State => LoaderProcStateEnum.ABORT;

        public override void Execute()
        {

        }

        public override void Resume()
        {
            StateTransition(new RemotingState(Module));
        }
    }
    public class DoneState : GP_ARMToChuckState
    {
        public DoneState(GP_ARMToChuck module) : base(module)
        {
            if (CHUCK.Holder.TransferObject != null)
            {
                LoggerManager.ActionLog(ModuleLogType.ARM_TO_STAGE, StateLogType.DONE, $"OriginHolder: {Loader.SlotToFoupConvert(CHUCK.Holder.TransferObject.OriginHolder)}, Source: {ARM}, DestinationHolder: {CHUCK}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_STAGE, StateLogType.DONE, ARM.ID.Label, CHUCK.ID.Label, CHUCK.Holder.TransferObject.OriginHolder.Label);
            }
            else
            {
                LoggerManager.ActionLog(ModuleLogType.ARM_TO_STAGE, StateLogType.DONE, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {ARM}, DestinationHolder: {CHUCK}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_STAGE, StateLogType.DONE, ARM.ID.Label, CHUCK.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label);
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_ARMToChuckState
    {
        public SystemErrorState(GP_ARMToChuck module) : base(module)
        {
            try
            {
                TransferObject transObj = null;

                if (ARM.Holder.TransferObject != null)
                {
                    transObj = ARM.Holder.TransferObject;
                }
                else
                {
                    transObj = CHUCK.Holder.TransferObject;
                }
                EventCodeEnum errorcode = EventCodeEnum.LOADER_ARM_TO_STAGE_TRANSFER_ERROR;
                LoggerManager.ActionLog(ModuleLogType.ARM_TO_STAGE, StateLogType.ERROR, $"OriginHolder: {Loader.SlotToFoupConvert(transObj.OriginHolder)}, Source: {ARM}, DestinationHolder: {CHUCK}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_STAGE, StateLogType.ERROR, ARM.ID.Label, CHUCK.ID.Label, transObj.OriginHolder.Label, errMsg: errorcode.ToString());
                this.NotifyManager().Notify(errorcode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}
