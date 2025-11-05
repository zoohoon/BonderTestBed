namespace E84
{
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

    public class EMotionTekE84CommModule : IFactoryModule, IE84CommModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> Define E84 API </remarks>

        /*** Environment ***/

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Connection(int netId, int set, out int get, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_OS_Version(int netId, out int osVersion, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Dll_Version(int netId, out int dllVersion, out int status);

        /*** Log ***/
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Is_Loggable(int netId, [MarshalAs(UnmanagedType.I1)] out bool loggable, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Enable_Log(int netId, [MarshalAs(UnmanagedType.I1)] bool enable, out int status, StringBuilder logPath);

        /*** Mode ***/
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_Mode_Auto(int netId, int mode, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Mode_Auto(int netId, out int mode, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_Aux_Options(int netId, int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_Aux_Options(int netId, out int auxInput0, out int auxInput1, out int auxInput2, out int auxInput3, out int auxInput4, out int auxInput5, out int auxOutput0, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_Reverse_Signal(int netId, int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_Reverse_Signal(int netId, out int auxInput0, out int auxInput1, out int auxInput2, out int auxInput3, out int auxInput4, out int auxInput5, out int auxOutput0, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_Aux_Output_Signal(int netId, int auxOutput0, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Aux_Output_Signal(int netId, out int auxOutput0, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Aux_Signals(int netId, out int auxInputs, out int auxOutput0, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_Use_LP1(int netId, int use, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_Use_LP1(int netId, out int use, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_Clamp(int netId, int use, int inputType, int actionType, int timer, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_Clamp(int netId, out int use, out int inputType, out int actionType, out int timer, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_Clamp_Signal(int netId, int loadPortNo, int clampOn, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Clamp_Signal(int netId, int loadPortNo, out int clampOn, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_Carrier_Signal(int netId, int loadPortNo, int carrierExist, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Carrier_Signal(int netId, int loadPortNo, out int carrierExist, out int status);

        /*** E84 Interface ***/
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Reset_E84_Interface(int netId, int reset, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_E84_Signals(int netId, out int inputSignals, out int outputSignals, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_E84_Signal_Options(int netId, int useCs1, int readyOff, int validOn, int validOff, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_E84_Signal_Options(int netId, out int useCs1, out int readyOff, out int validOn, out int validOff, out int status);
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_E84_Signal_Out_Options(int netId, int nSigHoAvbl, int nSigReq, int nSigReady, out int status);
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_E84_Signal_Out_Options(int netId, out int nSigHoAvbl, out int nSigReq, out int nSigReady, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_LightCurtain_Signal_Options(int netId, int useHoAvbl, int useEs, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_LightCurtain_Signal_Options(int netId, out int useHoAvbl, out int useEs, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_HO_AVBL_Signal(int netId, int hoAvblOn, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_HO_AVBL_Signal(int netId, out int hoAvblOn, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_ES_Signal(int netId, int esSignal, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_ES_Signal(int netId, out int esSignal, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_e84_Signal_Out_Index(int netId, int signalIndex, int signalOn, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_e84_Signal_Out_Index(int netId, int signalIndex, out int signalOn, out int status);

        /*** Timer Options ***/
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_Input_Filter_Time(int netId, int inputFilterTime, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_Input_Filter_Time(int netId, out int inputFilterTime, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_TP_Timeout(int netId, int tp1, int tp2, int tp3, int tp4, int tp5, int tp6, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_TP_Timeout(int netId, out int tp1, out int tp2, out int tp3, out int tp4, out int tp5, out int tp6, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_TD_DelayTime(int netId, int td0, int td1, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_TD_DelayTime(int netId, out int td0, out int td1, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_Heartbeat_Time(int netId, int heartBeat, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_Heartbeat_Time(int netId, out int heartBeat, out int status);

        /*** Event code ***/
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Current_Status(int netId, out int controllerStatus, out int sequenceStep, out int sequenceSub, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Report_All_Status(int netId, out int controllerStatus, out int sequenceStep, out int sequenceSub, out int e84Inputs, out int e84Outputs, out int auxInputs, out int auxOutputs, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_Clear_Event(int netId, int clear, out int status);

        /*** Communication Config ***/
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_Communication(int NetId, int timeOut, int retry, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_Communication(int NetId, out int timeOut, out int retry, out int status);
        #endregion

        #region <remarks> Event </remarks>
        public event E84ErrorOccuredEvent E84ErrorOccured;
        public event E84ModeChangedEvent E84ModeChanged;
        #endregion

        #region property
        /// <summary>
        /// E84 Network Id (IP 4)
        /// </summary>
        public int NetId { get; protected set; }

        /// <summary>
        ///  Whether the device is disconnected or not disconnected after communication
        /// </summary>
        public bool IsDisconnected { get; set; } = false;

        /// <summary>
        /// Communication interval time
        /// </summary>
        public int Interval { get; set; } = 100;

        /// <summary>
        /// E84 communication type. UDP or serial
        /// </summary>
        private E84ComType _ComType;
        public E84ComType ComType
        {
            get { return _ComType; }
            private set
            {
                if (value != _ComType)
                {
                    _ComType = value;
                    RaisePropertyChanged();
                }
            }
        }
        /// <summary>
        /// Connection status
        /// </summary>
        private E84ComStatus _Connection;
        public E84ComStatus Connection
        {
            get { return _Connection; }
            protected set
            {
                if (value != _Connection)
                {
                    _Connection = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// E84 communication error code.
        /// </summary>
        public int ComErrorCode { get; set; }

        /// <summary>
        /// E84 Os version
        /// </summary>
        public int OsVersion { get; private set; }

        /// <summary>
        /// E84 run mode
        /// </summary>
        private E84Mode _RunMode = E84Mode.UNDEFIND;
        public E84Mode RunMode
        {
            get { return _RunMode; }
            private set
            {
                if (value != _RunMode)
                {
                    _RunMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        //SetUp page 에서만 옵션 정보들을 얻어오기 위해 사용하는 프로퍼티
        private bool _isGetOption { get; set; }
        private bool _isChangedEventNumber { get; set; }
        /// <summary>
        /// E84 Event number
        /// </summary>
        private int _EventNumber;
        public int EventNumber
        {
            get { return _EventNumber; }
            private set
            {
                if (value != _EventNumber)
                {
                    _isChangedEventNumber = true;
                    _EventNumber = value;
                    RaisePropertyChanged();
                    E84ErrorOccured(_EventNumber);
                }
            }
        }

        /// <summary>
        /// E84 Mode Change Trigger
        /// </summary>
        private int _ChangedMode = -1;
        public int ChangedMode
        {
            get { return _ChangedMode; }
            private set
            {
                if (value != _ChangedMode)
                {
                    _ChangedMode = value;
                    RaisePropertyChanged();
                    E84ModeChanged(_ChangedMode);
                }
            }
        }

        /// <summary>
        /// E84 Input signals
        /// </summary>
        private E84SignalInput _E84Inputs = new E84SignalInput(false);
        public E84SignalInput E84Inputs
        {
            get { return _E84Inputs; }
            private set
            {
                if (value != _E84Inputs)
                {
                    _E84Inputs = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// E84 Output signals
        /// </summary>
        private E84SignalOutput _E84Outputs = new E84SignalOutput(false);
        public E84SignalOutput E84Outputs
        {
            get { return _E84Outputs; }
            private set
            {
                if (value != _E84Outputs)
                {
                    _E84Outputs = value;
                    RaisePropertyChanged();
                }
            }
        }


        /// <summary>
        /// E84 Aux inputs
        /// </summary>
        private ObservableCollection<bool> _AuxInputs;
        public ObservableCollection<bool> AuxInputs
        {
            get { return _AuxInputs; }
            private set
            {
                if (value != _AuxInputs)
                {
                    _AuxInputs = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// E84 Aux outputs
        /// </summary>
        private ObservableCollection<bool> _AuxOutputs;
        public ObservableCollection<bool> AuxOutputs
        {
            get { return _AuxOutputs; }
            private set
            {
                if (value != _AuxOutputs)
                {
                    _AuxOutputs = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// E84 Aux option 
        /// </summary>
        private ObservableCollection<bool> _AuxInputOptions;
        public ObservableCollection<bool> AuxInputOptions
        {
            get { return _AuxInputOptions; }
            private set
            {
                if (value != _AuxInputOptions)
                {
                    _AuxInputOptions = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// E84 Aux output option
        /// </summary>
        private ObservableCollection<bool> _AuxOutputOptions;
        public ObservableCollection<bool> AuxOutputOptions
        {
            get { return _AuxOutputOptions; }
            private set
            {
                if (value != _AuxOutputOptions)
                {
                    _AuxOutputOptions = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// E84 Aux Input reverse option
        /// </summary>
        private ObservableCollection<bool> _AuxInputReverse;
        public ObservableCollection<bool> AuxInputReverse
        {
            get { return _AuxInputReverse; }
            private set
            {
                if (value != _AuxInputReverse)
                {
                    _AuxInputReverse = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// E84 Aux Output reverse option
        /// </summary>
        private ObservableCollection<bool> _AuxOutputReverse;
        public ObservableCollection<bool> AuxOutputReverse
        {
            get { return _AuxOutputReverse; }
            private set
            {
                if (value != _AuxOutputReverse)
                {
                    _AuxOutputReverse = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check if LP1 is used
        /// </summary>
        private bool _UseLP1;
        public bool UseLP1
        {
            get { return _UseLP1; }
            private set
            {
                if (value != _UseLP1)
                {
                    _UseLP1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check if carrier exist
        /// </summary>
        private ObservableCollection<bool> _Carrier;
        public ObservableCollection<bool> Carrier
        {
            get { return _Carrier; }
            private set
            {
                if (value != _Carrier)
                {
                    _Carrier = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Get clamp signal
        /// </summary>
        private ObservableCollection<bool> _Clamp;
        public ObservableCollection<bool> Clamp
        {
            get { return _Clamp; }
            private set
            {
                if (value != _Clamp)
                {
                    _Clamp = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check if clamp is used
        /// </summary>
        private bool _UseClamp;
        public bool UseClamp
        {
            get { return _UseClamp; }
            private set
            {
                if (value != _UseClamp)
                {
                    _UseClamp = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check clamp communication type : wired sensor or pc command
        /// </summary>
        private bool _ClampComType;
        public bool ClampComType
        {
            get { return _ClampComType; }
            private set
            {
                if (value != _ClampComType)
                {
                    _ClampComType = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check if clamp off event could be happened
        /// </summary>
        private bool _DisableClampEvent;
        public bool DisableClampEvent
        {
            get { return _DisableClampEvent; }
            private set
            {
                if (value != _DisableClampEvent)
                {
                    _DisableClampEvent = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check wait time for clamp - off
        /// </summary>
        private int _ClampOffWaitTime;
        public int ClampOffWaitTime
        {
            get { return _ClampOffWaitTime; }
            private set
            {
                if (value != _ClampOffWaitTime)
                {
                    _ClampOffWaitTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check if CS_1 is used
        /// </summary>
        private bool _UseCs1;
        public bool UseCs1
        {
            get { return _UseCs1; }
            private set
            {
                if (value != _UseCs1)
                {
                    _UseCs1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        /// <summary>
        /// check if Ready off sequence event is disabled
        /// </summary>
        private bool _DisableReadOffEvent;
        public bool DisableReadOffEvent
        {
            get { return _DisableReadOffEvent; }
            private set
            {
                if (value != _DisableReadOffEvent)
                {
                    _DisableReadOffEvent = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check if Valid on sequence event is disabled
        /// </summary>
        private bool _DisableValidOnEvent;
        public bool DisableValidOnEvent
        {
            get { return _DisableValidOnEvent; }
            private set
            {
                if (value != _DisableValidOnEvent)
                {
                    _DisableValidOnEvent = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check if Valid off sequence event is diabled 
        /// </summary>
        private bool _DisableValidOffEvent;
        public bool DisableValidOffEvent
        {
            get { return _DisableValidOffEvent; }
            private set
            {
                if (value != _DisableValidOffEvent)
                {
                    _DisableValidOffEvent = value;
                    RaisePropertyChanged();
                }
            }
        }


        /// <summary>
        /// when error occoured, control ho avbl sig not on controller 
        /// </summary>
        private bool _ControlHoAvblSigOff;
        public bool ControlHoAvblSigOff
        {
            get { return _ControlHoAvblSigOff; }
            private set
            {
                if (value != _ControlHoAvblSigOff)
                {
                    _ControlHoAvblSigOff = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// when error occoured, control ready sig not on controller 
        /// </summary>
        private bool _ControlReadySigOff;
        public bool ControlReadySigOff
        {
            get { return _ControlReadySigOff; }
            private set
            {
                if (value != _ControlReadySigOff)
                {
                    _ControlReadySigOff = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// when error occoured, control req sig not on controller 
        /// </summary>
        private bool _ControlReqSigOff;
        public bool ControlReqSigOff
        {
            get { return _ControlReqSigOff; }
            private set
            {
                if (value != _ControlReqSigOff)
                {
                    _ControlReqSigOff = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check if HO_AVBL is linked action signal
        /// </summary>
        private bool _HoAvblIsLinkedLightCurtain;
        public bool HoAvblIsLinkedLightCurtain
        {
            get { return _HoAvblIsLinkedLightCurtain; }
            private set
            {
                if (value != _HoAvblIsLinkedLightCurtain)
                {
                    _HoAvblIsLinkedLightCurtain = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check if ES is linked action signal
        /// </summary>
        private bool _EsIsLinkedLightCurtain;
        public bool EsIsLinkedLightCurtain
        {
            get { return _EsIsLinkedLightCurtain; }
            private set
            {
                if (value != _EsIsLinkedLightCurtain)
                {
                    _EsIsLinkedLightCurtain = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// AuxInputs[3]
        /// </summary>
        private bool _LightCurtainAuxFlag;
        public bool LightCurtainAuxFlag
        {
            get { return _LightCurtainAuxFlag; }
            set
            {
                if (value != _LightCurtainAuxFlag)
                {
                    _LightCurtainAuxFlag = value;
                    RaisePropertyChanged();
                }
            }
        }


        /// <summary>
        /// Check input filter time
        /// </summary>
        private int _InputFilterTime;
        public int InputFilterTime
        {
            get { return _InputFilterTime; }
            private set
            {
                if (value != _InputFilterTime)
                {
                    _InputFilterTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check heart beat time (minimun communiction interval time)
        /// </summary>
        private int _HeartBeatTime;
        public int HeartBeatTime
        {
            get { return _HeartBeatTime; }
            private set
            {
                if (value != _HeartBeatTime)
                {
                    _HeartBeatTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check Tp timer time
        /// </summary>
        private ObservableCollection<int> _TimerTp;
        public ObservableCollection<int> TimerTp
        {
            get { return _TimerTp; }
            private set
            {
                if (value != _TimerTp)
                {
                    _TimerTp = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check td delay time
        /// </summary>
        private ObservableCollection<int> _TimerTd;
        public ObservableCollection<int> TimerTd
        {
            get { return _TimerTd; }
            private set
            {
                if (value != _TimerTd)
                {
                    _TimerTd = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// RecoveryTimeout
        /// </summary>
        private int _RecoveryTimeout;
        public int RecoveryTimeout
        {
            get { return _RecoveryTimeout; }
            private set
            {
                if (value != _RecoveryTimeout)
                {
                    _RecoveryTimeout = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// RetryCount
        /// </summary>
        private int _RetryCount;
        public int RetryCount
        {
            get { return _RetryCount; }
            private set
            {
                if (value != _RetryCount)
                {
                    _RetryCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check current E84 step
        /// </summary>
        private E84Steps _CurrentStep = E84Steps.UNDEFIND;
        public E84Steps CurrentStep
        {
            get { return _CurrentStep; }
            set
            {
                if (value != _CurrentStep)
                {
                    _CurrentStep = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Check current E84 sub step
        /// </summary>
        private E84SubSteps _CurrentSubStep;

        public E84SubSteps CurrentSubStep
        {
            get { return _CurrentSubStep; }
            private set
            {
                _CurrentSubStep = value;
            }
        }



        #endregion

        #region Field
        private bool isExcuteThread = false;
        private Thread thread;
        #endregion

        private E84ModuleParameter Param { get; set; }

        private List<E84PinSignalParameter> _E84PinSignals;

        public List<E84PinSignalParameter> E84PinSignals
        {
            get { return _E84PinSignals; }
            set { _E84PinSignals = value; }
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        public EMotionTekE84CommModule()
        {
        }

        public void SetParameter(E84ModuleParameter param, List<E84PinSignalParameter> e84PinSignal)
        {
            try
            {
                Param = param;
                E84PinSignals = e84PinSignal;
                this.NetId = param.NetID;

                if (param.E84ConnType == E84ConnTypeEnum.SERIAL)
                {
                    this.ComType = E84ComType.SERIAL;
                }
                else if (param.E84ConnType == E84ConnTypeEnum.ETHERNET)
                {
                    this.ComType = E84ComType.UDP;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                InitPropertyArray();
                InitConnect();

                if (Connection != E84ComStatus.CONNECTED)
                {
                    retVal = EventCodeEnum.E84_CONNECT_ERROR;
                    Connection = E84ComStatus.DISCONNECTED;
                    LoggerManager.Debug($"E84Controller Connect Fail. net id = {NetId}");
                    return retVal;
                }

                StartCommunication();

                // (ISSD-3388) 장비 전원 off시 E84모듈 메모리에 저장되어 있는 옵션값들 날라감.
                // Maestro 로드 시에 파라미터에 있는 값으로 옵션 값 set 해주게 추가.
                SetCommunication(Param.RecoveryTimeout.Value, Param.RetryCount.Value);
                SetTpTimeout(Param.TP1.Value, Param.TP2.Value, Param.TP3.Value, Param.TP4.Value, Param.TP5.Value, Param.TP6.Value);
                SetTdDelayTime(Convert.ToInt32(Param.TD0.Value), Convert.ToInt32(Param.TD1.Value));
                SetHeartBeatTime(Param.HeartBeat.Value);
                SetInputFilterTime(Param.InputFilter.Value);
                SetAuxOptions(Param.AUXIN0.Value, Param.AUXIN1.Value, Param.AUXIN2.Value, Param.AUXIN3.Value, Param.AUXIN4.Value, Param.AUXIN5.Value, Param.AUXOUT0.Value);
                SetUseLp1(Param.UseLP1.Value);
                SetClampOptions(Param.UseClamp.Value, Param.ClampComType.Value, Param.DisableClampEvent.Value, Param.ClampOffWaitTime.Value);
                SetLightCurtainSignalOptions(Param.UseHOAVBL.Value, Param.UseES.Value);
                SetE84SignalOptions(Param.UseCS1.Value, Param.ReadyOff.Value, Param.ValidOn.Value, Param.ValidOff.Value);
                SetE84SignalOutOptions(Param.Control_HoAvblOff.Value, Param.Control_ReqOff.Value, Param.Control_ReadyOff.Value);
                SetAuxReverseOption(Param.RVAUXIN0.Value, Param.RVAUXIN1.Value, Param.RVAUXIN2.Value, Param.RVAUXIN3.Value, Param.RVAUXIN4.Value, Param.RVAUXIN5.Value, Param.RVAUXOUT0.Value);

                int[] data = new int[10];
                
                var returnValue = GetAllStatus(out data[0], out data[1], out data[2], out data[3], out data[4], out data[5], out data[6]);
                
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LoggerManager.Debug($"[E84].ThreadCommunication():Failed to get status. error code : " + returnValue.ToString());
                    ComErrorCode = returnValue;
                }
                else
                {
                    //1.1. Mode
                    if (data[0] == (int)E84Mode.MANUAL)
                    {
                        RunMode = E84Mode.MANUAL;
                        EventNumber = 0;
                    }
                    else if (data[0] == (int)E84Mode.AUTO)
                    {
                        RunMode = E84Mode.AUTO;
                        EventNumber = 0;
                    }
                    else
                    {
                        RunMode = E84Mode.AUTO;
                        //EventNumber = data[0];
                    }

                    E84Outputs.E84SignalChangeEvent += ChangedSignal;
                }

                int uselp1 = 0;

                GetUseLp1(out uselp1);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// Initialize array
        /// </summary>
        private void InitPropertyArray()
        {
            AuxInputs = new ObservableCollection<bool>();
            AuxOutputs = new ObservableCollection<bool>();
            AuxInputOptions = new ObservableCollection<bool>();
            AuxOutputOptions = new ObservableCollection<bool>();
            AuxInputReverse = new ObservableCollection<bool>();
            AuxOutputReverse = new ObservableCollection<bool>();

            if (Carrier == null)
            {
                Carrier = new ObservableCollection<bool>();
            }
            else
            {
                Carrier.Clear();
            }

            for (int i = 0; i < (int)E84MaxCount.E84_MAX_LOAD_PORT; i++)
            {
                Carrier.Add(false);
            }

            if (Clamp == null)
            {
                Clamp = new ObservableCollection<bool>();
            }
            else
            {
                Clamp.Clear();
            }

            for (int i = 0; i < (int)E84MaxCount.E84_MAX_LOAD_PORT; i++)
            {
                Clamp.Add(false);
            }

            TimerTd = new ObservableCollection<int>();
            TimerTp = new ObservableCollection<int>();

            RecoveryTimeout = 0;
            RetryCount = 0;

            for (int i = 0; i < (int)E84MaxCount.E84_MAX_AUX_INPUT; i++)
            {
                AuxInputs.Add(false);
            }
            for (int i = 0; i < (int)E84MaxCount.E84_MAX_AUX_OUTPUT; i++)
            {
                AuxOutputs.Add(false);
            }
            for (int i = 0; i < (int)E84MaxCount.E84_MAX_AUX_INPUT; i++)
            {
                AuxInputOptions.Add(false);
            }
            for (int i = 0; i < (int)E84MaxCount.E84_MAX_AUX_OUTPUT; i++)
            {
                AuxOutputOptions.Add(false);
            }
            for (int i = 0; i < (int)E84MaxCount.E84_MAX_AUX_INPUT; i++)
            {
                AuxInputReverse.Add(false);
            }
            for (int i = 0; i < (int)E84MaxCount.E84_MAX_AUX_OUTPUT; i++)
            {
                AuxOutputReverse.Add(false);
            }
            for (int i = 0; i < (int)E84MaxCount.E84_MAX_TIMER_TD; i++)
            {
                TimerTd.Add(0);
            }
            for (int i = 0; i < (int)E84MaxCount.E84_MAX_TIMER_TP; i++)
            {
                TimerTp.Add(0);
            }
        }

        /// <summary>
        /// Start to communication
        /// </summary>
        /// <returns></returns>
        public int StartCommunication()
        {
            int returnValue = 0;
            if (Connection == E84ComStatus.CONNECTED)
            {
                isExcuteThread = true;
                thread = new Thread(new ThreadStart(ThreadCommunication));
                thread.Start();
            }
            else
            {
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Stop communicating with controller
        /// </summary>
        /// <returns>0: Success, -1: Not connected</returns>
        public virtual int StopCommunication()
        {
            try
            {
                if (Connection == (int)E84ComStatus.DISCONNECTED)
                    return -1;

                isExcuteThread = false;
                if (thread != null)
                {
                    thread.Join(5000);
                }
                thread = null;
                Disconnect();

                Connection = E84ComStatus.DISCONNECTED;
                IsDisconnected = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return 0;
        }

        /// <summary>
        /// Connect to controller by UDP
        /// </summary>
        /// <param name="NetId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Connect(int netId, out int data)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            int isConnect = 1;

            try
            {
                e84_Connection(NetId, isConnect, out data, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("Connect", "Failed to disconnect device");
                }
                else
                {
                    NetId = netId;
                }
            }
            catch (Exception ex)
            {
                LoggerManager.Debug("E84Device.Connect" + ex);
                returnValue = -1;
                data = 0;
            }

            return returnValue;
        }

        /// <summary>
        /// Disconnect to controller
        /// </summary>
        /// <returns></returns>
        protected int Disconnect()
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            int data;
            int isConnect = 0;

            if (Connection == E84ComStatus.DISCONNECTED)
                return -2;

            try
            {
                e84_Connection(this.NetId, isConnect, out data, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("Disconnect", "Failed to disconnect device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }
            return returnValue;
        }

        /// <summary>
        /// Get OS version from driver
        /// </summary>
        /// <param name="version">
        /// </param>
        /// <returns>Error Code</returns>
        public int GetOsVersion(out int version)
        {
            int returnValue = 0;

            try
            {
                e84_Get_OS_Version(this.NetId, out version, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetOsVersion", "Failed to get version from device");
                }
            }
            catch (Exception err)
            {
                version = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set E84 Mode
        /// </summary>
        /// <param name="mode">
        /// 0 : Manual <br/>
        /// 1 : Auto
        /// </param>
        /// <returns>Error Code</returns>
        public int SetMode(int mode)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Set_Mode_Auto(this.NetId, mode, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetMode", "Failed to set mode of device");
                }
                else
                {
                    if (mode == 0)
                    {
                        RunMode = E84Mode.MANUAL;
                    }
                    else if (mode == 1)
                    {
                        RunMode = E84Mode.AUTO;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set aux options (purpose of aux I/O)
        /// </summary>
        /// <param name="netId"></param>
        /// <param name="auxInput0">
        /// Carrier exist status of LP0 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxInput1">
        /// Carrier exist status of LP1 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxInput2">
        /// Auxiliary Light Curtain signal <br/>
        /// 0 : not used <br/>
        /// 1 : use as auxiliary light curtain signal
        /// </param>
        /// <param name="auxInput3">
        /// Light Curtain signal <br/>
        /// 0 : not used <br/>
        /// 1 : use as light curtain signal
        /// </param>
        /// <param name="auxInput4">
        /// Clamp status of LP0 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxInput5">
        /// Clamp status of LP1 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxOutput0">
        /// 0 : not used <br/>
        /// 1 : Use as General Purpose Output
        /// </param>
        /// <param name="status"></param>
        /// <returns> error code </returns>
        public int SetAuxOptions(int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Config_Set_Aux_Options(NetId, auxInput0, auxInput1, auxInput2, auxInput3, auxInput4, auxInput5, auxOutput0, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetAuxOptions", "Failed to set aux options of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get aux options (purpose of aux I/O)
        /// </summary>
        /// <param name="netId"></param>
        /// <param name="auxInput0">
        /// Carrier exist status of LP0 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxInput1">
        /// Carrier exist status of LP1 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxInput2">
        /// Auxiliary Light Curtain signal <br/>
        /// 0 : not used <br/>
        /// 1 : use as auxiliary light curtain signal
        /// </param>
        /// <param name="auxInput3">
        /// Light Curtain signal <br/>
        /// 0 : not used <br/>
        /// 1 : use as light curtain signal
        /// </param>
        /// <param name="auxInput4">
        /// Clamp status of LP0 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxInput5">
        /// Clamp status of LP1 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxOutput0">
        /// 0 : not used <br/>
        /// 1 : Use as General Purpose Output
        /// </param>
        /// <param name="status"></param>
        /// <returns> error code </returns>
        public int GetAuxOptions(out int auxInput0, out int auxInput1, out int auxInput2, out int auxInput3, out int auxInput4, out int auxInput5, out int auxOutput0)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                auxInput0 = 0;
                auxInput1 = 0;
                auxInput2 = 0;
                auxInput3 = 0;
                auxInput4 = 0;
                auxInput5 = 0;
                auxOutput0 = 0;
                return -2;
            }

            try
            {
                e84_Config_Get_Aux_Options(NetId, out auxInput0, out auxInput1, out auxInput2, out auxInput3, out auxInput4, out auxInput5, out auxOutput0, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetAuxOptions", "Failed to get aux options of device");
                }
            }
            catch (Exception err)
            {
                auxInput0 = 0;
                auxInput1 = 0;
                auxInput2 = 0;
                auxInput3 = 0;
                auxInput4 = 0;
                auxInput5 = 0;
                auxOutput0 = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Reverse recognition of aux I/O <br/><br/>
        /// 0 : recognize low as low and high as high <br/>
        /// 1 : recognize low as high and high as low <br/>
        /// </summary>
        /// <param name="auxInput0"></param>
        /// <param name="auxInput1"></param>
        /// <param name="auxInput2"></param>
        /// <param name="auxInput3"></param>
        /// <param name="auxInput4"></param>
        /// <param name="auxInput5"></param>
        /// <param name="auxOutput0"></param>
        /// <returns>error code</returns>
        public int SetAuxReverseOption(int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Config_Set_Reverse_Signal(NetId, auxInput0, auxInput1, auxInput2, auxInput3, auxInput4, auxInput5, auxOutput0, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetAuxReverseOption", "Failed to set aux reverse options of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Check reverse recognition option of aux I/O <br/><br/>
        /// 0 : recognize low as low and high as high <br/>
        /// 1 : recognize low as high and high as low
        /// </summary>
        /// <param name="auxInput0"></param>
        /// <param name="auxInput1"></param>
        /// <param name="auxInput2"></param>
        /// <param name="auxInput3"></param>
        /// <param name="auxInput4"></param>
        /// <param name="auxInput5"></param>
        /// <param name="auxOutput0"></param>
        /// <returns>error code</returns>
        public int GetAuxReverseOption(out int auxInput0, out int auxInput1, out int auxInput2, out int auxInput3, out int auxInput4, out int auxInput5, out int auxOutput0)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                auxInput0 = 0;
                auxInput1 = 0;
                auxInput2 = 0;
                auxInput3 = 0;
                auxInput4 = 0;
                auxInput5 = 0;
                auxOutput0 = 0;
                return -2;
            }

            try
            {
                e84_Config_Get_Reverse_Signal(NetId, out auxInput0, out auxInput1, out auxInput2, out auxInput3, out auxInput4, out auxInput5, out auxOutput0, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetAuxReverseOption", "Failed to get aux reverse options of device");
                }
            }
            catch (Exception err)
            {
                auxInput0 = 0;
                auxInput1 = 0;
                auxInput2 = 0;
                auxInput3 = 0;
                auxInput4 = 0;
                auxInput5 = 0;
                auxOutput0 = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set aux output0 <br/>
        /// 0 : off <br/>
        /// 1 : on
        /// </summary>
        /// <param name="output0">
        /// </param>
        /// <returns>Error Code</returns>
        public int SetOutput0(int output0)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Set_Aux_Output_Signal(this.NetId, output0, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetOutput0", "Failed to set output0 of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get aux output0 
        /// </summary>
        /// <param name="output0">
        /// 0 : off <br/>
        /// 1 : on
        /// </param>
        /// <returns>Error Code</returns>
        public int GetOutput0(out int output0)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                output0 = 0;
                return -2;
            }

            try
            {
                e84_Get_Aux_Output_Signal(this.NetId, out output0, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetOutput0", "Failed to get output0 of device");
                }
            }
            catch (Exception err)
            {
                output0 = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Enable or disable load port 1
        /// </summary>
        /// <param name="use">
        /// 0 : disable <br/>
        /// 1 : enable
        /// </param>
        /// <returns> error code </returns>
        public int SetUseLp1(int use)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Config_Set_Use_LP1(this.NetId, use, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetUseLP1", "Failed to set use load port 1 of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Check if load port 1 is used 
        /// </summary>
        /// <param name="use">
        /// 0 : not used <br/>
        /// 1 : used
        /// </param>
        /// <returns> error code </returns>
        public int GetUseLp1(out int use)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                use = 0;
                return -2;
            }

            try
            {
                e84_Config_Get_Use_LP1(this.NetId, out use, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetUseLp1", "Failed to check if load port 1 is used");
                }

                if (use == 0)
                {
                    _UseLP1 = false;
                }
                else if (use == 1)
                {
                    _UseLP1 = true;
                }
            }
            catch (Exception err)
            {
                use = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }


            return returnValue;
        }

        /// <summary>
        /// Setting options of clamp  
        /// </summary>
        /// <param name="use">use or not use clamp<br/>
        /// 0 : use clamp<br/>
        /// 1 : not use clamp
        /// </param>
        /// <param name="inputType">pc command or wired sensor <br/>
        /// 0 : use signal from wired clamp sensor<br/>
        /// 1 : use clamp status command from host pc
        /// </param>
        /// <param name="actionType"> Decide whether event is occured or not when clamp is used <br/> 
        /// 0 : When Clamp use option set, Before ‘READY’ on to OHT, if clamp is not off status, event occurs <br/>
        /// 1 : When Clamp use option set, Before ‘READY’ on to OHT, E84 Controller waits until clamp off status
        /// </param>
        /// <param name="timer">time to wait for clamp off <br/>
        /// if clamp off status is not off during this time, Event will be happened
        /// </param>
        /// <returns> error code </returns>
        public int SetClampOptions(int use, int inputType, int actionType, int timer)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Config_Set_Clamp(NetId, use, inputType, actionType, timer, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetClampOptions", "Failed to set clamp options of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Check options of clamp  
        /// </summary>
        /// <param name="use">use or not use clamp<br/>
        /// 0 : use clamp<br/>
        /// 1 : not use clamp
        /// </param>
        /// <param name="inputType">pc command or wired sensor <br/>
        /// 0 : use signal from wired clamp sensor<br/>
        /// 1 : use clamp status command from host pc
        /// </param>
        /// <param name="actionType"> Decide whether event is occured or not when clamp is used <br/> 
        /// 0 : When Clamp use option set, Before ‘READY’ on to OHT, if clamp is not off status, event occurs <br/>
        /// 1 : When Clamp use option set, Before ‘READY’ on to OHT, E84 Controller waits until clamp off status
        /// </param>
        /// <param name="timer">time to wait for clamp off <br/>
        /// if clamp off status is not off during this time, Event will be happened
        /// </param>
        /// <returns> error code </returns>
        public int GetClampOptions(out int use, out int inputType, out int actionType, out int timer)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                use = 0;
                inputType = 0;
                actionType = 0;
                timer = 0;
                return -2;
            }

            try
            {
                e84_Config_Get_Clamp(NetId, out use, out inputType, out actionType, out timer, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetClampOptions", "Failed to get clamp options of device");
                }
            }
            catch (Exception err)
            {
                use = 0;
                inputType = 0;
                actionType = 0;
                timer = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// on or off clamp signal
        /// </summary>
        /// <param name="loadPortNo">load port number of clamp</param>
        /// <param name="clampOn">on/off clamp signal <br/>
        /// 0 : off
        /// 1 : on
        /// </param>
        /// <returns>error code</returns>
        public int SetClampSignal(int loadPortNo, int clampOn)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Set_Clamp_Signal(NetId, loadPortNo, clampOn, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LoggerManager.Debug($"[E84] COMM_ERROR PORT{Param.FoupIndex} SetClampSignal().");

                    e84_Set_Clamp_Signal(NetId, loadPortNo, clampOn, out returnValue);

                    if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                    {
                        LogErrorMessage("SetClampSignal", "Failed to set clamp signal of device");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Check if clamp signal is on
        /// </summary>
        /// <param name="loadPortNo">load port number of clamp</param>
        /// <param name="clampOn">on/off clamp signal <br/>
        /// 0 : off
        /// 1 : on
        /// </param>
        /// <returns>error code</returns>
        public int GetClampSignal(int loadPortNo, out int clampOn)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                loadPortNo = 0;
                clampOn = 0;
                return -2;
            }

            try
            {
                e84_Get_Clamp_Signal(NetId, loadPortNo, out clampOn, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetClampSignal", "Failed to get clamp signal status of device");
                }
            }
            catch (Exception err)
            {
                loadPortNo = 0;
                clampOn = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// setting carrier exist signal
        /// </summary>
        /// <param name="loadPortNo">load port number of clamp</param>
        /// <param name="carrierExist">on/off clamp signal <br/>
        /// 0 : There is not carrier on the load port <br/>
        /// 1 : There is carrier on the load port
        /// </param>
        /// <returns>error code</returns>
        public int SetCarrierSignal(int loadPortNo, int carrierExist)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Set_Carrier_Signal(NetId, loadPortNo, carrierExist, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LoggerManager.Debug($"[E84] COMM_ERROR PORT{Param.FoupIndex} SetCarrierSignal().");

                    e84_Set_Carrier_Signal(NetId, loadPortNo, carrierExist, out returnValue);

                    if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                    {
                        LogErrorMessage("SetCarrierSignal", "Failed to set carrier signal of device");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Check if carrier exist
        /// </summary>
        /// <param name="loadPortNo">load port number of clamp</param>
        /// <param name="carrierExist">on/off clamp signal <br/>
        /// 0 : There is not carrier on the load port <br/>
        /// 1 : There is carrier on the load port
        /// </param>
        /// <returns></returns>
        public int GetCarrierSignal(int loadPortNo, out int carrierExist)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                carrierExist = 0;
                return -2;
            }

            try
            {
                e84_Get_Carrier_Signal(NetId, loadPortNo, out carrierExist, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LoggerManager.Debug($"[E84] COMM_ERROR PORT{Param.FoupIndex} GetCarrierSignal().");

                    LogErrorMessage("GetCarrierSignal", "Failed to check carrier of device");
                }
            }
            catch (Exception err)
            {
                carrierExist = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Reset E84 output signals and Initialize sequence interface
        /// </summary>
        /// <param name="reset">reset or not </br>
        /// 0 : no action <br/>
        /// 1 : reset E84 interface
        /// </param>
        /// <returns>error code</returns>
        public int ResetE84Interface(int reset)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Reset_E84_Interface(this.NetId, reset, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetUseLP1", "Failed to reset interface of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get E84 input signals and output signals
        /// </summary>
        /// <param name="inputSignals"> input signals (0 : off, 1: on)<br/>
        /// Bit 0 : VALID <br/>
        /// Bit 1 : CS_0 <br/>
        /// Bit 2 : CS_1 <br/>
        /// Bit 3 : AM_AVBL <br/>
        /// Bit 4 : TR_REQ <br/>
        /// Bit 5 : BUSY <br/>
        /// Bit 6 : COMPT <br/>
        /// Bit 7 : CONT <br/>
        /// Bit 8 : GO
        /// </param>
        /// <param name="outputSignals"> output signals <br/>
        /// Bit 0 : L_REQ <br/>
        /// Bit 1 : UL_REQ <br/>
        /// Bit 2 : VA <br/>
        /// Bit 3 : READY <br/>
        /// Bit 4 : VS_0 <br/>
        /// Bit 5 : VS_1 <br/>
        /// Bit 6 : HO_AVBL <br/>
        /// Bit 7 : ES <br/>
        /// Bit 8 : SELECT <br/>
        /// Bit 9 : MODE
        /// </param>
        /// <returns>error code</returns>
        public int GetE84Signals(out int inputSignals, out int outputSignals)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                inputSignals = 0;
                outputSignals = 0;
                return -2;
            }

            try
            {
                e84_Get_E84_Signals(this.NetId, out inputSignals, out outputSignals, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetE84Signals", "Failed to get E84 signals of device");
                }
            }
            catch (Exception err)
            {
                inputSignals = 0;
                outputSignals = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set e84 signal options
        /// </summary>
        /// <param name="useCs1">use or not use cs1 <br/>
        /// 0 : not use CS1 <br/>
        /// 1 : use CS1
        /// </param>
        /// <param name="readyOff">check option of sequence BUSY off -> TR_REQ off -> COMPT on <br/>
        /// 0 : if signals does not follow sequence, Event is occured <br/>
        /// 1 : don't mind sequence. Controller will go to next step
        /// </param>
        /// <param name="validOn">check option of sequence CS on -> VALID on <br/>
        /// 0 : if signals does not follow sequence, Event is occured <br/>
        /// 1 : don't mind sequence. Controller will go to next step
        /// </param>
        /// <param name="validOff">check option of sequence VALID off -> COMPT off -> CS off <br/>
        /// 0 : if signals does not follow sequence, Event is occured <br/> 
        /// 1 : don't mind sequence. Controller will go to next step
        /// </param>
        /// <returns>error code</returns>
        public int SetE84SignalOptions(int useCs1, int readyOff, int validOn, int validOff)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Config_Set_E84_Signal_Options(NetId, useCs1, readyOff, validOn, validOff, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetE84SignalOptions", "Failed to set clamp options of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get e84 signal options
        /// </summary>
        /// <param name="useCs1">use or not use cs1 <br/>
        /// 0 : not use CS1 <br/>
        /// 1 : use CS1
        /// </param>
        /// <param name="readyOff">check option of sequence BUSY off -> TR_REQ off -> COMPT on <br/>
        /// 0 : if signals does not follow sequence, Event is occured <br/>
        /// 1 : don't mind sequence. Controller will go to next step
        /// </param>
        /// <param name="validOn">check option of sequence CS on -> VALID on <br/>
        /// 0 : if signals does not follow sequence, Event is occured <br/>
        /// 1 : don't mind sequence. Controller will go to next step
        /// </param>
        /// <param name="validOff">check option of sequence VALID off -> COMPT off -> CS off <br/>
        /// 0 : if signals does not follow sequence, Event is occured <br/> 
        /// 1 : don't mind sequence. Controller will go to next step
        /// </param>
        /// <returns>error code</returns>
        public int GetE84SignalOptions(out int useCs1, out int readyOff, out int validOn, out int validOff)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                useCs1 = 0;
                readyOff = 0;
                validOn = 0;
                validOff = 0;
                return -2;
            }

            try
            {
                e84_Config_Get_E84_Signal_Options(NetId, out useCs1, out readyOff, out validOn, out validOff, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetE84SignalOptions", "Failed to set clamp options of device");
                }
            }
            catch (Exception err)
            {
                useCs1 = 0;
                readyOff = 0;
                validOn = 0;
                validOff = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set Configuration of E84 Signal Options
        /// </summary>
        /// <param name="nSigHoAvbl"></param>
        /// 0: When Event Status happened, Controller Controls this signal
        /// 1: When Event Status happened, Master Controls this signal
        /// <param name="nSigReq"></param>
        /// 0: When Event Status happened, Controller Controls this signal
        /// 1: When Event Status happened, Master Controls this signal
        /// <param name="nSigReady"></param>
        /// 0: When Event Status happened, Controller Controls this signal
        /// 1: When Event Status happened, Master Controls this signal
        /// <param name="nStatus"></param>
        /// 0: communication ok
        /// 1: disconnect communication
        /// <returns></returns>

        public int SetE84SignalOutOptions(int nSigHoAvbl, int nSigReq, int nSigReady)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                int firmware_ver = 0;
                GetOsVersion(out firmware_ver);

                if (firmware_ver >= 3013)
                {
                    e84_Config_Set_E84_Signal_Out_Options(NetId, nSigHoAvbl, nSigReq, nSigReady, out returnValue);

                    if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                    {
                        LogErrorMessage("SetE84SignalOutOptions", "Failed to set signal out option");
                    }
                }
                else
                {
                    LoggerManager.Debug($"SetE84SignalOutOptions(): Skip to set signal out option. (It can offer upper 3013 version dll. current:{firmware_ver})");
                    return -2;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get Configuration of E84 Signal Options
        /// </summary>
        /// <param name="nSigHoAvbl"></param>
        /// <param name="nSigReq"></param>
        /// <param name="nSigReady"></param>
        /// <param name="nStatus"></param>
        /// <returns></returns>
        public int GetE84SignalOutOptions(out int nSigHoAvbl, out int nSigReq, out int nSigReady)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                nSigHoAvbl = 0;
                nSigReq = 0;
                nSigReady = 0;
                return -2;
            }

            try
            {
                e84_Config_Get_E84_Signal_Out_Options(NetId, out nSigHoAvbl, out nSigReq, out nSigReady, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetE84SignalOptions", "Failed to get signal out option");
                }
            }
            catch (Exception err)
            {
                nSigHoAvbl = 0;
                nSigReq = 0;
                nSigReady = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }


        /// <summary>
        /// link signal to light curtain status
        /// </summary>
        /// <param name="hoAvbl"> link or unlink hoavbl to light curtain status <br/>
        /// 0 : not use <br/>
        /// 1 : linked action signal is HO_AVBL when light curatin status occured
        /// </param>
        /// <param name="es"> link or unlink hoavbl to light curtain status <br/>
        /// 0 : not use <br/>
        /// 1 : linked action signal is ES when light curatin status occured
        /// </param>
        /// <returns></returns>
        public int SetLightCurtainSignalOptions(int hoAvbl, int es)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Config_Set_LightCurtain_Signal_Options(NetId, hoAvbl, es, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetLightCurtainSignalOptions", "Failed to set carrier signal of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// check if signals are linked to light curtain status
        /// </summary>
        /// <param name="hoAvbl"> link or unlink hoavbl to light curtain status <br/>
        /// 0 : not use <br/>
        /// 1 : linked action signal is HO_AVBL when light curatin status occured
        /// </param>
        /// <param name="es"> link or unlink hoavbl to light curtain status <br/>
        /// 0 : not use <br/>
        /// 1 : linked action signal is ES when light curatin status occured
        /// </param>
        /// <returns></returns>
        public int GetLightCuratinSignalOptions(out int hoAvbl, out int es)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                hoAvbl = 0;
                es = 0;
                return -2;
            }

            try
            {
                e84_Config_Get_LightCurtain_Signal_Options(NetId, out hoAvbl, out es, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetLightCuratinSignalOptions", "Failed to check light curtain options");
                }
            }
            catch (Exception err)
            {
                hoAvbl = 0;
                es = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set HO_AVBL signal
        /// </summary>
        /// <param name="hoAvblOn">
        /// 0 : off <br/>
        /// 1 : on
        /// </param>
        /// <returns>error code</returns>
        public int SetHoAvblSignal(int hoAvblOn)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Set_HO_AVBL_Signal(this.NetId, hoAvblOn, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetHoAvblSignal", "Failed to set ho avbl signal of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get HO_AVBL signal
        /// </summary>
        /// <param name="hoAvblOn">
        /// 0 : off
        /// 1 : on
        /// </param>
        /// <returns>error code</returns>
        public int GetHoAvblSignal(out int hoAvblOn)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                hoAvblOn = 0;
                return -2;
            }

            try
            {
                e84_Get_HO_AVBL_Signal(this.NetId, out hoAvblOn, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetHoAvblSignal", "Failed to check ho avbl signal 1 is used");
                }
            }
            catch (Exception err)
            {
                hoAvblOn = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set Es signal
        /// </summary>
        /// <param name="esOn">
        /// 0 : off <br/>
        /// 1 : on
        /// </param>
        /// <returns>error code</returns>
        public int SetEsSignal(int esOn)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Set_ES_Signal(this.NetId, esOn, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetEsSignal", "Failed to set ho avbl signal of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get ES signal
        /// </summary>
        /// <param name="esOn">
        /// 0 : off <br/>
        /// 1 : on
        /// </param>
        /// <returns>error code</returns>
        public int GetEsSignal(out int esOn)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                esOn = 0;
                return -2;
            }

            try
            {
                e84_Get_ES_Signal(this.NetId, out esOn, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetEsSignal", "Failed to check es signal is used");
                }
            }
            catch (Exception err)
            {
                esOn = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set E84 output signal by index
        /// </summary>
        /// <param name="signalIndex">the index of E84 output signal to set
        ///  0 : L_REQ <br/>
        ///  1 : UL_REQ <br/>
        ///  2 : VA <br/>
        ///  3 : READY <br/>
        ///  4 : VS_0 <br/>
        ///  5 : VS_1 <br/>
        ///  6 : HO_AVBL <br/>
        ///  7 : ES <br/>
        ///  8 : SELECT <br/>
        ///  9 : MODE
        ///  </param>
        /// <param name="onOff">
        /// 0 : off <br/>
        /// 1 : on
        /// </param>
        /// <returns></returns>
        public int SetE84OutputSignal(int signalIndex, int onOff)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Set_e84_Signal_Out_Index(NetId, signalIndex, onOff, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetE84OutputSignal", "Failed to set E84 output signal of device");
                }

                // Set OutPut Data
                //bool flag = false;
                //if (onOff == 0) flag = false;
                //else if (onOff == 1) flag = true;
                //E84Outputs[signalIndex] = flag;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Check E84 output signal by index
        /// </summary>
        /// <param name="signalIndex">the index of E84 output signal to check
        ///  0 : L_REQ <br/>
        ///  1 : UL_REQ <br/>
        ///  2 : VA <br/>
        ///  3 : READY <br/>
        ///  4 : VS_0 <br/>
        ///  5 : VS_1 <br/>
        ///  6 : HO_AVBL <br/>
        ///  7 : ES <br/>
        ///  8 : SELECT <br/>
        ///  9 : MODE
        ///  </param>
        /// <param name="onOff">
        /// 0 : off <br/>
        /// 1 : on
        /// </param>
        /// <returns></returns>
        public int GetE84OutputSignal(int signalIndex, out int onOff)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                onOff = 0;
                return -2;
            }
            try
            {
                e84_Get_e84_Signal_Out_Index(NetId, signalIndex, out onOff, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetE84OutputSignal", "Failed to check E84 output signal of device");
                }
            }
            catch (Exception err)
            {
                onOff = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set input filtering time
        /// </summary>
        /// <param name="inputFilterTime">
        /// If signal on(or off) status is sustained during setting time, It is defined as on(or off) status (unit : 10msec)
        /// </param>
        /// <returns></returns>
        public int SetInputFilterTime(int inputFilterTime)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Config_Set_Input_Filter_Time(this.NetId, inputFilterTime, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetInputFilterTime", "Failed to set input filter time of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Check input filter time
        /// </summary>
        /// <param name="inputFilterTime">
        /// If signal on(or off) status is sustained during setting time, It is defined as on(or off) status (unit : 10msec)
        /// </param>
        /// <returns></returns>
        public int GetInputFilterTime(out int inputFilterTime)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                inputFilterTime = 0;
                return -2;
            }

            try
            {
                e84_Config_Get_Input_Filter_Time(this.NetId, out inputFilterTime, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetInputFilterTime", "Failed to get input filter time is used");
                }
            }
            catch (Exception err)
            {
                inputFilterTime = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set tick time of passive timer (unit : sec)
        /// </summary>
        /// <param name="tp1">passive timer 1 tick time(unit : sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param> 
        /// <param name="tp2">passive timer 2 tick time(unit : sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time 
        /// </param>
        /// <param name="tp3">passive timer 3 tick time(unit : sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time 
        /// </param>
        /// <param name="tp4">passive timer 4 tick time(unit : sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time 
        /// </param>
        /// <param name="tp5">passive timer 5 tick time(unit : sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time 
        /// </param>
        /// <param name="tp6">passive timer 6 tick time(unit : sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time 
        /// </param>
        /// <returns></returns>
        public int SetTpTimeout(int tp1, int tp2, int tp3, int tp4, int tp5, int tp6)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Config_Set_TP_Timeout(NetId, tp1, tp2, tp3, tp4, tp5, tp6, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetTpTimeout", "Failed to set tp time out of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get tick time of passive timer (unit : 10 msec)
        /// </summary>
        /// <param name="tp1">passive timer 1 tick time(unit : 10msec)<br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param> 
        /// <param name="tp2">passive timer 2 tick time(unit : 10msec)<br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param> 
        /// <param name="tp3">passive timer 3 tick time(unit : 10msec)<br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param> 
        /// <param name="tp4">passive timer 4 tick time(unit : 10msec)<br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param> 
        /// <param name="tp5">passive timer 5 tick time(unit : 10msec)<br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param> 
        /// <param name="tp6">passive timer 6 tick time(unit : 10msec)<br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param> 
        /// <returns></returns>
        public int GetTpTimeout(out int tp1, out int tp2, out int tp3, out int tp4, out int tp5, out int tp6)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                tp1 = 0;
                tp2 = 0;
                tp3 = 0;
                tp4 = 0;
                tp5 = 0;
                tp6 = 0;
                return -2;
            }

            try
            {
                e84_Config_Get_TP_Timeout(NetId, out tp1, out tp2, out tp3, out tp4, out tp5, out tp6, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetTpTimeout", "Failed to get tp time out of device");
                }
            }
            catch (Exception err)
            {
                tp1 = 0;
                tp2 = 0;
                tp3 = 0;
                tp4 = 0;
                tp5 = 0;
                tp6 = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set tick time of delay timer (unit : sec)
        /// </summary>
        /// <param name="td0">delay timer tick time (unit sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param>
        /// <param name="td1">delay timer tick time (unit sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param>
        /// <returns></returns>
        public int SetTdDelayTime(int td0, int td1)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Config_Set_TD_DelayTime(NetId, td0, td1, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetTdDelayTime", "Failed to set td delay time of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get tick time of delay timer (unit : 10msec)
        /// </summary>
        /// <param name="td0">delay timer tick time (unit 10msec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param>
        /// <param name="td1">delay timer tick time (unit 10msec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param>
        /// <returns></returns>
        public int GetTdDelayTime(out int td0, out int td1)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                td0 = 0;
                td1 = 0;
                return -2;
            }

            try
            {
                e84_Config_Get_TD_DelayTime(NetId, out td0, out td1, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetTdDelayTime", "Failed to get td delay time of device");
                }
            }
            catch (Exception err)
            {
                td0 = 0;
                td1 = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set heartbeat time <br/>
        /// </summary>
        /// <param name="heartBeatTime">
        /// this value decide minimum communication interval (unit : sec)
        /// If There are no communication within this time. Event will be happend <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 120 : time
        /// </param>
        /// <returns>error code</returns>
        public int SetHeartBeatTime(int heartBeatTime)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Config_Set_Heartbeat_Time(this.NetId, heartBeatTime, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetHeartBeatTime", "Failed to set heartbeat time of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get heartbeat time <br/>
        /// </summary>
        /// <param name="heartBeatTime">
        /// this value decide minimum communication interval (unit : 10msec)
        /// If There are no communication within this time. Event will be happend <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 120 : time
        /// </param>
        /// <returns>error code</returns>
        public int GetHeartBeatTime(out int heartBeatTime)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                heartBeatTime = 0;
                return -2;
            }

            try
            {
                e84_Config_Get_Heartbeat_Time(this.NetId, out heartBeatTime, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetHeartBeatTime", "Failed to get input filter time is used");
                }
            }
            catch (Exception err)
            {
                heartBeatTime = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get all status including I/O
        /// </summary>
        /// <param name="controllerStatus">
        /// 0 : manual <br/>
        /// 1 : auto <br/>
        /// above 2 : event number
        /// </param>
        /// <param name="sequenceStep">
        /// 0 : wait HO_AVBL <br/>
        /// 1 : wait CS_0 or CS_1 <br/>
        /// 2 : wait VALID on <br/>
        /// 3 : wait TR_REQ on <br/>
        /// 4 : wait CLAMP off <br/>
        /// 5 : wait BUSY on <br/>
        /// </param>
        /// <param name="sequenceSub">
        /// 0 : initial status <br/>
        /// 1 : after L_REQ On to OHT <br/>
        /// 2 : after CS_0 or CS_1 off from OHT <br/>
        /// 3 : after UL_REQ On to OHT <br/>
        /// </param> 
        /// <param name="e84Inputs">E84 input signals (0 : off, 1: on) <br/>
        /// Bit 0 : VALID <br/>
        /// Bit 1 : CS_0 <br/>
        /// Bit 2 : CS_1 <br/>
        /// Bit 3 : AM_AVBL <br/>
        /// Bit 4 : TR_REQ <br/>
        /// Bit 5 : BUSY <br/>
        /// Bit 6 : COMPT <br/>
        /// Bit 7 : CONT <br/>
        /// Bit 8 : GO
        /// </param>
        /// <param name="e84Outputs">E84 output signals (0 : off, 1: on) <br/>
        /// Bit 0 : L_REQ <br/>
        /// Bit 1 : UL_REQ <br/>
        /// Bit 2 : VA <br/>
        /// Bit 3 : READY <br/>
        /// Bit 4 : VS_0 <br/>
        /// Bit 5 : VS_1 <br/>
        /// Bit 6 : HO_AVBL <br/>
        /// Bit 7 : ES <br/>
        /// Bit 8 : SELECT <br/>
        /// Bit 9 : MODE
        /// </param>
        /// <param name="auxInputs">Aux input signals (0 : off, 1 : on) <br/>
        /// bit 0 : aux input 0 (0 : off, 1: on) <br/>
        /// bit 1 : aux input 1 (0 : off, 1: on) <br/>
        /// bit 2 : aux input 2 (0 : off, 1: on) <br/>
        /// bit 3 : aux input 3 (0 : off, 1: on) <br/>
        /// bit 4 : aux input 4 (0 : off, 1: on) <br/>
        /// bit 5 : aux input 5 (0 : off, 1: on) 
        /// </param>
        /// <param name="auxOutputs"> aux output 0 (0 : off, 1 : on)</param>
        /// <returns>error code</returns>
        public int GetAllStatus(out int controllerStatus, out int sequenceStep, out int sequenceSub, out int e84Inputs, out int e84Outputs, out int auxInputs, out int auxOutputs)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                controllerStatus = 0;
                sequenceStep = 0;
                sequenceSub = 0;
                e84Inputs = 0;
                e84Outputs = 0;
                auxInputs = 0;
                auxOutputs = 0;
                return -2;
            }

            try
            {
                e84_Get_Report_All_Status(NetId, out controllerStatus, out sequenceStep, out sequenceSub, out e84Inputs, out e84Outputs, out auxInputs, out auxOutputs, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetAllStatus", "Failed to get all status of device");
                }
            }
            catch (Exception err)
            {
                controllerStatus = 0;
                sequenceStep = 0;
                sequenceSub = 0;
                e84Inputs = 0;
                e84Outputs = 0;
                auxInputs = 0;
                auxOutputs = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Clear event
        /// </summary>
        /// <param name="clear">
        /// 0 : no action
        /// 1 : clear
        /// </param>
        /// <returns></returns>
        public int SetClearEvent(int clear)
        {
            int returnValue = 0;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Set_Clear_Event(this.NetId, clear, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetHeartBeatTime", "Failed to set heartbeat time of device");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        public int GetEventNumber()
        {
            return EventNumber;
        }
        public bool GetLightCurtainSignal()
        {
            return LightCurtainAuxFlag;
        }
        public void InitConnect()
        {
            try
            {
                int data = -1;
                int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

                // Connect to device.
                Connection = E84ComStatus.CONNECTING;
                returnValue = Connect(NetId, out data);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LoggerManager.Debug($"[E84].ThreadCommunication() : Failed to connect device . error code {returnValue.ToString()}");
                    ComErrorCode = returnValue;
                    returnValue = Disconnect();
                    Connection = (int)E84ComStatus.DISCONNECTED;
                    return;
                }

                // Get os version
                returnValue = GetOsVersion(out data);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LoggerManager.Debug($"[E84].ThreadCommunication() : Failed to get os version.");
                    returnValue = Disconnect();
                    Connection = (int)E84ComStatus.DISCONNECTED;
                    return;
                }

                OsVersion = data;

                // Start communication.
                IsDisconnected = false;
                ComErrorCode = (int)e84ErrorCode.E84_ERROR_SUCCESS;
                Connection = E84ComStatus.CONNECTED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// Thread for communicating with E84
        /// </summary>
        /// <param name="arg"></param>
        protected virtual void ThreadCommunication()
        {
            try
            {
                int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;
                int[] data = new int[10];
                int preMode = -1;

                var hash = this.GetHashCode();
                
                while (isExcuteThread)
                {
                    try
                    {
                        Thread.Sleep(100);

                        #region Device status
                        //1. Device status
                        returnValue = GetAllStatus(out data[0], out data[1], out data[2], out data[3], out data[4], out data[5], out data[6]);
                        
                        if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                        {
                            LoggerManager.Debug($"[E84].ThreadCommunication():Failed to get status. NetID:{NetId} error code : " + returnValue.ToString());
                            ComErrorCode = returnValue;
                        }
                        else
                        {
                            //1.1. Mode
                            if (data[0] == (int)E84Mode.MANUAL)
                            {
                                RunMode = E84Mode.MANUAL;
                                EventNumber = 0;
                            }
                            else if (data[0] == (int)E84Mode.AUTO)
                            {
                                RunMode = E84Mode.AUTO;
                                EventNumber = 0;
                            }
                            else
                            {
                                RunMode = E84Mode.MANUAL;
                                EventNumber = data[0];
                            }

                            //1.1.1 PreMode , CurMode Check.
                            if (preMode != (int)RunMode && preMode > -1)
                            {
                                ChangedMode = (int)RunMode;
                            }
                            //preMode = data[0];
                            preMode = (int)RunMode;

                            //1.2. Current step
                            if (Enum.IsDefined(typeof(E84Steps), data[1]))
                            {
                                CurrentStep = (E84Steps)data[1];
                            }
                            else
                            {
                                if (_isChangedEventNumber)
                                {
                                    LoggerManager.Debug($"[E84].ThreadCommunication():Invalid step number {data[1].ToString()}");
                                    _isChangedEventNumber = false;
                                }
                            }

                            //1.3. Current sub step
                            if (Enum.IsDefined(typeof(E84SubSteps), data[2]))
                            {
                                if (CurrentSubStep != (E84SubSteps)data[2])
                                {
                                    CurrentSubStep = (E84SubSteps)data[2];
                                    LoggerManager.Debug($"#{Param.FoupIndex}.[E84SubStep] is {CurrentSubStep}");
                                }
                            }
                            else
                            {
                                //LoggerManager.Debug($"[E84].ThreadCommunication():Invalid sub step number {data[2].ToString()}");
                            }

                            //1.4. E84 input
                            for (int bitIndex = 0; bitIndex < (int)E84MaxCount.E84_MAX_E84_INPUT; bitIndex++)
                            {
                                var prev = bool.Parse(E84Inputs[bitIndex].ToString());
                                if (((data[3] >> bitIndex) & 0x01) == 1)
                                {
                                    E84Inputs[bitIndex] = true;

                                    if (prev != true)
                                    {
                                        var signalType = PinConvertSignalType(E84SignalActiveEnum.INPUT, bitIndex);
                                        LoggerManager.Debug($"[E84] {Param.E84OPModuleType}.PORT{Param.FoupIndex} INPUT [{signalType}] ON");

                                    }
                                }
                                else
                                {
                                    E84Inputs[bitIndex] = false;

                                    if (prev != false)
                                    {
                                        var signalType = PinConvertSignalType(E84SignalActiveEnum.INPUT, bitIndex);
                                        LoggerManager.Debug($"[E84] {Param.E84OPModuleType}.PORT{Param.FoupIndex} INPUT [{signalType}] OFF");
                                    }
                                }
                            }

                            //1.5. E84 output
                            for (int bitIndex = 0; bitIndex < (int)E84MaxCount.E84_MAX_E84_OUTPUT; bitIndex++)
                            {
                                if (((data[4] >> bitIndex) & 0x01) == 1)
                                {
                                    E84Outputs[bitIndex] = true;
                                }
                                else
                                {
                                    E84Outputs[bitIndex] = false;
                                }
                            }

                            //1.6. Aux input
                            for (int bitIndex = 0; bitIndex < (int)E84MaxCount.E84_MAX_AUX_INPUT; bitIndex++)
                            {
                                if (((data[5] >> bitIndex) & 0x01) == 1)
                                {
                                    AuxInputs[bitIndex] = true;
                                }
                                else
                                {
                                    AuxInputs[bitIndex] = false;
                                }

                                if (bitIndex == 3)
                                {
                                    LightCurtainAuxFlag = AuxInputs[bitIndex];
                                }
                            }

                            //1.7. Aux output
                            for (int bitIndex = 0; bitIndex < (int)E84MaxCount.E84_MAX_AUX_OUTPUT; bitIndex++)
                            {
                                if (((data[6] >> bitIndex) & 0x01) == 1)
                                {
                                    AuxOutputs[bitIndex] = true;
                                }
                                else
                                {
                                    AuxOutputs[bitIndex] = false;
                                }
                            }
                        }
                        #endregion

                        //6,7. Get load port signal
                        if (_UseLP1)
                        {
                            for (int loadPortIndex = 0; loadPortIndex < (int)E84MaxCount.E84_MAX_LOAD_PORT; loadPortIndex++)
                            {
                                //6. Get clamp signal
                                Array.Clear(data, 0, data.Length);
                                Thread.Sleep(10);

                                returnValue = GetClampSignal(loadPortIndex, out data[0]);

                                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                                {
                                    LoggerManager.Debug($"[E84].ThreadCommunication():Failed to get clamp. error code : {returnValue.ToString()}");
                                    ComErrorCode = returnValue;
                                }
                                else
                                {
                                    if (data[0] == 1)
                                    {
                                        Clamp[loadPortIndex] = true;
                                    }
                                    else
                                    {
                                        Clamp[loadPortIndex] = false;
                                    }
                                }

                                //7. Get Carrier exist signal
                                Array.Clear(data, 0, data.Length);
                                Thread.Sleep(10);

                                returnValue = GetCarrierSignal(loadPortIndex, out data[0]);

                                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                                {
                                    LoggerManager.Debug($"[E84].ThreadCommunication():Failed to check carrier existence. error code : {returnValue.ToString()}");
                                    ComErrorCode = returnValue;
                                }
                                else
                                {
                                    if (data[0] == 1)
                                    {
                                        Carrier[loadPortIndex] = true;
                                    }
                                    else
                                    {
                                        Carrier[loadPortIndex] = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            int loadPortIndex = 0;
                            int carrierExistFlag;
                            int clampFlag;
                            Thread.Sleep(10);

                            returnValue = GetCarrierSignal(loadPortIndex, out carrierExistFlag);

                            if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                            {
                                LoggerManager.Debug($"[E84].ThreadCommunication():Failed to check carrier existence. error code : {returnValue.ToString()}");
                                ComErrorCode = returnValue;
                            }
                            else
                            {
                                if (carrierExistFlag == 1)
                                {
                                    Carrier[loadPortIndex] = true;
                                }
                                else
                                {
                                    Carrier[loadPortIndex] = false;
                                }
                            }

                            Thread.Sleep(10);

                            returnValue = GetClampSignal(loadPortIndex, out clampFlag);

                            if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                            {
                                LoggerManager.Debug($"[E84].ThreadCommunication():Failed to get clamp. error code : {returnValue.ToString()}");
                                ComErrorCode = returnValue;
                            }
                            else
                            {
                                if (clampFlag == 1)
                                {
                                    Clamp[loadPortIndex] = true;
                                }
                                else
                                {
                                    Clamp[loadPortIndex] = false;
                                }
                            }
                        }

                        if (_isGetOption)
                        {
                            //2. Get Aux options
                            Array.Clear(data, 0, data.Length);
                            Thread.Sleep(10);

                            returnValue = GetAuxOptions(out data[0], out data[1], out data[2], out data[3], out data[4], out data[5], out data[6]);

                            if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                            {
                                LoggerManager.Debug($"[E84].ThreadCommunication():Failed to aux options. error code : {returnValue.ToString()}");
                                ComErrorCode = returnValue;
                            }
                            else
                            {
                                //2.1. Get aux input options
                                for (int auxIndex = 0; auxIndex < (int)E84MaxCount.E84_MAX_AUX_INPUT; auxIndex++)
                                {
                                    if (data[auxIndex] == 1)
                                    {
                                        AuxInputOptions[auxIndex] = true;
                                    }
                                    else
                                    {
                                        AuxInputOptions[auxIndex] = false;
                                    }
                                }

                                //2.2. Get aux output options
                                for (int auxIndex = (int)E84MaxCount.E84_MAX_AUX_INPUT; auxIndex < (int)E84MaxCount.E84_MAX_AUX_IO; auxIndex++)
                                {
                                    if (data[auxIndex] == 1)
                                    {
                                        AuxOutputOptions[(int)E84MaxCount.E84_MAX_AUX_INPUT - auxIndex] = true;
                                    }
                                    else
                                    {
                                        AuxOutputOptions[(int)E84MaxCount.E84_MAX_AUX_INPUT - auxIndex] = false;
                                    }
                                }
                            }

                            //3. Get Aux reverse options
                            Array.Clear(data, 0, data.Length);
                            Thread.Sleep(10);

                            returnValue = GetAuxReverseOption(out data[0], out data[1], out data[2], out data[3], out data[4], out data[5], out data[6]);

                            if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                            {
                                LoggerManager.Debug($"[E84].ThreadCommunication():Failed to aux reverse options. error code : {returnValue.ToString()}");
                                ComErrorCode = returnValue;
                            }
                            else
                            {
                                //3.1. Get aux input reverse options
                                for (int auxIndex = 0; auxIndex < (int)E84MaxCount.E84_MAX_AUX_INPUT; auxIndex++)
                                {
                                    if (data[auxIndex] == 1)
                                    {
                                        AuxInputReverse[auxIndex] = true;
                                    }
                                    else
                                    {
                                        AuxInputReverse[auxIndex] = false;
                                    }
                                }

                                //3.2. Get aux output reverse options
                                for (int auxIndex = (int)E84MaxCount.E84_MAX_AUX_INPUT; auxIndex < (int)E84MaxCount.E84_MAX_AUX_IO; auxIndex++)
                                {
                                    if (data[auxIndex] == 1)
                                    {
                                        AuxOutputReverse[(int)E84MaxCount.E84_MAX_AUX_INPUT - auxIndex] = true;
                                    }
                                    else
                                    {
                                        AuxOutputReverse[(int)E84MaxCount.E84_MAX_AUX_INPUT - auxIndex] = false;
                                    }
                                }
                            }

                            //4. Get use lp1
                            Array.Clear(data, 0, data.Length);
                            Thread.Sleep(10);

                            returnValue = GetUseLp1(out data[0]);

                            if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                            {
                                LoggerManager.Debug($"[E84].ThreadCommunication():Failed to get use lp 1. error code : {returnValue.ToString()}");
                                ComErrorCode = returnValue;
                            }
                            else
                            {
                                if (data[0] == 1)
                                {
                                    UseLP1 = true;
                                }
                                else
                                {
                                    UseLP1 = false;
                                }
                            }

                            //5. Get clamp options
                            Array.Clear(data, 0, data.Length);
                            Thread.Sleep(10);

                            returnValue = GetClampOptions(out data[0], out data[1], out data[2], out data[3]);

                            if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                            {
                                LoggerManager.Debug($"[E84].ThreadCommunication():Failed to get use clamp options. error code : {returnValue.ToString()}");
                                ComErrorCode = returnValue;
                            }
                            else
                            {
                                // 5.1. Get use clamp
                                if (data[0] == 1)
                                {
                                    UseClamp = true;
                                }
                                else
                                {
                                    UseClamp = false;
                                }

                                // 5.2. Clamp com type (sensor or command)
                                if (data[1] == 1)
                                {
                                    ClampComType = true;
                                }
                                else
                                {
                                    ClampComType = false;
                                }

                                // 5.3. Event "Clamp off event"
                                if (data[2] == 1)
                                {
                                    DisableClampEvent = true;
                                }
                                else
                                {
                                    DisableClampEvent = false;
                                }

                                // 5.4. Wait time to clamp off
                                ClampOffWaitTime = data[3];
                            }

                            //8. Get E84 signal options
                            Array.Clear(data, 0, data.Length);
                            Thread.Sleep(10);

                            returnValue = GetE84SignalOptions(out data[0], out data[1], out data[2], out data[3]);

                            if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                            {
                                LoggerManager.Debug($"[E84].ThreadCommunication():Failed to get E84 signal options. error code : {returnValue.ToString()}");
                                ComErrorCode = returnValue;
                            }
                            else
                            {
                                if (data[0] == 0)
                                {
                                    UseCs1 = false;
                                }
                                else
                                {
                                    UseCs1 = true;
                                }

                                if (data[1] == 0)
                                {
                                    DisableReadOffEvent = false;
                                }
                                else
                                {
                                    DisableReadOffEvent = true;
                                }

                                if (data[2] == 0)
                                {
                                    DisableValidOnEvent = false;
                                }
                                else
                                {
                                    DisableValidOnEvent = true;
                                }

                                if (data[3] == 0)
                                {
                                    DisableValidOffEvent = false;
                                }
                                else
                                {
                                    DisableValidOffEvent = true;
                                }
                            }

                            //9. Get light curtain options
                            Array.Clear(data, 0, data.Length);
                            Thread.Sleep(10);

                            returnValue = GetLightCuratinSignalOptions(out data[0], out data[1]);

                            if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                            {
                                LoggerManager.Debug($"[E84].ThreadCommunication():Failed to get lightcurtain signal options. error code : {returnValue.ToString()}");
                                ComErrorCode = returnValue;
                            }
                            else
                            {
                                if (data[0] == 0)
                                {
                                    HoAvblIsLinkedLightCurtain = false;
                                }
                                else
                                {
                                    HoAvblIsLinkedLightCurtain = true;
                                }

                                if (data[1] == 0)
                                {
                                    EsIsLinkedLightCurtain = false;
                                }
                                else
                                {
                                    EsIsLinkedLightCurtain = true;
                                }
                            }

                            //10. GetInputFilterTime
                            Array.Clear(data, 0, data.Length);
                            Thread.Sleep(10);

                            returnValue = GetInputFilterTime(out data[0]);

                            if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                            {
                                LoggerManager.Debug($"[E84].ThreadCommunication():Failed to get input filter time. error code : { returnValue.ToString()}");
                                ComErrorCode = returnValue;
                            }
                            else
                            {
                                InputFilterTime = data[0];
                            }

                            //11. Get tp time out time
                            Array.Clear(data, 0, data.Length);
                            Thread.Sleep(10);

                            returnValue = GetTpTimeout(out data[0], out data[1], out data[2], out data[3], out data[4], out data[5]);

                            if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                            {
                                LoggerManager.Debug($"[E84].ThreadCommunication():Failed to get tp time out. error code : { returnValue.ToString()}");
                                ComErrorCode = returnValue;
                            }
                            else
                            {
                                for (int tpIndex = 0; tpIndex < (int)E84MaxCount.E84_MAX_TIMER_TP; tpIndex++)
                                {
                                    TimerTp[tpIndex] = data[tpIndex];
                                }
                            }

                            //12. Get td delay time
                            Array.Clear(data, 0, data.Length);
                            Thread.Sleep(10);

                            returnValue = GetTdDelayTime(out data[0], out data[1]);

                            if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                            {
                                LoggerManager.Debug($"[E84].ThreadCommunication():Failed to get TD delay time. error code : {returnValue.ToString()}");
                                ComErrorCode = returnValue;
                            }
                            else
                            {
                                for (int tpIndex = 0; tpIndex < (int)E84MaxCount.E84_MAX_TIMER_TD; tpIndex++)
                                {
                                    TimerTd[tpIndex] = data[tpIndex];
                                }
                            }

                            //13. Get heartbeat time
                            Array.Clear(data, 0, data.Length);
                            Thread.Sleep(10);

                            returnValue = GetHeartBeatTime(out data[0]);

                            if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                            {
                                LoggerManager.Debug($"[E84].ThreadCommunication():Failed to get heartbeat time. error code : {returnValue.ToString()}");
                                ComErrorCode = returnValue;
                            }
                            else
                            {
                                HeartBeatTime = data[0];
                            }

                            //14. Get Communication option
                            Thread.Sleep(10);

                            returnValue = GetCommunication(out int timeout, out int retry);

                            if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                            {
                                LoggerManager.Debug($"[E84].ThreadCommunication():Failed to get heartbeat time. error code : {returnValue.ToString()}");
                                ComErrorCode = returnValue;
                            }
                            else
                            {
                                RecoveryTimeout = timeout;
                                RetryCount = retry;
                            }

                            //15. Get E84 out signal options
                            Array.Clear(data, 0, data.Length);
                            Thread.Sleep(10);

                            returnValue = GetE84SignalOutOptions(out data[0], out data[1], out data[2]);

                            if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                            {
                                LoggerManager.Debug($"[E84].ThreadCommunication():Failed to get E84 signal options. error code : {returnValue.ToString()}");
                                ComErrorCode = returnValue;
                            }
                            else
                            {
                                if (data[0] == 0)
                                {
                                    ControlHoAvblSigOff = false;
                                }
                                else
                                {
                                    ControlHoAvblSigOff = true;
                                }

                                if (data[1] == 0)
                                {
                                    ControlReqSigOff = false;
                                }
                                else
                                {
                                    ControlReqSigOff = true;
                                }

                                if (data[2] == 0)
                                {
                                    ControlReadySigOff = false;
                                }
                                else
                                {
                                    ControlReadySigOff = true;
                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                LoggerManager.Debug($"[E84].ThreadCommunication():The communication(network " + NetId.ToString() + ") is exit.");

                // Set communication end flag.
                //CommunicationDevice.ThreadCommunicationEndFlag = true;
                //CommunicationDevice.ThreadCommunication = null;
                //Connection = E84ComStatus.DISCONNECTED;
                //IsDisconnected = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetIsGetOptionFlag(bool flag)
        {
            _isGetOption = flag;
        }

        public void ChangedSignal(string signalName, bool value)
        {
            try
            {
                string signalVal = value ? "ON" : "OFF";
                LoggerManager.Debug($"[E84] {Param.E84OPModuleType}.PORT{Param.FoupIndex} OUTPUT [{signalName}] {signalVal} --");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public E84SignalTypeEnum PinConvertSignalType(E84SignalActiveEnum activeenum, int pin)
        {
            E84SignalTypeEnum type = E84SignalTypeEnum.INVALID;
            try
            {
                if (_E84PinSignals != null)
                {
                    type = _E84PinSignals.Find(param => param.Pin == pin && param.ActiveType == activeenum)?.Signal ?? E84SignalTypeEnum.INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return type;
        }

        public int SetCommunication(int timeOut, int retry)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                return -2;
            }

            try
            {
                e84_Config_Set_Communication(NetId, timeOut, retry, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("SetCommunication", "Failed to get input filter time is used");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        public int GetCommunication(out int timeOut, out int retry)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            if (Connection != E84ComStatus.CONNECTED)
            {
                timeOut = 0;
                retry = 0;
                return -2;
            }

            try
            {
                e84_Config_Get_Communication(NetId, out timeOut, out retry, out returnValue);

                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    LogErrorMessage("GetCommunication", "Failed to get input filter time is used");
                }
            }
            catch (Exception err)
            {
                timeOut = 0;
                retry = 0;
                LoggerManager.Exception(err);
                returnValue = -1;
            }

            return returnValue;
        }

        public void LogErrorMessage(string funcionname, string message, StringBuilder logPath = null)
        {
            if (this.ComType == E84ComType.SERIAL)
            {
                if (logPath != null)
                {
                    LoggerManager.Debug($"[E84].{funcionname}() : {message} (Serial, Path: {logPath})");
                }
                else
                {
                    LoggerManager.Debug($"[E84].{funcionname}() : {message} (Serial)");
                }
            }
            else
            {
                if (logPath != null)
                {
                    LoggerManager.Debug($"[E84].{funcionname}() : {message} (Network ID: {this.NetId}, Path: {logPath})");
                }
                else
                {
                    LoggerManager.Debug($"[E84].{funcionname}() : {message} (Network ID: {this.NetId})");
                }
            }
        }
    }
}
