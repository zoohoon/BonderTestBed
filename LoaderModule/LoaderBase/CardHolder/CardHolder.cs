using System;

using ProberInterfaces;
using LoaderParameters;
using LogModule;
using ProberInterfaces.Enum;

namespace LoaderBase
{
    /// <summary>
    /// CardHolder 를 정의합니다.
    /// </summary>
    [Serializable]
    public class CardHolder
    {
        internal CardHolderStatus StatusObj { get; set; }

        internal CardHolderBackup Backup { get; set; }

        public TransferObject CurrentCardInfo { get; set; }
        /// <summary>
        /// 인스턴스를 생성합니다.
        /// </summary>
        public CardHolder()
        {
            StatusObj = new CardHolderUndefinedStatus(this);

            Backup = new CardHolderBackupNotExistState(this);
        }
        /// <summary>
        /// Owner
        /// </summary>
        public bool isCardAttachHolder { get; set; } = true;

        /// <summary>
        /// Owner
        /// </summary>
        public ICardOwnable Owner { get; set; }

        /// <summary>
        /// BackupTransferObject
        /// </summary>
        public TransferObject BackupTransferObject { get; internal set; }

        /// <summary>
        /// TransferObject
        /// </summary>
        public TransferObject TransferObject { get; internal set; }

        /// <summary>
        /// GetTransferObjectClone
        /// </summary>
        /// <returns></returns>
        public TransferObject GetTransferObjectClone()
        {
            if (Status == EnumSubsStatus.EXIST|| Status == EnumSubsStatus.CARRIER)
            {
                return TransferObject.Clone() as TransferObject;
            }
            return null;
        }

        /// <summary>
        /// SetOwner
        /// </summary>
        /// <param name="owner"></param>
        public void SetOwner(ICardOwnable owner)
        {
            this.Owner = owner;
        }

        /// <summary>
        /// Status
        /// </summary>
        public EnumSubsStatus Status => StatusObj.Status;

        /// <summary>
        /// RecoveryBackupData
        /// </summary>
        /// <param name="info"></param>
        public void RecoveryBackupData(HolderModuleInfo info)
        {
            try
            {
                if (info == null)
                    return;

                switch (info.WaferStatus)
                {
                    case EnumSubsStatus.UNKNOWN:
                        SetUnknown();
                        Backup.Put(info.Substrate);
                        break;
                    case EnumSubsStatus.UNDEFINED:
                        SetUndefined();
                        break;
                    case EnumSubsStatus.NOT_EXIST:
                        SetUnload();
                        break;
                    case EnumSubsStatus.EXIST:
                        SetLoad(info.Substrate);
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// SetAllocate
        /// </summary>
        public void SetAllocate()
        {
            try
            {

                TransferObject newSub;
                if (Backup.State == CardHolderBackupStateEnum.EXIST)
                {
                    newSub = Backup.Takeout();
                    newSub.CleanPreAlignState(reason: "CardHolder Allocated.");
                    newSub.WaferType.Value = EnumWaferType.CARD;
                    SetLoad(newSub);
                }
                else
                {
                    newSub = CreateTransferObject(Owner.ID);
                    newSub.WaferType.Value = EnumWaferType.CARD;
                    SetLoad(newSub);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetAllocateCarrier()
        {
            try
            {

                TransferObject newSub;
                newSub = CreateTransferObject(Owner.ID);
                newSub.WaferType.Value = EnumWaferType.CARD;
                SetCarrier(newSub);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private TransferObject CreateTransferObject(ModuleID holderID)
        {
            TransferObject to = new TransferObject();

            try
            {

                to.ID.Value = Guid.NewGuid().ToString("N");
                to.OriginHolder = holderID;
                to.CurrPos = holderID;
                to.CurrHolder = holderID;
                to.Type.Value = SubstrateTypeEnum.Card;
                to.Size.Value = SubstrateSizeEnum.UNDEFINED;
                to.WaferType.Value = EnumWaferType.CARD;
                to.PreAlignState = PreAlignStateEnum.NONE;
                to.OCRReadState = OCRReadStateEnum.NONE;
                to.WaferState = EnumWaferState.UNPROCESSED;

                if (Owner is ICardSupplyModule)
                {
                    var wsm = Owner as ICardSupplyModule;

                    var devInfo = wsm.GetSourceDeviceInfo();
                    to.SetDeviceInfo(Owner.ID, devInfo);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return to;
        }

        /// <summary>
        /// SetUndefined
        /// </summary>
        public void SetUndefined()
        {
            try
            {
                StatusObj.SetUndefined();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// SetLoad
        /// </summary>
        /// <param name="transferObject"></param>
        public void SetLoad(TransferObject transferObject)
        {
            try
            {
                StatusObj.SetLoad(transferObject);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetCarrier(TransferObject transferObject)
        {
            try
            {
                StatusObj.SetCarrier(transferObject);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// SetUnload
        /// </summary>
        public void SetUnload()
        {
            try
            {
                StatusObj.SetUnload();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetHidden()
        {
            try
            {
                StatusObj.SetHidden();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// SetUnknown
        /// </summary>
        public void SetUnknown()
        {
            try
            {
                StatusObj.SetUnknown();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// SetPosition
        /// </summary>
        /// <param name="pos"></param>
        public void SetPosition(ICardLocatable pos)
        {
            try
            {
                StatusObj.SetPosition(pos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// SetTransfered
        /// </summary>
        /// <param name="target"></param>
        public void SetTransfered(ICardOwnable target)
        {
            try
            {
                StatusObj.SetTransfered(target);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetOriginID(ICardOwnable target)
        {
            try
            {
                StatusObj.SetOriginID(target);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
