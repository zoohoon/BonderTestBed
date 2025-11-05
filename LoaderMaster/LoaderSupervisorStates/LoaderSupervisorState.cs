using LoaderMaster.ExternalStates;
using LoaderMaster.InternalStates;
using LoaderParameters;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LoaderMaster.LoaderSupervisorStates
{

    public abstract class LoaderSupervisorStateBase
    {
        public LoaderSupervisor Module { get; set; }
        public LoaderSupervisorStateBase(LoaderSupervisor module)
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

        public void StateTransition(LoaderSupervisorStateBase stateInst)
        {
            try
            {
                var prevModuleState = Module.StateObj.ModuleState;

                Module.StateObj = stateInst;
                if (stateInst is LoaderSupervisorInternalStateBase)
                {
                    Module.InternalStateBase = (LoaderSupervisorInternalStateBase)stateInst;
                }
                if (stateInst is LoaderSupervisorExternalStateBase)
                {
                    Module.ExternalStateBase = (LoaderSupervisorExternalStateBase)stateInst;
                }

                if ((prevModuleState == ModuleStateEnum.IDLE && stateInst.ModuleState == ModuleStateEnum.RUNNING) ||
                    (prevModuleState == ModuleStateEnum.RUNNING && stateInst.ModuleState == ModuleStateEnum.PAUSED) ||
                    (prevModuleState == ModuleStateEnum.RUNNING && stateInst.ModuleState == ModuleStateEnum.ERROR) ||
                    stateInst.ModuleState == ModuleStateEnum.DONE)
                {
                    LoggerManager.Debug($"[LOT]Log Upload Start, Prev State: {prevModuleState}, Cur State: {stateInst.ModuleState}");
                    UploadLog(Module.LotStartTime, DateTime.Now);
                }
                LoggerManager.Debug($"[LOADERSupervisor] LoaderSupervisorModule.StateTransition() : Prev State: {prevModuleState}, moduleState={stateInst.ModuleState}, loaderState={stateInst.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum UploadLog(DateTime startTime, DateTime endTime)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                Task task = new Task(() =>
                {
                    try
                    {
                        if (Module.LoaderLogManager.LoaderLogParam.UploadEnable.Value == true)
                        {
                            ret = Module.LoaderLogManager.StagesLogUploadServer(startTime, endTime);

                            ret = Module.LoaderLogManager.LoaderLogUploadServer(startTime, endTime);

                            ret = Module.LoaderLogManager.StagesPinTipSizeValidationImageUploadServer(startTime, endTime);
                        }
                        else
                        {
                            LoggerManager.Debug($"ExternalState.UploadLog() UploadEnable is False");
                            ret = EventCodeEnum.NONE;
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                });
                task.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public abstract ModuleStateEnum ModuleState { get; }
        public abstract ModuleStateEnum GetModuleState();
        public abstract void Execute();
        public abstract bool CanExecute(IProbeCommandToken token);
        protected void RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            try
            {
                //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
                LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region => Loader Work Methods
        public virtual ResponseResult SetRequest(LoaderMap dstMap)
        {
            RaiseInvalidState();
            ResponseResult retVal = null;
            try
            {
                retVal = new ResponseResult();
                retVal.IsSucceed = false;
                retVal.ErrorMessage = $"Loader state invalid. state={GetType().Name}";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum AwakeProcessModule()
        {
            RaiseInvalidState();
            return EventCodeEnum.LOADER_STATE_INVALID;
        }

        public virtual EventCodeEnum AbortRequest()
        {
            RaiseInvalidState();
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum Pause()
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
        public virtual EventCodeEnum Resume()
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

        public virtual EventCodeEnum ClearRequestData()
        {
            RaiseInvalidState();
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual void SelfRecovery()
        {
            RaiseInvalidState();
        }
        #endregion

        #region => Motion Methods
        public virtual EventCodeEnum SystemInit()
        {
            RaiseInvalidState();
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum JogRelMove(EnumAxisConstants axis, double value)
        {
            RaiseInvalidState();
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum JogAbsMove(EnumAxisConstants axis, double value)
        {
            RaiseInvalidState();
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        public virtual EventCodeEnum SetEMGSTOP()
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

        public virtual EventCodeEnum UpdateSystem(LoaderSystemParameter systemParam)
        {
            RaiseInvalidState();
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum SaveSystem(LoaderSystemParameter systemParam = null)
        {
            RaiseInvalidState();
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum UpdateDevice(LoaderDeviceParameter deviceParam)
        {
            RaiseInvalidState();
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum SaveDevice(LoaderDeviceParameter deviceParam = null)
        {
            RaiseInvalidState();
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum MoveToModuleForSetup(ModuleTypeEnum module, bool skipuaxis, int slot, int index)
        {
            RaiseInvalidState();
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum RetractAll()
        {
            RaiseInvalidState();
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        protected EventCodeEnum SystemInitFunc()
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

        protected EventCodeEnum UpdateSystemFunc(LoaderSystemParameter systemParam)
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
        protected EventCodeEnum SaveSystemFunc(LoaderSystemParameter systemParam = null)
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
        protected EventCodeEnum UpdateDeviceFunc(LoaderDeviceParameter deviceParam)
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
        protected EventCodeEnum ModuleMoveFunc(ModuleTypeEnum module, bool uaxisskip, int slotnum, int index = 1)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {



            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
    }
}
