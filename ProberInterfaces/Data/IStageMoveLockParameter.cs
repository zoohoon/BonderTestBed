namespace ProberInterfaces.Data
{
    public interface IStageMoveLockParameter
    {
        Element<bool> LockAfterInit { get; set; }
        Element<bool> DoorInterLockEnable { get; set; }
    }
}
