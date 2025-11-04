using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Temperature.Temp.DryAir
{
    [Serializable]
    public class DryAirNetIOMappings : INotifyPropertyChanged, IParam, ISystemParameterizable, IParamNode
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
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

        public string FilePath { get; } = "\\Temperature\\Chiller\\";

        public string FileName { get; } = "HuberIOMapParam.json";
        [field: NonSerialized, JsonIgnore]

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string info = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            _Inputs.SetDefaultParam();
            _Outputs.SetDefaultParam();

            return EventCodeEnum.NONE;
        }

        public void SetElementMetaData()
        {

        }

        private HBInputPortDefinitions _Inputs = new HBInputPortDefinitions();

        public HBInputPortDefinitions Inputs
        {
            get { return _Inputs; }
            set
            {
                if (value != this._Inputs)
                {
                    _Inputs = value;
                    RaisePropertyChanged();
                }
            }
        }
        private HBOutputPortDefinitions _Outputs = new HBOutputPortDefinitions();

        public HBOutputPortDefinitions Outputs
        {
            get { return _Outputs; }
            set
            {
                if (value != this._Outputs)
                {
                    _Outputs = value;
                    RaisePropertyChanged();
                }
            }
        }

    }

    [Serializable]
    public class HBInputPortDefinitions : INotifyPropertyChanged, IParamNode
    {
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

        public List<object> Nodes { get; set; }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string info = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        #region // HB NetIO Inputs

        private IOPortDescripter<bool> _DI_LeakSensor0 
            = new IOPortDescripter<bool>(nameof(DI_LeakSensor0), EnumIOType.INPUT);
        public IOPortDescripter<bool> DI_LeakSensor0
        {
            get { return _DI_LeakSensor0; }
            set
            {
                if (value != this._DI_LeakSensor0)
                {
                    _DI_LeakSensor0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DI_LeakSensor1
            = new IOPortDescripter<bool>(nameof(DI_LeakSensor1), EnumIOType.INPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DI_LeakSensor1
        {
            get { return _DI_LeakSensor1; }
            set
            {
                if (value != this._DI_LeakSensor1)
                {
                    _DI_LeakSensor1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DI_LeakSensor2
            = new IOPortDescripter<bool>(nameof(DI_LeakSensor2), EnumIOType.INPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DI_LeakSensor2
        {
            get { return _DI_LeakSensor2; }
            set
            {
                if (value != this._DI_LeakSensor2)
                {
                    _DI_LeakSensor2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DI_LeakSensor3
            = new IOPortDescripter<bool>(nameof(DI_LeakSensor3), EnumIOType.INPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DI_LeakSensor3
        {
            get { return _DI_LeakSensor3; }
            set
            {
                if (value != this._DI_LeakSensor3)
                {
                    _DI_LeakSensor3 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DI_LeakSensor4 
            = new IOPortDescripter<bool>(nameof(DI_LeakSensor4), EnumIOType.INPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DI_LeakSensor4
        {
            get { return _DI_LeakSensor4; }
            set
            {
                if (value != this._DI_LeakSensor4)
                {
                    _DI_LeakSensor4 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DI_LeakSensor5 
            = new IOPortDescripter<bool>(nameof(DI_LeakSensor5), EnumIOType.INPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DI_LeakSensor5
        {
            get { return _DI_LeakSensor5; }
            set
            {
                if (value != this._DI_LeakSensor5)
                {
                    _DI_LeakSensor5 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DI_LeakSensor6
            = new IOPortDescripter<bool>(nameof(DI_LeakSensor6), EnumIOType.INPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DI_LeakSensor6
        {
            get { return _DI_LeakSensor6; }
            set
            {
                if (value != this._DI_LeakSensor6)
                {
                    _DI_LeakSensor6 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DI_LeakSensor7
            = new IOPortDescripter<bool>(nameof(DI_LeakSensor7), EnumIOType.INPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DI_LeakSensor7
        {
            get { return _DI_LeakSensor7; }
            set
            {
                if (value != this._DI_LeakSensor7)
                {
                    _DI_LeakSensor7 = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public void SetDefaultParam()
        {
            HBTestDefaultParam();
        }
        public void HBTestDefaultParam()
        {
            try
            {
                #region Input

                this.DI_LeakSensor0.ChannelIndex.Value = 13;
                this.DI_LeakSensor0.PortIndex.Value = 0;
                this.DI_LeakSensor0.IOOveride.Value = EnumIOOverride.NONE;

                this.DI_LeakSensor1.ChannelIndex.Value = 13;
                this.DI_LeakSensor1.PortIndex.Value = 1;
                this.DI_LeakSensor1.IOOveride.Value = EnumIOOverride.NLO;

                this.DI_LeakSensor2.ChannelIndex.Value = 13;
                this.DI_LeakSensor2.PortIndex.Value = 2;
                this.DI_LeakSensor2.IOOveride.Value = EnumIOOverride.NLO;

                this.DI_LeakSensor3.ChannelIndex.Value = 13;
                this.DI_LeakSensor3.PortIndex.Value = 3;
                this.DI_LeakSensor3.IOOveride.Value = EnumIOOverride.NLO;

                this.DI_LeakSensor4.ChannelIndex.Value = 13;
                this.DI_LeakSensor4.PortIndex.Value = 4;
                this.DI_LeakSensor4.IOOveride.Value = EnumIOOverride.NLO;

                this.DI_LeakSensor5.ChannelIndex.Value = 13;
                this.DI_LeakSensor5.PortIndex.Value = 5;
                this.DI_LeakSensor5.IOOveride.Value = EnumIOOverride.NLO;

                this.DI_LeakSensor6.ChannelIndex.Value = 13;
                this.DI_LeakSensor6.PortIndex.Value = 6;
                this.DI_LeakSensor6.IOOveride.Value = EnumIOOverride.NLO;

                this.DI_LeakSensor7.ChannelIndex.Value = 13;
                this.DI_LeakSensor7.PortIndex.Value = 7;
                this.DI_LeakSensor7.IOOveride.Value = EnumIOOverride.NLO;

                #endregion

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
    }

    [Serializable]
    public class HBOutputPortDefinitions : INotifyPropertyChanged, IParamNode
    {
        [JsonIgnore]
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

        [JsonIgnore]
        public List<object> Nodes { get; set; }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string info = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        #region // HB NetIO Outputs

        private IOPortDescripter<bool> _DI_SUPPLY_SV
            = new IOPortDescripter<bool>(nameof(DO_SUPPLY_SV), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DO_SUPPLY_SV
        {
            get { return _DI_SUPPLY_SV; }
            set
            {
                if (value != this._DI_SUPPLY_SV)
                {
                    _DI_SUPPLY_SV = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DO_Return_SV 
            = new IOPortDescripter<bool>(nameof(DO_Return_SV), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DO_Return_SV
        {
            get { return _DO_Return_SV; }
            set
            {
                if (value != this._DO_Return_SV)
                {
                    _DO_Return_SV = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DO_AIRPURGE_SV 
            = new IOPortDescripter<bool>(nameof(DO_AIRPURGE_SV), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DO_AIRPURGE_SV
        {
            get { return _DO_AIRPURGE_SV; }
            set
            {
                if (value != this._DO_AIRPURGE_SV)
                {
                    _DO_AIRPURGE_SV = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DO_E_RETURN_SV 
            = new IOPortDescripter<bool>(nameof(DO_E_RETURN_SV), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DO_E_RETURN_SV
        {
            get { return _DO_E_RETURN_SV; }
            set
            {
                if (value != this._DO_E_RETURN_SV)
                {
                    _DO_E_RETURN_SV = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DO_T_RETURN_SV
            = new IOPortDescripter<bool>(nameof(DO_T_RETURN_SV), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DO_T_RETURN_SV
        {
            get { return _DO_T_RETURN_SV; }
            set
            {
                if (value != this._DO_T_RETURN_SV)
                {
                    _DO_T_RETURN_SV = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DO_DRYAIR_STGSV1 
            = new IOPortDescripter<bool>(nameof(DO_DRYAIR_STGSV1), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_DRYAIR_STGSV1
        {
            get { return _DO_DRYAIR_STGSV1; }
            set
            {
                if (value != this._DO_DRYAIR_STGSV1)
                {
                    _DO_DRYAIR_STGSV1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DO_DRYAIR_STGSV2
            = new IOPortDescripter<bool>(nameof(DO_DRYAIR_STGSV2), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_DRYAIR_STGSV2
        {
            get { return _DO_DRYAIR_STGSV2; }
            set
            {
                if (value != this._DO_DRYAIR_STGSV2)
                {
                    _DO_DRYAIR_STGSV2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DO_DRYAIR_LDSV
            = new IOPortDescripter<bool>(nameof(DO_DRYAIR_LDSV), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_DRYAIR_LDSV
        {
            get { return _DO_DRYAIR_LDSV; }
            set
            {
                if (value != this._DO_DRYAIR_LDSV)
                {
                    _DO_DRYAIR_LDSV = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DO_DRYAIR_FOR_TESTER
            = new IOPortDescripter<bool>(nameof(DO_DRYAIR_FOR_TESTER), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_DRYAIR_FOR_TESTER
        {
            get { return _DO_DRYAIR_FOR_TESTER; }
            set
            {
                if (value != this._DO_DRYAIR_FOR_TESTER)
                {
                    _DO_DRYAIR_FOR_TESTER = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public void SetDefaultParam()
        {
            HBTestDefaultParam();
        }
        public void HBTestDefaultParam()
        {
            try
            {
                #region OUTPUT

                this.DO_SUPPLY_SV.ChannelIndex.Value = 11;
                this.DO_SUPPLY_SV.PortIndex.Value = 0;
                this.DO_SUPPLY_SV.IOOveride.Value = EnumIOOverride.NLO;

                this.DO_Return_SV.ChannelIndex.Value = 11;
                this.DO_Return_SV.PortIndex.Value = 1;
                this.DO_Return_SV.IOOveride.Value = EnumIOOverride.NLO;

                this.DO_AIRPURGE_SV.ChannelIndex.Value = 11;
                this.DO_AIRPURGE_SV.PortIndex.Value = 2;
                this.DO_AIRPURGE_SV.IOOveride.Value = EnumIOOverride.NLO;

                this.DO_E_RETURN_SV.ChannelIndex.Value = 11;
                this.DO_E_RETURN_SV.PortIndex.Value = 3;
                this.DO_E_RETURN_SV.IOOveride.Value = EnumIOOverride.NLO;

                this.DO_T_RETURN_SV.ChannelIndex.Value = 11;
                this.DO_T_RETURN_SV.PortIndex.Value = 4;
                this.DO_T_RETURN_SV.IOOveride.Value = EnumIOOverride.NLO;

                this.DO_DRYAIR_STGSV1.ChannelIndex.Value = 11;
                this.DO_DRYAIR_STGSV1.PortIndex.Value = 5;
                this.DO_DRYAIR_STGSV1.IOOveride.Value = EnumIOOverride.NONE;

                this.DO_DRYAIR_STGSV2.ChannelIndex.Value = 11;
                this.DO_DRYAIR_STGSV2.PortIndex.Value = 6;
                this.DO_DRYAIR_STGSV2.IOOveride.Value = EnumIOOverride.NONE;

                this.DO_DRYAIR_LDSV.ChannelIndex.Value = 11;
                this.DO_DRYAIR_LDSV.PortIndex.Value = 7;
                this.DO_DRYAIR_LDSV.IOOveride.Value = EnumIOOverride.NONE;

                this.DO_DRYAIR_FOR_TESTER.ChannelIndex.Value = 11;
                this.DO_DRYAIR_FOR_TESTER.PortIndex.Value = 0;
                this.DO_DRYAIR_FOR_TESTER.IOOveride.Value = EnumIOOverride.NONE;

                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}


