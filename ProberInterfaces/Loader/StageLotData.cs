using LogModule;
using ProberInterfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace LoaderParameters
{
    [Serializable]
    [DataContract]
    public class StageLotData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _LotID;

        [DataMember]
        public string LotID
        {
            get { return _LotID; }
            set { _LotID = value; RaisePropertyChanged(); }
        }


        private GPCellModeEnum _CellMode;
        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public GPCellModeEnum CellMode
        {
            get { return _CellMode; }
            set { _CellMode = value; RaisePropertyChanged(); }
        }

        private String _ConnectState;
        /// <summary>
        /// DeviceName 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String ConnectState
        {
            get { return _ConnectState; }
            set { _ConnectState = value; RaisePropertyChanged(); }
        }


        private String _DeviceName;
        /// <summary>
        /// DeviceName 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String DeviceName
        {
            get { return _DeviceName; }
            set { _DeviceName = value; RaisePropertyChanged(); }
        }


        private String _ProbeCardID;
        /// <summary>
        /// ProbeCardID 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String ProbeCardID
        {
            get { return _ProbeCardID; }
            set { _ProbeCardID = value; RaisePropertyChanged(); }
        }


        private String _WaferID;
        /// <summary>
        /// WaferID 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String WaferID
        {
            get { return _WaferID; }
            set { _WaferID = value; RaisePropertyChanged(); }
        }


        private String _WaferCount;
        /// <summary>
        /// _WaferCount 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String WaferCount
        {
            get { return _WaferCount; }
            set { _WaferCount = value; RaisePropertyChanged(); }
        }

        //private DateTime _WaferLoadingTime;
        ///// <summary>
        ///// 
        ///// </summary>
        //[DataMember]
        //public DateTime WaferLoadingTime
        //{
        //    get { return _WaferLoadingTime; }
        //    set { _WaferLoadingTime = value; RaisePropertyChanged(); }
        //}

        private string _WaferLoadingTime;
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string WaferLoadingTime
        {
            get { return _WaferLoadingTime; }
            set { _WaferLoadingTime = value; RaisePropertyChanged(); }
        }

        private bool _LoadingTimeEnable;
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public bool LoadingTimeEnable
        {
            get { return _LoadingTimeEnable; }
            set { _LoadingTimeEnable = value; RaisePropertyChanged(); }
        }

        private String _CurTemp;
        /// <summary>
        /// CurTemp 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String CurTemp
        {
            get { return _CurTemp; }
            set { _CurTemp = value; RaisePropertyChanged(); }
        }
        private String _SetTemp;
        /// <summary>
        /// SetTemp 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String SetTemp
        {
            get { return _SetTemp; }
            set { _SetTemp = value; RaisePropertyChanged(); }
        }

        private double _TargetTemp;
        /// <summary>
        /// TargetTemp 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public double TargetTemp
        {
            get { return _TargetTemp; }
            set { _TargetTemp = value; RaisePropertyChanged(); }
        }

        private String _Deviation;
        [DataMember]
        public String Deviation
        {
            get { return _Deviation; }
            set { _Deviation = value; RaisePropertyChanged(); }
        }

        private String _FoupNumber;
        /// <summary>
        /// FoupNumber 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String FoupNumber
        {
            get { return _FoupNumber; }
            set { _FoupNumber = value; RaisePropertyChanged(); }
        }

        private String _SlotNumber;
        /// <summary>
        /// SlotNumber 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String SlotNumber
        {
            get { return _SlotNumber; }
            set { _SlotNumber = value; RaisePropertyChanged(); }
        }

        private String _LotState;
        /// <summary>
        /// LotState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String LotState
        {
            get { return _LotState; }
            set { _LotState = value; RaisePropertyChanged(); }
        }


        private String _PinAlignState;
        /// <summary>
        /// PinAlignState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String PinAlignState
        {
            get { return _PinAlignState; }
            set { _PinAlignState = value; RaisePropertyChanged(); }
        }


        private String _WaferAlignState;
        /// <summary>
        /// WaferAlignState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String WaferAlignState
        {
            get { return _WaferAlignState; }
            set { _WaferAlignState = value; RaisePropertyChanged(); }
        }

        private String _PadCount;
        /// <summary>
        /// WaferAlignState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String PadCount
        {
            get { return _PadCount; }
            set { _PadCount = value; RaisePropertyChanged(); }
        }

        private String _MarkAlignState;
        /// <summary>
        /// MarkAlignState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String MarkAlignState
        {
            get { return _MarkAlignState; }
            set { _MarkAlignState = value; RaisePropertyChanged(); }
        }

        private String _ProbingState;
        /// <summary>
        /// ProbingState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String ProbingState
        {
            get { return _ProbingState; }
            set { _ProbingState = value; RaisePropertyChanged(); }
        }
        private String _SoakingState;
        /// <summary>
        /// SoakingState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String SoakingState
        {
            get { return _SoakingState; }
            set { _SoakingState = value; RaisePropertyChanged(); }
        }


        private String _ProbingOD;
        /// <summary>
        /// ProbingOD 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String ProbingOD
        {
            get { return _ProbingOD; }
            set { _ProbingOD = value; RaisePropertyChanged(); }
        }

        private String _SoakingType;
        /// <summary>
        /// SoakingTime 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String SoakingType
        {
            get { return _SoakingType; }
            set { _SoakingType = value; RaisePropertyChanged(); }
        }

        private String _SoakingRemainTime;
        /// <summary>
        /// SoakingRemainTime 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String SoakingRemainTime
        {
            get { return _SoakingRemainTime; }
            set { _SoakingRemainTime = value; RaisePropertyChanged(); }
        }

        private String _SoakingZClearance;
        /// <summary>
        /// SoakingZClearance 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String SoakingZClearance
        {
            get { return _SoakingZClearance; }
            set { _SoakingZClearance = value; RaisePropertyChanged(); }
        }

        private String _TempState;
        /// <summary>
        /// SoakingTime 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String TempState
        {
            get { return _TempState; }
            set { _TempState = value; RaisePropertyChanged(); }
        }

        private bool _StopSoakBtnEnable = true;
        [DataMember]
        public bool StopSoakBtnEnable
        {
            get { return _StopSoakBtnEnable; }
            set { _StopSoakBtnEnable = value; RaisePropertyChanged(); }
        }

        //private DateTime _LotStartTime;
        ///// <summary>
        ///// LotStartTime 를 가져오거나 설정합니다.
        ///// </summary>
        //[DataMember]
        //public DateTime LotStartTime
        //{
        //    get { return _LotStartTime; }
        //    set { _LotStartTime = value; RaisePropertyChanged(); }
        //}

        private string _LotStartTime;
        /// <summary>
        /// LotStartTime 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public string LotStartTime
        {
            get { return _LotStartTime; }
            set { _LotStartTime = value; RaisePropertyChanged(); }
        }

        private bool _LotStartTimeEnable;
        /// <summary>
        /// LotStartTime 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public bool LotStartTimeEnable
        {
            get { return _LotStartTimeEnable; }
            set { _LotStartTimeEnable = value; RaisePropertyChanged(); }
        }

        //private DateTime _LotEndTime;
        ///// <summary>
        ///// LotStartTime 를 가져오거나 설정합니다.
        ///// </summary>
        //[DataMember]
        //public DateTime LotEndTime
        //{
        //    get { return _LotEndTime; }
        //    set { _LotEndTime = value; RaisePropertyChanged(); }
        //}

        private string _LotEndTime;
        /// <summary>
        /// LotStartTime 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public string LotEndTime
        {
            get { return _LotEndTime; }
            set { _LotEndTime = value; RaisePropertyChanged(); }
        }

        private bool _LotEndTimeEnable;
        /// <summary>
        /// LotStartTime 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public bool LotEndTimeEnable
        {
            get { return _LotEndTimeEnable; }
            set { _LotEndTimeEnable = value; RaisePropertyChanged(); }
        }

        private DateTime _RenewTime;
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public DateTime RenewTime
        {
            get { return _RenewTime; }
            set { _RenewTime = value; RaisePropertyChanged(); }
        }
        private String _StageMoveState;
        /// <summary>
        /// StageMoveState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String StageMoveState
        {
            get { return _StageMoveState; }
            set { _StageMoveState = value; RaisePropertyChanged(); }
        }

        private String _Clearance;
        /// <summary>
        /// ProbeCardID 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public String Clearance
        {
            get { return _Clearance; }
            set { _Clearance = value; RaisePropertyChanged(); }
        }

        //private double _ProcessedWaferCountUntilBeforeCardChange;
        ///// <summary>
        ///// 
        ///// </summary>
        //[DataMember]
        //public double ProcessedWaferCountUntilBeforeCardChange
        //{
        //    get { return _ProcessedWaferCountUntilBeforeCardChange; }
        //    set { _ProcessedWaferCountUntilBeforeCardChange = value; RaisePropertyChanged(); }
        //}

        private string _ProcessedWaferCountUntilBeforeCardChange;
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string ProcessedWaferCountUntilBeforeCardChange
        {
            get { return _ProcessedWaferCountUntilBeforeCardChange; }
            set { _ProcessedWaferCountUntilBeforeCardChange = value; RaisePropertyChanged(); }
        }

        private double _MarkedWaferCountLastPolishWaferCleaning;
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public double MarkedWaferCountLastPolishWaferCleaning
        {
            get { return _MarkedWaferCountLastPolishWaferCleaning; }
            set { _MarkedWaferCountLastPolishWaferCleaning = value; RaisePropertyChanged(); }
        }

        //private double _TouchDownCountUntilBeforeCardChange;
        ///// <summary>
        ///// 
        ///// </summary>
        //[DataMember]
        //public double TouchDownCountUntilBeforeCardChange
        //{
        //    get { return _TouchDownCountUntilBeforeCardChange; }
        //    set { _TouchDownCountUntilBeforeCardChange = value; RaisePropertyChanged(); }
        //}

        private string _TouchDownCountUntilBeforeCardChange;
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string TouchDownCountUntilBeforeCardChange
        {
            get { return _TouchDownCountUntilBeforeCardChange; }
            set { _TouchDownCountUntilBeforeCardChange = value; RaisePropertyChanged(); }
        }

        private double _MarkedTouchDownCountLastPolishWaferCleaning;
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public double MarkedTouchDownCountLastPolishWaferCleaning
        {
            get { return _MarkedTouchDownCountLastPolishWaferCleaning; }
            set { _MarkedTouchDownCountLastPolishWaferCleaning = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        private LotModeEnum _lotMode;
        public LotModeEnum LotMode
        {
            get { return _lotMode; }
            set { _lotMode = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 사용자가 Lot를 abort 했는지 확인
        /// </summary>
        [DataMember]
        private bool _lotAbortedByUser;
        public bool LotAbortedByUser
        {
            get { return _lotAbortedByUser; }
            set { _lotAbortedByUser = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<StageLotDataComponent> _DataCollection = new ObservableCollection<StageLotDataComponent>();
        public ObservableCollection<StageLotDataComponent> DataCollection
        {
            get { return _DataCollection; }
            set
            {
                if (value != _DataCollection)
                {
                    _DataCollection = value;
                    RaisePropertyChanged();
                }
            }
        }
        public void DataCollect()
        {
            // TODO : 추후, 레시피 기반의 Collection 제작 형태..?
            Action AddDataCollect = () =>
            {
                try
                {
                    if (DataCollection == null)
                    {
                        DataCollection = new ObservableCollection<StageLotDataComponent>();
                    }

                    DataCollection.Clear();

                    var attribute = ProberInterfaces.Utility.EnumExtensions.GetDescription(this.LotMode);
                    AddDataCollection(StageLotDataEnum.LOTMODE, $"Lot Mode : {attribute.ToString()}");
                    AddDataCollection(StageLotDataEnum.WAFERLOADINGTIME, $"Loading Time : {this.WaferLoadingTime}");
                    AddDataCollection(StageLotDataEnum.FOUPNUMBER, $"Foup Number  : {this.FoupNumber}");
                    AddDataCollection(StageLotDataEnum.SLOTNUMBER, $"Slot Number  : {this.SlotNumber}");
                    AddDataCollection(StageLotDataEnum.WAFERCOUNT, $"Wafer Count (THIS LOT)          : {this.WaferCount}");
                    AddDataCollection(StageLotDataEnum.PROCESSEDWAFERCOUNTUNTILBEFORECARDCHANGE, $"Wafer Count (AFTER CARD CHANGE) : {this.ProcessedWaferCountUntilBeforeCardChange}");
                    AddDataCollection(StageLotDataEnum.TOUCHDOWNCOUNTUNTILBEFORECARDCHANGE, $"Touchdown Count (AFTER CARD CHANGE) : {this.TouchDownCountUntilBeforeCardChange}");
                    //AddDataCollection($"Current Temp      : {this.CurTemp}");
                    AddDataCollection(StageLotDataEnum.SETTEMP, $"Set Temp          : {this.SetTemp}");
                    AddDataCollection(StageLotDataEnum.DEVIATION, $"Deviation Temp    : {this.Deviation}");
                    AddDataCollection(StageLotDataEnum.LOTSTATE, $"LOT State         : {this.LotState}");
                    AddDataCollection(StageLotDataEnum.WAFERALIGNSTATE, $"Wafer Align       : {this.WaferAlignState}");
                    AddDataCollection(StageLotDataEnum.PADCOUNT, $"Pad count         : {this.PadCount}");
                    AddDataCollection(StageLotDataEnum.PINALIGNSTATE, $"Pin Align         : {this.PinAlignState}");
                    AddDataCollection(StageLotDataEnum.MARKALIGNSTATE, $"Mark Align        : {this.MarkAlignState}");
                    AddDataCollection(StageLotDataEnum.PROBINGSTATE, $"Probing State     : {this.ProbingState}");
                    AddDataCollection(StageLotDataEnum.PROBINGOD, $"Overdrive         : {this.ProbingOD}");
                    AddDataCollection(StageLotDataEnum.CLEARANCE, $"Clearance         : {this.Clearance}");
                    AddDataCollection(StageLotDataEnum.TEMPSTATE, $"Temperature State : {this.TempState}");
                    AddDataCollection(StageLotDataEnum.STAGEMOVESTATE, $"StageMove State   : {this.StageMoveState}");
                    AddDataCollection(StageLotDataEnum.LOTSTARTTIME, $"LOT Start Time    : {this.LotStartTime}");
                    AddDataCollection(StageLotDataEnum.LOTENDTIME, $"LOT End Time      : {this.LotEndTime}");

                    RaisePropertyChanged("DataCollection");
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            };

            try
            {
                if (System.Windows.Application.Current.Dispatcher.CheckAccess())
                {
                    AddDataCollect();
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        AddDataCollect();
                    }));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void AddDataCollection(StageLotDataEnum type, string val)
        {
            try
            {
                DataCollection.Add(new StageLotDataComponent(type, val));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
    }
}
