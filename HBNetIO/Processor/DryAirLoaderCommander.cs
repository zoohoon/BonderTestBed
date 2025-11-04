using System;

namespace HBDryAir.Processor
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Temperature.DryAir;
    public class DryAirLoaderCommander : IDryAirController
    {
        #region ..Property

        #endregion

        #region .. Init
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

        public EventCodeEnum InitConnect()
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

        #endregion

        #region .. Method

        #endregion

        public EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(dryairType == EnumDryAirType.STG)
                {
                    retVal = this.EnvControlManager().SetValveState(value, EnumValveType.DRYAIR, stageIndex);
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
                if(dryairType == EnumDryAirType.STG)
                {
                    retVal = this.EnvControlManager().GetValveState(EnumValveType.DRYAIR, stageIndex);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageindex = -1)
        {
            int retVal = -1;
            value = false;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public byte[] GetDryAirParam(int stageindex = -1)
        {
            return null;
        }

    }
}
