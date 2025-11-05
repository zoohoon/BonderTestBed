namespace LogModule.LoggerRule
{
    using LogModule.LoggerParam;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    public class DebugLoggerRuleConfig : NLoggerRuleConfiger
    {
        //public MemoryEventTarget LoggerMemoryTarget { get; set; }

        public override void Config(string loggerName, LoggingConfiguration config, NLoggerParam param)
        {
            //==> File rule setting
            LoggerFileTarget = new FileTarget("DebugLoggerFileTarget");
            //LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | [D] | ${message}";
            //LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${event-properties:item=LogIdentifier} | ${message}";
            LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${message}";
            LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\Debug_${shortdate}.log";
            
            LoggerFileTarget.CreateDirs = true;
            LoggerFileTarget.KeepFileOpen = false;
            LoggerFileTarget.ConcurrentWrites = true;
            LoggerFileTarget.CleanupFileName = false;
            LoggerFileTarget.AutoFlush = false;
            LoggerFileTarget.OpenFileFlushTimeout = 5;

            LoggerFileTarget.ArchiveFileName = "C:\\Logs\\Debug\\Archives\\Debug_${date:format=yyyy-MM-dd HHmmss}.log";
            LoggerFileTarget.ArchiveNumbering = ArchiveNumberingMode.DateAndSequence;
            //LoggerFileTarget.ArchiveAboveSize = 102400000;        // In Bytes
            LoggerFileTarget.ArchiveAboveSize = param.FileSizeLimit;        // In Bytes

            LoggerFileTarget.ArchiveDateFormat = "yyyy-MM-dd HHmmss";
            //archiveFileName = "${basedir}/archives/log.{#####}.txt"
            //archiveAboveSize = "10240"
            //archiveNumbering = "Sequence"

            //LoggerFileTarget.OpenFileFlushTimeout = 10;
            //==> Log를 찍을때 Buffer로 찍게 되면 성능상 파일 접근을 덜 하겠지만, 프로그램이 종료(정상이든 비정상이든)되었을 떄 
            //==> Buffer된 Log를 파일에 기록하지 못한다.

            ////==> Async Buffer는 제대로 동작 않함
            //AsyncTargetWrapper asyncTarget = new AsyncTargetWrapper();
            //asyncTarget.WrappedTarget = LoggerFileTarget;
            //asyncTarget.OverflowAction = AsyncTargetWrapperOverflowAction.Block;
            //asyncTarget.TimeToSleepBetweenBatches = 0;
            //asyncTarget.OptimizeBufferReuse = true;

            //BufferingTargetWrapper bufferingTarget = new BufferingTargetWrapper();
            //bufferingTarget.BufferSize = 100;//==> Logging Event Count
            //bufferingTarget.FlushTimeout = 1000;//==> 최소 ms 단위, -1이면 사용 않함.
            //bufferingTarget.SlidingTimeout = true;//==> true : Logging 함수 호출시 FlushTimeout 초기화 됨
            //bufferingTarget.OverflowAction = BufferingTargetWrapperOverflowAction.Flush;
            //bufferingTarget.WrappedTarget = LoggerFileTarget;

            config.AddTarget("debugLogFile", LoggerFileTarget);
            LoggingRule debugLogFileRule = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Info, LoggerFileTarget);
            config.LoggingRules.Add(debugLogFileRule);

            //==> Console rule setting
            ConsoleTarget debugLogConsoleTarget = new ConsoleTarget();
            //debugLogConsoleTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | [D] | ${message}";
            debugLogConsoleTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${message}";
            config.AddTarget("DebugLogConsole", debugLogConsoleTarget);
            LoggingRule debugLogConsoleRule = new LoggingRule(loggerName, LogLevel.Debug, debugLogConsoleTarget);
            config.LoggingRules.Add(debugLogConsoleRule);

            ////==> Create Log Target
            //LoggerMemoryTarget = new MemoryEventTarget();

            //AsyncTargetWrapper memoryAsyncWrapper = new AsyncTargetWrapper();
            //memoryAsyncWrapper.WrappedTarget = LoggerMemoryTarget;
            ////memoryAsyncWrapper.QueueLimit = 32767;
            //memoryAsyncWrapper.OverflowAction = AsyncTargetWrapperOverflowAction.Grow;
            //memoryAsyncWrapper.TimeToSleepBetweenBatches = 0;
            //memoryAsyncWrapper.BatchSize = 1000;

            //config.AddTarget("DebugMemoryEvent", memoryAsyncWrapper);
            //LoggingRule proLogFileRule2 = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Error, memoryAsyncWrapper);
            //config.LoggingRules.Add(proLogFileRule2);

            //BufferingTargetWrapper memoryBufferingTargetWrapper = new BufferingTargetWrapper();
            //memoryBufferingTargetWrapper.BufferSize = 1000;
            //memoryBufferingTargetWrapper.FlushTimeout = 1000;
            //memoryBufferingTargetWrapper.SlidingTimeout = true;
            //memoryBufferingTargetWrapper.WrappedTarget = LoggerMemoryTarget;

            //config.AddTarget("MemoryEvent", memoryBufferingTargetWrapper);
            //LoggingRule proLogFileRule2 = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Error, memoryBufferingTargetWrapper);
            //config.LoggingRules.Add(proLogFileRule2);
        }
        //public override void Config(string loghostname, string loggerName, LoggingConfiguration config, NLoggerParam param)
        //{
        //    //==> File rule setting
        //    LoggerFileTarget = new FileTarget("DebugLoggerFileTarget");
        //    LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${message}";
        //    LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\Debug ${shortdate}.log";

        //    LoggerFileTarget.CreateDirs = true;
        //    LoggerFileTarget.KeepFileOpen = true;
        //    LoggerFileTarget.ConcurrentWrites = true;
        //    LoggerFileTarget.CleanupFileName = false;
        //    LoggerFileTarget.AutoFlush = false;
        //    LoggerFileTarget.OpenFileFlushTimeout = 5;

        //    LoggerFileTarget.ArchiveFileName = "C:\\Logs\\Debug\\Archives\\Debug ${date:format=yyyy-MM-dd HHmmss}.log";
        //    LoggerFileTarget.ArchiveNumbering = ArchiveNumberingMode.DateAndSequence;
        //    LoggerFileTarget.ArchiveAboveSize = param.FileSizeLimit;        // In Bytes

        //    LoggerFileTarget.ArchiveDateFormat = "yyyy-MM-dd HHmmss";

        //    config.AddTarget("debugLogFile", LoggerFileTarget);
        //    LoggingRule debugLogFileRule = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Info, LoggerFileTarget);
        //    config.LoggingRules.Add(debugLogFileRule);

        //    //==> Console rule setting
        //    ConsoleTarget debugLogConsoleTarget = new ConsoleTarget();
        //    debugLogConsoleTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${message}";
        //    config.AddTarget("DebugLogConsole", debugLogConsoleTarget);
        //    LoggingRule debugLogConsoleRule = new LoggingRule(loggerName, LogLevel.Debug, debugLogConsoleTarget);
        //    config.LoggingRules.Add(debugLogConsoleRule);
        //}
    }
}
