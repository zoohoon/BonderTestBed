namespace ProberInterfaces.Command.Internal
{
    public class FoupCommandParam : ProbeCommandParameter
    {
        public int CassetteNumber { get; set; }
    }

    public interface ICassetteLoadCommand : IProbeCommand
    {
    }

    public interface ICassetteUnLoadCommand : IProbeCommand
    {
    }

}
