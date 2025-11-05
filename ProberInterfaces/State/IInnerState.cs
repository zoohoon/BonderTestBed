using ProberErrorCode;

namespace ProberInterfaces.State
{
    public interface IInnerState
    {
        ModuleStateEnum GetModuleState();
        EventCodeEnum Execute();
        EventCodeEnum Pause();
        EventCodeEnum End();
        EventCodeEnum ClearState();
        EventCodeEnum Resume();
        EventCodeEnum Abort();
    }

    ////temp
    //public interface IHasInnerState
    //{
    //    IInnerState InnerState { get; }
    //    IInnerState PreInnerState { get; }
    //    EventCodeEnum InnerStateTransition(IInnerState state);
    //}

}
