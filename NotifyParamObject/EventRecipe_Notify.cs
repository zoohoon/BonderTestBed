using LogModule;
using Newtonsoft.Json;
using ProberInterfaces;
using ProberInterfaces.Event;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NotifyParamObject
{
    [Serializable]
    public class EventRecipe_Notify : EventRecipe
    {
        public EventRecipe_Notify()
        {
            try
            {
                DLLPath = @"EXT";

                EventList = new List<EventComponent>();
                EventRecipeList_Notify = new List<INotifyEvent>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        [XmlIgnore, JsonIgnore]
        public List<INotifyEvent> EventRecipeList_Notify { get; set; }
    }

    [Serializable]
    public class EventComponent_Notify : EventComponent
    {
        public override string EventName { get; set; }
        public override AssemblyInfo AssemblyInfo { get; set; }
        public override List<string> EventFullNameList { get; set; }

        public EventComponent_Notify()
        {

        }

        public EventComponent_Notify(string eventname, AssemblyInfo assemblyinfo)
        {
            try
            {
                this.EventName = eventname;
                this.AssemblyInfo = new AssemblyInfo(assemblyinfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
