using Autofac;
using IOServiceProvider;
using LogModule;
using ParamHelper;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.Error;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Temperature.Temp.DryAir
{
    public interface IDryAirManager : IDisposable, IFactoryModule
    {
        DryAirNetIOMappings HBNetIOMap { get; }
        ErrorCodeEnum StartManager();

        int WriteBit(IOPortDescripter<bool> portDesc, bool value);
        int ReadBit(IOPortDescripter<bool> portDesc, out bool value);
        int WaitForIO(IOPortDescripter<bool> io, bool level, long timeout = 0);
        void SetHBNetIOMap(DryAirNetIOMappings hBNetIOMap);
    }

    public class DryAirManager : IDryAirManager, INotifyPropertyChanged
    {
        public int InitPriority { get; } = 100;
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        AutoResetEvent areUpdateEvent = new AutoResetEvent(false);

        private bool _DevConnected;
        public bool DevConnected
        {
            get { return _DevConnected; }
            set { _DevConnected = value; }
        }

        System.Timers.Timer _monitoringTimer;


        private IIOService _IOServ;

        public IIOService IOServ
        {
            get { return _IOServ; }
            set { _IOServ = value; }
        }


        private DryAirNetIOMappings _HBNetIOMap;
        public DryAirNetIOMappings HBNetIOMap
        {
            get { return _HBNetIOMap; }
        }




        bool bStopUpdateStateThread = false;
        Thread UpdateStateThread;
        private string IODevicePath;

        public DryAirManager()
        {
        }

        private bool isDisposed = false;
        public void Dispose()
        {
            try
            {
                if (!isDisposed)
                {
                    DeInitIOStates();
                    _monitoringTimer?.Stop();
                    isDisposed = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void InitIOStates()
        {
            try
            {
                DeliveryIOService();

                isDisposed = true;
                _monitoringTimer = new System.Timers.Timer(12);
                _monitoringTimer.Elapsed += _monitoringTimer_Elapsed;
                _monitoringTimer.Start();

                Type type = _HBNetIOMap.Outputs.GetType();
                PropertyInfo[] propertyinfos = type.GetProperties();
                IOPortDescripter<bool> portDesc;

                foreach (PropertyInfo property in propertyinfos)
                {
                    portDesc = (IOPortDescripter<bool>)property.GetValue(_HBNetIOMap.Outputs);

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
                        if (IOServ.Outputs.Count != 0 && IOServ.Outputs[portDesc.ChannelIndex.Value] != null)
                        {
                            portDesc.Value = IOServ.Outputs[portDesc.ChannelIndex.Value].Port[portDesc.PortIndex.Value].PortVal;
                        }
                    }
                }

                UpdateStateThread = new Thread(new ThreadStart(UpdateStateProc));
                bStopUpdateStateThread = false;
                UpdateStateThread.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void DeliveryIOService()
        {
            try
            {
                Type type = HBNetIOMap.Outputs.GetType();
                PropertyInfo[] propertyinfos = type.GetProperties();

                foreach (PropertyInfo property in propertyinfos)
                {
                    IOPortDescripter<bool> port = (IOPortDescripter<bool>)property.GetValue(_HBNetIOMap.Outputs);
                    port.SetService(IOServ);

                }

                type = _HBNetIOMap.Inputs.GetType();
                propertyinfos = type.GetProperties();
                foreach (PropertyInfo property in propertyinfos)
                {
                    IOPortDescripter<bool> port = (IOPortDescripter<bool>)property.GetValue(_HBNetIOMap.Inputs);
                    port.SetService(IOServ);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public int DeInitIOStates()
        {
            int retVal = -1;
            try
            {
                IOServ.DeInitIO();
                if (UpdateStateThread != null)
                {
                    bStopUpdateStateThread = true;
                    UpdateStateThread.Join();
                }

                DevConnected = false;
                retVal = 1;
            }
            catch (Exception err)
            {
                bStopUpdateStateThread = true;
                UpdateStateThread.Join();
                DevConnected = false;
                //LoggerManager.Error($"DeInitIO() Function error: " + err.Message);
                LoggerManager.Exception(err);

                throw new IOException(string.Format("DeInitIO(): Deinitializing failed."), err);
            }
            return retVal;
        }
        ~DryAirManager()
        {
            Dispose();
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
            areUpdateEvent.Set();
        }
        private void UpdateStateProc()
        {
            try
            {
                Type type = _HBNetIOMap.Outputs.GetType();
                PropertyInfo[] propertyinfos = type.GetProperties();
                IOPortDescripter<bool> portDesc;

                while (bStopUpdateStateThread == false)
                {
                    foreach (PropertyInfo property in propertyinfos)
                    {
                        portDesc = (IOPortDescripter<bool>)property.GetValue(_HBNetIOMap.Outputs);

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
                            //if (IOServ.Outputs.Count != 0 && IOServ.Outputs[portDesc.ChannelIndex] != null)
                            //{
                            //    IOServ.Outputs[portDesc.ChannelIndex].Port[portDesc.PortIndex] = portDesc.Value;
                            //}
                            ////if (IOServ.Outputs[portDesc.ChannelIndex] != null)
                            ////{
                            ////    IOServ.Outputs[portDesc.ChannelIndex].Port[portDesc.PortIndex] = portDesc.Value;
                            ////}
                        }
                    }

                    type = _HBNetIOMap.Inputs.GetType();
                    propertyinfos = type.GetProperties();

                    foreach (PropertyInfo property in propertyinfos)
                    {
                        portDesc = (IOPortDescripter<bool>)property.GetValue(_HBNetIOMap.Inputs);

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
                            if (IOServ.Outputs.Count != 0 && IOServ.Inputs[portDesc.ChannelIndex.Value] != null)
                            {
                                portDesc.Value = IOServ.Inputs[portDesc.ChannelIndex.Value].Port[portDesc.PortIndex.Value].PortVal;
                            }

                        }
                    }
                    areUpdateEvent.WaitOne(100);
                }

            }
            catch/* (Exception err)*/
            {
                //LoggerManager.Error($string.Format("UpdateIOProc(): Error occurred while update io proc. Err = {0}", err.Message));
            }
        }

        public ErrorCodeEnum StartManager()
        {
            ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
            try
            {

                if (IOServ != null)
                {
                    IOServ.DeInitIO();
                    IOServ = null;
                }

                IOServ = new IOService();
                IOServ.LoadSysParameter();

                this.IODevicePath = this.FileManager().GetSystemParamFullPath(@"\Temperature\Chiller\Huber\", "HBIODeviceMapping.json");
                IOServ.InitModule(0, IODevicePath, null); //첫번째, 세번째 인자는 여기에선 별 의미 없da.
                InitIOStates();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
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

        public void SetHBNetIOMap(DryAirNetIOMappings hBNetIOMap)
        {
        }
    }
}
