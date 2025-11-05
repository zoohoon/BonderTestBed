using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Command
{
    using Autofac;
    using ProberInterfaces;
    using System.IO;
    using System.Reflection;
    using System.Xml.Serialization;
    using System.Runtime.CompilerServices;
    using ProberInterfaces.Command;
    using ProberErrorCode;
    using LogModule;
    using Newtonsoft.Json;

    public class CommandManager : ICommandManager, IHasSysParameterizable, INotifyPropertyChanged
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        public static IContainer CommandContainer { get; set; }

        private static bool isConfiureDependecies = false;

        private CommandParam _CommandParam;
        public CommandParam CommandParam
        {
            get { return _CommandParam; }
            set
            {
                if (value != _CommandParam)
                {
                    _CommandParam = value;
                }
            }
        }

        private Dictionary<string, IProbeCommandParameter> _CommandLists;
        public Dictionary<string, IProbeCommandParameter> CommandLists
        {
            get { return _CommandLists; }
            set
            {
                if (value != _CommandLists)
                {
                    _CommandLists = value;
                }
            }
        }

        public IEnumerable<string> InternalCommandlist => CommandLists.Keys.ToList();

        public IContainer GetCommandContainer()
        {
            return CommandContainer;
        }

        public IContainer ConfigureDependencies(CommandParam param)
        {
            if (isConfiureDependecies == false)
            {
                var builder = new ContainerBuilder();

                //String strFolder = System.IO.Directory.GetCurrentDirectory();
                String strFolder = AppDomain.CurrentDomain.BaseDirectory;

                List<Assembly> allAssemblies = new List<Assembly>();

                // Load DLLs
                foreach (var name in param.CommandDLLNameList)
                {
                    string LoadDLLPath = Path.Combine(strFolder, name.AssemblyName);

                    try
                    {
                        Assembly ass = Assembly.LoadFrom(LoadDLLPath);

                        if (ass != null)
                        {
                            allAssemblies.Add(ass);

                            if (name.AssemblyName == "InternalCommands.dll")
                            {
                                foreach (var type in ass.GetTypes())
                                {
                                    var cmdInterfaceTypes = type.GetInterfaces().Where(x => typeof(IProbeCommand).IsAssignableFrom(x)).ToList();
                                    if (cmdInterfaceTypes.Count == 2)
                                    {
                                        var foundType = cmdInterfaceTypes.Where(item => item != typeof(IProbeCommand)).First();
                                        string cmdName = CommandNameGen.Generate(foundType);
                                    }
                                    else
                                    {
                                        //err
                                    }

                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        throw err;
                    }
                }

                foreach (var assembly in allAssemblies)
                {
                    builder.RegisterAssemblyModules(assembly);
                }

                CommandContainer = builder.Build();
                isConfiureDependecies = true;
            }

            return CommandContainer;
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public IGEMModule GEMModule { get; set; }
        public IGPIB GPIB { get; set; }
        public IInternal Internal { get; set; }

        public ITCPIP TCPIPModule { get; set; }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    isConfiureDependecies = false;
                    CommandLists = new Dictionary<string, IProbeCommandParameter>();

                    // Make CommandContainer
                    CommandContainer = ConfigureDependencies(CommandParam);

                    GEMModule = this.GEMModule();
                    Internal = this.Internal();

                    if (AppDomain.CurrentDomain.FriendlyName != "LoaderSystem.exe")
                    {
                        GPIB = this.GPIB();
                        TCPIPModule = this.TCPIPModule();
                    }

                    retval = LoadCommandList();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"LoadCommandList() Failed");
                    }

                    Initialized = true;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum LoadCommandList()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Type type = this.GetType();

                PropertyInfo[] propInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                IEnumerable<PropertyInfo> pList = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                List<IHasCommandRecipe> FacModules = new List<IHasCommandRecipe>();

                foreach (var item in pList)
                {
                    var value = item.GetValue(this, null);

                    if (value != null)
                    {
                        if (value.GetType().GetInterfaces().Contains(typeof(IHasCommandRecipe)))
                        {
                            FacModules.Add((IHasCommandRecipe)value);
                        }
                    }
                }

                foreach (var item in FacModules)
                {
                    retVal = item.SetCommandRecipe();
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);

                throw err;
            }

            return retVal;
        }

        public EventCodeEnum SaveCommandParamInfo()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            string FullPath = this.FileManager().GetSystemParamFullPath(CommandParam.FilePath, CommandParam.FileName);

            try
            {
                retVal = this.SaveParameter(CommandParam);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public IProbeCommandParameter GetCommandParameter(string cmdName)
        {
            IProbeCommandParameter pcp = null;

            try
            {
                CommandLists.TryGetValue(cmdName, out pcp);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return pcp;
        }

        public void AddCommandParameters(List<CommandDescriptor> list)
        {
            try
            {
                foreach (var desc in list)
                {
                    CommandLists.Add(desc.Command, desc.Parameter);
                }

                RaisePropertyChanged(nameof(InternalCommandlist));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ProbeCommandInfo MakeProbeInfo(IProbeCommand CurCommand)
        {
            ProbeCommandInfo CommandInfo = new ProbeCommandInfo();

            try
            {

                CommandInfo.Command = CurCommand;

                CommandInfo.StartTime = DateTime.Now;

                CommandInfo.HashCode = SecuritySystem.SecurityUtil.GetHashCode_SHA256((CommandInfo.StartTime.Ticks + CommandInfo.Command.GetType().FullName));

                CommandInfo.Description = "TEST";

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return CommandInfo;
        }

        public bool SetCommand(object sender, string commnadname, IProbeCommandParameter parameter = null)
        {
            bool isInjected = false;

            try
            {
                var cmd = CommandContainer.ResolveNamed<ProbeCommand>(commnadname);

                if (cmd == null)
                {
                    //==> Can't find Resolved Command
                    LoggerManager.Debug("[CommandManager SetCommand Error] [" + commnadname + "] Not Found");
                    return isInjected;
                }

                cmd.Parameter = parameter != null ? parameter : GetCommandParameter(commnadname);
                cmd.Sender = sender as IStateModule;
                
                isInjected = cmd.Execute();

                if (isInjected == false)
                {
                    LoggerManager.Debug("[CommandManager SetCommand Execute Error] [" + commnadname + "]");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"SetCommand(): Error occurred. Command name = {commnadname}, Sender = {sender}");
                LoggerManager.Exception(err);

                isInjected = false;
            }

            return isInjected;
        }

        public bool SetCommand<T>(object sender, IProbeCommandParameter parameter = null) where T : IProbeCommand
        {
            var cmdType = typeof(T);

            if (cmdType == typeof(NoCommand))
            {
                return true;
            }
            else
            {
                string commnadname = CommandNameGen.Generate(cmdType);

                return SetCommand(sender, commnadname, parameter);
            }
        }

        private bool IsCommandNone(string commandName)
        {
            bool retval = false;

            try
            {
                retval = string.IsNullOrEmpty(commandName) || CommandNameGen.Generate(typeof(NoCommand)) == commandName;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool ProcessIfRequested<TCommand>(IStateModule module, CommandCondition condition = null, CommandAction doAction = null, CommandAction abortAction = null) where TCommand : IProbeCommand
        {
            bool isProcessed = false;

            if (module.CommandRecvSlot.IsRequested<TCommand>())
            {
                try
                {
                    if (condition == null)
                    {
                        condition = CommandCondition.NONE;
                    }

                    if (doAction == null)
                    {
                        doAction = CommandAction.NONE;
                    }

                    if (condition.Execute())
                    {
                        module.CommandRecvProcSlot = module.CommandRecvSlot;
                        var runToken = module.CommandRecvProcSlot.Token;
                        module.CommandRecvSlot = new CommandSlot();

                        doAction.Execute();

                        module.CommandRecvProcSlot.Token.SetDone();
                        module.CommandRecvDoneSlot = module.CommandRecvProcSlot;
                        module.CommandRecvProcSlot = new CommandSlot();

                        if (runToken.Parameter != null && IsCommandNone(runToken.Parameter.Command) == false)
                        {
                            module.RunTokenSet.Add(runToken);
                        }

                        LoggerManager.Debug($"[{this.GetType().Name}], ProcessIfRequested(), [{typeof(TCommand).Name}] Done.");
                    }
                    else
                    {
                        abortAction.Execute();
                        module.CommandRecvSlot.Token.SetAbort();

                        LoggerManager.Debug($"[{this.GetType().Name}], ProcessIfRequested(), [{typeof(TCommand).Name}] Aborted.");
                    }

                    isProcessed = true;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            else
            {
                isProcessed = false;
            }

            return isProcessed;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new CommandParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(CommandParam));

                if (retVal == EventCodeEnum.NONE)
                {
                    CommandParam = tmpParam as CommandParam;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(CommandParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CommandParam : ISystemParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }



        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    retval = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"[CommandParam] [Method = Init] [Error = {err}]");
                    retval = EventCodeEnum.PARAM_ERROR;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }
        public List<AssemblyInfo> CommandDLLNameList { get; set; }
        public string FilePath { get; } = "Command";

        public string FileName { get; } = "CommandDLLName.Json";
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (CommandDLLNameList == null)
                {
                    CommandDLLNameList = new List<AssemblyInfo>();
                }

                CommandDLLNameList.Add(
                    AddCommandDLL("GPIBCommands.dll", 1000)
                    );

                CommandDLLNameList.Add(
                    AddCommandDLL("InternalCommands.dll", 1000)
                    );

                CommandDLLNameList.Add(
                    AddCommandDLL("CUICommands.dll", 1000)
                    );
                CommandDLLNameList.Add(
                    AddCommandDLL("TCPIPCommands.dll", 1000)
                    );

                //if (CommandParamList == null)
                //{
                //    CommandParamList = new List<string>();
                //}

                //CommandParamList.Add("CommandParam.GPIB.xml");
                //CommandParamList.Add("CommandParam.INTERNAL.xml");

                //if (CommandRecipeList == null)
                //{
                //    CommandRecipeList = new List<string>();
                //}

                //CommandRecipeList.Add("CommandRecipe.GPIB.xml");

                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;

            }
            return retVal;
        }
        public void SetElementMetaData()
        {

        }
        public AssemblyInfo AddCommandDLL(string assemblyname, int version)
        {
            AssemblyInfo Info = new ProberInterfaces.AssemblyInfo();
            try
            {

                Info.AssemblyName = assemblyname;
                Info.Version = version;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Info;
        }
    }
}