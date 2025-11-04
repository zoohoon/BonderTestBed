using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Xml.Serialization;
using ProberErrorCode;
using LogModule;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using ProberInterfaces.Enum;

namespace Temperature.Temp.DewPoint
{
    [Serializable]
    public class DewpointSysParameter : INotifyPropertyChanged, ISystemParameterizable, IParamNode
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }


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

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChange([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));



        #region Properties
        private Element<DewPointTypeEnum> _ModuleType = new Element<DewPointTypeEnum>();

        public Element<DewPointTypeEnum> ModuleType
        {
            get { return _ModuleType; }
            set { _ModuleType = value; }
        }

        private Element<double> _SensorCoeff = new Element<double>();

        public Element<double> SensorCoeff
        {
            get { return _SensorCoeff; }
            set { _SensorCoeff = value; }
        }

        private Element<int> _DevIndex = new Element<int>();
        public Element<int> DevIndex
        {
            get { return _DevIndex; }
            set { _DevIndex = value; }
        }

        private Element<int> _ChannelIndex = new Element<int>();
        public Element<int> ChannelIndex
        {
            get { return _ChannelIndex; }
            set { _ChannelIndex = value; }
        }
        private Element<double> _DewPointMaxValue = new Element<double>();
        public Element<double> DewPointMaxValue
        {
            get { return _DewPointMaxValue; }
            set { _DewPointMaxValue = value; }
        }
        private Element<double> _DewPointMinValue = new Element<double>();
        public Element<double> DewPointMinValue
        {
            get { return _DewPointMinValue; }
            set { _DewPointMinValue = value; }
        }
        private Element<double> _DewPointADMin = new Element<double>();
        public Element<double> DewPointADMin
        {
            get { return _DewPointADMin; }
            set { _DewPointADMin = value; }
        }
        private Element<double> _DewPointADMax = new Element<double>();
        public Element<double> DewPointADMax
        {
            get { return _DewPointADMax; }
            set { _DewPointADMax = value; }
        }

        private Element<int> _UpdateInterval = new Element<int>();

        public Element<int> UpdateInterval
        {
            get { return _UpdateInterval; }
            set { _UpdateInterval = value; }
        }
        private Element<double> _MinAvaDewPoint = new Element<double>();
        public Element<double> MinAvaDewPoint
        {
            get { return _MinAvaDewPoint; }
            set { _MinAvaDewPoint = value; }
        }


        #endregion

        public void SetElementMetaData()
        {
            SensorCoeff.Description = "Coeff. for sensor value conversion";
            SensorCoeff.UpperLimit = 100000.0;
            SensorCoeff.LowerLimit = 0.0;
            //SensorCoeff.Value = 14.1667;
            //SensorCoeff.CategoryID = ;

            DevIndex.Description = "Dew point input device index";
            DevIndex.UpperLimit = 999;
            DevIndex.LowerLimit = 0;
            //DevIndex.Value = 0;
            //DevIndex.CategoryID = ;

            ChannelIndex.Description = "Dew point input channel";
            ChannelIndex.UpperLimit = 999;
            ChannelIndex.LowerLimit = 0;
            //ChannelIndex.Value = 0;
            //ChannelIndex.CategoryID = ;

            DewPointMaxValue.Description = "Maximum Dew point value(dP°C)";
            DewPointMaxValue.UpperLimit = 20.0;
            DewPointMaxValue.LowerLimit = -70.0;
            // DewPointMaxValue.Value = 200;
            //DewPointMaxValue.CategoryID = ;

            DewPointMinValue.Description = "Minimum Dew point value(dP°C)";
            DewPointMinValue.UpperLimit = 20.0;
            DewPointMinValue.LowerLimit = -100.0;

            UpdateInterval.Description = "Dew point update interval in ms";
            UpdateInterval.UpperLimit = 10000;
            UpdateInterval.LowerLimit = 1;
            //UpdateInterval.CategoryID = ;

            ModuleType.Description = "Dew point Communication Type";
            //UpdateInterval.Value = 1000;
            //UpdateInterval.CategoryID = ;

            DewPointADMin.Description = "Dew point Analog digital Minimum value";
            DewPointADMin.UpperLimit = 65536;
            DewPointADMin.LowerLimit = 0;
            //UpdateInterval.CategoryID = ;

            DewPointADMax.Description = "Dew point Analog digital Maximum value";
            DewPointADMax.UpperLimit = 65536;
            DewPointADMax.LowerLimit = 0;
            //UpdateInterval.CategoryID = ;

            MinAvaDewPoint.Description = "Minimum Available Dew Point Value(dP°C)";
            MinAvaDewPoint.UpperLimit = 20.0;
            MinAvaDewPoint.LowerLimit = -100;
            //UpdateInterval.CategoryID = ;
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        public EventCodeEnum SetDefaultParam()
        {
            IsAttached.Value = false;
            PortName.Value = "COM1";
            BaudRate.Value = 9600;
            DataBits.Value = 8;
            StopBits.Value = System.IO.Ports.StopBits.One;
            Parity.Value = System.IO.Ports.Parity.None;

            ModAddress.Value = 1;
            DewPointOffset.Value = 0;

            SensorCoeff.Value = 14.1667;
            DevIndex.Value = 0;
            ChannelIndex.Value = 0;
            DewPointMaxValue.Value = 20.0;
            DewPointMinValue.Value = -100.0;
            UpdateInterval.Value = 1000;
            ModuleType.Value = DewPointTypeEnum.DIRECT_IO_TYPE;
            //DewPointADMin.Value = 4000;
            DewPointADMin.Value = 1000;
            //DewPointADMax.Value = 65535;
            DewPointADMax.Value = 32767;
            MinAvaDewPoint.Value = -80;
            Tolerence.Value = 1;
            return EventCodeEnum.NONE;
        }

        private Element<bool> _IsAttached
             = new Element<bool>();
        public Element<bool> IsAttached
        {
            get
            {
                return _IsAttached;
            }
            set
            {
                if(_IsAttached != value)
                {
                    _IsAttached = value;
                    RaisePropertyChange();
                }
            }
        }

        private Element<String> _PortName
             = new Element<string>();
        public Element<String> PortName
        {
            get { return _PortName; }
            set
            {
                if (_PortName != value)
                {
                    _PortName = value;
                    RaisePropertyChange();
                }
            }
        }
        private Element<int> _BaudRate
             = new Element<int>();
        public Element<int> BaudRate
        {
            get { return _BaudRate; }
            set
            {
                if(_BaudRate != value)
                {
                    _BaudRate = value;
                    RaisePropertyChange();
                }
            }
        }
        private Element<int> _DataBits
             = new Element<int>();
        public Element<int> DataBits
        {
            get { return _DataBits; }
            set
            {
                if(_DataBits != value)
                {
                    _DataBits = value;
                    RaisePropertyChange();
                }
            }
        }
        private Element<StopBits> _StopBits
             = new Element<StopBits>();
        public Element<StopBits> StopBits
        {
            get { return _StopBits; }
            set
            {
                if(_StopBits != value)
                {
                    _StopBits = value;
                    RaisePropertyChange();
                }
            }
        }
        private Element<Parity> _Parity
             = new Element<Parity>();
        public Element<Parity> Parity
        {
            get { return _Parity; }
            set
            {
                if(_Parity != value)
                {
                    _Parity = value;
                    RaisePropertyChange();
                }
            }
        }

        private Element<int> _ModAddress
             = new Element<int>();
        public Element<int> ModAddress
        {
            get { return _ModAddress; }
            set
            {
                if(_ModAddress != value)
                {
                    _ModAddress = value;
                    RaisePropertyChange();
                }
            }
        }

        // 온도의 * 10한 값.
        private Element<double> _DewPointOffset
            = new Element<double>();
        public Element<double> DewPointOffset
        {
            get { return _DewPointOffset; }
            set
            {
                if (_DewPointOffset != value)
                {
                    _DewPointOffset = value;
                    RaisePropertyChange();

                }
            }
        }
        private Element<double> _Tolerence
            = new Element<double>() ;
        public Element<double> Tolerence
        {
            get { return _Tolerence; }
            set
            {
                if (_Tolerence != value)
                {
                    _Tolerence = value;
                    RaisePropertyChange();

                }
            }
        }

        private Element<double> _Hysteresis = new Element<double>() { Value = 5 };

        public Element<double> Hysteresis
        {
            get { return _Hysteresis; }
            set
            {
                if (_Hysteresis != value)
                {
                    _Hysteresis = value;
                    RaisePropertyChange();

                }
            }
        }


        private Element<long> _WaitTimeout
            = new Element<long>(){ Value = 60000 };
        public Element<long> WaitTimeout
        {
            get { return _WaitTimeout; }
            set
            {
                if (_WaitTimeout != value)
                {
                    _WaitTimeout = value;
                    RaisePropertyChange();

                }
            }
        }

        [ParamIgnore]
        public string FilePath { get; } = "Temperature";
        [ParamIgnore]
        public string FileName { get; } = nameof(DewpointSysParameter) + ".Json";
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

    }
}
