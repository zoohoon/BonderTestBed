using System;
using System.Collections.Generic;

namespace ProberInterfaces.Lamp
{
    using Newtonsoft.Json;
    using ProberInterfaces;
    using System.Xml.Serialization;

    /*
     * 램프를 켜는 조합, Lamp Manager는 Lamp Combination을 통해 Lamp를 어떻게 켤지 결정 한다.
     */
    [Serializable]
    public abstract class LampCombination : IParamNode
    {
        [XmlIgnore, JsonIgnore]
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
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        public String ID { get; set; }

        private Element<LampStatusEnum> _RedLampStatus = new Element<LampStatusEnum>();
        public Element<LampStatusEnum> RedLampStatus
        {
            get { return _RedLampStatus; }
            set { _RedLampStatus = value; }
        }
        private Element<LampStatusEnum> _YellowLampStatus = new Element<LampStatusEnum>();
        public Element<LampStatusEnum> YellowLampStatus
        {
            get { return _YellowLampStatus; }
            set { _YellowLampStatus = value; }
        }
        private Element<LampStatusEnum> _BlueLampStatus = new Element<LampStatusEnum>();
        public Element<LampStatusEnum> BlueLampStatus
        {
            get { return _BlueLampStatus; }
            set { _BlueLampStatus = value; }
        }
        private Element<LampStatusEnum> _BuzzerStatus = new Element<LampStatusEnum>();
        public Element<LampStatusEnum> BuzzerStatus
        {
            get { return _BuzzerStatus; }
            set { _BuzzerStatus = value; }
        }

        //==> 램프 조합은 우선순위를 가진다. 우선순위에 따라 먼저 Lamp Manager는 먼저 처리 해야할 랩프 조합을 킨다.
        private Element<AlarmPriority> _Priority = new Element<AlarmPriority>();
        public Element<AlarmPriority> Priority
        {
            get { return _Priority; }
            set { _Priority = value; }
        }

        //==> Serialize/Deserialize 때문에 필요
        public LampCombination()
        {

        }
        public LampCombination(
            LampStatusEnum redLampStatus,
            LampStatusEnum yellowLampStatus,
            LampStatusEnum blueLampStatus,
            LampStatusEnum buzzerStatus,
            AlarmPriority priority,
            String id)
        {
            RedLampStatus.Value = redLampStatus;
            YellowLampStatus.Value = yellowLampStatus;
            BlueLampStatus.Value = blueLampStatus;
            BuzzerStatus.Value = buzzerStatus;
            Priority.Value = priority;
            ID = id;
        }
    }
}
