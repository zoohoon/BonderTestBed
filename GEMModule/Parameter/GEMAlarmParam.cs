using System;
using System.Collections.Generic;

namespace GEMModule
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    public class GEMAlarmParameter : ISystemParameterizable, INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        #endregion

        public string FilePath { get; } = "GEM";
        public string FileName { get; } = "GEMAlarmParam.Json";

        #region ==> IParamNode Implement
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; } = "GemSysAlarmParam";
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
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
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        #endregion

        #region Property
        private ObservableCollection<GemAlarmDiscriptionParam> _GemAlramInfos
             = new ObservableCollection<GemAlarmDiscriptionParam>();
        public ObservableCollection<GemAlarmDiscriptionParam> GemAlramInfos
        {
            get { return _GemAlramInfos; }
            set
            {
                if (value != _GemAlramInfos)
                {
                    _GemAlramInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region ISystemParameterizable

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
                SetDefaultParam();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        private void SEMICS()
        {
            #region <!-- ALCD 5 --> System Error Alarm
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(1001)); //PROBING_SEQUENCE_INVALID_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(13001)); //MARK_ALIGN_FOCUSING_FAILED
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(13051)); //MARK_ALGIN_PATTERN_MATCH_FAILED
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(13101)); //MARK_ALIGN_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(14001)); //WAFER_ALIGN_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15001)); //PIN_ALIGN_FAILED
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(18001)); //CARD_CHANGE_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(23001)); //HEATER_POWER_SUPPLY_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(23101)); //DEW_POINT_HIGH_ERR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(27001)); //CHILLER_REMOTE_NOT_CONNECTED
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(27021)); //CHILLER_ERROR_OCCURRED

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(27061)); //CHILLER_NOT_CONNECTED (chiller #1)
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(27062)); //CHILLER_NOT_CONNECTED (chiller #2)
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(27063)); //CHILLER_NOT_CONNECTED (chiller #3)

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50001)); //MONITORING_MAIN_POWER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50002)); //EMO_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50003)); //EMO_ERROR_FORM_TESTER
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50011)); //STAGE_MAIN_AIR_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50012)); //STAGE_MAIN_VAC_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50013)); //LOADER_MAIN_AIR_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50014)); //LOADER_MAIN_VAC_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50015)); //LOADER_RIGHT_DOOR_OPEN
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50016)); //LOADER_LEFT_DOOR_OPEN
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50019)); //LOADER_PCW_LEAK_ALARM
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50101)); //SYSTEM_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50151)); //SYSTEM_FILE_MISSING
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50201)); //HARD_DISK_FULL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50251)); //DIRECTORY_PATH_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50301)); //INVALID_PARAMETER_FIND
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50351)); //MONITORING_CHUCK_6VAC_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50221)); //MONITORING_MACHINE_INIT_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50451)); //WAFER_NOT_EXIST_EROOR

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60001)); //CASSETTE_LOCK_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60002));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60003));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60004));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60011)); //CASSETTE_UNLOCK_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60012));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60013));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60014));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60021)); //FOUP_OPEN_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60022));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60023));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60024));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60031)); // FOUP_CLOSE_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60032));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60033));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60034));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60041)); //FOUP_LOAD_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60042));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60043));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60044));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60051)); //FOUP_UNLOAD_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60052));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60053));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60054));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60061)); //FOUP_SCAN_FAILED
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60062));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60073));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60074));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(61001)); // LOADER_PA_FAILED
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(61002));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(61003));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(61011)); //LOADER_PA_VAC_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(61012));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(61013));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(61021)); //LOADER_FIND_NOTCH_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(61022));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(61023));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(61041)); //LOADER_PA_WAF_MISSED
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(61042));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(61043));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(62001)); //LOADER_WAFER_MISSED_ON_ARM
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(62002)); 

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(63001)); //LOADER_FIXED_TRAY_WAF_MISSED
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(63002));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(63003));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(63004));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(63005));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(63006));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(63007));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(63008));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(63009));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(64001)); //LOADER_BUFFER_WAF_MISSED
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(64002));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(64003));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(64004));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(64005));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(64006));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65000)); // LOADER_ARM_TO_BUFFER_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65001)); // LOADER_ARM_TO_STAGE_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65002)); // LOADER_ARM_TO_FIXED_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65003)); // LOADER_ARM_TO_INSP_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65004)); // LOADER_ARM_TO_PREALIGN_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65005)); // LOADER_ARM_TO_SLOT_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65006)); // LOADER_BUFFER_TO_ARM_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65007)); // LOADER_CARM_TO_CBUFFER_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65008)); // LOADER_CARM_TO_STAGE_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65009)); // LOADER_CARM_TO_CARDTRAY_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65010)); // LOADER_CBUFFER_TO_CARM_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65011)); // LOADER_STAGE_TO_CARM_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65012)); // LOADER_TRAY_TO_CARM_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65013)); // LOADER_STAGE_TO_ARM_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65014)); // LOADER_FIXED_TO_ARM_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65015)); // LOADER_INSP_TO_ARM_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65016)); // LOADER_PREALIGN_TO_ARM_TRANSFER_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(65017)); // LOADER_SLOT_TO_ARM_TRANSFER_ERROR


            GemAlramInfos.Add(new GemAlarmDiscriptionParam(70001)); //VISION_CAMERA_CHANGE_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(73001)); //MOTION_CONFIG_FILE_LOADING_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(73021)); //MOTION_HOMING_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(73041)); //MOTION_MOTIONDONE_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(73061)); //MOTION_NEG_SW_LIMIT_ERROR ,MOTION_POS_SW_LIMIT_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(73081)); //MONITORING_THREELEG_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(73101)); //PROBING_Z_LIMIT_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(73141)); //GP_CardChange_CARD_MAINVAC_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(73161)); //GP_CardChange_TOP_PLATE_AND_TESTER_VAC_OFF_ERROR

            #endregion

            #region <!-- ALCD 6 -->  Warning Alarm
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(1031)); //CARD_CONTACT_LIMIT
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(1201)); //STAGE_ERROR_OCCUR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(8001)); //PMI_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(9001)); //DEVICE_CHANGE_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(14101)); //WAFER_EDGE_NOT_FOUND
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(14151)); //WAFER_INDEX_ALIGN_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(14201)); //WAFER_LOW_PATTERN_NOT_FOUND, WAFER_HIGH_PATTERN_NOT_FOUND, WAFER_INDEX_ALIGN_PATTERN_NOT_FOUND
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(14251)); //WAFER_LOW_FOCUSING_FAIL, WAFER_HIGH_FOCUSING_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(14301)); //VERIFYALIGN_OVERFLOW_LIMIT
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(14351)); //WAFER_ALIGN_THETA_COMPENSATION_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15101)); //PIN_TEACH_FAILED
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15151)); //PIN_PAD_MATCH_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15201)); //PIN_ALIGN_VERIFY_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15251)); //PIN_FOCUS_FAILED
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15301)); //PIN_LOW_PATTERN_FAILED
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15401)); //PIN_NOT_ENOUGH
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15451)); //PIN_FIND_CENTER_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15501)); //PIN_EXEED_POS_TOLERANCE
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15551)); //PIN_EXEED_MINMAX_TOLERANCE
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15601)); //PIN_CANCEL_PINALIGNMENT
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15651)); //PIN_FAIL_PERCENT_TOLERANCE
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15701)); //PIN_GROUP_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15751)); //PIN_CENTER_POSITION_EXCEED_TOLERANCE
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15801)); //PIN_BLOB_POSITION_EXCEED_TOLERANC
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(15851)); //PIN_TIP_SIZE_VALIDATION_FAIL

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(27041)); //CHILLER_WARNING_OCCURRED

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60101)); //FOUP_SCAN_NOTDETECT
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60102)); 
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60103)); 
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60104)); 

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60111)); //CASSETTE_NOT_READY
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60112));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60113));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60114));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60141)); //OCR_READ_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60142));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60143));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60151)); //OCR_CHECKSUM_FAIL 
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60152));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60153));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60161)); //OCR_RETRY_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60162));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60163));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60171)); //OCR_AUTO_CALIBRATION_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60172));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60173));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60181)); //OCR_READ_FAIL_MANUAL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60182));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60183));

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60191)); //OCR_RESULT_ALREADY_PROBED_WAFERID
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60192));
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60193));


            GemAlramInfos.Add(new GemAlarmDiscriptionParam(17001)); //SOAKING_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(19001)); //POLISHWAFER_CAN_NOT_PERFORMED
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(19051)); //POLISHWAFER_FOCUSING_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(19101)); //POLISHWAFER_GET_NEXT_POSITION_FAIL
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(19151)); //POLISHWAFER_CLEAING_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(19201)); //POLISHWAFER_CENTERING_ERROR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(23151)); //TEMPARATURE_OUT_RANGE_HOT
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(23201)); //TEMPARATURE_IN_RANGE_HOT
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(23251)); //TEMPARATURE_OUT_RANGE_COLD
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(23301)); //TEMPARATURE_IN_RANGE_COLD
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50017)); //LOADER_ERROR_OCCUR
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(73181)); //MOTION_THREE_POD_LOAD_UNBALANCE_ERROR

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60601)); //E84_LOAD_PORT_ACCESS_VIOLATION
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60602)); //E84_LOAD_PORT_ACCESS_VIOLATION
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60603)); //E84_LOAD_PORT_ACCESS_VIOLATION
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(60604)); //E84_LOAD_PORT_ACCESS_VIOLATION
            #endregion

            #region <!-- ALCD 7 --> Attention
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(1)); //LOT_END
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(21)); //LOT_START
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(1051)); //STOP_BEFORE_PROBING
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(1071)); //STOP_AFTER_PROBING
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(1091)); //PIN_PAD_MATCHED_TEST_TIMEOUT

            GemAlramInfos.Add(new GemAlarmDiscriptionParam(1111)); // Lot Pause time out (cell)
            GemAlramInfos.Add(new GemAlarmDiscriptionParam(50018)); // Lot Pause time out (loader)
            #endregion

        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                    
                SEMICS();
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
            return;
        }
        #endregion
    }

    public class GemAlarmDiscriptionParam : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        #endregion

        private long _AlaramID;
        public long AlaramID
        {
            get { return _AlaramID; }
            set
            {
                if (value != _AlaramID)
                {
                    _AlaramID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _RaiseOnlyLot = false;
        /// <summary>
        /// true : Lot 가 도는 중에서 Alarm 보냄, flase: 모든 상황에 다 보냄
        /// </summary>
        public bool RaiseOnlyLot
        {
            get { return _RaiseOnlyLot; }
            set
            {
                if (value != _RaiseOnlyLot)
                {
                    _RaiseOnlyLot = value;
                    RaisePropertyChanged();
                }
            }
        }


        public GemAlarmDiscriptionParam()
        {

        }


        public GemAlarmDiscriptionParam(long alarmId)
        {
            this.AlaramID = alarmId;
        }

        public GemAlarmDiscriptionParam(long alarmId, bool raiseLotEnable)
        {
            AlaramID = alarmId;
            RaiseOnlyLot = raiseLotEnable;
        }
    }
   
  
}
