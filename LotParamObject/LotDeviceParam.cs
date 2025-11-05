namespace LotParamObject
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    [Serializable]
    public class LotDeviceParam : ILotDeviceParam, INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; } = "";

        public string FileName { get; } = "LotDevice.json";

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

        private LotStopOption _StopOption;
        public LotStopOption StopOption
        {
            get { return _StopOption; }
            set
            {
                if (value != _StopOption)
                {
                    _StopOption = value;
                    RaisePropertyChanged();
                }
            }
        }


        [XmlIgnore, JsonIgnore]
        private LotStopOption _OperatorStopOption;
        public LotStopOption OperatorStopOption
        {
            get { return _OperatorStopOption; }
            set
            {
                if (value != _OperatorStopOption)
                {
                    _OperatorStopOption = value;
                    RaisePropertyChanged();
                }
            }
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
            }

            return retval;
        }
        public void SetElementMetaData()
        {
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                StopOption = new LotStopOption();
                StopOption.StopAfterScanCassette.Value = false;
                StopOption.StopAfterWaferLoad.Value = false;
                StopOption.EveryStopBeforeProbing.Value = false;
                StopOption.EveryStopAfterProbing.Value = false;


                StopOption.StopBeforeProbing.Value = false;
                StopOption.StopAfterProbing.Value = false;

                StopOption.StopAfterProbingFlag.Value = new System.Collections.ObjectModel.ObservableCollection<bool>();
                StopOption.StopBeforeProbingFlag.Value = new System.Collections.ObjectModel.ObservableCollection<bool>();
                for(int i=0;i<25;i++)
                {
                    StopOption.StopAfterProbingFlag.Value.Add(false);
                    StopOption.StopBeforeProbingFlag.Value.Add(false);
                }
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SetDefaultParam();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
