using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Communication.E84;
using ProberInterfaces.E84;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace E84
{
    public class EmulCommModule : IE84CommModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> Property </remarks>

        //private int _EventNumber;
        //public int EventNumber
        //{
        //    get { return _EventNumber; }
        //    set// test code//
        //    {
        //        if (value != _EventNumber)
        //        {
        //            //_isChangedEventNumber = true;
        //            _EventNumber = value;
        //            RaisePropertyChanged();
        //            E84ErrorOccured(_EventNumber);
        //        }
        //    }
        //}

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
        public E84SubSteps CurrentSubStep { get; private set; }
        /// <summary>
        /// E84 run mode
        /// </summary>
        private E84Mode _RunMode = E84Mode.MANUAL;
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

#pragma warning disable 0067
        public event E84ErrorOccuredEvent E84ErrorOccured;
        public event E84ModeChangedEvent E84ModeChanged;
#pragma warning restore 0067
        #endregion
        public int GetAuxOptions(out int auxInput0, out int auxInput1, out int auxInput2, out int auxInput3, out int auxInput4, out int auxInput5, out int auxOutput0)
        {
            auxInput0 = 0;
            auxInput1 = 0;
            auxInput2 = 0;
            auxInput3 = 0;
            auxInput4 = 0;
            auxInput5 = 0;
            auxOutput0 = 0;
            return 0;
        }

        public int GetAuxReverseOption(out int auxInput0, out int auxInput1, out int auxInput2, out int auxInput3, out int auxInput4, out int auxInput5, out int auxOutput0)
        {
            auxInput0 = 0;
            auxInput1 = 0;
            auxInput2 = 0;
            auxInput3 = 0;
            auxInput4 = 0;
            auxInput5 = 0;
            auxOutput0 = 0;
            return 0;
        }

        public int GetCarrierSignal(int loadPortNo, out int carrierExist)
        {
            carrierExist = 0;
            return 0;
        }

        public int GetClampOptions(out int use, out int inputType, out int actionType, out int timer)
        {
            use = 0;
            inputType = 0;
            actionType = 0;
            timer = 0;
            return 0;
        }

        public int GetClampSignal(int loadPortNo, out int clampOn)
        {
            clampOn = 0;
            return 0;
        }

        public int GetE84OutputSignal(int signalIndex, out int onOff)
        {
            onOff = 0;
            return 0;
        }

        public int GetE84SignalOptions(out int useCs1, out int readyOff, out int validOn, out int validOff)
        {
            useCs1 = 0;
            readyOff = 0;
            validOn = 0;
            validOff = 0;
            return 0;
        }

        public int GetE84SignalOutOptions(out int nSigHoAvbl, out int nSigReq, out int nSigReady)
        {
            nSigHoAvbl = 0;
            nSigReq = 0;
            nSigReady = 0;
            return 0;
        }

        public int GetE84Signals(out int inputSignals, out int outputSignals)
        {
            inputSignals = 0;
            outputSignals = 0;
            return 0;
        }

        public int GetEsSignal(out int esOn)
        {
            esOn = 0;
            return 0;
        }

        public int GetEventNumber()
        {
            return 0;
        }

        public int GetHeartBeatTime(out int heartBeatTime)
        {
            heartBeatTime = 0;
            return 0;
        }

        public int GetHoAvblSignal(out int hoAvblOn)
        {
            hoAvblOn = 0;
            return 0;
        }

        public int GetInputFilterTime(out int inputFilterTime)
        {
            inputFilterTime = 0;
            return 0;
        }

        public int GetLightCuratinSignalOptions(out int hoAvbl, out int es)
        {
            hoAvbl = 0;
            es = 0;
            return 0;
        }

        public bool GetLightCurtainSignal()
        {
            return false;
        }

        public int GetOutput0(out int output0)
        {
            output0 = 0;
            return 0;
        }

        public int GetTdDelayTime(out int td0, out int td1)
        {
            td0 = 0;
            td1 = 0;
            return 0;
        }

        public int GetTpTimeout(out int tp1, out int tp2, out int tp3, out int tp4, out int tp5, out int tp6)
        {
            tp1 = 0;
            tp2 = 0;
            tp3 = 0;
            tp4 = 0;
            tp5 = 0;
            tp6 = 0;
            return 0;
        }

        public int GetUseLp1(out int use)
        {
            use = 0;
            return 0;
        }

        public void InitConnect()
        {
            Connection = E84ComStatus.CONNECTED;
            RunMode = E84Mode.AUTO;
        }

        public int ResetE84Interface(int reset)
        {
            return 0;
        }

        public int SetAuxOptions(int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0)
        {
            return 0;
        }

        public int SetAuxReverseOption(int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0)
        {
            return 0;
        }

        public int SetCarrierSignal(int loadPortNo, int carrierExist)
        {
            return 0;
        }

        public int SetClampOptions(int use, int inputType, int actionType, int timer)
        {
            return 0;
        }

        public int SetClampSignal(int loadPortNo, int clampOn)
        {
            return 0;
        }

        public int SetClearEvent(int clear)
        {
            return 0;
        }

        public int SetE84OutputSignal(int signalIndex, int onOff)
        {
            return 0;
        }

        public int SetE84SignalOptions(int useCs1, int readyOff, int validOn, int validOff)
        {
            return 0;
        }

        public int SetE84SignalOutOptions(int nSigHoAvbl, int nSigReq, int nSigReady)
        {
            return 0;
        }

        public int SetEsSignal(int esOn)
        {
            return 0;
        }

        public int SetHeartBeatTime(int heartBeatTime)
        {
            return 0;
        }

        public int SetHoAvblSignal(int hoAvblOn)
        {
            return 0;
        }

        public int SetInputFilterTime(int inputFilterTime)
        {
            return 0;
        }

        public void SetIsGetOptionFlag(bool flag)
        {
        }

        public int SetLightCurtainSignalOptions(int hoAvbl, int es)
        {
            return 0;
        }

        public int SetMode(int mode)
        {
            if (mode == 0)
                RunMode = E84Mode.MANUAL;
            else if (mode == 1)
                RunMode = E84Mode.AUTO;
            return 0;
        }

        public int SetOutput0(int output0)
        {
            return 0;
        }

        public int SetTdDelayTime(int td0, int td1)
        {
            return 0;
        }

        public int SetTpTimeout(int tp1, int tp2, int tp3, int tp4, int tp5, int tp6)
        {
            return 0;
        }

        public int SetUseLp1(int use)
        {
            return 0;
        }

        public int StartCommunication()
        {
            return 0;
        }

        public int StopCommunication()
        {
            Connection = E84ComStatus.DISCONNECTED;
            return 0;
        }

        public int GetCommunication(out int timeOut, out int retry)
        {
            timeOut = 0;
            retry = 0;
            return 0;
        }

        public int SetCommunication(int timeOut, int retry)
        {
            return 0;
        }

        public void SetParameter(E84ModuleParameter param, List<E84PinSignalParameter> e84PinSignal)
        {
            return;
        }

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }
    }
}