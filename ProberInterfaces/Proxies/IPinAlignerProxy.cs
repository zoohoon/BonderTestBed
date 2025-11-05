
namespace ProberInterfaces.Proxies
{
    public interface IPinAlignerProxy : IFactoryModule, IProberProxy
    {
        new void InitService();

        byte[] GetPinAlignerParam();
        void SetPinAlignerIParam(byte[] param);

        //byte[] GetTemplateParam();
    }
}
