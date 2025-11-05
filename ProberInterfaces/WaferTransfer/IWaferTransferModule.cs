using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces.WaferTransfer
{
    public interface IWaferTransferModule : IStateModule
    {
        WaferTransferSystemParam SystemParam { get; }

        void SelfRecovery();

        void ClearErrorState();

        bool NeedToRecovery { get; }

        bool StopAfterTransferDone { get; set; }
        WaferTransferProcStateEnum GetProcModuleState();
        WaferTransferTypeEnum GetProcModuleTransferType();
        bool TransferBrake { get; set; }
    }

    public enum WaferTransferProcStateEnum
    {
        IDLE,
        RUNNING,
        DONE,
        PENDING,
        SUSPENDED,
        ERROR
    }

    public interface IWaferTransferProcModule : IFactoryModule
    {
        WaferTransferTypeEnum TransferType { get; }

        WaferTransferModeEnum TransferMode { get; }

        WaferTransferProcStateEnum State { get; }

        void InitState();

        void Execute();
        void SelfRecovery();
    }

    public enum WaferTransferTypeEnum
    {
        IDLE,
        Loading,
        Unloading,
        CardLoading,
        CardUnLoding
    }

    public enum WaferTransferModeEnum
    {
        TransferByThreeLeg,
    }

    [Serializable]
    public class WaferTransferSystemParam : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private WaferTransferModeEnum _WaferTransferMode;
        public WaferTransferModeEnum WaferTransferMode
        {
            get { return _WaferTransferMode; }
            set { _WaferTransferMode = value; RaisePropertyChanged(); }
        }

    }
}
