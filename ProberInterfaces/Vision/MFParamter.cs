using LogModule;
using System;
using System.Collections.Generic;

namespace ProberInterfaces.Vision
{
    using Newtonsoft.Json;
    using System.Xml.Serialization;

    /// <summary>
    /// Pattern matching parameters
    /// </summary>
    [Serializable]
    public class MFParameter : IParamNode
    {

        [XmlIgnore, JsonIgnore]
        public virtual Object Owner { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public virtual string Genealogy { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        

        private Element<int> _Acceptance = new Element<int>();
        public Element<int> Acceptance
        {
            get { return _Acceptance; }
            set { _Acceptance = value; }
        }
        private Element<int> _Certainty = new Element<int>();
        public Element<int> Certainty
        {
            get { return _Certainty; }
            set { _Certainty = value; }
        }
        private Element<EnumModelTargetType> _ModelTargetType = new Element<EnumModelTargetType>();
        public Element<EnumModelTargetType> ModelTargetType
        {
            get { return _ModelTargetType; }
            set { _ModelTargetType = value; }
        }

        private Element<EnumForegroundType> _ForegroundType = new Element<EnumForegroundType>();
        public Element<EnumForegroundType> ForegroundType
        {
            get { return _ForegroundType; }
            set { _ForegroundType = value; }
        }
        private Element<double> _ModelVertThick = new Element<double>();
        public Element<double> ModelVertThick
        {
            get { return _ModelVertThick; }
            set { _ModelVertThick = value; }
        }
        private Element<double> _ModelHoriThick = new Element<double>();
        public Element<double> ModelHoriThick
        {
            get { return _ModelHoriThick; }
            set { _ModelHoriThick = value; }
        }

        private Element<double> _ScaleMin = new Element<double>();
        public Element<double> ScaleMin
        {
            get { return _ScaleMin; }
            set { _ScaleMin = value; }
        }
        private Element<double> _ScaleMax = new Element<double>();
        public Element<double> ScaleMax
        {
            get { return _ScaleMax; }
            set { _ScaleMax = value; }
        }

        private Element<double> _ModelWidth = new Element<double>();
        public Element<double> ModelWidth
        {
            get { return _ModelWidth; }
            set { _ModelWidth = value; }
        }
        private Element<double> _ModelHeight = new Element<double>();
        public Element<double> ModelHeight
        {
            get { return _ModelHeight; }
            set { _ModelHeight = value; }
        }
        private Element<double> _Smoothness = new Element<double>();
        public Element<double> Smoothness
        {
            get { return _Smoothness; }
            set { _Smoothness = value; }
        }
        private Element<string> _ModelFilePath = new Element<string>();
        public Element<string> ModelFilePath
        {
            get { return _ModelFilePath; }
            set { _ModelFilePath = value; }
        }
        private Element<EnumProberCam> _CamType = new Element<EnumProberCam>();
        public Element<EnumProberCam> CamType
        {
            get { return _CamType; }
            set { _CamType = value; }
        }


        private List<LightChannelType> _Lights;
        public List<LightChannelType> Lights
        {
            get { return _Lights; }
            set
            {
                if (value != _Lights)
                {
                    _Lights = value;
                }
            }
        }
        /// <summary>
        /// 검색할 모델 개수
        /// 0: ALL
        /// </summary>
        private Element<int> _ExpectedOccurrence = new Element<int>() { Value = 1 };
        public Element<int> ExpectedOccurrence
        {
            get { return _ExpectedOccurrence; }
            set { _ExpectedOccurrence = value; }
        }
        public List<LightValueParam> LightValues { get; set; }

        private MFParameter _Child;

        public MFParameter Child
        {
            get { return _Child; }
            set { _Child = value; }
        }

        public void CopyTo(MFParameter target)
        {
            try
            {
                if (target == null) target = new MFParameter();
                target.ModelTargetType.Value = this.ModelTargetType.Value;
                target.ForegroundType.Value = this.ForegroundType.Value;
                target.ModelWidth.Value = this.ModelWidth.Value;
                target.ModelHeight.Value = this.ModelHeight.Value;

                target.Acceptance.Value = this.Acceptance.Value;
                target.Certainty.Value = this.Certainty.Value;
               
                target.ModelVertThick.Value = this.ModelVertThick.Value;
                target.ModelHoriThick.Value = this.ModelHoriThick.Value;
                
                target.ScaleMin.Value = this.ScaleMin.Value;
                target.ScaleMax.Value = this.ScaleMax.Value;

                if (ModelFilePath.Value != null)
                    target.ModelFilePath.Value = (string)ModelFilePath.Value.Clone();

                target.Lights = new List<LightChannelType>();
                foreach (var light in this.Lights)
                {
                    target.Lights.Add(light);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public MFParameter()
        {
            try
            {
                //Acceptance.Value = 80;
                //Certainty.Value = 95;
                //Lights = new List<LightChannelType>();

                //Lights.Add(new LightChannelType(EnumLightType.COAXIAL, 100));
                //Lights.Add(new LightChannelType(EnumLightType.OBLIQUE, 0));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public MFParameter(int acceptance, int certainty)
        {
            try
            {
                Acceptance.Value = acceptance;
                Certainty.Value = certainty;
                //Lights.Add(new LightChannelType(EnumLightType.COAXIAL, 100));
                //Lights.Add(new LightChannelType(EnumLightType.OBLIQUE, 0));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        /// <summary>
        /// Width and Height in pixels.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public MFParameter(int width, int height, List<LightValueParam> lights, EnumModelTargetType enumModelTargetType = EnumModelTargetType.Rectangle )
        {
            try
            {
                ModelTargetType.Value = enumModelTargetType;
                ModelWidth.Value = width;
                ModelHeight.Value = height;
                ForegroundType.Value = EnumForegroundType.ANY;
                ScaleMin.Value = 0.9;
                ScaleMax.Value = 1.1;
                Acceptance.Value = 85;
                Certainty.Value = 95;
                Smoothness.Value = 65;
                ExpectedOccurrence.Value = 1;

                Lights = new List<LightChannelType>();

                foreach (var item in lights)
                {
                    Lights.Add(new LightChannelType(item.Type.Value, (int)item.Value.Value));
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }        
    }
}
