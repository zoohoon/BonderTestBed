using System;

namespace ProberInterfaces.Event
{
    public interface INotifyEvent : IProbeEvent
    {
        event EventHandler ProbeEventSubscibers;
        int EventNumber { get; set; }
    }
}
