using LogModule;
using Newtonsoft.Json;
using NotifyEventModule;
using ProberErrorCode;
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
    public class SignalTowerEventParameter : ISystemParameterizable, IParamNode, IParam
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

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

        [XmlIgnore, JsonIgnore]
        public String Genealogy { get; set; }

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner { get; set; }

        [XmlIgnore, JsonIgnore]
        public String FilePath { get; } = "";
        [XmlIgnore, JsonIgnore]
        public String FileName { get; } = "SignalTowerEventParameter.json";              
        
        private List<SignalTowerEventDefinition> _SignalTowerEvents = new List<SignalTowerEventDefinition>();
        public List<SignalTowerEventDefinition> SignalTowerEvents
        {
            get { return _SignalTowerEvents; }
            set { _SignalTowerEvents = value; }
        }

        public SignalTowerEventParameter()
        {

        }        

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            try
            {
                //=====> LotStartEvent + R off G on Y off B off
                _SignalTowerEvents.Add(new LotStartSignal());

                //=====> LotEndEvent + R off G off Y blinkon B off
                _SignalTowerEvents.Add(new LotEndSignal());                

                //=====> LotResumeEvent R off G on Y off B off
                _SignalTowerEvents.Add(new LotResumeSignal());

                //=====> LotPausedEvent R blinkon G off Y off B on
                _SignalTowerEvents.Add(new LotPausedSignal());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
    }
}
