namespace LoaderMaster
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class LoaderLotSysParameter : INotifyPropertyChanged, ISystemParameterizable
    {
        #region <remarks> PropertyChanged </remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> ISystemParameterizable Property </remarks>
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; } = "LOT";

        public string FileName { get; } = "LotSysParameter.json";
        public string Genealogy { get; set; }

        [NonSerialized]
        private Object _Owner;
        [JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        [JsonIgnore]
        public List<object> Nodes { get; set; }
        #endregion

        #region <remarks> Property </remarks>

        private CassetteLockModeEnum _CassetteLockMode = CassetteLockModeEnum.NORMAL;
        /// <summary>
        /// NORMAL : Cassette Load 시에 lock 하고 Load 한다.
        /// ATTACH : Cassette 를 Foup에 올렸을때 Presece 감지되면 lock 을 한다.
        /// LEFTOHT : OHT 가 떠나면 (E84사용) Lock 을 한다
        /// </summary>
        public CassetteLockModeEnum CassetteLockMode
        {
            get { return _CassetteLockMode; }
            set
            {
                if (value != _CassetteLockMode)
                {
                    _CassetteLockMode = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _IsCassetteAutoLock;
        /// <summary>
        /// true : Cassette 를 Foup에 올렸을때 Presece 감지되면 lock 을 한다.
        /// false : Cassette Load 시에 lock 하고 Load 한다.
        /// </summary>
        public bool IsCassetteAutoLock
        {
            get { return _IsCassetteAutoLock; }
            set
            {
                if (value != _IsCassetteAutoLock)
                {
                    _IsCassetteAutoLock = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCassetteAutoLockLeftOHT;
        /// <summary>
        /// true : OHT 가 떠난 후, Lock 을 한다.
        /// false : Cassette Load 시에 lock 하고 Load 한다.
        /// </summary>
        public bool IsCassetteAutoLockLeftOHT
        {
            get { return _IsCassetteAutoLockLeftOHT; }
            set
            {
                if (value != _IsCassetteAutoLockLeftOHT)
                {
                    _IsCassetteAutoLockLeftOHT = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _IsCassetteAutoUnloadAfterLot;
        /// <summary>
        /// Cassette 의 Lot 가 끝났을때 자동으로 Unload 까지할지 말지에 대한 설정.
        /// cf) Micron : true, YMTC : false
        /// </summary>
        public bool IsCassetteAutoUnloadAfterLot
        {
            get { return _IsCassetteAutoUnloadAfterLot; }
            set
            {
                if (value != _IsCassetteAutoUnloadAfterLot)
                {
                    _IsCassetteAutoUnloadAfterLot = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCancelCarrierEventNotRuning;
        /// <summary>
        /// Maseter 가 Running 상태가 아닐때 에도 Cancel Carreri 시 Gem Event 를 전송 할지 안할지.
        /// cf) Micron : true, YMTC : false
        /// </summary>
        public bool IsCancelCarrierEventNotRuning
        {
            get { return _IsCancelCarrierEventNotRuning; }
            set
            {
                if (value != _IsCancelCarrierEventNotRuning)
                {
                    _IsCancelCarrierEventNotRuning = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsWaitForWaferIdConfirm = false;
        public bool IsWaitForWaferIdConfirm
        {
            get { return _IsWaitForWaferIdConfirm; }
            set
            {
                if (value != _IsWaitForWaferIdConfirm)
                {
                    _IsWaitForWaferIdConfirm = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _WaferIdConfirmTimeout_msec = 60000;
        public int WaferIdConfirmTimeout_msec
        {
            get { return _WaferIdConfirmTimeout_msec; }
            set
            {
                if (value != _WaferIdConfirmTimeout_msec)
                {
                    _WaferIdConfirmTimeout_msec = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsCassetteDetectEventAfterRFID = true;
        /// <summary>
        /// Carrier Placed Event 를 언제 보낼지
        /// true : RFID 를 읽은 뒤
        /// false: 카세트 감지되자마자 
        /// cf) Micron : true, YMTC : false
        /// </summary>
        public bool IsCassetteDetectEventAfterRFID
        {
            get { return _IsCassetteDetectEventAfterRFID; }
            set
            {
                if (value != _IsCassetteDetectEventAfterRFID)
                {
                    _IsCassetteDetectEventAfterRFID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<LoaderBase.FoupShiftModeEnum> _FoupShiftMode = new Element<LoaderBase.FoupShiftModeEnum>() { Value = LoaderBase.FoupShiftModeEnum.NORMAL, UpperLimit = 1, LowerLimit = 0 };
        public Element<LoaderBase.FoupShiftModeEnum> FoupShiftMode
        {
            get { return _FoupShiftMode; }
            set
            {
                if (value != _FoupShiftMode)
                {
                    _FoupShiftMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _LoaderLotEndBuzzer = false;
        /// <summary>
        /// Loafer LoT 가 종료되었을 경우 LoaderBuzzer를 울릴지 설정하는 파라미터
        /// true : LoaderBuzzer ON
        /// false: LoaderBuzzer OFF
        /// cf) STM : true
        /// </summary>
        public bool LoaderLotEndBuzzerON
        {
            get { return _LoaderLotEndBuzzer; }
            set
            {
                if (value != _LoaderLotEndBuzzer)
                {
                    _LoaderLotEndBuzzer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _AlwaysCloseFoupCover = false;
        /// <summary>
        /// Foup cover 를 항상 닫고 사용하고 싶은 경우 설정하는 파라미터
        /// true: always close foup cover
        /// false: open foup cover
        /// cf) Micron (MMJ): true 
        /// </summary>
        public bool AlwaysCloseFoupCover
        {
            get { return _AlwaysCloseFoupCover; }
            set
            {
                if (value != _AlwaysCloseFoupCover)
                {
                    _AlwaysCloseFoupCover = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _LotPauseTimeoutAlarm
             = new Element<int>();
        /// <summary>
        /// LOT 가 Pause 된 후에 설정 Timeout 시간이 초과되었는데도 Resume 이 안되고 계속 Pause 상태가 유지될 시에 Alarm 을 발생
        /// Unit : Sec
        /// </summary>
        public Element<int> LotPauseTimeoutAlarm
        {
            get { return _LotPauseTimeoutAlarm; }
            set
            {
                if (value != _LotPauseTimeoutAlarm)
                {
                    _LotPauseTimeoutAlarm = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ExecutionTimeoutError = new Element<int>() { Value = 5 };
        /// <summary>
        /// Loader 가 Transfer 동작 중에 문제가 생겨서 동작을 완료 하지 못할 경우 Timeout 시간을 지정해 해당 시간이 초과되었는데도 동작을 안하면 Loader Time Out 발생 ( Host 나 Tester 의 응답이 안와도 동일함)
        /// Unit : Min
        /// </summary>
        public Element<int> ExecutionTimeoutError
        {
            get { return _ExecutionTimeoutError; }
            set
            {
                if (value != _ExecutionTimeoutError)
                {
                    _ExecutionTimeoutError = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DynamicModeEnum _DynamicMode;
        public DynamicModeEnum DynamicMode
        {
            get { return _DynamicMode; }
            set
            {
                if (value != _DynamicMode)
                {

                    _DynamicMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region <remarks> ISystemParameterizable Method </remarks>
        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
       

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetElementMetaData()
        {

        }
        #endregion
    }

}
