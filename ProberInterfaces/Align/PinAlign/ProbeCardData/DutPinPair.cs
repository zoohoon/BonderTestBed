namespace ProberInterfaces.PinAlign.ProbeCardData
{
    public interface IDutPinPair
    {
        MachineIndex DutMachineIndex { get; set; }
        int PinNum { get; set; }
    }
}
