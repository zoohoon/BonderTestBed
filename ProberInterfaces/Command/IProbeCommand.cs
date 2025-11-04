using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using SecuritySystem;
using LogModule;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces.Command
{
    public enum CommandStateEnum
    {
        NOCOMMAND,
        ISSUE,              // 커맨드 토큰을 발행하고 나서 받는 쪽이 토큰을 받기 전까지의 상태 (Sync를 맞추기 위해서 사용)
        REJECTED,           // 커맨드 토큰이 거부된 경우
        REQUESTED,          // 커맨드 토큰을 받는 쪽에서 받았고, 아직 처리 중인 상태
        //PROCESSING,
        ABORTED,
        DONE,
        ERROR
    }

    #region => IProbeCommand, IProbeCommandToken
    public interface IProbeCommand : IProbeCommandToken, IFactoryModule
    {
        bool Execute();
    }

    public interface IProbeCommandToken : IFactoryModule
    {
        string Name { get; }
        string SubjectInfo { get; set; }
        IStateModule Target { get; }
        IStateModule Sender { get; set; }
        IProbeCommandParameter Parameter { get; set; }
        CommandStateEnum GetState();
        void SetRejected();
        void SetRequested();
        void SetDone();
        void SetAbort();
        void SetError();
    }
    #endregion

    #region => Predefined Command Parameters(NoCommand, NoCommandParam, CallbackParam, AcknowledgeParameter)
    [Serializable]
    public class NoCommandParam : ProbeCommandParameter
    {
        public NoCommandParam()
        {
            Command = CommandNameGen.Generate(typeof(NoCommand));
        }
    }

    [Serializable]
    public class CallbackParam : ProbeCommandParameter
    {
        public CallbackParam() { }

        public CallbackParam(string callbackCommand)
        {
            Command = callbackCommand;
        }
    }

    [Serializable]
    public class AcknowledgeParam : ProbeCommandParameter
    {
        public AcknowledgeParam()
        {
            this.Command = CommandNameGen.Generate(typeof(NoCommand));
        }

        public AcknowledgeParam(string ack, string nack)
        {
            try
            {
                this.Command = CommandNameGen.Generate(typeof(NoCommand));
                this.ACK = ack;
                this.NACK = nack;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private string _ACK;
        public string ACK
        {
            get { return _ACK; }
            set { _ACK = value; }
        }

        private string _NACK;
        public string NACK
        {
            get { return _NACK; }
            set { _NACK = value; }
        }
    }
    #endregion

    #region => Predefined Command
    [Serializable]
    public class NoCommand : ProbeCommand
    {
        public NoCommand()
        {
            try
            {
                Name = CommandNameGen.Generate(GetType());
                Parameter = new NoCommandParam();

                CommandState = new CommandNoCommandState(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override bool Execute()
        {
            return true;
        }
    }
    #endregion

    #region => IProbeCommandParameter, ProbeCommandParameter
    public interface IProbeCommandParameter
    {
        string Command { get; set; }
    }

    [Serializable]
    public abstract class ProbeCommandParameter : IProbeCommandParameter
    {
        public string Command { get; set; }
    }
    #endregion


    public abstract class ProbeCommand : Autofac.Module, IProbeCommand
    {
        public ProbeCommand()
        {
            CommandState = new CommandIssueState(this);
        }

        private bool IsInfo = false;
        public string Name { get; protected set; }
        public string SubjectInfo { get; set; }

        public IProbeCommandParameter Parameter { get; set; }

        public CommandStateBase CommandState { get; set; }

        public IStateModule Target { get; set; }

        public IStateModule Sender { get; set; }

        public abstract bool Execute();

        private object commandLock = new object();
        protected bool SetCommandTo(IStateModule target)
        {
            bool canInject = false;

            lock (commandLock)
            {
                Target = target;

                bool isNoCommandFlag = target.CommandRecvSlot.IsNoCommand();
                bool isNoProcCommandFlag = target.CommandRecvProcSlot.IsNoCommand();
                bool isCanExecuteFlag = target.CanExecute(this);
                bool isOverlapCommandFlag = false;

                if (isNoCommandFlag == false)
                {
                    var callerclassname = new StackTrace().GetFrame(1).GetMethod().ReflectedType.Name;
                    var recvtokenname = target.CommandRecvSlot?.Token?.ToString().Split('.');

                    LoggerManager.Debug($"[COMMAND REQUEST] [Sender: {Sender}] [Receiver: {target}] Has Command Recv Slot :{target.CommandRecvSlot?.Token?.ToString()}", isInfo: IsInfo);

                    if (target.CommandRecvSlot.Token.GetState() != CommandStateEnum.NOCOMMAND)
                    {
                        if (recvtokenname != null)
                        {
                            if (recvtokenname[recvtokenname.Length - 1] == callerclassname)
                            {
                                isOverlapCommandFlag = true;
                            }
                        }
                    }

                    // command 가 있지만, slot에 있는 command 와 중복되는 command 가 아니라면 reject 하지 않게하기위해. 
                    if (isOverlapCommandFlag == false)
                    {
                        isNoCommandFlag = true;
                    }
                }

                if (isNoProcCommandFlag == false)
                {
                    var callerclassname = new StackTrace().GetFrame(1).GetMethod().ReflectedType.Name;
                    var recvtokenname = target.CommandRecvProcSlot?.Token?.ToString().Split('.');

                    LoggerManager.Debug($"[COMMAND REQUEST] [Sender: {Sender}] [Receiver: {target}] Has Command Recv Slot :{target.CommandRecvProcSlot?.Token?.ToString()}", isInfo: IsInfo);

                    if (target.CommandRecvProcSlot.Token.GetState() != CommandStateEnum.NOCOMMAND)
                    {
                        if (recvtokenname != null)
                        {
                            if (recvtokenname[recvtokenname.Length - 1] == callerclassname)
                            {
                                isOverlapCommandFlag = true;
                            }
                        }
                    }

                    // command 가 있지만, slot에 있는 command 와 중복되는 command 가 아니라면 reject 하지 않게하기위해. 
                    if (isOverlapCommandFlag == false)
                    {
                        isNoProcCommandFlag = true;
                    }
                }

                canInject = isNoCommandFlag && isNoProcCommandFlag && isCanExecuteFlag & !isOverlapCommandFlag;

                try
                {
                    if (canInject)
                    {
                        LoggerManager.Debug($"[COMMAND REQUEST] [Sender: {Sender}] [Receiver: {target}] Command Name:{this.ToString()}, Module State : {target.ModuleState.GetState()}, Inner State : {target.GetModuleMessage()}", isInfo: IsInfo);

                        SetRequested();
                    }
                    else
                    {
                        LoggerManager.Debug($"Token Name = {this.GetType().FullName}, isNoCommandFlag = {isNoCommandFlag}, isNoProcCommandFlag={isNoProcCommandFlag}, isCanExecuteFlag = {isCanExecuteFlag}, isOverlapCommandFlag = {isOverlapCommandFlag}", isInfo: IsInfo);

                        LoggerManager.Debug($"[ProbeCommand], SetCommandTo() : Target State : {target.ModuleState.GetState()}", isInfo: IsInfo);

                        LoggerManager.Debug($"[COMMAND REJECT] [Sender: {Sender}] [Receiver: {target}] Command Name:{this.ToString()}:", isInfo: IsInfo);

                        SetRejected();
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return canInject;
        }

        public CommandStateEnum GetState()
        {
            CommandStateEnum retval = CommandStateEnum.NOCOMMAND;

            try
            {
                retval = CommandState.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #region => Command State Methods
        public void SetRejected()
        {
            try
            {
                CommandState.Execute(CommandStateEnum.REJECTED);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetRequested()
        {
            try
            {
                CommandState.Execute(CommandStateEnum.REQUESTED);

                if (Target != null)
                {
                    this.SubjectInfo = SecurityUtil.GetHashCode_SHA256((DateTime.Now.ToString() + Target.GetType().FullName));

                    if (Target.CommandInfo == null)
                    {
                        Target.CommandInfo = new CommandInformation();
                        Target.CommandInfo.SenderModule = Sender;
                        Target.CommandInfo.HashCode = this.SubjectInfo;
                    }
                    else
                    {
                        //Command가 이미 있으므로 묻고하기...
                    }

                    Target.CommandRecvSlot.SetToken(this);

                    LoggerManager.Debug($"SetRequested Target :\"{Target}\", Token:\"{this.ToString()}\" ");

                    if (Target.CommandRecvProcSlot != null)
                    {
                        LoggerManager.Debug($"\"{Target}\" has a Probe Command. The command is Processing.");
                    }

                    if (Sender != null)
                    {
                        Sender.CommandSendSlot.SetToken(this);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetDone()
        {
            try
            {
                CommandState.Execute(CommandStateEnum.DONE);

                if (Target != null)
                {
                    LoggerManager.Debug($"[COMMAND PERFORM] Command Name:{this.ToString()}:");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        public void SetAbort()
        {
            try
            {
                CommandState.Execute(CommandStateEnum.ABORTED);

                if (Target != null)
                {
                    Target.CommandRecvSlot.ClearToken();

                    if (Sender != null)
                    {
                        Sender.CommandSendSlot.ClearToken();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetError()
        {
            try
            {
                CommandState.Execute(CommandStateEnum.ERROR);

                if (Target != null)
                {
                    Target.CommandRecvSlot.ClearToken();

                    if (Sender != null)
                    {
                        Sender.CommandSendSlot.ClearToken();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        // 삭제하면 안되는 함수
        protected override void Load(ContainerBuilder builder)
        {
            try
            {
                var type = this.GetType();
                var Nullconstructor = type.GetConstructor(Type.EmptyTypes);

                var nestedTypes = type.GetInterfaces().Where(x => typeof(IProbeCommand).IsAssignableFrom(x)).ToList();

                Type foundType = null;

                //External
                if (nestedTypes.Count == 1)
                {
                    foundType = GetType();
                }
                // Internal
                else if (nestedTypes.Count == 2)
                {
                    foundType = nestedTypes.Where(item => item != typeof(IProbeCommand)).First();
                }
                else
                {
                    // ERROR
                }

                if (foundType != null)
                {
                    Name = CommandNameGen.Generate(foundType);

                    builder.Register(x => Nullconstructor.Invoke(new object[] { })).Named<ProbeCommand>(Name);
                }
                else
                {
                    // ERROR
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
    public class CommandInformation
    {
        public IStateModule SenderModule;
        public string HashCode;
    }
    public class CommandSlot : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public CommandSlot() : this(new NoCommand()) { }

        internal CommandSlot(IProbeCommandToken commandToken)
        {
            try
            {
                Token = commandToken;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private IProbeCommandToken _Token;
        public IProbeCommandToken Token
        {
            get { return _Token; }
            private set
            {
                if (value != _Token)
                {
                    _Token = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsNoCommand()
        {
            bool retval = false;

            try
            {
                retval = (GetState() == CommandStateEnum.NOCOMMAND);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsRequested<T>() where T : IProbeCommand
        {
            bool retval = false;

            try
            {
                retval = GetState() == CommandStateEnum.REQUESTED && Token is T;

                if (retval)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], IsRequested<T>() : {typeof(T).Name}, retval = {retval}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsRequested(string commandName = "")
        {
            bool retval = false;

            try
            {
                if (String.IsNullOrEmpty(commandName))
                {
                    retval = GetState() == CommandStateEnum.REQUESTED;

                    if (retval)
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], IsRequested() : retval = {retval}");
                    }
                }
                else
                {
                    retval = GetState() == CommandStateEnum.REQUESTED && Token.Name == commandName;

                    if (retval)
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], IsRequested() : commandName = {commandName}, retval = {retval}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public CommandStateEnum GetState()
        {
            CommandStateEnum retval = CommandStateEnum.NOCOMMAND;

            try
            {
                retval = Token.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        internal void SetToken(IProbeCommandToken token)
        {
            try
            {
                Token = token;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ClearToken()
        {
            try
            {
                if (Token.GetState() != CommandStateEnum.NOCOMMAND)
                {
                    LoggerManager.Debug($"ClearToken(): Token.Name:{Token} Target:{Token.Target}");
                }

                Token = new NoCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class CommandTokenSet
    {
        public CommandTokenSet() { }

        private HashSet<IProbeCommandToken> _TokenSet = new HashSet<IProbeCommandToken>();

        public void Add(IProbeCommandToken token)
        {
            try
            {
                _TokenSet.Add(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Remove(IProbeCommandToken token)
        {
            try
            {
                _TokenSet.Remove(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Update()
        {
            try
            {
                if (_TokenSet.Count > 0)
                {
                    List<IProbeCommandToken> tokens = _TokenSet.ToList();

                    foreach (var token in tokens)
                    {
                        bool consumed = token.CommandManager().SetCommand(token.Target, token.Parameter.Command);
                        if (consumed == true)
                        {
                            _TokenSet.Remove(token);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Clear()
        {
            _TokenSet.Clear();
        }
    }
}