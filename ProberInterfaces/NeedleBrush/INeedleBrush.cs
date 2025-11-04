namespace ProberInterfaces.NeedleBrush
{
    public enum NeedleBrushStateEnum
    {
        UNDEFINED = 0,
        IDLE,
        DONE,
        ERROR,
        RUNNING,
        RECOVERY,
        SUSPENDED,
        ABORT,
        PAUSED
    }

    public interface INeedleBrushModule : IStateModule, ITemplateStateModule
    {
    }
}
