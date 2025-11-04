using System.Threading.Tasks;

namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.WaferAlignEX;
    using System.IO;
    using System.Windows.Input;

    public interface IWaferMapMakerVM
    {
        HeightPointEnum HeightPoint { get; set; }
        ICamera CurCam { get; set; }
        double DieSizeX { get; set; }
        double DieSizeY { get; set; }
        double Thickness { get; set; }
        double EdgeMargin { get; set; }
        double NotchAngle { get; set; }
        double NotchAngleOffset { get; set; }
        WaferNotchTypeEnum NotchType { get; set; }
        EnumWaferSize WaferSize { get; set; }
        double WaferSize_Offset_um { get; set; }
        WaferSubstrateTypeEnum WaferSubstrateType { get; set; }
        IPhysicalInfo PhysicalInfo { get; set; }
        ICommand ApplyCreateWaferMapCommand { get; }
        ICommand MoveToWaferThicknessCommand { get; }
        ICommand AdjustWaferHeightCommand { get; }
        ICommand CmdImportWaferData { get; }
        Stream CSVFileStream { get; set; }
        string CSVFilePath { get; set; }

        bool IsCanChangeWaferSize { get; set; }
        Task<EventCodeEnum> Cleanup(object parameter = null);

        Task<EventCodeEnum>ApplyCreateWaferMap();
        Task<EventCodeEnum> ImportWaferData();

        Task MoveToWaferThicknessCommandFunc();
        Task AdjustWaferHeightCommandFunc();
        Task SelectionChangedFunc(object param);
    }
}
