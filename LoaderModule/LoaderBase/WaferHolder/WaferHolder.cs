using System;

using ProberInterfaces;
using LoaderParameters;
using LogModule;
using ProberInterfaces.Enum;
using ProberErrorCode;
using ProberInterfaces.Loader;

namespace LoaderBase
{
    /// <summary>
    /// WaferHolder 를 정의합니다.
    /// </summary>
    [Serializable]
    public class WaferHolder : IWaferHolder
    {
        internal WaferHolderStatus StatusObj { get; set; }

        internal WaferHolderBackup Backup { get; set; }

        public TransferObject CurrentWaferInfo { get; set; }
        /// <summary>
        /// 인스턴스를 생성합니다.
        /// </summary>
        public WaferHolder()
        {
            StatusObj = new WaferHolderUndefinedStatus(this);

            Backup = new WaferHolderBackupNotExistState(this);
        }

        /// <summary>
        /// IsWaferOnHandler tri 나 bernoulli 에 웨이퍼가 있으면 true
        /// </summary>
        public bool IsWaferOnHandler { get; set; }

        /// <summary>
        /// Owner
        /// </summary>
        public IWaferOwnable Owner { get; set; }

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
            Backup.Put(TransferObject);

            if (Status == EnumSubsStatus.EXIST)
            {
                return TransferObject.Clone() as TransferObject;

            }
            else if (Status == EnumSubsStatus.HIDDEN)
            {
                if (TransferObject != null)
                {
                    return TransferObject.Clone() as TransferObject;
                }
            }
            else if (Status == EnumSubsStatus.UNKNOWN)
            {
                if (TransferObject != null)
                {
                    return TransferObject.Clone() as TransferObject;
                }
                else if (Backup.State == WaferHolderBackupStateEnum.EXIST) 
                {
                    if (Backup.Holder != null) 
                    {
                        if (Backup.Holder.BackupTransferObject != null) 
                        {
                            return Backup.Holder.BackupTransferObject.Clone() as TransferObject;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// SetOwner
        /// </summary>
        /// <param name="owner"></param>
        public void SetOwner(IWaferOwnable owner)
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
                        Backup.Put(info.Substrate);
                        break;
                    case EnumSubsStatus.HIDDEN:
                        SetLoad(info.Substrate);
                        Backup.Put(info.Substrate);
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
                if (Backup.State == WaferHolderBackupStateEnum.EXIST)
                {
                    newSub = Backup.Takeout();
                    newSub.CleanPreAlignState("WaferHolder SetAllocate.");

                    SetLoad(newSub);
                }
                else
                {
                    newSub = CreateTransferObject(Owner.ID);

                    SetLoad(newSub);
                }

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
                to.Type.Value = SubstrateTypeEnum.Wafer;
                to.Size.Value = SubstrateSizeEnum.UNDEFINED;
                to.WaferType.Value = EnumWaferType.UNDEFINED;
                to.PreAlignState = PreAlignStateEnum.NONE;
                to.OCRReadState = OCRReadStateEnum.NONE;
                to.WaferState = EnumWaferState.UNPROCESSED;

                if (Owner is IWaferSupplyModule)
                {
                    var wsm = Owner as IWaferSupplyModule;
                    if (wsm.ModuleType != ModuleTypeEnum.BUFFER && wsm.ModuleType != ModuleTypeEnum.ARM) 
                    {
                        var devInfo = wsm.GetSourceDeviceInfo();
                        to.SetDeviceInfo(Owner.ID, devInfo);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return to;
        }

        /// <summary>
        /// ChangeDeviceInfo after create
        /// </summary>
        /// <param name="transferObject"></param>
        public void ChangeDeviceInfo(TransferObject transferObject)
        {
            try
            {
                if (Owner is IWaferSupplyModule)
                {
                    var wsm = Owner as IWaferSupplyModule;

                    var devInfo = wsm.GetSourceDeviceInfo();
                    transferObject.SetDeviceInfo(Owner.ID, devInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
                if(transferObject==null)
                {
                    return;
                }
                StatusObj.SetLoad(transferObject);
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

        public void SetNull()
        {
            try
            {
                StatusObj.SetNull();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        /// <summary>
        /// SetUnknown
        /// </summary>
        public void SetUnknown(TransferObject transferObject = null)
        {
            try
            {
                StatusObj.SetUnknown(transferObject);
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
        public void SetPosition(IWaferLocatable pos)
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
        public void SetTransfered(IWaferOwnable target)
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
        public void SetOriginID(IWaferOwnable target)
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
