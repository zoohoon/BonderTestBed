using System;
using System.Collections.Generic;

namespace MonitoringModule
{
    using LogModule;
    using MonitoringModule.HardwarePartChecker;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Xml.Serialization;

    [Serializable]
    public class MonitoringSystemParameter : ISystemParameterizable, IParamNode, IParam
    {

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
        [XmlIgnore, JsonIgnore]
        public String Genealogy { get; set; }
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
        public String FilePath { get; } = "";
        [XmlIgnore, JsonIgnore]
        public String FileName { get; } = "MonitoringSystemParameter.json";

        private Element<bool> _MonitoringEnable = new Element<bool>();
        public Element<bool> MonitoringEnable
        {
            get { return _MonitoringEnable; }
            set { _MonitoringEnable = value; }
        }

        private Element<int> _StageAxesTimeout = new Element<int>();
        public Element<int> StageAxesTimeout
        {
            get { return _StageAxesTimeout; }
            set { _StageAxesTimeout = value; }
        }

        private Element<int> _LoaderAxesTimeout = new Element<int>();
        public Element<int> LoaderAxesTimeout
        {
            get { return _LoaderAxesTimeout; }
            set { _LoaderAxesTimeout = value; }
        }

        //==> TODO : Recipe Editor에서 EMGAxisList를 수정하기 위한 데이터 타입을 가져야 한다.
        //==> Value 가 String으로 두고 ,로 파싱하는 방식은 어떠할 까??
        private Dictionary<String, List<EnumAxisConstants>> _HWAxisCheckList = new Dictionary<String, List<EnumAxisConstants>>();
        public Dictionary<String, List<EnumAxisConstants>> HWAxisCheckList
        {
            get { return _HWAxisCheckList; }
            set { _HWAxisCheckList = value; }
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            MonitoringEnable.Value = false;
            StageAxesTimeout.Value = 10000;
            LoaderAxesTimeout.Value = 10000;

            List<EnumAxisConstants> stageAxisList = new List<EnumAxisConstants>();
            stageAxisList.Add(EnumAxisConstants.X);
            stageAxisList.Add(EnumAxisConstants.Y);
            stageAxisList.Add(EnumAxisConstants.Z);
            
            //==> key : 감시할 HW 부품, Value : HW 부품에 이상이 생겼을 때 정지 시킬 축 목록
            _HWAxisCheckList.Add(nameof(AirChecker), stageAxisList);
            _HWAxisCheckList.Add(nameof(VacuumChecker), stageAxisList);
            _HWAxisCheckList.Add(nameof(PowerChecker), stageAxisList);
            _HWAxisCheckList.Add(nameof(AxisChecker), stageAxisList);
            _HWAxisCheckList.Add(nameof(DoorChecker), stageAxisList);

            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

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
    }
}
