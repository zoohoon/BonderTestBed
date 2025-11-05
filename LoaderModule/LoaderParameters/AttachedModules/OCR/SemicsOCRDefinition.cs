using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using System.Runtime.Serialization;
using LogModule;
using ProberInterfaces;
using Newtonsoft.Json;

namespace LoaderParameters
{
    /// <summary>
    /// SEMICSOCR의 특성을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class SemicsOCRDefinition : OCRDefinitionBase, IParamNode
    {
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
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

        public new List<object> Nodes { get; set; }

        private Element<int> _LightChannel1 = new Element<int>();
        /// <summary>
        /// Light Channel1을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> LightChannel1
        {
            get { return _LightChannel1; }
            set { _LightChannel1 = value; RaisePropertyChanged(); }
        }

        private Element<int> _LightChannel2 = new Element<int>();
        /// <summary>
        /// Light Channel2을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> LightChannel2
        {
            get { return _LightChannel2; }
            set { _LightChannel2 = value; RaisePropertyChanged(); }
        }

        private Element<int> _LightChannel3 = new Element<int>();

        /// <summary>
        /// Light Channel3을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> LightChannel3
        {
            get { return _LightChannel3; }
            set { _LightChannel3 = value; RaisePropertyChanged(); }
        }

        private Element<ushort> _OcrLight1_Offset = new Element<ushort>();
        /// <summary>
        /// Light1의 Offset을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<ushort> OcrLight1_Offset
        {
            get { return _OcrLight1_Offset; }
            set { _OcrLight1_Offset = value; RaisePropertyChanged(); }
        }

        private Element<ushort> _OcrLight2_Offset = new Element<ushort>();
        /// <summary>
        /// Light2의 Offset을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<ushort> OcrLight2_Offset
        {
            get { return _OcrLight2_Offset; }
            set { _OcrLight2_Offset = value; RaisePropertyChanged(); }
        }

        private Element<ushort> _OcrLight3_Offset = new Element<ushort>();
        /// <summary>
        /// Light3의 Offset을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<ushort> OcrLight3_Offset
        {
            get { return _OcrLight3_Offset; }
            set { _OcrLight3_Offset = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 정의하는 모듈의 타입을 가져옵니다.
        /// </summary>
        /// <returns>모듈 타입</returns>
        public override ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.SEMICSOCR;
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public override object Clone()
        {
            try
            {
            var shallowClone = MemberwiseClone() as SemicsOCRDefinition;
                shallowClone.AccessParams = AccessParams.CloneFrom();
            return shallowClone;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
    }

}
