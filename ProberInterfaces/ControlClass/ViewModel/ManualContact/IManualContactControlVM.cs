using System.Threading.Tasks;

namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.PnpSetup;
    using RelayCommandBase;

    public interface IManualContactControlVM
    {

        //=====
        #region //..Jog
        PNPCommandButtonDescriptor PadJogLeft { get; set; }
        PNPCommandButtonDescriptor PadJogRight { get; set; }
        PNPCommandButtonDescriptor PadJogUp { get; set; }
        PNPCommandButtonDescriptor PadJogDown { get; set; }
        PNPCommandButtonDescriptor PadJogLeftUp { get; set; }
        PNPCommandButtonDescriptor PadJogRightUp { get; set; }
        PNPCommandButtonDescriptor PadJogLeftDown { get; set; }
        PNPCommandButtonDescriptor PadJogRightDown { get; set; }
        PNPCommandButtonDescriptor PadJogSelect { get; set; }
        #endregion
        int Tab_Idx { get; set; }
        bool IsVisiblePanel { get; set; }
        double WantToMoveZInterval { get; set; }
        bool CanUsingManualContactControl { get; set; }
        IManualContact MCM { get; set; }
        IProbingModule ProbingModule { get; set; }
        IAsyncCommand ControlLoadedCommand { get; }
        IAsyncCommand FirstContactSetCommand { get; }
        IAsyncCommand AllContactSetCommand { get; }
        IAsyncCommand ResetContactStartPositionCommand { get; }
        IAsyncCommand OverDriveTBClickCommand { get; }
        IAsyncCommand ChangeZUpStateCommand { get; }
        IAsyncCommand MoveToWannaZIntervalPlusCommand { get; }
        IAsyncCommand WantToMoveZIntervalTBClickCommand { get; }
        IAsyncCommand MoveToWannaZIntervalMinusCommand { get; }
        IAsyncCommand GoToInspectionViewCommand { get; }
        IAsyncCommand SetOverDriveCommand { get; }
        IAsyncCommand CPC_Z1_ClickCommand { get; }
        IAsyncCommand CPC_Z2_ClickCommand { get; }
        Task<EventCodeEnum> PageSwitched(object parameter = null);
        Task<EventCodeEnum> Cleanup(object parameter = null);
        ManaulContactDataDescription GetManaulContactSetupDataDescription();
        bool GetManaulContactMovingStage();
        void MCMChangeOverDrive(string overdrive);

        #region Remote commands

        Task FirstContactSetRemoteCommand();
        Task AllContactSetRemoteCommand();
        Task ResetContactStartPositionRemoteCommand();
        Task OverDriveTBClickRemoteCommand();
        Task ChangeZUpStateRemoteCommand();
        Task MoveToWannaZIntervalPlusRemoteCommand();
        Task WantToMoveZIntervalTBClickRemoteCommand();
        Task SetOverDriveRemoteCommand();
        Task MoveToWannaZIntervalMinusRemoteCommand();

        Task CPC_Z1_ClickRemoteCommand();
        void MCMChangeCPC_Z1(string z1);
        Task CPC_Z2_ClickRemoteCommand();
        void MCMChangeCPC_Z2(string z2);
        void GetOverDriveFromProbingModule();
        #endregion
    }
}
