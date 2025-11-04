using LogModule;
using System;
using System.ComponentModel;

namespace ProberInterfaces.Command
{
    public abstract class CommandStateBase : INotifyPropertyChanged
    {
        

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public abstract CommandStateEnum GetState();
        
        private CommandStateEnum _State;
        public CommandStateEnum State
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

        public abstract void Execute(CommandStateEnum state);
    }

    public abstract class CommandDescState : CommandStateBase
    {
        private ProbeCommand _Command;

        public ProbeCommand Command
        {
            get { return _Command; }
            private set { _Command = value; }
        }

        public CommandDescState(ProbeCommand command)
        {
            this.Command = command;
        }

        public override void Execute(CommandStateEnum state)
        {
            try
            {
                switch (state)
                {
                    case CommandStateEnum.ISSUE:
                        Command.CommandState = new CommandIssueState(Command);
                        break;
                    case CommandStateEnum.REJECTED:
                        Command.CommandState = new CommandRejectedState(Command);
                        break;
                    case CommandStateEnum.REQUESTED:
                        Command.CommandState = new CommandRequestedState(Command);
                        break;
                    case CommandStateEnum.ABORTED:
                        Command.CommandState = new CommandAbortedState(Command);
                        break;
                    case CommandStateEnum.DONE:
                        Command.CommandState = new CommandDoneState(Command);
                        break;
                    case CommandStateEnum.ERROR:
                        Command.CommandState = new CommandErrorState(Command);

                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }

    public class CommandNoCommandState : CommandDescState
    {
        public CommandNoCommandState(ProbeCommand sequenceJob) : base(sequenceJob)
        {
            State = CommandStateEnum.NOCOMMAND;
        }

        public override CommandStateEnum GetState()
        {
            return State;
        }
    }

    public class CommandIssueState : CommandDescState
    {
        public CommandIssueState(ProbeCommand sequenceJob) : base(sequenceJob)
        {
            State = CommandStateEnum.ISSUE;
        }

        public override CommandStateEnum GetState()
        {
            return State;
        }
    }

    public class CommandRejectedState : CommandDescState
    {
        public CommandRejectedState(ProbeCommand sequenceJob) : base(sequenceJob)
        {
            State = CommandStateEnum.REJECTED;
        }

        public override CommandStateEnum GetState()
        {
            return State;
        }
    }

    public class CommandRequestedState : CommandDescState
    {
        public CommandRequestedState(ProbeCommand sequenceJob) : base(sequenceJob)
        {
            State = CommandStateEnum.REQUESTED;
        }

        public override CommandStateEnum GetState()
        {
            return State;
        }
    }

    public class CommandAbortedState : CommandDescState
    {
        public CommandAbortedState(ProbeCommand sequenceJob) : base(sequenceJob)
        {
            State = CommandStateEnum.ABORTED;
        }

        public override CommandStateEnum GetState()
        {
            return State;
        }
    }
    
    public class CommandDoneState : CommandDescState
    {
        public CommandDoneState(ProbeCommand sequenceJob) : base(sequenceJob)
        {
            State = CommandStateEnum.DONE;
        }

        public override CommandStateEnum GetState()
        {
            return State;
        }
    }


    public class CommandErrorState : CommandDescState
    {
        public CommandErrorState(ProbeCommand sequenceJob) : base(sequenceJob)
        {
            State = CommandStateEnum.ERROR;
        }

        public override CommandStateEnum GetState()
        {
            return State;
        }
    }
}
