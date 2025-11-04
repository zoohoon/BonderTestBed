using ProberInterfaces;
using System;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberErrorCode;

namespace ViewModelModule
{
    using System.Collections.ObjectModel;
    using LogModule;
    using System.Windows.Media;
    using RelayCommandBase;
    using System.Windows.Input;
    using NLog;

    public class IconHeader : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public IconHeader(string name, Brush labelfg, Brush rectfill, Brush pathfill, string pathdata, ProberLogLevel level)
        {
            try
            {
                this.Name = name;
                this.LabelForeground = labelfg;
                this.RectangleFill = rectfill;
                this.PathFill = pathfill;
                this._PathData = pathdata;
                this.Level = level;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _LabelForeground;
        public Brush LabelForeground
        {
            get { return _LabelForeground; }
            set
            {
                if (value != _LabelForeground)
                {
                    _LabelForeground = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _RectangleFill;
        public Brush RectangleFill
        {
            get { return _RectangleFill; }
            set
            {
                if (value != _RectangleFill)
                {
                    _RectangleFill = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _PathFill;
        public Brush PathFill
        {
            get { return _PathFill; }
            set
            {
                if (value != _PathFill)
                {
                    _PathFill = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _PathData;
        public string PathData
        {
            get { return _PathData; }
            set
            {
                if (value != _PathData)
                {
                    _PathData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProberLogLevel _Level;
        public ProberLogLevel Level
        {
            get { return _Level; }
            set
            {
                if (value != _Level)
                {
                    _Level = value;
                    RaisePropertyChanged();
                }
            }
        }

    }
    public class LogVM : ILogVM, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<LogEventInfo> _PrologCollection;
        public ObservableCollection<LogEventInfo> PrologCollection
        {
            get { return _PrologCollection; }
            set
            {
                if (value != _PrologCollection)
                {
                    _PrologCollection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<LogEventInfo> _EventlogCollection;
        public ObservableCollection<LogEventInfo> EventlogCollection
        {
            get { return _EventlogCollection; }
            set
            {
                if (value != _EventlogCollection)
                {
                    _EventlogCollection = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private object _EventLoglock = new object();

        public bool Initialized { get; set; } = false;

        public void DeInitModule()
        {

        }

        private IconHeader _PrologHeader;
        public IconHeader PrologHeader
        {
            get { return _PrologHeader; }
            set
            {
                if (value != _PrologHeader)
                {
                    _PrologHeader = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IconHeader _DebuglogHeader;
        public IconHeader DebuglogHeader
        {
            get { return _DebuglogHeader; }
            set
            {
                if (value != _DebuglogHeader)
                {
                    _DebuglogHeader = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IconHeader _EventlogHeader;
        public IconHeader EventlogHeader
        {
            get { return _EventlogHeader; }
            set
            {
                if (value != _EventlogHeader)
                {
                    _EventlogHeader = value;
                    RaisePropertyChanged();
                }
            }
        }


        private LogHistoriesStorage _LogHistories;
        public LogHistoriesStorage LogHistories
        {
            get { return _LogHistories; }
            set
            {
                if (value != _LogHistories)
                {
                    _LogHistories = value;
                    RaisePropertyChanged();
                }
            }
        }


        //private bool _MadeHostoryForProlog;
        //public bool MadeHistoryForProlog
        //{
        //    get { return _MadeHostoryForProlog; }
        //    set
        //    {
        //        if (value != _MadeHostoryForProlog)
        //        {
        //            _MadeHostoryForProlog = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private bool _MadeHostoryForDebuglog;
        //public bool MadeHistoryForDebuglog
        //{
        //    get { return _MadeHostoryForDebuglog; }
        //    set
        //    {
        //        if (value != _MadeHostoryForDebuglog)
        //        {
        //            _MadeHostoryForDebuglog = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private bool _MadeHostoryForEventlog;
        //public bool MadeHistoryForEventlog
        //{
        //    get { return _MadeHostoryForEventlog; }
        //    set
        //    {
        //        if (value != _MadeHostoryForEventlog)
        //        {
        //            _MadeHostoryForEventlog = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //public CollectionViewSource ProLogList { get; set; }

        //private SynchronizedObservableCollection<LogEventInfo> _ProLogHistories;
        //public SynchronizedObservableCollection<LogEventInfo> ProLogHistories
        //{
        //    get { return _ProLogHistories; }
        //    set
        //    {
        //        if (value != _ProLogHistories)
        //        {
        //            _ProLogHistories = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private SynchronizedObservableCollection<LogEventInfo> _DebugLogHistories;
        //public SynchronizedObservableCollection<LogEventInfo> DebugLogHistories
        //{
        //    get { return _DebugLogHistories; }
        //    set
        //    {
        //        if (value != _DebugLogHistories)
        //        {
        //            _DebugLogHistories = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private SynchronizedObservableCollection<LogEventInfo> _EventLogHistories;
        //public SynchronizedObservableCollection<LogEventInfo> EventLogHistories
        //{
        //    get { return _EventLogHistories; }
        //    set
        //    {
        //        if (value != _EventLogHistories)
        //        {
        //            _EventLogHistories = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private int _EventLogUserNotNotifiedCount;
        public int EventLogUserNotNotifiedCount
        {
            get { return _EventLogUserNotNotifiedCount; }
            set
            {
                if (value != _EventLogUserNotNotifiedCount)
                {
                    _EventLogUserNotNotifiedCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand _LogTestCommand;
        public ICommand LogTestCommand
        {
            get
            {
                if (null == _LogTestCommand) _LogTestCommand = new RelayCommand(LogTestCmd);
                return _LogTestCommand;
            }
        }
        private void LogTestCmd()
        {
            try
            {
                PrologHeader = new IconHeader("Probe Log",
                            new SolidColorBrush(Colors.White),
                            new SolidColorBrush(Colors.Transparent),
                            new SolidColorBrush(Colors.White),
                            "M22.5,0h-21C0.7,0,0,0.7,0,1.5v20C0,22.3,0.7,23,1.5,23h21c0.8,0,1.5-0.7,1.5-1.5v-20C24,0.7,23.3,0,22.5,0z M20.5,18h-8.1c-0.3,1.4-1.6,2.2-2.9,2c-1-0.2-1.8-1-2-2h-4C3.3,18,3,17.8,3,17.5S3.3,17,3.5,17h4c0.3-1.4,1.6-2.2,2.9-2 c1,0.2,1.8,1,2,2h8.1c0.3,0,0.5,0.2,0.5,0.5C21,17.8,20.8,18,20.5,18L20.5,18z M20.5,12h-10c-0.3,1.4-1.6,2.2-2.9,2 c-1-0.2-1.8-1-2-2h-2c-0.3,0-0.5-0.2-0.5-0.5S3.3,11,3.6,11h2c0.3-1.4,1.6-2.2,2.9-2c1,0.2,1.8,1,2,2h10.1c0.3,0,0.5,0.2,0.5,0.5C21,11.8,20.8,12,20.5,12L20.5,12z M20.5,6h-6.1c-0.3,1.4-1.6,2.2-2.9,2c-1-0.2-1.8-1-2-2h-6C3.3,6,3,5.8,3,5.5S3.3,5,3.5,5h6c0.3-1.4,1.6-2.2,2.9-2c1,0.2,1.8,1,2,2h6.1C20.8,5,21,5.2,21,5.5C21,5.8,20.8,6,20.5,6L20.5,6z",
                            ProberLogLevel.PROLOG
                            );

                EventlogHeader = new IconHeader("Event Log",
                            new SolidColorBrush(Colors.White),
                            new SolidColorBrush(Colors.Transparent),
                            new SolidColorBrush(Colors.White),
                            "M22.5,0h-21C0.7,0,0,0.7,0,1.5v20C0,22.3,0.7,23,1.5,23h21c0.8,0,1.5-0.7,1.5-1.5v-20C24,0.7,23.3,0,22.5,0z M20.5,18h-8.1c-0.3,1.4-1.6,2.2-2.9,2c-1-0.2-1.8-1-2-2h-4C3.3,18,3,17.8,3,17.5S3.3,17,3.5,17h4c0.3-1.4,1.6-2.2,2.9-2 c1,0.2,1.8,1,2,2h8.1c0.3,0,0.5,0.2,0.5,0.5C21,17.8,20.8,18,20.5,18L20.5,18z M20.5,12h-10c-0.3,1.4-1.6,2.2-2.9,2 c-1-0.2-1.8-1-2-2h-2c-0.3,0-0.5-0.2-0.5-0.5S3.3,11,3.6,11h2c0.3-1.4,1.6-2.2,2.9-2c1,0.2,1.8,1,2,2h10.1c0.3,0,0.5,0.2,0.5,0.5C21,11.8,20.8,12,20.5,12L20.5,12z M20.5,6h-6.1c-0.3,1.4-1.6,2.2-2.9,2c-1-0.2-1.8-1-2-2h-6C3.3,6,3,5.8,3,5.5S3.3,5,3.5,5h6c0.3-1.4,1.6-2.2,2.9-2c1,0.2,1.8,1,2,2h6.1C20.8,5,21,5.2,21,5.5C21,5.8,20.8,6,20.5,6L20.5,6z",
                            ProberLogLevel.EVENT
                            );

                DebuglogHeader = new IconHeader("Debug Log",
                            new SolidColorBrush(Colors.White),
                            new SolidColorBrush(Colors.Transparent),
                            new SolidColorBrush(Colors.White),
                            "M22.5,0h-21C0.7,0,0,0.7,0,1.5v20C0,22.3,0.7,23,1.5,23h21c0.8,0,1.5-0.7,1.5-1.5v-20C24,0.7,23.3,0,22.5,0z M20.5,18h-8.1c-0.3,1.4-1.6,2.2-2.9,2c-1-0.2-1.8-1-2-2h-4C3.3,18,3,17.8,3,17.5S3.3,17,3.5,17h4c0.3-1.4,1.6-2.2,2.9-2 c1,0.2,1.8,1,2,2h8.1c0.3,0,0.5,0.2,0.5,0.5C21,17.8,20.8,18,20.5,18L20.5,18z M20.5,12h-10c-0.3,1.4-1.6,2.2-2.9,2 c-1-0.2-1.8-1-2-2h-2c-0.3,0-0.5-0.2-0.5-0.5S3.3,11,3.6,11h2c0.3-1.4,1.6-2.2,2.9-2c1,0.2,1.8,1,2,2h10.1c0.3,0,0.5,0.2,0.5,0.5C21,11.8,20.8,12,20.5,12L20.5,12z M20.5,6h-6.1c-0.3,1.4-1.6,2.2-2.9,2c-1-0.2-1.8-1-2-2h-6C3.3,6,3,5.8,3,5.5S3.3,5,3.5,5h6c0.3-1.4,1.6-2.2,2.9-2c1,0.2,1.8,1,2,2h6.1C20.8,5,21,5.2,21,5.5C21,5.8,20.8,6,20.5,6L20.5,6z",
                            ProberLogLevel.DEBUG
                            );

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    //IconHeader(string name, Brush labelfg, Brush rectfill, Brush pathfill, string pathdata)

                    PrologHeader = new IconHeader("Probe Log",
                        new SolidColorBrush(Colors.LimeGreen),
                        new SolidColorBrush(Colors.LimeGreen),
                        new SolidColorBrush(Colors.LimeGreen),
                        "M22.5,0h-21C0.7,0,0,0.7,0,1.5v20C0,22.3,0.7,23,1.5,23h21c0.8,0,1.5-0.7,1.5-1.5v-20C24,0.7,23.3,0,22.5,0z M20.5,18h-8.1c-0.3,1.4-1.6,2.2-2.9,2c-1-0.2-1.8-1-2-2h-4C3.3,18,3,17.8,3,17.5S3.3,17,3.5,17h4c0.3-1.4,1.6-2.2,2.9-2 c1,0.2,1.8,1,2,2h8.1c0.3,0,0.5,0.2,0.5,0.5C21,17.8,20.8,18,20.5,18L20.5,18z M20.5,12h-10c-0.3,1.4-1.6,2.2-2.9,2 c-1-0.2-1.8-1-2-2h-2c-0.3,0-0.5-0.2-0.5-0.5S3.3,11,3.6,11h2c0.3-1.4,1.6-2.2,2.9-2c1,0.2,1.8,1,2,2h10.1c0.3,0,0.5,0.2,0.5,0.5C21,11.8,20.8,12,20.5,12L20.5,12z M20.5,6h-6.1c-0.3,1.4-1.6,2.2-2.9,2c-1-0.2-1.8-1-2-2h-6C3.3,6,3,5.8,3,5.5S3.3,5,3.5,5h6c0.3-1.4,1.6-2.2,2.9-2c1,0.2,1.8,1,2,2h6.1C20.8,5,21,5.2,21,5.5C21,5.8,20.8,6,20.5,6L20.5,6z",
                        ProberLogLevel.PROLOG
                        );

                    DebuglogHeader = new IconHeader("Debug Log",
                        new SolidColorBrush(Colors.LimeGreen),
                        new SolidColorBrush(Colors.LimeGreen),
                        new SolidColorBrush(Colors.LimeGreen),
                        "M22.5,0h-21C0.7,0,0,0.7,0,1.5v20C0,22.3,0.7,23,1.5,23h21c0.8,0,1.5-0.7,1.5-1.5v-20C24,0.7,23.3,0,22.5,0z M20.5,18h-8.1c-0.3,1.4-1.6,2.2-2.9,2c-1-0.2-1.8-1-2-2h-4C3.3,18,3,17.8,3,17.5S3.3,17,3.5,17h4c0.3-1.4,1.6-2.2,2.9-2 c1,0.2,1.8,1,2,2h8.1c0.3,0,0.5,0.2,0.5,0.5C21,17.8,20.8,18,20.5,18L20.5,18z M20.5,12h-10c-0.3,1.4-1.6,2.2-2.9,2 c-1-0.2-1.8-1-2-2h-2c-0.3,0-0.5-0.2-0.5-0.5S3.3,11,3.6,11h2c0.3-1.4,1.6-2.2,2.9-2c1,0.2,1.8,1,2,2h10.1c0.3,0,0.5,0.2,0.5,0.5C21,11.8,20.8,12,20.5,12L20.5,12z M20.5,6h-6.1c-0.3,1.4-1.6,2.2-2.9,2c-1-0.2-1.8-1-2-2h-6C3.3,6,3,5.8,3,5.5S3.3,5,3.5,5h6c0.3-1.4,1.6-2.2,2.9-2c1,0.2,1.8,1,2,2h6.1C20.8,5,21,5.2,21,5.5C21,5.8,20.8,6,20.5,6L20.5,6z",
                        ProberLogLevel.DEBUG
                        );

                    EventlogHeader = new IconHeader("Event Log",
                        new SolidColorBrush(Colors.LimeGreen),
                        new SolidColorBrush(Colors.LimeGreen),
                        new SolidColorBrush(Colors.LimeGreen),
                        "M22.5,0h-21C0.7,0,0,0.7,0,1.5v20C0,22.3,0.7,23,1.5,23h21c0.8,0,1.5-0.7,1.5-1.5v-20C24,0.7,23.3,0,22.5,0z M20.5,18h-8.1c-0.3,1.4-1.6,2.2-2.9,2c-1-0.2-1.8-1-2-2h-4C3.3,18,3,17.8,3,17.5S3.3,17,3.5,17h4c0.3-1.4,1.6-2.2,2.9-2 c1,0.2,1.8,1,2,2h8.1c0.3,0,0.5,0.2,0.5,0.5C21,17.8,20.8,18,20.5,18L20.5,18z M20.5,12h-10c-0.3,1.4-1.6,2.2-2.9,2 c-1-0.2-1.8-1-2-2h-2c-0.3,0-0.5-0.2-0.5-0.5S3.3,11,3.6,11h2c0.3-1.4,1.6-2.2,2.9-2c1,0.2,1.8,1,2,2h10.1c0.3,0,0.5,0.2,0.5,0.5C21,11.8,20.8,12,20.5,12L20.5,12z M20.5,6h-6.1c-0.3,1.4-1.6,2.2-2.9,2c-1-0.2-1.8-1-2-2h-6C3.3,6,3,5.8,3,5.5S3.3,5,3.5,5h6c0.3-1.4,1.6-2.2,2.9-2c1,0.2,1.8,1,2,2h6.1C20.8,5,21,5.2,21,5.5C21,5.8,20.8,6,20.5,6L20.5,6z",
                        ProberLogLevel.EVENT
                        );

                    //_ProLogHistories = new SynchronizedObservableCollection<LogEventInfo>();
                    //_DebugLogHistories = new SynchronizedObservableCollection<LogEventInfo>();
                    //_EventLogHistories = new SynchronizedObservableCollection<LogEventInfo>();
                    //_LogHistories = new LogHistoriesStorage();

                    //MadeHistoryForProlog = false;
                    //MadeHistoryForEventlog = false;
                    //MadeHistoryForDebuglog = false;

                    //ProLogList = new CollectionViewSource();
                    //ProLogList.Source = _ProLogHistories;

                    //// Specify a sorting criteria for a particular column
                    //ProLogList.SortDescriptions.Add(new SortDescription("TimeStamp", ListSortDirection.Ascending));

                    //// Let the UI control refresh in order for changes to take place.
                    //ProLogList.View.Refresh();

                    //IList<LogEventInfo> prologhist = _ProLogHistories as IList<LogEventInfo>;

                    //ProLogList = CollectionViewSource.GetDefaultView(prologhist);

                    EventLogUserNotNotifiedCount = 0;

                    //retval = LogFlush();

                    //if (LoggerManager.ProLogRule != null)
                    //{
                    //    LoggerManager.ProLogRule.LoggerMemoryTarget.EventReceived += Prolog_Target_EventReceived;
                    //}

                    //if (LoggerManager.DebugLogRule != null)
                    //{
                    //    LoggerManager.DebugLogRule.LoggerMemoryTarget.EventReceived += Debuglog_Target_EventReceived;
                    //}

                    //if (LoggerManager.EventLogRule != null)
                    //{
                    //    LoggerManager.EventLogRule.LoggerMemoryTarget.EventReceived += Eventlog_Target_EventReceived;
                    //}

                    //if (EventLogHistories is INotifyCollectionChanged)
                    //{
                    //    var coll = EventLogHistories as INotifyCollectionChanged;

                    //    coll.CollectionChanged += LogEventInfo_CollectionChanged;
                    //}

                    //BindingOperations.EnableCollectionSynchronization(EventLogHistories, _EventLoglock);

                    PrologCollection = new ObservableCollection<LogEventInfo>();
                    EventlogCollection = new ObservableCollection<LogEventInfo>();

                    LoggerManager.ProLogBuffer.CollectionChanged += ProLogBuffer_CollectionChanged;
                    LoggerManager.EventLogBuffer.CollectionChanged += EventLogBuffer_CollectionChanged;

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void ProLogBuffer_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                LogEventInfo v = new LogEventInfo();

                lock (LoggerManager.ProLogBufferLockObject)
                {
                    var log = LoggerManager.ProLogBuffer.Last();

                    if (log.HasProperties == true)
                    {
                        v.Properties[LoggerManager.LogidentifierPropertyName] = log.Properties[LoggerManager.LogidentifierPropertyName];
                        v.Properties[LoggerManager.LogTypePropertyName] = log.Properties[LoggerManager.LogTypePropertyName];
                        v.Properties[LoggerManager.DescriptionPropertyName] = log.Properties[LoggerManager.DescriptionPropertyName];
                        v.Properties[LoggerManager.LogCodePropertyName] = log.Properties[LoggerManager.LogCodePropertyName];
                        v.Properties[LoggerManager.LogTagPropertyName] = log.Properties[LoggerManager.LogTagPropertyName];
                    }

                    v.TimeStamp = log.TimeStamp;
                    v.Message = log.Message;
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (PrologCollection.Count == LoggerManager.LogMaximumCount)
                    {
                        PrologCollection.RemoveAt(0);
                    }

                    PrologCollection.Add(v);
                });

                //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                //    if (PrologCollection.Count == LoggerManager.LogMaximumCount)
                //    {
                //        PrologCollection.RemoveAt(0);
                //    }

                //    PrologCollection.Add(v);
                //}));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void EventMarkAllSet()
        {
            foreach (var item in EventlogCollection)
            {
                if (item.HasProperties == true)
                {
                    item.Properties[LoggerManager.NotifiedPropertyName] = true;
                }
            }

            EventLogUserNotNotifiedCount = 0;
        }

        private void EventLogBuffer_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                LogEventInfo v = new LogEventInfo();

                lock (LoggerManager.EventLogBufferLockObject)
                {
                    var log = LoggerManager.EventLogBuffer.Last();

                    if(log.HasProperties == true)
                    {
                        v.Properties[LoggerManager.LogTypePropertyName] = log.Properties[LoggerManager.LogTypePropertyName];
                        v.Properties[LoggerManager.NotifiedPropertyName] = log.Properties[LoggerManager.NotifiedPropertyName];
                    }

                    v.TimeStamp = log.TimeStamp;
                    v.Message = log.Message;
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (EventlogCollection.Count == LoggerManager.LogMaximumCount)
                    {
                        EventlogCollection.RemoveAt(0);
                    }

                    EventlogCollection.Add(v);
                });

                //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                //    if (EventlogCollection.Count == LoggerManager.LogMaximumCount)
                //    {
                //        EventlogCollection.RemoveAt(0);
                //    }

                //    EventlogCollection.Add(v);
                //}));

                UpdateNotifiedCount();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //[MethodImpl(MethodImplOptions.Synchronized)]
        public EventCodeEnum UpdateNotifiedCount()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                int count = 0;

                foreach (var item in EventlogCollection)
                {
                    if(item.HasProperties == true)
                    {
                        var prop = item.Properties[LoggerManager.NotifiedPropertyName];

                        if (prop is bool)
                        {
                            bool flag = (bool)prop;

                            if (flag == false)
                            {
                                count++;
                            }
                        }
                    }
                }

                EventLogUserNotNotifiedCount = count;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void RefreshLoaderLogAlarmList()
        {
            return;
        }
    }
}
