using System;

using LoaderBase;
using ProberInterfaces;
using ProberErrorCode;
using System.Runtime.CompilerServices;
using LoaderParameters;
using LogModule;
using System.Windows;

namespace LoaderCore.LoaderStates
{
    public abstract class LoaderStateBase
    {
        public LoaderModule Module { get; set; }

        public LoaderStateBase(LoaderModule module)
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

        public void StateTransition(LoaderStateBase stateInst)
        {
            try
            {
                Module.StateObj = stateInst;
                Module.ModuleState = Module.StateObj.ModuleState;
                LoggerManager.Debug($"[LOADER] LoaderModule.StateTransition() : moduleState={stateInst.ModuleState}, loaderState={stateInst.GetType().Name}");

                string moduleState = ModuleState.ToString();
                LoggerManager.SetLoaderState(moduleState);

                Module.BroadcastLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public abstract ModuleStateEnum ModuleState { get; }

        public abstract void Execute();

        protected void RaiseInvalidState([CallerMemberName] string memberName = "")
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
        public virtual EventCodeEnum SetEMGSTOP()
        {
            StateTransition(new ERROR(Module));
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum AwakeProcessModule()
        {
            RaiseInvalidState();
            return EventCodeEnum.LOADER_STATE_INVALID;
        }
        public virtual EventCodeEnum AbortProcessModule()
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
                Module.PreStateObj = this;
                Module.PauseFlag = false;
                StateTransition(new PAUSED(Module));
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

        #region Recovery Methods
        public virtual EventCodeEnum MotionInit()
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
        public virtual EventCodeEnum ResetWaferLocation()
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

        #region => Setting Param Methods
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
        #endregion

        protected EventCodeEnum SystemInitFunc()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Module.Sequencer.Clear();
                retVal = Module.ModuleManager.InitAttachModules();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.Move.MotionInit();
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
                var backupSystem = Module.SystemParameter;

                Module.SystemParameter = systemParam;
                retVal = Module.ModuleManager.UpdateDefinitionParameters();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.SaveSysParameter();

                Module.ServiceCallback.OnLoaderParameterChanged(Module.SystemParameter, Module.DeviceParameter);
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
                if (systemParam != null)
                {
                    Module.SystemParameter = systemParam;
                }
                retVal = Module.SaveSysParameter();

                Module.ServiceCallback?.OnLoaderParameterChanged(Module.SystemParameter, Module.DeviceParameter);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        //protected EventCodeEnum SaveDeviceFunc(LoaderDeviceParameter deviceParam)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        if (deviceParam != null)
        //        {
        //            Module.DeviceParameter = deviceParam;
        //        }
        //       // retVal = Module.SaveDevParameter();

        //        Module.ServiceCallback.OnLoaderParameterChanged(Module.SystemParameter, Module.DeviceParameter);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}




        protected EventCodeEnum UpdateDeviceFunc(LoaderDeviceParameter deviceParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var backupDevice = Module.DeviceParameter;

                Module.DeviceParameter = deviceParam;

                retVal = Module.ModuleManager.UpdateDeviceParameters();

                if (Module.LoaderService != null)
                {
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        Module.LoaderService.UpdateLoaderSystem(i + 1);
                        Module.LoaderService.UpdateCassetteSystem(Module.GetLoaderCommands().GetDeviceSize(i), i + 1);
                    }
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    // retVal = Module.SaveDevParameter();

                    Module.ServiceCallback.OnLoaderParameterChanged(Module.SystemParameter, Module.DeviceParameter);
                }
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
                var scancammodule = (IScanCameraModule)Module.ModuleManager.FindModule(ModuleTypeEnum.SCANCAM, index);
                var cstmodule = (ICassetteModule)Module.ModuleManager.FindModule(ModuleTypeEnum.CST, index);
                var scansensormodule = (IScanSensorModule)Module.ModuleManager.FindModule(ModuleTypeEnum.SCANSENSOR, index);
                var armModule = (IARMModule)Module.ModuleManager.FindModule(ModuleTypeEnum.ARM, index);
                var paModule = (IPreAlignModule)Module.ModuleManager.FindModule(ModuleTypeEnum.PA, index);

                //var ocrModule = (IOCRReadable)Module.ModuleManager.FindModule(ModuleTypeEnum.COGNEXOCR, index);

                IOCRReadable ocrModule = null;

                if (module == ModuleTypeEnum.COGNEXOCR)
                {
                    ocrModule = (IOCRReadable)Module.ModuleManager.FindModule(ModuleTypeEnum.COGNEXOCR, index);
                }
                else if (module == ModuleTypeEnum.SEMICSOCR)
                {
                    ocrModule = (IOCRReadable)Module.ModuleManager.FindModule(ModuleTypeEnum.SEMICSOCR, index);
                }
                else
                {

                }

                var chuckModule = (IChuckModule)Module.ModuleManager.FindModule(ModuleTypeEnum.CHUCK, index);
                var fixedModule = (IFixedTrayModule)Module.ModuleManager.FindModule(ModuleTypeEnum.FIXEDTRAY, index);
                var inspecModule = (IInspectionTrayModule)Module.ModuleManager.FindModule(ModuleTypeEnum.INSPECTIONTRAY, index);
                var slotModule = (ISlotModule)Module.ModuleManager.FindModule(ModuleTypeEnum.SLOT, index);
                SubstrateTypeEnum subtype = SubstrateTypeEnum.Wafer;
                SubstrateSizeEnum subsize = cstmodule.Device.AllocateDeviceInfo.Size.Value;

                switch (module)
                {
                    case ModuleTypeEnum.UNDEFINED:
                        break;
                    case ModuleTypeEnum.CST:
                        //1st slot 
                        Module.Move.SetupToCstSlot1(scansensormodule, cstmodule, subtype, subsize, uaxisskip, slotnum, index);
                        break;
                    case ModuleTypeEnum.SCANSENSOR://loader => scancam
                        break;
                    case ModuleTypeEnum.SCANCAM: //loader => scancam
                        break;
                    case ModuleTypeEnum.SLOT: //arm => slot pos
                        Module.Move.SetupToCstMove(armModule, slotModule, subtype, subsize, uaxisskip, slotnum, index);

                        break;
                    //case ModuleTypeEnum.ARM:
                    //break;
                    case ModuleTypeEnum.PA:
                        if (paModule.Holder.Status == EnumSubsStatus.NOT_EXIST)
                        {
                            Module.Move.SetupToPAMove(armModule, paModule, LoaderMovingTypeEnum.NORMAL, subtype, subsize, uaxisskip, slotnum, index);
                        }
                        else
                        {

                        }
                        //
                        break;
                    case ModuleTypeEnum.FIXEDTRAY:
                        Module.Move.SetupToFixedTrayMove(armModule, fixedModule, subtype, subsize, uaxisskip, slotnum, index);
                        //
                        break;
                    case ModuleTypeEnum.INSPECTIONTRAY:
                        Module.Move.SetupToInspectionTrayMove(armModule, inspecModule, subtype, subsize, uaxisskip, slotnum, index);
                        //
                        break;
                    case ModuleTypeEnum.CHUCK:
                        Module.Move.SetupToChuckMove(armModule, chuckModule, subtype, subsize, uaxisskip, slotnum, index);
                        //
                        break;
                    case ModuleTypeEnum.SEMICSOCR:
                        Module.Move.SetupToOCRMove(armModule, paModule, ocrModule, subtype, subsize, uaxisskip, slotnum, index);
                        //
                        break;
                    case ModuleTypeEnum.COGNEXOCR:
                        Module.Move.SetupToOCRMove(armModule, paModule, ocrModule, subtype, subsize, uaxisskip, slotnum, index);
                        //
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
    }

    public class INIT : LoaderStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.INIT;

        public INIT(LoaderModule module) : base(module) { }

        public override void Execute() { /*No WORKS*/ }

        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new IDLE(Module));
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
                retVal = Module.Move.JogRelMove(axis, value);
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
                retVal = Module.Move.JogAbsMove(axis, value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion

        #region => Update Param Methods
        public override EventCodeEnum UpdateSystem(LoaderSystemParameter systemParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = UpdateSystemFunc(systemParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum SaveSystem(LoaderSystemParameter systemParam = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SaveSystemFunc(systemParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum UpdateDevice(LoaderDeviceParameter deviceParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = UpdateDeviceFunc(deviceParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        //public override EventCodeEnum SaveDevice(LoaderDeviceParameter deviceParam=null)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        retVal = SaveDeviceFunc(deviceParam);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}
        public override EventCodeEnum RetractAll()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.Move.RetractAll();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion
    }

    public class IDLE : LoaderStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.IDLE;

        public IDLE(LoaderModule module) : base(module)
        {
            if (Module.LoaderMaster != null && Module.LoaderMaster.ModuleState != null && (Module.LoaderMaster.ModuleState.State == ModuleStateEnum.IDLE || Module.LoaderMaster.ModuleState.State == ModuleStateEnum.PAUSED))
            {
                Module.GetLoaderCommands().LoaderLampSetState(ModuleState);
            }
        }

        public override void Execute() { /*No WORKS*/ }

        public override ResponseResult SetRequest(LoaderMap dstMap)
        {
            ResponseResult rr = null;

            try
            {
                rr = Module.Sequencer.SetRequest(dstMap);

                if (rr.IsSucceed)
                {
                    StateTransition(new SCHEDULING(Module));
                }
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
                    StateTransition(new IDLE(Module));
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
                retVal = Module.Move.JogRelMove(axis, value);
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
                retVal = Module.Move.JogAbsMove(axis, value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region => Update Param Methods
        public override EventCodeEnum UpdateSystem(LoaderSystemParameter systemParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = UpdateSystemFunc(systemParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SaveSystem(LoaderSystemParameter systemParam = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SaveSystemFunc(systemParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override EventCodeEnum UpdateDevice(LoaderDeviceParameter deviceParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = UpdateDeviceFunc(deviceParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        //public override EventCodeEnum SaveDevice(LoaderDeviceParameter deviceParam=null)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        retVal = SaveDeviceFunc(deviceParam);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}
        #endregion

        #region Setup

        public override EventCodeEnum MoveToModuleForSetup(ModuleTypeEnum module, bool skipuaxis, int slot, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ModuleMoveFunc(module, skipuaxis, slot, index);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum RetractAll()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.Move.RetractAll();
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
    }

    public class SCHEDULING : LoaderStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.RUNNING;

        public SCHEDULING(LoaderModule module) : base(module) { }

        public override void Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.Sequencer.DoSchedule();
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new ERROR(Module));
                    return;
                }

                if (Module.Sequencer.HasProcessor())
                {
                    StateTransition(new PROCESSING(Module));
                }
                else
                {
                    StateTransition(new DONE(Module));
                }
            }
            catch (Exception err)
            {
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
                    StateTransition(new IDLE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class PAUSED : LoaderStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.PAUSED;
        public PAUSED(LoaderModule module) : base(module) { }

        public override void Execute() { /*No WORKS*/ }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.ResumeFlag = false;
                StateTransition(Module.PreStateObj);
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
                    StateTransition(new IDLE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class PROCESSING : LoaderStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.RUNNING;

        public PROCESSING(LoaderModule module) : base(module)
        {
            if (Module.LoaderMaster != null && (Module.LoaderMaster.ModuleState.State == ModuleStateEnum.IDLE || Module.LoaderMaster.ModuleState.State == ModuleStateEnum.PAUSED))
            {
                Module.GetLoaderCommands().LoaderLampSetState(ModuleState);
            }
        }

        public override void Execute()
        {
            try
            {
                var procState = Module.Sequencer.DoProcess();
                Module.ProcModuleInfo.ProcModuleState = procState;
                if (procState == LoaderProcStateEnum.DONE)
                {
                    Module.LoaderJobViewerDone();
                    Module.ProcModuleInfo.Source = new ModuleID();
                    Module.ProcModuleInfo.Destnation = new ModuleID();
                    StateTransition(new SCHEDULING(Module));
                }
                else if (procState == LoaderProcStateEnum.SUSPENDED)
                {
                    StateTransition(new SUSPENDED(Module));
                    //#Hynix_Merge: 검토 필요 아래 코드는 Hynix 코드, 로그 계속 찍을 수도 있음. 
                    //try
                    //{
                    //    var retVal = this.Module.LoaderMaster.GetClient(Module.ChuckNumber).OnLoaderInfoChanged(Module.GetLoaderInfo());
                    //    LoggerManager.Debug($"[LOADER] OnLoaderInfoChangedClient() for {Module.StateObj}.{Module.StateObj.ModuleState} state, ChuckNumber:{Module.ChuckNumber} retValue:{retVal}");
                    //}catch(Exception err)
                    //{
                    //    Module.BroadcastLoaderInfo();
                    //}

                    //#Hynix_Merge: 검토 필요 아래 코드는 RC_Integrated 코드, 뭐가 맞는지 확인 필요.
                    Module.BroadcastLoaderInfo(false);

                }
                else if (procState == LoaderProcStateEnum.SYSTEM_ERROR)
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        LoggerManager.Debug($"[{procState}] LoaderJobViewList.Clear.");
                        Module.LoaderJobViewList.Clear();
                    });

                    StateTransition(new ERROR(Module));
                }
                else if (procState == LoaderProcStateEnum.ABORT)
                {
                    StateTransition(new ABORT(Module));
                }
                else if (procState == LoaderProcStateEnum.ALARM_ERROR)
                {

                }
                else
                {
                    //No STATE TR-=ANSITION.
                }
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
                Module.Sequencer.Clear();
                StateTransition(new IDLE(Module));
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }

    public class SUSPENDED : LoaderStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.SUSPENDED;

        public SUSPENDED(LoaderModule module) : base(module) { }

        public override void Execute() { /*No WORKS*/ }
        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new IDLE(Module));
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
                Module.Sequencer.AwakeProcessModule();

                StateTransition(new PROCESSING(Module));
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public override EventCodeEnum AbortProcessModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.Sequencer.Clear();

                StateTransition(new ABORT(Module));
                retVal = EventCodeEnum.NONE;
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
                StateTransition(new ABORT(Module));
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
                Module.Sequencer.Clear();
                StateTransition(new IDLE(Module));
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class ABORT : LoaderStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.ABORT;

        public ABORT(LoaderModule module) : base(module) { }

        public override void Execute() { /*No WORKS*/ }

        public override EventCodeEnum ClearRequestData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Module.Sequencer.Clear();
                StateTransition(new IDLE(Module));
                retVal = EventCodeEnum.NONE;
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
                    StateTransition(new IDLE(Module));
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
    }

    public class DONE : LoaderStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.DONE;

        public DONE(LoaderModule module) : base(module)
        {
            if (Module.LoaderMaster != null && (Module.LoaderMaster.ModuleState.State == ModuleStateEnum.IDLE || Module.LoaderMaster.ModuleState.State == ModuleStateEnum.PAUSED))
            {
                Module.GetLoaderCommands().LoaderLampSetState(ModuleState);
            }
        }

        public override void Execute() { /*No WORKS*/ }

        public override EventCodeEnum ClearRequestData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Module.Sequencer.Clear();
                StateTransition(new IDLE(Module));
                retVal = EventCodeEnum.NONE;
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
                    StateTransition(new IDLE(Module));
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
    }

    public class ERROR : LoaderStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.ERROR;

        public ERROR(LoaderModule module) : base(module)
        {

            //Module.GetLoaderCommands().LoaderLampSetState(ModuleState);
        }

        public override void Execute() { /*No WORKS*/ }

        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new IDLE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        #region Recovery Methods
        public override EventCodeEnum MotionInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.Move.MotionInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum ResetWaferLocation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.ModuleManager.ResetWaferLocation();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion

        public override void SelfRecovery()
        {
            try
            {
                Module.Sequencer.SelfRecovery();
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
                Module.Sequencer.Clear();

                if (Module.Move.State != LoaderMoveStateEnum.ERROR)
                {
                    Module.GetLoaderCommands().LoaderLampSetState(ModuleStateEnum.IDLE);
                    StateTransition(new IDLE(Module));
                    //Loader Transfer Error
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_ARM_TO_BUFFER_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_ARM_TO_STAGE_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_ARM_TO_FIXED_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_ARM_TO_INSP_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_ARM_TO_PREALIGN_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_ARM_TO_SLOT_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_BUFFER_TO_ARM_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_CARM_TO_CBUFFER_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_CARM_TO_STAGE_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_CARM_TO_CARDTRAY_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_CBUFFER_TO_CARM_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_STAGE_TO_CARM_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_TRAY_TO_CARM_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_STAGE_TO_ARM_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_FIXED_TO_ARM_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_INSP_TO_ARM_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_PREALIGN_TO_ARM_TRANSFER_ERROR);
                    Module.NotifyManager().ClearNotify(EventCodeEnum.LOADER_SLOT_TO_ARM_TRANSFER_ERROR);

                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}

