namespace ProberInterfaces
{
    using ProberErrorCode;
    using SecsGemServiceInterface;
    using XGEMWrapper;

    public interface IGemActBehavior : IFactoryModule
    {
        EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost);
        EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData);
        EventCodeEnum ExcuteCarrierCommander(CarrierActReqData actReqData, ISecsGemServiceHost gemServiceHost);
    }
}
