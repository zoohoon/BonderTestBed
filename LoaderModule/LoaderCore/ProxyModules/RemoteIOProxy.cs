using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoaderCore.ProxyModules
{
    using Autofac;
    using LoaderBase;
    using ProberErrorCode;
    using ProberInterfaces;
    using LogModule;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.IO;
    using System.Threading;
    using IOMappingsObject;
    using System.Runtime.CompilerServices;
    using TwinCatHelper;
    using System.Collections.ObjectModel;

    public class RemoteIOProxy : IFactoryModule, IIOManagerProxy, INotifyPropertyChanged, IHasSysParameterizable
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        bool bStopUpdateThread;
        Thread UpdateThread;

        private IIOManager _IOStates;
        public IIOManager IOStates { get => GetIOStates(); }
        public IIOManager GetIOStates()
        {
            if (_IOStates == null)
                GetModule<IIOManager>(out _IOStates);

            return _IOStates;
        }

        private IGPLoader _Remote;
        public IGPLoader Remote { get => GetRemote(); }
        public IGPLoader GetRemote()
        {
            if (_Remote == null)
                GetModule<IGPLoader>(out _Remote);

            return _Remote;
        }

        private void GetModule<T>(out T module)
        {
            if (Container != null)
            {
                if (Container.IsRegistered<T>() == true)
                {
                    module = Container.Resolve<T>();
                }
                else
                {
                    module = default(T);
                }
            }
            else
            {
                module = default(T);
            }
        }

        //public IIOManager IOStates => Container.Resolve<IIOManager>();
        public IIOMappingsParameter IOMappings => IOMapParam;
        private IOMappings _IOMapParam;
        public IOMappings IOMapParam
        {
            get { return _IOMapParam; }
            set
            {
                if (value != _IOMapParam)
                {
                    _IOMapParam = value;
                    RaisePropertyChanged();
                }
            }
        }
      
        private int inputPortCount = 16;
        private int outputPortCount = 16;

        public List<IOPortDescripter<bool>> InputPortDescs { get; set; }
        public List<IOPortDescripter<bool>> OutputPortDescs { get; set; }
        //public List<InputChannel> InChannels { get; set; }
        public bool Initialized { get; set; } = false;

        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL3;

        public Autofac.IContainer Container { get; set; }

        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();
        //public IGPLoader Remote => Container.Resolve<IGPLoader>();
        private static ManualResetEvent mreUpdateEvent = new ManualResetEvent(false);
        private ADSRouter tc => (ADSRouter)Remote.RemoteModule;
        public void DeInitModule()
        {
            bStopUpdateThread = true;
            if (UpdateThread != null)
            {
                UpdateThread.Join();
            }
            Initialized = false;
        }
        public List<IOPortDescripter<bool>> GetInputPorts()
        {
            List<IOPortDescripter<bool>> inputPorts = new List<IOPortDescripter<bool>>();
            try
            {
                var inputs = IOMappings.RemoteInputs;
                var inputType = inputs.GetType();
                var props = inputType.GetProperties();

                foreach (var item in props)
                {

                    //var port = item.GetValue(inputs) as List<IOPortDescripter<bool>>;
                    if (item.PropertyType == typeof(List<IOPortDescripter<bool>>))
                    {
                        var ios = item.GetValue(inputs) as List<IOPortDescripter<bool>>;
                        if (ios != null)
                        {
                            foreach (var port in ios)
                            {
                                if (port is IOPortDescripter<bool>)
                                {
                                    inputPorts.Add(port);
                                }
                            }
                        }
                    }
                    else if (item.PropertyType == typeof(IOPortDescripter<bool>))
                    {
                        var iodesc = item.GetValue(inputs) as IOPortDescripter<bool>;
                        inputPorts.Add(iodesc);
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return inputPorts;
        }
        public IOPortDescripter<bool> GetIOPortDescripter(string ioName)
        {
            try
            {
                if (!string.IsNullOrEmpty(ioName))
                {
                    var inputs = IOMappings.RemoteInputs;
                    var inputType = inputs.GetType();

                    if (ioName.Contains("."))
                    {
                        string propName = ioName.Substring(0, ioName.IndexOf('.'));
                        var indexOfComma = ioName.IndexOf('.');
                        var lengthOfName = ioName.Length;
                        string indexString = ioName.Substring(indexOfComma + 1, lengthOfName - indexOfComma - 1);
                        var prop = inputType.GetProperty(propName);

                        int indexOfProp = 0;

                        if (int.TryParse(indexString, out indexOfProp))
                        {
                            if (prop != null)
                            {
                                if (prop.PropertyType == typeof(List<IOPortDescripter<bool>>))
                                {
                                    var ios = prop.GetValue(inputs) as List<IOPortDescripter<bool>>;

                                    if (ios != null && indexOfProp >= 0 & indexOfProp < ios.Count)
                                    {
                                        var iodesc = ios[indexOfProp] as IOPortDescripter<bool>;
                                        return iodesc;
                                    }
                                    else
                                    {
                                        return null;
                                    }

                                }
                            }
                        }

                        var outputs = IOMappings.RemoteOutputs;
                        var outputType = outputs.GetType();
                        prop = outputType.GetProperty(propName);
                        if (int.TryParse(indexString, out indexOfProp))
                        {
                            if (prop != null)
                            {

                                if (prop.PropertyType == typeof(List<IOPortDescripter<bool>>))
                                {
                                    var ios = prop.GetValue(outputs) as List<IOPortDescripter<bool>>;
                                    if (indexOfProp >= 0 & indexOfProp < ios.Count)
                                    {
                                        var iodesc = ios[indexOfProp] as IOPortDescripter<bool>;
                                        return iodesc;
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var prop = inputType.GetProperty(ioName);
                        if (prop != null)
                        {
                            if (prop.PropertyType == typeof(IOPortDescripter<bool>))
                            {
                                var iodesc = prop.GetValue(inputs) as IOPortDescripter<bool>;
                                return iodesc;
                            }
                        }

                        var outputs = IOMappings.RemoteOutputs;
                        var outputType = outputs.GetType();
                        prop = outputType.GetProperty(ioName);
                        if (prop != null)
                        {
                            if (prop.PropertyType == typeof(IOPortDescripter<bool>))
                            {
                                var iodesc = prop.GetValue(outputs) as IOPortDescripter<bool>;
                                return iodesc;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return null;
        }

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    this.Container = container;

                    InputPortDescs = GetInputPorts();
                    IOStates.IOServ.Inputs = new ObservableCollection<InputChannel>();
                    for (int i = 0; i < inputPortCount; i++)
                    {
                        IOStates.IOServ.Inputs.Add(new InputChannel(0, i, 32, 0, 0));
                    }

                    for (int i = 0; i < outputPortCount; i++)
                    {
                        IOStates.IOServ.Outputs.Add(new OutputChannel(0, i, 32, 0, 0));
                    }
                    bStopUpdateThread = false;
                    UpdateThread = new Thread(new ThreadStart(UpdateProc));
                    UpdateThread.Name = this.GetType().Name;

                    UpdateThread.Start();

                    Initialized = true;
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION");

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        private void UpdateProc()
        {
            bool extendedIO = false;
            try
            {
                if (Remote.InputStates != null & Remote.OutputStates != null)
                {
                    extendedIO = true;
                }

                while (bStopUpdateThread == false)
                {
                    try
                    {
                        if (extendedIO == true)
                        {
                            if (Remote.InputStates.Count > 0)
                            {
                                Parallel.For(0, Remote.InputStates.Count, new ParallelOptions { MaxDegreeOfParallelism = 2 }, i =>
                                {
                                    IOStates.IOServ.Inputs[i].Value = (UInt32)Remote.InputStates[i];
                                });
                            }

                            if (Remote.OutputStates.Count > 0)
                            {
                                Parallel.For(0, Remote.OutputStates.Count, new ParallelOptions { MaxDegreeOfParallelism = 2 }, i =>
                                {
                                    IOStates.IOServ.Outputs[i].Value = (UInt32)Remote.OutputStates[i];
                                });
                            }
                        }
                        else
                        {
                            // ToDo: channel data type 변경(byte -> uint32)
                            IOStates.IOServ.Inputs[0].Value = (UInt32)Remote.CDXIn.Input1_State;
                            IOStates.IOServ.Inputs[1].Value = (UInt32)Remote.CDXIn.Input2_State;
                            IOStates.IOServ.Inputs[2].Value = (UInt32)Remote.CDXIn.Input3_State;
                            IOStates.IOServ.Inputs[3].Value = (UInt32)Remote.CDXIn.Input4_State;
                        }

                        for (int index = 0; index < Remote.InputStates.Count; index++)
                        {
                            Parallel.For(0, IOStates.IOServ.Inputs[0].Port.Count, new ParallelOptions { MaxDegreeOfParallelism = 2 }, i =>
                            {
                                IOStates.IOServ.Inputs[index].Port[i].PortVal = (((IOStates.IOServ.Inputs[index].Value >> i) & 0x01) == 0x01);
                            });
                        }


                        Parallel.For(0, InputPortDescs.Count, new ParallelOptions { MaxDegreeOfParallelism = 4 }, i =>
                        {
                            try
                            {
                                if(InputPortDescs[i].ForcedIO.IsForced == true)
                                {
                                    var value = InputPortDescs[i].ForcedIO.ForecedValue;
                                    InputPortDescs[i].Value = value;
                                }
                                else
                                {
                                    if (InputPortDescs[i].ChannelIndex.Value > -1 && InputPortDescs[i].PortIndex.Value > -1)
                                    {
                                        if (InputPortDescs[i].Reverse.Value == true)
                                        {
                                            InputPortDescs[i].Value = !(((IOStates.IOServ.Inputs[InputPortDescs[i].ChannelIndex.Value].Value >> InputPortDescs[i].PortIndex.Value) & 0x01) == 0x01);

                                        }
                                        else
                                        {
                                            InputPortDescs[i].Value = (((IOStates.IOServ.Inputs[InputPortDescs[i].ChannelIndex.Value].Value >> InputPortDescs[i].PortIndex.Value) & 0x01) == 0x01);
                                        }
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }

                        });

                        Thread.Sleep(150);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Debug($"Exception occurred. Err. = {err.Message}");
                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("UpdateIOProc(): Error occurred while update io proc. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule()
        {
            //Initialized = true;
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            IParam tmpParam = null;
            tmpParam = new IOMappings();
            tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
            RetVal = this.LoadParameter(ref tmpParam, typeof(IOMappings), null, null, Extensions_IParam.FileType.XML);

            if (RetVal == EventCodeEnum.NONE)
            {
                IOMapParam = tmpParam as IOMappings;
            }

           
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(IOMapParam);
                if (RetVal != EventCodeEnum.NONE)
                {
                    throw new Exception($"[{this.GetType().Name} - SaveSysParameter] Faile SaveParameter");
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[IOStates] SaveParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }
            return RetVal;
        }
        public EventCodeEnum MonitorForIO(IOPortDescripter<bool> iodesc, bool level, long sustain = 500, long timeout = 1000, bool writelog = true)
        {
            if (timeout == 0)
                timeout = 10000;
            //#endif
            int channel = iodesc.ChannelIndex.Value;
            int port = iodesc.PortIndex.Value;
            //=> Return Values
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            bool matched = false;
            Stopwatch elapsedStopWatch = new Stopwatch();
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            int cnt = 0;
            Stopwatch sustainStopWatch = new Stopwatch();
            sustainStopWatch.Reset();

            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();

            try
            {
                bool runFlag = true;
                bool value;

                if (iodesc.IOOveride.Value == EnumIOOverride.EMUL)
                {
                    if (writelog == true)
                    {
                        LoggerManager.Debug($"MonitorForIO({iodesc.Description}): IO overrided(Emul). Demend value = {level}");
                    }
                    runFlag = false;
                    errorCode = EventCodeEnum.NONE;
                }
                else
                {
                    //timeStamp.Add(new KeyValuePair<string, long>($"Entering DoWhile Loop", elapsedStopWatch.ElapsedMilliseconds));
                    do
                    {
                        //timeStamp.Add(new KeyValuePair<string, long>($"ReadBit Start.", elapsedStopWatch.ElapsedMilliseconds));
                        errorCode = ReadIO(iodesc, out value);
                        //timeStamp.Add(new KeyValuePair<string, long>($"ReadBit Done.", elapsedStopWatch.ElapsedMilliseconds));

                        cnt++;
                        if (errorCode == EventCodeEnum.NONE)
                        {
                            if (timeout != 0)
                            {
                                if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                                {
                                    if (writelog == true)
                                    {
                                        LoggerManager.Error($"MonitorForIO({channel}, {port}) : wait io timeout error occurred. Timeout = {timeout}ms");
                                    }
                                    runFlag = false;
                                    errorCode = EventCodeEnum.IO_TIMEOUT_ERROR;
                                    //throw new InOutException(
                                    //    string.Format("WaitForIO({0}, {1}) : wait io timeout error occurred. Timeout = {2}ms",
                                    //    channel, port, timeout));
                                }
                                else
                                {
                                    if (value == level)
                                    {
                                        if (matched == false)
                                        {
                                            sustainStopWatch.Start();
                                            matched = true;
                                            //timeStamp.Add(new KeyValuePair<string, long>($"Value matched.", elapsedStopWatch.ElapsedMilliseconds));

                                        }
                                        else
                                        {
                                            if (sustainStopWatch.ElapsedMilliseconds > sustain)
                                            {
                                                runFlag = false;
                                                errorCode = EventCodeEnum.NONE;
                                                if (writelog == true)
                                                {
                                                    LoggerManager.Debug($"MonitorForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
                                                }
                                                sustainStopWatch.Stop();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        sustainStopWatch.Stop();
                                        sustainStopWatch.Reset();

                                        matched = false;
                                        runFlag = true;
                                    }
                                }
                            }
                            else
                            {
                                if (value == level)
                                {
                                    runFlag = false;
                                    errorCode = EventCodeEnum.NONE;
                                    if (writelog == true)
                                    {
                                        LoggerManager.Debug($"MonitorForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
                                    }
                                }
                                else
                                {
                                    runFlag = true;
                                }
                            }
                        }
                        else
                        {
                            runFlag = false;

                            if (writelog == true)
                            {
                                LoggerManager.Error($"MonitorForIO({channel}, {port}) : wait io error occurred. Timeout = {timeout}ms");
                            }

                        }

                        mreUpdateEvent.WaitOne(2);

                    } while (runFlag == true);

                }
            }
            catch (Exception err)
            {
                errorCode = EventCodeEnum.SYSTEM_ERROR;
                //LoggerManager.Error($string.Format("MonitorForIO({0}, {1}) : wait io error occurred. Timeout = {2}ms, Err = {3}", channel, port, timeout, err.Message));
                LoggerManager.Exception(err);

            }
            finally
            {
                elapsedStopWatch?.Stop();

            }

            return errorCode;
        }

        public EventCodeEnum ReadIO(IOPortDescripter<bool> iodesc, out bool value)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;

            try
            {
                if (!iodesc.ForcedIO.IsForced)
                {
                    if (iodesc.IOOveride.Value == EnumIOOverride.NHI)
                    {
                        value = true;
                    }
                    else if (iodesc.IOOveride.Value == EnumIOOverride.NLO)
                    {
                        value = false;
                    }
                    else
                    {
                        value = iodesc.Value;
                    }
                }
                else
                {
                    value = iodesc.ForcedIO.ForecedValue;
                }

                errorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                value = false;
                errorCode = EventCodeEnum.SYSTEM_ERROR;
                LoggerManager.Exception(err);
            }
            return errorCode;
        }

        public EventCodeEnum WaitForIO(IOPortDescripter<bool> iodesc, bool level, long timeout = 1000)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            Stopwatch elapsedStopWatch = new Stopwatch();
            int channel = iodesc.ChannelIndex.Value;
            int port = iodesc.PortIndex.Value;

            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            int cnt = 0;
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();

            try
            {
                bool runFlag = true;
                bool value;

                timeStamp.Add(new KeyValuePair<string, long>($"Entering DoWhile Loop", elapsedStopWatch.ElapsedMilliseconds));
                do
                {
                    timeStamp.Add(new KeyValuePair<string, long>($"ReadBit Start.", elapsedStopWatch.ElapsedMilliseconds));
                    errorCode = ReadIO(iodesc, out value);
                    timeStamp.Add(new KeyValuePair<string, long>($"ReadBit Done.", elapsedStopWatch.ElapsedMilliseconds));

                    cnt++;
                    if (errorCode == EventCodeEnum.NONE)
                    {
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {
                                LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io timeout error occurred. Timeout = {timeout}ms");

                                runFlag = false;
                                errorCode = EventCodeEnum.IO_TIMEOUT_ERROR;
                                throw new IOException(
                                    string.Format("WaitForIO({0}, {1}) : wait io timeout error occurred. Timeout = {2}ms",
                                    channel, port, timeout));
                            }
                            else
                            {
                                if (value == level)
                                {
                                    runFlag = false;
                                    errorCode = EventCodeEnum.NONE;
                                    LoggerManager.Debug($"WaitForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
                                }
                                else runFlag = true;
                            }
                        }
                        else
                        {
                            if (value == level)
                            {
                                runFlag = false;
                                errorCode = EventCodeEnum.NONE;
                                LoggerManager.Debug($"WaitForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
                            }
                            else runFlag = true;
                        }
                    }
                    else
                    {
                        runFlag = false;

                        LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io error occurred. Timeout = {timeout}ms");
                    }
                    mreUpdateEvent.WaitOne(2);
                } while (runFlag == true);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("WaitForIO({0}, {1}) : wait io error occurred. Timeout = {2}ms, Err = {3}", channel, port, timeout, err.Message));
                LoggerManager.Exception(err);

            }
            finally
            {
                elapsedStopWatch?.Stop();

            }
            return errorCode;
        }

        public EventCodeEnum WriteIO(IOPortDescripter<bool> iodesc, bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
            try
            {
                retVal = Remote.WriteIO(iodesc, value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        
            return retVal;
        }
    }

}
