using System.Threading.Tasks;

namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.Param;
    using ProberInterfaces.Utility;
    using System.Windows;
    using System.Windows.Input;
    public interface IInspectionControlVM
    {
        Task<EventCodeEnum> PageSwitched(object parameter = null);
        Task<EventCodeEnum> Cleanup(object parameter = null);

        #region //..Property
        IInspection Inspection { get; }
        ICamera CurCam { get; set; }
        object MainViewTarget { get; set; }
        object MiniViewTarget { get; set; }
        Visibility MainViewZoomVisible { get; set; }
        Visibility MiniViewZoomVisible { get; set; }
        double SystemXShiftValue { get; set; }
        double SystemYShiftValue { get; set; }
        bool ToggleSetIndex { get; set; }
        bool SetFromToggle { get; set; }
        long MapIndexX { get; set; }
        long MapIndexY { get; set; }
        #endregion

        InspcetionDataDescription GetInscpetionInfo();
        //ICommand ViewSwapCommand { get; }
        //ICommand PlusCommand { get; }
        //ICommand MinusCommand { get; }
        //IAsyncCommand SetFromCommand { get; }
        //IAsyncCommand ApplyCommand { get; }
        //IAsyncCommand ClearCommand { get; }
        //IAsyncCommand PrevDutCommand { get; }
        //IAsyncCommand NextDutCommand { get; }
        //IAsyncCommand PadPrevCommand { get; }
        //IAsyncCommand PadNextCommand { get; }
        //IAsyncCommand ManualSetIndexCommand { get; }
        //IAsyncCommand PinAlignCommand { get; }
        //IAsyncCommand WaferAlignCommand { get; }

        ICommand ChangeXManualCommand { get; }
        ICommand ChangeYManualCommand { get; }
        //IAsyncCommand SavePadsCommand { get;}

        void Inspection_ChangeXManualIndex(long index);
        void Inspection_ChangeYManualIndex(long index);

        #region Remote commands
        Task<EventCodeEnum> CheckPMShiftLimit(double checkvalue);
        Task SetFromRemoteCommand();
        Task SaveRemoteCommand(InspcetionDataDescription info);
        Task ApplyRemoteCommand();
        Task SystemApplyRemoteCommand();
        Task ClearRemoteCommand();
        Task SystemClearRemoteCommand();
        Task PrevDutRemoteCommand();
        Task NextDutRemoteCommand();
        Task PadPrevRemoteCommand();
        Task PadNextRemoteCommand();
        //Task ManualSetIndexRemoteCommand();
        Task PinAlignRemoteCommand();
        Task WaferAlignRemoteCommand();
        Task SavePadsRemoteCommand();

        Task SaveTempOffsetRemoteCommand(ObservableDictionary<double, CatCoordinates> table);
        #endregion
    }
}
