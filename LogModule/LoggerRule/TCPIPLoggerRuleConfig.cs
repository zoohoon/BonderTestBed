namespace LogModule.LoggerRule
{
    using LogModule.LoggerParam;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    public class TCPIPLoggerRuleConfig : NLoggerRuleConfiger
    {
        //public MemoryEventTarget LoggerMemoryTarget { get; set; }

        public override void Config(string loggerName, LoggingConfiguration config, NLoggerParam param)
        {
            //==> File rule setting
            LoggerFileTarget = new FileTarget("TCPIPLoggerFileTarget");
            LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${message}";
            LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\TCPIP_${shortdate}.log";
            LoggerFileTarget.CreateDirs = true;
            LoggerFileTarget.KeepFileOpen = false;
            LoggerFileTarget.ConcurrentWrites = true;
            LoggerFileTarget.CleanupFileName = false;
            LoggerFileTarget.AutoFlush = false;
            LoggerFileTarget.OpenFileFlushTimeout = 5;
            LoggerFileTarget.ArchiveFileName = "C:\\Logs\\TCPIP\\Archives\\TCPIP_${date:format=yyyy-MM-dd HHmmss}.log";
            LoggerFileTarget.ArchiveNumbering = ArchiveNumberingMode.DateAndSequence;
            LoggerFileTarget.ArchiveAboveSize = param.FileSizeLimit;        // In Bytes
            LoggerFileTarget.ArchiveDateFormat = "yyyy-MM-dd HHmmss";

            config.AddTarget("TCPIPLogFile", LoggerFileTarget);
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
