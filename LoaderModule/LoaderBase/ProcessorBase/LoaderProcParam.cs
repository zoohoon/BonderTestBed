using ProberInterfaces;
using ProberInterfaces.Enum;

namespace LoaderBase
{
    /// <summary>
    /// LoaderProcParam 을 정의합니다.
    /// </summary>
    public interface ILoaderProcessParam { }

    /// <summary>
    /// TransferProcParam 을 정의합니다.
    /// </summary>
    public class TransferProcParam : ILoaderProcessParam
    {
        /// <summary>
        /// TransferObject
        /// </summary>
        public TransferObject TransferObject { get; set; }

        /// <summary>
        /// Curr
        /// </summary>
        public IWaferLocatable Curr { get; set; }

        /// <summary>
        /// Next
        /// </summary>
        public IWaferLocatable Next { get; set; }

        /// <summary>
        /// DestPos
        /// </summary>
        public IWaferLocatable DestPos { get; set; }

        /// <summary>
        /// UseARM
        /// </summary>
        public IARMModule UseARM { get; set; }

        public OCRReadStateEnum OCRState { get; set; }
    }
    public class CardTransferProcParam : ILoaderProcessParam
    {
        /// <summary>
        /// TransferObject
        /// </summary>
        public TransferObject TransferObject { get; set; }

        /// <summary>
        /// Curr
        /// </summary>
        public ICardLocatable Curr { get; set; }

        /// <summary>
        /// Next
        /// </summary>
        public ICardLocatable Next { get; set; }

        /// <summary>
        /// DestPos
        /// </summary>
        public ICardLocatable DestPos { get; set; }

        /// <summary>
        /// UseARM
        /// </summary>
        public ICardARMModule UseARM { get; set; }
    }
    /// <summary>
    /// PreAlignProcParam 을 정의합니다.
    /// </summary>
    public class PreAlignProcParam : ILoaderProcessParam
    {
        /// <summary>
        /// TransferObject
        /// </summary>
        public TransferObject TransferObject { get; set; }

        /// <summary>
        /// UsePA
        /// </summary>
        public IPreAlignModule UsePA { get; set; }

        /// <summary>
        /// UseARM
        /// </summary>
        public IARMModule UseARM { get; set; }

        /// <summary>
        /// DestPos
        /// </summary>
        public IWaferLocatable DestPos { get; set; }
    }

    /// <summary>
    /// OCRProcParam 을 정의합니다.
    /// </summary>
    public class OCRProcParam : ILoaderProcessParam
    {
        /// <summary>
        /// TransferObject
        /// </summary>
        public TransferObject TransferObject { get; set; }

        /// <summary>
        /// UseOCR
        /// </summary>
        public IOCRReadable UseOCR { get; set; }

        /// <summary>
        /// UseARM
        /// </summary>
        public IARMModule UseARM { get; set; }


        public IWaferOwnable UseOwnable { get; set; }
        /// <summary>
        /// DestPos
        /// </summary>
        public IWaferLocatable DestPos { get; set; }

        public OCRReadStateEnum OCRState { get; set; }
    }

    /// <summary>
    /// ScanProcParam 을 정의합니다.
    /// </summary>
    public class ScanProcParam : ILoaderProcessParam
    {
        /// <summary>
        /// Cassette
        /// </summary>
        public ICassetteModule Cassette { get; set; }

        /// <summary>
        /// UseScanable
        /// </summary>
        public ICassetteScanable UseScanable { get; set; }
    }

    public class CloseFoupCoverProcParam : ILoaderProcessParam
    {
        /// <summary>
        /// Cassette
        /// </summary>
        public ICassetteModule Cassette { get; set; }
    }
}
