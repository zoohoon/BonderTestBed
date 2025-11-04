
namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.EnvControl.Enum;
    using ProberInterfaces.GEM;
    using ProberInterfaces.Loader;
    using SecsGemServiceInterface;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using XGEMWrapper;

    //IGEMModule -> IGEMCommManager -> IGemProcessorCore

    //brett// Cell과의 GEM 연결이 끊어진 경우 Loader에 알려주기 위한 Callback Function 대리자 선언
    public delegate void GEMDisconnectDelegate(long lStageID);

    public interface IGEMModule : IGemCommCore, ICommunicationable, IHasSysParameterizable, IModule, IFactoryModule
    {
        new void CommunicationParamApply();
 
        ProberGemIdDictionaryParam DicSVID { get; set; }
        ProberGemIdDictionaryParam DicDVID { get; set; }
        ProberGemIdDictionaryParam DicECID { get; set; }
        IGemSysParameter GemSysParam { get; set; }
        IParam GemAlarmSysParam { get; set; }

        IGEMCommManager GemCommManager { get; set; }
        bool IsExternalLotMode { get; set; }
        /// <summary>
        /// Emul Gem에서 Event number를 보고 자동으로 zup/zdwon/waferunload를 동작하는지에 대한 Flag
        /// true: Emul Gem zup/zdwon/waferunload 전송
        /// false: Emul Gem zup/zdwon/waferunload 전송 안함.
        /// </summary>
        GEMHOST_MANUALACTION GEMHostManualAction { get; set; }
        bool LoadPortModeEnable { get; set; }

        long GetEventNumberFormEventName(string eventName);
        void NotifyValueChanged(long proberValueID, object value,IElement element);
        bool GemEnable();
        void RegisteEvent_OnLoadElementInfo();
        string GetGEMSerialNum();

        void ResetStageDownloadRecipe();
        bool GetComplateDownloadRecipe(List<int> stagelist);
        void SetStageDownloadRecipeResult(int stageindex, bool flag);
        EventCodeEnum ExcuteRemoteCommandAction(RemoteActReqData actReqData, object raiseobject);
        EventCodeEnum ExcuteCarrierCommandAction(CarrierActReqData actReqData, object raiseobject);
        int GetStageStateEnumValue(GEMStageStateEnum stagenum);
        int GetFoupStateEnumValue(GEMFoupStateEnum foupenum);
        int GetCardLPStateEnumValue(GEMFoupStateEnum lpenum);
        GEMFoupStateEnum GetfoupStatEnumType(int number);
        int GetPreHeatStateEnumValue(GEMPreHeatStateEnum preheatenum);
        int GetSmokeSensorStateEnumValue(GEMSensorStatusEnum sensorStatusEnum);
        IPIVContainer GetPIVContainer();
        (string key, GemVidInfo val) FindVidInfo(IDictionary<string, GemVidInfo> vidDic, long vid);
        EventCodeEnum ClearAllAlram();
        EventCodeEnum ClearAlarmOnly();
        
        void SetValue(IElement element);
        void DeleteNotifyEventToElement();
        bool CheckVIDGemEnable(string path);

        void RegistNotifyEventToElement(ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> DBElementDic);
        bool CanExecuteRemoteAction(string command);
        EventCodeEnum SetFixedTrayVID(ModuleID moduleID, IWaferHolder holder);
    }

    public interface IGEMCommManager : IGemCommCore, IGemCommHelper, IFactoryModule
    {
        SecsCommInform SecsCommInformData { get; }
        EventCodeEnum InitConnectService();
        EventCodeEnum DeInitConnectService();
        EventCodeEnum InitGemData();
        bool GetRemoteConnectState(int stageIndex = -1);
        void OnRemoteCommandAction(RemoteActReqData msgData);
        void OnCarrierActMsgRecive(CarrierActReqData msgData);
        void ECVChangeMsgReceive(EquipmentReqData msgData);
        void ECVChangeMsgReceive(long nMsgId, long nCount, long[] pnEcids, string[] psVals);
        EventCodeEnum SetSecsMessageReceiver();
        ISecsGemService GetSecsGemServiceModule();
        EventCodeEnum SetRemoteActionRecipe(Dictionary<string, IGemActBehavior> recipeparam);
        object GetProcessorLockObj();
        object GetLockObj();
        EventCodeEnum ClearAlarmOnly(int cellIndex = 0);
    }

    public interface IGemCommCore : IModule, IDisposable
    {
        long CommEnable();
        long CommDisable();
        long ReqOffLine();
        long ReqLocal();
        long ReqRemote();
        long SetInitControlState_Offline();
        long SetInitControlState_Local();
        long SetInitControlState_Remote();
        long SetInitProberEstablish();
        long SetInitHostEstablish();
        long SetInitCommunicationState_Enable();
        long SetInitCommunicationState_Disable();
        long TimeRequest();
        long SendTerminal(string sendStr);
        long SetEvent(long eventNum);//, int stgnum = -1, List<Dictionary<long, (int objtype, object value)>> ExecutorDataDic = null);
        //long SetEvent(INotifyEvent notifyevent);
        long SetAlarm(long nID, long nState, int cellIndex = 0);
        void CommunicationParamApply();
        bool GetConnectState(int index = 0);
        long SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount);
        long S3F17SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount);
        long S3F27SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount, List<CarrierChangeAccessModeResult> result);
        long MakeListObject(object list);

        void SetGEMDisconnectCallBack(GEMDisconnectDelegate callback);
    }

    public interface IGemCommHelper
    {
        int STAGE_ID_SECTION { get; }
        long GEMSetVariable(int vidLength, long[] convertDataID, string[] values);
        long GEMSetECVChanged(int vidLength, long[] convertDataID, string[] values);
        void SetVariable(long[] vids, string[] values, EnumVidType vidType, bool immediatelyUpdate = false);
        void SetVariables(long nObjectID, long nVid, EnumVidType vidType, bool immediatelyUpdate = false);
        EventCodeEnum StartWcfGemService();
        void ProcessClose();
        long[] GetOrgDataID(params long[] gemIDArray);

    }

    public interface IGemProcessorCore : IModule, IDisposable
    {
        object lockObj { get; }
        bool InitSecsGem(String ConfigPath = @"C:\ProberSystem\Parameters\SystemParam\GEM\GemConfig.cfg");
        EventCodeEnum ConnectService();
        EventCodeEnum DisConnectService();
        EventCodeEnum InitGemData();
        bool GetRemoteConnectState(int stageIndex = -1);
        ClientBase<ISecsGemService> GetSecsGemServiceProxyBase();
        ISecsGemService GetSecsGemServiceProxy();
        void SetSecsGemServiceProxy(ClientBase<ISecsGemService> proxy);
        bool HasSecsGemServiceProxy();
        void SetSecsGemServiceCallback(ISecsGemService callbackObj);
        void Proc_SetCommManager(IGEMCommManager gemManager);
        long Proc_SetVariable(int vidLength, long[] convertDataID, string[] values, bool immediatelyUpdate = false);
        long Proc_SetVariables(long nObjectID, long nVid, bool immediatelyUpdate = false);
        long Proc_SetECVChanged(int vidLength, long[] convertDataID, string[] values);        
        void Proc_SetParamInfoToGem();
        long Proc_SetEstablish(long bState);
        long Proc_SetEvent(long eventNum);//, int stgnum = -1, List<Dictionary<long, (int objtype, object value)>> ExecutorDataDic = null);
        long Proc_ReqOffline();
        long Proc_ReqLocal();
        long Proc_ReqRemote();
        long Proc_TimeRequest();
        long Proc_SendTerminal(string sendStr);
        long Start();
        long Proc_SetAlarm(long nID, long nState, int cellIndex = 0);
        EventCodeEnum Proc_ClearAlarmOnly(int cellIndex = 0);

        void OnRemoteCommandAction(RemoteActReqData msgData);
        bool GetConnectState(int index = 0);
        long SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount);
        long S3F17SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount);
        long S3F27SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount, List<CarrierChangeAccessModeResult> result);
        long MakeListObject(object value);

        void GEMDisconnectCallBack(long lStageID);
        void SetGEMDisconnectCallBack(GEMDisconnectDelegate callback);        
    }

    public interface IGemSysParameter
    {
        Element<bool> Enable { get; set; }
        Element<string> ConfigPath { get; set; }
        Element<string> GEMServiceHostWCFPath { get; set; }
        Element<string> GEMSerialNum { get; set; }
        Element<GemProcessorType> GemProcessrorType { get; set; }
        GemAPIType GEMServiceHostAPIType { get; set; }
        string GemMessageReceiveModulePath { get; set; }
        string ReceiveMessageType { get; set; }
    }
    public enum GemProcessorType
    {
        SINGLE = 0x0000,
        CELL = 0x0010,
        COMMANDER = 0x0011,
    }
    public enum GemAPIType
    {
        UNDEFIND = 0,
        XGEM =1,
        XGEM300 =2
    }
    public enum SecsEnum_ControlState
    {
        UNKNOWN = -1,
        UNDEFIND =0,
        EQ_OFFLINE = 1,
        ATTEMPT_ONLINE = 2,
        HOST_OFFLINE = 3,
        ONLINE_LOCAL = 4,
        ONLINE_REMOTE = 5
    }

    public enum GEMHOST_MANUALACTION
    {
        UNDEFIND = 0,
        CONNECT = 1,
        DISCONNECT = 2,
        RECONNECT = 3
    }

    //public enum SecsEnum_ControlState
    //{
    //    UNKNOWN = -1,
    //    OFFLINE = SecsEnum_ON_OFFLINEState.OFFLINE << 4,
    //    ONLINE_LOCAL = (SecsEnum_ON_OFFLINEState.ONLINE  << 4) + SecsEnum_OnlineSubState.LOCAL,
    //    ONLINE_REMOTE = (SecsEnum_ON_OFFLINEState.ONLINE << 4) + SecsEnum_OnlineSubState.REMOTE,
    //}

    //???? ??? ???? x
    public enum SecsEnum_ON_OFFLINEState
    {
        UNKNOWN = 0x00,
        OFFLINE = 0x01,
        ONLINE = 0x02
    }

    public enum SecsEnum_OfflineSubState
    {
        EQOFFLINE = 0x01,
        ATTEMPTONLINE = 0x02,
        HOSTOFFLINE = 0x03
    }

    //???? ??? ???? x
    public enum SecsEnum_OnlineSubState
    {
        LOCAL = 0x01,
        REMOTE = 0x02
    }

    public enum SecsEnum_CommunicationState
    {
        UNKNOWN = -1,
        UNDEFIND =0,
        COMM_DISABLED = 1,
        WAIT_CR_FROM_HOST = 2,
        WAIT_DELAY = 3,
        WAIT_CRA = 4,
        COMMUNICATING = 5
    }

    public enum SecsEnum_Enable
    {
        UNKNOWN = -1,
        DISABLE = 0,
        ENABLE = 1
    }

    public enum SecsEnum_EstablishSource
    {
        UNKNOWN = -1,
        HOST = 0,
        PROBER = 1
    }

    public enum SecsEnum_Passive
    {
        UNKNOWN = -1,
        ACTIVE,
        PASSIVE
    }

    public class SecsCommInform : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private string _GemProcessVersion;
        public string GemProcessVersion
        {
            get { return _GemProcessVersion; }
            set
            {
                if (_GemProcessVersion != value)
                {
                    _GemProcessVersion = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _HSMSIP;
        public string HSMSIP
        {
            get { return _HSMSIP; }
            set
            {
                if (_HSMSIP != value)
                {
                    _HSMSIP = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _HSMSPort;
        public string HSMSPort
        {
            get { return _HSMSPort; }
            set
            {
                if (_HSMSPort != value)
                {
                    _HSMSPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SecsEnum_Passive _HSMSPassive;
        public SecsEnum_Passive HSMSPassive
        {
            get { return _HSMSPassive; }
            set
            {
                if (_HSMSPassive != value)
                {
                    _HSMSPassive = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _HSMSDeviceID;
        public string HSMSDeviceID
        {
            get { return _HSMSDeviceID; }
            set
            {
                if (_HSMSDeviceID != value)
                {
                    _HSMSDeviceID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _HSMSLinkTestInterval;
        public string HSMSLinkTestInterval
        {
            get { return _HSMSLinkTestInterval; }
            set
            {
                if (_HSMSLinkTestInterval != value)
                {
                    _HSMSLinkTestInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _HSMSRetryLimit;
        public string HSMSRetryLimit
        {
            get { return _HSMSRetryLimit; }
            set
            {
                if (_HSMSRetryLimit != value)
                {
                    _HSMSRetryLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _T3;
        public string T3
        {
            get { return _T3; }
            set
            {
                if (_T3 != value)
                {
                    _T3 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _T5;
        public string T5
        {
            get { return _T5; }
            set
            {
                if (_T5 != value)
                {
                    _T5 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _T6;
        public string T6
        {
            get { return _T6; }
            set
            {
                if (_T6 != value)
                {
                    _T6 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _T7;
        public string T7
        {
            get { return _T7; }
            set
            {
                if (_T7 != value)
                {
                    _T7 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _T8;
        public string T8
        {
            get { return _T8; }
            set
            {
                if (_T8 != value)
                {
                    _T8 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _RequestTimeStr;
        public string RequestTimeStr
        {
            get { return _RequestTimeStr; }
            set
            {
                if (_RequestTimeStr != value)
                {
                    _RequestTimeStr = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SecsEnum_ON_OFFLINEState _InitControlState;
        public SecsEnum_ON_OFFLINEState InitControlState
        {
            get { return _InitControlState; }
            set
            {
                if(value != SecsEnum_ON_OFFLINEState.UNKNOWN)
                {
                    if (_InitControlState != value)
                    {
                        _InitControlState = value;
                        RaisePropertyChanged();
                    }
                }

            }
        }

        private SecsEnum_OfflineSubState _OffLineSubState;
        public SecsEnum_OfflineSubState OffLineSubState
        {
            get { return _OffLineSubState; }
            set
            {
                if (_OffLineSubState != value)
                {
                    _OffLineSubState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SecsEnum_OnlineSubState _OnLineSubState;
        public SecsEnum_OnlineSubState OnLineSubState
        {
            get { return _OnLineSubState; }
            set
            {
                if (_OnLineSubState != value)
                {
                    _OnLineSubState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SecsEnum_EstablishSource _Equipment_Initiated_Connected;
        public SecsEnum_EstablishSource Equipment_Initiated_Connected
        {
            get { return _Equipment_Initiated_Connected; }
            set
            {
                if(value != SecsEnum_EstablishSource.UNKNOWN)
                {
                    if (_Equipment_Initiated_Connected != value)
                    {
                        _Equipment_Initiated_Connected = value;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        private SecsEnum_Enable _DefaultCommState;
        public SecsEnum_Enable DefaultCommState
        {
            get { return _DefaultCommState; }
            set
            {
                if (_DefaultCommState != value)
                {
                    _DefaultCommState = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// true  = Host    Establish
        /// false = Prober  Establish
        /// </summary>
        private SecsEnum_EstablishSource _EstablishSource;
        public SecsEnum_EstablishSource EstablishSource
        {
            get { return _EstablishSource; }
            set
            {
                if (_EstablishSource != value)
                {
                    _EstablishSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SecsEnum_ControlState _ControlState;
        public SecsEnum_ControlState ControlState
        {
            get { return _ControlState; }
            set
            {
                if (_ControlState != value)
                {
                    _ControlState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SecsEnum_CommunicationState _CommunicationState;
        public SecsEnum_CommunicationState CommunicationState
        {
            get { return _CommunicationState; }
            set
            {
                if (_CommunicationState != value)
                {
                    _CommunicationState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SecsEnum_Enable _Enable;
        public SecsEnum_Enable Enable
        {
            get { return _Enable; }
            set
            {
                if (_Enable != value)
                {
                    _Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SecsGemStateEnum _GemState;
        public SecsGemStateEnum GemState
        {
            get { return _GemState; }
            set
            {
                if (value != _GemState)
                {
                    _GemState = value;
                    RaisePropertyChanged();
                }
            }
        }

    }
    public class CarrierChangeAccessModeResult
    {
        public int FoupNumber { get; set; }
        public int ErrorCode { get; set; }
        public string ErrText { get; set; } = "";

        ///ErrorCode  Example: "U4 2"
        //0 - ok
        //1 - unknown object
        //2 - unknown class
        //3 - unknown object instance
        //4 - unknown attribute type
        //5 - read-only attribute
        //6 - unknown class
        //7 - invalid attribute value
        //8 - syntax error
        //9 - verification error
        //10 - validation error
        //11 - object ID in use
        //12 - improper parameters
        //13 - missing parameters
        //14 - unsupported option requested
        //15 - busy
        //16 - unavailable
        //17 - command not valid in current state
        //18 - no material altered
        //19 - partially processed
        //20 - all material processed
        //21 - recipe specification error
        //22 - failure when processing
        //23 - failure when not processing
        //24 - lack of material
        //25 - job aborted
        //26 - job stopped
        //27 - job cancelled
        //28 - cannot change selected recipe
        //29 - unknown event
        //30 - duplicate report ID
        //31 - unknown data report
        //32 - data report not linked
        //33 - unknown trace report
        //34 - duplicate trace ID
        //35 - too many reports
        //36 - invalid sample period
        //37 - group size too large
        //38 - recovery action invalid
        //39 - busy with previous recovery
        //40 - no active recovery
        //41 - recovery failed
        //42 - recovery aborted
        //43 - invalid table element
        //44 - unknown table element
        //45 - cannot delete predefined
        //46 - invalid token
        //47 - invalid parameter
        //48 - Load port does not exist
        //49 - Load port is busy
        //50 - missing carrier
        //32768 - deferred for later initiation
        //32769 - can not be performed now
        //32770 - failure from errors
        //32771 - invalid command
        //32772 - client alarm
        //32773 - duplicate clientID
        //32774 - invalid client type
        //32776 - unknown clientID
        //32777 - Unsuccessful completion
        //32779 - detected obstacle
        //32780 - material not sent
        //32781 - material not received
        //32782 - material lost
        //32783 - hardware error
        //32784 - transfer cancelled
    }
}
