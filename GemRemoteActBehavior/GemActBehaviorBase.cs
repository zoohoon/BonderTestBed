namespace GemActBehavior
{
    using ProberErrorCode;
    using ProberInterfaces;
    using SecsGemServiceInterface;
    using XGEMWrapper;

    public abstract class GemActBehaviorBase : IGemActBehavior
    {
        public virtual EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            return EventCodeEnum.NONE;
        }

        public virtual EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            return EventCodeEnum.NONE;
        }

        public virtual EventCodeEnum ExcuteCarrierCommander(CarrierActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            return EventCodeEnum.NONE;
        }

        public virtual EventCodeEnum SetAck(CarrierActReqData carrierActReqData = null, RemoteActReqData remoteActReqData = null, ISecsGemServiceHost gemServiceHost = null)
        {
            return EventCodeEnum.NONE;
        }
    }
}
