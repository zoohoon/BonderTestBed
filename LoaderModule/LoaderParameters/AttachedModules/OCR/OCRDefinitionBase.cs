using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

using ProberInterfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using LogModule;
using Newtonsoft.Json;
using ProberInterfaces.Enum;

namespace LoaderParameters
{
    /// <summary>
    /// OCR의 기본 특성을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class OCRDefinitionBase : INotifyPropertyChanged, ICloneable, IParamNode
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

        private Element<int> _DependencyPreAlignNum = new Element<int>();
        /// <summary>
        /// OCR 수행 전 사용되는 PreAlign의 번호를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<int> DependencyPreAlignNum
        {
            get { return _DependencyPreAlignNum; }
            set { _DependencyPreAlignNum = value; RaisePropertyChanged(); }
        }

        private Element<EnumProberCam> _OCRCam = new Element<EnumProberCam>();
        /// <summary>
        /// OCR의 카메라를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<EnumProberCam> OCRCam
        {
            get { return _OCRCam; }
            set { _OCRCam = value; RaisePropertyChanged(); }
        }

        private Element<OCRDirectionEnum> _OCRDirection = new Element<OCRDirectionEnum>();
        /// <summary>
        /// OCR의 방향을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<OCRDirectionEnum> OCRDirection
        {
            get { return _OCRDirection; }
            set { _OCRDirection = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<OCRAccessParam> _AccessParams;
        /// <summary>
        /// OCR에 Access하기 위한 파라미터를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<OCRAccessParam> AccessParams
        {
            get { return _AccessParams; }
            set { _AccessParams = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<SubchuckMotionParam> _SubchuckMotionParams;
        /// <summary>
        /// OCR에 Access하기 위한 파라미터를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ObservableCollection<SubchuckMotionParam> SubchuckMotionParams
        {
            get { return _SubchuckMotionParams; }
            set { _SubchuckMotionParams = value; RaisePropertyChanged(); }
        }

        public OCRDefinitionBase()
        {
            _AccessParams = new ObservableCollection<OCRAccessParam>();
            _SubchuckMotionParams = new ObservableCollection<SubchuckMotionParam>();
        }
        /// <summary>
        /// 정의하는 모듈의 타입을 가져옵니다.
        /// </summary>
        /// <returns>모듈 타입</returns>
        public abstract ModuleTypeEnum GetModuleType();

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public abstract object Clone();
    }

    /// <summary>
    /// Loader 가 OCR에 Access하기위한 파라미터를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class OCRAccessParam : INotifyPropertyChanged, ICloneable, IParamNode
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

        private Element<SubstrateTypeEnum> _SubstrateType = new Element<SubstrateTypeEnum>();
        /// <summary>
        /// 이송 오브젝트의 타입을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<SubstrateTypeEnum> SubstrateType
        {
            get { return _SubstrateType; }
            set { _SubstrateType = value; RaisePropertyChanged(); }
        }

        private Element<SubstrateSizeEnum> _SubstrateSize = new Element<SubstrateSizeEnum>();
        /// <summary>
        /// 이송 오브젝트의 사이즈를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<SubstrateSizeEnum> SubstrateSize
        {
            get { return _SubstrateSize; }
            set { _SubstrateSize = value; RaisePropertyChanged(); }
        }

        private LoaderCoordinate _Position;
        /// <summary>
        /// OCR의 위치를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public LoaderCoordinate Position
        {
            get { return _Position; }
            set { _Position = value; RaisePropertyChanged(); }
        }

        private Element<double> _VPos = new Element<double>();
        /// <summary>
        /// Notch 위치를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> VPos
        {
            get { return _VPos; }
            set { _VPos = value; RaisePropertyChanged(); }
        }

        [XmlIgnore, JsonIgnore]
        public double OCROffsetU { get; set; }
        [XmlIgnore, JsonIgnore]
        public double OCROffsetW { get; set; }
        [XmlIgnore, JsonIgnore]
        public double OCROffsetV { get; set; }

        public OCRAccessParam()
        {
            _Position = new LoaderCoordinate();
            OCROffsetU = 0;
            OCROffsetW = 0;
            OCROffsetV = 0;
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            var shallowClone = MemberwiseClone() as OCRAccessParam;
            try
            {

                try
                {
                    shallowClone.Position = Position.Clone<LoaderCoordinate>();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return shallowClone;
        }
    }

    /// <summary>
    /// Pa sub chuckdl OCR에 Access하기위한 파라미터를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class SubchuckMotionParam: INotifyPropertyChanged, IParamNode
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
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Element<SubstrateTypeEnum> _SubstrateType = new Element<SubstrateTypeEnum>();
        /// <summary>
        /// 이송 오브젝트의 타입을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<SubstrateTypeEnum> SubstrateType
        {
            get { return _SubstrateType; }
            set { _SubstrateType = value; RaisePropertyChanged(); }
        }

        private Element<SubstrateSizeEnum> _SubstrateSize = new Element<SubstrateSizeEnum>();
        /// <summary>
        /// 이송 오브젝트의 사이즈를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<SubstrateSizeEnum> SubstrateSize
        {
            get { return _SubstrateSize; }
            set { _SubstrateSize = value; RaisePropertyChanged(); }
        }


        private Element<double> _SubchuckXCoord = new Element<double>();
        /// <summary>
        /// RND 커맨드에 사용되는 X1,Y, 위치를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> SubchuckXCoord
        {
            get { return _SubchuckXCoord; }
            set { _SubchuckXCoord = value; RaisePropertyChanged(); }
        }

        private Element<double> _SubchuckYCoord = new Element<double>();
        /// <summary>
        /// Notch 위치를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> SubchuckYCoord
        {
            get { return _SubchuckYCoord; }
            set { _SubchuckYCoord = value; RaisePropertyChanged(); }
        }
        private Element<double> _SubchuckAngle_Offset = new Element<double>();
        /// <summary>
        /// Notch 위치를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> SubchuckAngle_Offset
        {
            get { return _SubchuckAngle_Offset; }
            set { _SubchuckAngle_Offset = value; RaisePropertyChanged(); }
        }
        public SubchuckMotionParam()
        {
        }
    }
}
