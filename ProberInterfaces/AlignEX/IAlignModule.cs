
namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.State;

    public interface ISubModule : IParamNode, ITemplateModule
    {
        bool IsExecute();
        EventCodeEnum ClearData();
        EventCodeEnum Execute();
        EventCodeEnum Recovery();
        EventCodeEnum ExitRecovery();
        SubModuleStateEnum GetState();
        MovingStateEnum GetMovingState();
        new EventCodeEnum InitModule();
        void ClearState();
      
    }
    public interface IProcessingModule : ISubModule
    {
        SubModuleMovingStateBase MovingState { get; set; }
        SubModuleStateBase SubModuleState { get; set; }

        EventCodeEnum DoExecute();

        EventCodeEnum DoClearData();

        EventCodeEnum DoRecovery();

        EventCodeEnum DoExitRecovery();

    }

    public interface ISchedulingModule: ITemplateModule
    {
        bool IsExecute();
    }
}
