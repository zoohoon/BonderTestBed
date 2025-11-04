using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.PolishWafer;
using System;
using LogModule;

namespace InternalCommands
{
    public class DoManualPolishWaferCleaning : ProbeCommand, IDoManualPolishWaferCleaning
    {
        public override bool Execute()
        {
            try
            {
                IPolishWaferModule pwmodule = this.PolishWaferModule();
                return SetCommandTo(pwmodule);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
