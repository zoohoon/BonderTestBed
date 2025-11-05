using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProberInterfaces;
using NLog;
using ProberInterfaces.Command;
using System.Diagnostics;
using ProberInterfaces.Command.Internal;
using ProberErrorCode;
using ProberInterfaces.Enum;
using System.Threading;
using NotifyEventModule;
using LogModule;
using ProbingSequenceManager;
using ProberInterfaces.WaferTransfer;
using SequenceRunner;

namespace LotOP
{
    #region // Lot OP states

    #endregion
    public abstract class LotOPStateBase : LotOPState
    {
        protected static Logger Log = LogManager.GetCurrentClassLogger();
        private LotOPModule _Module;

        public LotOPModule Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        public LotOPStateBase(LotOPModule module) => Module = module;

        public override ErrorCodeEnum ClearState()
        {
            ErrorCodeEnum retval = ErrorCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new LotOPIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

    }


    public class LotOPIdleState : LotOPStateBase
    {
        public LotOPIdleState(LotOPModule module) : base(module)
        {
            try
            {
                this.Module.LotOPModule().IsLastWafer = false;
                this.Module.LotOPModule().ModuleStopFlag = false;
                (this.Module.ProbingSequenceModule() as ProbingSequenceModule).ProbingSequenceRemainCount = 0;
                this.Module.LotOPModule().LotInfo.StopAfterScanCSTFlag = false;
                if (module.PreInnerState != null && (module.PreInnerState.GetModuleState() == ModuleStateEnum.ABORT || module.PreInnerState.GetModuleState() == ModuleStateEnum.DONE))
                {
                    Module.ProgressDialogService().CloseDialg(Module.LotOPModule());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override ErrorCodeEnum Execute()
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
            try
            {
                //Module.MarkAligner().Run();
                
                if(Module.CommandRecvSlot.Token is ISystemInit)
                {
                    Module.InnerStateTransition(new LotOPReadyIdleToRunningState(Module));
                    return ErrorCodeEnum.NONE;
                }

                Func<bool> conditionFunc = () =>
                {
                    bool canRunning = false;
                    Module.LotStartFailReason = null;
                    canRunning = Module.IsCanLotStart();
                    if (canRunning)
                    {
                        Module.ProbingSequenceModule().UpdateProbingStateSequence();
                        canRunning = Module.CommandManager().SetCommand<ILoaderOpStart>(Module);
                    }

                    //isInjected = isInjected && Module.CommandManager().SetCommand<IFoupOpStart>(Module);

                    return canRunning;
                };

                Action doAction = () =>
                {
                    Module.LotEndFlag = false;
                    Module.CommandManager().SetCommand<ICassetteLoadCommand>(Module, new FoupCommandParam() { CassetteNumber = 1 });
                    Module.InnerStateTransition(new LotOPReadyIdleToRunningState(Module));
                };
                Action abortAction = () =>
                {
                    var task = Task<IProbeCommandToken>.Run((Func<Task>)(async () =>
                    {
                        Module.ViewModelManager().UnLockViewControl((int)Module.GetHashCode());
                    }));
                    //Module.MessageDialogService().ShowDialog("[LOTSTART]", "FAIL=" + Module.LotStartFailReason, EnumMessageStyle.AffirmativeAndNegative);
                };
                bool consumed;
                foreach (IStateModule module in Module.RunList)
                {
                    
                    module.Execute();
                    consumed = Module.CommandManager().ProcessIfRequested<ILotOpStart>(
                  Module,
                  conditionFunc,
                  doAction,
                  abortAction);
                    if (consumed)
                    {
                        break;
                    }
                }


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
            return ModuleStateEnum.IDLE;
        }

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.IDLE;
        }

        public override ErrorCodeEnum Pause()
        {
            LoggerManager.Debug($"Entry Point : Pause, Lot State : Idle.");

            return ErrorCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpStart | token is ISystemInit;

            

            return isValidCommand;
        }
    }

    public class LotOPReadyIdleToRunningState : LotOPStateBase
    {
        public LotOPReadyIdleToRunningState(LotOPModule module) : base(module) {

            //Module.PinAligner().ForcedDone = EnumModuleForcedState.ForcedDone;
            //Module.NeedleCleaner().ForcedDone = EnumModuleForcedState.ForcedDone;
            //Module.WaferAligner().ForcedDone = EnumModuleForcedState.ForcedDone;
            //Module.PMIModule().ForcedDone = EnumModuleForcedState.ForcedDone;
            //Module.SoakingModule().ForcedDone = EnumModuleForcedState.ForcedDone;
            //Module.AirBlowChuckCleaningModule().ForcedDone = EnumModuleForcedState.ForcedDone;
            //Module.AirBlowWaferCleaningModule().ForcedDone = EnumModuleForcedState.ForcedDone;
        }

        public override ErrorCodeEnum Execute()
        {
            ErrorCodeEnum retVal;
            try
            {
                if(Module.CommandRecvSlot.Token is ISystemInit)
                {
                    Task.Run(() =>
                    {
                        ErrorCodeEnum systemInitResult = Module.StageSupervisor().SystemInit().Result;
                        ErrorCodeEnum responseFromLoader = Module.LoaderController().ResponseSystemInit(systemInitResult);
                        if(responseFromLoader == ErrorCodeEnum.NONE)
                        {
                            //==> Recovery 동작을 수행 해여라
                            var cardStuckRecovery = new GP_PCardSutckRecovery();

                            var cardStuckRecoveryResult = cardStuckRecovery.Run().Result;
                            if (cardStuckRecoveryResult.ErrorCode == ErrorCodeEnum.NONE)
                            {
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            //==> Machine Init 수행 취소
                        }

                    });

                    Module.CommandRecvSlot.ClearToken();
                    Module.InnerStateTransition(new LotOPIdleState(Module));
                    return ErrorCodeEnum.NONE;
                }

                bool canTransitionToRunning;

                foreach (IStateModule module in Module.RunList)
                {
                    module.Execute();
                }

                canTransitionToRunning =
                    Module.SequenceEngineManager().GetRunState() &&
                    //Module.FoupOP.ModuleState.GetState() == ModuleStateEnum.RUNNING &&
                    Module.LoaderOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING;

                if (canTransitionToRunning)
                {
                    //Module.SoakingModule().ForcedDone = EnumModuleForcedState.ForcedDone;
                    ////Module.PinAligner().ForcedDone = EnumModuleForcedState.ForcedDone;
                    //Module.WaferAligner().ForcedDone = EnumModuleForcedState.ForcedDone;
                    //Module.NeedleCleaner().ForcedDone = EnumModuleForcedState.ForcedDone;
                    //Module.AirBlowWaferCleaningModule().ForcedDone = EnumModuleForcedState.ForcedDone;
                    //Module.AirBlowChuckCleaningModule().ForcedDone = EnumModuleForcedState.ForcedDone;
                    //Module.NeedleBrush().ForcedDone = EnumModuleForcedState.ForcedDone;
                    //Module.PinAligner().ForcedDone = EnumModuleForcedState.ForcedRunningAndDone;
                    //Module.PMIModule().ForcedDone = EnumModuleForcedState.Normal;
                    //Module.PolishWaferModule().ForcedDone = EnumModuleForcedState.Normal;
                    //Module.WaferTransferModule().ForcedDone = EnumModuleForcedState.Normal;
                    //Module.ProbingModule().ForcedDone = EnumModuleForcedState.Normal;
                    //Module.ViewModelManager().Lock(Module.GetHashCode(), "LOT", "LOTSTART");
                    //retVal = Module.ViewModelManager().LockAsync(Module.GetHashCode(), "LOT",  "LOTSTART").Result;
                    //Lot 시작시 모든 상태 클리어


                    Module.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);
                    Module.StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.IDLE);
                    Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                    Module.SoakingModule().SoackingDone = false;
                    this.Module.LotOPModule().IsLastWafer = false;
                    this.Module.LotOPModule().ModuleStopFlag = false;
                    Module.InitData();
                    //Module.LotInfo.LotName.Value = "ID" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second;
                    //Module.LotInfo.LotName.Value = "YNOT2884";
                    Module.InnerStateTransition(new LotOPRunningState(Module));
                    Module.EventManager().RaisingEvent(typeof(LotStartEvent).FullName);
                    Module.LotInfo.ClearWaferSummary();
                    Module.LotInfo.LotEndTimeEnable = false;
                    Module.LotInfo.LotStartTimeEnable = true;
                    Module.LotInfo.LotStartTime = DateTime.Now.ToLocalTime();
                    if ((!(this.Module.LotInfo.StopAfterScanCSTFlag)) &&
                                    (this.Module.LotDeviceParam.StopOption.StopAfterScanCassette.Value ||
                                    this.Module.LotDeviceParam.OperatorStopOption.StopAfterScanCassette.Value))
                    {
                        Module.ModuleStopFlag = true;
                    }
                    LoggerManager.Debug($"[{Module.LotInfo.LotStartTime}] : LOT START");
                }

                retVal = ErrorCodeEnum.NONE;
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

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.READYTORUNNING;
        }

        public override ErrorCodeEnum Pause()
        {
            LoggerManager.Debug($"Entry Point : Pause, Lot State : Idle.");

            return ErrorCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
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

    public class LotOPReadyPausedToRunningState : LotOPStateBase
    {
        public LotOPReadyPausedToRunningState(LotOPModule module) : base(module) {
            
        }

        public override ErrorCodeEnum Execute()
        {
            ErrorCodeEnum retVal;
            try
            {

                bool canTransitionToRunning;


                canTransitionToRunning =
                    Module.SequenceEngineManager().GetRunState() &&
                    //Module.FoupOP.ModuleState.GetState() == ModuleStateEnum.RUNNING &&
                    Module.LoaderOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING;

                if (canTransitionToRunning)
                {
                    Module.InnerStateTransition(new LotOPRunningState(Module));

                    Module.EventManager().RaisingEvent(typeof(LotResumeEvent).FullName);
                    Module.LotInfo.LotStartTime = DateTime.Now.ToLocalTime();
                    LoggerManager.Debug($"[{Module.LotInfo.LotStartTime}] : LOT START");
                }
                Func<bool> conditionPauseFunc = () =>
                {
                    Module.Pause();
                    while (true)
                    {
                        bool canPause = Module.RunList.Count(item =>
                          item.ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                          item.ModuleState.GetState() == ModuleStateEnum.PENDING) == 0;

                        if (canPause)
                        {
                            canPause = canPause && Module.CommandManager().SetCommand<ILoaderOpPause>(Module);

                            if (canPause == true)
                            {
                                break;
                            }
                        }
                        else
                        {
                            foreach (IStateModule module in Module.RunList)
                            {
                                module.Execute();
                            }
                        }

                        Module._delays.DelayFor(1);
                    };

                    return true;
                };

                Action doPauseAction = () =>
                {
                    foreach (IStateModule module in Module.RunList)
                    {
                        module.Pause();
                    }
                };
                Action abortPauseAction = () => Module.MessageDialogService().ShowDialog("[LOTPAUSE]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);

                bool consumedPuase;


                foreach (IStateModule module in Module.RunList)
                {
                    if (module.ModuleState.GetState() == ModuleStateEnum.ERROR)
                    {
                        Module.ReasonOfError = module.ReasonOfError;
                        Module.CommandManager().SetCommand<ILotOpPause>(Module);
                    }
                    consumedPuase = Module.CommandManager().ProcessIfRequested<ILotOpPause>(
                     Module,
                     conditionPauseFunc,
                     doPauseAction,
                     abortPauseAction);
                    if (consumedPuase)
                    {
                        break;
                    }
                }

                retVal = ErrorCodeEnum.NONE;
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
            return ModuleStateEnum.RESUMMING;
        }

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.READY_PAUSED_TO_RUNNING;
        }

        public override ErrorCodeEnum Pause()
        {
            Module.InnerStateTransition(new LotOPPausingState(Module));

            return ErrorCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            isValidCommand = token is ILotOpPause ||
                                token is ILotOpEnd;
            return isValidCommand;
        }
    }

    public class LotOPRunningState : LotOPStateBase
    {
        public LotOPRunningState(LotOPModule module) : base(module)
        {
           
        }

        public override ErrorCodeEnum Execute()
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
            bool bCheckToken = false;
            bool consumedPuase;

            try
            {

                Stopwatch stw = new Stopwatch();

                stw.Start();

                IGPIB GpibModule = Module.GPIB();


                if (Module.GPIB().GetGPIBEnable() == EnumGpibEnable.ENABLE & GpibModule.ModuleState.GetState() != ModuleStateEnum.RUNNING)
                {
                    Module.ReasonOfError = GpibModule.ReasonOfError;
                    Module.CommandManager().SetCommand<ILotOpPause>(Module);
                }

                Func<bool> conditionPauseFunc = () =>
                {
                    Module.Pause();
                    while (true)
                    {
                        bool canPause = Module.RunList.Count(item =>
                          item.ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                          item.ModuleState.GetState() == ModuleStateEnum.PENDING) == 0;

                        if (canPause)
                        {
                            canPause = canPause && Module.CommandManager().SetCommand<ILoaderOpPause>(Module);

                            if (canPause == true)
                            {
                                break;
                            }
                        }
                        else
                        {
                            foreach (IStateModule module in Module.RunList)
                            {
                                module.Execute();
                            }
                        }

                        Module._delays.DelayFor(1);
                    };

                    return true;
                };

                Action doPauseAction = () =>
                {
                    foreach (IStateModule module in Module.RunList)
                    {
                        module.Pause();
                    }
                };
                Action abortPauseAction = () => Module.MessageDialogService().ShowDialog("[LOTPAUSE]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);

                Func<bool> conditionEndFunc = () =>
                {
                    while (true)
                    {
                        bool canEnd = Module.RunList.Count(item =>
                          item.ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                          item.ModuleState.GetState() == ModuleStateEnum.PENDING) == 0;

                        if (canEnd)
                        {
                            canEnd = canEnd && Module.CommandManager().SetCommand<ILoaderOpEnd>(Module);

                            if (canEnd == true)
                            {
                                break;
                            }
                        }
                        else
                        {
                            foreach (IStateModule module in Module.RunList)
                            {
                                module.Execute();
                            }
                        }

                        Module._delays.DelayFor(1);
                    };
                    return true;
                };

                Action doEndAction = () =>
                {
                    Module.InnerStateTransition(new LotOPAborted(Module));
                };
                Action abortEndAction = () => Module.MessageDialogService().ShowDialog("[LOTEND]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                bool consumedEnd = false;


                if (Module.LoaderController().ModuleState.GetState() == ModuleStateEnum.ERROR || Module.LoaderController().ModuleState.GetState() == ModuleStateEnum.RECOVERY)
                {
                    if (Module.StageSupervisor().WaferObject.WaferStatus == EnumSubsStatus.EXIST)
                    {
                        if (Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.PROCESSED)
                        {
                            Module.ReasonOfError = Module.LoaderController().ReasonOfError;
                            bool isSucesses = Module.CommandManager().SetCommand<ILotOpPause>(Module);
                            if (isSucesses)
                            {
                            }
                        }
                    }
                    else
                    {
                        Module.ReasonOfError = Module.LoaderController().ReasonOfError;
                        bool isSucesses = Module.CommandManager().SetCommand<ILotOpPause>(Module);
                        if (isSucesses)
                        {

                        }
                    }
                }

                foreach (IStateModule module in Module.RunList)
                {
                    /*
                       [ 커맨드 토큰 동작 로직 정리 ver 1.0 by Ralph ]

                       한 모듈이 다른 모듈로 커맨드 토큰을 발행하고 수행되는 과정에 대하여....
                       여기서는 NC와 Pin 그리고 프로빙 모듈간 동작 순서를 가지고 설명.

                       1) 프로빙 도중에 NC가 다이 인터벌로 발동, 이 때 프로빙은 GPIB에 의해서 커맨드 토큰을 받은 상태로 SUSPEND가 됨
                       2) NC동작이 시작되고 Pin align before NC 옵션에 의해 NC가 핀 모듈로 커맨드 토큰 발행, 그 후 SUSPEND 상태로 전환
                       3) Run List에서 프로빙 모듈이 NC보다 뒤에 있으므로 특별하게 처리해 주지 않으면 다음 틱에서 pin이 수행되기 전에 프로빙 모듈이 끼어 들게 됨
                       4) 이를 방지하고자 프로빙 모듈인 경우에 한해서 동작 전에 매번 다른 모듈들에 발행된 토큰이 있는지 확인 후 동작
                       5) 만약 다른 모듈에 토큰이 발행되어 있다면 이번 틱에서 프로빙 모듈의 동작은 스킵
                       6) 즉, 프로빙 모듈은 항상 토큰들 간 우선순위에서 가장 후위에 놓이게 되며 한 번에 다수개의 토큰이 발행되는 상황이 벌어진다면
                          그것들을 처리하는 순서는 Run List를 따름
                       7) 추가적으로 NC가 발행한 커맨드 토큰에 대해 핀이 동작을 끝낸 순간 NC 모듈의 턴에서 자신의 상태를 SUSPEND에서 RUNNING으로 전환하고 턴을
                          끝내게 되는데, 이 때 다음 턴인 프로빙 모듈이 이미 SUSPEND에서 RUNNING으로 전환되어 있는 상황이 되어 프로빙 모듈이 끼어드는 것을 방지하기
                          위하여 다른 모듈이 SUSPEND나 혹은 RUNNING이 있는지 확인하는 로직을 추가함


                       3줄요약
                       
                       1. 프로빙 모듈의 토큰은 우선순위에서 가장 뒤쪽에 놓인다. 
                       2. 프로빙 모듈을 제외한 다른 모듈 중 어느 하나가 SUSPEND 혹은 RUNNING인 상태일 경우 프로빙 모듈은 대기 상태가 된다.
                       3. 커맨드 토큰의 동작 순서는 Run List에 있는 모듈의 동작 순서를 따른다. 따라서 먼저 수행되어야 할 우선순위 대로 Run List를 구성해야 한다.

                    */

                    //Lot Start 를 알기위해 ==
                    Module.LotStartFlag = true;
                    //=======================


                    if (module is IProbingModule)
                    {
                        bCheckToken = false;
                        foreach (IStateModule runModule in Module.RunList)
                        {
                            if (!(runModule is IProbingModule))
                            {
                                if (!runModule.CommandRecvSlot.IsNoCommand()
                                    || !(runModule.CommandRecvProcSlot?.IsNoCommand() ?? true)
                                    || runModule.ModuleState.GetState() == ModuleStateEnum.SUSPENDED
                                    || runModule.ModuleState.GetState() == ModuleStateEnum.RUNNING)
                                {
                                    bCheckToken = true;
                                }
                            }
                        }
                    }
                    if (bCheckToken == true) continue;

                    module.Execute();

                    Module.VisionManager().AllStageCameraStopGrab();
                    if (module.ModuleState.GetState() == ModuleStateEnum.ERROR || module.ModuleState.GetState() == ModuleStateEnum.RECOVERY || module.ModuleState.GetState() == ModuleStateEnum.PAUSED)
                    {
                        Module.ReasonOfError = module.ReasonOfError;
                        Module.CommandManager().SetCommand<ILotOpPause>(Module);
                    }
                    //if (module.ModuleState.GetState() == ModuleStateEnum.SUSPENDED&&!(module is IWaferTransferModule)&& !(module is IProbingModule))
                    //{
                    //    bool suspendStartFlag = false;
                    //    foreach (IStateModule runModule in Module.RunList) 
                    //    {
                    //        if(!runModule.CommandRecvSlot.IsNoCommand())
                    //        {
                    //            runModule.Execute();
                    //            suspendStartFlag = true;
                    //        }
                    //    }

                    //    if(!suspendStartFlag)
                    //    {
                    //        module.ReasonOfError.Reason = "Command Not Execute";
                    //        Module.ReasonOfError = module.ReasonOfError;
                    //        Module.CommandManager().SetCommand<ILotOpPause>(Module);
                    //    }
                    //}
                    consumedPuase = Module.CommandManager().ProcessIfRequested<ILotOpPause>(
                        Module,
                        conditionPauseFunc,
                        doPauseAction,
                        abortPauseAction);
                    if (consumedPuase)
                    {
                        break;
                    }
                    consumedEnd = Module.CommandManager().ProcessIfRequested<ILotOpEnd>(
                        Module,
                        conditionEndFunc,
                        doEndAction,
                        abortEndAction);
                    if (consumedEnd)
                    {
                        break;
                    }

                    if (Module.CommandRecvSlot.Token is IUnloadAllWafer)
                    {
                        if (conditionPauseFunc.Invoke())
                        {
                            doPauseAction();
                            break;
                        }
                    }

                }

                if (Module.LotInfo.UnProcessedWaferCount() == 0 && Module.LotEndFlag)
                {
                    bool canEnd = Module.RunList.Count(item =>
                         item.ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                         item.ModuleState.GetState() == ModuleStateEnum.PENDING) == 0;
                    canEnd = canEnd && Module.CommandManager().SetCommand<ILoaderOpEnd>(Module);
                    if (canEnd)
                    {
                        Module.InnerStateTransition(new LotOPAborted(Module));
                        //  Module.CommandManager().SetCommand<ILotOpEnd>(Module);
                        Module.LotInfo.SetCurrentWaferSlotNumber(0);
                        Module.LotEndFlag = false;
                    }
                }

                if (consumedEnd == true)
                {
                    LoggerManager.Debug($"[{DateTime.Now.ToLocalTime()}] : LOT END");
                }

                RetVal = ErrorCodeEnum.NONE;

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
            return ModuleStateEnum.RUNNING;
        }

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.RUNNING;
        }

        public override ErrorCodeEnum Pause()
        {
            Module.InnerStateTransition(new LotOPPausingState(Module));

            return ErrorCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = token is ILotOpPause ||
                token is ILotOpEnd ||
                token is IUnloadAllWafer;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }
    }
    public class LotOPPausingState : LotOPStateBase
    {
        public LotOPPausingState(LotOPModule module) : base(module)
        {
        }

        public override ErrorCodeEnum Execute()
        {
            ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
            try
            {
                bool canTransitionToPaused = false;
                int pauseCountModule = 0;
                foreach (IStateModule module in Module.RunList)
                {
                    module.Execute();
                }
                pauseCountModule = Module.RunList.Count
                (item => item.ModuleState.GetState() != ModuleStateEnum.PAUSED &&
                         item.ModuleState.GetState() != ModuleStateEnum.ERROR &&
                         item.ModuleState.GetState() != ModuleStateEnum.RECOVERY);
                canTransitionToPaused =
                    Module.LoaderOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE ||
                    Module.LoaderOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED;
                canTransitionToPaused = canTransitionToPaused && (pauseCountModule == 0);

                if (canTransitionToPaused)
                {
                    Module.InnerStateTransition(new LotOPPausedState(Module));
                    Module.EventManager().RaisingEvent(typeof(LotPausedEvent).FullName);
                    LoggerManager.Debug($"[{DateTime.Now.ToLocalTime()}] : LOT PAUSED");
                }

                retVal = ErrorCodeEnum.NONE;
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
            return ModuleStateEnum.PAUSING;
        }

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.PAUSING;
        }

        public override ErrorCodeEnum Pause()
        {
            LoggerManager.Debug($"Already paused.");

            return ErrorCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {
                isValidCommand =
                    token is IUnloadAllWafer;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override ErrorCodeEnum Resume()
        {
            ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
            return retVal;
        }
    }

    public class LotOPPausedState : LotOPStateBase
    {
        public LotOPPausedState(LotOPModule module) : base(module)
        {
            try
            {
                Module.ProgressDialogService().CloseDialg(Module.LotOPModule());
                Task.Run((Action)(() =>
                {
                    Module.ViewModelManager().UnLockViewControl((int)this.Module.GetHashCode());
                }));

                if (Module.ReasonOfError != null)
                {
                    Module.MessageDialogService().ShowDialog("[LOTPAUSE]" + DateTime.Now, "[" + Module.ReasonOfError.ModuleType + "] Reason=" + Module.ReasonOfError.Reason, EnumMessageStyle.Affirmative);
                    //Module.ReasonOfError.Reason = null;
                    Module.ReasonOfError = null;
                }
                else if (Module.ReasonOfStopOption.IsStop)
                {
                    Module.MessageDialogService().ShowDialog("[LOTPAUSE]" + DateTime.Now, "[Lot_stop_option] Reason=" + Module.ReasonOfStopOption.Reason.ToString(), EnumMessageStyle.Affirmative);
                    if (Module.ReasonOfStopOption.Reason == StopOptionEnum.STOP_AFTER_CASSETTE)
                    {
                        this.Module.LotOPModule().LotInfo.StopAfterScanCSTFlag = true;
                    }
                    else if (Module.ReasonOfStopOption.Reason == StopOptionEnum.STOP_BEFORE_PROBING)
                    {
                        this.Module.LotOPModule().LotInfo.StopBeforeProbeFlag = true;
                    }
                    else if (Module.ReasonOfStopOption.Reason == StopOptionEnum.EVERY_STOP_BEFORE_PROBING)
                    {
                        this.Module.LotOPModule().LotInfo.StopBeforeProbeFlag = true;

                    }
                    Module.ReasonOfStopOption.Reason = StopOptionEnum.UNDEFINED;
                    Module.ReasonOfStopOption.IsStop = false;
                    Module.ModuleStopFlag = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override ErrorCodeEnum Execute()
        {
            ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
            try
            {

                bool isExecuted;

                foreach (IStateModule module in Module.RunList)
                {
                    module.Execute();
                }
                //=> Process LotResume
                Func<bool> conditionFunc = () =>
                {
                    bool isInjected = Module.CommandManager().SetCommand<ILoaderOpResume>(Module);

                    if (Module.GPIB().ModuleState.GetState() == ModuleStateEnum.IDLE
                        || Module.GPIB().ModuleState.GetState() == ModuleStateEnum.PAUSED
                        || Module.GPIB().ModuleState.GetState() == ModuleStateEnum.ERROR
                        )
                    {
                        Module.GPIB().Resume();
                    }

                    if (Module.TempController().ModuleState.GetState() == ModuleStateEnum.ERROR)
                    {
                        Module.TempController().Resume();
                    }

                    //isInjected = isInjected && Module.CommandManager().SetCommand<IFoupOpStart>(Module);
                    return isInjected;
                };
                Action doAction = () =>
                {
                    Module.Resume();
                };
                Action abortAction = () => Module.MessageDialogService().ShowDialog("[LOTRESUME]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);

                isExecuted = Module.CommandManager().ProcessIfRequested<ILotOpResume>(
                    Module,
                    conditionFunc,
                    doAction,
                    abortAction);


                //=> Process LotEnd
                conditionFunc = () =>
                {
                    bool isInjected = false;
                    isInjected = Module.CommandManager().SetCommand<ILoaderOpEnd>(Module);


                    //isInjected = isInjected | Module.CommandManager().SetCommand<IUNLOADWAFER>(Module);

                    //isInjected = isInjected && Module.CommandManager().SetCommand<IFoupOpEnd>(Module);
                    return isInjected;
                };
                doAction = () =>
                {
                    this.End();

                    foreach (IStateModule module in Module.RunList)
                    {
                        module.End();
                    }
                };
                abortAction = () => Module.MessageDialogService().ShowDialog("[LOTEND]", "FAIL", EnumMessageStyle.Affirmative);

                isExecuted = Module.CommandManager().ProcessIfRequested<ILotOpEnd>(
                    Module,
                    conditionFunc,
                    doAction,
                    abortAction);

                isExecuted = Module.CommandManager().ProcessIfRequested<IUnloadAllWafer>(
                    Module,
                    conditionFunc,
                    doAction,
                    abortAction);


                retVal = ErrorCodeEnum.NONE;

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

        public override LotOPStateEnum GetState()
        {

            return LotOPStateEnum.PAUSED;
        }

        public override ErrorCodeEnum Pause()
        {
            LoggerManager.Debug($"Already paused.");

            return ErrorCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            isValidCommand =
                token is ILotOpResume ||
                token is ILotOpEnd ||
                token is IUnloadAllWafer;

            return isValidCommand;
        }

        public override ErrorCodeEnum End()
        {
            ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new LotOPAborted(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override ErrorCodeEnum Resume()
        {
            ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new LotOPReadyPausedToRunningState(Module));

                foreach (var runJob in Module.RunList)
                {
                    runJob.Resume();
                }
                retVal = ErrorCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class LotOPErrorState : LotOPStateBase
    {
        public LotOPErrorState(LotOPModule module) : base(module)
        {
        }

        public override ErrorCodeEnum Execute()
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
            try
            {

                RetVal = ErrorCodeEnum.NONE;

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
            return ModuleStateEnum.ERROR;
        }

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.ERROR;
        }

        public override ErrorCodeEnum Pause()
        {
            throw new NotImplementedException();
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }
    }
    public class LotOPAborted : LotOPStateBase
    {
        public LotOPAborted(LotOPModule module) : base(module)
        {
        }

        public override ErrorCodeEnum Execute()
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
            try
            {
                bool LotEndFlag = true;


                LotEndFlag = true;
                foreach (var runJob in Module.RunList)
                {
                    runJob.Execute();
                    if (runJob.ModuleState.GetState() != ModuleStateEnum.IDLE)
                    {
                        if (runJob.ModuleState.GetState() == ModuleStateEnum.PAUSED)
                        {
                            runJob.Abort();
                        }

                        LotEndFlag = false;
                    }

                }
                if (Module.LoaderOPModule().ModuleState.GetState() != ModuleStateEnum.IDLE || Module.LoaderController().ModuleState.GetState() != ModuleStateEnum.IDLE)
                {
                    LotEndFlag = false;
                }


                if (LotEndFlag)
                {
                    Module.LotInfo.LotEndTimeEnable = true;
                    Module.LotInfo.LotEndTime = DateTime.Now.ToLocalTime();

                    
                    Module.ViewModelManager().UnLockViewControl(Module.LotOPModule().GetHashCode());

                    RetVal = Module.InnerStateTransition(new LotOPIdleState(Module));

                    if(Module.CommandRecvDoneSlot.Token is IUnloadAllWafer)
                    { 
                        Module.EventManager().RaisingEvent(typeof(LotEndDueToUnloadAllWaferEvent).FullName);
                    }
                    else
                    {
                        Module.EventManager().RaisingEvent(typeof(LotEndEvent).FullName);
                    }

                    if (Module.LotInfo.ContinueLot == true)
                        Module.CommandManager().SetCommand<ILotOpStart>(this);
                    RetVal = ErrorCodeEnum.NONE;

                }

                Module._delays.DelayFor(1);



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
            return ModuleStateEnum.ABORT;
        }

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.ABORTED;
        }

        public override ErrorCodeEnum Pause()
        {
            throw new NotImplementedException();
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpEnd || token is ILotOpStart;
            return isValidCommand;
        }
    }
    public class LotOPDone : LotOPStateBase
    {
        public LotOPDone(LotOPModule module) : base(module)
        {
            try
            {

                Module.ViewModelManager().UnLockViewControl(Module.GetHashCode());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override ErrorCodeEnum Execute()
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
            try
            {

                RetVal = ErrorCodeEnum.NONE;

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
        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.DONE;
        }

        public override ErrorCodeEnum Pause()
        {
            throw new NotImplementedException();
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }
    }
}


