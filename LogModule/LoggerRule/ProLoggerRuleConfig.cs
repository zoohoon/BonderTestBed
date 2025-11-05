namespace LogModule.LoggerRule
{
    using LogModule.LoggerParam;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    public class ProLoggerRuleConfig : NLoggerRuleConfiger
    {
        //public MemoryEventTarget LoggerMemoryTarget { get; set; }

        public override void Config(string loggerName, LoggingConfiguration config, NLoggerParam param)
        {
            //==> Create Log Target
            LoggerFileTarget = new FileTarget();
            //LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | [P] | ${level} | ${message}";
            LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | [PL] | ${message}";
            //LoggerFileTarget.FileName = "C:\\Logs\\ProLog\\ProLog ${shortdate}.log";
            //LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\ProLog ${shortdate}.log";
            LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\ProLog_${shortdate}.log";


            LoggerFileTarget.CreateDirs = true;
            //==> Create Buffer Wrapper
            //BufferingTargetWrapper proLogFileBuffer = new BufferingTargetWrapper();
            //proLogFileBuffer.BufferSize = 100;
            //proLogFileBuffer.FlushTimeout = 500;
            //proLogFileBuffer.SlidingTimeout = true;
            //proLogFileBuffer.WrappedTarget = LoggerFileTarget;
            //==> Add Wrapping Target to config[Target Name : Target Object]

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

            config.AddTarget("ProoLogFile", asyncTarget);
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
