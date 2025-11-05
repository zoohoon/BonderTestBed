namespace LogModule.LoggerRule
{
    using LogModule.LoggerParam;
    using NLog.Config;
    using NLog.Targets;

    public abstract class NLoggerRuleConfiger
    {

        public FileTarget LoggerFileTarget { get; set; }
        public abstract void Config(string loggerName, LoggingConfiguration config, NLoggerParam param);
        //public abstract void Config(string loghostname, string loggerName, LoggingConfiguration config, NLoggerParam param);
    }
}
