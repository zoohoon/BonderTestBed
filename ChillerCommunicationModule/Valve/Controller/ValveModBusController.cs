namespace ControlModules.Valve.Controller
{
    using Autofac;
    using EasyModbus;
    using LoaderBase;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.EnvControl.Parameter;
    using System;
    using System.Collections.Generic;
    using System.Net.NetworkInformation;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Threading;

    public class ValveModBusController : IValveController
    {
        #region <remarks> Property </remarks>
        public bool Initialized { get; set; } = false;
        List<ValveStateInfo> ValveStates = new List<ValveStateInfo>();
        private long CommReadDelayTime { get; set; }
        private long CommWriteDelayTime { get; set; }
        private int PingTimeOut { get; set; }
        private List<ValveMappingParameter> ValveMappings = new List<ValveMappingParameter>();
        private List<ValveMappingParameter> DryAirMappings = new List<ValveMappingParameter>();

        public List<bool> Prev_ValveStates_IN { get; set; } = new List<bool>();
        public List<bool> Prev_ValveStates_OUT { get; set; } = new List<bool>();
        public List<bool> Prev_ValveStates_PURGE { get; set; } = new List<bool>();
        public List<bool> Prev_ValveStates_DRAIN { get; set; } = new List<bool>();
        public List<bool> Prev_ValveStates_DRYAIR { get; set; } = new List<bool>();
        public List<bool> Prev_ValveStates_LEAK { get; set; } = new List<bool>();

        public List<bool> Prev_ValveStates_MANUALPURGE { get; set; } = new List<bool>();

        #endregion

        #region <remarks> Init & DeInit </remarks>

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int chillerCount = this.EnvControlManager().ChillerManager.GetChillerModules().Count;
                for (int i = 0; i < chillerCount; i++)
                {
                    ValveStates.Add(new ValveStateInfo());
                }

                for (int i = 0; i < GPLoaderDef.StageCount; i++)//GPLoaderDef.StageCount 는 GPLoader.InitModule()과 동일하게 사용함.
                {
                    Prev_ValveStates_IN.Add(new bool());
                    Prev_ValveStates_OUT.Add(new bool());
                    Prev_ValveStates_PURGE.Add(new bool());
                    Prev_ValveStates_DRAIN.Add(new bool());
                    Prev_ValveStates_DRYAIR.Add(new bool());
                    Prev_ValveStates_LEAK.Add(new bool());
                    Prev_ValveStates_MANUALPURGE.Add(new bool());
                }

                retVal = InitValveState();
                InitValveMappings();
                SetModbusCommDelayTime();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void DeInitModule()
        {
            return;
        }

        #endregion
        object lockObj = new object();

        private int ConvertValveIndex(int nStageIndex)
        {
            int nValveIndex = nStageIndex;
            try
            {
                if (SystemManager.SystemType == SystemTypeEnum.Opera)
                {
                    switch (nStageIndex)
                    {
                        case 1:
                            nValveIndex = 9; break;
                        case 2:
                            nValveIndex = 10; break;
                        case 3:
                            nValveIndex = 11; break;
                        case 4:
                            nValveIndex = 12; break;
                        case 5:
                            nValveIndex = 5; break;
                        case 6:
                            nValveIndex = 6; break;
                        case 7:
                            nValveIndex = 7; break;
                        case 8:
                            nValveIndex = 8; break;
                        case 9:
                            nValveIndex = 1; break;
                        case 10:
                            nValveIndex = 2; break;
                        case 11:
                            nValveIndex = 3; break;
                        case 12:
                            nValveIndex = 4; break;
                        default:
                            break;
                    }
                }
                else if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                {
                    switch (nStageIndex)
                    {
                        case 1:
                            nValveIndex = 1; break;
                        case 2:
                            nValveIndex = 2; break;
                        case 3:
                            nValveIndex = 5; break;
                        case 4:
                            nValveIndex = 6; break;
                        case 5:
                            nValveIndex = 9; break;
                        case 6:
                            nValveIndex = 10; break;
                        case 7:
                            nValveIndex = 3; break;
                        case 8:
                            nValveIndex = 4; break;
                        case 9:
                            nValveIndex = 7; break;
                        case 10:
                            nValveIndex = 8; break;
                        case 11:
                            nValveIndex = 11; break;
                        case 12:
                            nValveIndex = 12; break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return nValveIndex;
        }
        public bool GetValveState(EnumValveType valveType, int stageIndex = -1)
        {
            bool retVal = false;
            bool prev_Val = false;
            string logAdd = string.Empty;
            try
            {
                lock (lockObj)
                {
                    int chillerIdx = this.EnvControlManager().ChillerManager.GetChillerIndex(stageIndex);
                    var chillerModule = this.EnvControlManager().ChillerManager.GetChillerModule(chillerIndex: chillerIdx);
                    int sIndex = GetListIndex(stageIndex);

                    if (valveType != EnumValveType.DRYAIR)
                    {
                        if (chillerModule.GetCommState() == ProberInterfaces.Enum.EnumCommunicationState.CONNECTED)
                        {
                            var obj = chillerModule.GetCommunicationObj();
                            ModbusClient Client = obj as ModbusClient;
                            object lockObj = chillerModule.GetCommLockObj();
                            ModBusValveEnum modBusValveEnum = ModBusValveEnum.UNDEFIND;
                            switch (valveType)
                            {
                                case EnumValveType.IN:
                                    modBusValveEnum = ModBusValveEnum.COOLANT;
                                    break;
                                case EnumValveType.OUT:
                                    modBusValveEnum = ModBusValveEnum.COOLANT;
                                    break;
                                case EnumValveType.PURGE:
                                    modBusValveEnum = ModBusValveEnum.CHUCK_PURGE;
                                    break;
                                case EnumValveType.DRAIN:
                                    modBusValveEnum = ModBusValveEnum.DRAIN;
                                    break;
                                case EnumValveType.DRYAIR:
                                    modBusValveEnum = ModBusValveEnum.DRYAIR;
                                    break;
                                case EnumValveType.MANUAL_PURGE:
                                    modBusValveEnum = ModBusValveEnum.PURGE;
                                    break;
                                default:
                                    break;
                            }

                            if (modBusValveEnum != ModBusValveEnum.UNDEFIND & modBusValveEnum != ModBusValveEnum.DRYAIR)
                            {
                                int[] ret = ReadHoldingRegister(Client, modBusValveEnum, lockObj);
                                if (ret != null)
                                {
                                    string str2 = Convert.ToString(ret[0], 2);   // 결과값 : 10101 //10진수를 2진수로.
                                    List<int> binArr = new List<int>();
                                    while (ret[0] != 0)
                                    {
                                        int reminder = ret[0] % 2;
                                        binArr.Add(reminder);
                                        ret[0] /= 2;
                                    }
                                    binArr.Reverse();

                                    if (binArr.Count != 4)
                                    {
                                        int offsetcount = 4 - binArr.Count;
                                        for (int i = 0; i < offsetcount; i++)
                                        {
                                            binArr.Insert(0, 0);
                                        }
                                    }
                                    if (binArr[sIndex] == 0)
                                        retVal = false;
                                    else if (binArr[sIndex] == 1)
                                        retVal = true;


                                    switch (valveType)
                                    {
                                        case EnumValveType.IN:
                                            prev_Val = Prev_ValveStates_IN[stageIndex - 1];
                                            ValveStates[chillerIdx - 1].CoolantValveStates = binArr.ToArray();
                                            logAdd = $"{prev_Val} => {retVal}";
                                            Prev_ValveStates_IN[stageIndex - 1] = retVal;
                                            break;
                                        case EnumValveType.OUT:
                                            prev_Val = Prev_ValveStates_OUT[stageIndex - 1];
                                            ValveStates[chillerIdx - 1].CoolantValveStates = binArr.ToArray();
                                            logAdd = $"{prev_Val} => {retVal}";
                                            Prev_ValveStates_OUT[stageIndex - 1] = retVal;
                                            break;
                                        case EnumValveType.PURGE:
                                            prev_Val = Prev_ValveStates_PURGE[stageIndex - 1];
                                            ValveStates[chillerIdx - 1].ChuckPurgeValveStates = binArr.ToArray();
                                            logAdd = $"{prev_Val} => {retVal}";
                                            Prev_ValveStates_PURGE[stageIndex - 1] = retVal;
                                            break;
                                        case EnumValveType.DRAIN:
                                            prev_Val = Prev_ValveStates_DRAIN[stageIndex - 1];
                                            ValveStates[chillerIdx - 1].DrainValveStates = binArr.ToArray();
                                            logAdd = $"{prev_Val} => {retVal}";
                                            Prev_ValveStates_DRAIN[stageIndex - 1] = retVal;
                                            break;
                                        case EnumValveType.MANUAL_PURGE:
                                            prev_Val = Prev_ValveStates_MANUALPURGE[stageIndex - 1];
                                            ValveStates[chillerIdx - 1].PurgeValveStates = binArr.ToArray();
                                            logAdd = $"{prev_Val} => {retVal}";
                                            Prev_ValveStates_MANUALPURGE[stageIndex - 1] = retVal;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                //retVal = false;
                            }
                        }
                    }
                    else // valveType == DRYAIR
                    {
                        ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                        LoaderBase.IGPLoaderCommands gpLoader = loaderMaster.Loader.GetLoaderCommands();
                        IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;

                        int nValveIndex = GetDryAirIndex(stageIndex);
                        prev_Val = Prev_ValveStates_DRYAIR[nValveIndex - 1];
                        logAdd = $"{prev_Val} => {retVal}";
                        if (loadercommand.DryAirValveStates.Count >= nValveIndex)
                            retVal = loadercommand.DryAirValveStates[nValveIndex - 1];
                        Prev_ValveStates_DRYAIR[nValveIndex - 1] = retVal;
                    }
                    if (prev_Val != retVal)
                    {
                        LoggerManager.Debug($"ValveController(MODBUS).StageIndex#{stageIndex}.{valveType}.GetValveState(): {logAdd}");
                    }
                }
            }
            catch (Exception err)
            {
                this.EnvControlManager().ChillerManager.GetChillerModule(stageIndex).DisConnect();
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum SetValveState(bool state, EnumValveType valveType, int stageIndex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                lock (lockObj)
                {
                    int chillerIdx = this.EnvControlManager().ChillerManager.GetChillerIndex(stageIndex);
                    var chillerModule = this.EnvControlManager().ChillerManager.GetChillerModule(chillerIndex: chillerIdx);
                    int sIndex = GetListIndex(stageIndex);

                    if (valveType != EnumValveType.DRYAIR)
                    {
                        if (chillerModule.GetCommState() == ProberInterfaces.Enum.EnumCommunicationState.CONNECTED)
                        {
                            var obj = chillerModule.GetCommunicationObj();
                            ModbusClient Client = obj as ModbusClient;
                            object lockObj = chillerModule.GetCommLockObj();
                            ModBusValveEnum modBusValveEnum = ModBusValveEnum.UNDEFIND;
                            switch (valveType)
                            {
                                case EnumValveType.IN:
                                    modBusValveEnum = ModBusValveEnum.COOLANT;
                                    break;
                                case EnumValveType.OUT:
                                    modBusValveEnum = ModBusValveEnum.COOLANT;
                                    break;
                                case EnumValveType.PURGE:
                                    modBusValveEnum = ModBusValveEnum.CHUCK_PURGE;
                                    break;
                                case EnumValveType.DRAIN:
                                    modBusValveEnum = ModBusValveEnum.DRAIN;
                                    break;
                                case EnumValveType.DRYAIR:
                                    modBusValveEnum = ModBusValveEnum.DRYAIR;
                                    break;
                                case EnumValveType.MANUAL_PURGE:
                                    modBusValveEnum = ModBusValveEnum.PURGE;
                                    break;
                                default:
                                    break;
                            }

                            if (modBusValveEnum != ModBusValveEnum.UNDEFIND)
                            {
                                string statelist = "";

                                switch (valveType)
                                {
                                    case EnumValveType.IN:
                                        ValveStates[chillerIdx - 1].CoolantValveStates[sIndex] = state ? 1 : 0;
                                        statelist = String.Join("", ValveStates[chillerIdx - 1].CoolantValveStates);
                                        break;
                                    case EnumValveType.OUT:
                                        ValveStates[chillerIdx - 1].CoolantValveStates[sIndex] = state ? 1 : 0;
                                        statelist = String.Join("", ValveStates[chillerIdx - 1].CoolantValveStates);
                                        break;
                                    case EnumValveType.PURGE:
                                        ValveStates[chillerIdx - 1].ChuckPurgeValveStates[sIndex] = state ? 1 : 0;
                                        statelist = String.Join("", ValveStates[chillerIdx - 1].ChuckPurgeValveStates);
                                        break;
                                    case EnumValveType.DRAIN:
                                        ValveStates[chillerIdx - 1].DrainValveStates[sIndex] = state ? 1 : 0;
                                        statelist = String.Join("", ValveStates[chillerIdx - 1].DrainValveStates);
                                        break;
                                    case EnumValveType.MANUAL_PURGE:
                                        ValveStates[chillerIdx - 1].PurgeValveStates[sIndex] = state ? 1 : 0;
                                        statelist = String.Join("", ValveStates[chillerIdx - 1].PurgeValveStates);
                                        break;
                                    case EnumValveType.DRYAIR:
                                        {
                                            ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                                            LoaderBase.IGPLoaderCommands gpLoader = loaderMaster.Loader.GetLoaderCommands();
                                            IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;

                                            int nValveIndex = GetDryAirIndex(stageIndex);
                                            retVal = loadercommand.ValveControl(valveType, nValveIndex, state);
                                        }
                                        break;
                                    default:
                                        break;
                                }

                                if (valveType != EnumValveType.DRYAIR)
                                {
                                    WriteSingleRegister(Client, modBusValveEnum, Convert.ToInt32(statelist, 2), lockObj);
                                    retVal = EventCodeEnum.NONE;
                                }
                            }
                        }
                        else
                        {
                            retVal = EventCodeEnum.ENVCONTROL_CONNECT_ERROR;
                        }
                    }
                    else // valveType is DRYAIR
                    {
                        ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                        LoaderBase.IGPLoaderCommands gpLoader = loaderMaster.Loader.GetLoaderCommands();
                        IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;

                        int nValveIndex = GetDryAirIndex(stageIndex);
                        retVal = loadercommand.ValveControl(valveType, nValveIndex, state);
                    }

                    LoggerManager.Debug($"ValveController(MODBUS).StageIndex#{stageIndex}.{valveType}.SetValveState(): {state}");

                }
            }
            catch (Exception err)
            {
                this.EnvControlManager().ChillerManager.GetChillerModule(stageIndex).DisConnect();
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        private int GetListIndex(int stageIndex)
        {
            int index = -1;
            try
            {
                //if (stageIndex == 1 | stageIndex == 5 | stageIndex == 12)
                //{
                //    index = 3;
                //}
                //else if (stageIndex == 2 | stageIndex == 6 | stageIndex == 11)
                //{
                //    index = 2;
                //}
                //else if (stageIndex == 3 | stageIndex == 7 | stageIndex == 10)
                //{
                //    index = 1;
                //}
                //else if (stageIndex == 4 | stageIndex == 8 | stageIndex == 9)
                //{
                //    index = 0;
                //}
                index = ValveMappings.Find(mappings => mappings.StageIndex == stageIndex)?.ValveIndex ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return index;
        }

        private int GetDryAirIndex(int stageIndex)
        {
            int index = -1;
            try
            {
                if(DryAirMappings != null)
                {
                    index = DryAirMappings.Find(mappings => mappings.StageIndex == stageIndex)?.ValveIndex ?? -1;
                }
                else
                {
                    index = ConvertValveIndex(stageIndex);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return index;
        }

        private EventCodeEnum InitValveState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int stageCount = this.GetLoaderContainer()?.Resolve<ILoaderSupervisor>()?.StageStates.Count ?? 0;
                
                for (int idx = 0; idx < stageCount; idx++)
                {
                    GetValveState(EnumValveType.IN, idx);
                    GetValveState(EnumValveType.DRAIN, idx);
                    GetValveState(EnumValveType.MANUAL_PURGE, idx);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        #region <remarks> Modbus Read/Write </remarks>
        private Ping ping = new Ping();
        public IPStatus PingTest(string address)
        {
            IPStatus status = IPStatus.Unknown;
            try
            {
                if (PingTimeOut == 0)
                {
                    status = this.ping.Send(address)?.Status ?? IPStatus.Unknown;
                    if (status != IPStatus.Success)
                    {
                        int retryCount = 3;
                        for (int count = 0; count < retryCount; count++)
                        {
                            status = this.ping.Send(address)?.Status ?? IPStatus.Unknown;
                            if (status == IPStatus.Success)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    status = this.ping.Send(address, PingTimeOut)?.Status ?? IPStatus.Unknown;
                    if (status != IPStatus.Success)
                    {
                        int retryCount = 3;
                        for (int count = 0; count < retryCount; count++)
                        {
                            status = this.ping.Send(address, PingTimeOut)?.Status ?? IPStatus.Unknown;
                            if (status == IPStatus.Success)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return status;
        }

        public void Wait(long time)
        {
            try
            {
                Thread.Sleep((int)time);
                //var frame = new DispatcherFrame();
                //new Thread(async() =>
                //{
                //    // asynchronously wait for the event/timeout
                //    await Task.Delay((int)time);
                //    // signal the secondary dispatcher to stop
                //    frame.Continue = false;
                //}).Start();
                //Dispatcher.PushFrame(frame); // start the secondary dispatcher, pausing this code
                //frame = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }





        private int[] ReadHoldingRegister(ModbusClient Client, ModBusValveEnum cmd, object lockObj)
        {
            if (lockObj == null)
            {
                LoggerManager.Debug($"GetValve Exception occured. comm lock obj is null.");
                return null;
            }

            lock (lockObj)
            {
                try
                {
                    if (Client != null && Client.IPAddress.Equals("0.0.0.0") == false)
                    {
                        var pingTestStatus = PingTest(Client?.IPAddress ?? "0");
                        if (pingTestStatus == IPStatus.Success)
                        {
                            int[] result = Client?.ReadHoldingRegisters((int)cmd, 1, (int)CommReadDelayTime) ?? null;

                            if (CommReadDelayTime != 0)
                            {
                                Wait(CommReadDelayTime);
                            }
                            if (result == null)
                            {
                                //Retry
                                result = Client?.ReadHoldingRegisters((int)cmd, 1, (int)CommReadDelayTime) ?? null;
                                if (CommReadDelayTime != 0)
                                {
                                    Wait(CommReadDelayTime);
                                }

                                if (result == null)
                                    throw new Exception($"[Chiller_Modbus] ReadHoldingRegister() : Fail Get {cmd} Data.");
                            }
                            return result;
                        }
                        else
                        {
                            throw new Exception($"[Chiller_Modbus] ReadHoldingRegister() : Fail Ping Test. Check connect the chiller.");
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception err)
                {
                    throw err;
                }
            }
        }

        private void WriteSingleRegister(ModbusClient Client, ModBusValveEnum cmd, int value, object lockObj)
        {
            if (lockObj == null)
                return;
            lock (lockObj)
            {
                try
                {
                    var pingTestStatus = PingTest(Client.IPAddress);//TODO: HuberChillerComm에도 넣어야하는가? 
                    if (pingTestStatus == IPStatus.Success)
                    {
                        Client?.WriteSingleRegister((int)cmd, value, (int)CommWriteDelayTime);
                        if (CommWriteDelayTime != 0)
                        {
                            Wait(CommWriteDelayTime);
                        }
                    }
                    else
                    {
                        throw new Exception($"[Chiller_Modbus] WriteSingleRegister() : Fail Ping Test. Check connect the chiller.");
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw err;
                }
            }
        }
        #endregion

        public void SetModbusCommDelayTime()
        {
            try
            {
                CommReadDelayTime = this.EnvControlManager().ChillerManager.GetCommReadDelayTime();
                CommWriteDelayTime = this.EnvControlManager().ChillerManager.GetCommWriteDelayTime();
                PingTimeOut = this.EnvControlManager().ChillerManager.GetPingTimeOut();
                if(CommReadDelayTime < 10)
                {
                    CommReadDelayTime = 10;
                }
                if(CommWriteDelayTime < 10)
                {
                    CommWriteDelayTime = 10;
                }
                if(PingTimeOut < 10)
                {
                    PingTimeOut = 10;
                }

                LoggerManager.Debug($"[ValveModBusController] SetModbusCommDelayTime(). CommReadDelayTime Set to {CommReadDelayTime}.");
                LoggerManager.Debug($"[ValveModBusController] SetModbusCommDelayTime(). CommWriteDelayTime Set to {CommWriteDelayTime}.");
                LoggerManager.Debug($"[ValveModBusController] SetModbusCommDelayTime(). PingTimeOut Set to {PingTimeOut}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void InitValveMappings()
        {
            try
            {
                ValveSysParameter param = this.EnvControlManager().GetValveParam() as ValveSysParameter;
                if(param != null)
                {
                    ValveMappings = param.ValveMappingParam;
                    DryAirMappings = param.DryAirMappingParam;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }

    public class ValveStateInfo
    {
        private int[] _CoolantValveStates = new int[4];

        public int[] CoolantValveStates
        {
            get { return _CoolantValveStates; }
            set { _CoolantValveStates = value; }
        }

        private int[] _ChuckPurgeValveStates = new int[4];

        public int[] ChuckPurgeValveStates
        {
            get { return _ChuckPurgeValveStates; }
            set { _ChuckPurgeValveStates = value; }
        }

        private int[] _DrainValveStates = new int[4];

        public int[] DrainValveStates
        {
            get { return _DrainValveStates; }
            set { _DrainValveStates = value; }
        }

        private int[] _PurgeValveStates = new int[4];

        public int[] PurgeValveStates
        {
            get { return _PurgeValveStates; }
            set { _PurgeValveStates = value; }
        }


    }
    public enum ModBusValveEnum
    {
        UNDEFIND = -1,
        COOLANT = 0xF0,
        CHUCK_PURGE = 0xF1,
        PURGE = 0xF2,
        DRAIN = 0xF3,
        CH1_PRESSURE = 0xF4,
        CH2_PRESSURE = 0xF5,
        CH3_PRESSURE = 0xF6,
        CH4_PRESSURE = 0xF7,
        DRYAIR = 0xF8
    }
}
