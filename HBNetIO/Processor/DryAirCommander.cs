using System;
using System.Collections.Generic;

namespace HBDryAir.Processor
{
    using LogModule;
    using Temperature.Temp.DryAir;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Temperature.DryAir;
    using System.Reflection;
    public class DryAirCommander : IDryAirController
    {
        public bool Initialized { get; set; } = false;
        private DryAirNetIOMappings _HBNetIOMap = null;
        public DryAirNetIOMappings HBNetIOMap
        {
            get { return _HBNetIOMap; }
        }

        private IIOService _IOServ;
        public IIOService IOServ
        {
            get { return this.IOManager().IOServ; }
            set { _IOServ = value; }
        }
        private HashSet<IOPortDescripter<bool>> OutPorts = new HashSet<IOPortDescripter<bool>>();
        private HashSet<IOPortDescripter<bool>> InPorts = new HashSet<IOPortDescripter<bool>>();

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(!Initialized)
                {
                    InitIOStates();
                    Initialized = true;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InitConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InitIOStates()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if ((this.EnvControlManager()?.IsUsingDryAir() ?? false) == true)
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

                    if(_HBNetIOMap != null)
                    {
                        Type type = _HBNetIOMap.Outputs.GetType();
                        PropertyInfo[] propertyinfos = type.GetProperties();
                        IOPortDescripter<bool> portDesc = null;

                        foreach (PropertyInfo property in propertyinfos)
                        {
                            portDesc = property.GetValue(_HBNetIOMap.Outputs) as IOPortDescripter<bool>;

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
                                    if (portDesc.ChannelIndex.Value != 1
                                        && portDesc.PortIndex.Value != -1
                                        && IOServ.Outputs[portDesc.ChannelIndex.Value] != null)
                                    {
                                        if (portDesc.ChannelIndex.Value >= 0 & portDesc.PortIndex.Value >= 0)
                                        {
                                            portDesc.Value = IOServ.Outputs[portDesc.ChannelIndex.Value].Port[portDesc.PortIndex.Value].PortVal;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                }
                else
                {
                    retval = EventCodeEnum.NONE;
                }
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(HBNetIOMap != null)
                {
                    Type type = HBNetIOMap.Outputs.GetType();
                    PropertyInfo[] propertyinfos = type.GetProperties();

                    foreach (PropertyInfo property in propertyinfos)
                    {
                        IOPortDescripter<bool> port = null;
                        port = property.GetValue(_HBNetIOMap.Outputs) as IOPortDescripter<bool>;
                        port?.SetService(IOServ);
                    }

                    type = _HBNetIOMap.Inputs.GetType();
                    propertyinfos = type.GetProperties();
                    foreach (PropertyInfo property in propertyinfos)
                    {
                        IOPortDescripter<bool> port = null;
                        port = property.GetValue(_HBNetIOMap.Inputs) as IOPortDescripter<bool>;
                        port?.SetService(IOServ);
                    }
                    retVal = EventCodeEnum.NONE;
                }



            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }

        private EventCodeEnum AddIOPortDescripter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (_HBNetIOMap != null)
                {


                    Type type = _HBNetIOMap.Outputs.GetType();
                    PropertyInfo[] propertyinfos = type.GetProperties();

                    OutPorts?.Clear();
                    InPorts?.Clear();

                    if (IOServ.Outputs.Count > 0)
                    {
                        foreach (PropertyInfo property in propertyinfos)
                        {
                            IOPortDescripter<bool> port = property.GetValue(_HBNetIOMap.Outputs) as IOPortDescripter<bool>;

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
                    }

                    type = _HBNetIOMap.Inputs.GetType();
                    propertyinfos = type.GetProperties();

                    if (IOServ.Inputs.Count > 0)
                    {
                        foreach (PropertyInfo property in propertyinfos)
                        {
                            IOPortDescripter<bool> port = property.GetValue(_HBNetIOMap.Inputs) as IOPortDescripter<bool>;

                            if (port != null)
                            {
                                InPorts.Add(port);
                                if (port.ChannelIndex.Value != -1 && port.PortIndex.Value != -1)
                                {
                                    IOServ.Inputs[port.ChannelIndex.Value].Port[port.PortIndex.Value].IOPortList.Add(port);
                                }
                            }
                        }
                    }

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                throw;
            }

            return retval;
        }

        public EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IOPortDescripter<bool> ioPort = null;

                switch (dryairType)
                {
                    case EnumDryAirType.STG:
                        ioPort = HBNetIOMap?.Outputs?.DO_DRYAIR_STGSV1;
                        break;
                    case EnumDryAirType.STGBOTTOM:
                        ioPort = HBNetIOMap?.Outputs?.DO_DRYAIR_STGSV2;
                        break;
                    case EnumDryAirType.LOADER:
                        ioPort = HBNetIOMap?.Outputs?.DO_DRYAIR_LDSV;
                        break;
                    case EnumDryAirType.TESTER:
                        ioPort = HBNetIOMap?.Outputs?.DO_DRYAIR_FOR_TESTER;
                        break;
                    default:
                        break;
                }
                if(ioPort != null)
                {
                    if (WriteBit(ioPort, value) != -1)
                        retVal = EventCodeEnum.NONE;
                    else
                        retVal = EventCodeEnum.DRYAIR_SETVALUE_IO_WRITEBIT_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;

        }

        public bool GetDryAirState(EnumDryAirType dryairType, int stageIndex = -1)
        {
            return false;
        }
        public int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageindex = -1)
        {
            int retVal = -1;
            value = false;
            try
            {
                IIOManager iOManager = this.IOManager();
                IOPortDescripter<bool> ioPort = null;

                switch (leakSensorIndex)
                {
                    case 0:
                        ioPort = HBNetIOMap?.Outputs?.DO_DRYAIR_FOR_TESTER;
                        break;
                    default:
                        break;
                }

                try
                {
                    if (ioPort != null && iOManager?.IOServ != null)
                    {
                        retVal = (int)iOManager.IOServ.ReadBit(ioPort, out value);
                    }
                    else
                    {
                        value = false;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public byte[] GetDryAirParam(int stageindex = -1)
        {
            return null;
        }

        public int SupplySV(bool value, int index = -1)
        {
            int retVal = -1;
            IOPortDescripter<bool> ioPort = HBNetIOMap?.Outputs?.DO_SUPPLY_SV;
            retVal = WriteBit(ioPort, value);
            return retVal;
        }

        public int ReturnSV(bool value, int index = -1) //1
        {
            int retVal = -1;
            IOPortDescripter<bool> ioPort = HBNetIOMap?.Outputs?.DO_Return_SV;
            retVal = WriteBit(ioPort, value);
            return retVal;
        }

        public int E_ReturnSV(bool value, int index = -1) //3
        {
            int retVal = -1;
            IOPortDescripter<bool> ioPort = HBNetIOMap?.Outputs?.DO_E_RETURN_SV;
            retVal = WriteBit(ioPort, value);
            return retVal;
        }

        public int T_returnSV(bool value, int index = -1) //4
        {
            int retVal = -1;
            IOPortDescripter<bool> ioPort = HBNetIOMap?.Outputs?.DO_T_RETURN_SV;
            retVal = WriteBit(ioPort, value);
            return retVal;
        }

        public int DryAirSTGSV1(bool value, int index = -1) //5
        {
            int retVal = -1;
            IOPortDescripter<bool> ioPort = HBNetIOMap?.Outputs?.DO_DRYAIR_STGSV1;
            retVal = WriteBit(ioPort, value);
            return retVal;
        }

        public int DryAirSTGSV2(bool value, int index = -1) //6
        {
            int retVal = -1;
            IOPortDescripter<bool> ioPort = HBNetIOMap?.Outputs?.DO_DRYAIR_STGSV2;
            retVal = WriteBit(ioPort, value);
            return retVal;
        }

        public int DryAirLDSV(bool value, int index = -1) //7
        {
            int retVal = -1;
            IOPortDescripter<bool> ioPort = HBNetIOMap?.Outputs?.DO_DRYAIR_LDSV;
            retVal = WriteBit(ioPort, value);
            return retVal;
        }

        public int DryAirforTester(bool value, int index = -1)
        {
            int retVal = -1;
            IOPortDescripter<bool> ioPort = HBNetIOMap?.Outputs?.DO_DRYAIR_FOR_TESTER;
            retVal = WriteBit(ioPort, value);
            return retVal;
        }


        private int WriteBit(IOPortDescripter<bool> ioPort, bool value)
        {
            int retVal = -1;
            IIOManager iOManager = this.IOManager();

            try
            {
                if (ioPort != null && iOManager?.IOServ != null)
                {
                    retVal = (int)iOManager.IOServ.WriteBit(ioPort, value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
