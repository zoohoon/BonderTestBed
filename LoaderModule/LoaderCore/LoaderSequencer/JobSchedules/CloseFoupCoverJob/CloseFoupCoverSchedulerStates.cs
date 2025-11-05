using LoaderBase;
using LogModule;
using ProberInterfaces.Foup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoaderCore.CloseFoupCoverSchedulerStates
{
    public abstract class CloseFoupCoverSchedulerStatesBase
    {
        public CloseFoupCoverScheduler Scheduler { get; private set; }

        public CloseFoupCoverSchedulerStatesBase(CloseFoupCoverScheduler scheduler)
        {
            try
            {
                this.Scheduler = scheduler;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public abstract JobValidateResult Validate();

        public abstract JobScheduleResult Execute();
    }
    public class ClosingState : CloseFoupCoverSchedulerStatesBase
    {
        public ClosingState(CloseFoupCoverScheduler scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();
            try
            {
                var Cassette = Scheduler.Cassette;
                if(Cassette.FoupState != FoupStateEnum.LOAD)
                {
                    rel.SetError($"Foup state invalid. cassetteNum={Cassette.ID.Index}, foupState={Cassette.FoupState}");
                    return rel;
                }    
                if(Cassette.FoupCoverState == FoupCoverStateEnum.ERROR)
                {
                    rel.SetError($"Foup Cover state invalid. cassetteNum={Cassette.ID.Index}, foupCoverState={Cassette.FoupCoverState}");
                    return rel;
                }
                rel.SetValid();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }
        public override JobScheduleResult Execute()
        {
            JobScheduleResult rel = new JobScheduleResult();
            try
            {                
                rel.SetJobDone();
                rel.SetCloseFoupCover(Scheduler.Cassette);
                LoggerManager.Debug("FoupCoverState: ClosingState.Execute() Done");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }
    }
    public class ClosedState : CloseFoupCoverSchedulerStatesBase
    {
        public ClosedState(CloseFoupCoverScheduler scheduler) : base(scheduler) { }
        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();

            try
            {
                rel.SetValid();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }
        public override JobScheduleResult Execute()
        {
            JobScheduleResult rel = new JobScheduleResult();

            try
            {
                rel.SetJobDone();
                LoggerManager.Debug("FoupCoverState: ClosedState.Execute() Done");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }
    }    
}
