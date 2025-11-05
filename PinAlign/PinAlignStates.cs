using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PinAlign
{
    using ProberInterfaces;
    using System.Threading;
    using ProberInterfaces.Align;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;
    using ProberErrorCode;
    using ProberInterfaces.State;
    using LogModule;
    using ProberInterfaces.NeedleClean;
    using NotifyEventModule;
    using ProberInterfaces.Event;
    using System.Linq;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using ProberInterfaces.Enum;

    public abstract class PinAlignState : IFactoryModule, IInnerState
    {
        public abstract EventCodeEnum Execute();
        //public abstract AlignStateEnum GetState();
        public abstract ModuleStateEnum GetModuleState();
        public abstract PinAlignInnerStateEnum GetState();

        public abstract bool CanExecute(IProbeCommandToken token);
        //public abstract EventCodeEnum Perform();

        public abstract EventCodeEnum Pause();

        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.UNDEFINED;
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }

        public abstract EventCodeEnum ClearState();

        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }
    }

    public abstract class PinAlignStateBase : PinAlignState
    {
        private PinAligner _Module;

        public PinAligner Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        public PinAlignStateBase(PinAligner module)
        {
            Module = module;
        }

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new PinAlignIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void ChangeRunningState()
        {
            try
            {
                Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                Module.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);

                Module.InnerStateTransition(new PinAlignRunningState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CheckCommands()
        {
            try
            {
                bool consumed = false;

                Func<bool> conditionFunc = () =>
                {
                    if (this.StageSupervisor().Get_TCW_Mode() == TCW_Mode.OFF)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                };

                Action doAction = () => { ChangeRunningState(); };
                Action abortAction = () => { };

                if (this.GetModuleState() == ModuleStateEnum.IDLE || this.GetModuleState() == ModuleStateEnum.PAUSED)
                {
                    consumed = this.CommandManager().ProcessIfRequested<IDoManualPinAlign>(Module, conditionFunc, doAction, abortAction);
                }

                consumed = this.CommandManager().ProcessIfRequested<IDOPINALIGN>(Module, conditionFunc, doAction, abortAction);
                consumed = this.CommandManager().ProcessIfRequested<IDOPinAlignAfterSoaking>(Module, conditionFunc, doAction, abortAction);
                consumed = this.CommandManager().ProcessIfRequested<IDOSamplePinAlignForSoaking>(Module, conditionFunc, doAction, abortAction);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool _UpdateLogUploadListFlag;

        public bool UpdateLogUploadListFlag 
        {
            get { return _UpdateLogUploadListFlag; }
            set { _UpdateLogUploadListFlag = value; }
        }
    }

    public class PinAlignIdleState : PinAlignStateBase
    {
        public PinAlignIdleState(PinAligner module) : base(module)
        { }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.Template != null)
                {
                    List<ISubModule> modules = Module.Template.GetProcessingModule();

                    if (modules != null && Module.Template.SchedulingModule != null)
                    {
                        if (Module.Template.SchedulingModule.IsExecute())
                        {
                            foreach (var subModule in modules)
                            {
                                if (subModule.IsExecute())
                                {
                                    RetVal = subModule.ClearData();

                                    if (RetVal == EventCodeEnum.NONE)
                                    {
                                        ChangeRunningState();
                                    }
                                    else
                                    {
                                        Module.InnerStateTransition(new PinAlignErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, RetVal.ToString(), Module.PinAlignState.GetType().Name)));
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }

                CheckCommands();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override PinAlignInnerStateEnum GetState()
        {
            return PinAlignInnerStateEnum.IDLE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected;

            isInjected = Module.CommandRecvSlot.IsNoCommand() &&
                        (token is IDOPINALIGN ||
                         token is IDOPinAlignAfterSoaking ||
                         token is IDOSamplePinAlignForSoaking ||
                         token is IDoManualPinAlign);

            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = Module.InnerStateTransition(new PinAlignPausedState(Module));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class PinAlignRunningState : PinAlignStateBase
    {
        public PinAlignRunningState(PinAligner module) : base(module)
        {
            UpdateLogUploadListFlag = true;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSING ||
                   this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.ABORT)
                {
                    Module.InnerStateTransition(new PinAlignIdleState(Module));
                }
                else
                {
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    Module.EventManager().RaisingEvent(typeof(PinAlignStartEvent).FullName, new ProbeEventArgs(this, semaphore));
                    semaphore.Wait();

                    retVal = Module.DoPinAlignProcess();

                    ///soaking module쪽에서 pin align을 대기중에 있다면 soaking module에 align 실패정보를 전달하여 정리할 수 있도록 한다.
                    if (EventCodeEnum.NONE != retVal)
                    {
                        bool ManualSoakingWorking = false;

                        if (Module.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE && this.SoakingModule().ManualSoakingStart)
                        {
                            ManualSoakingWorking = true;
                        }

                        bool EnableStatusSoaking = false;
                        bool IsGettingOptionSuccessul = this.SoakingModule().StatusSoakingParamIF.IsEnableStausSoaking(ref EnableStatusSoaking);

                        if ((IsGettingOptionSuccessul && EnableStatusSoaking) || ManualSoakingWorking)
                        {
                            if (this.SoakingModule().ModuleState.GetState() == ModuleStateEnum.SUSPENDED &&
                                (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE) // lot op가 running일때는 lot pause가 되면 soaking module이 pause처리됨, 하지만 Lot가 Idle에서는 처리가 없으므로 별도 abort 처리함.
                                ) //soaking module이 align을 대기하기 위해 suspend 상태일때
                            {
                                LoggerManager.SoakingErrLog($"Soaking Abort. because pin align is failed, Pin align Source : {Module.PinAlignSource}");

                                this.SoakingModule().Idle_SoakingFailed_PinAlign = true; // idle soaking 중 pin align이 실패난 경우 해당 flag를 통해 지속적으로 idle상태에서 soaking을 통한 pin align을 하지 않도록 처리
                                this.SoakingModule().ReasonOfError.AddEventCodeInfo(EventCodeEnum.SOAKING_ERROR_IDLE_PINALIGN, $"Pin Align failed. Pin align Source : {Module.PinAlignSource}", this.GetType().Name);
                                this.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR_IDLE_PINALIGN, "Can not start Idle soaking");
                                this.SoakingModule().Abort();
                            }
                        }
                    }
                    else
                    {
                        this.SoakingModule().Idle_SoakingFailed_PinAlign = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override PinAlignInnerStateEnum GetState()
        {
            return PinAlignInnerStateEnum.ALIGN;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;

            return RetVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    public class PinAlignErrorState : PinAlignStateBase
    {
        public PinAlignErrorState(PinAligner module, EventCodeInfo eventcode) : base(module)
        {
            try
            {
                if (this.GetModuleState() == ModuleStateEnum.ERROR)
                {
                    Module.ReasonOfError.AddEventCodeInfo(eventcode.EventCode, eventcode.Message, eventcode.Caller);
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Current State = {this.GetModuleState()}, Can not add ReasonOfError.");
                }

                if (Module.PreInnerState.GetModuleState() != ModuleStateEnum.RECOVERY)
                {
                    Module.CompleteManualOperation(eventcode.EventCode);
                    this.NotifyManager().Notify(EventCodeEnum.PIN_ALIGN_FAILED);
                    LoggerManager.ActionLog(ModuleLogType.PIN_ALIGN, StateLogType.ERROR, $"{eventcode.EventCode}", this.Module.LoaderController().GetChuckIndex());
                }

                if (UpdateLogUploadListFlag)
                {
                    UpdateLogUploadListFlag = false;
                    this.Module.LoaderController().UpdateLogUploadList(EnumUploadLogType.PIN);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum Execute()
        {
            try
            {
                if (Module.LotOPModule().InnerState.GetModuleState() != ModuleStateEnum.RUNNING)
                {
                    Module.InnerStateTransition(new PinAlignIdleState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override PinAlignInnerStateEnum GetState()
        {
            return PinAlignInnerStateEnum.ERROR;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.InnerStateTransition(new PinAlignIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.InnerStateTransition(new PinAlignIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// PinAlign완료시 오는 상태.
    /// 아래 값에 대한 업데이트 필요함.
    /// S6, F11 \[Event Report Send (ERS)], Device ID =   0, WBit = 1, SystemByte = 001502B4
    //LIST, 3
    //UINT4,      1, <187857>
    //UINT4,      1, <6587> - CEID
    //LIST, 1
    //	LIST, 2
    //		UINT4,      1, <411>
    //		LIST, 6
    //			INT2,       1, <17> - PINALIGN_PLANARITY(1594)
    //			FLOAT8,     1, <-0.010083000000> -PINALIGN_ANGLE(1593)
    //			FLOAT8,     1, <373.000000000000> - PINALIGN_CARDCENTER_X(1587)
    //			FLOAT8,     1, <-435.500000000000> - PINALIGN_CARDCENTER_Y(1588)
    //			FLOAT8,     1, <-6614.000000000000> - PINALIGN_CARDCENTER_Z(1589)
    //			LIST, 4 - PINALIGN_LIST(1586)
    //				LIST, 5 
    //					FLOAT8,     1, <77.000000000000> - Pin#n DUT No
    //					FLOAT8,     1, <-101035.000000000000> - Pin#n Pin Coordinate X
    //					FLOAT8,     1, <-101545.000000000000> - Pin#n Pin Coordinate Y
    //					FLOAT8,     1, <-6604.000000000000> - Pin#n Pin Coordinate Z
    //					FLOAT8,     1, <1110.000000000000> - Pin#n Tip Length : Pin Plate에서 Pin Tip 까지의 높이 차 또는 Key 부터 Pin Tip까지의 높이 차
    /// </summary>
    public class PinAlignDoneState : PinAlignStateBase
    {
        public PinAlignDoneState(PinAligner module) : base(module)
        {
            Module.CompleteManualOperation(EventCodeEnum.NONE);

            IProbeCardDevObject cardInfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef;

            try
            {
                List<List<double>> pinalignrst = new List<List<double>>();
                List<IPinData> pinlist = new List<IPinData>();
                var dutlist = cardInfo.DutList;

                foreach (var dut in dutlist)
                {
                    pinlist.AddRange(dut.PinList);
                }

                var sortedpinlist = pinlist.OrderBy(p => p.PinNum.Value);

                foreach (var item in sortedpinlist)
                {
                    var pininfo = new List<double>()
                                    { item.DutNumber.Value,
                                      item.AbsPos.X.Value, item.AbsPos.Y.Value, item.AbsPos.Z.Value};

                    if (cardInfo.ProbeCardType.Value == PROBECARD_TYPE.MEMS_Dual_AlignKey)
                    {
                        var alignkeyinfo = item?.PinSearchParam?.AlignKeyHigh ?? null;
                        if (alignkeyinfo != null && alignkeyinfo.Count > 0)
                        {
                            pininfo.Add(alignkeyinfo.FirstOrDefault().AlignKeyPos?.Z.Value ?? 0);
                        }
                        else
                        {
                            LoggerManager.Debug($"AlignResult Update Invalid. PinNum:{item.PinNum}");
                            pininfo.Add(0.0);// List개수가 달라지므로 읽는 쪽에서 포멧 문제 생길수 있어 0으로 넣어줌.
                        }
                    }
                    else
                    {
                        pininfo.Add(item.AbsPos.Z.Value);
                    }

                    pinalignrst.Add(pininfo);
                }

                //Pinalign 시작할때 PinHighAlignModule.PinAlign()에서 EachPinResultes Clear하는 컨셉때문에 EachPinResultes에서 가지고오지 못하고 위코드와 같이 카드정보에서 직접 가지고옴.
                PIVInfo pIV = new PIVInfo();

                if (pinlist.Any())
                {
                    // Pin Min Max 편차
                    pIV.PinAlignPlanarity = pinlist.Max(p => p.AbsPos.Z.Value) - pinlist.Min(p => p.AbsPos.Z.Value); 
                }

                pIV.PinAlignAngle = cardInfo.DutAngle;
                pIV.PinAlignCardCenterX = cardInfo.PinCenX;
                pIV.PinAlignCardCenterY = cardInfo.PinCenY;
                pIV.PinAlignCardCenterZ = cardInfo.PinHeight;
                pIV.PinAlignResults = pinalignrst;

                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Module.EventManager().RaisingEvent(typeof(PinAlignEndEvent).FullName, new ProbeEventArgs(this, semaphore, pIV));
                semaphore.Wait();

                LoggerManager.ActionLog(ModuleLogType.PIN_ALIGN, StateLogType.DONE,
                   $"Pin Center X: {cardInfo.PinCenX:0.000}, " +
                   $"Pin Center Y: {cardInfo.PinCenY:0.000}, " +
                   $"Dut Angle: {cardInfo.DutAngle:0.000000}" +
                   $"Optimized Angle: {cardInfo.DiffAngle:0.000000}",
                   this.Module.LoaderController().GetChuckIndex());

                this.Module.LoaderController().UpdateLogUploadList(EnumUploadLogType.PIN);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override EventCodeEnum Execute()
        {
            Module.InnerStateTransition(new PinAlignIdleState(Module));

            bool consumed = false;

            if (this.PinAligner().CommandRecvSlot.IsRequested<IDOPINALIGN>())
            {
                Func<bool> conditionFunc = () =>
                {
                    return true;
                };

                Action doAction = () =>
                {

                    Module.InnerStateTransition(new PinAlignRunningState(Module));
                };

                Action abortAction = () => { };

                consumed = this.CommandManager().ProcessIfRequested<IDOPINALIGN>(
                    this.PinAligner(),
                    conditionFunc,
                    doAction,
                    abortAction);
            }

            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }

        public override PinAlignInnerStateEnum GetState()
        {
            return PinAlignInnerStateEnum.DONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;

            isInjected = Module.CommandRecvSlot.IsNoCommand() && token is IDOPINALIGN;

            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(new PinAlignPausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
    }


    public class PinAlignPausedState : PinAlignStateBase
    {
        public PinAlignPausedState(PinAligner module) : base(module)
        {
            if (UpdateLogUploadListFlag)
            {
                UpdateLogUploadListFlag = false;
                this.Module.LoaderController().UpdateLogUploadList(EnumUploadLogType.PIN);
            }
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                CheckCommands();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override PinAlignInnerStateEnum GetState()
        {
            return PinAlignInnerStateEnum.PAUSED;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected;

            isInjected = Module.CommandRecvSlot.IsNoCommand() && token is IDoManualPinAlign;

            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.InnerStateTransition(Module.PreInnerState);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.InnerStateTransition(new PinAlignIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    public class PinAlignRecoveryState : PinAlignStateBase
    {
        public PinAlignRecoveryState(PinAligner module, EventCodeInfo eventcode) : base(module)
        {
            try
            {
                Module.CompleteManualOperation(eventcode.EventCode);
                this.NotifyManager().Notify(EventCodeEnum.PIN_ALIGN_FAILED);
                LoggerManager.ActionLog(ModuleLogType.PIN_ALIGN, StateLogType.ERROR, $"{eventcode.EventCode}", this.Module.LoaderController().GetChuckIndex());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                {
                    //var task = Task.Run(() =>
                    //{
                    List<ISubModule> modules = Module.Template.GetProcessingModule();

                    for (int index = 0; index < modules.Count; index++)
                    {
                        ISubModule module = modules[index];

                        if (module is IRecovery)
                        {
                            if (module.GetState() == SubModuleStateEnum.RECOVERY)
                            {
                                retVal = module.Recovery();
                            }
                        }
                    }
                    //});

                    this.PinAligner().IsRecoveryStarted = true;

                    Module.InnerStateTransition(new PinAlignIdleState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                retVal = EventCodeEnum.EXCEPTION;
                Module.InnerStateTransition(new PinAlignErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, retVal.ToString(), Module.PinAlignState.GetType().Name)));
            }

            // TODO : Check return value
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RECOVERY;
        }

        public override PinAlignInnerStateEnum GetState()
        {
            return PinAlignInnerStateEnum.RECOVERY;
        }
        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                Module.InnerStateTransition(new PinAlignIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
}
