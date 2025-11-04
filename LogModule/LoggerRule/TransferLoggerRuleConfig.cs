using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogModule.LoggerRule
{
    using LogModule.LoggerParam;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    public class TransferLoggerRuleConfig : NLoggerRuleConfiger
    {

        public override void Config(string loggerName, LoggingConfiguration config, NLoggerParam param)
        {
            //==> File rule setting
            LoggerFileTarget = new FileTarget("TransferFileTarget");
            LoggerFileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} | ${message}";
            LoggerFileTarget.FileName = $"{param.LogDirPath}" + "\\Transfer_${shortdate}.log";
            LoggerFileTarget.CreateDirs = true;
            LoggerFileTarget.KeepFileOpen = false;
            LoggerFileTarget.ConcurrentWrites = true;
            LoggerFileTarget.CleanupFileName = false;
            LoggerFileTarget.AutoFlush = false;
            LoggerFileTarget.OpenFileFlushTimeout = 5;
            LoggerFileTarget.ArchiveFileName = "C:\\Logs\\Temp\\Archives\\Transfer_${date:format=yyyy-MM-dd HHmmss}.log";
            LoggerFileTarget.ArchiveNumbering = ArchiveNumberingMode.DateAndSequence;
            LoggerFileTarget.ArchiveAboveSize = param.FileSizeLimit;        // In Bytes
            LoggerFileTarget.ArchiveDateFormat = "yyyy-MM-dd HHmmss";

            config.AddTarget("TransferLogFile", LoggerFileTarget);
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
