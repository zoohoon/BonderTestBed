using System;
using ProberInterfaces;
using ProberInterfaces.Foup;
using ProberErrorCode;
using LogModule;

namespace FoupModules.DockingPlate
{
    public class GPFOUPDockingPlate : FoupDockingPlateBase, ITemplateModule, IFactoryModule
    {
        private GPFOUPDockingPlateState _State;

        public GPFOUPDockingPlateState State
        {
            get { return _State; }
            set { _State = value; }
        }
        #region // ITemplateModule implementation.
        public bool Initialized { get; set; } = false;
        public void DeInitModule()
        {

        }
        public EventCodeEnum InitModule()
        {
            //EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            //ret = StateInit();
            //if (ret != EventCodeEnum.NONE)
            //{
            //    LoggerManager.Debug($"GPFoupCover12Inch.InitModule(): Init. error. Ret = {ret}");
            //}
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }
        public bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion
        public GPFOUPDockingPlate(IFoupModule module) : base(module)
        {
            StateInit();
        }
        public GPFOUPDockingPlate() : base()
        {
        }
        public void StateTransition(GPFOUPDockingPlateState state)
        {
            try
            {
                _State = state;

                this.EnumState = _State.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override EventCodeEnum CheckState()
        {
            EventCodeEnum retVal;

            try
            {
                 var cstState = this.Module.GPLoader.GetDeviceSize(Module.FoupIndex); // plc 에서 DockingPlate lock 한 결과 값으로 사이즈 값을 판별하기 때문에 해당 함수 호출함
                if (cstState == SubstrateSizeEnum.INCH6 ||
                    cstState == SubstrateSizeEnum.INCH8)
                {
                    StateTransition(new GPDockingPlateLock(this));
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    int ret1 = FoupIOManager.MonitorForIO(
                    FoupIOManager.IOMap.Inputs.DI_CST_LOCK12s[Module.FoupIndex], true, 100, 200);
                    int ret2 = FoupIOManager.MonitorForIO(
                    FoupIOManager.IOMap.Inputs.DI_CST_UNLOCK12s[Module.FoupIndex], true, 100, 200);
                    if (ret1 == 1 && ret2 != 1)
                    {
                        StateTransition(new GPDockingPlateLock(this));
                        retVal = EventCodeEnum.NONE;
                    }
                    else if(ret1 != 1 && ret2 == 1)
                    {
                        StateTransition(new GPDockingPlateUnLock(this));
                        retVal = EventCodeEnum.NONE;
                    }else
                    {
                        retVal = EventCodeEnum.FoupDockingPlate12Inch_Failure;
                        throw new Exception($"GPDockingPortStateBase.CheckState(): Return = {retVal}");
                    }

                }

                LoggerManager.Debug($"GPFOUPDockingPlate.CheckState(): Initial state is {_State.GetState()}");

            }
            catch (Exception err)
            {
                StateTransition(new GPDockingPlateError(this));
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. FoupDockingPlateNormal12Inch - CheckState() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
                return retVal;
            }

            return retVal;
        }
        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal=EventCodeEnum.UNDEFINED;

            try
            {
                retVal=CheckState();

                Inputs.Add(FoupIOManager.IOMap.Inputs.DI_CST12_PRESs[Module.FoupIndex]);
                Inputs.Add(FoupIOManager.IOMap.Inputs.DI_CST12_PRES2s[Module.FoupIndex]);
                Inputs.Add(FoupIOManager.IOMap.Inputs.DI_CST_LOCK12s[Module.FoupIndex]);
                Inputs.Add(FoupIOManager.IOMap.Inputs.DI_CST_UNLOCK12s[Module.FoupIndex]);

                Outputs.Add(FoupIOManager.IOMap.Outputs.DO_CST_12INCH_LOCKs[Module.FoupIndex]);
                Outputs.Add(FoupIOManager.IOMap.Outputs.DO_CST_12INCH_UNLOCKs[Module.FoupIndex]);

            }
            catch (Exception err)
            {
                StateTransition(new GPDockingPlateError(this));
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. FoupDockingPlateNormal12Inch - StateInit() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
                return retVal;
            }

            return retVal;
        }

        public override DockingPlateStateEnum GetState()
        {
            return State.GetState();
        }

        public override EventCodeEnum Lock()
        {
            return State.Lock();
        }

        public override EventCodeEnum Unlock()
        {
            return State.UnLock();
        }
        public override EventCodeEnum RecoveryUnlock()
        {
            return State.RecoveryUnlock();
        }

    }
    public abstract class GPFOUPDockingPlateState
    {

        public GPFOUPDockingPlateState(GPFOUPDockingPlate _module)
        {
            PlateModule = _module;
        }

        private GPFOUPDockingPlate _PlateModule;

        public GPFOUPDockingPlate PlateModule
        {
            get { return _PlateModule; }
            set { _PlateModule = value; }
        }

        public abstract DockingPlateStateEnum GetState();

        public abstract EventCodeEnum Lock();
        public abstract EventCodeEnum UnLock();
        public virtual EventCodeEnum RecoveryUnlock() { return EventCodeEnum.NONE; }
        protected EventCodeEnum LockMethod()
        {
            EventCodeEnum retVal;

            try
            {
                var cstState = PlateModule.Module.GPLoader.ReadCassetteCtrlState(PlateModule.Module.FoupIndex);
                if (cstState == EnumCSTCtrl.CSTLOCKED)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = PlateModule.Module.GPCommand.LockCassette(PlateModule.Module.FoupIndex);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        PlateModule.State = new GPDockingPlateLock(PlateModule);
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        throw new Exception($"Extend()");
                    }
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                PlateModule.NotifyManager().Notify(EventCodeEnum.CASSETTE_LOCK_ERROR, PlateModule.Module.FoupNumber);
                PlateModule.State = new GPDockingPlateError(PlateModule);
                LoggerManager.Debug($"{err.Message.ToString()}. DockingPlate12InchLock - Lock() : Error occured.");
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
            }
            return retVal;

        }
        protected EventCodeEnum UnLockMethod()
        {
            EventCodeEnum retVal;

            try
            {
                retVal = PlateModule.Module.GPCommand.UnLockCassette(PlateModule.Module.FoupIndex);
                if (retVal == EventCodeEnum.NONE)
                {
                    PlateModule.State = new GPDockingPlateUnLock(PlateModule);
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    throw new Exception($"Retract()");
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                PlateModule.NotifyManager().Notify(EventCodeEnum.CASSETTE_UNLOCK_ERROR,PlateModule.Module.FoupNumber);
                PlateModule.State = new GPDockingPlateError(PlateModule);
                LoggerManager.Debug($"{err.Message.ToString()}. DockingPlate8InchUnLock - UnLock() : Error occured.");
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
            }
            return retVal;
        }
    }

    public class GPDockingPlateLock : GPFOUPDockingPlateState
    {
        public GPDockingPlateLock(GPFOUPDockingPlate module) : base(module)
        {
            module.EnumState = DockingPlateStateEnum.LOCK;
        }
        public override DockingPlateStateEnum GetState()
        {
            return DockingPlateStateEnum.LOCK;
        }

        public override EventCodeEnum Lock()
        {
            return LockMethod();
        }

        public override EventCodeEnum UnLock()
        {
            return UnLockMethod();
        }
    }
    public class GPDockingPlateUnLock : GPFOUPDockingPlateState
    {
        public GPDockingPlateUnLock(GPFOUPDockingPlate module) : base(module)
        {
            module.EnumState = DockingPlateStateEnum.UNLOCK;
        }
        public override DockingPlateStateEnum GetState()
        {
            return DockingPlateStateEnum.UNLOCK;
        }
        public override EventCodeEnum Lock()
        {
            return LockMethod();
        }
        public override EventCodeEnum UnLock()
        {
            return UnLockMethod();
        }
    }
    public class GPDockingPlateError : GPFOUPDockingPlateState
    {
        public GPDockingPlateError(GPFOUPDockingPlate module) : base(module)
        {
          
            module.EnumState = DockingPlateStateEnum.ERROR;
        }
        public override DockingPlateStateEnum GetState()
        {
            return DockingPlateStateEnum.ERROR;
        }
        public override EventCodeEnum Lock()
        {
            return LockMethod();
        }
        public override EventCodeEnum UnLock()
        {
            return UnLockMethod();
        }
    }
}
