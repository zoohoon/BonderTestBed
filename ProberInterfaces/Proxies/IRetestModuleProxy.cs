namespace ProberInterfaces.Proxies
{
    public interface IRetestModuleProxy : IFactoryModule, IProberProxy
    {
        byte[] GetRetestParam();
        void SetRetestParam(byte[] param);
    }
}
