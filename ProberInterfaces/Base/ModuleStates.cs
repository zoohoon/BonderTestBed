using LogModule;
using System;
using System.ComponentModel;
using ProberInterfaces.Temperature;
using ProberErrorCode;

namespace ProberInterfaces
{
    public abstract class ModuleStateBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public bool IsInfo = false;

        private ModuleStateEnum _State;
        public ModuleStateEnum State
        {
            get { return _State; }
            set
            {
                if (value != _State)
                {
                    _State = value;
                    NotifyPropertyChanged("State");
                }
            }
        }

        public ModuleStateBase()
        {
        }

        public abstract ModuleStateEnum GetState();
        public abstract EventCodeEnum StateTransition(ModuleStateEnum state);
    }

    public abstract class ModuleDescState : ModuleStateBase
    {
        private IStateModule _SequenceJob;

        public IStateModule SequenceJob
        {
            get { return _SequenceJob; }
            private set { _SequenceJob = value; }
        }

        public ModuleDescState(IStateModule sequenceJob)
        {
            this.SequenceJob = sequenceJob;
        }


        public override EventCodeEnum StateTransition(ModuleStateEnum state)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.GetState() != state)
                {
                    string reason = null;

                    switch (state)
                    {
                        case ModuleStateEnum.UNDEFINED:
                            SequenceJob.StateTransition(new ModuleUndefinedState(SequenceJob));
                            break;
                        case ModuleStateEnum.INIT:
                            SequenceJob.StateTransition(new ModuleInitState(SequenceJob));
                            break;
                        case ModuleStateEnum.IDLE:
                            SequenceJob.StateTransition(new ModuleIdleState(SequenceJob));

                            break;
                        case ModuleStateEnum.RUNNING:
                            SequenceJob.StateTransition(new ModuleRunningState(SequenceJob));

                            break;
                        case ModuleStateEnum.PENDING:
                            SequenceJob.StateTransition(new ModulePendingState(SequenceJob));

                            break;
                        case ModuleStateEnum.PAUSED:
                            SequenceJob.StateTransition(new ModulePausedState(SequenceJob));

                            break;

                        case ModuleStateEnum.SUSPENDED:
                            SequenceJob.StateTransition(new ModuleSuspendedState(SequenceJob));

                            break;
                        case ModuleStateEnum.ABORT:
                            SequenceJob.StateTransition(new ModuleAbortState(SequenceJob));

                            break;
                        case ModuleStateEnum.RECOVERY:
                            SequenceJob.StateTransition(new ModuleRecoveryState(SequenceJob));

                            break;
                        case ModuleStateEnum.DONE:
                            SequenceJob.StateTransition(new ModuleDoneState(SequenceJob));
                            break;
                        case ModuleStateEnum.PAUSING:
                            SequenceJob.StateTransition(new ModulePaussingState(SequenceJob));
                            break;
                        case ModuleStateEnum.RESUMMING:
                            SequenceJob.StateTransition(new ModuleResummingState(SequenceJob));
                            break;
                        case ModuleStateEnum.ERROR:
                        default:
                            SequenceJob.StateTransition(new ModuleErrorState(SequenceJob));
                            break;
                    }

                    if (SequenceJob.TransitionInfo.Count > 100000)
                    {
                        SequenceJob.TransitionInfo.Clear();
                    }

                    LoggerManager.SetModuleState(SequenceJob.GetType().Name, state.ToString());

                    LoggerManager.Debug($"[STATE TRANSITION] {SequenceJob} | {GetState()} TO {state} [Previous TO Current]", isInfo: IsInfo);

                    SequenceJob.TransitionInfo.Add(new TransitionInfo(SequenceJob.ModuleState.State, reason));

                    if (state == ModuleStateEnum.RUNNING && SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        string msg = SequenceJob.GetModuleMessage();

                        if (msg != "" && msg != string.Empty)
                        {
                            int index = SequenceJob.LoaderController().GetChuckIndex();

                            if (!(SequenceJob is ITempController))
                            {
                                SequenceJob.LoaderController().SetTitleMessage(index, msg);
                                //SequenceJob.NotifyManager().SetLastStageMSG(msg);
                            }
                        }
                    }

                    retval = EventCodeEnum.STATE_TRANSITION_OK;
                }
                else
                {
                    retval = EventCodeEnum.STATE_TRANSITION_SKIP;
                }

                return retval;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //protected void SequenceJobExecute()
        //{
        //    int pauseResult = -1;
        //    //if(SequenceJob.InnerState.GetModuleState()==ModuleStateEnum.RUNNING)
        //    //{

        //    //}
        //    pauseResult = (int)SequenceJob.InnerState.Execute();
        //    StateTransition(SequenceJob.InnerState.GetModuleState());
        //    if (SequenceJob.InnerState.GetModuleState() == ModuleStateEnum.RUNNING)
        //    {

        //    }
        //}

        //public override int Pause()
        //{
        //    int retVal = -1;
        //    int pauseResult = -1;

        //    if (SequenceJob.InnerState.GetModuleState() != ModuleStateEnum.PAUSED)
        //    {
        //        pauseResult = (int)SequenceJob.InnerState.Pause();
        //        StateTransition(SequenceJob.InnerState.GetModuleState());
        //    }

        //    return retVal;
        //}

        //public override int Resume()
        //{
        //    int retVal = 0;

        //    if (SequenceJob.InnerState.GetModuleState() == ModuleStateEnum.PAUSED)
        //    {
        //        SequenceJob.InnerStateTransition(SequenceJob.PreInnerState);
        //        StateTransition(SequenceJob.InnerState.GetModuleState());
        //    }

        //    return retVal;
        //}

        //public override int Abort()
        //{
        //    int retVal = 0;
        //    int pauseResult = -1;

        //    if (SequenceJob.InnerState.GetModuleState() == ModuleStateEnum.PAUSED)
        //    {
        //        pauseResult = (int)SequenceJob.InnerState.Abort();
        //        StateTransition(SequenceJob.InnerState.GetModuleState());
        //    }

        //    return retVal;
        //}
    }

    public class ModuleUndefinedState : ModuleDescState
    {
        public ModuleUndefinedState(IStateModule sequenceJob) : base(sequenceJob)
        {
            State = ModuleStateEnum.UNDEFINED;
        }

        public override ModuleStateEnum GetState()
        {
            return State;
        }

    }
    public class ModuleInitState : ModuleDescState
    {
        public ModuleInitState(IStateModule sequenceJob) : base(sequenceJob)
        {
            State = ModuleStateEnum.INIT;
        }

        public override ModuleStateEnum GetState()
        {
            return State;
        }

    }
    public class ModuleIdleState : ModuleDescState
    {
        public ModuleIdleState(IStateModule sequenceJob) : base(sequenceJob)
        {
            State = ModuleStateEnum.IDLE;
        }
        public override ModuleStateEnum GetState()
        {
            return State;
        }
        //public override int Execute(ModuleStateEnum state, bool IsExecute = true)
        //{
        //    int retVal = -1;

        //    StateTransition(state);
        //    if (IsExecute == true)
        //    {
        //        SequenceJobExecute();
        //    }
        // SequenceJob.RunTokenSet.Update(); //=> Process RunTokens of module

        //    retVal = (int)state;

        //    return retVal;
        //}

    }
    public class ModuleRunningState : ModuleDescState
    {
        public ModuleRunningState(IStateModule sequenceJob) : base(sequenceJob)
        {
            State = ModuleStateEnum.RUNNING;
        }


        public override ModuleStateEnum GetState()
        {
            return State;
        }

    }

    public class ModulePendingState : ModuleDescState
    {
        public ModulePendingState(IStateModule sequenceJob) : base(sequenceJob)
        {
            State = ModuleStateEnum.PENDING;
        }


        public override ModuleStateEnum GetState()
        {
            return State;
        }

    }
    public class ModuleRecoveryState : ModuleDescState
    {
        public ModuleRecoveryState(IStateModule sequenceJob) : base(sequenceJob)
        {
            State = ModuleStateEnum.RECOVERY;
        }


        public override ModuleStateEnum GetState()
        {
            return State;
        }

    }

    public class ModulePausedState : ModuleDescState
    {
        public ModulePausedState(IStateModule sequenceJob) : base(sequenceJob)
        {
            State = ModuleStateEnum.PAUSED;
        }

        public override ModuleStateEnum GetState()
        {
            return State;
        }

    }

    public class ModuleSuspendedState : ModuleDescState
    {
        public ModuleSuspendedState(IStateModule sequenceJob) : base(sequenceJob)
        {
            State = ModuleStateEnum.SUSPENDED;
        }


        public override ModuleStateEnum GetState()
        {
            return State;
        }
    }
    public class ModuleAbortState : ModuleDescState
    {
        public ModuleAbortState(IStateModule sequenceJob) : base(sequenceJob)
        {
            State = ModuleStateEnum.ABORT;
        }


        public override ModuleStateEnum GetState()
        {
            return State;
        }
    }
    public class ModuleDoneState : ModuleDescState
    {
        public ModuleDoneState(IStateModule sequenceJob) : base(sequenceJob)
        {
            State = ModuleStateEnum.DONE;
        }


        public override ModuleStateEnum GetState()
        {
            return State;
        }

    }
    public class ModulePaussingState : ModuleDescState
    {
        public ModulePaussingState(IStateModule sequenceJob) : base(sequenceJob)
        {
            State = ModuleStateEnum.PAUSING;
        }


        public override ModuleStateEnum GetState()
        {
            return State;
        }

    }
    public class ModuleResummingState : ModuleDescState
    {
        public ModuleResummingState(IStateModule sequenceJob) : base(sequenceJob)
        {
            State = ModuleStateEnum.RESUMMING;
        }


        public override ModuleStateEnum GetState()
        {
            return State;
        }

    }
    public class ModuleErrorState : ModuleDescState
    {
        public ModuleErrorState(IStateModule sequenceJob) : base(sequenceJob)
        {
            State = ModuleStateEnum.ERROR;
        }

        public override ModuleStateEnum GetState()
        {
            //if (SequenceJob.Sequencer.RunStatus != EnumEngineStatus.PAUSED)
            //{
            //    SequenceJob.Sequencer.PauseSequencer(SequenceJob);
            //}

            return State;
        }
    }

    public class TransitionInfo
    {
        public ModuleStateEnum state;
        public DateTime TransitionTime;
        public string Reason;
        public TransitionInfo(ModuleStateEnum State)
        {
            try
            {
                this.state = State;
                TransitionTime = DateTime.Now;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public TransitionInfo(ModuleStateEnum State, string reason)
        {
            try
            {
                this.state = State;
                TransitionTime = DateTime.Now;
                Reason = reason;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    //public class ModulePaused : ModuleDescState
    //{
    //    public ModulePaused(ISequenceJob sequenceJob) : base(sequenceJob)
    //    {
    //        State = ModuleStateEnum.SUSPENDED;
    //    }

    //    public override ModuleStateEnum GetState()
    //    {
    //        return State;
    //    }

    //}
}

