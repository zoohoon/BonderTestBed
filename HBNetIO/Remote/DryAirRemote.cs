using System;

namespace HBDryAir.Processor
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Temperature.DryAir;
    using RemoteServiceProxy;

    using System.ServiceModel;

    [CallbackBehavior(UseSynchronizationContext = false)]
    public class DryAirRemote : IDryAirController
    {
        #region .. Property

        private DryAirServiceDirectProxy _DryAirServiceProxy;

        public DryAirServiceDirectProxy DryAirServiceProxy
        {
            get { return _DryAirServiceProxy; }
            set { _DryAirServiceProxy = value; }
        }

        private int _RemoteStageIndex;

        public int RemoteStageIndex
        {
            get { return _RemoteStageIndex; }
            set { _RemoteStageIndex = value; }
        }


        #endregion

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                RemoteStageIndex = this.LoaderController().GetChuckIndex();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED ;
            try
            {
                retVal = this.EnvControlManager().DryAirForProber(value, dryairType, RemoteStageIndex);
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
                retVal = this.EnvControlManager().GetValveState(EnumValveType.DRYAIR, RemoteStageIndex);
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
                retVal = this.EnvControlManager().GetLeakSensor(out value, leakSensorIndex, RemoteStageIndex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public byte[] GetDryAirParam(int stageIndex = -1)
        {
            byte[] retVal = null;
            try
            {
                retVal = this.EnvControlManager().GetDryAirParam(RemoteStageIndex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}
