using System;

namespace ControlModules.Valve.Controller
{
    using Autofac;
    using LoaderBase;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Collections.Generic;

    public class ValveLoaderController : IValveController
    {
        #region <remarks> Property </remarks>
        public bool Initialized { get; set; } = false;

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
                for (int i = 0; i < GPLoaderDef.StageCount; i++)//GPLoaderDef.StageCount 는 GPLoader.InitModule()과 동일하게 사용함.
                {
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

        #region <remarks> Method </remarks>

        private int ConvertValveIndex(int nStageIndex)
        {
            int nValveIndex = nStageIndex;
            try
            {
                if (SystemManager.SystemType == SystemTypeEnum.Opera || SystemManager.SystemType == SystemTypeEnum.GOP)
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

        public EventCodeEnum SetValveState(bool state, EnumValveType valveType, int stageIndex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                LoaderBase.IGPLoaderCommands gpLoader = loaderMaster.Loader.GetLoaderCommands();
                IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;

                int nValveIndex = ConvertValveIndex(stageIndex);

                retVal = loadercommand.ValveControl(valveType, nValveIndex, state);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        


        public bool GetValveState(EnumValveType valveType, int stageIndex = -1)
        {
            string logAdd = string.Empty;
            bool retVal = false;
            try
            {                
                if (stageIndex == -1)
                    return retVal;

                ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                LoaderBase.IGPLoaderCommands gpLoader = loaderMaster.Loader.GetLoaderCommands();
                IGPUtilityBoxCommands loadercommand = (IGPUtilityBoxCommands)gpLoader;                

                int nValveIndex = ConvertValveIndex(stageIndex);

                bool prev_Val = false;
                switch (valveType)
                {                    
                    case EnumValveType.IN:
                        if (loadercommand.CoolantInletValveStates.Count >= nValveIndex)
                        {
                            prev_Val = Prev_ValveStates_IN[nValveIndex - 1];
                            retVal = loadercommand.CoolantInletValveStates[nValveIndex - 1];
                            logAdd = $"{prev_Val} => {retVal}";
                            Prev_ValveStates_IN[nValveIndex - 1] = loadercommand.CoolantInletValveStates[nValveIndex - 1];
                        }
                            
                        break;
                    case EnumValveType.OUT:
                        if (loadercommand.CoolantOutletValveStates.Count >= nValveIndex)
                        {
                            prev_Val = Prev_ValveStates_OUT[nValveIndex - 1];
                            retVal = loadercommand.CoolantOutletValveStates[nValveIndex - 1];
                            logAdd = $"{prev_Val} => {retVal}";
                            Prev_ValveStates_OUT[nValveIndex - 1] = loadercommand.CoolantInletValveStates[nValveIndex - 1];
                        }
                        break;
                    case EnumValveType.PURGE:
                        if (loadercommand.PurgeValveStates.Count >= nValveIndex)
                        {
                            prev_Val = Prev_ValveStates_PURGE[nValveIndex - 1];
                            retVal = loadercommand.PurgeValveStates[nValveIndex - 1];
                            logAdd = $"{prev_Val} => {retVal}";
                            Prev_ValveStates_PURGE[nValveIndex - 1] = loadercommand.PurgeValveStates[nValveIndex - 1];
                        }
                        break;
                    case EnumValveType.DRAIN:
                        if (loadercommand.DrainValveStates.Count >= nValveIndex)
                        {
                            prev_Val = Prev_ValveStates_DRAIN[nValveIndex - 1];
                            retVal = loadercommand.DrainValveStates[nValveIndex - 1];
                            logAdd = $"{prev_Val} => {retVal}";
                            Prev_ValveStates_DRAIN[nValveIndex - 1] = loadercommand.DrainValveStates[nValveIndex - 1];
                        }
                        break;
                    case EnumValveType.DRYAIR:
                        if (loadercommand.DryAirValveStates.Count >= nValveIndex)
                        {
                            prev_Val = Prev_ValveStates_DRYAIR[nValveIndex - 1];
                            retVal = loadercommand.DryAirValveStates[nValveIndex - 1];
                            logAdd = $"{prev_Val} => {retVal}";
                            Prev_ValveStates_DRYAIR[nValveIndex - 1] = loadercommand.DryAirValveStates[nValveIndex - 1];
                        }
                        break;
                    case EnumValveType.Leak:
                        if (loadercommand.CoolantLeaks.Count >= nValveIndex)
                        {
                            prev_Val = Prev_ValveStates_LEAK[nValveIndex - 1];
                            retVal = loadercommand.CoolantLeaks[nValveIndex - 1];
                            logAdd = $"{prev_Val} => {retVal}";
                            Prev_ValveStates_LEAK[nValveIndex - 1] = loadercommand.CoolantLeaks[nValveIndex - 1];
                        }
                        break;
                    default:
                        break;
                }

                if (prev_Val != retVal)
                {
                    LoggerManager.Debug($"ValveController(LOADER)#{nValveIndex}.StageIndex#{stageIndex}.{valveType}.GetValveState(): {logAdd}");
                }
                //if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                //    return true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
     
            return retVal;
        }

        public void SetModbusCommDelayTime()
        {
            return;
        }
        #endregion
    }
}
