using Autofac;
using LogModule;
using ProberErrorCode;
using ProberInterfaces.Enum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TwinCatHelper;

namespace GPLoaderRouter
{
    using BCDCLV50x;
    using CardIDManualDialog;
    using CognexOCRManualDialog;
    using LoaderBase;
    using LoaderBase.AttachModules.ModuleInterfaces;
    using MetroDialogInterfaces;
    using ProberInterfaces;
    using ProberInterfaces.Foup;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using SystemExceptions.ProberSystemException;
    using TwinCAT.Ads;
    using NotifyEventModule;
    using ProberInterfaces.Event;
    using ProberInterfaces.RFID;
    using RFID;
    using SequenceRunner;
    using SequenceRunnerBehaviors;
    using ProberInterfaces.SignalTower;
    using SignalTowerModule;
    using System.Reflection;
    using System.Collections;
    using LoaderParameters;
    using ProberInterfaces.CardChange;

    public class GPLoader : IHasSysParameterizable, IFactoryModule, INotifyPropertyChanged, IModule,
        IGPLoader, IGPLoaderCommands, ICSTControlCommands, IGPUtilityBoxCommands
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private object plcLockObject = new object();

        private static int DefaultJobTimeout = 60000;
        private static double CardArmfrontPosition = 150000;
        private bool Loaderdooropenflag = false;
        ManualResetEvent mreUpdateEvent = new ManualResetEvent(false);
        //ManualResetEvent mreSyncEvent = new ManualResetEvent(false);
        private ManualResetEvent _SyncEvent = new ManualResetEvent(false);

        private bool _IsBuzzerOn;
        public bool IsBuzzerOn
        {
            get { return _IsBuzzerOn; }
            set
            {
                if (value != _IsBuzzerOn)
                {
                    _IsBuzzerOn = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsLoaderBusy;
        public bool IsLoaderBusy
        {
            get { return _IsLoaderBusy; }
            set
            {
                if (value != _IsLoaderBusy)
                {
                    _IsLoaderBusy = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _FeedRate = 100;
        public int FeedRate
        {
            get { return _FeedRate; }
            set
            {
                if (value != _FeedRate)
                {
                    _FeedRate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _LoaderRobotLockState = false;
        public bool LoaderRobotLockState
        {
            get { return _LoaderRobotLockState; }
        }

        private bool _CardTrayLockState = false;
        public bool CardTrayLockState
        {
            get { return _CardTrayLockState; }
        }


        public ManualResetEvent SyncEvent
        {
            get { return _SyncEvent; }
            private set { _SyncEvent = value; }
        }


        bool bStopUpdateThread;
        Thread UpdateThread;

        #region Properties
        private ADSRouter _PLCModule;

        public ADSRouter PLCModule
        {
            get { return _PLCModule; }
            private set { _PLCModule = value; }
        }
        private CLVBCDSensor _BCDReader;

        public CLVBCDSensor BCDReader
        {
            get { return _BCDReader; }
            set { _BCDReader = value; }
        }

        private RFIDModule _RFIDReader;
        public RFIDModule RFIDReader
        {
            get { return _RFIDReader; }
            set { _RFIDReader = value; }
        }

        private bool _DevConnected;
        public bool DevConnected
        {
            get { return _DevConnected; }
            private set { _DevConnected = value; }
        }
        private InputSymbols _CDXInSymbol;
        public InputSymbols CDXInSymbol
        {
            get { return _CDXInSymbol; }
            set
            {
                if (value != _CDXInSymbol)
                {
                    _CDXInSymbol = value;
                    RaisePropertyChanged();
                }
            }
        }
        private OutputSymbols _CDXOutSymbol;
        public OutputSymbols CDXOutSymbol
        {
            get { return _CDXOutSymbol; }
            set
            {
                if (value != _CDXOutSymbol)
                {
                    _CDXOutSymbol = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<int> _FOUPDriveErrs = new List<int>();
        public List<int> FOUPDriveErrs
        {
            get { return _FOUPDriveErrs; }
            private set
            {
                if (value != _FOUPDriveErrs)
                {
                    _FOUPDriveErrs = value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<double> _FOUPPoss = new List<double>();
        public List<double> FOUPPoss
        {
            get { return _FOUPPoss; }
            private set
            {
                if (value != _FOUPPoss)
                {
                    _FOUPPoss = value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<double> _FOUPVelos = new List<double>();
        public List<double> FOUPVelos
        {
            get { return _FOUPVelos; }
            private set
            {
                if (value != _FOUPVelos)
                {
                    _FOUPVelos = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<bool> _TesterCoolantValveOpened = new ObservableCollection<bool>();
        public ObservableCollection<bool> TesterCoolantValveOpened
        {
            get { return _TesterCoolantValveOpened; }
            private set
            {
                if (value != _TesterCoolantValveOpened)
                {
                    _TesterCoolantValveOpened = value;
                    RaisePropertyChanged();
                }
            }
        }
        private GPLoaderSysParam _SysParam;

        public GPLoaderSysParam SysParam
        {
            get { return _SysParam; }
            set { _SysParam = value; }
        }

        private stCDXIn _CDXIn;

        public stCDXIn CDXIn
        {
            get { return _CDXIn; }
            set { _CDXIn = value; }
        }
        private stCDXOut _CDXOut;

        public stCDXOut CDXOut
        {
            get { return _CDXOut; }
            set { _CDXOut = value; }
        }
        private stCDXOut _CDXOutState;

        public stCDXOut CDXOutState
        {
            get { return _CDXOutState; }
            set { _CDXOutState = value; }
        }
        private stGPLoaderParam _GPLoaderParam;
        public stGPLoaderParam GPLoaderParam
        {
            get { return _GPLoaderParam; }
            set
            {
                if (value != _GPLoaderParam)
                {
                    _GPLoaderParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<EnumCSTCtrl> _CSTCtrlStatus = new ObservableCollection<EnumCSTCtrl>();

        public ObservableCollection<EnumCSTCtrl> CSTCtrlStatus
        {
            get { return _CSTCtrlStatus; }
            set
            {
                if (value != _CSTCtrlStatus)
                {
                    _CSTCtrlStatus = value;
                    RaisePropertyChanged();
                }

            }
        }
        private ObservableCollection<EnumCSTCtrl> _CSTCtrlCmds = new ObservableCollection<EnumCSTCtrl>();

        public ObservableCollection<EnumCSTCtrl> CSTCtrlCmds
        {
            get { return _CSTCtrlCmds; }
            set
            {
                if (value != _CSTCtrlCmds)
                {
                    _CSTCtrlCmds = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private stGPRobotParam _GPRobotParams;
        //public stGPRobotParam GPRobotParams
        //{
        //    get { return _GPRobotParams; }
        //    set
        //    {
        //        if (value != _GPRobotParams)
        //        {
        //            _GPRobotParams = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private ObservableCollection<stAccessParam> _FixTrayAccParams;
        public ObservableCollection<stAccessParam> FixTrayAccParams
        {
            get { return _FixTrayAccParams; }
            set
            {
                if (value != _FixTrayAccParams)
                {
                    _FixTrayAccParams = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<stAccessParam> _CardBufferAccParams;
        public ObservableCollection<stAccessParam> CardBufferAccParams
        {
            get { return _CardBufferAccParams; }
            set
            {
                if (value != _CardBufferAccParams)
                {
                    _CardBufferAccParams = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<stAccessParam> _CSTAccParams;
        public ObservableCollection<stAccessParam> CSTAccParams
        {
            get { return _CSTAccParams; }
            set
            {
                if (value != _CSTAccParams)
                {
                    _CSTAccParams = value;
                    RaisePropertyChanged();
                }
            }
        }
        //public ObservableCollection<stAccessParam> CSTDRWParams { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        private ObservableCollection<stAccessParam> _CSTDRWParams;
        public ObservableCollection<stAccessParam> CSTDRWParams
        {
            get { return _CSTDRWParams; }
            set
            {
                if (value != _CSTDRWParams)
                {
                    _CSTDRWParams = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<stAccessParam> _PAAccParams;
        public ObservableCollection<stAccessParam> PAAccParams
        {
            get { return _PAAccParams; }
            set
            {
                if (value != _PAAccParams)
                {
                    _PAAccParams = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<stAccessParam> _ChuckAccParams;
        public ObservableCollection<stAccessParam> ChuckAccParams
        {
            get { return _ChuckAccParams; }
            set
            {
                if (value != _ChuckAccParams)
                {
                    _ChuckAccParams = value;
                    RaisePropertyChanged();
                }
            }
        }

        private stAccessParam _Card_ID_AccParam;
        public stAccessParam Card_ID_AccParam
        {
            get { return _Card_ID_AccParam; }
            set
            {
                if (value != _Card_ID_AccParam)
                {
                    _Card_ID_AccParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<stAccessParam> _CardAccParams;
        public ObservableCollection<stAccessParam> CardAccParams
        {
            get { return _CardAccParams; }
            set
            {
                if (value != _CardAccParams)
                {
                    _CardAccParams = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<stGPFoupParam> _FOUPParams = new ObservableCollection<stGPFoupParam>();
        public ObservableCollection<stGPFoupParam> FOUPParams
        {
            get { return _FOUPParams; }
            set
            {
                if (value != _FOUPParams)
                {
                    _FOUPParams = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<bool> _CoolantInletValveStates;
        public ObservableCollection<bool> CoolantInletValveStates
        {
            get { return _CoolantInletValveStates; }
            set
            {
                if (value != _CoolantInletValveStates)
                {
                    _CoolantInletValveStates = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<bool> _CoolantOutletValveStates;
        public ObservableCollection<bool> CoolantOutletValveStates
        {
            get { return _CoolantOutletValveStates; }
            set
            {
                if (value != _CoolantOutletValveStates)
                {
                    _CoolantOutletValveStates = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<bool> _PurgeValveStates;
        public ObservableCollection<bool> PurgeValveStates
        {
            get { return _PurgeValveStates; }
            set
            {
                if (value != _PurgeValveStates)
                {
                    _PurgeValveStates = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<bool> _DrainValveStates;
        public ObservableCollection<bool> DrainValveStates
        {
            get { return _DrainValveStates; }
            set
            {
                if (value != _DrainValveStates)
                {
                    _DrainValveStates = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<bool> _DryAirValveStates;
        public ObservableCollection<bool> DryAirValveStates
        {
            get { return _DryAirValveStates; }
            set
            {
                if (value != _DryAirValveStates)
                {
                    _DryAirValveStates = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<int> _CoolantPressures;
        public ObservableCollection<int> CoolantPressures
        {
            get { return _CoolantPressures; }
            set
            {
                if (value != _CoolantPressures)
                {
                    _CoolantPressures = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<bool> _CoolantLeaks;
        public ObservableCollection<bool> CoolantLeaks
        {
            get { return _CoolantLeaks; }
            set
            {
                if (value != _CoolantLeaks)
                {
                    _CoolantLeaks = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _AxisEnabled;
        public bool AxisEnabled
        {
            get { return _AxisEnabled; }
            set
            {
                if (value != _AxisEnabled)
                {
                    _AxisEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _LeftSaftyDoor_Lock;
        public bool LeftSaftyDoor_Lock
        {
            get { return _LeftSaftyDoor_Lock; }
            set
            {
                if (value != _LeftSaftyDoor_Lock)
                {
                    _LeftSaftyDoor_Lock = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _RightSaftyDoor_Lock;
        public bool RightSaftyDoor_Lock
        {
            get { return _RightSaftyDoor_Lock; }
            set
            {
                if (value != _RightSaftyDoor_Lock)
                {
                    _RightSaftyDoor_Lock = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Front_Signal_Red;
        public bool Front_Signal_Red
        {
            get { return _Front_Signal_Red; }
            set
            {
                if (value != _Front_Signal_Red)
                {
                    _Front_Signal_Red = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Front_Signal_Yellow;
        public bool Front_Signal_Yellow
        {
            get { return _Front_Signal_Yellow; }
            set
            {
                if (value != _Front_Signal_Yellow)
                {
                    _Front_Signal_Yellow = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Front_Signal_Green;
        public bool Front_Signal_Green
        {
            get { return _Front_Signal_Green; }
            set
            {
                if (value != _Front_Signal_Green)
                {
                    _Front_Signal_Green = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LightStateEnum _RedSignalState;

        public LightStateEnum RedSignalState
        {
            get { return _RedSignalState; }
            set { _RedSignalState = value; }
        }

        private LightStateEnum _YellowSignalState;

        public LightStateEnum YellowSignalState
        {
            get { return _YellowSignalState; }
            set { _YellowSignalState = value; }
        }

        private LightStateEnum _GreenSignalState;

        public LightStateEnum GreenSignalState
        {
            get { return _GreenSignalState; }
            set { _GreenSignalState = value; }
        }

        private bool _Front_Signal_Buzzer;
        public bool Front_Signal_Buzzer
        {
            get { return _Front_Signal_Buzzer; }
            set
            {
                if (value != _Front_Signal_Buzzer)
                {
                    _Front_Signal_Buzzer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Rear_Signal_Red;
        public bool Rear_Signal_Red
        {
            get { return _Rear_Signal_Red; }
            set
            {
                if (value != _Rear_Signal_Red)
                {
                    _Rear_Signal_Red = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Rear_Signal_Yellow;
        public bool Rear_Signal_Yellow
        {
            get { return _Rear_Signal_Yellow; }
            set
            {
                if (value != _Rear_Signal_Yellow)
                {
                    _Rear_Signal_Yellow = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Rear_Signal_Green;
        public bool Rear_Signal_Green
        {
            get { return _Rear_Signal_Green; }
            set
            {
                if (value != _Rear_Signal_Green)
                {
                    _Rear_Signal_Green = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Rear_Signal_Buzzer;
        public bool Rear_Signal_Buzzer
        {
            get { return _Rear_Signal_Buzzer; }
            set
            {
                if (value != _Rear_Signal_Buzzer)
                {
                    _Rear_Signal_Buzzer = value;
                    RaisePropertyChanged();
                }
            }
        }



        private bool _Visu_Active;
        public bool Visu_Active
        {
            get { return _Visu_Active; }
            set
            {
                if (value != _Visu_Active)
                {
                    _Visu_Active = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<UInt32> _InputStates;

        public List<UInt32> InputStates
        {
            get { return _InputStates; }
            set { _InputStates = value; }
        }


        private List<UInt32> _OutputStates;

        public List<UInt32> OutputStates
        {
            get { return _OutputStates; }
            set { _OutputStates = value; }
        }


        ILoaderModule loaderModule
        {
            get
            {
                if (this.GetLoaderContainer() != null)
                {
                    return this.GetLoaderContainer().Resolve<ILoaderModule>();
                }
                else
                {
                    return null;
                }
            }
        }

        private ProbeAxisObject _LUD;
        public ProbeAxisObject LUD
        {
            get { return _LUD; }
            set
            {
                if (value != _LUD)
                {
                    _LUD = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbeAxisObject _LUU;
        public ProbeAxisObject LUU
        {
            get { return _LUU; }
            set
            {
                if (value != _LUU)
                {
                    _LUU = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbeAxisObject _LCC;
        public ProbeAxisObject LCC
        {
            get { return _LCC; }
            set
            {
                if (value != _LCC)
                {
                    _LCC = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _LX;
        public ProbeAxisObject LX
        {
            get { return _LX; }
            set
            {
                if (value != _LX)
                {
                    _LX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _LZ;
        public ProbeAxisObject LZ
        {
            get { return _LZ; }
            set
            {
                if (value != _LZ)
                {
                    _LZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _LW;
        public ProbeAxisObject LW
        {
            get { return _LW; }
            set
            {
                if (value != _LW)
                {
                    _LW = value;
                    RaisePropertyChanged();
                }
            }
        }

        private GPLoaderSymbolMap _SymbolMap;

        public GPLoaderSymbolMap SymbolMap
        {
            get { return _SymbolMap; }
            set { _SymbolMap = value; }
        }
        private IOSymbolMappings _IOSymbolMap;

        private GPLoaderRobotCommandParam _RobotCommandParam;

        public GPLoaderRobotCommandParam RobotCommandParam
        {
            get { return _RobotCommandParam; }
            set { _RobotCommandParam = value; }
        }

        private GPLoaderFoupCommandParam _FoupCommandParam;

        public GPLoaderFoupCommandParam FoupCommandParam
        {
            get { return _FoupCommandParam; }
            set { _FoupCommandParam = value; }
        }

        private List<IGPLoaderRobotCommand> _RobotCommandList;

        public List<IGPLoaderRobotCommand> RobotCommandList
        {
            get { return _RobotCommandList; }
            set { _RobotCommandList = value; }
        }

        private List<IGPLoaderCSTCtrlCommand> _FoupCommandList;

        public List<IGPLoaderCSTCtrlCommand> FoupCommandList
        {
            get { return _FoupCommandList; }
            set { _FoupCommandList = value; }
        }


        public IOSymbolMappings IOSymbolMap
        {
            get { return _IOSymbolMap; }
            set { _IOSymbolMap = value; }
        }
     

        public bool Initialized { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public object RemoteModule => (object)PLCModule;


        private IPreAlignerControlItems _preAlignerControlItems;
        public IPreAlignerControlItems preAlignerControlItems
        {
            get { return _preAlignerControlItems; }
            set { _preAlignerControlItems = value; }
        }

        #endregion

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new GPLoaderSysParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(GPLoaderSysParam));
                tmpParam.Owner = this;
                if (RetVal == EventCodeEnum.NONE)
                {
                    SysParam = tmpParam as GPLoaderSysParam;
                }

                tmpParam = new GPLoaderSymbolMap();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(GPLoaderSymbolMap));
                tmpParam.Owner = this;
                if (RetVal == EventCodeEnum.NONE)
                {
                    SymbolMap = tmpParam as GPLoaderSymbolMap;
                }

                tmpParam = new GPLoaderRobotCommandParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(GPLoaderRobotCommandParam));
                tmpParam.Owner = this;
                if (RetVal == EventCodeEnum.NONE)
                {
                    RobotCommandParam = tmpParam as GPLoaderRobotCommandParam;
                    RobotCommandList = RobotCommandParam.GPLoaderRobotCommandList;
                }

                tmpParam = new GPLoaderFoupCommandParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(GPLoaderFoupCommandParam));
                tmpParam.Owner = this;
                if (RetVal == EventCodeEnum.NONE)
                {
                    FoupCommandParam = tmpParam as GPLoaderFoupCommandParam;
                    FoupCommandList = FoupCommandParam.GPLoaderFoupCommandList;
                }

                tmpParam = null;
                tmpParam = new IOSymbolMappings();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(IOSymbolMappings), null, null, Extensions_IParam.FileType.XML);

                if (RetVal == EventCodeEnum.NONE)
                {
                    IOSymbolMap = tmpParam as IOSymbolMappings;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = this.SaveParameter(SysParam);
                RetVal = this.SaveParameter(SymbolMap);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public GPLoader()
        {

            CDXIn = new stCDXIn();
            CDXOut = new stCDXOut();
            GPLoaderParam = new stGPLoaderParam();


            FixTrayAccParams = new ObservableCollection<stAccessParam>();
            for (int i = 0; i < SystemModuleCount.ModuleCnt.FixedTrayCount; i++)
            {
                FixTrayAccParams.Add(new stAccessParam());
            }
            CardBufferAccParams = new ObservableCollection<stAccessParam>();
            for (int i = 0; i < (SystemModuleCount.ModuleCnt.CardBufferCount + SystemModuleCount.ModuleCnt.CardTrayCount); i++)
            {
                CardBufferAccParams.Add(new stAccessParam());
            }
            //for (int i = 0; i < (loaderModule.SystemParameter.CardTrayIndexOffset.Value + SystemModuleCount.ModuleCnt.CardTrayCount); i++)
            //{
            //    CardBufferAccParams.Add(new stAccessParam());
            //}

            CardAccParams = new ObservableCollection<stAccessParam>();
            for (int i = 0; i < SystemModuleCount.ModuleCnt.StageCount; i++)
            {
                CardAccParams.Add(new stAccessParam());
            }


            CSTAccParams = new ObservableCollection<stAccessParam>();
            for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
            {
                CSTAccParams.Add(new stAccessParam());
            }


            FOUPParams = new ObservableCollection<stGPFoupParam>();
            for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
            {
                FOUPParams.Add(new stGPFoupParam());
            }

            CSTDRWParams = new ObservableCollection<stAccessParam>();
            for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
            {
                CSTDRWParams.Add(new stAccessParam());
            }

            ChuckAccParams = new ObservableCollection<stAccessParam>();
            for (int i = 0; i < SystemModuleCount.ModuleCnt.StageCount; i++)
            {
                ChuckAccParams.Add(new stAccessParam());
            }
            //for (int i = 0; i < GPLoaderDef.CardBufferCount; i++)
            //{
            //    FixTrayAccParams.Add(new stAccessParam());
            //}
            PAAccParams = new ObservableCollection<stAccessParam>();
            for (int i = 0; i < SystemModuleCount.ModuleCnt.PACount; i++)
            {
                PAAccParams.Add(new stAccessParam());
            }
            

            bStopUpdateThread = false;
            UpdateThread = new Thread(new ThreadStart(UpdateProc));
            UpdateThread.Name = this.GetType().Name;
            Card_ID_AccParam = new stAccessParam();
        }



        private void _monitoringTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            mreUpdateEvent.Set();
        }

        ~GPLoader()
        {
            DeInit();
        }
        public EventCodeEnum InitGPLoaderRouter(string connid)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                BCDReader = new CLVBCDSensor();
                PLCModule = new ADSRouter(connid);

                //#Hynix_Merge: 검토 필요, Hynix 코드 에만 있었음. PGV-RFID 추가되면서 추가된 코드 같음. 일단 잘못된 위치에 있는 코드 같아 주석.
                // System Parameter 읽기
                //EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
                //IParam tmpParam = null;
                //tmpParam = new GPLoaderSysParam();
                //tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                //RetVal = this.LoadParameter(ref tmpParam, typeof(GPLoaderSysParam));
                //tmpParam.Owner = this;
                //if (RetVal == EventCodeEnum.NONE)
                //{
                //    SysParam = tmpParam as GPLoaderSysParam;
                //}
                //#========================================================
                
                ret = BCDReader.InitComm(4);
                if (ret == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"InitGPLoaderRouter(): Barcode Reader connection fail.");
                }
                var result = PLCModule.InitComm();
                if (result == 0)
                {
                    RegistSymbols(PLCModule, SymbolMap.GetSymbols());

                    UpdateThread.Start();
                    DevConnected = true;
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    throw new Exception($"ADS Router connection failed. ADS ID.: {connid}");
                }
            }
            catch (Exception err)
            {
                DevConnected = false;
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return ret;
        }

        public void DeInit()
        {
            try
            {
                bStopUpdateThread = true;
                if (UpdateThread != null)
                {
                    if (UpdateThread.IsAlive == true)
                    {
                        UpdateThread.Join();
                    }
                }
                UnregistSymbols(PLCModule, SymbolMap.GetSymbols());
                DevConnected = false;
            }
            catch (Exception err)
            {
                DevConnected = false;
                LoggerManager.Exception(err, "InitGPLoaderRouter(): Exception has occurred.");
            }

        }

        private void RegistSymbols(ADSRouter router, List<ADSSymbolBase> symbols)
        {
            try
            {

                foreach (var symbol in symbols)
                {
                    try
                    {
                        
                        var handle = router.tcClient.CreateVariableHandle(symbol.SymbolName);
                        symbol.Handle = handle;
                        if (symbol.Handle == 0)
                        {
                            LoggerManager.Debug($"RegistSymbols(): [{symbol.SymbolName}], Invalid handle. ");
                        }
                        LoggerManager.Debug($"Symbol [{symbol.SymbolName}] registered. Handle = {symbol.Handle}");
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                        symbol.Handle = -1;
                        LoggerManager.Debug($"Symbol [{symbol.SymbolName}]: Registration failed.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"RegistSymbols(): Error occurred. Err = {err.Message}");
            }

        }
        private void UnregistSymbols(ADSRouter router, List<ADSSymbolBase> symbols)
        {
            if (DevConnected == true)
            {
                foreach (var symbol in symbols)
                {
                    router.tcClient.DeleteVariableHandle(symbol.Handle);
                }
            }
        }
        public EventCodeEnum RenewData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                lock (plcLockObject)
                {

                    if (DevConnected == true)
                    {
                        //  var CDXOutInitState = PLCModule.tcClient.ReadAny(SymbolMap.OutputSymbol.GPL_RobotParam.Handle, typeof(stGPRobotParam));
                        // PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Visu_Active.Handle, Visu_Active);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.GPLParam.Handle, GPLoaderParam);
                        //  PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol..Handle, CDXOut);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.FixTrayParams.Handle, FixTrayAccParams.ToArray());
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.PAAccParams.Handle, PAAccParams.ToArray());
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.ChuckAccParams.Handle, ChuckAccParams.ToArray());
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CardBufferParams.Handle, CardBufferAccParams.ToArray());
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CCAccParams.Handle, CardAccParams.ToArray());
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CSTAccParams.Handle, CSTAccParams.ToArray());
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.DRWAccParams.Handle, CSTDRWParams.ToArray());
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CardIDAccParam.Handle, Card_ID_AccParam);

                        //minskim// drax 버전에 추가된 FOUPParamExx Symbol Handle check 추가
                        if (SymbolMap.OutputSymbol.FOUPParamEx != null && SymbolMap.OutputSymbol.FOUPParamEx.Handle > 0)
                        {
                            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.FOUPParamEx.Handle, FOUPParams.ToArray());
                        }

                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.LeftSaftyDoor_Lock.Handle, LeftSaftyDoor_Lock);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.RightSaftyDoor_Lock.Handle, RightSaftyDoor_Lock);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Red.Handle, Front_Signal_Red);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Yellow.Handle, Front_Signal_Yellow);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Green.Handle, Front_Signal_Green);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Buzzer.Handle, Front_Signal_Buzzer);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Red.Handle, Rear_Signal_Red);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Yellow.Handle, Rear_Signal_Yellow);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Green.Handle, Rear_Signal_Green);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Buzzer.Handle, Rear_Signal_Buzzer);
                        retVal = EventCodeEnum.NONE;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public void LoaderBuzzer(bool isBuzzerOn)
        {
            try
            {
                lock (plcLockObject)
                {
                    IsBuzzerOn = isBuzzerOn;
                    Front_Signal_Buzzer = isBuzzerOn;

                    if (PLCModule != null)
                    {
                        if (PLCModule.tcClient.IsConnected)
                        {
                            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Buzzer.Handle, Front_Signal_Buzzer);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void LoaderLampSetState(ModuleStateEnum state)
        {
            try
            {
                //lock (plcLockObject)
                //{
                //    StopGreenBlink();
                //    StopRedBlink();
                //    bool isExecute = false;
                //    switch (state)
                //    {
                //        case ModuleStateEnum.IDLE:
                //            Front_Signal_Green = false;
                //            Front_Signal_Red = false;
                //            isExecute = true;

                //            GreenSignalState = LightStateEnum.OFF;
                //            RedSignalState = LightStateEnum.OFF;
                //            break;
                //        case ModuleStateEnum.RUNNING:
                //            Front_Signal_Green = true;
                //            Front_Signal_Red = false;
                //            isExecute = true;

                //            GreenSignalState = LightStateEnum.ON;
                //            RedSignalState = LightStateEnum.OFF;
                //            break;
                //        case ModuleStateEnum.PAUSED:
                //            RunGreenBlink();
                //            GreenSignalState = LightStateEnum.BLIKING;
                //            break;
                //        case ModuleStateEnum.ABORT:
                //            Front_Signal_Green = true;
                //            Front_Signal_Red = false;
                //            isExecute = true;

                //            GreenSignalState = LightStateEnum.ON;
                //            RedSignalState = LightStateEnum.OFF;
                //            break;
                //        case ModuleStateEnum.ERROR:
                //            LoaderBuzzer(true);
                //            RunRedBlink();

                //            RedSignalState = LightStateEnum.BLIKING;
                //            break;
                //        default:
                //            Front_Signal_Red = false;
                //            Front_Signal_Green = false;
                //            isExecute = true;

                //            GreenSignalState = LightStateEnum.OFF;
                //            RedSignalState = LightStateEnum.OFF;
                //            break;
                //    }

                //    if (PLCModule != null)
                //    {
                //        if (PLCModule.tcClient.IsConnected && isExecute)
                //        {
                //            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Red.Handle, Front_Signal_Red);
                //            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Red.Handle, Front_Signal_Red);
                //            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Green.Handle, Front_Signal_Green);
                //            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Green.Handle, Front_Signal_Green);
                //        }

                //        UpdataGemLampState();
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void StageLampSetState(ModuleStateEnum state)
        {
            try
            {
                /*
                lock (plcLockObject)
                {
                    bool isExecute = false;
                    StopYellowBlink();
                    StopRedBlink();
                    switch (state)
                    {
                        case ModuleStateEnum.IDLE:
                            isExecute = true;
                            Front_Signal_Yellow = false;

                            YellowSignalState = LightStateEnum.OFF;
                            break;
                        case ModuleStateEnum.PAUSED:
                            RunYellowBlink();

                            YellowSignalState = LightStateEnum.BLIKING;
                            break;
                        case ModuleStateEnum.ERROR:
                            LoaderBuzzer(true);
                            RunRedBlink();

                            RedSignalState = LightStateEnum.BLIKING;
                            break;
                        case ModuleStateEnum.RUNNING:
                            isExecute = true;
                            Front_Signal_Yellow = true;

                            YellowSignalState = LightStateEnum.ON;
                            break;
                    }

                    if (PLCModule != null)
                    {
                        if (PLCModule.tcClient.IsConnected && isExecute)
                        {
                            //PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Red.Handle, Front_Signal_Red);
                            // PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Red.Handle, Front_Signal_Red);
                            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Yellow.Handle, Front_Signal_Yellow);
                            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Yellow.Handle, Front_Signal_Yellow);
                        }

                        UpdataGemLampState();
                    }
                }
                */
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void UpdataGemLampState()
        {
            try
            {
                //this.loaderModule.LoaderMaster.GetLoaderPIV().SetLightState(RedSignalState, YellowSignalState, GreenSignalState);
                this.GEMModule().GetPIVContainer().SetLightState(RedSignalState, YellowSignalState, GreenSignalState);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetOnSignalTowerState(string signalTowerType, bool onoff)
        {
            try
            {
                lock (plcLockObject)
                {                                            
                    if (PLCModule != null)
                    {
                        if (PLCModule.tcClient.IsConnected)
                        {

                            switch (signalTowerType)
                            {
                                case "RED":
                                    StopRedBlink();
                                    //Front_Signal_Red = onoff;
                                    PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Red.Handle, onoff);
                                    PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Red.Handle, onoff);
                                    break;
                                case "GREEN":
                                    StopGreenBlink();
                                    //Front_Signal_Green = onoff;
                                    PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Green.Handle, onoff);
                                    PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Green.Handle, onoff);
                                    break;
                                case "YELLOW":
                                    StopYellowBlink();
                                    //Front_Signal_Yellow = onoff;
                                    PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Yellow.Handle, onoff);
                                    PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Yellow.Handle, onoff);
                                    break;
                                default:
                                    break;
                            }

                        }
                        UpdataGemLampState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);                
            }
        }
                            
        private bool _BlinkRedRun;
        private Task _BlinkRedRunTask;
        public void RunRedBlink()
        {
            try
            {
                if (_BlinkRedRun)
                    return;//==> 이미 실행중이어서 더 실행 시킬 필요 없음.

                StopRedBlink();//==> 안전 장치

                _BlinkRedRun = true;

                _BlinkRedRunTask = Task.Run(() =>
                {
                    const int blinkInterval = 500;
                    bool value = false;

                    do
                    {
                        Front_Signal_Red = value;
                        Thread.Sleep(blinkInterval);
                        value = !value;

                        if (PLCModule != null)
                        {
                            if (PLCModule.tcClient.IsConnected)
                            {
                                PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Red.Handle, Front_Signal_Red);
                                PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Red.Handle, Front_Signal_Red);                                                               
                            }
                        }

                    } while (_BlinkRedRun);
                    if (PLCModule.tcClient.IsConnected)
                    {
                        Front_Signal_Red = false;
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Red.Handle, Front_Signal_Red);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Red.Handle, Front_Signal_Red);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void StopRedBlink()
        {
            try
            {
                if(_BlinkRedRun)
                {
                    _BlinkRedRun = false;
                    if (_BlinkRedRunTask != null)
                    {
                        _BlinkRedRunTask.Wait();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private bool _BlinkGreenRun;
        private Task _BlinkGreenRunTask;

        public void RunGreenBlink()
        {
            try
            {
                if (_BlinkGreenRun)
                    return;//==> 이미 실행중이어서 더 실행 시킬 필요 없음.

                StopGreenBlink();//==> 안전 장치

                _BlinkGreenRun = true;

                _BlinkGreenRunTask = Task.Run(() =>
                {
                    const int blinkInterval = 500;
                    bool value = false;

                    do
                    {
                        Front_Signal_Green = value;
                        Thread.Sleep(blinkInterval);
                        value = !value;

                        if (PLCModule != null)
                        {
                            if (PLCModule.tcClient.IsConnected)
                            {
                                PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Green.Handle, Front_Signal_Green);
                                PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Green.Handle, Front_Signal_Green);
                            }
                        }

                    } while (_BlinkGreenRun);

                    if (PLCModule.tcClient.IsConnected)
                    {
                        Front_Signal_Green = false;
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Green.Handle, Front_Signal_Green);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Green.Handle, Front_Signal_Green);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StopGreenBlink()
        {
            try
            {
                if (_BlinkGreenRun == true)
                {
                    _BlinkGreenRun = false;
                    if (_BlinkGreenRunTask != null)
                    {
                        _BlinkGreenRunTask.Wait();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool _BlinkYellowRun;
        private Task _BlinkYellowRunTask;

        public void RunYellowBlink()
        {
            try
            {
                if (_BlinkYellowRun)
                    return;//==> 이미 실행중이어서 더 실행 시킬 필요 없음.

                StopYellowBlink();//==> 안전 장치

                _BlinkYellowRun = true;

                _BlinkYellowRunTask = Task.Run(() =>
                {
                    const int blinkInterval = 500;
                    bool value = false;

                    do
                    {
                        Front_Signal_Yellow = value;
                        Thread.Sleep(blinkInterval);
                        value = !value;

                        if (PLCModule != null)
                        {
                            if (PLCModule.tcClient.IsConnected)
                            {
                                PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Yellow.Handle, Front_Signal_Yellow);
                                PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Yellow.Handle, Front_Signal_Yellow);                                
                            }
                        }

                    } while (_BlinkYellowRun);
                    if (PLCModule.tcClient.IsConnected)
                    {
                        Front_Signal_Yellow = false;
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Front_Signal_Yellow.Handle, Front_Signal_Yellow);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Rear_Signal_Yellow.Handle, Front_Signal_Yellow);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StopYellowBlink()
        {
            try
            {
                if (_BlinkYellowRun)
                {
                    _BlinkYellowRun = false;
                    if (_BlinkYellowRunTask != null)
                    {
                        _BlinkYellowRunTask.Wait();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void UpdateProc()
        {
            long elapsedTime;
            AdsStream cdxInDataStream = new AdsStream(SymbolMap.InputSymbol.CDXIn.StreamLength);
            BinaryReader binRead = new BinaryReader(cdxInDataStream);
            AdsStream cdxOutDataStream = new AdsStream(SymbolMap.OutputSymbol.CDXOut.StreamLength);
            BinaryReader boutRead = new BinaryReader(cdxOutDataStream);
            
            //bool extendedIO = SymbolMap.InputSymbol.InputStates.Handle != -1 ? true : false;
            bool extendedIO = false;

            if (SymbolMap.InputSymbol.InputStates.Handle > 0)
            {
                extendedIO = true;
            }
            else
            {
                extendedIO = false;
            }

            Stopwatch stw = new Stopwatch();
            stw.Start();
            try
            {
                var CDXOutInitState = PLCModule.tcClient.ReadAny(SymbolMap.OutputSymbol.CDXOut.Handle, typeof(stCDXOut));
                CDXOut = (stCDXOut)CDXOutInitState;

                stw.Start();
                elapsedTime = stw.ElapsedMilliseconds;
                bool errHandled = false;
                while (bStopUpdateThread == false)
                {
                    try
                    {
                        if (errHandled == true)  // For error state. Lower update rate.
                        {
                            Thread.Sleep(1000);
                        }
                        SyncEvent.Reset();
                        if (DevConnected == true)
                        {

                            #region // Discrete update
                            //byte[] vs;
                            //int bufferIndex = 0;
                            //PLCModule.tcClient.Read(SymbolMap.InputSymbol.CDXIn.Handle, cdxInDataStream);
                            //cdxInDataStream.Position = 0;
                            //vs = new byte[cdxInDataStream.Length];
                            //cdxInDataStream.Read(vs, 0, (int)cdxInDataStream.Length);
                            //bufferIndex = 0;
                            #region // Descrete write
                            //CDXIn.nState = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 2;
                            //bufferIndex += 2;   // UINT -> UDINT

                            //CDXIn.nCtrlErrID = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;
                            //CDXIn.nCtrlState = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;
                            //CDXIn.nCSTIO = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;

                            //CDXIn.nRobotState = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 2;
                            //CDXIn.nRobotCSTPos = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 2;
                            //CDXIn.nRobotPreAPos = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 2;
                            //CDXIn.nRobotStagePos = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 2;
                            //CDXIn.nRobotWaferSlotPos = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 2;
                            //bufferIndex += 2;       // UINT -> UDINT

                            //CDXIn.nCSTWaferCnt = new UInt32[3];
                            //CDXIn.nCSTWaferCnt[0] = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 4;
                            //CDXIn.nCSTWaferCnt[1] = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 4;
                            //CDXIn.nCSTWaferCnt[2] = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 4;

                            //CDXIn.nCSTWaferState = new UInt32[9];
                            //CDXIn.nCSTWaferState[0] = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;
                            //CDXIn.nCSTWaferState[1] = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;
                            //CDXIn.nCSTWaferState[2] = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;
                            //CDXIn.nCSTWaferState[3] = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;
                            //CDXIn.nCSTWaferState[4] = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;
                            //CDXIn.nCSTWaferState[5] = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;
                            //CDXIn.nCSTWaferState[6] = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;
                            //CDXIn.nCSTWaferState[7] = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;
                            //CDXIn.nCSTWaferState[8] = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;

                            //CDXIn.nPreAState = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;

                            //CDXIn.nStageState_1 = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;

                            //CDXIn.nStageState_2 = BitConverter.ToUInt32(vs, bufferIndex);
                            //bufferIndex += 4;

                            //CDXIn.nLX_State = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 2;

                            //CDXIn.nLZM_State = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 2;

                            //CDXIn.nLZS_State = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 2; CDXIn.nLW_State = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 2; CDXIn.nLT_State = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 2; CDXIn.nLUD_State = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 2; CDXIn.nLUU_State = BitConverter.ToUInt16(vs, bufferIndex);
                            //bufferIndex += 2; CDXIn.nLCC_State = BitConverter.ToUInt16(vs, bufferIndex);

                            //bufferIndex += 2; CDXIn.nLX_Pos = BitConverter.ToInt32(vs, bufferIndex);
                            //bufferIndex += 4; CDXIn.nLZM_Pos = BitConverter.ToInt32(vs, bufferIndex);
                            //bufferIndex += 4; CDXIn.nLZS_Pos = BitConverter.ToInt32(vs, bufferIndex);
                            //bufferIndex += 4; CDXIn.nLW_Pos = BitConverter.ToInt32(vs, bufferIndex);
                            //bufferIndex += 4; CDXIn.nLT_Pos = BitConverter.ToInt32(vs, bufferIndex);
                            //bufferIndex += 4; CDXIn.nLUD_Pos = BitConverter.ToInt32(vs, bufferIndex);
                            //bufferIndex += 4; CDXIn.nLUU_Pos = BitConverter.ToInt32(vs, bufferIndex);
                            //bufferIndex += 4; CDXIn.nLCC_Pos = BitConverter.ToInt32(vs, bufferIndex);

                            //bufferIndex += 4; CDXIn.nLX_Velo = BitConverter.ToInt32(vs, bufferIndex);
                            //bufferIndex += 4; CDXIn.nLZM_Velo = BitConverter.ToInt32(vs, bufferIndex);
                            //bufferIndex += 4; CDXIn.nLZS_Velo = BitConverter.ToInt32(vs, bufferIndex);
                            //bufferIndex += 4; CDXIn.nLW_Velo = BitConverter.ToInt32(vs, bufferIndex);
                            //bufferIndex += 4; CDXIn.nLT_Velo = BitConverter.ToInt32(vs, bufferIndex);
                            //bufferIndex += 4; CDXIn.nLUD_Velo = BitConverter.ToInt32(vs, bufferIndex);
                            //bufferIndex += 4; CDXIn.nLUU_Velo = BitConverter.ToInt32(vs, bufferIndex);
                            //bufferIndex += 4; CDXIn.nLCC_Velo = BitConverter.ToInt32(vs, bufferIndex);
                            #endregion

                            #endregion
                            var inData = PLCModule.tcClient.ReadAny(SymbolMap.InputSymbol.CDXIn.Handle, typeof(stCDXIn));
                            CDXIn = (stCDXIn)inData;
                            #region // Position update
                            //this.MotionManager().GetAxis(EnumAxisConstants.LX).Status.RawPosition.Actual = CDXIn.nLX_Pos;
                            //this.MotionManager().GetAxis(EnumAxisConstants.LZM).Status.RawPosition.Actual = CDXIn.nLZM_Pos;
                            //this.MotionManager().GetAxis(EnumAxisConstants.LZS).Status.RawPosition.Actual = CDXIn.nLZS_Pos;
                            //this.MotionManager().GetAxis(EnumAxisConstants.LW).Status.RawPosition.Actual = CDXIn.nLW_Pos;
                            //this.MotionManager().GetAxis(EnumAxisConstants.LB).Status.RawPosition.Actual = CDXIn.nLT_Pos;
                            //this.MotionManager().GetAxis(EnumAxisConstants.LUD).Status.RawPosition.Actual = CDXIn.nLUD_Pos;
                            //this.MotionManager().GetAxis(EnumAxisConstants.LUU).Status.RawPosition.Actual = CDXIn.nLUU_Pos;
                            //this.MotionManager().GetAxis(EnumAxisConstants.LCC).Status.RawPosition.Actual = CDXIn.nLCC_Pos;

                            //this.MotionManager().GetAxis(EnumAxisConstants.FC1).Status.RawPosition.Actual = CDXIn.nFC_Pos[0];
                            //this.MotionManager().GetAxis(EnumAxisConstants.FC2).Status.RawPosition.Actual = CDXIn.nFC_Pos[1];
                            //this.MotionManager().GetAxis(EnumAxisConstants.FC3).Status.RawPosition.Actual = CDXIn.nFC_Pos[2];
                            #endregion

                            var outData = PLCModule.tcClient.ReadAny(SymbolMap.OutputSymbol.CDXOut.Handle, typeof(stCDXOut));
                            CDXOutState = (stCDXOut)outData;
                            short[] fsts = new short[SystemModuleCount.ModuleCnt.FoupCount];
                            short[] fcmd = new short[SystemModuleCount.ModuleCnt.FoupCount];

                            UInt16[] foupDriveErrs = null;
                            Int32[] foupPoss = null;
                            Int32[] foupVelos = null;

                            if (SymbolMap.InputSymbol.FOUPDriveErrs != null && SymbolMap.InputSymbol.FOUPDriveErrs.Handle > 0)
                            {
                                foupDriveErrs = (UInt16[])PLCModule.tcClient.ReadAny(SymbolMap.InputSymbol.FOUPDriveErrs.Handle, typeof(UInt16[]), new int[] { SystemModuleCount.ModuleCnt.FoupCount });
                            }
                            if (SymbolMap.InputSymbol.Foup_Poss != null && SymbolMap.InputSymbol.Foup_Poss.Handle > 0)
                            {
                                foupPoss = (Int32[])PLCModule.tcClient.ReadAny(SymbolMap.InputSymbol.Foup_Poss.Handle, typeof(Int32[]), new int[] { SystemModuleCount.ModuleCnt.FoupCount });
                            }
                            if (SymbolMap.InputSymbol.Foup_Velos != null && SymbolMap.InputSymbol.Foup_Velos.Handle > 0)
                            {
                                foupVelos = (Int32[])PLCModule.tcClient.ReadAny(SymbolMap.InputSymbol.Foup_Velos.Handle, typeof(Int32[]), new int[] { SystemModuleCount.ModuleCnt.FoupCount });
                            }

                            if (foupDriveErrs != null && foupPoss != null && foupVelos != null)
                            {
                                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                                {
                                    FOUPDriveErrs[i] = (int)foupDriveErrs[i];
                                    FOUPPoss[i] = (double)foupPoss[i];
                                    FOUPVelos[i] = (double)foupVelos[i];
                                }
                            }
                            if (extendedIO == true)
                            {
                                try
                                {
                                    var inStates = (UInt32[])PLCModule.tcClient.ReadAny(
                                        SymbolMap.InputSymbol.InputStates.Handle, typeof(UInt32[]), new int[] { GPLoaderDef.MaxInputModuleCount });
                                    var outStates = (UInt32[])PLCModule.tcClient.ReadAny(
                                        SymbolMap.OutputSymbol.OutputStates.Handle, typeof(UInt32[]), new int[] { GPLoaderDef.MaxOutputModuleCount });

                                    //Parallel.For(0, GPLoaderDef.MaxInputModuleCount, new ParallelOptions { MaxDegreeOfParallelism = 2 }, i =>
                                    //{
                                    //    InputStates[i] = inStates[i];
                                    //});
                                    //Parallel.For(0, GPLoaderDef.MaxOutputModuleCount, new ParallelOptions { MaxDegreeOfParallelism = 2 }, i =>
                                    //{
                                    //    OutputStates[i] = outStates[i];
                                    //});
                                    for (int i = 0; i < GPLoaderDef.MaxInputModuleCount; i++)
                                    {
                                        InputStates[i] = inStates[i];
                                    }
                                    for (int i = 0; i < GPLoaderDef.MaxOutputModuleCount; i++)
                                    {
                                        OutputStates[i] = outStates[i];
                                    }
                                }
                                catch (Exception err)
                                {
                                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                                }

                            }
                            else
                            {
                                InputStates[0] = CDXIn.Input1_State;
                                InputStates[1] = CDXIn.Input2_State;
                                InputStates[2] = CDXIn.Input3_State;
                                InputStates[3] = CDXIn.Input4_State;
                                OutputStates[0] = CDXIn.Output1_State;
                                OutputStates[1] = CDXIn.Output2_State;
                                OutputStates[2] = CDXIn.Output3_State;
                            }

                            fsts = (short[])PLCModule.tcClient.ReadAny(SymbolMap.InputSymbol.CSTCtrlStatus.Handle,
                                typeof(short[]), new int[] { SystemModuleCount.ModuleCnt.FoupCount });
                            fcmd = (short[])PLCModule.tcClient.ReadAny(SymbolMap.OutputSymbol.CSTCtrlCommand.Handle,
                                typeof(short[]), new int[] { SystemModuleCount.ModuleCnt.FoupCount });

                            for (int i = 0; i < fsts.Length; i++)
                            {
                                if (i <= CSTCtrlStatus.Count)
                                {
                                    CSTCtrlStatus[i] = (EnumCSTCtrl)fsts[i];
                                    CSTCtrlCmds[i] = (EnumCSTCtrl)fcmd[i];

                                }
                            }


                            lock (utilBoxLockObject)
                            {
                                bool[] civs = (bool[])PLCModule.tcClient.ReadAny(
                                SymbolMap.OutputSymbol.CoolantInletCommand.Handle, typeof(bool[]), new int[] { GPLoaderDef.StageCount });
                                for (int i = 0; i < civs.Count(); i++)
                                {
                                    CoolantInletValveStates[i] = civs[i];
                                }
                                bool[] covs = (bool[])PLCModule.tcClient.ReadAny(
                                    SymbolMap.OutputSymbol.CoolantOutletCommand.Handle, typeof(bool[]), new int[] { GPLoaderDef.StageCount });
                                for (int i = 0; i < covs.Count(); i++)
                                {
                                    CoolantOutletValveStates[i] = covs[i];
                                }
                                bool[] pvs = (bool[])PLCModule.tcClient.ReadAny(
                                    SymbolMap.OutputSymbol.CoolantPurgeCommand.Handle, typeof(bool[]), new int[] { GPLoaderDef.StageCount });
                                for (int i = 0; i < pvs.Count(); i++)
                                {
                                    PurgeValveStates[i] = pvs[i];
                                }
                                bool[] dvs = (bool[])PLCModule.tcClient.ReadAny(
                                    SymbolMap.OutputSymbol.CoolantDrainCommand.Handle, typeof(bool[]), new int[] { GPLoaderDef.StageCount });
                                for (int i = 0; i < dvs.Count(); i++)
                                {
                                    DrainValveStates[i] = dvs[i];
                                }
                                int[] cop = (int[])PLCModule.tcClient.ReadAny(
                                    SymbolMap.InputSymbol.CoolantPressStatus.Handle, typeof(int[]), new int[] { GPLoaderDef.StageCount });
                                for (int i = 0; i < cop.Count(); i++)
                                {
                                    CoolantPressures[i] = cop[i];
                                }
                                bool[] cols = (bool[])PLCModule.tcClient.ReadAny(
                                    SymbolMap.InputSymbol.CoolantLeakStatus.Handle, typeof(bool[]), new int[] { GPLoaderDef.StageCount });
                                for (int i = 0; i < cols.Count(); i++)
                                {
                                    CoolantLeaks[i] = cols[i];
                                }
                                bool[] das = (bool[])PLCModule.tcClient.ReadAny(
                                    SymbolMap.OutputSymbol.DryAirCommand.Handle, typeof(bool[]), new int[] { GPLoaderDef.StageCount });
                                for (int i = 0; i < das.Count(); i++)
                                {
                                    bool changedstate = false;
                                    if (DryAirValveStates[i] != das[i])
                                    {
                                        changedstate = true;
                                    }
                                    DryAirValveStates[i] = das[i];
                                    if (changedstate)
                                    {
                                        this.GEMModule().GetPIVContainer().SetDryAirValveState(i+1, DryAirValveStates[i]);
                                    }
                                }
                            }
                            //PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CDXOut.Handle, CDXOut);
                            AxisEnabled = ((CDXOutState.nCtrlCmd & 0x01) == 0x01);
                            SyncEvent.Set();
                        }
                        mreUpdateEvent.WaitOne(2);
                        if (errHandled == true)
                        {
                            LoggerManager.Debug($"GPLoader.UpdateProc(): Error recovered.");
                        }
                        errHandled = false;
                    }
                    catch (Exception err)
                    {
                        if (errHandled == false)
                        {
                            LoggerManager.Debug($"GPLoader.UpdateProc(): Error occurred. Err = {err.Message}, Stack trace = {err.StackTrace}");
                            errHandled = true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            stw.Stop();
        }
        public void DeInitModule()
        {
            DeInit();
        }
        public EventCodeEnum InitModule()
        {
            try
            {
                CoolantPressures = new ObservableCollection<int>();
                CoolantLeaks = new ObservableCollection<bool>();
                CoolantInletValveStates = new ObservableCollection<bool>();
                CoolantOutletValveStates = new ObservableCollection<bool>();
                PurgeValveStates = new ObservableCollection<bool>();
                DrainValveStates = new ObservableCollection<bool>();
                DryAirValveStates = new ObservableCollection<bool>();
                InputStates = new List<uint>();
                OutputStates = new List<uint>();
                TesterCoolantValveOpened = new ObservableCollection<bool>();

                for (int i = 0; i < GPLoaderDef.StageCount; i++)
                {
                    CoolantPressures.Add(new int());
                    CoolantLeaks.Add(new bool());
                    CoolantInletValveStates.Add(new bool());
                    CoolantOutletValveStates.Add(new bool());
                    PurgeValveStates.Add(new bool());
                    DrainValveStates.Add(new bool());
                    DryAirValveStates.Add(new bool());
                    TesterCoolantValveOpened.Add(new bool());
                }
                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                {
                    CSTCtrlCmds.Add(EnumCSTCtrl.NONE);
                    CSTCtrlStatus.Add(EnumCSTCtrl.NONE);
                    FOUPDriveErrs.Add(0);
                    FOUPPoss.Add(0);
                    FOUPVelos.Add(0);                    
                }
                for (int i = 0; i < GPLoaderDef.MaxOutputModuleCount; i++)
                {
                    InputStates.Add(new uint());
                }
                for (int i = 0; i < GPLoaderDef.MaxInputModuleCount; i++)
                {
                    OutputStates.Add(new uint());
                }

                preAlignerControlItems = new PreAlignerControlItems();

                EventCodeEnum errorCode = InitRFIDModule_ForCardID();
                if (errorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[RFID] InitRFIDModule_ForCardID Fail. errorCode : {errorCode}");
                }

                if (loaderModule != null)
                {
                    LUD = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LUD);
                    LUU = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LUU);
                    LCC = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LCC);
                    LX = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LX);
                    LZ = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LZM);
                    LW = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LW);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum Connect()
        {
            return InitGPLoaderRouter(SysParam.AMSNetID.Value);
        }


        private EventCodeEnum SetCSTCommand(EnumCSTCommand command)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;

            var prevCmd = CDXOutState.nCSTCtrlCmd;
            if (prevCmd != 0)
            {
                errorCode = EventCodeEnum.LOADER_STATE_INVALID;
            }

            SetLoaderMode(EnumLoaderMode.ACTIVE);
            var result = WaitForMode(EnumLoaderState.ACTIVE);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            lock (plcLockObject)
            {
                CDXOut.nCSTCtrlCmd = (ushort)command;
                PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CDXOut.Handle, CDXOut);      // Update position
            }
            errorCode = EventCodeEnum.NONE;
            // Wait for command update.
            return errorCode;
        }

        #region // PLC logging
        private int _PrevIndex_Robot;
        public int PrevIndex_Robot
        {
            get { return _PrevIndex_Robot; }
            set
            {
                if (value != _PrevIndex_Robot)
                {
                    _PrevIndex_Robot = value;
                }
            }
        }
        private int _CurIndex_Robot;
        public int CurIndex_Robot
        {
            get { return _CurIndex_Robot; }
            set
            {
                if (value != _CurIndex_Robot)
                {
                    _CurIndex_Robot = value;
                }
            }
        }
        private int _PrevIndex_Foup;
        public int PrevIndex_Foup
        {
            get { return _PrevIndex_Foup; }
            set
            {
                if (value != _PrevIndex_Foup)
                {
                    _PrevIndex_Foup = value;
                }
            }
        }
        private int _CurIndex_Foup;
        public int CurIndex_Foup
        {
            get { return _CurIndex_Foup; }
            set
            {
                if (value != _CurIndex_Foup)
                {
                    _CurIndex_Foup = value;
                }
            }
        }

        private int _PLCRingBufferLength = 1000;
        public int PLCRingBufferLength
        {
            get { return _PLCRingBufferLength; }
            set
            {
                if (value != _PLCRingBufferLength)
                {
                    _PLCRingBufferLength = value;
                }
            }
        }
        private List<stProEventInfo> GetCurCmdHistory(int foupidx = -1)
        {
            List<stProEventInfo> retVal = new List<stProEventInfo>();
            int prevIdx;
            int curIdx;
            try
            {
                stProEventInfo[] allLogs;
                if (foupidx != -1)
                {
                    allLogs = GetFoupEventLogBuffer(foupidx);
                    prevIdx = PrevIndex_Foup;
                    curIdx = CurIndex_Foup;
                }
                else
                {
                    allLogs = GetEventLogBuffer();
                    prevIdx = PrevIndex_Robot;
                    curIdx = CurIndex_Robot;
                }

                if (prevIdx < curIdx)
                {
                    for (int i = prevIdx; i < curIdx; i++)
                    {
                        retVal.Add(allLogs[i]);
                    }
                }
                else
                {
                    for (int i = prevIdx; i < PLCRingBufferLength; i++)
                    {
                        retVal.Add(allLogs[i]);
                    }
                    for (int i = 0; i < curIdx; i++)
                    {
                        retVal.Add(allLogs[i]);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region // Robot commands
        private EventCodeEnum SetRobotCommand(EnumRobotCommand command)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            if (CDXOutState != null)
            {
                bool isAccess = true;
                bool isPodup = false;
                int cellIndex = -1;

                //Card Arm Position값을 얻어와서 150000 이상인 경우 메세지 박스 뛰우고 난후 진행할지 말지 결정한다.
                if (command != EnumRobotCommand.IDLE)
                {
                    if (SymbolMap.InputSymbol.EventLog_RingBuffer != null && SymbolMap.InputSymbol.EventLog_RingBuffer.Handle > 0)
                    {
                        PrevIndex_Robot = GetEventLogIndex();
                    }

                    if (LCC != null && LCC.Status.Position.Actual > CardArmfrontPosition)
                    {
                        LoggerManager.Debug($"SetRobotCommand() Loader currrent Pos : LCC:{LCC.Status.Position.Actual}, LX:{LX.Status.Position.Actual}, LZ:{LZ.Status.Position.Actual}, LW:{LW.Status.Position.Actual}");
                        // 다른 셀과의 로딩포지션 차이는 LX 1,000,000 이상 남. 적당히 10만정도.
                        int ccLoadingPosMargin = 100000;
                        double targetpos_X = LX.Status.Position.Actual;
                        double targetpos_W = LW.Status.Position.Actual;
                        double targetpos_Z = LZ.Status.Position.Actual;

                        CCDefinition FindMatchingCCDefinition()
                        {
                            if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                            {
                                foreach (var cc in loaderModule.SystemParameter.CCModules)
                                {
                                    CCAccessParam accessParam = null;
                                    if (cc.AccessParams.Count == 1)
                                    {
                                        accessParam = cc.AccessParams[0];
                                    }
                                    else if (cc.AccessParams.Count == 2)
                                    {
                                        accessParam = cc.AccessParams[1];
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"invalid CC AccessParams. Count is {cc.AccessParams.Count}");
                                        return null;
                                    }

                                    if (accessParam.Position.LX.Value > targetpos_X - ccLoadingPosMargin && accessParam.Position.LX.Value < targetpos_X + ccLoadingPosMargin &&
                                        accessParam.Position.W.Value > targetpos_W - ccLoadingPosMargin && accessParam.Position.W.Value < targetpos_W + ccLoadingPosMargin &&
                                        accessParam.Position.A.Value > targetpos_Z - ccLoadingPosMargin && accessParam.Position.A.Value < targetpos_Z + ccLoadingPosMargin)
                                    {
                                        return cc;
                                    }
                                }
                            }
                            
                            return null;
                        }
                        
                        CCDefinition CC = FindMatchingCCDefinition();
                        
                        if (CC != null)
                        {
                            cellIndex = (int)loaderModule.SystemParameter.CCModules.IndexOf(CC) + 1;

                            if (cellIndex != -1)
                            {
                                EventCodeEnum podStatus = loaderModule.LoaderMaster.GetCardPodStatusClient(cellIndex);
                                if (podStatus == EventCodeEnum.NONE)
                                {
                                    isPodup = false;
                                }
                                else if (podStatus == EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS)
                                {
                                    isPodup = true;
                                }

                                if (isPodup)
                                {
                                    var retVal = this.MetroDialogManager().ShowMessageDialog($"The action cannot be performed.", $"Card Arm Position Warning. Position:{LCC.Status.Position.Actual}\n" +
                                                                                                                                    $"and Cell#{cellIndex} card pod up status", EnumMessageStyle.Affirmative);
                                    isAccess = false;
                                    errorCode = EventCodeEnum.ARM_DANGEROUS_POS;
                                }
                                else
                                {
                                    var retVal = this.MetroDialogManager().ShowMessageDialog($"Card Arm Position Warning. Position:{LCC.Status.Position.Actual}", "Do you want to proceed?", EnumMessageStyle.AffirmativeAndNegative);
                                    if (retVal.Result == EnumMessageDialogResult.AFFIRMATIVE)
                                    {
                                        isAccess = true;
                                    }
                                    else
                                    {
                                        isAccess = false;
                                        errorCode = EventCodeEnum.ARM_DANGEROUS_POS;
                                    }
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"FindMatchingCCDefinition() cell index is invalid");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"FindMatchingCCDefinition() is null");

                            var retVal = this.MetroDialogManager().ShowMessageDialog($"Card Arm Position Warning. Position:{LCC.Status.Position.Actual}", "Do you want to proceed?", EnumMessageStyle.AffirmativeAndNegative);
                            if (retVal.Result == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                isAccess = true;
                            }
                            else
                            {
                                isAccess = false;
                                errorCode = EventCodeEnum.ARM_DANGEROUS_POS;
                            }
                        }
                    }
                }

                if (isAccess)
                {
                    var prevCmd = CDXOutState.nRobotCmd;
                    if (prevCmd != 0)
                    {
                        errorCode = EventCodeEnum.LOADER_STATE_INVALID;
                    }

                    SetLoaderMode(EnumLoaderMode.ACTIVE);
                    var result = WaitForMode(EnumLoaderState.ACTIVE);
                    if (result != EventCodeEnum.NONE)
                    {
                        return result;
                    }
                    lock (plcLockObject)
                    {
                        CDXOut.nRobotCmd = (ushort)command;
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CDXOut.Handle, CDXOut);      // Update position
                    }
                    LoggerManager.Debug($"SetRobotCommand({command}): CDXOut.nRobotCmd = {CDXOut.nRobotCmd}");
                    errorCode = EventCodeEnum.NONE;
                }
            }
            // Wait for command update.
            return errorCode;
        }
        private EventCodeEnum RunPA(int paindex, double notchandgle)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            if (paindex < 0 | paindex > SystemModuleCount.ModuleCnt.PACount)
            {
                errorCode = EventCodeEnum.LOADER_PA_FAILED;
                LoggerManager.Debug($"SetPACommand(): PA(#{paindex}) Not available.");
                return errorCode;
            }
            
            loaderModule.PAManager.PAModules[paindex - 1].DoPreAlign(notchandgle);
            errorCode = EventCodeEnum.NONE;
            // Wait for command update.
            return errorCode;
        }
        private EventCodeEnum SetLoaderMode(EnumLoaderMode mode)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                lock (plcLockObject)
                {
                    //var CDXOutInitState = PLCModule.tcClient.ReadAny(SymbolMap.OutputSymbol.CDXOut.Handle, typeof(stCDXOut));
                    //CDXOut = (stCDXOut)CDXOutInitState;
                    CDXOut.nMode = (ushort)mode;

                    if (PLCModule != null)
                    {
                        if (PLCModule.tcClient.IsConnected)
                        {
                            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CDXOut.Handle, CDXOut);      // Update position
                        }
                    }

                    //TODO: tcClient null check
                }
                errorCode = EventCodeEnum.NONE;
                // Wait for command update.
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return errorCode;
        }

        /// <summary>
        /// 로더가 Move 중인지 확인하는 함수. 
        /// true 이면 동작 중이 아님.
        /// </summary>
        /// <returns></returns>
        public bool IsIdleState()
        {
            bool isidle = false;
            if (CDXIn.nRobotState == (short)EnumRobotState.IDLE)
            {
                isidle = true;
            }
            else
            {
                isidle = false;
            }
            return isidle;
        }

        /// <summary>
        /// 로더가 Card 관련 Move 중인지 확인하는 함수. 
        /// true 이면 동작 중이 아님.
        /// </summary>
        /// <returns></returns>
        public bool IsMovingOnCardOwner()
        {
            bool ismoving = true;
            if (CDXIn.nRobotState == (short)EnumRobotState.CARDBUFF_PICKING ||
                CDXIn.nRobotState == (short)EnumRobotState.CARDBUFF_PICKED ||
                CDXIn.nRobotState == (short)EnumRobotState.CARDBUFF_PUTTING ||
                CDXIn.nRobotState == (short)EnumRobotState.CARDBUFF_PUTED ||
                CDXIn.nRobotState == (short)EnumRobotState.CARDBUFF_FAILED
                )
            {
                ismoving = true;
            }
            else
            {
                ismoving = false;
            }
            return ismoving;
        }


        private EventCodeEnum WaitForCommandDone(EnumRobotState robotState, long timeout = 0)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            Stopwatch elapsedStopWatch = new Stopwatch();
            try
            {
                elapsedStopWatch.Reset();
                elapsedStopWatch.Start();
                bool commandDone = false;
                long waittimeout = timeout;
                bool runFlag = true;
                bool loaderdoorclose = false;
                bool actionFlag = false;
                EnumRobotState robotStateinAction = EnumRobotState.IDLE;
                // WaitCommandDone이라는건 SetRobotCommand를 헀다는 뜻. PLC동작을 수행했다는 뜻.
                loaderModule.LoaderRobotRunning = true;

                do
                {
                    if ((EnumRobotState)CDXIn.nRobotState != EnumRobotState.IDLE && !actionFlag)
                    {
                        robotStateinAction = (EnumRobotState)CDXIn.nRobotState;
                        actionFlag = true;
                    }

                    if ((CDXIn.nRobotState == (short)EnumRobotState.CST_FAILED) ||
                        (CDXIn.nRobotState == (short)EnumRobotState.BUFF_FAILED) ||
                        (CDXIn.nRobotState == (short)EnumRobotState.PA_FAILED) ||
                        (CDXIn.nRobotState == (short)EnumRobotState.STAGE_FAILED) ||
                        (CDXIn.nRobotState == (short)EnumRobotState.FT_FAILED) ||
                        (CDXIn.nRobotState == (short)EnumRobotState.CARDBUFF_FAILED) ||
                        (CDXIn.nRobotState == (short)EnumRobotState.CC_FAILED) ||
                        (CDXIn.nRobotState == (short)EnumRobotState.WAFEROUT_SEN_DETECT))
                    {
                        runFlag = false;
                        var state = (EnumRobotState)CDXIn.nRobotState;
                        switch (state)
                        {
                            case EnumRobotState.CST_FAILED:
                                errorCode = EventCodeEnum.LOADER_CST_FAILED;
                                break;
                            case EnumRobotState.PA_FAILED:
                                errorCode = EventCodeEnum.LOADER_PA_FAILED;
                                break;
                            case EnumRobotState.BUFF_FAILED:
                                errorCode = EventCodeEnum.LOADER_BUFF_FAILED;
                                break;
                            case EnumRobotState.STAGE_FAILED:
                                errorCode = EventCodeEnum.LOADER_STAGE_FAILED;
                                break;
                            case EnumRobotState.CC_FAILED:
                                errorCode = EventCodeEnum.LOADER_CC_FAILED;
                                break;
                            case EnumRobotState.FT_FAILED:
                                errorCode = EventCodeEnum.LOADER_FT_FAILED;
                                break;
                            case EnumRobotState.CARDBUFF_FAILED:
                                errorCode = EventCodeEnum.LOADER_CARDBUFF_FAILED;
                                break;
                            case EnumRobotState.WAFEROUT_SEN_DETECT:
                                errorCode = EventCodeEnum.FOUP_SCAN_WAFEROUT;
                                break;
                            default:
                                errorCode = EventCodeEnum.LOADER_STATE_INVALID;
                                break;
                        }

                        if (robotState != EnumRobotState.IDLE)
                        {
                            try
                            {
                                LoggingRobotEvent(robotStateinAction);
                            }
                            catch (Exception plcerr)
                            {
                                LoggerManager.Debug($"WaitForCommandDone(): Error occurred. Err = {plcerr}");
                            }
                        }
                    }
                    if (CDXIn.nRobotState == (short)robotState)
                    {
                        errorCode = EventCodeEnum.NONE;

                        commandDone = true;

                        SetRobotCommand(EnumRobotCommand.IDLE);
                    }
                    if (commandDone)
                    {
                        if (CDXIn.nRobotState == (short)EnumRobotCommand.IDLE)
                        {
                            runFlag = false;
                            errorCode = EventCodeEnum.NONE;
                        }
                    }
                    if (timeout != 0)
                    {
                        if (Loaderdooropenflag && CDXIn.nRobotState != (short)EnumRobotCommand.IDLE)
                        {
                            timeout = SetTimeouttime(timeout);
                            SetLeftRightLoaderdoorOpen(false);
                            loaderdoorclose = true;
                        }
                        else 
                        {
                            if (loaderdoorclose) 
                            {
                                elapsedStopWatch.Reset();
                                elapsedStopWatch.Start();
                                loaderdoorclose = false;
                                timeout = waittimeout;
                            }
                        }

                        if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                        {

                            if (robotState != EnumRobotState.IDLE)
                            {
                                try
                                {
                                    LoggingRobotEvent(robotStateinAction, true);
                                }
                                catch (Exception plcerr)
                                {
                                    LoggerManager.Debug($"WaitForCommandDone(): Error occurred. Err = {plcerr}");
                                }
                            }
                            runFlag = false;
                            LoggerManager.Debug($"WaitForCommandDone(): Timeout occurred. Target state = {robotState}, Curr. State = {CDXIn.nRobotState},Timeout = {timeout}");
                            errorCode = EventCodeEnum.LOADER_ROBOTCMD_TIMEOUT;
                        }
                        if (loaderModule.LoaderRobotAbortFlag)
                        {
                            runFlag = false;
                            loaderModule.LoaderRobotAbortFlag = false;
                            LoggerManager.Debug($"WaitForCommandDone(): Loader Robot Abort");
                            errorCode = EventCodeEnum.LOADER_ROBOTCMD_ABORT;
                        }
                    }
                } while (runFlag == true);

            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaitForCommandDone(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            finally
            {
                elapsedStopWatch.Stop();
                loaderModule.LoaderRobotRunning = false;
            }
            return errorCode;
        }

        private int GetEventLogIndex()
        {
            int retVal = -1;
            try
            {
                if (SymbolMap.InputSymbol.EventLog_Index != null && SymbolMap.InputSymbol.EventLog_Index.Handle > 0)
                {
                    var eventLogIndex = PLCModule.tcClient.ReadAny(SymbolMap.InputSymbol.EventLog_Index.Handle, typeof(int));
                    retVal = Convert.ToInt32(eventLogIndex);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private int GetFoupEventLogIndex(int foupIdx)
        {
            int retVal = -1;
            try
            {
                if (SymbolMap.InputSymbol.EventLog_Index_Foup != null && SymbolMap.InputSymbol.EventLog_Index_Foup.Handle > 0)
                {                    
                    int arrayLengh = SystemModuleCount.ModuleCnt.FoupCount;
                    var datasize = sizeof(int) / 2;
                    var buffer = new byte[arrayLengh * datasize];
                    var adsStream = new AdsStream(buffer);

                    PLCModule.tcClient.Read(SymbolMap.InputSymbol.EventLog_Index_Foup.Handle, adsStream);
                    var rcvdData = adsStream.ToArray();

                    
                    var intbuffer = new byte[datasize];
                    int dataCount = rcvdData.Length / arrayLengh;
                    int[] foupLogIndexs = new int[arrayLengh];

                    for (int i = 0; i < dataCount; i++)
                    {                        

                        Array.Copy(rcvdData, i * datasize, intbuffer, 0, datasize);

                        foupLogIndexs[i] = BitConverter.ToInt16(intbuffer, 0);
                        //eventLogs[i] = eventLog;
                    }

                    retVal = foupLogIndexs[foupIdx];

                    //var eventLogIndex = PLCModule.tcClient.ReadAny(SymbolMap.InputSymbol.EventLog_Index_Foup.Handle, typeof(int));
                    //retVal = Convert.ToInt32(eventLogIndex);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private stProEventInfo[] GetEventLogBuffer()
        {
            stProEventInfo[] retVal = null;
            try
            {
                //gEI_EventLogs							: ARRAY[gcn_MinCnt..gcn_MaxErrorHistoryNum] OF st_Pro_EventInfo;
                //gInt_EventIndex							: INT;

                //gEI_FoupEventLogs						: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF ARRAY[gcn_MinCnt..gcn_MaxErrorHistoryNum] OF st_Pro_EventInfo;
                //gInt_FoupEventIndex						: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF INT;

                if (SymbolMap.InputSymbol.EventLog_RingBuffer != null && SymbolMap.InputSymbol.EventLog_RingBuffer.Handle > 0)
                {
                    int structSize = stProEventInfo.SizeOf();
                    int arrayLengh = PLCRingBufferLength;

                    var buffer = new byte[structSize * arrayLengh];
                    var adsStream = new AdsStream(buffer);  

                    PLCModule.tcClient.Read(SymbolMap.InputSymbol.EventLog_RingBuffer.Handle, adsStream);
                    var rcvdData = adsStream.ToArray();

                    int structCount = rcvdData.Length / stProEventInfo.SizeOf();

                    stProEventInfo[] eventLogs = new stProEventInfo[structCount];

                    for (int i = 0; i < structCount; i++)
                    {
                        var eventLogBytes = new byte[stProEventInfo.SizeOf()];

                        Array.Copy(rcvdData, i * stProEventInfo.SizeOf(), eventLogBytes, 0, stProEventInfo.SizeOf());
                        var eventLog = new stProEventInfo
                        {
                            RobotState = BitConverter.ToInt16(eventLogBytes, 0),
                            EventTime = stProEventInfo.ConvertDateTime(eventLogBytes, 2),
                            EventCode = BitConverter.ToInt16(eventLogBytes, sizeof(Int16) + 6),
                            PrevEventCode = BitConverter.ToInt16(eventLogBytes, sizeof(Int16) * 2 + 6)
                        };

                        eventLogs[i] = eventLog;
                    }
                    retVal = eventLogs;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private stProEventInfo[] GetFoupEventLogBuffer(int FoupIdx)
        {
            stProEventInfo[] retVal = null;
            try
            {   //gEI_EventLogs							: ARRAY[gcn_MinCnt..gcn_MaxErrorHistoryNum] OF st_Pro_EventInfo;
                //gInt_EventIndex							: INT;

                //gEI_FoupEventLogs						: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF ARRAY[gcn_MinCnt..gcn_MaxErrorHistoryNum] OF st_Pro_EventInfo;
                //gInt_FoupEventIndex						: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF INT;


                if (SymbolMap.InputSymbol.EventLog_RingBuffer_Foup != null && SymbolMap.InputSymbol.EventLog_RingBuffer_Foup.Handle > 0)
                {
                    int structSize = stProEventInfo.SizeOf();
                    int arrayLengh = PLCRingBufferLength;
                    int foupCnt = SystemModuleCount.ModuleCnt.FoupCount;

                    var buffer = new byte[structSize * arrayLengh * foupCnt];

                    var adsStream = new AdsStream(buffer);

                    PLCModule.tcClient.Read(SymbolMap.InputSymbol.EventLog_RingBuffer_Foup.Handle, adsStream);
                    var rcvdData = adsStream.ToArray();


                    List<stProEventInfo[]> foupEventLogs = new List<stProEventInfo[]>();
                    for (int i = 0; i < foupCnt; i++)
                    {
                        int startPos = i * structSize * arrayLengh;
                        int structCount = rcvdData.Length / stProEventInfo.SizeOf() / foupCnt;
                        stProEventInfo[] eventLogs = new stProEventInfo[structCount];

                        for (int j = 0; j < structCount; j++)
                        {
                            var eventLogBytes = new byte[stProEventInfo.SizeOf()];

                            Array.Copy(rcvdData, startPos + j * stProEventInfo.SizeOf(), eventLogBytes, 0, stProEventInfo.SizeOf());
                            var eventLog = new stProEventInfo
                            {
                                RobotState = BitConverter.ToInt16(eventLogBytes, 0),
                                EventTime = stProEventInfo.ConvertDateTime(eventLogBytes, 2),
                                EventCode = BitConverter.ToInt16(eventLogBytes, sizeof(Int16) + 6),
                                PrevEventCode = BitConverter.ToInt16(eventLogBytes, sizeof(Int16) * 2 + 6)
                            };

                            eventLogs[j] = eventLog;
                        }
                        foupEventLogs.Add(eventLogs);
                    }

                    retVal = foupEventLogs[FoupIdx];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        private long SetTimeouttime(long timeout)
        {
            long retval = 0;
            try 
            {
                long blockingActiontimeout = loaderModule.GetBlockingJobtimeforOpenedDoor();
                if (blockingActiontimeout != 0)
                {
                    retval = blockingActiontimeout;
                }
                else 
                {
                    retval = timeout;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Timeoutcheckoption(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return timeout;
        }

        public void SetLeftRightLoaderdoorOpen(bool flag) 
        {
            try
            {
                Loaderdooropenflag = flag;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Timeoutcheckoption(): Exception occurred. Err = {err.Message}");
                throw err;
            }
        }
        public EventCodeEnum WriteWaitHandle(short value)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return EventCodeEnum.NONE;
                }

                if (SymbolMap.InputSymbol.WaitHandle.Handle != -1)
                {
                    PLCModule.tcClient.WriteAny(SymbolMap.InputSymbol.WaitHandle.Handle, value);
                    LoggerManager.Debug($"WriteWaitHandle(): Handle value is [{value}]");
                }
                errorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                errorCode = EventCodeEnum.LOADER_SYSTEM_ERROR;
                LoggerManager.Debug($"WriteWaitHandle(): Error occurre. Err = {err.Message}");
            }
            return errorCode;
        }
        public int ReadWaitHandle()
        {
            int handleState = -1;
            try
            {

                if (SymbolMap.InputSymbol.WaitHandle.Handle != -1)
                {
                    handleState = (int)PLCModule.tcClient.ReadAny(SymbolMap.InputSymbol.WaitHandle.Handle, typeof(int));
                }
            }
            catch (Exception err)
            {
                handleState = -1;
                LoggerManager.Debug($"ReadWaitHandle(): Error occurre. Err = {err.Message}");
            }
            return handleState;
        }
        public EventCodeEnum WaitForHandle(short handle, long timeout = 60000)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            Stopwatch elapsedStopWatch = new Stopwatch();
            try
            {
                if (SymbolMap.InputSymbol.WaitHandle.Handle != -1)
                {


                    elapsedStopWatch.Reset();
                    elapsedStopWatch.Start();
                    bool commandDone = false;
                    lock (plcLockObject)
                    {
                        bool runFlag = true;
                        do
                        {
                            var handleState = (int)PLCModule.tcClient.ReadAny(SymbolMap.InputSymbol.WaitHandle.Handle, typeof(int));
                            if (handleState == (short)handle)
                            {
                                commandDone = true;
                            }
                            if (commandDone == true)
                            {
                                if (handleState == (short)handle)
                                {
                                    runFlag = false;
                                    errorCode = EventCodeEnum.NONE;
                                }
                            }
                            if (timeout != 0)
                            {
                                if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                                {
                                    runFlag = false;
                                    LoggerManager.Debug($"WaitForHandle(): Timeout occurred. Target state = {handle}, Curr. State = {handleState}, Timeout = {timeout}");
                                    errorCode = EventCodeEnum.LOADER_ROBOTCMD_TIMEOUT;
                                }
                            }
                        } while (runFlag == true);
                    }
                }
                else
                {
                    errorCode = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaitForHandle(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            finally
            {
                elapsedStopWatch.Stop();
            }
            return errorCode;
        }


        private EventCodeEnum WaitForCSTCommandDone(EnumCSTCommand cstState, long timeout = 0)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            Stopwatch elapsedStopWatch = new Stopwatch();
            try
            {
                elapsedStopWatch.Reset();
                elapsedStopWatch.Start();
                bool commandDone = false;

                lock (plcLockObject)//#Hynix_Merge: Hynix 코드에만 Lock있음.
                {
                    bool runFlag = true;
                    do
                    {
                        if (CDXIn.nCSTCtrlState == (short)cstState)
                        {
                            commandDone = true;
                            SetCSTCommand(EnumCSTCommand.IDLE);
                        }
                        if (commandDone == true)
                        {
                            if (CDXIn.nCSTCtrlState == (short)cstState)
                            {
                                runFlag = false;
                                errorCode = EventCodeEnum.NONE;
                            }
                        }
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {
                                runFlag = false;
                                LoggerManager.Debug($"WaitForCommandDone(): Timeout occurred. Target state = {cstState}, Timeout = {timeout}");
                                errorCode = EventCodeEnum.LOADER_ROBOTCMD_TIMEOUT;
                            }
                        }
                    } while (runFlag == true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaitForCommandDone(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            finally
            {
                elapsedStopWatch.Stop();
            }
            return errorCode;
        }
        private EventCodeEnum WaitForCSTCommandDone(EnumCSTState cstState, int cstIdx, long timeout = 0)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            Stopwatch elapsedStopWatch = new Stopwatch();
            try
            {
                elapsedStopWatch.Reset();
                elapsedStopWatch.Start();
                bool commandDone = false;

                lock (plcLockObject)// #Hynix_Merge: Hynix 코드에만 Lock 있음.
                {
                    bool runFlag = true;
                    do
                    {
                        if ((CDXIn.nCSTState[cstIdx] == (short)EnumCSTState.NODETECTERROR) ||
                            (CDXIn.nCSTState[cstIdx] == (short)EnumCSTState.TIMEOUTERROR) ||
                            (CDXIn.nCSTState[cstIdx] == (short)EnumCSTState.WAFEROUT))
                        {
                            runFlag = false;
                            var state = (EnumCSTState)CDXIn.nCSTState[cstIdx];
                            switch (state)
                            {
                                case EnumCSTState.NODETECTERROR:
                                    errorCode = EventCodeEnum.FOUP_SCAN_NOTDETECT;
                                    runFlag = false;
                                    break;
                                case EnumCSTState.TIMEOUTERROR:
                                    errorCode = EventCodeEnum.FOUP_TIMEOUT;
                                    runFlag = false;
                                    break;
                                case EnumCSTState.WAFEROUT:
                                    errorCode = EventCodeEnum.FOUP_SCAN_WAFEROUT;
                                    runFlag = false;
                                    break;
                            }
                        }
                        if (CDXIn.nCSTState[cstIdx] == (short)cstState)
                        {
                            commandDone = true;
                            SetCSTCommand(EnumCSTCommand.IDLE);
                        }
                        if (commandDone == true)
                        {
                            if (CDXIn.nCSTState[cstIdx] == (short)cstState)
                            {
                                runFlag = false;
                                errorCode = EventCodeEnum.NONE;
                            }
                        }
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {
                                runFlag = false;
                                LoggerManager.Debug($"WaitForCommandDone(): Timeout occurred at Cassette #{cstIdx}. Target state = {cstState}, Timeout = {timeout}");
                                errorCode = EventCodeEnum.LOADER_ROBOTCMD_TIMEOUT;
                            }
                        }
                    } while (runFlag == true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaitForCommandDone(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            finally
            {
                elapsedStopWatch.Stop();
            }
            return errorCode;
        }
        private EventCodeEnum WaitForMode(EnumLoaderState demandstate, long timeout = 3000)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            Stopwatch elapsedStopWatch = new Stopwatch();
            try
            {
                elapsedStopWatch.Reset();
                elapsedStopWatch.Start();

                bool runFlag = true;
                do
                {
                    if (CDXIn.nState == (short)demandstate)
                    {
                        runFlag = false;
                        //LoggerManager.Debug($"WaitForMode(): Command = {demandstate}.");
                        errorCode = EventCodeEnum.NONE;
                    }
                    if (timeout != 0)
                    {
                        if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                        {
                            runFlag = false;
                            LoggerManager.Debug($"WaitForMode(): Timeout occurred. Demand state = {demandstate}, Timeout = {timeout}");
                            errorCode = EventCodeEnum.LOADER_ROBOTCMD_TIMEOUT;
                        }
                    }
                    if (CDXIn.nState == (short)EnumLoaderState.ERROR)
                    {
                        runFlag = false;
                        LoggerManager.Debug($"WaitForMode(): Error occurred. Demand state = {demandstate}");
                        errorCode = EventCodeEnum.LOADER_STATE_INVALID;
                    }
                    if (demandstate == EnumLoaderState.HOMED)
                    {
                        if (CDXIn.nState == (short)EnumLoaderState.RESET)
                        {                           
                            runFlag = false;
                            LoggerManager.Debug($"WaitForMode(): In idle state. Demand state = {demandstate}, skip await.");
                            errorCode = EventCodeEnum.LOADER_STATE_INVALID;
                        }

                    }
                } while (runFlag == true);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaitForMode(): Exception occurred. Err = {err.Message}");
                // 여기서 Loader - IsMoving = false 로 풀어줘야함. 
                throw err;
            }
            finally
            {
                elapsedStopWatch.Stop();
            }
            if (errorCode != EventCodeEnum.NONE)
                this.NotifyManager().Notify(errorCode);
            return errorCode;
        }
        private bool InitController()
        {
            bool initresult = false;
            try
            {

                SetLoaderMode(EnumLoaderMode.IDLE);
                var result = WaitForMode(EnumLoaderState.IDLE);
                if (result == EventCodeEnum.LOADER_STATE_INVALID)
                {
                    Thread.Sleep(100);
                    SetLoaderMode(EnumLoaderMode.RESET);
                    Thread.Sleep(100);
                    result = WaitForMode(EnumLoaderState.RESET, 1500);
                    if (result == EventCodeEnum.LOADER_ROBOTCMD_TIMEOUT)
                    {
                        result = WaitForMode(EnumLoaderState.IDLE, 1500);
                    }
                }
                else if (result != EventCodeEnum.NONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("InitController Error", "WaitFor Idle(1) Error", EnumMessageStyle.Affirmative);
                    return initresult;
                }
                lock (plcLockObject)
                {
                    CDXOut.nRobotCmd = 0;
                    CDXOut.nCtrlCmd = 0;
                    CDXOut.nCSTPos = 1;
                    CDXOut.nStagePos = 1;
                    CDXOut.nWaferSlotPos = 1;
                    CDXOut.nFixTrayPos = 1;
                    CDXOut.nLTSlotPos = 1;
                    CDXOut.nCardBufferPos = 1;
                    CDXOut.nPreAPos = 1;
                    WriteCDXOut();
                }
                Thread.Sleep(200);

                PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Visu_Active.Handle, false);

                SetLoaderMode(EnumLoaderMode.IDLE);
                result = WaitForMode(EnumLoaderState.IDLE);
                if (result != EventCodeEnum.NONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("InitController Error", "WaitFor Idle(2) Error", EnumMessageStyle.Affirmative);
                    return initresult;
                }

                Thread.Sleep(800);
                SetLoaderMode(EnumLoaderMode.RESET);
                result = WaitForMode(EnumLoaderState.RESET, 1500);
                if (result == EventCodeEnum.LOADER_ROBOTCMD_TIMEOUT)
                {
                    result = WaitForMode(EnumLoaderState.IDLE, 1500);
                }

                Thread.Sleep(200);
                if (result != EventCodeEnum.NONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("InitController Error", "WaitFor Reset Error !", EnumMessageStyle.Affirmative);
                    return initresult;
                }
                Thread.Sleep(200);
                SetLoaderMode(EnumLoaderMode.IDLE);
                result = WaitForMode(EnumLoaderState.IDLE);
                if (result != EventCodeEnum.NONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("InitController Error", "WaitFor Idle(3) Error", EnumMessageStyle.Affirmative);
                    return initresult;
                }
                result = ResetCtrlCommand();

                if (result == EventCodeEnum.NONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("InitController Done", "InitInitController Successed", EnumMessageStyle.Affirmative);
                    initresult = true;
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("InitController Error", "InitInitController Failed", EnumMessageStyle.Affirmative);
                    initresult = false;
                }
                //SetLoaderMode(EnumLoaderMode.HOME);
                //result = WaitForMode(EnumLoaderState.HOMED, 90000);
                //if (result == EventCodeEnum.NONE)
                //{
                //    SetLoaderMode(EnumLoaderMode.ACTIVE);
                //    WaitForMode(EnumLoaderState.ACTIVE);

                //    SetRobotCommand(EnumRobotCommand.IDLE);
                //}
                //else
                //{
                //    LoggerManager.Error($"Loader init. error occurred!");
                //}
                LUD = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LUD);
                LUU = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LUU);
                LCC = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LCC);
                LX = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LX);
                LZ = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LZM);
                LW = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LW);
                IsLoaderBusy = false;
            }
            catch (Exception err)
            {
                initresult = false;
                LoggerManager.Error($"Loader init. error occurred! Err = {err}");
            }
            return initresult;
        }

        public EventCodeEnum SetCardTrayVac(bool value)
        {
            EventCodeEnum result = EventCodeEnum.UNDEFINED;
            try
            {
                if (PLCModule.tcClient.IsConnected)
                {
                    PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CardTrayVac.Handle, value);
                    result = EventCodeEnum.NONE;
                }
                else
                {
                    result = EventCodeEnum.IO_DEV_CONN_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"SetCardTrayVac({value}): Error occurred. Err = {err.Message}");
            }
            return result;
        }

        public EventCodeEnum ResetRobotCommand()
        {
            EventCodeEnum result = EventCodeEnum.UNDEFINED;
            try
            {

                result = SetRobotCommand(EnumRobotCommand.IDLE);
                if (result != EventCodeEnum.NONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("ResetCtrlCommand Error", $"SetRobotCommand(IDLE) Error. Error={result}", EnumMessageStyle.Affirmative);
                    return result;
                }
                long timeOut = DefaultJobTimeout;
                result = WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (result != EventCodeEnum.NONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("ResetCtrlCommand Error", $"WaitForCommandDone Idle Error. Error={result}", EnumMessageStyle.Affirmative);
                    return result;
                }

                //Thread.Sleep(200);
                //WriteCDXOut();

                //delays.DelayFor(200);
                Thread.Sleep(200);
                lock (plcLockObject)
                {
                    CDXOut.nCtrlCmd = 1;
                    WriteCDXOut();
                }
                Thread.Sleep(1000);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Loader ResetCtrlCommand. error occurred! Err = {err}");
            }
            return result;
        }

        private EventCodeEnum ResetCtrlCommand()
        {
            EventCodeEnum result = EventCodeEnum.UNDEFINED;
            try
            {
                lock (plcLockObject)
                {
                    CDXOut.nCSTCtrlCmd = 0;
                    WriteCDXOut();
                }
                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                {
                    var cststate = GetCSTState(i);
                    var CstModules = loaderModule.ModuleManager.FindModules<ICassetteModule>();
                    if (cststate == EnumCSTState.EMPTY || cststate == EnumCSTState.TIMEOUTERROR || cststate == EnumCSTState.NODETECTERROR || cststate == EnumCSTState.RESET || cststate == EnumCSTState.IDLE
                        || CstModules[i].FoupState == FoupStateEnum.ERROR)
                    {
                        LoggerManager.Debug($"Loader Init, Foup{i + 1}, CST State = {cststate}, foup state = {CstModules[i].FoupState.ToString()}  FOUPReset Start.");
                        result = FOUPReset(i);

                        if (result != EventCodeEnum.NONE)
                        {
                            this.MetroDialogManager().ShowMessageDialog("ResetCtrlCommand Error", $"Foup{i + 1} Reset Error. Error={result}", EnumMessageStyle.Affirmative);
                            return result;
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"Loader Init, Foup{i + 1}, CST State = {cststate}, foup state = {CstModules[i].FoupState.ToString()} FOUPReset Skipped.");
                    }
                }

                result = SetRobotCommand(EnumRobotCommand.IDLE);
                if (result != EventCodeEnum.NONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("ResetCtrlCommand Error", $"SetRobotCommand(IDLE) Error. Error={result}", EnumMessageStyle.Affirmative);
                    return result;
                }
                long timeOut = DefaultJobTimeout;
                result = WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (result != EventCodeEnum.NONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("ResetCtrlCommand Error", $"WaitForCommandDone Idle Error. Error={result}", EnumMessageStyle.Affirmative);
                    return result;
                }

                //Thread.Sleep(200);
                //WriteCDXOut();

                //delays.DelayFor(200);
                Thread.Sleep(200);
                lock (plcLockObject)
                {
                    CDXOut.nCtrlCmd = 1;
                    WriteCDXOut();
                }
                Thread.Sleep(1000);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Loader ResetCtrlCommand. error occurred! Err = {err}");
            }
            return result;
        }
        private bool HomingSystem()
        {
            var homingResult = false;
            try
            {
                SetRobotCommand(EnumRobotCommand.IDLE);

                SetLoaderMode(EnumLoaderMode.RESET);
                WaitForMode(EnumLoaderState.RESET);

                if (CDXIn.nState == (short)EnumLoaderState.ERROR)
                {
                    //Thread.Sleep(200);
                    Thread.Sleep(200);
                    SetLoaderMode(EnumLoaderMode.RESET);
                    WaitForMode(EnumLoaderState.RESET);
                    //Thread.Sleep(500);
                    Thread.Sleep(500);
                }
                SetLoaderMode(EnumLoaderMode.IDLE);
                var result = WaitForMode(EnumLoaderState.IDLE);
                if (result != EventCodeEnum.NONE)
                {
                    homingResult = false;
                    LoggerManager.Error($"HomingSystem(): Set to idle state has been failed. Result = {result}");
                    this.MetroDialogManager().ShowMessageDialog("Homing Error", $"HomingSystem(): Set to idle state has been failed. Result = {result}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //Thread.Sleep(500);
                    Thread.Sleep(500);
                    LoggerManager.Debug($"HomingSystem Start!");
                    SetLoaderMode(EnumLoaderMode.HOME);
                    LoggerManager.Debug($"PA Initializing...");

                    for (int i = 0; i < SystemModuleCount.ModuleCnt.PACount; i++)
                    {
                        loaderModule.PAManager.PAModules[i].UpdateState();
                        LoggerManager.Debug($"HomingSystem(): PAModule [{i}] PA State (OriginDone) : {loaderModule.PAManager.PAModules[i].State.OriginDone} , PA Status {loaderModule.PAManager.PAModules[i].PAStatus}");
                        loaderModule.PAManager.PAModules[i].ModuleInit();
                    }

                    LoggerManager.Debug($"PA Initialized.");
                    LoggerManager.Debug($"Wait for homing...");
                    result = WaitForMode(EnumLoaderState.HOMED, 300000);
                    if (result == EventCodeEnum.NONE)
                    {
                        SetLoaderMode(EnumLoaderMode.ACTIVE);
                        result = WaitForMode(EnumLoaderState.ACTIVE);

                        lock (plcLockObject)
                        {
                            CDXOut.nCSTCtrlCmd = 0;
                            WriteCDXOut();
                        }

                        int foupCount = SystemModuleCount.ModuleCnt.FoupCount;
                        List<EventCodeEnum> fouphomingresult = new List<EventCodeEnum>(new EventCodeEnum[foupCount]);
                        for (int i = 0; i < foupCount; i++)
                        {
                            var CstModules = loaderModule.ModuleManager.FindModules<ICassetteModule>();
                            
                            var cststate = GetCSTState(i);
                            if (cststate == EnumCSTState.EMPTY || cststate == EnumCSTState.TIMEOUTERROR || cststate == EnumCSTState.NODETECTERROR || cststate == EnumCSTState.RESET || cststate == EnumCSTState.IDLE
                                || CstModules[i].FoupState == FoupStateEnum.ERROR)
                            {
                                LoggerManager.Debug($"Loader Homming, Foup{i + 1}, CST State = {cststate}, foup state = {CstModules[i].FoupState.ToString()} FOUPReset Start.");
                                result = FOUPReset(i);

                                if (result != EventCodeEnum.NONE)
                                {
                                    fouphomingresult[i] = EventCodeEnum.FOUP_HOMMING_ERROR;
                                    this.MetroDialogManager().ShowMessageDialog("Homing Error", $"Foup{i + 1} Reset Error. Error={result}", EnumMessageStyle.Affirmative);
                                    LoggerManager.Error($"FOUPReset error occurred!");
                                    break;
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"Loader Homming, Foup{i + 1}, CST State = {cststate}, foup state = {CstModules[i].FoupState.ToString()} FOUPReset Skipped.");
                            }

                            result = CoverClose(i);
                            if (result != EventCodeEnum.NONE)
                            {
                                if (CstModules[i].Enable == true)
                                {
                                    fouphomingresult[i] = EventCodeEnum.FOUP_HOMMING_ERROR;
                                    this.MetroDialogManager().ShowMessageDialog("Homing Error", $"Foup{i + 1} Cover Close Error. Error={result}", EnumMessageStyle.Affirmative);
                                    LoggerManager.Error($"CoverClose error occurred!");
                                    break;
                                }
                                else
                                {
                                    fouphomingresult[i] = EventCodeEnum.NONE;
                                    LoggerManager.Debug($"Foup #{i} CST Disabled.");
                                }
                            }
                            else
                            {
                                fouphomingresult[i] = EventCodeEnum.NONE;
                            }
                        }

                        if (result == EventCodeEnum.NONE && fouphomingresult.All(x => x.Equals(EventCodeEnum.NONE)))
                        {
                            homingResult = true;

                        }
                        else
                        {
                            homingResult = false;
                            LoggerManager.Error($"HomingSystem error occurred!");
                        }
                    }
                    else
                    {
                        DisableAxis();
                        homingResult = false;
                    }
                }
            }
            catch (Exception err)
            {
                homingResult = false;
                LoggerManager.Error($"HomingSystem error occurred! Err = {err}");
            }
            return homingResult;
        }

        public void HomingResultAlarm(EventCodeEnum result)
        {
            try
            {
                if (result == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"HomingResultAlarm(): HomingSystem done!");
                    this.MetroDialogManager().ShowMessageDialog("Homing Done", "HomingSystem done!", EnumMessageStyle.Affirmative);
                }
                else
                {
                    LoaderBuzzer(true);
                    LoggerManager.Debug($"HomingResultAlarm(): HomingSystem failed! Error Code = {result}");
                    this.MetroDialogManager().ShowMessageDialog("Homing Error", "HomingSystem error occurred!\n Please InitSystem Button Click and Homming Again", EnumMessageStyle.Affirmative);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"HomingResultAlarm(): Err = {err}");
            }
        }
        private void VaildWaferinfo()
        {
            try
            {
                for (int i = 0; i < SystemModuleCount.ModuleCnt.PACount; i++)
                {
                    WaferNotchTypeEnum NOTCH = WaferNotchTypeEnum.NOTCH;
                    SubstrateSizeEnum Size = SubstrateSizeEnum.INCH12;
                    var waferstatus = this.loaderModule.GetLoaderInfo().StateMap.PreAlignModules[i].WaferStatus;
                    if (waferstatus == EnumSubsStatus.EXIST)
                    {
                        if (this.loaderModule.GetLoaderInfo().StateMap.PreAlignModules[i].Substrate != null)
                        {
                            NOTCH = this.loaderModule.GetLoaderInfo().StateMap.PreAlignModules[i].Substrate.NotchType;
                            Size = this.loaderModule.GetLoaderInfo().StateMap.PreAlignModules[i].Substrate.Size.Value;
                        }

                        if (Size == SubstrateSizeEnum.INCH6)
                        {
                            NOTCH = WaferNotchTypeEnum.FLAT;
                        }
                    }

                    if (waferstatus == EnumSubsStatus.UNDEFINED || waferstatus == EnumSubsStatus.UNDEFINED ||
                        Size == SubstrateSizeEnum.UNDEFINED || Size == SubstrateSizeEnum.INVALID)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Homing Abort", $"PA [{i}] Initialized error occurred! wafer Size is undefined.\nPlease remove the wafer on the pa by manual", EnumMessageStyle.Affirmative);
                        LoggerManager.Error($"VaildWaferinfo(): error occurred! Wafer status: {waferstatus}, Wafer Size {Size} ");
                    }
                    else
                    {
                        LoggerManager.Debug($"VaildWaferinfo(): SetDeviceSize() Size{Size},NOTCH {NOTCH}.");
                        loaderModule.PAManager.PAModules[i].SetDeviceSize(Size, NOTCH);
                        var retval = loaderModule.PAManager.PAModules[i].ModuleReset();
                        if (retval != EventCodeEnum.NONE)
                        {
                            this.MetroDialogManager().ShowMessageDialog("Homing Abort", $"PA [{i}] Initialized error occurred! PA moves to loading pos.\nPlease check pa status", EnumMessageStyle.Affirmative);
                        }
                    }
                    
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"HomingSystem error occurred! Err = {err}");
            }
        }

       
        private bool IsControllerInIdle()
        {
            bool idle = false;


            //lock (plcLockObject)
            //{
            if (CDXIn.nRobotState == (short)EnumRobotState.IDLE)
            {
                idle = true;
            }
            //}
            return idle;
        }

        private bool IsCSTControllerInIdle(int cstNum)
        {
            bool idle = false;
            if (cstNum > -1 & cstNum < SystemModuleCount.ModuleCnt.FoupCount)
            {
                if (GetCSTState(cstNum) == EnumCSTState.EMPTY
                    | GetCSTState(cstNum) == EnumCSTState.UNLOADED
                    | GetCSTState(cstNum) == EnumCSTState.LOADED
                    | GetCSTState(cstNum) == EnumCSTState.PRESENCE)
                {
                    idle = true;
                }
            }
            else
            {
                LoggerManager.Debug($"IsCSTControllerInIdle(Cassette Num = {cstNum}): Invalid Cassette.");
            }

            //lock (plcLockObject)
            //{
            //if (cstNum == 1)
            //{
            //    if (CDXIn.nFoup1_State == EnumCSTState.EMPTY 
            //        | CDXIn.nFoup1_State == EnumCSTState.UNLOADED
            //        | CDXIn.nFoup1_State == EnumCSTState.LOADED
            //        | CDXIn.nFoup1_State == EnumCSTState.PRESENCE)
            //    {
            //        idle = true;
            //    }
            //}
            //else if (cstNum == 2)
            //{
            //    if (CDXIn.nFoup2_State == EnumCSTState.EMPTY
            //        | CDXIn.nFoup2_State == EnumCSTState.UNLOADED
            //        | CDXIn.nFoup2_State == EnumCSTState.LOADED
            //        | CDXIn.nFoup2_State == EnumCSTState.PRESENCE)
            //    {
            //        idle = true;
            //    }
            //}
            //else if (cstNum == 3)
            //{
            //    if (CDXIn.nFoup3_State == EnumCSTState.EMPTY
            //        | CDXIn.nFoup3_State == EnumCSTState.UNLOADED
            //        | CDXIn.nFoup3_State == EnumCSTState.LOADED
            //        | CDXIn.nFoup3_State == EnumCSTState.PRESENCE)
            //    {
            //        idle = true;
            //    }
            //}
            //}
            return idle;
        }
        public void EnableAxis()
        {
            lock (plcLockObject)
            {
                CDXOut.nCtrlCmd = 1;
                WriteCDXOut();
            }
        }
        public void DisableAxis()
        {
            lock (plcLockObject)
            {
                CDXOut.nCtrlCmd = 0;
                WriteCDXOut();
            }
        }

        private void WriteCDXOut()
        {
            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CDXOut.Handle, CDXOut);      // Update position
        }        
        private object LockObject = new object();
        public EventCodeEnum CassettePick(ISlotModule slot, IARMModule arm)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                var cstIndex = slot.Cassette;
                var localSlotNumber = slot;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {

                    lock (LockObject)
                    {
                        if (loaderModule.LoaderMaster.GetIsAlwaysCloseFoupCover())
                        {
                            // 무조건 연다.
                            errorCode = CoverOpen(slot.Cassette.ID.Index - 1);
                            if(errorCode == EventCodeEnum.NONE)
                            {
                                slot.Cassette.FoupCoverState = FoupCoverStateEnum.OPEN;
                                errorCode = EventCodeEnum.NONE;
                            }
                            else
                            {
                                throw new Exception($"GPLoader().CassettePick(): Error state.");
                            }
                            
                        }
                    }

                    lock (plcLockObject)
                    {
                        CDXOut.nCSTPos = (short)(cstIndex.ID.Index);
                        CDXOut.nWaferSlotPos = (short)(localSlotNumber.ID.Index - (cstIndex.ID.Index - 1) * 25);
                        CDXOut.nArmIndex = (ushort)armIndex;
                        WriteCDXOut();
                    }
                    LoggerManager.Debug($"CassettePick(): Pick wafer from {CDXOut.nWaferSlotPos} Slot of {CDXOut.nCSTPos} Cassette with {CDXOut.nArmIndex} Arm");

                    errorCode = SetRobotCommand(EnumRobotCommand.CST_PICK);

                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.CST_PICKED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"CassettePick(): Cassette Pick Error {errorCode}.";
                LoggerManager.Error($"CassettePick(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum CassettePut(IARMModule arm, ISlotModule slot)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                var cstIndex = slot.Cassette;
                var localSlotNumber = slot;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {

                    lock (LockObject)
                    {
                        if (loaderModule.LoaderMaster.GetIsAlwaysCloseFoupCover())
                        {
                            // 무조건 연다.
                            errorCode = CoverOpen(slot.Cassette.ID.Index - 1);
                            if (errorCode == EventCodeEnum.NONE)
                            {
                                slot.Cassette.FoupCoverState = FoupCoverStateEnum.OPEN;
                                errorCode = EventCodeEnum.NONE;
                            }
                            else
                            {
                                throw new Exception($"GPLoader().CassettePut(): Error state.");
                            }

                        }
                    }

                    CDXOut.nCSTPos = (short)(cstIndex.ID.Index);
                    CDXOut.nWaferSlotPos = (short)(localSlotNumber.ID.Index - (cstIndex.ID.Index - 1) * 25);
                    CDXOut.nArmIndex = (ushort)armIndex;
                    LoggerManager.Debug($"CassettePut(): Put wafer to {CDXOut.nWaferSlotPos} Slot of {CDXOut.nCSTPos} Cassette with {CDXOut.nArmIndex} Arm");

                    errorCode = SetRobotCommand(EnumRobotCommand.CST_PUT);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.CST_PUTED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"CassettePut(): Cassette Put Error {errorCode}.";
                LoggerManager.Error($"CassettePut(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum PAPick(IPreAlignModule pa, IARMModule arm)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    loaderModule.PAManager.PAModules[pa.ID.Index - 1].WaitForPA(5000);
                    errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].ReleaseSubstrate();
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"PAPick(): PA#{pa.ID.Index} Release error. ErrorCode = {errorCode}");
                        return errorCode;
                    }

                    if (loaderModule.PAManager.PAModules[pa.ID.Index - 1].State.AlignDone == false)
                    {
                        lock (plcLockObject)
                        {
                            CDXOut.nPreAPos = (short)pa.ID.Index;
                            CDXOut.nArmIndex = (ushort)armIndex;
                            WriteCDXOut();
                        }
                        errorCode = SetRobotCommand(EnumRobotCommand.PA_PICK);
                        if (errorCode != EventCodeEnum.NONE)
                        {
                            throw new ProberSystemException(errorCode);
                        }
                        errorCode = WaitForCommandDone(EnumRobotState.PA_PICKED, timeOut);
                        if (errorCode != EventCodeEnum.NONE)
                        {
                            throw new ProberSystemException(errorCode);
                        }

                        errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].ModuleReset();
                        if (errorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"PAPick(): PA#{pa.ID.Index} Reset error. ErrorCode = {errorCode}");
                            return errorCode;
                        }

                        if (loaderModule.LoaderMaster.ModuleState.GetState() == ProberInterfaces.ModuleStateEnum.RUNNING || loaderModule.LoaderMaster.ModuleState.GetState() == ProberInterfaces.ModuleStateEnum.ABORT)
                        {
                            loaderModule.PAManager.PAModules[pa.ID.Index - 1].State.Busy = true;
                        }
                        
                        errorCode = PAPut_NotOCR(arm, pa);
                        if (errorCode != EventCodeEnum.NONE)
                        {
                            throw new ProberSystemException(errorCode);
                        }
                        
                        loaderModule.PAManager.PAModules[pa.ID.Index - 1].WaitForPA(20000);
                        errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].ReleaseSubstrate();
                        if (errorCode != EventCodeEnum.NONE)
                        {

                            LoggerManager.Debug($"PAPick(): {CDXOut.nPreAPos} Pre-aligner error occurred. ErrorCode = {errorCode}");
                            throw new ProberSystemException(errorCode);
                        }
                    }
                    else if (errorCode != EventCodeEnum.NONE && false)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    CDXOut.nPreAPos = (short)pa.ID.Index;
                    CDXOut.nArmIndex = (ushort)armIndex;
                    LoggerManager.Debug($"PAPick(): Pick wafer from {CDXOut.nPreAPos} Pre-aligner with {CDXOut.nArmIndex} Arm");

                    errorCode = SetRobotCommand(EnumRobotCommand.PA_PICK);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.PA_PICKED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    loaderModule.PAManager.PAModules[pa.ID.Index - 1].ModuleReset();
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"PAPick(): PA Pick Error {errorCode}.";
                LoggerManager.Error($"PAPick(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum PAPut(IARMModule arm, IPreAlignModule pa)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                string sOrgin = arm.Holder.TransferObject.OriginHolder.Label;
                bool doPA = true;
                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                WaferNotchTypeEnum notchType = WaferNotchTypeEnum.UNKNOWN;
                SubstrateSizeEnum waferSize = SubstrateSizeEnum.UNDEFINED;
                int pwIDReadResult = -1;
                // Set wafer size and notch type
                if (arm.Holder.TransferObject != null)
                {
                    if (arm.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                    {
                        waferSize = arm.Holder.TransferObject.PolishWaferInfo.Size.Value;
                        notchType = arm.Holder.TransferObject.PolishWaferInfo.NotchType.Value;
                        LoggerManager.Debug($"PAPut(): Polsih Wafer Size = {waferSize}, NotchType = {notchType}");
                    }
                    else
                    {
                        waferSize = arm.Holder.TransferObject.Size.Value;
                        notchType = arm.Holder.TransferObject.NotchType;
                    }
                    
                    if (waferSize == SubstrateSizeEnum.INCH6)
                    {
                        notchType = WaferNotchTypeEnum.FLAT;
                    }
                }
                else
                {
                    LoggerManager.Debug($"PAPutAync(): Transfer object is NULL.");
                    return EventCodeEnum.WAFER_SIZE_ERROR;
                }

                LoggerManager.Debug($"PAPut(): Transfer object waferSize: {waferSize}, notch type {notchType}.");
                errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].SetDeviceSize(waferSize, notchType);
                if (errorCode != EventCodeEnum.NONE)
                {
                    return errorCode;
                }

                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {

                    LoggerManager.Debug($"PAPut(): Pre-aligner init(Module Reset). start");
                    errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].ModuleReset();
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        return errorCode;
                    }
                    CDXOut.nPreAPos = (short)pa.ID.Index;
                    CDXOut.nArmIndex = (ushort)armIndex;

                    LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.START, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT);
                    LoggerManager.Debug($"PAPut(): Put wafer to {CDXOut.nPreAPos} Pre-aligner with {CDXOut.nArmIndex} Arm. Start.");

                    errorCode = SetRobotCommand(EnumRobotCommand.PA_PUT);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.ERROR, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT, errorCode.ToString());
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.PA_PUTED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.ERROR, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT, errorCode.ToString());
                        throw new ProberSystemException(errorCode);
                    }
                    //loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(0.0);

                    double dstNotchAngle = 0;
                    if (arm.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        arm.Holder.CurrentWaferInfo = arm.Holder.TransferObject;
                        arm.Holder.SetTransfered(pa);
                        this.loaderModule.BroadcastLoaderInfo();
                    }
                    LoggerManager.Debug($"PAPut(): Put wafer to {CDXOut.nPreAPos} Pre-aligner with {CDXOut.nArmIndex} Arm. Done.");
                    LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.DONE, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT);

                    if (pa.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        if (pa.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                        {
                            dstNotchAngle = pa.Holder.TransferObject.PolishWaferInfo.CurrentAngle.Value - 90;

                            LoggerManager.Debug($"PAPut(): Polsih Wafer Angle = {dstNotchAngle}, CurrentAngle = {pa.Holder.TransferObject.PolishWaferInfo.CurrentAngle.Value}");
                        }
                        else
                        {
                            dstNotchAngle = pa.Holder.TransferObject.NotchAngle.Value - 90;
                        }

                        dstNotchAngle = dstNotchAngle % 360;

                        if (dstNotchAngle < 0)
                        {
                            dstNotchAngle += 360;
                        }
                    }


                    ICognexProcessManager cognexProcessManager = loaderModule.Container.Resolve<ICognexProcessManager>();
                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.IDLE;

                    IOCRReadable OCR = loaderModule.ModuleManager.FindModule(ModuleTypeEnum.COGNEXOCR, pa.ID.Index) as IOCRReadable;

                    TransferObject transferObj = null;
                    if (pa.Holder.TransferObject != null)
                    {
                        transferObj = pa.Holder.TransferObject;
                    }
                    else if (arm.Holder.TransferObject != null)
                    {
                        transferObj = arm.Holder.TransferObject;
                    }
                    else
                    {
                        transferObj = pa.Holder.TransferObject;
                    }


                    if (transferObj != null)
                    {
                        LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.START, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PRE_ALIGN);

                        int slotNum = 0;
                        int foupNum = 0;
                        if (transferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            slotNum = transferObj.OriginHolder.Index % 25;
                            int offset = 0;
                            if (slotNum == 0)
                            {
                                slotNum = 25;
                                offset = -1;
                            }
                            foupNum = ((transferObj.OriginHolder.Index + offset) / 25) + 1;
                        }
                        else
                        {
                            slotNum = transferObj.OriginHolder.Index;
                            foupNum = 0;
                        }

                        ActiveLotInfo transferObjActiveLot = null;
                        if (transferObj.CST_HashCode != null)
                        {
                            //origin이 cassette인 웨이퍼
                            transferObjActiveLot = loaderModule.LoaderMaster.ActiveLotInfos.FirstOrDefault(w => w.CST_HashCode == transferObj.CST_HashCode);
                            if (transferObjActiveLot == null)
                            {
                                transferObjActiveLot = loaderModule.LoaderMaster.Prev_ActiveLotInfos.FirstOrDefault(w => w.CST_HashCode == transferObj.CST_HashCode);
                            }
                        }// //origin이 cassette가 아니라 insp이나 fixed에 있던 standardwafer인 경우 null로 반환.

                        if (transferObj.OCRReadState == OCRReadStateEnum.NONE)
                        {
                            if (transferObj.WaferType.Value == EnumWaferType.STANDARD ||
                                  transferObj.WaferType.Value == EnumWaferType.TCW)
                            {
                                switch (transferObj.OCRMode.Value)
                                {
                                    case OCRModeEnum.NONE:
                                        {
                                            doPA = true;// OCR Done 처리를 이 스레드에서 하므로 여기에서 PreAlign 을 해야한다. 
                                                        //임의의 값을 자동할당
                                            transferObj.SetOCRState($"", 399, OCRReadStateEnum.DONE);
                                            cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;
                                            transferObj.CleanPreAlignState(reason: "Ocr Mode is NONE");

                                            LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}, WaferID: {transferObj.OCR.Value}");

                                            break;
                                        }
                                    case OCRModeEnum.READ:
                                        {
                                            doPA = true;
                                            if (loaderModule.OCRConfig.Enable)
                                            {
                                                foreach (var config in loaderModule.OCRConfig.ConfigList)
                                                {
                                                    transferObj.OCRDevParam.ConfigList.Add(config);
                                                }
                                            }
                                            double OCRAngle = 0;
                                            LoaderParameters.SubchuckMotionParam subchuckMotionParam = OCR.GetSubchuckMotionParam(transferObj.Size.Value);
                                            if (subchuckMotionParam != null)
                                            {
                                                OCRAngle = transferObj.OCRAngle.Value + subchuckMotionParam.SubchuckAngle_Offset.Value;
                                                LoggerManager.Debug($"DoPreAlign[OCR Aync] Start AnglePosition:{ transferObj.OCRAngle.Value} + Angle Offset{subchuckMotionParam.SubchuckAngle_Offset.Value} ");

                                            }
                                            else 
                                            {
                                                OCRAngle = transferObj.OCRAngle.Value;
                                                LoggerManager.Debug($"DoPreAlign[OCR Aync] Start AnglePosition:{ transferObj.OCRAngle.Value}");

                                            }

                                            errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(OCRAngle, true);
                                            LoggerManager.Debug($"DoPreAlign[OCR Aync] Result:{errorCode.ToString()}");

                                            if (errorCode != EventCodeEnum.NONE)
                                            {
                                                loaderModule.PAManager.PAModules[pa.ID.Index - 1].UpdateState();

                                                LoggerManager.ActionLog(ModuleLogType.PREALIGN_TO_OCR, StateLogType.RETRY, $"PA Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");

                                                LoggerManager.Debug($"DoPreAlign[OCR Aync retry] Start AnglePosition:{OCRAngle}");
                                                errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(OCRAngle, true);
                                                LoggerManager.Debug($"DoPreAlign[OCR Aync retry] Result:{errorCode.ToString()}");


                                            }
                                            else // DoPreAlign 성공한 경우 
                                            {
                                                if (transferObj.NotchAngle.Value == transferObj.OCRAngle.Value)
                                                {
                                                    transferObj.SetPreAlignDone(pa.ID);
                                                }
                                                else
                                                {
                                                    transferObj.CleanPreAlignState(reason: $"Notch Angle({transferObj.NotchAngle.Value}) and Ocr Angle({transferObj.OCRAngle.Value}) is not same");
                                                }
                                            }

                                            if (errorCode == EventCodeEnum.NONE)
                                            {
                                                LoggerManager.Debug($"[PAPut()] FoupNumber{foupNum}, SlotNumber{slotNum}");

                                                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                                                if (subchuckMotionParam != null)
                                                {
                                                    if (subchuckMotionParam.SubchuckXCoord.Value == 0 && subchuckMotionParam.SubchuckYCoord.Value == 0)
                                                    {
                                                        retVal = EventCodeEnum.NONE;
                                                    }
                                                    else 
                                                    {
                                                        retVal = PAMove(pa, subchuckMotionParam.SubchuckXCoord.Value, subchuckMotionParam.SubchuckYCoord.Value, 0);
                                                    }
                                                }
                                                else
                                                {
                                                    retVal = EventCodeEnum.NONE;
                                                }

                                                if (retVal == EventCodeEnum.NONE)
                                                {
                                                    transferObj.SetOCRState("", 0, OCRReadStateEnum.READING);// inspection에서 나온 웨이퍼 =                                                 
                                                    retVal = cognexProcessManager.DoOCRStringCatch(pa.ID.Index - 1, false, transferObj.OCRDevParam, transferObjActiveLot);
                                                    if (retVal == EventCodeEnum.NONE)
                                                    {
                                                        var ocr = cognexProcessManager.Ocr[pa.ID.Index - 1];
                                                        var ocrScore = cognexProcessManager.OcrScore[pa.ID.Index - 1];

                                                        LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"ID: {ocr}, Score: {ocrScore}, OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");

                                                        LoggerManager.Debug($"[OCR Result Data] State={OCRReadStateEnum.DONE}, ID: { ocr} , Score:{ocrScore} , OCR Index:{pa.ID.Index} , Origin Location:{loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");
                                                        transferObj.SetOCRState(ocr, ocrScore, OCRReadStateEnum.DONE);
                                                        cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;
                                                        cognexProcessManager.Ocr[pa.ID.Index - 1] = "";
                                                        cognexProcessManager.OcrScore[pa.ID.Index - 1] = 0;

                                                    }
                                                    else
                                                    {
                                                        doPA = false;
                                                        transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);
                                                        cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;
                                                        LoggerManager.Debug($"DoPreAlign[OCR] All OCRConfig Failed.");

                                                        LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.ERROR, $"OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");

                                                        loaderModule.NotifyManager.Notify(EventCodeEnum.OCR_READ_FAIL, pa.ID.Index);
                                                        cognexProcessManager.SaveOCRImage(pa.ID.Index - 1);
                                                    }
                                                }
                                                else
                                                {
                                                    doPA = false;
                                                    //transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);// System Error로 간주해서 손으로 Recovery 해줘야함. PAAlignAbort = true로 바뀐 상태.
                                                    LoggerManager.Debug($"DoPreAlign[OCR] PAMove Failed. retVal:{retVal}");
                                                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.ABORT;
                                                }

                                            }
                                            else
                                            {
                                                doPA = false;
                                                cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.ABORT;
                                                //PreAlign Fail인 경우                                                  
                                                if (errorCode == EventCodeEnum.LOADER_FIND_NOTCH_FAIL ||
                                                    errorCode == EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR)
                                                {
                                                    this.NotifyManager().Notify(errorCode, pa.ID.Index);
                                                }
                                                else
                                                {
                                                    LoggerManager.Debug($"DoPreAlign[OCR] PreAlign Retry Failed. errorCode:{errorCode}");
                                                }
                                                LoggerManager.Debug($"DoPreAlign[OCR] PreAlign Retry Failed. Cannnot try Read OCR.");
                                                //transferObj.SetOCRState("", 0, OCRReadStateEnum.ABORT);// System Error로 간주해서 손으로 Recovery 해줘야함. PAAlignAbort = true로 바뀐 상태.

                                            }
                                            break;
                                        }
                                    case OCRModeEnum.MANUAL:
                                        {
                                            transferObj.CleanPreAlignState(reason: "Ocr Mode is MANUAL");
                                            WaferIDManualDialog.WaferIDManualInput.Show(this.GetLoaderContainer(), transferObj);
                                            if (transferObj.OCR.Value == null || transferObj.OCR.Value == "")
                                            {
                                                errorCode = EventCodeEnum.NODATA;
                                                transferObj.SetOCRState("", 0, OCRReadStateEnum.ABORT);// 이미 Manual Input에 실패했는데 다시 기회를 줄 이유 없음.                                                    
                                                cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;
                                                LoggerManager.Error($"WaferIDManualInput is Null Error");
                                            }
                                            else
                                            {
                                                transferObj.SetOCRState(transferObj.OCR.Value, 0, OCRReadStateEnum.DONE);
                                                cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;

                                                LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}, WaferID: {transferObj.OCR.Value}");
                                            }
                                            break;
                                        }
                                    case OCRModeEnum.DEBUGGING:
                                        {
                                            Thread.Sleep(1000);
                                            doPA = true;
                                            transferObj.CleanPreAlignState(reason: "Ocr Mode is DEBUGGING");
                                            transferObj.SetOCRState("DEBUGGING", 399, OCRReadStateEnum.DONE);
                                            cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;
                                            //cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR; // DoOCRStringCatch 을 기다리는 중이 아니므로 할필요 없음.
                                            break;
                                        }
                                    default:
                                        break;
                                }



                                if (transferObj.OCRReadState == OCRReadStateEnum.FAILED)
                                {
                                    try
                                    {
                                        CognexManualInput.Show(this.GetLoaderContainer(), pa.ID.Index - 1);
                                        if (cognexProcessManager.GetManualOCRState(pa.ID.Index - 1))
                                        {
                                            var ocr = cognexProcessManager.Ocr[pa.ID.Index - 1];
                                            var ocrScore = cognexProcessManager.OcrScore[pa.ID.Index - 1];

                                            pa.Holder.TransferObject.SetOCRState(ocr, ocrScore, OCRReadStateEnum.DONE);

                                            LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"ManualDone, ID: {ocr}, Score: {0}, OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(pa.Holder.TransferObject.OriginHolder)}");

                                            LoggerManager.Debug($"[OCR Result Data] State=Manual_DONE, ID:{ocr}, Score: {0}  OCR Index:{pa.ID.Index} , Origin Location:{loaderModule.SlotToFoupConvert(pa.Holder.TransferObject.OriginHolder)}");
                                        }
                                        else
                                        {
                                            this.NotifyManager().Notify(EventCodeEnum.OCR_READ_FAIL_MANUAL, pa.ID.Index);
                                            pa.Holder.TransferObject.SetOCRState("", 0, OCRReadStateEnum.ABORT);
                                        }
                                    }
                                    catch (Exception err)
                                    {
                                        LoggerManager.Exception(err);
                                    }
                                    finally
                                    {
                                        doPA = true;
                                        loaderModule.LoaderMaster.OcrReadStateRisingEvent(transferObj, pa.ID);
                                    }

                                }
                                else if (transferObj.IsOCRDone())
                                {
                                    doPA = true;
                                    loaderModule.LoaderMaster.OcrReadStateRisingEvent(transferObj, pa.ID);
                                }
                                else
                                {
                                    doPA = true;
                                    LoggerManager.Debug($"Unexpected State.");
                                    //혹시 모를 예외처리
                                }

                            }
                            else if (transferObj.WaferType.Value == EnumWaferType.POLISH)
                            {
                                switch (transferObj.OCRMode.Value)
                                {
                                    case OCRModeEnum.READ:
                                        {
                                            // wafer change rcmd ocrread = enable trigger가 된 경우 READ
                                            doPA = true;

                                            LoggerManager.Debug($"DoPreAlign[OCR] Start AnglePosition:{ transferObj.OCRAngle.Value}");
                                            errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(transferObj.OCRAngle.Value, true);
                                            LoggerManager.Debug($"DoPreAlign[OCR] Result:{errorCode.ToString()}");

                                            if (errorCode != EventCodeEnum.NONE)
                                            {
                                                loaderModule.PAManager.PAModules[pa.ID.Index - 1].UpdateState();
                                                LoggerManager.ActionLog(ModuleLogType.PREALIGN_TO_OCR, StateLogType.RETRY, $"PA Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");
                                                LoggerManager.Debug($"DoPreAlign[OCR retry] Start AnglePosition:{ transferObj.OCRAngle.Value}");
                                                errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(transferObj.OCRAngle.Value, true);
                                                LoggerManager.Debug($"DoPreAlign[OCR retry] Result:{errorCode.ToString()}");
                                            }
                                            else // DoPreAlign 성공한 경우 
                                            {
                                                if (transferObj.NotchAngle.Value == transferObj.OCRAngle.Value)
                                                {
                                                    transferObj.SetPreAlignDone(pa.ID);
                                                }
                                                else
                                                {
                                                    transferObj.CleanPreAlignState(reason: $"Notch Angle({transferObj.NotchAngle.Value}) and Ocr Angle({transferObj.OCRAngle.Value}) is not same");
                                                }
                                            }

                                            if (errorCode == EventCodeEnum.NONE)
                                            {
                                                LoggerManager.Debug($"[PAPut()] FoupNumber{foupNum}, SlotNumber{slotNum}");

                                                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                                                if (transferObj.Size.Value == SubstrateSizeEnum.INCH6)
                                                {
                                                    retVal = PAMove(pa, 0, 30000);

                                                }
                                                else if (transferObj.Size.Value == SubstrateSizeEnum.INCH8)
                                                {
                                                    retVal = PAMove(pa, 0, -20000);
                                                }
                                                else
                                                {
                                                    retVal = EventCodeEnum.NONE;
                                                }

                                                if (retVal == EventCodeEnum.NONE)
                                                {
                                                    //rcmd로 부터 받은 ocr 값으로 우선 set 한다.
                                                    transferObj.SetOCRState(transferObj.OCR.Value, 0, OCRReadStateEnum.READING);
                                                    if(transferObj.PolishWaferInfo != null)
                                                    {
                                                        LoggerManager.Debug($"OCR Config DefineName = {transferObj.PolishWaferInfo.DefineName}");
                                                    }
                                                    retVal = cognexProcessManager.DoOCRStringCatch(pa.ID.Index - 1, false, transferObj.OCRDevParam, transferObjActiveLot);
                                                    if (retVal == EventCodeEnum.NONE)
                                                    {
                                                        //OCR 성공 !!
                                                        var ocr = cognexProcessManager.Ocr[pa.ID.Index - 1];
                                                        var ocrScore = cognexProcessManager.OcrScore[pa.ID.Index - 1];

                                                        if (transferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                                                        {
                                                            //OCR Value "" 인 경우, HOST에서 받은 값이 없다고 간주,
                                                            if (transferObj.OCR.Value != ocr && transferObj.OCR.Value != "")
                                                            {
                                                                //ocr = HOST 에서 받은 값
                                                                ocr = transferObj.OCR.Value;

                                                                //ocr result 2
                                                                pwIDReadResult = 2;
                                                            }
                                                            else
                                                            {
                                                                //ocr = cognex 결과 값
                                                                //ocr result 0 (성공)
                                                                pwIDReadResult = 0;
                                                            }
                                                        }
                                                        else if (transferObj.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY)
                                                        {
                                                            //ocr = cognex 결과 값
                                                            //ocr result 0 (성공)
                                                            pwIDReadResult = 0;
                                                        }

                                                        transferObj.SetOCRState(ocr, ocrScore, OCRReadStateEnum.DONE);
                                                        cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;
                                                        cognexProcessManager.Ocr[pa.ID.Index - 1] = "";
                                                        cognexProcessManager.OcrScore[pa.ID.Index - 1] = 0;

                                                        LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"ID: {ocr}, Score: {ocrScore}, OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");
                                                        LoggerManager.Debug($"[OCR Result Data] State={OCRReadStateEnum.DONE}, ID: { ocr} , Score:{ocrScore} , OCR Index:{pa.ID.Index} , Origin Location:{loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");

                                                    }
                                                    else
                                                    {
                                                        //OCR 실패 !!
                                                        doPA = false;
                                                        string ocr = "";
                                                        OCRReadStateEnum ocr_result = OCRReadStateEnum.NONE;
                                                        EnumCognexModuleState cognex_state = EnumCognexModuleState.IDLE;
                                                        if (transferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                                                        {
                                                            //OCR Value "" 인 경우, HOST에서 받은 값이 없다고 간주
                                                            if (transferObj.OCR.Value != "")
                                                            {
                                                                //ocr = HOST 에서 받은 값
                                                                ocr = transferObj.OCR.Value;
                                                                //ocr result 2 (manual 로 사용 하였음)
                                                                pwIDReadResult = 2;
                                                                ocr_result = OCRReadStateEnum.DONE;
                                                                cognex_state = EnumCognexModuleState.READ_OCR;
                                                            }
                                                            else
                                                            {
                                                                ocr = "EMPTY";
                                                                //ocr result 1 (실패)
                                                                pwIDReadResult = 1;
                                                                ocr_result = OCRReadStateEnum.FAILED;
                                                                cognex_state = EnumCognexModuleState.FAIL;
                                                            }
                                                        }
                                                        else if (transferObj.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY)
                                                        {
                                                            if (transferObj.OCR.Value != "")
                                                            {
                                                                //ocr 이미 가지고 있던 값
                                                                ocr = transferObj.OCR.Value;
                                                                //ocr result 2 (manual)
                                                                pwIDReadResult = 2;
                                                                ocr_result = OCRReadStateEnum.DONE;
                                                                cognex_state = EnumCognexModuleState.READ_OCR;
                                                            }
                                                            else
                                                            {
                                                                ocr = "EMPTY";
                                                                //ocr result 1 (실패)
                                                                pwIDReadResult = 1;
                                                                ocr_result = OCRReadStateEnum.FAILED;
                                                                cognex_state = EnumCognexModuleState.FAIL;
                                                            }
                                                        }

                                                        //MANUAL INPUT창을 안띄우는 것이니 ABORT로 한다.
                                                        transferObj.SetOCRState(ocr, 0, ocr_result);
                                                        cognexProcessManager.HostRunning[pa.ID.Index - 1] = cognex_state;
                                                        LoggerManager.Debug($"DoPreAlign[OCR] All OCRConfig Failed.");
                                                        loaderModule.NotifyManager.Notify(EventCodeEnum.OCR_READ_FAIL, pa.ID.Index);
                                                        cognexProcessManager.SaveOCRImage(pa.ID.Index - 1);
                                                    }
                                                }
                                                else
                                                {
                                                    doPA = false;
                                                    //transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);// System Error로 간주해서 손으로 Recovery 해줘야함. PAAlignAbort = true로 바뀐 상태.
                                                    LoggerManager.Debug($"DoPreAlign[OCR] PAMove Failed. retVal:{retVal}");
                                                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.ABORT;
                                                }

                                            }
                                            else
                                            {
                                                doPA = false;
                                                cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.ABORT;
                                                //PreAlign Fail인 경우                                                  
                                                if (errorCode == EventCodeEnum.LOADER_FIND_NOTCH_FAIL ||
                                                    errorCode == EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR)
                                                {
                                                    this.NotifyManager().Notify(errorCode, pa.ID.Index);
                                                }
                                                else
                                                {
                                                    LoggerManager.Debug($"DoPreAlign[OCR] PreAlign Retry Failed. errorCode:{errorCode}");
                                                }
                                                LoggerManager.Debug($"DoPreAlign[OCR] PreAlign Retry Failed. Cannnot try Read OCR.");
                                                //transferObj.SetOCRState("", 0, OCRReadStateEnum.ABORT);// System Error로 간주해서 손으로 Recovery 해줘야함. PAAlignAbort = true로 바뀐 상태.

                                            }

                                            LoggerManager.Debug($"PAPut() : OCRMode = {transferObj.OCRMode.Value}, pwIDReadResult = {pwIDReadResult}");

                                            break;
                                        }
                                    default:
                                        doPA = true;
                                        if (transferObj.OCR.Value != "")
                                        {
                                            pwIDReadResult = 2;
                                            //Host에서 받은 값이 있는 경우,
                                            transferObj.SetOCRState(transferObj.OCR.Value, 399, OCRReadStateEnum.DONE);
                                        }
                                        else
                                        {
                                            pwIDReadResult = 3; //ocr을 읽지 않는 mode이기 때문에 1(실패) 대신 3으로 Set.
                                            transferObj.SetOCRState("EMPTY", 399, OCRReadStateEnum.DONE);
                                        }
                                        LoggerManager.Debug($"PAPut() : POLISH Wafer OCRMode = {transferObj.OCRMode.Value}, OCR Result : {transferObj.OCR.Value}");
                                        break;
                                }

                                if (transferObj.OCRReadState == OCRReadStateEnum.FAILED)
                                {
                                    if (loaderModule.LoaderMaster.HostInitiatedWaferChangeInProgress == true)
                                    {
                                        //manual input 창 띄우지 않고 Abort
                                        this.NotifyManager().Notify(EventCodeEnum.OCR_READ_FAIL_MANUAL, pa.ID.Index);
                                        pa.Holder.TransferObject.SetOCRState("EMPTY", 0, OCRReadStateEnum.ABORT);

                                    }
                                    else
                                    {
                                        try
                                        {
                                            CognexManualInput.Show(this.GetLoaderContainer(), pa.ID.Index - 1);
                                            if (cognexProcessManager.GetManualOCRState(pa.ID.Index - 1))
                                            {
                                                pwIDReadResult = 2;
                                                var ocr = cognexProcessManager.Ocr[pa.ID.Index - 1];
                                                var ocrScore = cognexProcessManager.OcrScore[pa.ID.Index - 1];

                                                pa.Holder.TransferObject.SetOCRState(ocr, ocrScore, OCRReadStateEnum.DONE);

                                                LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"ManualDone, ID: {ocr}, Score: {0}, OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(pa.Holder.TransferObject.OriginHolder)}");

                                                LoggerManager.Debug($"[OCR Result Data] State=Manual_DONE, ID:{ocr}, Score: {0}  OCR Index:{pa.ID.Index} , Origin Location:{loaderModule.SlotToFoupConvert(pa.Holder.TransferObject.OriginHolder)}");
                                            }
                                            else
                                            {
                                                this.NotifyManager().Notify(EventCodeEnum.OCR_READ_FAIL_MANUAL, pa.ID.Index);
                                                pa.Holder.TransferObject.SetOCRState("EMPTY", 0, OCRReadStateEnum.ABORT);
                                            }
                                        }
                                        catch (Exception err)
                                        {
                                            this.NotifyManager().Notify(EventCodeEnum.OCR_READ_FAIL_MANUAL, pa.ID.Index);
                                            pa.Holder.TransferObject.SetOCRState("EMPTY", 0, OCRReadStateEnum.ABORT);
                                            LoggerManager.Exception(err);
                                        }
                                    }
                                }

                                doPA = true;
                                loaderModule.LoaderMaster.OcrReadStateRisingEvent(transferObj, pa.ID, pwIDReadResult);
                            }
                            else//CARD, INVALID, UNDEFINED
                            {
                                doPA = true;
                                LoggerManager.Debug($"PAPut(): OCRReadStateEnum = {transferObj.OCRReadState.ToString()}");
                                if (transferObj.IsOCRDone() == false)
                                {
                                    LoggerManager.Debug("OCR TransferObejct is NULL");
                                    transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);//기존 코드 동일하게 
                                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;//기존 코드 동일하게 
                                }
                            }
                        }

                        if (doPA)
                        {
                            LoggerManager.Debug($"DoPreAlign Start NotchAngle:{dstNotchAngle}");
                            errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(dstNotchAngle);
                            if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                            {
                                errorCode = EventCodeEnum.NONE;
                            }


                            if (errorCode != EventCodeEnum.NONE)// PAAlignAbort = true로 바뀐 상태.
                            {
                                bool notchTypeRetry = false;
                                if (notchType == WaferNotchTypeEnum.NOTCH)
                                {
                                    notchType = WaferNotchTypeEnum.FLAT;
                                    notchTypeRetry = true;
                                }
                                else if (notchType == WaferNotchTypeEnum.FLAT)
                                {
                                    notchType = WaferNotchTypeEnum.NOTCH;
                                    notchTypeRetry = true;
                                }

                                if (notchTypeRetry)
                                {
                                    errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].SetDeviceSize(waferSize, notchType);
                                    if (errorCode != EventCodeEnum.NONE)
                                    {
                                        return errorCode;
                                    }

                                    errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(dstNotchAngle);
                                    if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                                    {
                                        errorCode = EventCodeEnum.NONE;
                                    }

                                }

                                cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.ABORT;//다음 ProcModule의 WaitForOCR 중단.

                                if (errorCode == EventCodeEnum.LOADER_FIND_NOTCH_FAIL ||
                                   errorCode == EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR)
                                {
                                    this.NotifyManager().Notify(errorCode, pa.ID.Index);
                                }

                            }
                            else
                            {
                                //PreAlign 정상.
                            }
                        }

                        if (errorCode == EventCodeEnum.NONE)
                        {
                            LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.DONE, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PRE_ALIGN);
                        }
                        else
                        {
                            LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.ERROR, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PRE_ALIGN, errorCode.ToString());
                        }
                    }
                    else
                    {
                        errorCode = EventCodeEnum.LOADER_PA_WAF_MISSED;
                        LoggerManager.Debug($"PAPut(): TransferObject Cannot find. Arm:{arm.Holder.Status}, PA:{pa.Holder.Status}");
                    }

                }
                else
                {
                   
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"PAPut(): PA Put Error {errorCode}.";
                LoggerManager.Error($"PAPut(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum PAPick_NotVac(IPreAlignModule pa, IARMModule arm)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    CDXOut.nPreAPos = (short)pa.ID.Index;
                    CDXOut.nArmIndex = (ushort)armIndex;
                    LoggerManager.Debug($"PAPick(): Pick wafer from {CDXOut.nPreAPos} Pre-aligner with {CDXOut.nArmIndex} Arm");

                    errorCode = SetRobotCommand(EnumRobotCommand.PA_PICK);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.PA_PICKED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"PAPick_NotVac(): PA Pick Error {errorCode}.";
                LoggerManager.Error($"PAPick_NotVac(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum PAPut_NotVac(IARMModule arm, IPreAlignModule pa)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    // loaderModule.PAManager.PAModules[pa.ID.Index - 1].ModuleReset();
                    CDXOut.nPreAPos = (short)pa.ID.Index;
                    CDXOut.nArmIndex = (ushort)armIndex;
                    LoggerManager.Debug($"PAPut(): Put wafer to {CDXOut.nPreAPos} Pre-aligner with {CDXOut.nArmIndex} Arm");

                    errorCode = SetRobotCommand(EnumRobotCommand.PA_PUT);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.PA_PUTED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    //loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(0.0);
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"PAPut_NotVac(): PA Put Error {errorCode}.";
                LoggerManager.Error($"PAPut_NotVac(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum PAPut_NotOCR(IARMModule arm, IPreAlignModule pa)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].ModuleReset();
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        return errorCode;
                    }
                    CDXOut.nPreAPos = (short)pa.ID.Index;
                    CDXOut.nArmIndex = (ushort)armIndex;
                    LoggerManager.Debug($"PAPut(): Put wafer to {CDXOut.nPreAPos} Pre-aligner with {CDXOut.nArmIndex} Arm");

                    errorCode = SetRobotCommand(EnumRobotCommand.PA_PUT);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.PA_PUTED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    //loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(0.0);

                    double dstNotchAngle = 0;

                    if (pa.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        if (pa.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                        {
                            dstNotchAngle = pa.Holder.TransferObject.PolishWaferInfo.CurrentAngle.Value - 90;

                            LoggerManager.Debug($"[{this.GetType().Name}], PAPut_NotOCR(): Polsih Wafer Angle = {dstNotchAngle}");
                        }
                        else
                        {
                            dstNotchAngle = pa.Holder.TransferObject.NotchAngle.Value - 90;
                        }

                        dstNotchAngle = dstNotchAngle % 360;

                        if (dstNotchAngle < 0)
                        {
                            dstNotchAngle += 360;
                        }
                    }

                    errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(dstNotchAngle);

                    if (errorCode == EventCodeEnum.LOADER_FIND_NOTCH_FAIL)
                    {
                        this.NotifyManager().Notify(EventCodeEnum.LOADER_FIND_NOTCH_FAIL, pa.ID.Index);
                    }
                    else if (errorCode == EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR)
                    {
                        this.NotifyManager().Notify(EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR, pa.ID.Index);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"PAPut_NotOCR(): PA Put Error {errorCode}.";
                LoggerManager.Error($"PAPut_NotOCR(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }


        public EventCodeEnum PAForcedPick(IPreAlignModule pa, IARMModule arm)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    loaderModule.PAManager.PAModules[pa.ID.Index - 1].WaitForPA(5000);
                    errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].ReleaseSubstrate();
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"PAPick(): PA#{pa.ID.Index} Release error. ErrorCode = {errorCode}");
                        return errorCode;
                    }
                    CDXOut.nPreAPos = (short)pa.ID.Index;
                    CDXOut.nArmIndex = (ushort)armIndex;
                    LoggerManager.Debug($"PAPick(): Pick wafer from {CDXOut.nPreAPos} Pre-aligner with {CDXOut.nArmIndex} Arm");

                    errorCode = SetRobotCommand(EnumRobotCommand.PA_PICK);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.PA_PICKED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].ModuleReset();
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"PAForcedPick(): PA Forced Pick Error {errorCode}.";
                LoggerManager.Error($"PAForcedPick(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }

        public EventCodeEnum PAPutAync(IARMModule arm, IPreAlignModule pa)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                string sOrgin = arm.Holder.TransferObject.OriginHolder.Label;
                bool doPA = true;
                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);

                WaferNotchTypeEnum notchType = WaferNotchTypeEnum.UNKNOWN;
                SubstrateSizeEnum waferSize = SubstrateSizeEnum.UNDEFINED;
                // Set wafer size and notch type
                if (arm.Holder.TransferObject != null)
                {
                    if (arm.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                    {
                        waferSize = arm.Holder.TransferObject.PolishWaferInfo.Size.Value;
                        notchType = arm.Holder.TransferObject.PolishWaferInfo.NotchType.Value;
                        LoggerManager.Debug($"PAPutAync(): Polsih Wafer Size = {waferSize}");
                    }
                    else
                    {
                        waferSize = arm.Holder.TransferObject.Size.Value;
                        notchType = arm.Holder.TransferObject.NotchType;
                    }

                    if (waferSize == SubstrateSizeEnum.INCH6)
                    {
                        notchType = WaferNotchTypeEnum.FLAT;
                    }
                }
                else
                {
                    LoggerManager.Debug($"PAPutAync(): Transfer object is NULL.");
                    return EventCodeEnum.WAFER_SIZE_ERROR;
                }

                LoggerManager.Debug($"PAPutAync(): Transfer object waferSize: {waferSize}, notch type {notchType}.");
                errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].SetDeviceSize(waferSize, notchType);
                if (errorCode != EventCodeEnum.NONE)
                {
                    return errorCode;
                }

                if (IsControllerInIdle() == true)
                {
                    errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].ModuleReset();
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        return errorCode;
                    }

                    CDXOut.nPreAPos = (short)pa.ID.Index;
                    CDXOut.nArmIndex = (ushort)armIndex;

                    LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.START, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT);
                    LoggerManager.Debug($"PAPutAync(): Put wafer to {CDXOut.nPreAPos} Pre-aligner with {CDXOut.nArmIndex} Arm. Start.");
                    errorCode = SetRobotCommand(EnumRobotCommand.PA_PUT);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.ERROR, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT, errorCode.ToString());
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.PA_PUTED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.ERROR, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT, errorCode.ToString());
                        throw new ProberSystemException(errorCode);
                    }
                    if (arm.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        arm.Holder.CurrentWaferInfo = arm.Holder.TransferObject;
                        arm.Holder.SetTransfered(pa);
                        this.loaderModule.BroadcastLoaderInfo();
                    }
                    LoggerManager.Debug($"PAPutAync(): Put wafer to {CDXOut.nPreAPos} Pre-aligner with {CDXOut.nArmIndex} Arm. Done.");
                    LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.DONE, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT);

                    double dstNotchAngle = 0;
                    if (pa.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        if (pa.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                        {
                            dstNotchAngle = pa.Holder.TransferObject.PolishWaferInfo.CurrentAngle.Value - 90;

                            LoggerManager.Debug($"[{this.GetType().Name}], PAPutAsync(): Polsih Wafer Angle = {dstNotchAngle}");
                        }
                        else
                        {
                            dstNotchAngle = pa.Holder.TransferObject.NotchAngle.Value - 90;
                            LoggerManager.Debug($"PAPutAsync(): Wafer Angle = {dstNotchAngle}");
                        }

                        dstNotchAngle = dstNotchAngle % 360;

                        if (dstNotchAngle < 0)
                        {
                            dstNotchAngle += 360;
                        }
                    }
                    ICognexProcessManager cognexProcessManager = loaderModule.Container.Resolve<ICognexProcessManager>();
                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.IDLE;

                    IOCRReadable OCR = loaderModule.ModuleManager.FindModule(ModuleTypeEnum.COGNEXOCR, pa.ID.Index) as IOCRReadable;

                    TransferObject transferObj = null;
                    if (pa.Holder.TransferObject != null)
                    {
                        transferObj = pa.Holder.TransferObject;
                    }
                    else if (arm.Holder.TransferObject != null)
                    {
                        transferObj = arm.Holder.TransferObject;
                    }
                    else
                    {
                        transferObj = pa.Holder.TransferObject;
                    }

                    if (transferObj == null)
                    {
                        System.Threading.Thread.Sleep(500);

                        if (pa.Holder.TransferObject != null)
                        {
                            transferObj = pa.Holder.TransferObject;
                        }
                        else if (arm.Holder.TransferObject != null)
                        {
                            transferObj = arm.Holder.TransferObject;
                        }
                        else
                        {
                            transferObj = pa.Holder.TransferObject;
                        }
                    }

                    if (transferObj == null)
                    {
                        System.Threading.Thread.Sleep(1000);
                        if (pa.Holder.TransferObject != null) //transfer Object가 널인 경우 다시한번 체크 해준다.
                        {
                            transferObj = pa.Holder.TransferObject;
                        }
                        else if (arm.Holder.TransferObject != null)
                        {
                            transferObj = arm.Holder.TransferObject;
                        }
                        else
                        {
                            transferObj = pa.Holder.TransferObject;
                        }
                    }

                    if (transferObj != null)
                    {
                        int slotNum = 0;
                        int foupNum = 0;
                        if (transferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            slotNum = transferObj.OriginHolder.Index % 25;
                            int offset = 0;
                            if (slotNum == 0)
                            {
                                slotNum = 25;
                                offset = -1;
                            }
                            foupNum = ((transferObj.OriginHolder.Index + offset) / 25) + 1;
                        }
                        else
                        {
                            slotNum = transferObj.OriginHolder.Index;
                            foupNum = 0;
                        }

                        ActiveLotInfo transferObjActiveLot = null;
                        if (transferObj.CST_HashCode != null)
                        {
                            //origin이 cassette인 웨이퍼
                            transferObjActiveLot = loaderModule.LoaderMaster.ActiveLotInfos.FirstOrDefault(w => w.CST_HashCode == transferObj.CST_HashCode);
                            if (transferObjActiveLot == null)
                            {
                                transferObjActiveLot = loaderModule.LoaderMaster.Prev_ActiveLotInfos.FirstOrDefault(w => w.CST_HashCode == transferObj.CST_HashCode);
                            }
                        }// //origin이 cassette가 아니라 insp이나 fixed에 있던 standardwafer인 경우 null로 반환.

                        if (transferObj.OCRReadState == OCRReadStateEnum.NONE)
                        {                          
                            if (transferObj.WaferType.Value == EnumWaferType.STANDARD ||
                                  transferObj.WaferType.Value == EnumWaferType.TCW)
                            {
                                switch (transferObj.OCRMode.Value)
                                {
                                    case OCRModeEnum.NONE:
                                        {
                                            doPA = true;// OCR Done 처리를 이 스레드에서 하므로 여기에서 PreAlign 을 해야한다. 
                                            //임의의 값을 자동할당
                                            transferObj.SetOCRState($"", 399, OCRReadStateEnum.DONE);
                                            cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;
                                            transferObj.CleanPreAlignState(reason: "Ocr Mode is NONE");

                                            LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}, WaferID: {transferObj.OCR.Value}");
                                            break;
                                        }
                                    case OCRModeEnum.READ:
                                        {
                                            break;
                                        }
                                    case OCRModeEnum.MANUAL:
                                        {
                                            break;
                                        }
                                    case OCRModeEnum.DEBUGGING:
                                        {
                                            Thread.Sleep(1000);
                                            doPA = true;
                                            transferObj.CleanPreAlignState(reason: "Ocr Mode is DEBUGGING");
                                            transferObj.SetOCRState("DEBUGGING", 399, OCRReadStateEnum.DONE);
                                            cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;
                                            //cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR; // DoOCRStringCatch 을 기다리는 중이 아니므로 할필요 없음.
                                            break;
                                        }
                                    default:
                                        break;
                                }

                                if(transferObj.OCRReadState == OCRReadStateEnum.FAILED)
                                {
                                    doPA = false;
                                }
                                else if (transferObj.OCRMode.Value == OCRModeEnum.MANUAL || transferObj.OCRMode.Value == OCRModeEnum.READ)
                                {
                                    doPA = true;
                                    //Wait가 필요한 모드, 다음 스레드에서 처리.
                                }
                                else if (transferObj.OCRMode.Value == OCRModeEnum.DEBUGGING || transferObj.OCRMode.Value == OCRModeEnum.NONE)
                                {
                                    doPA = true;
                                    //Wait가 필요하지 않은 모드
                                    loaderModule.LoaderMaster.OcrReadStateRisingEvent(transferObj, pa.ID);
                                }
                                else
                                {
                                    doPA = false;
                                    LoggerManager.Debug($"Unexpected State.");
                                    //혹시 모를 예외처리
                                }

                            }
                            else if (transferObj.WaferType.Value == EnumWaferType.POLISH)
                            {
                                if (transferObj.OCR.Value != "")
                                {
                                    transferObj.SetOCRState(transferObj.OCR.Value, 399, OCRReadStateEnum.DONE);
                                }
                                else
                                {
                                    transferObj.SetOCRState("EMPTY", 399, OCRReadStateEnum.DONE);//TODO: Polish 의 OCR을시도하지 않더라도 ID로 구별해주는게 좋지 않을까? 
                                }
                                doPA = true;

                                if (pa.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                                {
                                    dstNotchAngle = pa.Holder.TransferObject.PolishWaferInfo.CurrentAngle.Value - 90;

                                    LoggerManager.Debug($"[{this.GetType().Name}], PAPut_NotOCR(): Polsih Wafer Angle = {dstNotchAngle}");
                                }
                                else
                                {
                                    dstNotchAngle = pa.Holder.TransferObject.NotchAngle.Value - 90;
                                }

                                dstNotchAngle = dstNotchAngle % 360;

                                if (dstNotchAngle < 0)
                                {
                                    dstNotchAngle += 360;
                                }
                            }
                            else//CARD, INVALID, UNDEFINED
                            {
                                doPA = true;
                                LoggerManager.Debug($"PAPutAsync(): OCRReadStateEnum = {transferObj.OCRReadState.ToString()}");
                                if (transferObj.IsOCRDone() == false)
                                {
                                    LoggerManager.Debug("OCR TransferObejct is NULL");
                                    transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);//기존 코드 동일하게 
                                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;//기존 코드 동일하게                                   
                                }
                            }
                        }


                        Task.Run(() =>
                        {
                            LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.START, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PRE_ALIGN);

                            //OCR Mode에 따라서 OCR Configs에 있는 모든 Config OCR 시도 
                            if (transferObj.OCRReadState == OCRReadStateEnum.NONE)
                            {
                                if (transferObj.WaferType.Value == EnumWaferType.STANDARD || transferObj.WaferType.Value == EnumWaferType.TCW)
                                {
                                    switch (transferObj.OCRMode.Value)
                                    {
                                        case OCRModeEnum.NONE:
                                            {
                                                break;
                                            }
                                        case OCRModeEnum.READ:
                                            {
                                                if (loaderModule.OCRConfig.Enable)
                                                {
                                                    foreach (var config in loaderModule.OCRConfig.ConfigList)
                                                    {
                                                        transferObj.OCRDevParam.ConfigList.Add(config);
                                                    }
                                                }
                                                double OCRAngle = 0;
                                                LoaderParameters.SubchuckMotionParam subchuckMotionParam = OCR.GetSubchuckMotionParam(transferObj.Size.Value);
                                                if (subchuckMotionParam != null)
                                                {
                                                    OCRAngle = transferObj.OCRAngle.Value + subchuckMotionParam.SubchuckAngle_Offset.Value;
                                                    LoggerManager.Debug($"DoPreAlign[OCR Aync] Start AnglePosition:{ transferObj.OCRAngle.Value} + Angle Offset{subchuckMotionParam.SubchuckAngle_Offset.Value} ");
                                                }
                                                else
                                                {
                                                    OCRAngle = transferObj.OCRAngle.Value;
                                                    LoggerManager.Debug($"DoPreAlign[OCR Aync] Start AnglePosition:{ transferObj.OCRAngle.Value}");
                                                }

                                                errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(OCRAngle, true);
                                                LoggerManager.Debug($"DoPreAlign[OCR Aync] Result:{errorCode.ToString()}");

                                                if (errorCode != EventCodeEnum.NONE)
                                                {                                                    
                                                    loaderModule.PAManager.PAModules[pa.ID.Index - 1].UpdateState();
                                                    
                                                    LoggerManager.ActionLog(ModuleLogType.PREALIGN_TO_OCR, StateLogType.RETRY, $"PA Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");

                                                    LoggerManager.Debug($"DoPreAlign[OCR Aync retry] Start AnglePosition:{OCRAngle}");
                                                    errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(OCRAngle, true);
                                                    LoggerManager.Debug($"DoPreAlign[OCR Aync retry] Result:{errorCode.ToString()}");                                                    


                                                }
                                                else // DoPreAlign 성공한 경우 
                                                {
                                                    if (transferObj.NotchAngle.Value == transferObj.OCRAngle.Value)
                                                    {
                                                        transferObj.SetPreAlignDone(pa.ID);
                                                    }
                                                    else
                                                    {
                                                        transferObj.CleanPreAlignState(reason: $"Notch Angle({transferObj.NotchAngle.Value}) and Ocr Angle({transferObj.OCRAngle.Value}) is not same");
                                                    }
                                                }


                                                if (errorCode == EventCodeEnum.NONE)
                                                {
                                                    LoggerManager.Debug($"[PAPutAsync()] FoupNumber{foupNum}, SlotNumber{slotNum}");

                                                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                                                    if (subchuckMotionParam != null)
                                                    {
                                                        if (subchuckMotionParam.SubchuckXCoord.Value == 0 && subchuckMotionParam.SubchuckYCoord.Value == 0)
                                                        {
                                                            retVal = EventCodeEnum.NONE;
                                                        }
                                                        else
                                                        {
                                                            retVal = PAMove(pa, subchuckMotionParam.SubchuckXCoord.Value, subchuckMotionParam.SubchuckYCoord.Value, 0);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        retVal = EventCodeEnum.NONE;
                                                    }

                                                    if (retVal == EventCodeEnum.NONE)
                                                    {
                                                        transferObj.SetOCRState("", 0, OCRReadStateEnum.READING);
                                                        retVal = cognexProcessManager.DoOCRStringCatch(pa.ID.Index - 1, false, transferObj.OCRDevParam, transferObjActiveLot);
                                                        if (retVal == EventCodeEnum.NONE)
                                                        {
                                                            var ocr = cognexProcessManager.Ocr[pa.ID.Index - 1];
                                                            var ocrScore = cognexProcessManager.OcrScore[pa.ID.Index - 1];
                                                            
                                                            LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"ID: {ocr}, Score: {ocrScore}, OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");

                                                            LoggerManager.Debug($"[OCR Result Data] State={OCRReadStateEnum.DONE}, ID: { ocr} , Score:{ocrScore} , OCR Index:{pa.ID.Index} , Origin Location:{loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");
                                                            transferObj.SetOCRState(ocr, ocrScore, OCRReadStateEnum.DONE);
                                                            cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;
                                                            cognexProcessManager.Ocr[pa.ID.Index - 1] = "";
                                                            cognexProcessManager.OcrScore[pa.ID.Index - 1] = 0;

                                                        }
                                                        else
                                                        {
                                                            doPA = false;
                                                            transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);
                                                            cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;
                                                            LoggerManager.Debug($"DoPreAlign[OCR Aync] All OCRConfig Failed.");

                                                            LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.ERROR, $"OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");

                                                            loaderModule.NotifyManager.Notify(EventCodeEnum.OCR_READ_FAIL, pa.ID.Index);
                                                            cognexProcessManager.SaveOCRImage(pa.ID.Index - 1);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        doPA = false;
                                                        //transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);// System Error로 간주해서 손으로 Recovery 해줘야함. PAAlignAbort = true로 바뀐 상태.
                                                        LoggerManager.Debug($"DoPreAlign[OCR Aync] PAMove Failed. retVal:{retVal}");
                                                        cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.ABORT;
                                                    }



                                                }
                                                else
                                                {
                                                    doPA = false;
                                                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.ABORT;
                                                    //PreAlign Fail인 경우                                                  
                                                    if (errorCode == EventCodeEnum.LOADER_FIND_NOTCH_FAIL ||
                                                        errorCode == EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR)
                                                    {
                                                        this.NotifyManager().Notify(errorCode, pa.ID.Index);
                                                    }
                                                    else
                                                    {
                                                        LoggerManager.Debug($"DoPreAlign[OCR Aync] PreAlign Retry Failed. errorCode:{errorCode}");
                                                    }                                                    
                                                    LoggerManager.Debug($"DoPreAlign[OCR Aync] PreAlign Retry Failed. Cannnot try Read OCR.");
                                                    //transferObj.SetOCRState("", 0, OCRReadStateEnum.ABORT); // System Error로 간주해서 손으로 Recovery 해줘야함. PAAlignAbort = true로 바뀐 상태.                                                                                                      
                                                }
                                                break;
                                            }
                                        case OCRModeEnum.MANUAL:
                                            {
                                                transferObj.CleanPreAlignState(reason: "Ocr Mode is MANUAL");
                                                WaferIDManualDialog.WaferIDManualInput.Show(this.GetLoaderContainer(), transferObj);
                                                if (transferObj.OCR.Value == null || transferObj.OCR.Value == "")
                                                {
                                                    errorCode = EventCodeEnum.NODATA;
                                                    transferObj.SetOCRState("", 0, OCRReadStateEnum.ABORT);// 이미 Manual Input에 실패했는데 다시 기회를 줄 이유 없음.                                                    
                                                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;
                                                    LoggerManager.Error($"WaferIDManualInput is Null Error");
                                                }
                                                else
                                                {
                                                    transferObj.SetOCRState(transferObj.OCR.Value, 0, OCRReadStateEnum.DONE);
                                                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;

                                                    LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}, WaferID: {transferObj.OCR.Value}");
                                                }
                                                break;
                                            }
                                        case OCRModeEnum.DEBUGGING:
                                            {
                                                break;
                                            }
                                        default:
                                            break;
                                    }

                              
                                }
                                else
                                {
                                    //Task 이전에 다 처리함.
                                }
                            }
                            else
                            {
                                //이미 ocr을 완료한 상태
                            }

                            //if (transferObj.IsOCRDone() == false)//예외처리
                            //{
                            //    doPA = false;//TODO: 빼고 에러 나는 부분확인할것.
                            //    LoggerManager.Debug("OCR TransferObejct is NULL");
                            //    transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);
                            //    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;
                            //}


                            //OCR Fail 이나 PreAlign Error 가 아닌경우 무조건 PreAlign 한다. 
                            if (doPA)
                            {
                                LoggerManager.Debug($"DoPreAlign Start NotchAngle:{dstNotchAngle}");
                                errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(dstNotchAngle);
                                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                                {
                                    errorCode = EventCodeEnum.NONE;
                                }
                                if (errorCode == EventCodeEnum.NONE)
                                {
                                    //transferObj.SetPreAlignDone(pa.ID);<-- 이걸 여기서 하면 NobufferMode일때 GP_PreAlignState를 가지고 올수 없어서 다음 job이 진행이 안됨.
                                }
                                else if (errorCode == EventCodeEnum.LOADER_FIND_NOTCH_FAIL ||
                                    errorCode == EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR)
                                {
                                    this.NotifyManager().Notify(errorCode, pa.ID.Index);
                                }
                                else //errorCode != EventCodeEnum.NONE
                                {
                                    transferObj.CleanPreAlignState(reason: "PreAlign Failed.");
                                }
                            }

                            if (errorCode == EventCodeEnum.NONE)
                            {
                                LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.DONE, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PRE_ALIGN);
                            }
                            else
                            {
                                LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.ERROR, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PRE_ALIGN, errorCode.ToString());
                            }
                        });
                    }
                    else
                    {
                        errorCode = EventCodeEnum.LOADER_PA_WAF_MISSED;
                        LoggerManager.Debug($"PAPutAsync(): TransferObject is null Arm:{arm.Holder.Status}, PA:{pa.Holder.Status}");
                    }
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"PAPutAync(): PA Put Error {errorCode}.";
                LoggerManager.Error($"PAPutAync(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }

        public EventCodeEnum SetBufferedMove(double xpos, double zpos, double wpos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            #region // Buffered Move
            bool bufferedMove = true;
            stAccessParam accessParam = new stAccessParam();
            accessParam.nLX_Pos = (int)xpos;
            accessParam.nLZ_Pos = (int)zpos;
            accessParam.nLW_Pos = (int)wpos;
            try
            {
                if (bufferedMove == true)
                {
                    if(SymbolMap.OutputSymbol.BufferedMove != null && SymbolMap.OutputSymbol.BufferedMove.Handle > 0)
                    {
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.BufferedMove.Handle, true);
                    }

                    if(SymbolMap.OutputSymbol.BufferedXPos != null && SymbolMap.OutputSymbol.BufferedXPos.Handle > 0)
                    {
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.BufferedXPos.Handle, (Int32)accessParam.nLX_Pos);
                    }

                    if(SymbolMap.OutputSymbol.BufferedZPos != null && SymbolMap.OutputSymbol.BufferedZPos.Handle > 0)
                    {
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.BufferedZPos.Handle, (Int32)accessParam.nLZ_Pos);
                    }

                    if(SymbolMap.OutputSymbol.BufferedWPos != null && SymbolMap.OutputSymbol.BufferedWPos.Handle > 0 )
                    {
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.BufferedWPos.Handle, (Int32)accessParam.nLW_Pos);
                    }
                    LoggerManager.Debug($"SetBufferedMove(): Request buffered move. Target = X:{accessParam.nLX_Pos}, Y:{accessParam.nLZ_Pos}, W:{accessParam.nLW_Pos}");
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
            #endregion
        }

        private string ProcessOcrString(string ocr, TransferObject transferobj)
        {
            var proc_ocr = ocr.ToString();
            try
            {
                if (proc_ocr.Count() > 0)
                {
                    if (transferobj.OCRDevParam.ConfigList.FirstOrDefault().CheckSum != "0")// CheckSum Disable
                    {
                        if (loaderModule.OCRConfig.RemoveCheckSum)
                        {
                            proc_ocr = proc_ocr.Substring(0, ocr.Length - 2);
                        }
                    }
                   

                    if (loaderModule.OCRConfig.ReplaceDashToDot)
                    {
                        proc_ocr = proc_ocr.Replace("-", ".");
                    }

                    if (proc_ocr != ocr)
                    {
                        LoggerManager.Debug($"Changed proc_ocr:{ocr} => {proc_ocr}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return proc_ocr;
        }


        public EventCodeEnum BufferPick(IBufferModule Buffer, IARMModule arm)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    CDXOut.nLTSlotPos = (short)Buffer.ID.Index;
                    CDXOut.nArmIndex = (ushort)armIndex;
                    LoggerManager.Debug($"BufferPick(): Pick wafer from {CDXOut.nLTSlotPos} Buffer with {CDXOut.nArmIndex} Arm");



                    errorCode = SetRobotCommand(EnumRobotCommand.BUFF_PICK);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.BUFF_PICKED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"BufferPick(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum BufferPut(IARMModule arm, IBufferModule Buffer)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    CDXOut.nLTSlotPos = (short)Buffer.ID.Index;
                    CDXOut.nArmIndex = (ushort)armIndex;
                    LoggerManager.Debug($"BufferPut(): Put wafer to {CDXOut.nLTSlotPos} Buffer with {CDXOut.nArmIndex} Arm");
                    errorCode = SetRobotCommand(EnumRobotCommand.BUFF_PUT);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.BUFF_PUTED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }

                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"BufferPut(): Buffer Put Error {errorCode}.";
                LoggerManager.Error($"BufferPut(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }

        public EventCodeEnum RFIDReInitialize()
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                if (RFIDReader != null && RFIDReader.CommModule != null)
                {
                    RFIDReader.CommModule.DisConnect();
                }

                errorCode = InitRFIDModule_ForCardID();
                if (errorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[RFID] Init RFIDModule Failed. EventCodeEnum : {errorCode}");
                    return errorCode;
                }

                if (RFIDReader?.CommModule == null)
                {
                    LoggerManager.Debug($"[RFID] RFID ReInitialize Fail, CommModule is null.");
                    return errorCode;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"RFIDReInitialize(): Exception occurred. Err = {err.Message}");
            }
            return errorCode;
        }

        private EventCodeEnum InitRFIDModule_ForCardID()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            // CardID를 읽기 위한 RFIDModule 추가
            try
            {
                RFIDReader = new RFIDModule(EnumRFIDModuleType.PROBECARD);                
                retval = RFIDReader.LoadSysParameter();
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"RFIDModule LoadSysParameter() Failed. EventCodeEnum : {retval}");
                    return retval;
                }

                retval = RFIDReader.InitModule();
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"RFIDModule InitModule() Failed. EventCodeEnum : {retval}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EnumCommunicationState GetRFIDCommState_ForCardID()
        {
            EnumCommunicationState retVal = EnumCommunicationState.UNAVAILABLE;
            try
            {
                if (RFIDReader == null)
                {
                    InitRFIDModule_ForCardID();
                }
                if (SysParam.CardIDReaderType.Value == EnumCardIDReaderType.RFID)
                {
                    retVal = RFIDReader.CommModule.GetCommState();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private bool IsRFIDDataReady()
        {
            bool retVal = false;
            try
            {
                if (RFIDReader == null)
                {
                    EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
                    errorCode = InitRFIDModule_ForCardID();
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[RFID] InitRFIDModule_ForCardID Fail. errorCode : {errorCode}");
                        return false;
                    }
                }
                if (RFIDReader.CommModule.GetCommState() == EnumCommunicationState.DISCONNECT
                    || RFIDReader.CommModule.GetCommState() == EnumCommunicationState.UNAVAILABLE)
                {
                    LoggerManager.Debug($"[RFID] CardID RFID Read Fail, CommModule is disconnected.");
                    return false;
                }
                else if (RFIDReader?.CommModule?.GetCommState() == EnumCommunicationState.CONNECTED)
                {
                    LoggerManager.Debug($"[RFID] CardID RFID CommModule is connected.");
                    return true;
                }
                else if (RFIDReader?.CommModule?.GetCommState() == EnumCommunicationState.EMUL)
                {
                    LoggerManager.Debug($"[RFID] CardID RFID CommModule is emul.");
                    return true;
                }
            }
            catch(Exception err)
            {
                LoggerManager.Error($"IsRFIDDataReady(): Exception occurred. Err = {err.Message}");
                retVal = false;
            }
            return retVal;
        }

        public bool GetCardIDReadDataReady()
        {
            try
            {
                bool bRet = false;
                switch(SysParam.CardIDReaderType?.Value)
                {
                    case EnumCardIDReaderType.BARCODE:
                        bRet = BCDReader.DataReady;
                        break;
                    case EnumCardIDReaderType.RFID:
                        bRet = IsRFIDDataReady();
                        break;
                    case EnumCardIDReaderType.DATETIME:
                        bRet = true;
                        break;
                    case EnumCardIDReaderType.NONE:
                    default:
                        break;
                }

                return bRet;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetCardIDReadDataReady(): Exception occurred. Err = {err.Message}");
                throw err;
            }
        }

        public string GetReceivedCardID()
        {
            try
            {
                string RetCardID = "";
                switch (SysParam.CardIDReaderType.Value)
                {
                    case EnumCardIDReaderType.BARCODE:
                        RetCardID = BCDReader.ReceivedBCD;
                        break;
                    case EnumCardIDReaderType.RFID:
                        RetCardID = RFIDReader.RFID_cont_READID();
                        break;
                    case EnumCardIDReaderType.DATETIME:
                        RetCardID = DateTime.Now.ToString("yyyyMMddHHmmss");
                        break;
                    case EnumCardIDReaderType.NONE:
                    default:
                        break;
                }

                if (string.IsNullOrEmpty(RetCardID))
                {
                    //Fail read CardID
                    LoggerManager.Debug($"[{SysParam.CardIDReaderType.Value}] CardID Read Fail, Id is null.");
                }
                else
                {
                    //Success read CardID
                    LoggerManager.Debug($"[{SysParam.CardIDReaderType.Value}] CardID Read Success. (ID: {RetCardID})");
                }

                return RetCardID;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetReceivedCardID(): Exception occurred. Err = {err.Message}");
                throw err;
            }
        }

        private void ClearCardID()
        {
            try
            {
                switch (SysParam.CardIDReaderType.Value)
                {
                    case EnumCardIDReaderType.BARCODE:
                        BCDReader.Clear();
                        break;
                    case EnumCardIDReaderType.RFID:
                    case EnumCardIDReaderType.DATETIME:
                    case EnumCardIDReaderType.NONE:
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetReceivedCardID(): Exception occurred. Err = {err.Message}");
                throw err;
            }
        }

        public EventCodeEnum CardIDMovePosition(CardHolder holder)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                if (holder == null)
                {
                    return errorCode;
                }
                long timeOut = DefaultJobTimeout;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    lock (plcLockObject)
                    {
                        CDXOut.nArmIndex = (ushort)3;
                        WriteCDXOut();
                    }
                    LoggerManager.Debug($"CardIDMovePosition(): ");
                    errorCode = SetRobotCommand(EnumRobotCommand.CARDID_MOVE);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.CARDID_MOVED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = SetRobotCommand(EnumRobotCommand.IDLE);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    var ccsuper = loaderModule.LoaderMaster.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
                    var pivinfo = new PIVInfo() { StageNumber = ccsuper.GetRunningCCInfo().cardreqmoduleIndex };//현재 진행중인 ActiveCCInfo의 CardReqModule.Id.Index가 되어야함.!
                    Func<EventCodeEnum> CardIDRetryAndEvent = delegate()
                    {
                        EventCodeEnum funcErrorCode = EventCodeEnum.NONE;
                        try
                        {
                            CardIDManualInput.Show(this.GetLoaderContainer(), holder.TransferObject);

                            if (holder.TransferObject == null)
                            {
                                funcErrorCode = EventCodeEnum.NODATA;
                                LoggerManager.Error($"CardIDManualInput is Null Error");

                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                this.EventManager().RaisingEvent(typeof(CardIdReadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();
                            }
                            else if (string.IsNullOrEmpty(holder.TransferObject.ProbeCardID.Value))
                            {
                                funcErrorCode = EventCodeEnum.NODATA;
                                LoggerManager.Error($"CardIDManualInput is Null Error");

                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                this.EventManager().RaisingEvent(typeof(CardIdReadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();
                            }
                            else
                            {
                                string Cardid = holder.TransferObject.ProbeCardID.Value;
                                EventCodeEnum cardidvalidatyionresult = ccsuper.CardinfoValidation(Cardid, out string Msg);
                                if (cardidvalidatyionresult == EventCodeEnum.NONE)
                                {
                                    loaderModule.LoaderMaster.CardIDLastTwoWord = Cardid.Substring(Cardid.Length - 2, 2); 
                                    pivinfo = new PIVInfo() { ProbeCardID = Cardid, StageNumber = ccsuper.GetRunningCCInfo().cardreqmoduleIndex }; //채워 넣어야함, 정보 채우는 부분임
                                   
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(CardIdReadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();
                                }
                                else 
                                {
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(CardIdReadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();
                                    loaderModule.ResonOfError = Msg;
                                    funcErrorCode = cardidvalidatyionresult;
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            funcErrorCode = EventCodeEnum.NODATA;
                            LoggerManager.Exception(err);
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(CardIdReadFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                            semaphore.Wait();
                        }
                        return funcErrorCode;
                        
                    };

                    if (!SysParam.CardIDReaderAttatched.Value)
                    {
                        //holder.TransferObject.ProbeCardID.Value = $"{holder.TransferObject.OriginHolder.Label}";
                        holder.TransferObject.ProbeCardID.Value = loaderModule.LoaderMaster.CardIDFullWord;
                        if (string.IsNullOrEmpty(loaderModule.LoaderMaster.CardIDFullWord))
                        {
                            errorCode = CardIDRetryAndEvent();
                        }
                        else
                        {
                            loaderModule.LoaderMaster.CardIDLastTwoWord = loaderModule.LoaderMaster.CardIDFullWord.Substring(loaderModule.LoaderMaster.CardIDFullWord.Length - 2, 2);
                        }                        
                    }
                    else
                    {
                        if (GetCardIDReadDataReady() == true
                            && (SysParam.CardIDReaderType.Value != EnumCardIDReaderType.RFID || RFIDReader?.CommModule?.GetCommState() == EnumCommunicationState.CONNECTED))
                        {
                            string ReceivedCardID = GetReceivedCardID();
                            if (ReceivedCardID.Equals(""))
                            {
                                errorCode = CardIDRetryAndEvent();
                            }
                            else
                            {
                                LoggerManager.Debug($"PC ID: {ReceivedCardID}");
                                EventCodeEnum cardidvalidatyionresult = ccsuper.CardinfoValidation(ReceivedCardID, out string Msg);
                                if (cardidvalidatyionresult == EventCodeEnum.NONE)
                                {
                                    if (holder.TransferObject != null)
                                    {
                                        holder.TransferObject.ProbeCardID.Value = ReceivedCardID;
                                    }
                                    loaderModule.LoaderMaster.CardIDLastTwoWord = ReceivedCardID.Substring(ReceivedCardID.Length - 2, 2);

                                    LoggerManager.Debug($"CardIDLastTwoWord : {loaderModule.LoaderMaster.CardIDLastTwoWord}");

                                    pivinfo = new PIVInfo() { ProbeCardID = ReceivedCardID, StageNumber = ccsuper.GetRunningCCInfo().cardreqmoduleIndex };
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(CardIdReadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();
                                }
                                else
                                {
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(CardIdReadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();
                                    loaderModule.ResonOfError = Msg;
                                    errorCode = cardidvalidatyionresult;
                                }
                            }

                            ClearCardID();
                        }
                        else
                        {
                            errorCode = CardIDRetryAndEvent();
                        }
                    }
                    
                }
            }
            catch (Exception err)
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                this.EventManager().RaisingEvent(typeof(CardIdReadFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                semaphore.Wait();
                loaderModule.ResonOfError = $"CardIDMovePosition(): Card ID Move Position Error {errorCode}.";
                LoggerManager.Error($"CardIDMovePosition(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }

        public EventCodeEnum SetCardID(CardHolder holder ,string cardid)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                if (holder.TransferObject != null)
                {
                    holder.TransferObject.ProbeCardID.Value = cardid;

                    if (string.IsNullOrEmpty(cardid) == false)
                    {
                        loaderModule.LoaderMaster.CardIDLastTwoWord = holder.TransferObject.ProbeCardID.Value.Substring(holder.TransferObject.ProbeCardID.Value.Length - 2, 2);
                        LoggerManager.Debug($"SetCardID. CardIDLastTwoWord: {loaderModule.LoaderMaster.CardIDLastTwoWord}");
                    }

                    errorCode = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"SetCardID(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum CardBufferPick(ICardBufferModule CCBuffer, ICardARMModule arm, int holderNum = -1)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                if (SystemManager.SystemType == SystemTypeEnum.GOP)
                {
                    if (holderNum == -1)
                    {
                        if (CCBuffer.Holder != null)
                        {
                            if (CCBuffer.Holder.Status == EnumSubsStatus.CARRIER)
                            {
                                PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)1);
                            }
                            else if (CCBuffer.Holder.Status == EnumSubsStatus.EXIST)
                            {
                                PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)2);
                            }
                            else
                            {
                                PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)0);
                            }
                        }
                    }
                    else
                    {
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)holderNum);
                    }
                }
                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    lock (plcLockObject)
                    {
                        CDXOut.nCardBufferPos = (short)CCBuffer.ID.Index;
                        CDXOut.nArmIndex = (ushort)3;
                        WriteCDXOut();
                    }
                    LoggerManager.Debug($"CardBufferPick(): Card pick from buffer. Target = {CDXOut.nCardBufferPos}");
                    errorCode = SetRobotCommand(EnumRobotCommand.CARDBUFF_PICK);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.CARDBUFF_PICKED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = SetRobotCommand(EnumRobotCommand.IDLE);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    //if (BCDReader.DataReady == true)
                    //{
                    //    CCBuffer.Holder.TransferObject.ProbeCardID.Value = BCDReader.ReceivedBCD;
                    //    LoggerManager.Debug($"PC ID: {BCDReader.ReceivedBCD}");
                    //    BCDReader.Clear();
                    //}
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"CardBufferPick(): Card Buffer Pick Error {errorCode}.";
                LoggerManager.Error($"CardBufferPick(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum CardBufferPut(ICardARMModule arm, ICardBufferModule CCBuffer, int holderNum = -1)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                if (SystemManager.SystemType == SystemTypeEnum.GOP)
                {
                    if (holderNum == -1)
                    {
                        if (arm.Holder != null)
                        {
                            if (arm.Holder.Status == EnumSubsStatus.CARRIER)
                            {
                                PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)1);
                            }
                            else if (arm.Holder.Status == EnumSubsStatus.EXIST)
                            {
                                PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)2);
                            }
                            else
                            {
                                PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)0);
                            }
                        }
                    }
                    else
                    {
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)holderNum);
                    }
                }
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    lock (plcLockObject)
                    {
                        CDXOut.nCardBufferPos = (short)CCBuffer.ID.Index;
                        CDXOut.nArmIndex = (ushort)3;
                        WriteCDXOut();
                    }
                    LoggerManager.Debug($"CardBufferPut(): Card put to buffer. Target = {CDXOut.nCardBufferPos}");

                    errorCode = SetRobotCommand(EnumRobotCommand.CARDBUFF_PUT);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.CARDBUFF_PUTED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = SetRobotCommand(EnumRobotCommand.IDLE);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"CardBufferPut(): Card Buffer Put Error {errorCode}.";
                LoggerManager.Error($"CardBufferPut(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum FixedTrayPick(IFixedTrayModule FixedTray, IARMModule arm)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    CDXOut.nFixTrayPos = (short)FixedTray.ID.Index;
                    CDXOut.nArmIndex = (ushort)armIndex;
                    LoggerManager.Debug($"FixedTrayPick(): Pick wafer from {CDXOut.nFixTrayPos} Buffer with {CDXOut.nArmIndex} Arm");

                    errorCode = SetRobotCommand(EnumRobotCommand.FT_PICK);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.FT_PICKED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"FixedTrayPick(): Fixed Tray Pick Error {errorCode}.";
                LoggerManager.Error($"FixedTrayPick(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum FixedTrayPut(IARMModule arm, IFixedTrayModule FixedTray)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    CDXOut.nFixTrayPos = (short)FixedTray.ID.Index;
                    CDXOut.nArmIndex = (ushort)armIndex;
                    LoggerManager.Debug($"FixedTrayPut(): Put wafer to {CDXOut.nFixTrayPos} Buffer with {CDXOut.nArmIndex} Arm");

                    errorCode = SetRobotCommand(EnumRobotCommand.FT_PUT);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.FT_PUTED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"FixedTrayPut(): Fixed Tray Put Error {errorCode}.";
                LoggerManager.Error($"FixedTrayPut(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }

        public EventCodeEnum CardMoveLoadingPosition(ICCModule CardChanger, ICardARMModule arm)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    CDXOut.nStagePos = (short)CardChanger.ID.Index;
                    LoggerManager.Debug($"CardMoveLoadingPosition(): Card Move to CC. Target = {CDXOut.nStagePos}");

                    errorCode = SetRobotCommand(EnumRobotCommand.CC_LOADMOVE);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.CC_LOADMOVED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"CardMoveLoadingPosition(): Card MoveLoading Position Error {errorCode}.";
                LoggerManager.Error($"CardMoveLoadingPosition(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum CardChangerPick(ICCModule CardChanger, ICardARMModule arm, int holderNum = -1)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                if (SystemManager.SystemType == SystemTypeEnum.GOP)
                {
                    if (holderNum == -1)
                    {
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)2);
                    }
                    else
                    {
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)holderNum);
                    }
                }

                long timeOut = DefaultJobTimeout * 3;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    CDXOut.nStagePos = (short)CardChanger.ID.Index;
                    LoggerManager.Debug($"CardChangerPick(): Card pick from CC. Target = {CDXOut.nStagePos}");

                    errorCode = SetRobotCommand(EnumRobotCommand.CC_PICK);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.CC_PICKED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"CardChangePick(): Card Change Pick Error {errorCode}.";
                LoggerManager.Error($"CardChangerPick(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum CardChangerPut(ICardARMModule arm, ICCModule CardChanger)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                WriteWaitHandle(0);
                if (IsControllerInIdle() == true)
                {
                    CDXOut.nStagePos = (short)CardChanger.ID.Index;
                    LoggerManager.Debug($"CardChangerPut(): Card put to CC. Target = {CDXOut.nStagePos}");

                    errorCode = SetRobotCommand(EnumRobotCommand.CC_PUT);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.CC_PUTED, timeOut * 3);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"CardChangePut(): Card Change Put Error {errorCode}.";
                LoggerManager.Error($"CardChangerPut(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum ChuckMoveLoadingPosition(IChuckModule Chuck, IARMModule arm)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    CDXOut.nStagePos = (short)Chuck.ID.Index;

                    errorCode = SetRobotCommand(EnumRobotCommand.STAGE_LOADMOVE);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.STAGE_LOADMOVED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"ChuckMoveLoadingPosition(): Chuck Move Loading Position Error {errorCode}.";
                LoggerManager.Error($"ChuckMoveLoadingPosition(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum ChuckPick(IChuckModule Chuck, IARMModule arm, int option = 0)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                long timeOut = DefaultJobTimeout;
                short handle = 0;
                var armIndex = arm.ID.Index;
                bool usinghandler = this.StageSupervisor().CheckUsingHandler(Chuck.ID.Index);

                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    CDXOut.nStagePos = (short)Chuck.ID.Index;
                    CDXOut.nArmIndex = (ushort)armIndex;

                    errorCode = WriteWaitHandle(0);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }

                    errorCode = SetRobotCommand(EnumRobotCommand.STAGE_PICK);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }

                    if (usinghandler == true)
                    {
                        // Check and wait for WaitHandle = 1
                        // Ready to receive wafers with arm vac off
                        handle = 1;
                        errorCode = WaitForHandle(handle, timeOut);
                        if (errorCode != EventCodeEnum.NONE)
                        {
                            throw new ProberSystemException(errorCode);
                        }
                        // Turn off Bernoulli

                        // Set wait handle as 2
                        // Arm vac on and take wafer
                        handle = 2;
                        errorCode = WriteWaitHandle(handle);
                        if (errorCode != EventCodeEnum.NONE)
                        {
                            throw new ProberSystemException(errorCode);
                        }

                        errorCode = this.StageSupervisor().HandlerVacOnOff(false, Chuck.ID.Index);
                        if (errorCode != EventCodeEnum.NONE)
                        {
                            throw new ProberSystemException(errorCode);
                        }
                        System.Threading.Thread.Sleep(500);
                    }

                    errorCode = WaitForCommandDone(EnumRobotState.STAGE_PICKED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"ChuckPick(): Chuck Pick Error {errorCode}.";
                LoggerManager.Error($"ChuckPick(): Exception occurred. Err = {err.Message}");
            }
            return errorCode;
        }
        public EventCodeEnum ChuckPut(IARMModule arm, IChuckModule Chuck, int option = 0)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            bool isNullError = false;
            bool usinghandler = this.StageSupervisor().CheckUsingHandler(Chuck.ID.Index);
            try
            {
                if (arm == null || arm.ID == null)
                {
                    LoggerManager.Debug($"ChuckPut(): Arm is Null");
                    isNullError = true;
                }
                if (Chuck == null || Chuck.ID == null)
                {
                    LoggerManager.Debug($"ChuckPut(): Chuck is Null");
                    isNullError = true;
                }

                if (usinghandler)
                {
                    errorCode = this.StageSupervisor().HandlerVacOnOff(true, Chuck.ID.Index);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                LoggerManager.Debug($"ChuckPut(): SetRobotCommand(IDLE) Start");
                SetRobotCommand(EnumRobotCommand.IDLE);
                LoggerManager.Debug($"ChuckPut(): SetRobotCommand(IDLE) Done");
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                LoggerManager.Debug($"ChuckPut(): WaitForCommandDone(IDLE) Done");
                if (IsControllerInIdle() == true)
                {
                    if (CDXOut == null)
                    {
                        LoggerManager.Debug($"ChuckPut(): CDXOut is Null");
                        isNullError = true;
                    }
                    CDXOut.nStagePos = (short)Chuck.ID.Index;
                    CDXOut.nArmIndex = (ushort)armIndex;
                    LoggerManager.Debug($"ChuckPut(): SetRobotCommand(STAGE_PUT) Start");
                    errorCode = SetRobotCommand(EnumRobotCommand.STAGE_PUT);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    LoggerManager.Debug($"ChuckPut(): SetRobotCommand(STAGE_PUT) Done");
                    errorCode = WaitForCommandDone(EnumRobotState.STAGE_PUTED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                if (isNullError)
                {
                    errorCode = EventCodeEnum.NODATA;
                }
                else
                {
                    errorCode = EventCodeEnum.UNDEFINED;
                }
                loaderModule.ResonOfError = $"ChuckPut(): Chuck Put Error {errorCode}.";
                LoggerManager.Error($"ChuckPut(): Exception occurred. Err = {err.Message}");
            }
            LoggerManager.Debug($"ChuckPut(): ReturnValue:{errorCode}");
            return errorCode;
        }
        public EventCodeEnum CardTrayPick(ICardBufferTrayModule CCTray, ICardARMModule arm)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    if (SystemManager.SystemType == SystemTypeEnum.GOP)
                    {
                        short trayIndexOffset = 0;
                        trayIndexOffset = (short)SystemModuleCount.ModuleCnt.CardBufferCount;
                        CDXOut.nCardBufferPos = (short)(CCTray.ID.Index + trayIndexOffset);
                        if (CCTray.Holder.Status == EnumSubsStatus.EXIST)
                        {
                            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)2);
                        }
                        else if (CCTray.Holder.Status == EnumSubsStatus.CARRIER)
                        {
                            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)1);
                        }
                        else
                        {
                            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)0);
                        }
                    }
                    else
                    {
                        //   CDXOut.nCardBufferPos = (short)(CCTray.ID.Index + SystemModuleCount.ModuleCnt.CardBufferCount);
                        CDXOut.nCardBufferPos = (short)(CCTray.ID.Index + loaderModule.SystemParameter.CardTrayIndexOffset.Value);
                    }
                    CDXOut.nArmIndex = (ushort)3;
                    WriteCDXOut();
                    LoggerManager.Debug($"CardTrayPick(): Pick card from tray. Target = {CDXOut.nCardBufferPos}.");
                    errorCode = SetRobotCommand(EnumRobotCommand.CARDBUFF_PICK);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.CARDBUFF_PICKED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }


                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"CardTrayPick(): Card Tray Pick Error {errorCode}.";
                LoggerManager.Error($"CardTrayPick(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum CardTrayPick(ICardBufferTrayModule CCTray, ICardARMModule arm, int holderNum = -1)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    if (SystemManager.SystemType == SystemTypeEnum.GOP)
                    {
                        short trayIndexOffset = 0;
                        trayIndexOffset = (short)SystemModuleCount.ModuleCnt.CardBufferCount;
                        CDXOut.nCardBufferPos = (short)(CCTray.ID.Index + trayIndexOffset);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)holderNum);
                    }
                    else
                    {
                       // CDXOut.nCardBufferPos = (short)(CCTray.ID.Index + SystemModuleCount.ModuleCnt.CardBufferCount);
                        CDXOut.nCardBufferPos = (short)(CCTray.ID.Index + loaderModule.SystemParameter.CardTrayIndexOffset.Value);
                    }
                    CDXOut.nArmIndex = (ushort)3;
                    WriteCDXOut();
                    LoggerManager.Debug($"CardTrayPick(): Pick card from tray. Target = {CDXOut.nCardBufferPos}.");
                    errorCode = SetRobotCommand(EnumRobotCommand.CARDBUFF_PICK);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.CARDBUFF_PICKED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }


                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"CardTrayPick(): Card Tray Pick Error {errorCode}.";
                LoggerManager.Error($"CardTrayPick(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum CardTrayPut(ICardARMModule arm, ICardBufferTrayModule CCTray, int holderNum = -1)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    if (SystemManager.SystemType == SystemTypeEnum.GOP)
                    {
                        short trayIndexOffset = 0;
                        trayIndexOffset = (short)SystemModuleCount.ModuleCnt.CardBufferCount;
                        CDXOut.nCardBufferPos = (short)(CCTray.ID.Index + trayIndexOffset);
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)holderNum);
                    }
                    else
                    {
                      //  CDXOut.nCardBufferPos = (short)(CCTray.ID.Index + SystemModuleCount.ModuleCnt.CardBufferCount);
                        CDXOut.nCardBufferPos = (short)(CCTray.ID.Index + loaderModule.SystemParameter.CardTrayIndexOffset.Value);
                    }
                    CDXOut.nArmIndex = (ushort)3;
                    WriteCDXOut();
                    LoggerManager.Debug($"CardTrayPut(): Put card to tray. Target = {CDXOut.nCardBufferPos}.");

                    errorCode = SetRobotCommand(EnumRobotCommand.CARDBUFF_PUT);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.CARDBUFF_PUTED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CardBufferPut(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum CardTrayPut(ICardARMModule arm, ICardBufferTrayModule CCTray)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                
                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    if (SystemManager.SystemType == SystemTypeEnum.GOP)
                    {
                        short trayIndexOffset = 0;
                        trayIndexOffset = (short)SystemModuleCount.ModuleCnt.CardBufferCount;
                        CDXOut.nCardBufferPos = (short)(CCTray.ID.Index + trayIndexOffset);
                        if (arm.Holder.Status == EnumSubsStatus.EXIST)
                        {
                            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)2);
                        }
                        else if (arm.Holder.Status == EnumSubsStatus.CARRIER)
                        {
                            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)1);
                        }
                        else
                        {
                            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.IsCardHolderOn.Handle, (short)0);
                        }
                    }
                    else
                    {
                       // CDXOut.nCardBufferPos = (short)(CCTray.ID.Index + SystemModuleCount.ModuleCnt.CardBufferCount);
                        CDXOut.nCardBufferPos = (short)(CCTray.ID.Index + loaderModule.SystemParameter.CardTrayIndexOffset.Value);
                    }
                    CDXOut.nArmIndex = (ushort)3;
                    WriteCDXOut();
                    LoggerManager.Debug($"CardTrayPut(): Put card to tray. Target = {CDXOut.nCardBufferPos}.");

                    errorCode = SetRobotCommand(EnumRobotCommand.CARDBUFF_PUT);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.CARDBUFF_PUTED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"CardBufferPut(): Card Buffer Put Error {errorCode}.";
                LoggerManager.Error($"CardBufferPut(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum JogMove(ProbeAxisObject axisobj, double dist)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                long timeOut = DefaultJobTimeout;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                EnumAxisConstants axis;



                if (IsControllerInIdle() == true)
                {
                    CDXOut.nLX_JogCmd = 0;
                    CDXOut.nLZ_JogCmd = 0;
                    CDXOut.nLW_JogCmd = 0;
                    CDXOut.nLT_JogCmd = 0;
                    CDXOut.nLUD_JogCmd = 0;
                    CDXOut.nLUU_JogCmd = 0;
                    CDXOut.nLCC_JogCmd = 0;
                    CDXOut.nFoup_JogCmd[0] = 0;
                    CDXOut.nFoup_JogCmd[1] = 0;
                    CDXOut.nFoup_JogCmd[2] = 0;
                    PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CDXOut.Handle, CDXOut);      // Update position

                    var foupJogCmd = new UInt16[SystemModuleCount.ModuleCnt.FoupCount];
                    var foupJogPos = new int[SystemModuleCount.ModuleCnt.FoupCount];
                    for (int i = 0; i < foupJogCmd.Count(); i++)
                    {
                        foupJogCmd[i] = 0;
                        foupJogPos[i] = 0;
                    }

                    if(SymbolMap.OutputSymbol.Foup_JogCmd != null && SymbolMap.OutputSymbol.Foup_JogCmd.Handle > 0 )
                    {
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Foup_JogCmd.Handle, foupJogCmd);      // Update position
                    }

                    axis = axisobj.AxisType.Value;
                    errorCode = SetRobotState(EnumLoaderMode.JOG);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    UInt16 jogCmd = 0x0;
                    if (dist > 0)
                    {
                        jogCmd = 0x01 << 1;
                        jogCmd = 0x01;
                    }
                    else if (dist < 0)
                    {
                        jogCmd = 0x01 << 2;
                        jogCmd = 0x01;
                    }
                    else
                    {
                        LoggerManager.Debug($"{axis} Axis zero length move error occurred.");
                        errorCode = EventCodeEnum.MOTION_MOVING_ERROR;
                        //throw new ProberSystemException(errorCode);
                    }
                    switch (axis)
                    {
                        case EnumAxisConstants.LX:
                            CDXOut.nLX_JogCmd = 0;
                            break;
                        case EnumAxisConstants.LZM:
                            CDXOut.nLZ_JogCmd = 0;
                            break;
                        case EnumAxisConstants.LZS:
                            CDXOut.nLZ_JogCmd = 0;
                            break;
                        case EnumAxisConstants.LW:
                            CDXOut.nLW_JogCmd = 0;
                            break;
                        case EnumAxisConstants.LB:
                            CDXOut.nLT_JogCmd = 0;
                            break;
                        case EnumAxisConstants.LUD:
                            CDXOut.nLUD_JogCmd = 0;
                            break;
                        case EnumAxisConstants.LUU:
                            CDXOut.nLUU_JogCmd = 0;
                            break;
                        case EnumAxisConstants.LCC:
                            CDXOut.nLCC_JogCmd = 0;
                            break;
                        case EnumAxisConstants.FC1:
                            CDXOut.nFoup_JogCmd[0] = 0;
                            foupJogCmd[0] = 0;
                            break;
                        case EnumAxisConstants.FC2:
                            CDXOut.nFoup_JogCmd[1] = 0;
                            foupJogCmd[1] = 0;
                            break;
                        case EnumAxisConstants.FC3:
                            CDXOut.nFoup_JogCmd[2] = 0;
                            foupJogCmd[2] = 0;
                            break;
                        case EnumAxisConstants.FC4:
                            foupJogCmd[3] = 0;
                            break;
                        default:
                            errorCode = EventCodeEnum.MOTION_UNAVAILABLE_AXIS_ERROR;
                            break;
                    }
                    CDXOut.nLX_JogCmd = 0;
                    CDXOut.nLZ_JogCmd = 0;
                    CDXOut.nLW_JogCmd = 0;
                    CDXOut.nLT_JogCmd = 0;
                    CDXOut.nLUD_JogCmd = 0;
                    CDXOut.nLUU_JogCmd = 0;
                    CDXOut.nLCC_JogCmd = 0;
                    CDXOut.nFoup_JogCmd[0] = 0;
                    CDXOut.nFoup_JogCmd[1] = 0;
                    CDXOut.nFoup_JogCmd[2] = 0;
                    PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CDXOut.Handle, CDXOut);

                    if (SymbolMap.OutputSymbol.Foup_JogCmd != null && SymbolMap.OutputSymbol.Foup_JogCmd.Handle > 0)
                    {
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Foup_JogCmd.Handle, foupJogCmd);      // Update position
                    }
                    
                    switch (axis)
                    {
                        case EnumAxisConstants.LX:
                            CDXOut.nLX_JogPos = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            break;
                        case EnumAxisConstants.LZM:
                            CDXOut.nLZ_JogPos = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            break;
                        case EnumAxisConstants.LZS:
                            CDXOut.nLZ_JogPos = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            break;
                        case EnumAxisConstants.LW:
                            CDXOut.nLW_JogPos = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            break;
                        case EnumAxisConstants.LB:
                            CDXOut.nLT_JogPos = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            break;
                        case EnumAxisConstants.LUD:
                            CDXOut.nLUD_JogPos = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            break;
                        case EnumAxisConstants.LUU:
                            CDXOut.nLUU_JogPos = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            break;
                        case EnumAxisConstants.LCC:
                            CDXOut.nLCC_JogPos = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            break;
                        case EnumAxisConstants.FC1:
                            CDXOut.nFoup_JogPos[0] = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            foupJogPos[0] = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            break;
                        case EnumAxisConstants.FC2:
                            CDXOut.nFoup_JogPos[1] = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            foupJogPos[1] = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            break;
                        case EnumAxisConstants.FC3:
                            CDXOut.nFoup_JogPos[2] = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            foupJogPos[2] = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            break;
                        case EnumAxisConstants.FC4:
                            //CDXOut.nFoup_JogPos[2] = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            foupJogPos[3] = (int)axisobj.DtoP(dist) + (int)axisobj.DtoP(axisobj.Status.Position.Actual);
                            break;
                        default:
                            errorCode = EventCodeEnum.MOTION_UNAVAILABLE_AXIS_ERROR;
                            break;
                    }

                    PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CDXOut.Handle, CDXOut);      // Update position

                    if(SymbolMap.OutputSymbol.Foup_JogPos != null && SymbolMap.OutputSymbol.Foup_JogPos.Handle > 0)
                    {
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Foup_JogPos.Handle, foupJogPos);
                    }

                    switch (axis)
                    {
                        case EnumAxisConstants.LX:
                            CDXOut.nLX_JogCmd = jogCmd;
                            break;
                        case EnumAxisConstants.LZM:
                            CDXOut.nLZ_JogCmd = jogCmd;
                            break;
                        case EnumAxisConstants.LZS:
                            CDXOut.nLZ_JogCmd = jogCmd;
                            break;
                        case EnumAxisConstants.LW:
                            CDXOut.nLW_JogCmd = jogCmd;
                            break;
                        case EnumAxisConstants.LB:
                            CDXOut.nLT_JogCmd = jogCmd;
                            break;
                        case EnumAxisConstants.LUD:
                            CDXOut.nLUD_JogCmd = jogCmd;
                            break;
                        case EnumAxisConstants.LUU:
                            CDXOut.nLUU_JogCmd = jogCmd;
                            break;
                        case EnumAxisConstants.LCC:
                            CDXOut.nLCC_JogCmd = jogCmd;
                            break;
                        case EnumAxisConstants.FC1:
                            CDXOut.nFoup_JogCmd[0] = jogCmd;
                            foupJogCmd[0] = jogCmd;
                            break;
                        case EnumAxisConstants.FC2:
                            CDXOut.nFoup_JogCmd[1] = jogCmd;
                            foupJogCmd[1] = jogCmd;
                            break;
                        case EnumAxisConstants.FC3:
                            CDXOut.nFoup_JogCmd[2] = jogCmd;
                            foupJogCmd[2] = jogCmd;
                            break;
                        case EnumAxisConstants.FC4:
                            CDXOut.nFoup_JogCmd[2] = jogCmd;
                            foupJogCmd[3] = jogCmd;
                            break;
                        default:
                            errorCode = EventCodeEnum.MOTION_UNAVAILABLE_AXIS_ERROR;
                            break;
                    }
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    else
                    {
                        LoggerManager.Debug($"JogNove(): {axis} Jog move. Dist. = {dist}");
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CDXOut.Handle, CDXOut);  // Issue command

                        if (SymbolMap.OutputSymbol.Foup_JogCmd != null && SymbolMap.OutputSymbol.Foup_JogCmd.Handle > 0)
                        {
                            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Foup_JogCmd.Handle, foupJogCmd);      // Update position
                        }

                        Thread.Sleep(100);
                        CDXOut.nLX_JogCmd = 0;
                        CDXOut.nLZ_JogCmd = 0;
                        CDXOut.nLW_JogCmd = 0;
                        CDXOut.nLT_JogCmd = 0;
                        CDXOut.nLUD_JogCmd = 0;
                        CDXOut.nLUU_JogCmd = 0;
                        CDXOut.nLCC_JogCmd = 0;
                        CDXOut.nFoup_JogCmd[0] = 0;
                        CDXOut.nFoup_JogCmd[1] = 0;
                        CDXOut.nFoup_JogCmd[2] = 0;


                        for (int i = 0; i < foupJogCmd.Count(); i++)
                        {
                            foupJogCmd[i] = 0;
                            foupJogPos[i] = 0;
                        }

                        if (SymbolMap.OutputSymbol.Foup_JogCmd != null && SymbolMap.OutputSymbol.Foup_JogCmd.Handle > 0)
                        {
                            PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.Foup_JogCmd.Handle, foupJogCmd);      // Update position
                        }
                        JogMoveWait();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"JogMove(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }

        public EventCodeEnum JogMoveWait()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            Stopwatch elapsedStopWatch = new Stopwatch();
            int timeout = 0;
            try
            {
                timeout = 1000;
                elapsedStopWatch.Reset();
                elapsedStopWatch.Start();
                bool JogCmdTriggerOnFlag = true;
                bool JogCmdTriggerOffFlag = true;

                //JobCmd가 1로 바뀔때 까지 기다려줌.
                //짧은 동작은 1로 바뀌었다가 바로 0으로 바뀔 수 있으므로 Timeout 줌(1000)
                while (JogCmdTriggerOnFlag)
                {
                    if (CDXOutState.nLX_JogCmd == 1
                        || CDXOutState.nLZ_JogCmd == 1
                        || CDXOutState.nLW_JogCmd == 1
                        || CDXOutState.nLT_JogCmd == 1
                        || CDXOutState.nLUD_JogCmd == 1
                        || CDXOutState.nLUU_JogCmd == 1
                        || CDXOutState.nLCC_JogCmd == 1)
                    {
                        JogCmdTriggerOnFlag = false;
                    }
                    if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                    {
                        JogCmdTriggerOnFlag = false;
                        LoggerManager.Debug($"JogMoveWait(): JogCmdTriggerOnFlag Timeout occurred, Timeout = {timeout}");
                    }
                    Thread.Sleep(100);
                }

                //모든 JobCmd가 0으로 바뀔때 까지 기다려줌.
                while (JogCmdTriggerOffFlag)
                {
                    if (CDXOutState.nLX_JogCmd == 0
                        && CDXOutState.nLZ_JogCmd == 0
                        && CDXOutState.nLW_JogCmd == 0
                        && CDXOutState.nLT_JogCmd == 0
                        && CDXOutState.nLUD_JogCmd == 0
                        && CDXOutState.nLUU_JogCmd == 0
                        && CDXOutState.nLCC_JogCmd == 0)
                    {
                        JogCmdTriggerOffFlag = false;
                    }
                    if (elapsedStopWatch.ElapsedMilliseconds > DefaultJobTimeout)
                    {
                        JogCmdTriggerOffFlag = false;
                        LoggerManager.Debug($"JogMoveWait(): JogCmdTriggerOffFlag Timeout occurred, Timeout = {DefaultJobTimeout}");
                    }
                    Thread.Sleep(100);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private EventCodeEnum SetRobotState(EnumLoaderMode mode, int timeout = 200)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                lock (plcLockObject)
                {
                    CDXOut.nMode = (ushort)mode;
                    PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CDXOut.Handle, CDXOut);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ChuckPut(): Exception occurred. Err = {err.Message}");
                errorCode = EventCodeEnum.LOADER_SYSTEM_ERROR;
                return errorCode;
            }

            Stopwatch elapsedStopWatch = new Stopwatch();
            try
            {
                if (timeout == 0)
                {
                    timeout = DefaultJobTimeout;
                }
                elapsedStopWatch.Reset();
                elapsedStopWatch.Start();
                bool runFlag = true;
                do
                {
                    if (CDXIn.nState == (ushort)mode)
                    {
                        runFlag = false;
                        errorCode = EventCodeEnum.NONE;
                    }
                    if (CDXIn.nState == (ushort)EnumLoaderMode.ERROR)
                    {
                        runFlag = false;
                        errorCode = EventCodeEnum.LOADER_STATE_INVALID;
                    }
                    if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                    {
                        runFlag = false;
                        LoggerManager.Debug($"SetRobotState(): Timeout occurred. Target state = {mode}, Timeout = {timeout}");
                        errorCode = EventCodeEnum.LOADER_ROBOTCMD_TIMEOUT;
                    }
                } while (runFlag == true);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaitForCommandDone(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            finally
            {
                elapsedStopWatch.Stop();
            }
            return errorCode;
        }
        public EnumLoaderMode GetRobotState()
        {
            return (EnumLoaderMode)CDXIn.nRobotState;
        }


        public EventCodeEnum CassetteLoad(ICassetteModule cassetteModule)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var CasstteIndex = cassetteModule.ID.Index;
                EnumCSTCommand cmd = EnumCSTCommand.IDLE;
                EnumCSTCommand resetcmd = EnumCSTCommand.IDLE;

                if (GetCSTState(cassetteModule.ID.Index - 1) == EnumCSTState.RESET)
                {

                }

                if (GetCSTState(cassetteModule.ID.Index - 1) == EnumCSTState.PRESENCE |
                    GetCSTState(cassetteModule.ID.Index - 1) == EnumCSTState.UNLOADED |
                    GetCSTState(cassetteModule.ID.Index - 1) == EnumCSTState.RESET)
                {

                }
                else
                {
                    LoggerManager.Debug($"Cassette not detected!");
                    errorCode = EventCodeEnum.NONE;
                    return errorCode;
                }

                if (IsCSTControllerInIdle(CasstteIndex) == true)
                {
                    //errorCode = WaitForCSTCommandDone(EnumCSTState.PRESENCE, cassetteModule.ID.Index - 1, timeOut);
                    //if (errorCode != EventCodeEnum.NONE)
                    //{
                    //    LoggerManager.Error($"CassetteLoad(): Exception occurred. Err = {errorCode}");
                    //    throw new ProberSystemException(errorCode);
                    //}

                    CDXOut.nCSTPos = (short)cassetteModule.ID.Index;
                    if (cassetteModule.ID.Index == 1)
                    {
                        cmd = EnumCSTCommand.CST1_LOAD;
                        resetcmd = EnumCSTCommand.CST1_LOAD_RESET;
                    }
                    else if (cassetteModule.ID.Index == 2)
                    {
                        cmd = EnumCSTCommand.CST2_LOAD;
                        resetcmd = EnumCSTCommand.CST2_LOAD_RESET;
                    }
                    else if (cassetteModule.ID.Index == 3)
                    {
                        cmd = EnumCSTCommand.CST3_LOAD;
                        resetcmd = EnumCSTCommand.CST3_LOAD_RESET;
                    }
                    LoggerManager.Debug($"CassetteLoad(): Casette load from port#{CDXOut.nCSTPos}.");
                    errorCode = SetCSTCommand(cmd);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"SetCSTCommand():" + cmd + " Exception occurred. Err = " + errorCode);
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCSTCommandDone(EnumCSTState.LOADED, cassetteModule.ID.Index - 1, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"WaitForCSTCommandDone(): LOADED Exception occurred. Err = {errorCode}");
                        //throw new ProberSystemException(errorCode);
                    }
                    errorCode = SetCSTCommand(resetcmd);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"SetCSTCommand():" + cmd + " Exception occurred. Err = " + errorCode);
                        //throw new ProberSystemException(errorCode);
                    }

                    errorCode = SetCSTCommand(EnumCSTCommand.IDLE);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"SetCSTCommand():" + cmd + " Exception occurred. Err = " + errorCode);
                        //throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CardChangerPut(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }

        public EventCodeEnum CassetteUnLoad(ICassetteModule cassetteModule)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var CasstteIndex = cassetteModule.ID.Index;
                EnumCSTCommand cmd = EnumCSTCommand.IDLE;
                EnumCSTCommand resetcmd = EnumCSTCommand.IDLE;
                if (IsCSTControllerInIdle(CasstteIndex) == true)
                {
                    //errorCode = WaitForCSTCommandDone(EnumCSTState.LOADED, cassetteModule.ID.Index, timeOut);
                    //if (errorCode != EventCodeEnum.NONE)
                    //{
                    //    LoggerManager.Error($"CassetteLoad(): Exception occurred. Err = {errorCode}");
                    //    throw new ProberSystemException(errorCode);
                    //}

                    CDXOut.nCSTPos = (short)cassetteModule.ID.Index;
                    if (cassetteModule.ID.Index == 1)
                    {
                        cmd = EnumCSTCommand.CST1_UNLOAD;
                        resetcmd = EnumCSTCommand.CST1_UNLOAD_RESET;
                    }
                    else if (cassetteModule.ID.Index == 2)
                    {
                        cmd = EnumCSTCommand.CST2_UNLOAD;
                        resetcmd = EnumCSTCommand.CST2_UNLOAD_RESET;
                    }
                    else if (cassetteModule.ID.Index == 3)
                    {
                        cmd = EnumCSTCommand.CST3_UNLOAD;
                        resetcmd = EnumCSTCommand.CST3_UNLOAD_RESET;
                    }
                    LoggerManager.Debug($"CassetteUnLoad(): Casette unload to port#{CDXOut.nCSTPos}.");

                    errorCode = SetCSTCommand(cmd);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);

                    }
                    //errorCode = WaitForCSTCommandDone(EnumCSTState.UNLOADING, cassetteModule.ID.Index, timeOut);
                    //if (errorCode != EventCodeEnum.NONE)
                    //{
                    //    throw new ProberSystemException(errorCode);
                    //}
                    errorCode = WaitForCSTCommandDone(EnumCSTState.UNLOADED, cassetteModule.ID.Index - 1, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = SetCSTCommand(resetcmd);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"SetCSTCommand():" + cmd + " Exception occurred. Err = " + errorCode);
                        throw new ProberSystemException(errorCode);
                    }
                    //errorCode = WaitForCSTCommandDone(EnumCSTState.RESET, cassetteModule.ID.Index - 1, timeOut);
                    //if (errorCode != EventCodeEnum.NONE)
                    //{
                    //    LoggerManager.Error($"WaitForCSTCommandDone(): LOADING Exception occurred. Err = {errorCode}");
                    //    throw new ProberSystemException(errorCode);
                    //}
                    errorCode = SetCSTCommand(EnumCSTCommand.IDLE);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"SetCSTCommand():" + cmd + " Exception occurred. Err = " + errorCode);
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CardChangerPut(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum CassetteScan(ICassetteModule cassetteModule)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            Stopwatch elapsedStopWatch = new Stopwatch();
            try
            {
                elapsedStopWatch.Reset();

                long timeOut = DefaultJobTimeout;
                var CasstteIndex = cassetteModule.ID.Index;
                //EnumCSTCommand cmd = EnumCSTCommand.IDLE;
                if (IsCSTControllerInIdle(CasstteIndex - 1) == true)
                {
                    //CDXOut.nCSTPos = (short)cassetteModule.ID.Index;
                    //WriteCDXOut();
                    var result = CheckArmCassetteInterference(CasstteIndex - 1);
                    if (result != EventCodeEnum.NONE)
                    {
                        return result;
                    }

                    var prevCmd = CDXOutState.nCSTCtrlCmd;
                    if (CasstteIndex > 0)
                    {
                        if (SymbolMap.OutputSymbol.CSTJobCommand != null && SymbolMap.OutputSymbol.CSTJobCommand.Handle > 0)
                        {
                            lock (plcLockObject)
                            {
                                short[] jobCmd = new short[SystemModuleCount.ModuleCnt.FoupCount];
                                var curJobCmd = tcReadArray(SymbolMap.OutputSymbol.CSTJobCommand.Handle, SystemModuleCount.ModuleCnt.FoupCount);

                                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                                {
                                    jobCmd[i] = (short)curJobCmd[i];
                                }
                                jobCmd[CasstteIndex - 1] = 0;
                                PLCModule.tcClient.WriteAny(
                                    SymbolMap.OutputSymbol.CSTJobCommand.Handle, jobCmd);
                                Thread.Sleep(100);
                                jobCmd[CasstteIndex - 1] = 1;
                                PLCModule.tcClient.WriteAny(
                                    SymbolMap.OutputSymbol.CSTJobCommand.Handle, jobCmd);
                            }
                        }
                        else
                        {
                            lock (plcLockObject)
                            {
                                SyncEvent.WaitOne(10);
                                var outData = PLCModule.tcClient.ReadAny(SymbolMap.OutputSymbol.CDXOut.Handle, typeof(stCDXOut));
                                CDXOutState = (stCDXOut)outData;
                                prevCmd = CDXOutState.nCSTCtrlCmd;
                                SyncEvent.WaitOne(10);

                                prevCmd = prevCmd & ~(uint)(0x01 << (CasstteIndex - 1));
                                CDXOut.nCSTCtrlCmd = prevCmd;
                                WriteCDXOut();
                                SyncEvent.WaitOne(10);
                                SyncEvent.WaitOne(10);
                                prevCmd = prevCmd | (uint)(0x01 << (CasstteIndex - 1));
                                CDXOut.nCSTCtrlCmd = prevCmd;
                                WriteCDXOut();
                            }
                        }

                        bool runFlag = true;
                        bool commandDone = false;
                        elapsedStopWatch.Start();

                        do
                        {
                            var cstState = ReadCassetteCtrlState(CasstteIndex - 1);
                            if (cstState == EnumCSTCtrl.SCANNING)
                            {
                                runFlag = false;
                            }
                            if (timeOut != 0)
                            {
                                if (elapsedStopWatch.ElapsedMilliseconds > timeOut)
                                {
                                    runFlag = false;
                                    LoggerManager.Debug($"CassetteScan(): Timeout occurred while wait for scanning. Timeout = {timeOut}");
                                    errorCode = EventCodeEnum.LOADER_ROBOTCMD_TIMEOUT;
                                }
                            }
                        } while (runFlag == true);
                        elapsedStopWatch.Stop();
                        elapsedStopWatch.Reset();
                        elapsedStopWatch.Start();
                        runFlag = true;
                        do
                        {
                            var cstState = ReadCassetteCtrlState(CasstteIndex - 1);
                            if (cstState == EnumCSTCtrl.SCAN_DONE)
                            {
                                LoggerManager.Debug($"CassetteScan(): SCAN_DONE");
                                //this.EventManager().RaisingEvent(typeof(ScanFoupDoneEvent).FullName); 스캔 결과를 같이 넘겨주기 위해서 DoScanJob()으로 옮김.
                                commandDone = true;
                                if (SymbolMap.OutputSymbol.CSTJobCommand != null && SymbolMap.OutputSymbol.CSTJobCommand.Handle > 0)
                                {
                                    lock (plcLockObject)
                                    {
                                        short[] jobCmd = new short[SystemModuleCount.ModuleCnt.FoupCount];
                                        var curJobCmd = tcReadArray(SymbolMap.OutputSymbol.CSTJobCommand.Handle, SystemModuleCount.ModuleCnt.FoupCount);

                                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                                        {
                                            jobCmd[i] = (short)curJobCmd[i];
                                        }
                                        jobCmd[CasstteIndex - 1] = 0;
                                        PLCModule.tcClient.WriteAny(
                                            SymbolMap.OutputSymbol.CSTJobCommand.Handle, jobCmd);
                                    }
                                }
                                else
                                {
                                    lock (plcLockObject)
                                    {
                                        SyncEvent.WaitOne(10);

                                        prevCmd = CDXOutState.nCSTCtrlCmd;
                                        prevCmd = prevCmd & ~(uint)(0x01 << (CasstteIndex - 1));
                                        CDXOut.nCSTCtrlCmd = prevCmd;
                                        WriteCDXOut();
                                    }
                                }
                            }
                            if (cstState == EnumCSTCtrl.ERROR)
                            {                                
                                runFlag = false;
                                errorCode = EventCodeEnum.FOUP_SCAN_FAILED;
                            }
                            if (commandDone == true)
                            {
                                runFlag = false;
                                errorCode = EventCodeEnum.NONE;
                            }
                            if (timeOut != 0)
                            {
                                if (elapsedStopWatch.ElapsedMilliseconds > timeOut)
                                {
                                    runFlag = false;
                                    LoggerManager.Debug($"CassetteScan(): Timeout occurred. Timeout = {timeOut}");
                                    errorCode = EventCodeEnum.LOADER_ROBOTCMD_TIMEOUT;
                                }
                            }
                        } while (runFlag == true);
                    }

                    //if (cassetteModule.ID.Index == 1)
                    //{
                    //    cmd = EnumCSTCommand.CST1_SCAN;
                    //}
                    //else if (cassetteModule.ID.Index == 2)
                    //{
                    //    cmd = EnumCSTCommand.CST2_SCAN;
                    //}
                    //else if (cassetteModule.ID.Index == 3)
                    //{
                    //    cmd = EnumCSTCommand.CST3_SCAN;
                    //}

                    //errorCode = SetCSTCommand(cmd);
                    //if (errorCode != EventCodeEnum.NONE)
                    //{
                    //    throw new ProberSystemException(errorCode);
                    //}
                    //errorCode = WaitForCSTCommandDone(cmd, timeOut);
                    //if (errorCode != EventCodeEnum.NONE)
                    //{
                    //    throw new ProberSystemException(errorCode);
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CassetteScan(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }

        public EventCodeEnum InitRobot()
        {
            EventCodeEnum result = EventCodeEnum.UNDEFINED;
            try
            {
                bool init = InitController();
                UnlockRobot(); //Hynix RobotFrezze Handle Registration이 안되는 경우가 있음.(1호기 기준)
                if (init == true)
                {
                    result = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }
        public EventCodeEnum HomingRobot()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var result = HomingSystem();
                if (result == true)
                {
                    this.loaderModule.ModuleManager.InitAttachModules(true);
                    var CstModules = loaderModule.ModuleManager.FindModules<ICassetteModule>();
                    foreach (var foup in CstModules)
                    {
                        if (foup.Enable == true && foup.FoupState == FoupStateEnum.LOAD && foup.ScanState == LoaderParameters.CassetteScanStateEnum.READ)
                        {
                            var coveropenresult = CoverOpen(foup.ID.Index - 1);
                            if (coveropenresult != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"HomingRobot() : Foup Num #{foup.ID.Index - 1} Cover Open Error.");
                                return EventCodeEnum.FOUP_OPEN_ERROR;
                            }

                        }
                    }
                    VaildWaferinfo();

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.MOTION_LOADER_INIT_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.EXCEPTION;
            }

            return retVal;
        }

        public EventCodeEnum PrealignWafer(IPreAlignModule pa, double notchAngle)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                //public ushort nPreAProcess;                 //: UINT;
                //public ushort nPreAReset;                   //: UINT;
                //public ushort[] nPreA_DWMCmd;
                short paIndex = (short)pa.ID.Index;
                long timeOut = DefaultJobTimeout;
                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    CDXOut.nPreA_DWMCmd[paIndex] = (ushort)(notchAngle * 100.0);
                    CDXOut.nPreAPos = paIndex;
                    errorCode = RunPA(paIndex, notchAngle);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    //errorCode = WaitForCommandDone(EnumRobotState.PA_PUTED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"PAPut(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }


        public int WaitForPA(IPreAlignModule pa)
        {
            int retVal = 0;
            try
            {
                LoggerManager.Debug($"[GPLoader] WaitForPA(): Start, PA.Index:{pa.ID.Index}, Timeout:{20000}");
                retVal = loaderModule.PAManager.PAModules[pa.ID.Index - 1].WaitForPA(20000);
                LoggerManager.Debug($"[GPLoader] WaitForPA(): End, PA.Index:{pa.ID.Index}, Timeout:{20000}");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaitForPA(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return retVal;
        }
        public EventCodeEnum WaitForOCR(IPreAlignModule pa, out String ocr, out double ocrScore)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            ocr = String.Empty;
            ocrScore = 0;
            try
            {
                var cognexprocessmanager = loaderModule.Container.Resolve<ICognexProcessManager>();
                retVal = cognexprocessmanager.WaitForOCR(pa.ID.Index - 1, cognexprocessmanager.CognexProcSysParam.WaitForOcrTimeout_msec);
                ocr = cognexprocessmanager.Ocr[pa.ID.Index - 1];
                ocrScore = cognexprocessmanager.OcrScore[pa.ID.Index - 1];
            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaitForPA(): Exception occurred. Err = {err.Message}");
                throw err;
            }

            return retVal;
        }


        public EventCodeEnum PARotateTo(IPreAlignModule pa, double notchAngle)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                //public ushort nPreAProcess;                 //: UINT;
                //public ushort nPreAReset;                   //: UINT;
                //public ushort[] nPreA_DWMCmd;
                short paIndex = (short)pa.ID.Index;
                long timeOut = DefaultJobTimeout;
                //SetRobotCommand(EnumRobotCommand.IDLE);
                //WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                //if (IsControllerInIdle() == true)
                //{
                //    CDXOut.nPreA_DWMCmd[paIndex] = (ushort)(notchAngle * 100.0);
                //    CDXOut.nPreAPos = paIndex;
                //    errorCode = RunPA(paIndex, notchAngle);
                //    if (errorCode != EventCodeEnum.NONE)
                //    {
                //        throw new ProberSystemException(errorCode);
                //    }
                //    errorCode = WaitForCommandDone(EnumRobotState.PA_PUTED, timeOut);
                //    if (errorCode != EventCodeEnum.NONE)
                //    {
                //        throw new ProberSystemException(errorCode);
                //    }
                //}
                LoggerManager.Debug($"PARotateTo(): Rotate wafer at {paIndex} PA to {notchAngle} Deg.");

                loaderModule.PAManager.PAModules[paIndex - 1].Rotate(notchAngle);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"PAPut(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        #endregion

        public EventCodeEnum CheckWaferIsOnPA(int index, out bool isExist)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = loaderModule.PAManager.PAModules[index - 1].IsSubstrateExist(out isExist);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CheckWaferIsOnPA(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return retVal;
        }

        #region // Cassette controls
        public EnumCSTCtrl ReadCassetteCtrlState(int index)
        {
            EnumCSTCtrl eStatus = EnumCSTCtrl.ERROR;
            try
            {
                if (PLCModule != null)
                {
                    if(SymbolMap.InputSymbol.CSTCtrlStatus.Handle != 0)
                    {
                        var status = tcReadArray(SymbolMap.InputSymbol.CSTCtrlStatus.Handle, SystemModuleCount.ModuleCnt.FoupCount);
                        eStatus = (EnumCSTCtrl)status[index];
                    }
                    else
                    {
                        LoggerManager.Debug($"ReadCassetteCtrlState(): Handle Not defined. Handle = {SymbolMap.InputSymbol.CSTCtrlStatus.Handle}");
                        eStatus = EnumCSTCtrl.NONE;
                    }
                }
                else
                {
                    LoggerManager.Debug($"ReadCassetteCtrlState(): Handle Not defined. Handle = {SymbolMap.InputSymbol.CSTCtrlStatus.Handle}");
                    eStatus = EnumCSTCtrl.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ReadCassetteCtrlState(): Error occurred. Err = {err.Message}");
            }
            return eStatus;
        }
        object cstLock = new object();
        public EventCodeEnum WriteCSTCtrlCommand(int index, EnumCSTCtrl ctrl)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (cstLock == null) cstLock = new object();
                lock (cstLock)
                {
                    if (ReadCassetteCtrlState(index) == EnumCSTCtrl.ERROR)
                    {
                        lock (plcLockObject)
                        {
                            Thread.Sleep(100);

                            //var status = (int[])PLCModule.tcClient.ReadAny(SymbolMap.OutputSymbol.CSTCtrlCommand.Handle,
                            //                            typeof(int[]),
                            //                            new int[] { GPLoaderDef.FoupCount });
                            var status = tcReadArray(SymbolMap.OutputSymbol.CSTCtrlCommand.Handle, SystemModuleCount.ModuleCnt.FoupCount);
                            //LoggerManager.Debug($"WriteCSTCtrlCommand({index}, {ctrl}): Status [{status[0]}, {status[1]}, {status[2]}]");
                            status[index] = (int)EnumCSTCtrl.RESET;
                            short[] ctrlCmds = new short[SystemModuleCount.ModuleCnt.FoupCount];
                            for (int i = 0; i < status.Length; i++)
                            {
                                ctrlCmds[i] = (short)status[i];
                                LoggerManager.Debug($"WriteCSTCtrlCommand({index}, {ctrl}): Status [{status[i]}]");
                            }
                            ctrlCmds[index] = (int)EnumCSTCtrl.RESET;
                            PLCModule.tcClient.WriteAny(
                                SymbolMap.OutputSymbol.CSTCtrlCommand.Handle,
                                ctrlCmds);

                            int readCount = 0;
                            int maxCount = 10;
                            do
                            {
                                Thread.Sleep(100);
                                status = tcReadArray(SymbolMap.OutputSymbol.CSTCtrlCommand.Handle, SystemModuleCount.ModuleCnt.FoupCount);
                                readCount++;
                                if (readCount > maxCount) break;
                            } while (status[index] != ctrlCmds[index]);

                            ret = WaitForCSTStatus(index, EnumCSTCtrl.RESET, DefaultJobTimeout);
                            status = tcReadArray(SymbolMap.OutputSymbol.CSTCtrlCommand.Handle, SystemModuleCount.ModuleCnt.FoupCount);
                            //LoggerManager.Debug($"WriteCSTCtrlCommand({index}, {ctrl}): Status [{status[0]}, {status[1]}, {status[2]}]");
                            status[index] = (int)EnumCSTCtrl.NONE;
                            ctrlCmds = new short[SystemModuleCount.ModuleCnt.FoupCount];
                            for (int i = 0; i < status.Length; i++)
                            {
                                ctrlCmds[i] = (short)status[i];
                                LoggerManager.Debug($"WriteCSTCtrlCommand({index}, {ctrl}): Status of {i} = [{status[i]}]");
                            }
                            ctrlCmds[index] = (int)EnumCSTCtrl.NONE;
                            PLCModule.tcClient.WriteAny(
                                SymbolMap.OutputSymbol.CSTCtrlCommand.Handle,
                                ctrlCmds);
                            readCount = 0;
                            maxCount = 10;
                            do
                            {
                                Thread.Sleep(100);
                                status = tcReadArray(SymbolMap.OutputSymbol.CSTCtrlCommand.Handle, SystemModuleCount.ModuleCnt.FoupCount);
                                readCount++;
                                if (readCount > maxCount) break;
                            } while (status[index] != ctrlCmds[index]);

                            ret = WaitForCSTStatus(index, EnumCSTCtrl.NONE, DefaultJobTimeout);
                        }
                    }

                    lock (plcLockObject)
                    {
                        if (ctrl != EnumCSTCtrl.NONE && ctrl != EnumCSTCtrl.RESET)
                        {
                            PrevIndex_Foup = GetFoupEventLogIndex(index);
                        }

                        LoggerManager.Debug($"WriteCSTCtrlComand(): CST Index = {index}, Command = {ctrl}");

                        var value = (int[])PLCModule.tcClient.ReadAny(
                            SymbolMap.OutputSymbol.CSTCtrlCommand.Handle,
                            typeof(int[]), new int[] { SystemModuleCount.ModuleCnt.FoupCount });

                        var status = tcReadArray(SymbolMap.OutputSymbol.CSTCtrlCommand.Handle, SystemModuleCount.ModuleCnt.FoupCount);

                        //LoggerManager.Debug($"WriteCSTCtrlCommand({index}, {ctrl}): Status [{status[0]}, {status[1]}, {status[2]}]");
                        status[index] = (int)ctrl;

                        short[] ctrlCmds = new short[SystemModuleCount.ModuleCnt.FoupCount];
                        for (int i = 0; i < status.Length; i++)
                        {
                            ctrlCmds[i] = (short)status[i];
                            LoggerManager.Debug($"WriteCSTCtrlCommand({index}, {ctrl}): Status of {i} = [{status[i]}]");
                        }
                        PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CSTCtrlCommand.Handle, ctrlCmds);

                        LoggerManager.Debug($"WriteCSTCtrlComand(): CST Index = {index}, Command = {ctrl}, Status = {status[index]}");
                        int readCount = 0;
                        int maxCount = 10;
                        do
                        {
                            Thread.Sleep(100);
                            status = (int[])PLCModule.tcClient.ReadAny(SymbolMap.OutputSymbol.CSTCtrlCommand.Handle,
                                                        typeof(int[]),
                                                        new int[] { SystemModuleCount.ModuleCnt.FoupCount });
                            readCount++;
                            if (readCount > maxCount) break;
                        } while (status[index] != ctrlCmds[index]);

                        LoggerManager.Debug($"WriteCSTCtrlComand() Done: CST Index = {index}, Command = {ctrl}");
                        ret = EventCodeEnum.NONE;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"WriteCSTCtrlCommand(): Error occurred. Err = {err.Message}");
            }
            return ret;
        }
        /// <summary>
        /// Use integer type array
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="size"></param>
        private int[] tcReadArray(int handle, int size)
        {
            int[] retData = new int[size];
            int dataSize = size * 2;
            AdsStream adsStream = new AdsStream(dataSize);
            try
            {
                var readResult = PLCModule.tcClient.Read(handle, adsStream);
                var rcvdData = adsStream.ToArray();
                //retData = rcvdData.Select(x => (int)x).ToArray();
                for (var index = 0; index < size; index++)
                {
                    retData[index] = BitConverter.ToInt16(rcvdData, index * 2);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"tcReadArray(): Handle = {handle}. Error occurred. Err = {err.Message}");
            }
            return retData;
        }
        private bool[] tcReadBoolArray(int handle, int size)
        {
            bool[] retData = new bool[size];
            int dataSize = size;
            AdsStream adsStream = new AdsStream(dataSize);
            try
            {
                var readResult = PLCModule.tcClient.Read(handle, adsStream);
                if(readResult != size)
                {
                    LoggerManager.Debug($"tcReadBoolArray(): handle = {handle}, Size = {readResult}, Data size Error.");
                }
                var rcvdData = adsStream.ToArray();
                for (var index = 0; index < size; index++)
                {
                    retData[index] = BitConverter.ToBoolean(rcvdData, index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"tcReadArray(): Handle = {handle}. Error occurred. Err = {err.Message}");
            }
            return retData;
        }
        /// <summary>
        /// Use integer type array
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="size"></param>
        private int[] tcReadSymbolArray(ADSSymbol symbol, int size)
        {
            int[] retData = new int[size];
            
            try
            {
                int dataSize = size * 2;
                switch (symbol.DataType)
                {
                    case EnumDataType.Type_Undefined:
                        break;
                    case EnumDataType.BOOL:
                        dataSize = size * 1;
                        break;
                    case EnumDataType.BYTE:
                        dataSize = size * 1;
                        break;
                    case EnumDataType.WORD:
                        dataSize = size * 2;
                        break;
                    case EnumDataType.DWORD:
                        dataSize = size * 4;
                        break;
                    case EnumDataType.SINT:
                        dataSize = size * 1;
                        break;
                    case EnumDataType.INT:
                        dataSize = size * 1;
                        break;
                    case EnumDataType.DINT:
                        dataSize = size * 4;
                        break;
                    case EnumDataType.USINT:
                        dataSize = size * 2;
                        break;
                    case EnumDataType.UINT:
                        dataSize = size * 2;
                        break;
                    case EnumDataType.UDINT:
                        dataSize = size * 4;
                        break;
                    case EnumDataType.REAL:
                        dataSize = size * 4;
                        break;
                    case EnumDataType.LREAL:
                        dataSize = size * 8;
                        break;
                    default:
                        dataSize = size * 2;
                        break;
                }
                
                AdsStream adsStream = new AdsStream(dataSize);
                var readResult = PLCModule.tcClient.Read(symbol.Handle, adsStream);
                var rcvdData = adsStream.ToArray();

                switch (symbol.DataType)
                {
                    case EnumDataType.Type_Undefined:
                        break;
                    case EnumDataType.BOOL:
                        break;
                    case EnumDataType.BYTE:
                        break;
                    case EnumDataType.WORD:
                        break;
                    case EnumDataType.DWORD:
                        break;
                    case EnumDataType.SINT:
                        break;
                    case EnumDataType.INT:
                        for (var index = 0; index < size; index++)
                        {
                            var shortData = BitConverter.ToInt16(rcvdData, index * 2);
                            retData[index] = shortData;
                        }
                        break;
                    case EnumDataType.DINT:
                        long intData = 0;
                        for (var index = 0; index < size; index++)
                        {
                            intData = BitConverter.ToUInt32(rcvdData, index * 4);
                            retData[index] = (int)intData;
                        }
                        break;
                    case EnumDataType.USINT:
                        break;
                    case EnumDataType.UINT:
                        break;
                    case EnumDataType.UDINT:
                        for (var index = 0; index < size; index++)
                        {
                            retData[index] = (int)BitConverter.ToUInt32(rcvdData, index * 4);
                        }
                        break;
                    case EnumDataType.REAL:
                        break;
                    case EnumDataType.LREAL:
                        break;
                    default:
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"tcReadArray(): Handle = {symbol}. Error occurred. Err = {err.Message}");
            }
            return retData;
        }
        public EventCodeEnum WaitForCSTStatus(int index, EnumCSTCtrl cstState, long timeout = 0)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            Stopwatch elapsedStopWatch = new Stopwatch();
            try
            {
                elapsedStopWatch.Reset();
                elapsedStopWatch.Start();
                bool commandDone = false;

                bool runFlag = true;
                do
                {
                    if (ReadCassetteCtrlState(index) == cstState)
                    {
                        commandDone = true;
                    }
                    if (cstState == EnumCSTCtrl.RESET)
                    {
                        if (ReadCassetteCtrlState(index) == EnumCSTCtrl.NONE)
                        {
                            commandDone = true;
                        }
                    }
                    if (ReadCassetteCtrlState(index) == EnumCSTCtrl.ERROR)
                    {
                        runFlag = false;
                        LoggerManager.Debug($"WaitForCSTStatus(): Error occurred. Target state = {cstState}, Timeout = {timeout}");
                        LoggingFoupEvent(index);
                    }
                    if (elapsedStopWatch.ElapsedMilliseconds % 10000 == 0)
                    {
                        LoggerManager.Debug($"WaitforCSTStatus({index}, Target = {cstState}): Curr.state ={ReadCassetteCtrlState(index)}");
                    }
                    if (timeout != 0 && elapsedStopWatch.ElapsedMilliseconds > timeout)
                    {
                        runFlag = false;
                        LoggerManager.Debug($"WaitForCSTStatus(): Timeout occurred. Target state = {cstState}, Timeout = {timeout}");
                        errorCode = EventCodeEnum.LOADER_ROBOTCMD_TIMEOUT;
                        LoggingFoupEvent(index, true);
                    }
                    if (commandDone)
                    {
                        runFlag = false;
                        errorCode = EventCodeEnum.NONE;
                        //LoggingFoupEvent(index, errorCode, cstState, cstCmd);
                    }

                } while (runFlag);
                
            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaitForCSTStatus(): Exception occurred. Err = {err.Message}");
                throw;
            }
            finally
            {
                elapsedStopWatch.Stop();
            }
            return errorCode;
        }
        private void LoggingFoupEvent(int index, bool timeout = false)
        {
            try
            {
                if (SymbolMap.InputSymbol.EventLog_RingBuffer_Foup != null && SymbolMap.InputSymbol.EventLog_RingBuffer_Foup.Handle > 0)
                {
                    var status = tcReadArray(SymbolMap.OutputSymbol.CSTCtrlCommand.Handle, SystemModuleCount.ModuleCnt.FoupCount);
                    EnumCSTCtrl cstCmd = (EnumCSTCtrl)status[index];

                    if (cstCmd != 0)
                    {
                        CurIndex_Foup = GetFoupEventLogIndex(index);
                        int errorStep = -1;

                        IGPLoaderCSTCtrlCommand cstCommandObj = FoupCommandList.Where(x => x.CommandName == cstCmd && GetDeviceSize(index) == x.WaferSize).FirstOrDefault();

                        if (cstCommandObj.CommandSequence != null && cstCommandObj.CommandSequence.Count > 0)
                        {
                            // 1차 필터 : SetCommand() ~ 에러 발생 사이의 history만 필터링. index로 찾음
                            List<stProEventInfo> curCmdHistory = GetCurCmdHistory(index);

                            // 2차 필터 : SetCommand()로 내린 커맨드에 대한 필터링. ex) CST_PICK동작을 시켰을 때 에러가 발생하면 PLC쪽에서 CST_FAILED RobotState로 바뀜.
                            curCmdHistory = curCmdHistory.Where(x => x.RobotState == Convert.ToInt16(cstCmd)).ToList();
                            for (int i = curCmdHistory.Count - 1; i >= 0; i--)
                            {
                                // 3차 필터 : Maestro에 정의되어 있는 Step인지 필터링. ex) 정의되어 있지 않은 Step에 경우 또는 PLC쪽 스탭이 에러로 빠진 부분을 제외시켜주기 위함.
                                var val = cstCommandObj.CommandSequence.TryGetValue(curCmdHistory[i].EventCode, out List<SequenceBehavior> cstCmdStepListAll);
                                if (val)
                                {
                                    // 4차 필터 : PLC쪽에서 에러가 발생했을 때 자동으로 리커버리 하는 동작이 있는데 실제 에러가 난 Step은 아니기에 이를 제외하기 위한 필터링.
                                    if (!cstCmdStepListAll.FirstOrDefault().isRecoverySeq)
                                    {
                                        errorStep = curCmdHistory[i].EventCode;
                                        break;
                                    }
                                }
                                else
                                {
                                    // 0번 스탭 에러 발생
                                    // ex) RobotState = 1 (CST_PICK), EventCode = 1000, PrevEventCode = 아예 다른 동작에서 마지막으로 동작했던 Step
                                    // Foup쪽은 0번 Step으로 초기화되는 부분이 없어보임..
                                    errorStep = 0;
                                    break;
                                }
                            }

                            if (errorStep != -1)
                            {
                                // 로더 리커버리 메시지 창에 현재 발생한 에러 Step에 대한 시퀀스 리스트 띄우기.
                                var errorCmdStepList = cstCommandObj.CommandSequence[errorStep];

                                if (errorCmdStepList != null && errorCmdStepList.Count != 0)
                                {
                                    if (this.FoupOpModule() != null)
                                    {
                                        this.FoupOpModule().GetFoupController(index + 1).Service.FoupModule.ErrorDetails = "Error occured while ";
                                    }
                                    this.FoupOpModule().GetFoupController(index + 1).Service.FoupModule.ErrorDetails += string.Join(" and \n", errorCmdStepList.Select(step => step.SequenceDescription));
                                }

                                string SequenceState = string.Empty;

                                for (int i = 0; i < curCmdHistory.Count; i++)
                                {
                                    int step = curCmdHistory[i].PrevEventCode;
                                    if (i == 0)
                                    {
                                        // FOUP쪽은 0번 Step은 링버퍼에 남겨지지 않음.. 이전 동작에 대한 마지막 Step이 담겨있음..
                                        step = 0;
                                    }

                                    var val = cstCommandObj.CommandSequence.TryGetValue(step, out List<SequenceBehavior> cstCmdStepListAll);

                                    if (val)
                                    {
                                        // Foup쪽은 AutoRecovery 시퀀스 없음.
                                        if (cstCmdStepListAll.FirstOrDefault().isRecoverySeq)
                                        {
                                            SequenceState = "Success(Auto Recovery)";
                                        }
                                        else
                                        {
                                            if (step == errorStep)
                                            {
                                                SequenceState = "Failed";
                                            }
                                            else
                                            {
                                                SequenceState = "Success";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //undefined (PLC에서는 실제 발생한 Step이지만 Maestro Sequence Parameter에는 정의되지 않은 항목)
                                        LoggerManager.Debug($"[PLC] CSTCmd : {cstCmd}, Undefined Sequence, Step : {step} (null), EventLogIndex : {CurIndex_Foup}");
                                    }

                                    if (cstCmdStepListAll != null)
                                    {
                                        for (int j = 0; j < cstCmdStepListAll.Count; j++)
                                        {
                                            LoggerManager.Debug($"[PLC] CSTCmd : {cstCmd}, Sequence {SequenceState}, Step : {step} ({cstCmdStepListAll[j].SequenceDescription}), EventLogIndex : {CurIndex_Foup}");
                                        }
                                    }
                                }
                                if (timeout)
                                {
                                    var val = cstCommandObj.CommandSequence.TryGetValue(errorStep, out List<SequenceBehavior> cstCmdStepListAll);
                                    if (val)
                                    {
                                        SequenceState = "Failed(Timeout)";
                                        if (cstCmdStepListAll != null)
                                        {
                                            for (int j = 0; j < cstCmdStepListAll.Count; j++)
                                            {
                                                LoggerManager.Debug($"[PLC] CSTCmd : {cstCmd}, Sequence {SequenceState}, Step : {errorStep} ({cstCmdStepListAll[j].SequenceDescription}), EventLogIndex : {CurIndex_Foup}");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"[PLC] errorStep is invalid");
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void LoggingRobotEvent(EnumRobotState robotState, bool timeout = false)
        {
            try
            {
                if (SymbolMap.InputSymbol.EventLog_RingBuffer != null && SymbolMap.InputSymbol.EventLog_RingBuffer.Handle > 0)
                {
                    EnumRobotCommand robotCmd = (EnumRobotCommand)CDXOut.nRobotCmd;
                    if (robotCmd != EnumRobotCommand.IDLE)
                    {
                        CurIndex_Robot = GetEventLogIndex();
                        int errorStep = -1;

                        IGPLoaderRobotCommand robotCommandObj = RobotCommandList.Where(x => x.CommandName == robotCmd).FirstOrDefault();

                        if (robotCommandObj.CommandSequence != null && robotCommandObj.CommandSequence.Count > 0)
                        {
                            // 1차 필터 : SetCommand() ~ 에러 발생 사이의 history만 필터링. index로 찾음
                            List<stProEventInfo> curCmdHistory = GetCurCmdHistory();

                            // 2차 필터 : SetCommand()로 내린 커맨드에 대한 필터링. ex) CST_PICK동작을 시켰을 때 에러가 발생하면 PLC쪽에서 CST_FAILED RobotState로 바뀜.
                            curCmdHistory = curCmdHistory.Where(x => x.RobotState == Convert.ToInt16(robotState)).ToList();
                            for (int i = curCmdHistory.Count - 1; i >= 0; i--)
                            {
                                // 3차 필터 : Maestro에 정의되어 있는 Step인지 필터링. ex) 정의되어 있지 않은 Step에 경우 또는 PLC쪽 스탭이 에러로 빠진 부분을 제외시켜주기 위함.
                                var val = robotCommandObj.CommandSequence.TryGetValue(curCmdHistory[i].EventCode, out List<SequenceBehavior> robotCmdStepListAll);
                                if (val)
                                {
                                    // 4차 필터 : PLC쪽에서 에러가 발생했을 때 자동으로 리커버리 하는 동작이 있는데 실제 에러가 난 Step은 아니기에 이를 제외하기 위한 필터링.
                                    if (!robotCmdStepListAll.FirstOrDefault().isRecoverySeq)
                                    {
                                        errorStep = curCmdHistory[i].EventCode;
                                        break;
                                    }
                                }
                                else
                                {
                                    // 0번 스탭 에러 발생
                                    // ex) RobotState = 1 (CST_PICK), EventCode = 1000, PrevEventCode = 0
                                    if (curCmdHistory[i].PrevEventCode == 0)
                                    {
                                        errorStep = 0;
                                        break;
                                    }
                                }
                            }

                            if (errorStep != -1)
                            {
                                // 로더 리커버리 메시지 창에 현재 발생한 에러 Step에 대한 시퀀스 리스트 띄우기.
                                var errorCmdStepList = robotCommandObj.CommandSequence[errorStep];

                                if (errorCmdStepList != null && errorCmdStepList.Count != 0)
                                {
                                    this.loaderModule.ErrorDetails = string.Join(" and \n", errorCmdStepList.Select(step => step.SequenceDescription));
                                }

                                string SequenceState = string.Empty;

                                for (int i = 0; i < curCmdHistory.Count; i++)
                                {
                                    int step = curCmdHistory[i].PrevEventCode;

                                    var val = robotCommandObj.CommandSequence.TryGetValue(step, out List<SequenceBehavior> robotCmdStepListAll);
                                    if (val)
                                    {
                                        if (robotCmdStepListAll.FirstOrDefault().isRecoverySeq)
                                        {
                                            SequenceState = "Success(Auto Recovery)";
                                        }
                                        else
                                        {
                                            if (step == errorStep)
                                            {
                                                SequenceState = "Failed";
                                            }
                                            else
                                            {
                                                SequenceState = "Success";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //undefined (PLC에서는 실제 발생한 Step이지만 Maestro Sequence Parameter에는 정의되지 않은 항목)
                                        LoggerManager.Debug($"[PLC] Cmd : {robotCmd}, Undefined Sequence, Step : {step} (null), EventLogIndex : {CurIndex_Robot}");
                                    }

                                    if (robotCmdStepListAll != null)
                                    {
                                        for (int j = 0; j < robotCmdStepListAll.Count; j++)
                                        {
                                            LoggerManager.Debug($"[PLC] Cmd : {robotCmd}, Sequence {SequenceState}, Step : {step} ({robotCmdStepListAll[j].SequenceDescription}), EventLogIndex : {CurIndex_Robot}");
                                        }
                                    }
                                }
                                if (timeout)
                                {
                                    var val = robotCommandObj.CommandSequence.TryGetValue(errorStep, out List<SequenceBehavior> robotCmdStepListAll);
                                    if (val)
                                    {
                                        SequenceState = "Failed(Timeout)";
                                        if (robotCmdStepListAll != null)
                                        {
                                            for (int j = 0; j < robotCmdStepListAll.Count; j++)
                                            {
                                                LoggerManager.Debug($"[PLC] Cmd : {robotCmd}, Sequence {SequenceState}, Step : {errorStep} ({robotCmdStepListAll[j].SequenceDescription}), EventLogIndex : {CurIndex_Robot}");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"[PLC] errorStep is invalid");
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum LockCassette(int index)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {                
                SetLoaderMode(EnumLoaderMode.ACTIVE);
                var result = WaitForMode(EnumLoaderState.ACTIVE);
                if (result != EventCodeEnum.NONE)
                {
                    return result;
                }

                if (ReadCassetteCtrlState(index) != EnumCSTCtrl.NONE)
                {
                    result = WriteCSTCtrlCommand(index, EnumCSTCtrl.NONE);
                    if (result != EventCodeEnum.NONE)
                    {
                        return result;
                    }
                }
                result = WriteCSTCtrlCommand(index, EnumCSTCtrl.CSTLOCK);
                if (result != EventCodeEnum.NONE)
                {
                    return result;
                }

                result = WaitForCSTStatus(index, EnumCSTCtrl.CSTLOCKED, DefaultJobTimeout);
                if (result != EventCodeEnum.NONE)
                {
                    return result;
                }
                
                var size = GetDeviceSize(index);
                
                this.loaderModule.SetCassetteDeviceSize(size, index + 1);

                errorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return errorCode;
        }
        public SubstrateSizeEnum GetDeviceSize(int index)
        {
            SubstrateSizeEnum retVal = SubstrateSizeEnum.UNDEFINED;
            try
            {
                if (SymbolMap.InputSymbol.LockedCST.Handle == 0 ||
                    SymbolMap.InputSymbol.LockedCST.Handle == -1)
                {
                    retVal = SubstrateSizeEnum.INCH12;
                }
                else
                {
                    short[] lockedStatus = new short[] { 0, 0, 0 };
                    lockedStatus = (short[])PLCModule.tcClient.ReadAny(
                        SymbolMap.InputSymbol.LockedCST.Handle, typeof(short[]), new int[] { SystemModuleCount.ModuleCnt.FoupCount });
                    var cstType = (LockedCSTTypeEnum)lockedStatus[index];

                    switch (cstType)
                    {
                        case LockedCSTTypeEnum.INVALID:
                            retVal = SubstrateSizeEnum.INVALID;
                            break;
                        case LockedCSTTypeEnum.UNDEFINED:
                            retVal = SubstrateSizeEnum.UNDEFINED;
                            break;
                        case LockedCSTTypeEnum.INCH12:
                            retVal = SubstrateSizeEnum.INCH12;
                            break;
                        case LockedCSTTypeEnum.INCH08:
                            retVal = SubstrateSizeEnum.INCH8;
                            break;
                        case LockedCSTTypeEnum.INCH06:
                            retVal = SubstrateSizeEnum.INCH6;
                            break;
                        default:
                            break;
                    }
                }

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL && loaderModule.GetDefaultWaferSize() != SubstrateSizeEnum.INCH12 && (retVal == SubstrateSizeEnum.INCH12 || retVal == SubstrateSizeEnum.UNDEFINED || retVal == SubstrateSizeEnum.INVALID)) 
                {
                    retVal = loaderModule.GetDefaultWaferSize();
                }

                if (retVal == SubstrateSizeEnum.INCH6 || retVal == SubstrateSizeEnum.INCH8 || retVal == SubstrateSizeEnum.INCH12)
                {
                    ICassetteModule cst = loaderModule.ModuleManager.FindModule(ModuleTypeEnum.CST, index + 1) as ICassetteModule;

                    if (cst != null)
                    {
                        cst.Device.AllocateDeviceInfo.Size.Value = retVal;
                        // Size에 따라서 NotchType이 변경되는 코드 
                        //if (retVal == SubstrateSizeEnum.INCH6)
                        //{
                        //    cst.Device.AllocateDeviceInfo.NotchType = WaferNotchTypeEnum.FLAT;
                        //}
                        //else
                        //{
                        //    cst.Device.AllocateDeviceInfo.NotchType = WaferNotchTypeEnum.NOTCH;
                        //}
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"LockCassette error. Cassette type not recognized. Err = {err.Message}");
            }
            return retVal;
        }
        public EventCodeEnum UnLockCassette(int index)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            SetLoaderMode(EnumLoaderMode.ACTIVE);
            var result = WaitForMode(EnumLoaderState.ACTIVE);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }

            if (ReadCassetteCtrlState(index) != EnumCSTCtrl.NONE)
            {
                result = WriteCSTCtrlCommand(index, EnumCSTCtrl.NONE);
                if (result != EventCodeEnum.NONE)
                {
                    return result;
                }
                //result = WaitForCSTStatus(index, EnumCSTCtrl.NONE, DefaultJobTimeout);
                //if (result != EventCodeEnum.NONE)
                //{
                //    return result;
                //}
            }
            result = WriteCSTCtrlCommand(index, EnumCSTCtrl.CSTUNLOCK);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            result = WaitForCSTStatus(index, EnumCSTCtrl.CSTUNLOCKED, DefaultJobTimeout);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }

            errorCode = EventCodeEnum.NONE;
            return errorCode;
        }

        public EventCodeEnum DockingPortIn(int index)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            SetLoaderMode(EnumLoaderMode.ACTIVE);
            var result = WaitForMode(EnumLoaderState.ACTIVE);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }

            if (ReadCassetteCtrlState(index) != EnumCSTCtrl.NONE)
            {
                result = WriteCSTCtrlCommand(index, EnumCSTCtrl.NONE);
                if (result != EventCodeEnum.NONE)
                {
                    return result;
                }
                //result = WaitForCSTStatus(index, EnumCSTCtrl.NONE, DefaultJobTimeout);
                //if (result != EventCodeEnum.NONE)
                //{
                //    return result;
                //}
            }
            result = WriteCSTCtrlCommand(index, EnumCSTCtrl.DPIN);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            result = WaitForCSTStatus(index, EnumCSTCtrl.DPINMoveDone, DefaultJobTimeout);
            if (result != EventCodeEnum.NONE)
            {
                EnumCSTCtrl cstctrlstate = ReadCassetteCtrlState(index);
                LoggerManager.RecoveryLog($"BehaviorError,Foup Ctrl State : {cstctrlstate}", true);
                return result;
            }
            errorCode = EventCodeEnum.NONE;
            return errorCode;
        }

        public EventCodeEnum DockingPortOut(int index)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            SetLoaderMode(EnumLoaderMode.ACTIVE);
            var result = WaitForMode(EnumLoaderState.ACTIVE);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }

            if (ReadCassetteCtrlState(index) != EnumCSTCtrl.NONE)
            {
                result = WriteCSTCtrlCommand(index, EnumCSTCtrl.NONE);
                if (result != EventCodeEnum.NONE)
                {
                    return result;
                }
                //result = WaitForCSTStatus(index, EnumCSTCtrl.NONE, DefaultJobTimeout);
                //if (result != EventCodeEnum.NONE)
                //{
                //    return result;
                //}
            }
            result = WriteCSTCtrlCommand(index, EnumCSTCtrl.DPOUT);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            result = WaitForCSTStatus(index, EnumCSTCtrl.DPOUTMoveDone, DefaultJobTimeout);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            errorCode = EventCodeEnum.NONE;
            return errorCode;
        }

        public EventCodeEnum CoverLock(int index)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            SetLoaderMode(EnumLoaderMode.ACTIVE);
            var result = WaitForMode(EnumLoaderState.ACTIVE);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }

            if (ReadCassetteCtrlState(index) != EnumCSTCtrl.NONE)
            {
                result = WriteCSTCtrlCommand(index, EnumCSTCtrl.NONE);
                if (result != EventCodeEnum.NONE)
                {
                    return result;
                }
                //result = WaitForCSTStatus(index, EnumCSTCtrl.NONE, DefaultJobTimeout);
                //if (result != EventCodeEnum.NONE)
                //{
                //    return result;
                //}
            }
            result = WriteCSTCtrlCommand(index, EnumCSTCtrl.COVERLOCK);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            result = WaitForCSTStatus(index, EnumCSTCtrl.COVERLOCKED, DefaultJobTimeout);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            errorCode = EventCodeEnum.NONE;
            return errorCode;
        }

        public EventCodeEnum CoverUnLock(int index)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            SetLoaderMode(EnumLoaderMode.ACTIVE);
            var result = WaitForMode(EnumLoaderState.ACTIVE);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }

            if (ReadCassetteCtrlState(index) != EnumCSTCtrl.NONE)
            {
                result = WriteCSTCtrlCommand(index, EnumCSTCtrl.NONE);
                if (result != EventCodeEnum.NONE)
                {
                    return result;
                }
                //result = WaitForCSTStatus(index, EnumCSTCtrl.NONE, DefaultJobTimeout);
                //if (result != EventCodeEnum.NONE)
                //{
                //    return result;
                //}
            }
            result = WriteCSTCtrlCommand(index, EnumCSTCtrl.COVERUNLOCK);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            result = WaitForCSTStatus(index, EnumCSTCtrl.COVERUNLOCKED, DefaultJobTimeout);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            errorCode = EventCodeEnum.NONE;
            return errorCode;
        }

        public EventCodeEnum CoverOpen(int index)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            SetLoaderMode(EnumLoaderMode.ACTIVE);
            var result = WaitForMode(EnumLoaderState.ACTIVE);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }

            result = CheckArmCassetteInterference(index);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }

            if (ReadCassetteCtrlState(index) != EnumCSTCtrl.NONE)
            {
                result = WriteCSTCtrlCommand(index, EnumCSTCtrl.NONE);
                if (result != EventCodeEnum.NONE)
                {
                    return result;
                }
                //result = WaitForCSTStatus(index, EnumCSTCtrl.NONE, DefaultJobTimeout);
                //if (result != EventCodeEnum.NONE)
                //{
                //    return result;
                //}
            }
            result = WriteCSTCtrlCommand(index, EnumCSTCtrl.COVEROPEN);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            result = WaitForCSTStatus(index, EnumCSTCtrl.COVEROPENED, DefaultJobTimeout);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            errorCode = EventCodeEnum.NONE;
            return errorCode;
        }

        public EventCodeEnum CoverClose(int index)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            SetLoaderMode(EnumLoaderMode.ACTIVE);
            var result = WaitForMode(EnumLoaderState.ACTIVE);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }

            result = CheckArmCassetteInterference(index);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }

            if (ReadCassetteCtrlState(index) != EnumCSTCtrl.NONE)
            {
                result = WriteCSTCtrlCommand(index, EnumCSTCtrl.NONE);
                if (result != EventCodeEnum.NONE)
                {
                    return result;
                }
                //result = WaitForCSTStatus(index, EnumCSTCtrl.NONE, DefaultJobTimeout);
                //if (result != EventCodeEnum.NONE)
                //{
                //    return result;
                //}
            }
            result = WriteCSTCtrlCommand(index, EnumCSTCtrl.NONE);
            if (result != EventCodeEnum.NONE)
            {
            }


            result = WriteCSTCtrlCommand(index, EnumCSTCtrl.COVERCLOSE);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            result = WaitForCSTStatus(index, EnumCSTCtrl.COVERCLOSED, DefaultJobTimeout * 2);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            errorCode = EventCodeEnum.NONE;
            return errorCode;
        }

        public EventCodeEnum FOUPReset(int index)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            int delaySec = 500;

            SetLoaderMode(EnumLoaderMode.ACTIVE);
            var result = WaitForMode(EnumLoaderState.ACTIVE);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            result = WriteCSTCtrlCommand(index, EnumCSTCtrl.RESET);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            System.Threading.Thread.Sleep(delaySec);
            //result = WaitForCSTStatus(index, EnumCSTCtrl.NONE, DefaultJobTimeout);
            //if (result != EventCodeEnum.NONE)
            //{
            //    return result;
            //}
            //추가된 코드
            result = WriteCSTCtrlCommand(index, EnumCSTCtrl.NONE);
            if (result != EventCodeEnum.NONE)
            {
                return result;
            }
            System.Threading.Thread.Sleep(delaySec);
            //result = WaitForCSTStatus(index, EnumCSTCtrl.NONE, DefaultJobTimeout);
            //if (result != EventCodeEnum.NONE)
            //{
            //    return result;
            //}
            //
            errorCode = EventCodeEnum.NONE;
            return errorCode;
        }

        public EventCodeEnum PAMove(IPreAlignModule pa, double x, double y, double angle = 0)
        {
            LoggerManager.Debug($"PAMove(): Source = {pa.ID.Label}. Position X = {x}, Y = {y}, Angle = {angle}");
            return loaderModule.PAManager.PAModules[pa.ID.Index - 1].MoveTo(x, y, angle);
        }

        public EventCodeEnum PAMove(IPreAlignModule pa, double angle)
        {
            return loaderModule.PAManager.PAModules[pa.ID.Index - 1].MoveTo(angle);
        }

        public void RaiseFoupModuleStateChanged(FoupModuleInfo moduleInfo)
        {
            if (this.loaderModule != null)
            {
                this.loaderModule.FOUP_RaiseFoupStateChanged(moduleInfo);
            }
        }

        public EventCodeEnum DRWPick(IInspectionTrayModule drw, IARMModule arm)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                var drwIndex = drw.ID.Index;

                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    lock (plcLockObject)
                    {
                        CDXOut.nDRWPos = (short)(drwIndex);
                        CDXOut.nArmIndex = (ushort)armIndex;
                        WriteCDXOut();
                    }
                    LoggerManager.Debug($"DRWPick(): Pick wafer from DRW#{drwIndex}  with {CDXOut.nArmIndex} Arm");

                    errorCode = SetRobotCommand(EnumRobotCommand.DRW_PICK);
                     
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.CSTDRW_PICKED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"DRWPick(): DRW Pick Error {errorCode}.";
                LoggerManager.Error($"DRWPick(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }

        public EventCodeEnum DRWPut(IARMModule arm, IInspectionTrayModule drw)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {

                long timeOut = DefaultJobTimeout;
                var armIndex = arm.ID.Index;
                var drwIndex = drw.ID.Index;

                SetRobotCommand(EnumRobotCommand.IDLE);
                WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                if (IsControllerInIdle() == true)
                {
                    lock (plcLockObject)
                    {
                        CDXOut.nDRWPos = (short)(drwIndex);
                        CDXOut.nArmIndex = (ushort)armIndex;
                        WriteCDXOut();
                    }
                    LoggerManager.Debug($"DRWPut(): Put wafer to DRW#{drwIndex}  with {CDXOut.nArmIndex} Arm");

                    errorCode = SetRobotCommand(EnumRobotCommand.DRW_PUT);

                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                    errorCode = WaitForCommandDone(EnumRobotState.CSTDRW_PUTED, timeOut);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new ProberSystemException(errorCode);
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"DRWPut(): DRW Put Error {errorCode}.";
                LoggerManager.Error($"DRWPut(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }

        object utilBoxLockObject = new object();
        public EventCodeEnum ValveControl(EnumValveType valveType, int index, bool state)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                lock (utilBoxLockObject)
                {
                    switch (valveType)
                    {
                        case EnumValveType.IN:
                            retval = HandleValve(SymbolMap.OutputSymbol.CoolantInletCommand.Handle, CoolantInletValveStates, index, state, "Coolant Inlet");
                            break;

                        case EnumValveType.OUT:
                            retval = HandleValve(SymbolMap.OutputSymbol.CoolantOutletCommand.Handle, CoolantOutletValveStates, index, state, "Coolant Outlet");
                            break;

                        case EnumValveType.PURGE:
                            retval = HandleValve(SymbolMap.OutputSymbol.CoolantPurgeCommand.Handle, PurgeValveStates, index, state, "Coolant Purge");
                            break;

                        case EnumValveType.DRAIN:
                            retval = HandleValve(SymbolMap.OutputSymbol.CoolantDrainCommand.Handle, DrainValveStates, index, state, "Coolant Drain");
                            break;

                        case EnumValveType.DRYAIR:
                            retval = HandleValve(SymbolMap.OutputSymbol.DryAirCommand.Handle, DryAirValveStates, index, state, "Dry Air");
                            break;
                        default:
                            LoggerManager.Error($"Invalid valve type: {valveType}");
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        private EventCodeEnum HandleValve(int handle, ObservableCollection<bool> valveStates, int index, bool state, string valveName)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (handle == 0)
                {
                    return retval;
                }

                bool[] vals = (bool[])PLCModule.tcClient.ReadAny(handle, typeof(bool[]), new int[] { GPLoaderDef.StageCount });

                for (int i = 0; i < vals.Count(); i++)
                {
                    valveStates[i] = vals[i];
                }

                valveStates[index - 1] = state;

                bool[] valve = valveStates.ToArray<bool>();
                PLCModule.tcClient.WriteAny(handle, valve);
                var valveState = state ? "Opened" : "Closed";

                LoggerManager.Debug($"{valveName} Valve #{index} Set to {valveState} state");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        Dictionary<string, int> IOSymbolCountDic = new Dictionary<string, int>();
        public EventCodeEnum WriteIO(IOPortDescripter<bool> io, bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                string iOName = "";
                int index = -1;
                int maxCnt =-1;
                var splitStr=io.Key.Value.Split('.');
                if (splitStr.Length == 1)
                {
                    iOName = splitStr[0];
                    maxCnt = 0;
                    index = 0;
                }
                else if(splitStr.Length == 2)
                {
                    iOName = splitStr[0];
                    index = Int32.Parse(splitStr[1]);
                }
               

                var symbolMap =IOSymbolMap.IOPortToSymbolList.Where(i => i.IOPortKey.Contains(iOName)).FirstOrDefault();
                if (symbolMap != null)
                {
                    if (IOSymbolCountDic.Count == 0)
                    {
                        BindingFlags _BindFlags = BindingFlags.Public | BindingFlags.Instance;

                        foreach (PropertyInfo propInfo in this.GetFoupIO().IOMap.Outputs.GetType().GetProperties(_BindFlags))
                        {
                            Object paramValue = propInfo.GetValue(this.GetFoupIO().IOMap.Outputs);
                            if (paramValue == null)
                                continue;
                            if (paramValue is IList)
                            {
                                int cnt = (paramValue as IList).Count;
                                if (cnt > 0)
                                {
                                    IOSymbolCountDic.Add(propInfo.Name, cnt);
                                }
                            }
                        }
                        foreach (PropertyInfo propInfo in this.IOManager().IO.RemoteOutputs.GetType().GetProperties(_BindFlags))
                        {
                            Object paramValue = propInfo.GetValue(this.IOManager().IO.RemoteOutputs);
                            if (paramValue == null)
                                continue;
                            if (paramValue is IList)
                            {
                                int cnt = (paramValue as IList).Count;
                                if (cnt > 0)
                                {
                                    if (!IOSymbolCountDic.ContainsKey(propInfo.Name))
                                    {
                                        IOSymbolCountDic.Add(propInfo.Name, cnt);
                                    }
                                }
                            }
                        }

                    }
                    if (maxCnt == -1)
                    {
                        maxCnt = IOSymbolCountDic[iOName];
                    }
                    var symBol =  SymbolMap.GetSymbols().Where(i => i.SymbolName == symbolMap.SymbolName).FirstOrDefault();
                    if (symBol != null)
                    {
                        if (symBol.Handle != 0)
                        {
                            if (symBol.VariableType != EnumVariableType.VAR)
                            {
                                bool[] vals = (bool[])PLCModule.tcClient.ReadAny(symBol.Handle, typeof(bool[]), new int[] { maxCnt });
                                
                                
                                vals[index] = value;
                                
                                PLCModule.tcClient.WriteAny(symBol.Handle, vals);
                            }
                            else 
                            {
                                PLCModule.tcClient.WriteAny(symBol.Handle, value);
                            }
                        }
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum CheckIOValue(IOPortDescripter<bool> io)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                string iOName = "";
                int index = -1;
                int maxCnt = -1;
                var splitStr = io.Key.Value.Split('.');
                if (splitStr.Length == 1)
                {
                    iOName = splitStr[0];
                    maxCnt = 0;
                    index = 0;
                }
                else if (splitStr.Length == 2)
                {
                    iOName = splitStr[0];
                    index = Int32.Parse(splitStr[1]);
                }

                var symbolMap = IOSymbolMap.IOPortToSymbolList.Where(i => i.IOPortKey.Contains(iOName)).FirstOrDefault();
                if (symbolMap != null)
                {
                    if (IOSymbolCountDic.Count == 0)
                    {
                        BindingFlags _BindFlags = BindingFlags.Public | BindingFlags.Instance;

                        foreach (PropertyInfo propInfo in this.GetFoupIO().IOMap.Outputs.GetType().GetProperties(_BindFlags))
                        {
                            Object paramValue = propInfo.GetValue(this.GetFoupIO().IOMap.Outputs);
                            if (paramValue == null)
                                continue;
                            if (paramValue is IList)
                            {
                                int cnt = (paramValue as IList).Count;
                                if (cnt > 0)
                                {
                                    IOSymbolCountDic.Add(propInfo.Name, cnt);
                                }
                            }
                        }
                        foreach (PropertyInfo propInfo in this.IOManager().IO.RemoteOutputs.GetType().GetProperties(_BindFlags))
                        {
                            Object paramValue = propInfo.GetValue(this.IOManager().IO.RemoteOutputs);
                            if (paramValue == null)
                                continue;
                            if (paramValue is IList)
                            {
                                int cnt = (paramValue as IList).Count;
                                if (cnt > 0)
                                {
                                    if (!IOSymbolCountDic.ContainsKey(propInfo.Name)) 
                                    {
                                        IOSymbolCountDic.Add(propInfo.Name, cnt);
                                    }
                                }
                            }
                        }

                    }
                    if (maxCnt == -1)
                    {
                        maxCnt = IOSymbolCountDic[iOName];
                    }
                    var symBol = SymbolMap.GetSymbols().Where(i => i.SymbolName == symbolMap.SymbolName).FirstOrDefault();
                    if (symBol != null)
                    {
                        bool[] vals = (bool[])PLCModule.tcClient.ReadAny(symBol.Handle, typeof(bool[]), new int[] { maxCnt });
                        io.Value = vals[index];
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion
        public EnumCSTState GetCSTState(int cstidx)
        {
            EnumCSTState cstState = EnumCSTState.IDLE;
            try
            {
                //GetCSTState(cstIdx): Obsolated
                if (SymbolMap.InputSymbol.CSTState != null)
                {
                    if (SymbolMap.InputSymbol.CSTState.Handle > 0)
                    {
                        //var csts = (short[])PLCModule.tcClient.ReadAny(SymbolMap.InputSymbol.CSTState.Handle,
                        //            typeof(short[]), new int[] { SystemModuleCount.ModuleCnt.FoupCount });
                        var csts = tcReadArray(SymbolMap.InputSymbol.CSTState.Handle, SystemModuleCount.ModuleCnt.FoupCount);
                        cstState = (EnumCSTState)csts[cstidx];
                    }
                    else
                    {
                        if (this.GetGPLoader().CDXIn.nCSTState != null) //emul 환경에서의 예외처리 추가
                        {
                            cstState = (EnumCSTState)this.GetGPLoader().CDXIn.nCSTState[cstidx];
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"GetCSTState({cstidx}): Error occurred. Err = {err.Message} ");
            }
            return cstState;
        }

        public uint GetScanCount(int foupidx)
        {
            uint waferCount = 0;
            try
            {
                //GetCSTState(cstIdx): Obsolated
                if (SymbolMap.InputSymbol.CSTWaferCount != null)
                {
                    if (SymbolMap.InputSymbol.CSTWaferCount.Handle > 0)
                    {
                        var csts = (uint[])PLCModule.tcClient.ReadAny(SymbolMap.InputSymbol.CSTWaferCount.Handle,
                                    typeof(uint[]), new int[] { SystemModuleCount.ModuleCnt.FoupCount });
                        //var csts = tcReadArray(SymbolMap.InputSymbol.CSTWaferCount.Handle, SystemModuleCount.ModuleCnt.FoupCount);
                        waferCount = (uint)csts[foupidx];
                    }
                    else
                    {
                        waferCount = this.GetGPLoader().CDXIn.nCSTWafer_Cnt[foupidx];
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"GetScanCount({foupidx}): Error occurred. Err = {err.Message} ");
            }
            return waferCount;
        }

        public short[] GetWaferInfos(int foupidx)
        {
            short[] waferInfos = null;
            try
            {
                if (SymbolMap.InputSymbol.CSTWaferCount != null)
                {
                    if (SymbolMap.InputSymbol.CSTWaferCount.Handle > 0)
                    {
                        if(SymbolMap.InputSymbol.WaferInfos != null && SymbolMap.InputSymbol.WaferInfos.Handle > 0)
                        {
                            var cstInfos = (stWaferInfos)PLCModule.tcClient.ReadAny(SymbolMap.InputSymbol.WaferInfos.Handle, typeof(stWaferInfos));
                            waferInfos = cstInfos.WaferInfos;
                        }
                    }
                    else
                    {
                        //foupidx 값은 array index값이므로 인자로 넘길시 -1을 해주어야 한다.
                        if (foupidx == 0)
                        {
                            waferInfos = this.GetGPLoader().CDXIn.nFOUP1_WaferInfos;
                        }
                        else if (foupidx == 1)
                        {
                            waferInfos = this.GetGPLoader().CDXIn.nFOUP2_WaferInfos;
                        }
                        else if (foupidx == 2)
                        {
                            waferInfos = this.GetGPLoader().CDXIn.nFOUP3_WaferInfos;
                        }
                        else if (foupidx == 3)
                        {
                            for (int i = 0; i < SystemModuleCount.ModuleCnt.SlotCount; i++)
                            {
                                waferInfos[i] = -2;
                            }
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"GetWaferInfos({foupidx}): Error occurred. Err = {err.Message} ");
            }
            return waferInfos;
        }

        private EventCodeEnum CheckArmCassetteInterference(int index)
        {
            EventCodeEnum errorCode = EventCodeEnum.NONE;
            try
            {
                if (LUD == null || LUU == null || LCC == null || LX == null || LZ == null || LW == null)
                {
                    LUD = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LUD);
                    LUU = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LUU);
                    LCC = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LCC);
                    LX = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LX);
                    LZ = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LZM);
                    LW = loaderModule.MotionManager.GetAxis(EnumAxisConstants.LW);
                }

                if (loaderModule.SystemParameter.Cassette_Arm_LX_Interference.Value <= 0)
                {
                    loaderModule.SystemParameter.Cassette_Arm_LX_Interference.Value = 5000;
                }
                if (loaderModule.SystemParameter.Cassette_Arm_LZ_Interference.Value <= 0)
                {
                    loaderModule.SystemParameter.Cassette_Arm_LZ_Interference.Value = 5000;
                }
                if (loaderModule.SystemParameter.Cassette_Arm_LW_Interference.Value <= 0)
                {
                    loaderModule.SystemParameter.Cassette_Arm_LW_Interference.Value = 50000;
                }
                if (loaderModule.SystemParameter.Cassette_Arm_LU_Interference.Value <= 0)
                {
                    loaderModule.SystemParameter.Cassette_Arm_LU_Interference.Value = 1000;
                }


                if (LUD != null && LUU != null && LCC != null && LX != null && LZ != null && LW != null)
                {
                    if (Math.Abs(CSTAccParams[index].nLX_Pos - LX.Status.Position.Actual) < loaderModule.SystemParameter.Cassette_Arm_LX_Interference.Value &&
                       (CSTAccParams[index].nLZ_Pos - loaderModule.SystemParameter.Cassette_Arm_LZ_Interference.Value < LZ.Status.Position.Actual) &&
                       (CSTAccParams[index].nLZ_Pos + FOUPParams[index].UpPos > LZ.Status.Position.Actual) &&
                       (CSTAccParams[index].nLW_Pos - LW.Status.Position.Actual) < loaderModule.SystemParameter.Cassette_Arm_LW_Interference.Value &&
                       ((LCC.Status.Position.Actual - LCC.Param.ClearedPosition.Value) > loaderModule.SystemParameter.Cassette_Arm_LU_Interference.Value ||
                       (LUU.Status.Position.Actual - LUU.Param.ClearedPosition.Value) > loaderModule.SystemParameter.Cassette_Arm_LU_Interference.Value ||
                       (LUD.Status.Position.Actual - LUD.Param.ClearedPosition.Value) > loaderModule.SystemParameter.Cassette_Arm_LU_Interference.Value))
                    {
                        errorCode = EventCodeEnum.ARM_DANGEROUS_POS;
                        this.MetroDialogManager().ShowMessageDialog($"Arm Position Warning.", $"Check ARM Position [ LUD: {LUD.Status.Position.Actual} , LUU: {LUU.Status.Position.Actual} , LCC: {LCC.Status.Position.Actual} ]", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        errorCode = EventCodeEnum.NONE;
                    }
                }
            }
            catch (Exception err)
            {
                loaderModule.ResonOfError = $"CheckArmCassetteInterference(): Error {errorCode}.";
                LoggerManager.Error($"CheckArmCassetteInterference(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }


        public EventCodeEnum LockRobot()
        {
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            try
            {
                if(SymbolMap.OutputSymbol.FreezeRobot.Handle > 0)
                {
                    LoggerManager.Debug($"LockRobot(): Loader robot Freeze.");
                    PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.FreezeRobot.Handle, true);
                    _LoaderRobotLockState = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCodeEnum;
        }

        public EventCodeEnum CardTrayLock(bool locktray)
        {
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            try
            {
                if (SymbolMap.OutputSymbol.CardTrayUnlock.Handle > 0)
                {
                    LoggerManager.Debug($"CardTrayLock({locktray}): Loader robot Card Tray Lock/Unlock. Lock = {locktray}");
                    PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.CardTrayUnlock.Handle, !locktray);
                    _LoaderRobotLockState = true;
                    eventCodeEnum = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"CardTrayLock({locktray}): Invalid Symbol.");
                    eventCodeEnum = EventCodeEnum.PARAM_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCodeEnum;
        }

        public EventCodeEnum UnlockRobot()
        {
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            try
            {
                
                if (SymbolMap.OutputSymbol.FreezeRobot.Handle > 0)
                {
                    LoggerManager.Debug($"LockRobot(): Loader robot release.");
                    PLCModule.tcClient.WriteAny(SymbolMap.OutputSymbol.FreezeRobot.Handle, false);
                    _CardTrayLockState = false;
                    eventCodeEnum = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"LockRobot(): Invalid Symbol. Symbol not defined in symbol map. Handle Value = {SymbolMap.OutputSymbol.FreezeRobot.Handle}");
                    eventCodeEnum = EventCodeEnum.PARAM_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                eventCodeEnum = EventCodeEnum.EXCEPTION;
            }
            return eventCodeEnum;
        }

        public EventCodeEnum SetTesterCoolantValve(int index, bool open)
        {
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            try
            {
                if (SymbolMap.OutputSymbol.PCWLeakStatus.Handle > 0)
                {
                    
                    var ctrlCmds = new bool[GPLoaderDef.StageCount];
                    bool[] status;

                    status = tcReadBoolArray(SymbolMap.OutputSymbol.PCWLeakStatus.Handle, GPLoaderDef.StageCount);

                    for (int i = 0; i < ctrlCmds.Length; i++)
                    {
                        ctrlCmds[i] = status[i];
                        //LoggerManager.Debug($"SetTesterCoolantValve({index}, {open}): Status of {i} = [{status[i]}]");
                    }
                    ctrlCmds[index] = !open;        // PCW Leak detect channel이므로 True일 경우 PCW valve가 닫힘. 따라서 Open flag의 reverse로 설정해야 함
                    PLCModule.tcClient.WriteAny(
                        SymbolMap.OutputSymbol.PCWLeakStatus.Handle,
                        ctrlCmds);
                    eventCodeEnum = EventCodeEnum.NONE;

                    status = tcReadBoolArray(SymbolMap.OutputSymbol.PCWLeakStatus.Handle, GPLoaderDef.StageCount);

                    for (int i = 0; i < ctrlCmds.Length; i++)
                    {
                        TesterCoolantValveOpened[i] = !status[i];   // PCW Leak detect channel이므로 True일 경우 PCW valve가 닫힘.
                        LoggerManager.Debug($"SetTesterCoolantValve({index}, {open}): Status of {i} = [{status[i]}]");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCodeEnum;
        }
        public EventCodeEnum GetTesterCoolantValveState(int index, out bool isopened)
        {
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            isopened = false;
            try
            {
                if (SymbolMap.OutputSymbol.PCWLeakStatus.Handle > 0)
                {

                    bool[] status;

                    status = tcReadBoolArray(SymbolMap.OutputSymbol.PCWLeakStatus.Handle, GPLoaderDef.StageCount);

                    for (int i = 0; i < status.Length; i++)
                    {
                        TesterCoolantValveOpened[i] = !status[i];   // PCW Leak detect channel이므로 True일 경우 PCW valve가 닫힘.
                        LoggerManager.Debug($"SetTesterCoolantValve(): Status of Tester #{i} Coolant Valve = [{status[i]}]");
                        if(i == index)
                        {
                            isopened = !status[i];
                        }
                    }
                    eventCodeEnum = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCodeEnum;
        }

        public IRFIDModule GetRFIDReaderForCard()
        {
            IRFIDModule ret = null;
            try
            {
                if(RFIDReader != null)
                {
                    ret = RFIDReader;
                }
                else
                {
                    LoggerManager.Debug($"GetRFIDReaderForCard() RFIDReader is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
    }
}
