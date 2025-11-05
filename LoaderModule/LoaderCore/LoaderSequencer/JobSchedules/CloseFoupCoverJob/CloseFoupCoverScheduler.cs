using LoaderBase;
using LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoaderCore
{
    using CloseFoupCoverSchedulerStates;
    public class CloseFoupCoverScheduler : ILoaderJob
    {
        public int Priority => 125;
        public CloseFoupCoverSchedulerStatesBase StateObj { get; set; }

        public ILoaderModule Loader { get; private set; }

        public ICassetteModule Cassette { get; private set; }

        public void Init(ILoaderModule loader, ICassetteModule cassette)
        {
            try
            {
                this.Loader = loader;
                this.Cassette = cassette;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public JobValidateResult Validate()
        {
            JobValidateResult result = new JobValidateResult();
            try
            {
                InitState();
                result = StateObj.Validate();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }

        public JobScheduleResult DoSchedule()
        {
            JobScheduleResult result = new JobScheduleResult();
            try
            {
                InitState();
                result = StateObj.Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }

        private void InitState()
        {
            try
            {
                // arm to slot 에서 open 하고 state open 으로 변경해야함.                
                if (Cassette.FoupCoverState == ProberInterfaces.Foup.FoupCoverStateEnum.CLOSE)
                {
                    StateObj = new ClosedState(this);
                }
                else
                {
                    StateObj = new ClosingState(this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
