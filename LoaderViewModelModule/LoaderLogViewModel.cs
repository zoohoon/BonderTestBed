using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LoaderViewModelModule
{

    using Autofac;
    using System.Collections.ObjectModel;
    using LogModule;
    using System.Windows.Media;
    using RelayCommandBase;
    using System.Windows.Input;
    using NLog;
    using ProberInterfaces;
    using System.Collections;
    using VirtualKeyboardControl;
    using LoaderBase.Communication;
    using System.Threading;

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

    public class LoaderLogViewModel : INotifyPropertyChanged, IFactoryModule, IModule, ILogVM
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        private ObservableCollection<AlarmLogData> _UnCheckedEventLogList = new ObservableCollection<AlarmLogData>();
        public ObservableCollection<AlarmLogData> UnCheckedEventLogList
        {
            get { return _UnCheckedEventLogList; }
            set
            {
                if (value != _UnCheckedEventLogList)
                {
                    _UnCheckedEventLogList = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        public EventLogManager eventLogManager = LoggerManager.EventLogMg;

        private int _LoaderEventLogUserNotNotifiedCount;
        public int LoaderEventLogUserNotNotifiedCount
        {
            get { return _LoaderEventLogUserNotNotifiedCount; }
            set
            {
                if (value != _LoaderEventLogUserNotNotifiedCount)
                {
                    _LoaderEventLogUserNotNotifiedCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<AlarmLogData> _LoaderLogViewAlarmList = new ObservableCollection<AlarmLogData>();
        public ObservableCollection<AlarmLogData> LoaderLogViewAlarmList
        {
            get { return _LoaderLogViewAlarmList; }
            set
            {
                if (value != _LoaderLogViewAlarmList)
                {
                    _LoaderLogViewAlarmList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AlarmLogData _SelectedAlarmLog;
        public AlarmLogData SelectedAlarmLog
        {
            get { return _SelectedAlarmLog; }
            set
            {
                if (value != _SelectedAlarmLog)
                {
                    _SelectedAlarmLog = value;
                    int selectedIndex = LoaderLogViewAlarmList.IndexOf(_SelectedAlarmLog);
                    if (selectedIndex > -1)
                    {
                        LoaderLogViewAlarmList[selectedIndex].IsChecked = true;
                    }
                    int count = 0;
                    foreach (var item in LoaderLogViewAlarmList)
                    {

                        if (item.IsChecked == false)
                        {
                            count++;
                        }
                    }

                    //알람 카운트
                    LoaderEventLogUserNotNotifiedCount = count;

                    RaisePropertyChanged();
                }
            }
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

        private ObservableCollection<FilterValue> _StageFilter = new ObservableCollection<FilterValue>();
        public ObservableCollection<FilterValue> StageFilter
        {
            get { return _StageFilter; }
            set
            {
                if (value != _StageFilter)
                {
                    _StageFilter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FilterValue _LoaderFilter = new FilterValue();
        public FilterValue LoaderFilter
        {
            get { return _LoaderFilter; }
            set
            {
                if (value != _LoaderFilter)
                {
                    _LoaderFilter = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _FilterSelctedAllFlag;
        public bool FilterSelctedAllFlag
        {
            get { return _FilterSelctedAllFlag; }
            set
            {
                if (value != _FilterSelctedAllFlag)
                {
                    _FilterSelctedAllFlag = value;
                    //SetupFilterSelectedAll();
                    RaisePropertyChanged();
                }
            }
        }

        private bool _SwitchLogReal;
        public bool SwitchLogReal
        {
            get { return _SwitchLogReal; }
            set
            {
                if (value != _SwitchLogReal)
                {
                    _SwitchLogReal = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SearchKeyword = string.Empty;
        public string SearchKeyword
        {
            get { return _SearchKeyword; }
            set
            {
                if (value != _SearchKeyword)
                {
                    _SearchKeyword = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AlarmLogData _SelectedItem;
        public AlarmLogData SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (value != _SelectedItem)
                {
                    _SelectedItem = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void SearchMatched()
        {
            string upper = SearchKeyword.ToUpper();
            try
            {
                if (SearchKeyword.Length > 0)
                {
                    ObservableCollection<AlarmLogData> temp = new ObservableCollection<AlarmLogData>();

                    var filtered = LoaderLogViewAlarmList.Where(
                       alarm => alarm.ErrorMessage.ToUpper().Contains(upper)
                   | alarm.ErrorCode.ToString().ToUpper().Contains(upper)
                   | alarm.OccurEquipment.ToString().ToUpper().Contains(upper));

                    if (filtered != null)
                    {
                        foreach (var item in filtered)
                        {
                            temp.Add(item);
                        }
                    }

                    LoaderLogViewAlarmList.Clear();
                    LoaderLogViewAlarmList.Clear();
                    foreach (var i in temp)
                    {
                        LoaderLogViewAlarmList.Add(i);
                    }
                }
                else
                {
                    LoaderLogViewAlarmList.Clear();
                    foreach (var item in eventLogManager.OriginEventLogList)
                    {
                        LoaderLogViewAlarmList.Add(item);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


        public bool Initialized { get; set; } = false;

        public ILoaderCommunicationManager LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        public IViewModelManager ViewModelManager { get; set; }
        public int EventLogUserNotNotifiedCount { get; set; }

        public void DeInitModule()
        {
        }

        public EventCodeEnum InitModule()
        {
            ViewModelManager = this.ViewModelManager();

            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
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


                    LoaderEventLogUserNotNotifiedCount = 0;
                    PrologCollection = new ObservableCollection<LogEventInfo>();
                    EventlogCollection = new ObservableCollection<LogEventInfo>();
                    eventLogManager.TopbarLogAdd += AddTopbarAlram;
                    eventLogManager.TopbarLogRemove += RemoveTopbarAlram;

                    foreach (var item1 in eventLogManager.OriginEventLogList)
                    {
                        LoaderLogViewAlarmList.Add(item1);
                    }

                    Initialized = true;

                    // Filter Init
                    LoaderFilter.Key = 0;
                    LoaderFilter.Value = true;

                    var stages = LoaderCommunicationManager.GetStages();
                    foreach (var stage in stages)
                    {
                        StageFilter.Add(new FilterValue() { Key = stage.Index });
                    }

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
        public void UpdateLoaderAlarmCount()
        {
            try
            {
                if (LoaderLogViewAlarmList != null && LoaderLogViewAlarmList.Count > 0)
                {
                    int count = 0;
                    foreach (var item in LoaderLogViewAlarmList)
                    {

                        if (item.IsChecked == false)
                        {
                            count++;
                        }
                    }

                    LoaderEventLogUserNotNotifiedCount = count;
                }
            }catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void AddTopbarAlram(AlarmLogData alarmLogData)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    LoaderLogViewAlarmList.Add(alarmLogData);
                    UpdateLoaderAlarmCount();

                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void RemoveTopbarAlram(AlarmLogData alarmLogData)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    for (int i = LoaderLogViewAlarmList.Count - 1; i >= 0; i--)
                    {
                        if (LoaderLogViewAlarmList[i].ErrorOccurTime == alarmLogData.ErrorOccurTime
                    && LoaderLogViewAlarmList[i].ErrorCode == alarmLogData.ErrorCode
                    && LoaderLogViewAlarmList[i].ErrorMessage == alarmLogData.ErrorMessage
                    && LoaderLogViewAlarmList[i].OccurEquipment == alarmLogData.OccurEquipment)
                        {
                            LoaderLogViewAlarmList.RemoveAt(i);

                            UpdateLoaderAlarmCount();
                        }
                    }

                    UpdateLoaderAlarmCount();
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private bool _FilterPopupIsOpen;
        public bool FilterPopupIsOpen
        {
            get { return _FilterPopupIsOpen; }
            set
            {
                if (value != _FilterPopupIsOpen)
                {
                    _FilterPopupIsOpen = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _PickDate;
        public string PickDate
        {
            get { return _PickDate; }
            set
            {
                if (value != _PickDate)
                {
                    _PickDate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime _StartDate = DateTime.Now;
        public DateTime StartDate
        {
            get { return _StartDate; }
            set
            {
                if (value != _StartDate)
                {
                    _StartDate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime _EndDate = DateTime.Now;
        public DateTime EndDate
        {
            get { return _EndDate; }
            set
            {
                if (value != _EndDate)
                {
                    _EndDate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand _PeriodFilterCommand;
        public ICommand PeriodFilterCommand
        {
            get
            {
                if (null == _PeriodFilterCommand) _PeriodFilterCommand = new RelayCommand(PeriodFilterFunc);
                return _PeriodFilterCommand;
            }
        }

        public Thread tGetReadLog = null;

        private void AddEventLogInfo(string[] arrLog)
        {
            string[] temp;
            foreach (var eventLog in arrLog)
            {                
                temp = eventLog.Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries);
                if (temp?.Length > 5)
                {
                    AlarmLogData log = new AlarmLogData();
                    log.ErrorOccurTime = Convert.ToDateTime(temp[0]);
                    log.OccurEquipment = Convert.ToInt32(temp[2]);
                    log.ErrorCode = (EventCodeEnum)Enum.Parse(typeof(EventCodeEnum), temp[3]);
                    log.ErrorMessage = temp[5] ?? "Unknown"; //message가 null인 경우에 대한 예외 처리
                    LoaderLogViewAlarmList.Add(log);
                }
            }
        }

        public void ReadLogs()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    DateTime dayEndDate;
                    DateTime dayStartDate;
                    int diffDay = 0;
                    string endDateCnv = "";
                    string startDateCnv = "";
                    string fileName = "";
                    string[] splitTime;
                    IStageSupervisorProxy proxy;

                    endDateCnv = EndDate.ToString("yyyy-MM-dd");
                    splitTime = endDateCnv.Split('-');
                    dayEndDate = new DateTime(Convert.ToInt32(splitTime[0]), Convert.ToInt32(splitTime[1]), Convert.ToInt32(splitTime[2]));

                    startDateCnv = StartDate.ToString("yyyy-MM-dd");
                    splitTime = startDateCnv.Split('-');
                    dayStartDate = new DateTime(Convert.ToInt32(splitTime[0]), Convert.ToInt32(splitTime[1]), Convert.ToInt32(splitTime[2]));

                    if (dayStartDate.CompareTo(dayEndDate) <= 0)
                    {
                        diffDay = dayEndDate.Day - dayStartDate.Day + 1;

                        LoaderLogViewAlarmList.Clear();
                        for (int i = 0; i < diffDay; i++)
                        {
                            fileName = "Event_" + dayStartDate.AddDays(i).ToString("yyyy-MM-dd");

                            //Stage
                            foreach (var filter in StageFilter)
                            {
                                if (filter.Value == true)
                                {
                                    if (filter.Key != 0)
                                    {
                                        proxy = this.LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(filter.Key);

                                        if (proxy != null)
                                        {                                            
                                            AddEventLogInfo(proxy.LoadStageEventLog(fileName));
                                        }
                                    }
                                }
                            }

                            //Loader
                            if (LoaderFilter.Value == true)
                            {
                                AddEventLogInfo(LoggerManager.LoadEventLog(fileName));
                            }
                        }
                    }

                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void PeriodFilterFunc()
        {
            try
            {
                tGetReadLog = new Thread(new ThreadStart(ReadLogs));
                tGetReadLog.Name = this.GetType().Name;
                tGetReadLog.IsBackground = true;
                tGetReadLog.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _FilterApplyCommand;
        public ICommand FilterApplyCommand
        {
            get
            {
                if (null == _FilterApplyCommand) _FilterApplyCommand = new RelayCommand<object>(FilterApplyCmd);
                return _FilterApplyCommand;
            }
        }

        private void FilterApplyCmd(object obj)
        {
            try
            {
                ObservableCollection<AlarmLogData> temp = new ObservableCollection<AlarmLogData>();

                foreach (var item in StageFilter)
                {
                    if (item.Value == true)
                    {
                        UpdateAlaramList(item.Key);
                    }
                }

                if (LoaderFilter.Value == true)
                {
                    UpdateAlaramList(0);
                }
                else
                {
                    LoaderLogViewAlarmList.Clear();
                }

                void UpdateAlaramList(int index)
                {
                    int orderCount = 0;
                    List<int> OrserCounts = new List<int>();
                    foreach (var i in (LoaderLogViewAlarmList.Where(alarm => alarm.OccurEquipment == index)))
                    {
                        temp.Add(i);
                        orderCount++;
                    }

                    LoaderLogViewAlarmList.Clear();
                    foreach (var i in temp)
                    {
                        LoaderLogViewAlarmList.Add(i);
                    }
                }

                FilterPopupIsOpen = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand _SetupFilterSelectedAllCommand;
        public ICommand SetupFilterSelectedAllCommand
        {
            get
            {
                if (null == _SetupFilterSelectedAllCommand) _SetupFilterSelectedAllCommand = new RelayCommand(SetupFilterSelectedAllFunc);
                return _SetupFilterSelectedAllCommand;
            }
        }
        private void SetupFilterSelectedAllFunc()
        {
            try
            {
                LoaderFilter.Value = FilterSelctedAllFlag;
                foreach (var stage in StageFilter)
                {
                    if (stage.IsConnected == true)
                    {

                        stage.Value = FilterSelctedAllFlag;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void SetupFilterStageInfo()
        {
            try
            {
                var stages = LoaderCommunicationManager.GetStages();
                foreach (var stage in stages)
                {
                    var stgFilterObj = StageFilter.Where(filterObj => filterObj.Key == stage.Index)?.FirstOrDefault();
                    if (stgFilterObj != null)
                    {
                        stgFilterObj.IsConnected = stage.StageInfo.IsConnected;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _FilterPopupOpenCommand;
        public ICommand FilterPopupOpenCommand
        {
            get
            {
                if (null == _FilterPopupOpenCommand) _FilterPopupOpenCommand = new RelayCommand<object>(FilterPopupOpenCommandCmd);
                return _FilterPopupOpenCommand;
            }
        }

        private void FilterPopupOpenCommandCmd(object obj)
        {
            try
            {
                SetupFilterStageInfo();
                FilterPopupIsOpen = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _FilterCloseCommand;
        public ICommand FilterCloseCommand
        {
            get
            {
                if (null == _FilterCloseCommand) _FilterCloseCommand = new RelayCommand<object>(FilterCloseCmd);
                return _FilterCloseCommand;
            }
        }

        private void FilterCloseCmd(object obj)
        {
            try
            {
                FilterPopupIsOpen = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _RefreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                if (null == _RefreshCommand) _RefreshCommand = new RelayCommand<object>(RefreshCmd);
                return _RefreshCommand;
            }
        }

        private void RefreshCmd(object obj)
        {
            try
            {
                LoaderLogViewAlarmList.Clear();

                if (SwitchLogReal == true)
                {
                    foreach (var item in eventLogManager.OriginEventLogList)
                    {
                        LoaderLogViewAlarmList.Add(item);
                    }

                    UpdateLoaderAlarmCount();
                }
                else
                {
                    PeriodFilterFunc();
                }
                SwitchLogReal = !SwitchLogReal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _AllAlramCheckedCommand;
        public ICommand AllAlramCheckedCommand
        {
            get
            {
                if (null == _AllAlramCheckedCommand) _AllAlramCheckedCommand = new RelayCommand(AllAlramCheckedCommandFunc);
                return _AllAlramCheckedCommand;
            }
        }

        private void AllAlramCheckedCommandFunc()
        {
            try
            {
                for (int i = 0; i < LoaderLogViewAlarmList.Count; i++)
                {
                    LoaderLogViewAlarmList[i].IsChecked = true;
                }
                UpdateLoaderAlarmCount();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        #region ==> ParamSearchBoxClickCommand
        private RelayCommand _ParamSearchBoxClickCommand;
        public ICommand ParamSearchBoxClickCommand
        {
            get
            {
                if (null == _ParamSearchBoxClickCommand) _ParamSearchBoxClickCommand = new RelayCommand(ParamSearchBoxClickCommandFunc);
                return _ParamSearchBoxClickCommand;
            }
        }
        private void ParamSearchBoxClickCommandFunc()
        {
            try
            {
                String filterKeyword = VirtualKeyboard.Show(SearchKeyword, KB_TYPE.DECIMAL | KB_TYPE.ALPHABET);

                if (filterKeyword == null)
                {
                    return;
                }


                SearchKeyword = filterKeyword;
                SearchMatched();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion


        private AsyncCommand<object> _SelectedItemChangedCommand;
        public ICommand SelectedItemChangedCommand
        {
            get
            {
                if (null == _SelectedItemChangedCommand) _SelectedItemChangedCommand = new AsyncCommand<object>(SelectedItemChangedCommandFunc);
                return _SelectedItemChangedCommand;
            }
        }
        private IList items = null;
        private Task SelectedItemChangedCommandFunc(object obj)
        {
            try
            {
                AlarmLogData alarmLogData;
                alarmLogData = obj as AlarmLogData;
                items = obj as IList;

                if (items != null)
                {
                    int selectedcount = items.Count;
                    LoaderLogViewAlarmList[selectedcount].IsChecked = true;


                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }


        public EventCodeEnum UpdateNotifiedCount()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                int count = 0;

                foreach (var item in EventlogCollection)
                {
                    if (item.HasProperties == true)
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

                LoaderEventLogUserNotNotifiedCount = count;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void EventLogBuffer_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                LogEventInfo v = new LogEventInfo();

                lock (LoggerManager.EventLogBufferLockObject)
                {
                    var log = LoggerManager.EventLogBuffer.Last();

                    if (log.HasProperties == true)
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

                UpdateNotifiedCount();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void EventMarkAllSet()
        {
            foreach (var item in LoaderLogViewAlarmList)
            {

                if (item.IsChecked == false)
                {
                    item.IsChecked = false;
                }
            }
        }

        public void RefreshLoaderLogAlarmList()
        {
            LoaderLogViewAlarmList.Clear();
            foreach (var item in eventLogManager.OriginEventLogList)
            {
                LoaderLogViewAlarmList.Add(item);
            }

            UpdateLoaderAlarmCount();
        }
    }



    public class FilterValue : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _Key;
        public int Key
        {
            get { return _Key; }
            set
            {
                if (value != _Key)
                {
                    _Key = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Value;
        public bool Value
        {
            get { return _Value; }
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsConnected;
        public bool IsConnected
        {
            get { return _IsConnected; }
            set
            {
                if (value != _IsConnected)
                {
                    _IsConnected = value;
                    RaisePropertyChanged();
                }
            }
        }


    }
}
