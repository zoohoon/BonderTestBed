namespace LogModule.LoggerRule
{
    using LogModule.LoggerParam;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    public class InfoLoggerRuleConfig : NLoggerRuleConfiger
    {
        public override void Config(string loggerName, LoggingConfiguration config, NLoggerParam param)
        {
            //==> File rule setting
            LoggerFileTarget = new FileTarget("InfoLoggerFileTarget");
            LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${message}";
            LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\Info_${shortdate}.log";
            
            LoggerFileTarget.CreateDirs = true;
            LoggerFileTarget.KeepFileOpen = false;
            LoggerFileTarget.ConcurrentWrites = true;
            LoggerFileTarget.CleanupFileName = false;
            LoggerFileTarget.AutoFlush = false;
            LoggerFileTarget.OpenFileFlushTimeout = 5;

            //LoggerFileTarget.ArchiveFileName = "C:\\Logs\\Info\\Archives\\Info_${date:format=yyyy-MM-dd HHmmss}.log";
            //LoggerFileTarget.ArchiveNumbering = ArchiveNumberingMode.DateAndSequence;
            //LoggerFileTarget.ArchiveAboveSize = param.FileSizeLimit;

            //LoggerFileTarget.ArchiveDateFormat = "yyyy-MM-dd HHmmss";

            config.AddTarget("infoLogFile", LoggerFileTarget);
            LoggingRule debugLogFileRule = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Info, LoggerFileTarget);
            config.LoggingRules.Add(debugLogFileRule);

            //==> Console rule setting
            //ConsoleTarget debugLogConsoleTarget = new ConsoleTarget();
            //debugLogConsoleTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${message}";
            //config.AddTarget("InfoLogConsole", debugLogConsoleTarget);
            //LoggingRule debugLogConsoleRule = new LoggingRule(loggerName, LogLevel.Debug, debugLogConsoleTarget);
            //config.LoggingRules.Add(debugLogConsoleRule);
        }
    }

    public class LoaderMapLoggerRuleConfig : NLoggerRuleConfiger
    {
        public override void Config(string loggerName, LoggingConfiguration config, NLoggerParam param)
        {
            //==> File rule setting
            LoggerFileTarget = new FileTarget("LoaderMapLoggerFileTarget");
            LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${message}";
            LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\LoaderMap_${shortdate}.log";

            LoggerFileTarget.CreateDirs = true;
            LoggerFileTarget.KeepFileOpen = false;
            LoggerFileTarget.ConcurrentWrites = true;
            LoggerFileTarget.CleanupFileName = false;
            LoggerFileTarget.AutoFlush = false;
            LoggerFileTarget.OpenFileFlushTimeout = 5;


            config.AddTarget("LoaderMapLogFile", LoggerFileTarget);
            LoggingRule debugLogFileRule = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Info, LoggerFileTarget);
            config.LoggingRules.Add(debugLogFileRule);

        }
    }
}
