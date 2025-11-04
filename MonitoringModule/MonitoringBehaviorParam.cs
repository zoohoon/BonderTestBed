namespace MonitoringModule
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.Monitoring;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    public class MonitoringBehaviorParam : ISystemParameterizable
    {
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
            }
            return retval;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                SetDefaultParam();
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                MonitoringBehaviorList = new List<IMonitoringBehavior>();
                MonitoringBehaviorList.Add(new CheckMainAirFromLoader());
                MonitoringBehaviorList.Add(new CheckMainVacFromLoader());
                MonitoringBehaviorList.Add(new CheckEMGFromLoader());
                MonitoringBehaviorList.Add(new CheckStageAxesState());
                MonitoringBehaviorList.Add(new CheckChuckVacuum());
                switch (this.StageSupervisor().CardChangeModule().GetCCType())
                {
                    case EnumCardChangeType.DIRECT_CARD:
                        MonitoringBehaviorList.Add(new CheckCardStuck());
                        MonitoringBehaviorList.Add(new CheckTesterVac());
                        break;
                    default:
                        break;
                }
                MonitoringBehaviorList.Add(new CheckBackSideDoor());
                MonitoringBehaviorList.Add(new CheckCurrentofThreePod());
                if(this.IOManager().IO.Inputs.DITESTERHEAD_PURGE.IOOveride.Value == EnumIOOverride.NONE)
                {
                    MonitoringBehaviorList.Add(new CheckTesterHeadPurge());
                }



                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public void SetElementMetaData()
        {

        }

        public string FilePath { get; } = "";

        public string FileName { get; } = "MonitoringBehaviors.json";

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
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
        public List<object> Nodes { get; set; }


        private List<IMonitoringBehavior> _MonitoringBehaviorList;
        public List<IMonitoringBehavior> MonitoringBehaviorList
        {
            get { return _MonitoringBehaviorList; }
            set
            {
                if (value != _MonitoringBehaviorList)
                {
                    _MonitoringBehaviorList = value;
                }
            }
        }
    }
}
