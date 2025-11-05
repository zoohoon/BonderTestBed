namespace ProberInterfaces.Proxies
{
    public interface ILotOPModuleProxy : IProberProxy
    {
        new void InitService();

        void SetDeviceName(string devicename);
    }
}
