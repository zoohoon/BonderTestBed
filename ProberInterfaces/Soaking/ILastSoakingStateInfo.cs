namespace ProberInterfaces.Soaking
{
    public interface ILastSoakingStateInfo
    {
        void TraceLastSoakingStateInfo(bool bStart);
        void ResetLastSoakingStateInfo();
        void SaveLastSoakingStateInfo(SoakingStateEnum state = SoakingStateEnum.PREPARE);
        object LoadLastSoakingStateInfo();
    }
}
