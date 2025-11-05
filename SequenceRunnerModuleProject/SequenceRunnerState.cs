using LogModule;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.DialogControl;
using ProberInterfaces.SequenceRunner;
using ProberInterfaces.Temperature;
using SequenceRunner;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SequenceRunnerModule
{
    public abstract class SequenceRunnerStateBase : SequenceRunnerState
    {
        private SequenceRunner _SequenceRunner;
        public SequenceRunner SequenceRunner
        {
            get { return _SequenceRunner; }
            private set
            {
                if (value != _SequenceRunner)
                {
                    _SequenceRunner = value;
                }
            }
        }

        public SequenceRunnerStateBase(SequenceRunner module) => SequenceRunner = module;

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerDoneState(SequenceRunner));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
    }

    public class SequenceRunnerIdleState : SequenceRunnerStateBase
    {
        public SequenceRunnerIdleState(SequenceRunner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
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

        public override ModuleStateEnum GetModuleState()
            => ModuleStateEnum.IDLE;

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.IDLE;

        public override EventCodeEnum RunCardChange(bool IsTempSetBeforeOperation)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool initResult = false;
                ObservableCollection<ISequenceBehaviorGroupItem> behaviorGroupItems = SequenceRunner.CardChangeModule().GetCcGroupCollection();

                SequenceRunner.StepGroupCollection = new SynchronizedObservableCollection<ISequenceBehaviorGroupItem>(behaviorGroupItems);

                initResult = SequenceRunner.InitStepGroupCollection(behaviorGroupItems);

                if (initResult == true)
                {
                    SequenceRunnerStateBase transitionState = null;
                    if (IsTempSetBeforeOperation)
                    {
                        bool setCommandResult = false; //단순 체크 용.
                        ICommandManager CommandManager = SequenceRunner.CommandManager();

                        transitionState = new SequenceRunnerReachToSetTempState(SequenceRunner)
                        { NextState = new SequenceRunnerCardChangeInitState(SequenceRunner) };
                        //온도 Set

                        setCommandResult = CommandManager.SetCommand<ISetTempForFrontDoorOpen>(SequenceRunner);
                    }
                    else
                    {
                        transitionState = new SequenceRunnerCardChangeInitState(SequenceRunner);
                    }
                    retVal = SequenceRunner.InnerStateTransition(transitionState);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum RunTestHeadDockUndock(THDockType type, bool IsTempSetBeforeOperation)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool initResult = false;
                ObservableCollection<ISequenceBehaviorGroupItem> behaviorGroupItems = SequenceRunner.CardChangeModule().GetTHDockGroupCollection(type);

                SequenceRunner.StepGroupCollection = new SynchronizedObservableCollection<ISequenceBehaviorGroupItem>(behaviorGroupItems);
                initResult = SequenceRunner.InitStepGroupCollection(behaviorGroupItems);

                if (initResult == true)
                {
                    SequenceRunnerStateBase transitionState = null;
                    if (IsTempSetBeforeOperation)
                    {
                        bool setCommandResult = false; //단순 체크 용.
                        ICommandManager CommandManager = SequenceRunner.CommandManager();

                        //온도 Set
                        setCommandResult = CommandManager.SetCommand<ISetTempForFrontDoorOpen>(SequenceRunner);


                        transitionState = new SequenceRunnerReachToSetTempState(SequenceRunner)
                        { NextState = new SequenceRunnerTHDockUndockInitState(SequenceRunner) };
                    }
                    else
                    {
                        transitionState = new SequenceRunnerTHDockUndockInitState(SequenceRunner);
                    }
                    retVal = SequenceRunner.InnerStateTransition(transitionState);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum RunNCPadChange(bool IsTempSetBeforeOperation)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool initResult = false;

                ObservableCollection<ISequenceBehaviorGroupItem> behaviorGroupItems = SequenceRunner.NeedleCleaner().GetNCPadChangeGroupCollection();
                SequenceRunner.StepGroupCollection = new SynchronizedObservableCollection<ISequenceBehaviorGroupItem>(behaviorGroupItems);
                initResult = SequenceRunner.InitStepGroupCollection(behaviorGroupItems);

                if (initResult == true)
                {
                    SequenceRunnerStateBase transitionState = null;
                    if (IsTempSetBeforeOperation)
                    {
                        bool setCommandResult = false; //단순 체크 용.
                        ICommandManager CommandManager = SequenceRunner.CommandManager();

                        //온도 Set
                        setCommandResult = CommandManager.SetCommand<ISetTempForFrontDoorOpen>(SequenceRunner);

                        transitionState = new SequenceRunnerReachToSetTempState(SequenceRunner)
                        { NextState = new SequenceRunnerNCPadChangeInitState(SequenceRunner) };
                    }
                    else
                    {
                        transitionState = new SequenceRunnerNCPadChangeInitState(SequenceRunner);
                    }
                    retVal = SequenceRunner.InnerStateTransition(transitionState);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await Task.Run(() => this.Execute());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public abstract class SequenceRunnerRunningState : SequenceRunnerStateBase
    {
        public SequenceRunnerRunningState(SequenceRunner module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState()
            => ModuleStateEnum.RUNNING;


        private ISequenceBehaviorGroupItem GetIndexFromBehaviorID
            (IList<ISequenceBehaviorGroupItem> behaviorCollection, String ID)
        {
            ISequenceBehaviorGroupItem retVal = null;
            try
            {

                foreach (var v in behaviorCollection)
                {
                    if (v.BehaviorID == ID)
                    {
                        retVal = v;
                        SequenceRunner.CCStartPoint = behaviorCollection.IndexOf(retVal);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected async Task<EventCodeEnum> RunBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            bool runResult = true;
            bool posnegResult = true;

            var behaviorCollection = SequenceRunner.StepGroupCollection;

            if (behaviorCollection != null && behaviorCollection.Count != 0)
            {
                ISequenceBehaviorGroupItem behaviorItem = null;
                //SequenceRunner.StateTransition(new ModuleRunningState(SequenceRunner));

                if (!string.IsNullOrEmpty(SequenceRunner.NextID))
                {
                    behaviorItem = GetIndexFromBehaviorID(behaviorCollection, SequenceRunner.NextID);
                }

                if (SequenceRunner.CCStartPoint == 0)
                    behaviorItem = behaviorCollection[0];

                if (behaviorItem != null)
                {
                    behaviorItem.InitModule();

                    try
                    {
                        runResult = await BehaviorGroupRun(behaviorItem);
                    }
                    catch (Exception err)
                    {
                        SequenceRunner.InnerStateTransition(new SequenceRunnerErrorState(SequenceRunner));
                        LoggerManager.Exception(err);
                        retVal = EventCodeEnum.UNDEFINED;
                        LoggerManager.Debug($"[{SequenceRunner.GetSequenceRunnerStateEnum()}] Error From {SequenceRunner.GetSequenceRunnerStateEnum()}.");
                    }

                    if (runResult != false)
                    {
                        behaviorItem = GetNextBehaviorGroupItem(behaviorItem);
                    }
                    else
                    {
                        SequenceRunner.InnerStateTransition(new SequenceRunnerErrorState(SequenceRunner));
                        retVal = EventCodeEnum.UNDEFINED;
                        LoggerManager.Debug($"[{SequenceRunner.GetSequenceRunnerStateEnum()}] Error From {SequenceRunner.GetSequenceRunnerStateEnum()}.");
                    }
                }
                else
                {
                    SequenceRunner.TempController().SetTemperatureFromDevParamSetTemp();
                    SequenceRunner.InnerStateTransition(new SequenceRunnerDoneState(SequenceRunner));
                    LoggerManager.Debug($"[{SequenceRunner.GetSequenceRunnerStateEnum()}] {SequenceRunner.GetSequenceRunnerStateEnum()} is Done");

                    if(this.GetSequenceRunnerStateEnum() == SequenceRunnerStateEnum.CARD_CHANGE)
                    {
                        SequenceRunner.SoakingModule().PreHeatEvent?.Invoke(this,new EventArgs());
                    }

                    retVal = EventCodeEnum.NONE;
                }
            }
            else
            {
                LoggerManager.Debug($"[{SequenceRunner.GetSequenceRunnerStateEnum()}] The 'File' is empty.");
                //LoggerManager.Info("[CardChange] The 'CCS' is empty.");
                SequenceRunner.TempController().SetTemperatureFromDevParamSetTemp();
                SequenceRunner.InnerStateTransition(new SequenceRunnerDoneState(SequenceRunner));
                retVal = EventCodeEnum.NONE;
            }

            return retVal;

            //Function
            ISequenceBehaviorGroupItem GetNextBehaviorGroupItem(ISequenceBehaviorGroupItem behaviorItem)
            {
                ISequenceBehaviorGroupItem nextBehaviorGoupItem = null;
                if (posnegResult == true)
                    SequenceRunner.NextID = behaviorItem.NextID_Positive;
                else
                    SequenceRunner.NextID = behaviorItem.NextID_Negative;

                return nextBehaviorGoupItem;
            }

            async Task<bool> BehaviorGroupRun(ISequenceBehaviorGroupItem behaviorItem)
            {
                bool retBGRun = true;
                try
                {
                    IBehaviorResult ccBehaviorGroupRet = new BehaviorResult();

                    try
                    {
                        ccBehaviorGroupRet = await behaviorItem.PreSafetyRun();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, e);
                        throw new Exception($"[{SequenceRunner.GetSequenceRunnerStateEnum().ToString()}] Exception : " + behaviorItem.ToString() + " PreSafety");
                    }

                    if (ccBehaviorGroupRet.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[{SequenceRunner.GetSequenceRunnerStateEnum().ToString()}] Error : " + behaviorItem.ToString() + " PreSafety");
                        retBGRun = false;
                    }

                    if (retBGRun != false)
                    {
                        try
                        {
                            ccBehaviorGroupRet = await behaviorItem.BehaviorRun();
                        }
                        catch (Exception err)
                        {
                            System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                            throw new Exception($"[{SequenceRunner.GetSequenceRunnerStateEnum().ToString()}] Exception : " + behaviorItem.ToString());
                        }
                        posnegResult = ccBehaviorGroupRet.PosNegBranch;
                        if (ccBehaviorGroupRet.ErrorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[{SequenceRunner.GetSequenceRunnerStateEnum().ToString()}] Error : " + behaviorItem.ToString());
                            retBGRun = false;
                        }
                    }

                    if (retBGRun != false)
                    {
                        try
                        {
                            ccBehaviorGroupRet = await behaviorItem.PostSafetyRun();
                        }
                        catch (Exception err)
                        {
                            System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                            throw new Exception($"[{SequenceRunner.GetSequenceRunnerStateEnum().ToString()}] Exception : " + behaviorItem.ToString() + " PostSafety");
                        }

                        if (ccBehaviorGroupRet.ErrorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[{SequenceRunner.GetSequenceRunnerStateEnum().ToString()}] Error : " + behaviorItem.ToString() + " PostSafety");
                            retBGRun = false;
                        }
                    }

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
                return retBGRun;
            }
        }

        protected EventCodeEnum MakeSuspendedState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SequenceRunner.SuspendedBeforeInnerState = this;
                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerSuspendedState(SequenceRunner));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerCardChangeInitState : SequenceRunnerRunningState
    {
        public SequenceRunnerCardChangeInitState(SequenceRunner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SequenceRunner.EventManager().RaisingEvent(typeof(CardChangedEvent).FullName);
                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerCardChangeState(SequenceRunner));
                LoggerManager.Debug($"[SequenceRunner] Start Probe Card Change.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.CARD_CHANGE;

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await Task.Run(() => this.Execute());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerCardChangeState : SequenceRunnerRunningState
    {
        public SequenceRunnerCardChangeState(SequenceRunner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
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

        public override ModuleStateEnum GetModuleState()
            => ModuleStateEnum.RUNNING;

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.CARD_CHANGE;

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await RunBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerTHDockUndockInitState : SequenceRunnerRunningState
    {
        public SequenceRunnerTHDockUndockInitState(SequenceRunner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SequenceRunner.EventManager().RaisingEvent(typeof(CardChangedEvent).FullName);
                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerTHDockUndockState(SequenceRunner));
                LoggerManager.Debug($"[SequenceRunner] Start TestHead Dock/Undock.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.TESTHEAD_DOCK_UNDOCK;

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await Task.Run(() => this.Execute());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerTHDockUndockState : SequenceRunnerRunningState
    {
        public SequenceRunnerTHDockUndockState(SequenceRunner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
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

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.TESTHEAD_DOCK_UNDOCK;

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await RunBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerNCPadChangeInitState : SequenceRunnerRunningState
    {
        public SequenceRunnerNCPadChangeInitState(SequenceRunner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerNCPadChangeState(SequenceRunner));
                LoggerManager.Debug($"[SequenceRunner] Start NCPad Change");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.NCPAD_CHANGE;

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await Task.Run(() => this.Execute());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerNCPadChangeState : SequenceRunnerRunningState
    {
        public SequenceRunnerNCPadChangeState(SequenceRunner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
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

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.NCPAD_CHANGE;

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await RunBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerReachToSetTempState : SequenceRunnerStateBase
    {
        public SequenceRunnerStateBase NextState { get; set; }
        //public bool IsTempSettingCancel = false;

        public SequenceRunnerReachToSetTempState(SequenceRunner module) : base(module)
        {
            try
            {
                frontDoorTempEventFlag = false;
                ITempDisplayDialogService TempDisplayDialogService = SequenceRunner.TempDisplayDialogService();

                if (TempDisplayDialogService.IsShowing == false)
                {
                    TempDisplayDialogService.TurnOnPossibleFlag();
                    Task dialogServiceTask = Task.Run(async () =>
                    {
                        bool result = false;
                        result = await TempDisplayDialogService.ShowDialog();

                        if (result == false)
                        {
                            //IsTempSettingCancel = false;
                            SequenceRunner.InnerStateTransition(new SequenceRunnerDoneState(SequenceRunner));
                        }
                    });
                }
                //Command 날리기.
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
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

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.REACH_TO_SET_TEMP;

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.SUSPENDED;

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                ITempDisplayDialogService TempDisplayDialogService = SequenceRunner.TempDisplayDialogService();
                ICommandManager CommandManager = SequenceRunner.CommandManager();
                ITempController tempController = SequenceRunner.TempController();

                if (frontDoorTempEventFlag)
                {
                    if (((IStateModule)tempController).ModuleState.State == ModuleStateEnum.DONE ||
                        ((IStateModule)tempController).ModuleState.State == ModuleStateEnum.IDLE)
                    {
                        retVal = SequenceRunner.InnerStateTransition(NextState);
                        await TempDisplayDialogService.CloseDialog();
                    }
                }
                else if (((IStateModule)tempController).ModuleState.State == ModuleStateEnum.ERROR)
                {
                    retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerDoneState(SequenceRunner));
                }

                if (frontDoorTempStopEventFlag)
                {
                    retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerDoneState(SequenceRunner));
                }
                //if (IsTempSettingCancel == true)
                //{
                //    retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerDoneState(SequenceRunner));
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerDoneState : SequenceRunnerStateBase
    {
        public SequenceRunnerDoneState(SequenceRunner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SequenceRunner.StepGroupCollection = null;
                SequenceRunner.CCStartPoint = 0;
                SequenceRunner.NextID = "";

                SequenceRunner.InnerStateTransition(new SequenceRunnerIdleState(SequenceRunner));

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
            => ModuleStateEnum.DONE;

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.DONE;

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await Task.Run(() => this.Execute());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerPausingState : SequenceRunnerStateBase
    {
        public SequenceRunnerPausingState(SequenceRunner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerPausedState(SequenceRunner));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
            => ModuleStateEnum.PAUSING;

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.PAUSING;

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await Task.Run(() => this.Execute());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerPausedState : SequenceRunnerStateBase
    {
        public SequenceRunnerPausedState(SequenceRunner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
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

        public override ModuleStateEnum GetModuleState()
            => ModuleStateEnum.PAUSED;

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.PAUSED;

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await Task.Run(() => this.Execute());
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerAbortState(SequenceRunner));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerSuspendedState : SequenceRunnerStateBase
    {
        public SequenceRunnerSuspendedState(SequenceRunner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
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

        public override ModuleStateEnum GetModuleState()
            => ModuleStateEnum.SUSPENDED;

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.SUSPENDED;

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await Task.Run(() => this.Execute());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private void ReturnToSuspendedBeforeState()
        {
            try
            {
                var innerState = SequenceRunner.SuspendedBeforeInnerState;
                if (innerState != null)
                {
                    SequenceRunner.SuspendedBeforeInnerState = null;
                    SequenceRunner.InnerStateTransition(innerState);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    public class SequenceRunnerAbortState : SequenceRunnerStateBase
    {
        public SequenceRunnerAbortState(SequenceRunner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SequenceRunner.StepGroupCollection = null;
                SequenceRunner.CCStartPoint = 0;
                SequenceRunner.NextID = "";
                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerIdleState(SequenceRunner));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
            => ModuleStateEnum.ABORT;

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.ABORT;

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await Task.Run(() => this.Execute());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerErrorState : SequenceRunnerStateBase
    {
        public SequenceRunnerErrorState(SequenceRunner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
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

        public override ModuleStateEnum GetModuleState()
            => ModuleStateEnum.ERROR;

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.ERROR;

        public override EventCodeEnum RunRetry()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerRetryRecoveryState(SequenceRunner));
            return retVal;
        }

        public override EventCodeEnum RunReverse()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerReverseRecoveryState(SequenceRunner));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum AlternateManualMode()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerManualWaitingState(SequenceRunner));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await Task.Run(() => this.Execute());
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerAbortState(SequenceRunner));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public abstract class SequenceRunnerRecoveryState : SequenceRunnerStateBase
    {
        public SequenceRunnerRecoveryState(SequenceRunner module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState()
            => ModuleStateEnum.RECOVERY;

        protected async Task<EventCodeEnum> RunBehaviorOnce(ISequenceBehaviorGroupItem BehaviorItem)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (BehaviorItem != null && BehaviorItem.IBehavior != null)
                {
                    IBehaviorResult ccRet = new BehaviorResult();
                    BehaviorItem.InitState();

                    ccRet = await BehaviorItem.PreSafetyRun();
                    if (ccRet.ErrorCode == EventCodeEnum.NONE)
                        ccRet = await BehaviorItem.BehaviorRun();
                    if (ccRet.ErrorCode == EventCodeEnum.NONE)
                        ccRet = await BehaviorItem.PostSafetyRun();

                    if (ccRet.ErrorCode == EventCodeEnum.NONE)
                    {
                        if (SequenceRunner.CCStartPoint <= SequenceRunner.SelectedBehaviorIdx)
                        {
                            SequenceRunner.CCStartPoint = SequenceRunner.SelectedBehaviorIdx + 1;
                        }

                        retVal = 0;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerRetryRecoveryState : SequenceRunnerRecoveryState
    {
        public SequenceRunnerRetryRecoveryState(SequenceRunner module) : base(module)
        {
        }

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.RETRY;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (SequenceRunner.ErrorBeforeInnerState != null)
                {
                    SequenceRunner.InnerStateTransition(SequenceRunner.ErrorBeforeInnerState);
                    SequenceRunner.ErrorBeforeInnerState = null;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
    }

    public class SequenceRunnerReverseRecoveryState : SequenceRunnerRecoveryState
    {
        public SequenceRunnerReverseRecoveryState(SequenceRunner module) : base(module)
        {
        }

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.REVERSE;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var behaviorCollection = SequenceRunner.StepGroupCollection;
                ISequenceBehavior reverseBehavior = behaviorCollection[SequenceRunner.CCStartPoint].IBehavior.ReverseBehavior;
                IBehaviorResult CCRet = new BehaviorResult();

                if (reverseBehavior != null)
                {
                    SequenceRunnerState errorBeforeInnerState = SequenceRunner.ErrorBeforeInnerState as SequenceRunnerState;
                    LoggerManager.Debug($"[{errorBeforeInnerState?.GetSequenceRunnerStateEnum().ToString()}] Start Reverse Recovery");
                    CCRet = await reverseBehavior.Run();
                    LoggerManager.Debug($"[{errorBeforeInnerState?.GetSequenceRunnerStateEnum().ToString()}] End Reverse Recovery");
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

                if (CCRet.ErrorCode == EventCodeEnum.NONE)
                {
                    behaviorCollection[SequenceRunner.CCStartPoint].InitState();
                }

                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerErrorState(SequenceRunner));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerManualWaitingState : SequenceRunnerRecoveryState
    {
        public SequenceRunnerManualWaitingState(SequenceRunner module) : base(module)
        {
        }

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.MANUAL_WAITING;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

        public override EventCodeEnum RunManualRetry()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerManualRetryState(SequenceRunner));

            return retVal;
        }

        public override EventCodeEnum RunManualReverse()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerManualReverseState(SequenceRunner));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum AlternateManualMode()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerErrorState(SequenceRunner));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await Task.Run(() => this.Execute());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerManualReverseState : SequenceRunnerRecoveryState
    {
        public SequenceRunnerManualReverseState(SequenceRunner module) : base(module)
        {
        }

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.MANUAL_REVERSE;

        public override EventCodeEnum Execute()
        {
            return EventCodeEnum.NONE;
        }

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                ISequenceBehaviorGroupItem tempBehaviorItem = null;
                var behaviorCollection = SequenceRunner.StepGroupCollection;
                tempBehaviorItem = behaviorCollection[SequenceRunner.SelectedBehaviorIdx].IReverseBehaviorGroupItem;

                SequenceRunnerState errorBeforeInnerState = SequenceRunner.ErrorBeforeInnerState as SequenceRunnerState;
                LoggerManager.Debug($"[{errorBeforeInnerState?.GetSequenceRunnerStateEnum().ToString()}] Start Reverse Recovery");
                retVal = await RunBehaviorOnce(tempBehaviorItem);
                LoggerManager.Debug($"[{errorBeforeInnerState?.GetSequenceRunnerStateEnum().ToString()}] Start Reverse Recovery");
                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerManualWaitingState(SequenceRunner));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class SequenceRunnerManualRetryState : SequenceRunnerRecoveryState
    {
        public SequenceRunnerManualRetryState(SequenceRunner module) : base(module)
        {
        }

        public override SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
            => SequenceRunnerStateEnum.MANUAL_RETRY;

        public override EventCodeEnum Execute()
        {
            return EventCodeEnum.NONE;
        }

        public override async Task<EventCodeEnum> TaskExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ISequenceBehaviorGroupItem tempBehaviorItem = null;
                var behaviorCollection = SequenceRunner.StepGroupCollection;
                tempBehaviorItem = behaviorCollection[SequenceRunner.SelectedBehaviorIdx];

                SequenceRunnerState errorBeforeInnerState = SequenceRunner.ErrorBeforeInnerState as SequenceRunnerState;
                LoggerManager.Debug($"[{errorBeforeInnerState?.GetSequenceRunnerStateEnum().ToString()}] Start Retry Recovery");
                retVal = await RunBehaviorOnce(tempBehaviorItem);
                LoggerManager.Debug($"[{errorBeforeInnerState?.GetSequenceRunnerStateEnum().ToString()}] Start Retry Recovery");
                retVal = SequenceRunner.InnerStateTransition(new SequenceRunnerManualWaitingState(SequenceRunner));

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
