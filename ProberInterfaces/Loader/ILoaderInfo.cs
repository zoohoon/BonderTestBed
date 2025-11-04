namespace ProberInterfaces.Loader
{
    public interface ILoaderInfo
    {

    }
    public interface IWaferHolder 
    {
        EnumSubsStatus Status { get; }
        TransferObject TransferObject { get; }
    }
}
