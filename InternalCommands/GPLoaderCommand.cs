using Autofac;
using LoaderBase;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using System;
using LogModule;

namespace InternalCommands
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

    public class StartCardChangeSequence : ProbeCommand, IStartCardChangeSequence
    {
        public override bool Execute()
        {
            try
            {
                ICardChangeSupervisor cardChangeSupervisor = this.GetLoaderContainer().Resolve<ICardChangeSupervisor>();

                return SetCommandTo(cardChangeSupervisor);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class StartWaferChangeSequence : ProbeCommand, IStartWaferChangeSequence
    {
        public override bool Execute()
        {
            try
            {
                IWaferChangeSupervisor waferChangeSupervisor = this.GetLoaderContainer().Resolve<IWaferChangeSupervisor>();

                return SetCommandTo(waferChangeSupervisor);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class TransferObjectCommand : ProbeCommand, ITransferObject
    {
        public override bool Execute()
        {
            try
            {
                ICardChangeSupervisor cardChangeSupervisor = this.GetLoaderContainer().Resolve<ICardChangeSupervisor>();

                return SetCommandTo(cardChangeSupervisor);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
  

    public class AbortCardChangeSequence : ProbeCommand, IAbortCardChangeSequence
    {
        public override bool Execute()
        {
            try
            {
                ICardChangeSupervisor cardChangeSupervisor = this.GetLoaderContainer().Resolve<ICardChangeSupervisor>();

                return SetCommandTo(cardChangeSupervisor);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
