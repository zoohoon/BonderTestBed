namespace GPLoaderRouter
{
    using ProberErrorCode;
    using ProberInterfaces;
    using SequenceRunner;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Serializable]
    public abstract class GPLoaderRobotCommand : IGPLoaderRobotCommand
    {
        public abstract EnumRobotCommand CommandName { get; set; }
        public abstract Dictionary<int, List<SequenceBehavior>> CommandSequence { get; set; }
        public abstract void CreateCommandSequence();
    }

    [Serializable]
    public abstract class GPLoaderCSTCommand : IGPLoaderCSTCtrlCommand
    {
        public abstract EnumCSTCtrl CommandName { get; set; }
        public abstract SubstrateSizeEnum WaferSize { get; set; }
        public abstract Dictionary<int, List<SequenceBehavior>> CommandSequence { get; set; }
        public abstract void CreateCommandSequence();
    }

   
   
}
