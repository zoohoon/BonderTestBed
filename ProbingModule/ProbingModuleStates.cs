using System;
using System.Linq;
using System.Threading.Tasks;
using ProberInterfaces;
using GPIBModule;
using ProberInterfaces.Param;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberErrorCode;
using System.ComponentModel;
using NotifyEventModule;
using ProberInterfaces.Event;
using SubstrateObjects;
using ProberInterfaces.Temperature;
using LogModule;
using ProberInterfaces.DialogControl;
using ProberInterfaces.State;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

namespace ProbingModule
{
    public abstract class ProbingState : IInnerState
    {
        public abstract bool CanExecute(IProbeCommandToken token);
        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();
        public abstract EnumProbingState GetState();
        public abstract ModuleStateEnum GetModuleState();
        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.NONE;
        }

        #region Action Method
        public abstract EventCodeEnum MoveToNextPinPadMatchedPos();
        public abstract EventCodeEnum ProbingZUP();
        public abstract EventCodeEnum ProbingZDOWN();
        public abstract EventCodeEnum MoveToCenterDiePos();
        #endregion

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
    public abstract class ProbingStateBase : ProbingState, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private Probing _Module;
        public Probing Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        public ProbingStateBase(Probing module)
        {
            Module = module;
        }

        public DateTime ZupTime
        {
            get;
            set;
        }

        public DateTime ZdownTime
        {
            get;
            set;
        }

        public void SetZTime(EnumProbingState state)
        {
            if (state == EnumProbingState.ZUPPERFORM)
            {
                ZupTime = DateTime.Now.ToLocalTime();
                Module.SetZupStartTime();
            }

            else if (state == EnumProbingState.ZDNPERFORM)
            {
                ZdownTime = DateTime.Now.ToLocalTime();
                Module.SetZupStartTime(false);
            }
            else
            {
                // TODO : ???
            }
        }

        protected EventCodeEnum IsCanProbingStart(ref string errorReasonStr)
        {
            EventCodeEnum ret = EventCodeEnum.PROBING_START_ERROR;
            try
            {
                if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    if (Module.SoakingModule().ChillingTimeMngObj.IsShowDebugString())
                    {
                        Trace.WriteLine($"[ShowDebugStr] IsCanProbingStart >> GetRunState():{Module.SequenceEngineManager().GetRunState().ToString()},ModuleStopFlag:{this.Module.LotOPModule().ModuleStopFlag.ToString()}," +
                            $"WaferObjectState:{Module.StageSupervisor().WaferObject.GetState().ToString()}, StatusSoakingOK:{Module.SoakingModule().IsStatusSoakingOk().ToString()}," +
                            $"WaferAlignmentState:{Module.StageSupervisor().WaferObject.GetAlignState()}, " +
                            $"ProbeCardInfoState:{Module.StageSupervisor().ProbeCardInfo.GetAlignState().ToString()}, " +
                            $"isMarkDone:{ Module.StageSupervisor().MarkObject.GetAlignState().ToString()}," +
                            $"GetSoakQueueCount:{Module.SoakingModule().GetSoakQueueCount().ToString()}," +
                            $"GetLotResumeTriggeredFlag:{Module.SoakingModule().GetLotResumeTriggeredFlag().ToString()}");
                    }

                    if (Module.SequenceEngineManager().GetRunState() &&
                        !this.Module.LotOPModule().ModuleStopFlag &&
                        Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.SKIPPED &&
                        Module.StageSupervisor().WaferObject.GetWaferType() != EnumWaferType.POLISH &&
                        Module.EnvModule().IsConditionSatisfied() == EventCodeEnum.NONE)
                    {
                        ret = Module.CheckOverdriveRangeProcessing(ref errorReasonStr);

                        if (ret == EventCodeEnum.PROBING_Z_LIMIT_ERROR)
                        {
                            return ret;
                        }

                        bool isMarkDone = false;
                        isMarkDone = Module.StageSupervisor().MarkObject.GetAlignState() == AlignStateEnum.DONE;
                        if (!isMarkDone)
                        {
                            isMarkDone = Module.StageSupervisor().WaferAligner().ForcedDone == EnumModuleForcedState.ForcedDone;
                            //ForceDone상태에선 마크 얼라인을 안하기때문에 마크를 대신볼수 있는 걸 추가.
                        }

                        bool IsStatusSoakingGood = true;
                        IsStatusSoakingGood = Module.SoakingModule().IsStatusSoakingOk();

                        if (
                         (Module.StageSupervisor().WaferObject.GetAlignState() == AlignStateEnum.DONE) &&
                         (Module.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE) &&
                         Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROCESSED &&
                         isMarkDone &&
                         Module.SoakingModule().GetSoakQueueCount() == 0 && Module.SoakingModule().GetLotResumeTriggeredFlag() == false &&
                         IsStatusSoakingGood
                         && Module.LoaderController().IsCancel == false)
                        {
                            if (IsStatusSoakingGood)
                            {
                                //UI화면 갱신이 안되어있을 수도 있으니 probing에 진입하면 UI 갱신 처리를 진행
                                Module.SoakingModule().Clear_SoakingInfoTxt();
                            }

                            PinPadMatchModule.PinPadMatchModule pinPadMatchModule = new PinPadMatchModule.PinPadMatchModule();

                            ret = pinPadMatchModule.DoPinPadMatch();

                            //if(Module.StageSupervisor().ProbeCardInfo.GetPinPadAlignState() != AlignStateEnum.DONE)

                            if (ret == EventCodeEnum.NONE)
                            {
                                Module.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.DONE);

                                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Pin_To_Pad_Alignment_OK);
                                ret = EventCodeEnum.NONE;
                            }
                            else
                            {

                                if (Module.WaferAligner().ForcedDone == EnumModuleForcedState.ForcedDone ||
                                    Module.PinAligner().ForcedDone == EnumModuleForcedState.ForcedDone)
                                {
                                    ret = EventCodeEnum.NONE;
                                }
                                else
                                {
                                    Module.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);

                                    //PTPA Fail
                                    LoggerManager.Error($"Error occurred while probing condition which PTPA. return code:{ret.ToString()}");
                                }
                            }
                            //isCanStart = true;
                        }
                        else
                        {
                            ret = EventCodeEnum.WAIT_FOR_PROBING_START;
                        }

                        //Command Slot Check
                        if (Module.CommandRecvSlot.IsRequested<IZDownRequest>())
                        {
                            Module.CommandRecvSlot.ClearToken();
                            LoggerManager.Debug("probingmodule IsRequested<IZDownRequest> is true. excute clear token. ");
                        }

                    }
                    else if (Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.SKIPPED)
                    {
                        ret = EventCodeEnum.PROBING_SEQUENCE_INVALID_ERROR;
                    }
                    else if(Module.CardChangeModule().IsZifRequestedState(true, writelog: false) != EventCodeEnum.NONE)
                    {
                        ret = EventCodeEnum.ZIF_STATE_NOT_READY;
                    }
                    else
                    {
                        ret = EventCodeEnum.WAIT_FOR_PROBING_START;
                    }
                }
                else
                {
                    ret = EventCodeEnum.WAIT_FOR_PROBING_START;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                ret = EventCodeEnum.PROBING_START_ERROR;
            }
            return ret;
        }
        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new ProbingIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum MoveToCenterDiePos()
        {
            LoggerManager.Error($"ProbingStateBase: Error occurred in MoveToCenterDiePos().");
            return EventCodeEnum.NODATA;
        }

        public override EventCodeEnum MoveToNextPinPadMatchedPos()
        {
            LoggerManager.Error($"ProbingStateBase: Error occurred in MOVETONEXTDIE().");
            return EventCodeEnum.NODATA;
        }
        public override EventCodeEnum ProbingZUP()
        {
            LoggerManager.Error($"ProbingStateBase: Error occurred in ProbingZUP().");
            return EventCodeEnum.NODATA;
        }
        public override EventCodeEnum ProbingZDOWN()
        {
            LoggerManager.Error($"ProbingStateBase: Error occurred in ProbingZDOWN().");
            return EventCodeEnum.NODATA;
        }
        private EventCodeEnum WaitforSoak(int soaktime)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                Stopwatch stw = new Stopwatch();
                stw.Start();
                bool runflag = true;
                while (runflag)
                {
                    if (stw.Elapsed.TotalSeconds >= soaktime)
                    {
                        ret = EventCodeEnum.NONE;
                        runflag = false;
                        stw.Stop();
                    }
                    if (stw.Elapsed.TotalSeconds >= 3600)
                    {
                        ret = EventCodeEnum.NONE;
                        runflag = false;
                        stw.Stop();
                    }
                    //System.Threading.Thread.Sleep(10);
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        protected EventCodeEnum BeforeZupSoak(WaferCoordinate wafer, PinCoordinate pin)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                bool UseStatusSoaking = false;
                bool IsEnableBeforeZupSoaking = false;
                double BeforeZupSoak_ZClearance = 0;
                int BeforeZupSoakTime = 0;

                Module.GetBeforeZupSoakOption(ref UseStatusSoaking, ref IsEnableBeforeZupSoaking, ref BeforeZupSoakTime, ref BeforeZupSoak_ZClearance);

                if (IsEnableBeforeZupSoaking)
                {
                    double zclearance = -1000;

                    LoggerManager.Debug($"Soak before zup OD:{0} Clearance Z:{BeforeZupSoak_ZClearance}", isInfo: Module.IsInfo);

                    zclearance = Module.ProbingModule().CalculateZClearenceUsingOD(0, BeforeZupSoak_ZClearance);

                    LoggerManager.Debug($"befre zup soaking Wafer X:{wafer.X.Value} Wafer Y:{wafer.Y.Value} Wafer Z:{wafer.Z.Value}, Pin X:{pin.X.Value} Pin Y:{pin.Y.Value} Pin Z:{pin.Z.Value} Clearance Z:{zclearance}", isInfo: Module.IsInfo);

                    LoggerManager.Debug($"Before Zup Soak Time:{BeforeZupSoakTime} sec", isInfo: Module.IsInfo);

                    ret = Module.StageSupervisor().StageModuleState.MovePadToPin(wafer, pin, zclearance);
                    if (ret == EventCodeEnum.NONE)
                    {
                        ret = WaitforSoak(BeforeZupSoakTime);
                    }
                    else
                    {
                        LoggerManager.Error($"Error occured while Before Zup Soak Moving");
                    }
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return ret;
        }

        protected EventCodeEnum ProbingZUPFunc()
        {
            LoggerManager.Debug($"[{Module.GetType().Name}].ProbingZUPFunc() : START");

            LoggerManager.ActionLog(ModuleLogType.PROBING_ZUP, StateLogType.START, $"OD : {Module.OverDrive}", this.Module.LoaderController().GetChuckIndex());

            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {

                //Camera 의 모든 조명 OFF
                var pinLowCam = Module.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                for (int lightindex = 0; lightindex < pinLowCam.LightsChannels.Count; lightindex++)
                {
                    pinLowCam.SetLight(pinLowCam.LightsChannels[lightindex].Type.Value, 0);
                }

                var pinHighCam = Module.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                for (int lightindex = 0; lightindex < pinHighCam.LightsChannels.Count; lightindex++)
                {
                    pinHighCam.SetLight(pinHighCam.LightsChannels[lightindex].Type.Value, 0);
                }

                var waferLowCam = Module.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                for (int lightindex = 0; lightindex < waferLowCam.LightsChannels.Count; lightindex++)
                {
                    waferLowCam.SetLight(waferLowCam.LightsChannels[lightindex].Type.Value, 0);
                }

                var waferHighCam = Module.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                for (int lightindex = 0; lightindex < waferHighCam.LightsChannels.Count; lightindex++)
                {
                    waferHighCam.SetLight(waferHighCam.LightsChannels[lightindex].Type.Value, 0);
                }



                IProbeCard probeCard = Module.StageSupervisor()?.ProbeCardInfo;

                WaferCoordinate Wafer = new WaferCoordinate();
                PinCoordinate pin = new PinCoordinate();

                double od = 0.0;
                this.Module.LoaderController().SetProbingStart(true);

                od = Module.OverDrive + Module.ManualContactModule().SelectedContactPosition;

                Wafer = Module.WaferAligner().MachineIndexConvertToProbingCoord((int)Module.ProbingLastMIndex.XIndex, (int)Module.ProbingLastMIndex.YIndex);

                if (Wafer.Z.Value == Module.WaferAligner().SafeHeightOnException)
                {
                    retval = EventCodeEnum.WAFER_HEIGHT_PROFILE_FAIL;
                    Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retval, "Probing ZUP Error", Module.ProbingModuleState.GetType().Name)));
                }
                else
                {
                    pin.X.Value = probeCard.ProbeCardDevObjectRef.PinCenX;
                    pin.Y.Value = probeCard.ProbeCardDevObjectRef.PinCenY;
                    pin.Z.Value = probeCard.ProbeCardDevObjectRef.PinHeight;

                    #region wafer/pin align forced done State => OverDrive값 변경
                    if (EnumModuleForcedState.ForcedDone.Equals(Module.LoaderController().GetModuleForcedState(ModuleEnum.PinAlign)) ||
                        EnumModuleForcedState.ForcedDone.Equals(Module.LoaderController().GetModuleForcedState(ModuleEnum.WaferAlign)))
                    {
                        od = -29000;
                        LoggerManager.Debug($"[{Module.GetType().Name}].ProbingZUPFunc() : Forced Done State -> overdrive value changed to {od}", isInfo: Module.IsInfo);
                    }
                    #endregion

                    retval = Module.StageSupervisor().StageModuleState.ProbingZUP(Wafer, pin, od);

                    if (Module.ManualContactModule().SelectedContactPosition != 0)
                    {
                        LoggerManager.Debug($"[Probing Contact Option] ODStartPosition : {Module.ProbingModuleSysParamRef.OverDriveStartPosition.Value}, OverDrive : {Module.ManualContactModule().SelectedContactPosition}", isInfo: Module.IsInfo);
                    }

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.ActionLog(ModuleLogType.PROBING_ZUP, StateLogType.ERROR, $"OD : {Module.OverDrive}", this.Module.LoaderController().GetChuckIndex());

                        Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retval, "Probing ZUP Error", Module.ProbingModuleState.GetType().Name)));
                    }
                    else
                    {
                        LoggerManager.ActionLog(ModuleLogType.PROBING_ZUP, StateLogType.DONE, $"OD : {Module.OverDrive}", this.Module.LoaderController().GetChuckIndex());

                        // Set ZupTime
                        SetZTime(GetState());

                        if (0 < Module.ProbingModuleSysParamRef.DWellZAxisTime.Value)
                        {
                            Module.InnerStateTransition(new ProbingZUpDWellState(Module));
                            LoggerManager.Debug($"[ProbingModule] Start Probing ZUp DWell. (Time : {Module.ProbingModuleSysParamRef.DWellZAxisTime.Value})", isInfo: Module.IsInfo);
                        }
                        else
                        {
                            Module.InnerStateTransition(new ProbingZUpState(Module));
                        }

                        Module.GetForceMeasure().MeasureProbingForce();
                    }
                }

                LoggerManager.Debug($"[{Module.GetType().Name}].ProbingZUPFunc() : END");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"[{Module.GetType().Name}].ProbingZUPFunc() : Fail");
                LoggerManager.Exception(err);

                retval = EventCodeEnum.EXCEPTION;

                Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retval, "Probing ZUP Error", Module.ProbingModuleState.GetType().Name)));
            }

            return retval;
        }

        protected EventCodeEnum ProbingZDNFunc()
        {
            LoggerManager.Debug($"[{Module.GetType().Name}].ProbingZDNFunc() : START");

            LoggerManager.ActionLog(ModuleLogType.PROBING_ZDOWN, StateLogType.START, $"Z Clearance : {Module.ZClearence}", this.Module.LoaderController().GetChuckIndex());

            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                ISubstrateInfo waferSubsInfo = Module.StageSupervisor().WaferObject.GetSubsInfo();
                WaferCoordinate Wafer = new WaferCoordinate();
                PinCoordinate pin = new PinCoordinate();

                double od = Module.OverDrive + Module.ManualContactModule().SelectedContactPosition;
                double zc = Module.ZClearence;
                zc = Module.CalculateZClearenceUsingOD(od, zc);

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    zc = -2000;
                }
                //zc = -2000; // Clearence for demo probing. Remove before release!!!

                Wafer.X.Value = 0;
                Wafer.Y.Value = 0;
                Wafer.Z.Value = waferSubsInfo.AveWaferThick;

                pin.X.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                pin.Y.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                pin.Z.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;
                this.Module.LoaderController().SetProbingStart(false);
                //
                RetVal = Module.StageSupervisor().StageModuleState.ProbingZDOWN(Wafer, pin, od, zc);

                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[{this.GetType().Name}] Fail ProbignZDown. od = {od}, zc = {zc}, wafer = 'x:{Wafer.GetX()},y:{Wafer.GetY()},z:{Wafer.GetZ()}', pin = 'x:{pin.GetX()},y:{pin.GetY()},z:{pin.GetZ()}'");

                    LoggerManager.ActionLog(ModuleLogType.PROBING_ZDOWN, StateLogType.ERROR, $"Z Clearance : {Module.ZClearence}", this.Module.LoaderController().GetChuckIndex());

                    Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "Probing ZDN Error. \nPlease check the status of the Z-Axis and recover it by system init.", Module.ProbingModuleState.GetType().Name)));

                    return EventCodeEnum.PROBING_ERROR;
                }
                else
                {
                    LoggerManager.ActionLog(ModuleLogType.PROBING_ZDOWN, StateLogType.DONE, $"Z Clearance : {Module.ZClearence}", this.Module.LoaderController().GetChuckIndex());

                    // Set ZDownTime
                    SetZTime(GetState());

                    ProbingStateBase nextState = null;

                    if (0 < Module.ProbingModuleSysParamRef.DWellZDownZAxisTime.Value)
                    {
                        nextState = new ProbingZDOWNDWellState(Module);
                        LoggerManager.Debug($"[ProbingModule] Start Probing ZDown DWell. (Time : {Module.ProbingModuleSysParamRef.DWellZAxisTime.Value})", isInfo: Module.IsInfo);
                    }
                    else
                    {
                        nextState = new ProbingZDOWNState(Module);
                    }

                    Module.GetForceMeasure().ResetMeasurement();
                    Module.InnerStateTransition(new ProbingZDownProcessEventState(Module, nextState));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                RetVal = EventCodeEnum.EXCEPTION;

                Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "Probing ZDN Error", Module.ProbingModuleState.GetType().Name)));
            }
            return RetVal;
        }

        protected EventCodeEnum MoveToNextDieFunc()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[{Module.GetType().Name}].MoveToNextDieFunc() : START");

            MachineIndex MI = null;
            SemaphoreSlim semaphore;
            PIVInfo pivinfo;
            try
            {
                RetVal = Module.ProbingSequenceModule().GetNextSequence(ref MI);

                // 마지막 프로빙 시퀀스를 가져온 후, 시퀀스모듈의 State는 NOSEQ

                ProbingSequenceStateEnum probingSequenceStateEnum = Module.ProbingSequenceModule().GetProbingSequenceState();

                if (probingSequenceStateEnum == ProbingSequenceStateEnum.SEQREMAIN || MI != null)
                {
                    Module.ProbingLastMIndex = MI;
                    Module.GetUnderDutDices(MI);

                    var usercoord = Module.CoordinateManager().WMIndexConvertWUIndex(MI.XIndex, MI.YIndex);
                    
                    LoggerManager.Debug($"[{Module.GetType().Name}].MOVETONEXTDIEFunc() : Index : X = {MI.XIndex}, Y = {MI.YIndex}", isInfo: true);
                   
                    long xindex = MI.XIndex;
                    long yindex = MI.YIndex;
                    string fullSite = "";

                    Module.GetStagePIVProbingData(ref xindex, ref yindex, usercoord, ref fullSite);

                    pivinfo = new PIVInfo(
                                xcoord: xindex,
                                ycoord: yindex,
                                FullSite: fullSite);

                    semaphore = new SemaphoreSlim(0);
                    RetVal = Module.EventManager().RaisingEvent(typeof(UpdateStageProbingDataEvent).FullName, new ProbeEventArgs(Module, semaphore, pivinfo));
                    semaphore.Wait();

                    WaferCoordinate Wafer = new WaferCoordinate();
                    PinCoordinate pin = new PinCoordinate();

                    double od = Module.OverDrive;
                    double zc = Module.ZClearence;

                    var flippedmovex = int.Parse(((int)MI.XIndex).ToString());
                    var flippedmovey = int.Parse(((int)MI.YIndex).ToString());

                    Wafer = Module.WaferAligner().MachineIndexConvertToProbingCoord((int)MI.XIndex, (int)MI.YIndex);

                    if (Wafer.Z.Value == Module.WaferAligner().SafeHeightOnException)
                    {
                        RetVal = EventCodeEnum.WAFER_HEIGHT_PROFILE_FAIL;
                        Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "MoveToNextDie Error", Module.ProbingModuleState.GetType().Name)));
                    }
                    else
                    {
                        pin.X.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                        pin.Y.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                        pin.Z.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                        zc = Module.CalculateZClearenceUsingOD(od, zc);

                        RetVal = Module.StageSupervisor().StageModuleState.MovePadToPin(Wafer, pin, zc);

                        if (RetVal == EventCodeEnum.NONE)
                        {
                            GPIBSysParam gpibSysParam = Module.GPIB().GPIBSysParam_IParam as GPIBSysParam;

                            if (gpibSysParam?.EnumGpibProbingMode?.Value == EnumGpibProbingMode.INTERNAL)
                            {
                                semaphore = new SemaphoreSlim(0);
                                Module.EventManager().RaisingEvent(typeof(CanProbingStartEvent).FullName, new ProbeEventArgs(Module, semaphore));
                                semaphore.Wait();
                            }

                            bool UseStatusSoaking = false;
                            bool IsEnableBeforeZupSoaking = false;
                            double TempBeforeZupSoak_ZClearance = -1000;
                            int TempBeforeZupSoakTime = 0;

                            Module.GetBeforeZupSoakOption(ref UseStatusSoaking, ref IsEnableBeforeZupSoaking, ref TempBeforeZupSoakTime, ref TempBeforeZupSoak_ZClearance);

                            if (IsEnableBeforeZupSoaking)
                            {
                                RetVal = BeforeZupSoak(Wafer, pin);

                                if (RetVal == EventCodeEnum.NONE)
                                {
                                    LoggerManager.Debug("Success soaking before zup");
                                }
                                else
                                {
                                    LoggerManager.Debug("problem has soaking before zup ");
                                }
                            }

                            // Stage 전환이 먼저 이루어져야 한다.
                            // ReadyToZUPEvent 발생 후, IZupRequest가 발생한다.
                            // ProbingPinPadMatchPerformState에서 받지 않고, ProbingPinPadMatchedState에서 받아짐.
                            // 타이밍이 꼬일 수 있다.
                            Module.InnerStateTransition(new ProbingPinPadMatchedState(Module));

                            ISubstrateInfo waferob = Module.GetParam_Wafer().GetSubsInfo();
                            int foupnum = Module.GetParam_Wafer().GetOriginFoupNumber();
                            int slotnum = (waferob.SlotIndex.Value % 25 == 0) ? 25 : waferob.SlotIndex.Value % 25;

                            Module.GetStagePIVProbingData(ref xindex, ref yindex, usercoord, ref fullSite);

                            pivinfo = new PIVInfo(
                            stagenumber: Module.LoaderController().GetChuckIndex(),
                            foupnumber: foupnum,
                            slotnumber: slotnum,
                            xcoord: xindex,
                            ycoord: yindex,
                            FullSite: fullSite,
                            curtemperature: Module.TempController().TempInfo.CurTemp.Value,
                            settemperature: Module.TempController().TempInfo.SetTemp.Value);

                            // ProbingPinPadMatchedState 스테이트 내부로 위치 변경
                            semaphore = new SemaphoreSlim(0);

                            if (Module.IsFirstZupSequence == false)
                            {
                                RetVal = Module.EventManager().RaisingEvent(typeof(MatchedToTestFirstProcessEvent).FullName, new ProbeEventArgs(Module, semaphore, pivinfo));
                                semaphore.Wait();
                            }

                           
                            RetVal = Module.EventManager().RaisingEvent(typeof(MatchedToTestEvent).FullName, new ProbeEventArgs(Module, semaphore, pivinfo));
                            semaphore.Wait();


                        }
                        else
                        {
                            Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "MovePadToPin Error", Module.ProbingModuleState.GetType().Name)));
                        }
                    }
                }
                else
                {
                    if (Module.ProbingSequenceModule().GetProbingSequenceState() == ProbingSequenceStateEnum.NOSEQ)
                    {
                        if (Module.IsFirstZupSequence != true && MI == null
                           && Module.LotOPModule().LotInfo.LotMode.Value == LotModeEnum.MPP)
                        {
                            Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PROBING_SEQUENCE_INVALID_ERROR, "There is currently no die to test on the wafer.\n Wafer is changed to the skipped state and returns to the origin position.", Module.ProbingModuleState.GetType().Name)));
                            Module.SetProbingEndState(ProbingEndReason.SEQUENCE_INVALID_ERROR);

                            Module.NotifyManager().Notify(EventCodeEnum.PROBING_SEQUENCE_INVALID_ERROR);
                            RetVal = EventCodeEnum.PROBING_SEQUENCE_INVALID_ERROR;
                        }
                        else
                        {
                            Module.InnerStateTransition(new DontRemainSequenceEventState(Module, this));
                        }

                    }
                    else
                    {
                        Module.StageSupervisor().StageModuleState.ZCLEARED();
                        Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, Module.ProbingSequenceModule().GetProbingSequenceState().ToString(), Module.ProbingModuleState.GetType().Name)));
                    }
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.EXCEPTION;

                LoggerManager.Error($"[{Module.GetType().Name}].MOVETONEXTDIEFunc() : Fail");
                LoggerManager.Exception(err);

                Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "MovePadToPin Error", Module.ProbingModuleState.GetType().Name)));
            }

            return RetVal;
        }
        protected EventCodeEnum MoveToCenterDieFunc()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[{Module.GetType().Name}].MoveToCenterDieFunc() : START");

            try
            {
                MachineIndex MI = new MachineIndex();
                MI.XIndex = Module.StageSupervisor().WaferObject.GetPhysInfo().MapCountX.Value / 2;
                MI.YIndex = Module.StageSupervisor().WaferObject.GetPhysInfo().MapCountY.Value / 2;
                //Module.ProbingSequenceModule().ProbingSequenceState.GetNextSequence(ref MI);

                Module.ProbingLastMIndex = MI;
                Module.GetUnderDutDices(MI);

                LoggerManager.Debug($"[{Module.GetType().Name}].MoveToCenterDieFunc() : Index : X = {MI.XIndex}, Y = {MI.YIndex}", isInfo: Module.IsInfo);

                WaferCoordinate Wafer = new WaferCoordinate();
                PinCoordinate pin = new PinCoordinate();
                double od = Module.OverDrive;
                double zc = Module.ZClearence;
                //Wafer = Module.WaferAligner().MachineIndexConvertToProbingCoord((int)MI.XIndex, (int)MI.YIndex);
                Wafer = Module.WaferAligner().MachineIndexConvertToProbingCoord((int)MI.XIndex, (int)MI.YIndex);

                if (Wafer.Z.Value == Module.WaferAligner().SafeHeightOnException)
                {
                    RetVal = EventCodeEnum.WAFER_HEIGHT_PROFILE_FAIL;
                    Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "MoveToCenterDie Error", Module.ProbingModuleState.GetType().Name)));
                }
                else
                {
                    pin.X.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                    pin.Y.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                    pin.Z.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;
                    zc = Module.CalculateZClearenceUsingOD(od, zc);

                    RetVal = Module.StageSupervisor().StageModuleState.MovePadToPin(Wafer, pin, zc);

                    if (RetVal == EventCodeEnum.NONE)
                    {
                        Module.InnerStateTransition(new ProbingPinPadMatchedState(Module));
                    }
                    else
                    {
                        Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "MovePadToPin Error", Module.ProbingModuleState.GetType().Name)));
                    }
                }

                LoggerManager.Debug($"[{Module.GetType().Name}].MoveToCenterDieFunc() : END");

            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.EXCEPTION;

                LoggerManager.Error($"[{Module.GetType().Name}].MoveToCenterDieFunc() : Fail");
                LoggerManager.Exception(err);

                Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "MovePadToPin Error", Module.ProbingModuleState.GetType().Name)));
            }
            return RetVal;
        }

        protected EventCodeEnum UnloadWafer()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Module.LoaderController().SetProbingStart(false);
                retVal = Module.StageSupervisor().StageModuleState.ZCLEARED();

                Module.ProbingMXIndex = Module.StageSupervisor().WaferObject.GetPhysInfo().MapCountX.Value / 2;
                Module.ProbingMYIndex = Module.StageSupervisor().WaferObject.GetPhysInfo().MapCountY.Value / 2;

                DateTime endTime = DateTime.Now.ToLocalTime();

                (Module.StageSupervisor().WaferObject.GetSubsInfo() as SubstrateInfo).ProbingEndTime = endTime;
                Module.LotOPModule().LotInfo.UpdateWafer(Module.StageSupervisor().WaferObject);

                LoggerManager.Debug($"PROBING END");

                Task<EventCodeEnum> uploadResultMapTask = null;

                // Result map Upload
                if (this.Module.StageSupervisor().StageMode != GPCellModeEnum.MAINTENANCE)
                {
                    if (this.Module.ResultMapManager().NeedUpload() &&
                        Module.StageSupervisor().WaferObject.GetSubsInfo().WaferType != EnumWaferType.POLISH &&
                        Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.SKIPPED)
                    {
                        uploadResultMapTask = new Task<EventCodeEnum>(() =>
                        {
                            return this.Module.ResultMapManager().Upload();
                        });
                        uploadResultMapTask.Start();
                    }
                }

                this.Module.InnerStateTransition(new ProbingHeatingPosState(Module, EnumProbingState.DONE, uploadResultMapTask));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                retVal = EventCodeEnum.EXCEPTION;

                Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, "Unload Wafer", Module.ProbingModuleState.GetType().Name)));
            }

            return retVal;
        }

        //x, y ´Â ¸Ó½Å ÀÎµ¦½º
        protected EventCodeEnum MoveToDiePositionFunc(long x, long y)
        {
            LoggerManager.Debug($"[{Module.GetType().Name}].MoveToDiePositionFunc() : START");

            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                MachineIndex MI = new MachineIndex(x, y);

                //if (MI.XIndex != 0 && MI.YIndex != 0)
                //{
                Module.ProbingLastMIndex = MI;
                Module.GetUnderDutDices(MI);
                //}

                if (Module.ProbingSequenceModule().GetProbingSequenceState() == ProbingSequenceStateEnum.SEQREMAIN)
                {
                    LoggerManager.Debug($"[{Module.GetType().Name}].MoveToDiePositionFunc() : Index : X = {MI.XIndex}, Y = {MI.YIndex}", isInfo: Module.IsInfo);

                    WaferCoordinate Wafer = new WaferCoordinate();
                    PinCoordinate pin = new PinCoordinate();
                    double od = Module.ProbingModule().OverDrive;
                    double zc = Module.ProbingModule().ZClearence;
                    zc = Module.ProbingModule().CalculateZClearenceUsingOD(od, zc);

                    //Wafer = Module.WaferAligner().MachineIndexConvertToProbingCoord((int)MI.XIndex, (int)MI.YIndex);
                    Wafer = Module.WaferAligner().MachineIndexConvertToProbingCoord((int)MI.XIndex, (int)MI.YIndex);

                    if (Wafer.Z.Value == Module.WaferAligner().SafeHeightOnException)
                    {
                        RetVal = EventCodeEnum.WAFER_HEIGHT_PROFILE_FAIL;
                        Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "MoveToDiePosition Error", Module.ProbingModuleState.GetType().Name)));
                    }
                    else
                    {
                        pin.X.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                        pin.Y.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                        pin.Z.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                        //todo - must remove!! jake.
                        //pin.Z.Value = -30000;

                        //zc = ProbingModule.ZClearence;
                        //zc = -2000; // Clearence for demo probing. Remove before release!!!

                        RetVal = Module.StageSupervisor().StageModuleState.MovePadToPin(Wafer, pin, zc);

                        if (RetVal == EventCodeEnum.NONE)
                        {
                            Module.InnerStateTransition(new MoveToDiePositionEventState(Module, new ProbingPinPadMatchedState(Module)));
                        }
                        else
                        {
                            Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "MovePadToPin Error", Module.ProbingModuleState.GetType().Name)));
                        }
                    }
                }
                else if (Module.ProbingSequenceModule().GetProbingSequenceState() == ProbingSequenceStateEnum.NOSEQ)
                {
                    Module.InnerStateTransition(new DontRemainSequenceEventState(Module, this));
                }
                else
                {
                    Module.StageSupervisor().StageModuleState.ZCLEARED();
                    Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, Module.ProbingSequenceModule().GetProbingSequenceState().ToString(), Module.ProbingModuleState.GetType().Name)));
                }

                LoggerManager.Debug($"[{Module.GetType().Name}].MoveToDiePositionFunc() : END");

            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.EXCEPTION;

                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, Module.ProbingSequenceModule().GetProbingSequenceState().ToString(), Module.ProbingModuleState.GetType().Name)));
            }
            return RetVal;
        }

        protected bool IsPossibleToStartProbing()
        {
            bool isProbingStart = true;

            if (SystemManager.SysteMode == SystemModeEnum.Single)
            {
                if (!this.Module.LotOPModule().LotInfo.StopBeforeProbeFlag)
                {
                    if (Module.LotOPModule().LotDeviceParam.StopOption.EveryStopBeforeProbing.Value ||
                        Module.LotOPModule().LotDeviceParam.OperatorStopOption.EveryStopBeforeProbing.Value)
                    {
                        isProbingStart = false;
                        Module.LotOPModule().ModuleStopFlag = true;

                        this.Module.LotOPModule().ReasonOfStopOption.IsStop = true;
                        this.Module.LotOPModule().ReasonOfStopOption.Reason = StopOptionEnum.EVERY_STOP_BEFORE_PROBING;

                        this.Module.CommandManager().SetCommand<ILotOpPause>(this);
                    }
                    else if (Module.LotOPModule().LotDeviceParam.StopOption.StopBeforeProbing.Value ||
                             Module.LotOPModule().LotDeviceParam.OperatorStopOption.StopBeforeProbing.Value)
                    {
                        if (Module.LotOPModule().LotDeviceParam.OperatorStopOption.StopBeforeProbingFlag.Value[Module.StageSupervisor().WaferObject.GetSubsInfo().SlotIndex.Value - 1] ||
                            Module.LotOPModule().LotDeviceParam.StopOption.StopBeforeProbingFlag.Value[Module.StageSupervisor().WaferObject.GetSubsInfo().SlotIndex.Value - 1])
                        {
                            isProbingStart = false;
                            Module.LotOPModule().ModuleStopFlag = true;

                            this.Module.LotOPModule().ReasonOfStopOption.IsStop = true;
                            this.Module.LotOPModule().ReasonOfStopOption.Reason = StopOptionEnum.STOP_AFTER_PROBING;

                            this.Module.CommandManager().SetCommand<ILotOpPause>(this);

                            Module.NotifyManager().Notify(EventCodeEnum.STOP_BEFORE_PROBING);
                        }
                    }
                }
            }

            return isProbingStart;
        }

        protected void RequestedGoToDieCommandProcessing<T>() where T : IProbeCommand
        {
            try
            {
                if (Module.CommandRecvSlot.IsRequested<T>())
                {
                    DateTime startTime = DateTime.Now.ToLocalTime();
                    (Module.StageSupervisor().WaferObject.GetSubsInfo() as SubstrateInfo).ProbingStartTime = startTime;

                    Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.PROBING);
                    Module.LotOPModule().LotInfo.UpdateWafer(Module.StageSupervisor().WaferObject);

                    LoggerManager.ActionLog(ModuleLogType.PROBING, StateLogType.START, $"Device: {Module.FileManager().GetDeviceName()}, Wafer ID : {Module.GetParam_Wafer().GetSubsInfo().WaferID.Value}, Card ID : {Module.CardChangeModule().GetProbeCardID()}, OD : {Module.OverDrive}", this.Module.LoaderController().GetChuckIndex());

                    Module.InnerStateTransition(new ProbingPinPadMatchPerformState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //protected void ResponseUnloadWaferCommand()
        //{
        //    Func<bool> conditionFunc = () => true;

        //    Action doAction = () =>
        //    {
        //        if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST)
        //        {
        //            EventCodeEnum actionRetVal = EventCodeEnum.UNDEFINED;
        //            actionRetVal = UnloadWafer();
        //        }
        //        else
        //        {
        //            //Idle 에서 probing 을 할 수 없는 상태일 때,  Unloading 을 한다는 것은 이상함.
        //            Module.CommandManager().SetCommand<ILotOpPause>(this);
        //        }
        //    };

        //    Action abortAction = () => { };

        //    bool isExecuted = Module.CommandManager().ProcessIfRequested<IUnloadWafer>(Module, conditionFunc, doAction, abortAction);
        //}
    }
    public class ProbingZDownProcessEventState : ProbingStateBase
    {
        public ProbingStateBase NextState { get; set; }

        public ProbingZDownProcessEventState(Probing module, ProbingStateBase NextState) : base(module)
        {
            try
            {
                this.NextState = NextState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool IsExecute = false;

            try
            {
                IsExecute = NextState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return IsExecute;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.CommandRecvDoneSlot.Token is IZDownRequest)
                {
                    ISubstrateInfo waferSubstrateInfo = Module.GetParam_Wafer().GetSubsInfo();

                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    retVal = Module.EventManager().RaisingEvent(typeof(ProbingZDownProcessEvent).FullName, new ProbeEventArgs(Module, semaphore));
                    semaphore.Wait();
                }

                Module.InnerStateTransition(NextState);
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

        public override EnumProbingState GetState()
        {
            return EnumProbingState.EVENT;
        }

        public override EventCodeEnum Pause()
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
    }
    public class GoToStartDieEventState : ProbingStateBase
    {
        public ProbingStateBase NextState { get; set; }

        public GoToStartDieEventState(Probing module, ProbingStateBase NextState) : base(module)
        {
            try
            {
                this.NextState = NextState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool IsExecute = false;

            try
            {
                IsExecute = NextState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return IsExecute;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                ISubstrateInfo waferob = Module.GetParam_Wafer().GetSubsInfo();
                int foupnum = ((waferob.SlotIndex.Value - 1) / 25) + 1;
                int slotnum = (waferob.SlotIndex.Value % 25 == 0) ? 25 : waferob.SlotIndex.Value % 25;
                
                MachineIndex MI = Module.ProbingLastMIndex;

                long xindex = MI.XIndex;
                long yindex = MI.YIndex;
                string fullSite = "";
                var usercoord = Module.CoordinateManager().WMIndexConvertWUIndex(MI.XIndex, MI.YIndex);

                Module.GetStagePIVProbingData(ref xindex, ref yindex, usercoord, ref fullSite);

                PIVInfo pivinfo = new PIVInfo(
                    foupnumber: foupnum,
                    //slotnumber: slotnum,
                    preloadingWaferId: "",
                    xcoord: xindex,
                    ycoord: yindex,
                    curtemperature: Module.TempController().TempInfo.CurTemp.Value,
                    settemperature: Module.TempController().TempInfo.SetTemp.Value
                    //od: Module.OverDrive
                    );

                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                retVal = Module.EventManager().RaisingEvent(typeof(GoToStartDieEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                semaphore.Wait();
                Module.InnerStateTransition(NextState);
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

        public override EnumProbingState GetState()
        {
            return EnumProbingState.EVENT;
        }

        public override EventCodeEnum Pause()
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
    }
    public class DontRemainSequenceEventState : ProbingStateBase
    {
        public ProbingStateBase NextState { get; set; }

        public DontRemainSequenceEventState(Probing module, ProbingStateBase NextState) : base(module)
        {
            try
            {
                this.NextState = NextState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool IsExecute = false;

            try
            {
                IsExecute = NextState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return IsExecute;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Module.SetProbingEndState(ProbingEndReason.NORMAL);
               
                if (Module.CommandRecvSlot.Token.Parameter is ProbingCommandParam)
                {
                    var ProbingParameter = Module.CommandRecvSlot.Token.Parameter as ProbingCommandParam;
                    if (ProbingParameter.ForcedZdownAndUnload == true)
                    {
                        var setCommRet = Module.CommandManager().SetCommand<IUnloadWafer>(Module);

                        if (setCommRet == false)
                        {
                            this.Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(ModuleEnum.Probing, EventCodeEnum.UNDEFINED, "", "")));
                        }
                    }
                    else
                    {
                        Module.CommandRecvSlot.ClearToken();
                        LoggerManager.Debug($"[ProbingModule] Forced Zdown And Unload : {ProbingParameter.ForcedZdownAndUnload}");
                    }
                }
                else 
                {
                    Module.CommandRecvSlot.ClearToken();
                }

                var physicalInfo = Module.GetParam_Wafer().GetPhysInfo();
                var substrateInfo = Module.GetParam_Wafer().GetSubsInfo();
                var pivinfo = new PIVInfo(
                                          foupnumber: Module.GetParam_Wafer().GetOriginFoupNumber(),
                                          notchangle: (int)physicalInfo.NotchAngle.Value,
                                          waferendrst: 0,
                                          totalDieCount: substrateInfo.TestedDieCount.Value,
                                          passDieCount: substrateInfo.PassedDieCount.Value,
                                          faildiecount: substrateInfo.FailedDieCount.Value,
                                          yieldbaddie: 0,
                                          waferStartTime: substrateInfo.DIEs[0, 0].CurTestHistory.StartTime.Value.ToString("yyyyMMddHHmmss"),
                                          waferEndTime: substrateInfo.DIEs[0, 0].CurTestHistory.EndTime.Value.ToString("yyyyMMddHHmmss"),
                                          pcardcontactcount: Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.TouchdownCount.Value,
                                          curtemperature: Module.TempController().TempInfo.CurTemp.Value,
                                          settemperature: Module.TempController().TempInfo.SetTemp.Value,
                                          od: (int)Module.OverDrive);

                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Module.EventManager().RaisingEvent(typeof(DontRemainSequenceEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                semaphore.Wait();

                Module.InnerStateTransition(NextState);
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

        public override EnumProbingState GetState()
        {
            return EnumProbingState.EVENT;
        }

        public override EventCodeEnum Pause()
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
    }
    public class MoveToDiePositionEventState : ProbingStateBase
    {
        public ProbingStateBase NextState { get; set; }

        public MoveToDiePositionEventState(Probing module, ProbingStateBase NextState) : base(module)
        {
            try
            {
                this.NextState = NextState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool IsExecute = false;

            try
            {
                IsExecute = NextState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return IsExecute;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.EventManager().RaisingEvent(typeof(MoveToDiePositionEvent).FullName);
                Module.InnerStateTransition(NextState);
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

        public override EnumProbingState GetState()
        {
            return EnumProbingState.EVENT;
        }

        public override EventCodeEnum Pause()
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
    }
    public class ProbingIdleState : ProbingStateBase
    {
        public ProbingIdleState(Probing module) : base(module)
        {
            try
            {
                this.Module.IsFirstZupSequence = false;
                this.Module.LotOPModule().LotInfo.StopBeforeProbeFlag = false;

                this.Module.ProbingSequenceModule().ResetProbingSequence();

                this.Module.IsReservePause = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum End()
        {
            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                string errorReasonStr = null;

                RetVal = IsCanProbingStart(ref errorReasonStr);

                if (RetVal == EventCodeEnum.NONE)
                {
                    this.Module.ProbingModule().SetProbingEndState(ProbingEndReason.UNDEFINED);
                    RetVal = this.Module.InnerStateTransition(new ProbingHeatingPosState(Module, EnumProbingState.READY));
                    return RetVal;
                }
                else if (RetVal == EventCodeEnum.WAIT_FOR_PROBING_START)
                {
                    // NOTHING
                }
                else
                {
                    string message = string.Empty;
                    bool isError = false;

                    if (RetVal == EventCodeEnum.PIN_PAD_MATCH_FAIL ||
                        RetVal == EventCodeEnum.PIN_NOT_ENOUGH ||
                        RetVal == EventCodeEnum.PIN_INVAILD_STATUS ||
                        RetVal == EventCodeEnum.PIN_OVER_ANGLE_TOLERANCE ||
                        RetVal == EventCodeEnum.PTPA_OUT_OF_TOLERENCE)
                    {
                        // TODO : RetVal을 사용하지 못하는 이유가 있는가?
                        Module.NotifyManager().Notify(EventCodeEnum.PIN_PAD_MATCH_FAIL);

                        message = "Pin/Pad matching failure : Please check matching tolerance.";
                        isError = true;
                    }
                    else if (RetVal == EventCodeEnum.ZIF_STATE_NOT_READY)
                    {
                        message = "Zif Lock State for probing is not ready";
                        isError = true;
                    }
                    else if (RetVal == EventCodeEnum.PROBING_START_ERROR)
                    {
                        message = "Error occured while is can probing start";
                        isError = true;
                    }
                    else if (RetVal == EventCodeEnum.PROBING_Z_LIMIT_ERROR)
                    {
                        message = $"OverDrive Value : {errorReasonStr}";
                        isError = true;
                    }
                    else
                    {
                        // Undefined 
                    }

                    if (isError)
                    {
                        LoggerManager.Prolog(PrologType.INFORMATION, RetVal, RetVal);

                        RetVal = Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, message, Module.ProbingModuleState.GetType().Name)));
                        return RetVal;
                    }
                }

                Func<bool> conditionFunc = () => true;

                Action doAction = () =>
                {
                    EventCodeEnum actionRetVal = EventCodeEnum.UNDEFINED;
                    actionRetVal = UnloadWafer();
                };

                Action abortAction = () => { };

                bool isExecuted = Module.CommandManager().ProcessIfRequested<IUnloadWafer>(Module, conditionFunc, doAction, abortAction);
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

        public override EnumProbingState GetState()
        {
            return EnumProbingState.IDLE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = token is IGoToStartDie ||
                                    token is IGoToCenterDie ||
                                    token is IMoveToDiePosition ||
                                    token is IUnloadWafer ||
                                    token is IZDownRequest;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.ProbingModuleState.GetType().Name)));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
    public class ProbingHeatingPosState : ProbingStateBase
    {
        public EnumProbingState NextState { get; set; }

        private bool _NeedToMoveHeatingPos;
        private bool NeedToMoveHeatingPos
        {
            get { return _NeedToMoveHeatingPos; }
            set
            {
                if (value != _NeedToMoveHeatingPos)
                {
                    LoggerManager.Debug($"[ProbingHeatingPosState] NeedToMoveHeatingPos Changed Prev:{_NeedToMoveHeatingPos} Cur:{value}");
                }
                _NeedToMoveHeatingPos = value;

            }
        }

        public Task<EventCodeEnum> UploadResultMapTask = null;

        public ProbingHeatingPosState(Probing module, EnumProbingState NextState, Task<EventCodeEnum> UploadResultMapTask = null) : base(module)
        {
            try
            {
                if (this.Module.ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "MOVE HEATING POS");
                }

                this.NeedToMoveHeatingPos = true;
                this.NextState = NextState;
                this.UploadResultMapTask = UploadResultMapTask;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.NextState == EnumProbingState.DONE) 
                {
                    if (UploadResultMapTask != null &&
                                       UploadResultMapTask.IsCompleted &&
                                       Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.SKIPPED)
                    {
                        if (UploadResultMapTask.Result == EventCodeEnum.NONE &&
                            UploadResultMapTask.IsCanceled == false &&
                            UploadResultMapTask.IsFaulted == false)
                        {
                            LoggerManager.Debug($"[ProbingDoneState] (Resultmap Upload) Done ");
                        }
                        else
                        {
                            LoggerManager.Debug($"[ProbingDoneState] (Resultmap Upload) Fail ");
                        }
                    }
                    else if (UploadResultMapTask != null)
                    {
                        return RetVal;
                    }
                }

                if (Module.PreInnerState.GetModuleState() == ModuleStateEnum.PAUSED)
                {
                    NeedToMoveHeatingPos = true;
                }
                
                EventCodeEnum moveRst = EventCodeEnum.UNDEFINED;
                if (NeedToMoveHeatingPos)
                {
                    moveRst = MoveHeatingPosition();
                    this.NeedToMoveHeatingPos = false;
                }
                else 
                {
                    moveRst = EventCodeEnum.NONE;
                }

                EnumProbingState PrevProbingState = (Module.PreInnerState as ProbingStateBase).GetState();
                EventCodeEnum code = EventCodeEnum.UNDEFINED;
                string message = string.Empty;
                if (PrevProbingState != EnumProbingState.PAUSED)
                {
                    if (PrevProbingState == EnumProbingState.IDLE && this.Module.LotOPModule().LotDeviceParam.StopOption.StopBeforeProbing.Value)
                    {
                        this.NeedToMoveHeatingPos = true;
                        code = EventCodeEnum.STOP_BEFORE_PROBING;
                        message = "Paused by StopBeforeProbing option";
                    }
                    else if (PrevProbingState == EnumProbingState.IDLE && this.Module.LotOPModule().LotDeviceParam.StopOption.OnceStopBeforeProbing.Value)
                    {
                        this.NeedToMoveHeatingPos = true;
                        code = EventCodeEnum.STOP_BEFORE_PROBING;
                        message = "Paused by StopBeforeProbing option (Once)";
                        this.Module.LotOPModule().LotDeviceParam.StopOption.OnceStopBeforeProbing.Value = false;
                    }

                    if (this.NextState == EnumProbingState.DONE && this.Module.LotOPModule().LotDeviceParam.StopOption.StopAfterProbing.Value)
                    {
                        this.NeedToMoveHeatingPos = true;
                        code = EventCodeEnum.STOP_AFTER_PROBING;
                        message = "Paused by StopAfterProbing option";
                    }
                    else if (this.NextState == EnumProbingState.DONE && this.Module.LotOPModule().LotDeviceParam.StopOption.OnceStopAfterProbing.Value)
                    {
                        this.NeedToMoveHeatingPos = true;
                        code = EventCodeEnum.STOP_AFTER_PROBING;
                        message = "Paused by StopAfterProbing option (Once)";

                        this.Module.LotOPModule().LotDeviceParam.StopOption.OnceStopAfterProbing.Value = false;
                    }

                    if (code == EventCodeEnum.STOP_AFTER_PROBING)
                    {
                        Module.FinalizeWaferProcessing();
                    }
                }

                if (moveRst != EventCodeEnum.NONE)
                {
                    Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "ProbingHeatingPosState Move Error", Module.ProbingModuleState.GetType().Name)));
                    return moveRst;
                }

                if (code == EventCodeEnum.STOP_AFTER_PROBING || code == EventCodeEnum.STOP_BEFORE_PROBING)
                {
                    RetVal = Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, code, message, Module.ProbingModuleState.GetType().Name)));
                    return RetVal;
                }

                // 이전 상태만으로 구분할 수 없다.
                if (this.NextState == EnumProbingState.READY)
                {
                    this.Module.InnerStateTransition(new ProbingReadyState(Module));
                }
                else if (this.NextState == EnumProbingState.DONE)
                {
                    RetVal = Module.InnerStateTransition(new ProbingDoneState(Module));
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.EXCEPTION;

                LoggerManager.Exception(err);

                Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "ProbingHeatingPosState Error", Module.ProbingModuleState.GetType().Name)));
            }

            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EnumProbingState GetState()
        {
            return EnumProbingState.MOVE_HEATING_POS;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool IsExecute = false;

            try
            {
                IsExecute = token is IGoToStartDie ||
                            token is IGoToCenterDie ||
                            token is IMoveToDiePosition ||
                            token is IUnloadWafer ||
                            token is IZDownRequest ||
                            token is IResumeProbing;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return IsExecute;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.ProbingModuleState.GetType().Name)));
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private EventCodeEnum MoveHeatingPosition()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                //Resume Command가 들어오기 전이면 X, Y는 Start Die, Z는 Pin Pad Matched ZPosition으로 움직인다.

                RetVal = Module.StageSupervisor().StageModuleState.ZCLEARED();

                if (RetVal != EventCodeEnum.NONE)
                {
                    return RetVal;
                }
                RetVal = Module.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
                if (RetVal != EventCodeEnum.NONE)
                {
                    return RetVal;
                }

                double od = Module.OverDrive;
                double zc = Module.ZClearence;

                WaferCoordinate Wafer = new WaferCoordinate();
                PinCoordinate pincoord = new PinCoordinate();

                pincoord.X.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                pincoord.Y.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                pincoord.Z.Value = Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                MachineIndex MI = new MachineIndex();
                var retVal = Module.ProbingModule().ProbingSequenceModule().GetFirstSequence(ref MI);

                if (retVal == EventCodeEnum.NONE)
                {
                    Wafer = Module.WaferAligner().MachineIndexConvertToProbingCoord((int)MI.XIndex, (int)MI.YIndex);
                    LoggerManager.Debug($"[{Module.GetType().Name}],wafercoord: (X:{Wafer.X.Value}, Y:{Wafer.Y.Value}, Z:{Wafer.Z.Value}, T:{Wafer.T.Value}), pincoord: (X:{pincoord.X.Value}, Y:{pincoord.Y.Value}, Z:{pincoord.Z.Value})");

                }
                else
                {
                    RetVal = EventCodeEnum.EXCEPTION;

                    LoggerManager.Error($"[{Module.GetType().Name}].GetFirstSequence() : Fail, MI(X={(int)MI.XIndex}, Y={(int)MI.YIndex})");

                    Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "GetFirstSequence() Error", Module.ProbingModuleState.GetType().Name)));
                    return RetVal;
                }

                zc = Module.ProbingModule().CalculateZClearenceUsingOD(od, zc); //핀패드매치드 Z위치

                LoggerManager.Debug($"[{Module.GetType().Name}],MI(X={(int)MI.XIndex}, Y={(int)MI.YIndex})");
                LoggerManager.Debug($"[{Module.GetType().Name}].Move to Start Die : START ");
                LoggerManager.Debug($"[{Module.GetType().Name}], CalculateZClearenceUsingOD(),zc : {zc}");

                RetVal = Module.StageSupervisor().StageModuleState.MovePadToPin(Wafer, pincoord, zc);

                if (RetVal == EventCodeEnum.NONE)
                {
                    double CurChuck_posX = 0; //현재 위치
                    double CurChuck_posY = 0;
                    double CurChuck_posZ = 0;

                    Module.MotionManager().GetRefPos(EnumAxisConstants.X, ref CurChuck_posX);
                    Module.MotionManager().GetRefPos(EnumAxisConstants.Y, ref CurChuck_posY);
                    Module.MotionManager().GetRefPos(EnumAxisConstants.Z, ref CurChuck_posZ);

                    LoggerManager.Debug($"[{Module.GetType().Name}],CurChuck_pos: ({CurChuck_posX}, {CurChuck_posY}, {CurChuck_posZ})");
                    LoggerManager.Debug($"[{Module.GetType().Name}].MovePadToPin() : END, RetVal : {RetVal}");
                }
                else
                {
                    RetVal = EventCodeEnum.EXCEPTION;
                    LoggerManager.Error($"[{Module.GetType().Name}].MovePadToPin() : Fail, wafercoord: (X:{Wafer.X.Value}, Y:{Wafer.Y.Value}, Z:{Wafer.Z.Value}, T:{Wafer.T.Value}), pincoord: (X:{pincoord.X.Value}, Y:{pincoord.Y.Value}, Z:{pincoord.Z.Value})");
                    Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "MovePadToPin Error", Module.ProbingModuleState.GetType().Name)));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                NeedToMoveHeatingPos = false;
            }

            return RetVal;
        }
    }
    public class ProbingReadyState : ProbingStateBase
    {
        private bool IsRaisedCanProbingStartEvent = false;
        private bool IsRaisedCellPausedEvent = false;

        public ProbingReadyState(Probing module) : base(module)
        {
            try
            {
                if (Module.ProbingModuleSysParamRef.WaitProbingStartRspEnable.Value)
                {
                    Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "WAIT FOR RESUME");
                }

                this.Module.IsFirstZupSequence = false;

                this.IsRaisedCanProbingStartEvent = false;
                this.IsRaisedCellPausedEvent = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void GoToDie()
        {
            try
            {
                bool isProbingStart = IsPossibleToStartProbing();

                // 현재 GP에서 항상 true
                if (isProbingStart)
                {
                    SemaphoreSlim semaphore;

                    if (!IsRaisedCanProbingStartEvent)
                    {
                        semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CanProbingStartEvent).FullName, new ProbeEventArgs(Module, semaphore));
                        semaphore.Wait();

                        IsRaisedCanProbingStartEvent = true;
                    }

                    if (Module.TCPIPModule().GetTCPIPOnOff() != EnumTCPIPEnable.ENABLE)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.GetParam_Wafer().GetOriginFoupNumber(), waferStartTime: DateTime.Now.ToString("yyyyMMddHHmmss"), curtemperature: Module.TempController().TempInfo.CurTemp.Value, settemperature: Module.TempController().TempInfo.SetTemp.Value);
                        semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(WaferStartEvent).FullName, new ProbeEventArgs(Module, semaphore, pivinfo));
                        semaphore.Wait();
                    }

                    GPIBSysParam gpibSysParam = Module.GPIB().GPIBSysParam_IParam as GPIBSysParam;

                    bool shouldGoTOStartDie = false;
                    bool shouldGoToCenterDie = false;

                    if (Module.GPIB().GetGPIBEnable() == EnumGpibEnable.ENABLE)
                    {
                        if (gpibSysParam?.EnumGpibProbingMode?.Value != EnumGpibProbingMode.EXTERNAL)
                        {
                            shouldGoTOStartDie = true;
                        }
                        else
                        {
                            shouldGoToCenterDie = true;
                        }
                    }
                    else if (Module.TCPIPModule().GetTCPIPOnOff() == EnumTCPIPEnable.ENABLE)
                    {
                        shouldGoTOStartDie = true;
                    }
                    else if (Module.GEMModule().GemEnable())
                    {
                        shouldGoTOStartDie = true;
                    }

                    if (shouldGoTOStartDie)
                    {
                        bool isInjected = Module.CommandManager().SetCommand<IGoToStartDie>(this);

                        if (isInjected)
                        {
                            RequestedGoToDieCommandProcessing<IGoToStartDie>();
                        }
                    }
                    else if (shouldGoToCenterDie)
                    {
                        bool isInjected = Module.CommandManager().SetCommand<IGoToCenterDie>(this);

                        if (isInjected)
                        {
                            RequestedGoToDieCommandProcessing<IGoToCenterDie>();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /* 2021.09.27 - YMTC
        * GPIB 시나리오에서 Idle 상태에서 IMoveToDiePosition 커맨드가 
        * 입력될 수도 있음. 
        * 이런 경우 바로 ProbingPinPadMatchPerformState로 전이함.
        * 2023.08.18 : Execute()에 있던 로직으로, 현재 사용되지 않았으며, 향후 다시 확인
        */
        //if (Module.GPIB().GetGPIBEnable() == EnumGpibEnable.ENABLE)
        //{
        //    if (Module.CommandRecvSlot.IsRequested<IMoveToDiePosition>())
        //    {
        //        RetVal = Module.InnerStateTransition(new ProbingPinPadMatchPerformState(Module));
        //    }
        //}
        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.ProbingModuleSysParamRef.WaitProbingStartRspEnable.Value)
                {
                    if (!IsRaisedCellPausedEvent)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.GetParam_Wafer().GetOriginFoupNumber(), waferid: Module.GetParam_Wafer().GetSubsInfo().WaferID.Value, curtemperature: Module.TempController().TempInfo.CurTemp.Value, settemperature: Module.TempController().TempInfo.SetTemp.Value);
                        
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CellPausedEvent).FullName, new ProbeEventArgs(Module, semaphore, pivinfo));
                        semaphore.Wait();

                        IsRaisedCellPausedEvent = true;
                    }

                    string setTitle = "WAIT FOR RESUME";
                    if (!Module.NotifyManager().GetLastStageMSG().Equals(setTitle))
                    {
                        //생성자에서 만들어준 Message 가 LotOPModule의 State 덮어지는 문제로 추가된 코드임.
                        Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), setTitle);
                    }

                    bool isExecuted = false;

                    Func<bool> condition = () => { return true; };

                    Action doAction = () =>
                    {
                        GoToDie();
                    };

                    isExecuted = Module.CommandManager().ProcessIfRequested<IResumeProbing>(Module, condition, doAction, null);

                    if (!isExecuted)
                    {
                        Func<bool> conditionFunc = () => true;

                        doAction = () =>
                        {
                            EventCodeEnum actionRetVal = EventCodeEnum.UNDEFINED;
                            actionRetVal = UnloadWafer();
                        };

                        Action abortAction = () => { };

                        isExecuted = Module.CommandManager().ProcessIfRequested<IUnloadWafer>(Module, conditionFunc, doAction, abortAction);
                    }
                }
                else
                {
                    GoToDie();
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.EXCEPTION;

                LoggerManager.Exception(err);

                Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, RetVal, "MovePadToPin Error", Module.ProbingModuleState.GetType().Name)));
            }

            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EnumProbingState GetState()
        {
            return EnumProbingState.READY;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool IsExecute = false;

            try
            {
                IsExecute = token is IGoToStartDie ||
                                    token is IGoToCenterDie ||
                                    token is IMoveToDiePosition ||
                                    token is IUnloadWafer ||
                                    token is IZDownRequest ||
                                    token is IResumeProbing;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return IsExecute;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.ProbingModuleState.GetType().Name)));
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

    }
    public class ProbingPinPadMatchPerformState : ProbingStateBase
    {
        public ProbingPinPadMatchPerformState(Probing module) : base(module)
        {
            if (Module.ModuleState.GetState() == ModuleStateEnum.RUNNING)
            {
                Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "PINPADMATCHPERFORM");
            }
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                Func<bool> conditionFunc = () => true;
                Action abortAction = () => { };
                bool isExecuted = false;

                Action doAction = () =>
                {
                    EventCodeEnum actionReturnVal = EventCodeEnum.UNDEFINED;

                    actionReturnVal = MoveToNextPinPadMatchedPos();

                    if (actionReturnVal == EventCodeEnum.NONE)
                    {
                        if (this.Module.IsFirstZupSequence == false)
                        {
                            this.Module.InnerStateTransition(new GoToStartDieEventState(Module, Module.InnerState as ProbingStateBase));

                            if (this.Module.ProbingModuleDevParamRef.IsEnableAutoZup.Value == true)
                            {
                                var setCommRet = Module.CommandManager().SetCommand<IZUPRequest>(Module);

                                if (setCommRet == false)
                                {
                                    this.Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(ModuleEnum.Probing, EventCodeEnum.UNDEFINED, "", "")));
                                }
                            }
                        }
                    }
                };

                isExecuted = Module.CommandManager().ProcessIfRequested<IGoToStartDie>(Module, conditionFunc, doAction, abortAction);

                Action doGoToCenterDieAction = () =>
                {
                    EventCodeEnum actionReturnVal = EventCodeEnum.UNDEFINED;
                    actionReturnVal = MoveToCenterDiePos();
                };

                isExecuted = Module.CommandManager().ProcessIfRequested<IGoToCenterDie>(Module, conditionFunc, doGoToCenterDieAction, abortAction);

                Action moveToNextDiePositionAction = () =>
                {
                    EventCodeEnum actionReturnVal = EventCodeEnum.UNDEFINED;
                    PositionParam point = null;
                    point = Module.CommandRecvProcSlot.Token.Parameter as PositionParam;

                    if (point != null)
                    {
                        actionReturnVal = MoveToDiePositionFunc(point.X, point.Y);
                        Module.EventManager().RaisingEvent(typeof(GoToCentertDieEvent).FullName);
                    }
                };

                isExecuted = Module.CommandManager().ProcessIfRequested<IMoveToDiePosition>(Module, conditionFunc, moveToNextDiePositionAction, abortAction);

                Action unLoadAction = () =>
                {
                    EventCodeEnum actionRetVal = EventCodeEnum.UNDEFINED;
                    actionRetVal = UnloadWafer();
                };

                isExecuted = Module.CommandManager().ProcessIfRequested<IUnloadWafer>(Module, conditionFunc, unLoadAction, abortAction);

                if (Module.CommandRecvSlot.IsRequested<IMoveToNextDie>())
                {
                    RetVal = MoveToNextPinPadMatchedPos();
                }
                else if (Module.CommandRecvSlot.IsRequested<IMoveToDiePositionAndZUp>())
                {
                    PositionParam point = null;

                    point = Module.CommandRecvSlot.Token.Parameter as PositionParam;

                    if (point != null)
                    {
                        RetVal = MoveToDiePositionFunc(point.X, point.Y);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EnumProbingState GetState()
        {
            return EnumProbingState.PINPADMATCHPERFORM;
        }

        public override EventCodeEnum MoveToNextPinPadMatchedPos()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = MoveToNextDieFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public override EventCodeEnum MoveToCenterDiePos()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = MoveToCenterDieFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = token is IUnloadWafer
                     || token is IGoToStartDie
                     || token is IMoveToDiePosition
                     || token is IMoveToDiePositionAndZUp;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.ProbingModuleState.GetType().Name)));
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
    public class ProbingPinPadMatchedState : ProbingStateBase
    {
        private DateTime InitStateTime;
        public bool rasingTimeOut = false;
        private bool IsRaisedWaferStartEventEvent = false;
        public ProbingPinPadMatchedState(Probing module) : base(module)
        {
            InitStateTime = DateTime.Now;
            rasingTimeOut = false;
            IsRaisedWaferStartEventEvent = false;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                bool IsLotHasSuspended = false;

                IsLotHasSuspended = Module.IsLotHasSuspendedState();

                if (!IsLotHasSuspended)
                {
                    if (Module.TCPIPModule().GetTCPIPOnOff() == EnumTCPIPEnable.ENABLE && IsRaisedWaferStartEventEvent == false && Module.IsFirstZupSequence != true)
                    {
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        RetVal = Module.EventManager().RaisingEvent(typeof(WaferStartEvent).FullName, new ProbeEventArgs(Module, semaphore));
                        semaphore.Wait();
                        IsRaisedWaferStartEventEvent = true;
                    }

                    if (Module.CommandRecvSlot.IsRequested<IZUPRequest>())
                    {
                        Module.InnerStateTransition(new ProbingZUPPerformState(Module));
                    }

                    if (Module.CommandRecvSlot.IsRequested<IMoveToDiePositionAndZUp>())
                    {
                        Module.InnerStateTransition(new ProbingZUPPerformState(Module));
                    }

                    if (Module.CommandRecvSlot.IsRequested<IMoveToDiePosition>())
                    {
                        Module.InnerStateTransition(new ProbingPinPadMatchPerformState(Module));
                    }

                    if (Module.CommandRecvSlot.IsRequested<IZDownRequest>())
                    {
                        Module.InnerStateTransition(new ProbingZDOWNPerformState(Module));
                    }

                    if (Module.ProbingModuleDevParamRef.PinPadMatchedTimeOut.Value != 0 && rasingTimeOut == false)
                    {
                        if (DateTime.Now.Subtract(InitStateTime).TotalSeconds >=
                            Module.ProbingModuleDevParamRef.PinPadMatchedTimeOut.Value)
                        {
                            Module.NotifyManager().Notify(EventCodeEnum.PIN_PAD_MATCHED_TEST_TIMEOUT);
                            rasingTimeOut = true;
                        }
                    }


                    Func<bool> conditionFuncForMoveToNextDie = () => true;
                    Action abortActionForMoveToNextDie = () => { };
                    Action doActionForCheckZupConditionAfterMoveToNextDie = () =>
                    {
                        try
                        {
                            var ProbingParameter = Module.CommandRecvProcSlot.Token.Parameter as ProbingCommandParam;

                            if (ProbingParameter != null)
                            {
                                var state = ProbingParameter.ProbingStateWhenReciveMoveToNextDie;

                                if (state == EnumProbingState.ZUP || state == EnumProbingState.ZUPDWELL)
                                {
                                    var setCommRet = Module.CommandManager().SetCommand<IZUPRequest>(Module);

                                    if (setCommRet == false)
                                    {
                                        this.Module.InnerStateTransition(new ProbingErrorState(Module, new EventCodeInfo(ModuleEnum.Probing, EventCodeEnum.UNDEFINED, "", "")));
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"[ProbingModule] ProbingState When Recive Move To Next Die : {state}");
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"[ProbingModule] IMoveToNextDie Parameter is null.");
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                    };

                    bool isExecuted = false;

                    isExecuted = Module.CommandManager().ProcessIfRequested<IMoveToNextDie>(Module, conditionFuncForMoveToNextDie, doActionForCheckZupConditionAfterMoveToNextDie, abortActionForMoveToNextDie);

                    Func<bool> conditionFuncForUnloadWafer = () => true;

                    Action doActionForUnloadWafer = () =>
                    {
                        EventCodeEnum actionRetVal = EventCodeEnum.UNDEFINED;
                        actionRetVal = UnloadWafer();
                    };

                    Action abortActionForUnloadWafer = () => { };

                    isExecuted = Module.CommandManager().ProcessIfRequested<IUnloadWafer>(Module, conditionFuncForUnloadWafer, doActionForUnloadWafer, abortActionForUnloadWafer);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }

        public override EnumProbingState GetState()
        {
            return EnumProbingState.PINPADMATCHED;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = token is IZUPRequest
                                || token is IMoveToNextDie
                                || token is IMoveToDiePositionAndZUp
                                || token is IMoveToDiePosition
                                || token is IUnloadWafer
                                || token is IZDownRequest;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.ProbingModuleState.GetType().Name)));
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
    public class ProbingZUPPerformState : ProbingStateBase
    {
        public ProbingZUPPerformState(Probing module) : base(module)
        {

        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                ITempController TempController = Module.TempController();

                if (TempController.IsCurTempWithinSetTempRange())
                {
                    Func<bool> conditionFunc = () => true;
                    Action doAction = () =>
                    {
                        EventCodeEnum actionRetVal = EventCodeEnum.UNDEFINED;

                        actionRetVal = ProbingZUP();

                        if (actionRetVal == EventCodeEnum.NONE)
                        {
                            // ZUP 성공 시
                            // (!) TouchDownCount 증가
                            // (2) 현재 사용중인 카드의 ContactCount 증가

                            Module.LotOPModule().LotInfo.IncreaseTouchDownCount();
                            Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.TouchdownCount.Value++;

                            var foupNum = Module.GetParam_Wafer().GetOriginFoupNumber();
                            var pivinfo = new PIVInfo() { FoupNumber = foupNum };
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);

                            if (Module.IsFirstZupSequence == false)
                            {
                                RetVal = Module.EventManager().RaisingEvent(typeof(ProbingZUpFirstProcessEvent).FullName, new ProbeEventArgs(Module, semaphore, pivinfo));
                                semaphore.Wait();
                            }

                            // multi contact 이 아닐경우에는 ProbingZUpFirstProcessEvent에서만 동작함. 한번만 컨택한다고 하면 ProbingZUpFirstProcessEvent와 ProbingZUpProcessEvent의 event id 똑같이 맞춰주기
                            RetVal = Module.EventManager().RaisingEvent(typeof(ProbingZUpProcessEvent).FullName, new ProbeEventArgs(Module, semaphore, pivinfo));
                            semaphore.Wait();

                            if (Module.IsFirstZupSequence == false)
                            {
                                Module.IsFirstZupSequence = true;
                            }
                        }
                        else
                        {

                        }
                    };
                    Action abortAction = () => { };

                    bool isExecuted = false;

                    isExecuted = Module.CommandManager().ProcessIfRequested<IZUPRequest>(Module, conditionFunc, doAction, abortAction);

                    isExecuted = Module.CommandManager().ProcessIfRequested<IMoveToDiePositionAndZUp>(Module, conditionFunc, doAction, abortAction);
                }
                else
                {
                    ITempDisplayDialogService TempDisplayDialogService = Module.TempDisplayDialogService();
                    TempDisplayDialogService.TurnOnPossibleFlag();

                    Task dialogServiceTask = Task.Run(async () =>
                    {
                        bool result = false;
                        result = await TempDisplayDialogService.ShowDialog();
                    });

                    Module.InnerStateTransition(new ProbingSuspendState(Module, ReasonforSuspend.TEMPERATURE));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EnumProbingState GetState()
        {
            return EnumProbingState.ZUPPERFORM;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = token is IUnloadWafer
                    || token is IZUPRequest
                    || token is IMoveToDiePosition
                    || token is IMoveToDiePositionAndZUp;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }

        public override EventCodeEnum ProbingZUP()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = ProbingZUPFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.ProbingModuleState.GetType().Name)));

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public override EventCodeEnum Resume()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new ProbingIdleState(Module));
            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }
            return retval;
        }
    }
    public class ProbingZUpDWellState : ProbingStateBase
    {
        DateTime startDate;

        public ProbingZUpDWellState(Probing module) : base(module)
        {
            startDate = DateTime.Now;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                DateTime EndDate = DateTime.Now;
                TimeSpan dateDiff = EndDate - startDate;

                if (Module.ProbingModuleSysParamRef.DWellZAxisTime.Value < dateDiff.TotalMilliseconds)
                {
                    Module.InnerStateTransition(new ProbingZUpState(Module));

                    LoggerManager.Debug($"[ProbingModule] End Probing ZUp DWell. (Maintain DWell Time : {dateDiff.TotalMilliseconds})", isInfo: Module.IsInfo);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EnumProbingState GetState()
        {
            // TODO : Dwell용 Enum 만들어서 넣을 것.

            return EnumProbingState.ZUPDWELL;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = token is IMoveToNextDie ||
                    token is IUnloadWafer ||
                    token is IMoveToDiePositionAndZUp ||
                    token is IZDownRequest;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }


        public override EventCodeEnum Pause()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.ProbingModuleState.GetType().Name)));
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
    public class ProbingZUpState : ProbingStateBase
    {
        DateTime startDate;
        DateTime startDate_Repeat;
        bool TestingTimeoutOccured = false;
        int RepeatCount;

        public ProbingZUpState(Probing module) : base(module)
        {
            startDate = DateTime.Now;
            TestingTimeoutOccured = true;
            RepeatCount = 0;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool isExecuted;
                bool IsLotHasSuspended = false;

                IsLotHasSuspended = Module.IsLotHasSuspendedState();

                if (!IsLotHasSuspended)
                {
                    if (Module.ProbingModuleSysParamRef.TestingTimeoutOnOff.Value == TestingTimeoutEnum.ON && 
                        TestingTimeoutOccured == true && 
                        Module.ProbingModuleSysParamRef.TestingTimeout.Value != 0)
                    {
                        DateTime EndDate = DateTime.Now;
                        TimeSpan dateDiff = EndDate - startDate;
                        double SecondsToMilli = Module.ProbingModuleSysParamRef.TestingTimeout.Value * 1000;

                        if (SecondsToMilli < dateDiff.TotalMilliseconds)
                        {
                            Module.NotifyManager().Notify(EventCodeEnum.PROBING_TESTING_TIMEOUT);
                            this.Module.LoaderController().BroadcastLotState(true);

                            LoggerManager.Debug($"[ProbingModule] Testing Timeout. (Setting Time : {Module.ProbingModuleSysParamRef.TestingTimeout.Value} seconds)", isInfo: Module.IsInfo);

                            TestingTimeoutOccured = false;
                            startDate_Repeat = DateTime.Now;
                        }
                    }
                    else if (Module.ProbingModuleSysParamRef.TestingTimeoutOnOff.Value == TestingTimeoutEnum.ON
                        && Module.ProbingModuleSysParamRef.RepeatedAlarmOnOff.Value == RepeatedAlarmEnum.ON
                        && TestingTimeoutOccured == false
                        && Module.ProbingModuleSysParamRef.RepeatedAlarmTime.Value != 0)
                    {
                        DateTime EndDate_Repeat = DateTime.Now;
                        TimeSpan dateDiff_Repeat = EndDate_Repeat - startDate_Repeat;
                        double SecondsToMilli_Repeat = Module.ProbingModuleSysParamRef.RepeatedAlarmTime.Value * 1000;

                        if (SecondsToMilli_Repeat < dateDiff_Repeat.TotalMilliseconds)
                        {
                            RepeatCount++;
                            Module.NotifyManager().Notify(EventCodeEnum.REPEATED_TESTING_TIMEOUT);
                            this.Module.LoaderController().BroadcastLotState(true);

                            LoggerManager.Debug($"[ProbingModule] Repeated Testing Timeout. (Setting Time: {Module.ProbingModuleSysParamRef.RepeatedAlarmTime.Value} seconds, Repeated Count: {RepeatCount})", isInfo: Module.IsInfo);

                            startDate_Repeat = DateTime.Now;
                        }
                    }

                    if (Module.CommandRecvSlot.IsRequested<IMoveToNextDie>() ||
                        Module.CommandRecvSlot.IsRequested<IZDownRequest>() ||
                        Module.CommandRecvSlot.IsRequested<IMoveToDiePositionAndZUp>())
                    {
                        if (Module.CommandRecvSlot.Token.Parameter as ProbingCommandParam != null) 
                        {
                            if ((Module.CommandRecvSlot.Token.Parameter as ProbingCommandParam).ForcedZdownAndUnload && Module.ProbingSequenceModule().ProbingSequenceRemainCount == 0)
                            {
                                this.Module.SetProbingEndState(ProbingEndReason.NORMAL);
                            }
                        }

                        Module.InnerStateTransition(new ProbingZDOWNPerformState(Module));
                    }

                    // TODO : Unload를 받고, ZDOWN State로의 전환과 로직 흐름을 위해, 일단 ZDownPerformState로 전환한다.
                    if (Module.GEMModule() != null && Module.GEMModule().GemEnable())
                    {
                        if (Module.CommandRecvSlot.IsRequested<IUnloadWafer>())
                        {
                            Module.InnerStateTransition(new ProbingZDOWNPerformState(Module));
                        }
                    }
                    else
                    {
                        Func<bool> conditionFunc = () => true;

                        Action doAction = () =>
                        {
                            EventCodeEnum actionRetVal = EventCodeEnum.UNDEFINED;
                            actionRetVal = UnloadWafer();
                        };

                        Action abortAction = () => { };

                        isExecuted = Module.CommandManager().ProcessIfRequested<IUnloadWafer>(Module, conditionFunc, doAction, abortAction);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }

        public override EnumProbingState GetState()
        {
            return EnumProbingState.ZUP;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = token is IMoveToNextDie ||
                    token is IUnloadWafer ||
                    token is IMoveToDiePositionAndZUp ||
                    token is IZDownRequest;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;

        }


        public override EventCodeEnum Pause()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.ProbingModuleState.GetType().Name)));
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
    public class ProbingZDOWNPerformState : ProbingStateBase
    {
        public ProbingZDOWNPerformState(Probing module) : base(module)
        {
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {
                isValidCommand = token is IMoveToDiePosition
                    || token is IMoveToDiePositionAndZUp;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                //bool isExecuted = false;

                if (Module.CommandRecvSlot.IsRequested<IMoveToNextDie>()
                    || Module.CommandRecvSlot.IsRequested<IMoveToDiePositionAndZUp>()
                    || Module.CommandRecvSlot.IsRequested<IZDownAndPause>()
                    || Module.CommandRecvSlot.IsRequested<IUnloadWafer>()
                    || Module.CommandRecvSlot.IsRequested<IZDownRequest>()
                    )
                {
                    ProbingZDOWN();

                    WaferObject waferObj = Module.StageSupervisor().WaferObject as WaferObject;

                    if (waferObj != null)
                    {
                        waferObj.SequenceProcessedCount.Value++;

                        this.Module.LotOPModule().LotInfo.UpdateWafer(waferObj);

                        this.Module.LotOPModule().SystemInfo.IncreaseDieCount();
                        this.Module.LotOPModule().SystemInfo.IncreaseTouchDownCountUntilBeforeCardChange();

                        this.Module.LotOPModule().LotInfo.ProcessedDieCnt += waferObj.SequenceProcessedCount.Value;
                    }

                    RetVal = UpdateTestHistory();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EnumProbingState GetState()
        {
            return EnumProbingState.ZDNPERFORM;
        }

        public EventCodeEnum UpdateTestHistory()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IProbingModule probingModule = Module.ProbingModule();

                ISubstrateInfo substrateInfo = Module.StageSupervisor().WaferObject.GetSubsInfo();
                IProbeCard probeCard = Module.GetParam_ProbeCard();

                int dutCount = probeCard.ProbeCardDevObjectRef.DutList.Count;

                for (int i = 0; i < dutCount; i++)
                {
                    //현재 위치해 있는 곳에서 프로빙하고 있는 Dut계산.
                    long dutXIndex = probingModule.ProbingLastMIndex.XIndex + probeCard.ProbeCardDevObjectRef.DutList[i].UserIndex.XIndex;
                    long dutYIndex = probingModule.ProbingLastMIndex.YIndex + probeCard.ProbeCardDevObjectRef.DutList[i].UserIndex.YIndex;

                    bool isDutIsInRange = probingModule.DutIsInRange(dutXIndex, dutYIndex);

                    if (isDutIsInRange)
                    {
                        var die = substrateInfo.DIEs[dutXIndex, dutYIndex];

                        if (Module.ProbingModule().IsTestedDIE(dutXIndex, dutYIndex) == true)
                        {
                            die.State.Value = DieStateEnum.TESTED;
                        }

                        TestHistory lastTestHistory = null;

                        if (die.TestHistory != null && die.TestHistory.Count > 0)
                        {
                            lastTestHistory = die.TestHistory.Last();

                            lastTestHistory.StartTime.Value = ZupTime;
                            lastTestHistory.EndTime.Value = ZdownTime;
                            lastTestHistory.DutIndex.Value = i;

                            lastTestHistory.Inkded.Value = false;
                            lastTestHistory.XWaferCenterDistance.Value = 0; // 계산 필요
                            lastTestHistory.YWaferCenterDistance.Value = 0; // 계산 필요
                            lastTestHistory.Overdrive.Value = probingModule.OverDrive;

                            lastTestHistory.FailMarkInspection.Value = false;    // ?
                            lastTestHistory.NeedleMarkInspection.Value = false;  // ?
                            lastTestHistory.NeedleCleaning.Value = false;        // ?
                            lastTestHistory.NeedleAlign.Value = false;           // ?

                            lastTestHistory.DutIndex.Value = i;
                        }
                        //die.TestHistory.Add(lastTestHistory);
                        //die.CurTestHistory = die.TestHistory.Last();

                        //die.CurTestHistory.StartTime.Value = ZupTime;
                        //die.CurTestHistory.EndTime.Value = ZdownTime;
                        //die.CurTestHistory.DutIndex.Value = i;

                        //die.CurTestHistory.Inkded.Value = false;
                        //die.CurTestHistory.XWaferCenterDistance.Value = 0; // 계산 필요
                        //die.CurTestHistory.YWaferCenterDistance.Value = 0; // 계산 필요
                        //die.CurTestHistory.Overdrive.Value = probingModule.OverDrive;

                        //die.CurTestHistory.FailMarkInspection.Value = false;    // ?
                        //die.CurTestHistory.NeedleMarkInspection.Value = false;  // ?
                        //die.CurTestHistory.NeedleCleaning.Value = false;        // ?
                        //die.CurTestHistory.NeedleAlign.Value = false;           // ?

                        //die.CurTestHistory.DutIndex.Value = i;

                        //// BIN 정보는 어떻게 업데이트 되는가?
                        //// SetBinAnalysisData 커맨드의 Execute() 에서 진행.

                        //die.TestHistory.Add(die.CurTestHistory);
                    }
                    else
                    {
                        // TODO : 
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum ProbingZDOWN()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"{this.GetType().Name}.ProbingZDOWN");

                RetVal = ProbingZDNFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.ProbingModuleState.GetType().Name)));

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
    public class ProbingZDOWNDWellState : ProbingStateBase
    {
        DateTime startDate;

        public ProbingZDOWNDWellState(Probing module) : base(module)
        {
            try
            {
                startDate = DateTime.Now;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = token is IUnloadWafer;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;

        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                DateTime EndDate = DateTime.Now;
                TimeSpan dateDiff = EndDate - startDate;

                if (Module.ProbingModuleSysParamRef.DWellZDownZAxisTime.Value < dateDiff.TotalMilliseconds)
                {
                    Module.InnerStateTransition(new ProbingZDOWNState(Module));

                    LoggerManager.Debug($"[ProbingModule] End Probing ZDown DWell. (Maintain DWell Time : {dateDiff.TotalMilliseconds})", isInfo: Module.IsInfo);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EnumProbingState GetState()
        {
            return EnumProbingState.ZDOWNDELL;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.ProbingModuleState.GetType().Name)));
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
    public class ProbingZDOWNState : ProbingStateBase
    {
        public ProbingZDOWNState(Probing module) : base(module)
        {
            // Z Down Gem Event 
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = token is IMoveToNextDie ||
                    token is IMoveToDiePosition ||
                    token is IMoveToDiePositionAndZUp ||
                    token is IUnloadWafer ||
                    token is IZUPRequest ||
                    token is IZDownRequest;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                ITempController TempController = Module.TempController();
                bool IsLotHasSuspended = false;

                if (Module.PMIModule() != null && Module.PMIModule().IsTurnOnPMIInLotRun())
                {
                    Module.InnerStateTransition(new ProbingSuspendState(Module, ReasonforSuspend.PMI));
                }
                else
                {
                    if (Module.IsReservePause)
                    {
                        Module.LotOPModule().ModuleStopFlag = true;

                        Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.STOP_AFTER_PROBING_RESERVE, "Reserved pause", Module.ProbingModuleState.GetType().Name)));
                        Module.IsReservePause = false;
                    }
                    else
                    {
                        if (TempController.IsCurTempWithinSetTempRange())
                        {
                            IsLotHasSuspended = Module.IsLotHasSuspendedState();

                            if (!IsLotHasSuspended)
                            {
                                if (Module.CommandRecvSlot.IsRequested<IZUPRequest>())
                                {
                                    Module.InnerStateTransition(new ProbingZUPPerformState(Module));
                                }

                                if (Module.CommandRecvSlot.IsRequested<IMoveToNextDie>() || 
                                    Module.CommandRecvSlot.IsRequested<IMoveToDiePosition>() || 
                                    Module.CommandRecvSlot.IsRequested<IMoveToDiePositionAndZUp>())
                                {
                                    Module.InnerStateTransition(new ProbingPinPadMatchPerformState(Module));
                                }

                                bool isExecuted = false;
                                Func<bool> conditionFunc = () => true;

                                Action doAction = () =>
                                {
                                    EventCodeEnum actionRetVal = EventCodeEnum.UNDEFINED;
                                    actionRetVal = UnloadWafer();
                                };

                                Action abortAction = () => { };

                                isExecuted = Module.CommandManager().ProcessIfRequested<IUnloadWafer>(Module, conditionFunc, doAction, abortAction);

                                isExecuted = false;
                                Func<bool> conditionFuncForZDown = () => true;
                                Action doActionForZDown = () =>
                                {
                                    ISubstrateInfo waferSubstrateInfo = Module.GetParam_Wafer().GetSubsInfo();

                                    this.Module.GEMModule().GetPIVContainer().SetStageWaferResult(0);

                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    RetVal = Module.EventManager().RaisingEvent(typeof(ProbingZDownProcessEvent).FullName, new ProbeEventArgs(this, semaphore));
                                    semaphore.Wait();

                                };

                                Action abortActionForZDown = () => { };

                                isExecuted = Module.CommandManager().ProcessIfRequested<IZDownRequest>(Module, conditionFuncForZDown, doActionForZDown, abortAction);
                            }

                            bool isZDownAndPauseExecuted = false;
                            Func<bool> ZDownAndPauseConditionFunc = () => true;

                            Action ZDownAndPauseDoAction = () =>
                            {
                                Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.ProbingModuleState.GetType().Name)));

                                LoggerManager.Debug($"PROBING Pause");
                            };

                            Action ZDownAndPauseAbortAction = () => { };

                            isZDownAndPauseExecuted = Module.CommandManager().ProcessIfRequested<IZDownAndPause>(Module, ZDownAndPauseConditionFunc, ZDownAndPauseDoAction, ZDownAndPauseAbortAction);
                        }
                        else
                        {
                            Module.InnerStateTransition(new ProbingSuspendState(Module, ReasonforSuspend.TEMPERATURE));
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }

        public override EnumProbingState GetState()
        {
            return EnumProbingState.ZDN;
        }

        public override EventCodeEnum Pause()
        {
            Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.ProbingModuleState.GetType().Name)));

            return EventCodeEnum.NONE;
        }
    }
    public class ProbingAbortState : ProbingStateBase
    {
        public ProbingAbortState(Probing module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
              
                RetVal = Module.InnerStateTransition(new ProbingIdleState(Module));
                Module.CommandRecvSlot.ClearToken();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }

        public override EnumProbingState GetState()
        {
            return EnumProbingState.ABORT;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }
    public class ProbingDoneState : ProbingStateBase
    {
        public ProbingDoneState(Probing module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.PROBING, StateLogType.DONE, $"Device : {Module.FileManager().GetDeviceName()}, Wafer ID : {Module.GetParam_Wafer().GetSubsInfo().WaferID.Value}, Card ID : {Module.CardChangeModule().GetProbeCardID()}, OD : {Module.OverDrive}", this.Module.LoaderController().GetChuckIndex());

            this.Module.LoaderController().SetProbingStart(false);
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                Module.FinalizeWaferProcessing();

                bool isProbingEnd = false;

                EnumWaferState enumWaferState = Module.StageSupervisor().WaferObject.GetState();
                EnumSubsStatus enumWaferStatus = Module.StageSupervisor().WaferObject.GetStatus();

                if (enumWaferState == EnumWaferState.PROCESSED || enumWaferState == EnumWaferState.SKIPPED || enumWaferStatus == EnumSubsStatus.NOT_EXIST)
                {
                    isProbingEnd = true;
                }

                if (isProbingEnd)
                {
                    Module.LotOPModule().SystemInfo.SaveLotInfo();

                    Module.InnerStateTransition(new ProbingIdleState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }

        public override EnumProbingState GetState()
        {
            return EnumProbingState.DONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = Module.CommandRecvSlot.IsNoCommand() && token is IUnloadWafer;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.ProbingModuleState.GetType().Name)));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
    public class ProbingPauseState : ProbingStateBase
    {
        private EventCodeEnum _ReasonOfPaused;
        public EventCodeEnum ReasonOfPaused
        {
            get { return _ReasonOfPaused; }
            set
            {
                if (value != _ReasonOfPaused)
                {
                    _ReasonOfPaused = value;
                }
            }
        }

        public ProbingPauseState(Probing module, EventCodeInfo eventcode) : base(module)
        {
            ReasonOfPaused = eventcode.EventCode;

            module.GEMModule().GetPIVContainer().SetStageWaferResult();

            if (this.GetModuleState() == ModuleStateEnum.PAUSED)
            {
                Module.ReasonOfError.AddEventCodeInfo(eventcode.EventCode, eventcode.Message, eventcode.Caller);
            }
            else
            {
                LoggerManager.Debug($"[{this.GetType().Name}] Current State = {this.GetModuleState()}, Can not add ReasonOfError.");
            }

            if (eventcode.EventCode == EventCodeEnum.STOP_BEFORE_PROBING)
            {
                Module.NotifyManager().Notify(EventCodeEnum.STOP_BEFORE_PROBING);

                LoggerManager.Debug($"STOP BEFORE PROBING");
                Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "STOP BEFORE PROBING");
            }

            if (eventcode.EventCode == EventCodeEnum.STOP_AFTER_PROBING)
            {
                Module.NotifyManager().Notify(EventCodeEnum.STOP_AFTER_PROBING);

                LoggerManager.Debug($"STOP AFTER PROBING");
                Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "STOP AFTER PROBING");
            }
            else if (eventcode.EventCode == EventCodeEnum.STOP_AFTER_PROBING_RESERVE)
            {
                LoggerManager.Debug($"RESERVED PAUSE");
                Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "RESERVED PAUSE");
            }
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                // STOP_AFTER_PROBING으로 Pause가 된 경우 Probing이 다 되고(ZDOWN 이후) Done이 될 타이밍이 였기 때문에
                // Pause가 된 뒤 웨이퍼를 매뉴얼로 뺏을 때 Processed인 Wafer로 만들어주기 위함. (ISSD-4261)
                if (ReasonOfPaused == EventCodeEnum.STOP_AFTER_PROBING)
                {
                    ReasonOfPaused = EventCodeEnum.UNDEFINED;
                }

                if (Module.CommandRecvSlot.IsRequested<IZDownAndPause>() ||
                    Module.CommandRecvSlot.IsRequested<IUnloadWafer>())
                {
                    if (Module.PreProbingStateEnum == EnumProbingState.ZDN)
                    {
                        RetVal = Module.InnerStateTransition(Module.PreInnerState);
                    }
                    else
                    {
                        RetVal = Module.InnerStateTransition(new ProbingZDOWNPerformState(Module));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override EnumProbingState GetState()
        {
            return EnumProbingState.PAUSED;
        }

        public override EventCodeEnum MoveToNextPinPadMatchedPos()
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum ProbingZDOWN()
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum ProbingZUP()
        {
            throw new NotImplementedException();
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = token is IUnloadWafer ||
                               ((token is IZDownAndPause) && (Module.PreProbingStateEnum != EnumProbingState.ZDN));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.Module.GetParam_Wafer().GetStatus() != EnumSubsStatus.EXIST || Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.SKIPPED)
                {
                    Module.CommandRecvSlot.ClearToken();
                    retVal = Module.InnerStateTransition(new ProbingIdleState(this.Module));
                }
                else
                {
                    if (Module.PreInnerState.GetModuleState() != ModuleStateEnum.IDLE &&  Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.TESTED)
                    {
                        Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.PROBING);
                    }

                    //Status soaking 사용 유무에 따른 Resume처리
                    bool NeedToReProbing = false;
                    if (Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROCESSED &&
                        Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.TESTED) 
                    {
                        if (Module.SoakingModule().GetShowStatusSoakingSettingPageToggleValue())
                        {
                            if (false == Module.SoakingModule().IsStatusSoakingOk()) //Resume 처리가 진행 시 Soaking staus가 probing을 할 수 없는 상태라면 다시 Idle 부터 진행한다.
                            {
                                NeedToReProbing = true;
                                LoggerManager.SoakingLog($"Trun on reprobing flag(soaking state is not maintain), probing preState:{Module.PreInnerState.ToString()}, probingState:{Module.PreProbingStateEnum.ToString()}");
                            }
                        }
                        else
                        {
                            if (Module.SoakingModule().GetLotResumeTriggeredFlag())
                            {
                                NeedToReProbing = true;
                            }
                        }
                    }
                    
                    IInnerState transitionBackupState = Module.PreInnerState;

                    bool isFirstConditionMet = (Module.PreProbingStateEnum == EnumProbingState.ZUPPERFORM
                        || Module.PreProbingStateEnum == EnumProbingState.PINPADMATCHED
                        || Module.PreProbingStateEnum == EnumProbingState.PINPADMATCHPERFORM)
                        && NeedToReProbing;

                    bool isSecondConditionMet = (Module.PreProbingStateEnum == EnumProbingState.ZDN
                        || Module.PreProbingStateEnum == EnumProbingState.MOVE_HEATING_POS
                        || Module.PreProbingStateEnum == EnumProbingState.SUSPENDED)
                        && NeedToReProbing;

                    AlignStateEnum pinalignstate = Module.StageSupervisor().ProbeCardInfo.AlignState.Value;
                    AlignStateEnum waferalignstate = Module.StageSupervisor().WaferObject.AlignState.Value;
                    
                    bool Alignstateisdone = pinalignstate == AlignStateEnum.DONE && waferalignstate == AlignStateEnum.DONE;

                    bool isThirdConditionMet = ((Module.PreProbingStateEnum == EnumProbingState.MOVE_HEATING_POS ||
                                                 Module.PreProbingStateEnum == EnumProbingState.ZDN || 
                                                 Module.PreProbingStateEnum == EnumProbingState.SUSPENDED) &&
                                                 Alignstateisdone == false);
                    if (isThirdConditionMet)
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}] Pre Probing State = {Module.PreProbingStateEnum}, Pin Alignment State = {pinalignstate}, Wafer Alignment State= {waferalignstate}.");
                    }


                    if (isFirstConditionMet || isSecondConditionMet)
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}] Pre Probing State = {Module.PreProbingStateEnum}, First Condition = {isFirstConditionMet}, Second Condition = {isSecondConditionMet}.");
                        retVal = Module.InnerStateTransition(new ProbingSuspendState(Module, ReasonforSuspend.SOAKING));

                        if (isFirstConditionMet)
                        {
                            Module.PreInnerState = new ProbingPinPadMatchPerformState(Module);
                        }

                        if (isSecondConditionMet)
                        {
                            Module.PreInnerState = transitionBackupState;
                        }
                    }
                    else if (isThirdConditionMet) 
                    {
                        //isFirstConditionMet || isSecondConditionMet 가 true 면 isThirdConditionMet 를 확인할 필요가 없음.
                        retVal = Module.InnerStateTransition(new ProbingSuspendState(Module, ReasonforSuspend.ALIGNMENT));
                        Module.PreInnerState = transitionBackupState;
                    }
                    else
                    {
                        retVal = Module.InnerStateTransition(Module.PreInnerState);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new ProbingAbortState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }
    }
    public class ProbingSuspendState : ProbingStateBase
    {
        ReasonforSuspend reasonforSuspend = ReasonforSuspend.NOMAL;
        public ProbingSuspendState(Probing module, ReasonforSuspend RasonofSuspendedSoaking) : base(module)
        {
            reasonforSuspend = RasonofSuspendedSoaking;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                AlignStateEnum pinalignstate = Module.StageSupervisor().ProbeCardInfo.AlignState.Value;
                AlignStateEnum waferalignstate = Module.StageSupervisor().WaferObject.AlignState.Value;
                switch (reasonforSuspend)
                {
                    case ReasonforSuspend.SOAKING:

                        bool LotResumesoaking = false;

                        if (Module.SoakingModule().GetShowStatusSoakingSettingPageToggleValue())
                        {
                            if (false == Module.SoakingModule().IsStatusSoakingOk())
                            {
                                LotResumesoaking = true;
                            }
                        }
                        else
                        {
                            LotResumesoaking = Module.SoakingModule().GetLotResumeTriggeredFlag();
                        }

                        if (waferalignstate == AlignStateEnum.DONE &&
                            (pinalignstate == AlignStateEnum.DONE &&
                            Module.PinAligner().CommandRecvSlot.IsRequested() == false) &&
                            Module.SequenceEngineManager().GetRunState() &&
                            Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROCESSED &&
                            LotResumesoaking == false)
                        {
                            RetVal = Module.InnerStateTransition(Module.PreInnerState);

                            reasonforSuspend = ReasonforSuspend.NOMAL;
                        }

                        break;
                    case ReasonforSuspend.ALIGNMENT:
                       
                        if (waferalignstate == AlignStateEnum.DONE &&
                            (pinalignstate == AlignStateEnum.DONE &&
                            Module.PinAligner().CommandRecvSlot.IsRequested() == false) &&
                            Module.SequenceEngineManager().GetRunState() &&
                            Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROCESSED)
                        {
                            RetVal = Module.InnerStateTransition(Module.PreInnerState);
                            reasonforSuspend = ReasonforSuspend.NOMAL;
                        }
                        break;
                    case ReasonforSuspend.PMI:

                        if (Module.PMIModule().IsTurnOnPMIInLotRun() == true)
                        {
                            if (Module.PMIModule().ModuleState.GetState() == ModuleStateEnum.DONE)
                            {
                                RetVal = Module.InnerStateTransition(Module.PreInnerState);

                                reasonforSuspend = ReasonforSuspend.NOMAL;
                            }
                        }

                        break;
                    case ReasonforSuspend.TEMPERATURE:

                        ITempController TempController = Module.TempController();

                        if (TempController.IsCurTempWithinSetTempRange())
                        {
                            RetVal = Module.InnerStateTransition(Module.PreInnerState);

                            reasonforSuspend = ReasonforSuspend.NOMAL;
                        }

                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }

        public override EnumProbingState GetState()
        {
            return EnumProbingState.SUSPENDED;
        }

        public override EventCodeEnum MoveToNextPinPadMatchedPos()
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum ProbingZDOWN()
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum ProbingZUP()
        {
            throw new NotImplementedException();
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IInnerState tmpStat = Module.PreInnerState;
                retVal = Module.InnerStateTransition(new ProbingPauseState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.PAUSED_BY_OTHERS, "Paused by others", Module.ProbingModuleState.GetType().Name)));
                Module.PreInnerState = tmpStat;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
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

            }
            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new ProbingAbortState(Module));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }
    }
    public class ProbingErrorState : ProbingStateBase
    {
        public ProbingErrorState(Probing module, EventCodeInfo eventcode) : base(module)
        {
            if (this.GetModuleState() == ModuleStateEnum.ERROR)
            {
                Module.ReasonOfError.AddEventCodeInfo(eventcode.EventCode, eventcode.Message, eventcode.Caller);
            }
            else
            {
                LoggerManager.Debug($"[{this.GetType().Name}] Current State = {this.GetModuleState()}, Can not add ReasonOfError.");
            }

            Module.CommandRecvSlot?.ClearToken();

            LoggerManager.ActionLog(ModuleLogType.PROBING, StateLogType.ERROR, $"Device : {Module.FileManager().GetDeviceName()}, Wafer ID : {Module.GetParam_Wafer().GetSubsInfo().WaferID.Value}, Card ID : {Module.CardChangeModule().GetProbeCardID()}, OD : {Module.OverDrive}", this.Module.LoaderController().GetChuckIndex());
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override EnumProbingState GetState()
        {
            return EnumProbingState.ERROR;
        }

        public override EventCodeEnum MoveToNextPinPadMatchedPos()
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum ProbingZDOWN()
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum ProbingZUP()
        {
            throw new NotImplementedException();
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            LoggerManager.Debug($"[ProbingErrorState] CanExecute(): false");
            return false;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Module.StageSupervisor().StageModuleState.ZCLEARED();

                if (retVal == EventCodeEnum.NONE)
                {
                    this.Module.GetParam_Wafer().SetWaferState(EnumWaferState.UNPROCESSED);

                    retVal = Module.InnerStateTransition(new ProbingIdleState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                this.Module.GetParam_Wafer().SetWaferState(EnumWaferState.UNPROCESSED);

                Module.InnerStateTransition(new ProbingIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                this.Module.GetParam_Wafer().SetWaferState(EnumWaferState.UNPROCESSED);

                Module.InnerStateTransition(new ProbingIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
