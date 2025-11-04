using System;
using LogModule;
using ProberInterfaces;
using ProberInterfaces.Enum;

namespace LoaderBase
{
    /// <summary>
    /// LoaderJob 을 정의합니다.
    /// </summary>
    public interface ILoaderJob
    {
        /// <summary>
        /// 우선순위를 가져옵니다.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 유효성을 검사합니다.
        /// </summary>
        /// <returns>결과정보</returns>
        JobValidateResult Validate();

        /// <summary>
        /// 스케쥴링합니다.
        /// </summary>
        /// <returns>결과정보</returns>
        JobScheduleResult DoSchedule();
    }

    /// <summary>
    /// Job 유효성 검사 결과를 정의합니다.
    /// </summary>
    public class JobValidateResult
    {
        /// <summary>
        /// 유효한 지 여부를 가져오거나 설정합니다.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 유효하지 않을 경우 오류 사유를 가져오거나 설정합니다.
        /// </summary>
        public string ReasonOfError { get; set; }

        /// <summary>
        /// 유효한 상태로 설정합니다.
        /// </summary>
        public void SetValid()
        {
            try
            {
                IsValid = true;
                ReasonOfError = "";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 유효하지 않은 상태로 설정합니다.
        /// </summary>
        /// <param name="reasonOfError">오류 사유</param>
        public void SetError(string reasonOfError)
        {
            try
            {
                IsValid = false;
                ReasonOfError = reasonOfError;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    /// <summary>
    /// JobScheduleRelCode 를 정의합니다
    /// </summary>
    public enum JobScheduleRelCodeEnum
    {
        /// <summary>
        /// UNDEFINED
        /// </summary>
        UNDEFINED,
        /// <summary>
        /// NEED_PROCESSING
        /// </summary>
        NEED_PROCESSING,
        /// <summary>
        /// DONE 
        /// </summary>
        JOB_DONE,
        /// <summary>
        /// ERROR
        /// </summary>
        ERROR,
    }

    /// <summary>
    /// JobScheduleResult 를 정의합니다.
    /// </summary>
    public class JobScheduleResult
    {
        /// <summary>
        /// JobScheduleRelCode 를 가져오거나 설정합니다.
        /// </summary>
        public JobScheduleRelCodeEnum RelCode { get; private set; }

        /// <summary>
        /// 다음에 수행해야 할 프로세스의 정보를 가져오거나 설정합니다.
        /// </summary>
        public ILoaderProcessParam NextProc { get; private set; }

        /// <summary>
        /// JOB_DONE 상태로 설정합니다.
        /// </summary>
        public void SetJobDone()
        {
            NextProc = null;

            RelCode = JobScheduleRelCodeEnum.JOB_DONE;
        }

        /// <summary>
        /// Error 상태로 설정합니다.
        /// </summary>
        /// <param name="reasonOfError">오류 사유</param>
        public void SetError(string reasonOfError = "")
        {
            NextProc = null;

            RelCode = JobScheduleRelCodeEnum.ERROR;
        }

        /// <summary>
        /// NEED_PROCESSING 상태로 설정합니다. (Transfer)
        /// </summary>
        /// <param name="substrate"></param>
        /// <param name="curr"></param>
        /// <param name="next"></param>
        /// <param name="useARM"></param>
        /// <param name="dstPos"></param>
        public void SetTransfer(TransferObject substrate, IWaferLocatable curr, IWaferLocatable next, IARMModule useARM, IWaferLocatable dstPos)
        {
            NextProc = new TransferProcParam()
            {
                TransferObject = substrate,
                Curr = curr,
                Next = next,
                DestPos = dstPos,
                UseARM = useARM,
            };

            RelCode = JobScheduleRelCodeEnum.NEED_PROCESSING;
        }

        public void SetTransfer(TransferObject substrate, IWaferLocatable curr, IWaferLocatable next, IARMModule useARM, IWaferLocatable dstPos,OCRReadStateEnum oCRState)
        {
            NextProc = new TransferProcParam()
            {
                TransferObject = substrate,
                Curr = curr,
                Next = next,
                DestPos = dstPos,
                UseARM = useARM,
                OCRState = oCRState
            };

            RelCode = JobScheduleRelCodeEnum.NEED_PROCESSING;
        }
        public void SetTransfer(TransferObject substrate, ICardLocatable curr, ICardLocatable next, ICardARMModule useARM, ICardLocatable dstPos)
        {
            NextProc = new CardTransferProcParam()
            {
                TransferObject = substrate,
                Curr = curr,
                Next = next,
                DestPos = dstPos,
                UseARM = useARM,
            };

            RelCode = JobScheduleRelCodeEnum.NEED_PROCESSING;
        }
        /// <summary>
        /// NEED_PROCESSING 상태로 설정합니다. (PreAlign)
        /// </summary>
        /// <param name="substrate"></param>
        /// <param name="PA"></param>
        /// <param name="useARM"></param>
        /// <param name="dstPos"></param>
        public void SetPreAlign(TransferObject substrate, IPreAlignModule PA, IARMModule useARM, IWaferLocatable dstPos)
        {
            NextProc = new PreAlignProcParam()
            {
                TransferObject = substrate,
                UsePA = PA,
                UseARM = useARM,
                DestPos = dstPos,
            };

            RelCode = JobScheduleRelCodeEnum.NEED_PROCESSING;
        }

        /// <summary>
        ///  NEED_PROCESSING 상태로 설정합니다. (OCR)
        /// </summary>
        /// <param name="substrate"></param>
        /// <param name="readable"></param>
        /// <param name="useARM"></param>
        /// <param name="dstPos"></param>
        public void SetReadOCR(TransferObject substrate, IOCRReadable readable, IARMModule useARM, IWaferLocatable dstPos)
        {
            NextProc = new OCRProcParam()
            {
                TransferObject = substrate,
                UseOCR = readable,
                UseARM = useARM,
                DestPos = dstPos,
            };

            RelCode = JobScheduleRelCodeEnum.NEED_PROCESSING;
        }

        /// <summary>
        ///  NEED_PROCESSING 상태로 설정합니다. (OCR)
        /// </summary>
        /// <param name="substrate"></param>
        /// <param name="readable"></param>
        /// <param name="useARM"></param>
        /// <param name="dstPos"></param>
        public void SetReadOCR(TransferObject substrate, IOCRReadable readable, IWaferOwnable useWaferOwnable, IWaferLocatable dstPos,OCRReadStateEnum ocrState)
        {
            NextProc = new OCRProcParam()
            {
                TransferObject = substrate,
                UseOCR = readable,
                UseOwnable = useWaferOwnable,
                DestPos = dstPos,
                OCRState = ocrState
            };

            RelCode = JobScheduleRelCodeEnum.NEED_PROCESSING;
        }
        /// <summary>
        ///  NEED_PROCESSING 상태로 설정합니다. (Scan)
        /// </summary>
        /// <param name="cassette"></param>
        /// <param name="scanable"></param>
        public void SetScan(ICassetteModule cassette, ICassetteScanable scanable)
        {
            try
            {
                NextProc = new ScanProcParam()
                {
                    Cassette = cassette,
                    UseScanable = scanable,
                };

                RelCode = JobScheduleRelCodeEnum.NEED_PROCESSING;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// NEED_PROCESSING 상태로 설정합니다.
        /// </summary>
        /// <param name="substrate"></param>
        /// <param name="curr"></param>
        /// <param name="next"></param>
        public void SetCloseFoupCover(ICassetteModule cassette)
        {
            try
            {
                NextProc = new CloseFoupCoverProcParam()
                {
                    Cassette = cassette
                };

                RelCode = JobScheduleRelCodeEnum.NEED_PROCESSING;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }

}
