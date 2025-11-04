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
    internal class CCModule : AttachedModuleBase, ICCModule
    {
        public override bool Initialized { get; set; } = false;

        public override ModuleTypeEnum ModuleType => ModuleTypeEnum.CC;

        public CCDefinition Definition { get; set; }

        public CCDevice Device { get; set; }

        public CardHolder Holder { get; private set; }

        public bool Enable { get; set; }
        public EventCodeEnum SetDefinition(CCDefinition definition, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Enable = true;
                Definition = definition;
                ID = ModuleID.Create(ModuleType, index, "");

                Holder = new CardHolder();
                Holder.SetOwner(this);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetDevice(CCDevice device)
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
                    RecoveryCardStatus();

                    Initialized = false;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION");

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

        public EventCodeEnum RecoveryCardStatus()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                EnumSubsStatus CCCardStatus;
                EnumWaferState CCState=EnumWaferState.UNDEFINED;
                if (Loader.ServiceCallback != null)
                {
                    CCCardStatus = Loader.ServiceCallback.UpdateCardStatus(out CCState);
                }
                else
                {
                    if (Loader.LoaderMaster != null && Loader.LoaderMaster.IsAliveClient(Loader.LoaderMaster.GetClient(ID.Index)))
                    {
                        Loader.LoaderMaster.CellCardRefresh(ID.Index);
                    }
                    else
                    {
                    }

                    return retVal;
                }

                if (Holder.Status == EnumSubsStatus.UNDEFINED)
                {
                    if (CCCardStatus == EnumSubsStatus.NOT_EXIST)
                    {
                        Holder.SetUnload();
                    }
                    else if (CCCardStatus == EnumSubsStatus.EXIST)
                    {
                        Holder.SetAllocate();
                        Holder.TransferObject.WaferState = CCState;
                    }
                    else
                    {
                        Holder.SetUnknown();
                    }
                }
                else if (Holder.Status == EnumSubsStatus.NOT_EXIST)
                {
                    if (CCCardStatus == EnumSubsStatus.NOT_EXIST)
                    {
                        //Holder.SetUnload();
                    }
                    else if (CCCardStatus == EnumSubsStatus.EXIST)
                    {
                        Holder.SetAllocate();
                        Holder.TransferObject.WaferState = CCState;
                    }
                    else
                    {
                        Holder.SetUnknown();
                    }
                }
                else if (Holder.Status == EnumSubsStatus.EXIST)
                {
                    if (CCCardStatus == EnumSubsStatus.NOT_EXIST)
                    {
                        Holder.SetUnload();
                    }
                    else if (CCCardStatus == EnumSubsStatus.EXIST)
                    {
                        Holder.TransferObject.WaferState = CCState;
                    }
                    else
                    {
                        Holder.SetUnknown();
                    }
                }
                else if (Holder.Status == EnumSubsStatus.UNKNOWN)
                {
                    Holder.SetUnknown();
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

        public void ValidateCardStatus()
        {
            try
            {
                EnumWaferState CCState = EnumWaferState.UNDEFINED;
                var CCCardStatus = Loader.ServiceCallback.UpdateCardStatus(out CCState);

                if (CCCardStatus != Holder.Status)
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

        public CCAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size)
        {
            CCAccessParam accessParam = null;

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

            CCAccessParam pa;

            try
            {
                pa = GetAccessParam(type, size);

                retval = new Degree(pa.Position.W.Value * ConstantValues.W_DIST_TO_DEGREE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
