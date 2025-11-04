using ProberInterfaces.Foup;

namespace FoupProcedureManagerProject
{
    public abstract class FoupProcedureStateBase
    {
        public abstract FoupProcedureStateEnum GetState();
    }

    public class FoupProcedureIdle : FoupProcedureStateBase
    {
        public override FoupProcedureStateEnum GetState()
        {
            return FoupProcedureStateEnum.IDLE;
        }
    }

    public class FoupProcedurePreSafetyError : FoupProcedureStateBase
    {
        public override FoupProcedureStateEnum GetState()
        {
            return FoupProcedureStateEnum.PreSafetyError;
        }
    }

    public class FoupProcedurePostSafetyError : FoupProcedureStateBase
    {
        public override FoupProcedureStateEnum GetState()
        {
            return FoupProcedureStateEnum.PostSafetyError;
        }
    }

    public class FoupProcedureBehaviorError : FoupProcedureStateBase
    {
        public override FoupProcedureStateEnum GetState()
        {
            return FoupProcedureStateEnum.BehaviorError;
        }
    }

    public class FoupProcedureDone : FoupProcedureStateBase
    {
        public override FoupProcedureStateEnum GetState()
        {
            return FoupProcedureStateEnum.DONE;
        }
    }
}
