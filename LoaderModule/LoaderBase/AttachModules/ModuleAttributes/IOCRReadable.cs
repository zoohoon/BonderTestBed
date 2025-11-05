
using LoaderParameters;
using LoaderParameters.Data;
using ProberInterfaces;
using ProberInterfaces.Enum;

namespace LoaderBase
{
    /// <summary>
    /// OCR을 담당하는 모듈임을 정의합니다.
    /// </summary>
    public interface IOCRReadable : IWaferLocatable
    {
        /// <summary>
        /// OCR Type을 가져옵니다.
        /// </summary>
        OCRTypeEnum OCRType { get; }
                
        /// <summary>
        /// OCR에 접근하는 파라미터를 가져옵니다.
        /// </summary>
        /// <param name="type">타입</param>
        /// <param name="size">사이즈</param>
        /// <returns>파라미터</returns>
        OCRAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size);

        /// <summary>
        /// OCR에 접근하는 파라미터를 가져옵니다.
        /// </summary>
        /// <param name="type">타입</param>
        /// <param name="size">사이즈</param>
        /// <returns>파라미터</returns>
        SubchuckMotionParam GetSubchuckMotionParam(SubstrateSizeEnum size);

        /// <summary>
        /// OCR의 Offset을 가져옵니다.
        /// </summary>
        /// <returns>offset</returns>
        OCROffset GetOCROffset();

        /// <summary>
        /// OCR의 Definition을 가져옵니다.
        /// </summary>
        OCRDefinitionBase OCRDefinitionBase { get; }

        /// <summary>
        /// 입력된 TransferObject를 Read할 수 있는지 여부를 가져옵니다.
        /// </summary>
        /// <param name="subObj">TransferObject</param>
        /// <returns>Read할 수 있으면 true, 그렇지 않으면 false</returns>
        bool CanOCR(TransferObject subObj);

        /// <summary>
        /// OCR에 종속된 PreAlign Module을 가져옵니다.
        /// </summary>
        /// <returns>PreAlign Module 인스턴스</returns>
        IPreAlignModule GetDependecyPA();
        ReservationInfo ReservationInfo { get; }
    }

    /// <summary>
    /// OCR의 위치 Offset을 정의합니다.
    /// </summary>
    public class OCROffset
    {
        /// <summary>
        /// OffsetU 를 가져오거나 설정합니다.
        /// </summary>
        public double OffsetU { get; set; }

        /// <summary>
        /// OffsetW 를 가져오거나 설정합니다.
        /// </summary>
        public double OffsetW { get; set; }

        /// <summary>
        /// OffsetV 를 가져오거나 설정합니다.
        /// </summary>
        public double OffsetV { get; set; }

    }
}
