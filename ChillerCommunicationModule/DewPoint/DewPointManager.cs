using System;

namespace ControlModules
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Temperature.DewPoint;
    using Temperature.Temp.DewPoint;

    public class DewPointManager : IDewPointManager
    {
        #region ..Property
        public bool Initialized { get; set; } = false;
        private IDewPointModule _DewPointModule;
        public IDewPointModule DewPointModule
        {
            get { return _DewPointModule; }
            set
            {
                _DewPointModule = value;
            }
        }

        #endregion

        public DewPointManager()
        {

        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                DewPointModule = new DewPointReader();
                retVal = DewPointModule.LoadSysParameter();
                retVal = DewPointModule.InitModule();
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
                if (DewPointModule != null)
                {
                    DewPointModule.DeInitModule();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return;
        }

        public double GetDewPoint(int stageindex)
        {
            double val = 0.0;
            try
            {
                if(stageindex != -1)
                {
                    var client = this.EnvControlManager().GetEnvControlClient(stageindex);
                    val = client.GetDewPointVal();
                }
                else
                {
                    val = DewPointModule.CurDewPoint;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return val;
        }

    }
}
