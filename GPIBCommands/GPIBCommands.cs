using LogModule;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using System;

namespace Command.GPIB
{


    [Serializable]
    public abstract class GpibCommandBase : ProbeCommand
    {
    }

    [Serializable]
    public class SRQ_RESPONSE : GpibCommandBase, ISRQ_RESPONSE
    {
        public override bool Execute()
        {
            bool RetVal = false;
            try
            {
                IGPIB GPIB = this.GPIB();
                GpibSrqParam param = this.Parameter as GpibSrqParam;

                if (param != null)
                {
                    GPIB.WriteSTB(param.StbNumber);
                    RetVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
    }

    [Serializable]
    public class GpibAbort : GpibCommandBase, IGpibAbort
    {
        public override bool Execute()
        {
            bool RetVal = true;
            try
            {
                IGPIB GPIB = this.GPIB();

                GPIB.DisConnect();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                
            }
            return RetVal;
        }
    }
}
