using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using ProberErrorCode;

using LogModule;
using Newtonsoft.Json;

namespace Configurator
{
    [Serializable]
    public class DeviceConfigurator : INotifyPropertyChanged, IParamNode, IParam, ISystemParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            try
            {
                DeviceConfiguratorParams.SetDefaultParamEmul();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            try
            {
                RetVal = DeviceConfiguratorParams.SetDefaultParam();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return EventCodeEnum.NONE;
        }

        public DeviceConfigurator()
        {
            DeviceConfiguratorParams = new DeviceConfiguratorParam();
        }

        private DeviceConfiguratorParam _DeviceConfiguratorParams;
        public DeviceConfiguratorParam DeviceConfiguratorParams
        {
            get { return _DeviceConfiguratorParams; }
            set { _DeviceConfiguratorParams = value; NotifyPropertyChanged(nameof(DeviceConfiguratorParams)); }
        }

        public string FilePath { get; } = "";
        public string FileName { get; } = "DeviceMapping.Json";

        [Serializable]
        public class DeviceConfiguratorParam : INotifyPropertyChanged, IParamNode
        {


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

            private ObservableCollection<DeviceDescripter> _DevDescripters;
            public ObservableCollection<DeviceDescripter> DevDescripters
            {
                get { return _DevDescripters; }
                set { _DevDescripters = value; }
            }
            private ObservableCollection<ChannelDescripter> _InputDescripter;
            public ObservableCollection<ChannelDescripter> InputDescripter
            {
                get { return _InputDescripter; }
                set { _InputDescripter = value; }
            }
            private ObservableCollection<ChannelDescripter> _OutputDescripter;
            public ObservableCollection<ChannelDescripter> OutputDescripter
            {
                get { return _OutputDescripter; }
                set { _OutputDescripter = value; }
            }
            public DeviceConfiguratorParam()
            {
                try
                {
                    DevDescripters = new ObservableCollection<DeviceDescripter>();
                    InputDescripter = new ObservableCollection<ChannelDescripter>();
                    OutputDescripter = new ObservableCollection<ChannelDescripter>();

                    // InputDescripter.Add(new ChannelDescripter(0, 0,0, 8));
                    //InputDescripter.Add(new ChannelDescripter(0, 1, 1, 8));
                    //InputDescripter.Add(new ChannelDescripter(0, 2, 2, 8));


                    //OutputDescripter.Add(new ChannelDescripter(0, 0, 3, 8));
                    //OutputDescripter.Add(new ChannelDescripter(0, 1, 4, 8));
                    //OutputDescripter.Add(new ChannelDescripter(0, 2, 5, 8));


                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }

            public EventCodeEnum SetDefaultParam()
            {
                EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
                try
                {

                    //SetDefaultParamEmul();
                    //SetDefaultParamBSCI1();
                    //SetDefaultParamOPUSV_Machine3();

                    // 251030 sebas
                    //SetDefaultParamOPUSV_Test();
                    SetDefaultParamBonder_Test();   // test add
                    RetVal = EventCodeEnum.NONE;

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
                return RetVal;
            }
            #region SetDefaultParam for Machines
            //private void SetDefaultParamOPUSVStage()
            //{
            //    try
            //    {
            //        DevDescripters.Add(new DeviceDescripter(DeviceType.EMulIO, 0));
            //        DevDescripters.Add(new DeviceDescripter(DeviceType.ECATIO, 1));
            //        InputDescripter.Add(new ChannelDescripter(0, 0, 8, EnumIOType.INPUT));
            //        OutputDescripter.Add(new ChannelDescripter(0, 1, 8, EnumIOType.OUTPUT));
            //        InputDescripter.Add(new ChannelDescripter(1, 0, 16, EnumIOType.INPUT));
            //        OutputDescripter.Add(new ChannelDescripter(1, 2, 16, EnumIOType.OUTPUT));
            //        OutputDescripter.Add(new ChannelDescripter(1, 3, 8, EnumIOType.OUTPUT));
            //        InputDescripter.Add(new ChannelDescripter(1, 1, 6, EnumIOType.INPUT));
            //    }
            //    catch (Exception err)
            //    {
            //        LoggerManager.Exception(err);
            //        throw;
            //    }
            //}

            //private void SetDefaultParamBSCI1()
            //{
            //    try
            //    {
            //        DevDescripters = new ObservableCollection<DeviceDescripter>();
            //        InputDescripter = new ObservableCollection<ChannelDescripter>();
            //        OutputDescripter = new ObservableCollection<ChannelDescripter>();
            //        DevDescripters.Add(new DeviceDescripter(DeviceType.ECATIO, 0));

            //        //stage
            //        InputDescripter.Add(new ChannelDescripter(0, 0, 16, EnumIOType.INPUT)); //g17 EL1862
            //        InputDescripter.Add(new ChannelDescripter(0, 1, 16, EnumIOType.INPUT)); //g18 EL1862
            //        InputDescripter.Add(new ChannelDescripter(0, 2, 16, EnumIOType.INPUT)); // g19 EL1862
            //                                                                                //loader
            //        InputDescripter.Add(new ChannelDescripter(0, 3, 16, EnumIOType.INPUT));  //g11 1862
            //        InputDescripter.Add(new ChannelDescripter(0, 4, 16, EnumIOType.INPUT));   //g12 1862

            //        //StageAxis
            //        InputDescripter.Add(new ChannelDescripter(0, 5, 32, EnumIOType.INPUT));      //XAxis
            //        InputDescripter.Add(new ChannelDescripter(0, 6, 32, EnumIOType.INPUT));     //TAxis



            //        //stage
            //        OutputDescripter.Add(new ChannelDescripter(0, 7, 8, EnumIOType.AO));     //g14 EL4008
            //        OutputDescripter.Add(new ChannelDescripter(0, 8, 16, EnumIOType.OUTPUT));  //g15  EL2872
            //        OutputDescripter.Add(new ChannelDescripter(0, 9, 16, EnumIOType.OUTPUT));  //g16   EL2872
            //                                                                                   //Loader
            //        OutputDescripter.Add(new ChannelDescripter(0, 10, 4, EnumIOType.AO));     //g08  el4004
            //        OutputDescripter.Add(new ChannelDescripter(0, 11, 4, EnumIOType.OUTPUT));  //g09  el2024
            //        OutputDescripter.Add(new ChannelDescripter(0, 12, 16, EnumIOType.OUTPUT));  //g10  el2872

            //    }
            //    catch (Exception err)
            //    {
            //        LoggerManager.Exception(err);
            //        throw;
            //    }
            //}

            //private void SetDefaultParamOPUSV_Machine3()
            //{
            //    try
            //    {
            //        DevDescripters = new ObservableCollection<DeviceDescripter>();
            //        InputDescripter = new ObservableCollection<ChannelDescripter>();
            //        OutputDescripter = new ObservableCollection<ChannelDescripter>();
            //        DevDescripters.Add(new DeviceDescripter(DeviceType.ECATIO, 0));

            //        //stage
            //        InputDescripter.Add(new ChannelDescripter(0, 0, 16, EnumIOType.INPUT)); //g17 EL1862
            //        InputDescripter.Add(new ChannelDescripter(0, 1, 16, EnumIOType.INPUT)); //g18 EL1862
            //        InputDescripter.Add(new ChannelDescripter(0, 2, 16, EnumIOType.INPUT)); // g19 EL1862
            //                                                                                //loader
            //        InputDescripter.Add(new ChannelDescripter(0, 3, 16, EnumIOType.INPUT));  //g11 1862
            //        InputDescripter.Add(new ChannelDescripter(0, 4, 16, EnumIOType.INPUT));   //g12 1862

            //        //StageAxis
            //        InputDescripter.Add(new ChannelDescripter(0, 5, 32, EnumIOType.INPUT));      //XAxis
            //        InputDescripter.Add(new ChannelDescripter(0, 6, 32, EnumIOType.INPUT));     //TAxis



            //        //stage
            //        OutputDescripter.Add(new ChannelDescripter(0, 7, 8, EnumIOType.AO));     //g14 EL4008
            //        OutputDescripter.Add(new ChannelDescripter(0, 8, 16, EnumIOType.OUTPUT));  //g15  EL2872
            //        OutputDescripter.Add(new ChannelDescripter(0, 9, 16, EnumIOType.OUTPUT));  //g16   EL2872
            //                                                                                   //Loader
            //        OutputDescripter.Add(new ChannelDescripter(0, 10, 4, EnumIOType.AO));     //g08  el4004
            //        OutputDescripter.Add(new ChannelDescripter(0, 11, 4, EnumIOType.OUTPUT));  //g09  el2024
            //        OutputDescripter.Add(new ChannelDescripter(0, 12, 16, EnumIOType.OUTPUT));  //g10  el2872

            //    }
            //    catch (Exception err)
            //    {
            //        LoggerManager.Exception(err);
            //        throw;
            //    }
            //}

            //private void SetDefaultParamOPUSVLoader()
            //{
            //    try
            //    {
            //        DevDescripters = new ObservableCollection<DeviceDescripter>();
            //        InputDescripter = new ObservableCollection<ChannelDescripter>();
            //        OutputDescripter = new ObservableCollection<ChannelDescripter>();

            //        DevDescripters.Add(new DeviceDescripter(DeviceType.ECATIO, 0));
            //        InputDescripter.Add(new ChannelDescripter(0, 0, 16, EnumIOType.INPUT));
            //        InputDescripter.Add(new ChannelDescripter(0, 1, 16, EnumIOType.INPUT));

            //        OutputDescripter.Add(new ChannelDescripter(0, 2, 4, EnumIOType.AO));
            //        OutputDescripter.Add(new ChannelDescripter(0, 3, 4, EnumIOType.OUTPUT));
            //        OutputDescripter.Add(new ChannelDescripter(0, 4, 16, EnumIOType.OUTPUT));

            //    }
            //    catch (Exception err)
            //    {
            //        LoggerManager.Exception(err);
            //        throw;
            //    }
            //}
            #endregion

            private void SetDefaultParamOPUSV_Test()
            {
                try
                {
                    DevDescripters = new ObservableCollection<DeviceDescripter>();
                    InputDescripter = new ObservableCollection<ChannelDescripter>();
                    OutputDescripter = new ObservableCollection<ChannelDescripter>();
                    DevDescripters.Add(new DeviceDescripter(DeviceType.ECATIO, 0));

                    //stage
                    InputDescripter.Add(new ChannelDescripter(0, 0, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(0, 1, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(0, 2, 16, EnumIOType.INPUT));
                    //loader
                    InputDescripter.Add(new ChannelDescripter(0, 3, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(0, 4, 16, EnumIOType.INPUT));
                    //StageAxis
                    InputDescripter.Add(new ChannelDescripter(0, 5, 32, EnumIOType.INPUT));


                    //stage
                    OutputDescripter.Add(new ChannelDescripter(0, 6, 8, EnumIOType.AO));
                    OutputDescripter.Add(new ChannelDescripter(0, 7, 16, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(0, 8, 16, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(0, 9, 16, EnumIOType.OUTPUT));
                    //Loader
                    OutputDescripter.Add(new ChannelDescripter(0, 10, 4, EnumIOType.AO));
                    OutputDescripter.Add(new ChannelDescripter(0, 11, 4, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(0, 12, 16, EnumIOType.OUTPUT));
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }

            private void SetDefaultParamBonder_Test()
            {
                try
                {
                    DevDescripters = new ObservableCollection<DeviceDescripter>();
                    InputDescripter = new ObservableCollection<ChannelDescripter>();
                    OutputDescripter = new ObservableCollection<ChannelDescripter>();

                    DevDescripters.Add(new DeviceDescripter(DeviceType.ECATIO, 0));

                    // input
                    InputDescripter.Add(new ChannelDescripter(0, 0, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(0, 1, 16, EnumIOType.INPUT));

                    // output
                    OutputDescripter.Add(new ChannelDescripter(0, 2, 8, EnumIOType.AO));
                    OutputDescripter.Add(new ChannelDescripter(0, 3, 8, EnumIOType.AO));
                    OutputDescripter.Add(new ChannelDescripter(0, 4, 4, EnumIOType.AO));

                    OutputDescripter.Add(new ChannelDescripter(0, 5, 16, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(0, 6, 16, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(0, 7, 16, EnumIOType.OUTPUT));
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }

            public void SetDefaultParamEmul()
            {
                try
                {
                    DevDescripters.Add(new DeviceDescripter(DeviceType.EMulIO, 0));
                    DevDescripters.Add(new DeviceDescripter(DeviceType.EMulIO, 1));
                    DevDescripters.Add(new DeviceDescripter(DeviceType.EMulIO, 2));
                    DevDescripters.Add(new DeviceDescripter(DeviceType.EMulIO, 3));
                    InputDescripter.Add(new ChannelDescripter(1, 0, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(1, 1, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(1, 2, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(2, 0, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(2, 1, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(2, 2, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(2, 3, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(2, 4, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(2, 5, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(3, 0, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(3, 1, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(3, 2, 16, EnumIOType.INPUT));
                    InputDescripter.Add(new ChannelDescripter(3, 3, 16, EnumIOType.INPUT));



                    OutputDescripter.Add(new ChannelDescripter(0, 0, 16, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(0, 1, 16, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(0, 2, 16, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(0, 3, 16, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(0, 4, 16, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(0, 5, 16, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(1, 3, 16, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(1, 4, 16, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(1, 5, 16, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(3, 4, 16, EnumIOType.OUTPUT));
                    OutputDescripter.Add(new ChannelDescripter(3, 5, 16, EnumIOType.OUTPUT));
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }

        }
    }


    public enum DeviceType
    {
        ENetIO,     /* Deprecated */
        USBNetIO,
        EMulIO,
        ECATIO,
        UNDEFINED
    }

    [Serializable]
    public class DeviceDescripter : INotifyPropertyChanged, IParamNode
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

        private Element<DeviceType> _DeviceType = new Element<DeviceType>();
        [XmlAttribute("Type")]
        public Element<DeviceType> DeviceType
        {
            get { return _DeviceType; }
            set
            {
                if (value != this.DeviceType)
                {
                    _DeviceType = value;
                    NotifyPropertyChanged("DeviceType");
                }
            }
        }
        private Element<int> _DeviceNum = new Element<int>();
        [XmlAttribute("DevNum")]
        public Element<int> DeviceNum
        {
            get { return _DeviceNum; }
            set
            {
                if (value != this._DeviceNum)
                {
                    _DeviceNum = value;
                    NotifyPropertyChanged("DeviceNum");
                }
            }
        }




        public DeviceDescripter()
        {
            try
            {
                _DeviceType.Value = Configurator.DeviceType.UNDEFINED;
                DeviceNum.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public DeviceDescripter(DeviceType type, int devNum)
        {
            try
            {
                _DeviceType.Value = type;
                _DeviceNum.Value = devNum;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    [Serializable]
    public class ChannelDescripter : INotifyPropertyChanged, IParamNode
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
        private Element<int> _Dev = new Element<int>();
        [XmlAttribute("Dev")]
        public Element<int> Dev
        {
            get { return _Dev; }
            set { _Dev = value; }
        }
        private Element<int> _HWChannel = new Element<int>();
        [XmlAttribute("HWChannel")]
        public Element<int> HWChannel
        {
            get { return _HWChannel; }
            set { _HWChannel = value; }
        }
        private Element<int> _Port = new Element<int>();
        [XmlAttribute("Port")]
        public Element<int> Port
        {
            get { return _Port; }
            set { _Port = value; }
        }
        private Element<EnumIOType> _IOType = new Element<EnumIOType>();
        [XmlAttribute("IOType")]
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

        private Element<int> _NodeIndex = new Element<int>(-1);
        [XmlAttribute("NodeIndex")]
        public Element<int> NodeIndex
        {
            get { return _NodeIndex; }
            set { _NodeIndex = value; }
        }

        private Element<int> _VarOffset = new Element<int>(-1);
        [XmlAttribute("VarOffset")]
        public Element<int> VarOffset
        {
            get { return _VarOffset; }
            set { _VarOffset = value; }
        }
        public ChannelDescripter()
        {
            try
            {
                _Dev.Value = 0;
                _HWChannel.Value = 0;
                _Port.Value = 0;
                _NodeIndex.Value = 0;
                _VarOffset.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ChannelDescripter(int dev, int hwChannel, int port)
        {
            try
            {
                _Dev.Value = dev;

                _HWChannel.Value = hwChannel;
                _Port.Value = port;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ChannelDescripter(int dev, int hwChannel, int port, EnumIOType iotype, int nodeIndex = -1, int varOffset = -1)
        {
            try
            {
                _Dev.Value = dev;

                _HWChannel.Value = hwChannel;
                _Port.Value = port;
                _IOType.Value = iotype;

                NodeIndex.Value = nodeIndex;
                VarOffset.Value = varOffset;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

    }

    [Serializable]
    public class ElmoDescripterParam : INotifyPropertyChanged, ISystemParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }

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

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public string FilePath { get; } = "Elmo";
        public string FileName { get; } = "ElmoSetting.Json";

        private String _PIP;
        public String PIP
        {
            get { return _PIP; }
            set { _PIP = value; }
        }
        private String _LIP;
        public String LIP
        {
            get { return _LIP; }
            set { _LIP = value; }
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                PIP = "192.168.99.3";
                LIP = "192.168.99.2";

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
    }


    [Serializable]
    public class ECATIODescripter : INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public ECATIODescripterParam ECATIODescripterParams { get; set; }

        public class ECATIODescripterParam : INotifyPropertyChanged, ISystemParameterizable
        {
            [JsonIgnore, ParamIgnore]
            public bool IsParamChanged { get; set; }
            public List<object> Nodes { get; set; }

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

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged(String info)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                }
            }
            public string FilePath { get; } = "Elmo";
            public string FileName { get; } = "ECATIODescripter.json";
            public void SetElementMetaData()
            {

            }
            public EventCodeEnum SetEmulParam()
            {
                return SetDefaultParam();
            }
            public EventCodeEnum SetDefaultParam()
            {
                EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
                try
                {

                    //SetDefaultOPUSV_Machine3();
                    //SetDefaultBSCI1();
                    //SetDefaultOPUSV_Test();
                    SetDefaultSorter_LAB();

                    RetVal = EventCodeEnum.NONE;

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
                return RetVal;
            }
            private void SetDefaultOPUSVStage()
            {
                try
                {
                    ECATNodeDefinitions = new ObservableCollection<ECATNodeDefinition>();
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("g11", EnumIOType.INPUT, EnumIONodeType.IO, 16));
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a03", EnumIOType.INPUT, EnumIONodeType.Axis, 6));
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("g07", EnumIOType.OUTPUT, EnumIONodeType.IO, 16));
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("g06", EnumIOType.AO, EnumIONodeType.IO, 8));

                    SetDefaultOPUSV_Test();

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }

            private void SetDefaultBSCI1()
            {
                try
                {

                    ECATNodeDefinitions = new ObservableCollection<ECATNodeDefinition>();
                    //stage

                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g17", EnumIOType.INPUT, EnumIONodeType.IO, 16));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g18", EnumIOType.INPUT, EnumIONodeType.IO, 16));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g19", EnumIOType.INPUT, EnumIONodeType.IO, 16));
                    //loader
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g11", EnumIOType.INPUT, EnumIONodeType.IO, 16));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g12", EnumIOType.INPUT, EnumIONodeType.IO, 16));

                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a20", EnumIOType.INPUT, EnumIONodeType.Axis, 32));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a22", EnumIOType.INPUT, EnumIONodeType.Axis, 32));



                    //stage
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g14", EnumIOType.AO, EnumIONodeType.IO, 8));

                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g15", EnumIOType.OUTPUT, EnumIONodeType.IO, 16));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g16", EnumIOType.OUTPUT, EnumIONodeType.IO, 16));
                    //Loader
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g08", EnumIOType.AO, EnumIONodeType.IO, 4));

                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g09", EnumIOType.OUTPUT, EnumIONodeType.IO, 4));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g10", EnumIOType.OUTPUT, EnumIONodeType.IO, 16));







                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }
            ObservableCollection<ECATNodeDefinition> _ECATNodeDefinitions;
            public ObservableCollection<ECATNodeDefinition> ECATNodeDefinitions
            {
                get { return _ECATNodeDefinitions; }
                set { _ECATNodeDefinitions = value; }
            }

            private void SetDefaultSorter_LAB()
            {
                try
                {
                    ECATNodeDefinitions = new ObservableCollection<ECATNodeDefinition>();
                    // stage
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g01", EnumIOType.INPUT, EnumIONodeType.IO, 32));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g01", EnumIOType.OUTPUT, EnumIONodeType.IO, 32));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g01", EnumIOType.AI, EnumIONodeType.IO, 2));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g01", EnumIOType.AO, EnumIONodeType.IO, 1));
                    // Stage Axis IO Defs.

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }
            public void SetDeafultOPUSVLoader()
            {
                try
                {
                    ECATNodeDefinitions = new ObservableCollection<ECATNodeDefinition>();


                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g05", EnumIOType.INPUT, EnumIONodeType.IO, 16)); // 1862
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g06", EnumIOType.INPUT, EnumIONodeType.IO, 16)); //1862


                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g02", EnumIOType.AO, EnumIONodeType.IO, 4));    //EL4004
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g03", EnumIOType.OUTPUT, EnumIONodeType.IO, 4)); //EL2024
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g04", EnumIOType.OUTPUT, EnumIONodeType.IO, 16)); // EL2872
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }
            private void SetDefaultOPUSV_Test()
            {
                try
                {
                    ECATNodeDefinitions = new ObservableCollection<ECATNodeDefinition>();
                    //stage
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g22", EnumIOType.INPUT, EnumIONodeType.IO, 16)); // 1862
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g23", EnumIOType.INPUT, EnumIONodeType.IO, 16)); // 1862
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g24", EnumIOType.INPUT, EnumIONodeType.IO, 16)); // 1862
                                                                                                                     //Loader
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g09", EnumIOType.INPUT, EnumIONodeType.IO, 16)); // 1862
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g10", EnumIOType.INPUT, EnumIONodeType.IO, 16)); // 1862

                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a25", EnumIOType.INPUT, EnumIONodeType.Axis, 32)); // EL2872


                    //stage
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g18", EnumIOType.AO, EnumIONodeType.IO, 8));    //EL4008
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g19", EnumIOType.OUTPUT, EnumIONodeType.IO, 16)); //EL2872
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g20", EnumIOType.OUTPUT, EnumIONodeType.IO, 16)); // EL2872
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g21", EnumIOType.OUTPUT, EnumIONodeType.IO, 16)); // EL2872
                                                                                                                      //Loader
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g06", EnumIOType.AO, EnumIONodeType.IO, 4));    //EL4004
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g07", EnumIOType.OUTPUT, EnumIONodeType.IO, 4)); //EL2024
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g08", EnumIOType.OUTPUT, EnumIONodeType.IO, 16)); // EL2872

                    // Stage Axis IO Defs.

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }
            private void SetDefaultOPUSV_Machine3()
            {
                try
                {
                    ECATNodeDefinitions = new ObservableCollection<ECATNodeDefinition>();
                    //stage
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g21", EnumIOType.INPUT, EnumIONodeType.IO, 16)); // 1862
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g22", EnumIOType.INPUT, EnumIONodeType.IO, 16)); // 1862
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g23", EnumIOType.INPUT, EnumIONodeType.IO, 16)); // 1862
                                                                                                                     //Loader
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g09", EnumIOType.INPUT, EnumIONodeType.IO, 16)); // 1862
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g10", EnumIOType.INPUT, EnumIONodeType.IO, 16)); // 1862

                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a24", EnumIOType.INPUT, EnumIONodeType.Axis, 32)); // EL2872
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a25", EnumIOType.INPUT, EnumIONodeType.Axis, 32)); // EL2872


                    //stage
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g18", EnumIOType.AO, EnumIONodeType.IO, 8));    //EL4008
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g19", EnumIOType.OUTPUT, EnumIONodeType.IO, 16)); //EL2872
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g20", EnumIOType.OUTPUT, EnumIONodeType.IO, 16)); // EL2872
                                                                                                                      //Loader
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g06", EnumIOType.AO, EnumIONodeType.IO, 4));    //EL4004
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g07", EnumIOType.OUTPUT, EnumIONodeType.IO, 4)); //EL2024
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g08", EnumIOType.OUTPUT, EnumIONodeType.IO, 16)); // EL2872

                    // Stage Axis IO Defs.

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }
        }

    }



    [Serializable]
    public class ECATAxisDescripter : INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public ECATAxisDescripterParam ECATAxisDescripterParams { get; set; }
        public class ECATAxisDescripterParam : INotifyPropertyChanged, ISystemParameterizable
        {
            [JsonIgnore, ParamIgnore]
            public bool IsParamChanged { get; set; }
            public List<object> Nodes { get; set; }

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
            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged(String info)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                }
            }
            public string FilePath { get; } = "Elmo";
            public string FileName { get; } = "ECATAxisSetting.json";
            public EventCodeEnum SetEmulParam()
            {
                return SetDefaultParam();
            }
            public EventCodeEnum SetDefaultParam()
            {
                EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
                try
                {

                    //SetDefaultOPUSV_Machine3();
                    //SetDefaultBSCI1();
                    //SetDefaultOPUSVSTest();
                    SetDefaultSorter_LAB();

                    RetVal = EventCodeEnum.NONE;

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
                return RetVal;
            }

            private void SetDefaultSorter_LAB()
            {
                try
                {
                    ECATNodeDefinitions = new ObservableCollection<ECATNodeDefinition>();

                    //Stage
                    /*ECATNodeDefinitions.Add(new ECATNodeDefinition("a02", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // X:0
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a03", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // Y:1
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a04", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // TRI:2
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a05", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // THETA:3
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a06", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // PZ:4

                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a07", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.MASTER, 0) { GroupName = "v01" }); // Z0:5
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a08", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.SLAVE, 0) { GroupName = "v01" }); // Z1:6
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a09", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.SLAVE, 0) { GroupName = "v01" }); // z2:7
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("v01", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.GROUP, 0)); // GROUP Z:8*/
                    //251030 yb add
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a01", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a02", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a03", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a04", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a05", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a06", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a07", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a08", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a09", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a10", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));

                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a11", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a12", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a13", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a14", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a15", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a16", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a17", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a18", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a19", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a20", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));

                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a21", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a22", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a23", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g33", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g34", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g35", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g36", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g37", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g38", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("g39", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0));

                    GroupAxisDeviation = 2500;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }


            private void SetDefaultOPUSV_Machine3()
            {
                try
                {
                    ECATNodeDefinitions = new ObservableCollection<ECATNodeDefinition>();

                    //CC
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a01", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // CC1
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a02", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.MASTER, 0) { GroupName = "v02" }); // CCM
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a03", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.SLAVE, 0) { GroupName = "v02" }); // CCS
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a04", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // ROT
                                                                                                                                                   //Loader
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a11", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // u1
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a12", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // u2
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a13", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // W
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a14", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // A
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a15", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // SC
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a16", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // V
                                                                                                                                                   //Stage
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a24", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // X
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a25", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // Y
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a26", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // THETA
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a27", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // TRI
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a28", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // PZ

                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a29", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.MASTER, 0) { GroupName = "v01" }); // Z0
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a30", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.SLAVE, 0) { GroupName = "v01" }); // Z1
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a31", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.SLAVE, 0) { GroupName = "v01" }); // z2
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("v01", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.GROUP, 0)); // GROUP Z
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("v02", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.GROUP, 0)); // Group CC

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }
            private void SetDefaultOPUSVSTest()
            {
                try
                {
                    ECATNodeDefinitions = new ObservableCollection<ECATNodeDefinition>();

                    //OPUSV STage
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a13", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // x 
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a14", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // y 
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a16", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // theta 
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a01", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // CT 
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a17", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // MNC 
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a15", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // tri 
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a04", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // ROT 
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a03", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.MASTER, 0) { GroupName = "v01" }); // CCM 
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a02", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.SLAVE, 0) { GroupName = "v01" }); // CCS 
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("v01", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.GROUP, 0)); // Group

                    //OPUSV Loader
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a07", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // U1
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a08", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // U2
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a09", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // W
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a10", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // A
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a11", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // SCAN
                    //ECATNodeDefinitions.Add(new ECATNodeDefinition("a12", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // V

                    //CC
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a01", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // CC1
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a02", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.MASTER, 0) { GroupName = "v02" }); // CCM
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a03", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.SLAVE, 0) { GroupName = "v02" }); // CCS
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a04", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // ROT
                                                                                                                                                   //Loader
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a11", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // u1
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a12", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // u2
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a13", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // W
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a14", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // A
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a15", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // SC
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a16", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // V
                                                                                                                                                   //Stage
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a25", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // X
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a26", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // Y
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a27", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // TRI
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a28", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // THETA
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a29", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // PZ

                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a30", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.MASTER, 0) { GroupName = "v01" }); // Z0
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a31", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.SLAVE, 0) { GroupName = "v01" }); // Z1
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a32", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.SLAVE, 0) { GroupName = "v01" }); // z2
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("v01", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.GROUP, 0)); // GROUP Z
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("v02", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.GROUP, 0)); // Group CC
                    GroupAxisDeviation = 2000;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }

            private void SetDefaultBSCI1()
            {
                try
                {
                    ECATNodeDefinitions = new ObservableCollection<ECATNodeDefinition>();

                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a01", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // u1 
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a02", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // u2 
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a03", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // W 
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a04", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // V 
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a05", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // A 
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a06", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // SC 



                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a20", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // x 
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a21", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // y 
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a22", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // theta 
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a23", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // tri 
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a24", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.UNDEFINED, 0)); // MNC 
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a25", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.MASTER, 0) { GroupName = "v01" }); // Z0
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a26", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.SLAVE, 0) { GroupName = "v01" }); // Z1
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("a27", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.SLAVE, 0) { GroupName = "v01" }); // z2
                    ECATNodeDefinitions.Add(new ECATNodeDefinition("v01", EnumIOType.UNDEFINED, EnumIONodeType.Axis, EnumGroupType.GROUP, 0)); // z

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }

            ObservableCollection<ECATNodeDefinition> _ECATNodeDefinitions;
            public ObservableCollection<ECATNodeDefinition> ECATNodeDefinitions
            {
                get { return _ECATNodeDefinitions; }
                set { _ECATNodeDefinitions = value; }
            }

            private double _GroupAxisDeviation;
            public double GroupAxisDeviation
            {
                get { return _GroupAxisDeviation; }
                set { _GroupAxisDeviation = value; }
            }

        }
    }
    public class ECATNodeDefinition : INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private String _ID;
        [XmlAttribute("ID")]
        public String ID
        {
            get { return _ID; }
            set { _ID = value; }
        }
        private EnumIOType _IOType;
        [XmlAttribute("IOType")]
        public EnumIOType IOType
        {
            get { return _IOType; }
            set { _IOType = value; }
        }
        private int _ChNum;
        [XmlAttribute("ChannelNum")]
        public int ChNum
        {
            get { return _ChNum; }
            set { _ChNum = value; }
        }

        private EnumIONodeType _NodeType;
        [XmlAttribute("NodeType")]
        public EnumIONodeType NodeType
        {
            get { return _NodeType; }
            set { _NodeType = value; }
        }
        private EnumGroupType _GroupType;
        [XmlAttribute("GroupType")]
        public EnumGroupType GroupType
        {
            get { return _GroupType; }
            set { _GroupType = value; }
        }
        private string _GroupName = string.Empty;
        [XmlAttribute("GroupName")]
        public string GroupName
        {
            get { return _GroupName; }
            set { _GroupName = value; }
        }

        [XmlAttribute("InputVariablesCount")]
        public int InputVariablesCount { get; set; }

        [XmlAttribute("OutputVariablesCount")]
        public int OutputVariablesCount { get; set; }

        public ECATNodeDefinition()
        {
            try
            {
                _ID = "";
                _IOType = EnumIOType.UNDEFINED;
                _ChNum = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ECATNodeDefinition(string id, EnumIOType type, int chNum)
        {
            try
            {
                _ID = id;
                _IOType = type;
                _ChNum = chNum;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ECATNodeDefinition(string id, EnumIOType type, EnumIONodeType nodeType, int chNum)
        {
            try
            {
                _ID = id;
                _IOType = type;
                _NodeType = nodeType;
                _ChNum = chNum;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ECATNodeDefinition(string id, EnumIOType type, EnumIONodeType nodeType, EnumGroupType groupType, int chNum)
        {
            try
            {
                _ID = id;
                _IOType = type;
                _NodeType = nodeType;
                _ChNum = chNum;
                _GroupType = groupType;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ECATNodeDefinition(string id, EnumIONodeType nodeType, int inputVariablesCount, int outputVariablesCount)
        {
            try
            {
                ID = id;
                NodeType = nodeType;
                GroupType = EnumGroupType.UNDEFINED;
                GroupName = "";
                InputVariablesCount = inputVariablesCount;
                OutputVariablesCount = outputVariablesCount;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
