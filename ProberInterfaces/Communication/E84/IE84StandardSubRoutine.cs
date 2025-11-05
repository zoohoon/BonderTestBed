namespace ProberInterfaces.Communication.E84
{
    using ProberErrorCode;
    using ProberInterfaces.Foup;

    public interface IE84SubRoutine
    {
        EventCodeEnum CheckBeforeSendSignal(IFoupController foupcontroller, E84SignalTypeEnum signal, bool flag);
    }
}
