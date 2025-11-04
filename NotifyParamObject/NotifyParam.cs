using System;
using System.Collections.Generic;
using ProberInterfaces;
using ProberInterfaces.Event;
using System.ComponentModel;
using System.Xml.Serialization;
using ProberErrorCode;
using NotifyEventModule;
using System.Reflection;
using LogModule;
using Newtonsoft.Json;

namespace NotifyParamObject
{
    public class NotifyParam : INotifyParameters
    {
        public NotifyRecipeParam NotifyRecipeParams { get; set; }

        public NotifyEventRecipeParam NotifyEventRecipeParams { get; set; }

        public List<INotifyEvent> EventRecipeList_Notify { get => NotifyEventRecipeParams.EventRecipe_Notify.EventRecipeList_Notify; }

        public List<EventComponent> EventList { get => NotifyEventRecipeParams.EventRecipe_Notify.EventList; }
        public Dictionary<string, string> NotifyRecipe { get => NotifyRecipeParams.NotifyRecipe; }

        [Serializable]
        public class NotifyRecipeParam : ISystemParameterizable, INotifyPropertyChanged
        {
            [JsonIgnore, ParamIgnore]
            public bool IsParamChanged { get; set; }
            [JsonIgnore]
            public List<object> Nodes { get; set; }
            [JsonIgnore]
            public string Genealogy { get; set; }
            [NonSerialized]
            private Object _Owner;
            [XmlIgnore, JsonIgnore, ParamIgnore]
            public Object Owner
            {
                get { return _Owner; }
                set
                {
                    if (_Owner != value)
                    {
                        _Owner = value;
                    }
                }
            }


            public EventCodeEnum Init()
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                try
                {
                    retval = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    

                    retval = EventCodeEnum.PARAM_ERROR;
                }

                return retval;
            }
            public void SetElementMetaData()
            {

            }

            [field:NonSerialized, JsonIgnore]
            public event PropertyChangedEventHandler PropertyChanged;
            private void NotifyPropertyChanged(String info)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                }
            }

            //

            public string FilePath { get; } = "Event";

            public string FileName { get; } = "Recipe_Notify.json";
            private Dictionary<string, string> _NotifyRecipe;
            public Dictionary<string, string> NotifyRecipe
            {
                get { return _NotifyRecipe; }
                set
                {
                    if (value != _NotifyRecipe)
                    {
                        _NotifyRecipe = value;
                        NotifyPropertyChanged("NotifyRecipe");
                    }
                }
            }

            public NotifyRecipeParam()
            {
                NotifyRecipe = new Dictionary<string, string>();
            }
            public EventCodeEnum SetEmulParam()
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                try
                {
                retVal = SetDefaultParam();

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                     throw;
                }
                return retVal;
            }
            public EventCodeEnum SetDefaultParam()
            {
                EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

                try
                {
                    if (NotifyRecipe == null)
                    {
                        NotifyRecipe = new Dictionary<string, string>();
                    }

                    string CommandName = nameof(CassetteLoadDoneEvent);
                    string tmp1 = typeof(CassetteLoadDoneEvent).FullName;// "NotifyEventModule.CassetteLoadDoneEvent";
                    NotifyRecipe.Add(CommandName, tmp1);
                    RetVal = EventCodeEnum.NONE;
                }
                catch(Exception err)
                {
                    throw new Exception($"Error during Setting Default Param From NotifyRecipeParam. {err.Message}");
                }

                return RetVal;
            }
        }

        [Serializable]
        public class NotifyEventRecipeParam : ISystemParameterizable, INotifyPropertyChanged
        {
            [JsonIgnore, ParamIgnore]
            public bool IsParamChanged { get; set; }
            public List<object> Nodes { get; set; }
            public string Genealogy { get; set; }
            [NonSerialized]
            private Object _Owner;
            [XmlIgnore, JsonIgnore, ParamIgnore]
            public Object Owner
            {
                get { return _Owner; }
                set
                {
                    if (_Owner != value)
                    {
                        _Owner = value;
                    }
                }
            }


            public EventCodeEnum Init()
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                try
                {
                    retval = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"[NotifyEventRecipeParam] [Method = Init] [Error = {err}]");
                    retval = EventCodeEnum.PARAM_ERROR;
                }

                return retval;
            }

            [field: NonSerialized, JsonIgnore]
            public event PropertyChangedEventHandler PropertyChanged;
            private void NotifyPropertyChanged(String info)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                }
            }
            public void SetElementMetaData()
            {

            }
            //

            public string FilePath { get; } = "Event";

            public string FileName { get; } = "EventRecipe_Notify.Json";
            private EventRecipe_Notify _EventRecipe_Notify;
            public EventRecipe_Notify EventRecipe_Notify
            {
                get { return _EventRecipe_Notify; }
                set
                {
                    if (value != _EventRecipe_Notify)
                    {
                        _EventRecipe_Notify = value;
                        NotifyPropertyChanged("EventRecipe_Notify");
                    }
                }
            }

            public NotifyEventRecipeParam()
            {
                EventRecipe_Notify = new EventRecipe_Notify();
            }

            public EventComponent_Notify MakeNotifyEventObject(string EventName, string AssemblyName, int AssemblyVersion)
            {
                EventComponent_Notify EventComp;
                try
                {
                AssemblyInfo AssemblyInfo = new AssemblyInfo();

                AssemblyInfo.AssemblyName = AssemblyName;
                AssemblyInfo.Version = AssemblyVersion;

                EventComp = new EventComponent_Notify(EventName, AssemblyInfo);

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                     throw;
                }
                return EventComp;

            }
            public EventCodeEnum SetEmulParam()
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                try
                {
                retVal = SetDefaultParam();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                     throw;
                }
                return retVal;
            }
            public EventCodeEnum SetDefaultParam()
            {
                EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

                try
                {
                    Assembly assemObj = Assembly.GetExecutingAssembly();

                    _EventRecipe_Notify.EventList.Add(MakeNotifyEventObject(nameof(CassetteLoadDoneEvent), "NotifyEvent.dll", 1000));
                    RetVal = EventCodeEnum.NONE;
                }
                catch(Exception err)
                {
                    throw new Exception($"Error during Setting Default Param From NotifyParam. {err.Message}");
                }

                return RetVal;
            }
        }
    }
}
