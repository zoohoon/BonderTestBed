using LoaderBase;
using LoaderMaster.LoaderSupervisorStates;
using LoaderParameters;
using LoaderRecoveryControl;
using LogModule;
using MetroDialogInterfaces;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.Foup;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using ProberInterfaces.Event;
using LoaderParameters.Data;
using System.Collections.Generic;
using Autofac;
using System.Threading.Tasks;

namespace LoaderMaster.ExternalStates
{
    public abstract class LoaderSupervisorExternalStateBase : LoaderSupervisorStateBase
    {
        public LoaderSupervisorExternalStateBase(LoaderSupervisor module) : base(module)
        {
        }
        public override EventCodeEnum SetEMGSTOP()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                StateTransition(new External_Error(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
    public class External_Idle : LoaderSupervisorExternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.IDLE;
        bool isRepeatError = false;
        ICardChangeSupervisor cardsuper = null;
        public External_Idle(LoaderSupervisor module) : base(module)
        {
            if (this.Module.GetGPLoader() != null)
            {
                this.Module.GetGPLoader().LoaderLampSetState(ModuleState);
                this.Module.GetGPLoader().IsLoaderBusy = false;
            }
            Module.IsAbortError = false;
            cardsuper = Module.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
        }

        public override void Execute()
        {
            Func<bool> conditionFunc = () =>
            {
                bool canRunning = false;
                canRunning = Module.IsCanLotStart();

                return canRunning;
            };

            Action doAction = () =>
            {
                Module.ActiveLotQueueReset();
                Module.LotStartTime = DateTime.Now;
                StateTransition(new External_Running(Module));
            };
            Action abortAction = () =>
            {
                //Module.MetroDialogManager().ShowMessageDialog("[LOTSTART]", "FAIL=" + Module.LotStartFailReason, EnumMessageStyle.AffirmativeAndNegative);
            };
            bool consumed;

            try
            {
                if (Module.LoaderSystemInitFailure == true)
                {
                    string message = "";
                    List<ModuleID> inspection_id = new List<ModuleID>();
                    var unkown_inspection = Module.Loader.GetLoaderInfo().StateMap.InspectionTrayModules.Where(x => x.WaferStatus == EnumSubsStatus.UNKNOWN);
                    foreach (var item in unkown_inspection)
                    {
                        inspection_id.Add(item.ID);
                    }

                    if (unkown_inspection != null)
                    {
                        string inspectionIdsString = string.Join(", ", inspection_id);
                        message = inspectionIdsString + " has an unknown status.\nPlease open and close it.";
                    }
                    Module.Loader.ResonOfError = $"Loader System Init failed. Loader module state is {Module.Loader.ModuleState}";
                    Module.Loader.ErrorDetails = message + "\nLoader initialize is not possible due to module status.";
                    Module.Loader.RecoveryBehavior = "LoaderSystemInitRecovery";
                    StateTransition(new External_Error(Module));
                }


                Module.LotCancelRequestJob();

                bool canExcuteMap = false;
                var remainJob = Module.Loader.LoaderJobViewList.Count(x => x.JobDone == false);
                if (remainJob == 0)
                {
                    canExcuteMap = true;
                }

                if (canExcuteMap)// 다른 작업이 완료되었을때만 물어봐야함.
                {
                    LoaderMap loaderMap = Module.ExternalRequestJob_Idle();
                    if (loaderMap == null && cardsuper.ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        consumed = Module.CommandManager().ProcessIfRequested<IGPLotOpStart>(
                                                                                  Module,
                                                                                  conditionFunc,
                                                                                  doAction,
                                                                                  abortAction);

                        return;
                    }


              

               
                    var slicedMap = Module.MapSlicer.ManualSlicing(loaderMap);
                    if (slicedMap != null && slicedMap.Count() > 0)
                    {
                        try
                        {
                            Module.MetroDialogManager().SetDataWaitCancelDialog(message: "Wait PW loading/unloading for soaking...", hashcoe: this.GetHashCode().ToString());

                            Module.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait PW loading/unloading for soaking...");

                            bool isAbort = false;
                            isRepeatError = false;
                            for (int i = 0; i < slicedMap.Count; i++)
                            {
                                if (isAbort)
                                {
                                    break;
                                }
                                else
                                {
                                    if (Module.DynamicMode == DynamicModeEnum.DYNAMIC)
                                    {
                                        for (int cnt = 0; cnt < Module.ActiveLotInfos.Count(); cnt++)
                                        {
                                            if (Module.ActiveLotInfos[cnt].ResevationState == FoupReservationEnum.RESERVE)
                                            {
                                                if (Module.Loader.IsFoupUnload(Module.ActiveLotInfos[cnt].FoupNumber))
                                                {
                                                    Module.ActiveLotInfos[cnt].Forced_FoupUnLoad();
                                                }
                                                else
                                                {
                                                    Module.ActiveLotInfos[cnt].ResevationState = FoupReservationEnum.NOT_PROCESS;
                                                }
                                            }
                                        }
                                    }
                                }

                                var result = Module.Loader.SetRequest(slicedMap[i]);
                                if (result.IsSucceed == false)
                                {

                                    int remain_joblist = Module.Loader.LoaderJobViewList.Where(x => x.JobDone == false).Count();
                                    StateTransition(new External_Error(Module));
                                    LoggerManager.Debug($"Loader SetRequest Error. Error Msg: {result.ErrorMessage}.  LoaderJobList Count = {remain_joblist}");
                                    break;

                                }

                                while (true)
                                {
                                    if (Module.Loader.ModuleState == ModuleStateEnum.DONE)
                                    {
                                        Module.Loader.ClearRequestData();
                                        break;
                                    }
                                    else if (Module.Loader.ModuleState == ModuleStateEnum.ERROR)
                                    {
                                        LoaderRecoveryControlVM.Show(Module.cont, Module.Loader.ResonOfError, Module.Loader.ErrorDetails);
                                        Module.Loader.ResonOfError = "";
                                        Module.Loader.ErrorDetails = "";
                                        isRepeatError = true;
                                        break;

                                        //StateTransition(new External_Error(Module));
                                        //return;
                                    }
                                    else if (Module.Loader.ModuleState == ModuleStateEnum.ABORT)
                                    {
                                        Module.Loader.ClearRequestData();
                                        isAbort = true;
                                        return;
                                    }
                                    else if (Module.Loader.ModuleState == ModuleStateEnum.SUSPENDED)
                                    {

                                    }

                                    Thread.Sleep(100);
                                }
                                if (isRepeatError)
                                {
                                    break;
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                        finally
                        {
                            Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                        }
                    }
                }


                
                if(cardsuper.ModuleState.GetState() == ModuleStateEnum.IDLE)
                {
                    consumed = Module.CommandManager().ProcessIfRequested<IGPLotOpStart>(
                                                                              Module,
                                                                              conditionFunc,
                                                                              doAction,
                                                                              abortAction);
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return;
        }

        public override ResponseResult SetRequest(LoaderMap dstMap)
        {
            ResponseResult rr = null;
            try
            {
                //rr = Module.Sequencer.SetRequest(dstMap);

                //if (rr.IsSucceed)
                //{
                //    StateTransition(new SCHEDULING(Module));
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rr;
        }
        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new External_Idle(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        #region => Jog Methods
        public override EventCodeEnum JogRelMove(EnumAxisConstants axis, double value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Module.Move.JogRelMove(axis, value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum JogAbsMove(EnumAxisConstants axis, double value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // retVal = Module.Move.JogAbsMove(axis, value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        public override EventCodeEnum ClearRequestData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is IGPLotOpStart;

            return isValidCommand;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }
    }

    public class External_ReadyIdleToRunning : LoaderSupervisorExternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.RUNNING;

        public External_ReadyIdleToRunning(LoaderSupervisor module) : base(module)
        {
            if (this.Module.GetGPLoader() != null)
            {
                this.Module.GetGPLoader().LoaderLampSetState(ModuleState);
                this.Module.GetGPLoader().IsLoaderBusy = true;
            }
        }

        public override void Execute()
        {
            try
            {
                var scanjob = Module.Loader.GetLoaderInfo().StateMap;
                Module.Loader.ScanCount = 25;

                //if (Module.Loader.ScanCount == 0)
                //{
                //}
                //Module.Loader.ScanCount=Module.Loader.ScanCount--;
                bool[] scanflag = new bool[3];

                ///어떤 foup이 로드 됬는지 확인한후에 로딩된 foup scan하기 ~
                ///
                int cassette1_idx = 1;
                int cassette2_idx = 2;
                int cassette3_idx = 3;
                Module.Loader.ScanFlag[0] = false;
                Module.Loader.ScanFlag[1] = false;
                Module.Loader.ScanFlag[2] = false;
                scanflag[0] = Module.Loader.ScanFlag[0]; //testcode
                scanflag[1] = Module.Loader.ScanFlag[1]; //testcode
                scanflag[2] = Module.Loader.ScanFlag[2]; //testcode

                var Cassette1 = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, cassette1_idx);
                if (Cassette1.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                {
                    Cassette1.SetNoReadScanState();
                    if (Cassette1.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette1.ScanState == CassetteScanStateEnum.NONE)
                    {
                        scanflag[0] = true;
                        Module.Loader.DoScanJob(cassette1_idx);
                    }
                }

                var Cassette2 = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, cassette2_idx);
                if (Cassette2.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                {
                    Cassette2.SetNoReadScanState();
                    if (Cassette2.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette2.ScanState == CassetteScanStateEnum.NONE)
                    {
                        scanflag[0] = true;
                        Module.Loader.DoScanJob(cassette2_idx);
                    }
                }

                var Cassette3 = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, cassette3_idx);
                if (Cassette3.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                {
                    Cassette3.SetNoReadScanState();
                    if (Cassette3.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette3.ScanState == CassetteScanStateEnum.NONE)
                    {
                        scanflag[0] = true;
                        Module.Loader.DoScanJob(cassette3_idx);
                    }
                }

                while (true)
                {
                    if (scanflag[0])
                    {
                        if (Cassette1.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette1.ScanState == CassetteScanStateEnum.READ)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                    //de.DelayFor(10);
                    Thread.Sleep(10);
                }
                while (true)
                {
                    if (scanflag[1])
                    {
                        if (Cassette2.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette2.ScanState == CassetteScanStateEnum.READ)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                    //de.DelayFor(10);
                    Thread.Sleep(10);
                }
                while (true)
                {
                    if (scanflag[2])
                    {
                        if (Cassette3.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette3.ScanState == CassetteScanStateEnum.READ)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                    //de.DelayFor(10);
                    Thread.Sleep(10);
                }



                Module.LotOPStart();
                Module.ArrangeCallbackIndex();
                StateTransition(new External_Running(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpStart;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }
    }

    public class External_Running : LoaderSupervisorExternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.RUNNING;
        public External_Running(LoaderSupervisor module, bool bResume = false) : base(module)
        {
            if (bResume)
            {
                LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.RESUME, $"Loader Lot Resume", isLoaderMap: true);
            }

            if (this.Module.GetGPLoader() != null)
            {
                this.Module.GetGPLoader().LoaderLampSetState(ModuleState);
                this.Module.GetGPLoader().IsLoaderBusy = true;
            }
            Module.GEMModule().ClearAlarmOnly();
            Module.IsAbortError = false;
        }
        //private EventCodeEnum UploadLog(DateTime startTime, DateTime endTime)
        //{
        //    EventCodeEnum ret = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        Task task = new Task(() =>
        //        {
        //            if (Module.LoaderLogManager.LoaderLogParam.UploadEnable.Value == true)
        //            {
        //                ret = Module.LoaderLogManager.StagesLogUploadServer(startTime, endTime);

        //                ret = Module.LoaderLogManager.LoaderLogUploadServer(startTime, endTime);

        //                ret = Module.LoaderLogManager.StagesPinTipSizeValidationImageUploadServer(startTime, endTime);
        //            }
        //            else
        //            {
        //                LoggerManager.Debug($"ExternalState.UploadLog() UploadEnable is False");
        //                ret = EventCodeEnum.NONE;
        //            }
        //        });
        //        task.Start();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return ret;
        //}

        //private EventCodeEnum UploadPinImage(DateTime startTime, DateTime endTime)
        //{
        //    EventCodeEnum ret = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        if (Module.LoaderLogManager.LoaderLogParam.UploadEnable.Value == true)
        //        {
        //            ret = Module.LoaderLogManager.StagesPinTipSizeValidationImageUploadServer(startTime, endTime);
        //            if (ret != EventCodeEnum.NONE)
        //            {
        //                LoggerManager.Error($"Error occurd while pin image upload func{MethodBase.GetCurrentMethod().Name} retcode:{ret}");
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return ret;
        //}

        //public async Task<EventCodeEnum> CassetteUnload(int foupNum)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        await Task.Factory.StartNew(() =>
        //        {
        //            var Cassette = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupNum);
        //            if (Cassette.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD)
        //            {
        //                retVal = Module.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
        //            }
        //        }
        //          );

        //    }
        //    catch (Exception err)
        //    {

        //    }
        //    return retVal;
        //}

        public override void Execute()
        {
            try
            {
                bool isLoaderPauseTrigger = false;
                var allWafer = Module.Loader.GetLoaderInfo().StateMap.GetTransferObjectAll();
                var buffCnt = Module.Loader.GetLoaderInfo().StateMap.BufferModules?.Count();
                var PaCnt = Module.Loader.GetLoaderInfo().StateMap.PreAlignModules?.Count();
                bool[] isLotEnd;
                bool[] isLotInitFlag;
                bool[] isLotPause = null;
                bool isLoaderEnd = false;
                LoaderMap loaderMap = null;
                bool isEnableBuffer = true;
                bool isEnablePa = true;

                Module.LotCancelRequestJob();


                if (Module.Loader.GetLoaderInfo().StateMap.ARMModules.Where(i => i.WaferStatus == EnumSubsStatus.UNKNOWN).Count() > 0)
                {
                    Module.Loader.ResonOfError = "There is Arm Unknown Status, please check the status of Arm.";
                    Module.MetroDialogManager().ShowMessageDialog("Arm Unknown Error", $"There is Arm Unknown Status, please check the status of Arm.", EnumMessageStyle.Affirmative);
                    StateTransition(new External_Paused(Module));
                    return;
                }

                if (buffCnt > 0)
                {
                    isEnableBuffer = false;
                    for (int i = 0; i < Module.Loader.GetLoaderInfo().StateMap.BufferModules.Count(); i++)
                    {
                        isEnableBuffer = Module.Loader.GetLoaderInfo().StateMap.BufferModules[i].Enable;
                        if (isEnableBuffer == true)
                        {
                            break;
                        }
                    }
                }

                if (PaCnt > 0)
                {
                    for (int i = 0; i < Module.Loader.GetLoaderInfo().StateMap.PreAlignModules.Count(); i++)
                    {
                        var PA = Module.Loader.ModuleManager.FindModule<IPreAlignModule>(ModuleTypeEnum.PA, i +1);
                        if (PA != null)
                        {
                            isEnablePa = PA.PAStatus != ProberInterfaces.PreAligner.EnumPAStatus.Error;
                            if (isEnablePa == true)
                            {
                                break;
                            }
                        }                                                                                                                           
                    }
                }

                if (isEnablePa == false) 
                {
                    Module.Loader.ResonOfError = "There is no PreAligner available, please check the status of PreAligner.";
                    Module.MetroDialogManager().ShowMessageDialog("PreAlign Error", $"There is no PreAligner available, please check the status of PreAligner.", EnumMessageStyle.Affirmative);
                    StateTransition(new External_Paused(Module));
                    return;
                }

                if (buffCnt == 0 || isEnableBuffer == false)
                {
                    if (Module.GetFoupShiftMode() == FoupShiftModeEnum.SHIFT)
                    { //[FOUP_SHIFT]*
                        loaderMap = Module.ExternalRequestJob_FoupShift_NoBuffer(out isLotEnd, out isLotInitFlag, out isLoaderEnd, out isLotPause);
                    }
                    else
                    {
                        if (Module.Loader.GetLoaderInfo().StateMap.PreAlignModules.Count() == 1)
                            loaderMap = Module.ExternalRequestJobNoBuffer(out isLotEnd, out isLotInitFlag, out isLoaderEnd, out isLotPause);
                        else
                            loaderMap = Module.ExternalRequestJob_Hynix(out isLotEnd, out isLotInitFlag, out isLoaderEnd, out isLotPause);
                    }
                }
                else
                {
                    if (Module.DynamicMode == DynamicModeEnum.NORMAL && Module.GetFoupShiftMode() == FoupShiftModeEnum.NORMAL)
                    {
                        loaderMap = Module.ExternalRequestJob(out isLotEnd, out isLotInitFlag, out isLoaderEnd, out isLotPause);
                        /*
                        if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                        {
                            loaderMap = Module.ExternalRequestJob_DRAX(out isLotEnd, out isLotInitFlag, out isLoaderEnd, out isLotPause);
                        }
                        else
                        {
                            loaderMap = Module.ExternalRequestJob1(out isLotEnd, out isLotInitFlag, out isLoaderEnd, out isLotPause);
                        }
                        */
                    }
                    else
                    {
                        loaderMap = Module.ExternalRequestJob_Dynamic(out isLotEnd, out isLotInitFlag, out isLoaderEnd, out isLotPause);
                    }
                }

                for (int i = 0; i < isLotEnd.Length; i++)
                {
                    if (Module.ActiveLotInfos[i].State == LotStateEnum.Running || Module.ActiveLotInfos[i].State == LotStateEnum.Cancel)
                    {
                        if (isLotPause[i])
                        {
                            StateTransition(new External_Paused(Module));
                            return;
                        }                  
                        else if (isLotEnd[i] && isLotInitFlag[i])                                                                    
                        {
                            // Lot가 끝나 Foup이 언로드 되어야 하는데 만들어진 Map이 있다면, 카세트가 언로드 되는 동시에 해당 카세트에 로더가 접근하여 사고가 발생할 수 있음.
                            loaderMap = null;

                            if (Module.ActiveLotInfos[i].State == LotStateEnum.Running | Module.ActiveLotInfos[i].State == LotStateEnum.Cancel)
                            {                                
                                PIVInfo pivinfo = new PIVInfo(                                    
                                    foupnumber: Module.ActiveLotInfos[i].FoupNumber,
                                    foupshiftmode: (int)Module.GetFoupShiftMode()
                                    );
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                Module.EventManager().RaisingEvent(typeof(EndWaferReturnSlotEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();

                                //Cassette 빼기전에 Cell 에 Lot 끝난다고 알리기.

                                //==<Cell Check>==
                                Module.NotifyLotEndToCell(Module.ActiveLotInfos[i].FoupNumber, Module.ActiveLotInfos[i].LotID);
                                //================
                            }

                            var Cassette = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, Module.ActiveLotInfos[i].FoupNumber);

                            if (Cassette.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD && !(Module.ContinueLotFlag))
                            {
                                Module.FoupOpModule().FoupControllers[i].SetLock(false);

                                if (Module.ActiveLotInfos[i].State == LotStateEnum.Running )
                                {
                                    if (Module.GetIsCassetteAutoUnloadAfterLot() && !Module.ActiveLotInfos[i].IsFoupEnd)
                                    {
                                        Module.ActiveLotInfos[i].IsFoupEnd = true;
                                        var retVal = Module.CassetteNormalUnload(Module.ActiveLotInfos[i].FoupNumber);
                                    }
                                    PIVInfo pivinfo = new PIVInfo(foupnumber: i + 1);

                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    Module.EventManager().RaisingEvent(typeof(CarrierCompleateEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();
                                }
                            }

                            Module.ExternalLotOPEnd(Module.ActiveLotInfos[i].FoupNumber);
                            Module.Loader.Foups[i].LotEndTime = DateTime.Now;

                            //UploadLog(Module.Loader.Foups[i].LotStartTime, Module.Loader.Foups[i].LotEndTime);

                            if (Module.GetFoupShiftMode() == FoupShiftModeEnum.NORMAL)
                            {
                                if (Module.IsSameDeviceEndToSlot(Module.ActiveLotInfos[i].DeviceName, Module.ActiveLotInfos[i].UsingStageIdxList) == true)
                                {
                                    PIVInfo pivinfo = new PIVInfo(foupnumber: Module.ActiveLotInfos[i].FoupNumber);
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    Module.EventManager().RaisingEvent(typeof(SameDeviceEndToSlot).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();
                                }
                            }

                            if (Module.ActiveLotInfos[i].State == LotStateEnum.Running) //정상 종료
                            {
                                Module.DeAllocate(Module.Loader.Foups[i]);

                                Module.FoupOpModule().FoupControllers[i].SetLock(false);
                                Module.ActiveLotInfos[i].State = LotStateEnum.End;
                                Module.ActiveLotInfos[i].LotPriority = 0;
                                Module.Loader.Foups[i].LotState = Module.ActiveLotInfos[i].State;
                                //현재 index ActiveLotInfos State End가 되어 있는 상태여야 함.
                                Module.CheckAndEndLotIfRunningCellsExist(Module.Loader.Foups[i]);
                                Module.Loader.Foups[i].AllocatedCellInfo = string.Empty;
                                Module.Loader.Foups[i].LotPriority = 0;

                                PIVInfo pivinfo = new PIVInfo(foupnumber: i + 1);

                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                Module.EventManager().RaisingEvent(typeof(LoaderLotEndEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();


                                if (Module.GEMModule().IsExternalLotMode == true)
                                {
                                    int count = Module.ActiveLotInfos.FindAll(lotinfo => lotinfo.State == LotStateEnum.Running)?.Count ?? 0;
                                    if (count == 0)
                                    {
                                        if (Module.ContinueLotFlag == false)
                                        {
                                            Module.GEMModule().IsExternalLotMode = false;
                                        }
                                    }
                                }
                            }
                            else if (Module.ActiveLotInfos[i].State == LotStateEnum.Cancel) // 캐리어 캔슬 이벤트 종료 
                            {
                                Module.DeAllocate(Module.Loader.Foups[i]);
                                //==<Cell Check>==
                                //Module.NotifyLotEndToCell(Module.ActiveLotInfos[i].FoupNumber, Module.ActiveLotInfos[i].LotID);
                                //================
                                Module.FoupOpModule().FoupControllers[i].SetLock(false);
                                Module.ContinueLotFlag = false;

                                Module.ActiveLotInfos[i].State = LotStateEnum.End;
                                Module.ActiveLotInfos[i].LotPriority = 999;

                                Module.Loader.Foups[i].LotState = Module.ActiveLotInfos[i].State;
                                //현재 index ActiveLotInfos State End가 되어 있는 상태여야 함.
                                Module.CheckAndEndLotIfRunningCellsExist(Module.Loader.Foups[i]);
                                Module.Loader.Foups[i].AllocatedCellInfo = "";
                                

                                PIVInfo pivinfo = new PIVInfo(foupnumber: i + 1);
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                Module.EventManager().RaisingEvent(typeof(CarrierCanceledEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();

                                if (Module.GEMModule().IsExternalLotMode == true)
                                {
                                    int count = Module.ActiveLotInfos.FindAll(lotinfo => lotinfo.State == LotStateEnum.Running)?.Count ?? 0;
                                    if (count == 0)
                                    {
                                        if (Module.ContinueLotFlag == false)
                                        {
                                            Module.GEMModule().IsExternalLotMode = false;
                                        }
                                    }
                                }
                            }
                            
                            //<!-- LOT끝날때 AbortStageList 에서 해당 LOT 정보는 삭제 -->
                            Module.RemoveLotInfoAtLotBannedList(Module.ActiveLotInfos[i].FoupNumber, Module.ActiveLotInfos[i].LotID, Module.ActiveLotInfos[i].CST_HashCode);
                            Module.DeactiveDevice(i);
                        }
                    }
                    else if (Module.ActiveLotInfos[i].State == LotStateEnum.End)
                    {                                               
                        var Cassette = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, Module.ActiveLotInfos[i].FoupNumber);

                        if (Cassette.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD && !(Module.ContinueLotFlag))
                        {
                            if (Module.GetIsCassetteAutoUnloadAfterLot() && !Module.ActiveLotInfos[i].IsFoupEnd)
                            {
                                Module.ActiveLotInfos[i].IsFoupEnd = true;
                                var retVal = Module.CassetteNormalUnload(Module.ActiveLotInfos[i].FoupNumber);
                            }
                        }

                        Module.DeAllocate(Module.Loader.Foups[i]);
                        LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.DONE, $"FOUP: {Module.ActiveLotInfos[i].FoupNumber}", isLoaderMap: true);
                        Module.DeactiveProcess(i);
                    }
                    else if(Module.ActiveLotInfos[i].State == LotStateEnum.Abort)
                    {
                        Module.FoupOpModule().FoupControllers[i].SetLock(false);
                        Module.DeAllocate(Module.Loader.Foups[i]);

                        PIVInfo pivinfo = new PIVInfo(foupnumber: i + 1);

                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(LoaderLotEndEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                        //<!-- LOT끝날때 AbortStageList 에서 해당 LOT 정보는 삭제 -->
                        Module.RemoveLotInfoAtLotBannedList(Module.ActiveLotInfos[i].FoupNumber, Module.ActiveLotInfos[i].LotID, Module.ActiveLotInfos[i].CST_HashCode);
                        // 이 위치 맞는지 확인 필요.

                        //Module.ActiveLotInfos[i].State = LotStateEnum.Running;
                    }
                }

                if (loaderMap == null && isLoaderEnd && Module.ExtenalIsLotEndReady())
                {
                    bool isLOTEnd = true; 
                    for (int i = 0; i < Module.ActiveLotInfos.Count; i++)
                    {
                        if (Module.ActiveLotInfos[i].State == LotStateEnum.End)
                        {
                            Module.FoupOpModule().FoupControllers[i].SetLock(false);
                            Module.ActiveLotInfos[i].State = LotStateEnum.Done;
                            Module.Loader.Foups[i].LotPriority = 0;
                            if (!Module.ContinueLotFlag)
                            {
                                Module.ActiveLotInfos[i].UsingSlotList.Clear();
                                Module.ActiveLotInfos[i].NotDoneSlotList.Clear();
                            }

                            Module.Loader.Foups[i].LotState = Module.ActiveLotInfos[i].State;
                        }

                        // Running 인게 남아있거나, Idle이지만 할당은 되어있는 경우. End 되지 않고 Loader의 Running 상태를 유지하게 하기 위해
                        if (Module.ActiveLotInfos[i].State == LotStateEnum.Running ||
                           (Module.ActiveLotInfos[i].State == LotStateEnum.Idle && Module.ActiveLotInfos[i].AssignState == LotAssignStateEnum.ASSIGNED))
                        {
                            isLOTEnd = false; //LOT End 시점에 Host로부터 LOT Start 가 다른 CST로 들어오는 경우 마지막으로 확인 
                        }
                    }

                    if (Module.DynamicMode == DynamicModeEnum.DYNAMIC 
                        || Module.GetFoupShiftMode() == FoupShiftModeEnum.SHIFT)
                    {
                        var ldMap = Module.Loader.GetLoaderInfo().StateMap;
                        for (int cnt = 0; cnt < Module.ActiveLotInfos.Count(); cnt++)
                        {
                            if (Module.ActiveLotInfos[cnt].ResevationState == FoupReservationEnum.RESERVE 
                                || Module.ActiveLotInfos[cnt].ResevationState == FoupReservationEnum.NOT_PROCESS)
                            {
                                Thread.Sleep(2000);
                                var ret = Module.ActiveLotInfos[cnt].Forced_FoupUnLoad();
                            }
                            else if (Module.ActiveLotInfos[cnt].DynamicFoupState == DynamicFoupStateEnum.UNLOAD 
                                && ldMap.CassetteModules[cnt].FoupState == FoupStateEnum.LOAD 
                                && Module.DynamicMode == DynamicModeEnum.DYNAMIC)
                            {
                                bool isUnloadFoup = true;

                                foreach (var slotModule in ldMap.CassetteModules[cnt].SlotModules)
                                {
                                    if (slotModule.WaferStatus != EnumSubsStatus.EXIST)
                                    {
                                        isUnloadFoup = false;
                                        break;
                                    }
                                }
                                if (isUnloadFoup)
                                {
                                    var ret = Module.ActiveLotInfos[cnt].Forced_FoupUnLoad();
                                }

                            }
                        }
                    }

                    if (isLOTEnd)
                    {
                        StateTransition(new External_Done(Module));
                        return;
                    }
                }

                bool canExcuteMap = false;
                var remainJob = Module.Loader.LoaderJobViewList.Count(x => x.JobDone == false);
                if (remainJob == 0)
                {
                    canExcuteMap = true;
                }

                if (canExcuteMap)
                {
                    bool isLoaderMapMadeByIdleRequest = false;
                    if (loaderMap == null)
                    {
                        // Lot Run에 포함되지 않은 Cell이 PW를 필요로 할 때 PW를 전달하기 위해 추가
                        loaderMap = Module.ExternalRequestJob_Idle();

                        if (loaderMap != null)
                        {
                            isLoaderMapMadeByIdleRequest = true;
                        }
                    }

                    List<LoaderMap> slicedMap = null;
                    if (isLoaderMapMadeByIdleRequest)
                    {
                        slicedMap = Module.MapSlicer.ManualSlicing(loaderMap);
                    }
                    else
                    {
                        slicedMap = Module.MapSlicer.Slicing(loaderMap);
                    }


                    Module.MapSlicerErrorFlag = false;
                    if (loaderMap != null)
                    {
                        if (slicedMap == null)
                        {
                            //LoggerManager.LoaderMapLog($"MapSlicerError Flag is true", basicLog: false);
                            Module.MapSlicerErrorFlag = true;
                        }
                    }

                    if (slicedMap != null)
                    {
                        bool isAbort = false;

                        if (Module.DynamicMode == DynamicModeEnum.DYNAMIC
                                    || Module.GetFoupShiftMode() == FoupShiftModeEnum.SHIFT)
                        {
                            for (int cnt = 0; cnt < Module.ActiveLotInfos.Count(); cnt++)
                            {
                                if (Module.ActiveLotInfos[cnt].ResevationState == FoupReservationEnum.RESERVE)
                                {
                                    if (Module.Loader.IsFoupUnload(Module.ActiveLotInfos[cnt].FoupNumber))
                                    {
                                        Module.ActiveLotInfos[cnt].Forced_FoupUnLoad();
                                    }
                                    else
                                    {
                                        Module.ActiveLotInfos[cnt].ResevationState = FoupReservationEnum.NOT_PROCESS;//현재 잡을 다 움직이고 나서 언로드 해야함. 
                                    }
                                }
                            }
                        }


                        for (int i = 0; i < slicedMap.Count; i++)
                        {
                            if (isAbort)
                            {
                                break;
                            }
                            else
                            {
                                if (Module.DynamicMode == DynamicModeEnum.DYNAMIC)
                                {
                                    for (int cnt = 0; cnt < Module.ActiveLotInfos.Count(); cnt++)
                                    {
                                        if (Module.ActiveLotInfos[cnt].ResevationState == FoupReservationEnum.RESERVE)
                                        {
                                            if (Module.Loader.IsFoupUnload(Module.ActiveLotInfos[cnt].FoupNumber))
                                            {
                                                Thread.Sleep(2000);
                                                Module.ActiveLotInfos[cnt].Forced_FoupUnLoad();
                                            }
                                            else
                                            {
                                                Module.ActiveLotInfos[cnt].ResevationState = FoupReservationEnum.NOT_PROCESS;
                                            }
                                        }


                                    }
                                }
                            }

                            var result = Module.Loader.SetRequest(slicedMap[i]);
                            if (result.IsSucceed == false)
                            {
                                Module.Loader.ResonOfError = $"Loader module state is {Module.Loader.ModuleState}";
                                Module.Loader.ErrorDetails = "Loader operation request failure. Please try again.";
                                int remain_joblist = Module.Loader.LoaderJobViewList.Where(x => x.JobDone == false).Count();
                                StateTransition(new External_Error(Module));
                                LoggerManager.Debug($"Loader SetRequest Error. Error Msg: {result.ErrorMessage}.  LoaderJobList Count = {remain_joblist}");
                                break;
                            }
                            while (true)
                            {
                                if (Module.Loader.ExceedRunningStateDuration())
                                {
                                    Module.Loader.ResonOfError = $"An error occurred that caused the loader to stop{Environment.NewLine}for 5 minutes.";
                                    Module.Loader.SetEMGSTOP(); //loader modulestate가 error가 되어 next tick에서 external error가 발생한다.
                                }

                                if (Module.Loader.ModuleState == ModuleStateEnum.DONE)
                                {
                                    Module.Loader.ClearRequestData();
                                    break;
                                }
                                else if (Module.Loader.ModuleState == ModuleStateEnum.ERROR)
                                {
                                    StateTransition(new External_Error(Module));
                                    return;
                                }
                                else if (Module.Loader.ModuleState == ModuleStateEnum.ABORT)
                                {
                                    Module.Loader.ClearRequestData();
                                    isAbort = true;
                                    return;
                                }
                                Thread.Sleep(100);
                            }
                            Func<bool> conditionFunc = () =>
                            {
                                bool canRunning = true;
                                Module.ExternalLotOPPause();
                                return canRunning;
                            };

                            Action doAction = () =>
                            {
                                if (Module.IsSelectedLoader)
                                {
                                    isLoaderPauseTrigger = true;
                                }
                                else
                                {
                                    isLoaderPauseTrigger = false;
                                }
                            };
                            Action abortAction = () =>
                            {
                            };
                            bool consumed;

                            consumed = Module.CommandManager().ProcessIfRequested<IGPLotOpPause>(
                            Module,
                            conditionFunc,
                            doAction,
                            abortAction);

                            Func<bool> resumeConditionFunc = () =>
                            {
                                bool canRunning = true;
                                Module.ExternalLotOPResume();
                                return canRunning;
                            };

                            Action resumeDoAction = () =>
                            {
                            };
                            Action resumeAbortAction = () =>
                            {
                            };
                            bool resumConsumed;

                            resumConsumed = Module.CommandManager().ProcessIfRequested<IGPLotOpResume>(
                            Module,
                            resumeConditionFunc,
                            resumeDoAction,
                            resumeAbortAction);
                        }

                        if (Module.DynamicMode == DynamicModeEnum.DYNAMIC
                            || Module.GetFoupShiftMode() == FoupShiftModeEnum.SHIFT)
                        {
                            var ldMap = Module.Loader.GetLoaderInfo().StateMap;
                            for (int cnt = 0; cnt < Module.ActiveLotInfos.Count(); cnt++)
                            {
                                if (Module.ActiveLotInfos[cnt].ResevationState == FoupReservationEnum.RESERVE
                                    || Module.ActiveLotInfos[cnt].ResevationState == FoupReservationEnum.NOT_PROCESS)
                                {
                                    Thread.Sleep(2000);
                                    var ret = Module.ActiveLotInfos[cnt].Forced_FoupUnLoad();
                                }
                                else if (Module.ActiveLotInfos[cnt].DynamicFoupState == DynamicFoupStateEnum.UNLOAD
                                    && ldMap.CassetteModules[cnt].FoupState == FoupStateEnum.LOAD
                                    && Module.DynamicMode == DynamicModeEnum.DYNAMIC)
                                {
                                    bool isUnloadFoup = true;

                                    foreach (var slotModule in ldMap.CassetteModules[cnt].SlotModules)
                                    {
                                        if (slotModule.WaferStatus != EnumSubsStatus.EXIST)
                                        {
                                            isUnloadFoup = false;
                                            break;
                                        }
                                    }
                                    if (isUnloadFoup)
                                    {
                                        var ret = Module.ActiveLotInfos[cnt].Forced_FoupUnLoad();
                                    }

                                }
                            }

                        }
                    }
                }

                Func<bool> conditionFunc1 = () =>
                {
                    bool canRunning = true;
                    Module.ExternalLotOPPause();
                    return canRunning;
                };

                Action doAction1 = () =>
                {
                    if (Module.IsSelectedLoader)
                    {
                        isLoaderPauseTrigger = true;
                    }
                    else
                    {
                        isLoaderPauseTrigger = false;
                    }
                };
                Action abortAction1 = () =>
                {
                };
                bool consumed1;

                consumed1 = Module.CommandManager().ProcessIfRequested<IGPLotOpPause>(
              Module,
              conditionFunc1,
              doAction1,
              abortAction1);

                Func<bool> resumeConditionFunc1 = () =>
                {
                    bool canRunning = true;
                    Module.ExternalLotOPResume();
                    return canRunning;
                };

                Action resumeDoAction1 = () =>
                {
                };
                Action resumeAbortAction1 = () =>
                {
                };
                bool resumConsumed1;

                resumConsumed1 = Module.CommandManager().ProcessIfRequested<IGPLotOpResume>(
              Module,
              resumeConditionFunc1,
              resumeDoAction1,
              resumeAbortAction1);

                if (isLoaderPauseTrigger)
                {
                    //Module.WaitCancelDialogService().ShowDialog("Wait").Wait();
                    // Module.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait").Wait();

                    StateTransition(new External_Paused(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new External_Idle(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is IGPLotOpPause;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }
    }

    public class External_ReadyPausedToRunning : LoaderSupervisorExternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.RUNNING;

        public External_ReadyPausedToRunning(LoaderSupervisor module) : base(module) { }

        public override void Execute()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum ClearRequestData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpStart;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSING;
        }

    }

    public class External_Pausing : LoaderSupervisorExternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.SUSPENDED;

        public External_Pausing(LoaderSupervisor module) : base(module) { }

        public override void Execute() { /*No WORKS*/ }
        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new External_Idle(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override EventCodeEnum AwakeProcessModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        //public override EventCodeEnum Pause()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    Module.PreStateObj = this;
        //    StateTransition(new PAUSED(Module));
        //    return retVal;
        //}

        public override EventCodeEnum AbortRequest()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum ClearRequestData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpStart;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSING;
        }
    }

    public class External_Paused : LoaderSupervisorExternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.PAUSED;
        private DateTime RunningTime = DateTime.Now;
        private bool occuralarm = false;
        public External_Paused(LoaderSupervisor module) : base(module)
        {
            // [YMTC] 모든 ALID를 보고 해야함. 일단 모든 Error에 대해서 Stage Error로 무조건 보고한다.
            Module.NotifyManager().Notify(EventCodeEnum.LOADER_ERROR_OCCUR);

            LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.PAUSE, $"Loader Lot Paused", isLoaderMap: true);
            if (this.Module.GetGPLoader() != null)
            {
                this.Module.GetGPLoader().LoaderLampSetState(ModuleState);
                if (Module.DynamicMode == DynamicModeEnum.DYNAMIC
                    || Module.GetFoupShiftMode() == FoupShiftModeEnum.SHIFT)
                {
                    this.Module.GetGPLoader().IsLoaderBusy = true;
                }
                else
                {
                    this.Module.GetGPLoader().IsLoaderBusy = false;
                }
            }
            //Module.ModuleState.State = ModuleStateEnum.PAUSED;

        }

        public override void Execute()
        {
            try
            {
                Func<bool> resumeConditionFunc = () =>
                {
                    bool canRunning = true;
                    Module.ExternalLotOPResume();
                    return canRunning;
                };

                Action resumeDoAction = () =>
                {
                    if (Module.IsSelectedLoader)
                    {
                    //if (Module.IsAbortError)
                    //{
                    //    StateTransition(new External_Aborted(Module));
                    //}
                    //else
                    //{
                    StateTransition(new External_Running(Module, bResume: true));
                    //}
                }
                };
                Action resumeAbortAction = () =>
                {

                //Module.MetroDialogManager().ShowMessageDialog("[LOTSTART]", "FAIL=" + Module.LotStartFailReason, EnumMessageStyle.AffirmativeAndNegative);
            };
                bool resumConsumed;

                resumConsumed = Module.CommandManager().ProcessIfRequested<IGPLotOpResume>(
              Module,
              resumeConditionFunc,
              resumeDoAction,
              resumeAbortAction);



                Func<bool> conditionFunc = () =>
                {
                    bool canRunning = true;
                    Module.ExternalLotOPPause();
                    return canRunning;
                };

                Action doAction = () =>
                {
                };
                Action abortAction = () =>
                {
                };
                bool consumed;

                consumed = Module.CommandManager().ProcessIfRequested<IGPLotOpPause>(
              Module,
              conditionFunc,
              doAction,
              abortAction);

                Func<bool> EndConditionFunc = () =>
                {
                    bool canRunning = true;
                //Module.LotOPEnd();
                return canRunning;
                };

                Action EndDoAction = () =>
                {
                    StateTransition(new External_Aborted(Module));
                };
                Action EndAbortAction = () =>
                {
                };
                bool endConsumed;

                endConsumed = Module.CommandManager().ProcessIfRequested<IGPLotOpEnd>(
              Module,
              EndConditionFunc,
              EndDoAction,
              EndAbortAction);

                #region <remarks> Check Paused Time </remarks>
                if (Module.GetLotPauseTimeoutAlarm() != 0)
                {
                    DateTime curDateTime = DateTime.Now;
                    TimeSpan ts = curDateTime - RunningTime;
                    if (Module.GetLotPauseTimeoutAlarm() <= ts.TotalSeconds)
                    {
                        if (occuralarm == false)
                        {
                            // Occur timeout alarm.
                            occuralarm = true;
                            LoggerManager.Debug($"Occur LOT_PAUSE_TIMEOUT_LOADER. Set Time : {Module.GetLotPauseTimeoutAlarm()}(sec), Cur Time : {ts.TotalSeconds}(sec)");
                            Module.NotifyManager().Notify(EventCodeEnum.LOT_PAUSE_TIMEOUT_LOADER);
                        }
                    }
                }
                #endregion

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum ClearRequestData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new External_Idle(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is IGPLotOpResume ||
                token is IGPLotOpEnd;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }
    }

    public class External_Aborted : LoaderSupervisorExternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.ABORT;

        public External_Aborted(LoaderSupervisor module) : base(module)
        {
            if (this.Module.GetGPLoader() != null)
            {
                this.Module.GetGPLoader().LoaderLampSetState(ModuleState);
                this.Module.GetGPLoader().IsLoaderBusy = true;
            }
            Module.GEMModule().ClearAlarmOnly();
            Module.IsAbortError = true;
        }

        public override void Execute()
        {
            if (Module.Loader.GetLoaderInfo().StateMap.ARMModules.Where(i => i.WaferStatus == EnumSubsStatus.UNKNOWN).Count() > 0)
            {
                Module.Loader.ResonOfError = "There is Arm Unknown Status, please check the status of Arm.";
                Module.MetroDialogManager().ShowMessageDialog("Arm Unknown Error", $"There is Arm Unknown Status, please check the status of Arm.", EnumMessageStyle.Affirmative);
                StateTransition(new External_Paused(Module));
                return;
            }

            var allWafer = Module.Loader.GetLoaderInfo().StateMap.GetTransferObjectAll();
            bool isLotEnd = false;
            bool isLockMode = false;
            bool isAvailableModule = true;
            var loaderMap = Module.UnloadRequestJob(out isLotEnd, out isLockMode, out isAvailableModule);

            if (Module.ForcedLotEndFlag)
            {
                isLotEnd = true;
            }
            if (isAvailableModule == false)
            {
                this.Module.GPLoader.LoaderBuzzer(true);
                var retVal = Module.MetroDialogManager().ShowMessageDialog("LOT END Fail", $"There are ARM or PA modules that cannot be used. Please Check it.", EnumMessageStyle.Affirmative).Result;
                StateTransition(new External_Paused(Module));
                this.Module.MetroDialogManager().CloseWaitCancelDialaog(string.Empty);
                return;
            }
            if (loaderMap == null && isLotEnd&& !isLockMode)
            {
                bool AllLotsWaferConfirmCompleteFlag = true;
                bool FoupUnloadSuccessConfirmFlag = true;
                //foreach (var lotInfo in Module.ActiveLotInfos) ActiveLotInfos 변경되었다고 exception 나서 변경함.
                for (int i =0; i< Module.ActiveLotInfos.Count(); i++)
                {
                    var lotInfo = Module.ActiveLotInfos[i];
                    Module.SetSkipUnprocessedWafer(Module.Loader.GetLoaderInfo().StateMap, lotInfo);
                    if (lotInfo.State == LotStateEnum.Running)
                    {
                        if (Module.ConfirmWaferArrivalInFoup(Module.Loader.GetLoaderInfo().StateMap, lotInfo) != EventCodeEnum.NONE) 
                        {
                            AllLotsWaferConfirmCompleteFlag = false;
                            continue;//다른 lot의 foup은 unload 될 수 있도록 한다.
                        }

                        PIVInfo pivinfo = new PIVInfo(foupnumber: lotInfo.FoupNumber);
                        SemaphoreSlim semaphore = null;
                        if (Module.IsSameDeviceEndToSlot(lotInfo.DeviceName, lotInfo.UsingStageIdxList, isExistUnprocessedWafer: true) == true)
                        {
                            semaphore = new SemaphoreSlim(0);
                            Module.EventManager().RaisingEvent(typeof(SameDeviceEndToSlot).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }

                        var Cassette = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, lotInfo.FoupNumber);
                        if (Cassette.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                        {
                            Module.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].SetLock(false);
                            //Module.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
                            if(lotInfo.FoupUnLoad() != EventCodeEnum.NONE)
                            {
                                // foup unload 시, error 발생
                                FoupUnloadSuccessConfirmFlag = false;
                                continue;
                            }
                        }

                        //==<Cell Check>==
                        Module.NotifyLotEndToCell(lotInfo.FoupNumber, lotInfo.LotID);
                        //================

                        //PIVInfo pivinfo = new PIVInfo(foupnumber: lotInfo.FoupNumber, lotid: Module.ActiveLotInfos[lotInfo.FoupNumber - 1].LotID);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CarrierCanceledEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();

                        Module.DeAllocate(Module.Loader.Foups[lotInfo.FoupNumber - 1]);

                        lotInfo.State = LotStateEnum.Idle;
                        lotInfo.LotPriority = 999;
                        Module.Loader.Foups[lotInfo.FoupNumber - 1].LotState = lotInfo.State;
                        Module.Loader.Foups[lotInfo.FoupNumber - 1].AllocatedCellInfo = "";
                        Module.Loader.Foups[lotInfo.FoupNumber - 1].LotPriority = 0;
                        Module.DeactiveDevice(lotInfo.FoupNumber-1);
                        Module.ExternalLotOPEnd(lotInfo.FoupNumber);
                    }

                }

                Module.IsSuperUser = true;
                Module.ContinueLotFlag = false; //ContinueLotFlag의 Set안에 IsSuperUser가 true일 때만 Set가능하게 되어있음..
                Module.IsSuperUser = false;

                Module.ForcedLotEndFlag = false;
                Module.ClearLotAbortStageInfos();
                if (AllLotsWaferConfirmCompleteFlag == true && FoupUnloadSuccessConfirmFlag == true)
                {
                    StateTransition(new External_Done(Module));
                }
                else
                {
                    this.Module.GPLoader.LoaderBuzzer(true);
                    StateTransition(new External_Paused(Module));
                    this.Module.MetroDialogManager().CloseWaitCancelDialaog(string.Empty);
                }
                return;
            }
            if (loaderMap != null)
            {
                var slicedMap = Module.MapSlicer.Slicing(loaderMap);
                if (slicedMap != null)
                {
                    for (int i = 0; i < slicedMap.Count; i++)
                    {
                        var result = Module.Loader.SetRequest(slicedMap[i]);
                        if (result.IsSucceed == false)
                        {

                            StateTransition(new External_Paused(Module));
                            LoggerManager.Debug($"Loader SetRequest Error. Error Msg: {result.ErrorMessage}");
                            break;

                        }
                        while (true)
                        {
                            if (Module.Loader.ModuleState == ModuleStateEnum.DONE)
                            {
                                Module.Loader.ClearRequestData();
                                break;
                            }
                            else if (Module.Loader.ModuleState == ModuleStateEnum.ERROR)
                            {

                                // Module.MetroDialogManager().ShowMessageDialog("LOADER ERROR", Module.Loader.ResonOfError, ProberInterfaces.Enum.EnumMessageStyle.Affirmative);
                                StateTransition(new External_Error(Module));

                                return;
                            }
                            //delays.DelayFor(100);
                            Thread.Sleep(100);
                        }
                    }
                }
            }
            else
            {
                if (isLockMode)
                {
                    this.Module.GPLoader.LoaderBuzzer(true);
                    var retVal = Module.MetroDialogManager().ShowMessageDialog("Lot End Fail", $"Cell BackSide Door is Opened.", EnumMessageStyle.Affirmative).Result;
                    StateTransition(new External_Paused(Module));
                    this.Module.MetroDialogManager().CloseWaitCancelDialaog(string.Empty);
                }
                else if (isLotEnd == false)
                {
                    var retVal = Module.MetroDialogManager().ShowMessageDialog("Lot End Canceled", $"Cell still has the job! Please use cell end prior to end the LOT.", EnumMessageStyle.Affirmative).Result;
                    StateTransition(new External_Paused(Module));
                    this.Module.MetroDialogManager().CloseWaitCancelDialaog(string.Empty);
                }
            }

        }

        public override EventCodeEnum ClearRequestData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new External_Idle(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpStart;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }
        //public override EventCodeEnum Pause()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    Module.PreStateObj = this;
        //    StateTransition(new PAUSED(Module));
        //    return retVal;
        //}
    }
    public class External_Error : LoaderSupervisorExternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.ERROR;

        public External_Error(LoaderSupervisor module) : base(module)
        {
            if (this.Module.GetGPLoader() != null)
            {
                this.Module.GetGPLoader().LoaderLampSetState(ModuleState);
                this.Module.GetGPLoader().IsLoaderBusy = false;
            }
        }
        bool firstFlag = true;
        public override void Execute()
        {
            if (firstFlag)
            {
                // Module.LotOPPause();
                LoaderRecoveryControlVM.Show(Module.cont, Module.Loader.ResonOfError, Module.Loader.ErrorDetails, Module.Loader.RecoveryBehavior);
                Module.Loader.ResonOfError = "";
                Module.Loader.ErrorDetails = "";
                Module.Loader.RecoveryBehavior = "";
                firstFlag = false;
            }

        }

        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new External_Idle(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }



        public override void SelfRecovery()
        {
            try
            {
                //Module.Sequencer.SelfRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum ClearRequestData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //prev state를 보고 external_pause or external_idle로 보내준다.
                if (Module.ActiveLotInfos.All(x => x.State == LotStateEnum.Idle) == true)
                {
                    StateTransition(new External_Idle(Module));
                }
                else
                {
                    StateTransition(new External_Paused(Module));
                }
                Module.ModuleState.StateTransition(Module.StateObj.ModuleState);
                Module.ModuleState.State = Module.StateObj.ModuleState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpStart;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }
    }

    public class External_Done : LoaderSupervisorExternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.DONE;

        public External_Done(LoaderSupervisor module) : base(module)
        {
            this.Module.GetGPLoader().IsLoaderBusy = false;
            LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.LOADERALLDONE, $"All Done", isLoaderMap: true);
        }

        public override void Execute()
        {
            try
            {
                //Module.WaitCancelDialogService().CloseDialog().Wait();
                Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString()).Wait();

                Module.SelectedLotInfo = null;

                //TOdo ret  none아니면 뭔가를 해야하
                // ret = UploadLog();


                // #Hynix_Merge: 검토 필요, lock 내용 RemoveLotInfo랑 중복 기능 인것 같음. 확인핤것.

                //lock (Module.Prev_ActiveLotInfos)
                //{
                //    if (Module.Prev_ActiveLotInfos != null && Module.Prev_ActiveLotInfos.Count > 0)
                //    {
                //        //Module.Prev_ActiveLotInfos.Clear();
                //        var allWafer = Module.Loader.GetLoaderInfo().StateMap.GetTransferObjectAll();
                //        foreach (var lotInfo in Module.Prev_ActiveLotInfos.Reverse<ActiveLotInfo>())
                //        {
                //            if (null != allWafer.Where(x => x.CST_HashCode == lotInfo.CST_HashCode))
                //            {
                //                continue;
                //            }

                //            Module.Prev_ActiveLotInfos.Remove(lotInfo);
                //        }
                //    }
                //}

                LoggerManager.Debug($"Prev_ActiveLotInfos Clear.");
                Module.Prev_ActiveLotInfos.Clear();

                if (Module.ContinueLotFlag)
                {
                    for (int i = 0; i < Module.ActiveLotInfos.Count; i++)
                    {
                        if (Module.ActiveLotInfos[i].State == LotStateEnum.Done)
                        {
                            Module.ActiveLotInfos[i].State = LotStateEnum.Running;
                            Module.ActiveLotInfos[i].IsFoupEnd = false;
                            var Cassette1 = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, i + 1);

                            if (Cassette1.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                            {
                                Cassette1.SetNoReadScanState();

                                bool scanWaitFlag = false;

                                if (Cassette1.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette1.ScanState == CassetteScanStateEnum.NONE)
                                {
                                    var scanRetVal = Module.Loader.DoScanJob(i + 1);
                                    if (scanRetVal.Result == EventCodeEnum.NONE)
                                    {
                                        scanWaitFlag = true;
                                    }
                                }
                                while (scanWaitFlag)
                                {
                                    if (Cassette1.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette1.ScanState == CassetteScanStateEnum.READ)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(10);
                                }

                                if (scanWaitFlag == false)
                                {
                                    StateTransition(new External_Idle(Module));
                                    Module.ContinueLotFlag = false;
                                    LoggerManager.Debug($"DoScanJob() Fail. ret:{scanWaitFlag} Loader State Transition to Idle State");
                                    return;
                                }
                                Module.ExternalLotOPStart(i + 1, Module.ActiveLotInfos[i].LotID);
                            }
                         }
                    }

                    Module.ActiveLotQueueReset();
                    Module.LotStartTime = DateTime.Now;
                    StateTransition(new External_Running(Module));
                 }
                else
                {
                    for (int i = 0; i < Module.ActiveLotInfos.Count; i++)
                    {
                        if(Module.ActiveLotInfos[i].AssignState != LotAssignStateEnum.CANCEL)
                        {
                            Module.ActiveLotInfos[i].State = LotStateEnum.Idle;
                        }

                        Module.ActiveLotInfos[i].IsFoupEnd = false;
                        Module.ActiveLotInfos[i].LotPriority = 0;

                        if (Module.Loader.Foups.Count > i)
                        {
                            Module.Loader.Foups[i].LotState = Module.ActiveLotInfos[i].State;
                            Module.Loader.Foups[i].AllocatedCellInfo = "";
                        }
                    }

                    foreach(var cell in Module.CellsInfo)
                    {
                        if(cell.StageState==ModuleStateEnum.PAUSED|| cell.StageState == ModuleStateEnum.RUNNING)
                        {
                            LoggerManager.Debug($"LoaderSupervisor Done State. Cell{cell.Index} is {cell.StageState} State. LotOP End Command Execute.");
                            var client = Module.GetClient(cell.Index);
                            if (Module.IsAliveClient(client))
                            {
                                var ret = client.LotOPEnd();

                                if (ret)
                                {
                                    LoggerManager.Debug($"Cell{cell.Index} LotOP End Command Sucess");
                                }
                                else
                                {
                                    LoggerManager.Debug($"Cell{cell.Index} LotOP End Command Fail");
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"ExternalState: [{this.GetModuleState()}] Stage #{cell.Index} is not alive, LotOP End Command Fail");
                            }
                        }
                    }

                    StateTransition(new External_Idle(Module));
                }
            }
            catch (Exception err)
            {
                StateTransition(new External_Error(Module));

                LoggerManager.Exception(err);
            }
        }


        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new External_Idle(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }



        public override void SelfRecovery()
        {
            try
            {
                //Module.Sequencer.SelfRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override EventCodeEnum ClearRequestData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //Module.Sequencer.Clear();

                //if (Module.Move.State != LoaderMoveStateEnum.ERROR)
                //{
                //    StateTransition(new IDLE(Module));
                //    retVal = EventCodeEnum.NONE;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpStart;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }
    }
}
