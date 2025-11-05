using System;

namespace ControlModules.Valve.Controller
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    public class ValveRemoteController : IValveController
    {
        #region ..Property
        public bool Initialized { get; set; } = false;
        #endregion

        #region .. Init & DeInit

        public EventCodeEnum InitModule()
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
        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }
        #endregion

        #region ..Method
        public bool GetValveState(EnumValveType valveType, int stageIndex = -1)
        {
            try
            {
                return false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public EventCodeEnum SetValveState(bool state, EnumValveType valveType, int stageIndex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                /*
                switch (valveType)
                {
                    case EnumValveType.INVALID:
                        break;
                    case EnumValveType.UNDEFINED:
                        break;
                    case EnumValveType.IN:
                        break;
                    case EnumValveType.OUT:
                        break;
                    case EnumValveType.PRUGE:
                        break;
                    case EnumValveType.DRYAIR:
                        break;
                    default:
                        break;
                }
                */
                //LoggerManager.Debug($"Valve [{valveType}] state set as {state}.");
                retVal = EventCodeEnum.NONE;
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

        #endregion
    }
}
