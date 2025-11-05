namespace LogModule.LoggerRule
{
    using LogModule.LoggerParam;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    public class ParamLoggerRuleConfig : NLoggerRuleConfiger
    {
        public ParamLoggerRuleConfig()
        {
            index = -1;
        }
        public ParamLoggerRuleConfig(int idx)
        {
            index = idx;
        }
        int index;
        public override void Config(string loggerName, LoggingConfiguration config, NLoggerParam param)
        {
            //==> File rule setting
            LoggerFileTarget = new FileTarget("ParamFileTarget");
            LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${message}";

            if (index >= 0)
            {
                if (index == 0)
                {
                    LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\LOADER_PARAM_${shortdate}.log";
                }
                else
                {
                    LoggerFileTarget.FileName = $"{param.LogDirPath}" + $"\\C{index}" + "_PARAM_${shortdate}.log";
                }
            }
            else
            {
                LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\PARAM_${shortdate}.log";
            }
            LoggerFileTarget.CreateDirs = true;
            LoggerFileTarget.KeepFileOpen = false;
            LoggerFileTarget.ConcurrentWrites = true;
            LoggerFileTarget.CleanupFileName = false;
            LoggerFileTarget.AutoFlush = false;
            LoggerFileTarget.OpenFileFlushTimeout = 5;
            LoggerFileTarget.ArchiveFileName = "C:\\Logs\\Temp\\Archives\\ParamHistory_${date:format=yyyy-MM-dd HHmmss}.log";
            LoggerFileTarget.ArchiveNumbering = ArchiveNumberingMode.DateAndSequence;
            LoggerFileTarget.ArchiveAboveSize = param.FileSizeLimit;        // In Bytes
            LoggerFileTarget.ArchiveDateFormat = "yyyy-MM-dd HHmmss";

            config.AddTarget("ParamLogFile", LoggerFileTarget);
            LoggingRule TempLogFileRule = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Info, LoggerFileTarget);
            config.LoggingRules.Add(TempLogFileRule);

            ////==> Console rule setting
            //ConsoleTarget TempLogConsoleTarget = new ConsoleTarget();
            ////debugLogConsoleTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | [D] | ${message}";
            //TempLogConsoleTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${message}";
            //config.AddTarget("TempLogConsole", TempLogConsoleTarget);
            //LoggingRule TempLogConsoleRule = new LoggingRule(loggerName, LogLevel.Debug, TempLogConsoleTarget);
            //config.LoggingRules.Add(TempLogConsoleRule);

        }

    }
}
