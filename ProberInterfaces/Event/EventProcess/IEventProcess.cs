namespace ProberInterfaces.Event.EventProcess
{
    using ProberErrorCode;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    public interface IEventProcess
    {
        string EventFullName { get; set; }
        object Parameter { get; set; }

        void EventNotify(object sender, ProbeEventArgs e);
    }

    [Serializable]
    public abstract class EventProcessBase : IEventProcess, IFactoryModule
    {
        [XmlElement]
        public string EventFullName { get; set; }
        // eventId를 정의하는 기준: 현재는 네자리 숫자로 사용중. 다른곳에 미리 정의되어 있지는 않고 중복되지만 않게 정의하기.
        [XmlElement]
        public object Parameter { get; set; }
        [XmlElement]
        private bool _Enable = true;

        public bool Enable
        {
            get { return _Enable; }
            set { _Enable = value; }
        }
        [XmlElement]
        public EventConditionCheckBase ConditionChecker { get; set; }

        [XmlIgnore, JsonIgnore]
        public String OwnerModuleName { get; set; }

        public abstract void EventNotify(object sender, ProbeEventArgs e);

        //public static Type[] GetAllType()
        //{
        //    List<Type> retList = null;

        //    try
        //    {
        //        var EventProcessTypes = from assembly in AppDomain.CurrentDomain.GetAssemblies()
        //                                from type in assembly.GetTypes()
        //                                where type.IsSubclassOf(typeof(EventProcessBase))
        //                                select type;

        //        retList = EventProcessTypes.ToList();
        //        retList.Add(typeof(GpibEventParam));
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //    }

        //    return retList.ToArray();
        //}
    }

    [Serializable]
    public class EventProcessList : List<EventProcessBase>, IParam
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlElement]
        public string EventFullName { get; set; }
        [XmlElement]
        public object Parameter { get; set; }

        [XmlIgnore, JsonIgnore]
        public String OwnerModuleName { get; set; }

        public string FilePath { get; set; }

        public string FileName { get; set; }

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
        public List<object> Nodes { get; set; } = new List<object>();

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }
    }

    public abstract class EventConditionCheckBase : IFactoryModule
    {
        public abstract bool DoCkeck(ref string checkFailReason);
    }

}
