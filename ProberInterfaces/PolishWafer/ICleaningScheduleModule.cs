namespace ProberInterfaces.PolishWafer
{
    public interface ICleaningScheduleModule
    {
        bool RequiredRunningStateTransition(bool CanTriggered = false);
    }

    public enum CleaningScheduleState
    {
        IDLE,
        NEED,
        NOTNEED,
        RUNNING,
    }
}
