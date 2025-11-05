using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.Foup;
using System;
using LogModule;

namespace InternalCommands
{
    public class CassetteLoad : ProbeCommand, ICassetteLoadCommand
    {
        public override bool Execute()
        {
            try
            {
                //IGEMModule GEMModule = Container.Resolve<IGEMModule>();
                //GEMModule.SetEvent(6451);

                var foupCmdParma = this.Parameter as FoupCommandParam;
                IFoupOpModule foupOp = this.FoupOpModule();
                var foupController = foupOp.GetFoupController(foupCmdParma.CassetteNumber);

                bool isSucceed = foupController.Execute(new FoupLoadCommand()) == EventCodeEnum.NONE;
                return isSucceed;
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }

    public class CassetteUnload : ProbeCommand, ICassetteUnLoadCommand
    {
        public override bool Execute()
        {
            try
            {
                var foupCmdParma = this.Parameter as FoupCommandParam;
                IFoupOpModule foupMgr = this.FoupOpModule();
                var foupController = foupMgr.GetFoupController(foupCmdParma.CassetteNumber);

                bool isSucceed = foupController.Execute(new FoupUnloadCommand()) == EventCodeEnum.NONE;
                return isSucceed;
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
