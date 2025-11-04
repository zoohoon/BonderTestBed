using System;

namespace ProberInterfaces.Command.Cui
{
    public class TestCommand1Param : ProbeCommandParameter
    {
        public String Key { get; set; } = "Param1";

        public bool ParseLogCmdArgument(string log)
        {
            return true;
        }
        public string WriteLogCmdArgument()
        {
            return String.Empty;
        }
    }

    public class TestCommand2Param : ProbeCommandParameter
    {
        public String Key { get; set; } = "Param2";
        public bool ParseLogCmdArgument(string log)
        {
            return true;
        }
        public string WriteLogCmdArgument()
        {
            return String.Empty;
        }
    }

    public class TestCommand3Param : ProbeCommandParameter
    {
        public String Key { get; set; } = "Param3";
        public bool ParseLogCmdArgument(string log)
        {
            return true;
        }
        public string WriteLogCmdArgument()
        {
            return String.Empty;
        }
    }

    public interface ITestCommand1 : IProbeCommand
    {
    }
    public interface ITestCommand2 : IProbeCommand
    {
    }
    public interface ITestCommand3 : IProbeCommand
    {
    }
}
