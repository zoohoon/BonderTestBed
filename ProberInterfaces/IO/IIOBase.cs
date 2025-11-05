using LogModule;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using ProberErrorCode;

using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace ProberInterfaces
{
    public enum IOLockState
    {
        UNLOCK,
        LOCK
    }
    public interface IIOBase : INotifyPropertyChanged, IFactoryModule
    {
        short DeviceNumber { get; set; }
        ObservableCollection<Channel> Channels { get; set; }

        bool DevConnected { get; }
        //bool ReadBit(IOPortDescripter<bool> io);

        IORet ReadBit(int port, int bit, out bool value, bool reverse = false, bool isForced = false, bool ForcedValue = false);
        IORet ReadValue(int channel, int port, out long value, bool reverse = false);
        IORet WriteBit(int channel, int port, bool value);
        IORet WriteValue(int channel, int port, long value);
        int WaitForIO(int port, int bit, bool level, long timeout = 0, bool isForced = false, bool ForcedValue = false);
        //  int WaitForIO(int channel, int port, bool level, long sustain = 0, long timeout = 10000);
        int MonitorForIO(int channel, int port, bool level, long sustain = 0, long timeout = 10000, bool reverse = false, bool isForced = false, bool ForcedValue = false, bool writelog = true, string ioKey = "");

        int InitIO(int devNum, ObservableCollection<Channel> channels);
        //EventCodeEnum InitIO(int devNum, ObservableCollection<Channel> channels, string loadFilePath);
        int DeInitIO();
    }
    public interface IECATIO
    {
        ushort AxisReference { get; set; }
        void ReadPIVar(ushort index, PIVarDirection direction, VAR_TYPE varType, ref PI_VAR_UNION varUnion);
        void WritePIVar(ushort index, PI_VAR_UNION varUnion, VAR_TYPE varType);
    }


    public class ConvertValuePort : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                byte _value = byte.Parse(value.ToString());
                int _param = int.Parse(parameter.ToString());
                if (((_value >> _param) & 0x1) == 0x1)
                {
                    return true;
                }
                return false;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long retValue = 0;
            return (object)retValue;
        }
    }

    public class ConvertPortColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                bool _value = bool.Parse(value.ToString());
                if (_value)
                {
                    return Brushes.DarkGreen;
                }
                return Brushes.DimGray;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long retValue = 0;
            return (object)retValue;
        }
    }
    public abstract class Channel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public Channel(int devIndex, int channelIndex, int portNum)
        {
            try
            {
                _DevIndex = devIndex;
                _ChannelIndex = channelIndex;
                Port = new ObservableCollection<PortState>();
                for (int portindex = 0; portindex < portNum; portindex++)
                {
                    Port.Add(new PortState());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public Channel(int portNum)
        {
            try
            {
                Port = new ObservableCollection<PortState>();
                for (int portindex = 0; portindex < portNum; portindex++)
                {
                    Port.Add(new PortState());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public Channel()
        {

        }

        private EnumIOType _IOType;
        public EnumIOType IOType
        {
            get { return _IOType; }
            set
            {
                if (this._IOType == value) return;
                this._IOType = value;
                RaisePropertyChanged();
            }
        }

        private int _DevIndex;
        public int DevIndex
        {
            get { return _DevIndex; }
            set
            {
                if (this._DevIndex == value) return;
                this._DevIndex = value;
                RaisePropertyChanged();
            }
        }

        private int _NodeIndex;
        public int NodeIndex
        {
            get { return _NodeIndex; }
            set
            {
                if (this._NodeIndex == value)
                {
                    return;
                }

                this._NodeIndex = value;
                RaisePropertyChanged();
            }
        }

        private int _ChannelIndex;
        public int ChannelIndex
        {
            get { return _ChannelIndex; }
            set
            {
                if (this._ChannelIndex == value) return;
                this._ChannelIndex = value;
                RaisePropertyChanged();
            }
        }

        private int _VarOffset;
        public int VarOffset
        {
            get { return _VarOffset; }
            set
            {
                if (this._VarOffset == value)
                {
                    return;
                }

                this._VarOffset = value;
                RaisePropertyChanged();
            }
        }

        private UInt32 _Value;

        public UInt32 Value
        {
            get { return _Value; }
            set
            {
                if (this._Value == value) return;
                this._Value = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<PortState> _Port = new ObservableCollection<PortState>();

        public ObservableCollection<PortState> Port
        {
            get { return _Port; }
            protected set
            {
                if (this._Port == value) return;
                this._Port = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<PortValue> _Values = new ObservableCollection<PortValue>();

        public ObservableCollection<PortValue> Values
        {
            get { return _Values; }
            protected set
            {
                if (this._Values == value) return;
                this._Values = value;
                RaisePropertyChanged();
            }
        }
    }
    public class InputChannel : Channel, INotifyPropertyChanged
    {
        public InputChannel(int devIndex, int channelIndex, int portNum, int nodeIndex, int varOffset)
        {
            try
            {
                DevIndex = devIndex;
                ChannelIndex = channelIndex;
                IOType = EnumIOType.INPUT;
                Port = new ObservableCollection<PortState>();
                Values = new ObservableCollection<PortValue>();
                for (int portindex = 0; portindex < portNum; portindex++)
                {
                    Port.Add(new PortState());
                    Values.Add(new PortValue());
                }
                NodeIndex = nodeIndex;
                VarOffset = varOffset;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public InputChannel(int portNum)
        {
            try
            {
                Port = new ObservableCollection<PortState>();
                Values = new ObservableCollection<PortValue>();
                for (int portindex = 0; portindex < portNum; portindex++)
                {
                    Port.Add(new PortState());
                    Values.Add(new PortValue());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


    }
    public class OutputChannel : Channel, INotifyPropertyChanged
    {
        public OutputChannel(int devIndex, int channelIndex, int portNum, int nodeIndex, int varOffset)
        {
            try
            {
                DevIndex = devIndex;
                ChannelIndex = channelIndex;
                IOType = EnumIOType.OUTPUT;
                Port = new ObservableCollection<PortState>();
                Values = new ObservableCollection<PortValue>();
                for (int portindex = 0; portindex < portNum; portindex++)
                {
                    Port.Add(new PortState());
                    Values.Add(new PortValue());
                }
                NodeIndex = nodeIndex;
                VarOffset = varOffset;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public OutputChannel(int portNum)
        {
            try
            {
                Port = new ObservableCollection<PortState>();
                Values = new ObservableCollection<PortValue>();
                for (int portindex = 0; portindex < portNum; portindex++)
                {
                    Port.Add(new PortState());
                    Values.Add(new PortValue());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
    public class AnalogInputChannel : Channel, INotifyPropertyChanged
    {
        public AnalogInputChannel(int devIndex, int channelIndex, int portNum, int nodeIndex, int varOffset)
        {
            try
            {
                DevIndex = devIndex;
                ChannelIndex = channelIndex;
                IOType = EnumIOType.AI;
                Port = new ObservableCollection<PortState>();
                Values = new ObservableCollection<PortValue>();
                for (int portindex = 0; portindex < portNum; portindex++)
                {
                    Port.Add(new PortState());
                    Values.Add(new PortValue());
                }
                NodeIndex = nodeIndex;
                VarOffset = varOffset;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public AnalogInputChannel(int portNum)
        {
            try
            {
                IOType = EnumIOType.AI;
                Port = new ObservableCollection<PortState>();
                Values = new ObservableCollection<PortValue>();
                for (int portindex = 0; portindex < portNum; portindex++)
                {
                    Port.Add(new PortState());
                    Values.Add(new PortValue());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


    }

    public class AnalogOutputChannel : Channel, INotifyPropertyChanged
    {
        public AnalogOutputChannel(int devIndex, int channelIndex, int portNum, int nodeIndex, int varOffset)
        {
            try
            {
                DevIndex = devIndex;
                ChannelIndex = channelIndex;
                IOType = EnumIOType.AO;
                Port = new ObservableCollection<PortState>();
                Values = new ObservableCollection<PortValue>();
                for (int portindex = 0; portindex < portNum; portindex++)
                {
                    Port.Add(new PortState());
                    Values.Add(new PortValue());
                }
                NodeIndex = nodeIndex;
                VarOffset = varOffset;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public AnalogOutputChannel(int portNum)
        {
            try
            {
                IOType = EnumIOType.AO;
                Port = new ObservableCollection<PortState>();
                Values = new ObservableCollection<PortValue>();
                for (int portindex = 0; portindex < portNum; portindex++)
                {
                    Port.Add(new PortState());
                    Values.Add(new PortValue());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


    }

    public class PortState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private bool _PortVal;
        public bool PortVal
        {
            get { return _PortVal; }
            set
            {
                if (this._PortVal.Equals(value)) return;
                this._PortVal = value;
                NotifyPropertyChanged("PortVal");
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _IOPortList = new ObservableCollection<IOPortDescripter<bool>>();

        public ObservableCollection<IOPortDescripter<bool>> IOPortList
        {
            get { return _IOPortList; }
            set
            {
                if (this._IOPortList == value) return;
                this._IOPortList = value;
                NotifyPropertyChanged("IOPortList");
            }
        }
        public string GetIOPortDescipterString()
        {
            if (IOPortList.Count > 0)
            {
                string str = "";
                foreach (IOPortDescripter<bool> portDesc in IOPortList)
                {
                    str += " " + portDesc.Key.ToString() + " ";
                }
                return str;
            }
            return "NoMapping";
        }
    }
    public class PortValue : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private long _PortVal;
        public long PortVal
        {
            get { return _PortVal; }
            set
            {
                if (this._PortVal.Equals(value)) return;
                this._PortVal = value;
                NotifyPropertyChanged("PortVal");
            }
        }
        private ObservableCollection<IOPortDescripter<long>> _IOPortList = new ObservableCollection<IOPortDescripter<long>>();
        public ObservableCollection<IOPortDescripter<long>> IOPortList
        {
            get { return _IOPortList; }
            set
            {
                if (this._IOPortList == value) return;
                this._IOPortList = value;
                NotifyPropertyChanged("IOPortList");
            }
        }
        public string GetIOPortDescipterString()
        {
            if (IOPortList.Count > 0)
            {
                string str = "";
                foreach (IOPortDescripter<long> portDesc in IOPortList)
                {
                    str += " " + portDesc.Key.ToString() + " ";
                }
                return str;
            }
            return "NoMapping";
        }
    }

    public enum IORet
    {
        WarningIntrNotAvailable = -1610612736,
        WarningParamOutOfRange = -1610612735,
        WarningPropValueOutOfRange = -1610612734,
        WarningPropValueNotSpted = -1610612733,
        WarningPropValueConflict = -1610612732,
        WarningVrgOfGroupNotSame = -1610612731,
        ErrorHandleNotValid = -536870912,
        ErrorParamOutOfRange = -536870911,
        ErrorParamNotSpted = -536870910,
        ErrorParamFmtUnexpted = -536870909,
        ErrorMemoryNotEnough = -536870908,
        ErrorBufferIsNull = -536870907,
        ErrorBufferTooSmall = -536870906,
        ErrorDataLenExceedLimit = -536870905,
        ErrorFuncNotSpted = -536870904,
        ErrorEventNotSpted = -536870903,
        ErrorPropNotSpted = -536870902,
        ErrorPropReadOnly = -536870901,
        ErrorPropValueConflict = -536870900,
        ErrorPropValueOutOfRange = -536870899,
        ErrorPropValueNotSpted = -536870898,
        ErrorPrivilegeNotHeld = -536870897,
        ErrorPrivilegeNotAvailable = -536870896,
        ErrorDriverNotFound = -536870895,
        ErrorDriverVerMismatch = -536870894,
        ErrorDriverCountExceedLimit = -536870893,
        ErrorDeviceNotOpened = -536870892,
        ErrorDeviceNotExist = -536870891,
        ErrorDeviceUnrecognized = -536870890,
        ErrorConfigDataLost = -536870889,
        ErrorFuncNotInited = -536870888,
        ErrorFuncBusy = -536870887,
        ErrorIntrNotAvailable = -536870886,
        ErrorDmaNotAvailable = -536870885,
        ErrorDeviceIoTimeOut = -536870884,
        ErrorSignatureNotMatch = -536870883,
        ErrorFuncConflictWithBfdAi = -536870882,
        ErrorVrgNotAvailableInSeMode = -536870881,
        ErrorUndefined = -536805377,

        UNKNOWN = -2,
        ERROR = -1,
        NO_ERR = 0,

        LAST_RET = NO_ERR + 1
    }


    [Serializable]
    public class IODevAddress
    {
        private string _DevAddresses;
        [XmlAttribute("Address")]
        public string DevAddresses
        {
            get { return _DevAddresses; }
            set { _DevAddresses = value; }
        }
    }
    public enum EnumIOType
    {
        UNDEFINED,
        INPUT,
        OUTPUT,
        BIDIRECTION,
        MEMORY,
        AI,
        AO,
        INT,
        CNT,
        MIXED,
        LAST_TYPE = MIXED
    }
    public enum EnumIONodeType
    {
        UNDEFINED,
        Axis,
        IO
    }
    public enum EnumIOOverride
    {
        NONE,
        NLO,
        NHI,
        EMUL
    }
    public enum EnumGroupType
    {
        UNDEFINED,
        SINGLE,
        GROUP,
        MASTER,
        SLAVE
    }

    public class ForcedIOValue : INotifyPropertyChanged
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public ForcedIOValue()
        {
            _IsForced = false;
            _ForecedValue = false;
        }

        private bool _IsForced;
        public bool IsForced
        {
            get { return _IsForced; }
            set
            {
                if (value != _IsForced)
                {
                    _IsForced = value;
                    NotifyPropertyChanged("IsForced");
                }
            }
        }

        private bool _ForecedValue;
        public bool ForecedValue
        {
            get { return _ForecedValue; }
            set
            {
                if (value != _ForecedValue)
                {
                    _ForecedValue = value;
                    NotifyPropertyChanged("ForecedValue");
                }
            }
        }

    }
    //public class IOBitDescripter
    //{

    //    private int _ChannelIndex;

    //    public int ChannelIndex
    //    {
    //        get { return _ChannelIndex; }
    //        set { _ChannelIndex = value; }
    //    }
    //    private int _PortIndex;

    //    public int PortIndex
    //    {
    //        get { return _PortIndex; }
    //        set { _PortIndex = value; }
    //    }
    //    private bool _Reverse;

    //    public bool Reverse
    //    {
    //        get { return _Reverse; }
    //        set { _Reverse = value; }
    //    }
    //    public IOBitDescripter(int channelindex, int portindex, bool reverse)
    //    {
    //        _ChannelIndex = channelindex;
    //        _PortIndex = portindex;
    //        _Reverse = reverse;
    //    }

    //}

    [Serializable, DataContract]
    public class IOPortDescripter<T> : INotifyPropertyChanged, IParamNode
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
        private Element<long> _TimeOut = new Element<long>();
        // [XmlAttribute("TimeOut")]
        [DataMember]
        public Element<long> TimeOut
        {
            get { return _TimeOut; }
            set
            {
                if (this._TimeOut == value) return;
                this._TimeOut = value;
                NotifyPropertyChanged("TimeOut");
            }
        }
        private Element<long> _MaintainTime = new Element<long>();
        // [XmlAttribute("MaintainTime")]
        [DataMember]
        public Element<long> MaintainTime
        {
            get { return _MaintainTime; }
            set
            {
                if (this._MaintainTime == value) return;
                this._MaintainTime = value;
                NotifyPropertyChanged("MaintainTime");
            }
        }
        private Element<EnumIOType> _IOType = new Element<EnumIOType>();
        //[XmlAttribute("Type")]
        [DataMember]
        public Element<EnumIOType> IOType
        {
            get { return _IOType; }
            set
            {
                if (this._IOType == value) return;
                this._IOType = value;
                NotifyPropertyChanged("IOType");
            }
        }
        private Element<EnumIOOverride> _IOOveride = new Element<EnumIOOverride>();
        //[XmlAttribute("Override")]
        [DataMember]
        public Element<EnumIOOverride> IOOveride
        {
            get { return _IOOveride; }
            set
            {
                if (this._IOOveride == value) return;
                this._IOOveride = value;
                NotifyPropertyChanged("IOOveride");
            }
        }
        private Element<int> _ChannelIndex = new Element<int>();
        //[XmlAttribute("Channel")]
        [DataMember]
        public Element<int> ChannelIndex
        {
            get { return _ChannelIndex; }
            set
            {
                if (this._ChannelIndex == value) return;
                this._ChannelIndex = value;
                NotifyPropertyChanged("ChannelIndex");
            }
        }

        private Element<int> _PortIndex = new Element<int>();
        //[XmlAttribute("Port")]
        [DataMember]
        public Element<int> PortIndex
        {
            get { return _PortIndex; }
            set
            {
                if (this._PortIndex == value) return;
                this._PortIndex = value;
                NotifyPropertyChanged("PortIndex");
            }
        }
        private Element<bool> _Reverse = new Element<bool>();
        //[XmlAttribute("Reverse")]
        [DataMember]
        public Element<bool> Reverse
        {
            get { return _Reverse; }
            set
            {
                if (this._Reverse == value) return;
                this._Reverse = value;
                NotifyPropertyChanged("Reverse");
            }
        }

        private Element<string> _Key = new Element<string>();
        //[XmlAttribute("Key")]
        [DataMember]
        public Element<string> Key
        {
            get { return _Key; }
            set
            {
                if (this._Key == value) return;
                this._Key = value;
                NotifyPropertyChanged("Key");
            }
        }
        private Element<string> _Description = new Element<string>();
        //[XmlAttribute("Description")]
        [DataMember]
        public Element<string> Description
        {
            get { return _Description; }
            set
            {
                if (this._Description == value) return;
                this._Description = value;
                NotifyPropertyChanged("Description");
            }
        }
        [NonSerialized]
        private IOLockState _LockState = IOLockState.UNLOCK;
        [XmlIgnore, JsonIgnore, DataMember]
        public IOLockState LockState
        {
            get { return _LockState; }
            set
            {
                if (this._LockState == value) return;
                this._LockState = value;
                NotifyPropertyChanged("LockState");
            }
        }
        [NonSerialized]
        private Element<string> _Alias = new Element<string>();
        //[XmlAttribute("Key")]
        [XmlIgnore, JsonIgnore, DataMember]
        public Element<string> Alias
        {
            get { return _Alias; }
            set
            {
                if (this._Alias == value) return;
                this._Alias = value;
                NotifyPropertyChanged("Alias");
            }
        }
        [NonSerialized]
        private Element<bool> _IsActive= new Element<bool>();
        //[XmlAttribute("Key")]
        [XmlIgnore, JsonIgnore, DataMember]
        public Element<bool> IsActive
        {
            get { return _IsActive; }
            set
            {
                if (this._IsActive == value) return;
                this._IsActive = value;
                NotifyPropertyChanged("IsActive");
            }
        }
        [NonSerialized]
        private ForcedIOValue _ForcedIO = new ForcedIOValue();
        [JsonIgnore]
        public ForcedIOValue ForcedIO
        {
            get { return _ForcedIO; }
            set
            {
                if (value != _ForcedIO)
                {
                    _ForcedIO = value;
                    NotifyPropertyChanged("ForcedIO");

                }
            }
        }
        [NonSerialized]
        private AsyncCommand _OutputOffCommand;
        [JsonIgnore]
        public ICommand OutputOffCommand
        {
            get
            {
                if (null == _OutputOffCommand) _OutputOffCommand = new AsyncCommand(ValueOffCommand);
                return _OutputOffCommand;
            }
        }
        [NonSerialized]
        private AsyncCommand _OutputOnCommand;
        [JsonIgnore]
        public ICommand OutputOnCommand
        {
            get
            {
                if (null == _OutputOnCommand) _OutputOnCommand = new AsyncCommand(ValueOnCommand);
                return _OutputOnCommand;
            }
        }

        [NonSerialized]
        private IIOService ioService;
        public IOPortDescripter()
        {
            try
            {
                Description.Value = "Undefined";
                Key.Value = Description.Value;
                _IOType.Value = EnumIOType.UNDEFINED;
                MaintainTime.Value = 0;
                TimeOut.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public IOPortDescripter(string portdesc, EnumIOType type)
        {
            try
            {
                _Description.Value = portdesc;
                _Key.Value = Description.Value;
                _IOType.Value = type;
                _IOOveride.Value = EnumIOOverride.NONE;
                MaintainTime.Value = 10;
                TimeOut.Value = 60000;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public IOPortDescripter(string portdesc, EnumIOType type, EnumIOOverride over)
        {
            try
            {
                _Description.Value = portdesc;
                _Key.Value = Description.Value;
                _IOType.Value = type;
                _IOOveride.Value = over;
                if (over != EnumIOOverride.NONE)
                {
                    _ChannelIndex.Value = -1;
                    _PortIndex.Value = -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public IOPortDescripter(int channelIndex, int portIndex, bool reverse, EnumIOType type)
        {
            try
            {
                ChannelIndex.Value = channelIndex;
                PortIndex.Value = portIndex;
                Reverse.Value = reverse;
                _IOType.Value = type;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public IOPortDescripter(int channelIndex, int portIndex, string key, EnumIOType type)
        {
            ChannelIndex.Value = channelIndex;
            PortIndex.Value = portIndex;
            Key.Value = key;
            _IOType.Value = type;

        }

        public void SetValue()
        {
            lock (lockObj)
            {
                if (this.IOType.Value == EnumIOType.OUTPUT)
                {
                    if (ioService != null)
                    {
                        try
                        {
                            if (typeof(T) == typeof(bool))
                            {
                                ioService.WriteBit((IOPortDescripter<bool>)Convert.ChangeType(this, typeof(IOPortDescripter<bool>)), true);
                                LoggerManager.Debug($"IIOBase.SetValue(): {this.Key}(#{this.ChannelIndex}.{this.PortIndex}) Port Setted.");
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            throw;
                        }
                    }
                }
            }
        }

        object lockObj = new object();
        public void ResetValue()
        {
            lock (lockObj)
            {
                if (this.IOType.Value == EnumIOType.OUTPUT)
                {
                    if (ioService != null)
                    {
                        try
                        {
                            if (typeof(T) == typeof(bool))
                            {
                                ioService.WriteBit((IOPortDescripter<bool>)Convert.ChangeType(this, typeof(IOPortDescripter<bool>)), false);
                                LoggerManager.Debug($"IIOBase.SetValue(): {this.Key}(#{this.ChannelIndex}.{this.PortIndex}) Port Resetted.");
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            throw;
                        }
                    }
                }
            }
        }

        private async Task ValueOffCommand()
        {
            try
            {
                Task t1 = new Task(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (this.Value)
                        {
                            LoggerManager.Debug($"ValueChangeCommand:{this.Key.Value} ResetValue.");
                            ResetValue();
                            //Thread.Sleep(1000);
                        }
                    });
                });
                t1.Start();
                await t1;




            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


        private async Task ValueOnCommand()
        {
            try
            {
                Task t1 = new Task(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (!this.Value)
                        {
                            LoggerManager.Debug($"ValueChangeCommand:{this.Key.Value} SetValue.");
                            SetValue();
                            //Thread.Sleep(1000);
                        }
                    });
                });
                t1.Start();
                await t1;


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public int MonitorForIO(bool HL)
        {
            //this.MaintainTime.Value = 500;
            //this.TimeOut.Value = 60000;

            int value = -1;
            try
            {


                if (ioService != null)
                {
                    if (typeof(T) == typeof(bool))
                    {
                        value = ioService.MonitorForIO((IOPortDescripter<bool>)Convert.ChangeType(this, typeof(IOPortDescripter<bool>)), HL, MaintainTime.Value, TimeOut.Value);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return value;
        }
        public void LOCK()
        {
            this.LockState = IOLockState.LOCK;
        }
        public void UNLOCK()
        {
            this.LockState = IOLockState.UNLOCK;
        }
        public void SetService(IIOService serv)
        {
            ioService = serv;
        }
     
        private bool _Value;
        [XmlIgnore, JsonIgnore]
        [DataMember]
        public bool Value
        {
            get { return _Value; }
            set
            {
                if (value == _Value)
                {
                }
                else
                {
                    _Value = value;
                    NotifyPropertyChanged("Value");
                }
            }
        }
        public string GetChannelDescipterString()
        {
            if (_IOOveride.Value == EnumIOOverride.NONE)
            {
                return " Channel=" + ChannelIndex + " Port=" + PortIndex;
            }
            return _IOOveride.ToString();
        }
    }

    [Serializable]
    public class InputPortDefinitions : INotifyPropertyChanged, IParamNode
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

        public InputPortDefinitions()
        {
        }

        #region Loader Inputs
        private IOPortDescripter<bool> _DIMAINAIR = new IOPortDescripter<bool>(nameof(DIMAINAIR), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIMAINAIR
        {
            get { return _DIMAINAIR; }
            set
            {
                if (value != this._DIMAINAIR)
                {
                    _DIMAINAIR = value;
                    NotifyPropertyChanged(nameof(DIMAINAIR));
                }
            }
        }
        private IOPortDescripter<bool> _DIMAINVAC = new IOPortDescripter<bool>(nameof(DIMAINVAC), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIMAINVAC
        {
            get { return _DIMAINVAC; }
            set
            {
                if (value != this._DIMAINVAC)
                {
                    _DIMAINVAC = value;
                    NotifyPropertyChanged(nameof(DIMAINVAC));
                }
            }
        }
        private IOPortDescripter<bool> _DIARM1VAC = new IOPortDescripter<bool>(nameof(DIARM1VAC), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIARM1VAC
        {
            get { return _DIARM1VAC; }
            set
            {
                if (value != this._DIARM1VAC)
                {
                    _DIARM1VAC = value;
                    NotifyPropertyChanged(nameof(DIARM1VAC));
                }
            }
        }
        private IOPortDescripter<bool> _DIARM2VAC = new IOPortDescripter<bool>(nameof(DIARM2VAC), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIARM2VAC
        {
            get { return _DIARM2VAC; }
            set
            {
                if (value != this._DIARM2VAC)
                {
                    _DIARM2VAC = value;
                    NotifyPropertyChanged(nameof(DIARM2VAC));
                }
            }
        }

        private IOPortDescripter<bool> _DIDOORCLOSE = new IOPortDescripter<bool>(nameof(DIDOORCLOSE), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIDOORCLOSE
        {
            get { return _DIDOORCLOSE; }
            set
            {
                if (value != this._DIDOORCLOSE)
                {
                    _DIDOORCLOSE = value;
                    NotifyPropertyChanged(nameof(DIDOORCLOSE));
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERSENSOR = new IOPortDescripter<bool>(nameof(DIWAFERSENSOR), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWAFERSENSOR
        {
            get { return _DIWAFERSENSOR; }
            set
            {
                if (value != this._DIWAFERSENSOR)
                {
                    _DIWAFERSENSOR = value;
                    NotifyPropertyChanged(nameof(DIWAFERSENSOR));
                }
            }
        }
        private IOPortDescripter<bool> _DI8PRS1 = new IOPortDescripter<bool>(nameof(DI8PRS1), EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8PRS1
        {
            get { return _DI8PRS1; }
            set
            {
                if (value != this._DI8PRS1)
                {
                    _DI8PRS1 = value;
                    NotifyPropertyChanged(nameof(DI8PRS1));
                }
            }
        }
        private IOPortDescripter<bool> _DI8PRS2 = new IOPortDescripter<bool>(nameof(DI8PRS2), EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8PRS2
        {
            get { return _DI8PRS2; }
            set
            {
                if (value != this._DI8PRS2)
                {
                    _DI8PRS2 = value;
                    NotifyPropertyChanged(nameof(DI8PRS2));
                }
            }
        }

        private IOPortDescripter<bool> _DI8PRS3 = new IOPortDescripter<bool>(nameof(DI8PRS3), EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8PRS3
        {
            get { return _DI8PRS3; }
            set
            {
                if (value != this._DI8PRS3)
                {
                    _DI8PRS3 = value;
                    NotifyPropertyChanged(nameof(DI8PRS3));
                }
            }
        }
        private IOPortDescripter<bool> _DI6PLA = new IOPortDescripter<bool>(nameof(DI6PLA), EnumIOType.INPUT);
        public IOPortDescripter<bool> DI6PLA
        {
            get { return _DI6PLA; }
            set
            {
                if (value != this._DI6PLA)
                {
                    _DI6PLA = value;
                    NotifyPropertyChanged(nameof(DI6PLA));
                }
            }
        }

        private IOPortDescripter<bool> _DI8PLA = new IOPortDescripter<bool>(nameof(DI8PLA), EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8PLA
        {
            get { return _DI8PLA; }
            set
            {
                if (value != this._DI8PLA)
                {
                    _DI8PLA = value;
                    NotifyPropertyChanged(nameof(DI8PLA));
                }
            }
        }

        private IOPortDescripter<bool> _DIFOUPSWINGSENSOR = new IOPortDescripter<bool>(nameof(DIFOUPSWINGSENSOR), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFOUPSWINGSENSOR
        {
            get { return _DIFOUPSWINGSENSOR; }
            set
            {
                if (value != this._DIFOUPSWINGSENSOR)
                {
                    _DIFOUPSWINGSENSOR = value;
                    NotifyPropertyChanged(nameof(DIFOUPSWINGSENSOR));
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERDETECTSENSOR = new IOPortDescripter<bool>(nameof(DIWAFERDETECTSENSOR), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWAFERDETECTSENSOR
        {
            get { return _DIWAFERDETECTSENSOR; }
            set
            {
                if (value != this._DIWAFERDETECTSENSOR)
                {
                    _DIWAFERDETECTSENSOR = value;
                    NotifyPropertyChanged(nameof(DIWAFERDETECTSENSOR));
                }
            }
        }
        private IOPortDescripter<bool> _DISCANMAPPINGSENSOR = new IOPortDescripter<bool>(nameof(DISCANMAPPINGSENSOR), EnumIOType.INPUT);
        public IOPortDescripter<bool> DISCANMAPPINGSENSOR
        {
            get { return _DISCANMAPPINGSENSOR; }
            set
            {
                if (value != this._DISCANMAPPINGSENSOR)
                {
                    _DISCANMAPPINGSENSOR = value;
                    NotifyPropertyChanged(nameof(DISCANMAPPINGSENSOR));
                }
            }
        }
        private IOPortDescripter<bool> _DITRAYDETECTSENSOR = new IOPortDescripter<bool>(nameof(DITRAYDETECTSENSOR), EnumIOType.INPUT);
        public IOPortDescripter<bool> DITRAYDETECTSENSOR
        {
            get { return _DITRAYDETECTSENSOR; }
            set
            {
                if (value != this._DITRAYDETECTSENSOR)
                {
                    _DITRAYDETECTSENSOR = value;
                    NotifyPropertyChanged(nameof(DITRAYDETECTSENSOR));
                }
            }
        }

        private IOPortDescripter<bool> _DISUBCHUCKVACSENSOR = new IOPortDescripter<bool>(nameof(DISUBCHUCKVACSENSOR), EnumIOType.INPUT);
        public IOPortDescripter<bool> DISUBCHUCKVACSENSOR
        {
            get { return _DISUBCHUCKVACSENSOR; }
            set
            {
                if (value != this._DISUBCHUCKVACSENSOR)
                {
                    _DISUBCHUCKVACSENSOR = value;
                    NotifyPropertyChanged(nameof(DISUBCHUCKVACSENSOR));
                }
            }
        }



        private IOPortDescripter<bool> _DICLEANUNITVAC = new IOPortDescripter<bool>(nameof(DICLEANUNITVAC), EnumIOType.INPUT);
        public IOPortDescripter<bool> DICLEANUNITVAC
        {
            get { return _DICLEANUNITVAC; }
            set
            {
                if (value != this._DICLEANUNITVAC)
                {
                    _DICLEANUNITVAC = value;
                    NotifyPropertyChanged(nameof(DICLEANUNITVAC));
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERONARM = new IOPortDescripter<bool>(nameof(DIWAFERONARM), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWAFERONARM
        {
            get { return _DIWAFERONARM; }
            set
            {
                if (value != this._DIWAFERONARM)
                {
                    _DIWAFERONARM = value;
                    NotifyPropertyChanged(nameof(DIWAFERONARM));
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERONARM2 = new IOPortDescripter<bool>(nameof(DIWAFERONARM2), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWAFERONARM2
        {
            get { return _DIWAFERONARM2; }
            set
            {
                if (value != this._DIWAFERONARM2)
                {
                    _DIWAFERONARM2 = value;
                    NotifyPropertyChanged(nameof(DIWAFERONARM2));
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERONSUBCHUCK = new IOPortDescripter<bool>(nameof(DIWAFERONSUBCHUCK), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWAFERONSUBCHUCK
        {
            get { return _DIWAFERONSUBCHUCK; }
            set
            {
                if (value != this._DIWAFERONSUBCHUCK)
                {
                    _DIWAFERONSUBCHUCK = value;
                    NotifyPropertyChanged(nameof(DIWAFERONSUBCHUCK));
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERONDRAWER = new IOPortDescripter<bool>(nameof(DIWAFERONDRAWER), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWAFERONDRAWER
        {
            get { return _DIWAFERONDRAWER; }
            set
            {
                if (value != this._DIWAFERONDRAWER)
                {
                    _DIWAFERONDRAWER = value;
                    NotifyPropertyChanged(nameof(DIWAFERONDRAWER));
                }
            }
        }

        private IOPortDescripter<bool> _DIDRAWEROPEN = new IOPortDescripter<bool>(nameof(DIDRAWEROPEN), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIDRAWEROPEN
        {
            get { return _DIDRAWEROPEN; }
            set
            {
                if (value != this._DIDRAWEROPEN)
                {
                    _DIDRAWEROPEN = value;
                    NotifyPropertyChanged(nameof(DIDRAWEROPEN));
                }
            }
        }

        private IOPortDescripter<bool> _DIDRAWEREMOVED = new IOPortDescripter<bool>(nameof(DIDRAWEREMOVED), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIDRAWEREMOVED
        {
            get { return _DIDRAWEREMOVED; }
            set
            {
                if (value != this._DIDRAWEREMOVED)
                {
                    _DIDRAWEREMOVED = value;
                    NotifyPropertyChanged(nameof(DIDRAWEREMOVED));
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERONFIXEDTRAY0 = new IOPortDescripter<bool>(nameof(DIWAFERONFIXEDTRAY0), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWAFERONFIXEDTRAY0
        {
            get { return _DIWAFERONFIXEDTRAY0; }
            set
            {
                if (value != this._DIWAFERONFIXEDTRAY0)
                {
                    _DIWAFERONFIXEDTRAY0 = value;
                    NotifyPropertyChanged(nameof(DIWAFERONFIXEDTRAY0));
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERONFIXEDTRAY1 = new IOPortDescripter<bool>(nameof(DIWAFERONFIXEDTRAY1), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWAFERONFIXEDTRAY1
        {
            get { return _DIWAFERONFIXEDTRAY1; }
            set
            {
                if (value != this._DIWAFERONFIXEDTRAY1)
                {
                    _DIWAFERONFIXEDTRAY1 = value;
                    NotifyPropertyChanged(nameof(DIWAFERONFIXEDTRAY1));
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERONFIXEDTRAY2 = new IOPortDescripter<bool>(nameof(DIWAFERONFIXEDTRAY2), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWAFERONFIXEDTRAY2
        {
            get { return _DIWAFERONFIXEDTRAY2; }
            set
            {
                if (value != this._DIWAFERONFIXEDTRAY2)
                {
                    _DIWAFERONFIXEDTRAY2 = value;
                    NotifyPropertyChanged(nameof(DIWAFERONFIXEDTRAY2));
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERONFIXEDTRAY3 = new IOPortDescripter<bool>(nameof(DIWAFERONFIXEDTRAY3), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWAFERONFIXEDTRAY3
        {
            get { return _DIWAFERONFIXEDTRAY3; }
            set
            {
                if (value != this._DIWAFERONFIXEDTRAY3)
                {
                    _DIWAFERONFIXEDTRAY3 = value;
                    NotifyPropertyChanged(nameof(DIWAFERONFIXEDTRAY3));
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERONFIXEDTRAY4 = new IOPortDescripter<bool>(nameof(DIWAFERONFIXEDTRAY4), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWAFERONFIXEDTRAY4
        {
            get { return _DIWAFERONFIXEDTRAY4; }
            set
            {
                if (value != this._DIWAFERONFIXEDTRAY4)
                {
                    _DIWAFERONFIXEDTRAY4 = value;
                    NotifyPropertyChanged(nameof(DIWAFERONFIXEDTRAY4));
                }
            }
        }

        private IOPortDescripter<bool> _DIARMIN = new IOPortDescripter<bool>(nameof(DIARMIN), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIARMIN
        {
            get { return _DIARMIN; }
            set
            {
                if (value != this._DIARMIN)
                {
                    _DIARMIN = value;
                    NotifyPropertyChanged(nameof(DIARMIN));
                }
            }
        }

        private IOPortDescripter<bool> _DIARMOUT = new IOPortDescripter<bool>(nameof(DIARMOUT), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIARMOUT
        {
            get { return _DIARMOUT; }
            set
            {
                if (value != this._DIARMOUT)
                {
                    _DIARMOUT = value;
                    NotifyPropertyChanged(nameof(DIARMOUT));
                }
            }
        }

        private IOPortDescripter<bool> _DIFOUP_COVER_LOCK = new IOPortDescripter<bool>(nameof(DIFOUP_COVER_LOCK), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFOUP_COVER_LOCK
        {
            get { return _DIFOUP_COVER_LOCK; }
            set
            {
                if (value != this._DIFOUP_COVER_LOCK)
                {
                    _DIFOUP_COVER_LOCK = value;
                    NotifyPropertyChanged(nameof(DIFOUP_COVER_LOCK));
                }
            }
        }

        private IOPortDescripter<bool> _DIFOUP_COVER_UNLOCK = new IOPortDescripter<bool>(nameof(DIFOUP_COVER_UNLOCK), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFOUP_COVER_UNLOCK
        {
            get { return _DIFOUP_COVER_UNLOCK; }
            set
            {
                if (value != this._DIFOUP_COVER_UNLOCK)
                {
                    _DIFOUP_COVER_UNLOCK = value;
                    NotifyPropertyChanged(nameof(DIFOUP_COVER_UNLOCK));
                }
            }
        }

        private IOPortDescripter<bool> _DIFRAMEDONSUBCHUCK = new IOPortDescripter<bool>(nameof(DIFRAMEDONSUBCHUCK), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFRAMEDONSUBCHUCK
        {
            get { return _DIFRAMEDONSUBCHUCK; }
            set
            {
                if (value != this._DIFRAMEDONSUBCHUCK)
                {
                    _DIFRAMEDONSUBCHUCK = value;
                    NotifyPropertyChanged(nameof(DIFRAMEDONSUBCHUCK));
                }
            }
        }

        private IOPortDescripter<bool> _DIFRAMEDPREUP = new IOPortDescripter<bool>(nameof(DIFRAMEDPREUP), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFRAMEDPREUP
        {
            get { return _DIFRAMEDPREUP; }
            set
            {
                if (value != this._DIFRAMEDPREUP)
                {
                    _DIFRAMEDPREUP = value;
                    NotifyPropertyChanged(nameof(DIFRAMEDPREUP));
                }
            }
        }

        private IOPortDescripter<bool> _DIFRAMEDPREDN = new IOPortDescripter<bool>(nameof(DIFRAMEDPREDN), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFRAMEDPREDN
        {
            get { return _DIFRAMEDPREDN; }
            set
            {
                if (value != this._DIFRAMEDPREDN)
                {
                    _DIFRAMEDPREDN = value;
                    NotifyPropertyChanged(nameof(DIFRAMEDPREDN));
                }
            }
        }

        private IOPortDescripter<bool> _DIFRAMEDPREIN = new IOPortDescripter<bool>(nameof(DIFRAMEDPREIN), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFRAMEDPREIN
        {
            get { return _DIFRAMEDPREIN; }
            set
            {
                if (value != this._DIFRAMEDPREIN)
                {
                    _DIFRAMEDPREIN = value;
                    NotifyPropertyChanged(nameof(DIFRAMEDPREIN));
                }
            }
        }

        private IOPortDescripter<bool> _DIFRAMEDPREOT = new IOPortDescripter<bool>(nameof(DIFRAMEDPREOT), EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFRAMEDPREOT
        {
            get { return _DIFRAMEDPREOT; }
            set
            {
                if (value != this._DIFRAMEDPREOT)
                {
                    _DIFRAMEDPREOT = value;
                    NotifyPropertyChanged(nameof(DIFRAMEDPREOT));
                }
            }
        }

        private IOPortDescripter<bool> _DISPARE0 = new IOPortDescripter<bool>(nameof(DISPARE0), EnumIOType.INPUT);
        public IOPortDescripter<bool> DISPARE0
        {
            get { return _DISPARE0; }
            set
            {
                if (value != this._DISPARE0)
                {
                    _DISPARE0 = value;
                    NotifyPropertyChanged(nameof(DISPARE0));
                }
            }
        }

        private IOPortDescripter<bool> _DISPARE1 = new IOPortDescripter<bool>(nameof(DISPARE1), EnumIOType.INPUT);
        public IOPortDescripter<bool> DISPARE1
        {
            get { return _DISPARE1; }
            set
            {
                if (value != this._DISPARE1)
                {
                    _DISPARE1 = value;
                    NotifyPropertyChanged(nameof(DISPARE1));
                }
            }
        }
        #endregion

        #region // Stage Inputs

        private IOPortDescripter<bool> _DICARDDOOR_CLOSE = new IOPortDescripter<bool>(nameof(DICARDDOOR_CLOSE), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DICARDDOOR_CLOSE
        {
            get { return _DICARDDOOR_CLOSE; }
            set
            {
                if (value != this._DICARDDOOR_CLOSE)
                {
                    _DICARDDOOR_CLOSE = value;
                    NotifyPropertyChanged(nameof(DICARDDOOR_CLOSE));
                }
            }
        }

        private IOPortDescripter<bool> _DICARDDOOR_OPEN = new IOPortDescripter<bool>(nameof(DICARDDOOR_OPEN), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DICARDDOOR_OPEN
        {
            get { return _DICARDDOOR_OPEN; }
            set
            {
                if (value != this._DICARDDOOR_OPEN)
                {
                    _DICARDDOOR_OPEN = value;
                    NotifyPropertyChanged(nameof(DICARDDOOR_OPEN));
                }
            }
        }

        private IOPortDescripter<bool> _DILOADERDOOR_CLOSE = new IOPortDescripter<bool>(nameof(DILOADERDOOR_CLOSE), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DILOADERDOOR_CLOSE
        {
            get { return _DILOADERDOOR_CLOSE; }
            set
            {
                if (value != this._DILOADERDOOR_CLOSE)
                {
                    _DILOADERDOOR_CLOSE = value;
                    NotifyPropertyChanged(nameof(DILOADERDOOR_CLOSE));
                }
            }
        }

        private IOPortDescripter<bool> _DILOADERDOOR_OPEN = new IOPortDescripter<bool>(nameof(DILOADERDOOR_OPEN), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DILOADERDOOR_OPEN
        {
            get { return _DILOADERDOOR_OPEN; }
            set
            {
                if (value != this._DILOADERDOOR_OPEN)
                {
                    _DILOADERDOOR_OPEN = value;
                    NotifyPropertyChanged(nameof(DILOADERDOOR_OPEN));
                }
            }
        }

        private IOPortDescripter<bool> _DILOADERDOOR_SEAL_CLOSE = new IOPortDescripter<bool>(nameof(DILOADERDOOR_SEAL_CLOSE), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DILOADERDOOR_SEAL_CLOSE
        {
            get { return _DILOADERDOOR_SEAL_CLOSE; }
            set
            {
                if (value != this._DILOADERDOOR_SEAL_CLOSE)
                {
                    _DILOADERDOOR_SEAL_CLOSE = value;
                    NotifyPropertyChanged(nameof(DILOADERDOOR_SEAL_CLOSE));
                }
            }
        }

        private IOPortDescripter<bool> _DILOADERDOOR_SEAL_OPEN = new IOPortDescripter<bool>(nameof(DILOADERDOOR_SEAL_OPEN), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DILOADERDOOR_SEAL_OPEN
        {
            get { return _DILOADERDOOR_SEAL_OPEN; }
            set
            {
                if (value != this._DILOADERDOOR_SEAL_OPEN)
                {
                    _DILOADERDOOR_SEAL_OPEN = value;
                    NotifyPropertyChanged(nameof(DILOADERDOOR_SEAL_OPEN));
                }
            }
        }
        private IOPortDescripter<bool> _DICARDDOOR_SEAL_CLOSE = new IOPortDescripter<bool>(nameof(DICARDDOOR_SEAL_CLOSE), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DICARDDOOR_SEAL_CLOSE
        {
            get { return _DICARDDOOR_SEAL_CLOSE; }
            set
            {
                if (value != this._DICARDDOOR_SEAL_CLOSE)
                {
                    _DICARDDOOR_SEAL_CLOSE = value;
                    NotifyPropertyChanged(nameof(DICARDDOOR_SEAL_CLOSE));
                }
            }
        }

        private IOPortDescripter<bool> _DICARDDOOR_SEAL_OPEN = new IOPortDescripter<bool>(nameof(DICARDDOOR_SEAL_OPEN), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DICARDDOOR_SEAL_OPEN
        {
            get { return _DICARDDOOR_SEAL_OPEN; }
            set
            {
                if (value != this._DICARDDOOR_SEAL_OPEN)
                {
                    _DICARDDOOR_SEAL_OPEN = value;
                    NotifyPropertyChanged(nameof(DICARDDOOR_SEAL_OPEN));
                }
            }
        }

        private IOPortDescripter<bool> _DITOPPLATE_OPEN = new IOPortDescripter<bool>(nameof(DITOPPLATE_OPEN), EnumIOType.INPUT);

        public IOPortDescripter<bool> DITOPPLATE_OPEN
        {
            get { return _DITOPPLATE_OPEN; }
            set
            {
                if (value != this._DITOPPLATE_OPEN)
                {
                    _DITOPPLATE_OPEN = value;
                    NotifyPropertyChanged(nameof(DITOPPLATE_OPEN));
                }
            }
        }

        private IOPortDescripter<bool> _DITOPPLATE_CLOSE = new IOPortDescripter<bool>(nameof(DITOPPLATE_CLOSE), EnumIOType.INPUT);

        public IOPortDescripter<bool> DITOPPLATE_CLOSE
        {
            get { return _DITOPPLATE_CLOSE; }
            set
            {
                if (value != this._DITOPPLATE_CLOSE)
                {
                    _DITOPPLATE_CLOSE = value;
                    NotifyPropertyChanged(nameof(DITOPPLATE_CLOSE));
                }
            }
        }

        private IOPortDescripter<bool> _DIFRONTDOOR_LOCK_R = new IOPortDescripter<bool>(nameof(DIFRONTDOOR_LOCK_R), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIFRONTDOOR_LOCK_R
        {
            get { return _DIFRONTDOOR_LOCK_R; }
            set
            {
                if (value != this._DIFRONTDOOR_LOCK_R)
                {
                    _DIFRONTDOOR_LOCK_R = value;
                    NotifyPropertyChanged(nameof(DIFRONTDOOR_LOCK_R));
                }
            }
        }

        private IOPortDescripter<bool> _DIFRONTDOOR_UNLOCK_R = new IOPortDescripter<bool>(nameof(DIFRONTDOOR_UNLOCK_R), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIFRONTDOOR_UNLOCK_R
        {
            get { return _DIFRONTDOOR_UNLOCK_R; }
            set
            {
                if (value != this._DIFRONTDOOR_UNLOCK_R)
                {
                    _DIFRONTDOOR_UNLOCK_R = value;
                    NotifyPropertyChanged(nameof(DIFRONTDOOR_UNLOCK_R));
                }
            }
        }

        private IOPortDescripter<bool> _DIFRONTDOOR_LOCK_L = new IOPortDescripter<bool>(nameof(DIFRONTDOOR_LOCK_L), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIFRONTDOOR_LOCK_L
        {
            get { return _DIFRONTDOOR_LOCK_L; }
            set
            {
                if (value != this._DIFRONTDOOR_LOCK_L)
                {
                    _DIFRONTDOOR_LOCK_L = value;
                    NotifyPropertyChanged(nameof(DIFRONTDOOR_LOCK_L));
                }
            }
        }

        private IOPortDescripter<bool> _DIFRONTDOOR_UNLOCK_L = new IOPortDescripter<bool>(nameof(DIFRONTDOOR_UNLOCK_L), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIFRONTDOOR_UNLOCK_L
        {
            get { return _DIFRONTDOOR_UNLOCK_L; }
            set
            {
                if (value != this._DIFRONTDOOR_UNLOCK_L)
                {
                    _DIFRONTDOOR_UNLOCK_L = value;
                    NotifyPropertyChanged(nameof(DIFRONTDOOR_UNLOCK_L));
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERCAMMIDDLE = new IOPortDescripter<bool>(nameof(DIWAFERCAMMIDDLE), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIWAFERCAMMIDDLE
        {
            get { return _DIWAFERCAMMIDDLE; }
            set
            {
                if (value != this._DIWAFERCAMMIDDLE)
                {
                    _DIWAFERCAMMIDDLE = value;
                    NotifyPropertyChanged(nameof(DIWAFERCAMMIDDLE));
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERCAMREAR = new IOPortDescripter<bool>(nameof(DIWAFERCAMREAR), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIWAFERCAMREAR
        {
            get { return _DIWAFERCAMREAR; }
            set
            {
                if (value != this._DIWAFERCAMREAR)
                {
                    _DIWAFERCAMREAR = value;
                    NotifyPropertyChanged(nameof(DIWAFERCAMREAR));
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERONCHUCK = new IOPortDescripter<bool>(nameof(DIWAFERONCHUCK), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIWAFERONCHUCK
        {
            get { return _DIWAFERONCHUCK; }
            set
            {
                if (value != this._DIWAFERONCHUCK)
                {
                    _DIWAFERONCHUCK = value;
                    NotifyPropertyChanged(nameof(DIWAFERONCHUCK));
                }
            }
        }

        private IOPortDescripter<bool> _DIINKERDOWN_0 = new IOPortDescripter<bool>(nameof(DIINKERDOWN_0), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIINKERDOWN_0
        {
            get { return _DIINKERDOWN_0; }
            set
            {
                if (value != this._DIINKERDOWN_0)
                {
                    _DIINKERDOWN_0 = value;
                    NotifyPropertyChanged(nameof(DIINKERDOWN_0));
                }
            }
        }
        private IOPortDescripter<bool> _DIINKERDOWN_1 = new IOPortDescripter<bool>(nameof(DIINKERDOWN_1), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIINKERDOWN_1
        {
            get { return _DIINKERDOWN_1; }
            set
            {
                if (value != this._DIINKERDOWN_1)
                {
                    _DIINKERDOWN_1 = value;
                    NotifyPropertyChanged(nameof(DIINKERDOWN_1));
                }
            }
        }
        private IOPortDescripter<bool> _DIHOMEII_0 = new IOPortDescripter<bool>(nameof(DIHOMEII_0), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIHOMEII_0
        {
            get { return _DIHOMEII_0; }
            set
            {
                if (value != this._DIHOMEII_0)
                {
                    _DIHOMEII_0 = value;
                    NotifyPropertyChanged(nameof(DIHOMEII_0));
                }
            }
        }
        private IOPortDescripter<bool> _DIHOMEII_1 = new IOPortDescripter<bool>(nameof(DIHOMEII_1), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIHOMEII_1
        {
            get { return _DIHOMEII_1; }
            set
            {
                if (value != this._DIHOMEII_1)
                {
                    _DIHOMEII_1 = value;
                    NotifyPropertyChanged(nameof(DIHOMEII_1));
                }
            }
        }
        private IOPortDescripter<bool> _DITHREELEGUP_0 = new IOPortDescripter<bool>(nameof(DITHREELEGUP_0), EnumIOType.INPUT);

        public IOPortDescripter<bool> DITHREELEGUP_0
        {
            get { return _DITHREELEGUP_0; }
            set
            {
                if (value != this._DITHREELEGUP_0)
                {
                    _DITHREELEGUP_0 = value;
                    NotifyPropertyChanged(nameof(DITHREELEGUP_0));
                }
            }
        }
        private IOPortDescripter<bool> _DITHREELEGUP_1 = new IOPortDescripter<bool>(nameof(DITHREELEGUP_1), EnumIOType.INPUT);

        public IOPortDescripter<bool> DITHREELEGUP_1
        {
            get { return _DITHREELEGUP_1; }
            set
            {
                if (value != this._DITHREELEGUP_1)
                {
                    _DITHREELEGUP_1 = value;
                    NotifyPropertyChanged(nameof(DITHREELEGUP_1));
                }
            }
        }
        private IOPortDescripter<bool> _DICLEANUNITUP_0 = new IOPortDescripter<bool>(nameof(DICLEANUNITUP_0), EnumIOType.INPUT);

        public IOPortDescripter<bool> DICLEANUNITUP_0
        {
            get { return _DICLEANUNITUP_0; }
            set
            {
                if (value != this._DICLEANUNITUP_0)
                {
                    _DICLEANUNITUP_0 = value;
                    NotifyPropertyChanged(nameof(DICLEANUNITUP_0));
                }
            }
        }
        private IOPortDescripter<bool> _DICLEANUNITUP_1 = new IOPortDescripter<bool>(nameof(DICLEANUNITUP_1), EnumIOType.INPUT);

        public IOPortDescripter<bool> DICLEANUNITUP_1
        {
            get { return _DICLEANUNITUP_1; }
            set
            {
                if (value != this._DICLEANUNITUP_1)
                {
                    _DICLEANUNITUP_1 = value;
                    NotifyPropertyChanged(nameof(DICLEANUNITUP_1));
                }
            }
        }
        private IOPortDescripter<bool> _DICARDHOLDEROPEN = new IOPortDescripter<bool>(nameof(DICARDHOLDEROPEN), EnumIOType.INPUT);

        public IOPortDescripter<bool> DICARDHOLDEROPEN
        {
            get { return _DICARDHOLDEROPEN; }
            set
            {
                if (value != this._DICARDHOLDEROPEN)
                {
                    _DICARDHOLDEROPEN = value;
                    NotifyPropertyChanged(nameof(DICARDHOLDEROPEN));
                }
            }
        }
        private IOPortDescripter<bool> _DICARDHOLDERCLOSE = new IOPortDescripter<bool>(nameof(DICARDHOLDERCLOSE), EnumIOType.INPUT);

        public IOPortDescripter<bool> DICARDHOLDERCLOSE
        {
            get { return _DICARDHOLDERCLOSE; }
            set
            {
                if (value != this._DICARDHOLDERCLOSE)
                {
                    _DICARDHOLDERCLOSE = value;
                    NotifyPropertyChanged(nameof(DICARDHOLDERCLOSE));
                }
            }
        }
        private IOPortDescripter<bool> _DICARDHOLDERUP = new IOPortDescripter<bool>(nameof(DICARDHOLDERUP), EnumIOType.INPUT);

        public IOPortDescripter<bool> DICARDHOLDERUP
        {
            get { return _DICARDHOLDERUP; }
            set
            {
                if (value != this._DICARDHOLDERUP)
                {
                    _DICARDHOLDERUP = value;
                    NotifyPropertyChanged(nameof(DICARDHOLDERUP));
                }
            }
        }

        private IOPortDescripter<bool> _DICSTSIZE_0 = new IOPortDescripter<bool>(nameof(DICSTSIZE_0), EnumIOType.INPUT);

        public IOPortDescripter<bool> DICSTSIZE_0
        {
            get { return _DICSTSIZE_0; }
            set
            {
                if (value != this._DICSTSIZE_0)
                {
                    _DICSTSIZE_0 = value;
                    NotifyPropertyChanged(nameof(DICSTSIZE_0));
                }
            }
        }
        private IOPortDescripter<bool> _DICSTSIZE_1 = new IOPortDescripter<bool>(nameof(DICSTSIZE_1), EnumIOType.INPUT);

        public IOPortDescripter<bool> DICSTSIZE_1
        {
            get { return _DICSTSIZE_1; }
            set
            {
                if (value != this._DICSTSIZE_1)
                {
                    _DICSTSIZE_1 = value;
                    NotifyPropertyChanged(nameof(DICSTSIZE_1));
                }
            }
        }
        private IOPortDescripter<bool> _DICSTSIZE_2 = new IOPortDescripter<bool>(nameof(DICSTSIZE_2), EnumIOType.INPUT);

        public IOPortDescripter<bool> DICSTSIZE_2
        {
            get { return _DICSTSIZE_2; }
            set
            {
                if (value != this._DICSTSIZE_2)
                {
                    _DICSTSIZE_2 = value;
                    NotifyPropertyChanged(nameof(DICSTSIZE_2));
                }
            }
        }
        private IOPortDescripter<bool> _DIEMGSTOPSW = new IOPortDescripter<bool>(nameof(DIEMGSTOPSW), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIEMGSTOPSW
        {
            get { return _DIEMGSTOPSW; }
            set
            {
                if (value != this._DIEMGSTOPSW)
                {
                    _DIEMGSTOPSW = value;
                    NotifyPropertyChanged(nameof(DIEMGSTOPSW));
                }
            }
        }
        private IOPortDescripter<bool> _DICLEANWAFERONDRAWER = new IOPortDescripter<bool>(nameof(DICLEANWAFERONDRAWER), EnumIOType.INPUT);

        public IOPortDescripter<bool> DICLEANWAFERONDRAWER
        {
            get { return _DICLEANWAFERONDRAWER; }
            set
            {
                if (value != this._DICLEANWAFERONDRAWER)
                {
                    _DICLEANWAFERONDRAWER = value;
                    NotifyPropertyChanged(nameof(DICLEANWAFERONDRAWER));
                }
            }
        }

        private IOPortDescripter<bool> _DICARDHOLDERLOCK = new IOPortDescripter<bool>(nameof(DICARDHOLDERLOCK), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICARDHOLDERLOCK
        {
            get { return _DICARDHOLDERLOCK; }
            set
            {
                if (value != this._DICARDHOLDERLOCK)
                {
                    _DICARDHOLDERLOCK = value;
                    NotifyPropertyChanged(nameof(DICARDHOLDERLOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DICSTPORTCLOSE = new IOPortDescripter<bool>(nameof(DICSTPORTCLOSE), EnumIOType.INPUT);

        public IOPortDescripter<bool> DICSTPORTCLOSE
        {
            get { return _DICSTPORTCLOSE; }
            set
            {
                if (value != this._DICSTPORTCLOSE)
                {
                    _DICSTPORTCLOSE = value;
                    NotifyPropertyChanged(nameof(DICSTPORTCLOSE));
                }
            }
        }
        private IOPortDescripter<bool> _DIFRONTDOORCLOSE = new IOPortDescripter<bool>(nameof(DIFRONTDOORCLOSE), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIFRONTDOORCLOSE
        {
            get { return _DIFRONTDOORCLOSE; }
            set
            {
                if (value != this._DIFRONTDOORCLOSE)
                {
                    _DIFRONTDOORCLOSE = value;
                    NotifyPropertyChanged(nameof(DIFRONTDOORCLOSE));
                }
            }
        }
        private IOPortDescripter<bool> _DIWAFERSLOTOUT = new IOPortDescripter<bool>(nameof(DIWAFERSLOTOUT), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIWAFERSLOTOUT
        {
            get { return _DIWAFERSLOTOUT; }
            set
            {
                if (value != this._DIWAFERSLOTOUT)
                {
                    _DIWAFERSLOTOUT = value;
                    NotifyPropertyChanged(nameof(DIWAFERSLOTOUT));
                }
            }
        }
        private IOPortDescripter<bool> _DICH_ROT_OPEN = new IOPortDescripter<bool>(nameof(DICH_ROT_OPEN), EnumIOType.INPUT);

        public IOPortDescripter<bool> DICH_ROT_OPEN
        {
            get { return _DICH_ROT_OPEN; }
            set
            {
                if (value != this._DICH_ROT_OPEN)
                {
                    _DICH_ROT_OPEN = value;
                    NotifyPropertyChanged(nameof(DICH_ROT_OPEN));
                }
            }
        }
        private IOPortDescripter<bool> _DICH_ROT_CLOSE = new IOPortDescripter<bool>(nameof(DICH_ROT_CLOSE), EnumIOType.INPUT);

        public IOPortDescripter<bool> DICH_ROT_CLOSE
        {
            get { return _DICH_ROT_CLOSE; }
            set
            {
                if (value != this._DICH_ROT_CLOSE)
                {
                    _DICH_ROT_CLOSE = value;
                    NotifyPropertyChanged(nameof(DICH_ROT_CLOSE));
                }
            }
        }
        private IOPortDescripter<bool> _DICH_TUB_OPEN = new IOPortDescripter<bool>(nameof(DICH_TUB_OPEN), EnumIOType.INPUT);

        public IOPortDescripter<bool> DICH_TUB_OPEN
        {
            get { return _DICH_TUB_OPEN; }
            set
            {
                if (value != this._DICH_TUB_OPEN)
                {
                    _DICH_TUB_OPEN = value;
                    NotifyPropertyChanged(nameof(DICH_TUB_OPEN));
                }
            }
        }
        private IOPortDescripter<bool> _DICH_TUB_CLOSE = new IOPortDescripter<bool>(nameof(DICH_TUB_CLOSE), EnumIOType.INPUT);

        public IOPortDescripter<bool> DICH_TUB_CLOSE
        {
            get { return _DICH_TUB_CLOSE; }
            set
            {
                if (value != this._DICH_TUB_CLOSE)
                {
                    _DICH_TUB_CLOSE = value;
                    NotifyPropertyChanged(nameof(DICH_TUB_CLOSE));
                }
            }
        }

        private IOPortDescripter<bool> _DICH_CARRIER_UP = new IOPortDescripter<bool>(nameof(DICH_CARRIER_UP), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICH_CARRIER_UP
        {
            get { return _DICH_CARRIER_UP; }
            set
            {
                if (value != this._DICH_CARRIER_UP)
                {
                    _DICH_CARRIER_UP = value;
                    NotifyPropertyChanged(nameof(DICH_CARRIER_UP));
                }
            }
        }
        private IOPortDescripter<bool> _DIPOWER_DOWN = new IOPortDescripter<bool>(nameof(DIPOWER_DOWN), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIPOWER_DOWN
        {
            get { return _DIPOWER_DOWN; }
            set
            {
                if (value != this._DIPOWER_DOWN)
                {
                    _DIPOWER_DOWN = value;
                    NotifyPropertyChanged(nameof(DIPOWER_DOWN));
                }
            }
        }
        private IOPortDescripter<bool> _DIMANIPULATORREADY = new IOPortDescripter<bool>(nameof(DIMANIPULATORREADY), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIMANIPULATORREADY
        {
            get { return _DIMANIPULATORREADY; }
            set
            {
                if (value != this._DIMANIPULATORREADY)
                {
                    _DIMANIPULATORREADY = value;
                    NotifyPropertyChanged(nameof(DIMANIPULATORREADY));
                }
            }
        }

        private IOPortDescripter<bool> _DIMANIPULATOR_READY_0 = new IOPortDescripter<bool>(nameof(DIMANIPULATOR_READY_0), EnumIOType.INPUT, EnumIOOverride.NHI);

        public IOPortDescripter<bool> DIMANIPULATOR_READY_0
        {
            get { return _DIMANIPULATOR_READY_0; }
            set
            {
                if (value != this._DIMANIPULATOR_READY_0)
                {
                    _DIMANIPULATOR_READY_0 = value;
                    NotifyPropertyChanged(nameof(DIMANIPULATOR_READY_0));
                }
            }
        }
        private IOPortDescripter<bool> _DIMANIPULATOR_READY_1 = new IOPortDescripter<bool>(nameof(DIMANIPULATOR_READY_1), EnumIOType.INPUT, EnumIOOverride.NHI);

        public IOPortDescripter<bool> DIMANIPULATOR_READY_1
        {
            get { return _DIMANIPULATOR_READY_1; }
            set
            {
                if (value != this._DIMANIPULATOR_READY_1)
                {
                    _DIMANIPULATOR_READY_1 = value;
                    NotifyPropertyChanged(nameof(DIMANIPULATOR_READY_1));
                }
            }
        }
        private IOPortDescripter<bool> _DIZIF_LOCK = new IOPortDescripter<bool>(nameof(DIZIF_LOCK), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIZIF_LOCK
        {
            get { return _DIZIF_LOCK; }
            set
            {
                if (value != this._DIZIF_LOCK)
                {
                    _DIZIF_LOCK = value;
                    NotifyPropertyChanged(nameof(DIZIF_LOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DIZIF_UNLOCK = new IOPortDescripter<bool>(nameof(DIZIF_UNLOCK), EnumIOType.INPUT, EnumIOOverride.NHI);

        public IOPortDescripter<bool> DIZIF_UNLOCK
        {
            get { return _DIZIF_UNLOCK; }
            set
            {
                if (value != this._DIZIF_UNLOCK)
                {
                    _DIZIF_UNLOCK = value;
                    NotifyPropertyChanged(nameof(DIZIF_UNLOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DIMB_LOCK = new IOPortDescripter<bool>(nameof(DIMB_LOCK), EnumIOType.INPUT, EnumIOOverride.NHI);

        public IOPortDescripter<bool> DIMB_LOCK
        {
            get { return _DIMB_LOCK; }
            set
            {
                if (value != this._DIMB_LOCK)
                {
                    _DIMB_LOCK = value;
                    NotifyPropertyChanged(nameof(DIMB_LOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DICARRIER_LOCK_0 = new IOPortDescripter<bool>(nameof(DICARRIER_LOCK_0), EnumIOType.INPUT, EnumIOOverride.NHI);

        public IOPortDescripter<bool> DICARRIER_LOCK_0
        {
            get { return _DICARRIER_LOCK_0; }
            set
            {
                if (value != this._DICARRIER_LOCK_0)
                {
                    _DICARRIER_LOCK_0 = value;
                    NotifyPropertyChanged(nameof(DICARRIER_LOCK_0));
                }
            }
        }
        private IOPortDescripter<bool> _DICARRIER_LOCK_1 = new IOPortDescripter<bool>(nameof(DICARRIER_LOCK_1), EnumIOType.INPUT, EnumIOOverride.NHI);

        public IOPortDescripter<bool> DICARRIER_LOCK_1
        {
            get { return _DICARRIER_LOCK_1; }
            set
            {
                if (value != this._DICARRIER_LOCK_1)
                {
                    _DICARRIER_LOCK_1 = value;
                    NotifyPropertyChanged(nameof(DICARRIER_LOCK_1));
                }
            }
        }
        private IOPortDescripter<bool> _DICARRIER_POS = new IOPortDescripter<bool>(nameof(DICARRIER_POS), EnumIOType.INPUT, EnumIOOverride.NHI);

        public IOPortDescripter<bool> DICARRIER_POS
        {
            get { return _DICARRIER_POS; }
            set
            {
                if (value != this._DICARRIER_POS)
                {
                    _DICARRIER_POS = value;
                    NotifyPropertyChanged(nameof(DICARRIER_POS));
                }
            }
        }
        private IOPortDescripter<bool> _DIFRONTDOOROPEN = new IOPortDescripter<bool>(nameof(DIFRONTDOOROPEN), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFRONTDOOROPEN
        {
            get { return _DIFRONTDOOROPEN; }
            set
            {
                if (value != this._DIFRONTDOOROPEN)
                {
                    _DIFRONTDOOROPEN = value;
                    NotifyPropertyChanged(nameof(DIFRONTDOOROPEN));
                }
            }
        }
        private IOPortDescripter<bool> _DITH_LOCK = new IOPortDescripter<bool>(nameof(DITH_LOCK), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DITH_LOCK
        {
            get { return _DITH_LOCK; }
            set
            {
                if (value != this._DITH_LOCK)
                {
                    _DITH_LOCK = value;
                    NotifyPropertyChanged(nameof(DITH_LOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DITH_UNLOCK = new IOPortDescripter<bool>(nameof(DITH_UNLOCK), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DITH_UNLOCK
        {
            get { return _DITH_UNLOCK; }
            set
            {
                if (value != this._DITH_UNLOCK)
                {
                    _DITH_UNLOCK = value;
                    NotifyPropertyChanged(nameof(DITH_UNLOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DITH_MBLOCK = new IOPortDescripter<bool>(nameof(DITH_MBLOCK), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DITH_MBLOCK
        {
            get { return _DITH_MBLOCK; }
            set
            {
                if (value != this._DITH_MBLOCK)
                {
                    _DITH_MBLOCK = value;
                    NotifyPropertyChanged(nameof(DITH_MBLOCK));
                }
            }
        }

        private IOPortDescripter<bool> _DITH_MBUNLOCK = new IOPortDescripter<bool>(nameof(DITH_MBUNLOCK), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DITH_MBUNLOCK
        {
            get { return _DITH_MBUNLOCK; }
            set
            {
                if (value != this._DITH_MBUNLOCK)
                {
                    _DITH_MBUNLOCK = value;
                    NotifyPropertyChanged(nameof(DITH_MBUNLOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DITH_PBLOCK = new IOPortDescripter<bool>(nameof(DITH_PBLOCK), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DITH_PBLOCK
        {
            get { return _DITH_PBLOCK; }
            set
            {
                if (value != this._DITH_PBLOCK)
                {
                    _DITH_PBLOCK = value;
                    NotifyPropertyChanged(nameof(DITH_PBLOCK));
                }
            }
        }

        private IOPortDescripter<bool> _DITH_PBUNLOCK = new IOPortDescripter<bool>(nameof(DITH_PBUNLOCK), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DITH_PBUNLOCK
        {
            get { return _DITH_PBUNLOCK; }
            set
            {
                if (value != this._DITH_PBUNLOCK)
                {
                    _DITH_PBUNLOCK = value;
                    NotifyPropertyChanged(nameof(DITH_PBUNLOCK));
                }
            }
        }

        private IOPortDescripter<bool> _DITH_DOWN = new IOPortDescripter<bool>(nameof(DITH_DOWN), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DITH_DOWN
        {
            get { return _DITH_DOWN; }
            set
            {
                if (value != this._DITH_DOWN)
                {
                    _DITH_DOWN = value;
                    NotifyPropertyChanged(nameof(DITH_DOWN));
                }
            }
        }

        private IOPortDescripter<bool> _DITH_UP = new IOPortDescripter<bool>(nameof(DITH_UP), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DITH_UP
        {
            get { return _DITH_UP; }
            set
            {
                if (value != this._DITH_UP)
                {
                    _DITH_UP = value;
                    NotifyPropertyChanged(nameof(DITH_UP));
                }
            }
        }

        private IOPortDescripter<bool> _DIWMB0 = new IOPortDescripter<bool>(nameof(DIWMB0), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DIWMB0
        {
            get { return _DIWMB0; }
            set
            {
                if (value != this._DIWMB0)
                {
                    _DIWMB0 = value;
                    NotifyPropertyChanged(nameof(DIWMB0));
                }
            }
        }

        private IOPortDescripter<bool> _DIWMB1 = new IOPortDescripter<bool>(nameof(DIWMB1), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DIWMB1
        {
            get { return _DIWMB1; }
            set
            {
                if (value != this._DIWMB1)
                {
                    _DIWMB1 = value;
                    NotifyPropertyChanged(nameof(DIWMB1));
                }
            }
        }

        private IOPortDescripter<bool> _DIWMB2 = new IOPortDescripter<bool>(nameof(DIWMB2), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DIWMB2
        {
            get { return _DIWMB2; }
            set
            {
                if (value != this._DIWMB2)
                {
                    _DIWMB2 = value;
                    NotifyPropertyChanged(nameof(DIWMB2));
                }
            }
        }

        private IOPortDescripter<bool> _DIWMB3 = new IOPortDescripter<bool>(nameof(DIWMB3), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DIWMB3
        {
            get { return _DIWMB3; }
            set
            {
                if (value != this._DIWMB3)
                {
                    _DIWMB3 = value;
                    NotifyPropertyChanged(nameof(_DIWMB3));
                }
            }
        }

        private IOPortDescripter<bool> _DILATCHED_VERIFY = new IOPortDescripter<bool>(nameof(DILATCHED_VERIFY), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DILATCHED_VERIFY
        {
            get { return _DILATCHED_VERIFY; }
            set
            {
                if (value != this._DILATCHED_VERIFY)
                {
                    _DILATCHED_VERIFY = value;
                    NotifyPropertyChanged(nameof(DILATCHED_VERIFY));
                }
            }
        }
        private IOPortDescripter<bool> _DIUNLATCHED_VERIFY = new IOPortDescripter<bool>(nameof(DIUNLATCHED_VERIFY), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIUNLATCHED_VERIFY
        {
            get { return _DIUNLATCHED_VERIFY; }
            set
            {
                if (value != this._DIUNLATCHED_VERIFY)
                {
                    _DIUNLATCHED_VERIFY = value;
                    NotifyPropertyChanged(nameof(DIUNLATCHED_VERIFY));
                }
            }
        }
        private IOPortDescripter<bool> _DIPROBE_CARD_PRESENTED = new IOPortDescripter<bool>(nameof(DIPROBE_CARD_PRESENTED), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIPROBE_CARD_PRESENTED
        {
            get { return _DIPROBE_CARD_PRESENTED; }
            set
            {
                if (value != this._DIPROBE_CARD_PRESENTED)
                {
                    _DIPROBE_CARD_PRESENTED = value;
                    NotifyPropertyChanged(nameof(DIPROBE_CARD_PRESENTED));
                }
            }
        }
        private IOPortDescripter<bool> _DITESTER_RDY_FOR_PC = new IOPortDescripter<bool>(nameof(DITESTER_RDY_FOR_PC), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DITESTER_RDY_FOR_PC
        {
            get { return _DITESTER_RDY_FOR_PC; }
            set
            {
                if (value != this._DITESTER_RDY_FOR_PC)
                {
                    _DITESTER_RDY_FOR_PC = value;
                    NotifyPropertyChanged(nameof(DITESTER_RDY_FOR_PC));
                }
            }
        }
        private IOPortDescripter<bool> _DICHUCK_SAFE_CMD = new IOPortDescripter<bool>(nameof(DICHUCK_SAFE_CMD), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICHUCK_SAFE_CMD
        {
            get { return _DICHUCK_SAFE_CMD; }
            set
            {
                if (value != this._DICHUCK_SAFE_CMD)
                {
                    _DICHUCK_SAFE_CMD = value;
                    NotifyPropertyChanged(nameof(DICHUCK_SAFE_CMD));
                }
            }
        }
        private IOPortDescripter<bool> _DICARDMOVEINDONE = new IOPortDescripter<bool>(nameof(DICARDMOVEINDONE), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICARDMOVEINDONE
        {
            get { return _DICARDMOVEINDONE; }
            set
            {
                if (value != this._DICARDMOVEINDONE)
                {
                    _DICARDMOVEINDONE = value;
                    NotifyPropertyChanged(nameof(DICARDMOVEINDONE));
                }
            }
        }
        private IOPortDescripter<bool> _DICARDMOVEOUTDONE = new IOPortDescripter<bool>(nameof(DICARDMOVEOUTDONE), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICARDMOVEOUTDONE
        {
            get { return _DICARDMOVEOUTDONE; }
            set
            {
                if (value != this._DICARDMOVEOUTDONE)
                {
                    _DICARDMOVEOUTDONE = value;
                    NotifyPropertyChanged(nameof(DICARDMOVEOUTDONE));
                }
            }
        }
        private IOPortDescripter<bool> _DIPlateSwingUpPOS = new IOPortDescripter<bool>(nameof(DIPlateSwingUpPOS), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIPlateSwingUpPOS
        {
            get { return _DIPlateSwingUpPOS; }
            set
            {
                if (value != this._DIPlateSwingUpPOS)
                {
                    _DIPlateSwingUpPOS = value;
                    NotifyPropertyChanged(nameof(DIPlateSwingUpPOS));
                }
            }
        }
        private IOPortDescripter<bool> _DIPlateSwingDownPOS = new IOPortDescripter<bool>(nameof(DIPlateSwingDownPOS), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIPlateSwingDownPOS
        {
            get { return _DIPlateSwingDownPOS; }
            set
            {
                if (value != this._DIPlateSwingDownPOS)
                {
                    _DIPlateSwingDownPOS = value;
                    NotifyPropertyChanged(nameof(DIPlateSwingDownPOS));
                }
            }
        }
        private IOPortDescripter<bool> _DICardOnPlateL = new IOPortDescripter<bool>(nameof(DICardOnPlateL), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICardOnPlateL
        {
            get { return _DICardOnPlateL; }
            set
            {
                if (value != this._DICardOnPlateL)
                {
                    _DICardOnPlateL = value;
                    NotifyPropertyChanged(nameof(DICardOnPlateL));
                }
            }
        }
        private IOPortDescripter<bool> _DICardOnPlateR = new IOPortDescripter<bool>(nameof(DICardOnPlateR), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICardOnPlateR
        {
            get { return _DICardOnPlateR; }
            set
            {
                if (value != this._DICardOnPlateR)
                {
                    _DICardOnPlateR = value;
                    NotifyPropertyChanged(nameof(DICardOnPlateR));
                }
            }
        }
        private IOPortDescripter<bool> _DIHardDockActive = new IOPortDescripter<bool>(nameof(DIHardDockActive), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIHardDockActive
        {
            get { return _DIHardDockActive; }
            set
            {
                if (value != this._DIHardDockActive)
                {
                    _DIHardDockActive = value;
                    NotifyPropertyChanged(nameof(DIHardDockActive));
                }
            }
        }
        private IOPortDescripter<bool> _DISoftDockActive = new IOPortDescripter<bool>(nameof(DISoftDockActive), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DISoftDockActive
        {
            get { return _DISoftDockActive; }
            set
            {
                if (value != this._DISoftDockActive)
                {
                    _DISoftDockActive = value;
                    NotifyPropertyChanged(nameof(DISoftDockActive));
                }
            }
        }
        private IOPortDescripter<bool> _DIStiffDetect = new IOPortDescripter<bool>(nameof(DIStiffDetect), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIStiffDetect
        {
            get { return _DIStiffDetect; }
            set
            {
                if (value != this._DIStiffDetect)
                {
                    _DIStiffDetect = value;
                    NotifyPropertyChanged(nameof(DIStiffDetect));
                }
            }
        }
        private IOPortDescripter<bool> _DIStiffOpen = new IOPortDescripter<bool>(nameof(DIStiffOpen), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIStiffOpen
        {
            get { return _DIStiffOpen; }
            set
            {
                if (value != this._DIStiffOpen)
                {
                    _DIStiffOpen = value;
                    NotifyPropertyChanged(nameof(DIStiffOpen));
                }
            }
        }
        private IOPortDescripter<bool> _DIFDOOR_CLOSE = new IOPortDescripter<bool>(nameof(DIFDOOR_CLOSE), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFDOOR_CLOSE
        {
            get { return _DIFDOOR_CLOSE; }
            set
            {
                if (value != this._DIFDOOR_CLOSE)
                {
                    _DIFDOOR_CLOSE = value;
                    NotifyPropertyChanged(nameof(DIFDOOR_CLOSE));
                }
            }
        }
        private IOPortDescripter<bool> _DILDOOR_CLOSE = new IOPortDescripter<bool>(nameof(DILDOOR_CLOSE), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DILDOOR_CLOSE
        {
            get { return _DILDOOR_CLOSE; }
            set
            {
                if (value != this._DILDOOR_CLOSE)
                {
                    _DILDOOR_CLOSE = value;
                    NotifyPropertyChanged(nameof(DILDOOR_CLOSE));
                }
            }
        }
        private IOPortDescripter<bool> _DIFDOOR_OPEN = new IOPortDescripter<bool>(nameof(DIFDOOR_OPEN), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFDOOR_OPEN
        {
            get { return _DIFDOOR_OPEN; }
            set
            {
                if (value != this._DIFDOOR_OPEN)
                {
                    _DIFDOOR_OPEN = value;
                    NotifyPropertyChanged(nameof(DIFDOOR_OPEN));
                }
            }
        }
        private IOPortDescripter<bool> _DILDOOR_OPEN = new IOPortDescripter<bool>(nameof(DILDOOR_OPEN), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DILDOOR_OPEN
        {
            get { return _DILDOOR_OPEN; }
            set
            {
                if (value != this._DILDOOR_OPEN)
                {
                    _DILDOOR_OPEN = value;
                    NotifyPropertyChanged(nameof(DILDOOR_OPEN));
                }
            }
        }
        private IOPortDescripter<bool> _DINO_CLAMP = new IOPortDescripter<bool>(nameof(DINO_CLAMP), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DINO_CLAMP
        {
            get { return _DINO_CLAMP; }
            set
            {
                if (value != this._DINO_CLAMP)
                {
                    _DINO_CLAMP = value;
                    NotifyPropertyChanged(nameof(DINO_CLAMP));
                }
            }
        }
        private IOPortDescripter<bool> _DICLP_LOCK = new IOPortDescripter<bool>(nameof(DICLP_LOCK), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DICLP_LOCK
        {
            get { return _DICLP_LOCK; }
            set
            {
                if (value != this._DICLP_LOCK)
                {
                    _DICLP_LOCK = value;
                    NotifyPropertyChanged(nameof(DICLP_LOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DICLP_UNLOCK = new IOPortDescripter<bool>(nameof(DICLP_UNLOCK), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICLP_UNLOCK
        {
            get { return _DICLP_UNLOCK; }
            set
            {
                if (value != this._DICLP_UNLOCK)
                {
                    _DICLP_UNLOCK = value;
                    NotifyPropertyChanged(nameof(DICLP_UNLOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DIMB_440 = new IOPortDescripter<bool>(nameof(DIMB_440), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIMB_440
        {
            get { return _DIMB_440; }
            set
            {
                if (value != this._DIMB_440)
                    _DIMB_440 = value;
                NotifyPropertyChanged(nameof(DIMB_440));
            }
        }

        private IOPortDescripter<bool> _DIMB_480 = new IOPortDescripter<bool>(nameof(DIMB_480), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIMB_480
        {
            get { return _DIMB_480; }
            set
            {
                if (value != this._DIMB_480)
                    _DIMB_480 = value;
                NotifyPropertyChanged(nameof(DIMB_480));
            }
        }
        private IOPortDescripter<bool> _DINEEDLE_BRUSH_UP = new IOPortDescripter<bool>(nameof(DINEEDLE_BRUSH_UP), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DINEEDLE_BRUSH_UP
        {
            get { return _DINEEDLE_BRUSH_UP; }
            set
            {
                if (value != this._DINEEDLE_BRUSH_UP)
                    _DINEEDLE_BRUSH_UP = value;
                NotifyPropertyChanged(nameof(DINEEDLE_BRUSH_UP));
            }
        }
        private IOPortDescripter<bool> _DINEEDLE_BRUSH_DN = new IOPortDescripter<bool>(nameof(DINEEDLE_BRUSH_DN), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DINEEDLE_BRUSH_DN
        {
            get { return _DINEEDLE_BRUSH_DN; }
            set
            {
                if (value != this._DINEEDLE_BRUSH_DN)
                    _DINEEDLE_BRUSH_DN = value;
                NotifyPropertyChanged(nameof(DINEEDLE_BRUSH_DN));
            }
        }
        private IOPortDescripter<bool> _DIBeamLOCK = new IOPortDescripter<bool>(nameof(DIBeamLOCK), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIBeamLOCK
        {
            get { return _DIBeamLOCK; }
            set
            {
                if (value != this._DIBeamLOCK)
                    _DIBeamLOCK = value;
                NotifyPropertyChanged(nameof(DIBeamLOCK));
            }
        }
        private IOPortDescripter<bool> _DIBeamUnLOCK = new IOPortDescripter<bool>(nameof(DIBeamUnLOCK), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIBeamUnLOCK
        {
            get { return _DIBeamUnLOCK; }
            set
            {
                if (value != this._DIBeamUnLOCK)
                    _DIBeamUnLOCK = value;
                NotifyPropertyChanged(nameof(DIBeamUnLOCK));
            }
        }
        private IOPortDescripter<bool> _DIDD_FRONT_INNER_COVER_OPEN = new IOPortDescripter<bool>(nameof(DIDD_FRONT_INNER_COVER_OPEN), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIDD_FRONT_INNER_COVER_OPEN
        {
            get { return _DIDD_FRONT_INNER_COVER_OPEN; }
            set
            {
                if (value != this._DIDD_FRONT_INNER_COVER_OPEN)
                    _DIDD_FRONT_INNER_COVER_OPEN = value;
                NotifyPropertyChanged(nameof(DIDD_FRONT_INNER_COVER_OPEN));
            }
        }
        private IOPortDescripter<bool> _DIDD_FRONT_INNER_COVER_CLOSE = new IOPortDescripter<bool>(nameof(DIDD_FRONT_INNER_COVER_CLOSE), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIDD_FRONT_INNER_COVER_CLOSE
        {
            get { return _DIDD_FRONT_INNER_COVER_CLOSE; }
            set
            {
                if (value != this._DIDD_FRONT_INNER_COVER_CLOSE)
                    _DIDD_FRONT_INNER_COVER_CLOSE = value;
                NotifyPropertyChanged(nameof(DIDD_FRONT_INNER_COVER_CLOSE));
            }
        }
        private IOPortDescripter<bool> _DIFOUP_Cover_Lock = new IOPortDescripter<bool>(nameof(DIFOUP_Cover_Lock), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFOUP_Cover_Lock
        {
            get { return _DIFOUP_Cover_Lock; }
            set
            {
                if (value != this._DIFOUP_Cover_Lock)
                    _DIFOUP_Cover_Lock = value;
                NotifyPropertyChanged(nameof(DIFOUP_Cover_Lock));
            }
        }
        private IOPortDescripter<bool> _DIFOUP_Cover_UnLock = new IOPortDescripter<bool>(nameof(DIFOUP_Cover_UnLock), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFOUP_Cover_UnLock
        {
            get { return _DIFOUP_Cover_UnLock; }
            set
            {
                if (value != this._DIFOUP_Cover_UnLock)
                    _DIFOUP_Cover_UnLock = value;
                NotifyPropertyChanged(nameof(DIFOUP_Cover_UnLock));
            }
        }

        private IOPortDescripter<bool> _DITESTER_INTERFACE_DUMMY = new IOPortDescripter<bool>(nameof(DITESTER_INTERFACE_DUMMY), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DITESTER_INTERFACE_DUMMY
        {
            get { return _DITESTER_INTERFACE_DUMMY; }
            set
            {
                if (value != this._DITESTER_INTERFACE_DUMMY)
                    _DITESTER_INTERFACE_DUMMY = value;
                NotifyPropertyChanged(nameof(DITESTER_INTERFACE_DUMMY));
            }
        }
        private IOPortDescripter<bool> _DIT2K_CLAMP_UP = new IOPortDescripter<bool>(nameof(DIT2K_CLAMP_UP), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIT2K_CLAMP_UP
        {
            get { return _DIT2K_CLAMP_UP; }
            set
            {
                if (value != this._DIT2K_CLAMP_UP)
                    _DIT2K_CLAMP_UP = value;
                NotifyPropertyChanged(nameof(DIT2K_CLAMP_UP));
            }
        }
        private IOPortDescripter<bool> _DIT2K_CLAMP_MID = new IOPortDescripter<bool>(nameof(DIT2K_CLAMP_MID), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIT2K_CLAMP_MID
        {
            get { return _DIT2K_CLAMP_MID; }
            set
            {
                if (value != this._DIT2K_CLAMP_MID)
                    _DIT2K_CLAMP_MID = value;
                NotifyPropertyChanged(nameof(DIT2K_CLAMP_MID));
            }
        }
        private IOPortDescripter<bool> _DIT2K_PROBECARD_UP = new IOPortDescripter<bool>(nameof(DIT2K_PROBECARD_UP), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIT2K_PROBECARD_UP
        {
            get { return _DIT2K_PROBECARD_UP; }
            set
            {
                if (value != this._DIT2K_PROBECARD_UP)
                    _DIT2K_PROBECARD_UP = value;
                NotifyPropertyChanged(nameof(DIT2K_PROBECARD_UP));
            }
        }
        private IOPortDescripter<bool> _DIT2K_PROBECARD_MID = new IOPortDescripter<bool>(nameof(DIT2K_PROBECARD_MID), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIT2K_PROBECARD_MID
        {
            get { return _DIT2K_PROBECARD_MID; }
            set
            {
                if (value != this._DIT2K_PROBECARD_MID)
                    _DIT2K_PROBECARD_MID = value;
                NotifyPropertyChanged(nameof(DIT2K_PROBECARD_MID));
            }
        }
        private IOPortDescripter<bool> _DIT2K_CYLINDER_UP = new IOPortDescripter<bool>(nameof(DIT2K_CYLINDER_UP), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIT2K_CYLINDER_UP
        {
            get { return _DIT2K_CYLINDER_UP; }
            set
            {
                if (value != this._DIT2K_CYLINDER_UP)
                    _DIT2K_CYLINDER_UP = value;
                NotifyPropertyChanged(nameof(DIT2K_CYLINDER_UP));
            }
        }
        private IOPortDescripter<bool> _DIT2K_CYLINDER_MID = new IOPortDescripter<bool>(nameof(DIT2K_CYLINDER_MID), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIT2K_CYLINDER_MID
        {
            get { return _DIT2K_CYLINDER_MID; }
            set
            {
                if (value != this._DIT2K_CYLINDER_MID)
                    _DIT2K_CYLINDER_MID = value;
                NotifyPropertyChanged(nameof(DIT2K_CYLINDER_MID));
            }
        }
        private IOPortDescripter<bool> _DIT2K_CYLINDER_DOWN = new IOPortDescripter<bool>(nameof(DIT2K_CYLINDER_DOWN), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIT2K_CYLINDER_DOWN
        {
            get { return _DIT2K_CYLINDER_DOWN; }
            set
            {
                if (value != this._DIT2K_CYLINDER_DOWN)
                    _DIT2K_CYLINDER_DOWN = value;
                NotifyPropertyChanged(nameof(DIT2K_CYLINDER_DOWN));
            }
        }
        private IOPortDescripter<bool> _DITESTER_HEAD_DOCK = new IOPortDescripter<bool>(nameof(DITESTER_HEAD_DOCK), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DITESTER_HEAD_DOCK
        {
            get { return _DITESTER_HEAD_DOCK; }
            set
            {
                if (value != this._DITESTER_HEAD_DOCK)
                    _DITESTER_HEAD_DOCK = value;
                NotifyPropertyChanged(nameof(DITESTER_HEAD_DOCK));
            }
        }
        private IOPortDescripter<bool> _DITESTER_HEAD_HORI = new IOPortDescripter<bool>(nameof(DITESTER_HEAD_HORI), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DITESTER_HEAD_HORI
        {
            get { return _DITESTER_HEAD_HORI; }
            set
            {
                if (value != this._DITESTER_HEAD_HORI)
                    _DITESTER_HEAD_HORI = value;
                NotifyPropertyChanged(nameof(DITESTER_HEAD_HORI));
            }
        }
        private IOPortDescripter<bool> _DICard_Holder_Detect = new IOPortDescripter<bool>(nameof(DICard_Holder_Detect), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICard_Holder_Detect
        {
            get { return _DICard_Holder_Detect; }
            set
            {
                if (value != this._DICard_Holder_Detect)
                    _DICard_Holder_Detect = value;
                NotifyPropertyChanged(nameof(DICard_Holder_Detect));
            }
        }
        private IOPortDescripter<bool> _DITESTER_VACUUM_READY = new IOPortDescripter<bool>(nameof(DITESTER_VACUUM_READY), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DITESTER_VACUUM_READY
        {
            get { return _DITESTER_VACUUM_READY; }
            set
            {
                if (value != this._DITESTER_VACUUM_READY)
                    _DITESTER_VACUUM_READY = value;
                NotifyPropertyChanged(nameof(DITESTER_VACUUM_READY));
            }
        }
        private IOPortDescripter<bool> _DITESTER_EMERGENCY = new IOPortDescripter<bool>(nameof(DITESTER_EMERGENCY), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DITESTER_EMERGENCY
        {
            get { return _DITESTER_EMERGENCY; }
            set
            {
                if (value != this._DITESTER_EMERGENCY)
                    _DITESTER_EMERGENCY = value;
                NotifyPropertyChanged(nameof(DITESTER_EMERGENCY));
            }
        }
        private IOPortDescripter<bool> _DICOOLANT_LOW = new IOPortDescripter<bool>(nameof(DICOOLANT_LOW), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICOOLANT_LOW
        {
            get { return _DICOOLANT_LOW; }
            set
            {
                if (value != this._DICOOLANT_LOW)
                    _DICOOLANT_LOW = value;
                NotifyPropertyChanged(nameof(DICOOLANT_LOW));
            }
        }
        private IOPortDescripter<bool> _DIDRY_AIR_PRSSURE = new IOPortDescripter<bool>(nameof(DIDRY_AIR_PRSSURE), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIDRY_AIR_PRSSURE
        {
            get { return _DIDRY_AIR_PRSSURE; }
            set
            {
                if (value != this._DIDRY_AIR_PRSSURE)
                    _DIDRY_AIR_PRSSURE = value;
                NotifyPropertyChanged(nameof(DIDRY_AIR_PRSSURE));
            }
        }
        private IOPortDescripter<bool> _DIBeamLOCK_2 = new IOPortDescripter<bool>(nameof(DIBeamLOCK_2), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIBeamLOCK_2
        {
            get { return _DIBeamLOCK_2; }
            set
            {
                if (value != this._DIBeamLOCK_2)
                    _DIBeamLOCK_2 = value;
                NotifyPropertyChanged(nameof(DIBeamLOCK_2));
            }
        }
        private IOPortDescripter<bool> _DIBeamUnLOCK_2 = new IOPortDescripter<bool>(nameof(DIBeamUnLOCK_2), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIBeamUnLOCK_2
        {
            get { return _DIBeamUnLOCK_2; }
            set
            {
                if (value != this._DIBeamUnLOCK_2)
                    _DIBeamUnLOCK_2 = value;
                NotifyPropertyChanged(nameof(DIBeamUnLOCK_2));
            }
        }

        private IOPortDescripter<bool> _DIV93K_POGO_DUT_DOCK = new IOPortDescripter<bool>(nameof(DIV93K_POGO_DUT_DOCK), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIV93K_POGO_DUT_DOCK
        {
            get { return _DIV93K_POGO_DUT_DOCK; }
            set
            {
                if (value != this._DIV93K_POGO_DUT_DOCK)
                    _DIV93K_POGO_DUT_DOCK = value;
                NotifyPropertyChanged(nameof(DIV93K_POGO_DUT_DOCK));
            }
        }
        private IOPortDescripter<bool> _DIHEATER_FAIL = new IOPortDescripter<bool>(nameof(DIHEATER_FAIL), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIHEATER_FAIL
        {
            get { return _DIHEATER_FAIL; }
            set
            {
                if (value != this._DIHEATER_FAIL)
                    _DIHEATER_FAIL = value;
                NotifyPropertyChanged(nameof(DIHEATER_FAIL));
            }
        }
        private IOPortDescripter<bool> _DIFD_PREIN_FRONT_IN = new IOPortDescripter<bool>(nameof(DIFD_PREIN_FRONT_IN), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFD_PREIN_FRONT_IN
        {
            get { return _DIFD_PREIN_FRONT_IN; }
            set
            {
                if (value != this._DIFD_PREIN_FRONT_IN)
                    _DIFD_PREIN_FRONT_IN = value;
                NotifyPropertyChanged(nameof(DIFD_PREIN_FRONT_IN));
            }
        }

        private IOPortDescripter<bool> _DIFD_PREIN_FRONT_OUT = new IOPortDescripter<bool>(nameof(DIFD_PREIN_FRONT_OUT), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFD_PREIN_FRONT_OUT
        {
            get { return _DIFD_PREIN_FRONT_OUT; }
            set
            {
                if (value != this._DIFD_PREIN_FRONT_OUT)
                    _DIFD_PREIN_FRONT_OUT = value;
                NotifyPropertyChanged(nameof(DIFD_PREIN_FRONT_OUT));
            }
        }

        private IOPortDescripter<bool> _DIFD_PREIN_REAR_IN = new IOPortDescripter<bool>(nameof(DIFD_PREIN_REAR_IN), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFD_PREIN_REAR_IN
        {
            get { return _DIFD_PREIN_REAR_IN; }
            set
            {
                if (value != this._DIFD_PREIN_REAR_IN)
                    _DIFD_PREIN_REAR_IN = value;
                NotifyPropertyChanged(nameof(DIFD_PREIN_REAR_IN));
            }
        }

        private IOPortDescripter<bool> _DIFD_PREIN_REAR_OUT = new IOPortDescripter<bool>(nameof(DIFD_PREIN_REAR_OUT), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFD_PREIN_REAR_OUT
        {
            get { return _DIFD_PREIN_REAR_OUT; }
            set
            {
                if (value != this._DIFD_PREIN_REAR_OUT)
                    _DIFD_PREIN_REAR_OUT = value;
                NotifyPropertyChanged(nameof(DIFD_PREIN_REAR_OUT));
            }
        }

        private IOPortDescripter<bool> _DIFD_SUBCHUCK_ON_WAFER1 = new IOPortDescripter<bool>(nameof(DIFD_SUBCHUCK_ON_WAFER1), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFD_SUBCHUCK_ON_WAFER1
        {
            get { return _DIFD_SUBCHUCK_ON_WAFER1; }
            set
            {
                if (value != this._DIFD_SUBCHUCK_ON_WAFER1)
                    _DIFD_SUBCHUCK_ON_WAFER1 = value;
                NotifyPropertyChanged(nameof(DIFD_SUBCHUCK_ON_WAFER1));
            }
        }

        private IOPortDescripter<bool> _DIFD_SUBCHUCK_ON_WAFER2 = new IOPortDescripter<bool>(nameof(DIFD_SUBCHUCK_ON_WAFER2), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFD_SUBCHUCK_ON_WAFER2
        {
            get { return _DIFD_SUBCHUCK_ON_WAFER2; }
            set
            {
                if (value != this._DIFD_SUBCHUCK_ON_WAFER2)
                    _DIFD_SUBCHUCK_ON_WAFER2 = value;
                NotifyPropertyChanged(nameof(DIFD_SUBCHUCK_ON_WAFER2));
            }
        }

        private IOPortDescripter<bool> _DIFD_NOTCH1 = new IOPortDescripter<bool>(nameof(DIFD_NOTCH1), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFD_NOTCH1
        {
            get { return _DIFD_NOTCH1; }
            set
            {
                if (value != this._DIFD_NOTCH1)
                    _DIFD_NOTCH1 = value;
                NotifyPropertyChanged(nameof(DIFD_NOTCH1));
            }
        }

        private IOPortDescripter<bool> _DIFD_NOTCH2 = new IOPortDescripter<bool>(nameof(DIFD_NOTCH2), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFD_NOTCH2
        {
            get { return _DIFD_NOTCH2; }
            set
            {
                if (value != this._DIFD_NOTCH2)
                    _DIFD_NOTCH2 = value;
                NotifyPropertyChanged(nameof(DIFD_NOTCH2));
            }
        }
        private IOPortDescripter<bool> _DIFD_THREE_LEGUP = new IOPortDescripter<bool>(nameof(DIFD_THREE_LEGUP), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFD_THREE_LEGUP
        {
            get { return _DIFD_THREE_LEGUP; }
            set
            {
                if (value != this._DIFD_THREE_LEGUP)
                    _DIFD_THREE_LEGUP = value;
                NotifyPropertyChanged(nameof(DIFD_THREE_LEGUP));
            }
        }
        private IOPortDescripter<bool> _DIFD_THREE_LEGDN = new IOPortDescripter<bool>(nameof(DIFD_THREE_LEGDN), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFD_THREE_LEGDN
        {
            get { return _DIFD_THREE_LEGDN; }
            set
            {
                if (value != this._DIFD_THREE_LEGDN)
                    _DIFD_THREE_LEGDN = value;
                NotifyPropertyChanged(nameof(DIFD_THREE_LEGDN));
            }
        }
        private IOPortDescripter<bool> _DIWAFERONCHUCK_6 = new IOPortDescripter<bool>(nameof(DIWAFERONCHUCK_6), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIWAFERONCHUCK_6
        {
            get { return _DIWAFERONCHUCK_6; }
            set
            {
                if (value != this._DIWAFERONCHUCK_6)
                    _DIWAFERONCHUCK_6 = value;
                NotifyPropertyChanged(nameof(DIWAFERONCHUCK_6));
            }
        }
        private IOPortDescripter<bool> _DIWAFERONCHUCK_8 = new IOPortDescripter<bool>(nameof(DIWAFERONCHUCK_8), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIWAFERONCHUCK_8
        {
            get { return _DIWAFERONCHUCK_8; }
            set
            {
                if (value != this._DIWAFERONCHUCK_8)
                    _DIWAFERONCHUCK_8 = value;
                NotifyPropertyChanged(nameof(DIWAFERONCHUCK_8));
            }
        }
        private IOPortDescripter<bool> _DIWAFERONCHUCK_12 = new IOPortDescripter<bool>(nameof(DIWAFERONCHUCK_12), EnumIOType.INPUT);

        public IOPortDescripter<bool> DIWAFERONCHUCK_12
        {
            get { return _DIWAFERONCHUCK_12; }
            set
            {
                if (value != this._DIWAFERONCHUCK_12)
                    _DIWAFERONCHUCK_12 = value;
                NotifyPropertyChanged(nameof(DIWAFERONCHUCK_12));
            }
        }

        private IOPortDescripter<bool> _DIFRONT_DOOR_OPEN = new IOPortDescripter<bool>(nameof(DIFRONT_DOOR_OPEN), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIFRONT_DOOR_OPEN
        {
            get { return _DIFRONT_DOOR_OPEN; }
            set
            {
                if (value != this._DIFRONT_DOOR_OPEN)
                    _DIFRONT_DOOR_OPEN = value;
                NotifyPropertyChanged(nameof(DIFRONT_DOOR_OPEN));
            }
        }

        private IOPortDescripter<bool> _DINC_SENSOR = new IOPortDescripter<bool>(nameof(DINC_SENSOR), EnumIOType.INPUT);

        public IOPortDescripter<bool> DINC_SENSOR
        {
            get { return _DINC_SENSOR; }
            set
            {
                if (value != this._DINC_SENSOR)
                    _DINC_SENSOR = value;
                NotifyPropertyChanged(nameof(DINC_SENSOR));
            }
        }

        private IOPortDescripter<bool> _DINCPAD_VAC = new IOPortDescripter<bool>(nameof(DINCPAD_VAC), EnumIOType.INPUT);

        public IOPortDescripter<bool> DINCPAD_VAC
        {
            get { return _DINCPAD_VAC; }
            set
            {
                if (value != this._DINCPAD_VAC)
                    _DINCPAD_VAC = value;
                NotifyPropertyChanged(nameof(DINCPAD_VAC));
            }
        }

        private IOPortDescripter<bool> _DICHECK_WAFER_OUT_ON_CST = new IOPortDescripter<bool>(nameof(DICHECK_WAFER_OUT_ON_CST), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICHECK_WAFER_OUT_ON_CST
        {
            get { return _DICHECK_WAFER_OUT_ON_CST; }
            set
            {
                if (value != this._DICHECK_WAFER_OUT_ON_CST)
                    _DICHECK_WAFER_OUT_ON_CST = value;
                NotifyPropertyChanged(nameof(DICHECK_WAFER_OUT_ON_CST));
            }
        }
        private IOPortDescripter<bool> _DICHECK_WAFER_ON_ARM = new IOPortDescripter<bool>(nameof(DICHECK_WAFER_ON_ARM), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICHECK_WAFER_ON_ARM
        {
            get { return _DICHECK_WAFER_ON_ARM; }
            set
            {
                if (value != this._DICHECK_WAFER_ON_ARM)
                    _DICHECK_WAFER_ON_ARM = value;
                NotifyPropertyChanged(nameof(DICHECK_WAFER_ON_ARM));
            }
        }
        private IOPortDescripter<bool> _DICHECK_WAFER_ON_PRE = new IOPortDescripter<bool>(nameof(DICHECK_WAFER_ON_PRE), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICHECK_WAFER_ON_PRE
        {
            get { return _DICHECK_WAFER_ON_PRE; }
            set
            {
                if (value != this._DICHECK_WAFER_ON_PRE)
                    _DICHECK_WAFER_ON_PRE = value;
                NotifyPropertyChanged(nameof(DICHECK_WAFER_ON_PRE));
            }
        }

        private IOPortDescripter<bool> _DICARDTRAY_LOCK = new IOPortDescripter<bool>(nameof(DICARDTRAY_LOCK), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICARDTRAY_LOCK
        {
            get { return _DICARDTRAY_LOCK; }
            set
            {
                if (value != this._DICARDTRAY_LOCK)
                    _DICARDTRAY_LOCK = value;
                NotifyPropertyChanged(nameof(DICARDTRAY_LOCK));
            }
        }

        private IOPortDescripter<bool> _DICARDTRAY_UNLOCK = new IOPortDescripter<bool>(nameof(DICARDTRAY_UNLOCK), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICARDTRAY_UNLOCK
        {
            get { return _DICARDTRAY_UNLOCK; }
            set
            {
                if (value != this._DICARDTRAY_UNLOCK)
                    _DICARDTRAY_UNLOCK = value;
                NotifyPropertyChanged(nameof(DICARDTRAY_UNLOCK));
            }
        }

        private IOPortDescripter<bool> _DICardOnPlateM = new IOPortDescripter<bool>(nameof(DICardOnPlateM), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DICardOnPlateM
        {
            get { return _DICardOnPlateM; }
            set
            {
                if (value != this._DICardOnPlateM)
                    _DICardOnPlateM = value;
                NotifyPropertyChanged(nameof(DICardOnPlateM));
            }
        }

        private IOPortDescripter<bool> _DITESTER_DOCK_SENSOR = new IOPortDescripter<bool>(nameof(DITESTER_DOCK_SENSOR), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DITESTER_DOCK_SENSOR
        {
            get { return _DITESTER_DOCK_SENSOR; }
            set
            {
                if (value != this._DITESTER_DOCK_SENSOR)
                    _DITESTER_DOCK_SENSOR = value;
                NotifyPropertyChanged(nameof(DITESTER_DOCK_SENSOR));
            }
        }



        private IOPortDescripter<bool> _DIINSPECTION_COVER_LOCK = new IOPortDescripter<bool>(nameof(DIINSPECTION_COVER_LOCK), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIINSPECTION_COVER_LOCK
        {
            get { return _DIINSPECTION_COVER_LOCK; }
            set
            {
                if (value != this._DIINSPECTION_COVER_LOCK)
                    _DIINSPECTION_COVER_LOCK = value;
                NotifyPropertyChanged(nameof(DIINSPECTION_COVER_LOCK));
            }
        }

        private IOPortDescripter<bool> _DIINSPECTION_COVER_UNLOCK = new IOPortDescripter<bool>(nameof(DIINSPECTION_COVER_UNLOCK), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIINSPECTION_COVER_UNLOCK
        {
            get { return _DIINSPECTION_COVER_UNLOCK; }
            set
            {
                if (value != this._DIINSPECTION_COVER_UNLOCK)
                    _DIINSPECTION_COVER_UNLOCK = value;
                NotifyPropertyChanged(nameof(DIINSPECTION_COVER_UNLOCK));
            }
        }

        private IOPortDescripter<bool> _DISCAN_SENSOR_OUT = new IOPortDescripter<bool>(nameof(DISCAN_SENSOR_OUT), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DISCAN_SENSOR_OUT
        {
            get { return _DISCAN_SENSOR_OUT; }
            set
            {
                if (value != this._DISCAN_SENSOR_OUT)
                    _DISCAN_SENSOR_OUT = value;
                NotifyPropertyChanged(nameof(DISCAN_SENSOR_OUT));
            }
        }

        private IOPortDescripter<bool> _DISCAN_SENSOR_IN = new IOPortDescripter<bool>(nameof(DISCAN_SENSOR_IN), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DISCAN_SENSOR_IN
        {
            get { return _DISCAN_SENSOR_IN; }
            set
            {
                if (value != this._DISCAN_SENSOR_IN)
                    _DISCAN_SENSOR_IN = value;
                NotifyPropertyChanged(nameof(DISCAN_SENSOR_IN));
            }
        }
        private IOPortDescripter<bool> _DIMECH_CLAMP = new IOPortDescripter<bool>(nameof(DIMECH_CLAMP), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DIMECH_CLAMP
        {
            get { return _DIMECH_CLAMP; }
            set
            {
                if (value != this._DIMECH_CLAMP)
                {
                    _DIMECH_CLAMP = value;
                    NotifyPropertyChanged(nameof(DIMECH_CLAMP));
                }
            }
        }

        private IOPortDescripter<bool> _DITP_LOCK = new IOPortDescripter<bool>(nameof(DITP_LOCK), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DITP_LOCK
        {
            get { return _DITP_LOCK; }
            set
            {
                if (value != this._DITP_LOCK)
                {
                    _DITP_LOCK = value;
                    NotifyPropertyChanged(nameof(DITP_LOCK));
                }
            }
        }

        private IOPortDescripter<bool> _DITP_UNLOCK = new IOPortDescripter<bool>(nameof(DITP_UNLOCK), EnumIOType.INPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DITP_UNLOCK
        {
            get { return _DITP_UNLOCK; }
            set
            {
                if (value != this._DITP_UNLOCK)
                {
                    _DITP_UNLOCK = value;
                    NotifyPropertyChanged(nameof(DITP_UNLOCK));
                }
            }
        }

        private IOPortDescripter<bool> _DICARDCHANGE_IN = new IOPortDescripter<bool>(nameof(DICARDCHANGE_IN), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DICARDCHANGE_IN
        {
            get { return _DICARDCHANGE_IN; }
            set
            {
                if (value != this._DICARDCHANGE_IN)
                {
                    _DICARDCHANGE_IN = value;
                    NotifyPropertyChanged(nameof(DICARDCHANGE_IN));
                }
            }
        }

        private IOPortDescripter<bool> _DICARDCHANGE_IDLE = new IOPortDescripter<bool>(nameof(DICARDCHANGE_IDLE), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DICARDCHANGE_IDLE
        {
            get { return _DICARDCHANGE_IDLE; }
            set
            {
                if (value != this._DICARDCHANGE_IDLE)
                {
                    _DICARDCHANGE_IDLE = value;
                    NotifyPropertyChanged(nameof(DICARDCHANGE_IDLE));
                }
            }
        }

        private IOPortDescripter<bool> _DICARDCHANGE_OUT = new IOPortDescripter<bool>(nameof(DICARDCHANGE_OUT), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DICARDCHANGE_OUT
        {
            get { return _DICARDCHANGE_OUT; }
            set
            {
                if (value != this._DICARDCHANGE_OUT)
                {
                    _DICARDCHANGE_OUT = value;
                    NotifyPropertyChanged(nameof(DICARDCHANGE_OUT));
                }
            }
        }

        private List<IOPortDescripter<bool>> _DI_FFU_ONLINES;

        public List<IOPortDescripter<bool>> DI_FFU_ONLINES
        {
            get { return _DI_FFU_ONLINES; }
            set
            {
                if (value != _DI_FFU_ONLINES)
                {
                    _DI_FFU_ONLINES = value;
                    NotifyPropertyChanged(nameof(DI_FFU_ONLINES));
                }
            }
        }
        /// <summary>
        /// Three leg vac sensor io in Opera 
        /// </summary>
        private IOPortDescripter<bool> _DI_ThreeLegVac = new IOPortDescripter<bool>(nameof(DI_ThreeLegVac), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DI_ThreeLegVac
        {
            get { return _DI_ThreeLegVac; }
            set
            {
                if (value != this._DI_ThreeLegVac)
                {
                    _DI_ThreeLegVac = value;
                    NotifyPropertyChanged(nameof(DI_ThreeLegVac));
                }
            }
        }
        #region ==> Group Prober Card Change

        #region ==> DIUPMODULE_LEFT_SENSOR : 왼쪽 UpModule(Chuck 옆에 있는 Card 지지대) 센서, true : Up, false : Down
        private IOPortDescripter<bool> _DIUPMODULE_LEFT_SENSOR = new IOPortDescripter<bool>(nameof(DIUPMODULE_LEFT_SENSOR), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIUPMODULE_LEFT_SENSOR
        {
            get { return _DIUPMODULE_LEFT_SENSOR; }
            set
            {
                if (value != this._DIUPMODULE_LEFT_SENSOR)
                {
                    _DIUPMODULE_LEFT_SENSOR = value;
                    NotifyPropertyChanged(nameof(DIUPMODULE_LEFT_SENSOR));
                }
            }
        }
        #endregion

        #region ==> DIUPMODULE_RIGHT_SENSOR : 오른쪽 UpModule(Chuck 옆에 있는 Card 지지대) 센서, true : Up, false : Down
        private IOPortDescripter<bool> _DIUPMODULE_RIGHT_SENSOR = new IOPortDescripter<bool>(nameof(DIUPMODULE_RIGHT_SENSOR), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIUPMODULE_RIGHT_SENSOR
        {
            get { return _DIUPMODULE_RIGHT_SENSOR; }
            set
            {
                if (value != this._DIUPMODULE_RIGHT_SENSOR)
                {
                    _DIUPMODULE_RIGHT_SENSOR = value;
                    NotifyPropertyChanged(nameof(DIUPMODULE_RIGHT_SENSOR));
                }
            }
        }
        #endregion

        #region ==> DIUPMODULE_CARDEXIST_SENSOR : UpModule에 달려 있는 반사 센서, Card 존재 여부 확인용, true : Exists, false : Not Exists
        private IOPortDescripter<bool> _DIUPMODULE_CARDEXIST_SENSOR = new IOPortDescripter<bool>(nameof(DIUPMODULE_CARDEXIST_SENSOR), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIUPMODULE_CARDEXIST_SENSOR
        {
            get { return _DIUPMODULE_CARDEXIST_SENSOR; }
            set
            {
                if (value != this._DIUPMODULE_CARDEXIST_SENSOR)
                {
                    _DIUPMODULE_CARDEXIST_SENSOR = value;
                    NotifyPropertyChanged(nameof(DIUPMODULE_CARDEXIST_SENSOR));
                }
            }
        }
        #endregion

        #region ==> DICARD_EXIST_ON_CARDPOD  : UpModule에 달려 있는 반사 센서, Card 존재 여부 확인용: 카드가 카드팟에 있을 때만 체크함. true : Exists, false : Not Exists
        private IOPortDescripter<bool> _DICARD_EXIST_ON_CARDPOD = new IOPortDescripter<bool>(nameof(DICARD_EXIST_ON_CARDPOD), EnumIOType.INPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DICARD_EXIST_ON_CARDPOD// DIUPMODULE_CARDEXIST_SENSOR를 EMUL로 쓰는 곳에서 NONE으로 판단하기위함.
        {
            get { return _DICARD_EXIST_ON_CARDPOD; }
            set
            {
                if (value != this._DICARD_EXIST_ON_CARDPOD)
                {
                    _DICARD_EXIST_ON_CARDPOD = value;
                    NotifyPropertyChanged(nameof(_DICARD_EXIST_ON_CARDPOD));
                }
            }
        }
        #endregion

        #region ==> DIUPMODULE_VACU_SENSOR : UpModule에 달려 있는 Vauum 센서, Card 존재 여부 확인용, true : Exists, false : Not Exists
        private IOPortDescripter<bool> _DIUPMODULE_VACU_SENSOR = new IOPortDescripter<bool>(nameof(DIUPMODULE_VACU_SENSOR), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIUPMODULE_VACU_SENSOR
        {
            get { return _DIUPMODULE_VACU_SENSOR; }
            set
            {
                if (value != this._DIUPMODULE_VACU_SENSOR)
                {
                    _DIUPMODULE_VACU_SENSOR = value;
                    NotifyPropertyChanged(nameof(DIUPMODULE_VACU_SENSOR));
                }
            }
        }
        #endregion

        #region ==> DIUPMODULE_TOUCH_SENSOR_L : UpModule에 달려 있는 touch 센서, Card 존재 여부 확인용 , true : Exists, false : Not Exists
        private IOPortDescripter<bool> _DIUPMODULE_TOUCH_SENSOR_L = new IOPortDescripter<bool>(nameof(DIUPMODULE_TOUCH_SENSOR_L), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_L
        {
            get { return _DIUPMODULE_TOUCH_SENSOR_L; }
            set
            {
                if (value != this._DIUPMODULE_TOUCH_SENSOR_L)
                {
                    _DIUPMODULE_TOUCH_SENSOR_L = value;
                    NotifyPropertyChanged(nameof(DIUPMODULE_TOUCH_SENSOR_L));
                }
            }
        }
        #endregion

        #region ==> DIUPMODULE_TOUCH_SENSOR : UpModule에 달려 있는 touch 센서, Card 존재 여부 확인용 , true : Exists, false : Not Exists
        private IOPortDescripter<bool> _DIUPMODULE_TOUCH_SENSOR_R = new IOPortDescripter<bool>(nameof(DIUPMODULE_TOUCH_SENSOR_R), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_R
        {
            get { return _DIUPMODULE_TOUCH_SENSOR_R; }
            set
            {
                if (value != this._DIUPMODULE_TOUCH_SENSOR_R)
                {
                    _DIUPMODULE_TOUCH_SENSOR_R = value;
                    NotifyPropertyChanged(nameof(DIUPMODULE_TOUCH_SENSOR_R));
                }
            }
        }
        #endregion

        #region ==> DIHOLDER_ON_TOPPLATE : Card Holder가  상판에 있는 상태, Rot lock 과 상관없이 Holder가 상판 Pos에 있다는 것을 의미함. , true : Exists, false : Not Exists
        private IOPortDescripter<bool> _DIHOLDER_ON_TOPPLATE = new IOPortDescripter<bool>(nameof(DIHOLDER_ON_TOPPLATE), EnumIOType.INPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DIHOLDER_ON_TOPPLATE
        {
            get { return _DIHOLDER_ON_TOPPLATE; }
            set
            {
                if (value != this._DIHOLDER_ON_TOPPLATE)
                {
                    _DIHOLDER_ON_TOPPLATE = value;
                    NotifyPropertyChanged(nameof(DIHOLDER_ON_TOPPLATE));
                }
            }
        }
        #endregion

        #region ==> DITESTER_DOCKING_SENSOR : Tester가 상판에 Docking 되어 있는지 확인하는 Sensor, true : Docking, false :UnDocking
        private IOPortDescripter<bool> _DITESTER_DOCKING_SENSOR = new IOPortDescripter<bool>(nameof(DITESTER_DOCKING_SENSOR), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DITESTER_DOCKING_SENSOR
        {
            get { return _DITESTER_DOCKING_SENSOR; }
            set
            {
                if (value != this._DITESTER_DOCKING_SENSOR)
                {
                    _DITESTER_DOCKING_SENSOR = value;
                    NotifyPropertyChanged(nameof(DITESTER_DOCKING_SENSOR));
                }
            }
        }
        #endregion

        #region ==> DITPLATE_PCLATCH_SENSOR_LOCK : 상판에 달려 있는 Card 낙하 방지 Lock 센서, true : Lock, false : UnLock
        private IOPortDescripter<bool> _DITPLATE_PCLATCH_SENSOR_LOCK = new IOPortDescripter<bool>(nameof(DITPLATE_PCLATCH_SENSOR_LOCK), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DITPLATE_PCLATCH_SENSOR_LOCK
        {
            get { return _DITPLATE_PCLATCH_SENSOR_LOCK; }
            set
            {
                if (value != this._DITPLATE_PCLATCH_SENSOR_LOCK)
                {
                    _DITPLATE_PCLATCH_SENSOR_LOCK = value;
                    NotifyPropertyChanged(nameof(DITPLATE_PCLATCH_SENSOR_LOCK));
                }
            }
        }
        #endregion

        #region ==> DITPLATE_PCLATCH_SENSOR_UNLOCK : 상판에 달려 있는 Card 낙하 방지 UnLock 센서, true : UnLock, false : Lock 
        private IOPortDescripter<bool> _DITPLATE_PCLATCH_SENSOR_UNLOCK = new IOPortDescripter<bool>(nameof(DITPLATE_PCLATCH_SENSOR_UNLOCK), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DITPLATE_PCLATCH_SENSOR_UNLOCK
        {
            get { return _DITPLATE_PCLATCH_SENSOR_UNLOCK; }
            set
            {
                if (value != this._DITPLATE_PCLATCH_SENSOR_UNLOCK)
                {
                    _DITPLATE_PCLATCH_SENSOR_UNLOCK = value;
                    NotifyPropertyChanged(nameof(DITPLATE_PCLATCH_SENSOR_UNLOCK));
                }
            }
        }
        #endregion

        #region ==> DIPOGOCARD_VACU_SENSOR : POGO에 Card와 접촉 되는 면의 Vacuum 센서, Card가 POGO에 흡착 되어 있는지 확인 하는 용도 : true : 흡착 됨, false : 흑착 안됨
        private IOPortDescripter<bool> _DIPOGOCARD_VACU_SENSOR = new IOPortDescripter<bool>(nameof(DIPOGOCARD_VACU_SENSOR), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIPOGOCARD_VACU_SENSOR
        {
            get { return _DIPOGOCARD_VACU_SENSOR; }
            set
            {
                if (value != this._DIPOGOCARD_VACU_SENSOR)
                {
                    _DIPOGOCARD_VACU_SENSOR = value;
                    NotifyPropertyChanged(nameof(DIPOGOCARD_VACU_SENSOR));
                }
            }
        }
        #endregion

        #region ==> DIPOGOTESTER_VACU_SENSOR : 상판에 Tester와 접촉 되는 면의 Vacuum 센서, Tester가 상판에 흡착 되어 있는지 확인 하는 용도 : true : 흡착 됨, false : 흑착 안됨
        private IOPortDescripter<bool> _DIPOGOTESTER_VACU_SENSOR = new IOPortDescripter<bool>(nameof(DIPOGOTESTER_VACU_SENSOR), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIPOGOTESTER_VACU_SENSOR
        {
            get { return _DIPOGOTESTER_VACU_SENSOR; }
            set
            {
                if (value != this._DIPOGOTESTER_VACU_SENSOR)
                {
                    _DIPOGOTESTER_VACU_SENSOR = value;
                    NotifyPropertyChanged(nameof(DIPOGOTESTER_VACU_SENSOR));
                }
            }
        }
        #endregion

        #region ==> DITPLATEIN_SENSOR : 상판이 Docking되어 있는지 확인하는 센서, true : Docking 됨, flase : UnDocking 됨.
        private IOPortDescripter<bool> _DITPLATEIN_SENSOR = new IOPortDescripter<bool>(nameof(DITPLATEIN_SENSOR), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DITPLATEIN_SENSOR
        {
            get { return _DITPLATEIN_SENSOR; }
            set
            {
                if (value != this._DITPLATEIN_SENSOR)
                {
                    _DITPLATEIN_SENSOR = value;
                    NotifyPropertyChanged(nameof(DITPLATEIN_SENSOR));
                }
            }
        }
        #endregion

        #endregion
        #region ==> Bernoulli handler
        private IOPortDescripter<bool> _DIBERNOULLI_HANDLER_UP = new IOPortDescripter<bool>(nameof(DIBERNOULLI_HANDLER_UP), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIBERNOULLI_HANDLER_UP
        {
            get { return _DIBERNOULLI_HANDLER_UP; }
            set
            {
                if (value != this._DIBERNOULLI_HANDLER_UP)
                {
                    _DIBERNOULLI_HANDLER_UP = value;
                    NotifyPropertyChanged(nameof(DIBERNOULLI_HANDLER_UP));
                }
            }
        }

        private IOPortDescripter<bool> _DIBERNOULLI_HANDLER_DOWN = new IOPortDescripter<bool>(nameof(DIBERNOULLI_HANDLER_DOWN), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIBERNOULLI_HANDLER_DOWN
        {
            get { return _DIBERNOULLI_HANDLER_DOWN; }
            set
            {
                if (value != this._DIBERNOULLI_HANDLER_DOWN)
                {
                    _DIBERNOULLI_HANDLER_DOWN = value;
                    NotifyPropertyChanged(nameof(_DIBERNOULLI_HANDLER_DOWN));
                }
            }
        }
        private IOPortDescripter<bool> _DIBERNOULLIWAFER_EXIST = new IOPortDescripter<bool>(nameof(DIBERNOULLIWAFER_EXIST), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIBERNOULLIWAFER_EXIST
        {
            get { return _DIBERNOULLIWAFER_EXIST; }
            set
            {
                if (value != this._DIBERNOULLIWAFER_EXIST)
                {
                    _DIBERNOULLIWAFER_EXIST = value;
                    NotifyPropertyChanged(nameof(DIBERNOULLIWAFER_EXIST));
                }
            }
        }
        private IOPortDescripter<bool> _DIBERNOULLI_ALIGN1_ON = new IOPortDescripter<bool>(nameof(DIBERNOULLI_ALIGN1_ON), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIBERNOULLI_ALIGN1_ON
        {
            get { return _DIBERNOULLI_ALIGN1_ON; }
            set
            {
                if (value != this._DIBERNOULLI_ALIGN1_ON)
                {
                    _DIBERNOULLI_ALIGN1_ON = value;
                    NotifyPropertyChanged(nameof(DIBERNOULLI_ALIGN1_ON));
                }
            }
        }
        private IOPortDescripter<bool> _DIBERNOULLI_ALIGN1_OFF = new IOPortDescripter<bool>(nameof(DIBERNOULLI_ALIGN1_OFF), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIBERNOULLI_ALIGN1_OFF
        {
            get { return _DIBERNOULLI_ALIGN1_OFF; }
            set
            {
                if (value != this._DIBERNOULLI_ALIGN1_OFF)
                {
                    _DIBERNOULLI_ALIGN1_OFF = value;
                    NotifyPropertyChanged(nameof(DIBERNOULLI_ALIGN1_OFF));
                }
            }
        }
        private IOPortDescripter<bool> _DIBERNOULLI_ALIGN2_ON = new IOPortDescripter<bool>(nameof(DIBERNOULLI_ALIGN2_ON), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIBERNOULLI_ALIGN2_ON
        {
            get { return _DIBERNOULLI_ALIGN2_ON; }
            set
            {
                if (value != this._DIBERNOULLI_ALIGN2_ON)
                {
                    _DIBERNOULLI_ALIGN2_ON = value;
                    NotifyPropertyChanged(nameof(DIBERNOULLI_ALIGN2_ON));
                }
            }
        }
        private IOPortDescripter<bool> _DIBERNOULLI_ALIGN2_OFF = new IOPortDescripter<bool>(nameof(DIBERNOULLI_ALIGN2_OFF), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIBERNOULLI_ALIGN2_OFF
        {
            get { return _DIBERNOULLI_ALIGN2_OFF; }
            set
            {
                if (value != this._DIBERNOULLI_ALIGN2_OFF)
                {
                    _DIBERNOULLI_ALIGN2_OFF = value;
                    NotifyPropertyChanged(nameof(DIBERNOULLI_ALIGN2_OFF));
                }
            }
        }
        private IOPortDescripter<bool> _DIBERNOULLI_ALIGN3_ON = new IOPortDescripter<bool>(nameof(DIBERNOULLI_ALIGN3_ON), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIBERNOULLI_ALIGN3_ON
        {
            get { return _DIBERNOULLI_ALIGN3_ON; }
            set
            {
                if (value != this._DIBERNOULLI_ALIGN3_ON)
                {
                    _DIBERNOULLI_ALIGN3_ON = value;
                    NotifyPropertyChanged(nameof(DIBERNOULLI_ALIGN3_ON));
                }
            }
        }
        private IOPortDescripter<bool> _DIBERNOULLI_ALIGN3_OFF = new IOPortDescripter<bool>(nameof(DIBERNOULLI_ALIGN3_OFF), EnumIOType.INPUT, EnumIOOverride.NONE);
        public IOPortDescripter<bool> DIBERNOULLI_ALIGN3_OFF
        {
            get { return _DIBERNOULLI_ALIGN3_OFF; }
            set
            {
                if (value != this._DIBERNOULLI_ALIGN3_OFF)
                {
                    _DIBERNOULLI_ALIGN3_OFF = value;
                    NotifyPropertyChanged(nameof(DIBERNOULLI_ALIGN3_OFF));
                }
            }
        }
        #endregion

        #region ==> Stage InterLock
        private IOPortDescripter<bool> _DI_BACKSIDE_DOOR_OPEN = new IOPortDescripter<bool>(nameof(DI_BACKSIDE_DOOR_OPEN), EnumIOType.INPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DI_BACKSIDE_DOOR_OPEN
        {
            get { return _DI_BACKSIDE_DOOR_OPEN; }
            set
            {
                if (value != this._DI_BACKSIDE_DOOR_OPEN)
                {
                    _DI_BACKSIDE_DOOR_OPEN = value;
                    NotifyPropertyChanged(nameof(DI_BACKSIDE_DOOR_OPEN));
                }
            }
        }

        private IOPortDescripter<bool> _DITESTERHEAD_PURGE = new IOPortDescripter<bool>(nameof(DITESTERHEAD_PURGE), EnumIOType.INPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DITESTERHEAD_PURGE
        {
            get { return _DITESTERHEAD_PURGE; }
            set
            {
                if (value != this._DITESTERHEAD_PURGE)
                {
                    _DITESTERHEAD_PURGE = value;
                    NotifyPropertyChanged(nameof(_DITESTERHEAD_PURGE));
                }
            }
        }

        private IOPortDescripter<bool> _DIEXTRA_CHUCK_VAC_READY = new IOPortDescripter<bool>(nameof(DIEXTRA_CHUCK_VAC_READY), EnumIOType.INPUT, EnumIOOverride.EMUL) { MaintainTime = new Element<long>(100), TimeOut = new Element<long>(1000) };

        public IOPortDescripter<bool> DIEXTRA_CHUCK_VAC_READY
        {
            get { return _DIEXTRA_CHUCK_VAC_READY; }
            set
            {
                if (value != this._DIEXTRA_CHUCK_VAC_READY)
                {
                    _DIEXTRA_CHUCK_VAC_READY = value;
                    NotifyPropertyChanged(nameof(_DIEXTRA_CHUCK_VAC_READY));
                }
            }
        }
        #endregion
        #endregion

        #region Sorter Inputs

        private IOPortDescripter<bool> _DI_CH0P00 = new IOPortDescripter<bool>(nameof(DI_CH0P00), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P00
        {
            get { return _DI_CH0P00; }
            set
            {
                if (value != this._DI_CH0P00)
                {
                    _DI_CH0P00 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P00));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P01 = new IOPortDescripter<bool>(nameof(DI_CH0P01), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P01
        {
            get { return _DI_CH0P01; }
            set
            {
                if (value != this._DI_CH0P01)
                {
                    _DI_CH0P01 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P01));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P02 = new IOPortDescripter<bool>(nameof(DI_CH0P02), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P02
        {
            get { return _DI_CH0P02; }
            set
            {
                if (value != this._DI_CH0P02)
                {
                    _DI_CH0P02 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P02));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P03 = new IOPortDescripter<bool>(nameof(DI_CH0P03), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P03
        {
            get { return _DI_CH0P03; }
            set
            {
                if (value != this._DI_CH0P03)
                {
                    _DI_CH0P03 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P03));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P04 = new IOPortDescripter<bool>(nameof(DI_CH0P04), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P04
        {
            get { return _DI_CH0P04; }
            set
            {
                if (value != this._DI_CH0P04)
                {
                    _DI_CH0P04 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P04));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P05 = new IOPortDescripter<bool>(nameof(DI_CH0P05), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P05
        {
            get { return _DI_CH0P05; }
            set
            {
                if (value != this._DI_CH0P05)
                {
                    _DI_CH0P05 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P05));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P06 = new IOPortDescripter<bool>(nameof(DI_CH0P06), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P06
        {
            get { return _DI_CH0P06; }
            set
            {
                if (value != this._DI_CH0P06)
                {
                    _DI_CH0P06 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P06));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P07 = new IOPortDescripter<bool>(nameof(DI_CH0P07), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P07
        {
            get { return _DI_CH0P07; }
            set
            {
                if (value != this._DI_CH0P07)
                {
                    _DI_CH0P07 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P07));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P08 = new IOPortDescripter<bool>(nameof(DI_CH0P08), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P08
        {
            get { return _DI_CH0P08; }
            set
            {
                if (value != this._DI_CH0P08)
                {
                    _DI_CH0P08 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P08));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P09 = new IOPortDescripter<bool>(nameof(DI_CH0P09), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P09
        {
            get { return _DI_CH0P09; }
            set
            {
                if (value != this._DI_CH0P09)
                {
                    _DI_CH0P09 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P09));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P10 = new IOPortDescripter<bool>(nameof(DI_CH0P10), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P10
        {
            get { return _DI_CH0P10; }
            set
            {
                if (value != this._DI_CH0P10)
                {
                    _DI_CH0P10 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P10));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P11 = new IOPortDescripter<bool>(nameof(DI_CH0P11), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P11
        {
            get { return _DI_CH0P11; }
            set
            {
                if (value != this._DI_CH0P11)
                {
                    _DI_CH0P11 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P11));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P12 = new IOPortDescripter<bool>(nameof(DI_CH0P12), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P12
        {
            get { return _DI_CH0P12; }
            set
            {
                if (value != this._DI_CH0P12)
                {
                    _DI_CH0P12 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P12));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P13 = new IOPortDescripter<bool>(nameof(DI_CH0P13), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P13
        {
            get { return _DI_CH0P13; }
            set
            {
                if (value != this._DI_CH0P13)
                {
                    _DI_CH0P13 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P13));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P14 = new IOPortDescripter<bool>(nameof(DI_CH0P14), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P14
        {
            get { return _DI_CH0P14; }
            set
            {
                if (value != this._DI_CH0P14)
                {
                    _DI_CH0P14 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P14));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH0P15 = new IOPortDescripter<bool>(nameof(DI_CH0P15), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH0P15
        {
            get { return _DI_CH0P15; }
            set
            {
                if (value != this._DI_CH0P15)
                {
                    _DI_CH0P15 = value;
                    NotifyPropertyChanged(nameof(DI_CH0P15));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P00 = new IOPortDescripter<bool>(nameof(DI_CH1P00), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P00
        {
            get { return _DI_CH1P00; }
            set
            {
                if (value != this._DI_CH1P00)
                {
                    _DI_CH1P00 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P00));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P01 = new IOPortDescripter<bool>(nameof(DI_CH1P01), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P01
        {
            get { return _DI_CH1P01; }
            set
            {
                if (value != this._DI_CH1P01)
                {
                    _DI_CH1P01 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P01));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P02 = new IOPortDescripter<bool>(nameof(DI_CH1P02), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P02
        {
            get { return _DI_CH1P02; }
            set
            {
                if (value != this._DI_CH1P02)
                {
                    _DI_CH1P02 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P02));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P03 = new IOPortDescripter<bool>(nameof(DI_CH1P03), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P03
        {
            get { return _DI_CH1P03; }
            set
            {
                if (value != this._DI_CH1P03)
                {
                    _DI_CH1P03 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P03));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P04 = new IOPortDescripter<bool>(nameof(DI_CH1P04), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P04
        {
            get { return _DI_CH1P04; }
            set
            {
                if (value != this._DI_CH1P04)
                {
                    _DI_CH1P04 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P04));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P05 = new IOPortDescripter<bool>(nameof(DI_CH1P05), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P05
        {
            get { return _DI_CH1P05; }
            set
            {
                if (value != this._DI_CH1P05)
                {
                    _DI_CH1P05 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P05));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P06 = new IOPortDescripter<bool>(nameof(DI_CH1P06), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P06
        {
            get { return _DI_CH1P06; }
            set
            {
                if (value != this._DI_CH1P06)
                {
                    _DI_CH1P06 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P06));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P07 = new IOPortDescripter<bool>(nameof(DI_CH1P07), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P07
        {
            get { return _DI_CH1P07; }
            set
            {
                if (value != this._DI_CH1P07)
                {
                    _DI_CH1P07 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P07));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P08 = new IOPortDescripter<bool>(nameof(DI_CH1P08), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P08
        {
            get { return _DI_CH1P08; }
            set
            {
                if (value != this._DI_CH1P08)
                {
                    _DI_CH1P08 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P08));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P09 = new IOPortDescripter<bool>(nameof(DI_CH1P09), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P09
        {
            get { return _DI_CH1P09; }
            set
            {
                if (value != this._DI_CH1P09)
                {
                    _DI_CH1P09 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P09));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P10 = new IOPortDescripter<bool>(nameof(DI_CH1P10), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P10
        {
            get { return _DI_CH1P10; }
            set
            {
                if (value != this._DI_CH1P10)
                {
                    _DI_CH1P10 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P10));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P11 = new IOPortDescripter<bool>(nameof(DI_CH1P11), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P11
        {
            get { return _DI_CH1P11; }
            set
            {
                if (value != this._DI_CH1P11)
                {
                    _DI_CH1P11 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P11));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P12 = new IOPortDescripter<bool>(nameof(DI_CH1P12), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P12
        {
            get { return _DI_CH1P12; }
            set
            {
                if (value != this._DI_CH1P12)
                {
                    _DI_CH1P12 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P12));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P13 = new IOPortDescripter<bool>(nameof(DI_CH1P13), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P13
        {
            get { return _DI_CH1P13; }
            set
            {
                if (value != this._DI_CH1P13)
                {
                    _DI_CH1P13 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P13));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P14 = new IOPortDescripter<bool>(nameof(DI_CH1P14), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P14
        {
            get { return _DI_CH1P14; }
            set
            {
                if (value != this._DI_CH1P14)
                {
                    _DI_CH1P14 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P14));
                }
            }
        }

        private IOPortDescripter<bool> _DI_CH1P15 = new IOPortDescripter<bool>(nameof(DI_CH1P15), EnumIOType.INPUT, EnumIOOverride.NONE);

        public IOPortDescripter<bool> DI_CH1P15
        {
            get { return _DI_CH1P15; }
            set
            {
                if (value != this._DI_CH1P15)
                {
                    _DI_CH1P15 = value;
                    NotifyPropertyChanged(nameof(DI_CH1P15));
                }
            }
        }
        #endregion

        // 250911 LJH 항목추가
        private IOPortDescripter<bool> _DICCARMVAC = new IOPortDescripter<bool>("DICCARMVAC", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICCARMVAC
        {
            get { return _DICCARMVAC; }
            set
            {
                if (value != this._DICCARMVAC)
                {
                    _DICCARMVAC = value;
                    NotifyPropertyChanged(nameof(DICCARMVAC));
                }
            }
        }

        private IOPortDescripter<bool> _DIFixTrays_0 = new IOPortDescripter<bool>("DIFixTrays_0", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFixTrays_0
        {
            get { return _DIFixTrays_0; }
            set
            {
                if (value != this._DIFixTrays_0)
                {
                    _DIFixTrays_0 = value;
                    NotifyPropertyChanged(nameof(_DIFixTrays_0));
                }
            }
        }
        private IOPortDescripter<bool> _DI6inchWaferOnFixTs_0 = new IOPortDescripter<bool>("DI6inchWaferOnFixTs_0", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI6inchWaferOnFixTs_0
        {
            get { return _DI6inchWaferOnFixTs_0; }
            set
            {
                if (value != this._DI6inchWaferOnFixTs_0)
                {
                    _DI6inchWaferOnFixTs_0 = value;
                    NotifyPropertyChanged(nameof(DI6inchWaferOnFixTs_0));
                }
            }
        }
        private IOPortDescripter<bool> _DI8inchWaferOnFixTs_0 = new IOPortDescripter<bool>("DI8inchWaferOnFixTs_0", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8inchWaferOnFixTs_0
        {
            get { return _DI8inchWaferOnFixTs_0; }
            set
            {
                if (value != this._DI8inchWaferOnFixTs_0)
                {
                    _DI8inchWaferOnFixTs_0 = value;
                    NotifyPropertyChanged(nameof(DI8inchWaferOnFixTs_0));
                }
            }
        }

        private IOPortDescripter<bool> _DIFixTrays_1 = new IOPortDescripter<bool>("DIFixTrays_1", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFixTrays_1
        {
            get { return _DIFixTrays_1; }
            set
            {
                if (value != this._DIFixTrays_1)
                {
                    _DIFixTrays_1 = value;
                    NotifyPropertyChanged(nameof(DIFixTrays_1));
                }
            }
        }
        private IOPortDescripter<bool> _DI6inchWaferOnFixTs_1 = new IOPortDescripter<bool>("DI6inchWaferOnFixTs_1", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI6inchWaferOnFixTs_1
        {
            get { return _DI6inchWaferOnFixTs_1; }
            set
            {
                if (value != this._DI6inchWaferOnFixTs_1)
                {
                    _DI6inchWaferOnFixTs_1 = value;
                    NotifyPropertyChanged(nameof(DI6inchWaferOnFixTs_1));
                }
            }
        }
        private IOPortDescripter<bool> _DI8inchWaferOnFixTs_1 = new IOPortDescripter<bool>("DI8inchWaferOnFixTs_1", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8inchWaferOnFixTs_1
        {
            get { return _DI8inchWaferOnFixTs_1; }
            set
            {
                if (value != this._DI8inchWaferOnFixTs_1)
                {
                    _DI8inchWaferOnFixTs_1 = value;
                    NotifyPropertyChanged(nameof(DI8inchWaferOnFixTs_1));
                }
            }
        }

        private IOPortDescripter<bool> _DIFixTrays_2 = new IOPortDescripter<bool>("DIFixTrays_2", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFixTrays_2
        {
            get { return _DIFixTrays_2; }
            set
            {
                if (value != this._DIFixTrays_2)
                {
                    _DIFixTrays_2 = value;
                    NotifyPropertyChanged(nameof(DIFixTrays_2));
                }
            }
        }
        private IOPortDescripter<bool> _DI6inchWaferOnFixTs_2 = new IOPortDescripter<bool>("DI6inchWaferOnFixTs_2", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI6inchWaferOnFixTs_2
        {
            get { return _DI6inchWaferOnFixTs_2; }
            set
            {
                if (value != this._DI6inchWaferOnFixTs_2)
                {
                    _DI6inchWaferOnFixTs_2 = value;
                    NotifyPropertyChanged(nameof(DI6inchWaferOnFixTs_2));
                }
            }
        }
        private IOPortDescripter<bool> _DI8inchWaferOnFixTs_2 = new IOPortDescripter<bool>("DI8inchWaferOnFixTs_2", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8inchWaferOnFixTs_2
        {
            get { return _DI8inchWaferOnFixTs_2; }
            set
            {
                if (value != this._DI8inchWaferOnFixTs_2)
                {
                    _DI8inchWaferOnFixTs_2 = value;
                    NotifyPropertyChanged(nameof(DI8inchWaferOnFixTs_2));
                }
            }
        }
        private IOPortDescripter<bool> _DIFixTrays_3 = new IOPortDescripter<bool>("DIFixTrays_3", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFixTrays_3
        {
            get { return _DIFixTrays_3; }
            set
            {
                if (value != this._DIFixTrays_3)
                {
                    _DIFixTrays_3 = value;
                    NotifyPropertyChanged(nameof(DIFixTrays_3));
                }
            }
        }
        private IOPortDescripter<bool> _DI6inchWaferOnFixTs_3 = new IOPortDescripter<bool>("DI6inchWaferOnFixTs_3", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI6inchWaferOnFixTs_3
        {
            get { return _DI6inchWaferOnFixTs_3; }
            set
            {
                if (value != this._DI6inchWaferOnFixTs_3)
                {
                    _DI6inchWaferOnFixTs_3 = value;
                    NotifyPropertyChanged(nameof(DI6inchWaferOnFixTs_3));
                }
            }
        }
        private IOPortDescripter<bool> _DI8inchWaferOnFixTs_3 = new IOPortDescripter<bool>("DI8inchWaferOnFixTs_3", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8inchWaferOnFixTs_3
        {
            get { return _DI8inchWaferOnFixTs_3; }
            set
            {
                if (value != this._DI8inchWaferOnFixTs_3)
                {
                    _DI8inchWaferOnFixTs_3 = value;
                    NotifyPropertyChanged(nameof(DI8inchWaferOnFixTs_3));
                }
            }
        }
        private IOPortDescripter<bool> _DIFixTrays_4 = new IOPortDescripter<bool>("DIFixTrays_4", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFixTrays_4
        {
            get { return _DIFixTrays_4; }
            set
            {
                if (value != this._DIFixTrays_4)
                {
                    _DIFixTrays_4 = value;
                    NotifyPropertyChanged(nameof(DIFixTrays_4));
                }
            }
        }
        private IOPortDescripter<bool> _DI6inchWaferOnFixTs_4 = new IOPortDescripter<bool>("DI6inchWaferOnFixTs_4", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI6inchWaferOnFixTs_4
        {
            get { return _DI6inchWaferOnFixTs_4; }
            set
            {
                if (value != this._DI6inchWaferOnFixTs_4)
                {
                    _DI6inchWaferOnFixTs_4 = value;
                    NotifyPropertyChanged(nameof(DI6inchWaferOnFixTs_4));
                }
            }
        }
        private IOPortDescripter<bool> _DI8inchWaferOnFixTs_4 = new IOPortDescripter<bool>("DI8inchWaferOnFixTs_4", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8inchWaferOnFixTs_4
        {
            get { return _DI8inchWaferOnFixTs_4; }
            set
            {
                if (value != this._DI8inchWaferOnFixTs_4)
                {
                    _DI8inchWaferOnFixTs_4 = value;
                    NotifyPropertyChanged(nameof(DI8inchWaferOnFixTs_4));
                }
            }
        }
        private IOPortDescripter<bool> _DIFixTrays_5 = new IOPortDescripter<bool>("DIFixTrays_5", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFixTrays_5
        {
            get { return _DIFixTrays_5; }
            set
            {
                if (value != this._DIFixTrays_5)
                {
                    _DIFixTrays_5 = value;
                    NotifyPropertyChanged(nameof(DIFixTrays_5));
                }
            }
        }
        private IOPortDescripter<bool> _DI6inchWaferOnFixTs_5 = new IOPortDescripter<bool>("DI6inchWaferOnFixTs_5", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI6inchWaferOnFixTs_5
        {
            get { return _DI6inchWaferOnFixTs_5; }
            set
            {
                if (value != this._DI6inchWaferOnFixTs_5)
                {
                    _DI6inchWaferOnFixTs_5 = value;
                    NotifyPropertyChanged(nameof(DI6inchWaferOnFixTs_5));
                }
            }
        }
        private IOPortDescripter<bool> _DI8inchWaferOnFixTs_5 = new IOPortDescripter<bool>("DI8inchWaferOnFixTs_5", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8inchWaferOnFixTs_5
        {
            get { return _DI8inchWaferOnFixTs_5; }
            set
            {
                if (value != this._DI8inchWaferOnFixTs_5)
                {
                    _DI8inchWaferOnFixTs_5 = value;
                    NotifyPropertyChanged(nameof(DI8inchWaferOnFixTs_5));
                }
            }
        }
        private IOPortDescripter<bool> _DIFixTrays_6 = new IOPortDescripter<bool>("DIFixTrays_6", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFixTrays_6
        {
            get { return _DIFixTrays_6; }
            set
            {
                if (value != this._DIFixTrays_6)
                {
                    _DIFixTrays_6 = value;
                    NotifyPropertyChanged(nameof(DIFixTrays_6));
                }
            }
        }
        private IOPortDescripter<bool> _DI6inchWaferOnFixTs_6 = new IOPortDescripter<bool>("DI6inchWaferOnFixTs_6", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI6inchWaferOnFixTs_6
        {
            get { return _DI6inchWaferOnFixTs_6; }
            set
            {
                if (value != this._DI6inchWaferOnFixTs_6)
                {
                    _DI6inchWaferOnFixTs_6 = value;
                    NotifyPropertyChanged(nameof(DI6inchWaferOnFixTs_6));
                }
            }
        }
        private IOPortDescripter<bool> _DI8inchWaferOnFixTs_6 = new IOPortDescripter<bool>("DI8inchWaferOnFixTs_6", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8inchWaferOnFixTs_6
        {
            get { return _DI8inchWaferOnFixTs_6; }
            set
            {
                if (value != this._DI8inchWaferOnFixTs_6)
                {
                    _DI8inchWaferOnFixTs_6 = value;
                    NotifyPropertyChanged(nameof(DI8inchWaferOnFixTs_6));
                }
            }
        }
        private IOPortDescripter<bool> _DIFixTrays_7 = new IOPortDescripter<bool>("DIFixTrays_7", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFixTrays_7
        {
            get { return _DIFixTrays_7; }
            set
            {
                if (value != this._DIFixTrays_7)
                {
                    _DIFixTrays_7 = value;
                    NotifyPropertyChanged(nameof(DIFixTrays_7));
                }
            }
        }
        private IOPortDescripter<bool> _DI6inchWaferOnFixTs_7 = new IOPortDescripter<bool>("DI6inchWaferOnFixTs_7", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI6inchWaferOnFixTs_7
        {
            get { return _DI6inchWaferOnFixTs_7; }
            set
            {
                if (value != this._DI6inchWaferOnFixTs_7)
                {
                    _DI6inchWaferOnFixTs_7 = value;
                    NotifyPropertyChanged(nameof(DI6inchWaferOnFixTs_7));
                }
            }
        }
        private IOPortDescripter<bool> _DI8inchWaferOnFixTs_7 = new IOPortDescripter<bool>("DI8inchWaferOnFixTs_7", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8inchWaferOnFixTs_7
        {
            get { return _DI8inchWaferOnFixTs_7; }
            set
            {
                if (value != this._DI8inchWaferOnFixTs_7)
                {
                    _DI8inchWaferOnFixTs_7 = value;
                    NotifyPropertyChanged(nameof(DI8inchWaferOnFixTs_7));
                }
            }
        }
        private IOPortDescripter<bool> _DIFixTrays_8 = new IOPortDescripter<bool>("DIFixTrays_8", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIFixTrays_8
        {
            get { return _DIFixTrays_8; }
            set
            {
                if (value != this._DIFixTrays_8)
                {
                    _DIFixTrays_8 = value;
                    NotifyPropertyChanged(nameof(DIFixTrays_8));
                }
            }
        }
        private IOPortDescripter<bool> _DI6inchWaferOnFixTs_8 = new IOPortDescripter<bool>("DI6inchWaferOnFixTs_8", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI6inchWaferOnFixTs_8
        {
            get { return _DI6inchWaferOnFixTs_8; }
            set
            {
                if (value != this._DI6inchWaferOnFixTs_8)
                {
                    _DI6inchWaferOnFixTs_8 = value;
                    NotifyPropertyChanged(nameof(DI6inchWaferOnFixTs_8));
                }
            }
        }
        private IOPortDescripter<bool> _DI8inchWaferOnFixTs_8 = new IOPortDescripter<bool>("DI8inchWaferOnFixTs_8", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8inchWaferOnFixTs_8
        {
            get { return _DI8inchWaferOnFixTs_8; }
            set
            {
                if (value != this._DI8inchWaferOnFixTs_8)
                {
                    _DI8inchWaferOnFixTs_8 = value;
                    NotifyPropertyChanged(nameof(DI8inchWaferOnFixTs_8));
                }
            }
        }
        private IOPortDescripter<bool> _DI6inchWaferOnInSPs_0 = new IOPortDescripter<bool>("DI6inchWaferOnInSPs_0", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI6inchWaferOnInSPs_0
        {
            get { return _DI6inchWaferOnInSPs_0; }
            set
            {
                if (value != this._DI6inchWaferOnInSPs_0)
                {
                    _DI6inchWaferOnInSPs_0 = value;
                    NotifyPropertyChanged(nameof(DI6inchWaferOnInSPs_0));
                }
            }
        }
        private IOPortDescripter<bool> _DI8inchWaferOnInSPs_0 = new IOPortDescripter<bool>("DI8inchWaferOnInSPs_0", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8inchWaferOnInSPs_0
        {
            get { return _DI8inchWaferOnInSPs_0; }
            set
            {
                if (value != this._DI8inchWaferOnInSPs_0)
                {
                    _DI8inchWaferOnInSPs_0 = value;
                    NotifyPropertyChanged(nameof(DI8inchWaferOnInSPs_0));
                }
            }
        }
        private IOPortDescripter<bool> _DIWaferOnInSPs_0 = new IOPortDescripter<bool>("DIWaferOnInSPs_0", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWaferOnInSPs_0
        {
            get { return _DIWaferOnInSPs_0; }
            set
            {
                if (value != this._DIWaferOnInSPs_0)
                {
                    _DIWaferOnInSPs_0 = value;
                    NotifyPropertyChanged(nameof(DIWaferOnInSPs_0));
                }
            }
        }
        private IOPortDescripter<bool> _DIOpenInSPs_0 = new IOPortDescripter<bool>("DIOpenInSPs_0", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIOpenInSPs_0
        {
            get { return _DIOpenInSPs_0; }
            set
            {
                if (value != this._DIOpenInSPs_0)
                {
                    _DIOpenInSPs_0 = value;
                    NotifyPropertyChanged(nameof(DIOpenInSPs_0));
                }
            }
        }
        private IOPortDescripter<bool> _DI6inchWaferOnInSPs_1 = new IOPortDescripter<bool>("DI6inchWaferOnInSPs_1", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI6inchWaferOnInSPs_1
        {
            get { return _DI6inchWaferOnInSPs_1; }
            set
            {
                if (value != this._DI6inchWaferOnInSPs_1)
                {
                    _DI6inchWaferOnInSPs_1 = value;
                    NotifyPropertyChanged(nameof(DI6inchWaferOnInSPs_1));
                }
            }
        }
        private IOPortDescripter<bool> _DI8inchWaferOnInSPs_1 = new IOPortDescripter<bool>("DI8inchWaferOnInSPs_1", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8inchWaferOnInSPs_1
        {
            get { return _DI8inchWaferOnInSPs_1; }
            set
            {
                if (value != this._DI8inchWaferOnInSPs_1)
                {
                    _DI8inchWaferOnInSPs_1 = value;
                    NotifyPropertyChanged(nameof(DI8inchWaferOnInSPs_1));
                }
            }
        }
        private IOPortDescripter<bool> _DIWaferOnInSPs_1 = new IOPortDescripter<bool>("DIWaferOnInSPs_1", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWaferOnInSPs_1
        {
            get { return _DIWaferOnInSPs_1; }
            set
            {
                if (value != this._DIWaferOnInSPs_1)
                {
                    _DIWaferOnInSPs_1 = value;
                    NotifyPropertyChanged(nameof(DIWaferOnInSPs_1));
                }
            }
        }
        private IOPortDescripter<bool> _DIOpenInSPs_1 = new IOPortDescripter<bool>("DIOpenInSPs_1", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIOpenInSPs_1
        {
            get { return _DIOpenInSPs_1; }
            set
            {
                if (value != this._DIOpenInSPs_1)
                {
                    _DIOpenInSPs_1 = value;
                    NotifyPropertyChanged(nameof(DIOpenInSPs_1));
                }
            }
        }
        private IOPortDescripter<bool> _DI6inchWaferOnInSPs_2 = new IOPortDescripter<bool>("DI6inchWaferOnInSPs_2", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI6inchWaferOnInSPs_2
        {
            get { return _DI6inchWaferOnInSPs_2; }
            set
            {
                if (value != this._DI6inchWaferOnInSPs_2)
                {
                    _DI6inchWaferOnInSPs_2 = value;
                    NotifyPropertyChanged(nameof(DI6inchWaferOnInSPs_2));
                }
            }
        }
        private IOPortDescripter<bool> _DI8inchWaferOnInSPs_2 = new IOPortDescripter<bool>("DI8inchWaferOnInSPs_2", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI8inchWaferOnInSPs_2
        {
            get { return _DI8inchWaferOnInSPs_2; }
            set
            {
                if (value != this._DI8inchWaferOnInSPs_2)
                {
                    _DI8inchWaferOnInSPs_2 = value;
                    NotifyPropertyChanged(nameof(DI8inchWaferOnInSPs_2));
                }
            }
        }
        private IOPortDescripter<bool> _DIWaferOnInSPs_2 = new IOPortDescripter<bool>("DIWaferOnInSPs_2", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWaferOnInSPs_2
        {
            get { return _DIWaferOnInSPs_2; }
            set
            {
                if (value != this._DIWaferOnInSPs_2)
                {
                    _DIWaferOnInSPs_2 = value;
                    NotifyPropertyChanged(nameof(DIWaferOnInSPs_2));
                }
            }
        }
        private IOPortDescripter<bool> _DIOpenInSPs_2 = new IOPortDescripter<bool>("DIOpenInSPs_2", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIOpenInSPs_2
        {
            get { return _DIOpenInSPs_2; }
            set
            {
                if (value != this._DIOpenInSPs_2)
                {
                    _DIOpenInSPs_2 = value;
                    NotifyPropertyChanged(nameof(DIOpenInSPs_2));
                }
            }
        }
        private IOPortDescripter<bool> _DIBUFVACS_0 = new IOPortDescripter<bool>("DIBUFVACS_0", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIBUFVACS_0
        {
            get { return _DIBUFVACS_0; }
            set
            {
                if (value != this._DIBUFVACS_0)
                {
                    _DIBUFVACS_0 = value;
                    NotifyPropertyChanged(nameof(DIBUFVACS_0));
                }
            }
        }
        private IOPortDescripter<bool> _DIBUFVACS_1 = new IOPortDescripter<bool>("DIBUFVACS_1", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIBUFVACS_1
        {
            get { return _DIBUFVACS_1; }
            set
            {
                if (value != this._DIBUFVACS_1)
                {
                    _DIBUFVACS_1 = value;
                    NotifyPropertyChanged(nameof(DIBUFVACS_1));
                }
            }
        }
        private IOPortDescripter<bool> _DIBUFVACS_2 = new IOPortDescripter<bool>("DIBUFVACS_2", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIBUFVACS_2
        {
            get { return _DIBUFVACS_2; }
            set
            {
                if (value != this._DIBUFVACS_2)
                {
                    _DIBUFVACS_2 = value;
                    NotifyPropertyChanged(nameof(DIBUFVACS_2));
                }
            }
        }
        private IOPortDescripter<bool> _DIBUFVACS_3 = new IOPortDescripter<bool>("DIBUFVACS_3", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIBUFVACS_3
        {
            get { return _DIBUFVACS_3; }
            set
            {
                if (value != this._DIBUFVACS_3)
                {
                    _DIBUFVACS_3 = value;
                    NotifyPropertyChanged(nameof(DIBUFVACS_3));
                }
            }
        }
        private IOPortDescripter<bool> _DIBUFVACS_4 = new IOPortDescripter<bool>("DIBUFVACS_4", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIBUFVACS_4
        {
            get { return _DIBUFVACS_4; }
            set
            {
                if (value != this._DIBUFVACS_4)
                {
                    _DIBUFVACS_4 = value;
                    NotifyPropertyChanged(nameof(DIBUFVACS_4));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_0 = new IOPortDescripter<bool>("DICardBuffs_0", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_0
        {
            get { return _DICardBuffs_0; }
            set
            {
                if (value != this._DICardBuffs_0)
                {
                    _DICardBuffs_0 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_0));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_1 = new IOPortDescripter<bool>("DICardBuffs_1", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_1
        {
            get { return _DICardBuffs_1; }
            set
            {
                if (value != this._DICardBuffs_1)
                {
                    _DICardBuffs_1 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_1));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_2 = new IOPortDescripter<bool>("DICardBuffs_2", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_2
        {
            get { return _DICardBuffs_2; }
            set
            {
                if (value != this._DICardBuffs_2)
                {
                    _DICardBuffs_2 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_2));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_3 = new IOPortDescripter<bool>("DICardBuffs_3", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_3
        {
            get { return _DICardBuffs_3; }
            set
            {
                if (value != this._DICardBuffs_3)
                {
                    _DICardBuffs_3 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_3));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_4 = new IOPortDescripter<bool>("DICardBuffs_4", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_4
        {
            get { return _DICardBuffs_4; }
            set
            {
                if (value != this._DICardBuffs_4)
                {
                    _DICardBuffs_4 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_4));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_5 = new IOPortDescripter<bool>("DICardBuffs_5", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_5
        {
            get { return _DICardBuffs_5; }
            set
            {
                if (value != this._DICardBuffs_5)
                {
                    _DICardBuffs_5 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_5));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_6 = new IOPortDescripter<bool>("DICardBuffs_6", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_6
        {
            get { return _DICardBuffs_6; }
            set
            {
                if (value != this._DICardBuffs_6)
                {
                    _DICardBuffs_6 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_6));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_7 = new IOPortDescripter<bool>("DICardBuffs_7", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_7
        {
            get { return _DICardBuffs_7; }
            set
            {
                if (value != this._DICardBuffs_7)
                {
                    _DICardBuffs_7 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_7));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_8 = new IOPortDescripter<bool>("DICardBuffs_8", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_8
        {
            get { return _DICardBuffs_8; }
            set
            {
                if (value != this._DICardBuffs_8)
                {
                    _DICardBuffs_8 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_8));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_9 = new IOPortDescripter<bool>("DICardBuffs_9", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_9
        {
            get { return _DICardBuffs_9; }
            set
            {
                if (value != this._DICardBuffs_9)
                {
                    _DICardBuffs_9 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_9));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_10 = new IOPortDescripter<bool>("DICardBuffs_10", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_10
        {
            get { return _DICardBuffs_10; }
            set
            {
                if (value != this._DICardBuffs_10)
                {
                    _DICardBuffs_10 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_10));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_11 = new IOPortDescripter<bool>("DICardBuffs_11", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_11
        {
            get { return _DICardBuffs_11; }
            set
            {
                if (value != this._DICardBuffs_11)
                {
                    _DICardBuffs_11 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_11));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_12 = new IOPortDescripter<bool>("DICardBuffs_12", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_12
        {
            get { return _DICardBuffs_12; }
            set
            {
                if (value != this._DICardBuffs_12)
                {
                    _DICardBuffs_12 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_12));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_13 = new IOPortDescripter<bool>("DICardBuffs_13", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_13
        {
            get { return _DICardBuffs_13; }
            set
            {
                if (value != this._DICardBuffs_13)
                {
                    _DICardBuffs_13 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_13));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_14 = new IOPortDescripter<bool>("DICardBuffs_14", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_14
        {
            get { return _DICardBuffs_14; }
            set
            {
                if (value != this._DICardBuffs_14)
                {
                    _DICardBuffs_14 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_14));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_15 = new IOPortDescripter<bool>("DICardBuffs_15", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_15
        {
            get { return _DICardBuffs_15; }
            set
            {
                if (value != this._DICardBuffs_15)
                {
                    _DICardBuffs_15 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_15));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_16 = new IOPortDescripter<bool>("DICardBuffs_16", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_16
        {
            get { return _DICardBuffs_16; }
            set
            {
                if (value != this._DICardBuffs_16)
                {
                    _DICardBuffs_16 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_16));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_17 = new IOPortDescripter<bool>("DICardBuffs_17", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_17
        {
            get { return _DICardBuffs_17; }
            set
            {
                if (value != this._DICardBuffs_17)
                {
                    _DICardBuffs_17 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_17));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_18 = new IOPortDescripter<bool>("DICardBuffs_18", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_18
        {
            get { return _DICardBuffs_18; }
            set
            {
                if (value != this._DICardBuffs_18)
                {
                    _DICardBuffs_18 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_18));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_19 = new IOPortDescripter<bool>("DICardBuffs_19", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_19
        {
            get { return _DICardBuffs_19; }
            set
            {
                if (value != this._DICardBuffs_19)
                {
                    _DICardBuffs_19 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_19));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_20 = new IOPortDescripter<bool>("DICardBuffs_20", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_20
        {
            get { return _DICardBuffs_20; }
            set
            {
                if (value != this._DICardBuffs_20)
                {
                    _DICardBuffs_20 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_20));
                }
            }
        }
        private IOPortDescripter<bool> _DICardBuffs_21 = new IOPortDescripter<bool>("DICardBuffs_21", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardBuffs_21
        {
            get { return _DICardBuffs_21; }
            set
            {
                if (value != this._DICardBuffs_21)
                {
                    _DICardBuffs_21 = value;
                    NotifyPropertyChanged(nameof(DICardBuffs_21));
                }
            }
        }
        // <-- 251017 sebas : 8개
        private IOPortDescripter<bool> _DI_ARM_BLOW1 = new IOPortDescripter<bool>("DI_ARM_BLOW1", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI_ARM_BLOW1
        {
            get { return _DI_ARM_BLOW1; }
            set
            {
                if (value != this._DI_ARM_BLOW1)
                {
                    _DI_ARM_BLOW1 = value;
                    NotifyPropertyChanged(nameof(DI_ARM_BLOW1));
                }
            }
        }
        private IOPortDescripter<bool> _DI_ARM_VAC_SENSOR1 = new IOPortDescripter<bool>("DI_ARM_VAC_SENSOR1", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI_ARM_VAC_SENSOR1
        {
            get { return _DI_ARM_VAC_SENSOR1; }
            set
            {
                if (value != this._DI_ARM_VAC_SENSOR1)
                {
                    _DI_ARM_VAC_SENSOR1 = value;
                    NotifyPropertyChanged(nameof(DI_ARM_VAC_SENSOR1));
                }
            }
        }
        private IOPortDescripter<bool> _DI_ARM_FLOW1 = new IOPortDescripter<bool>("DI_ARM_FLOW1", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI_ARM_FLOW1
        {
            get { return _DI_ARM_FLOW1; }
            set
            {
                if (value != this._DI_ARM_FLOW1)
                {
                    _DI_ARM_FLOW1 = value;
                    NotifyPropertyChanged(nameof(DI_ARM_FLOW1));
                }
            }
        }
        private IOPortDescripter<bool> _DI_ARM_BLOW2 = new IOPortDescripter<bool>("DI_ARM_BLOW2", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI_ARM_BLOW2
        {
            get { return _DI_ARM_BLOW2; }
            set
            {
                if (value != this._DI_ARM_BLOW2)
                {
                    _DI_ARM_BLOW2 = value;
                    NotifyPropertyChanged(nameof(DI_ARM_BLOW2));
                }
            }
        }
        private IOPortDescripter<bool> _DI_ARM_VAC_SENSOR2 = new IOPortDescripter<bool>("DI_ARM_VAC_SENSOR2", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI_ARM_VAC_SENSOR2
        {
            get { return _DI_ARM_VAC_SENSOR2; }
            set
            {
                if (value != this._DI_ARM_VAC_SENSOR2)
                {
                    _DI_ARM_VAC_SENSOR2 = value;
                    NotifyPropertyChanged(nameof(DI_ARM_VAC_SENSOR2));
                }
            }
        }
        private IOPortDescripter<bool> _DI_ARM_FLOW2 = new IOPortDescripter<bool>("DI_ARM_FLOW2", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI_ARM_FLOW2
        {
            get { return _DI_ARM_FLOW2; }
            set
            {
                if (value != this._DI_ARM_FLOW2)
                {
                    _DI_ARM_FLOW2 = value;
                    NotifyPropertyChanged(nameof(DI_ARM_FLOW2));
                }
            }
        }
        private IOPortDescripter<bool> _DI_FD_VAC_SENSOR = new IOPortDescripter<bool>("DI_FD_VAC_SENSOR", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI_FD_VAC_SENSOR
        {
            get { return _DI_FD_VAC_SENSOR; }
            set
            {
                if (value != this._DI_FD_VAC_SENSOR)
                {
                    _DI_FD_VAC_SENSOR = value;
                    NotifyPropertyChanged(nameof(DI_FD_VAC_SENSOR));
                }
            }
        }
        private IOPortDescripter<bool> _DI_EJ_VAC_SENSOR = new IOPortDescripter<bool>("DI_EJ_VAC_SENSOR", EnumIOType.INPUT);
        public IOPortDescripter<bool> DI_EJ_VAC_SENSOR
        {
            get { return _DI_EJ_VAC_SENSOR; }
            set
            {
                if (value != this._DI_EJ_VAC_SENSOR)
                {
                    _DI_EJ_VAC_SENSOR = value;
                    NotifyPropertyChanged(nameof(DI_EJ_VAC_SENSOR));
                }
            }
        }
        // -->
    }
    [Serializable]
    public class OutputPortDefinitions : INotifyPropertyChanged, IParamNode
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

        public OutputPortDefinitions()
        {
            DO_X_BRAKERELEASE.IOOveride.Value = EnumIOOverride.EMUL;
            DO_X_BRAKERELEASE.ChannelIndex.Value = 0;
            DO_X_BRAKERELEASE.PortIndex.Value = 0;
            DO_Y_BRAKERELEASE.IOOveride.Value = EnumIOOverride.EMUL;
            DO_Y_BRAKERELEASE.ChannelIndex.Value = 0;
            DO_Y_BRAKERELEASE.PortIndex.Value = 0;

            DO_TRILEG_SUCTION.IOOveride.Value = EnumIOOverride.EMUL;
            DO_TRILEG_SUCTION.ChannelIndex.Value = 0;
            DO_TRILEG_SUCTION.PortIndex.Value = 0;
        }
        #region Loader Outputs

        private IOPortDescripter<bool> _DOOCR1LIGHT = new IOPortDescripter<bool>(nameof(DOOCR1LIGHT), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOOCR1LIGHT
        {
            get { return _DOOCR1LIGHT; }
            set
            {
                if (value != this._DOOCR1LIGHT)
                {
                    _DOOCR1LIGHT = value;
                    NotifyPropertyChanged(nameof(DOOCR1LIGHT));
                }
            }
        }

        private IOPortDescripter<bool> _DOOCR2LIGHT = new IOPortDescripter<bool>(nameof(DOOCR2LIGHT), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOOCR2LIGHT
        {
            get { return _DOOCR2LIGHT; }
            set
            {
                if (value != this._DOOCR2LIGHT)
                {
                    _DOOCR2LIGHT = value;
                    NotifyPropertyChanged(nameof(DOOCR2LIGHT));
                }
            }
        }

        private IOPortDescripter<bool> _DOOCR3LIGHT = new IOPortDescripter<bool>(nameof(DOOCR3LIGHT), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOOCR3LIGHT
        {
            get { return _DOOCR3LIGHT; }
            set
            {
                if (value != this._DOOCR3LIGHT)
                {
                    _DOOCR3LIGHT = value;
                    NotifyPropertyChanged(nameof(DOOCR3LIGHT));
                }
            }
        }

        private IOPortDescripter<bool> _DOPACL = new IOPortDescripter<bool>(nameof(DOPACL), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOPACL
        {
            get { return _DOPACL; }
            set
            {
                if (value != this._DOPACL)
                {
                    _DOPACL = value;
                    NotifyPropertyChanged(nameof(DOPACL));
                }
            }
        }

        private IOPortDescripter<bool> _DOREDLAMP = new IOPortDescripter<bool>(nameof(DOREDLAMP), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOREDLAMP
        {
            get { return _DOREDLAMP; }
            set
            {
                if (value != this._DOREDLAMP)
                {
                    _DOREDLAMP = value;
                    NotifyPropertyChanged(nameof(DOREDLAMP));
                }
            }
        }

        private IOPortDescripter<bool> _DOGREENLAMP = new IOPortDescripter<bool>(nameof(DOGREENLAMP), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOGREENLAMP
        {
            get { return _DOGREENLAMP; }
            set
            {
                if (value != this._DOGREENLAMP)
                {
                    _DOGREENLAMP = value;
                    NotifyPropertyChanged(nameof(DOGREENLAMP));
                }
            }
        }

        private IOPortDescripter<bool> _DOYELLOWLAMP = new IOPortDescripter<bool>(nameof(DOYELLOWLAMP), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOYELLOWLAMP
        {
            get { return _DOYELLOWLAMP; }
            set
            {
                if (value != this._DOYELLOWLAMP)
                {
                    _DOYELLOWLAMP = value;
                    NotifyPropertyChanged(nameof(DOYELLOWLAMP));
                }
            }
        }

        private IOPortDescripter<bool> _DOCASSETTELOCK = new IOPortDescripter<bool>(nameof(DOCASSETTELOCK), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOCASSETTELOCK
        {
            get { return _DOCASSETTELOCK; }
            set
            {
                if (value != this._DOCASSETTELOCK)
                {
                    _DOCASSETTELOCK = value;
                    NotifyPropertyChanged(nameof(DOCASSETTELOCK));
                }
            }
        }

        private IOPortDescripter<bool> _DOFOUPSWING = new IOPortDescripter<bool>(nameof(DOFOUPSWING), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOFOUPSWING
        {
            get { return _DOFOUPSWING; }
            set
            {
                if (value != this._DOFOUPSWING)
                {
                    _DOFOUPSWING = value;
                    NotifyPropertyChanged(nameof(DOFOUPSWING));
                }
            }
        }

        private IOPortDescripter<bool> _DOPRE = new IOPortDescripter<bool>(nameof(DOPRE), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOPRE
        {
            get { return _DOPRE; }
            set
            {
                if (value != this._DOPRE)
                {
                    _DOPRE = value;
                    NotifyPropertyChanged(nameof(DOPRE));
                }
            }
        }

        private IOPortDescripter<bool> _DOARM1 = new IOPortDescripter<bool>(nameof(DOARM1), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOARM1
        {
            get { return _DOARM1; }
            set
            {
                if (value != this._DOARM1)
                {
                    _DOARM1 = value;
                    NotifyPropertyChanged(nameof(DOARM1));
                }
            }
        }

        private IOPortDescripter<bool> _DOARM2 = new IOPortDescripter<bool>(nameof(DOARM2), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOARM2
        {
            get { return _DOARM2; }
            set
            {
                if (value != this._DOARM2)
                {
                    _DOARM2 = value;
                    NotifyPropertyChanged(nameof(DOARM2));
                }
            }
        }

        private IOPortDescripter<bool> _DOARMAIRON = new IOPortDescripter<bool>(nameof(DOARMAIRON), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOARMAIRON
        {
            get { return _DOARMAIRON; }
            set
            {
                if (value != this._DOARMAIRON)
                {
                    _DOARMAIRON = value;
                    NotifyPropertyChanged(nameof(DOARMAIRON));
                }
            }
        }

        private IOPortDescripter<bool> _DOARM2AIRON = new IOPortDescripter<bool>(nameof(DOARM2AIRON), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOARM2AIRON
        {
            get { return _DOARM2AIRON; }
            set
            {
                if (value != this._DOARM2AIRON)
                {
                    _DOARM2AIRON = value;
                    NotifyPropertyChanged(nameof(DOARM2AIRON));
                }
            }
        }

        private IOPortDescripter<bool> _DOSUBCHUCKAIRON = new IOPortDescripter<bool>(nameof(DOSUBCHUCKAIRON), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOSUBCHUCKAIRON
        {
            get { return _DOSUBCHUCKAIRON; }
            set
            {
                if (value != this._DOSUBCHUCKAIRON)
                {
                    _DOSUBCHUCKAIRON = value;
                    NotifyPropertyChanged(nameof(DOSUBCHUCKAIRON));
                }
            }
        }

        private IOPortDescripter<bool> _DOARMINAIR = new IOPortDescripter<bool>(nameof(DOARMINAIR), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOARMINAIR
        {
            get { return _DOARMINAIR; }
            set
            {
                if (value != this._DOARMINAIR)
                {
                    _DOARMINAIR = value;
                    NotifyPropertyChanged(nameof(DOARMINAIR));
                }
            }
        }

        private IOPortDescripter<bool> _DOARMOUTAIR = new IOPortDescripter<bool>(nameof(DOARMOUTAIR), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOARMOUTAIR
        {
            get { return _DOARMOUTAIR; }
            set
            {
                if (value != this._DOARMOUTAIR)
                {
                    _DOARMOUTAIR = value;
                    NotifyPropertyChanged(nameof(DOARMOUTAIR));
                }
            }
        }

        private IOPortDescripter<bool> _DOBUZZERON = new IOPortDescripter<bool>(nameof(DOBUZZERON), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBUZZERON
        {
            get { return _DOBUZZERON; }
            set
            {
                if (value != this._DOBUZZERON)
                {
                    _DOBUZZERON = value;
                    NotifyPropertyChanged(nameof(DOBUZZERON));
                }
            }
        }

        private IOPortDescripter<bool> _DOBLUELAMPON = new IOPortDescripter<bool>(nameof(DOBLUELAMPON), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBLUELAMPON
        {
            get { return _DOBLUELAMPON; }
            set
            {
                if (value != this._DOBLUELAMPON)
                {
                    _DOBLUELAMPON = value;
                    NotifyPropertyChanged(nameof(DOBLUELAMPON));
                }
            }
        }

        private IOPortDescripter<bool> _DOYELLOWLAMPON = new IOPortDescripter<bool>(nameof(DOYELLOWLAMPON), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOYELLOWLAMPON
        {
            get { return _DOYELLOWLAMPON; }
            set
            {
                if (value != this._DOYELLOWLAMPON)
                {
                    _DOYELLOWLAMPON = value;
                    NotifyPropertyChanged(nameof(DOYELLOWLAMPON));
                }
            }
        }

        private IOPortDescripter<bool> _DOREDLAMPON = new IOPortDescripter<bool>(nameof(DOREDLAMPON), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOREDLAMPON
        {
            get { return _DOREDLAMPON; }
            set
            {
                if (value != this._DOREDLAMPON)
                {
                    _DOREDLAMPON = value;
                    NotifyPropertyChanged(nameof(DOREDLAMPON));
                }
            }
        }

        private IOPortDescripter<bool> _DOBLUELAMPON2 = new IOPortDescripter<bool>(nameof(DOBLUELAMPON2), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBLUELAMPON2
        {
            get { return _DOBLUELAMPON2; }
            set
            {
                if (value != this._DOBLUELAMPON2)
                {
                    _DOBLUELAMPON2 = value;
                    NotifyPropertyChanged(nameof(DOBLUELAMPON2));
                }
            }
        }
        private IOPortDescripter<bool> _DOVMUX_SIGNAL5 = new IOPortDescripter<bool>(nameof(DOVMUX_SIGNAL5), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOVMUX_SIGNAL5
        {
            get { return _DOVMUX_SIGNAL5; }
            set
            {
                if (value != this._DOVMUX_SIGNAL5)
                {
                    _DOVMUX_SIGNAL5 = value;
                    NotifyPropertyChanged(nameof(DOMUX_SIGNAL0));
                }
            }
        }
        private IOPortDescripter<bool> _DOVMUX_SIGNAL6 = new IOPortDescripter<bool>(nameof(DOVMUX_SIGNAL6), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOVMUX_SIGNAL6
        {
            get { return _DOVMUX_SIGNAL6; }
            set
            {
                if (value != this._DOVMUX_SIGNAL6)
                {
                    _DOVMUX_SIGNAL6 = value;
                    NotifyPropertyChanged(nameof(DOMUX_SIGNAL0));
                }
            }
        }
        private IOPortDescripter<bool> _DOVMUX_SIGNAL7 = new IOPortDescripter<bool>(nameof(DOVMUX_SIGNAL7), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOVMUX_SIGNAL7
        {
            get { return _DOVMUX_SIGNAL7; }
            set
            {
                if (value != this._DOVMUX_SIGNAL7)
                {
                    _DOVMUX_SIGNAL7 = value;
                    NotifyPropertyChanged(nameof(DOMUX_SIGNAL0));
                }
            }
        }
        private IOPortDescripter<bool> _DOMUX_SIGNAL0 = new IOPortDescripter<bool>(nameof(DOMUX_SIGNAL0), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOMUX_SIGNAL0
        {
            get { return _DOMUX_SIGNAL0; }
            set
            {
                if (value != this._DOMUX_SIGNAL0)
                {
                    _DOMUX_SIGNAL0 = value;
                    NotifyPropertyChanged(nameof(DOMUX_SIGNAL0));
                }
            }
        }

        private IOPortDescripter<bool> _DOMUX_SIGNAL1 = new IOPortDescripter<bool>(nameof(DOMUX_SIGNAL1), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOMUX_SIGNAL1
        {
            get { return _DOMUX_SIGNAL1; }
            set
            {
                if (value != this._DOMUX_SIGNAL1)
                {
                    _DOMUX_SIGNAL1 = value;
                    NotifyPropertyChanged(nameof(DOMUX_SIGNAL1));
                }
            }
        }

        private IOPortDescripter<bool> _DOMUX_SIGNAL2 = new IOPortDescripter<bool>(nameof(DOMUX_SIGNAL2), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOMUX_SIGNAL2
        {
            get { return _DOMUX_SIGNAL2; }
            set
            {
                if (value != this._DOMUX_SIGNAL2)
                {
                    _DOMUX_SIGNAL2 = value;
                    NotifyPropertyChanged(nameof(DOMUX_SIGNAL2));
                }
            }
        }

        private IOPortDescripter<bool> _DOMUX_ENABLE = new IOPortDescripter<bool>(nameof(DOMUX_ENABLE), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOMUX_ENABLE
        {
            get { return _DOMUX_ENABLE; }
            set
            {
                if (value != this._DOMUX_ENABLE)
                {
                    _DOMUX_ENABLE = value;
                    NotifyPropertyChanged(nameof(DOMUX_ENABLE));
                }
            }
        }

        private IOPortDescripter<bool> _DODATA = new IOPortDescripter<bool>(nameof(DODATA), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DODATA
        {
            get { return _DODATA; }
            set
            {
                if (value != this._DODATA)
                {
                    _DODATA = value;
                    NotifyPropertyChanged(nameof(DODATA));
                }
            }
        }

        private IOPortDescripter<bool> _DOCLK = new IOPortDescripter<bool>(nameof(DOCLK), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOCLK
        {
            get { return _DOCLK; }
            set
            {
                if (value != this._DOCLK)
                {
                    _DOCLK = value;
                    NotifyPropertyChanged(nameof(DOCLK));
                }
            }
        }

        private IOPortDescripter<bool> _DOVMUX_SIGNAL0 = new IOPortDescripter<bool>(nameof(DOVMUX_SIGNAL0), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOVMUX_SIGNAL0
        {
            get { return _DOVMUX_SIGNAL0; }
            set
            {
                if (value != this._DOVMUX_SIGNAL0)
                {
                    _DOVMUX_SIGNAL0 = value;
                    NotifyPropertyChanged(nameof(DOVMUX_SIGNAL0));
                }
            }
        }

        private IOPortDescripter<bool> _DOVMUX_SIGNAL1 = new IOPortDescripter<bool>(nameof(DOVMUX_SIGNAL1), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOVMUX_SIGNAL1
        {
            get { return _DOVMUX_SIGNAL1; }
            set
            {
                if (value != this._DOVMUX_SIGNAL1)
                {
                    _DOVMUX_SIGNAL1 = value;
                    NotifyPropertyChanged(nameof(DOVMUX_SIGNAL1));
                }
            }
        }

        private IOPortDescripter<bool> _DOVMUX_SIGNAL2 = new IOPortDescripter<bool>(nameof(DOVMUX_SIGNAL2), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOVMUX_SIGNAL2
        {
            get { return _DOVMUX_SIGNAL2; }
            set
            {
                if (value != this._DOVMUX_SIGNAL2)
                {
                    _DOVMUX_SIGNAL2 = value;
                    NotifyPropertyChanged(nameof(DOVMUX_SIGNAL2));
                }
            }
        }

        private IOPortDescripter<bool> _DOSCANLIGHT = new IOPortDescripter<bool>(nameof(DOSCANLIGHT), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOSCANLIGHT
        {
            get { return _DOSCANLIGHT; }
            set
            {
                if (value != this._DOSCANLIGHT)
                {
                    _DOSCANLIGHT = value;
                    NotifyPropertyChanged(nameof(DOSCANLIGHT));
                }
            }
        }

        private IOPortDescripter<bool> _DOFOUP_COVER_LOCK = new IOPortDescripter<bool>(nameof(DOFOUP_COVER_LOCK), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOFOUP_COVER_LOCK
        {
            get { return _DOFOUP_COVER_LOCK; }
            set
            {
                if (value != this._DOFOUP_COVER_LOCK)
                {
                    _DOFOUP_COVER_LOCK = value;
                    NotifyPropertyChanged(nameof(DOFOUP_COVER_LOCK));
                }
            }
        }

        private IOPortDescripter<bool> _DOLDRY_AIR_ON = new IOPortDescripter<bool>(nameof(DOLDRY_AIR_ON), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOLDRY_AIR_ON
        {
            get { return _DOLDRY_AIR_ON; }
            set
            {
                if (value != this._DOLDRY_AIR_ON)
                {
                    _DOLDRY_AIR_ON = value;
                    NotifyPropertyChanged(nameof(DOLDRY_AIR_ON));
                }
            }
        }

        private IOPortDescripter<bool> _DOFRAMEDPREIN = new IOPortDescripter<bool>(nameof(DOFRAMEDPREIN), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOFRAMEDPREIN
        {
            get { return _DOFRAMEDPREIN; }
            set
            {
                if (value != this._DOFRAMEDPREIN)
                {
                    _DOFRAMEDPREIN = value;
                    NotifyPropertyChanged(nameof(DOFRAMEDPREIN));
                }
            }
        }

        private IOPortDescripter<bool> _DOFRAMEDPREUP = new IOPortDescripter<bool>(nameof(DOFRAMEDPREUP), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOFRAMEDPREUP
        {
            get { return _DOFRAMEDPREUP; }
            set
            {
                if (value != this._DOFRAMEDPREUP)
                {
                    _DOFRAMEDPREUP = value;
                    NotifyPropertyChanged(nameof(DOFRAMEDPREUP));
                }
            }
        }
        #endregion

        #region // Stage Outputs

        private IOPortDescripter<bool> _DOTESTERHEAD_PURGE = new IOPortDescripter<bool>(nameof(DOTESTERHEAD_PURGE), EnumIOType.OUTPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DOTESTERHEAD_PURGE
        {
            get { return _DOTESTERHEAD_PURGE; }
            set
            {
                if (value != this._DOTESTERHEAD_PURGE)
                {
                    _DOTESTERHEAD_PURGE = value;
                    NotifyPropertyChanged(nameof(DOTESTERHEAD_PURGE));
                }
            }
        }

        private IOPortDescripter<bool> _DOTHREEPOD_COOLING_ON = new IOPortDescripter<bool>(nameof(DOTHREEPOD_COOLING_ON), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOTHREEPOD_COOLING_ON
        {
            get { return _DOTHREEPOD_COOLING_ON; }
            set
            {
                if (value != this._DOTHREEPOD_COOLING_ON)
                {
                    _DOTHREEPOD_COOLING_ON = value;
                    NotifyPropertyChanged(nameof(DOTHREEPOD_COOLING_ON));
                }
            }
        }

        private IOPortDescripter<bool> _DOWAFERREAR = new IOPortDescripter<bool>(nameof(DOWAFERREAR), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOWAFERREAR
        {
            get { return _DOWAFERREAR; }
            set
            {
                if (value != this._DOWAFERREAR)
                {
                    _DOWAFERREAR = value;
                    NotifyPropertyChanged(nameof(DOWAFERREAR));
                }
            }
        }

        private IOPortDescripter<bool> _DOWAFERMIDDLE = new IOPortDescripter<bool>(nameof(DOWAFERMIDDLE), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOWAFERMIDDLE
        {
            get { return _DOWAFERMIDDLE; }
            set
            {
                if (value != this._DOWAFERMIDDLE)
                {
                    _DOWAFERMIDDLE = value;
                    NotifyPropertyChanged(nameof(DOWAFERMIDDLE));
                }
            }
        }

        private IOPortDescripter<bool> _DORESETAMP_0 = new IOPortDescripter<bool>(nameof(DORESETAMP_0), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DORESETAMP_0
        {
            get { return _DORESETAMP_0; }
            set
            {
                if (value != this._DORESETAMP_0)
                {
                    _DORESETAMP_0 = value;
                    NotifyPropertyChanged(nameof(DORESETAMP_0));
                }
            }
        }

        private IOPortDescripter<bool> _DORESETAMP_1 = new IOPortDescripter<bool>(nameof(DORESETAMP_1), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DORESETAMP_1
        {
            get { return _DORESETAMP_1; }
            set
            {
                if (value != this._DORESETAMP_1)
                {
                    _DORESETAMP_1 = value;
                    NotifyPropertyChanged(nameof(DORESETAMP_1));
                }
            }
        }

        private IOPortDescripter<bool> _DOCHUCKAIRON_0 = new IOPortDescripter<bool>(nameof(DOCHUCKAIRON_0), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOCHUCKAIRON_0
        {
            get { return _DOCHUCKAIRON_0; }
            set
            {
                if (value != this._DOCHUCKAIRON_0)
                {
                    _DOCHUCKAIRON_0 = value;
                    NotifyPropertyChanged(nameof(DOCHUCKAIRON_0));
                }
            }
        }

        private IOPortDescripter<bool> _DOCHUCK_EXTRA_AIRON_0 = new IOPortDescripter<bool>(nameof(DOCHUCK_EXTRA_AIRON_0), EnumIOType.OUTPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DOCHUCK_EXTRA_AIRON_0
        {
            get { return _DOCHUCK_EXTRA_AIRON_0; }
            set
            {
                if (value != this._DOCHUCK_EXTRA_AIRON_0)
                {
                    _DOCHUCK_EXTRA_AIRON_0 = value;
                    NotifyPropertyChanged(nameof(DOCHUCK_EXTRA_AIRON_0));
                }
            }
        }

        private IOPortDescripter<bool> _DOCHUCKAIRON_1 = new IOPortDescripter<bool>(nameof(DOCHUCKAIRON_1), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOCHUCKAIRON_1
        {
            get { return _DOCHUCKAIRON_1; }
            set
            {
                if (value != this._DOCHUCKAIRON_1)
                {
                    _DOCHUCKAIRON_1 = value;
                    NotifyPropertyChanged(nameof(DOCHUCKAIRON_1));
                }
            }
        }
        private IOPortDescripter<bool> _DOCHUCKAIRON_2 = new IOPortDescripter<bool>(nameof(DOCHUCKAIRON_2), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOCHUCKAIRON_2
        {
            get { return _DOCHUCKAIRON_2; }
            set
            {
                if (value != this._DOCHUCKAIRON_2)
                {
                    _DOCHUCKAIRON_2 = value;
                    NotifyPropertyChanged(nameof(DOCHUCKAIRON_2));
                }
            }
        }

        private IOPortDescripter<bool> _DOCHUCK_EXTRA_AIRON_2 = new IOPortDescripter<bool>(nameof(DOCHUCK_EXTRA_AIRON_2), EnumIOType.OUTPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DOCHUCK_EXTRA_AIRON_2
        {
            get { return _DOCHUCK_EXTRA_AIRON_2; }
            set
            {
                if (value != this._DOCHUCK_EXTRA_AIRON_2)
                {
                    _DOCHUCK_EXTRA_AIRON_2 = value;
                    NotifyPropertyChanged(nameof(DOCHUCK_EXTRA_AIRON_2));
                }
            }
        }

        private IOPortDescripter<bool> _DOINKERDOWN = new IOPortDescripter<bool>(nameof(DOINKERDOWN), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOINKERDOWN
        {
            get { return _DOINKERDOWN; }
            set
            {
                if (value != this._DOINKERDOWN)
                {
                    _DOINKERDOWN = value;
                    NotifyPropertyChanged(nameof(DOINKERDOWN));
                }
            }
        }

        private IOPortDescripter<bool> _DOTHREELEGUP = new IOPortDescripter<bool>(nameof(DOTHREELEGUP), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOTHREELEGUP
        {
            get { return _DOTHREELEGUP; }
            set
            {
                if (value != this._DOTHREELEGUP)
                {
                    _DOTHREELEGUP = value;
                    NotifyPropertyChanged(nameof(DOTHREELEGUP));
                }
            }
        }
        private IOPortDescripter<bool> _DOCLEANUNITUP = new IOPortDescripter<bool>(nameof(DOCLEANUNITUP), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOCLEANUNITUP
        {
            get { return _DOCLEANUNITUP; }
            set
            {
                if (value != this._DOCLEANUNITUP)
                {
                    _DOCLEANUNITUP = value;
                    NotifyPropertyChanged(nameof(DOCLEANUNITUP));
                }
            }
        }
        private IOPortDescripter<bool> _DOCHUCKCOOLAIRON = new IOPortDescripter<bool>(nameof(DOCHUCKCOOLAIRON), EnumIOType.OUTPUT, EnumIOOverride.NHI);

        public IOPortDescripter<bool> DOCHUCKCOOLAIRON
        {
            get { return _DOCHUCKCOOLAIRON; }
            set
            {
                if (value != this._DOCHUCKCOOLAIRON)
                {
                    _DOCHUCKCOOLAIRON = value;
                    NotifyPropertyChanged(nameof(DOCHUCKCOOLAIRON));
                }
            }
        }
        private IOPortDescripter<bool> _DONEEDLECLEANAIRON = new IOPortDescripter<bool>(nameof(DONEEDLECLEANAIRON), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DONEEDLECLEANAIRON
        {
            get { return _DONEEDLECLEANAIRON; }
            set
            {
                if (value != this._DONEEDLECLEANAIRON)
                {
                    _DONEEDLECLEANAIRON = value;
                    NotifyPropertyChanged(nameof(DONEEDLECLEANAIRON));
                }
            }
        }
        private IOPortDescripter<bool> _DOCARDHOLDEROPEN = new IOPortDescripter<bool>(nameof(DOCARDHOLDEROPEN), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOCARDHOLDEROPEN
        {
            get { return _DOCARDHOLDEROPEN; }
            set
            {
                if (value != this._DOCARDHOLDEROPEN)
                {
                    _DOCARDHOLDEROPEN = value;
                    NotifyPropertyChanged(nameof(DOCARDHOLDEROPEN));
                }
            }
        }

        private IOPortDescripter<bool> _DOINKERSHOT = new IOPortDescripter<bool>(nameof(DOINKERSHOT), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOINKERSHOT
        {
            get { return _DOINKERSHOT; }
            set
            {
                if (value != this._DOINKERSHOT)
                {
                    _DOINKERSHOT = value;
                    NotifyPropertyChanged(nameof(DOINKERSHOT));
                }
            }
        }
        private IOPortDescripter<bool> _DOINKERSHOTEX = new IOPortDescripter<bool>(nameof(DOINKERSHOTEX), EnumIOType.OUTPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DOINKERSHOTEX
        {
            get { return _DOINKERSHOTEX; }
            set
            {
                if (value != this._DOINKERSHOTEX)
                {
                    _DOINKERSHOTEX = value;
                    NotifyPropertyChanged(nameof(DOINKERSHOTEX));
                }
            }
        }
        private IOPortDescripter<bool> _DOCSTREADYLAMPON = new IOPortDescripter<bool>(nameof(DOCSTREADYLAMPON), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOCSTREADYLAMPON
        {
            get { return _DOCSTREADYLAMPON; }
            set
            {
                if (value != this._DOCSTREADYLAMPON)
                {
                    _DOCSTREADYLAMPON = value;
                    NotifyPropertyChanged(nameof(DOCSTREADYLAMPON));
                }
            }
        }

        private IOPortDescripter<bool> _DOPREALIGNCAM_0 = new IOPortDescripter<bool>(nameof(DOPREALIGNCAM_0), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOPREALIGNCAM_0
        {
            get { return _DOPREALIGNCAM_0; }
            set
            {
                if (value != this._DOPREALIGNCAM_0)
                {
                    _DOPREALIGNCAM_0 = value;
                    NotifyPropertyChanged(nameof(DOPREALIGNCAM_0));
                }
            }
        }
        private IOPortDescripter<bool> _DOPREALIGNCAM_1 = new IOPortDescripter<bool>(nameof(DOPREALIGNCAM_1), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOPREALIGNCAM_1
        {
            get { return _DOPREALIGNCAM_1; }
            set
            {
                if (value != this._DOPREALIGNCAM_1)
                {
                    _DOPREALIGNCAM_1 = value;
                    NotifyPropertyChanged(nameof(DOPREALIGNCAM_1));
                }
            }
        }

        private IOPortDescripter<bool> _DOPREALIGNORGSW_0 = new IOPortDescripter<bool>(nameof(DOPREALIGNORGSW_0), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOPREALIGNORGSW_0
        {
            get { return _DOPREALIGNORGSW_0; }
            set
            {
                if (value != this._DOPREALIGNORGSW_0)
                {
                    _DOPREALIGNORGSW_0 = value;
                    NotifyPropertyChanged(nameof(DOPREALIGNORGSW_0));
                }
            }
        }

        private IOPortDescripter<bool> _DOPREALIGNORGSW_1 = new IOPortDescripter<bool>(nameof(DOPREALIGNORGSW_1), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOPREALIGNORGSW_1
        {
            get { return _DOPREALIGNORGSW_1; }
            set
            {
                if (value != this._DOPREALIGNORGSW_1)
                {
                    _DOPREALIGNORGSW_1 = value;
                    NotifyPropertyChanged(nameof(DOPREALIGNORGSW_1));
                }
            }
        }

        private IOPortDescripter<bool> _DOTRIGGEROUT = new IOPortDescripter<bool>(nameof(DOTRIGGEROUT), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOTRIGGEROUT
        {
            get { return _DOTRIGGEROUT; }
            set
            {
                if (value != this._DOTRIGGEROUT)
                {
                    _DOTRIGGEROUT = value;
                    NotifyPropertyChanged(nameof(DOTRIGGEROUT));
                }
            }
        }
        private IOPortDescripter<bool> _DOCAMERA_MUX_0 = new IOPortDescripter<bool>(nameof(DOCAMERA_MUX_0), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOCAMERA_MUX_0
        {
            get { return _DOCAMERA_MUX_0; }
            set
            {
                if (value != this._DOCAMERA_MUX_0)
                {
                    _DOCAMERA_MUX_0 = value;
                    NotifyPropertyChanged(nameof(DOCAMERA_MUX_0));
                }
            }
        }
        private IOPortDescripter<bool> _DOCAMERA_MUX_1 = new IOPortDescripter<bool>(nameof(DOCAMERA_MUX_1), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOCAMERA_MUX_1
        {
            get { return _DOCAMERA_MUX_1; }
            set
            {
                if (value != this._DOCAMERA_MUX_1)
                {
                    _DOCAMERA_MUX_1 = value;
                    NotifyPropertyChanged(nameof(DOCAMERA_MUX_1));
                }
            }
        }
        private IOPortDescripter<bool> _DOCAMERA_MUX_2 = new IOPortDescripter<bool>(nameof(DOCAMERA_MUX_2), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOCAMERA_MUX_2
        {
            get { return _DOCAMERA_MUX_2; }
            set
            {
                if (value != this._DOCAMERA_MUX_2)
                {
                    _DOCAMERA_MUX_2 = value;
                    NotifyPropertyChanged(nameof(DOCAMERA_MUX_2));
                }
            }
        }
        private IOPortDescripter<bool> _DOZUPLAMPON = new IOPortDescripter<bool>(nameof(DOZUPLAMPON), EnumIOType.OUTPUT, EnumIOOverride.NHI);

        public IOPortDescripter<bool> DOZUPLAMPON
        {
            get { return _DOZUPLAMPON; }
            set
            {
                if (value != this._DOZUPLAMPON)
                {
                    _DOZUPLAMPON = value;
                    NotifyPropertyChanged(nameof(DOZUPLAMPON));
                }
            }
        }


        private IOPortDescripter<bool> _DONO_WAFER = new IOPortDescripter<bool>(nameof(DONO_WAFER), EnumIOType.OUTPUT, EnumIOOverride.EMUL);

        public IOPortDescripter<bool> DONO_WAFER
        {
            get { return _DONO_WAFER; }
            set
            {
                if (value != this._DONO_WAFER)
                {
                    _DONO_WAFER = value;
                    NotifyPropertyChanged(nameof(DONO_WAFER));
                }
            }
        }

        private IOPortDescripter<bool> _DOFAN_CONTROL = new IOPortDescripter<bool>(nameof(DOFAN_CONTROL), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOFAN_CONTROL
        {
            get { return _DOFAN_CONTROL; }
            set
            {
                if (value != this._DOFAN_CONTROL)
                {
                    _DOFAN_CONTROL = value;
                    NotifyPropertyChanged(nameof(DOFAN_CONTROL));
                }
            }
        }
        private IOPortDescripter<bool> _DOARMAUXAIRON = new IOPortDescripter<bool>(nameof(DOARMAUXAIRON), EnumIOType.OUTPUT, EnumIOOverride.NLO);

        public IOPortDescripter<bool> DOARMAUXAIRON
        {
            get { return _DOARMAUXAIRON; }
            set
            {
                if (value != this._DOARMAUXAIRON)
                {
                    _DOARMAUXAIRON = value;
                    NotifyPropertyChanged(nameof(DOARMAUXAIRON));
                }
            }
        }
        private IOPortDescripter<bool> _DOCH_ROT_OPEN = new IOPortDescripter<bool>(nameof(DOCH_ROT_OPEN), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOCH_ROT_OPEN
        {
            get { return _DOCH_ROT_OPEN; }
            set
            {
                if (value != this._DOCH_ROT_OPEN)
                {
                    _DOCH_ROT_OPEN = value;
                    NotifyPropertyChanged(nameof(DOCH_ROT_OPEN));
                }
            }
        }
        private IOPortDescripter<bool> _DOCH_ROT_CLOSE = new IOPortDescripter<bool>(nameof(DOCH_ROT_CLOSE), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOCH_ROT_CLOSE
        {
            get { return _DOCH_ROT_CLOSE; }
            set
            {
                if (value != this._DOCH_ROT_CLOSE)
                {
                    _DOCH_ROT_CLOSE = value;
                    NotifyPropertyChanged(nameof(DOCH_ROT_CLOSE));
                }
            }
        }
        private IOPortDescripter<bool> _DOCH_TUB_OPEN = new IOPortDescripter<bool>(nameof(DOCH_TUB_OPEN), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOCH_TUB_OPEN
        {
            get { return _DOCH_TUB_OPEN; }
            set
            {
                if (value != this._DOCH_TUB_OPEN)
                {
                    _DOCH_TUB_OPEN = value;
                    NotifyPropertyChanged(nameof(DOCH_TUB_OPEN));
                }
            }
        }
        private IOPortDescripter<bool> _DOSACCENABLE = new IOPortDescripter<bool>(nameof(DOSACCENABLE), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOSACCENABLE
        {
            get { return _DOSACCENABLE; }
            set
            {
                if (value != this._DOSACCENABLE)
                {
                    _DOSACCENABLE = value;
                    NotifyPropertyChanged(nameof(DOSACCENABLE));
                }
            }
        }
        private IOPortDescripter<bool> _DOMANILOCK = new IOPortDescripter<bool>(nameof(DOMANILOCK), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOMANILOCK
        {
            get { return _DOMANILOCK; }
            set
            {
                if (value != this._DOMANILOCK)
                {
                    _DOMANILOCK = value;
                    NotifyPropertyChanged(nameof(DOMANILOCK));
                }
            }
        }

        private IOPortDescripter<bool> _DOZIF_LOCK_REQ = new IOPortDescripter<bool>(nameof(DOZIF_LOCK_REQ), EnumIOType.OUTPUT, EnumIOOverride.NHI);
        public IOPortDescripter<bool> DOZIF_LOCK_REQ
        {
            get { return _DOZIF_LOCK_REQ; }
            set
            {
                if (value != this._DOZIF_LOCK_REQ)
                {
                    _DOZIF_LOCK_REQ = value;
                    NotifyPropertyChanged(nameof(DOZIF_LOCK_REQ));
                }
            }
        }
        private IOPortDescripter<bool> _DOZIF_UNLOCK_REQ = new IOPortDescripter<bool>(nameof(DOZIF_UNLOCK_REQ), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOZIF_UNLOCK_REQ
        {
            get { return _DOZIF_UNLOCK_REQ; }
            set
            {
                if (value != this._DOZIF_UNLOCK_REQ)
                {
                    _DOZIF_UNLOCK_REQ = value;
                    NotifyPropertyChanged(nameof(DOZIF_UNLOCK_REQ));
                }
            }
        }
        private IOPortDescripter<bool> _DOCARRIER_LOCK = new IOPortDescripter<bool>(nameof(DOCARRIER_LOCK), EnumIOType.OUTPUT, EnumIOOverride.NHI);
        public IOPortDescripter<bool> DOCARRIER_LOCK
        {
            get { return _DOCARRIER_LOCK; }
            set
            {
                if (value != this._DOCARRIER_LOCK)
                {
                    _DOCARRIER_LOCK = value;
                    NotifyPropertyChanged(nameof(DOCARRIER_LOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DOCH_TUB_CLOSE = new IOPortDescripter<bool>(nameof(DOCH_TUB_CLOSE), EnumIOType.OUTPUT, EnumIOOverride.NHI);
        public IOPortDescripter<bool> DOCH_TUB_CLOSE
        {
            get { return _DOCH_TUB_CLOSE; }
            set
            {
                if (value != this._DOCH_TUB_CLOSE)
                {
                    _DOCH_TUB_CLOSE = value;
                    NotifyPropertyChanged(nameof(DOCH_TUB_CLOSE));
                }
            }
        }
        private IOPortDescripter<bool> _DOCARDHOLDERCLOSE = new IOPortDescripter<bool>(nameof(DOCARDHOLDERCLOSE), EnumIOType.OUTPUT, EnumIOOverride.NHI);
        public IOPortDescripter<bool> DOCARDHOLDERCLOSE
        {
            get { return _DOCARDHOLDERCLOSE; }
            set
            {
                if (value != this._DOCARDHOLDERCLOSE)
                {
                    _DOCARDHOLDERCLOSE = value;
                    NotifyPropertyChanged(nameof(DOCARDHOLDERCLOSE));
                }
            }
        }

        private IOPortDescripter<bool> _DOCHUCK_FG_OPEN = new IOPortDescripter<bool>(nameof(DOCHUCK_FG_OPEN), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOCHUCK_FG_OPEN
        {
            get { return _DOCHUCK_FG_OPEN; }
            set
            {
                if (value != this._DOCHUCK_FG_OPEN)
                {
                    _DOCHUCK_FG_OPEN = value;
                    NotifyPropertyChanged(nameof(DOCHUCK_FG_OPEN));
                }
            }
        }
        private IOPortDescripter<bool> _DONMC_COMM_RESET = new IOPortDescripter<bool>(nameof(DONMC_COMM_RESET), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DONMC_COMM_RESET
        {
            get { return _DONMC_COMM_RESET; }
            set
            {
                if (value != this._DONMC_COMM_RESET)
                {
                    _DONMC_COMM_RESET = value;
                    NotifyPropertyChanged(nameof(DONMC_COMM_RESET));
                }
            }
        }
        private IOPortDescripter<bool> _DOCLEAN_AIR_ON = new IOPortDescripter<bool>(nameof(DOCLEAN_AIR_ON), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOCLEAN_AIR_ON
        {
            get { return _DOCLEAN_AIR_ON; }
            set
            {
                if (value != this._DOCLEAN_AIR_ON)
                {
                    _DOCLEAN_AIR_ON = value;
                    NotifyPropertyChanged(nameof(DOCLEAN_AIR_ON));
                }
            }
        }
        private IOPortDescripter<bool> _DOCHUCKTEMP_AIR_ON = new IOPortDescripter<bool>(nameof(DOCHUCKTEMP_AIR_ON), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOCHUCKTEMP_AIR_ON
        {
            get { return _DOCHUCKTEMP_AIR_ON; }
            set
            {
                if (value != this._DOCHUCKTEMP_AIR_ON)
                {
                    _DOCHUCKTEMP_AIR_ON = value;
                    NotifyPropertyChanged(nameof(DOCHUCKTEMP_AIR_ON));
                }
            }
        }
        private IOPortDescripter<bool> _DOMANI_CON_ENABLE = new IOPortDescripter<bool>(nameof(DOMANI_CON_ENABLE), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOMANI_CON_ENABLE
        {
            get { return _DOMANI_CON_ENABLE; }
            set
            {
                if (value != this._DOMANI_CON_ENABLE)
                {
                    _DOMANI_CON_ENABLE = value;
                    NotifyPropertyChanged(nameof(DOMANI_CON_ENABLE));
                }
            }
        }
        private IOPortDescripter<bool> _DOCHUCK_SAFE_STATUS = new IOPortDescripter<bool>(nameof(DOCHUCK_SAFE_STATUS), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOCHUCK_SAFE_STATUS
        {
            get { return _DOCHUCK_SAFE_STATUS; }
            set
            {
                if (value != this._DOCHUCK_SAFE_STATUS)
                {
                    _DOCHUCK_SAFE_STATUS = value;
                    NotifyPropertyChanged(nameof(DOCHUCK_SAFE_STATUS));
                }
            }
        }
        private IOPortDescripter<bool> _DOLATCH_CMD = new IOPortDescripter<bool>(nameof(DOLATCH_CMD), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOLATCH_CMD
        {
            get { return _DOLATCH_CMD; }
            set
            {
                if (value != this._DOLATCH_CMD)
                {
                    _DOLATCH_CMD = value;
                    NotifyPropertyChanged(nameof(DOLATCH_CMD));
                }
            }
        }
        private IOPortDescripter<bool> _DOUNLATCH_CMD = new IOPortDescripter<bool>(nameof(DOUNLATCH_CMD), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOUNLATCH_CMD
        {
            get { return _DOUNLATCH_CMD; }
            set
            {
                if (value != this._DOUNLATCH_CMD)
                {
                    _DOUNLATCH_CMD = value;
                    NotifyPropertyChanged(nameof(DOUNLATCH_CMD));
                }
            }
        }
        private IOPortDescripter<bool> _DOEMGStopForCCMCU = new IOPortDescripter<bool>(nameof(DOEMGStopForCCMCU), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOEMGStopForCCMCU
        {
            get { return _DOEMGStopForCCMCU; }
            set
            {
                if (value != this._DOEMGStopForCCMCU)
                {
                    _DOEMGStopForCCMCU = value;
                    NotifyPropertyChanged(nameof(DOEMGStopForCCMCU));
                }
            }
        }
        private IOPortDescripter<bool> _DOCCMoveInMotorEnable = new IOPortDescripter<bool>(nameof(DOCCMoveInMotorEnable), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOCCMoveInMotorEnable
        {
            get { return _DOCCMoveInMotorEnable; }
            set
            {
                if (value != this._DOCCMoveInMotorEnable)
                {
                    _DOCCMoveInMotorEnable = value;
                    NotifyPropertyChanged(nameof(DOCCMoveInMotorEnable));
                }
            }
        }
        private IOPortDescripter<bool> _DOCCMoveOutMotorEnable = new IOPortDescripter<bool>(nameof(DOCCMoveOutMotorEnable), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOCCMoveOutMotorEnable
        {
            get { return _DOCCMoveOutMotorEnable; }
            set
            {
                if (value != this._DOCCMoveOutMotorEnable)
                {
                    _DOCCMoveOutMotorEnable = value;
                    NotifyPropertyChanged(nameof(DOCCMoveOutMotorEnable));
                }
            }
        }
        private IOPortDescripter<bool> _DOFDOOR_LOCK = new IOPortDescripter<bool>(nameof(DOFDOOR_LOCK), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOFDOOR_LOCK
        {
            get { return _DOFDOOR_LOCK; }
            set
            {
                if (value != this._DOFDOOR_LOCK)
                {
                    _DOFDOOR_LOCK = value;
                    NotifyPropertyChanged(nameof(DOFDOOR_LOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DOFDOOR_UNLOCK = new IOPortDescripter<bool>(nameof(DOFDOOR_UNLOCK), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOFDOOR_UNLOCK
        {
            get { return _DOFDOOR_UNLOCK; }
            set
            {
                if (value != this._DOFDOOR_UNLOCK)
                {
                    _DOFDOOR_UNLOCK = value;
                    NotifyPropertyChanged(nameof(DOFDOOR_UNLOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DOLDOOR_LOCK = new IOPortDescripter<bool>(nameof(DOLDOOR_LOCK), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOLDOOR_LOCK
        {
            get { return _DOLDOOR_LOCK; }
            set
            {
                if (value != this._DOLDOOR_LOCK)
                {
                    _DOLDOOR_LOCK = value;
                    NotifyPropertyChanged(nameof(DOLDOOR_LOCK));
                }
            }
        }

        private IOPortDescripter<bool> _DOLOADERDOOR_CLOSE = new IOPortDescripter<bool>(nameof(DOLOADERDOOR_CLOSE), EnumIOType.OUTPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DOLOADERDOOR_CLOSE
        {
            get { return _DOLOADERDOOR_CLOSE; }
            set
            {
                if (value != this._DOLOADERDOOR_CLOSE)
                {
                    _DOLOADERDOOR_CLOSE = value;
                    NotifyPropertyChanged(nameof(DOLOADERDOOR_CLOSE));
                }
            }
        }

        private IOPortDescripter<bool> _DOLOADERDOOR_OPEN = new IOPortDescripter<bool>(nameof(DOLOADERDOOR_OPEN), EnumIOType.OUTPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DOLOADERDOOR_OPEN
        {
            get { return _DOLOADERDOOR_OPEN; }
            set
            {
                if (value != this._DOLOADERDOOR_OPEN)
                {
                    _DOLOADERDOOR_OPEN = value;
                    NotifyPropertyChanged(nameof(DOLOADERDOOR_OPEN));
                }
            }
        }

        private IOPortDescripter<bool> _DOLOADERDOOR_SEAL_OPEN = new IOPortDescripter<bool>(nameof(DOLOADERDOOR_SEAL_OPEN), EnumIOType.OUTPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DOLOADERDOOR_SEAL_OPEN
        {
            get { return _DOLOADERDOOR_SEAL_OPEN; }
            set
            {
                if (value != this._DOLOADERDOOR_SEAL_OPEN)
                {
                    _DOLOADERDOOR_SEAL_OPEN = value;
                    NotifyPropertyChanged(nameof(DOLOADERDOOR_SEAL_OPEN));
                }
            }
        }
        private IOPortDescripter<bool> _DOLOADERDOOR_SEAL_CLOSE = new IOPortDescripter<bool>(nameof(DOLOADERDOOR_SEAL_CLOSE), EnumIOType.OUTPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DOLOADERDOOR_SEAL_CLOSE
        {
            get { return _DOLOADERDOOR_SEAL_CLOSE; }
            set
            {
                if (value != this._DOLOADERDOOR_SEAL_CLOSE)
                {
                    _DOLOADERDOOR_SEAL_CLOSE = value;
                    NotifyPropertyChanged(nameof(DOLOADERDOOR_SEAL_CLOSE));
                }
            }
        }

        private IOPortDescripter<bool> _DOCARDDOOR_OPEN = new IOPortDescripter<bool>(nameof(DOCARDDOOR_OPEN), EnumIOType.OUTPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DOCARDDOOR_OPEN
        {
            get { return _DOCARDDOOR_OPEN; }
            set
            {
                if (value != this._DOCARDDOOR_OPEN)
                {
                    _DOCARDDOOR_OPEN = value;
                    NotifyPropertyChanged(nameof(DOCARDDOOR_OPEN));
                }
            }
        }

        private IOPortDescripter<bool> _DOCARDDOOR_CLOSE = new IOPortDescripter<bool>(nameof(DOCARDDOOR_CLOSE), EnumIOType.OUTPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DOCARDDOOR_CLOSE
        {
            get { return _DOCARDDOOR_CLOSE; }
            set
            {
                if (value != this._DOCARDDOOR_CLOSE)
                {
                    _DOCARDDOOR_CLOSE = value;
                    NotifyPropertyChanged(nameof(DOCARDDOOR_CLOSE));
                }
            }
        }

        private IOPortDescripter<bool> _DOCARDDOOR_SEAL_OPEN = new IOPortDescripter<bool>(nameof(DOCARDDOOR_SEAL_OPEN), EnumIOType.OUTPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DOCARDDOOR_SEAL_OPEN
        {
            get { return _DOCARDDOOR_SEAL_OPEN; }
            set
            {
                if (value != this._DOCARDDOOR_SEAL_OPEN)
                {
                    _DOCARDDOOR_SEAL_OPEN = value;
                    NotifyPropertyChanged(nameof(DOCARDDOOR_SEAL_OPEN));
                }
            }
        }

        private IOPortDescripter<bool> _DOCARDDOOR_SEAL_CLOSE = new IOPortDescripter<bool>(nameof(DOCARDDOOR_SEAL_CLOSE), EnumIOType.OUTPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DOCARDDOOR_SEAL_CLOSE
        {
            get { return _DOCARDDOOR_SEAL_CLOSE; }
            set
            {
                if (value != this._DOCARDDOOR_SEAL_CLOSE)
                {
                    _DOCARDDOOR_SEAL_CLOSE = value;
                    NotifyPropertyChanged(nameof(DOCARDDOOR_SEAL_CLOSE));
                }
            }
        }

        private IOPortDescripter<bool> _DOCLP_LOCK_REQ = new IOPortDescripter<bool>(nameof(DOCLP_LOCK_REQ), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOCLP_LOCK_REQ
        {
            get { return _DOCLP_LOCK_REQ; }
            set
            {
                if (value != this._DOCLP_LOCK_REQ)
                {
                    _DOCLP_LOCK_REQ = value;
                    NotifyPropertyChanged(nameof(DOCLP_LOCK_REQ));
                }
            }
        }
        private IOPortDescripter<bool> _DOCLP_UNLOCK_REQ = new IOPortDescripter<bool>(nameof(DOCLP_UNLOCK_REQ), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOCLP_UNLOCK_REQ
        {
            get { return _DOCLP_UNLOCK_REQ; }
            set
            {
                if (value != this._DOCLP_UNLOCK_REQ)
                {
                    _DOCLP_UNLOCK_REQ = value;
                    NotifyPropertyChanged(nameof(DOCLP_UNLOCK_REQ));
                }
            }
        }
        private IOPortDescripter<bool> _DONEEDLE_BRUSH_UP = new IOPortDescripter<bool>(nameof(DONEEDLE_BRUSH_UP), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DONEEDLE_BRUSH_UP
        {
            get { return _DONEEDLE_BRUSH_UP; }
            set
            {
                if (value != this._DONEEDLE_BRUSH_UP)
                {
                    _DONEEDLE_BRUSH_UP = value;
                    NotifyPropertyChanged(nameof(DONEEDLE_BRUSH_UP));
                }
            }
        }
        private IOPortDescripter<bool> _DOBridgeBeamLock = new IOPortDescripter<bool>(nameof(DOBridgeBeamLock), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOBridgeBeamLock
        {
            get { return _DOBridgeBeamLock; }
            set
            {
                if (value != this._DOBridgeBeamLock)
                {
                    _DOBridgeBeamLock = value;
                    NotifyPropertyChanged(nameof(DOBridgeBeamLock));
                }
            }
        }
        private IOPortDescripter<bool> _DOBridgeBeamUnLock = new IOPortDescripter<bool>(nameof(DOBridgeBeamUnLock), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOBridgeBeamUnLock
        {
            get { return _DOBridgeBeamUnLock; }
            set
            {
                if (value != this._DOBridgeBeamUnLock)
                {
                    _DOBridgeBeamUnLock = value;
                    NotifyPropertyChanged(nameof(DOBridgeBeamUnLock));
                }
            }
        }
        private IOPortDescripter<bool> _DODD_SWING_UP = new IOPortDescripter<bool>(nameof(DODD_SWING_UP), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DODD_SWING_UP
        {
            get { return _DODD_SWING_UP; }
            set
            {
                if (value != this._DODD_SWING_UP)
                {
                    _DODD_SWING_UP = value;
                    NotifyPropertyChanged(nameof(DODD_SWING_UP));
                }
            }
        }
        private IOPortDescripter<bool> _DODD_SWING_DN = new IOPortDescripter<bool>(nameof(DODD_SWING_DN), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DODD_SWING_DN
        {
            get { return _DODD_SWING_DN; }
            set
            {
                if (value != this._DODD_SWING_DN)
                {
                    _DODD_SWING_DN = value;
                    NotifyPropertyChanged(nameof(DODD_SWING_DN));
                }
            }
        }
        private IOPortDescripter<bool> _DODD_FRONT_INNER_COVER_OPEN = new IOPortDescripter<bool>(nameof(DODD_FRONT_INNER_COVER_OPEN), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DODD_FRONT_INNER_COVER_OPEN
        {
            get { return _DODD_FRONT_INNER_COVER_OPEN; }
            set
            {
                if (value != this._DODD_FRONT_INNER_COVER_OPEN)
                {
                    _DODD_FRONT_INNER_COVER_OPEN = value;
                    NotifyPropertyChanged(nameof(DODD_FRONT_INNER_COVER_OPEN));
                }
            }
        }
        private IOPortDescripter<bool> _DOFOUP_Cover_Lock = new IOPortDescripter<bool>(nameof(DOFOUP_Cover_Lock), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOFOUP_Cover_Lock
        {
            get { return _DOFOUP_Cover_Lock; }
            set
            {
                if (value != this._DOFOUP_Cover_Lock)
                {
                    _DOFOUP_Cover_Lock = value;
                    NotifyPropertyChanged(nameof(DOFOUP_Cover_Lock));
                }
            }
        }
        private IOPortDescripter<bool> _DOT2K_CYLINDER_UP = new IOPortDescripter<bool>(nameof(DOT2K_CYLINDER_UP), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOT2K_CYLINDER_UP
        {
            get { return _DOT2K_CYLINDER_UP; }
            set
            {
                if (value != this._DOT2K_CYLINDER_UP)
                {
                    _DOT2K_CYLINDER_UP = value;
                    NotifyPropertyChanged(nameof(DOT2K_CYLINDER_UP));
                }
            }
        }
        private IOPortDescripter<bool> _DOT2K_CYLINDER_MID = new IOPortDescripter<bool>(nameof(DOT2K_CYLINDER_MID), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOT2K_CYLINDER_MID
        {
            get { return _DOT2K_CYLINDER_MID; }
            set
            {
                if (value != this._DOT2K_CYLINDER_MID)
                {
                    _DOT2K_CYLINDER_MID = value;
                    NotifyPropertyChanged(nameof(DOT2K_CYLINDER_MID));
                }
            }
        }
        private IOPortDescripter<bool> _DOT2K_CYLINDER_DOWN = new IOPortDescripter<bool>(nameof(DOT2K_CYLINDER_DOWN), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOT2K_CYLINDER_DOWN
        {
            get { return _DOT2K_CYLINDER_DOWN; }
            set
            {
                if (value != this._DOT2K_CYLINDER_DOWN)
                {
                    _DOT2K_CYLINDER_DOWN = value;
                    NotifyPropertyChanged(nameof(DOT2K_CYLINDER_DOWN));
                }
            }
        }

        private IOPortDescripter<bool> _DODRY_AIR_ON = new IOPortDescripter<bool>(nameof(DODRY_AIR_ON), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DODRY_AIR_ON
        {
            get { return _DODRY_AIR_ON; }
            set
            {
                if (value != this._DODRY_AIR_ON)
                {
                    _DODRY_AIR_ON = value;
                    NotifyPropertyChanged(nameof(DODRY_AIR_ON));
                }
            }
        }
        private IOPortDescripter<bool> _DOCHILLER_ENABLE_0 = new IOPortDescripter<bool>(nameof(DOCHILLER_ENABLE_0), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOCHILLER_ENABLE_0
        {
            get { return _DOCHILLER_ENABLE_0; }
            set
            {
                if (value != this._DOCHILLER_ENABLE_0)
                {
                    _DOCHILLER_ENABLE_0 = value;
                    NotifyPropertyChanged(nameof(DOCHILLER_ENABLE_0));
                }
            }
        }
        private IOPortDescripter<bool> _DOCHILLER_ENABLE_1 = new IOPortDescripter<bool>(nameof(DOCHILLER_ENABLE_1), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOCHILLER_ENABLE_1
        {
            get { return _DOCHILLER_ENABLE_1; }
            set
            {
                if (value != this._DOCHILLER_ENABLE_1)
                {
                    _DOCHILLER_ENABLE_1 = value;
                    NotifyPropertyChanged(nameof(DOCHILLER_ENABLE_1));
                }
            }
        }
        private IOPortDescripter<bool> _DOCOOLANT_VALVE = new IOPortDescripter<bool>(nameof(DOCOOLANT_VALVE), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOCOOLANT_VALVE
        {
            get { return _DOCOOLANT_VALVE; }
            set
            {
                if (value != this._DOCOOLANT_VALVE)
                {
                    _DOCOOLANT_VALVE = value;
                    NotifyPropertyChanged(nameof(DOCOOLANT_VALVE));
                }
            }
        }
        private IOPortDescripter<bool> _DOCOOLANT_OUT = new IOPortDescripter<bool>(nameof(DOCOOLANT_OUT), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOCOOLANT_OUT
        {
            get { return _DOCOOLANT_OUT; }
            set
            {
                if (value != this._DOCOOLANT_OUT)
                {
                    _DOCOOLANT_OUT = value;
                    NotifyPropertyChanged(nameof(DOCOOLANT_OUT));
                }
            }
        }

        private IOPortDescripter<bool> _DOCOLD_CHUCK_AIR = new IOPortDescripter<bool>(nameof(DOCOLD_CHUCK_AIR), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOCOLD_CHUCK_AIR
        {
            get { return _DOCOLD_CHUCK_AIR; }
            set
            {
                if (value != this._DOCOLD_CHUCK_AIR)
                {
                    _DOCOLD_CHUCK_AIR = value;
                    NotifyPropertyChanged(nameof(DOCOLD_CHUCK_AIR));
                }
            }
        }

        private IOPortDescripter<bool> _DOACS_INTERFACE_SIGNAL = new IOPortDescripter<bool>(nameof(DOACS_INTERFACE_SIGNAL), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOACS_INTERFACE_SIGNAL
        {
            get { return _DOACS_INTERFACE_SIGNAL; }
            set
            {
                if (value != this._DOACS_INTERFACE_SIGNAL)
                {
                    _DOACS_INTERFACE_SIGNAL = value;
                    NotifyPropertyChanged(nameof(DOACS_INTERFACE_SIGNAL));
                }
            }
        }
        private IOPortDescripter<bool> _DOFD_PREIN_FRONT = new IOPortDescripter<bool>(nameof(DOFD_PREIN_FRONT), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOFD_PREIN_FRONT
        {
            get { return _DOFD_PREIN_FRONT; }
            set
            {
                if (value != this._DOFD_PREIN_FRONT)
                {
                    _DOFD_PREIN_FRONT = value;
                    NotifyPropertyChanged(nameof(DOFD_PREIN_FRONT));
                }
            }
        }
        private IOPortDescripter<bool> _DOFD_PREIN_REAR = new IOPortDescripter<bool>(nameof(DOFD_PREIN_REAR), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOFD_PREIN_REAR
        {
            get { return _DOFD_PREIN_REAR; }
            set
            {
                if (value != this._DOFD_PREIN_REAR)
                {
                    _DOFD_PREIN_REAR = value;
                    NotifyPropertyChanged(nameof(DOFD_PREIN_REAR));
                }
            }
        }
        private IOPortDescripter<bool> _DOFD_THREE_LEGUP = new IOPortDescripter<bool>(nameof(DOFD_THREE_LEGUP), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOFD_THREE_LEGUP
        {
            get { return _DOFD_THREE_LEGUP; }
            set
            {
                if (value != this._DOFD_THREE_LEGUP)
                {
                    _DOFD_THREE_LEGUP = value;
                    NotifyPropertyChanged(nameof(DOFD_THREE_LEGUP));
                }
            }
        }
        private IOPortDescripter<bool> _DOTHREELEGDOWN = new IOPortDescripter<bool>(nameof(DOTHREELEGDOWN), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOTHREELEGDOWN
        {
            get { return _DOTHREELEGDOWN; }
            set
            {
                if (value != this._DOTHREELEGDOWN)
                {
                    _DOTHREELEGDOWN = value;
                    NotifyPropertyChanged(nameof(DOTHREELEGDOWN));
                }
            }
        }
        private IOPortDescripter<bool> _DOTESTER_HEAD_LOCK = new IOPortDescripter<bool>(nameof(DOTESTER_HEAD_LOCK), EnumIOType.OUTPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DOTESTER_HEAD_LOCK
        {
            get { return _DOTESTER_HEAD_LOCK; }
            set
            {
                if (value != this._DOTESTER_HEAD_LOCK)
                {
                    _DOTESTER_HEAD_LOCK = value;
                    NotifyPropertyChanged(nameof(DOTESTER_HEAD_LOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DOTESTER_HEAD_UNLOCK = new IOPortDescripter<bool>(nameof(DOTESTER_HEAD_UNLOCK), EnumIOType.OUTPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DOTESTER_HEAD_UNLOCK
        {
            get { return _DOTESTER_HEAD_UNLOCK; }
            set
            {
                if (value != this._DOTESTER_HEAD_UNLOCK)
                {
                    _DOTESTER_HEAD_UNLOCK = value;
                    NotifyPropertyChanged(nameof(DOTESTER_HEAD_UNLOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DOCARDTRAY_UNLOCK = new IOPortDescripter<bool>(nameof(DOCARDTRAY_UNLOCK), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOCARDTRAY_UNLOCK
        {
            get { return _DOCARDTRAY_UNLOCK; }
            set
            {
                if (value != this._DOCARDTRAY_UNLOCK)
                {
                    _DOCARDTRAY_UNLOCK = value;
                    NotifyPropertyChanged(nameof(DOCARDTRAY_UNLOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DOINSPECTION_COVER_LOCK = new IOPortDescripter<bool>(nameof(DOINSPECTION_COVER_LOCK), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOINSPECTION_COVER_LOCK
        {
            get { return _DOINSPECTION_COVER_LOCK; }
            set
            {
                if (value != this._DOINSPECTION_COVER_LOCK)
                {
                    _DOINSPECTION_COVER_LOCK = value;
                    NotifyPropertyChanged(nameof(DOINSPECTION_COVER_LOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DOSCAN_SENSOR_OUT = new IOPortDescripter<bool>(nameof(DOSCAN_SENSOR_OUT), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOSCAN_SENSOR_OUT
        {
            get { return _DOSCAN_SENSOR_OUT; }
            set
            {
                if (value != this._DOSCAN_SENSOR_OUT)
                {
                    _DOSCAN_SENSOR_OUT = value;
                    NotifyPropertyChanged(nameof(DOSCAN_SENSOR_OUT));
                }
            }
        }
        private IOPortDescripter<bool> _DOSERVICE_MANI_LOCK = new IOPortDescripter<bool>(nameof(DOSERVICE_MANI_LOCK), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOSERVICE_MANI_LOCK
        {
            get { return _DOSERVICE_MANI_LOCK; }
            set
            {
                if (value != this._DOSERVICE_MANI_LOCK)
                {
                    _DOSERVICE_MANI_LOCK = value;
                    NotifyPropertyChanged(nameof(DOSERVICE_MANI_LOCK));
                }
            }
        }
        private IOPortDescripter<bool> _DOTP_LOCK_REQ = new IOPortDescripter<bool>(nameof(DOTP_LOCK_REQ), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOTP_LOCK_REQ
        {
            get { return _DOTP_LOCK_REQ; }
            set
            {
                if (value != this._DOTP_LOCK_REQ)
                {
                    _DOTP_LOCK_REQ = value;
                    NotifyPropertyChanged(nameof(DOTP_LOCK_REQ));
                }
            }
        }
        private IOPortDescripter<bool> _DOTP_UNLOCK_REQ = new IOPortDescripter<bool>(nameof(DOTP_UNLOCK_REQ), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DOTP_UNLOCK_REQ
        {
            get { return _DOTP_UNLOCK_REQ; }
            set
            {
                if (value != this._DOTP_UNLOCK_REQ)
                {
                    _DOTP_UNLOCK_REQ = value;
                    NotifyPropertyChanged(nameof(DOTP_UNLOCK_REQ));
                }
            }
        }

        private IOPortDescripter<bool> _DO_X_BRAKERELEASE = new IOPortDescripter<bool>(nameof(DO_X_BRAKERELEASE), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DO_X_BRAKERELEASE
        {
            get { return _DO_X_BRAKERELEASE; }
            set
            {
                if (value != this._DO_X_BRAKERELEASE)
                {
                    _DO_X_BRAKERELEASE = value;
                    NotifyPropertyChanged(nameof(DO_X_BRAKERELEASE));
                }
            }
        }
        private IOPortDescripter<bool> _DO_Y_BRAKERELEASE = new IOPortDescripter<bool>(nameof(DO_Y_BRAKERELEASE), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DO_Y_BRAKERELEASE
        {
            get { return _DO_Y_BRAKERELEASE; }
            set
            {
                if (value != this._DO_Y_BRAKERELEASE)
                {
                    _DO_Y_BRAKERELEASE = value;
                    NotifyPropertyChanged(nameof(DO_Y_BRAKERELEASE));
                }
            }
        }

        private IOPortDescripter<bool> _DO_TRILEG_SUCTION = new IOPortDescripter<bool>(nameof(DO_TRILEG_SUCTION), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DO_TRILEG_SUCTION
        {
            get { return _DO_TRILEG_SUCTION; }
            set
            {
                if (value != this._DO_TRILEG_SUCTION)
                {
                    _DO_TRILEG_SUCTION = value;
                    NotifyPropertyChanged(nameof(DO_TRILEG_SUCTION));
                }
            }
        }
        private IOPortDescripter<bool> _DO_STAGE_BRAKER_RELEASE = new IOPortDescripter<bool>(nameof(DO_STAGE_BRAKER_RELEASE), EnumIOType.OUTPUT, EnumIOOverride.NLO);
        public IOPortDescripter<bool> DO_STAGE_BRAKER_RELEASE
        {
            get { return _DO_STAGE_BRAKER_RELEASE; }
            set
            {
                if (value != this._DO_STAGE_BRAKER_RELEASE)
                {
                    _DO_STAGE_BRAKER_RELEASE = value;
                    NotifyPropertyChanged(nameof(DO_STAGE_BRAKER_RELEASE));
                }
            }
        }

        private IOPortDescripter<bool> _DOEXTRA_CHUCK_AIR_READY = new IOPortDescripter<bool>(nameof(DOEXTRA_CHUCK_AIR_READY), EnumIOType.OUTPUT, EnumIOOverride.EMUL);
        /// <summary>
        /// Extra Chuck Vac을 위해 베큠 라인이 메인베큠과 별도로 추가되었음. 
        /// 추가된 베큠라인의 Air를 사용할 것인 지에 대한 출력값.
        /// Reverse 주의할 것.
        /// </summary>
        public IOPortDescripter<bool> DOEXTRA_CHUCK_AIR_READY
        {
            get { return _DOEXTRA_CHUCK_AIR_READY; }
            set
            {
                if (value != this._DOEXTRA_CHUCK_AIR_READY)
                {
                    _DOEXTRA_CHUCK_AIR_READY = value;
                    NotifyPropertyChanged(nameof(_DOEXTRA_CHUCK_AIR_READY));
                }
            }
        }
        //

        #region ==> Group Prober Card Change

        #region ==> DOUPMODULE_UP : UpModule(Chuck 옆에 있는 Card 지지대)을 올리는 IO, true : Up, false : Down
        private IOPortDescripter<bool> _DOUPMODULE_UP = new IOPortDescripter<bool>(nameof(DOUPMODULE_UP), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOUPMODULE_UP
        {
            get { return _DOUPMODULE_UP; }
            set
            {
                if (value != this._DOUPMODULE_UP)
                {
                    _DOUPMODULE_UP = value;
                    NotifyPropertyChanged(nameof(DOUPMODULE_UP));
                }
            }
        }
        #endregion

        #region ==> DOUPMODULE_DOWN : UpModule(Chuck 옆에 있는 Card 지지대)을 내리는 IO, true : Down, false : Up
        private IOPortDescripter<bool> _DOUPMODULE_DOWN = new IOPortDescripter<bool>(nameof(DOUPMODULE_DOWN), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOUPMODULE_DOWN
        {
            get { return _DOUPMODULE_DOWN; }
            set
            {
                if (value != this._DOUPMODULE_DOWN)
                {
                    _DOUPMODULE_DOWN = value;
                    NotifyPropertyChanged(nameof(DOUPMODULE_DOWN));
                }
            }
        }
        #endregion

        #region ==> DOUPMODULE_VACU : UpModule(Chuck 옆에 있는 Card 지지대)의 Vacuum 제어 IO, true : Vacuum On, false : Vacuum off
        private IOPortDescripter<bool> _DOUPMODULE_VACU = new IOPortDescripter<bool>(nameof(DOUPMODULE_VACU), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOUPMODULE_VACU
        {
            get { return _DOUPMODULE_VACU; }
            set
            {
                if (value != this._DOUPMODULE_VACU)
                {
                    _DOUPMODULE_VACU = value;
                    NotifyPropertyChanged(nameof(DOUPMODULE_VACU));
                }
            }
        }
        #endregion

        #region ==> DOTPLATE_PCLATCH_LOCK : 상판에 달려 있는 낙하 방지 Latch 제어, true : Lock, false : UnLock
        private IOPortDescripter<bool> _DOTPLATE_PCLATCH_LOCK = new IOPortDescripter<bool>(nameof(DOTPLATE_PCLATCH_LOCK), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOTPLATE_PCLATCH_LOCK
        {
            get { return _DOTPLATE_PCLATCH_LOCK; }
            set
            {
                if (value != this._DOTPLATE_PCLATCH_LOCK)
                {
                    _DOTPLATE_PCLATCH_LOCK = value;
                    NotifyPropertyChanged(nameof(DOTPLATE_PCLATCH_LOCK));
                }
            }
        }
        #endregion

        #region ==> DOPOGOCARD_VACU_RELEASE : POGO에 Card 접촉면 쪽 진공 파기 제어, 흡착된 Card 진공 해제 용도, true : Blow, false : Not Blow
        private IOPortDescripter<bool> _DOPOGOCARD_VACU_RELEASE = new IOPortDescripter<bool>(nameof(DOPOGOCARD_VACU_RELEASE), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOPOGOCARD_VACU_RELEASE
        {
            get { return _DOPOGOCARD_VACU_RELEASE; }
            set
            {
                if (value != this._DOPOGOCARD_VACU_RELEASE)
                {
                    _DOPOGOCARD_VACU_RELEASE = value;
                    NotifyPropertyChanged(nameof(DOPOGOCARD_VACU_RELEASE));
                }
            }
        }
        #endregion

        #region ==> DOPOGOCARD_VACU : POGO에 Card 접촉면 쪽 진공 제어, true : Vacuum On, false : Vacuum Off
        private IOPortDescripter<bool> _DOPOGOCARD_VACU = new IOPortDescripter<bool>(nameof(DOPOGOCARD_VACU), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOPOGOCARD_VACU
        {
            get { return _DOPOGOCARD_VACU; }
            set
            {
                if (value != this._DOPOGOCARD_VACU)
                {
                    _DOPOGOCARD_VACU = value;
                    NotifyPropertyChanged(nameof(DOPOGOCARD_VACU));
                }
            }
        }
        #endregion

        #region ==> DOPOGOCARD_VACU_RELEASE_SUB : POGO에 Card 접촉면 쪽 진공 서브 파기 제어, 흡착된 Card 진공 해제 용도, true : Blow, false : Not Blow
        private IOPortDescripter<bool> _DOPOGOCARD_VACU_RELEASE_SUB = new IOPortDescripter<bool>(nameof(DOPOGOCARD_VACU_RELEASE_SUB), EnumIOType.OUTPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DOPOGOCARD_VACU_RELEASE_SUB
        {
            get { return _DOPOGOCARD_VACU_RELEASE_SUB; }
            set
            {
                if (value != this._DOPOGOCARD_VACU_RELEASE_SUB)
                {
                    _DOPOGOCARD_VACU_RELEASE_SUB = value;
                    NotifyPropertyChanged(nameof(DOPOGOCARD_VACU_RELEASE_SUB));
                }
            }
        }
        #endregion

        #region ==> DOPOGOCARD_VACU_SUB : POGO에 Card 접촉면 쪽 서브 진공 제어, true : Vacuum On, false : Vacuum Off
        private IOPortDescripter<bool> _DOPOGOCARD_VACU_SUB = new IOPortDescripter<bool>(nameof(DOPOGOCARD_VACU_SUB), EnumIOType.OUTPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DOPOGOCARD_VACU_SUB
        {
            get { return _DOPOGOCARD_VACU_SUB; }
            set
            {
                if (value != this._DOPOGOCARD_VACU_SUB)
                {
                    _DOPOGOCARD_VACU_SUB = value;
                    NotifyPropertyChanged(nameof(DOPOGOCARD_VACU_SUB));
                }
            }
        }
        #endregion

        #region ==> DOPOGOTESTER_VACU_RELEASE : 상판에 Tester 접촉면 쪽 진공 파기 제어, 흡착된 Tester 진공 해제 용도, true : Blow, false : Not Blow
        private IOPortDescripter<bool> _DOPOGOTESTER_VACU_RELEASE = new IOPortDescripter<bool>(nameof(DOPOGOTESTER_VACU_RELEASE), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOPOGOTESTER_VACU_RELEASE
        {
            get { return _DOPOGOTESTER_VACU_RELEASE; }
            set
            {
                if (value != this._DOPOGOTESTER_VACU_RELEASE)
                {
                    _DOPOGOTESTER_VACU_RELEASE = value;
                    NotifyPropertyChanged(nameof(DOPOGOTESTER_VACU_RELEASE));
                }
            }
        }
        #endregion

        #region ==> DOPOGOTESTER_VACU : 상판에 Tester 접촉면 쪽 진공 제어, true : Vacuum on, false : Vacuum off
        private IOPortDescripter<bool> _DOPOGOTESTER_VACU = new IOPortDescripter<bool>(nameof(DOPOGOTESTER_VACU), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOPOGOTESTER_VACU
        {
            get { return _DOPOGOTESTER_VACU; }
            set
            {
                if (value != this._DOPOGOTESTER_VACU)
                {
                    _DOPOGOTESTER_VACU = value;
                    NotifyPropertyChanged(nameof(DOPOGOTESTER_VACU));
                }
            }
        }
        #endregion

        #region ==> DOTESTER_CLAMPED : MANIPULATOR에서 TESTER를 떼어갈 수 없는 상태. (Tester Vac : ON || Card Vac : ON)
        private IOPortDescripter<bool> _DOTESTER_CLAMPED = new IOPortDescripter<bool>(nameof(DOTESTER_CLAMPED), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOTESTER_CLAMPED
        {
            get { return _DOTESTER_CLAMPED; }
            set
            {
                if (value != this._DOTESTER_CLAMPED)
                {
                    _DOTESTER_CLAMPED = value;
                    NotifyPropertyChanged(nameof(DOTESTER_CLAMPED));
                }
            }
        }
        #endregion

        #region ==> DOTESTER_UNCLAMPED : MANIPULATOR에서 TESTER를 떼어갈 수 있는 상태. (Tester Vac : OFF && Card Vac : OFF)
        private IOPortDescripter<bool> _DOTESTER_UNCLAMPED = new IOPortDescripter<bool>(nameof(DOTESTER_UNCLAMPED), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOTESTER_UNCLAMPED
        {
            get { return _DOTESTER_UNCLAMPED; }
            set
            {
                if (value != this._DOTESTER_UNCLAMPED)
                {
                    _DOTESTER_CLAMPED = value;
                    NotifyPropertyChanged(nameof(DOTESTER_UNCLAMPED));
                }
            }
        }
        #endregion

        #endregion
        #region ==> BERNOULLI
        private IOPortDescripter<bool> _DOCHUCK_BLOW = new IOPortDescripter<bool>(nameof(DOCHUCK_BLOW), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOCHUCK_BLOW
        {
            get { return _DOCHUCK_BLOW; }
            set
            {
                if (value != this._DOCHUCK_BLOW)
                {
                    _DOCHUCK_BLOW = value;
                    NotifyPropertyChanged(nameof(DOCHUCK_BLOW));
                }
            }
        }
        private IOPortDescripter<bool> _DOCHUCK_BLOW_12 = new IOPortDescripter<bool>(nameof(DOCHUCK_BLOW_12), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOCHUCK_BLOW_12
        {
            get { return _DOCHUCK_BLOW_12; }
            set
            {
                if (value != this._DOCHUCK_BLOW_12)
                {
                    _DOCHUCK_BLOW_12 = value;
                    NotifyPropertyChanged(nameof(DOCHUCK_BLOW_12));
                }
            }
        }

        private IOPortDescripter<bool> _DOBERNOULLI_HANDLER_UP = new IOPortDescripter<bool>(nameof(DOBERNOULLI_HANDLER_UP), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBERNOULLI_HANDLER_UP
        {
            get { return _DOBERNOULLI_HANDLER_UP; }
            set
            {
                if (value != this._DOBERNOULLI_HANDLER_UP)
                {
                    _DOBERNOULLI_HANDLER_UP = value;
                    NotifyPropertyChanged(nameof(DOBERNOULLI_HANDLER_UP));
                }
            }
        }
        private IOPortDescripter<bool> _DOBERNOULLI_HANDLER_DOWN = new IOPortDescripter<bool>(nameof(DOBERNOULLI_HANDLER_DOWN), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBERNOULLI_HANDLER_DOWN
        {
            get { return _DOBERNOULLI_HANDLER_DOWN; }
            set
            {
                if (value != this._DOBERNOULLI_HANDLER_DOWN)
                {
                    _DOBERNOULLI_HANDLER_DOWN = value;
                    NotifyPropertyChanged(nameof(DOBERNOULLI_HANDLER_DOWN));
                }
            }
        }
        private IOPortDescripter<bool> _DOBERNOULLI_6INCH = new IOPortDescripter<bool>(nameof(DOBERNOULLI_6INCH), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBERNOULLI_6INCH
        {
            get { return _DOBERNOULLI_6INCH; }
            set
            {
                if (value != this._DOBERNOULLI_6INCH)
                {
                    _DOBERNOULLI_6INCH = value;
                    NotifyPropertyChanged(nameof(DOBERNOULLI_6INCH));
                }
            }
        }
        private IOPortDescripter<bool> _DOBERNOULLI_8INCH = new IOPortDescripter<bool>(nameof(DOBERNOULLI_8INCH), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBERNOULLI_8INCH
        {
            get { return _DOBERNOULLI_8INCH; }
            set
            {
                if (value != this._DOBERNOULLI_8INCH)
                {
                    _DOBERNOULLI_8INCH = value;
                    NotifyPropertyChanged(nameof(DOBERNOULLI_8INCH));
                }
            }
        }
        private IOPortDescripter<bool> _DOBERNOULLI_12INCH = new IOPortDescripter<bool>(nameof(DOBERNOULLI_12INCH), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBERNOULLI_12INCH
        {
            get { return _DOBERNOULLI_12INCH; }
            set
            {
                if (value != this._DOBERNOULLI_12INCH)
                {
                    _DOBERNOULLI_12INCH = value;
                    NotifyPropertyChanged(nameof(DOBERNOULLI_12INCH));
                }
            }
        }
        private IOPortDescripter<bool> _DOBERNOULLI_ALIGN_RETRACT = new IOPortDescripter<bool>(nameof(DOBERNOULLI_ALIGN_RETRACT), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBERNOULLI_ALIGN_RETRACT
        {
            get { return _DOBERNOULLI_ALIGN_RETRACT; }
            set
            {
                if (value != this._DOBERNOULLI_ALIGN_RETRACT)
                {
                    _DOBERNOULLI_ALIGN_RETRACT = value;
                    NotifyPropertyChanged(nameof(DOBERNOULLI_ALIGN_RETRACT));
                }
            }
        }
        private IOPortDescripter<bool> _DOBERNOULLI_ALIGN_EXTEND = new IOPortDescripter<bool>(nameof(DOBERNOULLI_ALIGN_EXTEND), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBERNOULLI_ALIGN_EXTEND
        {
            get { return _DOBERNOULLI_ALIGN_EXTEND; }
            set
            {
                if (value != this._DOBERNOULLI_ALIGN_EXTEND)
                {
                    _DOBERNOULLI_ALIGN_EXTEND = value;
                    NotifyPropertyChanged(nameof(DOBERNOULLI_ALIGN_EXTEND));
                }
            }
        }


        private IOPortDescripter<bool> _DOBERNOULLI_ANTIPAD = new IOPortDescripter<bool>(nameof(DOBERNOULLI_ANTIPAD), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBERNOULLI_ANTIPAD
        {
            get { return _DOBERNOULLI_ANTIPAD; }
            set
            {
                if (value != this._DOBERNOULLI_ANTIPAD)
                {
                    _DOBERNOULLI_ANTIPAD = value;
                    NotifyPropertyChanged(nameof(DOBERNOULLI_ANTIPAD));
                }
            }
        }

        private IOPortDescripter<bool> _DOBERNOULLI_ANTIPAD2 = new IOPortDescripter<bool>(nameof(DOBERNOULLI_ANTIPAD2), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBERNOULLI_ANTIPAD2
        {
            get { return _DOBERNOULLI_ANTIPAD2; }
            set
            {
                if (value != this._DOBERNOULLI_ANTIPAD2)
                {
                    _DOBERNOULLI_ANTIPAD2 = value;
                    NotifyPropertyChanged(nameof(DOBERNOULLI_ANTIPAD2));
                }
            }
        }
        #endregion
        #endregion

        #region Sorter Outputs

        private IOPortDescripter<bool> _DO_CH0P00 = new IOPortDescripter<bool>(nameof(DO_CH0P00), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P00
        {
            get { return _DO_CH0P00; }
            set
            {
                if (value != this._DO_CH0P00)
                {
                    _DO_CH0P00 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P00));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P01 = new IOPortDescripter<bool>(nameof(DO_CH0P01), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P01
        {
            get { return _DO_CH0P01; }
            set
            {
                if (value != this._DO_CH0P01)
                {
                    _DO_CH0P01 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P01));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P02 = new IOPortDescripter<bool>(nameof(DO_CH0P02), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P02
        {
            get { return _DO_CH0P02; }
            set
            {
                if (value != this._DO_CH0P02)
                {
                    _DO_CH0P02 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P02));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P03 = new IOPortDescripter<bool>(nameof(DO_CH0P03), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P03
        {
            get { return _DO_CH0P03; }
            set
            {
                if (value != this._DO_CH0P03)
                {
                    _DO_CH0P03 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P03));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P04 = new IOPortDescripter<bool>(nameof(DO_CH0P04), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P04
        {
            get { return _DO_CH0P04; }
            set
            {
                if (value != this._DO_CH0P04)
                {
                    _DO_CH0P04 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P04));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P05 = new IOPortDescripter<bool>(nameof(DO_CH0P05), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P05
        {
            get { return _DO_CH0P05; }
            set
            {
                if (value != this._DO_CH0P05)
                {
                    _DO_CH0P05 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P05));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P06 = new IOPortDescripter<bool>(nameof(DO_CH0P06), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P06
        {
            get { return _DO_CH0P06; }
            set
            {
                if (value != this._DO_CH0P06)
                {
                    _DO_CH0P06 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P06));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P07 = new IOPortDescripter<bool>(nameof(DO_CH0P07), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P07
        {
            get { return _DO_CH0P07; }
            set
            {
                if (value != this._DO_CH0P07)
                {
                    _DO_CH0P07 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P07));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P08 = new IOPortDescripter<bool>(nameof(DO_CH0P08), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P08
        {
            get { return _DO_CH0P08; }
            set
            {
                if (value != this._DO_CH0P08)
                {
                    _DO_CH0P08 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P08));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P09 = new IOPortDescripter<bool>(nameof(DO_CH0P09), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P09
        {
            get { return _DO_CH0P09; }
            set
            {
                if (value != this._DO_CH0P09)
                {
                    _DO_CH0P09 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P09));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P10 = new IOPortDescripter<bool>(nameof(DO_CH0P10), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P10
        {
            get { return _DO_CH0P10; }
            set
            {
                if (value != this._DO_CH0P10)
                {
                    _DO_CH0P10 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P10));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P11 = new IOPortDescripter<bool>(nameof(DO_CH0P11), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P11
        {
            get { return _DO_CH0P11; }
            set
            {
                if (value != this._DO_CH0P11)
                {
                    _DO_CH0P11 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P11));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P12 = new IOPortDescripter<bool>(nameof(DO_CH0P12), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P12
        {
            get { return _DO_CH0P12; }
            set
            {
                if (value != this._DO_CH0P12)
                {
                    _DO_CH0P12 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P12));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P13 = new IOPortDescripter<bool>(nameof(DO_CH0P13), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P13
        {
            get { return _DO_CH0P13; }
            set
            {
                if (value != this._DO_CH0P13)
                {
                    _DO_CH0P13 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P13));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P14 = new IOPortDescripter<bool>(nameof(DO_CH0P14), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P14
        {
            get { return _DO_CH0P14; }
            set
            {
                if (value != this._DO_CH0P14)
                {
                    _DO_CH0P14 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P14));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH0P15 = new IOPortDescripter<bool>(nameof(DO_CH0P15), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH0P15
        {
            get { return _DO_CH0P15; }
            set
            {
                if (value != this._DO_CH0P15)
                {
                    _DO_CH0P15 = value;
                    NotifyPropertyChanged(nameof(DO_CH0P15));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P00 = new IOPortDescripter<bool>(nameof(DO_CH1P00), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P00
        {
            get { return _DO_CH1P00; }
            set
            {
                if (value != this._DO_CH1P00)
                {
                    _DO_CH1P00 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P00));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P01 = new IOPortDescripter<bool>(nameof(DO_CH1P01), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P01
        {
            get { return _DO_CH1P01; }
            set
            {
                if (value != this._DO_CH1P01)
                {
                    _DO_CH1P01 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P01));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P02 = new IOPortDescripter<bool>(nameof(DO_CH1P02), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P02
        {
            get { return _DO_CH1P02; }
            set
            {
                if (value != this._DO_CH1P02)
                {
                    _DO_CH1P02 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P02));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P03 = new IOPortDescripter<bool>(nameof(DO_CH1P03), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P03
        {
            get { return _DO_CH1P03; }
            set
            {
                if (value != this._DO_CH1P03)
                {
                    _DO_CH1P03 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P03));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P04 = new IOPortDescripter<bool>(nameof(DO_CH1P04), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P04
        {
            get { return _DO_CH1P04; }
            set
            {
                if (value != this._DO_CH1P04)
                {
                    _DO_CH1P04 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P04));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P05 = new IOPortDescripter<bool>(nameof(DO_CH1P05), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P05
        {
            get { return _DO_CH1P05; }
            set
            {
                if (value != this._DO_CH1P05)
                {
                    _DO_CH1P05 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P05));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P06 = new IOPortDescripter<bool>(nameof(DO_CH1P06), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P06
        {
            get { return _DO_CH1P06; }
            set
            {
                if (value != this._DO_CH1P06)
                {
                    _DO_CH1P06 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P06));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P07 = new IOPortDescripter<bool>(nameof(DO_CH1P07), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P07
        {
            get { return _DO_CH1P07; }
            set
            {
                if (value != this._DO_CH1P07)
                {
                    _DO_CH1P07 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P07));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P08 = new IOPortDescripter<bool>(nameof(DO_CH1P08), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P08
        {
            get { return _DO_CH1P08; }
            set
            {
                if (value != this._DO_CH1P08)
                {
                    _DO_CH1P08 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P08));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P09 = new IOPortDescripter<bool>(nameof(DO_CH1P09), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P09
        {
            get { return _DO_CH1P09; }
            set
            {
                if (value != this._DO_CH1P09)
                {
                    _DO_CH1P09 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P09));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P10 = new IOPortDescripter<bool>(nameof(DO_CH1P10), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P10
        {
            get { return _DO_CH1P10; }
            set
            {
                if (value != this._DO_CH1P10)
                {
                    _DO_CH1P10 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P10));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P11 = new IOPortDescripter<bool>(nameof(DO_CH1P11), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P11
        {
            get { return _DO_CH1P11; }
            set
            {
                if (value != this._DO_CH1P11)
                {
                    _DO_CH1P11 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P11));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P12 = new IOPortDescripter<bool>(nameof(DO_CH1P12), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P12
        {
            get { return _DO_CH1P12; }
            set
            {
                if (value != this._DO_CH1P12)
                {
                    _DO_CH1P12 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P12));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P13 = new IOPortDescripter<bool>(nameof(DO_CH1P13), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P13
        {
            get { return _DO_CH1P13; }
            set
            {
                if (value != this._DO_CH1P13)
                {
                    _DO_CH1P13 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P13));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P14 = new IOPortDescripter<bool>(nameof(DO_CH1P14), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P14
        {
            get { return _DO_CH1P14; }
            set
            {
                if (value != this._DO_CH1P14)
                {
                    _DO_CH1P14 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P14));
                }
            }
        }

        private IOPortDescripter<bool> _DO_CH1P15 = new IOPortDescripter<bool>(nameof(DO_CH1P15), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DO_CH1P15
        {
            get { return _DO_CH1P15; }
            set
            {
                if (value != this._DO_CH1P15)
                {
                    _DO_CH1P15 = value;
                    NotifyPropertyChanged(nameof(DO_CH1P15));
                }
            }
        }
        #endregion

        // 250911 LJH 항목 추가
        private IOPortDescripter<bool> _DOARM1Vac = new IOPortDescripter<bool>(nameof(DOARM1Vac), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOARM1Vac
        {
            get { return _DOARM1Vac; }
            set
            {
                if (value != this._DOARM1Vac)
                {
                    _DOARM1Vac = value;
                    NotifyPropertyChanged(nameof(DOARM1Vac));
                }
            }
        }

        private IOPortDescripter<bool> _DOARMVac2 = new IOPortDescripter<bool>(nameof(DOARMVac2), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOARMVac2
        {
            get { return _DOARMVac2; }
            set
            {
                if (value != this._DOARMVac2)
                {
                    _DOARMVac2 = value;
                    NotifyPropertyChanged(nameof(DOARMVac2));
                }
            }
        }

        private IOPortDescripter<bool> _DOCCArmVac = new IOPortDescripter<bool>(nameof(DOCCArmVac), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOCCArmVac
        {
            get { return _DOCCArmVac; }
            set
            {
                if (value != this._DOCCArmVac)
                {
                    _DOCCArmVac = value;
                    NotifyPropertyChanged(nameof(DOCCArmVac));
                }
            }
        }

        private IOPortDescripter<bool> _DOCCArmVac_Break = new IOPortDescripter<bool>(nameof(DOCCArmVac_Break), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOCCArmVac_Break
        {
            get { return _DOCCArmVac_Break; }
            set
            {
                if (value != this._DOCCArmVac_Break)
                {
                    _DOCCArmVac_Break = value;
                    NotifyPropertyChanged(nameof(DOCCArmVac_Break));
                }
            }
        }

        private IOPortDescripter<bool> _DOBuffVacs_0 = new IOPortDescripter<bool>(nameof(DOBuffVacs_0), EnumIOType.OUTPUT);

        public IOPortDescripter<bool> DOBuffVacs_0
        {
            get { return _DOBuffVacs_0; }
            set
            {
                if (value != this._DOBuffVacs_0)
                {
                    _DOBuffVacs_0 = value;
                    NotifyPropertyChanged(nameof(DOBuffVacs_0));
                }
            }
        }

        private IOPortDescripter<bool> _DOBuffVacs_1 = new IOPortDescripter<bool>(nameof(DOBuffVacs_1), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBuffVacs_1
        {
            get { return _DOBuffVacs_1; }
            set
            {
                if (value != this._DOBuffVacs_1)
                {
                    _DOBuffVacs_1 = value;
                    NotifyPropertyChanged(nameof(DOBuffVacs_1));
                }
            }
        }

        private IOPortDescripter<bool> _DOBuffVacs_2 = new IOPortDescripter<bool>(nameof(DOBuffVacs_2), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBuffVacs_2
        {
            get { return _DOBuffVacs_2; }
            set
            {
                if (value != this._DOBuffVacs_2)
                {
                    _DOBuffVacs_2 = value;
                    NotifyPropertyChanged(nameof(DOBuffVacs_2));
                }
            }
        }

        private IOPortDescripter<bool> _DOBuffVacs_3 = new IOPortDescripter<bool>(nameof(DOBuffVacs_3), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBuffVacs_3
        {
            get { return _DOBuffVacs_3; }
            set
            {
                if (value != this._DOBuffVacs_3)
                {
                    _DOBuffVacs_3 = value;
                    NotifyPropertyChanged(nameof(DOBuffVacs_3));
                }
            }
        }

        private IOPortDescripter<bool> _DOBuffVacs_4 = new IOPortDescripter<bool>(nameof(DOBuffVacs_4), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOBuffVacs_4
        {
            get { return _DOBuffVacs_4; }
            set
            {
                if (value != this._DOBuffVacs_4)
                {
                    _DOBuffVacs_4 = value;
                    NotifyPropertyChanged(nameof(DOBuffVacs_4));
                }
            }
        }
        // <-- 251017 sebas 총 10개
        private IOPortDescripter<bool> _DO_ARM_VACON1 = new IOPortDescripter<bool>(nameof(DO_ARM_VACON1), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_ARM_VACON1
        {
            get { return _DO_ARM_VACON1; }
            set
            {
                if (value != this._DO_ARM_VACON1)
                {
                    _DO_ARM_VACON1 = value;
                    NotifyPropertyChanged(nameof(DO_ARM_VACON1));
                }
            }
        }
        private IOPortDescripter<bool> _DO_ARM_VACOFF1 = new IOPortDescripter<bool>(nameof(DO_ARM_VACOFF1), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_ARM_VACOFF1
        {
            get { return _DO_ARM_VACOFF1; }
            set
            {
                if (value != this._DO_ARM_VACOFF1)
                {
                    _DO_ARM_VACOFF1 = value;
                    NotifyPropertyChanged(nameof(DO_ARM_VACOFF1));
                }
            }
        }
        private IOPortDescripter<bool> _DO_ARM_AIR1 = new IOPortDescripter<bool>(nameof(DO_ARM_AIR1), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_ARM_AIR1
        {
            get { return _DO_ARM_AIR1; }
            set
            {
                if (value != this._DO_ARM_AIR1)
                {
                    _DO_ARM_AIR1 = value;
                    NotifyPropertyChanged(nameof(DO_ARM_AIR1));
                }
            }
        }
        private IOPortDescripter<bool> _DO_ARM_AIR1_OFF = new IOPortDescripter<bool>(nameof(DO_ARM_AIR1_OFF), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_ARM_AIR1_OFF
        {
            get { return _DO_ARM_AIR1_OFF; }
            set
            {
                if (value != this._DO_ARM_AIR1_OFF)
                {
                    _DO_ARM_AIR1_OFF = value;
                    NotifyPropertyChanged(nameof(DO_ARM_AIR1_OFF));
                }
            }
        }
        private IOPortDescripter<bool> _DO_ARM_VACON2 = new IOPortDescripter<bool>(nameof(DO_ARM_VACON2), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_ARM_VACON2
        {
            get { return _DO_ARM_VACON2; }
            set
            {
                if (value != this._DO_ARM_VACON2)
                {
                    _DO_ARM_VACON2 = value;
                    NotifyPropertyChanged(nameof(DO_ARM_VACON2));
                }
            }
        }
        private IOPortDescripter<bool> _DO_ARM_VACOFF2 = new IOPortDescripter<bool>(nameof(DO_ARM_VACOFF2), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_ARM_VACOFF2
        {
            get { return _DO_ARM_VACOFF2; }
            set
            {
                if (value != this._DO_ARM_VACOFF2)
                {
                    _DO_ARM_VACOFF2 = value;
                    NotifyPropertyChanged(nameof(DO_ARM_VACOFF2));
                }
            }
        }
        private IOPortDescripter<bool> _DO_ARM_AIR2 = new IOPortDescripter<bool>(nameof(DO_ARM_AIR2), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_ARM_AIR2
        {
            get { return _DO_ARM_AIR2; }
            set
            {
                if (value != this._DO_ARM_AIR2)
                {
                    _DO_ARM_AIR2 = value;
                    NotifyPropertyChanged(nameof(DO_ARM_AIR2));
                }
            }
        }
        private IOPortDescripter<bool> _DO_ARM_AIR2_OFF = new IOPortDescripter<bool>(nameof(DO_ARM_AIR2_OFF), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_ARM_AIR2_OFF
        {
            get { return _DO_ARM_AIR2_OFF; }
            set
            {
                if (value != this._DO_ARM_AIR2_OFF)
                {
                    _DO_ARM_AIR2_OFF = value;
                    NotifyPropertyChanged(nameof(DO_ARM_AIR2_OFF));
                }
            }
        }
        private IOPortDescripter<bool> _DO_FD_VAC = new IOPortDescripter<bool>(nameof(DO_FD_VAC), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_FD_VAC
        {
            get { return _DO_FD_VAC; }
            set
            {
                if (value != this._DO_FD_VAC)
                {
                    _DO_FD_VAC = value;
                    NotifyPropertyChanged(nameof(DO_FD_VAC));
                }
            }
        }
        private IOPortDescripter<bool> _DO_EJ_VAC = new IOPortDescripter<bool>(nameof(DO_EJ_VAC), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_EJ_VAC
        {
            get { return _DO_EJ_VAC; }
            set
            {
                if (value != this._DO_EJ_VAC)
                {
                    _DO_EJ_VAC = value;
                    NotifyPropertyChanged(nameof(DO_EJ_VAC));
                }
            }
        }
        private IOPortDescripter<bool> _DO_MAGNETIC1 = new IOPortDescripter<bool>(nameof(DO_MAGNETIC1), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_MAGNETIC1
        {
            get { return _DO_MAGNETIC1; }
            set
            {
                if (value != this._DO_MAGNETIC1)
                {
                    _DO_MAGNETIC1 = value;
                    NotifyPropertyChanged(nameof(DO_MAGNETIC1));
                }
            }
        }
        private IOPortDescripter<bool> _DO_MAGNETIC2 = new IOPortDescripter<bool>(nameof(DO_MAGNETIC2), EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DO_MAGNETIC2
        {
            get { return _DO_MAGNETIC2; }
            set
            {
                if (value != this._DO_MAGNETIC2)
                {
                    _DO_MAGNETIC2 = value;
                    NotifyPropertyChanged(nameof(DO_MAGNETIC2));
                }
            }
        }
        // -->
    }

    [Serializable]
    public class RemoteInputPortDefinitions : INotifyPropertyChanged, IParamNode
    {
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

        public RemoteInputPortDefinitions()
        {

        }

        #region Loader Inputs
        private IOPortDescripter<bool> _DIARM1VAC = new IOPortDescripter<bool>("DIARM1VAC", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIARM1VAC
        {
            get { return _DIARM1VAC; }
            set
            {
                if (value != this._DIARM1VAC)
                {
                    _DIARM1VAC = value;
                    NotifyPropertyChanged("DIARM1VAC");
                }
            }
        }
        private IOPortDescripter<bool> _DIARM2VAC = new IOPortDescripter<bool>("DIARM2VAC", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIARM2VAC
        {
            get { return _DIARM2VAC; }
            set
            {
                if (value != this._DIARM2VAC)
                {
                    _DIARM2VAC = value;
                    NotifyPropertyChanged("DIARM2VAC");
                }
            }
        }
        private IOPortDescripter<bool> _DICCARMVAC = new IOPortDescripter<bool>("DICCARMVAC", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICCARMVAC
        {
            get { return _DICCARMVAC; }
            set
            {
                if (value != this._DICCARMVAC)
                {
                    _DICCARMVAC = value;
                    NotifyPropertyChanged("DICCARMVAC");
                }
            }
        }
        private IOPortDescripter<bool> _DIEMGState = new IOPortDescripter<bool>("DIEMGState", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIEMGState
        {
            get { return _DIEMGState; }
            set
            {
                if (value != this._DIEMGState)
                {
                    _DIEMGState = value;
                    NotifyPropertyChanged("DIEMGState");
                }
            }
        }
        private IOPortDescripter<bool> _DILeftDoorOpen = new IOPortDescripter<bool>("DILeftDoorOpen", EnumIOType.INPUT);
        public IOPortDescripter<bool> DILeftDoorOpen
        {
            get { return _DILeftDoorOpen; }
            set
            {
                if (value != this._DILeftDoorOpen)
                {
                    _DILeftDoorOpen = value;
                    NotifyPropertyChanged("DILeftDoorOpen");
                }
            }
        }
        private IOPortDescripter<bool> _DICardExistSensorInBuffer = new IOPortDescripter<bool>("DICardExistSensorInBuffer", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICardExistSensorInBuffer
        {
            get { return _DICardExistSensorInBuffer; }
            set
            {
                if (value != this._DICardExistSensorInBuffer)
                {
                    _DICardExistSensorInBuffer = value;
                    NotifyPropertyChanged("DICardExistSensorInBuffer");
                }
            }
        }
        private IOPortDescripter<bool> _DILeftDoorClose = new IOPortDescripter<bool>("DILeftDoorClose", EnumIOType.INPUT);
        public IOPortDescripter<bool> DILeftDoorClose
        {
            get { return _DILeftDoorClose; }
            set
            {
                if (value != this._DILeftDoorClose)
                {
                    _DILeftDoorClose = value;
                    NotifyPropertyChanged("DILeftDoorClose");
                }
            }
        }
        private IOPortDescripter<bool> _DIRightDoorClose = new IOPortDescripter<bool>("DIRightDoorClose", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIRightDoorClose
        {
            get { return _DIRightDoorClose; }
            set
            {
                if (value != this._DIRightDoorClose)
                {
                    _DIRightDoorClose = value;
                    NotifyPropertyChanged("DIRightDoorClose");
                }
            }
        }
        private IOPortDescripter<bool> _DIRightDoorOpen = new IOPortDescripter<bool>("DIRightDoorOpen", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIRightDoorOpen
        {
            get { return _DIRightDoorOpen; }
            set
            {
                if (value != this._DIRightDoorOpen)
                {
                    _DIRightDoorOpen = value;
                    NotifyPropertyChanged("DIRightDoorOpen");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DIBUFVACS;
        public List<IOPortDescripter<bool>> DIBUFVACS
        {
            get { return _DIBUFVACS; }
            set
            {
                if (value != _DIBUFVACS)
                {
                    _DIBUFVACS = value;
                    NotifyPropertyChanged("DIBUFVACS");
                }
            }
        }

        //private IOPortDescripter<bool> _DIBUFVAC1 = new IOPortDescripter<bool>("DIBUFVAC1", EnumIOType.INPUT);
        //public IOPortDescripter<bool> DIBUFVAC1
        //{
        //    get { return _DIBUFVAC1; }
        //    set
        //    {
        //        if (value != this._DIBUFVAC1)
        //        {
        //            _DIBUFVAC1 = value;
        //            NotifyPropertyChanged("DIBUFVAC1");
        //        }
        //    }
        //}
        //private IOPortDescripter<bool> _DIBUFVAC2 = new IOPortDescripter<bool>("DIBUFVAC2", EnumIOType.INPUT);
        //public IOPortDescripter<bool> DIBUFVAC2
        //{
        //    get { return _DIBUFVAC2; }
        //    set
        //    {
        //        if (value != this._DIBUFVAC2)
        //        {
        //            _DIBUFVAC2 = value;
        //            NotifyPropertyChanged("DIBUFVAC2");
        //        }
        //    }
        //}
        //private IOPortDescripter<bool> _DIBUFVAC3 = new IOPortDescripter<bool>("DIBUFVAC3", EnumIOType.INPUT);
        //public IOPortDescripter<bool> DIBUFVAC3
        //{
        //    get { return _DIBUFVAC3; }
        //    set
        //    {
        //        if (value != this._DIBUFVAC3)
        //        {
        //            _DIBUFVAC3 = value;
        //            NotifyPropertyChanged("DIBUFVAC3");
        //        }
        //    }
        //}
        //private IOPortDescripter<bool> _DIBUFVAC4 = new IOPortDescripter<bool>("DIBUFVAC4", EnumIOType.INPUT);
        //public IOPortDescripter<bool> DIBUFVAC4
        //{
        //    get { return _DIBUFVAC4; }
        //    set
        //    {
        //        if (value != this._DIBUFVAC4)
        //        {
        //            _DIBUFVAC4 = value;
        //            NotifyPropertyChanged("DIBUFVAC4");
        //        }
        //    }
        //}
        //private IOPortDescripter<bool> _DIBUFVAC5 = new IOPortDescripter<bool>("DIBUFVAC5", EnumIOType.INPUT);
        //public IOPortDescripter<bool> DIBUFVAC5
        //{
        //    get { return _DIBUFVAC5; }
        //    set
        //    {
        //        if (value != this._DIBUFVAC5)
        //        {
        //            _DIBUFVAC5 = value;
        //            NotifyPropertyChanged("DIBUFVAC5");
        //        }
        //    }
        //}


        private List<IOPortDescripter<bool>> _DIFixTrays;
        public List<IOPortDescripter<bool>> DIFixTrays
        {
            get { return _DIFixTrays; }
            set
            {
                if (value != _DIFixTrays)
                {
                    _DIFixTrays = value;
                    NotifyPropertyChanged("DIFixTrays");
                }
            }
        }

        private List<IOPortDescripter<bool>> _DI6inchWaferOnFixTs;
        public List<IOPortDescripter<bool>> DI6inchWaferOnFixTs
        {
            get { return _DI6inchWaferOnFixTs; }
            set
            {
                if (value != _DI6inchWaferOnFixTs)
                {
                    _DI6inchWaferOnFixTs = value;
                    NotifyPropertyChanged("DI6inchWaferOnFixTs");
                }
            }
        }

        private List<IOPortDescripter<bool>> _DI8inchWaferOnFixTs;
        public List<IOPortDescripter<bool>> DI8inchWaferOnFixTs
        {
            get { return _DI8inchWaferOnFixTs; }
            set
            {
                if (value != _DI8inchWaferOnFixTs)
                {
                    _DI8inchWaferOnFixTs = value;
                    NotifyPropertyChanged("DI8inchWaferOnFixTs");
                }
            }
        }

        private List<IOPortDescripter<bool>> _DICardBuffs;
        public List<IOPortDescripter<bool>> DICardBuffs
        {
            get { return _DICardBuffs; }
            set
            {
                if (value != _DICardBuffs)
                {
                    _DICardBuffs = value;
                    NotifyPropertyChanged("DICardBuffs");
                }
            }
        }

        private List<IOPortDescripter<bool>> _DIMainAirs;
        public List<IOPortDescripter<bool>> DIMainAirs
        {
            get { return _DIMainAirs; }
            set
            {
                if (value != _DIMainAirs)
                {
                    _DIMainAirs = value;
                    NotifyPropertyChanged("DIMainAirs");
                }
            }
        }

        private List<IOPortDescripter<bool>> _DIWaferOnInSPs;
        public List<IOPortDescripter<bool>> DIWaferOnInSPs
        {
            get { return _DIWaferOnInSPs; }
            set
            {
                if (value != _DIWaferOnInSPs)
                {
                    _DIWaferOnInSPs = value;
                    NotifyPropertyChanged("DIWaferOnInSPs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI6inchWaferOnInSPs;
        public List<IOPortDescripter<bool>> DI6inchWaferOnInSPs
        {
            get { return _DI6inchWaferOnInSPs; }
            set
            {
                if (value != _DI6inchWaferOnInSPs)
                {
                    _DI6inchWaferOnInSPs = value;
                    NotifyPropertyChanged("DI6inchWaferOnInSPs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DI8inchWaferOnInSPs;
        public List<IOPortDescripter<bool>> DI8inchWaferOnInSPs
        {
            get { return _DI8inchWaferOnInSPs; }
            set
            {
                if (value != _DI8inchWaferOnInSPs)
                {
                    _DI8inchWaferOnInSPs = value;
                    NotifyPropertyChanged("DI8inchWaferOnInSPs");
                }
            }
        }
        
        private List<IOPortDescripter<bool>> _DIOpenInSPs;
        public List<IOPortDescripter<bool>> DIOpenInSPs
        {
            get { return _DIOpenInSPs; }
            set
            {
                if (value != _DIOpenInSPs)
                {
                    _DIOpenInSPs = value;
                    NotifyPropertyChanged("DIOpenInSPs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DIMovedInSPs;
        public List<IOPortDescripter<bool>> DIMovedInSPs
        {
            get { return _DIMovedInSPs; }
            set
            {
                if (value != _DIMovedInSPs)
                {
                    _DIMovedInSPs = value;
                    NotifyPropertyChanged("DIMovedInSPs");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DICardOnCarrierVacs;
        public List<IOPortDescripter<bool>> DICardOnCarrierVacs
        {
            get { return _DICardOnCarrierVacs; }
            set
            {
                if (value != _DICardOnCarrierVacs)
                {
                    _DICardOnCarrierVacs = value;
                    NotifyPropertyChanged("DICardOnCarrierVacs");
                }
            }
        }
        private IOPortDescripter<bool> _DICARD_EXIST_ARM = new IOPortDescripter<bool>("DICARD_EXIST_ARM", EnumIOType.INPUT);
        public IOPortDescripter<bool> DICARD_EXIST_ARM
        {
            get { return _DICARD_EXIST_ARM; }
            set
            {
                if (value != this._DICARD_EXIST_ARM)
                {
                    _DICARD_EXIST_ARM = value;
                    NotifyPropertyChanged("DICARD_EXIST_ARM");
                }
            }
        }
        private IOPortDescripter<bool> _DILD_PCW_LEAK_STATE = new IOPortDescripter<bool>("DILD_PCW_LEAK_STATE", EnumIOType.INPUT);
        public IOPortDescripter<bool> DILD_PCW_LEAK_STATE
        {
            get { return _DILD_PCW_LEAK_STATE; }
            set
            {
                if (value != this._DILD_PCW_LEAK_STATE)
                {
                    _DILD_PCW_LEAK_STATE = value;
                    NotifyPropertyChanged("DILD_PCW_LEAK_STATE");
                }
            }
        }
        /// <summary>
        /// Cell side PCW Leak sensor state
        /// </summary>
        private List<IOPortDescripter<bool>> _DIPCWLeakStatus;
        public List<IOPortDescripter<bool>> DIPCWLeakStatus
        {
            get { return _DIPCWLeakStatus; }
            set
            {
                if (value != _DIPCWLeakStatus)
                {
                    _DIPCWLeakStatus = value;
                    NotifyPropertyChanged("DIPCWLeakStatus");
                }
            }
        }

        private IOPortDescripter<bool> _DILD_WAFER_OUT_SEONSOR = new IOPortDescripter<bool>("DILD_WAFER_OUT_SEONSOR", EnumIOType.INPUT, EnumIOOverride.EMUL);
        public IOPortDescripter<bool> DILD_WAFER_OUT_SEONSOR
        {
            get { return _DILD_WAFER_OUT_SEONSOR; }
            set
            {
                if (value != this._DILD_WAFER_OUT_SEONSOR)
                {
                    _DILD_WAFER_OUT_SEONSOR = value;
                    NotifyPropertyChanged("DILD_WAFER_OUT_SEONSOR");
                }
            }
        }
        #endregion

        #region // Foup Inputs
        private List<IOPortDescripter<bool>> _DICSTLocks;
        public List<IOPortDescripter<bool>> DICSTLocks
        {
            get { return _DICSTLocks; }
            set
            {
                if (value != _DICSTLocks)
                {
                    _DICSTLocks = value;
                    NotifyPropertyChanged("DICSTLocks");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DICSTUnlocks;
        public List<IOPortDescripter<bool>> DICSTUnlocks
        {
            get { return _DICSTUnlocks; }
            set
            {
                if (value != _DICSTUnlocks)
                {
                    _DICSTUnlocks = value;
                    NotifyPropertyChanged("DICSTUnlocks");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DICSTLoads;
        public List<IOPortDescripter<bool>> DICSTLoads
        {
            get { return _DICSTLoads; }
            set
            {
                if (value != _DICSTLoads)
                {
                    _DICSTLoads = value;
                    NotifyPropertyChanged("DICSTLoads");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DICSTUnLoads;
        public List<IOPortDescripter<bool>> DICSTUnLoads
        {
            get { return _DICSTUnLoads; }
            set
            {
                if (value != _DICSTUnLoads)
                {
                    _DICSTUnLoads = value;
                    NotifyPropertyChanged("DICSTUnLoads");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DICSTCoverCloses;
        public List<IOPortDescripter<bool>> DICSTCoverCloses
        {
            get { return _DICSTCoverCloses; }
            set
            {
                if (value != _DICSTCoverCloses)
                {
                    _DICSTCoverCloses = value;
                    NotifyPropertyChanged("DICSTCoverCloses");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DICSTCoverOpens;
        public List<IOPortDescripter<bool>> DICSTCoverOpens
        {
            get { return _DICSTCoverOpens; }
            set
            {
                if (value != _DICSTCoverOpens)
                {
                    _DICSTCoverOpens = value;
                    NotifyPropertyChanged("DICSTCoverOpens");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DIMappings;
        public List<IOPortDescripter<bool>> DIMappings
        {
            get { return _DIMappings; }
            set
            {
                if (value != _DIMappings)
                {
                    _DIMappings = value;
                    NotifyPropertyChanged("DIMappings");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DIWaferOut;
        public List<IOPortDescripter<bool>> DIWaferOut
        {
            get { return _DIWaferOut; }
            set
            {
                if (value != _DIWaferOut)
                {
                    _DIWaferOut = value;
                    NotifyPropertyChanged("DIWaferOut");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DICoverVac;
        public List<IOPortDescripter<bool>> DICoverVac
        {
            get { return _DICoverVac; }
            set
            {
                if (value != _DICoverVac)
                {
                    _DICoverVac = value;
                    NotifyPropertyChanged("DICoverVac");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DICST12Locks;
        public List<IOPortDescripter<bool>> DICST12Locks
        {
            get { return _DICST12Locks; }
            set
            {
                if (value != _DICST12Locks)
                {
                    _DICST12Locks = value;
                    NotifyPropertyChanged("DICST12Locks");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DICST12UnLocks;
        public List<IOPortDescripter<bool>> DICST12UnLocks
        {
            get { return _DICST12UnLocks; }
            set
            {
                if (value != _DICST12UnLocks)
                {
                    _DICST12UnLocks = value;
                    NotifyPropertyChanged("DICST12UnLocks");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DICST8Locks;
        public List<IOPortDescripter<bool>> DICST8Locks
        {
            get { return _DICST8Locks; }
            set
            {
                if (value != _DICST8Locks)
                {
                    _DICST8Locks = value;
                    NotifyPropertyChanged("DICST8Locks");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DICST8Unlocks;
        public List<IOPortDescripter<bool>> DICST8Unlocks
        {
            get { return _DICST8Unlocks; }
            set
            {
                if (value != _DICST8Unlocks)
                {
                    _DICST8Unlocks = value;
                    NotifyPropertyChanged("DICST8Unlocks");
                }
            }
        }


        private IOPortDescripter<bool> _DIWAFERONDRAWER = new IOPortDescripter<bool>("DIWAFERONDRAWER", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIWAFERONDRAWER
        {
            get { return _DIWAFERONDRAWER; }
            set
            {
                if (value != this._DIWAFERONDRAWER)
                {
                    _DIWAFERONDRAWER = value;
                    NotifyPropertyChanged("DIWAFERONDRAWER");
                }
            }
        }

        private IOPortDescripter<bool> _DIDRAWEROPEN = new IOPortDescripter<bool>("DIDRAWEROPEN", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIDRAWEROPEN
        {
            get { return _DIDRAWEROPEN; }
            set
            {
                if (value != this._DIDRAWEROPEN)
                {
                    _DIDRAWEROPEN = value;
                    NotifyPropertyChanged("DIDRAWEROPEN");
                }
            }
        }

        private IOPortDescripter<bool> _DIDRAWEREMOVED = new IOPortDescripter<bool>("DIDRAWEREMOVED", EnumIOType.INPUT);
        public IOPortDescripter<bool> DIDRAWEREMOVED
        {
            get { return _DIDRAWEREMOVED; }
            set
            {
                if (value != this._DIDRAWEREMOVED)
                {
                    _DIDRAWEREMOVED = value;
                    NotifyPropertyChanged("DIDRAWEREMOVED");
                }
            }
        }

        #endregion
    }
    [Serializable]
    public class RemoteOutputPortDefinitions : INotifyPropertyChanged, IParamNode
    {
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

        public RemoteOutputPortDefinitions()
        {

        }
        #region Loader Outputs
        private IOPortDescripter<bool> _DOARM1Vac = new IOPortDescripter<bool>("DOARM1Vac", EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOARM1Vac
        {
            get { return _DOARM1Vac; }
            set
            {
                if (value != this._DOARM1Vac)
                {
                    _DOARM1Vac = value;
                    NotifyPropertyChanged("DOARM1Vac");
                }
            }
        }
        private IOPortDescripter<bool> _DOARMVac2 = new IOPortDescripter<bool>("DOARMVac2", EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOARMVac2
        {
            get { return _DOARMVac2; }
            set
            {
                if (value != this._DOARMVac2)
                {
                    _DOARMVac2 = value;
                    NotifyPropertyChanged("DOARMVac2");
                }
            }
        }
        private IOPortDescripter<bool> _DOCCArmVac = new IOPortDescripter<bool>("DOCCArmVac", EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOCCArmVac
        {
            get { return _DOCCArmVac; }
            set
            {
                if (value != this._DOCCArmVac)
                {
                    _DOCCArmVac = value;
                    NotifyPropertyChanged("DOCCArmVac");
                }
            }
        }

        private IOPortDescripter<bool> _DOCCArmVac_Break = new IOPortDescripter<bool>("DOCCArmVac_Break", EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOCCArmVac_Break
        {
            get { return _DOCCArmVac_Break; }
            set
            {
                if (value != this._DOCCArmVac_Break)
                {
                    _DOCCArmVac_Break = value;
                    NotifyPropertyChanged("DOCCArmVac_Break");
                }
            }
        }


        private List<IOPortDescripter<bool>> _DOBuffVacs;
        public List<IOPortDescripter<bool>> DOBuffVacs
        {
            get { return _DOBuffVacs; }
            set
            {
                if (value != _DOBuffVacs)
                {
                    _DOBuffVacs = value;
                    NotifyPropertyChanged("DOBuffVacs");
                }
            }
        }
        #endregion
        #region // Foups
        private List<IOPortDescripter<bool>> _DOCSTCoverOpens;
        public List<IOPortDescripter<bool>> DOCSTCoverOpens
        {
            get { return _DOCSTCoverOpens; }
            set
            {
                if (value != _DOCSTCoverOpens)
                {
                    _DOCSTCoverOpens = value;
                    NotifyPropertyChanged("DOCSTCoverOpens");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DOCSTCoverCloses;
        public List<IOPortDescripter<bool>> DOCSTCoverCloses
        {
            get { return _DOCSTCoverCloses; }
            set
            {
                if (value != _DOCSTCoverCloses)
                {
                    _DOCSTCoverCloses = value;
                    NotifyPropertyChanged("DOCSTCoverCloses");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DOCSTCoverLocks;
        public List<IOPortDescripter<bool>> DOCSTCoverLocks
        {
            get { return _DOCSTCoverLocks; }
            set
            {
                if (value != _DOCSTCoverLocks)
                {
                    _DOCSTCoverLocks = value;
                    NotifyPropertyChanged("DOCSTCoverLocks");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DOCSTCoverUnlock;
        public List<IOPortDescripter<bool>> DOCSTCoverUnlock
        {
            get { return _DOCSTCoverUnlock; }
            set
            {
                if (value != _DOCSTCoverUnlock)
                {
                    _DOCSTCoverUnlock = value;
                    NotifyPropertyChanged("DOCSTCoverUnlock");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DOCSTLoadLamps;
        public List<IOPortDescripter<bool>> DOCSTLoadLamps
        {
            get { return _DOCSTLoadLamps; }
            set
            {
                if (value != _DOCSTLoadLamps)
                {
                    _DOCSTLoadLamps = value;
                    NotifyPropertyChanged("DOCSTLoadLamps");
                }
            }
        }
        private List<IOPortDescripter<bool>> _DOCSTUnloadLamps;
        public List<IOPortDescripter<bool>> DOCSTUnloadLamps
        {
            get { return _DOCSTUnloadLamps; }
            set
            {
                if (value != _DOCSTUnloadLamps)
                {
                    _DOCSTUnloadLamps = value;
                    NotifyPropertyChanged("DOCSTUnloadLamps");
                }
            }
        }
        #endregion
        #region // Lamps
        private IOPortDescripter<bool> _DOFrontSigRed = new IOPortDescripter<bool>("DOFrontSigRed", EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOFrontSigRed
        {
            get { return _DOFrontSigRed; }
            set
            {
                if (value != this._DOFrontSigRed)
                {
                    _DOFrontSigRed = value;
                    NotifyPropertyChanged("DOFrontSigRed");
                }
            }
        }
        private IOPortDescripter<bool> _DOFrontSigYl = new IOPortDescripter<bool>("DOFrontSigYl", EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOFrontSigYl
        {
            get { return _DOFrontSigYl; }
            set
            {
                if (value != this._DOFrontSigYl)
                {
                    _DOFrontSigYl = value;
                    NotifyPropertyChanged("DOFrontSigYl");
                }
            }
        }
        private IOPortDescripter<bool> _DOFrontSigGrn = new IOPortDescripter<bool>("DOFrontSigGrn", EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOFrontSigGrn
        {
            get { return _DOFrontSigGrn; }
            set
            {
                if (value != this._DOFrontSigGrn)
                {
                    _DOFrontSigGrn = value;
                    NotifyPropertyChanged("DOFrontSigGrn");
                }
            }
        }
        private IOPortDescripter<bool> _DOFrontSigBuz = new IOPortDescripter<bool>("DOFrontSigBuz", EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DOFrontSigBuz
        {
            get { return _DOFrontSigBuz; }
            set
            {
                if (value != this._DOFrontSigBuz)
                {
                    _DOFrontSigBuz = value;
                    NotifyPropertyChanged("DOFrontSigBuz");
                }
            }
        }
        private IOPortDescripter<bool> _DORearSigRed = new IOPortDescripter<bool>("DORearSigRed", EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DORearSigRed
        {
            get { return _DORearSigRed; }
            set
            {
                if (value != this._DORearSigRed)
                {
                    _DORearSigRed = value;
                    NotifyPropertyChanged("DORearSigRed");
                }
            }
        }
        private IOPortDescripter<bool> _DORearSigYl = new IOPortDescripter<bool>("DORearSigYl", EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DORearSigYl
        {
            get { return _DORearSigYl; }
            set
            {
                if (value != this._DORearSigYl)
                {
                    _DORearSigYl = value;
                    NotifyPropertyChanged("DORearSigYl");
                }
            }
        }
        private IOPortDescripter<bool> _DORearSigGrn = new IOPortDescripter<bool>("DORearSigGrn", EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DORearSigGrn
        {
            get { return _DORearSigGrn; }
            set
            {
                if (value != this._DORearSigGrn)
                {
                    _DORearSigGrn = value;
                    NotifyPropertyChanged("DORearSigGrn");
                }
            }
        }
        private IOPortDescripter<bool> _DORearSigBuz = new IOPortDescripter<bool>("DORearSigBuz", EnumIOType.OUTPUT);
        public IOPortDescripter<bool> DORearSigBuz
        {
            get { return _DORearSigBuz; }
            set
            {
                if (value != this._DORearSigBuz)
                {
                    _DORearSigBuz = value;
                    NotifyPropertyChanged("DORearSigBuz");
                }
            }
        }
        #endregion
    }
    [Serializable]
    public class InputDescripters
    {
        public InputDescripters()
        {
            MultipleInput = new List<IOPortDescripter<bool>>();
        }

        private string _InputDescripter;

        public string InputDescripter
        {
            get { return _InputDescripter; }
            set { _InputDescripter = value; }
        }
        private List<IOPortDescripter<bool>> _MultipleInput;


        public List<IOPortDescripter<bool>> MultipleInput
        {
            get { return _MultipleInput; }
            set { _MultipleInput = value; }
        }

    }

    [Serializable]
    public class CylinderIOParameter
    {
        public IOPortDescripter<bool> Extend_Output;
        public IOPortDescripter<bool> Retract_OutPut;
        public InputDescripters Extend_Input;
        public InputDescripters Retract_Input;

        private InputDescripters _InterlockInputs = new InputDescripters();
        public InputDescripters InterlockInputs
        {
            get { return _InterlockInputs; }
            set
            {
                if (value != _InterlockInputs)
                {
                    _InterlockInputs = value;
                }
            }
        }

        public CylinderIOParameter()
        {
            try
            {
                Extend_Input = new InputDescripters();
                Retract_Input = new InputDescripters();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    [Serializable]
    public class CylinderMappingParameter
    {
        private string _CylinderName;
        public string CylinderName
        {
            get { return _CylinderName; }
            set
            {
                if (value != _CylinderName)
                {
                    _CylinderName = value;
                }
            }
        }

        private string _Extend_Output_Key;
        public string Extend_Output_Key
        {
            get { return _Extend_Output_Key; }
            set
            {
                if (value != _Extend_Output_Key)
                {
                    _Extend_Output_Key = value;
                }
            }
        }

        private string _Retract_OutPut_Key;
        public string Retract_OutPut_Key
        {
            get { return _Retract_OutPut_Key; }
            set
            {
                if (value != _Retract_OutPut_Key)
                {
                    _Retract_OutPut_Key = value;
                }
            }
        }

        private List<string> _Extend_Input_key_list;
        public List<string> Extend_Input_key_list
        {
            get { return _Extend_Input_key_list; }
            set
            {
                if (value != _Extend_Input_key_list)
                {
                    _Extend_Input_key_list = value;
                }
            }
        }

        private List<string> _Retract_Input_key_list;
        public List<string> Retract_Input_key_list
        {
            get { return _Retract_Input_key_list; }
            set
            {
                if (value != _Retract_Input_key_list)
                {
                    _Retract_Input_key_list = value;
                }
            }
        }
        private List<string> _Interlock_Input_key_list;
        public List<string> Interlock_Input_key_list
        {
            get { return _Interlock_Input_key_list; }
            set
            {
                if (value != _Interlock_Input_key_list)
                {
                    _Interlock_Input_key_list = value;
                }
            }
        }
        public string ParamLabel { get; set; }
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

    [Serializable]
    public class CylinderParams : IParam
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<CylinderMappingParameter> CylinderMappingParameterList;

        public CylinderParams()
        {
            CylinderMappingParameterList = new List<CylinderMappingParameter>();
        }

        public string ParamLabel { get; set; }

        public string FilePath { get; set; } = "Cylinder";
        public string FileName { get; set; }

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

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }
    }
}
