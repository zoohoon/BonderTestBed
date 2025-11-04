using System;
using System.Collections.Generic;

namespace ProberInterfaces
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    public class ChillerSysParameter : INotifyPropertyChanged , IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region .. IParam Property
        [JsonIgnore]
        public string FilePath { get; }
        [JsonIgnore]
        public string FileName { get; }
        [JsonIgnore]
        public bool IsParamChanged { get; set; }
        [JsonIgnore]
        public string Genealogy { get; set; }
        [JsonIgnore]
        public object Owner { get; set; }
        [JsonIgnore]
        public List<object> Nodes { get; set; }
        #endregion

        private ObservableCollection<ChillerParameter> _ChillerParams
             = new ObservableCollection<ChillerParameter>();
        public ObservableCollection<ChillerParameter> ChillerParams
        {
            get { return _ChillerParams; }
            set
            {
                if (value != _ChillerParams)
                {
                    _ChillerParams = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<long> _CommReadDelayTime
     = new Element<long>() { Value = 500 };
        /// <summary>
        /// Milliseconds 
        /// </summary>
        [DataMember]
        public Element<long> CommReadDelayTime
        {
            get { return _CommReadDelayTime; }
            set
            {
                if (value != _CommReadDelayTime)
                {
                    _CommReadDelayTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<long> _CommWriteDelayTime
             = new Element<long>() { Value = 500 };
        /// <summary>
        /// Milliseconds 
        /// </summary>
        [DataMember]
        public Element<long> CommWriteDelayTime
        {
            get { return _CommWriteDelayTime; }
            set
            {
                if (value != _CommWriteDelayTime)
                {
                    _CommWriteDelayTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _PingTimeOut
     = new Element<int>() { Value = 300 };
        /// <summary>
        /// Milliseconds 
        /// </summary>
        [DataMember]
        public Element<int> PingTimeOut
        {
            get { return _PingTimeOut; }
            set
            {
                if (value != _PingTimeOut)
                {
                    _PingTimeOut = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ChillerParams.Add(new ChillerParameter() { StageIndexs = new ObservableCollection<int>() { 1, 2, 3, 4 } , ChillerModuleMode = new Element<EnumChillerModuleMode>() { Value = EnumChillerModuleMode.HUBER} });
                ChillerParams.Add(new ChillerParameter() { StageIndexs = new ObservableCollection<int>() { 5, 6, 7, 8 }, ChillerModuleMode = new Element<EnumChillerModuleMode>() { Value = EnumChillerModuleMode.HUBER } });
                ChillerParams.Add(new ChillerParameter() { StageIndexs = new ObservableCollection<int>() { 9, 10, 11, 12 }, ChillerModuleMode = new Element<EnumChillerModuleMode>() { Value = EnumChillerModuleMode.HUBER } });
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ChillerParams.Add(new ChillerParameter() { StageIndexs = new ObservableCollection<int>() { 1, 2, 3, 4 }, ChillerModuleMode = new Element<EnumChillerModuleMode>() { Value = EnumChillerModuleMode.EMUL } });
                ChillerParams[0].IP = "192.168.0.157";
                ChillerParams[0].Port = 502;

                ChillerParams.Add(new ChillerParameter() { StageIndexs = new ObservableCollection<int>() { 5, 6, 7, 8 }, ChillerModuleMode = new Element<EnumChillerModuleMode>() { Value = EnumChillerModuleMode.EMUL } });
                ChillerParams[1].IP = "192.168.0.156";
                ChillerParams[1].Port = 502;

                ChillerParams.Add(new ChillerParameter() { StageIndexs = new ObservableCollection<int>() { 9, 10, 11, 12 }, ChillerModuleMode = new Element<EnumChillerModuleMode>() { Value = EnumChillerModuleMode.EMUL } });
                ChillerParams[2].IP = "192.168.0.155";
                ChillerParams[2].Port = 502;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetElementMetaData()
        {
            foreach (var param in ChillerParams)
            {
                param.SetElementMetaData();
            }
        }
    

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }
    }



    //ChillerParameter 의 변경이 있을경우 ChillerModule 의 InitParam 함수도 변경해주어야한다.
    //ChillerModule - InitParam : ChillerRemote 인 경우 Commander 로 부터 Parameter 를 얻어와 Setting 하기 위한 함수.
    [DataContract]
    public class ChillerParameter : INotifyPropertyChanged, IChillerParameter, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region .. IParam Property
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; }
        #endregion

        private string _IP;
        [DataMember]
        public string IP
        {
            get { return _IP; }
            set { _IP = value; }
        }

        private int _Port;
        [DataMember]
        public int Port
        {
            get { return _Port; }
            set { _Port = value; }
        }

        private byte _SubIndex = 0x00;
        [DataMember]
        public byte SubIndex
        {
            get { return _SubIndex; }
            set { _SubIndex = value; }
        }


        private Element<EnumChillerModuleMode> _ChillerModuleMode
            = new Element<EnumChillerModuleMode>();
        [XmlElement(nameof(ChillerModuleMode))]
        [DataMember]
        public Element<EnumChillerModuleMode> ChillerModuleMode
        {
            get { return _ChillerModuleMode; }
            set
            {
                _ChillerModuleMode = value;
            }
        }


        private ObservableCollection<int> _StageIndexs
             = new ObservableCollection<int>();
        [DataMember]
        public ObservableCollection<int> StageIndexs
        {
            get { return _StageIndexs; }
            set
            {
                if (value != _StageIndexs)
                {
                    _StageIndexs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _GroupIndex = 0;
        [DataMember]
        public int GroupIndex
        {
            get { return _GroupIndex; }
            set
            {
                if (value != _GroupIndex)
                {
                    _GroupIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _ChillerHotLimitTemp
             = new Element<double>() { Value = 120.0};
        /// <summary>
        /// 쿨런트 기화 온도
        /// </summary>
        [DataMember]
        public Element<double> ChillerHotLimitTemp
        {
            get { return _ChillerHotLimitTemp; }
            set
            {
                if (value != _ChillerHotLimitTemp)
                {
                    _ChillerHotLimitTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CoolantInTemp
            = new Element<double>();
        /// <summary>
        /// Chiller 구동 시작 온도.
        /// </summary>
        [DataMember]
        public Element<double> CoolantInTemp
        {
            get { return _CoolantInTemp; }
            set
            {
                if (value != _CoolantInTemp)
                {
                    _CoolantInTemp = value;
                    RaisePropertyChanged();
                }
            }
        }


        //private Element<double> _ChillerOffset
        //     = new Element<double>() { Value = -5.0};
        //[DataMember]
        //public Element<double> ChillerOffset
        //{
        //    get { return _ChillerOffset; }
        //    set
        //    {
        //        if (value != _ChillerOffset)
        //        {
        //            _ChillerOffset = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Element<double> _Tolerance
             = new Element<double>() { Value = 0};
        //칠러 허용 오차
        [DataMember]
        public Element<double> Tolerance
        {
            get { return _Tolerance; }
            set
            {
                if (value != _Tolerance)
                {
                    _Tolerance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _ActivatableHighTemp
             = new Element<double>() { Value = 30.0};
        /// <summary>
        /// Heater 키는 온도 
        /// </summary>
        [DataMember]
        public Element<double> ActivatableHighTemp
        {
            get { return _ActivatableHighTemp; }
            set
            {
                if (value != _ActivatableHighTemp)
                {
                    _ActivatableHighTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _InRangeWindowTemp
             = new Element<double>() { Value = 5.0 };
        /// <summary>
        /// Chiller 구동시 Heater 를 몇도의 Range 에 들어왔을때 킬것인지 ( + -) 0 도 이상일때
        /// </summary>
        [DataMember]
        public Element<double> InRangeWindowTemp
        {
            get { return _InRangeWindowTemp; }
            set
            {
                if (value != _InRangeWindowTemp)
                {
                    _InRangeWindowTemp = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _AmbientTemp
            = new Element<double>() { Value = 26.0};
        [DataMember]
        public Element<double> AmbientTemp
        {
            get { return _AmbientTemp; }
            set
            {
                if (value != _AmbientTemp)
                {
                    _AmbientTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _SlowPumpSpeed 
             = new Element<int>() { Value = 1000 };
        [DataMember]
        public Element<int> SlowPumpSpeed
        {
            get { return _SlowPumpSpeed; }
            set
            {
                if (value != _SlowPumpSpeed)
                {
                    _SlowPumpSpeed = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _NormalPumpSpeed 
            = new Element<int>() { Value = 1500 };
        [DataMember]
        public Element<int> NormalPumpSpeed
        {
            get { return _NormalPumpSpeed; }
            set
            {
                if (value != _NormalPumpSpeed)
                {
                    _NormalPumpSpeed = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _FastPumpSpeed 
            = new Element<int>() { Value = 1800 };
        [DataMember]
        public Element<int> FastPumpSpeed
        {
            get { return _FastPumpSpeed; }
            set
            {
                if (value != _FastPumpSpeed)
                {
                    _FastPumpSpeed = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _IsEnableUsePurge
            = new Element<bool>() { Value = false};
        [DataMember]
        public Element<bool> IsEnableUsePurge
        {
            get { return _IsEnableUsePurge; }
            set
            {
                if (value != _IsEnableUsePurge)
                {
                    _IsEnableUsePurge = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<Dictionary<double, double>> _CoolantOffsetDictionary
             = new Element<Dictionary<double, double>>() { Value = new Dictionary<double, double>()};
        [DataMember]
        public Element<Dictionary<double, double>> CoolantOffsetDictionary
        {
            get { return _CoolantOffsetDictionary; }
            set
            {
                if (value != _CoolantOffsetDictionary)
                {
                    _CoolantOffsetDictionary = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 칠러 동작이 강제로 멈춘것인지 알기위한 Flag. 
        /// 1. 로더를 껐다 켰을때 자동으로 재실행 되는것을 막기위해
        /// </summary>
        private bool _IsAbortActivate;
        [DataMember]
        public bool IsAbortActivate
        {
            get { return _IsAbortActivate; }
            set
            {
                if (value != _IsAbortActivate)
                {
                    _IsAbortActivate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _IsAutoValveControl = new Element<bool>();
        [DataMember]
        public Element<bool> IsAutoValveControl
        {
            get { return _IsAutoValveControl; }
            set { _IsAutoValveControl = value; }
        }

        public ChillerParameter()
        {
            InRangeWindowTemp.Value = 5.0;
            SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            try
            {
                CoolantOffsetDictionary.Value.Add(125, -7);
                CoolantOffsetDictionary.Value.Add(30, -12);
                CoolantOffsetDictionary.Value.Add(-30, -12);
                CoolantOffsetDictionary.Value.Add(-40, -15);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            SetDefaultParam();
            return EventCodeEnum.NONE;
        }

        public void SetElementMetaData()
        {
            try
            {
                var categoryID = "00010019";
                ChillerHotLimitTemp.CategoryID = categoryID;
                ChillerHotLimitTemp.ElementName = "ChillerHotLimitTemp";
                CoolantInTemp.CategoryID = categoryID;
                CoolantInTemp.ElementName = "CoolantInTemp";
                //ChillerOffset.CategoryID = categoryID;
                //ChillerOffset.ElementName = "ChillerOffset";
                Tolerance.CategoryID = categoryID;
                Tolerance.ElementName = "Tolerance";
                ActivatableHighTemp.CategoryID = categoryID;
                ActivatableHighTemp.ElementName = "ActivatableHighTemp";
                InRangeWindowTemp.CategoryID = categoryID;
                InRangeWindowTemp.ElementName = "InRangeWindowTemp";
                AmbientTemp.CategoryID = categoryID;
                AmbientTemp.ElementName = "AmbientTemp";
                SlowPumpSpeed.CategoryID = categoryID;
                SlowPumpSpeed.ElementName = "SlowPumpSpeed";
                NormalPumpSpeed.CategoryID = categoryID;
                NormalPumpSpeed.ElementName = "NormalPumpSpeed";
                FastPumpSpeed.CategoryID = categoryID;
                FastPumpSpeed.ElementName = "FastPumpSpeed";
                IsEnableUsePurge.CategoryID = categoryID;
                IsEnableUsePurge.ElementName = "IsEnableUsePurge";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }
    }
    public interface IChillerParameter
    {
        string IP { get; set; }
        int Port { get; set; }
        byte SubIndex { get; set; }
        ObservableCollection<int> StageIndexs { get; set; }
        Element<EnumChillerModuleMode> ChillerModuleMode { get; set; }
        Element<double> ChillerHotLimitTemp { get; set; }
        Element<double> CoolantInTemp { get; set; }
        //Element<double> ChillerOffset { get; set; }
        Element<double> Tolerance { get; set; }
        Element<double> ActivatableHighTemp { get; set; }
        Element<double> InRangeWindowTemp { get; set; }
        Element<double> AmbientTemp { get; set; }
        Element<int> SlowPumpSpeed { get; set; }
        Element<int> NormalPumpSpeed { get; set; }
        Element<int> FastPumpSpeed { get; set; }
        Element<bool> IsEnableUsePurge { get; set; }
        Element<Dictionary<double, double>> CoolantOffsetDictionary { get; set; }
        bool IsAbortActivate { get; set; }
        Element<bool> IsAutoValveControl { get; set; }
    }

}
