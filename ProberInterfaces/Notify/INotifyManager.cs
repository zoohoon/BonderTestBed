namespace ProberInterfaces
{
    using ProberErrorCode;

    public interface INotifyManager : IFactoryModule, IModule, IHasSysParameterizable, ILoaderFactoryModule
    {
        IParam NotifySysParam { get; }
        EventCodeEnum Notify(EventCodeEnum errorCode, int indexOffset = 1, bool isStack = false);
        EventCodeEnum Notify(EventCodeEnum errorCode, string message, int indexOffset = 1, bool isStack = false);
        EventCodeEnum Notify(EventCodeParam noticeparam, int indexOffset = 1, bool isStack = false);
        EventCodeEnum ClearNotify(EventCodeEnum errorCode, int indexOffset = 1);

        void SetLastStageMSG(string msg);
        string GetLastStageMSG();
        void SendLastMSGToLoader();
        EventCodeEnum NotifyStackParams();

        bool IsCriticalError(EventCodeEnum eventcode);
        EventCodeParam GetNotifyParam(EventCodeEnum errorCode);
    }
}
