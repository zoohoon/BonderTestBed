using System;
using ProberInterfaces.WaferTransfer;
using LogModule;

namespace WaferTransfer
{
    using WaferTransfer.GP_WaferLoadProcStates;

    public class GP_WaferLoadProcModule : IWaferTransferProcModule
    {
        public WaferTransferModule WaferTransferModule { get; set; }

        public GP_WaferLoadProcModule(WaferTransferModule module)
        {
            this.WaferTransferModule = module;
        }

        public GP_WaferLoadProcState StateObj { get; set; }

        public WaferTransferTypeEnum TransferType => WaferTransferTypeEnum.Loading;

        public WaferTransferModeEnum TransferMode => WaferTransferModeEnum.TransferByThreeLeg;

        public WaferTransferProcStateEnum State => StateObj.State;

        public void InitState()
        {
            try
            {
                this.StateObj = new IdleState(this);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
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
