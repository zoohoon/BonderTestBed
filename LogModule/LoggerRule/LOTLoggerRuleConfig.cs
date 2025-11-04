namespace LogModule.LoggerRule
{
    using LogModule.LoggerParam;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    public class LOTLoggerRuleConfig : NLoggerRuleConfiger
    {
        public LOTLoggerRuleConfig()
        {
            index = -1;
        }
        public LOTLoggerRuleConfig(int idx)
        {
            index = idx;
        }
        int index;
        public override void Config(string loggerName, LoggingConfiguration config, NLoggerParam param)
        {
            //==> File rule setting
            LoggerFileTarget = new FileTarget("LOTFileTarget");
            LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${message}";

            if (index >= 0)
            {
                if (index == 0)
                {
                    LoggerFileTarget.FileName = $"{param.LogDirPath}"+ "\\LOADER_LOT_${shortdate}.log";
                }
                else
                {
                    LoggerFileTarget.FileName = $"{param.LogDirPath}" + $"\\C{index}"+"_LOT_${shortdate}.log";
                }
            }
            else
            {
                LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\LOT_${shortdate}.log";
            }
            LoggerFileTarget.CreateDirs = true;
            LoggerFileTarget.KeepFileOpen = false;
            LoggerFileTarget.ConcurrentWrites = true;
            LoggerFileTarget.CleanupFileName = false;
            LoggerFileTarget.AutoFlush = false;
            LoggerFileTarget.OpenFileFlushTimeout = 5;
            LoggerFileTarget.ArchiveFileName = "C:\\Logs\\Temp\\Archives\\LOT_${date:format=yyyy-MM-dd HHmmss}.log";
            LoggerFileTarget.ArchiveNumbering = ArchiveNumberingMode.DateAndSequence;
            LoggerFileTarget.ArchiveAboveSize = param.FileSizeLimit;        // In Bytes
            LoggerFileTarget.ArchiveDateFormat = "yyyy-MM-dd HHmmss";

            config.AddTarget("LOTLogFile", LoggerFileTarget);
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
