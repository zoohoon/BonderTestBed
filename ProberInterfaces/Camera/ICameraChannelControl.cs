namespace ProberInterfaces
{
    public interface ICameraChannelControl
    {
        void WriteCameraPort(int chan, int port, bool isSet);
    }
}
