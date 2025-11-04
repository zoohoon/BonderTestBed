using System;
using ProberInterfaces;
using ProberInterfaces.WaferTransfer;
using LogModule;

namespace WaferTransfer
{
    using WaferTransfer.GP_WaferUnloadProcStates;

    public class GP_WaferUnloadProcModule : IWaferTransferProcModule
    {
        public WaferTransferModule WaferTransferModule { get; set; }

        public GP_WaferUnloadProcModule(WaferTransferModule module)
        {
            this.WaferTransferModule = module;
        }

        public GP_WaferUnloadProcState StateObj { get; set; }

        public WaferTransferTypeEnum TransferType => WaferTransferTypeEnum.Unloading;

        public WaferTransferModeEnum TransferMode => WaferTransferModeEnum.TransferByThreeLeg;

        public WaferTransferProcStateEnum State => StateObj.State;

        public void InitState()
        {
            if(WaferTransferModule.NeedToRecovery == true && 
                WaferTransferModule.ModuleState.GetState() == ModuleStateEnum.ERROR)
            {
                this.StateObj = new SystemErrorState(this);
            }
            else
            {
                this.StateObj = new IdleState(this);
            }
        }

        public void Execute()
        {
            try
            {
                this.StateObj.Execute();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public void SelfRecovery()
        {
            try
            {
                this.StateObj.SelfRecovery();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }


}
