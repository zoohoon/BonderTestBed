using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace LoaderBase.AttachModules.ModuleInterfaces
{
    [Serializable]
    public class CognexProcessDevParameter : IDeviceParameterizable, IParam, IParamNode
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FilePath { get; } = "OCR";
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; } = "CognexProcessDevParam.json";
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
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        [NonSerialized]
        public const String ManualConfigName = "Manual";
        [NonSerialized]
        public const String DefaultConfigName = "Default";

        public EnumCognexRunMode RunMode { get; set; }
        public List<CognexHost> CognexHostList { get; set; }
        public List<CognexConfig> ConfigList { get; set; }

        public CognexProcessDevParameter()
        {

        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            return retval;
        }
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            RunMode = EnumCognexRunMode.EMUL;
            SetDefaultParam();
            return retVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            RunMode = EnumCognexRunMode.COGNEX;

            CognexHostList = new List<CognexHost>();
            CognexConfig defaultConfig = new CognexConfig();
            CognexConfig manualConfig = new CognexConfig();
            ConfigList = new List<CognexConfig>();
            if (SystemManager.SysteMode == SystemModeEnum.Single)
            {
                CognexHostList.Add(new CognexHost("COGNEX", DefaultConfigName));

                defaultConfig.Name = DefaultConfigName;
                defaultConfig.Direction = "0";
                defaultConfig.Mark = "11";
                defaultConfig.CheckSum = "2";
                defaultConfig.RetryOption = "2";
                defaultConfig.FieldString = "************";
                defaultConfig.OCRCutLineScore = 370;
                defaultConfig.RegionY = 160;
                defaultConfig.RegionX = 90;
                defaultConfig.RegionHeight = 150;
                defaultConfig.RegionWidth = 580;
                defaultConfig.CharY = 160;
                defaultConfig.CharX = 90;
                defaultConfig.CharHeight = 35;
                defaultConfig.CharWidth = 20;
                defaultConfig.Light = "0";
                defaultConfig.LightIntensity = 1;
                ConfigList.Add(defaultConfig);

                //==> Manual Config에서는 의미 있는 값이 조명과 Check Sum 뿐이다.
                manualConfig.Name = ManualConfigName + "1";
                manualConfig.Light = "0";
                manualConfig.LightIntensity = 1;
                manualConfig.CheckSum = "2";

                ConfigList.Add(manualConfig);
            }

            if (SystemManager.SysteMode == SystemModeEnum.Multiple)
            {
                CognexHostList.Add(new CognexHost("COGNEX1", DefaultConfigName + "1"));

                defaultConfig = new CognexConfig();
                defaultConfig.Name = DefaultConfigName + "1";
                defaultConfig.Direction = "0";
                defaultConfig.Mark = "11";
                defaultConfig.CheckSum = "2";
                defaultConfig.RetryOption = "2";
                defaultConfig.FieldString = "************";
                defaultConfig.OCRCutLineScore = 0;
                defaultConfig.RegionY = 160;
                defaultConfig.RegionX = 90;
                defaultConfig.RegionHeight = 150;
                defaultConfig.RegionWidth = 580;
                defaultConfig.CharY = 160;
                defaultConfig.CharX = 90;
                defaultConfig.CharHeight = 35;
                defaultConfig.CharWidth = 20;
                defaultConfig.Light = "0";
                defaultConfig.LightIntensity = 18;
                ConfigList.Add(defaultConfig);


                CognexHostList.Add(new CognexHost("COGNEX2", DefaultConfigName + "2"));

                defaultConfig = new CognexConfig();
                defaultConfig.Name = DefaultConfigName + "2";
                defaultConfig.Direction = "0";
                defaultConfig.Mark = "11";
                defaultConfig.CheckSum = "2";
                defaultConfig.RetryOption = "2";
                defaultConfig.FieldString = "************";
                defaultConfig.OCRCutLineScore = 0;
                defaultConfig.RegionY = 160;
                defaultConfig.RegionX = 90;
                defaultConfig.RegionHeight = 150;
                defaultConfig.RegionWidth = 580;
                defaultConfig.CharY = 160;
                defaultConfig.CharX = 90;
                defaultConfig.CharHeight = 35;
                defaultConfig.CharWidth = 20;
                defaultConfig.Light = "0";
                defaultConfig.LightIntensity = 18;
                ConfigList.Add(defaultConfig);

                //==> Manual Config에서는 의미 있는 값이 조명과 Check Sum 뿐이다.
                manualConfig = new CognexConfig();
                manualConfig.Name = ManualConfigName + "2";
                manualConfig.Light = "0";
                manualConfig.LightIntensity = 1;
                manualConfig.CheckSum = "2";

                ConfigList.Add(manualConfig);


                CognexHostList.Add(new CognexHost("COGNEX3", DefaultConfigName + "3"));

                defaultConfig = new CognexConfig();
                defaultConfig.Name = DefaultConfigName + "3";
                defaultConfig.Direction = "0";
                defaultConfig.Mark = "11";
                defaultConfig.CheckSum = "2";
                defaultConfig.RetryOption = "2";
                defaultConfig.FieldString = "************";
                defaultConfig.OCRCutLineScore = 370;
                defaultConfig.RegionY = 160;
                defaultConfig.RegionX = 90;
                defaultConfig.RegionHeight = 150;
                defaultConfig.RegionWidth = 580;
                defaultConfig.CharY = 160;
                defaultConfig.CharX = 90;
                defaultConfig.CharHeight = 35;
                defaultConfig.CharWidth = 20;
                defaultConfig.Light = "1";
                defaultConfig.LightIntensity = 30;
                ConfigList.Add(defaultConfig);

                //==> Manual Config에서는 의미 있는 값이 조명과 Check Sum 뿐이다.
                manualConfig = new CognexConfig();
                manualConfig.Name = ManualConfigName + "3";
                manualConfig.Light = "0";
                manualConfig.LightIntensity = 1;
                manualConfig.CheckSum = "2";

                ConfigList.Add(manualConfig);

            }




                return retVal;
        }
        public void SetElementMetaData()
        {

        }
        public CognexConfig GetManualConfig()
        {
            CognexConfig manualConfig = ConfigList.FirstOrDefault(item => item.Name == ManualConfigName);

            if (manualConfig == null)
            {
                manualConfig = new CognexConfig();
                manualConfig.Name = ManualConfigName;
                manualConfig.Light = "0";
                manualConfig.LightIntensity = 1;
                manualConfig.CheckSum = "2";
                ConfigList.Add(manualConfig);
            }

            return manualConfig;
        }
    }
    public enum EnumCognexRunMode { COGNEX, EMUL }
}