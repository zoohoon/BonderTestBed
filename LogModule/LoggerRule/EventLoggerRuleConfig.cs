namespace LogModule.LoggerRule
{
    using LogModule.LoggerParam;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    public class EventLoggerRuleConfig : NLoggerRuleConfiger
    {
        //public MemoryEventTarget LoggerMemoryTarget { get; set; }

        public override void Config(string loggerName, LoggingConfiguration config, NLoggerParam param)
        {
            //==> Create Log Target
            LoggerFileTarget = new FileTarget();
            //LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${event-properties:item=LogIdentifier} | ${level} | ${message}";
            LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | [EV] | ${message}";
            //LoggerFileTarget.FileName = "C:\\Logs\\Event\\Event ${shortdate}.log";
            LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\Event_${shortdate}.log";

            LoggerFileTarget.CreateDirs = true;

            AsyncTargetWrapper asyncTarget = new AsyncTargetWrapper();
            asyncTarget.WrappedTarget = LoggerFileTarget;
            //asyncTarget.QueueLimit = 32767;
            asyncTarget.OverflowAction = AsyncTargetWrapperOverflowAction.Grow;
            asyncTarget.TimeToSleepBetweenBatches = 0;
            asyncTarget.BatchSize = 1000;

            //BufferingTargetWrapper bufferingTarget = new BufferingTargetWrapper();
            //bufferingTarget.BufferSize = 1000;//==> Logging Event Count
            ////bufferingTarget.FlushTimeout = 5000;//==> 최소 초 단위, -1이면 사용 않함.
            //bufferingTarget.FlushTimeout = 1000;//==> 최소 초 단위, -1이면 사용 않함.
            ////bufferingTarget.SlidingTimeout = false;//==> true : Logging 함수 호출시 FlushTimeout 초기화 됨
            //bufferingTarget.SlidingTimeout = true;//==> true : Logging 함수 호출시 FlushTimeout 초기화 됨
            //bufferingTarget.WrappedTarget = LoggerFileTarget;

            config.AddTarget("EventLogFile", asyncTarget);
            LoggingRule proLogFileRule = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Error, asyncTarget);
            config.LoggingRules.Add(proLogFileRule);

            //==> Create Log Target
            //LoggerMemoryTarget = new MemoryEventTarget();

            //AsyncTargetWrapper memoryAsyncWrapper = new AsyncTargetWrapper();
            //memoryAsyncWrapper.WrappedTarget = LoggerMemoryTarget;
            ////memoryAsyncWrapper.QueueLimit = 32767;
            //memoryAsyncWrapper.OverflowAction = AsyncTargetWrapperOverflowAction.Grow;
            //memoryAsyncWrapper.TimeToSleepBetweenBatches = 0;
            //memoryAsyncWrapper.BatchSize = 1000;

            //config.AddTarget("MemoryEvent", memoryAsyncWrapper);
            //LoggingRule proLogFileRule2 = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Error, memoryAsyncWrapper);
            //config.LoggingRules.Add(proLogFileRule2);

            //AsyncTargetWrapper memoryAsyncWrapper = new AsyncTargetWrapper();
            //memoryAsyncWrapper.WrappedTarget = LoggerMemoryTarget;
            ////memoryAsyncWrapper.QueueLimit = 32767;
            //memoryAsyncWrapper.OverflowAction = AsyncTargetWrapperOverflowAction.Grow;
            //memoryAsyncWrapper.TimeToSleepBetweenBatches = 0;
            //memoryAsyncWrapper.BatchSize = 1000;

            //config.AddTarget("EventMemoryEvent", memoryAsyncWrapper);
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
    }
}


