using System;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.WaferTransfer;
using LogModule;

namespace Command.Internal
{
    public class ChuckLoadCommand : ProbeCommand, IChuckLoadCommand
    {
        public ChuckLoadCommand()
        {

        }
        public override bool Execute()
        {
            try
            {
                IWaferTransferModule module = this.WaferTransferModule();

                return SetCommandTo(module);
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class ChuckUnloadCommand : ProbeCommand, IChuckUnloadCommand
    {
        public override bool Execute()
        {
            try
            {
                IWaferTransferModule module = this.WaferTransferModule();

                return SetCommandTo(module);
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }


    public class CardLoadCommand : ProbeCommand, ICardLoadCommand
    {
        public CardLoadCommand()
        {

        }
        public override bool Execute()
        {
            try
            {
                IWaferTransferModule module = this.WaferTransferModule();

                return SetCommandTo(module);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class CardUnloadCommand : ProbeCommand, ICardUnloadCommand
    {
        public override bool Execute()
        {
            try
            {
                IWaferTransferModule module = this.WaferTransferModule();

                return SetCommandTo(module);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
