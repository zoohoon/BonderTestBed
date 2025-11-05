using Autofac;
using System;
using System.Collections.Generic;
using ProberErrorCode;

namespace ProberInterfaces.Command
{
    public interface ICommandManager : IFactoryModule,IModule
    {
        IEnumerable<string> InternalCommandlist { get; }

        IContainer GetCommandContainer();

        //string GetCommand(string commandName);

        void AddCommandParameters(List<CommandDescriptor> list);

        bool SetCommand<T>(object sender, IProbeCommandParameter parameter = null) where T : IProbeCommand;

        bool SetCommand(object sender, string commnadname, IProbeCommandParameter parameter = null);

        bool ProcessIfRequested<TCommand>(
            ProberInterfaces.IStateModule module, CommandCondition condition = null, CommandAction doAction = null, CommandAction abortAction = null)
            where TCommand : IProbeCommand;
    }
    
    public class CommandCondition
    {
        private Func<bool> _Func;

        public CommandCondition(Func<bool> func = null)
        {
            _Func = func;
        }

        public bool Execute()
        {
            return _Func?.Invoke() ?? true;
        }
        
        public static CommandCondition NONE
        {
            get { return new CommandCondition(); }
        }

        public static implicit operator CommandCondition(Func<bool> func)
        {
            return new CommandCondition(func);
        }
    }

    public class CommandAction
    {
        private Action _Action;

        public CommandAction(Action action = null)
        {
            _Action = action;
        }

        public void Execute()
        {
            _Action?.Invoke();
        }

        public static CommandAction NONE
        {
            get { return new CommandAction(); }
        }

        public static implicit operator CommandAction(Action action)
        {
            return new CommandAction(action);
        }
    }
    
    public class ProbeCommandInfo
    {
        public string HashCode { get; set; }
        public string Description { get; set; }
        public IProbeCommand Command { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EventCodeEnum Status { get; set; }
    }
}
