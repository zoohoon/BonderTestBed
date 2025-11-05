using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Serialization;
using System.Collections;
using System.Linq;

namespace FoupModules
{

    public class FoupIOStates : IFoupIOStates, INotifyPropertyChanged, IDisposable
    {
        public bool Initialized { get; set; } = false;
        private bool IsDisposed = false;
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

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        AutoResetEvent areUpdateEvent = new AutoResetEvent(false);

        private bool _DevConnected;
        public bool DevConnected
        {
            get { return _DevConnected; }
            set { _DevConnected = value; }
        }

        System.Timers.Timer _monitoringTimer;
        private FoupIOMappings _FoupIOMap;
        public FoupIOMappings IOMap
        {
            get { return _FoupIOMap; }

        }

        private IIOService _IOServ;

        public IIOService IOServ
        {
            get { return this.IOManager().IOServ; }
            set { _IOServ = value; }
        }

        bool bStopUpdateStateThread = false;
        Thread UpdateStateThread;

        //public static readonly string IOMapParamPath = @"C:\ProberSystem\Parameters\Foup\FoupIOMapParam.xml";
        //public static readonly string IODevicePath = @"C:\ProberSystem\Parameters\Foup\FoupIODeviceMapping.xml";
        //public static readonly string EcatIOParamPath = @"C:\ProberSystem\Parameters\Foup\FoupECATIOSetting.xml";

        public FoupIOStates()
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

                _monitoringTimer = new System.Timers.Timer(12);
                _monitoringTimer.Elapsed += _monitoringTimer_Elapsed;

                Type type = _FoupIOMap.Outputs.GetType();
                PropertyInfo[] propertyinfos = type.GetProperties();
                IOPortDescripter<bool> portDesc;
                List<IOPortDescripter<bool>> listPortDesc;

                foreach (PropertyInfo property in propertyinfos)
                {
                    portDesc = property.GetValue(_FoupIOMap.Outputs) as IOPortDescripter<bool>;

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
                            OutPorts.Add(portDesc);
                            if (portDesc.ChannelIndex.Value > -1
                                && portDesc.PortIndex.Value > -1
                                && IOServ.Outputs.Count > portDesc.PortIndex.Value)
                            {
                                if (IOServ.Outputs[portDesc.ChannelIndex.Value] != null)
                                {

                                    portDesc.Value = IOServ.Outputs[portDesc.ChannelIndex.Value].Port[portDesc.PortIndex.Value].PortVal;
                                }
                            }
                        }
                    }
                }

                type = _FoupIOMap.Inputs.GetType();
                propertyinfos = type.GetProperties();

                foreach (PropertyInfo property in propertyinfos)
                {
                    IOPortDescripter<bool> port = property.GetValue(_FoupIOMap.Inputs) as IOPortDescripter<bool>;

                    if (port != null)
                    {
                        InPorts.Add(port);
                    }

                    if (property.PropertyType.IsGenericType &&
                      property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        listPortDesc = property.GetValue(_FoupIOMap.Inputs) as List<IOPortDescripter<bool>>;
                        ListInPorts.Add(listPortDesc);
                    }
                }

                _monitoringTimer.Start();

                UpdateStateThread = new Thread(new ThreadStart(UpdateStateProc));
                UpdateStateThread.Name = this.GetType().Name;
                bStopUpdateStateThread = false;
                UpdateStateThread.Start();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private EventCodeEnum DeliveryIOService()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Type type = IOMap.Outputs.GetType();
                PropertyInfo[] propertyinfos = type.GetProperties();

                foreach (PropertyInfo property in propertyinfos)
                {
                    IOPortDescripter<bool> port = property.GetValue(_FoupIOMap.Outputs) as IOPortDescripter<bool>;
                    Object nodeInstance = null;
                    if (port != null)
                    {
                        port.SetService(IOServ);
                    }else
                    {
                        nodeInstance = property.GetValue(_FoupIOMap.Outputs);
                        if(nodeInstance is IList)
                        {
                            Type genericArgType = nodeInstance.GetType().GenericTypeArguments.FirstOrDefault();
                            if (typeof(IOPortDescripter<bool>).IsAssignableFrom(genericArgType))
                            {
                                IList list = null;
                                list = nodeInstance as IList;
                                for (int index = 0; index < list.Count; index++)
                                {
                                    if (list[index] is IOPortDescripter<bool>)
                                    {
                                        IOPortDescripter<bool> elem = list[index] as IOPortDescripter<bool>;
                                        elem.SetService(IOServ);
                                    }

                                }

                            }
                        }
                    }
                }

                type = _FoupIOMap.Inputs.GetType();
                propertyinfos = type.GetProperties();
                foreach (PropertyInfo property in propertyinfos)
                {
                    IOPortDescripter<bool> port = property.GetValue(_FoupIOMap.Inputs) as IOPortDescripter<bool>;
                    Object nodeInstance = null;
                    if (port != null)
                    {
                        port.SetService(IOServ);
                    }
                    else
                    {
                        nodeInstance = property.GetValue(_FoupIOMap.Inputs);
                        if (nodeInstance is IList)
                        {
                            Type genericArgType = nodeInstance.GetType().GenericTypeArguments.FirstOrDefault();
                            if (typeof(IOPortDescripter<bool>).IsAssignableFrom(genericArgType))
                            {
                                IList list = null;
                                list = nodeInstance as IList;
                                for (int index = 0; index < list.Count; index++)
                                {
                                    if (list[index] is IOPortDescripter<bool>)
                                    {
                                        IOPortDescripter<bool> elem = list[index] as IOPortDescripter<bool>;
                                        elem.SetService(IOServ);
                                    }

                                }

                            }
                        }
                    }
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
        private HashSet<List<IOPortDescripter<bool>>> ListInPorts = new HashSet<List<IOPortDescripter<bool>>>();
        public int DeInitIOStates()
        {
            int retVal = -1;

            try
            {
                IOServ.DeInitIO();

                //if (UpdateStateThread != null)
                //{
                //    bStopUpdateStateThread = true;
                //    UpdateStateThread.Join();
                //}

                bStopUpdateStateThread = true;
                UpdateStateThread?.Join();
                DevConnected = false;

                _monitoringTimer?.Stop();
                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"DeInitIO() Function error: " + err.Message);

                bStopUpdateStateThread = true;
                UpdateStateThread?.Join();
                DevConnected = false;

                //UpdateStateThread.Join();

                throw new IOException(string.Format("DeInitIO(): Deinitializing failed."), err);
            }
            return retVal;
        }
        ~FoupIOStates()
        {
            _monitoringTimer?.Stop();
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
        private void _monitoringTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!areUpdateEvent.SafeWaitHandle.IsClosed)
                areUpdateEvent.Set();
        }
        private void UpdateStateProc()
        {
            try
            {
                IOPortDescripter<bool> portDesc;
                while (bStopUpdateStateThread == false)
                {
                    foreach (var port in OutPorts)
                    {
                        portDesc = port;

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
                        }
                    }

                    foreach (var port in InPorts)
                    {
                        portDesc = port;

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
                                    if (IOServ.Inputs[portDesc.ChannelIndex.Value] != null)
                                    {
                                        if (!port.ForcedIO.IsForced)
                                        {
                                            var portVal = IOServ.Inputs[portDesc.ChannelIndex.Value].Port[portDesc.PortIndex.Value].PortVal;
                                            var value = portDesc.Reverse.Value ? !portVal : portVal;

                                            portDesc.Value = value;
                                        }
                                        else
                                        {
                                            var value = port.ForcedIO.ForecedValue;
                                            port.Value = value;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (var listPort in ListInPorts)
                    {
                        if (listPort != null)
                        {
                            foreach (var port in listPort)
                            {
                                if (port.IOOveride.Value == EnumIOOverride.NLO)
                                {
                                    port.Value = false;
                                }
                                else if (port.IOOveride.Value == EnumIOOverride.NHI)
                                {
                                    port.Value = true;
                                }
                                else
                                {
                                    if (port.ChannelIndex.Value > -1 && port.PortIndex.Value > -1)
                                    {
                                        if (IOServ.Inputs[port.ChannelIndex.Value] != null)
                                        {
                                            if(IOServ.Inputs.Count > port.ChannelIndex.Value)
                                            {
                                                if(IOServ.Inputs[port.ChannelIndex.Value].Port.Count > port.PortIndex.Value)
                                                {
                                                    if(!port.ForcedIO.IsForced)
                                                    {
                                                        var portVal = IOServ.Inputs[port.ChannelIndex.Value].Port[port.PortIndex.Value].PortVal;
                                                        var value = port.Reverse.Value ? !portVal : portVal;

                                                        port.Value = value;
                                                    }
                                                    else
                                                    {
                                                        var value = port.ForcedIO.ForecedValue;
                                                        port.Value = value;
                                                    }
                                                }
                                                else
                                                {
                                                    //LoggerManager.Error($"[FoupIOStates] UpdateStateProc() : ChannelIndex = {port.ChannelIndex.Value}, PortIndex = {port.PortIndex.Value}, Port count = {IOServ.Inputs[port.ChannelIndex.Value].Port.Count}");
                                                }
                                            }
                                            else
                                            {
                                                //LoggerManager.Error($"[FoupIOStates] UpdateStateProc() : ChannelIndex = {port.ChannelIndex.Value}, PortIndex = {port.PortIndex.Value}, Input count = {IOServ.Inputs.Count}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    areUpdateEvent.WaitOne(100);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
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
                    //IOServ = this.IOManager().IOServ;
                    retval = InitIOStates();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"InitIOStates() Failed");
                    }

                    IsDisposed = false;
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
        public int WaitForIO(IOPortDescripter<bool> io, bool level, long timeout = 0)
        {
            return IOServ.WaitForIO(io, level, timeout);
        }
        public int MonitorForIO(IOPortDescripter<bool> io, bool level, long maintainTime = 0, long timeout = 0)
        {
            return IOServ.MonitorForIO(io, level, maintainTime, timeout);
        }

        public EventCodeEnum LoadFoupIOMapDefinitions()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                _FoupIOMap = new FoupIOMappings();

                string FullPath = this.FileManager().GetSystemParamFullPath(_FoupIOMap.FilePath, _FoupIOMap.FileName);

                try
                {
                    IParam tmpParam = null;
                    RetVal = this.LoadParameter(ref tmpParam, typeof(FoupIOMappings), null, FullPath);
                    if (RetVal == EventCodeEnum.NONE)
                    {
                        _FoupIOMap = tmpParam as FoupIOMappings;
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($String.Format("LoadFoupIOMapDefinitions(): Error occurred while loading parameters. Err = {0}", err.Message));
                    LoggerManager.Exception(err);

                    throw;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveFoupIOMapDefinitions()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                string FullPath = this.FileManager().GetSystemParamFullPath(_FoupIOMap.FilePath, _FoupIOMap.FileName);

                try
                {
                    if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
                    }

                    RetVal = Extensions_IParam.SaveParameter(null, _FoupIOMap, null, FullPath);
                    if (RetVal != EventCodeEnum.NONE)
                    {
                        throw new Exception();
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($String.Format("SaveFoupIOMapDefinitions(): Error occurred while loading parameters. Err = {0}", err.Message));
                    LoggerManager.Exception(err);

                    throw;
                }

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
                tmpParam = new FoupIOMappings();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(FoupIOMappings));

                if (RetVal == EventCodeEnum.NONE)
                {
                    _FoupIOMap = tmpParam as FoupIOMappings;
                    //SetFoupDefault_GPIO();
                    //SaveSysParameter();
                    SetFoupAlias();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SetFoupDefault_GPIO()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            try
            {
                if(SystemModuleCount.ModuleCnt.FoupCount == 1)
                {
                    if (_FoupIOMap.Inputs.DI_COVER_OPENs == null || _FoupIOMap.Inputs.DI_COVER_OPENs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_OPENs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_OPENs.Add(new IOPortDescripter<bool>($"DI_COVER_OPENs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].PortIndex.Value = 12 + i;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_CLOSEs == null || _FoupIOMap.Inputs.DI_COVER_CLOSEs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_CLOSEs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs.Add(new IOPortDescripter<bool>($"DI_COVER_CLOSEs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].PortIndex.Value = 15 + i;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_LOCK12s == null || _FoupIOMap.Inputs.DI_CST_LOCK12s.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_LOCK12s = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_LOCK12s.Add(new IOPortDescripter<bool>($"DI_CST_LOCK12s.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].PortIndex.Value = 0 + i;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_UNLOCK12s == null || _FoupIOMap.Inputs.DI_CST_UNLOCK12s.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_UNLOCK12s = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s.Add(new IOPortDescripter<bool>($"DI_CST_UNLOCK12s.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].PortIndex.Value = 3 + i;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST12_PRESs == null || _FoupIOMap.Inputs.DI_CST12_PRESs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST12_PRESs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST12_PRESs.Add(new IOPortDescripter<bool>($"DI_CST12_PRESs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].ChannelIndex.Value = 3;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].PortIndex.Value = 3 + (i * 2);
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST12_PRES2s == null || _FoupIOMap.Inputs.DI_CST12_PRES2s.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST12_PRES2s = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST12_PRES2s.Add(new IOPortDescripter<bool>($"DI_CST12_PRES2s.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].ChannelIndex.Value = 3;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].PortIndex.Value = 4 + (i * 2);
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_DP_INs == null || _FoupIOMap.Inputs.DI_DP_INs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_DP_INs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_DP_INs.Add(new IOPortDescripter<bool>($"DI_DP_INs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_DP_INs[i].ChannelIndex.Value = 1;
                            _FoupIOMap.Inputs.DI_DP_INs[i].PortIndex.Value = 18 + i;
                            _FoupIOMap.Inputs.DI_DP_INs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_DP_INs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_DP_INs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_DP_INs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_DP_OUTs == null || _FoupIOMap.Inputs.DI_DP_OUTs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_DP_OUTs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_DP_OUTs.Add(new IOPortDescripter<bool>($"DI_DP_OUTs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].ChannelIndex.Value = 1;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].PortIndex.Value = 21 + i;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_LOCKs == null || _FoupIOMap.Inputs.DI_COVER_LOCKs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_LOCKs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_LOCKs.Add(new IOPortDescripter<bool>($"DI_COVER_LOCKs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].ChannelIndex.Value = 1 + i;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].PortIndex.Value = 30;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_UNLOCKs == null || _FoupIOMap.Inputs.DI_COVER_UNLOCKs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_UNLOCKs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs.Add(new IOPortDescripter<bool>($"DI_COVER_UNLOCKs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].ChannelIndex.Value = 1 + i;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].PortIndex.Value = 31;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_CoverVacuums == null || _FoupIOMap.Inputs.DI_CST_CoverVacuums.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_CoverVacuums = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums.Add(new IOPortDescripter<bool>($"DI_CST_CoverVacuum.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].ChannelIndex.Value = 5;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].PortIndex.Value = 0 + i;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_MappingOuts == null || _FoupIOMap.Inputs.DI_CST_MappingOuts.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_MappingOuts = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_MappingOuts.Add(new IOPortDescripter<bool>($"DI_CST_MappingOut.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].ChannelIndex.Value = 4;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].PortIndex.Value = 24 + i;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_UPs == null || _FoupIOMap.Inputs.DI_COVER_UPs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_UPs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_UPs.Add(new IOPortDescripter<bool>($"DI_COVER_UPs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].PortIndex.Value = 12 + i;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_DOWNs == null || _FoupIOMap.Inputs.DI_COVER_DOWNs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_DOWNs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_DOWNs.Add(new IOPortDescripter<bool>($"DI_COVER_DOWNs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].PortIndex.Value = 15 + i;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].TimeOut.Value = 500;
                        }
                    }
                }
                else if (SystemModuleCount.ModuleCnt.FoupCount == 2)
                {
                    if(_FoupIOMap.Inputs.DI_COVER_OPENs==null || _FoupIOMap.Inputs.DI_COVER_OPENs.Count==0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_OPENs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_OPENs.Add(new IOPortDescripter<bool>($"DI_COVER_OPENs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].PortIndex.Value = 12 + i;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_CLOSEs == null || _FoupIOMap.Inputs.DI_COVER_CLOSEs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_CLOSEs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs.Add(new IOPortDescripter<bool>($"DI_COVER_CLOSEs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].PortIndex.Value = 15 + i;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_LOCK12s == null || _FoupIOMap.Inputs.DI_CST_LOCK12s.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_LOCK12s = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_LOCK12s.Add(new IOPortDescripter<bool>($"DI_CST_LOCK12s.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].PortIndex.Value = 0 + i;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_UNLOCK12s == null || _FoupIOMap.Inputs.DI_CST_UNLOCK12s.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_UNLOCK12s = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s.Add(new IOPortDescripter<bool>($"DI_CST_UNLOCK12s.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].PortIndex.Value = 3 + i;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST12_PRESs == null || _FoupIOMap.Inputs.DI_CST12_PRESs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST12_PRESs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST12_PRESs.Add(new IOPortDescripter<bool>($"DI_CST12_PRESs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].ChannelIndex.Value = 3;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].PortIndex.Value = 3 + (i * 2);
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST12_PRES2s == null || _FoupIOMap.Inputs.DI_CST12_PRES2s.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST12_PRES2s = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST12_PRES2s.Add(new IOPortDescripter<bool>($"DI_CST12_PRES2s.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].ChannelIndex.Value = 3;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].PortIndex.Value = 4 + (i * 2);
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_DP_INs == null || _FoupIOMap.Inputs.DI_DP_INs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_DP_INs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_DP_INs.Add(new IOPortDescripter<bool>($"DI_DP_INs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_DP_INs[i].ChannelIndex.Value = 1;
                            _FoupIOMap.Inputs.DI_DP_INs[i].PortIndex.Value = 18 + i;
                            _FoupIOMap.Inputs.DI_DP_INs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_DP_INs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_DP_INs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_DP_INs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_DP_OUTs == null || _FoupIOMap.Inputs.DI_DP_OUTs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_DP_OUTs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_DP_OUTs.Add(new IOPortDescripter<bool>($"DI_DP_OUTs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].ChannelIndex.Value = 1;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].PortIndex.Value = 21 + i;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_LOCKs == null || _FoupIOMap.Inputs.DI_COVER_LOCKs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_LOCKs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_LOCKs.Add(new IOPortDescripter<bool>($"DI_COVER_LOCKs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].ChannelIndex.Value = 1 + i;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].PortIndex.Value = 30;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_UNLOCKs == null || _FoupIOMap.Inputs.DI_COVER_UNLOCKs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_UNLOCKs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs.Add(new IOPortDescripter<bool>($"DI_COVER_UNLOCKs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].ChannelIndex.Value = 1 + i;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].PortIndex.Value = 31;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_CoverVacuums == null || _FoupIOMap.Inputs.DI_CST_CoverVacuums.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_CoverVacuums = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums.Add(new IOPortDescripter<bool>($"DI_CST_CoverVacuum.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].ChannelIndex.Value = 3;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].PortIndex.Value = 0 + i;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_MappingOuts == null || _FoupIOMap.Inputs.DI_CST_MappingOuts.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_MappingOuts = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_MappingOuts.Add(new IOPortDescripter<bool>($"DI_CST_MappingOut.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].PortIndex.Value = 24 + i;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_UPs == null || _FoupIOMap.Inputs.DI_COVER_UPs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_UPs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_UPs.Add(new IOPortDescripter<bool>($"DI_COVER_UPs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].PortIndex.Value = 12 + i;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_DOWNs == null || _FoupIOMap.Inputs.DI_COVER_DOWNs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_DOWNs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_DOWNs.Add(new IOPortDescripter<bool>($"DI_COVER_DOWNs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].PortIndex.Value = 15 + i;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].TimeOut.Value = 500;
                        }
                    }
                }
                else if(SystemModuleCount.ModuleCnt.FoupCount == 3)
                {
                    if (_FoupIOMap.Inputs.DI_COVER_OPENs == null || _FoupIOMap.Inputs.DI_COVER_OPENs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_OPENs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_OPENs.Add(new IOPortDescripter<bool>($"DI_COVER_OPENs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].PortIndex.Value = 12 + i;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_CLOSEs == null || _FoupIOMap.Inputs.DI_COVER_CLOSEs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_CLOSEs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs.Add(new IOPortDescripter<bool>($"DI_COVER_CLOSEs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].PortIndex.Value = 15 + i;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_LOCK12s == null || _FoupIOMap.Inputs.DI_CST_LOCK12s.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_LOCK12s = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_LOCK12s.Add(new IOPortDescripter<bool>($"DI_CST_LOCK12s.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].PortIndex.Value = 0 + i;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_UNLOCK12s == null || _FoupIOMap.Inputs.DI_CST_UNLOCK12s.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_UNLOCK12s = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s.Add(new IOPortDescripter<bool>($"DI_CST_UNLOCK12s.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].PortIndex.Value = 3 + i;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST12_PRESs == null || _FoupIOMap.Inputs.DI_CST12_PRESs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST12_PRESs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST12_PRESs.Add(new IOPortDescripter<bool>($"DI_CST12_PRESs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].ChannelIndex.Value = 3;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].PortIndex.Value = 3 + (i * 2);
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST12_PRES2s == null || _FoupIOMap.Inputs.DI_CST12_PRES2s.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST12_PRES2s = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST12_PRES2s.Add(new IOPortDescripter<bool>($"DI_CST12_PRES2s.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].ChannelIndex.Value = 3;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].PortIndex.Value = 4 + (i * 2);
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_DP_INs == null || _FoupIOMap.Inputs.DI_DP_INs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_DP_INs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_DP_INs.Add(new IOPortDescripter<bool>($"DI_DP_INs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_DP_INs[i].ChannelIndex.Value = 1;
                            _FoupIOMap.Inputs.DI_DP_INs[i].PortIndex.Value = 18 + i;
                            _FoupIOMap.Inputs.DI_DP_INs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_DP_INs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_DP_INs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_DP_INs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_DP_OUTs == null || _FoupIOMap.Inputs.DI_DP_OUTs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_DP_OUTs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_DP_OUTs.Add(new IOPortDescripter<bool>($"DI_DP_OUTs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].ChannelIndex.Value = 1;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].PortIndex.Value = 21 + i;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_LOCKs == null || _FoupIOMap.Inputs.DI_COVER_LOCKs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_LOCKs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_LOCKs.Add(new IOPortDescripter<bool>($"DI_COVER_LOCKs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].ChannelIndex.Value = 1 + i;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].PortIndex.Value = 30;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_UNLOCKs == null || _FoupIOMap.Inputs.DI_COVER_UNLOCKs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_UNLOCKs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs.Add(new IOPortDescripter<bool>($"DI_COVER_UNLOCKs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].ChannelIndex.Value = 1 + i;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].PortIndex.Value = 31;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_CoverVacuums == null || _FoupIOMap.Inputs.DI_CST_CoverVacuums.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_CoverVacuums = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums.Add(new IOPortDescripter<bool>($"DI_CST_CoverVacuum.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].ChannelIndex.Value = 5;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].PortIndex.Value = 0 + i;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_MappingOuts == null || _FoupIOMap.Inputs.DI_CST_MappingOuts.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_MappingOuts = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_MappingOuts.Add(new IOPortDescripter<bool>($"DI_CST_MappingOut.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].ChannelIndex.Value = 4;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].PortIndex.Value = 24 + i;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_UPs == null || _FoupIOMap.Inputs.DI_COVER_UPs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_UPs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_UPs.Add(new IOPortDescripter<bool>($"DI_COVER_UPs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].PortIndex.Value = 12 + i;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_DOWNs == null || _FoupIOMap.Inputs.DI_COVER_DOWNs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_DOWNs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_DOWNs.Add(new IOPortDescripter<bool>($"DI_COVER_DOWNs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].ChannelIndex.Value = 2;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].PortIndex.Value = 15 + i;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].TimeOut.Value = 500;
                        }
                    }
                
                }
                else if(SystemModuleCount.ModuleCnt.FoupCount == 4)
                {
                    if (_FoupIOMap.Inputs.DI_COVER_OPENs == null || _FoupIOMap.Inputs.DI_COVER_OPENs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_OPENs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_OPENs.Add(new IOPortDescripter<bool>($"DI_COVER_OPENs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].ChannelIndex.Value = i+5;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].PortIndex.Value = 8;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_OPENs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_CLOSEs == null || _FoupIOMap.Inputs.DI_COVER_CLOSEs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_CLOSEs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs.Add(new IOPortDescripter<bool>($"DI_COVER_CLOSEs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].ChannelIndex.Value = i+5;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].PortIndex.Value = 9;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_LOCK12s == null || _FoupIOMap.Inputs.DI_CST_LOCK12s.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_LOCK12s = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_LOCK12s.Add(new IOPortDescripter<bool>($"DI_CST_LOCK12s.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].ChannelIndex.Value = i + 5;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].PortIndex.Value =4;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_LOCK12s[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_UNLOCK12s == null || _FoupIOMap.Inputs.DI_CST_UNLOCK12s.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_UNLOCK12s = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s.Add(new IOPortDescripter<bool>($"DI_CST_UNLOCK12s.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].ChannelIndex.Value = i + 5;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].PortIndex.Value = 5;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST12_PRESs == null || _FoupIOMap.Inputs.DI_CST12_PRESs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST12_PRESs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST12_PRESs.Add(new IOPortDescripter<bool>($"DI_CST12_PRESs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].ChannelIndex.Value = i + 5;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].PortIndex.Value = 15;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST12_PRESs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST12_PRES2s == null || _FoupIOMap.Inputs.DI_CST12_PRES2s.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST12_PRES2s = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST12_PRES2s.Add(new IOPortDescripter<bool>($"DI_CST12_PRES2s.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].ChannelIndex.Value = i + 5;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].PortIndex.Value = 16;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST12_PRES2s[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_DP_INs == null || _FoupIOMap.Inputs.DI_DP_INs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_DP_INs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_DP_INs.Add(new IOPortDescripter<bool>($"DI_DP_INs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_DP_INs[i].ChannelIndex.Value = i + 5;
                            _FoupIOMap.Inputs.DI_DP_INs[i].PortIndex.Value = 0;
                            _FoupIOMap.Inputs.DI_DP_INs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_DP_INs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_DP_INs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_DP_INs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_DP_OUTs == null || _FoupIOMap.Inputs.DI_DP_OUTs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_DP_OUTs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_DP_OUTs.Add(new IOPortDescripter<bool>($"DI_DP_OUTs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].ChannelIndex.Value = i + 5;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].PortIndex.Value = 1;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_DP_OUTs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_LOCKs == null || _FoupIOMap.Inputs.DI_COVER_LOCKs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_LOCKs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_LOCKs.Add(new IOPortDescripter<bool>($"DI_COVER_LOCKs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].ChannelIndex.Value = i + 5;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].PortIndex.Value = 24;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_LOCKs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_UNLOCKs == null || _FoupIOMap.Inputs.DI_COVER_UNLOCKs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_UNLOCKs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs.Add(new IOPortDescripter<bool>($"DI_COVER_UNLOCKs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].ChannelIndex.Value = i + 5;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].PortIndex.Value = 25;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_CoverVacuums == null || _FoupIOMap.Inputs.DI_CST_CoverVacuums.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_CoverVacuums = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums.Add(new IOPortDescripter<bool>($"DI_CST_CoverVacuum.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].ChannelIndex.Value = i + 5;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].PortIndex.Value = 14;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_CST_MappingOuts == null || _FoupIOMap.Inputs.DI_CST_MappingOuts.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_CST_MappingOuts = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_CST_MappingOuts.Add(new IOPortDescripter<bool>($"DI_CST_MappingOut.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].ChannelIndex.Value = i + 5;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].PortIndex.Value = 12;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_CST_MappingOuts[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_UPs == null || _FoupIOMap.Inputs.DI_COVER_UPs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_UPs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_UPs.Add(new IOPortDescripter<bool>($"DI_COVER_UPs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].ChannelIndex.Value = i + 5;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].PortIndex.Value = 8;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_UPs[i].TimeOut.Value = 500;
                        }
                    }
                    if (_FoupIOMap.Inputs.DI_COVER_DOWNs == null || _FoupIOMap.Inputs.DI_COVER_DOWNs.Count == 0)
                    {
                        _FoupIOMap.Inputs.DI_COVER_DOWNs = new List<IOPortDescripter<bool>>();
                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            _FoupIOMap.Inputs.DI_COVER_DOWNs.Add(new IOPortDescripter<bool>($"DI_COVER_DOWNs.{i}", EnumIOType.INPUT));
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].ChannelIndex.Value = i + 5;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].PortIndex.Value = 9;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].Reverse.Value = false;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].IOOveride.Value = EnumIOOverride.NONE;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].MaintainTime.Value = 100;
                            _FoupIOMap.Inputs.DI_COVER_DOWNs[i].TimeOut.Value = 500;
                        }
                    }
                }

                #region Output
                if (_FoupIOMap.Outputs.DO_CST_12INCH_LOCKs == null || _FoupIOMap.Outputs.DO_CST_12INCH_LOCKs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_CST_12INCH_LOCKs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_CST_12INCH_LOCKs.Add(new IOPortDescripter<bool>($"DO_CST_12INCH_LOCKs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_CST_12INCH_LOCKs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_12INCH_LOCKs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_12INCH_LOCKs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_CST_12INCH_LOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_CST_12INCH_LOCKs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_CST_12INCH_LOCKs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_CST_12INCH_UNLOCKs == null || _FoupIOMap.Outputs.DO_CST_12INCH_UNLOCKs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_CST_12INCH_UNLOCKs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_CST_12INCH_UNLOCKs.Add(new IOPortDescripter<bool>($"DO_CST_12INCH_UNLOCKs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_CST_12INCH_UNLOCKs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_12INCH_UNLOCKs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_12INCH_UNLOCKs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_CST_12INCH_UNLOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_CST_12INCH_UNLOCKs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_CST_12INCH_UNLOCKs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_CST_LOADs == null || _FoupIOMap.Outputs.DO_CST_LOADs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_CST_LOADs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_CST_LOADs.Add(new IOPortDescripter<bool>($"DO_CST_LOADs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_CST_LOADs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_LOADs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_LOADs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_CST_LOADs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_CST_LOADs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_CST_LOADs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_CST_UNLOADs == null || _FoupIOMap.Outputs.DO_CST_UNLOADs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_CST_UNLOADs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_CST_UNLOADs.Add(new IOPortDescripter<bool>($"DO_CST_UNLOADs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_CST_UNLOADs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_UNLOADs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_UNLOADs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_CST_UNLOADs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_CST_UNLOADs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_CST_UNLOADs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_CST_VACUUMs == null || _FoupIOMap.Outputs.DO_CST_VACUUMs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_CST_VACUUMs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_CST_VACUUMs.Add(new IOPortDescripter<bool>($"DO_CST_VACUUMs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_CST_VACUUMs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_VACUUMs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_VACUUMs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_CST_VACUUMs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_CST_VACUUMs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_CST_VACUUMs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_COVER_LOCKs == null || _FoupIOMap.Outputs.DO_COVER_LOCKs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_COVER_LOCKs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_COVER_LOCKs.Add(new IOPortDescripter<bool>($"DO_COVER_LOCKs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_COVER_LOCKs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_COVER_LOCKs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_COVER_LOCKs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_COVER_LOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_COVER_LOCKs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_COVER_LOCKs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_COVER_UNLOCKs == null || _FoupIOMap.Outputs.DO_COVER_UNLOCKs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_COVER_UNLOCKs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_COVER_UNLOCKs.Add(new IOPortDescripter<bool>($"DO_COVER_UNLOCKs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_COVER_UNLOCKs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_COVER_UNLOCKs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_COVER_UNLOCKs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_COVER_UNLOCKs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_COVER_UNLOCKs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_COVER_UNLOCKs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_COVER_OPENs == null || _FoupIOMap.Outputs.DO_COVER_OPENs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_COVER_OPENs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_COVER_OPENs.Add(new IOPortDescripter<bool>($"DO_COVER_OPENs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_COVER_OPENs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_COVER_OPENs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_COVER_OPENs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_COVER_OPENs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_COVER_OPENs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_COVER_OPENs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_COVER_Closes == null || _FoupIOMap.Outputs.DO_COVER_Closes.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_COVER_Closes = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_COVER_Closes.Add(new IOPortDescripter<bool>($"DO_COVER_Closes.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_COVER_Closes[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_COVER_Closes[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_COVER_Closes[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_COVER_Closes[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_COVER_Closes[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_COVER_Closes[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_CST_MAPPINGs == null || _FoupIOMap.Outputs.DO_CST_MAPPINGs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_CST_MAPPINGs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_CST_MAPPINGs.Add(new IOPortDescripter<bool>($"DO_COVER_Closes.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_CST_MAPPINGs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_MAPPINGs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_MAPPINGs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_CST_MAPPINGs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_CST_MAPPINGs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_CST_MAPPINGs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_CST_IND_ALARMs == null || _FoupIOMap.Outputs.DO_CST_IND_ALARMs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_CST_IND_ALARMs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_CST_IND_ALARMs.Add(new IOPortDescripter<bool>($"DO_CST_IND_ALARMs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_CST_IND_ALARMs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_ALARMs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_ALARMs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_CST_IND_ALARMs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_CST_IND_ALARMs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_CST_IND_ALARMs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_CST_IND_BUSYs == null || _FoupIOMap.Outputs.DO_CST_IND_BUSYs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_CST_IND_BUSYs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_CST_IND_BUSYs.Add(new IOPortDescripter<bool>($"DO_CST_IND_BUSYs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_CST_IND_BUSYs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_BUSYs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_BUSYs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_CST_IND_BUSYs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_CST_IND_BUSYs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_CST_IND_BUSYs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_CST_IND_RESERVEDs == null || _FoupIOMap.Outputs.DO_CST_IND_RESERVEDs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_CST_IND_RESERVEDs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_CST_IND_RESERVEDs.Add(new IOPortDescripter<bool>($"DO_CST_IND_RESERVEDs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_CST_IND_RESERVEDs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_RESERVEDs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_RESERVEDs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_CST_IND_RESERVEDs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_CST_IND_RESERVEDs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_CST_IND_RESERVEDs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_CST_IND_AUTOs == null || _FoupIOMap.Outputs.DO_CST_IND_AUTOs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_CST_IND_AUTOs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_CST_IND_AUTOs.Add(new IOPortDescripter<bool>($"DO_CST_IND_AUTOs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_CST_IND_AUTOs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_AUTOs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_AUTOs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_CST_IND_AUTOs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_CST_IND_AUTOs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_CST_IND_AUTOs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_CST_IND_LOADs == null || _FoupIOMap.Outputs.DO_CST_IND_LOADs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_CST_IND_LOADs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_CST_IND_LOADs.Add(new IOPortDescripter<bool>($"DO_CST_IND_LOADs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_CST_IND_LOADs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_LOADs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_LOADs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_CST_IND_LOADs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_CST_IND_LOADs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_CST_IND_LOADs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_CST_IND_UNLOADs == null || _FoupIOMap.Outputs.DO_CST_IND_UNLOADs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_CST_IND_UNLOADs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_CST_IND_UNLOADs.Add(new IOPortDescripter<bool>($"DO_CST_IND_UNLOADs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_CST_IND_UNLOADs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_UNLOADs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_UNLOADs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_CST_IND_UNLOADs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_CST_IND_UNLOADs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_CST_IND_UNLOADs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_CST_IND_PLACEMENTs == null || _FoupIOMap.Outputs.DO_CST_IND_PLACEMENTs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_CST_IND_PLACEMENTs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_CST_IND_PLACEMENTs.Add(new IOPortDescripter<bool>($"DO_CST_IND_PLACEMENTs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_CST_IND_PLACEMENTs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_PLACEMENTs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_PLACEMENTs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_CST_IND_PLACEMENTs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_CST_IND_PLACEMENTs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_CST_IND_PLACEMENTs[i].TimeOut.Value = 500;
                    }
                }
                if (_FoupIOMap.Outputs.DO_CST_IND_PRESENCEs == null || _FoupIOMap.Outputs.DO_CST_IND_PRESENCEs.Count == 0)
                {
                    _FoupIOMap.Outputs.DO_CST_IND_PRESENCEs = new List<IOPortDescripter<bool>>();
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        _FoupIOMap.Outputs.DO_CST_IND_PRESENCEs.Add(new IOPortDescripter<bool>($"DO_CST_IND_PRESENCEs.{i}", EnumIOType.OUTPUT));
                        _FoupIOMap.Outputs.DO_CST_IND_PRESENCEs[i].ChannelIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_PRESENCEs[i].PortIndex.Value = 0;
                        _FoupIOMap.Outputs.DO_CST_IND_PRESENCEs[i].Reverse.Value = false;
                        _FoupIOMap.Outputs.DO_CST_IND_PRESENCEs[i].IOOveride.Value = EnumIOOverride.NONE;
                        _FoupIOMap.Outputs.DO_CST_IND_PRESENCEs[i].MaintainTime.Value = 100;
                        _FoupIOMap.Outputs.DO_CST_IND_PRESENCEs[i].TimeOut.Value = 500;
                    }
                }
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SetFoupAlias()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            try
            {
                for(int i=0;i< _FoupIOMap.Inputs.DI_COVER_OPENs.Count;i++)
                {
                    _FoupIOMap.Inputs.DI_COVER_OPENs[i].Alias.Value = "COVER OPEN";
                }
                for (int i = 0; i < _FoupIOMap.Inputs.DI_COVER_CLOSEs.Count; i++)
                {
                    _FoupIOMap.Inputs.DI_COVER_CLOSEs[i].Alias.Value = "COVER CLOSE";
                }
                for (int i = 0; i < _FoupIOMap.Inputs.DI_CST_LOCK12s.Count; i++)
                {
                    _FoupIOMap.Inputs.DI_CST_LOCK12s[i].Alias.Value = "CST LOCK";
                }
                for (int i = 0; i < _FoupIOMap.Inputs.DI_CST_UNLOCK12s.Count; i++)
                {
                    _FoupIOMap.Inputs.DI_CST_UNLOCK12s[i].Alias.Value = "CST UNLOCK";
                }
                for (int i = 0; i < _FoupIOMap.Inputs.DI_CST12_PRESs.Count; i++)
                {
                    _FoupIOMap.Inputs.DI_CST12_PRESs[i].Alias.Value = "PRESENCE1";
                }
                for (int i = 0; i < _FoupIOMap.Inputs.DI_CST12_PRES2s.Count; i++)
                {
                    _FoupIOMap.Inputs.DI_CST12_PRES2s[i].Alias.Value = "PRESENCE2";
                }
                for (int i = 0; i < _FoupIOMap.Inputs.DI_DP_INs.Count; i++)
                {
                    _FoupIOMap.Inputs.DI_DP_INs[i].Alias.Value = "CST IN";
                }
                for (int i = 0; i < _FoupIOMap.Inputs.DI_DP_OUTs.Count; i++)
                {
                    _FoupIOMap.Inputs.DI_DP_OUTs[i].Alias.Value = "CST OUT";
                }
                for (int i = 0; i < _FoupIOMap.Inputs.DI_COVER_LOCKs.Count; i++)
                {
                    _FoupIOMap.Inputs.DI_COVER_LOCKs[i].Alias.Value = "OPENER LOCK";
                }
                for (int i = 0; i < _FoupIOMap.Inputs.DI_COVER_UNLOCKs.Count; i++)
                {
                    _FoupIOMap.Inputs.DI_COVER_UNLOCKs[i].Alias.Value = "OPENER UNLOCK";
                }
                for (int i = 0; i < _FoupIOMap.Inputs.DI_CST_CoverVacuums.Count; i++)
                {
                    _FoupIOMap.Inputs.DI_CST_CoverVacuums[i].Alias.Value = "COVER VACUUM";
                }
                for (int i = 0; i < _FoupIOMap.Inputs.DI_CST_MappingOuts.Count; i++)
                {
                    _FoupIOMap.Inputs.DI_CST_MappingOuts[i].Alias.Value = "MAPPING OUT";
                }
                for (int i = 0; i < _FoupIOMap.Inputs.DI_COVER_UPs.Count; i++)
                {
                    _FoupIOMap.Inputs.DI_COVER_UPs[i].Alias.Value = "COVER OPEN";
                }
                for (int i = 0; i < _FoupIOMap.Inputs.DI_COVER_DOWNs.Count; i++)
                {
                    _FoupIOMap.Inputs.DI_COVER_DOWNs[i].Alias.Value = "COVER CLOSE";
                }

                //OUTPUT
                for (int i = 0; i < _FoupIOMap.Outputs.DO_CST_12INCH_LOCKs.Count; i++)
                {
                    _FoupIOMap.Outputs.DO_CST_12INCH_LOCKs[i].Alias.Value = "CST LOCK";
                }
                for (int i = 0; i < _FoupIOMap.Outputs.DO_CST_12INCH_UNLOCKs.Count; i++)
                {
                    _FoupIOMap.Outputs.DO_CST_12INCH_UNLOCKs[i].Alias.Value = "CST UNLOCK";
                }
                for (int i = 0; i < _FoupIOMap.Outputs.DO_CST_LOADs.Count; i++)
                {
                    _FoupIOMap.Outputs.DO_CST_LOADs[i].Alias.Value = "CST IN";
                }
                for (int i = 0; i < _FoupIOMap.Outputs.DO_CST_UNLOADs.Count; i++)
                {
                    _FoupIOMap.Outputs.DO_CST_UNLOADs[i].Alias.Value = "CST OUT";
                }
                for (int i = 0; i < _FoupIOMap.Outputs.DO_CST_VACUUMs.Count; i++)
                {
                    _FoupIOMap.Outputs.DO_CST_VACUUMs[i].Alias.Value = "COVER VACUUM";
                }
                for (int i = 0; i < _FoupIOMap.Outputs.DO_COVER_LOCKs.Count; i++)
                {
                    _FoupIOMap.Outputs.DO_COVER_LOCKs[i].Alias.Value = "OPENER LOCK";
                }
                for (int i = 0; i < _FoupIOMap.Outputs.DO_COVER_UNLOCKs.Count; i++)
                {
                    _FoupIOMap.Outputs.DO_COVER_UNLOCKs[i].Alias.Value = "OPENER UNLOCK";
                }
                for (int i = 0; i < _FoupIOMap.Outputs.DO_COVER_OPENs.Count; i++)
                {
                    _FoupIOMap.Outputs.DO_COVER_OPENs[i].Alias.Value = "COVER OPEN";
                }
                for (int i = 0; i < _FoupIOMap.Outputs.DO_COVER_Closes.Count; i++)
                {
                    _FoupIOMap.Outputs.DO_COVER_Closes[i].Alias.Value = "COVER CLOSE";
                }
                for (int i = 0; i < _FoupIOMap.Outputs.DO_CST_MAPPINGs.Count; i++)
                {
                    _FoupIOMap.Outputs.DO_CST_MAPPINGs[i].Alias.Value = "CST MAPPING";
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

                RetVal = SaveFoupIOMapDefinitions();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public void DeInitModule()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                DeInitIOStates();
                IsDisposed = true;
            }
        }

        public IOPortDescripter<bool> GetIOPortDescripter(string ioName)
        {
            try
            {
                if (!string.IsNullOrEmpty(ioName))
                {
                    var inputs = IOMap.Inputs;
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

                        var outputs = IOMap.Outputs;
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

                        var outputs = IOMap.Outputs;
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

    }

}
