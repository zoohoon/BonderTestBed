using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using IOMappingsObject;
using ProberErrorCode;
using SystemExceptions.InOutException;

using LogModule;
using System.Runtime.CompilerServices;

namespace IOManagerModule
{
    public class IOManager : IIOManager, INotifyPropertyChanged, IHasSysParameterizable
    {
        //private IParam _SysParam;
        //[ParamIgnore]
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

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private bool _DevConnected;
        public bool DevConnected
        {
            get { return _DevConnected; }
            set { _DevConnected = value; }
        }

        //private IOMappings _IO;
        //public IOMappings IO
        //{
        //    get { return _IO; }
        //}

        private IIOMappingsParameter _IO;
        public IIOMappingsParameter IO
        {
            get { return _IO; }
        }


        private IIOService _IOServ;
        public IIOService IOServ
        {
            get { return _IOServ; }
            set
            {
                if (value != _IOServ)
                {
                    _IOServ = value;
                    RaisePropertyChanged();
                }
            }
        }



        public IIOMappingsParameter IOMapObject { get => _IO; set => _IO = value; }

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



        bool bStopUpdateStateThread = false;
        Thread UpdateStateThread;

        public IOManager()
        {

        }
        public EventCodeEnum InitIOStates()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                retval = DeliveryIOService();

                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"DeliveryIOService() Failed");
                }

                retval = AddIOPortDescripter();

                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"AddIOPortDescripter() Failed");
                }
                Type type = IOMapParam.Outputs.GetType();
                PropertyInfo[] propertyinfos = type.GetProperties();
                IOPortDescripter<bool> portDesc;

                foreach (PropertyInfo property in propertyinfos)
                {
                    try
                    {
                        portDesc = property.GetValue(IOMapParam.Outputs) as IOPortDescripter<bool>;

                        if (portDesc != null)
                        {
                            if (portDesc.IOOveride.Value == EnumIOOverride.NLO)
                            {
                                portDesc.Value = false;
                            }
                            else if (portDesc.IOOveride.Value == EnumIOOverride.NHI)
                            {
                                portDesc.Value = true;
                            }
                            else
                            {
                                if (IOServ.Outputs.Count > 0)
                                {
                                    if (portDesc.ChannelIndex.Value!=1 && portDesc.PortIndex.Value !=-1 && IOServ.Outputs[portDesc.ChannelIndex.Value] != null)
                                    {
                                        if (portDesc.ChannelIndex.Value >= 0 & portDesc.PortIndex.Value >= 0)
                                        {
                                            if (portDesc.ChannelIndex.Value > -1 && portDesc.PortIndex.Value > -1)
                                            {
                                                portDesc.Value = IOServ.Outputs[portDesc.ChannelIndex.Value].Port[portDesc.PortIndex.Value].PortVal;
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                UpdateStateThread = new Thread(new ThreadStart(UpdateStateProc));
                UpdateStateThread.Name = this.GetType().Name;
                bStopUpdateStateThread = false;
                UpdateStateThread.Start();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }
        private EventCodeEnum DeliveryIOService()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Type type = IOMapParam.Outputs.GetType();
                PropertyInfo[] propertyinfos = type.GetProperties();

                foreach (PropertyInfo property in propertyinfos)
                {

                    IOPortDescripter<bool> port = property.GetValue(IOMapParam.Outputs) as IOPortDescripter<bool>;

                    if (port != null)
                        port.SetService(IOServ);
                }

                type = IOMapParam.Inputs.GetType();
                propertyinfos = type.GetProperties();

                foreach (PropertyInfo property in propertyinfos)
                {
                    IOPortDescripter<bool> port = property.GetValue(IOMapParam.Inputs) as IOPortDescripter<bool>;
                    if (port != null)
                        port.SetService(IOServ);
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private HashSet<IOPortDescripter<bool>> OutPorts = new HashSet<IOPortDescripter<bool>>();
        private HashSet<IOPortDescripter<bool>> InPorts = new HashSet<IOPortDescripter<bool>>();
        private HashSet<IOPortDescripter<int>> AIPorts = new HashSet<IOPortDescripter<int>>();
        private EventCodeEnum AddIOPortDescripter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Type type = IOMapParam.Outputs.GetType();
                PropertyInfo[] propertyinfos = type.GetProperties();

                if (IOServ.Outputs.Count > 0)
                {
                    foreach (PropertyInfo property in propertyinfos)
                    {
                        IOPortDescripter<bool> port = property.GetValue(IOMapParam.Outputs) as IOPortDescripter<bool>;

                        try
                        {
                            if (port != null)
                            {
                                OutPorts.Add(port);

                                if (port.ChannelIndex.Value != -1 && port.PortIndex.Value != -1)
                                {
                                    IOServ.Outputs[port.ChannelIndex.Value].Port[port.PortIndex.Value].IOPortList.Add(port);
                                    port.Value = IOServ.Outputs[port.ChannelIndex.Value].Port[port.PortIndex.Value].PortVal;
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                            LoggerManager.Debug($"AddIOPortDescripter(): Outputs adding error. Property = {property.Name}");
                        }
                    }
                }

                if (IOServ.Inputs.Count > 0)
                {
                    type = IOMapParam.Inputs.GetType();
                    propertyinfos = type.GetProperties();

                    foreach (PropertyInfo property in propertyinfos)
                    {

                        IOPortDescripter<bool> port = property.GetValue(IOMapParam.Inputs) as IOPortDescripter<bool>;
                        try
                        {

                            if (port != null)
                            {
                                InPorts.Add(port);
                                if (port.ChannelIndex.Value != -1 && port.PortIndex.Value != -1)
                                {
                                    IOServ.Inputs[port.ChannelIndex.Value].Port[port.PortIndex.Value].IOPortList.Add(port);
                                }
                            }
                            else
                            {
                                List<IOPortDescripter<bool>> ports = property.GetValue(IOMapParam.Inputs) as List<IOPortDescripter<bool>>;

                                if (property.GetValue(IOMapParam.Inputs) is List<IOPortDescripter<bool>>)
                                {
                                    for (int i = 0; i < ports.Count; i++)
                                    {
                                        InPorts.Add(ports[i]);
                                        if (ports[i].ChannelIndex.Value != -1 && ports[i].PortIndex.Value != -1)
                                        {
                                            IOServ.Inputs[ports[i].ChannelIndex.Value].Port[ports[i].PortIndex.Value].IOPortList.Add(port);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                            LoggerManager.Debug($"AddIOPortDescripter(): Inputs adding error. Property = {property.Name}");
                        }
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                throw;
            }

            return retval;
        }
        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

                IOServ.DeInitIO();


                bStopUpdateStateThread = true;
                UpdateStateThread?.Join();
                DevConnected = false;

            }
            catch (Exception err)
            {
                bStopUpdateStateThread = true;
                UpdateStateThread?.Join();
                DevConnected = false;

                //UpdateStateThread.Join();

                LoggerManager.Error($"DeInitIO() Function error: " + err.Message);
                throw new InOutException(string.Format("DeInitIO(): Deinitializing failed."), err);
            }
        }

        public void DeInitService()
        {
            try
            {
                IOMapParam.DeInitService();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        ~IOManager()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void StopUpdate()
        {
            try
            {
                bStopUpdateStateThread = true;
                if (UpdateStateThread != null)
                {
                    UpdateStateThread.Join();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void UpdateStateProc()
        {
            bool tmpPval;
            bool tmpValue;

            try
            {

                IOPortDescripter<bool> portDesc;

                //type = IOMapParam.Outputs.GetType();
                //propertyinfos = type.GetProperties();

                while (bStopUpdateStateThread == false)
                {


                    foreach (var port in OutPorts)
                    {
                        portDesc = port;

                        //if (property.PropertyType == typeof(IOPortDescripter<bool>))
                        //{
                        //    portDesc = property.GetValue(IOMapParam.Outputs) as IOPortDescripter<bool>;
                        //}

                        if (portDesc != null)
                        {
                            if (portDesc.IOOveride.Value == EnumIOOverride.NLO)
                            {
                                portDesc.Value = false;
                            }
                            else if (portDesc.IOOveride.Value == EnumIOOverride.NHI)
                            {
                                portDesc.Value = true;
                            }
                            else
                            {
                                if (portDesc.ChannelIndex.Value > -1 && portDesc.PortIndex.Value > -1)
                                {
                                    if (IOServ.Outputs[portDesc.ChannelIndex.Value] != null)
                                    {
                                        tmpPval = IOServ.Outputs[portDesc.ChannelIndex.Value].Port[portDesc.PortIndex.Value].PortVal;
                                        tmpValue = portDesc.Reverse.Value ? !tmpPval : tmpPval;

                                        portDesc.Value = tmpValue;

                                        //portDesc.Value = IOServ.Inputs[portDesc.ChannelIndex].Port[portDesc.PortIndex].PortVal;
                                    }
                                }
                            }
                        }
                    }

                    //  type = IOMapParam.Inputs.GetType();
                    //  propertyinfos = type.GetProperties();
                    foreach (var port in InPorts)
                    {
                        portDesc = port;
                        //if (property.PropertyType == typeof(IOPortDescripter<bool>))
                        //{
                        //    portDesc = property.GetValue(IOMapParam.Inputs) as IOPortDescripter<bool>;
                        //}
                        if (portDesc != null)
                        {
                            if (portDesc.ChannelIndex.Value == 5)
                            {

                            }

                            if (portDesc.IOOveride.Value == EnumIOOverride.NLO)
                            {
                                portDesc.Value = false;
                            }
                            else if (portDesc.IOOveride.Value == EnumIOOverride.NHI)
                            {
                                portDesc.Value = true;

                            }
                            else
                            {
                                if (portDesc.ChannelIndex.Value > -1 && portDesc.PortIndex.Value > -1)
                                {
                                    if (IOServ.Inputs[portDesc.ChannelIndex.Value] != null)
                                    {
                                        if (portDesc.ChannelIndex.Value == 5)
                                        {

                                        }

                                        //var portVal = IOServ.Inputs[portDesc.ChannelIndex.Value].Port[portDesc.PortIndex.Value].PortVal;
                                        //var value = portDesc.Reverse.Value ? !portVal : portVal;

                                        tmpPval = IOServ.Inputs[portDesc.ChannelIndex.Value].Port[portDesc.PortIndex.Value].PortVal;
                                        tmpValue = portDesc.Reverse.Value ? !tmpPval : tmpPval;

                                        portDesc.Value = tmpValue;

                                        //portDesc.Value = IOServ.Inputs[portDesc.ChannelIndex].Port[portDesc.PortIndex].PortVal;
                                    }
                                }
                            }
                        }
                    }
                    //minskim// GC 호출 및 CPU 사용률 절감을 위해 기존 timer+resetevent로 thread 제어하던 로직을 제거 하고 sleep으로 대체함, sleep시간은 기존 timer interval 주기 값으로 설정함
                    System.Threading.Thread.Sleep(12);
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Error($string.Format("UpdateIOProc(): Error occurred while update io proc. Err = {0}", err.Message));
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    IOServ = new IOServiceProvider.IOService();

                    retval = IOServ.LoadSysParameter();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"IOServ.LoadSysParameter() Failed");
                    }

                    retval = IOServ.InitModule();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"IOServ.InitModule() Failed");
                    }

                    InitIOStates();

                    Initialized = true;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public int WriteBit(IOPortDescripter<bool> portDesc, bool value)
        {
            int retVal = -1;
            try
            {
                if (portDesc.IOType.Value == EnumIOType.OUTPUT)
                {
                    retVal = (int)IOServ.WriteBit(portDesc, value);
                    portDesc.Value = value;
                }
            }
            catch (Exception err)
            {
                retVal = -1;
                //LoggerManager.Error($string.Format("WriteBit: Error occurred while update io proc. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public int ReadBit(IOPortDescripter<bool> portDesc, out bool value)
        {
            int retVal = -1;
            value = false;
            try
            {
                if (portDesc.IOType.Value == EnumIOType.INPUT || portDesc.IOType.Value == EnumIOType.OUTPUT)
                {
                    retVal = (int)IOServ.ReadBit(portDesc, out value);
                    portDesc.Value = value;
                }
            }
            catch (Exception err)
            {
                retVal = -1;
                //LoggerManager.Error($string.Format("ReadBit: Error occurred while update io proc. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DefaultParameterSet()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }


        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new IOMappings();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(IOMappings), null, null, Extensions_IParam.FileType.XML);

                if (RetVal == EventCodeEnum.NONE)
                {
                    IOMapParam = tmpParam as IOMappings;
                    IOMapObject = IOMapParam;
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

        public EventCodeEnum LoadParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                RetVal = LoadSysParameter();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = SaveSysParameter();
                if (RetVal != EventCodeEnum.NONE)
                {
                    RetVal = IOServ.SaveSysParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
    }
}
