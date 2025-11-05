
using System;
using System.Collections.Generic;

namespace ProberInterfaces.Event
{
    using ProberErrorCode;
    using ProberInterfaces.Event.EventProcess;
    using System.Threading;

    public class ProbeEventArgs : EventArgs
    {
        public Object Sender { get; set; }
        public SemaphoreSlim Semaphore { get; set; }

        public Object Parameter { get; set; }
        public ProbeEventArgs()
        {
        }
        public ProbeEventArgs(Object sender)
        {
            Sender = sender;
        }
        public ProbeEventArgs(Object sender, SemaphoreSlim semaphore)
        {
            Sender = sender;
            Semaphore = semaphore;
        }
        public ProbeEventArgs(Object sender, SemaphoreSlim semaphore, Object parameter)
        {
            Sender = sender;
            Semaphore = semaphore;
            Parameter = parameter;
        }

        public string WriteLogCmdArgument() 
        {
            return String.Empty;
        }

        public bool ParseLogCmdArgument(string log)
        {
            return true;
        }

        public void SamaphoreRelease()
        {
            try
            {
                Semaphore?.Release();
            }
            catch (Exception)
            {
            }
        }
    }

    public class ProbeEventInfo
    {
        public string HashCode { get; set; }
        public string description { get; set; }
        public IProbeEvent Event { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EventCodeEnum Status { get; set; }
        public ProbeEventType eventtype { get; set; }
        public Queue<ProbeEventArgs> EventArgQueue { get; set; }
    }

    //public class ProbeEvent
    //{
    //    Guid guid { get; set; }
    //    string description { get; set; }
    //    DateTime StartTime { get; set; }
    //    EventCodeEnum Status { get; set; }
    //    ProbeEventType eventtype { get; set; }
    //    ProbeEventLevel eventlevel { get; set; }
    //}
    public interface IProbeEventSubscriber
    {
        EventProcessList SubscribeRecipeParam { get; set; }
        EventCodeEnum RegistEventSubscribe();
    }
    
    public interface IProbeEvent : IFactoryModule
    {
        EventCodeEnum DoEvent(ProbeEventArgs args = null);
        Queue<ProbeEventArgs> EventArgQueue { get; set; }
    }

    public enum ProbeEventType
    {
        UNKNOWN = 0,
        NOTIFY,
        INTERNAL,
        GPIB,
        GEM,
        MANUAL
    }

    public enum ProbeEventLevel
    {
        NOTIFY = 0,
        ERROR,
        ARARM,
        WARNING
    }
}
