using System;
using LogModule;
using ProberInterfaces;
using ProberInterfaces.WaferTransfer;

namespace WaferTransfer
{
    using WaferTransfer.GP_CardUnLoadProcStates;

    public class GP_CardUnLoadProcModule : IWaferTransferProcModule
    {
        public WaferTransferModule WaferTransferModule { get; set; }

        public GP_CardUnLoadProcModule(WaferTransferModule module)
        {
            this.WaferTransferModule = module;
        }

        public GP_CardUnLoadProcState StateObj { get; set; }

        public WaferTransferTypeEnum TransferType => WaferTransferTypeEnum.CardUnLoding;

        public WaferTransferModeEnum TransferMode => WaferTransferModeEnum.TransferByThreeLeg;

        public WaferTransferProcStateEnum State => StateObj.State;

        public void InitState()
        {
            if (this.CardChangeModule().GetCCDockType() == ProberInterfaces.CardChange.EnumCardDockType.DIRECTDOCK)
            {
                this.StateObj = new IdleDDState(this);
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
