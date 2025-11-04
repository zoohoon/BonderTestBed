using System;
using System.Linq;

using LoaderBase;
using ProberInterfaces.Foup;
using ProberErrorCode;
using LogModule;

namespace LoaderCore.ScanCassetteJobStates
{
    public abstract class ScanCassetteJobStateBase
    {
        public ScanCassetteJob Scheduler { get; private set; }

        public ScanCassetteJobStateBase(ScanCassetteJob scheduler)
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

    public class ReadingState : ScanCassetteJobStateBase
    {
        public ReadingState(ScanCassetteJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();

            try
            {
                var Cassette = Scheduler.Cassette;

                if (Scheduler.Loader.ServiceCallback != null)
                {
                    var foupInfo = Scheduler.Loader.ServiceCallback.FOUP_GetFoupModuleInfo(Cassette.ID.Index);
                    if (foupInfo.State != FoupStateEnum.LOAD)
                    {
                        rel.SetError($"Foup state invalid. cassetteNum={Cassette.ID.Index}, foupState={foupInfo.State}");
                        return rel;
                    }

                    if (foupInfo.FoupCoverState == FoupCoverStateEnum.OPEN)
                    {
                        var retVal = Scheduler.Loader.ServiceCallback.FOUP_MonitorForWaferOutSensor(Cassette.ID.Index, false);
                        if (retVal != EventCodeEnum.NONE)
                        {
                            rel.SetError($"Wafer out sensor detected. cassetteNum={Cassette.ID.Index}");
                            return rel;
                        }
                    }
                }
                ICassetteScanable scanable;
                scanable = Scheduler.Loader.ModuleManager.FindModules<ICassetteScanable>().Where(
                    item =>
                    item.CanScan(Cassette)).FirstOrDefault();

                if (scanable == null)
                {
                    rel.SetError($"Can not found scan module. cassetteNum={Cassette.ID.Index}");
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
                ICassetteScanable scanable;
                scanable = Scheduler.Loader.ModuleManager.FindModules<ICassetteScanable>().Where(
                    item =>
                    item.CanScan(Scheduler.Cassette)).FirstOrDefault();
                var scanners = Scheduler.Loader.ModuleManager.FindModules<ICassetteScanable>();
                var scansensor = scanners.Where(s => s.ID.Index == Scheduler.Cassette.ID.Index).FirstOrDefault();
                if(scansensor != null)
                {
                    if (scansensor.CanScan(Scheduler.Cassette) == true)
                    {
                        scanable = (ICassetteScanable)scansensor;
                    }
                }

                rel.SetScan(Scheduler.Cassette, scanable);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }
    }

    public class ReadState : ScanCassetteJobStateBase
    {
        public ReadState(ScanCassetteJob scheduler) : base(scheduler) { }

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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }
    }
}
