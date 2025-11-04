using System;

namespace EventProcessModule.EventSoaking
{
    using LogModule;
    using NotifyEventModule;
    using ProberInterfaces;
    using ProberInterfaces.Event;
    using ProberInterfaces.Event.EventProcess;

    public abstract class EventSoakingProcessBase : EventProcessBase
    {
    }
    public class PreHeatSokaingProcess : EventSoakingProcessBase
    {
        public override void EventNotify(object sender, ProbeEventArgs e)
        {
            try
            {
                NotifyEvent eventInfo = sender as NotifyEvent;
                if (eventInfo == null)
                    return;

                LoggerManager.Debug($"[Soaking][{OwnerModuleName} HANDLER] Start...");

                this.SoakingModule().IsPreHeatEvent = true;

                LoggerManager.Debug($"[Soaking][{OwnerModuleName} HANDLER] End...");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
