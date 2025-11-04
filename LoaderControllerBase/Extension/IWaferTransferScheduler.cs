using System;

using LoaderParameters;
using LogModule;
using ProberInterfaces;

namespace LoaderControllerBase
{
    public enum WaferTransferSchedulerStateEnum
    {
        UNDEFINED,
        NO_CASSETTE,
        NO_READ,
        NO_WAFER_ON_CHUCK,
        WAFER_ON_CHUCK,
        LAST_WAFER_ON_CHUCK_ONLY,
        CASSETTE_DONE,
    }

    public enum WaferTransferScheduleRelEnum
    {
        ERROR,
        NOT_NEED,
        NEED,
    }

    public class WaferTransferScheduleResult
    {
        public WaferTransferScheduleRelEnum ResultCode { get; set; }

        public LoaderMapEditor Editor { get; set; }

        public static WaferTransferScheduleResult NOT_NEED => new WaferTransferScheduleResult() { ResultCode = WaferTransferScheduleRelEnum.NOT_NEED };

        public static WaferTransferScheduleResult CreateNeed(LoaderMapEditor editor)
        {
            try
            {
                return new WaferTransferScheduleResult()
                {
                    ResultCode = WaferTransferScheduleRelEnum.NEED,
                    Editor = editor
                };
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

    }

    public interface IWaferTransferScheduler : IFactoryModule
    {
        WaferTransferSchedulerStateEnum State { get; }

        bool UpdateState(out bool isNeedWafer, out string cstHashCodeOfRequestLot, bool canloadwafer = true);

        WaferTransferScheduleResult Execute();        
    }
}
