using System;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;
using LogModule;
using WaferDisappearControl;
using LoaderParameters.Data;

namespace LoaderCore
{
    internal class ChuckModule : AttachedModuleBase, IChuckModule
    {
        public override bool Initialized { get; set; } = false;

        public override ModuleTypeEnum ModuleType => ModuleTypeEnum.CHUCK;

        public ChuckDefinition Definition { get; set; }

        public ChuckDevice Device { get; set; }

        public WaferHolder Holder { get; private set; }

        public ReservationInfo ReservationInfo { get; set; }
        public bool Enable { get; set; }
        public UpdateHolderDelegate HolderStatusChanged { get; set; }
        public EventCodeEnum SetDefinition(ChuckDefinition definition, int index)
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

        public EventCodeEnum SetDevice(ChuckDevice device)
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

                    retval=RecoveryWaferStatus();

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
            //SetWaferObjectStatus()
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (Loader.StageContainer != null)
                {
                    Loader.StageContainer.Resolve<IStageSupervisor>().SetWaferObjectStatus();
                }
                EnumSubsStatus chuckWaferStatus;
                if (Loader.ServiceCallback != null)
                {
                    chuckWaferStatus = Loader.ServiceCallback.GetChuckWaferStatus();
                }
                else
                {
                    if (Loader.LoaderMaster != null && Loader.LoaderMaster.IsAliveClient(Loader.LoaderMaster.GetClient(ID.Index)))
                    {
                        Loader.LoaderMaster.CellWaferRefresh(ID.Index);
                    }
                    else
                    {
                        Holder.SetNull();
                    }
                    
                    return retVal;
                }
                if (Holder.Status == EnumSubsStatus.UNDEFINED)
                {
                    if (chuckWaferStatus == EnumSubsStatus.NOT_EXIST)
                    {
                        Holder.SetUnload();
                    }
                    else if (chuckWaferStatus == EnumSubsStatus.EXIST)
                    {
                        Holder.SetAllocate();
                    }
                    else
                    {
                        retVal = EventCodeEnum.LOADER_STATE_INVALID;
                        Holder.SetUnknown();
                    }
                }
                else if (Holder.Status == EnumSubsStatus.NOT_EXIST)
                {
                    if (chuckWaferStatus == EnumSubsStatus.NOT_EXIST)
                    {

                    }
                    else if (chuckWaferStatus == EnumSubsStatus.EXIST)
                    {
                        bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.NOT_EXIST.ToString(), EnumSubsStatus.EXIST.ToString());
                        if (isChecked)
                        {
                            Holder.SetAllocate();
                        }
                        else
                        {
                            retVal = EventCodeEnum.LOADER_STATE_INVALID;
                            Holder.SetUnknown();
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.LOADER_STATE_INVALID;
                        Holder.SetUnknown();
                    }
                }
                else if (Holder.Status == EnumSubsStatus.EXIST)
                {
                    if (chuckWaferStatus == EnumSubsStatus.NOT_EXIST)
                    {
                        bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.EXIST.ToString(), EnumSubsStatus.NOT_EXIST.ToString());

                        if (isChecked)
                        {
                            Holder.SetUnload();
                        }
                        else
                        {
                            retVal = EventCodeEnum.LOADER_STATE_INVALID;
                            Holder.SetUnknown();
                        }
                    }
                    else if (chuckWaferStatus == EnumSubsStatus.EXIST)
                    {

                    }
                    else
                    {
                        retVal = EventCodeEnum.LOADER_STATE_INVALID;
                        Holder.SetUnknown();
                    }
                }
                else if (Holder.Status == EnumSubsStatus.UNKNOWN)
                {


                    if (chuckWaferStatus == EnumSubsStatus.NOT_EXIST)
                    {
                        bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.UNKNOWN.ToString(), EnumSubsStatus.NOT_EXIST.ToString());
                        if (isChecked)
                        {
                            Holder.SetUnload();
                        }
                        else
                        {
                            retVal = EventCodeEnum.LOADER_STATE_INVALID;
                        }
                    }
                    else if (chuckWaferStatus == EnumSubsStatus.EXIST)
                    {
                        bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.UNKNOWN.ToString(), EnumSubsStatus.EXIST.ToString());
                        if (isChecked)
                        {
                            Holder.SetAllocate();
                        }
                        else
                        {
                            retVal = EventCodeEnum.LOADER_STATE_INVALID;
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.LOADER_STATE_INVALID;
                        Holder.SetUnknown();
                    }
                }
                else
                {
                    throw new NotImplementedException("InitWaferStatus()");
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void ValidateWaferStatus()
        {
            try
            {
                var chuckWaferStatus = Loader.ServiceCallback.GetChuckWaferStatus();

                if (chuckWaferStatus != Holder.Status)
                {
                    //=> state sync error
                    Holder.SetUnknown();
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

        public ChuckAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size)
        {
            ChuckAccessParam accessParam = null;

            try
            {
                accessParam = this.Definition.AccessParams
               .Where(
               item =>
               item.SubstrateType.Value == type &&
               item.SubstrateSize.Value == size
               ).FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return accessParam;
        }

        public Degree GetLoadingAngle()
        {
            Degree retval = Degree.ZERO;

            try
            {
                retval = Device.LoadingNotchAngle.Value;
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

            ChuckAccessParam pa;

            try
            {
                pa = GetAccessParam(type, size);

                if(pa != null)
                {
                    retval = new Degree(pa.Position.W.Value * ConstantValues.W_DIST_TO_DEGREE);
                }
                else
                {
                    LoggerManager.Error($"[ChuckModule], GetWaxisAngle() : pa is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
