using ProberErrorCode;

namespace ProberInterfaces.EnvControl
{
   public interface IEnvModule : IFactoryModule, IHasSysParameterizable /*IStateModule*/
   {
        EventCodeEnum IsConditionSatisfied();
    }

    //public enum EnumEnvModuleState
    //{
    //    IDLE = 0x0000,
    //    RUNNING,
    //    SUSPEND,
    //    PAUSE,
    //    DONE,
    //    ABORT,
    //    ERROR
    //}
}
