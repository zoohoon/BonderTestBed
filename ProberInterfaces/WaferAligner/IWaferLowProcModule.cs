using ProberErrorCode;
using ProberInterfaces.State;

namespace ProberInterfaces.WaferAligner
{
    public interface IWaferLowProcModule
    {
        IParam LowStandard_IParam { get; set; }
        EventCodeEnum SaveDevParameter();

        EventCodeEnum SetStepSetupCompleteState();

        EventCodeEnum SetStepSetupNotCompleteState();

        void SetRecoveryStepState(EnumMoudleSetupState state, bool isparent = false);
    }
}
