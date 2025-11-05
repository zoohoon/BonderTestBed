using System;

using LoaderParameters;
using LogModule;
using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// CardHolderBackupStateEnum
    /// </summary>
    public enum CardHolderBackupStateEnum
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
    /// CardHolderBackup
    /// </summary>
    public abstract class CardHolderBackup
    {
        internal CardHolder Holder { get; set; }

        internal CardHolderBackup(CardHolder holder)
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

        internal void StateTransition(CardHolderBackup container)
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
                StateTransition(new CardHolderBackupNotExistState(Holder));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// CardHolderBackupState
        /// </summary>
        public abstract CardHolderBackupStateEnum State { get; }

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

    internal class CardHolderBackupNotExistState : CardHolderBackup
    {
        public CardHolderBackupNotExistState(CardHolder holder) : base(holder) { }

        public override CardHolderBackupStateEnum State => CardHolderBackupStateEnum.NOT_EXIST;

        public override void Put(TransferObject transferObject)
        {
            try
            {
                if (transferObject == null)
                    return;

                Holder.BackupTransferObject = transferObject.Clone<TransferObject>();
                StateTransition(new CardHolderBackupExistState(Holder));
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

    internal class CardHolderBackupExistState : CardHolderBackup
    {
        public CardHolderBackupExistState(CardHolder holder) : base(holder) { }

        public override CardHolderBackupStateEnum State => CardHolderBackupStateEnum.EXIST;

        public override void Put(TransferObject transferObject)
        {
            try
            {
                if (transferObject == null)
                    return;

                Holder.BackupTransferObject = transferObject.Clone<TransferObject>();
                //StateTransition(new CardHolderBackupExistState(Holder));
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
                StateTransition(new CardHolderBackupNotExistState(Holder));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return backupObject;
        }
    }
}
