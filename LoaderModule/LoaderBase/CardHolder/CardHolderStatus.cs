using System;
using System.Runtime.CompilerServices;

using ProberInterfaces;
using LogModule;

namespace LoaderBase
{
    [Serializable]
    internal abstract class CardHolderStatus
    {
        internal CardHolderStatus(CardHolder slot)
        {
            this.Holder = slot;
        }

        internal CardHolder Holder { get; private set; }

        internal void ChangeStatus(EnumSubsStatus status)
        {
            try
            {
                if (Holder.StatusObj.Status != status)
                {
                    switch (status)
                    {
                        case EnumSubsStatus.UNKNOWN:
                            Holder.StatusObj = new CardHolderUnknownStatus(Holder);
                            break;
                        case EnumSubsStatus.UNDEFINED:
                            Holder.StatusObj = new CardHolderUndefinedStatus(Holder);
                            break;
                        case EnumSubsStatus.NOT_EXIST:
                            Holder.StatusObj = new CardHolderNotExistStatus(Holder);
                            break;
                        case EnumSubsStatus.EXIST:
                            Holder.StatusObj = new CardHolderExistStatus(Holder);
                            break;
                        case EnumSubsStatus.HIDDEN:
                            Holder.StatusObj = new CardHolderHiddenStatus(Holder);
                            break;
                        case EnumSubsStatus.CARRIER:
                            Holder.StatusObj = new CardHolderCarrierExistStatus(Holder);
                            break;
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

        public virtual void SetLoad(TransferObject substrate)
        {
            ThrowInvalidState();
        }
        public virtual void SetCarrier(TransferObject substrate)
        {
            ThrowInvalidState();
        }
        public virtual void SetUnknown()
        {
            ThrowInvalidState();
        }
        public virtual void SetHidden()
        {
            ThrowInvalidState();
        }

        public virtual void SetPosition(ICardLocatable pos)
        {
            ThrowInvalidState();
        }

        public virtual void SetTransfered(ICardOwnable target)
        {
            ThrowInvalidState();
        }
        public virtual void SetOriginID(ICardOwnable target)
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

                Holder.TransferObject = transferObject;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        internal void SetUnknownFunc()
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

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        internal void SetPositionFunc(ICardLocatable pos)
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
    internal class CardHolderUndefinedStatus : CardHolderStatus
    {
        public CardHolderUndefinedStatus(CardHolder slot) : base(slot) { }

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
        public override void SetCarrier(TransferObject transferObject)
        {
            try
            {
                SetLoadFunc(transferObject);
                ChangeStatus(EnumSubsStatus.CARRIER);
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

        public override void SetUnknown()
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
    internal class CardHolderNotExistStatus : CardHolderStatus
    {
        public CardHolderNotExistStatus(CardHolder slot) : base(slot) { }

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
        public override void SetCarrier(TransferObject transferObject)
        {
            try
            {
                SetLoadFunc(transferObject);
                ChangeStatus(EnumSubsStatus.CARRIER);
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

        public override void SetUnknown()
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
    internal class CardHolderExistStatus : CardHolderStatus
    {
        public CardHolderExistStatus(CardHolder slot) : base(slot) { }

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
        public override void SetCarrier(TransferObject transferObject)
        {
            try
            {
                SetLoadFunc(transferObject);
                ChangeStatus(EnumSubsStatus.CARRIER);
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

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //NoWork
        }

        public override void SetUnknown()
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

        public override void SetPosition(ICardLocatable pos)
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

        public override void SetTransfered(ICardOwnable target)
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

        public override void SetOriginID(ICardOwnable target)
        {
            try
            {
                var transferObj = Holder.TransferObject;

                if (target.Holder.Owner is ICardSupplyModule)
                {
                    var wsm = target.Holder.Owner as ICardSupplyModule;

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
    internal class CardHolderCarrierExistStatus : CardHolderStatus
    {
        public CardHolderCarrierExistStatus(CardHolder slot) : base(slot) { }

        public override EnumSubsStatus Status => EnumSubsStatus.CARRIER;

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
        public override void SetCarrier(TransferObject transferObject)
        {
            try
            {
           
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

        public override void SetUnknown()
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

        public override void SetPosition(ICardLocatable pos)
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

        public override void SetTransfered(ICardOwnable target)
        {
            try
            {
                var transferObj = Holder.TransferObject;

                SetUnload();

                target.Holder.SetCarrier(transferObj);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetOriginID(ICardOwnable target)
        {
            try
            {
                var transferObj = Holder.TransferObject;

                if (target.Holder.Owner is ICardSupplyModule)
                {
                    var wsm = target.Holder.Owner as ICardSupplyModule;

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
    internal class CardHolderUnknownStatus : CardHolderStatus
    {
        public CardHolderUnknownStatus(CardHolder slot) : base(slot) { }

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
        public override void SetCarrier(TransferObject transferObject)
        {
            try
            {
                SetLoadFunc(transferObject);
                ChangeStatus(EnumSubsStatus.CARRIER);
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

        public override void SetUnknown()
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
    }

    [Serializable]
    internal class CardHolderHiddenStatus : CardHolderStatus
    {
        public CardHolderHiddenStatus(CardHolder slot) : base(slot) { }

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
        public override void SetCarrier(TransferObject transferObject)
        {
            try
            {
                SetLoadFunc(transferObject);
                ChangeStatus(EnumSubsStatus.CARRIER);
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

        public override void SetUnknown()
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

        public override void SetPosition(ICardLocatable pos)
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

        public override void SetTransfered(ICardOwnable target)
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

        public override void SetOriginID(ICardOwnable target)
        {
            try
            {
                var transferObj = Holder.TransferObject;

                if (target.Holder.Owner is ICardSupplyModule)
                {
                    var wsm = target.Holder.Owner as ICardSupplyModule;

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

}
