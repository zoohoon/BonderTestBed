using System;

namespace LogModule.LoggerController
{
    using LogModule.LoggerParam;
    using System.IO;

    public abstract class LoggerController
    {
        protected String _LogFilePrefix;
        protected String _LogFileExtension;
        protected String _Date;
        public LoggerParameter LoggerParam { get; set; }
        public virtual String CurFileTargetPath { get; set; }


        public LoggerController(LoggerParameter loggerParam)
        {
            if (loggerParam == null)
            {
                return;
            }

            LoggerParam = loggerParam;
        }

        public void UpdateCurrentFileTargetPath()
        {
            if (LoggerParam == null)
            {
                return;
            }

            String logDirectoryPath = BuildLogDirectoryPath();
            String logFileName = BuildLogFileName();

            CurFileTargetPath = Path.Combine(logDirectoryPath, logFileName);
        }
        public String BuildLogDirectoryPath()
        {
            if (LoggerParam == null)
            {
                return null;
            }

            String currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            String logDirectoryPath = Path.Combine(LoggerParam.LogDirPath, currentDate);

            do
            {
                if (currentDate == _Date)
                {
                    break;
                }

                if (Directory.Exists(logDirectoryPath) == false)
                {
                    Directory.CreateDirectory(logDirectoryPath);
                }

            } while (false);

            return logDirectoryPath;
        }

        public abstract String BuildLogFileName();
    }
}