using System;
using LogModule;


namespace InternalCommands
{
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;

    public class DoFDWaferAlign : ProbeCommand, IDOFDWAFERALIGN
    {
        public override bool Execute()
        {
            try
            {
                return SetCommandTo(this.FDWaferAligner());
            }
            catch (Exception err)
            {
                LoggerManager.Debug("[Bonder] DoFDWaferAlign, IDOFDWAFERALIGN command Error");
                throw;
            }
        }
    }
}
