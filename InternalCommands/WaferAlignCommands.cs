using System;
using LogModule;

namespace InternalCommands
{
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;

    public class DoWaferAlign : ProbeCommand, IDOWAFERALIGN
    {
        public override bool Execute()
        {
            try
            {
                IProberStation prober = this.ProberStation();
                prober.ChangeScreenLotOP();
                return SetCommandTo(this.WaferAligner());
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class DoManualWaferAlign : ProbeCommand, IDoManualWaferAlign
    {
        public override bool Execute()
        {
            bool retVal;

            retVal = SetCommandTo(this.WaferAligner());

            return retVal;
        }
    }

    
}
