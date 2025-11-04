namespace ProberInterfaces.State
{

    public enum EnumEnableState
    {
        UNDEFINED = 0,
        IDLE,
        ENABLE,
        DISABLE,
        MUST,
        MUSTNOT
    }

    public interface IEnableState
    {
        EnumEnableState GetEnableState();
        //void ChangeEnableState(EnableStateBase state);
        IEnableState SetIdle(IEnableState state);
        IEnableState SetEnable(IEnableState state);
        IEnableState SetDisable(IEnableState state);
        IEnableState SetMust(IEnableState state);
        IEnableState SetMustNot(IEnableState state);
    }


}
