namespace ProberInterfaces
{
    using ProberInterfaces.LightJog;
    using System.Windows.Input;

    public enum CameraBtnType { UNDEFINED, High, Low, High2X, Low2X }
    public interface ILightJobViewModel : IFactoryModule
    {
        bool IsUseNC { get; set; }
        ICommand HighBtnEventHandler { get; set; }
        ICommand LowBtnEventHandler { get; set; }
        CameraBtnType CurSelectedMag { get; set; }
        int CurCameraLightValue { get; set; }
        ILightChannel CurLightChannel { get; }
        EnumLightType CurCameraLightType { get; }
        void InitCameraJog(IUseLightJog module, EnumProberCam camtype = EnumProberCam.UNDEFINED);
        void ChangeCamPosition(EnumProberCam cam);
        bool UpdateCamera(EnumProberCam cam);
        void UpdateCameraLightValue();

        //bool SetLightType(EnumLightType lighttype);
        //bool SetLightValue(int intensity);
    }
}
