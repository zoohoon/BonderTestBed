using LogModule;
using Newtonsoft.Json;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;


namespace LoaderParameters
{
    /// <summary>
    /// Buffer의 디바이스를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class BufferDevice : INotifyPropertyChanged, ICloneable, IParamNode
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

        /// <summary>
        /// 속성값이 변경되면 발생합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Element<string> _Label = new Element<string>();
        /// <summary>
        /// Label를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> Label
        {
            get { return _Label; }
            set { _Label = value; RaisePropertyChanged(); }
        }

        private TransferObject _AllocateDeviceInfo;
        /// <summary>
        /// Buffer에서 할당되는 오브젝트의 디바이스 정보를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public TransferObject AllocateDeviceInfo
        {
            get { return _AllocateDeviceInfo; }
            set { _AllocateDeviceInfo = value; RaisePropertyChanged(); }
        }

        public BufferDevice()
        {
            _Label.Value = string.Empty;
            _AllocateDeviceInfo = new TransferObject();
        }
        /// <summary>
        /// 정의하는 모듈의 타입을 가져옵니다.
        /// </summary>
        /// <returns>모듈 타입</returns>
        public ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.BUFFER;
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            var shallowClone = MemberwiseClone() as BufferDevice;

            try
            {
                shallowClone.AllocateDeviceInfo = AllocateDeviceInfo.Clone<TransferObject>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return shallowClone;
        }
    }
}
