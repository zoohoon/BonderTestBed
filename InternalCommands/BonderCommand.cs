using LogModule;
using ProberInterfaces;
using ProberInterfaces.Bonder;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command.Internal
{
    public class BonderStartCommand : ProbeCommand , IBonderStartCommand
    {
        public override bool Execute()
        {
            LoggerManager.Debug("[Bonder] BonderCommand : BonderStartCommand Execute()");
            IBonderModule module = this.BonderModule();

            return SetCommandTo(module);
        }
    }
    public class BonderEndCommand : ProbeCommand, IBonderEndCommand
    {
        public override bool Execute()
        {
            LoggerManager.Debug("[Bonder] BonderCommand : BonderEndCommand Execute()");
            IBonderModule module = this.BonderModule();

            return SetCommandTo(module);
        }
    }
    public class PickCommand : ProbeCommand, IPickCommand
    {
        public override bool Execute()
        {
            LoggerManager.Debug("[Bonder] BonderCommand : PickCommand Execute()");
            IBonderModule module = this.BonderModule();

            return SetCommandTo(module);
        }
    }
    public class PlaceCommand : ProbeCommand, IPlaceCommand
    {
        public override bool Execute()
        {
            LoggerManager.Debug("[Bonder] BonderCommand : PlaceCommand Execute()");
            IBonderModule module = this.BonderModule();

            return SetCommandTo(module);
        }
    }
    public class RotationCommand : ProbeCommand, IRotationCommand
    {
        public override bool Execute()
        {
            LoggerManager.Debug("[Bonder] BonderCommand : RotationCommand Execute()");
            IBonderModule module = this.BonderModule();

            return SetCommandTo(module);
        }
    }
    public class NanoAlign : ProbeCommand, IDieAlign
    {
        public override bool Execute()
        {
            LoggerManager.Debug("[Bonder] BonderCommand : DieAlign Execute()");
            IBonderModule module = this.BonderModule();

            return SetCommandTo(module);
        }
    }
}
