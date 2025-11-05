namespace ProberInterfaces.E84
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    public class E84ModuleParameter : INotifyPropertyChanged, IParamNode, IE84ModuleParameter
    {
        #region <remark> PropertyChanged </remark>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> ISystem & IParam Property </remarks>
        [JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; }
        [JsonIgnore, ParamIgnore]
        public object Owner { get; set; }
        [JsonIgnore, ParamIgnore]
        public List<object> Nodes { get; set; }
        #endregion

        private int _FoupIndex;
        public int FoupIndex
        {
            get { return _FoupIndex; }
            set
            {
                if (value != _FoupIndex)
                {
                    _FoupIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84CommModuleTypeEnum _E84ModuleType;
        public E84CommModuleTypeEnum E84ModuleType
        {
            get { return _E84ModuleType; }
            set
            {
                if (value != _E84ModuleType)
                {
                    _E84ModuleType = value;
                    RaisePropertyChanged();
                }
            }
        }


        private E84ConnTypeEnum _E84ConnType;
        public E84ConnTypeEnum E84ConnType
        {
            get { return _E84ConnType; }
            set
            {
                if (value != _E84ConnType)
                {
                    _E84ConnType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84OPModuleTypeEnum _E84OPModuleType = E84OPModuleTypeEnum.FOUP;
        public E84OPModuleTypeEnum E84OPModuleType
        {
            get { return _E84OPModuleType; }
            set
            {
                if (value != _E84OPModuleType)
                {
                    _E84OPModuleType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<E84Mode> _AccessMode = new Element<E84Mode> { Value = E84Mode.AUTO};
        public Element<E84Mode> AccessMode
        {
            get { return _AccessMode; }
            set
            {
                if (value != _AccessMode)
                {
                    _AccessMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region <remarks> Conn is Serial </remarks>

        private Element<string> _Port = new Element<string>();
        public Element<string> Port
        {
            get { return _Port; }
            set
            {
                if (value != _Port)
                {
                    _Port = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<int> _BaudRate = new Element<int>();
        public Element<int> BaudRate
        {
            get { return _BaudRate; }
            set
            {
                if (value != _BaudRate)
                {
                    _BaudRate = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<int> _DataBits = new Element<int>();
        public Element<int> DataBits
        {
            get { return _DataBits; }
            set
            {
                if (value != _DataBits)
                {
                    _DataBits = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<int> _StopBits = new Element<int>();
        public Element<int> StopBits
        {
            get { return _StopBits; }
            set
            {
                if (value != _StopBits)
                {
                    _StopBits = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region <remarks> Conn is Ethernet </remarks>
        private int _NetID;
        public int NetID
        {
            get { return _NetID; }
            set
            {
                if (value != _NetID)
                {
                    _NetID = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion


        /// <summary>
        /// Period To Monitor the Timer : L_REQ On ~ TR_REQ , U_REQ On ~TR_REQ 
        /// Range(sec): 1 ~ 999
        /// </summary>
        private Element<int> _TP1 = new Element<int>();
        public Element<int> TP1
        {
            get { return _TP1; }
            set
            {
                if (value != _TP1)
                {
                    _TP1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Period To Monitor the Timer : READY On ~ Busy On
        /// Range(sec): 1 ~ 999
        /// </summary>
        private Element<int> _TP2 = new Element<int>();
        public Element<int> TP2
        {
            get { return _TP2; }
            set
            {
                if (value != _TP2)
                {
                    _TP2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Period To Monitor the Timer : BUSY On ~ Carrier Detect , BUSY On ~Carrier Remove
        /// Range(sec): 1 ~ 999
        /// </summary>
        private Element<int> _TP3 = new Element<int>();
        public Element<int> TP3
        {
            get { return _TP3; }
            set
            {
                if (value != _TP3)
                {
                    _TP3 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Period To Monitor the Timer : L_REQ Off ~ BUSY Off, U_REQ Off ~BUSY Off
        /// Range(sec): 1 ~ 999
        /// </summary>
        private Element<int> _TP4 = new Element<int>();
        public Element<int> TP4
        {
            get { return _TP4; }
            set
            {
                if (value != _TP4)
                {
                    _TP4 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Period To Monitor the Timer : READY Off ~ VALID Off 
        /// Range(sec): 1 ~ 999
        /// </summary>
        private Element<int> _TP5 = new Element<int>();
        public Element<int> TP5
        {
            get { return _TP5; }
            set
            {
                if (value != _TP5)
                {
                    _TP5 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Period To Monitor the Timer : VALID Off ~ VALID On(Continuous Handoff)
        /// Range(sec): 1 ~ 999
        /// </summary>
        private Element<int> _TP6 = new Element<int>();
        public Element<int> TP6
        {
            get { return _TP6; }
            set
            {
                if (value != _TP6)
                {
                    _TP6 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Period To Monitor the Timer : CS_X On ~ VALID On
        /// Range(sec): 0.1 ~ 0.2
        /// </summary>
        private Element<double> _TD0 = new Element<double>();
        public Element<double> TD0
        {
            get { return _TD0; }
            set
            {
                if (value != _TD0)
                {
                    _TD0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Period To Monitor the Timer : VALID On ~ VALID Off
        /// Range(sec): 1 ~ 999
        /// </summary>
        private Element<int> _TD1 = new Element<int>();
        public Element<int> TD1
        {
            get { return _TD1; }
            set
            {
                if (value != _TD1)
                {
                    _TD1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// RecoveryTimeout
        /// Default: 150
        /// </summary>
        private Element<int> _RecoveryTimeout = new Element<int>() { Value = 150 };
        public Element<int> RecoveryTimeout
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

        /// <summary>
        /// RetryCount
        /// Default: 3
        /// </summary>
        private Element<int> _RetryCount = new Element<int>() { Value = 3 };
        public Element<int> RetryCount
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

        /// <summary>
        /// HeartBeat
        /// </summary>
        private Element<int> _HeartBeat = new Element<int>();
        public Element<int> HeartBeat
        {
            get { return _HeartBeat; }
            set
            {
                if (value != _HeartBeat)
                {
                    _HeartBeat = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// InputFilter
        /// </summary>
        private Element<int> _InputFilter = new Element<int>();
        public Element<int> InputFilter
        {
            get { return _InputFilter; }
            set
            {
                if (value != _InputFilter)
                {
                    _InputFilter = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// AUXIN0
        /// </summary>
        private Element<int> _AUXIN0 = new Element<int>();
        public Element<int> AUXIN0
        {
            get { return _AUXIN0; }
            set
            {
                if (value != _AUXIN0)
                {
                    _AUXIN0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// AUXIN1
        /// </summary>
        private Element<int> _AUXIN1 = new Element<int>();
        public Element<int> AUXIN1
        {
            get { return _AUXIN1; }
            set
            {
                if (value != _AUXIN1)
                {
                    _AUXIN1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// AUXIN2
        /// </summary>
        private Element<int> _AUXIN2 = new Element<int>();
        public Element<int> AUXIN2
        {
            get { return _AUXIN2; }
            set
            {
                if (value != _AUXIN2)
                {
                    _AUXIN2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// AUXIN3
        /// </summary>
        private Element<int> _AUXIN3 = new Element<int>();
        public Element<int> AUXIN3
        {
            get { return _AUXIN3; }
            set
            {
                if (value != _AUXIN3)
                {
                    _AUXIN3 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// AUXIN4
        /// </summary>
        private Element<int> _AUXIN4 = new Element<int>();
        public Element<int> AUXIN4
        {
            get { return _AUXIN4; }
            set
            {
                if (value != _AUXIN4)
                {
                    _AUXIN4 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// AUXIN5
        /// </summary>
        private Element<int> _AUXIN5 = new Element<int>();
        public Element<int> AUXIN5
        {
            get { return _AUXIN5; }
            set
            {
                if (value != _AUXIN5)
                {
                    _AUXIN5 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// AUXOUT0
        /// </summary>
        private Element<int> _AUXOUT0 = new Element<int>();
        public Element<int> AUXOUT0
        {
            get { return _AUXOUT0; }
            set
            {
                if (value != _AUXOUT0)
                {
                    _AUXOUT0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// UseLP1
        /// </summary>
        private Element<int> _UseLP1 = new Element<int>();
        public Element<int> UseLP1
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
        /// UseES
        /// </summary>
        private Element<int> _UseES = new Element<int>();
        public Element<int> UseES
        {
            get { return _UseES; }
            set
            {
                if (value != _UseES)
                {
                    _UseES = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// UseHOAVBL
        /// </summary>
        private Element<int> _UseHOAVBL = new Element<int>();
        public Element<int> UseHOAVBL
        {
            get { return _UseHOAVBL; }
            set
            {
                if (value != _UseHOAVBL)
                {
                    _UseHOAVBL = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// ReadyOff
        /// </summary>
        private Element<int> _ReadyOff = new Element<int>();
        public Element<int> ReadyOff
        {
            get { return _ReadyOff; }
            set
            {
                if (value != _ReadyOff)
                {
                    _ReadyOff = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// UseCS1
        /// </summary>
        private Element<int> _UseCS1 = new Element<int>();
        public Element<int> UseCS1
        {
            get { return _UseCS1; }
            set
            {
                if (value != _UseCS1)
                {
                    _UseCS1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Use Clamp
        /// </summary>
        private Element<int> _UseClamp = new Element<int>() { Value = 0 };
        public Element<int> UseClamp
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
        /// Use Clamp Comm Type
        /// </summary>
        private Element<int> _ClampComType = new Element<int>() { Value = 0 };
        public Element<int> ClampComType
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
        /// Clamp Off Wait Timer
        /// </summary>
        private Element<int> _ClampOffWaitTime = new Element<int>() { Value = 0 };
        public Element<int> ClampOffWaitTime
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
        /// Use Action Type
        /// </summary>
        private Element<int> _DisableClampEvent = new Element<int>() { Value = 0 };
        public Element<int> DisableClampEvent
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
        /// ValidOff
        /// </summary>
        private Element<int> _ValidOff = new Element<int>();
        public Element<int> ValidOff
        {
            get { return _ValidOff; }
            set
            {
                if (value != _ValidOff)
                {
                    _ValidOff = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// ValidOn
        /// </summary>
        private Element<int> _ValidOn = new Element<int>();
        public Element<int> ValidOn
        {
            get { return _ValidOn; }
            set
            {
                if (value != _ValidOn)
                {
                    _ValidOn = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Control_HoAvblOff
        /// </summary>
        private Element<int> _Control_HoAvblOff = new Element<int>() { Value = 0 };
        public Element<int> Control_HoAvblOff
        {
            get { return _Control_HoAvblOff; }
            set
            {
                if (value != _Control_HoAvblOff)
                {
                    _Control_HoAvblOff = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Control_ReqOff
        /// </summary>
        private Element<int> _Control_ReqOff = new Element<int>() { Value = 0 };
        public Element<int> Control_ReqOff
        {
            get { return _Control_ReqOff; }
            set
            {
                if (value != _Control_ReqOff)
                {
                    _Control_ReqOff = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Control_ReadyOff
        /// </summary>
        private Element<int> _Control_ReadyOff = new Element<int>() { Value = 0 };
        public Element<int> Control_ReadyOff
        {
            get { return _Control_ReadyOff; }
            set
            {
                if (value != _Control_ReadyOff)
                {
                    _Control_ReadyOff = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// RVAUXIN0
        /// </summary>
        private Element<int> _RVAUXIN0 = new Element<int>();
        public Element<int> RVAUXIN0
        {
            get { return _RVAUXIN0; }
            set
            {
                if (value != _RVAUXIN0)
                {
                    _RVAUXIN0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// RVAUXIN1
        /// </summary>
        private Element<int> _RVAUXIN1 = new Element<int>();
        public Element<int> RVAUXIN1
        {
            get { return _RVAUXIN1; }
            set
            {
                if (value != _RVAUXIN1)
                {
                    _RVAUXIN1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// RVAUXIN2
        /// </summary>
        private Element<int> _RVAUXIN2 = new Element<int>();
        public Element<int> RVAUXIN2
        {
            get { return _RVAUXIN2; }
            set
            {
                if (value != _RVAUXIN2)
                {
                    _RVAUXIN2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// RVAUXIN3
        /// </summary>
        private Element<int> _RVAUXIN3 = new Element<int>();
        public Element<int> RVAUXIN3
        {
            get { return _RVAUXIN3; }
            set
            {
                if (value != _RVAUXIN3)
                {
                    _RVAUXIN3 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// RVAUXIN4
        /// </summary>
        private Element<int> _RVAUXIN4 = new Element<int>();
        public Element<int> RVAUXIN4
        {
            get { return _RVAUXIN4; }
            set
            {
                if (value != _RVAUXIN4)
                {
                    _RVAUXIN4 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// RVAUXIN5
        /// </summary>
        private Element<int> _RVAUXIN5 = new Element<int>();
        public Element<int> RVAUXIN5
        {
            get { return _RVAUXIN5; }
            set
            {
                if (value != _RVAUXIN5)
                {
                    _RVAUXIN5 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// RVAUXOUT0
        /// </summary>
        private Element<int> _RVAUXOUT0 = new Element<int>();
        public Element<int> RVAUXOUT0
        {
            get { return _RVAUXOUT0; }
            set
            {
                if (value != _RVAUXOUT0)
                {
                    _RVAUXOUT0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// true : E84 사용 한다, false : E84 사용 안한다.
        /// </summary>
        private bool _E84_Attatched;
        public bool E84_Attatched
        {
            get { return _E84_Attatched; }
            set
            {
                if (value != _E84_Attatched)
                {
                    _E84_Attatched = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
