namespace ProberInterfaces.Foup
{
    public interface IFoupServiceCallback
    {
        bool IsFoupUsingByLoader();

        void RaiseFoupModuleStateChanged(FoupModuleInfo moduleInfo);

        void RaiseWaferOutDetected();

    }
    public enum FoupServiceTypeEnum
    {
        Direct,
        WCF,
        EMUL
    }
}
