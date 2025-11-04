using System;
using ProberInterfaces.Command;
using ProberInterfaces.LoaderController;
using ProberInterfaces.Command.Internal;
using ProberInterfaces;
using LogModule;

namespace Command.Internal
{
    public class LoaderMapCommand : ProbeCommand, ILoaderMapCommand
    {
        public override bool Execute()
        {
            try
            {
                ILoaderController LoaderController = this.LoaderController();

                return SetCommandTo(LoaderController);
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
