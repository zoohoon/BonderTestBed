using LogModule;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ProberErrorCode;
using Newtonsoft.Json;

namespace ProberInterfaces.Command
{
    public interface IHasCommandRecipe
    {
        EventCodeEnum SetCommandRecipe();
    }

    public interface ICommandRecipe
    {
        List<CommandDescriptor> Descriptors { get; }
    }

    [Serializable]
    public class CommandDescriptor
    {
        public CommandDescriptor()
        {
            try
            {
                Command = CommandNameGen.Generate(typeof(NoCommand));
                Parameter = new NoCommandParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public CommandDescriptor(string command, ProbeCommandParameter parameter)
        {
            try
            {
                Command = command;
                Parameter = parameter;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string Command { get; set; }

        public ProbeCommandParameter Parameter { get; set; }

        public static CommandDescriptor Create<ICommand>()
            where ICommand : IProbeCommand
        {
            return new CommandDescriptor(CommandNameGen.Generate(typeof(ICommand)), new NoCommandParam());
        }

        public static CommandDescriptor Create(string command)
        {
            return new CommandDescriptor(command, new NoCommandParam());
        }
    }

    [Serializable]
    public abstract class CommandRecipe : ICommandRecipe, ISystemParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore]
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


        public abstract EventCodeEnum Init();

        private List<CommandDescriptor> _Descriptors = new List<CommandDescriptor>();
        public List<CommandDescriptor> Descriptors
        {
            get { return _Descriptors; }
            set { _Descriptors = value; }
        }
        [XmlIgnore, JsonIgnore]
        public abstract string FilePath { get; }

        [XmlIgnore, JsonIgnore]
        public abstract string FileName { get; }


        public abstract EventCodeEnum SetDefaultParam();

        public abstract EventCodeEnum SetEmulParam();

        public EventCodeEnum FindCallbackCommand(string command, out string callbackCommand)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            callbackCommand = string.Empty;

            try
            {
                var cmdDesc = this.Descriptors.Find(x => x.Command == command);

                if (cmdDesc != null)
                {
                    callbackCommand = cmdDesc.Parameter.Command;
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    callbackCommand = string.Empty;
                    retVal = EventCodeEnum.NODATA;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public void SetElementMetaData()
        {

        }
        public void AddRecipe(CommandDescriptor desc)
        {
            Descriptors.Add(desc);
        }
    }
}
