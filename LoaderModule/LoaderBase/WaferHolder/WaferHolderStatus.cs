using System;
using System.Runtime.CompilerServices;

using ProberInterfaces;
using LogModule;

namespace LoaderBase
{
    [Serializable]
    internal abstract class WaferHolderStatus
    {
        internal WaferHolderStatus(WaferHolder slot)
        {
            this.Holder = slot;
        }

        internal WaferHolder Holder { get; private set; }

        internal void ChangeStatus(EnumSubsStatus status)
        {
            try
            {
                if (Holder.StatusObj.Status != status)
                {
                    switch (status)
                    {
                        case EnumSubsStatus.UNKNOWN:
                            Holder.StatusObj = new WaferHolderUnknownStatus(Holder);
                            break;
                        case EnumSubsStatus.UNDEFINED:
                            Holder.StatusObj = new WaferHolderUndefinedStatus(Holder);
                            break;
                        case EnumSubsStatus.NOT_EXIST:
                            Holder.StatusObj = new WaferHolderNotExistStatus(Holder);
                            break;
                        case EnumSubsStatus.EXIST:
                            Holder.StatusObj = new WaferHolderExistStatus(Holder);
                            break;
                        case EnumSubsStatus.HIDDEN:
                            Holder.StatusObj = new WaferHolderHiddenStatus(Holder);
                            break;
                    }

                    if (Holder.Owner.HolderStatusChanged != null)
                    {
                        Holder.Owner.HolderStatusChanged(Holder.Owner.ID,Holder);
                    }

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public abstract EnumSubsStatus Status { get; }

        public virtual void SetUndefined()
        {
            ThrowInvalidState();
        }

        public virtual void SetUnload()
        {
            ThrowInvalidState();
        }

        public virtual void SetHidden()
        {
            ThrowInvalidState();
        }

        public virtual void SetNull()
        {
            ThrowInvalidState();
        }

        public virtual void SetLoad(TransferObject substrate)
        {
            ThrowInvalidState();
        }

        public virtual void SetUnknown(TransferObject substrate = null)
        {
            ThrowInvalidState();
        }

        public virtual void SetPosition(IWaferLocatable pos)
        {
            ThrowInvalidState();
        }

        public virtual void SetTransfered(IWaferOwnable target)
        {
            ThrowInvalidState();
        }
        public virtual void SetOriginID(IWaferOwnable target)
        {
            ThrowInvalidState();
        }
        internal void ThrowInvalidState([CallerMemberName] string memberName = "")
        {
            string msg = $"[{Holder.Owner.ID}].Holder.{Status}.{memberName}() : Raise invalid state error occurred.";
            throw new Exception(msg);
        }

        internal void SetUndefinedFunc()
        {
            try
            {
                Holder.Backup.Clear();
                Holder.TransferObject = null;
                Holder.IsWaferOnHandler = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        internal void SetUnloadFunc()
        {
            try
            {
                Holder.Backup.Clear();
                Holder.TransferObject = null;
                Holder.IsWaferOnHandler = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        internal void SetNullFunc()
        {
            try
            {
                if (Holder.TransferObject != null)
                {
                    Holder.TransferObject = null;
                }
                Holder.IsWaferOnHandler = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        internal void SetLoadFunc(TransferObject transferObject)
        {
            try
            {
                Holder.Backup.Clear();

                transferObject.PrevPos = transferObject.CurrPos;
                transferObject.CurrPos = Holder.Owner.ID;

                transferObject.PrevHolder = transferObject.CurrHolder;
                transferObject.CurrHolder = Holder.Owner.ID;

                //transferObject.UnloadHolder = transferObject,;
                Holder.TransferObject = transferObject;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        internal void SetUnknownFunc(TransferObject transferObject = null)
        {
            try
            {

                if (Holder.TransferObject != null)
                {
                    //backup
                    Holder.Backup.Put(Holder.TransferObject);

                    //
                    Holder.TransferObject.WaferState = EnumWaferState.MISSED;

                    Holder.TransferObject.PrevPos = Holder.TransferObject.CurrPos;
                    Holder.TransferObject.CurrPos = ModuleID.UNDEFINED;

                    Holder.TransferObject.PrevHolder = Holder.TransferObject.CurrHolder;
                    Holder.TransferObject.CurrHolder = ModuleID.UNDEFINED;
                }
                else if (Holder.Status == EnumSubsStatus.NOT_EXIST &&
                    transferObject != null) 
                {
                    Holder.Backup.Clear();
                    Holder.Backup.Put(transferObject);

                    Holder.TransferObject = transferObject;

                    Holder.TransferObject.PrevPos = transferObject.CurrPos;
                    Holder.TransferObject.CurrPos = Holder.Owner.ID;

                    Holder.TransferObject.PrevHolder = transferObject.CurrHolder;
                    Holder.TransferObject.CurrHolder = Holder.Owner.ID;

                    
                }
                Holder.IsWaferOnHandler = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        internal void SetPositionFunc(IWaferLocatable pos)
        {
            try
            {
                Holder.TransferObject.PrevPos = Holder.TransferObject.CurrPos;
                Holder.TransferObject.CurrPos = pos.ID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    internal class WaferHolderUndefinedStatus : WaferHolderStatus
    {
        public WaferHolderUndefinedStatus(WaferHolder slot) : base(slot) { }

        public override EnumSubsStatus Status => EnumSubsStatus.UNDEFINED;

        public override void SetUndefined()
        {
            //No WORKS.
        }

        public override void SetLoad(TransferObject transferObject)
        {
            try
            {
                SetLoadFunc(transferObject);
                ChangeStatus(EnumSubsStatus.EXIST);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override void SetNull()
        {
            try
            {
                SetNullFunc();
                ChangeStatus(EnumSubsStatus.UNDEFINED);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetUnload()
        {
            try
            {
                SetUnloadFunc();
                ChangeStatus(EnumSubsStatus.NOT_EXIST);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetUnknown(TransferObject substrate)
        {
            try
            {
                SetUnknownFunc();
                ChangeStatus(EnumSubsStatus.UNKNOWN);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override void SetHidden()
        {
            try
            {
                ChangeStatus(EnumSubsStatus.HIDDEN);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //NoWork
        }
    }

    [Serializable]
    internal class WaferHolderNotExistStatus : WaferHolderStatus
    {
        public WaferHolderNotExistStatus(WaferHolder slot) : base(slot) { }

        public override EnumSubsStatus Status => EnumSubsStatus.NOT_EXIST;

        public override void SetUndefined()
        {
            try
            {
                SetUndefinedFunc();
                ChangeStatus(EnumSubsStatus.UNDEFINED);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetLoad(TransferObject transferObject)
        {
            try
            {
                SetLoadFunc(transferObject);
                ChangeStatus(EnumSubsStatus.EXIST);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override void SetUnload()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //NoWork;
        }
        public override void SetNull()
        {
            try
            {
                SetNullFunc();
                ChangeStatus(EnumSubsStatus.UNDEFINED);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override void SetUnknown(TransferObject substrate)
        {
            try
            {
                SetUnknownFunc(substrate);
                ChangeStatus(EnumSubsStatus.UNKNOWN);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override void SetHidden()
        {
            try
            {
                ChangeStatus(EnumSubsStatus.HIDDEN);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //NoWork
        }
    }

    [Serializable]
    internal class WaferHolderExistStatus : WaferHolderStatus
    {
        public WaferHolderExistStatus(WaferHolder slot) : base(slot) { }

        public override EnumSubsStatus Status => EnumSubsStatus.EXIST;

        public override void SetUndefined()
        {
            try
            {
                SetUndefinedFunc();
                ChangeStatus(EnumSubsStatus.UNDEFINED);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetUnload()
        {
            try
            {
                SetUnloadFunc();
                ChangeStatus(EnumSubsStatus.NOT_EXIST);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override void SetNull()
        {
            try
            {
                SetNullFunc();
                ChangeStatus(EnumSubsStatus.UNDEFINED);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public override void SetLoad(TransferObject substrate)
        {
            try
            {
                SetLoadFunc(substrate);
                ChangeStatus(EnumSubsStatus.EXIST);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //NoWork
        }

        public override void SetUnknown(TransferObject substrate)
        {
            try
            {
                SetUnknownFunc();
                ChangeStatus(EnumSubsStatus.UNKNOWN);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetPosition(IWaferLocatable pos)
        {
            try
            {
                SetPositionFunc(pos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetTransfered(IWaferOwnable target)
        {
            try
            {
                var transferObj = Holder.TransferObject;

                SetUnload();

                target.Holder.SetLoad(transferObj);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetOriginID(IWaferOwnable target)
        {
            try
            {
                var transferObj = Holder.TransferObject;

                if (target.Holder.Owner is IWaferSupplyModule)
                {
                    var wsm = target.Holder.Owner as IWaferSupplyModule;

                    var devInfo = wsm.GetSourceDeviceInfo();
                    transferObj.SetDeviceInfo(target.Holder.Owner.ID, devInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override void SetHidden()
        {
            try
            {
                ChangeStatus(EnumSubsStatus.HIDDEN);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //NoWork
        }

    }

    [Serializable]
    internal class WaferHolderUnknownStatus : WaferHolderStatus
    {
        public WaferHolderUnknownStatus(WaferHolder slot) : base(slot) { }

        public override EnumSubsStatus Status => EnumSubsStatus.UNKNOWN;

        public override void SetUndefined()
        {
            try
            {
                SetUndefinedFunc();
                ChangeStatus(EnumSubsStatus.UNDEFINED);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetUnload()
        {
            try
            {
                SetUnloadFunc();
                ChangeStatus(EnumSubsStatus.NOT_EXIST);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetNull()
        {
            try
            {
                SetNullFunc();
                ChangeStatus(EnumSubsStatus.UNDEFINED);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override void SetLoad(TransferObject transferObject)
        {
            try
            {
                SetLoadFunc(transferObject);
                ChangeStatus(EnumSubsStatus.EXIST);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetUnknown(TransferObject transferObject)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //No WORKS.
        }
        public override void SetHidden()
        {
            try
            {
                ChangeStatus(EnumSubsStatus.HIDDEN);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //NoWork
        }
        public override void SetTransfered(IWaferOwnable target)
        {
            try
            {
                var transferObj = Holder.TransferObject;

                SetUnload();

                target.Holder.SetUnknown(transferObj);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    internal class WaferHolderHiddenStatus : WaferHolderStatus
    {
        public WaferHolderHiddenStatus(WaferHolder slot) : base(slot) { }

        public override EnumSubsStatus Status => EnumSubsStatus.HIDDEN;

        public override void SetUndefined()
        {
            try
            {
                SetUndefinedFunc();
                ChangeStatus(EnumSubsStatus.UNDEFINED);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetUnload()
        {
            try
            {
                SetUnloadFunc();
                ChangeStatus(EnumSubsStatus.NOT_EXIST);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override void SetNull()
        {
            try
            {
                SetNullFunc();
                ChangeStatus(EnumSubsStatus.UNDEFINED);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public override void SetLoad(TransferObject substrate)
        {
            try
            {
                SetLoadFunc(substrate);
                ChangeStatus(EnumSubsStatus.EXIST);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //NoWork
        }

        public override void SetHidden()
        {
            try
            {
                ChangeStatus(EnumSubsStatus.HIDDEN);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //NoWork
        }

        public override void SetUnknown(TransferObject substrate)
        {
            try
            {
                SetUnknownFunc();
                ChangeStatus(EnumSubsStatus.UNKNOWN);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetPosition(IWaferLocatable pos)
        {
            try
            {
                SetPositionFunc(pos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetTransfered(IWaferOwnable target)
        {
            try
            {
                var transferObj = Holder.TransferObject;

                SetUnload();

                target.Holder.SetLoad(transferObj);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetOriginID(IWaferOwnable target)
        {
            try
            {
                var transferObj = Holder.TransferObject;

                if (target.Holder.Owner is IWaferSupplyModule)
                {
                    var wsm = target.Holder.Owner as IWaferSupplyModule;

                    var devInfo = wsm.GetSourceDeviceInfo();
                    transferObj.SetDeviceInfo(target.Holder.Owner.ID, devInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }
}
