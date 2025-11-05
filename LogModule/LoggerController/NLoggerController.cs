using System;
using System.Threading.Tasks;

namespace LogModule.LoggerController
{
    using LogModule.LoggerParam;
    using LogModule.LoggerRule;
    using NLog;

    public class NLoggerController : LoggerController
    {
        private Logger _Logger;
        private NLoggerRuleConfiger _NLoggerRuleConfiger;
        //private Task _UploadTimerTask;
        //private bool _UploadTimerRun;

        private NLoggerParam _NLoggerParam;
        private String _CurFileTargetPath;
        private long _FileSize;
        private int _LatestFileNum;

        public override String CurFileTargetPath
        {
            get
            {
                return _CurFileTargetPath;
            }
            set
            {
                _CurFileTargetPath = value;
            }
        }
        public NLoggerController(Logger logger, NLoggerParam nLoggerParam, NLoggerRuleConfiger nLoggerRuleConfiger) : base(nLoggerParam)
        {
            _Logger = logger;
            _NLoggerRuleConfiger = nLoggerRuleConfiger;

            if (nLoggerParam == null)
            {
                return;
            }

            _LogFilePrefix = "Log_";
            _LogFileExtension = ".txt";
            _NLoggerParam = nLoggerParam;
        }

        public override String BuildLogFileName()
        {
            if (_FileSize > _NLoggerParam.FileSizeLimit)
            {
                _FileSize = 0;
                _LatestFileNum++;
            }

            return MakeNumberingFileNmae(_LatestFileNum);
        }
        private String MakeNumberingFileNmae(int fileCount)
        {
            return _LogFilePrefix + fileCount.ToString().PadLeft(4, '0') + _LogFileExtension;
        }
        public void WriteLog(String msg)
        {
            _Logger.Debug(msg);
        }
    }
}
