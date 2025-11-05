using System.Collections.Generic;

using ProberInterfaces;
using LoaderParameters;
using ProberInterfaces.Foup;
using ProberErrorCode;

namespace LoaderBase
{
    /// <summary>
    /// 카세트 모듈을 정의합니다.
    /// </summary>
    public interface ICassetteModule : IAttachedModule
    {
        /// <summary>
        /// 카세트의 스캔 상태를 가져옵니다.
        /// </summary>
        CassetteScanStateEnum ScanState { get; }

        FoupStateEnum FoupState { get; }
        FoupCoverStateEnum FoupCoverState { get; set; }        
        LotModeEnum LotMode { get; set; }

        string HashCode { get; }

        string FoupID { get; set; }
        /// <summary>
        /// 모듈의 정의 파라미터를 가져옵니다.
        /// </summary>
        CassetteDefinition Definition { get; }

        /// <summary>
        /// 모듈의 디바이스 파라미터를 가져옵니다.
        /// </summary>
        CassetteDevice Device { get; }

        /// <summary>
        /// 모듈의 사용여부
        /// </summary>
        bool Enable { get; set; }

        /// <summary>
        /// 모듈을 정의합니다.
        /// </summary>
        /// <param name="definition">파라미터</param>
        /// <param name="index">인덱스</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDefinition(CassetteDefinition definition, int index);

        /// <summary>
        /// 모듈의 디바이스를 설정합니다.
        /// </summary>
        /// <param name="device">디바이스</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetDevice(CassetteDevice device);

        /// <summary>
        /// 카세트의 첫번째 슬롯의 접근 파라미터를 가져옵니다.
        /// </summary>
        /// <param name="type">타입</param>
        /// <param name="size">사이즈</param>
        /// <returns>파라미터</returns>
        CassetteSlot1AccessParam GetSlot1AccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size);
                
        /// <summary>
        /// 카세트의 상태를 ILLEGAL 상태로 설정합니다.
        /// </summary>
        void SetIllegalScanState();

        /// <summary>
        /// 카세트의 상태를 READING 상태로 설정합니다.
        /// </summary>
        void SetReadingScanState();

        /// <summary>
        /// 카세트의 상태를 NO_READ 상태로 설정합니다.
        /// </summary>
        void SetNoReadScanState();

        /// <summary>
        /// 카세트의 상태를 Reserved 상태로 설정합니다.
        /// </summary>
        void SetReservedScanState();

        /// <summary>
        /// 카세트의 스캔 결과를 입력합니다. <br/>
        /// 결과에 따라 SLOT의 상태 및 카세트의 스캔상태가 변경됩니다.
        /// </summary>
        /// <param name="scanRelDic">스캔 결과</param>
        void SetScanResult(Dictionary<ISlotModule, SlotScanStateEnum> scanRelDic);

        /// <summary>
        /// 카세트가 입력된 FOUP에 속해 있는 지 여부를 가져옵니다.
        /// </summary>
        /// <param name="foupNumber">FOUP 번호</param>
        /// <returns>FOUP에 속해 있으면 true, 그렇지 않으면 fase</returns>
        bool IsInFoup(int foupNumber);

        void SetFoupState(FoupStateEnum state);
        void SetFoupCoverState(FoupCoverStateEnum state);
        void SetCarrierId(string carrierid);
        void SetHashCode(bool isAttached);
        object GetLockObj();
    }

    /// <summary>
    /// 슬롯의 스캔 상태를 정의합니다.
    /// </summary>
    public enum SlotScanStateEnum
    {
        /// <summary>
        /// 알수 없는 상태
        /// </summary>
        UNKNOWN,
        /// <summary>
        /// 오브젝트가 검출되지 않은 상태
        /// </summary>
        NOT_DETECTED,
        /// <summary>
        /// 오브젝트 검출된 상태
        /// </summary>
        DETECTED,
    }
}
