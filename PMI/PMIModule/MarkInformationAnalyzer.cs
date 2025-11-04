using LogModule;
using ProberErrorCode;
using ProberInterfaces.PMI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMIModule
{
    //public enum MarkStatusCodeEnum
    //{
    //    PASS = 0,
    //    TOO_CLOSE_TO_EDGE,
    //    NO_PROBE_MARK,
    //    MARK_AREA_TOO_SMALL,
    //    MARK_AREA_TOO_BIG,
    //    MARK_SIZE_TOO_SMALL,
    //    MARK_SIZE_TOO_BIG,
    //    TOO_MANY_PROBE_MARK,
    //}

    public abstract class AnalysisType
    {
        
    }

    public class PMIMarkInformationAnalyzer : IPMIMarkInformationAnalyzer
    {
        public bool Initialized { get; set; } = false;

        public void DeInitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }

        public List<MarkStatusCodeEnum> GetMarkStatusCode()
        {
            List<MarkStatusCodeEnum> retval = new List<MarkStatusCodeEnum>();

            try
            {
                if(IsCloseTothePadEdge())
                {
                    retval.Add(MarkStatusCodeEnum.TOO_CLOSE_TO_EDGE);
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retval;
        }

        public bool IsCloseTothePadEdge()
        {
            bool retval = false;

            return retval;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


    }
}
