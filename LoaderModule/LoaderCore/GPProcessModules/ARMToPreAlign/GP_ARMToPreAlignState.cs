using Autofac;
using LogModule;
using ProberErrorCode;
using System;
using System.Runtime.CompilerServices;
using ProberInterfaces;

namespace LoaderCore.GP_ARMToPreAlignStates
{
    using LoaderBase;
    using NotifyEventModule;
    using ProberInterfaces.Event;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class GP_ARMToPreAlignState : LoaderProcStateBase
    {
        public GP_ARMToPreAlign Module { get; set; }

        public GP_ARMToPreAlignState(GP_ARMToPreAlign module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_ARMToPreAlignState stateObj)
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

        protected IPreAlignModule PreAlign => Module.Param.Next as IPreAlignModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_ARMToPreAlignState
    {
        public IdleState(GP_ARMToPreAlign module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.ARM_TO_PREALIGN;
                Loader.ProcModuleInfo.Source = ARM.ID;
                Loader.ProcModuleInfo.Destnation = PreAlign.ID;
                Loader.ProcModuleInfo.Origin = ARM.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_ARMToPreAlignState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} OriginHolder:{ARM.Holder.TransferObject.OriginHolder.ToString()} , DestinationHolder: {PreAlign.ToString()}");
                StateTransition(new RunningState(Module));
            }
            catch(Exception err)
            {
                LoggerManager.Error($"GP_ARMToPreAlignState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class RunningState : GP_ARMToPreAlignState
    {
        public RunningState(GP_ARMToPreAlign module) : base(module)
        {
            Loader.LoaderMaster.LoaderLogManager.UpdateLogUploadListForLoader(EnumUploadLogType.LoaderOCR);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                LoggerManager.ActionLog(ModuleLogType.ARM_TO_PREALIGN, StateLogType.START, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {ARM}, DestinationHolder: {PreAlign}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_PREALIGN, StateLogType.START, ARM.ID.Label, PreAlign.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label);

                var targetPAAngle = Module.Param.DestPos as IHasLoadNotchAngle;
                var loaderMaster = Module.Container.Resolve<ILoaderSupervisor>();

                if (loaderMaster.ModuleState.State != ModuleStateEnum.RUNNING)//TODO: IDLE일때만 GP_ManualOCREnable 에 따라서 동작해야하는 것 아닐까?
                {
                    if ((this.Loader as LoaderModule).GP_ManualOCREnable)
                    {
                        ARM.Holder.TransferObject.OCRMode.Value = ProberInterfaces.Enum.OCRModeEnum.READ; //Lot 중이아니면 무조건 MANUAL                                                
                    }
                    else
                    {
                        if (ARM.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                        {
                            if (loaderMaster.HostInitiatedWaferChangeInProgress == true)
                            {
                                //ARM.Holder.TransferObject.OCRMode.Value 값을 사용 하도록 한다.
                            }
                            else
                            {
                                ARM.Holder.TransferObject.OCRMode.Value = ProberInterfaces.Enum.OCRModeEnum.NONE;
                            }
                        }
                        else
                        {
                            ARM.Holder.TransferObject.OCRMode.Value = ProberInterfaces.Enum.OCRModeEnum.DEBUGGING;
                        }
                    }
                }
                else
                {
                    if (ARM.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                    {
                        if (loaderMaster.HostInitiatedWaferChangeInProgress == true)
                        {
                            //ARM.Holder.TransferObject.OCRMode.Value 값을 사용 하도록 한다.
                        }
                        else
                        {
                            ARM.Holder.TransferObject.OCRMode.Value = ProberInterfaces.Enum.OCRModeEnum.NONE;
                        }
                    }
                    else if (ARM.Holder.TransferObject.WaferType.Value == EnumWaferType.TCW)
                    {
                        // TCW의 경우 OCR을 읽지 않도록 하기 위함.
                        ARM.Holder.TransferObject.OCRMode.Value = ProberInterfaces.Enum.OCRModeEnum.DEBUGGING;
                    }
                    else
                    {
                        if (loaderMaster.Loader.OCRConfig.Mode == OCR_OperateMode.DEBUG)
                        {
                            ARM.Holder.TransferObject.OCRMode.Value = ProberInterfaces.Enum.OCRModeEnum.DEBUGGING;
                        }
                        else if (loaderMaster.Loader.OCRConfig.Mode == OCR_OperateMode.MANUAL_INPUT)
                        {
                            ARM.Holder.TransferObject.OCRMode.Value = ProberInterfaces.Enum.OCRModeEnum.MANUAL;
                        }
                        else
                        {
                            //NG_GO, READ
                            ARM.Holder.TransferObject.OCRMode.Value = ProberInterfaces.Enum.OCRModeEnum.READ;
                        }
                    }
                }

                EventCodeEnum result;

                this.Loader.PAManager.PAModules[((IPreAlignModule)Module.Param.DestPos).ID.Index - 1].State.Busy = true;
                this.Loader.PAManager.PAModules[((IPreAlignModule)Module.Param.DestPos).ID.Index - 1].State.AlignDone = false;
                this.Loader.PAManager.PAModules[((IPreAlignModule)Module.Param.DestPos).ID.Index - 1].State.PAAlignAbort = false;
                ARM.Holder.TransferObject.CleanPreAlignState(reason: "Manual Handling");
                
                result = this.Loader.SetTransferWaferSize(Module.Param.TransferObject, ARM.Holder.Status);
                if (result != EventCodeEnum.NONE)
                {
                    // PA의 Wafer obj 에 정보 이상일 수 있음.
                }
                else 
                {
                    if ((this.Loader.LoaderMaster.ModuleState.GetState() == ProberInterfaces.ModuleStateEnum.RUNNING ||
                         this.Loader.LoaderMaster.ModuleState.GetState() == ProberInterfaces.ModuleStateEnum.ABORT) &&
                         this.Loader.LoaderMaster.HostInitiatedWaferChangeInProgress == false)
                    {
                        result = this.GetLoaderCommands().PAPutAync(Module.Param.UseARM, (IPreAlignModule)Module.Param.DestPos);
                    }
                    else
                    {
                        result = this.GetLoaderCommands().PAPut(Module.Param.UseARM, (IPreAlignModule)Module.Param.DestPos);
                    }
                }

                if (result == EventCodeEnum.NONE)
                {
                    // ARM.Holder.CurrentWaferInfo = ARM.Holder.TransferObject;
                    // ARM.Holder.SetTransfered(PreAlign);
                    // Loader.BroadcastLoaderInfo();

                    StateTransition(new DoneState(Module));
                }
                else
                {
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    Loader.LoaderMaster.EventManager().RaisingEvent(typeof(PreAlignFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                    semaphore.Wait();

                    LoggerManager.Error($"GP_ARMToPreAlignState(): Transfer failed. Job result = {result}");

                    Loader.ResonOfError = $"ARM{ARM.ID.Index} To PreAlign{PreAlign.ID.Index} Transfer failed. {Environment.NewLine} Job result = {result}";
                    result = ARM.MonitorForVacuum(true); //베큠을 체크해본다.
                    if (result != EventCodeEnum.NONE) // arm에 웨이퍼가 없을 경우
                    {
                        bool isExist = false; //PA 베큠체크
                        result = Loader.GetLoaderCommands().CheckWaferIsOnPA(PreAlign.ID.Index, out isExist);
                        if (result == EventCodeEnum.NONE) // arm에 웨이퍼가 없을 경우
                        {
                            if (isExist == false)
                            {
                                var clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                                ARM.Holder.SetUnknown();
                                PreAlign.Holder.SetUnknown(clonedTransferObject);
                            }
                            else
                            {
                                if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                                {
                                    ARM.Holder.SetTransfered(PreAlign); // 있으면 PreAlign
                                }
                            }
                        }
                        else
                        {
                            ARM.Holder.SetUnknown();
                            var clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                            PreAlign.Holder.SetUnknown(clonedTransferObject);
                        }
                    }
                    else 
                    {
                        if (PreAlign.Holder.Status == EnumSubsStatus.EXIST)
                        {
                            PreAlign.Holder.SetTransfered(ARM);
                        }
                    }

                    Loader.BroadcastLoaderInfo();

                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                TransferObject clonedTransferObject = null;
                if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                }
                else if (PreAlign.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = PreAlign.Holder.TransferObject.Clone() as TransferObject;
                }
                LoggerManager.Error($"GP_ARMToPreAlignState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                Loader.ResonOfError = $"ARM{ARM.ID.Index} To PreAlign{PreAlign.ID.Index} Transfer failed. {Environment.NewLine} Job result = {err.Message}";
                var result = ARM.MonitorForVacuum(true); //베큠을 체크해본다.
                if (result != EventCodeEnum.NONE) // arm에 웨이퍼가 없을 경우
                {
                    bool isExist = false; //PA 베큠체크
                    result = Loader.GetLoaderCommands().CheckWaferIsOnPA(PreAlign.ID.Index, out isExist);
                    if (result == EventCodeEnum.NONE)
                    {
                        if (isExist == false)
                        {
                            ARM.Holder.SetUnknown(clonedTransferObject);
                            PreAlign.Holder.SetUnknown(clonedTransferObject);
                        }
                        else
                        {
                            if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                            {
                                ARM.Holder.SetTransfered(PreAlign); // 있으면 PreAlign
                            }
                        }
                    }
                    else
                    {
                        ARM.Holder.SetUnknown(clonedTransferObject);
                        PreAlign.Holder.SetUnknown(clonedTransferObject);
                    }
                }
                else
                {
                    if (PreAlign.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        PreAlign.Holder.SetTransfered(ARM);
                    }
                }
                Loader.BroadcastLoaderInfo();
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class DoneState : GP_ARMToPreAlignState
    {
        public DoneState(GP_ARMToPreAlign module) : base(module)
        {
            TransferObject Transfer = null;
            if (ARM.Holder.TransferObject!=null)
            {
                Transfer = ARM.Holder.TransferObject;
            }
            else
            {
                Transfer = PreAlign.Holder.TransferObject;
            }

            LoggerManager.ActionLog(ModuleLogType.ARM_TO_PREALIGN, StateLogType.DONE, $"OriginHolder: {Loader.SlotToFoupConvert(Transfer.OriginHolder)}, Source: {ARM}, DestinationHolder: {PreAlign}");
            LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_PREALIGN, StateLogType.DONE, ARM.ID.Label, PreAlign.ID.Label, Transfer.OriginHolder.Label, ocr: Transfer.OCR.Value);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_ARMToPreAlignState
    {
        public SystemErrorState(GP_ARMToPreAlign module) : base(module)
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
                    transObj = PreAlign.Holder.TransferObject;
                }
                EventCodeEnum errorcode = EventCodeEnum.LOADER_ARM_TO_PREALIGN_TRANSFER_ERROR;
                LoggerManager.ActionLog(ModuleLogType.ARM_TO_PREALIGN, StateLogType.ERROR, $"OriginHolder: {Loader.SlotToFoupConvert(transObj.OriginHolder)}, Source: {ARM}, DestinationHolder: {PreAlign}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_PREALIGN, StateLogType.ERROR, ARM.ID.Label, PreAlign.ID.Label, transObj.OriginHolder.Label, errMsg: errorcode.ToString());
                this.NotifyManager().Notify(errorcode);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}
