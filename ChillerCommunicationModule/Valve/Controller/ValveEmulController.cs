namespace ControlModules.Valve.Controller
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ValveEmulController : IValveController
    {
        #region <remarks> Property </remarks>
        public bool Initialized { get; set; } = false;

        private ObservableCollection<bool> _CoolantValvestates
            = new ObservableCollection<bool>();

        public ObservableCollection<bool> CoolantValvestates
        {
            get { return _CoolantValvestates; }
            set { _CoolantValvestates = value; }
        }

        private ObservableCollection<bool> _PurgeValveStates
             = new ObservableCollection<bool>();

        public ObservableCollection<bool> PurgeValveStates
        {
            get { return _PurgeValveStates; }
            set { _PurgeValveStates = value; }
        }

        private ObservableCollection<bool> _DrainValveStates
             = new ObservableCollection<bool>();

        public ObservableCollection<bool> DrainValveStates
        {
            get { return _DrainValveStates; }
            set { _DrainValveStates = value; }
        }

        private ObservableCollection<bool> _DryAirValveStates
             = new ObservableCollection<bool>();

        public ObservableCollection<bool> DryAirValveStates
        {
            get { return _DryAirValveStates; }
            set { _DryAirValveStates = value; }
        }

        public List<bool> Prev_ValveStates_IN { get; set; } = new List<bool>();
        public List<bool> Prev_ValveStates_OUT { get; set; } = new List<bool>();
        public List<bool> Prev_ValveStates_PURGE { get; set; } = new List<bool>();
        public List<bool> Prev_ValveStates_DRAIN { get; set; } = new List<bool>();
        public List<bool> Prev_ValveStates_DRYAIR { get; set; } = new List<bool>();
        public List<bool> Prev_ValveStates_LEAK { get; set; } = new List<bool>();


        #endregion

        #region <remarks> Init & DeInit </remarks>

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {                
                for (int i = 0; i < SystemModuleCount.ModuleCnt.StageCount; i++)
                {
                    CoolantValvestates.Add(new bool());
                    PurgeValveStates.Add(new bool());
                    DrainValveStates.Add(new bool());
                    DryAirValveStates.Add(new bool());

                    Prev_ValveStates_IN.Add(new bool());
                    Prev_ValveStates_OUT.Add(new bool());
                    Prev_ValveStates_PURGE.Add(new bool());
                    Prev_ValveStates_DRAIN.Add(new bool());
                    Prev_ValveStates_DRYAIR.Add(new bool());
                    Prev_ValveStates_LEAK.Add(new bool());
                }

                retVal = EventCodeEnum.NONE;
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

        public bool GetValveState(EnumValveType valveType, int stageIndex = -1)
        {
            string logAdd = string.Empty;

            bool retVal = false;
            try
            {
                int nValveIndex = stageIndex;//TODO: 추후 DryAir ValveMapping 파라미터 구현해야함. 

                bool prev_Val = false;
                if (stageIndex != -1)
                {
                    switch (valveType)
                    {
                        case EnumValveType.INVALID:
                            break;
                        case EnumValveType.UNDEFINED:
                            break;
                        case EnumValveType.IN:
                            if (CoolantValvestates.Count >= nValveIndex)
                            {
                                prev_Val = Prev_ValveStates_IN[nValveIndex - 1];
                                retVal = CoolantValvestates[nValveIndex - 1];
                                logAdd = $"{prev_Val} => {retVal}";
                                Prev_ValveStates_IN[nValveIndex - 1] = CoolantValvestates[nValveIndex - 1];
                            }
                            break;
                        case EnumValveType.OUT:
                            if (CoolantValvestates.Count >= nValveIndex)
                            {
                                prev_Val = Prev_ValveStates_OUT[nValveIndex - 1];
                                retVal = CoolantValvestates[nValveIndex - 1];
                                logAdd = $"{prev_Val} => {retVal}";
                                Prev_ValveStates_OUT[nValveIndex - 1] = CoolantValvestates[nValveIndex - 1];
                            }
                            break;
                        case EnumValveType.PURGE:
                            if (PurgeValveStates.Count >= nValveIndex)
                            {
                                prev_Val = Prev_ValveStates_PURGE[nValveIndex - 1];
                                retVal = PurgeValveStates[nValveIndex - 1];
                                logAdd = $"{prev_Val} => {retVal}";
                                Prev_ValveStates_PURGE[nValveIndex - 1] = PurgeValveStates[nValveIndex - 1];
                            }
                            break;
                        case EnumValveType.DRAIN:
                            if (DrainValveStates.Count >= nValveIndex)
                            {
                                prev_Val = Prev_ValveStates_DRAIN[nValveIndex - 1];
                                retVal = DrainValveStates[nValveIndex - 1];
                                logAdd = $"{prev_Val} => {retVal}";
                                Prev_ValveStates_DRAIN[nValveIndex - 1] = DrainValveStates[nValveIndex - 1];
                            }
                            break;
                        case EnumValveType.DRYAIR:
                            if (DryAirValveStates.Count >= nValveIndex)
                            {
                                prev_Val = Prev_ValveStates_DRYAIR[nValveIndex - 1];
                                retVal = DryAirValveStates[nValveIndex - 1];
                                logAdd = $"{prev_Val} => {retVal}";
                                Prev_ValveStates_DRYAIR[nValveIndex - 1] = DryAirValveStates[nValveIndex - 1];
                            }
                            break;
                        case EnumValveType.Leak:                          
                            break;
                        case EnumValveType.MANUAL_PURGE:
                            break;
                        default:
                            break;
                    }

                    
                    if (prev_Val != retVal)
                    {                        
                        LoggerManager.Debug($"ValveController(EMUL)#{nValveIndex}.StageIndex#{stageIndex}.{valveType}.GetValveState(): {logAdd}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

      
        public EventCodeEnum SetValveState(bool state, EnumValveType valveType, int stageIndex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (stageIndex != -1)
                {
                    if(CoolantValvestates[stageIndex - 1] != state)
                    {
                        LoggerManager.Debug($"[EnvValveEmul] Set ValveType : {valveType} , StageIdx : {stageIndex}, State: {state}.");
                    }

                    switch (valveType)
                    {
                        case EnumValveType.INVALID:
                            break;
                        case EnumValveType.UNDEFINED:
                            break;
                        case EnumValveType.IN:
                            CoolantValvestates[stageIndex - 1] = state;
                            break;
                        case EnumValveType.OUT:
                            CoolantValvestates[stageIndex - 1] = state;
                            break;
                        case EnumValveType.PURGE:
                            PurgeValveStates[stageIndex - 1] = state;
                            break;
                        case EnumValveType.DRAIN:
                            DrainValveStates[stageIndex - 1] = state;
                            break;
                        case EnumValveType.DRYAIR:
                            DryAirValveStates[stageIndex - 1] = state;
                            break;
                        case EnumValveType.Leak:
                            break;
                        case EnumValveType.MANUAL_PURGE:
                            break;
                        default:
                            break;
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

        public void SetModbusCommDelayTime()
        {
            return;
        }
    }
}
