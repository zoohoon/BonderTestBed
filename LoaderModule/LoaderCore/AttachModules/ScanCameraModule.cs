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
    internal class ScanCameraModule : AttachedModuleBase, IScanCameraModule
    {
        public override bool Initialized { get; set; } = false;
        public override ModuleTypeEnum ModuleType => ModuleTypeEnum.SCANCAM;

        public ScanCameraDefinition Definition { get; set; }

        public ScanCameraDevice Device { get; set; }

        public override void DeInitModule()
        {

        }

        public EventCodeEnum SetDefinition(ScanCameraDefinition definition, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Definition = definition;
                ID = ModuleID.Create(ModuleType, index, "");

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetDevice(ScanCameraDevice device)
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

        public ScanCameraParam GetScanCameraParam(ICassetteModule Cassette)
        {
            ScanCameraParam retval = null;

            try
            {
                retval = Definition.ScanParams.Where(item =>
            item.CassetteNumber.Value == Cassette.ID.Index &&
            item.SubstrateType.Value == Cassette.Device.AllocateDeviceInfo.Type.Value &&
            item.SubstrateSize.Value == Cassette.Device.AllocateDeviceInfo.Size.Value
            ).FirstOrDefault();
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

                var scanParam = GetScanCameraParam(Cassette);
                bool hasScanParam = scanParam != null;

                retval = equalsScanType && hasScanParam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

}
