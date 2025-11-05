using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ProberInterfaces;
using ProberInterfaces.Temperature.DryAir;
using ProberInterfaces.Command;
using ProberErrorCode;
using LogModule;
using ProberInterfaces.EnvControl.Parameter;
using SerializerUtil;

namespace Temperature.Temp.DryAir
{
    public class EmulDryAirModule : IDryAirModule
    {
        public bool Initialized { get; set; } = false;

        private IDryAirManager _Manager;
        public IDryAirManager Manager
        {
            get { return _Manager; }
            set { _Manager = value; }
        }
        public IDryAirController Processor { get; set; }

        private Dictionary<EnumDryAirType, bool> _DyrAirState
             = new Dictionary<EnumDryAirType, bool>();

        public Dictionary<EnumDryAirType, bool> DyrAirState
        {
            get { return _DyrAirState; }
            set { _DyrAirState = value; }
        }


        public EmulDryAirModule()
        {
            Init();
        }
        public EmulDryAirModule(DryAirSysParameter param, IDryAirManager manager)
        {
            DryAirSysParam = param;
            Manager = manager;
        }

        ~EmulDryAirModule()
        {
            Dispose();
        }
        bool isDisposed = false;

        private bool _bSupply_flag;
        public bool bSupply_flag
        {
            get { return _bSupply_flag; }
            set
            {
                if (value != _bSupply_flag)
                {
                    _bSupply_flag = value;
                    NotifyPropertyChanged("bSupply_flag");
                }
            }
        }
        private IParam _Parameter;
        public IParam Parameter
        {
            get { return _Parameter; }
            set
            {
                if (value != _Parameter)
                {
                    _Parameter = value;
                    NotifyPropertyChanged("Parameter");
                }
            }
        }

        private DryAirSysParameter _DryAirSysParam;
        public DryAirSysParameter DryAirSysParam
        {
            get { return _DryAirSysParam; }
            set { _DryAirSysParam = value; }
        }

        //private double _DryAirActivableHighTemp;

        public double DryAirActivableHighTemp
        {
            get { return DryAirSysParam?.ActivatableHighTemp?.Value ?? 0; }
            set { DryAirSysParam.ActivatableHighTemp.Value = value; }
        }

        private bool _bReturn_flag;
        public bool bReturn_flag
        {
            get { return _bReturn_flag; }
            set
            {
                _bReturn_flag = value;
                NotifyPropertyChanged(nameof(bReturn_flag));
            }
        }

        private bool _bAirPurge_flag;
        public bool bAirPurge_flag
        {
            get { return _bAirPurge_flag; }
            set
            {
                _bAirPurge_flag = value;
                NotifyPropertyChanged(nameof(bAirPurge_flag));
            }
        }

        private bool _bE_Return_flag;
        public bool bE_Return_flag
        {
            get { return _bE_Return_flag; }
            set
            {
                _bE_Return_flag = value;
                NotifyPropertyChanged(nameof(bE_Return_flag));
            }
        }

        private bool _bTReplen_flag;
        public bool bTReplen_flag
        {
            get { return _bTReplen_flag; }
            set
            {
                _bTReplen_flag = value;
                NotifyPropertyChanged(nameof(bTReplen_flag));
            }
        }

        private bool _bDryAirSTG1_flag;
        public bool bDryAirSTG1_flag
        {
            get { return _bDryAirSTG1_flag; }
            set
            {
                _bDryAirSTG1_flag = value;
                NotifyPropertyChanged(nameof(bDryAirSTG1_flag));
            }
        }

        private bool _bDryAirSTG2_flag;
        public bool bDryAirSTG2_flag
        {
            get { return _bDryAirSTG2_flag; }
            set
            {
                _bDryAirSTG2_flag = value;
                NotifyPropertyChanged(nameof(bDryAirSTG2_flag));
            }
        }

        private bool _bDryAirLD_flag;
        public bool bDryAirLD_flag
        {
            get { return _bDryAirLD_flag; }
            set
            {
                _bDryAirLD_flag = value;
                NotifyPropertyChanged(nameof(bDryAirLD_flag));
            }
        }

        private bool _bDryAirTH_flag;
        public bool bDryAirTH_flag
        {
            get { return _bDryAirTH_flag; }
            set
            {
                _bDryAirTH_flag = value;
                NotifyPropertyChanged(nameof(bDryAirTH_flag));
            }
        }

        //

        private bool _bLeakSensor0;
        public bool bLeakSensor0
        {
            get { return _bLeakSensor0; }
            set
            {
                _bLeakSensor0 = value;
                NotifyPropertyChanged(nameof(bLeakSensor0));
            }
        }

        private bool _bLeakSensor1;
        public bool bLeakSensor1
        {
            get { return _bLeakSensor1; }
            set
            {
                _bLeakSensor1 = value;
                NotifyPropertyChanged(nameof(bLeakSensor1));
            }
        }

        private bool _bLeakSensor2;
        public bool bLeakSensor2
        {
            get { return _bLeakSensor2; }
            set
            {
                _bLeakSensor2 = value;
                NotifyPropertyChanged(nameof(bLeakSensor2));
            }
        }

        private bool _bLeakSensor3;
        public bool bLeakSensor3
        {
            get { return _bLeakSensor3; }
            set
            {
                _bLeakSensor3 = value;
                NotifyPropertyChanged(nameof(bLeakSensor3));
            }
        }

        private bool _bLeakSensor4;
        public bool bLeakSensor4
        {
            get { return _bLeakSensor4; }
            set
            {
                _bLeakSensor4 = value;
                NotifyPropertyChanged(nameof(bLeakSensor4));
            }
        }

        private bool _bLeakSensor5;
        public bool bLeakSensor5
        {
            get { return _bLeakSensor5; }
            set
            {
                _bLeakSensor5 = value;
                NotifyPropertyChanged(nameof(bLeakSensor5));
            }
        }

        private bool _bLeakSensor6;
        public bool bLeakSensor6
        {
            get { return _bLeakSensor6; }
            set
            {
                _bLeakSensor6 = value;
                NotifyPropertyChanged(nameof(bLeakSensor6));
            }
        }

        private bool _bLeakSensor7;
        public bool bLeakSensor7
        {
            get { return _bLeakSensor7; }
            set
            {
                _bLeakSensor7 = value;
                NotifyPropertyChanged(nameof(bLeakSensor7));
            }
        }


        public void Init()
        {
            isDisposed = false;
        }

        public void Dispose()
        {
            try
            {
            if (!isDisposed)
            {
                isDisposed = true;
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public void Start()
        {
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    CommandSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void DeInitModule()
        {

        }
        public EventCodeEnum InitModule(Autofac.IContainer container, object param)
        {
            this.Container = container;

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum InitConnect()
        {
            return EventCodeEnum.NONE;
        }
        private Autofac.IContainer Container;

        #region //
    

        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            private set { _ModuleState = value; }
        }

        private ObservableCollection<TransitionInfo> _TransitionInfo;
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set
            {
                if (value != _TransitionInfo)
                {
                    _TransitionInfo = value;
                    NotifyPropertyChanged("TransitionInfo");
                }
            }
        }


        private CommandSlot _CommandSlot;
        public CommandSlot CommandSlot
        {
            get { return _CommandSlot; }
            set { _CommandSlot = value; }
        }

        private CommandTokenSet _RunTokenSet;

        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }
        private IProbeCommandToken _RequestToken;
        public IProbeCommandToken RequestToken
        {
            get { return _RequestToken; }
            set { _RequestToken = value; }
        }

        //public IParam SysParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propInfo)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propInfo));
        }

        #region //
        public void StateTransition(ModuleStateBase state)
        {
            throw new NotImplementedException();
        }

        public void SetInjector(object obj, string HashCode)
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Run()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum FreeRun()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Pause()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Resume()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Abort()
        {
            return ModuleStateEnum.UNDEFINED;
        }

        public void RunAsync()
        {
            throw new NotImplementedException();
        }

        public int SupplySV(bool value) //0
        {
            bSupply_flag = value;
            return 0;
        }

        public int ReturnSV(bool value) //1
        {
            bReturn_flag = value;
            return 0;
        }

        public int AirPurgeSV(bool value) //2
        {
            bAirPurge_flag = value;
            return 0;
        }

        public int E_ReturnSV(bool value) //3
        {
            bE_Return_flag = value;
            return 0;
        }

        public int T_returnSV(bool value) //4
        {
            bTReplen_flag = value;
            return 0;
        }

        public int DryAirSTGSV1(bool value) //5
        {
            bDryAirSTG1_flag = value;
            return 0;
        }

        public int DryAirSTGSV2(bool value) //6
        {
            bDryAirSTG2_flag = value;
            return 0;
        }

        public int DryAirLDSV(bool value) //7
        {
            bDryAirLD_flag = value;
            return 0;
        }

        public int DryAirforTester(bool value)
        {
            bDryAirTH_flag = value;
            return 0;
        }

        public int Read_SupplySV(out bool value)
        {
            value = bSupply_flag;
            return 0;
        }

        public int Read_ReturnSV(out bool value)
        {
            value = bReturn_flag;
            return 0;
        }

        public int Read_AirPurgeSV(out bool value)
        {
            value = bAirPurge_flag;
            return 0;
        }

        public int Read_E_ReturnSV(out bool value)
        {
            value = bE_Return_flag;
            return 0;
        }

        public int Read_T_returnSV(out bool value)
        {
            value = bTReplen_flag;
            return 0;
        }

        public int Read_DryAirSTGSV1(out bool value)
        {
            value = bDryAirSTG1_flag;
            return 0;
        }

        public int Read_DryAirSTGSV2(out bool value)
        {
            value = bDryAirSTG2_flag;
            return 0;
        }

        public int Read_DryAirLDSV(out bool value)
        {
            value = bDryAirLD_flag;
            return 0;
        }

        public int Read_LeakSensor0(out bool value)
        {
            value = bLeakSensor0;
            return 0;
        }

        public int Read_LeakSensor1(out bool value)
        {
            value = bLeakSensor1;
            return 0;
        }

        public int Read_LeakSensor2(out bool value)
        {
            value = bLeakSensor2;
            return 0;
        }
        public int Read_LeakSensor3(out bool value)
        {
            value = bLeakSensor3;
            return 0;
        }
        public int Read_LeakSensor4(out bool value)
        {
            value = bLeakSensor4;
            return 0;
        }
        public int Read_LeakSensor5(out bool value)
        {
            value = bLeakSensor5;
            return 0;
        }
        public int Read_LeakSensor6(out bool value)
        {
            value = bLeakSensor6;
            return 0;
        }
        public int Read_LeakSensor7(out bool value)
        {
            value = bLeakSensor7;
            return 0;
        }

        //public int NetInPortAll()
        //{
        //    return mHBNetIOManager.(mHBNetIOManager.HBNetIOMap.Outputs.DO_DRYAIR_LDSV, value);
        //}

        //private int WriteBit(IOPortDescripter<bool> portDesc, bool value)
        //{
        //    return HBNetIOManager.WriteBit(portDesc, value);
        //}

        private int WaitForIO(IOPortDescripter<bool> io, bool level, long timeout = 0)
        {
            return 0;
        }

        private int ReadBit(IOPortDescripter<bool> portDesc, out bool value)
        {
            value = false;
            return 0;
        }

        public EventCodeEnum ClearData()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public EventCodeEnum LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveSysParameter()
        {
            return EventCodeEnum.NONE;
        }


        public List<IOPortDescripter<bool>> GetInputPorts()
        {
            return new List<IOPortDescripter<bool>>();
        }

        public List<IOPortDescripter<bool>> GetOutputPorts()
        {
            return new List<IOPortDescripter<bool>>();
        }

        #endregion

        public EventCodeEnum Execute()
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
        
        public EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (DyrAirState != null)
                {
                    if (DyrAirState.ContainsKey(dryairType))
                    {
                        DyrAirState[dryairType] = value;
                    }
                    else
                    {
                        DyrAirState.Add(dryairType, value);
                    }
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public bool GetDryAirState(EnumDryAirType dryairType, int stageIndex = -1)
        {
            bool retVal = false;
            try
            {
                if(DyrAirState != null)
                {
                    if(DyrAirState.ContainsKey(dryairType))
                    {
                        DyrAirState.TryGetValue(dryairType, out retVal);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageIndex = -1)
        {
            value = false;
            return 0;
        }
        public byte[] GetDryAirParam(int stageIndex = -1)
        {
            byte[] param = null;
            try
            {
                param = SerializeManager.SerializeToByte(DryAirSysParam, typeof(DryAirSysParameter));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return param;
        }


    }
}
