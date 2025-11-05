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

    public interface IGPLoaderRobotCommand
    {
        EnumRobotCommand CommandName { get; set; }
        Dictionary<int, List<SequenceBehavior>> CommandSequence { get; set; }
        void CreateCommandSequence();
    }

    public interface IGPLoaderCSTCtrlCommand
    {
        EnumCSTCtrl CommandName { get; set; }
        SubstrateSizeEnum WaferSize { get; set; }
        Dictionary<int, List<SequenceBehavior>> CommandSequence { get; set; }
        void CreateCommandSequence();
    }
}
