using System;
using ProberInterfaces.Command;
using ProberInterfaces;
using ProberInterfaces.Command.Internal;
using LogModule;

namespace Command.Internal
{
    public class LoaderOpStart : ProbeCommand, ILoaderOpStart
    {
        public override bool Execute()
        {
            try
            {
                ILoaderOPModule LoaderOp = this.LoaderOPModule();

                return SetCommandTo(LoaderOp);
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
    
    public class LoaderOpPause : ProbeCommand, ILoaderOpPause
    {
        public LoaderOpPause()
        {
        }

        public override bool Execute()
        {
            try
            {
                ILoaderOPModule LoaderOp = this.LoaderOPModule();

                return SetCommandTo(LoaderOp);
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class LoaderOpResume : ProbeCommand, ILoaderOpResume
    {
        public LoaderOpResume()
        {
        }

        public override bool Execute()
        {
            try
            {
                ILoaderOPModule LoaderOp = this.LoaderOPModule();

                return SetCommandTo(LoaderOp);
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class LoaderOpEnd : ProbeCommand, ILoaderOpEnd
    {
        public LoaderOpEnd()
        {
        }

        public override bool Execute()
        {
            try
            {
                ILoaderOPModule LoaderOp = this.LoaderOPModule();

                return SetCommandTo(LoaderOp);
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
    
}
