using System;

namespace EnvControlModule
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;

    public class EnvController : IEnvController 
    {
        #region //..Property
        public bool Initialized { get; set; }
        private EnvControlServiceHost ServiceHost { get; set; }

        #endregion
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(!Initialized)
                {
                    retVal = InitServiceHost();
                    Initialized = true;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum InitServiceHost()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(SystemManager.SysteMode != SystemModeEnum.Single)
                {
                    ServiceHost = new EnvControlServiceHost() { Manager = this };
                    if (ServiceHost != null)
                        ServiceHost.InitModule();
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InitConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = InitServiceHost();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DisConnect(int index = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(ServiceHost != null)
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
            return;
        }

        public IEnvControlServiceCallback GetEnvControlClient(int stageindex = -1)
        {
            return ServiceHost.GetEnvControlClient(stageindex);
        }
        
        public bool IsUsingChiller(int stageindex = -1)
        {
            bool retVal = false;
            try
            {
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
                retVal = ((int)this.EnvControlManager().DryAirManager.GetMode(stageindex) & (int)ModuleEnableType.ENABLE) == (int)ModuleEnableType.ENABLE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool GetIsExcute()
        {
            try
            {
                return true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        #region ... Valve
        public EventCodeEnum SetValveState(bool enableFlag, EnumValveType valveType, int stageIndex = -1)
        {
            return this.EnvControlManager().SetValveState(enableFlag, valveType, stageIndex);
        }
        public bool GetValveState(EnumValveType valveType, int stageIndex = -1)
        {
            return this.EnvControlManager().GetValveState(valveType, stageIndex);
        }
        #endregion


        #region ... Dry Air
        public byte[] GetDryAirParam(int stageindex = -1)
        {
            return this.EnvControlManager().GetDryAirParam(stageindex);
        }
        public EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1)
        {
            return this.EnvControlManager().DryAirForProber(value, dryairType, stageIndex);
        }
        public int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageindex = -1)
        {
            return this.EnvControlManager().GetLeakSensor(out value, leakSensorIndex, stageindex);
        }
        #endregion

        #region ... FFU
        public void RaiseFFUAlarm(string alarmmessage)
        {
            //LoaderSystem
            if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
            {
                this.GetGPLoader().LoaderLampSetState(ModuleStateEnum.ERROR);   // 경광등
                LoggerManager.Debug($"FFUError LoaderLampOn");
            }
            string[] splitAlarmMessage = alarmmessage.Split(new string[] { "/" }, StringSplitOptions.None);
            string title = splitAlarmMessage[0];
            string content = splitAlarmMessage[1];
            LoggerManager.Debug($"Raise FFUAlarm Message");
            this.MetroDialogManager().ShowMessageDialog(title, content,
            MetroDialogInterfaces.EnumMessageStyle.Affirmative);
        }
        #endregion
    }
}
