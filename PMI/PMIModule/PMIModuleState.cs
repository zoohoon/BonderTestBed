using LogModule;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Enum;
using ProberInterfaces.Event;
using ProberInterfaces.PMI;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PMIModule
{
    public abstract class PMIModuleState : IInnerState
    {


        public abstract EventCodeEnum Execute();

        public abstract EventCodeEnum Pause();

        public abstract PMIStateEnum GetState();

        public abstract ModuleStateEnum GetModuleState();

        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.NONE;
        }

        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }

        //public virtual EventCodeEnum ClearState()
        //{
        //    return EventCodeEnum.NONE;
        //}
        public abstract EventCodeEnum ClearState();

        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }
    }
    public abstract class PMIModuleStateBase : PMIModuleState
    {
        private PMIModule _Module;

        public PMIModule Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        public PMIModuleStateBase(PMIModule module)
        {
            Module = module;
        }

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.InnerStateTransition(new PMIIdleState(Module));

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private bool _UpdateLogUploadListFlag;

        public bool UpdateLogUploadListFlag
        {
            get { return _UpdateLogUploadListFlag; }
            set { _UpdateLogUploadListFlag = value; }
        }
    }

    public class PMIIdleState : PMIModuleStateBase
    {
        public PMIIdleState(PMIModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                List<ISubModule> modules = Module.Template.GetProcessingModule();

                if (Module.Template.SchedulingModule.IsExecute())
                {
                    foreach (var subModule in modules)
                    {
                        if (subModule.IsExecute())
                        {
                            Module.InnerStateTransition(new PMIRunningState(Module));
                            break;
                        }
                    }
                }

                //    if (PMIModule.SubModules.SchedulingModule.IsExecute())
                //{
                //    foreach (var subModule in PMIModule.SubModules.SubModules)
                //    {
                //        if (subModule.IsExecute())
                //        {
                //            PMIModule.InnerStateTransition(new PMIRunningState(PMIModule));
                //            break;
                //        }
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override PMIStateEnum GetState()
        {
            return PMIStateEnum.IDLE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.PAUSED_BY_OTHERS;
                Module.InnerStateTransition(new PMIPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.PMIState.GetType().Name)));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class PMIRunningState : PMIModuleStateBase
    {
        public PMIRunningState(PMIModule module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.PMI, StateLogType.START, $"", this.Module.LoaderController().GetChuckIndex());
            UpdateLogUploadListFlag = true;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Module.EventManager().RaisingEvent(typeof(PMIStartEvent).FullName, new ProbeEventArgs(this, semaphore));
                semaphore.Wait();

                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new PMIDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    return EventCodeEnum.NONE;
                }

                List<ISubModule> modules = Module.Template.GetProcessingModule();

                foreach (var subModule in modules)
                {
                    retVal = subModule.ClearData();
                    retVal = subModule.Execute();

                    if (subModule.GetState() == SubModuleStateEnum.DONE)
                    {
                        if (retVal == EventCodeEnum.NONE)
                        {
                            Module.InnerStateTransition(new PMIDoneState(Module));
                        }
                        else if (retVal == EventCodeEnum.PMI_PAUSE_IMMEDIATELY)
                        {
                            Module.NotifyManager().Notify(EventCodeEnum.PMI_FAIL);
                            LoggerManager.ActionLog(ModuleLogType.PMI, StateLogType.PAUSE, $"", this.Module.LoaderController().GetChuckIndex());

                            string ErrMessage = string.Empty;

                            if (Module.DoPMIInfo.LastPMIDieResult != null && Module.DoPMIInfo.LastPMIDieResult.UI != null)
                            {
                                long ui_x = Module.DoPMIInfo.LastPMIDieResult.UI.XIndex;
                                long ui_y = Module.DoPMIInfo.LastPMIDieResult.UI.YIndex;

                                ErrMessage = $"It was caused by PMI PAUSE parameter.\n" + $"User Index X = {ui_x}, Y = {ui_y}, Failed Pad Count = {Module.DoPMIInfo.LastPMIDieResult.FailPadCount}";
                            }
                            else
                            {
                                ErrMessage = $"It was caused by PMI PAUSE parameter.";
                            }

                            Module.InnerStateTransition(new PMIPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, ErrMessage, Module.PMIState.GetType().Name)));
                        }
                        else if (retVal == EventCodeEnum.PMI_PAUSE_AFTER_ALL_DIE_INSPECTION)
                        {
                            Module.NotifyManager().Notify(EventCodeEnum.PMI_FAIL);
                            LoggerManager.ActionLog(ModuleLogType.PMI, StateLogType.PAUSE, $"", this.Module.LoaderController().GetChuckIndex());

                            string ErrMessage = string.Empty;

                            ErrMessage = $"It was caused by PMI PAUSE parameter.\n" + $"Inspected die count : {this.Module.DoPMIInfo.ProcessedPMIMIndex.Count}\n"
                                                                                    + $"Total pad count : {this.Module.DoPMIInfo.AllPassPadCount + this.Module.DoPMIInfo.AllFailPadCount}\n"
                                                                                    + $"Pass : {this.Module.DoPMIInfo.AllPassPadCount}\n"
                                                                                    + $"Fail : {this.Module.DoPMIInfo.AllFailPadCount}";

                            Module.InnerStateTransition(new PMIPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, ErrMessage, Module.PMIState.GetType().Name)));
                        }
                        else
                        {
                            Module.NotifyManager().Notify(EventCodeEnum.PMI_FAIL);
                            Module.InnerStateTransition(new PMIErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, retVal.ToString(), Module.PMIState.GetType().Name)));
                        }
                    }
                    else if (subModule.GetState() == SubModuleStateEnum.ERROR)
                    {
                        if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                        {
                            Module.InnerStateTransition(new PMIDoneState(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");

                            retVal = EventCodeEnum.NONE;
                            break;
                        }
                        else
                        {
                            Module.InnerStateTransition(new PMIErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, retVal.ToString(), Module.PMIState.GetType().Name)));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");

                            retVal = EventCodeEnum.NONE;
                            break;
                        }
                    }

                    if (Module.PMIState.GetModuleState() != ModuleStateEnum.RUNNING)
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                Module.DoPMIInfo.Result = retVal;
                
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Module.EventManager().RaisingEvent(typeof(PMIEndEvent).FullName, new ProbeEventArgs(this, semaphore));
                semaphore.Wait();
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override PMIStateEnum GetState()
        {
            return PMIStateEnum.RUNNING;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class PMISuspendState : PMIModuleStateBase
    {
        public PMISuspendState(PMIModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.CommandSendSlot.GetState() == CommandStateEnum.DONE)
                {
                    Module.InnerStateTransition(new PMIRunningState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }

        public override PMIStateEnum GetState()
        {
            return PMIStateEnum.SUSPENDED;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class PMIAbortState : PMIModuleStateBase
    {
        public PMIAbortState(PMIModule module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new PMIIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }

        public override PMIStateEnum GetState()
        {
            return PMIStateEnum.ABORT;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }

    public class PMIPauseState : PMIModuleStateBase
    {
        public PMIPauseState(PMIModule module, EventCodeInfo eventcode) : base(module)
        {
            if (this.GetModuleState() == ModuleStateEnum.PAUSED)
            {
                Module.ReasonOfError.AddEventCodeInfo(eventcode.EventCode, eventcode.Message, eventcode.Caller);
            }
            else
            {
                LoggerManager.Debug($"[{this.GetType().Name}] Current State = {this.GetModuleState()}, Can not add ReasonOfError.");
            }

            if (UpdateLogUploadListFlag)
            {
                UpdateLogUploadListFlag = false;
                this.Module.LoaderController().UpdateLogUploadList(EnumUploadLogType.PMI);
            }
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override PMIStateEnum GetState()
        {
            return PMIStateEnum.PAUSED;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.PreInnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                {
                    Module.InnerStateTransition(Module.PreInnerState);
                }
                else
                {
                    Module.InnerStateTransition(new PMIIdleState(Module));
                }

                //if (Module.PreInnerState.GetModuleState() == ModuleStateEnum.ERROR ||
                //    Module.PreInnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                //{
                //    Module.InnerStateTransition(new PMIIdleState(Module));
                //}
                //else
                //{
                //    Module.InnerStateTransition(Module.PreInnerState);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Module.InnerStateTransition(new PMIAbortState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class PMIDoneState : PMIModuleStateBase
    {
        public PMIDoneState(PMIModule module) : base(module)
        {
            try
            {
                LoggerManager.ActionLog(ModuleLogType.PMI, StateLogType.DONE,
                                $"Pass Pad Count: {Module.DoPMIInfo.AllPassPadCount}, " +
                                $"Fail Pad Count: {Module.DoPMIInfo.AllFailPadCount}",
                                this.Module.LoaderController().GetChuckIndex());
                this.Module.LoaderController().UpdateLogUploadList(EnumUploadLogType.PMI);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override EventCodeEnum Execute()
        {

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new PMIIdleState(Module));
                }

                if (Module.ProbingModule().ModuleState.GetState() != ModuleStateEnum.SUSPENDED)
                {
                    // 프로빙이 재게되면, 이전 결과를 지운다.
                    //Module.DoPMIInfo.ProcessedPMIMIndex.Clear();
                    Module.InnerStateTransition(new PMIIdleState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }

        public override PMIStateEnum GetState()
        {
            return PMIStateEnum.DONE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.PAUSED_BY_OTHERS;
                Module.InnerStateTransition(new PMIPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.PMIState.GetType().Name)));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class PMIErrorState : PMIModuleStateBase
    {
        public PMIErrorState(PMIModule module, EventCodeInfo eventcode) : base(module)
        {
            if (this.GetModuleState() == ModuleStateEnum.ERROR)
            {
                Module.ReasonOfError.AddEventCodeInfo(eventcode.EventCode, eventcode.Message, eventcode.Caller);
            }
            else
            {
                LoggerManager.Debug($"[{this.GetType().Name}] Current State = {this.GetModuleState()}, Can not add ReasonOfError.");
            }

            LoggerManager.ActionLog(ModuleLogType.PMI, StateLogType.ERROR, $"", this.Module.LoaderController().GetChuckIndex());

            if (UpdateLogUploadListFlag)
            {
                UpdateLogUploadListFlag = false;
                this.Module.LoaderController().UpdateLogUploadList(EnumUploadLogType.PMI);
            }
        }
        public override EventCodeEnum Execute()
        {
            return EventCodeEnum.NODATA;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override PMIStateEnum GetState()
        {
            return PMIStateEnum.ERROR;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.PAUSED_BY_OTHERS;
                Module.InnerStateTransition(new PMIPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.PMIState.GetType().Name)));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.InnerStateTransition(new PMIIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.InnerStateTransition(new PMIIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }
    }
}
