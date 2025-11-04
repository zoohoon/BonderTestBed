using System;
using LoaderControllerBase;

namespace LoaderController
{
    using LogModule;
    using WaferTransferSchedulerStates;

    public class WaferTransferScheduler : IWaferTransferScheduler
    {
        public WaferTransferScheduler(LoaderController controller)
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
        private bool _IsCstStopOption=false;

        public bool IsCstStopOption
        {
            get { return _IsCstStopOption; }
            set { _IsCstStopOption = value; }
        }

        public LoaderController LoaderController { get; set; }

        public WaferTransferSchedulerStateBase StateObj { get; set; }

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
            bool retVal = false;
            isNeedWafer = false;
            cstHashCodeOfRequestLot = "";
            try
            {
                retVal=StateObj.UpdateState(out isNeedWafer, canloadwafer);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }


}
