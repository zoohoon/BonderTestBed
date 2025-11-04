using System;
using System.Collections.Generic;
using System.Linq;

namespace LogModule.LoggerParam
{
    using LogModule;
    using ProberErrorCode;
    using System.IO;
    using System.IO.Compression;
    using System.Text.RegularExpressions;

    public class LoggerManagerParameter
    {
        //==> IDeviceParameterizable를 상속 받고 기본 구현해야할 프로퍼티
        public String FilePath { get; set; } = @"C:\Logs";
        public String FileName { get; } = "LogManagerSystemParam.json";
        public NLoggerParam DebugLoggerParam { get; set; }
        public NLoggerParam ProLoggerParam { get; set; }
        public NLoggerParam EventLoggerParam { get; set; }
        public NLoggerParam GpibLoggerParam { get; set; }
        public NLoggerParam PinLoggerParam { get; set; }
        public NLoggerParam PMILoggerParam { get; set; }
        public NLoggerParam TempLoggerParam { get; set; }
        public NLoggerParam LOTLoggerParam { get; set; }
        public NLoggerParam ParamLoggerParam { get; set; }
        public NLoggerParam TCPIPLoggerParam { get; set; }
        public NLoggerParam SoakingLoggerParam { get; set; }
        public NLoggerParam EnvMonitoringLoggerParam { get; set; }        

        public NLoggerParam MonitoringLoggerParam { get; set; }
        public NLoggerParam CompVerifyLoggerParam { get; set; }
        public NLoggerParam ExceptionLoggerParam { get; set; }
        public NLoggerParam InfoLoggerParam { get; set; }
        public NLoggerParam LoaderMapLoggerParam { get; set; }
        public ImageLoggerParam CognexLoggerParam { get; set; }
        public ImageLoggerParam ImageLoggerParam { get; set; }
        public string DevFolder { get; set; }
        public bool LoaderMapLog { get; set; } = false;
        public String GetFilePath()
        {
            string parampath = null;
            string[] CommandLineArgs = Environment.GetCommandLineArgs();
            if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
            {
                parampath = "C:\\ProberSystem\\LoaderSystem\\EMUL";
            }
            else
            {
                foreach (var v in CommandLineArgs)
                {
                    if (v.ToLower().Contains("[path]"))
                    {
                        string[] splitString = v.Split(new string[] { "[path]", "[Path]", "[PATH]" }, StringSplitOptions.RemoveEmptyEntries);

                        if (0 < splitString.Length)
                        {
                            parampath = splitString[0];
                        }
                    }
                }
            }

            string filepath = null;

            if (parampath != null)
            {
                filepath = Path.Combine(parampath, "Parameters", "SystemParam");

            }
            else
            {
                filepath = FilePath;
            }

            return filepath;
        }
        /*
        public EventCodeEnum DeleteLog()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            List<NLoggerParam> nLoggers = new List<NLoggerParam>();
            nLoggers.Add(DebugLoggerParam);
            nLoggers.Add(ProLoggerParam);
            nLoggers.Add(TskLoggerParam);
            nLoggers.Add(EventLoggerParam);
            nLoggers.Add(GpibLoggerParam);
            nLoggers.Add(PinLoggerParam);
            nLoggers.Add(PMILoggerParam);
            nLoggers.Add(TempLoggerParam);
            nLoggers.Add(LOTLoggerParam);
            nLoggers.Add(ParamLoggerParam);
            nLoggers.Add(ExceptionLoggerParam);
            nLoggers.Add(TCPIPLoggerParam);
            nLoggers.Add(SoakingLoggerParam);

            foreach (NLoggerParam n in nLoggers)    //로그 폴더 리스트
            {
                string deletePath = n.LogDirPath;
                DirectoryInfo di = new DirectoryInfo(deletePath);
                List<string> BackupFileList = new List<string>();   //백업하고 삭제 할 파일 리스트
                if (Directory.Exists(deletePath))
                {
                    if (di.GetFiles().Length != 0)  //폴더 존재 여부
                    {
                        FileInfo[] files = di.GetFiles();   //폴더안에 파일 존재 여부
                        string date = DateTime.Today.AddDays(-n.DeleteLogByDay).ToString("yyyy-MM-dd"); //날짜 설정(파라미터로 로그 종류마다 날짜 다르게)
                        Regex rgx = new Regex(@"\d{4}-\d{2}-\d{2}");    //로그 파일 이름에서 날짜만 추출(날짜 형식 : yyyy-MM-dd)

                        foreach (FileInfo file in files)
                        {
                            Match mat = rgx.Match(file.Name);
                            if (date.CompareTo(mat.ToString()) > 0)
                            {
                                BackupFileList.Add(file.FullName);  //설정한 날짜보다 이전 날짜의 파일 백업리스트에 저장
                            }
                        }
                        if (!Directory.Exists(n.LogDirPath + @"\Backup"))
                            Directory.CreateDirectory(n.LogDirPath + @"\Backup");   //Backup폴더 생성
                        CompressZipByIO(BackupFileList, n.LogDirPath + @"\Backup\" + date + " 이전 날짜의 로그 파일 백업.zip", di);    //파일 백업을 위한 압축

                        foreach (FileInfo file in files)
                        {
                            Match mat = rgx.Match(file.Name);
                            if (date.CompareTo(mat.ToString()) > 0)
                            {
                                File.Delete(di + "\\" + file.Name); //백업 후 해당 로그 파일 삭제
                            }
                        }
                    }
                }

            }
            return retval;
        }
        public static void CompressZipByIO(List<string> backupFileList, string zipPath, DirectoryInfo di)
        {
            var filelist = backupFileList;
            if (filelist.Count != 0)
            {
                using (FileStream fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write))  //압축 파일 생성
                {
                    using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                    {
                        foreach (string file in filelist)
                        {
                            string path = file.Substring(di.FullName.Length + 1);
                            zipArchive.CreateEntryFromFile(file, path); //압축 파일에 파일 추가하여 보관
                        }
                    }
                }
            }
        }
        */
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                var loggerParams = new Dictionary<EnumLoggerType, NLoggerParam>();

                if (ProLoggerParam == null) ProLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.PROLOG.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (DebugLoggerParam == null) DebugLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.DEBUG.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (ExceptionLoggerParam == null) ExceptionLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.EXCEPTION.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (EventLoggerParam == null) EventLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.EVENT.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (PinLoggerParam == null) PinLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.PIN.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (PMILoggerParam == null) PMILoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.PMI.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (SoakingLoggerParam == null) SoakingLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.SOAKING.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (GpibLoggerParam == null) GpibLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.GPIB.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (TCPIPLoggerParam == null) TCPIPLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.TCPIP.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (TempLoggerParam == null) TempLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.TEMP.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (LOTLoggerParam == null) LOTLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.LOT.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (ParamLoggerParam == null) ParamLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.PARAMETER.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (CompVerifyLoggerParam == null) CompVerifyLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.COMPVERIFY.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (InfoLoggerParam == null) InfoLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.INFO.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (MonitoringLoggerParam == null) MonitoringLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.MONITORING.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (EnvMonitoringLoggerParam == null) EnvMonitoringLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.ENVMONITORING.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                if (LoaderMapLoggerParam == null) LoaderMapLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.LOADERMAP.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };

                loggerParams[EnumLoggerType.PROLOG] = ProLoggerParam;
                loggerParams[EnumLoggerType.DEBUG] = DebugLoggerParam;
                loggerParams[EnumLoggerType.EXCEPTION] = ExceptionLoggerParam;
                loggerParams[EnumLoggerType.EVENT] = EventLoggerParam;
                loggerParams[EnumLoggerType.PIN] = PinLoggerParam;
                loggerParams[EnumLoggerType.PMI] = PMILoggerParam;
                loggerParams[EnumLoggerType.SOAKING] = SoakingLoggerParam;
                loggerParams[EnumLoggerType.GPIB] = GpibLoggerParam;
                loggerParams[EnumLoggerType.TCPIP] = TCPIPLoggerParam;
                loggerParams[EnumLoggerType.TEMP] = TempLoggerParam;
                loggerParams[EnumLoggerType.LOT] = LOTLoggerParam;
                loggerParams[EnumLoggerType.PARAMETER] = ParamLoggerParam;
                loggerParams[EnumLoggerType.COMPVERIFY] = CompVerifyLoggerParam;
                loggerParams[EnumLoggerType.INFO] = InfoLoggerParam;
                loggerParams[EnumLoggerType.ENVMONITORING] = EnvMonitoringLoggerParam;

                loggerParams[EnumLoggerType.LOADERMAP] = LoaderMapLoggerParam;

                foreach (var item in loggerParams)
                {
                    if (!Directory.Exists(item.Value.LogDirPath))
                    {
                        Directory.CreateDirectory(item.Value.LogDirPath);
                    }
                }

                if (CognexLoggerParam == null)
                {
                    CognexLoggerParam = new ImageLoggerParam();
                    CognexLoggerParam.LogDirPath = Path.Combine(FilePath, DevFolder, "Cognex");

                    if (!Directory.Exists(CognexLoggerParam.LogDirPath))
                    {
                        Directory.CreateDirectory(CognexLoggerParam.LogDirPath);
                    }
                }

                if (ImageLoggerParam == null)
                {
                    ImageLoggerParam = new ImageLoggerParam();
                    ImageLoggerParam.LogDirPath = Path.Combine(FilePath);

                    if (!Directory.Exists(ImageLoggerParam.LogDirPath))
                    {
                        Directory.CreateDirectory(ImageLoggerParam.LogDirPath);
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                string parampath = null;
                string[] CommandLineArgs = Environment.GetCommandLineArgs();

                foreach (var v in CommandLineArgs)
                {
                    if (v.ToLower().Contains("[path]"))
                    {
                        string[] splitString = v.Split(new string[] { "[path]", "[Path]", "[PATH]" }, StringSplitOptions.RemoveEmptyEntries);

                        if (0 < splitString.Length)
                        {
                            parampath = splitString[0];
                        }
                    }
                }

                string filepath = null;

                if (parampath != null)
                {
                    filepath = Path.Combine(parampath, "Parameters", "SystemParam");
                    string[] devfolder = parampath.Split('\\');
                    DevFolder = devfolder[devfolder.Count() - 1];
                }
                else
                {
                    DevFolder = "";
                }

                var loggerParams = new Dictionary<EnumLoggerType, NLoggerParam>();

                ProLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.PROLOG.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                DebugLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.DEBUG.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                ExceptionLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.EXCEPTION.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                EventLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.EVENT.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                PinLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.PIN.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                PMILoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.PMI.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                SoakingLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.SOAKING.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                GpibLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.GPIB.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                TCPIPLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.TCPIP.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                TempLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.TEMP.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                LOTLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.LOT.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                ParamLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.PARAMETER.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                CompVerifyLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.COMPVERIFY.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                InfoLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.INFO.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };
                EnvMonitoringLoggerParam = new NLoggerParam { LogDirPath = Path.Combine(FilePath, DevFolder, EnumLoggerType.ENVMONITORING.ToString()), UploadPath = "", UploadEnable = false, FileSizeLimit = 104857600, UploadFileSizeInterval = 0, UploadTimeInterval = 0, DeleteLogByDay = 90 };

                loggerParams[EnumLoggerType.PROLOG] = ProLoggerParam;
                loggerParams[EnumLoggerType.DEBUG] = DebugLoggerParam;
                loggerParams[EnumLoggerType.EXCEPTION] = ExceptionLoggerParam;
                loggerParams[EnumLoggerType.EVENT] = EventLoggerParam;
                loggerParams[EnumLoggerType.PIN] = PinLoggerParam;
                loggerParams[EnumLoggerType.PMI] = PMILoggerParam;
                loggerParams[EnumLoggerType.SOAKING] = SoakingLoggerParam;
                loggerParams[EnumLoggerType.GPIB] = GpibLoggerParam;
                loggerParams[EnumLoggerType.TCPIP] = TCPIPLoggerParam;
                loggerParams[EnumLoggerType.TEMP] = TempLoggerParam;
                loggerParams[EnumLoggerType.LOT] = LOTLoggerParam;
                loggerParams[EnumLoggerType.PARAMETER] = TempLoggerParam;
                loggerParams[EnumLoggerType.COMPVERIFY] = CompVerifyLoggerParam;
                loggerParams[EnumLoggerType.INFO] = InfoLoggerParam;
                loggerParams[EnumLoggerType.ENVMONITORING] = EnvMonitoringLoggerParam;

                foreach (var item in loggerParams)
                {
                    if (!Directory.Exists(item.Value.LogDirPath))
                    {
                        Directory.CreateDirectory(item.Value.LogDirPath);
                    }
                }

                CognexLoggerParam = new ImageLoggerParam();
                CognexLoggerParam.LogDirPath = Path.Combine(FilePath, DevFolder, "Cognex");

                if (!Directory.Exists(CognexLoggerParam.LogDirPath))
                {
                    Directory.CreateDirectory(CognexLoggerParam.LogDirPath);
                }

                ImageLoggerParam = new ImageLoggerParam();
                ImageLoggerParam.LogDirPath = Path.Combine(FilePath);

                if (!Directory.Exists(ImageLoggerParam.LogDirPath))
                {
                    Directory.CreateDirectory(ImageLoggerParam.LogDirPath);
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
