using System;
using System.Threading.Tasks;
using ProberInterfaces;
using System.Collections.ObjectModel;
using ProberInterfaces.CardChange;
using ProberErrorCode;
using LogModule;
using ProberInterfaces.SequenceRunner;
using ProberInterfaces.Command.Internal;
using SequenceRunner;
using ProberInterfaces.Command;
using System.Threading;
using LoaderController.GPController;
using MetroDialogInterfaces;

namespace CardChange
{
    public abstract class CardChangeStateBase : ICardChangeState
    {
        public abstract bool CanExecute(IProbeCommandToken token);

        private CardChangeModule _Module;

        public CardChangeModule Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        public CardChangeStateBase(CardChangeModule module)
        {
            Module = module;
        }

        public virtual Task<int> ExecuteCC()
        {
            throw new NotImplementedException();
        }

        public virtual int InitExecute()
        {
            throw new NotImplementedException();
        }

        public abstract CardChangeModuleStateEnum GetState();

        public abstract ModuleStateEnum GetModuleState();

        public virtual EventCodeEnum Execute()
        {
            throw new NotImplementedException();
        }
        public abstract EventCodeEnum Pause();

        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                try
                {
                    retval = Module.InnerStateTransition(new CardChangeIdleState(Module));
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
        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class CardChangeInitState : CardChangeStateBase
    {
        public CardChangeInitState(CardChangeModule module) : base(module)
        {
        }
        public override Task<int> ExecuteCC()
        {
            Task<int> retVal = Task.FromResult<int>(-1);
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public override int InitExecute()
        {
            int retVal = 0;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.IDLE;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }
    }

    public class CardChangeIdleState : CardChangeStateBase
    {
        public CardChangeIdleState(CardChangeModule module) : base(module)
        {
        }
        public override Task<int> ExecuteCC()
        {
            Task<int> retVal = Task.FromResult<int>(0);
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
        public override EventCodeEnum Execute()
        {
            //Func<bool> conditionFunc = () => Module.RunList.Count(item =>
            //        item.ModuleState.GetState() == ModuleStateEnum.RUNNING ||
            //        item.ModuleState.GetState() == ModuleStateEnum.PENDING) == 0;

            Func<bool> conditionFunc = () => true;
            Action doAction = () =>
            {
                Module.InnerStateTransition(new CardChangeRunningState(Module));
            };
            Action abortAction = () => { };

            bool isExecuted;
            isExecuted = Module.CommandManager().ProcessIfRequested<IStagecardChangeStart>(
                Module,
                conditionFunc,
                doAction,
                abortAction);

            return EventCodeEnum.NONE;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.IDLE;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = token is IStagecardChangeStart;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }
    }

    public class CardChangeRunningState : CardChangeStateBase
    {
        public CardChangeRunningState(CardChangeModule module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Thread.Sleep(1000);
                //ObservableCollection<ISequenceBehaviorGroupItem> Seqgroup = this.CardChangeModule()?.GetCcDockCollection(); 
                ObservableCollection<ISequenceBehaviorGroupItem> Seqgroup = Module.GetCCCommandParamCollection();
                SequenceBehavior cur_beh = Module.GetBehavior() as SequenceBehavior;
                ObservableCollection<ISequenceBehaviorRun> preCheck_beh = null;
                ObservableCollection<ISequenceBehaviorRun> postCheck_beh = null;
                IBehaviorResult result = null;
                try
                {
                    if(Module.CurBehaviorIdx >= 0 && Module.CurBehaviorIdx < Seqgroup.Count)
                    {
                        Seqgroup[Module.CurBehaviorIdx].SequenceBehaviorStateTransition(new SequenceBehaviorRunningState());
                        //Thread.Sleep(2000);
                    }
                    preCheck_beh = Module.GetPreCheckBehavior();//«¡∏ÆÆc
                    if (preCheck_beh != null && preCheck_beh.Count != 0)
                    {
                        foreach (var beh in preCheck_beh)
                        {
                            result = beh.Run().Result;
                            if (result.ErrorCode != EventCodeEnum.NONE)
                            {
                                Module.SetExecutionResult(result);
                                var rootCause = result.GetRootCause();
                                LoggerManager.Error("[behavior Previous check return error.]\n" +
                                    rootCause + "\n" +
                                    $"Reason of error : {result.Reason}" +
                                    $"behavior Previous check class : {beh}\n" +
                                    $"Current Behavior class name : {cur_beh}\n" +
                                    $"Card change State : {this.GetState().ToString()}\n");
                                    
                                this.CardChangeModule().UpdateErrorMessage("[behavior Previous check return error.]\n" +
                                    rootCause + "\n" +
                                    $"Reason of error : {result.Reason}" +
                                    $"behavior Previous check class : {beh}\n" +
                                    $"Current Behavior class name : {cur_beh}\n" +
                                    $"Card change State : {this.GetState().ToString()}\n");

                                retVal = Module.InnerStateTransition(new CardChangeErrorState(Module));
                                if (Module.CurBehaviorIdx >= 0 && Module.CurBehaviorIdx < Seqgroup.Count)
                                {
                                    Seqgroup[Module.CurBehaviorIdx].SequenceBehaviorStateTransition(new SequenceBehaviorErrorState());
                                }
                            }
                        }
                        LoggerManager.Debug($"***********Behavior Previous Check Success**********");
                    }
                    else
                    {
                        LoggerManager.Debug($"Behavior Previous behavior list is null");
                    }

                    result =  cur_beh.Run().Result;

                    if (result.ErrorCode == EventCodeEnum.NONE)
                    {
                        if (GetState() == CardChangeModuleStateEnum.PAUSE) 
                        {
                            Module.InnerStateTransition(new CardChangePauseState(Module));
                        }
                        else
                        {
                            postCheck_beh = Module.GetPostCheckBehavior();//∆˜Ω∫∆ÆÆc
                            if (postCheck_beh != null && postCheck_beh.Count != 0)
                            {
                                foreach (var beh in postCheck_beh)
                                {
                                    result = beh.Run().Result;
                                    if (result.ErrorCode != EventCodeEnum.NONE)
                                    {
                                        Module.SetExecutionResult(result);
                                        var rootCause = result.GetRootCause();
                                        LoggerManager.Error("[behavior Post check return error.]\n" +
                                            rootCause + "\n" +
                                            $"Reason of error : {result.Reason}" +
                                                $"behavior Post check class : {beh}\n" +
                                            $"Current Behavior class name : {cur_beh}\n" +
                                            $"Card change State : {this.GetState().ToString()}\n");

                                        this.CardChangeModule().UpdateErrorMessage("[behavior Post check return error.]\n" +
                                            rootCause + "\n" +
                                            $"Reason of error : {result.Reason}" +
                                                $"behavior Post check class : {beh}\n" +
                                            $"Current Behavior class name : {cur_beh}\n" +
                                            $"Card change State : {this.GetState().ToString()}\n");
                                        if (Module.CurBehaviorIdx >= 0 && Module.CurBehaviorIdx < Seqgroup.Count)
                                        {
                                            Seqgroup[Module.CurBehaviorIdx].SequenceBehaviorStateTransition(new SequenceBehaviorErrorState());
                                        }
                                        retVal = Module.InnerStateTransition(new CardChangeErrorState(Module));
                                        return retVal;
                                    }
                                }
                                LoggerManager.Debug($"***********Behavior Post Check Success**********");
                            }
                            else
                            {
                                LoggerManager.Debug($"Behavior Post behavior list is null");
                            }
                            if (Module.CurBehaviorIdx >= 0 && Module.CurBehaviorIdx < Seqgroup.Count)
                            {
                                Seqgroup[Module.CurBehaviorIdx].SequenceBehaviorStateTransition(new SepenceBehaviorDoneState());
                            }
                            cur_beh = Module.GetNextBehavior() as SequenceBehavior;
                            if(cur_beh is IEndBehavior)
                            {
                                if (Module.GetCardDockingStatus() == EnumCardDockingStatus.DOCKING)
                                {
                                    retVal = Module.CardChangeModule().LoaderNotifyCardStatus();

                                    Module.SetCardDockingStatus(EnumCardDockingStatus.DOCKED);
                                }
                                else if (Module.GetCardDockingStatus() == EnumCardDockingStatus.UNDOCKING)
                                {
                                    retVal = Module.CardChangeModule().LoaderNotifyCardStatus();

                                    Module.SetCardDockingStatus(EnumCardDockingStatus.UNDOCKED);
                                }
                                else
                                {
                                    LoggerManager.Debug($"CardChangeMaintenanceState | Card Docking Status is an invalid value. GetCardDockingStatus(): {Module.GetCardDockingStatus()}");
                                }
                                retVal = Module.InnerStateTransition(new CardChangeDoneState(Module));
                            }
                        }
                    }
                    else
                    {
                        Module.SetExecutionResult(result);
                        var rootCause = result.GetRootCause();
                        LoggerManager.Error($"Card change Failed. {rootCause}\n" +
                            $"Reason of error : {result.Reason}" +
                            $"Current Behavior class name : {cur_beh}\n" +
                            $"Card change State : {this.GetState().ToString()}\n");


                        this.CardChangeModule().UpdateErrorMessage(rootCause +"\n"+
                            $"Reason of error : {result.Reason}" +
                            $"Current Behavior class name : {cur_beh}\n" +
                            $"Card change State : {this.GetState().ToString()}\n");
                        if (Module.CurBehaviorIdx >= 0 && Module.CurBehaviorIdx < Seqgroup.Count)
                        {
                            Seqgroup[Module.CurBehaviorIdx].SequenceBehaviorStateTransition(new SequenceBehaviorErrorState());
                        }
                        retVal = Module.InnerStateTransition(new CardChangeErrorState(Module));
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    this.CardChangeModule().UpdateErrorMessage("exception error" + $"\nCardChangeRunningState() : {cur_beh}");
                    if (Module.CurBehaviorIdx >= 0 && Module.CurBehaviorIdx < Seqgroup.Count)
                    {
                        Seqgroup[Module.CurBehaviorIdx].SequenceBehaviorStateTransition(new SequenceBehaviorErrorState());
                    }
                    retVal = Module.InnerStateTransition(new CardChangeErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public ISequenceBehaviorGroupItem GetIndexFromBehaviorID(ObservableCollection<ISequenceBehaviorGroupItem> behaviorCollection, String ID)
        {
            ISequenceBehaviorGroupItem retVal = null;
            try
            {
                foreach (var v in behaviorCollection)
                {
                    if (v.BehaviorID == ID)
                    {
                        retVal = v;
                        Module.CCStartPoint = behaviorCollection.IndexOf(retVal);
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

        public override Task<int> ExecuteCC()
        {
            int retVal = -1;
            try
            {     

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<int>(retVal);
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.RUNNING;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// soaking¿ª ¡¶ø‹«— ¥Ÿ∏• ∏µ‚µÈ¿∫ running¿Ã µ… ºˆ ¿÷¥¬ ªÛ≈¬.
    /// </summary>
    public class CardChangeSuspendedState : CardChangeStateBase
    {
        public CardChangeSuspendedState(CardChangeModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            //do nothing, soaking ªÛ≈¬ø°º≠ ¿Ã ªÛ≈¬∏¶ ∫∏∞Ì soaking idle -> running µ«¡ˆ æ µµ∑œ ∏∑«Ù¿÷¿Ω. 
            //TODO: Title RunListø°º≠ Running¿Œ ∏µ‚ æ¯¿ª ∂ß Suspend ∑Œ «•Ω√«œ±‚          
            
            return EventCodeEnum.NONE;
        }

        public override Task<int> ExecuteCC()
        {
            Task<int> retVal = Task.FromResult<int>(-1);
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.SUSPENDED;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }
    }

    public class CardChangeCheckState : CardChangeStateBase
    {
        public CardChangeCheckState(CardChangeModule module) : base(module)
        {
        }
        public override Task<int> ExecuteCC()
        {
            Task<int> retVal = Task.FromResult<int>(0);
            try
            {

            Module.CCStartPoint++;
            Module.InnerStateTransition(new CardChangeRunningState(Module));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.CHECK;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }
    }

    public class CardChangeDoneState : CardChangeStateBase
    {
        public CardChangeDoneState(CardChangeModule module) : base(module)
        {
        }
        public override Task<int> ExecuteCC()
        {
            Task<int> retVal = Task.FromResult<int>(0);
            try
            {

            Module.InnerStateTransition(new CardChangeRunningState(Module));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.DONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }
        public override EventCodeEnum Execute()
        {
            this.CardChangeModule().UpdateErrorMessage("");
            ObservableCollection<ISequenceBehaviorGroupItem> Seqgroup = Module.GetCCCommandParamCollection();
            if (Module.CurBehaviorIdx == Seqgroup.Count)
            {
                foreach (var idx in Seqgroup)
                {
                    idx.SequenceBehaviorStateTransition(new SequenceBehaviorIdleState());
                }
                Module.CurBehaviorIdx = 0;
            }

            // ¿ÃªÁ∞°æﬂ«‘.
            if (Module.GetWaitForCardPermitEnable() == true && 
                Module.IsExistCard() &&
                (Module.GPCCBehavior is GP_DockPCardTopPlate ||
                Module.GPCCBehavior is GOP_DockPCardTopPlate)
                )
            {
                Module.CardChangeModule().WaitForCardPermission();
            }

            return EventCodeEnum.UNDEFINED;
        }
    }

    public class CardChangeErrorState : CardChangeStateBase
    {
        public CardChangeErrorState(CardChangeModule module) : base(module)
        {
        }
        public override async Task<int> ExecuteCC()
        {
            int retVal = 0;
            try
            {
            if (Module.CCRecoveryState == CardChangeModuleStateEnum.RECOVERY_RETRY)
            {
                Module.InnerStateTransition(new CardChangeRetryState(Module));
                retVal = await Module.CardChangeState.ExecuteCC();
            }
            else if (Module.CCRecoveryState == CardChangeModuleStateEnum.RECOVERY_REVERSE)
            {
                Module.InnerStateTransition(new CardChangeReverseState(Module));
                retVal = await Module.CardChangeState.ExecuteCC();
            }
            else if (Module.CCRecoveryState == CardChangeModuleStateEnum.RECOVERY_MENUAL)
                Module.InnerStateTransition(new CardChangeMenualState(Module));
            else if (Module.CCRecoveryState == CardChangeModuleStateEnum.END)
            {
                Module.InnerStateTransition(new CardChangeEndState(Module));
                retVal = await Module.CardChangeState.ExecuteCC();
            }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.ERROR;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum Execute()
        {
            return EventCodeEnum.UNDEFINED;
        }

    }

    public class CardChangeRetryState : CardChangeStateBase
    {
        public CardChangeRetryState(CardChangeModule module) : base(module)
        {
        }
        public override Task<int> ExecuteCC()
        {
            int retVal = -1;
            return Task.FromResult<int>(retVal);
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.RECOVERY_RETRY;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }
    }

    public class CardChangeReverseState : CardChangeStateBase
    {
        public CardChangeReverseState(CardChangeModule module) : base(module)
        {

        }

        public override Task<int> ExecuteCC()
        {
            int retVal = -1;
            try
            {


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<int>(retVal);
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.RECOVERY_REVERSE;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }
    }

    public class CardChangeMenualState : CardChangeStateBase
    {
        public CardChangeMenualState(CardChangeModule module) : base(module)
        {

        }
        public override Task<int> ExecuteCC()
        {
            int retVal = -1;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<int>(retVal);
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.RECOVERY_MENUAL;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }
    }

    public class CardChangeEndState : CardChangeStateBase
    {
        public CardChangeEndState(CardChangeModule module) : base(module)
        {

        }
        public override Task<int> ExecuteCC()
        {
            int retVal = -1;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<int>(retVal);
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.END;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }
    }
    public class CardChangeMaintenanceState : CardChangeStateBase
    {

        bool retry_type = false;//Continue¿Œ¡ˆ Step Up¿Œ¡ˆ(Countinue : true, Step Up: false)
        public CardChangeMaintenanceState(CardChangeModule module, bool retry_type = false) : base(module)
        {
            this.retry_type = retry_type;
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            ObservableCollection<ISequenceBehaviorGroupItem> Seqgroup = Module.GetCCCommandParamCollection();
            try
            {
                if (Module.CurBehaviorIdx >= 0 && Module.CurBehaviorIdx < Seqgroup.Count)
                {
                    Seqgroup[Module.CurBehaviorIdx].SequenceBehaviorStateTransition(new SequenceBehaviorRunningState());
                    //Thread.Sleep(2000);
                }
                SequenceBehavior cur_beh = Module.GetBehavior() as SequenceBehavior;
                ObservableCollection<ISequenceBehaviorRun> preCheck_beh = null;
                ObservableCollection<ISequenceBehaviorRun> postCheck_beh = null;
                IBehaviorResult result = null;
                try
                {
                    preCheck_beh = Module.GetPreCheckBehavior();//«¡∏ÆÆc
                    if (preCheck_beh != null && preCheck_beh.Count != 0)
                    {
                        foreach (var beh in preCheck_beh)
                        {
                            result = beh.Run().Result;
                            if (result.ErrorCode != EventCodeEnum.NONE)
                            {
                                Module.SetExecutionResult(result);
                                var rootCause = result.GetRootCause();
                                LoggerManager.Error("[behavior Previous check return error.]\n" +
                                    rootCause + "\n" +
                                    $"Reason of error : {result.Reason}" +
                                    $"behavior Previous check class : {beh}\n" +
                                    $"Current Behavior class name : {cur_beh}\n" +
                                    $"Card change State : {this.GetState().ToString()}\n");

                                this.CardChangeModule().UpdateErrorMessage("[behavior Previous check return error.]\n" +
                                    rootCause + "\n" +
                                    $"Reason of error : {result.Reason}" +
                                    $"behavior Previous check class : {beh}\n" +
                                    $"Current Behavior class name : {cur_beh}\n" +
                                    $"Card change State : {this.GetState().ToString()}\n");
                                if (Module.CurBehaviorIdx >= 0 && Module.CurBehaviorIdx < Seqgroup.Count)
                                {
                                    Seqgroup[Module.CurBehaviorIdx].SequenceBehaviorStateTransition(new SequenceBehaviorErrorState());
                                }
                                retVal = Module.InnerStateTransition(new CardChangeErrorState(Module));
                                return retVal;
                            }
                        }
                        LoggerManager.Debug($"***********Behavior Previous Check Success**********");
                    }
                    else
                    {
                        LoggerManager.Debug($"***********Behavior Previous behavior list is null**********");
                    }

                    result = cur_beh.Run().Result;

                    if (result.ErrorCode == EventCodeEnum.NONE)
                    {
                        postCheck_beh = Module.GetPostCheckBehavior();//∆˜Ω∫∆ÆÆc
                        if (postCheck_beh != null && postCheck_beh.Count != 0)
                        {
                            foreach (var beh in postCheck_beh)
                            {
                                result = beh.Run().Result;
                                if (result.ErrorCode != EventCodeEnum.NONE)
                                {
                                    Module.SetExecutionResult(result);
                                    var rootCause = result.GetRootCause();
                                    LoggerManager.Error("[behavior Post check return error.]\n" +
                                        rootCause + "\n" +
                                        $"Reason of error : {result.Reason}" +
                                            $"behavior Post check class : {beh}\n" +
                                        $"Current Behavior class name : {cur_beh}\n" +
                                        $"Card change State : {this.GetState().ToString()}\n");

                                    this.CardChangeModule().UpdateErrorMessage("[behavior Post check return error.]\n" +
                                        rootCause + "\n" +
                                        $"Reason of error : {result.Reason}" +
                                            $"behavior Post check class : {beh}\n" +
                                        $"Current Behavior class name : {cur_beh}\n" +
                                        $"Card change State : {this.GetState().ToString()}\n");
                                    if (Module.CurBehaviorIdx >= 0 && Module.CurBehaviorIdx < Seqgroup.Count)
                                    {
                                        Seqgroup[Module.CurBehaviorIdx].SequenceBehaviorStateTransition(new SequenceBehaviorErrorState());
                                    }
                                    retVal = Module.InnerStateTransition(new CardChangeErrorState(Module));
                                    return retVal;
                                }
                            }
                            LoggerManager.Debug($"***********Behavior Post Check Success**********");
                        }
                        else
                        {
                            LoggerManager.Debug($"***********Behavior Post behavior list is null**********");
                        }
                        if (Module.CurBehaviorIdx >= 0 && Module.CurBehaviorIdx < Seqgroup.Count)
                        {
                            Seqgroup[Module.CurBehaviorIdx].SequenceBehaviorStateTransition(new SepenceBehaviorDoneState());
                        }
                        if (this.retry_type == false)
                        {
                            Module.InnerStateTransition(new CardChangePauseState(Module));
                        }
                        cur_beh = Module.GetNextBehavior() as SequenceBehavior;
                        if (cur_beh is IEndBehavior)
                        {
                            if (Module.GetCardDockingStatus() == EnumCardDockingStatus.DOCKING)
                            {
                                retVal = Module.CardChangeModule().LoaderNotifyCardStatus();
                                Module.SetCardDockingStatus(EnumCardDockingStatus.DOCKED);

                                Module.MetroDialogManager().ShowMessageDialog("Message", "Card docking recovery completed. Please remove carrier from the cell.", EnumMessageStyle.Affirmative);
                            }
                            else if (Module.GetCardDockingStatus() == EnumCardDockingStatus.UNDOCKING)
                            {
                                retVal = Module.CardChangeModule().LoaderNotifyCardStatus();

                                Module.SetCardDockingStatus(EnumCardDockingStatus.UNDOCKED);
                            }
                            else
                            {
                                LoggerManager.Debug($"CardChangeMaintenanceState | Card Docking Status is an invalid value. GetCardDockingStatus(): {Module.GetCardDockingStatus()}");
                            }
                            retVal = Module.InnerStateTransition(new CardChangeDoneState(Module));
                        }
                    }
                    else
                    {
                        if (Module.CurBehaviorIdx >= 0 && Module.CurBehaviorIdx < Seqgroup.Count)
                        {
                            Seqgroup[Module.CurBehaviorIdx].SequenceBehaviorStateTransition(new SequenceBehaviorErrorState());
                        }
                        {
                            Module.SetExecutionResult(result);
                            var rootCause = result.GetRootCause();
                            LoggerManager.Error($"Card change Failed. {rootCause}\n" +
                                $"Reason of error : {result.Reason}" +
                                $"Current Behavior class name : {cur_beh}\n" +
                                $"Card change State : {this.GetState().ToString()}\n");

                            //Module.SetExecutionResult(result);

                            this.CardChangeModule().UpdateErrorMessage(rootCause + "\n" +
                                $"Reason of error : {result.Reason}" +
                                $"Current Behavior class name : {cur_beh}\n" +
                                $"Card change State : {this.GetState().ToString()}\n");

                            retVal = Module.InnerStateTransition(new CardChangeErrorState(Module));
                        }
                    }
                            
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    this.CardChangeModule().UpdateErrorMessage("exception error" + $"\nCardChangeRunningState() : {cur_beh}");
                    if (Module.CurBehaviorIdx >= 0 && Module.CurBehaviorIdx < Seqgroup.Count)
                    {
                        Seqgroup[Module.CurBehaviorIdx].SequenceBehaviorStateTransition(new SequenceBehaviorErrorState());
                    }
                    retVal = Module.InnerStateTransition(new CardChangeErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.STEP_UP;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }
    }
    public class CardChangeAbortState : CardChangeStateBase
    {
        public CardChangeAbortState(CardChangeModule module) : base(module)
        {
            ObservableCollection<ISequenceBehaviorGroupItem> Seqgroup = Module.GetCCCommandParamCollection();
            foreach (var idx in Seqgroup)
            {
                idx.SequenceBehaviorStateTransition(new SequenceBehaviorIdleState());
            }
            Module.CurBehaviorIdx = 0;
        }
        public override EventCodeEnum Execute()
        {
            //this.ClearState();
            //Module.InnerStateTransition(new CardChangeIdleState(Module));
            Thread.Sleep(20);
            return EventCodeEnum.UNDEFINED;
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.ABORT;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum Pause()
        {
            throw new NotImplementedException();
        }
    }
    public class CardChangePauseState : CardChangeStateBase
    {
        public CardChangePauseState(CardChangeModule module) : base(module)
        {
            //ObservableCollection<ISequenceBehaviorGroupItem> Seqgroup = this.CardChangeModule()?.GetCcDockCollection();
            ObservableCollection<ISequenceBehaviorGroupItem> Seqgroup = Module.GetCCCommandParamCollection();
            if (Module.CurBehaviorIdx >= 0 && Module.CurBehaviorIdx < Seqgroup.Count)
            {
                if (Module.CurBehaviorIdx + 1 < Seqgroup.Count)//æ»¿¸¿Âƒ°
                {
                    Seqgroup[Module.CurBehaviorIdx + 1].SequenceBehaviorStateTransition(new SequenceBehaviorPausedState());
                }
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
            }
            return retVal;
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.PAUSE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum Pause()
        {
            throw new NotImplementedException();
        }
    }
}

