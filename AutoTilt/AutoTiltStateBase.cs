using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.State;
using System;

namespace AutoTiltModule
{
    public enum EnumAutoTiltState
    {
        UNDEFINED = -1,
        IDLE,
        RUN,
        PAUSED,
        SUSPENDED,
        DONE,
        ERROR
    }
    public abstract class AutoTiltStateBase : IInnerState
    {
        protected AutoTiltModule AutoTiltMoudle;
        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();
        public abstract EnumAutoTiltState GetState();
        public abstract ModuleStateEnum GetModuleState();
        protected static Random rnd = new Random();
        protected static DateTime EndTime;


        public AutoTiltStateBase(AutoTiltModule autotiltmodule)
        {
            this.AutoTiltMoudle = autotiltmodule;
        }

        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class AutoTiltIDLEState : AutoTiltStateBase
    {
        public AutoTiltIDLEState(AutoTiltModule atmodule) : base(atmodule) { }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (AutoTiltMoudle.PinAligner().ModuleState.GetState() != ModuleStateEnum.DONE ||
                    AutoTiltMoudle.PinAligner().ModuleState.GetState() != ModuleStateEnum.IDLE)
                {
                    AutoTiltMoudle.InnerStateTransition(new AutoTiltRUNState(AutoTiltMoudle));
                    ret = EventCodeEnum.NONE;
                }
                else
                {

                    ret = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override EnumAutoTiltState GetState()
        {
            return EnumAutoTiltState.IDLE;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class AutoTiltRUNState : AutoTiltStateBase
    {
        public AutoTiltRUNState(AutoTiltModule atmodule) : base(atmodule) { }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                if (AutoTiltMoudle.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    AutoTiltMoudle.InnerStateTransition(new AutoTiltDONEState(AutoTiltMoudle));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={AutoTiltMoudle.AutoTiltState.GetState()}");
                    return EventCodeEnum.NONE;
                }

                // Old
                //DateTime StartTime = DateTime.Now;
                //double pindif = 0;
                //if (this.AutoTiltMoudle.Prober.PinAlign.ModuleState.GetState() == ModuleStateEnum.DONE)
                //{
                //    AutoTiltMoudle.AutoTiltStateTransition(new AutoTiltDONEState(AutoTiltMoudle));
                //    return EventCodeEnum.NONE;
                //}
                //// 핀얼라인 내부 상태도 봐야함 OutofToloerance
                //if (this.AutoTiltMoudle.Prober.PinAlign.ModuleState.GetState() == ModuleStateEnum.SUSPENDED)
                //{
                //    double retVal = this.AutoTiltMoudle.CheckPinPlanePlanarity_By_Pin_HPT(out pindif);
                //    if (retVal == -1)
                //    {
                //        AutoTiltMoudle.AutoTiltStateTransition(new AutoTiltERRORState(AutoTiltMoudle));
                //        ret = EventCodeEnum.PIN_CALC_NEEDLE_POS_FAIL;
                //    }
                //    else
                //    {
                //        if (this.AutoTiltMoudle.ATDeviceFile.TiltIntPlaneMinMaxtol < pindif)
                //        {
                //            int retval = this.AutoTiltMoudle.GetPinPlaneCalVal_By_Pin_HPT();
                //            if (retVal != 0)
                //            {
                //                ret = EventCodeEnum.PIN_CALC_NEEDLE_POS_FAIL;
                //                AutoTiltMoudle.AutoTiltStateTransition(new AutoTiltERRORState(AutoTiltMoudle));
                //                return ret;
                //            }
                //            ret = AutoTiltMoudle.StageSuperVisor.StageModuleState.TiltingMove(
                //         AutoTiltMoudle.TZ1Offset, this.AutoTiltMoudle.TZ2Offset, this.AutoTiltMoudle.TZ3Offset);
                //            if (ret == EventCodeEnum.NONE)
                //            {
                //                if (this.AutoTiltMoudle.Prober.PinAlign.ModuleState.GetState() != ModuleStateEnum.DONE)
                //                {
                //                    AutoTiltMoudle.InjectionTarget.obj = AutoTiltMoudle.Prober.PinAlign;
                //                    AutoTiltMoudle.InjectionTarget.HashCode = SecurityUtil.GetHashCode_SHA256((StartTime + this.GetType().Name));

                //                    AutoTiltMoudle.Prober.PinAlign.SetInjector(AutoTiltMoudle, AutoTiltMoudle.InjectionTarget.HashCode);

                //                    AutoTiltMoudle.AutoTiltStateTransition(new AutoTiltSusPendedState(AutoTiltMoudle));
                //                    ret = EventCodeEnum.NONE;
                //                }
                //                //else
                //                //{
                //                //    StartTime = DateTime.Now;
                //                //    delay.DelayFor(3000 + (rnd.Next() % 4000 + 1));
                //                //    AutoTiltMoudle.AutoTiltStateTransition(new AutoTiltDONEState(AutoTiltMoudle));
                //                //    ret = EventCodeEnum.NONE;
                //                //    EndTime = DateTime.Now;
                //                //}
                //            }
                //        }
                //        else
                //        {
                //            // Nothing 
                //        }
                //    }
                //}


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EnumAutoTiltState GetState()
        {
            return EnumAutoTiltState.RUN;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class AutoTiltSusPendedState : AutoTiltStateBase
    {
        public AutoTiltSusPendedState(AutoTiltModule atmodule) : base(atmodule) { }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                // Old
                //if (this.AutoTiltMoudle.Prober.PinAlign.ModuleState.GetState() == ModuleStateEnum.DONE)
                //{
                //    this.AutoTiltMoudle.AutoTiltStateTransition(new AutoTiltDONEState(AutoTiltMoudle));
                //    return EventCodeEnum.NONE;
                //}

                //IModule tmp = (IModule)this.AutoTiltMoudle.InjectionTarget.obj;
                ////IModule tmp = Module.InjectionTarget.obj as IModule;
                //if (tmp != null)
                //{
                //    bool alreadyExists = tmp.Requestors.Any(x => x.HashCode == this.AutoTiltMoudle.InjectionTarget.HashCode);
                //    if (alreadyExists == true)
                //    {
                //        this.AutoTiltMoudle.AutoTiltStateTransition(new AutoTiltRUNState(AutoTiltMoudle));
                //        ret = EventCodeEnum.NONE;
                //    }
                //    else
                //    {
                //        // 서스펜드 상태 유지 
                //        ret = EventCodeEnum.NONE;
                //    }
                //}
                //else
                //{
                //    // 서스 펜드 상태 유지 
                //    ret = EventCodeEnum.NONE;

                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }

        public override EnumAutoTiltState GetState()
        {
            return EnumAutoTiltState.SUSPENDED;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class AutoTiltPausedState : AutoTiltStateBase
    {
        public AutoTiltPausedState(AutoTiltModule atmodule) : base(atmodule) { }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override EnumAutoTiltState GetState()
        {
            return EnumAutoTiltState.PAUSED;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class AutoTiltDONEState : AutoTiltStateBase
    {
        public AutoTiltDONEState(AutoTiltModule atmodule) : base(atmodule) { }
        public override EventCodeEnum Execute()
        {
            try
            {
                EventCodeEnum ret = EventCodeEnum.UNDEFINED;
                if (AutoTiltMoudle.PinAligner().ModuleState.GetState() == ModuleStateEnum.IDLE)
                {
                    this.AutoTiltMoudle.InnerStateTransition(new AutoTiltIDLEState(AutoTiltMoudle));
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    //상태 유지 
                    ret = EventCodeEnum.NONE;
                }

                return ret;
                // 카드 체인저한테 이벤트 받는다함 
                //다시 핀얼라인의 상태를 보고 런으로 바꿔줌 
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }

        public override EnumAutoTiltState GetState()
        {
            return EnumAutoTiltState.DONE;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class AutoTiltERRORState : AutoTiltStateBase
    {
        public AutoTiltERRORState(AutoTiltModule atmodule) : base(atmodule) { }
        public override EventCodeEnum Execute()
        {
            throw new NotImplementedException();
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override EnumAutoTiltState GetState()
        {
            return EnumAutoTiltState.ERROR;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
}
