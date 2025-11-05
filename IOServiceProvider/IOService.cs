using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading;
using System.ComponentModel;
using System.Windows.Input;
using System.IO;
using ProberInterfaces;
using Autofac;
using Configurator;
using RelayCommandBase;
using ECATIO;
using ProberErrorCode;
using SystemExceptions.InOutException;
using System.Reflection;
using LogModule;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace IOServiceProvider
{
    public class IOService : IIOService, INotifyPropertyChanged, IParamNode
    {
        //public static List<ParameterKeyValuePair> ParameterPairs = new List<ParameterKeyValuePair>(new ParameterKeyValuePair[] {
        //                                            new ParameterKeyValuePair(typeof(DeviceConfigurator), @"C:\ProberSystem\Parameters\DeviceMapping.xml"),
        //});


        //private static readonly string DeviceDefinitionFilePath = @"C:\ProberSystem\Parameters\DeviceMapping.xml";
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

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        //private IParam _DevParam;
        //public IParam DevParam
        //{
        //    get { return _DevParam; }
        //    set
        //    {
        //        if (value != _DevParam)
        //        {
        //            _DevParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //private IParam _SysParam;
        //public IParam SysParam
        //{
        //    get { return _SysParam; }
        //    set
        //    {
        //        if (value != _SysParam)
        //        {
        //            _SysParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        public bool Initialized { get; set; } = false;

        private bool IsInfo = false;

        private List<IIOBase> _IOList;
        public List<IIOBase> IOList
        {
            get { return _IOList; }
            private set
            {
                if (value == _IOList) return;
                _IOList = value;
                RaisePropertyChanged();
            }
        }

        private IParam _DeviceConfigurator_IParam;
        public IParam DeviceConfigurator_IParam
        {
            get { return _DeviceConfigurator_IParam; }
            set
            {
                if (value != _DeviceConfigurator_IParam)
                {
                    _DeviceConfigurator_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private DeviceConfigurator _DevConfig;
        public DeviceConfigurator DevConfig
        {
            get { return DeviceConfigurator_IParam as DeviceConfigurator; }
        }

        private int MonitoringInterValInms = 12;

        bool bStopUpdateThread;


        Thread UpdateThread;

        public ObservableCollection<InputChannel> _Inputs;
        public ObservableCollection<InputChannel> Inputs
        {
            get { return _Inputs; }
            set
            {
                if (value == _Inputs) return;
                _Inputs = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<OutputChannel> _Outputs;
        public ObservableCollection<OutputChannel> Outputs
        {
            get { return _Outputs; }
            set
            {
                if (value == _Outputs) return;
                _Outputs = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<AnalogInputChannel> _AnalogInputs;
        public ObservableCollection<AnalogInputChannel> AnalogInputs
        {
            get { return _AnalogInputs; }
            set
            {
                if (value == _AnalogInputs) return;
                _AnalogInputs = value;
                RaisePropertyChanged();
            }
        }

        public IOService()
        {
            try
            {
                //IOList = new List<IIOBase>();
                //Inputs = new ObservableCollection<InputChannel>();
                //Outputs = new ObservableCollection<OutputChannel>();                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private bool _DevConnected;
        public bool DevConnected
        {
            get { return _DevConnected; }
            set { _DevConnected = value; }
        }

        public int DeInitIO()
        {
            int retVal = -1;

            try
            {
                for (int devindex = 0; devindex < IOList.Count; devindex++)
                {
                    IOList[devindex].DeInitIO();
                }

                //if (UpdateThread != null)
                //{
                //    bStopUpdateThread = true;
                //    UpdateThread.Join();
                //}

                bStopUpdateThread = true;
                UpdateThread?.Join();

                DevConnected = false;
                retVal = 0;
                ResultValidate(MethodBase.GetCurrentMethod(), (int)retVal);
            }
            catch (InOutException ex)
            {
                throw new InOutException("DeInitIO Error", ex, EventCodeEnum.IO_DEV_CONN_ERROR, (int)retVal, this);
            }
            catch (Exception err)
            {
                bStopUpdateThread = true;
                //UpdateThread.Join();
                UpdateThread?.Join();

                DevConnected = false;
                LoggerManager.Error($"DeInitIO() Function error: " + err.Message);
                throw new IOException(string.Format("DeInitIO(): Deinitializing failed."), err);
            }

            return retVal;
        }

        public int InitIOService()
        {
            int retVal = -1;

            try
            {
                for (int inputIndex = 0; inputIndex < DevConfig.DeviceConfiguratorParams.InputDescripter.Count; inputIndex++)
                {
                    if(DevConfig.DeviceConfiguratorParams.InputDescripter[inputIndex].IOType.Value == EnumIOType.AI)
                    {
                        AnalogInputs.Add(
                       new AnalogInputChannel(
                           DevConfig.DeviceConfiguratorParams.InputDescripter[inputIndex].Dev.Value,
                           DevConfig.DeviceConfiguratorParams.InputDescripter[inputIndex].HWChannel.Value,
                           DevConfig.DeviceConfiguratorParams.InputDescripter[inputIndex].Port.Value,
                           DevConfig.DeviceConfiguratorParams.InputDescripter[inputIndex].NodeIndex.Value,
                           DevConfig.DeviceConfiguratorParams.InputDescripter[inputIndex].VarOffset.Value)
                       { IOType = DevConfig.DeviceConfiguratorParams.InputDescripter[inputIndex].IOType.Value });
                    }
                    else
                    {
                        Inputs.Add(
                       new InputChannel(
                           DevConfig.DeviceConfiguratorParams.InputDescripter[inputIndex].Dev.Value,
                           DevConfig.DeviceConfiguratorParams.InputDescripter[inputIndex].HWChannel.Value,
                           DevConfig.DeviceConfiguratorParams.InputDescripter[inputIndex].Port.Value,
                           DevConfig.DeviceConfiguratorParams.InputDescripter[inputIndex].NodeIndex.Value,
                           DevConfig.DeviceConfiguratorParams.InputDescripter[inputIndex].VarOffset.Value)
                       { IOType = DevConfig.DeviceConfiguratorParams.InputDescripter[inputIndex].IOType.Value });
                    }
                }
                for (int outputIndex = 0; outputIndex < DevConfig.DeviceConfiguratorParams.OutputDescripter.Count; outputIndex++)
                {
                    Outputs.Add(
                    new OutputChannel(
                        DevConfig.DeviceConfiguratorParams.OutputDescripter[outputIndex].Dev.Value,
                        DevConfig.DeviceConfiguratorParams.OutputDescripter[outputIndex].HWChannel.Value,
                        DevConfig.DeviceConfiguratorParams.OutputDescripter[outputIndex].Port.Value,
                        DevConfig.DeviceConfiguratorParams.OutputDescripter[outputIndex].NodeIndex.Value,
                        DevConfig.DeviceConfiguratorParams.OutputDescripter[outputIndex].VarOffset.Value)
                    { IOType = DevConfig.DeviceConfiguratorParams.OutputDescripter[outputIndex].IOType.Value });
                }

                for (int devindex = 0; devindex < DevConfig.DeviceConfiguratorParams.DevDescripters.Count; devindex++)
                {
                    List<InputChannel> inputList = DevConfig.DeviceConfiguratorParams.InputDescripter.Where(c => c.IOType.Value == EnumIOType.INPUT && c.Dev.Value == devindex).
                        Select(a => new InputChannel(DevConfig.DeviceConfiguratorParams.DevDescripters[devindex].DeviceNum.Value, a.HWChannel.Value, a.Port.Value, a.NodeIndex.Value, a.VarOffset.Value)).ToList();
                    List<OutputChannel> outputList = DevConfig.DeviceConfiguratorParams.OutputDescripter.Where(c => c.Dev.Value == devindex).
                        Select(a => new OutputChannel(DevConfig.DeviceConfiguratorParams.DevDescripters[devindex].DeviceNum.Value, a.HWChannel.Value, a.Port.Value, a.NodeIndex.Value, a.VarOffset.Value) { IOType = a.IOType.Value}).ToList();
                    List<AnalogInputChannel> aiList = DevConfig.DeviceConfiguratorParams.InputDescripter.Where(ai => ai.IOType.Value == EnumIOType.AI && ai.Dev.Value == devindex).
                        Select(a => new AnalogInputChannel(DevConfig.DeviceConfiguratorParams.DevDescripters[devindex].DeviceNum.Value, a.HWChannel.Value, a.Port.Value, a.NodeIndex.Value, a.VarOffset.Value)).ToList();

                    List<Channel> ioList = new List<Channel>();
                    ioList.AddRange(inputList);
                    ioList.AddRange(aiList);
                    ioList.AddRange(outputList);

                    ObservableCollection<Channel> channels = new ObservableCollection<Channel>(ioList);

                    DeviceType deviceType = DevConfig.DeviceConfiguratorParams.DevDescripters[devindex].DeviceType.Value;

                    if (deviceType == DeviceType.USBNetIO)
                    {
                        throw new NotSupportedException();
                    }
                    else if (deviceType == DeviceType.EMulIO)
                    {
                        IOList.Add(new EmulIOModule.EmulIOProvider());
                        IOList[devindex].InitIO(DevConfig.DeviceConfiguratorParams.DevDescripters[devindex].DeviceNum.Value, channels);
                    }
                    else if (deviceType == DeviceType.ECATIO)
                    {
                        this.PMASManager().LoadSysParameter();
                        this.PMASManager().InitializeController();

                        IOList.Add(new ECATIOProvider());
                        IOList[devindex].InitIO(this.PMASManager().ConnHndl, channels);
                    }
                }

                if (DevConfig.DeviceConfiguratorParams.DevDescripters.Count > 0)
                {
                    DevConnected = true;
                    retVal = 0;
                }

                if (DevConnected)
                {
                    UpdateOutputChanels();

                    bStopUpdateThread = false;
                    UpdateThread = new Thread(new ThreadStart(UpdateIOProc));
                    UpdateThread.Name = this.GetType().Name;
                    UpdateThread.Start();
                }
                ResultValidate(MethodBase.GetCurrentMethod(), (int)retVal);
            }
            catch (InOutException ex)
            {
                throw new InOutException("InitIOService Error", ex, EventCodeEnum.IO_DEV_CONN_ERROR, (int)retVal, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                //LoggerManager.Error($err, string.Format("InitIOService(): System error occurred."));
            }

            return retVal;
        }

        private void UpdateOutputChanels()
        {
            for (int chIndex = 0; chIndex < Outputs.Count; chIndex++)
            {
                int dev = DevConfig.DeviceConfiguratorParams.OutputDescripter[chIndex].Dev.Value;

                //Channel ouputChannel = IOList[dev].Channels.FirstOrDefault(
                //    ch => ch is OutputChannel && (ch.ChannelIndex == Outputs[chIndex].ChannelIndex));

                Channel ouputChannel = IOList[dev].Channels.Where(
                    ch => ch is OutputChannel && (ch.ChannelIndex == Outputs[chIndex].ChannelIndex)).FirstOrDefault();


                if (DevConfig.DeviceConfiguratorParams.DevDescripters[dev].DeviceType.Value == DeviceType.ENetIO)
                {
                    Outputs[chIndex].Value = ouputChannel.Value;

                    for (int portIndex = 0; portIndex < Outputs[chIndex].Port.Count; portIndex++)
                    {
                        if (((Outputs[chIndex].Value >> portIndex) & 0x1) == 0x1)
                        {
                            Outputs[chIndex].Port[portIndex].PortVal = true;
                        }
                        else
                        {
                            Outputs[chIndex].Port[portIndex].PortVal = false;
                        }
                    }
                }
                else if (DevConfig.DeviceConfiguratorParams.DevDescripters[dev].DeviceType.Value == DeviceType.ECATIO)
                {
                    for (int portIndex = 0; portIndex < Outputs[chIndex].Port.Count; portIndex++)
                    {
                        Outputs[chIndex].Port[portIndex].PortVal = ouputChannel.Port[portIndex].PortVal;
                        //if (ouputChannel.Port[portIndex].PortVal == true)
                        //{
                        //    Outputs[chIndex].Values[portIndex].PortVal = 1;
                        //}
                        //else
                        //{
                        //    Outputs[chIndex].Values[portIndex].PortVal = 0;
                        //}
                    }
                }

            }
        }

        private void UpdateInputChanels()
        {
            for (int chIndex = 0; chIndex < Inputs.Count; chIndex++)
            {
                int dev = DevConfig.DeviceConfiguratorParams.InputDescripter[chIndex].Dev.Value;

                Channel inputChannel = IOList[dev].Channels.FirstOrDefault(
                    ch => ch is InputChannel && (ch.ChannelIndex == Inputs[chIndex].ChannelIndex)
                    );

                Inputs[chIndex].Value = inputChannel.Value;

                for (int portIndex = 0; portIndex < Inputs[chIndex].Port.Count; portIndex++)
                {
                    Inputs[chIndex].Port[portIndex].PortVal = inputChannel.Port[portIndex].PortVal;
                    //if (((Inputs[chIndex].Value >> portIndex) & 0x1) == 0x1)
                    //{
                    //    Inputs[chIndex].Port[portIndex].PortVal = true;
                    //}
                    //else
                    //{
                    //    Inputs[chIndex].Port[portIndex].PortVal = false;
                    //}
                    
                }

            }
        }

        private void UpdateAnalogInputChanels()
        {
            for (int chIndex = 0; chIndex < AnalogInputs.Count; chIndex++)
            {
                int dev = DevConfig.DeviceConfiguratorParams.InputDescripter[chIndex].Dev.Value;

                Channel analoginputChannel = IOList[dev].Channels.FirstOrDefault(
                    ch => ch is AnalogInputChannel && (ch.ChannelIndex == AnalogInputs[chIndex].ChannelIndex)
                    );

                AnalogInputs[chIndex].Value = analoginputChannel.Value;

                for (int portIndex = 0; portIndex < AnalogInputs[chIndex].Port.Count; portIndex++)
                {
                    AnalogInputs[chIndex].Port[portIndex].PortVal = analoginputChannel.Port[portIndex].PortVal;
                }

                for (int portIndex = 0; portIndex < AnalogInputs[chIndex].Values.Count; portIndex++)
                {
                    AnalogInputs[chIndex].Values[portIndex].PortVal = analoginputChannel.Values[portIndex].PortVal;
                }

            }
        }

        private void UpdateIOProc()
        {
            ushort[] recvData = new ushort[1];
            bool bExceptionState = false;
            try
            {
                while (bStopUpdateThread == false)
                {
                    try
                    {
                        if (DevConnected == true)
                        {
                            UpdateOutputChanels();

                            UpdateInputChanels();

                            UpdateAnalogInputChanels();
                        }
                        //minskim// GC 호출 및 CPU 사용률 절감을 위해 기존 timer+resetevent로 thread 제어하던 로직을 제거 하고 sleep으로 대체함
                        //sleep시간은 기존 timer interval 주기 값(Init 함수에서 setting 함)으로 설정함
                        System.Threading.Thread.Sleep(MonitoringInterValInms);
                        if (bExceptionState == true)
                        {
                            bExceptionState = false;
                        }
                    }
                    catch (Exception err)
                    {
                        if (bExceptionState == false)
                        {
                            LoggerManager.Error($"UpdateIOProc(): Exception occurred. Err = {err.Message}");
                            bExceptionState = true;
                        }
                    }
                }

            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("UpdateIOProc(): Error occurred while update io proc. Err = {0}", err.Message));
                LoggerManager.Exception(err);

            }

        }

        public IORet WriteBit(IOPortDescripter<bool> io, bool value)
        {
            IORet retVal = IORet.ERROR;
            try
            {
                if (io.ChannelIndex.Value == -1 ||
                    io.PortIndex.Value == -1 ||
                    io.IOOveride.Value == EnumIOOverride.NLO ||
                    io.IOOveride.Value == EnumIOOverride.NHI ||
                    io.IOOveride.Value == EnumIOOverride.EMUL)
                {
                    LoggerManager.Debug($"WriteBit({io.ChannelIndex.Value}, {io.PortIndex.Value}) : IO value write bit skip. value:{value}, overide:{io.IOOveride.Value}, io:{io.Key.Value}", isInfo: IsInfo);
                    retVal = IORet.NO_ERR;
                }
                else
                {
                    if (io.IOType.Value == EnumIOType.OUTPUT && IOList[Outputs[io.ChannelIndex.Value].DevIndex] != null)
                    {
                        if (io.LockState == IOLockState.UNLOCK)
                        {
                            if (io.Reverse.Value == true)
                            {                                    
                                retVal = IOList[Outputs[io.ChannelIndex.Value].DevIndex].WriteBit(Outputs[io.ChannelIndex.Value].ChannelIndex, io.PortIndex.Value, !value);
                                LoggerManager.Debug($"WriteBit({io.ChannelIndex.Value}, {io.PortIndex.Value}) : IO value writed with {value}, io:{io.Key.Value}", isInfo: IsInfo);
                                    
                                if (retVal == IORet.NO_ERR)
                                    io.Value = !value;
                            }
                            else
                            {                                    
                                retVal = IOList[Outputs[io.ChannelIndex.Value].DevIndex].WriteBit(Outputs[io.ChannelIndex.Value].ChannelIndex, io.PortIndex.Value, value);
                                LoggerManager.Debug($"WriteBit({io.ChannelIndex.Value}, {io.PortIndex.Value}) : IO value writed with {value}, io:{io.Key.Value}", isInfo: IsInfo);
                                    
                                if (retVal == IORet.NO_ERR)
                                    io.Value = value;
                            }

                        }
                        else
                        {
                            LoggerManager.Error($"IOLOCK STATE");
                        }
                    }
                    else if (io.IOType.Value == EnumIOType.INPUT)
                    {

                        throw new IOException(string.Format("Invalid port, defined as output port. Channel={0} Port={1}", io.ChannelIndex.Value, io.PortIndex.Value));
                    }
                }
                

                if (retVal != IORet.NO_ERR)
                {
                    LoggerManager.Debug($"[IOService_Write Bit][Result={retVal}] Description:{io.Description.Value} ,Value: {value}, Channel:{io.ChannelIndex.Value}, Port:{io.PortIndex.Value} Reverse:{ io.Reverse.Value}, io:{io.Key.Value}", isInfo: IsInfo);
                }

                ResultValidate(MethodBase.GetCurrentMethod(), (int)retVal);
            }
            catch (InOutException ex)
            {
                LoggerManager.Debug($"[IOService_Write Bit][Result={retVal}] Description:{io.Description.Value} ,Value: {value}, Channel:{io.ChannelIndex.Value}, Port:{io.PortIndex.Value} Reverse:{ io.Reverse.Value}, io:{io.Key.Value}", isInfo: IsInfo);
                throw new InOutException("WriteBit Error", ex, EventCodeEnum.IO_PORT_ERROR, (int)retVal, this);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"IOLOCK STATE");
                retVal = IORet.ERROR;
                LoggerManager.Debug($"[IOService_Write Bit][Result={retVal}] Description:{io.Description.Value} ,Value: {value}, Channel:{io.ChannelIndex.Value}, Port:{io.PortIndex.Value} Reverse:{ io.Reverse.Value}, io:{io.Key.Value}", isInfo: IsInfo);
                //LoggerManager.Error($string.Format("Invaild Parameter error Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public IORet ReadBit(IOPortDescripter<bool> io, out bool value)
        {
            IORet retVal = IORet.ERROR;
            value = false;
            try
            {
                if (io.ChannelIndex.Value != -1)
                {
                    if (io.IOType.Value == EnumIOType.INPUT && IOList[Inputs[io.ChannelIndex.Value].DevIndex] != null)
                    {
                        if (io.IOOveride.Value == EnumIOOverride.EMUL)
                        {
                            retVal = IORet.NO_ERR;
                        }
                        else
                        {
                            retVal = IOList[Inputs[io.ChannelIndex.Value].DevIndex].ReadBit(Inputs[io.ChannelIndex.Value].ChannelIndex, io.PortIndex.Value, out value, io.Reverse.Value, isForced:io.ForcedIO.IsForced, ForcedValue: io.ForcedIO.ForecedValue);
                        }
                    }
                    else if (io.IOType.Value == EnumIOType.OUTPUT && IOList[Outputs[io.ChannelIndex.Value].DevIndex] != null)
                    {
                        if (io.IOOveride.Value == EnumIOOverride.EMUL)
                        {
                            value = io.Value;
                            retVal = IORet.NO_ERR;
                        }
                        else
                        {
                            retVal = IOList[Outputs[io.ChannelIndex.Value].DevIndex].ReadBit(Outputs[io.ChannelIndex.Value].ChannelIndex, io.PortIndex.Value, out value);
                        }
                    }
                    else
                    {
                        retVal = IORet.ERROR;
                    }
                }
                else if(io.ChannelIndex.Value == -1 && io.PortIndex.Value == -1 && io.IOOveride.Value == EnumIOOverride.NLO)
                {
                    retVal = IORet.NO_ERR;
                }
                //else
                //{
                //    throw new Exception();
                //}

                if (io.IOOveride.Value == EnumIOOverride.EMUL)
                {
                    retVal = IORet.NO_ERR;
                }

                if (retVal != IORet.NO_ERR)
                {
                    LoggerManager.Debug($"[IOService_Write Bit][Result={retVal}] Description:{io.Description.Value} ,Value: {value}, Channel:{io.ChannelIndex.Value}, Port:{io.PortIndex.Value} Reverse:{ io.Reverse.Value}", isInfo: IsInfo);
                }
                ResultValidate(MethodBase.GetCurrentMethod(), (int)retVal);
            }
            catch (InOutException ex)
            {
                LoggerManager.Debug($"[IOService_Read Bit][Result={retVal}] Description:{io.Description.Value} ,Value: {value}, Channel:{io.ChannelIndex.Value}, Port:{io.PortIndex.Value} Reverse:{ io.Reverse.Value}", isInfo: IsInfo);
                throw new InOutException("ReadBit Error", ex, EventCodeEnum.IO_PORT_ERROR, (int)retVal, this);                
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[IOService_Read Bit][Result={retVal}] Description:{io.Description.Value} ,Value: {value}, Channel:{io.ChannelIndex.Value}, Port:{io.PortIndex.Value} Reverse:{ io.Reverse.Value}", isInfo: IsInfo);
                //LoggerManager.Error($string.Format("Invaild Parameter error Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        //public IORet ReadBit(int channel, int port, out bool value, bool reverse = false)
        //{
        //    value = false;
        //    if (IOList[Inputs[channel].DevIndex] != null)
        //    {
        //        return IOList[Inputs[channel].DevIndex].ReadBit(Inputs[channel].ChannelIndex, port, out value, reverse);
        //    }
        //    return IORet.ERROR;
        //}

        public int WaitForIO(IOPortDescripter<bool> io, bool level, long timeout = 0)
        {
            int retVal = -1;
            try
            {
                if (io.ChannelIndex.Value != -1)
                {
                    if (io.IOType.Value == EnumIOType.INPUT && IOList[Inputs[io.ChannelIndex.Value].DevIndex] != null)
                    {
                        if (io.Reverse.Value)
                        {
                            level = !level;
                        }
                        if (io.IOOveride.Value == EnumIOOverride.EMUL)
                        {
                            retVal = 0;
                        }
                        else
                        {
                            retVal = IOList[Inputs[io.ChannelIndex.Value].DevIndex].WaitForIO(Inputs[io.ChannelIndex.Value].ChannelIndex, io.PortIndex.Value, level, timeout, io.ForcedIO.IsForced, io.ForcedIO.ForecedValue);
                        }
                    }
                    else if (io.IOType.Value == EnumIOType.OUTPUT && IOList[Outputs[io.ChannelIndex.Value].DevIndex] != null)
                    {
                        if (io.IOOveride.Value == EnumIOOverride.EMUL)
                        {
                            retVal = 0;
                        }
                        else
                        {
                            retVal = IOList[Outputs[io.ChannelIndex.Value].DevIndex].WaitForIO(Outputs[io.ChannelIndex.Value].ChannelIndex, io.PortIndex.Value, level, timeout);
                        }
                    }
                    else
                    {
                        retVal = -1;
                    }
                }
                else
                {
                    if (io.IOOveride.Value == EnumIOOverride.EMUL)
                    {
                        retVal = 0;
                    }
                    else
                    {
                        retVal = -1;
                    }
                }

                if (retVal != -2)// timeout 일때는 제외
                {
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                }
                else
                {
                    LoggerManager.Debug($"[IOService_WaitForIO][Result={retVal}] Description:{io.Description.Value} ,Value: {level}, Channel:{io.ChannelIndex.Value}, Port:{io.PortIndex.Value} Reverse:{ io.Reverse.Value}", isInfo: IsInfo);
                    LoggerManager.Error($"WaitForIO TimeOut {io.Description.Value}");
                }
            }
            catch (InOutException ex)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, ex);
                LoggerManager.Debug($"[IOService_WaitForIO][Result={retVal}] Description:{io.Description.Value} ,Value: {level}, Channel:{io.ChannelIndex.Value}, Port:{io.PortIndex.Value} Reverse:{ io.Reverse.Value}", isInfo: IsInfo);
                LoggerManager.Error(string.Format("WaitForIO Error Err = {0}", EventCodeEnum.IO_TIMEOUT_ERROR));
                // throw new InOutException("WaitForIO Error", ex, EventCodeEnum.IO_TIMEOUT_ERROR, (int)retVal, this);
            }
            catch (Exception err)
            {
                retVal = -1;
                LoggerManager.Debug($"[IOService_WaitForIO][Result={retVal}] Description:{io.Description.Value} ,Value: {level}, Channel:{io.ChannelIndex.Value}, Port:{io.PortIndex.Value} Reverse:{ io.Reverse.Value}", isInfo: IsInfo);
                //LoggerManager.Error($string.Format("Invaild Parameter error Err = {0}", err.Message));
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public int MonitorForIO(IOPortDescripter<bool> io, bool level, long maintainTime = 0, long timeout = 0, bool writelog = true)
        {
            int retVal = -1;
            try
            {
                if(io != null)
                {
                    if (io.ChannelIndex.Value != -1)
                    {
                        if (io.IOType.Value == EnumIOType.INPUT && IOList[Inputs[io.ChannelIndex.Value].DevIndex] != null)
                        {
                            if (io.IOOveride.Value == EnumIOOverride.EMUL)
                            {
                                retVal = 0;
                            }
                            else
                            {
                                retVal = IOList[Inputs[io.ChannelIndex.Value].DevIndex].MonitorForIO(Inputs[io.ChannelIndex.Value].ChannelIndex,
                                        io.PortIndex.Value,
                                        level,
                                        maintainTime,
                                        timeout,
                                        io.Reverse.Value, io.ForcedIO.IsForced, io.ForcedIO.ForecedValue,
                                        writelog,
                                        io.Key.Value);
                            }
                        }
                        else if (io.IOType.Value == EnumIOType.OUTPUT && IOList[Outputs[io.ChannelIndex.Value].DevIndex] != null)
                        {
                            if (io.IOOveride.Value == EnumIOOverride.EMUL)
                            {
                                retVal = 0;
                            }
                            else
                            {
                                retVal = IOList[Outputs[io.ChannelIndex.Value].DevIndex].MonitorForIO(Outputs[io.ChannelIndex.Value].ChannelIndex,
                                        io.PortIndex.Value,
                                        level,
                                        maintainTime,
                                        timeout,
                                        io.Reverse.Value, false, false,
                                        writelog,
                                        io.Key.Value);
                            }
                        }
                        else
                        {
                            retVal = -1;
                        }
                    }
                    else if (io.IOOveride.Value == EnumIOOverride.EMUL)
                    {
                      LoggerManager.Debug($"[IOService_MonitorForIO] IO Type is Emul.RetValue:0");
                      retVal = 0;
                    }
                    else
                    {
                        if (io.IOOveride.Value == EnumIOOverride.EMUL)
                            retVal = 0;
                        else
                        {
                            retVal = -1;
                            throw new Exception();
                        }                            
                    }
                }
                else
                {
                    retVal = -1;
                }

                if (retVal != -2)// timeout 일때는 제외
                {
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                }
                else
                {
                    if(writelog == true)
                    {
                        LoggerManager.Error($"MonitorForIO TimeOut {io.Description.Value}");
                    }
                }

            }
            catch (InOutException ex)
            {
                throw new InOutException("LoadParameter Error", ex, EventCodeEnum.IO_DEV_CONN_ERROR, (int)retVal, this);
            }
            catch (Exception err)
            {
                retVal = -1;
                LoggerManager.Debug($"[IOService_MoitorForIO][Result={retVal}] Description:{io.Description.Value} ,Value: {level}, Channel:{io.ChannelIndex.Value}, Port:{io.PortIndex.Value} Reverse:{ io.Reverse.Value}", isInfo: IsInfo);
                //LoggerManager.Error($string.Format("Invaild Parameter error Err = {0}", err.Message));
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public void BitOutputEnable(IOPortDescripter<bool> ioport)
        {
            try
            {
                WriteBit(ioport, true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void BitOutputDisable(IOPortDescripter<bool> ioport)
        {
            WriteBit(ioport, false);
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    IOList = new List<IIOBase>();
                    Inputs = new ObservableCollection<InputChannel>();
                    Outputs = new ObservableCollection<OutputChannel>();
                    AnalogInputs = new ObservableCollection<AnalogInputChannel>();


                    //==> Ioservice Ãß°¡
                    InitIOService();
                    MonitoringInterValInms = 100;

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void DeInitModule()
        {

        }

        #region Foup
        private int _ECAT_CTRL_NUM = 0;
        public EventCodeEnum InitModule(int ctrlNum, string devConfigParam, string ecatIOConfigParam)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                _ECAT_CTRL_NUM = ctrlNum;
                IOList = new List<IIOBase>();
                Inputs = new ObservableCollection<InputChannel>();
                Outputs = new ObservableCollection<OutputChannel>();


                IParam param = null;
                retVal = this.LoadParameter(ref param, typeof(DeviceConfigurator.DeviceConfiguratorParam), null, devConfigParam);


                FoupInitIOService(ecatIOConfigParam);
                MonitoringInterValInms = 12;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }        
        private int FoupInitIOService(string ECATIOFilePath)
        {
            int retVal = -1;
            try
            {

                foreach (var inputDescripter in DevConfig.DeviceConfiguratorParams.InputDescripter)
                {
                    Inputs.Add(new InputChannel(inputDescripter.Dev.Value, inputDescripter.HWChannel.Value, inputDescripter.Port.Value, inputDescripter.NodeIndex.Value, inputDescripter.VarOffset.Value));
                }

                foreach (var outputDescripter in DevConfig.DeviceConfiguratorParams.OutputDescripter)
                {
                    Outputs.Add(new OutputChannel(outputDescripter.Dev.Value, outputDescripter.HWChannel.Value, outputDescripter.Port.Value, outputDescripter.NodeIndex.Value, outputDescripter.VarOffset.Value));
                }

                for (int devindex = 0; devindex < DevConfig.DeviceConfiguratorParams.DevDescripters.Count; devindex++)
                {
                    List<InputChannel> inputList = DevConfig.DeviceConfiguratorParams.InputDescripter.Where(c => c.Dev.Value == devindex).
                        Select(a => new InputChannel(DevConfig.DeviceConfiguratorParams.DevDescripters[devindex].DeviceNum.Value, a.HWChannel.Value, a.Port.Value, a.NodeIndex.Value, a.VarOffset.Value)).ToList();
                    List<OutputChannel> outputList = DevConfig.DeviceConfiguratorParams.OutputDescripter.Where(c => c.Dev.Value == devindex).
                        Select(a => new OutputChannel(DevConfig.DeviceConfiguratorParams.DevDescripters[devindex].DeviceNum.Value, a.HWChannel.Value, a.Port.Value, a.NodeIndex.Value, a.VarOffset.Value)).ToList();

                    List<Channel> ioList = new List<Channel>();
                    ioList.AddRange(inputList);
                    ioList.AddRange(outputList);

                    ObservableCollection<Channel> channels = new ObservableCollection<Channel>(ioList);

                    if (DevConfig.DeviceConfiguratorParams.DevDescripters[devindex].DeviceType.Value == DeviceType.USBNetIO)
                    {
                        throw new NotSupportedException();
                        //IOList.Add(new AdvanUSBIOLib());
                        //IOList[devindex].InitIO(DevConfig.DevDescripters[devindex].DeviceNum, inputs, outputs);
                    }
                    else if (DevConfig.DeviceConfiguratorParams.DevDescripters[devindex].DeviceType.Value == DeviceType.EMulIO)
                    {
                        IOList.Add(new EmulIOModule.EmulIOProvider());
                        IOList[devindex].InitIO(DevConfig.DeviceConfiguratorParams.DevDescripters[devindex].DeviceNum.Value, channels);
                        return retVal;
                    }
                    else if (DevConfig.DeviceConfiguratorParams.DevDescripters[devindex].DeviceType.Value == DeviceType.ECATIO)
                    {
                        IOList.Add(new ECATIOProvider());
                        IOList[devindex].InitIO(_ECAT_CTRL_NUM, channels);
                    }
                }

                if (DevConfig.DeviceConfiguratorParams.DevDescripters.Count > 0)
                {
                    DevConnected = true;
                    retVal = 1;
                }

                if (DevConnected)
                {
                    UpdateOutputChanels();

                    bStopUpdateThread = false;
                    UpdateThread = new Thread(new ThreadStart(UpdateIOProc));
                    UpdateThread.Start();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        #endregion

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new DeviceConfigurator();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(DeviceConfigurator));

                if (RetVal != EventCodeEnum.NONE)
                {
                    ResultValidate(MethodBase.GetCurrentMethod(), (int)RetVal);
                }
                else
                {
                    DeviceConfigurator_IParam = tmpParam;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(DeviceConfigurator_IParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        //public EventCodeEnum LoadParameter()
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        RetVal = LoadSysParameter();
        //        if (RetVal != EventCodeEnum.NONE)
        //        {
        //            ResultValidate(MethodBase.GetCurrentMethod(), (int)RetVal);
        //        }
        //    }
        //    catch (InOutException ex)
        //    {
        //        throw new InOutException("LoadParameter Error", ex, EventCodeEnum.IO_PARAM_ERROR,(int)RetVal, this);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw new Exception("StartScanPosCapt Error in MotionProvider", ex);
        //    }
        //    return RetVal;
        //}

        //public EventCodeEnum SaveParameter()
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
        //    RetVal = SaveSysParameter();
        //    if (RetVal != EventCodeEnum.NONE)
        //    {
        //        ResultValidate(MethodBase.GetCurrentMethod(), (int)RetVal);
        //    }
        //    return RetVal;
        //}

        private RelayCommand<IOPortDescripter<bool>> _BitOutputEnableCommand;
        public ICommand BitOutputEnableCommand
        {
            get
            {
                if (null == _BitOutputEnableCommand) _BitOutputEnableCommand
                        = new RelayCommand<IOPortDescripter<bool>>(BitOutputEnable);
                return _BitOutputEnableCommand;
            }
        }
        private RelayCommand<IOPortDescripter<bool>> _BitOutputDisableCommand;
        public ICommand BitOutputDisableCommand
        {
            get
            {
                if (null == _BitOutputDisableCommand) _BitOutputDisableCommand
                        = new RelayCommand<IOPortDescripter<bool>>(BitOutputDisable);
                return _BitOutputDisableCommand;
            }
        }

        public int ResultValidate(object funcname, int retcode)
        {
            if ((IORet)retcode != IORet.NO_ERR)
            {
                throw new InOutException(string.Format("Funcname: {0} ReturnValue: {1} Error occurred", funcname.ToString(), retcode.ToString()));
            }
            return retcode;
        }



    }
}
