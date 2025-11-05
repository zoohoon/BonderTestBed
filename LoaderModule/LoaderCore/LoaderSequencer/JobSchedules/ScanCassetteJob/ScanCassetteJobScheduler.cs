using System;

using LoaderBase;
using LoaderParameters;

namespace LoaderCore
{
    using LogModule;
    using ScanCassetteJobStates;

    public class ScanCassetteJob : ILoaderJob
    {
        public int Priority => 100;

        public ScanCassetteJobStateBase StateObj { get; set; }

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
                if (Cassette.ScanState == CassetteScanStateEnum.READ)
                {
                    StateObj = new ReadState(this);
                }
                else
                {
                    StateObj = new ReadingState(this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }
}
