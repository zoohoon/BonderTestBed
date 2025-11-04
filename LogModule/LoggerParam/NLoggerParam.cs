using System;

namespace LogModule.LoggerParam
{
    public class NLoggerParam : LoggerParameter
    {
        public String UploadPath { get; set; }
        public bool UploadEnable { get; set; }
        public int FileSizeLimit { get; set; }
        public int UploadFileSizeInterval { get; set; }
        public int UploadTimeInterval { get; set; }
        public int DeleteLogByDay { get; set; }
    }
}