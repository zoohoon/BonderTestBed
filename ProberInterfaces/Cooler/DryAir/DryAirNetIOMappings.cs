using System;
using System.Collections.Generic;

namespace ProberInterfaces.Cooler.DryAir
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    public class DryAirNetIOMappings : INotifyPropertyChanged, IParam, ISystemParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }

        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
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
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [JsonIgnore]
        public string FilePath { get; set; } = @"\Temperature\Chiller\Huber\";

        [JsonIgnore]
        public string FileName { get; set; } = "HBIOMapParam.json";

        [JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [JsonIgnore, ParamIgnore]
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
        = new List<object>();
    }

    public class HBInputPortDefinitions : INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #region // HB NetIO Inputs
        private IOPortDescripter<bool> _DI_LeakSensor0 = new IOPortDescripter<bool>("DI_LeakSensor0", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_LeakSensor0
        {
            get { return _DI_LeakSensor0; }
            set
            {
                if (value != this._DI_LeakSensor0)
                {
                    _DI_LeakSensor0 = value;
                    NotifyPropertyChanged("DI_LeakSensor0");
                }
            }
        }

        private IOPortDescripter<bool> _DI_LeakSensor1 = new IOPortDescripter<bool>("DI_LeakSensor1", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_LeakSensor1
        {
            get { return _DI_LeakSensor1; }
            set
            {
                if (value != this._DI_LeakSensor1)
                {
                    _DI_LeakSensor1 = value;
                    NotifyPropertyChanged("DI_LeakSensor1");
                }
            }
        }

        private IOPortDescripter<bool> _DI_LeakSensor2 = new IOPortDescripter<bool>("DI_LeakSensor2", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_LeakSensor2
        {
            get { return _DI_LeakSensor2; }
            set
            {
                if (value != this._DI_LeakSensor2)
                {
                    _DI_LeakSensor2 = value;
                    NotifyPropertyChanged("DI_LeakSensor2");
                }
            }
        }

        private IOPortDescripter<bool> _DI_LeakSensor3 = new IOPortDescripter<bool>("DI_LeakSensor3", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_LeakSensor3
        {
            get { return _DI_LeakSensor3; }
            set
            {
                if (value != this._DI_LeakSensor3)
                {
                    _DI_LeakSensor3 = value;
                    NotifyPropertyChanged("DI_LeakSensor3");
                }
            }
        }

        private IOPortDescripter<bool> _DI_LeakSensor4 = new IOPortDescripter<bool>("DI_LeakSensor4", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_LeakSensor4
        {
            get { return _DI_LeakSensor4; }
            set
            {
                if (value != this._DI_LeakSensor4)
                {
                    _DI_LeakSensor4 = value;
                    NotifyPropertyChanged("DI_LeakSensor4");
                }
            }
        }

        private IOPortDescripter<bool> _DI_LeakSensor5 = new IOPortDescripter<bool>("DI_LeakSensor5", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_LeakSensor5
        {
            get { return _DI_LeakSensor5; }
            set
            {
                if (value != this._DI_LeakSensor5)
                {
                    _DI_LeakSensor5 = value;
                    NotifyPropertyChanged("DI_LeakSensor5");
                }
            }
        }

        private IOPortDescripter<bool> _DI_LeakSensor6 = new IOPortDescripter<bool>("DI_LeakSensor6", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_LeakSensor6
        {
            get { return _DI_LeakSensor6; }
            set
            {
                if (value != this._DI_LeakSensor6)
                {
                    _DI_LeakSensor6 = value;
                    NotifyPropertyChanged("DI_LeakSensor6");
                }
            }
        }

        private IOPortDescripter<bool> _DI_LeakSensor7 = new IOPortDescripter<bool>("DI_LeakSensor7", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_LeakSensor7
        {
            get { return _DI_LeakSensor7; }
            set
            {
                if (value != this._DI_LeakSensor7)
                {
                    _DI_LeakSensor7 = value;
                    NotifyPropertyChanged("DI_LeakSensor7");
                }
            }
        }

        //private IOPortDescripter<bool> _DI_FO_UP = new IOPortDescripter<bool>("DI_FO_UP", EnumIOType.INPUT);

        //public IOPortDescripter<bool> DI_FO_UP
        //{
        //    get { return _DI_FO_UP; }
        //    set
        //    {
        //        if (value != this._DI_FO_UP)
        //        {
        //            _DI_FO_UP = value;
        //            NotifyPropertyChanged("DI_FO_UP");
        //        }
        //    }
        //}

        #endregion
    }

    public class HBOutputPortDefinitions : INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #region // HB NetIO Outputs

        private IOPortDescripter<bool> _DI_SUPPLY_SV = new IOPortDescripter<bool>("DO_SUPPLY_SV ", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_SUPPLY_SV
        {
            get { return _DI_SUPPLY_SV; }
            set
            {
                if (value != this._DI_SUPPLY_SV)
                {
                    _DI_SUPPLY_SV = value;
                    NotifyPropertyChanged("DO_SUPPLY_SV");
                }
            }
        }

        private IOPortDescripter<bool> _DO_Return_SV = new IOPortDescripter<bool>("DO_Return_SV", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_Return_SV
        {
            get { return _DO_Return_SV; }
            set
            {
                if (value != this._DO_Return_SV)
                {
                    _DO_Return_SV = value;
                    NotifyPropertyChanged("DO_Return_SV");
                }
            }
        }

        private IOPortDescripter<bool> _DO_AIRPURGE_SV = new IOPortDescripter<bool>("DO_AIRPURGE_SV", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_AIRPURGE_SV
        {
            get { return _DO_AIRPURGE_SV; }
            set
            {
                if (value != this._DO_AIRPURGE_SV)
                {
                    _DO_AIRPURGE_SV = value;
                    NotifyPropertyChanged("DO_AIRPURGE_SV");
                }
            }
        }

        private IOPortDescripter<bool> _DO_E_RETURN_SV = new IOPortDescripter<bool>("DO_E_RETURN_SV", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_E_RETURN_SV
        {
            get { return _DO_E_RETURN_SV; }
            set
            {
                if (value != this._DO_E_RETURN_SV)
                {
                    _DO_E_RETURN_SV = value;
                    NotifyPropertyChanged("DO_E_RETURN_SV");
                }
            }
        }

        private IOPortDescripter<bool> _DO_T_RETURN_SV = new IOPortDescripter<bool>("DO_T_RETURN_SV", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_T_RETURN_SV
        {
            get { return _DO_T_RETURN_SV; }
            set
            {
                if (value != this._DO_T_RETURN_SV)
                {
                    _DO_T_RETURN_SV = value;
                    NotifyPropertyChanged("DO_T_RETURN_SV");
                }
            }
        }

        private IOPortDescripter<bool> _DO_DRYAIR_STGSV1 = new IOPortDescripter<bool>("DO_DRYAIR_STGSV1", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_DRYAIR_STGSV1
        {
            get { return _DO_DRYAIR_STGSV1; }
            set
            {
                if (value != this._DO_DRYAIR_STGSV1)
                {
                    _DO_DRYAIR_STGSV1 = value;
                    NotifyPropertyChanged("DO_DRYAIR_STGSV1");
                }
            }
        }

        private IOPortDescripter<bool> _DO_DRYAIR_STGSV2 = new IOPortDescripter<bool>("DO_DRYAIR_STGSV2", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_DRYAIR_STGSV2
        {
            get { return _DO_DRYAIR_STGSV2; }
            set
            {
                if (value != this._DO_DRYAIR_STGSV2)
                {
                    _DO_DRYAIR_STGSV2 = value;
                    NotifyPropertyChanged("DO_DRYAIR_STGSV2");
                }
            }
        }

        private IOPortDescripter<bool> _DO_DRYAIR_LDSV = new IOPortDescripter<bool>("DO_DRYAIR_LDSV", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_DRYAIR_LDSV
        {
            get { return _DO_DRYAIR_LDSV; }
            set
            {
                if (value != this._DO_DRYAIR_LDSV)
                {
                    _DO_DRYAIR_LDSV = value;
                    NotifyPropertyChanged("DO_DRYAIR_LDSV");
                }
            }
        }

        private IOPortDescripter<bool> _DO_DRYAIR_FOR_TESTER = new IOPortDescripter<bool>("DO_DRYAIR_FOR_TESTER", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_DRYAIR_FOR_TESTER
        {
            get { return _DO_DRYAIR_FOR_TESTER; }
            set
            {
                if (value != this._DO_DRYAIR_FOR_TESTER)
                {
                    _DO_DRYAIR_FOR_TESTER = value;
                    NotifyPropertyChanged("DO_DRYAIR_FOR_TESTER");
                }
            }
        }

        #endregion
    }
}
