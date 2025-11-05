namespace ProberInterfaces.Command.Internal
{
    public interface IChangeTemperatureToSetTemp : IProbeCommand
    {
    }

    public interface IChangeTemperatureToSetTempWhenWaferTransfer : IProbeCommand
    {
    }

    public interface IChangeTempToSetTempFullReach : IProbeCommand
    {
    }
    
    public interface ISetTempForFrontDoorOpen : IProbeCommand
    {
    }

    public interface IReturnToDefaltSetTemp : IProbeCommand
    {
    }

    public interface IEndTempEmergencyError : IProbeCommand
    {

    }

    public interface IChangeTemperatureToSetTempAfterConnectTempController : IProbeCommand
    {

    }

    public interface ITemperatureSettingTriggerOccurrence : IProbeCommand
    {

    }
}
