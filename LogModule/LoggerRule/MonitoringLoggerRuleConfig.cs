using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogModule.LoggerRule
{
    using LogModule;
    using LogModule.LoggerParam;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    public class MonitoringLoggerRuleConfig : NLoggerRuleConfiger
    {
        public override void Config(string loggerName, LoggingConfiguration config, NLoggerParam param)
        {
            try
            {
                //==> File rule setting
                LoggerFileTarget = new FileTarget("MonitoringLoggerFileTarget");
                LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${message}";
                LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\MonitoringLog_${shortdate}.log";

                LoggerFileTarget.CreateDirs = true;
                LoggerFileTarget.KeepFileOpen = false;
                LoggerFileTarget.ConcurrentWrites = true;
                LoggerFileTarget.CleanupFileName = false;
                LoggerFileTarget.AutoFlush = false;
                LoggerFileTarget.OpenFileFlushTimeout = 5;

                LoggerFileTarget.ArchiveFileName = $"{param.LogDirPath}" + "\\MonitoringLog_${shortdate}.log";
                LoggerFileTarget.ArchiveNumbering = ArchiveNumberingMode.Rolling;
                LoggerFileTarget.ArchiveAboveSize = param.FileSizeLimit;        // In Bytes
                LoggerFileTarget.ArchiveDateFormat = "yyyy-MM-dd HHmmss";
                config.AddTarget("MonitoringLogFile", LoggerFileTarget);
                LoggingRule MonitoringLogFileRule = new LoggingRule(loggerName, LogLevel.Debug, LogLevel.Info, LoggerFileTarget);
                config.LoggingRules.Add(MonitoringLogFileRule);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
