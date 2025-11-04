using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using System;
using ProberInterfaces;
using ProberInterfaces.NeedleClean;
using LogModule;

namespace InternalCommands
{
    public class DoNeedleClean : ProbeCommand, IDoNeedleCleaningCommand
    {
        public override bool Execute()
        {
            try
            {
                INeedleCleanModule module = this.NeedleCleaner();

                return SetCommandTo(module);
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
