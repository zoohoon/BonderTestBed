namespace ProberInterfaces.Vision
{

    using System.Collections.ObjectModel;

    public interface ICameraParameter : IParamNode
    {
        Element<int> DigiNumber { get; set; }
        Element<int> ChannelNumber { get; set; }
        Element<string> ChannelDesc { get; set; }
        Element<EnumProberCam> ChannelType { get; set; }
        Element<FlipEnum> VerticalFlip { get; set; }
        Element<FlipEnum> HorizontalFlip { get; set; }
        Element<double> RatioX { get; set; }
        Element<double> RatioY { get; set; }
        //Element<double> CamAngle { get; set; }
        Element<int> GrabSizeX { get; set; }
        Element<int> GrabSizeY { get; set; }
        Element<int> Band { get; set; }
        Element<int> ColorDept { get; set; }
        //Element<int> PixelDirectionX { get; set; }
        //Element<int> PixelDirectionY { get; set; }
        Element<ObservableCollection<LightChannelType>> LightsChannels { get; set; }
        Element<double> Rotate { get; set; }
        //Element<bool> DoubleGrabEnable { get; }
    }
}
