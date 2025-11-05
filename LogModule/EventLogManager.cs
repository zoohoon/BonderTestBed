using ProberErrorCode;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LogModule
{
    public class EventLogManager : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public delegate void OriginEventLogListAddHandler(AlarmLogData AlarmLogData);
        public event OriginEventLogListAddHandler TopbarLogAdd;

        public delegate void OriginEventLogListRemoveHandler(AlarmLogData AlarmLogData);
        public event OriginEventLogListRemoveHandler TopbarLogRemove;

        public delegate void CellOriginEventLogListAddHandler(AlarmLogData alarm);
        public event CellOriginEventLogListAddHandler CellLogAdd;


        public delegate void CellOriginEventLogListRemoveHandler(AlarmLogData alarm);
        public event CellOriginEventLogListRemoveHandler CellLogRemove;

        private ObservableCollection<AlarmLogData> _OriginEventLogList = new ObservableCollection<AlarmLogData>();
        public ObservableCollection<AlarmLogData> OriginEventLogList
        {
            get { return _OriginEventLogList; }
            set
            {
                if (value != _OriginEventLogList)
                {
                    _OriginEventLogList = value;
                    RaisePropertyChanged();
                }
            }
        }
        bool bStopUpdateThread = false;
        DateTime oneDayTime;

        public Thread tGetDataTask = null;
        public void SetOriginEventLogList(int occurEquipment, DateTime errorOccurTime, EventCodeEnum errorCode, string msg = null, EnumProberModule? moduleType = null, string moduleStartTime = "", string imageDatasHashCode = "")
        {
            try
            {
                AlarmLogData log = new AlarmLogData();

                log.ErrorCode = errorCode;
                log.ErrorMessage = msg;
                log.OccurEquipment = occurEquipment;
                log.ErrorOccurTime = errorOccurTime;

                log.ModuleType = (EnumProberModule)moduleType;
                log.ModuleStartTime = moduleStartTime;
                log.ImageDatasHashCode = imageDatasHashCode;

                OriginEventLogList.Add(log);

                if(TopbarLogAdd != null)
                {
                    TopbarLogAdd(log);
                }

                if (log.OccurEquipment != 0)
                {
                    if(CellLogAdd != null)
                    {
                        CellLogAdd(log);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventLogManager()
        {
        }

        public void StartUpdateAlarm()
        {
            try
            {
                tGetDataTask = new Thread(new ThreadStart(UpdateAlarmData));
                tGetDataTask.Name = this.GetType().Name;
                tGetDataTask.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        ~EventLogManager()
        {
            bStopUpdateThread = true;

            if(tGetDataTask != null)
            {
                tGetDataTask.Join();
            }
        }
        public void DeInit()
        {
            bStopUpdateThread = true;

            if (tGetDataTask != null)
            {
                tGetDataTask.Join();
            }
        }
        public void UpdateAlarmData()
        {
            bool bErrHandled = false;

            try
            {
                while (bStopUpdateThread == false)
                {
                    try
                    {
                        oneDayTime = DateTime.Now.AddDays(-1);
                        DeleteHistoryAfterPeriod(oneDayTime);
                        //minskim// GC 호출 및 CPU 사용률 절감을 위해 기존 timer+resetevent로 thread 제어하던 로직을 제거 하고 sleep으로 대체함, sleep시간은 기존 timer interval 주기 값으로 설정함
                        Thread.Sleep(1000);
                        bErrHandled = false;
                    }
                    catch (Exception err)
                    {
                        if(bErrHandled == false)
                        {
                            bErrHandled = true;
                            LoggerManager.Debug($"UpdateAlarmData(): Error occurred. Err = {err.Message}");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        int procLimit = 10;
        public void DeleteHistoryAfterPeriod(DateTime period)
        {
            int procCount = 0;

            try
            {
                for (int i = 0; i < OriginEventLogList.Count; i++)
                {
                    if (OriginEventLogList[i] != null)
                    {
                        if (0 <= period.CompareTo(OriginEventLogList[i].ErrorOccurTime))
                        {
                            AlarmLogData log = new AlarmLogData();

                            log.ErrorCode = OriginEventLogList[i].ErrorCode;
                            log.ErrorMessage = OriginEventLogList[i].ErrorMessage;
                            log.OccurEquipment = OriginEventLogList[i].OccurEquipment;
                            log.ErrorOccurTime = OriginEventLogList[i].ErrorOccurTime;
                            log.IsChecked = OriginEventLogList[i].IsChecked;

                            if (TopbarLogRemove != null)
                            {
                                TopbarLogRemove(log);
                            }

                            if (log.OccurEquipment != 0)
                            {
                                if (CellLogRemove != null)
                                {
                                    CellLogRemove(log);
                                }
                            }

                            OriginEventLogList.Remove(OriginEventLogList[i]);
                            procCount++;
                        }

                        if (procCount > procLimit)
                        {
                            break;
                        }
                        Thread.Sleep(10);
                    }
                    else
                    {
                        // OriginEventLogList[i] 가  null 인 경우 
                        // Array 에서 지울지 생각필요
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        public void WriteEventCodeEnumTable()
        {
            try
            {
                // Get all values from the EventCodeEnum
                var values = Enum.GetValues(typeof(EventCodeEnum));

                // Start building the table
                Console.WriteLine("EventCodeEnum\tValue\tUsed\tNotify\tDescription");
                int index = 1;

                foreach (var value in values)
                {
                    // Get the numeric value of the enum
                    int numericValue = (int)value;

                    // Format the output line for the table
                    string tableLine = $"{index}\t{value}\t0x{numericValue:X8}";

                    // Print the line
                    Console.WriteLine(tableLine);
                    index++;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
