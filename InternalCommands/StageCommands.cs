using System;

namespace InternalCommands
{
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;
    using LogModule;

    public class SystemInit : ProbeCommand, ISystemInit
    {
        public override bool Execute()
        {
            try
            {
                ILotOPModule LOTOP = this.LotOPModule();

                return SetCommandTo(LOTOP);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
