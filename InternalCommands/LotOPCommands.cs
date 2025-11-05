using System;
using ProberInterfaces.Command;
using ProberInterfaces;
using ProberInterfaces.Command.Internal;
using LogModule;

namespace Command.Internal
{
    public class LotOpStart : ProbeCommand, ILotOpStart
    {
        public override bool Execute()
        {
            try
            {
                ILotOPModule LOTOP = this.LotOPModule();

                return SetCommandTo(LOTOP);
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class LotOpPause : ProbeCommand, ILotOpPause
    {
        public override bool Execute()
        {
            try
            {
                ILotOPModule LOTOP = this.LotOPModule();

                return SetCommandTo(LOTOP);
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class LotOpResume : ProbeCommand, ILotOpResume
    {
        public override bool Execute()
        {
            try
            {
                ILotOPModule LOTOP = this.LotOPModule();

                return SetCommandTo(LOTOP);
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class LotOpEnd : ProbeCommand, ILotOpEnd
    {
        public override bool Execute()
        {
            try
            {
                ILotOPModule LOTOP = this.LotOPModule();
                LOTOP.IsNeedLotEnd = true;

                return SetCommandTo(LOTOP);
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }



    public class UnloadAllWafer : ProbeCommand, IUnloadAllWafer
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
