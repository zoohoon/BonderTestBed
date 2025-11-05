//using Newtonsoft.Json;
//using ProberInterfaces;
//using ProberInterfaces.Event;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Serialization;
//using LogModule;

//namespace GPIBModule
//{
//    [Serializable]
//    public class EventRecipe_GPIB : EventRecipe
//    {
//        //public override string DLLPath { get; set; } = @"EXT";
//        //public override List<EventCommand> EventCommands { get; set; }

//        public EventRecipe_GPIB()
//        {
//            try
//            {
//                DLLPath = @"EXT";

//                EventList = new List<EventComponent>();
//                EventRecipeList_GPIB = new List<IGPIBEvent>();
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);

//            }
//        }

//        [XmlIgnore, JsonIgnore]
//        public List<IGPIBEvent> EventRecipeList_GPIB { get; set; }
//    }

//    [Serializable]
//    public class EventComponent_GPIB : EventComponent
//    {
//        public override string EventName { get; set; }
//        public override AssemblyInfo AssemblyInfo { get; set; }

//        public List<string> ACK { get; set; }
//        public List<string> NACK { get; set; }

//        public List<string> EventDoInfo { get; set; }
//        public string STBAlias { get; set; }

//        public override List<string> EventFullNameList { get; set; }

//        public EventComponent_GPIB()
//        {

//        }

//        public EventComponent_GPIB(string eventname, AssemblyInfo assemblyinfo)
//        {
//            try
//            {
//                this.EventName = eventname;
//                this.AssemblyInfo = new AssemblyInfo(assemblyinfo);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//        }

//        public EventComponent_GPIB(string eventname, AssemblyInfo assemblyinfo, List<string> eventfullnamelist)
//        {
//            try
//            {
//                this.EventName = eventname;
//                this.AssemblyInfo = new AssemblyInfo(assemblyinfo);

//                this.EventFullNameList = new List<string>(eventfullnamelist);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//        }

//        public EventComponent_GPIB(string eventname, AssemblyInfo assemblyinfo, string STBAlias)
//        {
//            try
//            {
//                this.EventName = eventname;
//                this.AssemblyInfo = new AssemblyInfo(assemblyinfo);
//                this.STBAlias = STBAlias;
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//        }

//        public EventComponent_GPIB(string eventname, AssemblyInfo assemblyinfo, List<string> ACK, List<string> NACK, List<string> EventDoInfo)
//        {
//            try
//            {
//                this.EventName = eventname;
//                this.AssemblyInfo = new AssemblyInfo(assemblyinfo);

//                this.ACK = new List<string>(ACK);
//                this.NACK = new List<string>(NACK);
//                this.EventDoInfo = new List<string>(EventDoInfo);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//        }
//    }
//}
