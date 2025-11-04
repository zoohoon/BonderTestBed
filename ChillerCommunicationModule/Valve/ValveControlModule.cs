using System;

namespace ControlModules
{
    using ControlModules.Valve.Controller;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.EnvControl.Parameter;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class ValveManager : IValveManager
    {
        #region .. Property
        public bool Initialized { get; set; } = false;
        private IValveController _Controller;

        public IValveController Controller
        {
            get { return _Controller; }
            set { _Controller = value; }
        }

        public ValveSysParameter SysParam { get; set; }

        public List<ValveStateOfStage> ValveStates = new List<ValveStateOfStage>();

        #endregion

        #region .. Creator & init & DeInite
        public ValveManager(ValveSysParameter param)
        {
            SysParam = param;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (SysParam.ValveModuleType.Value == EnumValveModuleType.LOADER)
                {
                    //loader
                    Controller = new ValveLoaderController();
                }
                else if (SysParam.ValveModuleType.Value == EnumValveModuleType.MODBUS)
                {
                    Controller = new ValveModBusController();
                }
                else if(SysParam.ValveModuleType.Value == EnumValveModuleType.REMOTE)
                {
                    //cell
                    Controller = new ValveRemoteController();
                }
                else
                {
                    // Controller를 쓰지 않음.
                    Controller = new ValveEmulController();                   
                }

                for (int i = 0; i < SystemModuleCount.ModuleCnt.StageCount; i++)
                {
                    ValveStates.Add(new ValveStateOfStage(i + 1));
                }

                Controller?.InitModule();

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
            try
            {
                Controller?.DeInitModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        public EventCodeEnum GetValveModuleType(out EnumValveModuleType valveModuleType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            EnumValveModuleType valvemodule = EnumValveModuleType.INVALID;
            try
            {
                if (this.EnvControlManager().ValveManager != null)
                {
                    valvemodule = SysParam.ValveModuleType.Value;
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            finally
            {
                valveModuleType = valvemodule;
            }
            return retVal;
        }

        public bool GetValveState(EnumValveType valveType, int stageIndex = -1)
        {
            bool ret = false;
            try
            {
                if(Controller != null)
                {
                    ret = Controller.GetValveState(valveType, stageIndex);
                    SetValveStateOfStage(ret, valveType, stageIndex);
                }    
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return ret;
        }

        public EventCodeEnum SetValveState(bool state, EnumValveType valveType, int stageIndex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Controller != null)
                {
                    if(GetValveStateOfStage(valveType, stageIndex) == state)
                    {
                        retVal = EventCodeEnum.NONE;
                        return retVal;
                    }

                    retVal = Controller.SetValveState(state, valveType, stageIndex);
                    if(retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"ValveManager({valveType}).Stage#{stageIndex}.SetValveState(): Failed. eventcode = {retVal} ");
                        return retVal;
                    }
                    
                    Thread.Sleep(SysParam.ValveSettlingTime_msec.Value);
                    bool getVal = Controller.GetValveState(valveType, stageIndex);
                    if (getVal == state)
                    {
                        SetValveStateOfStage(state, valveType, stageIndex);
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        Stopwatch elapsedStopWatch = new Stopwatch();
                        elapsedStopWatch.Reset();
                        elapsedStopWatch.Start();

                        retVal = WaitForValveState(state, valveType, stageIndex, elapsedStopWatch);
                        if(retVal == EventCodeEnum.NONE)
                        {
                            SetValveStateOfStage(state, valveType, stageIndex);
                        }
                    }
                  
                }

                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.VALVE_SET_ERROR;
            }
        }

        private EventCodeEnum WaitForValveState(bool targetState, EnumValveType valveType, int stageIndex, Stopwatch stopwatch)
        {
            EventCodeEnum retVal = EventCodeEnum.VALVE_SET_ERROR;
            try
            {
                bool runFlag = true;
                int retrycnt = 0;

                bool getVal = Controller.GetValveState(valveType, stageIndex);

                do
                {
                    Thread.Sleep(10);//settling time이 0일 떄 너무 빨리 동작하지 않기 위해서 안전 장치

                    if (getVal != targetState)
                    {
                        retrycnt += 1;
                        retVal = Controller.SetValveState(targetState, valveType, stageIndex);
                        Thread.Sleep(SysParam.ValveSettlingTime_msec.Value);
                        getVal = Controller.GetValveState(valveType, stageIndex);

                        if (getVal == targetState)
                        {
                            retVal = EventCodeEnum.NONE;
                            runFlag = false;
                            break;
                        }
                        else if (stopwatch.ElapsedMilliseconds > SysParam.ValveWaitTimeout_msec.Value)
                        {
                            LoggerManager.Debug($"ValveManager({valveType}).Stage#{stageIndex}.SetValveState(): eventcode = {retVal}, retry({retrycnt}) target:{targetState}, cur:{getVal}, SettlingTime:{SysParam.ValveSettlingTime_msec.Value}, Timeout:{SysParam.ValveWaitTimeout_msec.Value} ");
                            retVal = EventCodeEnum.VALVE_SET_ERROR;
                            runFlag = false;
                            break;
                        }
                        else
                        {
                            // 다음 loop
                        }

                    }
                    else
                    {
                        retVal = EventCodeEnum.NONE;
                        runFlag = false;
                        break;
                    }

                } while (runFlag);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetEMGSTOP()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SetValveState(false, EnumValveType.IN);
                retVal = SetValveState(false, EnumValveType.OUT);
                retVal = SetValveState(false, EnumValveType.DRYAIR);
                retVal = SetValveState(false, EnumValveType.PURGE);
                retVal = SetValveState(false, EnumValveType.DRAIN);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private void SetValveStateOfStage(bool state, EnumValveType valveType, int stageIndex)
        {
            try
            {
                switch (valveType)
                {
                    case EnumValveType.INVALID:
                        break;
                    case EnumValveType.UNDEFINED:
                        break;
                    case EnumValveType.IN:
                        ValveStates[stageIndex - 1].CoolantValveState = state;
                        break;
                    case EnumValveType.OUT:
                        ValveStates[stageIndex - 1].CoolantValveState = state;
                        break;
                    case EnumValveType.PURGE:
                        ValveStates[stageIndex - 1].ChuckPurgeValveState = state;
                        break;
                    case EnumValveType.DRAIN:
                        ValveStates[stageIndex - 1].DrainValveState = state;
                        break;
                    case EnumValveType.DRYAIR:
                        ValveStates[stageIndex - 1].DryAirValveState = state;
                        break;
                    case EnumValveType.Leak:
                        ValveStates[stageIndex - 1].LeakValveState = state;
                        break;
                    case EnumValveType.MANUAL_PURGE:
                        ValveStates[stageIndex - 1].PurgeValveState = state;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ValveStateOfStage GetValveStateOfStage(int stageIndex)
        {
            ValveStateOfStage valveStateOfStage = null;
            try
            {
                if(ValveStates != null && ValveStates.Count > 0)
                {
                    valveStateOfStage = ValveStates.Find(valveinfo => valveinfo.StageIndex == stageIndex);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return valveStateOfStage;
        }

        private bool GetValveStateOfStage(EnumValveType valveType, int stageIndex)
        {
            bool retVal = false;
            try
            {
                switch (valveType)
                {
                    case EnumValveType.INVALID:
                        break;
                    case EnumValveType.UNDEFINED:
                        break;
                    case EnumValveType.IN:
                        retVal = ValveStates[stageIndex - 1].CoolantValveState;
                        break;
                    case EnumValveType.OUT:
                        retVal = ValveStates[stageIndex - 1].CoolantValveState;
                        break;
                    case EnumValveType.PURGE:
                        retVal = ValveStates[stageIndex - 1].ChuckPurgeValveState;
                        break;
                    case EnumValveType.DRAIN:
                        retVal = ValveStates[stageIndex - 1].DrainValveState;
                        break;
                    case EnumValveType.DRYAIR:
                        retVal = ValveStates[stageIndex - 1].DryAirValveState;
                        break;
                    case EnumValveType.Leak:
                        retVal = ValveStates[stageIndex - 1].LeakValveState;
                        break;
                    case EnumValveType.MANUAL_PURGE:
                        retVal = ValveStates[stageIndex - 1].PurgeValveState;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }
}
