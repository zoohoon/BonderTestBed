namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.PolishWafer;
    using ProberInterfaces.Proxies;

    public interface IPolishWaferModuleProxy : IFactoryModule, IProberProxy
    {
        new void InitService();

        EventCodeEnum DoCentering(IPolishWaferCleaningParameter param);
        EventCodeEnum DoFocusing(IPolishWaferCleaningParameter param);
        EventCodeEnum DoCleaning(IPolishWaferCleaningParameter param);
        byte[] GetPolishWaferParam();
        void SetPolishWaferIParam(byte[] param);
        bool PWIntervalhasLotstart(int index = -1);
    }
}
