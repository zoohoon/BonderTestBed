using LogModule;
using Newtonsoft.Json;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace LoaderParameters
{
    /// <summary>
    /// UExtensionCylinder 을 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class UExtensionCylinder : UExtensionBase
    {
        [XmlIgnore, JsonIgnore]
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

        [XmlIgnore, JsonIgnore]
        public new List<object> Nodes { get; set; }

        /// <summary>
        /// 인스턴스를 생성합니다.
        /// </summary>
        public UExtensionCylinder()
        {
            try
            {
            UExtensionType.Value = UExtensionTypeEnum.NONE;

            _IOWaitTimeout.Value = 10000;
            _DOARMINAIR.Value = "";
            _DOARMOUTAIR.Value = "";
            _DIARMIN.Value = "";
            _DIARMOUT.Value = "";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private Element<long> _IOWaitTimeout = new Element<long>();
        /// <summary>
        /// IO의 값을 기다릴 때 Timeout이 발생하는 시간을 가져오거나 설정합니다. (단위 ms)
        /// </summary>
        [DataMember]
        public Element<long> IOWaitTimeout
        {
            get { return _IOWaitTimeout; }
            set { _IOWaitTimeout = value; RaisePropertyChanged(); }
        }

        private Element<string> _DOARMINAIR = new Element<string>();
        /// <summary>
        /// DOARMINAIR의 Output Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DOARMINAIR
        {
            get { return _DOARMINAIR; }
            set { _DOARMINAIR = value; RaisePropertyChanged(); }
        }

        private Element<string> _DOARMOUTAIR = new Element<string>();
        /// <summary>
        /// DOARMOUTAIR의 Output Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DOARMOUTAIR
        {
            get { return _DOARMOUTAIR; }
            set { _DOARMOUTAIR = value; RaisePropertyChanged(); }
        }

        private Element<string> _DIARMIN = new Element<string>();
        /// <summary>
        ///  DIARMIN 의 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DIARMIN
        {
            get { return _DIARMIN; }
            set { _DIARMIN = value; RaisePropertyChanged(); }
        }

        private Element<string> _DIARMOUT = new Element<string>();
        /// <summary>
        ///  DIARMIN 의 Input Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DIARMOUT
        {
            get { return _DIARMOUT; }
            set { _DIARMOUT = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public override object Clone()
        {
            try
            {
            var shallowClone = MemberwiseClone();
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
