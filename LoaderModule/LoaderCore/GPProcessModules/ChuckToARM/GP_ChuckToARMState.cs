using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using System;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using LoaderParameters;
using System.Threading;
using NotifyEventModule;
using ProberInterfaces.Event;
using MetroDialogInterfaces;
using System.Linq;
using System.Collections.Generic;

namespace LoaderCore.GP_ChuckToARMStates
{
    public abstract class GP_ChuckToARMState : LoaderProcStateBase
    {
        public GP_ChuckToARM Module { get; set; }

        public GP_ChuckToARMState(GP_ChuckToARM module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_ChuckToARMState stateObj)
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

        protected IChuckModule CHUCK => Module.Param.Curr as IChuckModule;

        protected IARMModule ARM => Module.Param.Next as IARMModule;


        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
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

        public virtual EventCodeEnum Wafer_MoveLoadingPosition()
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

        public virtual EventCodeEnum NotifyTransferObject(TransferObject transferobj)
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


        public virtual EventCodeEnum NotifyUnloadedFromThreeLeg(EnumWaferState waferState, int cellIdx, bool isWaferStageChange = false)
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
        public virtual EventCodeEnum WTR_Notifyhandlerholdwafer(bool ishandlerhold)
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




    }

    public class IdleState : GP_ChuckToARMState
    {
        public IdleState(GP_ChuckToARM module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.STAGE_TO_ARM;
                Loader.ProcModuleInfo.Source = CHUCK.ID;
                Loader.ProcModuleInfo.Destnation = ARM.ID;
                Loader.ProcModuleInfo.Origin = CHUCK.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_ChuckToARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                //if (CHUCK.Holder.TransferObject.SlotNotchAngle.Value != CHUCK.Holder.TransferObject.ChuckNotchAngle.Value)// PathGenerator에서 해줘야하는일.
                //{
                //    CHUCK.Holder.TransferObject.CleanPreAlignState(reason: $"SlotNotchAngle({CHUCK.Holder.TransferObject.SlotNotchAngle.Value}) and ChuckNotchAngle({CHUCK.Holder.TransferObject.ChuckNotchAngle.Value}) is not same");
                //}


                Loader.ChuckNumber = CHUCK.ID.Index;
                
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} OriginHolder:{CHUCK.Holder.TransferObject.OriginHolder.ToString()} , Chuck Index:{Loader.ChuckNumber}, DestinationHolder: {ARM.ToString()}");

                if (Loader.LoaderMaster.ModuleState.State == ModuleStateEnum.RUNNING || Loader.LoaderMaster.ModuleState.State == ModuleStateEnum.ABORT)
                {
                    if (Loader.LoaderMaster.GetStageMode(CHUCK.ID.Index) == GPCellModeEnum.MAINTENANCE)
                    {
                        LoggerManager.Debug($"GP_ChuckToARMState(): Error occurred. LOT State:{Loader.LoaderMaster.ModuleState.State}, Cell {CHUCK.ID.Index} Mode: MAINTENANCE");
                        StateTransition(new SystemErrorState(Module));
                        this.MetroDialogManager().ShowMessageDialog("Can not be Unload", $"During LOT, Cell{CHUCK.ID.Index} is in maintenance mode.\r\nPlease LOT Resume again.", EnumMessageStyle.Affirmative);
                        return;
                    }
                }
                StateTransition(new WaitForRemotingState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_ChuckToARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }


    public class PreChuckDownMoveState : GP_ChuckToARMState
    {
        public PreChuckDownMoveState(GP_ChuckToARM module) : base(module) { }

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
                CHUCK.Holder.TransferObject.CleanPreAlignState(reason: "PreChuck DownMove");

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

    public class WaitForRemotingState : GP_ChuckToARMState
    {
        public WaitForRemotingState(GP_ChuckToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SUSPENDED;

        public override ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.UNLOAD;

        bool firstFlag = true;
        public override void Execute()
        {
            try
            {
                if (firstFlag)
                {
                    Loader.BroadcastLoaderInfo();
                    firstFlag = false;
                }
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

    public class RemotingState : GP_ChuckToARMState
    {
        public RemotingState(GP_ChuckToARM module) : base(module) {
            Loader.BroadcastLoaderInfo();
        }

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
                    Loader.ResonOfError = "ChuckTo Arm Error. ";
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        //public override EventCodeEnum SetResonOfError(string errorMsg)
        //{
        //    try
        //    {
        //        Loader.ResonOfError = "ChuckToArm Error.\n";
        //        Loader.ResonOfError += errorMsg;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return EventCodeEnum.NONE;
        //}
        //public override EventCodeEnum WriteVacuum(bool value)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        retval = ARM.WriteVacuum(value);
        //        LoggerManager.Debug($"[LOADER] {Module.GetType().Name} WriteVacuum({value}) ReturnValue={retval}");
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //public override EventCodeEnum MonitorForVacuum(bool value)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        retval = ARM.MonitorForVacuum(value);
        //        LoggerManager.Debug($"[LOADER] {Module.GetType().Name} MonitorForVacuum({value}) ReturnValue={retval}");
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //public override EventCodeEnum WaitForVacuum(bool value)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        retval = ARM.WaitForVacuum(value);
        //        LoggerManager.Debug($"[LOADER] {Module.GetType().Name} WaitForVacuum() ReturnValue={retval}");
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        public override EventCodeEnum ChuckDownMove(int option = 0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.ActionLog(ModuleLogType.STAGE_TO_ARM, StateLogType.START, $"OriginHolder: {Loader.SlotToFoupConvert(CHUCK.Holder.TransferObject.OriginHolder)}, Source: {CHUCK}, DestinationHolder: {ARM}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.STAGE_TO_ARM, StateLogType.START, CHUCK.ID.Label, ARM.ID.Label, CHUCK.Holder.TransferObject.OriginHolder.Label);
                retval = this.Loader.SetTransferWaferSize(Module.Param.TransferObject, CHUCK.Holder.Status);
                if (retval == EventCodeEnum.NONE)
                {
                    retval = this.GetLoaderCommands().ChuckPick(CHUCK, ARM, option);
                }
                
                if (retval == EventCodeEnum.NONE)
                {

                }
                else
                {
                    retval = ARM.MonitorForVacuum(true); //arm 베큠 체크

                    if (retval == EventCodeEnum.NONE) // arm 에 웨이퍼가 있을 경우
                    {
                        if (CHUCK.Holder.Status == EnumSubsStatus.EXIST)
                        {
                            CHUCK.Holder.SetTransfered(ARM); // 있으면 Arm
                        }
                    }
                    Loader.BroadcastLoaderInfo();
                    LoggerManager.Error($"GP_ChuckToARMState(): Transfer failed. Job result = {retval}");
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                 retval = ARM.MonitorForVacuum(true); //arm 베큠 체크

                if (retval == EventCodeEnum.NONE) // arm 에 웨이퍼가 있을 경우
                {
                    if (CHUCK.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        CHUCK.Holder.SetTransfered(ARM); // 있으면 Arm
                    }
                }
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public override EventCodeEnum Wafer_MoveLoadingPosition()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.GetLoaderCommands().ChuckMoveLoadingPosition(CHUCK, ARM);
                if (retval == EventCodeEnum.NONE)
                {

                }
                else
                {
                    //   CHUCK.Holder.SetUnknown();
                    //   ARM.Holder.SetUnknown();
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
        public override EventCodeEnum NotifyTransferObject(TransferObject transferobj)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (CHUCK.Holder.TransferObject.PolishWaferInfo != null)
                {
                    CHUCK.Holder.TransferObject.PolishWaferInfo.CurrentAngle.Value = transferobj.PolishWaferInfo.CurrentAngle.Value;
                    CHUCK.Holder.TransferObject.PolishWaferInfo.Priorty.Value = transferobj.PolishWaferInfo.Priorty.Value;
                    CHUCK.Holder.TransferObject.PolishWaferInfo.TouchCount.Value = transferobj.PolishWaferInfo.TouchCount.Value;

                    if (CHUCK.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                    {
                        var loadablePolishWafers = Loader.ModuleManager.GetTransferObjectAll().Where(
                            item => item.WaferType.Value == EnumWaferType.POLISH &&
                             (item.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY ||
                                 item.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY) &&
                                 item.PolishWaferInfo != null &&
                                 item.PolishWaferInfo.DefineName.Value == transferobj.PolishWaferInfo.DefineName.Value
                            ).OrderBy(i => i.PolishWaferInfo.Priorty.Value).ThenBy(i => i.OriginHolder.Index).ToList();

                        LoggerManager.Debug($"PolishWafer Priority Sorting. Polish Define Name:{transferobj.PolishWaferInfo.DefineName.Value}");

                        for (int i = 0; i < loadablePolishWafers.Count(); i++)
                        {
                            LoggerManager.Debug($"Priority:{i}. OriginHolder:{loadablePolishWafers[i].OriginHolder}, CurrentHolder:{loadablePolishWafers[i].CurrHolder}, Prev Priority:{loadablePolishWafers[i].PolishWaferInfo.Priorty.Value }");
                            loadablePolishWafers[i].PolishWaferInfo.Priorty.Value = i;
                        }

                        LoggerManager.Debug($"Reassigning Priority of Polish Wafers.");
                    }
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"[RemotingState], NotifyTransferObject() : PolishWaferInfo is Null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum NotifyUnloadedFromThreeLeg(EnumWaferState waferState,int cellIdx, bool isWaferStateChange = true)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (isWaferStateChange)
                {
                    CHUCK.Holder.TransferObject.WaferState = waferState;
                    CHUCK.Holder.TransferObject.ProcessCellIndex = cellIdx;
                }
       
                CHUCK.Holder.CurrentWaferInfo = CHUCK.Holder.TransferObject;
                CHUCK.Holder.TransferObject.NotchAngle.Value = CHUCK.Holder.TransferObject.SlotNotchAngle.Value;
                CHUCK.Holder.SetTransfered(ARM);

                Loader.BroadcastLoaderInfo();

                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} NotifyUnloadedFromThreeLeg()");


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

    }
    public class RunningState : GP_ChuckToARMState
    {
        public RunningState(GP_ChuckToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                var result = this.GetLoaderCommands().ChuckPick((IChuckModule)Module.Param.Curr, Module.Param.UseARM);
                if (result == EventCodeEnum.NONE)
                {
                    StateTransition(new DoneState(Module));
                }
                else
                {
                    CHUCK.Holder.SetUnknown();
                    ARM.Holder.SetUnknown();
                    LoggerManager.Error($"GP_ChuckToARMState(): Transfer failed. Job result = {result}");
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                CHUCK.Holder.SetUnknown();
                ARM.Holder.SetUnknown();
                LoggerManager.Error($"GP_ChuckToARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class DoneState : GP_ChuckToARMState
    {
        public DoneState(GP_ChuckToARM module) : base(module)
        {
            if (ARM.Holder.TransferObject != null)
            {
                LoggerManager.ActionLog(ModuleLogType.STAGE_TO_ARM, StateLogType.DONE, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {CHUCK}, DestinationHolder: {ARM}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.STAGE_TO_ARM, StateLogType.DONE, CHUCK.ID.Label, ARM.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label);
            }
            else
            {
                LoggerManager.ActionLog(ModuleLogType.STAGE_TO_ARM, StateLogType.DONE, $"OriginHolder: {Loader.SlotToFoupConvert(CHUCK.Holder.TransferObject.OriginHolder)}, Source: {CHUCK}, DestinationHolder: {ARM}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.STAGE_TO_ARM, StateLogType.DONE, CHUCK.ID.Label, ARM.ID.Label, CHUCK.Holder.TransferObject.OriginHolder.Label);
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_ChuckToARMState
    {
        public SystemErrorState(GP_ChuckToARM module) : base(module)
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
                Loader.LoaderMaster.NotifyManager().Notify(EventCodeEnum.STAGE_TO_ARM);

                EventCodeEnum errorcode = EventCodeEnum.LOADER_STAGE_TO_ARM_TRANSFER_ERROR;
                LoggerManager.ActionLog(ModuleLogType.STAGE_TO_ARM, StateLogType.ERROR, $"OriginHolder: {Loader.SlotToFoupConvert(transObj.OriginHolder)}, Source: {CHUCK}, DestinationHolder: {ARM}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.STAGE_TO_ARM, StateLogType.ERROR, CHUCK.ID.Label, ARM.ID.Label, transObj.OriginHolder.Label, errMsg: errorcode.ToString());


                if (Loader.LoaderMaster.DynamicMode == DynamicModeEnum.DYNAMIC)
                {
                    int foupNum = 0;
                    if(transObj.OriginHolder.ModuleType==ModuleTypeEnum.SLOT)
                    {
                        var slotCount = 25;
                        int slotNum = transObj.OriginHolder.Index % slotCount;
                        int offset = 0;
                        if (slotNum == 0)
                        {
                            slotNum = 25;
                            offset = -1;
                        }
                        foupNum = ((transObj.OriginHolder.Index + offset) / slotCount) + 1;
                    }
                    PIVInfo pivinfo = new PIVInfo(foupnumber: foupNum, lotid: transObj.LOTID, waferid: transObj.OCR.Value);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(WaferUnloadedFailToSlotEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                }
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
