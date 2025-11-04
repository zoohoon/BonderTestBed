using System;
using System.Collections.Generic;
using System.Linq;

namespace PIVManagerModule
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;

    public class PIVManager : IFactoryModule , IPIVManager
    {
        private PIVParameter _PIVSysParam;

        public PIVParameter PIVSysParam
        {
            get { return _PIVSysParam; }
            set { _PIVSysParam = value; }
        }

        private List<FoupStateConvertParam> _FoupStateParams
             = new List<FoupStateConvertParam>();
        public List<FoupStateConvertParam> FoupStateParams
        {
            get { return _FoupStateParams; }
            set { _FoupStateParams = value; }
        }

        private List<StageStateConvertParam> _StageStateParams
             = new List<StageStateConvertParam>();
        public List<StageStateConvertParam> StageStateParams
        {
            get { return _StageStateParams; }
            set { _StageStateParams = value; }
        }

        private List<PreHeatStateConvertParam> _PreHeatStateParams
            = new List<PreHeatStateConvertParam>();
        public List<PreHeatStateConvertParam> PreHeatStateParams
        {
            get { return _PreHeatStateParams; }
            set { _PreHeatStateParams = value; }
        }


        #region <remarks> System Parameter Load & Save </remarks>

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new PIVParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(PIVParameter));

                if (retVal == EventCodeEnum.NONE)
                {
                    PIVSysParam = tmpParam as PIVParameter;
                    FoupStateParams = PIVSysParam.FoupStates.ToList<FoupStateConvertParam>();
                    StageStateParams = PIVSysParam.StageStates.ToList<StageStateConvertParam>();
                    PreHeatStateParams = PIVSysParam.PreHeatStates.ToList<PreHeatStateConvertParam>();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.SaveParameter(PIVSysParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        #endregion

        public int GetFoupState(GEMFoupStateEnum foupStateEnum)
        {
            int retVal = -1;
            try
            {
                var stateVal = FoupStateParams.Find(sparam => sparam.State == foupStateEnum);
                if(stateVal != null)
                {
                    if(stateVal.IsEnable == true)
                    {
                        retVal = stateVal.Value;
                    }    
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public int GetStageState(GEMStageStateEnum stageStateEnum)
        {
            int retVal = -1;
            try
            {
                var stateVal = StageStateParams.Find(sparam => sparam.State == stageStateEnum);
                if (stateVal != null)
                {
                    if (stateVal.IsEnable == true)
                    {
                        retVal = stateVal.Value;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public int GetPreHeatState(GEMPreHeatStateEnum preHeatStateEnum)
        {
            int retVal = -1;
            try
            {
                var stateVal = PreHeatStateParams.Find(sparam => sparam.State == preHeatStateEnum);
                if (stateVal != null)
                {
                    if (stateVal.IsEnable == true)
                    {
                        retVal = stateVal.Value;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
    }
}
