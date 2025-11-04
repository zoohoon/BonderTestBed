using ProberErrorCode;
using LoaderParameters;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberInterfaces.Enum;

namespace LoaderBase
{
    /// <summary>
    /// CardBufferTrayModue을 정의합니다.
    /// </summary>
    public interface ICardBufferTrayModule : ICardOwnable, ICardSupplyModule
    {
        /// <summary>
        /// 모듈의 정의 파라미터를 가져옵니다.
        /// </summary>
        CardBufferTrayDefinition Definition { get; }

        /// <summary>
        /// 모듈의 디바이스 파라미터를 가져옵니다.
        /// </summary>
        CardBufferTrayDevice Device { get; }

        /// <summary>
        /// 모듈을 정의합니다.
        /// </summary>
        /// <param name="definition">파라미터</param>
        /// <param name="index">인덱스</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDefinition(CardBufferTrayDefinition definition, int index);

        /// <summary>
        /// 모듈의 디바이스를 설정합니다.
        /// </summary>
        /// <param name="device">파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDevice(CardBufferTrayDevice device);

        /// <summary>
        /// Card를 소유 하고 있는 상태가 value와 일치 하는 지 확인합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum MonitorForSubstrate(bool value);

        /// <summary>
        /// CardBufferTray에 접근하는 파라미터를 가져옵니다.
        /// </summary>
        /// <param name="type">타입</param>
        /// <param name="size">사이즈</param>
        /// <returns>파라미터</returns>
        CardBufferTrayAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size);
        bool IsDrawerSensorOn();
        /// <summary>
        /// Carrier를 소유 하고 있는 상태가 value와 일치 하는 지 확인합니다.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        EventCodeEnum MonitorForSubstrate_Down(bool v);
    }

    public delegate void CardPresenceStateChangeEvent();
    public delegate void CardBufferStateUpdateEvent(bool forced_event = false);
    public interface ICardBufferModule : ICardOwnable, ICardSupplyModule
    {
        event CardPresenceStateChangeEvent E84PresenceStateChangedEvent;
        event CardBufferStateUpdateEvent CardBufferStateUpdateEvent;
        /// <summary>
        /// 모듈의 정의 파라미터를 가져옵니다.
        /// </summary>
        CardBufferDefinition Definition { get; }

        /// <summary>
        /// 모듈의 디바이스 파라미터를 가져옵니다.
        /// </summary>
        CardBufferDevice Device { get; }

        IOPortDescripter<bool> GetDICARRIERVAC();
        /// <summary>
        /// 모듈을 정의합니다.
        /// </summary>
        /// <param name="definition">파라미터</param>
        /// <param name="index">인덱스</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDefinition(CardBufferDefinition definition, int index);

        /// <summary>
        /// 모듈의 디바이스를 설정합니다.
        /// </summary>
        /// <param name="device">파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDevice(CardBufferDevice device);

        /// <summary>
        /// Wafer를 소유 하고 있는 상태가 value와 일치 하는 지 확인합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum MonitorForSubstrate(bool value);

        /// <summary>
        /// CardBufferTray에 접근하는 파라미터를 가져옵니다.
        /// </summary>
        /// <param name="type">타입</param>
        /// <param name="size">사이즈</param>
        /// <returns>파라미터</returns>
        CardBufferAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size);
        /// <summary>
        /// Card Buffer 에 Card 유무 상태.
        /// </summary>
        CardPRESENCEStateEnum CardPRESENCEState { get; set; }

        /// <summary>
        /// cardbuffer에서 card와 cardholder를 구분할수 있는지 확인하는 함수
        /// 구분 불가능할 경우 cardholder만 있어도 card가 있다고 인식함.
        /// </summary>
        /// <returns></returns>
        bool CanDistinguishCard();

        /// <summary>
        /// 센서를 보고 cardbufferpresence 상태를 업데이트합니다. 
        /// </summary>
        /// <param name="risingevent"></param>
        void UpdateCardBufferState(bool forced_event = false, CardPRESENCEStateEnum forced_presence = CardPRESENCEStateEnum.UNDEFINED);

        /// <summary>
        /// CardBuffer의 LoadPort 상태를 Gem으로 업데이트 합니다. 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="forcewrite"></param>
        void ChangeCardLPServiceStatus(GEMFoupStateEnum state, bool forcewrite = false, PIVInfo pivinfo = null);

        /// <summary>
        /// //CardHolder 가 CARD, HOLDER 가 따로 없고 CARD 만 있기 때문에 카드 홀더만 있을때 RFID를 안읽게 하도록 attach 일때만 READ 상태로 만듦.                
        /// </summary>
        /// <returns></returns>
        OCRModeEnum NeedToReadCardId();
        /// <summary>
        /// Carrier를 소유 하고 있는 상태가 value와 일치 하는 지 확인합니다.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        EventCodeEnum MonitorForCarrierVac(bool v);
    }
}
