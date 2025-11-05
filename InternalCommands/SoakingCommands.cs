using LogModule;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternalCommands
{
    public class EventSoakingManual : ProbeCommand, IEventSoakingManualCommand
    {
        public override bool Execute()
        {
            ISoakingModule SoakingModule = this.SoakingModule();

            bool setCommandResult = false;

            try
            {
                setCommandResult = SetCommandTo(SoakingModule);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return setCommandResult;
        }
    }
}
