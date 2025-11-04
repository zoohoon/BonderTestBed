using System;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;
using LogModule;
using LoaderParameters.Data;
using ProberInterfaces.Loader;

namespace LoaderCore
{
    internal class SlotModule : AttachedModuleBase, ISlotModule
    {
        public override bool Initialized { get; set; } = false;
        public override ModuleTypeEnum ModuleType
        {
            get { return ModuleTypeEnum.SLOT; }
        }

        public SlotDefinition Definition { get; set; }

        public SlotDevice Device { get; set; }

        public ICassetteModule Cassette { get; set; }

        public WaferHolder Holder { get; set; }
        public ReservationInfo ReservationInfo { get; set; }

        public int LocalSlotNumber { get; private set; }
        public bool Enable { get; set; }
        public UpdateHolderDelegate HolderStatusChanged { get; set; }
        public EventCodeEnum SetDefinition(SlotDefinition definition, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Enable = true;
                Definition = definition;
                ID = ModuleID.Create(ModuleType, index, "");

                Holder = new WaferHolder();
                Holder.SetOwner(this);

                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return retVal;
        }

        public EventCodeEnum SetDevice(SlotDevice device)
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

        public void SetCassette(ICassetteModule cassette, int localSlotNumber)
        {
            try
            {
                this.Cassette = cassette;
                this.LocalSlotNumber = localSlotNumber;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    RecoveryWaferStatus();

                    Initialized = false;
                    ReservationInfo = new ReservationInfo();
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

        public override void DeInitModule()
        {

        }

        public EventCodeEnum RecoveryWaferStatus(bool forcedAllocate = false)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (Holder.Status == EnumSubsStatus.UNDEFINED)
                {
                    if (Cassette.ScanState == CassetteScanStateEnum.READ)
                    {
                        Holder.SetUnknown();
                    }
                    else
                    {
                        Holder.SetUndefined();
                    }
                }
                else if (Holder.Status == EnumSubsStatus.NOT_EXIST)
                {
                    if (Cassette.ScanState == CassetteScanStateEnum.READ)
                    {
                        //No changed.
                    }
                    else
                    {
                        Holder.SetUndefined();
                    }
                }
                else if (Holder.Status == EnumSubsStatus.EXIST)
                {
                    if (Cassette.ScanState == CassetteScanStateEnum.READ)
                    {
                        //No changed.
                    }
                    else
                    {
                        Holder.SetUndefined();
                    }
                }
                else if (Holder.Status == EnumSubsStatus.UNKNOWN)
                {
                    if (Cassette.ScanState == CassetteScanStateEnum.READ)
                    {
                        
                    }
                    else
                    {
                        Holder.SetUndefined();
                    }
                }
                else
                {
                    throw new NotImplementedException("InitWaferStatus()");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void ValidateWaferStatus()
        {
            try
            {
                if (Holder.Status == EnumSubsStatus.EXIST &&
                                Cassette.ScanState != CassetteScanStateEnum.READ)
                {
                    Holder.SetUnknown();
                }
                else
                {
                    //No state changed.
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum IsWaferonmodule(out bool Result)
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            Result = false;
            try
            {
               
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.IO_NOT_MATCHED;
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public TransferObject GetSourceDeviceInfo()
        {
            TransferObject desc = null;

            try
            {
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    if (Device != null && Device.IsOverrideEnable.Value)
                    {
                        desc = Cassette.Device.AllocateDeviceInfo.Clone() as TransferObject;
                        desc.WaferType = Device.OverrideWaferType;
                        desc.OCRMode = Device.OverrideOCRMode;
                        desc.OCRType = Device.OverrideOCRType;
                        desc.OCRDirection = Device.OverrideOCRDirection;
                    }
                    else
                    {
                        desc = Cassette.Device.AllocateDeviceInfo;
                    }
                }
                else
                {
                
                    if (Device != null && Device.IsOverrideEnable.Value) //TODO Remove Lloyd 190426
                    {
                        desc = Cassette.Device.AllocateDeviceInfo.Clone() as TransferObject;
                        desc.WaferType = Device.OverrideWaferType;
                        desc.OCRMode = Device.OverrideOCRMode;
                        desc.OCRType = Device.OverrideOCRType;
                        desc.OCRDirection = Device.OverrideOCRDirection;
                        desc.CST_HashCode = Cassette.HashCode;
                    }
                    else
                    {
                        if (Cassette.Device.AllocateDeviceInfo.OCRDevParam.ConfigList.Count() == 0)
                        {                            
                            IDeviceManager devicemanager = this.Loader.Container.Resolve<IDeviceManager>();

                            if (devicemanager != null)
                            {
                                OCRDevParameter OCRDev = new OCRDevParameter();
                                var getparam = devicemanager.GetOCRDevParameter("DEFAULTDEVNAME");
                                if (getparam.retVal == EventCodeEnum.NONE)
                                {
                                    OCRDev = getparam.param as OCRDevParameter;
                                    Cassette.Device.AllocateDeviceInfo.OCRDevParam = OCRDev;
                                }

                            }
                        }
                        desc = Cassette.Device.AllocateDeviceInfo;
                        desc.CST_HashCode = Cassette.HashCode;
                    }
                   // desc = this.Loader.DeviceManager.GetDeviceInfo(this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return desc;
        }
        
        public Degree GetLoadingAngle()
        {
            Degree retval = Degree.ZERO;

            try
            {
                retval = Cassette.Device.LoadingNotchAngle.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Degree GetWaxisAngle(SubstrateTypeEnum type, SubstrateSizeEnum size)
        {
            Degree retval = Degree.ZERO;

            try
            {
                var slot1Accparam = Cassette.GetSlot1AccessParam(type, size);

                retval = (slot1Accparam.Position.W.Value * ConstantValues.W_DIST_TO_DEGREE);

                //retval = new Degree(slot1Accparam.Position.W * ConstantValues.W_DIST_TO_DEGREE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

    }
}
