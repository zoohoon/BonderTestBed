using ProberErrorCode;
using ProberInterfaces.E84;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProberInterfaces.Communication.E84
{
    public delegate void E84SignalChangedEvent(E84SignalActiveEnum activeenum, int pin, bool signal);
    public delegate void E84ErrorOccuredEvent(int errorCode = -1, E84EventCode code = E84EventCode.UNDEFINE);//, bool isActiveRecovery = false);
    public delegate void E84ModeChangedEvent(int mode = -1);
    public interface IE84CommModule
    {
        event E84ErrorOccuredEvent E84ErrorOccured;
        event E84ModeChangedEvent E84ModeChanged;

        void SetParameter(E84ModuleParameter param, List<E84PinSignalParameter> e84PinSignal);

        EventCodeEnum InitModule();

        E84SignalInput E84Inputs { get; }
        E84SignalOutput E84Outputs { get; }
        ObservableCollection<bool> Carrier { get; }
        ObservableCollection<bool> Clamp { get; }
        E84ComStatus Connection { get; }
        E84Steps CurrentStep { get; }
        E84Mode RunMode { get; }
        E84SubSteps CurrentSubStep { get; }

        //int EventNumber { get; set; } // test code //
        void InitConnect();
        int StartCommunication();
        int StopCommunication();
        int SetMode(int mode);
        int SetClearEvent(int clear);
        int GetEventNumber();

        #region <remarks> E84 </remarks>
        int SetE84OutputSignal(int signalIndex, int onOff);
        int GetE84OutputSignal(int signalIndex, out int onOff);

        int SetEsSignal(int esOn);
        int GetEsSignal(out int esOn);

        int SetHoAvblSignal(int hoAvblOn);
        int GetHoAvblSignal(out int hoAvblOn);
        int SetE84SignalOptions(int useCs1, int readyOff, int validOn, int validOff);
        int GetE84SignalOptions(out int useCs1, out int readyOff, out int validOn, out int validOff);

        int SetE84SignalOutOptions(int nSigHoAvbl, int nSigReq, int nSigReady);
        int GetE84SignalOutOptions(out int nSigHoAvbl, out int nSigReq, out int nSigReady);
        int ResetE84Interface(int reset);
        int GetE84Signals(out int inputSignals, out int outputSignals);
        #endregion

        #region <remarks> Aux </remarks>
        int SetOutput0(int output0);
        int GetOutput0(out int output0);
        int SetAuxOptions(int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0);
        int GetAuxOptions(out int auxInput0, out int auxInput1, out int auxInput2, out int auxInput3, out int auxInput4, out int auxInput5, out int auxOutput0);
        int SetAuxReverseOption(int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0);
        int GetAuxReverseOption(out int auxInput0, out int auxInput1, out int auxInput2, out int auxInput3, out int auxInput4, out int auxInput5, out int auxOutput0);
        #endregion

        int SetUseLp1(int use);
        int GetUseLp1(out int use);
        int SetClampOptions(int use, int inputType, int actionType, int timer);
        int GetClampOptions(out int use, out int inputType, out int actionType, out int timer);
        int SetClampSignal(int loadPortNo, int clampOn);
        int GetClampSignal(int loadPortNo, out int clampOn);
        int SetCarrierSignal(int loadPortNo, int carrierExist);
        int GetCarrierSignal(int loadPortNo, out int carrierExist);
        int SetLightCurtainSignalOptions(int hoAvbl, int es);
        int GetLightCuratinSignalOptions(out int hoAvbl, out int es);
        int SetTpTimeout(int tp1, int tp2, int tp3, int tp4, int tp5, int tp6);
        int GetTpTimeout(out int tp1, out int tp2, out int tp3, out int tp4, out int tp5, out int tp6);
        int SetTdDelayTime(int td0, int td1);
        int GetTdDelayTime(out int td0, out int td1);
        int SetHeartBeatTime(int heartBeatTime);
        int GetHeartBeatTime(out int heartBeatTime);
        int SetInputFilterTime(int inputFilterTime);
        int GetInputFilterTime(out int inputFilterTime);
        bool GetLightCurtainSignal();
        void SetIsGetOptionFlag(bool flag);
        int GetCommunication(out int timeOut, out int retry);
        int SetCommunication(int timeOut, int retry);
    }
}