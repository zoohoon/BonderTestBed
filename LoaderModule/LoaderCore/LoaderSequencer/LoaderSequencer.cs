using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using LoaderBase;

using Autofac;
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;
using System.Diagnostics;
using LogModule;
using System.Threading;
////using ProberInterfaces.ThreadSync;

namespace LoaderCore
{
    public class LoaderSequencer : ILoaderSequencer
    {
        private LoaderMap DstMap;
        private ILoaderProcessModulePakage ProcessModulePakage;
        private List<ILoaderJob> JobSchedulerList;
        private ILoaderJob JobScheduler;
        private ILoaderProcessModule ProcModule;

        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL1;

        public Autofac.IContainer Container { get; set; }

        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();

        public ILoaderMapAnalyzer LoaderMapAnalyzer { get; set; }

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Container = container;

                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    LoaderMapAnalyzer = new LoaderMapAnalyzer(Loader);
                }
                else
                {
                    LoaderMapAnalyzer = new GPLoaderMapAnalyzer(Loader);
                }
               

                var assem = Assembly.GetCallingAssembly();

                this.ProcessModulePakage = Container.Resolve<ILoaderProcessModulePakage>();

                this.JobSchedulerList = new List<ILoaderJob>();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void DeInitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        //private LockKey procSyncObj = new LockKey("Loader Sequencer");
        private static object procSyncObj = new object();

        public bool HasProcessor()
        {
            //using (Locker locker = new Locker(procSyncObj))
            //{
            //    if (locker.AcquiredLock == false)
            //    {
            //        System.Diagnostics.Debugger.Break();
            //        return false;
            //    }
            lock (procSyncObj)
            {

                bool retVal = false;
                try
                {
                    retVal = ProcModule != null;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }

        public LoaderProcStateEnum GetProcState()
        {
            try
            {
                if (HasProcessor())
                    return ProcModule.State;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return LoaderProcStateEnum.IDLE;
        }

        public ReasonOfSuspendedEnum GetSuspendedInfo()
        {
            try
            {
                if (HasProcessor())
                    return ProcModule.ReasonOfSuspended;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ReasonOfSuspendedEnum.NONE;
        }

        public ResponseResult SetRequest(LoaderMap dstMap)
        {
            ResponseResult rr = null;

            try
            {
                var list = LoaderMapAnalyzer.Build(dstMap);

                if (list.Count > 0)
                {
                    int runnableCount = 0;

                    StringBuilder errSb = new StringBuilder();
                    foreach (var scheduler in list)
                    {
                        var validRel = scheduler.Validate();
                        if (validRel.IsValid)
                        {
                            runnableCount++;
                        }
                        else
                        {
                            errSb.AppendLine(validRel.ReasonOfError);
                        }
                    }

                    if (runnableCount > 0)
                    {
                        rr = new ResponseResult();
                        rr.IsSucceed = true;
                        rr.ErrorMessage = "";

                        SetNewReq(dstMap, list);
                    }
                    else
                    {
                        rr = new ResponseResult();
                        rr.IsSucceed = false;
                        rr.ErrorMessage = errSb.ToString();
                    }
                }
                else
                {
                    rr = new ResponseResult();
                    rr.IsSucceed = false;
                    rr.ErrorMessage = "job empty.";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rr;
        }

        public EventCodeEnum DoSchedule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                //clear process
                if (this.ProcModule is IWaferTransferRemotableProcessModule)
                {
                    Loader.WaferTransferRemoteService.Deactivate();
                }

                if (this.ProcModule is IOCRRemotableProcessModule)
                {
                    Loader.OCRRemoteService.Deactivate();
                }
                ClearProcessor();

                //=> find process
                while (true)
                {
                    //check remani job 
                    bool isRemainJob = this.JobSchedulerList != null && JobSchedulerList.Count > 0;
                    if (isRemainJob == false)
                    {
                        //all job done.
                        Loader.WaferTransferRemoteService.Deactivate();
                        Loader.OCRRemoteService.Deactivate();

                        retVal = EventCodeEnum.NONE;
                        break;
                    }

                    //find priority job
                    if (JobScheduler == null)
                    {
                        JobScheduler = GetPriorityRunnableJob(this.JobSchedulerList);
                        if (JobScheduler == null)
                        {
                            //
                            retVal = EventCodeEnum.UNDEFINED;
                        }
                    }

                    //find processor
                    var jsr = JobScheduler.DoSchedule();
                    if (jsr.RelCode == JobScheduleRelCodeEnum.NEED_PROCESSING)
                    {
                        retVal = ChangeProcessor(jsr.NextProc);
                        if (retVal == EventCodeEnum.NONE)
                        {
                            break;
                        }
                    }
                    else if (jsr.RelCode == JobScheduleRelCodeEnum.JOB_DONE)
                    {
                        this.JobSchedulerList.Remove(JobScheduler);
                        JobScheduler = null;
                    }
                    else// Error
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                    }

                    //_delays.DelayFor(1);
                    Thread.Sleep(1);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public LoaderProcStateEnum DoProcess()
        {
            try
            {
                //while (true)
                //{
                    ProcModule.Execute();

                    LoaderProcStateEnum procState = ProcModule.State;

                    if (procState == LoaderProcStateEnum.SUSPENDED ||
                        procState == LoaderProcStateEnum.DONE ||
                        procState == LoaderProcStateEnum.SYSTEM_ERROR)
                    {
                        LoggerManager.Debug($"Proc. {ProcModule.ToString()} in {ProcModule.State} state.");
                        //break;
                    }

                //    _delays.DelayFor(1);
              //  }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ProcModule.State;
        }

        public void AwakeProcessModule()
        {
            try
            {
                ProcModule.Awake();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SelfRecovery()
        {
            try
            {
                ProcModule.SelfRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Clear()
        {
            try
            {
                DstMap = null;
                JobScheduler = null;
                ProcModule = null;
                JobSchedulerList.Clear();
                Loader.CardTransferRemoteService.Deactivate();
                Loader.WaferTransferRemoteService.Deactivate();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region => Privae Methods

        private EventCodeEnum ChangeProcessor(ILoaderProcessParam procParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            //using (Locker locker = new Locker(procSyncObj))
            //{
            //    if (locker.AcquiredLock == false)
            //    {
            //        System.Diagnostics.Debugger.Break();
            //        return retVal;
            //    }
            lock (procSyncObj)
            {
                try
                {
                    var foundProc = ProcessModulePakage.ProcModuleList.Where(item => item.CanExecute(procParam) == true);
                    if (foundProc.Count() == 1)
                    {
                        this.ProcModule = foundProc.First();
                        this.ProcModule.Init(Container, procParam);

                        if (this.ProcModule is IWaferTransferRemotableProcessModule)
                        {
                            Loader.WaferTransferRemoteService.Activate(ProcModule as IWaferTransferRemotableProcessModule);
                        }
                        if (this.ProcModule is ICardTransferRemotableProcessModule)
                        {
                            Loader.CardTransferRemoteService.Activate(ProcModule as ICardTransferRemotableProcessModule);
                        }
                        if (this.ProcModule is IOCRRemotableProcessModule)
                        {
                            Loader.OCRRemoteService.Activate(ProcModule as IOCRRemotableProcessModule);
                        }

                        retVal = EventCodeEnum.NONE;
                    }
                    else //error
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        private void ClearProcessor()
        {
            //using (Locker locker = new Locker(procSyncObj))
            //{
            //    if (locker.AcquiredLock == false)
            //    {
            //        System.Diagnostics.Debugger.Break();
            //        return;
            //    }
            lock (procSyncObj)
            {

                try
                {
                    this.ProcModule = null;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }

        private void SetNewReq(LoaderMap dstMap, List<ILoaderJob> list)
        {
            try
            {
                Clear();

                this.ProcModule = null;

                this.DstMap = dstMap;
                this.JobSchedulerList = list;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ILoaderJob GetPriorityRunnableJob(List<ILoaderJob> jobList)
        {
            ILoaderJob foundJobSch = null;
            try
            {
                var priorityJobList = jobList.OrderByDescending(item => item.Priority);


                foreach (var jobSch in priorityJobList)
                {
                    var jvr = jobSch.Validate();
                    if (jvr.IsValid)
                    {
                        foundJobSch = jobSch;
                        break;
                    }

                    if (foundJobSch != null)
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return foundJobSch;
        }
        #endregion
    }
}
