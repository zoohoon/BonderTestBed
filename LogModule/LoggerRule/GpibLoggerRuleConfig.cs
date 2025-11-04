namespace LogModule.LoggerRule
{
    using LogModule.LoggerParam;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    public class GpibLoggerRuleConfig : NLoggerRuleConfiger
    {
        //public MemoryEventTarget LoggerMemoryTarget { get; set; }
        public override void Config(string loggerName, LoggingConfiguration config, NLoggerParam param)
        {
            //==> File rule setting
            LoggerFileTarget = new FileTarget();
            //LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | [D] | ${message}";
            LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | [GP] | ${message}";
            //LoggerFileTarget.FileName = "C:\\Logs\\GPIB\\GPIB ${shortdate}.log";
            LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\GPIB_${shortdate}.log";

            LoggerFileTarget.CreateDirs = true;

            BufferingTargetWrapper debugLogFileBuffer = new BufferingTargetWrapper();
            debugLogFileBuffer.BufferSize = 100;
            debugLogFileBuffer.FlushTimeout = 500;
            debugLogFileBuffer.OverflowAction = BufferingTargetWrapperOverflowAction.Flush;
            debugLogFileBuffer.SlidingTimeout = true;
            debugLogFileBuffer.WrappedTarget = LoggerFileTarget;

            //==> Log를 찍을때 Buffer로 찍게 되면 성능상 파일 접근을 덜 하겠지만, 프로그램이 종료(정상이든 비정상이든)되었을 떄 
            //==> Buffer된 Log를 파일에 기록하지 못한다.

            //////==> Async Buffer는 제대로 동작 않함
            //AsyncTargetWrapper asyncTarget = new AsyncTargetWrapper();
            //asyncTarget.WrappedTarget = LoggerFileTarget;
            ////asyncTarget.QueueLimit = 32767;
            //asyncTarget.OverflowAction = AsyncTargetWrapperOverflowAction.Grow;
            //asyncTarget.TimeToSleepBetweenBatches = 0;
            //asyncTarget.BatchSize = 1000;

            //config.AddTarget("async", asyncTarget);
            //LoggingRule asyncDebugLogFileRule = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Error, asyncTarget);
            //config.LoggingRules.Add(asyncDebugLogFileRule);

            //BufferingTargetWrapper bufferingTarget = new BufferingTargetWrapper();
            //bufferingTarget.BufferSize = 1000;//==> Logging Event Count
            //bufferingTarget.FlushTimeout = 1000;//==> 최소 초 단위, -1이면 사용 않함.
            //bufferingTarget.SlidingTimeout = true;//==> true : Logging 함수 호출시 FlushTimeout 초기화 됨
            //bufferingTarget.WrappedTarget = LoggerFileTarget;

            config.AddTarget("GPIBLogFile", debugLogFileBuffer);
            LoggingRule debugLogFileRule = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Info, debugLogFileBuffer);
            config.LoggingRules.Add(debugLogFileRule);

            //==> Console rule setting
            ConsoleTarget debugLogConsoleTarget = new ConsoleTarget();
            //debugLogConsoleTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | [D] | ${message}";
            debugLogConsoleTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | [GP] | ${message}";
            config.AddTarget("GPIBLogConsole", debugLogConsoleTarget);
            LoggingRule debugLogConsoleRule = new LoggingRule(loggerName, LogLevel.Debug, debugLogConsoleTarget);
            config.LoggingRules.Add(debugLogConsoleRule);

            ////==> Create Log Target
            //LoggerMemoryTarget = new MemoryEventTarget();

            ////AsyncTargetWrapper memoryAsyncWrapper = new AsyncTargetWrapper();
            ////memoryAsyncWrapper.WrappedTarget = LoggerMemoryTarget;
            //////memoryAsyncWrapper.QueueLimit = 32767;
            ////memoryAsyncWrapper.OverflowAction = AsyncTargetWrapperOverflowAction.Grow;
            ////memoryAsyncWrapper.TimeToSleepBetweenBatches = 0;
            ////memoryAsyncWrapper.BatchSize = 1000;

            ////config.AddTarget("MemoryEvent", memoryAsyncWrapper);
            ////LoggingRule proLogFileRule2 = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Error, memoryAsyncWrapper);
            ////config.LoggingRules.Add(proLogFileRule2);

            //BufferingTargetWrapper memoryBufferingTargetWrapper = new BufferingTargetWrapper();
            //memoryBufferingTargetWrapper.BufferSize = 1000;
            //memoryBufferingTargetWrapper.FlushTimeout = 1000;
            //memoryBufferingTargetWrapper.SlidingTimeout = true;
            //memoryBufferingTargetWrapper.WrappedTarget = LoggerMemoryTarget;

            //config.AddTarget("MemoryEvent", memoryBufferingTargetWrapper);
            //LoggingRule proLogFileRule2 = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Error, memoryBufferingTargetWrapper);
            //config.LoggingRules.Add(proLogFileRule2);
        }
    }
}
