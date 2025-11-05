namespace ProberInterfaces.Enum
{
    public enum EnumPrevNext
    {
        Prev = 0,
        Next
    }

    public enum EnumFileTransferDirection
    {
        StageToLoader = 0,
        LoaderToStage,
        LoaderToNetwork,
        NetworkToLoader,
        StageToNetwork,
        NetworkToStage
    }
}
