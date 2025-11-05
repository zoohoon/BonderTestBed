using Autofac;
using LoaderBase.Communication;
using LoaderBase.LoaderLog;
using LoaderBase.LoaderResultMapUpDown;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberInterfaces.ODTP;
using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LoaderLogModule
{
    public class LoaderLogManagerModule : INotifyPropertyChanged, ILoaderLogManagerModule, IFactoryModule, IDisposable, ILoaderFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public bool Initialized { get; set; }
        public InitPriorityEnum InitPriority { get; set; }
        
        private string GPCardPatternBufferImagePath = null;

        private string PMIImagePath = null;
        

        private string ProbeCardListPath = null;

        private LoaderLogParameter _LoaderLogParam = new LoaderLogParameter();

        public LoaderLogParameter LoaderLogParam
        {
            get { return _LoaderLogParam; }
            set { _LoaderLogParam = value; }
        }

        private LoaderProbeCardListParameter _LoaderProbeCardListParam = new LoaderProbeCardListParameter();

        public LoaderProbeCardListParameter LoaderProbeCardListParam
        {
            get { return _LoaderProbeCardListParam; }
            set { _LoaderProbeCardListParam = value; }
        }

        private List<string> _StageDebugDatesFromServer;

        public List<string> StageDebugDatesFromServer
        {
            get { return _StageDebugDatesFromServer; }
            set { _StageDebugDatesFromServer = value; }
        }

        private List<string> _StagePINDatesFromServer;

        public List<string> StagePINDatesFromServer
        {
            get { return _StagePINDatesFromServer; }
            set { _StagePINDatesFromServer = value; }
        }

        private List<string> _StageTEMPatesFromServer;

        public List<string> StageTEMPatesFromServer
        {
            get { return _StageTEMPatesFromServer; }
            set { _StageTEMPatesFromServer = value; }
        }
        private List<string> _StagePMIatesFromServer;

        public List<string> StagePMIatesFromServer
        {
            get { return _StagePMIatesFromServer; }
            set { _StagePMIatesFromServer = value; }
        }

        private List<string> _StageLotDatesFromServer;

        public List<string> StageLotDatesFromServer
        {
            get { return _StageLotDatesFromServer; }
            set { _StageLotDatesFromServer = value; }
        }
        private List<string> _StagePinImageDatesFromServer;

        public List<string> StagePinImageDatesFromServer
        {
            get { return _StagePinImageDatesFromServer; }
            set { _StagePinImageDatesFromServer = value; }
        }

        private List<string> _StageDebugDatesFromStage;

        public List<string> StageDebugDatesFromStage
        {
            get { return _StageDebugDatesFromStage; }
            set { _StageDebugDatesFromStage = value; }
        }

        private List<string> _StagePINDatesFromStage;

        public List<string> StagePINDatesFromStage
        {
            get { return _StagePINDatesFromStage; }
            set { _StagePINDatesFromStage = value; }
        }

        private List<string> _StageTEMPDatesFromStage;

        public List<string> StageTEMPDatesFromStage
        {
            get { return _StageTEMPDatesFromStage; }
            set { _StageTEMPDatesFromStage = value; }
        }
        private List<string> _StagePMIDatesFromStage;

        public List<string> StagePMIDatesFromStage
        {
            get { return _StagePMIDatesFromStage; }
            set { _StagePMIDatesFromStage = value; }
        }

        private List<string> _StageLotDatesFromStage;

        public List<string> StageLotDatesFromStage
        {
            get { return _StageLotDatesFromStage; }
            set { _StageLotDatesFromStage = value; }
        }
        private List<string> _StagePinImageDatesFromStage;

        public List<string> StagePinImageDatesFromStage
        {
            get { return _StagePinImageDatesFromStage; }
            set { _StagePinImageDatesFromStage = value; }
        }

        private List<string> _LoaderDebugDatesFromLoader;

        public List<string> LoaderDebugDatesFromLoader
        {
            get { return _LoaderDebugDatesFromLoader; }
            set { _LoaderDebugDatesFromLoader = value; }
        }

        private List<string> _LoaderDebugDatesFromServer;

        public List<string> LoaderDebugDatesFromServer
        {
            get { return _LoaderDebugDatesFromServer; }
            set { _LoaderDebugDatesFromServer = value; }
        }

        private List<string> _LoaderOCRDatesFromServer;

        public List<string> LoaderOCRDatesFromServer
        {
            get { return _LoaderOCRDatesFromServer; }
            set { _LoaderOCRDatesFromServer = value; }
        }
        private List<string> _LoaderOCRDatesFromLoader;

        public List<string> LoaderOCRDatesFromLoader
        {
            get { return _LoaderOCRDatesFromLoader; }
            set { _LoaderOCRDatesFromLoader = value; }
        }
        

        private List<EnumUploadLogType> _FailedPathLogType = new List<EnumUploadLogType>();
        public List<EnumUploadLogType> FailedPathLogType
        {
            get { return _FailedPathLogType; }
            set
            {
                if (value != _FailedPathLogType)
                {
                    _FailedPathLogType = value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<string> _FailedPath = new List<string>();
        public List<string> FailedPath
        {
            get { return _FailedPath; }
            set
            {
                if (value != _FailedPath)
                {
                    _FailedPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<EnumUploadLogType> _UploadLogListForLoader = new List<EnumUploadLogType>();
        public List<EnumUploadLogType> UploadLogListForLoader
        {
            get { return _UploadLogListForLoader; }
            set
            {
                if (value != _UploadLogListForLoader)
                {
                    _UploadLogListForLoader = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ILoaderCommunicationManager LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        public ILoaderLogSplitManager LoaderLogSplitManager => this.GetLoaderContainer().Resolve<ILoaderLogSplitManager>();
        private ILoaderResultMapUpDownMng LoaderResultMapUpDownMng => this.GetLoaderContainer().Resolve<ILoaderResultMapUpDownMng>();

        private ILoaderODTPManager LoaderODTPManager => this.GetLoaderContainer().Resolve<ILoaderODTPManager>();

        object LogUploadLockObj = new object();

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            return EventCodeEnum.NONE;
        }
        public void Dispose()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void DeInitModule()
        {
            return;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoadParameter();
                this.Initialized = true;
                StageDebugDatesFromServer = new List<string>();
                StageDebugDatesFromStage = new List<string>();
                StagePINDatesFromServer = new List<string>();
                StagePINDatesFromStage = new List<string>();
                StageTEMPatesFromServer = new List<string>();
                StageTEMPDatesFromStage = new List<string>();
                StagePMIatesFromServer = new List<string>();
                StagePMIDatesFromStage = new List<string>();
                StageLotDatesFromServer = new List<string>();
                StageLotDatesFromStage = new List<string>();
                StagePinImageDatesFromStage= new List<string>();
                StagePinImageDatesFromServer = new List<string>();

                LoaderDebugDatesFromLoader = new List<string>();
                LoaderDebugDatesFromServer = new List<string>();
                LoaderOCRDatesFromLoader = new List<string>();
                LoaderOCRDatesFromServer = new List<string>();
                LoaderResultMapUpDownMng.InitModule();
                LoaderODTPManager.InitModule();
                try
                {
                    GPCardPatternBufferImagePath = this.FileManager().GetSystemRootPath() + "//CardBufferImg";
                    if (Directory.Exists(GPCardPatternBufferImagePath) == false)
                    {
                        Directory.CreateDirectory(GPCardPatternBufferImagePath);
                    }
                    PMIImagePath = this.FileManager().GetSystemRootPath() + "//PMIImg";
                    if (Directory.Exists(PMIImagePath) == false)
                    {
                        Directory.CreateDirectory(PMIImagePath);
                    }

                    ProbeCardListPath = GPCardPatternBufferImagePath + "\\" + LoaderProbeCardListParam.FileName;
                    if (File.Exists(ProbeCardListPath) == false)
                    {
                        if (LoaderProbeCardListParam != null)
                        {
                            Extensions_IParam.SaveParameter(null, LoaderProbeCardListParam, null, ProbeCardListPath);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            retVal = EventCodeEnum.PARAM_ERROR;
                        }
                    }
                    else
                    {
                        IParam tmpParam = null;
                        retVal = this.LoadParameter(ref tmpParam, typeof(LoaderProbeCardListParameter), null, ProbeCardListPath);
                        if(retVal == EventCodeEnum.NONE)
                        {
                            LoaderProbeCardListParam = (tmpParam as LoaderProbeCardListParameter);
                        }
                        else
                        {
                            LoggerManager.Debug($"[LoaderLogManagerModule]LoaderProbeCardListParam LoadParameter Fail.");
                        }
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                }

                IntervalMilliSec = LoaderLogParam.AutoLogUploadIntervalMinutes.Value * 60000;
                UpdateLogUploadListForLoader(EnumUploadLogType.LoaderDebug);
                if (LoaderLogParam.AutoLogUploadIntervalMinutes.Value > 0)
                {
                    logUploadTimer = new Timer(UploadlogsCallback, null, IntervalMilliSec, IntervalMilliSec);
                }
                else
                {
                    logUploadTimer = new Timer(UploadlogsCallback, null, Timeout.Infinite, Timeout.Infinite);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private static Timer logUploadTimer;

        private int _IntervalMilliSec;
        public int IntervalMilliSec
        {
            get { return _IntervalMilliSec; }
            set
            {
                if (value != _IntervalMilliSec)
                {
                    _IntervalMilliSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void SetIntervalForLogUpload(int newInterval)
        {
            try
            {
                LoaderLogParam.AutoLogUploadIntervalMinutes.Value = newInterval;
                IntervalMilliSec = LoaderLogParam.AutoLogUploadIntervalMinutes.Value * 60000;

                if (logUploadTimer != null)
                {
                    if (IntervalMilliSec > 0)
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule]SetIntervalForLogUpload: {IntervalMilliSec}, Timer Reset");
                        logUploadTimer.Change(IntervalMilliSec, IntervalMilliSec);
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule]SetIntervalForLogUpload: {IntervalMilliSec}, Timer abort");
                        logUploadTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void UploadlogsCallback(object obj)
        {
            try
            {
                UploadRecentLogs();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UploadRecentLogs(int cellidx = -1)
        {
            try
            {
                // Auto업로드에서 각 타입에 맞는 로그를 업로드 하고 있는 중에 Type이 Add되거나 변경되면
                // Exception이 발생할 수 있기 때문에 List에 접근하는 곳은 LOCK 처리 함.
                lock (updateLogUploadListLockObj)
                {
                    LoggerManager.Debug($"[LoaderLogManagerModule]UploadRecentLogs() Cell Log Upload Start");
                    var stages = this.LoaderCommunicationManager.GetStages();
                    IEnumerable<IStageObject> targetStages;

                    if (cellidx != -1)
                    {
                        // 특정 셀
                        targetStages = stages.Where(stage => stage.Index == cellidx);
                    }
                    else
                    {
                        // 전체 셀
                        targetStages = stages;
                    }

                    foreach (var stage in targetStages)
                    {
                        try
                        {
                            if (cellidx == -1 && !stage.StageInfo.IsConnected)
                            {
                                continue;
                            }

                            foreach (var type in stage.StageInfo.UploadLogList)
                            {
                                CellLogUploadToServer(stage.Index, type);
                            }

                            // Debug 로그 제외 업로드 리스트에서 클리어
                            stage.StageInfo.UploadLogList = stage.StageInfo.UploadLogList.Where(x => x == EnumUploadLogType.Debug).ToList();
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                    }

                    LoggerManager.Debug($"[LoaderLogManagerModule]UploadRecentLogs() Cell Log Upload End");

                    LoggerManager.Debug($"[LoaderLogManagerModule]UploadRecentLogs() Loader Log Upload Start");
                    foreach (var type in UploadLogListForLoader)
                    {
                        LoaderLogUploadToServer(type);
                    }
                    // Loader Debug 로그 제외 업로드 리스트에서 클리어
                    UploadLogListForLoader = UploadLogListForLoader.Where(x => x == EnumUploadLogType.LoaderDebug).ToList();
                    LoggerManager.Debug($"[LoaderLogManagerModule]UploadRecentLogs() Loader Log Upload End");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        object updateLogUploadListLockObj = new object();
        public void UpdateLogUploadListForLoader(EnumUploadLogType logType)
        {
            try
            {
                // 로그를 업로드 하는 중에 List가 업데이트 되려고 하면 Lock으로 인해 기다리고 있기 때문에 로그 업로드 중에 동작이 멈출 수 있다. 
                // Task로 비동기 처리하여 호출한 곳에 서는 다음 동작을 이어서 동작할 수 있도록 한다.
                Task task = new Task(() =>
                {
                    // Auto업로드에서 각 타입에 맞는 로그를 업로드 하고 있는 중에 Type이 Add되거나 변경되면
                    // Exception이 발생할 수 있기 때문에 List에 접근하는 곳은 LOCK 처리 함.
                    try
                    {
                        lock (updateLogUploadListLockObj)
                        {
                            if (!UploadLogListForLoader.Contains(logType))
                            {
                                LoggerManager.Debug($"[LoaderLogManagerModule]UpdateLogUploadListForLoader() type: {logType}");
                                UploadLogListForLoader.Add(logType);
                            }
                            else
                            {
                                LoggerManager.Debug($"[LoaderLogManagerModule]UpdateLogUploadListForLoader() already included type : {logType}");
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                });
                task.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateLogUploadListForStage(int cellindex, EnumUploadLogType logType)
        {
            try
            {
                Task task = new Task(() =>
                {
                    try
                    {
                        lock (updateLogUploadListLockObj)
                        {
                            var stages = this.LoaderCommunicationManager.GetStages();
                            var stage = stages.Where(x => x.Index == cellindex).FirstOrDefault();
                            if (!stage.StageInfo.UploadLogList.Contains(logType))
                            {
                                LoggerManager.Debug($"[LoaderLogManagerModule]UpdateLogUploadListForStage() type: {logType}");
                                stage.StageInfo.UploadLogList.Add(logType);
                            }
                            else
                            {
                                LoggerManager.Debug($"[LoaderLogManagerModule]UpdateLogUploadListForStage() already included type : {logType}");
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                });
                task.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //spooling option apply
                LoaderResultMapUpDownMng.SpoolingOptionUpdate(LoaderLogParam.ResultMapUpLoadPath.Value, LoaderLogParam.UserName.Value, 
                    LoaderLogParam.Password.Value, LoaderLogParam.FTPUsePassive.Value, LoaderLogParam.ResultMapUploadRetryCount.Value, LoaderLogParam.ResultMapUploadDelayTime.Value);

                LoaderODTPManager.SpoolingOptionUpdate(LoaderLogParam.ODTPUpLoadPath.Value, LoaderLogParam.UserName.Value,
                    LoaderLogParam.Password.Value);

                string OpParameterPath = Path.Combine(this.FileManager().GetRootParamPath(), @"Parameters\Loader\LoaderLogParameter.json");

                if (File.Exists(OpParameterPath) == true)
                {
                    if (LoaderLogParam != null)
                    {
                        Extensions_IParam.SaveParameter(null, LoaderLogParam, null, OpParameterPath);
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        retVal = EventCodeEnum.PARAM_ERROR;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"SaveParameter(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum LoadParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                string OpParameterPath = Path.Combine(this.FileManager().GetRootParamPath(), @"Parameters\Loader\LoaderLogParameter.json");

                if (File.Exists(OpParameterPath) == false)
                {
                    LoaderLogParam = CreateDefaultParamm();
                    LoaderLogParam.SetDefaultParam();
                    Extensions_IParam.SaveParameter(null, LoaderLogParam, null, OpParameterPath);

                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                    IParam tmpParam = null;
                    retVal = this.LoadParameter(ref tmpParam, typeof(LoaderLogParameter), null, OpParameterPath);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        throw new Exception("[LoaderLogManager] Load Failure Parameter.");
                    }
                    else
                    {
                        LoaderLogParam = tmpParam as LoaderLogParameter;
                        LoadParameterAddNewItemDefaultSet();
                    }
                }

                LoaderLogParameter CreateDefaultParamm()
                {
                    return new LoaderLogParameter();
                }

                /// LoaderLogParameter.json 파일이 있으면서 신규 Item이 추가 되었을때 해당 Item의 
                void LoadParameterAddNewItemDefaultSet()
                {
                   if(null != LoaderLogParam)
                    {
                        if( string.IsNullOrEmpty(LoaderLogParam.SpoolingBasePath.Value) )
                        {
                            LoaderLogParam.SpoolingBasePath.Value = LoaderLogParam.spoolingBasePath ;
                        }

                        if (LoaderLogParam.ResultMapUploadDelayTime.Value < LoaderLogParam.minSpoolingDelayTimeSec) // 3초보다 작게 설정하면 default 5초 값으로 설정
                            LoaderLogParam.ResultMapUploadDelayTime.Value = LoaderLogParam.defaultSpoolingDelayTimeSec; 

                        if(0 == LoaderLogParam.ResultMapUploadRetryCount.Value)
                           LoaderLogParam.ResultMapUploadRetryCount.Value = LoaderLogParam.defaultSpoolingRetryCnt;
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"LoadParameter(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #region Loader

        #region Compare Loader and server
        private EventCodeEnum CompareDatesLoaderAndServer()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                for (int i = 0; i < LoaderDebugDatesFromLoader.Count; i++)
                {
                    for (int j = 0; j < LoaderDebugDatesFromServer.Count; j++)
                    {
                        if (LoaderDebugDatesFromServer[j] == LoaderDebugDatesFromLoader[i])
                        {
                            LoaderDebugDatesFromLoader.RemoveAt(i);
                        }
                    }
                }

                for (int i = 0; i < LoaderOCRDatesFromLoader.Count; i++)
                {
                    for (int j = 0; j < LoaderOCRDatesFromServer.Count; j++)
                    {
                        if (LoaderOCRDatesFromServer[j] == LoaderOCRDatesFromLoader[i])
                        {
                            LoaderOCRDatesFromLoader.RemoveAt(i);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CompareDatesLoaderAndServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Debug($"[LoaderLogManagerModule]Fail Compare date between loader and server");
                LoggerManager.Exception(err);
            }
            finally
            {
            }
            return ret;
        }
        #endregion

        #region Loader log upload to server
        private EventCodeEnum UploadLoaderDebug()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                string zippath = this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value + "\\" + $"LoaderLog" + ".zip";
                var serverpath = this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value;
                //루드/디버그
                if (!Directory.Exists(serverpath))
                {
                    Directory.CreateDirectory(serverpath);
                }


                //루트/템프
                if (!Directory.Exists(serverpath + "\\" + "templog"))
                {
                    Directory.CreateDirectory(serverpath + "\\" + "templog");
                    ZipFile.ExtractToDirectory(serverpath + "\\" + "LoaderLog" + ".zip",
                        serverpath + "\\" + "templog");
                }
                else
                {
                    Directory.Delete(serverpath + "\\" + "templog", true);
                    Directory.CreateDirectory(serverpath + "\\" + "templog");
                    ZipFile.ExtractToDirectory(serverpath + "\\" + "LoaderLog" + ".zip",
                         serverpath + "\\" + "templog");
                }
                if (Directory.Exists(serverpath + "\\" + "templog" + "\\" + "Debug"))
                {
                    string[] files = System.IO.Directory.GetFiles(serverpath + "\\" + "templog" + "\\" + "Debug", "*.log");
                    if (files.Length != 0)
                    {
                        foreach (var s in files)
                        {
                            // Create the FileInfo object only when needed to ensure
                            // the information is as current as possible.
                            System.IO.FileInfo fi = null;
                            try
                            {
                                fi = new System.IO.FileInfo(s);

                                File.Copy(fi.FullName, serverpath + "\\" + fi.Name, true);
                                LoggerManager.Debug($"[LoaderLogManagerModule]Succeed upload file:{fi.Name}");
                            }
                            catch (System.IO.FileNotFoundException e)
                            {
                                LoggerManager.Exception(e);
                                continue;
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }
                        }
                    }
                }
                else
                {
                    //올릴게 없다
                }
                if (Directory.Exists(serverpath + "\\" + "templog"))
                {
                    System.Threading.Thread.Sleep(300);
                    Directory.Delete(serverpath + "\\" + "templog", true);
                }
                ret = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private EventCodeEnum UploadLoaderOCRLogAndImage()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                //string date = $"{DateTime.Today.Year}-{DateTime.Today.Month.ToString().PadLeft(2, '0')}-{DateTime.Today.Day.ToString().PadLeft(2, '0')}";
                string zippath = this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value + "\\" + $"LoaderLog" + ".zip";
                var serverpath = this.LoaderLogParam.LoaderOCRUpDownLoadPath.Value;
                string ocrpath = serverpath;

                //루드/디버그
                if (!Directory.Exists(serverpath))
                {
                    Directory.CreateDirectory(serverpath);
                }

                //루트/템프
                if (!Directory.Exists(serverpath + "\\" + "templog"))
                {
                    Directory.CreateDirectory(serverpath + "\\" + "templog");
                    ZipFile.ExtractToDirectory(zippath,
                        serverpath + "\\" + "templog");
                }
                else
                {
                    Directory.Delete(serverpath + "\\" + "templog", true);
                    Directory.CreateDirectory(serverpath + "\\" + "templog");
                    ZipFile.ExtractToDirectory(zippath,
                         serverpath + "\\" + "templog");
                }

                if (!Directory.Exists(ocrpath))
                {
                    Directory.CreateDirectory(ocrpath);
                }

                if (Directory.Exists(serverpath + "\\" + "templog" + "\\" + "Cognex"))
                {
                    DirectoryInfo directory = new DirectoryInfo(serverpath + "\\" + "templog" + "\\" + "Cognex");

                    var folders = directory.GetDirectories();

                    foreach (var folder in folders)
                    {
                        Directory.CreateDirectory(serverpath + "\\" + folder.Name);
                        var files = folder.GetFiles();
                        if (files.Length != 0)
                        {
                            foreach (var file in files)
                            {
                                file.CopyTo(serverpath + "\\" + folder.Name + "\\" + file.Name, true);
                                LoggerManager.Debug($"[LoaderLogManagerModule]Succeed upload file:{file.Name}");
                            }
                        }

                    }

                }
                else
                {
                    //올릴게 없다
                }

                if (Directory.Exists(serverpath + "\\" + "templog"))
                {
                    System.Threading.Thread.Sleep(300);
                    Directory.Delete(serverpath + "\\" + "templog", true);
                }

                ret = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public EventCodeEnum ManualLoaderLogUploadServer()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string todaydate = $"{DateTime.Today.Year}-{DateTime.Today.Month.ToString().PadLeft(2, '0')}-{DateTime.Today.Day.ToString().PadLeft(2, '0')}";

            string loaderlogtemppath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp";
            string localzippath = @"C:\Logs\LoaderUpload" + "\\" + $"LoaderLog" + ".zip";
            var localserverpath = @"C:\Logs\LoaderUpload" + "\\" + $"LoaderLog";

            string zippath = this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value + "\\" + $"LoaderLog" + ".zip";
            var serverpath = this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value;

            if (LoaderLogParam.UploadEnable.Value == true)
            {
                try
                {
                    lock (LogUploadLockObj)
                    {
                        if (Directory.Exists(loaderlogtemppath))
                            Directory.Delete(loaderlogtemppath, true);

                        if (!Directory.Exists(loaderlogtemppath))
                        {
                            Directory.CreateDirectory(loaderlogtemppath);
                        }

                        Dictionary<EnumUploadLogType, bool> pathCheck = new Dictionary<EnumUploadLogType, bool>();
                        Action<string, EnumUploadLogType> AddPathCheck = (string logPath, EnumUploadLogType logType) =>
                        {
                            ret = LoaderLogSplitManager.ConnectCheck(logPath, this.LoaderLogParam.UserName.Value, this.LoaderLogParam.Password.Value);
                            if (ret == EventCodeEnum.NONE)
                            {
                                ret = LoaderLogSplitManager.CheckFolderExist(logPath, this.LoaderLogParam.UserName.Value, this.LoaderLogParam.Password.Value);
                                if (ret != EventCodeEnum.NONE)
                                {
                                    ret = LoaderLogSplitManager.CreateDicrectory(logPath, this.LoaderLogParam.UserName.Value, this.LoaderLogParam.Password.Value);
                                    if (ret != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[LoaderLogManagerModule]The path is incorrect : {logPath}");
                                        pathCheck.Add(logType, false);
                                        FailedPath.Add(logPath);
                                        FailedPathLogType.Add(logType);
                                    }
                                    else
                                    {
                                        pathCheck.Add(logType, true);
                                    }
                                }
                                else
                                {
                                    pathCheck.Add(logType, true);
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"[LoaderLogManagerModule]The path is incorrect : {logPath}");
                                FailedPath.Add(logPath);
                                FailedPathLogType.Add(logType);
                            // throw 던지기
                            // throw new Exception("[ManualLoaderLogUploadServer] ConnectCheck Function Failure.");
                        }
                        };

                        GetLoaderDebugDatesFromLoader();
                        AddPathCheck(this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value, EnumUploadLogType.LoaderDebug);
                        GetLoaderDebugDatesFromServer();

                        GetLoaderOCRDatesFromLoader();
                        AddPathCheck(this.LoaderLogParam.LoaderOCRUpDownLoadPath.Value, EnumUploadLogType.LoaderOCR);
                        GetLoaderOCRDatesFromServer();

                        CompareDatesLoaderAndServer();

                        LoaderDebugDatesFromLoader.Add(todaydate);
                        LoaderOCRDatesFromLoader.Add(todaydate);
                        //Debug,OCR,
                        for (int i = 0; i < LoaderDebugDatesFromLoader.Count; i++)
                        {
                            CopyLoaderDebugLog(LoaderDebugDatesFromLoader[i]);
                        }
                        for (int i = 0; i < LoaderOCRDatesFromLoader.Count; i++)
                        {
                            CopyLoaderOCRLogAndImage(LoaderOCRDatesFromLoader[i]);
                        }

                        if (!File.Exists(localzippath))
                        {
                            ZipFile.CreateFromDirectory(loaderlogtemppath, localzippath);
                        }

                        if (Directory.Exists(localserverpath))
                            Directory.Delete(localserverpath, true);
                        System.Threading.Thread.Sleep(1000);
                        ZipFile.ExtractToDirectory(localzippath, localserverpath);
                        bool path = false;

                        pathCheck.TryGetValue(EnumUploadLogType.LoaderDebug, out path);
                        if (path)
                        {
                            LoaderLogSplitManager.LoaderLogUploadToServer(localserverpath,
                                this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value, this.LoaderLogParam.UserName.Value, this.LoaderLogParam.Password.Value,
                                EnumUploadLogType.LoaderDebug);
                        }
                        pathCheck.TryGetValue(EnumUploadLogType.LoaderOCR, out path);
                        if (path)
                        {
                            LoaderLogSplitManager.LoaderLogUploadToServer(localserverpath,
                                this.LoaderLogParam.LoaderOCRUpDownLoadPath.Value, this.LoaderLogParam.UserName.Value, this.LoaderLogParam.Password.Value,
                                EnumUploadLogType.LoaderOCR);
                        }
                        if (File.Exists(localzippath))
                            File.Delete(localzippath);
                        if (Directory.Exists(localserverpath))
                            Directory.Delete(localserverpath, true);
                        if (Directory.Exists(loaderlogtemppath))
                            Directory.Delete(loaderlogtemppath, true);
                        LoggerManager.Debug($"[LoaderLogManagerModule]Succeed Upload Loader Log");
                        ret = EventCodeEnum.NONE;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"[LoaderLogManagerModule]Fail log upload to server");
                    LoggerManager.Exception(err);
                    if (ret == EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT)
                    {

                        LoggerManager.Debug("[LoaderLogManagerModule]Login or password incorrect! error code : LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT");
                        return ret = EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT;
                    }
                    else
                    {
                        LoggerManager.Debug("[LoaderLogManagerModule]Could not connect to server. error code : LOGUPLOAD_CONNECT_FAIL");
                        return ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                    }                    
                }
                finally
                {
                    if (File.Exists(localzippath))
                        File.Delete(localzippath);
                }
            }
            else
            {
                LoggerManager.Debug($"[LoaderLogManagerModule]ManualLoaderLogUploadServer() LoaderLogParam.UploadEnable is False");
                ret = EventCodeEnum.NONE;
            }
            return ret;
        }
        public EventCodeEnum LoaderLogUploadServer(DateTime startdate, DateTime enddate)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            int uploadcnt = enddate.Day - startdate.Day;

            try
            {
                if (LoaderLogParam.UploadEnable.Value == false)
                {
                    LoggerManager.Debug($"[LoaderLogManagerModule]LoaderLogUploadServer() Failed. UploadEnable is false");
                    return ret;
                }

                lock (LogUploadLockObj)
                {
                    LoggerManager.Debug("[LoaderLogManagerModule][S]Loader log Upload To Server");

                    string loaderlogtemppath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp";
                    string localzippath = @"C:\Logs\LoaderUpload" + "\\" + $"LoaderLog" + ".zip";
                    var localserverpath = @"C:\Logs\LoaderUpload" + "\\" + $"LoaderLog";

                    if (Directory.Exists(loaderlogtemppath))
                        Directory.Delete(loaderlogtemppath, true);

                    if (!Directory.Exists(loaderlogtemppath))
                    {
                        Directory.CreateDirectory(loaderlogtemppath);
                    }

                    FolderExistCheckAndCreate(this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value, EnumUploadLogType.LoaderDebug);

                    FolderExistCheckAndCreate(this.LoaderLogParam.LoaderOCRUpDownLoadPath.Value, EnumUploadLogType.LoaderOCR);

                    string date = null;
                    //Debug,OCR,
                    for (int i = 0; i <= uploadcnt; i++)
                    {
                        date = $"{DateTime.Today.AddDays(i * -1).Year}-{DateTime.Today.AddDays(i).Month.ToString().PadLeft(2, '0')}-{DateTime.Today.AddDays(i * -1).Day.ToString().PadLeft(2, '0')}";
                        CopyLoaderDebugLog(date);
                        CopyLoaderOCRLogAndImage(date);
                    }

                    if (!File.Exists(localzippath))
                    {
                        ZipFile.CreateFromDirectory(loaderlogtemppath, localzippath);
                    }

                    if (Directory.Exists(localserverpath))
                        Directory.Delete(localserverpath, true);
                    System.Threading.Thread.Sleep(1000);
                    ZipFile.ExtractToDirectory(localzippath, localserverpath);

                    ret = LoaderLogSplitManager.LoaderLogUploadToServer(localserverpath,
                        this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value, this.LoaderLogParam.UserName.Value, this.LoaderLogParam.Password.Value,
                        EnumUploadLogType.LoaderDebug);
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule]Fail {EnumUploadLogType.LoaderDebug} upload. ret={ret}");
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule]Sucess {EnumUploadLogType.LoaderDebug} upload");
                    }

                    ret = LoaderLogSplitManager.LoaderLogUploadToServer(localserverpath,
                        this.LoaderLogParam.LoaderOCRUpDownLoadPath.Value, this.LoaderLogParam.UserName.Value, this.LoaderLogParam.Password.Value,
                        EnumUploadLogType.LoaderOCR);
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule]Fail {EnumUploadLogType.LoaderOCR} upload. ret={ret}");
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule]Sucess {EnumUploadLogType.LoaderOCR} upload");
                    }

                    if (File.Exists(localzippath))
                        File.Delete(localzippath);
                    if (Directory.Exists(localserverpath))
                        Directory.Delete(localserverpath, true);
                    if (Directory.Exists(loaderlogtemppath))
                        Directory.Delete(loaderlogtemppath, true);

                    LoggerManager.Debug("[LoaderLogManagerModule][E]Loader log Upload To Server");

                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LoaderLogManagerModule]Fail log upload to server");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        #endregion

        #region Get LoaderLog
        public EventCodeEnum CopyLoaderOCRLogAndImage(string date)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string logdate = date;
            string loaderlogtemppath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp" + "\\" + "Cognex";

            try
            {
                DirectoryInfo directory = new DirectoryInfo(LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "Cognex");

                if (!Directory.Exists(loaderlogtemppath))
                {
                    Directory.CreateDirectory(loaderlogtemppath);
                }
                try
                {
                    var folders = directory.GetDirectories();

                    foreach (var folder in folders)
                    {
                        if (folder.Name.Contains(logdate))
                        {
                            if (Directory.Exists(loaderlogtemppath))
                            {
                                Directory.CreateDirectory(loaderlogtemppath + "\\" + logdate);
                                var files = folder.GetFiles();
                                foreach (var file in files)
                                {
                                    file.CopyTo(loaderlogtemppath + "\\" + logdate + "\\" + $"{file.Name}", true);
                                }
                            }
                        }
                    }

                    ret = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"CopyLoaderOCRLogAndImage(): Error occurred. Err = {err.Message}");
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"CopyLoaderOCRLogAndImage(): Error occurred. Err = {err.Message}");
                LoggerManager.Debug($"[LoaderLogManagerModule]Fail log upload to server");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public EventCodeEnum CopyLoaderDebugLog(string date)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string logdate = date;
            string loaderlogtemppath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp" + "\\" + "LOT";

            try
            {
                DirectoryInfo directory = new DirectoryInfo(LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "LOT");

                if (!Directory.Exists(loaderlogtemppath))
                {
                    Directory.CreateDirectory(loaderlogtemppath);
                }
                try
                {
                    var files = directory.GetFiles();
                    foreach (var debugfile in files)
                    {
                        if (debugfile.Name.Contains(logdate))
                        {
                            if (Directory.Exists(loaderlogtemppath))
                            {
                                debugfile.CopyTo(loaderlogtemppath + "\\" + $"{debugfile.Name}", true);
                            }

                        }
                    }
                    ret = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"CopyLoaderDebugLog(): Error occurred. Err = {err.Message}");
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"CopyLoaderDebugLog(): Error occurred. Err = {err.Message}");
                LoggerManager.Debug($"[LoaderLogManagerModule]Fail log upload to server");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        #endregion

        #region GetDates Loader and server
        private EventCodeEnum GetLoaderDebugDatesFromServer()
        {
            //ManualLoaderLogUploadServer() 에서만 사용되기에 연결 확인 안함. 이미 연결 확인 완료.

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            NetworkCredential credentials = null;
            credentials = new NetworkCredential(this.LoaderLogParam.UserName.Value,
                           this.LoaderLogParam.Password.Value);
            LoaderDebugDatesFromServer.Clear();
            List<string> tmplist = new List<string>();
            string checkpath = null;
            
            if(this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value == null || this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value == "")
            {
                LoggerManager.Debug($"[LoaderLogManagerModule]GetLoaderDebugDatesFromServer() LoaderSystemUpDownLoadPath is null");
                return ret;
            }

            if (this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value[this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value.Length - 1] == '/')
            {
                checkpath = this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value;
            }
            else
            {
                checkpath = this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value + '/';
            }

            ret = LoaderLogSplitManager.CheckFolderExist(checkpath, this.LoaderLogParam.UserName.Value, this.LoaderLogParam.Password.Value);
            if (ret == EventCodeEnum.NONE)
            {
            }
            else
            {
                ret = LoaderLogSplitManager.CreateDicrectory(checkpath, this.LoaderLogParam.UserName.Value, this.LoaderLogParam.Password.Value);
                if (ret != EventCodeEnum.NONE)
                {
                    return ret;
                }
            }
            LoaderLogSplitManager.GetLoaderDatesFromServer(this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value,
                this.LoaderLogParam.UserName.Value, this.LoaderLogParam.Password.Value, ref tmplist);
            var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");

            try
            {
                //using (new NetworkConnection(this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value, credentials))
                //{
                //    var serverpath = this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value;
                //    if (Directory.Exists(serverpath))
                //    {
                //        string[] files = System.IO.Directory.GetFiles(serverpath, "*.log");
                //        foreach (var s in files)
                //        {
                //            System.IO.FileInfo fi = null;
                //            try
                //            {
                //                fi = new System.IO.FileInfo(s);
                //                foreach (Match m in reg.Matches(fi.Name))
                //                {
                //                    LoaderDebugDatesFromServer.Add(m.Value);
                //                }
                //            }
                //            catch (System.IO.FileNotFoundException e)
                //            {
                //                LoggerManager.Exception(e);
                //                continue;
                //            }
                //            catch (Exception err)
                //            {
                //                LoggerManager.Exception(err);
                //            }
                //        }

                //    }
                //}
                //)
                if (tmplist.Count > 0)
                {
                    foreach (var item in tmplist)
                    {
                        foreach (Match m in reg.Matches(item))
                        {
                            LoaderDebugDatesFromServer.Add(m.Value);
                        }
                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetLoaderDebugDatesFromServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private EventCodeEnum GetLoaderOCRDatesFromServer()
        {
            //ManualLoaderLogUploadServer() 에서만 사용되기에 연결 확인 안함. 이미 연결 확인 완료.

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            NetworkCredential credentials = null;
            credentials = new NetworkCredential(this.LoaderLogParam.UserName.Value,
                           this.LoaderLogParam.Password.Value);
            LoaderOCRDatesFromServer.Clear();
            List<string> tmplist = new List<string>();
            string checkpath = null;

            if (this.LoaderLogParam.LoaderOCRUpDownLoadPath.Value == null || this.LoaderLogParam.LoaderOCRUpDownLoadPath.Value == "")
            {
                LoggerManager.Debug($"[LoaderLogManagerModule]GetLoaderOCRDatesFromServer() LoaderSystemUpDownLoadPath is null");
                return ret;
            }

            if (this.LoaderLogParam.LoaderOCRUpDownLoadPath.Value[this.LoaderLogParam.LoaderOCRUpDownLoadPath.Value.Length - 1] == '/')
            {
                checkpath = this.LoaderLogParam.LoaderOCRUpDownLoadPath.Value;
            }
            else
            {
                checkpath = this.LoaderLogParam.LoaderOCRUpDownLoadPath.Value + '/';
            }
            ret = LoaderLogSplitManager.CheckFolderExist(checkpath, this.LoaderLogParam.UserName.Value, this.LoaderLogParam.Password.Value);
            if (ret == EventCodeEnum.NONE)
            {
            }
            else
            {
                ret = LoaderLogSplitManager.CreateDicrectory(checkpath, this.LoaderLogParam.UserName.Value, this.LoaderLogParam.Password.Value);
                if (ret != EventCodeEnum.NONE)
                {
                    return ret;
                }
            }
            LoaderLogSplitManager.GetLoaderOCRDatesFromServer(this.LoaderLogParam.LoaderOCRUpDownLoadPath.Value,
                this.LoaderLogParam.UserName.Value, this.LoaderLogParam.Password.Value, ref tmplist);
            var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");
            var serverpath = this.LoaderLogParam.LoaderOCRUpDownLoadPath.Value;

            try
            {
                //using (new NetworkConnection(serverpath, credentials))
                //{
                //    if (Directory.Exists(serverpath))
                //    {
                //        DirectoryInfo directory = new DirectoryInfo(serverpath);
                //        var dir = directory.GetDirectories();
                //        foreach (var folder in dir)
                //        {
                //            foreach (Match m in reg.Matches(folder.Name))
                //            {
                //                LoaderOCRDatesFromServer.Add(m.Value);
                //            }
                //        }

                //    }
                //}
                if (tmplist.Count > 0)
                {
                    foreach (var item in tmplist)
                    {
                        foreach (Match m in reg.Matches(item))
                        {
                            LoaderOCRDatesFromServer.Add(m.Value);
                        }
                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetLoaderOCRDatesFromServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private EventCodeEnum GetLoaderDebugDatesFromLoader()
        {
            //ManualLoaderLogUploadServer() 에서만 사용되기에 연결 확인 안함. 이미 연결 확인 완료.

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            LoaderDebugDatesFromLoader.Clear();
            var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");
            try
            {
                DirectoryInfo directory = new DirectoryInfo(LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "LOT");
                var files = directory.GetFiles();
                foreach (var debugfile in files)
                {
                    foreach (Match m in reg.Matches(debugfile.Name))
                    {
                        LoaderDebugDatesFromLoader.Add(m.Value);
                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetLoaderDebugDatesFromLoader(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private EventCodeEnum GetLoaderOCRDatesFromLoader()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            LoaderOCRDatesFromLoader.Clear();
            var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");
            try
            {
                DirectoryInfo directory = new DirectoryInfo(LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "Cognex");
                var dir = directory.GetDirectories();
                foreach (var folder in dir)
                {
                    foreach (Match m in reg.Matches(folder.Name))
                    {
                        LoaderOCRDatesFromLoader.Add(m.Value);
                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetLoaderOCRDatesFromLoader(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        #endregion

        #endregion

        #region Stage
        #region Compare Stage and server
        private EventCodeEnum CompareDatesStageAndServer()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                for (int i = 0; i < StageDebugDatesFromStage?.Count; i++)
                {
                    for (int j = 0; j < StageDebugDatesFromServer.Count; j++)
                    {
                        if (StageDebugDatesFromStage.Count > 0)
                        {
                            if (StageDebugDatesFromServer[j] == StageDebugDatesFromStage[i])
                            {
                                StageDebugDatesFromStage.RemoveAt(i);
                            }
                        }
                    }
                }

                for (int i = 0; i < StageTEMPDatesFromStage?.Count; i++)
                {
                    for (int j = 0; j < StageTEMPatesFromServer.Count; j++)
                    {
                        if (StageTEMPDatesFromStage.Count > 0)
                        {
                            if (StageTEMPatesFromServer[j] == StageTEMPDatesFromStage[i])
                            {
                                StageTEMPDatesFromStage.RemoveAt(i);
                            }
                        }
                    }
                }

                for (int i = 0; i < StagePINDatesFromStage?.Count; i++)
                {
                    for (int j = 0; j < StagePINDatesFromServer.Count; j++)
                    {
                        if (StagePINDatesFromStage.Count > 0)
                        {
                            if (StagePINDatesFromServer[j] == StagePINDatesFromStage[i])
                            {
                                StagePINDatesFromStage.RemoveAt(i);
                            }
                        }
                    }
                }

                for (int i = 0; i < StagePMIDatesFromStage?.Count; i++)
                {
                    for (int j = 0; j < StagePMIatesFromServer.Count; j++)
                    {
                        if (StagePMIDatesFromStage.Count > 0)
                        { 
                            if (StagePMIatesFromServer[j] == StagePMIDatesFromStage[i])
                            {
                                StagePMIDatesFromStage.RemoveAt(i);
                            }
                        }
                    }
                }

                for (int i = 0; i < StageLotDatesFromStage?.Count; i++)
                {
                    for (int j = 0; j < StageLotDatesFromServer.Count; j++)
                    {
                        if (StageLotDatesFromStage.Count > 0)
                        {
                            if (StageLotDatesFromServer[j] == StageLotDatesFromStage[i])
                            {
                                StageLotDatesFromStage.RemoveAt(i);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CompareDatesStageAndServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Debug($"[LoaderLogManagerModule]Fail log upload to server");
                LoggerManager.Exception(err);
            }
            finally
            {
            }
            return ret;
        }

        private EventCodeEnum CompareDatesStageAndServer(EnumUploadLogType logtype)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                switch (logtype)
                {
                    case EnumUploadLogType.Debug:
                        for (int i = 0; i < StageDebugDatesFromStage?.Count; i++)
                        {
                            for (int j = 0; j < StageDebugDatesFromServer.Count; j++)
                            {
                                if (StageDebugDatesFromServer[j] == StageDebugDatesFromStage[i])
                                {
                                    StageDebugDatesFromStage.RemoveAt(i);
                                }
                            }
                        }
                        break;
                    case EnumUploadLogType.Temp:
                        for (int i = 0; i < StageTEMPDatesFromStage?.Count; i++)
                        {
                            for (int j = 0; j < StageTEMPatesFromServer.Count; j++)
                            {
                                if (StageTEMPatesFromServer[j] == StageTEMPDatesFromStage[i])
                                {
                                    StageTEMPDatesFromStage.RemoveAt(i);
                                }
                            }
                        }
                        break;
                    case EnumUploadLogType.PMI:
                        for (int i = 0; i < StagePMIDatesFromStage?.Count; i++)
                        {
                            for (int j = 0; j < StagePMIatesFromServer.Count; j++)
                            {
                                if (StagePMIatesFromServer[j] == StagePMIDatesFromStage[i])
                                {
                                    StagePMIDatesFromStage.RemoveAt(i);
                                }
                            }
                        }
                        break;
                    case EnumUploadLogType.PIN:
                        for (int i = 0; i < StagePINDatesFromStage?.Count; i++)
                        {
                            for (int j = 0; j < StagePINDatesFromServer.Count; j++)
                            {
                                if (StagePINDatesFromServer[j] == StagePINDatesFromStage[i])
                                {
                                    StagePINDatesFromStage.RemoveAt(i);
                                }
                            }
                        }
                        break;
                    case EnumUploadLogType.LOT:
                        for (int i = 0; i < StageLotDatesFromStage?.Count; i++)
                        {
                            for (int j = 0; j < StageLotDatesFromServer.Count; j++)
                            {
                                if (StageLotDatesFromServer[j] == StageLotDatesFromStage[i])
                                {
                                    StageLotDatesFromStage.RemoveAt(i);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CompareDatesStageAndServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Debug($"[LoaderLogManagerModule]Fail log upload to server");
                LoggerManager.Exception(err);
            }
            finally
            {
            }
            return ret;
        }
        #endregion
        #region StageLog upload to server
        private EventCodeEnum UploadDebug(int cellindex)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                string zippath = this.LoaderLogParam.StageSystemUpDownLoadPath.Value + "\\" + $"Cell{cellindex.ToString().PadLeft(2, '0')}" + ".zip";
                var serverpath = this.LoaderLogParam.StageSystemUpDownLoadPath.Value;
                var cell = $"Cell{ cellindex.ToString().PadLeft(2, '0')}";
                //루드/디버그
                if (!Directory.Exists(serverpath))
                {
                    Directory.CreateDirectory(serverpath);
                }
                if (!Directory.Exists(serverpath + "\\" + cell))
                {
                    Directory.CreateDirectory(serverpath + "\\" + cell);
                }

                //루트/템프
                if (!Directory.Exists(serverpath + "\\" + cell + "\\" + "templog"))
                {
                    Directory.CreateDirectory(serverpath + "\\" + cell + "\\" + "templog");
                    ZipFile.ExtractToDirectory(serverpath + "\\" + cell + ".zip",
                        serverpath + "\\" + cell + "\\" + "templog");
                }
                else
                {
                    Directory.Delete(serverpath + "\\" + cell + "\\" + "templog", true);
                    Directory.CreateDirectory(serverpath + "\\" + cell + "\\" + "templog");
                    ZipFile.ExtractToDirectory(serverpath + "\\" + cell + ".zip",
                        serverpath + "\\" + cell + "\\" + "templog");
                }
                if (Directory.Exists(serverpath + "\\" + cell + "\\" + "templog" + "\\" + "Debug"))
                {
                    string[] files = System.IO.Directory.GetFiles(serverpath + "\\" + cell + "\\" + "templog" + "\\" + "Debug", "*.log");
                    foreach (var s in files)
                    {
                        // Create the FileInfo object only when needed to ensure
                        // the information is as current as possible.
                        System.IO.FileInfo fi = null;
                        try
                        {
                            fi = new System.IO.FileInfo(s);

                            File.Copy(fi.FullName, serverpath + "\\" + cell + "\\" + fi.Name, true);
                            LoggerManager.Debug($"[LoaderLogManagerModule]Succeed upload file:{fi.Name}, Cell{cellindex.ToString().PadLeft(2, '0')}");
                        }
                        catch (System.IO.FileNotFoundException e)
                        {
                            LoggerManager.Exception(e);
                            continue;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                    }
                }
                else
                {
                    //올릴게 없다
                }
                if (Directory.Exists(serverpath + "\\" + cell + "\\" + "templog"))
                {
                    System.Threading.Thread.Sleep(300);
                    Directory.Delete(serverpath + "\\" + cell + "\\" + "templog", true);
                }
                ret = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Error($"UploadDebug(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private EventCodeEnum UploadPMI(int cellindex)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var cell = $"Cell{ cellindex.ToString().PadLeft(2, '0')}";
                string zippath = this.LoaderLogParam.StageSystemUpDownLoadPath.Value + "\\" + cell + ".zip";
                var serverpath = this.LoaderLogParam.StagePMIUpDownLoadPath.Value;
                //루드/디버그
                if (!Directory.Exists(serverpath))
                {
                    Directory.CreateDirectory(serverpath);
                }
                if (!Directory.Exists(serverpath + "\\" + cell))
                {
                    Directory.CreateDirectory(serverpath + "\\" + cell);
                }

                //루트/템프
                if (!Directory.Exists(serverpath + "\\" + cell + "\\" + "templog"))
                {
                    Directory.CreateDirectory(serverpath + "\\" + cell + "\\" + "templog");
                    ZipFile.ExtractToDirectory(zippath,
                        serverpath + "\\" + cell + "\\" + "templog");
                }
                else
                {
                    Directory.Delete(serverpath + "\\" + cell + "\\" + "templog", true);
                    Directory.CreateDirectory(serverpath + "\\" + cell + "\\" + "templog");
                    ZipFile.ExtractToDirectory(zippath,
                        serverpath + "\\" + cell + "\\" + "templog");
                }
                if (Directory.Exists(serverpath + "\\" + cell + "\\" + "templog" + "\\" + "PMI"))
                {
                    string[] files = System.IO.Directory.GetFiles(serverpath + "\\" + cell + "\\" + "templog" + "\\" + "PMI", "*.log");
                    foreach (var s in files)
                    {
                        // Create the FileInfo object only when needed to ensure
                        // the information is as current as possible.
                        System.IO.FileInfo fi = null;
                        try
                        {
                            fi = new System.IO.FileInfo(s);

                            File.Copy(fi.FullName, serverpath + "\\" + cell + "\\" + fi.Name, true);
                            LoggerManager.Debug($"[LoaderLogManagerModule]Succeed upload file:{fi.Name}, Cell{cellindex.ToString().PadLeft(2, '0')}");
                        }
                        catch (System.IO.FileNotFoundException e)
                        {
                            LoggerManager.Exception(e);
                            continue;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                    }
                }
                else
                {
                    //올릴게 없다
                }
                if (Directory.Exists(serverpath + "\\" + cell + "\\" + "templog"))
                {
                    System.Threading.Thread.Sleep(300);
                    Directory.Delete(serverpath + "\\" + cell + "\\" + "templog", true);
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"UploadPMI(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private EventCodeEnum UploadPIN(int cellindex)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var cell = $"Cell{ cellindex.ToString().PadLeft(2, '0')}";
                string zippath = this.LoaderLogParam.StageSystemUpDownLoadPath.Value + "\\" + cell + ".zip";
                var serverpath = this.LoaderLogParam.StagePinUpDownLoadPath.Value;
                if (!Directory.Exists(serverpath))
                {
                    Directory.CreateDirectory(serverpath);
                }
                if (!Directory.Exists(serverpath + "\\" + cell))
                {
                    Directory.CreateDirectory(serverpath + "\\" + cell);
                }

                if (!Directory.Exists(serverpath + "\\" + cell + "\\" + "templog"))
                {
                    Directory.CreateDirectory(serverpath + "\\" + cell + "\\" + "templog");
                    ZipFile.ExtractToDirectory(zippath,
                        serverpath + "\\" + cell + "\\" + "templog");
                }
                else
                {
                    Directory.Delete(serverpath + "\\" + cell + "\\" + "templog", true);
                    Directory.CreateDirectory(serverpath + "\\" + cell + "\\" + "templog");
                    ZipFile.ExtractToDirectory(zippath,
                        serverpath + "\\" + cell + "\\" + "templog");
                }

                if (Directory.Exists(serverpath + "\\" + cell + "\\" + "templog" + "\\" + "PIN"))
                {
                    string[] files = System.IO.Directory.GetFiles(serverpath + "\\" + cell + "\\" + "templog" + "\\" + "PIN", "*.log");
                    foreach (var s in files)
                    {
                        // Create the FileInfo object only when needed to ensure
                        // the information is as current as possible.
                        System.IO.FileInfo fi = null;
                        try
                        {
                            fi = new System.IO.FileInfo(s);

                            File.Copy(fi.FullName, serverpath + "\\" + cell + "\\" + fi.Name, true);
                            LoggerManager.Debug($"[LoaderLogManagerModule]Succeed upload file:{fi.Name}, Cell{cellindex.ToString().PadLeft(2, '0')}");
                        }
                        catch (System.IO.FileNotFoundException e)
                        {
                            LoggerManager.Exception(e);
                            continue;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                    }
                }
                else
                {
                    //올릴게 없다
                }
                if (Directory.Exists(serverpath + "\\" + cell + "\\" + "templog"))
                {
                    System.Threading.Thread.Sleep(300);
                    Directory.Delete(serverpath + "\\" + cell + "\\" + "templog", true);
                }
                ret = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Error($"UploadPIN(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private EventCodeEnum UploadTemp(int cellindex)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var cell = $"Cell{ cellindex.ToString().PadLeft(2, '0')}";
                string zippath = this.LoaderLogParam.StageSystemUpDownLoadPath.Value + "\\" + cell + ".zip";
                var serverpath = this.LoaderLogParam.StageTempUpDownLoadPath.Value;
                if (!Directory.Exists(serverpath))
                {
                    Directory.CreateDirectory(serverpath);
                }
                if (!Directory.Exists(serverpath + "\\" + cell))
                {
                    Directory.CreateDirectory(serverpath + "\\" + cell);
                }

                if (!Directory.Exists(serverpath + "\\" + cell + "\\" + "templog"))
                {
                    Directory.CreateDirectory(serverpath + "\\" + cell + "\\" + "templog");
                    ZipFile.ExtractToDirectory(zippath,
                        serverpath + "\\" + cell + "\\" + "templog");
                }
                else
                {
                    Directory.Delete(serverpath + "\\" + cell + "\\" + "templog", true);
                    Directory.CreateDirectory(serverpath + "\\" + cell + "\\" + "templog");
                    ZipFile.ExtractToDirectory(zippath,
                        serverpath + "\\" + cell + "\\" + "templog");
                }

                if (Directory.Exists(serverpath + "\\" + cell + "\\" + "templog" + "\\" + "TEMP"))
                {
                    string[] files = System.IO.Directory.GetFiles(serverpath + "\\" + cell + "\\" + "templog" + "\\" + "TEMP", "*.log");
                    foreach (var s in files)
                    {
                        // Create the FileInfo object only when needed to ensure
                        // the information is as current as possible.
                        System.IO.FileInfo fi = null;
                        try
                        {
                            fi = new System.IO.FileInfo(s);

                            File.Copy(fi.FullName, serverpath + "\\" + cell + "\\" + fi.Name, true);
                            LoggerManager.Debug($"[LoaderLogManagerModule]Succeed upload file:{fi.Name}, Cell{cellindex.ToString().PadLeft(2, '0')}");
                        }
                        catch (System.IO.FileNotFoundException e)
                        {
                            LoggerManager.Exception(e);
                            continue;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                    }
                }
                else
                {
                    //올릴게 없다
                }
                if (Directory.Exists(serverpath + "\\" + cell + "\\" + "templog"))
                {
                    System.Threading.Thread.Sleep(300);
                    Directory.Delete(serverpath + "\\" + cell + "\\" + "templog", true);
                }
                ret = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Error($"UploadTemp(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        
        public bool FileWriteAndExtract(string localzippath, string localserverpath, byte[] datas)
        {
            bool bRet = false;

            try
            {              
                if (datas.Length <= 0)
                {
                    LoggerManager.Debug($"[LoaderLogManagerModule][FileWriteAndExtract] upload count is zero Path:{localserverpath}");
                    return bRet;
                }

                if (!Directory.Exists(localserverpath))
                {
                    Directory.CreateDirectory(localserverpath);
                }

                File.WriteAllBytes(localzippath, datas);

                if (Directory.Exists(localserverpath))
                {
                    Directory.Delete(localserverpath, true);
                }

                System.Threading.Thread.Sleep(1000);
                ZipFile.ExtractToDirectory(localzippath, localserverpath);

                bRet = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return bRet;
        }
        public EventCodeEnum StagesPinTipSizeValidationImageUploadServer(DateTime startdate, DateTime enddate)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            EventCodeEnum isFolderExist = EventCodeEnum.UNDEFINED;
            int uploadcount = enddate.Day - startdate.Day;
            string stagePINImagePath = "";
            
            try
            {
                if (LoaderLogParam.UploadEnable.Value == false)
                {
                    LoggerManager.Debug($"[LoaderLogManagerModule]StagesPinTipSizeValidationImageUploadServer() Failed. UploadEnable is false");
                    return ret;
                }

                lock (LogUploadLockObj)
                {
                    LoggerManager.Debug("[LoaderLogManagerModule][S]Stage PinTipSizeValidImage Upload To Server");
                    
                    var stages = this.LoaderCommunicationManager.GetStages();
                    foreach (var stage in stages)
                    {
                        try
                        {
                            if (stage.StageInfo.IsConnected)
                            {
                                StageLogParameter stageLog = this.LoaderLogParam.StageLogParams.Where(x => x.StageIndex.Value == stage.Index).FirstOrDefault();

                                if (stageLog == null)
                                {
                                    LoggerManager.Debug($"[LoaderLogManagerModule]StagesPinTipSizeValidationImageUploadServer() StageLogParam is null");
                                    continue;
                                }
                                else
                                {
                                    if (LoaderLogParam.CanUseStageLogParam.Value == true)
                                    {
                                        stagePINImagePath = stageLog.StagePinTipValidationResultUploadPath.Value;
                                    }
                                    else
                                    {
                                        stagePINImagePath = LoaderLogParam.StagePinTipValidationResultUploadPath.Value;
                                    }
                                }

                                LoggerManager.Debug($"[LoaderLogManagerModule]Before Get Proxy. Cell{stage.Index.ToString().PadLeft(2, '0')} ");
                                var cell = this.LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stage.Index);
                                LoggerManager.Debug($"[LoaderLogManagerModule]Before Cell{stage.Index.ToString().PadLeft(2, '0')} uploading pin Image to server");

                                StagePinImageDatesFromStage.Clear();

                                for (int i = 0; i <= uploadcount; i++)
                                {
                                    string date = $"{DateTime.Today.AddDays(i * -1).Year}-{DateTime.Today.AddDays(i).Month.ToString().PadLeft(2, '0')}-{DateTime.Today.AddDays(i * -1).Day.ToString().PadLeft(2, '0')}";
                                    StagePinImageDatesFromStage.Add(date);
                                }

                                isFolderExist = FolderExistCheckAndCreate(stagePINImagePath, EnumUploadLogType.PINImage, stage.Index);
                                if (isFolderExist != EventCodeEnum.NONE)
                                {
                                    LoggerManager.Debug($"[LoaderLogManagerModule]StagesPinTipSizeValidationImageUploadServer() Failed. ret: {isFolderExist}");
                                    return ret;
                                }

                                byte[] datas = cell.GetPinImageFromStage(StagePinImageDatesFromStage);

                                string localzippath = @"C:\Logs\LoaderUpload" + "\\" + $"PinImageLotEnd" + "\\" + $"Cell{stage.Index.ToString().PadLeft(2, '0')}" + ".zip";
                                var localserverpath = @"C:\Logs\LoaderUpload" + "\\" + $"PinImageLotEnd" + "\\" + $"Cell{stage.Index.ToString().PadLeft(2, '0')}";

                                if (FileWriteAndExtract(localzippath, localserverpath, datas))
                                {
                                    string[] folders = Directory.GetDirectories(localserverpath);
                                    foreach (var folder in folders)
                                    {
                                        string final_folder_name = new DirectoryInfo(folder).Name;

                                        string destpath;
                                        if (LoaderLogParam.CanUseStageLogParam.Value == true)
                                        {
                                            destpath = stagePINImagePath + '/' + final_folder_name;
                                        }
                                        else
                                        {
                                            destpath = stagePINImagePath;
                                        }
                                        LoaderLogSplitManager.CellLogUploadToServer(stage.Index, localserverpath + "\\" + final_folder_name, destpath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.PINImage, final_folder_name);
                                    }

                                    LoggerManager.Debug($"[LoaderLogManagerModule]Succeed Upload pin Image Cell{stage.Index.ToString().PadLeft(2, '0')}");
                                }

                                if (File.Exists(localzippath))
                                {
                                    File.Delete(localzippath);
                                }

                                if (Directory.Exists(localserverpath))
                                {
                                    Directory.Delete(localserverpath, true);
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"StagesPinTipSizeValidationImageUploadServer(): Error occurred. Err = {err.Message}");
                            LoggerManager.Debug($"[LoaderLogManagerModule]Error occured Cell {stage.Index.ToString().PadLeft(2, '0')} uploading pin image to server");
                            LoggerManager.Exception(err);
                        }
                    }

                    LoggerManager.Debug("[LoaderLogManagerModule][E]Stage PinTipSizeValidImage Upload To Server");

                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"StagesPinTipSizeValidationImageUploadServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Debug($"[LoaderLogManagerModule]Error occured while get stages");
                LoggerManager.Exception(err);
            }
            
            return ret;
        }
        public EventCodeEnum StagesLogUploadServer(DateTime startdate, DateTime enddate)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            int uploadcount = enddate.Day - startdate.Day;
            string stageDebugLogPath = "";
            string stagePinLogPath = "";
            string stageTempLogPath = "";
            string stagePMILogPath = "";
            string stageLOTLogPath = "";

            try
            {
                if (LoaderLogParam.UploadEnable.Value == false)
                {
                    LoggerManager.Debug($"[LoaderLogManagerModule]StagesLogUploadServer() Failed. UploadEnable is false");
                    return ret;
                }

                lock (LogUploadLockObj)
                {
                    LoggerManager.Debug("[LoaderLogManagerModule][S]Stage log Upload To Server");

                    var stages = this.LoaderCommunicationManager.GetStages();
                    foreach (var stage in stages)
                    {
                        try
                        {
                            if (stage.StageInfo.IsConnected)
                            {
                                StageLogParameter stageLog = this.LoaderLogParam.StageLogParams.Where(x => x.StageIndex.Value == stage.Index).FirstOrDefault();

                                if (stageLog == null)
                                {
                                    LoggerManager.Debug($"[LoaderLogManagerModule]StagesLogUploadServer() StageLogParam is null");
                                    continue;
                                }
                                else
                                {
                                    if (LoaderLogParam.CanUseStageLogParam.Value == true)
                                    {
                                        stageDebugLogPath = stageLog.StageSystemUpDownLoadPath.Value;
                                        stagePinLogPath = stageLog.StagePinUpDownLoadPath.Value;
                                        stageTempLogPath = stageLog.StageTempUpDownLoadPath.Value;
                                        stagePMILogPath = stageLog.StagePMIUpDownLoadPath.Value;
                                        stageLOTLogPath = stageLog.StageLOTUpDownLoadPath.Value;
                                    }
                                    else
                                    {
                                        stageDebugLogPath = LoaderLogParam.StageSystemUpDownLoadPath.Value;
                                        stagePinLogPath = LoaderLogParam.StagePinUpDownLoadPath.Value;
                                        stageTempLogPath = LoaderLogParam.StageTempUpDownLoadPath.Value;
                                        stagePMILogPath = LoaderLogParam.StagePMIUpDownLoadPath.Value;
                                        stageLOTLogPath = LoaderLogParam.StageLOTUpDownLoadPath.Value;
                                    }
                                }

                                LoggerManager.Debug($"[LoaderLogManagerModule]Before Get Proxy. Cell{stage.Index.ToString().PadLeft(2, '0')} ");
                                var cell = this.LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stage.Index);
                                LoggerManager.Debug($"[LoaderLogManagerModule]Before Cell{stage.Index.ToString().PadLeft(2, '0')} uploading log to server");

                                StageDebugDatesFromStage.Clear();
                                StageTEMPDatesFromStage.Clear();
                                StagePINDatesFromStage.Clear();
                                StagePMIDatesFromStage.Clear();
                                StageLotDatesFromStage.Clear();

                                for (int i = 0; i <= uploadcount; i++)
                                {
                                    string date = $"{DateTime.Today.AddDays(i * -1).Year}-{DateTime.Today.AddDays(i).Month.ToString().PadLeft(2, '0')}-{DateTime.Today.AddDays(i * -1).Day.ToString().PadLeft(2, '0')}";
                                    StageDebugDatesFromStage.Add(date);
                                    StageTEMPDatesFromStage.Add(date);
                                    StagePINDatesFromStage.Add(date);
                                    StagePMIDatesFromStage.Add(date);
                                    StageLotDatesFromStage.Add(date);
                                }

                                FolderExistCheckAndCreate(stageDebugLogPath, EnumUploadLogType.Debug, stage.Index);

                                FolderExistCheckAndCreate(stageTempLogPath, EnumUploadLogType.Temp, stage.Index);

                                FolderExistCheckAndCreate(stagePinLogPath, EnumUploadLogType.PIN, stage.Index);

                                FolderExistCheckAndCreate(stagePMILogPath, EnumUploadLogType.PMI, stage.Index);

                                FolderExistCheckAndCreate(stageLOTLogPath, EnumUploadLogType.LOT, stage.Index);

                                byte[] datas = cell.GetLogFromFilename(StageDebugDatesFromStage, StageTEMPDatesFromStage, StagePINDatesFromStage, StagePMIDatesFromStage, StageLotDatesFromStage);

                                string localzippath = @"C:\Logs\LoaderUpload" + "\\" + $"All" + "\\" + $"Cell{stage.Index.ToString().PadLeft(2, '0')}" + ".zip";
                                var localserverpath = @"C:\Logs\LoaderUpload" + "\\" + $"All" + "\\" + $"Cell{stage.Index.ToString().PadLeft(2, '0')}";

                                if (FileWriteAndExtract(localzippath, localserverpath, datas))
                                {
                                    ret = LoaderLogSplitManager.CellLogUploadToServer(stage.Index, localserverpath, stageDebugLogPath,
                                    LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.Debug);
                                    if (ret != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[LoaderLogManagerModule]Fail Cell{stage.Index.ToString().PadLeft(2, '0')} System Log upload. ret={ret}");
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"[LoaderLogManagerModule]Sucess Cell{stage.Index.ToString().PadLeft(2, '0')} System Log upload");
                                    }
                                    ret = LoaderLogSplitManager.CellLogUploadToServer(stage.Index, localserverpath, stagePinLogPath,
                                        LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.PIN);
                                    if (ret != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[LoaderLogManagerModule]Fail Cell{stage.Index.ToString().PadLeft(2, '0')} Pin Log upload. ret={ret}");
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"[LoaderLogManagerModule]Sucess Cell{stage.Index.ToString().PadLeft(2, '0')} Pin Log upload");
                                    }
                                    ret = LoaderLogSplitManager.CellLogUploadToServer(stage.Index, localserverpath, stagePMILogPath,
                                        LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.PMI);
                                    if (ret != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[LoaderLogManagerModule]Fail Cell{stage.Index.ToString().PadLeft(2, '0')} PMI Log upload. ret={ret}");
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"[LoaderLogManagerModule]Sucess Cell{stage.Index.ToString().PadLeft(2, '0')} PMI Log upload");
                                    }
                                    ret = LoaderLogSplitManager.CellLogUploadToServer(stage.Index, localserverpath, stageTempLogPath,
                                        LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.Temp);
                                    if (ret != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[LoaderLogManagerModule]Fail Cell{stage.Index.ToString().PadLeft(2, '0')} Temp Log upload. ret={ret}");
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"[LoaderLogManagerModule]Sucess Cell{stage.Index.ToString().PadLeft(2, '0')} Temp Log upload");
                                    }
                                    ret = LoaderLogSplitManager.CellLogUploadToServer(stage.Index, localserverpath, stageLOTLogPath,
                                        LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.LOT);
                                    if (ret != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[LoaderLogManagerModule]Fail Cell{stage.Index.ToString().PadLeft(2, '0')} LOT Log upload. ret={ret}");
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"[LoaderLogManagerModule]Sucess Cell{stage.Index.ToString().PadLeft(2, '0')} LOT Log upload");
                                    }

                                    LoggerManager.Debug($"[LoaderLogManagerModule]Succeed Upload Log Cell{stage.Index.ToString().PadLeft(2, '0')}");
                                }

                                if (File.Exists(localzippath))
                                {
                                    File.Delete(localzippath);
                                }

                                if (Directory.Exists(localserverpath))
                                {
                                    Directory.Delete(localserverpath, true);
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"StagesLogUploadServer(): Error occurred. Err = {err.Message}");
                            LoggerManager.Debug($"[LoaderLogManagerModule]Error occured Cell {stage.Index.ToString().PadLeft(2, '0')} uploading log to server");
                            LoggerManager.Exception(err);
                        }
                    }

                    LoggerManager.Debug("[LoaderLogManagerModule][E]Stage log Upload To Server");

                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"StagesLogUploadServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Debug($"[LoaderLogManagerModule]Error occured while get stages");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public EventCodeEnum ManualStageLogUploadServer()
        {
            // //ManualUploadStageAndLoader() 에서만 사용되기에 연결 확인 안함. 이미 연결 확인 완료. 

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string todaydate = $"{DateTime.Today.Year}-{DateTime.Today.Month.ToString().PadLeft(2, '0')}-{DateTime.Today.Day.ToString().PadLeft(2, '0')}";
            string stageDebugLogPath = "";
            string stagePinLogPath = "";
            string stageTempLogPath = "";
            string stagePMILogPath = "";
            string stageLOTLogPath = "";

            try
            {
                lock (LogUploadLockObj)
                {
                    Dictionary<EnumUploadLogType, bool> pathCheck = new Dictionary<EnumUploadLogType, bool>();
                    var stages = this.LoaderCommunicationManager.GetStages();

                    Action<string, EnumUploadLogType, StageLogParameter> AddPathCheck = (string logPath, EnumUploadLogType logType, StageLogParameter stageLog) =>
                    {
                        ret = LoaderLogSplitManager.ConnectCheck(logPath,
                            LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
                        if (ret == EventCodeEnum.NONE)
                        {
                            ret = LoaderLogSplitManager.CheckFolderExist(logPath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
                            if (ret != EventCodeEnum.NONE)
                            {
                                ret = LoaderLogSplitManager.CreateDicrectory(logPath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
                                if (ret != EventCodeEnum.NONE)
                                {
                                    LoggerManager.Debug($"[LoaderLogManagerModule]The path is incorrect : {logPath}");
                                    pathCheck.Add(logType, false);
                                    FailedPath.Add(logPath);
                                    FailedPathLogType.Add(logType);
                                }
                                else
                                {
                                    pathCheck.Add(logType, true);
                                }
                            }
                            else
                            {
                                pathCheck.Add(logType, true);
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"[LoaderLogManagerModule]The path is incorrect : {logPath}");
                            FailedPath.Add(logPath);
                            FailedPathLogType.Add(logType);
                        // throw 던지기
                        // throw new Exception("[ManualLoaderLogUploadServer] ConnectCheck Function Failure.");
                    }
                    };

                    foreach (var stage in stages)
                    {
                        try
                        {

                            if (stage.StageInfo.IsConnected)
                            {
                                var cell = this.LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stage.Index);

                                StageLogParameter stageLog = this.LoaderLogParam.StageLogParams.Where(x => x.StageIndex.Value == stage.Index).FirstOrDefault();

                                if (stageLog == null)
                                {
                                    LoggerManager.Debug($"[LoaderLogManagerModule]ManualStageLogUploadServer() StageLogParam is null");
                                    continue;
                                }
                                else
                                {
                                    if (LoaderLogParam.CanUseStageLogParam.Value == true)
                                    {
                                        stageDebugLogPath = stageLog.StageSystemUpDownLoadPath.Value;
                                        stagePinLogPath = stageLog.StagePinUpDownLoadPath.Value;
                                        stageTempLogPath = stageLog.StageTempUpDownLoadPath.Value;
                                        stagePMILogPath = stageLog.StagePMIUpDownLoadPath.Value;
                                        stageLOTLogPath = stageLog.StageLOTUpDownLoadPath.Value;
                                    }
                                    else
                                    {
                                        stageDebugLogPath = LoaderLogParam.StageSystemUpDownLoadPath.Value;
                                        stagePinLogPath = LoaderLogParam.StagePinUpDownLoadPath.Value;
                                        stageTempLogPath = LoaderLogParam.StageTempUpDownLoadPath.Value;
                                        stagePMILogPath = LoaderLogParam.StagePMIUpDownLoadPath.Value;
                                        stageLOTLogPath = LoaderLogParam.StageLOTUpDownLoadPath.Value;
                                    }
                                }

                                LoggerManager.Debug($"[LoaderLogManagerModule]Before Cell{stage.Index.ToString().PadLeft(2, '0')} uploading log to server");
                                StageDebugDatesFromStage.Clear();
                                StageDebugDatesFromStage = cell.GetStageDebugDates();
                                StageTEMPDatesFromStage.Clear();
                                StageTEMPDatesFromStage = cell.GetStageTempDates();
                                StagePINDatesFromStage.Clear();
                                StagePINDatesFromStage = cell.GetStagePinDates();
                                StagePMIDatesFromStage.Clear();
                                StagePMIDatesFromStage = cell.GetStagePMIDates();
                                StageLotDatesFromStage.Clear();
                                StageLotDatesFromStage = cell.GetStageLotDates();

                                string cellindex = $"Cell{stage.Index.ToString().PadLeft(2, '0')}";

                                AddPathCheck(stageDebugLogPath, EnumUploadLogType.Debug, stageLog);
                                GetStageDebugDatesFromServer(cellindex, stageLog);

                                AddPathCheck(stageTempLogPath, EnumUploadLogType.Temp, stageLog);
                                GetStageTEMPDatesFromServer(cellindex, stageLog);

                                AddPathCheck(stagePinLogPath, EnumUploadLogType.PIN, stageLog);
                                GetStagePINDatesFromServer(cellindex, stageLog);

                                AddPathCheck(stagePMILogPath, EnumUploadLogType.PMI, stageLog);
                                GetStagePMIDatesFromServer(cellindex, stageLog);

                                AddPathCheck(stageLOTLogPath, EnumUploadLogType.LOT, stageLog);
                                GetStageLotDatesFromServer(cellindex, stageLog);

                                CompareDatesStageAndServer();

                                StageDebugDatesFromStage.Add(todaydate);
                                StageTEMPDatesFromStage.Add(todaydate);
                                StagePINDatesFromStage.Add(todaydate);
                                StagePMIDatesFromStage.Add(todaydate);
                                StageLotDatesFromStage.Add(todaydate);

                                byte[] datas = cell.GetLogFromFilename(StageDebugDatesFromStage, StageTEMPDatesFromStage, StagePINDatesFromStage, StagePMIDatesFromStage, StageLotDatesFromStage);

                                string localzippath = @"C:\Logs\LoaderUpload" + "\\" + $"All" + "\\" + $"Cell{stage.Index.ToString().PadLeft(2, '0')}" + ".zip";
                                var localserverpath = @"C:\Logs\LoaderUpload" + "\\" + $"All" + "\\" + $"Cell{stage.Index.ToString().PadLeft(2, '0')}";

                                if (FileWriteAndExtract(localzippath, localserverpath, datas))
                                {
                                    bool check = false;

                                    pathCheck.TryGetValue(EnumUploadLogType.Debug, out check);
                                    if (check)
                                    {
                                        LoaderLogSplitManager.CellLogUploadToServer(stage.Index, localserverpath, stageDebugLogPath,
                                        LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.Debug);
                                    }
                                    pathCheck.TryGetValue(EnumUploadLogType.PIN, out check);
                                    if (check)
                                    {
                                        LoaderLogSplitManager.CellLogUploadToServer(stage.Index, localserverpath, stagePinLogPath,
                                        LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.PIN);
                                    }
                                    pathCheck.TryGetValue(EnumUploadLogType.PMI, out check);
                                    if (check)
                                    {
                                        LoaderLogSplitManager.CellLogUploadToServer(stage.Index, localserverpath, stagePMILogPath,
                                        LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.PMI);
                                    }
                                    pathCheck.TryGetValue(EnumUploadLogType.Temp, out check);
                                    if (check)
                                    {
                                        LoaderLogSplitManager.CellLogUploadToServer(stage.Index, localserverpath, stageTempLogPath,
                                        LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.Temp);
                                    }
                                    pathCheck.TryGetValue(EnumUploadLogType.LOT, out check);
                                    if (check)
                                    {
                                        LoaderLogSplitManager.CellLogUploadToServer(stage.Index, localserverpath, stageLOTLogPath,
                                        LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.LOT);
                                    }

                                    LoggerManager.Debug($"[LoaderLogManagerModule]Succeed Upload Log Cell{stage.Index.ToString().PadLeft(2, '0')}");
                                }

                                string zippath = stageDebugLogPath + "\\" + $"Cell{stage.Index.ToString().PadLeft(2, '0')}" + ".zip";

                                if (File.Exists(zippath))
                                {
                                    File.Delete(zippath);
                                }

                                if (File.Exists(localzippath))
                                {
                                    File.Delete(localzippath);
                                }

                                if (Directory.Exists(localserverpath))
                                {
                                    Directory.Delete(localserverpath, true);
                                }
                            }
                            pathCheck.Clear();
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"ManaualStageLogUploadServer(): Error occurred. Err = {err.Message}");
                            LoggerManager.Debug($"[LoaderLogManagerModule]Error occured Cell {stage.Index.ToString().PadLeft(2, '0')} uploading log to server");
                            LoggerManager.Exception(err);
                            if (ret == EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT)
                            {

                                LoggerManager.Debug("[LoaderLogManagerModule]Login or password incorrect! error code : LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT");
                                return ret = EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT;
                            }
                            else
                            {
                                LoggerManager.Debug("[LoaderLogManagerModule]Could not connect to server. error code : LOGUPLOAD_CONNECT_FAIL");
                                return ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                            }
                        }
                    }
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ManualStageLogUploadServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Debug($"[LoaderLogManagerModule]Fail log upload to server");
                LoggerManager.Exception(err);
            }
            finally
            {
            }
            return ret;
        }

        public EventCodeEnum LoaderLogUploadToServer(EnumUploadLogType logtype)
        {
            
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string lopPath = "";
            EventCodeEnum isFolderExist = EventCodeEnum.UNDEFINED;

            try
            {
                string todaydate = $"{DateTime.Today.Year}-{DateTime.Today.Month.ToString().PadLeft(2, '0')}-{DateTime.Today.Day.ToString().PadLeft(2, '0')}";

                if (LoaderLogParam.UploadEnable.Value == false)
                {
                    LoggerManager.Debug($"[LoaderLogManagerModule]LoaderLogUploadToServer() Failed. UploadEnable is false, Log Type: {logtype}");
                    return ret;
                }

                lock (LogUploadLockObj)
                {
                    string loaderlogtemppath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp";
                    string localzippath = @"C:\Logs\LoaderUpload" + "\\" + $"LoaderLog" + ".zip";
                    var localserverpath = @"C:\Logs\LoaderUpload" + "\\" + $"LoaderLog";

                    if (Directory.Exists(loaderlogtemppath))
                        Directory.Delete(loaderlogtemppath, true);

                    if (!Directory.Exists(loaderlogtemppath))
                    {
                        Directory.CreateDirectory(loaderlogtemppath);
                    }

                    switch (logtype)
                    {
                        case EnumUploadLogType.LoaderDebug:
                            lopPath = this.LoaderLogParam.LoaderSystemUpDownLoadPath.Value;
                            isFolderExist = FolderExistCheckAndCreate(lopPath, logtype);
                            if (isFolderExist == EventCodeEnum.NONE)
                            {
                                CopyLoaderDebugLog(todaydate);
                            }
                            else
                            {
                                return isFolderExist;
                            }       
                            break;
                        case EnumUploadLogType.LoaderOCR:
                            lopPath = this.LoaderLogParam.LoaderOCRUpDownLoadPath.Value;
                            isFolderExist = FolderExistCheckAndCreate(lopPath, logtype);
                            if (isFolderExist == EventCodeEnum.NONE)
                            {
                                CopyLoaderOCRLogAndImage(todaydate);
                            }
                            else
                            {
                                return isFolderExist;
                            }       
                            break;
                        default:
                            break;
                    }

                    if (!File.Exists(localzippath))
                    {
                        ZipFile.CreateFromDirectory(loaderlogtemppath, localzippath);
                    }

                    if (Directory.Exists(localserverpath))
                        Directory.Delete(localserverpath, true);
                    System.Threading.Thread.Sleep(1000);
                    ZipFile.ExtractToDirectory(localzippath, localserverpath);

                    ret = LoaderLogSplitManager.LoaderLogUploadToServer(localserverpath,
                        lopPath, this.LoaderLogParam.UserName.Value, this.LoaderLogParam.Password.Value,
                        logtype);

                    if (ret == EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule][End] Succeed Upload {logtype} (Loader)");
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule][End] Fail Upload {logtype} (Loader)");
                    }

                    if (File.Exists(localzippath))
                        File.Delete(localzippath);
                    if (Directory.Exists(localserverpath))
                        Directory.Delete(localserverpath, true);
                    if (Directory.Exists(loaderlogtemppath))
                        Directory.Delete(loaderlogtemppath, true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"LoaderLogUploadToServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Debug($"[LoaderLogManagerModule]Fail {logtype} log upload to server");
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public EventCodeEnum CellLogUploadToServer(int stageindex, EnumUploadLogType logtype)
        {
            lock (LogUploadLockObj)
            {
                EventCodeEnum ret = EventCodeEnum.UNDEFINED;
                EventCodeEnum isFolderExist = EventCodeEnum.UNDEFINED;

                // 12시가 넘어가서 오늘 날짜 로그에 유효한 정보가 없을 수 있기 때문에 어제 까지 추가.
                DateTime yesterday = DateTime.Today.AddDays(-1);
                string yesterdayDate = $"{yesterday.Year}-{yesterday.Month.ToString().PadLeft(2, '0')}-{yesterday.Day.ToString().PadLeft(2, '0')}";
                string todaydate = $"{DateTime.Today.Year}-{DateTime.Today.Month.ToString().PadLeft(2, '0')}-{DateTime.Today.Day.ToString().PadLeft(2, '0')}";

                string stageDebugLogPath = "";
                string stagePinLogPath = "";
                string stageTempLogPath = "";
                string stagePMILogPath = "";
                string stageLOTLogPath = "";

                try
                {
                    StageLogParameter stageLog = this.LoaderLogParam.StageLogParams.Where(x => x.StageIndex.Value == stageindex).FirstOrDefault();

                    if (stageLog == null)
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule]CellLogUploadToServer() StageLogParam is null");
                        return ret;
                    }
                    else
                    {
                        if (LoaderLogParam.CanUseStageLogParam.Value == true)
                        {
                            stageDebugLogPath = stageLog.StageSystemUpDownLoadPath.Value;
                            stagePinLogPath = stageLog.StagePinUpDownLoadPath.Value;
                            stageTempLogPath = stageLog.StageTempUpDownLoadPath.Value;
                            stagePMILogPath = stageLog.StagePMIUpDownLoadPath.Value;
                            stageLOTLogPath = stageLog.StageLOTUpDownLoadPath.Value;
                        }
                        else
                        {
                            stageDebugLogPath = LoaderLogParam.StageSystemUpDownLoadPath.Value;
                            stagePinLogPath = LoaderLogParam.StagePinUpDownLoadPath.Value;
                            stageTempLogPath = LoaderLogParam.StageTempUpDownLoadPath.Value;
                            stagePMILogPath = LoaderLogParam.StagePMIUpDownLoadPath.Value;
                            stageLOTLogPath = LoaderLogParam.StageLOTUpDownLoadPath.Value;
                        }
                    }

                    if (LoaderLogParam.UploadEnable.Value == false)
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule]Not Excute Cell Log Upload because UploadEnable is false. Cell Number : {stageindex}, Log Type : {logtype}");
                        return ret;
                    }

                    LoggerManager.Debug($"[LoaderLogManagerModule][Start] Cell{stageindex.ToString().PadLeft(2, '0')} uploading log to server, Upload Log Type : {logtype}");
                    
                    var cell = this.LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stageindex);
                    string cellindex = $"Cell{stageindex.ToString().PadLeft(2, '0')}";

                    byte[] datas = null;

                    string localzippath = @"C:\Logs\LoaderUpload" + "\\" + $"{logtype}" +"\\" + $"Cell{stageindex.ToString().PadLeft(2, '0')}" + ".zip";
                    var localserverpath = @"C:\Logs\LoaderUpload" + "\\" + $"{logtype}" + "\\" + $"Cell{stageindex.ToString().PadLeft(2, '0')}";

                    bool needWrite = false;

                    switch (logtype)
                    {
                        case EnumUploadLogType.Debug:
                            StageDebugDatesFromStage.Clear();
                            StageDebugDatesFromStage.Add(yesterdayDate);    
                            StageDebugDatesFromStage.Add(todaydate);
                            isFolderExist = FolderExistCheckAndCreate(stageDebugLogPath, logtype, stageindex);
                            if (isFolderExist == EventCodeEnum.NONE)
                            {
                                datas = cell.GetLogFromFileName(logtype, StageDebugDatesFromStage);
                                needWrite = true;
                            }
                            else
                            {
                                return isFolderExist;
                            }
                            break;
                        case EnumUploadLogType.Temp:
                            StageTEMPDatesFromStage.Clear();
                            StageTEMPDatesFromStage.Add(yesterdayDate);
                            StageTEMPDatesFromStage.Add(todaydate);
                            isFolderExist = FolderExistCheckAndCreate(stageTempLogPath, logtype, stageindex);
                            if (isFolderExist == EventCodeEnum.NONE)
                            {
                                datas = cell.GetLogFromFileName(logtype, StageTEMPDatesFromStage);
                                needWrite = true;
                            }
                            else
                            {
                                return isFolderExist;
                            }
                            break;
                        case EnumUploadLogType.PMI:
                            StagePMIDatesFromStage.Clear();
                            StagePMIDatesFromStage.Add(yesterdayDate);
                            StagePMIDatesFromStage.Add(todaydate);
                            isFolderExist = FolderExistCheckAndCreate(stagePMILogPath, logtype, stageindex);
                            if (isFolderExist == EventCodeEnum.NONE)
                            {
                                datas = cell.GetLogFromFileName(logtype, StagePMIDatesFromStage);
                                needWrite = true;
                            }
                            else
                            {
                                return isFolderExist;
                            }
                            break;
                        case EnumUploadLogType.PIN:
                            StagePINDatesFromStage.Clear();
                            StagePMIDatesFromStage.Add(yesterdayDate);
                            StagePINDatesFromStage.Add(todaydate);
                            isFolderExist = FolderExistCheckAndCreate(stagePinLogPath, logtype, stageindex);
                            if (isFolderExist == EventCodeEnum.NONE)
                            {
                                datas = cell.GetLogFromFileName(logtype, StagePINDatesFromStage);
                                needWrite = true;
                            }
                            else
                            {
                                return isFolderExist;
                            }
                            break;
                        case EnumUploadLogType.LOT:
                            StageLotDatesFromStage.Clear();
                            StagePMIDatesFromStage.Add(yesterdayDate);
                            StageLotDatesFromStage.Add(todaydate);
                            isFolderExist = FolderExistCheckAndCreate(stageLOTLogPath, logtype, stageindex);
                            if (isFolderExist == EventCodeEnum.NONE)
                            {
                                datas = cell.GetLogFromFileName(logtype, StageLotDatesFromStage);
                                needWrite = true;
                            }
                            else
                            {
                                return isFolderExist;
                            }
                            break;
                        default:
                            break;
                    }

                    if(needWrite)
                    {
                        if (FileWriteAndExtract(localzippath, localserverpath, datas))
                        {
                            switch (logtype)
                            {
                                case EnumUploadLogType.Debug:
                                    ret = LoaderLogSplitManager.CellLogUploadToServer(stageindex, localserverpath, stageDebugLogPath,
                                    LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.Debug);
                                    break;
                                case EnumUploadLogType.Temp:
                                    ret = LoaderLogSplitManager.CellLogUploadToServer(stageindex, localserverpath, stageTempLogPath,
                                    LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.Temp);
                                    break;
                                case EnumUploadLogType.PMI:
                                    ret = LoaderLogSplitManager.CellLogUploadToServer(stageindex, localserverpath, stagePMILogPath,
                                LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.PMI);
                                    break;
                                case EnumUploadLogType.PIN:
                                    ret = LoaderLogSplitManager.CellLogUploadToServer(stageindex, localserverpath, stagePinLogPath,
                                    LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.PIN);
                                    break;
                                case EnumUploadLogType.LOT:
                                    ret = LoaderLogSplitManager.CellLogUploadToServer(stageindex, localserverpath, stageLOTLogPath,
                                    LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.LOT);
                                    break;
                                default:
                                    break;
                            }

                            if (ret == EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"[LoaderLogManagerModule][End] Succeed Upload {logtype} Log (Cell{stageindex.ToString().PadLeft(2, '0')})");
                            }
                            else
                            {
                                LoggerManager.Debug($"[LoaderLogManagerModule][End] Fail Upload {logtype} Log (Cell{stageindex.ToString().PadLeft(2, '0')})");
                            }
                        }
                    }
                            
                    if (File.Exists(localzippath))
                    {
                        File.Delete(localzippath);
                    }

                    if (Directory.Exists(localserverpath))
                    {
                        Directory.Delete(localserverpath, true);
                    }

                    ret = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"CellLogUploadToServer(): Error occurred. Err = {err.Message}");
                    LoggerManager.Debug($"[LoaderLogManagerModule]Fail {logtype} log upload to server");
                    LoggerManager.Exception(err);
                }
                finally
                {
                }
                return ret;
            }
        }
        
        #endregion

        #region Getdates Stage and server
        private EventCodeEnum GetStageDebugDatesFromServer(string cellindex, StageLogParameter stageLog)
        {

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            NetworkCredential credentials = null;
            credentials = new NetworkCredential(LoaderLogParam.UserName.Value,
                           LoaderLogParam.Password.Value);
            StageDebugDatesFromServer.Clear();
            List<string> tmplist = new List<string>();
            string checkpath = null;
            
            if (LoaderLogParam.CanUseStageLogParam.Value == true)
            {
                checkpath = stageLog.StageSystemUpDownLoadPath.Value;
            }
            else
            {
                if (LoaderLogParam.StageSystemUpDownLoadPath.Value[LoaderLogParam.StageSystemUpDownLoadPath.Value.Length - 1] == '/')
                {
                    checkpath = LoaderLogParam.StageSystemUpDownLoadPath.Value + cellindex + '/';
                }
                else
                {
                    checkpath = LoaderLogParam.StageSystemUpDownLoadPath.Value + '/' + cellindex + '/';
                }
            }

            ret = LoaderLogSplitManager.CheckFolderExist(checkpath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
            if (ret == EventCodeEnum.NONE)
            {
            }
            else
            {
                ret = LoaderLogSplitManager.CreateDicrectory(checkpath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
                if (ret != EventCodeEnum.NONE)
                {
                    return ret;
                }
            }
            LoaderLogSplitManager.GetStageDatesFromServer(cellindex, checkpath,
                LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, ref tmplist);

            var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");
            try
            {
                if (tmplist.Count > 0)
                {
                    foreach (var item in tmplist)
                    {
                        foreach (Match m in reg.Matches(item))
                        {
                            StageDebugDatesFromServer.Add(m.Value);
                        }
                    }
                }

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetStageDebugDatesFromServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private EventCodeEnum GetStagePINDatesFromServer(string cellindex, StageLogParameter stageLog)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            NetworkCredential credentials = null;
            credentials = new NetworkCredential(LoaderLogParam.UserName.Value,
                           LoaderLogParam.Password.Value);
            StagePINDatesFromServer.Clear();
            List<string> tmplist = new List<string>();
            string checkpath = null;
            
            if (LoaderLogParam.CanUseStageLogParam.Value == true)
            {
                checkpath = stageLog.StagePinUpDownLoadPath.Value;
            }
            else
            {
                if (LoaderLogParam.StagePinUpDownLoadPath.Value[LoaderLogParam.StagePinUpDownLoadPath.Value.Length - 1] == '/')
                {
                    checkpath = LoaderLogParam.StagePinUpDownLoadPath.Value + cellindex + '/';
                }
                else
                {
                    checkpath = LoaderLogParam.StagePinUpDownLoadPath.Value + '/' + cellindex + '/';
                }
            }
            ret = LoaderLogSplitManager.CheckFolderExist(checkpath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
            if (ret == EventCodeEnum.NONE)
            {
            }
            else
            {
                ret = LoaderLogSplitManager.CreateDicrectory(checkpath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
                if (ret != EventCodeEnum.NONE)
                {
                    return ret;
                }
            }
            LoaderLogSplitManager.GetStageDatesFromServer(cellindex, checkpath,
                LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, ref tmplist);

            var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");

            try
            {
                if (tmplist.Count > 0)
                {
                    foreach (var item in tmplist)
                    {
                        foreach (Match m in reg.Matches(item))
                        {
                            StagePINDatesFromServer.Add(m.Value);
                        }
                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetStagePINDatesFromServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private EventCodeEnum GetStageTEMPDatesFromServer(string cellindex, StageLogParameter stageLog)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            NetworkCredential credentials = null;
            credentials = new NetworkCredential(LoaderLogParam.UserName.Value,
                           LoaderLogParam.Password.Value);
            StageTEMPatesFromServer.Clear();
            List<string> tmplist = new List<string>();
            string checkpath = null;
            
            if (LoaderLogParam.CanUseStageLogParam.Value == true)
            {
                checkpath = stageLog.StageTempUpDownLoadPath.Value;
            }
            else
            {
                if (LoaderLogParam.StageTempUpDownLoadPath.Value[LoaderLogParam.StageTempUpDownLoadPath.Value.Length - 1] == '/')
                {
                    checkpath = LoaderLogParam.StageTempUpDownLoadPath.Value + cellindex + '/';
                }
                else
                {
                    checkpath = LoaderLogParam.StageTempUpDownLoadPath.Value + '/' + cellindex + '/';
                }
            }
            ret = LoaderLogSplitManager.CheckFolderExist(checkpath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
            if (ret == EventCodeEnum.NONE)
            {
            }
            else
            {
                ret = LoaderLogSplitManager.CreateDicrectory(checkpath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
                if (ret != EventCodeEnum.NONE)
                {
                    return ret;
                }
            }
            LoaderLogSplitManager.GetStageDatesFromServer(cellindex, checkpath,
                LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, ref tmplist);
            var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");

            try
            {
                if (tmplist.Count > 0)
                {
                    foreach (var item in tmplist)
                    {
                        foreach (Match m in reg.Matches(item))
                        {
                            StageTEMPatesFromServer.Add(m.Value);
                        }
                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetStageTEMPDatesFromServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private EventCodeEnum GetStagePMIDatesFromServer(string cellindex, StageLogParameter stageLog)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            NetworkCredential credentials = null;
            credentials = new NetworkCredential(LoaderLogParam.UserName.Value,
                           LoaderLogParam.Password.Value);
            StagePMIatesFromServer.Clear();
            List<string> tmplist = new List<string>();
            string checkpath = null;
            
            if(LoaderLogParam.CanUseStageLogParam.Value == true)
            {
                checkpath = stageLog.StagePMIUpDownLoadPath.Value;
            }
            else
            {
                if (LoaderLogParam.StagePMIUpDownLoadPath.Value[LoaderLogParam.StagePMIUpDownLoadPath.Value.Length - 1] == '/')
                {
                    checkpath = LoaderLogParam.StagePMIUpDownLoadPath.Value + cellindex + '/';
                }
                else
                {
                    checkpath = LoaderLogParam.StagePMIUpDownLoadPath.Value + '/' + cellindex + '/';
                }
            }
            

            ret = LoaderLogSplitManager.CheckFolderExist(checkpath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
            if (ret == EventCodeEnum.NONE)
            {
            }
            else
            {
                ret = LoaderLogSplitManager.CreateDicrectory(checkpath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
                if (ret != EventCodeEnum.NONE)
                {
                    return ret;
                }
            }
            LoaderLogSplitManager.GetStageDatesFromServer(cellindex, checkpath,
                LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, ref tmplist);
            var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");

            try
            {
                if (tmplist.Count > 0)
                {
                    foreach (var item in tmplist)
                    {
                        foreach (Match m in reg.Matches(item))
                        {
                            StagePMIatesFromServer.Add(m.Value);
                        }
                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetStagePMIDatesFromServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private EventCodeEnum GetStageLotDatesFromServer(string cellindex, StageLogParameter stageLog)
        {

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            NetworkCredential credentials = null;
            credentials = new NetworkCredential(LoaderLogParam.UserName.Value,
                           LoaderLogParam.Password.Value);
            StageLotDatesFromServer.Clear();
            List<string> tmplist = new List<string>();
            string checkpath = null;
            
            if (LoaderLogParam.CanUseStageLogParam.Value == true)
            {
                checkpath = stageLog.StageLOTUpDownLoadPath.Value;
            }
            else
            {
                if (LoaderLogParam.StageLOTUpDownLoadPath != null)
                {
                    if (LoaderLogParam.StageLOTUpDownLoadPath.Value[LoaderLogParam.StageLOTUpDownLoadPath.Value.Length - 1] == '/')
                    {
                        checkpath = LoaderLogParam.StageLOTUpDownLoadPath.Value + cellindex + '/';
                    }
                    else
                    {
                        checkpath = LoaderLogParam.StageLOTUpDownLoadPath.Value + '/' + cellindex + '/';
                    }
                }
            }
            ret = LoaderLogSplitManager.CheckFolderExist(checkpath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
            if (ret == EventCodeEnum.NONE)
            {
            }
            else
            {
                ret = LoaderLogSplitManager.CreateDicrectory(checkpath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
                if (ret != EventCodeEnum.NONE)
                {
                    return ret;
                }
            }
            LoaderLogSplitManager.GetStageDatesFromServer(cellindex, checkpath,
                LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, ref tmplist);
            var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");

            try
            {
                if (tmplist.Count > 0)
                {
                    foreach (var item in tmplist)
                    {
                        foreach (Match m in reg.Matches(item))
                        {
                            StageLotDatesFromServer.Add(m.Value);
                        }
                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetStageLotDatesFromServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }

        #endregion

        #endregion

        #region Manual transfer stage, loader and Server 
        public EventCodeEnum ManualUploadStageAndLoader()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                //var rootpath = LoaderLogParam.DeviceUpLoadPath.Value;
                //NetworkCredential credentials = new NetworkCredential(LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
                if (LoaderLogParam.UploadEnable.Value == false)
                {
                    this.MetroDialogManager().ShowMessageDialog("Fail", $"Log UploadEnable Parameter is false", enummessagesytel: EnumMessageStyle.Affirmative);
                    return ret;
                }

                this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Log upload");

                StringBuilder stb = new StringBuilder();
                stb.Append("Succeed Upload Stages & Loader Log");
                stb.Append(System.Environment.NewLine);

                ret = ManualStageLogUploadServer();
                if (ret != EventCodeEnum.NONE)
                {
                    //this.WaitCancelDialogService().CloseDialog();
                    this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                    this.MetroDialogManager().ShowMessageDialog("Fail", $"FtpStatusCode : {LoaderLogSplitManager.showErrorMsg}\n", enummessagesytel: EnumMessageStyle.Affirmative);

                    return EventCodeEnum.NODATA;
                }

                ret = ManualLoaderLogUploadServer();

                if (ret != EventCodeEnum.NONE)
                {
                    //this.WaitCancelDialogService().CloseDialog();
                    this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                    this.MetroDialogManager().ShowMessageDialog("Fail", $"FtpStatusCode :\n{LoaderLogSplitManager.showErrorMsg}\n", enummessagesytel: EnumMessageStyle.Affirmative);
                    return EventCodeEnum.NODATA;
                } 
                if (FailedPath.Count != 0)
                {
                    stb.Append("Failed Log");
                    stb.Append(System.Environment.NewLine);
                    for (int i = 0; i < FailedPath.Count; i++)
                    {
                        stb.Append($" - {FailedPathLogType[i].ToString()} : {FailedPath[i]}");
                        stb.Append(System.Environment.NewLine);
                    }
                }
                this.MetroDialogManager().ShowMessageDialog("Done", stb.ToString(), enummessagesytel: EnumMessageStyle.Affirmative);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ManualUploadStageAndLoader(): Error occurred. Err = {err.Message}");
                LoggerManager.Debug($"[LoaderLogManagerModule]Fail log upload to server");
                LoggerManager.Exception(err);
            }
            finally
            {
                FailedPathLogType.Clear();
                FailedPath.Clear();
                //this.WaitCancelDialogService().CloseDialog();
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }

            return ret;
        }
        #endregion

        private string GetConnectPath()
        {
            string connectPath = string.Empty;

            foreach (var prop in LoaderLogParam.GetType().GetProperties())
            {
                if (!prop.Name.Contains("Path"))
                {
                    continue;
                }

                string value = prop.GetValue(LoaderLogParam, null).ToString();

                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                string ftpPattern = @"^ftp://|FTP:// ";
                string networkPattern = @"^\\(\\(\w+\s*)+)";

                if (Regex.IsMatch(value, ftpPattern))
                {
                    string path = value.TrimEnd('/');
                    connectPath = path.Substring(0, path.LastIndexOf('/'));
                }
                else if (Regex.IsMatch(value, networkPattern))
                {
                    string path = value.TrimEnd('\\');
                    connectPath = path.Substring(0, path.LastIndexOf('\\'));
                }
                else
                {
                    // Not support protocol
                }

                break;
            }

            return connectPath;
        }
      
        #region CardChangeImage
        public EventCodeEnum UploadCardPatternImages(byte[] data, string filename, string devicename, string cardid)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            if (GPCardPatternBufferImagePath == null)
                GPCardPatternBufferImagePath = this.FileManager().GetSystemRootPath() + "//CardBufferImg";
            string dirfullpath;

            if (cardid != null)
            {
                dirfullpath = GPCardPatternBufferImagePath + $"\\{cardid}";
            }
            else
            {
                dirfullpath = GPCardPatternBufferImagePath + $"\\{devicename}";
            }
            
            string fullpath = dirfullpath + $"\\{filename}.bmp";
            try
            {
                if (Directory.Exists(GPCardPatternBufferImagePath) == false)
                {
                    Directory.CreateDirectory(GPCardPatternBufferImagePath);
                }
                if (Directory.Exists(dirfullpath) == false)
                {
                    Directory.CreateDirectory(dirfullpath);
                }
                File.WriteAllBytes(fullpath, data);
                LoggerManager.Debug($"[LoaderLogManagerModule]Upload Pattern Image {fullpath}");
                
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public List<CardImageBuffer> DownloadCardPatternImages(string devicename, int downimgcnt, string cardID)
        {

            List<CardImageBuffer> ret = new List<CardImageBuffer>();
            if (GPCardPatternBufferImagePath == null)
                GPCardPatternBufferImagePath = this.FileManager().GetSystemRootPath() + "//CardBufferImg";
            try
            {
                string imgfolder;
                if (cardID != null)
                {
                    imgfolder = GPCardPatternBufferImagePath + "//" + cardID;
                }
                else
                {
                    imgfolder = GPCardPatternBufferImagePath + "//" + devicename;
                }
                
                LoggerManager.Debug($"[LoaderLogManagerModule]Card image download path:{imgfolder}");

                if (Directory.Exists(imgfolder) == true)
                {
                    DirectoryInfo di = new DirectoryInfo(imgfolder);
                    var idxzero = di.GetFiles().Where(x => x.Name.Contains("PT0_")).OrderByDescending(x => x.CreationTime).ToArray();
                    var idxone = di.GetFiles().Where(x => x.Name.Contains("PT1_")).OrderByDescending(x => x.CreationTime).ToArray();
                    var idxtwo = di.GetFiles().Where(x => x.Name.Contains("PT2_")).OrderByDescending(x => x.CreationTime).ToArray();
                    var idxthree = di.GetFiles().Where(x => x.Name.Contains("PT3_")).OrderByDescending(x => x.CreationTime).ToArray();
                    for (int i = 0; i < idxzero.Length; i++)
                    {
                        try
                        {
                            FileInfo fi;
                            fi = new System.IO.FileInfo(idxzero[i].FullName);
                            CardImageBuffer tempdata = new CardImageBuffer();
                            tempdata.ImageByte = File.ReadAllBytes(fi.FullName);
                            tempdata.ImgFileName = fi.Name;
                            ret.Add(tempdata);
                            LoggerManager.Debug($"[LoaderLogManagerModule]Card image {tempdata.ImgFileName} added list in DownloadCardPatternImages ");
                            if (i >= downimgcnt)
                            {
                                break;
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"ZeroIndex Exception occured while DownloadCardPatternImages function {err.Message} ");
                        }
                    }
                    for (int i = 0; i < idxone.Length; i++)
                    {
                        try
                        {
                            FileInfo fi;
                            fi = new System.IO.FileInfo(idxone[i].FullName);
                            CardImageBuffer tempdata = new CardImageBuffer();
                            tempdata.ImageByte = File.ReadAllBytes(fi.FullName);
                            tempdata.ImgFileName = fi.Name;
                            ret.Add(tempdata);
                            LoggerManager.Debug($"[LoaderLogManagerModule]Card image {tempdata.ImgFileName} added list in DownloadCardPatternImages ");
                            if (i >= downimgcnt)
                            {
                                break;
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"TwoIndex Exception occured while DownloadCardPatternImages function {err.Message} ");
                        }
                    }
                    for (int i = 0; i < idxtwo.Length; i++)
                    {
                        try
                        {
                            FileInfo fi;
                            fi = new System.IO.FileInfo(idxtwo[i].FullName);
                            CardImageBuffer tempdata = new CardImageBuffer();
                            tempdata.ImageByte = File.ReadAllBytes(fi.FullName);
                            tempdata.ImgFileName = fi.Name;
                            ret.Add(tempdata);
                            LoggerManager.Debug($"[LoaderLogManagerModule]Card image {tempdata.ImgFileName} added list in DownloadCardPatternImages ");
                            if (i >= downimgcnt)
                            {
                                break;
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"OneIndex Exception occured while DownloadCardPatternImages function {err.Message} ");
                        }
                    }
                    for (int i = 0; i < idxthree.Length; i++)
                    {
                        try
                        {
                            FileInfo fi;
                            fi = new System.IO.FileInfo(idxthree[i].FullName);
                            CardImageBuffer tempdata = new CardImageBuffer();
                            tempdata.ImageByte = File.ReadAllBytes(fi.FullName);
                            tempdata.ImgFileName = fi.Name;
                            ret.Add(tempdata);
                            LoggerManager.Debug($"[LoaderLogManagerModule]Card image {tempdata.ImgFileName} added list in DownloadCardPatternImages ");
                            if (i >= downimgcnt)
                            {
                                break;
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"ThreeIndex Exception occured while DownloadCardPatternImages function {err.Message} ");
                        }
                    }

                    //for (int i = 0; i < filelist.Length; i++)
                    //{
                    //    //condition downimgcnt and date 
                    //    FileInfo fi;
                    //    fi = new System.IO.FileInfo(filelist[i]);

                    //    CardImageBuffer tempdata = new CardImageBuffer();
                    //    tempdata.ImageByte = File.ReadAllBytes(fi.FullName);
                    //    tempdata.ImgFileName = fi.Name;
                    //    ret.Add(tempdata);
                    //    LoggerManager.Debug($"[LoaderLogManagerModule]Card image {tempdata.ImgFileName} added list in DownloadCardPatternImages ");
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Exception occured while DownloadCardPatternImages function {err.Message} ");
            }
            return ret;
        }

        public EventCodeEnum UploadProbeCardInfo(ProberCardListParameter probeCard)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (probeCard != null)
                {
                    int proberCardCnt = LoaderProbeCardListParam.ProbeCardList.Count(x => x.CardID == probeCard.CardID);
                    if (proberCardCnt == 0) // not exist
                    {
                        LoaderProbeCardListParam.ProbeCardList.Add(probeCard);
                    }
                    else // is exist
                    {
                        int i = LoaderProbeCardListParam.ProbeCardList.FindIndex(x => x.CardID == probeCard.CardID);
                        LoaderProbeCardListParam.ProbeCardList[i] = probeCard;
                    }

                    if (File.Exists(ProbeCardListPath) == true)
                    {
                        if (LoaderProbeCardListParam != null)
                        {
                            Extensions_IParam.SaveParameter(null, LoaderProbeCardListParam, null, ProbeCardListPath);
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule]UploadProbeCardInfo() ProbeCardListPath is not exist");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;   
        }

        public ProberCardListParameter DownloadProbeCardInfo(string cardID)
        {
            ProberCardListParameter ret = null;
            try
            {
                ret = LoaderProbeCardListParam.ProbeCardList.FirstOrDefault(x => x.CardID == cardID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        #endregion


        public EventCodeEnum PMIImageUploadStageToLoader(int cellindex, byte[] data, string filename)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string filename_parse = "";
            string fullpath = "";

            try
            {
                if (PMIImagePath == null)
                    PMIImagePath = this.FileManager().GetSystemRootPath() + "//PMIImg"; // Cell로 부터 받은 Image Loader에 저장할 Path

                if (filename.Contains("#"))
                {
                    filename_parse = filename.Replace("#", "");
                }
                else
                {
                    filename_parse = filename;
                }

                fullpath = PMIImagePath + $"\\{filename_parse}.bmp";

                if (!Directory.Exists(PMIImagePath))
                {
                    Directory.CreateDirectory(PMIImagePath);
                }

                File.WriteAllBytes(fullpath, data);

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public Task<EventCodeEnum> PMIImageUploadLoaderToServer(int stageindex)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string stagePMIImagePath = "";

            try
            {
                LoggerManager.Debug($"[LoaderLogManagerModule]LoaderLogManagerModule.PMIImageUploadLoaderToServer() Start upload");
                string PMIImageZipPath = PMIImagePath + ".zip";

                StageLogParameter stageLog = this.LoaderLogParam.StageLogParams.Where(x => x.StageIndex.Value == stageindex).FirstOrDefault();

                if (stageLog == null)
                {
                    LoggerManager.Debug($"[LoaderLogManagerModule]PMIImageUploadLoaderToServer() StageLogParam is null");
                    return Task.FromResult<EventCodeEnum>(ret);
                }
                else
                {
                    if (LoaderLogParam.CanUseStageLogParam.Value == true)
                    {
                        stagePMIImagePath = stageLog.StagePMIImageUploadPath.Value;
                    }
                    else
                    {
                        stagePMIImagePath = LoaderLogParam.StagePMIImageUploadPath.Value;
                    }
                }

                if (!File.Exists(PMIImageZipPath))
                {
                    ZipFile.CreateFromDirectory(PMIImagePath, PMIImageZipPath);
                }
                else
                {
                    File.Delete(PMIImageZipPath);
                    ZipFile.CreateFromDirectory(PMIImagePath, PMIImageZipPath);
                }

                byte[] datas = File.ReadAllBytes(PMIImageZipPath);

                string localzippath = @"C:\Logs\LoaderUpload" + "\\" + $"PMIImage" + "\\" + $"Cell{stageindex.ToString().PadLeft(2, '0')}" + ".zip";
                var localserverpath = @"C:\Logs\LoaderUpload" + "\\" + $"PMIImage" + "\\" + $"Cell{stageindex.ToString().PadLeft(2, '0')}";

                if (FileWriteAndExtract(localzippath, localserverpath, datas))
                {
                    ret = LoaderLogSplitManager.CheckFolderExist(stagePMIImagePath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
                    if (ret != EventCodeEnum.NONE)
                    {
                        ret = LoaderLogSplitManager.CreateDicrectory(stagePMIImagePath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
                        if (ret != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[LoaderLogManagerModule]The path is incorrect : {stagePMIImagePath}");
                        }
                        else
                        {
                            LoggerManager.Debug($"[LoaderLogManagerModule]The path is correct : {stagePMIImagePath}");
                        }
                    }

                    LoaderLogSplitManager.CellLogUploadToServer(stageindex, localserverpath, stagePMIImagePath,LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.PMIImage);

                    LoggerManager.Debug($"[LoaderLogManagerModule]LoaderLogManagerModule.PMIImageUploadLoaderToServer() End upload");
                }

                if (File.Exists(localzippath))
                {
                    File.Delete(localzippath);
                }

                if (File.Exists(PMIImageZipPath))
                {
                    File.Delete(PMIImageZipPath);
                }

                if (Directory.Exists(localserverpath))
                {
                    Directory.Delete(localserverpath, true);
                }

                if (Directory.Exists(PMIImagePath))
                {
                    Directory.Delete(PMIImagePath, true);
                }

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(ret);
        }

        public EventCodeEnum PINImageUploadLoaderToServer(int stageindex, byte[] images)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string stagePINImagePath = "";
            try
            {
                LoggerManager.Debug($"[LoaderLogManagerModule]LoaderLogManagerModule.PINImageUploadLoaderToServer() Start upload, stageIdx : {stageindex}");

                //서버에 원하는 경로에 File Exist, Create하기
                StageLogParameter stageLog = this.LoaderLogParam.StageLogParams.Where(x => x.StageIndex.Value == stageindex).FirstOrDefault();

                if (LoaderLogParam.CanUseStageLogParam.Value == true)
                {
                    stagePINImagePath = stageLog.StagePinTipValidationResultUploadPath.Value;
                }
                else
                {
                    stagePINImagePath = LoaderLogParam.StagePinTipValidationResultUploadPath.Value;
                }

                ret = FolderExistCheckAndCreate(stagePINImagePath, EnumUploadLogType.PINImage, stageindex);

                if (ret!= EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LoaderLogManagerModule]PINImageUploadLoaderToServer() Failed. ret: {ret}");
                    return ret;
                }

                byte[] datas = images;

                string localzippath = @"C:\Logs\LoaderUpload" + "\\" + $"PinImage" + "\\" + $"Cell{stageindex.ToString().PadLeft(2, '0')}" + ".zip";
                var localserverpath = @"C:\Logs\LoaderUpload" + "\\" + $"PinImage" + "\\" + $"Cell{stageindex.ToString().PadLeft(2, '0')}";

                if(FileWriteAndExtract(localzippath, localserverpath, datas))
                {
                    //서버로 올리기
                    string[] folders = Directory.GetDirectories(localserverpath);
                    foreach (var folder in folders)
                    {
                        string final_folder_name = new DirectoryInfo(folder).Name;
                        string destpath;
                        if (LoaderLogParam.CanUseStageLogParam.Value == true)
                        {
                            destpath = stagePINImagePath + '/' + final_folder_name;
                        }
                        else
                        {
                            destpath = stagePINImagePath;
                        }
                        LoaderLogSplitManager.CellLogUploadToServer(stageindex, localserverpath + "\\" + final_folder_name, destpath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value, EnumUploadLogType.PINImage, final_folder_name);
                    }

                    LoggerManager.Debug($"[LoaderLogManagerModule]LoaderLogManagerModule.PINImageUploadLoaderToServer() End upload, stageIdx : {stageindex}");
                }

                if (File.Exists(localzippath))
                {
                    File.Delete(localzippath);
                }

                if (Directory.Exists(localserverpath))
                {
                    Directory.Delete(localserverpath, true);
                }
                
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }


        private EventCodeEnum FolderExistCheckAndCreate(string logPath, EnumUploadLogType logType, int stageIdx = -1)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (string.IsNullOrEmpty(logPath))
                {
                    if(stageIdx != -1)
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule][FolderExistCheckAndCreate]Cell{stageIdx.ToString().PadLeft(2, '0')} dest path: {logPath} Log FolderCheck fail. log type: {logType}");
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule][FolderExistCheckAndCreate]Loader dest path: {logPath} Log FolderCheck fail. log type: {logType}");
                    }
                    return EventCodeEnum.LOGUPLOAD_UPLOAD_PATH_NULL;
                }

                ret = LoaderLogSplitManager.ConnectCheck(logPath,
                        LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
                if (ret == EventCodeEnum.NONE)
                {
                    ret = LoaderLogSplitManager.CheckFolderExist(logPath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
                    if (ret != EventCodeEnum.NONE)
                    {
                        ret = LoaderLogSplitManager.CreateDicrectory(logPath, LoaderLogParam.UserName.Value, LoaderLogParam.Password.Value);
                        if (ret != EventCodeEnum.NONE)
                        {
                            if (stageIdx != -1)
                            {
                                LoggerManager.Debug($"[LoaderLogManagerModule][FolderExistCheckAndCreate]Cell{stageIdx.ToString().PadLeft(2, '0')}, {logType} CreateDicrectory Fail.");
                            }
                            else
                            {
                                LoggerManager.Debug($"[LoaderLogManagerModule][FolderExistCheckAndCreate]Loader, {logType} CreateDicrectory Fail.");
                            }
                            return EventCodeEnum.LOGUPLOAD_CREATE_DIRECTORY_FAIL;
                        }
                    }

                    if (stageIdx != -1)
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule][FolderExistCheckAndCreate]Cell{stageIdx.ToString().PadLeft(2, '0')}, {logType} Log FolderCheck Ok");
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule][FolderExistCheckAndCreate]Loader, {logType} Log FolderCheck Ok");
                    }
                    ret = EventCodeEnum.NONE;
                }
                else if (ret == EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT)
                {
                    if (stageIdx != -1)
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule][FolderExistCheckAndCreate]Cell{stageIdx.ToString().PadLeft(2, '0')}, Login or password incorrect!");
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule][FolderExistCheckAndCreate]Loader, Login or password incorrect!");
                    }
                    
                    ret = EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT;
                }
                else
                {
                    if (stageIdx != -1)
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule][FolderExistCheckAndCreate]Cell{stageIdx.ToString().PadLeft(2, '0')}, Could not connect to server");
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderLogManagerModule][FolderExistCheckAndCreate]Loader, Could not connect to server");
                    }
                    ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
    }
}
