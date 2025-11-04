using LogModule;
using NLog;
using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    //public enum ProberLogLevel
    //{
    //    UNDEFINED = 0,
    //    PROLOG,
    //    EVENT,
    //    DEBUG,
    //    FILTEREDDEBUG,
    //}

    //public enum PrologType
    //{
    //    UNDEFINED = 0,
    //    PROLOG_INFORMATION,
    //    PROLOG_OPERATION_ALARM,
    //    PROLOG_SYSTEM_FAULT,
    //}

    //public enum DebuglogType
    //{
    //    UNDEFINED = 0,
    //    DEBUG,
    //}

    //public enum EventlogType
    //{
    //    UNDEFINED = 0,
    //    EVENT,
    //}
    public enum EnumUploadLogType
    {
        Debug = 0,
        Temp = 1,
        PMI = 2,
        PIN = 3,
        LoaderDebug = 4,
        LoaderOCR = 5,
        LOT = 6,
        PMIImage = 7,
        PINImage = 8
    }

    public interface ILoaderLogSplitManager : IModule
    {
        EventCodeEnum ConnectCheck(string path, string username, string password);
        EventCodeEnum CheckFolderExist(string path, string username, string password);
        EventCodeEnum CreateDicrectory(string path, string username, string password);
        EventCodeEnum GetStageDatesFromServer(string cellindex, string path,
            string username, string password, ref List<string> dateslist);
        EventCodeEnum GetLoaderDatesFromServer(string path, string username, string password, ref List<string> dateslist);
        EventCodeEnum GetLoaderOCRDatesFromServer(string path, string username, string password, ref List<string> dateslist);
        EventCodeEnum CellLogUploadToServer(int cellindex, string sourcepath, string destpath, string username, string password, EnumUploadLogType logtype, params string[] subpath);
        EventCodeEnum LoaderLogUploadToServer(string sourcepath, string destpath, string username, string password, EnumUploadLogType logtype);
        EventCodeEnum LoaderDeviceUploadToServer(string localzippath, string destpath, string username, string password);
        EventCodeEnum LoaderDeviceDownloadFromServer(string serverpath, string localpath, string username, string password);
        EventCodeEnum GetFolderListFromServer(string path, string username, string password, ref List<string> folderlist);
        List<string> reasonOferror { get; set; }
        string showErrorMsg { get; set; }
    }
    public enum LogLevel
    {
        UNDEFINED = 0,
        PROLOG_INFORMATION,
        PROLOG_OPERATION_ALARM,
        PROLOG_SYSTEM_FAULT,
        EVENTLOG,
        DEBUGLOG,
    }
    public class LogHistoriesStorage : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private SynchronizedObservableCollection<LogDataStructure> _FilteredLogHistories = new SynchronizedObservableCollection<LogDataStructure>();
        public SynchronizedObservableCollection<LogDataStructure> FilteredLogHistories
        {
            get { return _FilteredLogHistories; }
            set
            {
                if (value != _FilteredLogHistories)
                {
                    _FilteredLogHistories = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SynchronizedObservableCollection<LogDataStructure> _ProLogHistories = new SynchronizedObservableCollection<LogDataStructure>();
        public SynchronizedObservableCollection<LogDataStructure> ProLogHistories
        {
            get { return _ProLogHistories; }
            set
            {
                if (value != _ProLogHistories)
                {
                    _ProLogHistories = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SynchronizedObservableCollection<LogDataStructure> _DebugLogHistories = new SynchronizedObservableCollection<LogDataStructure>();
        public SynchronizedObservableCollection<LogDataStructure> DebugLogHistories
        {
            get { return _DebugLogHistories; }
            set
            {
                if (value != _DebugLogHistories)
                {
                    _DebugLogHistories = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SynchronizedObservableCollection<LogDataStructure> _EventLogHistories = new SynchronizedObservableCollection<LogDataStructure>();
        public SynchronizedObservableCollection<LogDataStructure> EventLogHistories
        {
            get { return _EventLogHistories; }
            set
            {
                if (value != _EventLogHistories)
                {
                    _EventLogHistories = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
    public class LogDataSelectableItem : SelectableItem<LogDataStructure>
    {
        public LogDataSelectableItem(LogDataStructure item)
           : base(item)
        {
        }
    }


    public class LogDataStructure : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public LogDataStructure(LogEventInfo log)
        {
            try
            {
                if ((log.CallerFilePath != null) && (log.CallerMemberName != null))
                {
                    Description = $"{log.CallerFilePath}, {log.CallerMemberName}, {log.CallerLineNumber}";
                }
                else
                {
                    if (log.Properties.ContainsKey(LoggerManager.DescriptionPropertyName))
                    {
                        Description = (string)log.Properties[LoggerManager.DescriptionPropertyName];
                    }
                    else
                    {
                        Description = "UNKNOWN";
                    }
                }

                if(log.Properties.ContainsKey(LoggerManager.LogTypePropertyName))
                {
                    Type = log.Properties[LoggerManager.LogTypePropertyName];
                }
                else
                {
                    Type = LogLevel.UNDEFINED;
                }

                if (log.Properties.ContainsKey(LoggerManager.LogCodePropertyName))
                {
                    Code = (string)log.Properties[LoggerManager.LogCodePropertyName];
                }
                else
                {
                    Code = "UNKNOWN";
                }

                if (log.Properties.ContainsKey(LoggerManager.LogTagPropertyName))
                {
                    Tag = (List<string>)log.Properties[LoggerManager.LogTagPropertyName];
                }
                else
                {
                    Tag = null;
                }

                Time = log.TimeStamp;
                
                Message = log.Message;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private int _HashCode;
        //public int HashCode
        //{
        //    get { return _HashCode; }
        //    set
        //    {
        //        if (value != _HashCode)
        //        {
        //            _HashCode = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        //private bool _UserNotified;
        //public bool UserNotified
        //{
        //    get { return _UserNotified; }
        //    set
        //    {
        //        if (value != _UserNotified)
        //        {
        //            _UserNotified = value; 
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private object _Type;
        public object Type
        {
            get { return _Type; }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime _Time;
        public DateTime Time
        {
            get { return _Time; }
            set
            {
                if (value != _Time)
                {
                    _Time = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Code;
        public string Code
        {
            get { return _Code; }
            set
            {
                if (value != _Code)
                {
                    _Code = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<string> _Tag;
        public List<string> Tag
        {
            get { return _Tag; }
            set
            {
                if (value != _Tag)
                {
                    _Tag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Message;
        public string Message
        {
            get { return _Message; }
            set
            {
                if (value != _Message)
                {
                    _Message = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Description;
        public string Description
        {
            get { return _Description; }
            set
            {
                if (value != _Description)
                {
                    _Description = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
