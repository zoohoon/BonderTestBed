using System;
using ProberInterfaces.WaferTransfer;
using LogModule;

namespace WaferTransfer
{
    using WaferTransfer.GOP_CardUnLoadProcStates;

    public class GOP_CardUnLoadProcModule : IWaferTransferProcModule
    {
        public WaferTransferModule WaferTransferModule { get; set; }

        public GOP_CardUnLoadProcModule(WaferTransferModule module)
        {
            this.WaferTransferModule = module;
        }

        public GOP_CardUnLoadProcState StateObj { get; set; }

        public WaferTransferTypeEnum TransferType => WaferTransferTypeEnum.CardUnLoding;

        public WaferTransferModeEnum TransferMode => WaferTransferModeEnum.TransferByThreeLeg;

        public WaferTransferProcStateEnum State => StateObj.State;

        public void InitState()
        {
            this.StateObj = new IdleState(this);
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
