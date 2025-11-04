using System;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;
using LogModule;

namespace LoaderCore
{
    internal class ScanSensorModule : AttachedModuleBase, IScanSensorModule
    {
        private IOPortDescripter<bool> DOSCAN_SENSOR_OUT;
        private IOPortDescripter<bool> DISCAN_SENSOR_OUT;
        private IOPortDescripter<bool> DISCAN_SENSOR_IN;

        public override bool Initialized { get; set; } = false;
        public override ModuleTypeEnum ModuleType => ModuleTypeEnum.SCANSENSOR;

        public ScanSensorDefinition Definition { get; set; }

        public ScanSensorDevice Device { get; set; }

        public override void DeInitModule()
        {

        }

        public EventCodeEnum SetDefinition(ScanSensorDefinition definition, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Definition = definition;
                ID = ModuleID.Create(ModuleType, index, "");

                DOSCAN_SENSOR_OUT = Loader.IOManager.GetIOPortDescripter(Definition.DOSCAN_SENSOR_OUT.Value);
                DISCAN_SENSOR_OUT = Loader.IOManager.GetIOPortDescripter(Definition.DISCAN_SENSOR_OUT.Value);
                DISCAN_SENSOR_IN = Loader.IOManager.GetIOPortDescripter(Definition.DISCAN_SENSOR_IN.Value);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetDevice(ScanSensorDevice device)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Device = device;

                if (string.IsNullOrEmpty(device.Label.Value) == false)
                    this.ID = ModuleID.Create(ID.ModuleType, ID.Index, device.Label.Value);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Initialized = false;

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
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ScanSensorStateEnum GetState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            ScanSensorStateEnum state = ScanSensorStateEnum.UNKNOWN;

            try
            {
                bool insensor, outsensor = false;

                retVal = Loader.IOManager.ReadIO(DISCAN_SENSOR_IN, out insensor);
                retVal = Loader.IOManager.ReadIO(DISCAN_SENSOR_OUT, out outsensor);

                if (insensor == true && outsensor == false)
                {
                    state = ScanSensorStateEnum.RETRACTED;
                }
                else if (insensor == false && outsensor == true)
                {
                    state = ScanSensorStateEnum.EXTENDED;
                }
                else
                {
                    state = ScanSensorStateEnum.UNKNOWN;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return state;
        }

        public EventCodeEnum Retract()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = WriteSensor(false);

                retVal = WaitForSensor(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum Extend()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = WriteSensor(true);

                retVal = WaitForSensor(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WriteSensor(bool isOut)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.IOManager.WriteIO(DOSCAN_SENSOR_OUT, isOut);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WaitForSensor(bool isOut)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.IOManager.WaitForIO(DISCAN_SENSOR_IN, !isOut, Definition.IOWaitTimeout.Value);

                retVal = Loader.IOManager.WaitForIO(DISCAN_SENSOR_OUT, isOut, Definition.IOWaitTimeout.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public ScanSensorParam GetScanParam(ICassetteModule Cassette)
        {
            ScanSensorParam retval = null;

            try
            {
                retval = Definition.ScanParams.Where(item => item.SubstrateType.Value == Cassette.Device.AllocateDeviceInfo.Type.Value && item.SubstrateSize.Value == Cassette.Device.AllocateDeviceInfo.Size.Value).FirstOrDefault();

                if(retval == null)
                {
                    LoggerManager.Debug($"[ScanSensorModule], GetScanParam(), Parameter is not matched. Type = {Cassette.Device.AllocateDeviceInfo.Type.Value}, Size = {Cassette.Device.AllocateDeviceInfo.Size.Value}");

                    foreach (var item in Definition.ScanParams.Select((value, index) => new { Value = value, Index = index }))
                    {
                        ScanSensorParam currentValue = item.Value;
                        int currentIndex = item.Index;

                        LoggerManager.Debug($"[ScanSensorModule], GetScanParam(), ScanSensorParam[{currentIndex}], Type : {currentValue.SubstrateType}, Size = {currentValue.SubstrateSize}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool CanScan(ICassetteModule Cassette)
        {
            bool retval = false;

            try
            {
                bool equalsScanType = Cassette.Definition.ScanModuleType.Value == ID.ModuleType;

                var scanParam = GetScanParam(Cassette);
                bool hasParam = scanParam != null;

                retval = equalsScanType && hasParam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

}
