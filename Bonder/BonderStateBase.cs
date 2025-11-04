using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Bonder;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder    // 251013 sebas
{
    public abstract class BonderState : IInnerState
    {
        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();
        public abstract ModuleStateEnum GetModuleState();
        public abstract ModuleStateEnum GetState { get; }

        public abstract EventCodeEnum End();

        public abstract EventCodeEnum Abort();

        public abstract EventCodeEnum ClearState();

        public abstract EventCodeEnum Resume();

        public virtual bool CanExecute(IProbeCommandToken token)
        {   // BonderStateBase에 있어도 되야하는데 안되서 여기로 올림
            bool isInjected = false;
            return isInjected;
        }
    }

    public abstract class BonderStateBase : BonderState
    {
        public BonderModule Module { get; set; }

        public BonderStateBase(BonderModule module)
        {
            this.Module = module;
        }

        public virtual void SelfRecovery()
        {
            //Invalid call
        }

        public virtual void ClearErrorState()
        {
            //Invalid call
        }

        public override EventCodeEnum End()
        {
            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    retval = Module.InnerStateTransition(new IDLE(Module));
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public override EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }

        protected EventCodeEnum FreeRunForIdle()
        {
            EventCodeEnum retVal;
            try
            {
                BonderModule bonderModule = ShareInstance.bonderModule;

                Func<bool> conditionFuc;
                Action doAction;
                Action abortAction;

                #region => Process IBonderStartCommand
                // Bonder 모듈이 처음부터 끝까지 진행되는 프로세스 시작
                conditionFuc = () =>
                {
                    // 실행 조건 추가 예정
                    bonderModule.IsFDChuckMove = true;
                    return true;
                };
                doAction = () =>
                {
                    Module.ActivateModules();

                    Module.InnerStateTransition(new RUNNING(Module));
                };
                abortAction = () => { LoggerManager.Debug($"[IBonderStartCommand] : Aborted."); };

                Module.CommandManager().ProcessIfRequested<IBonderStartCommand>(
                    Module,
                    conditionFuc,
                    doAction,
                    abortAction);
                #endregion

                #region => Process IPickCommand (Only Picker)
                conditionFuc = () =>
                {
                    // Pick 까지 실행하는 경우
                    bonderModule.IsFDChuckMove = true;
                    bonderModule.IsPickerOnlyDoing = true;
                    return true;
                };
                doAction = () =>
                {
                    //Module.PickActivateProcModule();
                    //Module.InnerStateTransition(new RUNNING(Module));

                    Module.ActivateModules();
                    Module.InnerStateTransition(new RUNNING(Module));
                };
                abortAction = () => { LoggerManager.Debug($"[IPickCommand] : Aborted."); };

                Module.CommandManager().ProcessIfRequested<IPickCommand>(
                    Module,
                    conditionFuc,
                    doAction,
                    abortAction);
                #endregion

                #region => Process IRotationCommand (Only Rotation)
                conditionFuc = () =>
                {
                    // Pick Only 실행 후 Rotation까지 실행하는 경우
                    bonderModule.IsRotationOnlyMove = true;
                    bonderModule.IsRotationMove = true;
                    return true;
                };

                doAction = () =>
                {
                    //Module.ActivateProcModule(BonderTransferTypeEnum.ROTATING);
                    //Module.InnerStateTransition(new PROCESSING(Module));
                };
                abortAction = () => { LoggerManager.Debug($"[IRotationCommand] : Aborted."); };

                Module.CommandManager().ProcessIfRequested<IRotationCommand>(
                    Module,
                    conditionFuc,
                    doAction,
                    abortAction);
                #endregion

                #region => Process IPlaceCommand (Only Placer)
                conditionFuc = () =>
                {
                    // Rotation 후 Place 실행
                    bonderModule.IsWaferChuckMove = true;
                    bonderModule.IsPlaceOnlyDoing = true;
                    return true;
                };

                doAction = () =>
                {
                    //Module.PlaceActivateProcModule();
                    //Module.InnerStateTransition(new RUNNING(Module));
                };
                abortAction = () => { LoggerManager.Debug($"[IPlaceCommand] : Aborted."); };

                Module.CommandManager().ProcessIfRequested<IPlaceCommand>(
                    Module,
                    conditionFuc,
                    doAction,
                    abortAction);
                #endregion

                #region => BonderEndCommand
                conditionFuc = () =>
                {
                    return true;
                };
                doAction = () =>
                {
                    Module.InnerStateTransition(new DONE(Module));
                };
                abortAction = () => { };

                bool consumed = Module.CommandManager().ProcessIfRequested<IBonderEndCommand>(
                    Module,
                    conditionFuc,
                    doAction,
                    abortAction);
                #endregion

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected EventCodeEnum FreeRunForDone()
        {
            EventCodeEnum retVal;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class IDLE : BonderStateBase
    {
        public IDLE(BonderModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.IDLE;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal;
            try
            {
                Func<bool> conditionFuc;
                Action doAction;
                Action abortAction;

                if (Module.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                {
                    if(true)    // sebas 예정 : 본더 준비를 체크하는 IsFDAlignFinish 추가 예정. FDAlign 끝나고 바꾸면 될듯
                    {
                        // conditionFuc에 각 모듈 준비를 체크하는 함수 추가 예정
                        #region => Process Pick

                        #endregion

                        #region => Process Place

                        #endregion
                    }
                    else
                    {

                    }
                }
                else
                {
                    retVal = FreeRunForIdle();
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override EventCodeEnum Pause()
        {
            //if (Module.CommandRecvSlot.IsRequested<IChuckLoadCommand>())
            //{
            //}
            //else
            //{
            //    Module.PreInnerState = this;
            //    Module.InnerStateTransition(new PAUSE(Module));
            //}
            return EventCodeEnum.NONE;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {
                isInjected = Module.CommandRecvSlot.IsNoCommand() && (
                token is IBonderStartCommand ||
                token is IPickCommand ||
                token is IPlaceCommand ||
                token is IRotationCommand ||
                token is IDieAlign ||
                token is IBonderEndCommand);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isInjected;
        }
    }

    public class PROCESSING : BonderStateBase
    {
        // 모듈 1개 실행하는 경우
        public PROCESSING(BonderModule module) : base(module) { }
        public override ModuleStateEnum GetState => ModuleStateEnum.RUNNING;
        public override EventCodeEnum Execute()
        {
            LoggerManager.Debug("[Bonder] BonderStateBase. PROCESSING. Execute()");
            EventCodeEnum retVal;
            try
            {
                while (true)
                {
                    Module.ProcModule.Execute();

                    var procState = Module.ProcModule.State;

                    if (procState == BonderTransferProcStateEnum.DONE)
                    {
                        if (Module.ProcModule != null)
                        {
                            Module.ProcModule = null;
                        }
                        Module.InnerStateTransition(new DONE(Module));
                        break;
                    }

                    if (procState == BonderTransferProcStateEnum.ERROR)
                    {
                        Module.InnerStateTransition(new ERROR(Module));
                        break;
                    }

                    if (procState == BonderTransferProcStateEnum.PENDING)
                    {
                        Module.InnerStateTransition(new PENDING(Module));
                        break;
                    }
                    System.Threading.Thread.Sleep(1);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = 
                token is IBonderStartCommand ||
                token is IPickCommand ||
                token is IPlaceCommand ||
                token is IRotationCommand ||
                token is IDieAlign ||
                token is IBonderEndCommand;

            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

    }
    public class RUNNING : BonderStateBase
    {
        // 모듈을 모두 병렬실행시켜 전체 동작을 실행하는 경우
        public RUNNING(BonderModule module) : base(module) { }
        public override ModuleStateEnum GetState => ModuleStateEnum.RUNNING;

        private bool IsInitModule = false;  //InitModule을 1번만 하려고

        public override EventCodeEnum Execute()
        {
            BonderModule bonderModule = ShareInstance.bonderModule;

            //LoggerManager.Debug("[Bonder] BonderStateBase. RUNNING. Execute()");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                if (bonderModule.IsBonderEnd)
                {
                    bonderModule.IsBonderEnd = false;

                    Module.CommandRecvDoneSlot.ClearToken();
                    Module.CommandRecvSlot.ClearToken();

                    Module.InnerStateTransition(new IDLE(Module));
                }

                Func<bool> conditionFunc = () =>
                {
                    return true;
                };
                Action doAction = () =>
                {
                    Module.InnerStateTransition(new DONE(Module));
                };
                Action abortAction = () => { };

                if (IsInitModule)
                {
                    foreach (var module in Module.ProcModules)
                    {
                        module?.Execute();

                        bool consumed = Module.CommandManager().ProcessIfRequested<IBonderEndCommand>(
                            Module,
                            conditionFunc,
                            doAction,
                            abortAction);

                        if (consumed)
                        {
                            retVal = EventCodeEnum.NONE;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var module in Module.ProcModules)
                    {
                        module?.InitState();
                    }
                    IsInitModule = true;
                }
            }
            catch
            {
                throw;
            }

            return retVal;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected =
                token is IBonderStartCommand ||
                token is IBonderEndCommand;

            return isInjected;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }
    }
    public class ABORT : BonderStateBase
    {
        public ABORT(BonderModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.ABORT;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal;
            try
            {
                retVal = Module.InnerStateTransition(new IDLE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = true;
            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.PreInnerState = this;
                retVal = Module.InnerStateTransition(new PAUSE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }
    }
    public class DONE : BonderStateBase
    {
        public DONE(BonderModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.DONE;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //clear process module
                ModuleStateEnum LotOpModuleState = Module.LotOPModule().InnerState.GetModuleState();
                if (LotOpModuleState == ModuleStateEnum.RUNNING)
                {

                    Module.InnerStateTransition(new IDLE(Module));

                    retVal = EventCodeEnum.NONE;
                }
                else if (LotOpModuleState == ModuleStateEnum.PAUSING)
                {
                    retVal = FreeRunForDone();
                    retVal = Module.InnerStateTransition(new PAUSE(Module));
                }
                else if (LotOpModuleState == ModuleStateEnum.ABORT || LotOpModuleState == ModuleStateEnum.IDLE || LotOpModuleState == ModuleStateEnum.PAUSED)
                {
                    retVal = Module.InnerStateTransition(new IDLE(Module));
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {
                isInjected =
                    token is IBonderStartCommand ||
                    token is IPickCommand ||
                    token is IPlaceCommand ||
                    token is IRotationCommand ||
                    token is IDieAlign ||
                    token is IBonderEndCommand;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.PreInnerState = this;
                retVal = Module.InnerStateTransition(new PAUSE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }
    }
    public class PENDING : BonderStateBase
    {
        public PENDING(BonderModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.PENDING;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Func<bool> conditionFuc;
                Action doAction;
                Action abortAction;

                if (Module.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING || Module.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.ABORT || Module.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.PAUSING)
                {
                    #region => Process IPickCommand
                    conditionFuc = () =>
                    {
                        // var chuckFDWaferStatus = Module.StageSupervisor().FDWaferObject.GetStatus();     // FDWaferObject 추가필요
                        // bool isPassFDWaferStatus = chuckFDWaferStatus == EnumSubsStatus.NOT_EXIST;       // chuckFDWaferStatus 추가필요
                        // return isPassFDWaferStatus;
                        return true;
                    };

                    doAction = () =>
                    {
                        Module.ActivateProcModule(BonderTransferTypeEnum.PICKING);

                        Module.InnerStateTransition(new PROCESSING(Module));
                    };
                    abortAction = () => { LoggerManager.Debug($"[IPickCommand] : Aborted."); };

                    bool ret;

                    ret = Module.CommandManager().ProcessIfRequested<IPickCommand>(
                        Module,
                        conditionFuc,
                        doAction,
                        abortAction);
                    #endregion

                    retVal = EventCodeEnum.NONE;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {
                isInjected =
                    token is IBonderStartCommand ||
                    token is IPickCommand ||
                    token is IPlaceCommand ||
                    token is IRotationCommand ||
                    token is IDieAlign ||
                    token is IBonderEndCommand;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.PreInnerState = this;
                retVal = Module.InnerStateTransition(new PAUSE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PENDING;
        }
    }
    public class ERROR : BonderStateBase
    {
        public ERROR(BonderModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.ERROR;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal;
            try
            {
                LoggerManager.Debug("[Bonder] BonderStateBase is Error State");
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }


        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {
                isInjected =
                    token is IBonderStartCommand ||
                    token is IPickCommand ||
                    token is IPlaceCommand ||
                    token is IRotationCommand ||
                    token is IDieAlign ||
                    token is IBonderEndCommand;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isInjected;
        }
        public override void ClearErrorState()
        {
            Module.InnerStateTransition(new IDLE(Module));
        }

        public override void SelfRecovery()
        {
            try
            {
                if (Module.ProcModule != null)
                {
                    //Module.ProcModule.SelfRecovery();
                    Module.ProcModule = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }
        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.InnerStateTransition(new IDLE(Module));
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
                Module.InnerStateTransition(new IDLE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    public class PAUSE : BonderStateBase
    {
        public PAUSE(BonderModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.PAUSED;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal;
            try
            {
                Func<bool> conditionFuc;
                Action doAction;
                Action abortAction;

                if (Module.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                {
                    LoggerManager.Debug("[Bonder] BonderStateBase. PAUSE. Execute()");
                    #region => Process IPickCommand

                    #endregion

                    #region => Process IPlaceCommand

                    #endregion

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = FreeRunForIdle();
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
            return EventCodeEnum.NONE;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {
                isInjected =
                    token is IBonderStartCommand ||
                    token is IPickCommand ||
                    token is IPlaceCommand ||
                    token is IRotationCommand ||
                    token is IDieAlign ||
                    token is IBonderEndCommand;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isInjected;
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

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                FreeRunForDone();
                retVal = Module.InnerStateTransition(new IDLE(Module));
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
