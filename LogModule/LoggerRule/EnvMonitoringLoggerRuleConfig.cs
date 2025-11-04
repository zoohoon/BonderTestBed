using LogModule.LoggerParam;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogModule.LoggerRule
{
    public class EnvMonitoringLoggerRuleConfig : NLoggerRuleConfiger
    {
        public override void Config(string loggerName, LoggingConfiguration config, NLoggerParam param)
        {
            //==> File rule setting
            LoggerFileTarget = new FileTarget("EnvMonitoringLoggerFileTarget");
            LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${message}";
            LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\EnvMonitoring_${shortdate}.log";

            LoggerFileTarget.CreateDirs = true;
            LoggerFileTarget.KeepFileOpen = false;
            LoggerFileTarget.ConcurrentWrites = true;
            LoggerFileTarget.CleanupFileName = false;
            LoggerFileTarget.AutoFlush = false;
            LoggerFileTarget.OpenFileFlushTimeout = 5;

            LoggerFileTarget.ArchiveFileName = "C:\\Logs\\EnvMonitoring\\Archives\\EnvMonitoring_${date:format=yyyy-MM-dd HHmmss}.log";
            LoggerFileTarget.ArchiveNumbering = ArchiveNumberingMode.DateAndSequence;
            LoggerFileTarget.ArchiveAboveSize = param.FileSizeLimit;        // In Bytes
            LoggerFileTarget.ArchiveDateFormat = "yyyy-MM-dd HHmmss";
            config.AddTarget("SmokeSensorLogFile", LoggerFileTarget);
            LoggingRule SoakingLogFileRule = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Info, LoggerFileTarget);
            config.LoggingRules.Add(SoakingLogFileRule);
        }
    }
}
