
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using ProberInterfaces.Temperature.TempManager;
using TempControllerParameter;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LogModule;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace TempManager
{
    [Serializable]
    [DataContract]
    public class TempSysParam : ISystemParameterizable , IParamNode, INotifyPropertyChanged, IParam
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
        
        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName]string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                

                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }

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

        private TempCommInfoParam _TempCommInfoParam;
        private Element<Dictionary<double, double>> _HeaterOffsetDictionary;
        private Element<Dictionary<double, ITCParamArgs>> _TCParamArgsDictionary;

        [DataMember]
        public TempCommInfoParam TempCommInfoParam
        {
            get { return _TempCommInfoParam; }
            set
            {
                if (value != _TempCommInfoParam)
                {
                    _TempCommInfoParam = value;
                    NotifyPropertyChanged(nameof(TempCommInfoParam));
                }
            }
        }
        [DataMember]
        public Element<Dictionary<double, double>> HeaterOffsetDictionary
        {
            get { return _HeaterOffsetDictionary; }
            set
            {
                _HeaterOffsetDictionary = value;
            }
        }
        [DataMember]
        public Element<Dictionary<double, ITCParamArgs>> TCParamArgsDictionary
        {
            get { return _TCParamArgsDictionary; }
            set
            {
                _TCParamArgsDictionary = value;
            }
        }
        /// <summary>
        /// TC_CommunicationInfo.json에 있는 "HeaterOffsetDictionary"의 Set Temp들을 체크할지 안할지 결정하는 파라미터
        /// True: 체크함; Temp Table에 현재 Device의 SetTemp가 포함되어 있지 않으면 Lot Pause처리 됨
        /// Flase: 체크하지 않음; Temp Table에 현재 Device의 Set Temp가 포함 되어있든 아니든 상관안함.
        /// </summary>
        private Element<bool> _CheckingTCTempTable = new Element<bool> { Value = false };
        [DataMember]
        public Element<bool> CheckingTCTempTable
        {
            get { return _CheckingTCTempTable; }
            set
            {
                _CheckingTCTempTable = value;
            }
        }

        [ParamIgnore]
        public string FilePath { get; } = "Temperature";
        [ParamIgnore]
        public string FileName { get; } = "TC_CommunicationInfo.json";
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            TempCommInfoParam = new TempCommInfoParam();
            TempCommInfoParam.SetDefaultData();

            HeaterOffsetDictionary = new Element<Dictionary<double, double>>();
            HeaterOffsetDictionary.Value = new Dictionary<double, double>();
            HeaterOffsetDictionary.Value.Clear();
            //for (double i = -6; i <= 20; i += 0.5)
            //{
            //    HeaterOffsetDictionary.Value.Add(i, 0);
            //}

            for (double i = -700; i <= 2000; i += 100)
            {
                HeaterOffsetDictionary.Value.Add(i, i);
            }

            TCParamArgsDictionary = new Element<Dictionary<double, ITCParamArgs>>();
            TCParamArgsDictionary.Value = new Dictionary<double, ITCParamArgs>();
            TCParamArgsDictionary.Value.Clear();
            for (int i = -4; i < 16; i++)
            {
                TCParamArgsDictionary.Value.Add(i * 100,
                new TCParamArgs()
                {
                    Pb = new Element<double>() { Value = 10 }
                                    ,
                    iT = new Element<double>() { Value = 100 }
                                    ,
                    dE = new Element<double>() { Value = 10 }
                });
            }
            CheckingTCTempTable.Value = false;
            return EventCodeEnum.NONE;
        }
    }
}
