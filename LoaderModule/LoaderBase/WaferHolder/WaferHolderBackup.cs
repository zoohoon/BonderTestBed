using System;

using LoaderParameters;
using LogModule;
using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// WaferHolderBackupStateEnum
    /// </summary>
    public enum WaferHolderBackupStateEnum
    {
        /// <summary>
        /// NOT_EXIST
        /// </summary>
        NOT_EXIST,
        /// <summary>
        /// EXIST
        /// </summary>
        EXIST
    }

    /// <summary>
    /// WaferHolderBackup
    /// </summary>
    public abstract class WaferHolderBackup
    {
        internal WaferHolder Holder { get; set; }

        internal WaferHolderBackup(WaferHolder holder)
        {
            try
            {
                this.Holder = holder;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        internal void StateTransition(WaferHolderBackup container)
        {
            try
            {
                this.Holder.Backup = container;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            try
            {
                Holder.BackupTransferObject = null;
                StateTransition(new WaferHolderBackupNotExistState(Holder));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// WaferHolderBackupState
        /// </summary>
        public abstract WaferHolderBackupStateEnum State { get; }

        /// <summary>
        /// Put
        /// </summary>
        /// <param name="transferObject"></param>
        public abstract void Put(TransferObject transferObject);

        /// <summary>
        /// Takeout
        /// </summary>
        /// <returns></returns>
        public abstract TransferObject Takeout();
    }

    internal class WaferHolderBackupNotExistState : WaferHolderBackup
    {
        public WaferHolderBackupNotExistState(WaferHolder holder) : base(holder) { }

        public override WaferHolderBackupStateEnum State => WaferHolderBackupStateEnum.NOT_EXIST;

        public override void Put(TransferObject transferObject)
        {
            try
            {
                if (transferObject == null)
                    return;

                Holder.BackupTransferObject = transferObject.Clone<TransferObject>();
                StateTransition(new WaferHolderBackupExistState(Holder));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override TransferObject Takeout()
        {
            throw new Exception("No Access");
        }
    }

    internal class WaferHolderBackupExistState : WaferHolderBackup
    {
        public WaferHolderBackupExistState(WaferHolder holder) : base(holder) { }

        public override WaferHolderBackupStateEnum State => WaferHolderBackupStateEnum.EXIST;

        public override void Put(TransferObject transferObject)
        {
            try
            {
                if (transferObject == null)
                    return;

                Holder.BackupTransferObject = transferObject.Clone<TransferObject>();
                //StateTransition(new WaferHolderBackupExistState(Holder));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override TransferObject Takeout()
        {
            var backupObject = Holder.BackupTransferObject;

            try
            {
                Holder.BackupTransferObject = null;
                StateTransition(new WaferHolderBackupNotExistState(Holder));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return backupObject;
        }
    }
}
