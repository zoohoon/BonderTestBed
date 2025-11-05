
namespace ProberInterfaces
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using ProberInterfaces.Param;
    using ProberInterfaces.Vision;
    using System.Windows;
    using ProberErrorCode;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.ServiceModel;

    public delegate void ImageUpdatedEventHandler(ImageBuffer image);
    public delegate void ByteImageUpdatedEventHandler(ICamera cam, byte[] vs);
    public delegate void CameraUpdatedEventHandler(byte[] cameradata, ImageBuffer image);
    [ServiceContract]
    public interface ICamera : IFactoryModule, INotifyPropertyChanged, IModule
    {
        ImageBuffer.DrawDisplay DrawDisplayDelegate { get; set; }
        List<LightChannelType> LightsChannels { get; set; }
        ObservableCollection<ILightChannel> LightModules { get; }
        IDisplay DisplayService { get; set; }
        CameraChannelType CameraChannel { get; set; }
        CatCoordinates CamSystemPos { get; }
        UserIndex CamSystemUI { get; }
        MachineIndex CamSystemMI { get; }
        NCCoordinate CamSystemNC { get; }
        ImageBuffer CamCurImage { get; }
        bool UpdateOverlayFlag { get; set; }
        Visibility EnableGetFocusValueFlag { get; set; }
        bool IsMovingPos { get; set; }
        //ImageBuffer Grab_SingleShot();
        //ImageBuffer WaitSingleShot();
        CatCoordinates GetCurCoordPos();
        UserIndex GetCurCoordIndex();
        MachineIndex GetCurCoordMachineIndex();
        NCCoordinate GetCurCoorNCCoord();
        ICameraParameter Param { get; set; }
        int GetDigitizerIndex();
        EnumProberCam GetChannelType();
        double GetRatioX();
        double GetRatioY();
        double GetGrabSizeWidth();
        double GetGrabSizeHeight();
        FlipEnum GetVerticalFlip();
        FlipEnum GetHorizontalFlip();
        void SetVerticalFlip(FlipEnum fliptype);
        void SetHorizontalFlip(FlipEnum fliptype);

        //void ImageGrabbed(ImageBuffer img);
        //void ImageProcessed(ImageBuffer img);
        //void InitGrab();
        //void InitLights();
        void SetDefault(EnumProberCam chntype,
            EnumGrabberRaft grabberType = EnumGrabberRaft.UNDIFIND,
            int diginum = 0, int channelnum = 0);
        //void StartGrab();
        //void StopGrab();
        EventCodeEnum SetLight(EnumLightType type, ushort intensity);
        int SetLightNoLUT(EnumLightType type, ushort intensity);
        int SetupLightLookUpTable(EnumLightType type, IntListParam lightLUT);
        //int SwitchCamera();
        [OperationContract]
        void GetCurImage(out ImageBuffer curimg);

        Task<MachineIndex> SeletedMoveToPos(int MoveX, int MoveY);
        //EventCodeEnum MoveToCoord(int xMove, int yMove);

        //void ViewDisplay();

        int GetLight(EnumLightType type);

        EventCodeEnum DrawOverlayDisplay();
        EventCodeEnum InDrawOverlayDisplay();
        void SetCamSystemPos(CatCoordinates coord);
        void SetCamSystemUI(UserIndex uindex);
        void SetCamSystemMI(MachineIndex mindex);
        void BackupLights(bool turnoff = false);
        void RestoreLights();
    }
}
