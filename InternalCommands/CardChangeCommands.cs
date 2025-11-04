using LogModule;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using SequenceRunner;
using System;

namespace ProberInterfaces.CardChange
{
    public class RunCardChangeCommand : ProbeCommand, IRUNCARDCHANGECOMMAND
    {
        public override bool Execute()
        {
            try
            {
                return true;
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
    public class StagecardChangeStartCommand : ProbeCommand, IStagecardChangeStart
    {
        public override bool Execute()
        {
            try
            {
                var CCCmdParam = this.Parameter as SequenceBehaviors;
                ICardChangeModule CCModule = this.CardChangeModule();
                CCModule.ReleaseWaitForCardPermission();//cardload 또는 unload 시작
                var CCController = CCModule.GetSequence(CCCmdParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return SetCommandTo(this.CardChangeModule());
        }
    }

    public class AbortCardChangeCommand : ProbeCommand, IAbortcardChange
    {
        public override bool Execute()
        {
            return SetCommandTo(this.WaferTransferModule());
        }
    }
    
}
