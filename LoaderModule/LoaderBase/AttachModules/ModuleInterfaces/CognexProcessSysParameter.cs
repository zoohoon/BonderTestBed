using System;
using System.Collections.Generic;
using System.Linq;

namespace LoaderBase.AttachModules.ModuleInterfaces
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Xml.Serialization;

    [Serializable]
    public class CognexProcessSysParameter : ISystemParameterizable, IParam, IParamNode
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FilePath { get; } = "OCR";
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; } = "CognexProcessSysParam.json";
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set { _Owner = value; }
        }

        public List<CognexModule> CognexIPList { get; set; }

        public List<CognexConfig> CognexModuleConfigList { get; set; } // 장비마다 밝기값이 다르기 때문에 밝기값을 다르게 주기 위한 파라미터(Light와 LightIntensity값만 쓴다)


        private int _WaitForOcrTimeout_msec = 120000;

        public int WaitForOcrTimeout_msec
        {
            get { return _WaitForOcrTimeout_msec; }
            set { _WaitForOcrTimeout_msec = value; }
        }



        /// </summary>
        /// <returns></returns>

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            return retval;
        }
        public void SetElementMetaData()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            SetDefaultParam();
            return retVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            CognexIPList = new List<CognexModule>();
            CognexIPList.Add(new CognexModule("COGNEX1", "192.168.103.1",0));
            if (SystemManager.SysteMode == SystemModeEnum.Multiple)
            {
                CognexIPList.Add(new CognexModule("COGNEX2", "192.168.103.2",1));
                CognexIPList.Add(new CognexModule("COGNEX3", "192.168.103.3",2));
                CognexModuleConfigList = new List<CognexConfig>();
                CognexModuleConfigList.Add(new CognexConfig());
                CognexModuleConfigList.Add(new CognexConfig());
                CognexModuleConfigList.Add(new CognexConfig());
                WaitForOcrTimeout_msec = 120000;
            }



            return retVal;
        }
        public String GetIPOrNull(String moduleName)
        {
            var firstItem = CognexIPList?.FirstOrDefault(item => item.ModuleName == moduleName);

            if(firstItem == null)
            {
                return null;
            }
            return firstItem.IP;
        }
        public String GetIPOrNull_Index(int index)
        {
            var firstItem = CognexIPList.FirstOrDefault(item => item.Index == index);
            if (firstItem == null)
            {
                return null;
            }
            return firstItem.IP;
        }
    }
    [Serializable]
    public class CognexModule
    {
        public String ModuleName { get; set; }
        public String IP { get; set; }
        public int Index { get; set; }
        public CognexModule(string moduleName, string iP,int index)
        {
            ModuleName = moduleName;
            IP = iP;
            Index = index;
        }

    }
    public enum EnumCognexModuleState { IDLE, RUNNING, READ_OCR,FAIL,ABORT}
}