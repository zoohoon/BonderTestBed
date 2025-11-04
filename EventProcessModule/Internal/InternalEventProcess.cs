

namespace EventProcessModule.Internal
{
    using LogModule;
    using NotifyEventModule;
    using ProberInterfaces.Event;
    using ProberInterfaces.Event.EventProcess;
    using System;

    public abstract class InternalEventProcessBase : EventProcessBase
    {
    }

    public class LotEventProc_CassetteLoadDone : InternalEventProcessBase
    {
        public override void EventNotify(object sender, ProbeEventArgs e)
        {
            try
            {
                NotifyEvent eventInfo = sender as NotifyEvent;
                if (eventInfo == null)
                    return;

                LoggerManager.Debug($"[CassetteLoadDone][{OwnerModuleName} HANDLER] Start...");
                LoggerManager.Debug($"[CassetteLoadDone][{OwnerModuleName} HANDLER] End...");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
    }
}
