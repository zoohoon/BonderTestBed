using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.FDAlign;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDWaferAlign
{
    public abstract class FDWaferAlignState : IFactoryModule, IInnerState
    {
        public abstract bool CanExecute(IProbeCommandToken token);
        public abstract FDWaferAlignInnerStateEnum GetState();
        public abstract ModuleStateEnum GetModuleState();
        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();
        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.UNDEFINED;
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
        public abstract EventCodeEnum ClearState();
        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }
    }

    public abstract class FDWaferAlignStateBase : FDWaferAlignState
    {
        private FDWaferAligner _Module;
        public FDWaferAligner Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            var isInjected = false;
            return isInjected;
        }
        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new FDWaferAlignIdleState(Module));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public FDWaferAlignStateBase(FDWaferAligner module)
        {
            Module = module;
        }
        public void ChangeRunningState()
        {
            try
            {
                Module.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);
                Module.InnerStateTransition(new FDWaferAlignRunningState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CheckCommands()
        {
            try
            {
                bool consumed = false;

                Func<bool> conditionFunc = () =>
                {
                    if (this.StageSupervisor().Get_TCW_Mode() == TCW_Mode.OFF)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                };
                Action doAction = () => { ChangeRunningState(); };
                Action abortAction = () => { };

                if (this.GetModuleState() == ModuleStateEnum.IDLE || this.GetModuleState() == ModuleStateEnum.PAUSED)
                {
                    //consumed = this.CommandManager().ProcessIfRequested<IDoManualWaferAlign>(Module, conditionFunc, doAction, abortAction);
                }

                consumed = this.CommandManager().ProcessIfRequested<IDOFDWAFERALIGN>(Module, conditionFunc, doAction, abortAction);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
    public class FDWaferAlignIdleState : FDWaferAlignStateBase
    {
        public FDWaferAlignIdleState(FDWaferAligner module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.Template != null)
                {
                    List<ISubModule> modules = Module.Template.GetProcessingModule();

                    if (modules != null)
                    {
                        if (Module.Template.SchedulingModule.IsExecute())
                        {
                            bool isExecute = false;

                            foreach (var subModule in modules)
                            {
                                isExecute = subModule.IsExecute();
                            }

                            if (isExecute)
                            {
                                ChangeRunningState();
                            }
                        }
                    }
                }

                CheckCommands();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(new FDWaferAlignPausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public override FDWaferAlignInnerStateEnum GetState()
        {
            return FDWaferAlignInnerStateEnum.IDLE;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;

            isInjected = Module.CommandRecvSlot.IsNoCommand() &&
                            token is IDOFDWAFERALIGN;

            return isInjected;
        }
    }
    public class FDWaferAlignRunningState : FDWaferAlignStateBase
    {
        public FDWaferAlignRunningState(FDWaferAligner module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSING ||
                    this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.ABORT)
                {
                    Module.InnerStateTransition(new FDWaferAlignIdleState(Module));
                }
                else
                {
                    LoggerManager.Debug("[FD Align] FD Align RUNNING Execute()");

                    //Module.VisionManager().ClearGrabberUserImage(EnumProberCam.WAFER_LOW_CAM);
                    //Module.LotOPModule().VisionScreenToLotScreen();

                    //int foupnum = Module.GetParam_Wafer().GetOriginFoupNumber();
                    //SemaphoreSlim semaphore = new SemaphoreSlim(0);

                    //*** FDAlign 이벤트 만들기
                    //Module.EventManager().RaisingEvent(typeof(WaferAlignStartEvent).FullName, new ProbeEventArgs(this, semaphore, new PIVInfo() { FoupNumber = foupnum }));

                    //semaphore.Wait();

                    retVal = Module.DoFDWaferAlignProcess();

                    //if (EventCodeEnum.NONE != retVal)
                    //{
                    //    Module.WaferAlignFailProcForStatusSoaking();
                    //}

                    //Module.LotOPModule().MapScreenToLotScreen();

                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString());
            }



            return retVal;
        }
        public override FDWaferAlignInnerStateEnum GetState()
        {
            return FDWaferAlignInnerStateEnum.ALIGN;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.UNDEFINED;
        }
    }
    public class FDWaferAlignSetupState : FDWaferAlignStateBase
    {
        public FDWaferAlignSetupState(FDWaferAligner module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING ||
                    Module.CommandRecvSlot.IsRequested<IDOFDWAFERALIGN>())
                {
                    Module.InnerStateTransition(new FDWaferAlignIdleState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public override FDWaferAlignInnerStateEnum GetState()
        {
            return FDWaferAlignInnerStateEnum.SETUP;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;

            //isInjected = Module.CommandRecvSlot.IsNoCommand() && token is IDOWAFERALIGN;

            return isInjected;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class FDWaferAlignRecoveryState : FDWaferAlignStateBase
    {
        public FDWaferAlignRecoveryState(FDWaferAligner module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED ||
                    Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSING)
                {
                    List<ISubModule> modules = Module.Template.GetProcessingModule();

                    for (int index = 0; index < modules.Count; index++)
                    {
                        ISubModule module = modules[index];

                        if (module.GetState() == SubModuleStateEnum.RECOVERY)
                        {
                            retVal = module.Recovery();
                            //Module.SetIsModify(true);

                            break;
                        }
                    }

                    Module.InnerStateTransition(new FDWaferAlignIdleState(Module));
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                Module.InnerStateTransition(new FDWaferAlignIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public override FDWaferAlignInnerStateEnum GetState()
        {
            return FDWaferAlignInnerStateEnum.RECOVERY;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RECOVERY;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class FDWaferAlignPausedState : FDWaferAlignStateBase
    {
        public FDWaferAlignPausedState(FDWaferAligner module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                CheckCommands();
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
                retVal = Module.InnerStateTransition(new FDWaferAlignAbortState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(Module.PreInnerState);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public override FDWaferAlignInnerStateEnum GetState()
        {
            return FDWaferAlignInnerStateEnum.PAUSED;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected;

            //isInjected = Module.CommandRecvSlot.IsNoCommand() && token is IDoManualWaferAlign;
            isInjected = false;
            return isInjected;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class FDWaferAlignSuspendedState : FDWaferAlignStateBase
    {
        public FDWaferAlignSuspendedState(FDWaferAligner module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (Module.TempController().IsCurTempWithinSetTempRange())
                {
                    retVal = Module.InnerStateTransition(Module.PreInnerState);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(new FDWaferAlignPausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public override FDWaferAlignInnerStateEnum GetState()
        {
            return FDWaferAlignInnerStateEnum.SUSPENDED;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }
    }
    public class FDWaferAlignDoneState : FDWaferAlignStateBase
    {
        public FDWaferAlignDoneState(FDWaferAligner module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.GetModuleState() != ModuleStateEnum.DONE)
                {
                    Module.ModuleState.StateTransition(this.GetModuleState());
                }
                Module.ClearState();

                LoggerManager.Debug("[FD Align] FD Align Done().");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(new FDWaferAlignPausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public override FDWaferAlignInnerStateEnum GetState()
        {
            return FDWaferAlignInnerStateEnum.DONE;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }
    }
    public class FDWaferAlignAbortState : FDWaferAlignStateBase
    {
        public FDWaferAlignAbortState(FDWaferAligner module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(new FDWaferAlignIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public override FDWaferAlignInnerStateEnum GetState()
        {
            return FDWaferAlignInnerStateEnum.ABORT;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.UNDEFINED;
        }
    }
    public class FDWaferAlignErrorState : FDWaferAlignStateBase
    {
        public FDWaferAlignErrorState(FDWaferAligner module, EventCodeInfo eventcode) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            try
            {
                if (Module.LotOPModule().InnerState.GetModuleState() != ModuleStateEnum.RUNNING)
                {
                    Module.InnerStateTransition(new FDWaferAlignIdleState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public override FDWaferAlignInnerStateEnum GetState()
        {
            return FDWaferAlignInnerStateEnum.ERROR;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                Module.InnerStateTransition(new FDWaferAlignIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                Module.InnerStateTransition(new FDWaferAlignIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
    }
}
