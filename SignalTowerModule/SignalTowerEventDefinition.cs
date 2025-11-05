using Newtonsoft.Json;
using NotifyEventModule;
using ProberInterfaces;
using ProberInterfaces.SignalTower;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SignalTowerModule
{
    [Serializable]
    public abstract class SignalTowerEventDefinition : IParamNode
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

        public abstract string EventFullName { get; set; }        
        public virtual string ReverseEventFullName { get; set; }

        public abstract EnumSignalTowerState RedLampStatus { get; set; }
        public abstract EnumSignalTowerState GreenLampStatus { get; set; }
        public abstract EnumSignalTowerState YellowLampStatus { get; set; }

        public abstract EnumSignalTowerState BuzzerStatus { get; set; }
 

        public SignalTowerEventDefinition()
        {

        }
    }

    public class LotStartSignal : SignalTowerEventDefinition
    {
        public LotStartSignal() { }

        public override string EventFullName { get; set; } = typeof(LotStartEvent).FullName;        
        public override EnumSignalTowerState RedLampStatus { get; set; } = EnumSignalTowerState.OFF;
        public override EnumSignalTowerState GreenLampStatus { get; set; } = EnumSignalTowerState.ON;
        public override EnumSignalTowerState YellowLampStatus { get; set; } = EnumSignalTowerState.OFF;
        public override EnumSignalTowerState BuzzerStatus { get; set; } = EnumSignalTowerState.OFF;
        public override string ReverseEventFullName { get; set; } = typeof(LotEndEvent).FullName;
    }

    public class LotEndSignal : SignalTowerEventDefinition
    {
        public LotEndSignal() { }
        
        //R off G Blinkon Y off BU off
        public override string EventFullName { get; set; } = typeof(LotEndEvent).FullName;        
        public override EnumSignalTowerState RedLampStatus { get; set; } = EnumSignalTowerState.OFF;
        public override EnumSignalTowerState GreenLampStatus { get; set; } = EnumSignalTowerState.OFF;
        public override EnumSignalTowerState YellowLampStatus { get; set; } = EnumSignalTowerState.BLINK;
        public override EnumSignalTowerState BuzzerStatus { get; set; } = EnumSignalTowerState.OFF;
    }
    
    public class LotResumeSignal : SignalTowerEventDefinition
    {
        public LotResumeSignal() { }

        public override string EventFullName { get; set; } = typeof(LotResumeEvent).FullName;        
        public override EnumSignalTowerState RedLampStatus { get; set; } = EnumSignalTowerState.OFF;
        public override EnumSignalTowerState GreenLampStatus { get; set; } = EnumSignalTowerState.ON;
        public override EnumSignalTowerState YellowLampStatus { get; set; } = EnumSignalTowerState.OFF;
        public override EnumSignalTowerState BuzzerStatus { get; set; } = EnumSignalTowerState.OFF;
    }

    public class LotPausedSignal : SignalTowerEventDefinition
    {
        public override string EventFullName { get; set; } = typeof(LotPausedEvent).FullName;
        public override EnumSignalTowerState RedLampStatus { get; set; } = EnumSignalTowerState.BLINK;
        public override EnumSignalTowerState GreenLampStatus { get; set; } = EnumSignalTowerState.OFF;
        public override EnumSignalTowerState YellowLampStatus { get; set; } = EnumSignalTowerState.OFF;
        public override EnumSignalTowerState BuzzerStatus { get; set; } = EnumSignalTowerState.ON;
        public override string ReverseEventFullName { get; set; } = typeof(LotResumeEvent).FullName;
    }    
}
