using ProberInterfaces.AirBlow;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using System;
using ProberInterfaces;
using LogModule;

namespace Command.Internal
{
    public class AirBlowChuckCleaningCommand : ProbeCommand, IAirBlowChuckCleaningCommand
    {
        public override bool Execute()
        {
            try
            {
                IAirBlowChuckCleaningModule module = this.AirBlowChuckCleaningModule();

                return SetCommandTo(module);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    //public class AirBlowChuckCleaningDoneCommand : ProbeCommand, IAirBlowChuckCleaningDoneCommand
    //{
    //    public override bool ExecuteCMD()
    //    {
    //        ILoaderController module = Container.Resolve<ILoaderController>();

    //        return SetCommandTokenTo(module);
    //    }
    //}

    //public class AirBlowChuckCleaningAbortCommand : ProbeCommand, IAirBlowChuckCleaningAbortCommand
    //{
    //    public override bool ExecuteCMD()
    //    {
    //        ILoaderController module = Container.Resolve<ILoaderController>();

    //        return SetCommandTokenTo(module);
    //    }
    //}

    public class AirBlowWaferCleaningCommand : ProbeCommand, IAirBlowWaferCleaningCommand
    {
        public override bool Execute()
        {
            IAirBlowWaferCleaningModule module = this.AirBlowWaferCleaningModule();

            return SetCommandTo(module);
        }
    }

    //public class AirBlowWaferCleaningDoneCommand : ProbeCommand, IAirBlowWaferCleaningDoneCommand
    //{
    //    public override bool ExecuteCMD()
    //    {
    //        ILoaderController module = Container.Resolve<ILoaderController>();

    //        return SetCommandTokenTo(module);
    //    }
    //}

    //public class AirBlowWaferCleaningAbortCommand : ProbeCommand, IAirBlowWaferCleaningAbortCommand
    //{
    //    public override bool ExecuteCMD()
    //    {
    //        ILoaderController module = Container.Resolve<ILoaderController>();

    //        return SetCommandTokenTo(module);
    //    }
    //}
}
