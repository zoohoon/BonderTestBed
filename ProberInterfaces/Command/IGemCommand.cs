namespace ProberInterfaces
{
    using ProberErrorCode;

    public interface IGemCommand
    {
        EventCodeEnum SetPIV(PIVInfo pivInfo);
        EventCodeEnum AfterSetPIV(PIVInfo pivInfo);
        EventCodeEnum PreCheck();
    }
    public interface IFoupNotifyEvent
    {
        EventCodeEnum CheckFoupMode(PIVInfo pivInfo);
    }
}
