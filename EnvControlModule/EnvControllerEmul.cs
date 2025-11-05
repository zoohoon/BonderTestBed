using System;

namespace EnvControlModule
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using SerializerUtil;
    public class EnvControllerEmul : IEnvController, IEnvControlServiceCallback
    {
        public bool Initialized { get; set; }
        private EnvControlServiceHost ServiceHost { get; set; }
        public EventCodeEnum DisConnect(int index = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (ServiceHost != null)
                {
                    ServiceHost.DisConnect(index);
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
            
        }
        public bool IsAlive()
        {
            return true;
        }

        public double GetDewPointVal()
        {
            return this.TempController().TempInfo.DewPoint.Value;
        }

        public IEnvControlServiceCallback GetEnvControlClient(int stageindex = -1)
        {
            if(SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
            {
                return this;
            }
            else if(SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
            {
                return ServiceHost.GetEnvControlClient(stageindex);
            }
            return null;
        }
        public double GetChillerTargetTemp()
        {
            return this.EnvControlManager().GetChillerModule().ChillerInfo.TargetTemp;
        }
        public double GetTempTargetTemp()
        {
            return this.TempController().TempInfo.TargetTemp.Value;
        }
        public bool GetChillerActiveState()
        {
            return this.EnvControlManager().GetChillerModule().ChillerInfo.ChillerActiveStage;
        }
        public EventCodeEnum InitModule()
        {
            LoggerManager.Debug($"Environment Controller Emulator Initialized.");
            if(SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
            {
                ServiceHost = new EnvControlServiceHost() { Manager = this };
                if (ServiceHost != null)
                    ServiceHost.InitModule();
            }
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum InitConnect()
        {
            LoggerManager.Debug($"Environment Controller Emulator init connect.");
            return EventCodeEnum.NONE;
        }

        public bool GetIsExcute()
        {
            return true;
        }
        public bool IsUsingChiller(int stageindex = -1)
        {
            bool retVal = false;
            try
            {
                if(stageindex == -1)
                {
                    stageindex = 0;
                }
                retVal = ((int)this.EnvControlManager().ChillerManager.GetMode(stageindex)
                    & (int)ModuleEnableType.ENABLE) == (int)ModuleEnableType.ENABLE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool IsUsingDryAir(int stageindex = -1)
        {
            bool retVal = false;
            try
            {
                if (stageindex == -1)
                {
                    stageindex = 0;
                }
                retVal = ((int)this.EnvControlManager().DryAirManager.GetMode(stageindex) & (int)ModuleEnableType.ENABLE) == (int)ModuleEnableType.ENABLE;
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
                if (stageIndex == -1)
                {
                    stageIndex = 0;
                }

                if (this.EnvControlManager().GetValveParam().ValveModuleType.Value == EnumValveModuleType.NA
                    && (valveType == EnumValveType.IN || valveType == EnumValveType.OUT))
                {
                    this.EnvControlManager().ChillerManager.SetCircuationActive(state, (byte)stageIndex);
                    retVal = EventCodeEnum.NONE;
                }

                // else 로 하면 emul valve값 업데이트 안됨.
                retVal = this.EnvControlManager().ValveManager.SetValveState(state, valveType, stageIndex);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool GetValveState(EnumValveType valveType, int stageIndex = -1)
        {
            bool retVal = false;
            try
            {
                if (stageIndex == -1)
                {
                    stageIndex = 0;
                }

                if (this.EnvControlManager().GetValveParam().ValveModuleType.Value == EnumValveModuleType.NA
                    && (valveType == EnumValveType.IN || valveType == EnumValveType.OUT))
                {
                    retVal = this.EnvControlManager().ChillerManager.IsCirculationActive(stageIndex);
                }
                else
                {
                    
                    retVal = this.EnvControlManager().ValveManager.GetValveState(valveType, (byte)stageIndex);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retVal;
        }

        public byte[] GetDryAirParam(int stageindex = -1)
        {
            if (stageindex == -1)
            {
                stageindex = 0;
            }
            return this.EnvControlManager().DryAirManager.GetDryAirParam() ;

        }
        public EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1)
        {
            if (stageIndex == -1)
            {
                stageIndex = 0;
            }
            return this.EnvControlManager().DryAirManager.DryAirForProber(value, dryairType, stageIndex);
        }
        public int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageindex = -1)
        {
            if (stageindex == -1)
            {
                stageindex = 0;
            }
            return this.EnvControlManager().DryAirManager.GetLeakSensor(out value, leakSensorIndex, stageindex);
        }

        public RemoteStageColdSetupData GetRemoteColdData()
        {
            RemoteStageColdSetupData data = new RemoteStageColdSetupData();
            try
            {
                data.DewPointTolerance = this.EnvControlManager().GetDewPointModule().DewPointOffset;
                data.DryAirActivatableHighTemp = this.EnvControlManager().GetDryAirModule().DryAirActivableHighTemp;
                data.DewPointTimeOut = this.EnvControlManager().GetDewPointModule().WaitTimeout;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return data;
        }
        public void SetRemoteColdData(RemoteStageColdSetupData remotedata)
        {

        }
        public void SetChillerData(byte[] chillerparam, bool setremotechange = false)
        {
            object target;
            SerializeManager.DeserializeFromByte(chillerparam, out target, typeof(ChillerParameter));
            this.EnvControlManager().GetChillerModule()?.InitParam((IChillerParameter)target, setremotechange);
        }

        public void SetChillerAbortMode(bool flag)
        {
            try
            {
                if(this.EnvControlManager().GetChillerModule() != null)
                {
                    this.EnvControlManager().GetChillerModule().ChillerInfo.AbortChiller = flag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void RaiseFFUAlarm(string alarmMessage)
        {
            //StringBuilder stb = new StringBuilder();
            //for (int i = 0; i < errorCodes.Count(); i++)
            //{
            //    stb.Append(errorCodes[i].ToString());
            //    if(i < errorCodes.Count() - 1)
            //    {
            //        stb.Append("+");
            //    }
            //}
            //LoggerManager.Debug($"FFU Alarm Occurred. Node num. = {nodeNum}, {stb.ToString()}");
        }
    }
}
