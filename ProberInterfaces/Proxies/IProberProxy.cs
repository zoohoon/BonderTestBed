namespace ProberInterfaces.Proxies
{
    public interface IProberProxy
    {
        //bool IsOpened();
        void InitService();
        void DeInitService();
        bool IsServiceAvailable();
    }
}
