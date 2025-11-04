namespace ProberInterfaces.Communication.E84
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    public delegate void E84SignalChangeEvent(string signalName, bool value);

    public class E84SignalInput : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public event E84SignalChangeEvent E84SignalChangeEvent;

        private bool _Valid;
        public bool Valid
        {
            get { return _Valid; }
            set
            {
                if (value != _Valid)
                {
                    _Valid = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("VALID", _Valid);
                    }
                }
            }
        }

        private bool _CS0;
        public bool CS0
        {
            get { return _CS0; }
            set
            {
                if (value != _CS0)
                {
                    _CS0 = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("CS_0", _CS0);
                    }
                }
            }
        }

        private bool _CS1;
        public bool CS1
        {
            get { return _CS1; }
            set
            {
                if (value != _CS1)
                {
                    _CS1 = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("CS_1", _CS1);
                    }
                }
            }
        }

        private bool _AmAvbl;
        public bool AmAvbl
        {
            get { return _AmAvbl; }
            set
            {
                if (value != _AmAvbl)
                {
                    _AmAvbl = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("AM_AVBL", _AmAvbl);
                    }
                }
            }
        }

        private bool _TrReq;
        public bool TrReq
        {
            get { return _TrReq; }
            set
            {
                if (value != _TrReq)
                {
                    _TrReq = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("TR_REQ", _TrReq);
                    }
                }
            }
        }

        private bool _Busy;
        public bool Busy
        {
            get { return _Busy; }
            set
            {
                if (value != _Busy)
                {
                    _Busy = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("BUSY", _Busy);
                    }
                }
            }
        }

        private bool _Compt;
        public bool Compt
        {
            get { return _Compt; }
            set
            {
                if (value != _Compt)
                {
                    _Compt = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("COMPT", _Compt);
                    }
                }
            }
        }

        private bool _Cont;
        public bool Cont
        {
            get { return _Cont; }
            set
            {
                if (value != _Cont)
                {
                    _Cont = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("CONT", _Cont);
                    }
                }
            }
        }

        private bool _Go;
        public bool Go
        {
            get { return _Go; }
            set
            {
                if (value != _Go)
                {
                    _Go = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("GO", _Go);
                    }
                }
            }
        }

        public E84SignalInput(bool value)
        {
            Valid = value;
            CS0 = value;
            CS1 = value;
            AmAvbl = value;
            TrReq = value;
            Busy = value;
            Compt = value;
            Cont = value;
            Go = value;
        }
        
        public bool this[int index]
        {
            get
            {
                switch (index)
                {
                    case (int)E84SignalInputIndex.VALID:
                        return Valid;
                    case (int)E84SignalInputIndex.CS_0:
                        return CS0;
                    case (int)E84SignalInputIndex.CS_1:
                        return CS1;
                    case (int)E84SignalInputIndex.AM_AVBL:
                        return AmAvbl;
                    case (int)E84SignalInputIndex.TR_REQ:
                        return TrReq;
                    case (int)E84SignalInputIndex.BUSY:
                        return Busy;
                    case (int)E84SignalInputIndex.COMPT:
                        return Compt;
                    case (int)E84SignalInputIndex.CONT:
                        return Cont;
                    case (int)E84SignalInputIndex.GO:
                        return Go;
                    default:
                        return false;
                }
            }
            set
            {
                switch (index)
                {
                    case (int)E84SignalInputIndex.VALID:
                        this.Valid = value;
                        break;
                    case (int)E84SignalInputIndex.CS_0:
                        this.CS0 = value;
                        break;
                    case (int)E84SignalInputIndex.CS_1:
                        this.CS1 = value;
                        break;
                    case (int)E84SignalInputIndex.AM_AVBL:
                        this.AmAvbl = value;
                        break;
                    case (int)E84SignalInputIndex.TR_REQ:
                        this.TrReq = value;
                        break;
                    case (int)E84SignalInputIndex.BUSY:
                        this.Busy = value;
                        break;
                    case (int)E84SignalInputIndex.COMPT:
                        this.Compt = value;
                        break;
                    case (int)E84SignalInputIndex.CONT:
                        this.Cont = value;
                        break;
                    case (int)E84SignalInputIndex.GO:
                        this.Go = value;
                        break;
                }
            }
        }
    }
    public class E84SignalOutput : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public event E84SignalChangeEvent E84SignalChangeEvent;
        private bool _LReq;
        public bool LReq
        {
            get { return _LReq; }
            set
            {
                if (value != _LReq)
                {
                    _LReq = value;
                    RaisePropertyChanged();
                    if(E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("L_REQ", _LReq);
                    }
                }
            }
        }
        private bool _UlReq;
        public bool UlReq
        {
            get { return _UlReq; }
            set
            {
                if (value != _UlReq)
                {
                    _UlReq = value;
                    RaisePropertyChanged();
                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("UL_REQ", _UlReq);
                    }
                }
            }
        }

        private bool _Va;
        public bool Va
        {
            get { return _Va; }
            set
            {
                if (value != _Va)
                {
                    _Va = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("VA", _Va);
                    }
                }
            }
        }

        private bool _Ready;
        public bool Ready
        {
            get { return _Ready; }
            set
            {
                if (value != _Ready)
                {
                    _Ready = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("READY", _Ready);
                    }
                }
            }
        }

        private bool _VS0;
        public bool VS0
        {
            get { return _VS0; }
            set
            {
                if (value != _VS0)
                {
                    _VS0 = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("VS_0", _VS0);
                    }
                }
            }
        }

        private bool _VS1;
        public bool VS1
        {
            get { return _VS1; }
            set
            {
                if (value != _VS1)
                {
                    _VS1 = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("VS_1", _VS0);
                    }
                }
            }
        }

        private bool _HoAvbl;
        public bool HoAvbl
        {
            get { return _HoAvbl; }
            set
            {
                if (value != _HoAvbl)
                {
                    _HoAvbl = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("HO_AVBL", _HoAvbl);
                    }
                }
            }
        }

        private bool _ES;
        public bool ES
        {
            get { return _ES; }
            set
            {
                if (value != _ES)
                {
                    _ES = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("ES", _ES);
                    }
                }
            }
        }

        private bool _Select;
        public bool Select
        {
            get { return _Select; }
            set
            {
                if (value != _Select)
                {
                    _Select = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("SELECT", _Select);
                    }
                }
            }
        }

        private bool _Mode;
        public bool Mode
        {
            get { return _Mode; }
            set
            {
                if (value != _Mode)
                {
                    _Mode = value;
                    RaisePropertyChanged();

                    if (E84SignalChangeEvent != null)
                    {
                        E84SignalChangeEvent("MODE", _Mode);
                    }
                }
            }
        }

        public E84SignalOutput(bool value)
        {
            LReq = value;
            UlReq = value;
            Va = value;
            Ready = value;
            VS0 = value;
            VS1 = value;
            HoAvbl = value;
            ES = value;
            Select = value;
            Mode = value;
        }

        public bool this[int index]
        {
            get
            {
                switch (index)
                {
                    case (int)E84SignalOutputIndex.L_REQ:
                        return LReq;
                    case (int)E84SignalOutputIndex.U_REQ:
                        return UlReq;
                    case (int)E84SignalOutputIndex.VA:
                        return Va;
                    case (int)E84SignalOutputIndex.READY:
                        return Ready;
                    case (int)E84SignalOutputIndex.VS_0:
                        return VS0;
                    case (int)E84SignalOutputIndex.VS_1:
                        return VS1;
                    case (int)E84SignalOutputIndex.HO_AVBL:
                        return HoAvbl;
                    case (int)E84SignalOutputIndex.ES:
                        return ES;
                    case (int)E84SignalOutputIndex.SELECT:
                        return Select;
                    case (int)E84SignalOutputIndex.MODE:
                        return Mode;
                    default:
                        return false;
                }
            }
            set
            {
                switch (index)
                {
                    case (int)E84SignalOutputIndex.L_REQ:
                        LReq = value;
                        break;
                    case (int)E84SignalOutputIndex.U_REQ:
                        UlReq = value;
                        break;
                    case (int)E84SignalOutputIndex.VA:
                        Va = value;
                        break;
                    case (int)E84SignalOutputIndex.READY:
                        Ready = value;
                        break;
                    case (int)E84SignalOutputIndex.VS_0:
                        VS0 = value;
                        break;
                    case (int)E84SignalOutputIndex.VS_1:
                        VS1 = value;
                        break;
                    case (int)E84SignalOutputIndex.HO_AVBL:
                        HoAvbl = value;
                        break;
                    case (int)E84SignalOutputIndex.ES:
                        ES = value;
                        break;
                    case (int)E84SignalOutputIndex.SELECT:
                        Select = value;
                        break;
                    case (int)E84SignalOutputIndex.MODE:
                        Mode = value;
                        break;
                }
            }
        }
    }
}
