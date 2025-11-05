using System;

namespace DeviceModule
{
    using LogModule;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Event;
    using ProberInterfaces.State;
    using System.Threading;

    public abstract class DeviceModuleState : IFactoryModule, IInnerState
    {
        public abstract EventCodeEnum Execute();
        public abstract ModuleStateEnum GetModuleState();
        public abstract bool CanExecute(IProbeCommandToken token);

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

    public abstract class DeviceModuleStateBase : DeviceModuleState
    {
        private DeviceModule _Module;

        public DeviceModule Module
        {
            get { return _Module; }
            set { _Module = value; }
        }
        public DeviceModuleStateBase(DeviceModule module)
        {
            Module = module;
        }

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new DeviceModuleIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class DeviceModuleIdleState : DeviceModuleStateBase
    {
        public DeviceModuleIdleState(DeviceModule module) : base(module)
        {
            //if(module.InnerState != null)
            //{
            //    if (module.InnerState.GetModuleState() != this.GetModuleState())
            //    {//다른 State에서 IdleState가 되었을 때 Clear 해줌.
            //        this.CardChangeModule().ReleaseWaitForCardPermission();
            //    }
            //}

        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            return isInjected;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Load 할 Device 가 있고, Load 를 할 수 있는 상태라면 
                //LOT 가 Idle 상태일때는 바로 Load 하러 Running 상태로 변경.
                //LOT 가 Idle 상태가 아닐 때는 Wafer 가 없다면 바로 Load 하러 Running 상태로 변경, Wafer 가 있다면 Unload 를 기다리기위해 Suspend 상태로 변경 됨.

                if (Module.IsNeedDeviceLoad()) //Load 할 Device 가 있고
                {
                    var deviceLoadCheckInfo = Module.IsCanDeviceLoad();//Load 를 할 수 있는 상태라면 

                    if (deviceLoadCheckInfo.SequenceEngineResult && deviceLoadCheckInfo.SoakingResult && deviceLoadCheckInfo.LotIDResult)
                    {
                        if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE && deviceLoadCheckInfo.WaferStatusResult == true)
                        {
                            retVal = Module.InnerStateTransition(new DeviceModuleRunningState(Module));
                        }
                        else
                        {
                            if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.NOT_EXIST)
                            {
                                retVal = Module.InnerStateTransition(new DeviceModuleRunningState(Module));
                            }
                            else
                            {
                                retVal = Module.InnerStateTransition(new DeviceModuleSuspendState(Module));
                            }
                        }
                    }
                    else if (deviceLoadCheckInfo.SequenceEngineResult && deviceLoadCheckInfo.SoakingResult == false && deviceLoadCheckInfo.WaferStatusResult == true && deviceLoadCheckInfo.LotIDResult)
                    {
                        //디바이스 조건은 통과 했지만 Soaking이 Align중 일 때
                        retVal = Module.InnerStateTransition(new DeviceModuleSuspendState(Module));
                    }
                    else
                    {
                        //디바이스 로드 조건도 안맞고 Soaking의 디바이스 로드 조건도 안맞을때(False, False)
                        //디바이스 로드 조건이 안맞고 Soaking의 디바이스 로드 조건은 맞을 때(False, True)
                        //위 상태일 때는 그냥 Idle
                    }

                    deviceLoadCheckInfo = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.InnerStateTransition(new DeviceModulePausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class DeviceModuleSuspendState : DeviceModuleStateBase
    {
        public DeviceModuleSuspendState(DeviceModule module) : base(module)
        {
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            return isInjected;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.NeedLoadDeviceInfo != null)
                {
                    var deviceLoadCheckInfo = Module.IsCanDeviceLoad();

                    if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.NOT_EXIST ||
                        (deviceLoadCheckInfo.SequenceEngineResult && deviceLoadCheckInfo.SoakingResult && deviceLoadCheckInfo.WaferStatusResult))
                    {
                        retVal = Module.InnerStateTransition(new DeviceModuleRunningState(Module));
                    }

                    deviceLoadCheckInfo = null;
                }
                else
                {
                    retVal = Module.InnerStateTransition(new DeviceModuleIdleState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.InnerStateTransition(new DeviceModulePausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.RemoveAllReservationRecipe();
                retVal = Module.InnerStateTransition(new DeviceModuleIdleState(Module));
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class DeviceModuleRunningState : DeviceModuleStateBase
    {
        public DeviceModuleRunningState(DeviceModule module) : base(module)
        {
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            return isInjected;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.NeedLoadDeviceInfo != null)
                {
                    int loadReserveFoupNumber = Module.NeedLoadDeviceInfo.FoupNumber;
                    try
                    {
                        string Loadlotid = Module.NeedLoadDeviceInfo.LotID;

                        this.Module.LotOPModule().UpdateLotName(Loadlotid);

                        LoggerManager.Debug($"[DEVICE MODULE] Running State - Loadrecipefoupnumber : {loadReserveFoupNumber}, WaferStatus : {Module.GetParam_Wafer().GetStatus()}");

                        retVal = this.Module.LoadDevice();

                        if (retVal == EventCodeEnum.NONE)
                        {
                            if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
                            {
                                LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.START,
                               $"Lot ID: {Module.LotOPModule().LotInfo.LotName.Value}, Device:{Module.FileManager().GetDeviceName()}," +
                               $"Card ID:{Module.CardChangeModule().GetProbeCardID()}, OD:{Module.ProbingModule().OverDrive}, " +
                               $"TouchDown Count: {this.LotOPModule().SystemInfo.TouchDownCountUntilBeforeCardChange} "
                               , this.Module.LoaderController().GetChuckIndex());

                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                Module.EventManager().RaisingEvent(typeof(LotSwitchedEvent).FullName, new ProbeEventArgs(this, semaphore, loadReserveFoupNumber));
                                semaphore.Wait();
                            }
                            Module.LotOPModule().LotInfo.SetFoupInfo(Module.NeedLoadDeviceInfo.FoupNumber, Module.NeedLoadDeviceInfo.LotCstHashCode);
                            Module.GEMModule().GetPIVContainer().FoupNumber.Value = Module.NeedLoadDeviceInfo.FoupNumber;

                            Module.SetNeedLoadDeviceInfo(null);

                            if (Module.IsNeedDeviceLoad())
                            {
                                /// Device Load 가 필요한지 확인 후 Load 할 데이터가 있다면 Suupend 상태로 변경한다.
                                /// LOT 관련 Device가 아닌경우에 N개 의 Device 를 Download 받은 경우 여러번 Load 하기 위한 목적
                                retVal = Module.InnerStateTransition(new DeviceModuleSuspendState(Module)); // 다른 모듈에서 채가지 않게 하기위함. 
                            }
                            else
                            {
                                retVal = Module.InnerStateTransition(new DeviceModuleIdleState(Module));
                            }

                            //Module.NeedLoadDeviceInfo = null;
                        }
                        else
                        {
                            retVal = Module.InnerStateTransition(new DeviceModuleErrorState(Module));
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = Module.InnerStateTransition(new DeviceModuleErrorState(Module));
                        LoggerManager.Exception(err);
                    }
                }
                else
                {
                    retVal = Module.InnerStateTransition(new DeviceModuleErrorState(Module));
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

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.InnerStateTransition(new DeviceModulePausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
    }

    public class DeviceModulePausedState : DeviceModuleStateBase
    {
        public DeviceModulePausedState(DeviceModule module) : base(module)
        {
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            return isInjected;
        }

        public override EventCodeEnum Execute()
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

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
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
                retVal = Module.InnerStateTransition(new DeviceModuleAbortState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class DeviceModuleAbortState : DeviceModuleStateBase
    {
        public DeviceModuleAbortState(DeviceModule module) : base(module)
        {
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            return isInjected;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.InnerStateTransition(new DeviceModuleIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }

    public class DeviceModuleErrorState : DeviceModuleStateBase
    {
        public DeviceModuleErrorState(DeviceModule module) : base(module)
        {
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            return isInjected;
        }

        public override EventCodeEnum Execute()
        {
            // lot idle 상황에서 DLRecipe fail 후 error state 계속 남아 있을 경우 복구 방법이 없다.
            // lot idle == DLRecipe fail이더라도 또 명령이 들어오면 재수행 될 수 있도록 하기위한 수정임
            if (string.IsNullOrEmpty(Module.NeedLoadDeviceInfo.LotID) && Module.NeedLoadDeviceInfo.FoupNumber == 0)
            {
                Module.DeviceInfos.Remove(Module.NeedLoadDeviceInfo);
                Module.NeedLoadDeviceInfo = null;
            }

            this.ClearState();           
            return EventCodeEnum.UNDEFINED;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
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
                //retVal = Module.InnerStateTransition(Module.PreInnerState);
                retVal = Module.InnerStateTransition(new DeviceModuleIdleState(Module));
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
                Module.InnerStateTransition(new DeviceModuleIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
}

