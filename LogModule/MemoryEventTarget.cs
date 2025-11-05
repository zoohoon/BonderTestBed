using System;

namespace LogModule
{
    using NLog;

    [NLog.Targets.Target("MemoryEvent")]
    public class MemoryEventTarget : NLog.Targets.Target
    {
        public event Action<LogEventInfo> EventReceived;
        protected override void Write(LogEventInfo logEvent)
        {
            if (EventReceived != null)
            {
                EventReceived(logEvent);
            }
        }
    }
}
