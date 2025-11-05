using System;
using System.Collections.Generic;
using ProberErrorCode;

namespace ProberInterfaces.Event
{
    public interface IEventRecipe
    {
        EventCodeEnum CreateEventRecipe();

        EventCodeEnum GetEventList();
    }

    [Serializable]
    public abstract class EventRecipe
    {
        public string DLLPath { get; set; }

        public List<EventComponent> EventList { get; set; }

        public EventRecipe()
        {

        }

        public EventRecipe(string dllpath)
        {
            this.DLLPath = dllpath;
        }
    }

    //[Serializable]
    //public abstract class EventCommand
    //{
    //    //public string CommandName { get; set; }

    //    public abstract List<EventComponent> EventComponents { get; set; }
    //    public EventCommand()
    //    {

    //    }
    //}

    [Serializable]
    public abstract class EventComponent
    {
        public EventComponent()
        {
        }

        public abstract AssemblyInfo AssemblyInfo { get; set; }
        public abstract string EventName { get; set; }
        //public abstract List<EventDoInformation> EventDoList { get; set; }
        public abstract List<string> EventFullNameList { get; set; }
    }
}
