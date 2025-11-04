using System;

namespace LoaderController.GPWaferTransferScheduler
{
    using global::LoaderController.GPController;
    using LoaderControllerBase;
    using LogModule;

    public class GP_WaferTransferScheduler : IWaferTransferScheduler
    {
        public GP_WaferTransferScheduler(GP_LoaderController controller)
        {
            try
            {
                this.LoaderController = controller;

                StateObj = new NO_CASSETTE(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private bool _IsCstStopOption = false;

        public bool IsCstStopOption
        {
            get { return _IsCstStopOption; }
            set { _IsCstStopOption = value; }
        }

        public GP_LoaderController LoaderController { get; set; }

        public GP_WaferTransferSchedulerStateBase StateObj { get; set; }

        public WaferTransferSchedulerStateEnum State => StateObj.State;

        public WaferTransferScheduleResult Execute()
        {
            WaferTransferScheduleResult result = null;
            try
            {
                result = StateObj.Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return result;
        }

        public bool UpdateState(out bool isNeedWafer, out string cstHashCodeOfRequestLot, bool canloadwafer = true)
        {
            bool isExchange = false;
            isNeedWafer = false;
            cstHashCodeOfRequestLot = "";
            try
            {
                isExchange= StateObj.UpdateState(out isNeedWafer, out cstHashCodeOfRequestLot, canloadwafer);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return isExchange;
        }
    }
}
