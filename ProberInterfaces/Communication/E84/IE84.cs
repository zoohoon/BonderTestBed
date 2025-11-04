using ProberInterfaces.Communication.E84;

namespace ProberInterfaces.E84
{
    using ProberErrorCode;
    using System.Collections.Generic;

    namespace ProberInterfaces
    {
        public interface IE84Module : IFactoryModule, IModule, IHasSysParameterizable
        {
            IE84Parameters E84SysParam { get; }
            IE84Controller GetE84Controller(int foupindex, E84OPModuleTypeEnum oPModuleTypeEnum);
            E84SignalTypeEnum PinConvertSignalType(E84SignalActiveEnum activeenum, int pin);
            int SignalTypeConvertPin(E84SignalTypeEnum signal);
            EventCodeEnum SetSignal(int foupindex, E84OPModuleTypeEnum oPModuleTypeEnum, E84SignalTypeEnum signal, bool flag);
            EventCodeEnum SetClampSignal(int foupindex, bool onflag);
            bool GetSignal(int foupindex, E84SignalTypeEnum signal);
            bool GetIsCDBypass();
            double GetCDBypassDelayTimeInSec();
            void SetIsCassetteAutoLock(bool flag);
            void SetIsCassetteAutoLockLeftOHT(bool flag);
            E84CassetteLockParam GetE84CassetteLockParam();
            void SetE84CassetteLockParam(E84CassetteLockParam param);
            EventCodeEnum SetFoupCassetteLockOption();
            E84PresenceTypeEnum GetE84PreseceType();
            void SetE84PreseceType(E84PresenceTypeEnum e84PresenceType);
            long GetTimeoutOnPresenceAfterOnExistSensor();
            void SetTimeoutOnPresenceAfterOnExistSensor(long timeout);
            bool CheckE84Attatched(int index);
            EventCodeEnum SetAttatched(int index, E84OPModuleTypeEnum optype);
            EventCodeEnum SetDetached(int index, E84OPModuleTypeEnum optype);
            EventCodeEnum ValidationFoupLoadedState(int foupNumber, ref string foupstateStr);
            EventCodeEnum ValidationFoupUnloadedState(int foupNumber, ref string foupstateStr);
        }
        public interface IE84Controller : IFactoryModule, IE84Comm
        {
            //int FoupIndex { get; set; }
            bool IsChangedE84Mode { get; set; }
            IE84CommModule CommModule { get; }            
            ModuleStateEnum ModulestateEnum { get; set; }
            IE84ModuleParameter E84ModuleParaemter { get; set; }            
            ModuleStateEnum GetModuleStateEnum();
            ModuleStateEnum Execute();
            EventCodeEnum ClearState();
            int ClearEvent();
            void ResetE84Interface();
            int SetMode(int mode);
            EventCodeEnum SetCarrierState();
            EventCodeEnum SetCardStateInBuffer();
            //Stopwatch GetRecoveryAvailable_StopWatch();
            void E84ErrorOccured(int errorCode = -1, E84EventCode code = E84EventCode.UNDEFINE);//, bool isActiveRecovery = true);            
            void SetCardBehaviorStateEnum();
            void SetFoupBehaviorStateEnum(bool init = false);
            EventCodeEnum SetOutput0(bool flag);
            void UpdateCardBufferState(bool forced_event = false);
            EventCodeEnum ConnectCommodule(E84ModuleParameter moduleparam, List<E84PinSignalParameter> e84PinSignal);
            EventCodeEnum DisConnectCommodule();
            void TempSetE84ModeStatus(bool entry);

        }
        public interface IE84Comm
        {
            EventCodeEnum SetSignal(E84SignalTypeEnum signal, bool flag, E84Mode setMode = E84Mode.UNDEFIND);
            bool GetSignal(E84SignalTypeEnum signal);
            EventCodeEnum SetClampSignal(bool onflag);
        }
    }

}
