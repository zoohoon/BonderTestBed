using LogModule;
using Newtonsoft.Json;
using ProberInterfaces;
using ProberInterfaces.Event;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace InternalParamObject
{
    [Serializable]
    public class EventRecipe_Internal : EventRecipe
    {
        public EventRecipe_Internal()
        {
            try
            {
                DLLPath = @"EXT";

                EventList = new List<EventComponent>();
                EventRecipeList_Internal = new List<IInternalEvent>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        [XmlIgnore, JsonIgnore]
        public List<IInternalEvent> EventRecipeList_Internal { get; set; }
    }

    [Serializable]
    public class EventComponent_Internal : EventComponent
    {
        public override string EventName { get; set; }
        public override AssemblyInfo AssemblyInfo { get; set; }
        public override List<string> EventFullNameList { get; set; }

        public EventComponent_Internal()
        {

        }

        public EventComponent_Internal(string eventname, AssemblyInfo assemblyinfo)
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
