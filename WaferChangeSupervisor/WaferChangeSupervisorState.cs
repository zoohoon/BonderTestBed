namespace WaferChangeSupervisor.WaferChangeState
{
    using LogModule;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using System.Runtime.CompilerServices;
    using ProberInterfaces.State;
    using ProberErrorCode;
    using ProberInterfaces.Command.Internal;
    using System.Linq.Expressions;
    using LoaderBase;
    using ProberInterfaces.Foup;
    using LoaderParameters;
    using ProberInterfaces.Enum;
    using System.Threading;
    using NotifyEventModule;
    using ProberInterfaces.Event;

    public abstract class WaferChangeSupervisorStateBase : IInnerState
    {
        public WaferChangeSupervisorStateBase(WaferChangeSupervisor module)
        {
            try
            {
                this.Module = module;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public WaferChangeSupervisor Module { get; set; }
        public abstract ModuleStateEnum GetModuleState();
        public abstract WaferChangeStateEnum GetState();
        public abstract bool CanExecute(IProbeCommandToken token);
        protected void RaiseInvalidState([CallerMemberName] string memberName = "")
        {
            try
            {
                LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public abstract EventCodeEnum Execute();

        public EventCodeEnum Abort()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum Pause()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum End()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ClearState()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum Resume()
        {
            throw new NotImplementedException();
        }

        public void WaferChangeResult()
        {
            try
            {
                int autofeedresult = -1;
                if (Module.WaferChangeAutofeed.AutoFeedActions.FirstOrDefault(x => x.Result == AutoFeedResult.FAILURE || x.Result == AutoFeedResult.SKIPPED) != null)
                {
                    autofeedresult = 1;
                }
                else
                {
                    autofeedresult = 0;
                }

                PIVInfo pivinfo = new PIVInfo()
                {
                    WaferAutofeedResult = autofeedresult
                };

                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Module.EventManager().RaisingEvent(typeof(WaferAutofeedResultEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                semaphore.Wait();

                if (Module.Master.CommandRecvDoneSlot.Token?.Sender is IWaferChangeSupervisor)
                {
                    Module.Master.CommandRecvDoneSlot.ClearToken();
                }
                Module.CommandSendSlot.ClearToken();

                Module.CloseWaferChangeWaitDialog();
                Module.WaferChangeAutofeed.Clear();

                Module.Master.HostInitiatedWaferChangeInProgress = false;
                Module.InnerStateTransition(new WAFERCHANGE_IDLE(Module));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
    public class WAFERCHANGE_IDLE : WaferChangeSupervisorStateBase
    {
        public WAFERCHANGE_IDLE(WaferChangeSupervisor module) : base(module)
        {
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.IDLE;
        public override WaferChangeStateEnum GetState() => WaferChangeStateEnum.IDLE;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Func<bool> conditionFuc = () =>
                {
                    return true;
                };

                Action startAction = () =>
                {
                    Module.InnerStateTransition(new WAFERCHANGE_RUNNING(Module));
                };

                Action abortAction = () => { LoggerManager.Debug($"[WaferChangeSupervisor].IDLE(): Command Is Not Excuted."); };

                Module.CommandManager().ProcessIfRequested<IStartWaferChangeSequence>(
                    Module,
                    conditionFuc,
                    startAction,
                    abortAction);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            return token is IStartWaferChangeSequence;
        }
    }
    public class WAFERCHANGE_RUNNING : WaferChangeSupervisorStateBase
    {
        private bool IsShowingWait = false;
        private bool WriteLog = false;

        public WAFERCHANGE_RUNNING(WaferChangeSupervisor module) : base(module)
        {
            // 커맨드를 받았을 당시의 Master의 상태를 기록
            Module.PrevLoaderState = Module.Master.ModuleState.GetState();
            WriteLog = false;
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override WaferChangeStateEnum GetState() => WaferChangeStateEnum.RUNNING;
        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsShowingWait == false)
                {
                    Module.ShowWaferChangeWaitDialog();

                    IsShowingWait = true;
                }

                // Master의 상태가 Running인 경우, Pause 시킨다.
                if (Module.Master.ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    bool needSetPauseCommand = false;

                    //아직 recvdone slot까지 안옸어.
                    if (Module.CommandSendSlot.Token is IGPLotOpPause == true)
                    {
                        if (Module.CommandSendSlot.Token.SubjectInfo == Module.Master.CommandRecvSlot.Token.SubjectInfo
                        || Module.CommandSendSlot.Token.SubjectInfo == Module.Master.CommandRecvProcSlot.Token.SubjectInfo
                        || Module.CommandSendSlot.Token.SubjectInfo == Module.Master.CommandRecvDoneSlot.Token.SubjectInfo)
                        {
                            ////현재 지금 cmd가 set이 된 상황이기 때문에 중복으로 보내지 않기 위함.
                        }
                        else
                        {
                            needSetPauseCommand = true;
                        }
                    }
                    else
                    {
                        needSetPauseCommand = true;
                    }

                    if(needSetPauseCommand)
                    {
                        foreach (var cell in Module.Master.CellsInfo)
                        {
                            cell.StageInfo.IsChecked = false;
                        }

                        Module.Master.SetMapSlicerLotPause(true);
                        Module.CommandManager().SetCommand<IGPLotOpPause>(Module);
                    }
                }
                else if (Module.Master.ModuleState.GetState() == ModuleStateEnum.IDLE ||
                         Module.Master.ModuleState.GetState() == ModuleStateEnum.PAUSED)
                {
                    // Transfer를 할 수 있는 상태 (IDLE, PAUSED)

                    bool canExcuteMap = false;
                    var remainJob = Module.Master.Loader.LoaderJobViewList.Count(x => x.JobDone == false);

                    if (remainJob == 0)
                    {
                        canExcuteMap = true;
                    }

                    if (canExcuteMap)
                    {
                        if (IsShowingWait)
                        {
                            Module.MetroDialogManager().SetDataWaitCancelDialog(message: "Wait for Wafer changing", hashcoe: Module.GetHashCode().ToString(), restarttimer: true);
                        }
                        //ArmtoPA에서 WaferChange, Maual 분리하기 위함
                        Module.Master.HostInitiatedWaferChangeInProgress = true;

                        foreach (var action in Module.WaferChangeAutofeed.AutoFeedActions)
                        {
                            //validation 을 추가 한다.
                            EventCodeEnum valid_result = WaferChangeActionValidation(action);

                            if (valid_result == EventCodeEnum.NONE)
                            {
                                // action전에 넣는다.

                                EventCodeEnum dataInit_result = Module.Master.SetPolishWaferInfoByLoadModule(action.Allocate_Loc1 as IWaferOwnable, action.Allocate_Loc2 as IWaferOwnable);
                                if (dataInit_result == EventCodeEnum.NONE)
                                {
                                    var isSuccess = Module.Master.TransferWaferObjectFunc(Module.WaferChangeAutofeed.PolishWaferOCRMode, action.Allocate_Loc1 as IWaferOwnable, action.Allocate_Loc2 as IWaferOwnable, action.Allocate_WaferID);

                                    if (isSuccess == AutoFeedResult.SUCCESS)
                                    {
                                        action.Result = isSuccess;
                                        LoggerManager.Debug($"WaferChangeSupervisorState.Execute() TransferWaferObjectFunc_action.Result : {action.Result} ");
                                    }
                                    else if (isSuccess == AutoFeedResult.SKIPPED)
                                    {
                                        // OCR Fail로 인해 웨이퍼가 SLOT으로 되돌아 간 경우..
                                        // Result 가 Success가 아니라 최종 result는 1(실패)
                                        // 하지만 다음 웨이퍼는 진행해야 함.
                                        action.Result = isSuccess;
                                        LoggerManager.Debug($"WaferChangeSupervisorState.Execute() TransferWaferObjectFunc_action.Result : {action.Result} ");
                                    }
                                    else if (isSuccess == AutoFeedResult.FAILURE)
                                    {
                                        // TODO : 이후 Action을 진행할 수 있는가?
                                        // Abort 처리를 해야 하는가?
                                        action.Result = isSuccess;
                                        LoggerManager.Debug($"WaferChangeSupervisorState.Execute() TransferWaferObjectFunc_action.Result : {action.Result} ");
                                        retval = EventCodeEnum.WAFER_CHANGE_AUTOFEED_LOADER_HANDLING_ERROR;
                                        Module.NotifyManager().Notify(retval);
                                        break;
                                    }
                                    else
                                    {
                                        // Undefined??
                                        action.Result = isSuccess;
                                        LoggerManager.Debug($"WaferChangeSupervisorState.Execute() TransferWaferObjectFunc_action.Result : {action.Result} ");
                                        retval = EventCodeEnum.WAFER_CHANGE_AUTOFEED_LOADER_HANDLING_ERROR;
                                        Module.NotifyManager().Notify(retval);
                                        break;
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name} :  Wafer Change Action Polish Data Initialize Failure Reason = {dataInit_result}");
                                    action.Result = AutoFeedResult.FAILURE;
                                }



                                ReportSingleWaferChangeResult(action);// 현재 수행한 동작에 대해서만 리포트한다.                

                            }
                            else
                            {
                                LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name} :  Wafer Change Action Validation Failure Reason = {valid_result}");
                                action.Result = AutoFeedResult.SKIPPED;
                                retval = EventCodeEnum.WAFER_CHANGE_AUTOFEED_VALIDATION_FAULURE;
                                Module.NotifyManager().Notify(retval);
                            }
                        }

                        if (Module.WaferChangeAutofeed.AutoFeedActions.FirstOrDefault(wa => wa.Result == AutoFeedResult.FAILURE) != null)
                        {
                            Module.InnerStateTransition(new WAFERCHANGE_ERROR(Module));
                        }
                        else
                        {
                            Module.InnerStateTransition(new WAFERCHANGE_CLEAR(Module));
                        }
                    }
                    else
                    {
                        // LoaderJobViewList의 데이터 중, JobDone이 false인게 사라지지 않는 경우, 무한 루프 가능성이 있을 수 있지만 LoaderJobViewList Clear 로직을 보강 해야 하는 문제로 본다.
                        // 로그만 찍도록 한다.
                        if (WriteLog == false)
                        {
                            WriteLog = true;
                            LoggerManager.Debug(($"{Module.GetType().Name}.{GetType().Name} : LoaderJobViewList remain job. canExecuteMap Flag : {canExcuteMap}"));
                        }
                    }
                }
                else
                {
                    //Gem Validation에서는 Idle, Running이였는데 Running 되고 난 후에 Abort, Error 가 발생할 수 있다고 판단 하여 Error State 처리 함.
                    LoggerManager.Debug(($"{Module.GetType().Name}.{GetType().Name} : Loader Module State is {Module.Master.ModuleState.GetState()} not valid"));
                    Module.InnerStateTransition(new WAFERCHANGE_ERROR(Module));
                }
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.EXCEPTION;
                Module.InnerStateTransition(new WAFERCHANGE_ERROR(Module));
                LoggerManager.Exception(err);
            }

            return retval;

            void ReportSingleWaferChangeResult(AutoFeedAction currentAction)
            {
                try
                {
                    int autofeedresult = -1;
                    if (currentAction.Result == AutoFeedResult.FAILURE || currentAction.Result == AutoFeedResult.SKIPPED)
                    {
                        autofeedresult = 1;
                    }
                    else
                    {
                        autofeedresult = 0;
                    }

                    var loc1_waferid = Module.Master.Loader.ModuleManager.FindModule<IWaferOwnable>(currentAction.Allocate_Loc1.ID)?.Holder?.TransferObject?.OCR?.Value ?? "";
                    var loc2_waferid = Module.Master.Loader.ModuleManager.FindModule<IWaferOwnable>(currentAction.Allocate_Loc2.ID)?.Holder?.TransferObject?.OCR?.Value ?? "";

                    PIVInfo pivinfo = new PIVInfo()
                    {
                        WaferAutofeedResult = autofeedresult,

                        // 명령 내용 
                        WaferChange_Location1_LoadPortId = currentAction.GetLoc1LoadPortId(),
                        WaferChange_Location1_AtomId = currentAction.GetLoc1AtomId(),//TODO:값 의도된 대로 나오는 지확인 필요
                        WaferChange_Location2_LoadPortId = currentAction.GetLoc2LoadPortId(),
                        WaferChange_Location2_AtomId = currentAction.GetLoc2AtomId(),

                        // 현재 위치
                        WaferChange_Location1_WaferId = loc1_waferid,
                        WaferChange_Location2_WaferId = loc2_waferid


                    };

                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    Module.EventManager().RaisingEvent(typeof(SingleWaferAutofeedResultEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }
        private EventCodeEnum WaferChangeActionValidation(AutoFeedAction active_info)
        {
            try
            {
                LoaderInfo loadermap = Module.Master.Loader.GetLoaderInfo();

                #region [1] Arm 둘다 비어 있는지 확인
                bool use_arm = loadermap.StateMap.ARMModules.All(i => i.Substrate == null && i.Enable == true && i.WaferStatus == EnumSubsStatus.NOT_EXIST);
                if (use_arm == false)
                {
                    return EventCodeEnum.WAFER_CHANGE_AUTOFEED_ERROR_BOTH_ARMS_UNAVAILABLE;
                }

                #endregion

                #region [2] 사용가능한 PA 있는지 확인
                var abort_pa = Module.Master.Loader.PAManager.PAModules.FirstOrDefault(x => x.State.PAAlignAbort == false); //Abort 가 아닌 PA가 있는가
                var use_pamodule = loadermap.StateMap.PreAlignModules.FirstOrDefault(i => i.Substrate == null && i.Enable == true && i.WaferStatus == EnumSubsStatus.NOT_EXIST); //사용가능한데 비어 있는가
                if (abort_pa == null || use_pamodule == null)
                {
                    return EventCodeEnum.WAFER_CHANGE_AUTOFEED_ERROR_ALL_PA_UNAVAILABLE;
                }
                #endregion

                #region [3] LOC1, LOC2가 같은 모듈인 경우 ex) (s,s) (f,f)
                LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name} :  Assign Module Type Location 1 : {active_info.Allocate_Loc1.ModuleType.ToString()}, Location 2 : {active_info.Allocate_Loc2.ModuleType.ToString()} ");
                if (active_info.Allocate_Loc1.ModuleType == active_info.Allocate_Loc2.ModuleType)
                {
                    return EventCodeEnum.WAFER_CHANGE_AUTOFEED_ERROR_SAME_MODULE_DEFINITION;
                }
                #endregion

                #region [4] LOC1, LOC2에 둘다 Wafer가 없는 경우
                var loc1_status = loadermap.StateMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.Equals(active_info.Allocate_Loc1.ID));
                var loc2_status = loadermap.StateMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.Equals(active_info.Allocate_Loc2.ID));
                if (loc1_status?.WaferStatus == EnumSubsStatus.NOT_EXIST && loc2_status?.WaferStatus == EnumSubsStatus.NOT_EXIST)
                {
                    return EventCodeEnum.WAFER_CHANGE_AUTOFEED_LOCATION_WAFER_STATUS_NOT_EXIST;
                }
                #endregion

                #region [5] CST Scan 확인 
                foreach (int lpNum in new[] { active_info.LPNum1, active_info.LPNum2 })
                {
                    //lpNum == 0 인 경우는 Fixed Tray
                    if (lpNum >= 1)
                    {
                        var cassette = Module.Master.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, lpNum);
                        if (cassette.ScanState != CassetteScanStateEnum.READ)
                        {
                            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name} :  Foup #{lpNum}, Scan State : {cassette.ScanState}");
                            return EventCodeEnum.WAFER_CHANGE_AUTOFEED_LOADPORT_SCAN_STATE_FAILURE;
                        }
                    }
                }
                #endregion

                #region [6] Load, Unload할 SLOT이 Lot에 할당되어 있는가?
                foreach (int lpNum in new[] { active_info.LPNum1, active_info.LPNum2 })
                {
                    //lpNum == 0 인 경우는 Fixed Tray
                    if (lpNum >= 1)
                    {
                        bool isnotidlestate = Module.Master.ActiveLotInfos[lpNum - 1].State != LotStateEnum.Idle;
                        if (isnotidlestate == true)
                        {
                            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name} :  Foup #{lpNum}, The wafer assigned to the lot is ready for wafer change operation.");
                            return EventCodeEnum.WAFER_CHANGE_AUTOFEED_WAFER_ASSIGNED_TO_LOT;
                        }
                    }
                }
                #endregion

                DeviceManagerParameter DMParam = Module.DeviceManager()?.DeviceManagerParamerer_IParam as DeviceManagerParameter;

                #region [7] PolishWaferSourceParameters 가 있는지 확인할 것.
                if (DMParam.PolishWaferSourceParameters == null || DMParam.PolishWaferSourceParameters.Count <= 0)
                {
                    LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name} :  Param Path = ..\\Loader\\GPDeviceInfos.json , PolishWaferSourceParameters");
                    return EventCodeEnum.PARAM_ERROR;
                }
                #endregion

                #region [8] Fixed Tray의 상태가 Can Use Buffer 사용 하거나 Disable 되어 있는지 확인 & Polish Wafer Assign이 안되어 있는지 확인
                foreach (var allocateLoc in new[] { active_info.Allocate_Loc1, active_info.Allocate_Loc2 })
                {
                    if (allocateLoc is IFixedTrayModule fixedModule)
                    {
                        if (fixedModule.CanUseBuffer || fixedModule.Enable == false)
                        {
                            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name} :  Fixed #{allocateLoc.ID.Index}, Can Use Buffer : {fixedModule.CanUseBuffer} Enable : {fixedModule.Enable}");
                            return EventCodeEnum.WAFER_CHANGE_AUTOFEED_FIXEDTRAY_NOT_AVAILABLE;
                        }
                        else
                        {
                            var assingfixed = DMParam.DeviceMappingInfos.Where(x => x.WaferSupplyInfo.ModuleType == ModuleTypeEnum.FIXEDTRAY && x.WaferSupplyInfo.ID == allocateLoc.ID);
                            var assingpw = assingfixed.FirstOrDefault(y => y.DeviceInfo.PolishWaferInfo != null);
                            if (assingpw == null)
                            {
                                return EventCodeEnum.WAFER_CHANGE_AUTOFEED_FIXEDTRAY_NOT_ASSIGN_POLISH_TYPE;
                            }
                        }
                    }
                }
                #endregion

                List<TransferObject> allwafer = loadermap.StateMap.GetTransferObjectAll();

                #region [9] Exchange 동작 전인데 LOC1, LOC2 Origin을 가진 다른 Wafer가 Map상에 있는 경우 (Origin이 중복이 되는 TO가 있는가?)
                int loc1_duplication = CountWaferDuplicates(allwafer, active_info.Allocate_Loc1);
                if (loc1_duplication > 1)
                {
                    LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name} :  LOC1 Module Origin Invalid.");
                    return EventCodeEnum.WAFER_CHANGE_AUTOFEED_WAFER_DUPLICATION_FAILURE;
                }

                int loc2_duplication = CountWaferDuplicates(allwafer, active_info.Allocate_Loc2);
                if (loc2_duplication > 1)
                {
                    LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name} :  LOC2 Module Origin Invalid.");
                    return EventCodeEnum.WAFER_CHANGE_AUTOFEED_WAFER_DUPLICATION_FAILURE;
                }
                #endregion

                #region [10] Exchange 동작 전인데 LOC1, LOC2 Origin을 가진 Wafer가 Origin = Curr 다른 경우 (집 나간 경우가 있는가?)
                foreach (var loc in new[] { active_info.Allocate_Loc1, active_info.Allocate_Loc2 })
                {
                    var wafer = allwafer.FirstOrDefault(w => w.OriginHolder.ModuleType == loc.ModuleType && w.OriginHolder.Index == loc.ID.Index);
                    if (wafer != null && wafer.OriginHolder != wafer.CurrHolder)
                    {
                        LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name} : The wafer assigned to {loc.GetType().Name} is in another {wafer.CurrHolder} module.");
                        return EventCodeEnum.WAFER_CHANGE_AUTOFEED_WAFER_ANOTHER_MODULE;
                    }
                }
                #endregion

                return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.EXCEPTION;
            }

            int CountWaferDuplicates(List<TransferObject> allwafer, IAttachedModule activeLoc)
            {
                return allwafer.Count(w => w.OriginHolder.ModuleType == activeLoc.ModuleType && w.OriginHolder.Index == activeLoc.ID.Index);
            }
        }
    }

    public class WAFERCHANGE_CLEAR : WaferChangeSupervisorStateBase
    {
        private bool IsUnloaded { get; set; }
        public WAFERCHANGE_CLEAR(WaferChangeSupervisor module) : base(module)
        {
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override WaferChangeStateEnum GetState() => WaferChangeStateEnum.CLEAR;
        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                var loaderstate = Module.Master.ModuleState.GetState();
                if (loaderstate == ModuleStateEnum.ABORT || loaderstate == ModuleStateEnum.ERROR)
                {
                    //예외 상황 
                    //resume X , foup unload X
                    Module.InnerStateTransition(new WAFERCHANGE_ERROR(Module));
                    return EventCodeEnum.NONE;
                }

                if (IsUnloaded == false)
                {
                    IsUnloaded = Module.CSTAutoUnloadAfterWaferChange();
                }

                if (loaderstate == ModuleStateEnum.PAUSED && Module.PrevLoaderState == ModuleStateEnum.RUNNING)
                {
                    //현재 상태가 Pause인 경우 Resume을 보낸다.
                    // Case3의 경우
                    if (Module.CommandSendSlot.Token is IGPLotOpResume == true)
                    {
                        if (Module.CommandSendSlot.Token.SubjectInfo == Module.Master.CommandRecvSlot.Token.SubjectInfo
                        || Module.CommandSendSlot.Token.SubjectInfo == Module.Master.CommandRecvProcSlot.Token.SubjectInfo
                        || Module.CommandSendSlot.Token.SubjectInfo == Module.Master.CommandRecvDoneSlot.Token.SubjectInfo)
                        {
                            //현재 지금 cmd가 set이 된 상황이기 때문에 중복으로 보내지 않기 위함.
                        }
                        else
                        {   
                            Module.Master.SetMapSlicerLotPause(false);
                            Module.CommandManager().SetCommand<IGPLotOpResume>(Module);
                        }
                    }
                    else
                    {
                        Module.Master.SetMapSlicerLotPause(false);
                        Module.CommandManager().SetCommand<IGPLotOpResume>(Module);
                    }
                }
                else
                {
                    //다른 Module, Cmd에 의해서 State Running, Idle이 된 경우가 있을 수 있다.
                    WaferChangeResult();
                    Module.InnerStateTransition(new WAFERCHANGE_DONE(Module));
                    retval = EventCodeEnum.NONE;
                }


                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class WAFERCHANGE_DONE : WaferChangeSupervisorStateBase
    {
        public WAFERCHANGE_DONE(WaferChangeSupervisor module) : base(module)
        {
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.DONE;
        public override WaferChangeStateEnum GetState() => WaferChangeStateEnum.DONE;

        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.InnerStateTransition(new WAFERCHANGE_IDLE(Module));

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
    public class WAFERCHANGE_ABORT : WaferChangeSupervisorStateBase
    {
        public WAFERCHANGE_ABORT(WaferChangeSupervisor module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.ABORT;
        public override WaferChangeStateEnum GetState() => WaferChangeStateEnum.ABORT;

        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }
        public override EventCodeEnum Execute()
        {
            // TODO : 
            return EventCodeEnum.NONE;
        }
    }

    //Handling Error가 발생한 경우에 대해서 ERROR STATE로 보낸다.
    public class WAFERCHANGE_ERROR : WaferChangeSupervisorStateBase
    {
        public WAFERCHANGE_ERROR(WaferChangeSupervisor module) : base(module)
        {
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.ERROR;
        public override WaferChangeStateEnum GetState() => WaferChangeStateEnum.ERROR;
        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //(Module.WaferChangeAutofeed.AutoFeedActions.FirstOrDefault(x=>x.Result == AutoFeedResult.FAILURE) != null 상황
                //Foup Unload 하지 않는다. Recovery 가 필요한 상황일 수 있기 때문.
                //Resume 안한다.

                //WaferChange Abort 동작이 필요한 경우 Cmd 형식으로 구현이 필요할 수 있음.

                WaferChangeResult();

                Module.InnerStateTransition(new WAFERCHANGE_IDLE(Module));

                retval = EventCodeEnum.NONE;
                return retval;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

    }
}
