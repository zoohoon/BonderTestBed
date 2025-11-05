using Autofac;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using System;
using LogModule;

namespace LoaderBase.Commands
{
    public class GPLotOpStart : ProbeCommand, IGPLotOpStart
    {
        public override bool Execute()
        {
            try
            {
                ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                return SetCommandTo(loaderMaster);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class GPLotOpPause : ProbeCommand, IGPLotOpPause
    {
        public override bool Execute()
        {
            try
            {
                ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                return SetCommandTo(loaderMaster);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class GPLotOpResume : ProbeCommand, IGPLotOpResume
    {
        public override bool Execute()
        {
            try
            {
                ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                return SetCommandTo(loaderMaster);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class GPLotOpEnd : ProbeCommand, IGPLotOpEnd
    {
        public override bool Execute()
        {
            try
            {
                ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                return SetCommandTo(loaderMaster);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

}
