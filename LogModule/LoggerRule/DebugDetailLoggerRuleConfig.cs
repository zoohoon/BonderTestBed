namespace LogModule.LoggerRule
{
    using LogModule.LoggerParam;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    public class DebugDetailLoggerRuleConfig : NLoggerRuleConfiger
    {
        public override void Config(string loggerName, LoggingConfiguration config, NLoggerParam param)
        {
            //==> File rule setting
            LoggerFileTarget = new FileTarget();
            LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${callsite:skipFrames=3} | Line : ${callsite-linenumber:skipFrames=3} | ${message} ${exception:format=tostring} ${newline}${stacktrace:format=DetailedFlat:separator=\r\n:topFrames=6:skipFrames=3}";
            //LoggerFileTarget.FileName = "C:\\Logs\\Exception\\Exception ${shortdate}.log";
            LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\Exception_${shortdate}.log";

            LoggerFileTarget.CreateDirs = true;
            BufferingTargetWrapper debugLogFileBuffer = new BufferingTargetWrapper();
            debugLogFileBuffer.BufferSize = 100;
            debugLogFileBuffer.FlushTimeout = 500;
            debugLogFileBuffer.OverflowAction = BufferingTargetWrapperOverflowAction.Flush;
            debugLogFileBuffer.SlidingTimeout = true;
            debugLogFileBuffer.WrappedTarget = LoggerFileTarget;

            //AsyncTargetWrapper asyncTarget = new AsyncTargetWrapper();
            //asyncTarget.WrappedTarget = LoggerFileTarget;
            ////asyncTarget.QueueLimit = 32767;
            //asyncTarget.OverflowAction = AsyncTargetWrapperOverflowAction.Grow;
            //asyncTarget.TimeToSleepBetweenBatches = 0;
            //asyncTarget.BatchSize = 1000;

            //BufferingTargetWrapper bufferingTarget = new BufferingTargetWrapper();
            //bufferingTarget.BufferSize = 1000;//==> Logging Event Count
            ////bufferingTarget.FlushTimeout = 5000;//==> 최소 초 단위, -1이면 사용 않함.
            //bufferingTarget.FlushTimeout = 1000;//==> 최소 초 단위, -1이면 사용 않함.
            ////bufferingTarget.SlidingTimeout = false;//==> true : Logging 함수 호출시 FlushTimeout 초기화 됨
            //bufferingTarget.SlidingTimeout = true;//==> true : Logging 함수 호출시 FlushTimeout 초기화 됨
            //bufferingTarget.WrappedTarget = LoggerFileTarget;

            config.AddTarget("ExceptionLogFile", debugLogFileBuffer);
            LoggingRule debugLogFileRule = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Info, debugLogFileBuffer);
            config.LoggingRules.Add(debugLogFileRule);
            
            ////==> Console rule setting
            //ConsoleTarget debugLogConsoleTarget = new ConsoleTarget();
            //debugLogConsoleTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | [D] | ${callsite:skipFrames=3} | Line : ${callsite-linenumber:skipFrames=3} | ${message}";
            //config.AddTarget("debugLogConsole", debugLogConsoleTarget);
            //LoggingRule debugLogConsoleRule = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Info, debugLogConsoleTarget);
            //config.LoggingRules.Add(debugLogConsoleRule);
        }
    }
}
