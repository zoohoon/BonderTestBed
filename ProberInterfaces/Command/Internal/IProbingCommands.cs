using ProberInterfaces.BinData;

namespace ProberInterfaces.Command.Internal
{
    public interface IGoToStartDie : IProbeCommand
    {
    }

    public interface IGoToCenterDie : IProbeCommand
    {
    }

    public interface IZUPRequest : IProbeCommand
    {
    }

    public interface IZUPResponse : IProbeCommand
    {
    }

    public interface IZDownRequest : IProbeCommand
    {
    }

    public interface IZDownResponse : IProbeCommand
    {
    }

    public interface ICheckRemainSequenceCommand : IProbeCommand
    {
    }

    public interface IResponseGoToStartDie : IProbeCommand
    {
    }

    public interface IMoveToNextDie : IProbeCommand
    {
    }

    public interface IMoveToDiePosition : IProbeCommand
    {
    }

    public interface IMoveToDiePositionAndZUp : IProbeCommand
    {
    }
    public interface IProbingSequenceResponse : IProbeCommand
    {
    }

    public interface IUnloadWafer : IProbeCommand
    {
    }

    public interface IZDownAndPause : IProbeCommand
    {
    }

    public interface IResumeProbing : IProbeCommand
    {
    }


    public interface ISRQ_RESPONSE : IProbeCommand
    {
    }

    public interface ISetBinAnalysisData : IProbeCommand
    {
    }

    public class BinCommandParam : ProbeCommandParameter
    {
        public BinAnalysisDataArray Param { get; set; }
    }

    public class ProbingCommandParam : ProbeCommandParameter
    {
        public EnumProbingState ProbingStateWhenReciveMoveToNextDie { get; set; }

        public bool ForcedZdownAndUnload { get; set; }
    }
}
