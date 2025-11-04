using System;
using System.Linq;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberErrorCode;
using LogModule;
using System.Threading;
using MetroDialogInterfaces;

namespace LoaderOP
{
    public abstract class LoaderOPStateBase : LoaderOPState
    {
        private LoaderOPModule _Module;

        public LoaderOPModule Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        public LoaderOPStateBase(LoaderOPModule module)
        {
            try
            {
                _Module = module;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new LoaderOPIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

    }
    public class LoaderOPIdleState : LoaderOPStateBase
    {
        public LoaderOPIdleState(LoaderOPModule module) : base(module)
        {
            try
            {

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
                foreach (IStateModule module in Module.RunList)
                {
                    module.Execute();

                    if (module.ModuleState.GetState() == ModuleStateEnum.ERROR ||
                        module.ModuleState.GetState() == ModuleStateEnum.PAUSED ||
                        module.ModuleState.GetState() == ModuleStateEnum.RECOVERY)
                    {
                        EventCodeInfo eventCodeInfo = module.ReasonOfError.GetLastEventCode();

                        if(eventCodeInfo != null && eventCodeInfo.Checked == false)
                        {
                            Module.MetroDialogManager().ShowMessageDialog($"[Loader Error]", $"Code = {eventCodeInfo.EventCode}\n" +
                                                                                             $"Occurred time : {eventCodeInfo.OccurredTime}\n" + 
                                                                                             $"Occurred location : {eventCodeInfo.ModuleType}\n" +
                                                                                             $"Reason : {eventCodeInfo.Message}", 
                                                                                             EnumMessageStyle.Affirmative);
                            module.ReasonOfError.Confirmed();
                        }
                    }
                }

                // 메뉴얼 동작 중, LoaderController에서 에러 발생 시, LoaderController가 갖고 있는 ReasonOfError를 이용하여 사용자에게 알려주자.



                Func<bool> conditionFunc = () => Module.RunList.Count(item =>
                    item.ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                    item.ModuleState.GetState() == ModuleStateEnum.PENDING) == 0;

                Action doAction = () =>
                {
                    Module.InnerStateTransition(new LoaderOPRunningState(Module));
                };

                Action abortAction = () => { };

                bool isExecuted;
                isExecuted = Module.CommandManager().ProcessIfRequested<ILoaderOpStart>(
                    Module,
                    conditionFunc,
                    doAction,
                    abortAction);

                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.IDLE;

        public override LoaderOPStateEnum GetState() => LoaderOPStateEnum.IDLE;

        public override EventCodeEnum Pause()
        {
            try
            {
                Module.InnerStateTransition(new LoaderOPPausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum Resume()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            // LoggerManager.Debug($"Entry Point : Resume, Lot State : Idle.");

            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is ILoaderOpStart;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }
    }

    public class LoaderOPRunningState : LoaderOPStateBase
    {
        public LoaderOPRunningState(LoaderOPModule module) : base(module) { }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                Func<bool> conditionFunc;
                Action doAction;
                Action abortAction;
                bool consumed = false;

                //=> ILoaderOpPause 
                if (Module.CommandRecvSlot.IsRequested<ILoaderOpPause>())
                {
                    conditionFunc = () =>
                  {
                      while (true)
                      {
                          bool canPause = Module.RunList.Count(item =>
                          item.ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                          item.ModuleState.GetState() == ModuleStateEnum.PENDING
                          ) == 0;

                          if (canPause)
                          {
                              foreach (IStateModule module in Module.RunList)
                              {
                                  var moduleState = module.ModuleState.GetState();

                                  if (moduleState == ModuleStateEnum.IDLE
                                     || moduleState == ModuleStateEnum.SUSPENDED
                                     || moduleState == ModuleStateEnum.DONE
                                     )
                                  {
                                      module.Pause();
                                  }
                              }
                              break;
                          }

                          foreach (IStateModule module in Module.RunList)
                          {
                              module.Execute();
                          }

                          Thread.Sleep(1);
                      }

                      return true;
                  };

                    doAction = () =>
                    {
                        Module.InnerStateTransition(new LoaderOPPausedState(Module));
                    };

                    abortAction = () => { };

                    consumed = Module.CommandManager().ProcessIfRequested<ILoaderOpPause>(
                        Module,
                        conditionFunc,
                        doAction,
                        abortAction);
                }
                //=> ILoaderOpEnd 
                else if (Module.CommandRecvSlot.IsRequested<ILoaderOpEnd>())
                {
                    conditionFunc = () =>
                {
                    while (true)
                    {
                        bool canEnd = Module.RunList.Count(item =>
                        item.ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                        item.ModuleState.GetState() == ModuleStateEnum.PENDING) == 0;

                        if (canEnd)
                        {
                            break;
                        }

                        foreach (IStateModule module in Module.RunList)
                        {
                            module.Execute();
                        }

                        Thread.Sleep(1);
                    }

                    return true;
                };

                    doAction = () =>
                   {
                       Module.InnerStateTransition(new LoaderOPIdleState(Module));
                   };

                    abortAction = () => { };

                    Module.CommandManager().ProcessIfRequested<ILoaderOpEnd>(
                        Module,
                        conditionFunc,
                        doAction,
                        abortAction);
                }

                //
                if (consumed == false)
                {
                    foreach (IStateModule module in Module.RunList)
                    {
                        module.Execute();
                    }
                }

                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override LoaderOPStateEnum GetState()
        {
            return LoaderOPStateEnum.RUNNING;
        }

        public override EventCodeEnum Pause()
        {
            try
            {
                Module.InnerStateTransition(new LoaderOPPausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum Resume()
        {
            try
            {
                LoggerManager.Debug($"Already running.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILoaderOpPause ||
            token is ILoaderOpEnd;

            return isValidCommand;
        }
    }
    public class LoaderOPPausedState : LoaderOPStateBase
    {
        public LoaderOPPausedState(LoaderOPModule module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                foreach (IStateModule module in Module.RunList)
                {
                    module.Execute();
                }

                //=> ILoaderOpResume
                Func<bool> conditionFunc = () =>
                {
                    return true;
                };

                Action doAction = () =>
                {
                    foreach (IStateModule module in Module.RunList)
                    {
                        if (module.ModuleState.GetState() == ModuleStateEnum.PAUSED)
                        {
                            module.Resume();
                        }
                    }

                    Module.InnerStateTransition(new LoaderOPRunningState(Module));
                };
                Action abortAction = () => { };

                Module.CommandManager().ProcessIfRequested<ILoaderOpResume>(
                    Module,
                    conditionFunc,
                    doAction,
                    abortAction);


                //=> ILoaderOpEnd
                conditionFunc = () =>
                {
                    return true;
                };

                doAction = () =>
                {
                    //old 
                    //foreach (IStateModule module in Module.RunList)
                    //{
                    //    if (module.ModuleState.GetState() == ModuleStateEnum.PAUSED)
                    //    {
                    //        module.End();
                    //        module.Execute();
                    //    }
                    //}

                    //new

                    //1. make loader controller to idle(so make loader to idle)
                    foreach (IStateModule module in Module.RunList)
                    {
                        if (module.ModuleState.GetState() == ModuleStateEnum.PAUSED)
                        {
                            module.End();
                            //result : paused state => abort state
                        }

                    }

                    Module.InnerStateTransition(new LoaderOPPending(Module));
                };
                abortAction = () => { };

                Module.CommandManager().ProcessIfRequested<ILoaderOpEnd>(
                    Module,
                    conditionFunc,
                    doAction,
                    abortAction);

                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override LoaderOPStateEnum GetState()
        {
            return LoaderOPStateEnum.PAUSED;
        }

        public override EventCodeEnum Pause()
        {
            try
            {
                LoggerManager.Debug($"Already paused.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum Resume()
        {
            try
            {
                Module.InnerStateTransition(new LoaderOPRunningState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILoaderOpResume ||
                token is ILoaderOpEnd;

            return isValidCommand;
        }
        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.InnerStateTransition(new LoaderOPAborted(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
    public class LoaderOPErrorState : LoaderOPStateBase
    {
        public LoaderOPErrorState(LoaderOPModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override LoaderOPStateEnum GetState()
        {
            return LoaderOPStateEnum.ERROR;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new NotImplementedException();
            }
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new NotImplementedException();
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool retVal = false;
            return retVal;
        }
    }
    public class LoaderOPAborted : LoaderOPStateBase
    {
        public LoaderOPAborted(LoaderOPModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = Module.InnerStateTransition(new LoaderOPIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }

        public override LoaderOPStateEnum GetState()
        {
            return LoaderOPStateEnum.ABORTED;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new LoaderOPPausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new NotImplementedException();
            }
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new NotImplementedException();
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool retVal = false;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new NotImplementedException();
            }
            return retVal;
        }
    }
    public class LoaderOPDone : LoaderOPStateBase
    {
        public LoaderOPDone(LoaderOPModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (IStateModule module in Module.RunList)
                {
                    module.Execute();
                    if (module.ModuleState.GetState() == ModuleStateEnum.IDLE && module.LotOPModule().LotEndFlag == true)
                    {
                        Module.InnerStateTransition(new LoaderOPIdleState(Module));
                    }
                }

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }
        public override LoaderOPStateEnum GetState()
        {
            return LoaderOPStateEnum.DONE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new LoaderOPPausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new NotImplementedException();
            }
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new NotImplementedException();
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool retVal = false;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new NotImplementedException();
            }
            return retVal;
        }
    }

    public class LoaderOPPending : LoaderOPStateBase
    {
        public LoaderOPPending(LoaderOPModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (IStateModule module in Module.RunList)
                {
                    module.Execute();

                    if (module.ModuleState.GetState() == ModuleStateEnum.DONE && module.LotOPModule().LotEndFlag == true)
                    {
                        Module.InnerStateTransition(new LoaderOPDone(Module));
                    }
                    else if (module.ModuleState.GetState() == ModuleStateEnum.IDLE && module.LotOPModule().LotEndFlag == true)
                    {
                        Module.InnerStateTransition(new LoaderOPIdleState(Module));

                    }
                    //else if (module.ModuleState.GetState() == ModuleStateEnum.IDLE && SystemManager.SysteMode == SystemModeEnum.Multiple)
                    else if (module.ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        // TODO : Single에서...?
                        Module.InnerStateTransition(new LoaderOPIdleState(Module));
                    }
                    else if (module.ModuleState.GetState() == ModuleStateEnum.RECOVERY)
                    {
                        Module.InnerStateTransition(new LoaderOPErrorState(Module));
                    }
                }

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PENDING;
        }
        public override LoaderOPStateEnum GetState()
        {
            return LoaderOPStateEnum.RUNNING;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new LoaderOPPausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new NotImplementedException();
            }
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new NotImplementedException();
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool retVal = false;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new NotImplementedException();
            }
            return retVal;
        }
    }

}
