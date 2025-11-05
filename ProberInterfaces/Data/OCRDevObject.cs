using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;

namespace ProberInterfaces
{
    /// <summary>
    /// Transfer Object를 정의합니다.
    /// </summary>
    [Serializable]
    public class OCRDevParameter : IDeviceParameterizable, IParam, IParamNode
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FilePath { get; } = "OCR";
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; } = "OCRDevice.json";
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


        public EnumOCRRunMode RunMode { get; set; }
        public double OCRAngle { get; set; }
        public List<OCRConfig> ConfigList { get; set; }

        public LotIntegrity lotIntegrity { get; set; }

        public OCRDevParameter()
        {
            ConfigList = new List<OCRConfig>();
            lotIntegrity = new LotIntegrity();
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            return retval;
        }
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            RunMode = EnumOCRRunMode.EMUL;
            SetDefaultParam();
            return retVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            RunMode = EnumOCRRunMode.COGNEX;
            lotIntegrity = new LotIntegrity();
            lotIntegrity.LotIntegrityEnable = false;
            lotIntegrity.LotnameDigit = 0;
            lotIntegrity.Lotnamelength = 0;
            ConfigList = new List<OCRConfig>();
            OCRConfig defaultConfig = new OCRConfig();
            defaultConfig.Direction = "0";
            defaultConfig.Mark = "4";
            defaultConfig.CheckSum = "2";
            defaultConfig.RetryOption = "2";
            defaultConfig.FieldString = "************";
            defaultConfig.OCRCutLineScore = 200;
            defaultConfig.RegionY = 123.0;
            defaultConfig.RegionX = 75.0;
            defaultConfig.RegionHeight = 165.0;
            defaultConfig.RegionWidth = 580.0;
            defaultConfig.CharY = 170.5;
            defaultConfig.CharX = 527.0;
            defaultConfig.CharHeight = 39.0;
            defaultConfig.CharWidth = 20.0;
            defaultConfig.Light = "6";
            defaultConfig.LightIntensity = 4;
            defaultConfig.UOffset = 0;
            defaultConfig.WOffset = 0;
            defaultConfig.AngleOffset = 0;
            OCRAngle = 275;
            ConfigList.Add(defaultConfig);

            return retVal;
        }
        public void SetElementMetaData()
        {

        }

        public OCRConfig GetManualConfig()
        {
            OCRConfig manualConfig = new OCRConfig();
            manualConfig.Light = "0";
            manualConfig.LightIntensity = 1;
            manualConfig.CheckSum = "2";
            ConfigList.Add(manualConfig);

            return manualConfig;
        }

        public EventCodeEnum SetHynixDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                this.RunMode = EnumOCRRunMode.COGNEX;
                this.OCRAngle = 87.0;

                this.ConfigList = new List<OCRConfig>();

                OCRConfig config = new OCRConfig();

                config.Direction = "2";
                config.Mark = "4";
                config.CheckSum = "2";
                config.RetryOption = "2";
                config.FieldString = "naannnn-nnan";
                config.OCRCutLineScore = 300;
                config.RegionY = 150.5;
                config.RegionX = 172.0;
                config.RegionHeight = 140.0;
                config.RegionWidth = 580.0;
                config.CharY = 213.0;
                config.CharX = 274.5;
                config.CharHeight = 34.0;
                config.CharWidth = 17.0;
                config.UOffset = 0.0;
                config.WOffset = 0.0;
                config.AngleOffset = 0.0;
                config.OCRAngle = 0.0;
                config.Light = "13";
                config.LightIntensity = 200;

                this.ConfigList.Add(config);

                this.lotIntegrity.LotIntegrityEnable = false;
                this.lotIntegrity.LotnameDigit = 0;
                this.lotIntegrity.Lotnamelength = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void Copy(OCRDevParameter source)
        {
            try
            {
                this.IsParamChanged = source.IsParamChanged;
                this.Nodes = source.Nodes;
                this.Genealogy = source.Genealogy;
                this.Owner = source.Owner;
                this.RunMode = source.RunMode;
                this.OCRAngle = source.OCRAngle;

                //if (this.ConfigList == null)
                //{
                //    this.ConfigList = new List<OCRConfig>();
                //}

                //this.ConfigList.Clear();
                //위 코드 일경우 this 와 source 가 같을 경우 둘다 ConfigList.Clear() 될수 있음. 
                this.ConfigList = new List<OCRConfig>(); // 새로운 인스턴스를 할당하여 참조 분리


                foreach (OCRConfig sourceConfig in source.ConfigList)
                {
                    OCRConfig copiedConfig = new OCRConfig();
                    copiedConfig.Copy(sourceConfig);

                    this.ConfigList.Add(copiedConfig);
                }

                if (this.lotIntegrity == null)
                {
                    this.lotIntegrity = new LotIntegrity();
                }

                this.lotIntegrity.Copy(source.lotIntegrity);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
    public enum EnumOCRRunMode { COGNEX, EMUL }

    public enum OCR_OperateMode { NORMAL, DEBUG, NG_GO, MANUAL_INPUT }
    [Serializable]
    public class LoaderOCRConfigParam : ISystemParameterizable, IParam, IParamNode, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [JsonIgnore]
        public PropertyChangedEventHandler propertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }
        #endregion
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FilePath { get; } = "OCR";
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; } = "OCRConfig.json";
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

        private OCR_OperateMode _Mode;
        public OCR_OperateMode Mode
        {
            get { return _Mode; }
            set
            {
                if (value != _Mode)
                {
                    _Mode = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _Enable = true;
        public bool Enable
        {
            get { return _Enable; }
            set { _Enable = value; }
        }

        private bool _RemoveCheckSum = false;
        public bool RemoveCheckSum
        {
            get { return _RemoveCheckSum; }
            set { _RemoveCheckSum = value; }
        }

        private bool _ReplaceDashToDot = false;
        public bool ReplaceDashToDot
        {
            get { return _ReplaceDashToDot; }
            set { _ReplaceDashToDot = value; }
        }

        private bool _SkipWaferOcrFail = false;
        public bool SkipWaferOcrFail
        {
            get { return _SkipWaferOcrFail; }
            set { _SkipWaferOcrFail = value; }
        }

        public List<OCRConfig> ConfigList { get; set; }

        public LoaderOCRConfigParam()
        {
            ConfigList = new List<OCRConfig>();
        }


        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            return retval;
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

            ConfigList = new List<OCRConfig>();
            OCRConfig defaultConfig = new OCRConfig();
            defaultConfig.Direction = "0";
            defaultConfig.Mark = "4";
            defaultConfig.CheckSum = "2";
            defaultConfig.RetryOption = "2";
            defaultConfig.FieldString = "************";
            defaultConfig.OCRCutLineScore = 200;
            defaultConfig.RegionY = 123.0;
            defaultConfig.RegionX = 75.0;
            defaultConfig.RegionHeight = 165.0;
            defaultConfig.RegionWidth = 580.0;
            defaultConfig.CharY = 170.5;
            defaultConfig.CharX = 527.0;
            defaultConfig.CharHeight = 39.0;
            defaultConfig.CharWidth = 20.0;
            defaultConfig.Light = "6";
            defaultConfig.LightIntensity = 4;
            defaultConfig.UOffset = 0;
            defaultConfig.WOffset = 0;
            defaultConfig.AngleOffset = 0;

            ConfigList.Add(defaultConfig);
            Mode = OCR_OperateMode.NORMAL;
            Enable = true;
            RemoveCheckSum = false;
            ReplaceDashToDot = false;

            return retVal;
        }
        public void SetElementMetaData()
        {

        }
        public OCRConfig GetManualConfig()
        {
            OCRConfig manualConfig = new OCRConfig();
            manualConfig.Light = "0";
            manualConfig.LightIntensity = 1;
            manualConfig.CheckSum = "2";
            ConfigList.Add(manualConfig);

            return manualConfig;
        }
    }


    [Serializable]
    public class OCRConfig
    {
        /*
         * 0 : Normal
         * 1 : Mirrored horizontally
         * 2 : Flipped vertically
         * 3 : Rotated 180 degrees
         */
        public String Direction { get; set; }
        /*
         * 1  : BC, BC 412
         * 2  : BC, IBM 412
         * 3  : Internal Use Only
         * 4  : Chars, SEMI
         * 5  : Chars, IBM
         * 6  : Chars, Triple
         * 7  : OCR-A
         * 11 : SEMI m1.15
         */
        public String Mark { get; set; }
        /*
         * 0 : Virtual
         * 1 : SEMI (Not use)
         * 2 : SEMI with Virtual
         * 3 : BC 412 with Virtual (Not use)
         * 4 : IBM 412 with Virtual
         */
        public String CheckSum { get; set; }
        /*
         * 0 : Not Adjust
         * 1 : Adjust
         * 2 : Adjust & Save
         */
        public String RetryOption { get; set; }
        public String FieldString { get; set; }
        public int OCRCutLineScore { get; set; }
        public double RegionY { get; set; }
        public double RegionX { get; set; }
        public double RegionHeight { get; set; }
        public double RegionWidth { get; set; }
        public double CharY { get; set; }
        public double CharX { get; set; }
        public double CharHeight { get; set; }
        public double CharWidth { get; set; }
        public double UOffset { get; set; }
        public double WOffset { get; set; }
        public double AngleOffset { get; set; }
        public double OCRAngle { get; set; }
        public String Light { get; set; }
        public int LightIntensity { get; set; }
        public OCRConfig()
        {
            Direction = "0";
            Mark = "4";
            CheckSum = "2";
            RetryOption = "2";
            FieldString = "************";
            OCRCutLineScore = 200;
            RegionY = 123.0;
            RegionX = 75.0;
            RegionHeight = 165.0;
            RegionWidth = 580.0;
            CharY = 170.5;
            CharX = 527.0;
            CharHeight = 39.0;
            CharWidth = 20.0;
            Light = "6";
            LightIntensity = 4;
            UOffset = 0;
            WOffset = 0;
            AngleOffset = 0;
        }

        public void Copy(OCRConfig source)
        {
            try
            {
                this.Direction = source.Direction;
                this.Mark = source.Mark;
                this.CheckSum = source.CheckSum;
                this.RetryOption = source.RetryOption;
                this.FieldString = source.FieldString;
                this.OCRCutLineScore = source.OCRCutLineScore;
                this.RegionY = source.RegionY;
                this.RegionX = source.RegionX;
                this.RegionHeight = source.RegionHeight;
                this.RegionWidth = source.RegionWidth;
                this.CharY = source.CharY;
                this.CharX = source.CharX;
                this.CharHeight = source.CharHeight;
                this.CharWidth = source.CharWidth;
                this.UOffset = source.UOffset;
                this.WOffset = source.WOffset;
                this.AngleOffset = source.AngleOffset;
                this.OCRAngle = source.OCRAngle;
                this.Light = source.Light;
                this.LightIntensity = source.LightIntensity;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class LotIntegrity
    {
        public bool LotIntegrityEnable { get; set; }
        public int LotnameDigit { get; set; }
        public int Lotnamelength { get; set; }
        public LotIntegrity()
        {
            LotIntegrityEnable = false;
            LotnameDigit = 0;
            Lotnamelength = 0;
        }

        public void Copy(LotIntegrity source)
        {
            try
            {
                this.LotIntegrityEnable = source.LotIntegrityEnable;
                this.LotnameDigit = source.LotnameDigit;
                this.Lotnamelength = source.Lotnamelength;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
