using LogModule;
using Newtonsoft.Json;

using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ProberInterfaces.Foup
{
    [Serializable]
    public class FoupIOMappings : INotifyPropertyChanged, ISystemParameterizable, IParam, IParamNode
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

        public string FilePath { get; } = "Foup";

        public string FileName { get; } = "FoupIOMapParam.json";
        [field: NonSerialized, JsonIgnore]

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
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
        #region // Device indices
        //private List<IODevAddress> _InputDevAddresses = new List<IODevAddress>();

        //public List<IODevAddress> InputDevAddresses
        //{
        //    get { return _InputDevAddresses; }
        //    set { _InputDevAddresses = value; }
        //}
        //private List<IODevAddress> _OutputDevAddresses = new List<IODevAddress>();

        //public List<IODevAddress> OutputDevAddresses
        //{
        //    get { return _OutputDevAddresses; }
        //    set { _OutputDevAddresses = value; }
        //}
        #endregion

        private FoupInputPortDefinitions _Inputs = new FoupInputPortDefinitions();

        public FoupInputPortDefinitions Inputs
        {
            get { return _Inputs; }
            set
            {
                if (value != this._Inputs)
                {
                    _Inputs = value;
                    NotifyPropertyChanged("Inputs");
                }
            }
        }
        private FoupOutputPortDefinitions _Outputs = new FoupOutputPortDefinitions();

        public FoupOutputPortDefinitions Outputs
        {
            get { return _Outputs; }
            set
            {
                if (value != this._Outputs)
                {
                    _Outputs = value;
                    NotifyPropertyChanged("Outputs");
                }
            }
        }
    }

    [Serializable]
    public class FoupInputPortDefinitions : INotifyPropertyChanged, IParamNode
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

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #region // Foup Inputs

        private IOPortDescripter<bool> _DI_WAFER_OUT = new IOPortDescripter<bool>("DI_WAFER_OUT", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_WAFER_OUT
        {
            get { return _DI_WAFER_OUT; }
            set
            {
                if (value != this._DI_WAFER_OUT)
                {
                    _DI_WAFER_OUT = value;
                    NotifyPropertyChanged("DI_WAFER_OUT");
                }
            }
        }
        private IOPortDescripter<bool> _DI_FO_UP = new IOPortDescripter<bool>("DI_FO_UP", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_FO_UP
        {
            get { return _DI_FO_UP; }
            set
            {
                if (value != this._DI_FO_UP)
                {
                    _DI_FO_UP = value;
                    NotifyPropertyChanged("DI_FO_UP");
                }
            }
        }
        private IOPortDescripter<bool> _DI_FO_DOWN = new IOPortDescripter<bool>("DI_FO_DOWN", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_FO_DOWN
        {
            get { return _DI_FO_DOWN; }
            set
            {
                if (value != this._DI_FO_DOWN)
                {
                    _DI_FO_DOWN = value;
                    NotifyPropertyChanged("DI_FO_DOWN");
                }
            }
        }
        private IOPortDescripter<bool> _DI_LOAD_SWITCH = new IOPortDescripter<bool>("DI_LOAD_SWITCH", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_LOAD_SWITCH
        {
            get { return _DI_LOAD_SWITCH; }
            set
            {
                if (value != this._DI_LOAD_SWITCH)
                {
                    _DI_LOAD_SWITCH = value;
                    NotifyPropertyChanged("DI_LOAD_SWITCH");
                }
            }
        }
        private IOPortDescripter<bool> _DI_UNLOAD_SWITCH = new IOPortDescripter<bool>("DI_UNLOAD_SWITCH", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_UNLOAD_SWITCH
        {
            get { return _DI_UNLOAD_SWITCH; }
            set
            {
                if (value != this._DI_UNLOAD_SWITCH)
                {
                    _DI_UNLOAD_SWITCH = value;
                    NotifyPropertyChanged("DI_UNLOAD_SWITCH");
                }
            }
        }
        private IOPortDescripter<bool> _DI_COVER = new IOPortDescripter<bool>("DI_COVER", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_COVER
        {
            get { return _DI_COVER; }
            set
            {
                if (value != this._DI_COVER)
                {
                    _DI_COVER = value;
                    NotifyPropertyChanged("DI_COVER");
                }
            }
        }
        private IOPortDescripter<bool> _DI_COVER_DOOR_OPEN = new IOPortDescripter<bool>("DI_COVER_DOOR_OPEN", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_COVER_DOOR_OPEN
        {
            get { return _DI_COVER_DOOR_OPEN; }
            set
            {
                if (value != this._DI_COVER_DOOR_OPEN)
                {
                    _DI_COVER_DOOR_OPEN = value;
                    NotifyPropertyChanged("DI_COVER_DOOR_OPEN");
                }
            }
        }
        private IOPortDescripter<bool> _DI_COVER_DOOR_CLOSE = new IOPortDescripter<bool>("DI_COVER_DOOR_CLOSE", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_COVER_DOOR_CLOSE
        {
            get { return _DI_COVER_DOOR_CLOSE; }
            set
            {
                if (value != this._DI_COVER_DOOR_CLOSE)
                {
                    _DI_COVER_DOOR_CLOSE = value;
                    NotifyPropertyChanged("DI_COVER_DOOR_CLOSE");
                }
            }
        }
        private IOPortDescripter<bool> _DI_FO_VAC = new IOPortDescripter<bool>("DI_FO_VAC", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_FO_VAC
        {
            get { return _DI_FO_VAC; }
            set
            {
                if (value != this._DI_FO_VAC)
                {
                    _DI_FO_VAC = value;
                    NotifyPropertyChanged("DI_FO_VAC");
                }
            }
        }

        private IOPortDescripter<bool> _DI_CP_IN = new IOPortDescripter<bool>("DI_CP_IN", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_CP_IN
        {
            get { return _DI_CP_IN; }
            set
            {
                if (value != this._DI_CP_IN)
                {
                    _DI_CP_IN = value;
                    NotifyPropertyChanged("DI_CP_IN");
                }
            }
        }
        private IOPortDescripter<bool> _DI_CP_OUT = new IOPortDescripter<bool>("DI_CP_OUT", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_CP_OUT
        {
            get { return _DI_CP_OUT; }
            set
            {
                if (value != this._DI_CP_OUT)
                {
                    _DI_CP_OUT = value;
                    NotifyPropertyChanged("DI_CP_OUT");
                }
            }
        }
        private IOPortDescripter<bool> _DI_CP_40_IN = new IOPortDescripter<bool>("DI_CP_40_IN", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_CP_40_IN
        {
            get { return _DI_CP_40_IN; }
            set
            {
                if (value != this._DI_CP_40_IN)
                {
                    _DI_CP_40_IN = value;
                    NotifyPropertyChanged("DI_CP_40_IN");
                }
            }
        }

        private IOPortDescripter<bool> _DI_CP_40_OUT = new IOPortDescripter<bool>("DI_CP_40_OUT", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_CP_40_OUT
        {
            get { return _DI_CP_40_OUT; }
            set
            {
                if (value != this._DI_CP_40_OUT)
                {
                    _DI_CP_40_OUT = value;
                    NotifyPropertyChanged("DI_CP_40_OUT");
                }
            }
        }
        private IOPortDescripter<bool> _DI_FO_OPEN = new IOPortDescripter<bool>("DI_FO_OPEN", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_FO_OPEN
        {
            get { return _DI_FO_OPEN; }
            set
            {
                if (value != this._DI_FO_OPEN)
                {
                    _DI_FO_OPEN = value;
                    NotifyPropertyChanged("DI_FO_OPEN");
                }
            }
        }
        private IOPortDescripter<bool> _DI_FO_CLOSE = new IOPortDescripter<bool>("DI_FO_CLOSE", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_FO_CLOSE
        {
            get { return _DI_FO_CLOSE; }
            set
            {
                if (value != this._DI_FO_CLOSE)
                {
                    _DI_FO_CLOSE = value;
                    NotifyPropertyChanged("DI_FO_CLOSE");
                }
            }
        }
        private IOPortDescripter<bool> _DI_FO_COVER_OPEN = new IOPortDescripter<bool>("DI_FO_COVER_OPEN", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_FO_COVER_OPEN
        {
            get { return _DI_FO_COVER_OPEN; }
            set
            {
                if (value != this._DI_FO_COVER_OPEN)
                {
                    _DI_FO_COVER_OPEN = value;
                    NotifyPropertyChanged("DI_FO_COVER_OPEN");
                }
            }
        }
        private IOPortDescripter<bool> _DI_FO_COVER_CLOSE = new IOPortDescripter<bool>("DI_FO_COVER_CLOSE", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_FO_COVER_CLOSE
        {
            get { return _DI_FO_COVER_CLOSE; }
            set
            {
                if (value != this._DI_FO_COVER_CLOSE)
                {
                    _DI_FO_COVER_CLOSE = value;
                    NotifyPropertyChanged("DI_FO_COVER_CLOSE");
                }
            }
        }
        private IOPortDescripter<bool> _DI_C6IN_C8IN_PRESENCE1 = new IOPortDescripter<bool>("DI_C6IN_C8IN_PRESENCE1", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_C6IN_C8IN_PRESENCE1
        {
            get { return _DI_C6IN_C8IN_PRESENCE1; }
            set
            {
                if (value != this._DI_C6IN_C8IN_PRESENCE1)
                {
                    _DI_C6IN_C8IN_PRESENCE1 = value;
                    NotifyPropertyChanged("DI_C6IN_C8IN_PRESENCE1");
                }
            }
        }

        private IOPortDescripter<bool> _DI_C6IN_C8IN_PRESENCE2 = new IOPortDescripter<bool>("DI_C6IN_C8IN_PRESENCE2", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_C6IN_C8IN_PRESENCE2
        {
            get { return _DI_C6IN_C8IN_PRESENCE2; }
            set
            {
                if (value != this._DI_C6IN_C8IN_PRESENCE2)
                {
                    _DI_C6IN_C8IN_PRESENCE2 = value;
                    NotifyPropertyChanged("DI_C6IN_C8IN_PRESENCE2");
                }
            }
        }

        private IOPortDescripter<bool> _DI_C6IN_C8IN_PRESENCE3 = new IOPortDescripter<bool>("DI_C6IN_C8IN_PRESENCE3", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_C6IN_C8IN_PRESENCE3
        {
            get { return _DI_C6IN_C8IN_PRESENCE3; }
            set
            {
                if (value != this._DI_C6IN_C8IN_PRESENCE3)
                {
                    _DI_C6IN_C8IN_PRESENCE3 = value;
                    NotifyPropertyChanged("DI_C6IN_C8IN_PRESENCE3");
                }
            }
        }

        private IOPortDescripter<bool> _DI_C6IN_PLACEMENT = new IOPortDescripter<bool>("DI_C6IN_PLACEMENT", EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_C6IN_PLACEMENT
        {
            get { return _DI_C6IN_PLACEMENT; }
            set
            {
                if (value != this._DI_C6IN_PLACEMENT)
                {
                    _DI_C6IN_PLACEMENT = value;
                    NotifyPropertyChanged("DI_C6IN_PLACEMENT");
                }
            }
        }
        private IOPortDescripter<bool> _DI_C8IN_PLACEMENT = new IOPortDescripter<bool>("DI_C8IN_PLACEMENT", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_C8IN_PLACEMENT
        {
            get { return _DI_C8IN_PLACEMENT; }
            set
            {
                if (value != this._DI_C8IN_PLACEMENT)
                {
                    _DI_C8IN_PLACEMENT = value;
                    NotifyPropertyChanged("DI_C8IN_PLACEMENT");
                }
            }
        }
        private IOPortDescripter<bool> _DI_C6IN_C8IN_NPLACEMENT = new IOPortDescripter<bool>("DI_C6IN_C8IN_NPLACEMENT", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_C6IN_C8IN_NPLACEMENT
        {
            get { return _DI_C6IN_C8IN_NPLACEMENT; }
            set
            {
                if (value != this._DI_C6IN_C8IN_NPLACEMENT)
                {
                    _DI_C6IN_C8IN_NPLACEMENT = value;
                    NotifyPropertyChanged("DI_C6IN_C8IN_NPLACEMENT");
                }
            }
        }
        private IOPortDescripter<bool> _DI_C12IN_PRESENCE1 = new IOPortDescripter<bool>("DI_C12IN_PRESENCE1", EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_C12IN_PRESENCE1
        {
            get { return _DI_C12IN_PRESENCE1; }
            set
            {
                if (value != this._DI_C12IN_PRESENCE1)
                {
                    _DI_C12IN_PRESENCE1 = value;
                    NotifyPropertyChanged("DI_C12IN_PRESENCE1");
                }
            }
        }
        private IOPortDescripter<bool> _DI_C12IN_PRESENCE2 = new IOPortDescripter<bool>("DI_C12IN_PRESENCE2", EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_C12IN_PRESENCE2
        {
            get { return _DI_C12IN_PRESENCE2; }
            set
            {
                if (value != this._DI_C12IN_PRESENCE2)
                {
                    _DI_C12IN_PRESENCE2 = value;
                    NotifyPropertyChanged("DI_C12IN_PRESENCE2");
                }
            }
        }
        private IOPortDescripter<bool> _DI_C12IN_PLACEMENT = new IOPortDescripter<bool>("DI_C12IN_PLACEMENT", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_C12IN_PLACEMENT
        {
            get { return _DI_C12IN_PLACEMENT; }
            set
            {
                if (value != this._DI_C12IN_PLACEMENT)
                {
                    _DI_C12IN_PLACEMENT = value;
                    NotifyPropertyChanged("DI_C12IN_PLACEMENT");
                }
            }
        }
        private IOPortDescripter<bool> _DI_C12IN_NPLACEMENT = new IOPortDescripter<bool>("DI_C12IN_NPLACEMENT", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_C12IN_NPLACEMENT
        {
            get { return _DI_C12IN_NPLACEMENT; }
            set
            {
                if (value != this._DI_C12IN_NPLACEMENT)
                {
                    _DI_C12IN_NPLACEMENT = value;
                    NotifyPropertyChanged("DI_C12IN_NPLACEMENT");
                }
            }
        }
        private IOPortDescripter<bool> _DI_C12IN_POSA = new IOPortDescripter<bool>("DI_C12IN_POSA", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_C12IN_POSA
        {
            get { return _DI_C12IN_POSA; }
            set
            {
                if (value != this._DI_C12IN_POSA)
                {
                    _DI_C12IN_POSA = value;
                    NotifyPropertyChanged("DI_C12IN_POSA");
                }
            }
        }
        private IOPortDescripter<bool> _DI_C12IN_POSB = new IOPortDescripter<bool>("DI_C12IN_POSB", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_C12IN_POSB
        {
            get { return _DI_C12IN_POSB; }
            set
            {
                if (value != this._DI_C12IN_POSB)
                {
                    _DI_C12IN_POSB = value;
                    NotifyPropertyChanged("DI_C12IN_POSB");
                }
            }
        }
        private IOPortDescripter<bool> _DI_SPARE7 = new IOPortDescripter<bool>("DI_SPARE7", EnumIOType.INPUT);

        public IOPortDescripter<bool> DI_SPARE7
        {
            get { return _DI_SPARE7; }
            set
            {
                if (value != this._DI_SPARE7)
                {
                    _DI_SPARE7 = value;
                    NotifyPropertyChanged("DI_SPARE7");
                }
            }
        }

        private IOPortDescripter<bool> _DI_SPARE6 = new IOPortDescripter<bool>("DI_SPARE6", EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_SPARE6
        {
            get { return _DI_SPARE6; }
            set
            {
                if (value != this._DI_SPARE6)
                {
                    _DI_SPARE6 = value;
                    NotifyPropertyChanged("DI_SPARE6");
                }
            }
        }
        private IOPortDescripter<bool> _DI_SPARE5 = new IOPortDescripter<bool>("DI_SPARE5", EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_SPARE5
        {
            get { return _DI_SPARE5; }
            set
            {
                if (value != this._DI_SPARE5)
                {
                    _DI_SPARE5 = value;
                    NotifyPropertyChanged("DI_SPARE5");
                }
            }
        }
        private IOPortDescripter<bool> _DI_SPARE4 = new IOPortDescripter<bool>("DI_SPARE4", EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_SPARE4
        {
            get { return _DI_SPARE4; }
            set
            {
                if (value != this._DI_SPARE4)
                {
                    _DI_SPARE4 = value;
                    NotifyPropertyChanged("DI_SPARE4");
                }
            }
        }

        private IOPortDescripter<bool> _DI_SPARE3 = new IOPortDescripter<bool>("DI_SPARE3", EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_SPARE3
        {
            get { return _DI_SPARE3; }
            set
            {
                if (value != this._DI_SPARE3)
                {
                    _DI_SPARE3 = value;
                    NotifyPropertyChanged("DI_SPARE3");
                }
            }
        }
        private IOPortDescripter<bool> _DI_SPARE2 = new IOPortDescripter<bool>("DI_SPARE2", EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_SPARE2
        {
            get { return _DI_SPARE2; }
            set
            {
                if (value != this._DI_SPARE2)
                {
                    _DI_SPARE2 = value;
                    NotifyPropertyChanged("DI_SPARE2");
                }
            }
        }
        private IOPortDescripter<bool> _DI_CP_ROT_IN = new IOPortDescripter<bool>("DI_CP_ROT_IN", EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CP_ROT_IN
        {
            get { return _DI_CP_ROT_IN; }
            set
            {
                if (value != this._DI_CP_ROT_IN)
                {
                    _DI_CP_ROT_IN = value;
                    NotifyPropertyChanged("DI_CP_ROT_IN");
                }
            }
        }
        private IOPortDescripter<bool> _DI_CP_ROT_OUT = new IOPortDescripter<bool>("DI_CP_ROT_OUT", EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CP_ROT_OUT
        {
            get { return _DI_CP_ROT_OUT; }
            set
            {
                if (value != this._DI_CP_ROT_OUT)
                {
                    _DI_CP_ROT_OUT = value;
                    NotifyPropertyChanged("DI_CP_ROT_OUT");
                }
            }
        }

        private IOPortDescripter<bool> _DI_FOUP_COVER_LOCK = new IOPortDescripter<bool>("DI_FOUP_COVER_LOCK", EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DI_FOUP_COVER_LOCK
        {
            get { return _DI_FOUP_COVER_LOCK; }
            set
            {
                if (value != this._DI_FOUP_COVER_LOCK)
                {
                    _DI_FOUP_COVER_LOCK = value;
                    NotifyPropertyChanged("DI_FOUP_COVER_LOCK");
                }
            }
        }
        private IOPortDescripter<bool> _DI_FOUP_COVER_UNLOCK = new IOPortDescripter<bool>("DI_FOUP_COVER_UNLOCK", EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DI_FOUP_COVER_UNLOCK
        {
            get { return _DI_FOUP_COVER_UNLOCK; }
            set
            {
                if (value != this._DI_FOUP_COVER_UNLOCK)
                {
                    _DI_FOUP_COVER_UNLOCK = value;
                    NotifyPropertyChanged("DI_FOUP_COVER_UNLOCK");
                }
            }
        }
        private IOPortDescripter<bool> _DI_CSTT_UP = new IOPortDescripter<bool>("DI_CSTT_UP", EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DI_CSTT_UP
        {
            get { return _DI_CSTT_UP; }
            set
            {
                if (value != this._DI_CSTT_UP)
                {
                    _DI_CSTT_UP = value;
                    NotifyPropertyChanged("DI_CSTT_UP");
                }
            }
        }
        private IOPortDescripter<bool> _DI_CSTT_DOWN = new IOPortDescripter<bool>("DI_CSTT_DOWN", EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DI_CSTT_DOWN
        {
            get { return _DI_CSTT_DOWN; }
            set
            {
                if (value != this._DI_CSTT_DOWN)
                {
                    _DI_CSTT_DOWN = value;
                    NotifyPropertyChanged("DI_CSTT_DOWN");
                }
            }
        }
        #endregion
        #region // Remote FOUP Inputs
        private List<IOPortDescripter<bool>> _DI_WAFER_OUTs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_WAFER_OUTs
        {
            get { return _DI_WAFER_OUTs; }
            set
            {
                if (value != _DI_WAFER_OUTs)
                {
                    _DI_WAFER_OUTs = value;
                    NotifyPropertyChanged("DI_WAFER_OUTs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_CST12_PRESs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_CST12_PRESs
        {
            get { return _DI_CST12_PRESs; }
            set
            {
                if (value != _DI_CST12_PRESs)
                {
                    _DI_CST12_PRESs = value;
                    NotifyPropertyChanged("DI_CST12_PRESs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_CST12_PRES2s = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_CST12_PRES2s
        {
            get { return _DI_CST12_PRES2s; }
            set
            {
                if (value != _DI_CST12_PRES2s)
                {
                    _DI_CST12_PRES2s = value;
                    NotifyPropertyChanged("DI_CST12_PRES2s");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_CST08_PRESs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_CST08_PRESs
        {
            get { return _DI_CST08_PRESs; }
            set
            {
                if (value != _DI_CST08_PRESs)
                {
                    _DI_CST08_PRESs = value;
                    NotifyPropertyChanged("DI_CST08_PRESs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_CST08_PRES_2s = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_CST08_PRES_2s
        {
            get { return _DI_CST08_PRES_2s; }
            set
            {
                if (value != _DI_CST08_PRES_2s)
                {
                    _DI_CST08_PRES_2s = value;
                    NotifyPropertyChanged("DI_CST08_PRES_2s");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_CST_LOCK12s = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_CST_LOCK12s
        {
            get { return _DI_CST_LOCK12s; }
            set
            {
                if (value != _DI_CST_LOCK12s)
                {
                    _DI_CST_LOCK12s = value;
                    NotifyPropertyChanged("DI_CST_LOCK12s");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_CST_UNLOCK12s = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_CST_UNLOCK12s
        {
            get { return _DI_CST_UNLOCK12s; }
            set
            {
                if (value != _DI_CST_UNLOCK12s)
                {
                    _DI_CST_UNLOCK12s = value;
                    NotifyPropertyChanged("DI_CST_UNLOCK12s");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_DP_INs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_DP_INs
        {
            get { return _DI_DP_INs; }
            set
            {
                if (value != _DI_DP_INs)
                {
                    _DI_DP_INs = value;
                    NotifyPropertyChanged("DI_DP_INs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_DP_OUTs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_DP_OUTs
        {
            get { return _DI_DP_OUTs; }
            set
            {
                if (value != _DI_DP_OUTs)
                {
                    _DI_DP_OUTs = value;
                    NotifyPropertyChanged("DI_DP_OUTs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_COVER_UPs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_COVER_UPs
        {
            get { return _DI_COVER_UPs; }
            set
            {
                if (value != _DI_COVER_UPs)
                {
                    _DI_COVER_UPs = value;
                    NotifyPropertyChanged("DI_COVER_UPs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_COVER_DOWNs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_COVER_DOWNs
        {
            get { return _DI_COVER_DOWNs; }
            set
            {
                if (value != _DI_COVER_DOWNs)
                {
                    _DI_COVER_DOWNs = value;
                    NotifyPropertyChanged("DI_COVER_DOWNs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_COVER_OPENs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_COVER_OPENs
        {
            get { return _DI_COVER_OPENs; }
            set
            {
                if (value != _DI_COVER_OPENs)
                {
                    _DI_COVER_OPENs = value;
                    NotifyPropertyChanged("DI_COVER_OPENs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_COVER_CLOSEs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_COVER_CLOSEs
        {
            get { return _DI_COVER_CLOSEs; }
            set
            {
                if (value != _DI_COVER_CLOSEs)
                {
                    _DI_COVER_CLOSEs = value;
                    NotifyPropertyChanged("DI_COVER_CLOSEs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_COVER_LOCKs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_COVER_LOCKs
        {
            get { return _DI_COVER_LOCKs; }
            set
            {
                if (value != _DI_COVER_LOCKs)
                {
                    _DI_COVER_LOCKs = value;
                    NotifyPropertyChanged("DI_COVER_LOCKs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_COVER_UNLOCKs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_COVER_UNLOCKs
        {
            get { return _DI_COVER_UNLOCKs; }
            set
            {
                if (value != _DI_COVER_UNLOCKs)
                {
                    _DI_COVER_UNLOCKs = value;
                    NotifyPropertyChanged("DI_COVER_UNLOCKs");
                }
            }
        }

        private List<IOPortDescripter<bool>> _DI_FOUP_LOAD_BUTTONs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_FOUP_LOAD_BUTTONs
        {
            get { return _DI_FOUP_LOAD_BUTTONs; }
            set
            {
                if (value != _DI_FOUP_LOAD_BUTTONs)
                {
                    _DI_FOUP_LOAD_BUTTONs = value;
                    NotifyPropertyChanged("DI_FOUP_LOAD_BUTTONs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_FOUP_UNLOAD_BUTTONs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_FOUP_UNLOAD_BUTTONs
        {
            get { return _DI_FOUP_UNLOAD_BUTTONs; }
            set
            {
                if (value != _DI_FOUP_UNLOAD_BUTTONs)
                {
                    _DI_FOUP_UNLOAD_BUTTONs = value;
                    NotifyPropertyChanged("DI_FOUP_UNLOAD_BUTTONs");
                }
            }
        }

        private List<IOPortDescripter<bool>> _DI_CST_Exists = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_CST_Exists
        {
            get { return _DI_CST_Exists; }
            set
            {
                if (value != _DI_CST_Exists)
                {
                    _DI_CST_Exists = value;
                    NotifyPropertyChanged("DI_CST_Exists");
                }
            }
        }

        private List<IOPortDescripter<bool>> _DI_CST_MappingOuts = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_CST_MappingOuts
        {
            get { return _DI_CST_MappingOuts; }
            set
            {
                if (value != _DI_CST_MappingOuts)
                {
                    _DI_CST_MappingOuts = value;
                    NotifyPropertyChanged("DI_CST_MappingOuts");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI_CST_CoverVacuums = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_CST_CoverVacuums
        {
            get { return _DI_CST_CoverVacuums; }
            set
            {
                if (value != _DI_CST_CoverVacuums)
                {
                    _DI_CST_CoverVacuums = value;
                    NotifyPropertyChanged("DI_CST_CoverVacuums");
                }
            }
        }

        private List<IOPortDescripter<bool>> _DI_PAD_As = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_PAD_As
        {
            get { return _DI_PAD_As; }
            set
            {
                if (value != _DI_PAD_As)
                {
                    _DI_PAD_As = value;
                    NotifyPropertyChanged("DI_PAD_As");
                }
            }
        }

        private List<IOPortDescripter<bool>> _DI_PAD_Bs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DI_PAD_Bs
        {
            get { return _DI_PAD_Bs; }
            set
            {
                if (value != _DI_PAD_Bs)
                {
                    _DI_PAD_Bs = value;
                    NotifyPropertyChanged("DI_PAD_Bs");
                }
            }
        }
        #endregion

        public void SetDefaultParam()
        {
            if(SystemManager.SysteMode == SystemModeEnum.Multiple)
            {
                RemoteFoupDefaultParam();
            }
            else
            {
                FoupBSCIDefaultParam();
            }
        }
        public void FoupNormalDefaultParam()
        {
            try
            {
            #region // Foup Normal 12Inch FOUP IO
            #region // Foup Setdefault Inputs

            DI_WAFER_OUT.ChannelIndex.Value = 9;
            DI_WAFER_OUT.PortIndex.Value = 7;
            DI_WAFER_OUT.Reverse.Value = false;
            DI_WAFER_OUT.IOOveride.Value = EnumIOOverride.NONE;
            DI_WAFER_OUT.MaintainTime.Value = 50;
            DI_WAFER_OUT.TimeOut.Value = 5000;

            DI_FO_UP.ChannelIndex.Value = 9;
            DI_FO_UP.PortIndex.Value = 6;
            DI_FO_UP.Reverse.Value = true;
            DI_FO_UP.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_UP.MaintainTime.Value = 50;
            DI_FO_UP.TimeOut.Value = 5000;

            DI_FO_DOWN.ChannelIndex.Value = 9;
            DI_FO_DOWN.PortIndex.Value = 5;
            DI_FO_DOWN.Reverse.Value = true;
            DI_FO_DOWN.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_DOWN.MaintainTime.Value = 50;
            DI_FO_DOWN.TimeOut.Value = 5000;

            DI_LOAD_SWITCH.ChannelIndex.Value = 9;
            DI_LOAD_SWITCH.PortIndex.Value = 4;
            DI_LOAD_SWITCH.Reverse.Value = true;
            DI_LOAD_SWITCH.IOOveride.Value = EnumIOOverride.NONE;
            DI_LOAD_SWITCH.MaintainTime.Value = 50;
            DI_LOAD_SWITCH.TimeOut.Value = 5000;

            DI_UNLOAD_SWITCH.ChannelIndex.Value = 9;
            DI_UNLOAD_SWITCH.PortIndex.Value = 3;
            DI_UNLOAD_SWITCH.Reverse.Value = true;
            DI_UNLOAD_SWITCH.IOOveride.Value = EnumIOOverride.NONE;
            DI_UNLOAD_SWITCH.MaintainTime.Value = 50;
            DI_UNLOAD_SWITCH.TimeOut.Value = 5000;

            DI_COVER_DOOR_OPEN.ChannelIndex.Value = 9;
            DI_COVER_DOOR_OPEN.PortIndex.Value = 2;
            DI_COVER_DOOR_OPEN.Reverse.Value = true;
            DI_COVER_DOOR_OPEN.IOOveride.Value = EnumIOOverride.NONE;
            DI_COVER_DOOR_OPEN.MaintainTime.Value = 50;
            DI_COVER_DOOR_OPEN.TimeOut.Value = 5000;

            DI_COVER_DOOR_CLOSE.ChannelIndex.Value = 9;
            DI_COVER_DOOR_CLOSE.PortIndex.Value = 1;
            DI_COVER_DOOR_CLOSE.Reverse.Value = false;
            DI_COVER_DOOR_CLOSE.IOOveride.Value = EnumIOOverride.NONE;
            DI_COVER_DOOR_CLOSE.MaintainTime.Value = 50;
            DI_COVER_DOOR_CLOSE.TimeOut.Value = 5000;

            DI_COVER.ChannelIndex.Value = 10;
            DI_COVER.PortIndex.Value = 0;
            DI_COVER.Reverse.Value = true;
            DI_COVER.IOOveride.Value = EnumIOOverride.NONE;
            DI_COVER.MaintainTime.Value = 50;
            DI_COVER.TimeOut.Value = 5000;

            DI_FO_VAC.ChannelIndex.Value = 9;
            DI_FO_VAC.PortIndex.Value = 0;
            DI_FO_VAC.Reverse.Value = true;
            DI_FO_VAC.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_VAC.MaintainTime.Value = 50;
            DI_FO_VAC.TimeOut.Value = 5000;

            DI_CP_IN.ChannelIndex.Value = 10;
            DI_CP_IN.PortIndex.Value = 6;
            DI_CP_IN.Reverse.Value = true;
            DI_CP_IN.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_IN.MaintainTime.Value = 50;
            DI_CP_IN.TimeOut.Value = 5000;

            DI_CP_OUT.ChannelIndex.Value = 10;
            DI_CP_OUT.PortIndex.Value = 5;
            DI_CP_OUT.Reverse.Value = true;
            DI_CP_OUT.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_OUT.MaintainTime.Value = 50;
            DI_CP_OUT.TimeOut.Value = 5000;

            DI_CP_40_IN.ChannelIndex.Value = 10;
            DI_CP_40_IN.PortIndex.Value = 3;
            DI_CP_40_IN.Reverse.Value = true;
            DI_CP_40_IN.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_40_IN.MaintainTime.Value = 50;
            DI_CP_40_IN.TimeOut.Value = 5000;

            DI_CP_40_OUT.ChannelIndex.Value = 10;
            DI_CP_40_OUT.PortIndex.Value = 4;
            DI_CP_40_OUT.Reverse.Value = true;
            DI_CP_40_OUT.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_40_OUT.MaintainTime.Value = 50;
            DI_CP_40_OUT.TimeOut.Value = 5000;

            DI_FO_OPEN.ChannelIndex.Value = 10;
            DI_FO_OPEN.PortIndex.Value = 2;
            DI_FO_OPEN.Reverse.Value = true;
            DI_FO_OPEN.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_OPEN.MaintainTime.Value = 50;
            DI_FO_OPEN.TimeOut.Value = 5000;

            DI_FO_CLOSE.ChannelIndex.Value = 10;
            DI_FO_CLOSE.PortIndex.Value = 1;
            DI_FO_CLOSE.Reverse.Value = true;
            DI_FO_CLOSE.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_CLOSE.MaintainTime.Value = 50;
            DI_FO_CLOSE.TimeOut.Value = 5000;

            DI_FO_COVER_OPEN.ChannelIndex.Value = 10;
            DI_FO_COVER_OPEN.PortIndex.Value = 0;
            DI_FO_COVER_OPEN.Reverse.Value = false;
            DI_FO_COVER_OPEN.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_COVER_OPEN.MaintainTime.Value = 50;
            DI_FO_COVER_OPEN.TimeOut.Value = 5000;

            DI_FO_COVER_CLOSE.ChannelIndex.Value = 10;
            DI_FO_COVER_CLOSE.PortIndex.Value = 0;
            DI_FO_COVER_CLOSE.Reverse.Value = true;
            DI_FO_COVER_CLOSE.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_COVER_CLOSE.MaintainTime.Value = 50;
            DI_FO_COVER_CLOSE.TimeOut.Value = 5000;

            DI_C6IN_C8IN_PRESENCE1.ChannelIndex.Value = 11;
            DI_C6IN_C8IN_PRESENCE1.PortIndex.Value = 7;
            DI_C6IN_C8IN_PRESENCE1.Reverse.Value = true;
            DI_C6IN_C8IN_PRESENCE1.IOOveride.Value = EnumIOOverride.NONE;
            DI_C6IN_C8IN_PRESENCE1.MaintainTime.Value = 50;
            DI_C6IN_C8IN_PRESENCE1.TimeOut.Value = 5000;

            DI_C6IN_C8IN_PRESENCE2.ChannelIndex.Value = 0;
            DI_C6IN_C8IN_PRESENCE2.PortIndex.Value = 0;
            DI_C6IN_C8IN_PRESENCE2.Reverse.Value = false;
            DI_C6IN_C8IN_PRESENCE2.IOOveride.Value = EnumIOOverride.NONE;
            DI_C6IN_C8IN_PRESENCE2.MaintainTime.Value = 50;
            DI_C6IN_C8IN_PRESENCE2.TimeOut.Value = 5000;

            DI_C6IN_PLACEMENT.ChannelIndex.Value = 11;
            DI_C6IN_PLACEMENT.PortIndex.Value = 6;
            DI_C6IN_PLACEMENT.Reverse.Value = true;
            DI_C6IN_PLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C6IN_PLACEMENT.MaintainTime.Value = 50;
            DI_C6IN_PLACEMENT.TimeOut.Value = 5000;

            DI_C8IN_PLACEMENT.ChannelIndex.Value = 11;
            DI_C8IN_PLACEMENT.PortIndex.Value = 5;
            DI_C8IN_PLACEMENT.Reverse.Value = true;
            DI_C8IN_PLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C8IN_PLACEMENT.MaintainTime.Value = 50;
            DI_C8IN_PLACEMENT.TimeOut.Value = 5000;

            DI_C6IN_C8IN_NPLACEMENT.ChannelIndex.Value = 11;
            DI_C6IN_C8IN_NPLACEMENT.PortIndex.Value = 4;
            DI_C6IN_C8IN_NPLACEMENT.Reverse.Value = false;
            DI_C6IN_C8IN_NPLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C6IN_C8IN_NPLACEMENT.MaintainTime.Value = 50;
            DI_C6IN_C8IN_NPLACEMENT.TimeOut.Value = 5000;

            DI_C12IN_PRESENCE1.ChannelIndex.Value = 11;
            DI_C12IN_PRESENCE1.PortIndex.Value = 3;
            DI_C12IN_PRESENCE1.Reverse.Value = true;
            DI_C12IN_PRESENCE1.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_PRESENCE1.MaintainTime.Value = 50;
            DI_C12IN_PRESENCE1.TimeOut.Value = 5000;

            DI_C12IN_PRESENCE2.ChannelIndex.Value = 0;
            DI_C12IN_PRESENCE2.PortIndex.Value = 0;
            DI_C12IN_PRESENCE2.Reverse.Value = false;
            DI_C12IN_PRESENCE2.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_PRESENCE2.MaintainTime.Value = 50;
            DI_C12IN_PRESENCE2.TimeOut.Value = 5000;

            DI_C12IN_PLACEMENT.ChannelIndex.Value = 11;
            DI_C12IN_PLACEMENT.PortIndex.Value = 2;
            DI_C12IN_PLACEMENT.Reverse.Value = true;
            DI_C12IN_PLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_PLACEMENT.MaintainTime.Value = 50;
            DI_C12IN_PLACEMENT.TimeOut.Value = 5000;

            DI_C12IN_NPLACEMENT.ChannelIndex.Value = 11;
            DI_C12IN_NPLACEMENT.PortIndex.Value = 1;
            DI_C12IN_NPLACEMENT.Reverse.Value = true;
            DI_C12IN_NPLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_NPLACEMENT.MaintainTime.Value = 50;
            DI_C12IN_NPLACEMENT.TimeOut.Value = 5000;

            DI_C12IN_POSA.ChannelIndex.Value = 11;
            DI_C12IN_POSA.PortIndex.Value = 0;
            DI_C12IN_POSA.Reverse.Value = false;
            DI_C12IN_POSA.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_POSA.MaintainTime.Value = 50;
            DI_C12IN_POSA.TimeOut.Value = 5000;

            DI_C12IN_POSB.ChannelIndex.Value = 10;
            DI_C12IN_POSB.PortIndex.Value = 7;
            DI_C12IN_POSB.Reverse.Value = false;
            DI_C12IN_POSB.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_POSB.MaintainTime.Value = 50;
            DI_C12IN_POSB.TimeOut.Value = 5000;

            DI_SPARE7.ChannelIndex.Value = 12;
            DI_SPARE7.PortIndex.Value = 0;
            DI_SPARE7.Reverse.Value = false;
            DI_SPARE7.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE7.MaintainTime.Value = 50;
            DI_SPARE7.TimeOut.Value = 5000;

            DI_SPARE6.ChannelIndex.Value = 12;
            DI_SPARE6.PortIndex.Value = 1;
            DI_SPARE6.Reverse.Value = false;
            DI_SPARE6.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE6.MaintainTime.Value = 50;
            DI_SPARE6.TimeOut.Value = 5000;

            DI_SPARE5.ChannelIndex.Value = 12;
            DI_SPARE5.PortIndex.Value = 2;
            DI_SPARE5.Reverse.Value = false;
            DI_SPARE5.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE5.MaintainTime.Value = 50;
            DI_SPARE5.TimeOut.Value = 5000;

            DI_SPARE4.ChannelIndex.Value = 12;
            DI_SPARE4.PortIndex.Value = 3;
            DI_SPARE4.Reverse.Value = false;
            DI_SPARE4.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE4.MaintainTime.Value = 50;
            DI_SPARE4.TimeOut.Value = 5000;

            DI_SPARE3.ChannelIndex.Value = 12;
            DI_SPARE3.PortIndex.Value = 4;
            DI_SPARE3.Reverse.Value = false;
            DI_SPARE3.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE3.MaintainTime.Value = 50;
            DI_SPARE3.TimeOut.Value = 5000;

            DI_SPARE2.ChannelIndex.Value = 0;
            DI_SPARE2.PortIndex.Value = 0;
            DI_SPARE2.Reverse.Value = false;
            DI_SPARE2.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE2.MaintainTime.Value = 50;
            DI_SPARE2.TimeOut.Value = 5000;

            DI_CP_ROT_IN.ChannelIndex.Value = 0;
            DI_CP_ROT_IN.PortIndex.Value = 0;
            DI_CP_ROT_IN.Reverse.Value = false;
            DI_CP_ROT_IN.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_ROT_IN.MaintainTime.Value = 50;
            DI_CP_ROT_IN.TimeOut.Value = 5000;

            DI_CP_ROT_OUT.ChannelIndex.Value = 0;
            DI_CP_ROT_OUT.PortIndex.Value = 0;
            DI_CP_ROT_OUT.Reverse.Value = false;
            DI_CP_ROT_OUT.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_ROT_OUT.MaintainTime.Value = 50;
            DI_CP_ROT_OUT.TimeOut.Value = 5000;

            DI_FOUP_COVER_LOCK.ChannelIndex.Value = 0;
            DI_FOUP_COVER_LOCK.PortIndex.Value = 0;
            DI_FOUP_COVER_LOCK.Reverse.Value = false;
            DI_FOUP_COVER_LOCK.IOOveride.Value = EnumIOOverride.NONE;
            DI_FOUP_COVER_LOCK.MaintainTime.Value = 50;
            DI_FOUP_COVER_LOCK.TimeOut.Value = 5000;

            DI_FOUP_COVER_UNLOCK.ChannelIndex.Value = 0;
            DI_FOUP_COVER_UNLOCK.PortIndex.Value = 0;
            DI_FOUP_COVER_UNLOCK.Reverse.Value = false;
            DI_FOUP_COVER_UNLOCK.IOOveride.Value = EnumIOOverride.NONE;
            DI_FOUP_COVER_UNLOCK.MaintainTime.Value = 50;
            DI_FOUP_COVER_UNLOCK.TimeOut.Value = 5000;

            //Cassette Tilting 
            DI_CSTT_DOWN.ChannelIndex.Value = 0;
            DI_CSTT_DOWN.PortIndex.Value = 0;
            DI_CSTT_DOWN.Reverse.Value = true;
            DI_CSTT_DOWN.IOOveride.Value = EnumIOOverride.NONE;
            DI_CSTT_DOWN.MaintainTime.Value = 50;
            DI_CSTT_DOWN.TimeOut.Value = 5000;

            DI_CSTT_UP.ChannelIndex.Value = 0;
            DI_CSTT_UP.PortIndex.Value = 0;
            DI_CSTT_UP.Reverse.Value = false;
            DI_CSTT_UP.IOOveride.Value = EnumIOOverride.NONE;
            DI_CSTT_UP.MaintainTime.Value = 50;
            DI_CSTT_UP.TimeOut.Value = 5000;
            #endregion
            #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public void FoupBSCIDefaultParam()
        {
            try
            {
            #region // BSCI장비 Foup 8Inch IO
            #region // Foup Setdefault Inputs
            DI_WAFER_OUT.ChannelIndex.Value = 4;
            DI_WAFER_OUT.PortIndex.Value = 1;
            DI_WAFER_OUT.Reverse.Value = false;
            DI_WAFER_OUT.IOOveride.Value = EnumIOOverride.NONE;
            DI_WAFER_OUT.MaintainTime.Value = 50;
            DI_WAFER_OUT.TimeOut.Value = 5000;

            DI_FO_UP.ChannelIndex.Value = -1;
            DI_FO_UP.PortIndex.Value = -1;
            DI_FO_UP.Reverse.Value = true;
            DI_FO_UP.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_UP.MaintainTime.Value = 50;
            DI_FO_UP.TimeOut.Value = 5000;

            DI_FO_DOWN.ChannelIndex.Value = -1;
            DI_FO_DOWN.PortIndex.Value = -1;
            DI_FO_DOWN.Reverse.Value = true;
            DI_FO_DOWN.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_DOWN.MaintainTime.Value = 50;
            DI_FO_DOWN.TimeOut.Value = 5000;

            DI_LOAD_SWITCH.ChannelIndex.Value = 3;
            DI_LOAD_SWITCH.PortIndex.Value = 10;
            DI_LOAD_SWITCH.Reverse.Value = true;
            DI_LOAD_SWITCH.IOOveride.Value = EnumIOOverride.NONE;
            DI_LOAD_SWITCH.MaintainTime.Value = 50;
            DI_LOAD_SWITCH.TimeOut.Value = 5000;

            DI_UNLOAD_SWITCH.ChannelIndex.Value = 3;
            DI_UNLOAD_SWITCH.PortIndex.Value = 11;
            DI_UNLOAD_SWITCH.Reverse.Value = true;
            DI_UNLOAD_SWITCH.IOOveride.Value = EnumIOOverride.NONE;
            DI_UNLOAD_SWITCH.MaintainTime.Value = 50;
            DI_UNLOAD_SWITCH.TimeOut.Value = 5000;

            DI_COVER_DOOR_OPEN.ChannelIndex.Value = -1;
            DI_COVER_DOOR_OPEN.PortIndex.Value = -1;
            DI_COVER_DOOR_OPEN.Reverse.Value = true;
            DI_COVER_DOOR_OPEN.IOOveride.Value = EnumIOOverride.NONE;
            DI_COVER_DOOR_OPEN.MaintainTime.Value = 50;
            DI_COVER_DOOR_OPEN.TimeOut.Value = 5000;

            DI_COVER_DOOR_CLOSE.ChannelIndex.Value = -1;
            DI_COVER_DOOR_CLOSE.PortIndex.Value = -1;
            DI_COVER_DOOR_CLOSE.Reverse.Value = false;
            DI_COVER_DOOR_CLOSE.IOOveride.Value = EnumIOOverride.NONE;
            DI_COVER_DOOR_CLOSE.MaintainTime.Value = 50;
            DI_COVER_DOOR_CLOSE.TimeOut.Value = 5000;

            DI_COVER.ChannelIndex.Value = -1;
            DI_COVER.PortIndex.Value = -1;
            DI_COVER.Reverse.Value = true;
            DI_COVER.IOOveride.Value = EnumIOOverride.NONE;
            DI_COVER.MaintainTime.Value = 50;
            DI_COVER.TimeOut.Value = 5000;

            DI_FO_VAC.ChannelIndex.Value = -1;
            DI_FO_VAC.PortIndex.Value = -1;
            DI_FO_VAC.Reverse.Value = true;
            DI_FO_VAC.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_VAC.MaintainTime.Value = 50;
            DI_FO_VAC.TimeOut.Value = 5000;

            DI_CP_IN.ChannelIndex.Value = -1;
            DI_CP_IN.PortIndex.Value = -1;
            DI_CP_IN.Reverse.Value = true;
            DI_CP_IN.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_IN.MaintainTime.Value = 50;
            DI_CP_IN.TimeOut.Value = 5000;

            DI_CP_OUT.ChannelIndex.Value = -1;
            DI_CP_OUT.PortIndex.Value = -1;
            DI_CP_OUT.Reverse.Value = true;
            DI_CP_OUT.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_OUT.MaintainTime.Value = 50;
            DI_CP_OUT.TimeOut.Value = 5000;

            DI_CP_40_IN.ChannelIndex.Value = -1;
            DI_CP_40_IN.PortIndex.Value = -1;
            DI_CP_40_IN.Reverse.Value = true;
            DI_CP_40_IN.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_40_IN.MaintainTime.Value = 50;
            DI_CP_40_IN.TimeOut.Value = 5000;

            DI_CP_40_OUT.ChannelIndex.Value = -1;
            DI_CP_40_OUT.PortIndex.Value = -1;
            DI_CP_40_OUT.Reverse.Value = true;
            DI_CP_40_OUT.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_40_OUT.MaintainTime.Value = 50;
            DI_CP_40_OUT.TimeOut.Value = 5000;

            DI_FO_OPEN.ChannelIndex.Value = -1;
            DI_FO_OPEN.PortIndex.Value = -1;
            DI_FO_OPEN.Reverse.Value = true;
            DI_FO_OPEN.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_OPEN.MaintainTime.Value = 50;
            DI_FO_OPEN.TimeOut.Value = 5000;

            DI_FO_CLOSE.ChannelIndex.Value = -1;
            DI_FO_CLOSE.PortIndex.Value = -1;
            DI_FO_CLOSE.Reverse.Value = true;
            DI_FO_CLOSE.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_CLOSE.MaintainTime.Value = 50;
            DI_FO_CLOSE.TimeOut.Value = 5000;

            DI_FO_COVER_OPEN.ChannelIndex.Value = -1;
            DI_FO_COVER_OPEN.PortIndex.Value = -1;
            DI_FO_COVER_OPEN.Reverse.Value = false;
            DI_FO_COVER_OPEN.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_COVER_OPEN.MaintainTime.Value = 50;
            DI_FO_COVER_OPEN.TimeOut.Value = 5000;

            DI_FO_COVER_CLOSE.ChannelIndex.Value = -1;
            DI_FO_COVER_CLOSE.PortIndex.Value = -1;
            DI_FO_COVER_CLOSE.Reverse.Value = true;
            DI_FO_COVER_CLOSE.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_COVER_CLOSE.MaintainTime.Value = 50;
            DI_FO_COVER_CLOSE.TimeOut.Value = 5000;

            DI_C6IN_C8IN_PRESENCE1.ChannelIndex.Value = 4;
            DI_C6IN_C8IN_PRESENCE1.PortIndex.Value = 2;
            DI_C6IN_C8IN_PRESENCE1.Reverse.Value = false;
            DI_C6IN_C8IN_PRESENCE1.IOOveride.Value = EnumIOOverride.NONE;
            DI_C6IN_C8IN_PRESENCE1.MaintainTime.Value = 50;
            DI_C6IN_C8IN_PRESENCE1.TimeOut.Value = 5000;

            DI_C6IN_C8IN_PRESENCE2.ChannelIndex.Value = 4;
            DI_C6IN_C8IN_PRESENCE2.PortIndex.Value = 3;
            DI_C6IN_C8IN_PRESENCE2.Reverse.Value = false;
            DI_C6IN_C8IN_PRESENCE2.IOOveride.Value = EnumIOOverride.NONE;
            DI_C6IN_C8IN_PRESENCE2.MaintainTime.Value = 50;
            DI_C6IN_C8IN_PRESENCE2.TimeOut.Value = 5000;

            DI_C6IN_C8IN_PRESENCE3.ChannelIndex.Value = 4;
            DI_C6IN_C8IN_PRESENCE3.PortIndex.Value = 4;
            DI_C6IN_C8IN_PRESENCE3.Reverse.Value = false;
            DI_C6IN_C8IN_PRESENCE3.IOOveride.Value = EnumIOOverride.NONE;
            DI_C6IN_C8IN_PRESENCE3.MaintainTime.Value = 50;
            DI_C6IN_C8IN_PRESENCE3.TimeOut.Value = 5000;

            DI_C6IN_PLACEMENT.ChannelIndex.Value = 4;
            DI_C6IN_PLACEMENT.PortIndex.Value = 6;
            DI_C6IN_PLACEMENT.Reverse.Value = true;
            DI_C6IN_PLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C6IN_PLACEMENT.MaintainTime.Value = 50;
            DI_C6IN_PLACEMENT.TimeOut.Value = 10000;

            DI_C8IN_PLACEMENT.ChannelIndex.Value = 4;
            DI_C8IN_PLACEMENT.PortIndex.Value = 5;
            DI_C8IN_PLACEMENT.Reverse.Value = true;
            DI_C8IN_PLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C8IN_PLACEMENT.MaintainTime.Value = 50;
            DI_C8IN_PLACEMENT.TimeOut.Value = 10000;

            DI_C6IN_C8IN_NPLACEMENT.ChannelIndex.Value = 4;
            DI_C6IN_C8IN_NPLACEMENT.PortIndex.Value = 0;
            DI_C6IN_C8IN_NPLACEMENT.Reverse.Value = true;
            DI_C6IN_C8IN_NPLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C6IN_C8IN_NPLACEMENT.MaintainTime.Value = 50;
            DI_C6IN_C8IN_NPLACEMENT.TimeOut.Value = 10000;

            DI_C12IN_PRESENCE1.ChannelIndex.Value = -1;
            DI_C12IN_PRESENCE1.PortIndex.Value = -1;
            DI_C12IN_PRESENCE1.Reverse.Value = true;
            DI_C12IN_PRESENCE1.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_PRESENCE1.MaintainTime.Value = 50;
            DI_C12IN_PRESENCE1.TimeOut.Value = 5000;

            DI_C12IN_PRESENCE2.ChannelIndex.Value = -1;
            DI_C12IN_PRESENCE2.PortIndex.Value = -1;
            DI_C12IN_PRESENCE2.Reverse.Value = false;
            DI_C12IN_PRESENCE2.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_PRESENCE2.MaintainTime.Value = 50;
            DI_C12IN_PRESENCE2.TimeOut.Value = 5000;

            DI_C12IN_PLACEMENT.ChannelIndex.Value = -1;
            DI_C12IN_PLACEMENT.PortIndex.Value = -1;
            DI_C12IN_PLACEMENT.Reverse.Value = true;
            DI_C12IN_PLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_PLACEMENT.MaintainTime.Value = 50;
            DI_C12IN_PLACEMENT.TimeOut.Value = 5000;

            DI_C12IN_NPLACEMENT.ChannelIndex.Value = -1;
            DI_C12IN_NPLACEMENT.PortIndex.Value = -1;
            DI_C12IN_NPLACEMENT.Reverse.Value = true;
            DI_C12IN_NPLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_NPLACEMENT.MaintainTime.Value = 50;
            DI_C12IN_NPLACEMENT.TimeOut.Value = 5000;

            DI_C12IN_POSA.ChannelIndex.Value = -1;
            DI_C12IN_POSA.PortIndex.Value = -1;
            DI_C12IN_POSA.Reverse.Value = false;
            DI_C12IN_POSA.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_POSA.MaintainTime.Value = 50;
            DI_C12IN_POSA.TimeOut.Value = 5000;

            DI_C12IN_POSB.ChannelIndex.Value = -1;
            DI_C12IN_POSB.PortIndex.Value = -1;
            DI_C12IN_POSB.Reverse.Value = false;
            DI_C12IN_POSB.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_POSB.MaintainTime.Value = 50;
            DI_C12IN_POSB.TimeOut.Value = 5000;

            DI_SPARE7.ChannelIndex.Value = -1;
            DI_SPARE7.PortIndex.Value = -1;
            DI_SPARE7.Reverse.Value = false;
            DI_SPARE7.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE7.MaintainTime.Value = 50;
            DI_SPARE7.TimeOut.Value = 5000;

            DI_SPARE6.ChannelIndex.Value = -1;
            DI_SPARE6.PortIndex.Value = -1;
            DI_SPARE6.Reverse.Value = false;
            DI_SPARE6.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE6.MaintainTime.Value = 50;
            DI_SPARE6.TimeOut.Value = 5000;

            DI_SPARE5.ChannelIndex.Value = -1;
            DI_SPARE5.PortIndex.Value = -1;
            DI_SPARE5.Reverse.Value = false;
            DI_SPARE5.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE5.MaintainTime.Value = 50;
            DI_SPARE5.TimeOut.Value = 5000;

            DI_SPARE4.ChannelIndex.Value = -1;
            DI_SPARE4.PortIndex.Value = -1;
            DI_SPARE4.Reverse.Value = false;
            DI_SPARE4.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE4.MaintainTime.Value = 50;
            DI_SPARE4.TimeOut.Value = 5000;

            DI_SPARE3.ChannelIndex.Value = -1;
            DI_SPARE3.PortIndex.Value = -1;
            DI_SPARE3.Reverse.Value = false;
            DI_SPARE3.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE3.MaintainTime.Value = 50;
            DI_SPARE3.TimeOut.Value = 5000;

            DI_SPARE2.ChannelIndex.Value = -1;
            DI_SPARE2.PortIndex.Value = -1;
            DI_SPARE2.Reverse.Value = false;
            DI_SPARE2.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE2.MaintainTime.Value = 50;
            DI_SPARE2.TimeOut.Value = 5000;

            DI_CP_ROT_IN.ChannelIndex.Value = -1;
            DI_CP_ROT_IN.PortIndex.Value = -1;
            DI_CP_ROT_IN.Reverse.Value = false;
            DI_CP_ROT_IN.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_ROT_IN.MaintainTime.Value = 50;
            DI_CP_ROT_IN.TimeOut.Value = 5000;

            DI_CP_ROT_OUT.ChannelIndex.Value = -1;
            DI_CP_ROT_OUT.PortIndex.Value = -1;
            DI_CP_ROT_OUT.Reverse.Value = false;
            DI_CP_ROT_OUT.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_ROT_OUT.MaintainTime.Value = 50;
            DI_CP_ROT_OUT.TimeOut.Value = 5000;

            DI_FOUP_COVER_LOCK.ChannelIndex.Value = -1;
            DI_FOUP_COVER_LOCK.PortIndex.Value = -1;
            DI_FOUP_COVER_LOCK.Reverse.Value = false;
            DI_FOUP_COVER_LOCK.IOOveride.Value = EnumIOOverride.NONE;
            DI_FOUP_COVER_LOCK.MaintainTime.Value = 50;
            DI_FOUP_COVER_LOCK.TimeOut.Value = 5000;

            DI_FOUP_COVER_UNLOCK.ChannelIndex.Value = -1;
            DI_FOUP_COVER_UNLOCK.PortIndex.Value = -1;
            DI_FOUP_COVER_UNLOCK.Reverse.Value = false;
            DI_FOUP_COVER_UNLOCK.IOOveride.Value = EnumIOOverride.NONE;
            DI_FOUP_COVER_UNLOCK.MaintainTime.Value = 50;
            DI_FOUP_COVER_UNLOCK.TimeOut.Value = 5000;

            //Cassette Tilting 
            DI_CSTT_DOWN.ChannelIndex.Value = 4;
            DI_CSTT_DOWN.PortIndex.Value = 7;
            DI_CSTT_DOWN.Reverse.Value = false;
            DI_CSTT_DOWN.IOOveride.Value = EnumIOOverride.NONE;
            DI_CSTT_DOWN.MaintainTime.Value = 50;
            DI_CSTT_DOWN.TimeOut.Value = 5000;

            DI_CSTT_UP.ChannelIndex.Value = 4;
            DI_CSTT_UP.PortIndex.Value = 7;
            DI_CSTT_UP.Reverse.Value = true;
            DI_CSTT_UP.IOOveride.Value = EnumIOOverride.NONE;
            DI_CSTT_UP.MaintainTime.Value = 50;
            DI_CSTT_UP.TimeOut.Value = 5000;

            #endregion
            #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public void FoupTestDefaultParam()
        {
            try
            {
            #region // TEST장비 Foup 8Inch IO
            #region // Foup Setdefault Inputs
            DI_WAFER_OUT.ChannelIndex.Value = 4;
            DI_WAFER_OUT.PortIndex.Value = 1;
            DI_WAFER_OUT.Reverse.Value = false;
            DI_WAFER_OUT.IOOveride.Value = EnumIOOverride.NONE;
            DI_WAFER_OUT.MaintainTime.Value = 50;
            DI_WAFER_OUT.TimeOut.Value = 50000;

            DI_FO_UP.ChannelIndex.Value = -1;
            DI_FO_UP.PortIndex.Value = -1;
            DI_FO_UP.Reverse.Value = true;
            DI_FO_UP.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_UP.MaintainTime.Value = 50;
            DI_FO_UP.TimeOut.Value = 5000;

            DI_FO_DOWN.ChannelIndex.Value = -1;
            DI_FO_DOWN.PortIndex.Value = -1;
            DI_FO_DOWN.Reverse.Value = true;
            DI_FO_DOWN.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_DOWN.MaintainTime.Value = 50;
            DI_FO_DOWN.TimeOut.Value = 5000;

            DI_LOAD_SWITCH.ChannelIndex.Value = -1;
            DI_LOAD_SWITCH.PortIndex.Value = -1;
            DI_LOAD_SWITCH.Reverse.Value = true;
            DI_LOAD_SWITCH.IOOveride.Value = EnumIOOverride.NLO;
            DI_LOAD_SWITCH.MaintainTime.Value = 50;
            DI_LOAD_SWITCH.TimeOut.Value = 5000;

            DI_UNLOAD_SWITCH.ChannelIndex.Value = -1;
            DI_UNLOAD_SWITCH.PortIndex.Value = -1;
            DI_UNLOAD_SWITCH.Reverse.Value = true;
            DI_UNLOAD_SWITCH.IOOveride.Value = EnumIOOverride.NLO;
            DI_UNLOAD_SWITCH.MaintainTime.Value = 50;
            DI_UNLOAD_SWITCH.TimeOut.Value = 5000;

            DI_COVER_DOOR_OPEN.ChannelIndex.Value = -1;
            DI_COVER_DOOR_OPEN.PortIndex.Value = -1;
            DI_COVER_DOOR_OPEN.Reverse.Value = true;
            DI_COVER_DOOR_OPEN.IOOveride.Value = EnumIOOverride.NONE;
            DI_COVER_DOOR_OPEN.MaintainTime.Value = 50;
            DI_COVER_DOOR_OPEN.TimeOut.Value = 5000;

            DI_COVER_DOOR_CLOSE.ChannelIndex.Value = -1;
            DI_COVER_DOOR_CLOSE.PortIndex.Value = -1;
            DI_COVER_DOOR_CLOSE.Reverse.Value = false;
            DI_COVER_DOOR_CLOSE.IOOveride.Value = EnumIOOverride.NONE;
            DI_COVER_DOOR_CLOSE.MaintainTime.Value = 50;
            DI_COVER_DOOR_CLOSE.TimeOut.Value = 5000;

            DI_COVER.ChannelIndex.Value = -1;
            DI_COVER.PortIndex.Value = -1;
            DI_COVER.Reverse.Value = true;
            DI_COVER.IOOveride.Value = EnumIOOverride.NONE;
            DI_COVER.MaintainTime.Value = 50;
            DI_COVER.TimeOut.Value = 5000;

            DI_FO_VAC.ChannelIndex.Value = -1;
            DI_FO_VAC.PortIndex.Value = -1;
            DI_FO_VAC.Reverse.Value = true;
            DI_FO_VAC.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_VAC.MaintainTime.Value = 50;
            DI_FO_VAC.TimeOut.Value = 5000;

            DI_CP_IN.ChannelIndex.Value = -1;
            DI_CP_IN.PortIndex.Value = -1;
            DI_CP_IN.Reverse.Value = true;
            DI_CP_IN.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_IN.MaintainTime.Value = 50;
            DI_CP_IN.TimeOut.Value = 5000;

            DI_CP_OUT.ChannelIndex.Value = -1;
            DI_CP_OUT.PortIndex.Value = -1;
            DI_CP_OUT.Reverse.Value = true;
            DI_CP_OUT.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_OUT.MaintainTime.Value = 50;
            DI_CP_OUT.TimeOut.Value = 5000;

            DI_CP_40_IN.ChannelIndex.Value = -1;
            DI_CP_40_IN.PortIndex.Value = -1;
            DI_CP_40_IN.Reverse.Value = true;
            DI_CP_40_IN.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_40_IN.MaintainTime.Value = 50;
            DI_CP_40_IN.TimeOut.Value = 5000;

            DI_CP_40_OUT.ChannelIndex.Value = -1;
            DI_CP_40_OUT.PortIndex.Value = -1;
            DI_CP_40_OUT.Reverse.Value = true;
            DI_CP_40_OUT.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_40_OUT.MaintainTime.Value = 50;
            DI_CP_40_OUT.TimeOut.Value = 5000;

            DI_FO_OPEN.ChannelIndex.Value = -1;
            DI_FO_OPEN.PortIndex.Value = -1;
            DI_FO_OPEN.Reverse.Value = true;
            DI_FO_OPEN.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_OPEN.MaintainTime.Value = 50;
            DI_FO_OPEN.TimeOut.Value = 5000;

            DI_FO_CLOSE.ChannelIndex.Value = -1;
            DI_FO_CLOSE.PortIndex.Value = -1;
            DI_FO_CLOSE.Reverse.Value = true;
            DI_FO_CLOSE.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_CLOSE.MaintainTime.Value = 50;
            DI_FO_CLOSE.TimeOut.Value = 5000;

            DI_FO_COVER_OPEN.ChannelIndex.Value = -1;
            DI_FO_COVER_OPEN.PortIndex.Value = -1;
            DI_FO_COVER_OPEN.Reverse.Value = false;
            DI_FO_COVER_OPEN.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_COVER_OPEN.MaintainTime.Value = 50;
            DI_FO_COVER_OPEN.TimeOut.Value = 5000;

            DI_FO_COVER_CLOSE.ChannelIndex.Value = -1;
            DI_FO_COVER_CLOSE.PortIndex.Value = -1;
            DI_FO_COVER_CLOSE.Reverse.Value = true;
            DI_FO_COVER_CLOSE.IOOveride.Value = EnumIOOverride.NONE;
            DI_FO_COVER_CLOSE.MaintainTime.Value = 50;
            DI_FO_COVER_CLOSE.TimeOut.Value = 5000;

            DI_C6IN_C8IN_PRESENCE1.ChannelIndex.Value = 4;
            DI_C6IN_C8IN_PRESENCE1.PortIndex.Value = 2;
            DI_C6IN_C8IN_PRESENCE1.Reverse.Value = false;
            DI_C6IN_C8IN_PRESENCE1.IOOveride.Value = EnumIOOverride.NONE;
            DI_C6IN_C8IN_PRESENCE1.MaintainTime.Value = 50;
            DI_C6IN_C8IN_PRESENCE1.TimeOut.Value = 5000;

            DI_C6IN_C8IN_PRESENCE2.ChannelIndex.Value = 4;
            DI_C6IN_C8IN_PRESENCE2.PortIndex.Value = 3;
            DI_C6IN_C8IN_PRESENCE2.Reverse.Value = false;
            DI_C6IN_C8IN_PRESENCE2.IOOveride.Value = EnumIOOverride.NONE;
            DI_C6IN_C8IN_PRESENCE2.MaintainTime.Value = 50;
            DI_C6IN_C8IN_PRESENCE2.TimeOut.Value = 5000;

            DI_C6IN_C8IN_PRESENCE3.ChannelIndex.Value = 4;
            DI_C6IN_C8IN_PRESENCE3.PortIndex.Value = 4;
            DI_C6IN_C8IN_PRESENCE3.Reverse.Value = false;
            DI_C6IN_C8IN_PRESENCE3.IOOveride.Value = EnumIOOverride.NONE;
            DI_C6IN_C8IN_PRESENCE3.MaintainTime.Value = 50;
            DI_C6IN_C8IN_PRESENCE3.TimeOut.Value = 5000;

            //DI_C6IN_PLACEMENT.ChannelIndex.Value = 4;
            //DI_C6IN_PLACEMENT.PortIndex.Value = 6;
            //DI_C6IN_PLACEMENT.Reverse.Value = false;
            //DI_C6IN_PLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            //DI_C6IN_PLACEMENT.MaintainTime.Value = 50;
            //DI_C6IN_PLACEMENT.TimeOut.Value = 10000;

            //DI_C8IN_PLACEMENT.ChannelIndex.Value = 4;
            //DI_C8IN_PLACEMENT.PortIndex.Value = 5;
            //DI_C8IN_PLACEMENT.Reverse.Value = false;
            //DI_C8IN_PLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            //DI_C8IN_PLACEMENT.MaintainTime.Value = 50;
            //DI_C8IN_PLACEMENT.TimeOut.Value = 10000;

            DI_C6IN_PLACEMENT.ChannelIndex.Value = 4;
            DI_C6IN_PLACEMENT.PortIndex.Value = 0;
            DI_C6IN_PLACEMENT.Reverse.Value = false;
            DI_C6IN_PLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C6IN_PLACEMENT.MaintainTime.Value = 50;
            DI_C6IN_PLACEMENT.TimeOut.Value = 5000;

            DI_C8IN_PLACEMENT.ChannelIndex.Value = 4;
            DI_C8IN_PLACEMENT.PortIndex.Value = 5;
            DI_C8IN_PLACEMENT.Reverse.Value = false;
            DI_C8IN_PLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C8IN_PLACEMENT.MaintainTime.Value = 50;
            DI_C8IN_PLACEMENT.TimeOut.Value = 5000;

            DI_C6IN_C8IN_NPLACEMENT.ChannelIndex.Value = 4;
            DI_C6IN_C8IN_NPLACEMENT.PortIndex.Value = 6;
            DI_C6IN_C8IN_NPLACEMENT.Reverse.Value = false;
            DI_C6IN_C8IN_NPLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C6IN_C8IN_NPLACEMENT.MaintainTime.Value = 50;
            DI_C6IN_C8IN_NPLACEMENT.TimeOut.Value = 5000;

            DI_C12IN_PRESENCE1.ChannelIndex.Value = -1;
            DI_C12IN_PRESENCE1.PortIndex.Value = -1;
            DI_C12IN_PRESENCE1.Reverse.Value = true;
            DI_C12IN_PRESENCE1.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_PRESENCE1.MaintainTime.Value = 50;
            DI_C12IN_PRESENCE1.TimeOut.Value = 5000;

            DI_C12IN_PRESENCE2.ChannelIndex.Value = -1;
            DI_C12IN_PRESENCE2.PortIndex.Value = -1;
            DI_C12IN_PRESENCE2.Reverse.Value = false;
            DI_C12IN_PRESENCE2.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_PRESENCE2.MaintainTime.Value = 50;
            DI_C12IN_PRESENCE2.TimeOut.Value = 5000;

            DI_C12IN_PLACEMENT.ChannelIndex.Value = -1;
            DI_C12IN_PLACEMENT.PortIndex.Value = -1;
            DI_C12IN_PLACEMENT.Reverse.Value = true;
            DI_C12IN_PLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_PLACEMENT.MaintainTime.Value = 50;
            DI_C12IN_PLACEMENT.TimeOut.Value = 5000;

            DI_C12IN_NPLACEMENT.ChannelIndex.Value = -1;
            DI_C12IN_NPLACEMENT.PortIndex.Value = -1;
            DI_C12IN_NPLACEMENT.Reverse.Value = true;
            DI_C12IN_NPLACEMENT.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_NPLACEMENT.MaintainTime.Value = 50;
            DI_C12IN_NPLACEMENT.TimeOut.Value = 5000;

            DI_C12IN_POSA.ChannelIndex.Value = -1;
            DI_C12IN_POSA.PortIndex.Value = -1;
            DI_C12IN_POSA.Reverse.Value = false;
            DI_C12IN_POSA.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_POSA.MaintainTime.Value = 50;
            DI_C12IN_POSA.TimeOut.Value = 5000;

            DI_C12IN_POSB.ChannelIndex.Value = -1;
            DI_C12IN_POSB.PortIndex.Value = -1;
            DI_C12IN_POSB.Reverse.Value = false;
            DI_C12IN_POSB.IOOveride.Value = EnumIOOverride.NONE;
            DI_C12IN_POSB.MaintainTime.Value = 50;
            DI_C12IN_POSB.TimeOut.Value = 5000;

            DI_SPARE7.ChannelIndex.Value = -1;
            DI_SPARE7.PortIndex.Value = -1;
            DI_SPARE7.Reverse.Value = false;
            DI_SPARE7.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE7.MaintainTime.Value = 50;
            DI_SPARE7.TimeOut.Value = 5000;

            DI_SPARE6.ChannelIndex.Value = -1;
            DI_SPARE6.PortIndex.Value = -1;
            DI_SPARE6.Reverse.Value = false;
            DI_SPARE6.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE6.MaintainTime.Value = 50;
            DI_SPARE6.TimeOut.Value = 5000;

            DI_SPARE5.ChannelIndex.Value = -1;
            DI_SPARE5.PortIndex.Value = -1;
            DI_SPARE5.Reverse.Value = false;
            DI_SPARE5.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE5.MaintainTime.Value = 50;
            DI_SPARE5.TimeOut.Value = 5000;

            DI_SPARE4.ChannelIndex.Value = -1;
            DI_SPARE4.PortIndex.Value = -1;
            DI_SPARE4.Reverse.Value = false;
            DI_SPARE4.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE4.MaintainTime.Value = 50;
            DI_SPARE4.TimeOut.Value = 5000;

            DI_SPARE3.ChannelIndex.Value = -1;
            DI_SPARE3.PortIndex.Value = -1;
            DI_SPARE3.Reverse.Value = false;
            DI_SPARE3.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE3.MaintainTime.Value = 50;
            DI_SPARE3.TimeOut.Value = 5000;

            DI_SPARE2.ChannelIndex.Value = -1;
            DI_SPARE2.PortIndex.Value = -1;
            DI_SPARE2.Reverse.Value = false;
            DI_SPARE2.IOOveride.Value = EnumIOOverride.NONE;
            DI_SPARE2.MaintainTime.Value = 50;
            DI_SPARE2.TimeOut.Value = 5000;

            DI_CP_ROT_IN.ChannelIndex.Value = -1;
            DI_CP_ROT_IN.PortIndex.Value = -1;
            DI_CP_ROT_IN.Reverse.Value = false;
            DI_CP_ROT_IN.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_ROT_IN.MaintainTime.Value = 50;
            DI_CP_ROT_IN.TimeOut.Value = 5000;

            DI_CP_ROT_OUT.ChannelIndex.Value = -1;
            DI_CP_ROT_OUT.PortIndex.Value = -1;
            DI_CP_ROT_OUT.Reverse.Value = false;
            DI_CP_ROT_OUT.IOOveride.Value = EnumIOOverride.NONE;
            DI_CP_ROT_OUT.MaintainTime.Value = 50;
            DI_CP_ROT_OUT.TimeOut.Value = 5000;

            DI_FOUP_COVER_LOCK.ChannelIndex.Value = -1;
            DI_FOUP_COVER_LOCK.PortIndex.Value = -1;
            DI_FOUP_COVER_LOCK.Reverse.Value = false;
            DI_FOUP_COVER_LOCK.IOOveride.Value = EnumIOOverride.NONE;
            DI_FOUP_COVER_LOCK.MaintainTime.Value = 50;
            DI_FOUP_COVER_LOCK.TimeOut.Value = 5000;

            DI_FOUP_COVER_UNLOCK.ChannelIndex.Value = -1;
            DI_FOUP_COVER_UNLOCK.PortIndex.Value = -1;
            DI_FOUP_COVER_UNLOCK.Reverse.Value = false;
            DI_FOUP_COVER_UNLOCK.IOOveride.Value = EnumIOOverride.NONE;
            DI_FOUP_COVER_UNLOCK.MaintainTime.Value = 50;
            DI_FOUP_COVER_UNLOCK.TimeOut.Value = 5000;

            //Cassette Tilting 
            DI_CSTT_DOWN.ChannelIndex.Value = 4;
            DI_CSTT_DOWN.PortIndex.Value = 7;
            DI_CSTT_DOWN.Reverse.Value = false;
            DI_CSTT_DOWN.IOOveride.Value = EnumIOOverride.NONE;
            DI_CSTT_DOWN.MaintainTime.Value = 50;
            DI_CSTT_DOWN.TimeOut.Value = 7000;

            DI_CSTT_UP.ChannelIndex.Value = 4;
            DI_CSTT_UP.PortIndex.Value = 7;
            DI_CSTT_UP.Reverse.Value = true;
            DI_CSTT_UP.IOOveride.Value = EnumIOOverride.NONE;
            DI_CSTT_UP.MaintainTime.Value = 50;
            DI_CSTT_UP.TimeOut.Value = 7000;
            #endregion
            #endregion

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public void RemoteFoupDefaultParam()
        {
            try
            {
                #region Input
                if (SystemModuleCount.ModuleCnt.FoupCount > 3) //DRAMSYSTEM_PLC (DRAX)
                {
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        DI_WAFER_OUTs.Add(new IOPortDescripter<bool>($"DI_WAFER_OUTs.{i}", EnumIOType.INPUT)); //Done
                        DI_WAFER_OUTs[i].ChannelIndex.Value = i + 5;
                        DI_WAFER_OUTs[i].PortIndex.Value = 13;
                        DI_WAFER_OUTs[i].Reverse.Value = false;
                        DI_WAFER_OUTs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_WAFER_OUTs[i].MaintainTime.Value = 100;
                        DI_WAFER_OUTs[i].TimeOut.Value = 500;

                        DI_CST12_PRESs.Add(new IOPortDescripter<bool>($"DI_CST12_PRESs.{i}", EnumIOType.INPUT)); //Done
                        DI_CST12_PRESs[i].ChannelIndex.Value = i + 5;
                        DI_CST12_PRESs[i].PortIndex.Value = 15;
                        DI_CST12_PRESs[i].Reverse.Value = false;
                        DI_CST12_PRESs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_CST12_PRESs[i].MaintainTime.Value = 100;
                        DI_CST12_PRESs[i].TimeOut.Value = 500;

                        DI_CST12_PRES2s.Add(new IOPortDescripter<bool>($"DI_CST12_PRES2s.{i}", EnumIOType.INPUT)); //Done
                        DI_CST12_PRES2s[i].ChannelIndex.Value = i + 5;
                        DI_CST12_PRES2s[i].PortIndex.Value = 16;
                        DI_CST12_PRES2s[i].Reverse.Value = false;
                        DI_CST12_PRES2s[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_CST12_PRES2s[i].MaintainTime.Value = 100;
                        DI_CST12_PRES2s[i].TimeOut.Value = 500;

                        DI_CST_LOCK12s.Add(new IOPortDescripter<bool>($"DI_CST_LOCK12s.{i}", EnumIOType.INPUT)); //Done
                        DI_CST_LOCK12s[i].ChannelIndex.Value = i + 5;
                        DI_CST_LOCK12s[i].PortIndex.Value = 4;
                        DI_CST_LOCK12s[i].Reverse.Value = false;
                        DI_CST_LOCK12s[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_CST_LOCK12s[i].MaintainTime.Value = 100;
                        DI_CST_LOCK12s[i].TimeOut.Value = 500;

                        DI_CST_UNLOCK12s.Add(new IOPortDescripter<bool>($"DI_CST_UNLOCK12s.{i}", EnumIOType.INPUT)); //Done
                        DI_CST_UNLOCK12s[i].ChannelIndex.Value = i + 5;
                        DI_CST_UNLOCK12s[i].PortIndex.Value = 5;
                        DI_CST_UNLOCK12s[i].Reverse.Value = false;
                        DI_CST_UNLOCK12s[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_CST_UNLOCK12s[i].MaintainTime.Value = 100;
                        DI_CST_UNLOCK12s[i].TimeOut.Value = 500;

                        DI_DP_INs.Add(new IOPortDescripter<bool>($"DI_DP_INs.{i}", EnumIOType.INPUT)); //Done
                        DI_DP_INs[i].ChannelIndex.Value = i + 5;
                        DI_DP_INs[i].PortIndex.Value = 0;
                        DI_DP_INs[i].Reverse.Value = false;
                        DI_DP_INs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_DP_INs[i].MaintainTime.Value = 100;
                        DI_DP_INs[i].TimeOut.Value = 500;

                        DI_DP_OUTs.Add(new IOPortDescripter<bool>($"DI_DP_OUTs.{i}", EnumIOType.INPUT)); //Done
                        DI_DP_OUTs[i].ChannelIndex.Value = i + 5;
                        DI_DP_OUTs[i].PortIndex.Value = 1;
                        DI_DP_OUTs[i].Reverse.Value = false;
                        DI_DP_OUTs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_DP_OUTs[i].MaintainTime.Value = 100;
                        DI_DP_OUTs[i].TimeOut.Value = 500;

                        DI_COVER_UPs.Add(new IOPortDescripter<bool>($"DI_COVER_UPs.{i}", EnumIOType.INPUT)); //Done
                        DI_COVER_UPs[i].ChannelIndex.Value = i + 5;
                        DI_COVER_UPs[i].PortIndex.Value = 8;
                        DI_COVER_UPs[i].Reverse.Value = false;
                        DI_COVER_UPs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_COVER_UPs[i].MaintainTime.Value = 100;
                        DI_COVER_UPs[i].TimeOut.Value = 500;

                        DI_COVER_DOWNs.Add(new IOPortDescripter<bool>($"DI_COVER_DOWNs.{i}", EnumIOType.INPUT)); //Done
                        DI_COVER_DOWNs[i].ChannelIndex.Value = i + 5;
                        DI_COVER_DOWNs[i].PortIndex.Value = 9;
                        DI_COVER_DOWNs[i].Reverse.Value = false;
                        DI_COVER_DOWNs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_COVER_DOWNs[i].MaintainTime.Value = 100;
                        DI_COVER_DOWNs[i].TimeOut.Value = 500;

                        DI_COVER_OPENs.Add(new IOPortDescripter<bool>($"DI_COVER_OPENs.{i}", EnumIOType.INPUT)); // Done = Ups
                        DI_COVER_OPENs[i].ChannelIndex.Value = i + 5;
                        DI_COVER_OPENs[i].PortIndex.Value = 8;
                        DI_COVER_OPENs[i].Reverse.Value = false;
                        DI_COVER_OPENs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_COVER_OPENs[i].MaintainTime.Value = 100;
                        DI_COVER_OPENs[i].TimeOut.Value = 500;

                        DI_COVER_CLOSEs.Add(new IOPortDescripter<bool>($"DI_COVER_CLOSEs.{i}", EnumIOType.INPUT)); // Done = Downs
                        DI_COVER_CLOSEs[i].ChannelIndex.Value = i + 5;
                        DI_COVER_CLOSEs[i].PortIndex.Value = 9;
                        DI_COVER_CLOSEs[i].Reverse.Value = false;
                        DI_COVER_CLOSEs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_COVER_CLOSEs[i].MaintainTime.Value = 100;
                        DI_COVER_CLOSEs[i].TimeOut.Value = 500;

                        DI_COVER_UNLOCKs.Add(new IOPortDescripter<bool>($"DI_COVER_UNLOCKs.{i}", EnumIOType.INPUT)); //Done
                        DI_COVER_UNLOCKs[i].ChannelIndex.Value = i + 5;
                        DI_COVER_UNLOCKs[i].PortIndex.Value = 5;
                        DI_COVER_UNLOCKs[i].Reverse.Value = false;
                        DI_COVER_UNLOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_COVER_UNLOCKs[i].MaintainTime.Value = 100;
                        DI_COVER_UNLOCKs[i].TimeOut.Value = 500;

                        DI_COVER_LOCKs.Add(new IOPortDescripter<bool>($"DI_COVER_LOCKs.{i}", EnumIOType.INPUT)); //Done
                        DI_COVER_LOCKs[i].ChannelIndex.Value = i + 5;
                        DI_COVER_LOCKs[i].PortIndex.Value = 4;
                        DI_COVER_LOCKs[i].Reverse.Value = false;
                        DI_COVER_LOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_COVER_LOCKs[i].MaintainTime.Value = 100;
                        DI_COVER_LOCKs[i].TimeOut.Value = 500;

                        DI_FOUP_LOAD_BUTTONs.Add(new IOPortDescripter<bool>($"DI_FOUP_LOAD_BUTTONs.{i}", EnumIOType.INPUT)); //Done
                        DI_FOUP_LOAD_BUTTONs[i].ChannelIndex.Value = i + 5;
                        DI_FOUP_LOAD_BUTTONs[i].PortIndex.Value = 22;
                        DI_FOUP_LOAD_BUTTONs[i].Reverse.Value = false;
                        DI_FOUP_LOAD_BUTTONs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_FOUP_LOAD_BUTTONs[i].MaintainTime.Value = 100;
                        DI_FOUP_LOAD_BUTTONs[i].TimeOut.Value = 500;

                        DI_FOUP_UNLOAD_BUTTONs.Add(new IOPortDescripter<bool>($"DI_FOUP_UNLOAD_BUTTONs.{i}", EnumIOType.INPUT)); //Done
                        DI_FOUP_UNLOAD_BUTTONs[i].ChannelIndex.Value = i + 5;
                        DI_FOUP_UNLOAD_BUTTONs[i].PortIndex.Value = 23;
                        DI_FOUP_UNLOAD_BUTTONs[i].Reverse.Value = false;
                        DI_FOUP_UNLOAD_BUTTONs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_FOUP_UNLOAD_BUTTONs[i].MaintainTime.Value = 100;
                        DI_FOUP_UNLOAD_BUTTONs[i].TimeOut.Value = 500;

                        DI_CST_Exists.Add(new IOPortDescripter<bool>($"DI_CST_Exists.{i}", EnumIOType.INPUT)); //PLC에서 확인 필요.
                        DI_CST_Exists[i].ChannelIndex.Value = i + 5; 
                        DI_CST_Exists[i].PortIndex.Value = 26; 
                        DI_CST_Exists[i].Reverse.Value = false;
                        DI_CST_Exists[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_CST_Exists[i].MaintainTime.Value = 100;
                        DI_CST_Exists[i].TimeOut.Value = 500;

                        DI_CST_MappingOuts.Add(new IOPortDescripter<bool>($"DI_CST_MappingOuts.{i}", EnumIOType.INPUT)); //Done
                        DI_CST_MappingOuts[i].ChannelIndex.Value = i + 5;
                        DI_CST_MappingOuts[i].PortIndex.Value = 13;
                        DI_CST_MappingOuts[i].Reverse.Value = false;
                        DI_CST_MappingOuts[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_CST_MappingOuts[i].MaintainTime.Value = 100;
                        DI_CST_MappingOuts[i].TimeOut.Value = 500;

                        DI_CST_CoverVacuums.Add(new IOPortDescripter<bool>($"DI_CST_CoverVacuums.{i}", EnumIOType.INPUT)); //Done
                        DI_CST_CoverVacuums[i].ChannelIndex.Value = i + 5;
                        DI_CST_CoverVacuums[i].PortIndex.Value = 14;
                        DI_CST_CoverVacuums[i].Reverse.Value = false;
                        DI_CST_CoverVacuums[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_CST_CoverVacuums[i].MaintainTime.Value = 100;
                        DI_CST_CoverVacuums[i].TimeOut.Value = 500;
                    }

                }
                else //OPRT_nC & OPERA_PLC(master) (Opera, GOP, MPT)
                {
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        DI_WAFER_OUTs.Add(new IOPortDescripter<bool>($"DI_WAFER_OUTs.{i}", EnumIOType.INPUT)); //DONE
                        DI_WAFER_OUTs[i].ChannelIndex.Value = 2;
                        DI_WAFER_OUTs[i].PortIndex.Value = 27 + i;
                        DI_WAFER_OUTs[i].Reverse.Value = false;
                        DI_WAFER_OUTs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_WAFER_OUTs[i].MaintainTime.Value = 100;
                        DI_WAFER_OUTs[i].TimeOut.Value = 500;
                         
                        DI_CST12_PRESs.Add(new IOPortDescripter<bool>($"DI_CST12_PRESs.{i}", EnumIOType.INPUT)); //DONE
                        DI_CST12_PRESs[i].ChannelIndex.Value = 3;
                        DI_CST12_PRESs[i].PortIndex.Value = 3 + (i * 2);
                        DI_CST12_PRESs[i].Reverse.Value = false;
                        DI_CST12_PRESs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_CST12_PRESs[i].MaintainTime.Value = 100;
                        DI_CST12_PRESs[i].TimeOut.Value = 500;

                        DI_CST12_PRES2s.Add(new IOPortDescripter<bool>($"DI_CST12_PRES2s.{i}", EnumIOType.INPUT)); //DONE
                        DI_CST12_PRES2s[i].ChannelIndex.Value = 3;
                        DI_CST12_PRES2s[i].PortIndex.Value = 4 + (i * 2);
                        DI_CST12_PRES2s[i].Reverse.Value = false;
                        DI_CST12_PRES2s[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_CST12_PRES2s[i].MaintainTime.Value = 100;
                        DI_CST12_PRES2s[i].TimeOut.Value = 500;

                        DI_CST_LOCK12s.Add(new IOPortDescripter<bool>($"DI_CST_LOCK12s.{i}", EnumIOType.INPUT)); //DONE
                        DI_CST_LOCK12s[i].ChannelIndex.Value = 2;
                        DI_CST_LOCK12s[i].PortIndex.Value = 0 + i;
                        DI_CST_LOCK12s[i].Reverse.Value = false;
                        DI_CST_LOCK12s[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_CST_LOCK12s[i].MaintainTime.Value = 100;
                        DI_CST_LOCK12s[i].TimeOut.Value = 500;

                        DI_CST_UNLOCK12s.Add(new IOPortDescripter<bool>($"DI_CST_UNLOCK12s.{i}", EnumIOType.INPUT)); //DONE
                        DI_CST_UNLOCK12s[i].ChannelIndex.Value = 2;
                        DI_CST_UNLOCK12s[i].PortIndex.Value = 3 + i;
                        DI_CST_UNLOCK12s[i].Reverse.Value = false;
                        DI_CST_UNLOCK12s[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_CST_UNLOCK12s[i].MaintainTime.Value = 100;
                        DI_CST_UNLOCK12s[i].TimeOut.Value = 500;

                        DI_DP_INs.Add(new IOPortDescripter<bool>($"DI_DP_INs.{i}", EnumIOType.INPUT)); //DONE
                        DI_DP_INs[i].ChannelIndex.Value = 1;
                        DI_DP_INs[i].PortIndex.Value = 18 + i;
                        DI_DP_INs[i].Reverse.Value = false;
                        DI_DP_INs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_DP_INs[i].MaintainTime.Value = 100;
                        DI_DP_INs[i].TimeOut.Value = 500;

                        DI_DP_OUTs.Add(new IOPortDescripter<bool>($"DI_DP_OUTs.{i}", EnumIOType.INPUT)); //DONE
                        DI_DP_OUTs[i].ChannelIndex.Value = 1;
                        DI_DP_OUTs[i].PortIndex.Value = 21 + i;
                        DI_DP_OUTs[i].Reverse.Value = false;
                        DI_DP_OUTs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_DP_OUTs[i].MaintainTime.Value = 100;
                        DI_DP_OUTs[i].TimeOut.Value = 500;

                        DI_COVER_UPs.Add(new IOPortDescripter<bool>($"DI_COVER_UPs.{i}", EnumIOType.INPUT)); //DONE
                        DI_COVER_UPs[i].ChannelIndex.Value = 2;
                        DI_COVER_UPs[i].PortIndex.Value = 12 + i;
                        DI_COVER_UPs[i].Reverse.Value = false;
                        DI_COVER_UPs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_COVER_UPs[i].MaintainTime.Value = 100;
                        DI_COVER_UPs[i].TimeOut.Value = 500;

                        DI_COVER_DOWNs.Add(new IOPortDescripter<bool>($"DI_COVER_DOWNs.{i}", EnumIOType.INPUT)); //DONE
                        DI_COVER_DOWNs[i].ChannelIndex.Value = 2;
                        DI_COVER_DOWNs[i].PortIndex.Value = 15 + i;
                        DI_COVER_DOWNs[i].Reverse.Value = false;
                        DI_COVER_DOWNs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_COVER_DOWNs[i].MaintainTime.Value = 100;
                        DI_COVER_DOWNs[i].TimeOut.Value = 500;

                        DI_COVER_OPENs.Add(new IOPortDescripter<bool>($"DI_COVER_OPENs.{i}", EnumIOType.INPUT)); //Done = Ups
                        DI_COVER_OPENs[i].ChannelIndex.Value = 2;
                        DI_COVER_OPENs[i].PortIndex.Value = 12 + i;
                        DI_COVER_OPENs[i].Reverse.Value = false;
                        DI_COVER_OPENs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_COVER_OPENs[i].MaintainTime.Value = 100;
                        DI_COVER_OPENs[i].TimeOut.Value = 500;

                        DI_COVER_CLOSEs.Add(new IOPortDescripter<bool>($"DI_COVER_CLOSEs.{i}", EnumIOType.INPUT)); //Done = Downs
                        DI_COVER_CLOSEs[i].ChannelIndex.Value = 2;
                        DI_COVER_CLOSEs[i].PortIndex.Value = 15 + i;
                        DI_COVER_CLOSEs[i].Reverse.Value = false;
                        DI_COVER_CLOSEs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_COVER_CLOSEs[i].MaintainTime.Value = 100;
                        DI_COVER_CLOSEs[i].TimeOut.Value = 500;

                        DI_COVER_UNLOCKs.Add(new IOPortDescripter<bool>($"DI_COVER_UNLOCKs.{i}", EnumIOType.INPUT)); //Done
                        DI_COVER_UNLOCKs[i].ChannelIndex.Value = 1 + i;
                        DI_COVER_UNLOCKs[i].PortIndex.Value = 31; 
                        DI_COVER_UNLOCKs[i].Reverse.Value = false;
                        DI_COVER_UNLOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_COVER_UNLOCKs[i].MaintainTime.Value = 100;
                        DI_COVER_UNLOCKs[i].TimeOut.Value = 500;

                        DI_COVER_LOCKs.Add(new IOPortDescripter<bool>($"DI_COVER_LOCKs.{i}", EnumIOType.INPUT)); //Done
                        DI_COVER_LOCKs[i].ChannelIndex.Value = 1 + i; 
                        DI_COVER_LOCKs[i].PortIndex.Value = 30;
                        DI_COVER_LOCKs[i].Reverse.Value = false;
                        DI_COVER_LOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_COVER_LOCKs[i].MaintainTime.Value = 100;
                        DI_COVER_LOCKs[i].TimeOut.Value = 500;

                        DI_FOUP_LOAD_BUTTONs.Add(new IOPortDescripter<bool>($"DI_FOUP_LOAD_BUTTONs.{i}", EnumIOType.INPUT)); //Done
                        DI_FOUP_LOAD_BUTTONs[i].ChannelIndex.Value = 3;
                        DI_FOUP_LOAD_BUTTONs[i].PortIndex.Value = 24 + i;
                        DI_FOUP_LOAD_BUTTONs[i].Reverse.Value = false;
                        DI_FOUP_LOAD_BUTTONs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_FOUP_LOAD_BUTTONs[i].MaintainTime.Value = 100;
                        DI_FOUP_LOAD_BUTTONs[i].TimeOut.Value = 500;

                        DI_FOUP_UNLOAD_BUTTONs.Add(new IOPortDescripter<bool>($"DI_FOUP_UNLOAD_BUTTONs.{i}", EnumIOType.INPUT)); //Done
                        DI_FOUP_UNLOAD_BUTTONs[i].ChannelIndex.Value = 3;
                        DI_FOUP_UNLOAD_BUTTONs[i].PortIndex.Value = 27 + i;
                        DI_FOUP_UNLOAD_BUTTONs[i].Reverse.Value = false;
                        DI_FOUP_UNLOAD_BUTTONs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_FOUP_UNLOAD_BUTTONs[i].MaintainTime.Value = 100;
                        DI_FOUP_UNLOAD_BUTTONs[i].TimeOut.Value = 500;

                        DI_CST_Exists.Add(new IOPortDescripter<bool>($"DI_CST_Exists.{i}", EnumIOType.INPUT)); //Done
                        DI_CST_Exists[i].ChannelIndex.Value = 2;
                        DI_CST_Exists[i].PortIndex.Value = 9 + i;
                        DI_CST_Exists[i].Reverse.Value = false;
                        DI_CST_Exists[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_CST_Exists[i].MaintainTime.Value = 100;
                        DI_CST_Exists[i].TimeOut.Value = 500;

                        DI_CST_MappingOuts.Add(new IOPortDescripter<bool>($"DI_CST_MappingOuts.{i}", EnumIOType.INPUT)); //Done
                        DI_CST_MappingOuts[i].ChannelIndex.Value = 2;
                        DI_CST_MappingOuts[i].PortIndex.Value = 24 + i;
                        DI_CST_MappingOuts[i].Reverse.Value = false;
                        DI_CST_MappingOuts[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_CST_MappingOuts[i].MaintainTime.Value = 100;
                        DI_CST_MappingOuts[i].TimeOut.Value = 500;

                        DI_CST_CoverVacuums.Add(new IOPortDescripter<bool>($"DI_CST_CoverVacuums.{i}", EnumIOType.INPUT)); //Done
                        DI_CST_CoverVacuums[i].ChannelIndex.Value = 3;
                        DI_CST_CoverVacuums[i].PortIndex.Value = 0 + i;
                        DI_CST_CoverVacuums[i].Reverse.Value = false;
                        DI_CST_CoverVacuums[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_CST_CoverVacuums[i].MaintainTime.Value = 100;
                        DI_CST_CoverVacuums[i].TimeOut.Value = 500;

                        // selly port index 수정하기
                        DI_PAD_As.Add(new IOPortDescripter<bool>($"DI_PAD_As.{i}", EnumIOType.INPUT)); //Done
                        DI_PAD_As[i].ChannelIndex.Value = -1;
                        DI_PAD_As[i].PortIndex.Value = -1;
                        DI_PAD_As[i].Reverse.Value = false;
                        DI_PAD_As[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_PAD_As[i].MaintainTime.Value = 100;
                        DI_PAD_As[i].TimeOut.Value = 500;

                        DI_PAD_Bs.Add(new IOPortDescripter<bool>($"DI_PAD_Bs.{i}", EnumIOType.INPUT)); //Done
                        DI_PAD_Bs[i].ChannelIndex.Value = -1;
                        DI_PAD_Bs[i].PortIndex.Value = -1;
                        DI_PAD_Bs[i].Reverse.Value = false;
                        DI_PAD_Bs[i].IOOveride.Value = EnumIOOverride.NONE;
                        DI_PAD_Bs[i].MaintainTime.Value = 100;
                        DI_PAD_Bs[i].TimeOut.Value = 500;

                    }

                }
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
    public class FoupOutputPortDefinitions : INotifyPropertyChanged, IParamNode
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

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        #region // Foup Outputs
        private IOPortDescripter<bool> _DO_NULL = new IOPortDescripter<bool>("DO_NULL", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_NULL
        {
            get { return _DO_NULL; }
            set
            {
                if (value != this._DO_NULL)
                {
                    _DO_NULL = value;
                    NotifyPropertyChanged("DO_NULL");
                }
            }
        }

        private IOPortDescripter<bool> _DO_FO_ROTATOR_OPEN_AIR = new IOPortDescripter<bool>("DO_FO_ROTATOR_OPEN_AIR ", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_FO_ROTATOR_OPEN_AIR
        {
            get { return _DO_FO_ROTATOR_OPEN_AIR; }
            set
            {
                if (value != this._DO_FO_ROTATOR_OPEN_AIR)
                {
                    _DO_FO_ROTATOR_OPEN_AIR = value;
                    NotifyPropertyChanged("DO_FO_ROTATOR_OPEN_AIR");
                }
            }
        }

        private IOPortDescripter<bool> _DO_FO_ROTATOR_CLOSE_AIR = new IOPortDescripter<bool>("DO_FO_ROTATOR_CLOSE_AIR", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_FO_ROTATOR_CLOSE_AIR
        {
            get { return _DO_FO_ROTATOR_CLOSE_AIR; }
            set
            {
                if (value != this._DO_FO_ROTATOR_CLOSE_AIR)
                {
                    _DO_FO_ROTATOR_CLOSE_AIR = value;
                    NotifyPropertyChanged("DO_FO_ROTATOR_CLOSE_AIR");
                }
            }
        }

        private IOPortDescripter<bool> _DO_FO_VAC_AIR = new IOPortDescripter<bool>("DO_FO_VAC_AIR", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_FO_VAC_AIR
        {
            get { return _DO_FO_VAC_AIR; }
            set
            {
                if (value != this._DO_FO_VAC_AIR)
                {
                    _DO_FO_VAC_AIR = value;
                    NotifyPropertyChanged("DO_FO_VAC_AIR");
                }
            }
        }

        private IOPortDescripter<bool> _DO_SERIAL = new IOPortDescripter<bool>("DO_SERIAL", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_SERIAL
        {
            get { return _DO_SERIAL; }
            set
            {
                if (value != this._DO_SERIAL)
                {
                    _DO_SERIAL = value;
                    NotifyPropertyChanged("DO_SERIAL");
                }
            }
        }

        private IOPortDescripter<bool> _DO_CLK = new IOPortDescripter<bool>("DO_CLK", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CLK
        {
            get { return _DO_CLK; }
            set
            {
                if (value != this._DO_CLK)
                {
                    _DO_CLK = value;
                    NotifyPropertyChanged("DO_CLK");
                }
            }
        }
        private IOPortDescripter<bool> _DO_LATCH = new IOPortDescripter<bool>("DO_LATCH", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_LATCH
        {
            get { return _DO_LATCH; }
            set
            {
                if (value != this._DO_LATCH)
                {
                    _DO_LATCH = value;
                    NotifyPropertyChanged("DO_LATCH");
                }
            }
        }
        private IOPortDescripter<bool> _DO_C6IN_C8IN_LOCK_AIR = new IOPortDescripter<bool>("DO_C6IN_C8IN_LOCK_AIR", EnumIOType.OUTPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DO_C6IN_C8IN_LOCK_AIR
        {
            get { return _DO_C6IN_C8IN_LOCK_AIR; }
            set
            {
                if (value != this._DO_C6IN_C8IN_LOCK_AIR)
                {
                    _DO_C6IN_C8IN_LOCK_AIR = value;
                    NotifyPropertyChanged("DO_C6IN_C8IN_LOCK_AIR");
                }
            }
        }
        private IOPortDescripter<bool> _DO_C12IN_LOCK_AIR = new IOPortDescripter<bool>("DO_C12IN_LOCK_AIR", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_C12IN_LOCK_AIR
        {
            get { return _DO_C12IN_LOCK_AIR; }
            set
            {
                if (value != this._DO_C12IN_LOCK_AIR)
                {
                    _DO_C12IN_LOCK_AIR = value;
                    NotifyPropertyChanged("DO_C12IN_LOCK_AIR");
                }
            }
        }
        private IOPortDescripter<bool> _DO_CP_CYL_IN_AIR = new IOPortDescripter<bool>("DO_CP_CYL_IN_AIR", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CP_CYL_IN_AIR
        {
            get { return _DO_CP_CYL_IN_AIR; }
            set
            {
                if (value != this._DO_CP_CYL_IN_AIR)
                {
                    _DO_CP_CYL_IN_AIR = value;
                    NotifyPropertyChanged("DO_CP_CYL_IN_AIR");
                }
            }
        }

        private IOPortDescripter<bool> _DO_CP_CYL_OUT_AIR = new IOPortDescripter<bool>("DO_CP_CYL_OUT_AIR", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CP_CYL_OUT_AIR
        {
            get { return _DO_CP_CYL_OUT_AIR; }
            set
            {
                if (value != this._DO_CP_CYL_OUT_AIR)
                {
                    _DO_CP_CYL_OUT_AIR = value;
                    NotifyPropertyChanged("DO_CP_CYL_OUT_AIR");
                }
            }
        }
        private IOPortDescripter<bool> _DO_CP_40_CYL_IN_AIR = new IOPortDescripter<bool>("DO_CP_40_CYL_IN_AIR", EnumIOType.OUTPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DO_CP_40_CYL_IN_AIR
        {
            get { return _DO_CP_40_CYL_IN_AIR; }
            set
            {
                if (value != this._DO_CP_40_CYL_IN_AIR)
                {
                    _DO_CP_40_CYL_IN_AIR = value;
                    NotifyPropertyChanged("DO_CP_40_CYL_IN_AIR");
                }
            }
        }
        private IOPortDescripter<bool> _DO_CP_40_CYL_OUT_AIR = new IOPortDescripter<bool>("DO_CP_40_CYL_OUT_AIR", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CP_40_CYL_OUT_AIR
        {
            get { return _DO_CP_40_CYL_OUT_AIR; }
            set
            {
                if (value != this._DO_CP_40_CYL_OUT_AIR)
                {
                    _DO_CP_40_CYL_OUT_AIR = value;
                    NotifyPropertyChanged("DO_CP_40_CYL_OUT_AIR");
                }
            }
        }

        private IOPortDescripter<bool> _DO_FO_UP_AIR = new IOPortDescripter<bool>("DO_FO_UP_AIR", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_FO_UP_AIR
        {
            get { return _DO_FO_UP_AIR; }
            set
            {
                if (value != this._DO_FO_UP_AIR)
                {
                    _DO_FO_UP_AIR = value;
                    NotifyPropertyChanged("DO_FO_UP_AIR");
                }
            }
        }
        private IOPortDescripter<bool> _DO_FO_DO_WN_AIR = new IOPortDescripter<bool>("DO_FO_DO_WN_AIR", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_FO_DOWN_AIR
        {
            get { return _DO_FO_DO_WN_AIR; }
            set
            {
                if (value != this._DO_FO_DO_WN_AIR)
                {
                    _DO_FO_DO_WN_AIR = value;
                    NotifyPropertyChanged("DO_FO_DO_WN_AIR");
                }
            }
        }

        private IOPortDescripter<bool> _DO_CSTT_AIR = new IOPortDescripter<bool>("DO_CSTT_AIR", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CSTT_AIR
        {
            get { return _DO_CSTT_AIR; }
            set
            {
                if (value != this._DO_CSTT_AIR)
                {
                    _DO_CSTT_AIR = value;
                    NotifyPropertyChanged("DO_CSTT_AIR");
                }
            }
        }
        private IOPortDescripter<bool> _DO_LOAD_LAMP = new IOPortDescripter<bool>("DO_LOAD_LAMP", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_LOAD_LAMP
        {
            get { return _DO_LOAD_LAMP; }
            set
            {
                if (value != this._DO_LOAD_LAMP)
                {
                    _DO_LOAD_LAMP = value;
                    NotifyPropertyChanged("DO_LOAD_LAMP");
                }
            }
        }
        private IOPortDescripter<bool> _DO_UNLOAD_LAMP = new IOPortDescripter<bool>("DO_UNLOAD_LAMP", EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_UNLOAD_LAMP
        {
            get { return _DO_UNLOAD_LAMP; }
            set
            {
                if (value != this._DO_UNLOAD_LAMP)
                {
                    _DO_UNLOAD_LAMP = value;
                    NotifyPropertyChanged("DO_UNLOAD_LAMP");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_COVER_OPENs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_COVER_OPENs
        {
            get { return _DO_COVER_OPENs; }
            set
            {
                if (value != _DO_COVER_OPENs)
                {
                    _DO_COVER_OPENs = value;
                    NotifyPropertyChanged("DO_COVER_OPENs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_COVER_Closes = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_COVER_Closes
        {
            get { return _DO_COVER_Closes; }
            set
            {
                if (value != _DO_COVER_Closes)
                {
                    _DO_COVER_Closes = value;
                    NotifyPropertyChanged("DO_COVER_Closes");
                }
            }
        }

        private List<IOPortDescripter<bool>> _DO_COVER_LOCKs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_COVER_LOCKs
        {
            get { return _DO_COVER_LOCKs; }
            set
            {
                if (value != _DO_COVER_LOCKs)
                {
                    _DO_COVER_LOCKs = value;
                    NotifyPropertyChanged("DO_COVER_LOCKs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_COVER_UNLOCKs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_COVER_UNLOCKs
        {
            get { return _DO_COVER_UNLOCKs; }
            set
            {
                if (value != _DO_COVER_UNLOCKs)
                {
                    _DO_COVER_UNLOCKs = value;
                    NotifyPropertyChanged("DO_COVER_UNLOCKs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_LOADs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_LOADs
        {
            get { return _DO_CST_LOADs; }
            set
            {
                if (value != _DO_CST_LOADs)
                {
                    _DO_CST_LOADs = value;
                    NotifyPropertyChanged("DO_CST_LOADs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_UNLOADs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_UNLOADs
        {
            get { return _DO_CST_UNLOADs; }
            set
            {
                if (value != _DO_CST_UNLOADs)
                {
                    _DO_CST_UNLOADs = value;
                    NotifyPropertyChanged("DO_CST_UNLOADs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_12INCH_LOCKs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_12INCH_LOCKs
        {
            get { return _DO_CST_12INCH_LOCKs; }
            set
            {
                if (value != _DO_CST_12INCH_LOCKs)
                {
                    _DO_CST_12INCH_LOCKs = value;
                    NotifyPropertyChanged("DO_CST_12INCH_LOCKs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_12INCH_UNLOCKs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_12INCH_UNLOCKs
        {
            get { return _DO_CST_12INCH_UNLOCKs; }
            set
            {
                if (value != _DO_CST_12INCH_UNLOCKs)
                {
                    _DO_CST_12INCH_UNLOCKs = value;
                    NotifyPropertyChanged("DO_CST_12INCH_UNLOCKs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_8INCH_LOCKs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_8INCH_LOCKs
        {
            get { return _DO_CST_8INCH_LOCKs; }
            set
            {
                if (value != _DO_CST_8INCH_LOCKs)
                {
                    _DO_CST_8INCH_LOCKs = value;
                    NotifyPropertyChanged("DO_CST_8INCH_LOCKs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_8INCH_UNLOCKs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_8INCH_UNLOCKs
        {
            get { return _DO_CST_8INCH_UNLOCKs; }
            set
            {
                if (value != _DO_CST_8INCH_UNLOCKs)
                {
                    _DO_CST_8INCH_UNLOCKs = value;
                    NotifyPropertyChanged("DO_CST_8INCH_UNLOCKs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_VACUUMs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_VACUUMs
        {
            get { return _DO_CST_VACUUMs; }
            set
            {
                if (value != _DO_CST_VACUUMs)
                {
                    _DO_CST_VACUUMs = value;
                    NotifyPropertyChanged("DO_CST_VACUUMs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_MAPPINGs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_MAPPINGs
        {
            get { return _DO_CST_MAPPINGs; }
            set
            {
                if (value != _DO_CST_MAPPINGs)
                {
                    _DO_CST_MAPPINGs = value;
                    NotifyPropertyChanged("DO_CST_MAPPINGs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_IND_ALARMs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_IND_ALARMs
        {
            get { return _DO_CST_IND_ALARMs; }
            set
            {
                if (value != _DO_CST_IND_ALARMs)
                {
                    _DO_CST_IND_ALARMs = value;
                    NotifyPropertyChanged("DO_CST_IND_ALARMs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_IND_BUSYs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_IND_BUSYs
        {
            get { return _DO_CST_IND_BUSYs; }
            set
            {
                if (value != _DO_CST_IND_BUSYs)
                {
                    _DO_CST_IND_BUSYs = value;
                    NotifyPropertyChanged("DO_CST_IND_BUSYs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_IND_RESERVEDs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_IND_RESERVEDs
        {
            get { return _DO_CST_IND_RESERVEDs; }
            set
            {
                if (value != _DO_CST_IND_RESERVEDs)
                {
                    _DO_CST_IND_RESERVEDs = value;
                    NotifyPropertyChanged("DO_CST_IND_RESERVEDs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_IND_AUTOs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_IND_AUTOs
        {
            get { return _DO_CST_IND_AUTOs; }
            set
            {
                if (value != _DO_CST_IND_AUTOs)
                {
                    _DO_CST_IND_AUTOs = value;
                    NotifyPropertyChanged("DO_CST_IND_AUTOs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_IND_LOADs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_IND_LOADs
        {
            get { return _DO_CST_IND_LOADs; }
            set
            {
                if (value != _DO_CST_IND_LOADs)
                {
                    _DO_CST_IND_LOADs = value;
                    NotifyPropertyChanged("DO_CST_IND_LOADs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_IND_UNLOADs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_IND_UNLOADs
        {
            get { return _DO_CST_IND_UNLOADs; }
            set
            {
                if (value != _DO_CST_IND_UNLOADs)
                {
                    _DO_CST_IND_UNLOADs = value;
                    NotifyPropertyChanged("DO_CST_IND_UNLOADs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_IND_PLACEMENTs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_IND_PLACEMENTs
        {
            get { return _DO_CST_IND_PLACEMENTs; }
            set
            {
                if (value != _DO_CST_IND_PLACEMENTs)
                {
                    _DO_CST_IND_PLACEMENTs = value;
                    NotifyPropertyChanged("DO_CST_IND_PLACEMENTs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DO_CST_IND_PRESENCEs = new List<IOPortDescripter<bool>>();
        public List<IOPortDescripter<bool>> DO_CST_IND_PRESENCEs
        {
            get { return _DO_CST_IND_PRESENCEs; }
            set
            {
                if (value != _DO_CST_IND_PRESENCEs)
                {
                    _DO_CST_IND_PRESENCEs = value;
                    NotifyPropertyChanged("DO_CST_IND_PRESENCEs");
                }
            }
        }

        #endregion

        public void SetDefaultParam()
        {
            if (SystemManager.SysteMode == SystemModeEnum.Multiple)
            {
                RemoteFoupDefaultParam();
            }
            else
            {
                FoupBSCIDefaultParam();
            }
        }
        private void SetDefaultParam_ST()
        {
            FoupBSCIDefaultParam();
        }
        public void FoupNormalDefaultParam()
        {
            try
            {
            #region // normal 12inch top foup
            DO_FO_ROTATOR_OPEN_AIR.ChannelIndex.Value = 9;
            DO_FO_ROTATOR_OPEN_AIR.PortIndex.Value = 7;
            DO_FO_ROTATOR_OPEN_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_FO_ROTATOR_CLOSE_AIR.ChannelIndex.Value = 9;
            DO_FO_ROTATOR_CLOSE_AIR.PortIndex.Value = 6;
            DO_FO_ROTATOR_CLOSE_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_FO_VAC_AIR.ChannelIndex.Value = 9;
            DO_FO_VAC_AIR.PortIndex.Value = 5;
            DO_FO_VAC_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_SERIAL.ChannelIndex.Value = 9;
            DO_SERIAL.PortIndex.Value = 3;
            DO_SERIAL.IOOveride.Value = EnumIOOverride.NONE;

            DO_CLK.ChannelIndex.Value = 9;
            DO_CLK.PortIndex.Value = 2;
            DO_CLK.IOOveride.Value = EnumIOOverride.NONE;

            DO_LATCH.ChannelIndex.Value = 9;
            DO_LATCH.PortIndex.Value = 1;
            DO_LATCH.IOOveride.Value = EnumIOOverride.NONE;

            DO_C6IN_C8IN_LOCK_AIR.ChannelIndex.Value = 10;
            DO_C6IN_C8IN_LOCK_AIR.PortIndex.Value = 7;
            DO_C6IN_C8IN_LOCK_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_C12IN_LOCK_AIR.ChannelIndex.Value = 10;
            DO_C12IN_LOCK_AIR.PortIndex.Value = 6;
            DO_C12IN_LOCK_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_CP_CYL_IN_AIR.ChannelIndex.Value = 10;
            DO_CP_CYL_IN_AIR.PortIndex.Value = 5;
            DO_CP_CYL_IN_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_CP_CYL_OUT_AIR.ChannelIndex.Value = 10;
            DO_CP_CYL_OUT_AIR.PortIndex.Value = 4;
            DO_CP_CYL_OUT_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_CP_40_CYL_IN_AIR.ChannelIndex.Value = 10;
            DO_CP_40_CYL_IN_AIR.PortIndex.Value = 2;
            DO_CP_40_CYL_IN_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_CP_40_CYL_OUT_AIR.ChannelIndex.Value = 10;
            DO_CP_40_CYL_OUT_AIR.PortIndex.Value = 3;
            DO_CP_40_CYL_OUT_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_FO_UP_AIR.ChannelIndex.Value = 10;
            DO_FO_UP_AIR.PortIndex.Value = 1;
            DO_FO_UP_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_FO_DOWN_AIR.ChannelIndex.Value = 10;
            DO_FO_DOWN_AIR.PortIndex.Value = 0;
            DO_FO_DOWN_AIR.IOOveride.Value = EnumIOOverride.NONE;


            //Cassette Tilting 
            DO_CSTT_AIR.ChannelIndex.Value = 6;
            DO_CSTT_AIR.PortIndex.Value = 1;
            DO_CSTT_AIR.IOOveride.Value = EnumIOOverride.NONE;

            //LOAD SWITCH LED ON
            DO_LOAD_LAMP.ChannelIndex.Value = 6;
            DO_LOAD_LAMP.PortIndex.Value = 3;
            DO_LOAD_LAMP.IOOveride.Value = EnumIOOverride.NONE;

            //UNLOAD SWITCH LED ON
            DO_UNLOAD_LAMP.ChannelIndex.Value = 6;
            DO_UNLOAD_LAMP.PortIndex.Value = 4;
            DO_UNLOAD_LAMP.IOOveride.Value = EnumIOOverride.NONE;
            #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public void FoupBSCIDefaultParam()
        {
            try
            {
            #region //BSCI장비 8inch loader foup
            #region //BSCI장비 8inch loader foup OUTPUT
            DO_FO_ROTATOR_OPEN_AIR.ChannelIndex.Value = -1;
            DO_FO_ROTATOR_OPEN_AIR.PortIndex.Value = -1;
            DO_FO_ROTATOR_OPEN_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_FO_ROTATOR_CLOSE_AIR.ChannelIndex.Value = -1;
            DO_FO_ROTATOR_CLOSE_AIR.PortIndex.Value = -1;
            DO_FO_ROTATOR_CLOSE_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_FO_VAC_AIR.ChannelIndex.Value = -1;
            DO_FO_VAC_AIR.PortIndex.Value = -1;
            DO_FO_VAC_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_SERIAL.ChannelIndex.Value = -1;
            DO_SERIAL.PortIndex.Value = -1;
            DO_SERIAL.IOOveride.Value = EnumIOOverride.NONE;

            DO_CLK.ChannelIndex.Value = -1;
            DO_CLK.PortIndex.Value = -1;
            DO_CLK.IOOveride.Value = EnumIOOverride.NONE;

            DO_LATCH.ChannelIndex.Value = -1;
            DO_LATCH.PortIndex.Value = -1;
            DO_LATCH.IOOveride.Value = EnumIOOverride.NONE;

            DO_C6IN_C8IN_LOCK_AIR.ChannelIndex.Value = 5;
            DO_C6IN_C8IN_LOCK_AIR.PortIndex.Value = 0;
            DO_C6IN_C8IN_LOCK_AIR.IOOveride.Value = EnumIOOverride.NONE;


            DO_C12IN_LOCK_AIR.ChannelIndex.Value = -1;
            DO_C12IN_LOCK_AIR.PortIndex.Value = -1;
            DO_C12IN_LOCK_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_CP_CYL_IN_AIR.ChannelIndex.Value = -1;
            DO_CP_CYL_IN_AIR.PortIndex.Value = -1;
            DO_CP_CYL_IN_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_CP_CYL_OUT_AIR.ChannelIndex.Value = -1;
            DO_CP_CYL_OUT_AIR.PortIndex.Value = -1;
            DO_CP_CYL_OUT_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_CP_40_CYL_IN_AIR.ChannelIndex.Value = -1;
            DO_CP_40_CYL_IN_AIR.PortIndex.Value = -1;
            DO_CP_40_CYL_IN_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_CP_40_CYL_OUT_AIR.ChannelIndex.Value = -1;
            DO_CP_40_CYL_OUT_AIR.PortIndex.Value = -1;
            DO_CP_40_CYL_OUT_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_FO_UP_AIR.ChannelIndex.Value = -1;
            DO_FO_UP_AIR.PortIndex.Value = -1;
            DO_FO_UP_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_FO_DOWN_AIR.ChannelIndex.Value = -1;
            DO_FO_DOWN_AIR.PortIndex.Value = -1;
            DO_FO_DOWN_AIR.IOOveride.Value = EnumIOOverride.NONE;


            //Cassette Tilting 
            DO_CSTT_AIR.ChannelIndex.Value = 5;
            DO_CSTT_AIR.PortIndex.Value = 1;
            //DO_CSTT_AIR.ChannelIndex.Value = 10;  Test Machine
            //DO_CSTT_AIR.PortIndex.Value = 0; Test Machine
            DO_CSTT_AIR.IOOveride.Value = EnumIOOverride.NONE;

            //LOAD SWITCH LED ON
            DO_LOAD_LAMP.ChannelIndex.Value = 5;
            DO_LOAD_LAMP.PortIndex.Value = 4;
            DO_LOAD_LAMP.IOOveride.Value = EnumIOOverride.NONE;

            //UNLOAD SWITCH LED ON
            DO_UNLOAD_LAMP.ChannelIndex.Value = 5;
            DO_UNLOAD_LAMP.PortIndex.Value = 3;
            DO_UNLOAD_LAMP.IOOveride.Value = EnumIOOverride.NONE;

            #endregion
            #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public void FoupTestDefaultParam()
        {
            try
            {
            #region //TEST장비 8inch loader foup
            #region OUTPUT
            DO_FO_ROTATOR_OPEN_AIR.ChannelIndex.Value = -1;
            DO_FO_ROTATOR_OPEN_AIR.PortIndex.Value = -1;
            DO_FO_ROTATOR_OPEN_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_FO_ROTATOR_CLOSE_AIR.ChannelIndex.Value = -1;
            DO_FO_ROTATOR_CLOSE_AIR.PortIndex.Value = -1;
            DO_FO_ROTATOR_CLOSE_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_FO_VAC_AIR.ChannelIndex.Value = -1;
            DO_FO_VAC_AIR.PortIndex.Value = -1;
            DO_FO_VAC_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_SERIAL.ChannelIndex.Value = -1;
            DO_SERIAL.PortIndex.Value = -1;
            DO_SERIAL.IOOveride.Value = EnumIOOverride.NONE;

            DO_CLK.ChannelIndex.Value = -1;
            DO_CLK.PortIndex.Value = -1;
            DO_CLK.IOOveride.Value = EnumIOOverride.NONE;

            DO_LATCH.ChannelIndex.Value = -1;
            DO_LATCH.PortIndex.Value = -1;
            DO_LATCH.IOOveride.Value = EnumIOOverride.NONE;

            DO_C6IN_C8IN_LOCK_AIR.ChannelIndex.Value = 6;
            DO_C6IN_C8IN_LOCK_AIR.PortIndex.Value = 0;
            DO_C6IN_C8IN_LOCK_AIR.IOOveride.Value = EnumIOOverride.NONE;


            DO_C12IN_LOCK_AIR.ChannelIndex.Value = -1;
            DO_C12IN_LOCK_AIR.PortIndex.Value = -1;
            DO_C12IN_LOCK_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_CP_CYL_IN_AIR.ChannelIndex.Value = -1;
            DO_CP_CYL_IN_AIR.PortIndex.Value = -1;
            DO_CP_CYL_IN_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_CP_CYL_OUT_AIR.ChannelIndex.Value = -1;
            DO_CP_CYL_OUT_AIR.PortIndex.Value = -1;
            DO_CP_CYL_OUT_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_CP_40_CYL_IN_AIR.ChannelIndex.Value = -1;
            DO_CP_40_CYL_IN_AIR.PortIndex.Value = -1;
            DO_CP_40_CYL_IN_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_CP_40_CYL_OUT_AIR.ChannelIndex.Value = -1;
            DO_CP_40_CYL_OUT_AIR.PortIndex.Value = -1;
            DO_CP_40_CYL_OUT_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_FO_UP_AIR.ChannelIndex.Value = -1;
            DO_FO_UP_AIR.PortIndex.Value = -1;
            DO_FO_UP_AIR.IOOveride.Value = EnumIOOverride.NONE;

            DO_FO_DOWN_AIR.ChannelIndex.Value = -1;
            DO_FO_DOWN_AIR.PortIndex.Value = -1;
            DO_FO_DOWN_AIR.IOOveride.Value = EnumIOOverride.NONE;


            //Cassette Tilting 
            DO_CSTT_AIR.ChannelIndex.Value = 6;
            DO_CSTT_AIR.PortIndex.Value = 1;
            //DO_CSTT_AIR.ChannelIndex.Value = 10;  Test Machine
            //DO_CSTT_AIR.PortIndex.Value = 0; Test Machine
            DO_CSTT_AIR.IOOveride.Value = EnumIOOverride.NONE;

            //LOAD SWITCH LED ON
            DO_LOAD_LAMP.ChannelIndex.Value = 6;
            DO_LOAD_LAMP.PortIndex.Value = 4;
            DO_LOAD_LAMP.IOOveride.Value = EnumIOOverride.NONE;

            //UNLOAD SWITCH LED ON
            DO_UNLOAD_LAMP.ChannelIndex.Value = 6;
            DO_UNLOAD_LAMP.PortIndex.Value = 3;
            DO_UNLOAD_LAMP.IOOveride.Value = EnumIOOverride.NONE;
            #endregion
            #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public void RemoteFoupDefaultParam()
        {
            try
            {
                #region Output (Channel, Port 사용 안됨)
                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                {
                    DO_CST_12INCH_LOCKs.Add(new IOPortDescripter<bool>($"DO_CST_12INCH_LOCKs.{i}", EnumIOType.OUTPUT));
                    DO_CST_12INCH_LOCKs[i].ChannelIndex.Value = -1;
                    DO_CST_12INCH_LOCKs[i].PortIndex.Value = -1;
                    DO_CST_12INCH_LOCKs[i].Reverse.Value = false;
                    DO_CST_12INCH_LOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_12INCH_LOCKs[i].MaintainTime.Value = 100;
                    DO_CST_12INCH_LOCKs[i].TimeOut.Value = 500;

                    DO_CST_12INCH_UNLOCKs.Add(new IOPortDescripter<bool>($"DO_CST_12INCH_UNLOCKs.{i}", EnumIOType.OUTPUT));
                    DO_CST_12INCH_UNLOCKs[i].ChannelIndex.Value = -1;
                    DO_CST_12INCH_UNLOCKs[i].PortIndex.Value = -1;
                    DO_CST_12INCH_UNLOCKs[i].Reverse.Value = false;
                    DO_CST_12INCH_UNLOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_12INCH_UNLOCKs[i].MaintainTime.Value = 100;
                    DO_CST_12INCH_UNLOCKs[i].TimeOut.Value = 500;

                    DO_CST_LOADs.Add(new IOPortDescripter<bool>($"DO_CST_LOADs.{i}", EnumIOType.OUTPUT));
                    DO_CST_LOADs[i].ChannelIndex.Value = -1;
                    DO_CST_LOADs[i].PortIndex.Value = -1;
                    DO_CST_LOADs[i].Reverse.Value = false;
                    DO_CST_LOADs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_LOADs[i].MaintainTime.Value = 100;
                    DO_CST_LOADs[i].TimeOut.Value = 500;

                    DO_CST_UNLOADs.Add(new IOPortDescripter<bool>($"DO_CST_UNLOADs.{i}", EnumIOType.OUTPUT));
                    DO_CST_UNLOADs[i].ChannelIndex.Value = -1;
                    DO_CST_UNLOADs[i].PortIndex.Value = -1;
                    DO_CST_UNLOADs[i].Reverse.Value = false;
                    DO_CST_UNLOADs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_UNLOADs[i].MaintainTime.Value = 100;
                    DO_CST_UNLOADs[i].TimeOut.Value = 500;

                    DO_CST_VACUUMs.Add(new IOPortDescripter<bool>($"DO_CST_VACUUMs.{i}", EnumIOType.OUTPUT));
                    DO_CST_VACUUMs[i].ChannelIndex.Value = -1;
                    DO_CST_VACUUMs[i].PortIndex.Value = -1;
                    DO_CST_VACUUMs[i].Reverse.Value = false;
                    DO_CST_VACUUMs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_VACUUMs[i].MaintainTime.Value = 100;
                    DO_CST_VACUUMs[i].TimeOut.Value = 500;

                    DO_CST_MAPPINGs.Add(new IOPortDescripter<bool>($"DO_CST_MAPPINGs.{i}", EnumIOType.OUTPUT));
                    DO_CST_MAPPINGs[i].ChannelIndex.Value = -1;
                    DO_CST_MAPPINGs[i].PortIndex.Value = -1;
                    DO_CST_MAPPINGs[i].Reverse.Value = false;
                    DO_CST_MAPPINGs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_MAPPINGs[i].MaintainTime.Value = 100;
                    DO_CST_MAPPINGs[i].TimeOut.Value = 500;

                    DO_COVER_LOCKs.Add(new IOPortDescripter<bool>($"DO_COVER_LOCKs.{i}", EnumIOType.OUTPUT));
                    DO_COVER_LOCKs[i].ChannelIndex.Value = -1;
                    DO_COVER_LOCKs[i].PortIndex.Value = -1;
                    DO_COVER_LOCKs[i].Reverse.Value = false;
                    DO_COVER_LOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_COVER_LOCKs[i].MaintainTime.Value = 100;
                    DO_COVER_LOCKs[i].TimeOut.Value = 500;

                    DO_COVER_UNLOCKs.Add(new IOPortDescripter<bool>($"DO_COVER_UNLOCKs.{i}", EnumIOType.OUTPUT));
                    DO_COVER_UNLOCKs[i].ChannelIndex.Value = -1;
                    DO_COVER_UNLOCKs[i].PortIndex.Value = -1;
                    DO_COVER_UNLOCKs[i].Reverse.Value = false;
                    DO_COVER_UNLOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_COVER_UNLOCKs[i].MaintainTime.Value = 100;
                    DO_COVER_UNLOCKs[i].TimeOut.Value = 500;

                    DO_COVER_OPENs.Add(new IOPortDescripter<bool>($"DO_COVER_OPENs.{i}", EnumIOType.OUTPUT));
                    DO_COVER_OPENs[i].ChannelIndex.Value = -1;
                    DO_COVER_OPENs[i].PortIndex.Value = -1;
                    DO_COVER_OPENs[i].Reverse.Value = false;
                    DO_COVER_OPENs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_COVER_OPENs[i].MaintainTime.Value = 100;
                    DO_COVER_OPENs[i].TimeOut.Value = 500;

                    DO_COVER_Closes.Add(new IOPortDescripter<bool>($"DO_COVER_Closes.{i}", EnumIOType.OUTPUT));
                    DO_COVER_Closes[i].ChannelIndex.Value = -1;
                    DO_COVER_Closes[i].PortIndex.Value = -1;
                    DO_COVER_Closes[i].Reverse.Value = false;
                    DO_COVER_Closes[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_COVER_Closes[i].MaintainTime.Value = 100;
                    DO_COVER_Closes[i].TimeOut.Value = 500;

                    DO_CST_8INCH_UNLOCKs.Add(new IOPortDescripter<bool>($"DO_CST_8INCH_UNLOCKs.{i}", EnumIOType.OUTPUT));
                    DO_CST_8INCH_UNLOCKs[i].ChannelIndex.Value = -1;
                    DO_CST_8INCH_UNLOCKs[i].PortIndex.Value = -1;
                    DO_CST_8INCH_UNLOCKs[i].Reverse.Value = false;
                    DO_CST_8INCH_UNLOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_8INCH_UNLOCKs[i].MaintainTime.Value = 100;
                    DO_CST_8INCH_UNLOCKs[i].TimeOut.Value = 500;

                    DO_CST_8INCH_LOCKs.Add(new IOPortDescripter<bool>($"DO_CST_8INCH_LOCKs.{i}", EnumIOType.OUTPUT));
                    DO_CST_8INCH_LOCKs[i].ChannelIndex.Value = -1;
                    DO_CST_8INCH_LOCKs[i].PortIndex.Value = -1;
                    DO_CST_8INCH_LOCKs[i].Reverse.Value = false;
                    DO_CST_8INCH_LOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_8INCH_LOCKs[i].MaintainTime.Value = 100;
                    DO_CST_8INCH_LOCKs[i].TimeOut.Value = 500;

                    DO_CST_MAPPINGs.Add(new IOPortDescripter<bool>($"DO_CST_MAPPINGs.{i}", EnumIOType.OUTPUT));
                    DO_CST_MAPPINGs[i].ChannelIndex.Value = -1;
                    DO_CST_MAPPINGs[i].PortIndex.Value = -1;
                    DO_CST_MAPPINGs[i].Reverse.Value = false;
                    DO_CST_MAPPINGs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_MAPPINGs[i].MaintainTime.Value = 100;
                    DO_CST_MAPPINGs[i].TimeOut.Value = 500;

                    DO_CST_IND_ALARMs.Add(new IOPortDescripter<bool>($"DO_CST_IND_ALARMs.{i}", EnumIOType.OUTPUT));
                    DO_CST_IND_ALARMs[i].ChannelIndex.Value = -1;
                    DO_CST_IND_ALARMs[i].PortIndex.Value = -1;
                    DO_CST_IND_ALARMs[i].Reverse.Value = false;
                    DO_CST_IND_ALARMs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_IND_ALARMs[i].MaintainTime.Value = 100;
                    DO_CST_IND_ALARMs[i].TimeOut.Value = 500;

                    DO_CST_IND_BUSYs.Add(new IOPortDescripter<bool>($"DO_CST_IND_BUSYs.{i}", EnumIOType.OUTPUT));
                    DO_CST_IND_BUSYs[i].ChannelIndex.Value = -1;
                    DO_CST_IND_BUSYs[i].PortIndex.Value = -1;
                    DO_CST_IND_BUSYs[i].Reverse.Value = false;
                    DO_CST_IND_BUSYs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_IND_BUSYs[i].MaintainTime.Value = 100;
                    DO_CST_IND_BUSYs[i].TimeOut.Value = 500;

                    DO_CST_IND_RESERVEDs.Add(new IOPortDescripter<bool>($"DO_CST_IND_RESERVEDs.{i}", EnumIOType.OUTPUT));
                    DO_CST_IND_RESERVEDs[i].ChannelIndex.Value = -1;
                    DO_CST_IND_RESERVEDs[i].PortIndex.Value = -1;
                    DO_CST_IND_RESERVEDs[i].Reverse.Value = false;
                    DO_CST_IND_RESERVEDs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_IND_RESERVEDs[i].MaintainTime.Value = 100;
                    DO_CST_IND_RESERVEDs[i].TimeOut.Value = 500;

                    DO_CST_IND_AUTOs.Add(new IOPortDescripter<bool>($"DO_CST_IND_AUTOs.{i}", EnumIOType.OUTPUT));
                    DO_CST_IND_AUTOs[i].ChannelIndex.Value = -1;
                    DO_CST_IND_AUTOs[i].PortIndex.Value = -1;
                    DO_CST_IND_AUTOs[i].Reverse.Value = false;
                    DO_CST_IND_AUTOs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_IND_AUTOs[i].MaintainTime.Value = 100;
                    DO_CST_IND_AUTOs[i].TimeOut.Value = 500;

                    DO_CST_IND_LOADs.Add(new IOPortDescripter<bool>($"DO_CST_IND_LOADs.{i}", EnumIOType.OUTPUT));
                    DO_CST_IND_LOADs[i].ChannelIndex.Value = -1;
                    DO_CST_IND_LOADs[i].PortIndex.Value = -1;
                    DO_CST_IND_LOADs[i].Reverse.Value = false;
                    DO_CST_IND_LOADs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_IND_LOADs[i].MaintainTime.Value = 100;
                    DO_CST_IND_LOADs[i].TimeOut.Value = 500;

                    DO_CST_IND_UNLOADs.Add(new IOPortDescripter<bool>($"DO_CST_IND_UNLOADs.{i}", EnumIOType.OUTPUT));
                    DO_CST_IND_UNLOADs[i].ChannelIndex.Value = -1;
                    DO_CST_IND_UNLOADs[i].PortIndex.Value = -1;
                    DO_CST_IND_UNLOADs[i].Reverse.Value = false;
                    DO_CST_IND_UNLOADs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_IND_UNLOADs[i].MaintainTime.Value = 100;
                    DO_CST_IND_UNLOADs[i].TimeOut.Value = 500;

                    DO_CST_IND_PLACEMENTs.Add(new IOPortDescripter<bool>($"DO_CST_IND_PLACEMENTs.{i}", EnumIOType.OUTPUT));
                    DO_CST_IND_PLACEMENTs[i].ChannelIndex.Value = -1;
                    DO_CST_IND_PLACEMENTs[i].PortIndex.Value = -1;
                    DO_CST_IND_PLACEMENTs[i].Reverse.Value = false;
                    DO_CST_IND_PLACEMENTs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_IND_PLACEMENTs[i].MaintainTime.Value = 100;
                    DO_CST_IND_PLACEMENTs[i].TimeOut.Value = 500;

                    DO_CST_IND_PRESENCEs.Add(new IOPortDescripter<bool>($"DO_CST_IND_PRESENCEs.{i}", EnumIOType.OUTPUT));
                    DO_CST_IND_PRESENCEs[i].ChannelIndex.Value = -1;
                    DO_CST_IND_PRESENCEs[i].PortIndex.Value = -1;
                    DO_CST_IND_PRESENCEs[i].Reverse.Value = false;
                    DO_CST_IND_PRESENCEs[i].IOOveride.Value = EnumIOOverride.NONE;
                    DO_CST_IND_PRESENCEs[i].MaintainTime.Value = 100;
                    DO_CST_IND_PRESENCEs[i].TimeOut.Value = 500;

                }
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
