using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.Bonder
{
    public interface IBonderModule : IStateModule
    {

    }

    public enum BonderTransferProcStateEnum
    {
        IDLE,
        RUNNING,
        DONE,
        PENDING,
        SUSPENDED,
        ERROR
    }
    public interface IBonderProcModule : IFactoryModule
    {
        BonderTransferTypeEnum TransferType { get; }

        BonderModeEnum TransferMode { get; }

        BonderTransferProcStateEnum State { get; }

        void InitState();

        void Execute();

        void SelfRecovery();
    }
    public enum BonderTransferTypeEnum
    {
        IDLE,
        PICKING,
        ROTATING,
        PLACING,
        DIEALIGN,
        STAGE
    }

    public enum BonderModeEnum
    {
        BONDER
    }

    [Serializable]
    public class BonderSystemParam : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private BonderModeEnum _BonderTransferMode;
        public BonderModeEnum BonderTransferMode
        {
            get { return _BonderTransferMode; }
            set { _BonderTransferMode = value; RaisePropertyChanged(); }
        }
    }
}
