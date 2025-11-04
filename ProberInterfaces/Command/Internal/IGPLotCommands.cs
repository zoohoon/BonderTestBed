namespace ProberInterfaces.Command.Internal
{
    public interface IGPLotOpStart : IProbeCommand
    {
    }

    public interface IGPLotOpPause : IProbeCommand
    {
    }

    public interface IGPLotOpResume : IProbeCommand
    {
    }

    public interface IGPLotOpEnd : IProbeCommand
    {
    }

    public class GPLotOpPauseParam : ProbeCommandParameter
    {
        public bool LotOpPauseWithCell { get; set; } = true;
    }
}
