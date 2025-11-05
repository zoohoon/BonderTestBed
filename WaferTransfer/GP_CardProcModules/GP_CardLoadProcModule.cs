using ProberInterfaces;
using ProberInterfaces.WaferTransfer;
using LogModule;

namespace WaferTransfer
{
    using System;
    using WaferTransfer.GP_CardLoadProcStates;

    public class GP_CardLoadProcModule : IWaferTransferProcModule
    {
        public WaferTransferModule WaferTransferModule { get; set; }

        public GP_CardLoadProcModule(WaferTransferModule module)
        {
            this.WaferTransferModule = module;
        }

        public GP_CardLoadProcState StateObj { get; set; }

        public WaferTransferTypeEnum TransferType => WaferTransferTypeEnum.CardLoading;

        public WaferTransferModeEnum TransferMode => WaferTransferModeEnum.TransferByThreeLeg;

        public WaferTransferProcStateEnum State => StateObj.State;

        public void InitState()
        {
            try
            {
                if(this.CardChangeModule().GetCCDockType() == ProberInterfaces.CardChange.EnumCardDockType.DIRECTDOCK)
                {
                    this.StateObj = new IdleDDState(this);
                }
                else
                {
                    this.StateObj = new IdleState(this);

                }
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
