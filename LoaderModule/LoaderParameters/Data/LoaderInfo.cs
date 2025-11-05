using LogModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

using ProberInterfaces;
using System.Xml.Serialization;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces.Loader;

namespace LoaderParameters
{
    /// <summary>
    /// Loader Tpe
    /// </summary>
    [DataContract]
    [Flags]
    public enum EnumLoaderMovingMethodType
    {
        /// <summary>
        /// OPUS3 (Deprecated) 
        /// </summary>
        [EnumMember]
        OPUS3 = 0, //[Deprecated]
        /// <summary>
        /// OPUSV mini 
        /// </summary>
        [EnumMember]
        OPUSV_MINI = 1,
    }

    
    /// <summary>
    /// Loader의 정보를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class LoaderInfo : INotifyPropertyChanged, ICloneable, IParamNode, IParam, ILoaderInfo
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
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

        public LoaderInfo()
        {
            _TimeStamp.Value = DateTime.Now;
        }
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

        private Element<DateTime> _TimeStamp = new Element<DateTime>();
        /// <summary>
        /// TimeStamp 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<DateTime> TimeStamp
        {
            get { return _TimeStamp; }
            set { _TimeStamp = value; RaisePropertyChanged(); }
        }

        private LoaderModuleInfo _ModuleInfo;
        /// <summary>
        /// ModuleInfo 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public LoaderModuleInfo ModuleInfo
        {
            get { return _ModuleInfo; }
            set { _ModuleInfo = value; RaisePropertyChanged(); }
        }

        private LoaderMap _StateMap;
        /// <summary>
        /// StateMap 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public LoaderMap StateMap
        {
            get { return _StateMap; }
            set { _StateMap = value; RaisePropertyChanged(); }
        }
        private int _DynamicMode = 0;
        /// <summary>
        /// StateMap 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public int DynamicMode  // 0=NORMAL , 1= DYNAMIC
        {
            get { return _DynamicMode; }
            set { _DynamicMode = value; RaisePropertyChanged(); }
        }

        private int _FoupShiftMode = 0;
        /// <summary>
        /// StateMap 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public int FoupShiftMode  // 0=NORMAL , 1= SHIFT
        {
            get { return _FoupShiftMode; }
            set { _FoupShiftMode = value; RaisePropertyChanged(); }
        }

        public string FilePath { get; set; } = @"C:\Logs\ProberSystem\";

        public string FileName { get; set; } = @"loaderInfo.json";

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            LoaderInfo info = new LoaderInfo();
            try
            {
            info.ModuleInfo = ModuleInfo.Clone() as LoaderModuleInfo;
            info.StateMap = StateMap.Clone() as LoaderMap;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return info;
        }

        public void ValueCopy(LoaderInfo source)
        {
            this.ModuleInfo = source.ModuleInfo;
            this.StateMap = source.StateMap;
            this.TimeStamp = source.TimeStamp;
        }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }
    }

    /// <summary>
    /// 서스펜드 사유를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public enum ReasonOfSuspendedEnum
    {
        /// <summary>
        /// NONE
        /// </summary>
        [EnumMember]
        NONE,
        /// <summary>
        /// LOAD
        /// </summary>
        [EnumMember]
        LOAD,
        /// <summary>
        /// UNLOAD
        /// </summary>
        [EnumMember]
        UNLOAD,
        /// <summary>
        /// OCR_FAILED
        /// </summary>
        [EnumMember]
        OCR_FAILED,

        [EnumMember]
        OCR_REMOTING,

        [EnumMember]
        OCR_ABORT,

        [EnumMember]
        ALARM_ERROR,

        [EnumMember]
        SCAN_FAILED,

        [EnumMember]
        CARD_LOAD,
        /// <summary>
        /// UNLOAD
        /// </summary>
        [EnumMember]
        CARD_UNLOAD,
    }

    /// <summary>
    /// LoaderModuleInfo 를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class LoaderModuleInfo : INotifyPropertyChanged, ICloneable, IParamNode
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

        private ModuleStateEnum _ModuleState;
        /// <summary>
        /// ModuleState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ModuleStateEnum ModuleState
        {
            get { return _ModuleState; }
            set { _ModuleState = value; RaisePropertyChanged(); }
        }
        
        private ReasonOfSuspendedEnum _ReasonOfSuspended;
        /// <summary>
        /// ReasonOfSuspended 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ReasonOfSuspendedEnum ReasonOfSuspended
        {
            get { return _ReasonOfSuspended; }
            set { _ReasonOfSuspended = value; RaisePropertyChanged(); }
        }

        private int _ChuckNumber;
        [DataMember]
        public int ChuckNumber
        {
            get { return _ChuckNumber; }
            set { _ChuckNumber = value; RaisePropertyChanged(); }
        }

        private string _ProbeCardID;
        [DataMember]
        public string ProbeCardID
        {
            get { return _ProbeCardID; }
            set 
            { _ProbeCardID = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            try
            {
                var shallowClone = MemberwiseClone() as LoaderModuleInfo;
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
