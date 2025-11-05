using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Communication.E84;
using ProberInterfaces.E84;
using System;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace E84
{
    public class E84Simulator : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int TimeOutInterval = 500;

        public readonly object _lock = new object();

        private E84SignalInput _E84Inputs = new E84SignalInput(false);
        public E84SignalInput E84Inputs
        {
            get { return _E84Inputs; }
            set
            {
                if (value != _E84Inputs)
                {
                    _E84Inputs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84SignalOutput _E84Outputs = new E84SignalOutput(false);
        public E84SignalOutput E84Outputs
        {
            get { return _E84Outputs; }
            set
            {
                if (value != _E84Outputs)
                {
                    _E84Outputs = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 1 ~ 16, E84 controller Ethernet Id
        /// </summary>
        private int _nNet_id;
        public int nNet_id
        {
            get { return _nNet_id; }
            set
            {
                if (value != _nNet_id)
                {
                    _nNet_id = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Mode;
        public int Mode
        {
            get { return _Mode; }
            set
            {
                if (value != _Mode)
                {
                    _Mode = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Carrier exist status of LP0
        /// ‘0’ : Use Host Command
        /// ‘1’ : Use physically connected sensor
        /// </summary>
        private int _nAux_in0;
        public int nAux_in0
        {
            get { return _nAux_in0; }
            set
            {
                if (value != _nAux_in0)
                {
                    _nAux_in0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Carrier exist status of LP1
        /// ‘0’ : Use Host Command
        /// ‘1’ : Use physically connected sensor
        /// </summary>
        private int _nAux_in1;
        public int nAux_in1
        {
            get { return _nAux_in1; }
            set
            {
                if (value != _nAux_in1)
                {
                    _nAux_in1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Auxiliary Light Curtain signal
        /// ‘0’ : Not used
        /// ‘1’ : Use as Auxiliary Light Curtain signal
        /// </summary>
        private int _nAux_in2;
        public int nAux_in2
        {
            get { return _nAux_in2; }
            set
            {
                if (value != _nAux_in2)
                {
                    _nAux_in2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Light Curtain signal
        /// ‘0’ : Not used
        /// ‘1’ : Use as Light Curtain Signal
        /// </summary>
        private int _nAux_in3;
        public int nAux_in3
        {
            get { return _nAux_in3; }
            set
            {
                if (value != _nAux_in3)
                {
                    _nAux_in3 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Clamp status of LP0
        /// ‘0’ : Use Host Command
        /// ‘1’ : Use physically connected sensor
        /// </summary>
        private int _nAux_in4;
        public int nAux_in4
        {
            get { return _nAux_in4; }
            set
            {
                if (value != _nAux_in4)
                {
                    _nAux_in4 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Clamp status of LP1 
        /// ‘0’ : Use Host Command
        /// ‘1’ : Use physically connected sensor
        /// </summary>
        private int _nAux_in5;
        public int nAux_in5
        {
            get { return _nAux_in5; }
            set
            {
                if (value != _nAux_in5)
                {
                    _nAux_in5 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Aux_in0_Val;
        public int Aux_in0_Val
        {
            get { return _Aux_in0_Val; }
            set
            {
                if (value != _Aux_in0_Val)
                {
                    _Aux_in0_Val = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Aux_in1_Val;
        public int Aux_in1_Val
        {
            get { return _Aux_in1_Val; }
            set
            {
                if (value != _Aux_in1_Val)
                {
                    _Aux_in1_Val = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Aux_in2_Val;
        public int Aux_in2_Val
        {
            get { return _Aux_in2_Val; }
            set
            {
                if (value != _Aux_in2_Val)
                {
                    _Aux_in2_Val = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Aux_in3_Val;
        public int Aux_in3_Val
        {
            get { return _Aux_in3_Val; }
            set
            {
                if (value != _Aux_in3_Val)
                {
                    _Aux_in3_Val = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Aux_in4_Val;
        public int Aux_in4_Val
        {
            get { return _Aux_in4_Val; }
            set
            {
                if (value != _Aux_in4_Val)
                {
                    _Aux_in4_Val = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Aux_in5_Val;
        public int Aux_in5_Val
        {
            get { return _Aux_in5_Val; }
            set
            {
                if (value != _Aux_in5_Val)
                {
                    _Aux_in5_Val = value;
                    RaisePropertyChanged();
                }
            }
        }
        /// <summary>
        /// Value of AUX Output 0 
        /// ‘0’ : Not used
        /// ‘1’ : Use as General Purpose Output
        /// </summary>
        private int _nAux_out;
        public int nAux_out
        {
            get { return _nAux_out; }
            set
            {
                if (value != _nAux_out)
                {
                    _nAux_out = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Aux_out0_Val;
        public int Aux_out0_Val
        {
            get { return _Aux_out0_Val; }
            set
            {
                if (value != _Aux_out0_Val)
                {
                    _Aux_out0_Val = value;
                    RaisePropertyChanged();
                }
            }
        }

        // Reverse_Signal
        // In case of False : Sensor in/out -> input/output
        // In case of True  : Sensor on -> signal off, Sensor off -> signal on

        private int _nAux_in0_reverse;
        public int nAux_in0_reverse
        {
            get { return _nAux_in0_reverse; }
            set
            {
                if (value != _nAux_in0_reverse)
                {
                    _nAux_in0_reverse = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _nAux_in1_reverse;
        public int nAux_in1_reverse
        {
            get { return _nAux_in1_reverse; }
            set
            {
                if (value != _nAux_in1_reverse)
                {
                    _nAux_in1_reverse = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _nAux_in2_reverse;
        public int nAux_in2_reverse
        {
            get { return _nAux_in2_reverse; }
            set
            {
                if (value != _nAux_in2_reverse)
                {
                    _nAux_in2_reverse = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _nAux_in3_reverse;
        public int nAux_in3_reverse
        {
            get { return _nAux_in3_reverse; }
            set
            {
                if (value != _nAux_in3_reverse)
                {
                    _nAux_in3_reverse = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _nAux_in4_reverse;
        public int nAux_in4_reverse
        {
            get { return _nAux_in4_reverse; }
            set
            {
                if (value != _nAux_in4_reverse)
                {
                    _nAux_in4_reverse = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _nAux_in5_reverse;
        public int nAux_in5_reverse
        {
            get { return _nAux_in5_reverse; }
            set
            {
                if (value != _nAux_in5_reverse)
                {
                    _nAux_in5_reverse = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _nAux_out_reverse;
        public int nAux_out_reverse
        {
            get { return _nAux_out_reverse; }
            set
            {
                if (value != _nAux_out_reverse)
                {
                    _nAux_out_reverse = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// ‘0’ : nonuse(default) 
        /// ‘1’ : Use LP1
        /// </summary>
        private int _UseLP1 = 0;
        public int UseLP1
        {
            get { return _UseLP1; }
            set
            {
                if (value != _UseLP1)
                {
                    _UseLP1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// ‘0’ : nonuse Clamp 
        /// ‘1’ : Use Clamp
        /// </summary>
        private int _UseClamp;
        public int UseClamp
        {
            get { return _UseClamp; }
            set
            {
                if (value != _UseClamp)
                {
                    _UseClamp = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// ‘0’ : Clamp sensor is connected by wiring 
        /// ‘1’ : In case of getting Clamp Status Command from host PC
        /// </summary>
        private int _ClampComType;
        public int ClampComType
        {
            get { return _ClampComType; }
            set
            {
                if (value != _ClampComType)
                {
                    _ClampComType = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// ‘0’ : When Clamp use option set, Before ‘READY’ on to OHT, if clamp is not off status, event occurs
        /// ‘1’ : When Clamp use option set, Before ‘READY’ on to OHT, E84 Controller waits until clamp off status
        /// </summary>
        private int _DisableClampEvent;
        public int DisableClampEvent
        {
            get { return _DisableClampEvent; }
            set
            {
                if (value != _DisableClampEvent)
                {
                    _DisableClampEvent = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// In case of E84 Controller is waiting until Clamp off status, sets this time out. If Clamp off status is not during this setting time, event occurs
        /// </summary>
        private int _ClampOffWaitTime;
        public int ClampOffWaitTime
        {
            get { return _ClampOffWaitTime; }
            set
            {
                if (value != _ClampOffWaitTime)
                {
                    _ClampOffWaitTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Index = ‘0’ : Load Port 0, ‘1’ : Load Port 1
        /// ‘0’ : Clamp off
        /// ‘1’ : Clamp on
        /// </summary>
        private ObservableCollection<int> _ClampSignal;
        public ObservableCollection<int> ClampSignal
        {
            get { return _ClampSignal; }
            set
            {
                if (value != _ClampSignal)
                {
                    _ClampSignal = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Index = ‘0’ : Load Port 0, ‘1’ : Load Port 1
        /// ‘0’ : There is not Carrier on LP 
        /// ‘1’ : There is Carrier on LP
        /// </summary>
        private ObservableCollection<int> _CarrierSignal;
        public ObservableCollection<int> CarrierSignal
        {
            get { return _CarrierSignal; }
            set
            {
                if (value != _CarrierSignal)
                {
                    _CarrierSignal = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<bool> _ChangedCarrierSignal;
        public ObservableCollection<bool> ChangedCarrierSignal
        {
            get { return _ChangedCarrierSignal; }
            set
            {
                if (value != _ChangedCarrierSignal)
                {
                    _ChangedCarrierSignal = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// ‘0’ : ‘Cs_1’ signal 를 nonuse
        /// ‘1’ : ‘Cs_1’ signal 를 사용 함
        /// </summary>
        private int _UseCs1;
        public int UseCs1
        {
            get { return _UseCs1; }
            set
            {
                if (value != _UseCs1)
                {
                    _UseCs1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        // TODO : 
        /// <summary>
        /// ‘0’ : ‘BUSY’ off -> ‘TR_REQ’ off -> ‘COMPT’ on status is continuously.If it is not, event occurs
        /// ‘1’ : Regardless of above three signal sequence, once ‘BUSY’ off, ‘TR_REQ’ off, ‘COMPT’ on status is executed, E84 Controller can go next step sequence
        /// </summary>
        private int _DisableReadOffEvent;
        public int DisableReadOffEvent
        {
            get { return _DisableReadOffEvent; }
            set
            {
                if (value != _DisableReadOffEvent)
                {
                    _DisableReadOffEvent = value;
                    RaisePropertyChanged();
                }
            }
        }

        // TODO : 
        /// <summary>
        /// ‘0’ : ‘CS’ on -> ‘VALID’ on status is continuously. It it is not, event occurs
        /// ‘1’ : Regardless of above two signal sequence, once ‘CS’ on, ‘VALID’ on status is executed, E84 Controller can go next step sequence(‘REQ’ on)
        /// </summary>
        private int _DisableValidOnEvent;
        public int DisableValidOnEvent
        {
            get { return _DisableValidOnEvent; }
            set
            {
                if (value != _DisableValidOnEvent)
                {
                    _DisableValidOnEvent = value;
                    RaisePropertyChanged();
                }
            }
        }

        // TODO : 
        /// <summary>
        /// ‘0’ : ‘VALID’ off -> ‘COMPT’ off -> ‘CS’ off status is continuously.If it is not, event occurs
        /// ‘1’ : Regardless of above two signal sequence, once ‘VALID’ off, ‘COMPT’ off, ‘CS’ off status is executed, E84 Controller can go next step sequence
        /// </summary>
        private int _DisableValidOffEvent;
        public int DisableValidOffEvent
        {
            get { return _DisableValidOffEvent; }
            set
            {
                if (value != _DisableValidOffEvent)
                {
                    _DisableValidOffEvent = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// ‘0’ : nonuse 
        /// ‘1’ : Linked action signal is ‘HO_AVBL’ when Light Curtain status occurs
        /// </summary>
        private int _HoAvblIsLinkedLightCurtain;
        public int HoAvblIsLinkedLightCurtain
        {
            get { return _HoAvblIsLinkedLightCurtain; }
            set
            {
                if (value != _HoAvblIsLinkedLightCurtain)
                {
                    _HoAvblIsLinkedLightCurtain = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 0’ : nonuse 
        /// ‘1’ : Linked action signal is ‘ES’ when Light Curtain status occurs
        /// </summary>
        private int _EsIsLinkedLightCurtain;
        public int EsIsLinkedLightCurtain
        {
            get { return _EsIsLinkedLightCurtain; }
            set
            {
                if (value != _EsIsLinkedLightCurtain)
                {
                    _EsIsLinkedLightCurtain = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// If signal on(or off) status is sustained during setting time, It is defined as on(or off) status
        /// </summary>
        private int _InputFilterTime;
        public int InputFilterTime
        {
            get { return _InputFilterTime; }
            set
            {
                if (value != _InputFilterTime)
                {
                    _InputFilterTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// (unit:sec), 
        /// ‘0’ : nonuse
        /// Above ‘1’ : use(range : 1 ~ 999 sec) 
        /// TP1 : Time from ‘REQ’ On to ‘TR_REQ’ On
        /// TP2 : Time from ‘READY’ On to ‘BUSY’ On
        /// TP3 : Time from ‘BUSY’ On to Carrier detect(or Carrier removal)
        /// TP4 : Time from ‘REQ’ Off to ‘BUSY’ Off
        /// TP5 : Time from ‘READY’ Off to ‘VALID’ Off
        /// TP6 : Time from ‘VALID’ Off to Next ‘VALID’ On(In the Continuous Mode)
        /// </summary>
        private ObservableCollection<int> _TimerTp;
        public ObservableCollection<int> TimerTp
        {
            get { return _TimerTp; }
            set
            {
                if (value != _TimerTp)
                {
                    _TimerTp = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// (unit:sec), 
        /// ‘0’ : nonuse
        /// Above ‘1’ : use(range : 1 ~ 999 sec) 
        /// TD0 : Time from ‘CS’ On to ‘VALID’ On
        /// TD1 : Time from ‘VALID’ Off to Next ‘VALID’ On
        /// TD1 delay time is smaller than TP6 Timeout value
        /// </summary>
        private ObservableCollection<int> _TimerTd;
        public ObservableCollection<int> TimerTd
        {
            get { return _TimerTd; }
            set
            {
                if (value != _TimerTd)
                {
                    _TimerTd = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// (unit:sec), 
        /// ‘0’ : nonuse
        /// Above ‘1’ : use(range : 1 ~ 120 sec) 
        /// </summary>
        private int _HeartBeatTime;
        public int HeartBeatTime
        {
            get { return _HeartBeatTime; }
            set
            {
                if (value != _HeartBeatTime)
                {
                    _HeartBeatTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// In case of auto mode, Get current all status information of E84 controller 
        /// ‘0’ : wait ‘HO_AVBL’ on 
        /// ‘1’ : wait ‘CS_0’ 또는 ‘CS_1’ on 
        /// ‘2’ : wait ‘VALID’ on 
        /// ‘3’ : wait ‘TR_REQ’ on 
        /// ‘4’ : wait ‘CLAMP’ off 
        /// ‘5’ : wait ‘BUSY’ on 
        /// ‘6’ : wait Changing LP Carrier Status 
        /// ‘7’ : wait ‘BUSY’ off 
        /// ‘8’ : wait ‘TR_REQ’ off 
        /// ‘9’ : wait ‘COMPT’ on 
        /// ‘10’ : wait ‘VALID’ off 
        /// ‘11’ : wait ‘COMPT’ off 
        /// ‘12’ : wait ‘CS_0’ 또는 ‘CS_1’ off
        /// </summary>
        private int _SequenceStep;
        public int SequenceStep
        {
            get { return _SequenceStep; }
            set
            {
                if (value != _SequenceStep)
                {
                    _SequenceStep = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// In case of auto mode, Get current status information of E84 controller During Carrier handshaking 
        /// ‘0’ : Initial status 
        /// ‘1’ : After ‘L_REQ’ On to OHT (Starting Loading)
        /// ‘2’ : After ‘CS_0’(or ‘CS_1’) Off from OHT (After Loading Cycle)
        /// ‘3’ : After ‘UL_REQ’ On to OHT (Starting Unloading)
        /// ‘4’ : After ‘CS_0’(or ‘CS_1’) Off from OHT (After Unloading Cycle)
        /// </summary>
        private int _SequenceSub;
        public int SequenceSub
        {
            get { return _SequenceSub; }
            set
            {
                if (value != _SequenceSub)
                {
                    _SequenceSub = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84ErrorCode _EventNumber = new E84ErrorCode();
        public E84ErrorCode EventNumber
        {
            get { return _EventNumber; }
            set
            {
                if (value != _EventNumber)
                {
                    _EventNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _RecoveryTimeout;
        public int RecoveryTimeout
        {
            get { return _RecoveryTimeout; }
            set
            {
                if (value != _RecoveryTimeout)
                {
                    _RecoveryTimeout = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _RetryCount;
        public int RetryCount
        {
            get { return _RetryCount; }
            set
            {
                if (value != _RetryCount)
                {
                    _RetryCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<E84ErrorCode> _E84Errors = new ObservableCollection<E84ErrorCode>();
        public ObservableCollection<E84ErrorCode> E84Errors
        {
            get { return _E84Errors; }
            set
            {
                if (value != _E84Errors)
                {
                    _E84Errors = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsStop;
        public bool IsStop
        {
            get { return _IsStop; }
            set
            {
                if (value != _IsStop)
                {
                    _IsStop = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int nSigHoAvbl;
        private int nSigReq;
        private int nSigReady;

        private Dictionary<(E84SignalTypeEnum, bool), E84EventCode> signalEventMap;
        private Dictionary<(E84SignalTypeEnum, bool), E84EventCode> signalTimeOutMap;
        private Dictionary<E84EventCode, E84TimeOutData> signalTimeOutDataMap;
        private Dictionary<int, Action<bool>> signalSetMappings;
        private Dictionary<int, Func<bool>> signalGetMappings;

        private bool isExcuteThread = false;
        private Thread thread;

        #region InitializeMapping
        public void InitializesignalEventMap()
        {
            signalEventMap = new Dictionary<(E84SignalTypeEnum, bool), E84EventCode>
            {
                {(E84SignalTypeEnum.HO_AVBL, true), E84EventCode.HO_AVBL_SEQUENCE_ERROR},
                {(E84SignalTypeEnum.CS_0, true), E84EventCode.CS_ON_SEQUENCE_ERROR},
                {(E84SignalTypeEnum.VALID, true), E84EventCode.VALED_ON_SEQUENCE_ERROR},
                {(E84SignalTypeEnum.TR_REQ, true), E84EventCode.TR_REQ_ON_SEQUENCE_ERROR},
                {(E84SignalTypeEnum.READY, true), E84EventCode.CLAMP_OFF_SEQUENCE_ERROR},
                {(E84SignalTypeEnum.BUSY, true), E84EventCode.BUSY_ON_SEQUENCE_ERROR},
                //{(), E84EventCode.CARRIER_SUATUS_CHANGE_STEP_SEQUENC_ERROR}, // TODO : Enum 외 데이터 
                {(E84SignalTypeEnum.BUSY, false), E84EventCode.BUSY_OFF_SEQUENCE_ERROR},
                {(E84SignalTypeEnum.TR_REQ, false), E84EventCode.TR_REQ_OFF_SEQUENCE_ERROR},
                {(E84SignalTypeEnum.COMPT, true), E84EventCode.COMPT_ON_SEQUENCE_ERROR},
                {(E84SignalTypeEnum.VALID, false), E84EventCode.VALID_OFF_SEQUENCE_ERROR},
                {(E84SignalTypeEnum.COMPT, false), E84EventCode.COMPT_OFF_SEQUENCE_ERROR},
                {(E84SignalTypeEnum.CS_0, false), E84EventCode.CS_OFF_SEQUENCE_ERROR},
                //{(), E84EventCode.HO_AVAL_OFF_SEQUENCE_ERROR}, // TODO : Monitoring
                //{(), E84EventCode.SENSOR_ERROR_LOAD_ONLY_PRESENCS},
                //{(), E84EventCode.SENSOR_ERROR_LOAD_ONLY_PLANCEMENT},
                //{(), E84EventCode.SENSOR_ERROR_UNLOAD_STILL_PRESENCS},
                //{(), E84EventCode.SENSOR_ERROR_UNLOAD_STILL_PLANCEMENT},
                //{(), E84EventCode.CLAMP_ON_BEFORE_OFF_BUSY},
                //{(), E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE},
                //{(), E84EventCode.HAND_SHAKE_ERROR_LOAD_MODE}
            };
        }
        /// TD1 delay time is smaller than TP6 Timeout value
        /// TD1 : Time from ‘VALID’ Off to Next ‘VALID’ On
        /// TP3 : Time from ‘BUSY’ On to Carrier detect(or Carrier removal)
        /// TP6 : Time from ‘VALID’ Off to Next ‘VALID’ On(In the Continuous Mode)
        public void InitializesignalTimeOutMap()
        {
            signalTimeOutMap = new Dictionary<(E84SignalTypeEnum, bool), E84EventCode>
            {
                {(E84SignalTypeEnum.TR_REQ, true), E84EventCode.TP1_TIMEOUT},
                {(E84SignalTypeEnum.BUSY, true), E84EventCode.TP2_TIMEOUT},
                //{(), E84EventCode.TP3_TIMEOUT}, // 별도 구현
                {(E84SignalTypeEnum.BUSY, false), E84EventCode.TP4_TIMEOUT},
                {(E84SignalTypeEnum.VALID, false), E84EventCode.TP5_TIMEOUT},
                //{(), E84EventCode.TP6_TIMEOUT}, // Not Used
                {(E84SignalTypeEnum.VALID, true), E84EventCode.TD0_DELAY},
                //{(), E84EventCode.TD1_DELAY}, // TODO 
            };
        }
        public void InitializeSignalSetMappings()
        {
            signalSetMappings = new Dictionary<int, Action<bool>>()
            {
                { 0, (value) => AutoChangeValue(E84SignalTypeEnum.L_REQ, value)},
                { 1, (value) => AutoChangeValue(E84SignalTypeEnum.U_REQ, value)},
                { 2, (value) => AutoChangeValue(E84SignalTypeEnum.VA, value)},
                { 3, (value) => AutoChangeValue(E84SignalTypeEnum.READY, value)},
                { 4, (value) => AutoChangeValue(E84SignalTypeEnum.VS_0, value)},
                { 5, (value) => AutoChangeValue(E84SignalTypeEnum.VS_1, value)},
                { 6, (value) => AutoChangeValue(E84SignalTypeEnum.HO_AVBL, value)},
                { 7, (value) => AutoChangeValue(E84SignalTypeEnum.ES, value)},
                { 8, (value) => AutoChangeValue(E84SignalTypeEnum.SELECT, value)},
                { 9, (value) => AutoChangeValue(E84SignalTypeEnum.MODE, value)}
            };
        }
        public void InitializeSignalGetMappings()
        {
            signalGetMappings = new Dictionary<int, Func<bool>>()
            {
                { 0, () => this.E84Outputs.LReq },
                { 1, () => this.E84Outputs.UlReq },
                { 2, () => this.E84Outputs.Va },
                { 3, () => this.E84Outputs.Ready },
                { 4, () => this.E84Outputs.VS0 },
                { 5, () => this.E84Outputs.VS1 },
                { 6, () => this.E84Outputs.HoAvbl },
                { 7, () => this.E84Outputs.ES },
                { 8, () => this.E84Outputs.Select },
                { 9, () => this.E84Outputs.Mode }
            };
        }
        public void InitializesignalTimeOutDataMap()
        {
            try
            {
                if (signalTimeOutDataMap == null)
                {
                    signalTimeOutDataMap = new Dictionary<E84EventCode, E84TimeOutData>();
                }

                AssignTimeOutData(E84EventCode.TP1_TIMEOUT);
                AssignTimeOutData(E84EventCode.TP2_TIMEOUT);
                AssignTimeOutData(E84EventCode.TP3_TIMEOUT);
                AssignTimeOutData(E84EventCode.TP4_TIMEOUT);
                AssignTimeOutData(E84EventCode.TP5_TIMEOUT);
                AssignTimeOutData(E84EventCode.TD0_DELAY);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void AssignTimeOutData(E84EventCode eventCode)
        {
            E84TimeOutData e84TimeOutData = new E84TimeOutData();

            switch (eventCode)
            {
                case E84EventCode.TP1_TIMEOUT:
                    e84TimeOutData.TimeOut = TimerTp[0];
                    e84TimeOutData.TimeOutInterval = TimeOutInterval;
                    e84TimeOutData.TargetSignalName = "REQ";
                    e84TimeOutData.TargetState = true;
                    break;
                case E84EventCode.TP2_TIMEOUT:
                    e84TimeOutData.TimeOut = TimerTp[1];
                    e84TimeOutData.TimeOutInterval = TimeOutInterval;
                    e84TimeOutData.TargetSignalName = "READY";
                    e84TimeOutData.TargetState = true;
                    break;
                case E84EventCode.TP3_TIMEOUT:
                    e84TimeOutData.TimeOut = TimerTp[2];
                    e84TimeOutData.TimeOutInterval = TimeOutInterval;
                    e84TimeOutData.TargetSignalName = "Carrier";
                    //e84TimeOutData.TargetState;
                    break;
                case E84EventCode.TP4_TIMEOUT:
                    e84TimeOutData.TimeOut = TimerTp[3];
                    e84TimeOutData.TimeOutInterval = TimeOutInterval;
                    e84TimeOutData.TargetSignalName = "REQ";
                    e84TimeOutData.TargetState = false;
                    break;
                case E84EventCode.TP5_TIMEOUT:
                    e84TimeOutData.TimeOut = TimerTp[4];
                    e84TimeOutData.TimeOutInterval = TimeOutInterval;
                    e84TimeOutData.TargetSignalName = "READY";
                    e84TimeOutData.TargetState = false;
                    break;
                case E84EventCode.TP6_TIMEOUT:
                    // Not Used
                    break;
                case E84EventCode.TD0_DELAY:
                    e84TimeOutData.TimeOut = TimerTd[0];
                    e84TimeOutData.TimeOutInterval = TimeOutInterval;
                    e84TimeOutData.TargetSignalName = "CS";
                    e84TimeOutData.TargetState = true;
                    break;
                case E84EventCode.TD1_DELAY:
                    // TODO : 
                    break;
                default:
                    // Handle undefined event codes or add more cases if needed
                    break;
            }

            SetTimeOutData(eventCode, e84TimeOutData);
        }
        public void SetTimeOutData(E84EventCode eventCode, E84TimeOutData timeoutData)
        {
            try
            {
                if (signalTimeOutDataMap.ContainsKey(eventCode))
                {
                    signalTimeOutDataMap[eventCode] = timeoutData;
                }
                else
                {
                    signalTimeOutDataMap.Add(eventCode, timeoutData);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private void Initialize()
        {
            try
            {
                ClampSignal = new ObservableCollection<int>();
                CarrierSignal = new ObservableCollection<int>();
                ChangedCarrierSignal = new ObservableCollection<bool>();

                TimerTp = new ObservableCollection<int>();
                TimerTd = new ObservableCollection<int>();

                for (int i = 0; i < (int)E84MaxCount.E84_MAX_LOAD_PORT; i++)
                {
                    ClampSignal.Add(0);
                    CarrierSignal.Add(0);
                    ChangedCarrierSignal.Add(false);
                }

                for (int i = 0; i < (int)E84MaxCount.E84_MAX_TIMER_TP; i++)
                {
                    TimerTp.Add(0);
                }

                for (int i = 0; i < (int)E84MaxCount.E84_MAX_TIMER_TD; i++)
                {
                    TimerTd.Add(0);
                }

                InitializeSignalSetMappings();
                InitializeSignalGetMappings();

                InitializesignalEventMap();
                InitializesignalTimeOutMap();
                InitializesignalTimeOutDataMap();

                MakeErrorData();

                StartThread();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SignalReset()
        {
            try
            {


                this.E84Inputs.Valid = false;
                this.E84Inputs.CS0 = false;
                this.E84Inputs.CS1 = false;
                this.E84Inputs.AmAvbl = false;
                this.E84Inputs.TrReq = false;
                this.E84Inputs.Busy = false;
                this.E84Inputs.Compt = false;
                this.E84Inputs.Cont = false;
                this.E84Inputs.Go = false;

                this.E84Outputs.LReq = false;
                this.E84Outputs.UlReq = false;
                this.E84Outputs.Va = false;
                this.E84Outputs.Ready = false;
                this.E84Outputs.VS0 = false;
                this.E84Outputs.VS1 = false;
                //this.E84Outputs.HoAvbl = false;
                //this.E84Outputs.ES = false;
                this.E84Outputs.Select = false;
                this.E84Outputs.Mode = false;

                // Manual
                if (this.Mode == 0)
                {
                    E84Outputs.HoAvbl = false;
                    E84Outputs.ES = false;
                }
                else if (this.Mode == 1)
                {
                    SequenceStep = 0;
                    SequenceSub = 0;

                    // TODO : 
                    E84Outputs.HoAvbl = true;
                    E84Outputs.ES = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void LightCurtainOnOff(bool flag)
        {
            try
            {
                // Use as Light Curtain Signal
                if (this.nAux_in3 == 1)
                {
                    // 에뮬 환경에서는 일단 고려하지 않음.
                    //if(nAux_in3_reverse == 0)
                    //{
                    //    this.LightCurtainAuxFlag = flag;
                    //}
                    //else if (nAux_in3_reverse == 1)
                    //{
                    //    this.LightCurtainAuxFlag = !flag;
                    //}

                    this.Aux_in3_Val = flag ? 1 : 0;

                    // Manual
                    if (this.Mode == 0)
                    {
                        // NOTHING : TODO 
                    }
                    // Auto
                    else if (this.Mode == 1)
                    {
                        // 현장에서는 옵션을 모두 1로 사용 중.
                        // 1로 사용해야 감지가 안됐을 때, 꺼져있는 것으로 인식 되기 때문.
                        if (this.HoAvblIsLinkedLightCurtain == 1)
                        {
                            E84Outputs.HoAvbl = !flag;
                        }

                        if (this.EsIsLinkedLightCurtain == 1)
                        {
                            E84Outputs.ES = !flag;
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], LightCurtainOnOff() : Not used. nAux_in3 = {nAux_in3}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private E84ErrorCode MakeE84ErrorWarningCode(int CodeNumber, string CodeName, string Description, E84EventCode EventCode)
        {
            E84ErrorCode retval = null;

            try
            {
                retval = new E84ErrorCode(CodeNumber, CodeName, Description, EventCode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private void MakeErrorData()
        {
            try
            {
                E84Errors.Add(MakeE84ErrorWarningCode(21, "TP1 Timeout Error", "TP1 Timeout Error", E84EventCode.TP1_TIMEOUT));
                E84Errors.Add(MakeE84ErrorWarningCode(22, "TP2 Timeout Error", "TP2 Timeout Error", E84EventCode.TP2_TIMEOUT));
                E84Errors.Add(MakeE84ErrorWarningCode(23, "TP3 Timeout Error", "TP3 Timeout Error", E84EventCode.TP3_TIMEOUT));
                E84Errors.Add(MakeE84ErrorWarningCode(24, "TP4 Timeout Error", "TP4 Timeout Error", E84EventCode.TP4_TIMEOUT));
                E84Errors.Add(MakeE84ErrorWarningCode(25, "TP5 Timeout Error", "TP5 Timeout Error", E84EventCode.TP5_TIMEOUT));
                E84Errors.Add(MakeE84ErrorWarningCode(26, "TP6 Timeout Error", "TP6 Timeout Error", E84EventCode.TP6_TIMEOUT));

                E84Errors.Add(MakeE84ErrorWarningCode(31, "TD0 Delay Timer Warning", "TD0 Delay Timer Warning", E84EventCode.TD0_DELAY));
                E84Errors.Add(MakeE84ErrorWarningCode(32, "TD1 Delay Timer Warning", "TD1 Delay Timer Warning", E84EventCode.TD1_DELAY));

                E84Errors.Add(MakeE84ErrorWarningCode(34, "Heartbeat Timeout Error", "Heartbeat Timeout Error", E84EventCode.HEARTBEAT_TIMEOUT));
                E84Errors.Add(MakeE84ErrorWarningCode(41, "Light Curtain Blocked Error", "Light Curtain Error", E84EventCode.LIGHT_CURTAIN_ERROR));

                E84Errors.Add(MakeE84ErrorWarningCode(51, "Cannot determine Transfer Type Error", "In time of ‘L_REQ’ On or ‘UL_REQ’ On , E84 board can not decision in some case, which is Loading Sequence or Unloading Sequence", E84EventCode.DETERMINE_TRANSFET_TYPE_ERROR));
                E84Errors.Add(MakeE84ErrorWarningCode(52, "Unclamp Timeout Error", "When Clamp off time out", E84EventCode.UNCLAMP_TIMEOUT));

                E84Errors.Add(MakeE84ErrorWarningCode(70, "“HO_AVBL” On Sequence Error", "When “HO_AVBL” should be On, In case of above one signal is On status between “CS_0 / CS_1”“VALID”,”TR_REQ”,”BUSY”,”COMPT”", E84EventCode.HO_AVBL_SEQUENCE_ERROR));
                E84Errors.Add(MakeE84ErrorWarningCode(71, "“CS_0/CS_1” On Sequence Error", "When “CS_0/CS_1” should be On, In case of above one signal is on status between “VALID”, ”TR_REQ”, ”BUSY”, ”COMPT”", E84EventCode.CS_ON_SEQUENCE_ERROR));
                E84Errors.Add(MakeE84ErrorWarningCode(72, "“VALID” OnSequence Error", "When “VALID” should be On, In case of “CS_0 / CS_1” is Off, or,In case of above one signal is On status between“VALID”, “TR_REQ”, “BUSY”, “COMPT”", E84EventCode.VALED_ON_SEQUENCE_ERROR));
                E84Errors.Add(MakeE84ErrorWarningCode(73, "“TR_REQ” On Sequence Error", "When “TR_REQ” should be On, In case of above one signal is Off status between “CS_0/CS_1” or “VALID”, or, In case of above one signal is On status between “TR_REQ”, “BUSY”, “COMPT”", E84EventCode.TR_REQ_ON_SEQUENCE_ERROR));
                E84Errors.Add(MakeE84ErrorWarningCode(74, "Clamp Off Sequence Error", "In case of Clamp On status before “READY” On status", E84EventCode.CLAMP_OFF_SEQUENCE_ERROR));
                E84Errors.Add(MakeE84ErrorWarningCode(75, "“BUSY” On Sequence Error", "When “BUSY” should be On, In case of above one signal is Off status between “CS_0 / CS_1”, “VALID”, “TR_REQ”, or, In case of “COMPT” On status, or, In case of Clamp On status before “BUSY” On", E84EventCode.BUSY_ON_SEQUENCE_ERROR));
                E84Errors.Add(MakeE84ErrorWarningCode(76, "Carrier Status Change Step Sequence Error", "In case of Clamp On status before Carrier status changed", E84EventCode.CARRIER_SUATUS_CHANGE_STEP_SEQUENC_ERROR));
                E84Errors.Add(MakeE84ErrorWarningCode(77, "“BUSY” Off Sequence Error", "When “BUSY” should be Off, In case of above one signal is Off status between “CS_0 / CS_1”, “VALID”, “TR_REQ”, or, In case of “COMPT” On", E84EventCode.BUSY_OFF_SEQUENCE_ERROR));
                E84Errors.Add(MakeE84ErrorWarningCode(78, "“TR_REQ” Off Sequence Error", "When “TR_REQ” should be Off, In case of above one signal is Off status between “CS_0 / CS_1”, “VALID”, or, In case of above one signal is On status between “BUSY”, “COMPT”", E84EventCode.TR_REQ_OFF_SEQUENCE_ERROR));
                E84Errors.Add(MakeE84ErrorWarningCode(79, "“COMPT” On Sequence Error", "When “COMPT” should be On, In case of above one signal is Off status between “CS_0 / CS_1”, “VALID”, or, In case of above one signal is On status between “TR_REQ”, “BUSY”", E84EventCode.COMPT_ON_SEQUENCE_ERROR));
                E84Errors.Add(MakeE84ErrorWarningCode(80, "“VALID” Off Sequence Error", "When “VALID” should be Off, In case of above one signal is Off status between “CS_0 / CS1”, “COMPT”, or, In case of above one signal is On status between “TR_REQ”, “BUSY”", E84EventCode.VALID_OFF_SEQUENCE_ERROR));
                E84Errors.Add(MakeE84ErrorWarningCode(81, "“COMPT” Off Sequence Error ", "When “COMPT” should be Off, In case of “CS_0 / CS_1” is Off status, or, In case of above one signal is On status between “VALID”, “TR_REQ”, “BUSY”", E84EventCode.COMPT_OFF_SEQUENCE_ERROR));

                // 82 : Placement or Presence sensor inputs changed
                // 83 : Clamp Status sensor input changed
                // 84 : E84 Input Register changed
                // 85 : E84 Output Register changed
                // 88 : AUX Input Status changed

                E84Errors.Add(MakeE84ErrorWarningCode(82, "“CS_0/CS_1” Off Sequence Error", "When “CS_0/CS_1” should be Off, In case of above one signal is On status between “VALID”, “TR_REQ”, “BUSY”, “COMPT”", E84EventCode.CS_OFF_SEQUENCE_ERROR));

                // In case of “HO_AVBL” Off Status(from OS Version 3008)
                E84Errors.Add(MakeE84ErrorWarningCode(83, "“HO_AVBL” Off Sequence Error", "When “CS_0 / CS_1” is On, In case of “HO_AVBL” Off Status", E84EventCode.HO_AVAL_OFF_SEQUENCE_ERROR));
                E84Errors.Add(MakeE84ErrorWarningCode(84, "Sensor Error (Load Sequence)", "Only on PRESENCS sensor during Load sequence. Recover LP by User from E84 page after OHT left", E84EventCode.SENSOR_ERROR_LOAD_ONLY_PRESENCS));
                E84Errors.Add(MakeE84ErrorWarningCode(85, "Sensor Error (Load Sequence)", "Only on PLANCEMENT sensor during Load sequence. Recover LP by User from E84 page after OHT left", E84EventCode.SENSOR_ERROR_LOAD_ONLY_PLANCEMENT));
                E84Errors.Add(MakeE84ErrorWarningCode(86, "Sensor Error (Unload Sequence)", "Still on PRESENCS sensor during Unload sequence. Recover LP by User from E84 page after OHT left", E84EventCode.SENSOR_ERROR_UNLOAD_STILL_PRESENCS));
                E84Errors.Add(MakeE84ErrorWarningCode(87, "Sensor Error (Unload Sequence)", "Still on PLANCEMENT sensor during Unload sequence. Recover LP by User from E84 page after OHT left", E84EventCode.SENSOR_ERROR_UNLOAD_STILL_PLANCEMENT));
                E84Errors.Add(MakeE84ErrorWarningCode(88, "Operation Error(Load Sequence)", "Carrier Presenced Alreay", E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE));
                E84Errors.Add(MakeE84ErrorWarningCode(89, "Sensor Error (Unload Sequence)", "Cannot Change Mode. CS, VALID is On", E84EventCode.HAND_SHAKE_ERROR_LOAD_MODE));
                E84Errors.Add(MakeE84ErrorWarningCode(90, "Sequence Error", "Clamp On Before Busy Off", E84EventCode.CLAMP_ON_BEFORE_OFF_BUSY));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public E84Simulator()
        {
            try
            {
                Initialize();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void StartThread()
        {
            try
            {
                isExcuteThread = true;
                thread = new Thread(new ThreadStart(ThreadCommunication));
                thread.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void ChangeEventNumber(E84ErrorCode code)
        {
            try
            {
                if (code != null)
                {
                    E84ErrorCode prev = EventNumber;
                    EventNumber = code.Copy();

                    if (prev.CodeNumber != EventNumber.CodeNumber)
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], ChangeEventNumber() : Before = [{prev.CodeNumber}]-{prev.CodeName}, After = [{EventNumber.CodeNumber}]-{EventNumber.CodeName}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void CheckLightCurtain()
        {
            try
            {
                if (this.Mode == 1 && nAux_in3 == 1 && Aux_in3_Val == 1)
                {
                    E84ErrorCode desiredItem = E84Errors.FirstOrDefault(item => item.EventCode == E84EventCode.LIGHT_CURTAIN_ERROR);

                    ChangeEventNumber(desiredItem);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void Check_HO_AVBL()
        {
            try
            {
                if (E84Inputs.CS0 == true && E84Outputs.HoAvbl == false)
                {
                    E84ErrorCode desiredItem = E84Errors.FirstOrDefault(item => item.EventCode == E84EventCode.HO_AVAL_OFF_SEQUENCE_ERROR);

                    ChangeEventNumber(desiredItem);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void ThreadCommunication()
        {
            try
            {
                while (isExcuteThread)
                {
                    try
                    {
                        // AUTO Mode
                        if (this.Mode == 1)
                        {
                            ChangeSequenceStep(SequenceStep);
                            ChangeSequenceSub(SequenceSub);

                            if (SequenceStep >= 3)
                            {
                                // 1. L_REQ or UL_REQ : 'ON'
                                // Condition) : CS_0 (ON) and VALID (ON)

                                if (SequenceStep == 3)
                                {
                                    if (CarrierSignal[0] == 0 && SequenceSub == 0)
                                    {
                                        AutoChangeValue(E84SignalTypeEnum.L_REQ, true);
                                    }
                                    else if (CarrierSignal[0] == 1 && SequenceSub == 0)
                                    {
                                        AutoChangeValue(E84SignalTypeEnum.U_REQ, true);
                                    }
                                }

                                // 2. READY : 'ON'
                                // Condition) : TR_REQ (ON)

                                if (SequenceStep == 4)
                                {
                                    AutoChangeValue(E84SignalTypeEnum.READY, true);
                                }

                                // 3. L_REQ or UL_REQ : 'OFF'
                                // Condition) : BUSY (ON)

                                if (SequenceStep == 6)
                                {
                                    if (E84Inputs.Busy == true)
                                    {
                                        if (CarrierSignal[0] == 1 && SequenceSub == 1)
                                        {
                                            AutoChangeValue(E84SignalTypeEnum.L_REQ, false);
                                        }
                                        else if (CarrierSignal[0] == 0 && SequenceSub == 3)
                                        {
                                            AutoChangeValue(E84SignalTypeEnum.U_REQ, false);
                                        }
                                    }
                                }

                                // 4. READY : 'OFF'
                                // Condition) : COMPT (ON)

                                if (E84Inputs.Compt == true)
                                {
                                    AutoChangeValue(E84SignalTypeEnum.READY, false);
                                }
                            }
                        }

                        CheckLightCurtain();
                        
                        //Check_HO_AVBL();

                        Thread.Sleep(10);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void ChangeSequenceSub(int currentStep)
        {
            /// In case of auto mode, Get current status information of E84 controller During Carrier handshaking 
            /// ‘0’ : Initial status 

            /// ‘1’ : After ‘L_REQ’ On to OHT (Starting Loading)
            /// ‘2’ : After ‘CS_0’(or ‘CS_1’) Off from OHT (After Loading Cycle)

            /// ‘3’ : After ‘UL_REQ’ On to OHT (Starting Unloading)
            /// ‘4’ : After ‘CS_0’(or ‘CS_1’) Off from OHT (After Unloading Cycle)

            try
            {
                // TODO : SequenceSub의 값을 0으로 초기화 해주는 로직 필요

                if (SequenceSub == 2 || SequenceSub == 4)
                {
                    SequenceSub = 0;
                }

                if (currentStep == 0)
                {
                    if (E84Outputs.LReq == true)
                    {
                        SequenceSub = 1;
                    }
                    else if (E84Outputs.UlReq == true)
                    {
                        SequenceSub = 3;
                    }
                    else
                    {
                    }
                }
                else if (currentStep == 1 || currentStep == 3)
                {
                    if (currentStep == 1 && E84Inputs.CS0 == false)
                    {
                        SequenceSub = 2;
                    }

                    if (currentStep == 3 && E84Inputs.CS0 == false)
                    {
                        SequenceSub = 4;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ChangeSequenceStep(int currentStep)
        {
            /// In case of auto mode, Get current all status information of E84 controller 
            /// ‘0’ : wait ‘HO_AVBL’ on 
            /// ‘1’ : wait ‘CS_0’ 또는 ‘CS_1’ on 
            /// ‘2’ : wait ‘VALID’ on 
            /// ‘3’ : wait ‘TR_REQ’ on 
            /// ‘4’ : wait ‘CLAMP’ off 
            /// ‘5’ : wait ‘BUSY’ on 
            /// ‘6’ : wait Changing LP Carrier Status 
            /// ‘7’ : wait ‘BUSY’ off 
            /// ‘8’ : wait ‘TR_REQ’ off 
            /// ‘9’ : wait ‘COMPT’ on 
            /// ‘10’ : wait ‘VALID’ off 
            /// ‘11’ : wait ‘COMPT’ off 
            /// ‘12’ : wait ‘CS_0’ 또는 ‘CS_1’ off

            try
            {
                switch (currentStep)
                {
                    case 0:
                        if (E84Outputs.HoAvbl == true)
                        {
                            SequenceStep = 1;
                        }
                        break;
                    case 1:
                        if (E84Inputs.CS0 == true)
                        {
                            SequenceStep = 2;
                        }
                        break;
                    case 2:
                        if (E84Inputs.Valid == true)
                        {
                            SequenceStep = 3;
                        }
                        break;
                    case 3:
                        if (E84Inputs.TrReq == true)
                        {
                            SequenceStep = 4;
                        }
                        break;
                    case 4:
                        if (ClampSignal[0] == 0)
                        {
                            SequenceStep = 5;
                        }
                        break;
                    case 5:
                        if (E84Inputs.Busy == true)
                        {
                            SequenceStep = 6;
                        }
                        break;
                    case 6:
                        if (ChangedCarrierSignal[0] == true)
                        {
                            SequenceStep = 7;
                        }
                        break;
                    case 7:
                        if (E84Inputs.Busy == false)
                        {
                            SequenceStep = 8;
                        }
                        break;
                    case 8:
                        if (E84Inputs.TrReq == false)
                        {
                            SequenceStep = 9;
                        }
                        break;
                    case 9:
                        if (E84Inputs.Compt == true)
                        {
                            SequenceStep = 10;
                        }
                        break;
                    case 10:
                        if (E84Inputs.Valid == false)
                        {
                            SequenceStep = 11;
                        }
                        break;
                    case 11:
                        if (E84Inputs.Compt == false)
                        {
                            SequenceStep = 12;
                        }
                        break;
                    case 12:
                        if (E84Inputs.CS0 == false)
                        {
                            SequenceStep = 0;
                        }
                        break;
                    case 13:
                        // Additional logic here if needed
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public E84SignalTypeEnum GetE84SignalNameUsingName(string name)
        {
            E84SignalTypeEnum retval = E84SignalTypeEnum.UNDEFINED;

            try
            {
                switch (name)
                {
                    case "VALID":
                        retval = E84SignalTypeEnum.VALID;
                        break;
                    case "CS_0":
                        retval = E84SignalTypeEnum.CS_0;
                        break;
                    case "CS_1":
                        retval = E84SignalTypeEnum.CS_1;
                        break;
                    case "AM_AVBL":
                        retval = E84SignalTypeEnum.AM_AVBL;
                        break;
                    case "TR_REQ":
                        retval = E84SignalTypeEnum.TR_REQ;
                        break;
                    case "BUSY":
                        retval = E84SignalTypeEnum.BUSY;
                        break;
                    case "COMPT":
                        retval = E84SignalTypeEnum.COMPT;
                        break;
                    case "CONT":
                        retval = E84SignalTypeEnum.CONT;
                        break;
                    case "GO":
                        retval = E84SignalTypeEnum.GO;
                        break;
                    case "L_REQ":
                        retval = E84SignalTypeEnum.L_REQ;
                        break;
                    case "UL_REQ":
                        retval = E84SignalTypeEnum.U_REQ;
                        break;
                    case "VA":
                        retval = E84SignalTypeEnum.VA;
                        break;
                    case "READY":
                        retval = E84SignalTypeEnum.READY;
                        break;
                    case "VS_0":
                        retval = E84SignalTypeEnum.VS_0;
                        break;
                    case "VS_1":
                        retval = E84SignalTypeEnum.VS_1;
                        break;
                    case "HO_AVBL":
                        retval = E84SignalTypeEnum.HO_AVBL;
                        break;
                    case "ES":
                        retval = E84SignalTypeEnum.ES;
                        break;
                    case "SELECT":
                        retval = E84SignalTypeEnum.SELECT;
                        break;
                    case "MODE":
                        retval = E84SignalTypeEnum.MODE;
                        break;
                    // Special 1)
                    case "CS":
                        // TODO : CS_0 or CS_1, 현재 CS_1은 사용하고 있지 않아 CS_0 사용
                        retval = E84SignalTypeEnum.CS_0;
                        break;
                    // Special 2)
                    case "REQ":

                        // Carrier 판단
                        if (this.CarrierSignal[0] == 0)
                        {
                            retval = E84SignalTypeEnum.L_REQ;
                        }
                        else if (this.CarrierSignal[0] == 1)
                        {
                            retval = E84SignalTypeEnum.U_REQ;
                        }

                        break;
                    default:
                        // Handle the case where the Name is not recognized or not handled
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private async Task<bool> CheckTimeout(E84TimeOutData e84TimeOutData)
        {
            try
            {
                if (e84TimeOutData != null)
                {
                    var TargetSignal = GetE84SignalNameUsingName(e84TimeOutData.TargetSignalName);

                    if (e84TimeOutData.TimeOut > 0)
                    {
                        DateTime startTime = DateTime.Now;

                        bool logging = true;

                        do
                        {
                            // Check if the target signal matches the specified flag
                            bool signalMatch = CheckSignalMatch(TargetSignal, e84TimeOutData.TargetState, logging);
                            logging = false;

                            if (signalMatch)
                            {
                                return false;
                            }

                            // Check if the elapsed time exceeds the timeout value
                            TimeSpan elapsed = DateTime.Now - startTime;

                            if (elapsed.TotalSeconds >= e84TimeOutData.TimeOut)
                            {
                                return true;
                            }

                            if (IsStop)
                            {
                                return true;
                            }

                            await Task.Delay(e84TimeOutData.TimeOutInterval);

                        } while (true);
                    }
                    else
                    {
                        // Check if the target signal matches the specified flag
                        bool signalMatch = CheckSignalMatch(TargetSignal, e84TimeOutData.TargetState);

                        if (signalMatch)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            // Timeout has not occurred
            return false;
        }
        private async Task<bool> CheckCarrierTimeout()
        {
            try
            {
                E84TimeOutData e84TimeOutData = GetTimeOutData(E84EventCode.TP3_TIMEOUT);

                if (e84TimeOutData.TimeOut > 0)
                {
                    DateTime startTime = DateTime.Now;

                    do
                    {
                        bool signalMatch = this.ChangedCarrierSignal[0];

                        if (signalMatch)
                        {
                            return false;
                        }

                        // Check if the elapsed time exceeds the timeout value
                        TimeSpan elapsed = DateTime.Now - startTime;

                        if (elapsed.TotalSeconds >= e84TimeOutData.TimeOut)
                        {
                            return true;
                        }

                        if (IsStop)
                        {
                            return true;
                        }

                        await Task.Delay(e84TimeOutData.TimeOutInterval);

                    } while (true);
                }
                else
                {
                    bool signalMatch = this.ChangedCarrierSignal[0];

                    if (signalMatch)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            // Timeout has not occurred
            return false;
        }
        private bool CheckSignalMatch(E84SignalTypeEnum targettype, bool match, bool logging = true)
        {
            bool retval = false;

            try
            {
                var ret = GetSignalIndex(targettype);

                if (ret != null)
                {
                    int? index = ret.Item2;

                    bool targetVal = ret.Item1 == "Input" ? E84Inputs[(int)index] : E84Outputs[(int)index];

                    if (targetVal == match)
                    {
                        retval = true;
                    }
                    else
                    {
                        if (logging)
                        {
                            LoggerManager.Debug($"[{this.GetType().Name}], CheckSignalMatch() : [{(ret.Item1 == "Input" ? "Input" : "Output")}], Signal = {targettype}, Match Value = {match}, Real = {targetVal}");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public E84TimeOutData GetTimeOutData(E84EventCode eventCode)
        {
            E84TimeOutData retval = null;

            try
            {
                if (signalTimeOutDataMap.ContainsKey(eventCode))
                {
                    retval = signalTimeOutDataMap[eventCode];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private async Task<E84BehaviorResult> HandleE84EventCode(E84EventCode eventCode, string TargetSignal, bool TargetState)
        {
            bool IsError = false;
            bool IsTimeOut = false;

            E84BehaviorResult retval = new E84BehaviorResult(TargetSignal, TargetState);
            E84TimeOutData e84TimeOutData = null;

            try
            {
                switch (eventCode)
                {
                    case E84EventCode.UNDEFINE:
                        break;
                    case E84EventCode.TP1_TIMEOUT:

                        e84TimeOutData = GetTimeOutData(eventCode);
                        IsTimeOut = await CheckTimeout(e84TimeOutData);

                        break;
                    case E84EventCode.TP2_TIMEOUT:

                        e84TimeOutData = GetTimeOutData(eventCode);
                        IsTimeOut = await CheckTimeout(e84TimeOutData);

                        break;
                    case E84EventCode.TP3_TIMEOUT:

                        // 별도 구현

                        break;
                    case E84EventCode.TP4_TIMEOUT:

                        e84TimeOutData = GetTimeOutData(eventCode);
                        IsTimeOut = await CheckTimeout(e84TimeOutData);

                        break;
                    case E84EventCode.TP5_TIMEOUT:

                        e84TimeOutData = GetTimeOutData(eventCode);
                        IsTimeOut = await CheckTimeout(e84TimeOutData);

                        break;
                    case E84EventCode.TP6_TIMEOUT:
                        // Not Used
                        break;
                    case E84EventCode.TD0_DELAY:

                        e84TimeOutData = GetTimeOutData(eventCode);
                        IsTimeOut = await CheckTimeout(e84TimeOutData);

                        if (IsTimeOut)
                        {
                            // TODO : EVENT
                        }

                        break;
                    case E84EventCode.TD1_DELAY:

                        // "TD1 Delay Timer Warning"

                        break;
                    case E84EventCode.HEARTBEAT_TIMEOUT:

                        // "Heartbeat Timeout Error"

                        break;
                    case E84EventCode.LIGHT_CURTAIN_ERROR:

                        // "Light Curtain Error"

                        break;
                    case E84EventCode.DETERMINE_TRANSFET_TYPE_ERROR:

                        // "In time of ‘L_REQ’ On or ‘UL_REQ’ On , E84 board can not decision in some case, which is Loading Sequence or Unloading Sequence"

                        break;
                    case E84EventCode.UNCLAMP_TIMEOUT:

                        // "When Clamp off time out"

                        break;
                    case E84EventCode.HO_AVBL_SEQUENCE_ERROR:

                        if (!CheckSignalMatch(E84SignalTypeEnum.CS_0, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.VALID, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.TR_REQ, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.BUSY, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.COMPT, false))
                        {
                            IsError = true;
                        }

                        break;
                    case E84EventCode.CS_ON_SEQUENCE_ERROR:

                        if (!CheckSignalMatch(E84SignalTypeEnum.VALID, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.TR_REQ, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.BUSY, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.COMPT, false))
                        {
                            IsError = true;
                        }

                        break;
                    case E84EventCode.VALED_ON_SEQUENCE_ERROR:

                        if (!CheckSignalMatch(E84SignalTypeEnum.CS_0, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.VALID, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.TR_REQ, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.BUSY, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.COMPT, false))
                        {
                            IsError = true;
                        }

                        break;
                    case E84EventCode.TR_REQ_ON_SEQUENCE_ERROR:

                        if (!CheckSignalMatch(E84SignalTypeEnum.CS_0, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.VALID, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.TR_REQ, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.BUSY, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.COMPT, false))
                        {
                            IsError = true;
                        }

                        break;
                    case E84EventCode.CLAMP_OFF_SEQUENCE_ERROR:

                        if (ClampSignal[0] == 1)
                        {
                            IsError = true;
                        }

                        break;
                    case E84EventCode.BUSY_ON_SEQUENCE_ERROR:

                        if (!CheckSignalMatch(E84SignalTypeEnum.CS_0, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.VALID, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.TR_REQ, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.COMPT, false) ||
                            ClampSignal[0] == 1)
                        {
                            IsError = true;
                        }

                        break;
                    case E84EventCode.CARRIER_SUATUS_CHANGE_STEP_SEQUENC_ERROR:

                        // TODO : 

                        // "In case of Clamp On status before Carrier status changed"

                        break;
                    case E84EventCode.BUSY_OFF_SEQUENCE_ERROR:

                        if (!CheckSignalMatch(E84SignalTypeEnum.CS_0, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.VALID, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.TR_REQ, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.COMPT, false))
                        {
                            IsError = true;
                        }

                        break;
                    case E84EventCode.TR_REQ_OFF_SEQUENCE_ERROR:

                        if (!CheckSignalMatch(E84SignalTypeEnum.CS_0, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.VALID, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.BUSY, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.COMPT, false))
                        {
                            IsError = true;
                        }

                        break;
                    case E84EventCode.COMPT_ON_SEQUENCE_ERROR:

                        if (!CheckSignalMatch(E84SignalTypeEnum.CS_0, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.VALID, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.TR_REQ, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.BUSY, false))
                        {
                            IsError = true;
                        }

                        break;
                    case E84EventCode.VALID_OFF_SEQUENCE_ERROR:

                        if (!CheckSignalMatch(E84SignalTypeEnum.CS_0, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.COMPT, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.TR_REQ, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.BUSY, false))
                        {
                            IsError = true;
                        }

                        break;
                    case E84EventCode.COMPT_OFF_SEQUENCE_ERROR:

                        if (!CheckSignalMatch(E84SignalTypeEnum.CS_0, true) ||
                            !CheckSignalMatch(E84SignalTypeEnum.VALID, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.TR_REQ, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.BUSY, false))
                        {
                            IsError = true;
                        }

                        break;
                    case E84EventCode.CS_OFF_SEQUENCE_ERROR:

                        if (!CheckSignalMatch(E84SignalTypeEnum.VALID, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.TR_REQ, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.BUSY, false) ||
                            !CheckSignalMatch(E84SignalTypeEnum.COMPT, false))
                        {
                            IsError = true;
                        }

                        break;
                    // 83
                    case E84EventCode.HO_AVAL_OFF_SEQUENCE_ERROR:

                        // TODO : 

                        // "When “CS_0 / CS_1” is On, In case of “HO_AVBL” Off Status"

                        break;
                    // 84
                    case E84EventCode.SENSOR_ERROR_LOAD_ONLY_PRESENCS:

                        // "Only on PRESENCS sensor during Load sequence. Recover LP by User from E84 page after OHT left"
                        // NOTHING

                        break;
                    // 85
                    case E84EventCode.SENSOR_ERROR_LOAD_ONLY_PLANCEMENT:

                        // "Only on PLANCEMENT sensor during Load sequence. Recover LP by User from E84 page after OHT left"
                        // NOTHING

                        break;
                    // 86
                    case E84EventCode.SENSOR_ERROR_UNLOAD_STILL_PRESENCS:

                        // "Still on PRESENCS sensor during Unload sequence. Recover LP by User from E84 page after OHT left"
                        // NOTHING

                        break;
                    // 87
                    case E84EventCode.SENSOR_ERROR_UNLOAD_STILL_PLANCEMENT:

                        // "Still on PLANCEMENT sensor during Unload sequence. Recover LP by User from E84 page after OHT left"
                        // NOTHING

                        break;
                    // 88
                    case E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE:

                        // "Carrier Presenced Alreay"
                        // NOTHING

                        break;
                    // 89
                    case E84EventCode.HAND_SHAKE_ERROR_LOAD_MODE:

                        // "Cannot Change Mode. CS, VALID is On"
                        // NOTHING

                        break;
                    // 90
                    case E84EventCode.CLAMP_ON_BEFORE_OFF_BUSY:

                        // "Clamp On Before Busy Off"
                        // NOTHING

                        break;
                    default:
                        break;
                }

                // ERROR
                if (IsTimeOut || IsError)
                {
                    E84ErrorCode desiredItem = E84Errors.FirstOrDefault(item => item.EventCode == eventCode);

                    if (desiredItem != null)
                    {
                        retval.ErrorCode = desiredItem.Copy();
                        EventNumber = desiredItem.Copy();
                    }
                    else
                    {
                        // ERROR
                    }

                    if (IsTimeOut)
                    {
                        retval.TimeOutData = e84TimeOutData;
                    }
                }
                // SUCCESS
                else
                {
                    retval.ErrorCode = new E84ErrorCode(0);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private Tuple<string, int?> GetSignalIndex(E84SignalTypeEnum type)
        {
            Tuple<string, int?> retval = null;

            try
            {
                switch (type)
                {
                    // Input cases
                    case E84SignalTypeEnum.VALID:
                        retval = new Tuple<string, int?>("Input", (int)E84SignalInputIndex.VALID);
                        break;
                    case E84SignalTypeEnum.CS_0:
                        retval = new Tuple<string, int?>("Input", (int)E84SignalInputIndex.CS_0);
                        break;
                    case E84SignalTypeEnum.CS_1:
                        retval = new Tuple<string, int?>("Input", (int)E84SignalInputIndex.CS_1);
                        break;
                    case E84SignalTypeEnum.AM_AVBL:
                        retval = new Tuple<string, int?>("Input", (int)E84SignalInputIndex.AM_AVBL);
                        break;
                    case E84SignalTypeEnum.TR_REQ:
                        retval = new Tuple<string, int?>("Input", (int)E84SignalInputIndex.TR_REQ);
                        break;
                    case E84SignalTypeEnum.BUSY:
                        retval = new Tuple<string, int?>("Input", (int)E84SignalInputIndex.BUSY);
                        break;
                    case E84SignalTypeEnum.COMPT:
                        retval = new Tuple<string, int?>("Input", (int)E84SignalInputIndex.COMPT);
                        break;
                    case E84SignalTypeEnum.CONT:
                        retval = new Tuple<string, int?>("Input", (int)E84SignalInputIndex.CONT);
                        break;
                    case E84SignalTypeEnum.GO:
                        retval = new Tuple<string, int?>("Input", (int)E84SignalInputIndex.GO);
                        break;

                    // Output cases
                    case E84SignalTypeEnum.L_REQ:
                        retval = new Tuple<string, int?>("Output", (int)E84SignalOutputIndex.L_REQ);
                        break;
                    case E84SignalTypeEnum.U_REQ:
                        retval = new Tuple<string, int?>("Output", (int)E84SignalOutputIndex.U_REQ);
                        break;
                    case E84SignalTypeEnum.VA:
                        retval = new Tuple<string, int?>("Output", (int)E84SignalOutputIndex.VA);
                        break;
                    case E84SignalTypeEnum.READY:
                        retval = new Tuple<string, int?>("Output", (int)E84SignalOutputIndex.READY);
                        break;
                    case E84SignalTypeEnum.VS_0:
                        retval = new Tuple<string, int?>("Output", (int)E84SignalOutputIndex.VS_0);
                        break;
                    case E84SignalTypeEnum.VS_1:
                        retval = new Tuple<string, int?>("Output", (int)E84SignalOutputIndex.VS_1);
                        break;
                    case E84SignalTypeEnum.HO_AVBL:
                        retval = new Tuple<string, int?>("Output", (int)E84SignalOutputIndex.HO_AVBL);
                        break;
                    case E84SignalTypeEnum.ES:
                        retval = new Tuple<string, int?>("Output", (int)E84SignalOutputIndex.ES);
                        break;
                    case E84SignalTypeEnum.SELECT:
                        retval = new Tuple<string, int?>("Output", (int)E84SignalOutputIndex.SELECT);
                        break;
                    case E84SignalTypeEnum.MODE:
                        retval = new Tuple<string, int?>("Output", (int)E84SignalOutputIndex.MODE);
                        break;

                    default:
                        retval = new Tuple<string, int?>("None", null);
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        // [추가 확인 필요]
        // 1) VALID => ON, OUTPUTS.READY가  False가 아니면 에러 발생 
        // 2) CS_0 => OFF, OUTPUTS.READY가  False가 아니면 에러 발생 
        // 3) TR_REQ => OFF, INPUTS.BUSY가  False가 아니면 에러 발생 
        // 4) COMPT => ON, INPUTS.TR_REQ가  False가 아니면 에러 발생 
        // 5) COMPT => OFF, OUTPUTS.READY가 False가 아니면 에러 발생 
        private bool SetState(E84SignalTypeEnum type, bool state)
        {
            bool retval = false;

            try
            {
                var ret = GetSignalIndex(type);

                if (ret.Item1 == "Input")
                {
                    var inputIndex = (int)ret.Item2;

                    if (E84Inputs[inputIndex] != state)
                    {
                        E84Inputs[inputIndex] = state;
                        retval = true;
                    }
                }
                else if (ret.Item1 == "Output")
                {
                    var outputIndex = (int)ret.Item2;

                    if (E84Outputs[outputIndex] != state)
                    {
                        E84Outputs[outputIndex] = state;
                        retval = true;
                    }
                }

                if (retval)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], AutoChangeValue() : Type = {type}, State = {state}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private async Task<E84BehaviorResult> HandleChangeValue(E84SignalTypeEnum type, bool state)
        {
            E84BehaviorResult retval = null;
            bool isChanged = false;

            try
            {
                // TODO : 
                //if (this.EventNumber != null && this.EventNumber.CodeNumber != 0)
                //{
                //    // ERROR
                //    retval = new E84BehaviorResult(type.ToString(), state);
                //    retval.ErrorCode = this.EventNumber.Copy();

                //    return retval;
                //}

                E84EventCode eventCode = E84EventCode.UNDEFINE;
                bool isExistEvent = signalTimeOutMap.TryGetValue((type, state), out eventCode);

                if (isExistEvent)
                {
                    retval = await HandleE84EventCode(eventCode, type.ToString(), state);
                }

                if (retval == null || retval.ErrorCode.CodeNumber == 0)
                {
                    isExistEvent = signalEventMap.TryGetValue((type, state), out eventCode);

                    if (isExistEvent)
                    {
                        retval = await HandleE84EventCode(eventCode, type.ToString(), state);

                        if (retval?.ErrorCode.CodeNumber == 0)
                        {
                            isChanged = SetState(type, state);
                        }
                    }
                    else
                    {
                        isChanged = SetState(type, state);
                    }
                }

                // 값이 변경 뒤, 확인해야 하는 로직
                if (isChanged)
                {
                    // Time from ‘BUSY’ On to Carrier detect(or Carrier removal)
                    if (type == E84SignalTypeEnum.BUSY && state == true)
                    {
                        var timeout_occurred = await CheckCarrierTimeout();

                        // TODO : 
                        if (ChangedCarrierSignal[0] == true)
                        {
                            ChangedCarrierSignal[0] = false;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private void AutoChangeValue(E84SignalTypeEnum type, bool state)
        {
            try
            {
                if (this.EventNumber != null && this.EventNumber.CodeNumber != 0)
                {
                    //LoggerManager.Error($"[{this.GetType().Name}], AutoChangeValue() : type = {type}, state = {state}, Code = {this.EventNumber.CodeNumber}");
                }
                else
                {
                    HandleChangeValue(type, state).ConfigureAwait(false);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task<E84BehaviorResult> ChangeValue(E84SignalTypeEnum type, bool state)
        {
            return await HandleChangeValue(type, state);
        }
        public async Task<E84BehaviorResult> SetInput(string name, bool state)
        {
            E84BehaviorResult retval = null;
            string ReasonOfError = string.Empty;

            try
            {
                E84SignalTypeEnum signal = GetE84SignalNameUsingName(name);

                retval = await ChangeValue(signal, state);

                if (retval.ErrorCode.CodeNumber != 0)
                {
                    LoggerManager.Error($"[{this.GetType().Name}], SetInput() : ERROR, Code = {retval.ErrorCode.CodeNumber}, Descirption = {retval.ErrorCode.Description}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private int GetInputs()
        {
            int inputs = 0;

            try
            {
                if (E84Inputs.Valid)
                    inputs |= 1 << (int)E84SignalInputIndex.VALID;
                if (E84Inputs.CS0)
                    inputs |= 1 << (int)E84SignalInputIndex.CS_0;
                if (E84Inputs.CS1)
                    inputs |= 1 << (int)E84SignalInputIndex.CS_1;
                if (E84Inputs.AmAvbl)
                    inputs |= 1 << (int)E84SignalInputIndex.AM_AVBL;
                if (E84Inputs.TrReq)
                    inputs |= 1 << (int)E84SignalInputIndex.TR_REQ;
                if (E84Inputs.Busy)
                    inputs |= 1 << (int)E84SignalInputIndex.BUSY;
                if (E84Inputs.Compt)
                    inputs |= 1 << (int)E84SignalInputIndex.COMPT;
                if (E84Inputs.Cont)
                    inputs |= 1 << (int)E84SignalInputIndex.CONT;
                if (E84Inputs.Go)
                    inputs |= 1 << (int)E84SignalInputIndex.GO;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return inputs;
        }
        private int GetOutputs()
        {
            int outputs = 0;

            try
            {
                if (E84Outputs.LReq)
                    outputs |= 1 << (int)E84SignalOutputIndex.L_REQ;
                if (E84Outputs.UlReq)
                    outputs |= 1 << (int)E84SignalOutputIndex.U_REQ;
                if (E84Outputs.Va)
                    outputs |= 1 << (int)E84SignalOutputIndex.VA;
                if (E84Outputs.Ready)
                    outputs |= 1 << (int)E84SignalOutputIndex.READY;
                if (E84Outputs.VS0)
                    outputs |= 1 << (int)E84SignalOutputIndex.VS_0;
                if (E84Outputs.VS1)
                    outputs |= 1 << (int)E84SignalOutputIndex.VS_1;
                if (E84Outputs.HoAvbl)
                    outputs |= 1 << (int)E84SignalOutputIndex.HO_AVBL;
                if (E84Outputs.ES)
                    outputs |= 1 << (int)E84SignalOutputIndex.ES;
                if (E84Outputs.Select)
                    outputs |= 1 << (int)E84SignalOutputIndex.SELECT;
                if (E84Outputs.Mode)
                    outputs |= 1 << (int)E84SignalOutputIndex.MODE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return outputs;
        }
        private int GetAuxIn()
        {
            int Aux_in_Val = 0;

            try
            {
                if (Aux_in0_Val != 0)
                    Aux_in_Val |= 1 << 0;
                if (Aux_in1_Val != 0)
                    Aux_in_Val |= 1 << 1;
                if (Aux_in2_Val != 0)
                    Aux_in_Val |= 1 << 2;
                if (Aux_in3_Val != 0)
                    Aux_in_Val |= 1 << 3;
                if (Aux_in4_Val != 0)
                    Aux_in_Val |= 1 << 4;
                if (Aux_in5_Val != 0)
                    Aux_in_Val |= 1 << 5;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Aux_in_Val;
        }
        // Not used의 경우 리턴값을 0으로 처리 해놓음.
        // 메뉴얼에 없는 내용 중, 임의로 0 값으로 처리
        #region Not used
        public void e84_Get_Dll_Version(int netId, out int dllVersion, out int status)
        {
            // Not used
            dllVersion = 0;
            status = 0;
        }

        public void e84_Is_Loggable(int netId, [MarshalAs(UnmanagedType.I1)] out bool loggable, out int status)
        {
            // Not used
            loggable = false;
            status = 0;
        }

        public void e84_Enable_Log(int netId, [MarshalAs(UnmanagedType.I1)] bool enable, out int status, StringBuilder logPath)
        {
            // Not used
            status = 0;
        }

        public void e84_Get_Mode_Auto(int netId, out int mode, out int status)
        {
            // Not used
            mode = 0;
            status = 0;
        }

        public void e84_Get_Aux_Signals(int netId, out int auxInputs, out int auxOutput0, out int status)
        {
            // Not used
            auxInputs = 0;
            auxOutput0 = 0;
            status = 0;
        }

        public void e84_Get_Current_Status(int netId, out int controllerStatus, out int sequenceStep, out int sequenceSub, out int status)
        {
            // Not used
            controllerStatus = 0;
            sequenceStep = 0;
            sequenceSub = 0;
            status = 0;
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="netId"></param>
        /// <param name="set"></param>
        /// <param name="get"></param>
        /// If return ‘98’, it is connection is ok 
        /// <param name="status"></param>
        /// ‘0’ : communication ok
        /// Above ‘1’ : disconnect communication
        public void e84_Connection(int netId, int set, out int get, out int status)
        {
            lock (_lock)
            {
                get = 98;
                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="netId"></param>
        /// <param name="osVersion"></param>
        /// E84 OS Version
        /// <param name="status"></param>
        public void e84_Get_OS_Version(int netId, out int osVersion, out int status)
        {
            lock (_lock)
            {
                // TODO : 
                // Maestro의 경우, Hynix에서 ErrorAct 옵션을 사용하기 위해서 v3013을 사용 중.
                osVersion = 3013;
                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        // (주의) In case of manual mode : When entering from auto mode to manual mode, E84 Controller set ‘HO_AVBL’ and ‘ES’ signal off first in order to do not moving AMH
        // AMHS : Automated Material Handling System (the other name is OHT(over head transfer) vehicle) 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="netId"></param>
        /// <param name="mode"></param>
        /// ‘0’ : Manual Mode 
        /// ‘1’ : Auto Mode
        /// <param name="status"></param>
        public void e84_Set_Mode_Auto(int netId, int mode, out int status)
        {
            lock (_lock)
            {
                this.nNet_id = netId;
                this.Mode = mode;

                ChangeMode();

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        // When changing Auto_mode  Manual_mode, ‘ho_avbl’, ‘es’ signal off by E84 Controller
        private void ChangeMode()
        {
            try
            {
                // Manual
                if (this.Mode == 0)
                {
                    E84Outputs.HoAvbl = false;
                    E84Outputs.ES = false;
                }
                else if (this.Mode == 1)
                {
                    // TODO : 
                    E84Outputs.HoAvbl = true;
                    E84Outputs.ES = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void e84_Config_Set_Aux_Options(int netId, int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0, out int status)
        {
            lock (_lock)
            {
                this.nNet_id = netId;

                this.nAux_in0 = auxInput0;
                this.nAux_in1 = auxInput1;
                this.nAux_in2 = auxInput2;
                this.nAux_in3 = auxInput3;
                this.nAux_in4 = auxInput4;
                this.nAux_in5 = auxInput5;
                this.nAux_out = auxOutput0;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Get_Aux_Options(int netId, out int auxInput0, out int auxInput1, out int auxInput2, out int auxInput3, out int auxInput4, out int auxInput5, out int auxOutput0, out int status)
        {
            lock (_lock)
            {
                auxInput0 = this.nAux_in0;
                auxInput1 = this.nAux_in1;
                auxInput2 = this.nAux_in2;
                auxInput3 = this.nAux_in3;
                auxInput4 = this.nAux_in4;
                auxInput5 = this.nAux_in5;
                auxOutput0 = this.nAux_out;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Set_Reverse_Signal(int netId, int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0, out int status)
        {
            lock (_lock)
            {
                this.nNet_id = netId;

                this.nAux_in0_reverse = auxInput0;
                this.nAux_in1_reverse = auxInput1;
                this.nAux_in2_reverse = auxInput2;
                this.nAux_in3_reverse = auxInput3;
                this.nAux_in4_reverse = auxInput4;
                this.nAux_in5_reverse = auxInput5;
                this.nAux_out_reverse = auxOutput0;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Get_Reverse_Signal(int netId, out int auxInput0, out int auxInput1, out int auxInput2, out int auxInput3, out int auxInput4, out int auxInput5, out int auxOutput0, out int status)
        {
            lock (_lock)
            {
                auxInput0 = this.nAux_in0_reverse;
                auxInput1 = this.nAux_in1_reverse;
                auxInput2 = this.nAux_in2_reverse;
                auxInput3 = this.nAux_in3_reverse;
                auxInput4 = this.nAux_in4_reverse;
                auxInput5 = this.nAux_in5_reverse;
                auxOutput0 = this.nAux_out_reverse;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Set_Aux_Output_Signal(int netId, int auxOutput0, out int status)
        {
            lock (_lock)
            {
                this.nNet_id = netId;

                this.nAux_out = auxOutput0;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        // TODO : 메뉴얼에 없음
        public void e84_Get_Aux_Output_Signal(int netId, out int auxOutput0, out int status)
        {
            lock (_lock)
            {
                auxOutput0 = this.nAux_out;
                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Set_Use_LP1(int netId, int use, out int status)
        {
            lock (_lock)
            {
                this.UseLP1 = use;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Get_Use_LP1(int netId, out int use, out int status)
        {
            lock (_lock)
            {
                use = this.UseLP1;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Set_Clamp(int netId, int use, int inputType, int actionType, int timer, out int status)
        {
            lock (_lock)
            {
                this.UseClamp = use;
                this.ClampComType = inputType;
                this.DisableClampEvent = actionType;
                this.ClampOffWaitTime = timer;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Get_Clamp(int netId, out int use, out int inputType, out int actionType, out int timer, out int status)
        {
            lock (_lock)
            {
                use = this.UseClamp;
                inputType = this.ClampComType;
                actionType = this.DisableClampEvent;
                timer = this.ClampOffWaitTime;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Set_Clamp_Signal(int netId, int loadPortNo, int clampOn, out int status)
        {
            lock (_lock)
            {
                this.ClampSignal[loadPortNo] = clampOn;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Get_Clamp_Signal(int netId, int loadPortNo, out int clampOn, out int status)
        {
            lock (_lock)
            {
                clampOn = this.ClampSignal[loadPortNo];

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Set_Carrier_Signal(int netId, int loadPortNo, int carrierExist, out int status)
        {
            lock (_lock)
            {
                if (this.CarrierSignal[loadPortNo] != carrierExist)
                {
                    this.ChangedCarrierSignal[loadPortNo] = true;
                }

                this.CarrierSignal[loadPortNo] = carrierExist;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Get_Carrier_Signal(int netId, int loadPortNo, out int carrierExist, out int status)
        {
            lock (_lock)
            {
                carrierExist = this.CarrierSignal[loadPortNo];

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Reset_E84_Interface(int netId, int reset, out int status)
        {
            lock (_lock)
            {
                if (reset == 0)
                {
                    // No Action
                }
                else if (reset == 1)
                {
                    // TODO : E84 Interface Reset

                    SignalReset();
                }
                else
                {
                    // ERROR
                }

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Get_E84_Signals(int netId, out int inputSignals, out int outputSignals, out int status)
        {
            lock (_lock)
            {
                inputSignals = GetInputs();
                outputSignals = GetOutputs();

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Set_E84_Signal_Options(int netId, int useCs1, int readyOff, int validOn, int validOff, out int status)
        {
            lock (_lock)
            {
                this.UseCs1 = useCs1;
                this.DisableReadOffEvent = readyOff;
                this.DisableValidOnEvent = validOn;
                this.DisableValidOffEvent = validOff;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Get_E84_Signal_Options(int netId, out int useCs1, out int readyOff, out int validOn, out int validOff, out int status)
        {
            lock (_lock)
            {
                useCs1 = this.UseCs1;
                readyOff = this.DisableReadOffEvent;
                validOn = this.DisableValidOnEvent;
                validOff = this.DisableValidOffEvent;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        // TODO : 메뉴얼에 없음
        public void e84_Config_Set_E84_Signal_Out_Options(int netId, int nSigHoAvbl, int nSigReq, int nSigReady, out int status)
        {
            lock (_lock)
            {
                // TODO : 
                this.nSigHoAvbl = nSigHoAvbl;
                this.nSigReq = nSigReq;
                this.nSigReady = nSigReady;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        // TODO : 메뉴얼에 없음
        public void e84_Config_Get_E84_Signal_Out_Options(int netId, out int nSigHoAvbl, out int nSigReq, out int nSigReady, out int status)
        {
            lock (_lock)
            {
                // TODO : 
                nSigHoAvbl = this.nSigHoAvbl;
                nSigReq = this.nSigReq;
                nSigReady = this.nSigReady;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Set_LightCurtain_Signal_Options(int netId, int useHoAvbl, int useEs, out int status)
        {
            lock (_lock)
            {
                this.HoAvblIsLinkedLightCurtain = useHoAvbl;
                this.EsIsLinkedLightCurtain = useEs;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Get_LightCurtain_Signal_Options(int netId, out int useHoAvbl, out int useEs, out int status)
        {
            lock (_lock)
            {
                useHoAvbl = this.HoAvblIsLinkedLightCurtain;
                useEs = this.EsIsLinkedLightCurtain;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Set_HO_AVBL_Signal(int netId, int hoAvblOn, out int status)
        {
            lock (_lock)
            {
                this.E84Outputs.HoAvbl = hoAvblOn != 0;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Get_HO_AVBL_Signal(int netId, out int hoAvblOn, out int status)
        {
            lock (_lock)
            {
                hoAvblOn = this.E84Outputs.HoAvbl ? 1 : 0;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Set_ES_Signal(int netId, int esSignal, out int status)
        {
            lock (_lock)
            {
                this.E84Outputs.ES = esSignal != 0;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Get_ES_Signal(int netId, out int esSignal, out int status)
        {
            lock (_lock)
            {
                esSignal = this.E84Outputs.ES ? 1 : 0;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Set_e84_Signal_Out_Index(int netId, int signalIndex, int signalOn, out int status)
        {
            lock (_lock)
            {
                if (signalSetMappings.TryGetValue(signalIndex, out Action<bool> setSignal))
                {
                    bool value = signalOn != 0;
                    setSignal(value);
                }
                else
                {
                    // ERROR: Invalid signalIndex
                }

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Get_e84_Signal_Out_Index(int netId, int signalIndex, out int signalOn, out int status)
        {
            lock (_lock)
            {
                if (signalGetMappings.TryGetValue(signalIndex, out Func<bool> getSignal))
                {
                    signalOn = getSignal() ? 1 : 0;
                }
                else
                {
                    // ERROR: Invalid signalIndex
                    signalOn = 0;
                }

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Set_Input_Filter_Time(int netId, int inputFilterTime, out int status)
        {
            lock (_lock)
            {
                this.InputFilterTime = inputFilterTime;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Get_Input_Filter_Time(int netId, out int inputFilterTime, out int status)
        {
            lock (_lock)
            {
                inputFilterTime = this.InputFilterTime;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Set_TP_Timeout(int netId, int tp1, int tp2, int tp3, int tp4, int tp5, int tp6, out int status)
        {
            lock (_lock)
            {
                this.TimerTp[0] = tp1;
                this.TimerTp[1] = tp2;
                this.TimerTp[2] = tp3;
                this.TimerTp[3] = tp4;
                this.TimerTp[4] = tp5;
                this.TimerTp[5] = tp6;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;

                try
                {
                    InitializesignalTimeOutDataMap();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }
        public void e84_Config_Get_TP_Timeout(int netId, out int tp1, out int tp2, out int tp3, out int tp4, out int tp5, out int tp6, out int status)
        {
            lock (_lock)
            {
                tp1 = this.TimerTp[0];
                tp2 = this.TimerTp[1];
                tp3 = this.TimerTp[2];
                tp4 = this.TimerTp[3];
                tp5 = this.TimerTp[4];
                tp6 = this.TimerTp[5];

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Set_TD_DelayTime(int netId, int td0, int td1, out int status)
        {
            lock (_lock)
            {
                this.TimerTd[0] = td0;
                this.TimerTd[1] = td1;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Get_TD_DelayTime(int netId, out int td0, out int td1, out int status)
        {
            lock (_lock)
            {
                td0 = this.TimerTd[0];
                td1 = this.TimerTd[1];

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Set_Heartbeat_Time(int netId, int heartBeat, out int status)
        {
            lock (_lock)
            {
                this.HeartBeatTime = heartBeat;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        public void e84_Config_Get_Heartbeat_Time(int netId, out int heartBeat, out int status)
        {
            lock (_lock)
            {
                heartBeat = this.HeartBeatTime;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        private int GetControllerStatus()
        {
            int retval = 0;

            try
            {
                retval = this.Mode;

                // TODO : 

                // 많이 발생했던 에러 위주로 먼저 구현.
                // UNDEFINE,
                // [O] TP1_TIMEOUT,
                // [O] TP2_TIMEOUT,
                // [O] TP3_TIMEOUT,
                // [O] TP4_TIMEOUT,
                // [O] TP5_TIMEOUT,
                // TP6_TIMEOUT,
                // TD0_DELAY,
                // TD1_DELAY,
                // HEARTBEAT_TIMEOUT,
                // [O] LIGHT_CURTAIN_ERROR,
                // DETERMINE_TRANSFET_TYPE_ERROR,
                // UNCLAMP_TIMEOUT,
                // [O] HO_AVBL_SEQUENCE_ERROR,
                // CS_ON_SEQUENCE_ERROR,
                // VALED_ON_SEQUENCE_ERROR,
                // TR_REQ_ON_SEQUENCE_ERROR,
                // CLAMP_OFF_SEQUENCE_ERROR,
                // BUSY_ON_SEQUENCE_ERROR,
                // CARRIER_SUATUS_CHANGE_STEP_SEQUENC_ERROR,
                // BUSY_OFF_SEQUENCE_ERROR,
                // TR_REQ_OFF_SEQUENCE_ERROR,
                // COMPT_ON_SEQUENCE_ERROR,
                // VALID_OFF_SEQUENCE_ERROR,
                // COMPT_OFF_SEQUENCE_ERROR,
                // CS_OFF_SEQUENCE_ERROR,
                // [O] HO_AVAL_OFF_SEQUENCE_ERROR,
                // SENSOR_ERROR_LOAD_ONLY_PRESENCS,
                // SENSOR_ERROR_LOAD_ONLY_PLANCEMENT,
                // SENSOR_ERROR_UNLOAD_STILL_PRESENCS,
                // SENSOR_ERROR_UNLOAD_STILL_PLANCEMENT,
                // HAND_SHAKE_ERROR_LOAD_PRESENCE,  // 이재작업이 시작된 상태가 아닌데 캐리어가 감지된 경우
                // HAND_SHAKE_ERROR_LOAD_MODE,   // OHT 가 도착하여 CS, VALID 신호가 켜졌는데 모드를 변경하려고 한 경우
                // CLAMP_ON_BEFORE_OFF_BUSY

                if (EventNumber != null && EventNumber.CodeNumber > 1)
                {
                    retval = EventNumber.CodeNumber;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void e84_Get_Report_All_Status(int netId, out int controllerStatus, out int sequenceStep, out int sequenceSub, out int e84Inputs, out int e84Outputs, out int auxInputs, out int auxOutputs, out int status)
        {
            lock (_lock)
            {
                controllerStatus = GetControllerStatus();

                sequenceStep = this.SequenceStep;
                sequenceSub = this.SequenceSub;

                e84Inputs = GetInputs();
                e84Outputs = GetOutputs();

                auxInputs = GetAuxIn();
                auxOutputs = 0;

                if (Aux_out0_Val != 0)
                {
                    auxOutputs |= 1 << 0;
                }

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        /// <summary>
        /// Command to clear events that have occured
        /// nSet : (decimal), (unit:sec)
        /// ‘0’ : nonuse 
        /// ‘1’ : Event Clear
        public void e84_Set_Clear_Event(int netId, int clear, out int status)
        {
            lock (_lock)
            {
                if (clear == 1)
                {
                    EventNumber.CodeNumber = 0;
                }

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }

        // TODO : 메뉴얼에 없음
        public void e84_Config_Set_Communication(int NetId, int timeOut, int retry, out int status)
        {
            lock (_lock)
            {
                this.RecoveryTimeout = timeOut;
                this.RetryCount = retry;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
        // TODO : 메뉴얼에 없음
        public void e84_Config_Get_Communication(int NetId, out int timeOut, out int retry, out int status)
        {
            lock (_lock)
            {
                timeOut = this.RecoveryTimeout;
                retry = this.RetryCount;

                status = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            }
        }
    }
}
