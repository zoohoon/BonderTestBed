using ProberErrorCode;
using ProberInterfaces.State;

namespace ProberInterfaces.WaferAligner
{    
    
    public interface IWaferHighProcModule
    {
        EventCodeEnum UpdateHeightProfiling();
        IParam HighStandard_IParam { get; set; }
        EventCodeEnum SaveDevParameter();
        EventCodeEnum SetStepSetupCompleteState();
        EventCodeEnum SetStepSetupNotCompleteState();
        void SetRecoveryStepState(EnumMoudleSetupState state, bool isparent = false);
    }
}
